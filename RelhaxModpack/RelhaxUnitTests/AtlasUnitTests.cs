using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using RelhaxModpack.Atlases;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using RelhaxModpack.Utilities;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using RelhaxModpack.Atlases.Packing;
using System.Threading;

namespace RelhaxUnitTests
{
    public struct PackerSettings
    {
        public bool powerTwo;
        public bool squareImage;
    }

    [TestClass]
    public class AtlasUnitTests : UnitTestLogBase
    {
        private AtlasCreator AtlasCreator = new AtlasCreator();
        List<PackerSettings> PackerSettingsToTest = new List<PackerSettings>()
        {
            new PackerSettings(){powerTwo = false, squareImage = false},
            new PackerSettings(){powerTwo = true, squareImage = false},
            new PackerSettings(){powerTwo = false, squareImage = true},
            new PackerSettings(){powerTwo = true, squareImage = true},
        };
        string[] AtlasFiles = new string[]
        {
            "vehicleMarkerAtlas",
            "battleAtlas",
        };

        [TestMethod]
        public void LoadLibrariesTest()
        {
            Logfile log = UnitTestHelper.CreateLogfile();
            Assert.IsNotNull(log);
            Assert.IsTrue(log.CanWrite);

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

            UnitTestHelper.DestroyLogfile(ref log, false);
            Assert.IsNull(log);
        }

        [TestMethod]
        public void DdsTest()
        {
            Logfile log = UnitTestHelper.CreateLogfile();
            Assert.IsNotNull(log);
            Assert.IsTrue(log.CanWrite);

            Bitmap LoadedImage = null;

            string testFileIn = Path.Combine(UnitTestHelper.ResourcesFolder, "battleAtlas.dds");
            Assert.IsTrue(File.Exists(testFileIn));

            log.Write("Asserting to load the dds file 'battleAtlas.dds' to Bitmap");
            LoadedImage = AtlasCreator.UnitTestLoadDDS(testFileIn);
            log.Write(string.Format("Load status: {0}", LoadedImage != null));
            Assert.IsNotNull(LoadedImage);
            log.Write(string.Format("Width expected: {0}, actual: {1}",4096,LoadedImage.Width));
            Assert.AreEqual(4096, LoadedImage.Width);
            log.Write(string.Format("Height expected: {0}, actual: {1}", 4512, LoadedImage.Height));
            Assert.AreEqual(4512, LoadedImage.Height);

            string testFileOut = Path.Combine(UnitTestHelper.ResourcesFolder, "battleAtlas2.dds");
            if (File.Exists(testFileOut))
                File.Delete(testFileOut);

            AtlasCreator.Atlas = new Atlas() { AtlasFile = testFileOut };
            log.Write("Asserting to write the Bitmap to dds");
            Assert.IsTrue(AtlasCreator.UnitTestSaveDDS(testFileOut, ref LoadedImage));
            log.Write(string.Format("File written: {0}", File.Exists(testFileOut)));
            Assert.IsTrue(File.Exists(testFileOut));
            File.Delete(testFileOut);
            LoadedImage.Dispose();
            LoadedImage = null;

            UnitTestHelper.DestroyLogfile(ref log, false);
            Assert.IsNull(log);
        }

        [TestMethod]
        public void TextureLoadTest()
        {
            Logfile log = UnitTestHelper.CreateLogfile();
            Assert.IsNotNull(log);
            Assert.IsTrue(log.CanWrite);

            List<Texture> Texturelist = null;
            string testFileIn = Path.Combine(UnitTestHelper.ResourcesFolder, "battleAtlas.xml");
            Assert.IsTrue(File.Exists(testFileIn));
            string testFileOut = Path.Combine(UnitTestHelper.ResourcesFolder, "battleAtlas2.xml");
            if (File.Exists(testFileOut))
                File.Delete(testFileOut);

            XmlDocument textureDocument = new XmlDocument();
            textureDocument.Load(testFileIn);
            int numSubTextures = textureDocument.SelectNodes("//SubTexture").Count;

            log.Write("Asserting to load a xml texture file to Texture list");
            Texturelist = AtlasCreator.UnitTestLoadMapFile(testFileIn);
            log.Write(string.Format("Texture class load status: {0}", Texturelist != null));
            Assert.IsNotNull(Texturelist);
            log.Write(string.Format("Xml node textures: {0}, parsed: {1}", numSubTextures, Texturelist.Count));
            Assert.AreEqual(numSubTextures, Texturelist.Count);

            UnitTestHelper.DestroyLogfile(ref log, false);
            Assert.IsNull(log);
        }

