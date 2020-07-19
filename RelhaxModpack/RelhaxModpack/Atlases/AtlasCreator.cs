using RelhaxModpack.Atlases.Packing;
using RelhaxModpack.Utilities;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using TeximpNet;
using TeximpNet.Compression;

namespace RelhaxModpack.Atlases
{
    /// <summary>
    /// List of possible areas in the Atlas creation process where it could fail
    /// </summary>
    public enum FailCode
    {
        /// <summary>
        /// No error occurred
        /// </summary>
        None = 0,

        /// <summary>
        /// Failed to import the DDS image file to a bitmap object
        /// </summary>
        ImageImporter,

        /// <summary>
        /// Failed to export the bitmap object to a DDS image file
        /// </summary>
        ImageExporter,

        /// <summary>
        /// Failed to load and parse the WG xml atlas map
        /// </summary>
        MapImporter,

        /// <summary>
        /// Failed to parse and save the WG xml atlas map
        /// </summary>
        MapExporter,

        /// <summary>
        /// No images to build for the atlas
        /// </summary>
        NoImages,

        /// <summary>
        /// Duplicate image names in list of images to pack
        /// </summary>
        ImageNameCollision,

        /// <summary>
        /// Failed to pack the images into one large image (most likely they don't fit into the provided dimensions)
        /// </summary>
        FailedToPackImage,

        /// <summary>
        /// Failed to compress an atlas that requires over the 2GB process limit on 32bit systems
        /// </summary>
        OutOfMemory32bit,

        /// <summary>
        /// Failed to create the atlas bitmap object
        /// </summary>
        FailedToCreateBitmapAtlas
    }

    /// <summary>
    /// The delegate to invoke when calling back to the sender for the AtlasProgres event
    /// </summary>
    /// <param name="sender">The sending Atlas Creator</param>
    /// <param name="e">Event arguments</param>
    public delegate void AtlasProgressDelegate(object sender, EventArgs e);

    /// <summary>
    /// Represents the entire process of building an atlas image
    /// </summary>
    public class AtlasCreator : IDisposable
    {
        /// <summary>
        /// The (arbitrary, guessing) limit of atlas size that the DDS compressor can process without exceeding 2GB
        /// </summary>
        /// <remarks>In 32bit windows OSs, the maximum amount of memory that a process can have is 2GB.
        /// Exceeding that in the compressor will trigger a AccessViolationException</remarks>
        public static int MAX_ATLAS_SIZE_32BIT = 8000 * 8000;

        /// <summary>
        /// The object of atlas arguments for building the image
        /// </summary>
        public Atlas Atlas = null;

        /// <summary>
        /// The token for handling a cancel call from the user
        /// </summary>
        public CancellationToken Token;

        /// <summary>
        /// The event when atlas child progress occurs
        /// </summary>
        public event AtlasProgressDelegate OnAtlasProgres;

        private ImagePacker imagePacker = new ImagePacker();
        private ImageHandler imageHandler = new ImageHandler();
        private MapHandler mapHandler = new MapHandler();
        private Stopwatch stopwatch = new Stopwatch();
        private Bitmap atlasImage = null;
        private Bitmap outputAtlasImage = null;
        private long totalMillisecondsToCreateImage = 0;

        /// <summary>
        /// Create the atlas image and map xml
        /// </summary>
        /// <param name="atlas">The atlas arguments object</param>
        /// <returns>Success code if complete, any other FailCode otherwise</returns>
        public FailCode CreateAtlas(Atlas atlas)
        {
            this.Atlas = atlas;
            return CreateAtlas();
        }

