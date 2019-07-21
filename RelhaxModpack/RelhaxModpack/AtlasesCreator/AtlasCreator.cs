using RelhaxModpack.AtlasesCreator.Packing;
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

namespace RelhaxModpack.AtlasesCreator
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
        /// Failed to create the atlas bitmap object
        /// </summary>
        FailedToCreateBitmapAtlas
    }

    /// <summary>
    /// Represents the entire process of building an atlas image
    /// </summary>
    public class AtlasCreator : IDisposable
    {
        /// <summary>
        /// The object of atlas arguments for building the image
        /// </summary>
        public Atlas Atlas;

        /// <summary>
        /// Object that exists in all Atlas creation threads. Used for sensitive areas of code that can only be executed by one thread at a time
        /// </summary>
        public object DebugLockObject;

        /// <summary>
        /// The token for handling a cancel call from the user
        /// </summary>
        public CancellationToken Token;

        private ImagePacker imagePacker = new ImagePacker();
        private Stopwatch stopwatch = new Stopwatch();
        private Bitmap atlasImage;
        private Bitmap outputAtlasImage;

        private string tempAtlasImageFile;
        private string tempAtlasMapFile;

        private static Task parseModTexturesTask;
        private static List<Texture> realModTextures;
        private static Stopwatch modParseStopwatch = new Stopwatch();

        /// <summary>
        /// Loads all mod textures from disk into texture objects. This is done on a separate thread so it is not done redundantly multiple times on each atlas thread
        /// </summary>
        /// <param name="allModFolderPaths">The list of absolute paths containing images to be loaded</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public static Task ParseModTexturesAsync(List<string> allModFolderPaths, CancellationToken token)
        {
            parseModTexturesTask = Task.Run(() =>
            {
                //override bitmaps with what exists in the mods folders
                Logging.Info("static: mod images parsing starting");
                modParseStopwatch.Restart();
                realModTextures = new List<Texture>();
                foreach (string folder in allModFolderPaths)
                {
                    string realFolder = Utils.MacroReplace(folder, ReplacementTypes.FilePath);
                    Logging.Info("static: checking for images in directory {0}", realFolder);
                    token.ThrowIfCancellationRequested();
                    if (!Directory.Exists(realFolder))
                    {
                        Logging.Warning("static: directory {0} does not exist, skipping", realFolder);
                        continue;
                    }

                    //get every image file in the folder
                    File.SetAttributes(realFolder, FileAttributes.Normal);
                    FileInfo[] customContourIcons = new string[] { "*.jpg", "*.png", "*.bmp" }
                                .SelectMany(i => new DirectoryInfo(realFolder).GetFiles(i, SearchOption.TopDirectoryOnly))
                                .ToArray();

                    if (customContourIcons.Count() == 0)
                    {
                        Logging.Warning("static: directory {0} does not contain image files, skipping", realFolder);
                        continue;
                    }

                    foreach (FileInfo customContourIcon in customContourIcons)
                    {
                        token.ThrowIfCancellationRequested();
                        //load the custom image into the texture list
                        string filename = Path.GetFileNameWithoutExtension(customContourIcon.Name);
                        //load the bitmap as well
                        Bitmap newImage = Image.FromFile(customContourIcon.FullName) as Bitmap;
                        //don't care about the x an y for the mod textures
                        realModTextures.Add(new Texture()
                        {
                            Name = filename,
                            Height = newImage.Height,
                            Width = newImage.Width,
                            X = 0,
                            Y = 0,
                            AtlasImage = newImage
                        });
                    }
                }
                Logging.Info("static: mod images parsing completed in {0} msec", modParseStopwatch.ElapsedMilliseconds);
                modParseStopwatch.Stop();
            });
            return parseModTexturesTask;
        }

        /// <summary>
        /// Dispose of all textures in the shared mod texture list
        /// </summary>
        public static void DisposeparseModTextures()
        {
            if (realModTextures != null)
            {
                foreach (Texture tex in realModTextures)
                {
                    if (tex.AtlasImage != null)
                    {
                        tex.AtlasImage.Dispose();
                        tex.AtlasImage = null;
                    }
                }
                realModTextures = null;
            }
        }

        /// <summary>
        /// Create the atlas image
        /// </summary>
        /// <returns>Success code if complete, any other FailCode otherwise</returns>
        public FailCode CreateAtlas()
        {
            //input checks
            if (Atlas == null)
                throw new BadMemeException("you forgot to set the atlas object. nice.");
            if (DebugLockObject == null)
                throw new BadMemeException("you forgot to set the debug lock object. very nice.");

            //create the save directory if it does not already exist
            if (!Directory.Exists(Atlas.AtlasSaveDirectory))
                Directory.CreateDirectory(Atlas.AtlasSaveDirectory);

            //set the mapfile name
            Atlas.MapFile = string.Format("{0}.xml", Path.GetFileNameWithoutExtension(Atlas.AtlasFile));
            Logging.Debug("atlas file {0}: set map name as {1}", Path.GetFileName(Atlas.AtlasFile), Path.GetFileName(Atlas.MapFile));

            //set location to extract original WG atlas files
            tempAtlasImageFile = Path.Combine(Settings.RelhaxTempFolder, Atlas.AtlasFile);
            tempAtlasMapFile = Path.Combine(Settings.RelhaxTempFolder, Atlas.MapFile);

            //delete the temp files if they exist
            if (File.Exists(tempAtlasImageFile))
                File.Delete(tempAtlasImageFile);
            if (File.Exists(tempAtlasMapFile))
                File.Delete(tempAtlasMapFile);

            //extract the map and atlas files
            Logging.Info("atlas file {0}: unpack of atlas and map starting", Path.GetFileName(Atlas.AtlasFile));
            stopwatch.Restart();
            Logging.Debug("atlas file {0}: atlas file unpack", Path.GetFileName(Atlas.AtlasFile));
            lock(DebugLockObject)
            {
                XMLUtils.Unpack(Atlas.Pkg, Path.Combine(Atlas.DirectoryInArchive, Atlas.AtlasFile), tempAtlasImageFile);
            }
            Token.ThrowIfCancellationRequested();

            Logging.Debug("atlas file {0}: map file unpack", Path.GetFileName(Atlas.AtlasFile));
            lock (DebugLockObject)
            {
                XMLUtils.Unpack(Atlas.Pkg, Path.Combine(Atlas.DirectoryInArchive, Atlas.MapFile), tempAtlasMapFile);
            }
            Token.ThrowIfCancellationRequested();

            stopwatch.Stop();
            Logging.Info("atlas file {0}: unpack completed in {1} msec", Path.GetFileName(Atlas.AtlasFile), stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            //parse the xml map file into the list of subtextures
            Logging.Info("atlas file {0}: parsing map file", Path.GetFileName(Atlas.AtlasFile));
            Logging.Debug("atlas file {0}: using map file {1}", Path.GetFileName(Atlas.AtlasFile), tempAtlasMapFile);
            Atlas.TextureList = LoadMapFile(tempAtlasMapFile);

            //using the parsed size and location definitions from above, copy each individual subtexture to the texture list
            Size originalAtlasSize = new Size();
            Logging.Info("atlas file {0}: loading atlas to bitmap data", Path.GetFileName(Atlas.AtlasFile));
            Logging.Debug("atlas file {0}: using atlas file {1}", Path.GetFileName(Atlas.AtlasFile), tempAtlasImageFile);
            stopwatch.Restart();
            lock (DebugLockObject)
            {
                //the native library can only be used once at a time
                atlasImage = LoadDDS(tempAtlasImageFile);
            }
            Token.ThrowIfCancellationRequested();

            //get the size from grumpel code
            originalAtlasSize = atlasImage.Size;

            //copy the subtexture bitmap data to each texture bitmap data
            Logging.Info("atlas file {0}: parsing bitmap data", Path.GetFileName(Atlas.AtlasFile));
            foreach (Texture texture in Atlas.TextureList)
            {
                Token.ThrowIfCancellationRequested();
                //copy the texture bitmap data into the texture bitmap object
                //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.clone?redirectedfrom=MSDN&view=netframework-4.8#System_Drawing_Bitmap_Clone_System_Drawing_Rectangle_System_Drawing_Imaging_PixelFormat_
                //rectangle of desired area to clone
                Rectangle textureRect = new Rectangle(texture.X, texture.Y, texture.Width, texture.Height);
                //copy the bitmap
                texture.AtlasImage = atlasImage.Clone(textureRect, atlasImage.PixelFormat);
            }
            
            stopwatch.Stop();
            Logging.Info("atlas file {0}: parsing bitmap data completed in {1} msec", Path.GetFileName(Atlas.AtlasFile), stopwatch.ElapsedMilliseconds);

            //prepare atlas objects for processing
            Atlas.AtlasFile = Path.Combine(Atlas.AtlasSaveDirectory, Atlas.AtlasFile);
            Atlas.MapFile = Path.Combine(Atlas.AtlasSaveDirectory, Atlas.MapFile);

            // if the arguments in width and/or height of the atlases-creator-config-xml-file are 0 (or below) or not given, work with the original file dimensions to get working width and height
            if ((Atlas.AtlasHeight < 1) | (Atlas.AtlasWidth < 1))
            {
                //fix atlas width and size parameters if they are wrong
                if (Atlas.AtlasWidth < 1)
                    throw new BadMemeException("grumpel...");

                if (Atlas.AtlasHeight < 1)
                    throw new BadMemeException("grumpel...");

                //
                if ((originalAtlasSize.Height * originalAtlasSize.Width) == (Atlas.AtlasWidth * Atlas.AtlasHeight))
                {
                    Atlas.AtlasHeight = (int)(Atlas.AtlasHeight * 1.5);
                }
                else
                {
                    // this is to be sure that the image size that will be created, is at least the original size
                    while ((originalAtlasSize.Height * originalAtlasSize.Width) > (Atlas.AtlasWidth * Atlas.AtlasHeight))
                        Atlas.AtlasHeight = (int)(Atlas.AtlasHeight * 1.2);
                }
            }

            //
            if ((originalAtlasSize.Height * originalAtlasSize.Width) > (Atlas.AtlasWidth * Atlas.AtlasHeight))
            {
                Logging.Warning("atlas file {0}: max possible size is smaller then original size", Path.GetFileName(Atlas.AtlasFile));
                Logging.Warning("original h x w:     {1} x {2}", originalAtlasSize.Height, originalAtlasSize.Width);
                Logging.Warning("max possible h x w: {3} x {4}", Atlas.AtlasHeight, Atlas.AtlasWidth);
            }
            else
            {
                Logging.Debug("atlas file {0}: max possible size of new atlas file-> {1} (h) x {2} (w)", Path.GetFileName(Atlas.AtlasFile), Atlas.AtlasHeight, Atlas.AtlasWidth);
            }

            //wait for task here
            Logging.Info("atlas file {0}: waiting for mod texture parse task", Path.GetFileName(Atlas.AtlasFile));
            parseModTexturesTask.Wait();
            Logging.Info("atlas file {0}: mod texture parse task complete, continue execution", Path.GetFileName(Atlas.AtlasFile));

            //check if any custom mod contour icons were parsed
            if (realModTextures.Count > 0)
            {
                Logging.Info("atlas file {0}: {1} custom icons parsed", Path.GetFileName(Atlas.AtlasFile), realModTextures.Count);
            }
            else
            {
                Logging.Info("atlas file {0}: 0 custom icons parsed for atlas file {1}, no need to make a custom atlas!", realModTextures.Count, Path.GetFileName(Atlas.AtlasFile));
                return FailCode.None;
            }
            
            //replace the original atlas textures with the mod ones
            Logging.Info("atlas file {0}: mod images replacing starting", Path.GetFileName(Atlas.AtlasFile));
            stopwatch.Restart();
            for (int i = 0; i < Atlas.TextureList.Count; i++)
            {
                Token.ThrowIfCancellationRequested();

                //get the matching texture, if it exists
                Texture[] originalResults = realModTextures.Where(texturee => texturee.Name.Equals(Atlas.TextureList[i].Name)).ToArray();
                if (originalResults.Count() == 0)
                    continue;
                Texture textureResult = originalResults[originalResults.Count() - 1];
                //here means the count is one, replace the WG original subtexture with the mod one
                Atlas.TextureList[i].AtlasImage.Dispose();
                Atlas.TextureList[i].AtlasImage = null;
                Atlas.TextureList[i].AtlasImage = textureResult.AtlasImage;
                Atlas.TextureList[i].X = 0;
                Atlas.TextureList[i].Y = 0;
                Atlas.TextureList[i].Height = textureResult.AtlasImage.Height;
                Atlas.TextureList[i].Width = textureResult.AtlasImage.Width;
            }
            stopwatch.Stop();
            Logging.Info("atlas file {0}: mod images replacing completed in {1} msec", Path.GetFileName(Atlas.AtlasFile), stopwatch.ElapsedMilliseconds);
            
            //(finally) run the atlas creator program
            Logging.Info("atlas file {0}: building starting", Path.GetFileName(Atlas.AtlasFile));
            stopwatch.Restart();

            // pack the image, generating a map only if desired
            FailCode result = imagePacker.PackImage(Atlas.TextureList, Atlas.PowOf2, Atlas.Square, Atlas.FastImagePacker, Atlas.AtlasWidth, Atlas.AtlasHeight,
                Atlas.Padding, out Bitmap outputImage, out Dictionary<string, Rectangle> outputMap);
            if (result != 0)
            {
                Logging.Error("atlas file {0}: There was an error making the image sheet", Path.GetFileName(Atlas.AtlasFile));
                //error result 7 = "failed to pack image" most likely it won't fit
                return result;
            }
            else
            {
                Logging.Info("atlas file {0}: Success Packing into {1} x {2} pixel", Path.GetFileName(Atlas.AtlasFile), outputImage.Height, outputImage.Width);
            }

            //save it to the class for disposal
            outputAtlasImage = outputImage;

            //export the atlas file
            //delete one if it exists
            if (File.Exists(Atlas.AtlasFile))
                File.Delete(Atlas.AtlasFile);
            //then save
            SaveDDS(Atlas.AtlasFile, outputImage);
            Logging.Info("atlas file {0}: successfully created Atlas image: {1}", Path.GetFileName(Atlas.AtlasFile), Atlas.AtlasFile);

            //export the mapfile
            //delete one if it exists
            if (File.Exists(Atlas.MapFile))
                File.Delete(Atlas.MapFile);

            //then save
            SaveMapfile(Atlas.MapFile, outputMap);
            Logging.Info("atlas file {0}: successfully created map: {1}", Path.GetFileName(Atlas.AtlasFile), Atlas.MapFile);

            stopwatch.Stop();
            Logging.Info("atlas file {0}: building completed in {1} msec", Path.GetFileName(Atlas.AtlasFile), stopwatch.ElapsedMilliseconds);
            
            return FailCode.None;
        }

        #region Atlas image handling
        private Bitmap LoadDDS(string filename)
        {
            //check to make sure file exists
            if (!File.Exists(filename))
            {
                Logging.Error("image file does not exist at path {0}", filename);
                return null;
            }

            //check to make sure file is a DDS file
            if (!TeximpNet.DDS.DDSFile.IsDDSFile(filename))
            {
                Logging.Error("image is not a DDS file: {0}", filename);
            }

            //load the image into freeImage format
            Surface surface = Surface.LoadFromFile(filename, ImageLoadFlags.Default);

            //flip it because it's upside down because reasons.
            surface.FlipVertically();

            //stride is row pitch

            return new Bitmap(surface.Width, surface.Height, surface.Pitch, System.Drawing.Imaging.PixelFormat.Format32bppArgb, surface.DataPtr);
        }

        private void SaveDDS(string filename, Bitmap image)
        {
            // Lock the bitmap's bits. 
            //https://stackoverflow.com/questions/28655133/difference-between-bitmap-and-bitmapdata
            //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata.scan0?view=netframework-4.8#System_Drawing_Imaging_BitmapData_Scan0
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);

            //create surface object for processing
            //and compress to DDS
            using (Surface surfaceFromRawData = Surface.LoadFromRawData(bmpData.Scan0, image.Width, image.Height, bmpData.Stride, true))
            using (Compressor compressor = new Compressor())
            {
                compressor.Compression.Format = CompressionFormat.DXT5;
                compressor.Input.AlphaMode = AlphaMode.None;
                compressor.Input.GenerateMipmaps = false;
                compressor.Input.ConvertToNormalMap = false;
                compressor.Input.SetData(surfaceFromRawData);
                compressor.Process(filename);
            }
            image.UnlockBits(bmpData);
        }
        #endregion

        #region Map handling
        // parses a xmlAtlasesFile to the process queue
        private List<Texture> LoadMapFile(string mapFile)
        {
            //check to make sure file exists first
            if (!File.Exists(mapFile))
            {
                Logging.Error("Texture list does not exist in {0}", mapFile);
                return null;
            }

            //make the texture list to return later
            List<Texture> TextureList = new List<Texture>();

            //load the xml file
            XDocument doc = XMLUtils.LoadXDocument(mapFile, XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Error("Failed to load xml texture list in {0}", mapFile);
                return null;
            }

            //parse the xml values into the texture type
            foreach (XElement xmlTexture in doc.XPathSelectElements("/root/SubTexture"))
            {
                Texture texture = new Texture();
                foreach (XElement item in xmlTexture.Elements())
                {
                    switch (item.Name.ToString().ToLower())
                    {
                        case "name":
                            texture.Name = item.Value.ToString().Trim();
                            break;
                        case "x":
                            texture.X = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "y":
                            texture.Y = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "width":
                            texture.Width = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "height":
                            texture.Height = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        default:
                            Logging.Error("unexpected Item found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value);
                            break;
                    }
                }
                TextureList.Add(texture);
            }
            return TextureList;
        }

        private void SaveMapfile(string filename, Dictionary<string, Rectangle> map)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("<root createdBy=\"Relhax Modpack\" date=\"{0:yyyy-MM-dd HH:mm:ss.fff}\">", System.DateTime.Now);
                foreach (var entry in map.OrderBy(key => key.Key))
                {
                    Rectangle r = entry.Value;
                    writer.WriteLine(string.Format("\t<SubTexture>"));
                    writer.WriteLine(string.Format("\t\t<name> {0} </name>", Path.GetFileNameWithoutExtension(entry.Key)));
                    writer.WriteLine(string.Format("\t\t<x> {0} </x>", r.X));
                    writer.WriteLine(string.Format("\t\t<y> {0} </y>", r.Y));
                    writer.WriteLine(string.Format("\t\t<width> {0} </width>", r.Width));
                    writer.WriteLine(string.Format("\t\t<height> {0} </height>", r.Height));
                    writer.WriteLine(string.Format("\t</SubTexture>"));
                }
                writer.WriteLine("</root>");
            }
        }
        #endregion

        #region IDisposable Support
        private void DisposeCleanup()
        {
            Logging.Info("atlas file {0}: disposing resources from DisposeCleanup()", Path.GetFileName(Atlas.AtlasFile));
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
            if (File.Exists(tempAtlasImageFile))
                File.Delete(tempAtlasImageFile);
            if (File.Exists(tempAtlasMapFile))
                File.Delete(tempAtlasMapFile);
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
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                DisposeCleanup();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AtlasCreator() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <summary>
        /// Dispose of the Atlas Creator (mostly disposing image data)
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
