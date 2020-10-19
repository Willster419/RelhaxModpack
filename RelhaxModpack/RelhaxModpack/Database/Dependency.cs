using System.Collections.Generic;
using System.Linq;
using System;
using RelhaxModpack.Database;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Represents a package with logical calculations. A dependency is only installed when a selectable package is checked
    /// for installation and is dependent on the dependency i.e. 6th sense sound and icon mods require the 6th sense script dependency
    /// </summary>
    public class Dependency : DatabasePackage, IDatabaseComponent, IComponentWithDependencies, IXmlSerializable
    {
        #region Xml serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes
        /// </summary>
        /// <returns>A list of string property names</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes();
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements
        /// </summary>
        /// <returns>A list of string property names</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional</remarks>
        public override string[] PropertiesForSerializationElements()
        {
            return base.PropertiesForSerializationElements().Concat(DependencyPropertiesToXmlParseElements.ToArray()).ToArray();
        }

        private static readonly List<string> DependencyPropertiesToXmlParseElements = new List<string>()
        {
            nameof(Dependencies)
        };
        #endregion

        #region Database Properties
        /// <summary>
        /// List of linked mods and configs that use this dependency at install time
        /// </summary>
        public List<DatabaseLogic> DatabasePackageLogic { get; set; } = new List<DatabaseLogic>();

        /// <summary>
        /// List of dependencies this dependency calls on
        /// </summary>
        public List<DatabaseLogic> Dependencies { get; set; } = new List<DatabaseLogic>();
        #endregion

        #region Other Properties and Methods
        /// <summary>
        /// Create an instance of the Dependency class and over-ride DatabasePackage default values
        /// </summary>
        public Dependency()
        {
            //https://stackoverflow.com/questions/326223/overriding-fields-or-properties-in-subclasses
            //the custom constructor will be called after the base one
            InstallGroup = 2;
            PatchGroup = 2;
        }

        /// <summary>
        /// Create an instance of the Dependency class and over-ride DatabasePackage default values, while using values provided for copy objects
        /// </summary>
        /// <param name="packageToCopyFrom">The package to copy the information from</param>
        /// <param name="deep">Set to true to copy list objects, false to use new lists</param>
        public Dependency(DatabasePackage packageToCopyFrom, bool deep) : base(packageToCopyFrom, deep)
        {
            InstallGroup = 2;
            PatchGroup = 2;
            if (packageToCopyFrom is Dependency dep)
            {
                this.DatabasePackageLogic = new List<DatabaseLogic>();
                this.Dependencies = new List<DatabaseLogic>();

                if (deep)
                {
                    foreach (DatabaseLogic logic in dep.Dependencies)
                        this.Dependencies.Add(DatabaseLogic.Copy(logic));
                }
            }
            else if (packageToCopyFrom is SelectablePackage sp)
            {
                this.Dependencies = new List<DatabaseLogic>();
                if(deep)
                {
                    foreach (DatabaseLogic logic in sp.Dependencies)
                        this.Dependencies.Add(DatabaseLogic.Copy(logic));
                }
            }
        }
        #endregion
    }
}
