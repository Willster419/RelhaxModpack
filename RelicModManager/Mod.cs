using System.Collections.Generic;

namespace RelhaxModpack
{
    //a mod is the core of the modpack. A modification for WoT.
    public class Mod
    {
        public string name { get; set; }
        //the developer's version of the mod
        public string version { get; set; }
        public string modZipFile { get; set; }
        public string crc { get; set; }
        public bool enabled { get; set; }
        public bool modChecked { get; set; }
        public List<Config> configs = new List<Config>();
        public string updateComment { get; set; }
        public string description { get; set; }
        public string devURL { get; set; }
        public List<Picture> picList = new List<Picture>();
        public List<string> userFiles = new List<string>();
        public List<Dependency> modDependencies = new List<Dependency>();
        //size of the mod zip file
        public float size { get; set; }
        //default constructor
        public Mod()
        {
            //by default make these false
            enabled = false;
            modChecked = false;
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
    }
}
