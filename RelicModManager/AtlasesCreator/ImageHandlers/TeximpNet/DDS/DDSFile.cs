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
using System.IO;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.DDS
{
    /// <summary>
    /// Contains functions to read/write to a DDS file.
    /// </summary>
    public static class DDSTypes
    {
        //The 4 characters "DDS "
        private static readonly FourCC DDS_MAGIC = new FourCC('D', 'D', 'S', ' ');

        /// <summary>
        /// Determines whether a file contains the DDS format. This does not validate the header.
        /// </summary>
        /// <param name="fileName">Name of the file to check.</param>
        /// <returns>True if the file is DDS format, false if not. </returns>
        public static bool IsDDSFile(String fileName)
        {
            if (!File.Exists(fileName))
                return false;

            using (FileStream fs = File.OpenRead(fileName))
                return IsDDSFile(fs);
        }

        /// <summary>
        /// Determines whether a stream contains the DDS format. This does not validate the header. It does not
        /// advance the stream.
        /// </summary>
        /// <param name="input">Stream containing the file data.</param>
        /// <returns>True if the file is DDS format, false if not. </returns>
        public static bool IsDDSFile(Stream input)
        {
            if (input == null || !input.CanRead)
                return false;

            long minSize = (long)(MemoryHelper.SizeOf<Header>() + FourCC.SizeInBytes);
            if (!StreamHelper.CanReadBytes(input, minSize))
                return false;

            //Check magic word
            long pos = input.Position;
            uint magicWord;
            StreamHelper.ReadUInt32(input, out magicWord);
            input.Position = pos;

            return magicWord == DDS_MAGIC;
        }

        /// <summary>
        /// Reads a DDS file from disk. Image data is always returned as DXGI-Compliant
        /// format, therefore some old legacy formats will automatically be converted.
        /// </summary>
        /// <param name="fileName">File to load.</param>
        /// <param name="flags">Flags to control how the DDS data is loaded.</param>
        /// <returns>Loaded image data, or null if the data failed to load.</returns>
        public static DDSContainer Read(String fileName, DDSFlags flags = DDSFlags.None)
        {
            if (!File.Exists(fileName))
                return null;

            using (FileStream fs = File.OpenRead(fileName))
                return Read(fs, flags);
        }

        /// <summary>
        /// Reads DDS formatted data from a stream. Image data is always returned as DXGI-Compliant
        /// format, therefore some old legacy formats will automatically be converted.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="flags">Flags to control how the DDS data is loaded.</param>
        /// <returns>Loaded image data, or null if the data failed to load.</returns>
        public static DDSContainer Read(Stream input, DDSFlags flags = DDSFlags.None)
        {
            StreamTransferBuffer buffer = new StreamTransferBuffer();

            Header header;
            Header10? headerExt;

            //Reads + validates header(s)
            if (!ReadHeader(input, buffer, out header, out headerExt))
                return null;

            //Gather up metadata 
            List<MipChain> mipChains = null;
            DXGIFormat format = DXGIFormat.Unknown;
            TextureDimension texDim = TextureDimension.Two;
            ConversionFlags convFlags = ConversionFlags.None;
            bool legacyDword = (flags & DDSFlags.LegacyDword) == DDSFlags.LegacyDword ? true : false;

            int width = Math.Max((int)header.Width, 1);
            int height = Math.Max((int)header.Height, 1);
            int depth = Math.Max((int)header.Depth, 1);
            int mipCount = (int)header.MipMapCount;
            int arrayCount = 1;

            //Has extended header, a modern DDS
            if (headerExt.HasValue)
            {
                Header10 extendedHeader = headerExt.Value;
                arrayCount = (int)extendedHeader.ArraySize;
                format = extendedHeader.Format;

                switch (extendedHeader.ResourceDimension)
                {
                    case D3D10ResourceDimension.Texture1D:
                        {
                            texDim = TextureDimension.One;

                            if (height > 1 || depth > 1)
                                return null;
                        }
                        break;
                    case D3D10ResourceDimension.Texture2D:
                        {
                            if ((extendedHeader.MiscFlags & Header10Flags.TextureCube) == Header10Flags.TextureCube)
                            {
                                //Specifies # of cubemaps, so to get total # of faces must multiple by 6
                                arrayCount *= 6;

                                texDim = TextureDimension.Cube;
                            }
                            else
                            {
                                texDim = TextureDimension.Two;
                            }

                            if (depth > 1)
                                return null;
                        }
                        break;
                    case D3D10ResourceDimension.Texture3D:
                        {
                            texDim = TextureDimension.Three;

                            if (arrayCount > 1 || (header.Caps2 & HeaderCaps2.Volume) != HeaderCaps2.Volume)
                                return null;
                        }
                        break;

                }
            }
            else
            {
                //Otherwise, read legacy DDS and possibly convert data

                //Check volume flag
                if ((header.Caps2 & HeaderCaps2.Volume) == HeaderCaps2.Volume)
                {
                    texDim = TextureDimension.Three;
                }
                else
                {
                    //legacy DDS could not express 1D textures, so either a cubemap or a 2D non-array texture

                    if ((header.Caps2 & HeaderCaps2.Cubemap) == HeaderCaps2.Cubemap)
                    {
                        //Must have all six faces. DirectX 8 and above always would write out all 6 faces
                        if ((header.Caps2 & HeaderCaps2.Cubemap_AllFaces) != HeaderCaps2.Cubemap_AllFaces)
                            return null;

                        arrayCount = 6;
                        texDim = TextureDimension.Cube;
                    }
                    else
                    {
                        texDim = TextureDimension.Two;
                    }
                }

                format = FormatConverter.DetermineDXGIFormat(header.PixelFormat, flags, out convFlags);
            }

            //Modify conversion flags, if necessary
            FormatConverter.ModifyConversionFormat(ref format, ref convFlags, flags);

            //If palette image, the palette will be the first thing
            int[] palette = null;
            if (FormatConverter.HasConversionFlag(convFlags, ConversionFlags.Pal8))
            {
                palette = new int[256];
                int palSize = palette.Length * sizeof(int);
                buffer.ReadBytes(input, palSize);

                if (buffer.LastReadByteCount != palSize)
                    return null;

                MemoryHelper.CopyBytes<int>(buffer.ByteArray, 0, palette, 0, palette.Length);
            }

            //Now read data based on available mip/arrays
            mipChains = new List<MipChain>(arrayCount);

            byte[] scanline = buffer.ByteArray;
            IntPtr scanlinePtr = buffer.Pointer;
            bool noPadding = (flags & DDSFlags.NoPadding) == DDSFlags.NoPadding ? true : false;
            bool isCompressed = FormatConverter.IsCompressed(format);
            bool errored = false;

            try
            {
                //Iterate over each array face...
                for (int i = 0; i < arrayCount; i++)
                {
                    MipChain mipChain = new MipChain(mipCount);
                    mipChains.Add(mipChain);

                    //Iterate over each mip face...
                    for (int mipLevel = 0; mipLevel < mipCount; mipLevel++)
                    {
                        //Calculate mip dimensions
                        int mipWidth = width;
                        int mipHeight = height;
                        int mipDepth = depth;
                        ImageHelper.CalculateMipmapLevelDimensions(mipLevel, ref mipWidth, ref mipHeight, ref mipDepth);

                        //Compute pitch, based on MSDN programming guide which says PitchOrLinearSize is unreliable and to calculate based on format.
                        //"real" mip width/height is the given mip width/height for all non-compressed, compressed images it will be smaller since each block
                        //is a 4x4 region of pixels.
                        int realMipWidth, realMipHeight, dstRowPitch, dstSlicePitch, bytesPerPixel;
                        ImageHelper.ComputePitch(format, mipWidth, mipHeight, out dstRowPitch, out dstSlicePitch, out realMipWidth, out realMipHeight, out bytesPerPixel, legacyDword);

                        int srcRowPitch = dstRowPitch;
                        int srcSlicePitch = dstSlicePitch;

                        //Are we converting from a legacy format, possibly?
                        if (!headerExt.HasValue)
                        {
                            int legacySize = FormatConverter.LegacyFormatBitsPerPixelFromConversionFlag(convFlags);
                            if (legacySize != 0)
                            {
                                srcRowPitch = (realMipWidth * legacySize + 7) / 8;
                                srcSlicePitch = srcRowPitch * realMipHeight;
                            }
                        }

                        //If output data is requested not to have padding, recompute destination pitches
                        if (noPadding)
                        {
                            dstRowPitch = bytesPerPixel * realMipWidth;
                            dstSlicePitch = dstRowPitch * realMipHeight;
                        }

                        //Setup memory to hold the loaded image
                        MipData mipSurface = new MipData(mipWidth, mipHeight, mipDepth, dstRowPitch, dstSlicePitch);
                        mipChain.Add(mipSurface);

                        //Ensure read buffer is sufficiently sized for a single scanline
                        if (buffer.Length < srcRowPitch)
                            buffer.Resize(srcRowPitch, false);

                        IntPtr dstPtr = mipSurface.Data;

                        //Advance stream one slice at a time...
                        for (int slice = 0; slice < mipDepth; slice++)
                        {
                            long slicePos = input.Position;
                            IntPtr dPtr = dstPtr;

                            //Copy scanline into temp buffer, do any conversions, copy to output
                            for (int row = 0; row < realMipHeight; row++)
                            {
                                int numBytesRead = input.Read(scanline, 0, srcRowPitch);
                                if (numBytesRead != srcRowPitch)
                                {
                                    errored = true;
                                    System.Diagnostics.Debug.Assert(false);
                                    return null;
                                }

                                //Copy scanline, optionally convert data
                                FormatConverter.CopyScanline(dPtr, dstRowPitch, scanlinePtr, srcRowPitch, format, convFlags, palette);

                                //Increment dest pointer to next row
                                dPtr = MemoryHelper.AddIntPtr(dPtr, dstRowPitch);
                            }

                            //Advance stream and destination pointer to the next slice
                            input.Position = slicePos + srcSlicePitch;
                            dstPtr = MemoryHelper.AddIntPtr(dstPtr, dstSlicePitch);
                        }
                    }
                }
            }
            finally
            {
                //If errored, clean up any mip surfaces we allocated...no null entries should have been made either
                if (errored)
                    DisposeMipChains(mipChains);
            }

            if (!ValidateInternal(mipChains, format, texDim))
            {
                System.Diagnostics.Debug.Assert(false);
                return null;
            }

            return new DDSContainer(mipChains, format, texDim);
        }

        /// <summary>
        /// Writes a DDS file to disk. Image data is expected to be 32-bit color data, if not then mipmaps are converted as necessary automatically without modifying input data.
        /// </summary>
        /// <param name="fileName">File to write to. If it doesn't exist, it will be created.</param>
        /// <param name="image">Single image to write.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(String fileName, Surface image, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            using (FileStream fs = File.Create(fileName))
                return Write(fs, image, texDim, flags);
        }

        /// <summary>
        /// Writes a DDS file to disk. Image data is expected to be 32-bit color data, if not then mipmaps are converted as necessary automatically without modifying input data.
        /// </summary>
        /// <param name="fileName">File to write to. If it doesn't exist, it will be created.</param>
        /// <param name="mipChain">Single mipchain of images to write.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(String fileName, List<Surface> mipChain, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            using (FileStream fs = File.Create(fileName))
                return Write(fs, mipChain, texDim, flags);
        }

        /// <summary>
        /// Writes a DDS file to disk. Image data is expected to be 32-bit color data, if not then mipmaps are converted as necessary automatically without modifying input data.
        /// </summary>
        /// <param name="fileName">File to write to. If it doesn't exist, it will be created.</param>
        /// <param name="mipChains">Mipmap chains to write. Each mipmap chain represents a single face (so > 1 represents an array texture or a Cubemap). All faces must have
        /// equivalent dimensions and each chain must have the same number of mipmaps.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(String fileName, List<List<Surface>> mipChains, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            using (FileStream fs = File.Create(fileName))
                return Write(fs, mipChains, texDim, flags);
        }

        /// <summary>
        /// Writes a DDS file to a stream. Image data is expected to be 32-bit color data, if not then mipmaps are converted as necessary automatically without modifying input data.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="image">Single image to write.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(Stream output, Surface image, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            List<Surface> mipChain = new List<Surface>(1);
            mipChain.Add(image);
            List<List<Surface>> mipChains = new List<List<Surface>>(1);
            mipChains.Add(mipChain);

            return Write(output, mipChains, texDim, flags);
        }

        /// <summary>
        /// Writes a DDS file to a stream. Image data is expected to be 32-bit color data, if not then mipmaps are converted as necessary automatically without modifying input data.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="mipChain">Single mipchain of images to write.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(Stream output, List<Surface> mipChain, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            List<List<Surface>> mipChains = new List<List<Surface>>(1);
            mipChains.Add(mipChain);

            return Write(output, mipChains, texDim, flags);
        }

        /// <summary>
        /// Writes a DDS file to a stream. Image data is expected to be 32-bit color data, if not then mipmaps are converted as necessary automatically without modifying input data.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="mipChains">Mipmap chains to write. Each mipmap chain represents a single face (so > 1 represents an array texture or a Cubemap). All faces must have
        /// equivalent dimensions and each chain must have the same number of mipmaps.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(Stream output, List<List<Surface>> mipChains, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            if (mipChains == null || mipChains.Count == 0 || mipChains[0] == null || mipChains[0].Count == 0)
                return false;

            //FreeImage doesn't support volume textures.
            if (texDim == TextureDimension.Three)
                return false;

            //If texcube, must have multiples of 6, every 6 mipchains are a complete cubemap
            if (texDim == TextureDimension.Cube && (mipChains.Count % 6 != 0))
                return false;

            //FreeImage surfaces are always uncompressed and we expect 32-bit color, if not we'll convert. We'll export in whatever color order freeimage is in,
            //but we can force RGBA based on the flags
            List<MipChain> ddsMipChains = new List<MipChain>(mipChains.Count);

            bool forceRGBA = (flags & DDSFlags.ForceRgb) == DDSFlags.ForceRgb;
            bool isBGRAOrder = Surface.IsBGRAOrder;
            bool needToSwizzle = isBGRAOrder && forceRGBA;
            DXGIFormat format = (isBGRAOrder) ? DXGIFormat.B8G8R8A8_UNorm : DXGIFormat.R8G8B8A8_UNorm;

            if (forceRGBA)
                format = DXGIFormat.R8G8B8A8_UNorm;

            try
            {
                int mipCount = -1;

                foreach (List<Surface> fiMipChain in mipChains)
                {
                    MipChain ddsMipChain = new MipChain(fiMipChain.Count);
                    ddsMipChains.Add(ddsMipChain);

                    if (mipCount == -1)
                        mipCount = fiMipChain.Count;

                    //All chains must have same # of mips
                    if (mipCount != fiMipChain.Count)
                        return false;

                    foreach (Surface fiMip in fiMipChain)
                    {
                        if (fiMip == null)
                            return false;

                        //Validate dimension
                        switch (texDim)
                        {
                            case TextureDimension.One:
                                if (fiMip.Height > 1)
                                    return false;
                                break;
                            case TextureDimension.Cube:
                                if (fiMip.Width != fiMip.Height)
                                    return false;
                                break;
                        }

                        bool is32BitBitmap = fiMip.ImageType == ImageType.Bitmap && fiMip.ColorType == ImageColorType.RGBA && fiMip.BitsPerPixel == 32;

                        if (is32BitBitmap)
                        {
                            //If no swizzling...just use the data directly
                            if (!needToSwizzle)
                            {
                                ddsMipChain.Add(new MipData(fiMip));
                            }
                            else
                            {
                                MipData newMip = new MipData(fiMip.Width, fiMip.Height, fiMip.Pitch);
                                ImageHelper.CopyColorImageData(newMip.Data, newMip.RowPitch, 0, fiMip.DataPtr, fiMip.Pitch, 0, newMip.Width, newMip.Height, 1, true);
                                ddsMipChain.Add(newMip);
                            }
                        }
                        else
                        {
                            //Need to convert. Possible to map other DXGI formats to free image bitmaps (most likely RGBA floats), but we're keeping it simple. User can wrap surfaces
                            //and use the general write method
                            using (Surface converted = fiMip.Clone())
                            {
                                if (!converted.ConvertTo(ImageConversion.To32Bits))
                                    return false;

                                MipData newMip = new MipData(converted.Width, converted.Height, converted.Pitch);
                                ImageHelper.CopyColorImageData(newMip.Data, newMip.RowPitch, 0, converted.DataPtr, converted.Pitch, 0, newMip.Width, newMip.Height, 1, needToSwizzle);
                                ddsMipChain.Add(newMip);
                            }
                        }
                    }
                }

                //Write out DDS
                return Write(output, ddsMipChains, format, texDim, flags);
            }
            finally
            {
                //Dispose of mip surfaces. If they own any data, it'll be cleaned up
                DisposeMipChains(ddsMipChains);
            }
        }

        /// <summary>
        /// Writes a DDS file to disk. Image data is expected to be DXGI-compliant data, but an effort is made to write out D3D9-compatible headers when possible.
        /// </summary>
        /// <param name="fileName">File to write to. If it doesn't exist, it will be created.</param>
        /// <param name="mipChains">Mipmap chains to write. Each mipmap chain represents a single face (so > 1 represents an array texture or a Cubemap). All faces must have
        /// equivalent dimensions and each chain must have the same number of mipmaps.</param>
        /// <param name="format">DXGI format the image data is stored as.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(String fileName, List<MipChain> mipChains, DXGIFormat format, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            using (FileStream fs = File.Create(fileName))
                return Write(fs, mipChains, format, texDim, flags);
        }

        /// <summary>
        /// Writes DDS formatted data to a stream. Image data is expected to be DXGI-compliant data, but an effort is made to write out D3D9-compatible headers when possible.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="mipChains">Mipmap chains to write. Each mipmap chain represents a single face (so > 1 represents an array texture or a Cubemap). All faces must have
        /// equivalent dimensions and each chain must have the same number of mipmaps.</param>
        /// <param name="format">DXGI format the image data is stored as.</param>
        /// <param name="texDim">Dimension of the texture to write.</param>
        /// <param name="flags">Flags to control how the DDS data is saved.</param>
        /// <returns>True if writing the data was successful, false if otherwise.</returns>
        public static bool Write(Stream output, List<MipChain> mipChains, DXGIFormat format, TextureDimension texDim, DDSFlags flags = DDSFlags.None)
        {
            if (output == null || !output.CanWrite || mipChains == null || mipChains.Count == 0 || mipChains[0].Count == 0 || format == DXGIFormat.Unknown)
                return false;

            //Extract details
            int width, height, depth, arrayCount, mipCount;
            MipData firstMip = mipChains[0][0];
            width = firstMip.Width;
            height = firstMip.Height;
            depth = firstMip.Depth;
            arrayCount = mipChains.Count;
            mipCount = mipChains[0].Count;

            if (!ValidateInternal(mipChains, format, texDim))
                return false;

            //Setup a transfer buffer
            StreamTransferBuffer buffer = new StreamTransferBuffer(firstMip.RowPitch, false);

            //Write out header
            if (!WriteHeader(output, buffer, texDim, format, width, height, depth, arrayCount, mipCount, flags))
                return false;

            //Iterate over each array face...
            for (int i = 0; i < arrayCount; i++)
            {
                MipChain mipChain = mipChains[i];

                //Iterate over each mip face...
                for (int mipLevel = 0; mipLevel < mipCount; mipLevel++)
                {
                    MipData mip = mipChain[mipLevel];

                    //Compute pitch, based on MSDN programming guide. We will write out these pitches rather than the supplied in order to conform to the recomendation
                    //that we compute pitch based on format
                    int realMipWidth, realMipHeight, dstRowPitch, dstSlicePitch, bytesPerPixel;
                    ImageHelper.ComputePitch(format, mip.Width, mip.Height, out dstRowPitch, out dstSlicePitch, out realMipWidth, out realMipHeight, out bytesPerPixel);

                    //Ensure write buffer is sufficiently sized for a single scanline
                    if (buffer.Length < dstRowPitch)
                        buffer.Resize(dstRowPitch, false);

                    //Sanity check
                    if (dstRowPitch < mip.RowPitch)
                        return false;

                    IntPtr srcPtr = mip.Data;

                    //Advance stream one slice at a time...
                    for (int slice = 0; slice < mip.Depth; slice++)
                    {
                        int bytesToWrite = dstSlicePitch;
                        IntPtr sPtr = srcPtr;

                        //Copy scanline into temp buffer, write to output
                        for (int row = 0; row < realMipHeight; row++)
                        {
                            MemoryHelper.CopyMemory(buffer.Pointer, sPtr, dstRowPitch);
                            buffer.WriteBytes(output, dstRowPitch);
                            bytesToWrite -= dstRowPitch;

                            //Advance to next scanline in source data
                            sPtr = MemoryHelper.AddIntPtr(sPtr, mip.RowPitch);
                        }

                        //Pad slice if necessary
                        if (bytesToWrite > 0)
                        {
                            MemoryHelper.ClearMemory(buffer.Pointer, 0, bytesToWrite);
                            buffer.WriteBytes(output, bytesToWrite);
                        }

                        //Advance source pointer to next slice
                        srcPtr = MemoryHelper.AddIntPtr(srcPtr, mip.SlicePitch);
                    }
                }
            }

            return true;
        }

        private static bool ReadHeader(Stream input, StreamTransferBuffer buffer, out Header header, out Header10? headerExt)
        {
            headerExt = null;

            //Validate that this is a DDS file and can at a minimum read (basic) header info from it
            if (!IsDDSFile(input))
            {
                header = new Header();
                return false;
            }

            //Magic word read, advance by size of the magic word
            input.Position += FourCC.SizeInBytes;

            //Read primary header
            buffer.Read<Header>(input, out header);

            //Verify header
            if (header.Size != MemoryHelper.SizeOf<Header>() || header.PixelFormat.Size != MemoryHelper.SizeOf<PixelFormat>())
                return false;

            //Possibly read extended header
            if (header.PixelFormat.IsDX10Extended)
            {
                //Check if we can read the header
                long minSize = MemoryHelper.SizeOf<Header10>();
                if (!StreamHelper.CanReadBytes(input, minSize))
                    return false;

                Header10 header10;
                buffer.Read<Header10>(input, out header10);

                //Check array size
                if (header10.ArraySize == 0)
                    return false;

                headerExt = header10;
            }

            //Ensure have at least one miplevel, seems like sometimes this will be zero even though there is one mip surface (the main image)
            if (header.MipMapCount == 0)
                header.MipMapCount = 1;

            return true;
        }

        private static bool WriteHeader(Stream output, StreamTransferBuffer buffer, TextureDimension texDim, DXGIFormat format, int width, int height, int depth, int arrayCount, int mipCount, DDSFlags flags)
        {
            //Force the DX10 header...
            bool writeDX10Header = (flags & DDSFlags.ForceExtendedHeader) == DDSFlags.ForceExtendedHeader;

            //Or do DX10 if the following is true...1D textures or 2D texture arrays that aren't cubemaps...
            if (!writeDX10Header)
            {
                switch (texDim)
                {
                    case TextureDimension.One:
                        writeDX10Header = true;
                        break;
                    case TextureDimension.Two:
                        writeDX10Header = arrayCount > 1;
                        break;
                }
            }

            //Figure out pixel format, if not writing DX10 header...
            PixelFormat pixelFormat;
            if (!writeDX10Header)
            {
                switch (format)
                {
                    case DXGIFormat.R8G8B8A8_UNorm:
                        pixelFormat = PixelFormat.A8B8G8R8;
                        break;
                    case DXGIFormat.R16G16_UNorm:
                        pixelFormat = PixelFormat.G16R16;
                        break;
                    case DXGIFormat.R8G8_UNorm:
                        pixelFormat = PixelFormat.A8L8;
                        break;
                    case DXGIFormat.R16_UNorm:
                        pixelFormat = PixelFormat.L16;
                        break;
                    case DXGIFormat.R8_UNorm:
                        pixelFormat = PixelFormat.L8;
                        break;
                    case DXGIFormat.A8_UNorm:
                        pixelFormat = PixelFormat.A8;
                        break;
                    case DXGIFormat.R8G8_B8G8_UNorm:
                        pixelFormat = PixelFormat.R8G8_B8G8;
                        break;
                    case DXGIFormat.G8R8_G8B8_UNorm:
                        pixelFormat = PixelFormat.G8R8_G8B8;
                        break;
                    case DXGIFormat.BC1_UNorm:
                        pixelFormat = PixelFormat.DXT1;
                        break;
                    case DXGIFormat.BC2_UNorm:
                        pixelFormat = PixelFormat.DXT3;
                        break;
                    case DXGIFormat.BC3_UNorm:
                        pixelFormat = PixelFormat.DXT5;
                        break;
                    case DXGIFormat.BC4_UNorm:
                        pixelFormat = PixelFormat.BC4_UNorm;
                        break;
                    case DXGIFormat.BC4_SNorm:
                        pixelFormat = PixelFormat.BC4_SNorm;
                        break;
                    case DXGIFormat.BC5_UNorm:
                        pixelFormat = PixelFormat.BC5_UNorm;
                        break;
                    case DXGIFormat.BC5_SNorm:
                        pixelFormat = PixelFormat.BC5_SNorm;
                        break;
                    case DXGIFormat.B5G6R5_UNorm:
                        pixelFormat = PixelFormat.R5G6B5;
                        break;
                    case DXGIFormat.B5G5R5A1_UNorm:
                        pixelFormat = PixelFormat.A1R5G5B5;
                        break;
                    case DXGIFormat.B8G8R8A8_UNorm:
                        pixelFormat = PixelFormat.A8R8G8B8;
                        break;
                    case DXGIFormat.B8G8R8X8_UNorm:
                        pixelFormat = PixelFormat.X8R8G8B8;
                        break;
                    case DXGIFormat.B4G4R4A4_UNorm:
                        pixelFormat = PixelFormat.A4R4G4B4;
                        break;
                    case DXGIFormat.R32G32B32A32_Float:
                        pixelFormat = PixelFormat.R32G32B32A32_Float;
                        break;
                    case DXGIFormat.R16G16B16A16_Float:
                        pixelFormat = PixelFormat.R16G16B16A16_Float;
                        break;
                    case DXGIFormat.R16G16B16A16_UNorm:
                        pixelFormat = PixelFormat.R16G16B16A16_UNorm;
                        break;
                    case DXGIFormat.R16G16B16A16_SNorm:
                        pixelFormat = PixelFormat.R16G16B16A16_SNorm;
                        break;
                    case DXGIFormat.R32G32_Float:
                        pixelFormat = PixelFormat.R32G32_Float;
                        break;
                    case DXGIFormat.R16G16_Float:
                        pixelFormat = PixelFormat.R16G16_Float;
                        break;
                    case DXGIFormat.R32_Float:
                        pixelFormat = PixelFormat.R32_Float;
                        break;
                    case DXGIFormat.R16_Float:
                        pixelFormat = PixelFormat.R16_Float;
                        break;
                    default:
                        pixelFormat = PixelFormat.DX10Extended;
                        writeDX10Header = true;
                        break;
                }
            }
            else
            {
                pixelFormat = PixelFormat.DX10Extended;
            }

            Header header = new Header();
            header.Size = (uint)MemoryHelper.SizeOf<Header>();
            header.PixelFormat = pixelFormat;
            header.Flags = HeaderFlags.Caps | HeaderFlags.Width | HeaderFlags.Height | HeaderFlags.PixelFormat;
            header.Caps = HeaderCaps.Texture;

            Header10? header10 = null;

            if (mipCount > 0)
            {
                header.Flags |= HeaderFlags.MipMapCount;
                header.MipMapCount = (uint)mipCount;
                header.Caps |= HeaderCaps.MipMap;
            }

            switch (texDim)
            {
                case TextureDimension.One:
                    header.Width = (uint)width;
                    header.Height = 1;
                    header.Depth = 1;

                    //Should always be writing out extended header for 1D textures
                    System.Diagnostics.Debug.Assert(writeDX10Header);

                    header10 = new Header10(format, D3D10ResourceDimension.Texture1D, Header10Flags.None, (uint)arrayCount, Header10Flags2.None);

                    break;
                case TextureDimension.Two:
                    header.Width = (uint)width;
                    header.Height = (uint)height;
                    header.Depth = 1;

                    if (writeDX10Header)
                        header10 = new Header10(format, D3D10ResourceDimension.Texture2D, Header10Flags.None, (uint)arrayCount, Header10Flags2.None);

                    break;
                case TextureDimension.Cube:
                    header.Width = (uint)width;
                    header.Height = (uint)height;
                    header.Depth = 1;
                    header.Caps |= HeaderCaps.Complex;
                    header.Caps2 |= HeaderCaps2.Cubemap_AllFaces;

                    //can support array tex cubes, so must be multiples of 6
                    if (arrayCount % 6 != 0)
                        return false;

                    if (writeDX10Header)
                        header10 = new Header10(format, D3D10ResourceDimension.Texture2D, Header10Flags.TextureCube, (uint)arrayCount / 6, Header10Flags2.None);

                    break;
                case TextureDimension.Three:
                    header.Width = (uint)width;
                    header.Height = (uint)height;
                    header.Depth = (uint)depth;
                    header.Flags |= HeaderFlags.Depth;
                    header.Caps2 |= HeaderCaps2.Volume;

                    if (arrayCount != 1)
                        return false;

                    if (writeDX10Header)
                        header10 = new Header10(format, D3D10ResourceDimension.Texture3D, Header10Flags.None, 1, Header10Flags2.None);

                    break;
            }

            int realWidth, realHeight, rowPitch, slicePitch;
            ImageHelper.ComputePitch(format, width, height, out rowPitch, out slicePitch, out realWidth, out realHeight);

            if (FormatConverter.IsCompressed(format))
            {
                header.Flags |= HeaderFlags.LinearSize;
                header.PitchOrLinearSize = (uint)slicePitch;
            }
            else
            {
                header.Flags |= HeaderFlags.Pitch;
                header.PitchOrLinearSize = (uint)rowPitch;
            }

            //Write out magic word, DDS header, and optionally extended header
            buffer.Write<FourCC>(output, DDS_MAGIC);
            buffer.Write<Header>(output, header);

            if (header10.HasValue)
            {
                System.Diagnostics.Debug.Assert(header.PixelFormat.IsDX10Extended);
                buffer.Write<Header10>(output, header10.Value);
            }

            return true;
        }

        internal static bool ValidateInternal(List<MipChain> mipChains, DXGIFormat format, TextureDimension texDim)
        {
            if (format == DXGIFormat.Unknown)
                return false;

            //Mipchains must exist, must have at least one, and chain must have mipmaps.
            if (mipChains == null || mipChains.Count == 0 || mipChains[0].Count == 0)
                return false;

            //Validate cubemap...must have multiples of 6 faces (can be an array of cubes).
            if (texDim == TextureDimension.Cube && (mipChains.Count % 6) != 0)
                return false;

            //Validate 3d texture..can't have arrays
            if (texDim == TextureDimension.Three && mipChains.Count > 1)
                return false;

            int width, height, depth, rowPitch, slicePitch;

            //Save the first image dimensions
            MipData firstSurface = mipChains[0][0];
            width = firstSurface.Width;
            height = firstSurface.Height;
            depth = firstSurface.Depth;
            rowPitch = firstSurface.RowPitch;
            slicePitch = firstSurface.SlicePitch;

            //Validate first surface
            if (width < 1 || height < 1 || depth < 1 || rowPitch < 1 || slicePitch < 1)
                return false;

            //Validate 1D texture...must only have 1 height
            if (texDim == TextureDimension.One && height > 1)
                return false;

            //Validate cubemap...width/height must be same
            if (texDim == TextureDimension.Cube && (width != height))
                return false;

            //Only 3d textures have depth
            if (texDim != TextureDimension.Three && depth > 1)
                return false;

            //Go through each chain and validate against the first texture and ensure mipmaps are progressively smaller
            int mipCount = -1;

            for (int i = 0; i < mipChains.Count; i++)
            {
                MipChain mipmaps = mipChains[i];

                //Mips must exist...
                if (mipmaps == null || mipmaps.Count == 0)
                    return false;

                //Grab a mip count from first chain
                if (mipCount == -1)
                    mipCount = mipmaps.Count;

                //Each chain must have the same number of mip surfaces
                if (mipmaps.Count != mipCount)
                    return false;

                //Each mip surface must have data and check sizes
                MipData prevMip = mipmaps[0];

                //Check against the first main image we looked at earlier
                if (prevMip.Width != width || prevMip.Height != height || prevMip.Depth != depth || prevMip.Data == IntPtr.Zero || prevMip.RowPitch != rowPitch || prevMip.SlicePitch != slicePitch)
                    return false;

                for (int mipLevel = 1; mipLevel < mipmaps.Count; mipLevel++)
                {
                    MipData nextMip = mipmaps[mipLevel];

                    //Ensure each mipmap is progressively smaller or same at the least
                    if (nextMip.Width > prevMip.Width || nextMip.Height > prevMip.Height || nextMip.Depth > prevMip.Depth || nextMip.Data == IntPtr.Zero
                        || nextMip.RowPitch > prevMip.RowPitch || nextMip.SlicePitch > prevMip.SlicePitch || nextMip.RowPitch == 0 || nextMip.SlicePitch == 0)
                        return false;

                    prevMip = nextMip;
                }
            }

            return true;
        }

        internal static void DisposeMipChains(List<MipChain> mipChains)
        {
            if (mipChains == null)
                return;

            foreach (MipChain chain in mipChains)
            {
                if (chain == null)
                    continue;

                foreach (MipData surface in chain)
                {
                    if (surface == null)
                        continue;

                    surface.Dispose();
                }

                chain.Clear();
            }

            mipChains.Clear();
        }
    }
}
