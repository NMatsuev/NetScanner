using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace NetScanner
{
    public partial class FrmMain : Form
    {
        public NetworkInterface NetInterface { get { return GetNetInterface(); } }
        public int ProgressBarValue { get { return GetProgressBarValue(); } set { SetProgressBarValue(value); } }
        public bool IsBtnEnabled { get { return GetBtnEnabled(); }set { SetBtnEnabled(value); } }
        private IPAddress _localIP;
        private IPAddress _mask;
        private List<string> _IPs;
        private List<string> _MACs;

        private NetworkInterface GetNetInterface()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics)
            {
                if (nic.Name == ComboBoxInterface.Text)
                    return nic;

            }
            return null;
        }

        private int GetProgressBarValue()
        {
            return ProgressBar.Value;
        }

        private void SetProgressBarValue(int value)
        {
            ProgressBar.Value = value;
        }

        private bool GetBtnEnabled()
        {
            return BtnStart.Enabled;
        }

        private void SetBtnEnabled(bool value)
        {
            BtnStart.Enabled = value;
        }

        private (IPAddress, IPAddress) GetLocalIP()
        {
            var ipProperties = NetInterface.GetIPProperties();
            var ipv4Address = ipProperties.UnicastAddresses
                .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);
            if (ipv4Address != null)
            {
                return (ipv4Address.Address, ipv4Address.IPv4Mask);
            }
            else
            {
                return (null, null);
            }
        }
        public FrmMain()
        {
            InitializeComponent();
            _IPs = new List<string>();
            _MACs = new List<string>();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics)
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    ComboBoxInterface.Items.Add(nic.Name);
                }
            }
            BtnStart.Enabled = false;
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            _IPs.Clear();
            _MACs.Clear();
            (_localIP, _mask) = GetLocalIP();
            ProgressBarValue = 0;
            IsBtnEnabled = false;

            await Scanner.ScanNodesPar(_localIP, _IPs);
            ProgressBarValue += 30;

            ARP.ExecuteArpCommand();
            ProgressBarValue += 10;

            var _arpEntries = ARP.ParseArpOutput("arp_output.txt");
            ProgressBarValue += 10;

            ARP.FillMACs(_localIP, NetInterface, _IPs, _MACs, _arpEntries);

            BtnStart.Enabled = true;
            ProgressBar.Value = ProgressBar.Maximum;
            Update(_IPs, _MACs);
        }

        private void Update(List<string> IPs, List<string> MACs)
        {
            LViewNodes.Items.Clear();
            for (int i = 0; i < IPs.Count; i++)
            {
                ListViewItem item = new ListViewItem(IPs[i]);
                item.SubItems.Add(MACs[i]);
                LViewNodes.Items.Add(item);
            }
        }
        private class Scanner
        {
            public static async Task ScanNodesPar(IPAddress LocalIP, List<string> IPs)
            {
                if (LocalIP == null)
                {
                    MessageBox.Show("Unable to get local IP address.");
                    return;
                }

                string[] parts = LocalIP.ToString().Split('.');
                string baseIP = $"{parts[0]}.{parts[1]}.{parts[2]}.";//Ã¿— ¿ œŒƒ—≈“» ƒŒÀ∆Õ¿ ¡€“‹

                Task[] tasks = new Task[254];

                for (int i = 1; i < 255; i++)
                {
                    string ip = baseIP + i;
                    tasks[i - 1] = PingDevice(ip, IPs);
                }

                await Task.WhenAll(tasks);
            }

            static async Task PingDevice(string ip, List<string> IPs)
            {
                using (Ping ping = new Ping())
                {
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(ip, 100);
                        if (reply.Status == IPStatus.Success)
                        {
                            IPs.Add(ip);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ping failed for {ip}: {ex.Message}");
                    }
                }
            }
        }

        private class ARP
        {
            public static void ExecuteArpCommand()
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = "-a",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    File.WriteAllText("arp_output.txt", output);
                }
            }

            public static Dictionary<string, string> ParseArpOutput(string filePath)
            {
                Dictionary<string, string> arpEntries = new Dictionary<string, string>();

                string[] lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var match = Regex.Match(line, @"(?<ip>\d{1,3}(\.\d{1,3}){3})\s+(?<mac>([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2}))");

                    if (match.Success)
                    {
                        string ipAddress = match.Groups["ip"].Value;
                        string macAddress = match.Groups["mac"].Value;

                        if (!arpEntries.ContainsKey(ipAddress))
                        {
                            arpEntries[ipAddress] = macAddress;
                        }
                    }
                }

                return arpEntries;
            }

            public static void FillMACs(IPAddress LocalIP, NetworkInterface NetInterface, List <string> IPs, List <string> MACs, Dictionary<string, string> ARP)
            {
                for (int i = 0; i < IPs.Count; i++)
                {
                    if (ARP.ContainsKey(IPs[i]))
                        MACs.Add(ARP[IPs[i]]);
                    else
                        if (IPs[i] == LocalIP.ToString())
                    {
                        string macAddress = NetInterface.GetPhysicalAddress().ToString();
                        if (macAddress.Length == 12)
                        {
                            string formattedMacAddress = $"{macAddress.Substring(0, 2)}-{macAddress.Substring(2, 2)}-{macAddress.Substring(4, 2)}-{macAddress.Substring(6, 2)}-{macAddress.Substring(8, 2)}-{macAddress.Substring(10, 2)}";
                            MACs.Add(formattedMacAddress.ToLower());
                        }
                        else
                            MACs.Add("ÕÂÚ Ò‚Â‰ÂÌËÈ");
                    }
                    else
                        MACs.Add("ÕÂÚ Ò‚Â‰ÂÌËÈ");
                }
            }
        }

        private void ComboBoxInterface_SelectedIndexChanged(object sender, EventArgs e)
        {
            IsBtnEnabled = ComboBoxInterface.SelectedIndex != -1;
        }
    }
}