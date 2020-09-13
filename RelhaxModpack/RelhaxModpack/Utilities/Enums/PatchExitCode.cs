namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// Exit codes during a patch operation
    /// </summary>
    public enum PatchExitCode
    {
        /// <summary>
        /// The patch operation succeeded
        /// </summary>
        Success = 0,

        /// <summary>
        /// The patch operation succeeded, but with warnings
        /// </summary>
        Warning = -1,

        /// <summary>
        /// The patch operation did not succeeded
        /// </summary>
        Error = -2
    }
}
