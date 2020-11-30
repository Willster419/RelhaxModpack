using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// An interface definition to specify a window that is a separate sub-function of the application:
    /// It has its own log and settings file.
    /// </summary>
    public interface ICustomFeatureWindow
    {
        /// <summary>
        /// The name of the xml settings file for this window
        /// </summary>
        string SettingsFilename { get; }

        /// <summary>
        /// The name of the logfile for this window
        /// </summary>
        string LogFilename { get; }

        /// <summary>
        /// The command line argument to launch this window
        /// </summary>
        string CommandLineArg { get; }

        /// <summary>
        /// Indicates if this editor instance was launched from the MainWindow or from command line
        /// </summary>
        /// <remarks>This changes the behavior of the logging for the editor</remarks>
        bool LaunchedFromMainWindow { get; set; }

        /// <summary>
        /// The settings definitions class for this window
        /// </summary>
        ISettingsFile Settings { get; }
    }
}
