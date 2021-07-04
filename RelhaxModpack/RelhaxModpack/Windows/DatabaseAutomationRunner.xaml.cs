﻿using RelhaxModpack.Settings;
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

        public DownloadProgressChangedEventHandler DownloadProgressChanged = null;

        public ProgressChangedEventHandler ProgressChanged = null;

        public DownloadDataCompletedEventHandler DownloadDataCompleted = null;

        public AsyncCompletedEventHandler DownloadFileCompleted = null;

        public UploadProgressChangedEventHandler UploadProgressChanged = null;

        public UploadFileCompletedEventHandler UploadFileCompleted = null;

        public EventHandler<RelhaxProgress> RelhaxProgressChanged = null;

        private AutomationRunnerSettings AutomationSettings = new AutomationRunnerSettings();

        private AutomationSequencer AutomationSequencer = null;

        private RelhaxLogViewer logViewer;

        private HtmlPathSelector htmlPathSelector;

        private DatabaseManager databaseManager;

        private SaveFileDialog SaveDatabaseDialog;

        private OpenFileDialog OpenRootXmlDialog;

        /// <summary>
        /// Create an instance of the DatabaseAutomationRunner window
        /// </summary>
        public DatabaseAutomationRunner(ModpackSettings modpackSettings) : base(modpackSettings)
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
        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);
            AutomationSequencer = new AutomationSequencer() { AutomationRunnerSettings = this.AutomationSettings, DatabaseAutomationRunner = this, DatabaseManager = databaseManager };

            LoadSettingsToUI();

            //init the log viewer window
            logViewer = new RelhaxLogViewer(ModpackSettings)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Top = this.Top,
                Left = this.Left + this.Width + 10
            };

            if (AutomationSettings.OpenLogWindowOnStartup)
                logViewer.Show();

            await LoadAutomationSequencerAsync();
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
            SequencesAvailableListBox.Items.Add("Loading available sequences from database...");
            Logging.Info("Loading sequences from root document");
            AutomationSequencer.AutomationSequences.Clear();
            await AutomationSequencer.LoadRootDocumentAsync();

            //load the available global macros
            Logging.Info("Loading global macros");
            await AutomationSequencer.LoadGlobalMacrosAsync();

            //parse the document now that it's loaded
            Logging.Info("Parsing sequences from root document");
            await AutomationSequencer.ParseRootDocumentAsync();

            //load the sequences into the listbox view
            SequencesAvailableListBox.Items.Clear();
            foreach (AutomationSequence sequence in AutomationSequencer.AutomationSequences)
            {
                SequencesAvailableListBox.Items.Add(sequence);
            }
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            DownloadProgressChanged = null;
            if (!logViewer.ViewerClosed)
                logViewer.Close();
            if (htmlPathSelector != null && htmlPathSelector.IsLoaded)
                htmlPathSelector.Close();
            AutomationSequencer.Dispose();
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
                ShowAutomationProgress(e.TotalBytesToReceive);
            }
            AutomationTaskProgressBar.Value = e.BytesReceived;
            AutomationTaskProgressTextBlock.Text = string.Format("{0} of {1}", e.BytesReceived, e.TotalBytesToReceive);
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
        #endregion

        #region Sequence move around buttons
        public struct LocationTracker
        {
            public AutomationSequence Sequence;
            public int OldIndex;
        }
        private void MoveSequencesToRunListButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesAvailableListBox.SelectedItems.Count == 0)
                return;

            foreach (AutomationSequence sequence in SequencesAvailableListBox.SelectedItems)
            {
                if (!SequencesToRunListBox.Items.Contains(sequence))
                    SequencesToRunListBox.Items.Add(sequence);
            }
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

        private void DeleteSelectedSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesToRunListBox.SelectedItems.Count == 0)
                return;

            List<AutomationSequence> itemsToRemove = SequencesToRunListBox.SelectedItems.Cast<AutomationSequence>().ToList();
            foreach (AutomationSequence sequence in itemsToRemove)
                SequencesToRunListBox.Items.Remove(sequence);
        }
        #endregion

        #region Open window buttons
        private void OpenLogfileViewerButton_Click(object sender, RoutedEventArgs e)
        {
            if (logViewer.ViewerClosed)
            {
                logViewer = null;
                logViewer = new RelhaxLogViewer(ModpackSettings)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Top = this.Top,
                    Left = this.Left + this.Width + 10
                };
                logViewer.Show();
            }
            else
            {
                logViewer.Focus();
            }
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
        #endregion

        private async void RunSequencesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesToRunListBox.Items.Count == 0)
                return;

            RunSequencesButton.IsEnabled = false;
            //load database
            Logging.Info("Loading database");
            await databaseManager.LoadDatabaseTestAsync(AutomationSettings.DatabaseSavePath);
            AutomationSequencer.WoTClientVersion = databaseManager.WoTClientVersion;
            AutomationSequencer.WoTModpackOnlineFolderVersion = databaseManager.WoTOnlineFolderVersion;

            Logging.Info(LogOptions.MethodName, "Invoking the sequencer");
            List<AutomationSequence> sequencesToRun = SequencesToRunListBox.Items.Cast<AutomationSequence>().ToList();
            if (await AutomationSequencer.RunSequencerAsync(sequencesToRun))
            {
                Logging.Info("Sequencer run SUCCESS, saving database");
                databaseManager.SaveDatabase(AutomationSettings.DatabaseSavePath);
            }
            else
            {
                Logging.Info("Sequencer run FAILURE");
            }
            RunSequencesButton.IsEnabled = true;
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
        #endregion
    }
}
