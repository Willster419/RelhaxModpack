using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.AtlasesCreator
{
    /// <summary>
    /// A utility class for Atlas file processing
    /// </summary>
    public static class AtlasUtils
    {
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
                    string realFolder = Utils.MacroReplace(folder, ReplacementTypes.FilePath);
                    Logging.Info("[ParseModTexturesTask]: checking for mod contour icon images in directory {0}", realFolder);
                    token.ThrowIfCancellationRequested();

                    if (!Directory.Exists(realFolder))
                    {
                        Logging.Warning("[ParseModTexturesTask]: directory {0} does not exist, skipping", realFolder);
                        continue;
                    }

                    ModContourIconFilesList.AddRange(Utils.DirectorySearch(realFolder, SearchOption.TopDirectoryOnly, false, "*", 5, 3, false));
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
            if (!Utils.FreeImageLibrary.IsLoaded)
            {
                Logging.Info("freeimage library is not loaded, loading");
                Utils.FreeImageLibrary.Load();
                Logging.Info("freeimage library loaded");
            }
            else
                Logging.Info("freeimage library is loaded");

            if (!Utils.NvTexLibrary.IsLoaded)
            {
                Logging.Info("nvtt library is not loaded, loading");
                Utils.NvTexLibrary.Load();
                Logging.Info("nvtt library loaded");
            }
            else
                Logging.Info("nvtt library is loaded");
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
