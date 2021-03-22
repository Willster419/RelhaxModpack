using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// The base class for reporting async progress to any UI receiver task.
    /// There are up to 3 levels of tasks that can be reported: child, parent, and total. At least total will be implemented an any given usage case.
    /// </summary>
    public class RelhaxProgress
    {
        /// <summary>
        /// The current completed and total child level tasks. (Tasks are arbitrary and defined by the task itself)
        /// </summary>
        public int ChildCurrent, ChildTotal = 0;

        /// <summary>
        /// A custom formatted string to use if the async task supports/implements it
        /// </summary>
        public string ChildCurrentProgress = string.Empty;

        /// <summary>
        /// The current completed and total parent level tasks. (Tasks are arbitrary and defined by the task itself)
        /// </summary>
        public int ParrentCurrent, ParrentTotal = 0;

        /// <summary>
        /// A custom formatted string to use if the async task supports/implements it
        /// </summary>
        public string ParrentCurrentProgress = string.Empty;

        /// <summary>
        /// The current completed and total level tasks. (Tasks are arbitrary and defined by the task itself)
        /// </summary>
        public int TotalCurrent, TotalTotal = 0;//#meta

        /// <summary>
        /// A custom formatted string to use if the async task supports/implements it
        /// </summary>
        public string TotalCurrentProgress = string.Empty;

        /// <summary>
        /// A custom formatted string to use if the async task supports/implements it
        /// </summary>
        public string ReportMessage = string.Empty;
    }
}
