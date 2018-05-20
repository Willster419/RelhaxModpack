using System.Drawing;

namespace RelhaxModpack.AtlasesCreator
{
    public class PngImageImporter : IImageImporter
    {
        public string ImageExtension
        {
            get { return "png"; }
        }

        public Bitmap Load(string filename)
        {
            return new Bitmap(filename);
        }
    }
}