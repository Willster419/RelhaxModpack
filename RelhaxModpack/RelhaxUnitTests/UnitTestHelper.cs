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
using RelhaxModpack.Utilities.Enums;

namespace RelhaxUnitTests
{
    public static class UnitTestHelper
    {
        public static Logfiles[] LogFilesWithPresetFilenames =
        {
            Logfiles.Application,
            Logfiles.Editor,
            Logfiles.PatchDesigner,
            Logfiles.Updater,
            Logfiles.AutomationRunner
        };

        public static Logfiles[] AllLogFiles = LogFilesWithPresetFilenames.Concat(new Logfiles[] { Logfiles.Installer, Logfiles.Uninstaller }).ToArray();

        public static string[] ListOfLogfilenames = new string[]
        {
            Logging.ApplicationLogFilename,
            RelhaxModpack.Windows.DatabaseEditor.LoggingFilename,
            RelhaxModpack.Windows.PatchDesigner.LoggingFilename,
            RelhaxModpack.Windows.DatabaseAutomationRunner.LoggingFilename,
            RelhaxModpack.Windows.ModpackToolbox.LoggingFilename
        };

        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// The root file structure of the source project
        /// </summary>
        public static readonly string ApplicationProjectRoot = Path.GetDirectoryName(Path.GetDirectoryName(ApplicationStartupPath));

        /// <summary>
        /// The absolute location to the 'Resources' folder
        /// </summary>
        public static readonly string ResourcesFolder = Path.Combine(ApplicationProjectRoot, "Resources");

        /// <summary>
        /// The string of the root folder to place unit tests
        /// </summary>
        public const string LogOutputRootFolder = "LogOutput";

        /// <summary>
        /// The string time format for log entries
        /// </summary>
        public const string UnitTestLogfileTimestamp = "yyyy-MM-dd_HH-mm-ss";

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
            string logfilePath = Path.Combine(ApplicationProjectRoot, LogOutputRootFolder, className, methodName);
            logfilePath = Path.Combine(logfilePath, string.Format("{0}.log", DateTime.Now.ToString(UnitTestLogfileTimestamp)));

            //create folder. it technically shouldn't exist yet (the whole time moving forward thing)
            if (!Directory.Exists(Path.GetDirectoryName(logfilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(logfilePath));

            //instance and init the logfile
            file = new Logfile(logfilePath, Logging.ApplicationLogfileTimestamp, true);
            if (!file.Init(false))
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
