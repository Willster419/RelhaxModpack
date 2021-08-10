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
using System.Threading.Tasks;
using System.Reflection;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Common;

namespace RelhaxUnitTests
{
    public struct PackerSettings
    {
        public bool PowerTwo;
        public bool SquareImage;
    }

    public struct ImageHandlerSettings
    {
        public string Filename;
        public int Height;
        public int Width;
    }

    [TestClass]
    public class Set02_AtlasUnitTests
    {
        List<PackerSettings> PackerSettingsToTest = new List<PackerSettings>()
        {
            new PackerSettings(){PowerTwo = false, SquareImage = false},
            new PackerSettings(){PowerTwo = true, SquareImage = false},
            new PackerSettings(){PowerTwo = false, SquareImage = true},
            new PackerSettings(){PowerTwo = true, SquareImage = true},
        };

        string[] AtlasFiles = new string[]
        {
            "vehicleMarkerAtlas",
            "battleAtlas",
        };

        Logfile log;

        private void LoadLibrariesForTest()
        {
            IRelhaxUnmanagedLibrary[] libraries = { AtlasUtils.FreeImageLibrary, AtlasUtils.NvTexLibrary };

            foreach (IRelhaxUnmanagedLibrary library in libraries)
            {
                if (!library.IsLoaded)
                    Assert.IsTrue(library.Load());
            }
        }

        private void UnloadLibrariesForTest()
        {
            IRelhaxUnmanagedLibrary[] libraries = { AtlasUtils.FreeImageLibrary, AtlasUtils.NvTexLibrary };

            foreach (IRelhaxUnmanagedLibrary library in libraries)
            {
                if (library.IsLoaded)
                    Assert.IsTrue(library.Unload());
            }
        }

        private void Ensure64BitProcess()
        {
            Assert.IsTrue(Environment.Is64BitProcess);
        }

        [TestInitialize]
        public void Initialize()
        {
            log = UnitTestHelper.CreateLogfile();
            Assert.IsNotNull(log);
            Assert.IsTrue(log.CanWrite);
        }

        [TestCleanup]
        public void Cleanup()
        {
            UnitTestHelper.DestroyLogfile(ref log, false);
            Assert.IsNull(log);
            UnloadLibrariesForTest();
        }

