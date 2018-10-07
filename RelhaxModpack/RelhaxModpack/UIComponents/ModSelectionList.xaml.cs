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
    public struct RelhaxProgress
    {
        public string ReportMessage;
        public int ChildProgressCurrent;
        public int ChildProgressTotal;
    }
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
        private ProgressIndicator loadingProgress;

        public ModSelectionList()
        {
            InitializeComponent();
        }

        private async void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            //init the lists
            ParsedCategoryList = new List<Category>();
            GlobalDependencies = new List<Dependency>();
            LogicalDependencies = new List<Dependency>();
            //create and show loading window
            loadingProgress = new ProgressIndicator()
            {
                ProgressMaximum = 8,
                ProgressMinimum = 0,
                Message = Translations.GetTranslatedString("loading")
            };
            loadingProgress.Show();
            this.Hide();
            //create and run async task
            try
            {
                Logging.WriteToLog("Starting async task: " + nameof(LoadSelectionListAsync) + "()");
                //https://blogs.msdn.microsoft.com/dotnet/2012/06/06/async-in-4-5-enabling-progress-and-cancellation-in-async-apis/
                var progressIndicator = new Progress<RelhaxProgress>(OnWindowLoadReportProgress);
                bool result = await LoadSelectionListAsync(progressIndicator);
                if (!result)
                    throw new BadMemeException("Result was false!!");
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("Failed to load ModSelectionList window\n" + ex.ToString(), Logfiles.Application, LogLevel.Exception);
                MessageBox.Show(Translations.GetTranslatedString("failedToLoadSelectionList"),
                    Translations.GetTranslatedString("critical"));
                loadingProgress.Close();
                loadingProgress = null;
                this.Close();
                return;
            }
            loadingProgress.Close();
            loadingProgress = null;
            this.Show();
        }

        private void OnWindowLoadReportProgress(RelhaxProgress progress)
        {
            if (loadingProgress != null)
            {
                loadingProgress.Message = progress.ReportMessage;
                loadingProgress.ProgressValue = progress.ChildProgressCurrent;
                loadingProgress.ProgressMaximum = progress.ChildProgressTotal;
            }
        }

        private async Task<bool> LoadSelectionListAsync(IProgress<RelhaxProgress> progress)
        {
            RelhaxProgress loadProgress = new RelhaxProgress()
            {
                ChildProgressTotal = 8,
                ChildProgressCurrent = 1,
                ReportMessage = Translations.GetTranslatedString("downloadingDatabase")
            };
            progress.Report(loadProgress);
            //-download online modInfo

            //-parse more versoin stuff if need be
            loadProgress.ChildProgressCurrent++;
            loadProgress.ReportMessage = Translations.GetTranslatedString("parsingDatabase");
            progress.Report(loadProgress);

            //-check db cache of local files
            loadProgress.ChildProgressCurrent++;
            loadProgress.ReportMessage = Translations.GetTranslatedString("verifyingDownloadCache");
            progress.Report(loadProgress);

            //-build UI
            loadProgress.ChildProgressCurrent++;
            loadProgress.ReportMessage = Translations.GetTranslatedString("loadingUI");
            progress.Report(loadProgress);

            return true;
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
