namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// Possible points at which the installer can fail
    /// </summary>
    public enum InstallerExitCodes
    {
        /// <summary>
        /// No fail
        /// </summary>
        Success = 0,

        /// <summary>
        /// Error with downloading mods
        /// </summary>
        DownloadModsError,

        /// <summary>
        /// Error with backup of mods to the RelhaxBackup folder
        /// </summary>
        BackupModsError,

        /// <summary>
        /// Error with backing up of user cache data to temporary folder
        /// </summary>
        BackupDataError,

        /// <summary>
        /// Error with clearing WoT app data cache
        /// </summary>
        ClearCacheError,

        /// <summary>
        /// Error with clearing game and mod logs
        /// </summary>
        ClearLogsError,

        /// <summary>
        /// Error with cleaning mods and res_mods folders
        /// </summary>
        CleanModsError,

        /// <summary>
        /// Error with mods extraction/installation
        /// </summary>
        ExtractionError,

        /// <summary>
        /// Error with user mods extraction
        /// </summary>
        UserExtractionError,

        /// <summary>
        /// Error with restoring user cache data from temporary folder
        /// </summary>
        RestoreUserdataError,

        /// <summary>
        /// Error with copying/extracting and unpacking binary xml files
        /// </summary>
        XmlUnpackError,

        /// <summary>
        /// Error with patching configuration files
        /// </summary>
        PatchError,

        /// <summary>
        /// Error with creating shortcuts
        /// </summary>
        ShortcutsError,

        /// <summary>
        /// Error with creating the contour icon atlas files
        /// </summary>
        ContourIconAtlasError,

        /// <summary>
        /// Error with installing fonts (starting the fontReg process)
        /// </summary>
        FontInstallError,

        /// <summary>
        /// Error with deleting old download files from the RelhaxDownloads folder
        /// </summary>
        TrimDownloadCacheError,

        /// <summary>
        /// Error with cleanup of temporary and leftover files
        /// </summary>
        CleanupError,

        /// <summary>
        /// An unknown error has occurred
        /// </summary>
        UnknownError
    }
}
