using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    public class ConflictingPackage : XmlComponent
    {
        public ConflictingPackage() : base()
        {

        }

        public ConflictingPackage(ConflictingPackage conflictingPackageToCopy) : base(conflictingPackageToCopy)
        {
            this.PackageName = conflictingPackageToCopy.PackageName;
            this.PackageUID = conflictingPackageToCopy.PackageUID;
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
                new XmlDatabaseProperty() { XmlName = nameof(PackageName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PackageName) },
                new XmlDatabaseProperty() { XmlName = nameof(PackageUID), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(PackageUID) }
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
        /// The name of the package that *this* package is dependent on
        /// </summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>
        /// The UID of the package that *this* package is dependent on
        /// </summary>
        public string PackageUID { get; set; } = string.Empty;

        public SelectablePackage ConflictingSelectablePackage { get; set; }

        public SelectablePackage ParentSelectablePackage { get; set; }

        public bool IsEqual(ConflictingPackage packageToCompare)
        {
            return packageToCompare.PackageName.Equals(this.PackageName) && packageToCompare.PackageUID.Equals(this.PackageUID);
        }


        public override string ToString()
        {
            return $"{nameof(PackageName)}: {PackageName}, {nameof(PackageUID)}: {PackageUID}";
        }
    }
}
