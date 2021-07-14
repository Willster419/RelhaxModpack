using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack.Automation
{
    public abstract class ImportTask : AutomationTask, IXmlSerializable, ICancelOperation
    {
        public string RepoUrlPath { get; set; } = string.Empty;

        protected abstract string XpathExpression { get; }

        protected WebClient client;

        protected XDocument document;

        protected string xmlString;

        protected bool importResult;

        protected IList objectList;

        protected XElement automationTaskHolder;

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(RepoUrlPath) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            RepoUrlPath = ProcessMacro(nameof(RepoUrlPath), RepoUrlPath);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(AutomationSequence == null, "AutomationSequence is null"))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(RepoUrlPath), string.Format("RepoUrlPath is empty string")))
                return;
        }

        public override async Task RunTask()
        {
            CreateList();
            Logging.Debug("Parsing the download url");
            ParseDownloadUrl();

            Logging.Debug("Downloading the file to xml string");
            if (!await DownloadXmlStringAsync())
            {
                Logging.Error("Failed to download the xml string");
                importResult = false;
                return;
            }

            Logging.Debug("Parsing task");
            if (!ParseXml())
            {
                Logging.Error("Failed to parse the xml string");
                importResult = false;
                return;
            }

            if (!ParseToList())
            {
                Logging.Error("Failed to parse the xml string");
                importResult = false;
                return;
            }

            Logging.Debug("Importing into sequence run");
            ImportList();
        }

        protected void ParseDownloadUrl()
        {
            if (RepoUrlPath[0].Equals('/'))
            {
                Logging.Debug("Removing extra \"/\" from the start of the URL");
                RepoUrlPath = RepoUrlPath.Substring(1);
            }

            AutomationMacro automationXmlRepoFilebaseEscapedMacro = Macros.Find(macro => macro.Name.Equals("automationRepoRoot"));
            if (automationXmlRepoFilebaseEscapedMacro == null)
                throw new BadMemeException("This shouldn't happen. Like literally. Should. Not. Happen.");
            
            RepoUrlPath = automationXmlRepoFilebaseEscapedMacro.Value + RepoUrlPath;
        }

        protected async Task<bool> DownloadXmlStringAsync()
        {
            Logging.Info("Downloading xmlString from parsed URL {0}", RepoUrlPath);
            try
            {
                using (client = new WebClient())
                {
                    xmlString = await client.DownloadStringTaskAsync(RepoUrlPath);
                }
            }
            catch (OperationCanceledException) { }
            catch (WebException wex)
            {
                Logging.Exception("Failed to download the xmlString");
                Logging.Exception(wex.ToString());
                return false;
            }
            return true;
        }

        protected bool ParseXml()
        {
            if (string.IsNullOrEmpty(xmlString))
                return false;

            Logging.Debug("Getting xml node results of TaskDefinitions");
            document = XmlUtils.LoadXDocument(xmlString, XmlLoadType.FromString);
            if (document == null)
            {
                Logging.Error("document failed to parse from string to xml");
                return false;
            }

            Logging.Debug("XpathExpression defined as {0}", XpathExpression);
            XPathNavigator result = XmlUtils.GetXNodeFromXpath(document, XpathExpression);
            if (result == null)
            {
                Logging.Error("document failed to xpath locate any tasks");
                return false;
            }

            automationTaskHolder = XElement.Parse(result.OuterXml);
            return true;
        }

        protected virtual bool ParseToList()
        {
            return CommonUtils.SetListEntries(objectList, ID, automationTaskHolder.Elements(), null, null);
        }

        protected abstract void CreateList();

        protected abstract void ImportList();

        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(importResult, "The import failed"))
                return;
        }

        public virtual void Cancel()
        {
            if (client != null)
                client.CancelAsync();
        }
        #endregion
    }
}
