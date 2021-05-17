using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using RelhaxModpack.Database;
using RelhaxModpack.Settings;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for EditorAddRemove.xaml
    /// </summary>
    public partial class EditorAddRemove : RelhaxWindow
    {
        //public
        /// <summary>
        /// The list of parsed global dependencies
        /// </summary>
        public List<DatabasePackage> GlobalDependencies;

        /// <summary>
        /// The list of parsed dependencies
        /// </summary>
        public List<Dependency> Dependencies;

        /// <summary>
        /// The list of parsed categories
        /// </summary>
        public List<Category> ParsedCategoryList;

        /// <summary>
        /// The referenced package when selecting to add, the package selected when removing
        /// </summary>
        public DatabasePackage SelectedPackage = null;

        /// <summary>
        /// True is moving package, false is adding package
        /// </summary>
        public bool EditOrAdd = true;

        /// <summary>
        /// Toggles if the user requests to add the package at a new level or in the same level as the referenced package
        /// </summary>
        public bool AddSameLevel = true;

        /// <summary>
        /// The currently selected package in the editor's database tree view
        /// </summary>
        public DatabasePackage DatabaseTreeviewSelectedItem = null;

        //private
        private const string GlobalDependenciesCategoryHeader = "--Global Dependencies--";
        private const string DependenciesCategoryHeader = "--Dependencies--";

        /// <summary>
        /// Create an instance of the EditorAddRemove window
        /// </summary>
        public EditorAddRemove(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //load the top box with selection info
            CategoryComboBox.Items.Clear();
            CategoryComboBox.Items.Add(GlobalDependenciesCategoryHeader);
            CategoryComboBox.Items.Add(DependenciesCategoryHeader);
            foreach (Category cat in ParsedCategoryList)
                CategoryComboBox.Items.Add(cat);

            PackageComboBox.Items.Clear();
            //edit or add info
            if(EditOrAdd)
            {
                //edit
                AddSameLevelRadioButton.Visibility = Visibility.Hidden;
                AddSameLevelRadioButton.IsEnabled = false;
                AddNewLevelRadioButton.Visibility = Visibility.Hidden;
                AddNewLevelRadioButton.IsEnabled = false;
                CategoryTextBox.Text = "Select the category to move package to";
                PackageTextBox.Text = "Select the package to move to";
            }
            else
            {
                //add
                AddSameLevelRadioButton.Visibility = Visibility.Visible;
                AddSameLevelRadioButton.IsEnabled = true;
                AddNewLevelRadioButton.Visibility = Visibility.Visible;
                AddNewLevelRadioButton.IsEnabled = true;
                CategoryTextBox.Text = "Select the category to add package to";
                PackageTextBox.Text = "Select the package to add to";
            }
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CategoryComboBox.SelectedItem is string s)
            {
                if(s.Equals(GlobalDependenciesCategoryHeader))
                {
                    foreach (DatabasePackage package in GlobalDependencies)
                    {
                        if (!package.Equals(DatabaseTreeviewSelectedItem))
                            PackageComboBox.Items.Add(new EditorComboBoxItem(package));
                        else
                            Logging.Editor("Skipping adding {0} because it is currently selected", LogLevel.Info, DatabaseTreeviewSelectedItem.PackageName);
                    }
                }
                else if (s.Equals(DependenciesCategoryHeader))
                {
                    foreach (DatabasePackage package in Dependencies)
                    {
                        if (!package.Equals(DatabaseTreeviewSelectedItem))
                            PackageComboBox.Items.Add(new EditorComboBoxItem(package));
                        else
                            Logging.Editor("Skipping adding {0} because it is currently selected", LogLevel.Info, DatabaseTreeviewSelectedItem.PackageName);
                    }
                }
            }
            else if(CategoryComboBox.SelectedItem is Category cat)
            {
                PackageComboBox.Items.Clear();
                foreach (DatabasePackage package in cat.GetFlatPackageList())
                {
                    if (!package.Equals(DatabaseTreeviewSelectedItem))
                        PackageComboBox.Items.Add(new EditorComboBoxItem(package));
                    else
                        Logging.Editor("Skipping adding {0} because it is currently selected", LogLevel.Info, DatabaseTreeviewSelectedItem.PackageName);
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PackageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PackageComboBox.SelectedItem is EditorComboBoxItem item)
            {
                SelectedPackage = item.Package;
                OKButton.IsEnabled = true;
            }
        }

        private void AddSameLevelRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            AddSameLevel = true;
        }

        private void AddNewLevelRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            AddSameLevel = false;
        }
    }
}
