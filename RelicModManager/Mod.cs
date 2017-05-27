using System.Collections.Generic;

namespace RelhaxModpack
{
    //a mod is the core of the modpack. A modification for WoT.
    public class Mod
    {
        public string name { get; set; }
        //the developer's version of the mod
        public string version { get; set; }
        public string zipFile { get; set; }
        public string crc { get; set; }
        public bool enabled { get; set; }
        public bool Checked { get; set; }
        public List<Config> configs = new List<Config>();
        public string updateComment { get; set; }
        public string description { get; set; }
        public string devURL { get; set; }
        public List<Picture> pictureList = new List<Picture>();
        public List<string> userFiles = new List<string>();
        public List<Dependency> dependencies = new List<Dependency>();
        //the parent of a mod is a config
        public Category parent { get; set; }
        //the index of where the mod is in the entire list ever
        public int index { get; set; }
        //size of the mod zip file
        public float size { get; set; }
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for publicly shared files.
        public string startAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string endAddress { get; set; }
        //default constructor
        public Mod()
        {
            //by default make these false
            enabled = false;
            Checked = false;
        }
        //returns the config of the specified name
        //if it does not exist, it returns null
        public Config getConfig(string configName)
        {
            if (configs == null || configs.Count == 0)
                return null;
            foreach (Config cfg in configs)
            {
                if (cfg.name.Equals(configName))
                    return cfg;
            }
            return null;
        }
        //sorts the mods
        public static int CompareMods(Mod x, Mod y)
        {
            return x.name.CompareTo(y.name);
        }
        //for the tostring thing
        public override string ToString()
        {
            return name;
        }
    }
}
