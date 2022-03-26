using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// Retrieves a string representation of a property of a package. If the property is a list of array, then retrieve the given index.
    /// </summary>
    /// <remarks>A great use of this is to get the developer URL at a specific index for where to check for package updates.</remarks>
    public class RetrievePackagePropertyTask : DatabasePackagePropertyTask
    {
        /// <summary>
        /// The xml name of this command.
        /// </summary>
        public const string TaskCommandName = "retrieve_package_property";

        /// <summary>
        /// Gets the xml name of the command to determine the task instance type.
        /// </summary>
        public override string Command { get { return TaskCommandName; } }

        /// <summary>
        /// The name of the macro to create with the retrieved value.
        /// </summary>
        public string MacroSaveName { get; set; }

        /// <summary>
        /// The index into the property array to retrieve the value.
        /// </summary>
        public string PropertyIndex { get; set; } = string.Empty;

        /// <summary>
        /// Flag to indicate of the value of the property of the package was successfully retrieved.
        /// </summary>
        protected bool propertyGot;

        /// <summary>
        /// Parsed result of the argument PropertyIndex.
        /// </summary>
        /// <seealso cref="PropertyIndex"/>
        protected int propertyIndex;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(MacroSaveName), nameof(PropertyIndex) }).ToArray();
        }
        #endregion

        #region Task execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();

            MacroSaveName = ProcessMacro(nameof(MacroSaveName), MacroSaveName);
            PropertyIndex = ProcessMacro(nameof(PropertyIndex), PropertyIndex);
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandTrue(string.IsNullOrEmpty(MacroSaveName), "The argument MacroSaveName is empty string"))
                return;

            if (ValidateCommandTrue(targetPackage == null, string.Format("A package by the UID {0} was not found in the database list", TargetPackageUID)))
                return;

            if (ValidateCommandTrue(property == null, string.Format("A property by the name {0} does not exist for package type {1}", PropertyName, packageType.ToString())))
                return;

            bool isValidType = property.PropertyType.IsValueType || property.PropertyType.IsArray || property.PropertyType.Equals(typeof(string));
            if (ValidateCommandFalse(isValidType, string.Format("Property must be a value type (string, int, etc), or array of a value type")))
                return;

            if (property.PropertyType.IsArray)
            {
                if (ValidateCommandTrue(string.IsNullOrEmpty(PropertyIndex), "Property detected to be an array and PropertyIndex is empty string"))
                    return;
                propertyIndex = int.Parse(PropertyIndex);
            }
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string valueToSave;
            if (property.PropertyType.IsValueType)
            {
                valueToSave = property.GetValue(targetPackage).ToString();
            }
            else
            {
                object currentValue = property.GetValue(targetPackage);
                //to get type of array
                //https://stackoverflow.com/a/2085186/3128017
                Type arrayType = currentValue.GetType().GetElementType();
                if (!(arrayType.IsValueType || arrayType.Equals(typeof(string))))
                {
                    Logging.Error("The array requested does not hold value type objects");
                    propertyGot = false;
                    return;
                }

                Array arrayObject = currentValue as Array;
                if (propertyIndex > arrayObject.Length || propertyIndex < 0)
                {
                    Logging.Error("Index {0} is not valid for this property, valid options are 0 to {1}", propertyIndex, arrayObject.Length - 1);
                    propertyGot = false;
                    return;
                }
                object indexedArrayObjectValue = arrayObject.GetValue(propertyIndex);
                valueToSave = indexedArrayObjectValue.ToString();
            }

            Logging.Info("Creating macro for package {0} (UID {1}) of property {2}", targetPackage.PackageName, targetPackage.UID, property.Name);
            Logging.Info("Macro name: {0}, Value: {1}", MacroSaveName, valueToSave);
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals(MacroSaveName));
            if (macro != null)
                Macros.Remove(macro);
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = MacroSaveName, Value = valueToSave });
            propertyGot = true;
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(propertyGot, string.Format("Failed to apply value")))
                return;
        }
        #endregion
    }
}
