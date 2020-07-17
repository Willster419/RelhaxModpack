using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RelhaxUnitTests
{
    [TestClass]
    public class LoggingUnitTests
    {
        [TestMethod]
        [UnitTestLogging()]
        public void TestMethod1()
        {
            Console.WriteLine("Test");
        }
    }
}
