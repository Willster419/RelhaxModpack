using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Represents an entry for a package that conflicts with another package in the database where both options cannot be selected at the same time.
    /// </summary>
    public class ConflictingPackage : XmlComponent
    {
        /// <summary>
        /// Create an instance of the ConflictingPackage class.
        /// </summary>
        public ConflictingPackage() : base()
        {

        }

        /// <summary>
        /// Create an instance of the ConflictingPackage class based on values from a given entry to copy.
        /// </summary>
        /// <param name="conflictingPackageToCopy">The entry to copy.</param>
        public ConflictingPackage(ConflictingPackage conflictingPackageToCopy) : base(conflictingPackageToCopy)
        {
            this.ConflictingPackageName = conflictingPackageToCopy.ConflictingPackageName;
            this.ConflictingPackageUID = conflictingPackageToCopy.ConflictingPackageUID;
            this.ConflictingSelectablePackage = conflictingPackageToCopy.ConflictingSelectablePackage;
            this.ParentSelectablePackage = conflictingPackageToCopy.ParentSelectablePackage;
        }

        #region Xml serialization V2
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
                new XmlDatabaseProperty() { XmlName = nameof(ConflictingPackageName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(ConflictingPackageName) },
                new XmlDatabaseProperty() { XmlName = nameof(ConflictingPackageUID), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(ConflictingPackageUID) }
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
            return this.GetXmlDatabasePropertiesV1Dot1();
        }
        #endregion

        /// <summary>
        /// The name of the package that conflicts with this package.
        /// </summary>
        public string ConflictingPackageName { get; set; } = string.Empty;

        /// <summary>
        /// The UID of the package that conflicts with this package.
        /// </summary>
        public string ConflictingPackageUID { get; set; } = string.Empty;

        /// <summary>
        /// The reference to the package that conflicts with this package.
        /// </summary>
        public SelectablePackage ConflictingSelectablePackage { get; set; }

        /// <summary>
        /// The reference package that this conflicting package entry belongs to.
        /// </summary>
        public SelectablePackage ParentSelectablePackage { get; set; }

        /// <summary>
        /// Determines if two entries are equal by comparing the package name and UID values.
        /// </summary>
        /// <param name="packageToCompare">The package to compare.</param>
        /// <returns>True if the entires contain the same values, false otherwise.</returns>
        public bool IsEqual(ConflictingPackage packageToCompare)
        {
            return packageToCompare.ConflictingPackageName.Equals(this.ConflictingPackageName) && packageToCompare.ConflictingPackageUID.Equals(this.ConflictingPackageUID);
        }

        /// <summary>
        /// Provide a string representation of the entry.
        /// </summary>
        /// <returns>The string representation of the entry, containing the entry's conflicting package name and UID</returns>
        public override string ToString()
        {
            return $"{nameof(ConflictingPackageName)}: {ConflictingPackageName}, {nameof(ConflictingPackageUID)}: {ConflictingPackageUID}";
        }
    }
}
