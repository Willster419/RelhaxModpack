using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class MacroDeleteTask : MacroTask
    {
        public const string TaskCommandName = "macro_delete";

        public override string Command { get { return TaskCommandName; } }

        public override async Task RunTask()
        {
            Logging.Info("Deleting macro {0}", MacroName);
            await base.RunTask();
            Logging.Info("Deleted");
        }

        public override void ProcessTaskResults()
        {
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroName));
            if (macro != null)
                throw new BadMemeException("you have made a mistake");
        }
    }
}
