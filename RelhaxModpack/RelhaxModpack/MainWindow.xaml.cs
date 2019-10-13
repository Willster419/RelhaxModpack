using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RelhaxModpack.Windows;
using RelhaxModpack.UIComponents;
using System.Xml;
using System.Diagnostics;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Timers;
using System.Threading;
using Timer = System.Timers.Timer;
using Microsoft.Win32;

namespace RelhaxModpack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon RelhaxIcon;
        private Stopwatch stopwatch = new Stopwatch();
        private ModSelectionList modSelectionList;
        private RelhaxProgress downloadProgress = null;
        private AdvancedProgress AdvancedProgressWindow;
        private bool closingFromFailure = false;
        private NewsViewer newsViewer = null;
        private WebClient client = null;
        private VersionInfo versionInfo = null;
        private Timer autoInstallTimer = new Timer();
        private bool databaseUpdateAvailableFromAutoSync = false;
        private bool autoInstallTimerRegistered = false;
        private CancellationTokenSource cancellationTokenSource;
        private InstallerComponents.InstallEngine installEngine;
        private OpenFileDialog FindTestDatabaseDialog = new OpenFileDialog()
        {
            AddExtension = true,
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            Title = "Select root database 2.0 file"
        };
        private DatabaseVersions databaseVersion;
        private bool loading = false;
        private string oldModpackTitle = string.Empty;
        //temp list of components not to toggle
        Control[] tempDisabledBlacklist = null;
        //backup components
        private bool disableTriggersBackupVal = true;
        private long backupFolderTotalSize = 0;
        private string[] backupFiles = null;
        //download ETA variables
        //measures elapsed time since download started
        private Stopwatch downloadTimer;
        //timer to fire every second to update the display download rate
        private Timer downloadDisplayTimer;
        //for download rate display, last internal's bytes downloaded
        private long lastBytesDownloaded;
        //for both rates, the current bytes downloaded
        private long currentBytesDownloaded;
        //for eta rate, the total byptes needed to download
        private long totalBytesToDownload;
        //download rate over the last second
        private double downloadRateDisplay;
        //remaining time
        private long remainingMilliseconds;

        /// <summary>
        /// The original width and height of the application before applying scaling
        /// </summary>
        public double OriginalWidth, OriginalHeight = 0;

        /// <summary>
        /// Creates the instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Minimized;
            tempDisabledBlacklist = new Control[]
            {
                ThemeDefault,
                ThemeDark,
                ThemeCustom,
                UseBetaApplicationCB,
                DisableTriggersCB,
                VerboseLoggingCB
            };
        }

        private async void TheMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //first hide the window
            //Hide();

            //set loading flag
            loading = true;

            //delete the updater scripts if they exist
#pragma warning disable CS0618
            foreach (string s in new string[] { Settings.RelicBatchUpdateScript, Settings.RelicBatchUpdateScriptOld })
            {
                if (File.Exists(s))
                {
                    Logging.Debug("{0} found, deleting", s);
                    File.Delete(s);
                }
            }
#pragma warning enable CS0618

            //get size of original width and height of window
            OriginalHeight = Height;
            OriginalWidth = Width;

            //load the progress report window
            ProgressIndicator progressIndicator = new ProgressIndicator()
            {
                Message = "Loading...",
                ProgressMinimum = 0,
                ProgressMaximum = 4
            };
            progressIndicator.Show();
            progressIndicator.UpdateProgress(0);
            Utils.AllowUIToUpdate();

            //load translations into combobox
            LanguagesSelector.Items.Clear();
            LanguagesSelector.Items.Add(Translations.LanguageEnglish);
            LanguagesSelector.Items.Add(Translations.LanguageFrench);
            LanguagesSelector.Items.Add(Translations.LanguageGerman);
            LanguagesSelector.Items.Add(Translations.LanguagePolish);
            LanguagesSelector.Items.Add(Translations.LanguageRussian);
            LanguagesSelector.Items.Add(Translations.LanguageSpanish);

            //load translation hashes and set default language
            Translations.LoadTranslations();
            Translations.SetLanguage(Languages.English);
            LanguagesSelector.SelectedIndex = 0;

            //apply translation settings after loading so it's at least in English
            Translations.LocalizeWindow(this, true);
            ApplyCustomUILocalizations(false);

            //create tray icons and menus
            CreateTray();

            //load and apply modpack settings
            Utils.AllowUIToUpdate();
            Settings.LoadSettings(Settings.ModpackSettingsFileName, typeof(ModpackSettings), ModpackSettings.PropertiesToExclude, null);

            //apply forced debugging settings
