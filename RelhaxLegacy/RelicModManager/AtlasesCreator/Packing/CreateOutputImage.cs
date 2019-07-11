using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace RelhaxModpack.AtlasesCreator
{
    public class CreateOutputImage
    {
        public static Bitmap generateImage(List<Texture> files, Dictionary<Texture, Rectangle> imagePlacement, int outputWidth, int outputHeight)
        {
            try
            {
                Bitmap outputImage = new Bitmap(outputWidth, outputHeight, PixelFormat.Format32bppArgb);

                // draw all the images into the output image
                foreach (var image in files)
                {
                    Rectangle location = imagePlacement[image];

                    // copy pixels over to avoid antialiasing or any other side effects of drawing
                    // the subimages to the output image using Graphics
                    for (int x = 0; x < image.AtlasImage.Width; x++)
                        for (int y = 0; y < image.AtlasImage.Height; y++)
                            outputImage.SetPixel(location.X + x, location.Y + y, image.AtlasImage.GetPixel(x, y));
                }
                return outputImage;
            }
            catch
            {
                return null;
            }
        }
    }
}
