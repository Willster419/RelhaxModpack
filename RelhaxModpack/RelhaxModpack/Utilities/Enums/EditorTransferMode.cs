namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The primary functional modes the application can run in
    /// </summary>
    public enum EditorTransferMode
    {
        /// <summary>
        /// Downloading a zip file from the FTP wot online folder to disk
        /// </summary>
        DownloadZip,

        /// <summary>
        /// Uploading a zip file from disk to the FTP wot online folder
        /// </summary>
        UploadZip,

        /// <summary>
        /// Uploading a media preview file from disk to the FTP Medias folder
        /// </summary>
        UploadMedia
    }
}
