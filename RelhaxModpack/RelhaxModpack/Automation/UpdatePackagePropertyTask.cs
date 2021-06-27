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
    public class UpdatePackagePropertyTask : DatabasePackageTask
    {
        public const string TaskCommandName = "update_package_property";

        public override string Command { get { return TaskCommandName; } }

        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }

        public string TargetPackageUID { get; set; } = "this";

        protected Type packageType;

        protected PropertyInfo property;

        protected bool propertySet;

        protected DatabasePackage targetPackage;

        #region Xml serialization
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(PropertyName), nameof(PropertyValue), nameof(TargetPackageUID) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            PreProcessTargetPackage();

            PropertyName = ProcessMacro(nameof(PropertyName), PropertyName);
            PropertyValue = ProcessMacro(nameof(PropertyValue), PropertyValue);
            TargetPackageUID = ProcessMacro(nameof(TargetPackageUID), TargetPackageUID);
        }

        public override void ValidateCommands()
        {
            if (ValidateCommandTrue(string.IsNullOrEmpty(PropertyName), "The arg PropertyName is empty string"))
                return;

            if (ValidateCommandTrue(string.IsNullOrEmpty(TargetPackageUID), "The arg PropertyName is empty string"))
                return;

            if (ValidateCommandTrue(property == null, string.Format("A property by the name {0} does not exist for package type {1}", PropertyName, packageType.ToString())))
                return;

            if (ValidateCommandTrue(targetPackage == null, string.Format("A package by the UID {0} was not found in the database list", TargetPackageUID)))
                return;
        }

        public override async Task RunTask()
        {
            Logging.Info("Applying value {0} to property {1} of type {2}", PropertyValue, property.Name, property.PropertyType.ToString());
            propertySet = CommonUtils.SetObjectProperty(DatabasePackage, property, PropertyValue);
        }

        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultFalse(propertySet, string.Format("Failed to apply value")))
                return;
        }

        protected virtual void PreProcessTargetPackage()
        {
            //check that the target package exists
            if (TargetPackageUID.Equals("this"))
            {
                targetPackage = DatabasePackage;
            }
            else
            {
                targetPackage = DatabasePackages.Find(package => package.UID.Equals(TargetPackageUID));
            }

            if (targetPackage != null)
            {
                //check that a property matching the given name exists. Case don't matter
                //this MUST be done in ProcessMacros() because it goes before validateCommands
                packageType = this.DatabasePackage.GetType();
                Logging.Debug("PackageType: {0}", packageType.ToString());
                property = packageType.GetProperties().First(prop => prop.Name.ToLower().Equals(PropertyName.ToLower()));

                if (property != null)
                {
                    string currentValue = property.GetValue(DatabasePackage).ToString();

                    //check if it exists first and delete
                    AutomationMacro macro = Macros.Find(mac => mac.Name.Equals("old_package_property_value"));
                    if (macro != null)
                        Macros.Remove(macro);

                    //add the macro so it could be used in this task (and later)
                    Logging.Debug("The old value was added to the macro list (macro name is old_package_property_value)");
                    Macros.Add(new AutomationMacro() { MacroType = MacroType.Local, Name = "old_package_property_value", Value = currentValue });
                }
            }
        }
        #endregion
    }
}
