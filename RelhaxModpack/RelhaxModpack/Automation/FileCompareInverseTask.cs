using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class FileCompareInverseTask : FileCompareTask, IXmlSerializable
    {
        public const string TaskCommandName = "file_compare_inverse";

        public override string Command { get { return TaskCommandName; } }

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
        }

        public override async Task RunTask()
        {
            await base.RunTask();
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultTrue(!fileHashComparer.HashACalculated, "Hash A failed to calculate"))
                return;
            if (ProcessTaskResultTrue(!fileHashComparer.HashBCalculated, "Hash B failed to calculate"))
                return;

            //getting to here means that successful hashes were calculated
            AutomationCompareTracker.AddCompare(this, FileA, fileAHash, FileB, fileBHash, Utilities.Enums.AutomationCompareMode.NoMatchStop);
        }
        #endregion
    }
}
