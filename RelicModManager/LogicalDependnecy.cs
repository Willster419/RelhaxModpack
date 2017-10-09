using System;
using System.Collections.Generic;

namespace RelhaxModpack
{
    public class LogicalDependnecy
    {
        //the zip file of the dependency
        public string dependencyZipFile { get; set; }
        //the timestamp of last change of zipfile name 
        public Int64 timestamp { get; set; }
        //the crc of the dependency
        public string dependencyZipCRC { get; set; }
        //flag to set to disable the dependency from being installed
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
        //acts as a NOT flag
        public bool negateFlag { get; set; }
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        //list of linked mods and configs that use 
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();
        public string devURL { get; set; }
        public LogicalDependnecy() { }
        //for the tostring thing
        public override string ToString()
        {
            return negateFlag? "(Not)" + packageName:"" + packageName;
        }
    }
}
