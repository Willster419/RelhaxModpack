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

            foreach (AutomationCompare automationCompare in AutomationCompareTracker.AutomationCompares)
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

            Logging.Info("Number of matching files: {0}", AutomationCompareTracker.NumMatches);
            Logging.Info("Number of different (continue) files: {0}", AutomationCompareTracker.NumDifferencesContinue);
            Logging.Info("Number of different (stop) files: {0}", AutomationCompareTracker.NumDifferencesStop);

            if (ProcessTaskResultTrue(AutomationCompareTracker.NumDifferencesContinue + AutomationCompareTracker.NumMatches == 0, "The number of total compares is 0"))
                return;

            if (ValidateForExitTrue(AutomationCompareTracker.NumDifferencesStop != 0, AutomationExitCode.ComparisonManualFilesToUpdate, string.Format("There are {0} files that require manual updating, cannot continue execution", AutomationCompareTracker.NumDifferencesStop)))
                return;

            if (ValidateForExitTrue(AutomationCompareTracker.NumDifferencesContinue == 0, AutomationExitCode.ComparisonNoFilesToUpdate, string.Format("There are no differences in files that need to be updated, no need to continue execution")))
                return;
        }
    }
}
