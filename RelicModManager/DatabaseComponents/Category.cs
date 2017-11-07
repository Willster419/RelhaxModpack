using System.Collections.Generic;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //the catagory class. a catagory is what makes up each tab in
    //the mod selection dislpay window
    public class Category
    {
        public string Name { get; set; }
        //the selection type of the catagory. can a user select
        //multiple mods of this catagory, or just one?
        public string SelectionType { get; set; }
        public List<Mod> Mods = new List<Mod>();
        //list of dependencies required if anything is
        //selected from this catagory
        public List<Dependency> Dependencies = new List<Dependency>();
        public Category() { }
        //returns the mod with the specified name
        //if it does not exist, it returns null
        public Mod GetMod(string modName)
        {
            if (Mods == null || Mods.Count == 0)
                return null;
            foreach (Mod m in Mods)
            {
                if (m.Name.Equals(modName))
                    return m;
            }
            return null;
        }
        //the TabPage refrence for the UI
        public TabPage @TabPage { get; set; }
        //sorts the catagories
        public static int CompareCatagories(Category x, Category y)
        {
            return x.Name.CompareTo(y.Name);
        }
        //for the tostring thing
        public override string ToString()
        {
            return Name;
        }
    }
}
