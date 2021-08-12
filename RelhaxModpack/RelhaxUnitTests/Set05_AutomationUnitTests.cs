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
using RelhaxModpack.Automation.Tasks;
using RelhaxModpack.Utilities;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set05_AutomationUnitTests
    {
        AutomationRunnerSettings AutomationRunnerSettings = new AutomationRunnerSettings()
        {
            DumpParsedMacrosPerSequenceRun = false,
            SelectedBranch = "master",
            BigmodsPassword = string.Empty,
            BigmodsUsername = string.Empty,
            DatabaseSavePath = string.Empty,
            DumpShellEnvironmentVarsPerSequenceRun = false,
            LocalRunnerDatabaseRoot = string.Empty,
            OpenLogWindowOnStartup = true,
            SequenceDebugMode = false,
            UseLocalRunnerDatabase = false,
            WoTClientInstallLocation = RegistryUtils.AutoFindWoTDirectoryFirst(),
            AutomationRunMode = AutomationRunMode.Interactive
        };

        ModpackSettings ModpackSettings = new ModpackSettings()
        {

        };

        CommandLineSettings CommandLineSettings = new CommandLineSettings(new string[] { })
        {

        };

        CancellationToken nullToken;

        [ClassInitialize]
        public static void SetDefaultValues(TestContext context)
        {
            Logging.RedirectLogOutput(Logfiles.AutomationRunner, Logfiles.Application);
        }

        private async Task RunTasks(AutomationSequence sequence)
        {
            foreach (AutomationTask task in sequence.AutomationTasks)
            {
                task.AutomationSequence = sequence;
                Logging.Info("Running task ID: {0}", task.ID);

                await task.Execute();

                Assert.IsTrue(task.ExitCode == AutomationExitCode.None);

                if (task is IDisposable taskDispose)
                    taskDispose.Dispose();
            }
            sequence.AutomationTasks.Clear();
        }

        [TestMethod]
        public async Task Test01_SequencerPreRunSetupTest()
        {
            AutomationSequencer sequencer = new AutomationSequencer()
            {
                AutomationRunnerSettings = this.AutomationRunnerSettings,
                CancellationToken = nullToken,
                DatabaseAutomationRunner = null,
                DatabaseManager = null
            };

            Assert.IsTrue(await sequencer.LoadBranchesListAsync());
            Assert.IsTrue(await sequencer.LoadRootDocumentAsync());
            Assert.IsTrue(sequencer.ParseRootDocumentAsync());
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

        [TestMethod]
        public async Task Test05_FileTaskSeries1Test()
        {
            //still need a automation sequence object to run this
            AutomationSequence sequence = new AutomationSequence(null, null, null, AutomationRunnerSettings, null, nullToken);
            List<AutomationTask> tasks = new List<AutomationTask>();

            //create the tasks to test
            tasks.Add(new FileCopyTask()
            {
                SourceFilePath = "RelhaxModpack.exe",
                DestinationFilePath = "BestModpackEver.exe"
            });

            tasks.Add(new FileMoveTask()
            {
                SourceFilePath = "BestModpackEver.exe",
                DestinationFilePath = "BestestModpackEver.exe"
            });

            tasks.Add(new FileDeleteTask()
            {
                SourceFilePath = "BestestModpackEver.exe"
            });

            foreach (AutomationTask task in tasks)
            {
                task.AutomationSequence = sequence;

                await task.Execute();

                Assert.IsTrue(task.ExitCode == AutomationExitCode.None);

                if (task is IDisposable taskDispose)
                    taskDispose.Dispose();
            }
        }

        [TestMethod]
        public async Task Test06_DirectoryTaskSeries1Test()
        {
            //still need a automation sequence object to run this
            AutomationSequence sequence = new AutomationSequence(null, null, null, AutomationRunnerSettings, null, nullToken);

            //delete previous runs
            string[] dirsToDelete = new string[]
            {
                "TestDir1",
                "copy_all_recurse_true",
                "copy_all_recurse_false",
                "copy_xml_recurse_true",
            };

            foreach (string path in dirsToDelete)
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

            //setup
            string testSubDirPath = Path.Combine("TestDir1", "TestSubDir2");

            //create the sub dirs
            sequence.AutomationTasks.Add(new DirectoryCreateTask
            {
                ID = "dir_setup",
                DirectoryPath = testSubDirPath
            });

            //copy over some sample files
            sequence.AutomationTasks.Add(new FileCopyTask
            {
                ID = "copy_0",
                SourceFilePath = "RelhaxModpack.exe",
                DestinationFilePath = Path.Combine("TestDir1", "RelhaxModpack1.exe")
            });

            sequence.AutomationTasks.Add(new FileCopyTask
            {
                ID = "copy_1",
                SourceFilePath = "RelhaxModpack.exe",
                DestinationFilePath = Path.Combine(testSubDirPath, "RelhaxModpack2.exe")
            });

            sequence.AutomationTasks.Add(new FileCopyTask
            {
                ID = "copy_2",
                SourceFilePath = "RelhaxModpack.xml",
                DestinationFilePath = Path.Combine("TestDir1", "RelhaxModpack1.xml")
            });

            sequence.AutomationTasks.Add(new FileCopyTask
            {
                ID = "copy_3",
                SourceFilePath = "RelhaxModpack.xml",
                DestinationFilePath = Path.Combine(testSubDirPath, "RelhaxModpack2.xml")
            });

            await RunTasks(sequence);

            //copy the directory (all files)
            sequence.AutomationTasks.Add(new DirectoryCopyTask
            {
                ID = "copy_all_recurse_true",
                Recursive = true.ToString(),
                DirectoryPath = "TestDir1",
                DestinationPath = "copy_all_recurse_true",
                SearchPattern = DirectorySearchTask.SEARCH_ALL
            });

            //copy the directory (top only)
            sequence.AutomationTasks.Add(new DirectoryCopyTask
            {
                ID = "copy_all_recurse_false",
                Recursive = false.ToString(),
                DirectoryPath = "TestDir1",
                DestinationPath = "copy_all_recurse_false",
                SearchPattern = DirectorySearchTask.SEARCH_ALL
            });

            //copy the directory (only xml files, recursive)
            sequence.AutomationTasks.Add(new DirectoryCopyTask
            {
                ID = "copy_xml_recurse_true",
                Recursive = true.ToString(),
                DirectoryPath = "TestDir1",
                DestinationPath = "copy_xml_recurse_true",
                SearchPattern = "*.xml"
            });

            await RunTasks(sequence);

            //check each of the tasks for file results
            string[] filesCopyAllRecurse = Directory.GetFiles("copy_all_recurse_true", "*", SearchOption.AllDirectories);
            string[] filesCopyAllNoRecurse = Directory.GetFiles("copy_all_recurse_false", "*", SearchOption.TopDirectoryOnly);
            string[] filesCopyXmlRecurse = Directory.GetFiles("copy_xml_recurse_true", "*", SearchOption.AllDirectories);

            Assert.AreEqual(filesCopyAllRecurse.Length, 4);
            Assert.AreEqual(filesCopyAllNoRecurse.Length, 2);
            Assert.AreEqual(filesCopyXmlRecurse.Length, 2);
            foreach (string path in filesCopyXmlRecurse)
                Assert.IsTrue(path.Contains(".xml"));

            sequence.AutomationTasks.Add(new DirectoryDeleteTask
            {
                ID = "delete_all_recurse_false",
                DirectoryPath = "copy_all_recurse_false",
                Recursive = false.ToString(),
                IncludeRootInSearch = false.ToString(),
                SearchPattern = DirectorySearchTask.SEARCH_ALL
            });

            sequence.AutomationTasks.Add(new DirectoryDeleteTask
            {
                ID = "delete_all_recurse_true",
                DirectoryPath = "copy_xml_recurse_true",
                Recursive = true.ToString(),
                IncludeRootInSearch = false.ToString(),
                SearchPattern = DirectorySearchTask.SEARCH_ALL
            });

            sequence.AutomationTasks.Add(new DirectoryDeleteTask
            {
                ID = "delete_xml_recurse_true",
                DirectoryPath = "copy_all_recurse_true",
                Recursive = true.ToString(),
                IncludeRootInSearch = false.ToString(),
                SearchPattern = "*.xml"
            });

            await RunTasks(sequence);

            Assert.IsFalse(Directory.Exists("copy_all_recurse_false"));
            Assert.IsFalse(Directory.Exists("copy_xml_recurse_true"));

            string[] filesCopyAllRecurse2 = Directory.GetFiles("copy_all_recurse_true", "*", SearchOption.AllDirectories);
            Assert.AreEqual(filesCopyAllRecurse2.Length, 2);
            foreach (string path in filesCopyAllRecurse2)
                Assert.IsTrue(path.Contains(".exe"));

            Directory.Delete("copy_all_recurse_true", true);
            Directory.Delete("TestDir1", true);
        }
    }
}
