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
        private const string DatabaseUpdateTxt = "databaseUpdate.txt";
        private const string DatabaseUpdateBackupTxt = "databaseUpdate.txt.bak";
        private const string KeyAddress = "aHR0cDovL3dvdG1vZHMucmVsaGF4bW9kcGFjay5jb20vUmVsaGF4TW9kcGFjay9SZXNvdXJjZXMvZXh0ZXJuYWwva2V5LnR4dA==";
        private const string ModpackUsername = "modpack@wotmods.relhaxmodpack.com";
        private const string ModpackPassword = "QjFZLi0zaGxsTStY";
        private const string FTPRoot =                       "ftp://wotmods.relhaxmodpack.com/";
        private const string FTPModpackRoot =                "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/";
        private const string FTPRescourcesRoot =             "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/";
        private const string FTPManagerInfoRoot =            "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/managerInfo/";
        private const string ModInfosLocation =              "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/modInfo/";
        private const string ModInfoBackupsFolderLocation =  "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/modInfoBackups/";
        private const string DatabaseBackupsFolderLocation = "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/databaseBackups/";
        private const string UpdateDatabaseOnlinePHP = "https://bigmods.relhaxmodpack.com/scripts/CreateDatabase.php";
        private const string SupportedClients = "supported_clients.xml";
        private const string ManagerVersion = "manager_version.xml";
        private const string TrashXML = "trash.xml";
        private const string DatabaseXml = "database.xml";
        private const int MaxFileSizeForHash = 510000000;
                                              
        #endregion

        #region Editables
        private string KeyFilename = "key.txt";//can be overridden by command line arguement
        private WebClient client;
        private NetworkCredential @Credentials;
        private bool authorized = false;
        private OpenFileDialog SelectModInfo = new OpenFileDialog() { Filter = "*.xml|*.xml" };
        private OpenFileDialog SelectManyModInfo = new OpenFileDialog();
        private OpenFileDialog SelectManyZip = new OpenFileDialog();
        //strings

        #endregion

        #region Stuff for parts 3 and 4 to share
        //strings
        //the modInfoXml document name to upload to the modInfo online folder
        //only used for uploading to the server TODO: can be moved to step 4 ONLY
        string currentModInfoXml = "";
        //the last supported modInfo, gotten from the supported_clients.xml for comparing with the currentModInfoXml
        string lastSupportedModInfoXml = "";
        //the new database version to upload
        string databaseUpdateVersion = "";
        //made from the above info, the name of the xml file of the last supported to be uploaded to the backups folder on the server
        string modInfoXmlToBackup = "";

        //string builders
        StringBuilder filesNotFoundSB;
        StringBuilder globalDepsSB;
        StringBuilder dependenciesSB;
        StringBuilder packagesSB;
        StringBuilder databaseUpdateText;
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
            //parse credentials
            Credentials = new NetworkCredential(ModpackUsername, Utils.Base64Decode(ModpackPassword));
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

        public bool AttemptAuth()
        {
            if(File.Exists(KeyFilename))
            {
                Logging.WriteToLog("Attempting to auth from AttemptAuth()");
                //compare local password to online version
                //NOTE: php script if passworded, does NOT need password itself to do stuff
                using(client = new WebClient(){ Credentials = Credentials })
                {
                    string onlinePassword = client.DownloadString(Utils.Base64Decode(KeyAddress));
                    string localPassword = File.ReadAllText(KeyFilename);
                    if(onlinePassword.Equals(localPassword))
                    {
                        Logging.WriteToLog("authorized from AttemptAuth()");
                        return true;
                    }
                    else
                        Logging.WriteToLog("not authorized from AttemptAuth()");
                }
            }
            return false;
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
            Utils.BuildLinksRefrence(parsecCateogryList);
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
            LogOutput.Text = "Running script CreateUpdatePackages.php...";
            //Replace("<br />", "\n")
            using (client = new WebClient() { Credentials = Credentials })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync("http://wotmods.relhaxmodpack.com/scripts/CreateUpdatePackages.php");
                    LogOutput.Text = result.Replace("<br />", "\n");
                }
                catch (WebException wex)
                {
                    ReportProgress("failed to run application upate step 8");
                    ReportProgress(wex.ToString());
                }
            }
        }

        private async void UpdateApplicationStep8_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Text = "Running script CreateManagerInfo.php...";
            using (client = new WebClient() { Credentials = Credentials })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync("http://wotmods.relhaxmodpack.com/scripts/CreateManagerInfo.php");
                    LogOutput.Text = result.Replace("<br />", "\n");
                }
                catch (WebException wex)
                {
                    ReportProgress("failed to run application update step 8");
                    ReportProgress(wex.ToString());
                }
            }
        }
        #endregion

        #region Cleaning online folders
        private async void CleanFoldersOnlineStep1_Click(object sender, RoutedEventArgs e)
        {
            //run trash xml collect script
            LogOutput.Text = "Running script CreateOutdatedFileList.php...";
            using (client = new WebClient() { Credentials = Credentials })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync("http://wotmods.relhaxmodpack.com/scripts/CreateOutdatedFileList.php");
                    LogOutput.Text = result.Replace("<br />", "\n");
                }
                catch (WebException wex)
                {
                    ReportProgress("failed to run clean folder online step 1");
                    ReportProgress(wex.ToString());
                }
            }
        }

        private async void CleanFoldersOnlineStep2a_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Downloading " + SupportedClients);
            //download supported_clients
            using(client = new WebClient() { Credentials = Credentials })
            {
                string xml = await client.DownloadStringTaskAsync(FTPManagerInfoRoot + SupportedClients);
            }
            //parse each online folder to list type string
            ReportProgress("Parsing " + SupportedClients);
            XmlNodeList supportedClients = XMLUtils.GetXMLNodesFromXPath(SupportedClients, "//versions/version");
            List<string> onlineFolders = new List<string>();
            foreach(XmlNode node in supportedClients)
            {
                string onlineFolderVersion = node.Attributes["folder"].Value;
                if(!onlineFolders.Contains(onlineFolderVersion))
                  onlineFolders.Add(onlineFolderVersion);
            }
            //clear it
            CleanFoldersOnlineStep2b.Items.Clear();
            foreach (string s in onlineFolders)
                CleanFoldersOnlineStep2b.Items.Add(s);
        }

        private void CleanFoldersOnlineStep3_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            //check if database versions set
            string wotFolderToClean = (string)CleanFoldersOnlineStep2b.SelectedItem;
            if (string.IsNullOrEmpty(wotFolderToClean))
            {
                ReportProgress("online folder version is blank");
                return;
            }
            //make sure folder exists
            ReportProgress(string.Format("Checking if folder {0} exists...", wotFolderToClean));
            string[] onlineFolders = FTPListFilesFolders("ftp://wotmods.relhaxmodpack.com/WoT/");
            if (!onlineFolders.Contains(wotFolderToClean))
            {
                ReportProgress(string.Format("ERROR: folder {0} does not exist in WoT folder!", wotFolderToClean));
                return;
            }
            //download trash xml file
            ReportProgress("Downloading " + TrashXML);
            string onlineFolderPath = "ftp://wotmods.relhaxmodpack.com/WoT/" + wotFolderToClean + "/";
            using (client = new WebClient() { Credentials = Credentials })
            {
                if (File.Exists(TrashXML))
                    File.Delete(TrashXML);
                client.DownloadFile(onlineFolderPath + TrashXML, TrashXML);
            }
            ReportProgress("Parsing " + TrashXML);
            XmlNodeList trashFiles = XMLUtils.GetXMLNodesFromXPath(TrashXML, "//trash/filename");
            if(trashFiles == null || trashFiles.Count == 0)
            {
                ReportProgress("Error: trashfiles is null or count = 0");
                return;
            }
            int totalFilesToDelete = trashFiles.Count;
            int filesDeleted = 1;
            //run the script with the selected online folder
            foreach (XmlNode file in trashFiles)
            {
                ReportProgress(string.Format("Deleting file {0} of {1}, filename={2}", filesDeleted++, totalFilesToDelete, file.InnerText));
                FTPDeleteFile(onlineFolderPath + file.InnerText);
            }
            ReportProgress("Complete");
            File.Delete(TrashXML);
        }
        #endregion

        #region FTP methods
        private void FTPMakeFolder(string addressWithDirectory)
        {
            WebRequest folderRequest = WebRequest.Create(addressWithDirectory);
            folderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            folderRequest.Credentials = Credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }
        
        private async void FTPMakeFolderAsync(string addressWithDirectory)
        {
            WebRequest folderRequest = WebRequest.Create(addressWithDirectory);
            folderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            folderRequest.Credentials = Credentials;
            using (FtpWebResponse webResponse = (FtpWebResponse) await folderRequest.GetResponseAsync())
            { }
        }

        private string[] FTPListFilesFolders(string address)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            folderRequest.Credentials = Credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string temp = reader.ReadToEnd();
                return temp.Split(new[] { "\r\n" }, StringSplitOptions.None);
            }
        }

        private async Task<string[]> FTPListFilesFoldersAsync(string address)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            folderRequest.Credentials = Credentials;
            using (FtpWebResponse response = (FtpWebResponse) await folderRequest.GetResponseAsync())
            {
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string temp = reader.ReadToEnd();
                return temp.Split(new[] { "\r\n" }, StringSplitOptions.None);
            }
        }

        private void FTPDeleteFile(string address)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = Credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }

        private async void FTPDeleteFileAsync(string address)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = Credentials;
            using (FtpWebResponse response = (FtpWebResponse) await folderRequest.GetResponseAsync())
            { }
        }

        private async Task<XmlNode> GetFilePropertiesAsync(string phpScriptAddress, string fileName, bool getMD5)
        {
            XmlDocument doc = new XmlDocument();
            using (client = new PatientWebClient() { Credentials = Credentials })
            {
                if(getMD5)
                {
                    Logging.WriteToLog("getMD5 is true, checking size before requesting hash", Logfiles.Application, LogLevel.Debug);
                    //get the size of the file first. if it's greator that 75 MB,
                    //download the file and get hash manually. should prevent timeout errors and manual editing
                    string parsedURLRequest = string.Format("{0}?folder={1}&file={2}&getMD5=0",
                        phpScriptAddress, Settings.WoTModpackOnlineFolderVersion, fileName);
                    string xmlString = await client.DownloadStringTaskAsync(parsedURLRequest);
                    try
                    {
                        doc.LoadXml(xmlString);
                    }
                    catch (XmlException ex)
                    {
                        Logging.WriteToLog(ex.ToString(), Logfiles.Application, LogLevel.Exception);
                        return null;
                    }
                    XmlNode fileProperties = doc.LastChild.LastChild;
                    XmlAttribute filePropertiesSize = fileProperties.Attributes["size"];
                    int fileSizeBytes = int.Parse(filePropertiesSize.Value);
                    if(fileSizeBytes > MaxFileSizeForHash)
                    {
                        //UPDATE UI
                        CancelDownloadButon.Visibility = Visibility.Visible;
                        Logging.WriteToLog("file size greator than limit, downloading for size", Logfiles.Application, LogLevel.Debug);
                        //http://wotmods.relhaxmodpack.com/WoT/
                        string fileDownloadURL = string.Format("http://{0}.relhaxmodpack.com/WoT/{1}/{2}",
                            (string)DomainSelectComboBox.SelectedItem, Settings.WoTModpackOnlineFolderVersion, fileName);
                        if (File.Exists(fileName))
                            File.Delete(fileName);
                        client.DownloadProgressChanged += OnDownloadProgress;
                        await client.DownloadFileTaskAsync(fileDownloadURL, fileName);
                        //get the actual md5 hash
                        string hash = Utils.CreateMD5Hash(fileName);
                        //append it to the node
                        XmlAttribute filePropertieshash = doc.CreateAttribute("MD5");
                        filePropertieshash.Value = hash;
                        fileProperties.Attributes.Append(filePropertieshash);
                        //and cleanup
                        File.Delete(fileName);
                        return fileProperties;
                    }
                    else
                    {
                        Logging.WriteToLog("file size smaller than limit, using online info for size", Logfiles.Application, LogLevel.Debug);
                        parsedURLRequest = string.Format("{0}?folder={1}&file={2}&getMD5=1",
                        phpScriptAddress, Settings.WoTModpackOnlineFolderVersion, fileName);
                        xmlString = await client.DownloadStringTaskAsync(parsedURLRequest);
                        try
                        {
                            doc = new XmlDocument();
                            doc.LoadXml(xmlString);
                        }
                        catch (XmlException ex)
                        {
                            Logging.WriteToLog(ex.ToString(), Logfiles.Application, LogLevel.Exception);
                            return null;
                        }
                        return doc.LastChild.LastChild;
                    }
                }
                else
                {
                    //only getting size and time, no timout issues
                    string parsedURLRequest = string.Format("{0}?folder={1}&file={2}&getMD5=0",
                        phpScriptAddress, Settings.WoTModpackOnlineFolderVersion, fileName);
                    string xmlString = await client.DownloadStringTaskAsync(parsedURLRequest);
                    try
                    {
                        doc.LoadXml(xmlString);
                    }
                    catch (XmlException ex)
                    {
                        Logging.WriteToLog(ex.ToString(), Logfiles.Application, LogLevel.Exception);
                        return null;
                    }
                    return doc.LastChild.LastChild;
                }
            }
        }

        private void OnDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            FileDownloadProgresBar.Minimum = 0;
            FileDownloadProgresBar.Value = e.ProgressPercentage;
            FileDownloadProgresBar.Maximum = 100;
            DownloadProgressText.Text = string.Format("{0}kb of {1}kb", e.BytesReceived/1024, e.TotalBytesToReceive/1024);
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
        #endregion

        #region Local zip file cleaning
        private List<string> zipFilesInDatabaseUse = new List<string>();
        private List<string> zipFilesInFolder = new List<string>();
        private List<string> zipFilesToDelete = new List<string>();
        private void CleanFoldersLocalStep1_Click(object sender, RoutedEventArgs e)
        {
            //init and checks
            LogOutput.Clear();
            if(SelectManyModInfo.ShowDialog() == false)
            {
                ReportProgress("User canceled");
                return;
            }
            zipFilesInDatabaseUse = new List<string>();
            List<DatabasePackage> packages = new List<DatabasePackage>();
            //get all packages to one list
            foreach(string s in SelectManyModInfo.FileNames)
            {
                List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
                List<Dependency> dependencies = new List<Dependency>();
                List<Category> categories = new List<Category>();
                XmlDocument doc = new XmlDocument();
                doc.Load(s);
                XMLUtils.ParseDatabase(doc, globalDependencies, dependencies, categories);
                packages.AddRange(globalDependencies);
                packages.AddRange(dependencies);
                foreach (Category cat in categories)
                    packages.AddRange(cat.GetFlatPackageList());
            }
            //parse to string list
            CleanZipFoldersTextbox.Clear();
            foreach(DatabasePackage databasePackage in packages)
            {
                if (!string.IsNullOrWhiteSpace(databasePackage.ZipFile) && !zipFilesInDatabaseUse.Contains(databasePackage.ZipFile))
                {
                    zipFilesInDatabaseUse.Add(databasePackage.ZipFile);
                    CleanZipFoldersTextbox.AppendText(databasePackage.ZipFile + "\n");
                }
            }
        }

        private void CleanFoldersLocalStep2_Click(object sender, RoutedEventArgs e)
        {
            if(SelectManyZip.ShowDialog() == false)
            {
                ReportProgress("canceled");
                return;
            }
            //make a new list of just zip files not in use
            //i.e. list of all files selected EXCEPT those in the modInfo selection
            zipFilesInFolder = SelectManyZip.FileNames.ToList();
            zipFilesToDelete = zipFilesInFolder.Except(zipFilesInDatabaseUse).ToList();
            CleanZipFoldersTextbox.Clear();
            foreach (string s in zipFilesToDelete)
                CleanZipFoldersTextbox.AppendText(s);
            ReportProgress("zips selected, ready for deleting");
        }

        private void CleanFoldersLocalStep3_Click(object sender, RoutedEventArgs e)
        {
            int numDeleted = 1;
            foreach(string s in zipFilesToDelete)
            {
                if(File.Exists(s))
                {
                    File.Delete(s);
                    ReportProgress(string.Format("Deleted {0} of {1}, {2}", numDeleted, zipFilesToDelete.Count, s));
                }
            }
        }
        #endregion

        #region Database Updating
        private async void UpdateDatabaseStep2PHP_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Starting Update database step 2...");
            ReportProgress("Running script CreateDatabase.php...");
            //custom credentials...
            //a PatientWebClient should allow a timeout value of 5 mins (or more)
            using (client = new PatientWebClient()
            { Credentials = new NetworkCredential(Utils.Base64Decode("dGVzdA=="), Utils.Base64Decode("dGVzdA==")) })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync(UpdateDatabaseOnlinePHP);
                    LogOutput.Text = result.Replace("<br />", "\n");
                }
                catch (WebException wex)
                {
                    ReportProgress("failed to run Update database step 2");
                    ReportProgress(wex.ToString());
                }
            }
        }
        
        private async void UpdateDatabaseStep2XML_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Starting database update step 2 (xml style)");
            //check for selected online folder version
            if(string.IsNullOrWhiteSpace(Settings.WoTModpackOnlineFolderVersion))
            {
                ReportProgress("string " + nameof(Settings.WoTModpackOnlineFolderVersion) + " is null or empty or whitespace");
                return;
            }
            //create actual base string to use for this (bigmods or wotmods?)
            //AND if its bigmods, confirm that you actually want to run this on bigmods
            //TODO verify index
            string currentSelectedDomain = (string)DomainSelectComboBox.SelectedItem;
            string bigmodsDomain = (string)DomainSelectComboBox.Items[0];//TODO
            if(currentSelectedDomain.Equals(bigmodsDomain))
            {
                if(MessageBox.Show("Are you sure you want to run xml update on bigmods and NOT use the php script??", "are you sure?", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    ReportProgress("Aborted");
                    return;
                }
            }
            //location to database.xml
            string databaseXMLLocation = string.Format("ftp://{0}.relhaxmodpack.com/WoT/{1}/{2}",
                currentSelectedDomain, Settings.WoTModpackOnlineFolderVersion, DatabaseXml);
            //location for script getZipFiles
            string getZipFilesURL = string.Format("http://{0}.relhaxmodpack.com/scripts/GetZipFiles.php?folder={1}",
                currentSelectedDomain, Settings.WoTModpackOnlineFolderVersion);
            //locatio for script getFileProperties
            string filePropertiesPHP = string.Format("http://{0}.relhaxmodpack.com/scripts/GetFileProperties.php", currentSelectedDomain);
            //download databaseInfo
            ReportProgress(string.Format("Loading database.xml from online folder {0}", Settings.WoTModpackOnlineFolderVersion));
            XmlDocument downloadedDatabaseXml = new XmlDocument();
            using (WebClient client = new WebClient() { Credentials = Credentials })
            {
                try
                {
                    string xmlString = await client.DownloadStringTaskAsync(databaseXMLLocation);
                    downloadedDatabaseXml.LoadXml(xmlString);
                }
                catch (Exception xmlx)
                {
                    ReportProgress("Failed");
                    ReportProgress(xmlx.ToString());
                    return;
                }
            }
            //make string list of file names from database.xml
            ReportProgress("Creating string list of names from database.xml");
            List<string> removed_files = new List<string>();
            XmlNode Database_elements = downloadedDatabaseXml.LastChild;
            foreach(XmlNode file in Database_elements.ChildNodes)
            {
                removed_files.Add(file.Attributes["name"].Value);
            }
            //get filelist
            //load xml string from php script of listing all files in folder
            ReportProgress("Loading list of files in online folder");
            XmlDocument files_in_folder = new XmlDocument();
            using (WebClient client = new WebClient() { Credentials = Credentials })
            {
                string xmlString = await client.DownloadStringTaskAsync(getZipFilesURL);
                files_in_folder.LoadXml(xmlString);
            }
            XmlNode zipFilesList = files_in_folder.LastChild;
            List<string> added_files = new List<string>();
            List<string> updated_files = new List<string>();
            List<string> error_files = new List<string>();
            //check vs time and size
            //for each file name:
            int progress = 0;
            int total = zipFilesList.ChildNodes.Count;
            StringBuilder summary = new StringBuilder();
            System.Diagnostics.Stopwatch time_for_each_file = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch total_time_php_processing = new System.Diagnostics.Stopwatch();
            total_time_php_processing.Restart();
            //verify UI is ready for the update process
            TotalUpdateProgressBar.Minimum = 0;
            TotalUpdateProgressBar.Value = TotalUpdateProgressBar.Minimum;
            TotalUpdateProgressBar.Maximum = total;
            FileDownloadProgresBar.Minimum = 0;
            FileDownloadProgresBar.Value = FileDownloadProgresBar.Minimum;
            FileDownloadProgresBar.Maximum = 1;
            foreach (XmlNode zipFileEntry in zipFilesList)
            {
                //always save progress so it's not lost
                downloadedDatabaseXml.Save("database.xml.bak");
                ReportProgress(string.Format("Parsing file {1} of {2} name={0}", zipFileEntry.Attributes["name"].Value, ++progress, total));
                //update the UI
                CancelDownloadButon.Visibility = Visibility.Hidden;
                DownloadProgressText.Text = "";
                FileDownloadProgresBar.Value = 0;
                TotalUpdateProgressBar.Value = progress;
                time_for_each_file.Restart();
                //if exists in string list of database.xml, remove it -> serves as removed files
                if (removed_files.Contains(zipFileEntry.Attributes["name"].Value))
                {
                    removed_files.Remove(zipFileEntry.Attributes["name"].Value);
                }
                //check if filename is in database.xml, if not, then it's new
                //attribute example: "//root/element/@attribute"
                XmlNode fileEntryInDownloadedDatabaseXml = downloadedDatabaseXml.SelectSingleNode(string.Format("//database/file[@name = \"{0}\"]", zipFileEntry.Attributes["name"].Value));
                //if the aboe is null, then it does not exist in the current database.xml, so we need to add it
                if (fileEntryInDownloadedDatabaseXml == null)
                {
                    //add
                    //get all file properties
                    XmlNode onlineFileProperties = null;
                    try
                    {
                        onlineFileProperties = await GetFilePropertiesAsync(filePropertiesPHP, zipFileEntry.Attributes["name"].Value, true);
                    }
                    catch (WebException ex)
                    {
                        ReportProgress("failed to update");
                        ReportProgress(ex.ToString());
                    }
                    if(onlineFileProperties == null)
                    {
                        string elapsed_time_add = string.Format(" Took {0}.{1} sec", (int)time_for_each_file.Elapsed.TotalSeconds, time_for_each_file.Elapsed.Milliseconds);
                        ReportProgress("Failed" + elapsed_time_add);
                        summary.AppendLine("[ERROR] " + zipFileEntry.Attributes["name"].Value);
                        continue;
                    }
                    //add node to database xml
                    //create element
                    fileEntryInDownloadedDatabaseXml = downloadedDatabaseXml.CreateElement("file");
                    //create attributes
                    XmlAttribute file_name = downloadedDatabaseXml.CreateAttribute("name");
                    file_name.Value = onlineFileProperties.Attributes["name"].Value;
                    XmlAttribute file_size = downloadedDatabaseXml.CreateAttribute("size");
                    file_size.Value = onlineFileProperties.Attributes["size"].Value;
                    XmlAttribute file_md5 = downloadedDatabaseXml.CreateAttribute("md5");
                    file_md5.Value = onlineFileProperties.Attributes["MD5"].Value;
                    //NEW: add the timestamp to database.xml
                    XmlAttribute file_time = downloadedDatabaseXml.CreateAttribute("time");
                    file_time.Value = onlineFileProperties.Attributes["time"].Value;
                    //add attributes to new element
                    fileEntryInDownloadedDatabaseXml.Attributes.Append(file_name);
                    fileEntryInDownloadedDatabaseXml.Attributes.Append(file_size);
                    fileEntryInDownloadedDatabaseXml.Attributes.Append(file_md5);
                    fileEntryInDownloadedDatabaseXml.Attributes.Append(file_time);
                    //add element to database xml
                    downloadedDatabaseXml.LastChild.AppendChild(fileEntryInDownloadedDatabaseXml);
                    //make UI and log updates
                    summary.AppendLine("[NEW] " + zipFileEntry.Attributes["name"].Value);
                    added_files.Add(zipFileEntry.Attributes["name"].Value);
                    string elapsed_time = string.Format(" Took {0}.{1} sec", (int)time_for_each_file.Elapsed.TotalSeconds, time_for_each_file.Elapsed.Milliseconds);
                    ReportProgress("Added" + elapsed_time);
                }
                else
                {
                    //update
                    //get file info without md5 check (faster)
                    bool force_md5_check = false;//change this when want to check slowly for all MD5 rather than just 
                    XmlNode onlineFileProperties = await GetFilePropertiesAsync(filePropertiesPHP, zipFileEntry.Attributes["name"].Value, false);
                    if (onlineFileProperties == null)
                    {
                        string elapsed_time_add = string.Format(" Took {0}.{1} sec", (int)time_for_each_file.Elapsed.TotalSeconds, time_for_each_file.Elapsed.Milliseconds);
                        ReportProgress("Failed" + elapsed_time_add);
                        summary.AppendLine("[ERROR] " + zipFileEntry.Attributes["name"].Value);
                        continue;
                    }
                    //legacy compatibility: if file time attribute is not there, then add it
                    XmlAttribute filePropertiesTimeDatabaseXml = fileEntryInDownloadedDatabaseXml.Attributes["time"];
                    if(filePropertiesTimeDatabaseXml == null)
                    {
                        filePropertiesTimeDatabaseXml = downloadedDatabaseXml.CreateAttribute("time");
                        filePropertiesTimeDatabaseXml.Value = onlineFileProperties.Attributes["time"].Value;
                        fileEntryInDownloadedDatabaseXml.Attributes.Append(filePropertiesTimeDatabaseXml);
                    }
                    //end legacy compatibility
                    //
                    XmlAttribute filePropertiesSizeDatabaseXml = fileEntryInDownloadedDatabaseXml.Attributes["size"];
                    XmlAttribute filePropertiesSizeOnline = onlineFileProperties.Attributes["size"];
                    XmlAttribute filePropertiesTimeOnline = onlineFileProperties.Attributes["time"];
                    //check if time, force md5 or size are not the same
                    if ((force_md5_check) ||
                        (!filePropertiesTimeDatabaseXml.Value.Equals(filePropertiesTimeOnline.Value)) ||
                        (!filePropertiesSizeDatabaseXml.Value.Equals(filePropertiesSizeOnline.Value)))
                    {
                        //get the script online file properties again but this time with the hash info
                        try
                        {
                            onlineFileProperties = await GetFilePropertiesAsync(filePropertiesPHP, zipFileEntry.Attributes["name"].Value, true);
                        }
                        catch(WebException exception)
                        {
                            ReportProgress("Failed to update");
                            ReportProgress(exception.ToString());
                            onlineFileProperties = null;
                        }
                        if (onlineFileProperties == null)
                        {
                            string elapsed_time_add = string.Format(" Took {0}.{1} sec", (int)time_for_each_file.Elapsed.TotalSeconds, time_for_each_file.Elapsed.Milliseconds);
                            ReportProgress("Failed" + elapsed_time_add);
                            summary.AppendLine("[ERROR] " + zipFileEntry.Attributes["name"].Value);
                            continue;
                        }
                        //set time and size equal to new time and size
                        filePropertiesSizeOnline = onlineFileProperties.Attributes["size"];
                        filePropertiesTimeOnline = onlineFileProperties.Attributes["time"];
                        filePropertiesTimeDatabaseXml.Value = filePropertiesTimeOnline.Value;
                        filePropertiesSizeDatabaseXml.Value = filePropertiesSizeOnline.Value;
                        //get the md5 properties
                        XmlAttribute filePropertiesHashOnline = onlineFileProperties.Attributes["MD5"];
                        XmlAttribute filePropertieshashDatabaseXml = fileEntryInDownloadedDatabaseXml.Attributes["md5"];
                        //update if MD5 not equal
                        if (filePropertieshashDatabaseXml == null || 
                            (!(filePropertiesHashOnline.Value.Equals(filePropertieshashDatabaseXml.Value))))
                        {
                            filePropertieshashDatabaseXml.Value = filePropertiesHashOnline.Value;
                            //add it to list of updated file names
                            updated_files.Add(zipFileEntry.Attributes["name"].Value);
                            summary.AppendLine("[UPDATE] " + zipFileEntry.Attributes["name"].Value);
                            string elapsed_time = string.Format(" Took {0}.{1} sec", (int)time_for_each_file.Elapsed.TotalSeconds, time_for_each_file.Elapsed.Milliseconds);
                            ReportProgress("Updated" + elapsed_time);
                        }
                        else
                        {
                            string elapsed_time = string.Format(" Took {0}.{1} sec", (int)time_for_each_file.Elapsed.TotalSeconds, time_for_each_file.Elapsed.Milliseconds);
                            ReportProgress("No Change (md5 check)" + elapsed_time);
                        }
                    }
                    else
                    {
                        string elapsed_time = string.Format(" Took {0}.{1} sec", (int)time_for_each_file.Elapsed.TotalSeconds, time_for_each_file.Elapsed.Milliseconds);
                        ReportProgress("No Change (time/size check)" + elapsed_time);
                    }
                }
            }
            string elapsed_time3 = string.Format(" took {0}.{1} sec", (int)total_time_php_processing.Elapsed.TotalSeconds, total_time_php_processing.Elapsed.Milliseconds);
            ReportProgress("Total php file processing" + elapsed_time3);
            foreach(string s in removed_files)
            {
                //delete the entry in the database
                XmlNode file_in_database_xml = downloadedDatabaseXml.SelectSingleNode(string.Format("//database/file[@name = \"{0}\"]", s));
                if(file_in_database_xml == null)
                {
                    //error
                }
                else
                {
                    Database_elements.RemoveChild(file_in_database_xml);
                }
                summary.AppendLine("[DELETE] " + s);
            }
            File.WriteAllText("update_files.log", summary.ToString());
            ReportProgress("results saved to update_files.log");
            downloadedDatabaseXml.Save("database.xml");
            downloadedDatabaseXml.Save("database.xml.bak");
            //upload it back
            using (WebClient client = new WebClient() { Credentials = Credentials })
            {
                try
                {
                    await client.UploadFileTaskAsync(databaseXMLLocation, "database.xml");
                    if (File.Exists("database.xml"))
                        File.Delete("database.xml");
                    ReportProgress("database.xml uploaded to wot folder " + Settings.WoTModpackOnlineFolderVersion);
                }
                catch (WebException ex)
                {
                    ReportProgress("Failed to upload back to server");
                    ReportProgress(ex.ToString());
                    ReportProgress("database.xml not deleted");
                }
            }
        }
        //string needed for step 4 go here

        private async void UpdateDatabaseStep3_Click(object sender, RoutedEventArgs e)
        {
            //getting local crc's and comparing them on server
            //init UI
            LogOutput.Clear();
            ReportProgress("Starting databaseUpdate step 3");
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
            //init stringbuilders
            filesNotFoundSB = new StringBuilder();
            globalDepsSB = new StringBuilder();
            dependenciesSB = new StringBuilder();
            packagesSB = new StringBuilder();
            databaseUpdateText = new StringBuilder();
            filesNotFoundSB.Append("FILES NOT FOUND:\n");
            globalDepsSB.Append("\nGlobal Dependencies updated:\n");
            dependenciesSB.Append("\nDependencies updated:\n");
            packagesSB.Append("\nPackages updated:\n");
            //init lists
            List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
            List<Dependency> dependencies = new List<Dependency>();
            List<Category> parsedCategoryList = new List<Category>();
            ReportProgress("downloading modInfo.xml from server");
            using (client = new WebClient() { Credentials = Credentials })
            {
                try
                {
                    await client.DownloadFileTaskAsync(string.Format("ftp://wotmods.relhaxmodpack.com/WoT/{0}/database.xml",
                        Settings.WoTModpackOnlineFolderVersion), "database.xml");
                }
                catch(WebException ex)
                {
                    ReportProgress("failed to download database.xml");
                    ReportProgress(ex.ToString());
                    return;
                }
            }
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(SelectModInfo.FileName);
            }
            catch (XmlException xmlx)
            {
                ReportProgress("failed to parse modInfo from xml");
                ReportProgress(xmlx.ToString());
                return;
            }
            if(!XMLUtils.ParseDatabase(doc,globalDependencies,dependencies,parsedCategoryList))
            {
                ReportProgress("failed to parse modInfo to lists");
                return;
            }
            List<DatabasePackage> flatList = Utils.GetFlatList(globalDependencies, dependencies, null, parsedCategoryList);
            //make the name of current (to be supported) xml file for uploading later (TODO: put this in later section, step 4?)
            //download and parse supported_clients to make xml name of last supported wot version for comparison
            //TODO: get new database update version here, confirm below with getting name to backup upload to server?
            //load it
            //make a flat list of it for comparison
            //load current modInfoXml and check for duplicates
            //check/create online backup folder for new wot versoin
            //make the name of the backup file as to not overwrite any currently backed up files
            //at some point check if the backup folder exists??
            //get the parsed update db version stirng (if not doing it above)
            //download and load latest database.xml file from server
            //parse the flat list packages from the current (to be supported) xml
            //this will updat ethe CRC values, add to list of updated packages, and add to list of missing packages
            //(packages zip files not existing in the zip file, therefore not registered on the server)
            //do list magic to get all added, removed, disabled, etc package lists
            //put them to stringbuilder, maybe put it to disk in temp filename?
        }

        private async void UpdateDatabaseStep4_Click(object sender, RoutedEventArgs e)
        {
            //backup the old live modInfo.xml
            //save and upload new modInfo file
            //if not dumped to temp file, dump/replace databaeUpdate.txt on disk
            //check if supported_clients needs to be updated and if so upload TODO: do this above??
            //update and upload manager_info.xml TODO: prepare it before??
        }

        private async void UpdateDatabaseStep5_Click(object sender, RoutedEventArgs e)
        {
            //upload databaseUpdate.txt
            LogOutput.Clear();
            ReportProgress("Starting update database step 5...");
            ReportProgress("Uploading databaseUpdate.txt");
            using (client = new WebClient() { Credentials = Credentials })
            {
                await client.UploadFileTaskAsync(FTPManagerInfoRoot + DatabaseUpdateTxt, DatabaseUpdateTxt);
            }
            ReportProgress("Uploaded sucessfully");
        }

        private async void UpdateDatabaseStep6_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Starting update database step 6...");
            ReportProgress("Running script CreateModInfo.php...");
            using (client = new WebClient() { Credentials = Credentials })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync("http://wotmods.relhaxmodpack.com/scripts/CreateModInfo.php");
                    LogOutput.Text = result.Replace("<br />", "\n");
                }
                catch (WebException wex)
                {
                    ReportProgress("failed to run Update database step 6");
                    ReportProgress(wex.ToString());
                }
            }
        }

        private async void UpdateDatabaseStep7_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Clear();
            ReportProgress("Starting Update database step 7...");
            ReportProgress("Running script CreateManagerInfo.php...");
            using (client = new WebClient() { Credentials = Credentials })
            {
                try
                {
                    string result = await client.DownloadStringTaskAsync("http://wotmods.relhaxmodpack.com/scripts/CreateManagerInfo.php");
                    LogOutput.Text = result.Replace("<br />", "\n");
                }
                catch (WebException wex)
                {
                    ReportProgress("failed to run Update database step 7");
                    ReportProgress(wex.ToString());
                }
            }
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
    
    }
}
