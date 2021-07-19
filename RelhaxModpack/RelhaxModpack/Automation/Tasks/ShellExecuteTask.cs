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
    public class ShellExecuteTask : AutomationTask, IXmlSerializable, ICancelOperation
    {
        public const string TaskCommandName = "shell_exec";

        public override string Command { get { return TaskCommandName; } }

        public string Cmd { get; set; } = string.Empty;

        public string Wd { get; set; } = string.Empty;

        public string Filename { get; set; } = string.Empty;

        protected Process process = null;

        protected ProcessStartInfo startInfo = null;

        protected bool processStarted;

        protected int exitCode;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Wd), nameof(Cmd), nameof(Filename) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            Cmd = ProcessMacro(nameof(Cmd), Cmd);
            Wd = ProcessMacro(nameof(Wd), Wd);
            Filename = ProcessMacro(nameof(Filename), Filename);

            Cmd = ProcessEscapeCharacters(nameof(Cmd), Cmd);
        }

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
                    processStarted = true;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
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
                exitCode = process.ExitCode;
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

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(processStarted, "The process failed to start"))
                return;

            //check error code
            if (ProcessTaskResultTrue(exitCode != 0, string.Format("The process returned exit code {0}", exitCode)))
                return;
        }

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
