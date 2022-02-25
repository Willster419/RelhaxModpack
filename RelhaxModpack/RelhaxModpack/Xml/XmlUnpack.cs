using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Installer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelhaxModpack.Xml
{
    /// <summary>
    /// Represents a set of xml instructions for extracting and copying xml files into another location. Includes xml binary decompression
    /// </summary>
    public class XmlUnpack : PackageExtractInstruction
    {
        /// <summary>
        /// For xml unpack instruction files, the xpath to return a list of all xml unpack instruction xml elements.
        /// </summary>
        /// <remarks>As of the time of this writing, all instructions are now stored inside the database and are no longer separate xml files in the package zip files.</remarks>
        public const string XmlUnpackXmlSearchPath = "/files/file";

        /// <summary>
        /// Creates an instance of the XmlUnpack class.
        /// </summary>
        public XmlUnpack() : base()
        {

        }

        /// <summary>
        /// Creates an instance of the XmlUnpack class, copying values form a given XmlUnpack object.
        /// </summary>
        /// <param name="xmlUnpackToCopy">The XmlUnpack object to copy.</param>
        public XmlUnpack(XmlUnpack xmlUnpackToCopy) : base(xmlUnpackToCopy)
        {
            this.FileName = xmlUnpackToCopy.FileName;
            this.ExtractDirectory = xmlUnpackToCopy.ExtractDirectory;
            this.NewFileName = xmlUnpackToCopy.NewFileName;
        }

        /// <summary>
        /// Creates a copy of the given XmlUnpack object.
        /// </summary>
        /// <param name="xmlUnpackToCopy">The XmlUnpack object to copy.</param>
        /// <returns>A copy of the XmlUnpack object.</returns>
        public static XmlUnpack Copy(XmlUnpack xmlUnpackToCopy)
        {
            return new XmlUnpack(xmlUnpackToCopy);
        }

        #region Xml serialization V1
        /// <summary>
        /// The xpath to use to get a list of xml element objects that represent each instruction to serialize.
        /// </summary>
        public override string RootObjectPath { get { return XmlUnpackXmlSearchPath; } }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml elements may always exist, but they may have empty inner text values.</remarks>
        public override string[] PropertiesToSerialize()
        {
            return new string[]
            {
                nameof(Pkg),
                nameof(FileName),
                nameof(DirectoryInArchive),
                nameof(ExtractDirectory),
                nameof(NewFileName)
            };
        }
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
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(FileName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(FileName) },
                new XmlDatabaseProperty() { XmlName = nameof(ExtractDirectory), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(ExtractDirectory) },
                new XmlDatabaseProperty() { XmlName = nameof(NewFileName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(NewFileName) }
            };
            return base.GetXmlDatabasePropertiesV1Dot0().Concat(xmlDatabaseProperties).ToList();
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
                new XmlDatabaseProperty() { XmlName = nameof(FileName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(FileName) },
                new XmlDatabaseProperty() { XmlName = nameof(ExtractDirectory), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(ExtractDirectory) },
                new XmlDatabaseProperty() { XmlName = nameof(NewFileName), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(NewFileName) }
            };
            return base.GetXmlDatabasePropertiesV1Dot0().Concat(xmlDatabaseProperties).ToList();
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
        /// Name of the file to extract
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Path to place the extracted or copied file
        /// </summary>
        public string ExtractDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Filename with replaced macros for destination writing
        /// </summary>
        public string NewFileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets a log formatted string for debugging containing key object name and values.
        /// </summary>
        /// <remarks>If debug output is enabled for the log file during an installation, then each instruction will have it's DumpInfoToLog property called.</remarks>
        public override string DumpInfoToLog
        {
            get
            {
                return $"{base.DumpInfoToLog}{Environment.NewLine}{ApplicationConstants.LogSpacingLineup}{nameof(FileName)}={FileName}, {nameof(ExtractDirectory)}={ExtractDirectory} {nameof(NewFileName)}={NewFileName}";
            }
        }

        /// <summary>
        /// A string representation of the object
        /// </summary>
        /// <returns>The FileName property name and value.</returns>
        public override string ToString()
        {
            return string.Format("FileName={0}", FileName);
        }

        /// <summary>
        /// Compares two instructions to determine if their values are equal.
        /// </summary>
        /// <param name="instructionToCompare">The instruction to compare against.</param>
        /// <returns>True if the compared values are equal, false otherwise.</returns>
        public override bool InstructionsEqual(Instruction instructionToCompare)
        {
            if (!base.InstructionsEqual(instructionToCompare))
                return false;

            XmlUnpack xmlUnpackToCompare = instructionToCompare as XmlUnpack;

            if (xmlUnpackToCompare == null)
                return false;

            if (!this.FileName.Equals(xmlUnpackToCompare.FileName))
                return false;

            if (!this.ExtractDirectory.Equals(xmlUnpackToCompare.ExtractDirectory))
                return false;

            if (!this.NewFileName.Equals(xmlUnpackToCompare.NewFileName))
                return false;

            return true;
        }
    }
}
