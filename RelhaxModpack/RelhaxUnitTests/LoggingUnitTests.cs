using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using System.Collections;
using System.Linq;

namespace RelhaxUnitTests
{
    [TestClass]
    public class LoggingUnitTests
    {
        [TestMethod]
        public void BaseLogfileTest()
        {
            Logfile log = UnitTestHelper.CreateLogfile();
            Assert.IsNotNull(log);

            Assert.IsTrue(log.CanWrite);
            log.Write("Successfully able to write to the log file");

            UnitTestHelper.DestroyLogfile(ref log, true);
            Assert.IsNull(log);
        }

        [TestMethod]
        public void AllLogfilesTest()
        {
            Logfiles[] logFilesWithPresetFilenames =
            {
                Logfiles.Application,
                Logfiles.Editor,
                Logfiles.PatchDesigner,
                Logfiles.Updater
            };

            Logfiles[] allLogFiles = logFilesWithPresetFilenames.Concat(new Logfiles[] { Logfiles.Installer, Logfiles.Uninstaller }).ToArray();

            foreach (Logfiles logfile in allLogFiles)
            {
                //create it
                Assert.IsTrue(Logging.Init(logfile, logFilesWithPresetFilenames.Contains(logfile) ? null: logfile.ToString() ));
                Assert.IsFalse(Logging.IsLogDisposed(logfile));
                Assert.IsTrue(Logging.IsLogOpen(logfile));

                //write to it
                Logging.WriteToLog("Successfully able to write to the logfile '{0}' with the method '{1}'", logfile, LogLevel.Info, logfile.ToString(), nameof(Logging.WriteToLog));

                Logging.GetLogfile(logfile).Write(string.Format("Successfully able to write to the logfile '{0}' with the method '{1}'", logfile.ToString(), nameof(Logging.GetLogfile)));

                //close it
                Logging.DisposeLogging(logfile);
                Assert.IsTrue(Logging.IsLogDisposed(logfile));
                Assert.IsFalse(Logging.IsLogOpen(logfile));
                Assert.IsNull(Logging.GetLogfile(logfile));
            }
        }
    }
}
