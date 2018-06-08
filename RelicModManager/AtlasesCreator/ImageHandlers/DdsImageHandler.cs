using System.Drawing;
using System;
using TeximpNet;
using TeximpNet.Compression;

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
            /*
            libaries needed to make this code work:
            FreeImage32.dll
            FreeImage64.dll
            nvtt32.dll
            nvtt64.dll
            TeximpNet.dll
            TeximpNet.xml
            */
            System.Drawing.Imaging.BitmapData bmpData = null;
            Compressor compressor = null;
            try
            {
                // Lock the bitmap's bits. 
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                bmpData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

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
                    // DDSFile.Write(filename, surfaceFromRawData, TextureDimension.Two, DDSFlags.None);
                    compressor = new Compressor();
                    compressor.Compression.Format = CompressionFormat.DXT5;
                    compressor.Input.AlphaMode = AlphaMode.None;
                    compressor.Input.GenerateMipmaps = false;
                    compressor.Input.ConvertToNormalMap = false;
                    compressor.Input.SetData(surfaceFromRawData);
                    compressor.Process(filename);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("DssImageExporter", "Write", ex);
                }
            }
            finally
            {
                compressor.Dispose();
                image.UnlockBits(bmpData);
                image.Dispose();
            }
        }

        private static readonly int CHUNK_SIZE = 0x20;
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
                Logging.Manager(filename + " is maybe a corrupted file (to short to be an image)");
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
            Int64 stamp = buffer[0x0] | (((Int64)buffer[0x1]) << 8) | (((Int64)buffer[0x2]) << 16) | (((Int64)buffer[0x3]) << 24);
            return stamp == 0x20534444;
        }
    }
}
