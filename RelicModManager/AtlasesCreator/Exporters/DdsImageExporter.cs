using System.Drawing;
using System.IO;
using System;
using TeximpNet;
using TeximpNet.DDS;

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
            image.Save(Path.ChangeExtension(filename, ".png"), System.Drawing.Imaging.ImageFormat.Png);

            Logging.Manager("The code to create DDS images is not integrated, so a PNG image is generated instead.");

            /*
            libaries needed to make this code work:
            FreeImage32.dll
            FreeImage64.dll
            nvtt32.dll
            nvtt64.dll
            TeximpNet.dll
            TeximpNet.xml

            try
            {
                try
                {
                    image.Save(Path.ChangeExtension(filename, ".png"), System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("DssImageExporter", "SavePNG", ex);
                    return;
                }

                Surface surfaceFromFile = null;
                String file = null;

                try
                {
                    file = Path.ChangeExtension(filename, ".png");
                    surfaceFromFile = Surface.LoadFromFile(file, true);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("DssImageExporter", "LoadPNG", ex);
                    return;
                }

                try
                {
                    String fileOut = Path.ChangeExtension(filename, ".dds");
                    DDSFile.Write(fileOut, surfaceFromFile, TextureDimension.Two, DDSFlags.None);
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("DssImageExporter", "SaveDDS", ex);
                }
            }
            finally
            {
                image.Dispose();
            }
            */
        }
    }
}
