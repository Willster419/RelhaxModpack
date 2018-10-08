using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;

namespace RelhaxModpack
{
    /// <summary>
    /// Utility class for all XML static methods
    /// </summary>
    public static class XMLUtils
    {
        //static path for all developer selections
        public const string DeveloperSelectionsXPath = "TODO";
        #region Components of packages
        //for base package
        public static readonly string[] BaseRequiredAttributes = new string[] { "PackageName", "Enabled" };
        public static readonly string[] BaseOptionalAttributes = new string[] { "Version", "Timestamp", "ZipFile", "CRC", "StartAddress",
            "EndAddress", "AppendExtraction", "LogAtInstall", "DevURL", "ExtractionLevel" };
        //for dependency
        public static readonly string[] DependnecyRequiredAttributes = new string[] { "LogicType" };
        //for selectable package
        public static readonly string[] SelectablePackageRequiredAttributes = new string[] { "Name", "Type" };
        public static readonly string[] SelectablePackageOptionalAttributes = new string[] { "Visible", "Size", "UpdateComment", "Description" };
        public static readonly string[] SelectablePackagOptionaleNodes = new string[] { "UserFiles", "Packages", "PictureList" };
        //for both:
        public static readonly string[] DependencySelectableOptionalNodes = new string[] { "Dependencies" };
        //inside packages - component required attributes
        public static readonly string[] DependnecyLogicRequiredAttributes = new string[] { "NegateFlag", "PackageName" };
        public static readonly string[] MediaRequiredAttributes = new string[] { "URL", "MediaType" };
        public static readonly string[] UserfilesRequiredAttributes = new string[] { "Pattern" };
        #endregion
        //check to make sure an xml file is valid
        public static bool IsValidXml(string filePath)
        {
            return IsValidXml(File.ReadAllText(filePath), Path.GetFileName(filePath));
        }
        //check to make sure an xml file is valid
        public static bool IsValidXml(string xmlString, string fileName)
        {
            using (XmlTextReader read = new XmlTextReader(xmlString))
            {
                try
                {
                    //continue to read the entire document
                    while (read.Read()) ;
                    read.Close();
                    return true;
                }
                catch (Exception e)
                {
                    Logging.WriteToLog(string.Format("Invalid XML file: {0}\n{1}",fileName,e.Message),Logfiles.Application, LogLevel.Error);
                    read.Close();
                    return false;
                }
            }
        }
        //get an xml element attribute given an xml path
        public static string GetXMLStringFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(file),e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLStringFromXPath(doc, xpath);
        }
        //get an xml element attribute given an xml path
        public static string GetXMLStringFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLStringFromXPath(doc, xpath);
        }
        //get an xml element attribute given an xml path
        //element example: "//root/element"
        //attribute example: "//root/element/@attribute"
        //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
        //for the folder version: //modInfoAlpha.xml/@version
        public static string GetXMLStringFromXPath(XmlDocument doc, string xpath)
        {
            XmlNode result = doc.SelectSingleNode(xpath);
            if (result == null)
                return null;
            return result.InnerText;
        }
        //get an xml element attribute given an xml path
        public static XmlNode GetXMLNodeFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(file), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLNodeFromXPath(doc, xpath);
        }
        //get an xml element attribute given an xml path
        public static XmlNode GetXMLNodeFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLNodeFromXPath(doc, xpath);
        }
        //same as above but in a node
        public static XmlNode GetXMLNodeFromXPath(XmlDocument doc, string xpath)
        {
          XmlNode result = doc.SelectSingleNode(xpath);
          if (result == null)
              return null;
          return result;
        }
        //get an xml element attribute given an xml path
        public static XmlNodeList GetXMLNodesFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(file), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLNodesFromXPath(doc, xpath);
        }
        //get an xml element attribute given an xml path
        public static XmlNodeList GetXMLNodesFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLNodesFromXPath(doc, xpath);
        }
        //same as above but in a node collection
        //NOTE: XmlElement and XmlAttribute inherit from XmlNode
        public static XmlNodeList GetXMLNodesFromXPath(XmlDocument doc, string xpath)
        {
            XmlNodeList results = doc.SelectNodes(xpath);
          if (results == null || results.Count == 0)
          {
            return null;
          }
          return results;
        }
        public static bool ParseDatabase(XmlDocument modInfoDocument, List<Dependency> globalDependencies,
            List<Dependency> logicalDependencies, List<Category> parsedCategoryList)
        {
            Logging.WriteToLog("start of ParseDatabase()", Logfiles.Application, LogLevel.Debug);
            //check all input parameters
            if (modInfoDocument == null)
                throw new BadMemeException("modInfoDocument is null dumbass");
            if (globalDependencies == null)
                globalDependencies = new List<Dependency>();
            else
                globalDependencies.Clear();
            if (logicalDependencies == null)
                logicalDependencies = new List<Dependency>();
            else
                logicalDependencies.Clear();
            if (parsedCategoryList == null)
                parsedCategoryList = new List<Category>();
            else
                parsedCategoryList.Clear();
            //determine which version of the document we are loading. allows for loading of different versions if structure change
            string versionString = GetXMLStringFromXPath(modInfoDocument, "//modInfoAlpha.xml@documentVersion");
            Logging.WriteToLog(nameof(versionString) + "=" + versionString, Logfiles.Application, LogLevel.Debug);
            switch(versionString)
            {
                case "2.0":
                    return ParseDatabaseV2(DocumentToXDocument(modInfoDocument), globalDependencies, logicalDependencies, parsedCategoryList);
                default:
                    Logging.WriteToLog(string.Format("unknown format of {0}: {1}", nameof(versionString), versionString),
                        Logfiles.Application, LogLevel.Error);
                    return false;
            }
        }
        public static bool ParseDatabaseV2(XDocument modInfoDocument, List<Dependency> globalDependencies,
            List<Dependency> logicalDependencies, List<Category> parsedCategoryList)
        {
            //parsing the global dependencies
            bool globalParsed = ParseDatabaseV2GlobalDependencies(
                modInfoDocument.XPathSelectElements("/modInfoAlpha.xml/globaldependencies/globaldependency").ToList(), globalDependencies);
            //parsing the logical dependnecies
            bool logicalDepParsed = ParseDatabaseV2LogicalDependencies(modInfoDocument.XPathSelectElements(
                "/modInfoAlpha.xml/logicalDependencies/logicalDependency").ToList(), logicalDependencies);
            //parsing the categories
            bool categoriesParsed = ParseDatabaseV2Categories(modInfoDocument.XPathSelectElements(
                "/modInfoAlpha.xml/catagories/catagory").ToList(), parsedCategoryList);
            return (globalParsed && logicalDepParsed && categoriesParsed) ? true : false;
        }
        private static bool ParseDatabaseV2GlobalDependencies(List<XElement> holder, List<Dependency> globalDependencies)
        {
            //first for loop is for each "dependency" object holder
            foreach(XElement dependency in holder)
            {
                Dependency globalDependency = new Dependency();
                
            }
            return true;
        }
        private static void DynamicXMLParse(XElement dependency, ref DatabasePackage package)
        {
            //first treat it as regular package
            //check for all fields in it
            List<string> copyOfRequiredBaseAttributes = new List<string>(BaseRequiredAttributes.ToList());
            List<string> unknownElements = new List<string>();
            foreach(XAttribute attribute in dependency.Attributes())//ATTRIBUTES
            {
                string attributeName = attribute.Name.LocalName;
                if (copyOfRequiredBaseAttributes.Contains(attributeName))
                {
                    //switch
                    copyOfRequiredBaseAttributes.Remove(attributeName);
                }
                else if (BaseOptionalAttributes.Contains(attributeName))
                {
                    //switch

                }
                else
                    unknownElements.Add(attributeName);
            }
            //check if it's logical or selectable
            if(package is Dependency dep)
            {
                DynamicXMLParse(dependency, ref dep, ref unknownElements);
            }
            else if (package is SelectablePackage sp)
            {
                DynamicXMLParse(dependency, ref sp, ref unknownElements);
            }
            else
            {
                Logging.WriteToLog("Package type should not get here", Logfiles.Application, LogLevel.Error);
            }
        }
        private static void DynamicXMLParse(XElement dependency, ref Dependency package, ref List<string> unknownElements)
        {
            //copy list of required

            //process list of attributes

            //process list of nodes
            /*foreach(XElement element in dependency.Elements())//ELEMENTS
            {
                string elementName = element.Name.LocalName;
                if (OptionalBaseNodes.Contains(elementName))
                {
                    //switch
                }
                else
                    unknownElements.Add(elementName);
            }*/
        }
        private static void DynamicXMLParse(XElement dependency, ref SelectablePackage package, ref List<string> unknownElements)
        {
            //copy list of required

            //process list of attributes

            //process list of nodes
            /*foreach(XElement element in dependency.Elements())//ELEMENTS
            {
                string elementName = element.Name.LocalName;
                if (OptionalBaseNodes.Contains(elementName))
                {
                    //switch
                }
                else
                    unknownElements.Add(elementName);
            }*/
        }
        private static bool ParseDatabaseV2LogicalDependencies(List<XElement> holder, List<Dependency> logicalDependencies)
        {

            return true;
        }
        private static bool ParseDatabaseV2Categories(List<XElement> holder, List<Category> categories)
        {

            return true;
        }
        public static void CheckCRC()
        {
            //stub
        }
        //https://blogs.msdn.microsoft.com/xmlteam/2009/03/31/converting-from-xmldocument-to-xdocument/
        private static XDocument DocumentToXDocument(XmlDocument doc)
        {
            if (doc == null)
                return null;
            return XDocument.Parse(doc.OuterXml,LoadOptions.SetLineInfo);
        }
    }
}
 