using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using RelhaxModpack.Automation;
using System;
using System.Threading.Tasks;
using RelhaxModpack.Utilities.Enums;
using System.Collections.Generic;
using RelhaxModpack.Database;
using System.Text.RegularExpressions;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set05_AutomationUnitTests : UnitTestLogBase
    {
        AutomationRunnerSettings AutomationRunnerSettings = new AutomationRunnerSettings()
        {
            DumpParsedMacrosPerSequenceRun = true,
            SelectedBranch = "master"
        };

        [TestMethod]
        public async Task Test01_GetAutomationRepoBranchesTest()
        {
            AutomationSequencer sequencer = new AutomationSequencer()
            {
                AutomationRunnerSettings = this.AutomationRunnerSettings,
                AutomationRunMode = AutomationRunMode.Interactive
            };

            await sequencer.LoadBranchesListAsync();
            await sequencer.LoadRootDocumentAsync();
            await sequencer.LoadGlobalMacrosAsync();
            List<DatabasePackage> DatabasePackages = new List<DatabasePackage>();
            DatabasePackages.Add(new SelectablePackage()
            {
                UID = "123456789ABCD",
                PackageName = "Some_rofl_op_russian_medium",
                ParentCategory = new Category() { Name = "Cat_name" },
                Level = 0
            });
            bool loadAutomationSequencesResult = await sequencer.LoadAutomationSequencesAsync(DatabasePackages);
            bool parseAutomationSequencesResult = sequencer.ParseAutomationSequences();
            bool runSequencesResult = await sequencer.RunSequencesAsync();
            //Assert.IsFalse(string.IsNullOrEmpty(LatestSupportedWoTVersion));
            //Assert.IsFalse(string.IsNullOrEmpty(LatestSupportedWoTVersion));
        }

        [TestMethod]
        public void Test02_RegexMacroReplacementTest()
        {
            
        }

        [TestInitialize]
        public void SetDefaultValues()
        {
            //TODO: dynamically get this from the beta db?
            Settings.WoTModpackOnlineFolderVersion = "1.10.0";
            Settings.WoTClientVersion = "1.10.0.4";
        }
    }
}
