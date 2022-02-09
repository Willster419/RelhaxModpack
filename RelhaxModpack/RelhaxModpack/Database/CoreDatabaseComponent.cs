using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RelhaxModpack.Database
{
    public abstract class CoreDatabaseComponent : XmlComponent
    {
        public CoreDatabaseComponent() : base()
        {

        }

        public CoreDatabaseComponent(CoreDatabaseComponent componentToCopy) : base (componentToCopy)
        {
            this.Maintainers = componentToCopy.Maintainers;
        }

        /// <summary>
        /// Reference for the UI element of this package in the database editor
        /// </summary>
        TreeViewItem EditorTreeViewItem { get; set; }

        /// <summary>
        /// A list of database managers who are known to maintain this component
        /// </summary>
        public string Maintainers { get; set; } = string.Empty;

        /// <summary>
        /// The internal ID of the component. Can be anything used to identify it.
        /// </summary>
        /// <remarks>When a databasePackage, the internal packageName. When category, the category name.</remarks>
        public abstract string ComponentInternalName { get; }

        /// <summary>
        /// Returns a list database managers who are known to maintain this component
        /// </summary>
        public List<string> MaintainersList
        {
            get { return Maintainers.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }

        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0()
        {
            return new List<XmlDatabaseProperty>()
            {
                new XmlDatabaseProperty() { XmlName = nameof(Maintainers), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Maintainers)}
            };
        }

        protected override List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot1()
        {
            return this.GetXmlDatabasePropertiesV1Dot0();
        }
    }
}
