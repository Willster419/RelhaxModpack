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

    /// <summary>
    /// The RelhaxInstallerProgress class adds additional properties for zip file extraction
    /// </summary>
    public class RelhaxInstallerProgress : RelhaxProgress
    {
        /// <summary>
        /// During zip file extraction, the number of processed bytes extracted, and the total bytes to extract.
        /// During copy operations, the number of processed copied bytes, and the total bytes to copy.
        /// </summary>
        public long BytesProcessed, BytesTotal = 0;

        /// <summary>
        /// Flag to mark if the install engine is waiting on a package to download
        /// </summary>
        public bool WaitingOnDownload = false;

        /// <summary>
        /// Flag to mark if the install engine is waiting on a package to download in a thread
        /// </summary>
        public bool[] WaitingOnDownloadOfAThread;

        /// <summary>
        /// The number of bytes currently processed in an entry in a thread
        /// </summary>
        public long[] BytesProcessedOfAThread;

        /// <summary>
        /// The number of bytes to total process in an entry in a thread
        /// </summary>
        public long[] BytesTotalOfAThread;

        /// <summary>
        /// The name of the file currently being processed
        /// </summary>
        public string Filename = string.Empty;

        /// <summary>
        /// The name of the file currently being processed in a thread
        /// </summary>
        public string[] FilenameOfAThread;

        /// <summary>
        /// During zip file extraction, the entry inside the zip file being processed
        /// </summary>
        public string EntryFilename = string.Empty;

        /// <summary>
        /// The number of entries currently processed in this zip file
        /// </summary>
        public uint EntriesProcessed = 0;

        /// <summary>
        /// The total number of entries in this zip file
        /// </summary>
        public uint EntriesTotal = 0;

        /// <summary>
        /// The number of processed entries of a zip file of a thread
        /// </summary>
        public uint[] EntriesProcessedOfAThread;

        /// <summary>
        /// The number of total entries of a zip file of a thread
        /// </summary>
        public uint[] EntriesTotalOfAThread;

        /// <summary>
        /// The entry name of a zip file of a thread
        /// </summary>
        public string[] EntryFilenameOfAThread;

        /// <summary>
        /// The ID number of the thread that this zip file belongs to
        /// </summary>
        public uint ThreadID = 0;

        /// <summary>
        /// The total number of extraction threads
        /// </summary>
        public uint TotalThreads = 0;

        /// <summary>
        /// The number of completed extraction threads
        /// </summary>
        public uint CompletedThreads = 0;

        /// <summary>
        /// The extraction installation group
        /// </summary>
        public uint InstallGroup = 0;

        /// <summary>
        /// The total number of install groups
        /// </summary>
        public uint TotalInstallGroups = 0;

        /// <summary>
        /// The number of completed extracted packages that thread [index] has
        /// </summary>
        public uint[] CompletedPackagesOfAThread;

        /// <summary>
        /// The number of packages that thread [index] has
        /// </summary>
        public uint[] TotalPackagesofAThread;

        /// <summary>
        /// The current status of the installer. Represents the current state of the installer. When it exists, it is also used as an exit code.
        /// </summary>
        public InstallerExitCodes InstallStatus = InstallerExitCodes.DownloadModsError;

        /// <summary>
        /// The current status of the uninstaller. Also used as exit code.
        /// </summary>
        public UninstallerExitCodes UninstallStatus = UninstallerExitCodes.GettingFilelistError;
    }
}
