using RelhaxModpack.Automation.Tasks;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public abstract class AutomationCompare
    {
        public AutomationCompare(string directoryA, string directoryB, AutomationCompareMode compareMode)
        {
            this.CompareAPath = directoryA;
            this.CompareBPath = directoryB;
            this.CompareMode = compareMode;
        }

        public abstract bool CompareResult { get; }

        public string CompareAPath;

        public string CompareBPath;

        public AutomationCompareMode CompareMode;

        public AutomationTask AutomationTask;

        public abstract void PrintToLog();
    }
}
