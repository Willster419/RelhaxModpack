namespace RelhaxModpack
{
    public class ConfigFormRadioButton : System.Windows.Forms.RadioButton, UIComponent
    {
        public Category catagory { get; set; }
        public Mod mod { get; set; }
        public Config config { get; set; }
    }
}
