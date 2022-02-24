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
                new XmlDatabaseProperty() { XmlName = nameof(Pkg), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Pkg) },
                new XmlDatabaseProperty() { XmlName = nameof(DirectoryInArchive), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(DirectoryInArchive) }
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

        public override bool InstructionsEqual(Instruction instructionToCompare)
        {
            PackageExtractInstruction packageExtractInstructionToCompare = instructionToCompare as PackageExtractInstruction;
            if (packageExtractInstructionToCompare == null)
                return false;

            if (!this.Pkg.Equals(packageExtractInstructionToCompare.Pkg))
                return false;

            if (!this.DirectoryInArchive.Equals(packageExtractInstructionToCompare.DirectoryInArchive))
                return false;

            return true;
        }

        public override string DumpInfoToLog
        {
            get
            {
                return string.Format("{0}, {1}={2}, {3}={4}", base.DumpInfoToLog, nameof(Pkg), Pkg, nameof(DirectoryInArchive), DirectoryInArchive);
            }
        }
    }
}
