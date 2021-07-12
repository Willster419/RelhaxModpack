namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The different behaviors of the comparison
    /// </summary>
    public enum AutomationCompareMode
    {
        /// <summary>
        /// If the file hashes are different, then continue the sequence. This is the primary behavior.
        /// </summary>
        /// <remarks>The implied use of this mode is that if the hashes are different, then the application
        /// needs to continue to update the version of the package in our database</remarks>
        NoMatchContinue,

        /// <summary>
        /// If the file hashes are different, then stop the sequence with appropriate exit code.
        /// </summary>
        /// <remarks>If the exit code associated with this enumeration occurs, then it implies that this
        /// file must be manually inspected and cannot be an automated change</remarks>
        NoMatchStop
    }
}
