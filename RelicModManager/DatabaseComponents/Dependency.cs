using RelhaxModpack.DatabaseComponents;
using System.Collections.Generic;

namespace RelhaxModpack
{
    //a dependency is a zip file like mod that is required for any of the mods to work
    //i.e. and sound mods require the sound memory to be increased
    public class Dependency : IDatabasePackage
    {
        //the zip file of the dependency
        public string ZipFile { get; set; }
        //the Timestamp of last change of zipfile name 
        public long Timestamp { get; set; }
        //the CRC of the dependency
        public string CRC { get; set; }
        //flag to set to disable the dependency from being installed
        public bool Enabled { get; set; }
        //the start address of the zip file location. Enabled us to use sites that
        //generate random filenames for publicly shared files.
        public string StartAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string EndAddress { get; set; }
        //later a unique name of the config entry
        public string PackageName { get; set; }
        //used to determine at install time if the zip file needs to be downloaded
        public bool DownloadFlag { get; set; }
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        //property to determine if it will be installed in the beginning or in the end
        public bool AppendExtraction;
        public bool ReadyForInstall { get; set; }
        public List<LogicalDependency> LogicalDependencies = new List<LogicalDependency>();
        public string DevURL { get; set; }
        public List<Shortcut> shortCuts = new List<Shortcut>();
        public Dependency() {
            ReadyForInstall = false;
        }
        //for the tostring thing
        public override string ToString()
        {
            return PackageName;
        }
    }
}
