namespace RelhaxModpack
{
    class ConfigWPFComboBox : System.Windows.Controls.ComboBox, UIComponent
    {
        public Category catagory { get; set; }
        public Mod mod { get; set; }
        public Config config { get; set; }
    }
}
