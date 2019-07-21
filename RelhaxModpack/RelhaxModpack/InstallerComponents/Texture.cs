using System.Drawing;

namespace RelhaxModpack
{
    /// <summary>
    /// A Texture is a piece of an atlas file. Contains image data such as the position, size, and bitmap itself.
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// The file name of where this texture came from
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The x position of the texture in the atlas image
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The y position of the texture in the atlas image
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The width of the texture in the atlas image
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the texture in the atlas image
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The actual bitmap in memory of the image
        /// </summary>
        public Bitmap AtlasImage { get; set; }
    }
}
