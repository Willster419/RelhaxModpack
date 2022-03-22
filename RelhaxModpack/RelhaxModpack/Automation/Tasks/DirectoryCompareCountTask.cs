using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class DirectoryCompareCountTask : DirectoryCompareTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_compare_count";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

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
            RunSearch();

            //check that the same number of files was returned to check for hashing
            if (directoryFilesA.Length != directoryFilesB.Length)
            {
                Logging.Info("The number of files between directory a ({0}) and directory b ({1}) do not match, update needed", directoryFilesA.Length, directoryFilesB.Length);
                AutomationCompareManager.AddCompare(this, new AutomationCompareDirectory(DirectoryComparePathA, DirectoryComparePathB, AutomationCompareMode.NoMatchContinue, directoryFilesA.Length, directoryFilesB.Length));
                operationFinished = true;
                return;
            }

            //check that the relative paths match between the folders
            for (int i = 0; i < directoryFilesA.Length; i++)
            {
                string relativePathA = directoryFilesA[i].Replace(DirectoryComparePathA, "");
                string relativePathB = directoryFilesB[i].Replace(DirectoryComparePathB, "");

                if (!relativePathA.Equals(relativePathB))
                {
                    Logging.Info("At index {0}, relative path a ({1}) does not match relative path b ({2})", i, relativePathA, relativePathB);
                    Logging.Info("The paths have changed, update needed");
                    AutomationCompareManager.AddCompare(this, new AutomationCompareDirectory(DirectoryComparePathA, DirectoryComparePathB, AutomationCompareMode.NoMatchContinue, directoryFilesA.Length, directoryFilesB.Length, i));
                    operationFinished = true;
                    return;
                }
            }

            //actually run the hashing
            operationFinished = await RunFileHashing();
        }

        protected virtual async Task<bool> RunFileHashing()
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

        protected override void RunSearch()
        {
            base.RunSearch();
        }

        public virtual void Cancel()
        {
            base.Cancel();
        }
        #endregion
    }
}
