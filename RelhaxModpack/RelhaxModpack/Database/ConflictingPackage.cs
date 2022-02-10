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
        }

        #region Xml serialization V2
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

        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot1()
        {
            return this.GetXmlDatabasePropertiesV1Dot0();
        }

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
    }
}
