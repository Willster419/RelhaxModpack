using System.Collections.Generic;

namespace RelhaxModpack
{
    //the catagory class. a catagory is what makes up each tab in
    //the mod selection dislpay window
    public class Category : DatabaseObject
    {
        public string name { get; set; }
        //the selection type of the catagory. can a user select
        //multiple mods of this catagory, or just one?
        public string selectionType { get; set; }
        public List<Mod> mods = new List<Mod>();
        //list of dependencies required if anything is
        //selected from this catagory
        public List<Dependency> dependencies = new List<Dependency>();
        public Category() { }
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
        //sorts the catagories
        public static int CompareCatagories(Category x, Category y)
        {
            return x.name.CompareTo(y.name);
        }
    }
}
