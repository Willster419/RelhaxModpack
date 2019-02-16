using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack.InstallerComponents;

namespace RelhaxModpack.UIComponents
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
        public int ChildCurrent, ChildTotal;
        /// <summary>
        /// A custom formatted string to use if the async task supports/implements it
        /// </summary>
        public string ChildCurrentProgress;
        /// <summary>
        /// The current completed and total parent level tasks. (Tasks are arbitrary and defined by the task itself)
        /// </summary>
        public int ParrentCurrent, ParrentTotal;
        /// <summary>
        /// A custom formatted string to use if the async task supports/implements it
        /// </summary>
        public string ParrentCurrentProgress;
        /// <summary>
        /// The current completed and total total level tasks. (Tasks are arbitrary and defined by the task itself)
        /// </summary>
        public int TotalCurrent, TotalTotal;//#meta
        /// <summary>
        /// A custom formatted string to use if the async task supports/implements it
        /// </summary>
        public string TotalCurrentProgress;
        /// <summary>
        /// A custom formatted string to use if the async task supports/implements it
        /// </summary>
        public string ReportMessage;
    }

    public class RelhaxInstallerProgress : RelhaxProgress
    {
        /// <summary>
        /// During zip file extraction, the number of processed bytes extracted, and the total bytes to extract.
        /// During copy operations, the number of processed copied bytes, and the total bytes to copy.
        /// </summary>
        public long BytesProcessed, BytesTotal;
        /// <summary>
        /// The name of the file currently being processed
        /// </summary>
        public string Filename;
        /// <summary>
        /// During zip file extraction, the entry inside the zip file being processed
        /// </summary>
        public string EntryFilename;
        /// <summary>
        /// The current status of the installer. Represents the current state of the installer. When it exists, it is also used as an exit code.
        /// </summary>
        public InstallerExitCodes InstallStatus;
    }
}
