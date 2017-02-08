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
        public List<string> picturesList = new List<string>();
        public string updateComment {get; set;}
        public string description {get; set;}
        public string devURL {get; set;}
        
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
    }
}
