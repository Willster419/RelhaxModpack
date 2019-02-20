using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{
    public enum ApplyBehavior
    {
        Default,
        ApplyTriggersSave,
        SaveTriggersApply
    }

    public class EditorSettings
    {

        public string WotmodsUsername = string.Empty;

        public string WotmodsPassword = string.Empty;

        public string BigmodsUsername = string.Empty;

        public string BigmodsPassword = string.Empty;

        public bool SaveSelectionBeforeLeave = false;

        public ApplyBehavior ApplyBehavior = ApplyBehavior.Default;

        public EditorSettings() { }
    }
}
