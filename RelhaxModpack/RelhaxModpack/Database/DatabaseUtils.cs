using RelhaxModpack.Utilities;
using RelhaxModpack.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using RelhaxModpack.Utilities.Enums;
using System.Net;
using System.IO;
using System.Xml.XPath;

namespace RelhaxModpack.Database
{
    #region Structs
    /// <summary>
    /// Allows the old and new versions of a SelectablePackage to be saved temporarily for comparing differences between two database structures
    /// </summary>
    public struct DatabaseBeforeAfter
    {
        /// <summary>
        /// The package reference for the database before changes
        /// </summary>
        public SelectablePackage Before;

        /// <summary>
        /// The package reference for the database after changes
        /// </summary>
        public SelectablePackage After;
    }

    /// <summary>
    /// Allows the old and new versions of a DatabasePackage to be saved temporarily for comparing differences between two database structures
    /// </summary>
    public struct DatabaseBeforeAfter2
    {
        /// <summary>
        /// The package reference for the database before changes
        /// </summary>
        public DatabasePackage Before;

        /// <summary>
        /// The package reference for the database after changes
        /// </summary>
        public DatabasePackage After;
    }

    /// <summary>
    /// A structure object to contain the WoT client version and online folder version. Allows for LINQ searching
    /// </summary>
    public struct VersionInfos
    {
        /// <summary>
        /// The WoT client version e.g. 1.5.1.3
        /// </summary>
        public string WoTClientVersion;

        /// <summary>
        /// The online folder number (major game version) that contains the game zip files
        /// </summary>
        public string WoTOnlineFolderVersion;

        /// <summary>
        /// Overrides the ToString() function to display the two properties
        /// </summary>
        /// <returns>Displays the WoTClientVersion and WoTOnlineFolderVersion</returns>
        public override string ToString()
        {
            return string.Format("WoTClientVersion={0}, WoTOnlineFolderVersion={1}", WoTClientVersion, WoTOnlineFolderVersion);
        }
    }

    /// <summary>
    /// A structure used to keep a reference of a component and a dependency that it calls
    /// </summary>
    /// <remarks>This is used to determine if any packages call any dependencies who's packageName does not exist in the database</remarks>
    public struct LogicTracking
    {
        /// <summary>
        /// The database component what has dependencies
        /// </summary>
        public IComponentWithDependencies ComponentWithDependencies;

        /// <summary>
        /// The called dependency from the component
        /// </summary>
        public DatabaseLogic DatabaseLogic;
    }
    #endregion

    /// <summary>
    /// A utility class for working with database components
    /// </summary>
    public static class DatabaseUtils
    {
        #region Getting beta database download links from github
        /// <summary>
        /// Downloads the root 'database.xml' file from github using the selected branch and loads it to an XmlDocument object
        /// </summary>
        /// <returns>the XmlDocument object of the root database file</returns>
        public static XmlDocument GetBetaDatabaseRoot1V1Document()
        {
            XmlDocument rootDocument = null;
            using (WebClient client = new WebClient())
            {
                //load string constant url from manager info xml
                string rootXml = Settings.BetaDatabaseV2FolderURL + Settings.BetaDatabaseV2RootFilename;

                //download the xml string into "modInfoXml"
                client.Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                rootDocument = XmlUtils.LoadXmlDocument(client.DownloadString(rootXml), XmlLoadType.FromString);
            }
            return rootDocument;
        }

        /// <summary>
        /// Get a list of all URLs to each xml document file of the database
        /// </summary>
        /// <param name="rootDocument">The root 'database.xml' document object</param>
        /// <returns>The list of URLs</returns>
        public static List<string> GetBetaDatabase1V1FilesList(XmlDocument rootDocument)
        {
            //global and logical dependencies
            List<string> databaseFiles = new List<string>()
            {
                Settings.BetaDatabaseV2FolderURL + XmlUtils.GetXmlStringFromXPath(rootDocument, "/modInfoAlpha.xml/globalDependencies/@file"),
                Settings.BetaDatabaseV2FolderURL + XmlUtils.GetXmlStringFromXPath(rootDocument, "/modInfoAlpha.xml/dependencies/@file")
            };

            //categories
            foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(rootDocument, "//modInfoAlpha.xml/categories/category"))
            {
                string categoryFileName = categoryNode.Attributes["file"].Value;
                databaseFiles.Add(Settings.BetaDatabaseV2FolderURL + categoryFileName);
            }

            return databaseFiles.Select(name => name.Replace(".Xml", ".xml")).ToList();
        }

        /// <summary>
        /// Get a list of all URLs to each xml document file of the database using the selected branch and loads it to an XmlDocument object
        /// </summary>
        /// <returns>The list of URLs</returns>
        public static List<string> GetBetaDatabase1V1FilesList()
        {
            return GetBetaDatabase1V1FilesList(GetBetaDatabaseRoot1V1Document());
        }
        #endregion

        #region Database Loading
        /// <summary>
        /// Parse the Xml database of any type into lists in memory
        /// </summary>
        /// <param name="modInfoDocument">The root document. Can either be the entire document (Legacy) or the file with database header information only (OnePointOne)</param>
        /// <param name="globalDependencies">The global dependencies list</param>
        /// <param name="dependencies">The dependencies list</param>
        /// <param name="parsedCategoryList">The category list</param>
        /// <param name="location">The folder path to the OnePointOne+ additional xml files as part of the database</param>
        /// <returns>True if database parsing was a success, false otherwise</returns>
        public static bool ParseDatabase(XmlDocument modInfoDocument, List<DatabasePackage> globalDependencies,
            List<Dependency> dependencies, List<Category> parsedCategoryList, string location = null)
        {
            Logging.Debug("[ParseDatabase]: Determining database version for parsing");
            //check all input parameters
            if (modInfoDocument == null)
                throw new ArgumentNullException("modInfoDocument is null");
            if (globalDependencies == null)
                throw new ArgumentNullException("lists cannot be null");
            else
                globalDependencies.Clear();
            if (dependencies == null)
                throw new ArgumentNullException("lists cannot be null");
            else
                dependencies.Clear();
            if (parsedCategoryList == null)
                throw new ArgumentNullException("lists cannot be null");
            else
                parsedCategoryList.Clear();

            //determine which version of the document we are loading. allows for loading of different versions if structure change
            //a blank value is assumed to be pre 2.0 version of the database
            string versionString = XmlUtils.GetXmlStringFromXPath(modInfoDocument, "//modInfoAlpha.xml/@documentVersion");
            Logging.Info("[ParseDatabase]: VersionString parsed as '{0}'", versionString);
            if (string.IsNullOrEmpty(versionString))
                Logging.Info("[ParseDatabase]: VersionString is null or empty, treating as legacy");

            switch (versionString)
            {
                case "1.1":
                    return ParseDatabase1V1FromFiles(location, modInfoDocument, globalDependencies, dependencies, parsedCategoryList);
                default:
                    Logging.Error("Unknown version string: '{0}'", versionString);
                    return true;
            }
        }

