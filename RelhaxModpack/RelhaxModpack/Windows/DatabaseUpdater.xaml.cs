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
        #region Constants
        //get the sensitive constants (like this test below) from the PrivateStuff class
        private const string DatabaseUpdateTxt = "databaseUpdate.txt";
        private const string SupportedClients = "supported_clients.xml";
        private const string ManagerVersion = "manager_version.xml";
        private const string TrashXML = "trash.xml";
        private const string DatabaseXml = "database.xml";
        private const string MissingPackagesTxt = "missingPackages.txt";
        #endregion

        #region Editables
        private string KeyFilename = "key.txt";//can be overridden by command line arguement
        private WebClient client;
        private bool authorized = false;
        private OpenFileDialog SelectModInfo = new OpenFileDialog() { Filter = "*.xml|*.xml" };
        private OpenFileDialog SelectManyModInfo = new OpenFileDialog();
        private OpenFileDialog SelectManyZip = new OpenFileDialog();
        #endregion

        #region Stuff for parts 3 and 4 to share
        //strings
        //the modInfoXml document name to upload to the modInfo online folder
        //only used for uploading to the server TODO: can be moved to step 4 ONLY
        //RENAME TO NewModInfoXml
        string CurrentModInfoXml = "";
        //the last supported modInfo, gotten from the supported_clients.xml for comparing with the currentModInfoXml
        //LastSupportedModInfoXml is old name
        string LastSupportedModInfoXml = "";
        //the new database version to upload
        string DatabaseUpdateVersion = "";
        //the version number of the last supported WoT client, used for making backup online folder
        string LastSupportedTanksVersion = "";

        #endregion

        #region Stuff for Cleaning online folders
        public struct VersionInfos
        {
            public string WoTClientVersion;
            public string WoTOnlineFolderVersion;
            public override string ToString()
            {
                return string.Format("WoTClientVersion={0}, WoTOnlineFolderVersion={1}", WoTClientVersion, WoTOnlineFolderVersion);
            }
        }
        List<VersionInfos> VersionInfosList;
        VersionInfos selectedVersionInfos;
        bool cancelDelete = false;
        #endregion

        #region Constructor
        public DatabaseUpdater()
        {
            InitializeComponent();
        }
        #endregion

        #region Password auth stuff
        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PasswordButton_Click(sender, e);
        }

        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            AuthStatusTextblock.Text = "Current status: Checking...";
            AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Yellow);
            if(AttemptAuth())
            {
                AuthStatusTextblock.Text = "Current status: Authorized";
                AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Green);
                authorized = true;
            }
            else
            {
                AuthStatusTextblock.Text = "Current status: Denied";
                AuthStatusTextblock.Foreground = new SolidColorBrush(Colors.Red);
                authorized = false;
            }
        }

        private bool AttemptAuth()
        {
            //check if key filename was changed from command line
            if(!string.IsNullOrWhiteSpace(CommandLineSettings.UpdateKeyFileName))
            {
                Logging.Debug("User specified from command line new key filename to use: {0}", CommandLineSettings.UpdateKeyFileName);
                KeyFilename = CommandLineSettings.UpdateKeyFileName;
            }
            if (File.Exists(KeyFilename))
            {
                Logging.WriteToLog("Attempting to auth from AttemptAuth()");
                //compare local password to online version
                using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
                {
                    string onlinePassword = client.DownloadString(PrivateStuff.KeyAddress);
                    string localPassword = File.ReadAllText(KeyFilename);
                    if (onlinePassword.Equals(localPassword))
                    {
                        Logging.WriteToLog("authorized from AttemptAuth()");
                        return true;
                    }
                    else
                        Logging.WriteToLog("not authorized from AttemptAuth()");
                }
            }
            else
                Logging.Error("Key file {0} not found", KeyFilename);
            return false;
        }
        #endregion

        #region Boring stuff
        private void OnLoadModInfo(object sender, RoutedEventArgs e)
        {
            if (SelectModInfo.ShowDialog() == true)
            {
                LogOutput.Text = "Loading ModInfo...";
                //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
                //for the folder version: //modInfoAlpha.xml/@version
                Settings.WoTModpackOnlineFolderVersion = XMLUtils.GetXMLStringFromXPath(SelectModInfo.FileName, "//modInfoAlpha.xml/@onlineFolder");
                Settings.WoTClientVersion = XMLUtils.GetXMLStringFromXPath(SelectModInfo.FileName, "//modInfoAlpha.xml/@version");
                string versionInfo = string.Format("{0}={1},  {2}={3}", nameof(Settings.WoTModpackOnlineFolderVersion)
                    , Settings.WoTModpackOnlineFolderVersion, nameof(Settings.WoTClientVersion), Settings.WoTClientVersion);
                Logging.WriteToLog(versionInfo);
                ReportProgress(versionInfo);
                //this.Name = "DatabaseUpdater: " + versionInfo;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!authorized)
                AuthStatusTab.Focus();
        }

        private void ReportProgress(string message)
        {
            //reports to the log file and the console otuptu
            Logging.WriteToLog(message);
            LogOutput.AppendText(message + "\n");
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
            Logging.WriteToLog("Deleting trash files...");
            string[] filesToDelete = new string[]
            {
                SupportedClients,
                ManagerVersion,
                DatabaseXml,
                CurrentModInfoXml,
                LastSupportedModInfoXml
            };
            foreach (string s in filesToDelete)
            {
                if (!string.IsNullOrWhiteSpace(s) && File.Exists(s))
                    File.Delete(s);
            }
        }
        #endregion

        #region Database output
        private void SaveDatabaseText(bool @internal)
        {
            //true = internal, false = user
            string notApplicable = "n/a";
            //list creation and parsing
            List<Category> parsecCateogryList = new List<Category>();
            List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
            List<Dependency> dependencies = new List<Dependency>();
            XmlDocument doc = new XmlDocument();
            doc.Load(SelectModInfo.FileName);
            XMLUtils.ParseDatabase(doc, globalDependencies, dependencies, parsecCateogryList);
            //link stuff in memory or something
            Utils.BuildLinksRefrence(parsecCateogryList, false);
            //create variables
            StringBuilder sb = new StringBuilder();
            string saveLocation = @internal ? System.IO.Path.Combine(Settings.ApplicationStartupPath, "database_internal.csv") :
                System.IO.Path.Combine(Settings.ApplicationStartupPath, "database_user.csv");
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
            }
            catch (IOException)
            {
                ReportProgress("Failed to save in " + saveLocation + " (IOException, probably file open in another window)");
            }
        }
        private void DatabaseOutputStep2a_Click(object sender, RoutedEventArgs e)
        {
            //init
            LogOutput.Text = "";
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
            //init
            LogOutput.Text = "";
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

        #region Application update
        private async void UpdateApplicationStep7_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Text = "Running script to create update packages...";
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateUpdatePackagesPHP, 30 * Utils.TO_SECONDS);
        }

        private async void UpdateApplicationStep8_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Text = "Running script to create manager info...";
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateManagerInfoPHP, 30 * Utils.TO_SECONDS);
        }
        #endregion

        #region Cleaning online folders
        private async void CleanFoldersOnlineStep1_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Running Clean online folders step 1");
            ReportProgress("Downloading and parsing " + SupportedClients);
            //download supported_clients
            XmlDocument doc;
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                string xml = await client.DownloadStringTaskAsync(PrivateStuff.FTPManagerInfoRoot + SupportedClients);
                doc = XMLUtils.LoadXmlDocument(xml, XmlLoadType.FromXml);
            }
            //parse each online folder to list type string
            ReportProgress("Parsing " + SupportedClients);
            CleanFoldersOnlineStep2b.Items.Clear();
            XmlNodeList supportedClients = XMLUtils.GetXMLNodesFromXPath(doc, "//versions/version");
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
        }

        private async void CleanFoldersOnlineStep2_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Running Clean Folders online step 2");
            if(CleanFoldersOnlineStep2b.Items.Count == 0)
            {
                ReportProgress("Combobox items count = 0");
                return;
            }
            if(VersionInfosList == null)
            {
                ReportProgress("VersionsInfoList == null");
                return;
            }
            if(CleanFoldersOnlineStep2b.SelectedItem == null)
            {
                ReportProgress("Item not selected");
                return;
            }
            if(VersionInfosList.Count == 0)
            {
                ReportProgress("VersionsInfosList count = 0");
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
                        doc.LoadXml(await client.DownloadStringTaskAsync(Settings.BetaDatabaseURL));
                        string betaDatabaseOnlineFolderVersion = XMLUtils.GetXMLStringFromXPath(doc, Settings.DatabaseOnlineFolderXpath);
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
                if (!XMLUtils.ParseDatabase(doc, globalDependencies, dependencies, parsedCategoryList))
                {
                    ReportProgress("failed to parse modInfo to lists");
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
                return;
            }
            else if(notUsedFiles.Count == 0)
            {
                ReportProgress("No files to clean!");
            }
            else
            {
                CleanZipFoldersTextbox.AppendText(string.Join("\n", notUsedFiles));
                ReportProgress("Complete");
            }
        }

        private void CleanFoldersOnlineCancel_Click(object sender, RoutedEventArgs e)
        {
            cancelDelete = true;
        }

        private async void CleanFoldersOnlineStep3_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(CleanZipFoldersTextbox.Text))
            {
                ReportProgress("textbox is empty");
            }
            if(string.IsNullOrWhiteSpace(selectedVersionInfos.WoTOnlineFolderVersion))
            {
                ReportProgress("selectedVersionInfos is null");
                return;
            }
            cancelDelete = false;
            CleanFoldersOnlineCancelStep3.Visibility = Visibility.Visible;
            List<string> filesToDelete = new List<string>();
            filesToDelete = CleanZipFoldersTextbox.Text.Split('\n').ToList();
            string[] filesActuallyInFolder = await Utils.FTPListFilesFoldersAsync(string.Format("ftp://bigmods.relhaxmodpack.com/{0}/",
                selectedVersionInfos.WoTOnlineFolderVersion),PrivateStuff.BigmodsNetworkCredential);
            int count = 1;
            foreach(string s in filesToDelete)
            {
                if(cancelDelete)
                {
                    ReportProgress("Cancel Requested");
                    return;
                }
                if(!filesActuallyInFolder.Contains(s))
                {
                    ReportProgress(string.Format("skipping file {0}, does not exist", s));
                    count++;
                    continue;
                }
                ReportProgress(string.Format("Deleting file {0} of {1}, {2}", count++, filesToDelete.Count, s));
                await Utils.FTPDeleteFileAsync(string.Format("ftp://bigmods.relhaxmodpack.com/{0}/{1}",
                    selectedVersionInfos.WoTOnlineFolderVersion, s), PrivateStuff.BigmodsNetworkCredential);
            }
            CleanZipFoldersTextbox.Clear();
            CleanFoldersOnlineCancelStep3.Visibility = Visibility.Hidden;
            ReportProgress("Done");
        }
        #endregion

        #region Database Updating
        private async void UpdateDatabaseStep2_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Starting Update database step 2...");
            ReportProgress("Running script to update online hash database...");
            //a PatientWebClient should allow a timeout value of 5 mins (or more)
            await RunPhpScript(PrivateStuff.BigmodsNetworkCredential, PrivateStuff.UpdateDatabaseOnlinePHP, 30 * Utils.TO_SECONDS * Utils.TO_MINUETS);
        }
        
        
        private async void UpdateDatabaseStep3_Click(object sender, RoutedEventArgs e)
        {
            //getting local crcs and comparing them on server
            //init UI
            LogOutput.Clear();
            ReportProgress("Starting DatabaseUpdate step 3");

            //checks
            if(string.IsNullOrEmpty(Settings.WoTModpackOnlineFolderVersion))
            {
                ReportProgress("wot online folder version is empty");
                return;
            }
            if(!File.Exists(SelectModInfo.FileName))
            {
                ReportProgress("selectMofInfo file selected does not exist:" + SelectModInfo.FileName);
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

            //init strings
            CurrentModInfoXml = string.Empty;
            DatabaseUpdateVersion = string.Empty;
            LastSupportedModInfoXml = string.Empty;
            LastSupportedTanksVersion = string.Empty;

            //load current modInfoXml and check for duplicates
            ReportProgress("Loading current modInfo.xml and checking for duplicates");
            XmlDocument currentDatabase = new XmlDocument();
            currentDatabase.Load(SelectModInfo.FileName);
            if(!XMLUtils.ParseDatabase(currentDatabase,globalDependencies,dependencies,parsedCategoryList))
            {
                ReportProgress("failed to parse modInfo to lists");
                return;
            }
            Utils.BuildLinksRefrence(parsedCategoryList, false);
            Utils.BuildLevelPerPackage(parsedCategoryList);
            List<DatabasePackage> flatListCurrent = Utils.GetFlatList(globalDependencies, dependencies, null, parsedCategoryList);
            List<string> duplicates = Utils.CheckForDuplicates(globalDependencies, dependencies, parsedCategoryList);
            if(duplicates.Count > 0)
            {
                ReportProgress("ERROR: Duplicates found!");
                foreach (string s in duplicates)
                    ReportProgress(s);
                return;
            }
            ReportProgress("Downloading list of supported clients for last supported WoT client");

            //make the name of current (to be supported) XML file for uploading later (TODO: put this in later section, step 4?)
            CurrentModInfoXml = "modInfo_" + Settings.WoTClientVersion + ".xml";

            //make the name of the new database update version (TODO: put this in later section, step 4?)
            ReportProgress("Making new database version string");
            //download manager_version.xml to get the string value of latest database version
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                string dateTimeFormat = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
                string xml = await client.DownloadStringTaskAsync(PrivateStuff.FTPManagerInfoRoot + ManagerVersion);
                XmlDocument doc = XMLUtils.LoadXmlDocument(xml, XmlLoadType.FromXml);
                XmlNode database_version_text = doc.SelectSingleNode("//version/database");
                //database update text is like this: <WoTVersion>_<Date>_<itteration>
                int lastItteration = int.Parse(database_version_text.InnerText.Split('_')[2]);
                DatabaseUpdateVersion = string.Format("{0}_{1}_{2}.xml", Settings.WoTClientVersion, dateTimeFormat, ++lastItteration);
            }

            //download and parse supported_clients to make XML name of last supported wot version for comparison
            //legacy compatibility: keep this here to delete just in case
            if (File.Exists(SupportedClients))
                File.Delete(SupportedClients);
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                //now just load it from FTP string
                string xml = await client.DownloadStringTaskAsync(PrivateStuff.FTPManagerInfoRoot + SupportedClients);
                XmlDocument doc = XMLUtils.LoadXmlDocument(xml, XmlLoadType.FromXml);
                XmlNodeList supported_clients = doc.SelectNodes("//versions/version");
                //last version is most recent supported from supported_clients.xml
                XmlNode lastSupportred = supported_clients[supported_clients.Count - 1];
                LastSupportedTanksVersion = lastSupportred.InnerText;
                LastSupportedModInfoXml = "modInfo_" + LastSupportedTanksVersion + ".xml";
                //also delete this just in case
                if (File.Exists(LastSupportedModInfoXml))
                    File.Delete(LastSupportedModInfoXml);
                ReportProgress("Downloading last supported client modInfo.xml for database comparison");
                await client.DownloadFileTaskAsync(PrivateStuff.ModInfosLocation + LastSupportedModInfoXml, LastSupportedModInfoXml);
            }

            //make a flat list of old (last supported modInfoxml) for comparison
            //TODO: TEST WITH NEW CHNAGES
            XmlDocument oldDatabase = new XmlDocument();
            oldDatabase.Load(LastSupportedModInfoXml);
            //globalDependencies.Clear();
            //dependencies.Clear();
            //parsedCategoryList.Clear();
            List<DatabasePackage> globalDependenciesOld = new List<DatabasePackage>();
            List<Dependency> dependenciesOld = new List<Dependency>();
            List<Category> parsedCateogryListOld = new List<Category>();
            if(!XMLUtils.ParseDatabase(oldDatabase,globalDependenciesOld,dependenciesOld, parsedCateogryListOld))
            {
                ReportProgress("failed to parse modInfo to lists");
                return;
            }
            Utils.BuildLinksRefrence(parsedCateogryListOld, false);
            Utils.BuildLevelPerPackage(parsedCateogryListOld);
            List<DatabasePackage> flatListOld = Utils.GetFlatList(globalDependenciesOld, dependenciesOld, null, parsedCateogryListOld);
            //download and load latest database.xml file from server
            ReportProgress("downloading database.xml of current WoT onlineFolder version from server");
            XmlDocument databaseXml = new XmlDocument();
            using (client = new WebClient())
            {
                if(File.Exists(DatabaseXml))
                {
                    File.Delete(DatabaseXml);
                }
                await client.DownloadFileTaskAsync(string.Format("http://bigmods.relhaxmodpack.com/WoT/{0}/{1}",
                    Settings.WoTModpackOnlineFolderVersion, DatabaseXml), DatabaseXml);
                databaseXml.Load(DatabaseXml);
            }
            //update the crc values, also makes list of updated mods
            foreach(DatabasePackage package in flatListCurrent)
            {
                if(string.IsNullOrEmpty(package.ZipFile))
                    continue;
                ////database/file[@name="Sounds_HRMOD_Gun_Sounds_by_Zorgane_v2.01_1.2.0_2018-10-12.zip"]
                string xpathText = string.Format("//database/file[@name=\"{0}\"]",package.ZipFile);
                XmlNode databaseEntry = databaseXml.SelectSingleNode(xpathText);
                if(databaseEntry != null)
                {
                    string newCRC = databaseEntry.Attributes["md5"].Value;
                    if (string.IsNullOrWhiteSpace(newCRC))
                        throw new BadMemeException("newCRC string is null, and you suck at writing code");
                    if (!package.CRC.Equals(newCRC))
                    {
                        package.CRC = databaseEntry.Attributes["md5"].Value;
                        updatedPackages.Add(package);
                    }
                }
                else if (package.CRC.Equals("f") || string.IsNullOrWhiteSpace(package.CRC))
                {
                    missingPackages.Add(package);
                }
            }
            //do list magic to get all added, removed, disabled, etc package lists
            //used for disabled, removed, added mods
            PackageComparerByPackageName pc = new PackageComparerByPackageName();
            //if in before but not after = removed
            removedPackages = flatListOld.Except(flatListCurrent, pc).ToList();
            //if not in before but after = added
            addedPackages = flatListCurrent.Except(flatListOld, pc).ToList();
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
            ReportProgress(string.Format("Number of Added packages: {0}\nNumber of Updated packages: {1}\n" +
                "Number of Disabled packages: {2}\nNumber of Removed packages: {3}\n",
                addedPackages.Count, updatedPackages.Count, disabledPackages.Count, removedPackages.Count));
            //abort if missing files
            if(missingPackages.Count > 0)
            {
                if (File.Exists(MissingPackagesTxt))
                    File.Delete(MissingPackagesTxt);
                filesNotFoundSB.Clear();
                foreach (DatabasePackage package in missingPackages)
                    filesNotFoundSB.AppendLine(package.ZipFile);
                File.WriteAllText(MissingPackagesTxt, filesNotFoundSB.ToString());
                ReportProgress("ERROR: " + missingPackages.Count + " packages missing files! (saved to missingPackages.txt)");
                return;
            }

            //make stringBuilder of databaseUpdate.text
            databaseUpdateText.Clear();
            databaseUpdateText.Append("Database Update!\n\n");
            databaseUpdateText.Append("Updated: " + DatabaseUpdateVersion + "\n\n");
            databaseUpdateText.Append("Added:\n");
            foreach (DatabasePackage dp in addedPackages)
                databaseUpdateText.Append("-" + dp.CompletePath + "\n");
            databaseUpdateText.Append("\nUpdated:\n");
            foreach (DatabasePackage dp in updatedPackages)
                databaseUpdateText.Append("-" + dp.CompletePath + "\n");
            databaseUpdateText.Append("\nDisabled:\n");
            foreach (DatabasePackage dp in disabledPackages)
                databaseUpdateText.Append("-" + dp.CompletePath + "\n");
            databaseUpdateText.Append("\nRemoved:\n");
            foreach (DatabasePackage dp in removedPackages)
                databaseUpdateText.Append("-" + dp.CompletePath + "\n");
            databaseUpdateText.Append("\nNotes:\n-\n\n-----------------------------------------------------" +
                "---------------------------------------------------------------------------------------");

            //save databaseUpdate.txt
            File.WriteAllText(DatabaseUpdateTxt, databaseUpdateText.ToString());
            ReportProgress("Database text processed and written to disk");

            //save new modInfo.xml
            ReportProgress("Updating new modInfo.xml");
            File.Delete(SelectModInfo.FileName);
            XMLUtils.SaveDatabase(SelectModInfo.FileName, Settings.WoTClientVersion, Settings.WoTModpackOnlineFolderVersion,
                globalDependencies, dependencies, parsedCategoryList, DatabaseXmlVersion.Legacy);

            ReportProgress("Ready for step 4");
        }

        private async void UpdateDatabaseStep4_Click(object sender, RoutedEventArgs e)
        {
            //check for stuff
            LogOutput.Clear();
            ReportProgress("Starting DatabaseUpdate step 4");

            //checks
            if (string.IsNullOrEmpty(Settings.WoTModpackOnlineFolderVersion))
            {
                ReportProgress("wot online folder version is empty");
                return;
            }
            if (!File.Exists(SelectModInfo.FileName))
            {
                ReportProgress("selectMofInfo file selected does not exist:" + SelectModInfo.FileName);
                return;
            }
            if(string.IsNullOrEmpty(DatabaseUpdateVersion))
            {
                ReportProgress("DatabaseUpdateVersion is null");
                return;
            }
            if(string.IsNullOrWhiteSpace(LastSupportedModInfoXml))
            {
                ReportProgress("LastSupportedModInfoXml is null, empty, or whitespace");
                return;
            }
            if (!File.Exists(LastSupportedModInfoXml))
            {
                ReportProgress("LastSupportedModInfoXml does not exist (" + LastSupportedModInfoXml + ")");
                return;
            }
            if(!File.Exists(ManagerVersion))
            {
                ReportProgress("manager_version.xml does not exist");
                return;
            }

            //save and upload new modInfo file (will override the current name if exist[which it should unless new WoT client version supported])
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                ReportProgress("Saving and uploading new modInfo.xml to live server folder");
                await client.UploadFileTaskAsync(PrivateStuff.ModInfosLocation + CurrentModInfoXml, SelectModInfo.FileName);

                //upload supported_clients.xml if exist
                if (File.Exists(SupportedClients))
                {
                    ReportProgress("Uploading new supported_clients.xml");
                    await client.UploadFileTaskAsync(PrivateStuff.FTPManagerInfoRoot + SupportedClients, SupportedClients);
                }
                else
                {
                    ReportProgress("No new supported_clients.xml to upload");
                }

                //upload manager_version.xml
                ReportProgress("Uploading new manager_version.xml");
                await client.UploadFileTaskAsync(PrivateStuff.FTPManagerInfoRoot + ManagerVersion, ManagerVersion);
            }

            //check if supported_clients.xml needs to be updated for a new version
            ReportProgress("Checking if supported_clients.xml needs to be updated for new WoT version");
            if (!LastSupportedTanksVersion.Equals(Settings.WoTClientVersion))
            {
                ReportProgress("DOES need to be updated");
                using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
                {
                    ReportProgress("Old version = " + LastSupportedTanksVersion + ", new version = " + Settings.WoTClientVersion);
                    XmlDocument supportedClients = new XmlDocument();
                    supportedClients.Load(SupportedClients);
                    XmlNode versionRoot = supportedClients.SelectSingleNode("//versions");
                    XmlElement supported_client = supportedClients.CreateElement("version");
                    supported_client.InnerText = Settings.WoTClientVersion;
                    supported_client.SetAttribute("folder", Settings.WoTModpackOnlineFolderVersion);
                    versionRoot.AppendChild(supported_client);
                    File.Delete(SupportedClients);
                    supportedClients.Save(SupportedClients);
                    client.UploadFile(PrivateStuff.FTPManagerInfoRoot + SupportedClients, SupportedClients);
                }
                ReportProgress("Done");
            }
            else
            {
                ReportProgress("DOES NOT need to be uploaded, deleting (since no changes)");
                if (File.Exists(SupportedClients))
                    File.Delete(SupportedClients);
            }

            //update manager_info.xml
            ReportProgress("Updating manager_version.xml");
            using (client = new WebClient() { Credentials = PrivateStuff.WotmodsNetworkCredential })
            {
                client.DownloadFile(PrivateStuff.FTPManagerInfoRoot + ManagerVersion, ManagerVersion);
                XmlDocument doc = new XmlDocument();
                doc.Load(ManagerVersion);
                XmlNode database_version_text = doc.SelectSingleNode("//version/database");
                database_version_text.InnerText = DatabaseUpdateVersion;
                doc.Save(ManagerVersion);
                client.UploadFile(PrivateStuff.FTPManagerInfoRoot + ManagerVersion, ManagerVersion);
                File.Delete(ManagerVersion);
            }
            ReportProgress("Done");
        }

        private async void UpdateDatabaseStep6_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Starting update database step 6...");
            ReportProgress("Running script to create mod info");
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateModInfoPHP, 100000);
        }

        private async void UpdateDatabaseStep7_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Starting Update database step 7...");
            ReportProgress("Running script to create manager info");
            await RunPhpScript(PrivateStuff.WotmodsNetworkCredential, PrivateStuff.CreateManagerInfoPHP, 100000);
        }

        private void UpdateDatabasestep9_NA_ENG_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.eu/index.php?/topic/624499-");
        }

        private void UpdateDatabaseStep9_EU_ENG_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.eu/index.php?/topic/623269-");
        }

        private void UpdateDatabaseStep9_EU_GER_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.com/index.php?/topic/535868-");
        }
        #endregion

        #region Util methods

        private async Task RunPhpScript(NetworkCredential credentials, string URL, int timeoutMilliseconds)
        {
            using (client = new PatientWebClient()
            { Credentials = PrivateStuff.WotmodsNetworkCredential, Timeout = timeoutMilliseconds })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync(PrivateStuff.CreateManagerInfoPHP);
                    LogOutput.Text = result.Replace("<br />", "\n");
                }
                catch (WebException wex)
                {
                    ReportProgress("failed to run script");
                    ReportProgress(wex.ToString());
                }
            }
        }

        private async void GenerateMD5Button_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
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
                return;
            }
            foreach (string s in zipsToHash.FileNames)
            {
                ReportProgress(string.Format("hash of {0}:", System.IO.Path.GetFileName(s)));
                ReportProgress(await Utils.CreateMD5HashAsync(s));
            }
            ReportProgress("Done");
        }
        #endregion
    }
}
