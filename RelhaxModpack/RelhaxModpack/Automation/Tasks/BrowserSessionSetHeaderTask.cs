using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public class BrowserSessionSetHeaderTask : AutomationTask
    {
        public const string TaskCommandName = "browser_session_set_header";

        public override string Command { get { return TaskCommandName; } }

        public string Name { get; set; }

        public string Value { get; set; }

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Name), nameof(Value) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            Name = ProcessMacro(nameof(Name), Name);
            Value = ProcessMacro(nameof(Value), Value);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(Name), string.Format("The parameter {0} is null or empty", nameof(Name))))
                return;
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
