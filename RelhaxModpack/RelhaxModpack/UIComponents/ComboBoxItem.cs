namespace RelhaxModpack
{
    //an item to add to a ComboBox
    class ComboBoxItem : System.Windows.Controls.ComboBoxItem
    {
        public SelectablePackage Package { get; set; }
        public string DisplayName { get; set; }
        public ComboBoxItem(SelectablePackage package, string display)
        {
            Package = package;
            DisplayName = display;
        }
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
