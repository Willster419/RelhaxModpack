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
        #region Constants
        /// <summary>
        /// The Startup root path of the application. Does not include the application name
        /// </summary>
        public static readonly string ApplicationStartupPath = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// The absolute name of the application settings file
        /// </summary>
        public const string ModpackSettingsFileName = "RelhaxSettingsV2.xml";

        public const string OldModpackSettingsFilename = "RelHaxSettings.xml";

        public const string UISettingsFileName = "UISettings.xml";

        public const string ThirdPartySettingsFileName = "ThirdPartySettings.xml";

        public const string EditorSettingsFilename = "EditorSettings.xml";

        public const string PatcherSettingsFilename = "PatchSettings.xml";

        public const string LastSavedConfigFilename = "lastInstalledConfig.xml";

        public const string EditorLaunchFromMainWindowFilename = "EditorUnlock.txt";

        public static string DefaultStartAddress = @"http://bigmods.relhaxmodpack.com/WoT/{onlineFolder}/";

        public static string WotmodsDatabaseDatRoot = @"http://wotmods.relhaxmodpack.com/WoT/{onlineFolder}/";

        public static string DefaultEndAddress = @"";

        public const string BetaDatabaseV1URL = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/master/modInfo.xml";

        public const string BetaDatabaseV2FolderURL = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/{branch}/latest_database/";

        public const string BetaDatabaseV2RootFilename = "database.xml";

        public const string DatabaseOnlineFolderXpath = "//modInfoAlpha.xml/@onlineFolder";

        public const string DatabaseOnlineVersionXpath = "//modInfoAlpha.xml/@version";

        public const string BetaDatabaseBranchesURL = "https://api.github.com/repos/Willster419/RelhaxModpackDatabase/branches";

        public const string ManagerInfoURL = "http://wotmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat";

        public const string ManagerInfoURLBigmods = "http://bigmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat";

        //the config file version for saving the user's selection preferences
        public const string ConfigFileVersion = "2.0";
        public const string ApplicationUpdateURL = "http://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.zip";
        public const string ApplicationBetaUpdateURL = "http://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpackBeta.zip";
        public const string ApplicationUpdateFileName = "RelhaxModpack_update.zip";
        public const string RelicBatchUpdateScript = "relic_self_updater.bat";
        public const string RelicBatchUpdateScriptServer = "relic_self_updater.txt";
        public const string RelicBatchUpdateScriptOld = "RelicCopyUpdate.bat";

        public const string SelectionsRoot = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/master/selection_files/";
        public const string SelectionsXml = "selections.xml";

        public const string ApplicationNotesStableUrl = "https://raw.githubusercontent.com/Willster419/RelhaxModpack/master/RelicModManager/bin/Debug/releaseNotes.txt";
        public const string ApplicationNotesBetaUrl = "https://raw.githubusercontent.com/Willster419/RelhaxModpack/master/RelicModManager/bin/Debug/releaseNotes_beta.txt";
        public const string DatabaseNotesUrl = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/master/databaseUpdate.txt";

        /// <summary>
        /// The current distribution version of the application
        /// Alpha should NEVER be built for public distribution unless direct testing!
        /// </summary>
        public const ApplicationVersions ApplicationVersion = ApplicationVersions.Alpha;

        public static bool TrueAlpha = false;

        public static readonly string RelhaxDownloadsFolder = Path.Combine(ApplicationStartupPath, "RelhaxDownloads");

        public static readonly string RelhaxModBackupFolder = Path.Combine(ApplicationStartupPath, "RelhaxModBackup");

        public static readonly string RelhaxUserConfigsFolder = Path.Combine(ApplicationStartupPath, "RelhaxUserConfigs");

        public static readonly string RelhaxUserModsFolder = Path.Combine(ApplicationStartupPath, "RelhaxUserMods");

        public static readonly string RelhaxTempFolder = Path.Combine(ApplicationStartupPath, "RelhaxTemp");

        public static readonly string RelhaxLibrariesFolder = Path.Combine(ApplicationStartupPath, "RelhaxLibraries");

        public const string PatchFolderName = "_patch";

        public const string ShortcutFolderName = "_shortcuts";

        public const string XmlUnpackFolderName = "_xmlUnPack";

        public const string AtlasCreationFoldername = "_atlases";

        public const string FontsToInstallFoldername = "_fonts";

        public const string ReadmeFromZipfileFolderName = "_readme";

        public static readonly string[] FoldersToCleanup = new string[]
        {
            PatchFolderName,
            ShortcutFolderName,
            XmlUnpackFolderName,
            AtlasCreationFoldername,
            FontsToInstallFoldername,
            ReadmeFromZipfileFolderName
        };

        public static readonly string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolder, "MD5HashDatabase.xml");

        public const string DefaultCheckedSelectionfile = "default_checked.xml";

        public static readonly string ManagerInfoDatFile = Path.Combine(RelhaxTempFolder, "managerInfo.dat");

        public static readonly string LastInstalledConfigFilepath = Path.Combine(RelhaxUserConfigsFolder, LastSavedConfigFilename);

        public static readonly string RelhaxSettingsFilepath = Path.Combine(ApplicationStartupPath, ModpackSettingsFileName);

        public static readonly string RelhaxLogFilepath = Path.Combine(ApplicationStartupPath, Logging.ApplicationLogFilename);

        public const string WoTProcessName = "WorldOfTanks";

        public static readonly string[] FoldersToCheck = new string[]
        {
            RelhaxDownloadsFolder,
            RelhaxModBackupFolder,
            RelhaxUserConfigsFolder,
            RelhaxUserModsFolder,
            RelhaxTempFolder,
            RelhaxLibrariesFolder
        };

        /// <summary>
        /// default maximum sprite sheet width (pixels)
        /// </summary>
        public const int DefaultMaximumSheetWidth = 4096;

        /// <summary>
        /// default maximum sprite sheet height (pixels)
        /// </summary>
        public const int DefaultMaximumSheetHeight = 4096;

        /// <summary>
        /// default image padding
        /// </summary>
        public const int DefaultImagePadding = 1;

        public const string SupportedClients = "supported_clients.xml";
        public const string ManagerVersion = "manager_version.xml";

        public static readonly int NumLogicalProcesors = Environment.ProcessorCount;

        public const string LogSpacingLinup = "                          ";
        #endregion

        #region Statics
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

        public static void ProcessFirstLoadings()
        {
            FirstLoad = !File.Exists(ModpackSettingsFileName) && !File.Exists(OldModpackSettingsFilename);
            FirstLoadToV2 = !File.Exists(ModpackSettingsFileName) && File.Exists(OldModpackSettingsFilename);
        }

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
