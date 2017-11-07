using RelhaxModpack.DatabaseComponents;
using System;
using System.Collections.Generic;

namespace RelhaxModpack
{
    public abstract class SelectableDatabasePackage : IDatabasePackage
    {
        public string name { get; set; }
        public string nameFormated
        {
            get { return Utils.ReplaceMacro(name,"version",version); }
        }
        //the developer's version of the mod
        public string version { get; set; }
        public string ZipFile { get; set; }
        public long Timestamp { get; set; }
        public List<Config> configs = new List<Config>();
        //the start address of the zip file location. Enabled us to use sites that
        //generate random filenames for ly shared files.
        public string StartAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string EndAddress { get; set; }
        public List<LogicalDependency> logicalDependencies = new List<LogicalDependency>();
        public List<Dependency> dependencies = new List<Dependency>();
        public List<Media> pictureList = new List<Media>();
        public List<ShortCut> shortCuts = new List<ShortCut>();
        public string CRC { get; set; }
        public bool Enabled { get; set; }
        //a flag to determine wether or not the mod should be shown
        public bool visible { get; set; }
        //later a unique name of the config entry
        public string PackageName { get; set; }
        //size of the mod zip file
        public Int64 size { get; set; }
        public string updateComment { get; set; }
        public string description { get; set; }
        public string devURL { get; set; }
        public List<string> userFiles = new List<string>();
        public bool Checked { get; set; }
        public bool DownloadFlag { get; set; }
    }
}
