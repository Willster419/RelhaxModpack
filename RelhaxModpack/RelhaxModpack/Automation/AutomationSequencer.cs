using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System.IO;
using RelhaxModpack.Database;

namespace RelhaxModpack.Automation
{
    public class AutomationSequencer : IDisposable
    {
        /// <summary>
        /// The API URL to return a json format document of the current branches in the automation repository
        /// </summary>
        public const string BranchesURL = "https://api.github.com/repos/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/branches";

        public const string AutomationXmlRepoFilebase = "https://raw.githubusercontent.com/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/{branch}/";

        public const string AutomationXmlRoot = AutomationXmlRepoFilebase + "root.xml";

        public List<AutomationMacro> GlobalMacros = null;

        public List<AutomationSequence> AutomationSequences = null;

        public AutomationRunnerSettings AutomationRunnerSettings = null;

        public int NumErrors { get; set; }

        public int NumWarnings { get; set; }

        public string[] AutomationBranches = null;

        public bool AutomationBranchesLoaded = false;

        public AutomationRunMode AutomationRunMode = AutomationRunMode.Interactive;

        private XmlDocument RootDocument = null;

        private WebClient WebClient = null;

        private string AutomationXmlRootEscaped { get { return AutomationRunnerSettings == null? string.Empty : AutomationXmlRoot.Replace("{branch}", AutomationRunnerSettings.SelectedBranch); } }

        private string AutomationXmlRepoFilebaseEscaped { get { return AutomationRunnerSettings == null ? string.Empty : AutomationXmlRepoFilebase.Replace("{branch}", AutomationRunnerSettings.SelectedBranch); } }

        public AutomationSequencer()
        {
            WebClient = new WebClient();
        }

        /// <summary>
        /// Load the list of branches from github
        /// </summary>
        /// <returns>A task of the asynchronous operation</returns>
        public async Task LoadBranchesListAsync()
        {
            if (AutomationBranchesLoaded)
            {
                Logging.AutomationRunner(LogOptions.MethodName, "Branches list already loaded, ignoring this call", LogLevel.Warning);
                return;
            }

            List<string> branches = await CommonUtils.GetListOfGithubRepoBranchesAsync(BranchesURL);
            AutomationBranches = branches.ToArray();
            AutomationBranchesLoaded = true;
        }

        public async Task LoadRootDocumentAsync()
        {
            string xmlString = await WebClient.DownloadStringTaskAsync(AutomationXmlRootEscaped);
            RootDocument = XmlUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
        }

        public async Task LoadGlobalMacrosAsync()
        {
            if (RootDocument == null)
                throw new NullReferenceException();

            //get the url to download from
            string globalMacrosUrlFile = XmlUtils.GetXmlStringFromXPath(RootDocument, "/root.xml/GlobalMacros/@path");
            string globalMacrosUrl = string.Format("{0}{1}", AutomationXmlRepoFilebaseEscaped, globalMacrosUrlFile);
            string globalMacrosXml = string.Empty;
            globalMacrosXml = await WebClient.DownloadStringTaskAsync(globalMacrosUrl);
            XmlDocument globalMacrosDocument = XmlUtils.LoadXmlDocument(globalMacrosXml, XmlLoadType.FromString);
        }

        public async Task<bool> LoadAutomationSequences(List<DatabasePackage> packagesToRun)
        {
            if (RootDocument == null)
                throw new NullReferenceException();
            if (packagesToRun == null)
                throw new NullReferenceException();
            if (packagesToRun.Count == 0)
                throw new BadMemeException("packagesToRun must have at least one package to run automation on");

            Logging.AutomationRunner(LogOptions.MethodName, "Checking root document for to build urls to download automation sequences", LogLevel.Info);
            foreach (DatabasePackage package in packagesToRun)
            {
                Logging.AutomationRunner(LogOptions.MethodName, "Parsing path for automation of package {0}", LogLevel.Info, package.PackageName);
                //sample xpath: /root.xml/AutomationSequence[@UID='123456789ABCD']
                XmlElement result = XmlUtils.GetXmlNodeFromXPath(RootDocument, string.Format("/root.xml/AutomationSequence[@UID='{0}']", package.UID)) as XmlElement;
                if (result == null)
                {
                    Logging.Error(Logfiles.AutomationRunner, "Package not found in automation database");
                    return false;
                }

                if (result.Attributes["packageName"].Value != package.PackageName)
                {
                    Logging.Warning(Logfiles.AutomationRunner, "The packageName property is out of date. From database: {0}. From Package: {1}", result.Attributes["packageName"].Value, package.PackageName);
                }

                string pathToFileFromRepoRoot = result.Attributes["path"].Value;
                if (string.IsNullOrEmpty(pathToFileFromRepoRoot))
                {
                    Logging.Error(Logfiles.AutomationRunner, "Package path attribute not found from xml node");
                    return false;
                }
                if (pathToFileFromRepoRoot[0] == '/')
                {
                    Logging.Warning(Logfiles.AutomationRunner, "Package path attribute starts with slash, please update entry to remove it!");
                    pathToFileFromRepoRoot = pathToFileFromRepoRoot.Substring(1);
                }
                AutomationSequences.Add(new AutomationSequence() { AutomationSequencer = this, Package = package, SequenceDownloadUrl = AutomationXmlRepoFilebaseEscaped + pathToFileFromRepoRoot });
                Logging.Debug(Logfiles.AutomationRunner, "Added automation sequence URL: {0}", AutomationSequences.Last().SequenceDownloadUrl);
            }

            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Parsing each automationSequence from its download URL");
            foreach (AutomationSequence automationSequence in AutomationSequences)
            {
                Logging.Info(Logfiles.AutomationRunner, "Parsing automation sequence for package {0}", automationSequence.Package.PackageName);
                //TODO
            }

            return true;
        }

        public void Dispose()
        {
            ((IDisposable)WebClient).Dispose();
        }
    }
}
