using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class BrowserSessionHeaderTask : AutomationTask
    {
        public string Name { get; set; }

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(Name) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            Name = ProcessMacro(nameof(Name), Name);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(Name), string.Format("The parameter {0} is null or empty", nameof(Name))))
                return;
        }
        #endregion
    }
}
