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

        public const string PatchXmlSearchPath = "/patchs/patch";

        public Patch() : base()
        {

        }

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

        public override string RootObjectPath { get { return PatchXmlSearchPath; } }

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

        #region Xml serialization V2
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
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
        /// Collects all patch information for logging
        /// </summary>
        public string DumpPatchInfoForLog
        {
            get
            {
                return string.Format("{0} patch, Version={1}, NativeProcessingFile={2}, ActualFile={3}," +
                    "{4}{5}PatchPath={6}, FileToPatch={7}," +
                    "{8}{9}Lines={10}, Path={11}, Search={12}, Replace={13}",
                    Type.ToLower(),
                    Version,
                    NativeProcessingFile,
                    ActualPatchName,
                    Environment.NewLine,
                    ApplicationConstants.LogSpacingLineup,
                    PatchPath,
                    File,
                    Environment.NewLine,
                    ApplicationConstants.LogSpacingLineup,
                    Lines == null ? "null" : string.Join(",", Lines),
                    string.IsNullOrEmpty(Path) ? "null" :Path,
                    Search,
                    Replace);
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

        public static Patch Copy(Patch shortcutToCopy)
        {
            return new Patch(shortcutToCopy);
        }
    }
}
