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
        /// <summary>
        /// The default branch to use for the automation's repository root.
        /// </summary>
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

        /// <summary>
        /// If true, the log window will be opened upon automation runner launch.
        /// </summary>
        public bool OpenLogWindowOnStartup { get; set; } = true;

        /// <summary>
        /// The user's FTP account username to the bigmods FTP server
        /// </summary>
        public string BigmodsUsername { get; set; } = string.Empty;

        /// <summary>
        /// The user's FTP account password to the bigmods FTP server
        /// </summary>
        public string BigmodsPassword { get; set; } = string.Empty;

        /// <summary>
        /// The file path to the root modpack database xml file (not the automation database)
        /// </summary>
        public string DatabaseSavePath { get; set; } = string.Empty;

        /// <summary>
        /// If true, all macro variables will be written to the log file when a new sequence is started.
        /// </summary>
        public bool DumpParsedMacrosPerSequenceRun { get; set; } = false;

        /// <summary>
        /// If true, all environment variables will be written to the log file when a new sequence is started.
        /// </summary>
        public bool DumpShellEnvironmentVarsPerSequenceRun { get; set; } = false;

        /// <summary>
        /// If true, use the local automation runner database on disk. If false, use the automation runner repository at the specified branch
        /// </summary>
        public bool UseLocalRunnerDatabase { get; set; } = false;

        /// <summary>
        /// The full path to the automation root xml database file (root.xml).
        /// </summary>
        public string LocalRunnerDatabaseRoot { get; set; } = string.Empty;

        /// <summary>
        /// The full path to the WoT client installation directory.
        /// </summary>
        public string WoTClientInstallLocation { get; set; } = string.Empty;

        /// <summary>
        /// If true, debug level log messages won't be shown in the log window (they will only be written to the log file).
        /// </summary>
        public bool SuppressDebugMessagesInLogWindow { get; set; } = true;

        /// <summary>
        /// If true, the log window text will be cleared upon starting a group of sequences.
        /// </summary>
        public bool ClearLogWindowOnSequenceStart { get; set; } = true;

        /// <summary>
        /// If true, the log file will be cleared upon starting a group of sequences.
        /// </summary>
        public bool ClearLogFileOnSequenceStart { get; set; } = false;

        /// <summary>
        /// The name of user supplied macro 1
        /// </summary>
        public string UserMacro1Name { get; set; } = string.Empty;

        /// <summary>
        /// The value of user supplied macro 1
        /// </summary>
        public string UserMacro1Value { get; set; } = string.Empty;

        /// <summary>
        /// The name of user supplied macro 2
        /// </summary>
        public string UserMacro2Name { get; set; } = string.Empty;

        /// <summary>
        /// The value of user supplied macro 2
        /// </summary>
        public string UserMacro2Value { get; set; } = string.Empty;

        /// <summary>
        /// The name of user supplied macro 3
        /// </summary>
        public string UserMacro3Name { get; set; } = string.Empty;

        /// <summary>
        /// The value of user supplied macro 3
        /// </summary>
        public string UserMacro3Value { get; set; } = string.Empty;
    }
}
