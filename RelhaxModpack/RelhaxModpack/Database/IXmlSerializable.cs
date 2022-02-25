using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Represents an interface that when implemented, will specify which properties of a class
    /// get serialized into xml attributes and elements.
    /// </summary>
    /// <remarks>This is also referenced as "Xml serialization V1".</remarks>
    public interface IXmlSerializable
    {
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        string[] PropertiesForSerializationAttributes();

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        string[] PropertiesForSerializationElements();
    }
}
