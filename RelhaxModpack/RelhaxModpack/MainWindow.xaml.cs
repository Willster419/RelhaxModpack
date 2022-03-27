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
using RelhaxModpack.UI;
using System.Xml;
using System.Diagnostics;
using Ionic.Zip;
using System.Net.Http;
using System.Threading;
using Microsoft.Win32;
using System.Text;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows.Threading;
using RelhaxModpack.Atlases;
using RelhaxModpack.Xml;
using RelhaxModpack.Utilities;
using RelhaxModpack.Database;
using System.Windows.Media;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Settings;
using RelhaxModpack.Common;
using RelhaxModpack.Utilities.ClassEventArgs;
using RelhaxModpack.UI.Extensions;

namespace RelhaxModpack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RelhaxWindow
    {
        #region Variables
        //install and uninstall engine variables
        private InstallEngine installEngine = null;
        private CancellationTokenSource installerCancellationTokenSource = null;
        private CancellationTokenSource downloaderCancellationTokenSource = null;
        private bool disableTriggersForInstall = true;
        private DatabaseVersions databaseVersion;

        //UI variables
        //flag for if the application is in loading mode to stop unintended method firing
        private bool loading = false;
        private string oldModpackTitle = string.Empty;
        //temp list of components not to toggle
        private Control[] disabledBlacklist = null;
        private Control[] enabledBlacklist = null;
        //timer of when to check the server for a database update
        private DispatcherTimer autoInstallPeriodicTimer = null;
        //notification tray icon
        private System.Windows.Forms.NotifyIcon RelhaxIcon = null;

        //custom application windows
        private PackageSelectionList modSelectionList = null;
        private AdvancedProgress AdvancedProgressWindow = null;
        private NewsViewer newsViewer = null;

        //application updater variables
        //flag for if the application is in "update mode" (downloading the new application update and closing)
        private bool updateMode = false;
        //task during update mode to get the manager info document as well
        private Task<ZipFile> DownloadManagerInfoZip = null;

        //modpack install backup variables
        //flag if the backup size has been calculated
        private bool BackupSizeCalculated = false;
        //backup total file size
        private long backupFolderTotalSize = 0;
        //backup list of files
        private string[] backupFiles = null;

        //download variables
        private DownloadManager downloadManager;
        //measures elapsed time since download started
        private Stopwatch downloadTimer = null;
        //timer to fire every second to update the display download rate
        private DispatcherTimer downloadDisplayTimer = null;
        //for download rate display, last internal's bytes downloaded
        private long lastBytesDownloaded;
        //for both rates, the current bytes downloaded
        private long currentBytesDownloaded;
        //for eta rate, the total bytes needed to download
        private long totalBytesToDownload;
        //download rate over the last second
        private double downloadRateDisplay;
        //remaining time
        private long remainingMilliseconds;
        private bool deferToDownloadReport = false;

        //database manager
        private DatabaseManager databaseManager;

        //task bar variables
        private TaskbarManager taskbarInstance = null;
        private TaskbarProgressBarState taskbarState = TaskbarProgressBarState.NoProgress;
        private int taskbarValue = 0;

        //auto install variables
        //beta database comparison for auto install
        private string oldBetaDB, newBetaDB;
        //auto install timer dispatcher flag to ensure only one even trigger happens at a time
        private bool timerActive = false;

        /// <summary>
        /// The location of the WoT installation directory parsed at installation time
        /// </summary>
        /// <remarks>The path is absolute, ending at "World_of_Tanks"</remarks>
        private string WoTDirectory = string.Empty;

        /// <summary>
        /// The version information of WoT parsed at installation time
        /// </summary>
        /// <remarks>This info is gathered from the "version.xml" file from the game's root directory</remarks>
        private string WoTClientVersion = string.Empty;

        /// <summary>
        /// The version of the online folder name containing the zip files for this game parsed at installation time
        /// </summary>
        /// <remarks>The online folders are done by major versions only i.e. 1.4.1, 1.5.0, etc. All zip files on 1.5.0.x are stored in this folder</remarks>
        private string WoTModpackOnlineFolderVersion = string.Empty;

        /// <summary>
        /// The version of the database parsed upon application load
        /// </summary>
        private string DatabaseVersion = string.Empty;

        /// <summary>
        /// Determines if this is the first time the application is loading, parsed upon application load
        /// </summary>
        /// <remarks>Done by checking if the settings file exists. If it is set to true in the application, it will be set to false again when it closes.</remarks>
        private bool FirstLoad = false;

        /// <summary>
        /// Determines if while being the first time loading, if this is an upgrade operation to Relhax V2, parsed upon application load
        /// </summary>
        /// <remarks>Done by if FirstLoad is true and the Relhax V1 settings file exists</remarks>
        private bool FirstLoadToV2 = false;
        #endregion

        #region MainWindow loading
        /// <summary>
        /// Creates the instance of the MainWindow class
        /// </summary>
        /// <param name="modpackSettings">The modpack settings object</param>
        public MainWindow(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
            //set window state as minimized in constructor to allow load window to show instead
            WindowState = WindowState.Minimized;
            disabledBlacklist = new Control[]
            {
                
            };
            enabledBlacklist = new Control[]
            {
                ViewNewsButton,
                Forms_ENG_EUButton,
                Forms_GER_EUButton,
                Forms_ENG_NAButton,
                DiscordButton,
                HomepageButton,
                FindBugAddModButton,
                SendEmailButton,
                DonateButton,
                LanguagesSelector
            };
        }

        private async void TheMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //set loading flag
            loading = true;

            //get task bar instance for color change if supported
            if (TaskbarManager.IsPlatformSupported && TaskbarManager.Instance != null)
            {
                taskbarInstance = TaskbarManager.Instance;
                taskbarInstance.SetProgressState(taskbarState);
            }

            //delete the updater scripts if they exist
#pragma warning disable CS0618
            foreach (string s in new string[] {
                ApplicationConstants.RelicBatchUpdateScript,
                ApplicationConstants.RelicBatchUpdateScriptOld,
                ApplicationConstants.ApplicationUpdateFileNameZip,
                ApplicationConstants.ApplicationUpdateFilenameExe, })
            {
                if (File.Exists(s))
                {
                    Logging.Debug("{0} found, deleting", s);
                    FileUtils.FileDelete(s);
                }
            }
#pragma warning restore CS0618

            //disconnect event handler before translation combobox is modified
            LanguagesSelector.SelectionChanged -= OnLanguageSelectionChanged;

            //load the supported translations into combobox
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

            //reconnect event handler
            LanguagesSelector.SelectionChanged += OnLanguageSelectionChanged;

            //set this window to use RelhaxWindow properties
            LocalizeWindow = true;
            ApplyToolTips = true;

            //apply translation settings now that they are loaded
            Translations.SetLanguage(ModpackSettings.Language);
            Translations.LocalizeWindow(this, true);
            ApplyCustomUILocalizations();

            //load the progress report window
            ProgressIndicator progressIndicator = new ProgressIndicator(this.ModpackSettings)
            {
                LocalizeWindow = false,
                Message = "Loading...",
                ProgressMinimum = 0,
                ProgressMaximum = 4,
                CommandLineSettings = this.CommandLineSettings
            };
            progressIndicator.Show();
            progressIndicator.UpdateProgress(0);
            UiUtils.AllowUIToUpdate();

            //load the download mirrors into the combobox
            SelectDownloadMirrorCombobox.Items.Clear();
            SelectDownloadMirrorCombobox.Items.Add(Translations.GetTranslatedString("downloadMirrorUsaDefault"));
            SelectDownloadMirrorCombobox.Items.Add(Translations.GetTranslatedString("downloadMirrorDe"));


            //create tray icons and menus
            CreateTray();

            //apply UI settings
            progressIndicator.UpdateProgress(2, Translations.GetTranslatedString("loadingSettings"));

            //note: if loadSettings load the language, apply to UI sets the UI option and triggers translation of MainWindow
            //note: in WPF, the enabled trigger will occur in the loading event, so this will launch the checked events
            //setting a isChecked checkbox will not launch the click event
            ApplySettingsToUI();

            //run the checked code to ensure that the font is loaded into the custom selector
            EnableCustomFontCheckbox_Click(null, null);

            //apply custom font if enabled and exists. if not, it should have already been taken care of in EnableCustomFontCheckbox_Click()
            if (ModpackSettings.EnableCustomFont)
            {
                Logging.Debug(LogOptions.MethodName, "Attempting to apply font {0}", ModpackSettings.CustomFontName);
                List<string> fontNames = CustomFontSelector.Items.SourceCollection.Cast<TextBlock>().Select(font => font.FontFamily.Source.Split('#')[1]).ToList();
                string fontName = fontNames.Find(match => match.Equals(ModpackSettings.CustomFontName));
                if (string.IsNullOrEmpty(fontName))
                {
                    Logging.Error("Custom font {0} not found on system, disable custom font selection", fontName);
                    ModpackSettings.CustomFontName = string.Empty;
                    ModpackSettings.EnableCustomFont = false;
                    EnableCustomFontCheckbox.IsEnabled = false;
                    CustomFontSelector.IsEnabled = false;
                    CustomFontSelector.SelectedItem = null;
                }
                else
                {
                    Logging.Debug("Custom font {0} found on system, applying", fontName);

                    //select the fontName that matches in the checkbox
                    bool selected = false;
                    foreach(TextBlock block in CustomFontSelector.Items)
                    {
                        if(block.FontFamily.Source.Split('#')[1].Equals(ModpackSettings.CustomFontName))
                        {
                            CustomFontSelector.SelectedItem = block;
                            selected = true;
                            CustomFontSelector_SelectionChanged(null, null);
                            break;
                        }
                    }
                    if (!selected)
                        throw new BadMemeException("The font was never set in the selector combobox");
                }
            }

            //setup the database manager helper class
            databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);

            //save database version to temp and process if command line test mode
            databaseVersion = ModpackSettings.DatabaseDistroVersion;
            if (CommandLineSettings.TestMode)
            {
                Logging.Info("Test mode set for application instance only (not saved to settings)");
                databaseVersion = DatabaseVersions.Test;
                Logging.Info("Test mode, disable statistics upload if enabled");
                if (ModpackSettings.AllowStatisticDataGather)
                {
                    //2020-02-02 checked in debugger and the event is not triggered by setting UI version to false
                    AllowStatsGatherCB.IsChecked = false;
                    ModpackSettings.AllowStatisticDataGather = false;
                }
            }

            //verify folder structure for all folders in the directory
            //this also serves as checking write permissions from the current working directory
            progressIndicator.UpdateProgress(3, Translations.GetTranslatedString("verDirStructure"));
            UiUtils.AllowUIToUpdate();
            Logging.Info("Verifying folder structure");
            foreach (string s in ApplicationConstants.FoldersToCheck)
            {
                try
                {
                    if (!Directory.Exists(s))
                        Directory.CreateDirectory(s);
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog("Failed to check application folder structure\n" + ex.ToString(), Logfiles.Application, LogLevel.Exception);
                    MessageBox.Show(Translations.GetTranslatedString("failedVerifyFolderStructure"));
                    Close();
                    Environment.Exit(0);
                    return;
                }
            }
            Logging.Info("Structure verified");

            //set the application appData directory
            if (!Directory.Exists(ApplicationConstants.AppDataFolder))
            {
                Logging.WriteToLog(string.Format("AppDataFolder does not exist at {0}, creating it", ApplicationConstants.AppDataFolder),
                    Logfiles.Application, LogLevel.Warning);
                Directory.CreateDirectory(ApplicationConstants.AppDataFolder);
            }

            //check for updates to application
            progressIndicator.UpdateProgress(4, Translations.GetTranslatedString("checkForUpdates"));
            bool isApplicationUpToDate = await CheckForApplicationUpdates();

            //if not up to date, ask if user wants to update
            if(!isApplicationUpToDate)
            {
                Logging.Info("Application is out of date, display update window");
                VersionInfo versionInfo = new VersionInfo(this.ModpackSettings);
                versionInfo.ShowDialog();

                if(!versionInfo.ConfirmUpdate)
                {
                    Logging.Info("Application is not up to date and user said don't update. we're done here.");
                    //Close() will still run the _close event handler
                    Close();
                    //https://stackoverflow.com/questions/57654546/taskcanceledexception-after-closing-window
                    Environment.Exit(0);
                    return;
                }

                //disable the UI during the application update process
                updateMode = true;
                ToggleUIButtons(false);

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
                            Close();
                            Environment.Exit(0);
                            return;
                        }
                    }
                    else
                        break;
                }

                //start download of new version
                using (WebClient client = new WebClient())
                {
                    //start download of new version
                    client.DownloadProgressChanged += OnUpdateDownloadProgresChange;
                    client.DownloadFileCompleted += OnUpdateDownloadCompleted;

                    //start to download the manager info file as well
                    Logging.Info("Starting task GetManagerInfoZipfileAsync()");
                    DownloadManagerInfoZip = CommonUtils.GetManagerInfoZipfileAsync(false);

                    //set the UI for a download
                    ResetUI();
                    downloadTimer = new Stopwatch();

                    //check to make sure this window is displayed for progress
                    if (WindowState != WindowState.Normal)
                        WindowState = WindowState.Normal;

                    //download the file
                    string modpackURL = (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Stable) ?
                        ApplicationConstants.ApplicationUpdateURL :
                        ApplicationConstants.ApplicationBetaUpdateURL;

                    //make sure to delete it if it's currently three
                    if (File.Exists(ApplicationConstants.ApplicationUpdateFileNameZip))
                        File.Delete(ApplicationConstants.ApplicationUpdateFileNameZip);
                    client.DownloadFileAsync(new Uri(modpackURL), ApplicationConstants.ApplicationUpdateFileNameZip);
                }

                //getting here means it's out of date and the update was accepted and started. just return
                return;
            }

            //check for updates to database
            await CheckForDatabaseUpdatesAsync(false);

            //set the file count and size for the backups folder
            Logging.Debug("Application is up to date, get file size of backups");
            GetBackupFilesizesAsync();

            //if the application is up to date, then check if we need to display the welcome message to the user
            Logging.Info("Application is up to date, checking to display welcome message");

            //run checks to see if it's the first time loading the application
            FirstLoad = !File.Exists(ModpackSettings.SettingsFilename) && !File.Exists(ModpackSettings.OldSettingsFilename);
            FirstLoadToV2 = !File.Exists(ModpackSettings.SettingsFilename) && File.Exists(ModpackSettings.OldSettingsFilename);
            Logging.Info("FirstLoading = {0}, FirstLoadingV2 = {1}", FirstLoad.ToString(), FirstLoadToV2.ToString());

            if (FirstLoad || FirstLoadToV2)
            {
                //display the selection of language if it's the first time loading (not an upgrade)
                if (FirstLoad && !FirstLoadToV2)
                {
                    FirstLoadSelectLanguage firstLoadSelectLanguage = new FirstLoadSelectLanguage(this.ModpackSettings);
                    firstLoadSelectLanguage.ShowDialog();
                    if (!firstLoadSelectLanguage.Continue)
                    {
                        Logging.Info("User did not select language, closing");
                        Close();
                        Environment.Exit(0);
                        return;
                    }
                    LanguagesSelector.SelectionChanged -= OnLanguageSelectionChanged;
                    LanguagesSelector.SelectedItem = Translations.GetLanguageNativeName(ModpackSettings.Language);
                    LanguagesSelector.SelectionChanged += OnLanguageSelectionChanged;
                    Translations.LocalizeWindow(this, true);
                    ApplyCustomUILocalizations();
                }

                //display the welcome window and make sure the user agrees to it
                FirstLoadAcknowledgments firstLoadAknowledgements = new FirstLoadAcknowledgments(this.ModpackSettings) { FirstLoadToV2 = this.FirstLoadToV2 };
                firstLoadAknowledgements.ShowDialog();
                if (!firstLoadAknowledgements.UserAgreed)
                {
                    Logging.Info("User did not agree to application load conditions, closing");
                    Close();
                    Environment.Exit(0);
                    return;
                }
                //if user agreed and its the first time loading in v2, the do the structure upgrade
                else if (FirstLoadToV2)
                {
                    progressIndicator.UpdateProgress(2, Translations.GetTranslatedString("upgradingStructure"));
                    UiUtils.AllowUIToUpdate();
                    Logging.Info("Starting upgrade to V2");

                    //process libraries folder
                    Logging.Info("Upgrade folders to new names");
#pragma warning disable CS0612
                    MoveUpgradeFolder(ApplicationConstants.RelhaxDownloadsFolderPathOld, ApplicationConstants.RelhaxDownloadsFolderPath);
                    MoveUpgradeFolder(ApplicationConstants.RelhaxModBackupFolderPathOld, ApplicationConstants.RelhaxModBackupFolderPath);
                    MoveUpgradeFolder(ApplicationConstants.RelhaxUserSelectionsFolderPathOld, ApplicationConstants.RelhaxUserSelectionsFolderPath);
                    MoveUpgradeFolder(ApplicationConstants.RelhaxUserModsFolderPathOld, ApplicationConstants.RelhaxUserModsFolderPath);
                    MoveUpgradeFolder(ApplicationConstants.RelhaxTempFolderPathOld, ApplicationConstants.RelhaxTempFolderPath);
                    MoveUpgradeFolder(ApplicationConstants.RelhaxLibrariesFolderPathOld, ApplicationConstants.RelhaxLibrariesFolderPath);
#pragma warning restore CS0612

                    //process xml settings file
                    //delete the new one, move the old one, reload settings
                    Logging.Info("Moving, loading settings");
                    File.Move(ModpackSettings.OldSettingsFilename, ModpackSettings.SettingsFilename);
                    SettingsParser parser = new SettingsParser();
                    parser.LoadSettings(ModpackSettings);
                    ApplySettingsToUI();

                    //process log file
                    Logging.Info("Moving and re-init of logging system");
                    if (File.Exists(Logging.OldApplicationLogFilename))
                    {
                        Logging.DisposeLogging(Logfiles.Application);
                        string tempNewLogText = File.ReadAllText(Logging.ApplicationLogFilename);
                        File.Delete(Logging.ApplicationLogFilename);
                        File.Move(Logging.OldApplicationLogFilename, Logging.ApplicationLogFilename);
                        File.AppendAllText(Logging.ApplicationLogFilename, tempNewLogText);
                        Logging.Init(Logfiles.Application, ModpackSettings.VerboseLogging, true, Logging.ApplicationLogFilename);
                    }
                    else
                        Logging.Info("Skipped (old log does not exist)");
                    Logging.Info("Upgrade to V2 complete, welcome to the future!");
                }
            }

            //save the old modpack title
            oldModpackTitle = Title;
            ProcessTitle();

            //if the editor unlock file exists, then enable the editor button
            if (File.Exists(ApplicationConstants.EditorLaunchFromMainWindowFilename))
            {
                Logging.Info("{0} found, enabling manager tools buttons", ApplicationConstants.EditorLaunchFromMainWindowFilename);
                LauchEditor.Visibility = Visibility.Visible;
                LauchEditor.IsEnabled = true;
                LauchAutomationRunner.Visibility = Visibility.Visible;
                LauchAutomationRunner.IsEnabled = true;

                //also disable update statistics
                Logging.Info("Also disable upload statistics since this is a developer environment");
                if (ModpackSettings.AllowStatisticDataGather)
                {
                    AllowStatsGatherCB.IsChecked = false;
                    ModpackSettings.AllowStatisticDataGather = false;
                }
            }
            else
            {
                LauchEditor.Visibility = Visibility.Hidden;
                LauchEditor.IsEnabled = false;
                LauchAutomationRunner.Visibility = Visibility.Hidden;
                LauchAutomationRunner.IsEnabled = false;
            }

            //dispose of please wait here
            if (progressIndicator != null)
            {
                progressIndicator.Close();
                progressIndicator = null;
            }

            //show the UI if not closing the application out of failure
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
                ApplyApplicationScale(ModpackSettings.DisplayScale);
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
                if (!File.Exists(Path.Combine(ApplicationConstants.RelhaxUserSelectionsFolderPath, CommandLineSettings.AutoInstallFileName)))
                {
                    Logging.Error("configuration file not found in {0}, aborting", ApplicationConstants.RelhaxUserSelectionsFolderPath);
                    CommandLineSettings.AutoInstallFileName = string.Empty;
                }
                else
                {
                    Logging.Info("file exists, launching modpack installation!");
                    InstallModpackButton_Click(null, null);
                }
            }
            //loading in normal mode, check if atlas image processing libraries can be loaded
            else if (!ModpackSettings.AtlasLibrariesCanBeLoaded)
            {
                Logging.Info("Atlas libraries never recorded being loaded, testing now via async task");
                Task.Run(async () =>
                {
                    await Task.Delay(1000);

                    ModpackSettings.AtlasLibrariesCanBeLoaded = AtlasUtils.TestLoadAtlasLibraries(true);

                    //if after test, it fails, inform the user
                    if (!ModpackSettings.AtlasLibrariesCanBeLoaded)
                    {
                        if (MessageBox.Show(string.Format("{0}\n{1}", Translations.GetTranslatedString("missingMSVCPLibraries"), Translations.GetTranslatedString("openLinkToMSVCP")),
                            Translations.GetTranslatedString("missingMSVCPLibrariesHeader"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            if (!CommonUtils.StartProcess(AtlasUtils.MSVCPLink))
                            {
                                Logging.Error("failed to open url to MSVCP: {0}", AtlasUtils.MSVCPLink);
                            }
                        }
                    }
                });
            }

            //unset loading flag
            loading = false;
        }
        #endregion

        #region MainWindow closing
        private void TheMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ModpackSettings.MinimizeToSystemTray)
            {
                Logging.Debug("minimizing to system tray");
                Hide();
                e.Cancel = true;
            }
            else
            {
                CloseApplication();
            }
        }

        private void CloseApplication()
        {
            //dispose of the timer if it's not already disposed
            Logging.TryWriteToLog("Disposing autoInstallTimer", Logfiles.Application, LogLevel.Debug);
            if (autoInstallPeriodicTimer != null)
            {
                autoInstallPeriodicTimer = null;
            }

            //don't save the settings file if it's in update mode or closing from a critical application failure
            if (updateMode)
            {
                Logging.TryWriteToLog("UpdateMode = true, don't save settings", Logfiles.Application, LogLevel.Debug);
            }
            else if(loading)
            {
                Logging.TryWriteToLog("loading = true, so never a clean load, don't save settings", Logfiles.Application, LogLevel.Debug);
            }
            else
            {
                Logging.TryWriteToLog("Saving settings", Logfiles.Application, LogLevel.Info);
                SettingsParser parser = new SettingsParser();
                parser.SaveSettings(ModpackSettings);
                Logging.TryWriteToLog("Settings saved", Logfiles.Application, LogLevel.Info);
            }

            Logging.TryWriteToLog("Disposing tray", Logfiles.Application, LogLevel.Info);

            if (RelhaxIcon != null)
            {
                RelhaxIcon.Visible = false;
                RelhaxIcon.Dispose();
                RelhaxIcon = null;
                Logging.TryWriteToLog("Tray disposed", Logfiles.Application, LogLevel.Info);
            }
            else
                Logging.TryWriteToLog("Tray already disposed?", Logfiles.Application, LogLevel.Warning);

            Application.Current.Shutdown(0);
        }
        #endregion

        #region Installation
        private void InstallModpackButton_Click(object sender, RoutedEventArgs e)
        {
            //toggle buttons and reset UI
            ResetUI();

            //POST MODPACK SHUTDOWN: check time and if it's passed the date, then show the shutdown message
            DateTime currentDate = DateTime.Now;
            //note hour needs to be 24 hour format
            DateTime shutdownDate = new DateTime(2022, 4, 20, 16, 20, 0);
            int result = DateTime.Compare(currentDate, shutdownDate);
            if (result > 0)
            {
                //POST MODPACK SHUTDOWN: if sender is null, then it's called from some automated script function. ignore it
                if (sender == null)
                    return;

                //POST MODPACK SHUTDOWN: display the end of life banner and exit
                EndOfLife eol = new EndOfLife(this.ModpackSettings);
                _ = eol.ShowDialog();
                return;
            }

            ToggleUIButtons(false);

            if (ModpackSettings.InformIfApplicationInDownloadsFolder)
            { 
                //inform user if application is in the downloads folder. it is not recommended
                string usersDownloadFolder = string.Empty;

                try
                {
                    usersDownloadFolder = FileUtils.GetSpecialFolderPath(KnownFolder.Downloads);
                }
                catch(Exception ex)
                {
                    Logging.Warning("Failed to get downloads folder via shell32, attempt manual");
                    Logging.Warning(ex.ToString());
                    usersDownloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                }

                if(!Directory.Exists(usersDownloadFolder))
                {
                    Logging.Error("Failed to detect user's downloads folder");
                }
                else
                {
                    string applicationStartUpPathNoDir = ApplicationConstants.ApplicationStartupPath.Substring(0, ApplicationConstants.ApplicationStartupPath.Length - 1);
                    if (usersDownloadFolder.Equals(applicationStartUpPathNoDir))
                    {
                        MessageBox.Show(Translations.GetTranslatedString("moveAppOutOfDownloads"));
                        ModpackSettings.InformIfApplicationInDownloadsFolder = false;
                    }
                }
                ModpackSettings.InformIfApplicationInDownloadsFolder = false;
            }

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
                        Logging.Debug("User canceled selecting folder, stop");
                        ToggleUIButtons(true);
                        return;
                    }
                }

                Logging.Debug("Ask which version of client to export for");
                ExportModeSelect exportModeSelect = new ExportModeSelect(this.ModpackSettings);
                if ((bool)exportModeSelect.ShowDialog())
                {
                    Logging.Debug("ExportModeSelect returned true, setting majorVersion to {0}, minorVersion to {1}",
                        exportModeSelect.SelectedVersionInfo.WoTOnlineFolderVersion, exportModeSelect.SelectedVersionInfo.WoTClientVersion);
                    WoTModpackOnlineFolderVersion = exportModeSelect.SelectedVersionInfo.WoTOnlineFolderVersion;
                    WoTClientVersion = exportModeSelect.SelectedVersionInfo.WoTClientVersion;
                    WoTDirectory = foldertoExportTo;
                }
                else
                {
                    Logging.Debug("ExportModeSelect returned false, stop");
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

                //parse WoT root directory
                Logging.Debug("Started looking for WoT root directory");
                string searchResult = string.Empty;

                //only run the code if the user wants to auto find the WoT directory (which is default)
                if(!ModpackSettings.ForceManuel)
                {
                    searchResult = RegistryUtils.AutoFindWoTDirectoryFirst();
                }

                if (string.IsNullOrEmpty(searchResult) || ModpackSettings.ForceManuel)
                {
                    Logging.WriteToLog("Auto detect failed or user requests manual", Logfiles.Application, LogLevel.Debug);

                    WoTClientSelection clientSelection = new WoTClientSelection(ModpackSettings);

                    if ((bool)clientSelection.ShowDialog())
                    {
                        searchResult = clientSelection.SelectedPath;
                        searchResult = searchResult.Replace(ApplicationConstants.WoT32bitFolderWithSlash, string.Empty).Replace(ApplicationConstants.WoT64bitFolderWithSlash, string.Empty);
                        Logging.Info(LogOptions.ClassName, "Selected WoT install: {0}", searchResult);
                    }
                    else
                    {
                        Logging.Info("User Canceled installation");
                        ToggleUIButtons(true);
                        return;
                    }
                }
                WoTDirectory = Path.GetDirectoryName(searchResult);
                Logging.Info("Wot root directory parsed as " + WoTDirectory);
                

                //check to make sure it is the root application, not the win32/64 versions
                if (searchResult.Contains(ApplicationConstants.WoT32bitFolderWithSlash) || searchResult.Contains(ApplicationConstants.WoT64bitFolderWithSlash))
                {
                    searchResult = searchResult.Replace(ApplicationConstants.WoT32bitFolderWithSlash, string.Empty).Replace(ApplicationConstants.WoT64bitFolderWithSlash, string.Empty);
                }

                //check to make sure a valid game path has been returned and the setting file exists in that directory
                if (string.IsNullOrEmpty(searchResult) || (!File.Exists(searchResult)))
                {
                    Logging.Error("Failed to detect WoT exe from path {0}", searchResult);
                    MessageBox.Show(Translations.GetTranslatedString("failedToFindWoTExe"));
                    ToggleUIButtons(true);
                    return;
                }

                WoTDirectory = Path.GetDirectoryName(searchResult);
                Logging.Info("Wot root directory parsed as " + WoTDirectory);

                string versionXml = Path.Combine(WoTDirectory, ApplicationConstants.WoTVersionXml);
                if (!File.Exists(versionXml))
                {
                    Logging.Error("Failed to find WoT version.xml file or the file does not exist! '{0}", versionXml);
                    MessageBox.Show(Translations.GetTranslatedString("failedToFindWoTVersionXml"));
                    ToggleUIButtons(true);
                    return;
                }

                //check to make sure the application is not in the same directory as the WoT install
                if (WoTDirectory.Equals(ApplicationConstants.ApplicationStartupPath))
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
                        OpenFileDialog FindTestDatabaseDialog = new OpenFileDialog()
                        {
                            AddExtension = true,
                            CheckFileExists = true,
                            CheckPathExists = true,
                            Multiselect = false,
                            Title = "Select root database 2.0 file"
                        };
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
                string versionTemp = XmlUtils.GetXmlStringFromXPath(versionXml, ApplicationConstants.WoTVersionXmlXpath);
                WoTClientVersion = versionTemp.Split('#')[0].Trim().Substring(2).Trim();
                Logging.Info("Detected client version: {0}", WoTClientVersion);

                //determine if current detected version of the game is supported
                //only if application distribution is not alpha and database distribution is not test
                //the warning will therefore also happen in beta, but not take effect
                if (databaseVersion != DatabaseVersions.Test)
                {
                    //extract supported_clients xml from the manager info file
                    string supportedClientsXML = FileUtils.GetStringFromZip(((App)Application.Current).ManagerInfoZipfile, ApplicationConstants.SupportedClients);
                    if (string.IsNullOrWhiteSpace(supportedClientsXML))
                    {
                        Logging.Info("Failed to parse supported_clients.xml from string from zipfile", Logfiles.Application, LogLevel.Exception);
                        InstallProgressTextBox.Text = string.Format("{0} {1}", Translations.GetTranslatedString("failedToParse"), ApplicationConstants.SupportedClients);
                        ToggleUIButtons(true);
                        return;
                    }

                    XmlDocument doc = XmlUtils.LoadXmlDocument(supportedClientsXML, XmlLoadType.FromString);
                    if(doc == null)
                    {
                        Logging.Error("Failed to parse supported_clients.xml into xml document");
                        InstallProgressTextBox.Text = string.Format("{0} {1}", Translations.GetTranslatedString("failedToParse"), ApplicationConstants.SupportedClients);
                        ToggleUIButtons(true);
                        return;
                    }

                    //copy inner text of each WoT version into a string array
                    XmlNodeList supportedVersionsXML = XmlUtils.GetXmlNodesFromXPath(doc, "//versions/version");
                    string[] supportedVersionsString = new string[supportedVersionsXML.Count];
                    for (int i = 0; i < supportedVersionsXML.Count; i++)
                    {
                        supportedVersionsString[i] = supportedVersionsXML[i].InnerText.Trim();
                        Logging.Debug("Supported client: {0}", supportedVersionsString[i]);

                        //see if this supported client version is the same as what was parsed to be the current client version
                        if (supportedVersionsString[i].Equals(WoTClientVersion))
                        {
                            //set the online folder
                            WoTModpackOnlineFolderVersion = supportedVersionsXML[i].Attributes["folder"].Value;
                            Logging.Debug("Set {0} to {1}", nameof(WoTModpackOnlineFolderVersion), WoTModpackOnlineFolderVersion);
                        }
                    }

                    //check to see if array of supported clients has the detected WoT client version
                    //if the version does not match, then we need to set the online folder download version
                    if (!supportedVersionsString.Contains(WoTClientVersion))
                    {
                        WoTModpackOnlineFolderVersion = supportedVersionsXML[supportedVersionsXML.Count - 1].Attributes["folder"].Value;

                        //if it's not alpha, show the warning messages
                        if (ApplicationConstants.ApplicationVersion != ApplicationVersions.Alpha)
                        {
#pragma warning disable CS0162
                            //log and inform the user
                            Logging.Warning("Current client version {0} does not exist in list: {1}", WoTClientVersion, string.Join(", ", supportedVersionsString));
                            MessageBox.Show(string.Format("{0}: {1}\n{2} {3}\n\n{4}:\n{5}",
                                Translations.GetTranslatedString("detectedClientVersion"),//0
                                WoTClientVersion,//1
                                Translations.GetTranslatedString("supportNotGuarnteed"),//2
                                ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Stable? Translations.GetTranslatedString("couldTryBeta") : string.Empty,//3
                                Translations.GetTranslatedString("supportedClientVersions"),//4
                                string.Join("\n", supportedVersionsString)),//5
                                Translations.GetTranslatedString("critical"));//header
#pragma warning restore CS0162
                        }
                    }

                    //if the user wants to, check if the database has actually changed
                    if (ModpackSettings.NotifyIfSameDatabase && ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Stable)
                    {
                        Logging.Info("NotifyIfSameDatabase is true and databaseDistroVersion is stable, checking if last installed database is the same as current");
                        //get the install log for last installed database version
                        string installedfilesLogPath = Path.Combine(WoTDirectory, "logs", Logging.InstallLogFilename);
                        if (File.Exists(installedfilesLogPath))
                        {
                            //use index 0 of array, index 18 of string array
                            string lastInstalledDatabaseVersion = File.ReadAllText(installedfilesLogPath).Split('\n')[0];
                            Logging.Debug("LastInstalledDatabaseVersion (pre trim): {0}", lastInstalledDatabaseVersion);
                            if(!string.IsNullOrWhiteSpace(lastInstalledDatabaseVersion) && lastInstalledDatabaseVersion.Length >=18)
                                lastInstalledDatabaseVersion = lastInstalledDatabaseVersion.Substring(18).Trim();
                            Logging.Debug("LastInstalledDatabaseVersion (post trim): {0}", lastInstalledDatabaseVersion);
                            if (DatabaseVersion.Equals(lastInstalledDatabaseVersion))
                            {
                                if (MessageBox.Show(Translations.GetTranslatedString("DatabaseVersionsSameBody"), Translations.GetTranslatedString("DatabaseVersionsSameHeader"), MessageBoxButton.YesNo) == MessageBoxResult.No)
                                {
                                    Logging.Info("User selected to not install");
                                    ToggleUIButtons(true);
                                    return;
                                }
                                else
                                    Logging.Info("User selected to install anyways");
                            }
                        }
                        else
                        {
                            Logging.Warning("InstalledRelhaxFiles.log does not exist, cannot notify if same database");
                        }
                    }
                    else if(ModpackSettings.NotifyIfSameDatabase)
                    {
                        Logging.Info("NotifyIfSameDatabase is selected but database distribution is {0}, skipping", ModpackSettings.DatabaseDistroVersion.ToString());
                    }
                }
            }

            //show the mod selection list
            modSelectionList = new PackageSelectionList(this.ModpackSettings, this.CommandLineSettings)
            {
                AutoInstallMode = (sender == null),
                WotClientVersion = this.WoTClientVersion,
                DatabaseVersion = this.DatabaseVersion,
                WoTDirectory = this.WoTDirectory
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
                if (e.IsAutoInstall && !e.IsSelectionOutOfDate)
                {
                    Logging.Info("Returning from an auto install check, selection is not out of date, so no need to install");
                    ToggleUIButtons(true);
                }
                else
                {
                    //if not stable db, update WoT online folder version macro from modInfoxml itself
                    if (databaseVersion != DatabaseVersions.Stable)
                    {
                        WoTModpackOnlineFolderVersion = e.WoTModpackOnlineFolderFromDB;
                    }

                    OnBeginInstallation(e.DatabaseManager, e.UserMods, e.IsAutoInstall);
                }
            }
            else
            {
                ToggleUIButtons(true);
            }

            //do a collection to free up memory
            //regardless if we start an install, we should set the selection list to null to free memory
            modSelectionList.OnSelectionListReturn -= ModSelectionList_OnSelectionListReturn;
            modSelectionList = null;
            GC.Collect();
        }

        private async void OnBeginInstallation(DatabaseManager databaseManager, List<SelectablePackage> UserMods, bool isAutoInstall)
        {
            //check if wot is running
            while (CommonUtils.IsProcessRunning(ApplicationConstants.WoTProcessName, WoTDirectory))
            {
                //create window to determine if cancel, wait, kill TODO
                AskCloseWoT askCloseWoT = new AskCloseWoT(this.ModpackSettings) { WoTDirectory = this.WoTDirectory };
                //a positive result means that we are going to retry the loop
                //it could mean the user hit retry (close true), or hit force close (succeeded, and try again anyways)
                askCloseWoT.ShowDialog();
                if (askCloseWoT.AskCloseWoTResult == AskCloseWoTResult.CancelInstallation)
                {
                    ToggleUIButtons(true);
                    return;
                }
                else if (askCloseWoT.AskCloseWoTResult == AskCloseWoTResult.ForceClosed)
                {
                    break;
                }
                Thread.Sleep(100);
            }

            //build macro hash for install
            MacroUtils.BuildFilepathMacroList(WoTClientVersion, WoTModpackOnlineFolderVersion, WoTDirectory);

            //start the timer
            Logging.Info("Starting an installation (timer starts now)");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            //run database list calculations
            databaseManager.CalculateInstallLists(false, ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test);
            List<DatabasePackage> packagesToInstall = databaseManager.PackagesToInstall;
            List<DatabasePackage> packagesToInstallWithZipfile = databaseManager.PackagesToInstallWithZipFile;
            List<DatabasePackage> packagesToDownload = databaseManager.PackagesToDownload;
            List<SelectablePackage> selectablePackagesToInstall = databaseManager.SelectablePackagesToInstall;
            List<DatabasePackage>[] orderedPackagesToInstall = databaseManager.PackagesToInstallByInstallGroup;

            //create list of user mods to install
            List<SelectablePackage> userModsToInstall = UserMods.Where(mod => mod.Checked).ToList();

            //we now have a list of enabled, checked and actual zip file mods that we are going to install based on install groups
            //log the time to process lists
            TimeSpan lastTime = stopwatch.Elapsed;
            Logging.Info(string.Format("Took {0} ms to process lists", stopwatch.ElapsedMilliseconds));

            //gather stats
            if (ModpackSettings.AllowStatisticDataGather)
            {
                //https://stackoverflow.com/questions/13781468/get-list-of-properties-from-list-of-objects
                List<string> packageNamesToUpload = packagesToInstall.Select(pack => pack.PackageName).ToList();

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
                        Logging.Error("An error occurred sending statistic data");
                        Logging.Error(ex.ToString());
                    }
                });
            }

            //and check if we need to actually install anything
            if (selectablePackagesToInstall.Count == 0 && userModsToInstall.Count == 0)
            {
                Logging.Info("No packages selected to install, return");
                ResetUI();
                ToggleUIButtons(true);
                return;
            }

            //first, if we have downloads to do, then start processing them
            if (packagesToDownload.Count > 0)
            {
                deferToDownloadReport = false;
                downloaderCancellationTokenSource = new CancellationTokenSource();
                downloadManager = new DownloadManager()
                {
                    CancellationToken = downloaderCancellationTokenSource.Token,
                    RetryCount = 3,
                    DownloadLocationBase = ApplicationConstants.RelhaxDownloadsFolderPath,
                    UrlBase = ApplicationConstants.DownloadMirrors[ModpackSettings.DownloadMirror].Replace("{onlineFolder}", WoTModpackOnlineFolderVersion)
                };

                //https://stackoverflow.com/questions/367523/how-to-ensure-an-event-is-only-subscribed-to-once
                CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;
                CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Download_Click;

                if (!ModpackSettings.InstallWhileDownloading)
                {
                    Logging.Info("Download while install = false and packages to download, processing downloads with await");

                    //toggle the cancel button to be available for the download process
                    CancelDownloadInstallButton.Visibility = Visibility.Visible;
                    CancelDownloadInstallButton.IsEnabled = true;

                    //create progress object and connect to downloadManager
                    Progress<RelhaxDownloadProgress> downloadProgress = new Progress<RelhaxDownloadProgress>();
                    downloadProgress.ProgressChanged += DownloadProgress_ProgressChanged;
                    downloadManager.Progress = downloadProgress;

                    try
                    {
                        await downloadManager.DownloadPackagesAsync(packagesToDownload);
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException)
                        {
                            Logging.Info("Download task was canceled, canceling installation");
                        }
                        else
                        {
                            Logging.Exception(ex.ToString());
                        }
                        ResetUI();
                        InstallProgressTextBox.Text = Translations.GetTranslatedString("canceled");
                        ToggleUIButtons(true);
                        return;
                    }
                    finally
                    {
                        downloadManager.Dispose();
                    }

                    //stop and end the timer
                    if (downloadDisplayTimer != null)
                    {
                        downloadDisplayTimer.Stop();
                        downloadDisplayTimer = null;
                    }

                    CancelDownloadInstallButton.IsEnabled = false;
                    CancelDownloadInstallButton.Visibility = Visibility.Hidden;

                    //connect the install and disconnect the download
                    CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;

                    Logging.Info("Download time took {0} msec", stopwatch.Elapsed.TotalMilliseconds - lastTime.TotalMilliseconds);
                    lastTime = stopwatch.Elapsed;
                }
                else
                {
                    Logging.Info("Download while install = true and packages to download, processing downloads without await");

                    //async does download and install at the same time, so subscribe to install too (download already subscribed)
                    CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Install_Click;

                    //create progress object and connect to downloadManager
                    Progress<RelhaxDownloadProgress> downloadProgress = new Progress<RelhaxDownloadProgress>();
                    downloadProgress.ProgressChanged += DownloadProgress_ProgressChanged_InstallWhileDownloading;
                    downloadManager.Progress = downloadProgress;

                    downloadManager.DownloadPackagesAsync(packagesToDownload).ContinueWith(taskk =>
                    {
                        if (taskk.IsFaulted)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ResetUI();
                                if (taskk.Exception.InnerException is OperationCanceledException)
                                {
                                    Logging.Info("Download task was canceled, canceling installation");
                                    InstallProgressTextBox.Text = Translations.GetTranslatedString("canceled");
                                }
                                else
                                {
                                    Logging.Exception(taskk.Exception.InnerException.ToString());
                                    InstallProgressTextBox.Text = Translations.GetTranslatedString("error");
                                }
                                ToggleUIButtons(true);

                                //set taskbar progress state back to normal
                                taskbarState = TaskbarProgressBarState.NoProgress;
                                taskbarInstance.SetProgressState(taskbarState);
                            });
                            return;
                        }
                    });
                }
            }
            else
                Logging.Info("No packages to download, continue");

            //now let's start the install procedures
            //like if we need to make the advanced install window
            //but null it at all times
            if (AdvancedProgressWindow != null)
                AdvancedProgressWindow = null;
            if (ModpackSettings.AdvancedInstalProgress)
            {
                Logging.Debug("advancedInstallProgress is true, making window and populating with reporters");
                AdvancedProgressWindow = new AdvancedProgress(this.ModpackSettings)
                {
                    ShouldUserInstallBeCalled = userModsToInstall.Count > 0
                };
                //build the number of InstallTaskReporter objects based on what we are doing
                //if we are making a backup of the mods then make a reporter for it
                if (ModpackSettings.BackupModFolder)
                {
                    Logging.Debug("adding backupModFolder reporter");
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter(nameof(AdvancedProgressWindow.BackupModsReporter))
                    {
                        IsSubProgressActive = true,
                        TaskTitle = Translations.GetTranslatedString("AdvancedInstallBackupMods"),
                        ReportState = TaskReportState.Inactive,
                        LoadedAfterApply = false
                    };
                    AdvancedProgressWindow.PreInstallPanel.Children.Add(reporter);
                    AdvancedProgressWindow.BackupModsReporter = reporter;
                }

                //if we are backing up data, clearing cache, or clearing logs, then make one to hold them all
                if (ModpackSettings.SaveUserData || ModpackSettings.ClearCache || ModpackSettings.DeleteLogs)
                {
                    Logging.Debug("adding userData/clearCache/deleteLogs reporter: SaveUserData={0}, ClearCache={1}, DeleteLogs={2}",
                        ModpackSettings.SaveUserData, ModpackSettings.ClearCache, ModpackSettings.DeleteLogs);
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter(nameof(AdvancedProgressWindow.BackupDataClearCacheClearLogsReporter))
                    {
                        IsSubProgressActive = false,
                        ReportState = TaskReportState.Inactive,
                        LoadedAfterApply = false
                    };
                    AdvancedProgressWindow.PreInstallPanel.Children.Add(reporter);
                    AdvancedProgressWindow.BackupDataClearCacheClearLogsReporter = reporter;
                }

                //same for cleaning mods
                if (ModpackSettings.CleanInstallation)
                {
                    Logging.Debug("adding CleanInstallation reporter");
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter(nameof(AdvancedProgressWindow.CleanModsReporter))
                    {
                        IsSubProgressActive = false,
                        TaskTitle = Translations.GetTranslatedString("AdvancedInstallClearMods"),
                        ReportState = TaskReportState.Inactive,
                        LoadedAfterApply = false
                    };
                    AdvancedProgressWindow.PreInstallPanel.Children.Add(reporter);
                    AdvancedProgressWindow.CleanModsReporter = reporter;
                }

                //extraction is done based on the number of threads
                int numThreads = ModpackSettings.MulticoreExtraction ? ApplicationConstants.NumLogicalProcesors : 1;
                AdvancedProgressWindow.ExtractionModsReporters = new RelhaxInstallTaskReporter[numThreads];
                Logging.Debug("adding {0} reporters (MultiCoreExtraction={1}", numThreads, ModpackSettings.MulticoreExtraction);
                for (int i = 0; i < numThreads; i++)
                {
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter(nameof(AdvancedProgressWindow.ExtractionModsReporters) + i)
                    {
                        IsSubProgressActive = true,
                        TaskTitle = string.Format("{0} {1}", Translations.GetTranslatedString("AdvancedInstallInstallMods"), (i + 1).ToString()),
                        ReportState = TaskReportState.Inactive,
                        LoadedAfterApply = false
                    };
                    AdvancedProgressWindow.ExtractionPanel.Children.Add(reporter);
                    AdvancedProgressWindow.ExtractionModsReporters[i] = reporter;
                }

                //same idea for user mods
                if (userModsToInstall.Count > 0)
                {
                    Logging.Debug("adding userMods reporter");
                    RelhaxInstallTaskReporter reporter = new RelhaxInstallTaskReporter(nameof(AdvancedProgressWindow.ExtractionUserModsReporter))
                    {
                        IsSubProgressActive = true,
                        TaskTitle = Translations.GetTranslatedString("AdvancedInstallUserInstallMods"),
                        ReportState = TaskReportState.Inactive,
                        LoadedAfterApply = false
                    };
                    AdvancedProgressWindow.ExtractionPanel.Children.Add(reporter);
                    AdvancedProgressWindow.ExtractionUserModsReporter = reporter;
                }

                //all the post-install options, you don't know until the extraction finishes and if the UI reports events on it

                AdvancedProgressWindow.Show();
            }
            else
                Logging.Debug("AdvancedInstallProgress is false");

            //make sure each trigger list for each package is unique
            foreach (DatabasePackage package in packagesToInstallWithZipfile)
            {
                //for debug, get the list of duplicates
                //https://stackoverflow.com/questions/3811464/how-to-get-duplicate-items-from-a-list-using-linq
                List<string> duplicates = package.TriggersList.GroupBy(trigger => trigger).Where(trig => trig.Count() > 1).Select(trig => trig.Key).ToList();
                if (duplicates.Count > 0)
                {
                    //first make it distinct
                    Logging.Debug("Duplicate triggers found in package {0}:{1}", package.PackageName, string.Join(",", duplicates));
                }
            }

            //create the cancellation token source
            installerCancellationTokenSource = new CancellationTokenSource();

            Logging.Debug("UserMods install count: {0}", userModsToInstall.Count);

            //if user mods are being installed, then disable triggers
            disableTriggersForInstall = ModpackSettings.DisableTriggers;
            if (userModsToInstall.Count > 0 && !ModpackSettings.DisableTriggers)
            {
                Logging.Info("DisableTriggers is false and user has mods to install. Disabling triggers");
                disableTriggersForInstall = true;
            }

            Logging.Debug("Creating install engine, cancel options and progress reporting");
            //and create and link the install engine
            installEngine = new InstallEngine(ModpackSettings, CommandLineSettings)
            {
                DatabaseManager = databaseManager,
                UserPackagesToInstall = userModsToInstall,
                CancellationToken = installerCancellationTokenSource.Token,
                DisableTriggersForInstall = disableTriggersForInstall,
                DatabaseVersion = this.DatabaseVersion,
                WoTDirectory = this.WoTDirectory,
                WoTClientVersion = this.WoTClientVersion
            };

            if (ModpackSettings.InstallWhileDownloading)
                installEngine.DownloadManager = downloadManager;

            //setup the cancel button for installs
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Install_Click;
            CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Install_Click;
            CancelDownloadInstallButton.Visibility = Visibility.Visible;
            CancelDownloadInstallButton.IsEnabled = true;

            //create progress object
            Progress<RelhaxInstallerProgress> progress = new Progress<RelhaxInstallerProgress>();
            progress.ProgressChanged += OnInstallProgressChanged;

            //run install
            Logging.Debug("Running installation from MainWindow");
            RelhaxInstallFinishedEventArgs results = await installEngine.RunInstallationAsync(progress);
            Logging.Debug("Installation has finished, returned to MainWindow");
            installEngine.Dispose();
            installEngine = null;

            //if install while downloading, it's now safe to dispose of the download manager
            if (ModpackSettings.InstallWhileDownloading && packagesToDownload.Count > 0)
                downloadManager.Dispose();

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
            if (results.ExitCode == InstallerExitCodes.Success)
            {
                taskbarInstance.SetProgressValue(100, 100);
                if(ModpackSettings.VerboseLogging)
                    DisplayAndLogInstallErrors(results, false);

                if (!isAutoInstall)
                {
                    if (ModpackSettings.ShowInstallCompleteWindow)
                    {
                        InstallFinished installFinished = new InstallFinished(this.ModpackSettings) { WoTDirectory = this.WoTDirectory };
                        installFinished.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show(Translations.GetTranslatedString("installationFinished"));
                    }
                }

                InstallProgressTextBox.Text = string.Empty;
                ToggleUIButtons(true);
            }
            else if (installerCancellationTokenSource.IsCancellationRequested)
            {
                Logging.Info("Install cancel success");
                ToggleUIButtons(true);
                InstallProgressTextBox.Text = Translations.GetTranslatedString("canceled");
            }
            else
            {
                taskbarState = TaskbarProgressBarState.Error;
                taskbarInstance.SetProgressState(taskbarState);
                DisplayAndLogInstallErrors(results, true);
                ToggleUIButtons(true);
            }

            //Run task to get backup text file size if a backup was done
            if(ModpackSettings.BackupModFolder)
                GetBackupFilesizesAsync();

            //set taskbar progress state back to normal
            taskbarState = TaskbarProgressBarState.NoProgress;
            taskbarInstance.SetProgressState(taskbarState);
        }

        private void DisplayAndLogInstallErrors(RelhaxInstallFinishedEventArgs results, bool addResultsExitCode)
        {
            if (!results.InstallFailedSteps.Contains(results.ExitCode) && results.ExitCode != InstallerExitCodes.Success)
                results.InstallFailedSteps.Add(results.ExitCode);
            if (results.InstallFailedSteps.Count > 0)
            {
                StringBuilder errorBuilder = new StringBuilder();
                errorBuilder.AppendFormat("{0}{1}", Translations.GetTranslatedString("installFailed") + ":", Environment.NewLine);
                if (!results.InstallFailedSteps.Contains(results.ExitCode) && addResultsExitCode)
                    results.InstallFailedSteps.Add(results.ExitCode);
                errorBuilder.Append(string.Join(Environment.NewLine, results.InstallFailedSteps));

                Logging.Exception("The installer failed in the following steps: {0}", string.Join(",", results.InstallFailedSteps));

                MessageBox.Show(errorBuilder.ToString());
            }
        }

        private void DownloadProgress_ProgressChanged_InstallWhileDownloading(object sender, RelhaxDownloadProgress e)
        {
            if (!deferToDownloadReport) return;

            if (ModpackSettings.AdvancedInstalProgress && AdvancedProgressWindow != null)
            {
                AdvancedProgressWindow.OnReportDownload(e);
            }

            //child is bytes downloaded
            ChildProgressBar.Maximum = e.ChildTotal;
            ChildProgressBar.Minimum = 0;
            ChildProgressBar.Value = e.ChildCurrent;

            //break it up into lines cause it's hard to read
            //"downloading package_name"
            string line1 = string.Format("{0} {1}", Translations.GetTranslatedString("Downloading"), e.DatabasePackage.PackageName);

            //"zip_file_name"
            string line2 = e.DatabasePackage.ZipFile;

            //https://stackoverflow.com/questions/9869346/double-string-format
            //"2MB of 8MB"
            string line3 = string.Format("{0} {1} {2}", FileUtils.SizeSuffix((ulong)e.ChildCurrent, 1, true), Translations.GetTranslatedString("of"), FileUtils.SizeSuffix((ulong)e.ChildTotal, 1, true));

            //also report to the download message process
            InstallProgressTextBox.Text = string.Format("{0}\n{1}\n{2}", line1, line2, line3);
        }

        private void DownloadProgress_ProgressChanged(object sender, RelhaxDownloadProgress e)
        {
            //if current is 0 then use it as an initial block
            switch (e.DownloadProgressState)
            {
                case DownloadProgressState.OpenStreams:
                    //init elapsed timer
                    if (downloadTimer == null)
                    {
                        downloadTimer = new Stopwatch();
                    }
                    downloadTimer.Restart();

                    //init update timer
                    if (downloadDisplayTimer == null)
                    {
                        downloadDisplayTimer = new DispatcherTimer()
                        {
                            Interval = TimeSpan.FromMilliseconds(1000),
                            IsEnabled = false
                        };
                        downloadDisplayTimer.Tick += DownloadDisplayTimer_Elapsed;
                    }
                    downloadDisplayTimer.Stop();
                    downloadDisplayTimer.Start();

                    //init rates and history
                    lastBytesDownloaded = 0;
                    downloadRateDisplay = 0;
                    break;
                case DownloadProgressState.None:
                    return;
            }

            totalBytesToDownload = e.ChildTotal;
            currentBytesDownloaded = e.ChildCurrent;

            //child is bytes downloaded
            ChildProgressBar.Maximum = e.ChildTotal;
            ChildProgressBar.Minimum = 0;
            ChildProgressBar.Value = e.ChildCurrent;

            //parent is packages downloaded
            ParentProgressBar.Maximum = e.ParrentTotal;
            ParentProgressBar.Minimum = 0;
            ParentProgressBar.Value = e.ParrentCurrent;

            //break it up into lines cause it's hard to read
            //"downloading 2 of 4"
            string line1 = string.Format("{0} {1} {2} {3}",
                Translations.GetTranslatedString("Downloading"), ParentProgressBar.Value, Translations.GetTranslatedString("of"), ParentProgressBar.Maximum);

            //"zip_file_name"
            string line2 = e.DatabasePackage.ZipFile;

            //https://stackoverflow.com/questions/9869346/double-string-format
            //"2MB of 8MB at 1 MB/S"
            string line3 = string.Format("{0} {1} {2} {3} {4}/s",
                FileUtils.SizeSuffix((ulong)e.ChildCurrent, 1, true), Translations.GetTranslatedString("of"), FileUtils.SizeSuffix((ulong)e.ChildTotal, 1, true),
                Translations.GetTranslatedString("at"), FileUtils.SizeSuffix((ulong)downloadRateDisplay, 1, true, true));

            //"4 seconds"
            //https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings
            TimeSpan remain = TimeSpan.FromMilliseconds(remainingMilliseconds);
            string line4 = string.Format("{0} {1} {2} {3}", remain.ToString(@"mm"), Translations.GetTranslatedString("minutes"), remain.ToString(@"ss"),
                Translations.GetTranslatedString("seconds"));

            //also report to the download message process
            InstallProgressTextBox.Text = string.Format("{0}\n{1}\n{2}\n{3}", line1, line2, line3, line4);
        }

        private void DownloadDisplayTimer_Elapsed(object sender, EventArgs e)
        {
            //update download rate display values
            downloadRateDisplay = currentBytesDownloaded - lastBytesDownloaded;

            //update download rate ETA values
            //bytes remaining
            long bytesRemainToDownload = totalBytesToDownload - currentBytesDownloaded;

            //overall download rate bytes/msec
            double downloadRateOverall = 0;
            if (downloadTimer.Elapsed.TotalMilliseconds > 0)
                downloadRateOverall = currentBytesDownloaded / downloadTimer.Elapsed.TotalMilliseconds;

            //remaining time msec
            if ((long)downloadRateOverall > 0)
                remainingMilliseconds = bytesRemainToDownload / (long)downloadRateOverall;
            else
                remainingMilliseconds = 0;

            //set current to previous
            lastBytesDownloaded = currentBytesDownloaded;
        }

        private void OnInstallProgressChanged(object sender, RelhaxInstallerProgress e)
        {
            //set taskbar progress
            if(taskbarInstance != null)
            {
                if (taskbarState != TaskbarProgressBarState.Normal)
                {
                    taskbarState = TaskbarProgressBarState.Normal;
                    taskbarInstance.SetProgressState(taskbarState);
                }
                if(taskbarValue != e.TotalCurrent)
                {
                    taskbarValue = e.TotalCurrent;
                    taskbarInstance.SetProgressValue(taskbarValue, e.TotalTotal);
                }
            }

            if (ModpackSettings.AdvancedInstalProgress)
            {
                if (AdvancedProgressWindow == null)
                {
                    throw new BadMemeException("Not so advanced now is it");
                }
                AdvancedProgressWindow.OnReportAdvancedProgress(e);
                if (ModpackSettings.InstallWhileDownloading && e.AllRemainingThreadsWaitingOnDownloads)
                {
                    deferToDownloadReport = true;
                }
                else
                {
                    deferToDownloadReport = false;
                }
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
                    case InstallerExitCodes.BackupModsError:
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
                    case InstallerExitCodes.BackupDataError:
                        //filename is name of file in package to backup
                        //parrentCurrentProgress is name of package
                        line1 = Translations.GetTranslatedString("installBackupData");
                        line2 = e.Filename;
                        line3 = e.ParrentCurrentProgress;
                        break;
                    case InstallerExitCodes.ClearCacheError:
                        line1 = Translations.GetTranslatedString("installClearCache");
                        break;
                    case InstallerExitCodes.ClearLogsError:
                        line1 = Translations.GetTranslatedString("installClearLogs");
                        line2 = Path.GetFileName(e.Filename);
                        break;
                    case InstallerExitCodes.CleanModsError:
                        line1 = Translations.GetTranslatedString("installCleanMods");
                        line2 = e.Filename;
                        break;
                    case InstallerExitCodes.ExtractionError:
                        if(ModpackSettings.MulticoreExtraction)
                        {
                            ChildProgressBar.Maximum = e.TotalInstallGroups;
                            ChildProgressBar.Value = e.InstallGroup;
                            line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installExtractingMods"), ((e.ParrentCurrent) > 0 ? e.ParrentCurrent : 1).ToString(),
                                Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                            line2 = string.Format("{0}: {1} {2} {3} {4} {5}", Translations.GetTranslatedString("installExtractingCompletedThreads"), e.CompletedThreads.ToString(),
                                Translations.GetTranslatedString("of"), e.TotalThreadsOfThisGroup.ToString(), Translations.GetTranslatedString("installExtractingOfGroup"), e.InstallGroup.ToString());
                            line3 = Path.GetFileName(e.Filename);
                            if (ModpackSettings.InstallWhileDownloading && e.AllRemainingThreadsWaitingOnDownloads)
                            {
                                deferToDownloadReport = true;
                            }
                            else
                            {
                                deferToDownloadReport = false;
                                line4 = e.EntryFilename;
                            }
                        }
                        else
                        {
                            ChildProgressBar.Maximum = e.BytesTotal;
                            ChildProgressBar.Value = e.BytesProcessed;
                            line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installExtractingMods"), ((e.ParrentCurrent) > 0 ? e.ParrentCurrent : 1).ToString(),
                                Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                            line2 = Path.GetFileName(e.Filename);
                            if (ModpackSettings.InstallWhileDownloading && e.AllThreadsWaitingOnDownloads)
                            {
                                deferToDownloadReport = true;
                            }
                            else
                            {
                                deferToDownloadReport = false;
                                line3 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installZipFileEntry"), ((e.EntriesProcessed) > 0 ? e.EntriesProcessed : 1).ToString(),
                                Translations.GetTranslatedString("of"), e.EntriesTotal.ToString());
                                line4 = e.EntryFilename;
                            }
                        }
                        break;
                    case InstallerExitCodes.UserExtractionError:
                        ChildProgressBar.Maximum = e.BytesTotal;
                        ChildProgressBar.Value = e.BytesProcessed;
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("extractingUserMod"), ((e.ParrentCurrent) > 0 ? e.ParrentCurrent : 1).ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = Path.GetFileName(e.Filename);
                        line3 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installZipFileEntry"), ((e.EntriesProcessed) > 0 ? e.EntriesProcessed : 1).ToString(),
                            Translations.GetTranslatedString("of"), e.EntriesTotal.ToString());
                        line4 = e.EntryFilename;
                        break;
                    case InstallerExitCodes.RestoreUserdataError:
                        //filename is name of file in package to backup
                        //parrentCurrentProgress is name of package
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installRestoreUserdata"), ((e.ParrentCurrent) > 0 ? e.ParrentCurrent : 1).ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = e.Filename;
                        line3 = e.ParrentCurrentProgress;
                        break;
                    case InstallerExitCodes.XmlUnpackError:
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installXmlUnpack"), ((e.ParrentCurrent) > 0 ? e.ParrentCurrent : 1).ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = e.Filename;
                        break;
                    case InstallerExitCodes.PatchError:
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installPatchFiles"), ((e.ParrentCurrent) > 0 ? e.ParrentCurrent : 1).ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = e.Filename;
                        break;
                    case InstallerExitCodes.ShortcutsError:
                        line1 = Translations.GetTranslatedString("installShortcuts");
                        line2 = e.Filename;
                        break;
                    case InstallerExitCodes.ContourIconAtlasError:
                        line1 = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("installContourIconAtlas"), ((e.ParrentCurrent) > 0 ? e.ParrentCurrent : 1).ToString(),
                            Translations.GetTranslatedString("of"), e.ParrentTotal.ToString());
                        line2 = string.Format("{0} {1} {2} {3}", e.ChildCurrent.ToString(), Translations.GetTranslatedString("of"), e.ChildTotal.ToString(),
                            Translations.GetTranslatedString("stepsComplete"));
                        break;
                    case InstallerExitCodes.FontInstallError:
                        line1 = Translations.GetTranslatedString("installFonts");
                        break;
                    case InstallerExitCodes.CleanupError:
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
        #endregion

        #region Uninstall
        private async void UninstallModpackButton_Click(object sender, RoutedEventArgs e)
        {
            //toggle the buttons and reset the UI
            ToggleUIButtons(false);
            ResetUI();

            //parse WoT root directory
            Logging.WriteToLog("Started looking for WoT root directory", Logfiles.Application, LogLevel.Debug);
            string autoSearchResult = string.Empty;

            //only run the code if the user wants to auto find the WoT directory (which is default)
            if (!ModpackSettings.ForceManuel)
            {
                autoSearchResult = RegistryUtils.AutoFindWoTDirectoryFirst();
            }

            if (string.IsNullOrEmpty(autoSearchResult) || ModpackSettings.ForceManuel)
            {
                Logging.WriteToLog("Auto detect failed or user requests manual", Logfiles.Application, LogLevel.Debug);

                WoTClientSelection clientSelection = new WoTClientSelection(ModpackSettings);

                if ((bool)clientSelection.ShowDialog())
                {
                    autoSearchResult = clientSelection.SelectedPath;
                    autoSearchResult = autoSearchResult.Replace(ApplicationConstants.WoT32bitFolderWithSlash, string.Empty).Replace(ApplicationConstants.WoT64bitFolderWithSlash, string.Empty);
                    Logging.Info(LogOptions.ClassName, "Selected WoT install: {0}", autoSearchResult);
                }
                else
                {
                    Logging.Info("User Canceled installation");
                    ToggleUIButtons(true);
                    return;
                }
            }
            WoTDirectory = Path.GetDirectoryName(autoSearchResult);
            Logging.Info("Wot root directory parsed as " + WoTDirectory);

            //get the version of tanks in the format of the res_mods version folder i.e. 0.9.17.0.3
            string versionTemp = XmlUtils.GetXmlStringFromXPath(Path.Combine(WoTDirectory, "version.xml"), "//version.xml/version");
            WoTClientVersion = versionTemp.Split('#')[0].Trim().Substring(2);

            //verify the uninstall
            string uninstallModeTranslated = ModpackSettings.UninstallMode == UninstallModes.Quick ?
                Translations.GetTranslatedString("UninstallQuickText") : Translations.GetTranslatedString("UninstallDefaultText");
            string uninstallConfirmMessage = string.Format(Translations.GetTranslatedString("verifyUninstallVersionAndLocation"), WoTDirectory, uninstallModeTranslated);
            if (MessageBox.Show(uninstallConfirmMessage, Translations.GetTranslatedString("confirmUninstallHeader"), MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                ToggleUIButtons(true);
                return;
            }

            //check if wot is running
            while (CommonUtils.IsProcessRunning(ApplicationConstants.WoTProcessName, WoTDirectory))
            {
                //create window to determine if cancel, wait, kill TODO
                AskCloseWoT askCloseWoT = new AskCloseWoT(this.ModpackSettings) { WoTDirectory = this.WoTDirectory };
                //a positive result means that we are going to retry the loop
                //it could mean the user hit retry (close true), or hit force close (succeeded, and try again anyways)
                askCloseWoT.ShowDialog();
                if (askCloseWoT.AskCloseWoTResult == AskCloseWoTResult.CancelInstallation)
                {
                    ToggleUIButtons(true);
                    return;
                }
                else if (askCloseWoT.AskCloseWoTResult == AskCloseWoTResult.ForceClosed)
                {
                    break;
                }
                Thread.Sleep(100);
            }

            //setup the cancel button
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Install_Click;
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;
            CancelDownloadInstallButton.Click += CancelDownloadInstallButton_Install_Click;
            CancelDownloadInstallButton.Visibility = Visibility.Visible;
            CancelDownloadInstallButton.IsEnabled = true;

            //create progress object
            Progress<RelhaxInstallerProgress> progress = new Progress<RelhaxInstallerProgress>();
            progress.ProgressChanged += UninstallProgressChanged;

            //create token source
            installerCancellationTokenSource = new CancellationTokenSource();

            //create and run uninstall engine
            installEngine = new InstallEngine(this.ModpackSettings, null)
            {
                CancellationToken = installerCancellationTokenSource.Token,
                ModpackSettings = this.ModpackSettings,
                DatabaseVersion = null, //not needed for uninstall
                WoTDirectory = this.WoTDirectory,
                WoTClientVersion = this.WoTClientVersion
            };
            RelhaxInstallFinishedEventArgs results = await installEngine.RunUninstallationAsync(progress);
            installEngine.Dispose();
            installEngine = null;

            //close and hide the install progress button
            CancelDownloadInstallButton.IsEnabled = false;
            CancelDownloadInstallButton.Visibility = Visibility.Hidden;
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Install_Click;
            CancelDownloadInstallButton.Click -= CancelDownloadInstallButton_Download_Click;

            //report results
            ChildProgressBar.Value = ChildProgressBar.Maximum;
            if (results.ExitCode == InstallerExitCodes.Success)
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
            if (e.UninstallStatus == UninstallerExitCodes.GettingFilelistError)
            {
                InstallProgressTextBox.Text = Translations.GetTranslatedString("scanningModsFolders");
            }
            else if (e.UninstallStatus == UninstallerExitCodes.UninstallError)
            {
                InstallProgressTextBox.Text = string.Format("{0} {1} {2} {3}{4}{5}", Translations.GetTranslatedString("uninstallingFile"), e.ChildCurrent,
                    Translations.GetTranslatedString("of"), e.ChildTotal, Environment.NewLine, e.Filename);
            }
        }
        #endregion

        #region Update code
        private async Task CheckForDatabaseUpdatesAsync(bool refreshModInfo)
        {
            Logging.Info("Checking for database updates in CheckForDatabaseUpdates()");

            //if we are getting a new ModInfo then do that
            XmlDocument doc;
            if (refreshModInfo)
            {
                doc = await CommonUtils.GetManagerInfoDocumentAsync(true);
            }
            else
            {
                if (((App)Application.Current).ManagerInfoZipfile == null)
                {
                    Logging.Debug(LogOptions.MethodName, "[CheckForDatabaseUpdatesAsync]: RefreshModInfo is false, but Settings.ModInfoZipfile is null. Getting latest modInfo");
                    ((App)Application.Current).ManagerInfoZipfile = await CommonUtils.GetManagerInfoZipfileAsync(false);
                }
                //only get if from the downloaded version
                //get the version info string
                string xmlString = FileUtils.GetStringFromZip(((App)Application.Current).ManagerInfoZipfile, ApplicationConstants.ManagerVersion);
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
            Logging.Info(string.Format("Comparing database versions, old={0}, new={1}", DatabaseVersion, databaseNewVersion));

            if (string.IsNullOrWhiteSpace(DatabaseVersion))
            {
                //auto apply and don't announce. this usually happens when the application is loading for first time
                Logging.Info("Settings.DatabaseVersion is empty, setting init value");
                DatabaseVersion = databaseNewVersion;
                DatabaseVersionLabel.Text = Translations.GetTranslatedString("databaseVersion") + " " + DatabaseVersion;
            }
            else if (!DatabaseVersion.Equals(databaseNewVersion))
            {
                //this happens when user clicks to manually check for updates or from the auto install feature
                Logging.Info("new version of database applied");
                DatabaseVersion = databaseNewVersion;
                DatabaseVersionLabel.Text = Translations.GetTranslatedString("databaseVersion") + " " + DatabaseVersion;
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
                if (ApplicationConstants.ApplicationVersion != ApplicationVersions.Alpha)
                {
#pragma warning disable CS0162
                    MessageBox.Show(Translations.GetTranslatedString("skipUpdateWarning"));
#pragma warning restore CS0162
                }
                Logging.Warning("Skipping update check from command-line option SkipUpdate");
                return true;
            }

            //if the request distribution version is alpha, correct it to stable
            if (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Alpha)
            {
                Logging.Warning("Alpha is an invalid option for ModpackSettings.ApplicationDistroVersion");
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Beta;
            }

            //4 possibilities:
            //stable->stable (update check)
            //stable->beta (auto out of date)
            //beta->stable (auto out of date)
            //beta->beta (update check)
            bool outOfDate = false;

            //check if old settings file exists and if it was the beta channel
            if (File.Exists(ModpackSettings.OldSettingsFilename))
            {
                Logging.Debug("Old settings file exists, load it and see if was beta distro");
                string betaDistro = XmlUtils.GetXmlStringFromXPath(ModpackSettings.OldSettingsFilename, @"//settings/BetaApplication");
                if (bool.TryParse(betaDistro, out bool result) && result)
                {
                    Logging.Debug("Application was beta, setting distro to beta");
                    ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Beta;
                }
                else
                {
                    Logging.Debug("Application was not beta: '{0}'", betaDistro);
                    ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Stable;
                }
            }

            //check if the new/regular settings file exists, and if it doesn't ,then set the application distro version to what the application was compiled as
            if (!File.Exists(ModpackSettings.SettingsFilename))
            {
                Logging.Info("{0} settings file does not exist. This is a first time load, set settings application distro version to application compile ({1})", ModpackSettings.SettingsFilename, ApplicationConstants.ApplicationVersion.ToString());
#pragma warning disable CS0162 // Unreachable code detected
                switch (ApplicationConstants.ApplicationVersion)
                {
                    case ApplicationVersions.Alpha:
                    case ApplicationVersions.Beta:
                        ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Beta;
                        break;
                    case ApplicationVersions.Stable:
                        ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Stable;
                        break;
                }
#pragma warning restore CS0162 // Unreachable code detected
            }

            //make a copy of the current application version and set it to stable if (fake) alpha
            ApplicationVersions version = ApplicationConstants.ApplicationVersion;

            //check if the documentation xml file is there, if so then we should say in alpha
            if (version == ApplicationVersions.Alpha && !File.Exists("RelhaxModpack.xml"))
            {
                Logging.Warning("You are running an alpha build of Relhax Modpack. Unless you were asked to run an alpha build for testing, or know what you're doing, you shouldn't be running an alpha build!");
                version = ApplicationVersions.Beta;
                //at least let's set this for now, might want to unset it later after testing. at this point if you're running an alpha build you're being moved to beta
                ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Beta;
            }
            else if (version == ApplicationVersions.Alpha && File.Exists("RelhaxModpack.xml"))
            {
                Logging.Debug("Application is alpha build, skipping update check");
                return true;
            }

            //declare these out here so the logger can access them
            string applicationBuildVersion = CommonUtils.GetApplicationVersion();

            //if current application build does not equal requested distribution channel
            //can assume out of date because switching distribution channels
            if (version != ModpackSettings.ApplicationDistroVersion)
            {
                outOfDate = true;
                Logging.Info("Current build is {0} ({1}), online build is NA (changing distribution version {1}->{2})",
                    applicationBuildVersion, version.ToString(), ModpackSettings.ApplicationDistroVersion.ToString());
            }
            else
            {
                outOfDate = !(await CommonUtils.IsManagerUptoDate(applicationBuildVersion, ModpackSettings.ApplicationDistroVersion));
            }

            if (!outOfDate)
            {
                Logging.Info("Application up to date");
                return true;
            }
            else if (outOfDate && ((App)Application.Current).CheckForUpdatesError)
            {
                Logging.Error("Application not up to date, it failed to check for updates.");
                //MODPACK POST-SHUTDOWN: if the domain is down or the site can't be reached, then continue anyways
                return true;
            }
            else
            {
                Logging.Info("Application not up to date");
                return false;
            }
        }

        private void OnUpdateDownloadProgresChange(object sender, DownloadProgressChangedEventArgs e)
        {
            //if it's not running, start it
            if (!downloadTimer.IsRunning)
                downloadTimer.Start();

            //set the update progress bar
            ChildProgressBar.Value = e.ProgressPercentage;
            float MBDownloaded = (float)e.BytesReceived / (float)FileUtils.BYTES_TO_MBYTES;
            float MBTotal = (float)e.TotalBytesToReceive / (float)FileUtils.BYTES_TO_MBYTES;
            MBDownloaded = (float)Math.Round(MBDownloaded, 2);
            MBTotal = (float)Math.Round(MBTotal, 2);
            string downloadMessage = string.Format("{0} {1}MB {2} {3}MB", Translations.GetTranslatedString("downloadingUpdate"),
                MBDownloaded, Translations.GetTranslatedString("of"), MBTotal);
            InstallProgressTextBox.Text = downloadMessage;
        }

        private async void OnUpdateDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //stop the timer
            downloadTimer.Reset();
            downloadTimer = null;

            if (e.Error != null)
            {
                Logging.WriteToLog("Failed to download application update\n" + e.Error.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
                MessageBox.Show(Translations.GetTranslatedString("cantDownloadNewVersion"));
                Environment.Exit(-1);
                return;
            }

            //try to extract the update
            Logging.Debug("Extracting update zip file");
            try
            {
                using (ZipFile zip = ZipFile.Read(ApplicationConstants.ApplicationUpdateFileNameZip))
                {
                    zip.ExtractAll(ApplicationConstants.ApplicationStartupPath, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch (ZipException zipex)
            {
                Logging.WriteToLog("Failed to extract update zip file\n" + zipex.ToString(), Logfiles.Application, LogLevel.ApplicationHalt);
                MessageBox.Show(Translations.GetTranslatedString("failedToExtractUpdateArchive"));
                Environment.Exit(-1);
                return;
            }

            //check that zip isn't null, and get it if it is
            Logging.Debug("Checking on Task GetManagerInfoZipfileAsync()");
            if (!DownloadManagerInfoZip.IsCompleted)
            {
                Logging.Debug("Task GetManagerInfoZipfileAsync() is not done, setting timeout for 10 seconds");
                if (DownloadManagerInfoZip.Wait(TimeSpan.FromSeconds(10)))
                {
                    Logging.Debug("Task GetManagerInfoZipfileAsync() finished successfully");
                }
                else
                {
                    Logging.Debug("Task GetManagerInfoZipfileAsync() failed, timeout. Try again...");
                }
            }
            else
            {
                ((App)Application.Current).ManagerInfoZipfile = DownloadManagerInfoZip.Result;
            }

            //and one final check
            if (((App)Application.Current).ManagerInfoZipfile == null)
            {
                Logging.Warning("Settings.((App)Application.Current).ManagerInfoZipfile is null, getting now");
                ((App)Application.Current).ManagerInfoZipfile = await CommonUtils.GetManagerInfoZipfileAsync(false);
                if (((App)Application.Current).ManagerInfoZipfile == null)
                {
                    MessageBox.Show(Translations.GetTranslatedString("failedToExtractUpdateArchive"));
                    Environment.Exit(-1);
                    return;
                }
            }

            //extract the batch script to update the application
            string batchScript = FileUtils.GetStringFromZip(((App)Application.Current).ManagerInfoZipfile, ApplicationConstants.RelicBatchUpdateScriptServer);
            Logging.Debug("Writing batch script to disk");
            File.WriteAllText(ApplicationConstants.RelicBatchUpdateScript, batchScript);

            //try to start the update script
            try
            {
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = Path.Combine(ApplicationConstants.ApplicationStartupPath, ApplicationConstants.RelicBatchUpdateScript),
                    Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray())
                };
                using (Process installUpdate = new Process { StartInfo = info })
                {
                    installUpdate.Start();
                }
            }
            catch (Exception e3)
            {
                Logging.WriteToLog("Failed to start " + ApplicationConstants.RelicBatchUpdateScript + "\n" + e3.ToString(),
                    Logfiles.Application, LogLevel.ApplicationHalt);
                MessageBox.Show(Translations.GetTranslatedString("cantStartNewApp"));
            }
            Environment.Exit(0);
            return;
        }
        #endregion

        #region One click and auto install code
        private void OneClickInstallCB_Click(object sender, RoutedEventArgs e)
        {
            string tmep = string.Empty;
            if (string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath) || !File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
            {
                tmep = ModpackSettings.AutoOneclickSelectionFilePath;
                Logging.Debug(LogOptions.MethodName, "AutoClickSelectionPath is null or doesn't exist, prompting user to change");
                LoadAutoSyncSelectionFile_Click(null, null);
            }

            if (string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath) || !File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
            {
                Logging.Debug(LogOptions.MethodName, "AutoClickSelectionPath is null or doesn't exist still, setting to false and reverting path");
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
                InitialDirectory = ApplicationConstants.RelhaxUserSelectionsFolderPath,
                Multiselect = false
            };

            if (!(bool)selectAutoSyncSelectionFileDialog.ShowDialog())
                return;

            AutoInstallOneClickInstallSelectionFilePath.TextChanged -= OnAutoInstallOneClickInstallSelectionFilePathTextChanged;
            AutoInstallOneClickInstallSelectionFilePath.Text = selectAutoSyncSelectionFileDialog.FileName;
            ModpackSettings.AutoOneclickSelectionFilePath = selectAutoSyncSelectionFileDialog.FileName;
            AutoInstallOneClickInstallSelectionFilePath.TextChanged += OnAutoInstallOneClickInstallSelectionFilePathTextChanged;
        }

        private void AutoInstallCB_Click(object sender, RoutedEventArgs e)
        {
            if (loading)
                return;

            //if it's turning off, then process that only
            if (!(bool)AutoInstallCB.IsChecked)
            {
                Logging.Debug(LogOptions.MethodName, "AutoInstall being turned off");
                ModpackSettings.AutoInstall = false;
                autoInstallPeriodicTimer.Stop();
                autoInstallPeriodicTimer = null;
                return;
            }

            //getting here means that auto install is being attempted to be turned on
            //the selection file must be set for this to work
            if (!CheckIfAutoInstallFileExists())
                return;

            //if the current database distro is beta AND the user now wants to turn on auto install, confirm with the user that we really want to do this
            //due to the potentially high install frequency
            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Beta)
            {
                Logging.Info(LogOptions.MethodName, "Database distro is beta, verify with user");
                if (autoInstallPeriodicTimer != null && autoInstallPeriodicTimer.IsEnabled)
                    autoInstallPeriodicTimer.Stop();

                if (MessageBox.Show(Translations.GetTranslatedString("autoInstallWithBetaDBConfirmBody"), Translations.GetTranslatedString("autoInstallWithBetaDBConfirmHeader"), MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    Logging.Debug(LogOptions.MethodName, "Database distro is beta, user declined, abort");
                    ModpackSettings.AutoInstall = false;
                    AutoInstallCB.Click -= AutoInstallCB_Click;
                    AutoInstallCB.IsChecked = false;
                    AutoInstallCB.Click += AutoInstallCB_Click;
                    autoInstallPeriodicTimer = null;
                    return;
                }
            }

            InitAutoInstallTimerAndConfigureInterval();
            if (databaseVersion == DatabaseVersions.Beta)
                InitAutoInstallTimerForBetaDbAsync();
            ConfigureAutoInstallTimerEvent();
            autoInstallPeriodicTimer.Start();

            //and finally set value into modpack settings
            ModpackSettings.AutoInstall = true;

            Logging.Info(LogOptions.MethodName, "Timer registered, listening for update check intervals");
        }

        private async Task InitAutoInstallTimerForBetaDbAsync()
        {
            //get the latest complete string set of the database and set both 'new' and 'old' versions for it
            //we don't need to do this for stable because it already stores the database version and running the periodic function will handle it for us
            //see CheckForAutoInstallUpdates() for more info -> uses CheckForDatabaseUpdatesAsync()
            Logging.Debug(LogOptions.MethodName, "AutoInstall is enabled and database distro = beta, need to get current beta database for comparison");
            oldBetaDB = await GetBetaDatabase1V1ForStringCompareAsync();
            newBetaDB = oldBetaDB;
        }

        private void ConfigureAutoInstallTimerEvent()
        {
            autoInstallPeriodicTimer.Tick -= AutoInstallTimer_ElapsedBeta;
            autoInstallPeriodicTimer.Tick -= AutoInstallTimer_Elapsed;
            switch (ModpackSettings.DatabaseDistroVersion)
            {
                case DatabaseVersions.Beta:
                    autoInstallPeriodicTimer.Tick += AutoInstallTimer_ElapsedBeta;
                    break;
                case DatabaseVersions.Stable:
                    autoInstallPeriodicTimer.Tick += AutoInstallTimer_Elapsed;
                    break;
            }
        }

        private void AutoInstallTimer_Elapsed(object sender, EventArgs e)
        {
            CheckForAutoInstallUpdates(DatabaseVersions.Stable);
        }

        private void AutoInstallTimer_ElapsedBeta(object sender, EventArgs e)
        {
            CheckForAutoInstallUpdates(DatabaseVersions.Beta);
        }

        private async void CheckForAutoInstallUpdates(DatabaseVersions version)
        {
            //don't run this if still loading
            if (loading)
            {
                Logging.Debug(LogOptions.MethodName, "Skipped check because loading MainWindow");
                return;
            }

            //use a class level boolean to prevent multiple checks if one is finishing while the dispatcher calls another
            if (timerActive)
            {
                Logging.Warning(LogOptions.MethodName, "Skipped check because timer is already active, is this intended?");
                return;
            }
            timerActive = true;

            //actual code here
            Logging.Debug(LogOptions.MethodName, "Timer has elapsed to check for database updates, version = {0}", version.ToString());
            bool updateAvailable = false;

            switch (version)
            {
                case DatabaseVersions.Beta:
                    //get the sum total of the database xml from all files
                    newBetaDB = await GetBetaDatabase1V1ForStringCompareAsync();

                    //if oldBetaDB is empty, then don't use that as the update check because it's in an init phase
                    if (string.IsNullOrEmpty(oldBetaDB))
                    {
                        Logging.Debug(LogOptions.MethodName, "OldBetaDB is null/empty (init), set them equal to force no update available");
                        oldBetaDB = newBetaDB;
                        break;
                    }

                    //check if database was updated
                    if (!newBetaDB.Equals(oldBetaDB))
                    {
                        oldBetaDB = newBetaDB;
                        updateAvailable = true;
                    }
                    break;
                case DatabaseVersions.Stable:
                    //reset check flag and get old db version
                    string oldDBVersion = DatabaseVersion;

                    //actually check for updates
                    await CheckForDatabaseUpdatesAsync(true);

                    //check if database was updated
                    if (!oldDBVersion.Equals(DatabaseVersion))
                    {
                        updateAvailable = true;
                    }
                    break;
            }

            if (updateAvailable)
            {
                //if modSelectionlist != null, then it's currently in use i.e. the user is selecting mods, so skip this
                if (modSelectionList != null)
                {
                    Logging.Info(LogOptions.MethodName, "Update found, but ModSelectionList != null, don't start an install");
                    return;
                }
                else
                {
                    Logging.Info(LogOptions.MethodName, "Update found, running installation");
                    InstallModpackButton_Click(null, null);
                }
            }
            else
                Logging.Debug(LogOptions.MethodName, "Update not found");

            timerActive = false;
        }

        private bool CheckIfAutoInstallFileExists()
        {
            bool exists = true;
            if (string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath) || !File.Exists(ModpackSettings.AutoOneclickSelectionFilePath))
            {
                Logging.Info(LogOptions.MethodName, "AutoClickSelectionPath is null or doesn't exist, abort");
                if (autoInstallPeriodicTimer != null && autoInstallPeriodicTimer.IsEnabled)
                    autoInstallPeriodicTimer.Stop();
                autoInstallPeriodicTimer = null;
                ModpackSettings.AutoInstall = false;
                AutoInstallCB.Click -= AutoInstallCB_Click;
                AutoInstallCB.IsChecked = false;
                AutoInstallCB.Click += AutoInstallCB_Click;
                MessageBox.Show(Translations.GetTranslatedString("autoOneclickSelectionFileNotExist"));
                exists = false;
            }
            return exists;
        }

        private void InitAutoInstallTimerAndConfigureInterval()
        {
            //stop the dispatcherTimer if running, and instance it if not already
            if (autoInstallPeriodicTimer != null && autoInstallPeriodicTimer.IsEnabled)
                autoInstallPeriodicTimer.Stop();
            autoInstallPeriodicTimer = null;
            autoInstallPeriodicTimer = new DispatcherTimer(DispatcherPriority.Normal);

            //check the time parsed value
            int timeToUse = CommonUtils.ParseInt(AutoSyncFrequencyTexbox.Text, 0);
            if (timeToUse <= 0)
            {
                Logging.Warning(LogOptions.MethodName, "Invalid time specified, must be above 0. using 1");
                timeToUse = 1;
                AutoSyncFrequencyTexbox.Text = timeToUse.ToString();
            }

            //parse the time into a timespan for the check timer
            Logging.Info(LogOptions.MethodName, "Registering auto install periodic timer");
            switch (AutoSyncFrequencyComboBox.SelectedIndex)
            {
                case 0://mins
                    autoInstallPeriodicTimer.Interval = TimeSpan.FromMinutes(timeToUse);
                    break;
                case 1://hours
                    autoInstallPeriodicTimer.Interval = TimeSpan.FromHours(timeToUse);
                    break;
                case 2://days
                    autoInstallPeriodicTimer.Interval = TimeSpan.FromDays(timeToUse);
                    break;
                default:
                    throw new BadMemeException("this should not happen");
            }
        }
        #endregion

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
            CloseApplication();
        }

        private async void OnMenuClickChekUpdates(object sender, EventArgs e)
        {
            Logging.Debug("check for database updates from menu click");
            string oldDBVersion = DatabaseVersion;

            //make and show progress indicator
            ProgressIndicator progressIndicator = new ProgressIndicator(this.ModpackSettings)
            {
                Message = Translations.GetTranslatedString("checkForUpdates"),
                ProgressMinimum = 0,
                ProgressMaximum = 1
            };
            progressIndicator.Show();

            //actually check for updates
            await CheckForDatabaseUpdatesAsync(true);

            //clean up progress indicator
            progressIndicator.Close();

            Logging.Debug("database check complete");
            if (!oldDBVersion.Equals(DatabaseVersion))
            {
                Logging.Debug("old and current db versions do not match, displaying notification window");
                MessageBox.Show(Translations.GetTranslatedString("newDBApplied"));
            }
        }

        private void OnMenuItemRestoreClick(object sender, EventArgs e)
        {
            RestoreWindowFromTray();
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
                    RestoreWindowFromTray();
                    break;
            }
        }

        private void RestoreWindowFromTray()
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

        #region Custom Font code
        private void CustomFontSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomFontSelector.SelectedItem != null)
            {
                SelectedFontFamily = FontList[CustomFontSelector.SelectedIndex];
                ModpackSettings.CustomFontName = SelectedFontFamily.FontName();
                ApplyFontToWindow();
            }
        }

        private void EnableCustomFontCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)EnableCustomFontCheckbox.IsChecked)
            {
                ModpackSettings.EnableCustomFont = true;

                FontList.Clear();
                string fontsfolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                FontList.AddRange(Fonts.GetFontFamilies(fontsfolder).ToList());
                CustomFontSelector.Items.Clear();

                foreach (FontFamily font in FontList)
                {
                    CustomFontSelector.Items.Add(new TextBlock()
                    {
                        FontFamily = font,
                        Text = font.FontName()
                    });
                }
                CustomFontSelector.IsEnabled = true;
            }
            else
            {
                ModpackSettings.EnableCustomFont = false;
                CustomFontSelector.SelectedIndex = -1;
                CustomFontSelector.IsEnabled = false;

                if (DefaultFontFamily == null)
                    DefaultFontFamily = this.FontFamily;

                SelectedFontFamily = DefaultFontFamily;
                ModpackSettings.CustomFontName = SelectedFontFamily.FontName();
                ApplyFontToWindow();
            }
        }

        /// <summary>
        /// Applies the given FontFamily font type to the window. If the DefaultFontFamily property is null, then the current default value is captured for use in other windows.
        /// </summary>
        protected override void ApplyFontToWindow()
        {
            if (DefaultFontFamily == null)
            {
                DefaultFontFamily = this.FontFamily;
                SelectedFontFamily = DefaultFontFamily;
                FontList.Clear();
            }

            if (FontList.Count == 0)
                FontList.AddRange(Fonts.GetFontFamilies(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)).ToList());

            base.ApplyFontToWindow();
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

        private void OnLegacySelectColorChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.EnableColorChangeLegacyView = (bool)EnableColorChangeLegacyCB.IsChecked;
        }

        private void OnVerboseLoggingChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.VerboseLogging = (bool)VerboseLoggingCB.IsChecked;
        }

        private void OnDisableTriggersChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.DisableTriggers = (bool)DisableTriggersCB.IsChecked;
        }

        private void OnDeleteCacheFilesChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.DeleteCacheFiles = (bool)DeleteOldPackagesCB.IsChecked;
        }

        private void OnMinimizeToSystemTrayChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.MinimizeToSystemTray = (bool)MinimizeToSystemTray.IsChecked;
        }

        private void OnUninstallChangedToDefault(object sender, RoutedEventArgs e)
        {
            ModpackSettings.UninstallMode = UninstallModes.Default;
        }

        private void OnUninstallChangedToQuick(object sender, RoutedEventArgs e)
        {
            ModpackSettings.UninstallMode = UninstallModes.Quick;
        }

        private void OnForcePackagesEnabledChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ForceEnabled = (bool)ForceEnabledCB.IsChecked;
        }

        private void OnForcePackagesVisibleChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ForceVisible = (bool)ForceVisibleCB.IsChecked;
        }

        private void OnExportModeChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ExportMode = (bool)ExportModeCB.IsChecked;
        }

        private void OnThemeChanged(object sender, RoutedEventArgs e)
        {
            this.DarkTheme = (bool)ThemeDark.IsChecked;
        }

        private void OnSaveDisabledModsInSelectionChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.SaveDisabledMods = (bool)SaveDisabledModsInSelection.IsChecked;
        }

        private void OnAllowStatsGatherChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.AllowStatisticDataGather = (bool)AllowStatsGatherCB.IsChecked;
        }

        private void OnAdvancedInstallationProgressChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.AdvancedInstalProgress = (bool)AdvancedInstallationProgress.IsChecked;
        }

        private void OnShowOptionsCollapsedLegacyChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.ShowOptionsCollapsedLegacy = (bool)ShowOptionsCollapsedLegacyCB.IsChecked;
        }

        private void OnAutoOneclickShowWarningOnSelectionsFailToLoadChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.AutoOneclickShowWarningOnSelectionsFail = (bool)AutoOneclickShowWarningOnSelectionsFailButton.IsChecked;
        }

        private void OnAutoInstallOneClickInstallSelectionFilePathTextChanged(object sender, TextChangedEventArgs e)
        {
            if (loading)
                return;

            ModpackSettings.AutoOneclickSelectionFilePath = AutoInstallOneClickInstallSelectionFilePath.Text;
        }

        private void OnBetaDatabaseBranchSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UseBetaDatabaseBranches.SelectedItem is string branchName)
                ModpackSettings.BetaDatabaseSelectedBranch = branchName;
            else if (UseBetaDatabaseBranches.SelectedItem == null)
                ModpackSettings.BetaDatabaseSelectedBranch = "master";
            else
                throw new BadMemeException("aids. on a stick");
        }

        private void OnAutoSyncFrequencyTexboxTextChanged(object sender, TextChangedEventArgs e)
        {
            //check the time parsed value
            int timeToUse = CommonUtils.ParseInt(AutoSyncFrequencyTexbox.Text, 0);
            if (timeToUse < 1)
            {
                Logging.Debug("Invalid time specified, must be above 0. not saving");
            }
            else
            {
                ModpackSettings.AutoInstallFrequencyInterval = timeToUse;
            }
        }

        private void OnAutoSyncFrequencyComboBoxTimeUnitSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModpackSettings.AutoInstallFrequencyTimeUnit = AutoSyncFrequencyComboBox.SelectedIndex;
        }

        private void OnMinimalistModeChanged(object sender, RoutedEventArgs e)
        {
            ModpackSettings.MinimalistMode = (bool)MinimalistModeCB.IsChecked;
        }
        #endregion

        #region Button click events
        private void ViewCreditsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModpackSettings.AutoInstall && autoInstallPeriodicTimer.IsEnabled)
                autoInstallPeriodicTimer.Stop();

            Credits credits = new Credits(this.ModpackSettings);
            credits.ShowDialog();

            if (ModpackSettings.AutoInstall)
                autoInstallPeriodicTimer.Start();
        }

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logging.Debug("Launching button, link={0}", (sender as LinkButton).Link);
                Process.Start((sender as LinkButton).Link);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
            }
        }

        private void DiagnosticUtilitiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModpackSettings.AutoInstall && autoInstallPeriodicTimer.IsEnabled)
                autoInstallPeriodicTimer.Stop();

            Diagnostics diagnostics = new Diagnostics(this.ModpackSettings) {WoTDirectory = this.WoTDirectory };
            diagnostics.ShowDialog();

            if (ModpackSettings.AutoInstall)
                autoInstallPeriodicTimer.Start();
        }

        private void ViewNewsButton_Click(object sender, RoutedEventArgs e)
        {
            if (newsViewer != null && (newsViewer.IsLoaded))
                newsViewer.Focus();
            else if (newsViewer != null && (!newsViewer.IsLoaded))
                newsViewer = null;

            newsViewer = new NewsViewer(this.ModpackSettings);
            newsViewer.Show();
        }

        private void CancelDownloadInstallButton_Download_Click(object sender, RoutedEventArgs e)
        {
            CancelAsyncProcess(downloaderCancellationTokenSource);
        }

        private void CancelDownloadInstallButton_Install_Click(object sender, RoutedEventArgs e)
        {
            CancelAsyncProcess(installerCancellationTokenSource);
        }

        private void CancelAsyncProcess(CancellationTokenSource cancellationTokenSource)
        {
            Logging.Info(LogOptions.MethodName, "Cancel press from UI, processing request");

            if (!cancellationTokenSource.IsCancellationRequested)
            {
                Logging.Info("Requesting cancel of installation from UI - cancel process started");
                cancellationTokenSource.Cancel();
            }
            else
            {
                Logging.Info("Cancel already started - skipping request");
            }
        }
        #endregion

        #region Custom window launching
        private void LauchPatchDesigner_Click(object sender, RoutedEventArgs e)
        {
            PatchDesigner designer = new PatchDesigner(this.ModpackSettings, Logfiles.PatchDesigner) { LaunchedFromMainWindow = true, CommandLineSettings = CommandLineSettings, RunStandAloneUpdateCheck = false };
            LaunchCustomWindow(designer.Logfile, ApplicationMode.Editor, designer);
        }

        private void LauchEditor_Click(object sender, RoutedEventArgs e)
        {
            DatabaseEditor editor = new DatabaseEditor(this.ModpackSettings, Logfiles.Editor) { LaunchedFromMainWindow = true, CommandLineSettings = CommandLineSettings, RunStandAloneUpdateCheck = false };
            LaunchCustomWindow(editor.Logfile, ApplicationMode.Editor, editor);
        }

        private void LauchAutomationRunner_Click(object sender, RoutedEventArgs e)
        {
            DatabaseAutomationRunner automationRunner = new DatabaseAutomationRunner(this.ModpackSettings, Logfiles.AutomationRunner) { LaunchedFromMainWindow = true, CommandLineSettings = CommandLineSettings, RunStandAloneUpdateCheck = false };
            LaunchCustomWindow(automationRunner.Logfile, ApplicationMode.AutomationRunner, automationRunner);
        }

        private void LaunchCustomWindow(Logfiles logfile, ApplicationMode applicationMode, RelhaxCustomFeatureWindow window)
        {
            if (autoInstallPeriodicTimer != null && autoInstallPeriodicTimer.IsEnabled)
                autoInstallPeriodicTimer.Stop();

            Logging.Info("Launching editor from MainWindow");
            if (!Logging.IsLogDisposed(Logfiles.Application))
                Logging.DisposeLogging(Logfiles.Application);

            CommandLineSettings.ApplicationMode = applicationMode;

            //start editor logging system
            if (!Logging.Init(logfile, ModpackSettings.VerboseLogging, true))
            {
                MessageBox.Show("Failed to initialize logfile for {0}", logfile.ToString());
                return;
            }
            Logging.WriteHeader(logfile);

            //redirect application log file to the editor
            if (!Logging.RedirectLogOutput(Logfiles.Application, logfile))
                Logging.Error(Logfiles.Editor, LogOptions.MethodName, "Failed to redirect messages from application to {0}", logfile.ToString());

            //run target window as dialog
            window.ShowDialog();

            //after window closed, disable redirection
            if (!Logging.DisableRedirection(Logfiles.Application, logfile))
                Logging.TryWriteToLog("Failed to cancel redirect messages from application to {0}", logfile, LogLevel.Error, logfile.ToString());

            //also de-init editor logging
            if (!Logging.IsLogDisposed(logfile))
                Logging.DisposeLogging(logfile);

            //also re-init application logging and set as application run mode
            CommandLineSettings.ApplicationMode = ApplicationMode.Default;
            if (!Logging.Init(Logfiles.Application, ModpackSettings.VerboseLogging, true))
            {
                MessageBox.Show(Translations.GetTranslatedString("appFailedCreateLogfile"));
                Application.Current.Shutdown((int)ReturnCodes.LogfileError);
            }

            if (ModpackSettings.AutoInstall)
                autoInstallPeriodicTimer.Start();
        }
        #endregion

        #region Actually interesting UI events
        private async void OnUseBetaDatabaseChanged(object sender, RoutedEventArgs e)
        {
            if (loading)
                return;

            UseBetaDatabaseCB.IsEnabled = false;
            UseBetaDatabaseBranches.IsEnabled = false;
            if ((bool)UseBetaDatabaseCB.IsChecked)
            {
                //if auto install is already enabled, then confirm with the user that we want to enable auto database updates AND use the beta version of the database
                if (ModpackSettings.AutoInstall)
                {
                    Logging.Info(LogOptions.MethodName, "AutoInstall is enabled already, verify with user that we want to enable beta database. Potential high installation frequency");
                    autoInstallPeriodicTimer.Stop();

                    if (MessageBox.Show(Translations.GetTranslatedString("autoInstallWithBetaDBConfirmBody"), Translations.GetTranslatedString("autoInstallWithBetaDBConfirmHeader"), MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        Logging.Info(LogOptions.MethodName, "AutoInstall is enabled, user declined, abort");
                        //disconnect the event handlers before we uncheck the beta database usage
                        UseBetaDatabaseCB.Click -= OnUseBetaDatabaseChanged;
                        UseBetaDatabaseCB.IsChecked = false;
                        UseBetaDatabaseCB.Click += OnUseBetaDatabaseChanged;
                        autoInstallPeriodicTimer.Start();
                        UseBetaDatabaseCB.IsEnabled = true;
                        return;
                    }
                }
                
                await PopulateBetaDatabaseBranchesListAsync();
                SelectBetaDatabaseBranch();

                ModpackSettings.DatabaseDistroVersion = DatabaseVersions.Beta;
                UseBetaDatabaseBranches.IsEnabled = true;
                UseBetaDatabaseCB.IsEnabled = true;
            }
            else
            {
                ModpackSettings.DatabaseDistroVersion = DatabaseVersions.Stable;
                if (UseBetaDatabaseBranches.Items.Count > 0)
                    UseBetaDatabaseBranches.Items.Clear();
                UseBetaDatabaseCB.IsEnabled = true;
            }

            if (databaseVersion == DatabaseVersions.Test)
            {
                MessageBox.Show("Setting applied, but you are currently in test mode. Test mode will remain active until application restart.");
                return;
            }

            //actually apply the value
            databaseVersion = ModpackSettings.DatabaseDistroVersion;

            if (ModpackSettings.AutoInstall)
            {
                //stop timer for applying changes
                Logging.Debug(LogOptions.MethodName, "AutoInstall is enabled, re-configure timer for change and start it again");
                if (databaseVersion == DatabaseVersions.Beta)
                    InitAutoInstallTimerForBetaDbAsync();
                ConfigureAutoInstallTimerEvent();
                //then restart the timer
                if (!autoInstallPeriodicTimer.IsEnabled)
                    autoInstallPeriodicTimer.Start();
            }

            ProcessTitle();
        }

        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //the event should not fire if it's loading. loading boolean set in window loading event takes care of this
            if (loading)
                return;
            Languages selectedLanguage = Languages.English;

            switch (LanguagesSelector.SelectedItem as string)
            {
                case Translations.LanguageEnglish:
                    selectedLanguage = Languages.English;
                    break;
                case Translations.LanguageFrench:
                    selectedLanguage = Languages.French;
                    break;
                case Translations.LanguageGerman:
                    selectedLanguage = Languages.German;
                    break;
                case Translations.LanguagePolish:
                    selectedLanguage = Languages.Polish;
                    break;
                case Translations.LanguageRussian:
                    selectedLanguage = Languages.Russian;
                    break;
                case Translations.LanguageSpanish:
                    selectedLanguage = Languages.Spanish;
                    break;
            }

            Translations.SetLanguage(selectedLanguage);
            this.ModpackSettings.Language = selectedLanguage;
            Translations.LocalizeWindow(this, true);
            ApplyCustomUILocalizations();
        }

        private void ApplyCustomScalingSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                //if the new slider and old display scale are the same, then no need to apply
                if (ModpackSettings.DisplayScale == ApplyCustomScalingSlider.Value)
                {
                    Logging.Debug("ModpackSettings.DisplayScale is same as Slider.Value, no need to apply");
                    return;
                }
                ApplyCustomScalingLabel.Text = string.Format("{0}x", ApplyCustomScalingSlider.Value.ToString("N"));
                double oldTempValue = ModpackSettings.DisplayScale;
                ModpackSettings.DisplayScale = ApplyCustomScalingSlider.Value;
                ApplyApplicationScale(ModpackSettings.DisplayScale);
                ScalingConfirmation confirmation = new ScalingConfirmation(this.ModpackSettings);
                if (!(bool)confirmation.ShowDialog())
                {
                    ModpackSettings.DisplayScale = oldTempValue;
                    ApplyApplicationScale(ModpackSettings.DisplayScale);
                    ApplyCustomScalingSlider.Value = ModpackSettings.DisplayScale;
                    ApplyCustomScalingLabel.Text = string.Format("{0}x", ApplyCustomScalingSlider.Value.ToString("N"));
                }
            }
        }

        private void SelectDownloadMirrorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loading)
                return;
            ModpackSettings.DownloadMirror = SelectDownloadMirrorCombobox.SelectedIndex;
        }
        #endregion

        #region Other UI methods
        private void ProcessTitle()
        {
            if (string.IsNullOrEmpty(oldModpackTitle))
            {
                Logging.Info("[ProcessTitle()] oldModpackTitle is empty, don't update text!");
                return;
            }

            //apply the title change for beta application and beta database
            if (databaseVersion != DatabaseVersions.Stable)
                Title = string.Format("{0} ({1} DB)", oldModpackTitle, databaseVersion.ToString());
            else
                Title = oldModpackTitle;

            if (ApplicationConstants.ApplicationVersion != ApplicationVersions.Stable)
                Title = string.Format("{0} ({1} APP)", Title, ApplicationConstants.ApplicationVersion.ToString());
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

        private void ApplySettingsToUI()
        {
            //apply the internal setting to what the UI setting is
            //this is only run in TheMainWindow_Loaded, either by default or running again after a v2 upgrade
            //NOTE: at this time, databaseVersion is not set yet
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
            DeleteOldPackagesCB.IsChecked = ModpackSettings.DeleteCacheFiles;
            MinimizeToSystemTray.IsChecked = ModpackSettings.MinimizeToSystemTray;
            AdvancedInstallationProgress.IsChecked = ModpackSettings.AdvancedInstalProgress;
            ShowOptionsCollapsedLegacyCB.IsChecked = ModpackSettings.ShowOptionsCollapsedLegacy;
            AutoOneclickShowWarningOnSelectionsFailButton.IsChecked = ModpackSettings.AutoOneclickShowWarningOnSelectionsFail;
            EnableCustomFontCheckbox.IsChecked = ModpackSettings.EnableCustomFont;
            OneClickInstallCB.IsChecked = ModpackSettings.OneClickInstall;
            AutoInstallCB.IsChecked = ModpackSettings.AutoInstall;
            MinimalistModeCB.IsChecked = ModpackSettings.MinimalistMode;

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

            switch (ModpackSettings.ApplicationTheme)
            {
                case UIThemes.Default:
                    ThemeDefault.IsChecked = true;
                    break;
                case UIThemes.Dark:
                    ThemeDark.IsChecked = true;
                    break;
            }
            DarkTheme = ModpackSettings.ApplicationTheme == UIThemes.Dark;

            //apply beta application settings
            UseBetaApplicationCB.IsChecked = (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Beta);

            //apply beta database settings
            UseBetaDatabaseCB.IsChecked = (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Beta);
            UseBetaDatabaseBranches.IsEnabled = false;
            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Beta)
            {
                //NOTE: it doesn't wait here, it continues to the last section. The auto install stuff below does not require to have this done
                Dispatcher.Invoke(async () =>
                {
                    //NOTE: it will at least execute these in order
                    await PopulateBetaDatabaseBranchesListAsync();
                    SelectBetaDatabaseBranch();
                    UseBetaDatabaseBranches.IsEnabled = true;
                }, DispatcherPriority.Normal);
            }

            //apply auto sync time unit and amount
            AutoSyncFrequencyTexbox.Text = ModpackSettings.AutoInstallFrequencyInterval.ToString();
            if (ModpackSettings.AutoInstallFrequencyTimeUnit < AutoSyncFrequencyComboBox.Items.Count && ModpackSettings.AutoInstallFrequencyTimeUnit > 0)
            {
                AutoSyncFrequencyComboBox.SelectedIndex = ModpackSettings.AutoInstallFrequencyTimeUnit;
            }
            else
            {
                AutoSyncFrequencyComboBox.SelectedIndex = 0;
            }

            if (!string.IsNullOrWhiteSpace(ModpackSettings.AutoOneclickSelectionFilePath))
                AutoInstallOneClickInstallSelectionFilePath.Text = ModpackSettings.AutoOneclickSelectionFilePath;

            //apply auto install check
            if (ModpackSettings.AutoInstall)
            {
                if (autoInstallPeriodicTimer != null && autoInstallPeriodicTimer.IsEnabled)
                    autoInstallPeriodicTimer.Stop();
                autoInstallPeriodicTimer = null;
                if (!CheckIfAutoInstallFileExists())
                    return;
                InitAutoInstallTimerAndConfigureInterval();
                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Beta)
                    InitAutoInstallTimerForBetaDbAsync();
                ConfigureAutoInstallTimerEvent();
                autoInstallPeriodicTimer.Start();
            }

            //apply download mirror
            SelectDownloadMirrorCombobox.SelectionChanged -= SelectDownloadMirrorCombobox_SelectionChanged;
            if ((SelectDownloadMirrorCombobox.Items.Count) > ModpackSettings.DownloadMirror)
            {
                SelectDownloadMirrorCombobox.SelectedIndex = ModpackSettings.DownloadMirror;
            }
            else if (SelectDownloadMirrorCombobox.Items.Count == 0)
            {
                Logging.Error("{0} items count is 0 when trying to set download mirror!", nameof(SelectDownloadMirrorCombobox));
            }
            else
            {
                Logging.Warning("{0} has {1} items when trying to select index of {2}. Using default index 0", nameof(SelectDownloadMirrorCombobox), SelectDownloadMirrorCombobox.Items.Count, ModpackSettings.DownloadMirror);
                ModpackSettings.DownloadMirror = 0;
                SelectDownloadMirrorCombobox.SelectedIndex = ModpackSettings.DownloadMirror;
            }
            SelectDownloadMirrorCombobox.SelectionChanged += SelectDownloadMirrorCombobox_SelectionChanged;
        }

        private void ToggleUIButtons(bool toggle)
        {
            Logging.Debug("The main window UI was toggled: {0}", toggle.ToString());
            List<FrameworkElement> controlsToToggle = UiUtils.GetAllWindowComponentsLogical(this, false);
            //any to remove here
            if (controlsToToggle.Contains(CancelDownloadInstallButton))
                controlsToToggle.Remove(CancelDownloadInstallButton);
            foreach (FrameworkElement control in controlsToToggle)
            {
                if (control is Button || control is CheckBox || control is RadioButton || control is ComboBox || control is Slider)
                {
                    if (disabledBlacklist.Contains(control))
                        control.IsEnabled = false;
                    else if (enabledBlacklist.Contains(control))
                        control.IsEnabled = true;
                    else
                        control.IsEnabled = toggle;
                }
            }
            //any to include here that aren't any of the above class types
            AutoSyncFrequencyTexbox.IsEnabled = toggle;
            AutoSyncFrequencyComboBox.IsEnabled = toggle;

            //if false, disabling components to do an installation. so disable the auto install timer as well
            //else enable it again
            //sending stop() will restart the timer
            //https://stackoverflow.com/questions/15617068/does-system-timers-timer-stop-restart-the-interval-countdown
            if (autoInstallPeriodicTimer != null)
            {
                if (toggle)
                    autoInstallPeriodicTimer.Start();
                else
                    autoInstallPeriodicTimer.Stop();
            }
        }

        private void ApplyCustomUILocalizations()
        {
            //set the application information text box
            ApplicationVersionLabel.Text = Translations.GetTranslatedString("applicationVersion") + " " + CommonUtils.GetApplicationVersion();

            //set the database information text box
            DatabaseVersionLabel.Text = Translations.GetTranslatedString("databaseVersion") + " " + DatabaseVersion;

            //set the number of cores label
            MulticoreExtractionCoresCountLabel.Text = string.Format(Translations.GetTranslatedString("MulticoreExtractionCoresCountLabel"), ApplicationConstants.NumLogicalProcesors);

            //set backup file size labels
            if (BackupSizeCalculated)
                BackupModsSizeLabelUsed.Text = string.Format(Translations.GetTranslatedString("BackupModsSizeLabelUsed"), backupFiles.Count(), FileUtils.SizeSuffix((ulong)backupFolderTotalSize, 1, true, true));
            else
                BackupModsSizeLabelUsed.Text = Translations.GetTranslatedString("backupModsSizeCalculating");

            //set values for the auto sync frequency combobox
            AutoSyncFrequencyComboBox.Items.Clear();
            AutoSyncFrequencyComboBox.Items.Add(Translations.GetTranslatedString("minutes"));
            AutoSyncFrequencyComboBox.Items.Add(Translations.GetTranslatedString("hours"));
            AutoSyncFrequencyComboBox.Items.Add(Translations.GetTranslatedString("days"));
        }

        private async Task PopulateBetaDatabaseBranchesListAsync()
        {
            Logging.Debug(LogOptions.MethodName, "Populate branch combobox with list of database branches");
            UseBetaDatabaseBranches.Items.Clear();
            UseBetaDatabaseBranches.Items.Add(Translations.GetTranslatedString("loadingBranches"));

            List<string> branches = await CommonUtils.GetListOfGithubRepoBranchesAsync(ApplicationConstants.BetaDatabaseBranchesURL);
            UseBetaDatabaseBranches.Items.Clear();

            //fill the UI with branch items
            foreach (string s in branches)
                UseBetaDatabaseBranches.Items.Add(s);
        }

        private void SelectBetaDatabaseBranch()
        {
            if (!string.IsNullOrEmpty(ModpackSettings.BetaDatabaseSelectedBranch) && UseBetaDatabaseBranches.Items.Contains(ModpackSettings.BetaDatabaseSelectedBranch))
            {
                Logging.Info(LogOptions.MethodName, "Branch '{0}' set from settings exists on repo and is being set", ModpackSettings.BetaDatabaseSelectedBranch);
                UseBetaDatabaseBranches.SelectedItem = ModpackSettings.BetaDatabaseSelectedBranch;
            }
            else
            {
                Logging.Warning(LogOptions.MethodName, "Branch '{0}' does not exist, setting default branch to repo default 'master'", string.IsNullOrWhiteSpace(ModpackSettings.BetaDatabaseSelectedBranch) ? "(empty)" : ModpackSettings.BetaDatabaseSelectedBranch);
                //select master as default if the one specified in the settings does not exist. also update settings
                UseBetaDatabaseBranches.SelectedItem = "master";
                ModpackSettings.BetaDatabaseSelectedBranch = "master";
            }
        }
        #endregion

        #region Other methods
        //move folders with a special middle step
        private void MoveUpgradeFolder(string oldPath, string newPath)
        {
            Logging.Info("upgrading folder {0} to {1}", Path.GetFileName(oldPath), Path.GetFileName(newPath));
            if (!Directory.Exists(oldPath))
            {
                Logging.Warning("old folder {0} does not exist, skipping", Path.GetFileName(oldPath));
                return;
            }

            if (Directory.Exists(newPath) && newPath.Equals(ApplicationConstants.RelhaxUserSelectionsFolderPath))
            {
                Logging.Warning("new folder {0} already exists, copy files over and delete old folder", Path.GetFileName(newPath));
                foreach (string file in FileUtils.FileSearch(oldPath, SearchOption.TopDirectoryOnly, false, false, "*.xml", 5, 3, false))
                {
                    string newFilePath = Path.Combine(ApplicationConstants.RelhaxUserSelectionsFolderPath, Path.GetFileName(file));
                    if (!File.Exists(newFilePath))
                        File.Copy(file, newFilePath);
                }
                FileUtils.DirectoryDelete(oldPath, true);
                return;
            }


            //step 1 is to move it to a temp folder
            string middlePath = oldPath + "_";
            Directory.Move(oldPath, middlePath);

            //step 2 is to move it to the real folder
            Directory.Move(middlePath, newPath);

            Logging.Info("upgrade of folder {0} successful", Path.GetFileName(newPath));
        }
        
        //asynchronously get the file sizes of backups
        private Task GetBackupFilesizesAsync()
        {
            return Task.Run(() =>
            {
                Logging.Debug("Starting async task of getting file sizes of backups");
                BackupModsSizeLabelUsed.Dispatcher.Invoke(() =>
                {
                    BackupModsSizeLabelUsed.Text = Translations.GetTranslatedString("backupModsSizeCalculating");
                });

                backupFolderTotalSize = 0;
                backupFiles = FileUtils.FileSearch(ApplicationConstants.RelhaxModBackupFolderPath, SearchOption.TopDirectoryOnly, false, false, "*.zip", 5, 3, false);
                foreach (string file in backupFiles)
                {
                    backupFolderTotalSize += FileUtils.GetFilesize(file);
                }
                BackupSizeCalculated = true;
                Logging.Debug("Completed async task of getting file sizes of backups");

                BackupModsSizeLabelUsed.Dispatcher.Invoke(() =>
                {
                    BackupModsSizeLabelUsed.Text = string.Format(Translations.GetTranslatedString("BackupModsSizeLabelUsed"), backupFiles.Count(), FileUtils.SizeSuffix((ulong)backupFolderTotalSize, 1, true, true));
                });
            });
        }

        //Get all xml strings for the V2 database file format from the selected beta database github branch
        private Task<string> GetBetaDatabase1V1ForStringCompareAsync()
        {
            return Task<string>.Run(async () => {
                List<string> downloadURLs = await databaseManager.GetBetaDatabase1V1FilesListAsync();

                string[] downloadStrings = CommonUtils.DownloadStringsFromUrls(downloadURLs);

                return string.Join(string.Empty, downloadStrings);
            });
        }
        #endregion
    }
}
