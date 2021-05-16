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

        public LoggingMessageWrite LogMessageWrite = null;

        /// <summary>
        /// Create an instance of the DatabaseAutomationRunner window
        /// </summary>
        public DatabaseAutomationRunner(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
            DownloadProgressChanged = WebClient_DownloadProgressChanged;
            Settings = AutomationSettings;
            LogMessageWrite = OnLogMessageWrite;
        }


        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            
        }

        private void OnLogMessageWrite(object sender, LogMessageEventArgs e)
        {
            
        }

        ~DatabaseAutomationRunner()
        {
            DownloadProgressChanged = null;
            LogMessageWrite = null;
        }
    }
}
