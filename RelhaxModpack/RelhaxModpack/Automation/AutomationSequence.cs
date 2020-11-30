using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class AutomationSequence
    {
        private List<AutomationMacro> LocalMacroList = null;

        private AutomationSequencer AutomationSequencer = null;

        private List<AutomationMacro> GlobalMacros { get { return AutomationSequencer.GlobalMacros; } }

        private List<AutomationTask> AutomationTasks = null;

        private DatabasePackage Package = null;
    }
}
