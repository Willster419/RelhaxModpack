using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using TeximpNet.Unmanaged;
using Ionic.Zip;

namespace RelhaxModpack
{
    public class RelhaxFreeImageLibrary
    {
        private FreeImageLibrary library = FreeImageLibrary.Instance;

        public string EmbeddedFilename
        {
            get { return UnmanagedLibrary.Is64Bit ? "FreeImage64.zip" : "FreeImage32.zip"; }
        }

        public string ExtractedFilename
        {
            get { return UnmanagedLibrary.Is64Bit ? "FreeImage64.dll" : "FreeImage32.dll"; }
        }

        public string Filepath
        {
            get
            { return Path.Combine(Settings.RelhaxLibrariesFolder, ExtractedFilename); }
        }

        public bool IsExtracted
        {
            get
            { return File.Exists(Filepath); }
        }

        public bool IsLoaded
        {
            get
            { return library.IsLibraryLoaded; }
        }

        public bool Load()
        {
            if (!IsExtracted)
                Extract();
            return library.LoadLibrary(Filepath);
        }

        public void Extract()
        {
            if(IsExtracted)
            {
                Logging.Warning("Unmanaged library {0} is already extracted", EmbeddedFilename);
                return;
            }
            //https://stackoverflow.com/questions/38381684/reading-zip-file-from-byte-array-using-ionic-zip
            string resourceName = Utils.GetAssemblyName(EmbeddedFilename);
            Logging.Info("Extracting unmanaged teximpnet library: {0}", EmbeddedFilename);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (ZipFile zout = ZipFile.Read(stream))
            {
                zout.ExtractAll(Settings.RelhaxLibrariesFolder);
            }
        }
    }
}
