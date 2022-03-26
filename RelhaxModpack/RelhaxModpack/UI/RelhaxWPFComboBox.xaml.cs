using RelhaxModpack.Database;
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

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Fires when the SelectionChanged event fires, but verifies that it's from user interaction (mouse of space bar/enter)
    /// </summary>
    /// <param name="source">The source of causing the event</param>
    /// <param name="e">The parameters from the SelectionChanged event</param>
    public delegate void OnSelectionCommitted(object source, SelectionChangedEventArgs e);

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
        /// Gets or sets the selected index property in the base class without invoking the selectionChanged event
        /// </summary>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/new-modifier"/>
        public new int SelectedIndex
        {
            get
            {
                return base.SelectedIndex;
            }
            set
            {
                this.SelectionFromUser = false;
                base.SelectedIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected item property in the base class without invoking the selectionChanged event
        /// </summary>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/new-modifier"/>
        public new object SelectedItem
        {
            get
            {
                return base.SelectedItem;
            }
            set
            {
                this.SelectionFromUser = false;
                base.SelectedItem = value;
            }
        }

        /// <summary>
        /// Event to fire when the selection changes from user interaction (mouse of space bar/enter)
        /// </summary>
        public event OnSelectionCommitted SelectionCommitted;

        //keeps track of the actual selected value
        private int SelectedIndexBackup = -1;

        //flag to determine if the selection commit event should fire
        //this needs to be set to false right before any selected properties are set
        private bool SelectionFromUser = true;

        private StackPanel thePanel = null;

        /// <summary>
        /// Create an instance of the RelhaxWPFComboBox class
        /// </summary>
        public RelhaxWPFComboBox()
        {
            InitializeComponent();
            SelectedIndex = 0;
            this.SelectionChanged += Combobox_SelectionChanged;
            this.DropDownOpened += Combobox_DropDownOpened;
            this.DropDownClosed += Combobox_DropDownClosed;
        }

        #region SelectionChanged event suppression
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
                    this.SelectedIndex = i;
                    //continue as to not uncheck this value, now that it's checked
                    continue;
                }

                //if value is false it will uncheck all the packages
                if (cbi.Package.Enabled && cbi.Package.Checked)
                    cbi.Package.Checked = false;
            }

            if (!value)
            {
                //reset to first selection option
                this.SelectedIndex = 0;
            }

            if (spc.ChangeColorOnValueChecked && spc.Visible && spc.IsStructureVisible)
            {
                if (spc.Checked || spc.AnyPackagesChecked())
                {
                    spc.ParentBorder.IsChildPackageChecked = true;
                }
                else
                {
                    spc.ParentBorder.IsChildPackageChecked = false;
                }
            }
        }

        private void Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!SelectionFromUser)
            {
                SelectionFromUser = true;
                return;
            }

            SelectionCommitted?.Invoke(this, e);
        }

        private void Combobox_DropDownOpened(object sender, EventArgs e)
        {
            //set selection to nothing, but back it up first
            SelectedIndexBackup = this.SelectedIndex;
            this.SelectedIndex = -1;
        }

        private void Combobox_DropDownClosed(object sender, EventArgs e)
        {
            if (this.SelectedIndex == -1)
            {
                //a selection was not chosen, the user must have clicked out, so set the original back
                this.SelectedIndex = SelectedIndexBackup;
            }
        }
        #endregion

        #region Image attach code
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
            if (package.FromWGmods)
            {
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/wgmods_package.png", UriKind.Relative))
                };
            }
        }
        #endregion
    }
}
