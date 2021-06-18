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

        private string packageUID;

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

        private string packageName;

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

        public List<AutomationMacro> MacrosListForTask { get; } = new List<AutomationMacro>();

        private WebClient WebClient = null;

        private XDocument TasksDocument = null;

        private AutomationRunnerSettings AutomationRunnerSettings { get { return AutomationSequencer.AutomationRunnerSettings; } }

        public string ComponentInternalName { get { return ((IComponentWithID)Package).ComponentInternalName; } }

        public AutomationSequence()
        {
            WebClient = new WebClient();
        }

        public async Task LoadAutomationXmlAsync()
        {
            if (string.IsNullOrEmpty(SequenceDownloadUrl))
                throw new BadMemeException("SequenceDownloadUrl is not set");

            string xmlString = await WebClient.DownloadStringTaskAsync(SequenceDownloadUrl);
            TasksDocument = XmlUtils.LoadXDocument(xmlString, XmlLoadType.FromString);
        }


        public bool ParseAutomationTasks()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Getting list and parsing of automation tasks");
            XPathNavigator result = XmlUtils.GetXNodeFromXpath(TasksDocument, "/AutomationSequence/TaskDefinitions");
            XElement automationTaskHolder =  XElement.Parse(result.OuterXml);
            PropertyInfo listPropertyInfo = this.GetType().GetProperty(nameof(AutomationTasks));
            try
            {
                CommonUtils.SetListEntries(this, listPropertyInfo, automationTaskHolder.Elements(), AutomationTask.AttributeNameForMapping, AutomationTask.TaskTypeMapper);
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
            }

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Finish parsing of tasks");
            return true;
        }

        public async Task<bool> RunTasksAsync()
        {
            if (Package == null || AutomationSequencer == null || AutomationRunnerSettings == null)
                throw new NullReferenceException();

            Logging.Debug(Logfiles.AutomationRunner, "Setting up macro list before task run");
            MacrosListForTask.Clear();
            MacrosListForTask.AddRange(ApplicationMacros);
            foreach (AutomationMacro macro in GlobalMacros)
            {
                MacrosListForTask.Add(AutomationMacro.Copy(macro));
            }

            Logging.Debug(Logfiles.AutomationRunner, "Setting up working directory");
            string workingDirectory = Path.Combine(ApplicationConstants.RelhaxTempFolderPath, Package.PackageName);
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, true);
                Directory.CreateDirectory(workingDirectory);
            }
            else
            {
                Directory.CreateDirectory(workingDirectory);
            }

            foreach (AutomationTask task in this.AutomationTasks)
            {
                Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Running task: {0}", task.ID);
                await task.Execute();
                if (task.ExitCode != 0)
                {
                    Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, "The task, '{0}', failed to execute. Check the task error output above for more details. You may want to enable verbose logging.");
                    return false;
                }
            }
            return true;
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
