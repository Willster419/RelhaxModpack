/*
using System.Windows.Forms;

namespace RelhaxModpack
{
    //a mod is the core of the modpack. A modification for WoT.
    //spacer
    public class Mod : SelectableDatabasePackage
    {
        //the tab index in the modpack 
        public TabPage TabIndex { get; set; }
        
        //the parent of a mod is a category
        public Category ParentCategory { get; set; }
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        //public bool Checked { get; set; }
        public UIComponent ModFormCheckBox { get; set; }
        //default constructor
        public Mod()
        {
            //by default make these false
            Enabled = false;
            Checked = false;
            DownloadFlag = false;
            Visible = true;
            ReadyForInstall = false;
        }
        //returns the config of the specified name
        //if it does not exist, it returns null
        public Config GetConfig(string configName)
        {
            if (configs == null || configs.Count == 0)
                return null;
            foreach (Config cfg in configs)
            {
                if (cfg.Name.Equals(configName))
                    return cfg;
            }
            return null;
        }
        //sorts the mods
        public static int CompareMods(Mod x, Mod y)
        {
            return x.Name.CompareTo(y.Name);
        }
        //for the tostring thing
        public override string ToString()
        {
            return NameFormatted;
        }
    }
}
*/
