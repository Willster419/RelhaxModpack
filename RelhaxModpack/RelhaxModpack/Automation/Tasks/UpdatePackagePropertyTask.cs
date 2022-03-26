using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Allows a database's property value to be updated with a value given from the PropertyValue argument.
    /// </summary>
    public class UpdatePackagePropertyTask : DatabasePackagePropertyTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "update_package_property";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The value to update the property with.
        /// </summary>
        public string PropertyValue { get; set; } = string.Empty;

        /// <summary>
        /// Flag to indicate if the property was successfully updated.
        /// </summary>
        protected bool propertySet;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(PropertyValue) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();

            //if the value contains the macro old_package_property_value, the user probably wants to use the old value as part of the new value
            //check if it exists first and delete
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals("old_package_property_value"));
            if (macro != null)
                Macros.Remove(macro);
            PropertyValue = ProcessMacro(nameof(PropertyValue), PropertyValue);
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string currentValue = property.GetValue(targetPackage).ToString();

            //add the macro so it could be used in this task (and later)
            Logging.Debug("The old value was added to the macro list (macro name is old_package_property_value)");
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = "old_package_property_value", Value = currentValue });

            //then run the macro process again in case the user is using the old value (using {old_package_property_value}, just created) to create the new one
            PropertyValue = ProcessMacro(nameof(PropertyValue), PropertyValue);

            Logging.Info("Applying value {0} to property {1} of type {2}", PropertyValue, property.Name, property.PropertyType.ToString());
            propertySet = CommonUtils.SetObjectProperty(targetPackage, property, PropertyValue);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(propertySet, string.Format("Failed to apply value")))
                return;

            string newValue = property.GetValue(targetPackage).ToString();
            if (ProcessTaskResultFalse(newValue.Equals(PropertyValue), "The target package's property is not updated to PropertyValue"))
                return;
        }
        #endregion
    }
}
