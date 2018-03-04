namespace RelhaxModpack.UIComponents
{
    public class RelhaxWPFRadioButton : System.Windows.Controls.RadioButton, IPackageUIComponent
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
