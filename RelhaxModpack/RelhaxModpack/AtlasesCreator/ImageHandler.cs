using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeximpNet;
using TeximpNet.Compression;

namespace RelhaxModpack.AtlasesCreator
{
    /// <summary>
    /// Handles loading and saving a DDS image in bitmap form
    /// </summary>
    public class ImageHandler
    {
        //stride is rowpitch


        public static Bitmap LoadBitmapFromDDS(string filename)
        {
            //check to make sure file exists
            if(!File.Exists(filename))
            {
                Logging.Error("image file does not exist at path {0}", filename);
                return null;
            }

            //check to make sure file is a dds file
            if(!TeximpNet.DDS.DDSFile.IsDDSFile(filename))
            {
                Logging.Error("image is not a dds file: {0}", filename);
            }

            //load the image into freeImage format
            Surface surface = Surface.LoadFromFile(filename, ImageLoadFlags.Default);
            //flip it because it's upside down because reasons.
            surface.FlipVertically();
            return new Bitmap(surface.Width, surface.Height, surface.Pitch, System.Drawing.Imaging.PixelFormat.Format32bppArgb, surface.DataPtr);
        }

        public static bool SaveDDS(string filename, Bitmap image, bool disposeImage)
        {
            // Lock the bitmap's bits. 
            //https://stackoverflow.com/questions/28655133/difference-between-bitmap-and-bitmapdata
            //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata.scan0?view=netframework-4.8#System_Drawing_Imaging_BitmapData_Scan0
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bmpData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);

            //create surface object for processing
            Surface surfaceFromRawData = Surface.LoadFromRawData(bmpData.Scan0, image.Width, image.Height, bmpData.Stride, true);

            //compress to DDS
            using (Compressor compressor = new Compressor())
            {
                compressor.Compression.Format = CompressionFormat.DXT5;
                compressor.Input.AlphaMode = AlphaMode.None;
                compressor.Input.GenerateMipmaps = false;
                compressor.Input.ConvertToNormalMap = false;
                compressor.Input.SetData(surfaceFromRawData);
                compressor.Process(filename);
            }

            //dispose and finish
            if(disposeImage)
            {
                image.UnlockBits(bmpData);
                image.Dispose();
            }

            return true;
        }

        public static Bitmap GenerateImage(List<Texture> files, Dictionary<Texture, Rectangle> imagePlacement, int outputWidth, int outputHeight)
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
