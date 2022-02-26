namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible return code error conditions that can occur when running an automation task.
    /// </summary>
    public enum AutomationExitCode
    {
        /// <summary>
        /// No error occurred, it ran correctly.
        /// </summary>
        None = 0,

        /// <summary>
        /// There was an unhandled program exception during the task main execution method.
        /// </summary>
        ExecuteException = -1,

        /// <summary>
        /// There was an error in the 'validate commands' step of the task.
        /// </summary>
        ValidateCommandsFail = 1,

        /// <summary>
        /// There was an error in the 'process results' step of the task.
        /// </summary>
        ProcessResultsFail = 2,

        /// <summary>
        /// There was a general error running the task.
        /// </summary>
        RunTaskFail = 3,

        /// <summary>
        /// There was an IO related error running the task.
        /// </summary>
        IOFail = 4,

        /// <summary>
        /// There was a process execution shell related error running the task.
        /// </summary>
        ShellFail = 6,

        /// <summary>
        /// There was an error loading the modpack database.
        /// </summary>
        DatabaseLoadFail = 7,

        /// <summary>
        /// There was an error saving the modpack database.
        /// </summary>
        DatabaseSaveFail = 8,

        /// <summary>
        /// There was a file download related error running the task.
        /// </summary>
        FileDownloadFail = 9,

        /// <summary>
        /// There was a file upload related error running the task.
        /// </summary>
        FileUploadFail = 10,

        /// <summary>
        /// There was an error processing macros when running the task.
        /// </summary>
        ProcessMacrosFail = 11,

        /// <summary>
        /// There were no files found during the compare session that needed to be updated. This is no an error, but meaning there is no work to do.
        /// </summary>
        ComparisonNoFilesToUpdate = 42,

        /// <summary>
        /// There were files found during the compare session that need to be manually updated.
        /// </summary>
        ComparisonManualFilesToUpdate = 44,

        /// <summary>
        /// The task was canceled.
        /// </summary>
        Cancel = 45
    }
}
