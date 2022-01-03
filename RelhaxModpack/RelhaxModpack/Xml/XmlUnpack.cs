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
        public const string XmlUnpackXmlSearchPath = "/files/file";

        public XmlUnpack() : base()
        {

        }

        public XmlUnpack(XmlUnpack xmlUnpackToCopy) : base(xmlUnpackToCopy)
        {
            this.FileName = xmlUnpackToCopy.FileName;
            this.ExtractDirectory = xmlUnpackToCopy.ExtractDirectory;
            this.NewFileName = xmlUnpackToCopy.NewFileName;
        }

        public static XmlUnpack Copy(XmlUnpack xmlUnpackToCopy)
        {
            return new XmlUnpack(xmlUnpackToCopy);
        }

        public override string RootObjectPath { get { return XmlUnpackXmlSearchPath; } }

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

        #region Xml serialization V2
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
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
        /// Collect all properties of the extraction instructions to dump into the log file
        /// </summary>
        public string DumpInfoToLog
        {
            get
            {
                return string.Format("NativeProcessingFile={0}, ActualPatchName={1}, Pkg={2}, DirectoryInArchive={3}, FileName={4}, ExtractDirectory={5}, NewFileName={6}",
                NativeProcessingFile,
                ActualPatchName,
                Pkg,
                DirectoryInArchive,
                FileName,
                ExtractDirectory,
                NewFileName);
            }
        }

        /// <summary>
        /// A string representation of the object
        /// </summary>
        /// <returns>The native name of the unpack instruction file and file target</returns>
        public override string ToString()
        {
            return string.Format("FileName={0}", FileName);
        }

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
