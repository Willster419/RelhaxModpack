namespace RelhaxModpack.UIComponents
{
    public class RelhaxUserCheckBox : System.Windows.Forms.CheckBox, IPackageUIComponent
    {
        public SelectablePackage Package { get; set; }
        public void OnEnabledChanged(bool Enabled)
        {
            //this.Enabled = Enabled;
        }
        public void OnCheckedChanged(bool Checked)
        {
            //this.Checked = Checked;
        }
    }
}
