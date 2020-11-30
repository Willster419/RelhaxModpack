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

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseAutomationRunner.xaml
    /// </summary>
    public partial class DatabaseAutomationRunner : RelhaxWindow, ICustomFeatureWindow
    {
        /// <summary>
        /// The command line argument specified at application launch to show this window
        /// </summary>
        public const string DatabaseAutomationRunnerCommandLineArg = "automation-runner";

        /// <summary>
        /// Create an instance of the DatabaseAutomationRunner window
        /// </summary>
        public DatabaseAutomationRunner()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The name of the xml settings file for the DatabaseAutomationRunner window
        /// </summary>
        public string SettingsFilename { get; } = RelhaxModpack.Settings.AutomationRunnerSettingsFilename;

        /// <summary>
        /// The name of the logfile for the DatabaseAutomationRunner window
        /// </summary>
        public string LogFilename { get; } = Logging.AutomationLogFilename;

        /// <summary>
        /// Indicates if this editor instance was launched from the MainWindow or from command line
        /// </summary>
        /// <remarks>This changes the behavior of the logging for the editor</remarks>
        public bool LaunchedFromMainWindow { get; set; } = false;

        /// <summary>
        /// The command line argument specified at application launch to show this window
        /// </summary>
        public string CommandLineArg { get; } = DatabaseAutomationRunnerCommandLineArg;

        /// <summary>
        /// The settings definitions class for this window
        /// </summary>
        public ISettingsFile Settings { get { return this.AutomationSettings; } }

        private AutomationRunnerSettings AutomationSettings = null;

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {

        }
    }
}
