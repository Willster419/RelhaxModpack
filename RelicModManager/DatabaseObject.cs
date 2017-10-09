using System;
using System.Collections.Generic;

namespace RelhaxModpack
{
    public abstract class DatabaseObject
    {
        public string name { get; set; }
        //the developer's version of the mod
        public string version { get; set; }
        public string zipFile { get; set; }
        public Int64 timestamp { get; set; }
        public List<Config> configs = new List<Config>();
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for ly shared files.
        public string startAddress { get; set; }
        public List<LogicalDependnecy> logicalDependencies = new List<LogicalDependnecy>();
        public List<Dependency> dependencies = new List<Dependency>();
        public List<Media> pictureList = new List<Media>();
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string endAddress { get; set; }
        public string crc { get; set; }
        public bool enabled { get; set; }
        //a flag to determine wether or not the mod should be shown
        public bool visible { get; set; }
        //later a unique name of the config entry
        public string packageName { get; set; }
        //size of the mod zip file
        public Int64 size { get; set; }
        public string updateComment { get; set; }
        public string description { get; set; }
        public string devURL { get; set; }
        public List<string> userFiles = new List<string>();
        public bool Checked { get; set; }
        public bool downloadFlag { get; set; }
    }
}
