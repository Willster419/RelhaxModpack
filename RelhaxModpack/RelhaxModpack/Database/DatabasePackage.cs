using RelhaxModpack.Common;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// A database component is the base class for all other packages
    /// </summary>
    public class DatabasePackage : IDatabaseComponent, IXmlSerializable, IDisposable
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
            nameof(MinimalistModeExclude)//,
            //nameof(LastUpdateCheck)
        };
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
        /// Gets a list of property names that are used for saving/loading the selection V3 file format
        /// </summary>
        /// <returns></returns>
        public virtual string[] AttributesToXmlParseSelectionFiles()
        { return PackagePropertiesToSaveForSelectionFile.ToArray(); }
        #endregion

        #region Database Properties
        /// <summary>
        /// A unique identifier for each component in the database. No two components will have the same PackageName
        /// </summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>
        /// Another non-changing, randomly generated unique 16 character identifier for each component in the database.
        /// </summary>
        /// <remarks>These fulfill the originally intended purpose of the PackageName defined back in OMC.
        /// Once generated, the value should not be modified, and will only be removed when the package is removed.</remarks>
        public string UID { get; set; } = string.Empty;

        /// <summary>
        /// A method to keep track of the version of the package
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// A list of database managers who are known to maintain this component
        /// </summary>
        public string Maintainers { get; set; } = string.Empty;

        /// <summary>
        /// Returns a list database managers who are known to maintain this component
        /// </summary>
        public List<string> MaintainersList
        {
            get { return Maintainers.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        /// <summary>
        /// Used to determine when the package entry was last modified
        /// </summary>
        public long Timestamp { get; set; } = 0;

        /// <summary>
        /// Used to determine when the last automation or human check for an update was completed
        /// </summary>
        public long LastUpdateCheck { get; set; } = 0;

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
        public string Triggers { get; set; } = string.Empty;

        /// <summary>
        /// Returns a list of triggers that this component can start
        /// </summary>
        public List<string> TriggersList
        {
            get { return Triggers.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        /// <summary>
        /// The URL link of where you can view the web page of the mod
        /// </summary>
        public string DevURL { get; set; } = string.Empty;

        /// <summary>
        /// Gets an array of dev URLs, separated by the newline character
        /// </summary>
        public string[] DevURLList
        {
            get { return MacroUtils.MacroReplace(DevURL, ReplacementTypes.TextUnescape).Replace("\r", string.Empty).Split('\n'); }
        }

        /// <summary>
        /// The level at which this package can be installed. It will be installed with other packages of the same install group at the same time
        /// </summary>
        public int InstallGroup { get; set; } = 0;

        /// <summary>
        /// The level at which this package will be installed, factoring if the category (if SelectablePackage) is set to offset the install group with the package level
        /// </summary>
        public virtual int InstallGroupWithOffset
        {
            get { return InstallGroup; }
        }

        /// <summary>
        /// The level at which the patches for this package can be installed. Patches will be executed with other patches of the same patch group
        /// </summary>
        public int PatchGroup { get; set; } = 0;

        /// <summary>
        /// Internal instructions for updating the mod for database managers
        /// </summary>
        public string InternalNotes { get; set; } = string.Empty;

        /// <summary>
        /// An escaped version of the internal notes. Replaces '\n' literal with '\n' special
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
        /// The list of tags that this package contains (like patches, scripts, etc)
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
        #endregion

        #region UI Properties
        /// <summary>
        /// Reference for the UI element of this package in the database editor
        /// </summary>
        public TreeViewItem EditorTreeViewItem { get; set; } = null;
        #endregion

        #region Other Properties and Methods
        /// <summary>
        /// When a databasePackage, the internal packageName. When category, the category name
        /// </summary>
        public string ComponentInternalName { get { return PackageName; } }

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
        /// Provides a complete tree style path to the package using its UID, starting with the category
        /// </summary>
        public virtual string CompleteUIDPath
        { get { return UID; } }

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
            this.LogAtInstall = packageToCopy.LogAtInstall;
            this.Triggers = packageToCopy.Triggers;
            this.DevURL = packageToCopy.DevURL;
            this.InstallGroup = packageToCopy.InstallGroup;
            this.PatchGroup = packageToCopy.PatchGroup;
            this.Maintainers = packageToCopy.Maintainers;
            this.UID = packageToCopy.UID;
            //don't call the property for enabled, just the internal field
            this._Enabled = packageToCopy._Enabled;
            this.LastUpdateCheck = packageToCopy.LastUpdateCheck;
        }

        public void UpdatePackageName(string newPackageName)
        {
            this.ZipFile = newPackageName;
            this.CRC = "f";
            this.Timestamp = CommonUtils.GetCurrentUniversalFiletimeTimestamp();
            this.LastUpdateCheck = this.Timestamp;
        }

        public string ToAutomationElement()
        {
            //example: <AutomationSequence UID="cw7xk1guapz8hayk" packageName="Dependency_OldSkool_modsCore" path="Dependencies/Dependency_OldSkool_modsCore.xml" />
            return string.Format("<AutomationSequence UID=\"{0}\" packageName=\"{1}\" path=\"path_to/{1}.xml\"/>", UID, PackageName);
        }
        #endregion

        #region Disposable support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Tags.Clear();
                    Tags = null;
                    if (EditorTreeViewItem != null)
                        EditorTreeViewItem = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DatabasePackage()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
