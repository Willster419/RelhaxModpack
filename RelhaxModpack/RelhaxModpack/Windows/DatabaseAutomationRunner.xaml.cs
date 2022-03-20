using RelhaxModpack.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using RelhaxModpack.Automation;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using Microsoft.Win32;
using System.IO;
using RelhaxModpack.Common;
using System.ComponentModel;
using RelhaxModpack.Utilities;
using RelhaxModpack.UI;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseAutomationRunner.xaml
    /// </summary>
    public partial class DatabaseAutomationRunner : RelhaxCustomFeatureWindow
    {
        /// <summary>
        /// The command line argument specified at application launch to show this window
        /// </summary>
        public const string CommandLineArg = "automation-runner";

        /// <summary>
        /// The name of the logfile
        /// </summary>
        public const string LoggingFilename = "RelhaxAutomation.log";

        /// <summary>
        /// Event handler to use when a task is reporting download progress.
        /// </summary>
        public DownloadProgressChangedEventHandler DownloadProgressChanged = null;

        /// <summary>
        /// Event handler to use when a task is reporting generic progress.
        /// </summary>
        public ProgressChangedEventHandler ProgressChanged = null;

        /// <summary>
        /// Event handler to use when a task is reporting a download has been completed.
        /// </summary>
        public DownloadDataCompletedEventHandler DownloadDataCompleted = null;

        /// <summary>
        /// Event handler to use when a task is reporting a download to a file has been completed.
        /// </summary>
        public AsyncCompletedEventHandler DownloadFileCompleted = null;

        /// <summary>
        /// Event handler to use when a task is reporting upload progress of a file.
        /// </summary>
        public UploadProgressChangedEventHandler UploadProgressChanged = null;

        /// <summary>
        /// Event handler to use when a task is reporting an upload of a file has been completed.
        /// </summary>
        public UploadFileCompletedEventHandler UploadFileCompleted = null;

        /// <summary>
        /// Event handler to use when a task is reporting generic progress using the RelhaxProgress object.
        /// </summary>
        /// <seealso cref="RelhaxProgress"/>
        public EventHandler<RelhaxProgress> RelhaxProgressChanged = null;

        private AutomationRunnerSettings AutomationSettings = new AutomationRunnerSettings();

        private AutomationSequencer AutomationSequencer = null;

        private RelhaxLogViewer logViewer;

        private HtmlPathSelector htmlPathSelector;

        private DatabaseManager databaseManager;

        private SaveFileDialog SaveDatabaseDialog;

        private OpenFileDialog OpenRootXmlDialog;

        private readonly Action[] settingsMethods;

        private CancellationTokenSource cancellationTokenSource;

        private Dispatcher loggerDispatcher;

        /// <summary>
        /// Create an instance of the DatabaseAutomationRunner window
        /// </summary>
        public DatabaseAutomationRunner(ModpackSettings modpackSettings, Logfiles logfile) : base(modpackSettings, logfile)
        {
            InitializeComponent();
            DownloadProgressChanged = WebClient_DownloadProgressChanged;
            DownloadDataCompleted = WebClient_DownloadDataComplted;
            DownloadFileCompleted = WebClient_TransferFileCompleted;
            UploadFileCompleted = WebClient_UploadFileCompleted;
            UploadProgressChanged = WebClient_UploadProgressChanged;
            RelhaxProgressChanged = RelhaxProgressReport_ProgressChanged;
            ProgressChanged = GenericProgressChanged;
            Settings = AutomationSettings;

            //https://stackoverflow.com/questions/7712137/array-containing-methods
            settingsMethods = new Action[]
            {
               () => OpenLogWindowOnStartupSetting_Click(null, null),
               () => BigmodsUsernameSetting_TextChanged(null, null),
               () => BigmodsPasswordSetting_TextChanged(null, null),
               () => DumpParsedMacrosPerSequenceRunSetting_Click(null, null),
               () => DumpEnvironmentVariablesAtSequenceStartSetting_Click(null, null),
               () => SuppressDebugMessagesSetting_Click(null, null),
               () => AutomamtionDatabaseSelectedBranchSetting_TextChanged(null, null),
               () => SelectDBSaveLocationSetting_TextChanged(null, null),
               () => UseLocalRunnerDatabaseSetting_Click(null, null),
               () => LocalRunnerDatabaseRootSetting_TextChanged(null, null),
               () => SelectWoTInstallLocationSetting_TextChanged(null, null),
               () => UserMacro1Name_TextChanged(null, null),
               () => UserMacro1Value_TextChanged(null, null),
               () => UserMacro2Name_TextChanged(null, null),
               () => UserMacro2Value_TextChanged(null, null),
               () => UserMacro3Name_TextChanged(null, null),
               () => UserMacro3Value_TextChanged(null, null)
            };
        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);
            AutomationSequencer = new AutomationSequencer() { AutomationRunnerSettings = this.AutomationSettings, DatabaseAutomationRunner = this, DatabaseManager = databaseManager };

            LoadSettingsToUI();

            //init the log viewer window
            FocusOrCreateLogWindow(AutomationSettings.OpenLogWindowOnStartup);

            await LoadAutomationSequencerAsync();

            Init = false;
        }

        private async Task LoadAutomationSequencerAsync()
        {
            if (!AutomationSettings.UseLocalRunnerDatabase)
            {
                //load branches from the server
                Logging.Info("Loading branches");
                await AutomationSequencer.LoadBranchesListAsync();

                //ensure that the branch specified in settings exists, and if so apply it. else apply the default setting
                if (!AutomationSequencer.AutomationBranches.Contains(AutomationSettings.SelectedBranch))
                {
                    Logging.Error("The selected branch does not exist on the server: {0}", AutomationSettings.SelectedBranch);
                    MessageBox.Show(string.Format("The selected branch {0} does not exist on the server, setting to default", AutomationSettings.SelectedBranch));
                    AutomationSettings.SelectedBranch = AutomationSequencer.AutomationRepoDefaultBranch;
                    AutomamtionDatabaseSelectedBranchSetting.Text = AutomationSequencer.AutomationRepoDefaultBranch;
                    //TODO: command line mode should be an exit return
                }
                else
                {
                    Logging.Info("Applying branch to load from: {0}", AutomationSettings.SelectedBranch);
                }
            }

            //load the available package sequences from the root document
            SequencesAvailableListBox.Items.Clear();
            SequencesToRunListBox.Items.Clear();
            SequencesAvailableListBox.Items.Add("Loading available sequences from database...");
            Logging.Info("Loading sequences from root document");
            await AutomationSequencer.LoadRootDocumentAsync();

            //parse the document now that it's loaded
            Logging.Info("Parsing sequences from root document");
            AutomationSequencer.ParseRootDocument();

            //load the sequences into the listbox view
            SequencesAvailableListBox.Items.Clear();
            foreach (AutomationSequence sequence in AutomationSequencer.AutomationSequences)
            {
                SequencesAvailableListBox.Items.Add(sequence);
            }
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            //close windows if open
            loggerDispatcher?.Invoke(() => { if (logViewer.IsLoaded) logViewer.Close(); });

            if (htmlPathSelector != null && htmlPathSelector.IsLoaded)
                htmlPathSelector.Close();

            //disposal
            if (AutomationSequencer != null)
                AutomationSequencer.Dispose();
            if (cancellationTokenSource != null)
                cancellationTokenSource.Dispose();
            DownloadProgressChanged = null;
        }

        private void LoadSettingsToUI()
        {
            BigmodsUsernameSetting.Text = AutomationSettings.BigmodsUsername;
            BigmodsPasswordSetting.Text = AutomationSettings.BigmodsPassword;
            OpenLogWindowOnStartupSetting.IsChecked = AutomationSettings.OpenLogWindowOnStartup;
            DumpParsedMacrosPerSequenceRunSetting.IsChecked = AutomationSettings.DumpParsedMacrosPerSequenceRun;
            AutomamtionDatabaseSelectedBranchSetting.Text = AutomationSettings.SelectedBranch;
            SelectDBSaveLocationSetting.Text = AutomationSettings.DatabaseSavePath;
            DumpEnvironmentVariablesAtSequenceStartSetting.IsChecked = AutomationSettings.DumpShellEnvironmentVarsPerSequenceRun;
            UseLocalRunnerDatabaseSetting.IsChecked = AutomationSettings.UseLocalRunnerDatabase;
            LocalRunnerDatabaseRootSetting.Text = AutomationSettings.LocalRunnerDatabaseRoot;
            SelectWoTInstallLocationSetting.Text = AutomationSettings.WoTClientInstallLocation;
            SuppressDebugMessagesSetting.IsChecked = AutomationSettings.SuppressDebugMessagesInLogWindow;
            ClearLogWindowOnSequenceRunSetting.IsChecked = AutomationSettings.ClearLogWindowOnSequenceStart;
            ClearLogFileOnSequenceRunSetting.IsChecked = AutomationSettings.ClearLogFileOnSequenceStart;
            UserMacro1Name.Text = AutomationSettings.UserMacro1Name;
            UserMacro1Value.Text = AutomationSettings.UserMacro1Value;
            UserMacro2Name.Text = AutomationSettings.UserMacro2Name;
            UserMacro2Value.Text = AutomationSettings.UserMacro2Value;
            UserMacro3Name.Text = AutomationSettings.UserMacro3Name;
            UserMacro3Value.Text = AutomationSettings.UserMacro3Value;
        }

        private void MoveSequenceToRunList()
        {
            if (SequencesAvailableListBox.SelectedItems.Count == 0)
                return;

            foreach (AutomationSequence sequence in SequencesAvailableListBox.SelectedItems)
            {
                if (!SequencesToRunListBox.Items.Contains(sequence))
                    SequencesToRunListBox.Items.Add(sequence);
            }
        }

        #region Progress reporting code
        private void GenericProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 69)
            {
                //used for getting ftp download size from server
                ShowAutomationProgress((long)e.UserState);
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (AutomationTaskProgressBar.Visibility != Visibility.Visible)
            {
                ShowAutomationProgress(e.TotalBytesToReceive);
            }

            AutomationTaskProgressBar.Value = e.BytesReceived;
            AutomationTaskProgressTextBlock.Text = string.Format("{0} of {1}", e.BytesReceived, (int)AutomationTaskProgressBar.Maximum);
        }

        private void WebClient_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (AutomationTaskProgressBar.Visibility != Visibility.Visible)
            {
                //this only will happen once during the task's execution
                ShowAutomationProgress(e.TotalBytesToSend);
            }
            AutomationTaskProgressBar.Value = e.BytesSent;
            AutomationTaskProgressTextBlock.Text = string.Format("{0} of {1}", e.BytesSent, e.TotalBytesToSend);
        }

        private void RelhaxProgressReport_ProgressChanged(object sender, RelhaxProgress e)
        {
            if (AutomationTaskProgressBar.Visibility != Visibility.Visible)
            {
                ShowAutomationProgress(e.ChildTotal);
            }

            if (e.ChildCurrent == e.ChildTotal)
            {
                HideAutomationProgress();
            }

            switch (e.ChildCurrentProgress)
            {
                case "barWithTextChild":
                    ReportRelhaxProgressBarWithTextChild(e);
                    return;

                case "barWithTextParrent":
                    ReportRelhaxProgressBarWithTextParrent(e);
                    return;

                case "barChildTextParent":
                    ReportRelhaxProgressBarChildTextParent(e);
                    return;
            }
        }

        private void WebClient_DownloadDataComplted(object sender, DownloadDataCompletedEventArgs e)
        {
            HideAutomationProgress();
        }

        private void WebClient_TransferFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            HideAutomationProgress();
        }

        private void WebClient_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            HideAutomationProgress();
        }

        private void ShowAutomationProgress(long maxProgress)
        {
            AutomationTaskProgressTextBlock.Visibility = Visibility.Visible;
            AutomationTaskProgressBar.Visibility = Visibility.Visible;
            AutomationTaskProgressBar.Minimum = 0;
            AutomationTaskProgressBar.Maximum = maxProgress;
        }

        private void HideAutomationProgress()
        {
            AutomationTaskProgressBar.Value = AutomationTaskProgressBar.Minimum;
            AutomationTaskProgressBar.Visibility = Visibility.Hidden;
            AutomationTaskProgressTextBlock.Text = string.Empty;
            AutomationTaskProgressTextBlock.Visibility = Visibility.Hidden;
        }

        private void ReportRelhaxProgressBarWithTextChild(RelhaxProgress relhaxProgress)
        {
            AutomationTaskProgressBar.Value = relhaxProgress.ChildCurrent;
            AutomationTaskProgressTextBlock.Text = string.Format("{0} of {1}", relhaxProgress.ChildCurrent, relhaxProgress.ChildTotal);
        }

        private void ReportRelhaxProgressBarWithTextParrent(RelhaxProgress relhaxProgress)
        {
            AutomationTaskProgressBar.Value = relhaxProgress.ParrentCurrent;
            AutomationTaskProgressTextBlock.Text = string.Format("{0} of {1}", relhaxProgress.ParrentCurrent, relhaxProgress.ParrentTotal);
        }

        private void ReportRelhaxProgressBarChildTextParent(RelhaxProgress relhaxProgress)
        {
            if (AutomationTaskProgressBar.Maximum != relhaxProgress.ChildTotal)
                AutomationTaskProgressBar.Maximum = relhaxProgress.ChildTotal;
            AutomationTaskProgressBar.Value = relhaxProgress.ChildCurrent;
            AutomationTaskProgressTextBlock.Text = string.Format("{0} of {1}", relhaxProgress.ParrentCurrent, relhaxProgress.ParrentTotal);
        }
        #endregion

        #region Sequence move around buttons
        private struct LocationTracker
        {
            public AutomationSequence Sequence;
            public int OldIndex;
        }

        private void MoveSequencesToRunListButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSequenceToRunList();
        }

        private void SequencesAvailableListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MoveSequenceToRunList();
        }

        private void MoveUpSelectedSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesToRunListBox.SelectedItems.Count == 0)
                return;

            //make a list outside of SequencesToRunListBox so we're not actively modifying a collection we're looping on
            List<AutomationSequence> itemsToMove = SequencesToRunListBox.SelectedItems.Cast<AutomationSequence>().ToList();

            //track the original index of each of these items
            int lowestIndex = SequencesToRunListBox.Items.Count;
            List<LocationTracker> trackerList = new List<LocationTracker>();
            foreach (AutomationSequence sequence in itemsToMove)
            {
                int currentIndex = SequencesToRunListBox.Items.IndexOf(sequence);
                if (currentIndex < lowestIndex)
                {
                    lowestIndex = currentIndex;
                }
                trackerList.Add(new LocationTracker
                {
                    Sequence = sequence,
                    OldIndex = SequencesToRunListBox.Items.IndexOf(sequence)
                });
            }

            if (lowestIndex == 0)
                return;

            //now we have the old index of each one. we essentially want to move them all up by 1
            foreach (LocationTracker locationTracker in trackerList)
            {
                //remove from the list
                SequencesToRunListBox.Items.Remove(locationTracker.Sequence);

                //re-insert at one index up
                SequencesToRunListBox.Items.Insert(locationTracker.OldIndex - 1, locationTracker.Sequence);
            }

            //then highlight/select them again
            SequencesToRunListBox.SelectedItems.Clear();
            foreach (AutomationSequence sequence in itemsToMove)
            {
                SequencesToRunListBox.SelectedItems.Add(sequence);
            }
        }

        private void MoveDownSelectedSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesToRunListBox.SelectedItems.Count == 0)
                return;

            List<AutomationSequence> itemsToMove = SequencesToRunListBox.SelectedItems.Cast<AutomationSequence>().ToList();
            int highestIndex = 0;
            List<LocationTracker> trackerList = new List<LocationTracker>();
            foreach (AutomationSequence sequence in itemsToMove)
            {
                int currentIndex = SequencesToRunListBox.Items.IndexOf(sequence);
                if (currentIndex > highestIndex)
                {
                    highestIndex = currentIndex;
                }
                trackerList.Add(new LocationTracker
                {
                    Sequence = sequence,
                    OldIndex = SequencesToRunListBox.Items.IndexOf(sequence)
                });
            }

            if (highestIndex == SequencesToRunListBox.Items.Count - 1)
                return;

            //now we have the old index of each one. we essentially want to move them all down by 1
            foreach (LocationTracker locationTracker in trackerList)
            {
                //remove from the list
                SequencesToRunListBox.Items.Remove(locationTracker.Sequence);

                //re-insert at one index up
                SequencesToRunListBox.Items.Insert(locationTracker.OldIndex + 1, locationTracker.Sequence);
            }

            //then highlight/select them again
            SequencesToRunListBox.SelectedItems.Clear();
            foreach (AutomationSequence sequence in itemsToMove)
            {
                SequencesToRunListBox.SelectedItems.Add(sequence);
            }
        }

        private void RemoveSelectedSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesToRunListBox.SelectedItems.Count == 0)
                return;

            List<AutomationSequence> itemsToRemove = SequencesToRunListBox.SelectedItems.Cast<AutomationSequence>().ToList();
            foreach (AutomationSequence sequence in itemsToRemove)
                SequencesToRunListBox.Items.Remove(sequence);
        }

        private void RemoveAllSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesToRunListBox.Items.Count == 0)
                return;

            SequencesToRunListBox.Items.Clear();
        }
        #endregion

        #region Bottom rows buttons
        private void OpenLogfileViewerButton_Click(object sender, RoutedEventArgs e)
        {
            FocusOrCreateLogWindow(true);
        }

        private void OpenHtmlPathSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            if (htmlPathSelector == null || !htmlPathSelector.IsLoaded)
            {
                if (htmlPathSelector != null)
                    htmlPathSelector = null;
                htmlPathSelector = new HtmlPathSelector(ModpackSettings);
                htmlPathSelector.Show();
            }
            else
            {
                htmlPathSelector.Focus();
            }
        }

        private async void ReloadSequencesButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAutomationSequencerAsync();
        }

        private async void CleanWorkingDirectoriesButton_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource tokenSource;
            Progress<RelhaxProgress> reporter = new Progress<RelhaxProgress>();
            RelhaxProgress progress = new RelhaxProgress();
            AutomationTaskProgressBar.Visibility = Visibility.Visible;
            AutomationTaskProgressBar.Minimum = AutomationTaskProgressBar.Maximum = AutomationTaskProgressBar.Value = 0;
            AutomationTaskProgressTextBlock.Visibility = Visibility.Visible;
            AutomationTaskProgressTextBlock.Text = string.Empty;

            reporter.ProgressChanged += (sender_, args) => 
            {
                if (AutomationTaskProgressBar.Minimum  != 0)
                    AutomationTaskProgressBar.Minimum = 0;

                if (AutomationTaskProgressBar.Maximum != args.ChildTotal)
                    AutomationTaskProgressBar.Maximum = args.ChildTotal;

                AutomationTaskProgressBar.Value = args.ChildCurrent;

                AutomationTaskProgressTextBlock.Text = args.ChildCurrentProgress;
            };

            using (tokenSource = new CancellationTokenSource())
            {
                await AutomationSequencer.CleanWorkingDirectoriesAsync(reporter, progress, tokenSource.Token);
            }

            this.Dispatcher.InvokeAsync(async () =>
            {
                AutomationTaskProgressTextBlock.Text = "Done";
                await Task.Delay(2000);
                AutomationTaskProgressBar.Visibility = Visibility.Hidden;
                AutomationTaskProgressBar.Minimum = AutomationTaskProgressBar.Maximum = AutomationTaskProgressBar.Value = 0;
                AutomationTaskProgressTextBlock.Visibility = Visibility.Hidden;
                AutomationTaskProgressTextBlock.Text = string.Empty;
            });
        }
        #endregion

        private void FocusOrCreateLogWindow(bool showOnStartup)
        {
            if (logViewer == null || (!(loggerDispatcher.Invoke(new Func<bool>(() => { return logViewer.IsLoaded; })))))
            {
                Thread thread = new Thread(() =>
                {
                    logViewer = new RelhaxLogViewer(ModpackSettings) { SuppressDebugMessages = this.AutomationSettings.SuppressDebugMessagesInLogWindow };

                    logViewer.WindowStartupLocation = WindowStartupLocation.Manual;
                    logViewer.Top = this.Dispatcher.Invoke(new Func<double>(() => { return this.Top; }));
                    logViewer.Left = this.Dispatcher.Invoke(new Func<double>(() => { return this.Left + this.Width + 10; })); 

                    if (showOnStartup)
                        logViewer.Show();

                    loggerDispatcher = Dispatcher.CurrentDispatcher;

                    //start the windows message pump
                    Dispatcher.Run();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                loggerDispatcher?.InvokeAsync(() =>
                {
                    if (logViewer.WindowState == WindowState.Minimized)
                        logViewer.WindowState = WindowState.Normal;
                    logViewer.Focus();
                });
            }
            
        }

        private async void RunSequencesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesToRunListBox.Items.Count == 0)
                return;

            (RunSequencesButton.Content as TextBlock).Text = "Cancel";
            RunSequencesButton.Click -= RunSequencesButton_Click;
            RunSequencesButton.Click += CancelSequencesButton_Click;

            //toggle UI
            ToggleUI(false);

            //clear log file and window
            bool isLogViewerLoaded = false;
            logViewer.Dispatcher.Invoke(() => isLogViewerLoaded = logViewer.IsLoaded);
            if (AutomationSettings.ClearLogWindowOnSequenceStart && logViewer != null && isLogViewerLoaded)
            {
                logViewer.ClearLogWindow();
            }

            if (AutomationSettings.ClearLogFileOnSequenceStart)
            {
                logViewer?.StopLogListener();
                if (!Logging.DisableRedirection(Logfiles.Application, Logfiles.AutomationRunner))
                    throw new BadMemeException("Failed to disable redirection");

                string logfilePath = Logging.GetLogfile(Logfiles.AutomationRunner).Filepath;
                Logging.DisposeLogging(Logfiles.AutomationRunner);
                File.Delete(logfilePath);

                if (!Logging.Init(Logfiles.AutomationRunner, ModpackSettings.VerboseLogging, true, logfilePath))
                    throw new BadMemeException("How did you manage to kill it and not init a new one");

                Logging.WriteHeader(Logfiles.AutomationRunner);
                if (!Logging.RedirectLogOutput(Logfiles.Application, Logfiles.AutomationRunner))
                    throw new BadMemeException("You were just redirecting how can you not do that now");

                //re-subscribe the log event to the log window
                logViewer?.StartLogListener();
            }

            //load database
            Logging.Info("Loading database before sequence run");
            try
            {
                await databaseManager.LoadDatabaseTestAsync(AutomationSettings.DatabaseSavePath);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
            }
            AutomationSequencer.WoTClientVersion = databaseManager.WoTClientVersion;
            AutomationSequencer.WoTModpackOnlineFolderVersion = databaseManager.WoTOnlineFolderVersion;

            //handle the cancel token system
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
            cancellationTokenSource = new CancellationTokenSource();
            AutomationSequencer.CancellationToken = cancellationTokenSource.Token;

            List<AutomationSequence> sequencesToRun = SequencesToRunListBox.Items.Cast<AutomationSequence>().ToList();

            //clear the sequences to run list and replace them with the list box items
            SequencesToRunListBox.Items.Clear();
            foreach (AutomationSequence sequence in sequencesToRun)
            {
                sequence.AutomationComboBoxItem = new AutomationListBoxItem() { AutomationSequence = sequence, Content = sequence };
                SequencesToRunListBox.Items.Add(sequence.AutomationComboBoxItem);
            }

            //add the user macros
            AutomationSequencer.UserMacrosDictionary.Clear();
            AutomationSequencer.UserMacrosDictionary.Add(AutomationSettings.UserMacro1Name, AutomationSettings.UserMacro1Value);
            AutomationSequencer.UserMacrosDictionary.Add(AutomationSettings.UserMacro2Name, AutomationSettings.UserMacro2Value);
            AutomationSequencer.UserMacrosDictionary.Add(AutomationSettings.UserMacro3Name, AutomationSettings.UserMacro3Value);

            SequencerExitCode sequenceRunResult = await AutomationSequencer.RunSequencerAsync(sequencesToRun);

            switch (sequenceRunResult)
            {
                case SequencerExitCode.LoadGlobalMacrosFail:
                case SequencerExitCode.LoadApplicationMacrosFail:
                case SequencerExitCode.LoadLocalMacrosFail:
                case SequencerExitCode.LinkPackagesToAutomationSequencesFail:
                case SequencerExitCode.LoadAutomationSequencesXmlToRunAsyncFail:
                case SequencerExitCode.ParseAutomationSequencesPreRunFail:
                case SequencerExitCode.ResetApplicationMacrosFail:
                    Logging.Info("Sequencer run SETUP ERROR ({0})", sequenceRunResult.ToString());
                    break;

                case SequencerExitCode.Errors:
                    Logging.Info("Sequencer run FAILURE");
                    break;

                case SequencerExitCode.Cancel:
                    Logging.Info("Sequencer run CANCEL");
                    break;

                case SequencerExitCode.NotRun:
                    Logging.Info("Sequencer run NOT RUN");
                    break;

                case SequencerExitCode.NoErrors:
                    Logging.Info("Sequencer run SUCCESS");
                    break;
            }

            if (sequenceRunResult == SequencerExitCode.NoErrors || sequenceRunResult == SequencerExitCode.Errors)
            {
                //save database
                databaseManager.SaveDatabase(AutomationSettings.DatabaseSavePath);
            }

            (RunSequencesButton.Content as TextBlock).Text = "Finish";
            RunSequencesButton.Click -= CancelSequencesButton_Click;
            RunSequencesButton.Click += FinishSequences_Click;

            AutomationTaskProgressBar.Visibility = Visibility.Hidden;
            AutomationTaskProgressBar.Minimum = AutomationTaskProgressBar.Maximum = AutomationTaskProgressBar.Value = 0;
            AutomationTaskProgressTextBlock.Visibility = Visibility.Hidden;
            AutomationTaskProgressTextBlock.Text = string.Empty;
        }

        private void FinishSequences_Click(object sender, RoutedEventArgs e)
        {
            //reset sequence items back to actual sequences, not the listbox
            List<AutomationSequence> sequencesToRun = SequencesToRunListBox.Items.Cast<AutomationListBoxItem>().ToList().Select(seq => seq.AutomationSequence).ToList();
            SequencesToRunListBox.Items.Clear();
            foreach (AutomationSequence sequence in sequencesToRun)
            {
                sequence.AutomationComboBoxItem = null;
                SequencesToRunListBox.Items.Add(sequence);
            }

            (RunSequencesButton.Content as TextBlock).Text = "Run";
            RunSequencesButton.Click -= FinishSequences_Click;
            RunSequencesButton.Click += RunSequencesButton_Click;

            ToggleUI(true);
        }

        private void CancelSequencesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
                AutomationSequencer.CancelSequence();
                Logging.Info("Cancel request sent");
            }
            else
            {
                Logging.Info("Cancel request already sent");
            }
        }

        private void ToggleUI(bool toggle)
        {
            Logging.Debug("The Sequence control UI was toggled: {0}", toggle);
            Control[] controlsToToggle = new Control[]
            {
                SequencesAvailableListBox,
                //SequencesToRunListBox,
                MoveSequencesToRunListButton,
                ReloadSequencesButton,
                CleanWorkingDirectoriesButton,
                OpenHtmlPathSelectorButton,
                MoveUpSelectedSequenceButton,
                MoveDownSelectedSequenceButton,
                RemoveSelectedSequenceButton,
                RemoveAllSequenceButton
            };

            foreach (Control control in controlsToToggle)
            {
                control.IsEnabled = toggle;
            }
        }

        private void InvokeSettingsAndnSaveToSettingsFile()
        {
            foreach (Action action in settingsMethods)
                action();

            SettingsParser parser = new SettingsParser();
            parser.SaveSettings(Settings);
        }

        #region Settings tab events
        private void OpenLogWindowOnStartupSetting_Click(object sender, RoutedEventArgs e)
        {
            AutomationSettings.OpenLogWindowOnStartup = (bool)OpenLogWindowOnStartupSetting.IsChecked;
        }

        private void BigmodsUsernameSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.BigmodsUsername = BigmodsUsernameSetting.Text;
        }

        private void BigmodsPasswordSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.BigmodsPassword = BigmodsPasswordSetting.Text;
        }

        private void DumpParsedMacrosPerSequenceRunSetting_Click(object sender, RoutedEventArgs e)
        {
            AutomationSettings.DumpParsedMacrosPerSequenceRun = (bool)DumpParsedMacrosPerSequenceRunSetting.IsChecked;
        }

        private void DumpEnvironmentVariablesAtSequenceStartSetting_Click(object sender, RoutedEventArgs e)
        {
            AutomationSettings.DumpShellEnvironmentVarsPerSequenceRun = (bool)DumpEnvironmentVariablesAtSequenceStartSetting.IsChecked;
        }

        private void SuppressDebugMessagesSetting_Click(object sender, RoutedEventArgs e)
        {
            AutomationSettings.SuppressDebugMessagesInLogWindow = (bool)SuppressDebugMessagesSetting.IsChecked;
            if (logViewer != null)
                logViewer.SuppressDebugMessages = AutomationSettings.SuppressDebugMessagesInLogWindow;
        }

        private void ClearLogWindowOnSequenceRunSetting_Click(object sender, RoutedEventArgs e)
        {
            AutomationSettings.ClearLogWindowOnSequenceStart = (bool)ClearLogWindowOnSequenceRunSetting.IsChecked;
        }

        private void ClearLogFileOnSequenceRunSetting_Click(object sender, RoutedEventArgs e)
        {
            AutomationSettings.ClearLogFileOnSequenceStart = (bool)ClearLogFileOnSequenceRunSetting.IsChecked;
        }

        private void AutomamtionDatabaseSelectedBranchSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.SelectedBranch = AutomamtionDatabaseSelectedBranchSetting.Text;
        }

        private void SelectDBSaveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDatabaseDialog == null)
                SaveDatabaseDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    //https://stackoverflow.com/questions/5512752/how-to-stop-overwriteprompt-when-creating-savefiledialog-using-getsavefilename
                    OverwritePrompt = false,
                    CheckFileExists = false,
                    DefaultExt = "xml",
                    InitialDirectory = string.IsNullOrWhiteSpace(SelectDBSaveLocationSetting.Text) ? ApplicationConstants.ApplicationStartupPath :
                    Directory.Exists(Path.GetDirectoryName(SelectDBSaveLocationSetting.Text)) ? SelectDBSaveLocationSetting.Text : ApplicationConstants.ApplicationStartupPath,
                    Title = "Select path to save database to. NOTE: It is only selecting path, does not save"
                };

            if (!(bool)SaveDatabaseDialog.ShowDialog())
                return;
            SelectDBSaveLocationSetting.Text = SaveDatabaseDialog.FileName;
        }

        private void SelectDBSaveLocationSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.DatabaseSavePath = SelectDBSaveLocationSetting.Text;
        }

        private async void UseLocalRunnerDatabaseSetting_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)UseLocalRunnerDatabaseSetting.IsChecked && string.IsNullOrEmpty(LocalRunnerDatabaseRootSetting.Text))
            {
                MessageBox.Show("Cannot check this setting when the path to the automation database is not set");
                UseLocalRunnerDatabaseSetting.IsChecked = AutomationSettings.UseLocalRunnerDatabase = false;
                return;
            }
            else if (!File.Exists(LocalRunnerDatabaseRootSetting.Text))
            {
                MessageBox.Show("The currently set path to the automation database root file does not exist");
                return;
            }
            AutomationSettings.UseLocalRunnerDatabase = (bool)UseLocalRunnerDatabaseSetting.IsChecked;

            await LoadAutomationSequencerAsync();
        }

        private void SelectLocalRunnerDatabaseRootButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenRootXmlDialog == null)
                OpenRootXmlDialog = new OpenFileDialog
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    CheckFileExists = true,
                    DefaultExt = "xml",
                    InitialDirectory = string.IsNullOrWhiteSpace(LocalRunnerDatabaseRootSetting.Text) ? ApplicationConstants.ApplicationStartupPath :
                    Directory.Exists(Path.GetDirectoryName(LocalRunnerDatabaseRootSetting.Text)) ? LocalRunnerDatabaseRootSetting.Text : ApplicationConstants.ApplicationStartupPath,
                    Title = "Select the root xml file of the automation repository"
                };

            if (!(bool)OpenRootXmlDialog.ShowDialog())
                return;
            LocalRunnerDatabaseRootSetting.Text = OpenRootXmlDialog.FileName;
        }

        private void LocalRunnerDatabaseRootSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.LocalRunnerDatabaseRoot = LocalRunnerDatabaseRootSetting.Text;
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            InvokeSettingsAndnSaveToSettingsFile();
        }

        private void MainTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem switchToTab = null;
            TabItem switchFromTab = null;

            if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
                switchToTab = e.AddedItems[0] as TabItem;

            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] != null)
                switchFromTab = e.RemovedItems[0] as TabItem;

            if (switchToTab == null || switchFromTab == null)
                return;

            if (switchFromTab == TabItemSettings && switchToTab != TabItemSettings)
                InvokeSettingsAndnSaveToSettingsFile();
        }

        private void SelectWoTInstallLocationButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog manualWoTFind = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "WorldOfTanks.exe|WorldOfTanks.exe",
                Title = Translations.GetTranslatedString("selectWOTExecutable"),
                Multiselect = false,
                ValidateNames = true
            };

            if (!(bool)manualWoTFind.ShowDialog())
                return;

            SelectWoTInstallLocationSetting.Text = manualWoTFind.FileName;
        }

        private void SelectWoTInstallLocationSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.WoTClientInstallLocation = SelectWoTInstallLocationSetting.Text;
        }

        private void UserMacro1Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.UserMacro1Name = UserMacro1Name.Text;
        }

        private void UserMacro1Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.UserMacro1Value = UserMacro1Value.Text;
        }

        private void UserMacro2Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.UserMacro2Name = UserMacro2Name.Text;
        }

        private void UserMacro2Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.UserMacro2Value = UserMacro2Value.Text;
        }

        private void UserMacro3Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.UserMacro3Name = UserMacro3Name.Text;
        }

        private void UserMacro3Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutomationSettings.UserMacro3Value = UserMacro3Value.Text;
        }
        #endregion

        #region Keyboard shortcuts
        private void SequencesToRunListBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
                RemoveSelectedSequenceButton_Click(null, null);
        }
        #endregion
    }
}
