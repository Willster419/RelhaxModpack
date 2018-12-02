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
using System.Windows;

namespace RelhaxModpack
{
    /// <summary>
    /// Utility class for all XML static methods
    /// </summary>
    
    public enum XmlLoadType
    {
        FromFile,
        FromXml
    }

    public static class XMLUtils
    {
        #region Statics
        //static path for all developer selections
        public const string DeveloperSelectionsXPath = "TODO";
        #endregion

        #region XML Validating
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
        #endregion

        #region Xpath stuff
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
            //set to something dumb for temporary purposes
            XmlNode result = doc.FirstChild;
            try
            {
                result = doc.SelectSingleNode(xpath);
            }
            catch
            {
                return null;
            }
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
        #endregion

        #region Database Loading
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
            //a blank value is assumed to be pre 2.0 version of the database
            string versionString = GetXMLStringFromXPath(modInfoDocument, "//modInfoAlpha.xml/@documentVersion");
            Logging.WriteToLog(nameof(versionString) + "=" + versionString, Logfiles.Application, LogLevel.Debug);
            switch(versionString)
            {
                case "2.0":
                    return ParseDatabaseV2(DocumentToXDocument(modInfoDocument), globalDependencies, dependencies, parsedCategoryList);
                default:
                    //parse legacy database
                    List<Dependency> logicalDependencies = new List<Dependency>();
                    ParseDatabaseLegacy(DocumentToXDocument(modInfoDocument), globalDependencies, dependencies, logicalDependencies,
                        parsedCategoryList, true);
                    dependencies.AddRange(logicalDependencies);
                    return true;
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
                                selectablePackage.Size = Utils.ParseuLong(xmlPackageProperty.Value.Trim(), 0);
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
        #endregion

        #region Other XML stuffs
        //https://blogs.msdn.microsoft.com/xmlteam/2009/03/31/converting-from-xmldocument-to-xdocument/
        private static XDocument DocumentToXDocument(XmlDocument doc)
        {
            if (doc == null)
                return null;
            return XDocument.Parse(doc.OuterXml,LoadOptions.SetLineInfo);
        }

        public static XmlDocument LoadXmlDocument(string fileOrXml, XmlLoadType type)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                switch(type)
                {
                    case XmlLoadType.FromFile:
                        doc.Load(fileOrXml);
                        break;
                    case XmlLoadType.FromXml:
                        doc.LoadXml(fileOrXml);
                        break;
                }
            }
            catch (XmlException xmlEx)
            {
                if (File.Exists(fileOrXml))
                    Logging.Exception("Failed to load xml file: {0}\n{1}", Path.GetFileName(fileOrXml), xmlEx.ToString());
                else
                    Logging.Exception("failed to load xml string:\n" + xmlEx.ToString());
                return null;
            }
            return doc;
        }
        #endregion

