using System.Collections.Generic;
using System.Linq;
using System;
using RelhaxModpack.Database;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Represents a package with logical calculations. A dependency is only installed when a selectable package is checked
    /// for installation and is dependent on the dependency i.e. 6th sense sound and icon mods require the 6th sense script dependency.
    /// </summary>
    public class Dependency : DatabasePackage, IDatabaseComponent, IComponentWithDependencies, IXmlSerializable
    {
        /// <summary>
        /// Create an instance of the Dependency class and over-ride DatabasePackage default values.
        /// </summary>
        public Dependency() : base()
        {
            //https://stackoverflow.com/questions/326223/overriding-fields-or-properties-in-subclasses
            //the custom constructor will be called after the base one
        }

        /// <summary>
        /// Create an instance of the Dependency class and over-ride DatabasePackage default values, while using values provided for copy objects.
        /// </summary>
        /// <param name="packageToCopyFrom">The package to copy the information from.</param>
        /// <param name="deep">Set to true to copy list objects, false to use new lists.</param>
        public Dependency(DatabasePackage packageToCopyFrom, bool deep) : base(packageToCopyFrom)
        {
            if (deep && packageToCopyFrom is Dependency dep)
            {
                foreach (DatabaseLogic logic in dep.Dependencies)
                    this.Dependencies.Add(DatabaseLogic.Copy(logic));
            }
        }

        /// <summary>
        /// Called from the constructor, handle any object initialization that should be done.
        /// </summary>
        protected override void InitComponent()
        {
            base.InitComponent();
            InstallGroup = 2;
            PatchGroup = 2;
        }

        #region Xml serialization V1
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes();
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationElements()
        {
            return base.PropertiesForSerializationElements().Concat(DependencyPropertiesToXmlParseElements.ToArray()).ToArray();
        }

        private static readonly List<string> DependencyPropertiesToXmlParseElements = new List<string>()
        {
            nameof(Dependencies)
        };
        #endregion

        #region Xml serialization V2
        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.0 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = base.GetXmlDatabasePropertiesV1Dot0();
            List<XmlDatabaseProperty> xmlDatabasePropertiesAddAfter = new List<XmlDatabaseProperty>()
            {
                new XmlDatabaseProperty() { XmlName = nameof(Dependencies), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Dependencies) }
            };
            //add stuff after base
            xmlDatabaseProperties.AddRange(xmlDatabasePropertiesAddAfter);
            return xmlDatabaseProperties;
        }

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.1 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot1()
        {
            return this.GetXmlDatabasePropertiesV1Dot0();
        }
        #endregion

        #region Database Properties
        /// <summary>
        /// List of linked packages that use this dependency at install time.
        /// </summary>
        public List<DatabaseLogic> DatabasePackageLogic { get; set; } = new List<DatabaseLogic>();

        /// <summary>
        /// List of dependencies this dependency calls on.
        /// </summary>
        public List<DatabaseLogic> Dependencies { get; set; } = new List<DatabaseLogic>();
        #endregion
    }
}
