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

namespace RelhaxModpack.Automation
{
    public class AutomationSequencer
    {
        /// <summary>
        /// The API URL to return a json format document of the current branches in the automation repository
        /// </summary>
        public const string BranchesURL = "https://api.github.com/repos/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/branches";

        public const string AutomationXmlRoot = "https://raw.githubusercontent.com/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/{branch}/root.xml";

        public List<AutomationMacro> GlobalMacros = null;

        public AutomationRunnerSettings AutomationRunnerSettings = null;

        public int NumErrors { get; set; }

        public int NumWarnings { get; set; }

        public string[] AutomationBranches = null;

        public bool AutomationBranchesLoaded = false;

        private XmlDocument RootDocument = null;

        private WebClient WebClient = null;

        private string AutomationXmlRootEscaped { get { return AutomationRunnerSettings == null? string.Empty : AutomationXmlRoot.Replace("{branch}", AutomationRunnerSettings.SelectedBranch); } }

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
            using (WebClient = new WebClient())
            {
                string xmlString = await WebClient.DownloadStringTaskAsync(AutomationXmlRootEscaped);
                RootDocument = XmlUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
            }
        }

        public async Task LoadGlobalMacrosAsync()
        {
            if (RootDocument == null)
                throw new NullReferenceException();

            //get the url to download from
            List<string> globalMacrosUrlSplit = AutomationXmlRootEscaped.Split('/').ToList();
            globalMacrosUrlSplit.RemoveAt(globalMacrosUrlSplit.Count() - 1);
            string globalMacrosUrlBase = string.Join("/", globalMacrosUrlSplit);
            string globalMacrosUrlFile = XmlUtils.GetXmlStringFromXPath(RootDocument, "/root.xml/GlobalMacros/@path");
            string globalMacrosUrl = string.Format("{0}/{1}", globalMacrosUrlBase, globalMacrosUrlFile);
            string globalMacrosXml = string.Empty;
            using (WebClient = new WebClient())
            {
                globalMacrosXml = await WebClient.DownloadStringTaskAsync(globalMacrosUrl);
            }
            XmlDocument globalMacrosDocument = XmlUtils.LoadXmlDocument(globalMacrosXml, XmlLoadType.FromString);
        }
    }
}
