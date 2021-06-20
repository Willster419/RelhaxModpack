namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible return code error conditions that can occur when loading the database
    /// </summary>
    public enum DatabaseLoadFailCode
    {
        /// <summary>
        /// No error occurred
        /// </summary>
        None = 0,

        FailedToDownloadZipFile,

        FailedToExtractXmlFromZipFile,

        FailedToParseXml,

        FailedToLoadXmlFromDisk,

        FailedToParseDatabase
    }
}
