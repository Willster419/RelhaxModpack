using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Windows;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using RelhaxModpack.Automation.Tasks;
using RelhaxModpack.UI;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// An AutomationSequence is a container to store a list of Automation tasks for updating a database package.
    /// </summary>
    public class AutomationSequence : IDisposable, IComponentWithID
    {
        /// <summary>
        /// The Xpath in the automation sequence xml document to get the list of macros.
        /// </summary>
        public const string AutomationSequenceMacroDefinitionsXpath = "/AutomationSequence/Macros";

        /// <summary>
        /// The Xpath in the automation sequence xml document to get the list of tasks.
        /// </summary>
        public const string AutomationSequenceTaskDefinitionsXpath = "/AutomationSequence/TaskDefinitions";

        /// <summary>
        /// Get or set the AutomationSequencer instance.
        /// </summary>
        public AutomationSequencer AutomationSequencer { get; set; } = null;

        /// <summary>
        /// Get the list of tasks to run for this automation sequence.
        /// </summary>
        public List<AutomationTask> AutomationTasks { get; } = new List<AutomationTask>();

        /// <summary>
        /// Get or set the automation ComboBox UI element for this sequence.
        /// </summary>
        public AutomationListBoxItem AutomationComboBoxItem { get; set; }

        /// <summary>
        /// Gets if the database package instance has been loaded into this sequence.
        /// </summary>
        public bool PackageLoaded { get { return Package != null; } }

        /// <summary>
        /// Get or set the UID of the package of this sequence. If the package is not loaded, it is stored internally.
        /// </summary>
        /// <remarks>When loaded, the UID internal value is used to link the sequence to the package upon parsing.</remarks>
        public string PackageUID
        {
            get
            {
                if (PackageLoaded)
                    return Package.UID;
                else
                    return packageUID;
            }
            set
            {
                if (!PackageLoaded)
                    packageUID = value;
            }
        }

        /// <summary>
        /// Get of set the PackageName of the package of this sequence. If the package is not loaded, it is stored internally.
        /// </summary>
        public string PackageName
        {
            get
            {
                if (PackageLoaded)
                    return Package.PackageName;
                else
                    return packageName;
            }
            set
            {
                if (!PackageLoaded)
                    packageName = value;
            }
        }

        /// <summary>
        /// Get or set the database package that this sequence is written for.
        /// </summary>
        public DatabasePackage Package { get; set; } = null;

        /// <summary>
        /// Get or set the direct link from repository to download this sequence's xml document.
        /// </summary>
        /// <remarks>This is used when the "use local repository on disk" is unchecked.</remarks>
        public string SequenceDownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Get the database automation runner window instance.
        /// </summary>
        public DatabaseAutomationRunner DatabaseAutomationRunner { get { return AutomationSequencer?.DatabaseAutomationRunner; } }

        /// <summary>
        /// Get the list of macros loaded from the sequence xml document.
        /// </summary>
        public List<AutomationMacro> SequenceMacros { get; } = new List<AutomationMacro>();

        /// <summary>
        /// Gets a list combining all the macros into one list.
        /// </summary>
        public List<AutomationMacro> AllMacros { get; private set; } = new List<AutomationMacro>();

        /// <summary>
        /// Get the automation compare manager.
        /// </summary>
        public AutomationCompareManager AutomationCompareManager { get; protected set; } = new AutomationCompareManager();

        /// <summary>
        /// Get the automation browser session manager.
        /// </summary>
        public BrowserSessionManager BrowserSessionManager { get; protected set; }

        /// <summary>
        /// Gets or sets the exit code from run of tasks.
        /// </summary>
        public SequencerExitCode ExitCode { get; set; } = SequencerExitCode.NotRun;

        private WebClient WebClient = null;

        private XDocument TasksDocument = null;

        /// <summary>
        /// Get the internal name of the loaded database package.
        /// </summary>
        public string ComponentInternalName { get { return ((IComponentWithID)Package).ComponentInternalName; } }

        private string packageName;

        private string packageUID;

        private Stopwatch ExecutionTimeStopwatch = new Stopwatch();

        /// <summary>
        /// Get or set the list of parsed database packages.
        /// </summary>
        public List<DatabasePackage> DatabasePackages { get; set; }

        /// <summary>
        /// Gets the list of application macros.
        /// </summary>
        public List<AutomationMacro> ApplicationMacros { get; private set; }

        /// <summary>
        /// Gets the list of global macros.
        /// </summary>
        public List<AutomationMacro> GlobalMacros { get; private set; }

        /// <summary>
        /// Gets the list of user macros.
        /// </summary>
        public List<AutomationMacro> UserMacros { get; private set; }

        /// <summary>
        /// Gets the automation runner settings instance.
        /// </summary>
        public AutomationRunnerSettings AutomationRunnerSettings { get; private set; }

        /// <summary>
        /// Gets the database manager instance.
        /// </summary>
        public DatabaseManager DatabaseManager { get; private set; }

        private CancellationToken CancellationToken;

        private AutomationTask RunningTask;

        /// <summary>
        /// Create an instance of the AutomationSequence class.
        /// </summary>
        /// <param name="databasePackages">The list of parsed database packages.</param>
        /// <param name="applicationMacros">The list of parsed application macros.</param>
        /// <param name="globalMacros">The list of parsed global macros.</param>
        /// <param name="userMacros">The list of parsed user macros.</param>
        /// <param name="automationRunnerSettings">The automation runner settings instance.</param>
        /// <param name="databaseManager">The database manager instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public AutomationSequence(List<DatabasePackage> databasePackages, List<AutomationMacro> applicationMacros, List<AutomationMacro> globalMacros,
            List<AutomationMacro> userMacros, AutomationRunnerSettings automationRunnerSettings, DatabaseManager databaseManager, CancellationToken cancellationToken)
        {
            WebClient = new WebClient();
            DatabasePackages = databasePackages;
            ApplicationMacros = applicationMacros;
            GlobalMacros = globalMacros;
            AutomationRunnerSettings = automationRunnerSettings;
            DatabaseManager = databaseManager;
            CancellationToken = cancellationToken;
            UserMacros = userMacros;
        }

        /// <summary>
        /// Load the automation sequence xml document info the XDocument instance.
        /// </summary>
        /// <returns>True if the document was loaded from xml, false otherwise.</returns>
        public async Task<bool> LoadAutomationXmlAsync()
        {
            if (string.IsNullOrEmpty(SequenceDownloadUrl))
                throw new BadMemeException("SequenceDownloadUrl is not set");

            Logging.Debug(LogOptions.ClassName, "Downloading sequence xml from {0}", SequenceDownloadUrl);
            if (AutomationRunnerSettings.UseLocalRunnerDatabase)
            {
                TasksDocument = XmlUtils.LoadXDocument(SequenceDownloadUrl, XmlLoadType.FromFile);
            }
            else
            {
                string xmlString = await WebClient.DownloadStringTaskAsync(SequenceDownloadUrl);
                TasksDocument = XmlUtils.LoadXDocument(xmlString, XmlLoadType.FromString);
            }

            if (TasksDocument == null)
            {
                Logging.Error("The xml document failed to download or load from disk");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse the automation xml document into the list of tasks.
        /// </summary>
        /// <returns>True if the parse operation was successful, false otherwise.</returns>
        public bool ParseAutomationTasks()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Getting list and parsing of automation tasks");
            Logging.Debug("Getting xml node results of TaskDefinitions");
            XPathNavigator result = XmlUtils.GetXNodeFromXpath(TasksDocument, AutomationSequenceTaskDefinitionsXpath);
            if (result == null)
            {
                Logging.Error("The xml document was valid xml and loaded, but has incorrect task definition formatting for the application (missing the TaskDefinitions root?)");
                return false;
            }

            XElement automationTaskHolder =  XElement.Parse(result.OuterXml);

            Logging.Debug("Getting property of automationTasks and setting list entries");
            AutomationTasks.Clear();
            PropertyInfo listPropertyInfo = this.GetType().GetProperty(nameof(AutomationTasks));
            try
            {
                bool setListEntriesResult = CommonUtils.SetListEntries(this, listPropertyInfo, automationTaskHolder.Elements(), AutomationTask.AttributeNameForMapping, AutomationTask.TaskTypeMapper);
                if (!setListEntriesResult)
                    return false;
            }
            catch (Exception ex)
            {
                Logging.AutomationRunner(ex.ToString(), LogLevel.Exception);
                return false;
            }

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Configuration of any additional properties for each task");
            foreach (AutomationTask task in AutomationTasks)
            {
                task.AutomationSequence = this;
                Logging.Debug("Processed task {0}", task.ID);
            }

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Finish parsing of tasks");
            return true;
        }

        /// <summary>
        /// Parse the automation xml document into the list of sequence macros.
        /// </summary>
        /// <returns>True if the parse operation was successful, false otherwise.</returns>
        public bool ParseSequenceMacros()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Parsing defined macros inside the sequence");
            XPathNavigator result = XmlUtils.GetXNodeFromXpath(TasksDocument, AutomationSequenceMacroDefinitionsXpath);
            XElement macroHolder = XElement.Parse(result.OuterXml);
            SequenceMacros.Clear();
            PropertyInfo listPropertyInfo = this.GetType().GetProperty(nameof(SequenceMacros));
            try
            {
                bool setListEntriesResult = CommonUtils.SetListEntries(this, listPropertyInfo, macroHolder.Elements());
                if (!setListEntriesResult)
                    return false;
            }
            catch (Exception ex)
            {
                Logging.AutomationRunner(ex.ToString(), LogLevel.Exception);
                return false;
            }
            foreach (AutomationMacro macro in SequenceMacros)
            {
                macro.MacroType = MacroType.Local;
            }
            return true;
        }

        /// <summary>
        /// Run the tasks of this sequence.
        /// </summary>
        public async Task RunTasksAsync()
        {
            ExecutionTimeStopwatch.Restart();
            RunningTask = null;
            ExitCode = SequencerExitCode.NotRun;
            if (Package == null || AutomationSequencer == null || AutomationRunnerSettings == null)
                throw new NullReferenceException();

            Logging.Debug(Logfiles.AutomationRunner, "Setting up macro list before task run");
            AllMacros.Clear();
            AllMacros.AddRange(ApplicationMacros);
            foreach (AutomationMacro macro in GlobalMacros)
            {
                AllMacros.Add(AutomationMacro.Copy(macro));
            }
            foreach (AutomationMacro macro in SequenceMacros)
            {
                AllMacros.Add(AutomationMacro.Copy(macro));
            }
            foreach (AutomationMacro macro in UserMacros)
            {
                AllMacros.Add(AutomationMacro.Copy(macro));
            }

            Logging.Debug(Logfiles.AutomationRunner, "Setting up working directory");
            string workingDirectory = Path.Combine(ApplicationConstants.RelhaxTempFolderPath, Package.PackageName);
            if (Directory.Exists(workingDirectory))
            {
                Logging.Debug("Directory exits, delete");
                if (!await FileUtils.DirectoryDeleteAsync(workingDirectory, true, true))
                {
                    Logging.Error(LogOptions.ClassName, "Failed to clear the working directory");
                    return;
                }
            }
            Directory.CreateDirectory(workingDirectory);

            bool updatePackageLastCheck = false;
            for (int index = 0; index < AutomationTasks.Count; index++)
            {
                AutomationTask task = AutomationTasks[index];
                RunningTask = task;
                bool breakLoop = false;
                Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task: {0}", task.ID);
                try
                {
                    await task.Execute();
                }
                catch (Exception ex)
                {
                    if (task is ICancelOperation taskThatNeedsCancel)
                        taskThatNeedsCancel.Cancel();

                    if (!(ex is OperationCanceledException))
                        Logging.Exception(ex.ToString());
                }

                switch (task.ExitCode)
                {
                    case AutomationExitCode.None:
                        breakLoop = false;
                        updatePackageLastCheck = true;
                        ExitCode = SequencerExitCode.NoErrors;
                        break;

                    case AutomationExitCode.Cancel:
                        breakLoop = true;
                        updatePackageLastCheck = false;
                        ExitCode = SequencerExitCode.Cancel;
                        break;

                    case AutomationExitCode.ComparisonNoFilesToUpdate:
                        breakLoop = true;
                        updatePackageLastCheck = true;
                        ExitCode = SequencerExitCode.NoErrors;
                        break;

                    case AutomationExitCode.ComparisonManualFilesToUpdate:
                        breakLoop = true;
                        updatePackageLastCheck = false;
                        ExitCode = SequencerExitCode.Errors;
                        break;

                    default:
                        Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "The task, '{0}', failed to execute. Check the task error output above for more details. You may want to enable verbose logging.", task.ID);
                        breakLoop = true;
                        updatePackageLastCheck = false;
                        ExitCode = SequencerExitCode.Errors;
                        break;
                }

                if (CancellationToken.IsCancellationRequested)
                {
                    breakLoop = true;
                    ExitCode = SequencerExitCode.Cancel;
                }

                if (breakLoop)
                    break;
            }

            //if it completed as updated or no update available, then mark the package as checked for latest updates
            if (updatePackageLastCheck)
                Package.LastUpdateCheck = CommonUtils.GetCurrentUniversalFiletimeTimestamp();

            //dispose/cleanup the tasks
            AutomationTasks.Clear();
            RunningTask = null;
            Logging.Info("Sequence {0} completed in {1} ms", PackageName, ExecutionTimeStopwatch.ElapsedMilliseconds);
            Dispose();
            return;
        }

        /// <summary>
        /// Updates the DatabasePackages property with the loaded packages from the database manager.
        /// </summary>
        public void UpdateDatabasePackageList()
        {
            DatabasePackages = DatabaseManager.GetFlatList();
        }

        /// <summary>
        /// Update the database package instance with the instance from the DatabasePackages list.
        /// </summary>
        public void UpdateCurrentDatabasePackage()
        {
            string oldPackageUID = Package.UID;
            Package = DatabasePackages.Find(pack => pack.UID.Equals(oldPackageUID));
            if (Package == null)
                throw new BadMemeException("If you're reading this, then it's too late.");
        }

        /// <summary>
        /// Reset the browser session manager (creates a new instance).
        /// </summary>
        /// <param name="type">The type of browser implementation to use.</param>
        public void ResetBrowserSessionManager(BrowserSessionType type)
        {
            ClearBrowserSessionManager();
            BrowserSessionManager = new BrowserSessionManager(type);
        }

        /// <summary>
        /// Clear the browser session manager for a new browser session.
        /// </summary>
        public void ClearBrowserSessionManager()
        {
            if (BrowserSessionManager != null)
            {
                BrowserSessionManager.Dispose();
                BrowserSessionManager = null;
            }
        }

        /// <summary>
        /// Dispose of the unmanaged resources used by the sequence.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)WebClient).Dispose();
        }

        /// <summary>
        /// Returns a string representation of this sequence object.
        /// </summary>
        /// <returns>A string of the package name and UID for this sequence.</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}", this.PackageName, this.PackageUID);
        }

        /// <summary>
        /// Cancels the currently running task.
        /// </summary>
        public void CancelTask()
        {
            if (RunningTask != null && RunningTask is ICancelOperation taskThatCanCancel)
                taskThatCanCancel.Cancel();
        }
    }
}
