using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// An AutomationMacro object represents a key-value pair that allows replacement of text inside task objects with bracket notation ({}).
    /// </summary>
    public sealed class AutomationMacro : IXmlSerializable
    {
        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(Name), nameof(Value) };
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }
        #endregion //Xml serialization

        /// <summary>
        /// The regex string used for macro analysis and replacement
        /// </summary>
        public const string MacroReplaceRegex = @"^[^{}]*(?'inner1'(?'inner2'(?'open'{)(?'inner3'[^{}]*))+(?>(?'-open'})[^{}]*)+)+(?(open)(?!))$";

        /// <summary>
        /// The first level of macro detection in a string. Represents the first set of macro replacement brackets.
        /// </summary>
        public const string RegexGroupInner1 = "inner1";

        /// <summary>
        /// The second level of macro detection in a string. Represents a macro inside a macro who's value is to be used for the name of the first level of the macro.
        /// </summary>
        /// <remarks>It is possible to have multiple recursive levels inside a macro. This label is 1 of 2 used for recursive macro processing.
        /// See the unit tests source code for more information.</remarks>
        public const string RegexGroupInner2 = "inner2";

        /// <summary>
        /// The second level of macro detection in a string. Represents a macro inside a macro who's value is to be used for the name of the first level of the macro.
        /// </summary>
        /// <remarks>It is possible to have multiple recursive levels inside a macro. This label is 1 of 2 used for recursive macro processing.
        /// See the unit tests source code for more information.</remarks>
        public const string RegexGroupInner3 = "inner3";

        /// <summary>
        /// The name of macro.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The value of the macro.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The type of macro to determine if it can be over-written.
        /// </summary>
        /// <seealso cref="MacroType"/>
        public MacroType MacroType { get; set; }

        /// <summary>
        /// Create a copy of an AutomationMacro.
        /// </summary>
        /// <param name="macroToCopy">The AutomationMacro instance to create a copy of.</param>
        /// <returns>A copy of the AutomationMacro object instance.</returns>
        public static AutomationMacro Copy(AutomationMacro macroToCopy)
        {
            return new AutomationMacro() { Name = macroToCopy.Name, Value = macroToCopy.Value, MacroType = macroToCopy.MacroType };
        }

        /// <summary>
        /// Returns the values of this instance into a formatted string.
        /// </summary>
        /// <returns>The formatted string containing the names and values of the instance's properties.</returns>
        public override string ToString()
        {
            return string.Format("Name: {0}, Value: {1}, Type: {2}", Name, Value, MacroType.ToString());
        }
    }
}
