namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The level of severity of the log message
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug message
        /// </summary>
        Debug,
        /// <summary>
        /// Informational message
        /// </summary>
        Info,
        /// <summary>
        /// A problem, but can be worked around
        /// </summary>
        Warning,
        /// <summary>
        /// Something is wrong, something may not work
        /// </summary>
        Error,
        /// <summary>
        /// Something is wrong, something will not work
        /// </summary>
        Exception,
        /// <summary>
        /// The application is closing now
        /// </summary>
        ApplicationHalt
    }
}
