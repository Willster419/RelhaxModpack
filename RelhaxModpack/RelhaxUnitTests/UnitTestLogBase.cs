using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxUnitTests
{
    public class UnitTestLogBase
    {
        public UnitTestLogBase()
        {
            //init all logs if they aren't already init
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                //if the log file isn't already open, then create it
                if(!Logging.IsLogOpen(logfile))
                    //init with the default name (pass in null to get default), or if no default, the name of the enumeration and ".log"
                    //throw exception if it fails to create the log file
                    if (!Logging.Init(logfile, UnitTestHelper.LogFilesWithPresetFilenames.Contains(logfile) ? null : string.Format("{0}.log", logfile.ToString())))
                        throw new BadMemeException("Failed to create a log file");
            }
        }
    }
}
