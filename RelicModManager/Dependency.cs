using System.Collections.Generic;

namespace RelhaxModpack
{
    //a dependency is a zip file like mod that is required for any of the mods to work
    //i.e. and sound mods require the sound memory to be increased
    public class Dependency
    {
        public string dependencyZipFile { get; set; }
        public string dependencyZipCRC { get; set; }
        public bool enabled { get; set; }
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for publicly shared files.
        public string startAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string endAddress { get; set; }
        //later a unique name of the config entry
        public string packageName { get; set; }
        //used to determine at install time if the zip file needs to be downloaded
        public bool downloadFlag { get; set; }
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        //property to determine if it will be installed in the beginning or in the end
        public bool appendExtraction;
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();
        public Dependency() { }
    }
}
