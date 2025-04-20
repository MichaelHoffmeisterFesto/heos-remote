using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    public class HeosPlayingInfo
    {
        public bool IsPlaying = false;
        public string Url = "";
        public string FirstLine = "";
        public string Nextlines = "";

        public override bool Equals(object? obj)
        {
            if ( obj == null || !(obj is HeosPlayingInfo other))
                return false;

            return IsPlaying == other.IsPlaying 
                && Url == other.Url 
                && FirstLine == other.FirstLine 
                && Nextlines == other.Nextlines;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
