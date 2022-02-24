using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// A CoreDatabaseComponent class is an XmlComponent to be saved/loaded to/from xml, but contains a list of maintainers of that component.
    /// </summary>
    public abstract class CoreDatabaseComponent : XmlComponent
    {
        /// <summary>
        /// Create an instance of the CoreDatabaseComponent class.
        /// </summary>
        public CoreDatabaseComponent() : base()
        {

        }

        /// <summary>
        /// Create an instance of the CoreDatabaseComponent class with copying a list of maintainers from a component to copy.
        /// </summary>
        /// <param name="componentToCopy">The component to copy.</param>
        public CoreDatabaseComponent(CoreDatabaseComponent componentToCopy) : base (componentToCopy)
        {
            this.Maintainers = componentToCopy.Maintainers;
        }

        /// <summary>
        /// Reference for the UI element of this package in the database editor.
        /// </summary>
        public TreeViewItem EditorTreeViewItem { get; set; }

        /// <summary>
        /// A list of database managers who are known to maintain this component.
        /// </summary>
        public string Maintainers { get; set; } = string.Empty;

        /// <summary>
        /// The internal ID of the component. Can be anything used to identify it.
        /// </summary>
        /// <remarks>When a databasePackage, the internal packageName. When category, the category name.</remarks>
        public abstract string ComponentInternalName { get; }

        /// <summary>
        /// Returns a list database managers who are known to maintain this component.
        /// </summary>
        public List<string> MaintainersList
        {
            get { return Maintainers.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.0 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            return new List<XmlDatabaseProperty>()
            {
                new XmlDatabaseProperty() { XmlName = nameof(Maintainers), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Maintainers)}
            };
        }

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.1 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot1()
        {
            return this.GetXmlDatabasePropertiesV1Dot0();
        }
    }
}
