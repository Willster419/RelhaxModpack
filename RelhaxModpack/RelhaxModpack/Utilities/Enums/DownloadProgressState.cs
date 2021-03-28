namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The primary functional modes the application can run in
    /// </summary>
    public enum DownloadProgressState
    {
        /// <summary>
        /// Default download state. No activity.
        /// </summary>
        None,

        /// <summary>
        /// The file stream and download stream have been opened. This will only fire once per download operation.
        /// </summary>
        OpenStreams,

        /// <summary>
        /// The file is being downloaded.
        /// </summary>
        Download,

        /// <summary>
        /// The file download is completed. This will only fire once per download operation.
        /// </summary>
        DownloadCompleted
    }
}
