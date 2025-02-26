using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    public class HeosConnectedItem
    {
        public HeosDiscoveredItem Item;
        public HeosTelnet? Telnet;
        public DateTime LastUpdate = DateTime.UtcNow;

        public HeosConnectedItem(HeosDiscoveredItem item)
        {
            Item = item;
        }

        public bool IsValid() => true;
    }

    /// <summary>
    /// This class manages a number of discovered connections in order to
    /// provide cache-alike access.
    /// Connections might be re-iterated after a while.
    /// </summary>
    public class HeosConnectedItemMgr : Dictionary<string,  HeosConnectedItem>
    {
        public int RenewalSecs = 60;

        public async Task<HeosConnectedItem?> DiscoverOrGet(
            HeosDeviceConfig? deviceConfig,
            int debugLevel = 0,
            int timeOutMs = 3000,
            string? interfaceName = null)
        {
            // access
            if (deviceConfig == null || deviceConfig.FriendlyName?.HasContent() != true)
                return null;

            var fn = deviceConfig.FriendlyName;
            var ep = deviceConfig.EndPoint;

            // check if a connection is already stored and young enough
            if (this.ContainsKey(fn))
            {
                // get the item
                var item = this[fn];
                if ((DateTime.UtcNow - item.LastUpdate).TotalSeconds < RenewalSecs)
                {
                    // probe, if alive
                    item.Telnet = new HeosTelnet(item.Item.Host);
                    var o1 = await item.Telnet.SendCommandAsync("heos://system/heart_beat\r\n");
                    if (o1?.heos.result.ToString() == "success")
                    {
                        item.LastUpdate = DateTime.UtcNow;
                        return item;
                    }
                }

                // no success, try close and remove
                if (item.Telnet != null)
                    item.Telnet.Close();
                this.Remove(fn);
            }

            // no connection, but make according endpoint
            HeosDiscoveredItem? newDi = null;
            if (ep != null)
            {
                if (ep.Port == 0)
                    ep.Port = HeosTelnet.DefaultPort;
                newDi = new()
                {
                    FriendlyName = fn,
                    Host = ep.Address.ToString(),
                    Port = ep.Port,
                    Location = "",
                    XmlDescription = "",
                    Manufacturer = ""
                };
            }
            else
            {
                // make new discovery
                newDi = (await HeosDiscovery.DiscoverItems(
                    firstFriedlyName: fn,
                    debugLevel: debugLevel,
                    timeOutMs: timeOutMs,
                    interfaceName: interfaceName))?.FirstOrDefault();
            }
            if (newDi == null)
                return null;

            // ok make new item
            var newIt = new HeosConnectedItem(newDi);
            newIt.Telnet = new HeosTelnet(newDi.Host, newDi.Port);
            this.Add(fn, newIt);

            // return 
            return newIt;
        }
    }
}
