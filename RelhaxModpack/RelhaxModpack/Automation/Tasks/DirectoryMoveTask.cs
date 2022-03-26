using RelhaxModpack.Database;
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
    /// Gives the ability to move a file within a drive, instead of a copy-modify-delete operation.
    /// </summary>
    public class DirectoryMoveTask : DirectoryDestinationTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "directory_move";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// Flag to determine if the move operation succeeded or not.
        /// </summary>
        protected bool good = false;

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

            await Task.Run(() =>
            {
                string lastSourceFile = string.Empty, lastDestinationFile = string.Empty;
                try
                {
                    //move each file over
                    foreach (string sourceFile in searchResults)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                            throw new OperationCanceledException(cancellationTokenSource.Token);

                        string destinationFile = sourceFile.Replace(DirectoryPath, DestinationPath);
                        string destinationPath = Path.GetDirectoryName(destinationFile);
                        if (!Directory.Exists(destinationPath))
                            Directory.CreateDirectory(destinationPath);

                        lastSourceFile = sourceFile;
                        lastDestinationFile = destinationFile;
                        File.Move(sourceFile, destinationFile);
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
                    Logging.Error("Failed moving file from {0} to {1}", lastSourceFile, lastDestinationFile);
                    good = false;
                    return;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            });
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(good, "The move process failed"))
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
