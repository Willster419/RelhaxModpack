using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.ComponentModel;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all generic constants and statics used throughout the application
    /// </summary>
    public static class Settings
    {
        #region Application Files and Folders
        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// The name of the application settings file
        /// </summary>
        public const string ModpackSettingsFileName = "RelhaxSettingsV2.xml";

        /// <summary>
        /// The name of the settings color file
        /// </summary>
        /// <remarks>Stores all settings</remarks>
        public const string UISettingsColorFile = "UISettings.xml";

        /// <summary>
        /// The name of the legacy application settings file
        /// </summary>
        public const string OldModpackSettingsFilename = "RelHaxSettings.xml";

        /// <summary>
        /// The name of the application color settings file
        /// </summary>
        public const string UISettingsFileName = "UISettings.xml";

        /// <summary>
        /// The name of the modpack editor tool settings file
        /// </summary>
        public const string EditorSettingsFilename = "EditorSettings.xml";

        /// <summary>
        /// The name of the modpack patch designer tool settings file
        /// </summary>
        public const string PatcherSettingsFilename = "PatchSettings.xml";

        /// <summary>
        /// The name of the modpack automation runner tool settings file
        /// </summary>
        public const string AutomationRunnerSettingsFilename = "AutomationRunnerSettings.xml";

        /// <summary>
        /// The name of the selection file when used in the setting "save last installed selection"
        /// </summary>
        public const string LastSavedConfigFilename = "lastInstalledConfig.xml";

        /// <summary>
        /// The file in the application root directory used to unlock the "launch editor" button
        /// </summary>
        public const string EditorLaunchFromMainWindowFilename = "EditorUnlock.txt";

        /// <summary>
        /// The default end address of the location of mod packages (start + zip + end)
        /// </summary>
        public const string DefaultEndAddress = "";

        /// <summary>
        /// The filename of the V2 root database document. All category names and filenames, and version info is in this document
        /// </summary>
        public const string BetaDatabaseV2RootFilename = "database.xml";

        /// <summary>
        /// The filename to download the latest stable or beta application zip file as
        /// </summary>
        public const string ApplicationUpdateFileName = "RelhaxModpack_update.zip";

        /// <summary>
        /// The filename to save the self updater script as
        /// </summary>
        public const string RelicBatchUpdateScript = "relic_self_updater.bat";

        /// <summary>
        /// The filename of the self updater script inside the manager zip file
        /// </summary>
        public const string RelicBatchUpdateScriptServer = "relic_self_updater.txt";

        /// <summary>
        /// The name of the application executable when compiled with stable distribution
        /// </summary>
        public const string ApplicationFilenameStable = "RelhaxModpack.exe";

        /// <summary>
        /// The name of the application executable when compiled with beta distribution
        /// </summary>
        public const string ApplicationFilenameBeta = "RelhaxModpackBeta.exe";

        /// <summary>
        /// The old V1 filename to save the self updater script as
        /// </summary>
        [Obsolete("Database format V1 is deprecated, please use V2 instead.")]
        public const string RelicBatchUpdateScriptOld = "RelicCopyUpdate.bat";

        /// <summary>
        /// The root filename of the list of selection files
        /// </summary>
        public const string SelectionsXml = "selections.xml";

        /// <summary>
        /// The absolute path of the application zip file and zip database file folder
        /// </summary>
        public static readonly string RelhaxDownloadsFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxDownloads");

        /// <summary>
        /// The old absolute path of the application zip file and zip database file folder
        /// </summary>
        /// [Obsolete]
        public static readonly string RelhaxDownloadsFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxDownloads");

        /// <summary>
        /// The absolute path of the application mod backup folder
        /// </summary>
        public static readonly string RelhaxModBackupFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxModBackup");

        /// <summary>
        /// The old absolute path of the application mod backup folder
        /// </summary>
        /// [Obsolete]
        public static readonly string RelhaxModBackupFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxModBackup");

        /// <summary>
        /// The absolute path of the application user selections folder. Default location 
        /// </summary>
        public static readonly string RelhaxUserSelectionsFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxUserSelections");

        /// <summary>
        /// The old absolute path of the application user selections folder. Old Default location
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxUserSelectionsFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxUserConfigs");

        /// <summary>
        /// The absolute path of the application folder where users can place custom mod zip files
        /// </summary>
        public static readonly string RelhaxUserModsFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxUserMods");

        /// <summary>
        /// The old absolute path of the application folder where users can place custom mod zip files
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxUserModsFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxUserMods");

        /// <summary>
        /// The absolute path of the application temporary folder
        /// </summary>
        public static readonly string RelhaxTempFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxTemp");

        /// <summary>
        /// The old absolute path of the application temporary folder
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxTempFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxTemp");

        /// <summary>
        /// The absolute path of the application 3rd party dll references folder. Currently used to hold atlas file libraries
        /// </summary>
        public static readonly string RelhaxLibrariesFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxLibraries");

        /// <summary>
        /// The old absolute path of the application 3rd party dll references folder. Currently used to hold atlas file libraries
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxLibrariesFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxLibraries");

        /// <summary>
        /// The name of the 64bit folder in the 'World_of_Tanks' directory
        /// </summary>
        public const string WoT64bitFolder = "win64";

        /// <summary>
        /// The name of the 32bit folder in the 'World_of_Tanks' directory
        /// </summary>
        public const string WoT32bitFolder = "win32";

        /// <summary>
        /// The name of the 'mods' directory
        /// </summary>
        public const string ModsDir = "mods";

        /// <summary>
        /// The name of the 'res_mods' directory
        /// </summary>
        public const string ResModsDir = "res_mods";

        /// <summary>
        /// The WoT 64bit folder name with the folder separator before it
        /// </summary>
        public static readonly string WoT64bitFolderWithSlash = Path.DirectorySeparatorChar + WoT64bitFolder;

        /// <summary>
        /// The WoT 32bit folder name with the folder separator before it
        /// </summary>
        public static readonly string WoT32bitFolderWithSlash = Path.DirectorySeparatorChar + WoT32bitFolder;

        /// <summary>
        /// The name of the version xml used for getting the current client version information
        /// </summary>
        public const string WoTVersionXml = "version.xml";

        /// <summary>
        /// The name of the installer folder to hold all patch xml files in
        /// </summary>
        public const string PatchFolderName = "_patch";

        /// <summary>
        /// The name of the installer folder to hold all shortcut xml files in
        /// </summary>
        public const string ShortcutFolderName = "_shortcuts";

        /// <summary>
        /// The name of the installer folder to hold all xml unpack entries in
        /// </summary>
        public const string XmlUnpackFolderName = "_xmlUnPack";

        /// <summary>
        /// The name of the installer folder to hold all xml atlas creation instructions in
        /// </summary>
        public const string AtlasCreationFoldername = "_atlases";

        /// <summary>
        /// The name of the installer folder to hold all fonts to install (or check if needs to install)
        /// </summary>
        public const string FontsToInstallFoldername = "_fonts";

        /// <summary>
        /// The name of the temporary install folder to hold database manager readme files. The end user does not need this folder
        /// and will be deleted at the end of the installation
        /// </summary>
        public const string ReadmeFromZipfileFolderName = "_readme";

        /// <summary>
        /// The name of the temporary install folder that holds the auto update information of the database editor.
        /// </summary>
        public const string AutoUpdateZipFolderName = "_autoUpdate";

        /// <summary>
        /// The list of installer folders in the root {WoT} directory to cleanup after an installation
        /// </summary>
        public static readonly string[] FoldersToCleanup = new string[]
        {
            PatchFolderName,
            ShortcutFolderName,
            XmlUnpackFolderName,
            AtlasCreationFoldername,
            FontsToInstallFoldername,
            ReadmeFromZipfileFolderName,
            AutoUpdateZipFolderName
        };

        /// <summary>
        /// The absolute path to the md5 hash zip file download database file
        /// </summary>
        public static readonly string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolderPath, "MD5HashDatabase.xml");

        /// <summary>
        /// The filename of the selection file used to select mods on default loading of the mod selection list
        /// </summary>
        public const string DefaultCheckedSelectionfile = "default_checked.xml";

        /// <summary>
        /// The filename of the xml document inside the manager info zip file containing the list of supported WoT clients
        /// </summary>
        public const string SupportedClients = "supported_clients.xml";

        /// <summary>
        /// The filename of the xml document inside the manager info zip file containing manager version information
        /// </summary>
        public const string ManagerVersion = "manager_version.xml";

        /// <summary>
        /// The location of the manager info zip file. Contains several xml files with database and client definitions
        /// </summary>
        [Obsolete("Do not use this unless for file deleting, here only for legacy purposes. File is no longer created.")]
        public static readonly string ManagerInfoDatFile = Path.Combine(RelhaxTempFolderPath, "managerInfo.dat");

        /// <summary>
        /// The absolute path of the selection file used for saving last selection
        /// </summary>
        public static readonly string LastInstalledConfigFilepath = Path.Combine(RelhaxUserSelectionsFolderPath, LastSavedConfigFilename);

        /// <summary>
        /// The absolute path of the V2 settings file used for saving ModpackSettings
        /// </summary>
        public static readonly string RelhaxSettingsFilepath = Path.Combine(ApplicationStartupPath, ModpackSettingsFileName);

        /// <summary>
        /// The absolute path of the Relhax log file
        /// </summary>
        public static readonly string RelhaxLogFilepath = Path.Combine(ApplicationStartupPath, Logging.ApplicationLogFilename);

        /// <summary>
        /// Array of all Modpack created folders in the application directory
        /// </summary>
        public static readonly string[] FoldersToCheck = new string[]
        {
            RelhaxDownloadsFolderPath,
            RelhaxModBackupFolderPath,
            RelhaxUserSelectionsFolderPath,
            RelhaxUserModsFolderPath,
            RelhaxTempFolderPath,
            RelhaxLibrariesFolderPath
        };

        /// <summary>
        /// The name of the pmod log file
        /// </summary>
        public const string PmodLog = "pmod.log";

        /// <summary>
        /// The name of the xvm log file
        /// </summary>
        public const string XvmLog = "xvm.log";

        /// <summary>
        /// The name of the WoT python debug log file
        /// </summary>
        public const string PythonLog = "python.log";

        /// <summary>
        /// The name of the logs folder used for WG CEF browser and Relhax Modpack
        /// </summary>
        public const string LogsFolder = "logs";

        /// <summary>
        /// The minimum release value of the user's installed .NET framework to use the modpack
        /// </summary>
        /// <remarks>It varies for OS. For example:
        /// On Windows 10 May 2019 Update and Windows 10 November 2019 Update: 528040
        /// On Windows 10 May 2020 Update: 528372
        /// On all other Windows operating systems(including other Windows 10 operating systems): 528049
        /// See: https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed </remarks>
        public const int MinimumDotNetFrameworkVersionRequired = 528040;

        /// <summary>
        /// URL to get the latest version of the .NET Framework
        /// </summary>
        public const string DotNetFrameworkLatestDownloadURL = "https://dotnet.microsoft.com/download/dotnet-framework";
        #endregion

        #region URLs
        /// <summary>
        /// The escaped constant URL of the stable database on the server, escaped with the 'dbVersion' macro
        /// </summary>
        public const string BigmodsDatabaseRootEscaped = "http://bigmods.relhaxmodpack.com/RelhaxModpack/resources/database/{dbVersion}/";

        /// <summary>
        /// The default starting address of the location of mod packages (start + zip + end), escaped with the 'onlineFolder' macro
        /// </summary>
        /// <remarks>'onlineFolder' is a 3 digit number representing the major release version of WoT e.g. 1.7.0</remarks>
        public const string DefaultStartAddress = @"http://bigmods.relhaxmodpack.com/WoT/{onlineFolder}/";

        /// <summary>
        /// The URL of the V2 beta database root folder, escaped with the 'branch' macro
        /// </summary>
        /// <remarks>'branch' is a name of a github branch on the RelhaxModpackDatabase repo</remarks>
        public const string BetaDatabaseV2FolderURLEscaped = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/{branch}/latest_database/";

        /// <summary>
        /// The URL of the V2 beta database root folder, using BetaDatabaseSelectedBranch as the replacement for the 'branch' macro
        /// </summary>
        /// <remarks>By default, this value is 'master'</remarks>
        /// <seealso cref="ModpackSettings.BetaDatabaseSelectedBranch"/>
        public static string BetaDatabaseV2FolderURL
        {
            get
            { return BetaDatabaseV2FolderURLEscaped.Replace(@"{branch}", ModpackSettings.BetaDatabaseSelectedBranch); }
        }

        /// <summary>
        /// The API URL to return a json format document of the current branches in the repository
        /// </summary>
        public const string BetaDatabaseBranchesURL = "https://api.github.com/repos/Willster419/RelhaxModpackDatabase/branches";

        /// <summary>
        /// The URL of the V2 manager info zip file
        /// </summary>
        public const string ManagerInfoURLBigmods = "http://bigmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat";

        /// <summary>
        /// The URL to the location of the latest stable version of the application as a zip file
        /// </summary>
        public const string ApplicationUpdateURL = "http://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.zip";

        /// <summary>
        /// The URL to the location of the latest beta version of the application as a zip file
        /// </summary>
        public const string ApplicationBetaUpdateURL = "http://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpackBeta.zip";

        /// <summary>
        /// The root URL of the V2 selection files location
        /// </summary>
        public const string SelectionsRoot = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/master/selection_files/";


        /// <summary>
        /// The URL path of the latest application stable release notes
        /// </summary>
        public const string ApplicationNotesStableUrl = "https://github.com/Willster419/RelhaxModpack/raw/master/RelhaxModpack/RelhaxModpack/bin/Debug/release_notes_stable.txt";

        /// <summary>
        /// The URL path of the latest application beta release notes
        /// </summary>
        public const string ApplicationNotesBetaUrl = "https://github.com/Willster419/RelhaxModpack/raw/master/RelhaxModpack/RelhaxModpack/bin/Debug/release_notes_beta.txt";

        /// <summary>
        /// The URL path of the latest V2 database release notes
        /// </summary>
        public const string DatabaseNotesUrl = "https://github.com/Willster419/RelhaxModpackDatabase/raw/master/resources/databaseUpdate.txt";
        #endregion

        #region Application and Database properties
        /// <summary>
        /// The xpath string to get the onlineFolder attribute from the document root
        /// </summary>
        public const string DatabaseOnlineFolderXpath = "/modInfoAlpha.xml/@onlineFolder";

        /// <summary>
        /// The xpath string to get the database version info attribute from the document root
        /// </summary>
        public const string DatabaseOnlineVersionXpath = "/modInfoAlpha.xml/@version";

        /// <summary>
        /// The old V2 config file version for saving the user's selection preferences
        /// </summary>
        [Obsolete("Selection file version 2.0 is deprecated")]
        public const string ConfigFileVersion2V0 = "2.0";

        /// <summary>
        /// The latest config file version for saving the user's selection preferences
        /// </summary>
        public const string ConfigFileVersion3V0 = "3.0";

        /// <summary>
        /// The name of the WoT process used for detecting if it is running
        /// </summary>
        public const string WoTProcessName = "WorldOfTanks";

        /// <summary>
        /// The xpath to the version information used by the modpack to determine the WoT client version
        /// </summary>
        public const string WoTVersionXmlXpath = "//version.xml/version";

        /// <summary>
        /// The current distribution version of the application.
        /// Alpha should NEVER be built for public distribution unless direct testing!
        /// </summary>
        public const ApplicationVersions ApplicationVersion = ApplicationVersions.Stable;

        /// <summary>
        /// Flag to determine if the user running is intentionally using the alpha version (or if an Alpha version was accidentally distributed)
        /// </summary>
        public static bool TrueAlpha = false;

        /// <summary>
        /// The number of logical processors (threads) detected on the system. Used to make n installation threads for faster extraction
        /// </summary>
        public static readonly int NumLogicalProcesors = Environment.ProcessorCount;

        /// <summary>
        /// The amount so space characters to line up a continued log entry without the date/time
        /// </summary>
        public const string LogSpacingLineup = "                          ";
        /// <summary>
        /// The location of the WoT app data folder parsed at installation time
        /// </summary>
        public static string AppDataFolder = string.Empty;

        /// <summary>
        /// The location of the WoT installation directory parsed at installation time
        /// </summary>
        /// <remarks>The path is absolute, ending at "World_of_Tanks"</remarks>
        public static string WoTDirectory = string.Empty;
        
        /// <summary>
        /// The version information of WoT parsed at install time
        /// </summary>
        /// <remarks>This info is gathered from the "version.xml" file from the game's root directory</remarks>
        public static string WoTClientVersion = string.Empty;

        /// <summary>
        /// The version of the online folder name containing the zip files for this game
        /// </summary>
        /// <remarks>The online folders are done by major versions only i.e. 1.4.1, 1.5.0, etc. All zip files on 1.5.0.x are stored in this folder</remarks>
        public static string WoTModpackOnlineFolderVersion = string.Empty;

        /// <summary>
        /// The version of the database parsed upon application load
        /// </summary>
        public static string DatabaseVersion = string.Empty;

        /// <summary>
        /// Determines if this is the first time the application is loading
        /// </summary>
        /// <remarks>Done by checking if the settings file exists. If it is set to true in the application, it will be set to false again when it closes.</remarks>
        public static bool FirstLoad = false;

        /// <summary>
        /// Determines if while being the first time loading, if this is an upgrade operation to Relhax V2
        /// </summary>
        /// <remarks>Done by if FirstLoad is true and the Relhax V1 settings file exists</remarks>
        public static bool FirstLoadToV2 = false;

        /// <summary>
        /// The maximum amount that the application will be allowed to scale. 300%
        /// </summary>
        public const double MaximumDisplayScale = 3.0F;

        /// <summary>
        /// The default amount that the application will be scaled to. 100%
        /// </summary>
        public const double MinimumDisplayScale = 1.0F;

        /// <summary>
        /// The number of characters that make up a package UID
        /// </summary>
        public const int NumberUIDCharacters = 16;

        /// <summary>
        /// The array of character options that are used for generating the UID
        /// </summary>
        public const string UIDCharacters = @"abcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// The manager info zip in a program reference. Allows for multiple instances of the application to be active at the same time. Also saves milliseconds by not having to write to disk.
        /// </summary>
        public static Ionic.Zip.ZipFile ManagerInfoZipfile = null;
        #endregion

        #region Settings parsing to/from XML file
        /// <summary>
        /// Loads/serializes an xml file into a settings class based on class type
        /// </summary>
        /// <param name="xmlfile">The path to the file</param>
        /// <param name="SettingsClass">The type of the settings class to load into</param>
        /// <param name="propertiesToExclude">A string list of properties (in the class) to not look for. If none, you can pass in null</param>
        /// <param name="classInstance">The object to append the xml settings to. If the settings class is static, pass in null</param>
        /// <returns>Success if loading, false otherwise</returns>
        public static bool LoadSettings(string xmlfile, Type SettingsClass, string[] propertiesToExclude, object classInstance)
        {
            //first check if the file even exists
            if (!File.Exists(xmlfile))
            {
                Logging.Info("Xml settings file {0} does not exist, using defaults set in class '{1}'", xmlfile, SettingsClass.Name.ToString());
                return false;
            }
            //get all fields from the class
            FieldInfo[] fields = SettingsClass.GetFields();
            //get all types from the types in the class
            List<Type> typesOfTypesInClass = new List<Type>();
            foreach(FieldInfo fieldInClass in fields)
            {
                //https://stackoverflow.com/questions/5090224/reflection-get-type-of-fieldinfo-object
                Type t = fieldInClass.FieldType;
                if (!typesOfTypesInClass.Contains(t))
                    typesOfTypesInClass.Add(t);
            }
            //now we have a list of all "types" that exist in the class
            //parse the xml list
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(xmlfile);
            }
            catch (XmlException ex)
            {
                Logging.Error("Failed to load {0}, using defaults set in class{1}{2}{3}", xmlfile, SettingsClass.GetType().ToString(), Environment.NewLine, ex.ToString());
                return false;
            }
            //using child of child rather than xpath gets around the fact that the root element name has changed or can change
            XmlNodeList settings = doc.ChildNodes[0].ChildNodes;
            //legacy compatibility: if it's modpackSettings, there's some V1 bad names that need to be manually parsed
            if(SettingsClass.Equals(typeof(ModpackSettings)))
            {
                ModpackSettings.ApplyOldSettings(settings);
            }
            for (int i = 0; i < settings.Count; i++)
            {
                //verify that the setting name in xml matches a fieldInfo property in the class
                FieldInfo[] matches = fields.Where(f => f.Name.Equals(settings[i].Name)).ToArray();
                //Logging.WriteToLog(string.Empty + matches.Count() + " matches for xml setting name " + settings[i].Name, Logfiles.Application, LogLevel.Debug);
                if(matches.Count() > 1)
                {
                    throw new BadMemeException("ugh");
                }
                else if (matches.Count() == 0)
                {
                    Logging.Warning("no match for xml setting {0}", settings[i].Name);
                }
                else
                {
                    FieldInfo settingField = matches[0];
                    //we have, based on name, matched the xml property to a property in the class
                    //now set the value
                    //BUT also check to make sure the item is not on the blacklist
                    if (propertiesToExclude != null && propertiesToExclude.Contains(settingField.Name))
                    {
                        Logging.Debug("Property {0} matched to exclusion list, skipping", settingField.Name);
                        continue;
                    }
                    //get the type of the field and make sure it actually exists in the list (it should)
                    if(typesOfTypesInClass.Contains(settingField.FieldType))
                    {
                        //since the type exists, it *should* save
                        //https://stackoverflow.com/questions/2380467/c-dynamic-parse-from-system-type
                        try
                        {
                            var converter = TypeDescriptor.GetConverter(settingField.FieldType);
                            if (classInstance != null)
                                settingField.SetValue(classInstance, converter.ConvertFrom(settings[i].InnerText));
                            else
                                settingField.SetValue(SettingsClass,converter.ConvertFrom(settings[i].InnerText));
                        }
                        catch (Exception e)
                        {
                            Logging.Debug("failed to load property to memory {0}{1}{2}", settingField.Name, Environment.NewLine, e.ToString());
                        }
                    }
                    else
                    {
                        throw new BadMemeException("hmmmmmm");
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Saves/serializes a settings class file to xml
        /// </summary>
        /// <param name="xmlFile">The file to write to. If exists, it will be overwritten</param>
        /// <param name="SettingsClass">The type of the class to save values from</param>
        /// <param name="propertiesToExclude">A string list of properties (in the class) to not look for</param>
        /// <param name="classInstance">The actual instance of the class to save from</param>
        /// <returns></returns>
        public static bool SaveSettings(string xmlFile, Type SettingsClass, string[] propertiesToExclude, object classInstance)
        {
            XmlDocument doc = new XmlDocument();
            //create element called ModpackSettings
            XmlElement settingsHolder = doc.CreateElement(SettingsClass.Name);
            doc.AppendChild(settingsHolder);
            if (File.Exists(xmlFile))
                File.Delete(xmlFile);
            //if it can delete, then it can save later
            FieldInfo[] fields = SettingsClass.GetFields();
            foreach (FieldInfo field in fields)
            {
                //but skip the exclusion list
                if (propertiesToExclude != null && propertiesToExclude.Contains(field.Name))
                {
                    Logging.Debug("Xml file {0}, property {1} matched to exclusion list, skipping", xmlFile, field.Name);
                    continue;
                }
                XmlElement element = doc.CreateElement(field.Name);
                try
                {
                    if (classInstance != null)
                        element.InnerText = field.GetValue(classInstance).ToString();
                    else
                        element.InnerText = field.GetValue(SettingsClass).ToString();
                }
                catch (Exception e)
                {
                    Logging.Debug("failed to save property from memory {0}{1}{2}", field.Name, Environment.NewLine, e.ToString());
                }
                settingsHolder.AppendChild(element);
            }
            doc.Save(xmlFile);
            return true;
        }
        #endregion
    }
}
