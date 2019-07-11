using System;

namespace RelhaxModpack.UIComponents
{
    public class RelhaxFormCheckBox : System.Windows.Forms.CheckBox, IPackageUIComponent
    {
        public SelectablePackage Package { get; set; }
        //up to two handlers may be needed
        public EventHandler handler;
        public void OnEnabledChanged(bool Enabled)
        {
            this.Enabled = Enabled;
        }
        public void OnCheckedChanged(bool Checked)
        {
            //CheckedChanged -= handler;
            this.Checked = Checked;
            //CheckedChanged += handler;
            //ALSO HANDLE COLOR CHANGE CODE HERE
        }
    }
}
