using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using RelhaxModpack.Common;
using RelhaxModpack.Installer;
using RelhaxModpack.Database;

namespace RelhaxModpack.Patching
{
    /// <summary>
    /// A patch is an instruction object of how to modify a text file. Can be a json, xml, or plain text file
    /// </summary>
    public class Patch : Instruction
    {
        /// <summary>
        /// One of two string values used for regex type patches
        /// </summary>
        public const string TypeRegex1 = "regex";

        /// <summary>
        /// One of two string values used for regex type patches
        /// </summary>
        public const string TypeRegex2 = "regx";

        /// <summary>
        /// The string value used for xml type patches
        /// </summary>
        public const string TypeXml = "xml";

        /// <summary>
        /// The string value used for json type patches
        /// </summary>
        public const string TypeJson = "json";

        /// <summary>
        /// The string value used for xvm type legacy patches
        /// </summary>
        public const string TypeXvm = "xvm";

        /// <summary>
        /// A list of all supported xml patch operation modes
        /// </summary>
        public static readonly string[] ValidXmlModes = new string[]
        {
            "add",
            "edit",
            "remove"
        };

        
        /// <summary>
        /// A list of all supported json patch operation modes
        /// </summary>
        public static readonly string[] ValidJsonModes = new string[]
        {
            "add",
            "arrayadd",
            "remove",
            "arrayremove",
            "edit",
            "arrayedit",
            "arrayclear"
        };

        /// <summary>
        /// For patch instruction files, the xpath to return a list of all patch instruction xml elements.
        /// </summary>
        /// <remarks>As of the time of this writing, all instructions are now stored inside the database and are no longer separate xml files in the package zip files.</remarks>
        public const string PatchXmlSearchPath = "/patchs/patch";

        /// <summary>
        /// Create an instance of the Patch class
        /// </summary>
        public Patch() : base()
        {

        }

        /// <summary>
        /// Create an instance of the Patch class, copying values from a given patch.
        /// </summary>
        /// <param name="patchToCopy">The Patch instance to copy from.</param>
        public Patch(Patch patchToCopy) : base(patchToCopy)
        {
            this.Path = patchToCopy.Path;
            this.Type = patchToCopy.Type;
            this.Mode = patchToCopy.Mode;
            this.PatchPath = patchToCopy.PatchPath;
            this.File = patchToCopy.File;
            this.Version = patchToCopy.Version;
            this.Search = patchToCopy.Search;
            this.Replace = patchToCopy.Replace;
            this.FollowPath = patchToCopy.FollowPath;
            this.Path = patchToCopy.Path;
            this.Line = patchToCopy.Line;
        }

        /// <summary>
        /// Make a copy of a Patch instance
        /// </summary>
        /// <param name="shortcutToCopy">The Patch instance to copy from.</param>
        /// <returns>A copy of the given Patch instance.</returns>
        public static Patch Copy(Patch shortcutToCopy)
        {
            return new Patch(shortcutToCopy);
        }

