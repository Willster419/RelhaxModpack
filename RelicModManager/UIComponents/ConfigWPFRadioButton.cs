namespace RelhaxModpack
{
    class ConfigWPFRadioButton : System.Windows.Controls.RadioButton, UIComponent
    {
        public Category catagory { get; set; }
        public SelectablePackage mod { get; set; }
        public SelectablePackage config { get; set; }
    }
}
