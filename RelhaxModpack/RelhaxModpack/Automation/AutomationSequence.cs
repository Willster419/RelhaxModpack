using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Windows;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
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

        public List<AutomationMacro> ApplicationMacros { get { return AutomationSequencer.ApplicationMacros; } }

        public List<AutomationMacro> GlobalMacros { get { return AutomationSequencer.GlobalMacros; } }

        public List<AutomationMacro> SequenceMacros { get; } = new List<AutomationMacro>();

        public List<AutomationMacro> AllMacros { get; private set; } = new List<AutomationMacro>();

        public AutomationCompareTracker AutomationCompareTracker { get; protected set; } = new AutomationCompareTracker();

        private WebClient WebClient = null;

        private XDocument TasksDocument = null;

        public AutomationRunnerSettings AutomationRunnerSettings { get { return AutomationSequencer.AutomationRunnerSettings; } }

        public DatabaseManager DatabaseManager { get { return AutomationSequencer.DatabaseManager; } }

        public string ComponentInternalName { get { return ((IComponentWithID)Package).ComponentInternalName; } }

        private string packageName;

        private string packageUID;

        public AutomationSequence()
        {
            WebClient = new WebClient();
        }

        public async Task LoadAutomationXmlAsync()
        {
            if (string.IsNullOrEmpty(SequenceDownloadUrl))
                throw new BadMemeException("SequenceDownloadUrl is not set");

            Logging.Debug(LogOptions.ClassName, "Downloading sequence xml from {0}", SequenceDownloadUrl);
            string xmlString = await WebClient.DownloadStringTaskAsync(SequenceDownloadUrl);
            TasksDocument = XmlUtils.LoadXDocument(xmlString, XmlLoadType.FromString);
        }


        public bool ParseAutomationTasks()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Getting list and parsing of automation tasks");
            Logging.Debug("Getting xml node results of TaskDefinitions");
            XPathNavigator result = XmlUtils.GetXNodeFromXpath(TasksDocument, "/AutomationSequence/TaskDefinitions");
            XElement automationTaskHolder =  XElement.Parse(result.OuterXml);

            Logging.Debug("Getting property of automationTasks and setting list entries");
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
            XPathNavigator result = XmlUtils.GetXNodeFromXpath(TasksDocument, "/AutomationSequence/Macros");
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

            Logging.Debug(Logfiles.AutomationRunner, "Setting up working directory");
            string workingDirectory = Path.Combine(ApplicationConstants.RelhaxTempFolderPath, Package.PackageName);
            if (Directory.Exists(workingDirectory))
            {
                if (!FileUtils.DirectoryDelete(workingDirectory, true, true))
                {
                    Logging.Error(LogOptions.ClassName, "Failed to clear the working directory");
                    return false;
                }
            }
            Directory.CreateDirectory(workingDirectory);

            bool taskReturnsGood = true;
            foreach (AutomationTask task in this.AutomationTasks)
            {
                bool breakLoop = false;
                Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task: {0}", task.ID);
                await task.Execute();

                switch (task.ExitCode)
                {
                    case AutomationExitCode.None:
                        breakLoop = false;
                        taskReturnsGood = true;
                        break;

                    case AutomationExitCode.ComparisonEqualFail:
                        breakLoop = true;
                        taskReturnsGood = true;
                        break;

                    default:
                        Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "The task, '{0}', failed to execute. Check the task error output above for more details. You may want to enable verbose logging.", task.ID);
                        breakLoop = true;
                        taskReturnsGood = false;
                        break;
                }

                if (breakLoop)
                    break;
            }

            //dispose/cleanup the tasks
            AutomationTasks.Clear();
            return taskReturnsGood;
        }

        public void Dispose()
        {
            ((IDisposable)WebClient).Dispose();
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.PackageName, this.PackageUID);
        }
    }
}
