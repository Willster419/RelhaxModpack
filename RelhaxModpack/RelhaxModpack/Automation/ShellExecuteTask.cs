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
        public string Cmd { get; set; } = string.Empty;

        public string Wd { get; set; } = string.Empty;

        public override string Command { get; } = "shell_exec";

        protected Process process = null;
        protected ProcessStartInfo startInfo = null;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Wd), nameof(Cmd) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            Cmd = ProcessMacro(nameof(Cmd), Cmd);
            Wd = ProcessMacro(nameof(Wd), Wd);
        }

        public override void ValidateCommands()
        {
            if (string.IsNullOrEmpty(Wd) || string.IsNullOrEmpty(Cmd))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: The Wd or Cmd is null/empty. DestinationPath: '{1}', Url: '{2}'.", ExitCode, Wd, Cmd);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }

            if (!Directory.Exists(Wd))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: The folder path for Wd does not exist: '{1}'", ExitCode, Wd);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
        }

        public override async Task RunTask()
        {
            //dump vars before run
            Logging.AutomationRunner("Dumping environment variables", LogLevel.Debug);

            //https://stackoverflow.com/a/141098/3128017
            foreach (KeyValuePair<string, string> keyValuePair in process.StartInfo.Environment)
            {
                Logging.AutomationRunner("Key = {0}, Value = {1}", LogLevel.Debug, keyValuePair.Key, keyValuePair.Value);
            }

            //build the task process
            process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = Cmd,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Wd
                },
            };

            //set std error and output redirect to the main window if the event handler isn't null
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            //run in separate task to avoid blocking on UI thread - no async wait
            await Task.Run(() =>
            {
                try
                {
                    if (!process.Start())
                    {
                        ExitCode = 2;
                        ErrorMessage = string.Format("{0} {1}: The task failed to start (Start() returned false)", nameof(ExitCode), ExitCode);
                        Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                    }
                }
                catch(Exception ex)
                {
                    ExitCode = 3;
                    ErrorMessage = string.Format("{0} {1}: The task failed to start (exception){2}{3}", nameof(ExitCode), ExitCode, Environment.NewLine, ex.ToString());
                    Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                }
            });
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logging.Info("[PROCESS INFO]: {0}", e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logging.Info("[PROCESS ERROR]: {0}", e.Data);
        }

        public override void ProcessTaskResults()
        {
            //check error code
            if (process.ExitCode != 0)
            {
                ExitCode = 4;
                ErrorMessage = string.Format("{0} {1}: The process returned exit code {2}", nameof(ExitCode), ExitCode, process.ExitCode);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
            }
        }

        public void Dispose()
        {
            ((IDisposable)process).Dispose();
        }
        #endregion
    }
}
