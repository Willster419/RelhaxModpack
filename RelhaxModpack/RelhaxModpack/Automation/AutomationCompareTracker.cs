using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class AutomationCompareTracker
    {
        public List<AutomationCompare> AutomationCompares { get; } = new List<AutomationCompare>();

        public int NumMatches { get { return AutomationCompares.Count(compare => compare.CompareResult); } }

        public int NumDifferences { get { return AutomationCompares.Count(compare => !compare.CompareResult); } }

        public List<AutomationCompare> Matches { get { return AutomationCompares.FindAll(compare => compare.CompareResult); } }

        public List<AutomationCompare> Differences { get { return AutomationCompares.FindAll(compare => !compare.CompareResult); } }

        public void Reset()
        {
            AutomationCompares.Clear();
        }

        public void AddCompare(AutomationTask task, AutomationCompare compare)
        {
            compare.AutomationTask = task;
            AutomationCompares.Add(compare);
        }

        public void AddCompare(AutomationTask task, string fileAPath, string fileAHash, string fileBPath, string fileBHash)
        {
            AutomationCompare compare = new AutomationCompare()
            {
                CompareAFilepath = fileAPath,
                CompareAHash = fileAHash,
                CompareBFilepath = fileBPath,
                CompareBHash = fileBHash
            };
            AddCompare(task, compare);
        }
    }
}
