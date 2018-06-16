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
using System.Runtime.InteropServices;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.DDS
{
    #region Enums

    internal enum D3D10ResourceDimension : uint
    {
        Unknown = 0,
        Buffer = 1,
        Texture1D = 2,
        Texture2D = 3,
        Texture3D = 4
    }

    [Flags]
    internal enum HeaderFlags : uint
    {
        None = 0,
        Caps = 0x1,
        Height = 0x2,
        Width = 0x4,
        Pitch = 0x8,
        PixelFormat = 0x1000,
        MipMapCount = 0x20000,
        LinearSize = 0x80000,
        Depth = 0x800000
    }

    [Flags]
    internal enum HeaderCaps : uint
    {
        None = 0,
        Complex = 0x8,
        Texture = 0x1000,
        MipMap = 0x400000
    }

    [Flags]
    internal enum HeaderCaps2 : uint
    {
        None = 0,
        Cubemap = 0x200,
        Cubemap_PositiveX = Cubemap | 0x400,
        Cubemap_NegativeX = Cubemap | 0x800,
        Cubemap_PositiveY = Cubemap | 0x1000,
        Cubemap_NegativeY = Cubemap | 0x2000,
        Cubemap_PositiveZ = Cubemap | 0x4000,
        Cubemap_NegativeZ = Cubemap | 0x8000,
        Cubemap_AllFaces = Cubemap_PositiveX | Cubemap_NegativeX | Cubemap_PositiveY | Cubemap_NegativeY | Cubemap_PositiveZ | Cubemap_NegativeZ,
        Volume = 0x200000
    }

    [Flags]
    internal enum Header10Flags : uint
    {
        None = 0,
        TextureCube = 0x4
    }

    [Flags]
    internal enum Header10Flags2 : uint
    {
        None = 0,
        AlphaModeStraight = 0x1,
        AlphaModePremultiplied = 0x2,
        AlphaModeOpaque = 0x3,
        AlphaModeCustom = 0x4
    }

    [Flags]
    internal enum PixelFormatFlags : uint
    {
        None = 0,
        AlphaPixels = 0x1, //Has an alpha channel
        Alpha = 0x2, //ONLY has alpha data, some old files use this
        FourCC = 0x4,
        RGB = 0x40,
        RGBA = RGB | AlphaPixels,
        YUV = 0x200,
        Luminance = 0x20000,
        LuminanceAlpha = Luminance | AlphaPixels,
        Pal8 = 0x00000020,
        Pal8Alpha = Pal8 | AlphaPixels,
        BumpDUDV = 0x00080000
    }

    #endregion

    #region Structures

    /// <summary>
    /// Header for a DDS file, comes right after the "DDS " magic word. If pixel format is set to use extended header, that comes right after this and then the data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("Width = {Width}, Height = {Height}, Depth = {Depth}, Flags = {Flags}, MipCount = {MipMapCount}, Caps = {Caps}, Caps2 = {Caps2}, PixelFormat = {PixelFormat}")]
    internal unsafe struct Header
    {
        public uint Size;
        public HeaderFlags Flags;
        public uint Height;
        public uint Width;
        public uint PitchOrLinearSize;
        public uint Depth;
        public uint MipMapCount;
        public fixed uint Reserved1[11];
        public PixelFormat PixelFormat;
        public HeaderCaps Caps;
        public HeaderCaps2 Caps2;
        public uint Caps3;
        public uint Caps4;
        public uint Reserved2;
    }

    /// <summary>
    /// Represents the extended header in a DDS header, which may or may not exist. This specifies array textures and expresses DXGI formats rather than D3D9 legacy formats defined in <see cref="PixelFormat"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("Format = {Format}, Dimension = {ResourceDimension}, ArraySize = {ArraySize}, MiscFlags = {MiscFlag}, MiscFlags2 = {MiscFlags2}")]
    internal struct Header10
    {
        public DXGIFormat Format;
        public D3D10ResourceDimension ResourceDimension;
        public Header10Flags MiscFlags;
        public uint ArraySize;
        public Header10Flags2 MiscFlags2;

        public Header10(DXGIFormat format, D3D10ResourceDimension resourceDim, Header10Flags miscFlags, uint arraySize, Header10Flags2 miscFlags2)
        {
            Format = format;
            ResourceDimension = resourceDim;
            MiscFlags = miscFlags;
            ArraySize = arraySize;
            MiscFlags2 = miscFlags2;
        }
    }

    /// <summary>
    /// Represents the pixel format in a DDS header. This is mostly a legacy format description, as if the DDS header has "DX10" extended information, the <see cref="DXGIFormat"/>
    /// enumeration is used.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct PixelFormat
    {
        public uint Size;
        public PixelFormatFlags Flags;
        public FourCC FourCC;
        public uint RGBBitCount;
        public uint RedBitMask;
        public uint GreenBitMask;
        public uint BlueBitMask;
        public uint AlphaBitMask;

        /// <summary>
        /// Gets a value indicating whether this format description is extended.
        /// </summary>
        public bool IsDX10Extended
        {
            get
            {
                return FourCC == new FourCC('D', 'X', '1', '0');
            }
        }

        public PixelFormat(PixelFormatFlags flags, FourCC formatCode, uint colorBitCount, uint redMask, uint greenMask, uint blueMask, uint alphaMask)
        {
            Size = (uint)MemoryHelper.SizeOf<PixelFormat>();
            Flags = flags;
            FourCC = formatCode;
            RGBBitCount = colorBitCount;
            RedBitMask = redMask;
            GreenBitMask = greenMask;
            BlueBitMask = blueMask;
            AlphaBitMask = alphaMask;
        }

        public PixelFormat(PixelFormatFlags flags, FourCC formatCode)
        {
            Size = (uint)MemoryHelper.SizeOf<PixelFormat>();
            Flags = flags;
            FourCC = formatCode;
            RGBBitCount = 0;
            RedBitMask = 0;
            GreenBitMask = 0;
            BlueBitMask = 0;
            AlphaBitMask = 0;
        }

        public override string ToString()
        {
            if (Flags == PixelFormatFlags.FourCC)
            {
                //D3D Format enum is sometimes the first char, if so then it's not a proper fourCC and will just appear as a single character. Lets display the integer value instead in this case.
                String fourCCValue = (FourCC.First > 0 && FourCC.Second == 0 && FourCC.Third == 0 && FourCC.Fourth == 0) ? ((int)FourCC.First).ToString() : FourCC.ToString();

                return String.Format("Flags = {0}, FourCC = '{1}'", Flags.ToString(), fourCCValue);
            }

            return String.Format("Flags = {0}, BitsPerPixel = {1}, RedMask = 0x{2}, GreenMask = 0x{3}, BlueMask = 0x{4}, AlphaMask = 0x{5}", Flags.ToString(), RGBBitCount.ToString(),
                RedBitMask.ToString("X8"), GreenBitMask.ToString("X8"), BlueBitMask.ToString("X8"), AlphaBitMask.ToString("X8"));
        }

        //Not a format, but signifies if there exists an extended header with DXGI format enum
        public static readonly PixelFormat DX10Extended = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', '1', '0'));

        #region Common Legacy Formats

        //Most of these we'll write out to maximize compatability with old tools (unless noted). According to MSDN, old D3D format names should be read right to left (least to most significant bits).

        public static readonly PixelFormat DXT1 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '1'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat DXT2 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '2'), 0, 0, 0, 0, 0); //Rarely used, maps to DXT3

        public static readonly PixelFormat DXT3 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '3'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat DXT4 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '4'), 0, 0, 0, 0, 0); //Rarely used, maps to DXT5

        public static readonly PixelFormat DXT5 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '5'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat BC4_UNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('B', 'C', '4', 'U'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat BC4_SNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('B', 'C', '4', 'S'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat BC5_UNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('B', 'C', '5', 'U'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat BC5_SNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('B', 'C', '5', 'S'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat R8G8_B8G8 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('R', 'G', 'B', 'G'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat G8R8_G8B8 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('G', 'R', 'G', 'B'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat A8R8G8B8 = new PixelFormat(PixelFormatFlags.RGBA, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000);

        public static readonly PixelFormat X8R8G8B8 = new PixelFormat(PixelFormatFlags.RGB, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);

        public static readonly PixelFormat A8B8G8R8 = new PixelFormat(PixelFormatFlags.RGBA, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000);

        public static readonly PixelFormat X8B8G8R8 = new PixelFormat(PixelFormatFlags.RGB, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000); //Use DX10, Maps to DXGI R8G8B8A8_UNorm

        public static readonly PixelFormat G16R16 = new PixelFormat(PixelFormatFlags.RGB, 0, 32, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000);

        public static readonly PixelFormat R5G6B5 = new PixelFormat(PixelFormatFlags.RGB, 0, 16, 0x0000f800, 0x000007e0, 0x0000001f, 0x00000000);

        public static readonly PixelFormat A1R5G5B5 = new PixelFormat(PixelFormatFlags.RGBA, 0, 16, 0x00007c00, 0x000003e0, 0x0000001f, 0x00008000);

        public static readonly PixelFormat A4R4G4B4 = new PixelFormat(PixelFormatFlags.RGBA, 0, 16, 0x00000f00, 0x000000f0, 0x0000000f, 0x0000f000);

        public static readonly PixelFormat R8G8B8 = new PixelFormat(PixelFormatFlags.RGB, 0, 24, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000); //Use DX10, Maps to R8G8B8A8_UNorm with opaque alpha

        public static readonly PixelFormat L8 = new PixelFormat(PixelFormatFlags.Luminance, 0, 8, 0xff, 0x00, 0x00, 0x00);

        public static readonly PixelFormat L16 = new PixelFormat(PixelFormatFlags.Luminance, 0, 16, 0xffff, 0x0000, 0x0000, 0x0000);

        public static readonly PixelFormat A8L8 = new PixelFormat(PixelFormatFlags.LuminanceAlpha, 0, 16, 0x00ff, 0x0000, 0x0000, 0xff00);

        public static readonly PixelFormat A8 = new PixelFormat(PixelFormatFlags.Alpha, 0, 8, 0x00, 0x00, 0x00, 0xff);


        //Some common legacy D3DX formats that use D3DFMT enum value as FourCC. The names correspond to the DXGI format name.

        public static readonly PixelFormat R32G32B32A32_Float = new PixelFormat(PixelFormatFlags.FourCC, new FourCC(116)); // D3DFMT_A32B32G32R32F

        public static readonly PixelFormat R16G16B16A16_Float = new PixelFormat(PixelFormatFlags.FourCC, new FourCC(113)); // D3DFMT_A16B16G16R16F

        public static readonly PixelFormat R16G16B16A16_UNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC(36)); // D3DFMT_A16B16G16R16

        public static readonly PixelFormat R16G16B16A16_SNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC(110)); // D3DFMT_Q16W16V16U16

        public static readonly PixelFormat R32G32_Float = new PixelFormat(PixelFormatFlags.FourCC, new FourCC(115)); // D3DFMT_G32R32F

        public static readonly PixelFormat R16G16_Float = new PixelFormat(PixelFormatFlags.FourCC, new FourCC(112)); // D3DFMT_G16R16F

        public static readonly PixelFormat R32_Float = new PixelFormat(PixelFormatFlags.FourCC, new FourCC(114)); // D3DFMT_R32F

        public static readonly PixelFormat R16_Float = new PixelFormat(PixelFormatFlags.FourCC, new FourCC(111)); // D3DFMT_R16F

        #endregion
    }

    #endregion
}
