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
        public const string ShortcutXmlSearchPath = "/shortcuts/shortcut";

        public Shortcut() : base()
        {

        }

        public Shortcut(Shortcut shortcutToCopy) : base()
        {
            this.Path = shortcutToCopy.Path;
            this.Name = shortcutToCopy.Name;
            this.Enabled = shortcutToCopy.Enabled;
        }

        public static Shortcut Copy(Shortcut shortcutToCopy)
        {
            return new Shortcut(shortcutToCopy);
        }

        #region Xml serialization V1
        public override string RootObjectPath { get { return ShortcutXmlSearchPath; } }

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
        /// <returns>The name and target of the shortcut</returns>
        public override string ToString()
        {
            return string.Format("Name={0}", Name);
        }

        public override string DumpInfoToLog
        {
            get
            {
                return string.Format( "{0}{1}{2}, {3}={4}, {5}={6}, {7}={8}", base.DumpInfoToLog, Environment.NewLine, ApplicationConstants.LogSpacingLineup, nameof(Path), Path, nameof(Name), Name, nameof(Enabled), Enabled);
            }
        }

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
