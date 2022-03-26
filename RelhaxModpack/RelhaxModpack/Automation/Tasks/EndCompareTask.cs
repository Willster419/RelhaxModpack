using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// End a compare session by closing the automation compare manager.
    /// </summary>
    public class EndCompareTask : AutomationTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "compare_end";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            //this method intentionally left blank
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            //this method intentionally left blank
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //this method intentionally left blank
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            Logging.Info("Starting analysis of compares");

            foreach (AutomationCompare automationCompare in AutomationCompareManager.AutomationCompares)
            {
                Logging.Info("Compare {0}", automationCompare.AutomationTask.ID);
                Logging.Debug("Compare mode: {0}", automationCompare.CompareMode.ToString());
                automationCompare.PrintToLog();

                if (automationCompare.CompareResult)
                {
                    Logging.Info("Compare {0} is MATCH", automationCompare.AutomationTask.ID); 
                }
                else
                {
                    Logging.Info("Compare {0} is DIFFERENT", automationCompare.AutomationTask.ID);
                }
            }

            Logging.Info("Number of matching files: {0}", AutomationCompareManager.NumMatches);
            Logging.Info("Number of different (continue) files: {0}", AutomationCompareManager.NumDifferencesContinue);
            Logging.Info("Number of different (stop) files: {0}", AutomationCompareManager.NumDifferencesStop);

            if (ProcessTaskResultTrue(AutomationCompareManager.NumDifferencesContinue + AutomationCompareManager.NumMatches == 0, "The number of total compares is 0"))
                return;

            if (ValidateForExitTrue(AutomationCompareManager.NumDifferencesStop != 0, AutomationExitCode.ComparisonManualFilesToUpdate, string.Format("There are {0} files that require manual updating, cannot continue execution", AutomationCompareManager.NumDifferencesStop)))
                return;

            if (ValidateForExitTrue(AutomationCompareManager.NumDifferencesContinue == 0, AutomationExitCode.ComparisonNoFilesToUpdate, string.Format("There are no differences in files that need to be updated, no need to continue execution")))
                return;
        }
    }
}
