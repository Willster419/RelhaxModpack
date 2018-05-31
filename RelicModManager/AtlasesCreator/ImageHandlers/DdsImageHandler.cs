using System.Drawing;
using System.IO;
using System;
using TeximpNet;
using TeximpNet.DDS;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RelhaxModpack.AtlasesCreator
{
    public class DssImageHandler : IImageHandler
    {
        public string ImageExtension
        {
            get { return "dds"; }
        }

        public Bitmap Load(string filename)
        {
            return DDSReader.DDS.LoadImage(filename);
        }

        public void Save(string filename, Bitmap image)
        {
            // image.Save(Path.ChangeExtension(filename, ".png"), System.Drawing.Imaging.ImageFormat.Png);

            // Logging.Manager("The code to create DDS images is not integrated, so a PNG image is generated instead.");

            /*
            libaries needed to make this code work:
            FreeImage32.dll
            FreeImage64.dll
            nvtt32.dll
            nvtt64.dll
            TeximpNet.dll
            TeximpNet.xml
            */
            try
            {
                // Lock the bitmap's bits. 
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                System.Drawing.Imaging.BitmapData bmpData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;
                image.UnlockBits(bmpData);

                Surface surfaceFromRawData = null;
                try
                {
                    surfaceFromRawData = Surface.LoadFromRawData(ptr, image.Width, image.Height, bmpData.Stride, true);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("DssImageExporter", "LoadFromRawData", ex);
                    return;
                }

                if (surfaceFromRawData == null)
                {
                    Logging.Manager("Failed to get surfaceFromRawData");
                    return;
                }
                try
                {
                    DDSFile.Write(filename, surfaceFromRawData, TextureDimension.Two, DDSFlags.None);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("DssImageExporter", "Write", ex);
                }
            }
            finally
            {
                image.Dispose();
            }
        }

        private static readonly int CHUNK_SIZE = 32;
        public Size GetImageSize(string filename)
        {
            byte[] chunk = null;
            Size size = new Size
            {
                Height = 0,
                Width = 0
            };

            chunk = Utils.ReadByteArrayFromFile(filename, CHUNK_SIZE);

            if (chunk.Length < CHUNK_SIZE)
            {
                Logging.Manager(filename + " is corrupted and NOT a valid " + ImageExtension.ToUpper() + " image");
            }
            else
            {
                if (!IsDDS(chunk))
                {
                    Logging.Manager(filename + " is NOT a valid " + ImageExtension.ToUpper() + " image");
                }
                else
                {
                    size.Height = chunk[0x0c] | chunk[0x0d] << 8 | chunk[0x0e] << 16 | chunk[0x0f] << 24;
                    size.Width = chunk[0x10] | chunk[0x11] << 8 | chunk[0x12] << 16 | chunk[0x13] << 24;
                }
            }
            // Logging.Manager(filename + " is: " + size.Height.ToString() + " height and " + size.Width.ToString() + " width.");
            return size;
        }

        private static bool IsDDS(byte[] buffer)
        {
            // Logging.Manager(Utils.ConvertByteArrayToString(buffer));
            int stamp = buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
            return stamp == 0x20534444;
        }
    }
}
