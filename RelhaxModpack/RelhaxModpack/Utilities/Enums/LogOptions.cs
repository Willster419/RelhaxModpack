namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// A series of additional options when writing a message to a log file.
    /// </summary>
    public enum LogOptions
    {
        /// <summary>
        /// No additional options.
        /// </summary>
        None, 

        /// <summary>
        /// Add the method name that this log message call is called from.
        /// </summary>
        MethodName, 

        /// <summary>
        /// Add the class name that this log message call is called from.
        /// </summary>
        ClassName, 

        /// <summary>
        /// Add the method and class name that this log messag ecall is called from.
        /// </summary>
        MethodAndClassName
    }
}