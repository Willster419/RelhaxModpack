namespace RelhaxModpack.UIComponents
{
    public class RelhaxWPFCheckBox : System.Windows.Controls.CheckBox, IPackageUIComponent
    {
        public SelectablePackage Package { get; set; }
        public void OnEnabledChanged(bool Enabled)
        {
            IsEnabled = Enabled;
        }
        public void OnCheckedChanged(bool Checked)
        {
            IsChecked = Checked;
        }
    }
}
