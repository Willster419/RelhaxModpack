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
    public class JpegImageHandler : IImageHandler
    {
        public string ImageExtension
        {
            get { return "jpg"; }
        }

        public Bitmap Load(string filename)
        {
            return new Bitmap(filename);
        }

        public bool Save(string filename, Bitmap image)
        {
            try
            {
                image.Save(filename, ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("JpgImageHandler", "Save", ex);
                return false;
            }
            return true;
        }

        private static readonly int CHUNK_SIZE = 0x200;
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
                if (!IsJPG(chunk))
                {
                    Logging.Manager(filename + " is NOT a valid " + ImageExtension.ToUpper() + " image");
                }
                else
                {
                    // https://community.microfocus.com/microfocus/cobol/extend_and_acucobol/f/forum/6074/archive-retrieving-bitmap-width-and-height-demesions
                    byte[,] patternArray = new byte[,] { { 0xff, 0xc0, 0x00, 0x11 }, { 0xff, 0xc1, 0x00, 0x11 }, { 0xff, 0xc2, 0x00, 0x11 }};
                    int ffcx_ptr = 0;

                    for (int i = 0; i <= patternArray.GetUpperBound(0); i++)
                    {
                        byte[] pattern = { patternArray[i, 0], patternArray[i, 1], patternArray[i, 2], patternArray[i, 3] };
                        foreach (var position in chunk.FindBytePatternInByteArray(pattern))
                        {
                            ffcx_ptr = position;
                            // Logging.Manager("ffcx_ptr: " + position.ToString());
                        }
                    }
                    size.Height = chunk[ffcx_ptr + 0x05] << 8 | chunk[ffcx_ptr + 0x06];
                    size.Width = chunk[ffcx_ptr + 0x07] << 8 | chunk[ffcx_ptr + 0x08];
                }
            }
            // Logging.Manager(filename + " is: " + size.Height.ToString() + " height and " + size.Width.ToString() + " width.");
            return size;
        }

        private static bool IsJPG(byte[] buffer)
        {
            Int64 stamp1 = (((Int64)buffer[0x00]) << 24) | (((Int64)buffer[0x01]) << 16) | (((Int64)buffer[0x02]) << 8) | buffer[0x03];
            Int64 stamp2 = (((Int64)buffer[0x06]) << 32) | (((Int64)buffer[0x07]) << 24) | (((Int64)buffer[0x08]) << 16) | (((Int64)buffer[0x09]) << 8) | buffer[0x0a];
            return stamp1 == 0xffd8ffe0 && stamp2 == 0x4a46494600;
        }
    }
}