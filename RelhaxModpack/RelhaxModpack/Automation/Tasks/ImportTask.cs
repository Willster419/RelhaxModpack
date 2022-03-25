using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Provides an implementation to import a list of xml objects into an already existing list of objects.
    /// </summary>
    public abstract class ImportTask : AutomationTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The path relative to the root of the source repository to where the xml document is located.
        /// </summary>
        public string RepoUrlPath { get; set; } = string.Empty;

        /// <summary>
        /// The Xpath search pattern to use for getting a list of xml element objects.
        /// </summary>
        protected abstract string XpathExpression { get; }

        /// <summary>
        /// The client to use for downloading the xml document with definitions to import.
        /// </summary>
        protected WebClient client;

        /// <summary>
        /// The xml document with definitions to import.
        /// </summary>
        protected XDocument document;

        /// <summary>
        /// A string pre-parsed representation of the xml document.
        /// </summary>
        /// <seealso cref="document"/>
        protected string xmlString;

        /// <summary>
        /// A flag to indicate if the import operation succeeded.
        /// </summary>
        protected bool importResult;

        /// <summary>
        /// A container list to hold the parsed objects from xml elements.
        /// </summary>
        protected IList objectList;

        /// <summary>
        /// A container list to hold the xml elements.
        /// </summary>
        protected XElement automationTaskHolder;

        /// <summary>
        /// The absolute parsed location (disk location or url) of where to retrieve the xml document.
        /// </summary>
        protected string fullFilepath;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(RepoUrlPath) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            RepoUrlPath = ProcessMacro(nameof(RepoUrlPath), RepoUrlPath);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(AutomationSequence == null, "AutomationSequence is null"))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(RepoUrlPath), string.Format("RepoUrlPath is empty string")))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            CreateList();
            Logging.Debug("Parsing the download url");
            ParseDownloadUrl();

            Logging.Debug("Downloading the file to xml string");
            if (!await LoadXmlStringAsync())
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
            importResult = true;
        }

        /// <summary>
        /// Creates an absolute path to load the xml document, either web URL or location on disk.
        /// </summary>
        protected void ParseDownloadUrl()
        {
            if (RepoUrlPath[0].Equals('/') || RepoUrlPath[0].Equals('\\'))
            {
                Logging.Debug("Removing extra slash from the start of the URL");
                RepoUrlPath = RepoUrlPath.Substring(1);
            }

            AutomationMacro automationXmlRepoFilebaseEscapedMacro = Macros.Find(macro => macro.Name.Equals("automationRepoRoot"));
            if (automationXmlRepoFilebaseEscapedMacro == null)
                throw new BadMemeException("This shouldn't happen. Like literally. Should. Not. Happen.");

            if (AutomationSettings.UseLocalRunnerDatabase)
            {
                RepoUrlPath = RepoUrlPath.Replace('/', '\\');
                fullFilepath = Path.Combine(Path.GetDirectoryName(AutomationSettings.LocalRunnerDatabaseRoot), RepoUrlPath);
            }
            else
            {
                if (RepoUrlPath.Contains("\\"))
                {
                    Logging.Warning("The RepoUrlPath argument contains folder separator chars, but should be http url slashes");
                    RepoUrlPath = RepoUrlPath.Replace('\\', '/');
                }
                fullFilepath = automationXmlRepoFilebaseEscapedMacro.Value + RepoUrlPath;
            }

            Logging.Debug("Parsed RepoUrlPath to resolve to {0}", fullFilepath);
        }

        /// <summary>
        /// Loads the xml document from a location on disk.
        /// </summary>
        /// <returns>The xml document string.</returns>
        protected string LoadXmlFromDisk()
        {
            if (!File.Exists(fullFilepath))
            {
                Logging.Error("The full file path {0} does not exist", fullFilepath);
                return null;
            }

            return File.ReadAllText(fullFilepath);
        }

        /// <summary>
        /// Loads the xml document from a url.
        /// </summary>
        /// <returns>The xml document string.</returns>
        protected async Task<string> LoadXmlFromUrlAsync()
        {
            try
            {
                using (client = new WebClient())
                {
                    return await client.DownloadStringTaskAsync(fullFilepath);
                }
            }
            catch (OperationCanceledException) { }
            catch (WebException wex)
            {
                Logging.Exception("Failed to download the xmlString");
                Logging.Exception(wex.ToString());
                return null;
            }
            return null;
        }

        /// <summary>
        /// Loads the xml document from a resource, either web or disk.
        /// </summary>
        /// <returns>True if the xml document was loaded to xmlString, false otherwise.</returns>
        protected async Task<bool> LoadXmlStringAsync()
        {
            if (AutomationSettings.UseLocalRunnerDatabase)
            {
                xmlString = LoadXmlFromDisk();
            }
            else
            {
                xmlString = await LoadXmlFromUrlAsync();
            }
            return !string.IsNullOrEmpty(xmlString);
        }

        /// <summary>
        /// Parse the xml string to an XmlDocument object and perform an Xpath search to get a list of xml element objects.
        /// </summary>
        /// <returns>True if the operation succeeded, false otherwise.</returns>
        protected bool ParseXml()
        {
            if (string.IsNullOrEmpty(xmlString))
                return false;

            Logging.Debug("Getting xml node results");
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

        /// <summary>
        /// Parses the xml element objects to custom type objects.
        /// </summary>
        /// <returns>True if the operation succeeded, false otherwise.</returns>
        protected virtual bool ParseToList()
        {
            return CommonUtils.SetListEntries(objectList, ID, automationTaskHolder.Elements(), null, null);
        }

        /// <summary>
        /// Initializes the list object objectList with the custom type to hold the parsed objects.
        /// </summary>
        protected abstract void CreateList();

        /// <summary>
        /// Perform and post-processing on the created objects and add them to a parent list.
        /// </summary>
        protected abstract void ImportList();

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (!ProcessTaskResultFalse(importResult, "The import failed"))
                return;
        }

        /// <summary>
        /// Sends a cancellation request to task's current operation.
        /// </summary>
        public virtual void Cancel()
        {
            if (client != null)
                client.CancelAsync();
        }
        #endregion
    }
}
