using RelhaxModpack.Database;
using RelhaxModpack.Installer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack.Atlases
{
    /// <summary>
    /// A class that serves as a description of an atlas file with processing instructions
    /// </summary>
    public class Atlas : PackageExtractInstruction
    {
        public const string AtlasXmlSearchPath = "/atlases/atlas";

        public override string RootObjectPath { get { return AtlasXmlSearchPath; } }

        public override string[] PropertiesToSerialize()
        {
            return new string[]
            {
                nameof(Pkg),
                nameof(DirectoryInArchive),
                nameof(AtlasFile),
                nameof(MapFile),
                nameof(PowOf2),
                nameof(Square),
                nameof(AtlasWidth),
                nameof(AtlasHeight),
                nameof(FastImagePacker),
                nameof(Padding),
                nameof(AtlasSaveDirectory),
                nameof(ImageFolders)
            };
        }

        #region Xml serialization V2
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(AtlasFile), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(AtlasFile) },
                new XmlDatabaseProperty() { XmlName = nameof(MapFile), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(MapFile) },
                new XmlDatabaseProperty() { XmlName = nameof(PowOf2), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PowOf2) },
                new XmlDatabaseProperty() { XmlName = nameof(Square), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Square) },
                new XmlDatabaseProperty() { XmlName = nameof(AtlasWidth), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(AtlasWidth) },
                new XmlDatabaseProperty() { XmlName = nameof(AtlasHeight), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(AtlasHeight) },
                new XmlDatabaseProperty() { XmlName = nameof(FastImagePacker), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(FastImagePacker) },
                new XmlDatabaseProperty() { XmlName = nameof(Padding), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Padding) },
                new XmlDatabaseProperty() { XmlName = nameof(AtlasSaveDirectory), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(AtlasSaveDirectory) },
                new XmlDatabaseProperty() { XmlName = nameof(ImageFolders), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(ImageFolders) }
            };
            return base.GetXmlDatabasePropertiesV1Dot0().Concat(xmlDatabaseProperties).ToList();
        }
        #endregion

        /// <summary>
        /// File name of the atlas image file to extract
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public string AtlasFile { get; set; } = string.Empty;

        /// <summary>
        /// File name of the atlas map file to extract
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public string MapFile { get; set; } = string.Empty;

        /// <summary>
        /// Path to place the generated atlas image file and xml map
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public string AtlasSaveDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Width of the new atlases file. 0 = get from original atlas file
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public int AtlasWidth { get; set; } = 0;

        /// <summary>
        /// Height of the new atlases file. 0 = get from original atlas file
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public int AtlasHeight { get; set; } = 0;

        /// <summary>
        /// Padding of the new atlases file (amount of pixels as a border between each image)
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public int Padding { get; set; } = 1;

        /// <summary>
        /// Creating an atlas file only with log base 2 numbers (16, 32, 64, etc.)
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public bool PowOf2 { get; set; } = false;

        /// <summary>
        /// Creating an atlas file only in a square (same width and height of atlas)
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public bool Square { get; set; } = false;

        /// <summary>
        /// allow to accept first successful image optimization layout
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public bool FastImagePacker { get; set; } = true;

        /// <summary>
        /// List of folders that could contain images to replace original images
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public List<string> ImageFolders { get; set; } = new List<string>();

        /// <summary>
        /// The list of textures in each atlas
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        public List<Texture> TextureList { get; set; } = new List<Texture>();

        /// <summary>
        /// The file path where the original atlas image file will be extracted/copied to
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        public string TempAtlasImageFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The file path where the original atlas map file will be extracted/copied to
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        public string TempAtlasMapFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The file path where the created atlas image file will be placed
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        public string AtlasImageFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The file path where the created map file will be placed
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        public string AtlasMapFilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns>The atlas file name</returns>
        public override string ToString()
        {
            return string.Format("AtlasFile: {0}", string.IsNullOrEmpty(AtlasFile) ? "(empty)" : AtlasFile);
        }

        public static Atlas Copy(Atlas atlasToCopy)
        {
            return new Atlas(atlasToCopy);
        }

        public Atlas() : base()
        {

        }

        public Atlas(Atlas atlasToCopy) : base(atlasToCopy)
        {
            this.AtlasFile = atlasToCopy.AtlasFile;
            this.MapFile = atlasToCopy.MapFile;
            this.PowOf2 = atlasToCopy.PowOf2;
            this.Square = atlasToCopy.Square;
            this.AtlasWidth = atlasToCopy.AtlasWidth;
            this.AtlasHeight = atlasToCopy.AtlasHeight;
            this.FastImagePacker = atlasToCopy.FastImagePacker;
            this.Padding = atlasToCopy.Padding;
            this.AtlasSaveDirectory = atlasToCopy.AtlasSaveDirectory;

            foreach (string s in atlasToCopy.ImageFolders)
                this.ImageFolders.Add(s);
        }
    }
}
