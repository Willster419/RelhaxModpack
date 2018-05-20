using System.Drawing;

namespace RelhaxModpack.AtlasesCreator
{
    public class JpegImageImporter : IImageImporter
    {
        public string ImageExtension
        {
            get { return "jpg"; }
        }

        public Bitmap Load(string filename)
        {
            return new Bitmap(filename);
        }
    }
}
