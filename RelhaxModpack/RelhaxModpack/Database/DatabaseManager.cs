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

namespace RelhaxModpack.Database
{
    public class DatabaseManager
    {
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

        public string ModInfoRootXmlString { get; private set; }

        public XmlDocument ModInfoRootXmlDocument { get; private set; }

        public string WoTClientVersion { get; private set; }

        public string WoTOnlineFolderVersion { get; private set; }

        /// <summary>
        /// A reference to the modpack settings window configuration class
        /// </summary>
        public ModpackSettings ModpackSettings { get; set; }

        public CommandLineSettings CommandLineSettings { get; set; }

        public ZipFile ManagerInfoZipfile { get; set; }

        private DatabaseVersions databaseVersionToLoad;

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public DatabaseManager(ModpackSettings modpackSettings)
        {
            if (this.ModpackSettings == null)
                this.ModpackSettings = modpackSettings;
        }

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public DatabaseManager(ModpackSettings modpackSettings, CommandLineSettings commandLineSettings)
        {
            if (this.ModpackSettings == null)
                this.ModpackSettings = modpackSettings;
            if (this.CommandLineSettings == null)
                this.CommandLineSettings = commandLineSettings;
        }

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public DatabaseManager()
        {
            
        }

        public async Task<DatabaseLoadFailCode> LoadDatabaseAsync()
        {
            if (ModpackSettings == null)
                throw new NullReferenceException();

            GlobalDependencies = new List<DatabasePackage>();
            Dependencies = new List<Dependency>();
            ParsedCategoryList = new List<Category>();

            databaseVersionToLoad = ModpackSettings.DatabaseDistroVersion;
            if (CommandLineSettings != null && CommandLineSettings.TestMode)
                databaseVersionToLoad = DatabaseVersions.Test;

            //get the Xml database loaded into a string based on database version type (from server download, from github, from testfile
            string modInfoXml = string.Empty;
            ZipFile modInfoZipFile = null;
            switch (databaseVersionToLoad)
            {
                //from server download
                case DatabaseVersions.Stable:
                    if (ManagerInfoZipfile == null)
                    {
                        //download the manager info zip file
                        using (WebClient client = new WebClient())
                        {
                            try
                            {
                                byte[] managerInfoZipFile = await client.DownloadDataTaskAsync(ApplicationConstants.ManagerInfoURLBigmods);
                                ManagerInfoZipfile = ZipFile.Read(new MemoryStream(managerInfoZipFile));
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
                    string supportedClientsXML = FileUtils.GetStringFromZip(ManagerInfoZipfile, ApplicationConstants.SupportedClients);
                    if (string.IsNullOrWhiteSpace(supportedClientsXML))
                    {
                        Logging.Info("Failed to extract supported_clients.xml from string from zipfile", Logfiles.Application, LogLevel.Exception);
                        return DatabaseLoadFailCode.FailedToExtractXmlFromZipFile;
                    }

                    XmlDocument doc = XmlUtils.LoadXmlDocument(supportedClientsXML, XmlLoadType.FromString);
                    if (doc == null)
                    {
                        Logging.Error("Failed to parse supported_clients.xml into xml document");
                        return DatabaseLoadFailCode.FailedToParseXml;
                    }

                    //parse the list of supported client xml nodes
                    XmlNodeList supportedVersionsXML = XmlUtils.GetXmlNodesFromXPath(doc, "//versions/version");
                    string lastSupportedWoTClient = supportedVersionsXML[supportedClientsXML.Count() - 1].InnerText.Trim();
                    Logging.Info(LogOptions.ClassName, "Parsed last supported WoT client as {0}", lastSupportedWoTClient);

                    //make string download rul
                    string modInfoXmlUrlStable = ApplicationConstants.BigmodsDatabaseRootEscaped.Replace(@"{dbVersion}", lastSupportedWoTClient) + ApplicationConstants.ModInfoZip;

                    //download latest modInfo xml
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            //save zip file into memory
                            Logging.Debug("Download {0} from {1}", ApplicationConstants.ModInfoZip, modInfoXmlUrlStable);
                            modInfoZipFile = ZipFile.Read(new MemoryStream(client.DownloadData(modInfoXmlUrlStable)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Exception("Failed to download {0}", ApplicationConstants.ModInfoZip);
                        Logging.Exception(ex.ToString());
                        return DatabaseLoadFailCode.FailedToDownloadZipFile;
                    }

                    //extract modinfo xml string
                    modInfoXml = FileUtils.GetStringFromZip(modInfoZipFile, ApplicationConstants.BetaDatabaseV2RootFilename);
                    break;

                //from github
                case DatabaseVersions.Beta:
                    using (WebClient client = new WebClient())
                    {
                        //load string constant url from manager info xml
                        string modInfoXmlUrlBeta = ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", ModpackSettings.BetaDatabaseSelectedBranch) + ApplicationConstants.BetaDatabaseV2RootFilename;
                        Logging.Debug("Download beta database from {0}", modInfoXmlUrlBeta);

                        //download the xml string into "modInfoXml"
                        client.Headers.Add("user-agent", "Mozilla / 4.0(compatible; MSIE 6.0; Windows NT 5.2;)");
                        modInfoXml = client.DownloadString(modInfoXmlUrlBeta);
                    }
                    break;

                //from testfile
                case DatabaseVersions.Test:
                    //make string
                    if (!File.Exists(ModpackSettings.CustomModInfoPath))
                    {
                        Logging.Error("The test database root file at {0} does not exist!", ModpackSettings.CustomModInfoPath);
                        return DatabaseLoadFailCode.FailedToLoadXmlFromDisk;
                    }

                    //load modinfo xml into string
                    modInfoXml = File.ReadAllText(ModpackSettings.CustomModInfoPath);
                    break;
            }

            //check to make sure the xml string is valid
            if (string.IsNullOrWhiteSpace(modInfoXml))
            {
                Logging.Error("Failed to load {0} into string", ApplicationConstants.BetaDatabaseV2RootFilename);
                if (databaseVersionToLoad == DatabaseVersions.Test)
                    return DatabaseLoadFailCode.FailedToLoadXmlFromDisk;
                else
                    return DatabaseLoadFailCode.FailedToExtractXmlFromZipFile;
            }

            //load the xml document into xml object
            XmlDocument modInfoDocument = XmlUtils.LoadXmlDocument(modInfoXml, XmlLoadType.FromString);
            if (modInfoDocument == null)
            {
                Logging.Error("Failed to parse {0} from xml string", ApplicationConstants.BetaDatabaseV2RootFilename);
                return DatabaseLoadFailCode.FailedToParseXml;
            }

            //get WoT xml version attributes from databasea root
            WoTOnlineFolderVersion = XmlUtils.GetXmlStringFromXPath(modInfoDocument, ApplicationConstants.DatabaseOnlineFolderXpath);
            WoTClientVersion = XmlUtils.GetXmlStringFromXPath(modInfoDocument, ApplicationConstants.DatabaseOnlineVersionXpath);

            //parse the modInfoXml to list in memory
            switch (databaseVersionToLoad)
            {
                case DatabaseVersions.Stable:
                    Logging.Debug("Getting xml string values from zip file");
                    List<string> categoriesXml = new List<string>();

                    string globalDependencyFilename = XmlUtils.GetXmlStringFromXPath(modInfoDocument, "/modInfoAlpha.xml/globalDependencies/@file");
                    Logging.Debug("Found xml entry: {0}", globalDependencyFilename);
                    string globalDependencyXmlString = FileUtils.GetStringFromZip(modInfoZipFile, globalDependencyFilename);

                    string dependencyFilename = XmlUtils.GetXmlStringFromXPath(modInfoDocument, "/modInfoAlpha.xml/dependencies/@file");
                    Logging.Debug("Found xml entry: {0}", dependencyFilename);
                    string dependenicesXmlString = FileUtils.GetStringFromZip(modInfoZipFile, dependencyFilename);

                    foreach (XmlNode categoryNode in XmlUtils.GetXmlNodesFromXPath(modInfoDocument, "//modInfoAlpha.xml/categories/category"))
                    {
                        string categoryFilename = categoryNode.Attributes["file"].Value;
                        Logging.Debug("Found xml entry: {0}", categoryFilename);
                        categoriesXml.Add(FileUtils.GetStringFromZip(modInfoZipFile, categoryFilename));
                    }
                    modInfoZipFile.Dispose();
                    modInfoZipFile = null;

                    //parse into lists
                    if (!DatabaseUtils.ParseDatabase1V1FromStrings(globalDependencyXmlString, dependenicesXmlString, categoriesXml, GlobalDependencies, Dependencies, ParsedCategoryList))
                    {
                        Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                        return DatabaseLoadFailCode.FailedToParseDatabase;
                    }
                    break;
                //github
                case DatabaseVersions.Beta:
                    Logging.Debug("Init beta db download resources");
                    //create download url list
                    List<string> downloadURLs = DatabaseUtils.GetBetaDatabase1V1FilesList(ApplicationConstants.BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", ModpackSettings.BetaDatabaseSelectedBranch), ModpackSettings.BetaDatabaseSelectedBranch);

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
                    Logging.Debug("Sending strings to db loader method");
                    if (!DatabaseUtils.ParseDatabase1V1FromStrings(globalDependencyXmlStringBeta, dependenicesXmlStringBeta, categoriesXmlBeta, GlobalDependencies, Dependencies, ParsedCategoryList))
                    {
                        Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                        return DatabaseLoadFailCode.FailedToParseDatabase;
                    }
                    break;
                //test
                case DatabaseVersions.Test:
                    if (!DatabaseUtils.ParseDatabase1V1FromFiles(Path.GetDirectoryName(ModpackSettings.CustomModInfoPath), modInfoDocument, GlobalDependencies, Dependencies, ParsedCategoryList))
                    {
                        Logging.WriteToLog("Failed to parse database", Logfiles.Application, LogLevel.Error);
                        return DatabaseLoadFailCode.FailedToParseDatabase;
                    }
                    break;
            }

            DatabaseUtils.BuildLinksRefrence(ParsedCategoryList, false);
            DatabaseUtils.BuildLevelPerPackage(ParsedCategoryList);

            return DatabaseLoadFailCode.None;
        }
    }
}
