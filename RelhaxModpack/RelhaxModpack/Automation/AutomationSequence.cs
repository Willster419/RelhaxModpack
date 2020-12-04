using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// An AutomationSequence class is a container to store a list of Automation tasks that pertain to a package.
    /// </summary>
    public class AutomationSequence : IDisposable
    {
        public AutomationSequencer AutomationSequencer { get; set; } = null;

        public List<AutomationTask> AutomationTasks { get; set; } = null;

        public DatabasePackage Package { get; set; } = null;

        public string SequenceDownloadUrl { get; set; } = string.Empty;

        private WebClient WebClient = null;

        private List<AutomationMacro> LocalMacroList = null;

        private XmlDocument TasksDocument = null;

        private List<AutomationMacro> GlobalMacros { get { return AutomationSequencer.GlobalMacros; } }

        private AutomationRunnerSettings AutomationRunnerSettings { get { return AutomationSequencer.AutomationRunnerSettings; } }

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
            TasksDocument = XmlUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
        }


        public bool ParseAutomationTasks()
        {
            if (Package == null)
                throw new NullReferenceException();
            if (string.IsNullOrEmpty(SequenceDownloadUrl))
                throw new BadMemeException("SequenceDownloadUrl is not set");



            return true;
        }

        public void Dispose()
        {
            ((IDisposable)WebClient).Dispose();
        }
    }
}
