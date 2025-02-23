using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml;
using System.Net.NetworkInformation;

namespace heos_remote_lib
{
    public class HeosDiscoveredItem
    {
        public string Location = "";
        public string Host = "";
        public int Port = 60001;
        public string XmlDescription = "";
        public string FriendlyName = "";
        public string Manufacturer = "";
    }

    public class HeosDiscovery
    {
        public static async Task<List<HeosDiscoveredItem>> DiscoverItems(
            string? firstFriedlyName = null,
            string? firstManufacturer = null,
            int debugLevel = 0,
            int timeOutMs = 3000,
            string? interfaceName = null)
        {
            await Task.Yield();

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            IPAddress? bestAdr = null;

            foreach (NetworkInterface adapter in nics)
            {
                // find a working IP4 address
                IPInterfaceProperties ip_properties = adapter.GetIPProperties();
                if (!adapter.GetIPProperties().MulticastAddresses.Any())
                    continue; // most of VPN adapters will be skipped
                if (!adapter.SupportsMulticast)
                    continue; // multicast is meaningless for this type of connection
                if (OperationalStatus.Up != adapter.OperationalStatus)
                    continue; // this adapter is off or not connected
                IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
                if (null == p)
                    continue; // IPv4 is not configured on this adapter

                // get the address itself
                IPAddress? foundAdr = null;
                foreach (var a in ip_properties.UnicastAddresses)
                {
                    // find IP4
                    if (a.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    foundAdr = a.Address.MapToIPv4();
                }
                if (foundAdr == null)
                    continue;

                // force this?
                if (interfaceName?.HasContent() == true
                    && adapter.Name.ToLower().Contains(interfaceName.Trim().ToLower()))
                    bestAdr = foundAdr;

                // set any?
                if (bestAdr == null)
                    bestAdr = foundAdr;
            }

            // no chance?
            var res = new List<HeosDiscoveredItem>();
            if (bestAdr == null)
                return res;

            // ok, use this address

            IPEndPoint LocalEndPoint = new IPEndPoint(bestAdr, 1901);      // modify with your local ip address
            //IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Parse("192.168.178.126"), 1901);      // modify with your local ip address
            IPEndPoint MulticastEndPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

            using (Socket UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                UdpSocket.SendTimeout = 3000;
                UdpSocket.ReceiveTimeout = 3000;

                UdpSocket.Bind(LocalEndPoint);
                // UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.Parse("239.255.255.250").GetAddressBytes());
                // UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)IPAddress.HostToNetworkOrder(p.Index));

                // string SearchString = "M-SEARCH * HTTP/1.1\r\nHOST:239.255.255.250:1900\r\nMAN:\"ssdp:discover\"\r\nST:ssdp:all\r\nMX:3\r\n\r\n";

                // prepare message
                var searchTargetName = "urn:schemas-denon-com:device:ACT-Denon:1";
                var msg = string.Join("\r\n", [
                    "M-SEARCH * HTTP/1.1",
                    "HOST: 239.255.255.250:1900",
                    $"ST: {searchTargetName}",
                    "MX: 5",
                    "MAN: \"ssdp:discover\"",
                    "\r\n"
                ]);

                // debug?
                if (debugLevel >= 2)
                    Console.WriteLine("Sending out SSDP multicast: {0}", msg);

                // send & receive
                UdpSocket.SendTo(Encoding.UTF8.GetBytes(msg), SocketFlags.None, MulticastEndPoint);

                // prepare receive

                byte[] ReceiveBuffer = new byte[64000];
                int ReceivedBytes = 0;

                // for time out
                var startTime = DateTime.UtcNow;

                // will download XMLs
                var sharedHttpClient = new HttpClient();

                // collect results
            
                while (true)
                {
                    if (UdpSocket.Available > 0)
                    {
                        ReceivedBytes = UdpSocket.Receive(ReceiveBuffer, SocketFlags.None);
                        if (ReceivedBytes < 1)
                            continue;

                        var ssdpAnswer = Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes);
                        if (ssdpAnswer.Length < 1)
                            continue;

                        // debug?
                        if (debugLevel >= 2)
                            Console.WriteLine("Raw answer: " + ssdpAnswer);

                        // extract location
                        var match = Regex.Match(ssdpAnswer, @"^LOCATION:\s*(\w+.*$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        if (!match.Success)
                            continue;
                        var xmlLocation = match.Groups[1].Value?.Trim();

                        // debug?
                        if (debugLevel >= 1)
                            Console.WriteLine("Found location: {0}", xmlLocation);

                        // async get location?
                        using HttpResponseMessage response = await sharedHttpClient.GetAsync(xmlLocation);
                        if (response == null || !response.IsSuccessStatusCode)
                            continue;

                        // ok, content?
                        var xmlContent = await response.Content.ReadAsStringAsync();
                        if (xmlContent.Length < 1)
                            continue;

                        // debug
                        if (debugLevel >= 1)
                            Console.WriteLine("Got XML content with len={0}", xmlContent.Length);
                        if (debugLevel >= 3)
                            Console.WriteLine("Got XML content: {0}", xmlContent);

                        // try access infos
                        try
                        {
                            // access XML node contest
                            // Note: This is so much damn complicated ENFORICING ALL SHITTY DETAILED NAMESPACE HANDLING stuff,
                            // that I regret EVERYTIME using this SHIT!!
                            XmlDocument doc = new XmlDocument();
                            var nst = new XmlNamespaceManager(doc.NameTable);
                            nst.AddNamespace("x", "urn:schemas-upnp-org:device-1-0");
                            doc.Load(new StringReader(xmlContent));

                            Func<string, string> askForNodeText = (xpath) =>
                            {
                                var nodes = doc.SelectNodes(xpath, nst);
                                if (nodes != null)
                                    foreach (XmlNode child in nodes)
                                    {
                                        if (child.InnerText?.HasContent() == true)
                                            return child.InnerText;
                                    }
                                return "";
                            };

                            var friendlyName = askForNodeText("x:root/x:device/x:friendlyName");
                            var manufacturer = askForNodeText("x:root/x:device/x:manufacturer");

                            var item = new HeosDiscoveredItem()
                            {
                                Location = xmlLocation ?? "",
                                XmlDescription = xmlContent,
                                FriendlyName = friendlyName,
                                Manufacturer = manufacturer
                            };

                            try
                            {
                                var uri = new Uri(xmlLocation ?? "");
                                item.Host = uri.Host;
                                item.Port = uri.Port;
                            } catch (Exception ex)
                            {
                                ;
                            }

                            // do a specific search?
                            if (firstFriedlyName?.HasContent() == true 
                                || firstManufacturer?.HasContent() == true)
                            {
                                var found =    (firstFriedlyName?.HasContent() == true &&
                                                firstFriedlyName.Equals(friendlyName, StringComparison.InvariantCultureIgnoreCase))
                                            || (firstManufacturer?.HasContent() == true &&
                                                firstManufacturer.Equals(manufacturer, StringComparison.InvariantCultureIgnoreCase));
                                if (found)
                                {
                                    res.Add(item);
                                    break;
                                }
                            }
                            else
                            {
                                // just add all
                                res.Add(item);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debugLevel >= 1)
                                Console.WriteLine("When doing SSDP discovery, got exception: {0}", ex.Message);
                        }
                    }

                    if (true && (DateTime.UtcNow - startTime).TotalMilliseconds > 3000)
                        break;
                }

                // UdpSocket.Disconnect(reuseSocket: true);
                UdpSocket.Close();
            }


            // last debug
            if (debugLevel >= 1)
                Console.WriteLine("Search finalized/ timed out. Returning {0} items.", res.Count);

            // ok
            return res;
        }
    }
}
