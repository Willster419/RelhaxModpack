using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class BrowserSessionUrlTask : AutomationTask
    {
        public string Url { get; set; }

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Url) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            Url = ProcessMacro(nameof(Url), Url);
            CreateLastDownloadFilenameMacro();
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(Url), string.Format("The parameter {0} is null or empty", nameof(Url))))
                return;
        }

        protected virtual void CreateLastDownloadFilenameMacro()
        {
            string[] urlSplit = Url.Split('/');
            string urlFilename = urlSplit.Last();
            Logging.Info("Url filename parsed as {0}", urlFilename);

            Logging.Info("Creating macro, Name: {0}, Value: {1}", "last_download_filename", urlFilename);
            AutomationMacro lastDownloadFilenameMacro = Macros.Find(mac => mac.Name.Equals("last_download_filename"));
            if (lastDownloadFilenameMacro == null)
                lastDownloadFilenameMacro = new AutomationMacro() { MacroType = MacroType.Local, Name = "last_download_filename", Value = urlFilename };
            Macros.Add(lastDownloadFilenameMacro);
        }
        #endregion
    }
}
