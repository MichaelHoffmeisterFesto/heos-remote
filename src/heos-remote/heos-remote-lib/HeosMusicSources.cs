using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    public class HeosMusicSource
    {
        public string Name = "";
        public string ImageUrl = "";
        public string Type = "";
        public int Sid = 0;
        public bool Available = false;
        public string ServiceUserName = "";
    }

    public class HeosMusicSourceList : List<HeosMusicSource>
    {
        public async Task<HeosMusicSourceList?> Acquire(HeosConnectedItem ci, bool onlyValid = false)
        {
            // access
            if (ci?.Telnet == null || !ci.IsValid())
                return null;

            // get
            var o2 = await ci.Telnet.SendCommandAsync($"heos://browse/get_music_sources\r\n");
            if (!HeosTelnet.IsSuccessCode(o2))
                return null;

            foreach (var x in o2.payload)
            {
                var ni = new HeosMusicSource()
                {
                    Name = x.name.ToString(),
                    ImageUrl = x.image_url.ToString(),
                    Type = x.type.ToString(),
                    Sid = x.sid,
                    Available = x.available,
                    ServiceUserName = x.service_username,
                };
                if (!onlyValid || ni.Available)
                    this.Add(ni);
            }

            // allow chaining
            return this;
        }

        public IEnumerable<HeosContainerLocation> GetStartPoints()
        {
            foreach (var ms in this)
            {
                // needs to be available
                if (!ms.Available)
                    continue;
                yield return new HeosContainerLocation()
                {
                    Name = $"Home ({ms.Name})",
                    Sid = ms.Sid,
                    Cid = ""
                };
            }
        }
    }
}
