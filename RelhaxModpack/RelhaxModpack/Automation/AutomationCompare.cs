using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public struct AutomationCompare
    {
        public bool CompareResult { get { return CompareAHash.Equals(CompareBHash); } }

        public string CompareAFilepath;

        public string CompareBFilepath;

        public string CompareAHash;

        public string CompareBHash;

        public AutomationCompareMode CompareMode;

        public AutomationTask AutomationTask;
    }
}
