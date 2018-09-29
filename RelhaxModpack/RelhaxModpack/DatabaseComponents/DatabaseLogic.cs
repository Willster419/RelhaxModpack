using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class DatabaseLogic
    {
        public string PackageName { get; set; }
        public bool Enabled { get; set; }
        public bool Checked { get; set; }
        public bool NotFlag { get; set; }
        public override string ToString()
        {
            return PackageName;
        }
    }
}
