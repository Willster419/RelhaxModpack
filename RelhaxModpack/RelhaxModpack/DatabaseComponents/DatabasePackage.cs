using RelhaxModpack.DatabaseComponents;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RelhaxModpack
{
    /// <summary>
    /// A database component is the base class for all other packages
    /// </summary>
    public class DatabasePackage : IXmlSerializable, IComponentWithID
    {
        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes
        /// </summary>
        /// <returns>A list of string property names</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional</remarks>
        public virtual string[] PropertiesForSerializationAttributes()
        {
            return PackagePropertiesToXmlParseAttributes.ToArray();
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements
        /// </summary>
        /// <returns>A list of string property names</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional</remarks>
        public virtual string[] PropertiesForSerializationElements()
        {
            return PackagePropertiesToXmlParseElements.ToArray();
        }

        private static readonly List<string> PackagePropertiesToXmlParseAttributes = new List<string>()
        {
            nameof(PackageName),
            nameof(Enabled),
            nameof(InstallGroup),
            nameof(PatchGroup)
        };

        private static readonly List<string> PackagePropertiesToXmlParseElements = new List<string>()
        {
            nameof(Size),
            nameof(Version),
            nameof(ZipFile),
            nameof(CRC),
            nameof(Timestamp),
            nameof(LogAtInstall),
            nameof(StartAddress),
            nameof(EndAddress),
            nameof(Triggers),
            nameof(DevURL),
            nameof(InternalNotes),
            nameof(Author)
        };

        /// <summary>
        /// Get the list of fields in the class that can be parsed as xml attributes
        /// </summary>
        /// <returns>The list of fields</returns>
        public static List<string> FieldsToXmlParseAttributes()
        {
            return new List<string>(PackagePropertiesToXmlParseAttributes);
        }

        /// <summary>
        /// Get the list of fields in the class that can be parsed as xml elements
        /// </summary>
        /// <returns>The list of fields</returns>
        public static List<string> FieldsToXmlParseNodes()
        {
            return new List<string>(PackagePropertiesToXmlParseElements);
        }
        #endregion

        #region Database Properties

        /// <summary>
        /// A unique identifier for each component in the database. No two components will have the same PackageName
        /// </summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>
        /// A method to keep track of the version of the package
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Used to determine when the package entry was last modified
        /// </summary>
        public long Timestamp { get; set; } = 0;

        /// <summary>
        /// Size of the zip file
        /// </summary>
        public ulong Size { get; set; } = 0;

        /// <summary>
        /// The zip file to extract (can be empty string)
        /// </summary>
        public string ZipFile { get; set; } = string.Empty;

        /// <summary>
        /// Internal field for Enabled property
        /// </summary>
        protected internal bool _Enabled = false;

        /// <summary>
        /// Determines if the component is enabled or disabled
        /// </summary>
        public virtual bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        /// <summary>
        /// The crc checksum of the zipfile
        /// </summary>
        public string CRC { get; set; } = string.Empty;

        /// <summary>
        /// The start address of the URL to the zip file
        /// URL format: StartAddress + ZipFile + EndAddress
        /// </summary>
        public string StartAddress { get; set; } = Settings.DefaultStartAddress;

        /// <summary>
        /// The end address of the URL to the zip file
        /// URL format: StartAddress + ZipFile + EndAddress
        /// </summary>
        public string EndAddress { get; set; } = Settings.DefaultEndAddress;

        /// <summary>
        /// Determine at install time if the package needs to be downloaded
        /// </summary>
        public bool DownloadFlag { get; set; } = false;

        /// <summary>
        /// Determine if the mod has been downloaded and is ready for installation
        /// </summary>
        public bool ReadyForInstall { get; set; } = false;

        /// <summary>
        /// Determine if the files from the package should be logged for un-installation
        /// only set this to false if absolutely necessary!
        /// </summary>
        public bool LogAtInstall { get; set; } = true;

        /// <summary>
        /// The list of triggers that this package can start (list of triggers that apply to this package)
        /// </summary>
        public List<string> Triggers { get; set; } = new List<string>();

        /// <summary>
        /// The URL link of where you can view the web page of the mod
        /// </summary>
        public string DevURL { get; set; } = string.Empty;

        public string[] DevURLList
        {
            get { return DevURL.Replace("\r", string.Empty).Split('\n'); }
        }

        /// <summary>
        /// The level at which this package can be installed. It will be installed with other packages of the same install group at the same time
        /// </summary>
        public int InstallGroup { get; set; } = 0;

        /// <summary>
        /// The level at which the patches for this package can be installed. Patches will be executed with other patches of the same patch group
        /// </summary>
        public int PatchGroup { get; set; } = 0;

        /// <summary>
        /// Internal instructions for updating the mod for database managers
        /// </summary>
        public string InternalNotes { get; set; } = string.Empty;

        public string InternalNotesEscaped
        {
            get { return Utils.MacroReplace(InternalNotes, ReplacementTypes.TextUnescape); }
        }

        /// <summary>
        /// The name of the author of the mod/configuration/etc.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// The number of bytes to download, used if "install while download" is true
        /// </summary>
        public long BytesToDownload { get; set; } = 0;

        /// <summary>
        /// The number of bytes currently downloaded, used if "install while download" is true
        /// </summary>
        public long BytesDownloaded { get; set; } = 0;

        /// <summary>
        /// Flag to determine if this package is the one currently downloading, used if "install while download" is true
        /// </summary>
        public bool IsCurrentlyDownloading { get; set; } = false;

        //append extraction flag
        /// <summary>
        /// Determines if this package should be put into a list that will be installed last. Used for when the package is possibly overwriting files, for example
        /// </summary>
        [Obsolete("This is for legacy database compatibility and will be ignored in Relhax V2")]
        public bool AppendExtraction = false;
        #endregion

        #region UI Properties
        /// <summary>
        /// Reference for the UI element of this package in the database editor
        /// </summary>
        public TreeViewItem EditorTreeViewItem { get; set; } = null;
        #endregion

        #region Other Properties and Methods
        /// <summary>
        /// Flag used for the "download while install" setting. Default is false until it is set true. Once set, the installer will not try to extract this package again
        /// </summary>
        public bool ExtractionStarted { get; set; } = false;

        public string ComponentInternalName { get { return PackageName; } }

        public DownloadInstructions DownloadInstructions { get; set; } = null;

        public UpdateInstructions UpdateInstructions { get; set; }= null;

        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>The PackageName of the package</returns>
        public override string ToString()
        {
            return PackageName;
        }

        /// <summary>
        /// Provides a complete tree style path to the package using its UI name, starting with the category
        /// </summary>
        public virtual string CompletePath
        { get {  return PackageName; } }

        /// <summary>
        /// Provides a complete tree style path to the package using its internal packageName, starting with the category
        /// </summary>
        public virtual string CompletePackageNamePath
        { get { return PackageName; } }

        /// <summary>
        /// Creates an instance of the DatabasePackage class
        /// </summary>
        public DatabasePackage()
        {

        }

        /// <summary>
        /// Creates an instance of the DatabasePackage class based on the provided DatabasePackage
        /// </summary>
        /// <param name="packageToCopy">The package to copy the information from</param>
        /// <param name="deep">Set to true to copy list objects, false to use new lists</param>
        public DatabasePackage(DatabasePackage packageToCopy, bool deep)
        {
            this.PackageName = packageToCopy.PackageName;
            this.Version = packageToCopy.Version;
            this.Timestamp = packageToCopy.Timestamp;
            this.ZipFile = packageToCopy.ZipFile;
            this.CRC = packageToCopy.CRC;
            this.StartAddress = packageToCopy.StartAddress;
            this.EndAddress = packageToCopy.EndAddress;
            this.LogAtInstall = packageToCopy.LogAtInstall;
            this.Triggers = new List<string>();
            this.DevURL = packageToCopy.DevURL;
            this.InstallGroup = packageToCopy.InstallGroup;
            this.PatchGroup = packageToCopy.PatchGroup;
            //don't call the property for enabled, just the internal field
            this._Enabled = packageToCopy._Enabled;

            if (deep)
            {
                foreach (string s in packageToCopy.Triggers)
                    this.Triggers.Add(s);
            }
        }
        #endregion
    }
}
