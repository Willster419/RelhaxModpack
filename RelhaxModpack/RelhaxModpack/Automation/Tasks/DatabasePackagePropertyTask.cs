using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public abstract class DatabasePackagePropertyTask : DatabasePackageTask
    {
        public string PropertyName { get; set; }

        public string TargetPackageUID { get; set; } = "this";

        protected DatabasePackage targetPackage;

        protected Type packageType;

        protected PropertyInfo property;

        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(PropertyName), nameof(TargetPackageUID) }).ToArray();
        }
        #endregion

        #region Task execution
        public override void ProcessMacros()
        {
            TargetPackageUID = ProcessMacro(nameof(TargetPackageUID), TargetPackageUID);
            PreProcessTargetPackage();
            PropertyName = ProcessMacro(nameof(PropertyName), PropertyName);
            PreProcessTargetProperty();
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
        }

        protected virtual void PreProcessTargetProperty()
        {
            if (targetPackage != null)
            {
                //check that a property matching the given name exists. Case don't matter
                //this MUST be done in ProcessMacros() because it goes before validateCommands
                packageType = targetPackage.GetType();
                Logging.Debug("PackageType: {0}", packageType.ToString());
                property = packageType.GetProperties().First(prop => prop.Name.ToLower().Equals(PropertyName.ToLower()));
            }
        }

        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandTrue(string.IsNullOrEmpty(PropertyName), "The arg PropertyName is empty string"))
                return;

            if (ValidateCommandTrue(targetPackage == null, string.Format("A package by the UID {0} was not found in the database list", TargetPackageUID)))
                return;

            if (ValidateCommandTrue(property == null, string.Format("A property by the name {0} does not exist for package type {1}", PropertyName, packageType.ToString())))
                return;
        }
        #endregion
    }
}
