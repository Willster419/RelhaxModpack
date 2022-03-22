using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class EndBrowserSessionTask : AutomationTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "browser_session_end";

        public override string Command { get { return TaskCommandName; } }

        public override void ProcessMacros()
        {
            //this method intentionally left blank
        }

        public override void ValidateCommands()
        {
            //this method intentionally left blank
        }

        public override async Task RunTask()
        {
            Logging.Debug("Browser session completed");
            AutomationSequence.ClearBrowserSessionManager();
        }

        public override void ProcessTaskResults()
        {
            //this method intentionally left blank
        }
    }
}
