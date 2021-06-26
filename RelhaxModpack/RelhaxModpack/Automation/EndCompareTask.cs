using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Automation
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
                Logging.Debug("Compare {0}", automationCompare.AutomationTask.ID);
                Logging.Debug("File a: {0}, Hash: {1}", automationCompare.CompareAFilepath, automationCompare.CompareAHash);
                Logging.Debug("File b: {0}, Hash: {1}", automationCompare.CompareBFilepath, automationCompare.CompareBHash);
                if (automationCompare.CompareResult)
                {
                    Logging.Info("Compare {0} is matching files", automationCompare.AutomationTask.ID); 
                }
                else
                {
                    Logging.Info("Compare {0} is different files", automationCompare.AutomationTask.ID);
                }
            }

            Logging.Info("Number of matching files: {0}", AutomationCompareTracker.NumMatches);
            Logging.Info("Number of different files: {0}", AutomationCompareTracker.NumDifferences);

            if (ProcessTaskResultTrue(AutomationCompareTracker.NumDifferences + AutomationCompareTracker.NumMatches == 0, "The number of total compares is 0"))
                return;

            if (ValidateForExit(AutomationCompareTracker.NumDifferences == 0, AutomationExitCode.ComparisonEqualFail, string.Format("There are no differences, no need to continue execution")))
                return;
        }
    }
}
