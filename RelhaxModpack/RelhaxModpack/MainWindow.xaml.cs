using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RelhaxModpack.Windows;
using RelhaxModpack.UIComponents;
using System.Xml;
using System.Diagnostics;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace RelhaxModpack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon RelhaxIcon;
        private Stopwatch stopwatch = new Stopwatch();
        private ModSelectionList modSelectionList;
        private Stopwatch downloadTimer = new Stopwatch();
        private double last_download_time;
        private double current_download_time;
        private long last_bytes_downloaded;
        private long current_bytes_downloaded;
        private RelhaxProgress downloadProgress = null;
        private AdvancedProgress AdvancedProgressWindow;
        bool closingFromFailure = false;

        /// <summary>
        /// Creates the instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TheMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();
            //load please wait thing
            ProgressIndicator progressIndicator = new ProgressIndicator()
            {
                Message = Translations.GetTranslatedString("loadingTranslations"),
                ProgressMinimum = 0,
                ProgressMaximum = 4
            };
            progressIndicator.Show();
            progressIndicator.UpdateProgress(0);
            //load translation hashes and set default language
            Translations.SetLanguage(Languages.English);
            Translations.LoadTranslations();
            //apply translations to this window
            Translations.LocalizeWindow(this,true);
            //create tray icons and menus
            CreateTray();
            //load and apply modpack settings
            progressIndicator.UpdateProgress(2, Translations.GetTranslatedString("loadingSettings"));
            Settings.LoadSettings(Settings.ModpackSettingsFileName, typeof(ModpackSettings), ModpackSettings.PropertiesToExclude,null);
            //apply settings to UI elements
            UISettings.LoadSettings(true);
            UISettings.ApplyUIColorSettings(this);
            //check command line settings
            CommandLineSettings.ParseCommandLineConflicts();
            //apply third party settings
            ThirdPartySettings.LoadSettings(Settings.ThirdPartySettingsFileName);
            //verify folder paths
            progressIndicator.UpdateProgress(3, Translations.GetTranslatedString("folderStructure"));
            //verify folder stucture for all folders in the directory
            VerifyApplicationFolderStructure();
            //set the application appData direcotry
            Settings.AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Wargaming.net", "WorldOfTanks");
            if(!Directory.Exists(Settings.AppDataFolder))
            {
                Logging.WriteToLog(string.Format("AppDataFolder does not exist at {0}, creating it",Settings.AppDataFolder),
                    Logfiles.Application,LogLevel.Warning);
                Directory.CreateDirectory(Settings.AppDataFolder);
            }
            //Build application macros TODO

            //check for updates to database and application
            progressIndicator.UpdateProgress(4, Translations.GetTranslatedString("checkForUpdates"));
            CheckForApplicationUpdates();
            CheckForDatabaseUpdates(false, true);
            //get the number of processor cores
            MulticoreExtractionCoresCountLabel.Text = string.Format(Translations.GetTranslatedString("detectedCores"), Settings.NumLogicalProcesors);

            //dispose of please wait here
            progressIndicator.Close();
            progressIndicator = null;
            if(!closingFromFailure)
                Show();
        }

        private void TheMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!Logging.IsLogDisposed(Logfiles.Application))
            {
                if(Logging.IsLogOpen(Logfiles.Application))
                    Logging.WriteToLog("Saving settings");
                if (Settings.SaveSettings(Settings.ModpackSettingsFileName, typeof(ModpackSettings), ModpackSettings.PropertiesToExclude,null))
                    if (Logging.IsLogOpen(Logfiles.Application))
                        Logging.WriteToLog("Settings saved");
                if (Logging.IsLogOpen(Logfiles.Application))
                    Logging.WriteToLog("Disposing tray icon");
                if (RelhaxIcon != null)
                {
                    RelhaxIcon.Dispose();
                    RelhaxIcon = null;
                }
                Logging.DisposeLogging(Logfiles.Application);
            }
        }

        #region Tray code
        private void CreateTray()
        {
            //create base tray icon
            RelhaxIcon = new System.Windows.Forms.NotifyIcon()
            {
                Visible = true,
                Icon = Properties.Resources.modpack_icon,
                Text = Title
            };
            //create menu options
            //RelhaxMenustrip
            System.Windows.Forms.ContextMenuStrip RelhaxMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            //MenuItemRestore
            System.Windows.Forms.ToolStripMenuItem MenuItemRestore = new System.Windows.Forms.ToolStripMenuItem();
            MenuItemRestore.Name = nameof(MenuItemRestore);
            //MenuItemCheckUpdates
            System.Windows.Forms.ToolStripMenuItem MenuItemCheckUpdates = new System.Windows.Forms.ToolStripMenuItem();
            MenuItemCheckUpdates.Name = nameof(MenuItemCheckUpdates);
            //MenuItemAppClose
            System.Windows.Forms.ToolStripMenuItem MenuItemAppClose = new System.Windows.Forms.ToolStripMenuItem();
            MenuItemAppClose.Name = nameof(MenuItemAppClose);
            //build it
            RelhaxMenuStrip.Items.Add(MenuItemRestore);
            RelhaxMenuStrip.Items.Add(MenuItemCheckUpdates);
            RelhaxMenuStrip.Items.Add(MenuItemAppClose);
            RelhaxIcon.ContextMenuStrip = RelhaxMenuStrip;
            //setup the right click option
            RelhaxIcon.MouseClick += OnIconMouseClick;
            //setup each even option
            MenuItemRestore.Click += OnMenuItemRestoreClick;
            MenuItemCheckUpdates.Click += OnMenuClickChekUpdates;
            MenuItemAppClose.Click += OnMenuItemCloseClick;
        }

        private void OnMenuItemCloseClick(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
            Close();
        }

        private void OnMenuClickChekUpdates(object sender, EventArgs e)
        {
            //make and show progress indicator
            ProgressIndicator progressIndicator = new ProgressIndicator()
            {
                Message = Translations.GetTranslatedString("checkForUpdates"),
                ProgressMinimum = 0,
                ProgressMaximum = 1
            };
            progressIndicator.Show();
            CheckForDatabaseUpdates(false,false);
            //clean up progress inicaogr
            progressIndicator.Close();
            progressIndicator = null;
        }

        private void OnMenuItemRestoreClick(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Normal)
                WindowState = WindowState.Normal;
            //https://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf
            this.Activate();
        }

        private void OnIconMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Right:
                    //apply translations for each sub menu option
                    foreach(System.Windows.Forms.ToolStripMenuItem item in RelhaxIcon.ContextMenuStrip.Items)
                    {
                        item.Text = Translations.GetTranslatedString(item.Name);
                    }
                    break;
                case System.Windows.Forms.MouseButtons.Left:
                    //if the application is not displayed on the screen (minimized, for example), then show it.
                    if (WindowState != WindowState.Normal)
                        WindowState = WindowState.Normal;
                    //https://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf
                    this.Activate();
                    break;
            }
        }
        #endregion

        #region Update Code
        private void CheckForDatabaseUpdates(bool refreshManagerInfo, bool init)
        {
            Logging.WriteToLog("Checking for database updates");
            //TODO: consider just getting it from online?
            if(refreshManagerInfo)
            {
                //delete the last one and download a new one
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        if (File.Exists(Settings.ManagerInfoDatFile))
                            File.Delete(Settings.ManagerInfoDatFile);
                        client.DownloadFile("http://wotmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat", Settings.ManagerInfoDatFile);

                    }
                    catch (Exception e)
                    {
                        Logging.WriteToLog(string.Format("Failed to check for updates: \n{0}", e), Logfiles.Application, LogLevel.ApplicationHalt);
                        Application.Current.Shutdown();
                        Close();
                        return;
                    }
                }
            }
            //get the version info string
            string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");
            if (string.IsNullOrEmpty(xmlString))
            {
                Logging.WriteToLog("Failed to get get xml string from managerInfo.dat", Logfiles.Application, LogLevel.ApplicationHalt);
                return;
            }
            //load the document info
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            //get new DB update version
            string databaseNewVersion = XMLUtils.GetXMLStringFromXPath(doc, "//version/database");
            if(init)
            {
                //auto apply and don't annouce
                Settings.DatabaseVersion = databaseNewVersion;
            }
            else
            {
                Logging.WriteToLog(string.Format("Comparing database versions, old={0}, new={1}", Settings.DatabaseVersion, databaseNewVersion));
                //compare and annouce if not equal
                if (!Settings.DatabaseVersion.Equals(databaseNewVersion))
                {
                    Logging.WriteToLog("new version of database applied");
                    Settings.DatabaseVersion = databaseNewVersion;
                    DatabaseVersionLabel.Text = Translations.GetTranslatedString("databaseVersion") + " " + Settings.DatabaseVersion;
                    MessageBox.Show(Translations.GetTranslatedString("newDBApplied"));
                }
                else
                    Logging.WriteToLog("database versions are the same");
            }
            Logging.WriteToLog("Checking for database updates complete");
        }

        private void CheckForApplicationUpdates()
        {
            //check if skipping updates
            Logging.WriteToLog("Started check for application updates");
            if(CommandLineSettings.SkipUpdate && ModpackSettings.DatabaseDistroVersion != DatabaseVersions.Test)
            {
                MessageBox.Show(Translations.GetTranslatedString("skipUpdateWarning"));
                Logging.WriteToLog("Skipping updates", Logfiles.Application, LogLevel.Warning);
                return;
            }
            //delete the last one and download a new one
            using (WebClient client = new WebClient())
            {
                try
                {
                    if (File.Exists(Settings.ManagerInfoDatFile))
                        File.Delete(Settings.ManagerInfoDatFile);
                    client.DownloadFile("http://wotmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat", Settings.ManagerInfoDatFile);

                }
                catch (Exception e)
                {
                    Logging.WriteToLog(string.Format("Failed to check for updates: \n{0}", e), Logfiles.Application, LogLevel.ApplicationHalt);
                    MessageBox.Show(Translations.GetTranslatedString("failedCheckUpdates"));
                    closingFromFailure = true;
                    Application.Current.Shutdown();
                    Close();
                    return;
                }
            }
            //get the version info string
            string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");
            if(string.IsNullOrEmpty(xmlString))
            {
                Logging.WriteToLog("Failed to get get xml string from managerInfo.dat", Logfiles.Application, LogLevel.ApplicationHalt);
                Application.Current.Shutdown();
                Close();
                return;
            }
            //load the document info
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            //if the request distro version is alpha, correct it to stable
            if (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Alpha)
            {
                Logging.WriteToLog(nameof(ModpackSettings.ApplicationDistroVersion) + "is Alpha, setting to stable for safety",
                    Logfiles.Application, LogLevel.Debug);
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Stable;
            }
            //make a copy of the curent application version and set it to stable if alphs
            ApplicationVersions version = Settings.ApplicationVersion;
            if (version == ApplicationVersions.Alpha)
            {
                Logging.WriteToLog("temp version of " + nameof(Settings.ApplicationVersion) + " is Alpha, setting to stable for safety",
                    Logfiles.Application, LogLevel.Debug);
                version = ApplicationVersions.Stable;
            }
            //4 possibilities:
            //stable->stable (update check)
            //stable->beta (auto out of date)
            //beta->stable (auto out of date)
            //beta->beta (update check)
            bool outOfDate = false;
            //declare these out hereso the logger can access them
            string applicationBuildVersion = Utils.GetApplicationVersion();
            //if current application build does not equal requestion distribution channel
            if (version != ModpackSettings.ApplicationDistroVersion)
            {
                outOfDate = true;//can assume out of date
                Logging.WriteToLog(string.Format("Current build is {0} ({1}), online build is NA (changing distro version {2}->{3})",
                    applicationBuildVersion, version.ToString(), version.ToString(), ModpackSettings.ApplicationDistroVersion.ToString()));
            }
            else
            {
                //actually compare the bulid of the application of the requested distribution channel
                string applicationOnlineVersion = (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Stable) ?
                    XMLUtils.GetXMLStringFromXPath(doc, "//version/manager_v2") ://stable
                    XMLUtils.GetXMLStringFromXPath(doc, "//version/manager_beta_v2");//beta
                if (!(applicationBuildVersion.Equals(applicationOnlineVersion)))
                    outOfDate = true;
                Logging.WriteToLog(string.Format("Current build is {0} ({1}), online build is {2} ({3})",
                    applicationBuildVersion, version.ToString(), applicationOnlineVersion, ModpackSettings.ApplicationDistroVersion.ToString()));
            }
            if(!outOfDate)
            {
                Logging.WriteToLog("Application up to date");
                return;
            }
            Logging.WriteToLog("Application is out of date, display update window");
            VersionInfo versionInfo = new VersionInfo();
            versionInfo.ShowDialog();
            if(versionInfo.ConfirmUpdate)
            {
                //check for any other running instances
                while (true)
                {
                    System.Threading.Thread.Sleep(100);
                    if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                    {
                        MessageBoxResult result = MessageBox.Show(Translations.GetTranslatedString("closeInstanceRunningForUpdate"), Translations.GetTranslatedString("critical"), MessageBoxButton.OKCancel);
                        if (result != MessageBoxResult.OK)
                        {
                            Logging.WriteToLog("User canceled update, because he does not want to end the parallel running Relhax instance.");
                            Application.Current.Shutdown();
                            Close();
                            return;
                        }
                    }
                    else
                        break;
                }
                using (WebClient client = new WebClient())
                {
                    //start download of new version
                    client.DownloadProgressChanged += OnUpdateDownloadProgresChange;
                    client.DownloadFileCompleted += OnUpdateDownloadCompleted;
                    //set the UI for a download
                    ResetUI();
                    stopwatch.Reset();
                    //check to make sure this window is displayed for progress
                    if (WindowState != WindowState.Normal)
                        WindowState = WindowState.Normal;
                    //download the file
                    string modpackURL = (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Stable) ?
                        Settings.ApplicationUpdateURL :
                        Settings.ApplicationBetaUpdateURL;
                    //make sure to delte it if it's currently three
                    if (File.Exists(Settings.ApplicationUpdateFileName))
                        File.Delete(Settings.ApplicationUpdateFileName);
                    client.DownloadFileAsync(new Uri(modpackURL), Settings.ApplicationUpdateFileName);
                }
            }
            else
            {
                Logging.WriteToLog("User pressed x or said no");
                Application.Current.Shutdown();
                Close();
                return;
            }
        }

        private void ResetUI()
        {
            ChildProgressBar.Value = ParentProgressBar.Value = TotalProgressBar.Value = 0;
            InstallProgressTextBox.Text = string.Empty;
        }
        
        private void OnUpdateDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //stop the timer
            stopwatch.Reset();
            if(e.Error != null)
            {
                Logging.WriteToLog("Failed to download application update\n" + e.Error.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
                MessageBox.Show(Translations.GetTranslatedString("cantDownloadNewVersion"));
                Application.Current.Shutdown();
            }
            //try to extract the update
            try
            {
                using (ZipFile zip = ZipFile.Read(Settings.ApplicationUpdateFileName))
                {
                    zip.ExtractAll(Settings.ApplicationStartupPath);
                }
            }
            catch (ZipException zipex)
            {
                Logging.WriteToLog("Failed to extract update zip file\n" + zipex.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
                MessageBox.Show(Translations.GetTranslatedString("failedToExtractUpdateArchive"));
                Application.Current.Shutdown();
            }
            //extract the batch script to update the application
            string batchScript = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "RelicCopyUpdate.txt");
            File.WriteAllText(Settings.RelicBatchUpdateScript, batchScript);
            //try to start the update script
            try
            {
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = Path.Combine(Settings.ApplicationStartupPath,Settings.RelicBatchUpdateScript),
                    Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray())
                };
                Process installUpdate = new Process
                {
                    StartInfo = info
                };
                installUpdate.Start();
            }
            catch (Exception e3)
            {
                Logging.WriteToLog("Failed to start " + Settings.RelicBatchUpdateScript + "\n" + e3.ToString(),
                    Logfiles.Application, LogLevel.ApplicationHalt);
                MessageBox.Show(Translations.GetTranslatedString("cantStartNewApp"));
            }
            Application.Current.Shutdown();
        }

        private void OnUpdateDownloadProgresChange(object sender, DownloadProgressChangedEventArgs e)
        {
            //if it's in instant extraction mode, don't show download progress
            if (ModpackSettings.DownloadInstantExtraction)
                return;
            //if it's not running, start it
            if (!stopwatch.IsRunning)
                stopwatch.Start();
            //set the update progress bar
            ChildProgressBar.Value = e.ProgressPercentage;
            float MBDownloaded = (float)e.BytesReceived / (float)Utils.BYTES_TO_MBYTES;
            float MBTotal = (float)e.TotalBytesToReceive / (float)Utils.BYTES_TO_MBYTES;
            MBDownloaded = (float)Math.Round(MBDownloaded,2);
            MBTotal = (float)Math.Round(MBTotal,2);
            string downloadMessage = string.Format("{0} {1}MB {2} {3}MB", Translations.GetTranslatedString("downloadingUpdate"),
                MBDownloaded, Translations.GetTranslatedString("of"), MBTotal);
            InstallProgressTextBox.Text = downloadMessage;
        }
        #endregion

        #region All the dumb events for all the changing of settings
        private void OnSelectionViewChanged(object sender, RoutedEventArgs e)
        {
            //selection view code for each new view goes here
            if ((bool)SelectionDefault.IsChecked)
                ModpackSettings.ModSelectionView = SelectionView.DefaultV2;
            else if ((bool)SelectionLegacy.IsChecked)
                ModpackSettings.ModSelectionView = SelectionView.Legacy;
        }

        private void OnMulticoreExtractionChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.MulticoreExtraction = (bool)MulticoreExtractionCB.IsChecked;
        }

        private void OnCreateShortcutsChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.CreateShortcuts = (bool)CreateShortcutsCB.IsChecked;
        }

        private void OnSaveUserDataChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.SaveUserData = (bool)SaveUserDataCB.IsChecked;
        }

        private void OnClearWoTCacheChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ClearCache = (bool)ClearCacheCB.IsChecked;
        }

        private void OnClearLogFilesChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.DeleteLogs = (bool)ClearLogFilesCB.IsChecked;
        }

        private void OnCleanInstallChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.CleanInstallation = (bool)CleanInstallCB.IsChecked;
        }

        private void OnImmidateExtarctionChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.DownloadInstantExtraction = (bool)InstantExtractionCB.IsChecked;
        }

        private void OnShowInstallCompleteWindowChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ShowInstallCompleteWindow = (bool)ShowInstallCompleteWindowCB.IsChecked;
        }

        private void OnBackupModsChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.BackupModFolder = (bool)BackupModsCB.IsChecked;
        }

        private void OnPreviewLoadingImageChange(object sender, RoutedEventArgs e)
        {
            if ((bool)ThirdGuardsLoadingImageRB.IsChecked)
                ModpackSettings.GIF = LoadingGifs.ThirdGuards;
            else if ((bool)StandardImageRB.IsChecked)
                ModpackSettings.GIF = LoadingGifs.Standard;
        }

        private void OnForceManuelGameDetectionChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ForceManuel = (bool)ForceManuelGameDetectionCB.IsChecked;
        }

        private void OnInformIfNoNewDatabaseChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.NotifyIfSameDatabase = (bool)NotifyIfSameDatabaseCB.IsChecked;
        }

        private void OnSaveLastInstallChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.SaveLastSelection = (bool)SaveLastInstallCB.IsChecked;
        }

        private void OnUseBetaAppChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)UseBetaApplicationCB.IsChecked)
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Beta;
            else if (!(bool)UseBetaApplicationCB.IsChecked)
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Stable;
        }

        private async void OnUseBetaDatabaseChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)UseBetaDatabaseCB.IsChecked)
            {
                UseBetaDatabaseCB.IsEnabled = false;
                //get the branches. the default selected should be master
                UseBetaDatabaseBranches.IsEnabled = false;
                UseBetaDatabaseBranches.Items.Clear();
                UseBetaDatabaseBranches.Items.Add(Translations.GetTranslatedString("loadingBranches"));
                UseBetaDatabaseBranches.SelectedIndex = 0;
                string jsonText = string.Empty;
                using (PatientWebClient client = new PatientWebClient() { Timeout = 1500 })
                {
                    try
                    {
                        client.Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                        jsonText = await client.DownloadStringTaskAsync(Settings.BetaDatabaseBranchesURL);
                    }
                    catch (WebException wex)
                    {
                        Logging.Exception(wex.ToString());
                    }
                }
                if(string.IsNullOrWhiteSpace(jsonText))
                {
                    //just load master and call it good. it should always be there
                    UseBetaDatabaseBranches.Items.Clear();
                    UseBetaDatabaseBranches.Items.Add("master");
                    UseBetaDatabaseBranches.SelectedIndex = 0;
                    UseBetaDatabaseBranches.IsEnabled = true;
                    UseBetaDatabaseCB.IsEnabled = true;
                    return;
                }
                JArray root = null;
                try
                {
                    root = JArray.Parse(jsonText);
                }
                catch (JsonException jex)
                {
                    Logging.Exception(jex.ToString());
                    UseBetaDatabaseBranches.Items.Clear();
                    UseBetaDatabaseBranches.Items.Add("master");
                    UseBetaDatabaseBranches.SelectedIndex = 0;
                    UseBetaDatabaseBranches.IsEnabled = true;
                    UseBetaDatabaseCB.IsEnabled = true;
                    return;
                }
                List<string> branches = new List<string>();
                foreach(JObject branch in root.Children())
                {
                    JValue value = (JValue)branch["name"];
                    branches.Add((string)value.Value);
                }
                branches.Reverse();
                UseBetaDatabaseBranches.Items.Clear();
                foreach (string s in branches)
                    UseBetaDatabaseBranches.Items.Add(s);
                ModpackSettings.DatabaseDistroVersion = DatabaseVersions.Beta;
                //default to master selected
                UseBetaDatabaseBranches.SelectedIndex = 0;
                UseBetaDatabaseBranches.IsEnabled = true;
                UseBetaDatabaseCB.IsEnabled = true;
            }
            else
            {
                ModpackSettings.DatabaseDistroVersion = DatabaseVersions.Stable;
            }
        }

        private void OnDefaultBordersV2Changed(object sender, RoutedEventArgs e)
        {
            ModpackSettings.EnableBordersDefaultV2View = (bool)EnableBordersDefaultV2CB.IsChecked;
        }

        private void OnDefaultSelectColorChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.EnableColorChangeDefaultV2View = (bool)EnableColorChangeDefaultV2CB.IsChecked;
        }

        private void OnLegacyBordersChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.EnableBordersLegacyView = (bool)EnableBordersLegacyCB.IsChecked;
        }

        private void OnLegacySelectColorChenged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.EnableColorChangeLegacyView = (bool)EnableColorChangeLegacyCB.IsChecked;
        }

        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Translations.SetLanguage((Languages)LanguagesSelector.SelectedIndex);
            Translations.LocalizeWindow(this, true);
        }
        #endregion

        private void InstallModpackButton_Click(object sender, RoutedEventArgs e)
        {
            //toggle buttons and reset UI
            ResetUI();
            ToggleUIButtons(false);
            //settings for export mode
            if(ModpackSettings.ExportMode)
            {
                throw new BadMemeException("TODO");
            }
            else if (ModpackSettings.AutoInstall || ModpackSettings.OneClickInstall)
            {
                //load the custom selection file, if it exists
                if (!File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
                {
                    MessageBox.Show(Translations.GetTranslatedString("autoOneclickSelectionFileNotExist"));
                    ToggleUIButtons(true);
                    return;
                }
            }
            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Beta)
            {
                //if mods sync
                if (ModpackSettings.AutoInstall || ModpackSettings.OneClickInstall)
                {
                    MessageBox.Show(Translations.GetTranslatedString("noAutoOneclickWithBeta"));
                    ToggleUIButtons(true);
                    return;
                }
            }
            //parse WoT root directory
            Logging.WriteToLog("started looking for WoT root directory", Logfiles.Application, LogLevel.Debug);
            if(!Utils.AutoFindWoTDirectory(ref Settings.WoTDirectory) || ModpackSettings.ForceManuel)
            {
                Logging.WriteToLog("auto detect failed or user requests manual", Logfiles.Application, LogLevel.Debug);
                Microsoft.Win32.OpenFileDialog manualWoTFind = new Microsoft.Win32.OpenFileDialog()
                {
                    InitialDirectory = string.IsNullOrWhiteSpace(Settings.WoTDirectory) ? Settings.ApplicationStartupPath : Settings.WoTDirectory,
                    AddExtension = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "WorldOfTanks.exe|WorldOfTanks.exe",
                    Multiselect = false,
                    RestoreDirectory = true,
                    ValidateNames = true
                };
                if((bool)manualWoTFind.ShowDialog())
                {
                    Settings.WoTDirectory = manualWoTFind.FileName;
                }
                else
                {
                    Logging.WriteToLog("User Canceled installation");
                    ToggleUIButtons(true);
                    return;
                }
            }
            Settings.WoTDirectory = Path.GetDirectoryName(Settings.WoTDirectory);
            Logging.WriteToLog("Wot root directory parsed as " + Settings.WoTDirectory);
            //check to make sure the application is not in the same directory as the WoT install
            if (Settings.WoTDirectory.Equals(Settings.ApplicationStartupPath))
            {
                //display error and abort
                MessageBox.Show(Translations.GetTranslatedString("moveOutOfTanksLocation"));
                ToggleUIButtons(true);
                return;
            }
            //get the version of tanks in the format
            //of the res_mods version folder i.e. 0.9.17.0.3
            string versionTemp = XMLUtils.GetXMLStringFromXPath(Path.Combine(Settings.WoTDirectory, "version.xml"), "//version.xml/version");
            Settings.WoTClientVersion = versionTemp.Split('#')[0].Trim().Substring(2);
            //determine if current detected version of the game is supported
            //only if applicaition distro is not alhpa and databate distro is not test
            if (ModpackSettings.DatabaseDistroVersion != DatabaseVersions.Test)
            {
                //make an array of all the supported versions
                string supportedClientsXML = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "supported_clients.xml");
                if (string.IsNullOrWhiteSpace(supportedClientsXML))
                {
                    Logging.WriteToLog("Failed to parse supported_clients.xml from string from zipfile", Logfiles.Application, LogLevel.Exception);
                    MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " supported_clients.xml");
                    ToggleUIButtons(true);
                    return;
                }
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.LoadXml(supportedClientsXML);
                }
                catch (XmlException ex)
                {
                    Logging.WriteToLog("Failed to parse supported_clients.xml to xml\n" + ex.ToString(), Logfiles.Application, LogLevel.Exception);
                    MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " supported_clients.xml");
                    ToggleUIButtons(true);
                    return;
                }
                //copy inner text of each WoT version into a string array
                XmlNodeList supportedVersionsXML = XMLUtils.GetXMLNodesFromXPath(doc, "//versions/version");
                string[] supportedVersionsString = new string[supportedVersionsXML.Count];
                for (int i = 0; i < supportedVersionsXML.Count; i++)
                {
                    supportedVersionsString[i] = supportedVersionsXML[i].InnerText;
                    //see if this supported client version is the same as what was parsed to be the current client version
                    if (supportedVersionsXML[i].InnerText.Equals(Settings.WoTClientVersion))
                    {
                        //WoTClientVersions is already set, set the online folder
                        Settings.WoTModpackOnlineFolderVersion = supportedVersionsXML[i].Attributes["folder"].Value;
                    }

                }
                //check to see if array of supported clients cas the detected WoT client version
                if (Settings.ApplicationVersion != ApplicationVersions.Alpha && !supportedVersionsString.Contains(Settings.WoTClientVersion))
                {
                    //log and inform the user
                    Logging.WriteToLog("Detected client version is " + Settings.WoTClientVersion + ", not supported",
                        Logfiles.Application, LogLevel.Warning);
                    Logging.WriteToLog("Supported versions are: " + string.Join(", ", supportedVersionsString));
                    MessageBox.Show(string.Format("{0}: {1}\n{2}\n\n{3}:\n{4}", Translations.GetTranslatedString("detectedClientVersion"),
                        Settings.WoTClientVersion, Translations.GetTranslatedString("supportNotGuarnteed"),
                        Translations.GetTranslatedString("supportedClientVersions"), string.Join("\n", supportedVersionsString)),
                        Translations.GetTranslatedString("critical"));
                    //set the version and online folder to the last ones
                    Settings.WoTClientVersion = supportedVersionsXML[supportedVersionsXML.Count - 1].InnerText;
                    Settings.WoTModpackOnlineFolderVersion = supportedVersionsXML[supportedVersionsXML.Count - 1].Attributes["folder"].Value;
                }
                //if the user wants to, check if the database has actually changed
                if (ModpackSettings.NotifyIfSameDatabase)
                {
                    //get the instal llog for last installed database version
                    string installedfilesLogPath = Path.Combine(Settings.WoTDirectory, "logs", "installedRelhaxFiles.log");
                    if (File.Exists(installedfilesLogPath))
                    {
                        //use index 0 of array, index 18 of string array
                        string lastInstalledDatabaseVersion = File.ReadAllText(installedfilesLogPath).Split('\n')[0].Substring(18).Trim();
                        if (Settings.DatabaseVersion.Equals(lastInstalledDatabaseVersion))
                        {
                            if (MessageBox.Show(Translations.GetTranslatedString("DatabaseVersionsSameBody"), Translations.GetTranslatedString("DatabaseVersionsSameHeader"), MessageBoxButton.YesNo) == MessageBoxResult.No)
                            {
                                ToggleUIButtons(true);
                                return;
                            }
                        }
                    }
                    else
                    {
                        Logging.WriteToLog("installedRelhaxFiles.log does not exist, cannnot notify if same database");
                    }
                }
            }
            //show the mod selection list
            modSelectionList = new ModSelectionList();
            //https://stackoverflow.com/questions/623451/how-can-i-make-my-own-event-in-c
            modSelectionList.OnSelectionListReturn += ModSelectionList_OnSelectionListReturn;
            modSelectionList.Visibility = Visibility.Hidden;
            modSelectionList.Hide();
            //modSelectionList.WindowState = WindowState.Minimized;
            modSelectionList.ShowDialog();
            modSelectionList.WindowState = WindowState.Normal;
        }

        //https://stackoverflow.com/questions/623451/how-can-i-make-my-own-event-in-c
        private void ModSelectionList_OnSelectionListReturn(object sender, SelectionListEventArgs e)
        {
            if (e.ContinueInstallation)
            {
                OnBeginInstallation(new List<Category>(e.ParsedCategoryList), new List<Dependency>(e.Dependencies),new List<DatabasePackage>(e.GlobalDependencies));
                modSelectionList = null;
            }
            else
            {
                ToggleUIButtons(true);
            }
        }

        private void OnBeginInstallation(List<Category> parsedCategoryList, List<Dependency> dependencies, List<DatabasePackage> globalDependencies)
        {
            if (parsedCategoryList == null || dependencies == null || globalDependencies == null ||
                parsedCategoryList.Count == 0 || dependencies.Count == 0 || globalDependencies.Count == 0)
                throw new BadMemeException("You suck at starting installations LEARN2CODE");
            Logging.WriteToLog("Starting an installation (timer starts now)");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            //check if wot is running
            while (Utils.IsProcessRunning("TODO",Settings.WoTDirectory))
            {
                System.Threading.Thread.Sleep(100);
                //create window to determine if cancel, wait, kill TODO

            }
            //build macro hash for install?

            //perform dependency calculations
            //get a flat list of packages to install
            List<DatabasePackage> flatList = Utils.GetFlatList(null, null, null, parsedCategoryList);
            List<SelectablePackage> flatListSelect = new List<SelectablePackage>();
            //convert it to correct class type
            foreach (SelectablePackage sp in flatList)
                flatListSelect.Add(sp);
            Logging.Debug("starting Utils.CalculateDependencies()");
            List<Dependency> dependneciesToInstall = new List<Dependency>(Utils.CalculateDependencies(dependencies, flatListSelect));
            //make a flat list of all packages to install (including those without a zip file) for statistic data gathering
            if(ModpackSettings.AllowStatisticDataGather)
            {
                List<DatabasePackage> packagesToGather = new List<DatabasePackage>();
                packagesToGather.AddRange(globalDependencies.Where(globalDep => globalDep.Enabled).ToList());
                packagesToGather.AddRange(dependneciesToInstall.Where(dep => dep.Enabled).ToList());
                packagesToGather.AddRange(flatListSelect.Where(fl => fl.Enabled && fl.Checked).ToList());
                //https://stackoverflow.com/questions/13781468/get-list-of-properties-from-list-of-objects
                List<string> packageNamesToUpload = packagesToGather.Select(pack => pack.PackageName).ToList();
                //https://stackoverflow.com/questions/10292730/httpclient-getasync-with-network-credentials
                using (HttpClientHandler handler = new HttpClientHandler()
                {
                    Credentials = PrivateStuff.BigmodsNetworkCredential,
                    ClientCertificateOptions = ClientCertificateOption.Automatic,
                    PreAuthenticate = true
                })
                using (HttpClient client = new HttpClient(handler) { BaseAddress = new Uri(PrivateStuff.BigmodsDownloadStatURL) })
                {
                    //https://stackoverflow.com/questions/15176538/net-httpclient-how-to-post-string-value
                    FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("packageNames", string.Join(",",packageNamesToUpload))
                    });
                    //remove first await when running later, this is just for testing
                    Task.Run(async () => 
                    {
                        try
                        {
                            HttpResponseMessage result = await client.PostAsync("", content);
                            Logging.Debug("Statistic data HTTP response code: {0}", result.StatusCode.ToString());
                            if(!result.IsSuccessStatusCode)
                            {
                                Logging.Warning("Failed to send statistic data. Response code={0}, reason={1}", result.StatusCode.ToString(), result.ReasonPhrase);
                            }
                            string resultContent = await result.Content.ReadAsStringAsync();
                        }
                        catch(Exception ex)
                        {
                            Logging.Error("an error occured sending statistic data");
                            Logging.Error(ex.ToString());
                        }

                    });
                    //for debug as well
                }
            }
            //make a flat list of all packages to install that will actually be installed
            List<DatabasePackage> packagesToInstall = new List<DatabasePackage>();
            packagesToInstall.AddRange(globalDependencies.Where(globalDep => globalDep.Enabled && !string.IsNullOrWhiteSpace(globalDep.ZipFile)).ToList());
            packagesToInstall.AddRange(dependneciesToInstall.Where(dep => dep.Enabled && !string.IsNullOrWhiteSpace(dep.ZipFile)).ToList());
            List<SelectablePackage> selectablePackagesToInstall = flatListSelect.Where(fl => fl.Enabled && fl.Checked && !string.IsNullOrWhiteSpace(fl.ZipFile)).ToList();
            packagesToInstall.AddRange(selectablePackagesToInstall);
            //while we're at it let's make a list of packages that need to be downloaded
            List<DatabasePackage> packagesToDownload = packagesToInstall.Where(pack => pack.DownloadFlag).ToList();
            //and check if we need to actuall install anything lol
            if (selectablePackagesToInstall.Count == 0)
            {
                Logging.WriteToLog("no packages selected to install...");
                ResetUI();
                ToggleUIButtons(true);
                return;
            }
            //perform list install order calculations
            List<DatabasePackage>[] orderedPackagesToInstall = Utils.CreateOrderedInstallList(packagesToInstall);
            //we now have a list of enabled, checked and actual zip file mods that we are going to install based on install groups
            //log the time to process lists
            TimeSpan lastTime = stopwatch.Elapsed;
            Logging.WriteToLog(string.Format("Took {0} msec to process lists", stopwatch.ElapsedMilliseconds));
            //first, if we have downloads to do and doing them the standard way, then start processing them
            if(packagesToDownload.Count > 0 && !ModpackSettings.DownloadInstantExtraction)
            {
                Logging.WriteToLog("download while install = false and packages to download, starting ProcessDownloads()");
                ProcessDownloads(packagesToDownload);
                Logging.WriteToLog(string.Format("download time took {0} msec", stopwatch.Elapsed.TotalMilliseconds - lastTime.TotalMilliseconds));
                lastTime = stopwatch.Elapsed;
            }
            else if(packagesToDownload.Count > 0 && ModpackSettings.DownloadInstantExtraction)
            {
                Logging.WriteToLog("download while install = true and packages to download, starting ProcessDownloadsAsync()");
                ProcessDownloadsAsync(packagesToDownload);
            }
            //now let's start the install procedures
            //like if we need to make the advanced install window
            //but null it at all times
            if (AdvancedProgressWindow != null)
                AdvancedProgressWindow = null;
            if (ModpackSettings.AdvancedInstalProgress)
            {
                AdvancedProgressWindow= new AdvancedProgress();
                AdvancedProgressWindow.Show();
            }
            //make sure each trigger list for each package is unique
            foreach(DatabasePackage package in packagesToInstall)
            {
                //for debug, get the list of duplicates
                //https://stackoverflow.com/questions/3811464/how-to-get-duplicate-items-from-a-list-using-linq
                List<string> duplicates = package.Triggers.GroupBy(trigger => trigger).Where(trig => trig.Count() > 1).Select(trig => trig.Key).ToList();
                if(duplicates.Count > 0)
                {
                    //first make it distinct
                    package.Triggers = package.Triggers.Distinct().ToList();
                    Logging.Debug("Duplicate triggers found in package {0}:{1}", package.PackageName, string.Join(",", duplicates));
                }
            }

            //and create and link the install engine
            InstallerComponents.InstallEngine engine = new InstallerComponents.InstallEngine()
            {
                FlatListSelectablePackages = flatListSelect,
                OrderedPackagesToInstall = orderedPackagesToInstall,
                PackagesToInstall = packagesToInstall,
                AwaitCallback = false,
                ParsedCategoryList = parsedCategoryList,
                Dependencies = dependencies,
                GlobalDependencies = globalDependencies
            };
            engine.OnInstallProgress += Engine_OnInstallProgress;
            engine.OnInstallFinish += Engine_OnInstallFinish;
            //call the engine to install
        }

        private async void Engine_OnInstallFinish(object sender, InstallerComponents.RelhaxInstallFinishedEventArgs e)
        {
            if(e.ExitCodes == InstallerComponents.InstallerExitCodes.Success)
            {
                //get a list of all zip files in the database, compare it with the files in the download cache folder
                //get a list of zip files in the cache that aren't in the database, these are old and can be deleted
                List<string> zipFilesInDatabase = new List<string>();
                foreach (DatabasePackage package in Utils.GetFlatList(e.GlobalDependencies, e.Dependencies, null, e.ParsedCategoryList))
                    if(!string.IsNullOrWhiteSpace(package.ZipFile))
                        zipFilesInDatabase.Add(package.ZipFile);
                List<string> zipFilesInCache = Utils.DirectorySearch(Settings.RelhaxDownloadsFolder, SearchOption.TopDirectoryOnly, "*.zip").ToList();
                List<string> oldZipFilesNotInDatabase = zipFilesInCache.Except(zipFilesInDatabase).ToList();
                if(oldZipFilesNotInDatabase.Count > 0)
                {
                    //there are files to delete
                    //if ask if false, assume we are deleting old files
                    if(ModpackSettings.AskToDeleteCache)
                    {
                        DeleteOldCache oldCache = new DeleteOldCache();
                        if(!(bool)oldCache.ShowDialog())
                        {
                            return;
                        }
                    }
                    InstallProgressTextBox.Text = Translations.GetTranslatedString("DeletingOldCache");
                    await Task.Run(() =>
                    {
                        foreach (string zipfile in oldZipFilesNotInDatabase)
                            Utils.FileDelete(Path.Combine(Settings.RelhaxDownloadsFolder, zipfile));
                    });
                }
                if(ModpackSettings.ShowInstallCompleteWindow)
                {
                    InstallFinished installFinished = new InstallFinished();
                    installFinished.ShowDialog();
                }
                else
                {
                    MessageBox.Show(Translations.GetTranslatedString("InstallationFinished"));
                }
                InstallProgressTextBox.Text = string.Empty;
            }
            else
            {
                //explain why if failed
                //messagebox

                //and log
                Logging.WriteToLog(string.Format("Installer failed to install, exit code {0}\n{1}", e.ExitCodes.ToString(), e.ErrorMessage),
                    Logfiles.Application, LogLevel.Exception);
            }
        }

        private void Engine_OnInstallProgress(object sender, RelhaxInstallerProgress e)
        {
            if(ModpackSettings.AdvancedInstalProgress )
            {
                if(AdvancedProgressWindow == null)
                {
                    throw new BadMemeException("but how");
                }
                AdvancedProgressWindow.OnReportAdvancedProgress(e);
            }
            else
            {
                //standard progress

                //other reporting here
            }
        }

        //handles processing of downloads
        private async Task ProcessDownloadsAsync(List<DatabasePackage> packagesToDownload)
        {
            using (WebClient client = new WebClient())
            {
                int retryCount = 3;
                string fileToDownload = string.Empty;
                string fileToSaveTo = string.Empty;
                foreach (DatabasePackage package in packagesToDownload)
                {
                    retryCount = 3;
                    while(retryCount > 0)
                    {
                        fileToDownload = package.StartAddress + package.ZipFile + package.EndAddress;
                        fileToSaveTo = Path.Combine(Settings.RelhaxDownloadsFolder, package.ZipFile);
                        try
                        {
                            await client.DownloadFileTaskAsync(fileToDownload, fileToSaveTo);
                            retryCount = 0;
                            package.DownloadFlag = false;
                        }
                        catch(WebException ex)
                        {
                            Logging.WriteToLog(string.Format("Failed to download the file {0}, try {1} of {2}\n{3}", package.ZipFile, retryCount, 1,
                                ex.ToString()), Logfiles.Application, LogLevel.Error);
                            retryCount--;
                        }
                    }
                }
            }
        }

        private async void ProcessDownloads(List<DatabasePackage> packagesToDownload)
        {
            //remmeber this is on the UI thread so we can update the progress via this
            //and also update the UI info
            ParentProgressBar.Minimum = 0;
            ParentProgressBar.Maximum = packagesToDownload.Count;
            ParentProgressBar.Value = 0;
            using(WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                string fileToDownload = string.Empty;
                string fileToSaveTo = string.Empty;
                downloadProgress = new RelhaxProgress()
                {
                    ParrentCurrent = 0,
                    ParrentTotal = packagesToDownload.Count
                };
                foreach (DatabasePackage package in packagesToDownload)
                {
                    //increate it out here, not in the repeat loop
                    ParentProgressBar.Value++;
                    downloadProgress.ChildCurrentProgress = package.ZipFile;
                    bool retry = true;
                    while (retry)
                    {
                        fileToDownload = package.StartAddress + package.ZipFile + package.EndAddress;
                        fileToSaveTo = Path.Combine(Settings.RelhaxDownloadsFolder, package.ZipFile);
                        current_bytes_downloaded = 0;
                        last_bytes_downloaded = 0;
                        last_download_time = 0;
                        current_download_time = 0;
                        //restarting the time should be the last thing to happen before starting file download
                        //kind of like a timing constraint

                        downloadTimer.Restart();
                        try
                        {
                            await client.DownloadFileTaskAsync(fileToDownload, fileToSaveTo);
                            retry = false;
                            package.DownloadFlag = false;
                        }
                        catch (WebException ex)
                        {
                            Logging.WriteToLog("failed to download the file " + package.ZipFile + "\n" + ex.ToString(),
                                Logfiles.Application, LogLevel.Error);
                            //show abort retry ignore window TODO
                            MessageBoxResult result = MessageBox.Show(string.Format("{0} \"{1}\" {2}",
                                Translations.GetTranslatedString("failedToDownload1"),
                                package.ZipFile, Translations.GetTranslatedString("failedToDownload2")),
                                Translations.GetTranslatedString("failedToDownloadHeader"), MessageBoxButton.YesNoCancel);
                            switch(result)
                            {
                                case MessageBoxResult.Yes:
                                    //keep retry as true
                                    break;
                                case MessageBoxResult.No:
                                    //skip this file
                                    retry = false;
                                    break;
                                case MessageBoxResult.Cancel:
                                    //stop the installation alltogether
                                    //cancel token stuff TODO
                                    retry = false;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //update the ETA
            //ignore the first hit of this method, because the timer started while the connection was
            //setting up, and not actually downloading in constant stream
            if(current_bytes_downloaded + current_download_time + last_bytes_downloaded + last_download_time == 0)
            {
                //set a starting point for the "current" download timer value and size downloaded
                current_bytes_downloaded = e.BytesReceived;
                current_download_time = downloadTimer.Elapsed.TotalMilliseconds;
                return;
            }
            //otherwise use standard estimating procedures
            //set current to last and get new currents
            last_bytes_downloaded = current_bytes_downloaded;
            last_download_time = current_download_time;
            current_bytes_downloaded = e.BytesReceived;
            current_download_time = downloadTimer.Elapsed.TotalMilliseconds;
            //get the current bytes per millisecond
            double bytes_per_millisecond = (current_bytes_downloaded-last_bytes_downloaded) / (current_download_time-last_download_time);
            double bytes_per_second = bytes_per_millisecond / 1000;
            double kbytes_per_second = bytes_per_second / 1024;
            double mbytes_per_second = kbytes_per_second / 1024;
            //if we have a download rate, and a remaining size, then we can get a remaining time!
            double remaining_bytes = e.TotalBytesToReceive - e.BytesReceived;
            double remaining_milliseconds = remaining_bytes / bytes_per_millisecond;
            double remaining_seconds = remaining_milliseconds / 1000;
            ChildProgressBar.Maximum = e.TotalBytesToReceive;
            ChildProgressBar.Minimum = 0;
            ChildProgressBar.Value = e.BytesReceived;
            //also report to the download message process
            InstallProgressTextBox.Text = string.Format("{0} {1} {2} {3}\n{4}\n{5} {6} {7}\n{8} {9} {10}",
                Translations.GetTranslatedString("downloading"), TotalProgressBar.Value, Translations.GetTranslatedString("of"),
                TotalProgressBar.Maximum, downloadProgress.ChildCurrentProgress, e.BytesReceived / (1024 * 1024), Translations.GetTranslatedString("of"),
                e.TotalBytesToReceive / (1024 * 1024), "ETA:", remaining_seconds, Translations.GetTranslatedString("seconds"));
        }

        private bool VerifyApplicationFolderStructure()
        {
            Logging.WriteToLog("Verifying folder structure");
            foreach (string s in Settings.FoldersToCheck)
            {
                try
                {
                    if (!Directory.Exists(s))
                        Directory.CreateDirectory(s);
                }
                catch (Exception e)
                {
                    Logging.WriteToLog("Failed to check application folder structure\n" + e.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
                    return false;
                }
            }
            Logging.WriteToLog("Structure verified");
            return true;
        }

        private void ToggleUIButtons(bool toggle)
        {
            List<FrameworkElement> controlsToToggle = Utils.GetAllWindowComponentsLogical(this, false);
            //any to remove here
            if (controlsToToggle.Contains(CancelDownloadButton))
                controlsToToggle.Remove(CancelDownloadButton);
            foreach (FrameworkElement control in controlsToToggle)
            {
                if (control is Button || control is CheckBox || control is RadioButton)
                    control.IsEnabled = toggle;
            }
            //any to include here
            AutoSyncFrequencyTexbox.IsEnabled = toggle;
            AutoSyncSelectionFileTextBox.IsEnabled = toggle;
        }

        #region UI events
        private void UninstallModpackButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DiagnosticUtilitiesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ViewNewsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DumpColorSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog()
            {
                AddExtension = true,
                CheckPathExists = true,
                OverwritePrompt = true,
                RestoreDirectory = true,
                DefaultExt = "xml",
                Title = Translations.GetTranslatedString("ColorDumpSaveFileDialog"),
                Filter = "XML Documents|*.xml"
            };
            bool result = (bool)saveFileDialog.ShowDialog();
            if(result)
            {
                Logging.WriteToLog("Saving color settings dump to " + saveFileDialog.FileName);
                UISettings.DumpAllWindowColorSettingsToFile(saveFileDialog.FileName);
                Logging.WriteToLog("Color settings saved");
            }
        }

        private void OnBetaDatabaseSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UseBetaDatabaseBranches.SelectedItem is string branchName)
                ModpackSettings.BetaDatabaseSelectedBranch = branchName;
            else if (UseBetaDatabaseBranches.SelectedItem == null)
                ModpackSettings.BetaDatabaseSelectedBranch = "master";
            else
                throw new BadMemeException("aids. on a stick");
        }

        private void VerboseLoggingCB_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void DisableTriggersCB_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void AskDownloadCacheDelete_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void MinimizeToSystemTray_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void UninstallDefault_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void UninstallQuick_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void EnableModsAutoSyncCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void LoadAutoSyncSelectionFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ForceEnabledCB_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ForceVisibleCB_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ExportModeCB_Checked(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ExportMode = (bool)ExportModeCB.IsChecked;
        }

        private void SaveDisabledModsInSelection_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ThemeDefault_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
