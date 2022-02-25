using System.Collections.Generic;
using RelhaxModpack.UI;
using RelhaxModpack.Database;
using System.Windows.Controls;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// a category is what makes up each tab in the mod selection display window. It holds the first level of list of SelectablePackages.
    /// </summary>
    public class Category : CoreDatabaseComponent, IDatabaseComponent, IComponentWithDependencies, IXmlSerializable
    {
        /// <summary>
        /// Create an instance of the Category class.
        /// </summary>
        public Category() : base()
        {

        }

        /// <summary>
        /// Create an instance of the Category class, copying the name and OffsetInstallGroups properties.
        /// </summary>
        /// <param name="categoryToCopy">The category to copy from.</param>
        public Category(Category categoryToCopy) : base(categoryToCopy)
        {
            this.Name = categoryToCopy.Name;
            this.OffsetInstallGroups = categoryToCopy.OffsetInstallGroups;
        }

        #region Xml serialization V1
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(Name), nameof(OffsetInstallGroups) };
        }

        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml elements.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public string[] PropertiesForSerializationElements()
        {
            return new string[] { nameof(Dependencies), nameof(Maintainers) };
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
            List<XmlDatabaseProperty> xmlDatabaseProperties = base.GetXmlDatabasePropertiesV1Dot0();
            List<XmlDatabaseProperty> xmlDatabasePropertiesAddBefore = new List<XmlDatabaseProperty>()
            {
                new XmlDatabaseProperty() { XmlName = nameof(Name), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(Name) },
                new XmlDatabaseProperty() { XmlName = nameof(OffsetInstallGroups), XmlEntryType = Utilities.Enums.XmlEntryType.XmlAttribute, PropertyName = nameof(OffsetInstallGroups) }
            };
            xmlDatabaseProperties.InsertRange(0, xmlDatabasePropertiesAddBefore);
            xmlDatabaseProperties.Add(new XmlDatabaseProperty() { XmlName = nameof(Dependencies), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Dependencies) });
            xmlDatabaseProperties.Add(new XmlDatabaseProperty() { XmlName = nameof(Packages), XmlEntryType = Utilities.Enums.XmlEntryType.XmlElement, PropertyName = nameof(Packages) });
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
            return this.GetXmlDatabasePropertiesV1Dot0();
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

        /// <summary>
        /// A hook from XmlComponent for when an xml entry is being loaded. This can be used to handle custom or one-off conversions to manage any legacy serialization issues (for example, old database formats never had a wrapping "Packages" element).
        /// </summary>
        /// <param name="thisPropertyXml">The xml database property entry being loaded. For example, the "Packages" list entry.</param>
        /// <param name="propertyXmlElement">The xml element entry of the current object being loaded. For example, the "Category" element.</param>
        /// <param name="schemaVersion">The version of the schema currently being loaded.</param>
        /// <param name="propertyInfo">The info meta-data about the property in the class object to be written to. For example, the "Packages" list property.</param>
        /// <param name="valueOfProperty">The current value of the property in the object. For example, this would be the list object of the "Packages" list property.</param>
        /// <param name="elementOfProperty">The xml element entry of the property being loaded. For example, the "Packages" xml entry.</param>
        /// <param name="continueProcessingProperty">A flag used back in XmlComponent if the current xml element to load (like "Packages") should continue to be loaded by XmlComponent.</param>
        /// <seealso cref="XmlComponent.OnParsingPropertyToXmlElement(XmlDatabaseProperty, XElement, string, PropertyInfo, object, XElement, out bool)"/>
        protected override void OnParsingPropertyToXmlElement(XmlDatabaseProperty thisPropertyXml, XElement propertyXmlElement, string schemaVersion, PropertyInfo propertyInfo, object valueOfProperty, XElement elementOfProperty, out bool continueProcessingProperty)
        {
            continueProcessingProperty = true;
            if (thisPropertyXml.PropertyName.Equals(nameof(Packages)) && schemaVersion.Equals(SchemaV1Dot0))
            {
                //manually parse these because in this schema (in converting from old db load method), there is no "Packages" folder for categories
                List<XElement> xmlPackages = propertyXmlElement.Elements("Package").ToList();
                int index = 0;
                foreach(SelectablePackage package in Packages)
                {
                    if (index >= xmlPackages.Count || xmlPackages[index] == null)
                    {
                        XElement element = new XElement("Package");
                        propertyXmlElement.Add(element);
                        xmlPackages = propertyXmlElement.Elements("Package").ToList();
                    }
                    package.ToXml(xmlPackages[index], schemaVersion);
                    index++;
                }

                //remove any extra XElements after the end of the loop
                while (index < xmlPackages.Count)
                {
                    xmlPackages.Last().Remove();
                    xmlPackages = xmlPackages[index].Elements("Package").ToList();
                }
                continueProcessingProperty = false;
            }
        }

        /// <summary>
        /// A hook from XmlComponent for when an xml entry is being saved. This can be used to handle custom or one-off conversions to manage any legacy serialization issues (for example, old database formats never had a wrapping "Packages" element).
        /// </summary>
        /// <param name="thisPropertyXml">The xml database property entry being saved. For example, the "Packages" list entry.</param>
        /// <param name="propertyXmlElement">The xml element entry of the current object being saved. For example, the "Category" element.</param>
        /// <param name="schemaVersion">The version of the schema currently being saved.</param>
        /// <param name="propertyInfo">The info meta-data about the property in the class object to be read from. For example, the "Packages" list property.</param>
        /// <param name="valueOfProperty">The current value of the property in the object. For example, this would be the list object of the "Packages" list property.</param>
        /// <param name="elementOfProperty">The xml element entry of the property being saved. For example, the "Packages" xml entry.</param>
        /// <param name="continueProcessingProperty">A flag used back in XmlComponent if the current xml element to save (like "Packages") should continue to be saved by XmlComponent.</param>
        /// <seealso cref="XmlComponent.OnParsingPropertyFromXmlElement(XmlDatabaseProperty, XElement, string, PropertyInfo, object, XElement, out bool)"/>
        protected override void OnParsingPropertyFromXmlElement(XmlDatabaseProperty thisPropertyXml, XElement propertyXmlElement, string schemaVersion, PropertyInfo propertyInfo, object valueOfProperty, XElement elementOfProperty, out bool continueProcessingProperty)
        {
            continueProcessingProperty = true;
            if (thisPropertyXml.PropertyName.Equals(nameof(Packages)) && schemaVersion.Equals(SchemaV1Dot0))
            {
                //manually parse these because in this schema (in converting from old db load method), there is no "Packages" folder for categories
                List<XElement> xmlPackages = propertyXmlElement.Elements("Package").ToList();

                foreach(XElement element in xmlPackages)
                {
                    object listEntryObject = Activator.CreateInstance(typeof(SelectablePackage));
                    XmlComponent component = listEntryObject as XmlComponent;
                    component.FromXml(element, schemaVersion);
                    Packages.Add(component as SelectablePackage);
                }
                continueProcessingProperty = false;
            }
        }

        /// <summary>
        /// A hook from XmlComponent for when an xml entry is finished being loaded into an object.
        /// </summary>
        /// <param name="propertyElement">The xml element of the entry being loaded. For example, the "Category" xml element.</param>
        /// <param name="loadStatus">The status of the loading of this object, if all properties of it were previously loaded correctly.</param>
        protected override void OnFinishedLoadingFromXml(XElement propertyElement, bool loadStatus)
        {
            base.OnFinishedLoadingFromXml(propertyElement, loadStatus);

            //create the top parent category header
            if (CategoryHeader != null)
                Logging.Warning("The category {0} already has a category header, overwriting", Name);
            CategoryHeader = new SelectablePackage()
            {
                Name = string.Format("----------[{0}]----------", Name),
                TabIndex = TabPage,
                ParentCategory = this,
                Type = SelectionTypes.multi,
                Visible = true,
                Enabled = true,
                Level = -1,
                PackageName = string.Format("Category_{0}_Header", Name.Replace(' ', '_')),
                Packages = Packages
            };

            //also assign the parent references
            CategoryHeader.Parent = CategoryHeader;
            CategoryHeader.TopParent = CategoryHeader;
        }
        #endregion

        #region Database Properties
        /// <summary>
        /// The category name displayed to the user in the selection list.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The xml filename of this category. Used in database structure V2.
        /// </summary>
        public string XmlFilename { get; set; } = string.Empty;

        /// <summary>
        /// Get or set if the installGroup property of all packages in this category will be offset by each package's level in the package tree.
        /// </summary>
        public bool OffsetInstallGroups { get; set; } = true;

        /// <summary>
        /// The list of packages contained in this category.
        /// </summary>
        public List<SelectablePackage> Packages { get; set; } = new List<SelectablePackage>();

        /// <summary>
        /// When a databasePackage, the internal packageName. When category, the category name.
        /// </summary>
        public override string ComponentInternalName { get { return Name; } }

        /// <summary>
        /// List of dependencies of this category (Any package selected in this category needs these dependencies).
        /// </summary>
        public List<DatabaseLogic> Dependencies { get; set; } = new List<DatabaseLogic>();
        #endregion

        #region UI Properties
        /// <summary>
        /// The TabItem object reference.
        /// </summary>
        public SelectionListTabItem TabPage { get; set; } = null;

        /// <summary>
        /// The package created at selection list building that represents the header of this category.
        /// </summary>
        public SelectablePackage CategoryHeader { get; set; } = null;
        #endregion

        #region Other Properties and Methods
        /// <summary>
        /// The DatabaseManager object being used to load and save the category entry.
        /// </summary>
        public DatabaseManager DatabaseManager { get; set; }

        /// <summary>
        /// Sorts the Categories by their name property. Currently not implemented.
        /// </summary>
        /// <param name="x">The first Category to compare.</param>
        /// <param name="y">The second Category to compare.</param>
        /// <returns>1 if y is later in the alphabet, 0 if equal, -1 else.</returns>
        public static int CompareCatagories(Category x, Category y)
        {
            return x.Name.CompareTo(y.Name);
        }

        /// <summary>
        /// Output the object to a string representation.
        /// </summary>
        /// <returns>The name of the category.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns a single depth (flat) list of packages in the category. Leveling is preserved (a sub-package will be directly below the parent in the list).
        /// </summary>
        /// <returns>The list of packages.</returns>
        /// <remarks>Does not include getting the Category's SelectablePackage header.</remarks>
        public List<SelectablePackage> GetFlatPackageList()
        {
            List<SelectablePackage> flatPackageList = new List<SelectablePackage>();
            foreach(SelectablePackage selectablePackage in Packages)
            {
                flatPackageList.Add(selectablePackage);
                if (selectablePackage.Packages.Count > 0)
                    GetFlatPackageList(flatPackageList, selectablePackage.Packages);
            }
            return flatPackageList;
        }

        private void GetFlatPackageList(List<SelectablePackage> flatPackageList, List<SelectablePackage> selectablePackages)
        {
            foreach (SelectablePackage selectablePackage in selectablePackages)
            {
                flatPackageList.Add(selectablePackage);
                if (selectablePackage.Packages.Count > 0)
                    GetFlatPackageList(flatPackageList, selectablePackage.Packages);
            }
        }

        /// <summary>
        /// Check if any packages in this category are selected for install.
        /// </summary>
        /// <returns>Try if any package is selected, false otherwise.</returns>
        /// <remarks>Does not include checking the Category's SelectablePackage header.</remarks>
        public bool AnyPackagesChecked()
        {
            foreach(SelectablePackage package in GetFlatPackageList())
            {
                if (package.Checked)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if at least one package is enabled and checked from the first level of packages in the category.
        /// </summary>
        /// <returns>True if any packages in this category are checked and enabled, false otherwise.</returns>
        /// <remarks>Does not include checking the Category's SelectablePackage header.</remarks>
        public bool IsAnyPackageCheckedEnabled()
        {
            bool anyPackages = false;
            foreach (SelectablePackage sp in this.Packages)
            {
                if (sp.Enabled && sp.Checked)
                    anyPackages = true;
            }
            return anyPackages;
        }

        /// <summary>
        /// Returns true if at least one package is enabled and checked and visible from the first level of packages in the category.
        /// </summary>
        /// <returns>True if at least one package is checked, enabled and visible, false otherwise.</returns>
        /// <remarks>Does not include checking the Category's SelectablePackage header.</remarks>
        public bool IsAnyPackageCheckedEnabledVisible()
        {
            bool anyPackages = false;
            foreach (SelectablePackage sp in this.Packages)
            {
                if (sp.Enabled && sp.Checked && sp.Visible)
                    anyPackages = true;
            }
            return anyPackages;
        }

        /// <summary>
        /// Performs reference linking code for each entry in the database, so that using Parent properties (for example) can allow traversal around the package tree of this category.
        /// </summary>
        /// <remarks>As of this writing, it links the Parent, TopParent, ParentCategory and sets the level of the package in the tree.</remarks>
        public void ProcessPackages()
        {
            foreach (SelectablePackage sp in Packages)
            {
                ProcessPackages(sp, CategoryHeader, 0);
            }
        }

        private void ProcessPackages(SelectablePackage sp, SelectablePackage parent, int level)
        {
            sp.Parent = parent;
            sp.TopParent = this.CategoryHeader;
            sp.ParentCategory = this;
            sp.Level = level;
            foreach (SelectablePackage sp2 in sp.Packages)
            {
                ProcessPackages(sp2, sp, level + 1);
            }
        }
        #endregion
    }
}
