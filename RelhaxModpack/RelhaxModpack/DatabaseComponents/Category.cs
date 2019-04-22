using System.Collections.Generic;
using RelhaxModpack.UIComponents;
using System.Windows.Controls;
using System;

namespace RelhaxModpack
{
    /// <summary>
    /// a catagory is what makes up each tab in the mod selection dislpay window. It holds the first level of list of SelectablePackages
    /// </summary>
    public class Category
    {
        #region Database Properties

        public string Name = "";

        public string XmlFilename = "";
        
        public List<SelectablePackage> Packages = new List<SelectablePackage>();

        //the install group number of the category
        //https://stackoverflow.com/questions/1759352/how-to-mark-a-method-as-obsolete-or-deprecated
        [Obsolete("This is for legacy database compatibility and will be ignored in Relhax V2")]
        public int InstallGroup = 0;
        #endregion

        #region UI Properties

        //the TabItem refrence for the UI
        public TabItem TabPage = null;

        //the holder for all packages of a catagory. UI only
        public SelectablePackage CategoryHeader = null;

        public TreeView @TreeView;

        //list of dependencies required if anything is selected from this catagory
        public List<DatabaseLogic> Dependencies = new List<DatabaseLogic>();
        #endregion

        #region Other Properties and Methods

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

        public List<SelectablePackage> GetFlatPackageList()
        {
            List<SelectablePackage> flatPackageList = new List<SelectablePackage>();
            foreach(SelectablePackage selectablePackage in Packages)
            {
                flatPackageList.Add(selectablePackage);
                if (selectablePackage.Packages.Count > 0)
                    GetFlatPackageList(flatPackageList, selectablePackage.Packages);
            }
            return flatPackageList;
        }

        private void GetFlatPackageList(List<SelectablePackage> flatPackageList, List<SelectablePackage> selectablePackages)
        {
            foreach (SelectablePackage selectablePackage in selectablePackages)
            {
                flatPackageList.Add(selectablePackage);
                if (selectablePackage.Packages.Count > 0)
                    GetFlatPackageList(flatPackageList, selectablePackage.Packages);
            }
        }
        #endregion
    }
}
