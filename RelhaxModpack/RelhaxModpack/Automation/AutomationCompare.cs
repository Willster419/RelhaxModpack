using RelhaxModpack.Automation.Tasks;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// An AutomationCompare object is a comparison between two different IO related sources to check if both are equal.
    /// </summary>
    public abstract class AutomationCompare
    {
        /// <summary>
        /// Creates the abstract parts of the AutomationCompare object.
        /// </summary>
        /// <param name="directoryA">The path to the first directory of the IO resource to compare.</param>
        /// <param name="directoryB">The path to the second directory of the IO resource to compare.</param>
        /// <param name="compareMode">The comparison mode of the IO resource.</param>
        public AutomationCompare(string directoryA, string directoryB, AutomationCompareMode compareMode)
        {
            this.CompareAPath = directoryA;
            this.CompareBPath = directoryB;
            this.CompareMode = compareMode;
        }

        /// <summary>
        /// Compare if the two IO resources are equal.
        /// </summary>
        public abstract bool CompareResult { get; }

        /// <summary>
        /// The path to IO resource A.
        /// </summary>
        public string CompareAPath;

        /// <summary>
        /// The path to IO resource B.
        /// </summary>
        public string CompareBPath;

        /// <summary>
        /// The comparison mode of the IO resource.
        /// </summary>
        public AutomationCompareMode CompareMode;

        /// <summary>
        /// The automation task object associated with this compare operation.
        /// </summary>
        public AutomationTask AutomationTask;

        /// <summary>
        /// Print information about this comparison entry to the log file.
        /// </summary>
        public abstract void PrintToLog();
    }
}
