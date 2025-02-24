using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    public class HeosContainerItem
    {
        public bool IsContainer { get; set; } = false;
        public bool IsPlayable { get; set; } = false;
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Cid { get; set; } = "";
        public string Mid { get; set; } = "";
        public int Sid { get; set; } = 0;
    }

    public class HeosContainerList : List<HeosContainerItem>
    {
        public async Task<HeosContainerList?> Acquire(HeosConnectedItem ci, int sid, string cid)
        {
            // access
            if (!ci.IsValid())
                return null;

            // get
            var cmd = (cid?.HasContent() == true)
                        ? $"heos://browse/browse?sid={sid}&cid={cid}\r\n"
                        : $"heos://browse/browse?sid={sid}\r\n";
            var o2 = await ci.Telnet.SendCommandAsync(cmd);
            if (o2?.heos?.result?.ToString() != "success")
                return null;

            foreach (var x in o2.payload)
            {
                var ni = new HeosContainerItem()
                {
                    IsContainer = Utils.EvalBool(x.container?.ToString()),
                    IsPlayable = Utils.EvalBool(x.playable?.ToString()),
                    Type = x.type?.ToString() ?? "",
                    Name = x.name?.ToString() ?? "",
                    ImageUrl = x.image_url?.ToString() ?? "",
                    Cid = x.cid?.ToString() ?? "",
                    Mid = x.mid?.ToString() ?? "",
                    Sid = x.sid ?? 0,
                };
                this.Add(ni);
            }

            // allow chaining
            return this;
        }
    }
}
