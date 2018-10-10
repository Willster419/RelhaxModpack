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
using System.Text;

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
        private const string KeyAddress = "TODO";
        private const string ModpackUsername = "modpack@wotmods.relhaxmodpack.com";
        private const string ModpackPassword = "QjFZLi0zaGxsTStY";
        private const string FTPRoot =                       "ftp://wotmods.relhaxmodpack.com/";
        private const string WotFolderRoot =                 "ftp://wotmods.relhaxmodpack.com/WoT/";
        private const string FTPModpackRoot =                "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/";
        private const string FTPRescourcesRoot =             "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/";
        private const string FTPManagerInfoRoot =            "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/managerInfo/";
        private const string ModInfosLocation =              "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/modInfo/";
        private const string ModInfoBackupsFolderLocation =  "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/modInfoBackups/";
        private const string DatabaseBackupsFolderLocation = "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/databaseBackups/";
        private const string SupportedClients = "supported_clients.xml";
        private const string ManagerVersion = "manager_version.xml";
        private const string TrashXML = "trash.xml";
        #endregion

        #region Editables
        private string KeyFilename = "key.txt";//can be overridden by command line arguement
        private WebClient client;
        private NetworkCredential @Credentials;
        private bool authorized = false;
        private OpenFileDialog SelectModInfo = new OpenFileDialog();
        private OpenFileDialog SelectManyModInfo = new OpenFileDialog();
        private OpenFileDialog SelectManyZip = new OpenFileDialog();
        #endregion

        public DatabaseUpdater()
        {
            InitializeComponent();
        }

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
        private void SaveDatabaseText(bool mode)
        {
            //true = internal, false = user
            //list creation and parsing
            List<Category> parsecCateogryList = new List<Category>();
            List<DatabasePackage> globalDependencies = new List<DatabasePackage>();
            List<Dependency> dependencies = new List<Dependency>();
            XmlDocument doc = new XmlDocument();
            doc.Load(SelectModInfo.FileName);
            XMLUtils.ParseDatabase(doc, globalDependencies, dependencies, parsecCateogryList);
            //create variables
            StringBuilder sb = new StringBuilder();
            string saveLocation = mode ? System.IO.Path.Combine(Settings.ApplicationStartupPath, "database_internal.csv") :
                System.IO.Path.Combine(Settings.ApplicationStartupPath, "database_user.csv");
            //global dependencies
            string header = mode ? "PackageName\tCategory\tPackage\tLevel\tZip\tDevURL\tEnabled\tVisible" : "Category\tMod\tDevURL";
            sb.AppendLine(header);
            foreach (DatabasePackage dp in globalDependencies)
            {
                sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", dp.PackageName, "GlobalDependencies", "", "0",
                    dp.ZipFile, string.IsNullOrWhiteSpace(dp.DevURL) ? "" : "=HYPERLINK(\"" + dp.DevURL + "\",\"link\")", dp.Enabled, ""));
            }
            foreach (Dependency dep in dependencies)
            {
                sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", dep.PackageName, "Dependencies", "", "0",
                    dep.ZipFile, string.IsNullOrWhiteSpace(dep.DevURL) ? "" : "=HYPERLINK(\"" + dep.DevURL + "\",\"link\")", dep.Enabled, ""));
            }
            foreach (Category cat in parsecCateogryList)
            {
                foreach (SelectablePackage sp in cat.GetFlatPackageList())
                {
                    string packageName = sp.PackageName;
                    if(!mode)
                    {
                        for (int i = 0; i <= sp.Level; i++)
                        {
                            packageName = "--" + packageName;
                        }
                    }
                    sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", packageName, sp.ParentCategory.Name, sp.NameFormatted,
                        sp.Level, sp.ZipFile, string.IsNullOrWhiteSpace(sp.DevURL) ? "" : "=HYPERLINK(\"" + sp.DevURL + "\",\"link\")",
                        sp.Enabled, sp.Visible));
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
        private void UpdateApplicationStep7_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Text = "Running script CreateUpdatePackages.php...";
            using (client = new WebClient() { Credentials = Credentials })
            {
                client.DownloadStringCompleted += OnDownloadStringComplete;
                client.DownloadStringAsync(new Uri("http://wotmods.relhaxmodpack.com/scripts/CreateUpdatePackages.php"));
            }
        }

        private void UpdateApplicationStep8_Click(object sender, RoutedEventArgs e)
        {
            LogOutput.Text = "Running script CreateManagerInfo.php...";
            using (WebClient client = new WebClient() { Credentials = Credentials })
            {
                client.DownloadStringCompleted += OnDownloadStringComplete;
                client.DownloadStringAsync(new Uri("http://wotmods.relhaxmodpack.com/scripts/CreateManagerInfo.php"));
            }
        }
        #endregion

        #region Cleaning online folders
        private async void CleanFoldersOnlineStep1_Click(object sender, RoutedEventArgs e)
        {
            //run trash xml collect script
            LogOutput.Text = "Running script CreateOutdatedFileList.php...";
            using (WebClient client = new WebClient() { Credentials = Credentials })
            {
                client.DownloadStringCompleted += OnDownloadStringComplete;
                await client.DownloadStringTaskAsync(new Uri("http://wotmods.relhaxmodpack.com/scripts/CreateOutdatedFileList.php"));
            }
        }

        private async void CleanFoldersOnlineStep2a_Click(object sender, RoutedEventArgs e)
        {
            ReportProgress("Downloading " + SupportedClients);
            //download supported_clients
            using(WebClient client = new WebClient() { Credentials = Credentials })
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

        private void FTPDeleteFile(string address)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = Credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
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
                this.Name = "DatabaseUpdater: " + versionInfo;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!authorized)
                AuthStatusTab.Focus();
        }

        private void OnDownloadStringComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            LogOutput.Text = e.Result.Replace("<br />", "\n");
        }

        private void ReportProgress(string message)
        {
            //reports to the log file and the console otuptu
            Logging.WriteToLog(message);
            LogOutput.AppendText(message + "\n");
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
    }
}
