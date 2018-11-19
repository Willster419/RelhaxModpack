using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace RelhaxModpack.UIComponents
{
    public class RelhaxWPFComboBox : System.Windows.Controls.ComboBox, IPackageUIComponent
    {
        public SelectablePackage Package { get; set; }
        public SelectionChangedEventHandler handler;
        public void OnEnabledChanged(bool Enabled)
        {

        }
        public void OnCheckedChanged(bool Checked)
        {

        }
        public Brush TextColor
        {
            get
            { return null; }
            set
            {  }
        }
        public Brush PanelColor
        {
            get
            {
                return Package.ParentBorder == null? null : Package.ParentBorder.Background;
            }
            set
            {
                if (Package.ParentBorder != null)
                    Package.ParentBorder.Background = value;
            }
        }
        public void OnDropDownSelectionChanged(SelectablePackage spc, bool value)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                ComboBoxItem cbi = (ComboBoxItem)Items[i];
                if (cbi.Package.Equals(spc) && value && cbi.Package.Enabled)
                {
                    if(handler != null)
                        SelectionChanged -= handler;
                    SelectedItem = cbi;
                    if (handler != null)
                        SelectionChanged += handler;
                    continue;
                }//if value is false it will uncheck all the packages
                if (cbi.Package.Enabled && cbi.Package.Checked)
                    cbi.Package.Checked = false;
            }
            if (!value)
            {
                SelectionChanged -= handler;
                SelectedIndex = 0;
                SelectionChanged += handler;
            }
        }
    }
}
