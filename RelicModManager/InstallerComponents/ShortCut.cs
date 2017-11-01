using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class ShortCut
    {
        public string path { get; set; }
        public string name { get; set; }
        public bool enabled { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", enabled == true ? "X": "O", name, path);
        }
    }

    
}
