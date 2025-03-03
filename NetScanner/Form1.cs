using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Numerics;
using System.Collections.Concurrent;
using System.Text;

namespace NetScanner
{
    public partial class FrmMain : Form
    {
        public NetworkInterface NetInterface { get { return GetNetInterface(); } }
        public int ProgressBarValue { get { return GetProgressBarValue(); } set { SetProgressBarValue(value); } }
        public bool IsBtnEnabled { get { return GetBtnEnabled(); } set { SetBtnEnabled(value); } }

        private IPAddress _localIP;
        private IPAddress _mask;
        private List<IPAddress> _IPs;
        private List<string> _MACs;
        private List<string> _Names;
        private List<string> _Ports;
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

        private void Update(List<IPAddress> IPs, List<string> MACs, List<string> Names, List<string> Ports)
        {
            LViewNodes.Items.Clear();
            for (int i = 0; i < IPs.Count; i++)
            {
                ListViewItem item = new ListViewItem(IPs[i].ToString());
                if (MACs.Count > i)
                    item.SubItems.Add(MACs[i]);
                if (Names.Count > i)
                    item.SubItems.Add(Names[i]);
                if (Ports.Count > i)
                    item.SubItems.Add(Ports[i]);
                LViewNodes.Items.Add(item);
            }
        }

        public FrmMain()
        {
            InitializeComponent();
            _IPs = new List<IPAddress>();
            _MACs = new List<string>();
            _Names = new List<string>();
            _Ports = new List<string>();
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
            _Ports.Clear();

            (_localIP, _mask) = GetLocalIP();
            ProgressBarValue = 0;
            IsBtnEnabled = false;

            await Scanner.ScanNodesPar(_localIP, _mask, _IPs);
            ProgressBarValue += TIME_FOR_IP;
            Update(_IPs, _MACs, _Names, _Ports);

            ARP.ExecuteArpCommand();
            var _arpEntries = ARP.ParseArpOutput("arp_output.txt");
            ARP.FillMACs(_localIP, NetInterface, _IPs, _MACs, _arpEntries);
            ProgressBarValue += TIME_FOR_MAC;
            Update(_IPs, _MACs, _Names, _Ports);

            await FillNames(_IPs, _Names);
            ProgressBarValue = TIME - TIME_FOR_PORTS;
            Update(_IPs, _MACs, _Names, _Ports);

            await PortsScanner.Scan(_IPs, _Ports);
            ProgressBarValue = TIME;
            Update(_IPs, _MACs, _Names, _Ports);

            BtnStart.Enabled = true;
        }

        private void ComboBoxInterface_SelectedIndexChanged(object sender, EventArgs e)
        {
            IsBtnEnabled = ComboBoxInterface.SelectedIndex != -1;
        }

        private class Scanner
        {
            public static async Task ScanNodesPar(IPAddress LocalIP, IPAddress Mask, List<IPAddress> IPs)
            {
                if (LocalIP == null)
                {
                    MessageBox.Show("Unable to get local IP address.");
                    return;
                }

                (IPAddress startIP, IPAddress endIP) = GetIPRange(LocalIP, Mask);

                List<Task> tasks = new List<Task>();

                for (IPAddress ip = startIP; CompareIP(ip, endIP) <= 0; ip = NextIP(ip))
                {
                    tasks.Add(PingDevice(ip.ToString(), IPs));
                }

                await Task.WhenAll(tasks);
            }

            private static (IPAddress, IPAddress) GetIPRange(IPAddress ip, IPAddress mask)
            {
                byte[] ipBytes = ip.GetAddressBytes();
                byte[] maskBytes = mask.GetAddressBytes();
                byte[] networkBytes = new byte[4];
                byte[] broadcastBytes = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                    broadcastBytes[i] = (byte)(networkBytes[i] | ~maskBytes[i]);
                }

                IPAddress network = new IPAddress(networkBytes);
                IPAddress broadcast = new IPAddress(broadcastBytes);

