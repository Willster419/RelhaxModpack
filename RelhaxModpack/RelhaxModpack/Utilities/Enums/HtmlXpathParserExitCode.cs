namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible return code error conditions that can occur when running the htmlPath browser parser
    /// </summary>
    public enum HtmlXpathParserExitCode
    {
        /// <summary>
        /// No error occurred
        /// </summary>
        None = 0,

        InvalidParameters,

        ErrorHtmlText,

        ErrorHtmlParsing,

        ErrorSavingToDisk
    }
}
