using RelhaxModpack.DatabaseComponents;
using System;
using System.Collections.Generic;

namespace RelhaxModpack
{
    //a dependency is a zip file like mod that is required for any of the mods to work
    //i.e. and sound mods require the sound memory to be increased
    public class Dependency : IDatabasePackage
    {
        //the zip file of the dependency
        public string zipFile { get; set; }
        //the timestamp of last change of zipfile name 
        public long timestamp { get; set; }
        //the crc of the dependency
        public string crc { get; set; }
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
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        //property to determine if it will be installed in the beginning or in the end
        public bool appendExtraction;
        public List<LogicalDependnecy> logicalDependencies = new List<LogicalDependnecy>();
        public string devURL { get; set; }
        public List<ShortCut> shortCuts = new List<ShortCut>();
        public Dependency() { }
        //for the tostring thing
        public override string ToString()
        {
            return packageName;
        }
    }
}
