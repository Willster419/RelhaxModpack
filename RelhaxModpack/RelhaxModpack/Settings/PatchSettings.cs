using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{
    public class PatchSettings
    {

        public bool SaveSelectionBeforeLeave = false;

        public bool SwitchToLogWhenTestingPatch = true;

        public string AppMacro = string.Empty;

        public string VersiondirMacro = string.Empty;

        public ApplyBehavior ApplyBehavior = ApplyBehavior.Default;

        public PatchSettings() { }
    }
}
