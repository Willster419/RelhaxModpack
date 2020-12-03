using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// An AutomationSequence class is a container to store a list of Automation tasks that pertain to a package.
    /// </summary>
    public class AutomationSequence
    {
        private List<AutomationMacro> LocalMacroList = null;

        private AutomationSequencer AutomationSequencer = null;

        private List<AutomationMacro> GlobalMacros { get { return AutomationSequencer.GlobalMacros; } }

        private AutomationRunnerSettings AutomationRunnerSettings { get { return AutomationSequencer.AutomationRunnerSettings; } }

        private List<AutomationTask> AutomationTasks = null;

        private DatabasePackage Package = null;
    }
}
