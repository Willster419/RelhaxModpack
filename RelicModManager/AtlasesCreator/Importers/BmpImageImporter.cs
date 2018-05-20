using System.Drawing;

namespace RelhaxModpack.AtlasesCreator
{
    public class BmpImageImporter : IImageImporter
    {
        public string ImageExtension
        {
            get { return "bmp"; }
        }

        public Bitmap Load(string filename)
        {
            return new Bitmap(filename);
        }
    }
}
