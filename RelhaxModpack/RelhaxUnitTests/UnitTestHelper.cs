using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace RelhaxUnitTests
{
    public static class UnitTestHelper
    {
        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// The string of the root folder to place unit tests
        /// </summary>
        public const string LogOutputRootFolder = "LogOutput";

        /// <summary>
        /// The string time format for log entries
        /// </summary>
        public const string UnittestLogfileTimestamp = "yyyy-MM-dd_HH-mm-ss";

        /// <summary>
        /// Determines if the unit test should delete the log file after. Useful for if rapid testing/debugging.
        /// </summary>
        public static bool DeleteLogFiles = true;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string CreateLogPath()
        {
            string logfilePath = ApplicationStartupPath;

            //the output folders should look like the following:
            //<appPath>\LogOutput\UnitTestClass\UnitTestMethod\yyyy-mm-dd_HH-mm-ss.log
            logfilePath = Path.Combine(logfilePath, LogOutputRootFolder);

            //get the name of the unit test method executing
            //frame 0 -> this method name
            //frame 1 -> the name of the above method (log creation method)
            //frame 2 -> the name of the test method
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(2);
            MethodBase mb = sf.GetMethod();

            //the method name
            logfilePath = Path.Combine(logfilePath, mb.Name);

            //the class name
            logfilePath = Path.Combine(logfilePath, mb.DeclaringType.Name);

            //the date and type stamp
            logfilePath = Path.Combine(logfilePath, string.Format("{0}.log", DateTime.Now.ToString(UnittestLogfileTimestamp)));

            return logfilePath;
        }
    }
}
