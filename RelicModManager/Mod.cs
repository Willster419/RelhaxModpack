using System.Collections.Generic;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //a mod is the core of the modpack. A modification for WoT.
    //spacer
    public class Mod : DatabaseObject
    {
        public string name { get; set; }
        //the developer's version of the mod
        public string version { get; set; }
        public string zipFile { get; set; }
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for publicly shared files.
        public string startAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string endAddress { get; set; }
        public string crc { get; set; }
        public bool enabled { get; set; }
        //the tab index in the modpack 
        public TabPage tabIndex { get; set; }
        //later a unique name of the config entry
        public string packageName { get; set; }
        //size of the mod zip file
        public float size { get; set; }
        public string updateComment { get; set; }
        public string description { get; set; }
        public string devURL { get; set; }
        public List<string> userFiles = new List<string>();
        public List<Picture> pictureList = new List<Picture>();
        public List<Config> configs = new List<Config>();
        public List<Dependency> dependencies = new List<Dependency>();
        //the parent of a mod is a category
        public Category parent { get; set; }
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        public bool Checked { get; set; }
        public UIComponent modFormCheckBox { get; set; }
        public bool downloadFlag { get; set; }
        //default constructor
        public Mod()
        {
            //by default make these false
            enabled = false;
            Checked = false;
            downloadFlag = false;
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
