using RelhaxModpack.Common;
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
    /// A class that serves as a xml representation of atlas creation instructions.
    /// </summary>
    public class Atlas : PackageExtractInstruction
    {
        /// <summary>
        /// For atlas instruction files, the xpath to return a list of all atlas instruction xml elements.
        /// </summary>
        /// <remarks>As of the time of this writing, all instructions are now stored inside the database and are no longer separate xml files in the package zip files.</remarks>
        public const string AtlasXmlSearchPath = "/atlases/atlas";

        /// <summary>
        /// Create an instance of the Atlas class.
        /// </summary>
        /// <seealso cref="PackageExtractInstruction"/>
        public Atlas() : base()
        {

        }

        /// <summary>
        /// Create an instance of the Atlas class, copying values from the given atlas instruction.
        /// </summary>
        /// <param name="atlasToCopy">The atlas instruction object to copy.</param>
        /// <seealso cref="Copy(Atlas)"/>
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

        /// <summary>
        /// Create an instance of the Atlas class, copying values from the given atlas instruction.
        /// </summary>
        /// <param name="atlasToCopy">The atlas instruction object to copy.</param>
        /// <returns>A new atlas file with all copied values.</returns>
        /// <seealso cref="Atlas.Atlas(Atlas)"/>
        public static Atlas Copy(Atlas atlasToCopy)
        {
            return new Atlas(atlasToCopy);
        }

        #region Xml serialization V1
        /// <summary>
        /// The xpath to use to get a list of xml element objects that represent each instruction to serialize.
        /// </summary>
        public override string RootObjectPath { get { return AtlasXmlSearchPath; } }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml elements may always exist, but they may have empty inner text values.</remarks>
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
        #endregion

        #region Xml serialization V2
        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.0 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                new XmlDatabaseProperty() { XmlName = nameof(AtlasFile), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(AtlasFile) },
                new XmlDatabaseProperty() { XmlName = nameof(MapFile), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(MapFile) },
                new XmlDatabaseProperty() { XmlName = nameof(PowOf2), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(PowOf2) },
                new XmlDatabaseProperty() { XmlName = nameof(Square), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Square) },
                new XmlDatabaseProperty() { XmlName = nameof(AtlasWidth), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(AtlasWidth) },
                new XmlDatabaseProperty() { XmlName = nameof(AtlasHeight), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(AtlasHeight) },
                new XmlDatabaseProperty() { XmlName = nameof(FastImagePacker), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(FastImagePacker) },
                new XmlDatabaseProperty() { XmlName = nameof(Padding), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Padding) },
                new XmlDatabaseProperty() { XmlName = nameof(AtlasSaveDirectory), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(AtlasSaveDirectory) },
                new XmlDatabaseProperty() { XmlName = nameof(ImageFolders), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(ImageFolders) }
            };
            return base.GetXmlDatabasePropertiesV1Dot0().Concat(xmlDatabaseProperties).ToList();
        }

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.1 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot1()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
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
            return base.GetXmlDatabasePropertiesV1Dot1().Concat(xmlDatabaseProperties).ToList();
        }

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.2 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot2()
        {
            return this.GetXmlDatabasePropertiesV1Dot1();
        }
        #endregion

        /// <summary>
        /// File name of the atlas image file to extract from inside the game's install directory.
        /// </summary>
        /// <remarks>
        /// If loading from an xml instructions file, this is loaded from the xml file.
        /// If the Pkg value is given, this is a location in a package zip file, else it is a file location in the game's install directory.
        /// </remarks>
        public string AtlasFile { get; set; } = string.Empty;

        /// <summary>
        /// File name of the atlas map file to extract from the game's install directory.
        /// </summary>
        /// <remarks>
        /// If loading from an xml instructions file, this is loaded from the xml file.
        /// If the Pkg value is given, this is a location in a package zip file, else it is a file location in the game's install directory.
        /// </remarks>
        public string MapFile { get; set; } = string.Empty;

        /// <summary>
        /// Path to place the generated atlas image file and xml map, relative to the game's install directory.
        /// </summary>
        /// <remarks>If loading from an xml instructions file, this is loaded from the xml file.</remarks>
        public string AtlasSaveDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Width of the new atlases file. 0 = get from original atlas file.
        /// </summary>
        /// <remarks>If loading from an xml instructions file, this is loaded from the xml file.</remarks>
        public int AtlasWidth { get; set; } = 0;

        /// <summary>
        /// Height of the new atlases file. 0 = get from original atlas file.
        /// </summary>
        /// <remarks>If loading from an xml instructions file, this is loaded from the xml file.</remarks>
        public int AtlasHeight { get; set; } = 0;

        /// <summary>
        /// Padding of the new atlases file (amount of pixels as a border between each image)
        /// </summary>
        /// <remarks>If loading from an xml instructions file, this is loaded from the xml file.</remarks>
        public int Padding { get; set; } = 1;

        /// <summary>
        /// Creating an atlas file only with log base 2 numbers (16, 32, 64, etc).
        /// </summary>
        /// <remarks>If loading from an xml instructions file, this is loaded from the xml file.</remarks>
        public bool PowOf2 { get; set; } = false;

        /// <summary>
        /// Creating an atlas file only in a square (same width and height of atlas).
        /// </summary>
        /// <remarks>If loading from an xml instructions file, this is loaded from the xml file.</remarks>
        public bool Square { get; set; } = false;

        /// <summary>
        /// Allow the packer to accept first successful image optimization layout (placement).
        /// </summary>
        /// <remarks>If loading from an xml instructions file, this is loaded from the xml file.</remarks>
        public bool FastImagePacker { get; set; } = true;

        /// <summary>
        /// List of folders that could contain images to replace original images found in the game's atlas file.
        /// </summary>
        /// <remarks>If loading from an xml instructions file, this is loaded from the xml file.</remarks>
        public List<string> ImageFolders { get; set; } = new List<string>();

        /// <summary>
        /// The list of texture objects in each atlas.
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        public List<Texture> TextureList { get; set; } = new List<Texture>();

        /// <summary>
        /// The file path where the original atlas image file will be extracted/copied to, relative to the application's temporary directory.
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        /// <seealso cref="ApplicationConstants.RelhaxTempFolderPath"/>
        public string TempAtlasImageFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The file path where the original atlas map file will be extracted/copied to, relative to the application's temporary directory.
        /// </summary>
        /// <remarks>This is *not* loaded from the xml file and is used internally</remarks>
        /// <seealso cref="ApplicationConstants.RelhaxTempFolderPath"/>
        public string TempAtlasMapFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The file path where the created atlas image file will be placed.
        /// </summary>
        /// <remarks>
        /// This is *not* loaded from the xml file and is used internally.
        /// This is created by combining the AtlasSaveDirectory and AtlasFile properties.
        /// </remarks>
        /// <seealso cref="AtlasSaveDirectory"/>
        /// <seealso cref="AtlasFile"/>
        public string AtlasImageFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The file path where the created map file will be placed.
        /// </summary>
        /// <remarks>
        /// This is *not* loaded from the xml file and is used internally.
        /// This is created by combining the AtlasSaveDirectory and AtlasFile properties.
        /// </remarks>
        public string AtlasMapFilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>The atlas file name, or "(empty)", if no atlas name is given.</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", nameof(AtlasFile), string.IsNullOrEmpty(AtlasFile) ? "(empty)" : AtlasFile);
        }

        /// <summary>
        /// Gets a log formatted string for debugging containing key object name and values.
        /// </summary>
        /// <remarks>If debug output is enabled for the log file during an installation, then each instruction will have it's DumpInfoToLog property called.</remarks>
        public override string DumpInfoToLog
        {
            get
            {
                return $"{base.DumpInfoToLog}{Environment.NewLine}{ApplicationConstants.LogSpacingLineup}{nameof(AtlasFile)}={AtlasFile} {nameof(MapFile)}={MapFile}{Environment.NewLine}{ApplicationConstants.LogSpacingLineup}" +
                    $"{nameof(PowOf2)}={PowOf2} {nameof(Square)}={Square} {nameof(AtlasWidth)}={AtlasWidth} {nameof(AtlasHeight)}={AtlasHeight} {nameof(FastImagePacker)}={FastImagePacker} {nameof(Padding)}={Padding}" +
                    $"{Environment.NewLine}{ApplicationConstants.LogSpacingLineup}{nameof(AtlasSaveDirectory)}={AtlasSaveDirectory}{Environment.NewLine}{ApplicationConstants.LogSpacingLineup}" +
                    $"{nameof(ImageFolders)}={string.Join(", ", ImageFolders)}";
            }
        }

        /// <summary>
        /// Compares two instructions to determine if their values are equal.
        /// </summary>
        /// <param name="instructionToCompare">The instruction to compare against.</param>
        /// <returns>True if the compared values are equal, false otherwise.</returns>
        public override bool InstructionsEqual(Instruction instructionToCompare)
        {
            if (!base.InstructionsEqual(instructionToCompare))
                return false;

            Atlas atlasToCompare = instructionToCompare as Atlas;

            if (atlasToCompare == null)
                return false;

            if (!this.AtlasFile.Equals(atlasToCompare.AtlasFile))
                return false;

            if (!this.MapFile.Equals(atlasToCompare.MapFile))
                return false;

            if (!this.PowOf2.Equals(atlasToCompare.PowOf2))
                return false;

            if (!this.Square.Equals(atlasToCompare.Square))
                return false;

            if (!this.AtlasWidth.Equals(atlasToCompare.AtlasWidth))
                return false;

            if (!this.AtlasHeight.Equals(atlasToCompare.AtlasHeight))
                return false;

            if (!this.FastImagePacker.Equals(atlasToCompare.FastImagePacker))
                return false;

            if (!this.Padding.Equals(atlasToCompare.Padding))
                return false;

            if (!this.AtlasSaveDirectory.Equals(atlasToCompare.AtlasSaveDirectory))
                return false;

            if (this.ImageFolders.Count != atlasToCompare.ImageFolders.Count)
                return false;

            for (int i = 0; i < this.ImageFolders.Count; i++)
                if (!this.ImageFolders[i].Equals(atlasToCompare.ImageFolders[i]))
                    return false;

            return true;
        }
    }
}
