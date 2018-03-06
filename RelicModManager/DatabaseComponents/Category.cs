using System.Collections.Generic;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //the catagory class. a catagory is what makes up each tab in
    //the mod selection dislpay window
    public class Category
    {
        public string Name = "";
        //for super extraction, the group that should be used for the category
        public int InstallGroup = 0;
        //the TabPage refrence for the UI
        public TabPage @TabPage = null;
        public List<SelectablePackage> Packages = new List<SelectablePackage>();
        //list of dependencies required if anything is
        //selected from this catagory
        public List<Dependency> Dependencies = new List<Dependency>();
        public Category()
        {
            InstallGroup = 0;
        }
        //returns the mod with the specified name
        //if it does not exist, it returns null
        public SelectablePackage GetSelectablePackage(string packageNameOld)
        {
            if (Packages == null || Packages.Count == 0)
                return null;
            foreach (SelectablePackage sp in Packages)
            {
                if (sp.Name.Equals(packageNameOld))
                    return sp;
            }
            return null;
        }
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
