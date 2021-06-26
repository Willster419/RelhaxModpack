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

        public override string Command { get { return TaskCommandName; } }

        public string DestinationPath { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

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
            //"true" version means that the test being true is "bad"
            if (ValidateCommandTrue(string.IsNullOrEmpty(DestinationPath), string.Format("DestinationPath is null/empty")))
                return;
            if (ValidateCommandTrue(string.IsNullOrEmpty(Url), string.Format("Url is null/empty")))
                return;
        }

        public override async Task RunTask()
        {
            DownloadSetup();

            await DownloadFile();
        }

        protected void DownloadSetup()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Verifying that destination path ({0}) exists and deleting file if exists", DestinationPath);
            string directoryPath = Path.GetDirectoryName(DestinationPath);
            if ((!string.IsNullOrWhiteSpace(directoryPath)) && (!Directory.Exists(directoryPath)))
                Directory.CreateDirectory(directoryPath);
            if (File.Exists(DestinationPath))
                File.Delete(DestinationPath);
        }

        protected async Task DownloadFile()
        {
            using (WebClient = new WebClient())
            {
                Logging.Info(Logfiles.AutomationRunner, "Downloading file");
                Logging.Debug(Logfiles.AutomationRunner, "Download url = {0}, file = {1}", Url, DestinationPath);
                //https://stackoverflow.com/questions/2953403/c-sharp-passing-method-as-the-argument-in-a-method
                if (DatabaseAutomationRunner != null)
                {
                    WebClient.DownloadProgressChanged += DatabaseAutomationRunner.DownloadProgressChanged;
                    WebClient.DownloadFileCompleted += DatabaseAutomationRunner.DownloadFileCompleted;
                }
                try
                {
                    await WebClient.DownloadFileTaskAsync(Url, DestinationPath);
                }
                catch (WebException wex)
                {
                    Logging.Exception(wex.ToString());
                }
                finally
                {
                    if (DatabaseAutomationRunner != null)
                    {
                        WebClient.DownloadProgressChanged -= DatabaseAutomationRunner.DownloadProgressChanged;
                        WebClient.DownloadFileCompleted -= DatabaseAutomationRunner.DownloadFileCompleted;
                    }
                }
                
            }
        }

        public override void ProcessTaskResults()
        {
            //"false" version means that the test being false is "bad"
            if (ProcessTaskResultFalse(File.Exists(DestinationPath), string.Format("The file {0} was not detected to exist", DestinationPath)))
                return;
        }
        #endregion
    }
}
