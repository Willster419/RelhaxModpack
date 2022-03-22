using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Automation.Tasks
{
    public class EndCompareTask : AutomationTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "compare_end";

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
            //this method intentionally left blank
        }

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
