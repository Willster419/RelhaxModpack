namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible return code error conditions that can occur when loading the database.
    /// </summary>
    public enum DatabaseLoadFailCode
    {
        /// <summary>
        /// No error occurred.
        /// </summary>
        None = 0,

        /// <summary>
        /// There was an error downloading the mod info data file (zip file).
        /// </summary>
        FailedToDownloadZipFile,

        /// <summary>
        /// There was an error extracting an xml document from the mod info data file (zip file).
        /// </summary>
        FailedToExtractXmlFromZipFile,

        /// <summary>
        /// There was an error loading the xml document into an xml object for database parsing.
        /// </summary>
        FailedToParseXml,

        /// <summary>
        /// There was an error loading the xml document from disk to into an xml object for database parsing.
        /// </summary>
        FailedToLoadXmlFromDisk,

        /// <summary>
        /// There was an error parsing the xml document object into the database manager package lists.
        /// </summary>
        FailedToParseDatabase
    }
}
