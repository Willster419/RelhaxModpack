using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class StartBrowserSessionTask : AutomationTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "browser_session_start";

        public override string Command { get { return TaskCommandName; } }

        public string Browser { get; set; } = BrowserSessionType.WebClient.ToString();

        protected BrowserSessionType browserEngine = BrowserSessionType.WebClient;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Browser) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            browserEngine = (BrowserSessionType)Enum.Parse(typeof(BrowserSessionType), ProcessMacro(nameof(Browser), Browser));
        }

        public override void ValidateCommands()
        {
            //this method intentionally left blank
        }

        public override async Task RunTask()
        {
            Logging.Info("Starting browser compare session");
            AutomationSequence.ResetBrowserSessionManager(browserEngine);
        }

        public override void ProcessTaskResults()
        {
            //this method intentionally left blank
        }
        #endregion
    }
}
