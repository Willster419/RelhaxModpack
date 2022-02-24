using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// An enumerated representation of "and" and "or" logic
    /// </summary>
    public enum Logic
    {
        /// <summary>
        /// A logical AND
        /// </summary>
        AND = 1,

        /// <summary>
        /// A logical OR
        /// </summary>
        OR = 0
    }

    /// <summary>
    /// Used for database dependency calculation. Determines what dependent packages use the package that this object is attached to
    /// </summary>
    public class DatabaseLogic : XmlComponent, IXmlSerializable
    {
        /// <summary>
        /// Create an instance of the DatabaseLogic class.
        /// </summary>
        public DatabaseLogic() : base()
        {

        }

        /// <summary>
        /// Create an instance of the DatabaseLogic class, copying values from a given DatabaseLogic object.
        /// </summary>
        /// <param name="databaseLogicToCopy">The DatabaseLogic object to copy values from</param>
        public DatabaseLogic(DatabaseLogic databaseLogicToCopy) : base(databaseLogicToCopy)
        {
            this.PackageName = databaseLogicToCopy.PackageName;
            this.PackageUID = databaseLogicToCopy.PackageUID;
            this.WillBeInstalled = databaseLogicToCopy.WillBeInstalled;
            this.NotFlag = databaseLogicToCopy.NotFlag;
            this.Logic = databaseLogicToCopy.Logic;
            this.RefrenceLinked = databaseLogicToCopy.RefrenceLinked;
            this.DependencyPackageRefrence = databaseLogicToCopy.DependencyPackageRefrence;
            this.ParentPackageRefrence = databaseLogicToCopy.ParentPackageRefrence;
        }

        #region Xml serialization V1
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(PackageName), nameof(NotFlag), nameof(Logic) };
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
        #endregion

        #region Xml serialization V2
        /// <summary>
        /// For some xml schema versions, the actual name of the xml entry used for this property.
        /// </summary>
        /// <remarks>A DatabaseLogic is used for dependency calculation to determine based on what the user selected for packages to install, what dependent packages also need to be installed.</remarks>
        public const string XmlElementName = "Dependency";

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.0 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(PackageName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PackageName) },
                new XmlDatabaseProperty() { XmlName = nameof(NotFlag), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(NotFlag) },
                new XmlDatabaseProperty() { XmlName = nameof(Logic), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Logic) }
            };
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

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.2 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot2()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(PackageName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PackageName) },
                new XmlDatabaseProperty() { XmlName = nameof(PackageUID), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PackageUID) },
                new XmlDatabaseProperty() { XmlName = nameof(NotFlag), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(NotFlag) },
                new XmlDatabaseProperty() { XmlName = nameof(Logic), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Logic) }
            };
            return xmlDatabaseProperties;
        }

        /// <summary>
        /// Gets the xml element name that should be used for saving and loading this component, based on the schema to load/save.
        /// </summary>
        /// <param name="schemaVersion">The schema version to load/save.</param>
        /// <returns>The name of the xml element to use for saving/loading this component.</returns>
        public override string GetXmlElementName(string schemaVersion)
        {
            switch (schemaVersion)
            {
                case SchemaV1Dot0:
                case SchemaV1Dot1:
                case SchemaV1Dot2:
                    return XmlElementName;
                default:
                    return base.GetXmlElementName(schemaVersion);
            }
        }
        #endregion

        /// <summary>
        /// The name of the package that *this* package is dependent on.
        /// </summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>
        /// The UID of the package that *this* package is dependent on.
        /// </summary>
        public string PackageUID { get; set; } = string.Empty;

        /// <summary>
        /// Flag to determine if this package will be installed.
        /// </summary>
        public bool WillBeInstalled { get; set; } = false;

        /// <summary>
        /// Flag for negating the "AND" and "OR" logic (Creates "NAND" and "NOR") of how to install the attach object.
        /// </summary>
        public bool NotFlag { get; set; } = false;

        /// <summary>
        /// The logic type to use for this package definition.
        /// </summary>
        public Logic Logic { get; set; } = Logic.OR;

        /// <summary>
        /// A flag for dependency calculation for when the application has linked the dependency reference
        /// </summary>
        /// <remarks>During dependency calculation, the application will 'link' the referenced dependencies in a package
        /// to the referenced dependency. This allows for the application to process dependency calculation logic in a dynamic
        /// AND and OR system. Having the flag can help to determine if a reference does not exist</remarks>
        public bool RefrenceLinked { get; set; } = false;

        /// <summary>
        /// Gets or sets a reference to the parent package this dependency came from.
        /// </summary>
        public IComponentWithDependencies ParentPackageRefrence { get; set; } = null;

        /// <summary>
        /// Gets or sets a reference to the dependency object that this databaseLogic object links to.
        /// </summary>
        public DatabasePackage DependencyPackageRefrence { get; set; } = null;

        /// <summary>
        /// String representation of the object.
        /// </summary>
        /// <returns>The name of the package this object attaches to.</returns>
        public override string ToString()
        {
            return PackageName;
        }

        /// <summary>
        /// Create a copy of the given DatabaseLogic object.
        /// </summary>
        /// <param name="databaseLogicToCopy">The object to copy.</param>
        /// <returns>A new DatabaseLogic object with the same values.</returns>
        public static DatabaseLogic Copy(DatabaseLogic databaseLogicToCopy)
        {
            return new DatabaseLogic(databaseLogicToCopy);
        }
    }
}
