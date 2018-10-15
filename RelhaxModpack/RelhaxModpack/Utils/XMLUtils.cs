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
        public static bool ParseDatabase(XmlDocument modInfoDocument, List<DatabasePackage> globalDependencies,
            List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            Logging.WriteToLog("start of ParseDatabase()", Logfiles.Application, LogLevel.Debug);
            //check all input parameters
            if (modInfoDocument == null)
                throw new BadMemeException("modInfoDocument is null dumbass");
            if (globalDependencies == null)
                globalDependencies = new List<DatabasePackage>();
            else
                globalDependencies.Clear();
            if (dependencies == null)
                dependencies = new List<Dependency>();
            else
                dependencies.Clear();
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
                    return ParseDatabaseV2(DocumentToXDocument(modInfoDocument), globalDependencies, dependencies, parsedCategoryList);
                default:
                    //parse legacy database

                    return false;
            }
        }
        public static bool ParseDatabaseV2(XDocument modInfoDocument, List<DatabasePackage> globalDependencies,
            List<Dependency> logicalDependencies, List<Category> parsedCategoryList)
        {
            //parsing the global dependencies
            bool globalParsed = ParseDatabaseV2GlobalDependencies(
                modInfoDocument.XPathSelectElements("/modInfoAlpha.xml/globaldependencies/globaldependency").ToList(), globalDependencies);
            //parsing the logical dependnecies
            bool logicalDepParsed = ParseDatabaseV2Dependencies(modInfoDocument.XPathSelectElements(
                "/modInfoAlpha.xml/logicalDependencies/logicalDependency").ToList(), logicalDependencies);
            //parsing the categories
            bool categoriesParsed = ParseDatabaseV2Categories(modInfoDocument.XPathSelectElements(
                "/modInfoAlpha.xml/catagories/catagory").ToList(), parsedCategoryList);
            return (globalParsed && logicalDepParsed && categoriesParsed) ? true : false;
        }
        private static bool ParseDatabaseV2GlobalDependencies(List<XElement> packageNodeHolder, List<DatabasePackage> globalDependencies)
        {
            //first for loop is for each "dependency" object holder
            foreach(XElement dependency in packageNodeHolder)
            {
                DatabasePackage globalDependency = new DatabasePackage();
                
            }
            return true;
        }
        private static bool ParseDatabaseV2Dependencies(List<XElement> packageNodeHolder, List<Dependency> logicalDependencies)
        {
            foreach (XElement dependency in packageNodeHolder)
            {
                

            }
            return true;
        }
        private static bool ParseDatabaseV2Categories(List<XElement> packageNodeHolder, List<Category> categories)
        {
            //TODO
            foreach (XElement dependency in packageNodeHolder)
            {
                

            }
            return true;
        }
        #region Components of packages for database parsing V2
        //for base package
        public static readonly string[] BaseRequiredNodesV2 = new string[] { "PackageName" };
        public static readonly string[] BaseOptionalNodesV2 = new string[] { "Version", "Timestamp", "ZipFile", "CRC", "StartAddress",
            "EndAddress", "AppendExtraction", "LogAtInstall", "DevURL", "ExtractionLevel", "Enabled" };//all of these have defaults
        //for dependency
        //--nothing--
        //for selectable package
        public static readonly string[] SelectablePackageRequiredNodesV2 = new string[] { "Name", "Type" };
        public static readonly string[] SelectablePackagOptionalNodesV2 = new string[] { "UserFiles", "Packages", "Medias",
            "Visible", "Size", "UpdateComment", "Description" };
        //for depdendency and selectable package
        public static readonly string[] DependencySelectableOptionalNodesV2 = new string[] { "Dependencies" };
        //for categories
        public static readonly string[] CategoryRequiredNodesV2 = new string[] { "name", "dependencies" };
        #endregion
        private static void DynamicXMLDatabasePackageParse(XElement xmlPackage, ref DatabasePackage package)
        {
            //first treat it as regular package
            //check for all fields in it
            List<string> copyOfRequiredBaseNodes = new List<string>(BaseRequiredNodesV2.ToList());
            List<string> unknownElements = new List<string>();
            //start with databasePackage
            foreach(XElement xmlPackageProperty in xmlPackage.Elements())//Properties of the package
            {
                string elementName = xmlPackageProperty.Name.LocalName;
                if (copyOfRequiredBaseNodes.Contains(elementName))
                {
                    //switch
                    switch (elementName)
                    {
                        case "PackageName":
                            package.PackageName = xmlPackageProperty.Value.Trim();
                            break;
                    }
                    copyOfRequiredBaseNodes.Remove(elementName);
                }
                else if (BaseOptionalNodesV2.Contains(elementName))
                {
                    //switch
                    switch (elementName)
                    {
                        case "PackageName":
                            package.PackageName = xmlPackageProperty.Value.Trim();
                            break;
                        case "Version":
                            package.Version = xmlPackageProperty.Value.Trim();
                            break;
                        case "Timestamp"://long
                            package.Timestamp = Utils.ParseLong(xmlPackageProperty.Value.Trim(),0);
                            break;
                        case "ZipFile":
                            package.ZipFile = xmlPackageProperty.Value.Trim();
                            break;
                        case "CRC":
                            package.CRC = xmlPackageProperty.Value.Trim();
                            break;
                        case "StartAddress":
                            package.StartAddress = xmlPackageProperty.Value.Trim();
                            break;
                        case "EndAddress":
                            package.EndAddress = xmlPackageProperty.Value.Trim();
                            break;
                        case "AppendExtraction"://bool
                            package.AppendExtraction = Utils.ParseBool(xmlPackageProperty.Value.Trim(),false);
                            break;
                        case "LogAtInstall"://bool
                            package.LogAtInstall = Utils.ParseBool(xmlPackageProperty.Value.Trim(),false);
                            break;
                        case "DevURL":
                            package.DevURL = xmlPackageProperty.Value.Trim();
                            break;
                        case "ExtractionLevel"://int
                            package.ExtractionLevel = Utils.ParseInt(xmlPackageProperty.Value.Trim(),5);
                            break;
                        case "Enabled"://bool
                            package.Enabled = Utils.ParseBool(xmlPackageProperty.Value.Trim(),false);
                            break;
                    }
                }
            }
            //check for mising required nodes of databasePackage
            if (copyOfRequiredBaseNodes.Count > 0)
            {
                foreach (string s in copyOfRequiredBaseNodes)
                {
                    //write it's missing
                    string lineNumber = "" + ((IXmlLineInfo)xmlPackage).LineNumber;
                    Logging.WriteToLog(string.Format("property name={0}, line number={1},", s, lineNumber),Logfiles.Application,LogLevel.Error);
                }
            }
            //check if it's a dependency or package
            if (package is Dependency dependency)
            {
                foreach (XElement xmlPackageProperty in xmlPackage.Elements())
                {
                    string elementName = xmlPackageProperty.Name.LocalName;
                    //{ "Dependencies" }
                    if (DependencySelectableOptionalNodesV2.Contains(elementName))
                    {
                        switch (elementName)
                        {
                            case "Dependencies":
                                ProcessDependencies(xmlPackageProperty, dependency.Dependencies);
                                break;
                        }
                    }
                }
                //no required dependency specific nodes...
            }
            else if (package is SelectablePackage selectablePackage)
            {
                List<string> copyOfRequiredSelectablePackages = new List<string>(SelectablePackageRequiredNodesV2.ToList());
                foreach (XElement xmlPackageProperty in xmlPackage.Elements())
                {
                    string elementName = xmlPackageProperty.Name.LocalName;
                    //{ "Name", "Type" }
                    if (SelectablePackageRequiredNodesV2.Contains(elementName))
                    {
                        //switch
                        switch (elementName)
                        {
                            case "name":
                                selectablePackage.Name = xmlPackageProperty.Value.Trim();
                                break;
                            case "type":
                                selectablePackage.Type = xmlPackageProperty.Value.Trim();
                                break;
                        }
                        copyOfRequiredSelectablePackages.Remove(elementName);
                    }
                    //{ "UserFiles", "Packages", "Medias", "Visible", "Size", "UpdateComment", "Description" }
                    else if (SelectablePackagOptionalNodesV2.Contains(elementName))
                    {
                        switch (elementName)
                        {
                            case "visible":
                            case "visable":
                                selectablePackage.Visible = Utils.ParseBool(xmlPackageProperty.Value.Trim(), false);
                                break;
                            case "size":
                                selectablePackage.Size = Utils.ParseLong(xmlPackageProperty.Value.Trim(), 0);
                                break;
                            case "updateComment"://TODO: need ot macro unescape text
                                selectablePackage.UpdateComment = xmlPackageProperty.Value.Trim();
                                break;
                            case "description":
                                selectablePackage.Description = xmlPackageProperty.Value.Trim();
                                break;
                            case "updateInstructions":
                                selectablePackage.UpdateInstructions = xmlPackageProperty.Value.Trim();
                                break;
                            case "userFiles":
                                ProcessUserFiles(xmlPackageProperty, selectablePackage.UserFiles);
                                break;
                            case "medias":
                                ProcessMedias(xmlPackageProperty, selectablePackage.Medias);
                                break;
                            case "packages":
                                throw new BadMemeException("THIS IS STILL TODO - RECURSIVE PACKAGE PROCESSING");
                                break;
                        }
                    }
                    //{ "Dependencies" }
                    else if (DependencySelectableOptionalNodesV2.Contains(elementName))
                    {
                        switch (elementName)
                        {
                            case "Dependencies":
                                ProcessDependencies(xmlPackageProperty, selectablePackage.Dependencies);
                                break;
                        }
                    }
                }
                //check for missing required nodes of package
                if (copyOfRequiredSelectablePackages.Count > 0)
                {
                    foreach (string s in copyOfRequiredSelectablePackages)
                    {
                        //write it's missing
                        string lineNumber = "" + ((IXmlLineInfo)xmlPackage).LineNumber;
                        Logging.WriteToLog(string.Format("property name={0}, line number={1},", s, lineNumber), Logfiles.Application, LogLevel.Error);
                    }
                }
            }
            //TODO: maybe put this in it's own method?
            //process if any extra unknown nodes are left
            List<string> allPossiblePackageNodes = new List<string>();
            //BaseRequiredNodesV2
            //BaseOptionalNodesV2
            //SelectablePackageRequiredNodesV2
            //SelectablePackagOptionalNodesV2
            //DependencySelectableOptionalNodesV2
            allPossiblePackageNodes.AddRange(BaseRequiredNodesV2);
            allPossiblePackageNodes.AddRange(BaseOptionalNodesV2);
            if (package is Dependency || package is SelectablePackage)
            {
                allPossiblePackageNodes.AddRange(DependencySelectableOptionalNodesV2);
                if(package is SelectablePackage)
                {
                    allPossiblePackageNodes.AddRange(SelectablePackageRequiredNodesV2);
                    allPossiblePackageNodes.AddRange(SelectablePackagOptionalNodesV2);
                }
            }
            foreach (XElement xmlPackageProperty in xmlPackage.Elements())
            {
                string elementName = xmlPackageProperty.Name.LocalName;
                string lineNumber = "" + ((IXmlLineInfo)xmlPackageProperty).LineNumber;
                if (!allPossiblePackageNodes.Contains(elementName))
                    unknownElements.Add(string.Format("node name={0}, line number={1}",elementName,lineNumber));
            }
            if(unknownElements.Count > 0)
            {
                foreach (string s in unknownElements)
                    Logging.WriteToLog("Parsing database error," + s, Logfiles.Application, LogLevel.Error);
            }
        }
        //{ "UserFiles", "Packages", "Medias" }//packages are different story
        //{ "Dependencies" }
        //inside packages - component required attributes
        public static readonly string[] UserfilesRequiredNodesV2 = new string[] { "Pattern" };
        private static void ProcessUserFiles(XElement userFilesHolder, List<UserFiles> userfiles)
        {
            //NOTE FOR ALL THESE: the holder is the holder, inside is each element
            foreach(XElement userfileElement in userFilesHolder.Elements())
            {
                //inside element values is the actual userdata element
                UserFiles files = new UserFiles
                {
                    Pattern = userfileElement.Value
                };
                //TODO: make into attributes?
                userfiles.Add(files);
            }
        }

        public static readonly string[] MediaRequiredNodesV2 = new string[] { "URL", "type" };
        private static void ProcessMedias(XElement mediasHolder, List<Media> medias)
        {
            foreach(XElement mediaElement in mediasHolder.Elements())
            {
                Media media = new Media();
                List<string> unknownElements = new List<string>();
                List<string> copyOfMediaRequiredNodes = new List<string>(MediaRequiredNodesV2.ToList());
                foreach(XElement mediaProperties in mediaElement.Elements())
                {
                    string elementName = mediaProperties.Name.LocalName;
                    if (MediaRequiredNodesV2.Contains(elementName))
                    {
                        switch(elementName)
                        {
                            case "URL":
                                media.URL = mediaProperties.Value.Trim();
                                break;
                            case "type":
                                media.MediaType = (MediaType) Utils.ParseInt(mediaProperties.Value.Trim(),1);
                                break;
                        }
                        copyOfMediaRequiredNodes.Remove(elementName);
                    }
                    else
                        unknownElements.Add(elementName);
                }
                if (copyOfMediaRequiredNodes.Count > 0)
                    ListNotUsedRequired(mediaElement, copyOfMediaRequiredNodes);
                if (unknownElements.Count > 0)
                    ListUnknown(mediaElement, unknownElements);
                medias.Add(media);
            }
        }
        public static readonly string[] DependnecyLogicRequiredNodesV2 = new string[] { "negateFlag", "packageName", "logicType" };
        private static void ProcessDependencies(XElement logicsHodler, List<DatabaseLogic> logics)
        {
            foreach (XElement logicsElement in logicsHodler.Elements())
            {
                DatabaseLogic logic = new DatabaseLogic();
                List<string> unknownElements = new List<string>();
                List<string> copyOfDependencyLigicRequiredNodes = new List<string>(DependnecyLogicRequiredNodesV2.ToList());
                foreach (XElement logicProperties in logicsElement.Elements())
                {
                    string elementName = logicProperties.Name.LocalName;
                    if (DependnecyLogicRequiredNodesV2.Contains(elementName))
                    {
                        switch (elementName)
                        {
                            case "negateFlag":
                                logic.NotFlag = Utils.ParseBool(logicProperties.Value.Trim(), false);
                                break;
                            case "packageName":
                                logic.PackageName = logicProperties.Value.Trim();
                                break;
                            case "logicType":
                                logic.Logic = (Logic)Utils.ParseInt(logicProperties.Value.Trim(), 0);
                                break;
                        }
                        copyOfDependencyLigicRequiredNodes.Remove(elementName);
                    }
                    else
                        unknownElements.Add(elementName);
                }
                if (copyOfDependencyLigicRequiredNodes.Count > 0)
                    ListNotUsedRequired(logicsElement, copyOfDependencyLigicRequiredNodes);
                if (unknownElements.Count > 0)
                    ListUnknown(logicsElement, unknownElements);
                logics.Add(logic);
            }
        }
        private static void ListNotUsedRequired(XElement node, List<string> missingRequriedProperties)
        {
            foreach (string s in missingRequriedProperties)
            {
                Logging.WriteToLog(string.Format("missing required xml property: name={0}, value={1}, line={2}", node.Name.LocalName, node.Value,
                    ((IXmlLineInfo)node).LineNumber), Logfiles.Application, LogLevel.Error);
            }
        }
        private static void ListUnknown(XElement node, List<string> unknownElements)
        {
            foreach (string s in unknownElements)
            {
                Logging.WriteToLog(string.Format("unknown xml property: name={0}, value={1}, line={2}", node.Name.LocalName, node.Value,
                    ((IXmlLineInfo)node).LineNumber), Logfiles.Application, LogLevel.Error);
            }
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
 