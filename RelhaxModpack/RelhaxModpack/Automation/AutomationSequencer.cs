using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Automation
{
    public class AutomationSequencer
    {
        /// <summary>
        /// The API URL to return a json format document of the current branches in the automation repository
        /// </summary>
        public const string BranchesURL = "https://api.github.com/repos/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/branches";

        public const string AutomationXmlRootEscaped = "https://raw.githubusercontent.com/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/{branch}/root.xml";

        public List<AutomationMacro> GlobalMacros = null;

        public AutomationRunnerSettings AutomationRunnerSettings = null;

        private string[] AutomationBranches = null;
        private bool AutomationBranchesLoaded = false;

        public async Task LoadBranchesList(string branchName)
        {
            if (AutomationBranchesLoaded)
            {
                Logging.AutomationRunner(LogOptions.MethodName, "Branches list already loaded, ignoring this call", LogLevel.Warning);
                return;
            }
            List<string> branches = await CommonUtils.GetListOfGithubRepoBranchesAsync(AutomationXmlRootEscaped.Replace("{branch}", AutomationRunnerSettings.SelectedBranch));
            AutomationBranches = branches.ToArray();
        }
    }
}
