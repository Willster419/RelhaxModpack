using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public abstract class MacroStringTask : MacroTask
    {
        protected string stringReturnValue { get; set; } = string.Empty;

        public override void ProcessTaskResults()
        {
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroName));
            if (ProcessTaskResultTrue(macro == null, "Could not find newly created macro in list"))
                return;
        }
    }
}
