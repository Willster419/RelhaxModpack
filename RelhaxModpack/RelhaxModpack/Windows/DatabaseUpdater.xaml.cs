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

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseUpdater.xaml
    /// </summary>
    public partial class DatabaseUpdater : RelhaxWindow
    {
        #region Constants and statics
        private const string DatabaseUpdateFilename = "databaseUpdate.txt";
        private const string TrashXML = "trash.xml";
        private const string DatabaseXml = "database.xml";
        private const string MissingPackagesTxt = "missingPackages.txt";
        private const string RepoResourcesFolder = "resources";
        private const string RepoLatestDatabaseFolder = "latest_database";
        /// <summary>
        /// The current path for Willster419's database repository
        /// </summary>
        /// <remarks>
        /// This was done because the database repository is different then the application repository.
        /// During debug, this can be set to have the updater (in the repository path) assume that it's in the database repository.
        /// </remarks>
        public const string HardCodeRepoPath = "F:\\Tanks Stuff\\RelhaxModpackDatabase";
        /// <summary>
        /// Flag to use the 
        /// </summary>
        public static bool UseHardCodePath = false;
        #endregion

        #region properties
        private string DatabaseUpdatePath
        {
            get { return Path.Combine(UseHardCodePath ? HardCodeRepoPath : Settings.ApplicationStartupPath, RepoResourcesFolder, DatabaseUpdateFilename); }
        }

        private string SupportedClientsPath
        {
            get { return Path.Combine(UseHardCodePath ? HardCodeRepoPath : Settings.ApplicationStartupPath, RepoResourcesFolder, Settings.SupportedClients); }
        }

        private string ManagerVersionPath
        {
            get { return Path.Combine(UseHardCodePath ? HardCodeRepoPath : Settings.ApplicationStartupPath, RepoResourcesFolder, Settings.ManagerVersion); }
        }

        private string RepoLatestDatabaseFolderPath
        {
            get { return Path.Combine(UseHardCodePath ? HardCodeRepoPath : Settings.ApplicationStartupPath, RepoLatestDatabaseFolder); }
        }
        #endregion

        #region Editables
        private string KeyFilename = "key.txt";//can be overridden by command line argument
        private WebClient client;
        private bool authorized = false;
        private OpenFileDialog SelectModInfo = new OpenFileDialog() { Filter = "*.xml|*.xml" };
        private OpenFileDialog SelectV1Application = new OpenFileDialog() { Title = "Find V1 application to upload", Filter = "*.exe|*.exe" };
        private OpenFileDialog SelectV2Application = new OpenFileDialog() { Title = "Find V2 application to upload", Filter = "*.exe|*.exe" };
        private OpenFileDialog SelectManagerInfoXml = new OpenFileDialog() { Title = "Find manager_version.xml", Filter = "manager_version.xml|manager_version.xml" };
        #endregion

        private bool loading = true;

        #region Stuff for parts 3 and 4 to share
        //strings
        //the modInfoXml document name to upload to the modInfo online folder
        //only used for uploading to the server TODO: can be moved to step 4 ONLY
        //RENAME TO NewModInfoXml
        private string CurrentModInfoXml = "";
        //the last supported modInfo, gotten from the supported_clients.xml for comparing with the currentModInfoXml
        //LastSupportedModInfoXml is old name
        private string LastSupportedModInfoXml = "";
        //the new database version to upload
        private string DatabaseUpdateVersion = "";
        //the version number of the last supported WoT client, used for making backup online folder
        private string LastSupportedTanksVersion = "";

        #endregion

        #region Stuff for Cleaning online folders
        List<VersionInfos> VersionInfosList;
        VersionInfos selectedVersionInfos;
        bool cancelDelete = false;
        #endregion

        #region Password auth stuff
        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //check if key filename was changed from command line
            if (!string.IsNullOrWhiteSpace(CommandLineSettings.UpdateKeyFileName))
            {
                Logging.Updater("User specified from command line new key filename to use: {0}", LogLevel.Info, CommandLineSettings.UpdateKeyFileName);
                KeyFilename = CommandLineSettings.UpdateKeyFileName;
            }
            if(File.Exists(KeyFilename))
            {
                Logging.Updater("File for auth exists, attempting authorization");
                Logging.Updater(KeyFilename);
                AttemptAuthFromFile(KeyFilename);
            }
            else
            {
                Logging.Updater("Loading without pre-file authorization");
            }
            loading = false;
        }

        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            AttemptAuthFromString(PaswordTextbox.Text);
        }

        private async Task<bool> AttemptAuthFromFile(string filepath)
        {
            Logging.Updater("attempting authorization", LogLevel.Info, filepath);
            return await AttemptAuthFromString(File.ReadAllText(filepath));
        }

        private async Task<bool> AttemptAuthFromString(string key)
        {
            AuthStatusTextblock.Text = "Current status: Checking...";
            AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Yellow);
            //compare local password to online version
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                string onlinePassword = await client.DownloadStringTaskAsync(PrivateStuff.KeyAddress);
                if (onlinePassword.Equals(key))
                {
                    Logging.Updater("authorized, keys match");
                    AuthStatusTextblock.Text = "Current status: Authorized";
                    AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Green);
                    authorized = true;
                    return true;
                }
                else
                {
                    Logging.Updater("not authorized, keys do not match");
                    AuthStatusTextblock.Text = "Current status: Denied";
                    AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Red);
                    authorized = false;
                    return false;
                }
            }
        }
        #endregion

        #region Boring stuff
        /// <summary>
        /// Create an instance of the DatabaseUpdater window
        /// </summary>
        public DatabaseUpdater()
        {
            InitializeComponent();
        }

        private void OnLoadModInfo(object sender, RoutedEventArgs e)
        {
            if (SelectModInfo.ShowDialog() == true)
            {
                LogOutput.Text = "Loading ModInfo...";
                //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
                //for the folder version: //modInfoAlpha.xml/@version
                Settings.WoTModpackOnlineFolderVersion = XmlUtils.GetXmlStringFromXPath(SelectModInfo.FileName, "//modInfoAlpha.xml/@onlineFolder");
                Settings.WoTClientVersion = XmlUtils.GetXmlStringFromXPath(SelectModInfo.FileName, "//modInfoAlpha.xml/@version");
                string versionInfo = string.Format("{0}={1},  {2}={3}", nameof(Settings.WoTModpackOnlineFolderVersion)
                    , Settings.WoTModpackOnlineFolderVersion, nameof(Settings.WoTClientVersion), Settings.WoTClientVersion);
                Logging.Updater(versionInfo);
                ReportProgress(versionInfo);
            }
        }

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

        private void OnApplicationClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if strings are not empty and file exists, delete them
            //for all the class level strings
            Logging.Updater("Deleting trash files...");
            string[] filesToDelete = new string[]
            {
                DatabaseXml,
                CurrentModInfoXml,
                LastSupportedModInfoXml,
                TrashXML,
                MissingPackagesTxt
            };
            foreach (string s in filesToDelete)
            {
                if (!string.IsNullOrWhiteSpace(s) && File.Exists(s))
                    File.Delete(s);
            }
        }

        private void PaswordTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PasswordButton_Click(null, null);
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
        private void ReportProgress(string message)
        {
            //reports to the log file and the console otuptu
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
                ReportProgress(await Utils.CreateMD5HashAsync(s));
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
            foreach (FrameworkElement control in Utils.GetAllWindowComponentsLogical(tab, false))
            {
                if (control is Button butt)
                    butt.IsEnabled = toggle;
            }
            SetProgress(JobProgressBar.Minimum);
            Utils.AllowUIToUpdate();
        }

        private void SetProgress(double prog)
        {
            JobProgressBar.Value = prog;
            Utils.AllowUIToUpdate();
        }
        #endregion

        #region Database output
        private void SaveDatabaseText(bool @internal)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            //true = internal, false = user
            string notApplicable = "n/a";
            //list creation and parsing
            List<Category> parsecCateogryList = new List<Category>();
            List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
            List<Dependency> dependencies = new List<Dependency>();
            XmlDocument doc = new XmlDocument();
            doc.Load(SelectModInfo.FileName);
            XmlUtils.ParseDatabase(doc, globalDependencies, dependencies, parsecCateogryList);
            //link stuff in memory or something
            Utils.BuildLinksRefrence(parsecCateogryList, false);
            //create variables
            StringBuilder sb = new StringBuilder();
            string saveLocation = @internal ? System.IO.Path.Combine(Settings.ApplicationStartupPath, "database_internal.csv") :
                Path.Combine(Settings.ApplicationStartupPath, "database_user.csv");
            //global dependencies
            string header = @internal ? "PackageName\tCategory\tPackage\tLevel\tZip\tDevURL\tEnabled\tVisible" : "Category\tMod\tDevURL";
            sb.AppendLine(header);
            if(@internal)
            {
                foreach (DatabasePackage dp in globalDependencies)
                {
                    sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", dp.PackageName, "GlobalDependencies", "", "0",
                        string.IsNullOrWhiteSpace(dp.ZipFile) ? notApplicable : dp.ZipFile,
                        string.IsNullOrWhiteSpace(dp.DevURL) ? "" : "=HYPERLINK(\"" + dp.DevURL + "\",\"link\")", dp.Enabled, ""));
                }
                foreach (Dependency dep in dependencies)
                {
                    sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", dep.PackageName, "Dependencies", "", "0",
                        string.IsNullOrWhiteSpace(dep.ZipFile) ? notApplicable : dep.ZipFile,
                        string.IsNullOrWhiteSpace(dep.DevURL) ? "" : "=HYPERLINK(\"" + dep.DevURL + "\",\"link\")", dep.Enabled, ""));
                }
            }
            foreach (Category cat in parsecCateogryList)
            {
                List<SelectablePackage> flatlist = cat.GetFlatPackageList();
                foreach (SelectablePackage sp in flatlist)
                {
                    string nameIndneted = sp.NameFormatted;
                    if (!@internal)
                    {
                        for (int i = 0; i < sp.Level; i++)
                        {
                            nameIndneted = "--" + nameIndneted;
                        }
                        sb.AppendLine(string.Format("{0}\t{1}\t{2}", sp.ParentCategory.Name, nameIndneted,
                            string.IsNullOrWhiteSpace(sp.DevURL) ? "" : "=HYPERLINK(\"" + sp.DevURL + "\",\"link\")"));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", nameIndneted, sp.ParentCategory.Name, sp.NameFormatted,
                        sp.Level, string.IsNullOrWhiteSpace(sp.ZipFile) ? notApplicable : sp.ZipFile,
                        string.IsNullOrWhiteSpace(sp.DevURL) ? "" : "=HYPERLINK(\"" + sp.DevURL + "\",\"link\")",
                        sp.Enabled, sp.Visible));
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
            if (string.IsNullOrEmpty(Settings.WoTClientVersion) || string.IsNullOrEmpty(Settings.WoTModpackOnlineFolderVersion))
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
            if (string.IsNullOrEmpty(Settings.WoTClientVersion) || string.IsNullOrEmpty(Settings.WoTModpackOnlineFolderVersion))
            {
                ReportProgress("Database not loaded");
                return;
            }
            SaveDatabaseText(false);
        }
        #endregion

        #region Application update V1
        private async void UpdateApplicationV1UploadApplicationStable(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running Upload Stable Application");
            if (!(bool)SelectV1Application.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Uploading stable exe to wotmods...");
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.FTPModpackRoot + Path.GetFileName(SelectV1Application.FileName), SelectV1Application.FileName);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateApplicationV1UploadApplicationBeta(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running Upload Beta Application");
            if (!(bool)SelectV1Application.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Uploading beta exe to wotmods...");
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.FTPModpackRoot + Path.GetFileName(SelectV1Application.FileName), SelectV1Application.FileName);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateApplicationV1UploadManagerInfo(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running upload manager_info.xml");
            if (!(bool)SelectManagerInfoXml.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Upload manager_info.xml to wotmods");
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.FTPManagerInfoRoot + Path.GetFileName(SelectManagerInfoXml.FileName), SelectManagerInfoXml.FileName);
            }

            ReportProgress("Upload manager_info.xml to bigmods");
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackManager + Path.GetFileName(SelectManagerInfoXml.FileName), SelectManagerInfoXml.FileName);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateApplicationV1CreateUpdatePackages(object sender, RoutedEventArgs e)
        {
            ReportProgress("Running script to create update packages (wotmods)...");
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateUpdatePackagesPHP, 30 * Utils.TO_SECONDS);
        }

        private async void UpdateApplicationV1CreateManagerInfoWotmods(object sender, RoutedEventArgs e)
        {
            ReportProgress("Running script to create manager info (wotmods)...");
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateManagerInfoPHP, 30 * Utils.TO_SECONDS);
        }

        private async void UpdateApplicationV1CreateManagerInfoBigmods(object sender, RoutedEventArgs e)
        {
            ReportProgress("Running script to create manager info (bigmods)...");
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateManagerInfoPHP, 30 * Utils.TO_SECONDS);
        }
        #endregion

        #region Application update V2
        private async void UpdateApplicationV2UploadApplicationStable(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running Upload Stable Application");
            if (!(bool)SelectV2Application.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Uploading stable exe to wotmods...");
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackRelhaxModpack + Path.GetFileName(SelectV2Application.FileName), SelectV2Application.FileName);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateApplicationV2UploadApplicationBeta(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running Upload Beta Application");
            if (!(bool)SelectV2Application.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Uploading beta exe to wotmods...");
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackRelhaxModpack + Path.GetFileName(SelectV2Application.FileName), SelectV2Application.FileName);
            }
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateApplicationV2UploadManagerInfo(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running upload manager_info.xml");
            if (!(bool)SelectManagerInfoXml.ShowDialog())
            {
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            ReportProgress("Upload manager_info.xml to wotmods");
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                await client.UploadFileTaskAsync(PrivateStuff.FTPManagerInfoRoot + Path.GetFileName(SelectManagerInfoXml.FileName), SelectManagerInfoXml.FileName);
            }

            ReportProgress("Upload manager_info.xml to bigmods");
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

        private async void UpdateApplicationV2CreateManagerInfoWotmods(object sender, RoutedEventArgs e)
        {
            ReportProgress("Running script to create manager info (wotmods)...");
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateManagerInfoPHP, 100000);
        }

        private async void UpdateApplicationV2CreateManagerInfoBigmods(object sender, RoutedEventArgs e)
        {
            ReportProgress("Running script to create manager info (bigmods)...");
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateManagerInfoPHP, 30 * Utils.TO_SECONDS);
        }
        #endregion

        #region Cleaning online folders
        private async void CleanFoldersOnlineStep1_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            ReportProgress("Running Clean online folders step 1");
            ReportProgress("Downloading and parsing " + Settings.SupportedClients);
            //download supported_clients
            XmlDocument doc;
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                string xml = await client.DownloadStringTaskAsync(PrivateStuff.FTPManagerInfoRoot + Settings.SupportedClients);
                doc = XmlUtils.LoadXmlDocument(xml, XmlLoadType.FromString);
            }
            //parse each online folder to list type string
            ReportProgress("Parsing " + Settings.SupportedClients);
            CleanFoldersOnlineStep2b.Items.Clear();
            XmlNodeList supportedClients = XmlUtils.GetXmlNodesFromXPath(doc, "//versions/version");
            VersionInfosList = new List<VersionInfos>();
            foreach (XmlNode node in supportedClients)
            {
                VersionInfos newVersionInfo = new VersionInfos()
                {
                    WoTOnlineFolderVersion = node.Attributes["folder"].Value,
                    WoTClientVersion = node.InnerText
                };
                VersionInfosList.Add(newVersionInfo);
                CleanFoldersOnlineStep2b.Items.Add(newVersionInfo);
            }
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
            if(VersionInfosList == null)
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
            if(VersionInfosList.Count == 0)
            {
                ReportProgress("VersionsInfosList count = 0");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            selectedVersionInfos = (VersionInfos)CleanFoldersOnlineStep2b.SelectedItem;
            ReportProgress("Getting all trash files in online folder " + selectedVersionInfos.WoTOnlineFolderVersion);
            //make a new list where it only has versions who's online folder match the selected one from the combobox
            List<VersionInfos> specificVersions = 
                VersionInfosList.Where(info => info.WoTOnlineFolderVersion.Equals(selectedVersionInfos.WoTOnlineFolderVersion)).ToList();
            List<string> allUsedZipFiles = new List<string>();
            specificVersions.Add(new VersionInfos { WoTClientVersion = "GITHUB" });
            foreach(VersionInfos infos in specificVersions)
            {
                ReportProgress("Adding zip files from WoTClientVersion " + infos.WoTClientVersion);
                XmlDocument doc = new XmlDocument();
                List<DatabasePackage> flatList = new List<DatabasePackage>();
                List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
                List<Dependency> dependencies = new List<Dependency>();
                List<Category> parsedCategoryList = new List<Category>();
                //download and parse database to flat list
                using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
                {
                    if(infos.WoTClientVersion.Equals("GITHUB"))
                    {
#pragma warning disable CS0618
                        doc.LoadXml(await client.DownloadStringTaskAsync(Settings.BetaDatabaseV1URL));
#pragma warning enable CS0618
                        string betaDatabaseOnlineFolderVersion = XmlUtils.GetXmlStringFromXPath(doc, Settings.DatabaseOnlineFolderXpath);
                        ReportProgress(string.Format("GITHUB online folder={0}, selected online folder to clean version={1}",
                            betaDatabaseOnlineFolderVersion, selectedVersionInfos.WoTOnlineFolderVersion));
                        if (!betaDatabaseOnlineFolderVersion.Equals(selectedVersionInfos.WoTOnlineFolderVersion))
                        {
                            ReportProgress("Skipping (online folders are not equal)");
                            continue;
                        }
                        else
                        {
                            ReportProgress("Including (online folders are equal");
                        }
                    }
                    else
                    {
                        string newModInfoName = "modInfo_" + infos.WoTClientVersion + ".xml";
                        doc.LoadXml(await client.DownloadStringTaskAsync(PrivateStuff.ModInfosLocation + newModInfoName));
                    }
                }
                if (!XmlUtils.ParseDatabase(doc, globalDependencies, dependencies, parsedCategoryList))
                {
                    ReportProgress("failed to parse modInfo to lists");
                    ToggleUI((TabController.SelectedItem as TabItem), true);
                    return;
                }
                Utils.BuildLinksRefrence(parsedCategoryList, false);
                Utils.BuildLevelPerPackage(parsedCategoryList);
                //if the list of zip files does not already have it, then add it
                flatList = Utils.GetFlatList(globalDependencies, dependencies, null, parsedCategoryList);
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
                filesInOnlineFolder.LoadXml(await client.DownloadStringTaskAsync(string.Format("http://bigmods.relhaxmodpack.com/WoT/{0}/{1}",
                    selectedVersionInfos.WoTOnlineFolderVersion, DatabaseXml)));
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
            string[] filesActuallyInFolder = await Utils.FTPListFilesFoldersAsync(
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
                    ReportProgress(string.Format("skipping file {0}, does not exist", s));
                    count++;
                    continue;
                }
                ReportProgress(string.Format("Deleting file {0} of {1}, {2}", count++, filesToDelete.Count, s));
                await Utils.FTPDeleteFileAsync(string.Format("{0}{1}/{2}",
                    PrivateStuff.BigmodsFTPRootWoT, selectedVersionInfos.WoTOnlineFolderVersion, s), PrivateStuff.BigmodsNetworkCredential);
            }
            CleanZipFoldersTextbox.Clear();
            CleanFoldersOnlineCancelStep3.Visibility = Visibility.Hidden;
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }
        #endregion

        #region Database Updating
        private async void UpdateDatabaseStep2_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Starting Update database step 2...");
            ReportProgress("Running script to update online hash database...");
            //a PatientWebClient should allow a timeout value of 5 mins (or more)
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateDatabasePHP, 30 * Utils.TO_SECONDS * Utils.TO_MINUETS);
        }
              
        private async void UpdateDatabaseStep3_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            //getting local crcs and comparing them on server
            //init UI
            ReportProgress("Starting DatabaseUpdate step 3");

            //checks
            if(string.IsNullOrEmpty(Settings.WoTModpackOnlineFolderVersion))
            {
                ReportProgress("wot online folder version is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if(!File.Exists(SelectModInfo.FileName))
            {
                ReportProgress("selectMofInfo file selected does not exist:" + SelectModInfo.FileName);
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //init stringBuilders
            StringBuilder filesNotFoundSB = new StringBuilder();
            StringBuilder databaseUpdateText = new StringBuilder();
            filesNotFoundSB.Append("FILES NOT FOUND:\n");

            //init lists
            List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
            List<Dependency> dependencies = new List<Dependency>();
            List<Category> parsedCategoryList = new List<Category>();
            List<DatabasePackage> addedPackages = new List<DatabasePackage>();
            List<DatabasePackage> updatedPackages = new List<DatabasePackage>();
            List<DatabasePackage> disabledPackages = new List<DatabasePackage>();
            List<DatabasePackage> removedPackages = new List<DatabasePackage>();
            List<DatabasePackage> missingPackages = new List<DatabasePackage>();
            List<DatabaseBeforeAfter> renamedPackages = new List<DatabaseBeforeAfter>();
            List<DatabaseBeforeAfter> internallyRenamed = new List<DatabaseBeforeAfter>();
            List<DatabaseBeforeAfter> movedPackages = new List<DatabaseBeforeAfter>();

            //init strings
            CurrentModInfoXml = string.Empty;
            DatabaseUpdateVersion = string.Empty;
            LastSupportedModInfoXml = string.Empty;
            LastSupportedTanksVersion = string.Empty;

            SetProgress(10);

            //load current modInfoXml and check for duplicates
            ReportProgress("Loading current modInfo.xml and checking for duplicates");
            XmlDocument currentDatabase = new XmlDocument();
            currentDatabase.Load(SelectModInfo.FileName);
            if(!XmlUtils.ParseDatabase(currentDatabase,globalDependencies,dependencies,parsedCategoryList))
            {
                ReportProgress("failed to parse modInfo to lists");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            SetProgress(20);

            Utils.BuildLinksRefrence(parsedCategoryList, true);
            Utils.BuildLevelPerPackage(parsedCategoryList);
            List<DatabasePackage> flatListCurrent = Utils.GetFlatList(globalDependencies, dependencies, null, parsedCategoryList);

            List<string> duplicates = Utils.CheckForDuplicates(globalDependencies, dependencies, parsedCategoryList);
            if(duplicates.Count > 0)
            {
                ReportProgress("ERROR: Duplicates found!");
                foreach (string s in duplicates)
                    ReportProgress(s);
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

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

            ReportProgress(string.Format("lastWoTClientVersion = {0}", lastWoTClientVersion));
            ReportProgress(string.Format("lastDate = {0}", lastDate));
            ReportProgress(string.Format("currentWoTClientVersion = {0}", Settings.WoTClientVersion));
            ReportProgress(string.Format("currentDate = {0}", dateTimeFormat));

            if (lastWoTClientVersion.Equals(Settings.WoTClientVersion) && lastDate.Equals(dateTimeFormat))
            {
                ReportProgress("lastWoTVersion and date match, so incrementing the version");
                int lastItteration = int.Parse(database_version_text.InnerText.Split('_')[2]);
                DatabaseUpdateVersion = string.Format("{0}_{1}_{2}", Settings.WoTClientVersion, dateTimeFormat, ++lastItteration);
            }
            else
            {
                ReportProgress("lastWoTVersion and/or date NOT match, not incrementing the version (starts at 1)");
                DatabaseUpdateVersion = string.Format("{0}_{1}_1", Settings.WoTClientVersion, dateTimeFormat);
            }

            ReportProgress(string.Format("DatabaseUpdateVersion = {0}", DatabaseUpdateVersion));

            //update and save the manager_version.xml file
            database_version_text.InnerText = DatabaseUpdateVersion;
            doc.Save(ManagerVersionPath);

            //get last supported wot version for comparison
            LastSupportedTanksVersion = lastWoTClientVersion;
            LastSupportedModInfoXml = "modInfo_" + LastSupportedTanksVersion + ".xml";

            //also delete this just in case
            if (File.Exists(LastSupportedModInfoXml))
                File.Delete(LastSupportedModInfoXml);

            SetProgress(40);

            ReportProgress("getting last supported modInfo");
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                await client.DownloadFileTaskAsync(PrivateStuff.ModInfosLocation + LastSupportedModInfoXml, LastSupportedModInfoXml);
            }

            //make a flat list of old (last supported modInfoxml) for comparison
            XmlDocument oldDatabase = new XmlDocument();
            oldDatabase.Load(LastSupportedModInfoXml);
            List<DatabasePackage> globalDependenciesOld = new List<DatabasePackage>();
            List<Dependency> dependenciesOld = new List<Dependency>();
            List<Category> parsedCateogryListOld = new List<Category>();
            if(!XmlUtils.ParseDatabase(oldDatabase,globalDependenciesOld,dependenciesOld, parsedCateogryListOld))
            {
                ReportProgress("failed to parse modInfo to lists");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            Utils.BuildLinksRefrence(parsedCateogryListOld, true);
            Utils.BuildLevelPerPackage(parsedCateogryListOld);
            List<DatabasePackage> flatListOld = Utils.GetFlatList(globalDependenciesOld, dependenciesOld, null, parsedCateogryListOld);

            //download and load latest database.xml file from server
            ReportProgress("downloading database.xml of current WoT onlineFolder version from server");
            XmlDocument databaseXml = new XmlDocument();
            if (File.Exists(DatabaseXml))
            {
                File.Delete(DatabaseXml);
            }
            using (client = new WebClient())
            {
                await client.DownloadFileTaskAsync(string.Format("http://bigmods.relhaxmodpack.com/WoT/{0}/{1}",
                    Settings.WoTModpackOnlineFolderVersion, DatabaseXml), DatabaseXml);
                databaseXml.Load(DatabaseXml);
            }

            SetProgress(50);

            //update the crc values, also makes list of updated mods
            ReportProgress("downloaded, comparing crc values for list of updated mods");
            Utils.AllowUIToUpdate();
            foreach(DatabasePackage package in flatListCurrent)
            {
                if(string.IsNullOrEmpty(package.ZipFile))
                    continue;
                //"//database/file[@name="Sounds_HRMOD_Gun_Sounds_by_Zorgane_v2.01_1.2.0_2018-10-12.zip"]"
                string xpathText = string.Format("//database/file[@name=\"{0}\"]",package.ZipFile);
                XmlNode databaseEntry = databaseXml.SelectSingleNode(xpathText);
                if(databaseEntry != null)
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
                    ulong fakeSize = Utils.ParseuLong(databaseEntry.Attributes["size"].Value, 0);
                    if (package.Size == 0 || fakeSize == 0 || package.Size != fakeSize)
                    {
                        //update the current size of the package
                        package.Size = fakeSize;
                        if (package.Size == 0)
                        {
                            Logging.Updater("zip file {0} is 0 bytes (empty file)", LogLevel.Info, package.ZipFile);
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
            ReportProgress("getting list of added and removed packages");
            Utils.AllowUIToUpdate();
            PackageComparerByPackageName pc = new PackageComparerByPackageName();

            //if in before but not after = removed
            removedPackages = flatListOld.Except(flatListCurrent, pc).ToList();

            //if not in before but after = added
            addedPackages = flatListCurrent.Except(flatListOld, pc).ToList();

            //get the list of renamed packages
            //a renamed package will have the same internal name, but a different display name
            //first start by getting the list of all current packages, then filter out removed and added packages
            ReportProgress("getting list of renamed packages");
            Utils.AllowUIToUpdate();
            List<DatabasePackage> renamedPackagesTemp = new List<DatabasePackage>(flatListCurrent);
            renamedPackagesTemp = renamedPackagesTemp.Except(removedPackages, pc).Except(addedPackages, pc).ToList();
            //https://stackoverflow.com/questions/3842714/linq-selection-by-type-of-an-object
            List<SelectablePackage> selectablePackagesOld = flatListOld.OfType<SelectablePackage>().ToList();
            List<SelectablePackage> potentialRenamedPackages = renamedPackagesTemp.OfType<SelectablePackage>().ToList();
            foreach(SelectablePackage selectablePackage in potentialRenamedPackages)
            {
                List<SelectablePackage> results = selectablePackagesOld.Where(pack => pack.PackageName.Equals(selectablePackage.PackageName)).ToList();
                if (results.Count == 0)
                    continue;
                SelectablePackage result = results[0];
                if(!selectablePackage.NameFormatted.Equals(result.NameFormatted))
                {
                    Logging.Updater("package rename-> old:{0}, new:{1}", LogLevel.Info, result.PackageName, selectablePackage.PackageName);
                    renamedPackages.Add(new DatabaseBeforeAfter() {Before = result, After = selectablePackage });
                }
            }

            //list of moved packages
            //a moved package will have a different completePackagePath and different completePath, but still have the same internalName
            ReportProgress("getting list of moved packages");
            Utils.AllowUIToUpdate();
            foreach (SelectablePackage selectablePackage in potentialRenamedPackages)
            {
                List<SelectablePackage> results = selectablePackagesOld.Where(pack => pack.PackageName.Equals(selectablePackage.PackageName)).ToList();
                if (results.Count == 0)
                    continue;
                SelectablePackage result = results[0];
                bool completeNamePathChanged = !result.CompletePath.Equals(selectablePackage.CompletePath);
                bool completePackageNamePathChanged = !result.CompletePackageNamePath.Equals(selectablePackage.CompletePackageNamePath);
                if (completeNamePathChanged && completePackageNamePathChanged)
                {
                    Logging.Updater("package moved: {0}", LogLevel.Info, selectablePackage.PackageName);
                    movedPackages.Add(new DatabaseBeforeAfter { Before = result, After = selectablePackage });
                }
            }

            SetProgress(85);

            //move them to lists of selectablePackageType as well
            List<SelectablePackage> actualMovedPackages = movedPackages.Select(intt => intt.After).ToList();
            actualMovedPackages.AddRange(movedPackages.Select(intt => intt.Before).ToList());
            actualMovedPackages = actualMovedPackages.Distinct().ToList();

            //remove any packages that say are added and removed, but actually just had internal structure changed
            addedPackages = addedPackages.Except(actualMovedPackages, pc).ToList();
            removedPackages = removedPackages.Except(actualMovedPackages, pc).ToList();

            //if a package was internally renamed, it will show up in the added and removed list
            //a internal renamed package will have a different completePackagePath but the same completePath (assuming it wasn't renamed as well), and different internalName
            //first get the list of *selectable* Packages for added and removed
            ReportProgress("checking add and removed list for internal renamed");
            Utils.AllowUIToUpdate();
            List<SelectablePackage> addedSelectablePackages = addedPackages.OfType<SelectablePackage>().ToList();
            List<SelectablePackage> removedSelectablePackages = removedPackages.OfType<SelectablePackage>().ToList();

            //get a string list of completePath and completePackagePath
            List<string> completePathDetect = addedSelectablePackages.Select(pack => pack.CompletePath).ToList();
            completePathDetect.AddRange(removedSelectablePackages.Select(pack => pack.CompletePath).ToList());
            completePathDetect = completePathDetect.Distinct().ToList();

            SetProgress(90);

            ReportProgress(string.Format("CompletePath count compare of add and remove is {0}", completePathDetect.Count));
            if (completePathDetect.Count > 0)
            {
                foreach(string completePathDet in completePathDetect)
                {
                    List<SelectablePackage> addResultList = addedSelectablePackages.Where(pack => pack.CompletePath.Equals(completePathDet)).ToList();
                    List<SelectablePackage> removeResultList = removedSelectablePackages.Where(pack => pack.CompletePath.Equals(completePathDet)).ToList();
                    if(addResultList.Count > 0 && removeResultList.Count > 0)
                    {
                        SelectablePackage addResult = addResultList[0];
                        SelectablePackage removeResult = removeResultList[0];
                        internallyRenamed.Add(new DatabaseBeforeAfter() { Before = removeResult, After = addResult });
                        addedPackages.Remove(addedPackages.Where(pack => pack.PackageName.Equals(addResult.PackageName)).ToList()[0]);
                        removedPackages.Remove(removedPackages.Where(pack => pack.PackageName.Equals(removeResult.PackageName)).ToList()[0]);
                    }
                }
            }

            //get a string list of name (without macro)
            List<string> nameWithMacro = addedSelectablePackages.Select(pack => pack.NameFormatted).ToList();
            nameWithMacro.AddRange(removedSelectablePackages.Select(pack => pack.NameFormatted).ToList());
            nameWithMacro = nameWithMacro.Distinct().ToList();
            ReportProgress(string.Format("Name count compare of add and remove is {0}", completePathDetect.Count));
            if(nameWithMacro.Count > 0)
            {
                //if the name exists in both, then it was moved and renamed
                foreach(string name in nameWithMacro)
                {
                    List<SelectablePackage> addResultList = addedSelectablePackages.Where(pack => pack.NameFormatted.Equals(name)).ToList();
                    List<SelectablePackage> removeResultList = removedSelectablePackages.Where(pack => pack.NameFormatted.Equals(name)).ToList();
                    if (addResultList.Count > 0 && removeResultList.Count > 0)
                    {
                        SelectablePackage addResult = addResultList[0];
                        SelectablePackage removeResult = removeResultList[0];
                        movedPackages.Add(new DatabaseBeforeAfter() { Before = removeResult, After = addResult });
                        renamedPackages.Add(new DatabaseBeforeAfter() { Before = removeResult, After = addResult });

                        for(int i = 0; i < addedPackages.Count; i++)
                        {
                            if(addedPackages[i] is SelectablePackage selectablePackage)
                            {
                                if(selectablePackage.NameFormatted.Equals(addResult.NameFormatted))
                                {
                                    addedPackages.RemoveAt(i);
                                    break;
                                }
                            }
                        }

                        for (int i = 0; i < removedPackages.Count; i++)
                        {
                            if (removedPackages[i] is SelectablePackage selectablePackage)
                            {
                                if (selectablePackage.NameFormatted.Equals(removeResult.NameFormatted))
                                {
                                    removedPackages.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            SetProgress(95);

            //list of disabled packages before
            List<DatabasePackage> disabledBefore = flatListOld.Where(p => !p.Enabled).ToList();

            //list of disabled packages after
            List<DatabasePackage> disabledAfter = flatListCurrent.Where(p => !p.Enabled).ToList();

            //compare except with after.before
            disabledPackages = disabledAfter.Except(disabledBefore, pc).ToList();

            //also need to remove and removed and added and disabled from updated
            updatedPackages = updatedPackages.Except(removedPackages, pc).ToList();
            updatedPackages = updatedPackages.Except(disabledPackages, pc).ToList();
            updatedPackages = updatedPackages.Except(addedPackages, pc).ToList();

            //put them to stringBuilder and write text to disk
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
            databaseUpdateText.AppendLine("New version tag: " + DatabaseUpdateVersion);

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
            ReportProgress("Updating databases");
            File.Delete(SelectModInfo.FileName);
            XmlUtils.SaveDatabase(SelectModInfo.FileName, Settings.WoTClientVersion, Settings.WoTModpackOnlineFolderVersion,
                globalDependencies, dependencies, parsedCategoryList, DatabaseXmlVersion.Legacy);

            //and save it in new form as well
            if ((bool)SaveV2StructureTest.IsChecked)
            {
                XmlUtils.SaveDatabase(RepoLatestDatabaseFolderPath, Settings.WoTClientVersion, Settings.WoTModpackOnlineFolderVersion,
                    globalDependencies, dependencies, parsedCategoryList, DatabaseXmlVersion.OnePointOne);
            }

            ReportProgress("Ready for step 4");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateDatabaseStep4_Click(object sender, RoutedEventArgs e)
        {
            ToggleUI((TabController.SelectedItem as TabItem), false);
            //check for stuff
            ReportProgress("Starting DatabaseUpdate step 4");

            //checks
            if (string.IsNullOrEmpty(Settings.WoTModpackOnlineFolderVersion))
            {
                ReportProgress("wot online folder version is empty");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if (!File.Exists(SelectModInfo.FileName))
            {
                ReportProgress("selectMofInfo file selected does not exist:" + SelectModInfo.FileName);
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if(string.IsNullOrEmpty(DatabaseUpdateVersion))
            {
                ReportProgress("DatabaseUpdateVersion is null");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }
            if(!File.Exists(ManagerVersionPath))
            {
                ReportProgress("manager_version.xml does not exist");
                ToggleUI((TabController.SelectedItem as TabItem), true);
                return;
            }

            //save and upload new modInfo file (will override the current name if exist[which it should unless new WoT client version supported])
            //make the name of current (to be supported) XML file for uploading later
            CurrentModInfoXml = "modInfo_" + Settings.WoTClientVersion + ".xml";
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                //upload new modINfo file
                ReportProgress("Saving and uploading new modInfo.xml to live server folder");
                await client.UploadFileTaskAsync(PrivateStuff.ModInfosLocation + CurrentModInfoXml, SelectModInfo.FileName);
                SetProgress(20);

                //upload manager_version.xml
                ReportProgress("Uploading new manager_version.xml to wotmods");
                await client.UploadFileTaskAsync(PrivateStuff.FTPManagerInfoRoot + Settings.ManagerVersion, ManagerVersionPath);
                SetProgress(40);

                //upload databaseUpdate.txt
                ReportProgress("uploading new databaseUpdate.txt");
                await client.UploadFileTaskAsync(PrivateStuff.FTPManagerInfoRoot + DatabaseUpdateFilename, DatabaseUpdatePath);
                SetProgress(60);
            }

            ReportProgress("Uploading new manager_version.xml to bigmods");
            using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
            {
                string completeURL = PrivateStuff.BigmodsFTPModpackManager + Settings.ManagerVersion;
                await client.UploadFileTaskAsync(completeURL, ManagerVersionPath);
            }

            SetProgress(80);

            //check if supported_clients.xml needs to be updated for a new version
            ReportProgress("Checking if supported_clients.xml needs to be updated for new WoT version");
            ReportProgress("Old version = " + LastSupportedTanksVersion + ", new version = " + Settings.WoTClientVersion);
            if (!LastSupportedTanksVersion.Equals(Settings.WoTClientVersion))
            {
                ReportProgress("DOES need to be updated/uploaded");
                XmlDocument supportedClients = XmlUtils.LoadXmlDocument(SupportedClientsPath, XmlLoadType.FromFile);
                XmlNode versionRoot = supportedClients.SelectSingleNode("//versions");
                XmlElement supported_client = supportedClients.CreateElement("version");
                supported_client.InnerText = Settings.WoTClientVersion;
                supported_client.SetAttribute("folder", Settings.WoTModpackOnlineFolderVersion);
                versionRoot.AppendChild(supported_client);
                supportedClients.Save(SupportedClientsPath);

                ReportProgress("Uploading new supported_clients.xml to wotmods");
                using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
                {
                    await client.UploadFileTaskAsync(PrivateStuff.FTPManagerInfoRoot + Settings.SupportedClients, SupportedClientsPath);
                }

                ReportProgress("Uploading new supported_clients.xml to bigmods");
                using (client = new WebClient() { Credentials = PrivateStuff.BigmodsNetworkCredential })
                {
                    await client.UploadFileTaskAsync(PrivateStuff.BigmodsFTPModpackManager + Settings.SupportedClients, SupportedClientsPath);
                }
                ReportProgress("Updated");
            }
            else
            {
                ReportProgress("DOES NOT need to be updated/uploaded");
            }
            SetProgress(100);
            ReportProgress("Done");
            ToggleUI((TabController.SelectedItem as TabItem), true);
        }

        private async void UpdateDatabaseStep5_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Starting update database step 5...");
            ReportProgress("Running script to create mod info");
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateModInfoPHP, 100000);
        }

        private async void UpdateDatabaseStep6a_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Starting Update database step 6a...");
            ReportProgress("Running script to create manager info (wotmods)");
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateManagerInfoPHP, 100000);
        }

        private async void UpdateDatabaseStep6b_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Starting Update database step 6a...");
            ReportProgress("Running script to create manager info (bigmods)");
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredentialScripts, PrivateStuff.BigmodsCreateManagerInfoPHP, 100000);
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
    }
}
