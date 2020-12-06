using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
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

        public List<AutomationTask> AutomationTasks { get; set; } = null;

        public DatabasePackage Package { get; set; } = null;

        public string SequenceDownloadUrl { get; set; } = string.Empty;

        public List<AutomationMacro> LocalMacroList { get; set;} = null;

        private WebClient WebClient = null;

        private XDocument TasksDocument = null;

        private List<AutomationMacro> GlobalMacros { get { return AutomationSequencer.GlobalMacros; } }

        private AutomationRunnerSettings AutomationRunnerSettings { get { return AutomationSequencer.AutomationRunnerSettings; } }

        public string ComponentInternalName { get { return ((IComponentWithID)Package).ComponentInternalName; } }

        public AutomationSequence()
        {
            WebClient = new WebClient();
        }

        public async Task LoadAutomationXmlAsync()
        {
            if (Package == null)
                throw new NullReferenceException();
            if (string.IsNullOrEmpty(SequenceDownloadUrl))
                throw new BadMemeException("SequenceDownloadUrl is not set");

            string xmlString = await WebClient.DownloadStringTaskAsync(SequenceDownloadUrl);
            TasksDocument = XmlUtils.LoadXDocument(xmlString, XmlLoadType.FromString);
        }


        public bool ParseAutomationTasks()
        {
            if (Package == null)
                throw new NullReferenceException();
            if (string.IsNullOrEmpty(SequenceDownloadUrl))
                throw new BadMemeException("SequenceDownloadUrl is not set");

            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Getting list and parsing of automation tasks");
            XPathNavigator result = XmlUtils.GetXNodeFromXpath(TasksDocument, "");
            XElement automationTaskHolder =  XElement.Parse(result.OuterXml);
            PropertyInfo listPropertyInfo = AutomationTasks.GetType().GetProperty(nameof(AutomationTasks));
            CommonUtils.SetListEntries(this, listPropertyInfo, automationTaskHolder.Elements(), AutomationTask.AttributeNameForMapping, AutomationTask.TaskTypeMapper);

            return true;
        }

        public void Dispose()
        {
            ((IDisposable)WebClient).Dispose();
        }
    }
}
