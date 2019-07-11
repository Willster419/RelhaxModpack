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
    /// Enumerates the type of an image.
    /// </summary>
    public enum ImageType
    {
        /// <summary>
        /// Unknown type. Returned value only, never use as an input value.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Standard image: 1-, 4-, 8-, 16-, 24-, 32-bit.
        /// </summary>
        Bitmap = 1,

        /// <summary>
        /// Array of unsigned 16-bit integers.
        /// </summary>
        UInt16 = 2,

        /// <summary>
        /// Array of signed 16-bit integers.
        /// </summary>
        Int16 = 3,

        /// <summary>
        /// Array of unsigned 32-bit integers.
        /// </summary>
        UInt32 = 4,

        /// <summary>
        /// Array of signed 32-bit integers.
        /// </summary>
        Int32 = 5,

        /// <summary>
        /// Array of 32-bit IEEE floating point.
        /// </summary>
        Float = 6,

        /// <summary>
        /// Array of 64-bit IEEE floating point.
        /// </summary>
        Double = 7,

        /// <summary>
        /// Array of 2 x 64-bit IEEE floating point.
        /// </summary>
        Complex = 8,

        /// <summary>
        /// 48-bit RGB image: 3 channels x 16-bit.
        /// </summary>
        RGB16 = 9,

        /// <summary>
        /// 64-bit RGBA image: 4 channels x 16-bit.
        /// </summary>
        RGBA16 = 10,

        /// <summary>
        /// 96-bit RGB float image: 3 channels x 32-bit IEEE floating point.
        /// </summary>
        RGBF = 11,

        /// <summary>
        /// 128-bit RGBA float image: 4 channels x 32-bit IEEE floating point.
        /// </summary>
        RGBAF = 12
    }

    /// <summary>
    /// Enumerates file formats that can be loaded / saved.
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// Unknown. Returned value only, never use as an input value.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Windows or OS/2 bitmap file (*.bmp).
        /// </summary>
        BMP = 0,

        /// <summary>
        /// Windows Icon (*.ico).
        /// </summary>
        ICO = 1,

        /// <summary>
        /// Independent JPEG group (*.jpg, *.jif, *.jpeg, *.jpe).
        /// </summary>
        JPEG = 2,

        /// <summary>
        /// JPEG network graphics (*.jng).
        /// </summary>
        JNG = 3,

        /// <summary>
        /// Commodore 64 Koala format (*.koa).
        /// </summary>
        KOALA = 4,

        /// <summary>
        /// Amiga IFF (*.iff, *.lbm).
        /// </summary>
        LBM = 5,

        /// <summary>
        /// Amiga IFF (*.iff, *.lb).
        /// </summary>
        IFF = LBM,

        /// <summary>
        /// Multiple network graphics (*.mng).
        /// </summary>
        MNG = 6,

        /// <summary>
        /// Portable bitmap (ASCII) (*.pbm).
        /// </summary>
        PBM = 7,

        /// <summary>
        /// Portable bitmap (BINARY) (*.pbm).
        /// </summary>
        PBMRAW = 8,

        /// <summary>
        /// Kodak PhotoCD (*.pcd).
        /// </summary>
        PCD = 9,

        /// <summary>
        /// Zsoft Paintbrush PCX bitmap (*.pcx).
        /// </summary>
        PCX = 10,

        /// <summary>
        /// Portable gramap (ASCII) (*.pgm).
        /// </summary>
        PGM = 11,

        /// <summary>
        /// Portable graymap (BINARY) (*.pgm).
        /// </summary>
        PGMRAW = 12,

        /// <summary>
        /// Portable network graphics (*.png).
        /// </summary>
        PNG = 13,

        /// <summary>
        /// Portable pixelmap (ASCII) (*.ppm).
        /// </summary>
        PPM = 14,

        /// <summary>
        /// Portable pixelmap (BINARY) (*.ppm).
        /// </summary>
        PPMRAW = 15,

        /// <summary>
        /// Sun Rasterfile (*.ras).
        /// </summary>
        RAS = 16,

        /// <summary>
        /// Truevision Targa file (*.tga, *.targa).
        /// </summary>
        TARGA = 17,

        /// <summary>
        /// Tagged Image File format (*.tif, *.tiff).
        /// </summary>
        TIFF = 18,

        /// <summary>
        /// Wireless bitmap (*.wbmp).
        /// </summary>
        WBMP = 19,

        /// <summary>
        /// Adobe photoshop (*.psd).
        /// </summary>
        PSD = 20,

        /// <summary>
        /// Dr. Halo (*.cut).
        /// </summary>
        CUT = 21,

        /// <summary>
        /// X11 bitmap format (*.xbm).
        /// </summary>
        XBM = 22,

        /// <summary>
        /// X11 pixmap format (*.xpm).
        /// </summary>
        XPM = 23,

        /// <summary>
        /// DirectDraw surface (*.dds).
        /// </summary>
        DDS = 24,

        /// <summary>
        /// Graphics InterChange Format (*.gif).
        /// </summary>
        GIF = 25,

        /// <summary>
        /// High Dynamic Range (*.hdr).
        /// </summary>
        HDR = 26,

        /// <summary>
        /// Raw Fax format CCITT G3 (*.g3).
        /// </summary>
        FAXG3 = 27,

        /// <summary>
        /// Silicon Graphics SGI image format (*.sgi).
        /// </summary>
        SGI = 28,

        /// <summary>
        /// OpenEXR format (*.exr).
        /// </summary>
        EXR = 29,

        /// <summary>
        /// JPEG-2000 format (*.j2k, *.j2c).
        /// </summary>
        J2K = 30,

        /// <summary>
        /// JPEG-2000 format (*.jp2).
        /// </summary>
        JP2 = 31,

        /// <summary>
        /// Portable floatmap (*.pfm).
        /// </summary>
        PFM = 32,

        /// <summary>
        /// Macintosh PICT (*.pict).
        /// </summary>
        PICT = 33,

        /// <summary>
        /// RAW camera image (*.* sometimes .raw).
        /// </summary>
        RAW = 34,

        /// <summary>
        /// Google WebP format (*.webp)
        /// </summary>
        WEBP = 35,

        /// <summary>
        /// JPEG extended range format (*.jxr).
        /// </summary>
        JXR = 36
    }

    /// <summary>
    /// Bitflags representing various options when loading from a variety of formats.
    /// The overlap between options is intentional. If the prefix differs from a given file format
    /// that is to be loaded, then that option will be ignored.
    /// </summary>
    [Flags]
    public enum ImageLoadFlags
    {
        /// <summary>
        /// Default loading procedure.
        /// </summary>
        Default = 0,


        /// <summary>
        /// Loads the GIF as a 256 color image with unused palette entries, if it's
        /// 16 or 2 color.
        /// </summary>
        GIF_Load256 = 1,

        /// <summary>
        /// "Play" the GIF to generate each frame (as 32bpp) instead of returning
        /// raw frame data when loading.
        /// </summary>
        GIF_Playback = 2,


        /// <summary>
        /// Convert to 32bpp and create an alpha channel from the AND-mask when
        /// loading.
        /// </summary>
        ICO_MakeAlpha = 1,


        /// <summary>
        /// Load the JPEG file as fast as possible, sacrificing some quality.
        /// </summary>
        JPEG_Fast = 1,

        /// <summary>
        /// Load the JPEG file with the best quality, sacrificing some speed.
        /// </summary>
        JPEG_Accurate = 2,

        /// <summary>
        /// Load the JPEG file with separated CMYK "as is".
        /// </summary>
        JPEG_CMYK = 4,

        /// <summary>
        /// Load and rotate JPEG file according to Exif 'Orientation' tag if available.
        /// </summary>
        JPEG_ExifRotate = 8,

        /// <summary>
        /// Load and convert JPEG file to an 8-bit greyscale image.
        /// </summary>
        JPEG_Greyscale = 16,


        /// <summary>
        /// Load PCD bitmap sized 768 x 512.
        /// </summary>
        PCD_Base = 1,

        /// <summary>
        /// Load PCD bitmap sized 384 x 256.
        /// </summary>
        PCD_BaseDiv4 = 2,

        /// <summary>
        /// Load PCD bitmap sized 192 x 128.
        /// </summary>
        PCD_BaseDiv16 = 3,


        /// <summary>
        /// Avoid gamma correction when loading PNG files.
        /// </summary>
        PNG_IgnoreGamma = 1,


        /// <summary>
        /// Try to load the RAW file's embedded JPEG preview with included Exif data or default to RGB 24-bit.
        /// </summary>
        RAW_Preview = 1,

        /// <summary>
        /// Try to load RAW as 24-bit RGB.
        /// </summary>
        RAW_Display = 2,


        /// <summary>
        /// When loading TARGA files, convert RGB555 and ARGB8888 to RGB888.
        /// </summary>
        TARGA_LoadRGB888 = 1,


        /// <summary>
        /// Reads and stores tags for separated CMYK when loading TIFF files.
        /// </summary>
        TIFF_CMYK = 1,


        /// <summary>
        /// Reads tags for separated CMYK (default is conversion to RGB) when loading PSD files.
        /// </summary>
        PSD_CMYK = 1,

        /// <summary>
        /// Reads tags for CIELab (default is conversion to RGB) when loading PSD files.
        /// </summary>
        PSD_Lab = 2
    }

    /// <summary>
    /// Bitflags representing various options when saving to a variety of formats.
    /// The overlap between options is intentional. If the prefix differs from a given file format
    /// that is to be saved, then that option will be ignored.
    /// </summary>
    [Flags]
    public enum ImageSaveFlags
    {
        /// <summary>
        /// Default saving procedure.
        /// </summary>
        Default = 0,


        /// <summary>
        /// Saves a BMP with run length encoding.
        /// </summary>
        BMP_SaveRLE = 1,


        /// <summary>
        /// Save EXR format as float instead of as half (not recommended).
        /// </summary>
        EXR_Float = 1,

        /// <summary>
        /// Save EXR format with no compression.
        /// </summary>
        EXR_None = 2,

        /// <summary>
        /// Save EXR format with zlib compression, in blocks of 16 scan lines.
        /// </summary>
        EXR_Zip = 4,

        /// <summary>
        /// Save EXR format with piz-based wavelet compression.
        /// </summary>
        EXR_Piz = 8,

        /// <summary>
        /// Save EXR format with lossy 24-bit float compression.
        /// </summary>
        EXR_PXR24 = 16,

        /// <summary>
        /// Save EXR format with lossy 44% float compression - goes to 22% when combined with <see cref="ImageSaveFlags.EXR_LC"/>.
        /// </summary>
        EXR_B44 = 32,

        /// <summary>
        /// Save EXR format with one luminance and two chroma channels, rather than as RGB (lossy compression).
        /// </summary>
        EXR_LC = 64,


        /// <summary>
        /// Save JPEG format with superb quality (100:1).
        /// </summary>
        JPEG_QualitySuperb = 128,

        /// <summary>
        /// Save JPEG format with good quality (75:1).
        /// </summary>
        JPEG_QualityGood = 256,

        /// <summary>
        /// Save JPEG format with normal quality (50:1).
        /// </summary>
        JPEG_QualityNormal = 512,

        /// <summary>
        /// Save JPEG format with average quality (25:1).
        /// </summary>
        JPEG_QualityAverage = 1024,

        /// <summary>
        /// Save JPEG format with bad quality (10:1).
        /// </summary>
        JPEG_QualityBad = 2048,

        /// <summary>
        /// Save JPEG format as a progressive-jpeg.
        /// </summary>
        JPEG_Progressive = 8192,

        /// <summary>
        /// Save JPEG format with high 4x1 chroma subsampling (4:1:1).
        /// </summary>
        JPEG_Subsampling_411 = 4096,

        /// <summary>
        /// Save JPEG format with medium 2x2 medium chroma subsampling (4:2:0) - default value.
        /// </summary>
        JPEG_Subsampling_420 = 16384,

        /// <summary>
        /// Save JPEG format with low 2x1 chroma subsampling (4:2:2).
        /// </summary>
        JPEG_Subsampling_422 = 32768,

        /// <summary>
        /// Save JPEG format with no chroma subsampling (4:4:4).
        /// </summary>
        JPEG_Subsampling_444 = 65536,

        /// <summary>
        /// When saving JPEG format, compute optimal Huffman coding tables (can reduce file size by a few percent).
        /// </summary>
        JPEG_Optimize = 131072,

        /// <summary>
        /// Save basic JPEG format, without metadata or any markers.
        /// </summary>
        JPEG_Baseline = 262144,


        /// <summary>
        /// Save PNG format using Zlib level 1 compression.
        /// </summary>
        PNG_Z_BestSpeed = 1,

        /// <summary>
        /// Save PNG format using Zlib level 6 compression. (Default value).
        /// </summary>
        PNG_Z_DefaultCompression = 6,

        /// <summary>
        /// Save PNG format using ZLib level 9 compression.
        /// </summary>
        PNG_Z_BestCompression = 9,

        /// <summary>
        /// Save PNG format without ZLib compression.
        /// </summary>
        PNG_Z_NoCompression = 256,

        /// <summary>
        /// Save PNG format using Adam7 interlacing.
        /// </summary>
        PNG_Interlaced = 512,


        /// <summary>
        /// Save PNM format in ASCII format.
        /// </summary>
        PNM_SaveAscii = 1,


        /// <summary>
        /// Save RAW format as half-size color image.
        /// </summary>
        RAW_HalfSize = 4,

        /// <summary>
        /// Save RAW format as a UInt16 raw Bayer image.
        /// </summary>
        RAW_Unprocessed = 8,


        /// <summary>
        /// Save TARGA format with RLE compression.
        /// </summary>
        TARGA_SaveRLE = 2,

        /// <summary>
        /// Saves TIFF format with packbits compression.
        /// </summary>
        TIFF_PackBits = 256,

        /// <summary>
        /// Saves TIFF format with Deflate compression (aka ZLib compression).
        /// </summary>
        TIFF_Deflate = 512,

        /// <summary>
        /// Saves TIFF format with Adobe Deflate compression.
        /// </summary>
        TIFF_AdobeDeflate = 1024,

        /// <summary>
        /// Saves TIFF format without any compression.
        /// </summary>
        TIFF_None = 2048,

        /// <summary>
        /// Saves TIFF format using CCITT Group 3 fax encoding.
        /// </summary>
        TIFF_CCITTFAX3 = 4096,

        /// <summary>
        /// Saves TIFF format using CCITT Group 4 fax encoding.
        /// </summary>
        TIFF_CCITTFAX4 = 8192,

        /// <summary>
        /// Saves TIFF format using LZW compression.
        /// </summary>
        TIFF_LZW = 16384,

        /// <summary>
        /// Saves TIFF format using JPEG compression.
        /// </summary>
        TIFF_JPEG = 32768,

        /// <summary>
        /// Saves TIFF format using LogLuv compression.
        /// </summary>
        TIFF_LogLuv = 65536,


        /// <summary>
        /// Saves WEBP format with lossless quality.
        /// </summary>
        WEBP_Lossless = 256,


        /// <summary>
        /// Saves JXR format with lossless quality.
        /// </summary>
        JXR_Lossless = 100,

        /// <summary>
        /// Saves JXR format as a progressive-JXR.
        /// </summary>
        JXR_Progressive = 8192
    }

    /// <summary>
    /// Enumerates image color models.
    /// </summary>
    public enum ImageColorType
    {
        /// <summary>
        /// Minimum value is white.
        /// </summary>
        MinIsWhite = 0,

        /// <summary>
        /// Minimum value is black.
        /// </summary>
        MinIsBlack = 1,

        /// <summary>
        /// RGB color model.
        /// </summary>
        RGB = 2,

        /// <summary>
        /// Colors indexed in a palette.
        /// </summary>
        Palette = 3,

        /// <summary>
        /// RGB color model with alpha channel.
        /// </summary>
        RGBA = 4,

        /// <summary>
        /// CMYK color model.
        /// </summary>
        CMYK = 5
    }

    /// <summary>
    /// Enumerates image filters used only in FreeImage (and NOT the Compression API).
    /// </summary>
    public enum ImageFilter
    {
        /// <summary>
        /// Box, pulse, Fourier window, 1st order (constant) b-spline
        /// </summary>
        Box = 0,

        /// <summary>
        /// Mitchell and Netravali's two-param cubic filter.
        /// </summary>
        Bicubic = 1,

        /// <summary>
        /// Bilinear filter.
        /// </summary>
        Bilinear = 2,

        /// <summary>
        /// 4th order (cubic) b-spline
        /// </summary>
        Bspline = 3,

        /// <summary>
        /// Catmull-Rom spline, Overhauser spline
        /// </summary>
        CatmullRom = 4,

        /// <summary>
        /// Lanczos3 filter
        /// </summary>
        Lanczos3 = 5
    }

    /// <summary>
    /// Enumerates types of image conversions.
    /// </summary>
    public enum ImageConversion
    {
        /// <summary>
        /// Converts a bitmap to 4 bits. If the bitmap was a high-color bitmap
        /// (16, 24, or 32-bit) or if it was a monochrome or greyscale bitmap (1 or 8-bit), the end result
        /// will be a greyscale bitmap, otherwise (1-bit palletised bitmaps) it will be a palletised bitmap.
        /// </summary>
        To4Bits,

        /// <summary>
        /// Converts a bitmap to 8 bits. If the bitmap was a high-color bitmap
        /// (16, 24, or 32-bit) or if it was a monochrome or greyscale bitmap (1 or 4-bit), the end result
        /// will be a greyscale bitmap, otherwise (1-bit palletised bitmaps) it will be a palletised bitmap.
        /// </summary>
        To8Bits,

        /// <summary>
        /// Converts a bitmap to 16 bits, where each pixel has a color pattern of 5 bits red, 5 bits green,
        /// and 5 bits blue. One bit in each pixel is unused.
        /// </summary>
        To16Bits555,

        /// <summary>
        /// Converts a bitmap to 16 bits, where each pixel has a color pattern of 5 bits red, 6 bits green,
        /// and 5 bits blue.
        /// </summary>
        To16Bits565,

        /// <summary>
        /// Converts a bitmap to 24 bits.
        /// </summary>
        To24Bits,

        /// <summary>
        /// Converts a bitmap to 32 bits.
        /// </summary>
        To32Bits,

        /// <summary>
        /// Converts a bitmap to an 8-bit greyscale image with a linear ramp.
        /// </summary>
        ToGreyscale,

        /// <summary>
        /// Converts a bitmap to an array of 32-bit IEEE floating point values.
        /// </summary>
        ToFloat,

        /// <summary>
        /// Converts a bitmap to an array of unsigned 16-bit integers.
        /// </summary>
        ToUInt16,

        /// <summary>
        /// Converts a bitmap to a 96-bit RGB float image, where 3 channels x 32-bit IEEE floating point.
        /// </summary>
        ToRGBF,

        /// <summary>
        /// Converts a bitmap to a 128-bit RGBA float image, where 4 channels x 32-bit IEEE floating point.
        /// </summary>
        ToRGBAF,

        /// <summary>
        /// Converts a bitmap to a 48-bit RGB image, where 3 channels x 16-bit.
        /// </summary>
        ToRGB16,

        /// <summary>
        /// Converts a bitmap to a 64-bit RGBA image, where 4 channels x 16-bit.
        /// </summary>
        ToRGBA16
    }
}