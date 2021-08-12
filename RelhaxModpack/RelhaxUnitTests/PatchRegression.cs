using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Utilities.ClassEventArgs;

namespace RelhaxModpack.Patching
{
    /// <summary>
    /// A regression object is an entire regression test suite. Manages the unit tests and runs the test assertions
    /// </summary>
    /// <remarks>A regression test is designed to only test one type of patch i.e. a series of XML patches.
    /// The patching system works by having a starting file and making changes at each unit test. It then loads the files and compares
    /// the results. Results are logged to a new logfile each time a regression run is started</remarks>
    public class PatchRegression
    {
        public const string PatchLogFolder = "patch_regressions";

        private Logfile RegressionLogfile;
        private List<PatchUnitTest> UnitTests;
        private int NumPassed = 0;
        private string Startfile = "startfile";
        private string CheckFilenamePrefix = "check_";
        private string RegressionFolderPath;
        private string RegressionTypeString = "";
        private string RegressionExtension = "";
        private Patcher Patcher = new Patcher() { DebugMode = true };

        /// <summary>
        /// Make a regression object
        /// </summary>
        /// <param name="regressionType">The type of regressions to run</param>
        /// <param name="unitTestsToRun">The list of unit tests to run</param>
        public PatchRegression(PatchRegressionTypes regressionType, List<PatchUnitTest> unitTestsToRun)
        {
            if (unitTestsToRun == null)
                throw new NullReferenceException();

            string logFilename;
            UnitTests = unitTestsToRun;
            switch (regressionType)
            {
                case PatchRegressionTypes.json:
                    RegressionTypeString = "json";
                    RegressionExtension = string.Format(".{0}",RegressionTypeString);
                    Startfile = string.Format("{0}{1}", Startfile, RegressionExtension);
                    RegressionFolderPath = Path.Combine(PatchLogFolder, RegressionTypeString);
                    logFilename = Path.Combine(PatchLogFolder, "logs", string.Format("{0}_{1}{2}", RegressionTypeString, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), ".log"));
                    RegressionLogfile = new Logfile(logFilename, Logging.ApplicationLogfileTimestamp, true);
                    break;
                case PatchRegressionTypes.regex:
                    RegressionTypeString = "regex";
                    RegressionExtension = string.Format(".{0}", "txt");
                    Startfile = string.Format("{0}{1}", Startfile, RegressionExtension);
                    RegressionFolderPath = Path.Combine(PatchLogFolder, RegressionTypeString);
                    logFilename = Path.Combine(PatchLogFolder, "logs", string.Format("{0}_{1}{2}", RegressionTypeString, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), ".log"));
                    RegressionLogfile = new Logfile(logFilename, Logging.ApplicationLogfileTimestamp, true);
                    break;
                case PatchRegressionTypes.xml:
                    RegressionTypeString = "xml";
                    RegressionExtension = string.Format(".{0}", RegressionTypeString);
                    Startfile = string.Format("{0}{1}", Startfile, RegressionExtension);
                    RegressionFolderPath = Path.Combine(PatchLogFolder, RegressionTypeString);
                    logFilename = Path.Combine(PatchLogFolder, "logs", string.Format("{0}_{1}{2}", RegressionTypeString, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), ".log"));
                    RegressionLogfile = new Logfile(logFilename, Logging.ApplicationLogfileTimestamp, true);
                    break;
                case PatchRegressionTypes.followPath:
                    RegressionTypeString = "json";
                    RegressionExtension = string.Format(".{0}", "xc");
                    Startfile = string.Format("{0}{1}", @"@xvm", RegressionExtension);
                    RegressionFolderPath = Path.Combine(PatchLogFolder, "followPath");
                    logFilename = Path.Combine(PatchLogFolder, "logs", string.Format("{0}_{1}{2}", "followPath", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), ".log"));
                    RegressionLogfile = new Logfile(logFilename, Logging.ApplicationLogfileTimestamp, true);
                    break;
            }

            RegressionLogfile.OnLogfileWrite += RegressionLogfile_OnLogfileWrite;
        }

        /// <summary>
        /// Run a complete regression test based on the list of unit tests
        /// </summary>
        /// <returns>Returns false if a setup error occurred, true otherwise</returns>
        /// <remarks>The return value of the method does NOT related to the success of the Unit Tests</remarks>
        public bool RunRegressions()
        {
            //init the logfile for regressions
            if(File.Exists(RegressionLogfile.Filepath))
            {
                Logging.Warning("regression log file previously exists, deleting...");
                File.Delete(RegressionLogfile.Filepath);
            }

            //make sure the path to the file exists
            string logfilePath = Path.GetDirectoryName(RegressionLogfile.Filepath);
            if (!Directory.Exists(logfilePath))
                Directory.CreateDirectory(logfilePath);

            if (!RegressionLogfile.Init(true))
            {
                Logging.Error("failed to initialize logfile");
                return false;
            }

            //make sure the files to test against exist first
            //and the start file
            if(!File.Exists(Path.Combine(RegressionFolderPath,Startfile)))
            {
                Logging.Error("regressions start file does not exist!");
                Logging.Error(Path.Combine(RegressionFolderPath, Startfile));
                return false;
            }

            for(int i = 1; i < UnitTests.Count+1; i++)
            {
                string checkfile = Path.Combine(RegressionFolderPath, string.Format("{0}{1}{2}", CheckFilenamePrefix, i.ToString("D2"), RegressionExtension));
                if (!File.Exists(checkfile))
                {
                    Logging.Error("checkfile does not exist!");
                    Logging.Error(checkfile);
                    return false;
                }
            }

            //make a new file to be the one to make changes to
            //path get extension gets the dot
            string filenameToTest = "testfile" + RegressionExtension;
            string filenameToTestPath = Path.Combine(RegressionFolderPath, filenameToTest);
            if (File.Exists(filenameToTestPath))
                File.Delete(filenameToTestPath);
            File.Copy(Path.Combine(RegressionFolderPath, Startfile), filenameToTestPath);

            RegressionLogfile.Write("----- Unit tests start -----");

            bool breakOutEarly = false;
            foreach (PatchUnitTest unitTest in UnitTests)
            {
                unitTest.Patch.CompletePath = filenameToTestPath;
                unitTest.Patch.File = filenameToTestPath;
                unitTest.Patch.Type = RegressionTypeString;
                RegressionLogfile.Write(string.Format("Running test {0} of {1}: {2}", ++NumPassed, UnitTests.Count, unitTest.Description));

                if(unitTest.Patch.FollowPath)
                {
                    //delete testfile
                    if (File.Exists(filenameToTestPath))
                        File.Delete(filenameToTestPath);
                    if (NumPassed >= 5)
                    {
                        File.Copy(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"check_04", RegressionExtension)), filenameToTestPath);
                        if (NumPassed == 6)
                        {
                            //backup currentPlayersPanel and copy over new one
                            if(File.Exists(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanelBackup", RegressionExtension))))
                                File.Delete(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanelBackup", RegressionExtension)));
                            File.Copy(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension)),
                                Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanelBackup", RegressionExtension)));

                            if(File.Exists(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension))))
                                File.Delete(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension)));
                            File.Copy(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"check_05", RegressionExtension)),
                                Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension)));
                        }
                        else if (NumPassed == 7)
                        {
                            if (File.Exists(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension))))
                                File.Delete(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension)));
                            File.Copy(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"check_06", RegressionExtension)),
                                Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension)));
                        }
                    }
                    else
                    {
                        File.Copy(Path.Combine(RegressionFolderPath, Startfile), filenameToTestPath);
                    }
                }

                PatchExitCode exitCode = Patcher.RunPatchFromEditor(unitTest.Patch);
                if (exitCode != PatchExitCode.Success)
                {
                    RegressionLogfile.Write(string.Format("Failed! ({0})", exitCode.ToString()));
                    breakOutEarly = true;
                    break;
                }

                string checkfile = Path.Combine(RegressionFolderPath, string.Format("{0}{1}{2}", CheckFilenamePrefix, NumPassed.ToString("D2"), Path.GetExtension(Startfile)));
                RegressionLogfile.Write(string.Format("Checking results against check file {0}...", Path.GetFileName(checkfile)));
                string patchRun = File.ReadAllText(filenameToTestPath);
                string patchTestAgainst = File.ReadAllText(checkfile);

                if (patchTestAgainst.Equals(patchRun))
                {
                    RegressionLogfile.Write("Success!");
                }
                else
                {
                    RegressionLogfile.Write("Failed!");
                    breakOutEarly = true;
                    break;
                }
            }

            if (breakOutEarly)
            {
                RegressionLogfile.Write("----- Unit tests finish (fail)-----");
            }
            else
            {
                RegressionLogfile.Write("----- Unit tests finish (pass)-----");

                //delete the test file, we don't need it. (it's the same text as the last check file anyways)
                if (File.Exists(filenameToTestPath))
                    File.Delete(filenameToTestPath);

                if(UnitTests[0].Patch.FollowPath)
                {
                    //delete not needed "escaped" files and put playersPanelBackup back
                    if (File.Exists(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension))))
                        File.Delete(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension)));
                    File.Copy(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanelBackup", RegressionExtension)),
                        Path.Combine(RegressionFolderPath, string.Format("{0}{1}", @"playersPanel", RegressionExtension)));
                    foreach (string file in new string[] { "battleLabelsTemplates_escaped", "battleLabels_escaped",
                        "damageLog_escaped", "playersPanel_escaped", "testfile_escaped", "playersPanelBackup" })
                    {
                        if (File.Exists(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", file, RegressionExtension))))
                            File.Delete(Path.Combine(RegressionFolderPath, string.Format("{0}{1}", file, RegressionExtension)));
                    }
                }
            }

            //dispose log file
            RegressionLogfile.Dispose();
            RegressionLogfile = null;
            return !breakOutEarly;
        }

        private void RegressionLogfile_OnLogfileWrite(object sender, LogMessageEventArgs e)
        {
            Logging.Info("[Regression Logfile]: {0}", e.Message);
        }
    }
}
