using RelhaxModpack.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using RelhaxModpack;
using RelhaxModpack.Utilities.ClassEventArgs;
using RelhaxModpack.Automation;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;

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

        private AutomationRunnerSettings AutomationSettings = new AutomationRunnerSettings();

        private AutomationSequencer AutomationSequencer = null;

        private RelhaxLogViewer logViewer;

        private HtmlPathSelector htmlPathSelector;

        private DatabaseManager databaseManager;

        /// <summary>
        /// Create an instance of the DatabaseAutomationRunner window
        /// </summary>
        public DatabaseAutomationRunner(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
            DownloadProgressChanged = WebClient_DownloadProgressChanged;
            Settings = AutomationSettings;
            databaseManager = new DatabaseManager(ModpackSettings, CommandLineSettings);
            AutomationSequencer = new AutomationSequencer() { AutomationRunnerSettings = this.AutomationSettings, DatabaseAutomationRunner = this, DatabaseManager = databaseManager};
        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //init the log viewer window
            logViewer = new RelhaxLogViewer(ModpackSettings)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Top = this.Top,
                Left = this.Left + this.Width + 10
            };

            if (AutomationSettings.OpenLogWindowOnStartup)
                logViewer.Show();

            //load branches from the server
            Logging.Info("Loading branches");
            await AutomationSequencer.LoadBranchesListAsync();

            //ensure that the branch specified in settings exists, and if so apply it. else apply the default setting
            if (!AutomationSequencer.AutomationBranches.Contains(AutomationSettings.SelectedBranch))
            {
                Logging.Error("The selected branch does not exist on the server: {0}", AutomationSettings.SelectedBranch);
                //TODO: command line mode should be an exit return
            }
            else
            {
                Logging.Info("Applying branch to load from: {0}", AutomationSettings.SelectedBranch);
            }

            //load the available package sequences from the root document
            SequencesAvailableListBox.Items.Clear();
            SequencesAvailableListBox.Items.Add("Loading available sequences from server...");
            Logging.Info("Loading sequences from root document");
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

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            
        }

        private void MainTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MoveSequencesToRunListButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesAvailableListBox.SelectedItem == null)
                return;

            if (SequencesToRunListBox.Items.Contains(SequencesAvailableListBox.SelectedItem))
                return;

            SequencesToRunListBox.Items.Add(SequencesAvailableListBox.SelectedItem);
        }

        private void MoveUpSelectedSequenceButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoveDownSelectedSequenceButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteSelectedSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequencesToRunListBox.SelectedItem == null)
                return;

            SequencesToRunListBox.Items.Remove(SequencesToRunListBox.SelectedItem);
        }

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

        private async void RunSequencesButton_Click(object sender, RoutedEventArgs e)
        {
            RunSequencesButton.IsEnabled = false;
            //load database
            Logging.Info("Loading database");
            await databaseManager.LoadDatabaseAsync();
            AutomationSequencer.WoTClientVersion = databaseManager.WoTClientVersion;
            AutomationSequencer.WoTModpackOnlineFolderVersion = databaseManager.WoTOnlineFolderVersion;

            Logging.Info(LogOptions.MethodName, "Invoking the sequencer");
            List<AutomationSequence> sequencesToRun = SequencesToRunListBox.Items.Cast<AutomationSequence>().ToList();
            if (await AutomationSequencer.RunSequencerAsync(sequencesToRun))
            {
                Logging.Info("Sequencer run SUCCESS");
            }
            else
            {
                Logging.Info("Sequencer run FAILURE");
            }
            RunSequencesButton.IsEnabled = true;
        }
    }
}
