using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class BrowserSessionSetHeaderTask : BrowserSessionHeaderTask
    {
        public const string TaskCommandName = "browser_session_set_header";

        public override string Command { get { return TaskCommandName; } }

        public string Value { get; set; }

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Value) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            Value = ProcessMacro(nameof(Value), Value);
        }

        public override async Task RunTask()
        {
            Logging.Info(Utilities.Enums.LogOptions.ClassName, "Setting header name: '{0}'", Name);
            Logging.Info(Utilities.Enums.LogOptions.ClassName, "Setting header value: '{0}'", Value);
            BrowserSessionManager.SetHeader(Name, Value);
        }

        public override void ProcessTaskResults()
        {
            //this method intentionally left blank
        }
        #endregion
    }
}
