using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace NetScanner
{
    public partial class FrmMain : Form
    {
        private string _executionTime;
        private string _netInterface;
        private List<string> _IPs;
        private List<string> _MACs;

        public FrmMain()
        {
            InitializeComponent();
            _IPs = new List<string>();
            _MACs = new List<string>();
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            _IPs.Clear();
            _MACs.Clear();
            ProgressBar.Value = 0;

            await Scanner.ScanNodesPar(_IPs);
            ProgressBar.Value += 30;

            ARP.ExecuteArpCommand();
            ProgressBar.Value += 10;

            var _arpEntries = ARP.ParseArpOutput("arp_output.txt");
            ProgressBar.Value += 10;

            for (int i = 0; i < _IPs.Count; i++)
            {
                if (_arpEntries.ContainsKey(_IPs[i]))
                    _MACs.Add(_arpEntries[_IPs[i]]);
                else
                    if (_IPs[i] == Scanner.GetLocalIPAddress())
                {
                    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface nic in nics)
                    {
                        if (nic.OperationalStatus == OperationalStatus.Up)
                        {
                            string macAddress = nic.GetPhysicalAddress().ToString();
                            string formattedMacAddress = $"{macAddress.Substring(0, 2)}-{macAddress.Substring(2, 2)}-{macAddress.Substring(4, 2)}-{macAddress.Substring(6, 2)}-{macAddress.Substring(8, 2)}-{macAddress.Substring(10, 2)}";
                            _MACs.Add(formattedMacAddress.ToLower());
                            break;
                        }
                    }
                }
                else
                    _MACs.Add("Нет сведений");
            }
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
            public static async Task ScanNodesPar(List<string> IPs)
            {
                string localIP = GetLocalIPAddress();
                if (localIP == null)
                {
                    MessageBox.Show("Unable to get local IP address.");
                    return;
                }

                string[] parts = localIP.Split('.');
                string baseIP = $"{parts[0]}.{parts[1]}.{parts[2]}.";

                Task[] tasks = new Task[254];

                for (int i = 1; i < 255; i++)
                {
                    string ip = baseIP + i;
                    tasks[i - 1] = PingDevice(ip, IPs);
                }

                await Task.WhenAll(tasks);
            }

            public static string GetLocalIPAddress()
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                return null;
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
        }
    }
}