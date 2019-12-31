using RelhaxModpack.DatabaseComponents;

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
    public class Media : IXmlSerializable
    {
        #region Xml serialization
        public string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(MediaType), nameof(URL) };
        }

        public string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }
        #endregion

        /// <summary>
        /// For direct link types, the URL to the element or resource
        /// </summary>
        /// <remarks>HTTP and HTTPS links work for this property</remarks>
        public string URL { get; set; } = "";

        /// <summary>
        /// The type of media for the URL to be interpreted as
        /// </summary>
        public MediaType MediaType { get; set; } = MediaType.Picture;

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

        /// <summary>
        /// Create a copy of the Media object
        /// </summary>
        /// <param name="mediaToCopy">The object to copy</param>
        /// <returns>A new Media object with the same values</returns>
        public static Media Copy(Media mediaToCopy)
        {
            return new Media()
            {
                URL = mediaToCopy.URL,
                MediaType = mediaToCopy.MediaType
            };
        }
    }
}
