namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The exit codes that can occur when running the instructions loader
    /// </summary>
    public enum InstructionsLoaderExitCode
    {
        /// <summary>
        /// No error occurred
        /// </summary>
        None,

        /// <summary>
        /// An error occurred when searching for files to load
        /// </summary>
        FileSearchError,

        /// <summary>
        /// An error occurred when loading the instructions to parse
        /// </summary>
        FileLoadError,

        /// <summary>
        /// An error occurred when parsing the instructions to a class object
        /// </summary>
        FileParseError
    }
}
