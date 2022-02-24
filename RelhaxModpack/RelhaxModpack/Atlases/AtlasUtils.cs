using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
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
    /// A utility class for Atlas file processing.
    /// </summary>
    public static class AtlasUtils
    {
        /// <summary>
        /// The link to the Microsoft Visual C++ dll package required by the atlas processing libraries.
        /// </summary>
        public const string MSVCPLink = "https://www.microsoft.com/en-us/download/details.aspx?id=40784";

        /// <summary>
        /// The manager instance of the FreeImage Library.
        /// </summary>
        public static RelhaxFreeImageLibrary FreeImageLibrary = new RelhaxFreeImageLibrary();

        /// <summary>
        /// The manager instance of the Nvidia Texture Tools Library.
        /// </summary>
        public static RelhaxNvTexLibrary NvTexLibrary = new RelhaxNvTexLibrary();

        /// <summary>
        /// Test the ability to load an unmanaged library.
        /// </summary>
        /// <returns>True if library loaded, false otherwise.</returns>
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
        /// Test the ability to load and unload all the atlas image processing libraries.
        /// </summary>
        /// <returns>True if both libraries loaded, false otherwise.</returns>
        public static bool TestLoadAtlasLibraries(bool unload)
        {
            bool freeImageLoaded = TestLibrary(FreeImageLibrary, "FreeImage", true);
            bool nvttLoaded = TestLibrary(NvTexLibrary, "nvtt", true);

            if (nvttLoaded && freeImageLoaded)
            {
                Logging.Info(LogOptions.MethodName, "Both libraries loaded");
                return true;
            }
            else
            {
                Logging.Error(LogOptions.MethodName, "Failed to load one or more atlas processing libraries: freeImage={0}, nvtt={1}",
                    freeImageLoaded.ToString(), nvttLoaded.ToString());
                return false;
            }
        }

        /// <summary>
        /// Lock object used in AtlasCreator for critical sections that can't be done at the same time.
        /// </summary>
        /// <remarks>Each atlas file is created by its own thread. However, the DDS loading and saving API used can't be used more then once at a time.</remarks>
        public static object AtlasLoaderLockObject { get; } = new object();

        /// <summary>
        /// A list of Atlas creating thread engines.
        /// </summary>
        public static List<AtlasCreator> AtlasBuilders = null;

        /// <summary>
        /// Verifies the Atlas processing libraries are loaded. If not, they are loaded.
        /// </summary>
        public static void VerifyImageLibsLoaded()
        {
            if (!FreeImageLibrary.IsLoaded)
            {
                Logging.Info(LogOptions.MethodName, "Freeimage library is not loaded, loading");
                FreeImageLibrary.Load();
                Logging.Info(LogOptions.MethodName, "Freeimage library loaded");
            }
            else
                Logging.Info(LogOptions.MethodName, "Freeimage library is loaded");

            if (!NvTexLibrary.IsLoaded)
            {
                Logging.Info(LogOptions.MethodName, "Nvtt library is not loaded, loading");
                NvTexLibrary.Load();
                Logging.Info(LogOptions.MethodName, "Nvtt library loaded");
            }
            else
                Logging.Info(LogOptions.MethodName, "Nvtt library is loaded");
        }

        /// <summary>
        /// Disposes of all statically used AtlasUtils resources, including releasing the custom texture list.
        /// </summary>
        public static void DisposeOfAllAtlasResources()
        {
            AtlasCreator.DisposeParsedCustomTextures();
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
