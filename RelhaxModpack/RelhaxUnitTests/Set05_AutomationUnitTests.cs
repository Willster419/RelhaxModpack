using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using RelhaxModpack.Automation;
using System;
using System.Threading.Tasks;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set05_AutomationUnitTests : UnitTestLogBase
    {
        [TestMethod]
        public async Task Test01_GetAutomationRepoBranchesTest()
        {
            AutomationSequencer sequencer = new AutomationSequencer()
            {
                AutomationRunnerSettings = new AutomationRunnerSettings()
                {
                    SelectedBranch = "master" // is default
                }
            };

            await sequencer.LoadBranchesListAsync();
            await sequencer.LoadRootDocumentAsync();
            await sequencer.LoadGlobalMacrosAsync();
            //Assert.IsFalse(string.IsNullOrEmpty(LatestSupportedWoTVersion));
            //Assert.IsFalse(string.IsNullOrEmpty(LatestSupportedWoTVersion));
        }
    }
}
