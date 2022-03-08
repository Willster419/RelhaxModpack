using RelhaxModpack.Atlases.Packing;
using RelhaxModpack.Common;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
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
    /// The delegate to invoke when calling back to the sender for the AtlasProgres event.
    /// </summary>
    /// <param name="sender">The sending Atlas Creator.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void AtlasProgressDelegate(object sender, EventArgs e);

    /// <summary>
    /// Represents the entire process of building an atlas image.
    /// </summary>
    public class AtlasCreator : IDisposable
    {
        /// <summary>
        /// The (arbitrary, estimated) limit of an atlas size that the DDS compressor can process without exceeding 2GB.
        /// </summary>
        /// <remarks>
        /// In 32bit windows OSs, the maximum amount of memory that a process can have is 2GB.
        /// Exceeding that in the compressor will trigger an AccessViolationException.
        /// </remarks>
        public static int MAX_ATLAS_SIZE_32BIT = 8000 * 8000;

        /// <summary>
        /// The Atlas instructions object for building the image and map.
        /// </summary>
        public Atlas Atlas = null;

        /// <summary>
        /// The token for handling a cancellation call from the user.
        /// </summary>
        public CancellationToken Token;

        /// <summary>
        /// The event when atlas creation progress occurs.
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
        /// Create the Atlas image and map xml.
        /// </summary>
        /// <param name="atlas">The atlas arguments object.</param>
        /// <returns>Success code if complete, any other FailCode otherwise.</returns>
        /// <seealso cref="FailCode"/>
        public FailCode CreateAtlas(Atlas atlas)
        {
            this.Atlas = atlas;
            return CreateAtlas();
        }

        /// <summary>
        /// Create the atlas image and map xml.
        /// </summary>
        /// <returns>Success code if complete, any other FailCode otherwise.</returns>
        /// <seealso cref="FailCode"/>
        public FailCode CreateAtlas()
        {
            //input checks
            if (Atlas == null)
                throw new BadMemeException("you forgot to set the atlas object. nice.");
            totalMillisecondsToCreateImage = 0;
            stopwatch.Restart();

            //configure names and paths
            //set the name of the map file based on the filename of the atlas image, if not set from xml load
            Logging.Info("[atlas file {0}]: Preparing to create atlas", Atlas.AtlasFile);
            if (string.IsNullOrEmpty(Atlas.MapFile))
                Atlas.MapFile = string.Format("{0}.xml", Path.GetFileNameWithoutExtension(Atlas.AtlasFile));
            //set the paths of the created image and map file
            Atlas.AtlasImageFilePath = Path.Combine(Atlas.AtlasSaveDirectory, Atlas.AtlasFile);
            Atlas.AtlasMapFilePath = Path.Combine(Atlas.AtlasSaveDirectory, Atlas.MapFile);
            //set location to extract original WG atlas files. If not custom set, then set them to the RelhaxTempfolder location
            if (string.IsNullOrEmpty(Atlas.TempAtlasImageFilePath))
                Atlas.TempAtlasImageFilePath = Path.Combine(ApplicationConstants.RelhaxTempFolderPath, Atlas.AtlasFile);
            if(string.IsNullOrEmpty(Atlas.TempAtlasMapFilePath))
                Atlas.TempAtlasMapFilePath = Path.Combine(ApplicationConstants.RelhaxTempFolderPath, Atlas.MapFile);

            //prepare the temp and output directories (lock to prevent multiple threads creating folders. Could get messy.
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
            //because of the potential to use the same package for multiple threads, it's safer to do one at a time
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
            //the native library can only be used once at a time
            lock (AtlasUtils.AtlasLoaderLockObject)
            {
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
            //get the overall size of the bitmap
            Rectangle rect = new Rectangle(0, 0, atlasImage.Width, atlasImage.Height);
            foreach (Texture texture in Atlas.TextureList)
            {
                Rectangle textureRect = new Rectangle(texture.X, texture.Y, texture.Width, texture.Height);
                //copy the texture bitmap data from the atlas bitmap into the texture bitmap
                //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.clone?redirectedfrom=MSDN&view=netframework-4.8#System_Drawing_Bitmap_Clone_System_Drawing_Rectangle_System_Drawing_Imaging_PixelFormat
                texture.AtlasImage = atlasImage.Clone(textureRect, atlasImage.PixelFormat);
                //do a quick lock on the bits to ensure that the image data is deep copied. Clone() seems to only shallow copy
                //https://stackoverflow.com/a/13935966/3128017
                BitmapData data = texture.AtlasImage.LockBits(new Rectangle(0, 0, texture.AtlasImage.Width, texture.AtlasImage.Height), ImageLockMode.ReadOnly, texture.AtlasImage.PixelFormat);
                texture.AtlasImage.UnlockBits(data);
            }
            //dispose of the original cause now we're done with it
            atlasImage.Dispose();
            OnAtlasProgres?.Invoke(this, null);
            Token.ThrowIfCancellationRequested();
            stopwatch.Stop();
            Logging.Info("[atlas file {0}]: Parsing bitmap data completed in {1} msec", Atlas.AtlasFile, stopwatch.ElapsedMilliseconds);
            totalMillisecondsToCreateImage += stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            //wait for parsing of mod/custom images task here
            Logging.Info("[atlas file {0}]: Waiting for mod texture parse task", Atlas.AtlasFile);
            ParseCustomTexturesTask.Wait();
            Logging.Info("[atlas file {0}]: Mod texture parse task complete, continue execution", Atlas.AtlasFile);
            OnAtlasProgres?.Invoke(this, null);
            Token.ThrowIfCancellationRequested();

            //check if any custom mod contour icons were parsed. if not, then there's no need to make a new one
            if (CustomContourIconImages.Count > 0)
            {
                Logging.Info("[atlas file {0}]: {1} custom icons parsed", Atlas.AtlasFile, CustomContourIconImages.Count);
            }
            else
            {
                Logging.Warning("[atlas file {0}]: 0 custom icons parsed for atlas file {1}, no need to make a custom atlas (is this the intent?)", CustomContourIconImages.Count, Atlas.AtlasFile);
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
                Texture[] originalResults = CustomContourIconImages.Where(texturee => texturee.Name.Equals(Atlas.TextureList[i].Name)).ToArray();
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
            FailCode result = imagePacker.PackImage(Atlas.TextureList, Atlas.PowOf2, Atlas.Square, Atlas.FastImagePacker, Atlas.AtlasWidth, Atlas.AtlasHeight, Atlas.Padding, Atlas.AtlasFile, out Bitmap outputImage, out Dictionary<string, Rectangle> outputMap);
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

        #region Atlas custom icons parsing
        /// <summary>
        /// The task object of parsing all custom images from multiple folders into a list of bitmaps.
        /// </summary>
        public static Task ParseCustomTexturesTask { get; private set; } = null;

        /// <summary>
        /// The list of parsed custom images from folders into textures.
        /// </summary>
        public static List<Texture> CustomContourIconImages { get; private set; } = null;

        private static Stopwatch ParseStopwatch = new Stopwatch();

        /// <summary>
        /// Loads all custom textures from disk into texture objects. This is done on a separate thread so it is not done redundantly multiple times on each atlas thread.
        /// </summary>
        /// <param name="CustomFolderPaths">The list of absolute paths containing custom contour icon images to be loaded.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The list of textures.</returns>
        public static Task LoadCustomContourIconsAsync(List<string> CustomFolderPaths, CancellationToken token)
        {
            ParseCustomTexturesTask = Task.Run(() =>
            {
                Logging.Info(LogOptions.MethodName, "Custom contour icon images task starting");
                ParseStopwatch.Restart();

                //parse each folder list to create a list of all custom contour icons
                Logging.Debug(LogOptions.MethodName, "Custom contour icon images folder count: {0}", CustomFolderPaths.Count);
                List<string> customContourIconFilesList = new List<string>();
                foreach (string folder in CustomFolderPaths)
                {
                    string realFolder = MacroUtils.MacroReplace(folder, ReplacementTypes.FilePath);
                    Logging.Info(LogOptions.MethodName, "Checking for custom contour icon images in directory {0}", realFolder);
                    token.ThrowIfCancellationRequested();

                    if (!Directory.Exists(realFolder))
                    {
                        Logging.Warning(LogOptions.MethodName, "Directory {0} does not exist, skipping", realFolder);
                        continue;
                    }

                    customContourIconFilesList.AddRange(FileUtils.FileSearch(realFolder, SearchOption.TopDirectoryOnly, false, false, "*", 5, 3, false));
                }

                //filter the list to just image files
                //{ "*.jpg", "*.png", "*.bmp" }
                Logging.Debug(LogOptions.MethodName, "List created, filtering for only png,jpg,bmp image files", CustomFolderPaths.Count);
                customContourIconFilesList = customContourIconFilesList.Where(filepath =>
                {
                    if (Path.GetExtension(filepath).ToLower().Contains("png"))
                        return true;
                    else if (Path.GetExtension(filepath).ToLower().Contains("jpg"))
                        return true;
                    else if (Path.GetExtension(filepath).ToLower().Contains("bmp"))
                        return true;
                    else
                        return false;

                }).ToList();
                customContourIconFilesList = customContourIconFilesList.Distinct().ToList();
                if (customContourIconFilesList.Count == 0)
                {
                    Logging.Warning(LogOptions.MethodName, "Total of 0 custom contour icons found to parse (Is this the intent?)");
                    return;
                }

                //just in case, dispose of the old one
                DisposeParsedCustomTextures();
                CustomContourIconImages = new List<Texture>();
                Logging.Debug(LogOptions.MethodName, "Loading custom images into data lists for atlas creator", CustomFolderPaths.Count);
                foreach (string customContourIconFilePath in customContourIconFilesList)
                {
                    token.ThrowIfCancellationRequested();

                    //load the bitmap as well
                    Bitmap customContourIconImage = new Bitmap(customContourIconFilePath);

                    //don't care about the x an y for the custom textures
                    CustomContourIconImages.Add(new Texture()
                    {
                        Name = Path.GetFileNameWithoutExtension(customContourIconFilePath),
                        Height = customContourIconImage.Height,
                        Width = customContourIconImage.Width,
                        X = 0,
                        Y = 0,
                        AtlasImage = customContourIconImage
                    });
                    customContourIconImage = null;
                }
                Logging.Info(LogOptions.MethodName, "Custom images parsing task completed in {0} msec", ParseStopwatch.ElapsedMilliseconds);
                ParseStopwatch.Stop();
            });
            return ParseCustomTexturesTask;
        }

        /// <summary>
        /// Dispose of all textures in the shared custom texture list.
        /// </summary>
        public static void DisposeParsedCustomTextures()
        {
            if (CustomContourIconImages != null)
            {
                foreach (Texture tex in CustomContourIconImages)
                {
                    if (tex != null)
                    {
                        if (tex.AtlasImage != null)
                        {
                            tex.AtlasImage.Dispose();
                            tex.AtlasImage = null;
                        }
                    }
                }
                CustomContourIconImages = null;
            }
        }
        #endregion

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
        /// Dispose of the Atlas Creator (mostly disposing image data).
        /// </summary>
        /// <param name="disposing">Set to true to dispose managed objects as well as unmanaged.</param>
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

        /// <summary>
        /// Destruct the instance of the AtlasCreator by the garbage collector.
        /// </summary>
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