#warning forced debugging settings is active
            ModpackSettings.DisableTriggers = true;
            ModpackSettings.VerboseLogging = true;

            //apply settings to UI elements
            progressIndicator.UpdateProgress(2, Translations.GetTranslatedString("loadingSettings"));
            if(ModpackSettings.ApplicationTheme == UIThemes.Custom)
            {
                if (!UISettings.LoadSettingsFile())
                {
                    Logging.Warning("failed to load custom UI settings file, make sure file is called{0} and the xml syntax is correct", Settings.UISettingsColorFile);
                    ModpackSettings.ApplicationTheme = UIThemes.Default;
                }
                else
                {
                    Logging.Info("{0} was successfully load", Settings.UISettingsColorFile);
                }
            }

            //apply custom UI themeing (only need to explicitly call this for MainWindow)
            UISettings.ApplyCustomStyles(this);

            //note: if loadSettings load the language, apply to UI sets the UI option and triggers translation of MainWindow
            //note: in wpf, the enabled trigger will occur in the loading event, so this will launch the checked events
            ApplySettingsToUI();

            //check command line settings
            CommandLineSettings.ParseCommandLineConflicts();

            //save database version to temp and process if command line test mode
            databaseVersion = ModpackSettings.DatabaseDistroVersion;
            if (CommandLineSettings.TestMode)
            {
                Logging.Info("test mode set for installation only (not saved to settings)");
                databaseVersion = DatabaseVersions.Test;
            }

            //verify folder structure for all folders in the directory
            progressIndicator.UpdateProgress(3, Translations.GetTranslatedString("verDirStructure"));
            Utils.AllowUIToUpdate();
            Logging.Info("Verifying folder structure");
            foreach (string s in Settings.FoldersToCheck)
            {
                try
                {
                    if (!Directory.Exists(s))
                        Directory.CreateDirectory(s);
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog("Failed to check application folder structure\n" + ex.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
                    MessageBox.Show(Translations.GetTranslatedString("failedVerifyFolderStructure"));
                    closingFromFailure = true;
                    Application.Current.Shutdown();
                    return;
                }
            }
            Logging.Info("Structure verified");

            //set the application appData directory
            Settings.AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wargaming.net", "WorldOfTanks");
            if (!Directory.Exists(Settings.AppDataFolder))
            {
                Logging.WriteToLog(string.Format("AppDataFolder does not exist at {0}, creating it", Settings.AppDataFolder),
                    Logfiles.Application, LogLevel.Warning);
                Directory.CreateDirectory(Settings.AppDataFolder);
            }

            //check for updates to database and application
            progressIndicator.UpdateProgress(4, Translations.GetTranslatedString("checkForUpdates"));
            bool isApplicationUpToDate = await CheckForApplicationUpdates();
            if(!isApplicationUpToDate && versionInfo != null && !versionInfo.ConfirmUpdate)
            {
                Logging.Info("application is not up to date and user said don't update. we're done here.");
                Close();
                //https://stackoverflow.com/questions/57654546/taskcanceledexception-after-closing-window
                Environment.Exit(0);
                return;
            }
            CheckForDatabaseUpdates(false);

            //set the file count and size for the backups folder
            if (!isApplicationUpToDate)
            {
                Logging.Info("don't get file size of backups, application is not up to date");
            }
            else
            {
                Logging.Debug("starting async task of getting file sizes of backups");
                Task.Run(() =>
                {
                    backupFolderTotalSize = 0;
                    backupFiles = Utils.DirectorySearch(Settings.RelhaxModBackupFolderPath, SearchOption.TopDirectoryOnly, false, "*.zip", 5, 3, false);
                    foreach (string file in backupFiles)
                    {
                        backupFolderTotalSize += Utils.GetFilesize(file);
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BackupModsSizeLabelUsed.Text = string.Format(Translations.GetTranslatedString("BackupModsSizeLabelUsed"), backupFiles.Count(), Utils.SizeSuffix((ulong)backupFolderTotalSize, 1, true));
                    });
                    Logging.Debug("completed async task of getting file sizes of backups");
                });
            }

            Logging.Debug("checking if application is up to date");
            //if the application is up to date, then check if we need to display the welcome message to the user
            if (isApplicationUpToDate && !closingFromFailure)
            {
                Logging.Debug("application is up to date, checking to display welcome message");

                //run checks to see if it's the first time loading the application
                Settings.FirstLoad = !File.Exists(Settings.ModpackSettingsFileName) && !File.Exists(Settings.OldModpackSettingsFilename);
                Settings.FirstLoadToV2 = !File.Exists(Settings.ModpackSettingsFileName) && File.Exists(Settings.OldModpackSettingsFilename);
                Logging.Debug("FirstLoading = {0}, FirstLoadingV2 = {1}", Settings.FirstLoad.ToString(), Settings.FirstLoadToV2.ToString());

                if (Settings.FirstLoad || Settings.FirstLoadToV2)
                {
                    //display the welcome window and make sure the user agrees to it
                    FirstLoadAcknowledgments firstLoadAknowledgements = new FirstLoadAcknowledgments();
                    firstLoadAknowledgements.ShowDialog();
                    if (!firstLoadAknowledgements.UserAgreed)
                    {
                        Logging.Debug("user did not agree to application load conditions, closing");
                        Application.Current.Shutdown();
                        closingFromFailure = true;
                        return;
                    }

                    //if user agreed and its the first time loading in v2, the do the structure upgrade
                    else if (Settings.FirstLoadToV2)
                    {
                        progressIndicator.UpdateProgress(2, Translations.GetTranslatedString("upgradingStructure"));
                        Utils.AllowUIToUpdate();
                        Logging.Info("starting upgrade to V2");

                        //process libraries folder
                        Logging.Info("upgrade folders to new names");
#pragma warning disable CS0612
                        MoveUpgradeFolder(Settings.RelhaxDownloadsFolderPathOld, Settings.RelhaxDownloadsFolderPath);
                        MoveUpgradeFolder(Settings.RelhaxModBackupFolderPathOld, Settings.RelhaxModBackupFolderPath);
                        MoveUpgradeFolder(Settings.RelhaxUserSelectionsFolderPathOld, Settings.RelhaxUserSelectionsFolderPath);
                        MoveUpgradeFolder(Settings.RelhaxUserModsFolderPathOld, Settings.RelhaxUserModsFolderPath);
                        MoveUpgradeFolder(Settings.RelhaxTempFolderPathOld, Settings.RelhaxTempFolderPath);
                        MoveUpgradeFolder(Settings.RelhaxLibrariesFolderPathOld, Settings.RelhaxLibrariesFolderPath);
#pragma warning enable CS0612

                        //process xml settings file
                        //delete the new one, move the old one, reload settings
                        Logging.Info("moving, loading settings");
                        File.Move(Settings.OldModpackSettingsFilename, Settings.ModpackSettingsFileName);
                        Settings.LoadSettings(Settings.ModpackSettingsFileName, typeof(ModpackSettings), ModpackSettings.PropertiesToExclude, null);
                        ApplySettingsToUI();

                        //process log file
                        Logging.Info("moving and re-init of logging system");
                        if (File.Exists(Logging.OldApplicationLogFilename))
                        {
                            Logging.DisposeLogging(Logfiles.Application);
                            string tempNewLogText = File.ReadAllText(Logging.ApplicationLogFilename);
                            File.Delete(Logging.ApplicationLogFilename);
                            File.Move(Logging.OldApplicationLogFilename, Logging.ApplicationLogFilename);
                            File.AppendAllText(Logging.ApplicationLogFilename, tempNewLogText);
                            Logging.Init(Logfiles.Application, Logging.ApplicationLogFilename);
                        }
                        else
                            Logging.Info("skipped (old log does not exist)");
                        Logging.Info("upgrade to V2 complete, welcome to the future!");
                    }

                    //else process settings for first time load
                    else if (Settings.FirstLoad)
                    {
                        Logging.Info("running processes for first time loading");
                        Translations.SetLanguageOnFirstLoad();
                        ApplySettingsToUI();
                    }
                }
            }

            //save the old modpack title
            oldModpackTitle = Title;
            ProcessTitle();

            //if the editor unlock file exists, then enable the editor button
            if (File.Exists(Settings.EditorLaunchFromMainWindowFilename))
            {
                Logging.Info("{0} found, enabling editor button", Settings.EditorLaunchFromMainWindowFilename);
                LauchEditor.Visibility = Visibility.Visible;
                LauchEditor.IsEnabled = true;
            }
            else
            {
                LauchEditor.Visibility = Visibility.Hidden;
                LauchEditor.IsEnabled = false;
            }

            //dispose of please wait here
            if (progressIndicator != null)
            {
                progressIndicator.Close();
                progressIndicator = null;
            }

            //show the UI if not closing the application out of failure
            if (!closingFromFailure)
            {
                WindowState = WindowState.Normal;
                //get the current application scale
                //https://stackoverflow.com/questions/5022397/scale-an-entire-wpf-window
                //https://stackoverflow.com/questions/44683626/wpf-application-same-size-at-every-system-scale-scale-independent
                double currentScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

                //if display scale is 0, then set it to what it is currently
                if (ModpackSettings.DisplayScale == 0)
                    ModpackSettings.DisplayScale = currentScale;

                //if current scale is not target, then update
                if (ModpackSettings.DisplayScale != currentScale)
                {
                    Utils.ApplyApplicationScale(this, ModpackSettings.DisplayScale);
                }

                //apply to slider
                ApplyCustomScalingSlider.Value = ModpackSettings.DisplayScale;
                ApplyCustomScalingLabel.Text = string.Format("{0}x", ApplyCustomScalingSlider.Value.ToString("N"));

                //if silent start is selected, start the application minimized
                if (CommandLineSettings.SilentStart)
                {
                    Logging.Info("SilentStart found from command line, minimizing on startup");
                    WindowState = WindowState.Minimized;
                }
                //else if the auto-install option was set, immediately start the installation
                else if (!string.IsNullOrEmpty(CommandLineSettings.AutoInstallFileName))
                {
                    Logging.Info("auto-install specified to launch install using {0}", CommandLineSettings.AutoInstallFileName);
                    if (!File.Exists(Path.Combine(Settings.RelhaxUserSelectionsFolderPath, CommandLineSettings.AutoInstallFileName)))
                    {
                        Logging.Error("configuration file not found in {0}, aborting", Settings.RelhaxUserSelectionsFolderPath);
                        CommandLineSettings.AutoInstallFileName = string.Empty;
                    }
                    else
                    {
                        Logging.Info("file exists, launching modpack installation!");
                        InstallModpackButton_Click(null, null);
                    }
                }
                //loading in normal mode, check if atlas image processing libraries can be loaded
                else if(!ModpackSettings.AtlasLibrariesCanBeLoaded)
                {
                    Logging.Info("Atlas libraries never recorded being loaded, testing now via async task");
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);

                        ModpackSettings.AtlasLibrariesCanBeLoaded = Utils.TestLoadAtlasLibraries(true);
                        //if after test, it fails, inform the user
                        if (!ModpackSettings.AtlasLibrariesCanBeLoaded)
                        {
                            if (MessageBox.Show(string.Format("{0}\n{1}", Translations.GetTranslatedString("missingMSVCPLibraries"), Translations.GetTranslatedString("openLinkToMSVCP")),
                                Translations.GetTranslatedString("missingMSVCPLibrariesHeader"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                if (!Utils.StartProcess(Utils.MSVCPLink))
                                {
                                    Logging.Error("failed to open url to MSVCP: {0}", Utils.MSVCPLink);
                                }
                            }
                        }
                    });
                }

                //unset loading flag
                loading = false;
            }
        }

        private void TheMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //dispose of the timer if it's not already disposed
            if (autoInstallTimer != null)
            {
                autoInstallTimer.Dispose();
                autoInstallTimer = null;
            }
            if (ModpackSettings.MinimizeToSystemTray)
            {
                Logging.Debug("minimizing to system try");
                Hide();
                e.Cancel = true;
            }
            else
            {
                if (!Logging.IsLogDisposed(Logfiles.Application))
                {
                    if (Logging.IsLogOpen(Logfiles.Application))
                        Logging.Info("Saving settings");
                    if (!closingFromFailure)
                        if (Settings.SaveSettings(Settings.ModpackSettingsFileName, typeof(ModpackSettings), ModpackSettings.PropertiesToExclude, null))
                            if (Logging.IsLogOpen(Logfiles.Application))
                                Logging.Info("Settings saved");
                    if (Logging.IsLogOpen(Logfiles.Application))
                        Logging.Info("Disposing tray");
                    if (RelhaxIcon != null)
                    {
                        RelhaxIcon.Dispose();
                        RelhaxIcon = null;
                        if (Logging.IsLogOpen(Logfiles.Application))
                            Logging.Info("Tray disposed");
                    }
                    else
                    {
                        if (Logging.IsLogOpen(Logfiles.Application))
                            Logging.Info("Tray already null");
                    }
                }
                Application.Current.Shutdown(0);
            }
        }

        private void ProcessTitle()
        {
            if(string.IsNullOrEmpty(oldModpackTitle))
            {
                Logging.Info("oldModpackTitle is empty, don't update text!");
                return;
            }
            //apply the title change for beta application and beta database
            if (databaseVersion != DatabaseVersions.Stable)
                Title = string.Format("{0} ({1} DB)", oldModpackTitle, databaseVersion.ToString());
            else
                Title = oldModpackTitle;

            if (ModpackSettings.ApplicationDistroVersion != ApplicationVersions.Stable)
            {
                //if it's real alpha, then put alpha
                if (Settings.TrueAlpha)
                    Title = string.Format("{0} ({1} APP)", Title, Settings.ApplicationVersion.ToString());
                else
                    Title = string.Format("{0} ({1} APP)", Title, ModpackSettings.ApplicationDistroVersion.ToString());
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
            CheckForDatabaseUpdatesPeriodic(false);
        }

        private void CheckForDatabaseUpdatesPeriodic(bool quiet)
        {
            Logging.Info("starting periodic check for database updates");
            databaseUpdateAvailableFromAutoSync = false;
            if (!quiet)
            {
                //make and show progress indicator
                ProgressIndicator progressIndicator = new ProgressIndicator()
                {
                    Message = Translations.GetTranslatedString("checkForUpdates"),
                    ProgressMinimum = 0,
                    ProgressMaximum = 1
                };
                progressIndicator.Show();
                CheckForDatabaseUpdates(true);
                //clean up progress indicator
                progressIndicator.Close();
            }
            else
            {
                CheckForDatabaseUpdates(true);
            }
            Logging.Info("database periodic check complete, result of update = {0}", databaseUpdateAvailableFromAutoSync);
        }

        private void OnMenuItemRestoreClick(object sender, EventArgs e)
        {
            Restore();
        }

        private void OnIconMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Right:
                    //apply translations for each sub menu option
                    foreach (System.Windows.Forms.ToolStripMenuItem item in RelhaxIcon.ContextMenuStrip.Items)
                    {
                        item.Text = Translations.GetTranslatedString(item.Name);
                    }
                    break;
                case System.Windows.Forms.MouseButtons.Left:
                    Restore();
                    break;
            }
        }

        private void Restore()
        {
            if (ModpackSettings.MinimizeToSystemTray)
            {
                if (Visibility == Visibility.Hidden)
                    this.Show();
                else
                    this.Activate();
            }
            else
            {
                //https://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf
                this.Activate();
            }
            //if the application is not displayed on the screen (minimized, for example), then show it.
            if (WindowState != WindowState.Normal)
                WindowState = WindowState.Normal;
        }
        #endregion

        #region Update Code
        private async void CheckForDatabaseUpdates(bool refreshModInfo)
        {
            Logging.Info("Checking for database updates in CheckForDatabaseUpdates()");

            //if we are getting a new ModInfo then do that
            XmlDocument doc;
            if (refreshModInfo)
            {
                doc = await Utils.GetManagerInfoDocumentAsync(true);
            }
            else
            {
                if (Settings.ManagerInfoZipfile == null)
                {
                    Logging.Debug("CheckForDatabaseUpdates(false), but Settings.ModInfoZipfile is null. getting latest modInfo");
                    Settings.ManagerInfoZipfile = await Utils.GetManagerInfoZipfileAsync(false);
                }
                //only get if from the downloaded version
                //get the version info string
                string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoZipfile, "manager_version.xml");
                if (string.IsNullOrEmpty(xmlString))
                {
                    Logging.WriteToLog("Failed to get xml string from managerInfo.dat", Logfiles.Application, LogLevel.ApplicationHalt);
                    return;
                }

                //load the document info
                doc = XmlUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
            }

            //get new DB update version and compare
            string databaseNewVersion = XmlUtils.GetXmlStringFromXPath(doc, "//version/database");
            Logging.Info(string.Format("Comparing database versions, old={0}, new={1}", Settings.DatabaseVersion, databaseNewVersion));

            if (string.IsNullOrWhiteSpace(Settings.DatabaseVersion))
            {
                //auto apply and don't announce. this usually happens when the application is loading for first time
                Logging.Info("Settings.DatabaseVersion is empty, setting init value");
                Settings.DatabaseVersion = databaseNewVersion;
                DatabaseVersionLabel.Text = Translations.GetTranslatedString("databaseVersion") + " " + Settings.DatabaseVersion;
            }
            else if (!Settings.DatabaseVersion.Equals(databaseNewVersion))
            {
                //this happens when user clicks to manually check for updates or from the auto install feature
                Logging.Info("new version of database applied");
                Settings.DatabaseVersion = databaseNewVersion;
                DatabaseVersionLabel.Text = Translations.GetTranslatedString("databaseVersion") + " " + Settings.DatabaseVersion;
                MessageBox.Show(Translations.GetTranslatedString("newDBApplied"));
            }
            else
            {
                Logging.Info("database versions are the same");
            }
            Logging.Info("Checking for database updates complete");
        }

        private async Task<bool> CheckForApplicationUpdates()
        {
            //check if skipping updates
            Logging.Info("Started check for application updates");
            if (CommandLineSettings.SkipUpdate)
            {
                if (Settings.ApplicationVersion != ApplicationVersions.Alpha)
                    MessageBox.Show(Translations.GetTranslatedString("skipUpdateWarning"));
                Logging.Warning("Skipping update check from command-line option SkipUpdate");
                return true;
            }

            //if the request distribution version is alpha, correct it to stable
            if (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Alpha)
            {
                Logging.Warning("Alpha is invalid option for ModpackSettings.ApplicationDistroVersion, setting to stable");
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Stable;
            }


            //4 possibilities:
            //stable->stable (update check)
            //stable->beta (auto out of date)
            //beta->stable (auto out of date)
            //beta->beta (update check)
            bool outOfDate = false;

            //make a copy of the current application version and set it to stable if (fake) alpha
            ApplicationVersions version = Settings.ApplicationVersion;

            //check if the documentation xml file is there, if so then we should say in alpha
            if (version == ApplicationVersions.Alpha && !File.Exists("RelhaxModpack.xml"))
            {
                Logging.Warning("You are running an alpha build of Relhax Modpack. Unless you knew you were running an alpha build, you shouldn't be running an alpha build!");
                Logging.Warning("This version is most likely unstable and was used for testing, you should update to beta or stable as soon as possible!");
                version = ApplicationVersions.Beta;
                //at least let's set this for now, might want to unset it later after testing. at this point if you're running an alpha build you're being moved to beta
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Beta;
            }
            else if (version == ApplicationVersions.Alpha && File.Exists("RelhaxModpack.xml"))
            {
                Logging.Debug("TRUE alpha detected");
                Settings.TrueAlpha = true;
            }

            //declare these out here so the logger can access them
            string applicationBuildVersion = Utils.GetApplicationVersion();

            //only true alpha build version will get here
            if (version == ApplicationVersions.Alpha)
            {
                Logging.Debug("application version is {0} on alpha build, skipping update check");
                return true;
            }

            //if current application build does not equal requested distribution channel
            if (version != ModpackSettings.ApplicationDistroVersion)
            {
                outOfDate = true;//can assume out of date
                Logging.Info("Current build is {0} ({1}), online build is NA (changing distribution version {1}->{2})",
                    applicationBuildVersion, version.ToString(), ModpackSettings.ApplicationDistroVersion.ToString());
            }
            else
            {
                outOfDate = !(await Utils.IsManagerUptoDate(applicationBuildVersion));
            }
            if (!outOfDate)
            {
                Logging.Info("Application up to date");
                return true;
            }

            Logging.Info("Application is out of date, display update window");
            versionInfo = new VersionInfo();
            versionInfo.ShowDialog();
            if (versionInfo.ConfirmUpdate)
            {
                //check for any other running instances
                while (true)
                {
                    Thread.Sleep(100);
                    if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                    {
                        MessageBoxResult result = MessageBox.Show(Translations.GetTranslatedString("closeInstanceRunningForUpdate"), Translations.GetTranslatedString("critical"), MessageBoxButton.OKCancel);
                        if (result != MessageBoxResult.OK)
                        {
                            Logging.Info("User canceled update, because he does not want to end the parallel running Relhax instance.");
                            Application.Current.Shutdown();
                            Close();
                            return false;
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
                    //make sure to delete it if it's currently three
                    if (File.Exists(Settings.ApplicationUpdateFileName))
                        File.Delete(Settings.ApplicationUpdateFileName);
                    client.DownloadFileAsync(new Uri(modpackURL), Settings.ApplicationUpdateFileName);
                }
            }
            else
            {
                Logging.Info("User pressed x or said no");
                return false;
            }
            return false;
        }

        private void ResetUI()
        {
            ChildProgressBar.Value = ParentProgressBar.Value = TotalProgressBar.Value = 0;
            InstallProgressTextBox.Text = string.Empty;
            if (CancelDownloadInstallButton.Visibility == Visibility.Visible)
            {
                CancelDownloadInstallButton.Visibility = Visibility.Hidden;
                CancelDownloadInstallButton.IsEnabled = false;
                CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Install_Click;
                CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;
            }
        }

        private void OnUpdateDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //stop the timer
            stopwatch.Reset();
            if (e.Error != null)
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
                    zip.ExtractAll(Settings.ApplicationStartupPath, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch (ZipException zipex)
            {
                Logging.WriteToLog("Failed to extract update zip file\n" + zipex.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
                MessageBox.Show(Translations.GetTranslatedString("failedToExtractUpdateArchive"));
                Application.Current.Shutdown();
            }
            //extract the batch script to update the application
            string batchScript = Utils.GetStringFromZip(Settings.ManagerInfoZipfile, Settings.RelicBatchUpdateScriptServer);
            File.WriteAllText(Settings.RelicBatchUpdateScript, batchScript);
            //try to start the update script
            try
            {
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = Path.Combine(Settings.ApplicationStartupPath, Settings.RelicBatchUpdateScript),
                    Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray())
                };
                using (Process installUpdate = new Process { StartInfo = info })
                {
                    installUpdate.Start();
                }
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
            if (ModpackSettings.InstallWhileDownloading)
                return;
            //if it's not running, start it
            if (!stopwatch.IsRunning)
                stopwatch.Start();
            //set the update progress bar
            ChildProgressBar.Value = e.ProgressPercentage;
            float MBDownloaded = (float)e.BytesReceived / (float)Utils.BYTES_TO_MBYTES;
            float MBTotal = (float)e.TotalBytesToReceive / (float)Utils.BYTES_TO_MBYTES;
            MBDownloaded = (float)Math.Round(MBDownloaded, 2);
            MBTotal = (float)Math.Round(MBTotal, 2);
            string downloadMessage = string.Format("{0} {1}MB {2} {3}MB", Translations.GetTranslatedString("downloadingUpdate"),
                MBDownloaded, Translations.GetTranslatedString("of"), MBTotal);
            InstallProgressTextBox.Text = downloadMessage;
        }
        #endregion

        #region Installation

        private void InstallModpackButton_Click(object sender, RoutedEventArgs e)
        {
            //toggle buttons and reset UI
            ResetUI();
            ToggleUIButtons(false);

            //settings for export mode
            if (ModpackSettings.ExportMode)
            {
                Logging.Debug("ExportMode is True, asking where to export");
                string foldertoExportTo = string.Empty;
                using (System.Windows.Forms.FolderBrowserDialog exportFolderSelect = new System.Windows.Forms.FolderBrowserDialog()
                {
                    ShowNewFolderButton = true,
                    Description = Translations.GetTranslatedString("selectLocationToExport")
                })
                {
                    if (exportFolderSelect.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        foldertoExportTo = exportFolderSelect.SelectedPath;
                    else
                    {
                        Logging.Debug("user canceled selecting folder, stop");
                        ToggleUIButtons(true);
                        return;
                    }
                }

                Logging.Debug("ask which version of client to export for");
                ExportModeSelect exportModeSelect = new ExportModeSelect();
                if ((bool)exportModeSelect.ShowDialog())
                {
                    Logging.Debug("ExportModeSelect returned true, setting majorVersion to {0}, minorVersion to {1}",
                        exportModeSelect.SelectedVersionInfo.WoTOnlineFolderVersion, exportModeSelect.SelectedVersionInfo.WoTClientVersion);
                    Settings.WoTModpackOnlineFolderVersion = exportModeSelect.SelectedVersionInfo.WoTOnlineFolderVersion;
                    Settings.WoTClientVersion = exportModeSelect.SelectedVersionInfo.WoTClientVersion;
                    Settings.WoTDirectory = foldertoExportTo;
                }
                else
                {
                    Logging.Debug("exportModeSelect returned false, stop");
                    ToggleUIButtons(true);
                    return;
                }
            }
            else
            {
                if (ModpackSettings.AutoInstall || ModpackSettings.OneClickInstall)
                {
                    //load the custom selection file, if it exists
                    if (!File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
                    {
                        MessageBox.Show(Translations.GetTranslatedString("autoOneclickSelectionFileNotExist"));
                        ToggleUIButtons(true);
                        return;
                    }
                }
                if (databaseVersion == DatabaseVersions.Beta)
                {
                    //if mods sync
                    if (ModpackSettings.AutoInstall)
                    {
                        MessageBox.Show(Translations.GetTranslatedString("noAutoInstallWithBeta"));
                        ToggleUIButtons(true);
                        return;
                    }
                }

                //parse WoT root directory
                Logging.WriteToLog("started looking for WoT root directory", Logfiles.Application, LogLevel.Debug);
                if (!Utils.AutoFindWoTDirectory(ref Settings.WoTDirectory) || ModpackSettings.ForceManuel)
                {
                    Logging.WriteToLog("auto detect failed or user requests manual", Logfiles.Application, LogLevel.Debug);
                    OpenFileDialog manualWoTFind = new OpenFileDialog()
                    {
                        InitialDirectory = string.IsNullOrWhiteSpace(Settings.WoTDirectory) ? Settings.ApplicationStartupPath : Settings.WoTDirectory,
                        AddExtension = true,
                        CheckFileExists = true,
                        CheckPathExists = true,
                        Filter = "WorldOfTanks.exe|WorldOfTanks.exe",
                        Multiselect = false,
                        ValidateNames = true
                    };
                    if ((bool)manualWoTFind.ShowDialog())
                    {
                        Settings.WoTDirectory = manualWoTFind.FileName;
                    }
                    else
                    {
                        Logging.Info("User Canceled installation");
                        ToggleUIButtons(true);
                        return;
                    }
                }
                Settings.WoTDirectory = Path.GetDirectoryName(Settings.WoTDirectory);
                Logging.Info("Wot root directory parsed as " + Settings.WoTDirectory);

                //check to make sure the application is not in the same directory as the WoT install
                if (Settings.WoTDirectory.Equals(Settings.ApplicationStartupPath))
                {
                    //display error and abort
                    MessageBox.Show(Translations.GetTranslatedString("moveOutOfTanksLocation"));
                    ToggleUIButtons(true);
                    return;
                }

                //if test mode, check if test path exists
                if (databaseVersion == DatabaseVersions.Test)
                {
                    if (string.IsNullOrWhiteSpace(ModpackSettings.CustomModInfoPath) || !File.Exists(ModpackSettings.CustomModInfoPath))
                    {
                        if (!(bool)FindTestDatabaseDialog.ShowDialog())
                        {
                            ToggleUIButtons(true);
                            return;
                        }
                        ModpackSettings.CustomModInfoPath = FindTestDatabaseDialog.FileName;
                    }
                }

                //get the version of tanks in the format
                //of the res_mods version folder i.e. 0.9.17.0.3
                string versionTemp = XmlUtils.GetXmlStringFromXPath(Path.Combine(Settings.WoTDirectory, "version.xml"), "//version.xml/version");
                Settings.WoTClientVersion = versionTemp.Split('#')[0].Trim().Substring(2).Trim();
                Logging.Info("detected client version: {0}", Settings.WoTClientVersion);

                //determine if current detected version of the game is supported
                //only if application distribution is not alpha and database distribution is not test
                //the warning will therefore also happen in beta, but not take effect
                if (databaseVersion != DatabaseVersions.Test)
                {
                    //make an array of all the supported versions
                    string supportedClientsXML = Utils.GetStringFromZip(Settings.ManagerInfoZipfile, "supported_clients.xml");
                    if (string.IsNullOrWhiteSpace(supportedClientsXML))
                    {
                        Logging.Info("Failed to parse supported_clients.xml from string from zipfile", Logfiles.Application, LogLevel.Exception);
                        MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " supported_clients.xml");
                        ToggleUIButtons(true);
                        return;
                    }

                    XmlDocument doc = XmlUtils.LoadXmlDocument(supportedClientsXML, XmlLoadType.FromString);
                    if(doc == null)
                    {
                        Logging.Error("Failed to parse supported_clients.xml ");
                        MessageBox.Show(Translations.GetTranslatedString("failedToParse") + " supported_clients.xml");
                        ToggleUIButtons(true);
                        return;
                    }

                    //copy inner text of each WoT version into a string array
                    XmlNodeList supportedVersionsXML = XmlUtils.GetXmlNodesFromXPath(doc, "//versions/version");
                    string[] supportedVersionsString = new string[supportedVersionsXML.Count];
                    for (int i = 0; i < supportedVersionsXML.Count; i++)
                    {
                        supportedVersionsString[i] = supportedVersionsXML[i].InnerText.Trim();

                        //see if this supported client version is the same as what was parsed to be the current client version
                        if (supportedVersionsString[i].Equals(Settings.WoTClientVersion))
                        {
                            //set the online folder
                            Settings.WoTModpackOnlineFolderVersion = supportedVersionsXML[i].Attributes["folder"].Value;
                        }
                    }

                    //check to see if array of supported clients has the detected WoT client version
                    //if the version does not match, then we need to set the online folder download version
                    if (!supportedVersionsString.Contains(Settings.WoTClientVersion))
                    {
                        Settings.WoTModpackOnlineFolderVersion = supportedVersionsXML[supportedVersionsXML.Count - 1].Attributes["folder"].Value;

                        //if it's not alpha, show the warning messages
                        if (Settings.ApplicationVersion != ApplicationVersions.Alpha)
                        {
                            //log and inform the user
                            Logging.Warning("current client version {0} does not exist in list: {1}", Settings.WoTClientVersion, string.Join(", ", supportedVersionsString));
                            MessageBox.Show(string.Format("{0}: {1}\n{2} {3}\n\n{4}:\n{5}",
                                Translations.GetTranslatedString("detectedClientVersion"),//0
                                Settings.WoTClientVersion,//1
                                Translations.GetTranslatedString("supportNotGuarnteed"),//2
                                Translations.GetTranslatedString("couldTryBeta"),//3
                                Translations.GetTranslatedString("supportedClientVersions"),//4
                                string.Join("\n", supportedVersionsString)),//5
                                Translations.GetTranslatedString("critical"));//header
                        }
                    }

                    //if the user wants to, check if the database has actually changed
                    if (ModpackSettings.NotifyIfSameDatabase)
                    {
                        //get the install log for last installed database version
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
                            Logging.Warning("installedRelhaxFiles.log does not exist, cannot notify if same database");
                        }
                    }
                }
            }

            //show the mod selection list
            modSelectionList = new ModSelectionList
            {
                //set the owner
                //https://stackoverflow.com/questions/21756542/why-is-window-showdialog-not-blocking-in-taskscheduler-task
                //https://docs.microsoft.com/en-us/dotnet/api/system.windows.window.owner?view=netframework-4.8
                Owner = GetWindow(this)
            };
            //https://stackoverflow.com/questions/623451/how-can-i-make-my-own-event-in-c
            modSelectionList.OnSelectionListReturn += ModSelectionList_OnSelectionListReturn;
            modSelectionList.Show();
        }

        //https://stackoverflow.com/questions/623451/how-can-i-make-my-own-event-in-c
        private void ModSelectionList_OnSelectionListReturn(object sender, SelectionListEventArgs e)
        {
            if (e.ContinueInstallation)
            {
                OnBeginInstallation(new List<Category>(e.ParsedCategoryList), new List<Dependency>(e.Dependencies),
                    new List<DatabasePackage>(e.GlobalDependencies), new List<SelectablePackage>(e.UserMods));
                modSelectionList = null;
            }
            else
            {
                ToggleUIButtons(true);
            }
        }

        private async void OnBeginInstallation(List<Category> parsedCategoryList, List<Dependency> dependencies, List<DatabasePackage> globalDependencies, List<SelectablePackage> UserMods)
        {
            //rookie mistake checks
            if (parsedCategoryList == null || dependencies == null || globalDependencies == null ||
                parsedCategoryList.Count == 0 || dependencies.Count == 0 || globalDependencies.Count == 0)
                throw new BadMemeException("You suck at starting installations LEARN2CODE");

            //start the timer
            Logging.Info("Starting an installation (timer starts now)");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            //check if wot is running
            AskCloseWoT askCloseWoT = null;
            while (Utils.IsProcessRunning(Settings.WoTProcessName, Settings.WoTDirectory))
            {
                //create window to determine if cancel, wait, kill TODO
                if (askCloseWoT == null)
                    askCloseWoT = new AskCloseWoT();
                //a positive result means that we are going to retry the loop
                //it could mean the user hit retry (close true), or hit force close (succeeded, and try again anyways)
                if (!(bool)askCloseWoT.ShowDialog())
                {
                    ToggleUIButtons(true);
                    return;
                }
                Thread.Sleep(100);
            }

            //build macro hash for install
            Utils.BuildFilepathMacroList();

            //perform dependency calculations
            //get a flat list of packages to install
            List<DatabasePackage> flatList = Utils.GetFlatList(null, null, null, parsedCategoryList);
            List<SelectablePackage> flatListSelect = new List<SelectablePackage>();

            //convert it to correct class type
            foreach (SelectablePackage sp in flatList)
                flatListSelect.Add(sp);
            Logging.Debug("starting Utils.CalculateDependencies()");
            List<Dependency> dependneciesToInstall = new List<Dependency>(Utils.CalculateDependencies(dependencies, flatListSelect, parsedCategoryList));

            //make a flat list of all packages to install (including those without a zip file) for statistic data gathering
            if (ModpackSettings.AllowStatisticDataGather)
            {
                List<DatabasePackage> packagesToGather = new List<DatabasePackage>();
                packagesToGather.AddRange(globalDependencies.Where(globalDep => globalDep.Enabled).ToList());
                packagesToGather.AddRange(dependneciesToInstall.Where(dep => dep.Enabled).ToList());
                packagesToGather.AddRange(flatListSelect.Where(fl => fl.Enabled && fl.Checked).ToList());
                //https://stackoverflow.com/questions/13781468/get-list-of-properties-from-list-of-objects
                List<string> packageNamesToUpload = packagesToGather.Select(pack => pack.PackageName).ToList();

                Task.Run(async () =>
                {
                    try
                    {
                        //https://stackoverflow.com/questions/10292730/httpclient-getasync-with-network-credentials
                        using (HttpClientHandler handler = new HttpClientHandler()
                        {
                            Credentials = PrivateStuff.BigmodsNetworkCredentialScripts,
                            ClientCertificateOptions = ClientCertificateOption.Automatic,
                            PreAuthenticate = true
                        })
                        using (HttpClient client = new HttpClient(handler) { BaseAddress = new Uri(PrivateStuff.BigmodsEnterDownloadStatPHP) })
                        {
                            //https://stackoverflow.com/questions/15176538/net-httpclient-how-to-post-string-value
                            FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("packageNames", string.Join(",",packageNamesToUpload))
                            });
                            HttpResponseMessage result = await client.PostAsync("", content);
                            Logging.Debug("Statistic data HTTP response code: {0}", result.StatusCode.ToString());
                            if (!result.IsSuccessStatusCode)
                            {
                                Logging.Warning("Failed to send statistic data. Response code={0}, reason={1}", result.StatusCode.ToString(), result.ReasonPhrase);
                            }
                            string resultContent = await result.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Error("an error occurred sending statistic data");
                        Logging.Error(ex.ToString());
                    }
                });
            }

            //make a flat list of all packages to install that will actually be installed
            List<DatabasePackage> packagesToInstall = new List<DatabasePackage>();
            packagesToInstall.AddRange(globalDependencies.Where(globalDep => globalDep.Enabled && !string.IsNullOrWhiteSpace(globalDep.ZipFile)).ToList());
            packagesToInstall.AddRange(dependneciesToInstall.Where(dep => dep.Enabled && !string.IsNullOrWhiteSpace(dep.ZipFile)).ToList());
            List<SelectablePackage> selectablePackagesToInstall = flatListSelect.Where(fl => fl.Enabled && fl.Checked && !string.IsNullOrWhiteSpace(fl.ZipFile)).ToList();
            packagesToInstall.AddRange(selectablePackagesToInstall);
            List<SelectablePackage> userModsToInstall = UserMods.Where(mod => mod.Checked).ToList();

            //while we're at it let's make a list of packages that need to be downloaded
            List<DatabasePackage> packagesToDownload = packagesToInstall.Where(pack => pack.DownloadFlag).ToList();

            //and check if we need to actually install anything
            if (selectablePackagesToInstall.Count == 0 && userModsToInstall.Count == 0)
            {
                Logging.Info("no packages selected to install...");
                ResetUI();
                ToggleUIButtons(true);
                return;
            }

            //temporary fix for allowing packages to be installed in similar order to v1 legacy system
            //offset the installGroup values of selectablePackages added by the level of the package in the tree
#warning selectablePackage level offset is active
            Logging.Warning("selectablePackage level offset is active");
            Utils.PropagateInstallGroupsPerLevel(packagesToInstall);

            //perform list install order calculations
            List<DatabasePackage>[] orderedPackagesToInstall = Utils.CreateOrderedInstallList(packagesToInstall);

            //we now have a list of enabled, checked and actual zip file mods that we are going to install based on install groups
            //log the time to process lists
            TimeSpan lastTime = stopwatch.Elapsed;
            Logging.Info(string.Format("Took {0} msec to process lists", stopwatch.ElapsedMilliseconds));

            //first, if we have downloads to do and doing them the standard way, then start processing them
            if (packagesToDownload.Count > 0 && !ModpackSettings.InstallWhileDownloading)
            {
                Logging.Info("download while install = false and packages to download, starting ProcessDownloads()");
                //toggle the button before and after as well
                CancelDownloadInstallButton.Visibility = Visibility.Visible;
                CancelDownloadInstallButton.IsEnabled = true;
                //disconnect the install method and connect the download
                //https://stackoverflow.com/questions/367523/how-to-ensure-an-event-is-only-subscribed-to-once
                CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Install_Click;
                CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;
                CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Download_Click;
                bool downlaodTaskComplete = await ProcessDownloads(packagesToDownload);
                //stop and end the timer
                downloadDisplayTimer.Stop();
                downloadDisplayTimer.Dispose();
                downloadDisplayTimer = null;
                if (!downlaodTaskComplete)
                {
                    Logging.Info("download task was canceled, canceling installation");
                    ToggleUIButtons(true);
                    return;
                }
                CancelDownloadInstallButton.IsEnabled = false;
                CancelDownloadInstallButton.Visibility = Visibility.Hidden;
                //connect the install and disconnect the download
                CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Install_Click;
                CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;
                Logging.Info(string.Format("download time took {0} msec", stopwatch.Elapsed.TotalMilliseconds - lastTime.TotalMilliseconds));
                lastTime = stopwatch.Elapsed;
            }
            else if (packagesToDownload.Count > 0 && ModpackSettings.InstallWhileDownloading)
            {
                Logging.Info("download while install = true and packages to download, starting ProcessDownloadsAsync()");
                ProcessDownloadsAsync(packagesToDownload);
                //async does download and install at the same time, so subscribe to both, install first
                CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Install_Click;
                CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;
                CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Install_Click;
                CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Download_Click;
            }
            else if (packagesToDownload.Count == 0)
                Logging.Info("no packages to download");

            //now let's start the install procedures
            //like if we need to make the advanced install window
            //but null it at all times
            if (AdvancedProgressWindow != null)
                AdvancedProgressWindow = null;
            if (ModpackSettings.AdvancedInstalProgress)
            {
                Logging.Debug("advancedInstallProgress is true, making window and populating with reporters");
                AdvancedProgressWindow = new AdvancedProgress()
                {
                    ShouldUserInstallBeCalled = userModsToInstall.Count > 0
                };
                //build the number of InstallTaskReporter objects based on what we are doing
                //if we are making a backup of the mods then make a reporter for it
                if (ModpackSettings.BackupModFolder)
                {
                    Logging.Debug("adding backupModFolder reporter");
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter()
                    {
                        IsSubProgressActive = true,
                        TaskTitle = Translations.GetTranslatedString("AdvancedInstallBackupMods"),
                        ReportState = TaskReportState.Inactive
                    };
                    AdvancedProgressWindow.PreInstallPanel.Children.Add(reporter);
                    AdvancedProgressWindow.BackupModsReporter = reporter;
                }

                //if we are backing up data, clearing cache, or clearing logs, then make one to hold them all
                if (ModpackSettings.SaveUserData || ModpackSettings.ClearCache || ModpackSettings.DeleteLogs)
                {
                    Logging.Debug("adding userData/clearCache/deleteLogs reporter: SaveUserData={0}, ClearCache={1}, DeleteLogs={2}",
                        ModpackSettings.SaveUserData, ModpackSettings.ClearCache, ModpackSettings.DeleteLogs);
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter()
                    {
                        IsSubProgressActive = false,
                        ReportState = TaskReportState.Inactive
                    };
                    AdvancedProgressWindow.PreInstallPanel.Children.Add(reporter);
                    AdvancedProgressWindow.BackupDataClearCacheClearLogsReporter = reporter;
                }

                //same for cleaning mods
                if (ModpackSettings.CleanInstallation)
                {
                    Logging.Debug("adding CleanInstallation reporter");
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter()
                    {
                        IsSubProgressActive = false,
                        TaskTitle = Translations.GetTranslatedString("AdvancedInstallClearMods"),
                        ReportState = TaskReportState.Inactive
                    };
                    AdvancedProgressWindow.PreInstallPanel.Children.Add(reporter);
                    AdvancedProgressWindow.CleanModsReporter = reporter;
                }

                //extraction is done based on the number of threads
                int numThreads = ModpackSettings.MulticoreExtraction ? Settings.NumLogicalProcesors : 1;
                AdvancedProgressWindow.ExtractionModsReporters = new RelhaxInstallTaskReporter[numThreads];
                Logging.Debug("adding {0} reporters (MultiCoreExtraction={1}", numThreads, ModpackSettings.MulticoreExtraction);
                for (int i = 0; i < numThreads; i++)
                {
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter()
                    {
                        IsSubProgressActive = true,
                        TaskTitle = string.Format("{0} {1}", Translations.GetTranslatedString("AdvancedInstallInstallMods"), (i + 1).ToString()),
                        ReportState = TaskReportState.Inactive
                    };
                    AdvancedProgressWindow.ExtractionPanel.Children.Add(reporter);
                    AdvancedProgressWindow.ExtractionModsReporters[i] = reporter;
                }

                //same idea for user mods
                if (userModsToInstall.Count > 0)
                {
                    Logging.Debug("adding userMods reporter");
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter()
                    {
                        IsSubProgressActive = true,
                        TaskTitle = Translations.GetTranslatedString("AdvancedInstallInstallUserMods"),
                        ReportState = TaskReportState.Inactive
                    };
                    AdvancedProgressWindow.ExtractionPanel.Children.Add(reporter);
                    AdvancedProgressWindow.ExtractionUserModsReporter = reporter;
                }

                //all the post-install options, you don't know until the extraction finishes and if the UI reports events on it

                AdvancedProgressWindow.Show();
            }
            else
                Logging.Debug("advancedInstallProgress is false");

            //make sure each trigger list for each package is unique
            foreach (DatabasePackage package in packagesToInstall)
            {
                //for debug, get the list of duplicates
                //https://stackoverflow.com/questions/3811464/how-to-get-duplicate-items-from-a-list-using-linq
                List<string> duplicates = package.Triggers.GroupBy(trigger => trigger).Where(trig => trig.Count() > 1).Select(trig => trig.Key).ToList();
                if (duplicates.Count > 0)
                {
                    //first make it distinct
                    package.Triggers = package.Triggers.Distinct().ToList();
                    Logging.Debug("Duplicate triggers found in package {0}:{1}", package.PackageName, string.Join(",", duplicates));
                }
            }

            //create the cancellation token source
            cancellationTokenSource = new CancellationTokenSource();

            Logging.Debug("userMods install count: {0}", userModsToInstall.Count);

            //if user mods are being installed, then disable triggers
            disableTriggersBackupVal = ModpackSettings.DisableTriggers;
            if (userModsToInstall.Count > 0 && !ModpackSettings.DisableTriggers)
            {
                Logging.Info("DisableTriggers is false and user has mods to install. Disabling triggers");
                disableTriggersBackupVal = true;
            }

            Logging.Debug("creating install engine, cancel options and progress reporting");
            //and create and link the install engine
            installEngine = new InstallerComponents.InstallEngine()
            {
                FlatListSelectablePackages = flatListSelect,
                OrderedPackagesToInstall = orderedPackagesToInstall,
                PackagesToInstall = packagesToInstall,
                ParsedCategoryList = parsedCategoryList,
                Dependencies = dependencies,
                GlobalDependencies = globalDependencies,
                UserPackagesToInstall = userModsToInstall,
                CancellationToken = cancellationTokenSource.Token,
                DisableTriggersForInstall = disableTriggersBackupVal
            };

            //setup the cancel button
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Install_Click;
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;
            CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Install_Click;
            CancelDownloadInstallButton.Visibility = Visibility.Visible;
            CancelDownloadInstallButton.IsEnabled = true;

            //create progress object
            Progress<RelhaxInstallerProgress> progress = new Progress<RelhaxInstallerProgress>();
            progress.ProgressChanged += OnInstallProgressChanged;

            //run install
            Logging.Debug("running installation from MainWindow");
            InstallerComponents.RelhaxInstallFinishedEventArgs results = await installEngine.RunInstallationAsync(progress);
            Logging.Debug("installation has finished, returned to MainWindow");
            installEngine.Dispose();
            installEngine = null;

            //close and hide the install progress button
            CancelDownloadInstallButton.IsEnabled = false;
            CancelDownloadInstallButton.Visibility = Visibility.Hidden;
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Install_Click;
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;

            //if from command line auto install, then turn it off to prevent another one
            if (!string.IsNullOrEmpty(CommandLineSettings.AutoInstallFileName))
                CommandLineSettings.AutoInstallFileName = string.Empty;

            //close and free up RAM from advanced install progress
            if (ModpackSettings.AdvancedInstalProgress)
            {
                if (AdvancedProgressWindow != null)
                {
                    AdvancedProgressWindow.Close();
                    AdvancedProgressWindow = null;
                }
            }

            //update the UI to be in a "finished" state
            InstallProgressTextBox.Clear();
            ParentProgressBar.Value = ParentProgressBar.Maximum;
            ChildProgressBar.Value = ChildProgressBar.Maximum;
            TotalProgressBar.Value = TotalProgressBar.Maximum;

            //after waiting for the installation...
            if (results.ExitCode == InstallerComponents.InstallerExitCodes.Success)
            {
                if (ModpackSettings.ShowInstallCompleteWindow)
                {
                    InstallFinished installFinished = new InstallFinished();
                    installFinished.ShowDialog();
                }
                else
                {
                    MessageBox.Show(Translations.GetTranslatedString("installationFinished"));
                }
                InstallProgressTextBox.Text = string.Empty;
                ToggleUIButtons(true);
            }
            else if (cancellationTokenSource.IsCancellationRequested)
            {
                Logging.Info("Cancel success");
                ToggleUIButtons(true);
                InstallProgressTextBox.Text = Translations.GetTranslatedString("canceled");
            }
            else
            {
                //explain why if failed
                MessageBox.Show(string.Format("{0}{1}{2}", Translations.GetTranslatedString("installFailed") + ":", Environment.NewLine, results.ExitCode.ToString()));
                //and log
                Logging.WriteToLog(string.Format("Installer failed to install, exit code {0}\n{1}", results.ExitCode.ToString(), results.ErrorMessage),
                    Logfiles.Application, LogLevel.Exception);
                ToggleUIButtons(true);
            }
        }

        private void OnInstallProgressChanged(object sender, RelhaxInstallerProgress e)
        {
            if (ModpackSettings.AdvancedInstalProgress)
            {
                if (AdvancedProgressWindow == null)
                {
                    throw new BadMemeException("Not so advanced now is it");
                }
                AdvancedProgressWindow.OnReportAdvancedProgress(e);
            }
            else
            {
                //setup progress bars
                if (TotalProgressBar.Maximum != e.TotalTotal)
                    TotalProgressBar.Maximum = e.TotalTotal;
                if (TotalProgressBar.Minimum != 0)
                    TotalProgressBar.Minimum = 0;
                if (TotalProgressBar.Value != e.TotalCurrent)
                    TotalProgressBar.Value = e.TotalCurrent;

                if (ParentProgressBar.Maximum != e.ParrentTotal)
                    ParentProgressBar.Maximum = e.ParrentTotal;
                if (ParentProgressBar.Minimum != 0)
                    ParentProgressBar.Minimum = 0;
                if (ParentProgressBar.Value != e.ParrentCurrent)
                    ParentProgressBar.Value = e.ParrentCurrent;

                if (ChildProgressBar.Maximum != e.ChildTotal)
                    ChildProgressBar.Maximum = e.ChildTotal;
                if (ChildProgressBar.Minimum != 0)
                    ChildProgressBar.Minimum = 0;
                if (ChildProgressBar.Value != e.ChildCurrent)
                    ChildProgressBar.Value = e.ChildCurrent;

                string line1 = string.Empty;
                string line2 = string.Empty;
                string line3 = string.Empty;
                string line4 = string.Empty;

                //standard progress
                switch (e.InstallStatus)
                {
                    case InstallerComponents.InstallerExitCodes.BackupModsError:
                        line1 = Translations.GetTranslatedString("installBackupMods");
                        if (string.IsNullOrEmpty(e.ParrentCurrentProgress))
                        {
                            line2 = e.EntryFilename;
                        }
                        else
                        {
                            line2 = e.ParrentCurrentProgress;
                        }
                        break;
                    case InstallerComponents.InstallerExitCodes.BackupDataError:
                        //filename is name of file in package to backup
                        //parrentCurrentProgress is name of package
                        line1 = Translations.GetTranslatedString("installBackupData");
                        line2 = e.Filename;
                        line3 = e.ParrentCurrentProgress;
                        break;
                    case InstallerComponents.InstallerExitCodes.ClearCacheError:
                        line1 = Translations.GetTranslatedString("installClearCache");
                        break;
                    case InstallerComponents.InstallerExitCodes.ClearLogsError:
                        line1 = Translations.GetTranslatedString("installClearLogs");
                        line2 = Path.GetFileName(e.Filename);
                        break;
                    case InstallerComponents.InstallerExitCodes.CleanModsError:
                        line1 = Translations.GetTranslatedString("installCleanMods");
                        line2 = e.Filename;
                        break;
                    case InstallerComponents.InstallerExitCodes.ExtractionError:
                        if(ModpackSettings.MulticoreExtraction && !ModpackSettings.AdvancedInstalProgress)
                        {
                            ChildProgressBar.Maximum = e.TotalInstallGroups;
                            ChildProgressBar.Value = e.InstallGroup;
                            line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installExtractingMods"), e.ParrentCurrent.ToString(),
                                Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                            line2 = string.Format("{0}: {1} {2} {3} {4} {5}", Translations.GetTranslatedString("installExtractingCompletedThreads"), e.CompletedThreads.ToString(),
                                Translations.GetTranslatedString("of"), e.TotalThreads.ToString(), Translations.GetTranslatedString("installExtractingOfGroup"), e.InstallGroup.ToString());
                            line3 = Path.GetFileName(e.Filename);
                            line4 = e.EntryFilename;
                        }
                        else
                        {
                            ChildProgressBar.Maximum = e.BytesTotal;
                            ChildProgressBar.Value = e.BytesProcessed;
                            line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installExtractingMods"), e.ParrentCurrent.ToString(),
                                Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                            line2 = Path.GetFileName(e.Filename);
                            line3 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installZipFileEntry"), e.EntriesProcessed.ToString(),
                                Translations.GetTranslatedString("of"), e.EntriesTotal.ToString());
                            line4 = e.EntryFilename;
                        }
                        break;
                    case InstallerComponents.InstallerExitCodes.UserExtractionError:
                        ChildProgressBar.Maximum = e.BytesTotal;
                        ChildProgressBar.Value = e.BytesProcessed;
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("extractingUserMod"), e.ParrentCurrent.ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = Path.GetFileName(e.Filename);
                        line3 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installZipFileEntry"), e.EntriesProcessed.ToString(),
                            Translations.GetTranslatedString("of"), e.EntriesTotal.ToString());
                        line4 = e.EntryFilename;
                        break;
                    case InstallerComponents.InstallerExitCodes.RestoreUserdataError:
                        //filename is name of file in package to backup
                        //parrentCurrentProgress is name of package
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installRestoreUserdata"), e.ParrentCurrent.ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = e.Filename;
                        line3 = e.ParrentCurrentProgress;
                        break;
                    case InstallerComponents.InstallerExitCodes.XmlUnpackError:
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installXmlUnpack"), e.ParrentCurrent.ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = e.Filename;
                        break;
                    case InstallerComponents.InstallerExitCodes.PatchError:
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installPatchFiles"), e.ParrentCurrent.ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = e.Filename;
                        break;
                    case InstallerComponents.InstallerExitCodes.ShortcutsError:
                        line1 = Translations.GetTranslatedString("installShortcuts");
                        line2 = e.Filename;
                        break;
                    case InstallerComponents.InstallerExitCodes.ContourIconAtlasError:
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installContourIconAtlas"), e.ParrentCurrent.ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = string.Format("{0} {1} {2} {3}", e.ChildCurrent.ToString(), Translations.GetTranslatedString("of"), e.ChildTotal.ToString(),
                            Translations.GetTranslatedString("stepsComplete"));
                        break;
                    case InstallerComponents.InstallerExitCodes.FontInstallError:
                        line1 = Translations.GetTranslatedString("installFonts");
                        break;
                    case InstallerComponents.InstallerExitCodes.CleanupError:
                        line1 = Translations.GetTranslatedString("installCleanup");
                        break;
                }

                InstallProgressTextBox.Text = string.Format("{0}{1}{2}{3}",
                    string.IsNullOrEmpty(line1) ? string.Empty : line1 + "\n",
                    string.IsNullOrEmpty(line2) ? string.Empty : line2 + "\n",
                    string.IsNullOrEmpty(line3) ? string.Empty : line3 + "\n",
                    string.IsNullOrEmpty(line4) ? string.Empty : line4 + "\n");

            }
        }

        //handles processing of downloads
        private async Task ProcessDownloadsAsync(List<DatabasePackage> packagesToDownload)
        {
            using (WebClient client = new WebClient())
            {
                this.client = client;
                int retryCount = 3;
                string fileToDownload = string.Empty;
                string fileToSaveTo = string.Empty;
                foreach (DatabasePackage package in packagesToDownload)
                {
                    retryCount = 3;
                    while (retryCount > 0)
                    {
                        package.StartAddress = package.StartAddress.Replace("{onlineFolder}", Settings.WoTModpackOnlineFolderVersion);
                        fileToDownload = package.StartAddress + package.ZipFile + package.EndAddress;
                        fileToSaveTo = Path.Combine(Settings.RelhaxDownloadsFolderPath, package.ZipFile);
                        try
                        {
                            Logging.Info("Async download of {0} start", package.ZipFile);
                            await client.DownloadFileTaskAsync(fileToDownload, fileToSaveTo);
                            Logging.Info("Async download of {0} finish", package.ZipFile);
                            retryCount = 0;
                            package.DownloadFlag = false;
                        }
                        catch (WebException ex)
                        {
                            if (cancellationTokenSource.IsCancellationRequested)
                            {
                                Logging.Info("Download canceled from UI request, stopping installation");
                                if (File.Exists(fileToSaveTo))
                                    File.Delete(fileToSaveTo);
                                return;
                            }
                            Logging.Error("Failed to download the file {0}, try {1} of {2}\n{3}", package.ZipFile, retryCount, 1, ex.ToString());
                            retryCount--;
                            //if it failed or not, the file should be deleted
                            if (File.Exists(fileToSaveTo))
                                File.Delete(fileToSaveTo);
                        }
                    }
                }
            }
        }

        private async Task<bool> ProcessDownloads(List<DatabasePackage> packagesToDownload)
        {
            //remember this is on the UI thread
            //reset the UI info
            ParentProgressBar.Minimum = 0;
            ParentProgressBar.Maximum = packagesToDownload.Count;
            ParentProgressBar.Value = 0;
            using (WebClient client = new WebClient())
            {
                this.client = client;
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
                    //increment it out here, not in the repeat loop
                    ParentProgressBar.Value++;
                    downloadProgress.ChildCurrentProgress = package.ZipFile;
                    bool retry = true;
                    while (retry)
                    {
                        //replace the start address macro
                        package.StartAddress = package.StartAddress.Replace("{onlineFolder}", Settings.WoTModpackOnlineFolderVersion);
                        fileToDownload = package.StartAddress + package.ZipFile + package.EndAddress;
                        fileToSaveTo = Path.Combine(Settings.RelhaxDownloadsFolderPath, package.ZipFile);
                        try
                        {
                            //reset current bytes downloaded
                            currentBytesDownloaded = 0;
                            Logging.Info("Download of {0} start", package.ZipFile);
                            await client.DownloadFileTaskAsync(fileToDownload, fileToSaveTo);
                            Logging.Info("Download of {0} finish", package.ZipFile);
                            retry = false;
                            package.DownloadFlag = false;
                        }
                        catch (WebException ex)
                        {
                            if (ex.Status == WebExceptionStatus.RequestCanceled)
                            {
                                Logging.Info("Download canceled from UI request, stopping installation");
                                downloadTimer.Stop();
                                ToggleUIButtons(true);
                                ResetUI();
                                retry = false;
                                if (File.Exists(fileToSaveTo))
                                    File.Delete(fileToSaveTo);
                                return false;
                            }
                            else
                            {
                                Logging.Error("failed to download the file {0} {1} {2}", package.ZipFile, Environment.NewLine, ex.ToString());
                                //show abort retry ignore window TODO
                                MessageBoxResult result = MessageBox.Show(string.Format("{0} {1} \"{2}\" {3}",
                                    Translations.GetTranslatedString("failedToDownload1"), Environment.NewLine,
                                    package.ZipFile, Translations.GetTranslatedString("failedToDownload2")),
                                    Translations.GetTranslatedString("failedToDownloadHeader"), MessageBoxButton.YesNoCancel);
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        //keep retry as true
                                        break;
                                    case MessageBoxResult.No:
                                        //skip this file
                                        retry = false;
                                        break;
                                    case MessageBoxResult.Cancel:
                                        //stop the installation all together
                                        ToggleUIButtons(true);
                                        ResetUI();
                                        retry = false;
                                        return false;
                                }
                            }
                            //if it failed or canceled, the file should be deleted
                            if (File.Exists(fileToSaveTo))
                                File.Delete(fileToSaveTo);
                        }
                        //stop the timer
                        downloadDisplayTimer.Stop();
                    }
                }
            }
            return true;
        }
        #endregion

        #region Uninstall
        private async void UninstallModpackButton_Click(object sender, RoutedEventArgs e)
        {
            //toggle the buttons and reset the UI
            ToggleUIButtons(false);
            ResetUI();

            //parse WoT root directory
            Logging.WriteToLog("started looking for WoT root directory", Logfiles.Application, LogLevel.Debug);
            if (!Utils.AutoFindWoTDirectory(ref Settings.WoTDirectory) || ModpackSettings.ForceManuel)
            {
                Logging.WriteToLog("auto detect failed or user requests manual", Logfiles.Application, LogLevel.Debug);
                OpenFileDialog manualWoTFind = new OpenFileDialog()
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
                if ((bool)manualWoTFind.ShowDialog())
                {
                    Settings.WoTDirectory = manualWoTFind.FileName;
                }
                else
                {
                    Logging.Info("User Canceled installation");
                    ToggleUIButtons(true);
                    return;
                }
            }
            Settings.WoTDirectory = Path.GetDirectoryName(Settings.WoTDirectory);
            Logging.Info("Wot root directory parsed as " + Settings.WoTDirectory);

            //get the version of tanks in the format of the res_mods version folder i.e. 0.9.17.0.3
            string versionTemp = XmlUtils.GetXmlStringFromXPath(Path.Combine(Settings.WoTDirectory, "version.xml"), "//version.xml/version");
            Settings.WoTClientVersion = versionTemp.Split('#')[0].Trim().Substring(2);

            //verify the uninstall
            if (MessageBox.Show(string.Format(Translations.GetTranslatedString("verifyUninstallVersionAndLocation"), Settings.WoTDirectory, ModpackSettings.UninstallMode.ToString()),
                Translations.GetTranslatedString("confirmUninstallHeader"), MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                ToggleUIButtons(true);
                return;
            }

            //create progress object
            Progress<RelhaxInstallerProgress> progress = new Progress<RelhaxInstallerProgress>();
            progress.ProgressChanged += UninstallProgressChanged;

            //create token source
            cancellationTokenSource = new CancellationTokenSource();

            //create and run uninstall engine
            installEngine = new InstallerComponents.InstallEngine
            {
                CancellationToken = cancellationTokenSource.Token
            };
            InstallerComponents.RelhaxInstallFinishedEventArgs results = await installEngine.RunUninstallationAsync(progress);
            installEngine.Dispose();
            installEngine = null;

            //report results
            ChildProgressBar.Value = ChildProgressBar.Maximum;
            if (results.ExitCode == InstallerComponents.InstallerExitCodes.Success)
            {
                InstallProgressTextBox.Text = Translations.GetTranslatedString("uninstallFinished");
                MessageBox.Show(Translations.GetTranslatedString("uninstallFinished"));
            }
            else
            {
                InstallProgressTextBox.Text = Translations.GetTranslatedString("uninstallFail");
                MessageBox.Show(Translations.GetTranslatedString("uninstallFail"));
            }
            ToggleUIButtons(true);
        }

        private void UninstallProgressChanged(object sender, RelhaxInstallerProgress e)
        {
            //no advanced progress for this one
            if (ChildProgressBar.Maximum != e.ChildTotal)
                ChildProgressBar.Maximum = e.ChildTotal;
            if (ChildProgressBar.Value != e.ChildCurrent)
                ChildProgressBar.Value = e.ChildCurrent;
            if (e.UninstallStatus == InstallerComponents.UninstallerExitCodes.GettingFilelistError)
            {
                InstallProgressTextBox.Text = Translations.GetTranslatedString("scanningModsFolders");
            }
            else if (e.UninstallStatus == InstallerComponents.UninstallerExitCodes.UninstallError)
            {
                InstallProgressTextBox.Text = string.Format("{0} {1} {2} {3}{4}{5}", Translations.GetTranslatedString("uninstallingFile"), e.ChildCurrent,
                    Translations.GetTranslatedString("of"), e.ChildTotal, Environment.NewLine, e.Filename);
            }
        }
        #endregion

        #region UI events
        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //if current is 0 then use it as an initial block
            if (currentBytesDownloaded == 0)
            {
                //init elapsed timer
                if(downloadTimer == null)
                {
                    downloadTimer = new Stopwatch();
                }
                downloadTimer.Restart();
                //init update timer
                if(downloadDisplayTimer == null)
                {
                    downloadDisplayTimer = new Timer()
                    {
                        Interval = 1000,
                        AutoReset = true
                    };
                    downloadDisplayTimer.Elapsed += DownloadDisplayTimer_Elapsed;
                }
                downloadDisplayTimer.Stop();
                downloadDisplayTimer.Start();
                //init rates and history
                lastBytesDownloaded = 0;
                downloadRateDisplay = 0;
            }
            currentBytesDownloaded = e.BytesReceived;
            totalBytesToDownload = e.TotalBytesToReceive;

            ChildProgressBar.Maximum = e.TotalBytesToReceive;
            ChildProgressBar.Minimum = 0;
            ChildProgressBar.Value = e.BytesReceived;

            //break it up into lines cause it's hard to read
            //"downloading 2 of 4"
            string line1 = string.Format("{0} {1} {2} {3}",
                Translations.GetTranslatedString("Downloading"), ParentProgressBar.Value, Translations.GetTranslatedString("of"), ParentProgressBar.Maximum);

            //"zip file name"
            string line2 = downloadProgress.ChildCurrentProgress;

            //https://stackoverflow.com/questions/9869346/double-string-format
            //"2MB of 8MB at 1 MB/S"
            string line3 = string.Format("{0} {1} {2} {3} {4}/s",
                Utils.SizeSuffix((ulong)e.BytesReceived, 1, true), Translations.GetTranslatedString("of"), Utils.SizeSuffix((ulong)e.TotalBytesToReceive, 1, true),
                Translations.GetTranslatedString("at"), Utils.SizeSuffix((ulong)downloadRateDisplay,1,true, true));

            //"4 seconds"
            //https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings
            TimeSpan remain = TimeSpan.FromMilliseconds(remainingMilliseconds);
            string line4 = string.Format("{0} {1} {2} {3}", remain.Minutes, Translations.GetTranslatedString("minutes"), remain.ToString(@"ss\.f"),
                Translations.GetTranslatedString("seconds"));

            //also report to the download message process
            InstallProgressTextBox.Text = string.Format("{0}\n{1}\n{2}\n{3}", line1, line2, line3, line4);
        }

        private void DownloadDisplayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //update download rate display values
            downloadRateDisplay = currentBytesDownloaded - lastBytesDownloaded;

            //update download rate ETA values
            //bytes remaining
            long bytesRemainToDownload = totalBytesToDownload - currentBytesDownloaded;

            //overall download rate bytes/msec
            double downloadRateOverall = 0;
            if (downloadTimer.Elapsed.TotalMilliseconds > 0)
                downloadRateOverall =  currentBytesDownloaded / downloadTimer.Elapsed.TotalMilliseconds;

            //remaining time msec
            if ((long)downloadRateOverall > 0)
                remainingMilliseconds = bytesRemainToDownload / (long)downloadRateOverall;
            else
                remainingMilliseconds = 0;

            //set current to previous
            lastBytesDownloaded = currentBytesDownloaded;
        }

        private void ToggleUIButtons(bool toggle)
        {
            Logging.Debug("The main window UI was toggled: {0}", toggle.ToString());
            List<FrameworkElement> controlsToToggle = Utils.GetAllWindowComponentsLogical(this, false);
            //any to remove here
            if (controlsToToggle.Contains(CancelDownloadInstallButton))
                controlsToToggle.Remove(CancelDownloadInstallButton);
            foreach (FrameworkElement control in controlsToToggle)
            {
                if (control is Button || control is CheckBox || control is RadioButton)
                {
                    if (tempDisabledBlacklist.Contains(control))
                        control.IsEnabled = false;
                    else
                        control.IsEnabled = toggle;
                }
            }
            //any to include here that aren't any of the above class types
            AutoSyncFrequencyTexbox.IsEnabled = toggle;
            AutoSyncFrequencyComboBox.IsEnabled = toggle;
        }

        private void OnLinkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Logging.Debug("launching button, link={0}", (sender as LinkButton).Link);
                Process.Start((sender as LinkButton).Link);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
