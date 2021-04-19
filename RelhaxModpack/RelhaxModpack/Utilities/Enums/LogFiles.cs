namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The different log files currently used in the modpack
    /// </summary>
    public enum Logfiles
    {
        /// <summary>
        /// The default modpack log file
        /// </summary>
        Application,

        /// <summary>
        /// The log file for when installing mods
        /// </summary>
        Installer,

        /// <summary>
        /// The log file for when uninstalling mods
        /// </summary>
        Uninstaller,

        /// <summary>
        /// The log file for the editor
        /// </summary>
        Editor,

        /// <summary>
        /// The log file for the patcher
        /// </summary>
        PatchDesigner,

        /// <summary>
        /// The log file for the database update tool
        /// </summary>
        Updater,

        /// <summary>
        /// The log file for the database automation runner window
        /// </summary>
        AutomationRunner
    }
}
