using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using heos_remote_lib;

namespace heos_remote_systray
{
    /// <summary>
    /// This class holds a dictionary between urls and (downloaded) images.
    /// </summary>
    public class HeosImageCache : Dictionary<string, Bitmap>
    {
        public async Task<Bitmap?> DownloadAndCache(HttpClient client, string url)
        {
            // access
            if (client == null || url?.HasContent() != true)
                return null;

            // does exists?
            if (this.ContainsKey(url))
            {
                var bm = this[url];
                if (bm != null)
                    return bm;
            }

            // no, try download
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var ba = await response.Content.ReadAsByteArrayAsync();
            var bm2 = WinFormsUtils.ByteToImage(ba);

            // remeber
            this.Add(url, bm2);

            // give back
            return bm2;
        }
    }
}
