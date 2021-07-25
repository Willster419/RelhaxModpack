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

        /// <summary>
        /// The regex string used for macro analysis and replacement
        /// </summary>
        public const string MacroReplaceRegex = @"^[^{}]*(?'inner1'(?'inner2'(?'open'{)(?'inner3'[^{}]*))+(?>(?'-open'})[^{}]*)+)+(?(open)(?!))$";

        public const string RegexGroupInner1 = "inner1";

        public const string RegexGroupInner2 = "inner2";

        public const string RegexGroupInner3 = "inner3";

        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public MacroType MacroType { get; set; }

        public static AutomationMacro Copy(AutomationMacro macroToCopy)
        {
            return new AutomationMacro() { Name = macroToCopy.Name, Value = macroToCopy.Value, MacroType = macroToCopy.MacroType };
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Value: {1}, Type: {2}", Name, Value, MacroType.ToString());
        }
    }
}
