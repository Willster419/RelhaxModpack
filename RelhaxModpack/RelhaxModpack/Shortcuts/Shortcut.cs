using RelhaxModpack.Common;
using RelhaxModpack.Database;
using RelhaxModpack.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack.Shortcuts
{
    /// <summary>
    /// Represents instructions on how to create a shortcut
    /// </summary>
    public class Shortcut : Instruction
    {
        /// <summary>
        /// For shortcut instruction files, the xpath to return a list of all shortcut instruction xml elements.
        /// </summary>
        /// <remarks>As of the time of this writing, all instructions are now stored inside the database and are no longer separate xml files in the package zip files.</remarks>
        public const string ShortcutXmlSearchPath = "/shortcuts/shortcut";

        /// <summary>
        /// Creates an instance of the Shortcut class.
        /// </summary>
        public Shortcut() : base()
        {

        }

        /// <summary>
        /// Creates an instance of the Shortcut class, copying values form a given Shortcut object.
        /// </summary>
        /// <param name="shortcutToCopy">The Shortcut object to copy.</param>
        public Shortcut(Shortcut shortcutToCopy) : base()
        {
            this.Path = shortcutToCopy.Path;
            this.Name = shortcutToCopy.Name;
            this.Enabled = shortcutToCopy.Enabled;
        }

        /// <summary>
        /// Creates a copy of the given Shortcut object.
        /// </summary>
        /// <param name="shortcutToCopy">The Shortcut object to copy.</param>
        /// <returns>A copy of the Shortcut object.</returns>
        public static Shortcut Copy(Shortcut shortcutToCopy)
        {
            return new Shortcut(shortcutToCopy);
        }

        #region Xml serialization V1
        /// <summary>
        /// The xpath to use to get a list of xml element objects that represent each instruction to serialize.
        /// </summary>
        public override string RootObjectPath { get { return ShortcutXmlSearchPath; } }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml elements may always exist, but they may have empty inner text values.</remarks>
        public override string[] PropertiesToSerialize()
        {
            return new string[]
            {
                nameof(Path),
                nameof(Name),
                nameof(Enabled)
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
                new XmlDatabaseProperty() { XmlName = nameof(Path), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Path) },
                new XmlDatabaseProperty() { XmlName = nameof(Name), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Name) },
                new XmlDatabaseProperty() { XmlName = nameof(Enabled), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Enabled) }
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
                new XmlDatabaseProperty() { XmlName = nameof(Path), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Path) },
                new XmlDatabaseProperty() { XmlName = nameof(Name), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Name) },
                new XmlDatabaseProperty() { XmlName = nameof(Enabled), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Enabled) }
            };
            return xmlDatabaseProperties;
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
        /// The target of the shortcut
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// The name for the shortcut
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Flag for in the installer to actually create the shortcut
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>The name property and value of the shortcut.</returns>
        public override string ToString()
        {
            return string.Format("Name={0}", Name);
        }

        /// <summary>
        /// Gets a log formatted string for debugging containing key object name and values.
        /// </summary>
        /// <remarks>If debug output is enabled for the log file during an installation, then each instruction will have it's DumpInfoToLog property called.</remarks>
        public override string DumpInfoToLog
        {
            get
            {
                return string.Format( "{0}{1}{2}, {3}={4}, {5}={6}, {7}={8}", base.DumpInfoToLog, Environment.NewLine, ApplicationConstants.LogSpacingLineup, nameof(Path), Path, nameof(Name), Name, nameof(Enabled), Enabled);
            }
        }

        /// <summary>
        /// Compares two instructions to determine if their values are equal.
        /// </summary>
        /// <param name="instructionToCompare">The instruction to compare against.</param>
        /// <returns>True if the compared values are equal, false otherwise.</returns>
        public override bool InstructionsEqual(Instruction instructionToCompare)
        {
            Shortcut shortcutToCompare = instructionToCompare as Shortcut;

            if (shortcutToCompare == null)
                return false;

            if (!this.Path.Equals(shortcutToCompare.Path))
                return false;

            if (!this.Name.Equals(shortcutToCompare.Name))
                return false;

            if (!this.Enabled.Equals(shortcutToCompare.Enabled))
                return false;

            return true;
        }
    }
}