        #region Xml serialization V1
        /// <summary>
        /// The xpath to use to get a list of xml element objects that represent each instruction to serialize.
        /// </summary>
        public override string RootObjectPath { get { return PatchXmlSearchPath; } }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml elements may always exist, but they may have empty inner text values.</remarks>
        public override string[] PropertiesToSerialize()
        {
            return new string[]
            {
                nameof(Type),
                nameof(Mode),
                nameof(PatchPath),
                nameof(FollowPath),
                nameof(File),
                nameof(Path),
                nameof(Version),
                nameof(Line),
                nameof(Search),
                nameof(Replace)
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
                new XmlDatabaseProperty() { XmlName = nameof(Type), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Type) },
                new XmlDatabaseProperty() { XmlName = nameof(Mode), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Mode) },
                new XmlDatabaseProperty() { XmlName = nameof(PatchPath), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(PatchPath) },
                new XmlDatabaseProperty() { XmlName = nameof(File), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(File) },
                new XmlDatabaseProperty() { XmlName = nameof(Version), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Version) },
                new XmlDatabaseProperty() { XmlName = nameof(Search), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Search) },
                new XmlDatabaseProperty() { XmlName = nameof(Replace), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Replace) },
                new XmlDatabaseProperty() { XmlName = nameof(FollowPath), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(FollowPath) },
                new XmlDatabaseProperty() { XmlName = nameof(Path), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Path) },
                new XmlDatabaseProperty() { XmlName = nameof(Line), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Line) }
            };
            return xmlDatabaseProperties;
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
                new XmlDatabaseProperty() { XmlName = nameof(Type), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Type) },
                new XmlDatabaseProperty() { XmlName = nameof(Mode), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Mode) },
                new XmlDatabaseProperty() { XmlName = nameof(PatchPath), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PatchPath) },
                new XmlDatabaseProperty() { XmlName = nameof(File), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(File) },
                new XmlDatabaseProperty() { XmlName = nameof(Version), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Version) },
                new XmlDatabaseProperty() { XmlName = nameof(Search), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Search) },
                new XmlDatabaseProperty() { XmlName = nameof(Replace), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Replace) },
                new XmlDatabaseProperty() { XmlName = nameof(FollowPath), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(FollowPath) },
                new XmlDatabaseProperty() { XmlName = nameof(Path), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Path) },
                new XmlDatabaseProperty() { XmlName = nameof(Line), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Line) }
            };
            return xmlDatabaseProperties;
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
        /// The type of patch, xml or regex (direct text replacement)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// If not regex, the mode that the xml patcher should use.<para/>Examples: add xml node, remove xml node, edit xml node
        /// </summary>
        public string Mode { get; set; } = string.Empty;

        /// <summary>
        /// The starting path to the file
        /// </summary>
        public string PatchPath { get; set; } = string.Empty;

        /// <summary>
        /// The path to the file, relative to patchPath
        /// </summary>
        public string File { get; set; } = string.Empty;

        /// <summary>
        /// The complete path to the file, saved at parse time
        /// </summary>
        public string CompletePath { get; set; } = string.Empty;

        /// <summary>
        /// The version of the patch for parsing. Allows for multiple variations. Default to 1
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// If xml or json, the xml xpath or json jsonpath to the node
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// If regex, the optional specific lines in the text file
        /// </summary>
        public string[] Lines
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Line))
                    return null;
                else
                    return Line.Split(',');
            }
        }

        /// <summary>
        /// If a regex patch, a comma seperated list of line numbers to attempt to apply the patch on.
        /// </summary>
        public string Line { get; set; } = string.Empty;

        /// <summary>
        /// The node inner text (xml) or regex criteria to search for
        /// </summary>
        public string Search { get; set; } = string.Empty;

        /// <summary>
        /// The text to replace the found search text with
        /// </summary>
        public string Replace { get; set; } = string.Empty;

        /// <summary>
        /// For json patches, if it should use the new method of separating the path for getting the xvm references
        /// </summary>
        public bool FollowPath { get; set; } = false;

        /// <summary>
        /// Gets a log formatted string for debugging containing key object name and values.
        /// </summary>
        /// <remarks>If debug output is enabled for the log file during an installation, then each instruction will have it's DumpInfoToLog property called.</remarks>
        public override string DumpInfoToLog
        {
            get
            {
                string linesString = Lines == null ? "null" : string.Join(",", Lines);
                string pathString = string.IsNullOrEmpty(Path) ? "null" : Path;
                return $"{base.DumpInfoToLog}{Environment.NewLine}{ApplicationConstants.LogSpacingLineup}{nameof(Type)}={Type} {nameof(Mode)}={Mode} {nameof(Version)}={Version}{nameof(PatchPath)}={PatchPath} {nameof(FollowPath)}={FollowPath}" +
                    $"{Environment.NewLine}{ApplicationConstants.LogSpacingLineup}{nameof(File)}={File} {nameof(Lines)}={linesString} {nameof(Path)}={pathString} {nameof(Search)}={Search} {nameof(Replace)}={Replace}";
            }
        }

        /// <summary>
        /// Gets a listbox style UI element display-ready text
        /// </summary>
        public string ListboxDisplay
        {
            get { return string.Format("type={0} ,mode={1}, lines/path={2}", Type, Mode, Lines == null ? Path : string.Join(",", Lines)); }
        }

        /// <summary>
        /// Gets a value that determines if all required properties are filed out to be saved to a patch file
        /// </summary>
        public bool IsValidForSave
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Type))
                    return false;

                if (string.IsNullOrWhiteSpace(Mode))
                    return false;

                if (string.IsNullOrWhiteSpace(PatchPath))
                    return false;

                if (string.IsNullOrWhiteSpace(File))
                    return false;

                if (string.IsNullOrWhiteSpace(Search) && string.IsNullOrWhiteSpace(Replace))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Returns a list of patch objects that are not ready to be written to a patch file
        /// </summary>
        /// <param name="patchList">The list of patches to check</param>
        /// <returns>A new list of patches that are not valid to save, or an empty list of all patches are valid</returns>
        /// <seealso cref="IsValidForSave"/>
        public static List<Patch> GetInvalidPatchesForSave(Patch[] patchList)
        {
            return GetInvalidPatchesForSave(patchList.ToList());
        }

        /// <summary>
        /// Returns a list of patch objects that are not ready to be written to a patch file
        /// </summary>
        /// <param name="patchList">The list of patches to check</param>
        /// <returns>A new list of patches that are not valid to save, or an empty list of all patches are valid</returns>
        /// <seealso cref="IsValidForSave"/>
        public static List<Patch> GetInvalidPatchesForSave(List<Patch> patchList)
        {
            //https://stackoverflow.com/questions/1938204/linq-where-vs-findall
            return patchList.FindAll(patch => !patch.IsValidForSave);
        }

        /// <summary>
        /// Returns a list of patch objects that are not ready to be written to a patch file
        /// </summary>
        /// <param name="patchList">The list of patches to check</param>
        /// <returns>A new list of patches that are not valid to save, or an empty list of all patches are valid</returns>
        /// <seealso cref="IsValidForSave"/>
        public static List<Patch> GetInvalidPatchesForSave(ItemCollection patchList)
        {
            //https://stackoverflow.com/questions/471595/casting-an-item-collection-from-a-listbox-to-a-generic-list
            return GetInvalidPatchesForSave(patchList.Cast<Patch>().ToList());
        }

        /// <summary>
        /// Compares two instructions to determine if their values are equal.
        /// </summary>
        /// <param name="instructionToCompare">The instruction to compare against.</param>
        /// <returns>True if the compared values are equal, false otherwise.</returns>
        public override bool InstructionsEqual(Instruction instructionToCompare)
        {
            Patch patchToCompare = instructionToCompare as Patch;
            if (patchToCompare == null)
                return false;

            if (!this.Type.Equals(patchToCompare.Type))
                return false;

            if (!this.Mode.Equals(patchToCompare.Mode))
                return false;

            if (!this.PatchPath.Equals(patchToCompare.PatchPath))
                return false;

            if (!this.File.Equals(patchToCompare.File))
                return false;

            if (!this.Version.Equals(patchToCompare.Version))
                return false;

            if (!this.Search.Equals(patchToCompare.Search))
                return false;

            if (!this.Replace.Equals(patchToCompare.Replace))
                return false;

            if (!this.FollowPath.Equals(patchToCompare.FollowPath))
                return false;

            if (!this.Path.Equals(patchToCompare.Path))
                return false;

            if (!this.Line.Equals(patchToCompare.Line))
                return false;

            return true;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>The patch's target file name, or "(empty)", if no target file name is given.</returns>
        public override string ToString()
        {
            return string.Format("File={0}", string.IsNullOrEmpty(File) ? "(empty)": File);
        }
    }
}
