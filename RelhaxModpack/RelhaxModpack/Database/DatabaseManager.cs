using RelhaxModpack.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RelhaxModpack.Utilities.Enums;
using Ionic.Zip;
using System.Net;
using RelhaxModpack.Utilities;
using System.IO;
using RelhaxModpack.Common;
using System.Windows;
using RelhaxModpack.Xml;
using System.Collections;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack.Database
{
    public class DatabaseManager
    {
        public const string DocumentVersion1V1 = "1.1";

        public const string DocumentVersion1V2 = "1.2";

        public const string WoTClientVersionXmlString = "version";

        public const string WoTOnlineFolderVersionXmlString = "onlineFolder";

        public const string DocumentVersionXmlString = "documentVersion";

        public const string SchemaVersionXmlString = "schemaVersion";

        /// <summary>
        /// The list of categories
        /// </summary>
        public List<Category> ParsedCategoryList { get; private set; }

        /// <summary>
        /// The list of global dependencies
        /// </summary>
        public List<DatabasePackage> GlobalDependencies { get; private set; }

        /// <summary>
        /// The list of dependencies
        /// </summary>
        public List<Dependency> Dependencies { get; private set; }

        public XmlDocument DatabaseRootXmlDocument { get; private set; }

        public string WoTClientVersion { get; private set; }

        public string WoTOnlineFolderVersion { get; private set; }

        public string DocumentVersion { get; private set; }

        public string SchemaVersion { get; private set; }

        /// <summary>
        /// A reference to the modpack settings window configuration class
        /// </summary>
        public ModpackSettings ModpackSettings { get; set; }

        public CommandLineSettings CommandLineSettings { get; set; }

        public ZipFile ManagerInfoZipfile { get; set; }

        private string databaseRootXmlString;

        private ZipFile modInfoZipFile;

        //options set for each public entrypoint
        private string lastSupportedWoTClient;

        private string CustomDatabaseLocation;

        private string BetaDatabaseBranch;

        public DatabaseVersions DatabaseDistroToLoad { get; private set; }

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public DatabaseManager(ModpackSettings modpackSettings)
        {
            this.ModpackSettings = modpackSettings;
        }

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public DatabaseManager(ModpackSettings modpackSettings, CommandLineSettings commandLineSettings)
        {
            this.ModpackSettings = modpackSettings;
            this.CommandLineSettings = commandLineSettings;
        }

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public DatabaseManager()
        {
            
        }

        #region Load database methods
        public async Task<DatabaseLoadFailCode> LoadDatabaseAsync(DatabaseVersions databaseDistroToLoad, string locationToLoadFrom, string betaDatabaseBranch)
        {
            if (ModpackSettings == null)
                throw new NullReferenceException();

            this.DatabaseDistroToLoad = databaseDistroToLoad;
            this.CustomDatabaseLocation = locationToLoadFrom;
            this.BetaDatabaseBranch = betaDatabaseBranch;

            return await LoadDatabaseWithOptionsAsync();
        }

        /// <summary>
        /// Load a test database from the specified location, including the root filename
        /// </summary>
        /// <param name="locationToLoadFrom">The path to the root xml file</param>
        /// <returns>The status enumeration code of the operation</returns>
        public async Task<DatabaseLoadFailCode> LoadDatabaseTestAsync(string locationToLoadFrom)
        {
            //this version of the overloaded method, it's implied that we're loading in test mode
            this.DatabaseDistroToLoad = DatabaseVersions.Test;
            this.CustomDatabaseLocation = locationToLoadFrom;
            this.BetaDatabaseBranch = null;

            return await LoadDatabaseWithOptionsAsync();
        }

        public async Task<DatabaseLoadFailCode> LoadDatabaseDistroAsync(DatabaseVersions databaseDistroToLoad)
        {
            if (ModpackSettings == null)
                throw new NullReferenceException();

            this.DatabaseDistroToLoad = databaseDistroToLoad;
            this.CustomDatabaseLocation = ModpackSettings.CustomModInfoPath;
            this.BetaDatabaseBranch = ModpackSettings.BetaDatabaseSelectedBranch;

            return await LoadDatabaseWithOptionsAsync();
        }

        public async Task<DatabaseLoadFailCode> LoadDatabaseAsync()
        {
            if (ModpackSettings == null)
                throw new NullReferenceException();
            if (CommandLineSettings == null)
                throw new NullReferenceException();

            this.DatabaseDistroToLoad = ModpackSettings.DatabaseDistroVersion;
            this.CustomDatabaseLocation = ModpackSettings.CustomModInfoPath;
            this.BetaDatabaseBranch = ModpackSettings.BetaDatabaseSelectedBranch;

            if (CommandLineSettings.TestMode)
                this.DatabaseDistroToLoad = DatabaseVersions.Test;

            return await LoadDatabaseWithOptionsAsync();
        }

        private async Task<DatabaseLoadFailCode> LoadDatabaseWithOptionsAsync()
        {
            if (this.DatabaseDistroToLoad == DatabaseVersions.Test && string.IsNullOrEmpty(this.CustomDatabaseLocation))
                throw new NullReferenceException();

            Init();

            DatabaseLoadFailCode databaseLoadFailCode;

            //run the stuff. the method names are self-explanatory
            databaseLoadFailCode = await LoadRootDatabaseXmlStringAsync();
            if (databaseLoadFailCode != DatabaseLoadFailCode.None)
                return databaseLoadFailCode;

            databaseLoadFailCode = ParseRootDatabaseFromString();
            if (databaseLoadFailCode != DatabaseLoadFailCode.None)
                return databaseLoadFailCode;

            LoadMetadataInfoFromRootXmlDocument();

            databaseLoadFailCode = await ParseDatabaseXmlAsync();
            if (databaseLoadFailCode != DatabaseLoadFailCode.None)
                return databaseLoadFailCode;

            DatabaseUtils.BuildTopLevelParents(ParsedCategoryList);
            DatabaseUtils.BuildLinksRefrence(ParsedCategoryList);
            DatabaseUtils.BuildLevelPerPackage(ParsedCategoryList);

            return DatabaseLoadFailCode.None;
        }

        public async Task<DatabaseLoadFailCode> LoadDatabaseStableSpecificClientAsync(string clientVersion)
        {
            if (string.IsNullOrEmpty(clientVersion))
                throw new NullReferenceException();
            Init();
            lastSupportedWoTClient = clientVersion;
            await LoadRootDatabaseXmlStringStableAsync();
            ParseRootDatabaseFromString();
            LoadMetadataInfoFromRootXmlDocument();
            ParseDatabaseXmlStable();
            DatabaseUtils.BuildTopLevelParents(ParsedCategoryList);
            DatabaseUtils.BuildLinksRefrence(ParsedCategoryList);
            DatabaseUtils.BuildLevelPerPackage(ParsedCategoryList);
            return DatabaseLoadFailCode.None;
        }
        #endregion

        #region Initializations
        private void Init()
        {
            if (GlobalDependencies == null)
                GlobalDependencies = new List<DatabasePackage>();
            else
                GlobalDependencies.Clear();

            if (Dependencies == null)
                Dependencies = new List<Dependency>();
            else
                Dependencies.Clear();

            if (ParsedCategoryList == null)
                ParsedCategoryList = new List<Category>();
            else
                ParsedCategoryList.Clear();

            databaseRootXmlString = null;

            if (modInfoZipFile != null)
                modInfoZipFile.Dispose();
            modInfoZipFile = null;
        }
        #endregion

        #region Loading the database root file into a string
        private async Task<DatabaseLoadFailCode> LoadRootDatabaseXmlStringAsync()
        {
            //get the Xml database loaded into a string based on database version type (from server download, from github, from testfile
            databaseRootXmlString = string.Empty;
            switch (DatabaseDistroToLoad)
            {
                //from server download
                case DatabaseVersions.Stable:
                    await GetLastSupportedWoTClientAsync();
                    await LoadRootDatabaseXmlStringStableAsync();
                    break;

                //from github
                case DatabaseVersions.Beta:
                    await LoadRootDatabaseXmlStringBetaAsync();
                    break;

                //from testfile
                case DatabaseVersions.Test:
                    LoadRootDatabaseXmlStringTest();
                    break;
            }

            //check to make sure the xml string is valid
            if (string.IsNullOrWhiteSpace(databaseRootXmlString))
            {
                Logging.Error("Failed to load {0} into string", ApplicationConstants.BetaDatabaseV2RootFilename);
                if (DatabaseDistroToLoad == DatabaseVersions.Test)
                    return DatabaseLoadFailCode.FailedToLoadXmlFromDisk;
                else
                    return DatabaseLoadFailCode.FailedToExtractXmlFromZipFile;
            }

            return DatabaseLoadFailCode.None;
        }

        private async Task<DatabaseLoadFailCode> GetLastSupportedWoTClientAsync()
        {
            if (ManagerInfoZipfile == null)
            {
                //download the manager info zip file
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        byte[] managerInfoByteArray = await client.DownloadDataTaskAsync(ApplicationConstants.ManagerInfoURLBigmods);
                        ManagerInfoZipfile = ZipFile.Read(new MemoryStream(managerInfoByteArray));
                    }
                    catch (Exception ex)
                    {
                        Logging.Exception("Failed to download managerInfo to memory stream");
                        Logging.Exception(ex.ToString());
                        return DatabaseLoadFailCode.FailedToDownloadZipFile;
                    }
                }
            }

            //extract supported_clients xml from the manager info file
            string supportedClientsXmlString = FileUtils.GetStringFromZip(ManagerInfoZipfile, ApplicationConstants.SupportedClients);
            if (string.IsNullOrWhiteSpace(supportedClientsXmlString))
            {
                Logging.Info("Failed to extract supported_clients.xml from string from zipfile", Logfiles.Application, LogLevel.Exception);
                return DatabaseLoadFailCode.FailedToExtractXmlFromZipFile;
            }

            XmlDocument doc = XmlUtils.LoadXmlDocument(supportedClientsXmlString, XmlLoadType.FromString);
            if (doc == null)
            {
                Logging.Error("Failed to parse supported_clients.xml into xml document");
                return DatabaseLoadFailCode.FailedToParseXml;
            }

            //parse the list of supported client xml nodes
            XmlNodeList supportedVersionsXml = XmlUtils.GetXmlNodesFromXPath(doc, "//versions/version");
            XmlNode lastSupportedVersionXml = supportedVersionsXml[supportedVersionsXml.Count - 1];
            lastSupportedWoTClient = lastSupportedVersionXml.InnerText.Trim();
            Logging.Info(LogOptions.ClassName, "Parsed last supported WoT client as {0}", lastSupportedWoTClient);
            return DatabaseLoadFailCode.None;
        }

        private async Task<DatabaseLoadFailCode> LoadRootDatabaseXmlStringStableAsync()
        {
            //make string download url
            string modInfoXmlUrlStable = ApplicationConstants.BigmodsDatabaseRootEscaped.Replace(@"{dbVersion}", lastSupportedWoTClient) + ApplicationConstants.ModInfoZip;

            //download latest modInfo xml
            try
            {
                using (WebClient client = new WebClient())
                {
                    //save zip file into memory
                    Logging.Debug("Download {0} from {1}", ApplicationConstants.ModInfoZip, modInfoXmlUrlStable);
                    modInfoZipFile = ZipFile.Read(new MemoryStream(await client.DownloadDataTaskAsync(modInfoXmlUrlStable)));
                }
            }
            catch (Exception ex)
            {
                Logging.Exception("Failed to download {0}", ApplicationConstants.ModInfoZip);
                Logging.Exception(ex.ToString());
                return DatabaseLoadFailCode.FailedToDownloadZipFile;
            }

            //extract modinfo xml string
            databaseRootXmlString = FileUtils.GetStringFromZip(modInfoZipFile, ApplicationConstants.BetaDatabaseV2RootFilename);

            return DatabaseLoadFailCode.None;
        }

        private async Task<DatabaseLoadFailCode> LoadRootDatabaseXmlStringBetaAsync()
        {
            using (WebClient client = new WebClient())
            {
                //load string constant url from manager info xml
                string modInfoXmlUrlBeta = ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", BetaDatabaseBranch) + ApplicationConstants.BetaDatabaseV2RootFilename;
                Logging.Debug("Download beta database from {0}", modInfoXmlUrlBeta);

                //download the xml string into "modInfoXml"
                client.Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                databaseRootXmlString = await client.DownloadStringTaskAsync(modInfoXmlUrlBeta);
            }

            return DatabaseLoadFailCode.None;
        }

        private DatabaseLoadFailCode LoadRootDatabaseXmlStringTest()
        {
            //make string
            if (!File.Exists(CustomDatabaseLocation))
            {
                Logging.Error("The test database root file at {0} does not exist!", CustomDatabaseLocation);
                return DatabaseLoadFailCode.FailedToLoadXmlFromDisk;
            }

            //load modinfo xml into string
            databaseRootXmlString = File.ReadAllText(CustomDatabaseLocation);
            return DatabaseLoadFailCode.None;
        }
        #endregion

        #region Parse the root xml document into xml object
        public DatabaseLoadFailCode ParseRootDatabaseXmlFromString(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString))
                throw new NullReferenceException();

            databaseRootXmlString = xmlString;
            return ParseRootDatabaseFromString();
        }

        private DatabaseLoadFailCode ParseRootDatabaseFromString()
        {
            //load the xml document into xml object
            DatabaseRootXmlDocument = XmlUtils.LoadXmlDocument(databaseRootXmlString, XmlLoadType.FromString);
            if (DatabaseRootXmlDocument == null)
            {
                Logging.Error("Failed to parse {0} from xml string", ApplicationConstants.BetaDatabaseV2RootFilename);
                return DatabaseLoadFailCode.FailedToParseXml;
            }
            return DatabaseLoadFailCode.None;
        }
        #endregion

        #region Load WoT online folder and version from root document metadata
        public async Task<DatabaseLoadFailCode> LoadWoTVersionInfoFromXmlDocumentAsync(DatabaseVersions databaseDistroToLoad, string locationToLoadFrom)
        {
            if (databaseDistroToLoad == DatabaseVersions.Test && string.IsNullOrEmpty(locationToLoadFrom))
                throw new NullReferenceException();

            //setup from DatabaseManager options
            this.DatabaseDistroToLoad = databaseDistroToLoad;
            this.CustomDatabaseLocation = locationToLoadFrom;

            //run the stuff. the method names are self-explanatory
            await LoadRootDatabaseXmlStringAsync();

            ParseRootDatabaseFromString();

            LoadMetadataInfoFromRootXmlDocument();

            return DatabaseLoadFailCode.None;
        }

        private void LoadMetadataInfoFromRootXmlDocument()
        {
            //get WoT xml version attributes from database root
            WoTOnlineFolderVersion = XmlUtils.GetXmlStringFromXPath(DatabaseRootXmlDocument, ApplicationConstants.DatabaseOnlineFolderXpath);
            WoTClientVersion = XmlUtils.GetXmlStringFromXPath(DatabaseRootXmlDocument, ApplicationConstants.DatabaseClientVersionXpath);
            DocumentVersion = XmlUtils.GetXmlStringFromXPath(DatabaseRootXmlDocument, ApplicationConstants.DatabaseDocumentVersionXpath);
            SchemaVersion = XmlUtils.GetXmlStringFromXPath(DatabaseRootXmlDocument, ApplicationConstants.DatabaseSchemaVersionXpath);
        }
        #endregion

        #region Parsing database from xml into memory
        public async Task<List<string>> GetBetaDatabase1V1FilesListAsync()
        {
            this.DatabaseDistroToLoad = DatabaseVersions.Beta;
            this.BetaDatabaseBranch = ModpackSettings.BetaDatabaseSelectedBranch;
            await LoadRootDatabaseXmlStringAsync();
            ParseRootDatabaseFromString();
            return GetBetaDatabase1V1FilesList(DatabaseRootXmlDocument, ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", this.BetaDatabaseBranch));
        }

        private async Task<DatabaseLoadFailCode> ParseDatabaseXmlAsync()
        {
            //parse the modInfoXml to list in memory
            switch (DatabaseDistroToLoad)
            {
                case DatabaseVersions.Stable:
                    ParseDatabaseXmlStable();
                    break;
                //github
                case DatabaseVersions.Beta:
                    await ParseDatabaseXmlBetaAsync();
                    break;
                //test
                case DatabaseVersions.Test:
                    ParseDatabaseXmlTest();
                    break;
            }

            return DatabaseLoadFailCode.None;
        }

        private DatabaseLoadFailCode ParseDatabaseXmlStable()
        {
            Logging.Debug("Getting xml string values from zip file");
            List<string> categoriesXml = new List<string>();

            string globalDependencyFilename = XmlUtils.GetXmlStringFromXPath(DatabaseRootXmlDocument, "/modInfoAlpha.xml/globalDependencies/@file");
            Logging.Debug("Found xml entry: {0}", globalDependencyFilename);
            string globalDependencyXmlString = FileUtils.GetStringFromZip(modInfoZipFile, globalDependencyFilename);

            string dependencyFilename = XmlUtils.GetXmlStringFromXPath(DatabaseRootXmlDocument, "/modInfoAlpha.xml/dependencies/@file");
            Logging.Debug("Found xml entry: {0}", dependencyFilename);
            string dependenicesXmlString = FileUtils.GetStringFromZip(modInfoZipFile, dependencyFilename);

            foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(DatabaseRootXmlDocument, "//modInfoAlpha.xml/categories/category"))
            {
                string categoryFilename = categoryNode.Attributes["file"].Value;
                Logging.Debug("Found xml entry: {0}", categoryFilename);
                categoriesXml.Add(FileUtils.GetStringFromZip(modInfoZipFile, categoryFilename));
            }
            modInfoZipFile.Dispose();
            modInfoZipFile = null;

            //parse into lists
            switch (DocumentVersion)
            {
                case DocumentVersion1V1:
                case DocumentVersion1V2:
                    if (!ParseDatabase1V1FromStrings(globalDependencyXmlString, dependenicesXmlString, categoriesXml, GlobalDependencies, Dependencies, ParsedCategoryList))
                    {
                        Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                        return DatabaseLoadFailCode.FailedToParseDatabase;
                    }
                    break;
                default:
                    Logging.Error("Unknown document version to load: {0}", DocumentVersion);
                    return DatabaseLoadFailCode.FailedToParseDatabase;
            }
            
            return DatabaseLoadFailCode.None;
        }

        private async Task<DatabaseLoadFailCode> ParseDatabaseXmlBetaAsync()
        {
            Logging.Debug("Init beta db download resources");
            //create download url list
            List<string> downloadURLs = GetBetaDatabase1V1FilesList(DatabaseRootXmlDocument, ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", BetaDatabaseBranch));

            string[] downloadStrings = await CommonUtils.DownloadStringsFromUrlsAsync(downloadURLs);

            //parse into strings
            Logging.Debug("Tasks finished, extracting task results");
            string globalDependencyXmlStringBeta = downloadStrings[0];
            string dependenicesXmlStringBeta = downloadStrings[1];

            List<string> categoriesXmlBeta = new List<string>();
            for (int i = 2; i < downloadURLs.Count; i++)
            {
                categoriesXmlBeta.Add(downloadStrings[i]);
            }

            //parse into lists
            switch (DocumentVersion)
            {
                case DocumentVersion1V1:
                case DocumentVersion1V2:
                    if (!ParseDatabase1V1FromStrings(globalDependencyXmlStringBeta, dependenicesXmlStringBeta, categoriesXmlBeta, GlobalDependencies, Dependencies, ParsedCategoryList))
                    {
                        Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                        return DatabaseLoadFailCode.FailedToParseDatabase;
                    }
                    break;
                default:
                    Logging.Error("Unknown document version to load: {0}", DocumentVersion);
                    return DatabaseLoadFailCode.FailedToParseDatabase;
            }
            return DatabaseLoadFailCode.None;
        }

        private DatabaseLoadFailCode ParseDatabaseXmlTest()
        {
            //parse into lists
            switch (DocumentVersion)
            {
                case DocumentVersion1V1:
                case DocumentVersion1V2:
                    if (!ParseDatabase1V1FromFiles())
                    {
                        Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                        return DatabaseLoadFailCode.FailedToParseDatabase;
                    }
                    break;
                default:
                    Logging.Error("Unknown document version to load: {0}", DocumentVersion);
                    return DatabaseLoadFailCode.FailedToParseDatabase;
            }
            return DatabaseLoadFailCode.None;
        }

        private List<string> GetBetaDatabase1V1FilesList(XmlDocument rootDocument, string betaDatabaseV2FolderUrlParsed)
        {
            //global and logical dependencies
            List<string> databaseFiles = new List<string>()
            {
                betaDatabaseV2FolderUrlParsed + XmlUtils.GetXmlStringFromXPath(rootDocument, "/modInfoAlpha.xml/globalDependencies/@file"),
                betaDatabaseV2FolderUrlParsed + XmlUtils.GetXmlStringFromXPath(rootDocument, "/modInfoAlpha.xml/dependencies/@file")
            };

            //categories
            foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(rootDocument, "//modInfoAlpha.xml/categories/category"))
            {
                string categoryFileName = categoryNode.Attributes["file"].Value;
                databaseFiles.Add(betaDatabaseV2FolderUrlParsed + categoryFileName);
            }

            return databaseFiles.Select(name => name.Replace(".Xml", ".xml")).ToList();
        }
        #endregion

        #region Database Loading
        /// <summary>
        /// Parse a database into the version 1.1 format from files on the disk
        /// </summary>
        private bool ParseDatabase1V1FromFiles()
        {
            //load each document to make sure they all exist first
            if (string.IsNullOrWhiteSpace(CustomDatabaseLocation))
                throw new NullReferenceException();

            if (Path.HasExtension(CustomDatabaseLocation))
                CustomDatabaseLocation = Path.GetDirectoryName(CustomDatabaseLocation);

            //document for global dependencies
            string completeFilepath = Path.Combine(CustomDatabaseLocation, XmlUtils.GetXmlStringFromXPath(DatabaseRootXmlDocument, "/modInfoAlpha.xml/globalDependencies/@file"));
            if (!File.Exists(completeFilepath))
            {
                Logging.Error("{0} file does not exist at {1}", "Global Dependency", completeFilepath);
                return false;
            }
            XDocument globalDepsDoc = XmlUtils.LoadXDocument(completeFilepath, XmlLoadType.FromFile);
            if (globalDepsDoc == null)
                throw new BadMemeException("this should not be null");

            //document for dependencies
            completeFilepath = Path.Combine(CustomDatabaseLocation, XmlUtils.GetXmlStringFromXPath(DatabaseRootXmlDocument, "/modInfoAlpha.xml/dependencies/@file"));
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
            foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(DatabaseRootXmlDocument, "//modInfoAlpha.xml/categories/category"))
            {
                //make string path
                completeFilepath = Path.Combine(CustomDatabaseLocation, categoryNode.Attributes["file"].Value);

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
            return ParseDatabase(globalDepsDoc, GlobalDependencies, depsDoc, Dependencies, categoryDocuments, ParsedCategoryList);
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
        public bool ParseDatabase1V1FromStrings(string globalDependenciesXml, string dependneciesXml, List<string> categoriesXml,
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

            return ParseDatabase(globalDependenciesdoc, globalDependencies, dependenciesdoc, dependencies, categoryDocuments, parsedCategoryList);
        }
        public bool ParseDatabase(XDocument globalDependenciesDoc, List<DatabasePackage> globalDependenciesList, XDocument dependenciesDoc,
            List<Dependency> dependenciesList, List<XDocument> categoryDocuments, List<Category> parsedCategoryList)
        {
            switch(DocumentVersion)
            {
                case DocumentVersion1V1:
                    return ParseDatabase1V1(globalDependenciesDoc, globalDependenciesList, dependenciesDoc, dependenciesList, categoryDocuments, parsedCategoryList);
                case DocumentVersion1V2:
                    return ParseDatabase1V2(globalDependenciesDoc, globalDependenciesList, dependenciesDoc, dependenciesList, categoryDocuments, parsedCategoryList);
                default:
                    Logging.Error("Unknown document version to parse: {0}", DocumentVersion);
                    return false;
            }
        }

        public bool ParseDatabase1V2(XDocument globalDependenciesDoc, List<DatabasePackage> globalDependenciesList, XDocument dependenciesDoc,
            List<Dependency> dependenciesList, List<XDocument> categoryDocuments, List<Category> parsedCategoryList)
        {
            //parsing the global dependencies
            Logging.Debug("[ParseDatabase1V2]: Parsing GlobalDependencies");
            bool globalParsed = ParseDatabase1V2Packages(globalDependenciesDoc.XPathSelectElements("/GlobalDependencies/GlobalDependency").ToList(), globalDependenciesList);

            //parsing the logical dependnecies
            Logging.Debug("[ParseDatabase1V2]: Parsing Dependencies");
            bool depsParsed = ParseDatabase1V2Packages(dependenciesDoc.XPathSelectElements("/Dependencies/Dependency").ToList(), dependenciesList);

            //parsing the categories
            bool categoriesParsed = true;
            for (int i = 0; i < categoryDocuments.Count; i++)
            {
                XElement categoryXml = categoryDocuments[i].XPathSelectElement(@"/Category");
                Category category = parsedCategoryList[i];

                //parse the list of packages
                Logging.Debug("[ParseDatabase1V2]: Parsing Packages for category {0}", i);
                if (!ParseDatabase1V2Package(categoryXml, category, category.GetType()))
                {
                    categoriesParsed = false;
                }
                parsedCategoryList.Add(category);
            }
            return globalParsed && depsParsed && categoriesParsed;
        }

        private bool ParseDatabase1V2Packages(List<XElement> xmlPackageNodesList, IList genericPackageList)
        {
            bool parsed = true;

            //get the type of element in the list
            Type listObjectType = genericPackageList.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1).FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];

            //for each Xml package entry
            int index = 0;
            foreach (XElement xmlPackageNode in xmlPackageNodesList)
            {
                XmlDatabaseComponent component = genericPackageList[index] as XmlDatabaseComponent;

                if (!ParseDatabase1V2Package(xmlPackageNode, component, listObjectType))
                    parsed = false;
            }
            return parsed;
        }

        private bool ParseDatabase1V2Package(XElement xmlPackageNode, XmlDatabaseComponent component, Type componentType)
        {
            if (xmlPackageNode == null)
                throw new ArgumentNullException(nameof(xmlPackageNode));

            if (component == null)
            {
                //make sure object type is properly implemented into serialization system
                if (!(Activator.CreateInstance(componentType) is XmlDatabaseComponent _component))
                    throw new BadMemeException("Type of this list is not of XmlDatabaseComponent");
                else
                    component = _component;
            }

            return component.FromXml(xmlPackageNode, XmlDatabaseComponent.SchemaV1Dot0);
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
        public bool ParseDatabase1V1(XDocument globalDependenciesDoc, List<DatabasePackage> globalDependenciesList, XDocument dependenciesDoc,
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

        private bool ParseDatabase1V1Packages(List<XElement> xmlPackageNodesList, IList genericPackageList)
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

                    //if it's a package entry, we need to recursively processes it
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
        /// Save the database to an Xml version format
        /// </summary>
        public void SaveDatabase(string saveLocation, string documentVersion = null)
        {
            if (string.IsNullOrEmpty(documentVersion))
                documentVersion = this.DocumentVersion;

            //
            CustomDatabaseLocation = saveLocation;
            if (Path.HasExtension(CustomDatabaseLocation))
                CustomDatabaseLocation = Path.GetDirectoryName(CustomDatabaseLocation);

            switch (documentVersion)
            {
                case DocumentVersion1V1:
                    SaveDatabase1V1(CustomDatabaseLocation);
                    return;
                case DocumentVersion1V2:
                    SaveDatabase1V2(CustomDatabaseLocation);
                    return;
                default:
                    Logging.Error("Unknown document version to save as: {0}", documentVersion);
                    return;
            }
        }

        private void SaveDatabase1V2(string savePath)
        {
            string rootDocPath = Path.Combine(savePath, "database.xml");

            //load or create document
            XDocument doc;
            if (File.Exists(rootDocPath))
                doc = XmlUtils.LoadXDocument(rootDocPath, XmlLoadType.FromFile);
            else
                doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));

            string docContentsBeforeSave = doc.Root.ToString();

            //get or create the top element
            XElement topElement = doc.Root;
            if (topElement == null)
            {
                topElement = new XElement("modInfoAlpha.xml");
                doc.Add(topElement);
            }

            //update or create the top attributes
            UpdateAttribute(topElement, WoTClientVersionXmlString, WoTClientVersion.Trim());
            UpdateAttribute(topElement, WoTOnlineFolderVersionXmlString, WoTOnlineFolderVersion.Trim());
            UpdateAttribute(topElement, DocumentVersionXmlString, DocumentVersion);
            UpdateAttribute(topElement, SchemaVersionXmlString, SchemaVersion);

            //add the elements if they don't already exist
            UpdateElement(topElement, "globalDependencies");
            UpdateElement(topElement, "dependencies");
            XElement categories = topElement.Element("categories");
            if (categories == null)
            {
                categories = new XElement("categories");
                topElement.Add(categories);
            }

            //add category element if don't already exist
            foreach (Category category in ParsedCategoryList)
            {
                if (string.IsNullOrWhiteSpace(category.XmlFilename))
                {
                    category.XmlFilename = category.Name.Replace(" ", string.Empty).Replace("/", "_").Replace("\\", "_") + ".xml";
                    UpdateElement(categories, "category", category.XmlFilename);
                }
                UpdateElement(categories, "category");
            }
            
            //check if the document has changed and needs to be saved
            string docContentsAfterSave = doc.Root.ToString();
            if (!docContentsBeforeSave.Equals(docContentsAfterSave))
                doc.Save(rootDocPath);

            //check each document file if it needs to be saved
            UpdateDatabaseComponentFile(Path.Combine(savePath, "globalDependencies.xml"), "GlobalDependencies");
            UpdateDatabaseComponentFile(Path.Combine(savePath, "dependencies.xml"), "Dependencies");
            foreach (Category cat in ParsedCategoryList)
                UpdateDatabaseComponentFile(Path.Combine(savePath, cat.XmlFilename), cat.GetType().Name, cat);
        }

        private void UpdateDatabaseComponentFile(string documentPath, string rootElementHolder, CoreDatabaseComponent component = null)
        {
            //load or create document
            XDocument doc;
            if (File.Exists(documentPath))
                doc = XmlUtils.LoadXDocument(documentPath, XmlLoadType.FromFile);
            else
                doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));

            string docContentsBeforeSave = doc.Root.ToString();

            //get or create the top element
            XElement topElement = doc.Root;
            if (topElement == null)
            {
                topElement = new XElement(rootElementHolder);
                doc.Add(topElement);
            }
            else if (!topElement.Name.Equals(rootElementHolder))
            {
                Logging.Warning("Expected component of type '{0}', but found '{1}'. It was removed", rootElementHolder, topElement.Name);
                topElement.Remove();
                topElement = new XElement(rootElementHolder);
                doc.Add(topElement);
            }

            //if dependency or global, they have an extra container
            switch(rootElementHolder)
            {
                case "GlobalDependencies":
                    UpdateDatabaseDependencyFile(topElement, GlobalDependencies);
                    break;
                case "Dependencies":
                    UpdateDatabaseDependencyFile(topElement, Dependencies);
                    break;
                case "Category":
                    component.ToXml(topElement, SchemaVersion);
                    break;
            }                

            string docContentsAfterSave = doc.Root.ToString();
            if (!docContentsBeforeSave.Equals(docContentsAfterSave))
                doc.Save(documentPath);
        }

        private void UpdateDatabaseDependencyFile(XElement rootElementHolder, IList packages)
        {
            List<XElement> xElements = rootElementHolder.Elements(packages[0].GetType().Name).ToList();
            int index = 0;
            foreach(DatabasePackage package in packages)
            {
                XElement packageHolder = xElements[index];
                if (packageHolder == null)
                {
                    packageHolder = new XElement(package.GetType().Name);
                    rootElementHolder.Add(packageHolder);
                    xElements = rootElementHolder.Elements(packages[0].GetType().Name).ToList();
                }
                else if (!packageHolder.Attribute("UID").Value.Equals(package.UID))
                {
                    packageHolder.Remove();
                    packageHolder = new XElement(package.GetType().Name);
                    rootElementHolder.Add(packageHolder);
                    xElements = rootElementHolder.Elements(packages[0].GetType().Name).ToList();
                }
                package.ToXml(packageHolder, SchemaVersion);
                index++;
            }
        }

        private void UpdateAttribute(XElement element, string attributeName, string attributeValue)
        {
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                element.Add(new XAttribute(attributeName, attributeValue));
            }
            else
            {
                if (attribute.Value != attributeValue)
                    attribute.Value = attributeValue;
            }
        }

        private void UpdateElement(XElement topElement, string elementName, string elementValue = null)
        {
            if (string.IsNullOrEmpty(elementValue))
                elementValue = elementName + ".xml";

            XElement listElement = topElement.Element(elementName);
            if (listElement == null)
                topElement.Add(new XElement(elementName, new XAttribute("file", elementValue)));
            else
            {
                UpdateAttribute(listElement, "file", elementValue);
            }
        }

        /// <summary>
        /// Save the database to the Xml version 1.1 standard
        /// </summary>
        /// <param name="savePath">The path to save all the xml files to</param>
        private void SaveDatabase1V1(string savePath)
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
            root.SetAttribute("version", WoTClientVersion.Trim());
            root.SetAttribute("onlineFolder", WoTOnlineFolderVersion.Trim());

            //create root document (contains filenames for all other xml documents)
            root.SetAttribute("documentVersion", DocumentVersion1V1);

            //put root element into document
            doc.AppendChild(root);

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
            foreach (Category cat in ParsedCategoryList)
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
            xmlDeclaration = xmlGlobalDependenciesFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlGlobalDependenciesFile.AppendChild(xmlDeclaration);
            XmlElement xmlGlobalDependenciesFileRoot = xmlGlobalDependenciesFile.CreateElement("GlobalDependencies");
            SaveDatabaseList1V1(GlobalDependencies, xmlGlobalDependenciesFileRoot, xmlGlobalDependenciesFile, "GlobalDependency");
            xmlGlobalDependenciesFile.AppendChild(xmlGlobalDependenciesFileRoot);
            xmlGlobalDependenciesFile.Save(Path.Combine(savePath, "globalDependencies.xml"));

            //dependency
            XmlDocument xmlDependenciesFile = new XmlDocument();
            xmlDeclaration = xmlDependenciesFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDependenciesFile.AppendChild(xmlDeclaration);
            XmlElement xmlDependenciesFileRoot = xmlDependenciesFile.CreateElement("Dependencies");
            SaveDatabaseList1V1(Dependencies, xmlDependenciesFileRoot, xmlDependenciesFile, "Dependency");
            xmlDependenciesFile.AppendChild(xmlDependenciesFileRoot);
            xmlDependenciesFile.Save(Path.Combine(savePath, "dependencies.xml"));

            //for each category do the same thing
            foreach (Category cat in ParsedCategoryList)
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
        private void SaveDatabaseList1V1(IList packagesToSave, XmlElement documentRootElement, XmlDocument docToMakeElementsFrom, string nameToSaveElementsBy)
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

        private void SavePropertiesToXmlElements(IXmlSerializable databaseComponentWithDefaultValues, string elementToSave, XmlElement elementContainer, IDatabaseComponent componentToSave, PropertyInfo propertyOfComponentToSave, XmlDocument docToMakeElementsFrom)
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

        private void SavePropertiesToXmlAttributes(IXmlSerializable databasePackageOfDefaultValues, XmlElement packageElement, IDatabaseComponent packageToSave)
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

        #region other methods
        public List<DatabasePackage> AllPackages()
        {
            return DatabaseUtils.GetFlatList(this.GlobalDependencies, this.Dependencies, this.ParsedCategoryList);
        }
        #endregion
    }
}
