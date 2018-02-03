namespace RelhaxModpack
{
    //a config is a configuration for a mod to make the mod function if a certain way
    //in some scenarios, the config is the mod itself
    public class Config : SelectableDatabasePackage
    {
        //can the user select multiple configs or one only?
        public string Type { get; set; }
        
        
        //the list of dependencies for this catagory
        //the parent of a config is a mod OR another config
        public SelectableDatabasePackage Parent { get; set; }
        //the absolute top of the config levels, the parent mod
        public Mod ParentMod { get; set; }
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        public ModFormCheckBox ModFormCheckBox { get; set; }
        public UIComponent ConfigUIComponent { get; set; }
        //basic config constructor
        public Config()
        {
            //by default make these false
            Enabled = false;
            Checked = false;
            DownloadFlag = false;
            Visible = true;
            ReadyForInstall = false;
        }
        public Config GetSubConfig(string subConfigName)
        {
            if (configs == null || configs.Count == 0)
                return null;
            foreach (Config sc in configs)
            {
                if (sc.Name.Equals(subConfigName))
                    return sc;
            }
            return null;
        }
        //for the tostring thing
        public override string ToString()
        {
            return NameFormatted;
        }
    }
}
