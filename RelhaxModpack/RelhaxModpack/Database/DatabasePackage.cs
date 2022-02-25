using RelhaxModpack.Atlases;
using RelhaxModpack.Common;
using RelhaxModpack.Installer;
using RelhaxModpack.Patching;
using RelhaxModpack.Shortcuts;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// A database component is the base class (that can be instanced) for all other packages. It represents an object in the database..
    /// </summary>
    public class DatabasePackage : CoreDatabaseComponent, IDatabaseComponent, IXmlSerializable
    {
        /// <summary>
        /// Creates an instance of the DatabasePackage class
        /// </summary>
        public DatabasePackage() : base()
        {
            InitComponent();
        }

        /// <summary>
        /// Creates an instance of the DatabasePackage class based on the provided DatabasePackage.
        /// </summary>
        /// <param name="packageToCopy">The package to copy the information from.</param>
        public DatabasePackage(DatabasePackage packageToCopy) : base(packageToCopy)
        {
            InitComponent();
            this.PackageName = packageToCopy.PackageName;
            this.Version = packageToCopy.Version;
            this.Timestamp = packageToCopy.Timestamp;
            this.ZipFile = packageToCopy.ZipFile;
            this.CRC = packageToCopy.CRC;
            this.LogAtInstall = packageToCopy.LogAtInstall;
            this.Triggers = packageToCopy.Triggers;
            this.DevURL = packageToCopy.DevURL;
            this.InstallGroup = packageToCopy.InstallGroup;
            this.PatchGroup = packageToCopy.PatchGroup;
            this.UID = packageToCopy.UID;
            //don't call the property for enabled, just the internal field
            this._Enabled = packageToCopy._Enabled;
            this.LastUpdateCheck = packageToCopy.LastUpdateCheck;
        }

        /// <summary>
        /// Called from the constructor, handles any object initialization that should be done.
        /// </summary>
        protected virtual void InitComponent()
        {
            //stub
        }

        #region Xml serialization V1
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public virtual string[] PropertiesForSerializationAttributes()
        {
            return PackagePropertiesToXmlParseAttributes.ToArray();
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public virtual string[] PropertiesForSerializationElements()
        {
            return PackagePropertiesToXmlParseElements.ToArray();
        }

        private static readonly List<string> PackagePropertiesToXmlParseAttributes = new List<string>()
        {
            nameof(PackageName),
            nameof(Enabled),
            nameof(Tags),
            nameof(InstallGroup),
            nameof(PatchGroup),
            nameof(UID)
        };

        private static readonly List<string> PackagePropertiesToXmlParseElements = new List<string>()
        {
            nameof(Size),
            nameof(Version),
            nameof(ZipFile),
            nameof(CRC),
            nameof(Timestamp),
            nameof(LogAtInstall),
            nameof(Triggers),
            nameof(DevURL),
            nameof(InternalNotes),
            nameof(Author),
            nameof(Maintainers),
            nameof(Deprecated),
            nameof(MinimalistModeExclude),
            nameof(LastUpdateCheck)
        };
        #endregion

        #region Xml serialization V2
        /// <summary>
        /// For some xml schema versions, the actual name of the xml entry used for this property.
        /// </summary>
        public const string XmlElementName = "Package";

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.0 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = base.GetXmlDatabasePropertiesV1Dot0();
            List<XmlDatabaseProperty> xmlDatabasePropertiesAddBefore = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(PackageName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PackageName) },
                new XmlDatabaseProperty() { XmlName = nameof(Enabled), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Enabled) },
                new XmlDatabaseProperty() { XmlName = nameof(Tags), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Tags) },
                new XmlDatabaseProperty() { XmlName = nameof(InstallGroup), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(InstallGroup) },
                new XmlDatabaseProperty() { XmlName = nameof(PatchGroup), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PatchGroup) },
                new XmlDatabaseProperty() { XmlName = nameof(UID), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(UID) },
                //list elements
                new XmlDatabaseProperty() { XmlName = nameof(Size), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Size) },
                new XmlDatabaseProperty() { XmlName = nameof(Version), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Version) },
                new XmlDatabaseProperty() { XmlName = nameof(ZipFile), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(ZipFile) },
                new XmlDatabaseProperty() { XmlName = nameof(CRC), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(CRC) },
                new XmlDatabaseProperty() { XmlName = nameof(Timestamp), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Timestamp) },
                new XmlDatabaseProperty() { XmlName = nameof(LogAtInstall), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(LogAtInstall) },
                new XmlDatabaseProperty() { XmlName = nameof(Triggers), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Triggers) },
                new XmlDatabaseProperty() { XmlName = nameof(DevURL), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(DevURL) },
                new XmlDatabaseProperty() { XmlName = nameof(InternalNotes), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(InternalNotes) },
                new XmlDatabaseProperty() { XmlName = nameof(Author), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Author) },
            };
            List<XmlDatabaseProperty> xmlDatabasePropertiesAddAfter = new List<XmlDatabaseProperty>()
            {
                new XmlDatabaseProperty() { XmlName = nameof(Deprecated), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Deprecated) },
                new XmlDatabaseProperty() { XmlName = nameof(MinimalistModeExclude), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(MinimalistModeExclude) },
                new XmlDatabaseProperty() { XmlName = nameof(LastUpdateCheck), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(LastUpdateCheck) },
                new XmlDatabaseProperty() { XmlName = nameof(Patches), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Patches) },
                new XmlDatabaseProperty() { XmlName = nameof(XmlUnpacks), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(XmlUnpacks) },
                new XmlDatabaseProperty() { XmlName = nameof(Atlases), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Atlases) },
                new XmlDatabaseProperty() { XmlName = nameof(Shortcuts), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Shortcuts) }
            };
            //add stuff before base
            xmlDatabaseProperties.InsertRange(0, xmlDatabasePropertiesAddBefore);
            //add stuff after base
            xmlDatabaseProperties.AddRange(xmlDatabasePropertiesAddAfter);
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
            return this.GetXmlDatabasePropertiesV1Dot0();
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

        /// <summary>
        /// Gets the xml element name that should be used for saving and loading this component, based on the schema to load/save.
        /// </summary>
        /// <param name="schemaVersion">The schema version to load/save.</param>
        /// <returns>The name of the xml element to use for saving/loading this component.</returns>
        public override string GetXmlElementName(string schemaVersion)
        {
            switch (schemaVersion)
            {
                case SchemaV1Dot0:
                case SchemaV1Dot1:
                case SchemaV1Dot2:
                    return XmlElementName;
                default:
                    return base.GetXmlElementName(schemaVersion);
            }
        }

        /// <summary>
        /// A hook from XmlComponent for when an xml entry is finished being loaded into an object.
        /// </summary>
        /// <param name="propertyElement">The xml element of the entry being loaded. For example, the "DatabasePackage" xml element.</param>
        /// <param name="loadStatus">The status of the loading of this object, if all properties of it were previously loaded correctly.</param>
        protected override void OnFinishedLoadingFromXml(XElement propertyElement, bool loadStatus)
        {
            base.OnFinishedLoadingFromXml(propertyElement, loadStatus);

            ProcessInstructions();
        }
        #endregion

        #region Selection file processing
        private static readonly List<string> PackagePropertiesToSaveForSelectionFile = new List<string>()
        {
            nameof(PackageName),
            nameof(UID),
            nameof(ZipFile),
            nameof(Timestamp),
            nameof(CRC),
            nameof(Version),
            nameof(Enabled)
        };

        /// <summary>
        /// Gets a list of property names that are used for saving/loading the selection V3 file format.
        /// </summary>
        /// <returns>The string array of property attributes to save for a v3 selection file.</returns>
        public virtual string[] AttributesToXmlParseSelectionFiles()
        { return PackagePropertiesToSaveForSelectionFile.ToArray(); }
        #endregion

        #region Database Properties
        /// <summary>
        /// A unique identifier for each component in the database. No two components will have the same PackageName.
        /// </summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>
        /// Another non-changing, randomly generated unique 16 character identifier for each component in the database.
        /// </summary>
        /// <remarks>These fulfill the originally intended purpose of the PackageName defined back in OMC.
        /// Once generated, the value should not be modified, and will only be removed when the package is removed.</remarks>
        public string UID { get; set; } = string.Empty;

        /// <summary>
        /// A method to keep track of the version of the package.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Used to determine when the package entry was last modified.
        /// </summary>
        public long Timestamp { get; set; } = 0;

        /// <summary>
        /// Used to determine when the last automation or human check for an update was completed.
        /// </summary>
        public long LastUpdateCheck { get; set; } = 0;

        /// <summary>
        /// Size of the zip file.
        /// </summary>
        public ulong Size { get; set; } = 0;

        /// <summary>
        /// The zip file to extract (can be empty string).
        /// </summary>
        public string ZipFile { get; set; } = string.Empty;

        /// <summary>
        /// Internal field for Enabled property.
        /// </summary>
        protected internal bool _Enabled = false;

        /// <summary>
        /// Determines if the component is enabled or disabled.
        /// </summary>
        public virtual bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        /// <summary>
        /// The crc checksum of the zipfile.
        /// </summary>
        public string CRC { get; set; } = string.Empty;

        /// <summary>
        /// Determine at install time if the package needs to be downloaded.
        /// </summary>
        public bool DownloadFlag { get; set; } = false;

        /// <summary>
        /// Determine if the mod has been downloaded and is ready for installation.
        /// </summary>
        public bool ReadyForInstall { get; set; } = false;

        /// <summary>
        /// Determine if the files from the package should be logged for un-installation.
        /// </summary>
        /// <remarks>Only set this to false if absolutely necessary!</remarks>
        public bool LogAtInstall { get; set; } = true;

        /// <summary>
        /// The list of triggers that this package can start (list of triggers that apply to this package).
        /// </summary>
        public string Triggers { get; set; } = string.Empty;

        /// <summary>
        /// Returns a list of triggers that this component can start.
        /// </summary>
        public List<string> TriggersList
        {
            get { return Triggers.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        /// <summary>
        /// The URL link of where you can view the web page of the package.
        /// </summary>
        public string DevURL { get; set; } = string.Empty;

        /// <summary>
        /// Gets an array of dev URLs, separated by the newline character.
        /// </summary>
        public string[] DevURLList
        {
            get { return MacroUtils.MacroReplace(DevURL, ReplacementTypes.TextUnescape).Replace("\r", string.Empty).Split('\n'); }
        }

        /// <summary>
        /// The level at which this package can be installed. It will be installed with other packages of the same install group at the same time.
        /// </summary>
        public int InstallGroup { get; set; } = 0;

        /// <summary>
        /// The level at which this package will be installed, factoring if the category (if SelectablePackage) is set to offset the install group with the package level.
        /// </summary>
        public virtual int InstallGroupWithOffset
        {
            get { return InstallGroup; }
        }

        /// <summary>
        /// The level at which the patches for this package can be installed. Patches will be executed with other patches of the same patch group.
        /// </summary>
        public int PatchGroup { get; set; } = 0;

        /// <summary>
        /// Internal instructions for updating the mod for database managers.
        /// </summary>
        public string InternalNotes { get; set; } = string.Empty;

        /// <summary>
        /// An escaped version of the internal notes. Replaces '\n' literal with '\n' special.
        /// </summary>
        public string InternalNotesEscaped
        {
            get { return MacroUtils.MacroReplace(InternalNotes, ReplacementTypes.TextUnescape); }
        }

        /// <summary>
        /// The name of the author of the mod/configuration/etc.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// The list of tags that this package contains (like patches, scripts, etc).
        /// </summary>
        public PackageTagsList Tags { get; set; }  = new PackageTagsList();

        /// <summary>
        /// A flag to set for a package that is considered to be outdated or no longer supported or stale.
        /// </summary>
        public bool Deprecated { get; set; } = false;

        /// <summary>
        /// A flag for determining if this package should be excluded from install when minimalist mode is enabled in ModpackSettings.
        /// </summary>
        /// <seealso cref="Settings.ModpackSettings.MinimalistMode"/>
        public bool MinimalistModeExclude { get; set; } = false;

        /// <summary>
        /// Get or set a list of patch instructions to perform after extraction of this package.
        /// </summary>
        public List<Patch> Patches { get; set; } = new List<Patch>();

        /// <summary>
        /// Get or set a list of xml unpack instructions to perform after extraction of this package.
        /// </summary>
        public List<XmlUnpack> XmlUnpacks { get; set; } = new List<XmlUnpack>();

        /// <summary>
        /// Get or set a list of atlas creation instructions to perform after extraction of this package.
        /// </summary>
        public List<Atlas> Atlases { get; set; } = new List<Atlas>();

        /// <summary>
        /// Get or set a list of shortcut creation instructions to perform after extraction of this package.
        /// </summary>
        public List<Shortcut> Shortcuts { get; set; } = new List<Shortcut>();
        #endregion

        #region Other Properties and Methods
        /// <summary>
        /// When a databasePackage, the internal packageName. When category, the category name.
        /// </summary>
        public override string ComponentInternalName { get { return PackageName; } }

        /// <summary>
        /// String representation of the object.
        /// </summary>
        /// <returns>The PackageName of the package.</returns>
        public override string ToString()
        {
            return PackageName;
        }

        /// <summary>
        /// Provides a complete tree style path to the package using its UI name, starting with the category.
        /// </summary>
        public virtual string CompletePath
        { get {  return PackageName; } }

        /// <summary>
        /// Provides a complete tree style path to the package using its internal packageName, starting with the category.
        /// </summary>
        public virtual string CompletePackageNamePath
        { get { return PackageName; } }

        /// <summary>
        /// Provides a complete tree style path to the package using its UID, starting with the category.
        /// </summary>
        public virtual string CompleteUIDPath
        { get { return UID; } }

        /// <summary>
        /// Updates the zip file entry of this package, as well as setting time stamp and md5 values.
        /// </summary>
        /// <param name="newZipfile">The new name of the zip file.</param>
        /// <remarks>When a zip file is updated, it means a file on the FTP server has been created or updated. When that happens, the md5 hash entry and other time stamp information about the package should be updated.</remarks>
        public void UpdateZipfile(string newZipfile)
        {
            this.ZipFile = newZipfile;
            this.CRC = string.IsNullOrEmpty(ZipFile)? string.Empty : "f";
            this.Timestamp = string.IsNullOrEmpty(ZipFile) ? 0 : CommonUtils.GetCurrentUniversalFiletimeTimestamp();
            this.LastUpdateCheck = this.Timestamp;
        }

        /// <summary>
        /// Returns an xml element representation of what this package's entry should be in the automation root xml file.
        /// </summary>
        /// <returns>The xml element representation.</returns>
        public string ToAutomationElement()
        {
            //example: <AutomationSequence UID="cw7xk1guapz8hayk" packageName="Dependency_OldSkool_modsCore" path="Dependencies/Dependency_OldSkool_modsCore.xml" />
            return string.Format("<AutomationSequence UID=\"{0}\" packageName=\"{1}\" path=\"path_to/{1}.xml\"/>", UID, PackageName);
        }

        /// <summary>
        /// Returns the list of instructions associated with this package based on the requested instruction enumeration.
        /// </summary>
        /// <param name="instructionsType">The type of instructions to return.</param>
        /// <returns>The list of instructions, or an empty list of none of that type exist.</returns>
        public List<Instruction> GetInstructions(InstructionsType instructionsType)
        {
            switch (instructionsType)
            {
                case InstructionsType.Atlas:
                    return this.Atlases.Cast<Instruction>().ToList();
                case InstructionsType.Patch:
                    return this.Patches.Cast<Instruction>().ToList();
                case InstructionsType.Shortcut:
                    return this.Shortcuts.Cast<Instruction>().ToList();
                case InstructionsType.UnpackCopy:
                    return this.XmlUnpacks.Cast<Instruction>().ToList();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a list of all instructions associated with this package.
        /// </summary>
        /// <returns>The list of instructions, or an empty list if no instructions in this package exist.</returns>
        public List<Instruction> GetInstructions()
        {
            List<Instruction> instructions = new List<Instruction>();
            instructions.AddRange(Atlases.Cast<Instruction>().ToList());
            instructions.AddRange(Patches.Cast<Instruction>().ToList());
            instructions.AddRange(Shortcuts.Cast<Instruction>().ToList());
            instructions.AddRange(XmlUnpacks.Cast<Instruction>().ToList());
            return instructions;
        }

        /// <summary>
        /// Add this package reference to each instruction entry.
        /// </summary>
        public void ProcessInstructions()
        {
            //process instruction package links
            foreach (Instruction instruction in this.GetInstructions())
                instruction.Package = this;
        }
        #endregion
    }
}
