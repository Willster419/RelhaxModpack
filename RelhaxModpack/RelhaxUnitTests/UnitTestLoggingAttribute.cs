using RelhaxModpack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RelhaxUnitTests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class UnitTestLoggingAttribute : Attribute
    {
        private Logfile UnitTestLog = null;

        public UnitTestLoggingAttribute()
        {
            UnitTestLog = new Logfile(UnitTestHelper.CreateLogPath(), RelhaxModpack.Logging.ApplicationLogfileTimestamp);
            if (!UnitTestLog.Init())
                throw new BadMemeException("Unable to create log file. Something happened.");
        }

        ~UnitTestLoggingAttribute()
        {
            string logfilePath = UnitTestLog.Filepath;
            if(UnitTestLog != null && UnitTestLog.CanWrite)
            {
                UnitTestLog.Dispose();
            }
            if(UnitTestLog != null)
            {
                UnitTestLog = null;
            }
            if(UnitTestHelper.DeleteLogFiles && File.Exists(logfilePath))
            {
                File.Delete(logfilePath);
            }
        }
    }
}
