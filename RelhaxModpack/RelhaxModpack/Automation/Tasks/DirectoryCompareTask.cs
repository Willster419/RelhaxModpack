using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Searches for a list of files to compare and compares their integrity using the MD5 hash algorithm, and comparison method NoMatchContinue.
    /// If the number or names of files between directories don't match. It is considered an error.
    /// </summary>
    /// <seealso cref="compareMode"/>
    /// <seealso cref="AutomationCompareMode"/>
    public class DirectoryCompareTask : DirectorySearchTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_compare";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The path to directory A to compare.
        /// </summary>
        public string DirectoryComparePathA { get; set; }

        /// <summary>
        /// The path to directory B to compare.
        /// </summary>
        public string DirectoryComparePathB { get; set; }

        /// <summary>
        /// The list of found files from directory A.
        /// </summary>
        protected string[] directoryFilesA;

        /// <summary>
        /// The list of found files from directory B.
        /// </summary>
        protected string[] directoryFilesB;

        /// <summary>
        /// Flag to indicate if all files were hashed or not.
        /// </summary>
        protected bool operationFinished = false;

        /// <summary>
        /// The implementation to compare files from directories A and B.
        /// </summary>
        protected FileHashComparer fileHashComparer;

        /// <summary>
        /// The object to hold progress report information.
        /// </summary>
        protected RelhaxProgress relhaxProgress;

        /// <summary>
        /// The implementation to report progress to a subscribing member.
        /// </summary>
        protected Progress<RelhaxProgress> calculationProgress;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        protected CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The comparison mode to use for the compare operations.
        /// </summary>
        protected AutomationCompareMode compareMode = AutomationCompareMode.NoMatchContinue;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Except(new string[] { nameof(DirectoryPath) }).Concat(new string[] { nameof(DirectoryComparePathA), nameof(DirectoryComparePathB) }).ToArray();
        }
        #endregion

        #region Task Execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            DirectoryPath = DirectoryComparePathA;
            base.ProcessMacros();
            DirectoryComparePathA = ProcessMacro(nameof(DirectoryComparePathA), DirectoryComparePathA);
            DirectoryComparePathB = ProcessMacro(nameof(DirectoryComparePathB), DirectoryComparePathB);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(DirectoryComparePathA), DirectoryComparePathA))
                return;
            if (ValidateCommandStringNullEmptyTrue(nameof(DirectoryComparePathB), DirectoryComparePathB))
                return;

            if (ValidateCommandFalse(Directory.Exists(DirectoryComparePathA), string.Format("DirectoryComparePathA of {0} file does not exist", DirectoryComparePathA)))
                return;
            if (ValidateCommandFalse(Directory.Exists(DirectoryComparePathB), string.Format("DirectoryComparePathB of {0} file does not exist", DirectoryComparePathB)))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            //base.RunTask() will run RunSearch()
            await base.RunTask();

            //check that the same number of files was returned to check for hashing
            if (directoryFilesA.Length != directoryFilesB.Length)
            {
                Logging.Error("The number of files between directory a ({0}) and directory b ({1}) do not match", directoryFilesA.Length, directoryFilesB.Length);
                return;
            }

            //check that the relative paths match between the folders
            for (int i = 0; i < directoryFilesA.Length; i++)
            {
                string relativePathA = directoryFilesA[i].Replace(DirectoryComparePathA, string.Empty);
                string relativePathB = directoryFilesB[i].Replace(DirectoryComparePathB, string.Empty);

                if (!relativePathA.Equals(relativePathB))
                {
                    Logging.Error("At index {0}, relative path a ({1}) does not match relative path b ({2})", i, relativePathA, relativePathB);
                    return;
                }
            }

            //actually run the hashing
            operationFinished = await RunFileHashing();
        }

        /// <summary>
        /// Runs the file hashing implementation between files list A and B, optionally reporting progress.
        /// </summary>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        protected virtual async Task<bool> RunFileHashing()
        {
            calculationProgress = new Progress<RelhaxProgress>();
            cancellationTokenSource = new CancellationTokenSource();
            relhaxProgress = new RelhaxProgress() { ChildCurrentProgress = "barWithTextChild", ChildCurrent = 0, ChildTotal = directoryFilesA.Length };
            fileHashComparer = new FileHashComparer()
            {
                CancellationTokenA = cancellationTokenSource.Token,
                CancellationTokenB = cancellationTokenSource.Token,
            };

            if (DatabaseAutomationRunner != null)
            {
                calculationProgress.ProgressChanged += DatabaseAutomationRunner.RelhaxProgressChanged;
            }

            try
            {
                for (int i = 0; i < directoryFilesA.Length; i++)
                {
                    await fileHashComparer.ComputeHashA(directoryFilesA[i]);
                    await fileHashComparer.ComputeHashB(directoryFilesB[i]);

                    string fileAHash = fileHashComparer.HashAStringBuilder?.ToString();
                    string fileBHash = fileHashComparer.HashBStringBuilder?.ToString();
                    Logging.Debug("File A hash: {0}", fileAHash);
                    Logging.Debug("File B hash: {0}", fileBHash);

                    AutomationCompareManager.AddCompare(this,
                        new AutomationCompareFile(DirectoryComparePathA, DirectoryComparePathB, compareMode, directoryFilesA[i], fileAHash, directoryFilesB[i], fileBHash));
                    relhaxProgress.ChildCurrent++;
                    (calculationProgress as IProgress<RelhaxProgress>).Report(relhaxProgress);

                    if (cancellationTokenSource.Token.IsCancellationRequested)
                        cancellationTokenSource.Cancel();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }
            finally
            {
                cancellationTokenSource.Dispose();
                if (DatabaseAutomationRunner != null)
                {
                    calculationProgress.ProgressChanged -= DatabaseAutomationRunner.RelhaxProgressChanged;
                }
            }
            return true;
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(operationFinished, "The operation failed to finish, check the above log messages"))
                return;
        }

        /// <summary>
        /// Runs the search on directories A and B to find files to compare.
        /// </summary>
        protected override void RunSearch()
        {
            directoryFilesA = FileUtils.FileSearch(DirectoryComparePathA, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, false, true, SearchPattern);
            directoryFilesB = FileUtils.FileSearch(DirectoryComparePathB, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, false, true, SearchPattern);
        }

        /// <summary>
        /// Sends a cancellation request to task's current operation.
        /// </summary>
        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
