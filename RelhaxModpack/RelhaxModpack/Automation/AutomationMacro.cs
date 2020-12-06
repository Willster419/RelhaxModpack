using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public sealed class AutomationMacro : IXmlSerializable
    {
        #region Xml serialization
        public string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(Name), nameof(Value) };
        }

        public string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }
        #endregion //Xml serialization

        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public MacroType MacroType { get; set; } = MacroType.None;

        public static AutomationMacro Copy(AutomationMacro macroToCopy)
        {
            return new AutomationMacro() { Name = macroToCopy.Name, Value = macroToCopy.Value, MacroType = macroToCopy.MacroType };
        }
    }
}
