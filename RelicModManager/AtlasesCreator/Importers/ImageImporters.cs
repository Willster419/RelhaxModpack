using System.Drawing;

namespace RelhaxModpack.AtlasesCreator
{
    /// <summary>
    /// An object able to load a sprite sheet image.
    /// </summary>
    public interface IImageImporter
    {
        /// <summary>
        /// Gets the extension for the image file type.
        /// </summary>
        string ImageExtension { get; }

        /// <summary>
        /// Load the image from a file.
        /// </summary>
        /// <param name="filename">The file from which the image should be loaded.</param>
        Bitmap Load(string filename);
    }
}
