using System.Collections.Generic;

namespace RelhaxModpack
{
    //a config is a configuration for a mod to make the mod function if a certain way
    //in some scenarios, the config is the mod itself
    public class Config : DatabaseObject
    {
        public string name { get; set; }
        public string version { get; set; }
        public string zipFile { get; set; }
        public string crc { get; set; }
        //is the config currently broken?
        public bool enabled { get; set; }
        public bool Checked { get; set; }
        //the array of sub config options available
        public List<Config> configs = new List<Config>();
        public string updateComment { get; set; }
        public string description { get; set; }
        public string devURL { get; set; }
        public List<string> pictureList = new List<string>();
        //the list of dependencies for this catagory
        public List<string> userFiles = new List<string>();
        public List<Dependency> dependencies = new List<Dependency>();
        //the parent of a config is a mod OR another config
        public DatabaseObject parent { get; set; }
        //public DatabaseObject parent2 { get; set; }
        //the index where this config is in the entire list of configs ever
        public int index { get; set; }
        //size of the config zip file
        public float size { get; set; }
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for publicly shared files.
        public string startAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string endAddress { get; set; }
        //can the user select multiple configs or one only?
        public string type { get; set; }
        //the absolute top of the config levels, the parent mod
        public Mod parentMod { get; set; }
        //basic config constructor
        public Config()
        {
            //by default make these false
            enabled = false;
            Checked = false;
        }
        public Config getSubConfig(string subConfigName)
        {
            if (configs == null || configs.Count == 0)
                return null;
            foreach (Config sc in configs)
            {
                if (sc.name.Equals(subConfigName))
                    return sc;
            }
            return null;
        }
        //for the tostring thing
        public override string ToString()
        {
            return name;
        }
    }
}
