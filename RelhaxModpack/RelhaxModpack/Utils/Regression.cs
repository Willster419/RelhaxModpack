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
    }
    public class Regression
    {

        private Logfile RegressionLogfile;
        private List<UnitTest> UnitTests;
        private int NumPassed = 0;
        private string Startfile = "startfile";
        private string CheckFilenamePrefix = "check_";
        private string RegressionFolderPath;

        public Regression(RegressionTypes regressionType, List<UnitTest> unitTestsToRun)
        {
            UnitTests = unitTestsToRun;
            switch (regressionType)
            {
                case RegressionTypes.json:
                    Startfile = Startfile + ".json";
                    RegressionFolderPath = Path.Combine("patch_regressions", "json");
                    RegressionLogfile = new Logfile(Path.Combine("patch_regressions", "logs", string.Format("{0}_{1}{2}", "json", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") , ".log")),Logging.ApplicationLogfileTimestamp);
                    break;
                case RegressionTypes.regex:
                    Startfile = Startfile + ".txt";
                    RegressionFolderPath = Path.Combine("patch_regressions", "regex");
                    RegressionLogfile = new Logfile(Path.Combine("patch_regressions", "logs", string.Format("{0}_{1}{2}", "json", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ".log")), Logging.ApplicationLogfileTimestamp);
                    break;
                case RegressionTypes.xml:
                    Startfile = Startfile + ".xml";
                    RegressionFolderPath = Path.Combine("patch_regressions", "xml");
                    RegressionLogfile = new Logfile(Path.Combine("patch_regressions", "logs", string.Format("{0}_{1}{2}", "json", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ".log")), Logging.ApplicationLogfileTimestamp);
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
            for(int i = 0; i < UnitTests.Count; i++)
            {
                string checkfile = Path.Combine(RegressionFolderPath, string.Format("{0}{1}.{2}", CheckFilenamePrefix, ++i, Path.GetExtension(Startfile)));
                if (!File.Exists(checkfile))
                {
                    Logging.Error("checkfile does not exist!");
                    Logging.Error(checkfile);
                    return false;
                }
            }

            //make a new file to be the one to make changes to
            string filenameToTest = "testfile" + "." + Path.GetExtension(Startfile);
            File.Copy(Path.Combine(RegressionFolderPath, Startfile), Path.Combine(RegressionFolderPath, filenameToTest));

            WriteToLogfiles("----- Unit tests start -----");

            foreach (UnitTest unitTest in UnitTests)
            {
                WriteToLogfiles("Running test {0} of {1}: {2}{3}{4}", ++NumPassed, UnitTests.Count, unitTest.Description, Environment.NewLine, unitTest.Patch.DumpPatchInfoForLog);
                unitTest.Patch.CompletePath = Path.Combine(RegressionFolderPath, filenameToTest);
                PatchUtils.RunPatch(unitTest.Patch);
                WriteToLogfiles("Checking results...");
                string patchRun = File.ReadAllText(Path.Combine(RegressionFolderPath, filenameToTest));
                string checkfile = Path.Combine(RegressionFolderPath, string.Format("{0}{1}.{2}", CheckFilenamePrefix, NumPassed, Path.GetExtension(Startfile)));
                string patchTestAgainst = File.ReadAllText(checkfile);
                if (patchTestAgainst.Equals(patchRun))
                {
                    WriteToLogfiles("Success!");
                }
                else
                {
                    WriteToLogfiles("Failed!");
                    break;
                }
            }

            WriteToLogfiles("----- Unit tests finish -----");
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
