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
using System.Xml;
using System.Net;

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
        private bool developerSelectionsReady = false;
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
                ChildProgressTotal = 4,
                ChildProgressCurrent = 1,
                ReportMessage = Translations.GetTranslatedString("downloadingDatabase")
            };
            progress.Report(loadProgress);
            //download online modInfo into xml file
            XmlDocument modInfoDocument = new XmlDocument();
            string modInfoXml = "";
            //get is based on different types of database mode
            switch(ModpackSettings.DatabaseDistroVersion)
            {
                case DatabaseVersions.Stable:
                    //make string
                    string modInfoxmlURL = Settings.DefaultStartAddress + "modInfo.dat";
                    modInfoxmlURL = modInfoxmlURL.Replace("{onlineFolder}", Settings.WoTModpackOnlineFolderVersion);
                    //download dat file
                    string tempDownloadLocation = System.IO.Path.Combine(Settings.RelhaxTempFolder, "modInfo.dat");
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            client.DownloadFile(modInfoxmlURL, tempDownloadLocation);
                        }
                        catch (Exception ex)
                        {
                            Logging.WriteToLog("Failed to download managerInfo.dat from " + modInfoxmlURL + "\n" + ex.ToString(),
                                Logfiles.Application, LogLevel.Exception);
                            return false;
                        }
                    }
                    //extract modinfo xml string
                    modInfoXml = Utils.GetStringFromZip(tempDownloadLocation, "modInfo.xml");
                    break;
                case DatabaseVersions.Beta:
                    //load string constant url from manager info xml
                    string managerInfoXml = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");
                    if (string.IsNullOrWhiteSpace(managerInfoXml))
                    {
                        Logging.WriteToLog("Failed to parse manager_version.xml from string from zipfile", Logfiles.Application, LogLevel.Exception);
                        MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " manager_version.xml");
                        return false;
                    }
                    //get download URL of static beta database location
                    string downloadURL = XMLUtils.GetXMLStringFromXPath(managerInfoXml, "//version/database_beta_url", "manager_version.xml");
                    if(string.IsNullOrWhiteSpace(downloadURL))
                    {
                        Logging.WriteToLog("Failed to get xpath value //version/database_beta_url from manager_version.xml",
                            Logfiles.Application, LogLevel.Exception);
                        return false;
                    }
                    //download document from string
                    using (WebClient client = new WebClient())
                    {
                        modInfoXml = client.DownloadString(downloadURL);
                    }
                    break;
                case DatabaseVersions.Test:
                    //make string
                    string modInfoFilePath = ModpackSettings.CustomModInfoPath;
                    if(string.IsNullOrWhiteSpace(modInfoFilePath))
                    {
                        modInfoFilePath = System.IO.Path.Combine(Settings.ApplicationStartupPath, "modInfo.xml");
                    }
                    //load modinfo xml
                    if (System.IO.File.Exists(modInfoFilePath))
                        modInfoXml = System.IO.File.ReadAllText(modInfoFilePath);
                    else
                    {
                        Logging.WriteToLog("modInfo.xml does not exist at " + modInfoFilePath, Logfiles.Application, LogLevel.Error);
                        return false;
                    }
                    break;
            }
            if (string.IsNullOrWhiteSpace(modInfoXml))
            {
                Logging.WriteToLog("Failed to read modInfoxml xml string", Logfiles.Application, LogLevel.Exception);
                MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                return false;
            }
            try
            {
                modInfoDocument.LoadXml(modInfoXml);
            }
            catch (XmlException ex)
            {
                Logging.WriteToLog("Failed to parse modInfoxml from xml string\n" + ex.ToString(), Logfiles.Application, LogLevel.Exception);
                MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " modInfo.xml");
                return false;
            }
            //if not stable db, update current version and online folder version from modInfoxml itself
            if(ModpackSettings.DatabaseDistroVersion != DatabaseVersions.Stable)
            {
                Settings.WoTModpackOnlineFolderVersion = XMLUtils.GetXMLStringFromXPath(modInfoDocument, "//modInfoAlpha.xml@onlineFolder");
                Settings.WoTClientVersion = XMLUtils.GetXMLStringFromXPath(modInfoDocument, "//modInfoAlpha.xml@version");
            }
            //parse the modInfoXml to list in memory

            //check db cache of local files
            loadProgress.ChildProgressCurrent++;
            loadProgress.ReportMessage = Translations.GetTranslatedString("verifyingDownloadCache");
            progress.Report(loadProgress);

            //build UI
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
