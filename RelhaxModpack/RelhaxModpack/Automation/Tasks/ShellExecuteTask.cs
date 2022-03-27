using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using RelhaxModpack.UI;
using System.Threading;
using System.Windows.Threading;
using ThreadState = System.Threading.ThreadState;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A ShellExecuteTask allows the launching of a shell process to run any command.
    /// </summary>
    public class ShellExecuteTask : AutomationTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "shell_exec";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The command arguments sent into the shell process.
        /// </summary>
        public string Cmd { get; set; } = string.Empty;

        /// <summary>
        /// The working directory where the shell will launch from.
        /// </summary>
        public string Wd { get; set; } = string.Empty;

        /// <summary>
        /// The binary file or shell command to execute.
        /// </summary>
        /// <remarks>This can either be an executable (with or without path) specified in your PATH environment variable, or an absolute path to an executable.</remarks>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// The process reference.
        /// </summary>
        protected Process process = null;

        /// <summary>
        /// The process start info reference.
        /// </summary>
        protected ProcessStartInfo startInfo = null;

        /// <summary>
        /// Flag to indicate if the process was able to launch.
        /// </summary>
        protected bool processStarted = false;

        /// <summary>
        /// Flag to indicate if the process was able to close without error. This does NOT include the return code from the process.
        /// </summary>
        protected bool processCompleted = false;

        /// <summary>
        /// The returned exit code from the shell process.
        /// </summary>
        protected int exitCode;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Wd), nameof(Cmd), nameof(Filename) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            Cmd = ProcessMacro(nameof(Cmd), Cmd);
            Wd = ProcessMacro(nameof(Wd), Wd);
            Filename = ProcessMacro(nameof(Filename), Filename);

            Cmd = ProcessEscapeCharacters(nameof(Cmd), Cmd);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(Wd), "Wd is empty string"))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(Cmd), "Cmd is empty string"))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(Filename), "Filename is empty string"))
                return;

            if (ValidateCommandTrue(!Directory.Exists(Wd), string.Format("The folder path for Wd does not exist: '{0}'", Wd)))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            //build the task process
            process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = Cmd,
                    //Setting this property to false enables you to redirect input, output, and error streams
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Wd,
                    FileName = this.Filename,
                    CreateNoWindow = true
                },
            };

            Logging.Info("Running shell execution");
            Logging.Info("Filename: {0}", Filename);
            Logging.Info("Working Directory: {0}", Wd);
            Logging.Info("Args {0}", Cmd);
            await Task.Run(() =>
            {
                try
                {
                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.ErrorDataReceived += Process_ErrorDataReceived;
                    processStarted = process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                    processCompleted = true;
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    if (ExitCode != AutomationExitCode.Cancel)
                    {
                        Logging.Exception(ex.ToString());
                        processStarted = false;
                    }
                }

                process.OutputDataReceived -= Process_OutputDataReceived;
                process.ErrorDataReceived -= Process_ErrorDataReceived;

                if (processCompleted)
                    exitCode = process.ExitCode;
                else
                    exitCode = -1;

                process.Dispose();
            });
            Logging.Info("Shell execution finishes");
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            Logging.Info("[PROCESS ERROR]: {0}", e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            Logging.Info("[PROCESS INFO]: {0}", e.Data);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(processStarted, "The process failed to start"))
                return;

            //check error code
            if (ProcessTaskResultTrue(exitCode != 0, string.Format("The process returned exit code {0}", exitCode)))
                return;
        }

        /// <summary>
        /// Sends a cancellation request to task's current operation.
        /// </summary>
        public virtual void Cancel()
        {
            ExitCode = AutomationExitCode.Cancel;
            if (process != null)
            {
                process.Kill();
                process.Dispose();
            }    
        }
        #endregion
    }
}
