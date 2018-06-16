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

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet
{
    /// <summary>
    /// Collection of helper methods for images.
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Gets the number of mipmaps that should be in the chain where the first image has the specified width/height/depth.
        /// </summary>
        /// <param name="width">Width of the first image in the mipmap chain.</param>
        /// <param name="height">Height of the first image in the mipmap chain.</param>
        /// <param name="depth">Depth of the first image in the mipmap chain.</param>
        /// <returns>Number of mipmaps that can be generated for the image.</returns>
        public static int CountMipmaps(int width, int height, int depth)
        {
            int mipmap = 0;

            while (width != 1 || height != 1 || depth != 1)
            {
                width = Math.Max(1, width / 2);
                height = Math.Max(1, height / 2);
                depth = Math.Max(1, depth / 2);
                mipmap++;
            }

            return mipmap + 1;
        }

        /// <summary>
        /// Calculates the mipmap level dimension given the level and the first level's dimensions.
        /// </summary>
        /// <param name="mipLevel">Mip map level to calculate for.</param>
        /// <param name="width">Initially the first level's width, holds the width of the mip level after function returns.</param>
        /// <param name="height">Initially the first level's height, holds the height of the mip level after function returns.</param>
        public static void CalculateMipmapLevelDimensions(int mipLevel, ref int width, ref int height)
        {
            width = Math.Max(1, width >> mipLevel);
            height = Math.Max(1, height >> mipLevel);
        }

        /// <summary>
        /// Calculates the mipmap level dimension given the level and the first level's dimensions.
        /// </summary>
        /// <param name="mipLevel">Mip map level to calculate for.</param>
        /// <param name="width">Initially the first level's width, holds the width of the mip level after function returns.</param>
        /// <param name="height">Initially the first level's height, holds the height of the mip level after function returns.</param>
        /// <param name="depth">Initially the first level's depth, holds the depth of the mip level after function returns.</param>
        public static void CalculateMipmapLevelDimensions(int mipLevel, ref int width, ref int height, ref int depth)
        {
            width = Math.Max(1, width >> mipLevel);
            height = Math.Max(1, height >> mipLevel);
            depth = Math.Max(1, depth >> mipLevel);
        }

        /// <summary>
        /// Gets the previous power of two value.
        /// </summary>
        /// <param name="v">Previous value.</param>
        /// <returns>Previous power of two.</returns>
        public static int PreviousPowerOfTwo(int v)
        {
            return NextPowerOfTwo(v + 1) / 2;
        }

        /// <summary>
        /// Gets the nearest power of two value.
        /// </summary>
        /// <param name="v">Starting value.</param>
        /// <returns>Nearest power of two.</returns>
        public static int NearestPowerOfTwo(int v)
        {
            int np2 = NextPowerOfTwo(v);
            int pp2 = PreviousPowerOfTwo(v);

            if (np2 - v <= v - pp2)
                return np2;
            else
                return pp2;
        }

        /// <summary>
        /// Get the next power of two value.
        /// </summary>
        /// <param name="v">Starting value.</param>
        /// <returns>Next power of two.</returns>
        public static int NextPowerOfTwo(int v)
        {
            int p = 1;
            while (v > p)
            {
                p += p;
            }
            return p;
        }

        /// <summary>
        /// Computes pitch information about an image given its DXGI format and uncompressed dimensions.
        /// </summary>
        /// <param name="format">Format of the image data.</param>
        /// <param name="width">Uncompressed width, in texels.</param>
        /// <param name="height">Uncompressed height, in texels.</param>
        /// <param name="rowPitch">Total # of bytes per scanline.</param>
        /// <param name="slicePitch">Total # of bytes per slice (if 3D texture).</param>
        /// <param name="widthCount">Compressed width, if the format is a compressed image format, otherwise the given width.</param>
        /// <param name="heightCount">Compressed height, if the format is a compressed image format, otherwise the given height.</param>
        /// <param name="legacyDword">True if need to use workaround computation for some incorrectly created DDS files based on legacy DirectDraw assumptions about pitch alignment.</param>
        public static void ComputePitch(DDS.DXGIFormat format, int width, int height, out int rowPitch, out int slicePitch, out int widthCount, out int heightCount, bool legacyDword = false)
        {
            int bytesPerPixel;
            ComputePitch(format, width, height, out rowPitch, out slicePitch, out widthCount, out heightCount, out bytesPerPixel, legacyDword);
        }

        /// <summary>
        /// Computes pitch information about an image given its DXGI format and uncompressed dimensions.
        /// </summary>
        /// <param name="format">Format of the image data.</param>
        /// <param name="width">Uncompressed width, in texels.</param>
        /// <param name="height">Uncompressed height, in texels.</param>
        /// <param name="rowPitch">Total # of bytes per scanline.</param>
        /// <param name="slicePitch">Total # of bytes per slice (if 3D texture).</param>
        /// <param name="widthCount">Compressed width, if the format is a compressed image format, otherwise the given width.</param>
        /// <param name="heightCount">Compressed height, if the format is a compressed image format, otherwise the given height.</param>
        /// <param name="bytesPerPixel">Gets the size of the format.</param>
        /// <param name="legacyDword">True if need to use workaround computation for some incorrectly created DDS files based on legacy DirectDraw assumptions about pitch alignment.</param>
        public static void ComputePitch(DDS.DXGIFormat format, int width, int height, out int rowPitch, out int slicePitch, out int widthCount, out int heightCount, out int bytesPerPixel, bool legacyDword = false)
        {
            widthCount = width;
            heightCount = height;

            if (DDS.FormatConverter.IsCompressed(format))
            {
                int blockSize = DDS.FormatConverter.GetCompressedBlockSize(format);

                widthCount = Math.Max(1, (width + 3) / 4);
                heightCount = Math.Max(1, (height + 3) / 4);

                rowPitch = widthCount * blockSize;
                slicePitch = rowPitch * heightCount;

                bytesPerPixel = blockSize;
            }
            else if (DDS.FormatConverter.IsPacked(format))
            {
                rowPitch = ((width + 1) >> 1) * 4;
                slicePitch = rowPitch * height;

                int bitsPerPixel = DDS.FormatConverter.GetBitsPerPixel(format);
                bytesPerPixel = Math.Max(1, bitsPerPixel / 8);
            }
            else
            {
                int bitsPerPixel = DDS.FormatConverter.GetBitsPerPixel(format);
                bytesPerPixel = Math.Max(1, bitsPerPixel / 8);

                if (legacyDword)
                {
                    //Allow for old DDS files that based pitch on certain assumptions
                    rowPitch = ((width * bitsPerPixel + 31) / 32) * sizeof(int);
                    slicePitch = rowPitch * height;
                }
                else
                {
                    rowPitch = (width * bitsPerPixel + 7) / 8;
                    slicePitch = rowPitch * height;
                }
            }
        }

        /// <summary>
        /// Copies image data from one memory location to another. This doesn't validate any data, so use at your own risk.
        /// </summary>
        /// <param name="dstPtr">Destination memory location.</param>
        /// <param name="dstRowPitch">Destination scanline stride, the image data + padding.</param>
        /// <param name="dstSlicePitch">Destination slice stride, the entire image + padding, if a 3D image that is comprised of multiple slices.</param>
        /// <param name="srcPtr">Source memory location.</param>
        /// <param name="srcRowPitch">Source scanline stride, the image data + padding.</param>
        /// <param name="srcSlicePitch">Source slice stride, the entire image + padding, if a 3D image that is comprised of multiple slices.</param>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="depth">Depth of the image, in texels.</param>
        public static unsafe void CopyImageData(IntPtr dstPtr, int dstRowPitch, int dstSlicePitch, IntPtr srcPtr, int srcRowPitch, int srcSlicePitch, int width, int height, int depth)
        {
            //Amount to copy per scanline
            int strideToCopy = Math.Min(dstRowPitch, srcRowPitch);

            //Iterate for each depth
            for (int slice = 0; slice < depth; slice++)
            {
                //Start with a pointer that points to the start of the slice
                IntPtr sPtr = srcPtr;
                IntPtr dPtr = dstPtr;

                //And iterate + copy each line per the height of the image
                for (int row = 0; row < height; row++)
                {
                    MemoryHelper.CopyMemory(dstPtr, sPtr, strideToCopy);

                    //Advance the temporary pointers to the next scanline
                    sPtr = MemoryHelper.AddIntPtr(sPtr, srcRowPitch);
                    dPtr = MemoryHelper.AddIntPtr(dPtr, dstRowPitch);
                }

                //Advance the pointers by their slice pitches to get to the next image
                srcPtr = MemoryHelper.AddIntPtr(srcPtr, srcSlicePitch);
                dstPtr = MemoryHelper.AddIntPtr(dstPtr, dstSlicePitch);
            }
        }

        /// <summary>
        /// Copies 32-bit RGBA/BGRA image data from one memory location to another. Optionally the data's first and third components can be swizzled to convert 
        /// RGBA->BGRA or BGRA->RGBA data. This doesn't validate any data, so use at your own risk.
        /// </summary>
        /// <param name="dstPtr">Destination memory location.</param>
        /// <param name="dstRowPitch">Destination scanline stride, the image data + padding.</param>
        /// <param name="dstSlicePitch">Destination slice stride, the entire image + padding, if a 3D image that is comprised of multiple slices.</param>
        /// <param name="srcPtr">Source memory location.</param>
        /// <param name="srcRowPitch">Source scanline stride, the image data + padding.</param>
        /// <param name="srcSlicePitch">Source slice stride, the entire image + padding, if a 3D image that is comprised of multiple slices.</param>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="depth">Depth of the image, in texels.</param>
        /// <param name="swizzle">Optionally swizzle first and third components.</param>
        public static unsafe void CopyColorImageData(IntPtr dstPtr, int dstRowPitch, int dstSlicePitch, IntPtr srcPtr, int srcRowPitch, int srcSlicePitch, int width, int height, int depth, bool swizzle = false)
        {
            //Amount to copy per scanline
            int strideToCopy = Math.Min(dstRowPitch, srcRowPitch);

            //Iterate for each depth
            for (int slice = 0; slice < depth; slice++)
            {
                //Start with a pointer that points to the start of the slice
                IntPtr sPtr = srcPtr;
                IntPtr dPtr = dstPtr;

                //And iterate + copy each line per the height of the image
                for (int row = 0; row < height; row++)
                {
                    CopyRGBALine(dPtr, sPtr, width, swizzle);

                    //Advance the temporary pointers to the next scanline
                    sPtr = MemoryHelper.AddIntPtr(sPtr, srcRowPitch);
                    dPtr = MemoryHelper.AddIntPtr(dPtr, dstRowPitch);
                }

                //Advance the pointers by their slice pitches to get to the next image
                srcPtr = MemoryHelper.AddIntPtr(srcPtr, srcSlicePitch);
                dstPtr = MemoryHelper.AddIntPtr(dstPtr, dstSlicePitch);
            }
        }

        /// <summary>
        /// Copies 32-bit RGBA/BGRA color data from the src point (with specified row/slice pitch -- it may be padded!) into a NON-PADDED 32-bit RGBA/BGRA color image. Optionally the
        /// data's first and third components can be swizzled to convert RGBA->BGRA or BGRA->RGBA data. This doesn't validate any data, so use at your own risk.
        /// </summary>
        /// <param name="dstPtr">Destination memory location.</param>
        /// <param name="srcPtr">Source memory location.</param>
        /// <param name="srcRowPitch">Source scanline stride, the image data + padding.</param>
        /// <param name="srcSlicePitch">Source slice stride, the entire image + padding, if a 3D image that is comprised of multiple slices.</param>
        /// <param name="width">Width of the image, in texels.</param>
        /// <param name="height">Height of the image, in texels.</param>
        /// <param name="depth">Depth of the image, in texels.</param>
        /// <param name="swizzle">Optionally swizzle first and third components.</param>
        public static unsafe void CopyColorImageData(IntPtr dstPtr, IntPtr srcPtr, int srcRowPitch, int srcSlicePitch, int width, int height, int depth, bool swizzle = false)
        {
            int formatSize = 4; //4-byte RGBA texel
            int dstRowPitch = formatSize * width;
            int dstSlicePitch = dstRowPitch * height;

            CopyColorImageData(dstPtr, dstRowPitch, dstSlicePitch, srcPtr, srcRowPitch, srcSlicePitch, width, height, depth, swizzle);
        }

        /// <summary>
        /// Copies a 32-bit color scanline from one memory location to another. Optionally can swizzle the first/third component
        /// of every texel as its copied.
        /// </summary>
        /// <param name="dstPtr">Destination memory location.</param>
        /// <param name="srcPtr">Source memory location.</param>
        /// <param name="width">Number of texels in the scanline.</param>
        /// <param name="swizzle">Optionally swizzle first and third components.</param>
        public static unsafe void CopyRGBALine(IntPtr dstPtr, IntPtr srcPtr, int width, bool swizzle = false)
        {
            if (!swizzle)
            {
                MemoryHelper.CopyMemory(dstPtr, srcPtr, width * 4);
                return;
            }

            byte* dPtr = (byte*)dstPtr.ToPointer();
            byte* sPtr = (byte*)srcPtr.ToPointer();

            for (int i = 0, byteIndex = 0; i < width; i++, byteIndex += 4)
            {
                int index0 = byteIndex;
                int index1 = byteIndex + 1;
                int index2 = byteIndex + 2;
                int index3 = byteIndex + 3;

                byte t0 = sPtr[index0];
                byte t1 = sPtr[index1];
                byte t2 = sPtr[index2];
                byte t3 = sPtr[index3];

                //Swap t0 and t2
                dPtr[index0] = t2;
                dPtr[index1] = t1;
                dPtr[index2] = t0;
                dPtr[index3] = t3;
            }
        }
    }
}
