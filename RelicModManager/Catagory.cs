using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelicModManager
{
    public class Catagory
    {
        public string name { get; set; }
        public string selectionType { get; set; }
        public List<Mod> mods = new List<Mod>();
        public List<Dependency> dependencies = new List<Dependency>();
        public Catagory() { }
        //returns the mod with the specified name
        //if it does not exist, it returns null
        public Mod getMod(string modName)
        {
          if (mods == null || mods.Count == 0)
            return null;
          foreach (Mod m in mods)
          {
            if (m.name.Equals(modName))
              return m;
          }
          return null;
        }
    }
}
