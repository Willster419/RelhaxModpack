using System.Windows.Media;

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
        public Brush TextColor
        {
            get
            { return Foreground; }
            set
            { Foreground = value; }
        }
        public Brush PanelColor
        {
            get
            {
                return Package.ParentBorder == null ? null : Package.ParentBorder.Background;
            }
            set
            {
                if (Package.ParentBorder != null)
                    Package.ParentBorder.Background = value;
            }
        }
    }
}
