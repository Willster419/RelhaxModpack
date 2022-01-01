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
    public abstract class XmlDatabaseComponent
    {
        public const string SchemaV1Dot0 = "1.0";

        public string XmlElementName { get { return this.GetType().Name; } }

        public virtual List<XmlDatabaseProperty> GetXmlDatabaseProperties(string schemaVersion)
        {
            switch (schemaVersion)
            {
                case SchemaV1Dot0:
                    return GetXmlDatabasePropertiesV1Dot0();

                default:
                    Logging.Error("Unknown schema type: {0}", string.IsNullOrEmpty(schemaVersion) ? "(null)" : schemaVersion);
                    return null;
            }
        }

        protected abstract List<XmlDatabaseProperty> GetXmlDatabasePropertiesV1Dot0();

        public virtual bool ToXml(XElement propertyElement, string schemaVersion)
        {
            if (propertyElement == null)
                throw new ArgumentNullException(nameof(propertyElement));
            if (string.IsNullOrEmpty(schemaVersion))
                throw new BadMemeException(string.Format("{0} is null or empty", nameof(schemaVersion)));

            bool good = true;

            //get the list of xml database properties and if they are attributes or elements
            List<XmlDatabaseProperty> xmlDatabaseProperties = this.GetXmlDatabaseProperties(schemaVersion);
            List<XmlDatabaseProperty> propertiesThatAreAttributes = xmlDatabaseProperties.FindAll(property => property.XmlEntryType == Utilities.Enums.XmlEntryType.XmlAttribute);
            List<XmlDatabaseProperty> propertiesThatAreElements = xmlDatabaseProperties.FindAll(property => property.XmlEntryType == Utilities.Enums.XmlEntryType.XmlElement);

            //first handle attributes
            foreach (XmlDatabaseProperty propertyFromXml in propertiesThatAreAttributes)
            {
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

                //get the xml attribute that corresponds to this object's property. if it does not exist, then create it
                XAttribute attribute = propertyElement.Attributes().ToList().Find(attributeToFind => attributeToFind.Name.LocalName.ToLower().Equals(propertyFromXml.XmlName.ToLower()));
                if (attribute == null)
                    attribute = new XAttribute(propertyFromXml.XmlName, valueOfProperty);
                else
                {
                    // check if it's in sync with the attribute value. A null value is treated as out of date
                    bool valuesInSync = valueOfProperty.ToString().Equals(attribute.Value);
                    if (!valuesInSync)
                        attribute.Value = valueOfProperty.ToString();
                }
            }

            //then handle elements. create a temp version for its default values
            object objectForDefaults = Activator.CreateInstance(this.GetType());
            foreach (XmlDatabaseProperty propertyFromXml in propertiesThatAreElements)
            {
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
                    else if (element == null)
                    {
                        element = new XElement(propertyFromXml.XmlName);
                        propertyElement.Add(element);
                    }

                    //get type of object that this list stores
                    //https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item//https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item
                    Type listObjectType = list.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1).FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];

                    //get all elements of this xml element (for example, get all Media objects of xml element/list Medias
                    int index = 0;
                    List<XElement> elements = element.Elements().ToList();
                    foreach (object obj in list)
                    {
                        //check to make sure an entry exists
                        if (elements[index] == null)
                        {
                            elements.Add(new XElement(XmlElementName, null));
                            elements = element.Elements().ToList();
                        }

                        //check if the list entry is a XmlDatabaseComponent or a value type
                        if (obj is XmlDatabaseComponent component)
                        {
                            component.ToXml(elements[index], schemaVersion);
                        }
                        else if (obj.GetType().IsValueType)
                        {
                            XElement listElement = elements[index];
                            if (listElement.Value != obj.ToString())
                                listElement.Value = obj.ToString();
                        }
                        else
                        {
                            Logging.Error("Unknown class type to save in schema that is not of XmlDatabaseComponent: '{0}' of database component '{1}' of ID '{2}', line {3}",
                                obj.GetType().ToString(), propertyElement.Name.LocalName, (this is CoreDatabaseComponent) ? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)element).LineNumber);
                            break;
                        }
                        index++;
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
                        element = new XElement(propertyFromXml.XmlName, valueOfProperty);
                        propertyElement.Add(element);
                    }
                    else
                    {
                        string value = valueOfProperty.ToString();
                        // check if it's in sync with the element value. A null value is treated as out of date
                        bool valuesInSync = value.Equals(element.Value);
                        if (!valuesInSync)
                            element.Value = value;
                    }
                }
            }

            return good;
        }

        public virtual bool FromXml(XElement propertyElement, string schemaVersion)
        {
            if (propertyElement == null)
                throw new ArgumentNullException(nameof(propertyElement));
            if (string.IsNullOrEmpty(schemaVersion))
                throw new BadMemeException(string.Format("{0} is null or empty", nameof(schemaVersion)));

            bool good = true;

            //get the list of xml database properties and if they are attributes or elements
            List<XmlDatabaseProperty> xmlDatabaseProperties = this.GetXmlDatabaseProperties(schemaVersion);
            List<XmlDatabaseProperty> propertiesThatAreAttributes = xmlDatabaseProperties.FindAll(property => property.XmlEntryType == Utilities.Enums.XmlEntryType.XmlAttribute);
            List<XmlDatabaseProperty> propertiesThatAreElements = xmlDatabaseProperties.FindAll(property => property.XmlEntryType == Utilities.Enums.XmlEntryType.XmlAttribute);

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
                if (element == null)
                    continue;

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

                //check if the property is a list type. if it is, then we need to load it by creating it and calling it's own version of FromXml
                if (valueOfProperty.GetType() is IList list)
                {
                    //get type of object that this list stores
                    //https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item//https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item
                    Type listObjectType = list.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1).FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];

                    //get all elements of this xml element (for example, get all Media objects of xml element/list Medias
                    int index = 0;
                    foreach (XElement listElement in element.Elements())
                    {
                        object listEntryObject = Activator.CreateInstance(listObjectType);
                        if (listEntryObject is XmlDatabaseComponent)
                        {
                            XmlDatabaseComponent xmlDatabaseComponent = listEntryObject as XmlDatabaseComponent;
                            //load this object from xml
                            xmlDatabaseComponent.FromXml(listElement, schemaVersion);
                        }
                        else if (listEntryObject.GetType().IsValueType)
                        {
                            //get the element in the list here
                            object entryInList = list[index];
                            
                            //if they are not equal, then update the property
                            if (entryInList == null || !entryInList.ToString().Equals(listElement.Value))
                            {
                                if (!CommonUtils.SetListIndexValueType(list, index, listObjectType, listElement.Value))
                                {
                                    Logging.Error("Failed to set value of list '{0}' index value {1} of component '{2}', ID {3}, line {4}",
                                        propertyInfo.Name, index, this.GetType().ToString(), (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)listElement).LineNumber);
                                }
                            }
                            index++;
                        }
                        else
                        {
                            Logging.Error("Unknown class type to load that is not of XmlDatabaseComponent: '{0}' of database component '{1}' of ID '{2}', line {3}",
                                listEntryObject.GetType().ToString(), propertyElement.Name.LocalName, (this is CoreDatabaseComponent)? (this as CoreDatabaseComponent).ComponentInternalName : "N/A", ((IXmlLineInfo)element).LineNumber);
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

            return good;
        }

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

        protected virtual void OnParsingPropertyToXmlElement(XmlDatabaseProperty propertyFromXml, XElement propertyElement, string schemaVersion, PropertyInfo propertyInfo, object valueOfProperty, XElement elementOfProperty, out bool continueProcessingProperty)
        {
            //stub
            continueProcessingProperty = true;
        }
    }
}
