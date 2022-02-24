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

namespace RelhaxModpack.Atlases
{
    /// <summary>
    /// Class for handling loading and saving of DDS atlas files.
    /// </summary>
    public class ImageHandler
    {
        /// <summary>
        /// Loads a DDS image into a Bitmap object.
        /// </summary>
        /// <param name="filename">The relative or absolute location of the DDS file.</param>
        /// <returns>The Bitmap of the DDS file, or null if it failed.</returns>
        public Bitmap LoadDDS(string filename)
        {
            //check to make sure file exists
            if (!File.Exists(filename))
            {
                Logging.Error("Image file does not exist at path {0}", filename);
                return null;
            }

            //check to make sure file is a DDS file
            if (!TeximpNet.DDS.DDSFile.IsDDSFile(filename))
            {
                Logging.Error("Image is not a DDS file: {0}", filename);
                return null;
            }

            //load the image into freeImage format
            Surface surface = Surface.LoadFromFile(filename, ImageLoadFlags.Default);

            //flip it because it's upside down because reasons.
            surface.FlipVertically();

            //copy the bitmap data to another Bitmap object to ensure a deep copy
            //https://stackoverflow.com/questions/2433481/is-passing-system-drawing-bitmap-across-class-libraries-unreliable
            using (Bitmap temp = new Bitmap(surface.Width, surface.Height, surface.Pitch, PixelFormat.Format32bppArgb, surface.DataPtr))
            {
                Bitmap copy = new Bitmap(temp);
                return copy;
            }
        }

        /// <summary>
        /// Saves a Bitmap image into a DDS file of DXT5 compression.
        /// </summary>
        /// <param name="savePath">The path to save the file.</param>
        /// <param name="image">The bitmap to save.</param>
        /// <param name="disposeImage">Set to true to dispose of the inputted bitmap after it's saved.</param>
        /// <returns>True if image creation was successful, false otherwise.</returns>
        public bool SaveDDS(string savePath, Bitmap image, bool disposeImage = false)
        {
            //get atlas filename
            string atlasFilename = Path.GetFileName(savePath);

            // Lock the bitmap's bits. 
            //https://stackoverflow.com/questions/28655133/difference-between-bitmap-and-bitmapdata
            //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata.scan0?view=netframework-4.8#System_Drawing_Imaging_BitmapData_Scan0
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            Logging.Debug("[atlas file {0}]: Locking bits of image {1} x {2} size", atlasFilename, image.Width, image.Height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);

            //create surface object for processing
            //and compress to DDS
            Logging.Debug("[atlas file {0}]: Bits locked, creating Surface and Compressor objects", atlasFilename);
            bool success = true;
            using (Surface surfaceFromRawData = Surface.LoadFromRawData(bmpData.Scan0, image.Width, image.Height, bmpData.Stride, true))
            using (Compressor compressor = new Compressor())
            {
                compressor.Compression.Format = CompressionFormat.DXT5;
                compressor.Input.AlphaMode = AlphaMode.None;
                compressor.Input.GenerateMipmaps = false;
                compressor.Input.ConvertToNormalMap = false;

                Logging.Debug("[atlas file {0}]: Attempting to set image data", atlasFilename);
                if (compressor.Input.SetData(surfaceFromRawData))
                {
                    Logging.Debug("[atlas file {0}]: Image data set, attempting to process for compression", atlasFilename);
                    success = compressor.Process(savePath);
                }
                else
                {
                    success = false;
                }
            }
            image.UnlockBits(bmpData);
            Logging.Debug("[atlas file {0}]: Processing status: {1}", atlasFilename, success);

            //dispose image if was requested
            if (disposeImage)
            {
                image.Dispose();
            }
            return success;
        }
    }
}
