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

        public string PackageName = "";

        //public bool Enabled { get; set; } = false;

        public bool willBeInstalled { get; set; } = false;

        public bool NotFlag = false;

        public Logic Logic = Logic.OR;

        public override string ToString()
        {
            return PackageName;
        }
    }
}
