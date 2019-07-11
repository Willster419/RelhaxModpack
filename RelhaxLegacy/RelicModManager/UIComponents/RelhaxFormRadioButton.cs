namespace RelhaxModpack.UIComponents
{
    public class RelhaxFormRadioButton : System.Windows.Forms.RadioButton, IPackageUIComponent
    {
        public SelectablePackage Package { get; set; }
        public void OnEnabledChanged(bool Enabled)
        {
            this.Enabled = Enabled;
        }
        public void OnCheckedChanged(bool Checked)
        {
            this.Checked = Checked;
        }
    }
}
