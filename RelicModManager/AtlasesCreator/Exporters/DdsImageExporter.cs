using System.Drawing;
using System.Drawing.Imaging;

namespace RelhaxModpack.AtlasesCreator
{
    public class DssImageExporter : IImageExporter
    {
        public string ImageExtension
        {
            get { return "dds"; }
        }

        public void Save(string filename, Bitmap image)
        {
            image.Save(filename, ImageFormat.Png);
        }
    }
}
