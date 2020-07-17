using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Atlases
{
    /// <summary>
    /// A utility class for Atlas file processing
    /// </summary>
    public static class AtlasUtils
    {
        /// <summary>
        /// The link to the Microsoft Visual C++ dll package required by the atlas processing libraries
        /// </summary>
        public const string MSVCPLink = "https://www.microsoft.com/en-us/download/details.aspx?id=40784";

        /// <summary>
        /// The manager instance of the FreeImage Library
        /// </summary>
        public static RelhaxFreeImageLibrary FreeImageLibrary = new RelhaxFreeImageLibrary();

        /// <summary>
        /// The manager instance of the Nvidia Texture Tools Library
        /// </summary>
        public static RelhaxNvTexLibrary NvTexLibrary = new RelhaxNvTexLibrary();

        /// <summary>
        /// Test the ability to load an unmanaged library
        /// </summary>
        /// <returns>True if library loaded, false otherwise</returns>
        public static bool TestLibrary(IRelhaxUnmanagedLibrary library, string name, bool unload)
        {
            Logging.Info("Testing {0} library", name);
            bool libraryLoaded;
            if (!library.IsLoaded)
            {
                if (library.Load())
                {
                    Logging.Info("Library loaded successfully");
                    libraryLoaded = true;
                }
                else
                {
                    Logging.Error("Library failed to load");
                    libraryLoaded = false;
                }
            }
            else
            {
                Logging.Info("Library already loaded");
                libraryLoaded = true;
            }

            if (unload && library.IsLoaded)
            {
                Logging.Info("Unload requested and library is loaded, unloading");
                if (library.Unload())
                {
                    Logging.Info("Library unloaded successfully");
                }
                else
                {
                    Logging.Error("Failed to unload library");
                    libraryLoaded = false;
                }
            }
            return libraryLoaded;
        }

        /// <summary>
        /// Test the ability to load and unload all the atlas image processing libraries
        /// </summary>
        /// <returns>True if both libraries loaded, false otherwise</returns>
        public static bool TestLoadAtlasLibraries(bool unload)
        {
            bool freeImageLoaded = TestLibrary(FreeImageLibrary, "FreeImage", true);
            bool nvttLoaded = TestLibrary(NvTexLibrary, "nvtt", true);

            if (nvttLoaded && freeImageLoaded)
            {
                Logging.Info("TestLoadAtlasLibraries(): both libraries loaded");
                return true;
            }
            else
            {
                Logging.Error("TestLoadAtlasLibraries(): failed to load one or more atlas processing libraries: freeImage={0}, nvtt={1}",
                    freeImageLoaded.ToString(), nvttLoaded.ToString());
                return false;
            }
        }

        /// <summary>
        /// The task of parsing all mod png images from multiple folders into a flat list of png bitmaps
        /// </summary>
        public static Task ParseModTexturesTask;

        /// <summary>
        /// The list of parsed mod png images into textures
        /// </summary>
        public static List<Texture> ModContourIconImages;

        /// <summary>
        /// Lock object used in AtlasCreator for critical sections that can't be done at the same time
        /// </summary>
        /// <remarks>Each atlas file is created by its own thread. However, the DDS loading and saving API used can't be used more then once at a time</remarks>
        public static object AtlasLoaderLockObject = new object();

        /// <summary>
        /// A list of Atlas creating thread engines
        /// </summary>
        public static List<AtlasCreator> AtlasBuilders = null;

        private static Stopwatch modParseStopwatch = new Stopwatch();

        /// <summary>
        /// Loads all mod textures from disk into texture objects. This is done on a separate thread so it is not done redundantly multiple times on each atlas thread
        /// </summary>
        /// <param name="allModFolderPaths">The list of absolute paths containing mod contour icon images to be loaded</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The list of textures</returns>
        public static Task LoadModContourIconsAsync(List<string> allModFolderPaths, CancellationToken token)
        {
            ParseModTexturesTask = Task.Run(() =>
            {
                Logging.Info("[ParseModTexturesTask]: mod contour icon images task starting");
                modParseStopwatch.Restart();

                //parse each folder list to create a list of all mod contour icons
                Logging.Debug("[ParseModTexturesTask]: mod contour icon images folder count: {0}", allModFolderPaths.Count);
                List<string> ModContourIconFilesList = new List<string>();
                foreach (string folder in allModFolderPaths)
                {
                    string realFolder = MacroUtils.MacroReplace(folder, ReplacementTypes.FilePath);
                    Logging.Info("[ParseModTexturesTask]: checking for mod contour icon images in directory {0}", realFolder);
                    token.ThrowIfCancellationRequested();

                    if (!Directory.Exists(realFolder))
                    {
                        Logging.Warning("[ParseModTexturesTask]: directory {0} does not exist, skipping", realFolder);
                        continue;
                    }

                    ModContourIconFilesList.AddRange(FileUtils.DirectorySearch(realFolder, SearchOption.TopDirectoryOnly, false, "*", 5, 3, false));
                }

                //filter the list to just image files
                //{ "*.jpg", "*.png", "*.bmp" }
                ModContourIconFilesList = ModContourIconFilesList.Where(filepath =>
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
                ModContourIconFilesList = ModContourIconFilesList.Distinct().ToList();
                if(ModContourIconFilesList.Count == 0)
                {
                    Logging.Warning("[ParseModTexturesTask]: 0 Mod contour icons to parse!");
                    return;
                }

                //just in case, dispose of the old one
                DisposeparseModTextures();
                ModContourIconImages = new List<Texture>();

                foreach (string modContourIconFilePath in ModContourIconFilesList)
                {
                    token.ThrowIfCancellationRequested();

                    //load the bitmap as well
                    Bitmap modContourIconImage = Image.FromFile(modContourIconFilePath) as Bitmap;

                    //don't care about the x an y for the mod textures
                    ModContourIconImages.Add(new Texture()
                    {
                        Name = Path.GetFileNameWithoutExtension(modContourIconFilePath),
                        Height = modContourIconImage.Height,
                        Width = modContourIconImage.Width,
                        X = 0,
                        Y = 0,
                        AtlasImage = modContourIconImage
                    });
                    modContourIconImage = null;
                }
                Logging.Info("[ParseModTexturesTask]: mod images parsing completed in {0} msec", modParseStopwatch.ElapsedMilliseconds);
                modParseStopwatch.Stop();
            });
            return ParseModTexturesTask;
        }

        /// <summary>
        /// Dispose of all textures in the shared mod texture list
        /// </summary>
        public static void DisposeparseModTextures()
        {
            if (ModContourIconImages != null)
            {
                foreach (Texture tex in ModContourIconImages)
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
                ModContourIconImages = null;
            }
        }

        /// <summary>
        /// Verifies the Atlas processing libraries are loaded. If not, they are loaded.
        /// </summary>
        public static void VerifyImageLibsLoaded()
        {
            if (!FreeImageLibrary.IsLoaded)
            {
                Logging.Info("Freeimage library is not loaded, loading");
                FreeImageLibrary.Load();
                Logging.Info("Freeimage library loaded");
            }
            else
                Logging.Info("Freeimage library is loaded");

            if (!NvTexLibrary.IsLoaded)
            {
                Logging.Info("Nvtt library is not loaded, loading");
                NvTexLibrary.Load();
                Logging.Info("Nvtt library loaded");
            }
            else
                Logging.Info("Nvtt library is loaded");
        }

        /// <summary>
        /// Disposes of all statically used AtlasUtils resources, including releasing the mod texture list
        /// </summary>
        public static void DisposeOfAllAtlasResources()
        {
            DisposeparseModTextures();
            if (AtlasBuilders != null)
            {
                for (int i = 0; i < AtlasBuilders.Count; i++)
                {
                    if (AtlasBuilders[i] != null)
                    {
                        AtlasBuilders[i].Dispose();
                        AtlasBuilders[i] = null;
                    }
                }
                AtlasBuilders = null;
            }
        }
    }
}
