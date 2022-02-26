namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible return code error conditions that can occur when running the htmlPath browser parser.
    /// </summary>
    public enum HtmlXpathParserExitCode
    {
        /// <summary>
        /// No error occurred.
        /// </summary>
        None = 0,

        /// <summary>
        /// There was an issue with the input parameters. Check the xpath and url.
        /// </summary>
        InvalidParameters,

        /// <summary>
        /// There was an error with downloading the html web page to a string.
        /// </summary>
        ErrorHtmlText,

        /// <summary>
        /// There was an error with parsing the htmlPath. Either there is a syntax error or the requested htmlNode was not found.
        /// </summary>
        ErrorHtmlParsing,

        /// <summary>
        /// There was an error saving the html document to disk.
        /// </summary>
        ErrorSavingToDisk
    }
}
