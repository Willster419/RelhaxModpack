using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;

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

            UnitTestHelper.DestroyLogfile(ref log, false);
            Assert.IsNull(log);
        }
    }
}
