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
    /// <summary>
    /// Provides an implementation for working with a property of a DatabasePackage.
    /// </summary>
    public abstract class DatabasePackagePropertyTask : DatabasePackageTask
    {
        /// <summary>
        /// The name of the property to access.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The UID of the DatabasePackage to access.
        /// </summary>
        /// <remarks>If the value of this argument is "this", then it will use the UID of this sequence's DatabasePackage.</remarks>
        public string TargetPackageUID { get; set; } = "this";

        /// <summary>
        /// The DatabasePackage object to access.
        /// </summary>
        protected DatabasePackage targetPackage;

        /// <summary>
        /// The type of the targetPackage.
        /// </summary>
        protected Type packageType;

        /// <summary>
        /// The metadata of the property to access.
        /// </summary>
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
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            TargetPackageUID = ProcessMacro(nameof(TargetPackageUID), TargetPackageUID);
            PreProcessTargetPackage();
            PropertyName = ProcessMacro(nameof(PropertyName), PropertyName);
            PreProcessTargetProperty();
        }

        /// <summary>
        /// Attempts to retrieve the target package by the TargetPackageUID task argument.
        /// </summary>
        /// <seealso cref="TargetPackageUID"/>
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

        /// <summary>
        /// Attempts to retrieve the target property metadata of the target package by the PropertyName task argument.
        /// </summary>
        /// <seealso cref="PropertyName"/>
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

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();

            if (ValidateCommandTrue(string.IsNullOrEmpty(PropertyName), "The argument PropertyName is empty string"))
                return;

            if (ValidateCommandTrue(targetPackage == null, string.Format("A package by the UID {0} was not found in the database list", TargetPackageUID)))
                return;

            if (ValidateCommandTrue(property == null, string.Format("A property by the name {0} does not exist for package type {1}", PropertyName, packageType.ToString())))
                return;
        }
        #endregion
    }
}
