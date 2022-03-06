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
    /// The AutomationCompareManager class provides methods to add, remove and compare AutomationCompare objects to determine if the IO resources between each one has changed.
    /// </summary>
    public class AutomationCompareManager
    {
        /// <summary>
        /// Gets the list of AutomationCompare objects.
        /// </summary>
        public List<AutomationCompare> AutomationCompares { get; } = new List<AutomationCompare>();

        /// <summary>
        /// Gets the number of AutomationCompare objects that are match results (the IO resources are equal).
        /// </summary>
        public int NumMatches { get { return AutomationCompares.Count(compare => compare.CompareResult); } }

        /// <summary>
        /// Gets the number of AutomationCompare objects where the IO resources are not equal, and the automation mode is NoMatchContinue.
        /// </summary>
        /// <seealso cref="AutomationCompareMode.NoMatchContinue"/>
        public int NumDifferencesContinue { get { return AutomationCompares.Count(compare => !compare.CompareResult && compare.CompareMode == AutomationCompareMode.NoMatchContinue); } }

        /// <summary>
        /// Gets the number of AutomationCompare objects where the IO resources are not equal, and the automation mode is NoMatchStop.
        /// </summary>
        /// <seealso cref="AutomationCompareMode.NoMatchStop"/>
        public int NumDifferencesStop { get { return AutomationCompares.Count(compare => !compare.CompareResult && compare.CompareMode == AutomationCompareMode.NoMatchStop); } }

        /// <summary>
        /// Gets the list of AutomationCompare objects that are match results (the IO resources are equal).
        /// </summary>
        public List<AutomationCompare> Matches { get { return AutomationCompares.FindAll(compare => compare.CompareResult); } }

        /// <summary>
        /// Gets the list of AutomationCompare objects that are not match results (the IO resources are not equal).
        /// </summary>
        public List<AutomationCompare> Differences { get { return AutomationCompares.FindAll(compare => !compare.CompareResult); } }

        /// <summary>
        /// Clears the current list of AutomationCompare objects.
        /// </summary>
        public void Reset()
        {
            AutomationCompares.Clear();
        }

        /// <summary>
        /// Add an AutomationCompare object to the manager's compare list.
        /// </summary>
        /// <param name="task">The task that the AutomationCompare object being added is associated with.</param>
        /// <param name="compare">The AutomationCompare object to add to the manager.</param>
        public void AddCompare(AutomationTask task, AutomationCompare compare)
        {
            if (compare == null)
                throw new ArgumentNullException(nameof(compare));

            compare.AutomationTask = task;
            AutomationCompares.Add(compare);
        }
    }
}
