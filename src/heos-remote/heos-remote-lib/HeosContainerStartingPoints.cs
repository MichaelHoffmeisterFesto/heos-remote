using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    /// <summary>
    /// For browsing, starting points (container pathes together with source ids) are required
    /// </summary>
    public class HeosContainerStartingPoint
    {
        public string Name { get; set; } = "";
        public int Sid = 0;
        public string Cid { get; set; } = "";
    }
}
