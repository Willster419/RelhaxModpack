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
using System.Diagnostics;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.DDS
{
    [Flags]
    internal enum ConversionFlags
    {
        None = 0x0,
        Expand = 0x1, // Conversion requires expanded pixel size
        NoAlpha = 0x2, // Conversion requires setting alpha to known value
        Swizzle = 0x4, // BGR/RGB order swizzling required
        Pal8 = 0x8, // Has an 8-bit palette
        Format888 = 0x10, // Source is an 8:8:8 (24bpp) format
        Format565 = 0x20, // Source is a 5:6:5 (16bpp) format
        Format5551 = 0x40, // Source is a 5:5:5:1 (16bpp) format
        Format4444 = 0x80, // Source is a 4:4:4:4 (16bpp) format
        Format44 = 0x100, // Source is a 4:4 (8bpp) format
        Format332 = 0x200, // Source is a 3:3:2 (8bpp) format
        Format8332 = 0x400, // Source is a 8:3:3:2 (16bpp) format
        FormatA8P8 = 0x800 // Has an 8-bit palette with an alpha channel
    };

    //Formats that we need to implicitly expand upon load, since we do not have a clean mapping to DXGI formats
    internal enum LegacyFormat
    {
        Unknown = 0,
        R8G8B8 = 1,
        R3G3B2 = 2,
        A8R3G3B2 = 3,
        P8 = 4,
        A8P8 = 5,
        A4L4 = 6,
        B4G4R4A4 = 7,
        B5G6R5 = 8,
        B5G5R5A1 = 9
    }

    [Flags]
    internal enum ScanlineFlags
    {
        None = 0,

        //Set alpha channel to an opaque value
        SetOpaqueAlpha = 0x1,

        //Enables specific legacy format conversion cases (currently R10G10B10A2 bug fix)
        Legacy = 0x2
    }

    //Maps legacy pixel formats to DXGI formats and have optional conversion flags denoting how the old format should be handled (if at all). Some formats can just be treated as a DXGI format without modification.
    [DebuggerDisplay("Format = {Format}, ConversionFlags = {ConversionFlags}, LegacyFormat = {LegacyFormat}")]
    internal struct LegacyFormatMap
    {
        public DXGIFormat Format;
        public ConversionFlags ConversionFlags;
        public PixelFormat LegacyFormat;

        public LegacyFormatMap(DXGIFormat format, ConversionFlags conversionFlags, PixelFormat legacyFormat)
        {
            Format = format;
            ConversionFlags = conversionFlags;
            LegacyFormat = legacyFormat;
        }
    }

    /// <summary>
    /// Adapted from SharpDX's toolkit (https://github.com/sharpdx/Toolkit/blob/master/Source/Toolkit/SharpDX.Toolkit.Graphics/DDSHelper.cs) to automatically
    /// convert legacy DDS formats to modern/friendlier DXGI formats, and other format utilities.
    /// </summary>
    internal static class FormatConverter
    {
        //Default set of mappings, legacy D3D formats should be read right-to-left for component ordering, DXGI is left to right.
        private static readonly LegacyFormatMap[] LegacyMappings = new LegacyFormatMap[]
        {
            // D3DFMT_DXT1
            new LegacyFormatMap(DXGIFormat.BC1_UNorm, ConversionFlags.None, PixelFormat.DXT1),

            // D3DFMT_DXT3
            new LegacyFormatMap(DXGIFormat.BC2_UNorm, ConversionFlags.None, PixelFormat.DXT3),

            // D3DFMT_DXT5
            new LegacyFormatMap(DXGIFormat.BC3_UNorm, ConversionFlags.None, PixelFormat.DXT5),

            // D3DFMT_DXT2 (ignore premultiply)
            new LegacyFormatMap(DXGIFormat.BC2_UNorm, ConversionFlags.None, PixelFormat.DXT2),

            // D3DFMT_DXT4 (ignore premultiply)
            new LegacyFormatMap(DXGIFormat.BC3_UNorm, ConversionFlags.None, PixelFormat.DXT4),

            new LegacyFormatMap(DXGIFormat.BC4_UNorm, ConversionFlags.None, PixelFormat.BC4_UNorm),
            new LegacyFormatMap(DXGIFormat.BC4_SNorm, ConversionFlags.None, PixelFormat.BC4_SNorm),
            new LegacyFormatMap(DXGIFormat.BC5_UNorm, ConversionFlags.None, PixelFormat.BC5_UNorm),
            new LegacyFormatMap(DXGIFormat.BC5_SNorm, ConversionFlags.None, PixelFormat.BC5_SNorm),

            new LegacyFormatMap(DXGIFormat.BC4_UNorm, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '1'))),
            new LegacyFormatMap(DXGIFormat.BC5_UNorm, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '2'))),

            // D3DFMT_YUY2
            new LegacyFormatMap(DXGIFormat.YUY2, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, new FourCC('Y', 'U', 'Y', '2'))),

            // D3DFMT_R8G8_B8G8
            new LegacyFormatMap(DXGIFormat.R8G8_B8G8_UNorm, ConversionFlags.None, PixelFormat.R8G8_B8G8),

            // D3DFMT_G8R8_G8B8
            new LegacyFormatMap(DXGIFormat.G8R8_G8B8_UNorm, ConversionFlags.None, PixelFormat.G8R8_G8B8),

            // D3DFMT_A8R8G8B8 (uses DXGI 1.1 format)
            new LegacyFormatMap(DXGIFormat.B8G8R8A8_UNorm, ConversionFlags.None, PixelFormat.A8R8G8B8),

            // D3DFMT_X8R8G8B8 (uses DXGI 1.1 format)
            new LegacyFormatMap(DXGIFormat.B8G8R8X8_UNorm, ConversionFlags.None, PixelFormat.X8R8G8B8),

             // D3DFMT_A8B8G8R8
            new LegacyFormatMap(DXGIFormat.R8G8B8A8_UNorm, ConversionFlags.None, PixelFormat.A8B8G8R8),

            // D3DFMT_X8B8G8R8
            new LegacyFormatMap(DXGIFormat.R8G8B8A8_UNorm, ConversionFlags.NoAlpha, PixelFormat.X8B8G8R8),

            // D3DFMT_G16R16
            new LegacyFormatMap(DXGIFormat.R16G16_UNorm, ConversionFlags.None, PixelFormat.G16R16),

            // D3DFMT_A2R10G10B10 (D3DX reversal issue workaround)
            new LegacyFormatMap(DXGIFormat.R10G10B10A2_UNorm, ConversionFlags.Swizzle, new PixelFormat(PixelFormatFlags.RGB, 0, 32, 0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000)),

            // D3DFMT_A2B10G10R10 (D3DX reversal issue workaround)
            new LegacyFormatMap(DXGIFormat.R10G10B10A2_UNorm, ConversionFlags.None, new PixelFormat(PixelFormatFlags.RGB, 0, 32, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000)),

            // D3DFMT_R8G8B8
            new LegacyFormatMap(DXGIFormat.R8G8B8A8_UNorm, ConversionFlags.Expand | ConversionFlags.NoAlpha | ConversionFlags.Format888, PixelFormat.R8G8B8),

            // D3DFMT_R5G6B5
            new LegacyFormatMap(DXGIFormat.B5G6R5_UNorm, ConversionFlags.Format565, PixelFormat.R5G6B5),

            // D3DFMT_A1R5G5B5
            new LegacyFormatMap(DXGIFormat.B5G5R5A1_UNorm, ConversionFlags.Format5551, PixelFormat.A1R5G5B5),

            // D3DFMT_X1R5G5B5
            new LegacyFormatMap(DXGIFormat.B5G5R5A1_UNorm, ConversionFlags.Format5551 | ConversionFlags.NoAlpha, new PixelFormat(PixelFormatFlags.RGB, 0, 16, 0x7c00, 0x03e0, 0x001f, 0x0000)),
     
            // D3DFMT_A8R3G3B2
            new LegacyFormatMap(DXGIFormat.R8G8B8A8_UNorm, ConversionFlags.Expand | ConversionFlags.Format8332, new PixelFormat(PixelFormatFlags.RGB, 0, 16, 0x00e0, 0x001c, 0x0003, 0xff00)),

            // D3DFMT_R3G3B2
            new LegacyFormatMap(DXGIFormat.B5G6R5_UNorm, ConversionFlags.Expand | ConversionFlags.Format332, new PixelFormat(PixelFormatFlags.RGB, 0, 8, 0xe0, 0x1c, 0x03, 0x00)),
  
            // D3DFMT_L8
            new LegacyFormatMap(DXGIFormat.R8_UNorm, ConversionFlags.None, PixelFormat.L8),

            // D3DFMT_L16
            new LegacyFormatMap(DXGIFormat.R16_UNorm, ConversionFlags.None, PixelFormat.L16),

            // D3DFMT_A8L8
            new LegacyFormatMap(DXGIFormat.R8G8_UNorm, ConversionFlags.None, PixelFormat.A8L8),

            // D3DFMT_A8
            new LegacyFormatMap(DXGIFormat.A8_UNorm, ConversionFlags.None, PixelFormat.A8),

            // D3DFMT_A16B16G16R16
            new LegacyFormatMap(DXGIFormat.R16G16B16A16_UNorm, ConversionFlags.None, PixelFormat.R16G16B16A16_UNorm),

            // D3DFMT_Q16W16V16U16
            new LegacyFormatMap(DXGIFormat.R16G16B16A16_SNorm, ConversionFlags.None, PixelFormat.R16G16B16A16_SNorm),

            // D3DFMT_R16F
            new LegacyFormatMap(DXGIFormat.R16_Float, ConversionFlags.None, PixelFormat.R16_Float),

            // D3DFMT_G16R16F
            new LegacyFormatMap(DXGIFormat.R16G16_Float, ConversionFlags.None, PixelFormat.R16G16_Float),

            // D3DFMT_A16B16G16R16F
            new LegacyFormatMap(DXGIFormat.R16G16B16A16_Float, ConversionFlags.None, PixelFormat.R16G16B16A16_Float),

            // D3DFMT_R32F
            new LegacyFormatMap(DXGIFormat.R32_Float, ConversionFlags.None, PixelFormat.R32_Float),

            // D3DFMT_G32R32F
            new LegacyFormatMap(DXGIFormat.R32G32_Float, ConversionFlags.None, PixelFormat.R32G32_Float),

             // D3DFMT_A32B32G32R32F
            new LegacyFormatMap(DXGIFormat.R32G32B32A32_Float, ConversionFlags.None, PixelFormat.R32G32B32A32_Float),

            // D3DFMT_R32F (D3DX uses FourCC 114 instead)
            new LegacyFormatMap(DXGIFormat.R32_Float, ConversionFlags.None, new DDS.PixelFormat(DDS.PixelFormatFlags.RGB, 0, 32, 0xffffffff, 0x00000000, 0x00000000, 0x00000000)),
            
            // D3DFMT_A8P8
            new LegacyFormatMap(DXGIFormat.R8G8B8A8_UNorm, ConversionFlags.Expand | ConversionFlags.Pal8 | ConversionFlags.FormatA8P8, new PixelFormat(PixelFormatFlags.Pal8, 0, 16, 0, 0, 0, 0)),

            // D3DFMT_P8
            new LegacyFormatMap(DXGIFormat.R8G8B8A8_UNorm, ConversionFlags.Expand | ConversionFlags.Pal8, new PixelFormat(PixelFormatFlags.Pal8, 0, 8, 0, 0, 0, 0)),

            // D3DFMT_A4R4G4B4 (uses DXGI 1.2 format)
            new LegacyFormatMap(DXGIFormat.B4G4R4A4_UNorm, ConversionFlags.Format4444, PixelFormat.A4R4G4B4 ),

            // D3DFMT_X4R4G4B4 (uses DXGI 1.2 format)
            new LegacyFormatMap(DXGIFormat.B4G4R4A4_UNorm, ConversionFlags.NoAlpha | ConversionFlags.Format4444, new PixelFormat(PixelFormatFlags.RGB, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)),

            // D3DFMT_A4L4 (uses DXGI 1.2 format)
            new LegacyFormatMap(DXGIFormat.B4G4R4A4_UNorm, ConversionFlags.Expand | ConversionFlags.Format44, new PixelFormat(PixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0)),
        };

        /// <summary>
        /// Determines the DXGI format based on the incoming pixel format's bit count and masks only.
        /// </summary>
        /// <param name="bitCount">Bits per pixel</param>
        /// <param name="redMask">Mask for red component.</param>
        /// <param name="greenMask">Mask for green component.</param>
        /// <param name="blueMask">Mask for blue component.</param>
        /// <param name="alphaMask">Mask for alpha component.</param>
        /// <returns>Found DXGI format, or unknown.</returns>
        public static DXGIFormat DetermineDXGIFormat(uint bitCount, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
        {
            //Search legacy mappings and try to determine the best-guess at the DXGI format

            int index = 0;
            for (index = 0; index < LegacyMappings.Length; index++)
            {
                LegacyFormatMap entry = LegacyMappings[index];

                if (entry.LegacyFormat.RGBBitCount == bitCount)
                {
                    //RGB, RGBA, Alpha, Luminance
                    if (entry.LegacyFormat.RedBitMask == redMask &&
                        entry.LegacyFormat.GreenBitMask == greenMask &&
                        entry.LegacyFormat.BlueBitMask == blueMask &&
                        entry.LegacyFormat.AlphaBitMask == alphaMask)
                    {
                        break;
                    }
                }
            }

            if (index >= LegacyMappings.Length)
                return DXGIFormat.Unknown;

            LegacyFormatMap mapping = LegacyMappings[index];
            return mapping.Format;
        }

        /// <summary>
        /// Determines the DXGI format based on the incoming pixel format and import flags.
        /// </summary>
        /// <param name="pixelFormat">Pixel format read from the file.</param>
        /// <param name="flags">Import flags.</param>
        /// <param name="conversionFlags">Conversion flags (if any) denoting how the data needs to be transformed to expected format.</param>
        /// <returns>Found DXGI format, or unknown.</returns>
        public static DXGIFormat DetermineDXGIFormat(in PixelFormat pixelFormat, DDSFlags flags, out ConversionFlags conversionFlags)
        {
            conversionFlags = ConversionFlags.None;

            //Search legacy mappings and try to determine the best-guess at the DXGI format

            int index = 0;
            for (index = 0; index < LegacyMappings.Length; index++)
            {
                LegacyFormatMap entry = LegacyMappings[index];

                //If has matching flags...
                if ((pixelFormat.Flags & entry.LegacyFormat.Flags) == pixelFormat.Flags)
                {
                    //If fourCC format
                    if ((entry.LegacyFormat.Flags & PixelFormatFlags.FourCC) == PixelFormatFlags.FourCC)
                    {
                        if (entry.LegacyFormat.FourCC == pixelFormat.FourCC)
                            break;
                    }
                    //If palette
                    else if ((entry.LegacyFormat.Flags & PixelFormatFlags.Pal8) == PixelFormatFlags.Pal8)
                    {
                        if (entry.LegacyFormat.RGBBitCount == pixelFormat.RGBBitCount)
                            break;
                    }
                    //If uncompressed color data
                    else if (entry.LegacyFormat.RGBBitCount == pixelFormat.RGBBitCount)
                    {
                        //RGB, RGBA, Alpha, Luminance
                        if (entry.LegacyFormat.RedBitMask == pixelFormat.RedBitMask &&
                            entry.LegacyFormat.GreenBitMask == pixelFormat.GreenBitMask &&
                            entry.LegacyFormat.BlueBitMask == pixelFormat.BlueBitMask &&
                            entry.LegacyFormat.AlphaBitMask == pixelFormat.AlphaBitMask)
                        {
                            break;
                        }
                    }
                }
            }

            if (index >= LegacyMappings.Length)
                return DXGIFormat.Unknown;

            LegacyFormatMap mapping = LegacyMappings[index];

            conversionFlags = mapping.ConversionFlags;
            DXGIFormat dxgiFormat = mapping.Format;

            //Remove 10:10:10:2 swizzle if we don't want it
            if (dxgiFormat == DXGIFormat.R10G10B10A2_UNorm && (flags & DDSFlags.NoR10B10G10A2Fixup) == DDSFlags.NoR10B10G10A2Fixup)
                conversionFlags &= ~ConversionFlags.Swizzle;

            return dxgiFormat;
        }

        public static void ModifyConversionFormat(ref DXGIFormat dxgiFormat, ref ConversionFlags conversionFlags, DDSFlags importFlags)
        {
            //Swizzle red and blue components if we want to always have RGBA ordering
            if ((importFlags & DDSFlags.ForceRgb) == DDSFlags.ForceRgb)
            {
                switch (dxgiFormat)
                {
                    case DXGIFormat.B8G8R8A8_UNorm:
                        dxgiFormat = DXGIFormat.R8G8B8A8_UNorm;
                        conversionFlags |= ConversionFlags.Swizzle;
                        break;
                    case DXGIFormat.B8G8R8X8_UNorm:
                        dxgiFormat = DXGIFormat.R8G8B8A8_UNorm;
                        conversionFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;
                    case DXGIFormat.B8G8R8A8_Typeless:
                        dxgiFormat = DXGIFormat.R8G8B8A8_Typeless; ;
                        conversionFlags |= ConversionFlags.Swizzle;
                        break;
                    case DXGIFormat.B8G8R8A8_UNorm_SRGB:
                        dxgiFormat = DXGIFormat.R8G8B8A8_UNorm_SRGB;
                        conversionFlags |= ConversionFlags.Swizzle;
                        break;
                    case DXGIFormat.B8G8R8X8_Typeless:
                        dxgiFormat = DXGIFormat.R8G8B8A8_Typeless;
                        conversionFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;
                    case DXGIFormat.B8G8R8X8_UNorm_SRGB:
                        dxgiFormat = DXGIFormat.R8G8B8A8_UNorm_SRGB;
                        conversionFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
                        break;
                }
            }

            //Expand 16bpp formats
            if ((importFlags & DDSFlags.No16Bpp) == DDSFlags.No16Bpp)
            {
                switch (dxgiFormat)
                {
                    case DXGIFormat.B5G6R5_UNorm:
                    case DXGIFormat.B5G5R5A1_UNorm:
                    case DXGIFormat.B4G4R4A4_UNorm:
                        if (dxgiFormat == DXGIFormat.B5G6R5_UNorm)
                            conversionFlags |= ConversionFlags.NoAlpha;

                        dxgiFormat = DXGIFormat.R8G8B8A8_UNorm;
                        conversionFlags |= ConversionFlags.Expand;
                        break;
                }
            }
        }

        public static bool HasScanlineFlag(ScanlineFlags bitSet, ScanlineFlags flag)
        {
            return (bitSet & flag) == flag;
        }

        public static bool HasConversionFlag(ConversionFlags bitSet, ConversionFlags flag)
        {
            return (bitSet & flag) == flag;
        }

        public static int LegacyFormatBitsPerPixelFromConversionFlag(ConversionFlags flags)
        {
            if (HasConversionFlag(flags, ConversionFlags.Pal8))
                return HasConversionFlag(flags, ConversionFlags.FormatA8P8) ? 16 : 8;

            if (HasConversionFlag(flags, ConversionFlags.Format888))
                return 24;

            if (HasConversionFlag(flags, ConversionFlags.Format332))
                return 8;

            if (HasConversionFlag(flags, ConversionFlags.Format8332))
                return 16;

            if (HasConversionFlag(flags, ConversionFlags.Format44))
                return 8;

            if (HasConversionFlag(flags, ConversionFlags.Format4444))
                return 16;

            if (HasConversionFlag(flags, ConversionFlags.Format565))
                return 16;

            if (HasConversionFlag(flags, ConversionFlags.Format5551))
                return 16;

            return 0;
        }

        public static LegacyFormat LegacyFormatFromConversionFlag(ConversionFlags flags)
        {
            if (HasConversionFlag(flags, ConversionFlags.Pal8))
                return HasConversionFlag(flags, ConversionFlags.FormatA8P8) ? LegacyFormat.A8P8 : LegacyFormat.P8;

            if (HasConversionFlag(flags, ConversionFlags.Format888))
                return LegacyFormat.R8G8B8;

            if (HasConversionFlag(flags, ConversionFlags.Format332))
                return LegacyFormat.R3G3B2;

            if (HasConversionFlag(flags, ConversionFlags.Format8332))
                return LegacyFormat.A8R3G3B2;

            if (HasConversionFlag(flags, ConversionFlags.Format44))
                return LegacyFormat.A4L4;

            if (HasConversionFlag(flags, ConversionFlags.Format4444))
                return LegacyFormat.B4G4R4A4;

            if (HasConversionFlag(flags, ConversionFlags.Format565))
                return LegacyFormat.B5G6R5;

            if (HasConversionFlag(flags, ConversionFlags.Format5551))
                return LegacyFormat.B5G5R5A1;

            return LegacyFormat.Unknown;
        }

        public static int GetCompressedBlockSize(DXGIFormat format)
        {
            switch (format)
            {
                case DXGIFormat.BC1_Typeless:
                case DXGIFormat.BC1_UNorm:
                case DXGIFormat.BC1_UNorm_SRGB:
                case DXGIFormat.BC4_Typeless:
                case DXGIFormat.BC4_UNorm:
                case DXGIFormat.BC4_SNorm:
                    return 8;
                default:
                    return 16;
            }
        }

        public static int GetBitsPerPixel(DXGIFormat format)
        {
            switch (format)
            {
                case DXGIFormat.R1_UNorm:
                    return 1;
                case DXGIFormat.BC1_Typeless:
                case DXGIFormat.BC1_UNorm:
                case DXGIFormat.BC1_UNorm_SRGB:
                case DXGIFormat.BC4_SNorm:
                case DXGIFormat.BC4_Typeless:
                case DXGIFormat.BC4_UNorm:
                    return 4;
                case DXGIFormat.A8_UNorm:
                case DXGIFormat.R8_SInt:
                case DXGIFormat.R8_SNorm:
                case DXGIFormat.R8_Typeless:
                case DXGIFormat.R8_UInt:
                case DXGIFormat.R8_UNorm:
                case DXGIFormat.BC2_Typeless:
                case DXGIFormat.BC2_UNorm:
                case DXGIFormat.BC2_UNorm_SRGB:
                case DXGIFormat.BC3_Typeless:
                case DXGIFormat.BC3_UNorm:
                case DXGIFormat.BC3_UNorm_SRGB:
                case DXGIFormat.BC5_SNorm:
                case DXGIFormat.BC5_Typeless:
                case DXGIFormat.BC5_UNorm:
                case DXGIFormat.BC6H_SF16:
                case DXGIFormat.BC6H_Typeless:
                case DXGIFormat.BC6H_UF16:
                case DXGIFormat.BC7_Typeless:
                case DXGIFormat.BC7_UNorm:
                case DXGIFormat.BC7_UNorm_SRGB:
                case DXGIFormat.YUY2:
                    return 8;
                case DXGIFormat.B5G5R5A1_UNorm:
                case DXGIFormat.B5G6R5_UNorm:
                case DXGIFormat.D16_UNorm:
                case DXGIFormat.R16_Float:
                case DXGIFormat.R16_SInt:
                case DXGIFormat.R16_SNorm:
                case DXGIFormat.R16_Typeless:
                case DXGIFormat.R16_UInt:
                case DXGIFormat.R16_UNorm:
                case DXGIFormat.R8G8_SInt:
                case DXGIFormat.R8G8_SNorm:
                case DXGIFormat.R8G8_Typeless:
                case DXGIFormat.R8G8_UInt:
                case DXGIFormat.R8G8_UNorm:
                case DXGIFormat.B4G4R4A4_UNorm:
                    return 16;
                case DXGIFormat.B8G8R8X8_Typeless:
                case DXGIFormat.B8G8R8X8_UNorm:
                case DXGIFormat.B8G8R8X8_UNorm_SRGB:
                case DXGIFormat.D24_UNorm_S8_UInt:
                case DXGIFormat.D32_Float:
                case DXGIFormat.D32_Float_S8X24_UInt:
                case DXGIFormat.G8R8_G8B8_UNorm:
                case DXGIFormat.R10G10B10_XR_Bias_A2_UNorm:
                case DXGIFormat.R10G10B10A2_Typeless:
                case DXGIFormat.R10G10B10A2_UInt:
                case DXGIFormat.R10G10B10A2_UNorm:
                case DXGIFormat.R11G11B10_Float:
                case DXGIFormat.R16G16_Float:
                case DXGIFormat.R16G16_SInt:
                case DXGIFormat.R16G16_SNorm:
                case DXGIFormat.R16G16_Typeless:
                case DXGIFormat.R16G16_UInt:
                case DXGIFormat.R16G16_UNorm:
                case DXGIFormat.R24_UNorm_X8_Typeless:
                case DXGIFormat.R24G8_Typeless:
                case DXGIFormat.R32_Float:
                case DXGIFormat.R32_Float_X8X24_Typeless:
                case DXGIFormat.R32_SInt:
                case DXGIFormat.R32_Typeless:
                case DXGIFormat.R32_UInt:
                case DXGIFormat.R8G8_B8G8_UNorm:
                case DXGIFormat.R8G8B8A8_SInt:
                case DXGIFormat.R8G8B8A8_SNorm:
                case DXGIFormat.R8G8B8A8_Typeless:
                case DXGIFormat.R8G8B8A8_UInt:
                case DXGIFormat.R8G8B8A8_UNorm:
                case DXGIFormat.R8G8B8A8_UNorm_SRGB:
                case DXGIFormat.B8G8R8A8_Typeless:
                case DXGIFormat.B8G8R8A8_UNorm:
                case DXGIFormat.B8G8R8A8_UNorm_SRGB:
                case DXGIFormat.R9G9B9E5_SharedExp:
                case DXGIFormat.X24_Typeless_G8_UInt:
                case DXGIFormat.X32_Typeless_G8X24_UInt:
                    return 32;
                case DXGIFormat.R16G16B16A16_Float:
                case DXGIFormat.R16G16B16A16_SInt:
                case DXGIFormat.R16G16B16A16_SNorm:
                case DXGIFormat.R16G16B16A16_Typeless:
                case DXGIFormat.R16G16B16A16_UInt:
                case DXGIFormat.R16G16B16A16_UNorm:
                case DXGIFormat.R32G32_Float:
                case DXGIFormat.R32G32_SInt:
                case DXGIFormat.R32G32_Typeless:
                case DXGIFormat.R32G32_UInt:
                case DXGIFormat.R32G8X24_Typeless:
                    return 64;
                case DXGIFormat.R32G32B32_Float:
                case DXGIFormat.R32G32B32_SInt:
                case DXGIFormat.R32G32B32_Typeless:
                case DXGIFormat.R32G32B32_UInt:
                    return 96;
                case DXGIFormat.R32G32B32A32_Float:
                case DXGIFormat.R32G32B32A32_SInt:
                case DXGIFormat.R32G32B32A32_Typeless:
                case DXGIFormat.R32G32B32A32_UInt:
                    return 128;
                default:
                    return 0;
            }
        }

        public static bool IsPacked(DXGIFormat format)
        {
            return format == DXGIFormat.R8G8_B8G8_UNorm || format == DXGIFormat.G8R8_G8B8_UNorm || format == DXGIFormat.YUY2;
        }

        public static bool IsCompressed(DXGIFormat format)
        {
            switch (format)
            {
                case DXGIFormat.BC1_Typeless:
                case DXGIFormat.BC1_UNorm:
                case DXGIFormat.BC1_UNorm_SRGB:
                case DXGIFormat.BC2_Typeless:
                case DXGIFormat.BC2_UNorm:
                case DXGIFormat.BC2_UNorm_SRGB:
                case DXGIFormat.BC3_Typeless:
                case DXGIFormat.BC3_UNorm:
                case DXGIFormat.BC3_UNorm_SRGB:
                case DXGIFormat.BC4_Typeless:
                case DXGIFormat.BC4_UNorm:
                case DXGIFormat.BC4_SNorm:
                case DXGIFormat.BC5_Typeless:
                case DXGIFormat.BC5_UNorm:
                case DXGIFormat.BC5_SNorm:
                case DXGIFormat.BC6H_Typeless:
                case DXGIFormat.BC6H_UF16:
                case DXGIFormat.BC6H_SF16:
                case DXGIFormat.BC7_Typeless:
                case DXGIFormat.BC7_UNorm:
                case DXGIFormat.BC7_UNorm_SRGB:
                    return true;
                default:
                    return false;
            }
        }

        public static void CopyScanline(IntPtr dstPtr, int dstPitch, IntPtr srcPtr, int srcPitch, DXGIFormat format, ConversionFlags convFlags, int[] palette = null)
        {
            if (IsCompressed(format))
            {
                MemoryHelper.CopyMemory(dstPtr, srcPtr, Math.Min(dstPitch, srcPitch));
                return;
            }

            ScanlineFlags scanLineFlags = HasConversionFlag(convFlags, ConversionFlags.NoAlpha) ? ScanlineFlags.SetOpaqueAlpha : ScanlineFlags.None;
            if (HasConversionFlag(convFlags, ConversionFlags.Swizzle))
                scanLineFlags |= ScanlineFlags.Legacy;

            if (HasConversionFlag(convFlags, ConversionFlags.Expand))
            {
                LegacyFormat oldFormat = LegacyFormatFromConversionFlag(convFlags);
                bool success = LegacyExpandScanline(dstPtr, dstPitch, format, srcPtr, srcPitch, oldFormat, palette, scanLineFlags);

                System.Diagnostics.Debug.Assert(success);
            }
            else if (HasConversionFlag(convFlags, ConversionFlags.Swizzle))
            {
                SwizzleScanline(dstPtr, dstPitch, srcPtr, srcPitch, format, scanLineFlags);
            }
            else
            {
                CopyScanline(dstPtr, dstPitch, srcPtr, srcPitch, format, scanLineFlags);
            }
        }

        //Adapated from SharpDX Toolkit DDSHelper.cs
        private unsafe static bool LegacyExpandScanline(IntPtr dstPtr, int dstPitch, DXGIFormat dstFormat, IntPtr srcPtr, int srcPitch, LegacyFormat srcFormat, int[] palette, ScanlineFlags flags)
        {
            switch (srcFormat)
            {
                case LegacyFormat.R8G8B8:
                    {
                        if (dstFormat != DXGIFormat.R8G8B8A8_UNorm)
                            return false;

                        // D3DFMT_R8G8B8 -> Format.R8G8B8A8_UNorm

                        byte* sPtr = (byte*)srcPtr.ToPointer();
                        int* dPtr = (int*)dstPtr.ToPointer();

                        for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount += 3, dstCount += 4)
                        {
                            //24 bpp D3D9 are BGR so swizzle as well
                            int t1 = (*(sPtr) << 16);
                            int t2 = (*(sPtr + 1) << 8);
                            int t3 = *(sPtr + 2);

                            *(dPtr++) = (int)(t1 | t2 | t3 | 0xff000000);
                            sPtr += 3;
                        }
                    }
                    return true;
                case LegacyFormat.R3G3B2:
                    switch (dstFormat)
                    {
                        case DXGIFormat.R8G8B8A8_UNorm:
                            {
                                // D3DFMT_R3G3B2 -> Format.R8G8B8A8_UNorm

                                byte* sPtr = (byte*)srcPtr.ToPointer();
                                int* dPtr = (int*)dstPtr.ToPointer();

                                for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount++, dstCount += 4)
                                {
                                    byte t = *(sPtr++);

                                    int t1 = (t & 0xe0) | ((t & 0xe0) >> 3) | ((t & 0xc0) >> 6);
                                    int t2 = ((t & 0x1c) << 11) | ((t & 0x1c) << 8) | ((t & 0x18) << 5);
                                    int t3 = ((t & 0x03) << 22) | ((t & 0x03) << 20) | ((t & 0x03) << 18) | ((t & 0x03) << 16);

                                    *(dPtr++) = (int)(t1 | t2 | t3 | 0xff000000);
                                }
                            }
                            return true;
                        case DXGIFormat.B5G6R5_UNorm:
                            {
                                // D3DFMT_R3G3B2 -> Format.B5G6R5_UNorm

                                byte* sPtr = (byte*)srcPtr.ToPointer();
                                ushort* dPtr = (ushort*)dstPtr.ToPointer();

                                for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount++, dstCount += 2)
                                {
                                    byte t = *(sPtr++);

                                    ushort t1 = (ushort)(((t & 0xe0) << 8) | ((t & 0xc0) << 5));
                                    ushort t2 = (ushort)(((t & 0x1c) << 6) | ((t & 0x1c) << 3));
                                    ushort t3 = (ushort)(((t & 0x03) << 3) | ((t & 0x03) << 1) | ((t & 0x02) >> 1));

                                    *(dPtr++) = (ushort)(t1 | t2 | t3);
                                }
                            }
                            return true;
                    }
                    break;
                case LegacyFormat.A8R3G3B2:
                    {
                        if (dstFormat != DXGIFormat.R8G8B8A8_UNorm)
                            return false;

                        // D3DFMT_A8R3G3B2 -> Format.R8G8B8A8_UNorm

                        short* sPtr = (short*)srcPtr.ToPointer();
                        int* dPtr = (int*)dstPtr.ToPointer();

                        for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount += 2, dstCount += 4)
                        {
                            short t = *(sPtr++);

                            uint t1 = (uint)((t & 0x00e0) | ((t & 0x00e0) >> 3) | ((t & 0x00c0) >> 6));
                            uint t2 = (uint)(((t & 0x001c) << 11) | ((t & 0x001c) << 8) | ((t & 0x0018) << 5));
                            uint t3 = (uint)(((t & 0x0003) << 22) | ((t & 0x0003) << 20) | ((t & 0x0003) << 18) | ((t & 0x0003) << 16));
                            uint ta = HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha) ? 0xff000000 : (uint)((t & 0xff00) << 16);

                            *(dPtr++) = (int)(t1 | t2 | t3 | ta);
                        }
                    }
                    return true;
                case LegacyFormat.P8:
                    {
                        if (dstFormat != DXGIFormat.R8G8B8A8_UNorm || palette == null)
                            return false;

                        // D3DFMT_P8 -> Format.R8G8B8A8_UNorm

                        byte* sPtr = (byte*)srcPtr.ToPointer();
                        int* dPtr = (int*)dstPtr.ToPointer();

                        for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount++, dstCount += 4)
                        {
                            byte index = *(sPtr++);

                            *(dPtr++) = palette[index];
                        }
                    }
                    return true;
                case LegacyFormat.A8P8:
                    {
                        if (dstFormat != DXGIFormat.R8G8B8A8_UNorm || palette == null)
                            return false;

                        // D3DFMT_A8P8 -> Format.R8G8B8A8_UNorm

                        ushort* sPtr = (ushort*)srcPtr.ToPointer();
                        uint* dPtr = (uint*)dstPtr.ToPointer();

                        for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount += 2, dstCount += 4)
                        {
                            ushort t = *(sPtr++);

                            uint t1 = (uint)palette[t & 0xff];
                            uint ta = HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha) ? 0xff000000 : (uint)((t & 0xff00) << 16);

                            *(dPtr++) = t1 | ta;
                        }
                    }
                    return true;
                case LegacyFormat.A4L4:
                    switch (dstFormat)
                    {
                        case DXGIFormat.B4G4R4A4_UNorm:
                            {
                                // D3DFMT_A4L4 -> Format.B4G4R4A4_UNorm 

                                byte* sPtr = (byte*)srcPtr.ToPointer();
                                ushort* dPtr = (ushort*)dstPtr.ToPointer();

                                for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount++, dstCount += 2)
                                {
                                    byte t = *(sPtr++);

                                    ushort t1 = (ushort)(t & 0x0f);
                                    ushort ta = HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha) ? (ushort)0xf000 : (ushort)((t & 0xf0) << 8);

                                    *(dPtr++) = (ushort)(t1 | (t1 << 4) | (t1 << 8) | ta);
                                }
                            }
                            return true;
                        case DXGIFormat.R8G8B8A8_UNorm:
                            {
                                // D3DFMT_A4L4 -> Format.R8G8B8A8_UNorm

                                byte* sPtr = (byte*)srcPtr.ToPointer();
                                uint* dPtr = (uint*)dstPtr.ToPointer();

                                for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount++, dstCount += 4)
                                {
                                    byte t = *(sPtr++);

                                    uint t1 = (uint)(((t & 0x0f) << 4) | (t & 0x0f));
                                    uint ta = HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha) ? 0xff000000 : (uint)(((t & 0xf0) << 24) | ((t & 0xf0) << 20));

                                    *(dPtr++) = t1 | (t1 << 8) | (t1 << 16) | ta;
                                }
                            }
                            return true;
                    }
                    break;
                case LegacyFormat.B4G4R4A4:
                    {
                        if (dstFormat != DXGIFormat.R8G8B8A8_UNorm)
                            return false;

                        // D3DFMT_A4R4G4B4 -> Format.R8G8B8A8_UNorm

                        ushort* sPtr = (ushort*)srcPtr.ToPointer();
                        uint* dPtr = (uint*)dstPtr.ToPointer();

                        for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount += 2, dstCount += 4)
                        {
                            ushort t = *(sPtr++);

                            uint t1 = (uint)(((t & 0x0f00) >> 4) | ((t & 0x0f00) >> 8));
                            uint t2 = (uint)(((t & 0x00f0) << 8) | ((t & 0x00f0) << 4));
                            uint t3 = (uint)(((t & 0x000f) << 20) | ((t & 0x000f) << 16));
                            uint ta = HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha) ? 0xff000000 : (uint)(((t & 0xf000) << 16) | ((t & 0xf000) << 12));

                            *(dPtr++) = t1 | t2 | t3 | ta;
                        }
                    }
                    return true;
                case LegacyFormat.B5G6R5:
                    {
                        if (dstFormat != DXGIFormat.B5G6R5_UNorm)
                            return false;

                        // DXGI.Format.B5G6R5_UNorm -> DXGI.Format.R8G8B8A8_UNorm

                        ushort* sPtr = (ushort*)srcPtr.ToPointer();
                        uint* dPtr = (uint*)dstPtr.ToPointer();

                        for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount += 2, dstCount += 4)
                        {
                            ushort t = *(sPtr++);

                            uint t1 = (uint)(((t & 0xf800) >> 8) | ((t & 0xe000) >> 13));
                            uint t2 = (uint)(((t & 0x07e0) << 5) | ((t & 0x0600) >> 5));
                            uint t3 = (uint)(((t & 0x001f) << 19) | ((t & 0x001c) << 14));

                            *(dPtr++) = t1 | t2 | t3 | 0xff000000;
                        }
                    }
                    return true;
                case LegacyFormat.B5G5R5A1:
                    {
                        if (dstFormat != DXGIFormat.B5G5R5A1_UNorm)
                            return false;

                        // DXGI.Format.B5G5R5A1_UNorm -> DXGI.Format.R8G8B8A8_UNorm

                        ushort* sPtr = (ushort*)srcPtr.ToPointer();
                        uint* dPtr = (uint*)dstPtr.ToPointer();

                        for (int dstCount = 0, srcCount = 0; (srcCount < srcPitch) && (dstCount < dstPitch); srcCount += 2, dstCount += 4)
                        {
                            ushort t = *(sPtr++);

                            uint t1 = (uint)(((t & 0x7c00) >> 7) | ((t & 0x7000) >> 12));
                            uint t2 = (uint)(((t & 0x03e0) << 6) | ((t & 0x0380) << 1));
                            uint t3 = (uint)(((t & 0x001f) << 19) | ((t & 0x001c) << 14));
                            uint ta = HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha) ? 0xff000000 : (uint)(((t & 0x8000) != 0 ? 0xff000000 : 0));

                            *(dPtr++) = t1 | t2 | t3 | ta;
                        }
                    }
                    return true;
            }

            return false;
        }

        //Adapated from SharpDX Toolkit DDSHelper.cs
        private unsafe static void CopyScanline(IntPtr dstPtr, int dstPitch, IntPtr srcPtr, int srcPitch, DXGIFormat format, ScanlineFlags flags)
        {
            //Copy the row, but set the alpha values to one as we go.
            if (HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha))
            {
                switch (format)
                {
                    case DXGIFormat.R32G32B32A32_Typeless:
                    case DXGIFormat.R32G32B32A32_Float:
                    case DXGIFormat.R32G32B32A32_UInt:
                    case DXGIFormat.R32G32B32A32_SInt:
                        {
                            uint alpha;
                            if (format == DXGIFormat.R32G32B32A32_Float)
                                alpha = 0x3f800000;
                            else if (format == DXGIFormat.R32G32B32A32_SInt)
                                alpha = 0x7fffffff;
                            else
                                alpha = 0xffffffff;

                            uint* sPtr = (uint*)srcPtr.ToPointer();
                            uint* dPtr = (uint*)dstPtr.ToPointer();

                            int pitch = Math.Min(dstPitch, srcPitch);

                            for (int count = 0; count < pitch; count += 16)
                            {
                                *(dPtr++) = *(sPtr++);
                                *(dPtr++) = *(sPtr++);
                                *(dPtr++) = *(sPtr++);
                                *(dPtr++) = alpha;
                                sPtr++;
                            }
                        }
                        return;

                    case DXGIFormat.R16G16B16A16_Typeless:
                    case DXGIFormat.R16G16B16A16_Float:
                    case DXGIFormat.R16G16B16A16_UNorm:
                    case DXGIFormat.R16G16B16A16_UInt:
                    case DXGIFormat.R16G16B16A16_SNorm:
                    case DXGIFormat.R16G16B16A16_SInt:
                        {
                            ushort alpha;
                            if (format == DXGIFormat.R16G16B16A16_Float)
                                alpha = 0x3c00;
                            else if (format == DXGIFormat.R16G16B16A16_SNorm || format == DXGIFormat.R16G16B16A16_SInt)
                                alpha = 0x7fff;
                            else
                                alpha = 0xffff;

                            ushort* sPtr = (ushort*)srcPtr.ToPointer();
                            ushort* dPtr = (ushort*)dstPtr.ToPointer();

                            int pitch = Math.Min(dstPitch, srcPitch);

                            for (int count = 0; count < pitch; count += 8)
                            {
                                *(dPtr++) = *(sPtr++);
                                *(dPtr++) = *(sPtr++);
                                *(dPtr++) = *(sPtr++);
                                *(dPtr++) = alpha;
                                sPtr++;
                            }
                        }
                        return;

                    case DXGIFormat.R10G10B10A2_Typeless:
                    case DXGIFormat.R10G10B10A2_UNorm:
                    case DXGIFormat.R10G10B10A2_UInt:
                    case DXGIFormat.R10G10B10_XR_Bias_A2_UNorm:
                        {
                            uint* sPtr = (uint*)srcPtr.ToPointer();
                            uint* dPtr = (uint*)dstPtr.ToPointer();

                            int pitch = Math.Min(dstPitch, srcPitch);

                            for (int count = 0; count < pitch; count += 4)
                            {
                                *(dPtr++) = *(sPtr++) | 0xC0000000;
                            }
                        }
                        return;

                    case DXGIFormat.R8G8B8A8_Typeless:
                    case DXGIFormat.R8G8B8A8_UNorm:
                    case DXGIFormat.R8G8B8A8_UNorm_SRGB:
                    case DXGIFormat.R8G8B8A8_UInt:
                    case DXGIFormat.R8G8B8A8_SNorm:
                    case DXGIFormat.R8G8B8A8_SInt:
                    case DXGIFormat.B8G8R8A8_UNorm:
                    case DXGIFormat.B8G8R8A8_Typeless:
                    case DXGIFormat.B8G8R8A8_UNorm_SRGB:
                        {
                            uint alpha = (format == DXGIFormat.R8G8B8A8_SNorm || format == DXGIFormat.R8G8B8A8_SInt) ? 0x7f000000 : 0xff000000;

                            uint* sPtr = (uint*)srcPtr.ToPointer();
                            uint* dPtr = (uint*)dstPtr.ToPointer();

                            int pitch = Math.Min(dstPitch, srcPitch);

                            for (int count = 0; count < pitch; count += 4)
                            {
                                uint t = *(sPtr++) & 0xFFFFFF;
                                t |= alpha;
                                *(dPtr++) = t;
                            }
                        }
                        return;

                    case DXGIFormat.B5G5R5A1_UNorm:
                        {

                            ushort* sPtr = (ushort*)srcPtr.ToPointer();
                            ushort* dPtr = (ushort*)dstPtr.ToPointer();

                            int pitch = Math.Min(dstPitch, srcPitch);

                            for (int count = 0; count < pitch; count += 2)
                            {
                                *(dPtr++) = (ushort)(*(sPtr++) | 0x8000);
                            }
                        }
                        return;

                    case DXGIFormat.A8_UNorm:
                        MemoryHelper.ClearMemory(dstPtr, 0xff, dstPitch);
                        return;

                    case DXGIFormat.B4G4R4A4_UNorm:
                        {
                            ushort* sPtr = (ushort*)srcPtr.ToPointer();
                            ushort* dPtr = (ushort*)dstPtr.ToPointer();

                            int pitch = Math.Min(dstPitch, srcPitch);

                            for (int count = 0; count < pitch; count += 2)
                            {
                                *(dPtr++) = (ushort)(*(sPtr++) | 0xF000);
                            }
                        }
                        return;
                }
            }

            //Fallback to a copy
            MemoryHelper.CopyMemory(dstPtr, srcPtr, Math.Min(dstPitch, srcPitch));
        }

        //Adapated from SharpDX Toolkit DDSHelper.cs
        private unsafe static void SwizzleScanline(IntPtr dstPtr, int dstPitch, IntPtr srcPtr, int srcPitch, DXGIFormat format, ScanlineFlags flags)
        {
            switch (format)
            {
                case DXGIFormat.R10G10B10A2_Typeless:
                case DXGIFormat.R10G10B10A2_UNorm:
                case DXGIFormat.R10G10B10A2_UInt:
                case DXGIFormat.R10G10B10_XR_Bias_A2_UNorm:
                    {
                        //If legacy, we're swapping red/blue because the format is in D3DFMT_A2R10G10B10. Otherwise just copy
                        if (HasScanlineFlag(flags, ScanlineFlags.Legacy))
                        {
                            uint* sPtr = (uint*)srcPtr.ToPointer();
                            uint* dPtr = (uint*)dstPtr.ToPointer();

                            int pitch = Math.Min(dstPitch, srcPitch);

                            for (int count = 0; count < pitch; count += 4)
                            {
                                uint t = *(sPtr++);

                                uint t1 = (t & 0x3ff00000) >> 20;
                                uint t2 = (t & 0x000003ff) << 20;
                                uint t3 = (t & 0x000ffc00);
                                uint ta = HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha) ? 0xC0000000 : (t & 0xC0000000);

                                *(dPtr++) = t1 | t2 | t3 | ta;
                            }

                            return;
                        }
                    }
                    break;

                case DXGIFormat.R8G8B8A8_Typeless:
                case DXGIFormat.R8G8B8A8_UNorm:
                case DXGIFormat.R8G8B8A8_UNorm_SRGB:
                case DXGIFormat.R8G8B8A8_UInt:
                case DXGIFormat.R8G8B8A8_SNorm:
                case DXGIFormat.R8G8B8A8_SInt:

                case DXGIFormat.B8G8R8A8_Typeless:
                case DXGIFormat.B8G8R8A8_UNorm:
                case DXGIFormat.B8G8R8A8_UNorm_SRGB:

                case DXGIFormat.B8G8R8X8_Typeless:
                case DXGIFormat.B8G8R8X8_UNorm:
                case DXGIFormat.B8G8R8X8_UNorm_SRGB:
                    {
                        //Swap Red and Blue channels

                        uint* sPtr = (uint*)srcPtr.ToPointer();
                        uint* dPtr = (uint*)dstPtr.ToPointer();

                        int pitch = Math.Min(dstPitch, srcPitch);

                        for (int count = 0; count < pitch; count += 4)
                        {
                            uint t = *(sPtr++);

                            uint t1 = (t & 0x00ff0000) >> 16;
                            uint t2 = (t & 0x000000ff) << 16;
                            uint t3 = (t & 0x0000ff00);
                            uint ta = HasScanlineFlag(flags, ScanlineFlags.SetOpaqueAlpha) ? 0xff000000 : (t & 0xFF000000);

                            *(dPtr++) = t1 | t2 | t3 | ta;
                        }
                    }
                    break;
            }

            //Fallback to a copy
            MemoryHelper.CopyMemory(dstPtr, srcPtr, Math.Min(dstPitch, srcPitch));
        }
    }
}
