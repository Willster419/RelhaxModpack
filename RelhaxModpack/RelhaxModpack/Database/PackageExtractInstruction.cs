using RelhaxModpack.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    public abstract class PackageExtractInstruction : Instruction
    {
        public PackageExtractInstruction() : base()
        {

        }

        public PackageExtractInstruction(PackageExtractInstruction packageExtractInstructionToCopyFrom) : base(packageExtractInstructionToCopyFrom)
        {
            this.Pkg = packageExtractInstructionToCopyFrom.Pkg;
            this.DirectoryInArchive = packageExtractInstructionToCopyFrom.DirectoryInArchive;
        }

        #region Xml serialization V2
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(Pkg), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Pkg) },
                new XmlDatabaseProperty() { XmlName = nameof(DirectoryInArchive), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(DirectoryInArchive) }
            };
            return xmlDatabaseProperties;
        }
        #endregion

        /// <summary>
        /// Complete path to the package file
        /// </summary>
        /// <remarks>This is loaded from the xml file. The package itself is a non-compressed zip file</remarks>
        public string Pkg { get; set; } = string.Empty;

        /// <summary>
        /// Path inside the pkg file to the filename to process. If Pkg is empty, this is the path to the atlas and map file
        /// </summary>
        /// <remarks>This is loaded from the xml file</remarks>
        public string DirectoryInArchive { get; set; } = string.Empty;
    }
}
