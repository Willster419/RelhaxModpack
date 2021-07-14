using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using RelhaxModpack.Automation;
using System;
using System.Threading.Tasks;
using RelhaxModpack.Utilities.Enums;
using System.Collections.Generic;
using RelhaxModpack.Database;
using System.Text.RegularExpressions;
using RelhaxModpack.Settings;
using RelhaxModpack.Common;
using System.IO;
using System.Threading;

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

        CancellationToken nullToken;

        [TestMethod]
        public async Task Test01_GetAutomationRepoBranchesTest()
        {
            AutomationSequencer sequencer = new AutomationSequencer()
            {
                AutomationRunnerSettings = this.AutomationRunnerSettings,
                AutomationRunMode = AutomationRunMode.Interactive,
                WoTClientVersion = "TODO",
                WoTModpackOnlineFolderVersion = "TODO"
            };
            return;
            //throw new BadMemeException("you should, like, finish this");
            /*
            //TODO: dynamically get this from the beta db?
            ApplicationSettings.WoTModpackOnlineFolderVersion = "1.10.0";
            ApplicationSettings.WoTClientVersion = "1.10.0.4";
            */

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
            //bool loadAutomationSequencesResult = await sequencer.LoadAutomationSequencesAsync(DatabasePackages);
            //bool parseAutomationSequencesResult = sequencer.ParseAutomationSequences();
            //bool runSequencesResult = await sequencer.RunSequencesAsync();
            //Assert.IsFalse(string.IsNullOrEmpty(LatestSupportedWoTVersion));
            //Assert.IsFalse(string.IsNullOrEmpty(LatestSupportedWoTVersion));
        }

        [TestMethod]
        public void Test02_RegexMacroReplacementTest()
        {
            /*
             * This is by far the hardest regex match i've done and therefore feel the need to cite my sources:
             * - http://regexstorm.net/tester
             * - https://regular-expressions.mobi/balancing.html
             * - https://regular-expressions.mobi/brackets.html
             * - https://regular-expressions.mobi/refrecurse.html?wlr=1
             * - https://www.rexegg.com/regex-conditionals.html#balancing
             * - https://www.rexegg.com/regex-capture.html#namedgroups
             * 
             * And here's the notes
             * 
                MATCHES ALL
                ^[^{}]*(?>(?>(?'open'{)[^{}]*)+(?>(?'-open'})[^{}]*)+)+(?(open)(?!))$

                ONLY MATCHES ONE
                ^[^{}]*(?'open'{)+(?'inner'[^{}]*)(?'-open'})+[^{}]*(?(open)(?!))$

                MODDED1
                ^[^{}]*(?>(?>(?'open'{)(?'inner3'[^{}]*))+(?>(?'-open'})[^{}]*)+)+(?(open)(?!))$

                MODDED2 (puts kinda inner text into it's own other group)
                ^[^{}]*(?'inner1'(?'inner2'(?'open'{)(?'inner3'[^{}]*))+(?>(?'-open'})[^{}]*)+)+(?(open)(?!))$

             * 
             * (Yeah, it's *that* complex)
             */

            string[] regexTests = 
            {
                @"{use_val}",
                @"{use_val}.png",
                @"path\to\file_{macro_name}.txt",
                @"name_{use_val}_thing",
                @"name_{use_val}_thing.{ext}",
                @"name_{use_{date}_val}_thing",
                @"name_{use_{date}_val}_thing.{ext}",
                @"name_{prefix}_{use_{date}_val}_thing.{ext}",
                @"name_{{date}_val}_thing.{ext}",
                @"name_{use_{date}}_thing.{ext}",
                @"path\to\folder_{macro_name}\file_{macro_name}.txt",
                @"name_{use_{date_val}}_thing_{date_val}.{ext}",
                @"name_{use_{{date}_val}}_thing.{ext}",
                @"{use_{date}}_{{ext}_val}_{prefix}.txt"
            };

            List<AutomationMacro> automationMacros = new List<AutomationMacro>()
            {
                new AutomationMacro() { MacroType = MacroType.Local, Name = "use_val", Value = "the_val" },
                new AutomationMacro() { MacroType = MacroType.Local, Name = "macro_name", Value = "the_macro_name" },
                new AutomationMacro() { MacroType = MacroType.Local, Name = "ext", Value = "png" },
                new AutomationMacro() { MacroType = MacroType.Local, Name = "prefix", Value = "a_prefix_value" },
                new AutomationMacro() { MacroType = MacroType.Local, Name = "date", Value = "the_date" },
                new AutomationMacro() { MacroType = MacroType.Local, Name = "use_the_date_val", Value = "the_val" }, //{use_{date}_val}
                new AutomationMacro() { MacroType = MacroType.Local, Name = "the_date_val", Value = "the_other_val" }, //{{date}_val}
                new AutomationMacro() { MacroType = MacroType.Local, Name = "use_the_date", Value = "the_other_other_val" }, //{use_{date}}
                new AutomationMacro() { MacroType = MacroType.Local, Name = "date_val", Value = "a_date_value" },
                new AutomationMacro() { MacroType = MacroType.Local, Name = "use_a_date_value", Value = "the_different_val" }, //{use_{date_val}}
                new AutomationMacro() { MacroType = MacroType.Local, Name = "use_the_other_val", Value = "the_final_val" },//{use_{{date}_val}}
                new AutomationMacro() { MacroType = MacroType.Local, Name = "png_val", Value = "a_png_value" } //{{ext}_val}
            };

            string[] regexTestsAnswers =
            {
              @"the_val",
              @"the_val.png",
              @"path\to\file_the_macro_name.txt",
              @"name_the_val_thing",
              @"name_the_val_thing.png",
              @"name_the_val_thing",
              @"name_the_val_thing.png",
              @"name_a_prefix_value_the_val_thing.png",
              @"name_the_other_val_thing.png",
              @"name_the_other_other_val_thing.png",
              @"path\to\folder_the_macro_name\file_the_macro_name.txt",
              @"name_the_different_val_thing_a_date_value.png",
              @"name_the_final_val_thing.png",
              @"the_other_other_val_a_png_value_a_prefix_value.txt"
            };

            //still need a automation sequence object to run this
            AutomationSequence sequence = new AutomationSequence(null, null, null, AutomationRunnerSettings, null, nullToken);

            //create a random task so we can process macros for this test
            ShellExecuteTask task = new ShellExecuteTask()
            {
                Wd = ApplicationConstants.ApplicationStartupPath,
                AutomationSequence = sequence
            };
            task.Macros.AddRange(automationMacros);

            for (int i = 0; i < regexTests.Length; i++)
            {
                string test = regexTests[i];
                string answer = regexTestsAnswers[i];

                task.Cmd = test;

                task.ProcessMacros();

                Assert.AreEqual(task.Wd, ApplicationConstants.ApplicationStartupPath);
                Assert.AreEqual(task.Cmd, answer);
            }

        }

        [TestMethod]
        public async Task Test03_DownloadBrowserTaskTest()
        {
            //still need a automation sequence object to run this
            AutomationSequence sequence = new AutomationSequence(null, null, null, AutomationRunnerSettings, null, nullToken);

            //create a random task so we can process macros for this test
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            //https://stackoverflow.com/a/39064452/3128017
            DownloadBrowserTask task = new DownloadBrowserTask()
            {
                DestinationPath = @"downloaded_file.zip",
                Url = "https://wgmods.net/2030/",
                ID = "download_mod_updated_test",
                HtmlPath = @"//a[contains(@class, 'ModDetails_hidden')]//@href",
                AutomationSequence = sequence
            };

            await task.Execute();
        }

        [TestMethod]
        public async Task Test04_FileHashCompareTest()
        {
            //first test the individual component, then the task
            FileHashComparer fileHashComparer = new FileHashComparer();

            string fileAPath = Path.Combine(UnitTestHelper.ResourcesFolder, "battleAtlas.dds");
            string fileACorrectHashPath = Path.Combine(UnitTestHelper.ResourcesFolder, "battleAtlas.dds.md5");
            string fileACorrectHash = File.ReadAllText(fileACorrectHashPath);

            await fileHashComparer.ComputeHashA(fileAPath);

            Assert.IsTrue(fileHashComparer.HashACalculated);
            Assert.IsNotNull(fileHashComparer.HashAStringBuilder);
            Assert.AreEqual(fileACorrectHash.ToLower(), fileHashComparer.HashAStringBuilder.ToString().ToLower());
        }

        /*
         * Kept in case later it's needed for test initialization
        [TestInitialize]
        public void SetDefaultValues()
        {
            
        }
        */
    }
}
