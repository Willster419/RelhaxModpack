using System.Collections.Generic;

namespace RelhaxModpack
{
    class SubConfig
    {
        //a Subconfig is a type of config that is a level below the config class. It can be used for choosing options within a config option,
        //or can be usefull to actually be a config option when the mod is at the config level. Like contour icons, for example
        public string name { get; set; }
        public string zipConfigFile { get; set; }
        public string crc { get; set; }
        //is the config currently broken?
        public bool enabled { get; set; }
        public bool configChecked { get; set; }
        //can the user select multiple configs or one only?
        public string type { get; set; }
        public List<string> pictureList = new List<string>();
        //the list of dependencies for this catagory
        public List<Dependency> catDependencies = new List<Dependency>();
        //size of the config zip file
        public float size { get; set; }
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for publicly shared files.
        public string startAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string endAddress { get; set; }
        //basic config constructor
        public SubConfig()
        {
            //by default make these false
            enabled = false;
            configChecked = false;
        }
    }
}
