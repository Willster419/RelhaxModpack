using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack;

namespace RelhaxUnitTests
{
    public class UnitTestLogBase
    {
        public UnitTestLogBase()
        {
            //init all logs if they aren't already init
            foreach (Logfiles logfile in UnitTestHelper.AllLogFiles)
            {
                if(!Logging.IsLogOpen(logfile))
                    if (!Logging.Init(logfile, UnitTestHelper.LogFilesWithPresetFilenames.Contains(logfile) ? null : logfile.ToString()))
                        throw new BadMemeException("Failed to create a log file");
            }
        }
    }
}
