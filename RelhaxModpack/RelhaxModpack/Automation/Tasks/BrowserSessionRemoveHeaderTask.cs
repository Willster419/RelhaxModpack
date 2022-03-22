using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class BrowserSessionRemoveHeaderTask : BrowserSessionHeaderTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "browser_session_remove_header";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        #region Task execution
        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            Logging.Info(Utilities.Enums.LogOptions.ClassName, "Removing header name: '{0}'", Name);
            BrowserSessionManager.RemoveHeader(Name);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            //this method intentionally left blank
        }
        #endregion
    }
}
