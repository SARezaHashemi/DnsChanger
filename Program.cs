using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

namespace DnsChanger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "netsh.exe",
                    Arguments = "wlan show interfaces",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var SSIDline = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(l => l.Contains("SSID") && !l.Contains("BSSID"));
            if (SSIDline == null)
            {
                Console.WriteLine("Please connect to a network");
            }
            var ssid = SSIDline.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart();
            Console.WriteLine($"Your SSID Is --------------------- {ssid}");

            var process2 = new Process
            {
                StartInfo =
                {
                    FileName = "netsh.exe",
                    Arguments = $"wlan show profile {ssid} key=clear",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process2.Start();
            var PassOut = process2.StandardOutput.ReadToEnd();
            var KeyContentLine = PassOut.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(l => l.Contains("Key Content"));
            var pass = KeyContentLine.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart();
            Console.WriteLine($"Your Password is ----------------- {pass}");

            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();

            var activeAdapter = networks.First(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                && x.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                                && x.OperationalStatus == OperationalStatus.Up
                                && x.Name.StartsWith("vEthernet") == false);
            Console.WriteLine($"Your Current Adapter is----------- {activeAdapter.Name} ||---|| {activeAdapter.Description}");
            IPAddressCollection Dnss;
            Dnss = GetDnsAdress();
            IPAddress defualt;
            defualt = IPAddress.Parse("192.168.1.1");
            if (Dnss.ToArray()[0] == defualt)
            {
                Console.WriteLine("You Dont Have any Dns enabled");
            }
            else
            {
                Console.WriteLine($"Your First DNS Is ---------------- {Dnss.ToArray()[0]}");
                if(Dnss.Count > 1)
                {
                    Console.WriteLine($"Your Second DNS Is --------------- {Dnss.ToArray()[1]}");
                }
                
            }
            Console.WriteLine($"Your Local IP Address Is --------- {GetLocalIPAddress()}");
            /*string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            var externalIp = IPAddress.Parse(externalIpString);

            Console.WriteLine("Your Public IP Address Is -------- " + externalIp.ToString());*/


            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine("   ██   ██      ██      ███████   ██   ██   ███████   ████         █████    ");
            Console.WriteLine("   ██   ██     ████     ██        ██   ██   ██        ██  ██      ██  ██    ");
            Console.WriteLine("   ███████    ██  ██    ███████   ███████   ███████   ██   ██    ██   ██    ");
            Console.WriteLine("   ██   ██   ████████        ██   ██   ██   ██        ██    ██  ██    ██    ");
            Console.WriteLine("   ██   ██   ██    ██        ██   ██   ██   ██        ██     ████     ██    ");
            Console.WriteLine("   ██   ██   ██    ██   ███████   ██   ██   ███████   ██      ██      ██    ");

            Console.ResetColor();
            Console.WriteLine("START TO CHANGE DNS TO SHEKAN");
            try
            {
                var changednsprocces = new Process() 
                { 
                    StartInfo =
                    {
                        FileName = "netsh.exe",
                        Arguments = $"interface ip set dns name=\"{activeAdapter.Name}\" static 178.22.122.100",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                changednsprocces.Start(); 
                var changednsprocces2 = new Process()
                {
                    StartInfo =
                    {
                        FileName = "netsh.exe",
                        Arguments = $"interface ip set dns name=\"{activeAdapter.Name}\" 185.51.200.2 index=2",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                changednsprocces2.Start();
                Console.WriteLine("Shekan Is enable now press any key to clear dns");
            }
            catch
            {
                Console.WriteLine("Error");
            }
            Console.ReadKey();

            var offnsprocces = new Process()
            {
                StartInfo =
                    {
                        FileName = "netsh.exe",
                        Arguments = $"interface ip set dns name=\"{activeAdapter.Name}\" static addr=none",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
            };
            offnsprocces.Start();
        }
        private static IPAddressCollection GetDnsAdress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;
                    return dnsAddresses;

                }
            }

            throw new InvalidOperationException("Unable to find DNS Address");
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
