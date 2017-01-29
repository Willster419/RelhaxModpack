using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelicModManager
{
    public class Mod
    {
        public string name { get; set; }
        public float version { get; set; }
        public string modZipFile { get; set; }
        public string crc { get; set; }
        public bool enabled { get; set; }
        public bool modChecked { get; set; }
        public string configType { get; set; }
        public List<Config> configs = new List<Config>();
        public string configDefault { get; set; }

        //creates an empty mod instance
        public Mod()
        {

        }
    }
}
