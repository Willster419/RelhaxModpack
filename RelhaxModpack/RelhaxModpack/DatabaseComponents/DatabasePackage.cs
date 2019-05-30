using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RelhaxModpack
{
    /// <summary>
    /// A database component is an abstract class for all Components within the database
    /// </summary>
    public class DatabasePackage
    {
        #region Xml parsing

        private static readonly List<string> PackageElementsToXmlParseAttributes = new List<string>()
        {
            nameof(PackageName),
            nameof(Enabled),
            nameof(InstallGroup),
            nameof(PatchGroup)
        };

        private static readonly List<string> PackageElementsToXmlParseNodes = new List<string>()
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
            nameof(InternalNotes)
        };

        public static List<string> FieldsToXmlParseAttributes()
        {
            return new List<string>(PackageElementsToXmlParseAttributes);
        }

        public static List<string> FieldsToXmlParseNodes()
        {
            return new List<string>(PackageElementsToXmlParseNodes);
        }
        #endregion

        #region Database Properties

        /// <summary>
        /// a unique identifier for each component in the database. No two components will have the same PackageName
        /// </summary>
        public string PackageName = "";

        /// <summary>
        /// a method to keep track of the version of the package
        /// </summary>
        public string Version = "";

        /// <summary>
        /// used to determine when the package entry was last modified
        /// </summary>
        public long Timestamp = 0;

        /// <summary>
        /// size of the zip file
        /// </summary>
        public ulong Size = 0;

        /// <summary>
        /// the zip file to extract (can be "")
        /// </summary>
        public string ZipFile = "";

        protected internal bool _Enabled = false;

        /// <summary>
        /// a toggle to enable and disable the component
        /// </summary>
        public virtual bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        /// <summary>
        /// the crc checksum of the zipfile
        /// </summary>
        public string CRC = "";

        /// <summary>
        /// the start address of the url to the zip file
        /// URL format: StartAddress + ZipFile + EndAddress
        /// </summary>
        public string StartAddress = Settings.DefaultStartAddress;

        /// <summary>
        /// the end address of the url to the zip file
        /// URL format: StartAddress + ZipFile + EndAddress
        /// </summary>
        public string EndAddress = Settings.DefaultEndAddress;

        /// <summary>
        /// determine at install time if the package needs to be downloaded
        /// </summary>
        public bool DownloadFlag = false;

        /// <summary>
        /// determine if the mod has been downloaded and is ready for installation
        /// </summary>
        public bool ReadyForInstall = false;

        /// <summary>
        /// determine if the files from the package should be logged for uninstallation
        /// only set this to false if absolutly necessary!
        /// </summary>
        public bool LogAtInstall = true;

        public List<string> Triggers = new List<string>();
        /// <summary>
        /// the URL link of where you can view the webpage of the mod
        /// </summary>
        public string DevURL = "";

        public int InstallGroup = 0;

        public int PatchGroup = 0;

        public string InternalNotes = "";

        //append extraction flag
        [Obsolete("This is for legacy database compatibility and will be ignored in Relhax V2")]
        public bool AppendExtraction = false;
        #endregion

        #region UI Properties
        //used for the editor ONLY
        public TreeViewItem EditorTreeViewItem = null;
        #endregion

        #region Other Properties and Methods
        public override string ToString()
        {
            return PackageName;
        }
        /// <summary>
        /// Provides (if possible) a complete tree style path in for the cateogry views
        /// </summary>
        public virtual string CompletePath
        { get {  return PackageName; } }

        public virtual string CompletePackageNamePath
        { get { return PackageName; } }
        #endregion
    }
}
