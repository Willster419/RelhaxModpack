using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using RelhaxModpack.Utilities.Enums;
using System.IO;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set01_LoggingUnitTests
    {
        //this technically applies to every test upon initialization, but it's placed here
        //https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2012/ms245572(v=vs.110)
        [AssemblyInitialize]
        public static void SetupLogging(TestContext context)
        {
            foreach (string logfile in UnitTestHelper.ListOfLogfilenames)
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
            }

            //if the log file isn't already open, then create it
            if (!Logging.IsLogOpen(Logfiles.Application))
                //init with the default name (pass in null to get default), or if no default, the name of the enumeration and ".log"
                //throw exception if it fails to create the log file
                if (!Logging.Init(Logfiles.Application, true, false))
                    throw new BadMemeException("Failed to create a log file");
        }

        [AssemblyCleanup]
        public static void CleanupLogging()
        {
            //init all logs if they aren't already init
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                if (!Logging.IsLogDisposed(logfile))
                    Logging.DisposeLogging(logfile);
            }
        }

        [TestMethod]
        public void Test01_BaseLogfileTest()
        {
            Logfile log = UnitTestHelper.CreateLogfile();
            Assert.IsNotNull(log);

            Assert.IsTrue(log.CanWrite);
            log.Write("Successfully able to write to the log file");

            UnitTestHelper.DestroyLogfile(ref log, true);
            Assert.IsNull(log);
        }

        [TestMethod]
        public void Test02_AllLogFilesNotOpenTest()
        {
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                //if already open (should only be application), then skip
                if (Logging.IsLogOpen(logfile))
                    continue;

                Assert.IsTrue(Logging.Init(logfile, true, false, UnitTestHelper.LogFilesWithPresetFilenames.Contains(logfile) ? null : logfile.ToString()));
                Assert.IsFalse(Logging.IsLogDisposed(logfile));
                Assert.IsTrue(Logging.IsLogOpen(logfile));

                //write to it
                Logging.WriteToLog("Successfully able to write to the logfile '{0}' with the method '{1}'", logfile, LogLevel.Info, logfile.ToString(), nameof(Logging.WriteToLog));
                Logging.GetLogfile(logfile).Write(string.Format("Successfully able to write to the logfile '{0}' with the method '{1}'", logfile.ToString(), nameof(Logging.GetLogfile)));

                //get string name
                string logfilePath = Logging.GetLogfile(logfile).Filepath;

                //close it
                Logging.DisposeLogging(logfile);

                //tests
                Assert.IsTrue(File.Exists(logfilePath));
                File.Delete(logfilePath);
                Assert.IsTrue(Logging.IsLogDisposed(logfile));
                Assert.IsFalse(Logging.IsLogOpen(logfile));
                Assert.IsNull(Logging.GetLogfile(logfile));
            }
        }
    }
}
