using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RelhaxModpack.Automation
{
    public class ShellExecuteTask : AutomationTask, IXmlSerializable, IDisposable
    {
        public const string TaskCommandName = "shell_exec";

        public override string Command { get { return TaskCommandName; } }

        public string Cmd { get; set; } = string.Empty;

        public string Wd { get; set; } = string.Empty;

        public string Filename { get; set; } = string.Empty;

        protected Process process = null;

        protected ProcessStartInfo startInfo = null;

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

            //dump vars before run
            Logging.AutomationRunner("Dumping current shell environment variables", LogLevel.Debug);

            //https://stackoverflow.com/a/141098/3128017
            foreach (KeyValuePair<string, string> keyValuePair in process.StartInfo.Environment)
            {
                Logging.AutomationRunner("Key = {0}, Value = {1}", LogLevel.Debug, keyValuePair.Key, keyValuePair.Value);
            }

            //log the command
            Logging.Info("Running Filename {0} in work directory {1} with args {1}", Filename, Wd, Cmd);

            //set std error and output redirect to the main window if the event handler isn't null
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            //run in separate task to avoid blocking on UI thread - no async wait
            await Task.Run(() =>
            {
                bool processStarted = true;
                try
                {
                    processStarted = process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
                catch(Exception ex)
                {
                    Logging.Exception(ex.ToString());
                    processStarted = false;
                }

                if (ValidateForExitTrueNew(!processStarted, AutomationExitCode.ShellFail, "The process failed to start"))
                    return;
            });

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
            //check error code
            if (ProcessTaskResultTrue(process.ExitCode != 0, string.Format("The process returned exit code {0}", process.ExitCode)))
                return;
        }

        public void Dispose()
        {
            process.OutputDataReceived -= Process_OutputDataReceived;
            process.ErrorDataReceived -= Process_ErrorDataReceived;
            ((IDisposable)process).Dispose();
        }
        #endregion
    }
}
