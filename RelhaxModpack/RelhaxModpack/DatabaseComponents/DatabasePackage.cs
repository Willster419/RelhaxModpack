using System.Collections.Generic;

namespace RelhaxModpack.DatabaseComponents
{
    /// <summary>
    /// A database component is an abstract class for all Components within the database
    /// </summary>
    public abstract class DatabasePackage
    {
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
        /// determine if the package needs to be installed at the end
        /// </summary>
        public bool AppendExtraction = false;
        /// <summary>
        /// determine if the files from the package should be logged for uninstallation
        /// only set this to false if absolutly necessary!
        /// </summary>
        public bool LogAtInstall = true;
        /// <summary>
        /// the URL link of where you can view the webpage of the mod
        /// </summary>
        public string DevURL = "";
        /// <summary>
        /// list containing all shortcuts that could be created based on package data
        /// </summary>
        public List<Shortcut> Shortcuts = new List<Shortcut>();
        /// <summary>
        /// used to inentify [if a PackageName confilct occurs] where in the xml file the conflict is
        /// </summary>
        public int CheckDatabaseListIndex = 0;
        public abstract override string ToString();
        /// <summary>
        /// Provides (if possible) a complete tree style path in for the cateogry views
        /// </summary>
        public virtual string CompletePath
        {
            get
            {
                return PackageName;
            }
        }
    }
}
