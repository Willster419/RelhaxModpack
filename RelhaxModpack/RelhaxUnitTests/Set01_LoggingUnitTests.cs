using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set01_LoggingUnitTests
    {
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
        public void Test02_AllLogFilesTest()
        {
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                //create it (if not already open)
                if (!Logging.IsLogOpen(logfile))
                {
                    Assert.IsTrue(Logging.Init(logfile, UnitTestHelper.LogFilesWithPresetFilenames.Contains(logfile) ? null : logfile.ToString()));
                    Assert.IsFalse(Logging.IsLogDisposed(logfile));
                    Assert.IsTrue(Logging.IsLogOpen(logfile));
                }

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
