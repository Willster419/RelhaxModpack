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
    public class PngImageHandler : IImageHandler
    {
        public string ImageExtension
        {
            get { return "png"; }
        }

        public Bitmap Load(string filename)
        {
            return new Bitmap(filename);
        }

        public bool Save(string filename, Bitmap image)
        {
            try
            {
                image.Save(filename, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("PngImageHandler", "Save", ex);
                return false;
            }
            return true;
        }

        private static readonly int CHUNK_SIZE = 0x100;
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
                if (!IsPNG(chunk))
                {
                    Logging.Manager(filename + " is NOT a valid " + ImageExtension.ToUpper() + " image");
                }
                else
                {
                    var pattern = new byte[] { 0x49, 0x48, 0x44, 0x52 };
                    int IHDR_ptr = 0;
                    foreach (var position in chunk.FindBytePatternInByteArray(pattern))
                    {
                        IHDR_ptr = position;
                        // Logging.Manager("IHDR_ptr: " + position.ToString());
                    }
                    size.Height = chunk[IHDR_ptr + 0x08] << 24 | chunk[IHDR_ptr + 0x09] << 16 | chunk[IHDR_ptr + 0x0a] << 8 | chunk[IHDR_ptr + 0x0b];
                    size.Width = chunk[IHDR_ptr + 0x04] << 24 | chunk[IHDR_ptr + 0x05] << 16 | chunk[IHDR_ptr + 0x06] << 8 | chunk[IHDR_ptr + 0x07];
                }
            }
            // Logging.Manager(filename + " is: " + size.Height.ToString() + " height and " + size.Width.ToString() + " width.");
            return size;
        }

        private static bool IsPNG(byte[] buffer)
        {
            Int64 stamp = (((Int64)buffer[0x0]) << 40) | (((Int64)buffer[0x1]) << 32) | (((Int64)buffer[0x2]) << 24) | (((Int64)buffer[0x3]) << 16) | (((Int64)buffer[0x4]) << 8) | buffer[0x5];
            return stamp == 0x89504e470d0a;
        }
    }
}