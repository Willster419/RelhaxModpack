using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// Defines settings used in the database automation runner window
    /// </summary>
    public class AutomationRunnerSettings : ISettingsFile
    {
        public const string RepoDefaultBranch = "master";

        /// <summary>
        /// The name of the xml file on disk
        /// </summary>
        public string Filename { get; } = "AutomationRunnerSettings.xml";

        /// <summary>
        /// A list of properties and fields to exclude from saving/loading to and from xml
        /// </summary>
        public string[] MembersToExclude { get { return new string[] { nameof(MembersToExclude), nameof(Filename), nameof(RepoDefaultBranch) }; } }

        /// <summary>
        /// The name of the branch on github that the user specifies to download the automation scripts from
        /// </summary>
        public string SelectedBranch { get; set; } = "master";

        public bool OpenLogWindowOnStartup { get; set; } = true;

        public string BigmodsUsername { get; set; } = string.Empty;

        public string BigmodsPassword { get; set; } = string.Empty;

        public string DatabaseSavePath { get; set; } = string.Empty;

        /// <summary>
        /// Toggle to dump the parsed macros to the log file before every sequence run
        /// </summary>
        public bool DumpParsedMacrosPerSequenceRun { get; set; } = false;

        public bool DumpShellEnvironmentVarsPerSequenceRun { get; set; } = false;

        public bool UseLocalRunnerDatabase { get; set; } = false;

        public string LocalRunnerDatabaseRoot { get; set; } = string.Empty;

        public string WoTClientInstallLocation { get; set; } = string.Empty;

        public AutomationRunMode AutomationRunMode { get; set; } = AutomationRunMode.Batch;

        public bool SuppressDebugMessagesInLogWindow { get; set; } = true;
    }
}
