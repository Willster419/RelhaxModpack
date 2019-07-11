using System;

namespace RelhaxModpack.UIComponents
{
    public class RelhaxFormComboBox : System.Windows.Forms.ComboBox, IPackageUIComponent
    {
        public SelectablePackage Package { get; set; }
        public EventHandler handler;
        public void OnEnabledChanged(bool Enabled)
        {

        }
        public void OnCheckedChanged(bool Checked)
        {
            
        }
        public void OnDropDownSelectionChanged(SelectablePackage spc, bool value)
        {
            for(int i = 0; i < Items.Count; i++)
            {
                ComboBoxItem cbi = (ComboBoxItem)Items[i];
                if (cbi.Package.Equals(spc) && value && cbi.Package.Enabled)
                {
                    if(handler != null)
                        SelectedIndexChanged -= handler;
                    SelectedItem = cbi;
                    if (handler != null)
                        SelectedIndexChanged += handler;
                    continue;
                }//if value is false it will uncheck all the packages
                if (cbi.Package.Enabled && cbi.Package.Checked)
                    cbi.Package.Checked = false;
            }
            if (!value)
            {
                if (handler != null)
                    SelectedIndexChanged -= handler;
                SelectedIndex = 0;
                if (handler != null)
                    SelectedIndexChanged += handler;
            }
        }
    }
}