        [TestMethod]
        public void Test01_LoadLibrariesTest()
        {
            IRelhaxUnmanagedLibrary[] libraries = { AtlasUtils.FreeImageLibrary, AtlasUtils.NvTexLibrary };

            //unload them if already loaded, just in case
            foreach (IRelhaxUnmanagedLibrary library in libraries)
            {
                if (library.IsLoaded)
                    library.Unload();
            }

            //delete the library folders if they exist
            if (Directory.Exists(ApplicationConstants.RelhaxLibrariesFolderPath))
                Directory.Delete(ApplicationConstants.RelhaxLibrariesFolderPath, true);

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

        [TestMethod]
        public void Test02_TextureTest()
        {
            //create objects
            List<Texture> texturelist = null;
            XmlNodeList xmlTextureList = null;
            MapHandler mapHandler = new MapHandler();
            ImagePacker imagePacker = new ImagePacker();
            XmlDocument textureDocument = new XmlDocument();

            //setup paths
            string testFileIn = Path.Combine(UnitTestHelper.ResourcesFolder, "battleAtlas.xml");
            Assert.IsTrue(File.Exists(testFileIn));
            string testFileOut = Path.Combine(UnitTestHelper.ResourcesFolder, "battleAtlas2.xml");
            if (File.Exists(testFileOut))
                File.Delete(testFileOut);

            //load sub-texture count to xml
            textureDocument.Load(testFileIn);
            xmlTextureList = textureDocument.SelectNodes("//SubTexture");
            int numSubTextures = xmlTextureList.Count;

            //load texture list
            log.Write("Asserting to load a xml texture file to Texture list");
            texturelist = mapHandler.LoadMapFile(testFileIn);
            log.Write(string.Format("Texture class load status: {0}", texturelist != null));
            Assert.IsNotNull(texturelist);

            //compare texture count
            log.Write(string.Format("Xml node textures: {0}, parsed: {1}", numSubTextures, texturelist.Count));
            Assert.AreEqual(numSubTextures, texturelist.Count);

            //for the packer, need to create bitmaps. we won't use them
            foreach (Texture tex in texturelist)
            {
                tex.AtlasImage = new Bitmap(1, 1);
            }

            //compare each individual texture
            log.Write("Asserting each texture matches from xml to texture list");
            for (int i = 0; i < texturelist.Count; i++)
            {
                Texture texture = texturelist[i];
                XmlElement xmlTexture = xmlTextureList[i] as XmlElement;

                //properties match lowercase xml properties names
                foreach (string propertyName in texture.PropertiesForSerializationElements())
                {
                    XmlElement element = xmlTexture.SelectSingleNode(propertyName.ToLower()) as XmlElement;
                    Assert.IsNotNull(element);
                    string xmlValue = element.InnerText.Trim();
                    //for reference
                    //PropertyInfo property = listObjectType.GetProperty(attributeName);
                    PropertyInfo property = typeof(Texture).GetProperty(propertyName);
                    Assert.IsNotNull(property);
                    object value = property.GetValue(texture);
                    Assert.IsNotNull(value);
                    Assert.AreEqual(xmlValue, value.ToString());
                }
            }

            //pack textures
            FailCode code = imagePacker.PackImage(texturelist, true, false, true, 8192, 8192, 1, "battleAtlas.dds", out Bitmap map, out Dictionary<string, Rectangle> imageSizes);
            log.Write(string.Format("Packer fail code: {0}", code.ToString()));
            Assert.AreEqual(FailCode.None, code);

            //for the packer, need to dispose bitmaps
            map.Dispose();
            foreach (Texture tex in texturelist)
            {
                tex.AtlasImage.Dispose();
            }

            //save to new xml file
            mapHandler.SaveMapfile(testFileOut, imageSizes);
            Assert.IsTrue(File.Exists(testFileOut));

            //compare texture count
            textureDocument = new XmlDocument();
            textureDocument.Load(testFileOut);
            xmlTextureList = textureDocument.SelectNodes("//SubTexture");
            numSubTextures = xmlTextureList.Count;
            log.Write(string.Format("Xml node textures: {0}, parsed: {1}", numSubTextures, imageSizes.Count));
            Assert.AreEqual(numSubTextures, imageSizes.Count);

            //compare each individual texture
            log.Write("Asserting each texture matches from xml to dictionary");
            for (int i = 0; i < xmlTextureList.Count; i++)
            {
                XmlElement xmlTexture = xmlTextureList[i] as XmlElement;
                Assert.IsNotNull(xmlTexture);
                string textureName = xmlTexture.SelectSingleNode(nameof(Texture.Name).ToLower()).InnerText.Trim();
                Rectangle imageSize = imageSizes[textureName];
                Assert.IsNotNull(imageSize);

                string[] propertiesToCheck = { nameof(imageSize.X), nameof(imageSize.Y), nameof(imageSize.Width), nameof(imageSize.Height) };
                foreach(string propertyName in propertiesToCheck)
                {
                    XmlElement xmlProperty = xmlTexture.SelectSingleNode(propertyName.ToLower()) as XmlElement;
                    Assert.IsNotNull(xmlProperty);

                    PropertyInfo property = imageSize.GetType().GetProperty(propertyName);
                    Assert.IsNotNull(property);
                    object propertyValue = property.GetValue(imageSize);
                    Assert.IsNotNull(propertyValue);
                    Assert.IsTrue(propertyValue is int);

                    Assert.AreEqual(xmlProperty.InnerText.Trim(), propertyValue.ToString());
                }
            }

            File.Delete(testFileOut);
        }

        [TestMethod]
        public void Test03_DdsTest()
        {
            Ensure64BitProcess();
            LoadLibrariesForTest();

            ImageHandlerSettings[] imageHandlerSettings =
            {
                new ImageHandlerSettings(){ Filename="vehicleMarkerAtlas.dds", Height=1024, Width=2048 },
                new ImageHandlerSettings(){ Filename="battleAtlas.dds", Height=4512, Width=4096 }
            };

            string outputPath = Path.Combine(UnitTestHelper.ResourcesFolder, "dds_test_out");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            foreach (ImageHandlerSettings settings in imageHandlerSettings)
            {
                ImageHandler handler = new ImageHandler();
                string testFileIn = Path.Combine(UnitTestHelper.ResourcesFolder, settings.Filename);
                string testFileOut = Path.Combine(outputPath, settings.Filename);
                Assert.IsTrue(File.Exists(testFileIn));
                if (File.Exists(testFileOut))
                    File.Delete(testFileOut);

                log.Write("Asserting to load the DDS file 'battleAtlas.dds' to Bitmap");
                Bitmap loadedImage = handler.LoadDDS(testFileIn);
                log.Write(string.Format("Load status: {0}", loadedImage != null));
                Assert.IsNotNull(loadedImage);
                log.Write(string.Format("Width expected: {0}, actual: {1}", settings.Width, loadedImage.Width));
                Assert.AreEqual(settings.Width, loadedImage.Width);
                log.Write(string.Format("Height expected: {0}, actual: {1}", settings.Height, loadedImage.Height));
                Assert.AreEqual(settings.Height, loadedImage.Height);

                log.Write("Asserting to write the Bitmap to DDS");
                Assert.IsTrue(handler.SaveDDS(testFileOut, loadedImage, true));
                log.Write(string.Format("File written: {0}", File.Exists(testFileOut)));
                Assert.IsTrue(File.Exists(testFileOut));
                File.Delete(testFileOut);
            }

            Directory.Delete(outputPath, true);
        }

        [TestMethod]
        public void Test04_CustomIconsLoadingTest()
        {
            log.Write("Asserting to start the loading of mod textures");
            List<string> modIconsLocation = new List<string>() { UnitTestHelper.ResourcesFolder };
            CancellationToken token = new CancellationToken();
            Task loadCustomContourIconsTask = AtlasCreator.LoadCustomContourIconsAsync(modIconsLocation, token);
            Assert.IsNotNull(loadCustomContourIconsTask);
            Assert.IsNotNull(AtlasUtils.AtlasLoaderLockObject);

            //wait
            loadCustomContourIconsTask.Wait();
            log.Write(string.Format("Task status: {0}", loadCustomContourIconsTask.Status.ToString()));
            Assert.IsTrue(loadCustomContourIconsTask.Status == TaskStatus.RanToCompletion);

            log.Write("Asserting each texture start point is (0,0) and width, height = image width, height");
            Assert.IsNotNull(AtlasCreator.CustomContourIconImages);
            foreach (Texture texture in AtlasCreator.CustomContourIconImages)
            {
                Assert.IsNotNull(texture.AtlasImage);
                Assert.IsTrue(texture.X == 0);
                Assert.IsTrue(texture.Y == 0);
                Assert.IsTrue(texture.Width == texture.AtlasImage.Width);
                Assert.IsTrue(texture.Height == texture.AtlasImage.Height);
            }

            //dispose of the mod contour icons
            AtlasCreator.DisposeParsedCustomTextures();
        }

        [TestMethod]
        public void Test05_FullAtlasTest()
        {
            Ensure64BitProcess();
            LoadLibrariesForTest();

            //make sure atlas packer's relhax temp folder exists
            if (!Directory.Exists(ApplicationConstants.RelhaxTempFolderPath))
                Directory.CreateDirectory(ApplicationConstants.RelhaxTempFolderPath);
            AtlasCreator atlasCreator = null;

            foreach (string atlasPrefix in AtlasFiles)
            using (atlasCreator = new AtlasCreator())
            {
                string testAtlasOut = Path.Combine(UnitTestHelper.ResourcesFolder, "atlas_out", string.Format("{0}.dds",atlasPrefix));
                string testMapOut = Path.Combine(UnitTestHelper.ResourcesFolder, "atlas_out", string.Format("{0}.xml", atlasPrefix));
                string testMapIn = Path.Combine(UnitTestHelper.ResourcesFolder, string.Format("{0}.xml", atlasPrefix));

                atlasCreator.Atlas = new Atlas()
                {
                    AtlasFile = "battleAtlas.dds", //changed later
                    AtlasHeight = 0, //auto-size
                    AtlasWidth = 0, //auto-size
                    AtlasSaveDirectory = Path.GetDirectoryName(testAtlasOut),
                    DirectoryInArchive = UnitTestHelper.ResourcesFolder, //set the full path without filename when it's a copy
                    FastImagePacker = true, //don't change this
                    PowOf2 = true, //changed later
                    Square = true, //changed later
                    Padding = 1, //also don't change this
                    Pkg = string.Empty, //set this to do a file copy rather then unpack
                };

                foreach (PackerSettings settings in PackerSettingsToTest)
                {
                    if (Directory.Exists(atlasCreator.Atlas.AtlasSaveDirectory))
                        Directory.Delete(atlasCreator.Atlas.AtlasSaveDirectory, true);
                    Directory.CreateDirectory(atlasCreator.Atlas.AtlasSaveDirectory);

                    log.Write("Asserting to start the loading of mod textures");
                    List<string> modIconsLocation = new List<string>() { UnitTestHelper.ResourcesFolder };
                    CancellationToken token = new CancellationToken();
                    AtlasCreator.LoadCustomContourIconsAsync(modIconsLocation, token);

                    log.Write(string.Format("Asserting to create the atlas '{0}' using the following settings:",atlasPrefix));
                    log.Write(string.Format("{0}={1}, {2}={3}", "powerTwo", settings.PowerTwo, "squareImage", settings.SquareImage));
                    atlasCreator.Atlas.PowOf2 = settings.PowerTwo;
                    atlasCreator.Atlas.Square = settings.SquareImage;
                    atlasCreator.Atlas.FastImagePacker = true;
                    atlasCreator.Atlas.AtlasFile = Path.GetFileName(testAtlasOut);

                    //actually run the packer
                    FailCode code = atlasCreator.CreateAtlas();
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
                    AtlasCreator.DisposeParsedCustomTextures();
                }
            }

            Directory.Delete(atlasCreator.Atlas.AtlasSaveDirectory, true);
        }
    }
}
