namespace RelhaxModpack.UIComponents
{
    public class RelhaxWPFRadioButton : System.Windows.Controls.RadioButton, IPackageUIComponent
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
