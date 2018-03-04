namespace RelhaxModpack.UIComponents
{
    public class RelhaxFormComboBox : System.Windows.Forms.ComboBox, IPackageUIComponent
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
