using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A FileCompareTask allows for an MD5 comparison of two files to determine their contents are equal.
    /// </summary>
    /// <remarks>If the files are found to not be equal, the comparison is reported as type NoMatchStop. Great for configuration files that have patches written for, that should be manually inspected when they change.</remarks>
    /// <seealso cref="AutomationCompareMode"/>
    public class FileCompareInverseTask : FileCompareTask, IXmlSerializable
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public new const string TaskCommandName = "file_compare_inverse";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await base.RunTask();
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultTrue(!fileHashComparer.HashACalculated, "Hash A failed to calculate"))
                return;
            if (ProcessTaskResultTrue(!fileHashComparer.HashBCalculated, "Hash B failed to calculate"))
                return;

            //getting to here means that successful hashes were calculated
            AutomationCompareFile compareFile = new AutomationCompareFile(Path.GetDirectoryName(FileA), Path.GetDirectoryName(FileB), AutomationCompareMode.NoMatchStop, Path.GetFileName(FileA), fileAHash, Path.GetFileName(FileB), fileBHash);
            AutomationCompareManager.AddCompare(this, compareFile);
        }
        #endregion
    }
}
