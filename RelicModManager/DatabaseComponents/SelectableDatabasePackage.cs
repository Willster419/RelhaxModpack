using RelhaxModpack.DatabaseComponents;
using System;
using System.Collections.Generic;

namespace RelhaxModpack
{
    public abstract class SelectableDatabasePackage : IDatabasePackage
    {
        public string Name { get; set; }
        public string NameFormatted
        {
            get { return Utils.ReplaceMacro(Name,"version",Version); }
        }
        //the developer's version of the mod
        public string Version { get; set; }
        public string ZipFile { get; set; }
        public long Timestamp { get; set; }
        public List<Config> configs = new List<Config>();
        //the start address of the zip file location. Enabled us to use sites that
        //generate random filenames for ly shared files.
        public string StartAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string EndAddress { get; set; }
        public List<LogicalDependency> LogicalDependencies = new List<LogicalDependency>();
        public List<Dependency> Dependencies = new List<Dependency>();
        public List<Media> PictureList = new List<Media>();
        public List<Shortcut> ShortCuts = new List<Shortcut>();
        public string CRC { get; set; }
        public bool Enabled { get; set; }
        //a flag to determine wether or not the mod should be shown
        public bool Visible { get; set; }
        //later a unique name of the config entry
        public string PackageName { get; set; }
        //size of the mod zip file
        public Int64 Size { get; set; }
        public string UpdateComment { get; set; }
        public string Description { get; set; }
        public string DevURL { get; set; }
        public List<string> UserFiles = new List<string>();
        public bool Checked { get; set; }
        public bool DownloadFlag { get; set; }
    }
}