        [TestMethod]
        public void FullAtlasTest()
        {
            Logfile log = UnitTestHelper.CreateLogfile();
            Assert.IsNotNull(log);
            Assert.IsTrue(log.CanWrite);

            //make sure atlas packer's relhax temp folder exists
            if (!Directory.Exists(Settings.RelhaxTempFolderPath))
                Directory.CreateDirectory(Settings.RelhaxTempFolderPath);

            foreach(string atlasPrefix in AtlasFiles)
            {
                string testAtlasOut = Path.Combine(UnitTestHelper.ResourcesFolder, "atlas_out", string.Format("{0}.dds",atlasPrefix));
                string testMapOut = Path.Combine(UnitTestHelper.ResourcesFolder, "atlas_out", string.Format("{0}.xml", atlasPrefix));
                string testMapIn = Path.Combine(UnitTestHelper.ResourcesFolder, string.Format("{0}.xml", atlasPrefix));

                AtlasCreator.Atlas = new Atlas()
                {
                    AtlasFile = "battleAtlas.dds",
                    AtlasHeight = 0, //auto-size
                    AtlasWidth = 0, //auto-size
                    AtlasSaveDirectory = Path.GetDirectoryName(testAtlasOut),
                    DirectoryInArchive = UnitTestHelper.ResourcesFolder, //set the full path without fileanme when it's a copy
                    FastImagePacker = true, //don't change this
                    PowOf2 = true, //changed later
                    Square = true, //changed later
                    Padding = 1, //also don't change this
                    Pkg = string.Empty, //set this to do a file copy rather then unpack
                };

                foreach (PackerSettings settings in PackerSettingsToTest)
                {
                    if (Directory.Exists(AtlasCreator.Atlas.AtlasSaveDirectory))
                        Directory.Delete(AtlasCreator.Atlas.AtlasSaveDirectory, true);
                    Directory.CreateDirectory(AtlasCreator.Atlas.AtlasSaveDirectory);

                    log.Write("Asserting to start the loading of mod textures");
                    List<string> modIconsLocation = new List<string>() { UnitTestHelper.ResourcesFolder };
                    CancellationToken token = new CancellationToken();
                    AtlasUtils.LoadModContourIconsAsync(modIconsLocation, token);

                    log.Write(string.Format("Asserting to create the atlas '{0}' using the following settings:",atlasPrefix));
                    log.Write(string.Format("{0}={1}, {2}={3}", "powerTwo", settings.powerTwo, "squareImage", settings.squareImage));
                    AtlasCreator.Atlas.PowOf2 = settings.powerTwo;
                    AtlasCreator.Atlas.Square = settings.squareImage;
                    AtlasCreator.Atlas.FastImagePacker = true;

                    //actually run the packer
                    FailCode code = AtlasCreator.CreateAtlas();
                    log.Write(string.Format("Packer fail code: {0}", code.ToString()));
                    Assert.AreEqual(FailCode.None, code);
                    Assert.IsTrue(File.Exists(testAtlasOut));
                    Assert.IsTrue(File.Exists(testMapOut));

                    //check the number of texture elements. should not have changed
                    log.Write("Asserting created map file has correct number of textures");
                    XmlDocument textureDocument = new XmlDocument();
                    textureDocument.Load(testMapIn);
                    int numSubTexturesIn = textureDocument.SelectNodes("//SubTexture").Count;

                    textureDocument = new XmlDocument();
                    textureDocument.Load(testMapOut);
                    int numSubTexturesOut = textureDocument.SelectNodes("//SubTexture").Count;
                    log.Write(string.Format("Expected node textures: {0}, actual {1}", numSubTexturesIn, numSubTexturesOut));
                    Assert.AreEqual(numSubTexturesIn, numSubTexturesOut);

                    //dispose of the mod contour icons
                    AtlasUtils.DisposeparseModTextures();
                }
            }

            Directory.Delete(AtlasCreator.Atlas.AtlasSaveDirectory, true);

            UnitTestHelper.DestroyLogfile(ref log, false);
            Assert.IsNull(log);
        }
    }
}
