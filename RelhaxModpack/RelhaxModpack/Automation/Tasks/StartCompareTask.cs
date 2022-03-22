using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class StartCompareTask : AutomationTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "compare_start";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
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
            Logging.Info("Clear previous compares");
            AutomationCompareManager.Reset();
        }

        public override void ProcessTaskResults()
        {
            //this method intentionally left blank
        }
    }
}
