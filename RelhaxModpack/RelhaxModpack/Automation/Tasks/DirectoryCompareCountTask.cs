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
        public const string TaskCommandName = "directory_compare_count";

        public override string Command { get { return TaskCommandName; } }

        #region Xml Serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes();
        }
        #endregion

        #region Task Execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
        }

        public async override Task RunTask()
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
