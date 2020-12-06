using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class AutomationTask : IXmlSerializable
    {
        #region Xml serialization
        public virtual string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(Name) };
        }

        public virtual string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }

        public static Dictionary<string, Type> TaskTypeMapper { get; } = new Dictionary<string, Type>()
        {
            {"task_command_name", typeof(AutomationTask) }
        };

        public const string AttributeNameForMapping = "name";
        #endregion //Xml serialization

        public virtual bool ValidateCommands()
        {
            //stub
            return true;
        }

        public AutomationSequence AutomationSequence { get; set; }

        public List<AutomationMacro> GlobalMacros { get { return AutomationSequence.AutomationSequencer.GlobalMacros; } }

        public List<AutomationMacro> LocalMacroList { get { return AutomationSequence.LocalMacroList; } }

        public int ExitCode { get; } = 0; // default is 0 - a good exit code

        public string Name { get; set; } = string.Empty;
    }
}
