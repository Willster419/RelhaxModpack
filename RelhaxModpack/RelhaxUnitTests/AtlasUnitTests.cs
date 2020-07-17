using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using RelhaxModpack.Atlases;
using System.IO;
using System.Diagnostics;

namespace RelhaxUnitTests
{
    [TestClass]
    public class AtlasUnitTests : UnitTestLogBase
    {

        [TestMethod]
        public void LoadDdsTest()
        {

        }

        [TestMethod]
        public void LoadLibrariesTest()
        {
            IRelhaxUnmanagedLibrary[] libraries = { AtlasUtils.FreeImageLibrary, AtlasUtils.NvTexLibrary };

            //delete the library folders if they exist
            if (Directory.Exists(Settings.RelhaxLibrariesFolderPath))
                Directory.Delete(Settings.RelhaxLibrariesFolderPath, true);

            //they are started as being statically constructed, but the constructors themselves don't really do anything
            foreach(IRelhaxUnmanagedLibrary library in libraries)
            {
                Assert.IsNotNull(library);

                //extract
                library.Extract();
                Assert.IsNotNull(library.ExtractedFilename);
                Assert.IsNotNull(library.Filepath);
                Assert.IsTrue(library.IsExtracted);

                //load
                Assert.IsFalse(library.IsLoaded);
                library.Load();
                Assert.IsTrue(library.IsLoaded);

                //unload
                library.Unload();
                Assert.IsFalse(library.IsLoaded);
            }
        }
    }
}
