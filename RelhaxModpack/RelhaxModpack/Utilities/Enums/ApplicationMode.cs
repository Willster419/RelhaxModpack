namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The primary functional modes the application can run in
    /// </summary>
    public enum ApplicationMode
    {
        /// <summary>
        /// The default mode of modpack installer. This is the primary focus of the application
        /// </summary>
        Default,

        /// <summary>
        /// The database editor mode
        /// </summary>
        Editor,

        /// <summary>
        /// The updater mode. Used for updating the database, application, and other various functions
        /// </summary>
        Updater,

        /// <summary>
        /// The patch designer mode. Allow the user to create and test patches
        /// </summary>
        PatchDesigner,

        /// <summary>
        /// The patch runner mode. Can be used in command line mode, used for patching files given patch file instructions
        /// </summary>
        Patcher,

        /// <summary>
        /// The database automation runner mode. Used for running automation sequences
        /// </summary>
        AutomationRunner
    }
}
