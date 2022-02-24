using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using TeximpNet.Unmanaged;
using Ionic.Zip;
using RelhaxModpack.Atlases;
using TeximpNet;
using RelhaxModpack.Utilities;
using RelhaxModpack.Common;

namespace RelhaxModpack
{
    /// <summary>
    /// A wrapper class around the TexImpNet FreeImage library class.
    /// </summary>
    /// <remarks>The class handles 32 and 64 bit library loading determination, extraction, and loading into memory.</remarks>
    public class RelhaxFreeImageLibrary : IRelhaxUnmanagedLibrary
    {
        private FreeImageLibrary library = FreeImageLibrary.Instance;

        /// <summary>
        /// Gets the name of the embedded zip file containing the dll, 32 or 64 bit version.
        /// </summary>
        public string EmbeddedFilename
        {
            get { return UnmanagedLibrary.Is64Bit ? "FreeImage64.zip" : "FreeImage32.zip"; }
        }

        /// <summary>
        /// Gets the name of the dll file inside the embedded zip file, 32 or 64bit version.
        /// </summary>
        public string ExtractedFilename
        {
            get { return UnmanagedLibrary.Is64Bit ? "FreeImage64.dll" : "FreeImage32.dll"; }
        }

        /// <summary>
        /// Gets the absolute path to the dll file.
        /// </summary>
        public string Filepath
        {
            get { return Path.Combine(ApplicationConstants.RelhaxLibrariesFolderPath, ExtractedFilename); }
        }

        /// <summary>
        /// Determines if the file is extracted to the Filepath property location. Also checks if latest version is extracted.
        /// </summary>
        public bool IsExtracted
        {
            get
            {
                if (!File.Exists(Filepath))
                {
                    Logging.Info("Teximpnet library file does not exist, extracting: {0}", EmbeddedFilename);
                    return false;
                }

                Logging.Info("Teximpnet library file exists, checking if latest via Hash comparison not exist, extracting: {0}", EmbeddedFilename);
                string extractedHash = FileUtils.CreateMD5Hash(Filepath);
                Logging.Debug("{0} hash local:    {1}", EmbeddedFilename, extractedHash);

                //file exists, but is it up to date?
                string embeddedHash = string.Empty;
                string resourceName = CommonUtils.GetAssemblyName(EmbeddedFilename);
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                using (ZipFile zout = ZipFile.Read(stream))
                using (Stream dllStream = zout[0].OpenReader())
                {
                    embeddedHash = FileUtils.CreateMD5Hash(dllStream);
                }
                Logging.Debug("{0} hash internal: {1}", EmbeddedFilename, embeddedHash);
                return extractedHash.Equals(embeddedHash);
            }
        }

        /// <summary>
        /// Determines if the library is loaded into memory.
        /// </summary>
        public bool IsLoaded
        {
            get { return library.IsLibraryLoaded; }
        }

        /// <summary>
        /// Attempts to load the library using the Filepath property.
        /// </summary>
        /// <returns>True if the library load was successful.</returns>
        public bool Load()
        {
            if (!IsExtracted)
                Extract();

            try
            {
                return library.LoadLibrary(Filepath);
            }
            catch (TeximpException ex)
            {
                Logging.Exception("failed to load native library");
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Attempts to unload the library.
        /// </summary>
        /// <returns>True if the library was unloaded, false otherwise.</returns>
        public bool Unload()
        {
            if (!IsLoaded)
                return true;
            else
            {
                return library.FreeLibrary();
            }
        }

        /// <summary>
        /// Extracts the embedded compressed library to the location in the Filepath property.
        /// </summary>
        public void Extract()
        {
            if(IsExtracted)
            {
                Logging.Warning("Unmanaged library {0} is already extracted", EmbeddedFilename);
                return;
            }

            if (File.Exists(Filepath))
                File.Delete(Filepath);

            //https://stackoverflow.com/questions/38381684/reading-zip-file-from-byte-array-using-ionic-zip
            string resourceName = CommonUtils.GetAssemblyName(EmbeddedFilename);
            Logging.Info("Extracting unmanaged teximpnet library: {0}", EmbeddedFilename);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (ZipFile zout = ZipFile.Read(stream))
            {
                zout.ExtractAll(ApplicationConstants.RelhaxLibrariesFolderPath);
            }
        }
    }
}
