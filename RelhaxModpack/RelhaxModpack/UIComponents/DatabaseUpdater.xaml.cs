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
        #region constants
        private const string DatabaseUpdateTxt = "databaseUpdate.txt";
        private const string DatabaseUpdateBackupTxt = "databaseUpdate.txt.bak";
        private const string KeyAddress = "TODO";
        private const string ModpackUsername = "modpack@wotmods.relhaxmodpack.com";
        private const string ModpackPassword = "QjFZLi0zaGxsTStY";
        private const string FTPRoot =                       "ftp://wotmods.relhaxmodpack.com/";
        private const string WotFolderRoot =                 "ftp://wotmods.relhaxmodpack.com/WoT/";
        private const string FTPModpackRoot =                "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/";
        private const string ModInfosLocation =              "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/modInfo/";
        private const string ModInfoBackupsFolderLocation =  "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/modInfoBackups/";
        private const string DatabaseBackupsFolderLocation = "ftp://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/databaseBackups/";
        private const string SupportedClients = "supported_clients.xml";
        private const string ManagerVersion = "manager_version.xml";
        #endregion
        #region editables
        private string KeyFilename = "key.txt";//can be overridden by command line arguement
        private WebClient client;
        private NetworkCredential @Credentials;
        private bool authorized = false;
        private OpenFileDialog SelectModInfo = new OpenFileDialog();
        private OpenFileDialog SelectManyModInfo = new OpenFileDialog();
        #endregion
        public DatabaseUpdater()
        {
            InitializeComponent();
        }
        #region password auth stuff
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

        #region database output
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

        #region cleaning folders
        private void CleanFoldersOnlineStep1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CleanFoldersOnlineStep2a_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CleanFoldersOnlineStep2b_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CleanFoldersOnlineStep3_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region boring stuff
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
    }
}
