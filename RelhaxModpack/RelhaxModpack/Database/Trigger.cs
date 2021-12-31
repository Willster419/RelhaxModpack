using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Represents a trigger object used in the installer as an event starter.
    /// For example, a contour icon trigger exists to start the building of contour icons
    /// </summary>
    public class Trigger : XmlDatabaseComponent, IXmlSerializable
    {
        #region Xml serialization V1
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes
        /// </summary>
        /// <returns>A list of string property names</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional</remarks>
        public string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(Name) };
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements
        /// </summary>
        /// <returns>A list of string property names</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional</remarks>
        public string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }
        #endregion

        #region Xml serialization V2
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            List<XmlDatabaseProperty> xmlDatabaseProperties = new List<XmlDatabaseProperty>()
            {
                //list attributes
                new XmlDatabaseProperty() { XmlName = nameof(Name), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Name) }
            };
            return xmlDatabaseProperties;
        }
        #endregion

        /// <summary>
        /// The name of the trigger
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total number of instances that this trigger exists in the selected packages to install
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// The number of processed triggers for this trigger type. Prevents the trigger from firing early
        /// </summary>
        public int NumberProcessed { get; set; }

        /// <summary>
        /// Flag to determine if the trigger task has started
        /// </summary>
        public bool Fired { get; set; }

        /// <summary>
        /// The reference for the task that the trigger should perform
        /// </summary>
        public Task TriggerTask { get; set; }
    }
}
