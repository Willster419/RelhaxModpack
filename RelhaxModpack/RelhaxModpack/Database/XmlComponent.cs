using RelhaxModpack.Common;
using RelhaxModpack.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// An XmlComponent is a base class of a database object that can be serialized to and from memory and xml documents.
    /// </summary>
    public abstract class XmlComponent
    {
        /// <summary>
        /// The string to represent the xml schema 1.0.
        /// </summary>
        public const string SchemaV1Dot0 = "1.0";

        /// <summary>
        /// The string to represent the xml schema 1.1.
        /// </summary>
        public const string SchemaV1Dot1 = "1.1";

        /// <summary>
        /// The string to represent the xml schema 1.2.
        /// </summary>
        public const string SchemaV1Dot2 = "1.2";

        /// <summary>
        /// Get or set the schema version that this object was loaded from.
        /// </summary>
        public string LoadedSchemaVersion { get; set; }

        /// <summary>
        /// Create an instance of a parent class of XmlComponent.
        /// </summary>
        public XmlComponent()
        {

        }

        /// <summary>
        /// Create an instance of a a parent class of XmlComponent, copying values from another instance.
        /// </summary>
        /// <param name="componentToCopyFrom">The instance to copy from.</param>
        public XmlComponent(XmlComponent componentToCopyFrom)
        {

        }

        /// <summary>
        /// Gets a list of all properties of a class that are mapped to xml attributes or elements, based on a given schema version.
        /// </summary>
        /// <param name="schemaVersion">The schema version to get the list of properties from.</param>
        /// <returns>The list of properties mapped to xml attributes or elements.</returns>
        public virtual List<XmlDatabaseProperty> GetXmlDatabaseProperties(string schemaVersion)
        {
            switch (schemaVersion)
            {
                case SchemaV1Dot0:
                    return GetXmlDatabasePropertiesV1Dot0();

                case SchemaV1Dot1:
                    return GetXmlDatabasePropertiesV1Dot1();

                case SchemaV1Dot2:
                    return GetXmlDatabasePropertiesV1Dot2();

                default:
                    Logging.Error("Unknown schema type: {0}", string.IsNullOrEmpty(schemaVersion) ? "(null)" : schemaVersion);
                    return null;
            }
        }

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.0 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected abstract List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0();

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.1 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected abstract List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot1();

        /// <summary>
        /// Creates the list of xml components (attributes and elements) to use for xml serialization according to the 1.2 xml schema.
        /// </summary>
        /// <returns>The list of xml components, describing the class property name, xml node name, and xml node type</returns>
        /// <remarks>The order of the properties in the list is used to consider where in the xml document they should be located (it tracks order).</remarks>
        /// <seealso cref="XmlDatabaseProperty"/>
        protected abstract List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot2();

        /// <summary>
        /// Gets the name of the xml element that this type of component is going to be named as, based on the schema version.
        /// </summary>
        /// <param name="schemaVersion">The schema version to get the list of properties from.</param>
        /// <returns>The expected xml element name for the component.</returns>
        public virtual string GetXmlElementName(string schemaVersion)
        {
            return this.GetType().Name;
        }

        /// <summary>
        /// Saves a component to an xml object representing an xml element.
        /// </summary>
        /// <param name="propertyElement">The xml object of the element to save.</param>
        /// <param name="schemaVersion">The schema version to save the element as.</param>
        /// <returns>True if the save operation was successful, false if errors occurred.</returns>
        public virtual bool ToXml(XElement propertyElement, string schemaVersion)
        {
            if (propertyElement == null)
                throw new ArgumentNullException(nameof(propertyElement));
            if (string.IsNullOrEmpty(schemaVersion))
                throw new BadMemeException(string.Format("{0} is null or empty", nameof(schemaVersion)));

            OnStartedSavingToXml(propertyElement, schemaVersion);

            bool good = true;

            //get the list of xml database properties and if they are attributes or elements
            List<XmlDatabaseProperty> xmlDatabaseProperties = this.GetXmlDatabaseProperties(schemaVersion);
            List<XmlDatabaseProperty> propertiesThatAreAttributes = xmlDatabaseProperties.FindAll(property => property.XmlEntryType == Utilities.Enums.XmlEntryType.XmlAttribute);
            List<XmlDatabaseProperty> propertiesThatAreElements = xmlDatabaseProperties.FindAll(property => property.XmlEntryType == Utilities.Enums.XmlEntryType.XmlElement);

            List<XAttribute> attributes = new List<XAttribute>();

            //first handle attributes
            for (int i = 0; i < propertiesThatAreAttributes.Count; i++)
            {
                XmlDatabaseProperty propertyFromXml = propertiesThatAreAttributes[i];
                //get the property that corresponds to the xml attribute entry
                PropertyInfo propertyInfo = this.GetType().GetProperty(propertyFromXml.PropertyName);
                if (propertyInfo == null)
                {
                    Logging.Error("The class property '{0}' does not exist in this database component '{1}' of ID '{2}', line {3}",
                        propertyFromXml.XmlName, propertyElement.Name.LocalName, (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)propertyElement).LineNumber);
                    good = false;
                    continue;
                }

                //get the value of this component's property
                object valueOfProperty = propertyInfo.GetValue(this);
                attributes.Add(new XAttribute(propertyFromXml.XmlName, valueOfProperty.ToString()));
            }
            //replace all the attributes cause we can't edit individual lists of attributes. why would we want to do that, microsoft??
            propertyElement.ReplaceAttributes(attributes);

            //then handle elements. create a temp version for its default values
            object objectForDefaults = Activator.CreateInstance(this.GetType());
            for (int i = 0; i < propertiesThatAreElements.Count; i++)
            {
                XmlDatabaseProperty propertyFromXml = propertiesThatAreElements[i];
                //get the property that corresponds to the xml attribute entry
                PropertyInfo propertyInfo = this.GetType().GetProperty(propertyFromXml.PropertyName);
                if (propertyInfo == null)
                {
                    Logging.Error("The class property '{0}' does not exist in this database component '{1}' of ID '{2}', line {3}",
                        propertyFromXml.XmlName, propertyElement.Name.LocalName, (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)propertyElement).LineNumber);
                    good = false;
                    continue;
                }

                //get the value of this component's property
                object valueOfProperty = propertyInfo.GetValue(this);

                //get the xml element that corresponds to this object's property
                XElement element = propertyElement.Elements().ToList().Find(elementToFind => elementToFind.Name.LocalName.ToLower().Equals(propertyFromXml.XmlName.ToLower()));

                OnParsingPropertyToXmlElement(propertyFromXml, propertyElement, schemaVersion, propertyInfo, valueOfProperty, element, out bool continueProcessing);
                if (!continueProcessing)
                    continue;

                //check if the property is a list type. if it is, then we need to load it by creating it and calling it's own version of FromXml
                if (valueOfProperty is IList list)
                {
                    //if the element entry in the xml is null (nothing in it) and the count of items in the list is 0, (nothing in it), then nothing to do
                    if (element == null && list.Count == 0)
                        continue;
                    else if (list.Count == 0)
                    {
                        element.Remove();
                        continue;
                    }
                    else if (element == null)
                    {
                        element = new XElement(propertyFromXml.XmlName);
                        if (i+1 == propertiesThatAreElements.Count)
                            propertyElement.Add(element);
                        else
                        {
                            bool valueApplied = false;
                            XmlDatabaseProperty nextPropertyFromXml;
                            XElement elementAfterThisOne;
                            for (int j = i + 1; j < propertiesThatAreElements.Count; j++)
                            {
                                nextPropertyFromXml = propertiesThatAreElements[j];
                                elementAfterThisOne = propertyElement.Elements().ToList().Find(elementToFind => elementToFind.Name.LocalName.ToLower().Equals(nextPropertyFromXml.XmlName.ToLower()));
                                if (elementAfterThisOne != null)
                                {
                                    valueApplied = true;
                                    elementAfterThisOne.AddBeforeSelf(element);
                                    break;
                                }
                            }
                            if (!valueApplied)
                                propertyElement.Add(element);
                        }
                    }

                    //get type of object that this list stores
                    //https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item//https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item
                    Type listObjectType = list.GetType().GetInterfaces().Where(j => j.IsGenericType && j.GenericTypeArguments.Length == 1).FirstOrDefault(j => j.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];

                    //get all elements of this xml element (for example, get all Media objects of xml element/list Medias
                    int index = 0;
                    List<XElement> elements = element.Elements().ToList();
                    foreach (object obj in list)
                    {
                        if (obj == null)
                        {
                            Logging.Error("List object is null in XmlDatabaseComponent: '{0}' of database component '{1}' of ID '{2}', line {3}",
                                obj.GetType().ToString(), propertyElement.Name.LocalName, (this is CoreDatabaseComponent) ? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)element).LineNumber);
                            continue;
                        }

                        //check to make sure an entry exists
                        if (index >= elements.Count || elements[index] == null)
                        {
                            string elementName = listObjectType.Name;
                            if (obj is XmlComponent comp)
                                elementName = comp.GetXmlElementName(schemaVersion);
                            element.Add(new XElement(elementName, null));
                            elements = element.Elements().ToList();
                        }

                        //check if the list entry is a XmlDatabaseComponent or a value type
                        if (obj is XmlComponent component)
                        {
                            component.ToXml(elements[index], schemaVersion);
                        }
                        else if (obj.GetType().IsValueType || obj.GetType().Equals(typeof(string)))
                        {
                            XElement listElement = elements[index];
                            string objectString = obj.ToString();
                            if (listElement.Value != objectString)
                                listElement.Value = objectString;
                        }
                        else
                        {
                            Logging.Error("Unknown class type to save in schema that is not of XmlDatabaseComponent: '{0}' of database component '{1}' of ID '{2}', line {3}",
                                obj.GetType().ToString(), propertyElement.Name.LocalName, (this is CoreDatabaseComponent) ? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)element).LineNumber);
                            break;
                        }
                        index++;
                    }

                    //remove any extras that exist on the xml side
                    while (index < elements.Count)
                    {
                        elements.Last().Remove();
                        elements = element.Elements().ToList();
                    }
                }
                else
                {
                    //get the default value
                    object defaultValueOfProperty = propertyInfo.GetValue(objectForDefaults);

                    //check if the default value matches what the current value is
                    bool valueIsDefault = false;
                    if (defaultValueOfProperty == null && valueOfProperty == null)
                        valueIsDefault = true;
                    else if (defaultValueOfProperty != null && defaultValueOfProperty.Equals(valueOfProperty))
                        valueIsDefault = true;

                    //if the element value is null, then remove the element value if it exists
                    if (valueOfProperty == null || valueIsDefault)
                    {
                        if (element != null)
                            element.Remove();
                        continue;
                    }

                    if (element == null)
                    {
                        element = new XElement(propertyFromXml.XmlName, valueOfProperty.ToString());
                        if (i+1 == propertiesThatAreElements.Count)
                            propertyElement.Add(element);
                        else
                        {
                            bool valueApplied = false;
                            XmlDatabaseProperty nextPropertyFromXml;
                            XElement elementAfterThisOne;
                            for (int j = i+1; j < propertiesThatAreElements.Count; j++)
                            {
                                nextPropertyFromXml = propertiesThatAreElements[j];
                                elementAfterThisOne = propertyElement.Elements().ToList().Find(elementToFind => elementToFind.Name.LocalName.ToLower().Equals(nextPropertyFromXml.XmlName.ToLower()));
                                if (elementAfterThisOne != null)
                                {
                                    valueApplied = true;
                                    elementAfterThisOne.AddBeforeSelf(element);
                                    break;
                                }
                            }
                            if (!valueApplied)
                                propertyElement.Add(element);
                        }
                    }
                    else
                    {
                        string objectString = valueOfProperty.ToString();
                            
                        // check if it's in sync with the element value. A null value is treated as out of date
                        bool valuesInSync = objectString.Equals(element.Value);
                        if (!valuesInSync)
                            element.Value = objectString;
                    }
                }
            }

            OnFinishedSavingToXml(propertyElement, schemaVersion, good);

            return good;
        }

        /// <summary>
        /// Loads a component from an xml object representing an xml element.
        /// </summary>
        /// <param name="propertyElement">The xml object of the element to load from.</param>
        /// <param name="schemaVersion">The schema version to load the element as.</param>
        /// <returns></returns>
        public virtual bool FromXml(XElement propertyElement, string schemaVersion)
        {
            if (propertyElement == null)
                throw new ArgumentNullException(nameof(propertyElement));
            if (string.IsNullOrEmpty(schemaVersion))
                throw new BadMemeException(string.Format("{0} is null or empty", nameof(schemaVersion)));

            LoadedSchemaVersion = schemaVersion;

            OnStartedLoadingFromXml(propertyElement);

            bool good = true;

            //get the list of xml database properties and if they are attributes or elements
            List<XmlDatabaseProperty> xmlDatabaseProperties = this.GetXmlDatabaseProperties(schemaVersion);
            List<XmlDatabaseProperty> propertiesThatAreAttributes = xmlDatabaseProperties.FindAll(property => property.XmlEntryType == Utilities.Enums.XmlEntryType.XmlAttribute);
            List<XmlDatabaseProperty> propertiesThatAreElements = xmlDatabaseProperties.FindAll(property => property.XmlEntryType == Utilities.Enums.XmlEntryType.XmlElement);

            //first handle attributes
            foreach (XmlDatabaseProperty propertyFromXml in propertiesThatAreAttributes)
            {
                //get the attribute from the element, ignoring case
                XAttribute attribute = propertyElement.Attributes().ToList().Find(attributeToFind => attributeToFind.Name.LocalName.ToLower().Equals(propertyFromXml.XmlName.ToLower()));
                if (attribute == null)
                {
                    Logging.Error("The xml attribute '{0}' does not exist for the element '{1}', line {2}", propertyFromXml.XmlName, propertyElement.Name.LocalName, ((IXmlLineInfo)propertyElement).LineNumber);
                    good = false;
                    continue;
                }

                //get the propertyInfo object representing the same name corresponding field or property in the memory database entry
                PropertyInfo propertyInfo = this.GetType().GetProperty(propertyFromXml.PropertyName);
                if (propertyInfo == null)
                {
                    Logging.Error("The xml attribute '{0}' does not exist as a property in this database component '{1}' of ID '{2}', line {3}",
                        propertyFromXml.XmlName, propertyElement.Name.LocalName, (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)attribute).LineNumber);
                    good = false;
                    continue;
                }

                //get the value of this component's property
                object valueOfProperty = propertyInfo.GetValue(this);

                if (!UpdatePropertyValue(valueOfProperty, attribute, propertyInfo, attribute.Value))
                    good = false;
            }

            //then handle elements
            foreach (XmlDatabaseProperty propertyFromXml in propertiesThatAreElements)
            {
                //get the element, ignoring case. If it does not exist, then we assume that the property is to take the default value
                XElement element = propertyElement.Elements().ToList().Find(elementToFind => elementToFind.Name.LocalName.ToLower().Equals(propertyFromXml.XmlName.ToLower()));

                //get the propertyInfo object representing the same name corresponding field or property in the memory database entry
                PropertyInfo propertyInfo = this.GetType().GetProperty(propertyFromXml.PropertyName);
                if (propertyInfo == null)
                {
                    Logging.Error("The xml attribute '{0}' does not exist as a property in this database component '{1}' of ID '{2}', line {3}",
                        propertyFromXml.XmlName, propertyElement.Name.LocalName, (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)element).LineNumber);
                    good = false;
                    continue;
                }

                //get the value of this component's property
                object valueOfProperty = propertyInfo.GetValue(this);

                OnParsingPropertyFromXmlElement(propertyFromXml, propertyElement, schemaVersion, propertyInfo, valueOfProperty, element, out bool continueProcessingProperty);

                if (!continueProcessingProperty || element == null)
                    continue;

                //check if the property is a list type. if it is, then we need to load it by creating it and calling it's own version of FromXml
                if (valueOfProperty is IList list)
                {
                    //get type of object that this list stores
                    //https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item//https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item
                    Type listObjectType = list.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1).FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];

                    //get all elements of this xml element (for example, get all Media objects of xml element/list Medias
                    int index = 0;
                    foreach (XElement listElement in element.Elements())
                    {
                        object listEntryObject = null;

                        bool isvalueOrString = listObjectType.IsValueType || listObjectType.Equals(typeof(string));

                        //don't construct an object if it's not a class/struct
                        if (!isvalueOrString)
                            listEntryObject = Activator.CreateInstance(listObjectType);

                        if (!isvalueOrString && listEntryObject is XmlComponent xmlDatabaseComponent)
                        {
                            //load this object from xml
                            xmlDatabaseComponent.FromXml(listElement, schemaVersion);
                            list.Add(listEntryObject);
                        }
                        else if (isvalueOrString)
                        {
                            //get the element in the list here
                            object entryInList = null;
                            if (index < list.Count)
                                entryInList = list[index];

                            //if they are not equal, then update the property
                            if (entryInList == null || !entryInList.ToString().Equals(listElement.Value))
                            {
                                bool setValue = CommonUtils.SetListIndexValueType(list, index, listObjectType, listElement.Value);
                                index++;
                                if (!setValue)
                                {
                                    Logging.Error("Failed to set value of list '{0}' index value {1} of component '{2}', ID {3}, line {4}",
                                        propertyInfo.Name, index-1, this.GetType().ToString(), (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)listElement).LineNumber);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            Logging.Error("Unknown class type to load that is not of XmlDatabaseComponent: '{0}' of database component '{1}' of ID '{2}', line {3}",
                                listEntryObject.GetType().ToString(), propertyElement.Name.LocalName, (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)element).LineNumber);
                            index++;
                            continue;
                        }
                    }
                }
                //else try to update the property if it needs it
                else
                {
                    if (!UpdatePropertyValue(valueOfProperty, element, propertyInfo, element.Value))
                        good = false;
                }
            }

            OnFinishedLoadingFromXml(propertyElement, good);

            return good;
        }

        /// <summary>
        /// Attempts to update a property's value if the current value does not match.
        /// </summary>
        /// <param name="valueOfProperty">The current value of the property.</param>
        /// <param name="xmlObject">The xml object that represents the container of this element. Only used for logging.</param>
        /// <param name="propertyInfo">The property metadata object of the target property to update.</param>
        /// <param name="xmlValue">The value from the xml element or attribute.</param>
        /// <returns>True if the value is up to date or updated, false if the value was unable to be updated.</returns>
        protected virtual bool UpdatePropertyValue(object valueOfProperty, XObject xmlObject, PropertyInfo propertyInfo, string xmlValue)
        {
            //check if it's in sync with the attribute value. A null value is treated as out of date
            bool valuesInSync = valueOfProperty == null ? false : valueOfProperty.ToString().Equals(xmlValue);

            //only try to set the value if the property and xml value are not in sync
            if (!valuesInSync)
            {
                if (!CommonUtils.SetObjectProperty(this, propertyInfo, xmlValue))
                {
                    Logging.Error("Failed to set property '{0}' of component '{1}', ID {2}, line {3}",
                        propertyInfo.Name, this.GetType().ToString(), (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)xmlObject).LineNumber);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// A hook for when an xml entry is being loaded. This can be used to handle custom or one-off conversions or implementations to manage any legacy serialization issues (for example, old database formats never had a wrapping "Packages" element).
        /// </summary>
        /// <param name="thisPropertyXml">The xml database property entry being loaded. For example, the "Packages" list entry.</param>
        /// <param name="propertyXmlElement">The xml element entry of the current object being loaded. For example, the "Category" element.</param>
        /// <param name="schemaVersion">The version of the schema currently being loaded.</param>
        /// <param name="propertyInfo">The info meta-data about the property in the class object to be written to. For example, the "Packages" list property.</param>
        /// <param name="valueOfProperty">The current value of the property in the object. For example, this would be the list object of the "Packages" list property.</param>
        /// <param name="elementOfProperty">The xml element entry of the property being loaded. For example, the "Packages" xml entry.</param>
        /// <param name="continueProcessingProperty">A flag used back in XmlComponent if the current xml element to load (like "Packages") should continue to be loaded by XmlComponent.</param>
        protected virtual void OnParsingPropertyToXmlElement(XmlDatabaseProperty thisPropertyXml, XElement propertyXmlElement, string schemaVersion, PropertyInfo propertyInfo, object valueOfProperty, XElement elementOfProperty, out bool continueProcessingProperty)
        {
            //stub
            continueProcessingProperty = true;
        }

        /// <summary>
        /// A hook for when an xml entry is being saved. This can be used to handle custom or one-off conversions to manage any legacy serialization issues (for example, old database formats never had a wrapping "Packages" element).
        /// </summary>
        /// <param name="thisPropertyXml">The xml database property entry being saved. For example, the "Packages" list entry.</param>
        /// <param name="propertyXmlElement">The xml element entry of the current object being saved. For example, the "Category" element.</param>
        /// <param name="schemaVersion">The version of the schema currently being saved.</param>
        /// <param name="propertyInfo">The info meta-data about the property in the class object to be read from. For example, the "Packages" list property.</param>
        /// <param name="valueOfProperty">The current value of the property in the object. For example, this would be the list object of the "Packages" list property.</param>
        /// <param name="elementOfProperty">The xml element entry of the property being saved. For example, the "Packages" xml entry.</param>
        /// <param name="continueProcessingProperty">A flag used back in XmlComponent if the current xml element to save (like "Packages") should continue to be saved by XmlComponent.</param>
        protected virtual void OnParsingPropertyFromXmlElement(XmlDatabaseProperty thisPropertyXml, XElement propertyXmlElement, string schemaVersion, PropertyInfo propertyInfo, object valueOfProperty, XElement elementOfProperty, out bool continueProcessingProperty)
        {
            //stub
            continueProcessingProperty = true;
        }

        /// <summary>
        /// A hook from XmlComponent for when an xml entry is started being loaded into an object.
        /// </summary>
        /// <param name="propertyElement">The xml element of the entry being loaded.</param>
        protected virtual void OnStartedLoadingFromXml(XElement propertyElement)
        {

        }

        /// <summary>
        /// A hook from XmlComponent for when an xml entry is finished being loaded into an object.
        /// </summary>
        /// <param name="propertyElement">The xml element of the entry being loaded.</param>
        /// <param name="loadStatus">The status of the loading of this object, if all properties of it were previously loaded correctly.</param>
        protected virtual void OnFinishedLoadingFromXml(XElement propertyElement, bool loadStatus)
        {

        }

        /// <summary>
        /// A hook from XmlComponent for when an xml entry is started being saved to an xml document object.
        /// </summary>
        /// <param name="propertyElement">The xml element of the entry being saved.</param>
        /// <param name="targetSchemaVersion">The schema version being used to save.</param>
        protected virtual void OnStartedSavingToXml(XElement propertyElement, string targetSchemaVersion)
        {

        }

        /// <summary>
        /// A hook from XmlComponent for when an xml entry is finished being saved to an xml document object.
        /// </summary>
        /// <param name="propertyElement">The xml element of the entry being saved.</param>
        /// <param name="targetSchemaVersion">The schema version being used to save.</param>
        /// <param name="saveStatus">The status of the saving of this object, if all properties of it were previously saved correctly.</param>
        protected virtual void OnFinishedSavingToXml(XElement propertyElement, string targetSchemaVersion, bool saveStatus)
        {

        }
    }
}
