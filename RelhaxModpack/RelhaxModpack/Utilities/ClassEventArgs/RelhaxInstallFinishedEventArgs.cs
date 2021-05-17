using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.ClassEventArgs
{
    /// <summary>
    /// Event arguments for when the installer finishes or is ended prematurely
    /// </summary>
    public class RelhaxInstallFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// The exit code from the installer thread
        /// </summary>
        public InstallerExitCodes ExitCode;

        /// <summary>
        /// The error message description
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// Reference to list of parsed categories
        /// </summary>
        public List<Category> ParsedCategoryList;

        /// <summary>
        /// Reference to list of parsed dependencies
        /// </summary>
        public List<Dependency> Dependencies;

        /// <summary>
        /// Reference to list of dependencies
        /// </summary>
        public List<DatabasePackage> GlobalDependencies;

        /// <summary>
        /// A list of all the steps that the installer failed at when returning back to the MainWindow
        /// </summary>
        /// <remarks>The installer creates many threads to complete different parts of the installation.
        /// One of more of these may fail and should be logged when the installer cleanly exists and returns to the MainWindow</remarks>
        public List<InstallerExitCodes> InstallFailedSteps = new List<InstallerExitCodes>();
    }
}