                return (NextIP(network), PreviousIP(broadcast));
            }

            private static IPAddress NextIP(IPAddress ip)
            {
                byte[] bytes = ip.GetAddressBytes();
                for (int i = 3; i >= 0; i--)
                {
                    if (++bytes[i] != 0) break;
                }
                return new IPAddress(bytes);
            }

            private static IPAddress PreviousIP(IPAddress ip)
            {
                byte[] bytes = ip.GetAddressBytes();
                for (int i = 3; i >= 0; i--)
                {
                    if (--bytes[i] != 255) break;
                }
                return new IPAddress(bytes);
            }

            private static int CompareIP(IPAddress ip1, IPAddress ip2)
            {
                byte[] array1 = ip1.GetAddressBytes();
                byte[] array2 = ip2.GetAddressBytes();
                Array.Reverse(array1);
                Array.Reverse(array2);
                return new BigInteger(array1).CompareTo(new BigInteger(array2));
            }

            static async Task PingDevice(string ip, List<IPAddress> IPs)
            {
                using (Ping ping = new Ping())
                {
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(ip, 1000);
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

            public static void FillMACs(IPAddress LocalIP, NetworkInterface NetInterface, List<IPAddress> IPs, List<string> MACs, Dictionary<IPAddress, string> ARP)
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

        private async Task FillNames(List<IPAddress> IPs, List<string> Names)
        {
            ConcurrentDictionary<string, string> hostMap = new ConcurrentDictionary<string, string>();
            Task[] tasks = new Task[IPs.Count];
            for (int i = 0; i < IPs.Count; i++)
            {
                tasks[i] = GetHostNameAsync(IPs[i].ToString(), hostMap, IPs.Count);
            }
            await Task.WhenAll(tasks);
            for (int i = 0; i < IPs.Count; i++)
            {
                Names.Add(hostMap[IPs[i].ToString()]);
            }
        }

        private async Task GetHostNameAsync(string ip, ConcurrentDictionary<string, string> hostMap, int IPCount)
        {
            try
            {
                IPHostEntry entry = await Dns.GetHostEntryAsync(ip);
                hostMap[ip] = entry.HostName;
            }
            catch
            {
                hostMap[ip] = "Нет сведений";
            }
            finally
            {
                lock (lockObj)
                {
                    ProgressBarValue += TIME_FOR_NAME / IPCount;
                }
            }

        }

        private class PortsScanner
        {
            public static async Task Scan(List<IPAddress> IPs, List<string> Ports)
            {
                int startPort = 1;
                int endPort = 1024;
                ConcurrentDictionary<string, string> portsMap = new ConcurrentDictionary<string, string>();
                List<Task> tasks = new List<Task>();
                foreach (var ip in IPs)
                {
                    tasks.Add(ScanPortsAsync(ip.ToString(), startPort, endPort, portsMap));
                }

                await Task.WhenAll(tasks);
                for (int i = 0; i < IPs.Count; i++)
                {
                    Ports.Add(portsMap[IPs[i].ToString()]);
                }
            }

            private static async Task ScanPortsAsync(string ipAddress, int startPort, int endPort, ConcurrentDictionary<string, string> portsMap)
            {
                StringBuilder openPorts = new StringBuilder();
                bool isEmpty = true;
                for (int port = startPort; port <= endPort; port++)
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        try
                        {
                            var connectTask = tcpClient.ConnectAsync(ipAddress, port);
                            if (await Task.WhenAny(connectTask, Task.Delay(10)) == connectTask)
                            {
                                isEmpty = false;
                                openPorts.Append($"{port}, ");
                            }
                        }
                        catch { }
                    }
                }
                if (!isEmpty)
                {
                    openPorts.Remove(openPorts.Length - 2, 2);
                }
                portsMap[ipAddress]  =  openPorts.ToString();
            }
        }
    }
}