#pragma warning disable CS0162
                if (Settings.ApplicationVersion != ApplicationVersions.Stable)
                    MessageBox.Show(ex.ToString());
#pragma warning enable CS0162
            }
        }

        private void DiagnosticUtilitiesButton_Click(object sender, RoutedEventArgs e)
        {
            Diagnostics diagnostics = new Diagnostics();
            diagnostics.ShowDialog();
        }

        private void ViewNewsButton_Click(object sender, RoutedEventArgs e)
        {
            if (newsViewer == null)
            {
                newsViewer = new NewsViewer();
                newsViewer.Show();
            }
            else if (newsViewer.IsLoaded)
            {
                newsViewer.Focus();
            }
            else
            {
                newsViewer = new NewsViewer();
                newsViewer.Show();
            }
        }

        private void DumpColorSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                AddExtension = true,
                CheckPathExists = true,
                OverwritePrompt = true,
                DefaultExt = "xml",
                Title = Translations.GetTranslatedString("ColorDumpSaveFileDialog"),
                Filter = "XML Documents|*.xml"
            };
            bool result = (bool)saveFileDialog.ShowDialog();
            if (result)
            {
                Logging.Info("Saving color settings dump to " + saveFileDialog.FileName);
                UISettings.DumpAllWindowColorSettingsToFile(saveFileDialog.FileName, this);
                Logging.Info("Color settings saved");
                MessageBox.Show(Translations.GetTranslatedString("DumpColorSettingsSaveSuccess"));
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

        private void CancelDownloadInstallButton_Download_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                Logging.Info("Cancel pressed in download mode (and download while install is false), but client reference is false, cannot cancel!");
                return;
            }
            Logging.Info("Cancel pressed from UI in download mode, sending cancel request");
            client.CancelAsync();
        }

        private void CancelDownloadInstallButton_Install_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info("Cancel press from UI in install mode, processing request");

            //cancel installer
            if (installEngine == null)
            {
                Logging.Error("Cancel request failed because installEngine is null!");
            }
            else if (!cancellationTokenSource.IsCancellationRequested)
            {
                Logging.Info("requesting cancel of installation from UI - cancel process started");
                cancellationTokenSource.Cancel();
            }
            else
            {
                Logging.Info("cancel already started for installer - skipping request");
            }

            //then cancel downloader
            if (ModpackSettings.InstallWhileDownloading)
            {
                Logging.Info("InstallWhileDownloading is true, attempt to cancel download thread");
                if (client == null)
                {
                    Logging.Info("Unable to cancel download thread, client reference is null (maybe no packages to download?)");
                }
                else
                {
                    Logging.Info("Sending cancel request to download thread");
                    client.CancelAsync();
                }
            }
        }

        private void AutoSyncFrequencyTexbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //check the time parsed value
            int timeToUse = Utils.ParseInt(AutoSyncFrequencyTexbox.Text, 0);
            if (timeToUse < 1)
            {
                Logging.Debug("Invalid time specified, must be above 0. not saving");
            }
            else
            {
                ModpackSettings.AutoInstallFrequencyInterval = timeToUse;
            }
        }

        private void AutoSyncFrequencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModpackSettings.AutoInstallFrequencyTimeUnit = AutoSyncFrequencyComboBox.SelectedIndex;
        }

        private void LauchEditor_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info("Launching editor from MainWindow");
            if (!Logging.IsLogDisposed(Logfiles.Application))
                Logging.DisposeLogging(Logfiles.Application);

            CommandLineSettings.ApplicationMode = ApplicationMode.Editor;
            DatabaseEditor editor = new DatabaseEditor() { LaunchedFromMainWindow = true };
            //start updater logging system
            if (!Logging.Init(Logfiles.Editor))
            {
                MessageBox.Show("Failed to initialize logfile for editor");
                editor.Close();
                return;
            }
            Logging.WriteHeader(Logfiles.Editor);
            editor.ShowDialog();

            CommandLineSettings.ApplicationMode = ApplicationMode.Default;
            if (!Logging.Init(Logfiles.Application))
            {
                MessageBox.Show(Translations.GetTranslatedString("appFailedCreateLogfile"));
                Application.Current.Shutdown((int)ReturnCodes.LogfileError);
            }
        }

        private void ApplyCustomScalingSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                //if the new slider and old display scale are the same, then no need to apply
                if(ModpackSettings.DisplayScale == ApplyCustomScalingSlider.Value)
                {
                    Logging.Debug("ModpackSettings.DisplayScale is same as Slider.Value, no need to apply");
                    return;
                }
                ApplyCustomScalingLabel.Text = string.Format("{0}x", ApplyCustomScalingSlider.Value.ToString("N"));
                double oldTempValue = ModpackSettings.DisplayScale;
                ModpackSettings.DisplayScale = ApplyCustomScalingSlider.Value;
                Utils.ApplyApplicationScale(this, ModpackSettings.DisplayScale);
                ScalingConfirmation confirmation = new ScalingConfirmation();
                if(!(bool)confirmation.ShowDialog())
                {
                    ModpackSettings.DisplayScale = oldTempValue;
                    Utils.ApplyApplicationScale(this, ModpackSettings.DisplayScale);
                    ApplyCustomScalingSlider.Value = ModpackSettings.DisplayScale;
                    ApplyCustomScalingLabel.Text = string.Format("{0}x", ApplyCustomScalingSlider.Value.ToString("N"));
                }
            }
        }

        private void ApplyCustomUILocalizations(bool displaySize)
        {
            //set the application information text box
            ApplicationVersionLabel.Text = Translations.GetTranslatedString("applicationVersion") + " " + Utils.GetApplicationVersion();

            //set the database information text box
            DatabaseVersionLabel.Text = Translations.GetTranslatedString("databaseVersion") + " " + Settings.DatabaseVersion;

            //get the number of processor cores
            MulticoreExtractionCoresCountLabel.Text = string.Format(Translations.GetTranslatedString("MulticoreExtractionCoresCountLabel"), Settings.NumLogicalProcesors);

            //display the backup file sizes (if requested)
            if (displaySize)
                BackupModsSizeLabelUsed.Text = string.Format(Translations.GetTranslatedString("BackupModsSizeLabelUsed"), backupFiles.Count(), Utils.SizeSuffix((ulong)backupFolderTotalSize, 1, true));
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
            ModpackSettings.InstallWhileDownloading = (bool)InstallWhileDownloadingCB.IsChecked;
        }

        private void OnShowInstallCompleteWindowChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ShowInstallCompleteWindow = (bool)ShowInstallCompleteWindowCB.IsChecked;
        }

        private void OnBackupModsChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.BackupModFolder = (bool)BackupModsCB.IsChecked;
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
            {
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Beta;
                MessageBox.Show(Translations.GetTranslatedString("noChangeUntilRestart"));
            }
            else if (!(bool)UseBetaApplicationCB.IsChecked)
            {
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Stable;
            }
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
                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    //just load master and call it good. it should always be there
                    UseBetaDatabaseBranches.Items.Clear();
                    UseBetaDatabaseBranches.Items.Add("master");
                    UseBetaDatabaseBranches.SelectedIndex = 0;
                    UseBetaDatabaseBranches.IsEnabled = true;
                    UseBetaDatabaseCB.IsEnabled = true;
                    return;
                }
                JArray root;
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
                foreach (JObject branch in root.Children())
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

            if (databaseVersion == DatabaseVersions.Test)
            {
                MessageBox.Show("Database setting applied, but you are currently in test mode. Test mode will remain active until application restart.");
            }
            else
            {
                databaseVersion = ModpackSettings.DatabaseDistroVersion;
            }

            ProcessTitle();
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
            //Translations.SetLanguage((Languages)LanguagesSelector.SelectedIndex);
            switch (LanguagesSelector.SelectedItem as string)
            {
                case Translations.LanguageEnglish:
                    Translations.SetLanguage(Languages.English);
                    break;
                case Translations.LanguageFrench:
                    Translations.SetLanguage(Languages.French);
                    break;
                case Translations.LanguageGerman:
                    Translations.SetLanguage(Languages.German);
                    break;
                case Translations.LanguagePolish:
                    Translations.SetLanguage(Languages.Polish);
                    break;
                case Translations.LanguageRussian:
                    Translations.SetLanguage(Languages.Russian);
                    break;
                case Translations.LanguageSpanish:
                    Translations.SetLanguage(Languages.Spanish);
                    break;
            }
            Translations.LocalizeWindow(this, true);
            ApplyCustomUILocalizations(!loading);
        }

        private void VerboseLoggingCB_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.VerboseLogging = (bool)VerboseLoggingCB.IsChecked;
        }

        private void DisableTriggersCB_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.DisableTriggers = (bool)DisableTriggersCB.IsChecked;
        }

        private void DeleteOldCacheFiles_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.DeleteCacheFiles = (bool)DeleteOldPackagesCB.IsChecked;
        }

        private void MinimizeToSystemTray_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.MinimizeToSystemTray = (bool)MinimizeToSystemTray.IsChecked;
        }

        private void UninstallDefault_Checked(object sender, RoutedEventArgs e)
        {
            ModpackSettings.UninstallMode = UninstallModes.Default;
        }

        private void UninstallQuick_Checked(object sender, RoutedEventArgs e)
        {
            ModpackSettings.UninstallMode = UninstallModes.Quick;
        }

        private void OneClickInstallCB_Click(object sender, RoutedEventArgs e)
        {
            string tmep = string.Empty;
            if (string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath) || !File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
            {
                tmep = ModpackSettings.AutoOneclickSelectionFilePath;
                Logging.Debug("autoClickSelectionPath is null or doesn't exist, prompting user to change");
                LoadAutoSyncSelectionFile_Click(null, null);
            }

            if (string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath) || !File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
            {
                Logging.Debug("autoClickSelectionPath is null or doesn't exist still, setting to false and reverting path");
                ModpackSettings.AutoOneclickSelectionFilePath = tmep;
                ModpackSettings.OneClickInstall = false;
                OneClickInstallCB.IsChecked = false;
            }
            else
            {
                ModpackSettings.OneClickInstall = (bool)OneClickInstallCB.IsChecked;
            }
        }

        private void LoadAutoSyncSelectionFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog selectAutoSyncSelectionFileDialog = new OpenFileDialog()
            {
                Filter = "*.xml|*.xml",
                Title = Translations.GetTranslatedString("MainWindowSelectSelectionFileToLoad"),
                InitialDirectory = Settings.RelhaxUserSelectionsFolderPath,
                Multiselect = false
            };
            if (!(bool)selectAutoSyncSelectionFileDialog.ShowDialog())
                return;
            AutoInstallOneClickInstallSelectionFilePath.Text = selectAutoSyncSelectionFileDialog.FileName;
            ModpackSettings.AutoOneclickSelectionFilePath = selectAutoSyncSelectionFileDialog.FileName;
        }

        private void ForceEnabledCB_Clicked(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ForceEnabled = (bool)ForceEnabledCB.IsChecked;
        }

        private void ForceVisibleCB_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ForceVisible = (bool)ForceVisibleCB.IsChecked;
        }

        private void ExportModeCB_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ExportMode = (bool)ExportModeCB.IsChecked;
        }

        private void Theme_Checked(object sender, RoutedEventArgs e)
        {
            //ModpackSettings is desited theme
            if ((bool)ThemeDefault.IsChecked)
                ModpackSettings.ApplicationTheme = UIThemes.Default;
            else if ((bool)ThemeDark.IsChecked)
                ModpackSettings.ApplicationTheme = UIThemes.Dark;
            else if ((bool)ThemeCustom.IsChecked)
                ModpackSettings.ApplicationTheme = UIThemes.Custom;
            //try to apply it
            UISettings.ApplyUIColorSettings(this);
            //load the result back in
            if (UISettings.CurrentTheme.Equals(Themes.Default))
                ThemeDefault.IsChecked = true;
            else if (UISettings.CurrentTheme.Equals(Themes.Dark))
                ThemeDark.IsChecked = true;
            else if (UISettings.CurrentTheme.Equals(Themes.Custom))
                ThemeCustom.IsChecked = true;
        }

        private void SaveDisabledModsInSelection_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.SaveDisabledMods = (bool)SaveDisabledModsInSelection.IsChecked;
        }

        private void AutoInstallCB_Click(object sender, RoutedEventArgs e)
        {
            string tmep = string.Empty;
            if (string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath) || !File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
            {
                tmep = ModpackSettings.AutoOneclickSelectionFilePath;
                Logging.Debug("autoClickSelectionPath is null or doesn't exist, prompting user to change");
                LoadAutoSyncSelectionFile_Click(null, null);
            }

            if (string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath) || !File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
            {
                Logging.Debug("autoClickSelectionPath is null or doesn't exist still, setting to false and reverting path");
                ModpackSettings.AutoOneclickSelectionFilePath = tmep;
                ModpackSettings.AutoInstall = false;
                AutoInstallCB.IsChecked = false;
                return;
            }

            //check the time parsed value
            int timeToUse = Utils.ParseInt(AutoSyncFrequencyTexbox.Text, 0);
            if (timeToUse < 1)
            {
                Logging.Info("Invalid time specified, must be above 0");
                MessageBox.Show("InvalidTimeNumberSpecified");
                ModpackSettings.AutoInstall = false;
                AutoInstallCB.IsChecked = false;
                return;
            }

            //parse the time into a timespan for the check timer
            switch (AutoSyncFrequencyComboBox.SelectedIndex)
            {
                case 0:
                    autoInstallTimer.Interval = TimeSpan.FromMinutes(timeToUse).TotalMilliseconds;
                    break;
                case 1:
                    autoInstallTimer.Interval = TimeSpan.FromHours(timeToUse).TotalMilliseconds;
                    break;
                case 2:
                    autoInstallTimer.Interval = TimeSpan.FromDays(timeToUse).TotalMilliseconds;
                    break;
                default:
                    throw new BadMemeException("this should not happen");
            }
            autoInstallTimer.AutoReset = true;
            if (!autoInstallTimerRegistered)
            {
                Logging.Debug("auto install timer not registered to event, setting");
                autoInstallTimer.Elapsed += AutoInstallTimer_Elapsed;
                autoInstallTimerRegistered = true;
            }
            else
            {
                Logging.Debug("auto install timer already registered");
            }
            ModpackSettings.AutoInstall = (bool)AutoInstallCB.IsChecked;
            if (ModpackSettings.AutoInstall)
                autoInstallTimer.Enabled = true;
            else
                autoInstallTimer.Enabled = false;
        }

        private void AutoInstallTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logging.Debug("timer has elapsed to check for database updates");
            CheckForDatabaseUpdatesPeriodic(true);
            if (databaseUpdateAvailableFromAutoSync)
            {
                Logging.Debug("update found from auto install, running installation");
                InstallModpackButton_Click(null, null);
            }
        }

        private void AllowStatsGatherCB_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.AllowStatisticDataGather = (bool)AllowStatsGatherCB.IsChecked;
        }

        private void AdvancedInstallationProgress_Click(object sender, RoutedEventArgs e)
        {
            ModpackSettings.AdvancedInstalProgress = (bool)AdvancedInstallationProgress.IsChecked;
        }

        private void ApplySettingsToUI()
        {
            //apply the internal setting to what the UI setting is
            //UI component = internal setting
            //simple settings first
            NotifyIfSameDatabaseCB.IsChecked = ModpackSettings.NotifyIfSameDatabase;
            BackupModsCB.IsChecked = ModpackSettings.BackupModFolder;
            CleanInstallCB.IsChecked = ModpackSettings.CleanInstallation;
            ForceManuelGameDetectionCB.IsChecked = ModpackSettings.ForceManuel;
            SaveLastInstallCB.IsChecked = ModpackSettings.SaveLastSelection;
            SaveUserDataCB.IsChecked = ModpackSettings.SaveUserData;
            SaveDisabledModsInSelection.IsChecked = ModpackSettings.SaveDisabledMods;
            VerboseLoggingCB.IsChecked = ModpackSettings.VerboseLogging;
            AllowStatsGatherCB.IsChecked = ModpackSettings.AllowStatisticDataGather;
            EnableBordersDefaultV2CB.IsChecked = ModpackSettings.EnableBordersDefaultV2View;
            EnableColorChangeDefaultV2CB.IsChecked = ModpackSettings.EnableColorChangeDefaultV2View;
            EnableBordersLegacyCB.IsChecked = ModpackSettings.EnableBordersLegacyView;
            EnableColorChangeLegacyCB.IsChecked = ModpackSettings.EnableColorChangeLegacyView;
            ShowInstallCompleteWindowCB.IsChecked = ModpackSettings.ShowInstallCompleteWindow;
            ClearCacheCB.IsChecked = ModpackSettings.ClearCache;
            ClearLogFilesCB.IsChecked = ModpackSettings.DeleteLogs;
            CreateShortcutsCB.IsChecked = ModpackSettings.CreateShortcuts;
            InstallWhileDownloadingCB.IsChecked = ModpackSettings.InstallWhileDownloading;
            MulticoreExtractionCB.IsChecked = ModpackSettings.MulticoreExtraction;
            ExportModeCB.IsChecked = ModpackSettings.ExportMode;
            ForceEnabledCB.IsChecked = ModpackSettings.ForceEnabled;
            ForceVisibleCB.IsChecked = ModpackSettings.ForceVisible;
            DisableTriggersCB.IsChecked = ModpackSettings.DisableTriggers;
            OneClickInstallCB.IsChecked = ModpackSettings.OneClickInstall;
            AutoInstallCB.IsChecked = ModpackSettings.AutoInstall;
            DeleteOldPackagesCB.IsChecked = ModpackSettings.DeleteCacheFiles;
            MinimizeToSystemTray.IsChecked = ModpackSettings.MinimizeToSystemTray;
            AdvancedInstallationProgress.IsChecked = ModpackSettings.AdvancedInstalProgress;

            //apply auto sync time unit and amount
            AutoSyncFrequencyTexbox.Text = ModpackSettings.AutoInstallFrequencyInterval.ToString();
            if (ModpackSettings.AutoInstallFrequencyTimeUnit < AutoSyncFrequencyComboBox.Items.Count && ModpackSettings.AutoInstallFrequencyTimeUnit > 0)
            {
                AutoSyncFrequencyComboBox.SelectedIndex = ModpackSettings.AutoInstallFrequencyTimeUnit;
            }
            else
            {
                if(!loading)
                    Logging.Warning("AutoInstallFrequencyTimeUnit is not valid selection, setting to default");
                AutoSyncFrequencyComboBox.SelectedIndex = 0;
            }

            if (!string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath))
                AutoInstallOneClickInstallSelectionFilePath.Text = ModpackSettings.AutoOneclickSelectionFilePath;

            //setup the languages selector
            switch (ModpackSettings.Language)
            {
                case Languages.English:
                    LanguagesSelector.SelectedItem = Translations.LanguageEnglish;
                    break;
                case Languages.French:
                    LanguagesSelector.SelectedItem = Translations.LanguageFrench;
                    break;
                case Languages.German:
                    LanguagesSelector.SelectedItem = Translations.LanguageGerman;
                    break;
                case Languages.Polish:
                    LanguagesSelector.SelectedItem = Translations.LanguagePolish;
                    break;
                case Languages.Russian:
                    LanguagesSelector.SelectedItem = Translations.LanguageRussian;
                    break;
                case Languages.Spanish:
                    LanguagesSelector.SelectedItem = Translations.LanguageSpanish;
                    break;
            }

            //setup the selection view
            switch (ModpackSettings.ModSelectionView)
            {
                case SelectionView.DefaultV2:
                    SelectionDefault.IsChecked = true;
                    break;

                case SelectionView.Legacy:
                    SelectionLegacy.IsChecked = true;
                    break;
            }

            //setup uninstall view
            switch (ModpackSettings.UninstallMode)
            {
                case UninstallModes.Default:
                    UninstallDefault.IsChecked = true;
                    break;
                case UninstallModes.Quick:
                    UninstallQuick.IsChecked = true;
                    break;
            }

            switch(ModpackSettings.ApplicationTheme)
            {
                case UIThemes.Default:
                    ThemeDefault.IsChecked = true;
                    break;
                case UIThemes.Dark:
                    ThemeDark.IsChecked = true;
                    break;
                case UIThemes.Custom:
                    ThemeCustom.IsChecked = true;
                    break;
            }

            //apply beta database settings
            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Beta)
            {
                UseBetaDatabaseCB.IsChecked = true;
                OnUseBetaDatabaseChanged(true, null);
            }

            //apply auto install check
            if (ModpackSettings.AutoInstall)
            {
                AutoInstallCB_Click(null, null);
            }
        }

        private void OpenColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            RelhaxColorPicker colorPicker = new RelhaxColorPicker();
            colorPicker.ShowDialog();
        }
        #endregion

        //move folders with a special middle step
        private void MoveUpgradeFolder(string oldPath, string newPath)
        {
            Logging.Info("upgrading folder {0} to {1}", Path.GetFileName(oldPath), Path.GetFileName(newPath));
            if (!Directory.Exists(oldPath))
            {
                Logging.Warning("old folder {0} does not exist, skipping", Path.GetFileName(oldPath));
                return;
            }

            if (Directory.Exists(newPath) && newPath.Equals(Settings.RelhaxUserSelectionsFolderPath))
            {
                Logging.Warning("new folder {0} already exists, copy files over and delete old folder", Path.GetFileName(newPath));
                foreach (string file in Utils.DirectorySearch(oldPath, SearchOption.TopDirectoryOnly, false, "*.xml", 5, 3, false))
                {
                    string newFilePath = Path.Combine(Settings.RelhaxUserSelectionsFolderPath, Path.GetFileName(file));
                    if (!File.Exists(newFilePath))
                        File.Copy(file, newFilePath);
                }
                Utils.DirectoryDelete(oldPath, true);
                return;
            }


            //step 1 is to move it to a temp folder
            string middlePath = oldPath + "_";
            Directory.Move(oldPath, middlePath);

            //step 2 is to move it to the real folder
            Directory.Move(middlePath, newPath);

            Logging.Info("upgrade of folder {0} successful", Path.GetFileName(newPath));
        }
    }
}
