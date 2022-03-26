using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Searches for a list of files to compare and compares their integrity using the MD5 hash algorithm, and comparison method NoMatchStop.
    /// If the number or names of files between directories don't match. It is considered an error.
    /// </summary>
    /// <seealso cref="compareMode"/>
    /// <seealso cref="AutomationCompareMode"/>
    public class DirectoryCompareInverseTask : DirectoryCompareTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public new const string TaskCommandName = "directory_compare_inverse";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The comparison mode to use for the compare operations.
        /// </summary>
        protected new AutomationCompareMode compareMode = AutomationCompareMode.NoMatchStop;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes();
        }
        #endregion

        #region Task Execution
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
        /// Runs the file hashing implementation between files list A and B, optionally reporting progress.
        /// </summary>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        protected override async Task<bool> RunFileHashing()
        {
            return await base.RunFileHashing();
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }

        /// <summary>
        /// Runs the search on directories A and B to find files to compare.
        /// </summary>
        protected override void RunSearch()
        {
            base.RunSearch();
        }

        /// <summary>
        /// Sends a cancellation request to task's current operation.
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
        }
        #endregion
    }
}
