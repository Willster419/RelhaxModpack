using System.Collections.Generic;

namespace RelhaxModpack
{
    //a config is a configuration for a mod to make the mod function if a certain way
    public class Config
    {
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
        public Config()
        {
            //by default make these false
            enabled = false;
            configChecked = false;
        }
    }
}
