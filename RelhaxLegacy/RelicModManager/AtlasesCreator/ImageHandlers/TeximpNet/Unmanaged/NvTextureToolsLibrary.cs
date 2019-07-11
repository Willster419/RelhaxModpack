using RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.Compression;
using RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.Unmanaged;
using System;
using System.Runtime.InteropServices;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet
{
    /// <summary>
    /// When the <see cref="Compressor"/> is processing, this will be called at the beginning of new image data.
    /// </summary>
    /// <param name="size">Total size of the image in bytes.</param>
    /// <param name="width">Width of the image.</param>
    /// <param name="height">Height of the image.</param>
    /// <param name="depth">Depth of the image.</param>
    /// <param name="face">Cubemap face or 2D array index.</param>
    /// <param name="mipLevel">Mipmap level.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void BeginImageHandler(int size, int width, int height, int depth, int face, int mipLevel);

    /// <summary>
    /// When the <see cref="Compressor"/> is processing, this will be called to write image data.
    /// </summary>
    /// <param name="data">Byte pointer containing the data.</param>
    /// <param name="size">Number of bytes to write.</param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool OutputHandler(IntPtr data, int size);

    /// <summary>
    /// When the <see cref="Compressor"/> is processing, this will be called at the end of new image data.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void EndImageHandler();

    /// <summary>
    /// Manages the lifetime and access to the Nvidia Texture Tools (NVTT) native library.
    /// </summary>
    public sealed class NvTextureToolsLibrary : UnmanagedLibrary
    {
        private static readonly Object s_sync = new Object();

        /// <summary>
        /// Default name of the 32-bit unmanaged library. Based on runtime implementation the prefix ("lib" on non-windows) and extension (.dll, .so, .dylib) will be appended automatically.
        /// </summary>
        private const String Default32BitName = "nvtt32";

        /// <summary>
        /// Default name of the 64-bit unmanaged library. Based on runtime implementation the prefix ("lib" on non-windows) and extension (.dll, .so, .dylib) will be appended automatically.
        /// </summary>
        private const String Default64BitName = "nvtt64";

        private static NvTextureToolsLibrary s_instance;

        /// <summary>
        /// Gets the instance of the NVTT library. This is thread-safe.
        /// </summary>
        public static NvTextureToolsLibrary Instance
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

        private NvTextureToolsLibrary(String default32BitName, String default64BitName, Type[] unmanagedFunctionDelegateTypes)
            : base(default32BitName, default64BitName, unmanagedFunctionDelegateTypes) { }

        private static NvTextureToolsLibrary CreateInstance()
        {
            return new NvTextureToolsLibrary(Default32BitName, Default64BitName, PlatformHelper.GetNestedTypes(typeof(Functions)));
        }

        #region Input options

        /// <summary>
        /// Create an input option object. This manages the compressor input images and other options.
        /// </summary>
        /// <returns>Pointer to input options object.</returns>
        public IntPtr CreateInputOptions()
        {
            LoadIfNotLoaded();

            Functions.nvttCreateInputOptions func = GetFunction<Functions.nvttCreateInputOptions>(FunctionNames.nvttCreateInputOptions);

            return func();
        }

        /// <summary>
        /// Destroy an input option object.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        public void DestroyInputOptions(IntPtr inputOptions)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttDestroyInputOptions func = GetFunction<Functions.nvttDestroyInputOptions>(FunctionNames.nvttDestroyInputOptions);

            func(inputOptions);
        }

        /// <summary>
        /// Sets the texture layout on the input options.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="type">Type of texture.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="depth">Depth of the image.</param>
        public void SetInputOptionsTextureLayout(IntPtr inputOptions, TextureType type, int width, int height, int depth)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsTextureLayout func = GetFunction<Functions.nvttSetInputOptionsTextureLayout>(FunctionNames.nvttSetInputOptionsTextureLayout);

            func(inputOptions, type, width, height, depth);
        }

        /// <summary>
        /// Reset the texture layout of the input option to default value.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        public void ResetInputOptionsTextureLayout(IntPtr inputOptions)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttResetInputOptionsTextureLayout func = GetFunction<Functions.nvttResetInputOptionsTextureLayout>(FunctionNames.nvttResetInputOptionsTextureLayout);

            func(inputOptions);
        }

        /// <summary>
        /// Sets mipmap image data to the input options that will be processed by the compressor. The texture layout must be first set, then each individual image representing
        /// the complete image must be set (all the necessary faces and/or mip levels). The data is copied, so it is safe to free the memory after control is returned.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="data">Pointer to NON-PADDED data, essentially an array of 2D or 3D data.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="depth">Depth of the image.</param>
        /// <param name="face">Array index or cubemap face.</param>
        /// <param name="mipmap">Mipmap level</param>
        /// <returns>True if the data was successfully set, false otherwise.</returns>
        public bool SetInputOptionsMipmapData(IntPtr inputOptions, IntPtr data, int width, int height, int depth, int face, int mipmap)
        {
            if (inputOptions == IntPtr.Zero || data == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsMipmapData func = GetFunction<Functions.nvttSetInputOptionsMipmapData>(FunctionNames.nvttSetInputOptionsMipmapData);

            return TranslateBool(func(inputOptions, data, width, height, depth, face, mipmap));
        }

        /// <summary>
        /// Sets the pixel format of data that will be set as input to the compressor.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="pixelFormat">Pixel format enumeration.</param>
        public void SetInputOptionsFormat(IntPtr inputOptions, int pixelFormat)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsFormat func = GetFunction<Functions.nvttSetInputOptionsFormat>(FunctionNames.nvttSetInputOptionsFormat);

            func(inputOptions, (NvttInputFormat)pixelFormat);
        }

        /// <summary>
        /// Sets the alpha mode to the input options.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="alphaMode">Alpha mode enumeration.</param>
        public void SetInputOptionsAlphaMode(IntPtr inputOptions, AlphaMode alphaMode)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsAlphaMode func = GetFunction<Functions.nvttSetInputOptionsAlphaMode>(FunctionNames.nvttSetInputOptionsAlphaMode);

            func(inputOptions, alphaMode);
        }

        /// <summary>
        /// Sets gamma options to the input options.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="inputGamma">Input gamma.</param>
        /// <param name="outputGamma">Output gamma.</param>
        public void SetInputOptionsGamma(IntPtr inputOptions, float inputGamma, float outputGamma)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsGamma func = GetFunction<Functions.nvttSetInputOptionsGamma>(FunctionNames.nvttSetInputOptionsGamma);

            func(inputOptions, inputGamma, outputGamma);
        }

        /// <summary>
        /// Sets wrap mode options to the input options.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="wrapMode">Wrap mode enumeration.</param>
        public void SetInputOptionsWrapMode(IntPtr inputOptions, WrapMode wrapMode)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsWrapMode func = GetFunction<Functions.nvttSetInputOptionsWrapMode>(FunctionNames.nvttSetInputOptionsWrapMode);

            func(inputOptions, wrapMode);
        }

        /// <summary>
        /// Sets the mipmap filtering to be used by the compressor during mipmap generation.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="filter">Filter enumeration.</param>
        public void SetInputOptionsMipmapFilter(IntPtr inputOptions, MipmapFilter filter)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsMipmapFilter func = GetFunction<Functions.nvttSetInputOptionsMipmapFilter>(FunctionNames.nvttSetInputOptionsMipmapFilter);

            func(inputOptions, filter);
        }

        /// <summary>
        /// Sets if mipmaps should be generated by the compressor.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="isEnabled">True if mipmaps should be generated, false if otherwise.</param>
        /// <param name="maxLevel">Maximum # of mipmaps to generate.</param>
        public void SetInputOptionsMipmapGeneration(IntPtr inputOptions, bool isEnabled, int maxLevel)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsMipmapGeneration func = GetFunction<Functions.nvttSetInputOptionsMipmapGeneration>(FunctionNames.nvttSetInputOptionsMipmapGeneration);

            func(inputOptions, (isEnabled) ? NvttBool.True : NvttBool.False, maxLevel);
        }

        /// <summary>
        /// Sets kaiser filter parameters when the mipmap filter is set to <see cref="MipmapFilter.Kaiser"/>.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="width">Width parameter.</param>
        /// <param name="alpha">Alpha parameter.</param>
        /// <param name="stretch">Stretch parameter.</param>
        public void SetInputOptionsKaiserParameters(IntPtr inputOptions, float width, float alpha, float stretch)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsKaiserParameters func = GetFunction<Functions.nvttSetInputOptionsKaiserParameters>(FunctionNames.nvttSetInputOptionsKaiserParameters);

            func(inputOptions, width, alpha, stretch);
        }

        /// <summary>
        /// Sets if the input image(s) should be treated if they're normal maps.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="isNormalMap">True if normal map, false if otherwise.</param>
        public void SetInputOptionsNormalMap(IntPtr inputOptions, bool isNormalMap)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsNormalMap func = GetFunction<Functions.nvttSetInputOptionsNormalMap>(FunctionNames.nvttSetInputOptionsNormalMap);

            func(inputOptions, (isNormalMap) ? NvttBool.True : NvttBool.False);
        }

        /// <summary>
        /// Specifies the input options that the images should be converted to normal maps.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="convertToNormalMap">True to convert the input images to normal maps, false if otherwise.</param>
        public void SetInputOptionsConvertToNormalMap(IntPtr inputOptions, bool convertToNormalMap)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsConvertToNormalMap func = GetFunction<Functions.nvttSetInputOptionsConvertToNormalMap>(FunctionNames.nvttSetInputOptionsConvertToNormalMap);

            func(inputOptions, (convertToNormalMap) ? NvttBool.True : NvttBool.False);
        }

        /// <summary>
        /// Sets height evaluation parameters for use in normal map generation. The height factors do not
        /// necessarily sum to one.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="redScale">Scale for the red channel.</param>
        /// <param name="greenScale">Scale for the green channel.</param>
        /// <param name="blueScale">Scale for the blue channel.</param>
        /// <param name="alphaScale">Scale for the alpha channel.</param>
        public void SetInputOptionsHeightEvaluation(IntPtr inputOptions, float redScale, float greenScale, float blueScale, float alphaScale)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsHeightEvaluation func = GetFunction<Functions.nvttSetInputOptionsHeightEvaluation>(FunctionNames.nvttSetInputOptionsHeightEvaluation);

            func(inputOptions, redScale, greenScale, blueScale, alphaScale);
        }

        /// <summary>
        /// Sets the filter parameters used during normal map generation.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="small">Small parameter.</param>
        /// <param name="medium">Medium parameter.</param>
        /// <param name="big">Big parameter.</param>
        /// <param name="large">Large parameter.</param>
        public void SetInputOptionsNormalFilter(IntPtr inputOptions, float small, float medium, float big, float large)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsNormalFilter func = GetFunction<Functions.nvttSetInputOptionsNormalFilter>(FunctionNames.nvttSetInputOptionsNormalFilter);

            func(inputOptions, small, medium, big, large);
        }

        /// <summary>
        /// Sets if normal maps that are generated by the compressor should be normalized.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="normalize">True if normal maps should be normalized, false if otherwise.</param>
        public void SetInputOptionsNormalizeMipmaps(IntPtr inputOptions, bool normalize)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsNormalizeMipmaps func = GetFunction<Functions.nvttSetInputOptionsNormalizeMipmaps>(FunctionNames.nvttSetInputOptionsNormalizeMipmaps);

            func(inputOptions, (normalize) ? NvttBool.True : NvttBool.False);
        }

        /// <summary>
        /// Sets the maximum texture dimensions, used during rounding.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="dimensions">Texture dimensions.</param>
        public void SetInputOptionsMaxExtents(IntPtr inputOptions, int dimensions)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsMaxExtents func = GetFunction<Functions.nvttSetInputOptionsMaxExtents>(FunctionNames.nvttSetInputOptionsMaxExtents);

            func(inputOptions, dimensions);
        }

        /// <summary>
        /// Sets how image dimensions should be rounded to be power of two.
        /// </summary>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="roundMode">Round mode enumeration.</param>
        public void SetInputOptionsRoundMode(IntPtr inputOptions, RoundMode roundMode)
        {
            if (inputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetInputOptionsRoundMode func = GetFunction<Functions.nvttSetInputOptionsRoundMode>(FunctionNames.nvttSetInputOptionsRoundMode);

            func(inputOptions, roundMode);
        }

        #endregion

        #region Compression options

        /// <summary>
        /// Creates a compression options object. This manages how input images are processed, such as the quality of compression or the pixel format (uncompressed or compressed).
        /// </summary>
        /// <returns>Pointer to compression options object.</returns>
        public IntPtr CreateCompressionOptions()
        {
            LoadIfNotLoaded();

            Functions.nvttCreateCompressionOptions func = GetFunction<Functions.nvttCreateCompressionOptions>(FunctionNames.nvttCreateCompressionOptions);

            return func();
        }

        /// <summary>
        /// Destroys a compression options object.
        /// </summary>
        /// <param name="compressOptions">Pointer to compression options object.</param>
        public void DestroyCompressionOptions(IntPtr compressOptions)
        {
            if (compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttDestroyCompressionOptions func = GetFunction<Functions.nvttDestroyCompressionOptions>(FunctionNames.nvttDestroyCompressionOptions);

            func(compressOptions);
        }

        /// <summary>
        /// Sets the format the images will be processed into. All save one are block compression formats, by uncompressed RGBA (up to 32-bits) can be outputted.
        /// </summary>
        /// <param name="compressOptions">Pointer to compression options object.</param>
        /// <param name="format">Compression format enumeration.</param>
        public void SetCompressionOptionsFormat(IntPtr compressOptions, CompressionFormat format)
        {
            if (compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsFormat func = GetFunction<Functions.nvttSetCompressionOptionsFormat>(FunctionNames.nvttSetCompressionOptionsFormat);

            func(compressOptions, format);
        }

        /// <summary>
        /// Sets the compression quality.
        /// </summary>
        /// <param name="compressOptions">Pointer to compression options object.</param>
        /// <param name="quality">Quality of the compression, higher quality tends to take longer.</param>
        public void SetCompressionOptionsQuality(IntPtr compressOptions, CompressionQuality quality)
        {
            if (compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsQuality func = GetFunction<Functions.nvttSetCompressionOptionsQuality>(FunctionNames.nvttSetCompressionOptionsQuality);

            func(compressOptions, quality);
        }

        /// <summary>
        /// Sets color weighting during compression. By default the compression error is measured for each channel
        /// uniformly, but for some images it may make more sense to measure the error in a perceptual color space.
        /// </summary>
        /// <param name="compressOptions">Pointer to compression options object.</param>
        /// <param name="red">Weight for the red channel.</param>
        /// <param name="green">Weight for the green channel.</param>
        /// <param name="blue">Weight for the blue channel.</param>
        /// <param name="alpha">Weight for the alpha channel.</param>
        public void SetCompressionOptionsColorWeights(IntPtr compressOptions, float red, float green, float blue, float alpha)
        {
            if (compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsColorWeights func = GetFunction<Functions.nvttSetCompressionOptionsColorWeights>(FunctionNames.nvttSetCompressionOptionsColorWeights);

            func(compressOptions, red, green, blue, alpha);
        }

        /// <summary>
        /// Sets the color output format if no block compression is set (up to 32 bit RGBA). For example, to convert to RGB 5:6:5 format,
        /// <code>SetPixelFormat(16, 0x001F, 0x07E0, 0xF800, 0)</code>.
        /// </summary>
        /// <param name="compressOptions">Pointer to compression options object.</param>
        /// <param name="bitsPerPixel">Bits per pixel of the color format.</param>
        /// <param name="red_mask">Mask for the bits that correspond to the red channel.</param>
        /// <param name="green_mask">Mask for the bits that correspond to the green channel.</param>
        /// <param name="blue_mask">Mask for the bits that correspond to the blue channel.</param>
        /// <param name="alpha_mask">Mask for the bits that correspond to the alpha channel.</param>
        public void SetCompressionOptionsPixelFormat(IntPtr compressOptions, uint bitsPerPixel, uint red_mask, uint green_mask, uint blue_mask, uint alpha_mask)
        {
            if (compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsPixelFormat func = GetFunction<Functions.nvttSetCompressionOptionsPixelFormat>(FunctionNames.nvttSetCompressionOptionsPixelFormat);

            func(compressOptions, bitsPerPixel, red_mask, green_mask, blue_mask, alpha_mask);
        }

        /// <summary>
        /// Sets whether the compressor should do dithering before compression
        /// or during quantiziation. When using block compression this does not generally improve
        /// the quality of the output image, but in some cases it can produce smoother results. It is
        /// generally a good idea to enable dithering when the output format is RGBA color.
        /// </summary>
        /// <param name="compressOptions">Pointer to compression options object.</param>
        /// <param name="colorDithering">True to enable color dithering, false otherwise.</param>
        /// <param name="alphaDithering">True to enable alpha dithering false otherwise.</param>
        /// <param name="binaryAlpha">True to use binary alpha, false otherwise.</param>
        /// <param name="alphaThreshold">Alpha threshold.</param>
        public void SetCompressionOptionsQuantization(IntPtr compressOptions, bool colorDithering, bool alphaDithering, bool binaryAlpha, int alphaThreshold)
        {
            if (compressOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetCompressionOptionsQuantization func = GetFunction<Functions.nvttSetCompressionOptionsQuantization>(FunctionNames.nvttSetCompressionOptionsQuantization);

            func(compressOptions, (colorDithering) ? NvttBool.True : NvttBool.False, (alphaDithering) ? NvttBool.True : NvttBool.False, (binaryAlpha) ? NvttBool.True : NvttBool.False, alphaThreshold);
        }

        #endregion

        #region Output options

        /// <summary>
        /// Create an output options object. This manages how processed images from the compressor are outputted, either to a file or to a stream.
        /// </summary>
        /// <returns>Pointer to output options object.</returns>
        public IntPtr CreateOutputOptions()
        {
            LoadIfNotLoaded();

            Functions.nvttCreateOutputOptions func = GetFunction<Functions.nvttCreateOutputOptions>(FunctionNames.nvttCreateOutputOptions);

            return func();
        }

        /// <summary>
        /// Destroys an output options object.
        /// </summary>
        /// <param name="outputOptions">Pointer to output options object.</param>
        public void DestroyOutputOptions(IntPtr outputOptions)
        {
            if (outputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttDestroyOutputOptions func = GetFunction<Functions.nvttDestroyOutputOptions>(FunctionNames.nvttDestroyOutputOptions);

            func(outputOptions);
        }

        /// <summary>
        /// Sets a file path to create a DDS file containing processed images.
        /// </summary>
        /// <param name="outputOptions">Pointer to output options object.</param>
        /// <param name="filename">DDS image containing the processed images.</param>
        public void SetOutputOptionsFileName(IntPtr outputOptions, String filename)
        {
            if (outputOptions == IntPtr.Zero || String.IsNullOrEmpty(filename))
                return;

            LoadIfNotLoaded();

            Functions.nvttSetOutputOptionsFileName func = GetFunction<Functions.nvttSetOutputOptionsFileName>(FunctionNames.nvttSetOutputOptionsFileName);

            func(outputOptions, filename);
        }

        /// <summary>
        /// If writing to a stream or file, specify if the DDS header should be written as well.
        /// </summary>
        /// <param name="outputOptions">Pointer to output options object.</param>
        /// <param name="value">True to write out the DDS header, false if otherwise.</param>
        public void SetOutputOptionsOutputHeader(IntPtr outputOptions, bool value)
        {
            if (outputOptions == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetOutputOptionsOutputHeader func = GetFunction<Functions.nvttSetOutputOptionsOutputHeader>(FunctionNames.nvttSetOutputOptionsOutputHeader);

            func(outputOptions, (value) ? NvttBool.True : NvttBool.False);
        }

        /// <summary>
        /// Specifies IO handlers when writing to a stream.
        /// </summary>
        /// <param name="outputOptions">Pointer to output options object.</param>
        /// <param name="beginImageHandlerCallback">Callback when a new image is about to begin, specifying image details.</param>
        /// <param name="outputHandlerCallback">Called when data needs to be written.</param>
        /// <param name="endImageHandlerCallback">Called when an image has completed.</param>
        public void SetOutputOptionsOutputHandler(IntPtr outputOptions, IntPtr beginImageHandlerCallback, IntPtr outputHandlerCallback, IntPtr endImageHandlerCallback)
        {
            if (outputOptions == IntPtr.Zero || beginImageHandlerCallback == IntPtr.Zero || outputHandlerCallback == IntPtr.Zero || endImageHandlerCallback == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttSetOutputOptionsOutputHandler func = GetFunction<Functions.nvttSetOutputOptionsOutputHandler>(FunctionNames.nvttSetOutputOptionsOutputHandler);

            func(outputOptions, beginImageHandlerCallback, outputHandlerCallback, endImageHandlerCallback);
        }

        #endregion

        #region Compressor

        /// <summary>
        /// Create a compressor object. The compressor ochestrates processing for generating mipmaps, compressing image data, and creating normal maps, then writing out
        /// the data to a file or to user data structures.
        /// </summary>
        /// <returns>Pointer to compressor object.</returns>
        public IntPtr CreateCompressor()
        {
            LoadIfNotLoaded();

            Functions.nvttCreateCompressor func = GetFunction<Functions.nvttCreateCompressor>(FunctionNames.nvttCreateCompressor);

            return func();
        }

        /// <summary>
        /// Destroys a compressor object.
        /// </summary>
        /// <param name="compressor">Pointer to compressor object.</param>
        public void DestroyCompressor(IntPtr compressor)
        {
            if (compressor == IntPtr.Zero)
                return;

            LoadIfNotLoaded();

            Functions.nvttDestroyCompressor func = GetFunction<Functions.nvttDestroyCompressor>(FunctionNames.nvttDestroyCompressor);

            func(compressor);
        }

        /// <summary>
        /// Executes processing of image data, based on input/compression/output options.
        /// </summary>
        /// <param name="compressor">Pointer to compressor object.</param>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="compressionOptions">Pointer to compression options object.</param>
        /// <param name="outputOptions">Pointer to output options object.</param>
        /// <returns>True if processing completed successfully, false if otherwise.</returns>
        public bool Process(IntPtr compressor, IntPtr inputOptions, IntPtr compressionOptions, IntPtr outputOptions)
        {
            if (compressor == IntPtr.Zero || inputOptions == IntPtr.Zero || compressionOptions == IntPtr.Zero || outputOptions == IntPtr.Zero)
                return false;

            LoadIfNotLoaded();

            Functions.nvttCompress func = GetFunction<Functions.nvttCompress>(FunctionNames.nvttCompress);

            return TranslateBool(func(compressor, inputOptions, compressionOptions, outputOptions));
        }

        /// <summary>
        /// Estimate the total number of bytes of the output, based on the specified options.
        /// </summary>
        /// <param name="compressor">Pointer to compressor object.</param>
        /// <param name="inputOptions">Pointer to input options object.</param>
        /// <param name="compressionOptions">Pointer to compression options object.</param>
        /// <returns>Total number of bytes that will contain all images, faces and mipmaps.</returns>
        public int EstimateSize(IntPtr compressor, IntPtr inputOptions, IntPtr compressionOptions)
        {
            if (compressor == IntPtr.Zero || inputOptions == IntPtr.Zero || compressionOptions == IntPtr.Zero)
                return 0;

            LoadIfNotLoaded();

            Functions.nvttEstimateSize func = GetFunction<Functions.nvttEstimateSize>(FunctionNames.nvttEstimateSize);

            return func(compressor, inputOptions, compressionOptions);
        }

        #endregion

        #region Global functions

        /// <summary>
        /// Gets the NVTT unmanaged library version.
        /// </summary>
        /// <returns></returns>
        public uint GetVersion()
        {
            LoadIfNotLoaded();

            Functions.nvttVersion func = GetFunction<Functions.nvttVersion>(FunctionNames.nvttVersion);

            return func();
        }

        private static bool TranslateBool(NvttBool value)
        {
            return (value == NvttBool.False) ? false : true;
        }

        #endregion

        #region Function names

        internal static class FunctionNames
        {
            #region Input options

            public const String nvttCreateInputOptions = "nvttCreateInputOptions";
            public const String nvttDestroyInputOptions = "nvttDestroyInputOptions";
            public const String nvttSetInputOptionsTextureLayout = "nvttSetInputOptionsTextureLayout";
            public const String nvttResetInputOptionsTextureLayout = "nvttResetInputOptionsTextureLayout";
            public const String nvttSetInputOptionsMipmapData = "nvttSetInputOptionsMipmapData";
            public const String nvttSetInputOptionsFormat = "nvttSetInputOptionsFormat";
            public const String nvttSetInputOptionsAlphaMode = "nvttSetInputOptionsAlphaMode";
            public const String nvttSetInputOptionsGamma = "nvttSetInputOptionsGamma";
            public const String nvttSetInputOptionsWrapMode = "nvttSetInputOptionsWrapMode";
            public const String nvttSetInputOptionsMipmapFilter = "nvttSetInputOptionsMipmapFilter";
            public const String nvttSetInputOptionsMipmapGeneration = "nvttSetInputOptionsMipmapGeneration";
            public const String nvttSetInputOptionsKaiserParameters = "nvttSetInputOptionsKaiserParameters";
            public const String nvttSetInputOptionsNormalMap = "nvttSetInputOptionsNormalMap";
            public const String nvttSetInputOptionsConvertToNormalMap = "nvttSetInputOptionsConvertToNormalMap";
            public const String nvttSetInputOptionsHeightEvaluation = "nvttSetInputOptionsHeightEvaluation";
            public const String nvttSetInputOptionsNormalFilter = "nvttSetInputOptionsNormalFilter";
            public const String nvttSetInputOptionsNormalizeMipmaps = "nvttSetInputOptionsNormalizeMipmaps";
            public const String nvttSetInputOptionsMaxExtents = "nvttSetInputOptionsMaxExtents";
            public const String nvttSetInputOptionsRoundMode = "nvttSetInputOptionsRoundMode";

            #endregion

            #region Compression options

            public const String nvttCreateCompressionOptions = "nvttCreateCompressionOptions";
            public const String nvttDestroyCompressionOptions = "nvttDestroyCompressionOptions";
            public const String nvttSetCompressionOptionsFormat = "nvttSetCompressionOptionsFormat";
            public const String nvttSetCompressionOptionsQuality = "nvttSetCompressionOptionsQuality";
            public const String nvttSetCompressionOptionsColorWeights = "nvttSetCompressionOptionsColorWeights";
            public const String nvttSetCompressionOptionsPixelFormat = "nvttSetCompressionOptionsPixelFormat";
            public const String nvttSetCompressionOptionsQuantization = "nvttSetCompressionOptionsQuantization";

            #endregion

            #region Output options

            public const String nvttCreateOutputOptions = "nvttCreateOutputOptions";
            public const String nvttDestroyOutputOptions = "nvttDestroyOutputOptions";
            public const String nvttSetOutputOptionsFileName = "nvttSetOutputOptionsFileName";
            public const String nvttSetOutputOptionsOutputHeader = "nvttSetOutputOptionsOutputHeader";
            public const String nvttSetOutputOptionsOutputHandler = "nvttSetOutputOptionsOutputHandler";

            #endregion

            #region Compressor

            public const String nvttCreateCompressor = "nvttCreateCompressor";
            public const String nvttDestroyCompressor = "nvttDestroyCompressor";
            public const String nvttCompress = "nvttCompress";
            public const String nvttEstimateSize = "nvttEstimateSize";

            #endregion

            #region Global functions

            public const String nvttVersion = "nvttVersion";
            public const String nvttErrorString = "nvttErrorString";

            #endregion
        }

        #endregion

        #region Enums

        //Just for easier interop
        internal enum NvttBool
        {
            False = 0,
            True = 1
        }

        //Only format support apparently, kind of no point but keep it around...
        internal enum NvttInputFormat
        {
            BGRA_8UB = 0
        }

        //C-API has the error handler commented out...
        internal enum NvttError
        {
            InvalidInput = 0,
            UserInterruption = 1,
            UnsupportedFeature = 2,
            CudaError = 3,
            Unknown = 4,
            FileOpen = 5,
            FileWrite = 6,
            UnsupportedOutputFormat = 7
        }

        #endregion

        #region Function delegates

        internal static class Functions
        {
            #region Input options

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCreateInputOptions)]
            public delegate IntPtr nvttCreateInputOptions();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttDestroyInputOptions)]
            public delegate void nvttDestroyInputOptions(IntPtr inputOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsTextureLayout)]
            public delegate void nvttSetInputOptionsTextureLayout(IntPtr inputOptions, TextureType type, int width, int height, int depth);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttResetInputOptionsTextureLayout)]
            public delegate void nvttResetInputOptionsTextureLayout(IntPtr inputOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsMipmapData)]
            public delegate NvttBool nvttSetInputOptionsMipmapData(IntPtr inputOptions, IntPtr data, int width, int height, int depth, int face, int mipmap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsFormat)]
            public delegate void nvttSetInputOptionsFormat(IntPtr inputOptions, NvttInputFormat format);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsAlphaMode)]
            public delegate void nvttSetInputOptionsAlphaMode(IntPtr inputOptions, AlphaMode alphaMode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsGamma)]
            public delegate void nvttSetInputOptionsGamma(IntPtr inputOptions, float inputGamma, float outputGamma);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsWrapMode)]
            public delegate void nvttSetInputOptionsWrapMode(IntPtr inputOptions, WrapMode wrapMode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsMipmapFilter)]
            public delegate void nvttSetInputOptionsMipmapFilter(IntPtr inputOptions, MipmapFilter filter);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsMipmapGeneration)]
            public delegate void nvttSetInputOptionsMipmapGeneration(IntPtr inputOptions, NvttBool isEnabled, int maxLevel);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsKaiserParameters)]
            public delegate void nvttSetInputOptionsKaiserParameters(IntPtr inputOptions, float width, float alpha, float stretch);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsNormalMap)]
            public delegate void nvttSetInputOptionsNormalMap(IntPtr inputOptions, NvttBool isNormalMap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsConvertToNormalMap)]
            public delegate void nvttSetInputOptionsConvertToNormalMap(IntPtr inputOptions, NvttBool convertToNormalMap);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsHeightEvaluation)]
            public delegate void nvttSetInputOptionsHeightEvaluation(IntPtr inputOptions, float redScale, float greenScale, float blueScale, float alphaScale);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsNormalFilter)]
            public delegate void nvttSetInputOptionsNormalFilter(IntPtr inputOptions, float small, float medium, float big, float large);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsNormalizeMipmaps)]
            public delegate void nvttSetInputOptionsNormalizeMipmaps(IntPtr inputOptions, NvttBool normalize);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsMaxExtents)]
            public delegate void nvttSetInputOptionsMaxExtents(IntPtr inputOptions, int dimensions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetInputOptionsRoundMode)]
            public delegate void nvttSetInputOptionsRoundMode(IntPtr inputOptions, RoundMode roundMode);

            #endregion

            #region Compression options

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCreateCompressionOptions)]
            public delegate IntPtr nvttCreateCompressionOptions();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttDestroyCompressionOptions)]
            public delegate void nvttDestroyCompressionOptions(IntPtr compressOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsFormat)]
            public delegate void nvttSetCompressionOptionsFormat(IntPtr compressOptions, CompressionFormat format);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsQuality)]
            public delegate void nvttSetCompressionOptionsQuality(IntPtr compressOptions, CompressionQuality quality);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsColorWeights)]
            public delegate void nvttSetCompressionOptionsColorWeights(IntPtr compressOptions, float red, float green, float blue, float alpha);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsPixelFormat)]
            public delegate void nvttSetCompressionOptionsPixelFormat(IntPtr compressOptions, uint bitsPerPixel, uint red_mask, uint green_mask, uint blue_mask, uint alpha_mask);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetCompressionOptionsQuantization)]
            public delegate void nvttSetCompressionOptionsQuantization(IntPtr compressOptions, NvttBool colorDithering, NvttBool alphaDithering, NvttBool binaryAlpha, int alphaThreshold);

            #endregion

            #region Output options

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCreateOutputOptions)]
            public delegate IntPtr nvttCreateOutputOptions();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttDestroyOutputOptions)]
            public delegate void nvttDestroyOutputOptions(IntPtr outputOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetOutputOptionsFileName)]
            public delegate void nvttSetOutputOptionsFileName(IntPtr outputOptions, [In, MarshalAs(UnmanagedType.LPStr)] String filename);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetOutputOptionsOutputHeader)]
            public delegate void nvttSetOutputOptionsOutputHeader(IntPtr outputOptions, NvttBool value);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttSetOutputOptionsOutputHandler)]
            public delegate void nvttSetOutputOptionsOutputHandler(IntPtr outputOptions, IntPtr beginImageHandlerCallback, IntPtr outputHandlerCallback, IntPtr endImageHandlerCallback);

            #endregion

            #region Compressor

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCreateCompressor)]
            public delegate IntPtr nvttCreateCompressor();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttDestroyCompressor)]
            public delegate void nvttDestroyCompressor(IntPtr compressor);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttCompress)]
            public delegate NvttBool nvttCompress(IntPtr compressor, IntPtr inputOptions, IntPtr compressionOptions, IntPtr outputOptions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttEstimateSize)]
            public delegate int nvttEstimateSize(IntPtr compressor, IntPtr inputOptions, IntPtr compressionOptions);

            #endregion

            #region Global functions

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttVersion)]
            public delegate uint nvttVersion();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl), UnmanagedFunctionName(FunctionNames.nvttErrorString)]
            [return: MarshalAs(UnmanagedType.LPStr)]
            public delegate String nvttErrorString(NvttError err);

            #endregion
        }

        #endregion
    }
}
