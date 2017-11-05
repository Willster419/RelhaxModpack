using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace RelhaxModpack.AtlasesExtrator
{
    public class Programm
    {
        public static void Main()
        {
            string ImageFile = @"C:\Users\Ich\Desktop\Test Atlases\battleAtlas\battleAtlas.png";
            string MapFile = @"C:\Users\Ich\Desktop\Test Atlases\battleAtlas\battleAtlas.xml";
            string workingFolder = @"D:\RelHax Manager\RelHaxTemp";
            if (Directory.Exists(workingFolder))
                Directory.Delete(workingFolder, true);



            int x = 10, y = 20, width = 200, height = 100;
            Bitmap source = new Bitmap(ImageFile);



                Bitmap CroppedImage = source.Clone(new System.Drawing.Rectangle(x, y, width, height), source.PixelFormat);
                CroppedImage.Save("file.png", ImageFormat.Png);
                CroppedImage.Dispose();

        }

    }
}
