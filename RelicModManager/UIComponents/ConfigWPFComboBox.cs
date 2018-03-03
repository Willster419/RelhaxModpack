namespace RelhaxModpack
{
    class ConfigWPFComboBox : System.Windows.Controls.ComboBox, UIComponent
    {
        public Category catagory { get; set; }
        public SelectablePackage mod { get; set; }
        public SelectablePackage config { get; set; }
    }
}