        #region Legacy methods
        //parses the xml mod info into the memory database (change XML reader from XMLDocument to XDocument)
        // https://www.google.de/search?q=c%23+xdocument+get+line+number&oq=c%23+xdocument+get+line+number&aqs=chrome..69i57j69i58.11773j0j7&sourceid=chrome&ie=UTF-8
        public static void ParseDatabaseLegacy(XDocument doc, List<DatabasePackage> globalDependencies, List<Dependency> dependencies,
            List<Dependency> logicalDependencies, List<Category> parsedCatagoryList, bool buildRefrences)
        {
            //LEGACY CONVERSION:
            //remove all the file loading stuff
            //add the global dependencies
            foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/globaldependencies/globaldependency"))
            {
                List<string> depNodeList = new List<string>() { "zipFile", "crc", "enabled", "packageName", "appendExtraction" };
                List<string> optionalDepNodList = new List<string>() { "startAddress", "endAddress", "devURL", "timestamp", "logAtInstall" };
                List<string> unknownNodeList = new List<string>() { };
                Dependency d = new Dependency();
                foreach (XElement globs in dependencyNode.Elements())
                {
                    switch (globs.Name.ToString())
                    {
                        case "zipFile":
                            d.ZipFile = globs.Value;
                            break;
                        case "timestamp":
                            d.Timestamp = long.Parse("0" + globs.Value);
                            break;
                        case "crc":
                            d.CRC = globs.Value;
                            break;
                        case "startAddress":
                            d.StartAddress = globs.Value;
                            break;
                        case "endAddress":
                            d.EndAddress = globs.Value;
                            break;
                        case "logAtInstall":
                            d.LogAtInstall = Utils.ParseBool(globs.Value,true);
                            break;
                        case "devURL":
                            d.DevURL = globs.Value;
                            break;
                        case "enabled":
                            d.Enabled = Utils.ParseBool(globs.Value, false);
                            break;
                        case "appendExtraction":
                            d.AppendExtraction = Utils.ParseBool(globs.Value, false);
                            break;
                        case "packageName":
                            d.PackageName = globs.Value.Trim();
                            if (string.IsNullOrEmpty(d.PackageName))
                            {
                                Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => (line {1})",
                                    globs.Name.ToString(), ((IXmlLineInfo)globs).LineNumber));
                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => globsPend {1}\n\nmore informations, see logfile",
                                        globs.Name.ToString(), d.ZipFile), "Warning",MessageBoxButton.OK);
                            }
                            break;
                        default:
                            Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => (line {1})",
                                globs.Name.ToString(), ((IXmlLineInfo)globs).LineNumber));
                            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: ZipFile, CRC, StartAddress, EndAddress, enabled, PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                    globs.Name.ToString()), "Warning", MessageBoxButton.OK);
                            break;
                    }
                    //mandatory component
                    if (depNodeList.Contains(globs.Name.ToString()))
                        depNodeList.Remove(globs.Name.ToString());
                    //optional component
                    else if (optionalDepNodList.Contains(globs.Name.ToString()))
                        optionalDepNodList.Remove(globs.Name.ToString());
                    //unknown component
                    else
                        unknownNodeList.Add(globs.Name.ToString());
                }
                if (depNodeList.Count > 0)
                    Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})",
                        string.Join(",", depNodeList), d.PackageName, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (unknownNodeList.Count > 0)
                    Logging.WriteToLog(string.Format("Error: modInfo.xml unknown nodes: {0} => globsPend {1} (line {2})",
                        string.Join(",", unknownNodeList), d.PackageName, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (string.IsNullOrWhiteSpace(d.PackageName))
                {
                    throw new BadMemeException("packagename is blank!!!!!1");
                }
                globalDependencies.Add(d);
            }
            //add the dependencies
            foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/dependencies/dependency"))
            {
                List<string> depNodeList = new List<string>() { "zipFile", "crc", "enabled", "packageName", "appendExtraction" };
                List<string> optionalDepNodList = new List<string>() { "startAddress", "endAddress", "devURL", "timestamp" , "logicalDependencies", "logAtInstall" };
                List<string> unknownNodeList = new List<string>() { };
                Dependency d = new Dependency();
                foreach (XElement globs in dependencyNode.Elements())
                {
                    switch (globs.Name.ToString())
                    {
                        case "zipFile":
                            d.ZipFile = globs.Value;
                            break;
                        case "timestamp":
                            d.Timestamp = long.Parse("0" + globs.Value);
                            break;
                        case "crc":
                            d.CRC = globs.Value;
                            break;
                        case "startAddress":
                            d.StartAddress = globs.Value;
                            break;
                        case "endAddress":
                            d.EndAddress = globs.Value;
                            break;
                        case "logAtInstall":
                            d.LogAtInstall = Utils.ParseBool(globs.Value, true);
                            break;
                        case "devURL":
                            d.DevURL = globs.Value;
                            break;
                        case "enabled":
                            d.Enabled = Utils.ParseBool(globs.Value, false);
                            break;
                        case "appendExtraction":
                            d.AppendExtraction = Utils.ParseBool(globs.Value, false);
                            break;
                        case "packageName":
                            d.PackageName = globs.Value.Trim();
                            if (string.IsNullOrEmpty(d.PackageName))
                            {
                                Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => (line {1})",
                                    globs.Name.ToString(), ((IXmlLineInfo)globs).LineNumber));
                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => globsPend {1}\n\nmore informations, see logfile",
                                    globs.Name.ToString(), d.ZipFile), "Warning", MessageBoxButton.OK);
                            }
                            break;
                        case "logicalDependencies":
                            //parse all dependencies
                            foreach (XElement logDependencyHolder in globs.Elements())
                            {
                                string[] logDepNodeList = new string[] { "packageName", "negateFlag" };
                                DatabaseLogic ld = new DatabaseLogic();
                                foreach (XElement logDependencyNode in logDependencyHolder.Elements())
                                {
                                    logDepNodeList = logDepNodeList.Except(new string[] { logDependencyNode.Name.ToString() }).ToArray();
                                    switch (logDependencyNode.Name.ToString())
                                    {
                                        case "packageName":
                                            ld.PackageName = logDependencyNode.Value.Trim();
                                            if (ld.PackageName.Equals(""))
                                            {
                                                Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => (line {1})",
                                                    logDependencyNode.Name.ToString(), ((IXmlLineInfo)logDependencyNode).LineNumber));
                                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\"  => dep {1}",
                                                    logDependencyNode.Name.ToString(), d.ZipFile), "Warning", MessageBoxButton.OK);
                                            }
                                            break;
                                        case "negateFlag":
                                            ld.NotFlag = Utils.ParseBool(logDependencyNode.Value, true);
                                            break;
                                        default:
                                            Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => (line {1})",
                                                logDependencyNode.Name.ToString(), ((IXmlLineInfo)logDependencyNode).LineNumber));
                                            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                    logDependencyNode.Name.ToString()));
                                            break;
                                    }
                                }
                                if (logDepNodeList.Length > 0)
                                    Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => logDep (line {1})",
                                    string.Join(",", logDepNodeList), ((IXmlLineInfo)logDependencyHolder).LineNumber));
                                if (ld.PackageName.Equals(""))
                                {
                                    ld.PackageName = Utils.RandomString(30);
                                    Logging.WriteToLog("PackageName is random generated: " + ld.PackageName);              // to avoid exceptions
                                }
                                d.DatabasePackageLogic.Add(ld);
                            }
                            break;
                        default:
                            Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => (line {1})",
                                globs.Name.ToString(), ((IXmlLineInfo)globs).LineNumber));
                            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: ZipFile, CRC, StartAddress, EndAddress, enabled, PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                    globs.Name.ToString()), "Warning", MessageBoxButton.OK);
                            break;
                    }
                    //mandatory component
                    if (depNodeList.Contains(globs.Name.ToString()))
                        depNodeList.Remove(globs.Name.ToString());
                    //optional component
                    else if (optionalDepNodList.Contains(globs.Name.ToString()))
                        optionalDepNodList.Remove(globs.Name.ToString());
                    //unknown component
                    else
                        unknownNodeList.Add(globs.Name.ToString());
                }
                if (depNodeList.Count > 0)
                    Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => (line {1})",
                        string.Join(",", depNodeList), ((IXmlLineInfo)dependencyNode).LineNumber));
                if (unknownNodeList.Count > 0)
                    Logging.WriteToLog(string.Format("Error: modInfo.xml unknown nodes: {0} => globsPend {1} (line {2})",
                        string.Join(",", unknownNodeList), d.PackageName, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (string.IsNullOrWhiteSpace(d.PackageName))
                {
                    throw new BadMemeException("packagename is blank!!!!!1");
                }
                dependencies.Add(d);
            }
            //add the logicalDependencies
            foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/logicalDependencies/logicalDependency"))
            {
                List<string> depNodeList = new List<string>() { "zipFile", "crc", "enabled", "packageName", "logic" };
                List<string> optionalDepNodList = new List<string>() { "startAddress", "endAddress", "devURL", "timestamp", "logAtInstall" };
                List<string> unknownNodeList = new List<string>() { };
                Dependency d = new Dependency();
                foreach (XElement globs in dependencyNode.Elements())
                {
                    switch (globs.Name.ToString())
                    {
                        case "zipFile":
                            d.ZipFile = globs.Value;
                            break;
                        case "timestamp":
                            d.Timestamp = long.Parse("0" + globs.Value);
                            break;
                        case "crc":
                            d.CRC = globs.Value;
                            break;
                        case "startAddress":
                            d.StartAddress = globs.Value;
                            break;
                        case "endAddress":
                            d.EndAddress = globs.Value;
                            break;
                        case "logAtInstall":
                            d.LogAtInstall = Utils.ParseBool(globs.Value, true);
                            break;
                        case "devURL":
                            d.DevURL = globs.Value;
                            break;
                        case "enabled":
                            d.Enabled = Utils.ParseBool(globs.Value, false);
                            break;
                        case "packageName":
                            d.PackageName = globs.Value.Trim();
                            if (d.PackageName.Equals(""))
                            {
                                Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => (line {1})",
                                    globs.Name.ToString(), ((IXmlLineInfo)globs).LineNumber));
                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => logDep {1}\n\nmore informations, see logfile",
                                        globs.Name.ToString(), d.ZipFile), "Warning", MessageBoxButton.OK);
                            }
                            break;
                        default:
                            Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => (line {1})",
                                globs.Name.ToString(), ((IXmlLineInfo)globs).LineNumber));
                            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: ZipFile, CRC, StartAddress, EndAddress, enabled, PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                    globs.Name.ToString()), "Warning", MessageBoxButton.OK);
                            break;
                    }
                    //mandatory component
                    if (depNodeList.Contains(globs.Name.ToString()))
                        depNodeList.Remove(globs.Name.ToString());
                    //optional component
                    else if (optionalDepNodList.Contains(globs.Name.ToString()))
                        optionalDepNodList.Remove(globs.Name.ToString());
                    //unknown component
                    else
                        unknownNodeList.Add(globs.Name.ToString());
                }
                if (depNodeList.Count > 0)
                    Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => (line {1})",
                        string.Join(",", depNodeList), ((IXmlLineInfo)dependencyNode).LineNumber));
                if (unknownNodeList.Count > 0)
                    Logging.WriteToLog(string.Format("Error: modInfo.xml unknown nodes: {0} => globsPend {1} (line {2})",
                        string.Join(",", unknownNodeList), d.PackageName, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (string.IsNullOrWhiteSpace(d.PackageName))
                {
                    throw new BadMemeException("packagename is blank!!!!!1");
                }
                logicalDependencies.Add(d);
            }
            foreach (XElement catagoryHolder in doc.XPathSelectElements("/modInfoAlpha.xml/catagories/catagory"))
            {
                Category cat = new Category();
                string[] catNodeList = new string[] { "name", "installGroup", "packages", "dependencies" };
                foreach (XElement catagoryNode in catagoryHolder.Elements())
                {
                    catNodeList = catNodeList.Except(new string[] { catagoryNode.Name.ToString() }).ToArray();
                    switch (catagoryNode.Name.ToString())
                    {
                        case "name":
                            cat.Name = catagoryNode.Value;
                            break;
                        case "packages":
                            foreach (XElement modHolder in catagoryNode.Elements())
                            {
                                switch (modHolder.Name.ToString())
                                {
                                    case "package":
                                        List<string> packageNodeList = new List<string>() { "name", "zipFile", "crc", "enabled", "visible", "packageName", "type" };
                                        List<string> optionalPackageNodeList = new List<string>() { "version", "timestamp", "startAddress", "endAddress", "size",
                                            "description", "updateComment", "devURL", "userDatas", "medias", "dependencies", "logicalDependencies", "packages", "logAtInstall" };
                                        List<string> unknownNodeList = new List<string>() { };
                                        SelectablePackage m = new SelectablePackage()
                                        {
                                            Level = 0
                                        };
                                        foreach (XElement modNode in modHolder.Elements())
                                        {
                                            switch (modNode.Name.ToString())
                                            {
                                                case "name":
                                                    m.Name = modNode.Value;
                                                    break;
                                                case "version":
                                                    m.Version = modNode.Value;
                                                    break;
                                                case "zipFile":
                                                    m.ZipFile = modNode.Value;
                                                    break;
                                                case "timestamp":
                                                    m.Timestamp = long.Parse("0" + modNode.Value);
                                                    break;
                                                case "startAddress":
                                                    m.StartAddress = modNode.Value;
                                                    break;
                                                case "endAddress":
                                                    m.EndAddress = modNode.Value;
                                                    break;
                                                case "logAtInstall":
                                                    m.LogAtInstall = Utils.ParseBool(modNode.Value, true);
                                                    break;
                                                case "crc":
                                                    m.CRC = modNode.Value;
                                                    break;
                                                case "type":
                                                    m.Type = modNode.Value;
                                                    break;
                                                case "enabled":
                                                    m.Enabled = Utils.ParseBool(modNode.Value, false);
                                                    break;
                                                case "visible":
                                                    m.Visible = Utils.ParseBool(modNode.Value, true);
                                                    break;
                                                case "packageName":
                                                    m.PackageName = modNode.Value.Trim();
                                                    if (m.PackageName.Equals(""))
                                                    {
                                                        Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => mod {1} ({2}) (line {3})",
                                                            modNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)modNode).LineNumber));
                                                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => mod {1} ({2})",
                                                                modNode.Name.ToString(), m.Name, m.ZipFile), "Warning", MessageBoxButton.OK);
                                                    }
                                                    break;
                                                case "size":
                                                    m.Size = Utils.ParseuLong(modNode.Value, 0);
                                                    break;
                                                case "description":
                                                    m.Description = ConvertFromXmlSaveFormat(modNode.Value);
                                                    break;
                                                case "updateComment":
                                                    m.UpdateComment = ConvertFromXmlSaveFormat(modNode.Value);
                                                    break;
                                                case "devURL":
                                                    m.DevURL = modNode.Value;
                                                    break;
                                                case "userDatas":
                                                    foreach (XElement userDataNode in modNode.Elements())
                                                    {
                                                        switch (userDataNode.Name.ToString())
                                                        {
                                                            case "userData":
                                                                string innerText = userDataNode.Value;
                                                                if (innerText == null)
                                                                    continue;
                                                                if (innerText.Equals(""))
                                                                    continue;
                                                                UserFiles uf = new UserFiles();
                                                                uf.Pattern = innerText;
                                                                if (userDataNode.Attribute("before") != null)
                                                                    uf.placeBeforeExtraction = Utils.ParseBool(userDataNode.Attribute("before").Value, false);
                                                                if (userDataNode.Attribute("pre") != null)
                                                                    uf.placeBeforeExtraction = Utils.ParseBool(userDataNode.Attribute("pre").Value, false);
                                                                if (userDataNode.Attribute("system") != null)
                                                                    uf.systemInitiated = Utils.ParseBool(userDataNode.Attribute("system").Value, false);
                                                                m.UserFiles.Add(uf);
                                                                break;
                                                            default:
                                                                Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => userDatas => expected node: userData (line {3})",
                                                                    userDataNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)userDataNode).LineNumber));
                                                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                                    MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: userData\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                        userDataNode.Name.ToString()), "Warning", MessageBoxButton.OK);
                                                                break;
                                                        }
                                                    }
                                                    break;
                                                case "medias":
                                                    //parse every picture
                                                    foreach (XElement pictureHolder in modNode.Elements())
                                                    {
                                                        Media med = new Media();
                                                        switch (pictureHolder.Name.ToString())
                                                        {
                                                            case "media":
                                                                foreach (XElement pictureNode in pictureHolder.Elements())
                                                                {
                                                                    switch (pictureNode.Name.ToString())
                                                                    {
                                                                        case "URL":
                                                                            string innerText = pictureNode.Value;
                                                                            if (innerText == null)
                                                                                continue;
                                                                            if (innerText.Equals(""))
                                                                                continue;
                                                                            med.URL = innerText;
                                                                            break;
                                                                        case "type":
                                                                            int innerValue = Utils.ParseInt(pictureNode.Value, 1);
                                                                            switch (innerValue)
                                                                            {
                                                                                case 1:
                                                                            med.MediaType = MediaType.Picture;
                                                                            break;
                                                                        case 2:
                                                                            med.MediaType = MediaType.Webpage;
                                                                            break;
                                                                        case 3:
                                                                            med.MediaType = MediaType.MediaFile;
                                                                            break;
                                                                        case 4:
                                                                            med.MediaType = MediaType.HTML;
                                                                            break;
                                                                            }
                                                                            break;
                                                                        default:
                                                                            Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => picture =>expected nodes: URL (line {3})",
                                                                                pictureNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)pictureNode).LineNumber));
                                                                            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: URL\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                                    pictureNode.Name.ToString()), "Warning", MessageBoxButton.OK);
                                                                            break;
                                                                    }
                                                                }
                                                                break;
                                                            default:
                                                                Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => expected node: picture (line {3})",
                                                                    pictureHolder.Name, m.Name, m.ZipFile, ((IXmlLineInfo)pictureHolder).LineNumber));
                                                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                                    MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: picture\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                        pictureHolder.Name.ToString()), "Warning", MessageBoxButton.OK);
                                                                break;
                                                        }
                                                        m.Medias.Add(med);
                                                    }
                                                    break;
                                                case "dependencies":
                                                    //parse all dependencies
                                                    foreach (XElement dependencyHolder in modNode.Elements())
                                                    {
                                                        string[] depNodeList = new string[] { "packageName" };
                                                        DatabaseLogic d = new DatabaseLogic() { Logic = Logic.OR };
                                                        foreach (XElement dependencyNode in dependencyHolder.Elements())
                                                        {
                                                            depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                            switch (dependencyNode.Name.ToString())
                                                            {
                                                                case "packageName":
                                                                    d.PackageName = dependencyNode.Value.Trim();
                                                                    if (d.PackageName.Equals(""))
                                                                    {
                                                                        Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => mod {1} ({2}) => (line {3})",
                                                                            dependencyNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                                        {
                                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => mod {1} ({2})",
                                                                                dependencyNode.Name.ToString(), m.Name, m.ZipFile), "Warning", MessageBoxButton.OK);
                                                                        }
                                                                    }
                                                                    break;
                                                                default:
                                                                    Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => (line {3})",
                                                                        dependencyNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                    if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                            dependencyNode.Name.ToString()), "Warning", MessageBoxButton.OK);
                                                                    break;
                                                            }
                                                        }
                                                        if (depNodeList.Length > 0)
                                                        {
                                                            Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} ({2}) => (line {3})",
                                                                string.Join(",", depNodeList), m.Name, m.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                                        }
                                                        if (string.IsNullOrWhiteSpace(d.PackageName))
                                                        {
                                                            throw new BadMemeException("packagename is blank!!!!!1");
                                                        }
                                                        m.Dependencies.Add(d);
                                                    }
                                                    break;
                                                case "logicalDependencies":
                                                    //parse all dependencies
                                                    foreach (XElement dependencyHolder in modNode.Elements())
                                                    {
                                                        string[] depNodeList = new string[] { "packageName", "negateFlag" };
                                                        DatabaseLogic d = new DatabaseLogic() { Logic = Logic.AND };
                                                        foreach (XElement dependencyNode in dependencyHolder.Elements())
                                                        {
                                                            depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                            switch (dependencyNode.Name.ToString())
                                                            {
                                                                case "packageName":
                                                                    d.PackageName = dependencyNode.Value.Trim();
                                                                    if (d.PackageName.Equals(""))
                                                                    {
                                                                        Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => config {1} ({2}) => (line {3})",
                                                                            dependencyNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => config {1} ({2})",
                                                                                dependencyNode.Name.ToString(), m.Name, m.ZipFile), "Warning", MessageBoxButton.OK);
                                                                    }
                                                                    break;
                                                                case "negateFlag":
                                                                    //d.NegateFlag = Utils.ParseBool(dependencyNode.Value, true);
                                                                    break;
                                                                default:
                                                                    Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => (line {3})",
                                                                        dependencyNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                    if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                                    {
                                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                            dependencyNode.Name.ToString()));
                                                                    };
                                                                    break;
                                                            }
                                                        }
                                                        if (depNodeList.Length > 0)
                                                        {
                                                            Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => (line {3})",
                                                                string.Join(",", depNodeList), m.Name, m.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                                        }
                                                        if (string.IsNullOrWhiteSpace(d.PackageName))
                                                        {
                                                            throw new BadMemeException("packagename is blank!!!!!1");
                                                        }
                                                        m.Dependencies.Add(d);
                                                    }
                                                    break;
                                                case "packages":
                                                    //run the process configs method
                                                    ProcessConfigs(modNode, m, true,m.Level+1);
                                                    break;
                                                default:
                                                    Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) (line {3})",
                                                        modNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)modNode).LineNumber));
                                                    if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, version, ZipFile," +
                                                            "StartAddress, EndAddress, CRC, Enabled, PackageName, size, description, updateComment, devURL, userDatas," +
                                                            " pictures, dependencies, configs\n\nNode found: {0}\n\nmore informations, see logfile",
                                                            modNode.Name.ToString()), "Warning", MessageBoxButton.OK);
                                                    break;
                                            }
                                            //mandatory component
                                            if (packageNodeList.Contains(modNode.Name.ToString()))
                                                packageNodeList.Remove(modNode.Name.ToString());
                                            //optional component
                                            else if (optionalPackageNodeList.Contains(modNode.Name.ToString()))
                                                optionalPackageNodeList.Remove(modNode.Name.ToString());
                                            //unknown component
                                            else
                                                unknownNodeList.Add(modNode.Name.ToString());
                                        }
                                        if (packageNodeList.Count > 0)
                                        Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => package {1} (line {2})",
                                            string.Join(",", packageNodeList), m.Name, ((IXmlLineInfo)modHolder).LineNumber));
                                        if (unknownNodeList.Count > 0)
                                            Logging.WriteToLog(string.Format("Error: modInfo.xml unknown nodes: {0} => package {1} (line {2})",
                                                string.Join(",", unknownNodeList), m.PackageName, ((IXmlLineInfo)modHolder).LineNumber));
                                        if (string.IsNullOrWhiteSpace(m.PackageName))
                                        {
                                            throw new BadMemeException("packagename is blank!!!!!1");
                                        }
                                        cat.Packages.Add(m);
                                        break;
                                    default:
                                        Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} (line {2})",
                                            modHolder.Name.ToString(), cat.Name, ((IXmlLineInfo)modHolder).LineNumber));
                                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                            MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: mod\n\nNode found: {0}\n\nmore informations, see logfile",
                                                modHolder.Name.ToString()), "Warning", MessageBoxButton.OK);
                                        break;
                                }
                            }
                            break;
                        case "dependencies":
                            //parse every dependency for that mod
                            foreach (XElement dependencyHolder in catagoryNode.Elements())
                            {
                                string[] depNodeList = new string[] { "packageName" };
                                DatabaseLogic d = new DatabaseLogic() { Logic = Logic.OR };
                                foreach (XElement dependencyNode in dependencyHolder.Elements())
                                {
                                    depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                    switch (dependencyNode.Name.ToString())
                                    {
                                        case "packageName":
                                            d.PackageName = dependencyNode.Value.Trim();
                                            if (d.PackageName.Equals(""))
                                            {
                                                Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => cat {1} => (line {2})",
                                                    dependencyNode.Name.ToString(), cat.Name, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                {
                                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => cat {1}",
                                                        dependencyNode.Name.ToString(), cat.Name), "Warning", MessageBoxButton.OK);
                                                }
                                            }
                                            break;
                                        default:
                                            Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} => (line {2})",
                                                dependencyNode.Name, cat.Name, ((IXmlLineInfo)dependencyNode).LineNumber));
                                            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                            {
                                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                    dependencyNode.Name), "Warning", MessageBoxButton.OK);
                                            }
                                            break;
                                    }
                                }
                                if (depNodeList.Length > 0)
                                {
                                    Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => cat {1} => (line {2})",
                                        string.Join(",", depNodeList), cat.Name, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                };
                                if (string.IsNullOrWhiteSpace(d.PackageName))
                                {
                                    throw new BadMemeException("packagename is blank!!!!!1");
                                }
                                cat.Dependencies.Add(d);
                            }
                            break;
                        default:
                            Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} (line {2})",
                                catagoryNode.Name.ToString(), cat.Name, ((IXmlLineInfo)catagoryNode).LineNumber));
                            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                            {
                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, selectionType, mods, dependencies\n\nNode found: {0}\n\nmore informations, see logfile",
                                    catagoryNode.Name.ToString()), "Warning", MessageBoxButton.OK);
                            }
                            break;
                    }
                }
                if (catNodeList.Length > 0)
                {
                    Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => cat {1} (line {2})",
                        string.Join(",", catNodeList), cat.Name, ((IXmlLineInfo)catagoryHolder).LineNumber));
                }
                parsedCatagoryList.Add(cat);
            }
        }
        //recursivly processes the configs
        public static void ProcessConfigs(XElement holder, SelectablePackage m, bool parentIsMod, int level, SelectablePackage con = null)
        {
            //parse every config for that mod
            foreach (XElement configHolder in holder.Elements())
            {
                switch (configHolder.Name.ToString())
                {
                    case "package":
                        List<string> packageNodeList = new List<string>() { "name", "zipFile", "crc", "enabled", "visible", "packageName", "type" };
                        List<string> optionalPackageNodeList = new List<string>() { "version", "timestamp", "startAddress", "endAddress", "size",
                            "description", "updateComment", "devURL", "userDatas", "medias", "dependencies", "logicalDependencies", "packages", "logAtInstall" };
                        List<string> unknownNodeList = new List<string>() { };
                        SelectablePackage c = new SelectablePackage()
                        {
                            Level = level
                        };
                        foreach (XElement configNode in configHolder.Elements())
                        {
                            switch (configNode.Name.ToString())
                            {
                                case "name":
                                    c.Name = configNode.Value;
                                    break;
                                case "version":
                                    c.Version = configNode.Value;
                                    break;
                                case "zipFile":
                                    c.ZipFile = configNode.Value;
                                    break;
                                case "timestamp":
                                    c.Timestamp = long.Parse("0" + configNode.Value);
                                    break;
                                case "startAddress":
                                    c.StartAddress = configNode.Value;
                                    break;
                                case "endAddress":
                                    c.EndAddress = configNode.Value;
                                    break;
                                case "logAtInstall":
                                    c.LogAtInstall = Utils.ParseBool(configNode.Value, true);
                                    break;
                                case "crc":
                                    c.CRC = configNode.Value;
                                    break;
                                case "enabled":
                                    c.Enabled = Utils.ParseBool(configNode.Value, false);
                                    break;
                                case "visible":
                                    c.Visible = Utils.ParseBool(configNode.Value, true);
                                    break;
                                case "packageName":
                                    c.PackageName = configNode.Value.Trim();
                                    if (c.PackageName.Equals(""))
                                    {
                                        Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => config {1} ({2}) (line {3})",
                                            configNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)configNode).LineNumber));
                                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => config {1} ({2})",
                                                configNode.Name.ToString(), c.Name, c.ZipFile), "Warning", MessageBoxButton.OK);
                                    }
                                    break;
                                case "size":
                                    c.Size = Utils.ParseuLong(configNode.Value, 0);
                                    break;
                                case "updateComment":
                                    c.UpdateComment = ConvertFromXmlSaveFormat(configNode.Value);
                                    break;
                                case "description":
                                    c.Description = ConvertFromXmlSaveFormat(configNode.Value);
                                    break;
                                case "devURL":
                                    c.DevURL = configNode.Value;
                                    break;
                                case "type":
                                    c.Type = configNode.Value;
                                    break;
                                case "packages":
                                    ProcessConfigs(configNode, m, false, c.Level+1 ,c);
                                    break;
                                case "userDatas":
                                    foreach (XElement userDataNode in configNode.Elements())
                                    {
                                        switch (userDataNode.Name.ToString())
                                        {
                                            case "userData":
                                                string innerText = userDataNode.Value;
                                                if (innerText == null)
                                                    continue;
                                                if (innerText.Equals(""))
                                                    continue;
                                                UserFiles uf = new UserFiles();
                                                uf.Pattern = innerText;
                                                if (userDataNode.Attribute("before") != null)
                                                    uf.placeBeforeExtraction = Utils.ParseBool(userDataNode.Attribute("before").Value, false);
                                                if (userDataNode.Attribute("pre") != null)
                                                    uf.placeBeforeExtraction = Utils.ParseBool(userDataNode.Attribute("pre").Value, false);
                                                if (userDataNode.Attribute("system") != null)
                                                    uf.systemInitiated = Utils.ParseBool(userDataNode.Attribute("system").Value, false);
                                                c.UserFiles.Add(uf);
                                                break;
                                            default:
                                                Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => userDatas => expected nodes: userData (line {3})",
                                                    userDataNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)configNode).LineNumber));
                                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                    MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: userData\n\nNode found: {0}\n\nmore informations, see logfile",
                                                        userDataNode.Name.ToString()));
                                                break;
                                        }
                                    }
                                    break;
                                case "medias":
                                    //parse every picture
                                    foreach (XElement pictureHolder in configNode.Elements())
                                    {
                                        Media med = new Media();
                                        switch (pictureHolder.Name.ToString())
                                        {
                                            case "media":
                                                foreach (XElement pictureNode in pictureHolder.Elements())
                                                {
                                                    switch (pictureNode.Name.ToString())
                                                    {
                                                        case "URL":
                                                            string innerText = pictureNode.Value;
                                                            if (innerText == null)
                                                                continue;
                                                            if (innerText.Equals(""))
                                                                continue;
                                                            med.URL = innerText;
                                                            break;
                                                        case "type":
                                                            switch (Utils.ParseInt(pictureNode.Value, 1))
                                                            {
                                                                case 1:
                                                                    med.MediaType = MediaType.Picture;
                                                                    break;
                                                                case 2:
                                                                    med.MediaType = MediaType.Webpage;
                                                                    break;
                                                                case 3:
                                                                    med.MediaType = MediaType.MediaFile;
                                                                    break;
                                                                case 4:
                                                                    med.MediaType = MediaType.HTML;
                                                                    break;
                                                            }
                                                            break;
                                                        default:
                                                            Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => picture =>expected nodes: URL (line {3})",
                                                                pictureNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)pictureNode).LineNumber));
                                                            if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: URL\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                    pictureNode.Name.ToString()), "Warning", MessageBoxButton.OK);
                                                            break;
                                                    }
                                                }
                                                break;
                                            default:
                                                Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => expected node: picture (line {3})",
                                                    pictureHolder.Name, m.Name, m.ZipFile, ((IXmlLineInfo)pictureHolder).LineNumber));
                                                if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                    MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: picture\n\nNode found: {0}\n\nmore informations, see logfile",
                                                        pictureHolder.Name.ToString()), "Warning", MessageBoxButton.OK);
                                                break;
                                        }
                                        c.Medias.Add(med);
                                    }
                                    break;
                                case "dependencies":
                                    //parse all dependencies
                                    foreach (XElement dependencyHolder in configNode.Elements())
                                    {
                                        string[] depNodeList = new string[] { "packageName" };
                                        DatabaseLogic d = new DatabaseLogic() { Logic = Logic.OR };
                                        foreach (XElement dependencyNode in dependencyHolder.Elements())
                                        {
                                            depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                            switch (dependencyNode.Name.ToString())
                                            {
                                                case "packageName":
                                                    d.PackageName = dependencyNode.Value.Trim();
                                                    if (d.PackageName.Equals(""))
                                                    {
                                                        Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => config {1} ({2}) => (line {3})",
                                                            dependencyNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => config {1} ({2})",
                                                                dependencyNode.Name.ToString(), c.Name, c.ZipFile), "Warning", MessageBoxButton.OK);
                                                    }
                                                    break;
                                                default:
                                                    Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => (line {3})",
                                                        dependencyNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                    if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                            dependencyNode.Name.ToString()));
                                                    break;
                                            }
                                        }
                                        if (depNodeList.Length > 0)
                                            Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => (line {3})",
                                                string.Join(",", depNodeList), c.Name, c.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                        if (d.PackageName.Equals(""))
                                        {
                                            d.PackageName = Utils.RandomString(30);
                                            Logging.WriteToLog("PackageName is random generated: " + d.PackageName);
                                        }              // to avoid exceptions
                                        c.Dependencies.Add(d);
                                    }
                                    break;
                                case "logicalDependencies":
                                    //parse all dependencies
                                    foreach (XElement dependencyHolder in configNode.Elements())
                                    {
                                        string[] depNodeList = new string[] { "packageName", "negateFlag" };
                                        DatabaseLogic d = new DatabaseLogic() { Logic = Logic.AND };
                                        foreach (XElement dependencyNode in dependencyHolder.Elements())
                                        {
                                            depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                            switch (dependencyNode.Name.ToString())
                                            {
                                                case "packageName":
                                                    d.PackageName = dependencyNode.Value.Trim();
                                                    if (d.PackageName.Equals(""))
                                                    {
                                                        Logging.WriteToLog(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => config {1} ({2}) => (line {3})",
                                                            dependencyNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => config {1} ({2})",
                                                                dependencyNode.Name.ToString(), c.Name, c.ZipFile), "Warning", MessageBoxButton.OK);
                                                    }
                                                    break;
                                                case "negateFlag":
                                                    //d.NegateFlag = Utils.ParseBool(dependencyNode.Value, true);
                                                    break;
                                                default:
                                                    Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => (line {3})",
                                                        dependencyNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                    if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                            dependencyNode.Name.ToString()));
                                                    break;
                                            }
                                        }
                                        if (depNodeList.Length > 0)
                                            Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => (line {3})",
                                                string.Join(",", depNodeList), c.Name, c.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                        if (d.PackageName.Equals(""))
                                        {
                                            d.PackageName = Utils.RandomString(30);
                                            Logging.WriteToLog("PackageName is random generated: " + d.PackageName);
                                        }              // to avoid exceptions
                                        c.Dependencies.Add(d);
                                    }
                                    break;
                                default:
                                    Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) (line {3})",
                                        configNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)configNode).LineNumber));
                                    if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, version, ZipFile, StartAddress, " +
                                            "EndAddress, CRC, Enabled, PackageName, size, updateComment, description, devURL, type, configs, userDatas, pictures, " +
                                            "dependencies\n\nNode found: {0}\n\nmore informations, see logfile",
                                            configNode.Name.ToString()));
                                    break;
                            }
                            //mandatory component
                            if (packageNodeList.Contains(configNode.Name.ToString()))
                                packageNodeList.Remove(configNode.Name.ToString());
                            //optional component
                            else if (optionalPackageNodeList.Contains(configNode.Name.ToString()))
                                optionalPackageNodeList.Remove(configNode.Name.ToString());
                            //unknown component
                            else
                                unknownNodeList.Add(configNode.Name.ToString());
                        }
                        if (packageNodeList.Count > 0)
                            Logging.WriteToLog(string.Format("Error: modInfo.xml nodes not used: {0} => package {1} (line {2})",
                                string.Join(",", packageNodeList), m.Name, ((IXmlLineInfo)configHolder).LineNumber));
                        if (unknownNodeList.Count > 0)
                            Logging.WriteToLog(string.Format("Error: modInfo.xml unknown nodes: {0} => package {1} (line {2})",
                                string.Join(",", unknownNodeList), m.PackageName, ((IXmlLineInfo)configHolder).LineNumber));
                        if (string.IsNullOrWhiteSpace(c.PackageName))
                        {
                            throw new BadMemeException("packagename is blank!!!!!1");
                        }
                        //attach it to eithor the config of correct level or the mod
                        if (parentIsMod)
                            m.Packages.Add(c);
                        else
                            con.Packages.Add(c);
                        break;
                    default:
                        Logging.WriteToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => expected nodes: config (line {3})",
                            configHolder.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)configHolder).LineNumber));
                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                            MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: config\n\nNode found: {0}\n\nmore informations, see logfile",
                                configHolder.Name));
                        break;
                }
            }
        }
        
        //convert string with CR and/or LF from Xml save format
        private static string ConvertFromXmlSaveFormat(string s)
        {
            return s.TrimEnd().Replace("@", "\n").Replace(@"\r", "\r").Replace(@"\t", "\t").Replace(@"\n", "\n").Replace(@"&#92;", @"\");
        }
        
        //convert string with CR and/or LF to Xml save format
        private static string ConvertToXmlSaveFormat(string s)
        {
            return s.TrimEnd().Replace(@"\", @"&#92;").Replace("\r", @"\r").Replace("\t", @"\t").Replace("\n", @"\n");
        }
        //saves the mod database
        public static void SaveDatabaseLegacy(string saveLocation, string gameVersion, string onlineFolderVersion, List<Dependency> globalDependencies, List<Dependency> dependencies, List<Dependency> logicalDependencies, List<Category> parsedCatagoryList)
        {
            XmlDocument doc = new XmlDocument();
            //database root modInfo.xml
            XmlElement root = doc.CreateElement("modInfoAlpha.xml");
            root.SetAttribute("version", gameVersion);
            root.SetAttribute("onlineFolder", onlineFolderVersion);
            doc.AppendChild(root);
            //global dependencies
            XmlElement globalDependenciesXml = doc.CreateElement("globaldependencies");
            foreach (Dependency d in globalDependencies)
            {
                //declare dependency root
                XmlElement globalDependencyRoot = doc.CreateElement("globaldependency");
                //make dependency elements
                //mandatory
                XmlElement globalDepZipFile = doc.CreateElement("zipFile");
                if(!string.IsNullOrWhiteSpace(d.ZipFile))
                    globalDepZipFile.InnerText = d.ZipFile.Trim();
                globalDependencyRoot.AppendChild(globalDepZipFile);

                XmlElement globalDepCRC = doc.CreateElement("crc");
                if (!string.IsNullOrWhiteSpace(d.CRC))
                    globalDepCRC.InnerText = d.CRC.Trim();
                globalDependencyRoot.AppendChild(globalDepCRC);

                XmlElement globalDepEnabled = doc.CreateElement("enabled");
                globalDepEnabled.InnerText = "" + d.Enabled;
                globalDependencyRoot.AppendChild(globalDepEnabled);

                XmlElement globalDepAppendExtraction = doc.CreateElement("appendExtraction");
                globalDepAppendExtraction.InnerText = "" + d.AppendExtraction;
                globalDependencyRoot.AppendChild(globalDepAppendExtraction);

                XmlElement globalDepPackageName = doc.CreateElement("packageName");
                if (!string.IsNullOrWhiteSpace(d.PackageName))
                    globalDepPackageName.InnerText = d.PackageName.Trim();
                globalDependencyRoot.AppendChild(globalDepPackageName);

                //optional elements
                if (d.Timestamp > 0)
                {
                    XmlElement globalDepTimestamp = doc.CreateElement("timestamp");
                    globalDepTimestamp.InnerText = d.Timestamp.ToString();
                    globalDependencyRoot.AppendChild(globalDepTimestamp);
                }
                if (!d.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(d.StartAddress))
                {
                    XmlElement globalDepStartAddress = doc.CreateElement("startAddress");
                    globalDepStartAddress.InnerText = d.StartAddress.Trim();
                    globalDependencyRoot.AppendChild(globalDepStartAddress);
                }
                if (!d.EndAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(d.EndAddress))
                {
                    XmlElement globalDepEndAddress = doc.CreateElement("endAddress");
                    globalDepEndAddress.InnerText = d.EndAddress.Trim();
                    globalDependencyRoot.AppendChild(globalDepEndAddress);
                }
                if (!string.IsNullOrWhiteSpace(d.DevURL))
                {
                    XmlElement globalDepURL = doc.CreateElement("devURL");
                    globalDepURL.InnerText = d.DevURL.Trim();
                    globalDependencyRoot.AppendChild(globalDepURL);
                }
                if (!d.LogAtInstall)
                {
                    XmlElement globalDepLogAtInstall = doc.CreateElement("logAtInstall");
                    globalDepLogAtInstall.InnerText = d.LogAtInstall.ToString();
                    globalDependencyRoot.AppendChild(globalDepLogAtInstall);
                }
                //attach dependency root
                globalDependenciesXml.AppendChild(globalDependencyRoot);
            }
            root.AppendChild(globalDependenciesXml);

            //dependencies
            XmlElement DependenciesXml = doc.CreateElement("dependencies");
            foreach (Dependency d in dependencies)
            {
                //declare dependency root
                XmlElement dependencyRoot = doc.CreateElement("dependency");
                //make dependency
                //mandatory
                XmlElement depZipFile = doc.CreateElement("zipFile");
                if (!string.IsNullOrWhiteSpace(d.ZipFile))
                    depZipFile.InnerText = d.ZipFile.Trim();
                dependencyRoot.AppendChild(depZipFile);

                XmlElement depCRC = doc.CreateElement("crc");
                if (!string.IsNullOrWhiteSpace(d.CRC))
                    depCRC.InnerText = d.CRC.Trim();
                dependencyRoot.AppendChild(depCRC);

                XmlElement depEnabled = doc.CreateElement("enabled");
                depEnabled.InnerText = "" + d.Enabled;
                dependencyRoot.AppendChild(depEnabled);

                XmlElement depAppendExtraction = doc.CreateElement("appendExtraction");
                depAppendExtraction.InnerText = "" + d.AppendExtraction;
                dependencyRoot.AppendChild(depAppendExtraction);

                XmlElement depPackageName = doc.CreateElement("packageName");
                if (!string.IsNullOrWhiteSpace(d.PackageName))
                    depPackageName.InnerText = d.PackageName.Trim();
                dependencyRoot.AppendChild(depPackageName);
                //optional
                if(d.Timestamp > 0)
                {
                    XmlElement depTimestamp = doc.CreateElement("timestamp");
                    depTimestamp.InnerText = d.Timestamp.ToString();
                    dependencyRoot.AppendChild(depTimestamp);
                }
                if(!d.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(d.StartAddress))
                {
                    XmlElement depStartAddress = doc.CreateElement("startAddress");
                    depStartAddress.InnerText = d.StartAddress.Trim();
                    dependencyRoot.AppendChild(depStartAddress);
                }
                if(!d.EndAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(d.EndAddress))
                {
                    XmlElement depEndAddress = doc.CreateElement("endAddress");
                    depEndAddress.InnerText = d.EndAddress.Trim();
                    dependencyRoot.AppendChild(depEndAddress);
                }
                if(!string.IsNullOrWhiteSpace(d.DevURL.Trim()))
                {
                    XmlElement depdevURL = doc.CreateElement("devURL");
                    depdevURL.InnerText = d.DevURL.Trim();
                    dependencyRoot.AppendChild(depdevURL);
                }
                if (!d.LogAtInstall)
                {
                    XmlElement depLogAtInstall = doc.CreateElement("logAtInstall");
                    depLogAtInstall.InnerText = d.LogAtInstall.ToString();
                    dependencyRoot.AppendChild(depLogAtInstall);
                }

                //logicalDependencies for the configs
                if (d.Dependencies.Count > 0)
                {
                    XmlElement depLogicalDependencies = doc.CreateElement("logicalDependencies");
                    foreach (DatabaseLogic ld in d.Dependencies)
                    {
                        //declare logicalDependency root
                        XmlElement DependencyRoot = doc.CreateElement("logicalDependency");
                        //make logicalDependency
                        XmlElement DependencyPackageName = doc.CreateElement("packageName");
                        DependencyPackageName.InnerText = ld.PackageName.Trim();
                        DependencyRoot.AppendChild(DependencyPackageName);
                        XmlElement DependencyNegateFlag = doc.CreateElement("negateFlag");
                        DependencyNegateFlag.InnerText = "" + ld.NotFlag;
                        DependencyRoot.AppendChild(DependencyNegateFlag);
                        //attach logicalDependency root
                        depLogicalDependencies.AppendChild(DependencyRoot);
                    }
                    dependencyRoot.AppendChild(depLogicalDependencies);
                }
                DependenciesXml.AppendChild(dependencyRoot);
            }
            root.AppendChild(DependenciesXml);
            //dependencies
            XmlElement logicalDependenciesXml = doc.CreateElement("logicalDependencies");
            foreach (Dependency d in logicalDependencies)
            {
                //declare dependency root
                XmlElement logicalDependencyRoot = doc.CreateElement("logicalDependency");
                //make dependency
                //mandatory
                XmlElement logicalDepZipFile = doc.CreateElement("zipFile");
                if (!d.ZipFile.Trim().Equals(""))
                    logicalDepZipFile.InnerText = d.ZipFile.Trim();
                logicalDependencyRoot.AppendChild(logicalDepZipFile);

                XmlElement logicalDepCRC = doc.CreateElement("crc");
                if (!d.ZipFile.Trim().Equals(""))
                    logicalDepCRC.InnerText = d.CRC.Trim();
                logicalDependencyRoot.AppendChild(logicalDepCRC);

                XmlElement logicalDepEnabled = doc.CreateElement("enabled");
                logicalDepEnabled.InnerText = "" + d.Enabled;
                logicalDependencyRoot.AppendChild(logicalDepEnabled);

                XmlElement logicalDepPackageName = doc.CreateElement("packageName");
                if (!d.ZipFile.Trim().Equals(""))
                    logicalDepPackageName.InnerText = d.PackageName.Trim();
                logicalDependencyRoot.AppendChild(logicalDepPackageName);

                //optional
                if (d.Timestamp > 0)
                {
                    XmlElement logicalDepTimestamp = doc.CreateElement("timestamp");
                    logicalDepTimestamp.InnerText = d.Timestamp.ToString();
                    logicalDependencyRoot.AppendChild(logicalDepTimestamp);
                }
                if (!d.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(d.StartAddress))
                {
                    XmlElement logicalDepStartAddress = doc.CreateElement("startAddress");
                    logicalDepStartAddress.InnerText = d.StartAddress.Trim();
                    logicalDependencyRoot.AppendChild(logicalDepStartAddress);
                }
                if (!d.StartAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(d.EndAddress))
                {
                    XmlElement logicalDepEndAddress = doc.CreateElement("endAddress");
                    logicalDepEndAddress.InnerText = d.EndAddress.Trim();
                    logicalDependencyRoot.AppendChild(logicalDepEndAddress);
                }
                if(!string.IsNullOrWhiteSpace(d.DevURL))
                {
                    XmlElement logicalDepdevURL = doc.CreateElement("devURL");
                    logicalDepdevURL.InnerText = d.DevURL.Trim();
                    logicalDependencyRoot.AppendChild(logicalDepdevURL);
                }
                if (!d.LogAtInstall)
                {
                    XmlElement logicalDepLogAtInstall = doc.CreateElement("logAtInstall");
                    logicalDepLogAtInstall.InnerText = d.LogAtInstall.ToString();
                    logicalDependencyRoot.AppendChild(logicalDepLogAtInstall);
                }
                //attach dependency root
                logicalDependenciesXml.AppendChild(logicalDependencyRoot);
            }
            root.AppendChild(logicalDependenciesXml);
            //catagories
            XmlElement catagoriesHolder = doc.CreateElement("catagories");
            foreach (Category c in parsedCatagoryList)
            {
                //catagory root
                XmlElement catagoryRoot = doc.CreateElement("catagory");
                //make catagory
                XmlElement catagoryName = doc.CreateElement("name");
                catagoryName.InnerText = c.Name.Trim();
                catagoryRoot.AppendChild(catagoryName);
                //dependencies for catagory
                XmlElement catagoryDependencies = doc.CreateElement("dependencies");
                foreach (DatabaseLogic d in c.Dependencies)
                {
                    //declare dependency root
                    XmlElement DependencyRoot = doc.CreateElement("dependency");
                    XmlElement DepPackageName = doc.CreateElement("packageName");
                    DepPackageName.InnerText = d.PackageName.Trim();
                    DependencyRoot.AppendChild(DepPackageName);
                    //attach dependency root
                    catagoryDependencies.AppendChild(DependencyRoot);
                }
                catagoryRoot.AppendChild(catagoryDependencies);
                //mods for catagory
                XmlElement modsHolder = doc.CreateElement("packages");
                foreach (SelectablePackage m in c.Packages)
                {
                    //add it to the list
                    XmlElement modRoot = doc.CreateElement("package");
                    //mandatory
                    XmlElement modName = doc.CreateElement("name");
                    if (!string.IsNullOrWhiteSpace(m.Name))
                        modName.InnerText = m.Name.Trim();
                    modRoot.AppendChild(modName);
                    XmlElement modZipFile = doc.CreateElement("zipFile");
                    if (!string.IsNullOrWhiteSpace(m.ZipFile))
                        modZipFile.InnerText = m.ZipFile.Trim();
                    modRoot.AppendChild(modZipFile);
                    XmlElement modZipCRC = doc.CreateElement("crc");
                    if (!string.IsNullOrWhiteSpace(m.CRC))
                        modZipCRC.InnerText = m.CRC.Trim();
                    modRoot.AppendChild(modZipCRC);
                    XmlElement modEnabled = doc.CreateElement("enabled");
                    modEnabled.InnerText = "" + m.Enabled;
                    modRoot.AppendChild(modEnabled);
                    XmlElement modVisible = doc.CreateElement("visible");
                    modVisible.InnerText = "" + m.Visible;
                    modRoot.AppendChild(modVisible);
                    XmlElement modPackageName = doc.CreateElement("packageName");
                    if (!string.IsNullOrWhiteSpace(m.PackageName))
                        modPackageName.InnerText = m.PackageName.Trim();
                    modRoot.AppendChild(modPackageName);
                    XmlElement modType = doc.CreateElement("type");
                    if (!string.IsNullOrWhiteSpace(m.Type))
                        modType.InnerText = m.Type.Trim();
                    modRoot.AppendChild(modType);
                    //optional
                    if (!string.IsNullOrWhiteSpace(m.Version))
                    {
                        XmlElement modVersion = doc.CreateElement("version");
                        modVersion.InnerText = m.Version.Trim();
                        modRoot.AppendChild(modVersion);
                    }
                    if(m.Timestamp > 0)
                    {
                        XmlElement modTimestamp = doc.CreateElement("timestamp");
                        modTimestamp.InnerText = m.Timestamp.ToString();
                        modRoot.AppendChild(modTimestamp);
                    }
                    if(!m.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(m.StartAddress))
                    {
                        XmlElement modStartAddress = doc.CreateElement("startAddress");
                        modStartAddress.InnerText = m.StartAddress.Trim();
                        modRoot.AppendChild(modStartAddress);
                    }
                    if(!m.EndAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(m.StartAddress))
                    {
                        XmlElement modEndAddress = doc.CreateElement("endAddress");
                        modEndAddress.InnerText = m.EndAddress.Trim();
                        modRoot.AppendChild(modEndAddress);
                    }
                    if(m.Size > 0)
                    {
                        XmlElement modZipSize = doc.CreateElement("size");
                        modZipSize.InnerText = "" + m.Size;
                        modRoot.AppendChild(modZipSize);
                    }
                    if (!m.LogAtInstall)
                    {
                        XmlElement modLogAtInstall = doc.CreateElement("logAtInstall");
                        modLogAtInstall.InnerText = m.LogAtInstall.ToString();
                        modRoot.AppendChild(modLogAtInstall);
                    }
                    if (!string.IsNullOrWhiteSpace(m.UpdateComment))
                    {
                        XmlElement modUpdateComment = doc.CreateElement("updateComment");
                        modUpdateComment.InnerText = ConvertToXmlSaveFormat(m.UpdateComment);
                        modRoot.AppendChild(modUpdateComment);
                    }
                    if(!string.IsNullOrWhiteSpace(m.Description))
                    {
                        XmlElement modDescription = doc.CreateElement("description");
                        modDescription.InnerText = ConvertToXmlSaveFormat(m.Description);
                        modRoot.AppendChild(modDescription);
                    }
                    if(!string.IsNullOrWhiteSpace(m.DevURL))
                    {
                        XmlElement modDevURL = doc.CreateElement("devURL");
                        modDevURL.InnerText = m.DevURL.Trim();
                        modRoot.AppendChild(modDevURL);
                    }
                    //datas for the mods
                    if(m.UserFiles.Count > 0)
                    {
                        XmlElement modDatas = doc.CreateElement("userDatas");
                        foreach (UserFiles us in m.UserFiles)
                        {
                            XmlElement userData = doc.CreateElement("userData");
                            userData.InnerText = us.Pattern.Trim();
                            if (us.placeBeforeExtraction)
                                userData.SetAttribute("pre", "" + us.placeBeforeExtraction);
                            if (us.systemInitiated)
                                userData.SetAttribute("system", "" + us.systemInitiated);
                            modDatas.AppendChild(userData);
                        }
                        modRoot.AppendChild(modDatas);
                    }
                    //pictures for the mods
                    if(m.Medias.Count > 0)
                    {
                        XmlElement modPictures = doc.CreateElement("medias");
                        foreach (Media p in m.Medias)
                        {
                            XmlElement pictureRoot = doc.CreateElement("media");
                            XmlElement pictureType = doc.CreateElement("type");
                            XmlElement pictureURL = doc.CreateElement("URL");
                            pictureURL.InnerText = p.URL.Trim();
                            pictureType.InnerText = "" + (int)p.MediaType;
                            pictureRoot.AppendChild(pictureType);
                            pictureRoot.AppendChild(pictureURL);
                            modPictures.AppendChild(pictureRoot);
                        }
                        modRoot.AppendChild(modPictures);
                    }
                    if (m.Packages.Count > 0)
                    {
                        //configs for the mods
                        XmlElement configsHolder = doc.CreateElement("packages");
                        SaveDatabaseConfigLegacy(doc, configsHolder, m.Packages);
                        modRoot.AppendChild(configsHolder);
                    }
                    if(m.Dependencies.Count > 0)
                    {
                        XmlElement modDependencies = doc.CreateElement("dependencies");
                        foreach (DatabaseLogic d in m.Dependencies)
                        {
                            //declare dependency root
                            XmlElement DependencyRoot = doc.CreateElement("dependency");
                            //make dependency
                            XmlElement DepPackageName = doc.CreateElement("packageName");
                            DepPackageName.InnerText = d.PackageName.Trim();
                            DependencyRoot.AppendChild(DepPackageName);
                            //attach dependency root
                            modDependencies.AppendChild(DependencyRoot);
                        }
                        modRoot.AppendChild(modDependencies);
                    }
                    modsHolder.AppendChild(modRoot);
                }
                catagoryRoot.AppendChild(modsHolder);
                //append catagory
                catagoriesHolder.AppendChild(catagoryRoot);
            }
            root.AppendChild(catagoriesHolder);

            // Create an XML declaration.
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");

            // Add the new node to the document.
            XmlElement cDoc = doc.DocumentElement;
            doc.InsertBefore(xmldecl, cDoc);

            // save database file
            doc.Save(saveLocation);
        }

        private static void SaveDatabaseConfigLegacy(XmlDocument doc, XmlElement configsHolder, List<SelectablePackage> configsList)
        {
            foreach (SelectablePackage cc in configsList)
            {
                //add the config to the list
                XmlElement configRoot = doc.CreateElement("package");
                configsHolder.AppendChild(configRoot);
                //mandatory
                XmlElement configName = doc.CreateElement("name");
                if (!string.IsNullOrWhiteSpace(cc.Name))
                    configName.InnerText = cc.Name.Trim();
                configRoot.AppendChild(configName);
                XmlElement configZipFile = doc.CreateElement("zipFile");
                if (!string.IsNullOrWhiteSpace(cc.ZipFile))
                    configZipFile.InnerText = cc.ZipFile.Trim();
                configRoot.AppendChild(configZipFile);
                XmlElement configZipCRC = doc.CreateElement("crc");
                if (!string.IsNullOrWhiteSpace(cc.CRC))
                    configZipCRC.InnerText = cc.CRC.Trim();
                configRoot.AppendChild(configZipCRC);
                XmlElement configEnabled = doc.CreateElement("enabled");
                configEnabled.InnerText = "" + cc.Enabled;
                configRoot.AppendChild(configEnabled);
                XmlElement configVisible = doc.CreateElement("visible");
                configVisible.InnerText = "" + cc.Visible;
                configRoot.AppendChild(configVisible);
                XmlElement configPackageName = doc.CreateElement("packageName");
                if (!string.IsNullOrWhiteSpace(cc.PackageName))
                    configPackageName.InnerText = cc.PackageName.Trim();
                configRoot.AppendChild(configPackageName);
                XmlElement configType = doc.CreateElement("type");
                if (!string.IsNullOrWhiteSpace(cc.Type))
                    configType.InnerText = cc.Type;
                configRoot.AppendChild(configType);
                //optional
                if(!string.IsNullOrWhiteSpace(cc.Version))
                {
                    XmlElement configVersion = doc.CreateElement("version");
                    configVersion.InnerText = cc.Version.Trim();
                    configRoot.AppendChild(configVersion);
                }
                if(cc.Timestamp > 0)
                {
                    XmlElement configTimestamp = doc.CreateElement("timestamp");
                    configTimestamp.InnerText = cc.Timestamp.ToString();
                    configRoot.AppendChild(configTimestamp);
                }
                if(!cc.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(cc.StartAddress))
                {
                    XmlElement configStartAddress = doc.CreateElement("startAddress");
                    configStartAddress.InnerText = cc.StartAddress.Trim();
                    configRoot.AppendChild(configStartAddress);
                }
                if(!cc.EndAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(cc.StartAddress))
                {
                    XmlElement configEndAddress = doc.CreateElement("endAddress");
                    configEndAddress.InnerText = cc.EndAddress.Trim();
                    configRoot.AppendChild(configEndAddress);
                }
                if(cc.Size > 0)
                {
                    XmlElement configSize = doc.CreateElement("size");
                    configSize.InnerText = "" + cc.Size;
                    configRoot.AppendChild(configSize);
                }
                if (!cc.LogAtInstall)
                {
                    XmlElement configLogAtInstall = doc.CreateElement("logAtInstall");
                    configLogAtInstall.InnerText = cc.LogAtInstall.ToString();
                    configRoot.AppendChild(configLogAtInstall);
                }
                if (!string.IsNullOrWhiteSpace(cc.UpdateComment))
                {
                    XmlElement configComment = doc.CreateElement("updateComment");
                    configComment.InnerText = ConvertToXmlSaveFormat(cc.UpdateComment);
                    configRoot.AppendChild(configComment);
                }
                if(!string.IsNullOrWhiteSpace(cc.Description))
                {
                    XmlElement configDescription = doc.CreateElement("description");
                    configDescription.InnerText = ConvertToXmlSaveFormat(cc.Description);
                    configRoot.AppendChild(configDescription);
                }
                if(!string.IsNullOrWhiteSpace(cc.DevURL))
                {
                    XmlElement configDevURL = doc.CreateElement("devURL");
                    configDevURL.InnerText = cc.DevURL.Trim();
                    configRoot.AppendChild(configDevURL);
                }
                //datas for the mods
                if(cc.UserFiles.Count > 0)
                {
                    XmlElement configDatas = doc.CreateElement("userDatas");
                    foreach (UserFiles us in cc.UserFiles)
                    {
                        XmlElement userData = doc.CreateElement("userData");
                        userData.InnerText = us.Pattern.Trim();
                        if (us.placeBeforeExtraction)
                            userData.SetAttribute("pre", "" + us.placeBeforeExtraction);
                        if (us.systemInitiated)
                            userData.SetAttribute("system", "" + us.systemInitiated);
                        configDatas.AppendChild(userData);
                    }
                    configRoot.AppendChild(configDatas);
                }
                //pictures for the configs
                if(cc.Medias.Count > 0)
                {
                    XmlElement configPictures = doc.CreateElement("medias");
                    foreach (Media p in cc.Medias)
                    {
                        XmlElement pictureRoot = doc.CreateElement("media");
                        XmlElement pictureType = doc.CreateElement("type");
                        XmlElement pictureURL = doc.CreateElement("URL");
                        pictureURL.InnerText = p.URL.Trim();
                        pictureType.InnerText = "" + (int)p.MediaType;
                        pictureRoot.AppendChild(pictureType);
                        pictureRoot.AppendChild(pictureURL);
                        configPictures.AppendChild(pictureRoot);
                    }
                    configRoot.AppendChild(configPictures);
                }
                //packages for the packages (meta)
                if(cc.Packages.Count > 0)
                {
                    XmlElement configsHolderSub = doc.CreateElement("packages");
                    //if statement here
                    SaveDatabaseConfigLegacy(doc, configsHolderSub, cc.Packages);
                    configRoot.AppendChild(configsHolderSub);
                }
                //dependencies for the packages
                if(cc.Dependencies.Count > 0)
                {
                    XmlElement catDependencies = doc.CreateElement("dependencies");
                    foreach (DatabaseLogic d in cc.Dependencies)
                    {
                        //declare dependency root
                        XmlElement DependencyRoot = doc.CreateElement("dependency");
                        //make dependency
                        XmlElement DepPackageName = doc.CreateElement("packageName");
                        DepPackageName.InnerText = d.PackageName.Trim();
                        DependencyRoot.AppendChild(DepPackageName);
                        //attach dependency root
                        catDependencies.AppendChild(DependencyRoot);
                    }
                    configRoot.AppendChild(catDependencies);
                }
                configsHolder.AppendChild(configRoot);
            }
        }
        #endregion

        #region Database Saving
        //saving database in a way that doesn't suck
        public static void SaveDatabase(string saveLocation, string gameVersion, string onlineFolderVersion, List<DatabasePackage> globalDependencies,
        List<Dependency> dependencies, List<Category> parsedCatagoryList)
        {
            //make root of document
            XmlDocument doc = new XmlDocument();
            //database root modInfo.xml
            XmlElement root = doc.CreateElement("modInfo.xml");
            root.SetAttribute("version", gameVersion);
            root.SetAttribute("onlineFolder", onlineFolderVersion);
            doc.AppendChild(root);
            //save global depednecies
            //idea: method to handle root leemnts here, but make a "save element" and from there do more if(element is SelectablePackage)
            //save dependencies
            
            //save categories
            
            //save databse
            if(File.Exists(saveLocation))
                File.Delete(saveLocation);
            doc.Save(saveLocation);
        }
        private static void SavePackage(XmlDocument root, XmlElement holder, DatabasePackage package)
        {
            //make the root element
            XmlElement element = root.CreateElement("globalDependency");
            SaveProperty(root, element, nameof(package.PackageName), package.PackageName, "there should always be a packageName lol", true);
            SaveProperty(root, element, nameof(package.Version), package.Version, "", true);
            SaveProperty(root, element, nameof(package.Timestamp), package.Timestamp.ToString(), ((long)0).ToString(), true);
            SaveProperty(root, element, nameof(package.ZipFile), package.ZipFile, "", true);
            SaveProperty(root, element, nameof(package.Enabled), package.Enabled.ToString(), false.ToString(), true);
            SaveProperty(root, element, nameof(package.CRC), package.CRC, "", true);
            SaveProperty(root, element, nameof(package.StartAddress), package.StartAddress, Settings.DefaultStartAddress, true);
            SaveProperty(root, element, nameof(package.EndAddress), package.EndAddress, Settings.DefaultEndAddress, true);
            SaveProperty(root, element, nameof(package.AppendExtraction), package.AppendExtraction.ToString(), false.ToString(), true);
            SaveProperty(root, element, nameof(package.LogAtInstall), package.LogAtInstall.ToString(), true.ToString(), true);
            SaveProperty(root, element, nameof(package.DevURL), package.DevURL, "", true);
            SaveProperty(root, element, nameof(package.UpdateInstructions), package.UpdateInstructions, "", true);
            SaveProperty(root, element, nameof(package.ExtractionLevel), package.ExtractionLevel.ToString(), ((int)5).ToString(), true);
            //save the basic elements (if they exist, otherwise defaults will be used on load)
            if (package is Dependency dependency)
            {
                //handle dependency specific stuff
                //like the Depedndencies
                if(dependency.Dependencies.Count > 0)
                {
                    //create holder and hold them
                    XmlElement dependenciesHolder = root.CreateElement("dependencies");
                    element.AppendChild(dependenciesHolder);
                    foreach(DatabaseLogic logics in dependency.Dependencies)
                    {
                        XmlElement dependencyLogic = root.CreateElement("dependency");
                        SaveProperty(root, dependencyLogic, nameof(logics.PackageName), logics.PackageName, "should always have this", false);
                        SaveProperty(root, dependencyLogic, nameof(logics.Enabled), logics.Enabled.ToString(), false.ToString(), false);
                        SaveProperty(root, dependencyLogic, nameof(logics.Checked), logics.Checked.ToString(), false.ToString(), false);
                        SaveProperty(root, dependencyLogic, nameof(logics.NotFlag), logics.NotFlag.ToString(), false.ToString(), false);
                        SaveProperty(root, dependencyLogic, nameof(logics.Logic), logics.Logic.ToString(), Logic.OR.ToString(), false);
                    }
                }
            }
            else if (package is SelectablePackage selectablePackage)
            {
                //handle package specific stuff
                SaveProperty(root, element, nameof(selectablePackage.Name), selectablePackage.Name, "", true, element[nameof(package.PackageName)]);
                SaveProperty(root, element, nameof(selectablePackage.Type), selectablePackage.Type, "", true, element[nameof(selectablePackage.Name)]);
                SaveProperty(root, element, nameof(selectablePackage.Visible), selectablePackage.Visible.ToString(), false.ToString(), true, element[nameof(selectablePackage.Enabled)]);
                SaveProperty(root, element, nameof(selectablePackage.Size), selectablePackage.Size.ToString(), ((int)0).ToString(), true, element[nameof(selectablePackage.CRC)]);
                SaveProperty(root, element, nameof(selectablePackage.UpdateComment), selectablePackage.UpdateComment, "", true);
                SaveProperty(root, element, nameof(selectablePackage.Description), selectablePackage.Description, "", true);
                SaveProperty(root, element, nameof(selectablePackage.Checked), selectablePackage.Checked.ToString(), false.ToString(), true, element[nameof(selectablePackage.Enabled)]);
                SaveProperty(root, element, nameof(selectablePackage.ShowInSearchList), selectablePackage.ShowInSearchList.ToString(), false.ToString(), true);
                if(selectablePackage.UserFiles.Count > 0)
                {
                    XmlElement userFilesHolder = root.CreateElement("userFiles");
                    element.AppendChild(userFilesHolder);
                    foreach(UserFiles file in selectablePackage.UserFiles)
                    {
                        XmlElement userFile = root.CreateElement("userFile");
                        userFile.InnerText = file.Pattern;
                        userFilesHolder.AppendChild(userFile);
                    }
                }
                if(selectablePackage.Medias.Count > 0)
                {
                    XmlElement mediasHolder = root.CreateElement("medias");
                    element.AppendChild(mediasHolder);
                    foreach(Media media in selectablePackage.Medias)
                    {
                        XmlElement mediaElement = root.CreateElement("media");
                        SaveProperty(root, mediaElement, nameof(media.MediaType), media.MediaType.ToString(), MediaType.Picture.ToString(), false);
                        SaveProperty(root, mediaElement, nameof(media.URL), media.URL, "", false);
                        mediasHolder.AppendChild(mediaElement);
                    }
                }
                if (selectablePackage.Dependencies.Count > 0)
                {
                    //create holder and hold them
                    XmlElement dependenciesHolder = root.CreateElement("dependencies");
                    element.AppendChild(dependenciesHolder);
                    foreach (DatabaseLogic logics in selectablePackage.Dependencies)
                    {
                        XmlElement dependencyLogic = root.CreateElement("dependency");
                        SaveProperty(root, dependencyLogic, nameof(logics.PackageName), logics.PackageName, "should always have this", false);
                        SaveProperty(root, dependencyLogic, nameof(logics.Enabled), logics.Enabled.ToString(), false.ToString(), false);
                        SaveProperty(root, dependencyLogic, nameof(logics.Checked), logics.Checked.ToString(), false.ToString(), false);
                        SaveProperty(root, dependencyLogic, nameof(logics.NotFlag), logics.NotFlag.ToString(), false.ToString(), false);
                        SaveProperty(root, dependencyLogic, nameof(logics.Logic), logics.Logic.ToString(), Logic.OR.ToString(), false);
                        dependenciesHolder.AppendChild(dependencyLogic);
                    }
                }
                if (selectablePackage.Packages.Count > 0)
                {
                    //create sub packages holder
                    XmlElement subPackagesHoder = root.CreateElement("packages");
                    element.AppendChild(subPackagesHoder);
                    foreach(SelectablePackage sp in selectablePackage.Packages)
                    {
                        XmlElement subPackage = root.CreateElement("package");
                        SavePackage(root, subPackage, sp);
                        subPackagesHoder.AppendChild(subPackage);
                    }
                }
            }
            //append to holder
            holder.AppendChild(element);
        }
        private static void SaveProperty(XmlDocument doc, XmlElement packageHolder, string propertyName, string propertyValue, string propertyDefault, bool elementType, XmlElement elementToInsertAfter = null)
        {
            //if the input value is the same as the default, don't even bother making it. saves space (and time to load it).
            if(propertyDefault.Equals(propertyValue))
              return;
            //TODO
            //if element, make->set->append
            if (elementType)
            {
                XmlElement element = doc.CreateElement(propertyName);
                element.InnerText = propertyValue;
                if (elementToInsertAfter != null)
                    packageHolder.InsertAfter(element, elementToInsertAfter);
                else
                    packageHolder.AppendChild(element);
            }
            //else if attribute, make->set->append
            else
            {
                XmlAttribute attribute = doc.CreateAttribute(propertyName);
                attribute.Value = propertyValue;
                packageHolder.Attributes.Append(attribute);
            }
        }
        #endregion

        #region Installer Parsing

        #endregion
    }
}
 