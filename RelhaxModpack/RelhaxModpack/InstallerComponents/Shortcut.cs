using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class Shortcut
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", Enabled == true ? "X": "O", Name, Path);
        }
    }

    
}
