using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using System.Text;
using Ionic.Zip;
using RelhaxModpack.XmlBinary;

namespace RelhaxModpack
{
    /// <summary>
    /// Xml document load type enumeration
    /// </summary>
    public enum XmlLoadType
    {
        /// <summary>
        /// loading xml from a file on disk
        /// </summary>
        FromFile,
        /// <summary>
        /// loading xml from a text string
        /// </summary>
        FromString
    }

    public enum DatabaseXmlVersion
    {

        Legacy,

        OnePointOne
    }

    /// <summary>
    /// Utility class for all XML static methods
    /// </summary>
    public static class XMLUtils
    {
        #region Statics
        //static path for all developer selections
        public const string DeveloperSelectionsXPath = "TODO";
        #endregion

        #region XML Validating
        /// <summary>
        /// check to make sure an xml file is valid
        /// </summary>
        /// <param name="filePath">The path to the xml file</param>
        /// <returns>True if valid XML, false otherwise</returns>
        public static bool IsValidXml(string filePath)
        {
            return IsValidXml(File.ReadAllText(filePath), Path.GetFileName(filePath));
        }
        /// <summary>
        /// check to make sure an xml file is valid
        /// </summary>
        /// <param name="xmlString">The xml text string</param>
        /// <param name="fileName">the name of the file, used for debugging purposes</param>
        /// <returns>True if valid XML, false otherwise</returns>
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
        /// <summary>
        /// get an xml element attribute given an xml path
        /// </summary>
        /// <param name="file">The path to the XML file</param>
        /// <param name="xpath">The xpath search string</param>
        /// <returns>The value from the xpath search, otherwise null</returns>
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
        /// <summary>
        /// get an xml element attribute given an xml path
        /// </summary>
        /// <param name="xmlString">The XML text string</param>
        /// <param name="xpath">The xpath search string</param>
        /// <param name="filename"></param>
        /// <returns>The value from the xpath search, otherwise null</returns>
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
        /// <summary>
        /// get an xml element attribute given an xml path
        /// </summary>
        /// <param name="doc">The XML document object to check</param>
        /// <param name="xpath">The xpath search string</param>
        /// <returns>The value from the xpath search, otherwise null</returns>
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
            List<Dependency> dependencies, List<Category> parsedCategoryList, string location = null)
        {
            Logging.WriteToLog("start of ParseDatabase()", Logfiles.Application, LogLevel.Debug);
            //check all input parameters
            if (modInfoDocument == null)
                throw new BadMemeException("modInfoDocument is null dumbass");
            if (globalDependencies == null)
                throw new BadMemeException("lists cannot be null");
            else
                globalDependencies.Clear();
            if (dependencies == null)
                throw new BadMemeException("lists cannot be null");
            else
                dependencies.Clear();
            if (parsedCategoryList == null)
                throw new BadMemeException("lists cannot be null");
            else
                parsedCategoryList.Clear();
            //determine which version of the document we are loading. allows for loading of different versions if structure change
            //a blank value is assumed to be pre 2.0 version of the database
            string versionString = GetXMLStringFromXPath(modInfoDocument, "//modInfoAlpha.xml/@documentVersion");
            Logging.WriteToLog(nameof(versionString) + "=" + (string.IsNullOrEmpty(versionString)? "(null)" : versionString), Logfiles.Application, LogLevel.Debug);
            if (string.IsNullOrEmpty(versionString))
                Logging.Warning("versionString is null or empty, treating as legacy");
            switch(versionString)
            {
                case "1.1":
                    return ParseDatabase1V1(modInfoDocument, globalDependencies, dependencies, parsedCategoryList, location);
                default:
                    //parse legacy database
                    List<Dependency> logicalDependencies = new List<Dependency>();
                    ParseDatabaseLegacy(DocumentToXDocument(modInfoDocument), globalDependencies, dependencies, logicalDependencies,
                        parsedCategoryList, true);
                    dependencies.AddRange(logicalDependencies);
                    return true;
            }
        }
        public static bool ParseDatabase1V1(XmlDocument rootDocument, List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList, string rootPath)
        {
            //load each document to make sure they all exist first
            if(string.IsNullOrWhiteSpace(rootPath))
            {
                Logging.Error("location string is empty in ParseDatabase1V1");
                return false;
            }
            //document for global dependencies
            string completeFilepath = Path.Combine(rootPath, GetXMLStringFromXPath(rootDocument, "/modInfoAlpha.xml/globalDependencies/@file"));
            if (!File.Exists(completeFilepath))
            {
                Logging.Error("{0} file does not exist at {1}", "Global Dependency", completeFilepath);
                return false;
            }
            XDocument globalDepsDoc = LoadXDocument(completeFilepath, XmlLoadType.FromFile);
            if (globalDepsDoc == null)
                throw new BadMemeException("this should not be null");
            //document for dependencies
            completeFilepath = Path.Combine(rootPath, GetXMLStringFromXPath(rootDocument, "/modInfoAlpha.xml/dependencies/@file"));
            if (!File.Exists(completeFilepath))
            {
                Logging.Error("{0} file does not exist at {1}", "Dependency", completeFilepath);
                return false;
            }
            XDocument depsDoc = LoadXDocument(completeFilepath, XmlLoadType.FromFile);
            if (depsDoc == null)
                throw new BadMemeException("this should not be null");
            //list of documents for categories
            List<XDocument> categoryDocuments = new List<XDocument>();
            foreach(XmlNode categoryNode in GetXMLNodesFromXPath(rootDocument, "//modInfoAlpha.xml/categories/category"))
            {
                //make string path
                completeFilepath = Path.Combine(rootPath, categoryNode.Attributes["file"].Value);
                //check if file exists
                if (!File.Exists(completeFilepath))
                {
                    Logging.Error("{0} file does not exist at {1}", "Category", completeFilepath);
                    return false;
                }
                //load xdocument of category from category file
                XDocument catDoc = LoadXDocument(completeFilepath, XmlLoadType.FromFile);
                if (catDoc == null)
                    throw new BadMemeException("this should not be null");
                //make new category object
                Category cat = new Category()
                {
                    XmlFilename = categoryNode.Attributes["file"].Value
                };
                //get the list of dependencies in the category
                IEnumerable<XElement> listOfDependencies = catDoc.XPathSelectElements("//Category/Dependencies/Dependency");
                Utils.SetListEntriesField(cat, cat.GetType().GetField(nameof(cat.Dependencies)), listOfDependencies);
                //add object cat to list
                parsedCategoryList.Add(cat);
                //add xml cat to list
                categoryDocuments.Add(catDoc);
            }

            //parsing the global dependencies
            bool globalParsed = ParseDatabase1V1Packages(globalDepsDoc.XPathSelectElements("//GlobalDependencies/GlobalDependency").ToList(), globalDependencies,
                DatabasePackage.FieldsToXmlParseAttributes(),DatabasePackage.FieldsToXmlParseNodes(),typeof(DatabasePackage));
            //parsing the logical dependnecies
            bool depsParsed = ParseDatabase1V1Packages(depsDoc.XPathSelectElements("//Dependencies/Dependency").ToList(), dependencies,
                Dependency.FieldsToXmlParseAttributes(), Dependency.FieldsToXmlParseNodes(), typeof(Dependency));
            //parsing the categories
            bool categoriesParsed = true;
            for(int i = 0; i < categoryDocuments.Count; i++)
            {
                parsedCategoryList[i].Name = categoryDocuments[i].Root.FirstAttribute.Value;
                if (!ParseDatabase1V1Packages(categoryDocuments[i].XPathSelectElements("//Category/Package").ToList(), parsedCategoryList[i].Packages,
                    SelectablePackage.FieldsToXmlParseAttributes(), SelectablePackage.FieldsToXmlParseNodes(), typeof(SelectablePackage)))
                {
                    categoriesParsed = false;
                }
            }
            return globalParsed && depsParsed && categoriesParsed;
        }
        private static bool ParseDatabase1V1Packages(List<XElement> xmlPackageNodesList, IList genericPackageList,
            List<string> whitelistAttributes, List<string> whitelistNodes, Type packageType)
        {
            //make the refrence for the base type of package it could be
            DatabasePackage packageOfAnyType;

            //get all fields and properties from the class
            MemberInfo[] membersInClass = packageType.GetMembers();

            //for each xml package entry
            foreach(XElement xmlPackageNode in xmlPackageNodesList)
            {
                //make the instance based on the custom class type
                if (packageType.Equals(typeof(DatabasePackage)))
                {
                    packageOfAnyType = new DatabasePackage();
                }
                else if (packageType.Equals(typeof(Dependency)))
                {
                    packageOfAnyType = new Dependency();
                }
                else if (packageType.Equals(typeof(SelectablePackage)))
                {
                    packageOfAnyType = new SelectablePackage();
                }
                else
                {
                    throw new BadMemeException("fuck you");
                }

                //make a copy of the list of whitelist nodes
                List<string> whitelistNodesReal = new List<string>(whitelistNodes);

                //first deal with the xml attributes in the entry
                List<string> unknownListAttributes = new List<string>();
                List<string> missingAttributes = new List<string>(whitelistAttributes);
                foreach (XAttribute attribute in xmlPackageNode.Attributes())
                {
                    //get the fieldInfo or propertyInfo object representing the same name corresponding field or property in the memory database entry
                    MemberInfo[] matchingPackageMembers = membersInClass.Where(mem => mem.Name.Equals(attribute.Name.LocalName)).ToArray();
                    if(matchingPackageMembers.Count() == 0)
                    {
                        Logging.Debug("member {0} from xml attribute does not exist in fieldInfo", attribute.Name);
                        unknownListAttributes.Add(attribute.Name.LocalName);
                        continue;
                    }
                    MemberInfo packageMember = matchingPackageMembers[0];
                    //make sure it's a part of the whitelist of attributes we actually want to try to load
                    if(missingAttributes.Contains(packageMember.Name))
                    {
                        //remove the entry name cause it's loaded (or fail loading)
                        missingAttributes.Remove(packageMember.Name);
                        //try to set it
                        if(!Utils.SetObjectMember(packageOfAnyType,packageMember,attribute.Value))
                        {
                            Logging.Error("Failed to set member {0}, default (if exists) was used instead, PackageName: {1}, LineNumber {2}",
                                attribute.Name.LocalName, packageOfAnyType.PackageName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                        }
                    }
                    else
                    {
                        //unkonwn attribute, add it to the unknown list
                        unknownListAttributes.Add(packageMember.Name);
                    }
                }
                //list any unknown attrubutes here
                foreach(string unknownAttribute in unknownListAttributes)
                {
                    //log it here
                    Logging.Error("Unknown Attribute from xml node not in whitelist or memberInfo: {0}, PackageName: {1}, LineNumber {2}",
                        unknownAttribute, packageOfAnyType.PackageName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                }
                //list any attributes not included here (error, should be included)
                foreach(string missingAttribute in missingAttributes)
                {
                    //log it here
                    Logging.Error("Missing required attribute not in xmlInfo: {0}, PackageName: {1}, LineNumber {2}",
                        missingAttribute, packageOfAnyType.PackageName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                }

                //now deal with node values. no need to log what isn't set
                List<string> unknownListNodes = new List<string>();
                foreach(XElement element in xmlPackageNode.Elements())
                {
                    MemberInfo[] matchingPackageMembers = membersInClass.Where(field => field.Name.Equals(element.Name.LocalName)).ToArray();
                    if (matchingPackageMembers.Count() == 0)
                    {
                        Logging.Debug("field {0} from xml attribute does not exist in fieldInfo", element.Name);
                        unknownListAttributes.Add(element.Name.LocalName);
                        continue;
                    }
                    MemberInfo packageMember = matchingPackageMembers[0];
                    if (whitelistNodesReal.Contains(packageMember.Name))
                    {
                        whitelistNodesReal.Remove(packageMember.Name);
                        //BUT, if it's a package entry, we need to recursivly procsses it
                        if (packageOfAnyType is SelectablePackage throwAwayPackage && element.Name.LocalName.Equals(nameof(throwAwayPackage.Packages)))
                        {
                            //need hard code special case for Packages
                            ParseDatabase1V1Packages(element.Elements().ToList(), throwAwayPackage.Packages,
                                SelectablePackage.FieldsToXmlParseAttributes(), SelectablePackage.FieldsToXmlParseNodes(), packageType);
                        }
                        //HOWEVER, if the object is a list type, we need to parse the list first
                        //https://stackoverflow.com/questions/4115968/how-to-tell-whether-a-type-is-a-list-or-array-or-ienumerable-or
                        else if(packageMember is FieldInfo packageField && typeof(IEnumerable).IsAssignableFrom(packageField.FieldType) && !packageField.FieldType.Equals(typeof(string)))
                        {
                            if(!Utils.SetListEntriesField(packageOfAnyType,packageField, xmlPackageNode.Element(element.Name).Elements()))
                            {
                                Logging.Error("Failed to parse values for list object: {0}, PackageName: {1}, LineNumber {2}",
                                    element.Name, packageOfAnyType.PackageName, ((IXmlLineInfo)element).LineNumber);
                            }
                        }
                        else if (packageMember is PropertyInfo packageProperty && typeof(IEnumerable).IsAssignableFrom(packageProperty.PropertyType) && !packageProperty.PropertyType.Equals(typeof(string)))
                        {
                            throw new BadMemeException("Literally just copy and paste the method again in Utils from FieldInfo");
                        }
                        else if (!Utils.SetObjectMember(packageOfAnyType, packageMember, element.Value))
                        {
                            Logging.Error("Failed to set member {0}, default (if exists) was used instead, PackageName: {1}, LineNumber {2}",
                                element.Name.LocalName, packageOfAnyType.PackageName, ((IXmlLineInfo)element).LineNumber);
                        }
                    }
                    else
                    {
                        unknownListNodes.Add(packageMember.Name);
                    }
                }
                //list any unknown attrubutes here
                foreach (string unknownAttribute in unknownListNodes)
                {
                    //log it here
                    Logging.Error("Unknown Element from xml node not in whitelist or memberInfo: {0}, PackageName: {1}, LineNumber {2}",
                        unknownAttribute, packageOfAnyType.PackageName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                }
                //oh yeah, add it to the internal memory list
                //globalDependencies.Add(globalDependency);
                genericPackageList.Add(packageOfAnyType);
            }
            return true;
        }
        #endregion

        #region Other XML stuffs
        //https://blogs.msdn.microsoft.com/xmlteam/2009/03/31/converting-from-xmldocument-to-xdocument/
        public static XDocument DocumentToXDocument(XmlDocument doc)
        {
            if (doc == null)
                return null;
            return XDocument.Parse(doc.OuterXml,LoadOptions.SetLineInfo);
        }

        public static XDocument LoadXDocument(string fileOrXml, XmlLoadType type)
        {
            if(type == XmlLoadType.FromFile && !File.Exists(fileOrXml))
            {
                Logging.Error("XmlLoadType set to file and file does not exist at {0}", fileOrXml);
                return null;
            }
            try
            {
                switch(type)
                {
                    case XmlLoadType.FromFile:
                        return XDocument.Load(fileOrXml, LoadOptions.SetLineInfo);
                    case XmlLoadType.FromString:
                        return XDocument.Parse(fileOrXml, LoadOptions.SetLineInfo);
                }
            }
            catch(XmlException ex)
            {
                Logging.Exception(ex.ToString());
                return null;
            }
            return null;
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
                    case XmlLoadType.FromString:
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

        public static void UnpackXmlFile(XmlUnpack xmlUnpack, StringBuilder unpackBuilder)
        {
            //log info for debugging if need be
            Logging.Info(xmlUnpack.ToString());

            //check if new destination name for replacing
            string destinationFilename = string.IsNullOrWhiteSpace(xmlUnpack.NewFileName) ? xmlUnpack.FileName : xmlUnpack.NewFileName;
            string destinationCompletePath = Path.Combine(xmlUnpack.ExtractDirectory, destinationFilename);
            string sourceCompletePath = Path.Combine(xmlUnpack.DirectoryInArchive, xmlUnpack.FileName);

            //if the destination file already exists, then don't copy it over
            if(File.Exists(destinationCompletePath))
            {
                Logging.Info("Replacement file already exists, skipping");
                return;
            }

            Unpack(xmlUnpack.Pkg, sourceCompletePath, destinationCompletePath);

            Logging.Info("unpacking xml binary file (if binary)");
            try
            {
                XmlBinaryHandler binaryHandler = new XmlBinaryHandler();
                binaryHandler.UnpackXmlFile(destinationCompletePath);
            }
            catch (Exception xmlUnpackExceptino)
            {
                Logging.Exception(xmlUnpackExceptino.ToString());
            }
        }

        public static void Unpack(string package, string sourceCompletePath, string destinationCompletePath)
        {
            string destinationFilename = Path.GetFileName(destinationCompletePath);
            string destinationDirectory = Path.GetDirectoryName(destinationCompletePath);

            //if the package entry is empty, then it's just a file copy
            if (string.IsNullOrWhiteSpace(package))
            {
                if (File.Exists(sourceCompletePath))
                    File.Copy(sourceCompletePath, destinationCompletePath);
                Logging.Info("file copied");
            }
            else
            {
                if (!File.Exists(package))
                {
                    Logging.Error("packagefile does not exist, skipping");
                    return;
                }
                using (ZipFile zip = new ZipFile(package))
                {
                    //get the files that match the specified path from the xml entry
                    string zipPath = sourceCompletePath.Replace(@"\", @"/");
                    ZipEntry[] matchingEntries = zip.Where(zipp => zipp.FileName.Equals(zipPath)).ToArray();
                    Logging.Debug("matching zip entries: {0}", matchingEntries.Count());
                    if (matchingEntries.Count() > 0)
                    {
                        foreach (ZipEntry entry in matchingEntries)
                        {
                            //change the name to the destination
                            entry.FileName = destinationFilename;

                            //extract to disk and log
                            entry.Extract(destinationDirectory, ExtractExistingFileAction.DoNotOverwrite);
                            Logging.Info("entry extracted: {0}", destinationFilename);
                        }
                    }
                }
            }
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
                DatabasePackage d = new DatabasePackage();
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
                            d.AppendExtraction = Utils.ParseBool(globs.Value.Trim(), false);
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
                d.wasLogicalDependencyLegacy = false;
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
                            d.AppendExtraction = Utils.ParseBool(globs.Value.Trim(), false);
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
                                ld.Logic = Logic.AND;
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
                                d.Dependencies.Add(ld);
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
                d.wasLogicalDependencyLegacy = true;
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
                        case "installGroup":
                            cat.InstallGroup = Utils.ParseInt(catagoryNode.Value.Trim(), 0);
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
                                                    switch (modNode.Value)
                                                    {
                                                        case "multi":
                                                            m.Type = SelectionTypes.multi;
                                                            break;
                                                        case "single":
                                                            m.Type = SelectionTypes.single1;
                                                            break;
                                                        case "single1":
                                                            m.Type = SelectionTypes.single1;
                                                            break;
                                                        case "single_dropdown":
                                                            m.Type = SelectionTypes.single_dropdown1;
                                                            break;
                                                        case "single_dropdown1":
                                                            m.Type = SelectionTypes.single_dropdown1;
                                                            break;
                                                        case "single_dropdown2":
                                                            m.Type = SelectionTypes.single_dropdown2;
                                                            break;
                                                    }
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
                                                                UserFile uf = new UserFile();
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
                                    switch (configNode.Value)
                                    {
                                        case "multi":
                                            c.Type = SelectionTypes.multi;
                                            break;
                                        case "single":
                                            c.Type = SelectionTypes.single1;
                                            break;
                                        case "single1":
                                            c.Type = SelectionTypes.single1;
                                            break;
                                        case "single_dropdown":
                                            c.Type = SelectionTypes.single_dropdown1;
                                            break;
                                        case "single_dropdown1":
                                            c.Type = SelectionTypes.single_dropdown1;
                                            break;
                                        case "single_dropdown2":
                                            c.Type = SelectionTypes.single_dropdown2;
                                            break;
                                    }
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
                                                UserFile uf = new UserFile();
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
        public static void SaveDatabaseLegacy(string saveLocation, XmlDocument doc, List<DatabasePackage> globalDependencies,
            List<Dependency> dependencies, List<Category> parsedCatagoryList)
        {
            //V2 compatibility: dependencies need to be sorted into logical and regular for legacy database
            List<Dependency> logicalDependencies = dependencies.Where(dep => dep.wasLogicalDependencyLegacy).ToList();
            dependencies = dependencies.Where(dep => !dep.wasLogicalDependencyLegacy).ToList();

            // Create an XML declaration. (except it already has it, so don't)
            //XmlDeclaration xmldecl;
            //xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");

            // Add the new node to the document.
            //XmlElement cDoc = doc.DocumentElement;
            //doc.InsertBefore(xmldecl, cDoc);

            //database root modInfo.xml
            XmlElement root = doc.DocumentElement;

            //global dependencies
            XmlElement globalDependenciesXml = doc.CreateElement("globaldependencies");
            foreach (DatabasePackage d in globalDependencies)
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

                XmlElement logicalDepLogic = doc.CreateElement("logic");
                logicalDepLogic.InnerText = "AND";//hard-coded for legacy
                logicalDependencyRoot.AppendChild(logicalDepLogic);

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

                XmlElement categoryInstallGroup = doc.CreateElement("installGroup");
                categoryInstallGroup.InnerText = "" + c.InstallGroup;
                catagoryRoot.AppendChild(categoryInstallGroup);

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
                    if (m.Type != SelectionTypes.none)
                        modType.InnerText = m.Type.ToString().Trim();
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
                        foreach (UserFile us in m.UserFiles)
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
                if (cc.Type != SelectionTypes.none)
                    configType.InnerText = cc.Type.ToString().Trim();
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
                    foreach (UserFile us in cc.UserFiles)
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
        List<Dependency> dependencies, List<Category> parsedCatagoryList, DatabaseXmlVersion versionToSaveAs)
        {
            //make root of document
            XmlDocument doc = new XmlDocument();
            //https://stackoverflow.com/questions/334256/how-do-i-add-a-custom-xmldeclaration-with-xmldocument-xmldeclaration
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            doc.AppendChild(xmlDeclaration);
            //database root modInfo.xml
            XmlElement root = doc.CreateElement("modInfoAlpha.xml");
            root.SetAttribute("version", gameVersion.Trim());
            root.SetAttribute("onlineFolder", onlineFolderVersion.Trim());
            //append the version information, game and online folder
            doc.AppendChild(root);
            switch (versionToSaveAs)
            {
                case DatabaseXmlVersion.Legacy:
                    //when legacy, saveLocation is a single file
                    if(Path.HasExtension(saveLocation))
                        SaveDatabaseLegacy(saveLocation, doc, globalDependencies, dependencies, parsedCatagoryList);
                    else
                        SaveDatabaseLegacy(Path.Combine(saveLocation, "modInfoAlpha.xml"), doc, globalDependencies, dependencies, parsedCatagoryList);
                    break;
                case DatabaseXmlVersion.OnePointOne:
                    //in 1.1, saveLocation is a document path
                    if (Path.HasExtension(saveLocation))
                        SaveDatabase1V1(Path.GetDirectoryName(saveLocation), doc, xmlDeclaration, globalDependencies, dependencies, parsedCatagoryList);
                    else
                        SaveDatabase1V1(saveLocation, doc, xmlDeclaration, globalDependencies, dependencies, parsedCatagoryList);
                    break;
            }            
        }

        public static void SaveDatabase1V1(string savePath, XmlDocument doc, XmlDeclaration xmlDeclaration, List<DatabasePackage> globalDependencies,
        List<Dependency> dependencies, List<Category> parsedCatagoryList)
        {
            //save the root/header database file
            XmlElement root = doc.DocumentElement;
            root.SetAttribute("documentVersion", "1.1");

            XmlElement xmlGlobalDependencies = doc.CreateElement("globalDependencies");
            xmlGlobalDependencies.SetAttribute("file", "globalDependencies.xml");
            root.AppendChild(xmlGlobalDependencies);

            XmlElement xmlDependencies = doc.CreateElement("dependencies");
            xmlDependencies.SetAttribute("file", "dependencies.xml");
            root.AppendChild(xmlDependencies);

            XmlElement xmlCategories = doc.CreateElement("categories");
            foreach(Category cat in parsedCatagoryList)
            {
                XmlElement xmlCategory = doc.CreateElement("category");
                if (string.IsNullOrWhiteSpace(cat.XmlFilename))
                {
                    cat.XmlFilename = cat.Name.Replace(" ", string.Empty);
                    cat.XmlFilename = cat.XmlFilename.Replace("/", "_") + ".xml";
                }
                xmlCategory.SetAttribute("file", cat.XmlFilename);
                xmlCategories.AppendChild(xmlCategory);
            }
            root.AppendChild(xmlCategories);
            doc.Save(Path.Combine(savePath, "database.xml"));

            //save each of the other lists
            XmlDocument xmlGlobalDependenciesFile = new XmlDocument();
            xmlDeclaration = xmlGlobalDependenciesFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlGlobalDependenciesFile.AppendChild(xmlDeclaration);
            XmlElement xmlGlobalDependenciesFileRoot = xmlGlobalDependenciesFile.CreateElement("GlobalDependencies");
            SaveDatabaseList1V1(globalDependencies, xmlGlobalDependenciesFileRoot, xmlGlobalDependenciesFile, "GlobalDependency");
            xmlGlobalDependenciesFile.AppendChild(xmlGlobalDependenciesFileRoot);
            xmlGlobalDependenciesFile.Save(Path.Combine(savePath, "globalDependencies.xml"));

            XmlDocument xmlDependenciesFile = new XmlDocument();
            xmlDeclaration = xmlDependenciesFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDependenciesFile.AppendChild(xmlDeclaration);
            XmlElement xmlDependenciesFileRoot = xmlDependenciesFile.CreateElement("Dependencies");
            SaveDatabaseList1V1(dependencies, xmlDependenciesFileRoot, xmlDependenciesFile, "Dependency");
            xmlDependenciesFile.AppendChild(xmlDependenciesFileRoot);
            xmlDependenciesFile.Save(Path.Combine(savePath, "dependencies.xml"));

            //for each cateory do the same thing
            foreach (Category cat in parsedCatagoryList)
            {
                XmlDocument xmlCategoryFile = new XmlDocument();
                xmlDeclaration = xmlCategoryFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlCategoryFile.AppendChild(xmlDeclaration);
                XmlElement xmlCategoryFileRoot = xmlCategoryFile.CreateElement("Category");
                xmlCategoryFileRoot.SetAttribute("Name", cat.Name);
                //need to incorporate the fact that categories have dependencies
                if (cat.Dependencies.Count > 0)
                {
                    XmlElement xmlCategoryDependencies = xmlCategoryFile.CreateElement("Dependencies");
                    foreach(DatabaseLogic logic in cat.Dependencies)
                    {
                        XmlElement xmlCategoryDependency = xmlCategoryFile.CreateElement("Dependency");
                        foreach(FieldInfo info in typeof(DatabaseLogic).GetFields())
                        {
                            xmlCategoryDependency.SetAttribute(info.Name,info.GetValue(logic).ToString());
                        }
                        xmlCategoryDependencies.AppendChild(xmlCategoryDependency);
                    }
                    xmlCategoryFileRoot.AppendChild(xmlCategoryDependencies);
                }
                SaveDatabaseList1V1(cat.Packages, xmlCategoryFileRoot, xmlCategoryFile, "Package");
                xmlCategoryFile.AppendChild(xmlCategoryFileRoot);
                xmlCategoryFile.Save(Path.Combine(savePath, cat.XmlFilename));
            }

        }

        private static void SaveDatabaseList1V1(IList packagesToSave, XmlElement documentRootElement, XmlDocument docToMakeElementsFrom, string nameToSaveElementsBy)
        {
            //save based on each type it is
            List<string> membersToXmlSaveAsAttributes = null;
            List<string> membersToXmlSaveAsNodes = null;
            FieldInfo[] fields = null;
            PropertyInfo[] properties = null;
            XmlElement PackageHolder = null;
            DatabasePackage samplePackageDefaults = null;

            if (packagesToSave is List<DatabasePackage>)
            {
                membersToXmlSaveAsAttributes = new List<string>(DatabasePackage.FieldsToXmlParseAttributes());
                membersToXmlSaveAsNodes = new List<string>(DatabasePackage.FieldsToXmlParseNodes());
                fields = typeof(DatabasePackage).GetFields();
                properties = typeof(DatabasePackage).GetProperties();
                samplePackageDefaults = new DatabasePackage();
            }
            else if (packagesToSave is List<Dependency>)
            {
                membersToXmlSaveAsAttributes = new List<string>(Dependency.FieldsToXmlParseAttributes());
                membersToXmlSaveAsNodes = new List<string>(Dependency.FieldsToXmlParseNodes());
                fields = typeof(Dependency).GetFields();
                properties = typeof(Dependency).GetProperties();
                samplePackageDefaults = new Dependency();
            }
            else if (packagesToSave is List<SelectablePackage>)
            {
                membersToXmlSaveAsAttributes = new List<string>(SelectablePackage.FieldsToXmlParseAttributes());
                membersToXmlSaveAsNodes = new List<string>(SelectablePackage.FieldsToXmlParseNodes());
                fields = typeof(SelectablePackage).GetFields();
                properties = typeof(SelectablePackage).GetProperties();
                samplePackageDefaults = new SelectablePackage();
            }

            for (int i = 0; i < packagesToSave.Count; i++)
            {
                //it's at least a databasePackage
                DatabasePackage packageToSaveOfAnyType = (DatabasePackage)packagesToSave[i];
                SelectablePackage packageOnlyUsedForNames = packageToSaveOfAnyType as SelectablePackage;
                //make the element to save to
                PackageHolder = docToMakeElementsFrom.CreateElement(nameToSaveElementsBy);
                //iterate through each of the attributes and nodes in the arrays to allow for listing in custom order
                foreach(string memberAttribute in membersToXmlSaveAsAttributes)
                {
                    //first check if it is in fields, then if it is in properties
                    FieldInfo[] fieldMatches = fields.Where(f => f.Name.Equals(memberAttribute)).ToArray();
                    PropertyInfo[] propertyMatches = properties.Where(p => p.Name.Equals(memberAttribute)).ToArray();
                    if (fieldMatches.Count() == 1)
                    {
                        FieldInfo fieldInType = fieldMatches[0];
                        PackageHolder.SetAttribute(fieldInType.Name, Utils.MacroReplace(fieldInType.GetValue(packageToSaveOfAnyType).ToString(), ReplacementTypes.TextEscape));
                    }
                    else if (propertyMatches.Count() == 1)
                    {
                        PropertyInfo propertyInType = propertyMatches[0];
                        PackageHolder.SetAttribute(propertyInType.Name, propertyInType.GetValue(packageToSaveOfAnyType).ToString());
                    }
                    else
                        throw new BadMemeException("this should not happen. something is very wrong");
                }
                foreach (string memberNode in membersToXmlSaveAsNodes)
                {
                    FieldInfo[] fieldMatches = fields.Where(f => f.Name.Equals(memberNode)).ToArray();
                    PropertyInfo[] propertyMatches = properties.Where(p => p.Name.Equals(memberNode)).ToArray();
                    if (fieldMatches.Count() == 1)
                    {
                        FieldInfo fieldInType = fieldMatches[0];
                        //check if it's a package list of packages
                        if (fieldInType.Name.Equals(nameof(packageOnlyUsedForNames.Packages)) && packageOnlyUsedForNames.Packages.Count > 0)
                        {
                            XmlElement packagesHolder = docToMakeElementsFrom.CreateElement(nameof(packageOnlyUsedForNames.Packages));
                            SaveDatabaseList1V1(packageOnlyUsedForNames.Packages, packagesHolder, docToMakeElementsFrom, nameToSaveElementsBy);
                            PackageHolder.AppendChild(packagesHolder);
                        }
                        //if it is a list type like media or
                        else if (typeof(IEnumerable).IsAssignableFrom(fieldInType.FieldType) && !fieldInType.FieldType.Equals(typeof(string)))
                        {
                            //get the list type to allow for itterate
                            IList list = (IList)fieldInType.GetValue(packageToSaveOfAnyType);
                            //if there's no items, then don't bother
                            if (list.Count == 0)
                                continue;
                            //get the types of objects stored in this list
                            Type objectTypeInList = list.GetType().GetInterfaces().Where(j => j.IsGenericType && j.GenericTypeArguments.Length == 1)
                                .FirstOrDefault(j => j.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];
                            //elementFieldHolder is holder for list type like "Medias"
                            XmlElement elementFieldHolder = docToMakeElementsFrom.CreateElement(fieldInType.Name);
                            for (int k = 0; k < list.Count; k++)
                            {
                                //list element value like "Media"
                                string objectInListName = objectTypeInList.Name;
                                //hard code compatibility "DatabaseLogic" -> "Dependency"
                                if (objectInListName.Equals(nameof(DatabaseLogic)))
                                    objectInListName = nameof(Dependency);
                                XmlElement elementFieldValue = docToMakeElementsFrom.CreateElement(objectInListName);
                                //could be a custom type with many fields, or a single type with no fields
                                FieldInfo[] fieldsInCustomType = objectTypeInList.GetFields();
                                if (fieldsInCustomType.Count() == 0)
                                {
                                    //single type like int or string
                                    elementFieldValue.InnerText = list[k].ToString();
                                }
                                else
                                {
                                    //custom type like media
                                    foreach (FieldInfo field in objectTypeInList.GetFields())
                                    {
                                        //at this time all custom classes only use fields and are stored as attributes
                                        elementFieldValue.SetAttribute(field.Name, field.GetValue(list[k]).ToString());
                                    }
                                }
                                elementFieldHolder.AppendChild(elementFieldValue);
                            }
                            PackageHolder.AppendChild(elementFieldHolder);
                        }
                        else
                        {
                            XmlElement element = docToMakeElementsFrom.CreateElement(fieldInType.Name);
                            element.InnerText = fieldInType.GetValue(packageToSaveOfAnyType).ToString();
                            string defaultFieldValue = fieldInType.GetValue(samplePackageDefaults).ToString();
                            //only save node values when they are not default
                            if (!element.InnerText.Equals(defaultFieldValue))
                            {
                                element.InnerText = Utils.MacroReplace(element.InnerText, ReplacementTypes.TextEscape);
                                PackageHolder.AppendChild(element);
                            }
                        }
                    }
                    else if (propertyMatches.Count() == 1)
                    {
                        PropertyInfo propertyInType = propertyMatches[0];
                        //at this time, properties don't store list attributes nor are packages lists
                        XmlElement element = docToMakeElementsFrom.CreateElement(propertyInType.Name);
                        element.InnerText = propertyInType.GetValue(packageToSaveOfAnyType).ToString();
                        string defaultFieldValue = propertyInType.GetValue(samplePackageDefaults).ToString();
                        if (!element.InnerText.Equals(defaultFieldValue))
                            PackageHolder.AppendChild(element);
                    }
                    else
                        throw new BadMemeException("this should not happen. something is very wrong");
                }
                //save them to the holder
                documentRootElement.AppendChild(PackageHolder);
            }
        }
        #endregion
        
    }
}
 