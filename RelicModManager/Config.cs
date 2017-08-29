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
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for publicly shared files.
        public string startAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string endAddress { get; set; }
        public string crc { get; set; }
        //is the config currently broken?
        public bool enabled { get; set; }
        //later a unique name of the config entry
        public string packageName { get; set; }
        //size of the config zip file
        public float size { get; set; }
        public string updateComment { get; set; }
        public string description { get; set; }
        public string devURL { get; set; }
        //can the user select multiple configs or one only?
        public string type { get; set; }
        public List<string> userFiles = new List<string>();
        public List<Media> pictureList = new List<Media>();
        public List<Config> configs = new List<Config>();
        //the list of dependencies for this catagory
        public List<Dependency> dependencies = new List<Dependency>();
        //the parent of a config is a mod OR another config
        public DatabaseObject parent { get; set; }
        //the absolute top of the config levels, the parent mod
        public Mod parentMod { get; set; }
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        public bool Checked { get; set; }
        public ModFormCheckBox modFormCheckBox { get; set; }
        public UIComponent configUIComponent { get; set; }
        public bool downloadFlag { get; set; }
        //basic config constructor
        public Config()
        {
            //by default make these false
            enabled = false;
            Checked = false;
            downloadFlag = false;
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
