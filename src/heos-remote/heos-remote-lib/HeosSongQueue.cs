﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    public class HeosSongQueueItem
    {
        public string Song { get; set; } = "";
        public string Album { get; set; } = "";
        public string Artist { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Qid { get; set; } = "";
        public string Mid { get; set; } = "";

        public string GetDisplayInfo()
        {
            var l = new List<string>();
            if (Song?.HasContent() == true)
                l.Add(Song);
            if (Artist?.HasContent() == true)
                l.Add(Artist);
            if (Album?.HasContent() == true)
                l.Add(Album);
            if (l.Count < 1)
                return "--";
            return string.Join("; ", l);
        }
    }

    public class HeosSongQueue : List<HeosSongQueueItem>
    {
        public string CurrentQid = "";

        public async Task<HeosSongQueue?> Acquire(HeosConnectedItem ci, int pid)
        {
            // access
            if (ci?.Telnet == null || !ci.IsValid())
                return null;

            CurrentQid = "";

            // do with pagination
            int recordOfs = 0;

            while (true && this.Count < 9999)
            {
                // get
                var o2 = await ci.Telnet.SendCommandAsync($"heos://player/get_queue?pid={pid}&range={recordOfs},{recordOfs + 99}\r\n");
                if (!HeosTelnet.IsSuccessCode(o2))
                    break;
                recordOfs += 100;

                bool any = false;

                foreach (var x in o2.payload)
                {
                    any = true;
                    var ni = new HeosSongQueueItem()
                    {
                        Song = x.song?.ToString() ?? "",
                        Album = x.album?.ToString() ?? "",
                        Artist = x.artist?.ToString() ?? "",
                        ImageUrl = x.image_url?.ToString() ?? "",
                        Qid = x.qid?.ToString() ?? "",
                        Mid = x.mid?.ToString() ?? ""
                    };
                    this.Add(ni);
                }

                if (!any)
                    break;
            }

            // get also now playing info (shortened)
            var o3 = await ci.Telnet.SendCommandAsync($"heos://player/get_now_playing_media?pid={pid}\r\n");
            if (o3?.heos.result.ToString() == "success")
            {
                CurrentQid = o3.payload?.qid?.ToString() ?? "";
            }

            // allow chaining
            return this;
        }

        public int SearchQidIndex(string qid)
        {
            for (int i = 0; i < this.Count; i++)
                if (qid == this[i].Qid)
                    return i;
            return -1;
        }
    }
}
