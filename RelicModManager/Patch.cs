using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelicModManager
{
    class Patch
    {
        public string type { get; set; }
        public string mode { get; set; }
        public string file { get; set; }
        public string path { get; set; }
        public string[] lines { get; set; }
        public string search { get; set; }
        public string replace { get; set; }
        public Patch() { }
    }
}
