namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// Possible points at which the uninstaller can fail
    /// </summary>
    public enum UninstallerExitCodes
    {
        /// <summary>
        /// Error with getting the file lists (from folder scan and/or from log file)
        /// </summary>
        GettingFilelistError,

        /// <summary>
        /// Error with deleting of files
        /// </summary>
        UninstallError,

        /// <summary>
        /// Error with deleting of empty folders
        /// </summary>
        ProcessingEmptyFolders,

        /// <summary>
        /// Error with cleanup of temporary and leftover files
        /// </summary>
        PerformFinalClearup,

        /// <summary>
        /// No error occurred
        /// </summary>
        Success
    }
}
