using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    public class HeosSearchCriteria
    {
        public string Name = "";
        public int Scid = 0;
        public bool Wildcard = false;
        public bool Playable = false;
        public string Cid = "";
    }

    public class HeosSearchCriteriaList : List<HeosSearchCriteria>
    {
        public async Task<HeosSearchCriteriaList?> Acquire(HeosConnectedItem ci, int sid)
        {
            // access
            if (!ci.IsValid())
                return null;

            // get
            var o2 = await ci.Telnet.SendCommandAsync($"heos://browse/get_search_criteria?sid={sid}\r\n");
            if (o2?.heos.result.ToString() != "success")
                return null;

            foreach (var x in o2.payload)
            {
                var ni = new HeosSearchCriteria()
                {
                    Name = x.name.ToString(),
                    Scid = x.scid,
                    Wildcard = Utils.EvalBool(x.wildcard?.ToString()),
                    Playable = Utils.EvalBool(x.playable?.ToString()),
                    Cid = x.cid?.ToString() ?? ""
                };
                this.Add(ni);
            }

            // allow chaining
            return this;
        }
    }
}
