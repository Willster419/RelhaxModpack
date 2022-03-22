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

        public string IncludeRootInSearch { get; set; }

        protected bool good = false;

        protected bool includeRootInSearch;

        protected bool ableToParseIncludeRootInSearch = false;

        protected bool reportingProgress { get { return DatabaseAutomationRunner != null; } }

        protected RelhaxProgress relhaxProgress;

        protected Progress<RelhaxProgress> progress;

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

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandStringNullEmptyTrue(nameof(IncludeRootInSearch), IncludeRootInSearch))
                return;

            if (ValidateCommandFalse(ableToParseIncludeRootInSearch, string.Format("Unable to parse the arg IncludeRootInSearch from given string {0}", IncludeRootInSearch)))
                return;
        }

        public async override Task RunTask()
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

        protected override void RunSearch()
        {
            searchResults = FileUtils.FileSearch(DirectoryPath, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, includeRootInSearch, true, SearchPattern);
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(good, "The delete process failed"))
                return;
        }

        public virtual void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
