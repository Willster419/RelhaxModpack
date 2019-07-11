/*
* Copyright (c) 2016-2018 TeximpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.Unmanaged;
using System.IO;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet
{
    /// <summary>
    /// Represents a 2D image surface. This object wraps a FreeImage bitmap.
    /// </summary>
    public sealed class Surface : IDisposable
    {
        private IntPtr m_imagePtr;

        private bool m_isDisposed;

        /// <summary>
        /// Gets if the OS is little endian. If Big Endian, then surface data is RGBA. If little, then surface data is BGRA.
        /// </summary>
        public static bool IsLittleEndian
        {
            get
            {
                FreeImageLibrary lib = FreeImageLibrary.Instance;
                if (lib == null || !lib.IsLibraryLoaded)
                    return true; //True for most platforms

                return lib.IsLittleEndian;
            }
        }

        /// <summary>
        /// Gets what color order the data in the surface will be stored at (this is coupled to endianness).
        /// </summary>
        /// <remarks>
        /// Note: This is based on the default compilation options for the FreeImage library. You can compile
        /// the library to use a hardcoded color order which we cannot detect. Make sure the native library is compiled
        /// with the default color order that is coupled to endianness.
        /// </remarks>
        public static bool IsBGRAOrder
        {
            get
            {
                return IsLittleEndian;
            }
        }

        /// <summary>
        /// Gets a struct that contains further details about color order that FreeImage expects (this is coupled to endianness). Such information
        /// includes the default mask values that are used for 4-component color channels.
        /// </summary>
        /// <remarks>
        /// Note: This is based on the default compilation options for the FreeImage library. You can compile
        /// the library to use a hardcoded color order which we cannot detect. Make sure the native library is compiled
        /// with the default color order that is coupled to endianness.
        /// </remarks>
        public static ColorOrder ColorOrder
        {
            get
            {
                return new ColorOrder(IsLittleEndian);
            }
        }

        /// <summary>
        /// Gets if the surface has been disposed or not.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return m_isDisposed;
            }
        }

        /// <summary>
        /// Gets the image type.
        /// </summary>
        public ImageType ImageType
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return ImageType.Unknown;

                return FreeImageLibrary.Instance.GetImageType(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets the number of bits per pixel. E.g. a typical RGBA bitmap would be 32 bits per pixel.
        /// </summary>
        public int BitsPerPixel
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetBitsPerPixel(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets the width of the image in bytes, rounded to the next 32-bit boundary. Also known as stride or scan width.
        /// </summary>
        public int Pitch
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetPitch(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets the width of the image in texels.
        /// </summary>
        public int Width
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetWidth(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets the height of the image in texels.
        /// </summary>
        public int Height
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetHeight(m_imagePtr);
            }
        }

        /// <summary>
        /// Returns the bit pattern that describes the red color component of a texel.
        /// </summary>
        public uint RedMask
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetRedMask(m_imagePtr);
            }
        }

        /// <summary>
        /// Returns the bit pattern that describes the green color component of a texel.
        /// </summary>
        public uint GreenMask
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetGreenMask(m_imagePtr);
            }
        }

        /// <summary>
        /// Returns the bit pattern that describes the blue color component of a texel.
        /// </summary>
        public uint BlueMask
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return FreeImageLibrary.Instance.GetBlueMask(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets whether the image has transparency or not.
        /// </summary>
        public bool IsTransparent
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return false;

                return FreeImageLibrary.Instance.IsTransparent(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets the color model of the bitmap.
        /// </summary>
        public ImageColorType ColorType
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return ImageColorType.RGBA;

                return FreeImageLibrary.Instance.GetImageColorType(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets a pointer to bitmap data.
        /// </summary>
        public IntPtr DataPtr
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return IntPtr.Zero;

                return FreeImageLibrary.Instance.GetData(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets a pointer to palette data. The total number of colors in the palette can be determined by <see cref="PaletteColorCount"/>. Each color is comprised of 4 bytes, one for each channel (Red, Green, Blue, Alpha). BGRA or RGBA color ordering
        /// is determined by <see cref="IsBGRAOrder"/>.
        /// </summary>
        public IntPtr PalettePtr
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return IntPtr.Zero;

                return FreeImageLibrary.Instance.GetPalette(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets if the image has a color palette or not.
        /// </summary>
        public bool HasPalette
        {
            get
            {
                return PalettePtr != IntPtr.Zero;
            }
        }

        /// <summary>
        /// If the image uses a palette, get the number of colors in the palette. If no palette is used, this will be zero.
        /// </summary>
        public int PaletteColorCount
        {
            get
            {
                if (m_imagePtr == IntPtr.Zero)
                    return 0;

                return (int)FreeImageLibrary.Instance.GetPaletteColorCount(m_imagePtr);
            }
        }

        /// <summary>
        /// Gets the pointer to the native FreeImage object.
        /// </summary>
        public IntPtr NativePtr
        {
            get
            {
                return m_imagePtr;
            }
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Surface"/> class. The image type will be <see cref="ImageType.Bitmap"/> that
        /// is 32-bit.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        public Surface(int width, int height) : this(32, width, height) { }

        /// <summary>
        /// Constructs a new instance of the <see cref="Surface"/> class. The image type will be <see cref="ImageType.Bitmap"/> that
        /// is either 32- or 24-bit.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="hasAlpha">If true then a 32-bit RGBA bitmap is created, if false then a 24-bit RGB bitmap is created.</param>
        public Surface(int width, int height, bool hasAlpha) : this((hasAlpha) ? 32 : 24, width, height) { }

        /// <summary>
        /// Constructs a new instance of the <see cref="Surface"/> class. The image type will be <see cref="ImageType.Bitmap"/>.
        /// </summary>
        /// <param name="bpp">Bit depth. Supported depth: 1-,4-,8-,16-,24-,32-bits per pixel.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        public Surface(int bpp, int width, int height) : this(bpp, width, height, 0, 0, 0) { }

        /// <summary>
        /// Constructs a new instance of the <see cref="Surface"/> class. The image type will be <see cref="ImageType.Bitmap"/>.
        /// </summary>
        /// <param name="bpp">Bit depth. Supported depth: 1-,4-,8-,16-,24-,32-bits per pixel.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="redMask">Red part of the color layout, e.g. 0xFF0000.</param>
        /// <param name="greenMask">Green part of the color layout, e.g. 0x00FF00. </param>
        /// <param name="blueMask">Blue part of the color layout, e.g. 0x0000FF.</param>
        public Surface(int bpp, int width, int height, uint redMask, uint greenMask, uint blueMask)
        {
            m_imagePtr = FreeImageLibrary.Instance.Allocate(width, height, bpp, redMask, greenMask, blueMask);

            AddGCMemoryPressure();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Surface"/> class.
        /// </summary>
        /// <param name="imageType">Type of the image.</param>
        /// <param name="bpp">Bit depth. Supported depth: 1-,4-,8-,16-,24-,32-bits per pixel.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        public Surface(ImageType imageType, int bpp, int width, int height) : this(imageType, bpp, width, height, 0, 0, 0) { }

        /// <summary>
        /// Constructs a new instance of the <see cref="Surface"/> class.
        /// </summary>
        /// <param name="imageType">Type of the image.</param>
        /// <param name="bpp">Bit depth. Supported depth: 1-,4-,8-,16-,24-,32-bits per pixel.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="redMask">Red part of the color layout, e.g. 0xFF0000.</param>
        /// <param name="greenMask">Green part of the color layout, e.g. 0x00FF00. </param>
        /// <param name="blueMask">Blue part of the color layout, e.g. 0x0000FF.</param>
        public Surface(ImageType imageType, int bpp, int width, int height, uint redMask, uint greenMask, uint blueMask)
        {
            m_imagePtr = FreeImageLibrary.Instance.AllocateT(imageType, width, height, bpp, redMask, greenMask, blueMask);

            AddGCMemoryPressure();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Surface"/> class.
        /// </summary>
        /// <param name="imagePtr">FreeImage bitmap pointer.</param>
        public Surface(IntPtr imagePtr)
        {
            m_imagePtr = imagePtr;

            AddGCMemoryPressure();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Surface"/> class.
        /// </summary>
        ~Surface()
        {
            Dispose(false);
        }

        /// <summary>
        /// Loads a surface from a file.
        /// </summary>
        /// <param name="filename">Name of file to load.</param>
        /// <param name="flags">Optional flags, by default this is <see cref="ImageLoadFlags.Default"/>.</param>
        /// <returns>Loaded surface, or null if there was an error in loading.</returns>
        public static Surface LoadFromFile(String filename, ImageLoadFlags flags = ImageLoadFlags.Default)
        {
            IntPtr imagePtr = FreeImageLibrary.Instance.LoadFromFile(filename, flags);
            if (imagePtr == IntPtr.Zero)
                return null;

            return new Surface(imagePtr);
        }

        /// <summary>
        /// Loads a surface from a file.
        /// </summary>
        /// <param name="filename">Name of file to load.</param>
        /// <param name="flipImage">Flip the image, by default FreeImage loads an image with the origin lower-left. Some applications may expect the image origin
        /// to be upper-left.</param>
        /// <param name="flags">Optional flags, by default this is <see cref="ImageLoadFlags.Default"/>.</param>
        /// <returns>Loaded surface, or null if there was an error in loading.</returns>
        public static Surface LoadFromFile(String filename, bool flipImage, ImageLoadFlags flags = ImageLoadFlags.Default)
        {
            Surface surface = LoadFromFile(filename, flags);
            if (surface != null && flipImage)
                surface.FlipVertically();

            return surface;
        }

        /// <summary>
        /// Loads a surface from a stream.
        /// </summary>
        /// <param name="stream">Stream to load data from.</param>
        /// <param name="flags">Optional flags, by default this is <see cref="ImageLoadFlags.Default"/>.</param>
        /// <returns>Loaded surface, or null if there was an error in loading.</returns>
        public static Surface LoadFromStream(Stream stream, ImageLoadFlags flags = ImageLoadFlags.Default)
        {
            IntPtr imagePtr = FreeImageLibrary.Instance.LoadFromStream(stream, flags);
            if (imagePtr == IntPtr.Zero)
                return null;

            return new Surface(imagePtr);
        }

        /// <summary>
        /// Loads a surface from a stream.
        /// </summary>
        /// <param name="stream">Stream to load data from.</param>
        /// <param name="flipImage">Flip the image, by default FreeImage loads an image with the origin lower-left. Some applications may expect the image origin
        /// to be upper-left.</param>
        /// <param name="flags">Optional flags, by default this is <see cref="ImageLoadFlags.Default"/>.</param>
        /// <returns>Loaded surface, or null if there was an error in loading.</returns>
        public static Surface LoadFromStream(Stream stream, bool flipImage, ImageLoadFlags flags = ImageLoadFlags.Default)
        {
            Surface surface = LoadFromStream(stream, flags);
            if (surface != null && flipImage)
                surface.FlipVertically();

            return surface;
        }

        /// <summary>
        /// Loads a surface from a raw 32-bit RGBA/BGRA image.
        /// </summary>
        /// <param name="imageDataPtr">Pointer to image data.</param>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="rowPitch">Pitch of the image, the # of bytes per scanline, which may include padding. </param>
        /// <param name="isBGRA">True if the data is BGRA, false if RGBA ordering. The input image may have to be converted to the default color ordering of the FreeImage surface (usually BGRA).</param>
        /// <param name="isTopDown">True if the image origin is considered to be upper left, false if lower left. FreeImage images are lower left origin, so if true this will cause the image data to be flipped vertically.</param>
        /// <returns>Surface containing the image, or null of there was an error in loading.</returns>
        public static Surface LoadFromRawData(IntPtr imageDataPtr, int width, int height, int rowPitch, bool isBGRA, bool isTopDown = false)
        {
            ColorOrder colorOrder = Surface.ColorOrder;
            int formatSizeBytes = 4; //32 bpp pixels;
            uint formatSizeBits = 32;

            IntPtr data = imageDataPtr;
            bool disposeData = false;

            try
            {
                //Convert if color order does not match incoming color order
                if (colorOrder.IsBGRAOrder != isBGRA)
                {
                    disposeData = true;
                    data = MemoryHelper.AllocateMemory(width * height * formatSizeBytes);
                    ImageHelper.CopyColorImageData(data, imageDataPtr, rowPitch, 0, width, height, 1, true);

                    //Compute new row pitch
                    rowPitch = width * formatSizeBytes;
                }

                IntPtr surfacePtr = FreeImageLibrary.Instance.ConvertFromRawBitsEx(true, data, ImageType.Bitmap, width, height, rowPitch, formatSizeBits, colorOrder.RedMask, colorOrder.GreenIndex, colorOrder.BlueMask, isTopDown);
                if (surfacePtr == IntPtr.Zero)
                    return null;

                return new Surface(surfacePtr);
            }
            finally
            {
                //Free temp memory, if we allocated it
                if (disposeData)
                    MemoryHelper.FreeMemory(data);
            }
        }

        /// <summary>
        /// Saves the surface to a file. This will overwrite a file that already exists.
        /// </summary>
        /// <param name="format">File format to save image in.</param>
        /// <param name="fileName">Name of file to create.</param>
        /// <param name="flags">Optional save flags, by default this is <see cref="ImageSaveFlags.Default"/>.</param>
        /// <returns>True if the operation is successful, false if otherwise.</returns>
        public bool SaveToFile(ImageFormat format, String fileName, ImageSaveFlags flags = ImageSaveFlags.Default)
        {
            if (String.IsNullOrEmpty(fileName) || m_imagePtr == IntPtr.Zero || format == ImageFormat.Unknown)
                return false;

            return FreeImageLibrary.Instance.SaveToFile(format, m_imagePtr, fileName, flags);
        }

        /// <summary>
        /// Saves the surface to a stream.
        /// </summary>
        /// <param name="format">File format to save image in.</param>
        /// <param name="stream">Stream to output image to.</param>
        /// <param name="flags">Optional save flags, by default this is <see cref="ImageSaveFlags.Default"/>.</param>
        /// <returns>True if the operation is successful, false if otherwise.</returns>
        public bool SaveToStream(ImageFormat format, Stream stream, ImageSaveFlags flags = ImageSaveFlags.Default)
        {
            if (stream == null || !stream.CanWrite || m_imagePtr == IntPtr.Zero || format == ImageFormat.Unknown)
                return false;

            return FreeImageLibrary.Instance.SaveToStream(format, m_imagePtr, stream, flags);
        }

        /// <summary>
        /// Gets a pointer to the start of the specified scan line.
        /// </summary>
        /// <param name="scanLine">Scanline to obtain a pointer to. The number of scanlines in an image is Height-1.</param>
        /// <returns>Pointer to scanline.</returns>
        public IntPtr GetScanLine(int scanLine)
        {
            if (m_imagePtr == IntPtr.Zero)
                return IntPtr.Zero;

            int height = Height;
            if (scanLine < 0 || scanLine >= height)
                return IntPtr.Zero;

            return FreeImageLibrary.Instance.GetScanLine(m_imagePtr, scanLine);
        }

        /// <summary>
        /// Clones the surface into a new instance. If the surface is disposed then this returns null.
        /// </summary>
        /// <returns>Cloned surface.</returns>
        public Surface Clone()
        {
            if (m_imagePtr == IntPtr.Zero)
                return null;

            IntPtr imagePtr = FreeImageLibrary.Instance.Clone(m_imagePtr);
            return new Surface(imagePtr);
        }

        /// <summary>
        /// Clones a sub-image of the surface into a new instance. If the surface is disposed or if the rectangle
        /// is not valid, then this returns null.
        /// </summary>
        /// <param name="left">Leftmost texel position of the rectangle.</param>
        /// <param name="top">Topmost texel position of the rectangle.</param>
        /// <param name="right">Rightmost texel position of the rectangle.</param>
        /// <param name="bottom">Bottommost texel position of the rectangle.</param>
        /// <returns>Subimage surface.</returns>
        public Surface Clone(int left, int top, int right, int bottom)
        {
            if (m_imagePtr == IntPtr.Zero)
                return null;

            IntPtr subimagePtr = FreeImageLibrary.Instance.Copy(m_imagePtr, left, top, right, bottom);
            if (subimagePtr == IntPtr.Zero)
                return null;

            return new Surface(subimagePtr);
        }

        /// <summary>
        /// Copies the src bitmap into this bitmap, starting at the left/top texel location.
        /// </summary>
        /// <param name="src">Bitmap to copy from.</param>
        /// <param name="left">Leftmost texel in this bitmap to start copying data to.</param>
        /// <param name="top">Topmost texel in this bitmap to start copying data to.</param>
        /// <param name="alphaBlend">0-255 alphablend value, if >= 255 then the src image data overwrites completely.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool CopyFrom(Surface src, int left, int top, int alphaBlend = 255)
        {
            if (m_imagePtr == IntPtr.Zero || src == null || src.m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.Paste(m_imagePtr, src.m_imagePtr, left, top, alphaBlend);
        }

        /// <summary>
        /// Converts the surface to another image format. This will create a new surface internally
        /// and dispose of the previous one.
        /// </summary>
        /// <param name="convertTo">Format to convert to.</param>
        /// <returns>True if the operation was successful, false otherwise. If conversion fails, the current data is not disposed.</returns>
        public bool ConvertTo(ImageConversion convertTo)
        {
            IntPtr newImagePtr = IntPtr.Zero;

            switch (convertTo)
            {
                case ImageConversion.To4Bits:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo4Bits(m_imagePtr);
                    break;
                case ImageConversion.To8Bits:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo8Bits(m_imagePtr);
                    break;
                case ImageConversion.To16Bits555:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo16Bits555(m_imagePtr);
                    break;
                case ImageConversion.To16Bits565:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo16Bits565(m_imagePtr);
                    break;
                case ImageConversion.To24Bits:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo24Bits(m_imagePtr);
                    break;
                case ImageConversion.To32Bits:
                    newImagePtr = FreeImageLibrary.Instance.ConvertTo32Bits(m_imagePtr);
                    break;
                case ImageConversion.ToGreyscale:
                    newImagePtr = FreeImageLibrary.Instance.ConvertToGreyscale(m_imagePtr);
                    break;
                case ImageConversion.ToFloat:
                    newImagePtr = FreeImageLibrary.Instance.ConvertToFloat(m_imagePtr);
                    break;
                case ImageConversion.ToUInt16:
                    newImagePtr = FreeImageLibrary.Instance.ConvertToUINT16(m_imagePtr);
                    break;
                case ImageConversion.ToRGBF:
                    newImagePtr = FreeImageLibrary.Instance.ConvertToRGBF(m_imagePtr);
                    break;
                case ImageConversion.ToRGBAF:
                    newImagePtr = FreeImageLibrary.Instance.ConvertToRGBAF(m_imagePtr);
                    break;
                case ImageConversion.ToRGB16:
                    newImagePtr = FreeImageLibrary.Instance.ConvertToRGB16(m_imagePtr);
                    break;
                case ImageConversion.ToRGBA16:
                    newImagePtr = FreeImageLibrary.Instance.ConvertToRGBA16(m_imagePtr);
                    break;
                default:
                    return false;
            }

            if (newImagePtr == IntPtr.Zero)
                return false;

            FreeImageLibrary.Instance.Unload(m_imagePtr);
            m_imagePtr = newImagePtr;

            return true;
        }

        /// <summary>
        /// Flips the image contents horizontally along the vertical axis, in place.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool FlipHorizontally()
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.FlipHorizontal(m_imagePtr);
        }

        /// <summary>
        /// Flips the image contents vertically along the horizontal axis, in place.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool FlipVertically()
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.FlipVertical(m_imagePtr);
        }

        /// <summary>
        /// Rotates the image by an angle. This allocates a new surface, and if the operation is successful,
        /// the old surface is disposed of.
        /// </summary>
        /// <param name="angle">Angle to rotate, in degrees.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool Rotate(double angle)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            IntPtr newImagePtr = FreeImageLibrary.Instance.Rotate(m_imagePtr, angle);

            if (newImagePtr == IntPtr.Zero)
                return false;

            FreeImageLibrary.Instance.Unload(m_imagePtr);
            m_imagePtr = newImagePtr;

            return true;
        }

        /// <summary>
        /// Applies the alpha value of each pixel to its color components. The alpha value stays unchanged. Only works with 32-bits color depth.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool PreMultiplyAlpha()
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.PreMultiplyWithAlpha(m_imagePtr);
        }

        /// <summary>
        /// Performs gamma correction on a 8-, 24- or 32-bit image.
        /// </summary>
        /// <param name="gamma">Gamma value (greater than zero). A value of 1.0 leaves the image, less darkens, and greater than one lightens.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool AdjustGamma(double gamma)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.AdjustGamma(m_imagePtr, gamma);
        }

        /// <summary>
        /// Adjusts the brightness of a 8-, 24- or 32-bit image by a certain amount.
        /// </summary>
        /// <param name="percentage">A value of zero means no change, less than zero will make the image darker, and greater than zero will make the image brighter.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool AdjustBrightness(double percentage)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.AdjustBrightness(m_imagePtr, percentage);
        }

        /// <summary>
        /// Adjusts the contrast of a 8-, 24- or 32-bit image by a certain amount.
        /// </summary>
        /// <param name="percentage">A value of zero means no change, less than zero will decrease the contrast, and greater than zero will increase the contrast.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool AdjustContrast(double percentage)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.AdjustContrast(m_imagePtr, percentage);
        }

        /// <summary>
        /// Inverts each pixel data.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool Invert()
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.Invert(m_imagePtr);
        }

        /// <summary>
        /// Resizes the image by resampling (or scaling, zooming). This allocates a new surface, and if the operation is successful,
        /// the old surface is disposed of.
        /// </summary>
        /// <param name="width">Destination width.</param>
        /// <param name="height">Destination height.</param>
        /// <param name="filter">Filter algorithm used for sampling.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool Resize(int width, int height, ImageFilter filter)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            IntPtr newImagePtr = FreeImageLibrary.Instance.Rescale(m_imagePtr, width, height, filter);

            if (newImagePtr == IntPtr.Zero)
                return false;

            FreeImageLibrary.Instance.Unload(m_imagePtr);
            m_imagePtr = newImagePtr;

            return true;
        }

        /// <summary>
        /// Swaps two specified colors on a 1-, 4- or 8-bit palletized or a 16-, 24- or 32-bit high color image.
        /// </summary>
        /// <param name="colorToReplace">Color value to find in image to replace.</param>
        /// <param name="colorToReplaceWith">Color value to replace with.</param>
        /// <param name="ignoreAlpha">True if alpha should be ignored or not, meaning if colors in a 32-bit image should be treated as 24-bit.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool SwapColors(RGBAQuad colorToReplace, RGBAQuad colorToReplaceWith, bool ignoreAlpha)
        {
            if (m_imagePtr == IntPtr.Zero)
                return false;

            return FreeImageLibrary.Instance.SwapColors(m_imagePtr, colorToReplace, colorToReplaceWith, ignoreAlpha) > 0;
        }

        /// <summary>
        /// Generates a mipmap chain from the surface. See the <see cref="Compression.Compressor"/> API for more advanced mipmap generation options.
        /// </summary>
        /// <param name="mipChain">List that will contain the mipmap chain.</param>
        /// <param name="filter">Filter used to downsample the image when generating the mipmaps.</param>
        /// <param name="includeFirst">Optionally include the first mip in the list, by default this is true.</param>
        /// <param name="maxLevel">Max mip level to generate, a value that is less than or equal to zero will result in the full mipchain.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool GenerateMipMaps(IList<Surface> mipChain, ImageFilter filter, bool includeFirst = true, int maxLevel = -1)
        {
            if (mipChain == null || m_imagePtr == IntPtr.Zero)
                return false;

            if (includeFirst)
                mipChain.Add(this);

            int width = Width;
            int height = Height;
            int mipCount = ImageHelper.CountMipmaps(width, height, 1);

            //If max level explicitly set, get the minimum since we can't go beyond the # of mips based on width/height
            if (maxLevel > 0)
                mipCount = Math.Min(mipCount, maxLevel);

            for (int i = 1; i < mipCount; i++)
            {
                int mipWidth = width;
                int mipHeight = height;
                ImageHelper.CalculateMipmapLevelDimensions(i, ref mipWidth, ref mipHeight);

                IntPtr mipPtr = FreeImageLibrary.Instance.Rescale(m_imagePtr, mipWidth, mipHeight, filter);
                if (mipPtr != IntPtr.Zero)
                    mipChain.Add(new Surface(mipPtr));
            }

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (!m_isDisposed)
            {
                if (m_imagePtr != IntPtr.Zero)
                {
                    RemoveGCMemoryPressure();

                    FreeImageLibrary.Instance.Unload(m_imagePtr);
                    m_imagePtr = IntPtr.Zero;
                }

                m_isDisposed = true;
            }
        }

        private void AddGCMemoryPressure()
        {
            int estimatedSize = Pitch * Height;
            GC.AddMemoryPressure(estimatedSize);
        }

        private void RemoveGCMemoryPressure()
        {
            int estimatedSize = Pitch * Height;
            GC.RemoveMemoryPressure(estimatedSize);
        }
    }
}