        /// <summary>
        /// Parse a database into the version 1.1 format from files on the disk
        /// </summary>
        /// <param name="rootPath">The path to the folder that contains all the xml database files</param>
        /// <param name="rootDocument">The root document object</param>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns></returns>
        public static bool ParseDatabase1V1FromFiles(string rootPath, XmlDocument rootDocument, List<DatabasePackage> globalDependencies,
            List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            //load each document to make sure they all exist first
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                Logging.Error("location string is empty in ParseDatabase1V1");
                return false;
            }

            //document for global dependencies
            string completeFilepath = Path.Combine(rootPath, XmlUtils.GetXmlStringFromXPath(rootDocument, "/modInfoAlpha.xml/globalDependencies/@file"));
            if (!File.Exists(completeFilepath))
            {
                Logging.Error("{0} file does not exist at {1}", "Global Dependency", completeFilepath);
                return false;
            }
            XDocument globalDepsDoc = XmlUtils.LoadXDocument(completeFilepath, XmlLoadType.FromFile);
            if (globalDepsDoc == null)
                throw new BadMemeException("this should not be null");

            //document for dependencies
            completeFilepath = Path.Combine(rootPath, XmlUtils.GetXmlStringFromXPath(rootDocument, "/modInfoAlpha.xml/dependencies/@file"));
            if (!File.Exists(completeFilepath))
            {
                Logging.Error("{0} file does not exist at {1}", "Dependency", completeFilepath);
                return false;
            }
            XDocument depsDoc = XmlUtils.LoadXDocument(completeFilepath, XmlLoadType.FromFile);
            if (depsDoc == null)
                throw new BadMemeException("this should not be null");

