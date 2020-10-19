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

        public string[] PropertiesForSerializationAttributes()
        {
            return null;
        }

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
        /// The file name of where this texture came from
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public string Name { get; set; }

        /// <summary>
        /// The x position of the texture in the atlas image
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public int X { get; set; }

        /// <summary>
        /// The y position of the texture in the atlas image
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public int Y { get; set; }

        /// <summary>
        /// The width of the texture in the atlas image
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public int Width { get; set; }

        /// <summary>
        /// The height of the texture in the atlas image
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public int Height { get; set; }

        /// <summary>
        /// The actual bitmap in memory of the image
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        public Bitmap AtlasImage = null;
    }
}
