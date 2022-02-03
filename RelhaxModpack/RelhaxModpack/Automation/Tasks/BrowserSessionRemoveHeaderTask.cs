using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class BrowserSessionRemoveHeaderTask : BrowserSessionHeaderTask
    {
        public const string TaskCommandName = "browser_session_remove_header";

        public override string Command { get { return TaskCommandName; } }

        #region Task execution
        public override async Task RunTask()
        {
            Logging.Info(Utilities.Enums.LogOptions.ClassName, "Removing header name: '{0}'", Name);
            BrowserSessionManager.RemoveHeader(Name);
        }

        public override void ProcessTaskResults()
        {
            //this method intentionally left blank
        }
        #endregion
    }
}
