#region MIT License

/*
 * Copyright (c) 2009-2010 Nick Gravelyn (nick@gravelyn.com), Markus Ewald (cygon@nuclex.org)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software 
 * is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 * 
 */

#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace RelhaxModpack.AtlasesCreator
{
    public class BmpImageHandler : IImageHandler
    {
        public string ImageExtension
        {
            get { return "bmp"; }
        }

        public Bitmap Load(string filename)
        {
            return new Bitmap(filename);
        }

        public bool Save(string filename, Bitmap image)
        {
            // change the "black/transparent" background to white
            // https://stackoverflow.com/questions/6513633/convert-transparent-png-to-jpg-with-non-black-background-color
            using (var b = new Bitmap(image.Width, image.Height))
            {
                b.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var g = Graphics.FromImage(b))
                {
                    g.Clear(Color.White);
                    g.DrawImageUnscaled(image, 0, 0);
                }
                try
                {
                    b.Save(filename, ImageFormat.Bmp);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("JpgImageHandler", "Save", ex);
                    return false;
                }
            }
            return true;
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
                if (!IsBMP(chunk))
                {
                    Logging.Manager(filename + " is NOT a valid " + ImageExtension.ToUpper() + " image");
                }
                else
                {
                    size.Height = chunk[0x16] | chunk[0x17] << 8 | chunk[0x18] << 16 | chunk[0x19] << 24;
                    size.Height = System.Math.Abs(size.Height);
                    size.Width = chunk[0x12] | chunk[0x13] << 8 | chunk[0x14] << 16 | chunk[0x15] << 24;
                }
            }
            // Logging.Manager(filename + " is: " + size.Height.ToString() + " height and " + size.Width.ToString() + " width.");
            return size;
        }

        private static bool IsBMP(byte[] buffer)
        {
            Int64 stamp = (((Int64)buffer[0x0]) << 8) | buffer[0x1];
            return stamp == 0x424d;
        }
    }
}