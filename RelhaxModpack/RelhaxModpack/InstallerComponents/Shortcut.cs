using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class Shortcut
    {
        public string Path = string.Empty;
        public string Name = string.Empty;
        public bool Enabled = false;

        public override string ToString()
        {
            return string.Format("Title={0} Target={1}", Name, Path);
        }
    }

    
}
