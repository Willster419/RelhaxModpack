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
    /// Searches for a list of files to delete and deletes them. Optionally deletes the root folder where the operation started from.
    /// </summary>
    public class DirectoryDeleteTask : DirectorySearchTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_delete";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// Flag to determine if the root directory of the search should be deleted as well.
        /// </summary>
        public string IncludeRootInSearch { get; set; }

        /// <summary>
        /// Flag to indicate if all found files were deleted or not.
        /// </summary>
        protected bool good = false;

        /// <summary>
        /// Parsed result of the argument IncludeRootInSearch.
        /// </summary>
        /// <seealso cref="IncludeRootInSearch"/>
        protected bool includeRootInSearch;

        /// <summary>
        /// Flag to determine if the task was able to parse the IncludeRootInSearch option.
        /// </summary>
        /// <seealso cref="IncludeRootInSearch"/>
        protected bool ableToParseIncludeRootInSearch = false;

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

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(IncludeRootInSearch) }).ToArray();
        }
        #endregion

        #region Task Execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();

            IncludeRootInSearch = ProcessMacro(nameof(IncludeRootInSearch), IncludeRootInSearch);

            if (bool.TryParse(IncludeRootInSearch, out bool result))
            {
                ableToParseIncludeRootInSearch = true;
                includeRootInSearch = result;
            }
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(IncludeRootInSearch), IncludeRootInSearch))
                return;

            if (ValidateCommandFalse(ableToParseIncludeRootInSearch, string.Format("Unable to parse The argument IncludeRootInSearch from given string {0}", IncludeRootInSearch)))
                return;
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
                    ChildCurrentProgress = "barWithTextChild",
                    ChildCurrent = 0,
                    ChildTotal = 0,
                    ParrentCurrent = 0,
                    ParrentTotal = 0
                };

                progress = new Progress<RelhaxProgress>();

                progress.ProgressChanged += DatabaseAutomationRunner.RelhaxProgressChanged;
            }

            await Task.Run(() =>
            {
                try
                {
                    if (reportingProgress)
                        relhaxProgress.ChildTotal = searchResults.Count();

                    Logging.Debug("Deleting files");
                    foreach (string file in searchResults)
                    {
                        if (reportingProgress)
                        {
                            relhaxProgress.ChildCurrent++;
                            (progress as IProgress<RelhaxProgress>).Report(relhaxProgress);
                        }

                        bool result = true;
                        if (File.Exists(file))
                            result = FileUtils.FileDelete(file);
                        if (!result)
                            return;
                    }

                    if (reportingProgress)
                    {
                        relhaxProgress.ChildTotal = relhaxProgress.ChildCurrent;
                        (progress as IProgress<RelhaxProgress>).Report(relhaxProgress);
                    }

                    Logging.Debug("Deleting empty directories");
                    FileUtils.ProcessEmptyDirectories(DirectoryPath, true);

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
        /// Runs the search for files to delete.
        /// </summary>
        protected override void RunSearch()
        {
            searchResults = FileUtils.FileSearch(DirectoryPath, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, includeRootInSearch, true, SearchPattern);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(good, "The delete process failed"))
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
