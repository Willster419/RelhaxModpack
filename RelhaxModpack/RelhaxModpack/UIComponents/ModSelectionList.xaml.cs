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

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for ModSelectionList.xaml
    /// </summary>
    public partial class ModSelectionList : RelhaxWindow
    {
        private SolidColorBrush SelectedColor = new SolidColorBrush(Colors.BlanchedAlmond);
        private SolidColorBrush NotSelectedColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush SelectedTextColor = SystemColors.ControlTextBrush;
        private SolidColorBrush NotSelectedTextColor = SystemColors.ControlTextBrush;
        public List<Category> ParsedCategoryList;
        public List<Dependency> GlobalDependencies;
        public List<Dependency> LogicalDependencies;
        public bool ContinueInstallation { get; set; } = false;

        public ModSelectionList()
        {
            InitializeComponent();
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            //init the lists
            ParsedCategoryList = new List<Category>();
            GlobalDependencies = new List<Dependency>();
            LogicalDependencies = new List<Dependency>();
            this.Hide();
            //create async task to:
            //-creat loading window
            //-download online modInfo
            //-parse more versoin stuff if need be
            //-check db cache of local files
            //-build UI
        }

        private void OnContinueInstallation(object sender, RoutedEventArgs e)
        {
            ContinueInstallation = true;
            this.Close();
        }

        private void OnCancelInstallation(object sender, RoutedEventArgs e)
        {
            ContinueInstallation = false;
            this.Close();
        }

        private void OnSaveSelectionClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnLoadSelectionClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnClearSelectionsClick(object sender, RoutedEventArgs e)
        {
            Logging.WriteToLog("Clearing selections");
            foreach (Category category in ParsedCategoryList)
                ClearSelections(category.Packages);
            Logging.WriteToLog("Selections cleared");
            MessageBox.Show(Translations.GetTranslatedString("selectionsCleared"));
        }

        private void ClearSelections(List<SelectablePackage> packages)
        {
            foreach(SelectablePackage package in packages)
            {
                if (package.Packages.Count > 0)
                    ClearSelections(package.Packages);
                package.Checked = false;
            }
        }

        private void LoadSelection()
        {

        }

        private void SaveSelection()
        {

        }
    }
}
