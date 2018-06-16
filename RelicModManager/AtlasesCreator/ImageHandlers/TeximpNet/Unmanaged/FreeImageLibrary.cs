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
using System.IO;
using System.Runtime.InteropServices;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.Unmanaged
{
    /// <summary>
    /// Manages the lifetime and access to the FreeImage native library.
    /// </summary>
    public sealed class FreeImageLibrary : UnmanagedLibrary
    {
        private static readonly Object s_sync = new Object();
        private bool? m_isLittleEndian;

        /// <summary>
        /// Default name of the 32-bit unmanaged library. Based on runtime implementation the prefix ("lib" on non-windows) and extension (.dll, .so, .dylib) will be appended automatically.
        /// </summary>
        private const String Default32BitName = "FreeImage32";

        /// <summary>
        /// Default name of the 64-bit unmanaged library. Based on runtime implementation the prefix ("lib" on non-windows) and extension (.dll, .so, .dylib) will be appended automatically.
        /// </summary>
        private const String Default64BitName = "FreeImage64";

        private static FreeImageLibrary s_instance;

        private FreeImageIOHandler m_ioHandler;

        /// <summary>
        /// Gets the instance of the FreeImage library. This is thread-safe.
        /// </summary>
        public static FreeImageLibrary Instance
        {
            get
            {
                lock (s_sync)
                {
                    if (s_instance == null)
                        s_instance = CreateInstance();

                    return s_instance;
                }
            }
        }

        /// <summary>
        /// Gets if the OS is little endian. If Big Endian, then surface data is RGBA. If little, then surface data is BGRA.
        /// </summary>
        public bool IsLittleEndian
        {
            get
            {
                if (m_isLittleEndian.HasValue)
                    return m_isLittleEndian.Value;

                return true; //Most often the case... 
            }
        }

        /// <summary>
        /// Gets the default color order / component masks for the red, green, blue, alpha channels.
        /// </summary>
        public ColorOrder ColorOrder
        {
            get
            {
                return new ColorOrder(IsLittleEndian);
            }
        }

        private FreeImageLibrary(String default32BitName, String default64BitName, Type[] unmanagedFunctionDelegateTypes)
            : base(default32BitName, default64BitName, unmanagedFunctionDelegateTypes)
        {
            m_ioHandler = new FreeImageIOHandler(Is64Bit && (GetPlatform() != Platform.Windows));
        }

        private static FreeImageLibrary CreateInstance()
        {
            return new FreeImageLibrary(Default32BitName, Default64BitName, PlatformHelper.GetNestedTypes(typeof(Functions)));
        }

        /// <summary>
        /// Called when the library is loaded.
        /// </summary>
        protected override void OnLibraryLoaded()
        {
            Functions.FreeImage_IsLittleEndian func = GetFunction<Functions.FreeImage_IsLittleEndian>(FunctionNames.FreeImage_IsLittleEndian);
            m_isLittleEndian = func();

            base.OnLibraryLoaded();
        }

        /// <summary>
        /// Called when the library is freed.
        /// </summary>
        protected override void OnLibraryFreed()
        {
            m_isLittleEndian = null;

            base.OnLibraryFreed();
        }

        #region Allocate / Clone / Unload

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bpp"></param>
        /// <returns>Pointer to FreeImage bitmap, or null if the operation was not successful.</returns>
        public IntPtr Allocate(int width, int height, int bpp)
        {
            return Allocate(width, height, bpp, 0, 0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bpp"></param>
        /// <param name="red_mask"></param>
        /// <param name="green_mask"></param>
        /// <param name="blue_mask"></param>
        /// <returns>Pointer to FreeImage bitmap, or null if the operation was not successful.</returns>
        public IntPtr Allocate(int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask)
        {
            LoadIfNotLoaded();

            Functions.FreeImage_Allocate func = GetFunction<Functions.FreeImage_Allocate>(FunctionNames.FreeImage_Allocate);

            return func(width, height, bpp, red_mask, green_mask, blue_mask);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageType"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bpp"></param>
        /// <param name="red_mask"></param>
        /// <param name="green_mask"></param>
        /// <param name="blue_mask"></param>
        /// <returns>Pointer to FreeImage bitmap, or null if the operation was not successful.</returns>
        public IntPtr AllocateT(ImageType imageType, int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask)
        {
            LoadIfNotLoaded();

            Functions.FreeImage_AllocateT func = GetFunction<Functions.FreeImage_AllocateT>(FunctionNames.FreeImage_AllocateT);

            return func(imageType, width, height, bpp, red_mask, green_mask, blue_mask);
        }

        /// <summary>
        /// Clones the FreeImage bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to the FreeImage bitmap.</param>
        /// <returns>Cloned image.</returns>
        public IntPtr Clone(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_Clone func = GetFunction<Functions.FreeImage_Clone>(FunctionNames.FreeImage_Clone);

            return func(bitmap);
        }

        /// <summary>
        /// Frees memory used by the FreeImage bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to the FreeImage bitmap.</param>
        public void Unload(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.FreeImage_Unload func = GetFunction<Functions.FreeImage_Unload>(FunctionNames.FreeImage_Unload);

            func(bitmap);
        }

        /// <summary>
        /// Copies data from the specified rectangle in the source image, and returns it as another bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to the FreeImage bitmap.</param>
        /// <param name="left">Leftmost texel.</param>
        /// <param name="top">Topmost texel.</param>
        /// <param name="right">Rightmost texel.</param>
        /// <param name="bottom">Bottommost texel.</param>
        /// <returns></returns>
        public IntPtr Copy(IntPtr bitmap, int left, int top, int right, int bottom)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_Copy func = GetFunction<Functions.FreeImage_Copy>(FunctionNames.FreeImage_Copy);

            return func(bitmap, left, top, right, bottom);
        }

        /// <summary>
        /// Pastes data from one bitmap into another, starting at the texel coordinate specified. The width and height of the
        /// data is determined by the source bitmap width and height.
        /// </summary>
        /// <param name="dstBitmap">Pointer to the destination FreeImage bitmap.</param>
        /// <param name="srcBitmap">Pointer to the source FreeImage bitmap.</param>
        /// <param name="left">X origin texel.</param>
        /// <param name="top">Y origin texel.</param>
        /// <param name="alpha">Alpha blend factor.</param>
        /// <returns>True if the operation was successful, false if otherwise.</returns>
        public bool Paste(IntPtr dstBitmap, IntPtr srcBitmap, int left, int top, int alpha)
        {
            if (dstBitmap == IntPtr.Zero || srcBitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_Paste func = GetFunction<Functions.FreeImage_Paste>(FunctionNames.FreeImage_Paste);

            return func(dstBitmap, srcBitmap, left, top, alpha);
        }

        #endregion

        #region Load / Save

        /// <summary>
        /// Loads an image from a file. The format is determined automatically.
        /// </summary>
        /// <param name="filename">File containing the image to load from.</param>
        /// <param name="flags">Load flags.</param>
        /// <returns>Pointer to FreeImage bitmap, or null if the operation was not successful.</returns>
        public IntPtr LoadFromFile(String filename, ImageLoadFlags flags = ImageLoadFlags.Default)
        {
            if (String.IsNullOrEmpty(filename))
                return IntPtr.Zero;

            LoadIfNotLoaded();

            IntPtr name = Marshal.StringToHGlobalAnsi(filename);
            Functions.FreeImage_GetFileType getFileTypeFunc = GetFunction<Functions.FreeImage_GetFileType>(FunctionNames.FreeImage_GetFileType);
            Functions.FreeImage_Load loadFunc = GetFunction<Functions.FreeImage_Load>(FunctionNames.FreeImage_Load);

            try
            {
                ImageFormat format = getFileTypeFunc(name, 0);

                if (format == ImageFormat.Unknown)
                    return IntPtr.Zero;

                return loadFunc(format, name, (int)flags);
            }
            finally
            {
                Marshal.FreeHGlobal(name);
            }
        }

        /// <summary>
        /// Loads an image from the stream. The format is determined automatically.
        /// </summary>
        /// <param name="stream">Stream to read data from.</param>
        /// <param name="flags">Load flags.</param>
        /// <returns>Pointer to FreeImage bitmap, or null if the operation was not successful.</returns>
        public unsafe IntPtr LoadFromStream(Stream stream, ImageLoadFlags flags = ImageLoadFlags.Default)
        {
            if (stream == null || !stream.CanRead)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            using (StreamWrapper wrapper = new StreamWrapper(stream))
            {
                Functions.FreeImage_LoadFromHandle loadFunc = GetFunction<Functions.FreeImage_LoadFromHandle>(FunctionNames.FreeImage_LoadFromHandle);
                Functions.FreeImage_GetFileTypeFromHandle getFileTypeFunc = GetFunction<Functions.FreeImage_GetFileTypeFromHandle>(FunctionNames.FreeImage_GetFileTypeFromHandle);

                FreeImageIO io = m_ioHandler.ImageIO;
                IntPtr ioPtr = new IntPtr(&io);

                ImageFormat format = getFileTypeFunc(ioPtr, wrapper.GetHandle(), 0);

                if (format == ImageFormat.Unknown)
                    return IntPtr.Zero;

                return loadFunc(format, ioPtr, wrapper.GetHandle(), (int)flags);
            }
        }

        /// <summary>
        /// Saves the image to a file.
        /// </summary>
        /// <param name="format">Image format to save as.</param>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="filename">File path at which to create a file to save the data at.</param>
        /// <param name="flags">Save flags.</param>
        /// <returns>True if the operation was successfully, false if otherwise.</returns>
        public bool SaveToFile(ImageFormat format, IntPtr bitmap, String filename, ImageSaveFlags flags = ImageSaveFlags.Default)
        {
            if (String.IsNullOrEmpty(filename) || format == ImageFormat.Unknown || bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_Save func = GetFunction<Functions.FreeImage_Save>(FunctionNames.FreeImage_Save);
            IntPtr name = Marshal.StringToHGlobalAnsi(filename);

            try
            {
                return func(format, bitmap, name, (int)flags);
            }
            finally
            {
                Marshal.FreeHGlobal(name);
            }
        }

        /// <summary>
        /// Saves the image to a stream.
        /// </summary>
        /// <param name="format">Image format to save as.</param>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="stream">Stream to write data to.</param>
        /// <param name="flags">Save flags.</param>
        /// <returns>True if the operation was successfully, false if otherwise.</returns>
        public unsafe bool SaveToStream(ImageFormat format, IntPtr bitmap, Stream stream, ImageSaveFlags flags = ImageSaveFlags.Default)
        {
            if (stream == null || !stream.CanWrite || format == ImageFormat.Unknown || bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            using (StreamWrapper wrapper = new StreamWrapper(stream))
            {
                Functions.FreeImage_SaveToHandle func = GetFunction<Functions.FreeImage_SaveToHandle>(FunctionNames.FreeImage_SaveToHandle);

                FreeImageIO io = m_ioHandler.ImageIO;
                return func(format, bitmap, new IntPtr(&io), wrapper.GetHandle(), (int)flags);
            }
        }

        #endregion

        #region Query routines

        /// <summary>
        /// Queries whether the bitmap has pixel data.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>True if the bitmap has pixels, false if not.</returns>
        public bool HasPixels(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_HasPixels func = GetFunction<Functions.FreeImage_HasPixels>(FunctionNames.FreeImage_HasPixels);

            return func(bitmap);
        }

        /// <summary>
        /// Determines the file format based on the filename.
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Image format.</returns>
        public ImageFormat GetFileTypeFromFile(String filename)
        {
            if (String.IsNullOrEmpty(filename))
                return ImageFormat.Unknown;

            LoadIfNotLoaded();

            Functions.FreeImage_GetFileType func = GetFunction<Functions.FreeImage_GetFileType>(FunctionNames.FreeImage_GetFileType);

            IntPtr name = Marshal.StringToHGlobalAnsi(filename);

            try
            {
                return func(name, 0);
            }
            finally
            {
                Marshal.FreeHGlobal(name);
            }
        }

        /// <summary>
        /// Determines the file format based on the contents of the stream.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Image format.</returns>
        public unsafe ImageFormat GetFileTypeFromStream(Stream stream)
        {
            if (stream == null || !stream.CanRead)
                return ImageFormat.Unknown;

            LoadIfNotLoaded();

            using (StreamWrapper wrapper = new StreamWrapper(stream, false))
            {
                Functions.FreeImage_GetFileTypeFromHandle func = GetFunction<Functions.FreeImage_GetFileTypeFromHandle>(FunctionNames.FreeImage_GetFileTypeFromHandle);

                FreeImageIO io = m_ioHandler.ImageIO;
                return func(new IntPtr(&io), wrapper.GetHandle(), 0);
            }
        }

        /// <summary>
        /// Queries the image type from the FreeImage bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Image type.</returns>
        public ImageType GetImageType(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return ImageType.Unknown;

            LoadIfNotLoaded();

            Functions.FreeImage_GetImageType func = GetFunction<Functions.FreeImage_GetImageType>(FunctionNames.FreeImage_GetImageType);

            return func(bitmap);
        }

        /// <summary>
        /// Queries the color model from the FreeImage bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Image color model.</returns>
        public ImageColorType GetImageColorType(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return ImageColorType.RGBA;

            LoadIfNotLoaded();

            Functions.FreeImage_GetColorType func = GetFunction<Functions.FreeImage_GetColorType>(FunctionNames.FreeImage_GetColorType);

            return func(bitmap);
        }

        /// <summary>
        /// Queries the image data from the FreeImage bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Pointer to the image data.</returns>
        public IntPtr GetData(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_GetBits func = GetFunction<Functions.FreeImage_GetBits>(FunctionNames.FreeImage_GetBits);

            return func(bitmap);
        }

        /// <summary>
        /// Queries the palette data from the FreeImage bitmap, if it has one.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Pointer to the palette color array.</returns>
        public IntPtr GetPalette(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_GetPalette func = GetFunction<Functions.FreeImage_GetPalette>(FunctionNames.FreeImage_GetPalette);

            return func(bitmap);
        }

        /// <summary>
        /// Queries the number of colors in the palette array from the FreeImage bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Number of palette entries, or zero if no palette exists.</returns>
        public uint GetPaletteColorCount(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetColorsUsed func = GetFunction<Functions.FreeImage_GetColorsUsed>(FunctionNames.FreeImage_GetColorsUsed);

            return func(bitmap);
        }

        /// <summary>
        /// Gets the scanline from the FreeImage bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="scanline">Row to get the scanline, in range of [0, Image Height)</param>
        /// <returns>Pointer to scanline data.</returns>
        public IntPtr GetScanLine(IntPtr bitmap, int scanline)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_GetScanLine func = GetFunction<Functions.FreeImage_GetScanLine>(FunctionNames.FreeImage_GetScanLine);

            return func(bitmap, scanline);
        }

        /// <summary>
        /// Gets the number of bits per pixel contained in the FreeImage bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Bits per pixel of the image.</returns>
        public int GetBitsPerPixel(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetBPP func = GetFunction<Functions.FreeImage_GetBPP>(FunctionNames.FreeImage_GetBPP);

            return (int)func(bitmap);
        }

        /// <summary>
        /// Gets the width of the FreeImage bitmap, in texels.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Width of the image.</returns>
        public int GetWidth(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetWidth func = GetFunction<Functions.FreeImage_GetWidth>(FunctionNames.FreeImage_GetWidth);

            return (int)func(bitmap);
        }

        /// <summary>
        /// Gets the height of the FreeImage bitmap, in texels.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Height of the image.</returns>
        public int GetHeight(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetHeight func = GetFunction<Functions.FreeImage_GetHeight>(FunctionNames.FreeImage_GetHeight);

            return (int)func(bitmap);
        }

        /// <summary>
        /// Gets the pitch of the bitmap, this is the # of bytes per row, which may or may not have padding.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Pitch</returns>
        public int GetPitch(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetPitch func = GetFunction<Functions.FreeImage_GetPitch>(FunctionNames.FreeImage_GetPitch);

            return (int)func(bitmap);
        }

        /// <summary>
        /// Gets the mask value to isolate the red component of a texel.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Red mask</returns>
        public uint GetRedMask(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetRedMask func = GetFunction<Functions.FreeImage_GetRedMask>(FunctionNames.FreeImage_GetRedMask);

            return func(bitmap);
        }

        /// <summary>
        /// Gets the mask value to isolate the green component of a texel.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Green mask</returns>
        public uint GetGreenMask(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetGreenMask func = GetFunction<Functions.FreeImage_GetGreenMask>(FunctionNames.FreeImage_GetGreenMask);

            return func(bitmap);
        }

        /// <summary>
        /// Gets the mask value to isolate the blue component of a texel.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>Blue mask</returns>
        public uint GetBlueMask(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_GetBlueMask func = GetFunction<Functions.FreeImage_GetBlueMask>(FunctionNames.FreeImage_GetBlueMask);

            return func(bitmap);
        }

        /// <summary>
        /// Determines if the image has any transparency.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>True if the image has transparency, false if otherwise.</returns>
        public bool IsTransparent(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_IsTransparent func = GetFunction<Functions.FreeImage_IsTransparent>(FunctionNames.FreeImage_IsTransparent);

            return func(bitmap);
        }

        #endregion

        #region Conversion routines

        /// <summary>
        /// Converts a bitmap to a 4 bit paletized format.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertTo4Bits(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo4Bits func = GetFunction<Functions.FreeImage_ConvertTo4Bits>(FunctionNames.FreeImage_ConvertTo4Bits);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to a 8 bit paletized format.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertTo8Bits(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo8Bits func = GetFunction<Functions.FreeImage_ConvertTo8Bits>(FunctionNames.FreeImage_ConvertTo8Bits);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to a 16 bit format where 5 bits for red, green, blue.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertTo16Bits555(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo16Bits555 func = GetFunction<Functions.FreeImage_ConvertTo16Bits555>(FunctionNames.FreeImage_ConvertTo16Bits555);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to a 16 bit format where 5 bits for red, 6 bits for green, 5 for blue.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertTo16Bits565(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo16Bits565 func = GetFunction<Functions.FreeImage_ConvertTo16Bits565>(FunctionNames.FreeImage_ConvertTo16Bits565);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to a 24-bit RGBA bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertTo24Bits(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo24Bits func = GetFunction<Functions.FreeImage_ConvertTo24Bits>(FunctionNames.FreeImage_ConvertTo24Bits);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to a 32-bit RGBA bitmap.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertTo32Bits(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertTo32Bits func = GetFunction<Functions.FreeImage_ConvertTo32Bits>(FunctionNames.FreeImage_ConvertTo32Bits);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to a 8-bit paletized greyscale format.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertToGreyscale(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToGreyscale func = GetFunction<Functions.FreeImage_ConvertToGreyscale>(FunctionNames.FreeImage_ConvertToGreyscale);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to 32-bit IEEE float.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertToFloat(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToFloat func = GetFunction<Functions.FreeImage_ConvertToFloat>(FunctionNames.FreeImage_ConvertToFloat);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to RGB format where each component has 32 bits and is a IEEE float.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertToRGBF(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToRGBF func = GetFunction<Functions.FreeImage_ConvertToRGBF>(FunctionNames.FreeImage_ConvertToRGBF);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to RGBA format where each component has 32 bits and is a IEEE float.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertToRGBAF(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToRGBAF func = GetFunction<Functions.FreeImage_ConvertToRGBAF>(FunctionNames.FreeImage_ConvertToRGBAF);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to UINT16 format.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertToUINT16(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToUINT16 func = GetFunction<Functions.FreeImage_ConvertToUINT16>(FunctionNames.FreeImage_ConvertToUINT16);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to RGB color format where each component has 16 bits.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertToRGB16(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToRGB16 func = GetFunction<Functions.FreeImage_ConvertToRGB16>(FunctionNames.FreeImage_ConvertToRGB16);

            return func(bitmap);
        }

        /// <summary>
        /// Converts a bitmap to RGBA color format where each component has 16 bits.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>FreeImage Bitmap</returns>
        public IntPtr ConvertToRGBA16(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToRGBA16 func = GetFunction<Functions.FreeImage_ConvertToRGBA16>(FunctionNames.FreeImage_ConvertToRGBA16);

            return func(bitmap);
        }

        /// <summary>
        /// Creates a FreeImage surface from raw data.
        /// </summary>
        /// <param name="copySource">True to copy the source data, false to hold onto the data pointer.</param>
        /// <param name="data">Image data pointer.</param>
        /// <param name="imageType">Type of bitmap to create.</param>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="pitch">Pitch of the input image data.</param>
        /// <param name="bpp">Bits per pixel of the input data.</param>
        /// <param name="redMask">Red component mask.</param>
        /// <param name="greenMask">Green component mask.</param>
        /// <param name="blueMask">Blue component mask.</param>
        /// <param name="topDown">True if the input image's origin is the upper left, false if lower left.</param>
        /// <returns>FreeImage surface, or null if an error occured.</returns>
        public IntPtr ConvertFromRawBitsEx(bool copySource, IntPtr data, ImageType imageType, int width, int height, int pitch, uint bpp, uint redMask, uint greenMask, uint blueMask, bool topDown)
        {
            if (data == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertFromRawBitsEx func = GetFunction<Functions.FreeImage_ConvertFromRawBitsEx>(FunctionNames.FreeImage_ConvertFromRawBitsEx);

            return func(copySource, data, imageType, width, height, pitch, bpp, redMask, greenMask, blueMask, topDown);
        }

        /// <summary>
        /// Converts an image of any stype to a standard 8-bit greyscale image.
        /// </summary>
        /// <param name="src">Source FreeImage object.</param>
        /// <param name="scaleLinearly">True if the image data should be scaled linearly, false if not.</param>
        /// <returns>FreeImage surface containing converted image.</returns>
        public IntPtr ConvertToStandardType(IntPtr src, bool scaleLinearly)
        {
            if (src == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToStandardType func = GetFunction<Functions.FreeImage_ConvertToStandardType>(FunctionNames.FreeImage_ConvertToStandardType);

            return func(src, scaleLinearly);
        }

        /// <summary>
        /// Converts a FreeImage surface to another image type, optionally scaling the data.
        /// </summary>
        /// <param name="src">Source FreeImage object.</param>
        /// <param name="dstType">Type of image to convert to.</param>
        /// <param name="scaleLinearly">True if the image data should be scaled linearly, false if not.</param>
        /// <returns>FreeImage surface containing converted image.</returns>
        public IntPtr ConvertToType(IntPtr src, ImageType dstType, bool scaleLinearly)
        {
            if (src == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_ConvertToType func = GetFunction<Functions.FreeImage_ConvertToType>(FunctionNames.FreeImage_ConvertToType);

            return func(src, dstType, scaleLinearly);
        }

        #endregion

        #region Image manipulation

        /// <summary>
        /// Flips the image contents horizontally along the vertical axis, in place.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool FlipHorizontal(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_FlipHorizontal func = GetFunction<Functions.FreeImage_FlipHorizontal>(FunctionNames.FreeImage_FlipHorizontal);

            return func(bitmap);
        }

        /// <summary>
        /// Flips the image contents vertically along the horizontal axis, in place.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool FlipVertical(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_FlipVertical func = GetFunction<Functions.FreeImage_FlipVertical>(FunctionNames.FreeImage_FlipVertical);

            return func(bitmap);
        }

        /// <summary>
        /// Resizes the image by resampling (or scaling, zooming). This allocates a new surface with the new scale.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="dst_width">Destination width.</param>
        /// <param name="dst_height">Destination height.</param>
        /// <param name="filter">Filter algorithm used for sampling.</param>
        /// <returns>Rescaled FreeImage bitmap.</returns>
        public IntPtr Rescale(IntPtr bitmap, int dst_width, int dst_height, ImageFilter filter)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            LoadIfNotLoaded();

            Functions.FreeImage_Rescale func = GetFunction<Functions.FreeImage_Rescale>(FunctionNames.FreeImage_Rescale);

            return func(bitmap, dst_width, dst_height, filter);
        }

        /// <summary>
        /// Applies the alpha value of each pixel to its color components. The alpha value stays unchanged. Only works with 32-bits color depth.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool PreMultiplyWithAlpha(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_PreMultiplyWithAlpha func = GetFunction<Functions.FreeImage_PreMultiplyWithAlpha>(FunctionNames.FreeImage_PreMultiplyWithAlpha);

            return func(bitmap);
        }

        /// <summary>
        /// Performs gamma correction on a 8-, 24- or 32-bit image.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="gamma">Gamma value (greater than zero). A value of 1.0 leaves the image, less darkens, and greater than one lightens.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool AdjustGamma(IntPtr bitmap, double gamma)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_AdjustGamma func = GetFunction<Functions.FreeImage_AdjustGamma>(FunctionNames.FreeImage_AdjustGamma);

            return func(bitmap, gamma);
        }

        /// <summary>
        /// Adjusts the brightness of a 8-, 24- or 32-bit image by a certain amount.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="percentage">A value of zero means no change, less than zero will make the image darker, and greater than zero will make the image brighter.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool AdjustBrightness(IntPtr bitmap, double percentage)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_AdjustBrightness func = GetFunction<Functions.FreeImage_AdjustBrightness>(FunctionNames.FreeImage_AdjustBrightness);

            return func(bitmap, percentage);
        }

        /// <summary>
        /// Adjusts the contrast of a 8-, 24- or 32-bit image by a certain amount.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="percentage">A value of zero means no change, less than zero will decrease the contrast, and greater than zero will increase the contrast.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool AdjustContrast(IntPtr bitmap, double percentage)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_AdjustContrast func = GetFunction<Functions.FreeImage_AdjustContrast>(FunctionNames.FreeImage_AdjustContrast);

            return func(bitmap, percentage);
        }

        /// <summary>
        /// Inverts each pixel data.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool Invert(IntPtr bitmap)
        {
            if (bitmap == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.FreeImage_Invert func = GetFunction<Functions.FreeImage_Invert>(FunctionNames.FreeImage_Invert);

            return func(bitmap);
        }

        /// <summary>
        /// Swaps two specified colors on a 1-, 4- or 8-bit palletized or a 16-, 24- or 32-bit high color image.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="colorToReplace">Color value to find in image to replace.</param>
        /// <param name="colorToReplaceWith">Color value to replace with.</param>
        /// <param name="ignoreAlpha">True if alpha should be ignored or not, meaning if colors in a 32-bit image should be treated as 24-bit.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public unsafe int SwapColors(IntPtr bitmap, RGBAQuad colorToReplace, RGBAQuad colorToReplaceWith, bool ignoreAlpha)
        {
            if (bitmap == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.FreeImage_SwapColors func = GetFunction<Functions.FreeImage_SwapColors>(FunctionNames.FreeImage_SwapColors);

            //BGRA in little endian
            if (IsLittleEndian)
            {
                //Swap RGBA to BGRA
                byte swap = colorToReplace.B;
                colorToReplace.B = colorToReplace.R;
                colorToReplace.R = swap;

                swap = colorToReplaceWith.B;
                colorToReplaceWith.B = colorToReplaceWith.R;
                colorToReplaceWith.R = swap;
            }

            return (int)func(bitmap, new IntPtr(&colorToReplace), new IntPtr(&colorToReplaceWith), ignoreAlpha);
        }

        /// <summary>
        /// Rotates the image by an angle. This allocates a new surface, and if the operation is successful,
        /// the old surface is disposed of.
        /// </summary>
        /// <param name="bitmap">Pointer to FreeImage bitmap.</param>
        /// <param name="angle">Angle to rotate, in degrees.</param>
        public IntPtr Rotate(IntPtr bitmap, double angle)
        {
            if (bitmap == IntPtr.Zero)
                return IntPtr.Zero;

            Functions.FreeImage_Rotate func = GetFunction<Functions.FreeImage_Rotate>(FunctionNames.FreeImage_Rotate);

            return func(bitmap, angle, IntPtr.Zero);
        }

        #endregion

        #region Versioning

        /// <summary>
        /// Gets the version of the native DLL that is loaded.
        /// </summary>
        /// <returns>Version string</returns>
        public String GetVersion()
        {
            LoadIfNotLoaded();

            Functions.FreeImage_GetVersion func = GetFunction<Functions.FreeImage_GetVersion>(FunctionNames.FreeImage_GetVersion);

            IntPtr ptr = func();

            if (ptr == IntPtr.Zero)
                return String.Empty;

            return Marshal.PtrToStringAnsi(ptr);
        }

        /// <summary>
        /// Gets the FreeImage copyright message.
        /// </summary>
        /// <returns>Legal copyright string.</returns>
        public String GetCopyrightMessage()
        {
            LoadIfNotLoaded();

            Functions.FreeImage_GetCopyrightMessage func = GetFunction<Functions.FreeImage_GetCopyrightMessage>(FunctionNames.FreeImage_GetCopyrightMessage);

            IntPtr ptr = func();

            if (ptr == IntPtr.Zero)
                return String.Empty;

            return Marshal.PtrToStringAnsi(ptr);
        }

        #endregion

        #region Function names

        internal static class FunctionNames
        {

            #region Allocate / Clone / Unload routines

            public const String FreeImage_Allocate = "FreeImage_Allocate";
            public const String FreeImage_AllocateT = "FreeImage_AllocateT";
            public const String FreeImage_Clone = "FreeImage_Clone";
            public const String FreeImage_Unload = "FreeImage_Unload";

            public const String FreeImage_Copy = "FreeImage_Copy";
            public const String FreeImage_Paste = "FreeImage_Paste";

            #endregion

            #region Load / Save routines

            public const String FreeImage_Load = "FreeImage_Load";
            public const String FreeImage_LoadFromHandle = "FreeImage_LoadFromHandle";

            public const String FreeImage_Save = "FreeImage_Save";
            public const String FreeImage_SaveToHandle = "FreeImage_SaveToHandle";

            #endregion

            #region Query routines

            public const String FreeImage_IsLittleEndian = "FreeImage_IsLittleEndian";
            public const String FreeImage_HasPixels = "FreeImage_HasPixels";
            public const String FreeImage_GetFileType = "FreeImage_GetFileType";
            public const String FreeImage_GetFileTypeFromHandle = "FreeImage_GetFileTypeFromHandle";
            public const String FreeImage_GetImageType = "FreeImage_GetImageType";
            public const String FreeImage_GetBits = "FreeImage_GetBits";
            public const String FreeImage_GetScanLine = "FreeImage_GetScanLine";
            public const String FreeImage_GetBPP = "FreeImage_GetBPP";
            public const String FreeImage_GetWidth = "FreeImage_GetWidth";
            public const String FreeImage_GetHeight = "FreeImage_GetHeight";
            public const String FreeImage_GetPitch = "FreeImage_GetPitch";

            public const String FreeImage_GetRedMask = "FreeImage_GetRedMask";
            public const String FreeImage_GetGreenMask = "FreeImage_GetGreenMask";
            public const String FreeImage_GetBlueMask = "FreeImage_GetBlueMask";
            public const String FreeImage_IsTransparent = "FreeImage_IsTransparent";
            public const String FreeImage_GetColorType = "FreeImage_GetColorType";
            public const String FreeImage_GetPalette = "FreeImage_GetPalette";
            public const String FreeImage_GetColorsUsed = "FreeImage_GetColorsUsed";

            #endregion

            #region Conversion routines

            public const String FreeImage_ConvertFromRawBitsEx = "FreeImage_ConvertFromRawBitsEx";
            public const String FreeImage_ConvertToStandardType = "FreeImage_ConvertToStandardType";
            public const String FreeImage_ConvertToType = "FreeImage_ConvertToType";
            public const String FreeImage_ConvertTo4Bits = "FreeImage_ConvertTo4Bits";
            public const String FreeImage_ConvertTo8Bits = "FreeImage_ConvertTo8Bits";
            public const String FreeImage_ConvertToGreyscale = "FreeImage_ConvertToGreyscale";
            public const String FreeImage_ConvertTo16Bits555 = "FreeImage_ConvertTo16Bits555";
            public const String FreeImage_ConvertTo16Bits565 = "FreeImage_ConvertTo16Bits565";
            public const String FreeImage_ConvertTo24Bits = "FreeImage_ConvertTo24Bits";
            public const String FreeImage_ConvertTo32Bits = "FreeImage_ConvertTo32Bits";
            public const String FreeImage_ConvertToFloat = "FreeImage_ConvertToFloat";
            public const String FreeImage_ConvertToRGBF = "FreeImage_ConvertToRGBF";
            public const String FreeImage_ConvertToRGBAF = "FreeImage_ConvertToRGBAF";
            public const String FreeImage_ConvertToUINT16 = "FreeImage_ConvertToUINT16";
            public const String FreeImage_ConvertToRGB16 = "FreeImage_ConvertToRGB16";
            public const String FreeImage_ConvertToRGBA16 = "FreeImage_ConvertToRGBA16";

            #endregion

            #region Image manipulation

            public const String FreeImage_FlipHorizontal = "FreeImage_FlipHorizontal";
            public const String FreeImage_FlipVertical = "FreeImage_FlipVertical";
            public const String FreeImage_Rescale = "FreeImage_Rescale";
            public const String FreeImage_PreMultiplyWithAlpha = "FreeImage_PreMultiplyWithAlpha";
            public const String FreeImage_AdjustGamma = "FreeImage_AdjustGamma";
            public const String FreeImage_AdjustBrightness = "FreeImage_AdjustBrightness";
            public const String FreeImage_AdjustContrast = "FreeImage_AdjustContrast";
            public const String FreeImage_Invert = "FreeImage_Invert";
            public const String FreeImage_SwapColors = "FreeImage_SwapColors";
            public const String FreeImage_Rotate = "FreeImage_Rotate";

            #endregion

            #region Versioning

            public const String FreeImage_GetVersion = "FreeImage_GetVersion";
            public const String FreeImage_GetCopyrightMessage = "FreeImage_GetCopyrightMessage";

            #endregion
        }

        #endregion

        #region Function delegates

        internal static class Functions
        {
            #region Allocate / Clone / Unload routines

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Allocate)]
            public delegate IntPtr FreeImage_Allocate(int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_AllocateT)]
            public delegate IntPtr FreeImage_AllocateT(ImageType imageType, int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Clone)]
            public delegate IntPtr FreeImage_Clone(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Unload)]
            public delegate void FreeImage_Unload(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Copy)]
            public delegate IntPtr FreeImage_Copy(IntPtr bitmap, int left, int top, int right, int bottom);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Paste)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_Paste(IntPtr dstBitmap, IntPtr srcBitmap, int left, int top, int alpha);

            #endregion

            #region Load / Save routines

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Load)]
            public delegate IntPtr FreeImage_Load(ImageFormat format, IntPtr filename, int flags);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_LoadFromHandle)]
            public delegate IntPtr FreeImage_LoadFromHandle(ImageFormat format, IntPtr io, IntPtr ioHandle, int flags);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Save)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_Save(ImageFormat format, IntPtr bitmap, IntPtr filename, int flags);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_SaveToHandle)]
            public delegate bool FreeImage_SaveToHandle(ImageFormat format, IntPtr bitmap, IntPtr io, IntPtr ioHandle, int flags);

            #endregion

            #region Query routines

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_IsLittleEndian)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_IsLittleEndian();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_HasPixels)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_HasPixels(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetFileType)]
            public delegate ImageFormat FreeImage_GetFileType(IntPtr fileName, int size);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetFileTypeFromHandle)]
            public delegate ImageFormat FreeImage_GetFileTypeFromHandle(IntPtr io, IntPtr ioHandle, int size);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetImageType)]
            public delegate ImageType FreeImage_GetImageType(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetBits)]
            public delegate IntPtr FreeImage_GetBits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetScanLine)]
            public delegate IntPtr FreeImage_GetScanLine(IntPtr bitmp, int scanline);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetBPP)]
            public delegate uint FreeImage_GetBPP(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetWidth)]
            public delegate uint FreeImage_GetWidth(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetHeight)]
            public delegate uint FreeImage_GetHeight(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetPitch)]
            public delegate uint FreeImage_GetPitch(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetRedMask)]
            public delegate uint FreeImage_GetRedMask(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetGreenMask)]
            public delegate uint FreeImage_GetGreenMask(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetBlueMask)]
            public delegate uint FreeImage_GetBlueMask(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_IsTransparent)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_IsTransparent(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetColorType)]
            public delegate ImageColorType FreeImage_GetColorType(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetPalette)]
            public delegate IntPtr FreeImage_GetPalette(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetColorsUsed)]
            public delegate uint FreeImage_GetColorsUsed(IntPtr bitmap);

            #endregion

            #region Conversion routines

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo4Bits)]
            public delegate IntPtr FreeImage_ConvertTo4Bits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo8Bits)]
            public delegate IntPtr FreeImage_ConvertTo8Bits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo16Bits555)]
            public delegate IntPtr FreeImage_ConvertTo16Bits555(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo16Bits565)]
            public delegate IntPtr FreeImage_ConvertTo16Bits565(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo24Bits)]
            public delegate IntPtr FreeImage_ConvertTo24Bits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertTo32Bits)]
            public delegate IntPtr FreeImage_ConvertTo32Bits(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToGreyscale)]
            public delegate IntPtr FreeImage_ConvertToGreyscale(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToFloat)]
            public delegate IntPtr FreeImage_ConvertToFloat(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToRGBF)]
            public delegate IntPtr FreeImage_ConvertToRGBF(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToRGBAF)]
            public delegate IntPtr FreeImage_ConvertToRGBAF(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToUINT16)]
            public delegate IntPtr FreeImage_ConvertToUINT16(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToRGB16)]
            public delegate IntPtr FreeImage_ConvertToRGB16(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToRGBA16)]
            public delegate IntPtr FreeImage_ConvertToRGBA16(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToStandardType)]
            public delegate IntPtr FreeImage_ConvertToStandardType(IntPtr src, [MarshalAs(UnmanagedType.Bool)] bool scaleLinearly);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertToType)]
            public delegate IntPtr FreeImage_ConvertToType(IntPtr src, ImageType dstType, [MarshalAs(UnmanagedType.Bool)] bool scaleLinearly);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_ConvertFromRawBitsEx)]
            public delegate IntPtr FreeImage_ConvertFromRawBitsEx([In, MarshalAs(UnmanagedType.Bool)] bool copySource, IntPtr data, ImageType type, int width, int height, int pitch, uint bpp, uint redMask, uint greenMask, uint blueMask, [In, MarshalAs(UnmanagedType.Bool)] bool topdown);

            #endregion

            #region Image manipulation

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_FlipHorizontal)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_FlipHorizontal(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_FlipVertical)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_FlipVertical(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Rescale)]
            public delegate IntPtr FreeImage_Rescale(IntPtr bitmap, int ds_width, int dst_height, ImageFilter filter);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_PreMultiplyWithAlpha)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_PreMultiplyWithAlpha(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_AdjustGamma)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_AdjustGamma(IntPtr bitmap, double gamma);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_AdjustBrightness)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_AdjustBrightness(IntPtr bitmap, double percentage);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_AdjustContrast)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_AdjustContrast(IntPtr bitmap, double percentage);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Invert)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public delegate bool FreeImage_Invert(IntPtr bitmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_SwapColors)]
            public delegate uint FreeImage_SwapColors(IntPtr bitmap, IntPtr rgbaToReplace, IntPtr rgbaToReplaceWith, [MarshalAs(UnmanagedType.Bool)] bool ignoreAlpha);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_Rotate)]
            public delegate IntPtr FreeImage_Rotate(IntPtr bitmap, double angle, IntPtr fillColor);

            #endregion

            #region Versioning

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetVersion)]
            public delegate IntPtr FreeImage_GetVersion();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.FreeImage_GetCopyrightMessage)]
            public delegate IntPtr FreeImage_GetCopyrightMessage();

            #endregion
        }

        #endregion

        #region FreeImageIOWrapper

        [StructLayout(LayoutKind.Sequential)]
        internal struct FreeImageIO
        {
            public IntPtr ReadProc;
            public IntPtr WriteProc;
            public IntPtr SeekProc;
            public IntPtr TellProc;
        }

        internal sealed class StreamWrapper : IDisposable
        {
            private Stream m_stream;
            private byte[] m_tempBuffer;
            private IntPtr m_tempBufferPtr;
            private GCHandle m_gcHandle;
            private bool m_isDisposed;

            public bool IsDisposed
            {
                get
                {
                    return m_isDisposed;
                }
            }

            public StreamWrapper(Stream str) : this(str, true) { }

            public StreamWrapper(Stream str, bool allocateTempBuffer)
            {
                m_stream = str;

                if (allocateTempBuffer)
                {
                    m_tempBuffer = new byte[8192];
                    m_tempBufferPtr = MemoryHelper.PinObject(m_tempBuffer);
                }

                m_gcHandle = new GCHandle();
                m_isDisposed = false;
            }

            public IntPtr GetHandle()
            {
                if (m_gcHandle.IsAllocated)
                    return GCHandle.ToIntPtr(m_gcHandle);

                m_gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
                return GCHandle.ToIntPtr(m_gcHandle);
            }

            ~StreamWrapper()
            {
                Dispose(false);
            }

            public uint Read(IntPtr buffer, uint size, uint count)
            {
                EnsureCapacity(size);

                uint readCount = 0;

                int read;

                while (readCount < count)
                {
                    read = m_stream.Read(m_tempBuffer, 0, (int)size);
                    if (read != (int)size)
                    {
                        m_stream.Seek(-read, SeekOrigin.Current);
                        break;
                    }

                    MemoryHelper.CopyMemory(buffer, m_tempBufferPtr, read);
                    buffer = MemoryHelper.AddIntPtr(buffer, read);

                    readCount++;
                }

                return readCount;
            }

            public uint Write(IntPtr buffer, uint size, uint count)
            {
                EnsureCapacity(size);

                uint writeCount = 0;

                while (writeCount < count)
                {
                    MemoryHelper.CopyMemory(m_tempBufferPtr, buffer, (int)size);
                    buffer = MemoryHelper.AddIntPtr(buffer, (int)size);

                    try
                    {
                        m_stream.Write(m_tempBuffer, 0, (int)size);
                    }
                    catch
                    {
                        return writeCount;
                    }

                    writeCount++;
                }

                return writeCount;
            }

            public void Seek(long offset, int origin)
            {
                m_stream.Seek(offset, (SeekOrigin)origin);
            }

            public long Tell()
            {
                return m_stream.Position;
            }

            public void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            private void Dispose(bool isDisposing)
            {
                if (!m_isDisposed)
                {
                    if (isDisposing)
                    {
                        if (m_tempBufferPtr != IntPtr.Zero)
                        {
                            if (m_tempBuffer != null)
                                MemoryHelper.UnpinObject(m_tempBuffer);

                            m_tempBuffer = null;
                            m_tempBufferPtr = IntPtr.Zero;
                        }
                    }

                    m_gcHandle.Free();

                    m_isDisposed = true;
                }
            }

            private void EnsureCapacity(uint size)
            {
                if (m_tempBuffer == null)
                {
                    m_tempBuffer = new byte[size];
                    m_tempBufferPtr = MemoryHelper.PinObject(m_tempBuffer);
                }
                else if (m_tempBuffer.Length < size)
                {
                    MemoryHelper.UnpinObject(m_tempBuffer);

                    m_tempBuffer = new byte[size];
                    m_tempBufferPtr = MemoryHelper.PinObject(m_tempBuffer);
                }
            }
        }

        internal sealed class FreeImageIOHandler
        {
            #region ImageIO Functions

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate uint FreeImageIO_ReadProc(IntPtr buffer, uint size, uint count, IntPtr ioHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate uint FreeImageIO_WriteProc(IntPtr buffer, uint size, uint count, IntPtr ioHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int FreeImageIO_SeekProc32(IntPtr ioHandle, int offset, int origin);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int FreeImageIO_SeekProc64(IntPtr ioHandle, long offset, int origin);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int FreeImageIO_TellProc32(IntPtr ioHandle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate long FreeImageIO_TellProc64(IntPtr ioHandle);

            #endregion

            private FreeImageIO_ReadProc m_readProc;
            private FreeImageIO_WriteProc m_writeProc;
            private Delegate m_seekProc;
            private Delegate m_tellProc;

            private FreeImageIO m_imageIO;

            public FreeImageIO ImageIO
            {
                get
                {
                    return m_imageIO;
                }
            }

            public FreeImageIOHandler(bool isLong64Bits)
            {
                m_readProc = ReadProc;
                m_writeProc = WriteProc;
                m_seekProc = (isLong64Bits) ? (Delegate)new FreeImageIO_SeekProc64(SeekProc64) : (Delegate)new FreeImageIO_SeekProc32(SeekProc32);
                m_tellProc = (isLong64Bits) ? (Delegate)new FreeImageIO_TellProc64(TellProc64) : (Delegate)new FreeImageIO_TellProc32(TellProc32);

                m_imageIO.ReadProc = PlatformHelper.GetFunctionPointerForDelegate(m_readProc);
                m_imageIO.WriteProc = PlatformHelper.GetFunctionPointerForDelegate(m_writeProc);
                m_imageIO.SeekProc = PlatformHelper.GetFunctionPointerForDelegate(m_seekProc);
                m_imageIO.TellProc = PlatformHelper.GetFunctionPointerForDelegate(m_tellProc);
            }

            private unsafe uint ReadProc(IntPtr buffer, uint size, uint count, IntPtr ioHandle)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                return wrapper.Read(buffer, size, count);
            }

            private uint WriteProc(IntPtr buffer, uint size, uint count, IntPtr ioHandle)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                return wrapper.Write(buffer, size, count);
            }

            private int SeekProc32(IntPtr ioHandle, int offset, int origin)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                wrapper.Seek((long)offset, origin);

                return 0;
            }

            private int SeekProc64(IntPtr ioHandle, long offset, int origin)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                wrapper.Seek(offset, origin);

                return 0;
            }

            private int TellProc32(IntPtr ioHandle)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                return (int)wrapper.Tell();
            }

            private long TellProc64(IntPtr ioHandle)
            {
                StreamWrapper wrapper = GCHandle.FromIntPtr(ioHandle).Target as StreamWrapper;

                return wrapper.Tell();
            }
        }

        #endregion
    }
}
