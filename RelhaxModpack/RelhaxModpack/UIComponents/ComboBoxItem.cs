namespace RelhaxModpack
{
    //an item to add ot a ComboBox
    class ComboBoxItem
    {
        public SelectablePackage Package { get; set; }
        public string DisplayName { get; set; }
        public ComboBoxItem(SelectablePackage cfg, string display)
        {
            Package = cfg;
            DisplayName = display;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
