using System.Drawing;
using System.Drawing.Imaging;

namespace RelhaxModpack.AtlasesCreator
{
    public class DdsImageImporter : IImageImporter
    {
        public string ImageExtension
        {
            get { return "dds"; }
        }

        public Bitmap Load(string filename)
        {
            return DDSReader.DDS.LoadImage(filename);
        }
    }
}