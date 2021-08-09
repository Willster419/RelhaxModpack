using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack;
using RelhaxModpack.Common;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxUnitTests
{
    public class UnitTestLogBase
    {
        public UnitTestLogBase()
        {
            string[] listofLogfilenames = new string[]
            {
                Logging.ApplicationLogFilename,
                RelhaxModpack.Windows.DatabaseEditor.LoggingFilename,
                RelhaxModpack.Windows.PatchDesigner.LoggingFilename,
                RelhaxModpack.Windows.DatabaseAutomationRunner.LoggingFilename,
                RelhaxModpack.Windows.ModpackToolbox.LoggingFilename
            };
            foreach (string logfile in listofLogfilenames)
            {
                if (File.Exists(logfile))
                    File.Delete(logfile);
            }

            //init all logs if they aren't already init
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                //if the log file isn't already open, then create it
                if(!Logging.IsLogOpen(logfile))
                    //init with the default name (pass in null to get default), or if no default, the name of the enumeration and ".log"
                    //throw exception if it fails to create the log file
                    if (!Logging.Init(logfile, true, false, UnitTestHelper.LogFilesWithPresetFilenames.Contains(logfile) ? null : string.Format("{0}.log", logfile.ToString())))
                        throw new BadMemeException("Failed to create a log file");
            }
            Logging.RedirectLogOutput(Logfiles.AutomationRunner, Logfiles.Application);
        }
    }
}
