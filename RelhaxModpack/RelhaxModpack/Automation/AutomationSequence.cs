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

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// An AutomationSequence class is a container to store a list of Automation tasks that pertain to a package.
    /// </summary>
    public class AutomationSequence : IDisposable, IComponentWithID
    {
        public const string AutomationSequenceMacroDefinitionsXpath = "/AutomationSequence/Macros";

        public const string AutomationSequenceTaskDefinitionsXpath = "/AutomationSequence/TaskDefinitions";

        public AutomationSequencer AutomationSequencer { get; set; } = null;

        public List<AutomationTask> AutomationTasks { get; } = new List<AutomationTask>();

        public bool PackageLoaded { get { return Package != null; } }

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
                if (PackageLoaded)
                    Package.UID = value;
                else
                    packageUID = value;
            }
        }

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
                if (PackageLoaded)
                    Package.PackageName = value;
                else
                    packageName = value;
            }
        }

        public DatabasePackage Package { get; set; } = null;

        public string SequenceDownloadUrl { get; set; } = string.Empty;

        public DatabaseAutomationRunner DatabaseAutomationRunner { get { return AutomationSequencer?.DatabaseAutomationRunner; } }

        public List<AutomationMacro> SequenceMacros { get; } = new List<AutomationMacro>();

        public List<AutomationMacro> AllMacros { get; private set; } = new List<AutomationMacro>();

        public AutomationCompareTracker AutomationCompareTracker { get; protected set; } = new AutomationCompareTracker();

        public SequencerExitCode ExitCode { get; set; } = SequencerExitCode.NotRun;

        private WebClient WebClient = null;

        private XDocument TasksDocument = null;

        public string ComponentInternalName { get { return ((IComponentWithID)Package).ComponentInternalName; } }

        private string packageName;

        private string packageUID;

        private Stopwatch ExecutionTimeStopwatch = new Stopwatch();

        public List<DatabasePackage> DatabasePackages { get; set; }

        public List<AutomationMacro> ApplicationMacros { get; private set; }

        public List<AutomationMacro> GlobalMacros { get; private set; }

        public AutomationRunnerSettings AutomationRunnerSettings { get; private set; }

        public DatabaseManager DatabaseManager { get; private set; }

        private CancellationToken CancellationToken;

        private AutomationTask RunningTask;

        public AutomationSequence(List<DatabasePackage> databasePackages, List<AutomationMacro> applicationMacros, List<AutomationMacro> globalMacros, AutomationRunnerSettings automationRunnerSettings, DatabaseManager databaseManager, CancellationToken cancellationToken)
        {
            WebClient = new WebClient();
            DatabasePackages = databasePackages;
            ApplicationMacros = applicationMacros;
            GlobalMacros = globalMacros;
            AutomationRunnerSettings = automationRunnerSettings;
            DatabaseManager = databaseManager;
            CancellationToken = cancellationToken;
        }

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
                task.PreProcessingHook();
                Logging.Info("Processed task {0}", task.ID);
            }

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Finish parsing of tasks");
            return true;
        }

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

        public async Task<bool> RunTasksAsync()
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

            //process local macros in case they were added with macros inside them
            foreach (AutomationMacro macro in SequenceMacros)
            {
                macro.Value = AutomationTask.ProcessMacro(macro.Name, macro.Value, SequenceMacros);
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
                    return false;
                }
            }
            Directory.CreateDirectory(workingDirectory);

            bool taskReturnGood = true;
            foreach (AutomationTask task in this.AutomationTasks)
            {
                RunningTask = task;
                bool breakLoop = false;
                Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task: {0}", task.ID);
                try
                {
                    if (DatabaseAutomationRunner != null && DatabaseAutomationRunner.HighPriorityLogViewer != null)
                        DatabaseAutomationRunner.HighPriorityLogViewer = false;
                    await task.Execute();
                    if (DatabaseAutomationRunner != null && DatabaseAutomationRunner.HighPriorityLogViewer != null)
                        DatabaseAutomationRunner.HighPriorityLogViewer = true;
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
                        taskReturnGood = true;
                        ExitCode = SequencerExitCode.NoTaskErrors;
                        break;

                    case AutomationExitCode.Cancel:
                        breakLoop = true;
                        taskReturnGood = true;
                        ExitCode = SequencerExitCode.Cancel;
                        break;

                    case AutomationExitCode.ComparisonNoFilesToUpdate:
                        breakLoop = true;
                        taskReturnGood = true;
                        ExitCode = SequencerExitCode.NoTaskErrors;
                        break;

                    case AutomationExitCode.ComparisonManualFilesToUpdate:
                        breakLoop = true;
                        taskReturnGood = true;
                        ExitCode = SequencerExitCode.NoTaskErrors;
                        break;

                    default:
                        Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "The task, '{0}', failed to execute. Check the task error output above for more details. You may want to enable verbose logging.", task.ID);
                        breakLoop = true;
                        taskReturnGood = false;
                        ExitCode = SequencerExitCode.TaskErrors;
                        break;
                }

                if (CancellationToken.IsCancellationRequested)
                {
                    breakLoop = true;
                    taskReturnGood = true;
                    ExitCode = SequencerExitCode.Cancel;
                }

                if (breakLoop)
                    break;
            }

            //dispose/cleanup the tasks
            AutomationTasks.Clear();
            RunningTask = null;
            Logging.Info("Sequence {0} completed in {1} ms", PackageName, ExecutionTimeStopwatch.ElapsedMilliseconds);
            Dispose();
            return taskReturnGood;
        }

        public void Dispose()
        {
            ((IDisposable)WebClient).Dispose();
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.PackageName, this.PackageUID);
        }

        public void CancelTask()
        {
            if (RunningTask != null && RunningTask is ICancelOperation taskThatCanCancel)
                taskThatCanCancel.Cancel();
        }
    }
}
