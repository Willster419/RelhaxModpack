using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class AutomationCompareFile : AutomationCompare
    {
        public AutomationCompareFile(string directoryA, string directoryB, AutomationCompareMode compareMode, string fileA, string hashA, string fileB, string hashB) : base(directoryA, directoryB, compareMode)
        {
            CompareAFile = fileA;
            CompareBFile = fileB;
            CompareAHash = hashA;
            CompareBHash = hashB;
        }

        public override bool CompareResult { get { return CompareAHash.Equals(CompareBHash); } }

        public string CompareAFile;

        public string CompareBFile;

        public string CompareAHash;

        public string CompareBHash;

        public string CompareAFilepath { get { return Path.Combine(CompareAPath, CompareAFile); } }

        public string CompareBFilepath { get { return Path.Combine(CompareBPath, CompareBFile); } }

        public override void PrintToLog()
        {
            Logging.Debug("A File {0}, Hash {1}", CompareAFilepath, CompareAHash);
            Logging.Debug("B File {0}, Hash {1}", CompareBFilepath, CompareBHash);
        }
    }
}
