using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public enum Logic
    {
        AND = 1,
        OR = 0
    }
    public class DatabaseLogic
    {
        public string PackageName { get; set; } = "";
        public bool Enabled { get; set; } = false;
        public bool Checked { get; set; } = false;
        public bool NotFlag { get; set; } = false;
        public Logic Logic { get; set; } = Logic.OR;
        public override string ToString()
        {
            return PackageName;
        }
    }
}
