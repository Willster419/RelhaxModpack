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
        public static Logfile CreateLogfile()
        {
            Logfile file = null;

            //get the method and class name
            //frame 0 -> this method
            //frame 1 -> the unit test
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            MethodBase mb = sf.GetMethod();
            string methodName = mb.Name;
            string className = mb.DeclaringType.Name;

            //the output folders should look like the following:
            //<appPath>\LogOutput\UnitTestClass\UnitTestMethod\yyyy-mm-dd_HH-mm-ss.log
            string pathTwoUp = Path.GetDirectoryName(Path.GetDirectoryName(ApplicationStartupPath));
            string logfilePath = Path.Combine(pathTwoUp, LogOutputRootFolder, className, methodName);
            logfilePath = Path.Combine(logfilePath, string.Format("{0}.log", DateTime.Now.ToString(UnittestLogfileTimestamp)));

            //create folder. it technically shouldn't exist yet (the whole time moving forward thing)
            if (!Directory.Exists(Path.GetDirectoryName(logfilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(logfilePath));

            //instance and init the logfile
            file = new Logfile(logfilePath, RelhaxModpack.Logging.ApplicationLogfileTimestamp);
            if (!file.Init())
            throw new BadMemeException("Unable to create log file. Something happened.");

            return file;
        }

        public static void DestroyLogfile(ref Logfile file, bool forceDelete = false)
        {
            if (file != null)
            {
                string logfilePath = file.Filepath;
                file.Dispose();
                file = null;
                if ((DeleteLogFiles || forceDelete) && File.Exists(logfilePath))
                {
                    File.Delete(logfilePath);
                }
            }
        }
    }
}
