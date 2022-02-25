using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System.Collections.Generic;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// A media object is a preview-able component stored in a list in SelectablePackages.
    /// </summary>
    public class Media : XmlComponent, IXmlSerializable
    {
        /// <summary>
        /// Create an instance of the Media class.
        /// </summary>
        public Media() : base()
        {

        }

        /// <summary>
        /// Create an instance of the Media class, by copying values from a given Media object.
        /// </summary>
        /// <param name="mediaToCopy">The Media object to copy values from.</param>
        public Media(Media mediaToCopy) : base(mediaToCopy)
        {
            this.URL = mediaToCopy.URL;
            this.MediaType = mediaToCopy.MediaType;
        }

        #region Xml serialization V1
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(URL), nameof(MediaType) };
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }
        #endregion

        #region Xml serialization V2
        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.0 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(URL), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(URL) },
                new XmlDatabaseProperty() { XmlName = nameof(MediaType), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(MediaType) }
            };
            return xmlDatabaseProperties;
        }

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.1 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot1()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(MediaType), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(MediaType) },
                new XmlDatabaseProperty() { XmlName = nameof(URL), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(URL) }
            };
            return xmlDatabaseProperties;
        }

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.2 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot2()
        {
            return this.GetXmlDatabasePropertiesV1Dot1();
        }
        #endregion

        /// <summary>
        /// For direct link types, the URL to the element or resource.
        /// </summary>
        /// <remarks>HTTP and HTTPS links work for this property.</remarks>
        public string URL { get; set; } = string.Empty;

        /// <summary>
        /// The type of media for the URL to be interpreted as.
        /// </summary>
        public MediaType MediaType { get; set; } = MediaType.Picture;

        /// <summary>
        /// Gets or sets the SelectablePackage parent of the media item.
        /// </summary>
        /// <remarks>This is used in the preview window to get the name to display in the title.</remarks>
        public SelectablePackage SelectablePackageParent { get; set; } = null;

        /// <summary>
        /// The string properties of the object.
        /// </summary>
        /// <returns>The integer code of the MediaType and the first 80 characters of the URL.</returns>
        public override string ToString()
        {
            if(URL.Length > 79)
                return "Type: " + (int)MediaType + " - " + URL.Substring(0, 80) + "...";
            else
                return "Type: " + (int)MediaType + " - " + URL;
        }

        /// <summary>
        /// Create a copy of the Media object.
        /// </summary>
        /// <param name="mediaToCopy">The object to copy.</param>
        /// <returns>A new Media object with the same values.</returns>
        public static Media Copy(Media mediaToCopy)
        {
            return new Media(mediaToCopy);
        }
    }
}