            //list of documents for categories
            List<XDocument> categoryDocuments = new List<XDocument>();
            foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(rootDocument, "//modInfoAlpha.xml/categories/category"))
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
                XDocument catDoc = XmlUtils.LoadXDocument(completeFilepath, XmlLoadType.FromFile);
                if (catDoc == null)
                    throw new BadMemeException("this should not be null");

                //add Xml cat to list
                categoryDocuments.Add(catDoc);
            }
            //run the loading method
            return ParseDatabase1V1(globalDepsDoc, globalDependencies, depsDoc, dependencies, categoryDocuments, parsedCategoryList);
        }

        /// <summary>
        /// Parse a database into version 1.1 from string representations of the Xml files
        /// </summary>
        /// <param name="globalDependenciesXml">The Xml string of the global dependencies document</param>
        /// <param name="dependneciesXml">The Xml string of the dependencies document</param>
        /// <param name="categoriesXml">The list of Xml strings of the categories document</param>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns></returns>
        public static bool ParseDatabase1V1FromStrings(string globalDependenciesXml, string dependneciesXml, List<string> categoriesXml,
            List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            Logging.Debug(LogOptions.MethodName, "Parsing global dependencies");
            XDocument globalDependenciesdoc = XmlUtils.LoadXDocument(globalDependenciesXml, XmlLoadType.FromString);

            Logging.Debug(LogOptions.MethodName, "Parsing dependencies");
            XDocument dependenciesdoc = XmlUtils.LoadXDocument(dependneciesXml, XmlLoadType.FromString);

            Logging.Debug(LogOptions.MethodName, "Parsing categories");
            List<XDocument> categoryDocuments = new List<XDocument>();
            foreach (string category in categoriesXml)
            {
                categoryDocuments.Add(XmlUtils.LoadXDocument(category, XmlLoadType.FromString));
            }

            //check if any of the databases failed to parse
            if (globalDependenciesdoc == null)
            {
                Logging.Error("Failed to parse global dependencies xml");
                return false;
            }

            if (dependenciesdoc == null)
            {
                Logging.Error("Failed to parse dependencies xml");
                return false;
            }

            for (int i = 0; i < categoryDocuments.Count; i++)
            {
                if (categoryDocuments[i] == null)
                {
                    Logging.Error("Category document index {0} failed to parse", i);
                    return false;
                }
            }

            return ParseDatabase1V1(globalDependenciesdoc, globalDependencies, dependenciesdoc, dependencies, categoryDocuments, parsedCategoryList);
        }

        /// <summary>
        /// Parse a database into version 1.1 from XDocument objects
        /// </summary>
        /// <param name="globalDependenciesDoc">The Xml document of global dependencies</param>
        /// <param name="globalDependenciesList">The list of global dependencies</param>
        /// <param name="dependenciesDoc">The Xml document of dependencies</param>
        /// <param name="dependenciesList">The list of dependencies</param>
        /// <param name="categoryDocuments">The list of xml documents of the category Xml documents</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns></returns>
        private static bool ParseDatabase1V1(XDocument globalDependenciesDoc, List<DatabasePackage> globalDependenciesList, XDocument dependenciesDoc,
            List<Dependency> dependenciesList, List<XDocument> categoryDocuments, List<Category> parsedCategoryList)
        {
            //parsing the global dependencies
            Logging.Debug("[ParseDatabase1V1]: Parsing GlobalDependencies");
            bool globalParsed = ParseDatabase1V1Packages(globalDependenciesDoc.XPathSelectElements("/GlobalDependencies/GlobalDependency").ToList(), globalDependenciesList);

            //parsing the logical dependnecies
            Logging.Debug("[ParseDatabase1V1]: Parsing Dependencies");
            bool depsParsed = ParseDatabase1V1Packages(dependenciesDoc.XPathSelectElements("/Dependencies/Dependency").ToList(), dependenciesList);

            //parsing the categories
            bool categoriesParsed = true;
            for (int i = 0; i < categoryDocuments.Count; i++)
            {
                Category cat = new Category()
                {
                    //https://stackoverflow.com/questions/18887061/getting-attribute-value-using-xelement
                    Name = categoryDocuments[i].Root.Attribute("Name").Value
                };
                XElement result = categoryDocuments[i].XPathSelectElement(@"/Category/Maintainers");
                if (result != null)
                    cat.Maintainers = result.Value.ToString();

                //parse the list of dependencies from Xml for the categories into the category in list
                Logging.Debug("[ParseDatabase1V1]: Parsing Dependency references for category {0}", cat.Name);
                IEnumerable<XElement> listOfDependencies = categoryDocuments[i].XPathSelectElements("//Category/Dependencies/Dependency");
                CommonUtils.SetListEntries(cat, cat.GetType().GetProperty(nameof(cat.Dependencies)), listOfDependencies);

                //parse the list of packages
                Logging.Debug("[ParseDatabase1V1]: Parsing Packages for category {0}", cat.Name);
                if (!ParseDatabase1V1Packages(categoryDocuments[i].XPathSelectElements("/Category/Package").ToList(), cat.Packages))
                {
                    categoriesParsed = false;
                }
                parsedCategoryList.Add(cat);
            }
            return globalParsed && depsParsed && categoriesParsed;
        }

        private static bool ParseDatabase1V1Packages(List<XElement> xmlPackageNodesList, IList genericPackageList)
        {
            //get the type of element in the list
            Type listObjectType = genericPackageList.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1)
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];

            //for each Xml package entry
            foreach (XElement xmlPackageNode in xmlPackageNodesList)
            {
                //make sure object type is properly implemented into serialization system
                if (!(Activator.CreateInstance(listObjectType) is IXmlSerializable listEntry))
                    throw new BadMemeException("Type of this list is not of IXmlSerializable");

                IDatabaseComponent databasePackageObject = (IDatabaseComponent)listEntry;

                //create attribute and element unknown and missing lists
                List<string> unknownAttributes = new List<string>();
                List<string> missingAttributes = new List<string>(listEntry.PropertiesForSerializationAttributes());
                List<string> unknownElements = new List<string>();

                //first deal with the Xml attributes in the entry
                foreach (XAttribute attribute in xmlPackageNode.Attributes())
                {
                    string attributeName = attribute.Name.LocalName;

                    //check if the whitelist contains it
                    if (!listEntry.PropertiesForSerializationAttributes().Contains(attributeName))
                    {
                        Logging.Debug("Member {0} from Xml attribute does not exist in fieldInfo", attributeName);
                        unknownAttributes.Add(attributeName);
                        continue;
                    }

                    //get the propertyInfo object representing the same name corresponding field or property in the memory database entry
                    PropertyInfo property = listObjectType.GetProperty(attributeName);

                    //check if attribute exists in class object
                    if (property == null)
                    {
                        Logging.Error("Property (xml attribute) {0} exists in array for serialization, but not in class design!, ", attributeName);
                        Logging.Error("Package: {0}, line: {1}", databasePackageObject.ComponentInternalName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                        continue;
                    }

                    missingAttributes.Remove(attributeName);

                    if (!CommonUtils.SetObjectProperty(databasePackageObject, property, attribute.Value))
                    {
                        Logging.Error("Failed to set member {0}, default (if exists) was used instead, PackageName: {1}, LineNumber {2}",
                            attributeName, databasePackageObject.ComponentInternalName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                    }
                }

                //list any unknown or missing attributes
                foreach (string unknownAttribute in unknownAttributes)
                {
                    Logging.Error("Unknown Attribute from Xml node not in whitelist or memberInfo: {0}, PackageName: {1}, LineNumber {2}",
                        unknownAttribute, databasePackageObject.ComponentInternalName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                }
                foreach (string missingAttribute in missingAttributes)
                {
                    Logging.Error("Missing required attribute not in xmlInfo: {0}, PackageName: {1}, LineNumber {2}",
                        missingAttribute, databasePackageObject.ComponentInternalName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                }

                //now deal with element values. no need to log what isn't set (elements are optional)
                foreach (XElement element in xmlPackageNode.Elements())
                {
                    string elementName = element.Name.LocalName;

                    //check if the whitelist contains it
                    if (!listEntry.PropertiesForSerializationElements().Contains(elementName))
                    {
                        Logging.Debug("member {0} from Xml attribute does not exist in fieldInfo", elementName);
                        unknownElements.Add(elementName);
                        continue;
                    }

                    //get the propertyInfo object representing the same name corresponding field or property in the memory database entry
                    PropertyInfo property = listObjectType.GetProperty(elementName);

                    //check if attribute exists in class object
                    if (property == null)
                    {
                        Logging.Error("Property (xml attribute) {0} exists in array for serialization, but not in class design!, ", elementName);
                        Logging.Error("Package: {0}, line: {1}", databasePackageObject.ComponentInternalName, ((IXmlLineInfo)element).LineNumber);
                        continue;
                    }

                    //if it's a package entry, we need to recursivly procsses it
                    if (databasePackageObject is SelectablePackage throwAwayPackage && elementName.Equals(nameof(throwAwayPackage.Packages)))
                    {
                        //need hard code special case for Packages
                        ParseDatabase1V1Packages(element.Elements().ToList(), throwAwayPackage.Packages);
                    }

                    //if the object is a list type, we need to parse the list first
                    //https://stackoverflow.com/questions/4115968/how-to-tell-whether-a-type-is-a-list-or-array-or-ienumerable-or
                    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && !property.PropertyType.Equals(typeof(string)))
                    {
                        CommonUtils.SetListEntries(databasePackageObject, property, xmlPackageNode.Element(element.Name).Elements());
                    }
                    else if (!CommonUtils.SetObjectProperty(databasePackageObject, property, element.Value))
                    {
                        Logging.Error("Failed to set member {0}, default (if exists) was used instead, PackageName: {1}, LineNumber {2}",
                            element.Name.LocalName, databasePackageObject.ComponentInternalName, ((IXmlLineInfo)element).LineNumber);
                    }
                }

                //list any unknown attributes here
                foreach (string unknownelement in unknownElements)
                {
                    //log it here
                    Logging.Error("Unknown Element from Xml node not in whitelist or memberInfo: {0}, PackageName: {1}, LineNumber {2}",
                        unknownelement, databasePackageObject.ComponentInternalName, ((IXmlLineInfo)xmlPackageNode).LineNumber);
                }

                //add it to the internal memory list
                genericPackageList.Add(databasePackageObject);
            }
            return true;
        }
        #endregion

        #region Database Saving
        /// <summary>
        /// Save the database to an Xml version
        /// </summary>
        /// <param name="saveLocation">The folder path to save the files into</param>
        /// <param name="gameVersion">The version of the game that this database supports</param>
        /// <param name="onlineFolderVersion">The online folder for the zip file location of this database</param>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCatagoryList">The list of categories</param>
        /// <param name="versionToSaveAs">The Xml version of the database to save as</param>
        public static void SaveDatabase(string saveLocation, string gameVersion, string onlineFolderVersion, List<DatabasePackage> globalDependencies,
        List<Dependency> dependencies, List<Category> parsedCatagoryList, DatabaseXmlVersion versionToSaveAs)
        {
            //make root of document
            XmlDocument doc = new XmlDocument();

            //add declaration
            //https://stackoverflow.com/questions/334256/how-do-i-add-a-custom-xmldeclaration-with-xmldocument-xmldeclaration
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            doc.AppendChild(xmlDeclaration);

            //database root modInfo.Xml
            XmlElement root = doc.CreateElement("modInfoAlpha.xml");

            //add version and onlineFolder attributes to modInfoAlpha.xml element
            root.SetAttribute("version", gameVersion.Trim());
            root.SetAttribute("onlineFolder", onlineFolderVersion.Trim());

            //put root element into document
            doc.AppendChild(root);

            //
            switch (versionToSaveAs)
            {
                case DatabaseXmlVersion.OnePointOne:
                    //in 1.1, saveLocation is a document path
                    if (Path.HasExtension(saveLocation))
                        SaveDatabase1V1(Path.GetDirectoryName(saveLocation), doc, globalDependencies, dependencies, parsedCatagoryList);
                    else
                        SaveDatabase1V1(saveLocation, doc, globalDependencies, dependencies, parsedCatagoryList);
                    break;
            }
        }

        /// <summary>
        /// Save the database to the Xml version 1.1 standard
        /// </summary>
        /// <param name="savePath">The path to save all the xml files to</param>
        /// <param name="doc">The root XmlDocument to save the header information to</param>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCatagoryList">The list of categories</param>
        public static void SaveDatabase1V1(string savePath, XmlDocument doc, List<DatabasePackage> globalDependencies,
        List<Dependency> dependencies, List<Category> parsedCatagoryList)
        {
            //create root document (contains filenames for all other xml documents)
            XmlElement root = doc.DocumentElement;
            root.SetAttribute("documentVersion", "1.1");

            //create and append globalDependencies
            XmlElement xmlGlobalDependencies = doc.CreateElement("globalDependencies");
            xmlGlobalDependencies.SetAttribute("file", "globalDependencies.xml");
            root.AppendChild(xmlGlobalDependencies);

            //create and append dependencies
            XmlElement xmlDependencies = doc.CreateElement("dependencies");
            xmlDependencies.SetAttribute("file", "dependencies.xml");
            root.AppendChild(xmlDependencies);

            //create and append categories
            XmlElement xmlCategories = doc.CreateElement("categories");
            foreach (Category cat in parsedCatagoryList)
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

            //save the actual xml files for database entries
            //globalDependency
            XmlDocument xmlGlobalDependenciesFile = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlGlobalDependenciesFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlGlobalDependenciesFile.AppendChild(xmlDeclaration);
            XmlElement xmlGlobalDependenciesFileRoot = xmlGlobalDependenciesFile.CreateElement("GlobalDependencies");
            SaveDatabaseList1V1(globalDependencies, xmlGlobalDependenciesFileRoot, xmlGlobalDependenciesFile, "GlobalDependency");
            xmlGlobalDependenciesFile.AppendChild(xmlGlobalDependenciesFileRoot);
            xmlGlobalDependenciesFile.Save(Path.Combine(savePath, "globalDependencies.xml"));

            //dependency
            XmlDocument xmlDependenciesFile = new XmlDocument();
            xmlDeclaration = xmlDependenciesFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDependenciesFile.AppendChild(xmlDeclaration);
            XmlElement xmlDependenciesFileRoot = xmlDependenciesFile.CreateElement("Dependencies");
            SaveDatabaseList1V1(dependencies, xmlDependenciesFileRoot, xmlDependenciesFile, "Dependency");
            xmlDependenciesFile.AppendChild(xmlDependenciesFileRoot);
            xmlDependenciesFile.Save(Path.Combine(savePath, "dependencies.xml"));

            //for each category do the same thing
            foreach (Category cat in parsedCatagoryList)
            {
                XmlDocument xmlCategoryFile = new XmlDocument();
                xmlDeclaration = xmlCategoryFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlCategoryFile.AppendChild(xmlDeclaration);

                XmlElement xmlCategoryFileRoot = xmlCategoryFile.CreateElement("Category");

                //save attributes for category
                SavePropertiesToXmlAttributes(cat, xmlCategoryFileRoot, cat);

                //create and savae maintainers element if not default
                SavePropertiesToXmlElements(new Category(), nameof(cat.Maintainers), xmlCategoryFileRoot, cat, null, xmlCategoryFile);

                //need to incorporate the fact that categories have dependencies
                if (cat.Dependencies.Count > 0)
                {
                    XmlElement xmlCategoryDependencies = xmlCategoryFile.CreateElement("Dependencies");
                    foreach (DatabaseLogic logic in cat.Dependencies)
                    {
                        XmlElement xmlCategoryDependency = xmlCategoryFile.CreateElement("Dependency");
                        foreach (string attributeToSave in logic.PropertiesForSerializationAttributes())
                        {
                            PropertyInfo propertyInfo = logic.GetType().GetProperty(attributeToSave);
                            xmlCategoryDependency.SetAttribute(propertyInfo.Name, propertyInfo.GetValue(logic).ToString());
                        }
                        xmlCategoryDependencies.AppendChild(xmlCategoryDependency);
                    }
                    xmlCategoryFileRoot.AppendChild(xmlCategoryDependencies);
                }

                //then save packages
                SaveDatabaseList1V1(cat.Packages, xmlCategoryFileRoot, xmlCategoryFile, "Package");
                xmlCategoryFile.AppendChild(xmlCategoryFileRoot);
                xmlCategoryFile.Save(Path.Combine(savePath, cat.XmlFilename));
            }

        }

        /// <summary>
        /// Saves a list of packages to a document
        /// </summary>
        /// <param name="packagesToSave">The generic list of packages to save</param>
        /// <param name="documentRootElement">The element that will be holding this list</param>
        /// <param name="docToMakeElementsFrom">The document needed to create xml elements and attributes</param>
        /// <param name="nameToSaveElementsBy">The string name to save the xml element name by</param>
        private static void SaveDatabaseList1V1(IList packagesToSave, XmlElement documentRootElement, XmlDocument docToMakeElementsFrom, string nameToSaveElementsBy)
        {
            //based on list type, get list of elements and attributes to save as
            //get the type of element in the list
            Type listObjectType = packagesToSave.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1)
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];

            if (!(Activator.CreateInstance(listObjectType) is IXmlSerializable listEntryWithDefaultValues))
                throw new BadMemeException("Type of this list is not of IXmlSerializable");

            //parse for each package type
            for (int i = 0; i < packagesToSave.Count; i++)
            {
                //it's at least a databasePackage
                DatabasePackage packageToSaveOfAnyType = (DatabasePackage)packagesToSave[i];
                SelectablePackage packageOnlyUsedForNames = packageToSaveOfAnyType as SelectablePackage;

                //make the element to save to (Package, Dependency, etc.)
                XmlElement PackageHolder = docToMakeElementsFrom.CreateElement(nameToSaveElementsBy);

                //iterate through each of the attributes and nodes in the arrays to allow for listing in custom order
                SavePropertiesToXmlAttributes(listEntryWithDefaultValues, PackageHolder, packageToSaveOfAnyType);

                foreach (string elementToSave in listEntryWithDefaultValues.PropertiesForSerializationElements())
                {
                    PropertyInfo propertyOfPackage = listEntryWithDefaultValues.GetType().GetProperty(elementToSave);

                    //check if it's a package list of packages
                    if (packageOnlyUsedForNames != null && propertyOfPackage.Name.Equals(nameof(packageOnlyUsedForNames.Packages)) && packageOnlyUsedForNames.Packages.Count > 0)
                    {
                        XmlElement packagesHolder = docToMakeElementsFrom.CreateElement(nameof(packageOnlyUsedForNames.Packages));
                        SaveDatabaseList1V1(packageOnlyUsedForNames.Packages, packagesHolder, docToMakeElementsFrom, nameToSaveElementsBy);
                        PackageHolder.AppendChild(packagesHolder);
                    }
                    //else handle as standard element/element container
                    else
                    {
                        SavePropertiesToXmlElements(listEntryWithDefaultValues, elementToSave, PackageHolder, packageToSaveOfAnyType, propertyOfPackage, docToMakeElementsFrom);
                    }
                }
                //save them to the holder
                documentRootElement.AppendChild(PackageHolder);
            }
        }

        private static void SavePropertiesToXmlElements(IXmlSerializable databaseComponentWithDefaultValues, string elementToSave, XmlElement elementContainer, IDatabaseComponent componentToSave, PropertyInfo propertyOfComponentToSave, XmlDocument docToMakeElementsFrom)
        {
            if (propertyOfComponentToSave == null)
            {
                propertyOfComponentToSave = databaseComponentWithDefaultValues.GetType().GetProperty(elementToSave);
            }
            if (typeof(IEnumerable).IsAssignableFrom(propertyOfComponentToSave.PropertyType) && !propertyOfComponentToSave.PropertyType.Equals(typeof(string)))
            {
                //get the list type to allow for itterate
                IList list = (IList)propertyOfComponentToSave.GetValue(componentToSave);

                //if there's no items, then don't bother
                if (list.Count == 0)
                    return;

                //get the types of objects stored in this list
                Type objectTypeInList = list[0].GetType();

                if (!(Activator.CreateInstance(objectTypeInList) is IXmlSerializable subListEntry))
                    throw new BadMemeException("Type of this list is not of IXmlSerializable");

                //elementFieldHolder is holder for list type like "Medias" (the property name)
                XmlElement elementFieldHolder = docToMakeElementsFrom.CreateElement(propertyOfComponentToSave.Name);
                for (int k = 0; k < list.Count; k++)
                {
                    //list element value like "Media"
                    string objectInListName = objectTypeInList.Name;

                    //hard code compatibility "DatabaseLogic" -> "Dependency"
                    if (objectInListName.Equals(nameof(DatabaseLogic)))
                        objectInListName = nameof(Dependency);

                    //create the xml holder based on object name like "Media"
                    XmlElement elementFieldValue = docToMakeElementsFrom.CreateElement(objectInListName);

                    //if it's a single value type (like string)
                    if (objectTypeInList.IsValueType)
                    {
                        elementFieldValue.InnerText = list[k].ToString();
                    }
                    else
                    {
                        //custom type like media
                        //store attributes
                        foreach (string attributeToSave in subListEntry.PropertiesForSerializationAttributes())
                        {
                            PropertyInfo attributeProperty = objectTypeInList.GetProperty(attributeToSave);
                            elementFieldValue.SetAttribute(attributeProperty.Name, attributeProperty.GetValue(list[k]).ToString());
                        }

                        //store elements
                        foreach (string elementsToSave in subListEntry.PropertiesForSerializationElements())
                        {
                            PropertyInfo elementProperty = objectTypeInList.GetProperty(elementToSave);
                            string defaultValue = elementProperty.GetValue(subListEntry).ToString();
                            string currentValue = elementProperty.GetValue(list[k]).ToString();
                            if (currentValue != defaultValue)
                            {
                                XmlElement elementOfCustomType = docToMakeElementsFrom.CreateElement(elementToSave);
                                elementOfCustomType.InnerText = currentValue;
                                elementFieldValue.AppendChild(elementOfCustomType);
                            }
                        }
                    }

                    //add the "Media" to the "Medias"
                    elementFieldHolder.AppendChild(elementFieldValue);
                }
                elementContainer.AppendChild(elementFieldHolder);
            }
            //else the inner text of the node is only set/added if it's not default
            else
            {
                XmlElement element = docToMakeElementsFrom.CreateElement(propertyOfComponentToSave.Name);
                element.InnerText = propertyOfComponentToSave.GetValue(componentToSave).ToString();
                string defaultFieldValue = propertyOfComponentToSave.GetValue(databaseComponentWithDefaultValues).ToString();
                //only save node values when they are not default
                if (!element.InnerText.Equals(defaultFieldValue))
                {
                    element.InnerText = MacroUtils.MacroReplace(element.InnerText, ReplacementTypes.TextEscape);
                    elementContainer.AppendChild(element);
                }
            }
        }

        private static void SavePropertiesToXmlAttributes(IXmlSerializable databasePackageOfDefaultValues, XmlElement packageElement, IDatabaseComponent packageToSave)
        {
            //iterate through each of the attributes and nodes in the arrays to allow for listing in custom order
            foreach (string attributeToSave in databasePackageOfDefaultValues.PropertiesForSerializationAttributes())
            {
                PropertyInfo propertyOfPackage = databasePackageOfDefaultValues.GetType().GetProperty(attributeToSave);
                //attributs are value types, so just set it
                packageElement.SetAttribute(propertyOfPackage.Name, propertyOfPackage.GetValue(packageToSave).ToString());
            }
        }
        #endregion

        #region Other Utilities
        /// <summary>
        /// Returns a flat list of the given recursive lists, in the order that the parameters are stated
        /// </summary>
        /// <param name="globalDependnecies">The list of global dependences</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <returns>The flat list</returns>
        /// <remarks>In the case of Categories, the flat list has the sub-level packages added at the level of the parent</remarks>
        public static List<DatabasePackage> GetFlatList(List<DatabasePackage> globalDependnecies = null, List<Dependency> dependencies = null, List<Category> parsedCategoryList = null)
        {
            if (globalDependnecies == null && dependencies == null  && parsedCategoryList == null)
                return null;

            List<DatabasePackage> flatList = new List<DatabasePackage>();
            if (globalDependnecies != null)
                flatList.AddRange(globalDependnecies);
            if (dependencies != null)
                flatList.AddRange(dependencies);
            if (parsedCategoryList != null)
                foreach (Category cat in parsedCategoryList)
                    flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
        }

        /// <summary>
        /// Returns a flat list of the given recursive lists, in the order that the parameters are stated
        /// </summary>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <returns>The flat list</returns>
        /// <remarks>In the case of Categories, the flat list has the sub-level packages added at the level of the parent</remarks>
        public static List<SelectablePackage> GetFlatSelectablePackageList(List<Category> parsedCategoryList)
        {
            if (parsedCategoryList == null)
                return null;
            List<SelectablePackage> flatList = new List<SelectablePackage>();
            foreach (Category cat in parsedCategoryList)
                flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
        }

        /// <summary>
        /// Checks for any duplicate UID entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of packages with duplicate UIDs, or an empty list if no duplicates</returns>
        public static List<DatabasePackage> CheckForDuplicateUIDsPackageList(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            List<DatabasePackage> duplicatesList = new List<DatabasePackage>();
            List<DatabasePackage> flatList = GetFlatList(globalDependencies, dependencies, parsedCategoryList);
            foreach (DatabasePackage package in flatList)
            {
                List<DatabasePackage> packagesWithMatchingUID = flatList.FindAll(item => item.UID.Equals(package.UID));
                //by default it will at least match itself
                if (packagesWithMatchingUID.Count > 1)
                    duplicatesList.Add(package);
            }
            return duplicatesList;
        }

        /// <summary>
        /// Checks for any duplicate UID entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of duplicate UIDs, or an empty list if no duplicates</returns>
        public static List<string> CheckForDuplicateUIDsStringsList(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            return CheckForDuplicateUIDsPackageList(globalDependencies, dependencies, parsedCategoryList).Select(package => package.UID).ToList();
        }

        /// <summary>
        /// Checks for any duplicate PackageName entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of duplicate packages, or an empty list if no duplicates</returns>
        public static List<string> CheckForDuplicates(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            List<string> duplicatesList = new List<string>();
            List<DatabasePackage> flatList = GetFlatList(globalDependencies, dependencies, parsedCategoryList);
            foreach (DatabasePackage package in flatList)
            {
                List<DatabasePackage> packagesWithPackagename = flatList.Where(item => item.PackageName.Equals(package.PackageName)).ToList();
                if (packagesWithPackagename.Count > 1)
                    duplicatesList.Add(package.PackageName);
            }
            return duplicatesList;
        }

        /// <summary>
        /// Checks if a packageName exists within a list of packages
        /// </summary>
        /// <param name="packagesToCheckWith">The list of packages to check inside</param>
        /// <param name="nameToCheck">The PackageName parameter to check</param>
        /// <returns>True if the nameToCheck exists in the list, false otherwise</returns>
        public static bool IsDuplicateName(List<DatabasePackage> packagesToCheckWith, string nameToCheck)
        {
            foreach (DatabasePackage package in packagesToCheckWith)
            {
                if (package.PackageName.Equals(nameToCheck))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sorts the packages inside each Category object
        /// </summary>
        /// <param name="parsedCategoryList">The list of categories to sort</param>
        public static void SortDatabase(List<Category> parsedCategoryList)
        {
            //the first level of packages are always sorted
            foreach (Category cat in parsedCategoryList)
            {
                SortDatabase(cat.Packages);
            }
        }

        /// <summary>
        /// Sorts a list of packages
        /// </summary>
        /// <param name="packages">The list of packages to sort</param>
        /// <param name="recursive">If the list should recursively sort</param>
        private static void SortDatabase(List<SelectablePackage> packages, bool recursive = true)
        {
            //sorts packages in alphabetical order
            packages.Sort(SelectablePackage.CompareModsName);
            if (recursive)
            {
                //if set in the database, child elements can be sorted as well
                foreach (SelectablePackage child in packages)
                {
                    if (child.SortChildPackages)
                    {
                        Logging.Debug("Sorting packages of package {0}", child.PackageName);
                        SortDatabase(child.Packages);
                    }
                }
            }
        }

        /// <summary>
        /// Links all the references (like parent, etc) for each class object making it possible to traverse the list tree in memory
        /// </summary>
        /// <param name="ParsedCategoryList">The List of categories</param>
        /// <param name="buildFakeParents">If the header parent SelectablePackage objects should be built as well</param>
        public static void BuildLinksRefrence(List<Category> ParsedCategoryList, bool buildFakeParents)
        {
            foreach (Category cat in ParsedCategoryList)
            {
                if (buildFakeParents)
                {
                    cat.CategoryHeader = new SelectablePackage()
                    {
                        Name = string.Format("----------[{0}]----------", cat.Name),
                        TabIndex = cat.TabPage,
                        ParentCategory = cat,
                        Type = SelectionTypes.multi,
                        Visible = true,
                        Enabled = true,
                        Level = -1,
                        PackageName = string.Format("Category_{0}_Header", cat.Name.Replace(' ', '_')),
                        Packages = cat.Packages
                    };
                }
                foreach (SelectablePackage sp in cat.Packages)
                {
                    BuildLinksRefrence(sp, cat, cat.CategoryHeader);
                }
            }
        }

        /// <summary>
        /// Links all the references (like parent, etc) for each class object making it possible to traverse the list tree in memory
        /// </summary>
        /// <param name="sp">The package to perform linking on</param>
        /// <param name="cat">The category that the SelectablePackagesp belongs to</param>
        /// <param name="parent">The tree parent of sp</param>
        private static void BuildLinksRefrence(SelectablePackage sp, Category cat, SelectablePackage parent)
        {
            sp.Parent = parent;
            sp.TopParent = cat.CategoryHeader;
            sp.ParentCategory = cat;
            if (sp.Packages.Count > 0)
            {
                foreach (SelectablePackage sp2 in sp.Packages)
                {
                    BuildLinksRefrence(sp2, cat, sp);
                }
            }
        }

        /// <summary>
        /// Assigns the level parameter to the packages based on how recursively deep they are in the package sub lists
        /// </summary>
        /// <param name="ParsedCategoryList">The list of assign package values to</param>
        /// <param name="startingLevel">The starting level to assign the level parameter</param>
        public static void BuildLevelPerPackage(List<Category> ParsedCategoryList, int startingLevel = 0)
        {
            //root level direct form category is 0
            foreach (Category cat in ParsedCategoryList)
            {
                foreach (SelectablePackage package in cat.Packages)
                {
                    package.Level = startingLevel;
                    if (package.Packages.Count > 0)
                        //increase the level BEFORE it calls the method
                        BuildLevelPerPackage(package.Packages, startingLevel + 1);
                }
            }
        }

        /// <summary>
        /// Assigns the level parameter to the packages based on how recursively deep they are in the package sub lists
        /// </summary>
        /// <param name="packages">The list of package values to</param>
        /// <param name="level">The level to assign the level parameter</param>
        private static void BuildLevelPerPackage(List<SelectablePackage> packages, int level)
        {
            foreach (SelectablePackage package in packages)
            {
                package.Level = level;
                if (package.Packages.Count > 0)
                    //increase the level BEFORE it calls the method
                    BuildLevelPerPackage(package.Packages, level + 1);
            }
        }

        /// <summary>
        /// Links the databasePackage objects with dependencies objects to have those objects link references to the parent and the dependency object
        /// </summary>
        /// <param name="componentsWithDependencies">List of all DatabasePackage objects that have dependencies</param>
        /// <param name="dependencies">List of all Dependencies that exist in the database</param>
        public static void BuildDependencyPackageRefrences(List<Category> componentsWithDependencies, List<Dependency> dependencies)
        {
            List<IComponentWithDependencies> componentsWithDependencies_ = new List<IComponentWithDependencies>();

            //get all categories where at least one dependency exists
            componentsWithDependencies_.AddRange(componentsWithDependencies.Where(cat => cat.Dependencies.Count > 0));

            //get all packages and dependencies where at least one dependency exists
            componentsWithDependencies_.AddRange(GetFlatList(null, dependencies, componentsWithDependencies).OfType<IComponentWithDependencies>().Where(component => component.Dependencies.Count > 0).ToList());

            foreach (IComponentWithDependencies componentWithDependencies in componentsWithDependencies_)
            {
                foreach (DatabaseLogic logic in componentWithDependencies.Dependencies)
                {
                    logic.ParentPackageRefrence = componentWithDependencies;
                    logic.DependencyPackageRefrence = dependencies.Find(dependency => dependency.PackageName.Equals(logic.PackageName));
                    if (logic.DependencyPackageRefrence == null)
                    {
                        Logging.Error("DatabaseLogic component from package {0} was unable to link to dependency {1} (does the dependency not exist or bad reference?)", componentWithDependencies.ComponentInternalName, logic.PackageName);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates which packages and dependencies are dependent on other dependencies and if each dependency that is selected for install is enabled for installation
        /// </summary>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <param name="suppressSomeLogging">Flag for it some of the more verbose logging should be suppressed</param>
        /// <returns>A list of calculated dependencies to install</returns>
        public static List<Dependency> CalculateDependencies(List<Dependency> dependencies, List<Category> parsedCategoryList, bool suppressSomeLogging)
        {
            //flat list is packages
            List<SelectablePackage> flatListSelect = GetFlatSelectablePackageList(parsedCategoryList);

            //1- build the list of calling mods that need it
            List<Dependency> dependenciesToInstall = new List<Dependency>();

            //create list to track all database dependency references
            List<LogicTracking> refrencedDependencies = new List<LogicTracking>();

            Logging.Debug("Starting step 1 of 4 in dependency calculation: adding from categories");
            foreach (Category category in parsedCategoryList)
            {
                foreach (DatabaseLogic logic in category.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = category
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Category \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                category.Name, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                PackageName = category.Name,
                                WillBeInstalled = category.AnyPackagesChecked(),
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 1 complete");

            Logging.Debug("Starting step 2 of 4 in dependency calculation: adding from selectable packages that use each dependency");
            foreach (SelectablePackage package in flatListSelect)
            {
                //got though each logic property. if the package called is this dependency, then add it to it's list
                foreach (DatabaseLogic logic in package.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = package
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("SelectablePackage \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                package.PackageName, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                //set PackageName to the selectablepackage package name so later we know where this logic entry came from
                                PackageName = package.PackageName,
                                WillBeInstalled = package.Checked,
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 2 complete");


            Logging.Debug("Starting step 3 of 4 in dependency calculation: adding dependencies that use each dependency");
            //for each dependency go through each dependency's package logic and if it's called then add it
            foreach (Dependency processingDependency in dependencies)
            {
                foreach (DatabaseLogic logic in processingDependency.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = processingDependency
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (processingDependency.PackageName.Equals(dependency.PackageName))
                            continue;
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Dependency \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                processingDependency.PackageName, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                PackageName = processingDependency.PackageName,
                                //by default, dependences that are dependent on dependencies start as false until proven needed
                                WillBeInstalled = false,
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 3 complete");

            //3a - check if any dependency references were never matched
            //like if a category references dependency the_dependency_packageName, but that package does not exist
            refrencedDependencies = refrencedDependencies.Where((refrence) => !refrence.DatabaseLogic.RefrenceLinked).ToList();
            Logging.Debug("Broken dependency references count: {0}", refrencedDependencies.Count);
            if (refrencedDependencies.Count > 0)
            {
                Logging.Error("The following packages call references to dependencies that do not exist:");
                foreach (LogicTracking logicTracking in refrencedDependencies)
                {
                    Logging.Error("Package: {0} => broken reference: {1}",
                        logicTracking.ComponentWithDependencies.ComponentInternalName, logicTracking.DatabaseLogic.PackageName);
                }
            }

            //4 - run calculations IN DEPENDENCY LIST ORDER FROM TOP DOWN
            List<Dependency> notProcessedDependnecies = new List<Dependency>(dependencies);
            Logging.Debug("Starting step 4 of 4 in dependency calculation: calculating dependencies from top down (perspective to list)");
            int calcNumber = 1;
            foreach (Dependency dependency in dependencies)
            {
                //first check if this dependency is referencing a dependency that has not yet been processed
                //if so then note it in the log
                if (!suppressSomeLogging)
                    Logging.Debug(string.Empty);
                if (!suppressSomeLogging)
                    Logging.Debug("Calculating if dependency {0} will be installed, {1} of {2}", dependency.PackageName, calcNumber++, dependencies.Count);

                foreach (DatabaseLogic login in dependency.DatabasePackageLogic)
                {
                    List<Dependency> matches = notProcessedDependnecies.Where(dep => login.PackageName.Equals(dep.PackageName)).ToList();
                    if (matches.Count > 0)
                    {
                        string errorMessage = string.Format("Dependency {0} is referenced by the dependency {1} which has not yet been processed! " +
                            "This will lead to logic errors in database calculation! Tip: this dependency ({0}) should be BELOW ({1}) in the" +
                            "list of dependencies in the editor. Order matters!", dependency.PackageName, login.PackageName);
                        Logging.Error(errorMessage);
                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                            MessageBox.Show(errorMessage);
                    }
                }

                //two types of logics - OR and AND (with NOT flags)
                //each can be calculated separately
                List<DatabaseLogic> localOR = dependency.DatabasePackageLogic.Where(logic => logic.Logic == Logic.OR).ToList();
                List<DatabaseLogic> logicalAND = dependency.DatabasePackageLogic.Where(logic => logic.Logic == Logic.AND).ToList();

                //debug logging
                if (!suppressSomeLogging)
                    Logging.Debug("Logical OR count: {0}", localOR.Count);
                if (!suppressSomeLogging)
                    Logging.Debug("Logical AND count: {0}", logicalAND.Count);

                //if there are no logical ands, then only do ors, vise versa
                bool ORsPass = localOR.Count > 0 ? false : true;
                bool ANDSPass = logicalAND.Count > 0 ? false : true;

                //if ors and ands are both true already, then something's broken
                if (ORsPass && ANDSPass)
                {
                    Logging.Warning("Logic ORs and ANDs already pass for dependency package {0} (nothing uses it?)", dependency.PackageName);
                    if (!suppressSomeLogging)
                        Logging.Debug("Skip calculation logic and remove from not processed list");

                    //remove it from list of not processed dependencies
                    notProcessedDependnecies.RemoveAt(0);
                    continue;
                }

                //calc the ORs first
                if (!suppressSomeLogging)
                    Logging.Debug("Processing OR logic");
                foreach (DatabaseLogic orLogic in localOR)
                {
                    //OR logic - if any mod/dependency is checked, then it's installed and can stop there
                    //because only one of them needs to be true
                    //same case goes for negatives - if mod is NOT checked and negateFlag
                    if (!orLogic.WillBeInstalled)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Skipping logic check of package {0} because it is not set for installation!", orLogic.PackageName);
                        continue;
                    }
                    else
                    {
                        if (!orLogic.NotFlag)
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, is checked and notFlag is false (package must be checked), sets orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                            ORsPass = true;
                            break;
                        }
                        else if (orLogic.NotFlag)
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, is NOT checked and notFlag is true (package must NOT be checked), sets orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                            ORsPass = true;
                            break;
                        }
                        else
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, does not set orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                        }
                    }
                }

                //now calc the ands
                if (!suppressSomeLogging)
                    Logging.Debug("Processing AND logic");
                foreach (DatabaseLogic andLogic in logicalAND)
                {
                    if (andLogic.WillBeInstalled && !andLogic.NotFlag)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, is checked and notFlag is false (package must be checked), correct AND logic, continue", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = true;
                    }
                    else if (!andLogic.WillBeInstalled && andLogic.NotFlag)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, is NOT checked and notFlag is true (package must NOT be checked), correct AND logic, continue", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = true;
                    }
                    else
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, incorrect AND logic, set ANDSPass=false and stop processing!", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = false;
                        break;
                    }
                }

                string final = string.Format("Final result for dependency {0}: AND={1}, OR={2}", dependency.PackageName, ANDSPass, ORsPass);
                if (ANDSPass && ORsPass)
                {
                    if (suppressSomeLogging)
                        Logging.Info(LogOptions.MethodAndClassName, "Dependency {0} WILL be installed!", dependency.PackageName);
                    else
                        Logging.Debug("{0} (AND and OR) = TRUE, dependency WILL be installed!", final);
                    dependenciesToInstall.Add(dependency);
                }
                else
                {
                    if (!suppressSomeLogging)
                        Logging.Debug("{0} (AND and OR) = FALSE, dependency WILL NOT be installed!", final);
                }

                if (dependency.DatabasePackageLogic.Count > 0 && (ANDSPass && ORsPass))
                {
                    if (!suppressSomeLogging)
                        Logging.Debug("Updating future references (like logicalDependnecies) for if dependency was checked");
                    //update any dependencies that use it
                    foreach (DatabaseLogic callingLogic in dependency.Dependencies)
                    {
                        //get the dependency (if it is a dependency) that called this dependency
                        List<Dependency> found = dependencies.Where(dep => dep.PackageName.Equals(callingLogic.PackageName)).ToList();

                        if (found.Count > 0)
                        {
                            Dependency refrenced = found[0];
                            //now get the logic entry that references the original calculated dependency
                            List<DatabaseLogic> foundLogic = refrenced.DatabasePackageLogic.Where(logic => logic.PackageName.Equals(dependency.PackageName)).ToList();
                            if (foundLogic.Count > 0)
                            {
                                Logging.Debug("Logic reference entry for dependency {0} updated to {1}", refrenced.PackageName, ANDSPass && ORsPass);
                                foundLogic[0].WillBeInstalled = ANDSPass && ORsPass;
                            }
                            else
                            {
                                Logging.Error("Found logics count is 0 for updating references");
                            }
                        }
                        else
                        {
                            Logging.Error("Found count is 0 for updating references");
                        }
                    }
                }

                //remove it from list of not processed dependencies
                notProcessedDependnecies.RemoveAt(0);
            }

            Logging.Debug("Step 4 complete");
            return dependenciesToInstall;
        }

        /// <summary>
        /// Creates an array of DatabasePackage lists sorted by Installation groups i.e. list in array index 0 is packages of install group 0
        /// </summary>
        /// <param name="packagesToInstall"></param>
        /// <returns>The array of DatabasePackage lists</returns>
        public static List<DatabasePackage>[] CreateOrderedInstallList(List<DatabasePackage> packagesToInstall)
        {
            //get the max number of defined groups
            int maxGrops = packagesToInstall.Select(max => max.InstallGroupWithOffset).Max();

            //make the list to return
            //make it maxGroups +1 because group 4 exists, but making a array of 4 is 0-3
            List<DatabasePackage>[] orderedList = new List<DatabasePackage>[maxGrops + 1];

            //new up the lists
            for (int i = 0; i < orderedList.Count(); i++)
                orderedList[i] = new List<DatabasePackage>();

            foreach (DatabasePackage package in packagesToInstall)
            {
                orderedList[package.InstallGroupWithOffset].Add(package);
            }
            return orderedList;
        }

        /// <summary>
        /// Gets the maximum InstallGroup number from a list of Packages
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum InstallGroup number</returns>
        public static int GetMaxInstallGroupNumber(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.InstallGroup);
        }

        /// <summary>
        /// Gets the maximum InstallGroup number from a list of Packages factoring in the offset that a category may apply to it
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum InstallGroup number</returns>
        public static int GetMaxInstallGroupNumberWithOffset(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.InstallGroupWithOffset);
        }

        /// <summary>
        /// Gets the maximum PatchGroup number from a list of Packages
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum PatchGroup number</returns>
        public static int GetMaxPatchGroupNumber(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.PatchGroup);
        }
        #endregion
    }
}
