namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible return code error conditions that can occur when running an automation task
    /// </summary>
    public enum AutomationExitCode
    {
        None = 0,

        ExecuteException = -1,

        ValidateCommandsFail = 1,

        ProcessResultsFail = 2,

        RunTaskFail = 3,

        IOFail = 4,

        ZipFail = 5,

        ShellFail = 6,

        DatabaseLoadFail = 7,

        DatabaseSaveFail = 8,

        FileDownloadFail = 9,

        FileUploadFail = 10,

        ProcessMacrosFail = 11,

        ParseTaskFail = 12,

        ComparisonNoFilesToUpdate = 42,

        ComparisonManualFilesToUpdate = 44,

        GenericEndSequenceEarlyOk = 43
    }
}
