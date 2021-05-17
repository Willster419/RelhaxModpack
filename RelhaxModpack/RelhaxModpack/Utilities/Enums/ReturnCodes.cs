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

        /// <summary>
        /// No files specified on the command line when in patch mode
        /// </summary>
        PatcherNoSpecifiedFiles = 2,

        /// <summary>
        /// No patch objects parsed when specified on the command line when in patch mode
        /// </summary>
        PatcherNoPatchesParsed = 3,
    }
}