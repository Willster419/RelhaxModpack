using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class AutomationCompareDirectory : AutomationCompare
    {
        public AutomationCompareDirectory(string directoryA, string directoryB, AutomationCompareMode compareMode, int numFilesA, int numFilesB, int indexOfFilenameChange = -1) : base(directoryA, directoryB, compareMode)
        {
            this.CompareANumFiles = numFilesA;
            this.CompareBNumFiles = numFilesB;
            this.IndexOfFilenameChange = indexOfFilenameChange;
        }

        public override bool CompareResult
        {
            get 
            {
                if (CompareBNumFiles != CompareANumFiles)
                    return false;
                else if (IndexOfFilenameChange != -1)
                    return false;
                else return true;
            } 
        }

        public int CompareANumFiles;

        public int CompareBNumFiles;

        public int IndexOfFilenameChange = 0;

        public override void PrintToLog()
        {
            Logging.Debug("A Directory {0}, Count {1}", CompareAPath, CompareANumFiles);
            Logging.Debug("B Directory {0}, Count {1}", CompareBPath, CompareBNumFiles);
        }
    }
}
