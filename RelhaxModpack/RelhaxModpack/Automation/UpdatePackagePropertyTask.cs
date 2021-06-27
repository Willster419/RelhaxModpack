using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public class UpdatePackagePropertyTask : DatabasePackagePropertyTask
    {
        public const string TaskCommandName = "update_package_property";

        public override string Command { get { return TaskCommandName; } }

        public string PropertyValue { get; set; } = string.Empty;

        protected bool propertySet;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(PropertyValue) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            base.ProcessMacros();

            PropertyValue = ProcessMacro(nameof(PropertyValue), PropertyValue);
        }

        public override async Task RunTask()
        {
            string currentValue = property.GetValue(DatabasePackage).ToString();

            //check if it exists first and delete
            AutomationMacro macro = Macros.Find(mac => mac.Name.Equals("old_package_property_value"));
            if (macro != null)
                Macros.Remove(macro);

            //add the macro so it could be used in this task (and later)
            Logging.Debug("The old value was added to the macro list (macro name is old_package_property_value)");
            Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = "old_package_property_value", Value = currentValue });

            Logging.Info("Applying value {0} to property {1} of type {2}", PropertyValue, property.Name, property.PropertyType.ToString());
            propertySet = CommonUtils.SetObjectProperty(targetPackage, property, PropertyValue);
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(propertySet, string.Format("Failed to apply value")))
                return;
        }
        #endregion
    }
}
