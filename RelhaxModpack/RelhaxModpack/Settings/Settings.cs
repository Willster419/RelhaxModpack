using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.ComponentModel;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all generic constants and statics used throughout the application
    /// </summary>
    public static class Settings
    {
        #region filenames and paths
        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// The name of the application settings file
        /// </summary>
        public const string ModpackSettingsFileName = "RelhaxSettingsV2.xml";

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
        /// The name of the selection file when used in the setting "save last installed selection"
        /// </summary>
        public const string LastSavedConfigFilename = "lastInstalledConfig.xml";

        /// <summary>
        /// The file in the application root directory used to unlock the "launch editor" button
        /// </summary>
        public const string EditorLaunchFromMainWindowFilename = "EditorUnlock.txt";

        /// <summary>
        /// The default starting address of the location of mod packages (start + zip + end)
        /// </summary>
        public const string DefaultStartAddress = @"http://bigmods.relhaxmodpack.com/WoT/{onlineFolder}/";

        /// <summary>
        /// The old default starting address of the location of mod packages (start + zip + end)
        /// </summary>
        [Obsolete("Database format V1 is deprecated, please use V2 instead.")]
        public const string WotmodsDatabaseDatRoot = @"http://wotmods.relhaxmodpack.com/WoT/{onlineFolder}/";

        /// <summary>
        /// The default end address of the location of mod packages (start + zip + end)
        /// </summary>
        public const string DefaultEndAddress = @"";

        /// <summary>
        /// The URL to the v1 legacy beta database
        /// </summary>
        [Obsolete("Database format V1 is deprecated, please use V2 instead.")]
        public const string BetaDatabaseV1URL = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/master/modInfo.xml";

        /// <summary>
        /// The URL of the V2 beta database root folder. (NOTE: database V2 is multiple files)
        /// </summary>
        public const string BetaDatabaseV2FolderURL = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/{branch}/latest_database/";

        /// <summary>
        /// The filename of the V2 root database document. All category names and filenames, and version info is in this document
        /// </summary>
        public const string BetaDatabaseV2RootFilename = "database.xml";

        /// <summary>
        /// The xpath string to get the onlineFolder attribute from the document root
        /// </summary>
        public const string DatabaseOnlineFolderXpath = "//modInfoAlpha.xml/@onlineFolder";

        /// <summary>
        /// The xpath string to get the database version info attribute from the document root
        /// </summary>
        public const string DatabaseOnlineVersionXpath = "//modInfoAlpha.xml/@version";

        /// <summary>
        /// The API URL to return a json format document of the current branches in the repository
        /// </summary>
        public const string BetaDatabaseBranchesURL = "https://api.github.com/repos/Willster419/RelhaxModpackDatabase/branches";

        /// <summary>
        /// The URL of the V1 manager info zip file
        /// </summary>
        [Obsolete("Database format V1 is deprecated, please use V2 instead.")]
        public const string ManagerInfoURL = "http://wotmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat";

        /// <summary>
        /// The URL of the V2 manager info zip file
        /// </summary>
        public const string ManagerInfoURLBigmods = "http://bigmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat";

        /// <summary>
        /// the latest config file version for saving the user's selection preferences
        /// </summary>
        public const string ConfigFileVersion = "2.0";

        /// <summary>
        /// The URL to the location of the latest stable version of the application as a zip file
        /// </summary>
        public const string ApplicationUpdateURL = "http://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.zip";

        /// <summary>
        /// The URL to the location of the latest beta version of the application as a zip file
        /// </summary>
        public const string ApplicationBetaUpdateURL = "http://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpackBeta.zip";

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
        /// The old V1 filename to save the self updater script as
        /// </summary>
        [Obsolete("Database format V1 is deprecated, please use V2 instead.")]
        public const string RelicBatchUpdateScriptOld = "RelicCopyUpdate.bat";

        /// <summary>
        /// The root URL of the V2 selection files location
        /// </summary>
        public const string SelectionsRoot = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/master/selection_files/";

        /// <summary>
        /// The root filename of the list of selection files
        /// </summary>
        public const string SelectionsXml = "selections.xml";

        /// <summary>
        /// The URL path of the latest application stable release notes
        /// </summary>
        public const string ApplicationNotesStableUrl = "https://raw.githubusercontent.com/Willster419/RelhaxModpack/master/RelicModManager/bin/Debug/releaseNotes.txt";

        /// <summary>
        /// The URL path of the latest application beta release notes
        /// </summary>
        public const string ApplicationNotesBetaUrl = "https://raw.githubusercontent.com/Willster419/RelhaxModpack/master/RelicModManager/bin/Debug/releaseNotes_beta.txt";

        /// <summary>
        /// The URL path of the latest V2 database release notes
        /// </summary>
        public const string DatabaseNotesUrl = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/master/databaseUpdate.txt";

        /// <summary>
        /// The absolute path of the application zip file and zip database file folder
        /// </summary>
        public static readonly string RelhaxDownloadsFolder = Path.Combine(ApplicationStartupPath, "RelhaxDownloads");

        /// <summary>
        /// The absolute path of the application mod backup folder
        /// </summary>
        public static readonly string RelhaxModBackupFolder = Path.Combine(ApplicationStartupPath, "RelhaxModBackup");

        /// <summary>
        /// The absolute path of the application user selections folder. Default location 
        /// </summary>
        public static readonly string RelhaxUserSelectionsFolder = Path.Combine(ApplicationStartupPath, "RelhaxUserSelections");

        /// <summary>
        /// The old absolute path of the application user selections folder. Old Default location
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxUserConfigsFolderOld = Path.Combine(ApplicationStartupPath, "RelhaxUserSelections");

        /// <summary>
        /// The absolute path of the application folder where users can place custom mod zip files
        /// </summary>
        public static readonly string RelhaxUserModsFolder = Path.Combine(ApplicationStartupPath, "RelhaxUserMods");

        /// <summary>
        /// The absolute path of the application temporary folder
        /// </summary>
        public static readonly string RelhaxTempFolder = Path.Combine(ApplicationStartupPath, "RelhaxTemp");

        /// <summary>
        /// The absolute path of the application 3rd party dll references folder. Currently used to hold atlas file libraries
        /// </summary>
        public static readonly string RelhaxLibrariesFolder = Path.Combine(ApplicationStartupPath, "RelhaxLibraries");

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
        /// The list of installer folders in the root {WoT} directory to cleanup after an installation
        /// </summary>
        public static readonly string[] FoldersToCleanup = new string[]
        {
            PatchFolderName,
            ShortcutFolderName,
            XmlUnpackFolderName,
            AtlasCreationFoldername,
            FontsToInstallFoldername,
            ReadmeFromZipfileFolderName
        };

        /// <summary>
        /// The absolute path to the md5 hash zip file download database file
        /// </summary>
        public static readonly string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "MD5HashDatabase.xml");

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

        public static readonly string ManagerInfoDatFile = Path.Combine(RelhaxTempFolder, "managerInfo.dat");

        public static readonly string LastInstalledConfigFilepath = Path.Combine(RelhaxUserSelectionsFolder, LastSavedConfigFilename);

        public static readonly string RelhaxSettingsFilepath = Path.Combine(ApplicationStartupPath, ModpackSettingsFileName);

        public static readonly string RelhaxLogFilepath = Path.Combine(ApplicationStartupPath, Logging.ApplicationLogFilename);

        public const string WoTProcessName = "WorldOfTanks";

        public static readonly string[] FoldersToCheck = new string[]
        {
            RelhaxDownloadsFolder,
            RelhaxModBackupFolder,
            RelhaxUserSelectionsFolder,
            RelhaxUserModsFolder,
            RelhaxTempFolder,
            RelhaxLibrariesFolder
        };

        public static readonly int NumLogicalProcesors = Environment.ProcessorCount;

        public const string LogSpacingLineup = "                          ";
        #endregion

        #region database properties

        #endregion

        #region application and installer properties

        /// <summary>
        /// The current distribution version of the application
        /// Alpha should NEVER be built for public distribution unless direct testing!
        /// </summary>
        public const ApplicationVersions ApplicationVersion = ApplicationVersions.Alpha;

        public static bool TrueAlpha = false;
        //file and folder macro locations
        public static string AppDataFolder = "";
        public static string WoTDirectory = "";
        //version informations
        public static string WoTClientVersion = "";
        public static string WoTModpackOnlineFolderVersion = "";
        public static string DatabaseVersion = "";
        public static bool FirstLoad = false;
        public static bool FirstLoadToV2 = false;
        public const double MaximumDisplayScale = 3.0F;
        public const double MinimumDisplayScale = 1.0F;
        #endregion

        public static Ionic.Zip.ZipFile ModInfoZipfile = null;

        #region Settings parsing to/from XML file

        public static bool LoadSettings(string xmlfile, Type SettingsClass, string[] propertiesToExclude, object classInstance)
        {
            //first check if the file even exists
            if (!File.Exists(xmlfile))
            {
                Logging.Warning("Xml settings file {0} does not exist, using defaults set in class{1}{2}", xmlfile, SettingsClass.GetType().ToString(), Environment.NewLine);
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
                //Logging.WriteToLog("" + matches.Count() + " matches for xml setting name " + settings[i].Name, Logfiles.Application, LogLevel.Debug);
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
                    Logging.Debug("XML file {0}, property {1} matched to exclusion list, skipping", xmlFile, field.Name);
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
