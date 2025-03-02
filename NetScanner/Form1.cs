using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Collections;

namespace NetScanner
{
    public partial class FrmMain : Form
    {
        public NetworkInterface NetInterface { get { return GetNetInterface(); } }
        public int ProgressBarValue { get { return GetProgressBarValue(); } set { SetProgressBarValue(value); } }
        public bool IsBtnEnabled { get { return GetBtnEnabled(); }set { SetBtnEnabled(value); } }

        private IPAddress _localIP;
        private IPAddress _mask;
        private List<IPAddress> _IPs;
        private List<string> _MACs;
        private List<string> _Names;
        private static readonly object lockObj = new object();

        private const int TIME_FOR_IP = 5;
        private const int TIME_FOR_MAC = 15;
        private const int TIME_FOR_NAME = 60;
        private const int TIME_FOR_PORTS = 20;
        private const int TIME = 100;

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
            _IPs = new List<IPAddress>();
            _MACs = new List<string>();
            _Names = new List<string>();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics)
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    ComboBoxInterface.Items.Add(nic.Name);
                }
            }
            BtnStart.Enabled = false;
            ProgressBar.Maximum = TIME;
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            _IPs.Clear();
            _MACs.Clear();
            _Names.Clear();

            (_localIP, _mask) = GetLocalIP();
            ProgressBarValue = 0;
            IsBtnEnabled = false;

            await Scanner.ScanNodesPar(_localIP, _IPs);
            ProgressBarValue += TIME_FOR_IP;
            Update(_IPs, _MACs, _Names);

            ARP.ExecuteArpCommand();
            var _arpEntries = ARP.ParseArpOutput("arp_output.txt");
            ARP.FillMACs(_localIP, NetInterface, _IPs, _MACs, _arpEntries);
            ProgressBarValue += TIME_FOR_MAC;
            Update(_IPs, _MACs, _Names);

            await FillNames(_IPs, _Names);
            ProgressBarValue = TIME-TIME_FOR_PORTS;
            Update(_IPs, _MACs, _Names);

            ProgressBarValue = TIME;
            BtnStart.Enabled = true;
        }

        private void ComboBoxInterface_SelectedIndexChanged(object sender, EventArgs e)
        {
            IsBtnEnabled = ComboBoxInterface.SelectedIndex != -1;
        }

        private async Task FillNames(List<IPAddress> IPs, List<string> Names)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            Task[] tasks = new Task[IPs.Count];
            for (int i = 0; i < IPs.Count; i++)
            {
                tasks[i] = GetHostNameAsync(IPs[i].ToString(), map, IPs.Count);
            }
            await Task.WhenAll(tasks);
            for (int i = 0; i < IPs.Count; i++)
            {
                Names.Add(map[IPs[i].ToString()]);
            }
        }

        private async Task GetHostNameAsync(string ip, Dictionary<string, string> map, int IPCount)
        {
            try
            {
                await Task.Run(() =>
                {
                    IPHostEntry entry = Dns.GetHostEntry(ip);
                    lock (lockObj)
                    {
                        map[ip] = entry.HostName;
                    }
                });
                ProgressBarValue += TIME_FOR_NAME/ IPCount;

            }
            catch (Exception ex)
            {
                map[ip] = "Нет сведений";
                ProgressBarValue += TIME_FOR_NAME / IPCount;
            }
        }

        private void Update(List<IPAddress> IPs, List<string> MACs, List<string> Names)
        {
            LViewNodes.Items.Clear();
            for (int i = 0; i < IPs.Count; i++)
            {
                ListViewItem item = new ListViewItem(IPs[i].ToString());
                if (MACs.Count > i) 
                    item.SubItems.Add(MACs[i]);
                if (Names.Count > i)
                    item.SubItems.Add(Names[i]);
                LViewNodes.Items.Add(item);
            }
        }
        private class Scanner
        {
            public static async Task ScanNodesPar(IPAddress LocalIP, List<IPAddress> IPs)
            {
                if (LocalIP == null)
                {
                    MessageBox.Show("Unable to get local IP address.");
                    return;
                }

                string[] parts = LocalIP.ToString().Split('.');
                string baseIP = $"{parts[0]}.{parts[1]}.{parts[2]}.";//МАСКА ПОДСЕТИ ДОЛЖНА БЫТЬ

                Task[] tasks = new Task[254];

                for (int i = 1; i < 255; i++)
                {
                    string ip = baseIP + i;
                    tasks[i - 1] = PingDevice(ip, IPs);
                }

                await Task.WhenAll(tasks);
            }

            static async Task PingDevice(string ip, List<IPAddress> IPs)
            {
                using (Ping ping = new Ping())
                {
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(ip, 100);
                        if (reply.Status == IPStatus.Success)
                        {
                            lock (lockObj)
                            {
                                IPs.Add(IPAddress.Parse(ip));
                            }
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

            public static Dictionary<IPAddress, string> ParseArpOutput(string filePath)
            {
                Dictionary<IPAddress, string> arpEntries = new Dictionary<IPAddress, string>();

                string[] lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var match = Regex.Match(line, @"(?<ip>\d{1,3}(\.\d{1,3}){3})\s+(?<mac>([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2}))");

                    if (match.Success)
                    {
                        string ipAddress = match.Groups["ip"].Value;
                        string macAddress = match.Groups["mac"].Value;

                        if (!arpEntries.ContainsKey(IPAddress.Parse(ipAddress)))
                        {
                            arpEntries[IPAddress.Parse(ipAddress)] = macAddress;
                        }
                    }
                }

                return arpEntries;
            }

            public static void FillMACs(IPAddress LocalIP, NetworkInterface NetInterface, List <IPAddress> IPs, List <string> MACs, Dictionary<IPAddress, string> ARP)
            {
                for (int i = 0; i < IPs.Count; i++)
                {
                    if (ARP.ContainsKey(IPs[i]))
                        MACs.Add(ARP[IPs[i]]);
                    else
                        if (IPs[i].Equals(LocalIP))
                    {
                        string macAddress = NetInterface.GetPhysicalAddress().ToString();
                        if (macAddress.Length == 12)
                        {
                            string formattedMacAddress = $"{macAddress.Substring(0, 2)}-{macAddress.Substring(2, 2)}-{macAddress.Substring(4, 2)}-{macAddress.Substring(6, 2)}-{macAddress.Substring(8, 2)}-{macAddress.Substring(10, 2)}";
                            MACs.Add(formattedMacAddress.ToLower());
                        }
                        else
                            MACs.Add("Нет сведений");
                    }
                    else
                        MACs.Add("Нет сведений");
                }
            }
        }
    }
}