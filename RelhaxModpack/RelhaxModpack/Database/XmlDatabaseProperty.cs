using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// An XmlDatabaseProperty object is a mapping of an xml attribute or element to a property in a class object for serialization.
    /// </summary>
    public class XmlDatabaseProperty
    {
        /// <summary>
        /// Get or set the name of the xml element or attribute.
        /// </summary>
        public string XmlName { get; set; }

        /// <summary>
        /// Get or set the type of xml entry.
        /// </summary>
        public XmlEntryType XmlEntryType { get; set; }

        /// <summary>
        /// Get or set the name of the class property name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// A string representation of the object to display its property information.
        /// </summary>
        /// <returns>The string representing object properties.</returns>
        public override string ToString()
        {
            return string.Format("{0} = {1}, {2} = {3}, {4} = {5}", nameof(XmlName), XmlName, nameof(XmlEntryType), XmlEntryType.ToString(), nameof(PropertyName), PropertyName);
        }
    }
}
