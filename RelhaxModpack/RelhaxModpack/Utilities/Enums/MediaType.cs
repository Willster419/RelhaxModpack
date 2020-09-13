namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The supported types of media formats supported for preview in the application
    /// </summary>
    public enum MediaType
    {
        /// <summary>
        /// Catch-all case for unknown media when parsing
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A direct link to a picture
        /// </summary>
        Picture = 1,

        /// <summary>
        /// A direct link to a website (like for embedding a web player)
        /// </summary>
        Webpage = 2,

        /// <summary>
        /// A direct link to an audio file
        /// </summary>
        MediaFile = 3,

        /// <summary>
        /// Raw HTML code to display in embedded browser
        /// </summary>
        HTML = 4
    }
}
