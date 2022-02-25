using RelhaxModpack.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// A PackageExtractInstruction is a type of Instruction that requires either extracting a file entry from a zip file or copying a file from one location to another.
    /// </summary>
    public abstract class PackageExtractInstruction : Instruction
    {
        /// <summary>
        /// Create an instance of a parent class of PackageExtractInstruction.
        /// </summary>
        public PackageExtractInstruction() : base()
        {

        }

        /// <summary>
        /// Create an instance of a parent class of PackageExtractInstruction, copying values from a given PackageExtractInstruction.
        /// </summary>
        /// <param name="packageExtractInstructionToCopyFrom">The PackageExtractInstruction to copy values from.</param>
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
        /// Complete path to the package file.
        /// </summary>
        /// <remarks>This is loaded from the xml file. The package itself is a zip file.</remarks>
        public string Pkg { get; set; } = string.Empty;

        /// <summary>
        /// Path inside the pkg file to the filename to process. If Pkg is empty, this is the path to the atlas and map file.
        /// </summary>
        /// <remarks>This is loaded from the xml file.</remarks>
        public string DirectoryInArchive { get; set; } = string.Empty;

        /// <summary>
        /// Compares two Instruction objects's values to determine if they are equal.
        /// </summary>
        /// <param name="instructionToCompare">The instruction to compare against.</param>
        /// <returns>True if the instructions are equal, false otherwise.</returns>
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

        /// <summary>
        /// Returns a string representation of the object used for the log file.
        /// </summary>
        public override string DumpInfoToLog
        {
            get
            {
                return string.Format("{0}, {1}={2}, {3}={4}", base.DumpInfoToLog, nameof(Pkg), Pkg, nameof(DirectoryInArchive), DirectoryInArchive);
            }
        }
    }
}
