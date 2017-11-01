namespace RelhaxModpack
{
    class ModWPFCheckBox : System.Windows.Controls.CheckBox, UIComponent
    {
        public Category catagory { get; set; }
        public Mod mod { get; set; }
        public Config config { get; set; }
    }
}
