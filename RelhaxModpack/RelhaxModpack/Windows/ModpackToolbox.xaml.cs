using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net;
using System.IO;
using Microsoft.Win32;
using System.Xml;
using RelhaxModpack.Utilities;
using RelhaxModpack.Xml;
using RelhaxModpack.UI;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Settings;
using RelhaxModpack.Common;
using Ionic.Zip;
using System.Xml.Linq;
using System.Xml.XPath;
using RelhaxModpack.Patching;
using RelhaxModpack.Atlases;
using RelhaxModpack.Shortcuts;
using RelhaxModpack.Utilities.Structs;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseUpdater.xaml
    /// </summary>
    public partial class ModpackToolbox : RelhaxCustomFeatureWindow
    {
        /// <summary>
        /// Allows the old and new versions of a SelectablePackage to be saved temporarily for comparing differences between two database structures
        /// </summary>
        public struct DatabaseBeforeAfter
        {
            /// <summary>
            /// The package reference for the database before changes
            /// </summary>
            public SelectablePackage Before;

            /// <summary>
            /// The package reference for the database after changes
            /// </summary>
            public SelectablePackage After;
        }

        /// <summary>
        /// Allows the old and new versions of a DatabasePackage to be saved temporarily for comparing differences between two database structures
        /// </summary>
        public struct DatabaseBeforeAfter2
        {
            /// <summary>
            /// The package reference for the database before changes
            /// </summary>
            public DatabasePackage Before;

            /// <summary>
            /// The package reference for the database after changes
            /// </summary>
            public DatabasePackage After;
        }

        /// <summary>
        /// The command line argument specified at application launch to show this window
        /// </summary>
        public const string CommandLineArg = "modpack-toolbox";

        /// <summary>
        /// The name of the logfile
        /// </summary>
        public const string LoggingFilename = "RelhaxToolbox.log";

        private const string DatabaseUpdateFilename = "databaseUpdate.txt";
        private const string TrashXML = "trash.xml";
        private const string DatabaseXml = "database.xml";
        private const string MissingPackagesTxt = "missingPackages.txt";
        private const string RepoResourcesFolder = "resources";
        private const string RepoLatestDatabaseFolder = "latest_database";
        private const string UpdaterErrorExceptionCatcherLogfile = "UpdaterErrorCatcher.log";
        private const string InstallStatisticsXml = "install_statistics.xml";
        private const string TranslationsCsv = "translations.csv";
        private const string AutomationElementsTxt = "AutomationElements.txt";
        private const string LastInstructionConvertedPackageTxt = "lastInstructionConvertedPackage.txt";

        private string DatabaseUpdatePath
        {
            get { return Path.Combine(ToolboxSettings.UseCustomDbPath ? ToolboxSettings.CustomDbPath : ApplicationConstants.ApplicationStartupPath, RepoResourcesFolder, DatabaseUpdateFilename); }
        }

        private string SupportedClientsPath
        {
            get { return Path.Combine(ToolboxSettings.UseCustomDbPath ? ToolboxSettings.CustomDbPath : ApplicationConstants.ApplicationStartupPath, RepoResourcesFolder, ApplicationConstants.SupportedClients); }
        }

        private string ManagerVersionPath
        {
            get { return Path.Combine(ToolboxSettings.UseCustomDbPath ? ToolboxSettings.CustomDbPath : ApplicationConstants.ApplicationStartupPath, RepoResourcesFolder, ApplicationConstants.ManagerVersion); }
        }

        private string RepoLatestDatabaseFolderPath
        {
            get { return Path.Combine(ToolboxSettings.UseCustomDbPath ? ToolboxSettings.CustomDbPath : ApplicationConstants.ApplicationStartupPath, RepoLatestDatabaseFolder); }
        }

        private ModpackToolboxSettings ToolboxSettings = new ModpackToolboxSettings();
        private string KeyFilename = "key.txt"; //can be overridden by command line argument
        private WebClient client;
        private bool authorized = false;
        private string WoTModpackOnlineFolderVersion;
        private string WoTClientVersion;
        //open
        private OpenFileDialog SelectModInfo = new OpenFileDialog() { Filter = "*.xml|*.xml" };
        private OpenFileDialog SelectV2Application = new OpenFileDialog() { Title = "Find V2 application to upload", Filter = "*.exe|*.exe" };
        private OpenFileDialog SelectManagerInfoXml = new OpenFileDialog() { Title = "Find " + ApplicationConstants.ManagerVersion, Filter = ApplicationConstants.ManagerVersion + "|" + ApplicationConstants.ManagerVersion };
        private OpenFileDialog SelectSupportedClientsXml = new OpenFileDialog() { Title = "Find " + ApplicationConstants.SupportedClients, Filter = ApplicationConstants.SupportedClients + "|" + ApplicationConstants.SupportedClients};
        //save
        private SaveFileDialog SelectModInfoSave = new SaveFileDialog() { Filter = "*.xml|*.xml" };

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckAuthorization();
            loading = false;
        }

        #region Password auth stuff
        private async Task CheckAuthorization()
        {
            //check if key filename was changed from command line
            if (!string.IsNullOrWhiteSpace(CommandLineSettings.UpdateKeyFileName))
            {
                Logging.Updater("User specified from command line new key filename to use: {0}", LogLevel.Info, CommandLineSettings.UpdateKeyFileName);
                KeyFilename = CommandLineSettings.UpdateKeyFileName;
            }
            if (File.Exists(KeyFilename))
            {
                Logging.Updater("File for auth exists, attempting authorization");
                Logging.Updater(KeyFilename);
                await AttemptAuthFromFile(KeyFilename);
            }
            else
            {
                Logging.Updater("Loading without pre-file authorization");
            }
        }

        private async void LoadPasswordFromText_Click(object sender, RoutedEventArgs e)
        {
            await AttemptAuthFromString(PaswordTextbox.Text);
        }

        private async Task AttemptAuthFromFile(string filepath)
        {
            Logging.Updater("Attempting authorization from file {0}", LogLevel.Info, filepath);
            await AttemptAuthFromString(File.ReadAllText(filepath));
        }

        private async Task AttemptAuthFromString(string key)
        {
            AuthStatusTextblock.Text = "Current status: Checking...";
            AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Orange);

            //if the user is already authorized, then no reason to check again
            if(authorized)
            {
                Logging.Updater("User is already authorized");
                AuthStatusTextblock.Text = "Current status: Authorized";
                AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Green);
                authorized = true;
                return;
            }

            //disable the buttons
            LoadPasswordFromFileButton.IsEnabled = false;
            LoadPasswordFromTextButton.IsEnabled = false;

            //compare local password to online version
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredentialPrivate })
            {
                string onlinePassword = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsModpackUpdaterKey);
                if (onlinePassword.Equals(key))
                {
                    Logging.Updater("Authorized, keys match");
                    AuthStatusTextblock.Text = "Current status: Authorized";
                    AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Green);
                    authorized = true;
                }
                else
                {
                    Logging.Updater("Not authorized, keys do not match");
                    AuthStatusTextblock.Text = "Current status: Denied";
                    AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Red);
                    authorized = false;
                }
            }

            //enable the buttons again
            LoadPasswordFromFileButton.IsEnabled = true;
            LoadPasswordFromTextButton.IsEnabled = true;
        }
        #endregion

        #region Standard class stuff
        private bool loading = true;
        /// <summary>
        /// Create an instance of the DatabaseUpdater window
        /// </summary>
        public ModpackToolbox(ModpackSettings modpackSettings, Logfiles logfile) : base(modpackSettings, logfile)
        {
            InitializeComponent();
            Settings = ToolboxSettings;
        }

        private void OnApplicationClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if strings are not empty and file exists, delete them
            //for all the class level strings
            Logging.Updater("Deleting trash files...");
            string[] filesToDelete = new string[]
            {
                DatabaseXml,
                TrashXML,
                MissingPackagesTxt
            };
            foreach (string s in filesToDelete)
            {
                if (!string.IsNullOrWhiteSpace(s) && File.Exists(s))
                    File.Delete(s);
            }
            Logging.DisposeLogging(Logfiles.Updater);
        }
        #endregion

        #region UI Interaction methods
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!authorized && !loading)
            {
                //only MD5 and database output are allowed
                if(GenerateHashesTab.IsSelected || DatabaseOutputTab.IsSelected)
                {
                    //don't do anything
                }
                else
                {
                    ReportProgress("You are not authorized to use this tab");
                    AuthStatusTab.IsSelected = true;
                    AuthStatusTab.Focus();
                }
            }
        }

        private void LogOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogOutput.ScrollToEnd();
        }

        private void CancelDownloadButon_Click(object sender, RoutedEventArgs e)
        {
            client.CancelAsync();
        }

        private void PaswordTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoadPasswordFromText_Click(null, null);
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUILog();
        }

        private async void LoadPasswordFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openPassword = new OpenFileDialog()
            {
                Title = "Locate password text file",
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                //Office Files|*.doc;*.xls;*.ppt
                Filter = "Text Files|*.txt",
                Multiselect = false
            };
            if ((bool)openPassword.ShowDialog())
            {
                await AttemptAuthFromFile(openPassword.FileName);
            }
        }
        #endregion

        #region Util methods
        private async Task<XmlDocument> ParseVersionInfoXmlDoc(string pathToSupportedClients)
        {
            XmlDocument doc = null;
            if (string.IsNullOrEmpty(pathToSupportedClients))
            {
                ReportProgress("Loading supported clients from online");
                using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
                {
                    string xml = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsFTPModpackManager + ApplicationConstants.SupportedClients);
                    doc = XmlUtils.LoadXmlDocument(xml, XmlLoadType.FromString);
                }
            }
            else
            {
                ReportProgress("Loading supported clients document from " + pathToSupportedClients);
                doc = XmlUtils.LoadXmlDocument(SelectSupportedClientsXml.FileName, XmlLoadType.FromFile);
            }

            return doc;
        }

        private async Task<List<VersionInfos>> ParseVersionInfoXml(string pathToSupportedClients)
        {
            List<VersionInfos> versionInfosList = new List<VersionInfos>();
            ReportProgress("Loading and parsing " + ApplicationConstants.SupportedClients);

            //load xml document
            XmlDocument doc = await ParseVersionInfoXmlDoc(pathToSupportedClients);

            //parse each online folder to list type string
            ReportProgress("Parsing " + ApplicationConstants.SupportedClients);
            XmlNodeList supportedClients = XmlUtils.GetXmlNodesFromXPath(doc, "//versions/version");
            foreach (XmlNode node in supportedClients)
            {
                VersionInfos newVersionInfo = new VersionInfos()
                {
                    WoTOnlineFolderVersion = node.Attributes["folder"].Value,
                    WoTClientVersion = node.InnerText
                };
                versionInfosList.Add(newVersionInfo);
            }
            return versionInfosList;
        }

        private void OnLoadModInfo(object sender, RoutedEventArgs e)
        {
            OnLoadModInfo();
        }

        private bool OnLoadModInfo()
        {
            if ((bool)SelectModInfo.ShowDialog())
            {
                LogOutput.Text = "Loading database...";
                //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
                //for the folder version: //modInfoAlpha.xml/@version
                WoTModpackOnlineFolderVersion = XmlUtils.GetXmlStringFromXPath(SelectModInfo.FileName, ApplicationConstants.DatabaseOnlineFolderXpath);
                WoTClientVersion = XmlUtils.GetXmlStringFromXPath(SelectModInfo.FileName, ApplicationConstants.DatabaseClientVersionXpath);
                string versionInfo = string.Format("{0} = {1},  {2} = {3}", nameof(WoTModpackOnlineFolderVersion), WoTModpackOnlineFolderVersion, nameof(WoTClientVersion), WoTClientVersion);
                ReportProgress(versionInfo);
                ReportProgress("Database loaded");
                return true;
            }
            else
            {
                ReportProgress("Canceled loading database");
                return false;
            }
        }

        private void ReportProgress(string message)
        {
            //reports to the log file and the console output
            Logging.Updater(message);
            LogOutput.AppendText(message + "\n");
        }

        private async Task RunPhpScript(NetworkCredential credentials, string URL, int timeoutMilliseconds)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            using (client = new PatientWebClient()
            { Credentials = credentials, Timeout = timeoutMilliseconds })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync(URL);
                    ReportProgress(result.Replace("<br />", "\n"));
                    ToggleUI((TabController.SelectedItem as TabItem), true);
                }
                catch (WebException wex)
                {
                    ReportProgress("failed to run script");
                    ReportProgress(wex.ToString());
                    ToggleUI((TabController.SelectedItem as TabItem), true);
                }
            }
        }

        private async void GenerateMD5Button_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Starting hashing");
            OpenFileDialog zipsToHash = new OpenFileDialog()
            {
                DefaultExt = "zip",
                Filter = "*.zip|*.zip",
                Multiselect = true,
                Title = "Load zip files to hash"
            };
            if (!(bool)zipsToHash.ShowDialog())
            {
                ReportProgress("Hashing Aborted");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            foreach (string s in zipsToHash.FileNames)
            {
                ReportProgress(string.Format("hash of {0}:", Path.GetFileName(s)));
                ReportProgress(await FileUtils.CreateMD5HashAsync(s));
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void ClearUILog()
        {
            LogOutput.Clear();
            ReportProgress("Log Cleared");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void ToggleUI(TabItem tab, bool toggle)
        {
            foreach (FrameworkElement control in UiUtils.GetAllWindowComponentsLogical(tab, false))
            {
                if (control is Button butt)
                    butt.IsEnabled = toggle;
            }
            SetProgress(JobProgressBar.Minimum);
        }

        private void SetProgress(double prog)
        {
            JobProgressBar.Value = prog;
        }

        private async Task<DatabaseManager> LoadDatabase1V1FromBigmods(string lastWoTClientVersion)
        {
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                DatabaseManager databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);
                string databaseFtpPath = string.Format("{0}{1}/", PrivateStuff.BigmodsFTPModpackDatabase, lastWoTClientVersion);

                ReportProgress(string.Format("FTP path parsed as {0}", databaseFtpPath));
                ReportProgress("Downloading documents");
                ReportProgress("Download root document");

                string rootDatabase = await client.DownloadStringTaskAsync(databaseFtpPath + "database.xml");
                XmlDocument root1V1Document = XmlUtils.LoadXmlDocument(rootDatabase, XmlLoadType.FromString);

                ReportProgress("Downloading globalDependencies document");
                string globalDependenciesStrings1V1 = await client.DownloadStringTaskAsync(databaseFtpPath + XmlUtils.GetXmlStringFromXPath(root1V1Document, "/modInfoAlpha.xml/globalDependencies/@file"));

                ReportProgress("Downloading dependencies document");
                string dependneciesStrings1V1 = await client.DownloadStringTaskAsync(databaseFtpPath + XmlUtils.GetXmlStringFromXPath(root1V1Document, "/modInfoAlpha.xml/dependencies/@file"));

                List<string> categoriesStrings1V1 = new List<string>();
                foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(root1V1Document, "//modInfoAlpha.xml/categories/category"))
                {
                    ReportProgress(string.Format("Downloading category {0}", categoryNode.Attributes["file"].Value));
                    categoriesStrings1V1.Add(await client.DownloadStringTaskAsync(databaseFtpPath + categoryNode.Attributes["file"].Value));
                }

                ReportProgress("Parsing to lists");
                DatabaseLoadFailCode code = databaseManager.LoadDatabaseCustomFromStringsAsync(root1V1Document, globalDependenciesStrings1V1, dependneciesStrings1V1, categoriesStrings1V1);
                if (code != DatabaseLoadFailCode.None)
                    return null;

                return databaseManager;
            }
        }

        private async void RunPhpScriptCreateModInfo(object sender, RoutedEventArgs e)
        {
            ReportProgress("Starting update database step 5");
            ReportProgress("Running script to create mod info data file(s)");
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateModInfoPHP, 100000);
        }

        private async void RunPhpScriptCreateManagerInfo(object sender, RoutedEventArgs e)
        {
            ReportProgress("Starting Update database step 6");
            ReportProgress("Running script to create manager info data file");
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateManagerInfoPHP, 100000);
        }
        #endregion

        #region Database output
        private async void SaveDatabaseText(bool @internal)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);

            //true = internal, false = user
            string notApplicable = "n/a";

            DatabaseManager saveDatabaseTextManager = new DatabaseManager(ModpackSettings, CommandLineSettings);

            await saveDatabaseTextManager.LoadDatabaseTestAsync(SelectModInfo.FileName);

            //create variables
            StringBuilder sb = new StringBuilder();
            string saveLocation = @internal ? Path.Combine(ApplicationConstants.ApplicationStartupPath, "database_internal.csv") :
                Path.Combine(ApplicationConstants.ApplicationStartupPath, "database_user.csv");

            //global dependencies
            string header = @internal ? "PackageName\tCategory\tPackage\tLevel\tZip\tTags\tDeprecated\tDevURL\tEnabled\tVisible\tVersion" : "Category\tMod\tDevURL";
            sb.AppendLine(header);

            if(@internal)
            {
                foreach (DatabasePackage dp in saveDatabaseTextManager.GlobalDependencies)
                {
                    sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                        dp.PackageName,
                        "GlobalDependencies",
                        string.Empty,
                        "0",
                        string.IsNullOrWhiteSpace(dp.ZipFile) ? notApplicable : dp.ZipFile,
                        dp.Tags.ToString(),
                        dp.Deprecated.ToString(),
                        //example google url: =HYPERLINK("http://wwiihwa.blogspot.de/","link")
                        string.IsNullOrWhiteSpace(dp.DevURLList[0].Trim()) ? string.Empty : "=HYPERLINK(\"" + dp.DevURLList[0].Trim() + "\",\"link\")",
                        dp.Enabled,
                        string.Empty,
                        dp.Version
                        ));
                }
                foreach (Dependency dep in saveDatabaseTextManager.Dependencies)
                {
                    sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                        dep.PackageName,
                        "Dependencies",
                        string.Empty,
                        "0",
                        string.IsNullOrWhiteSpace(dep.ZipFile) ? notApplicable : dep.ZipFile,
                        dep.Tags.ToString(),
                        dep.Deprecated.ToString(),
                        string.IsNullOrWhiteSpace(dep.DevURLList[0].Trim()) ? string.Empty : "=HYPERLINK(\"" + dep.DevURLList[0].Trim() + "\",\"link\")",
                        dep.Enabled,
                        string.Empty,
                        dep.Version));
                }
            }

            foreach (Category cat in saveDatabaseTextManager.ParsedCategoryList)
            {
                List<SelectablePackage> flatlist = cat.GetFlatPackageList();
                foreach (SelectablePackage sp in flatlist)
                {
                    
                    if (!@internal)
                    {
                        string nameIndneted = sp.NameFormatted;
                        for (int i = 0; i < sp.Level; i++)
                        {
                            nameIndneted = "--" + nameIndneted;
                        }
                        sb.AppendLine(string.Format("{0}\t{1}\t{2}", sp.ParentCategory.Name, nameIndneted,
                            string.IsNullOrWhiteSpace(sp.DevURLList[0].Trim()) ? string.Empty : "=HYPERLINK(\"" + sp.DevURLList[0].Trim() + "\",\"link\")"));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                            sp.PackageName,
                            sp.ParentCategory.Name,
                            sp.NameFormatted,
                            sp.Level,
                            string.IsNullOrWhiteSpace(sp.ZipFile) ? notApplicable : sp.ZipFile,
                            sp.Tags.ToString(),
                            sp.Deprecated.ToString(),
                            string.IsNullOrWhiteSpace(sp.DevURLList[0].Trim()) ? string.Empty : "=HYPERLINK(\"" + sp.DevURLList[0].Trim() + "\",\"link\")",
                            sp.Enabled,
                            sp.Visible,
                            sp.Version));
                    }
                }
            }

            try
            {
                File.WriteAllText(saveLocation, sb.ToString());
                ReportProgress("Saved in " + saveLocation);
                ToggleUI((TabController.SelectedItem as TabItem), true);
            }
            catch (IOException)
            {
                ReportProgress("Failed to save in " + saveLocation + " (IOException, probably file open in another window)");
                ToggleUI((TabController.SelectedItem as TabItem), true);
            }
        }

        private void DatabaseOutputStep2a_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Generation of internal csv...");

            //check
            if (string.IsNullOrEmpty(WoTClientVersion) || string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("Database not loaded");
                return;
            }

            SaveDatabaseText(true);
        }

        private void DatabaseOutputStep2b_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Generation of user csv...");

            //check
            if (string.IsNullOrEmpty(WoTClientVersion) || string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("Database not loaded");
                return;
            }

            SaveDatabaseText(false);
        }
        #endregion
        
        #region Application update V2
        private async void UpdateApplicationV2UploadApplicationStable(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Locate stable V2 application");
            if (!(bool)SelectV2Application.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                ReportProgress("Canceled");
                return;
            }
            ReportProgress("Located");

            //check if it's the correct file to upload
            if (!Path.GetFileName(SelectV2Application.FileName).Equals(ApplicationConstants.ApplicationFilenameStable))
            {
                if (MessageBox.Show(string.Format("The file selected is not {0}, are you sure you selected the correct file?",
                    ApplicationConstants.ApplicationFilenameStable), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    ToggleUI((TabController.SelectedItem as TabItem), true);
                    ReportProgress("Canceled");
                    return;
                }
            }

            ReportProgress("Uploading stable V2 application to bigmods...");
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackRelhaxModpack + ApplicationConstants.ApplicationFilenameStable, SelectV2Application.FileName);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateApplicationV2UploadApplicationBeta(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Locate beta V2 application");
            if (!(bool)SelectV2Application.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                ReportProgress("Canceled");
                return;
            }
            ReportProgress("Located");

            //check if it's the correct file to upload
            if (!Path.GetFileName(SelectV2Application.FileName).Equals(ApplicationConstants.ApplicationFilenameBeta))
            {
                if (MessageBox.Show(string.Format("The file selected is not {0}, are you sure you selected the correct file?",
                    ApplicationConstants.ApplicationFilenameBeta), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    ToggleUI((TabController.SelectedItem as TabItem), true);
                    ReportProgress("Canceled");
                    return;
                }
            }

            ReportProgress("Uploading beta V2 application to bigmods...");
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackRelhaxModpack + ApplicationConstants.ApplicationFilenameBeta, SelectV2Application.FileName);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateApplicationV2UploadManagerVersion(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress(string.Format("Running upload {0} to bigmods", ApplicationConstants.ManagerVersion));
            if (!(bool)SelectManagerInfoXml.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Upload " + ApplicationConstants.ManagerVersion);
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackManager + Path.GetFileName(SelectManagerInfoXml.FileName), SelectManagerInfoXml.FileName);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateApplicationV2CreateUpdatePackages(object sender, RoutedEventArgs e)
        {
            ReportProgress("Running script to create update packages (bigmods)...");
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateUpdatePackagesPHP, 100000);
        }

        private async void UpdateApplicationV2CreateManagerInfoBigmods(object sender, RoutedEventArgs e)
        {
            ReportProgress("Running script to create manager info (bigmods)...");
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateManagerInfoPHP, 30 * CommonUtils.TO_SECONDS);
        }
        #endregion

        #region Cleaning online folders
        List<VersionInfos> VersionInfosListClean;
        VersionInfos selectedVersionInfos;
        bool cancelDelete = false;

        private async void CleanFoldersOnlineStep1_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running Clean online folders step 1");

            VersionInfosListClean = await ParseVersionInfoXml(string.Empty);

            CleanFoldersOnlineStep2b.Items.Clear();
            foreach(VersionInfos vi in VersionInfosListClean)
                CleanFoldersOnlineStep2b.Items.Add(vi);

            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void CleanFoldersOnlineStep2_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running Clean Folders online step 2");
            if(CleanFoldersOnlineStep2b.Items.Count == 0)
            {
                ReportProgress("Combobox items count = 0");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if(VersionInfosListClean == null)
            {
                ReportProgress("VersionsInfoList == null");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if(CleanFoldersOnlineStep2b.SelectedItem == null)
            {
                ReportProgress("Item not selected");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if(VersionInfosListClean.Count == 0)
            {
                ReportProgress("VersionsInfosList count = 0");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            selectedVersionInfos = (VersionInfos)CleanFoldersOnlineStep2b.SelectedItem;
            ReportProgress("Getting all trash files in online folder " + selectedVersionInfos.WoTOnlineFolderVersion);

            //make a new list where it only has versions who's online folder match the selected one from the combobox
            List<VersionInfos> specificVersions = VersionInfosListClean.Where(info => info.WoTOnlineFolderVersion.Equals(selectedVersionInfos.WoTOnlineFolderVersion)).ToList();
            List<string> allUsedZipFiles = new List<string>();

            //could be multiple branches on github database
            ReportProgress("Getting list of branches");

            List<string> branches = await CommonUtils.GetListOfGithubRepoBranchesAsync(ApplicationConstants.BetaDatabaseBranchesURL);

            ReportProgress(string.Join(",", branches));

            foreach (string branch in branches)
            {
                specificVersions.Add(new VersionInfos { WoTClientVersion = "GITHUB," + branch });
            }

            foreach(VersionInfos infos in specificVersions)
            {
                ReportProgress("Adding zip files from WoTClientVersion " + infos.WoTClientVersion);
                DatabaseManager databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);
                List<DatabasePackage> flatList = new List<DatabasePackage>();

                //download and parse database to flat list
                if (infos.WoTClientVersion.Contains("GITHUB"))
                {
                    //get branch name
                    string branchName = infos.WoTClientVersion.Split(',')[1].Trim();
                    await databaseManager.LoadDatabaseAsync(DatabaseVersions.Beta, null, branchName);

                    //parse xml document for online folder version
                    string betaDatabaseOnlineFolderVersion = databaseManager.WoTOnlineFolderVersion;

                    ReportProgress(string.Format("GITHUB branch = {0}, online folder={1}, selected online folder to clean version={2}", branchName, betaDatabaseOnlineFolderVersion, selectedVersionInfos.WoTOnlineFolderVersion));

                    if (!betaDatabaseOnlineFolderVersion.Equals(selectedVersionInfos.WoTOnlineFolderVersion))
                    {
                        ReportProgress("Skipping (online folders are not equal)");
                        continue;
                    }
                }
                else
                {
                    await databaseManager.LoadDatabaseStableSpecificClientAsync(infos.WoTClientVersion);
                }

                //if the list of zip files does not already have it, then add it
                flatList = databaseManager.GetFlatList();
                foreach (DatabasePackage package in flatList)
                    if(!string.IsNullOrWhiteSpace(package.ZipFile) && !allUsedZipFiles.Contains(package.ZipFile))
                            allUsedZipFiles.Add(package.ZipFile);
            }

            //just a check
            allUsedZipFiles = allUsedZipFiles.Distinct().ToList();

            //have a list of ALL used zip files in the folder, now get the list of all zip files in the onlineFolder
            ReportProgress("Complete, getting database.xml in onlineFolder " + selectedVersionInfos.WoTOnlineFolderVersion);
            List<string> notUsedFiles = new List<string>();
            List<string> missingFiles = new List<string>();
            List<string> filesFromDatabaseXml = new List<string>();
            using (client = new WebClient())
            {
                XmlDocument filesInOnlineFolder = new XmlDocument();
                string downlaodUrlString = string.Format("https://bigmods.relhaxmodpack.com/WoT/{0}/{1}", selectedVersionInfos.WoTOnlineFolderVersion, DatabaseXml);
                ReportProgress("Downloading from " + downlaodUrlString);
                filesInOnlineFolder.LoadXml(await client.DownloadStringTaskAsync(downlaodUrlString));
                foreach(XmlNode node in filesInOnlineFolder.SelectNodes("//database/file"))
                {
                    filesFromDatabaseXml.Add(node.Attributes["name"].Value);
                }
            }
            filesFromDatabaseXml = filesFromDatabaseXml.Distinct().ToList();
            ReportProgress("Complete, building lists");
            notUsedFiles = filesFromDatabaseXml.Except(allUsedZipFiles).ToList();
            missingFiles = allUsedZipFiles.Except(filesFromDatabaseXml).ToList();
            CleanZipFoldersTextbox.Clear();
            if (missingFiles.Count > 0)
            {
                ReportProgress("ERROR: missing files on the server! (Saved in textbox)");
                CleanZipFoldersTextbox.AppendText(string.Join("\n", missingFiles));
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            else if(notUsedFiles.Count == 0)
            {
                ReportProgress("No files to clean!");
                ToggleUI((TabController.SelectedItem as TabItem), true);
            }
            else
            {
                CleanZipFoldersTextbox.AppendText(string.Join("\n", notUsedFiles));
                ReportProgress("Complete");
                ToggleUI((TabController.SelectedItem as TabItem), true);
            }
        }

        private void CleanFoldersOnlineCancel_Click(object sender, RoutedEventArgs e)
        {
            cancelDelete = true;
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void CleanFoldersOnlineStep3_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            if (string.IsNullOrWhiteSpace(CleanZipFoldersTextbox.Text))
            {
                ReportProgress("textbox is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if(string.IsNullOrWhiteSpace(selectedVersionInfos.WoTOnlineFolderVersion))
            {
                ReportProgress("selectedVersionInfos is null");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            cancelDelete = false;
            CleanFoldersOnlineCancelStep3.Visibility = Visibility.Visible;
            CleanFoldersOnlineCancelStep3.IsEnabled = true;
            List<string> filesToDelete;
            filesToDelete = CleanZipFoldersTextbox.Text.Split('\n').ToList();
            string[] filesActuallyInFolder = await FtpUtils.FtpListFilesFoldersAsync(
                PrivateStuff.BigmodsFTPRootWoT + selectedVersionInfos.WoTOnlineFolderVersion,PrivateStuff.BigmodsNetworkCredential);
            int count = 1;
            foreach(string s in filesToDelete)
            {
                if(cancelDelete)
                {
                    ReportProgress("Cancel Requested");
                    ToggleUI((TabController.SelectedItem as TabItem), true);
                    return;
                }
                if(!filesActuallyInFolder.Contains(s))
                {
                    ReportProgress(string.Format("Skipping file {0}, does not exist", s));
                    count++;
                    continue;
                }
                ReportProgress(string.Format("Deleting file {0} of {1}, {2}", count++, filesToDelete.Count, s));
                try
                {
                    await FtpUtils.FtpDeleteFileAsync(string.Format("{0}{1}/{2}",
                        PrivateStuff.BigmodsFTPRootWoT, selectedVersionInfos.WoTOnlineFolderVersion, s), PrivateStuff.BigmodsNetworkCredential);
                }
                catch
                {
                    ReportProgress("Failed to delete file");
                }
            }
            CleanZipFoldersTextbox.Clear();
            CleanFoldersOnlineCancelStep3.Visibility = Visibility.Hidden;
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void CleanFoldersCancel_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Stop requested, canceling operation");
            client.CancelAsync();
        }
        #endregion

        #region Database Updating V2
        //the version number of the last supported WoT client, used for making backup online folder
        private string LastSupportedTanksVersion = "";

        private async void UpdateDatabaseV2Step2_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Starting Update database step 2...");
            ReportProgress("Running script to update online hash database...");
            //a PatientWebClient should allow a timeout value of 5 mins (or more)
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateDatabasePHP, 30 * CommonUtils.TO_SECONDS * CommonUtils.TO_MINUTES);
        }

        private async void UpdateDatabaseV2Step3_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Starting DatabaseUpdate step 3");
            ReportProgress("Preparing database update");

            //checks
            if (string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("WoTModpackOnlineFolderVersion is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if (!File.Exists(SelectModInfo.FileName))
            {
                ReportProgress("SelectModInfo file selected does not exist:" + SelectModInfo.FileName);
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //init stringBuilders
            StringBuilder filesNotFoundSB = new StringBuilder();
            StringBuilder databaseUpdateText = new StringBuilder();
            filesNotFoundSB.Append("FILES NOT FOUND:\n");

            //init lists
            List<DatabasePackage> addedPackages = new List<DatabasePackage>();
            List<DatabasePackage> updatedPackages = new List<DatabasePackage>();
            List<DatabasePackage> disabledPackages = new List<DatabasePackage>();
            List<DatabasePackage> removedPackages = new List<DatabasePackage>();
            List<DatabasePackage> missingPackages = new List<DatabasePackage>();
            List<DatabaseBeforeAfter> renamedPackages = new List<DatabaseBeforeAfter>();
            List<DatabaseBeforeAfter> internallyRenamed = new List<DatabaseBeforeAfter>();
            List<DatabaseBeforeAfter> movedPackages = new List<DatabaseBeforeAfter>();

            //database manager
            DatabaseManager currentDatabaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);

            //init strings
            LastSupportedTanksVersion = string.Empty;
            SetProgress(10);

            ReportProgress("Loading Root database 1.1 document");
            DatabaseLoadFailCode failCode = await currentDatabaseManager.LoadDatabaseTestAsync(SelectModInfo.FileName);
            if (failCode != DatabaseLoadFailCode.None)
            {
                ReportProgress("Failed to parse database");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            SetProgress(20);

            List<DatabasePackage> flatListCurrent = currentDatabaseManager.GetFlatList();

            //check for duplicates
            ReportProgress("Checking for duplicate database packageName entries");
            List<string> duplicates = currentDatabaseManager.CheckForDuplicatePackageNamesStringsList();
            if (duplicates.Count > 0)
            {
                ReportProgress("ERROR: Duplicates found!");
                foreach (string s in duplicates)
                    ReportProgress(s);
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Checking for duplicate database UID entries");
            List<DatabasePackage> duplicatesList = currentDatabaseManager.CheckForDuplicateUIDsPackageList();
            if (duplicatesList.Count > 0)
            {
                ReportProgress("ERROR: The following packages are duplicate UIDs:");
                foreach (DatabasePackage package in duplicatesList)
                    ReportProgress(string.Format("PackageName: {0}, UID: {1}", package.PackageName, package.UID));
            }

            ReportProgress("No duplicates found");

            SetProgress(30);

            //make the name of the new database update version
            ReportProgress("Making new database version string");
            string dateTimeFormat = string.Format("{0:yyyy-MM-dd}", DateTime.Now);

            //load manager version xml file
            XmlDocument doc = XmlUtils.LoadXmlDocument(ManagerVersionPath, XmlLoadType.FromFile);
            XmlNode database_version_text = doc.SelectSingleNode("//version/database");

            //database update text is like this: <WoTVersion>_<Date>_<itteration>
            //only update the iteration if the WoT version and date match
            string lastWoTClientVersion = database_version_text.InnerText.Split('_')[0];
            string lastDate = database_version_text.InnerText.Split('_')[1];

            ReportProgress(string.Format("lastWoTClientVersion    = {0}", lastWoTClientVersion));
            ReportProgress(string.Format("currentWoTClientVersion = {0}", WoTClientVersion));
            ReportProgress(string.Format("lastDate                = {0}", lastDate));
            ReportProgress(string.Format("currentDate             = {0}", dateTimeFormat));

            string databaseVersionTag = string.Empty;

            if (lastWoTClientVersion.Equals(WoTClientVersion) && lastDate.Equals(dateTimeFormat))
            {
                ReportProgress("WoTVersion and date match, so incrementing the itteration");
                int lastItteration = int.Parse(database_version_text.InnerText.Split('_')[2]);
                databaseVersionTag = string.Format("{0}_{1}_{2}", WoTClientVersion, dateTimeFormat, ++lastItteration);
            }
            else
            {
                ReportProgress("lastWoTVersion and/or date NOT match, not incrementing the version (starts at 1)");
                databaseVersionTag = string.Format("{0}_{1}_1", WoTClientVersion, dateTimeFormat);
            }

            ReportProgress(string.Format("databaseVersionTag = {0}", databaseVersionTag));

            //update and save the manager_version.xml file
            database_version_text.InnerText = databaseVersionTag;
            doc.Save(ManagerVersionPath);

            SetProgress(40);

            //make a flat list of old (last supported modInfoxml) for comparison
            List<DatabasePackage> globalDependenciesOld;
            List<Dependency> dependenciesOld;
            List<Category> parsedCateogryListOld;

            //get last supported wot version for comparison
            ReportProgress("Getting last supported database files");
            LastSupportedTanksVersion = lastWoTClientVersion;

            //get strings for parsing
            DatabaseManager manager = await LoadDatabase1V1FromBigmods(lastWoTClientVersion);
            if (manager == null)
            {
                ReportProgress("Failed to parse modInfo to lists");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //build link references of old document
            manager.ProcessDatabase();
            globalDependenciesOld = manager.GlobalDependencies;
            dependenciesOld = manager.Dependencies;
            parsedCateogryListOld = manager.ParsedCategoryList;
            List<DatabasePackage> flatListOld = manager.GetFlatList();

            //check if any packages had a UID change, because this is not allowed
            //check based on packageName, loop through new
            //if it exists in old, make sure the UID did not change, else abort
            List<DatabaseBeforeAfter2> packagesWithChangedUIDs = new List<DatabaseBeforeAfter2>();
            foreach(DatabasePackage currentPackage in flatListCurrent)
            {
                DatabasePackage oldPackage = flatListOld.Find(pac => pac.PackageName.Equals(currentPackage.PackageName));
                if(oldPackage != null)
                {
                    if(!oldPackage.UID.Equals(currentPackage.UID))
                    {
                        packagesWithChangedUIDs.Add(new DatabaseBeforeAfter2() { Before = oldPackage, After = currentPackage });
                    }
                }
            }

            if(packagesWithChangedUIDs.Count > 0)
            {
                ReportProgress("A package had a UID change");
                foreach (DatabaseBeforeAfter2 beforeAfter in packagesWithChangedUIDs)
                {
                    ReportProgress(string.Format("Before package: PackageName = {0}, UID = {1}",beforeAfter.Before.PackageName, beforeAfter.Before.UID));
                    ReportProgress(string.Format("After package:  PackageName = {0}, UID = {1}", beforeAfter.After.PackageName, beforeAfter.After.UID));
                    string dialog = string.Format("Package {0} had a UID change:\nBefore: {1}\nAfter{2}\nIs this known?",
                        beforeAfter.Before.PackageName, beforeAfter.Before.UID, beforeAfter.After.UID);

                    if (MessageBox.Show(dialog, "Interesting", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        ToggleUI((TabController.SelectedItem as TabItem), true);
                        return;
                    }
                }
            }

            //check if any packages are missing UIDs
            List<DatabasePackage> packagesMissingUids = flatListCurrent.FindAll(pak => string.IsNullOrWhiteSpace(pak.UID));
            if(packagesMissingUids.Count > 0)
            {
                ReportProgress("ERROR: The following packages don't have UIDs and need to be added!");
                foreach (DatabasePackage package in packagesMissingUids)
                {
                    ReportProgress(string.Format("Package missing UID: {0}", package.PackageName));
                }
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //download and load latest zip file server database
            ReportProgress("Downloading database.xml of current WoT onlineFolder version from server");
            XmlDocument databaseXml = null;
            using (client = new WebClient())
            {
                string databaseXmlString = await client.DownloadStringTaskAsync(string.Format("https://bigmods.relhaxmodpack.com/WoT/{0}/{1}",
                    WoTModpackOnlineFolderVersion, DatabaseXml));
                databaseXml = XmlUtils.LoadXmlDocument(databaseXmlString, XmlLoadType.FromString);
            }

            SetProgress(50);

            //update the crc values, also makes list of updated mods
            ReportProgress("Downloaded, comparing crc values for list of updated mods");
            foreach (DatabasePackage package in flatListCurrent)
            {
                if (string.IsNullOrEmpty(package.ZipFile))
                    continue;
                //"//database/file[@name="Sounds_HRMOD_Gun_Sounds_by_Zorgane_v2.01_1.2.0_2018-10-12.zip"]"
                string xpathText = string.Format("//database/file[@name=\"{0}\"]", package.ZipFile);
                XmlNode databaseEntry = databaseXml.SelectSingleNode(xpathText);
                if (databaseEntry != null)
                {
                    string newCRC = databaseEntry.Attributes["md5"].Value;
                    if (string.IsNullOrWhiteSpace(newCRC))
                        throw new BadMemeException("newCRC string is null or whitespace");
                    if (!package.CRC.Equals(newCRC))
                    {
                        package.CRC = newCRC;
                        updatedPackages.Add(package);
                    }
                    //legacy compatibility check: size parameters need to be updated
                    ulong fakeSize = CommonUtils.ParseuLong(databaseEntry.Attributes["size"].Value, 0);
                    if (package.Size == 0 || fakeSize == 0 || package.Size != fakeSize)
                    {
                        //update the current size of the package
                        package.Size = fakeSize;
                        if (package.Size == 0)
                        {
                            Logging.Updater("Zip file {0} is 0 bytes (empty file)", LogLevel.Info, package.ZipFile);
                            return;
                        }
                    }
                }
                else if (package.CRC.Equals("f") || string.IsNullOrWhiteSpace(package.CRC))
                {
                    missingPackages.Add(package);
                }
            }

            SetProgress(80);

            //do list magic to get all added, removed, disabled, etc package lists
            //used for disabled, removed, added mods
            ReportProgress("Getting list of added and removed packages");
            PackageComparerByUID pc = new PackageComparerByUID();

            //if in before but not after = removed
            removedPackages = flatListOld.Except(flatListCurrent, pc).ToList();

            //if not in before but after = added
            addedPackages = flatListCurrent.Except(flatListOld, pc).ToList();

            ReportProgress("Getting list of packages old and new minus removed and added");

            //first start by getting the list of all current packages, then filter out removed and added packages
            //make a copy of the current flat list
            List<DatabasePackage> packagesNotRemovedOrAdded = new List<DatabasePackage>(flatListCurrent);

            //remove any added or removed packages from the new list to check for other stuff
            packagesNotRemovedOrAdded = packagesNotRemovedOrAdded.Except(removedPackages, pc).Except(addedPackages, pc).ToList();

            //we only want of type SelectablePackage 
            //https://stackoverflow.com/questions/3842714/linq-selection-by-type-of-an-object
            List<SelectablePackage> selectablePackagesNotRemovedOrAdded = packagesNotRemovedOrAdded.OfType<SelectablePackage>().ToList();

            //remove any added or removed packages from the new list to check for other stuff
            List<DatabasePackage> oldPackagesNotRemovedOrAdded = new List<DatabasePackage>(flatListOld);
            oldPackagesNotRemovedOrAdded = oldPackagesNotRemovedOrAdded.Except(removedPackages, pc).Except(addedPackages, pc).ToList();
            List<SelectablePackage> selectablePackagesOld = oldPackagesNotRemovedOrAdded.OfType<SelectablePackage>().ToList();

            //get the list of renamed packages
            //a renamed package will have the same internal name, but a different display name
            ReportProgress("Getting list of renamed packages");
            foreach (SelectablePackage selectablePackage in selectablePackagesNotRemovedOrAdded)
            {
                SelectablePackage oldPackageWithMatchingUID = selectablePackagesOld.Find(pack => pack.UID.Equals(selectablePackage.UID));
                if (oldPackageWithMatchingUID == null)
                    continue;

                if (!selectablePackage.NameFormatted.Equals(oldPackageWithMatchingUID.NameFormatted))
                {
                    Logging.Updater("Package rename-> old:{0}, new:{1}", LogLevel.Info, oldPackageWithMatchingUID.PackageName, selectablePackage.PackageName);
                    renamedPackages.Add(new DatabaseBeforeAfter() { Before = oldPackageWithMatchingUID, After = selectablePackage });
                }
            }

            //list of moved packages
            //a moved package will have a different UIDPath (the UID's don't change, so any change detected would imply a structure level change)
            ReportProgress("Getting list of moved packages");
            foreach (SelectablePackage selectablePackage in selectablePackagesNotRemovedOrAdded)
            {
                SelectablePackage oldPackageWithMatchingUID = selectablePackagesOld.Find(pack => pack.UID.Equals(selectablePackage.UID));
                if (oldPackageWithMatchingUID == null)
                    continue;

                if (!selectablePackage.CompleteUIDPath.Equals(oldPackageWithMatchingUID.CompleteUIDPath))
                {
                    Logging.Updater("Package moved: {0}", LogLevel.Info, selectablePackage.PackageName);
                    movedPackages.Add(new DatabaseBeforeAfter { Before = oldPackageWithMatchingUID, After = selectablePackage });
                }
            }

            SetProgress(85);

            //if a package was internally renamed, the packageName won't match
            ReportProgress("Getting list of internal renamed packages");
            foreach (SelectablePackage selectablePackage in selectablePackagesNotRemovedOrAdded)
            {
                SelectablePackage oldPackageWithMatchingUID = selectablePackagesOld.Find(pack => pack.UID.Equals(selectablePackage.UID));
                if (oldPackageWithMatchingUID == null)
                    continue;

                if (!selectablePackage.PackageName.Equals(oldPackageWithMatchingUID.PackageName))
                {
                    Logging.Updater("Package internal renamed: {0}", LogLevel.Info, selectablePackage.PackageName);
                    movedPackages.Add(new DatabaseBeforeAfter { Before = oldPackageWithMatchingUID, After = selectablePackage });
                }
            }
            
            SetProgress(90);

            ReportProgress("Getting list of disabled packages");
            //list of disabled packages before
            List<DatabasePackage> disabledBefore = oldPackagesNotRemovedOrAdded.Where(p => !p.Enabled).ToList();

            //list of disabled packages after
            List<DatabasePackage> disabledAfter = packagesNotRemovedOrAdded.Where(p => !p.Enabled).ToList();

            //compare except with after -> before
            disabledPackages = disabledAfter.Except(disabledBefore, pc).ToList();

            //any final list processing
            //also need to remove and removed and added and disabled from updated
            updatedPackages = updatedPackages.Except(removedPackages, pc).ToList();
            updatedPackages = updatedPackages.Except(disabledPackages, pc).ToList();
            updatedPackages = updatedPackages.Except(addedPackages, pc).ToList();
            //should remove any packages from the added list that were actually moved or renamed
            addedPackages = addedPackages.Except(movedPackages.Select(pack => pack.After)).ToList();
            addedPackages = addedPackages.Except(renamedPackages.Select(pack => pack.After)).ToList();
            //don't included any added packages that are disabled
            List<DatabasePackage> addedPackagesTemp = new List<DatabasePackage>();
            foreach(DatabasePackage package in addedPackages)
            {
                if (package is SelectablePackage sPack)
                {
                    if (sPack.Visible)
                        addedPackagesTemp.Add(package);
                }
                else
                    addedPackagesTemp.Add(package);
            }
            addedPackages = addedPackagesTemp;

            SetProgress(95);

            //put them to stringBuilder and write text to disk
            ReportProgress("Building databaseUpdate.txt");
            StringBuilder numberBuilder = new StringBuilder();
            numberBuilder.AppendLine(string.Format("Number of Added packages: {0}", addedPackages.Count));
            numberBuilder.AppendLine(string.Format("Number of Updated packages: {0}", updatedPackages.Count));
            numberBuilder.AppendLine(string.Format("Number of Disabled packages: {0}", disabledPackages.Count));
            numberBuilder.AppendLine(string.Format("Number of Removed packages: {0}", removedPackages.Count));
            numberBuilder.AppendLine(string.Format("Number of Moved packages: {0}", movedPackages.Count));
            numberBuilder.AppendLine(string.Format("Number of Renamed packages: {0}", renamedPackages.Count));
            numberBuilder.AppendLine(string.Format("Number of Internally renamed packages: {0}", internallyRenamed.Count));

            ReportProgress(numberBuilder.ToString());

            SetProgress(100);

            //abort if missing files
            if (missingPackages.Count > 0)
            {
                if (File.Exists(MissingPackagesTxt))
                    File.Delete(MissingPackagesTxt);
                filesNotFoundSB.Clear();
                foreach (DatabasePackage package in missingPackages)
                    filesNotFoundSB.AppendLine(package.ZipFile);
                File.WriteAllText(MissingPackagesTxt, filesNotFoundSB.ToString());
                ReportProgress("ERROR: " + missingPackages.Count + " packages missing files! (saved to missingPackages.txt)");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //make stringBuilder of databaseUpdate.text
            databaseUpdateText.Clear();
            databaseUpdateText.AppendLine("Database Update!\n");
            databaseUpdateText.AppendLine("New version tag: " + databaseVersionTag);

            databaseUpdateText.Append("\n" + numberBuilder.ToString());

            databaseUpdateText.AppendLine("\nAdded:");
            foreach (DatabasePackage dp in addedPackages)
                databaseUpdateText.AppendLine(" - " + dp.CompletePath);

            databaseUpdateText.AppendLine("\nUpdated:");
            foreach (DatabasePackage dp in updatedPackages)
                databaseUpdateText.AppendLine(" - " + dp.CompletePath);

            databaseUpdateText.AppendLine("\nRenamed:");
            foreach (DatabaseBeforeAfter dp in renamedPackages)
                databaseUpdateText.AppendFormat(" - \"{0}\" was renamed to \"{1}\"\r\n", dp.Before.NameFormatted, dp.After.NameFormatted);

            databaseUpdateText.AppendLine("\nMoved:");
            foreach (DatabaseBeforeAfter dp in movedPackages)
                databaseUpdateText.AppendFormat(" - \"{0}\" was moved to \"{1}\"\r\n", dp.Before.CompletePath, dp.After.CompletePath);

            databaseUpdateText.AppendLine("\nDisabled:");
            foreach (DatabasePackage dp in disabledPackages)
                databaseUpdateText.AppendLine(" - " + dp.CompletePath);

            databaseUpdateText.AppendLine("\nRemoved:");
            foreach (DatabasePackage dp in removedPackages)
                databaseUpdateText.AppendLine(" - " + dp.CompletePath);

            databaseUpdateText.AppendLine("\nNotes:\n - \n\n-----------------------------------------------------" +
                "---------------------------------------------------------------------------------------");

            //fix line endings
            string fixedDatabaseUpdate = databaseUpdateText.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");

            //save databaseUpdate.txt
            if (File.Exists(DatabaseUpdatePath))
                File.Delete(DatabaseUpdatePath);
            File.WriteAllText(DatabaseUpdatePath, fixedDatabaseUpdate);
            ReportProgress("Database text processed and written to disk");

            //save new modInfo.xml
            ReportProgress("Updating database");
            File.Delete(SelectModInfo.FileName);
            currentDatabaseManager.SaveDatabase(SelectModInfo.FileName);

            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateDatabaseV2Step4_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            //check for stuff
            ReportProgress("Starting DatabaseUpdate step 4");
            ReportProgress("Uploading changed files");

            //checks
            if (string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("WoTModpackOnlineFolderVersion is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if (!File.Exists(SelectModInfo.FileName))
            {
                ReportProgress("SelectModInfo file selected does not exist:" + SelectModInfo.FileName);
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if (!File.Exists(ManagerVersionPath))
            {
                ReportProgress("manager_version.xml does not exist");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            SetProgress(10);

            //database xmls
            ReportProgress("Uploading new database files to bigmods");
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                string databaseFtpPath = string.Format("{0}{1}/", PrivateStuff.BigmodsFTPModpackDatabase, WoTClientVersion);
                ReportProgress(string.Format("FTP upload path parsed as {0}", databaseFtpPath));

                //check if ftp folder exists
                ReportProgress(string.Format("Checking if FTP folder '{0}' exists", WoTClientVersion));
                string[] folders = await FtpUtils.FtpListFilesFoldersAsync(PrivateStuff.BigmodsFTPModpackDatabase, PrivateStuff.BigmodsNetworkCredential);
                if (!folders.Contains(WoTClientVersion))
                {
                    ReportProgress("Does not exist, making");
                    await FtpUtils.FtpMakeFolderAsync(databaseFtpPath, PrivateStuff.BigmodsNetworkCredential);
                }
                else
                {
                    ReportProgress("Yes, yes it does");
                }

                //RepoLatestDatabaseFolderPath
                ReportProgress("Uploading root");
                string rootDatabasePath = Path.Combine(RepoLatestDatabaseFolderPath, "database.xml");
                XmlDocument root1V1Document = XmlUtils.LoadXmlDocument(rootDatabasePath, XmlLoadType.FromFile);
                await client.UploadFileTaskAsync(databaseFtpPath + "database.xml", rootDatabasePath);

                //list of files by xml name to upload (because they all sit in the same folder, ftp and local
                ReportProgress("Creating list of database files to upload");
                List<string> xmlFilesNames = new List<string>()
                {
                    XmlUtils.GetXmlStringFromXPath(root1V1Document, "/modInfoAlpha.xml/globalDependencies/@file"),
                    XmlUtils.GetXmlStringFromXPath(root1V1Document, "/modInfoAlpha.xml/dependencies/@file")
                };
                foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(root1V1Document, "//modInfoAlpha.xml/categories/category"))
                {
                    xmlFilesNames.Add(categoryNode.Attributes["file"].Value);
                }

                //upload the files
                foreach(string xmlFilename in xmlFilesNames)
                {
                    ReportProgress(string.Format("Uploading {0}", xmlFilename));
                    string localPath = Path.Combine(RepoLatestDatabaseFolderPath, xmlFilename);
                    string ftpPath = databaseFtpPath + xmlFilename;
                    Logging.Updater("localPath = {0}", LogLevel.Debug, localPath);
                    Logging.Updater("ftpPath   = {0}", LogLevel.Debug, ftpPath);
                    await client.UploadFileTaskAsync(ftpPath, localPath);
                }
            }
            ReportProgress("Database uploads complete");

            SetProgress(85);

            //manager_version.xml
            ReportProgress("Uploading new manager_version.xml to bigmods");
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                string completeURL = PrivateStuff.BigmodsFTPModpackManager + ApplicationConstants.ManagerVersion;
                await client.UploadFileTaskAsync(completeURL, ManagerVersionPath);
            }

            SetProgress(90);

            //check if supported_clients.xml needs to be updated for a new version
            ReportProgress("Checking if supported_clients.xml needs to be updated for new WoT version");

            ReportProgress("Checking if latest WoT version is the same as this database supports");
            ReportProgress("Old version = " + LastSupportedTanksVersion + ", new version = " + WoTClientVersion);
            if (!LastSupportedTanksVersion.Equals(WoTClientVersion))
            {
                ReportProgress("Last supported version does not match");
                MessageBox.Show("Old database client version != new client version.\nPlease update the " + ApplicationConstants.SupportedClients + " document after publishing the database");
            }
            else
            {
                ReportProgress("DOES NOT need to be updated/uploaded");
            }

            SetProgress(100);
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void UpdateDatabasestep8_NA_ENG_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.com/index.php?/topic/535868-");
        }

        private void UpdateDatabaseStep8_EU_ENG_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.eu/index.php?/topic/623269-");
        }

        private void UpdateDatabaseStep8_EU_GER_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.eu/index.php?/topic/624499-");
        }
        #endregion

        #region Statistics
        private async void DatabaseStatsSave_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            //BigmodsFTPModpackManager
            ReportProgress("Starting stats saving");

            using (client = new WebClient { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                //download manager_version.xml
                ReportProgress("Downloading manager_version.xml from bigmods for app version");
                string managerVersionXml = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsFTPModpackManager + ApplicationConstants.ManagerVersion);
                string managerVersionXpath = @"/version/relhax_v2_stable";
                string managerVersion = XmlUtils.GetXmlStringFromXPath(managerVersionXml, managerVersionXpath, ApplicationConstants.ManagerVersion);
                ReportProgress("Done, parsed as " + managerVersion);

                //download supported_clients.xml
                ReportProgress("Downloading supported_clients.xml from bigmods for db version");
                string supportedClientsXml = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsFTPModpackManager + ApplicationConstants.SupportedClients);
                //https://stackoverflow.com/questions/1459132/xslt-getting-last-element
                string supportedClientsXpath = @"(//version)[last()]";
                string supportedClientLast = XmlUtils.GetXmlStringFromXPath(supportedClientsXml, supportedClientsXpath, ApplicationConstants.SupportedClients);
                ReportProgress("Done, parsed as " + supportedClientLast);

                //create new name
                string dateTimeFormat = string.Format("{0:yyyy_MM_dd}", DateTime.Now);
                string newFileName = string.Format("{0}_{1}_{2}_{3}.xml",dateTimeFormat, Path.GetFileNameWithoutExtension(InstallStatisticsXml),managerVersion,supportedClientLast);
                ReportProgress(string.Format("New filename parsed as '{0}', checking it doesn't exist on server", newFileName));

                //make sure name isn't on server already
                string[] filesOnServer = await FtpUtils.FtpListFilesFoldersAsync(PrivateStuff.BigmodsFTPModpackInstallStats, PrivateStuff.BigmodsNetworkCredential);
                if(filesOnServer.Contains(newFileName))
                {
                    ReportProgress("Already exists, abort");
                    return;
                }

                //upload to server
                ReportProgress("Does not exist, copying to new name");
                string instalStatsXmlText = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsFTPModpackInstallStats + InstallStatisticsXml);
                await client.UploadStringTaskAsync(PrivateStuff.BigmodsFTPModpackInstallStats + newFileName, instalStatsXmlText);
                ReportProgress("Done");
            }
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void DatabaseStatsUpdateCurrent_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
            List<Dependency> dependencies = new List<Dependency>();
            List<Category> parsedCategoryList = new List<Category>();
            string currentInstallStatsXml = string.Empty;
            DatabaseManager manager;

            using (client = new WebClient { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                //download supported_clients.xml
                ReportProgress("Downloading supported_clients.xml from bigmods for db version");
                string supportedClientsXml = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsFTPModpackManager + ApplicationConstants.SupportedClients);
                string supportedClientsXpath = @"(//version)[last()]";
                string supportedClientLast = XmlUtils.GetXmlStringFromXPath(supportedClientsXml, supportedClientsXpath, ApplicationConstants.SupportedClients);
                ReportProgress("Done, parsed as " + supportedClientLast);

                ReportProgress("Loading current database from bigmods");
                manager = await LoadDatabase1V1FromBigmods(supportedClientLast);
                if (manager == null)
                {
                    ReportProgress("Failed to parse modInfo to lists");
                    ToggleUI((TabController.SelectedItem as TabItem), true);
                    return;
                }

                //download current install statistics
                ReportProgress("Downloading current install statistics");
                currentInstallStatsXml = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsFTPModpackInstallStats + InstallStatisticsXml);
            }

            ReportProgress("Preparing lists for merge");
            XmlDocument installStats = XmlUtils.LoadXmlDocument(currentInstallStatsXml, XmlLoadType.FromString);
            List<DatabasePackage> flatList = manager.GetFlatList();

            //replace any non existent entries in installStats with empty entries where installCount = 0
            foreach (DatabasePackage package in flatList)
            {
                //@"//package[@name='Dependency_global_WoT_xml_Creation']"
                string xPath = string.Format(@"//package[@name='{0}']", package.PackageName);
                XmlNode node = installStats.SelectSingleNode(xPath);
                if(node == null)
                {
                    ReportProgress(string.Format("Package '{0}' does not exist, adding to install stats",package.PackageName));
                    XmlElement element = installStats.CreateElement("package");
                    XmlAttribute nameAttribute = installStats.CreateAttribute("name");
                    nameAttribute.Value = package.PackageName;
                    XmlAttribute instalCountAttribute = installStats.CreateAttribute("installCount");
                    instalCountAttribute.Value = 0.ToString();
                    element.Attributes.Append(nameAttribute);
                    element.Attributes.Append(instalCountAttribute);
                    installStats.DocumentElement.AppendChild(element);
                }
            }

            //upload document back to server
            ReportProgress("Merge complete, uploading statistics back to server");
            using (client = new WebClient { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                string xml = installStats.InnerXml;
                await client.UploadStringTaskAsync(PrivateStuff.BigmodsFTPModpackInstallStats + InstallStatisticsXml, xml);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void DatabaseStatsMakeNew_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
            List<Dependency> dependencies = new List<Dependency>();
            List<Category> parsedCategoryList = new List<Category>();
            DatabaseManager manager;

            using (client = new WebClient { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                //download supported_clients.xml
                ReportProgress("Downloading supported_clients.xml from bigmods for db version");
                string supportedClientsXml = await client.DownloadStringTaskAsync(PrivateStuff.BigmodsFTPModpackManager + ApplicationConstants.SupportedClients);
                string supportedClientsXpath = @"(//version)[last()]";
                string supportedClientLast = XmlUtils.GetXmlStringFromXPath(supportedClientsXml, supportedClientsXpath, ApplicationConstants.SupportedClients);
                ReportProgress("Done, parsed as " + supportedClientLast);

                ReportProgress("Loading current database from bigmods");
                manager = await LoadDatabase1V1FromBigmods(supportedClientLast);
                if (manager == null)
                {
                    ReportProgress("Failed to parse modInfo to lists");
                    ToggleUI((TabController.SelectedItem as TabItem), true);
                    return;
                }
            }

            ReportProgress("Creating new xml document");
            List<DatabasePackage> flatList = manager.GetFlatList();
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            doc.AppendChild(xmlDeclaration);
            XmlElement root = doc.CreateElement(InstallStatisticsXml);
            doc.AppendChild(root);

            foreach (DatabasePackage package in flatList)
            {
                XmlElement element = doc.CreateElement("package");
                XmlAttribute nameAttribute = doc.CreateAttribute("name");
                nameAttribute.Value = package.PackageName;
                XmlAttribute instalCountAttribute = doc.CreateAttribute("installCount");
                instalCountAttribute.Value = 0.ToString();
                element.Attributes.Append(nameAttribute);
                element.Attributes.Append(instalCountAttribute);
                root.AppendChild(element);
            }

            ReportProgress("Document created, uploading to server");
            using (client = new WebClient { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                string xml = doc.InnerXml;
                await client.UploadStringTaskAsync(PrivateStuff.BigmodsFTPModpackInstallStats + InstallStatisticsXml, xml);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }
        #endregion

        #region DatabasePackage and UIDs checks
        DatabaseManager databaseManagerDuplicateCheck;

        private async void AnotherLoadDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Loading database");

            if (!OnLoadModInfo())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //list creation and parsing
            databaseManagerDuplicateCheck = new DatabaseManager(ModpackSettings, CommandLineSettings);
            await databaseManagerDuplicateCheck.LoadDatabaseTestAsync(SelectModInfo.FileName);

            //link stuff in memory
            databaseManagerDuplicateCheck.ProcessDatabase();

            ReportProgress("Database loaded");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void DatabaseDuplicatePNsCheck_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Checking for duplicate packageNames");

            //checks
            if (string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("WoTModpackOnlineFolderVersion is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (databaseManagerDuplicateCheck == null)
            {
                ReportProgress("Duplicate Check database manager is null");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            List<string> duplicatesList = databaseManagerDuplicateCheck.CheckForDuplicatePackageNamesStringsList();

            if (duplicatesList == null || duplicatesList.Count == 0)
            {
                ReportProgress("No duplicates");
            }
            else
            {
                ReportProgress("The following packages are duplicate packageNames:");
                foreach (string s in duplicatesList)
                    ReportProgress(s);
            }

            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void DatabaseDuplicateUIDsCheck_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Checking for duplicate UIDs");

            //checks
            if (string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("WoTModpackOnlineFolderVersion is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (databaseManagerDuplicateCheck == null)
            {
                ReportProgress("Duplicate Check database manager is null");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            List<DatabasePackage> duplicatesList = databaseManagerDuplicateCheck.CheckForDuplicateUIDsPackageList();

            if (duplicatesList.Count == 0)
            {
                ReportProgress("No duplicates");
            }
            else
            {
                ReportProgress("The following packages are duplicate UIDs:");
                foreach (DatabasePackage package in duplicatesList)
                    ReportProgress(string.Format("PackageName: {0}, UID: {1}",package.PackageName,package.UID));
            }

            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void AddMissingUIDs_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Checking for missing UIDs and adding");

            //checks
            if (string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("WoTModpackOnlineFolderVersion is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (databaseManagerDuplicateCheck == null)
            {
                ReportProgress("Duplicate Check database manager is null");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //create a flat list
            List<DatabasePackage> allPackages = databaseManagerDuplicateCheck.GetFlatList();

            foreach (DatabasePackage packageToAddUID in allPackages)
            {
                if(string.IsNullOrWhiteSpace(packageToAddUID.UID))
                {
                    await Task.Run(() =>
                    {
                        packageToAddUID.UID = CommonUtils.GenerateUID(allPackages);
                    });
                    ReportProgress(string.Format("Package {0} got generated UID {1}", packageToAddUID.PackageName, packageToAddUID.UID));
                }
            }

            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void SaveDatabaseDuplicateCheckButton_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Saving Database");

            //checks
            if (databaseManagerDuplicateCheck == null)
            {
                ReportProgress("Duplicate Check database manager is null");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (SelectModInfoSave.ShowDialog() == false)
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            string fullDatabasePath = Path.Combine(Path.GetDirectoryName(SelectModInfoSave.FileName), ApplicationConstants.BetaDatabaseV2RootFilename);
            databaseManagerDuplicateCheck.SaveDatabase(fullDatabasePath);

            ReportProgress("Database saved");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }
        #endregion

        #region Supported_Clients updating
        private void LoadDatabaseUpdateSupportedClientsButton_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Loading database");

            if (!OnLoadModInfo())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void LoadSupportedClientsDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress(string.Format("Loading {0}", ApplicationConstants.SupportedClients));

            if(!(bool)SelectSupportedClientsXml.ShowDialog())
            {
                ReportProgress("Canceled");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void CheckClientsToRemoveFromDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Remove clients from xml and server");

            //checks
            if (string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("WoTModpackOnlineFolderVersion is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (string.IsNullOrEmpty(SelectModInfo.FileName))
            {
                ReportProgress("SelectModInfo filename is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (string.IsNullOrEmpty(SelectSupportedClientsXml.FileName))
            {
                ReportProgress("SelectSupportedClientsXml Filename is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //parse supported_clients.xml
            XmlDocument supportedClients = await ParseVersionInfoXmlDoc(SelectSupportedClientsXml.FileName);

            //get list of WoT online folders (ftp)
            ReportProgress("Getting a list of supported WoT clients by major version from the ftp server's folder list");
            string[] foldersList = await FtpUtils.FtpListFilesFoldersAsync(PrivateStuff.BigmodsFTPRootWoT, PrivateStuff.BigmodsNetworkCredential);

            //check any online folder clients where no matching server folder name and remove
            bool anyEntriesRemoved = false;
            XmlNodeList supportedClientsXmlList = XmlUtils.GetXmlNodesFromXPath(supportedClients, "//versions/version");
            for(int i = 0; i < supportedClientsXmlList.Count; i++)
            {
                XmlElement version = supportedClientsXmlList[i] as XmlElement;
                string onlineFolderVersion = version.Attributes["folder"].InnerText;

                if(!foldersList.Contains(onlineFolderVersion))
                {
                    ReportProgress(string.Format("Version {0} (online folder {1}) is not supported and will be removed", version.InnerText, onlineFolderVersion));
                    version.ParentNode.RemoveChild(version);
                    ReportProgress("Also removing the folder on the server if exists");
                    string[] databaseVersions = (await FtpUtils.FtpListFilesFoldersAsync(PrivateStuff.BigmodsFTPModpackDatabase, PrivateStuff.BigmodsNetworkCredential));
                    if (databaseVersions.Contains(version.InnerText))
                    {
                        string folderUrl = PrivateStuff.BigmodsFTPModpackDatabase + version.InnerText + "/";
                        await FtpUtils.FtpDeleteFolderAsync(folderUrl, PrivateStuff.BigmodsNetworkCredential);
                    }
                    anyEntriesRemoved = true;
                }
                else
                {
                    ReportProgress(string.Format("Version {0} (online folder {1}) exists", version.InnerText, onlineFolderVersion));
                }
            }

            if (anyEntriesRemoved)
            {
                ReportProgress("Entries removed, saving file back to disk and upload");
                supportedClients.Save(SelectSupportedClientsXml.FileName);

                using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
                {
                    await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackManager + ApplicationConstants.SupportedClients, SelectSupportedClientsXml.FileName);
                }
            }
            else
            {
                ReportProgress("No entries removed");
            }

            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void CheckClientsToAddToDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Add clients to and upload supported_clients.xml");
            
            //checks
            if (string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("WoTModpackOnlineFolderVersion is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (string.IsNullOrEmpty(SelectModInfo.FileName))
            {
                ReportProgress("SelectModInfo filename is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (string.IsNullOrEmpty(SelectSupportedClientsXml.FileName))
            {
                ReportProgress("SelectSupportedClientsXml Filename is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //parse supported_clients.xml
            XmlDocument supportedClients = await ParseVersionInfoXmlDoc(SelectSupportedClientsXml.FileName);

            //if loaded database's wot version is new, then add it to the document
            // "/versions/version[text()='1.8.0.1']"
            string xpathString = string.Format(@"/versions/version[text()='{0}']",WoTClientVersion);
            XmlNode selectedVersion = XmlUtils.GetXmlNodeFromXPath(supportedClients, xpathString);
            if(selectedVersion == null)
            {
                ReportProgress("Does not exist in the document, adding");
                //select the document root node
                XmlNode versionRoot = supportedClients.SelectSingleNode("/versions");

                //create the version element and set attributes and text
                XmlElement supported_client = supportedClients.CreateElement("version");
                supported_client.InnerText = WoTClientVersion;
                supported_client.SetAttribute("folder", WoTModpackOnlineFolderVersion);

                //add element to document at the end
                versionRoot.AppendChild(supported_client);

                ReportProgress("Entry added, saving file back to disk and upload");
                supportedClients.Save(SelectSupportedClientsXml.FileName);

                using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
                {
                    await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackManager + ApplicationConstants.SupportedClients, SelectSupportedClientsXml.FileName);
                }
            }
            else
            {
                //does exist, no further action needed
                ReportProgress("Already exists");
            }

            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }
        #endregion

        #region Translations
        private struct TranslationStruct
        {
            public TranslationStruct(string languageName, Dictionary<string,string> dict)
            {
                LanguageName = languageName;
                Dict = dict;
            }

            public string LanguageName { get; }
            public Dictionary<string, string> Dict { get; }

            public override string ToString() => $"({LanguageName})";
        }

        private void WriteTranslationsToCsvButton_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Loading translations if not loaded");

            if (!Translations.TranslationsLoaded)
                Translations.LoadTranslations();

            //create arrays of all languages and hashes
            TranslationStruct[] langauges =
            {
                new TranslationStruct(Translations.GetLanguageNativeName(Languages.English), Translations.GetLanguageDictionaries(Languages.English)),
                new TranslationStruct(Translations.GetLanguageNativeName(Languages.French), Translations.GetLanguageDictionaries(Languages.French)),
                new TranslationStruct(Translations.GetLanguageNativeName(Languages.German), Translations.GetLanguageDictionaries(Languages.German)),
                new TranslationStruct(Translations.GetLanguageNativeName(Languages.Russian), Translations.GetLanguageDictionaries(Languages.Russian)),
                new TranslationStruct(Translations.GetLanguageNativeName(Languages.Polish), Translations.GetLanguageDictionaries(Languages.Polish)),
                new TranslationStruct(Translations.GetLanguageNativeName(Languages.Spanish), Translations.GetLanguageDictionaries(Languages.Spanish)),
            };

            ReportProgress("Building the csv");
            StringBuilder translationsBuilder = new StringBuilder();

            //apply the header
            translationsBuilder.Append("ID");
            foreach(TranslationStruct translationStruct in langauges)
            {
                translationsBuilder.AppendFormat("\t{0}", translationStruct.LanguageName);
            }
            translationsBuilder.Append(Environment.NewLine);

            //apply each element. Use English as the method to get the key
            foreach(string key in Translations.GetLanguageDictionaries(Languages.English).Keys)
            {
                translationsBuilder.AppendFormat("{0}", key);
                foreach(TranslationStruct translationStruct in langauges)
                {
                    //if it doesn't exist, then mark it. Also need to use tabs, so escape them in the csv with "\t"
                    if (translationStruct.Dict.ContainsKey(key))
                        translationsBuilder.AppendFormat("\t{0}", MacroUtils.MacroReplace(translationStruct.Dict[key],ReplacementTypes.TextEscape));
                    else
                        translationsBuilder.AppendFormat("\tMISSING_TRANSLATION");
                }
                translationsBuilder.Append(Environment.NewLine);
            }

            //check if any of the other languages have any 'extra' translations we don't need anymore (i.e. aren't in english)
            foreach (TranslationStruct translationStruct in langauges)
            {
                if (translationStruct.LanguageName.Equals(Translations.GetLanguageNativeName(Languages.English)))
                    continue;
                foreach (string key in translationStruct.Dict.Keys)
                {
                    if (!Translations.GetLanguageDictionaries(Languages.English).ContainsKey(key))
                    {
                        Logging.Warning("The key {0} exists in language {1} but not in english, is it needed?", key, translationStruct.LanguageName);
                    }
                }
            }

            ReportProgress("Writing csv to disk");
            if (File.Exists(TranslationsCsv))
                File.Delete(TranslationsCsv);
            File.WriteAllText(TranslationsCsv, translationsBuilder.ToString());
            ReportProgress("Done");

            ToggleUI((TabController.SelectedItem as TabItem), true);
        }
        #endregion

        #region Medias Cleaning and Testing
        private struct MediasCleaningStruct
        {
            public SelectablePackage SelectablePackage;
            public Media MediaWithProblem;
        }

        private async void CleanMediasStep2_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Attempting to download all media to test if it's still valid");

            //checks
            if (string.IsNullOrEmpty(WoTModpackOnlineFolderVersion))
            {
                ReportProgress("WoTModpackOnlineFolderVersion is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            List<MediasCleaningStruct> brokenMedias = new List<MediasCleaningStruct>();
            DatabaseManager databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);
            DatabaseLoadFailCode failCode = await databaseManager.LoadDatabaseTestAsync(SelectModInfo.FileName);


            ReportProgress("Parsing database document");
            //parse main database
            if (failCode != DatabaseLoadFailCode.None)
            {
                ReportProgress("Failed to parse database");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            SetProgress(20);

            //bulid link refrences (parent/child, levels, etc)
            databaseManager.ProcessDatabase();
            PatientWebClient patientWebClient = new PatientWebClient() { Timeout = 60 * CommonUtils.TO_SECONDS };

            //get an estimate count beforehand
            int numToTest = 0;
            int totalTested = 0;
            foreach (SelectablePackage selectablePackage in databaseManager.GetFlatSelectablePackageList())
            {
                if (selectablePackage.Medias == null || selectablePackage.Medias.Count == 0)
                    continue;

                for (int i = 0; i < selectablePackage.Medias.Count; i++)
                {
                    Media media = selectablePackage.Medias[i];
                    if (media.MediaType == MediaType.MediaFile || media.MediaType == MediaType.Picture)
                    {
                        numToTest++;
                    }
                }
            }
            JobProgressBar.Maximum = numToTest;
            CleanMediasCancel.Visibility = Visibility.Visible;
            CleanMediasCancel.IsEnabled = true;

            foreach (SelectablePackage selectablePackage in databaseManager.GetFlatSelectablePackageList())
            {
                if (selectablePackage.Medias == null || selectablePackage.Medias.Count == 0)
                    continue;

                for (int i = 0; i < selectablePackage.Medias.Count; i++)
                {
                    Media media = selectablePackage.Medias[i];
                    if (media.MediaType == MediaType.MediaFile || media.MediaType == MediaType.Picture)
                    {
                        ReportProgress(string.Format("Attempt to download media {0} of {1}, package {2}, type {3}, url {4}", totalTested, numToTest, selectablePackage.PackageName, selectablePackage.Medias[i].MediaType, selectablePackage.Medias[i].URL));
                        try
                        {
                            byte[] tempByte = await patientWebClient.DownloadDataTaskAsync(selectablePackage.Medias[i].URL);
                            ReportProgress("Download PASS");
                        }
                        catch (WebException wex)
                        {
                            if (wex.Status == WebExceptionStatus.RequestCanceled)
                            {
                                ReportProgress("Process canceled");
                                CleanMediasCancel.Visibility = Visibility.Hidden;
                                CleanMediasCancel.IsEnabled = false;
                                ToggleUI((TabController.SelectedItem as TabItem), true);
                                JobProgressBar.Minimum = 0;
                                JobProgressBar.Value = JobProgressBar.Minimum;
                                JobProgressBar.Maximum = 100;
                                return;
                            }

                            ReportProgress("Download FAIL");
                            brokenMedias.Add(new MediasCleaningStruct() { SelectablePackage = selectablePackage, MediaWithProblem = media });
                            Logging.Error(wex.ToString());
                        }
                        finally
                        {
                            totalTested++;
                            SetProgress(totalTested);
                        }
                    }
                }
            }

            patientWebClient.Dispose();

            ReportProgress(string.Format("Finished, {0} of {1} medias have problems", brokenMedias.Count, numToTest));
            foreach (MediasCleaningStruct mediasCleaningStruct in brokenMedias)
            {
                SelectablePackage packageWithIssue = mediasCleaningStruct.SelectablePackage;
                Media mediaWithProblem = mediasCleaningStruct.MediaWithProblem;
                ReportProgress(string.Format("Package {0}, type {1}, url {2}", packageWithIssue.PackageName, mediaWithProblem.MediaType, mediaWithProblem.URL));
            }

            MessageBoxResult result = MessageBox.Show("Remove all packages with invalid media?", "Do the thing?", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                foreach (MediasCleaningStruct mediasCleaningStruct in brokenMedias)
                {
                    mediasCleaningStruct.SelectablePackage.Medias.Remove(mediasCleaningStruct.MediaWithProblem);
                }
                databaseManager.SaveDatabase(SelectModInfo.FileName);
                ReportProgress(string.Format("Broken Medias deleted"));
            }

            ReportProgress("Done");
            CleanMediasCancel.Visibility = Visibility.Hidden;
            CleanMediasCancel.IsEnabled = false;
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }
        #endregion

        #region Create Automation Root Entries
        private async void LoadDatabaseOutputRootAutomationText_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Loading database");

            if (!OnLoadModInfo())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //list creation and parsing
            databaseManagerDuplicateCheck = new DatabaseManager(ModpackSettings, CommandLineSettings);
            await databaseManagerDuplicateCheck.LoadDatabaseTestAsync(SelectModInfo.FileName);

            //link stuff in memory
            databaseManagerDuplicateCheck.ProcessDatabase();

            ReportProgress("Database loaded");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private void OutputRootAutomationEntriesButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            //checks
            if (databaseManagerDuplicateCheck == null)
            {
                ReportProgress("databaseManagerDuplicateCheck is null (load database)");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            if (databaseManagerDuplicateCheck.GlobalDependencies == null || databaseManagerDuplicateCheck.GlobalDependencies.Count == 0)
            {
                ReportProgress("databaseManagerDuplicateCheck has not been populated (load database)");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //script
            StringBuilder textBuilder = new StringBuilder();
            foreach (DatabasePackage package in databaseManagerDuplicateCheck.GetFlatList())
                textBuilder.AppendFormat("{0}{1}", package.ToAutomationElement(), Environment.NewLine);
            File.WriteAllText(AutomationElementsTxt, textBuilder.ToString());
            ReportProgress("Automation elements saved to " + AutomationElementsTxt);
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }
        #endregion

        #region Convert Instruction entries in zip files into new db schema
        private async void LoadDatabaseConvertInstructionsToDbButton_Click(object sender, RoutedEventArgs e)
        {
            //init UI
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Loading database");

            if (!OnLoadModInfo())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //list creation and parsing
            databaseManagerDuplicateCheck = new DatabaseManager(ModpackSettings, CommandLineSettings);
            await databaseManagerDuplicateCheck.LoadDatabaseTestAsync(SelectModInfo.FileName);

            //link stuff in memory
            databaseManagerDuplicateCheck.ProcessDatabase();

            ReportProgress("Database loaded");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void ConvertInstructionsToDbButton_Click(object sender, RoutedEventArgs e)
        {
            string[] rootFoldersToReplace = new string[]
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ApplicationConstants.PatchFolderName,
                ApplicationConstants.ShortcutFolderName,
                ApplicationConstants.XmlUnpackFolderName,
                ApplicationConstants.AtlasCreationFoldername
            };

            string[] rootFoldersToRemove = new string[]
            {
                ApplicationConstants.PatchFolderName,
                ApplicationConstants.ShortcutFolderName,
                ApplicationConstants.XmlUnpackFolderName,
                ApplicationConstants.AtlasCreationFoldername,
                ApplicationConstants.ReadmeFromZipfileFolderName
#pragma warning restore CS0618 // Type or member is obsolete
            };

            string[] xpathToTestFor = new string[]
            {
                Patch.PatchXmlSearchPath,
                Atlas.AtlasXmlSearchPath,
                XmlUnpack.XmlUnpackXmlSearchPath,
                Shortcut.ShortcutXmlSearchPath
            };

            ToggleUI((TabController.SelectedItem as TabItem), false);
            string lastConvertedPackageUID = string.Empty;
            if (File.Exists(LastInstructionConvertedPackageTxt))
            {
                lastConvertedPackageUID = File.ReadAllText(LastInstructionConvertedPackageTxt).Trim();
                ReportProgress(string.Format("Last instruction converted: {0}", lastConvertedPackageUID));
            }
            else
            {
                ReportProgress("Last instruction converted text files does not exist, assuming starting from beginning");
            }

            ReportProgress("Getting list of packages with zip files");
            List<DatabasePackage> packagesWithZipfiles = databaseManagerDuplicateCheck.GetFlatList().FindAll(package => !string.IsNullOrEmpty(package.ZipFile));

            DatabasePackage packageLastConverted = string.IsNullOrEmpty(lastConvertedPackageUID) ? packagesWithZipfiles[0] : packagesWithZipfiles.Find(package => package.UID.Equals(lastConvertedPackageUID));
            if (packageLastConverted == null)
            {
                ReportProgress(string.Format("Unable to find package with UID {0}", lastConvertedPackageUID));
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            for (int i = packagesWithZipfiles.IndexOf(packageLastConverted); i < packagesWithZipfiles.Count; i++)
            {
                DatabasePackage packageToConvert = packagesWithZipfiles[i];
                ReportProgress(string.Format("Converting package {0}, UID {1}", packageToConvert.PackageName, packageToConvert.UID));

                ReportProgress("Downloading package zipfile");
                if (File.Exists(packageToConvert.ZipFile))
                    File.Delete(packageToConvert.ZipFile);

                using (WebClient client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential})
                {
                    string downloadUrlString = string.Format("{0}{1}/{2}", PrivateStuff.BigmodsFTPRootWoT, databaseManagerDuplicateCheck.WoTOnlineFolderVersion, packageToConvert.ZipFile);
                    JobProgressBar.Maximum = await FtpUtils.FtpGetFilesizeAsync(downloadUrlString, PrivateStuff.BigmodsNetworkCredential);
                    JobProgressBar.Minimum = 0;
                    client.DownloadProgressChanged += (_sender, args) =>
                    {
                        JobProgressBar.Value = args.BytesReceived;
                    };
                    await client.DownloadFileTaskAsync(downloadUrlString, packageToConvert.ZipFile);

                    ReportProgress("Reading zip file for instructions");
                    bool zipfileModified = false;
                    bool zipIsNowEmpty = false;
                    using (ZipFile zip = new ZipFile(packageToConvert.ZipFile))
                    {
                        for (int j = 0; j < zip.Entries.Count; j++)
                        {
                            ZipEntry zipEntry = zip[j];
                            string[] splitEntryName = zipEntry.FileName.Split('/');
                            string rootFolder = splitEntryName[0];
                            if (rootFoldersToReplace.Contains(rootFolder) && !zipEntry.IsDirectory)
                            {
                                zipfileModified = true;
                                string potentialFile = splitEntryName[1];
                                string xmlString;
                                ReportProgress(string.Format("Processing Entry {0}", potentialFile));
                                UiUtils.AllowUIToUpdate();
                                using (MemoryStream ms = new MemoryStream() { Position = 0 })
                                using (StreamReader sr = new StreamReader(ms))
                                {
                                    zipEntry.Extract(ms);

                                    //read stream
                                    ms.Position = 0;
                                    xmlString = sr.ReadToEnd();
                                }

                                if (string.IsNullOrEmpty(xmlString))
                                {
                                    ReportProgress("ERROR: string is empty after extraction");
                                    continue;
                                }

                                XDocument doc = XmlUtils.LoadXDocument(xmlString, XmlLoadType.FromString);

                                List<XElement> results = null;
                                XmlComponent component = null;
                                string usedXpath = null;
                                foreach (string xpath in xpathToTestFor)
                                {
                                    List<XElement> results_ = doc.XPathSelectElements(xpath).ToList();
                                    if (results_ != null && results_.Count > 0)
                                    {
                                        results = results_;
                                        usedXpath = xpath;
                                        break;
                                    }
                                }

                                if (results == null)
                                {
                                    ReportProgress("ERROR: unable to detect instruction type");
                                    continue;
                                }

                                foreach (XElement element in results)
                                {
                                    //create correct object
                                    switch (usedXpath)
                                    {
                                        case Patch.PatchXmlSearchPath:
                                            component = new Patch();
                                            break;
                                        case Atlas.AtlasXmlSearchPath:
                                            component = new Atlas();
                                            break;
                                        case XmlUnpack.XmlUnpackXmlSearchPath:
                                            component = new XmlUnpack();
                                            break;
                                        case Shortcut.ShortcutXmlSearchPath:
                                            component = new Shortcut();
                                            break;
                                    }

                                    //parse from xml
                                    component.FromXml(element, XmlComponent.SchemaV1Dot0);

                                    //add to correct list
                                    switch (usedXpath)
                                    {
                                        case Patch.PatchXmlSearchPath:
                                            packageToConvert.Patches.Add(component as Patch);
                                            break;
                                        case Atlas.AtlasXmlSearchPath:
                                            packageToConvert.Atlases.Add(component as Atlas);
                                            break;
                                        case XmlUnpack.XmlUnpackXmlSearchPath:
                                            packageToConvert.XmlUnpacks.Add(component as XmlUnpack);
                                            break;
                                        case Shortcut.ShortcutXmlSearchPath:
                                            packageToConvert.Shortcuts.Add(component as Shortcut);
                                            break;
                                    }
                                }
                            }
                        }

                        if (zipfileModified)
                        {
                            bool zipWasActuallyModified = false;
                            foreach (string folderEntry in rootFoldersToRemove)
                            {
                                List<ZipEntry> zipsToRemove = zip.Entries.ToList().FindAll(entry => entry.FileName.StartsWith(folderEntry));
                                if (zipsToRemove.Count > 0)
                                    zipWasActuallyModified = true;
                                zip.RemoveEntries(zipsToRemove);
                            }
                            zipIsNowEmpty = zip.Entries.Count == 0;
                            if (zipfileModified && !zipWasActuallyModified)
                                throw new BadMemeException("Zip had instructions extracted but they wern't removed from the zip file. Hmmmmmmmmmm");
                            if (!zipIsNowEmpty)
                                zip.Save();
                        }
                    }
                    string zipToDelete = packageToConvert.ZipFile;

                    if (zipfileModified)
                    {
                        if (!zipIsNowEmpty)
                        {
                            ReportProgress("Zip file was modified, upload new one");
                            string newZipfileName = packageToConvert.ZipFile.Replace(".zip", "_converted.zip");
                            packageToConvert.UpdateZipfile(newZipfileName);

                            string uploadUrlString = string.Format("{0}{1}/{2}", PrivateStuff.BigmodsFTPRootWoT, databaseManagerDuplicateCheck.WoTOnlineFolderVersion, packageToConvert.ZipFile);
                            JobProgressBar.Maximum = FileUtils.GetFilesize(zipToDelete);
                            client.UploadProgressChanged += (__sender, args) =>
                            {
                                JobProgressBar.Value = args.BytesSent;
                            };
                            await client.UploadFileTaskAsync(uploadUrlString, zipToDelete);
                        }
                        else
                        {
                            ReportProgress("Zip is now empty");
                            packageToConvert.UpdateZipfile(string.Empty);
                        }
                        databaseManagerDuplicateCheck.SaveDatabase(SelectModInfo.FileName, DatabaseManager.DocumentVersion1V2, XmlComponent.SchemaV1Dot1);
                    }
                    else
                        ReportProgress("Zip file was not modified");

                    File.Delete(zipToDelete);
                }

                File.Delete(LastInstructionConvertedPackageTxt);
                lastConvertedPackageUID = packageToConvert.UID;
                File.WriteAllText(LastInstructionConvertedPackageTxt, lastConvertedPackageUID);
                await FtpUtils.TriggerMirrorSyncAsync();
            }

            ReportProgress(string.Format("Done"));
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }
        #endregion
    }
}
