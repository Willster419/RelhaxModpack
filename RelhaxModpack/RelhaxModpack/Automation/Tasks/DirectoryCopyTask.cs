using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities;
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
    /// Searches for a list of files to copy and copies them from the source to the destination.
    /// </summary>
    /// <remarks>The paths are relative to DirectoryPath and DestinationPath. In other words, {DirectoryPath}\folder\file.png will be copied to {DestinationPath}\folder\file.png</remarks>
    public class DirectoryCopyTask : DirectoryDestinationTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_copy";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// Flag to indicate if all found files were deleted or not.
        /// </summary>
        protected bool good = false;

        /// <summary>
        /// Flag to control if progress of the delete operation will be reported to the database automation runner window.
        /// </summary>
        protected bool reportingProgress { get { return DatabaseAutomationRunner != null; } }

        /// <summary>
        /// The object to hold progress report information.
        /// </summary>
        protected RelhaxProgress relhaxProgress;

        /// <summary>
        /// The implementation to report progress to a subscribing member.
        /// </summary>
        protected Progress<RelhaxProgress> progress;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        protected CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The implementation to copy files and their metadata from one location to another.
        /// </summary>
        protected FileCopier fileCopier;

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
            if (searchResults == null || searchResults.Count() == 0)
                return;

            cancellationTokenSource = new CancellationTokenSource();

            if (reportingProgress)
            {
                relhaxProgress = new RelhaxProgress()
                {
                    ChildCurrentProgress = "barChildTextParent",
                    ChildCurrent = 0,
                    ChildTotal = 0,
                    ParrentCurrent = 0,
                    ParrentTotal = 0
                };

                progress = new Progress<RelhaxProgress>();

                progress.ProgressChanged += DatabaseAutomationRunner.RelhaxProgressChanged;
            }

            await Task.Run(async () =>
            {
                try
                {
                    fileCopier = new FileCopier(relhaxProgress)
                    {
                        CancellationToken = this.cancellationTokenSource.Token,
                        Reporter = this.progress
                    };

                    if (reportingProgress)
                        relhaxProgress.ParrentTotal = searchResults.Count();

                    //copy each file over
                    foreach (string sourceFile in searchResults)
                    {
                        if (reportingProgress)
                        {
                            relhaxProgress.ParrentCurrent++;
                            (progress as IProgress<RelhaxProgress>).Report(relhaxProgress);
                        }

                        string destinationFile = sourceFile.Replace(DirectoryPath, DestinationPath);
                        string destinationPath = Path.GetDirectoryName(destinationFile);
                        if (!Directory.Exists(destinationPath))
                            Directory.CreateDirectory(destinationPath);

                        bool result = await fileCopier.CopyFileAsync(sourceFile, destinationFile);
                        if (!result)
                            return;
                    }

                    if (reportingProgress)
                    {
                        relhaxProgress.ParrentTotal = relhaxProgress.ParrentCurrent;
                        (progress as IProgress<RelhaxProgress>).Report(relhaxProgress);
                    }

                    good = true;
                }
                catch (OperationCanceledException)
                {
                    good = false;
                    return;
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex.ToString());
                    good = false;
                    return;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                    if (reportingProgress)
                    {
                        progress.ProgressChanged -= DatabaseAutomationRunner.RelhaxProgressChanged;
                    }
                }
            });
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(good, "The copy process failed"))
                return;
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
