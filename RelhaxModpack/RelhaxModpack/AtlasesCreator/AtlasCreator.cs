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
    public enum FailCode
    {
        None = 0,
        FailedParsingArguments,
        ImageExporter,
        MapExporter,
        NoImages,
        ImageNameCollision,

        FailedToLoadImage,
        FailedToPackImage,
        FailedToCreateImage,
        FailedToSaveImage,
        FailedToSaveMap,
        FailedToParseSubtextures
    }

    public class AtlasCreator : IDisposable
    {
        public Atlas atlas;
        public object debugLockObject;
        public CancellationToken token;

        private ImagePacker imagePacker = new ImagePacker();
        private Stopwatch stopwatch = new Stopwatch();
        private Bitmap atlasImage;
        private Bitmap outputAtlasImage;

        private string tempAtlasImageFile;
        private string tempAtlasMapFile;

        private static Task parseModTexturesTask;
        private static List<Texture> realModTextures;
        private static Stopwatch modParseStopwatch = new Stopwatch();

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
                            name = filename,
                            height = newImage.Height,
                            width = newImage.Width,
                            x = 0,
                            y = 0,
                            AtlasImage = newImage
                        });
                    }
                }
                Logging.Info("static: mod images parsing completed in {0} msec", modParseStopwatch.ElapsedMilliseconds);
                modParseStopwatch.Stop();
            });
            return parseModTexturesTask;
        }

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

        public FailCode CreateAtlas()
        {
            //input checks
            if (atlas == null)
                throw new BadMemeException("you forgot to set the atlas object. nice.");
            if (debugLockObject == null)
                throw new BadMemeException("you forgot to set the debug lock object. very nice.");

            //create the save directory if it does not already exist
            if (!Directory.Exists(atlas.AtlasSaveDirectory))
                Directory.CreateDirectory(atlas.AtlasSaveDirectory);

            //set the mapfile name
            atlas.MapFile = string.Format("{0}.xml", Path.GetFileNameWithoutExtension(atlas.AtlasFile));
            Logging.Debug("atlas file {0}: set map name as {1}", Path.GetFileName(atlas.AtlasFile), Path.GetFileName(atlas.MapFile));

            //set location to extract original WG atlas files
            tempAtlasImageFile = Path.Combine(atlas.TempAltasPresentDirectory, atlas.AtlasFile);
            tempAtlasMapFile = Path.Combine(atlas.TempAltasPresentDirectory, atlas.MapFile);

            //delete the temp files if they exist
            if (File.Exists(tempAtlasImageFile))
                File.Delete(tempAtlasImageFile);
            if (File.Exists(tempAtlasMapFile))
                File.Delete(tempAtlasMapFile);

            //extract the map and atlas files
            Logging.Info("atlas file {0}: unpack of atlas and map starting", Path.GetFileName(atlas.AtlasFile));
            stopwatch.Restart();
            Logging.Debug("atlas file {0}: atlas file unpack", Path.GetFileName(atlas.AtlasFile));
            lock(debugLockObject)
            {
                XMLUtils.Unpack(atlas.Pkg, Path.Combine(atlas.DirectoryInArchive, atlas.AtlasFile), tempAtlasImageFile);
            }
            token.ThrowIfCancellationRequested();

            Logging.Debug("atlas file {0}: map file unpack", Path.GetFileName(atlas.AtlasFile));
            lock (debugLockObject)
            {
                XMLUtils.Unpack(atlas.Pkg, Path.Combine(atlas.DirectoryInArchive, atlas.MapFile), tempAtlasMapFile);
            }
            token.ThrowIfCancellationRequested();

            stopwatch.Stop();
            Logging.Info("atlas file {0}: unpack completed in {1} msec", Path.GetFileName(atlas.AtlasFile), stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            //parse the xml map file into the list of subtextures
            Logging.Info("atlas file {0}: parsing map file", Path.GetFileName(atlas.AtlasFile));
            Logging.Debug("atlas file {0}: using map file {1}", Path.GetFileName(atlas.AtlasFile), tempAtlasMapFile);
            atlas.TextureList = LoadMapFile(tempAtlasMapFile);

            //using the parsed size and location definitions from above, copy each individual subtexture to the texture list
            Size originalAtlasSize = new Size();
            Logging.Info("atlas file {0}: loading atlas to bitmap data", Path.GetFileName(atlas.AtlasFile));
            Logging.Debug("atlas file {0}: using atlas file {1}", Path.GetFileName(atlas.AtlasFile), tempAtlasImageFile);
            stopwatch.Restart();
            lock (debugLockObject)
            {
                //the native library can only be used once at a time
                atlasImage = LoadDDS(tempAtlasImageFile);
            }
            token.ThrowIfCancellationRequested();

            //get the size from grumpel code
            originalAtlasSize = atlasImage.Size;

            //copy the subtexture bitmap data to each texture bitmap data
            Logging.Info("atlas file {0}: parsing bitmap data", Path.GetFileName(atlas.AtlasFile));
            foreach (Texture texture in atlas.TextureList)
            {
                token.ThrowIfCancellationRequested();
                //copy the texture bitmap data into the texture bitmap object
                //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.clone?redirectedfrom=MSDN&view=netframework-4.8#System_Drawing_Bitmap_Clone_System_Drawing_Rectangle_System_Drawing_Imaging_PixelFormat_
                //rectangle of desired area to clone
                Rectangle textureRect = new Rectangle(texture.x, texture.y, texture.width, texture.height);
                //copy the bitmap
                texture.AtlasImage = atlasImage.Clone(textureRect, atlasImage.PixelFormat);
            }
            
            stopwatch.Stop();
            Logging.Info("atlas file {0}: parsing bitmap data completed in {1} msec", Path.GetFileName(atlas.AtlasFile), stopwatch.ElapsedMilliseconds);

            //prepare atlas objects for processing
            atlas.AtlasFile = Path.Combine(atlas.AtlasSaveDirectory, atlas.AtlasFile);
            atlas.MapFile = Path.Combine(atlas.AtlasSaveDirectory, atlas.MapFile);

            // if the arguments in width and/or height of the atlases-creator-config-xml-file are 0 (or below) or not given, work with the original file dimensions to get working width and height
            if ((atlas.AtlasHeight < 1) | (atlas.AtlasWidth < 1))
            {
                //fix atlas width and size parameters if they are wrong
                if (atlas.AtlasWidth < 1)
                    throw new BadMemeException("grumpel...");

                if (atlas.AtlasHeight < 1)
                    throw new BadMemeException("grumpel...");

                //
                if ((originalAtlasSize.Height * originalAtlasSize.Width) == (atlas.AtlasWidth * atlas.AtlasHeight))
                {
                    atlas.AtlasHeight = (int)(atlas.AtlasHeight * 1.5);
                }
                else
                {
                    // this is to be sure that the image size that will be created, is at least the original size
                    while ((originalAtlasSize.Height * originalAtlasSize.Width) > (atlas.AtlasWidth * atlas.AtlasHeight))
                        atlas.AtlasHeight = (int)(atlas.AtlasHeight * 1.2);
                }
            }

            //
            if ((originalAtlasSize.Height * originalAtlasSize.Width) > (atlas.AtlasWidth * atlas.AtlasHeight))
            {
                Logging.Warning("atlas file {0}: max possible size is smaller then original size", Path.GetFileName(atlas.AtlasFile));
                Logging.Warning("original h x w:     {1} x {2}", originalAtlasSize.Height, originalAtlasSize.Width);
                Logging.Warning("max possible h x w: {3} x {4}", atlas.AtlasHeight, atlas.AtlasWidth);
            }
            else
            {
                Logging.Debug("atlas file {0}: max possible size of new atlas file-> {1} (h) x {2} (w)", Path.GetFileName(atlas.AtlasFile), atlas.AtlasHeight, atlas.AtlasWidth);
            }

            //wait for task here
            Logging.Info("atlas file {0}: waiting for mod texture parse task", Path.GetFileName(atlas.AtlasFile));
            parseModTexturesTask.Wait();
            Logging.Info("atlas file {0}: mod texture parse task complete, continue execution", Path.GetFileName(atlas.AtlasFile));

            //check if any custom mod contour icons were parsed
            if (realModTextures.Count > 0)
            {
                Logging.Info("atlas file {0}: {1} custom icons parsed", Path.GetFileName(atlas.AtlasFile), realModTextures.Count);
            }
            else
            {
                Logging.Info("atlas file {0}: 0 custom icons parsed for atlas file {1}, no need to make a custom atlas!", realModTextures.Count, Path.GetFileName(atlas.AtlasFile));
                return FailCode.None;
            }
            
            //replace the original atlas textures with the mod ones
            Logging.Info("atlas file {0}: mod images replacing starting", Path.GetFileName(atlas.AtlasFile));
            stopwatch.Restart();
            for (int i = 0; i < atlas.TextureList.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                //get the matching texture, if it exists
                Texture[] originalResults = realModTextures.Where(texturee => texturee.name.Equals(atlas.TextureList[i].name)).ToArray();
                if (originalResults.Count() == 0)
                    continue;
                Texture textureResult = originalResults[originalResults.Count() - 1];
                //here means the count is one, replace the WG original subtexture with the mod one
                atlas.TextureList[i].AtlasImage.Dispose();
                atlas.TextureList[i].AtlasImage = null;
                atlas.TextureList[i].AtlasImage = textureResult.AtlasImage;
                atlas.TextureList[i].x = 0;
                atlas.TextureList[i].y = 0;
                atlas.TextureList[i].height = textureResult.AtlasImage.Height;
                atlas.TextureList[i].width = textureResult.AtlasImage.Width;
            }
            stopwatch.Stop();
            Logging.Info("atlas file {0}: mod images replacing completed in {1} msec", Path.GetFileName(atlas.AtlasFile), stopwatch.ElapsedMilliseconds);
            
            //(finally) run the atlas creator program
            Logging.Info("atlas file {0}: building starting", Path.GetFileName(atlas.AtlasFile));
            stopwatch.Restart();

            // pack the image, generating a map only if desired
            FailCode result = imagePacker.PackImage(atlas.TextureList, atlas.PowOf2, atlas.Square, atlas.FastImagePacker, atlas.AtlasWidth, atlas.AtlasHeight,
                atlas.Padding, out Bitmap outputImage, out Dictionary<string, Rectangle> outputMap);
            if (result != 0)
            {
                Logging.Error("atlas file {0}: There was an error making the image sheet", Path.GetFileName(atlas.AtlasFile));
                //error result 7 = "failed to pack image" most likely it won't fit
                return result;
            }
            else
            {
                Logging.Info("atlas file {0}: Success Packing into {1} x {2} pixel", Path.GetFileName(atlas.AtlasFile), outputImage.Height, outputImage.Width);
            }

            //save it to the class for disposal
            outputAtlasImage = outputImage;

            //export the atlas file
            //delete one if it exists
            if (File.Exists(atlas.AtlasFile))
                File.Delete(atlas.AtlasFile);
            //then save
            SaveDDS(atlas.AtlasFile, outputImage);
            Logging.Info("atlas file {0}: successfully created Atlas image: {1}", Path.GetFileName(atlas.AtlasFile), atlas.AtlasFile);

            //export the mapfile
            //delete one if it exists
            if (File.Exists(atlas.MapFile))
                File.Delete(atlas.MapFile);

            //then save
            SaveMapfile(atlas.MapFile, outputMap);
            Logging.Info("atlas file {0}: successfully created map: {1}", Path.GetFileName(atlas.AtlasFile), atlas.MapFile);

            stopwatch.Stop();
            Logging.Info("atlas file {0}: building completed in {1} msec", Path.GetFileName(atlas.AtlasFile), stopwatch.ElapsedMilliseconds);
            
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

            //check to make sure file is a dds file
            if (!TeximpNet.DDS.DDSFile.IsDDSFile(filename))
            {
                Logging.Error("image is not a dds file: {0}", filename);
            }

            //load the image into freeImage format
            Surface surface = Surface.LoadFromFile(filename, ImageLoadFlags.Default);

            //flip it because it's upside down because reasons.
            surface.FlipVertically();

            //stride is rowpitch

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
                            texture.name = item.Value.ToString().Trim();
                            break;
                        case "x":
                            texture.x = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "y":
                            texture.y = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "width":
                            texture.width = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "height":
                            texture.height = int.Parse("0" + item.Value.ToString().Trim());
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
            Logging.Info("atlas file {0}: disposing resources from DisposeCleanup()", Path.GetFileName(atlas.AtlasFile));
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

            //dispose atlas subtexure data
            if (atlas != null)
            {
                if (atlas.TextureList != null)
                {
                    foreach (Texture tex in atlas.TextureList)
                    {
                        if (tex.AtlasImage != null)
                        {
                            tex.AtlasImage.Dispose();
                            tex.AtlasImage = null;
                        }
                    }
                    atlas.TextureList = null;
                }
            }

            //delete the temp files if they exist
            if (File.Exists(tempAtlasImageFile))
                File.Delete(tempAtlasImageFile);
            if (File.Exists(tempAtlasMapFile))
                File.Delete(tempAtlasMapFile);
        }

        private bool disposedValue = false; // To detect redundant calls

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
