namespace RelhaxModpack
{
    //an item to add ot a ComboBox
    class ComboBoxItem
    {
        public SelectablePackage config { get; set; }
        public string displayName { get; set; }
        public ComboBoxItem(SelectablePackage cfg, string display)
        {
            config = cfg;
            displayName = display;
        }
        public override string ToString()
        {
            return displayName;
        }
    }
}
