using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class BrowserSessionPostTask: BrowserSessionParseTask, IHtmlParseTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "browser_session_post_request";

        public override string Command { get { return TaskCommandName; } }

        public string PostData { get; set; }

        public string ContentType { get; set; }

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(PostData), nameof(ContentType) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            PostData = ProcessMacro(nameof(PostData), PostData);
            ContentType = ProcessMacro(nameof(ContentType), ContentType);
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(PostData), PostData))
                return;
            if (ValidateCommandStringNullEmptyTrue(nameof(ContentType), ContentType))
                return;
        }

        public override async Task RunTask()
        {
            await base.RunTask();
        }

        public override void ProcessTaskResults()
        {
            base.ProcessTaskResults();
        }

        protected override async Task<bool> GetHtmlString()
        {
            try
            {
                htmlText = await BrowserSessionManager.PostRequestStringAsync(Url, PostData, ContentType);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }
        }
        #endregion
    }
}
