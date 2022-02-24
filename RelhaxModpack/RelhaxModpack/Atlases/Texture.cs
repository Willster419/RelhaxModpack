using RelhaxModpack.Database;
using System.Drawing;

namespace RelhaxModpack
{
    /// <summary>
    /// A Texture is a piece of an atlas file. Contains image data such as the position, size, and bitmap itself.
    /// </summary>
    public class Texture : IXmlSerializable
    {
        #region Xml Serialization
        /// <summary>
        /// Gets a list of properties that represent xml attributes to be loaded/saved to/from an xml document.
        /// </summary>
        /// <returns>The list of properties.</returns>
        public string[] PropertiesForSerializationAttributes()
        {
            return null;
        }

        /// <summary>
        /// Gets a list of properties thata represent xml elements to be loaded/saved to/from an xml document.
        /// </summary>
        /// <returns>The list of properties.</returns>
        public string[] PropertiesForSerializationElements()
        {
            return new string[]
            { 
                nameof(Name),
                nameof(X),
                nameof(Y),
                nameof(Width),
                nameof(Height)
            };
        }
        #endregion

        /// <summary>
        /// The file name of where this texture came from, without extension.
        /// </summary>
        /// <remarks>This is loaded from the map file.</remarks>
        public string Name { get; set; }

        /// <summary>
        /// The x position of the texture in the atlas image.
        /// </summary>
        /// <remarks>This is loaded from the map file.</remarks>
        public int X { get; set; }

        /// <summary>
        /// The y position of the texture in the atlas image.
        /// </summary>
        /// <remarks>This is loaded from the xml file.</remarks>
        public int Y { get; set; }

        /// <summary>
        /// The width of the texture in the atlas image.
        /// </summary>
        /// <remarks>This is loaded from the map file.</remarks>
        public int Width { get; set; }

        /// <summary>
        /// The height of the texture in the atlas image.
        /// </summary>
        /// <remarks>This is loaded from the map file.</remarks>
        public int Height { get; set; }

        /// <summary>
        /// The actual bitmap in memory of the image.
        /// </summary>
        /// <remarks>This is *not* loaded from the map file and is used internally.</remarks>
        public Bitmap AtlasImage = null;
    }
}
