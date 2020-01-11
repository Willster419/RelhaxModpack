using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RelhaxModpack.UIComponents
{
    /// <summary>
    /// Interaction logic for RelhaxWPFComboBox.xaml
    /// </summary>
    public partial class RelhaxWPFComboBox : ComboBox
    {
        /// <summary>
        /// Flag to determine if the Combobox object has been already added to the ModSelectionList window
        /// </summary>
        /// <remarks>Many components of 'single_dropDown' exist in the Combobox, and therefore the UI generation code gets run for each object.
        /// So, a flag is used to prevent the ComboBox being added multiple times to the window</remarks>
        public bool AddedToList { get; set; } = false;

        /// <summary>
        /// Create an instance of the RelhaxWPFComboBox class
        /// </summary>
        public RelhaxWPFComboBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The event to subscribe to when the selection is changed
        /// </summary>
        public SelectionChangedEventHandler Handler;

        /// <summary>
        /// Called from the database object to update the UI on a combobox selection change
        /// </summary>
        /// <param name="spc">The SelectablePakage that caused the update</param>
        /// <param name="value">The checked value</param>
        public void OnDropDownSelectionChanged(SelectablePackage spc, bool value)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                ComboBoxItem cbi = (ComboBoxItem)Items[i];
                if (cbi.Package.Equals(spc) && value && cbi.Package.Enabled)
                {
                    //unsubscribe before changing the selected item
                    if (Handler != null)
                        SelectionChanged -= Handler;
                    //change it
                    SelectedItem = cbi;
                    //re-subscribe
                    if (Handler != null)
                        SelectionChanged += Handler;
                    //continue as to not uncheck this value, now that it's checked
                    continue;
                }
                //if value is false it will uncheck all the packages
                if (cbi.Package.Enabled && cbi.Package.Checked)
                    cbi.Package.Checked = false;
            }
            if (!value)
            {
                SelectionChanged -= Handler;
                SelectedIndex = 0;
                SelectionChanged += Handler;
            }
        }
    }
}