        /// <summary>
        /// Create the atlas image and map xml
        /// </summary>
        /// <returns>Success code if complete, any other FailCode otherwise</returns>
        public FailCode CreateAtlas()
        {
            //input checks
            if (Atlas == null)
                throw new BadMemeException("you forgot to set the atlas object. nice.");
            totalMillisecondsToCreateImage = 0;
            stopwatch.Restart();

            //configure names and paths
            //set the name of the mapfile based on the filename of the atlas image, if not set from xml load
            Logging.Info("[atlas file {0}]: Preparing to create atlas", Atlas.AtlasFile);
            if (string.IsNullOrEmpty(Atlas.MapFile))
                Atlas.MapFile = string.Format("{0}.xml", Path.GetFileNameWithoutExtension(Atlas.AtlasFile));
            //set the paths of the created image and map file
            Atlas.AtlasImageFilePath = Path.Combine(Atlas.AtlasSaveDirectory, Atlas.AtlasFile);
            Atlas.AtlasMapFilePath = Path.Combine(Atlas.AtlasSaveDirectory, Atlas.MapFile);
            //set location to extract original WG atlas files. If not custom set, then set them to the RelhaxTempfolder location
            if (string.IsNullOrEmpty(Atlas.TempAtlasImageFilePath))
                Atlas.TempAtlasImageFilePath = Path.Combine(Settings.RelhaxTempFolderPath, Atlas.AtlasFile);
            if(string.IsNullOrEmpty(Atlas.TempAtlasMapFilePath))
                Atlas.TempAtlasMapFilePath = Path.Combine(Settings.RelhaxTempFolderPath, Atlas.MapFile);

            //prepare the filesystem
            lock (AtlasUtils.AtlasLoaderLockObject)
            {
                //create temp directory if it does not already exist
                if (!Directory.Exists(Path.GetDirectoryName(Atlas.TempAtlasImageFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(Atlas.TempAtlasImageFilePath));

                //create the save directory if it does not already exist
                if (!Directory.Exists(Atlas.AtlasSaveDirectory))
                    Directory.CreateDirectory(Atlas.AtlasSaveDirectory);
            }
            //delete the temp files if they exist
            if (File.Exists(Atlas.TempAtlasImageFilePath))
                File.Delete(Atlas.TempAtlasImageFilePath);
            if (File.Exists(Atlas.TempAtlasMapFilePath))
                File.Delete(Atlas.TempAtlasMapFilePath);
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Preparing to create atlas completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //extract the map and atlas files
            //because of the potential to use the same package for multiple threads, it's safer to do one at a time
            //but it's fine cause these are quick so no big deal
            Logging.Info("[atlas file {0}]: Unpack of atlas and map starting", Atlas.AtlasFile);
            Logging.Debug("[atlas file {0}]: Atlas file unpack: pkg={1}, sourcePath={2}, dest={3}",
                Path.GetFileName(Atlas.AtlasFile), Atlas.Pkg, Path.Combine(Atlas.DirectoryInArchive, Atlas.AtlasFile), Atlas.TempAtlasImageFilePath);
            lock(AtlasUtils.AtlasLoaderLockObject)
            {
                FileUtils.Unpack(Atlas.Pkg, Path.Combine(Atlas.DirectoryInArchive, Atlas.AtlasFile), Atlas.TempAtlasImageFilePath);
            }
            OnAtlasProgres?.Invoke(this,null);
            Token.ThrowIfCancellationRequested();

            Logging.Debug("[atlas file {0}]: Map file unpack: pkg={1}, sourcePath={2}, dest={3}",
                Path.GetFileName(Atlas.AtlasFile), Atlas.Pkg, Path.Combine(Atlas.DirectoryInArchive, Atlas.MapFile), Atlas.TempAtlasMapFilePath);
            lock (AtlasUtils.AtlasLoaderLockObject)
            {
                FileUtils.Unpack(Atlas.Pkg, Path.Combine(Atlas.DirectoryInArchive, Atlas.MapFile), Atlas.TempAtlasMapFilePath);
            }
            OnAtlasProgres?.Invoke(this, null);
            Token.ThrowIfCancellationRequested();
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Unpack completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //parse the xml map file into the list of sub-textures
            Logging.Info("[atlas file {0}]: Parsing map file", Atlas.AtlasFile);
            Logging.Debug("[atlas file {0}]: Using map file path: {1}", Atlas.AtlasFile, Atlas.TempAtlasMapFilePath);
            Atlas.TextureList = mapHandler.LoadMapFile(Atlas.TempAtlasMapFilePath);
            OnAtlasProgres?.Invoke(this, null);
            Token.ThrowIfCancellationRequested();
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Parsing map completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //using the parsed size and location definitions from above, copy each individual sub-texture to the texture list
            Logging.Info("[atlas file {0}]: Parsing atlas to bitmap data", Atlas.AtlasFile);
            Logging.Debug("[atlas file {0}]: Using atlas file {1}", Atlas.AtlasFile, Atlas.TempAtlasImageFilePath);
            lock (AtlasUtils.AtlasLoaderLockObject)
            {
                //the native library can only be used once at a time
                atlasImage = imageHandler.LoadDDS(Atlas.TempAtlasImageFilePath);
            }
            OnAtlasProgres?.Invoke(this, null);
            Token.ThrowIfCancellationRequested();
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Parsing atlas completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Stop();

            //if the max width and height weren't given, then use 1.2x width and height of the original
            Size originalAtlasSize = new Size();
            originalAtlasSize = atlasImage.Size;
            if ((Atlas.AtlasHeight < 1) || (Atlas.AtlasWidth < 1))
            {
                Logging.Debug("Atlas width and/or height were not provided, using a 1.2x multiplier instead");
                Atlas.AtlasHeight = (int)(originalAtlasSize.Height * 1.2);
                Atlas.AtlasWidth = (int)(originalAtlasSize.Width * 1.2);
            }
            else if ((originalAtlasSize.Height * originalAtlasSize.Width) > (Atlas.AtlasWidth * Atlas.AtlasHeight))
            {
                Logging.Warning("[atlas file {0}]: Max possible size is smaller then original size", Atlas.AtlasFile);
                Logging.Warning("Original h x w:     {1} x {2}", originalAtlasSize.Height, originalAtlasSize.Width);
                Logging.Warning("Max possible h x w: {3} x {4}", Atlas.AtlasHeight, Atlas.AtlasWidth);
                Logging.Warning("Using a 1.2x multiplier instead");
                Atlas.AtlasHeight = (int)(originalAtlasSize.Height * 1.2);
                Atlas.AtlasWidth = (int)(originalAtlasSize.Width * 1.2);
            }
            else
            {
                Logging.Debug("[atlas file {0}]: Max possible size of new atlas file-> {1} (h) x {2} (w)", Atlas.AtlasFile, Atlas.AtlasHeight, Atlas.AtlasWidth);
            }

            //copy the sub-texture bitmap data to each texture bitmap data
            stopwatch.Start();
            Logging.Info("[atlas file {0}]: Parsing bitmap data", Atlas.AtlasFile);
            lock (AtlasUtils.AtlasLoaderLockObject)
            {
                //lock the atlas image into memory
                Rectangle rect = new Rectangle(0, 0, atlasImage.Width, atlasImage.Height);
                BitmapData atlasLock = atlasImage.LockBits(rect, ImageLockMode.ReadOnly, atlasImage.PixelFormat);
                foreach (Texture texture in Atlas.TextureList)
                {
                    Token.ThrowIfCancellationRequested();
                    //copy the texture bitmap data into the texture bitmap object
                    //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.clone?redirectedfrom=MSDN&view=netframework-4.8#System_Drawing_Bitmap_Clone_System_Drawing_Rectangle_System_Drawing_Imaging_PixelFormat_
                    //rectangle of desired area to clone
                    Rectangle textureRect = new Rectangle(texture.X, texture.Y, texture.Width, texture.Height);
                    //copy the bitmap
                    try
                    {
                        texture.AtlasImage = atlasImage.Clone(textureRect, atlasImage.PixelFormat);
                        //do a quick lock on the bits to ensure that the image data is deep copied
                        //https://stackoverflow.com/a/13935966/3128017
                        BitmapData data = texture.AtlasImage.LockBits(new Rectangle(0, 0, texture.AtlasImage.Width, texture.AtlasImage.Height), ImageLockMode.ReadOnly, texture.AtlasImage.PixelFormat);
                        texture.AtlasImage.UnlockBits(data);
                    }
                    catch (Exception ex)
                    {
                        Logging.Exception("Failed to clone atlas image data");
                        Logging.Exception(ex.ToString());
                        try
                        {
                            atlasImage.UnlockBits(atlasLock);
                            atlasImage.Dispose();
                        }
                        catch
                        { }
                        return FailCode.ImageImporter;
                    }
                }
                atlasImage.UnlockBits(atlasLock);
                atlasImage.Dispose();
            }
            OnAtlasProgres?.Invoke(this, null);
            Token.ThrowIfCancellationRequested();
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Parsing bitmap data completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //wait for parsing of mod/custom images task here
            Logging.Info("[atlas file {0}]: Waiting for mod texture parse task", Atlas.AtlasFile);
            AtlasUtils.ParseCustomTexturesTask.Wait();
            Logging.Info("[atlas file {0}]: Mod texture parse task complete, continue execution", Atlas.AtlasFile);
            OnAtlasProgres?.Invoke(this, null);
            Token.ThrowIfCancellationRequested();

            //check if any custom mod contour icons were parsed. if not, then there's no need to make a new one
            if (AtlasUtils.CustomContourIconImages.Count > 0)
            {
                Logging.Info("[atlas file {0}]: {1} custom icons parsed", Atlas.AtlasFile, AtlasUtils.CustomContourIconImages.Count);
            }
            else
            {
                Logging.Warning("[atlas file {0}]: 0 custom icons parsed for atlas file {1}, no need to make a custom atlas (is this the intent?)", AtlasUtils.CustomContourIconImages.Count, Atlas.AtlasFile);
                return FailCode.None;
            }
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //replace the original atlas textures with the mod ones
            Logging.Info("[atlas file {0}]: Replacing stock WG images with custom/mod images", Atlas.AtlasFile);
            for (int i = 0; i < Atlas.TextureList.Count; i++)
            {
                Token.ThrowIfCancellationRequested();
                Texture tex = Atlas.TextureList[i];

                //get the matching texture, if it exists
                Texture[] originalResults = AtlasUtils.CustomContourIconImages.Where(texturee => texturee.Name.Equals(Atlas.TextureList[i].Name)).ToArray();
                if (originalResults.Count() == 0)
                    continue;

                Texture textureResult = originalResults[originalResults.Count() - 1];
                //here means the count is one, replace the WG original subtexture with the mod one
                tex.AtlasImage.Dispose();
                tex.AtlasImage = null;
                tex.AtlasImage = textureResult.AtlasImage;
                tex.X = 0;
                tex.Y = 0;
                tex.Height = textureResult.AtlasImage.Height;
                tex.Width = textureResult.AtlasImage.Width;
            }
            OnAtlasProgres?.Invoke(this, null);
            Logging.Info("[atlas file {0}]: Replacing stock WG images completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //run the atlas creator program
            Logging.Info("[atlas file {0}]: Atlas image packing starting", Atlas.AtlasFile);
            FailCode result = imagePacker.PackImage(Atlas.TextureList, Atlas.PowOf2, Atlas.Square, Atlas.FastImagePacker, Atlas.AtlasWidth, Atlas.AtlasHeight,
#pragma warning disable IDE0068 // Use recommended dispose pattern
                Atlas.Padding, out Bitmap outputImage, out Dictionary<string, Rectangle> outputMap);
#pragma warning restore IDE0068 // Use recommended dispose pattern
            OnAtlasProgres?.Invoke(this, null);
            if (result != 0)
            {
                Logging.Error("[atlas file {0}]: There was an error making the image sheet", Atlas.AtlasFile);
                return result;
            }
            else
            {
                Logging.Info("[atlas file {0}]: Success packing into {1} x {2} pixel", Atlas.AtlasFile, outputImage.Height, outputImage.Width);
            }
            OnAtlasProgres?.Invoke(this, null);
            Token.ThrowIfCancellationRequested();
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Atlas image packing completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //save it to the class for disposal
            outputAtlasImage = outputImage;

            //check if we're on a 32bit process. if we are and the atlas size is above the 2GB (estimated) limit, then return an error code.
            //honestly why are you on a 32bit system to begin with. it's 2020. like come on.
            if(!Environment.Is64BitProcess)
            {
                Logging.Warning("This is a 32bit process, need to check if the atlas file is too large to process");
                int outputImageArea = outputImage.Width * outputImage.Height;
                if(outputImageArea > MAX_ATLAS_SIZE_32BIT)
                {
                    Logging.Error("The output image is dimensions: W={0}, H={1}, Area={2}. Maximum area for processing on a 32bit system is {3} (W={4}, H={5}).",
                        outputImage.Width, outputImage.Height, outputImageArea, MAX_ATLAS_SIZE_32BIT, 8000, 8000);
                    return FailCode.OutOfMemory32bit;
                }
            }
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //export the atlas image file
            Logging.Info("[atlas file {0}]: Atlas image creation starting", Atlas.AtlasFile);
            if (File.Exists(Atlas.AtlasImageFilePath))
            {
                Logging.Info("[atlas file {0}]: File already exists before write, deleting", Atlas.AtlasFile);
                File.Delete(Atlas.AtlasImageFilePath);
            }
            if(!imageHandler.SaveDDS(Atlas.AtlasImageFilePath, outputImage))
            {
                Logging.Error("[atlas file {0}]: Failed to create atlas image: {1}", Atlas.AtlasFile, Atlas.AtlasFile);
                return FailCode.ImageExporter;
            }
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Atlas image creation completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //export the atlas map file
            Logging.Info("[atlas file {0}]: Atlas map creation starting", Atlas.AtlasFile);
            if (File.Exists(Atlas.AtlasMapFilePath))
                File.Delete(Atlas.AtlasMapFilePath);
            mapHandler.SaveMapfile(Atlas.AtlasMapFilePath, outputMap);
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Atlas map creation completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Stop();

            //done
            Logging.Info("[atlas file {0}]: Creating atlas process completed in {1} msec", Atlas.AtlasFile, totalMillisecondsToCreateImage);
            
            return FailCode.None;
        }

        #region IDisposable Support
        private void Cleanup()
        {
            Logging.Debug(LogOptions.ClassName, "Disposing resources");
            //dispose main atlas image
            if (atlasImage != null)
            {
                atlasImage.Dispose();
                atlasImage = null;
            }

            //dispose of created atlas image
            if (outputAtlasImage != null)
            {
                outputAtlasImage.Dispose();
                outputAtlasImage = null;
            }

            //dispose atlas subtexture data
            if (Atlas != null)
            {
                if (Atlas.TextureList != null)
                {
                    foreach (Texture tex in Atlas.TextureList)
                    {
                        if (tex.AtlasImage != null)
                        {
                            tex.AtlasImage.Dispose();
                            tex.AtlasImage = null;
                        }
                    }
                    Atlas.TextureList = null;
                }
            }

            //delete the temp files if they exist
            if (Atlas != null)
            {
                if (File.Exists(Atlas.TempAtlasImageFilePath))
                    File.Delete(Atlas.TempAtlasImageFilePath);
                if (File.Exists(Atlas.TempAtlasMapFilePath))
                    File.Delete(Atlas.TempAtlasMapFilePath);
            }
        }

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose of the Atlas Creator (mostly disposing image data)
        /// </summary>
        /// <param name="disposing">Set to true to dispose managed objects as well as unmanaged</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                Cleanup();

                disposedValue = true;
            }
        }

        ~AtlasCreator()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Dispose of the Atlas Creator (mostly disposing image data)
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
