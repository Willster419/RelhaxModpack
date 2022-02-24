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
using RelhaxModpack.Patching;
using RelhaxModpack.Installer;
using RelhaxModpack.Shortcuts;
using RelhaxModpack.Atlases;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Provides methods for loading and saving xml component objects to and from xml database files.
    /// </summary>
    public class DatabaseManager
    {
        /// <summary>
        /// A structure used to keep a reference of a component and a dependency that it calls.
        /// </summary>
        /// <remarks>This is used to determine if any packages call any dependencies who's packageName does not exist in the database.</remarks>
        public struct LogicTracking
        {
            /// <summary>
            /// The database component what has dependencies.
            /// </summary>
            public IComponentWithDependencies ComponentWithDependencies;

            /// <summary>
            /// The called dependency from the component.
            /// </summary>
            public DatabaseLogic DatabaseLogic;
        }

        /// <summary>
        /// The document version 1.1 string.
        /// </summary>
        public const string DocumentVersion1V1 = "1.1";

        /// <summary>
        /// The document version 1.2 string.
        /// </summary>
        public const string DocumentVersion1V2 = "1.2";

        /// <summary>
        /// The current latest used schema.
        /// </summary>
        /// <seealso cref="XmlComponent.SchemaV1Dot2"/>
        public const string LatestSchema = XmlComponent.SchemaV1Dot2;

        /// <summary>
        /// The xml attribute used in the root database xml file to determine which version of WoT this database is compatible with.
        /// </summary>
        public const string WoTClientVersionXmlString = "version";

        /// <summary>
        /// The xml attribute used in the root database xml file to determine which online FTP folder this database should use.
        /// </summary>
        public const string WoTOnlineFolderVersionXmlString = "onlineFolder";

        /// <summary>
        /// The xml attribute used in the root database xml file to determine what version format the database is in.
        /// </summary>
        public const string DocumentVersionXmlString = "documentVersion";

        /// <summary>
        /// The xml attribute used in the root database xml file to determine what schema version the database is using for each xml component.
        /// </summary>
        public const string SchemaVersionXmlString = "schemaVersion";

        /// <summary>
        /// The list of categories.
        /// </summary>
        public List<Category> ParsedCategoryList { get; private set; }

        /// <summary>
        /// The list of global dependencies.
        /// </summary>
        public List<DatabasePackage> GlobalDependencies { get; private set; }

        /// <summary>
        /// The list of dependencies.
        /// </summary>
        public List<Dependency> Dependencies { get; private set; }

        /// <summary>
        /// The reference to the loaded root document object of the database.
        /// </summary>
        public XmlDocument DatabaseRootXmlDocument { get; private set; }

        /// <summary>
        /// The parsed version of the WoT client that this database is compatible with.
        /// </summary>
        public string WoTClientVersion { get; private set; }

        /// <summary>
        /// The parsed name of the online folder that this database should use for downloading packages.
        /// </summary>
        public string WoTOnlineFolderVersion { get; private set; }

        /// <summary>
        /// The parsed version format of the database xml.
        /// </summary>
        public string DocumentVersion { get; private set; }

        /// <summary>
        /// The parsed schema version format of the database xml.
        /// </summary>
        public string SchemaVersion { get; private set; }

        /// <summary>
        /// A reference to the modpack settings window configuration class.
        /// </summary>
        /// <remarks>This is used for determining the version of the database to load (stable or beta), and if beta, the branch to use for loading it.</remarks>
        public ModpackSettings ModpackSettings { get; set; }

        /// <summary>
        /// A reference to the command line settings configuration class.
        /// </summary>
        /// <remarks>This is used for determining if the database to be loaded is in 'test mode'.</remarks>
        public CommandLineSettings CommandLineSettings { get; set; }

        /// <summary>
        /// The manager info data file downloaded on application startup.
        /// </summary>
        /// <seealso cref="ApplicationConstants.ManagerVersion"/>
        public ZipFile ManagerInfoZipfile { get; set; }

        /// <summary>
        /// The version of the database to load, or has been loaded.
        /// </summary>
        public DatabaseVersions DatabaseDistroToLoad { get; private set; }

        /// <summary>
        /// The list of dependencies calculated to install.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        public List<Dependency> DependenciesToInstall { get { return dependenciesToInstall; } }

        /// <summary>
        /// The list of packages to install, including dependencies and user selected packages, and excluding packages if a minimal install is set from ModpackSettings.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        public List<DatabasePackage> PackagesToInstall { get { return packagesToInstall; } }

        /// <summary>
        /// The list of user selected packages to install, including dependencies and user selected packages, and excluding packages if a minimal install is set from ModpackSettings.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        public List<SelectablePackage> SelectablePackagesToInstall { get { return selectablePackagesToInstall; } }

        /// <summary>
        /// The list of packages to install that have a zip file.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        /// <seealso cref="PackagesToInstall"/>
        public List<DatabasePackage> PackagesToInstallWithZipFile { get { return packagesToInstall?.FindAll(package => !string.IsNullOrWhiteSpace(package.ZipFile)); } }

        /// <summary>
        /// The list of user selected packages to install that have a zip file.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        /// <seealso cref="SelectablePackagesToInstall"/>
        public List<SelectablePackage> SelectablePackagesToInstallWithZipFiles { get { return selectablePackagesToInstall?.FindAll(package => !string.IsNullOrWhiteSpace(package.ZipFile)); } }

        /// <summary>
        /// The list of packages to install that have zip files, that need to be downloaded.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        /// <seealso cref="PackagesToInstallWithZipFile"/>
        public List<DatabasePackage> PackagesToDownload { get { return PackagesToInstallWithZipFile?.FindAll(pack => pack.DownloadFlag); } }

        /// <summary>
        /// The list of packages to install (that is, those with zip files), sorted into their install groups.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        public List<DatabasePackage>[] PackagesToInstallByInstallGroup { get { return orderedListPackagesToInstall; } }

        /// <summary>
        /// The list of packages to install that have triggers.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        /// <seealso cref="PackagesToInstall"/>
        /// <seealso cref="Trigger"/>
        public List<DatabasePackage> PackagesToInstallWithTriggers { get { return packagesToInstall?.FindAll(package => !string.IsNullOrWhiteSpace(package.Triggers)); } }

        /// <summary>
        /// The list of patch operations to perform after package extraction and xml unpack operations.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        public List<Patch> PatchesToInstall { get { return patchesToInstall; } }

        /// <summary>
        /// The list of xml unpack operations to perform after package extraction.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        public List<XmlUnpack> XmlUnpacksToInstall { get { return xmlUnpacksToInstall; } }

        /// <summary>
        /// The list of shortcuts creation operations to perform after package extraction.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        public List<Shortcut> ShortcutsToInstall { get { return shortcutsToInstall; } }

        /// <summary>
        /// The list of atlas creation operations to perform after package extraction.
        /// </summary>
        /// <remarks>This is done by running the method to calculate what packages need to be installed.</remarks>
        /// <seealso cref="CalculateInstallLists(bool, bool)"/>
        public List<Atlas> AtlasesToInstall { get { return atlasesToInstall; } }

        private string databaseRootXmlString;

        private ZipFile modInfoZipFile;

        //options set for each public entry point
        private string lastSupportedWoTClient;

        private string CustomDatabaseLocation;

        private string BetaDatabaseBranch;

        //lists from install calculation
        private List<Dependency> dependenciesToInstall;

        private List<DatabasePackage> packagesToInstall;

        private List<SelectablePackage> selectablePackagesToInstall;

        private List<DatabasePackage>[] orderedListPackagesToInstall;

        private List<Patch> patchesToInstall;

        private List<XmlUnpack> xmlUnpacksToInstall;

        private List<Shortcut> shortcutsToInstall;

        private List<Atlas> atlasesToInstall;

        /// <summary>
        /// Creates an instance of the DatabaseManager class.
        /// </summary>
        /// <param name="modpackSettings">The reference to the modpack settings class.</param>
        public DatabaseManager(ModpackSettings modpackSettings)
        {
            this.ModpackSettings = modpackSettings;
        }

        /// <summary>
        /// Creates an instance of the DatabaseManager class.
        /// </summary>
        /// <param name="modpackSettings">The reference to the modpack settings class.</param>
        /// <param name="commandLineSettings">The reference to the command line settings class.</param>
        public DatabaseManager(ModpackSettings modpackSettings, CommandLineSettings commandLineSettings)
        {
            this.ModpackSettings = modpackSettings;
            this.CommandLineSettings = commandLineSettings;
        }

        /// <summary>
        /// Creates an instance of the DatabaseManager class.
        /// </summary>
        public DatabaseManager()
        {
            
        }

        #region Load database methods
        /// <summary>
        /// Loads and parses a database into the DatabaseManager.
        /// </summary>
        /// <param name="databaseDistroToLoad">The distribution (stable, beta or test) of the database to load.</param>
        /// <param name="locationToLoadFrom">The path (url or folder) to load the database files from.</param>
        /// <param name="betaDatabaseBranch">If loading the beta database, the github branch to use for loading the database.</param>
        /// <returns>The status enumeration code of the operation.</returns>
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
        /// Load a test database from the specified location, including the root filename.
        /// </summary>
        /// <param name="locationToLoadFrom">The path to the root xml file.</param>
        /// <returns>The status enumeration code of the operation.</returns>
        public async Task<DatabaseLoadFailCode> LoadDatabaseTestAsync(string locationToLoadFrom)
        {
            //this version of the overloaded method, it's implied that we're loading in test mode
            this.DatabaseDistroToLoad = DatabaseVersions.Test;
            this.CustomDatabaseLocation = locationToLoadFrom;
            this.BetaDatabaseBranch = null;

            return await LoadDatabaseWithOptionsAsync();
        }

        /// <summary>
        /// Loads and parses a database into the DatabaseManager.
        /// </summary>
        /// <param name="databaseDistroToLoad">The distribution (stable, beta or test) of the database to load.</param>
        /// <returns>The status enumeration code of the operation.</returns>
        public async Task<DatabaseLoadFailCode> LoadDatabaseDistroAsync(DatabaseVersions databaseDistroToLoad)
        {
            if (ModpackSettings == null)
                throw new NullReferenceException();

            this.DatabaseDistroToLoad = databaseDistroToLoad;
            this.CustomDatabaseLocation = ModpackSettings.CustomModInfoPath;
            this.BetaDatabaseBranch = ModpackSettings.BetaDatabaseSelectedBranch;

            return await LoadDatabaseWithOptionsAsync();
        }

        /// <summary>
        /// Loads and parses a database into the DatabaseManager
        /// </summary>
        /// <returns>The status enumeration code of the operation.</returns>
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

            ProcessDatabase();

            return DatabaseLoadFailCode.None;
        }

        /// <summary>
        /// Loads and parses the stable database of a specific client version to the DatabaseManager.
        /// </summary>
        /// <param name="clientVersion">The version of the client to try to load. This should correspond to the value of what is in the desired database's version.</param>
        /// <returns>Returns DatabaseLoadFailCode.None</returns>
        /// <remarks>This is used in the ModpackToolbox for getting a list of all packages that correspond to a WoT version. It shouldn't be used for any generic purpose.</remarks>
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
            ProcessDatabase();
            return DatabaseLoadFailCode.None;
        }

        /// <summary>
        /// Loads and parses a database from xml strings into the DatabaseManager.
        /// </summary>
        /// <param name="rootDocument">The xml document object of the root document.</param>
        /// <param name="globalDependenciesXml">The xml string of the global dependencies document.</param>
        /// <param name="dependneciesXml">The xml string of the dependencies document.</param>
        /// <param name="categoriesXml">The list of xml strings of each category document</param>
        /// <returns>Returns DatabaseLoadFailCode.None</returns>
        /// <remarks>This is used in the ModpackToolbox for a developer purpose. It shouldn't be used for any generic purpose.</remarks>
        public DatabaseLoadFailCode LoadDatabaseCustomFromStringsAsync(XmlDocument rootDocument, string globalDependenciesXml, string dependneciesXml, List<string> categoriesXml)
        {
            if (string.IsNullOrWhiteSpace(globalDependenciesXml))
                throw new ArgumentException("Was null or empty", nameof(globalDependenciesXml));
            if (string.IsNullOrWhiteSpace(dependneciesXml))
                throw new ArgumentException("Was null or empty", nameof(dependneciesXml));
            if (categoriesXml == null || categoriesXml.Count == 0)
                throw new ArgumentException("Was null or count was 0", nameof(globalDependenciesXml));

            Init();
            DatabaseRootXmlDocument = rootDocument;
            LoadMetadataInfoFromRootXmlDocument();
            ParseDatabaseFromStrings(globalDependenciesXml, dependneciesXml, categoriesXml, GlobalDependencies, Dependencies, ParsedCategoryList);
            ProcessDatabase();
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
        /// <summary>
        /// Loads the database root document into the DatabaseManager
        /// </summary>
        /// <param name="databaseDistroToLoad">The distribution of the database to load.</param>
        /// <param name="locationToLoadFrom">If loading a test distribution, the location to the xml file.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Gets the direct download links to each beta database xml file from a given branch specified in the ModpackSettings instance.
        /// </summary>
        /// <returns>A list of direct download links for each xml document of the beta database.</returns>
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

            return ParseDatabaseFromStrings(globalDependencyXmlStringBeta, dependenicesXmlStringBeta, categoriesXmlBeta, GlobalDependencies, Dependencies, ParsedCategoryList);
        }

        private DatabaseLoadFailCode ParseDatabaseFromStrings(string globalDependenciesXml, string dependneciesXml, List<string> categoriesXml,
            List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            //parse into lists
            switch (DocumentVersion)
            {
                case DocumentVersion1V1:
                case DocumentVersion1V2:
                    if (!ParseDatabase1V1FromStrings(globalDependenciesXml, dependneciesXml, categoriesXml, GlobalDependencies, Dependencies, ParsedCategoryList))
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
        /// Parse a database into the version 1.1 format from files on the disk.
        /// </summary>
        /// <returns>True if the parsing was successful, false otherwise.</returns>
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
        /// Parse a database into version 1.1 from string representations of the Xml files.
        /// </summary>
        /// <param name="globalDependenciesXml">The Xml string of the global dependencies document.</param>
        /// <param name="dependneciesXml">The Xml string of the dependencies document.</param>
        /// <param name="categoriesXml">The list of Xml strings of the categories document.</param>
        /// <param name="globalDependencies">The list of global dependencies.</param>
        /// <param name="dependencies">The list of dependencies.</param>
        /// <param name="parsedCategoryList">The list of categories.</param>
        /// <returns>True if the parsing was successful, false otherwise.</returns>
        private bool ParseDatabase1V1FromStrings(string globalDependenciesXml, string dependneciesXml, List<string> categoriesXml,
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

        /// <summary>
        /// Parse a database into the DatabaseManager from the xml object files.
        /// </summary>
        /// <param name="globalDependenciesDoc">The xml object of global dependencies.</param>
        /// <param name="globalDependenciesList">The list of global dependencies to populate.</param>
        /// <param name="dependenciesDoc">The xml object of dependencies.</param>
        /// <param name="dependenciesList">The list of dependencies to populate.</param>
        /// <param name="categoryDocuments">The list of xml objects of each category.</param>
        /// <param name="parsedCategoryList">The list of categories to populate.</param>
        /// <returns>True if the parsing was successful, false otherwise.</returns>
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

        /// <summary>
        /// Parse a database of format v1.2 into the DatabaseManager from the xml object files.
        /// </summary>
        /// <param name="globalDependenciesDoc">The xml object of global dependencies.</param>
        /// <param name="globalDependenciesList">The list of global dependencies to populate.</param>
        /// <param name="dependenciesDoc">The xml object of dependencies.</param>
        /// <param name="dependenciesList">The list of dependencies to populate.</param>
        /// <param name="categoryDocuments">The list of xml objects of each category.</param>
        /// <param name="parsedCategoryList">The list of categories to populate.</param>
        /// <returns>True if the parsing was successful, false otherwise.</returns>
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
                Category category = i >= parsedCategoryList.Count? null : parsedCategoryList[i];

                if (category == null)
                {
                    //make sure object type is properly implemented into serialization system
                    if (!(Activator.CreateInstance(typeof(Category)) is XmlComponent _component))
                        throw new BadMemeException("Type of this list is not of XmlDatabaseComponent");
                    else
                        category = _component as Category;
                }

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

                XmlComponent component = index >= genericPackageList.Count? null : genericPackageList[index] as XmlComponent;

                if (component == null)
                {
                    //make sure object type is properly implemented into serialization system
                    if (!(Activator.CreateInstance(listObjectType) is XmlComponent _component))
                        throw new BadMemeException("Type of this list is not of XmlDatabaseComponent");
                    else
                        component = _component;
                }

                //string beforeLoad = GetXmlStringOfComponent(new XElement(component.GetXmlElementName(SchemaVersion)), component);

                if (!ParseDatabase1V2Package(xmlPackageNode, component, listObjectType))
                    parsed = false;

                //string afterLoad = GetXmlStringOfComponent(new XElement(component.GetXmlElementName(SchemaVersion)), component);

                //if (beforeLoad != afterLoad)
                genericPackageList.Add(component);
                index++;
            }
            return parsed;
        }

        private string GetXmlStringOfComponent(XElement element, XmlComponent component)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (component == null)
                throw new ArgumentNullException(nameof(component));

            component.ToXml(element, SchemaVersion);
            return element.ToString();
        }

        private bool ParseDatabase1V2Package(XElement xmlPackageNode, XmlComponent component, Type componentType)
        {
            if (xmlPackageNode == null)
                throw new ArgumentNullException(nameof(xmlPackageNode));

            if (component == null)
                throw new ArgumentNullException(nameof(component));

            return component.FromXml(xmlPackageNode, string.IsNullOrEmpty(SchemaVersion)? XmlComponent.SchemaV1Dot0 : SchemaVersion);
        }

        /// <summary>
        /// Parse a database of format 1.1 into the DatabaseManager from the xml object files.
        /// </summary>
        /// <param name="globalDependenciesDoc">The xml object of global dependencies.</param>
        /// <param name="globalDependenciesList">The list of global dependencies to populate.</param>
        /// <param name="dependenciesDoc">The xml object of dependencies.</param>
        /// <param name="dependenciesList">The list of dependencies to populate.</param>
        /// <param name="categoryDocuments">The list of xml objects of each category.</param>
        /// <param name="parsedCategoryList">The list of categories to populate.</param>
        /// <returns>True if the parsing was successful, false otherwise.</returns>
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
        /// Save the database to a given location on disk.
        /// </summary>
        /// <param name="saveLocation">The folder path to save the xml documents to.</param>
        /// <param name="documentVersion">The version format of the xml documents to save.</param>
        /// <param name="schemaVersion">The schema version format (if the document version supports it) of the properties of xml documents to save.</param>
        public void SaveDatabase(string saveLocation, string documentVersion, string schemaVersion = null)
        {
            if (string.IsNullOrEmpty(saveLocation))
                throw new BadMemeException("saveLocation is null or empty");

            if (string.IsNullOrEmpty(documentVersion))
                throw new BadMemeException("documentVersion is null or empty");

            DocumentVersion = documentVersion;
            if (!string.IsNullOrEmpty(schemaVersion))
                SchemaVersion = schemaVersion;

            SaveDatabase(saveLocation, DocumentVersion);
        }

        /// <summary>
        /// Save the database to an Xml version format.
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

            //get or create the top element
            XElement topElement = doc.Root;
            if (topElement == null)
            {
                topElement = new XElement("modInfoAlpha.xml");
                doc.Add(topElement);
            }

            string docContentsBeforeSave = doc.Root.ToString();

            //update or create the top attributes
            UpdateAttribute(topElement, WoTClientVersionXmlString, WoTClientVersion.Trim());
            UpdateAttribute(topElement, WoTOnlineFolderVersionXmlString, WoTOnlineFolderVersion.Trim());
            UpdateAttribute(topElement, DocumentVersionXmlString, DocumentVersion);
            if (string.IsNullOrEmpty(SchemaVersion))
                SchemaVersion = XmlComponent.SchemaV1Dot0;
            UpdateAttribute(topElement, SchemaVersionXmlString, SchemaVersion);

            //add the elements if they don't already exist
            UpdateElement(topElement, "globalDependencies");
            UpdateElement(topElement, "dependencies");

            //always add the category element
            XElement categories = topElement.Element("categories");
            if (categories != null)
                categories.Remove();
            categories = new XElement("categories");
            topElement.Add(categories);

            //add category element if don't already exist
            foreach (Category category in ParsedCategoryList)
            {
                if (string.IsNullOrWhiteSpace(category.XmlFilename))
                {
                    category.XmlFilename = category.Name.Replace(" ", string.Empty).Replace("/", "_").Replace("\\", "_") + ".xml";
                }
                categories.Add(new XElement("category", new XAttribute("file", category.XmlFilename)));
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

            //get or create the top element
            XElement topElement = doc.Root;
            if (topElement == null)
            {
                topElement = new XElement(rootElementHolder);
                doc.Add(topElement);
            }
            else if (!topElement.Name.LocalName.Equals(rootElementHolder))
            {
                Logging.Warning("Expected component of type '{0}', but found '{1}'. It was removed", rootElementHolder, topElement.Name);
                topElement.Remove();
                topElement = new XElement(rootElementHolder);
                doc.Add(topElement);
            }

            string docContentsBeforeSave = doc.Root.ToString();

            //if dependency or global, they have an extra container
            switch (rootElementHolder)
            {
                case "GlobalDependencies":
                    UpdateDatabaseDependencyFile(topElement, GlobalDependencies, "GlobalDependency");
                    break;
                case "Dependencies":
                    UpdateDatabaseDependencyFile(topElement, Dependencies, typeof(Dependency).Name);
                    break;
                case "Category":
                    component.ToXml(topElement, SchemaVersion);
                    break;
            }                

            string docContentsAfterSave = doc.Root.ToString();
            if (!docContentsBeforeSave.Equals(docContentsAfterSave))
                doc.Save(documentPath);
        }

        private void UpdateDatabaseDependencyFile(XElement rootElementHolder, IList packages, string xmlElementName)
        {
            List<XElement> xElements = rootElementHolder.Elements(xmlElementName).ToList();
            int index = 0;
            foreach(DatabasePackage package in packages)
            {
                XElement packageHolder = null;
                if (index < xElements.Count)
                    packageHolder = xElements[index];
                if (packageHolder == null)
                {
                    packageHolder = new XElement(xmlElementName);
                    rootElementHolder.Add(packageHolder);
                    xElements = rootElementHolder.Elements(xmlElementName).ToList();
                }
                package.ToXml(packageHolder, SchemaVersion);
                index++;
            }

            //remove any extra XElements after the end of the loop
            while (index < xElements.Count)
            {
                xElements.Last().Remove();
                xElements = rootElementHolder.Elements(xmlElementName).ToList();
            }
        }

        private void UpdateAttribute(XElement element, string attributeName, string attributeValue)
        {
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                attribute = new XAttribute(attributeName, attributeValue);
                element.Add(attribute);
            }
            else if(attribute.Value != attributeValue)
            {
                attribute.Value = attributeValue;
            }
        }

        private void UpdateElement(XElement topElement, string elementName)
        {
            string elementValue = elementName + ".xml";

            XElement listElement = topElement.Element(elementName);
            if (listElement == null)
            {
                listElement = new XElement(elementName, new XAttribute("file", elementValue));
                topElement.Add(listElement);
            }
            else
            {
                UpdateAttribute(listElement, "file", elementValue);
            }
        }

        /// <summary>
        /// Save the database to the Xml version 1.1 standard.
        /// </summary>
        /// <param name="savePath">The path to save all the xml files to.</param>
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
        /// Saves a list of packages to a document using the document format version 1.1
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

        #region Process database after loading
        /// <summary>
        /// Perform any database post-load processing (such as performing reference linking of packages, conflicts, etc).
        /// </summary>
        public void ProcessDatabase()
        {
            foreach (Category category in ParsedCategoryList)
            {
                category.DatabaseManager = this;
                category.ProcessPackages();
            }

            ProcessDependencyPackageRefrences();
            ProcessConflictingPackages();
        }

        /// <summary>
        /// Links the databasePackage objects with dependencies objects to have those objects link references to the parent and the dependency object
        /// </summary>
        private void ProcessDependencyPackageRefrences()
        {
            List<IComponentWithDependencies> componentsWithDependencies_ = new List<IComponentWithDependencies>();

            //get all categories where at least one dependency exists
            componentsWithDependencies_.AddRange(ParsedCategoryList.Where(cat => cat.Dependencies.Count > 0));

            //get all packages and dependencies where at least one dependency exists
            componentsWithDependencies_.AddRange(GetFlatList().OfType<IComponentWithDependencies>().Where(component => component.Dependencies.Count > 0).ToList());

            foreach (IComponentWithDependencies componentWithDependencies in componentsWithDependencies_)
            {
                foreach (DatabaseLogic logic in componentWithDependencies.Dependencies)
                {
                    logic.ParentPackageRefrence = componentWithDependencies;

                    if (!string.IsNullOrEmpty(logic.PackageUID))
                        logic.DependencyPackageRefrence = Dependencies.Find(dependency => dependency.UID.Equals(logic.PackageUID));
                    else
                        logic.DependencyPackageRefrence = Dependencies.Find(dependency => dependency.PackageName.Equals(logic.PackageName));

                    if (logic.DependencyPackageRefrence == null)
                    {
                        Logging.Error("DatabaseLogic component from package {0} was unable to link to dependency {1} (does the dependency not exist or bad reference?)", componentWithDependencies.ComponentInternalName, logic.PackageName);
                        continue;
                    }

                    if (string.IsNullOrEmpty(logic.PackageUID))
                    {
                        logic.PackageUID = logic.DependencyPackageRefrence.UID;
                    }
                }
            }
        }

        private void ProcessConflictingPackages()
        {
            List<SelectablePackage> packages = GetFlatSelectablePackageList().FindAll(pack => pack.ConflictingPackagesNew != null && pack.ConflictingPackagesNew.Count > 0);
            foreach (SelectablePackage package in packages)
            {
                for (int i = 0; i < package.ConflictingPackagesNew.Count; i++)
                {
                    ConflictingPackage conflictingPackageEntry = package.ConflictingPackagesNew[i];

                    if (string.IsNullOrEmpty(conflictingPackageEntry.ConflictingPackageUID) && string.IsNullOrEmpty(conflictingPackageEntry.ConflictingPackageName))
                    {
                        Logging.Error($"The package {package.PackageName} conflicting package entry {i} does not have a packageName or packageUID. It was removed.");
                        package.ConflictingPackagesNew.Remove(conflictingPackageEntry);
                        i--;
                        continue;
                    }

                    SelectablePackage conflictingPackage;
                    string lookupProperty;
                    if (string.IsNullOrEmpty(conflictingPackageEntry.ConflictingPackageUID))
                    {
                        lookupProperty = conflictingPackageEntry.ConflictingPackageName;
                        conflictingPackage = GetSelectablePackageByPackageName(conflictingPackageEntry.ConflictingPackageName);
                        if (conflictingPackage != null)
                        {
                            conflictingPackageEntry.ConflictingPackageUID = conflictingPackage.UID;
                        }
                    }
                    else
                    {
                        lookupProperty = conflictingPackageEntry.ConflictingPackageUID;
                        conflictingPackage = GetSelectablePackageByUid(conflictingPackageEntry.ConflictingPackageUID);
                    }

                    if (conflictingPackage == null)
                    {
                        Logging.Error($"Package {package.PackageName} conflicting package failed to look up a package by property '{lookupProperty}'. It was removed.");
                        package.ConflictingPackagesNew.Remove(conflictingPackageEntry);
                        i--;
                        continue;
                    }

                    //make sure the entry doesn't already exist
                    if (package.ConflictingPackagesProcessed != null && package.ConflictingPackagesProcessed.Count > 1)
                    {
                        List<ConflictingPackage> matchingEntries = package.ConflictingPackagesNew.FindAll(_ => _.IsEqual(conflictingPackageEntry));
                        if (matchingEntries != null && matchingEntries.Count > 1)
                        {
                            Logging.Warning($"Package {package.PackageName} already has conflicting package entry '{lookupProperty}'. Skipping this entry.");
                            package.ConflictingPackagesNew.Remove(conflictingPackageEntry);
                            i--;
                            continue;
                        }
                    }

                    conflictingPackageEntry.ConflictingSelectablePackage = conflictingPackage;
                }
            }

            //now check that the package that it conflicts with is also entered as conflicting with the original package
            //(if it is listed in package A that it conflicts with package B, then it stands to reason that package B should have a corresponding entry)
            List<SelectablePackage> _packages = GetFlatSelectablePackageList().FindAll(pack => pack.ConflictingPackagesNew != null && pack.ConflictingPackagesNew.Count > 0);
            foreach (SelectablePackage _package in _packages)
            {
                foreach (SelectablePackage conflictingPackage in _package.ConflictingPackagesProcessed)
                {
                    ConflictingPackage result = conflictingPackage.ConflictingPackagesNew.Find(_ => _.ConflictingPackageUID.Equals(_package.UID));
                    if (result == null)
                    {
                        conflictingPackage.ConflictingPackagesNew.Add(new ConflictingPackage()
                        {
                            LoadedSchemaVersion = conflictingPackage.LoadedSchemaVersion,
                            ConflictingPackageName = _package.PackageName,
                            ConflictingPackageUID = _package.UID,
                            ParentSelectablePackage = conflictingPackage,
                            ConflictingSelectablePackage = _package
                        });
                        Logging.Info($"Package {_package.PackageName} is set to conflict with {conflictingPackage.PackageName}, but {conflictingPackage.PackageName} does not have a matching entry for {_package.PackageName}. It was created");
                    }
                }
            }
        }
        #endregion

        #region Install-time calculations
        /// <summary>
        /// Run calculations on package list components to determine what packages, features, etc get included for an installation operation.
        /// </summary>
        /// <param name="suppressFrequentDependencyCalculationMessages">If true, will suppress some dependency calculation messages.</param>
        /// <param name="showDependencyCalculationErrorMessages">If true, will show some dependency error calculation messages (some are always shown).</param>
        public void CalculateInstallLists(bool suppressFrequentDependencyCalculationMessages, bool showDependencyCalculationErrorMessages)
        {
            CalculateDependencies(suppressFrequentDependencyCalculationMessages, showDependencyCalculationErrorMessages);
            CreateListOfPackagesToInstall();
            CreateOrderedInstallList(PackagesToInstallWithZipFile);
            patchesToInstall = CreateOrderedInstructionList(packagesToInstall, InstructionsType.Patch)?.Cast<Patch>()?.ToList();
            atlasesToInstall = CreateOrderedInstructionList(packagesToInstall, InstructionsType.Atlas)?.Cast<Atlas>()?.ToList();
            xmlUnpacksToInstall = CreateOrderedInstructionList(packagesToInstall, InstructionsType.UnpackCopy)?.Cast<XmlUnpack>()?.ToList();
            shortcutsToInstall = CreateOrderedInstructionList(packagesToInstall, InstructionsType.Shortcut)?.Cast<Shortcut>()?.ToList();
        }

        /// <summary>
        /// Calculates what dependencies should be installed given a user's selection (and other recursive dependency calculations).
        /// </summary>
        /// <param name="suppressSomeLogging">If true, will suppress some dependency calculation messages.</param>
        /// <param name="showDependencyCalculationErrorMessages">If true, will show some dependency error calculation messages (some are always shown).</param>
        private void CalculateDependencies(bool suppressSomeLogging, bool showDependencyCalculationErrorMessages)
        {
            //flat list is packages
            List<SelectablePackage> flatListSelect = GetFlatSelectablePackageList();

            //1- build the list of calling mods that need it
            dependenciesToInstall = new List<Dependency>();

            //create list to track all database dependency references
            List<LogicTracking> refrencedDependencies = new List<LogicTracking>();

            Logging.Debug("Starting step 1 of 4 in dependency calculation: adding from categories");
            foreach (Category category in ParsedCategoryList)
            {
                foreach (DatabaseLogic logic in category.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = category
                    });
                    foreach (Dependency dependency in Dependencies)
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
                    foreach (Dependency dependency in Dependencies)
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
            foreach (Dependency processingDependency in Dependencies)
            {
                foreach (DatabaseLogic logic in processingDependency.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = processingDependency
                    });
                    foreach (Dependency dependency in Dependencies)
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
            List<Dependency> notProcessedDependnecies = new List<Dependency>(Dependencies);
            Logging.Debug("Starting step 4 of 4 in dependency calculation: calculating dependencies from top down (perspective to list)");
            int calcNumber = 1;
            foreach (Dependency dependency in Dependencies)
            {
                //first check if this dependency is referencing a dependency that has not yet been processed
                //if so then note it in the log
                if (!suppressSomeLogging)
                    Logging.Debug(string.Empty);
                if (!suppressSomeLogging)
                    Logging.Debug("Calculating if dependency {0} will be installed, {1} of {2}", dependency.PackageName, calcNumber++, Dependencies.Count);

                foreach (DatabaseLogic login in dependency.DatabasePackageLogic)
                {
                    List<Dependency> matches = notProcessedDependnecies.Where(dep => login.PackageName.Equals(dep.PackageName)).ToList();
                    if (matches.Count > 0)
                    {
                        string errorMessage = string.Format("Dependency {0} is referenced by the dependency {1} which has not yet been processed! " +
                            "This will lead to logic errors in database calculation! Tip: this dependency ({0}) should be BELOW ({1}) in the" +
                            "list of dependencies in the editor. Order matters!", dependency.PackageName, login.PackageName);
                        Logging.Error(errorMessage);
                        if (showDependencyCalculationErrorMessages)
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
                        List<Dependency> found = Dependencies.FindAll(dep => dep.PackageName.Equals(callingLogic.PackageName));

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
        }

        private void CreateListOfPackagesToInstall()
        {
            packagesToInstall = new List<DatabasePackage>();
            packagesToInstall.AddRange(GlobalDependencies.FindAll(globalDep => globalDep.Enabled));
            packagesToInstall.AddRange(dependenciesToInstall.FindAll(dep => dep.Enabled));
            selectablePackagesToInstall = GetFlatSelectablePackageList().FindAll(fl => fl.Enabled && fl.Checked);
            packagesToInstall.AddRange(selectablePackagesToInstall);
            if (ModpackSettings.MinimalistMode)
            {
                selectablePackagesToInstall.RemoveAll(selectablePack => selectablePack.MinimalistModeExclude);
                packagesToInstall.RemoveAll(pack => pack.MinimalistModeExclude);
            }
        }

        /// <summary>
        /// Creates an array of DatabasePackage lists sorted by Installation groups i.e. list in array index 0 is packages of install group 0
        /// </summary>
        /// <param name="packagesToInstallWithZipfile"></param>
        /// <returns>The array of DatabasePackage lists</returns>
        private void CreateOrderedInstallList(List<DatabasePackage> packagesToInstallWithZipfile)
        {
            //get the max number of defined groups
            int maxGrops = packagesToInstallWithZipfile.Select(max => max.InstallGroupWithOffset).Max();

            //make the list to return
            //make it maxGroups +1 because group 4 exists, but making a array of 4 is 0-3
            orderedListPackagesToInstall = new List<DatabasePackage>[maxGrops + 1];

            //new up the lists
            for (int i = 0; i < orderedListPackagesToInstall.Count(); i++)
                orderedListPackagesToInstall[i] = new List<DatabasePackage>();

            foreach (DatabasePackage package in packagesToInstallWithZipfile)
            {
                orderedListPackagesToInstall[package.InstallGroupWithOffset].Add(package);
            }
        }

        private List<Instruction> CreateOrderedPatchesList(List<DatabasePackage> packagesWithInstructions)
        {
            List<Instruction> orderedInstructions = new List<Instruction>();

            //first loop at each patch group number
            for (int i = 0; i <= GetMaxPatchGroupNumber(packagesWithInstructions); i++)
            {
                //then loop at each install group number
                for (int j = 0; j <= GetMaxInstallGroupNumberWithOffset(packagesWithInstructions); j++)
                {
                    List<DatabasePackage> packagesWithCorrectOffsets = packagesWithInstructions.FindAll(package => package.PatchGroup == i && package.InstallGroupWithOffset == j);
                    foreach (DatabasePackage packageWithCorrectOffsets in packagesWithCorrectOffsets)
                    {
                        orderedInstructions.AddRange(packageWithCorrectOffsets.Patches);
                    }
                }
            }

            return orderedInstructions;
        }

        private List<Instruction> CreateOrderedInstructionsList(List<DatabasePackage> packagesWithInstructions, InstructionsType instructionsType)
        {
            List<Instruction> orderedInstructions = new List<Instruction>();

            //loop at each install group number
            for (int j = 0; j <= GetMaxInstallGroupNumberWithOffset(packagesWithInstructions); j++)
            {
                List<DatabasePackage> packagesWithCorrectOffsets = packagesWithInstructions.FindAll(package => package.InstallGroupWithOffset == j);
                foreach (DatabasePackage packageWithCorrectOffsets in packagesWithCorrectOffsets)
                {
                    orderedInstructions.AddRange(packageWithCorrectOffsets.GetInstructions(instructionsType));
                }
            }

            return orderedInstructions;
        }

        private List<Instruction> CreateOrderedInstructionList(List<DatabasePackage> packagesToInstall, InstructionsType instructionsType)
        {
            if (packagesToInstall == null)
                throw new NullReferenceException(nameof(packagesToInstall));

            //filter out packages that don't have any instructions of the given type
            List<DatabasePackage> packagesWithInstructions = packagesToInstall.FindAll(package => package.GetInstructions(instructionsType) != null && package.GetInstructions(instructionsType).Count > 0);

            //if there are none, then nothing to do
            if (packagesWithInstructions == null || packagesWithInstructions.Count == 0)
                return null;

            switch (instructionsType)
            {
                case InstructionsType.Patch:
                    return CreateOrderedPatchesList(packagesWithInstructions);
                default:
                    return CreateOrderedInstructionsList(packagesWithInstructions, instructionsType);
            }
        }
        #endregion

        #region Utility methods
        /// <summary>
        /// Find a SelectablePackage in the list of loaded packages by searching for its UID.
        /// </summary>
        /// <param name="targetUid">The UID of the package to find.</param>
        /// <returns>The package if found, otherwise null.</returns>
        public SelectablePackage GetSelectablePackageByUid(string targetUid)
        {
            return GetSelectablePackageByUid(GetFlatSelectablePackageList(), targetUid);
        }

        private SelectablePackage GetSelectablePackageByUid(List<SelectablePackage> packages, string targetUid)
        {
            return packages.FirstOrDefault(pack => pack.UID.Equals(targetUid));
        }

        /// <summary>
        /// Find a SelectablePackage in the list of loaded packages by searching for its package name (PackageName, not user display name).
        /// </summary>
        /// <param name="targetPackageName">The package name of the package to find.</param>
        /// <returns>The package if found, otherwise null.</returns>
        public SelectablePackage GetSelectablePackageByPackageName(string targetPackageName)
        {
            return GetSelectablePackageByPackageName(GetFlatSelectablePackageList(), targetPackageName);
        }

        private SelectablePackage GetSelectablePackageByPackageName(List<SelectablePackage> packages, string targetPackageName)
        {
            return packages.FirstOrDefault(pack => pack.PackageName.Equals(targetPackageName));
        }

        /// <summary>
        /// Gets the maximum InstallGroup number from a list of Packages
        /// </summary>
        /// <returns>The maximum InstallGroup number</returns>
        public int GetMaxInstallGroupNumber()
        {
            return GetFlatList().Max(ma => ma.InstallGroup);
        }

        /// <summary>
        /// Gets the maximum InstallGroup number from a list of Packages factoring in the offset that a category may apply to it
        /// </summary>
        /// <returns>The maximum InstallGroup number</returns>
        public int GetMaxInstallGroupNumberWithOffset()
        {
            return GetMaxInstallGroupNumberWithOffset(GetFlatList().FindAll(package => package.Patches != null && package.Patches.Count > 0));
        }

        private int GetMaxInstallGroupNumberWithOffset(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.InstallGroupWithOffset);
        }

        /// <summary>
        /// Gets the maximum PatchGroup number from a list of Packages
        /// </summary>
        /// <returns>The maximum PatchGroup number</returns>
        public int GetMaxPatchGroupNumber()
        {
            return GetMaxPatchGroupNumber(GetFlatList().FindAll(package => package.Patches != null && package.Patches.Count > 0));
        }

        private int GetMaxPatchGroupNumber(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.PatchGroup);
        }/// <summary>
         /// Returns a flat list of the given recursive lists, in the order that the parameters are stated
         /// </summary>
         /// <returns>The flat list</returns>
         /// <remarks>In the case of Categories, the flat list has the sub-level packages added at the level of the parent</remarks>
        public List<DatabasePackage> GetFlatList()
        {
            List<DatabasePackage> flatList = new List<DatabasePackage>();
            flatList.AddRange(GlobalDependencies);
            flatList.AddRange(Dependencies);
            flatList.AddRange(GetFlatSelectablePackageList());
            return flatList;
        }

        /// <summary>
        /// Returns a flat list of the given recursive lists, in the order that the parameters are stated
        /// </summary>
        /// <returns>The flat list</returns>
        /// <remarks>In the case of Categories, the flat list has the sub-level packages added at the level of the parent</remarks>
        public List<SelectablePackage> GetFlatSelectablePackageList()
        {
            List<SelectablePackage> flatList = new List<SelectablePackage>();
            foreach (Category cat in ParsedCategoryList)
                flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
        }

        /// <summary>
        /// Checks for any duplicate UID entries inside the provided lists
        /// </summary>
        /// <returns>A list of packages with duplicate UIDs, or an empty list if no duplicates</returns>
        public List<DatabasePackage> CheckForDuplicateUIDsPackageList()
        {
            List<DatabasePackage> duplicatesList = new List<DatabasePackage>();
            List<DatabasePackage> flatList = GetFlatList();
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
        /// <returns>A list of duplicate UIDs, or an empty list if no duplicates</returns>
        public List<string> CheckForDuplicateUIDsStringsList()
        {
            return CheckForDuplicateUIDsPackageList().Select(package => package.UID).ToList();
        }

        /// <summary>
        /// Checks for any duplicate PackageName entries inside the provided lists
        /// </summary>
        /// <returns>A list of duplicate packages, or an empty list if no duplicates</returns>
        public List<string> CheckForDuplicatePackageNamesStringsList()
        {
            List<string> duplicatesList = new List<string>();
            List<DatabasePackage> flatList = GetFlatList();
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
        /// <param name="nameToCheck">The PackageName parameter to check</param>
        /// <returns>True if the nameToCheck exists in the list, false otherwise</returns>
        public bool IsDuplicatePackageName(string nameToCheck)
        {
            return GetFlatList().Find(pack => pack.PackageName.Equals(nameToCheck)) != null;
        }

        /// <summary>
        /// Sorts the packages inside each Category object
        /// </summary>
        public void SortDatabase()
        {
            //the first level of packages are always sorted
            foreach (Category cat in ParsedCategoryList)
            {
                SortDatabase(cat.Packages);
            }
        }

        /// <summary>
        /// Sorts a list of packages
        /// </summary>
        /// <param name="packages">The list of packages to sort</param>
        /// <param name="recursive">If the list should recursively sort</param>
        private void SortDatabase(List<SelectablePackage> packages, bool recursive = true)
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
        #endregion
    }
}
