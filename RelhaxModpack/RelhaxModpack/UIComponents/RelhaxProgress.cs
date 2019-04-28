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
        /// The current completed and total total level tasks. (Tasks are arbitrary and defined by the task itself)
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

    public class RelhaxInstallerProgress : RelhaxProgress
    {
        /// <summary>
        /// During zip file extraction, the number of processed bytes extracted, and the total bytes to extract.
        /// During copy operations, the number of processed copied bytes, and the total bytes to copy.
        /// </summary>
        public long BytesProcessed, BytesTotal = 0;
        /// <summary>
        /// The name of the file currently being processed
        /// </summary>
        public string Filename = string.Empty;
        /// <summary>
        /// During zip file extraction, the entry inside the zip file being processed
        /// </summary>
        public string EntryFilename = string.Empty;
        /// <summary>
        /// The current status of the installer. Represents the current state of the installer. When it exists, it is also used as an exit code.
        /// </summary>
        public InstallerExitCodes InstallStatus = InstallerExitCodes.DownloadModsError;

        public UninstallerExitCodes UninstallStatus = UninstallerExitCodes.GettingFilelistError;
    }
}
