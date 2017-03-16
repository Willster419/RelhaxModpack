using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        //size of the config zip file
        public float size { get; set; }
        //basic config constructor
        public Config()
        {
            //by default make these false
            enabled = false;
            configChecked = false;
        }
    }
}
