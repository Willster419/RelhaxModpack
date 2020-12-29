using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RelhaxModpack.Automation
{
    public class DownloadStaticTask : AutomationTask, IDownloadTask, IXmlSerializable
    {
        public const string TaskCommandName = "download_static";

        public string DestinationPath { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public override string Command { get { return TaskCommandName; } }

        protected WebClient WebClient = null;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationPath), nameof(Url) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            DestinationPath = ProcessMacro(nameof(DestinationPath), DestinationPath);
            Url = ProcessMacro(nameof(Url), Url);
        }

        public override void ValidateCommands()
        {
            if (string.IsNullOrEmpty(DestinationPath) || string.IsNullOrEmpty(Url))
            {
                ExitCode = 1;
                ErrorMessage = string.Format("ExitCode {0}: The DestinationPath or Url is null/empty. DestinationPath: '{1}', Url: '{2}'.", ExitCode, DestinationPath, Url);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
                return;
            }
        }

        public override async Task RunTask()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Verifying that destination path ({0}) exists and deleting file if exists", DestinationPath);
            string directoryPath = Path.GetDirectoryName(DestinationPath);
            if ((!string.IsNullOrWhiteSpace(directoryPath)) && (!Directory.Exists(directoryPath)))
                Directory.CreateDirectory(directoryPath);
            if (File.Exists(DestinationPath))
                File.Delete(DestinationPath);

            using (WebClient = new WebClient())
            {
                Logging.Info(Logfiles.AutomationRunner, "Downloading file");
                Logging.Debug(Logfiles.AutomationRunner, "Download url = {0}, file = {1}", Url, DestinationPath);
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                if (DatabaseAutomationRunner != null)
                    WebClient.DownloadProgressChanged += DatabaseAutomationRunner.DownloadProgressChanged;
                await WebClient.DownloadFileTaskAsync(Url, DestinationPath);
                if (DatabaseAutomationRunner != null)
                    WebClient.DownloadProgressChanged -= DatabaseAutomationRunner.DownloadProgressChanged;
            }
        }

        public override void ProcessTaskResults()
        {
            if (!File.Exists(DestinationPath))
            {
                ExitCode = 2;
                ErrorMessage = string.Format("ExitCode {0}: The file '{0}' was not detected to exist", ExitCode, DestinationPath);
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, ErrorMessage);
            }
        }
        #endregion
    }
}
