using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Performs a file download on a url resource.
    /// </summary>
    public class DownloadStaticTask : AutomationTask, IXmlSerializable, ICancelOperation
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "download_static";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The destination of where to download the file to, including the file name. If the path to the file doesn't exist, then it will be created.
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        /// <summary>
        /// The url to download the resource from.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The WebClient to use for downloading the resultant url to disk.
        /// </summary>
        protected WebClient WebClient = null;

        /// <summary>
        /// Temporary variable to parse the DestinationPath argument and determine (if there's a macro in it) if the macro resolves to an empty string.
        /// </summary>
        /// <remarks>This temporary variable is required because during the task's execution, it will create a macro "last_download_filename". That may be inside this argument and thus will need to be parsed later.</remarks>
        /// <seealso cref="GetDownloadUrlFilename"/>
        protected string destinationPathTemp;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(DestinationPath), nameof(Url) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            destinationPathTemp = ProcessMacro(nameof(DestinationPath), DestinationPath);
            Url = ProcessMacro(nameof(Url), Url);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            //"true" version means that the test being true is "bad"
            if (ValidateCommandTrue(string.IsNullOrEmpty(Url), string.Format("Url is null/empty")))
                return;
            if (ValidateCommandTrue((string.IsNullOrEmpty(DestinationPath) && string.IsNullOrEmpty(destinationPathTemp)), string.Format("DestinationPath is null/empty")))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            await DownloadFile();
        }

        /// <summary>
        /// Prepares the destination path for the download of the resource.
        /// </summary>
        protected void DownloadSetup()
        {
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.MethodName, "Verifying that destination path ({0}) exists and deleting file if exists", DestinationPath);
            string directoryPath = Path.GetDirectoryName(DestinationPath);
            if ((!string.IsNullOrWhiteSpace(directoryPath)) && (!Directory.Exists(directoryPath)))
                Directory.CreateDirectory(directoryPath);
            if (File.Exists(DestinationPath))
                File.Delete(DestinationPath);
        }

        /// <summary>
        /// Downloads the file from the url to the location on disk.
        /// </summary>
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
                    GetDownloadUrlFilename();
                    DownloadSetup();
                    await WebClient.DownloadFileTaskAsync(Url, DestinationPath);
                }
                catch (OperationCanceledException) { }
                catch (WebException wex)
                {
                    if (wex.Status != WebExceptionStatus.RequestCanceled)
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

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            //"false" version means that the test being false is "bad"
            if (ProcessTaskResultFalse(File.Exists(DestinationPath), string.Format("The file {0} was not detected to exist", DestinationPath)))
                return;
        }

        /// <summary>
        /// Parses the url argument for the file name of the resource to download. The macro "last_download_filename" is created with the parsed name.
        /// </summary>
        /// <seealso cref="Url"/>
        protected virtual void GetDownloadUrlFilename()
        {
            string[] urlSplit = Url.Split('/');
            string urlFilename = urlSplit.Last();
            Logging.Info("Url filename parsed as {0}", urlFilename);

            Logging.Info("Creating macro, Name: {0}, Value: {1}", "last_download_filename", urlFilename);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = "last_download_filename", Value = urlFilename });

            DestinationPath = ProcessMacro(nameof(DestinationPath), DestinationPath);
        }

        /// <summary>
        /// Cancels the download operation.
        /// </summary>
        public virtual void Cancel()
        {
            if (WebClient != null)
                WebClient.CancelAsync();
        }
        #endregion
    }
}
