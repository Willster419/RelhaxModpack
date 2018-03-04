namespace RelhaxModpack.UIComponents
{
    public class RelhaxWPFComboBox : System.Windows.Controls.ComboBox, IPackageUIComponent
    {
        public SelectablePackage Package { get; set; }
        public void OnEnabledChanged(bool Enabled)
        {

        }
        public void OnCheckedChanged(bool Checked)
        {

        }
    }
}
