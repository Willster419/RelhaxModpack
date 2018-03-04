namespace RelhaxModpack.UIComponents
{
    public class RelhaxWPFCheckBox : System.Windows.Controls.CheckBox, IPackageUIComponent
    {
        public SelectablePackage Package { get; set; }
        public void OnEnabledChanged(bool Enabled)
        {
            this.IsEnabled = Enabled;
        }
        public void OnCheckedChanged(bool Checked)
        {
            this.IsChecked = Checked;
        }
    }
}
