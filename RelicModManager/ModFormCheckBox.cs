namespace RelhaxModpack
{
    public class ModFormCheckBox : System.Windows.Forms.CheckBox, UIComponent
    {
        public Category catagory { get; set; }
        public Mod mod { get; set; }
        public Config config { get; set; }
    }
}
