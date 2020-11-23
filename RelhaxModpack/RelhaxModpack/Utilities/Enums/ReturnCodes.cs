namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// Application return error codes
    /// </summary>
    public enum ReturnCodes
    {
        /// <summary>
        /// No error occurred
        /// </summary>
        Success = 0,

        /// <summary>
        /// Error with logfile creation
        /// </summary>
        LogfileError = 1,

        PatcherNoSpecifiedFiles = 2,

        PatcherNoPatchesParsed = 3,
    }
}