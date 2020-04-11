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

        private StackPanel thePanel = null;

        /// <summary>
        /// Create an instance of the RelhaxWPFComboBox class
        /// </summary>
        public RelhaxWPFComboBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called from the database object to update the UI on a combobox selection change
        /// </summary>
        /// <param name="spc">The SelectablePakage that caused the update</param>
        /// <param name="value">The checked value</param>
        public void OnDropDownSelectionChanged(SelectablePackage spc, bool value)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                RelhaxComboBoxItem cbi = (RelhaxComboBoxItem)Items[i];
                if (cbi.Package.Equals(spc) && value && cbi.Package.Enabled)
                {
                    //change it
                    SelectedItem = cbi;
                    //continue as to not uncheck this value, now that it's checked
                    continue;
                }
                //if value is false it will uncheck all the packages
                if (cbi.Package.Enabled && cbi.Package.Checked)
                    cbi.Package.Checked = false;
            }
            if (!value)
            {
                SelectedIndex = 0;
            }
        }

        private void TemplateRootPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (thePanel == null)
            {
                thePanel = (StackPanel)sender;
                if (this.SelectedItem == null)
                    return;
                ApplyIcons();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedItem == null)
                return;
            if (thePanel == null)
                return;
            ApplyIcons();
        }

        private void ApplyIcons()
        {
            RelhaxComboBoxItem relhaxComboBoxItem = (RelhaxComboBoxItem)this.SelectedItem;
            SelectablePackage package = relhaxComboBoxItem.Package;

            while (thePanel.Children.Count > 1)
            {
                thePanel.Children.RemoveAt(thePanel.Children.Count - 1);
            }

            if (package.ObfuscatedMod)
            {
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/obfuscated_package_icon.png", UriKind.Relative))
                };
                thePanel.Children.Add(img);
            }
            if (package.GreyAreaMod)
            {
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/grey_area_mod.png", UriKind.Relative))
                };
                thePanel.Children.Add(img);
            }
            if (package.PopularMod)
            {
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/popular_icon.png", UriKind.Relative))
                };
                thePanel.Children.Add(img);
            }
        }
    }
}
