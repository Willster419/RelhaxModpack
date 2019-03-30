using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RelhaxModpack
{

    public enum RegressionTypes
    {
        json,
        xml,
        regex
    }

    public class UnitTest
    {
        public Patch Patch;
        public string Description;
        public bool ShouldPass;
    }

    public class Regression
    {

        private Logfile RegressionLogfile;
        private List<UnitTest> UnitTests;
        private int NumPassed = 0;
        private string Startfile = "startfile";
        private string CheckFilenamePrefix = "check_";
        private string RegressionFolderPath;
        private string RegressionTypeString = "";
        private string RegressionExtension = "";

        public Regression(RegressionTypes regressionType, List<UnitTest> unitTestsToRun)
        {
            UnitTests = unitTestsToRun;
            switch (regressionType)
            {
                case RegressionTypes.json:
                    RegressionTypeString = "json";
                    RegressionExtension = string.Format(".{0}",RegressionTypeString);
                    Startfile = string.Format("{0}{1}", Startfile, RegressionExtension);
                    RegressionFolderPath = Path.Combine("patch_regressions", RegressionTypeString);
                    RegressionLogfile = new Logfile(Path.Combine("patch_regressions", "logs", string.Format("{0}_{1}{2}", RegressionTypeString,
                        DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") , ".log")),Logging.ApplicationLogfileTimestamp);
                    break;
                case RegressionTypes.regex:
                    RegressionTypeString = "regex";
                    RegressionExtension = string.Format(".{0}", "txt");
                    Startfile = string.Format("{0}{1}", Startfile, RegressionExtension);
                    RegressionFolderPath = Path.Combine("patch_regressions", RegressionTypeString);
                    RegressionLogfile = new Logfile(Path.Combine("patch_regressions", "logs", string.Format("{0}_{1}{2}", RegressionTypeString,
                        DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), ".log")), Logging.ApplicationLogfileTimestamp);
                    break;
                case RegressionTypes.xml:
                    RegressionTypeString = "xml";
                    RegressionExtension = string.Format(".{0}", RegressionTypeString);
                    Startfile = string.Format("{0}{1}", Startfile, RegressionExtension);
                    RegressionFolderPath = Path.Combine("patch_regressions", RegressionTypeString);
                    RegressionLogfile = new Logfile(Path.Combine("patch_regressions", "logs", string.Format("{0}_{1}{2}", RegressionTypeString,
                        DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), ".log")), Logging.ApplicationLogfileTimestamp);
                    break;
            }
        }

        public bool RunRegressions()
        {
            //if from the editor, enable verbose logging (allows it to get debug log statements)
            bool tempVerboseLoggingSetting = ModpackSettings.VerboseLogging;
            if (!ModpackSettings.VerboseLogging)
            {
                Logging.Info("p.FromEditor=true and ModpackSettings.VerboseLogging=false, setting to true for duration of patch method");
                ModpackSettings.VerboseLogging = true;
            }

            //init the logfile for regressions
            if(File.Exists(RegressionLogfile.Filepath))
            {
                Logging.Warning("regression log file previously exists, deleting...");
                File.Delete(RegressionLogfile.Filepath);
            }

            if(!RegressionLogfile.Init())
            {
                Logging.Error("failed to initalize logfile");
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
                if (!File.Exists(checkfile))//!File.Exists(checkfile)
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

            WriteToLogfiles("----- Unit tests start -----");

            bool breakOutEarly = false;
            foreach (UnitTest unitTest in UnitTests)
            {
                unitTest.Patch.CompletePath = filenameToTestPath;
                unitTest.Patch.File = filenameToTestPath;
                unitTest.Patch.Type = RegressionTypeString;
                WriteToLogfiles("Running test {0} of {1}: {2}", ++NumPassed, UnitTests.Count, unitTest.Description);
                PatchUtils.RunPatch(unitTest.Patch);
                WriteToLogfiles("Checking results...");
                string patchRun = File.ReadAllText(filenameToTestPath);
                string checkfile = Path.Combine(RegressionFolderPath, string.Format("{0}{1}{2}", CheckFilenamePrefix, NumPassed.ToString("D2"), Path.GetExtension(Startfile)));
                string patchTestAgainst = File.ReadAllText(checkfile);
                if (patchTestAgainst.Equals(patchRun))
                {
                    WriteToLogfiles("Success!");
                }
                else
                {
                    WriteToLogfiles("Failed!");
                    breakOutEarly = true;
                    break;
                }
            }

            if (breakOutEarly)
            {
                WriteToLogfiles("----- Unit tests finish (fail)-----");
            }
            else
            {
                WriteToLogfiles("----- Unit tests finish (pass)-----");
                //delete the test file, we don't need it. (it's the same text as the last check file anyways)
                if (File.Exists(filenameToTestPath))
                    File.Delete(filenameToTestPath);
            }
            //dispose log file
            RegressionLogfile.Dispose();

            //set the verbose setting back
            Logging.Debug("temp logging setting={0}, ModpackSettings.VerboseLogging={1}, setting logging back to temp");
            ModpackSettings.VerboseLogging = tempVerboseLoggingSetting;
            return true;
        }

        private void WriteToLogfiles(string message, params object[] paramss)
        {
            WriteToLogfiles(string.Format(message, paramss));
        }

        private void WriteToLogfiles(string message)
        {
            Logging.Debug(message);
            RegressionLogfile.Write(message);
        }
    }
}
