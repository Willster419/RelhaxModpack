using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class BrowserSessionGetTask : BrowserSessionParseTask, IHtmlParseTask
    {
        public const string TaskCommandName = "browser_session_get_request";

        public override string Command { get { return TaskCommandName; } }

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();
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
                htmlText = await BrowserSessionManager.GetRequestStringAsync(Url);
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
