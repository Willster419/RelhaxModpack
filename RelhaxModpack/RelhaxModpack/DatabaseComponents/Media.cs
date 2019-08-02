namespace RelhaxModpack
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

    /// <summary>
    /// A media object is a preview-able component stored in a list in SelectablePackages
    /// </summary>
    public class Media
    {
        /// <summary>
        /// For direct link types, the URL to the element or resource
        /// </summary>
        /// <remarks>HTTP and HTTPS links work for this property</remarks>
        public string URL = "";

        /// <summary>
        /// The type of media for the URL to be interpreted as
        /// </summary>
        public MediaType MediaType = MediaType.Picture;

        /// <summary>
        /// The string properties of the object
        /// </summary>
        /// <returns>The integer code of the MediaType and the first 80 characters of the URL</returns>
        public override string ToString()
        {
            if(URL.Length > 79)
                return "Type: " + (int)MediaType + " - " + URL.Substring(0, 80) + "...";
            else
                return "Type: " + (int)MediaType + " - " + URL;
        }
    }
}
