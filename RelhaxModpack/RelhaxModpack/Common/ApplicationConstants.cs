using RelhaxModpack.Database;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Common
{
    /// <summary>
    /// Contains common fields and properties used throughout the entire application.
    /// </summary>
    public static class ApplicationConstants
    {
        #region Application Files and Folders
        /// <summary>
        /// The name of the selection file when used in the setting "save last installed selection".
        /// </summary>
        public const string LastSavedConfigFilename = "lastInstalledConfig.xml";

        /// <summary>
        /// The file in the application root directory used to unlock the "launch editor" button.
        /// </summary>
        public const string EditorLaunchFromMainWindowFilename = "EditorUnlock.txt";

        /// <summary>
        /// The filename of the V2 root database document. All category names and filenames, and version info is in this document.
        /// </summary>
        public const string BetaDatabaseV2RootFilename = "database.xml";

        /// <summary>
        /// The filename to download the latest stable or beta application zip file as for the update system.
        /// </summary>
        /// <remarks>
        /// The update system works by downloading the zip file, extracting the executable, and launching it with the same
        /// command-line arguments that were supplied to this instance.
        /// </remarks>
        /// <seealso cref="ApplicationUpdateFilenameExe"/>
        public const string ApplicationUpdateFileNameZip = "RelhaxModpack_update.zip";

        /// <summary>
        /// The filename to extract the application as when being updated via the update system.
        /// </summary>
        /// <seealso cref="ApplicationUpdateFileNameZip"/>
        public const string ApplicationUpdateFilenameExe = "RelhaxModpack_update.exe";

        /// <summary>
        /// The filename to save the self updater script as.
        /// </summary>
        /// <remarks>
        /// The server stores it as a text file that we write as a batch file.
        /// Something something downloading batch files is scary to anti-virus.
        /// </remarks>
        /// <seealso cref="RelicBatchUpdateScriptServer"/>
        public const string RelicBatchUpdateScript = "relic_self_updater.bat";

        /// <summary>
        /// The filename of the self updater script inside the manager zip file.
        /// </summary>
        /// <seealso cref="RelicBatchUpdateScript"/>
        public const string RelicBatchUpdateScriptServer = "relic_self_updater.txt";

        /// <summary>
        /// The name of the application executable when compiled with stable distribution.
        /// </summary>
        public const string ApplicationFilenameStable = "RelhaxModpack.exe";

        /// <summary>
        /// The name of the application executable when compiled with beta distribution.
        /// </summary>
        public const string ApplicationFilenameBeta = "RelhaxModpackBeta.exe";

        /// <summary>
        /// The old V1 filename to save the self updater script as.
        /// </summary>
        /// <remarks>
        /// This should only be used to check for and delete a possible old file instance from a legacy version of the application.
        /// </remarks>
        [Obsolete("Database format V1 is deprecated, please use V2 instead.")]
        public const string RelicBatchUpdateScriptOld = "RelicCopyUpdate.bat";

        /// <summary>
        /// The root filename of the list of selection files.
        /// </summary>
        /// <remarks>
        /// This file contains the display names and file names of developer selections shown in the "Developer Selections" window.
        /// </remarks>
        /// <seealso cref="RelhaxModpack.Windows.DeveloperSelectionsViewer"/>
        public const string SelectionsXml = "selections.xml";

        /// <summary>
        /// The file downloaded upon startup to check for application updates, database versions, supported clients, update scripts, and the default_checked.xml selection file.
        /// </summary>
        public const string ModInfoZip = "modInfo.dat";

        /// <summary>
        /// The name of the 64bit folder in the 'World_of_Tanks' directory.
        /// </summary>
        public const string WoT64bitFolder = "win64";

        /// <summary>
        /// The name of the 32bit folder in the 'World_of_Tanks' directory.
        /// </summary>
        public const string WoT32bitFolder = "win32";

        /// <summary>
        /// The name of the 'mods' directory, stored in the root World_of_Tanks folder.
        /// </summary>
        /// <remarks>
        /// The 'new' method for installing mods into the client. It mostly contains configuration data and mod packages, which are zip files with no compression.
        /// </remarks>
        /// <seealso cref="ResModsDir"/>
        public const string ModsDir = "mods";

        /// <summary>
        /// The name of the 'res_mods' directory, stored in the root World_of_Tanks folder.
        /// </summary>
        /// <remarks>
        /// The 'old' method for installing mods into the client. It's for legacy mods that haven't been updated to the package structure. All files are dumped into the folder and loaded as they are, to replace stock files.
        /// </remarks>
        /// <seealso cref="ModsDir"/>
        public const string ResModsDir = "res_mods";

        /// <summary>
        /// The name of the xml file in the World_of_Tanks folder used for getting the current client version information.
        /// </summary>
        /// <remarks>
        /// At the time of this writing, the version is stored as something like "". Using string splitting and trimming, the number combination before the # is the same number used for the version dirs in the mods and res_mods folders.
        /// The xpath to get the version is /version.xml/version.
        /// </remarks>
        public const string WoTVersionXml = "version.xml";

        /// <summary>
        /// The name of the installer folder that used to hold all patch instruction files in for processing.
        /// </summary>
        /// <remarks>
        /// The installer used to create this folder to write, order and process patches that would be inside the package zip files. Now, the information is stored in the database.
        /// </remarks>
        [Obsolete("This property should only be used for checking to remove leftover folders from old installations.")]
        public const string PatchFolderName = "_patch";

        /// <summary>
        /// The name of the installer folder that used to hold all shortcut instruction files in for processing.
        /// </summary>
        /// <remarks>
        /// The installer used to create this folder to write and process shortcut creation instructions for processing. Now, the information is stored in the database.
        /// </remarks>
        [Obsolete("This property should only be used for checking to remove leftover folders from old installations.")]
        public const string ShortcutFolderName = "_shortcuts";

        /// <summary>
        /// The name of the installer folder that used to hold xml unpack instruction files in for processing.
        /// </summary>
        /// <remarks>
        /// The installer used to create this folder to write and process xml unpack instructions for processing. Now, the information is stored in the database.
        /// </remarks>
        [Obsolete("This property should only be used for checking to remove leftover folders from old installations.")]
        public const string XmlUnpackFolderName = "_xmlUnPack";

        /// <summary>
        /// The name of the installer folder that used to hold atlas creation instruction files in for processing.
        /// </summary>
        /// <remarks>
        /// The installer used to create this folder to write and process atlas creation instructions for processing. Now, the information is stored in the database.
        /// </remarks>
        [Obsolete("This property should only be used for checking to remove leftover folders from old installations.")]
        public const string AtlasCreationFoldername = "_atlases";

        /// <summary>
        /// The name of the installer folder that is used to temporarily extract font files to for installation.
        /// </summary>
        /// <remarks>
        /// The installer uses this folder to list all fonts to install via the application fontReg.exe
        /// </remarks>
        public const string FontsToInstallFoldername = "_fonts";

        /// <summary>
        /// The name of the temporary install folder that used to hold database manager readme files. The end user does not need this folder
        /// and will be deleted at the end of the installation.
        /// </summary>
        /// <remarks>
        /// The purpose of this folder has been superseded by the "InternalNotes" field in a package's xml entry.
        /// </remarks>
        /// <seealso cref="Database.DatabasePackage.InternalNotes"/>
        public const string ReadmeFromZipfileFolderName = "_readme";

        /// <summary>
        /// The name of the temporary install folder that holds the auto update information of the database editor.
        /// </summary>
        /// <remarks>This is legacy and not used. It should be only used to check for and delete from legacy installations.</remarks>
        public const string AutoUpdateZipFolderName = "_autoUpdate";

        /// <summary>
        /// The filename of the selection file used to select default packages upon loading of the package selection list.
        /// </summary>
        /// <remarks>When loading from a stable or beta database distribution (so not test mode), it will auto-check these when loading the package selection list window.</remarks>
        public const string DefaultCheckedSelectionfile = "default_checked.xml";

        /// <summary>
        /// The filename of the xml document inside the manager info archive file containing the list of supported WoT clients and their respective 'online folder' that holds all the zip packages.
        /// </summary>
        /// <remarks>
        /// If the user attempts to install to a version of WoT that isn't supported, it will use the last one listed in the document, assuming that it is the most recent.
        /// This is also used for export mode to select what version of the WoT client to export an installation for.
        /// </remarks>
        public const string SupportedClients = "supported_clients.xml";

        /// <summary>
        /// The filename of the xml document inside the manager info archive file containing manager version information and the latest supported database tag.
        /// </summary>
        /// <remarks>This file is used to determine if the application is out of date.</remarks>
        public const string ManagerVersion = "manager_version.xml";

        /// <summary>
        /// The name of the pmod log file.
        /// </summary>
        public const string PmodLog = "pmod.log";

        /// <summary>
        /// The name of the xvm log file.
        /// </summary>
        public const string XvmLog = "xvm.log";

        /// <summary>
        /// The name of the WoT python debug log file.
        /// </summary>
        public const string PythonLog = "python.log";

        /// <summary>
        /// The name of the logs folder used for WG CEF browser and for storing installer log files.
        /// </summary>
        public const string LogsFolder = "logs";

        /// <summary>
        /// URL to get the latest version of the .NET Framework.
        /// </summary>
        public const string DotNetFrameworkLatestDownloadURL = "https://dotnet.microsoft.com/download/dotnet-framework";

        /// <summary>
        /// The Startup root path of the application. Does not include the application name.
        /// </summary>
        public static readonly string ApplicationStartupPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// The absolute path of the application zip file and zip database file folder.
        /// </summary>
        public static readonly string RelhaxDownloadsFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxDownloads");

        /// <summary>
        /// The old absolute path of the application zip file and zip database file folder.
        /// </summary>
        /// [Obsolete]
        public static readonly string RelhaxDownloadsFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxDownloads");

        /// <summary>
        /// The absolute path of the application mod backup folder.
        /// </summary>
        public static readonly string RelhaxModBackupFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxModBackup");

        /// <summary>
        /// The old absolute path of the application mod backup folder.
        /// </summary>
        /// [Obsolete]
        public static readonly string RelhaxModBackupFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxModBackup");

        /// <summary>
        /// The absolute path of the application user selections folder. Default location.
        /// </summary>
        public static readonly string RelhaxUserSelectionsFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxUserSelections");

        /// <summary>
        /// The old absolute path of the application user selections folder. Old Default location.
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxUserSelectionsFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxUserConfigs");

        /// <summary>
        /// The absolute path of the application folder where users can place custom mod zip files.
        /// </summary>
        public static readonly string RelhaxUserModsFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxUserMods");

        /// <summary>
        /// The old absolute path of the application folder where users can place custom mod zip files.
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxUserModsFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxUserMods");

        /// <summary>
        /// The absolute path of the application temporary folder.
        /// </summary>
        public static readonly string RelhaxTempFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxTemp");

        /// <summary>
        /// The old absolute path of the application temporary folder.
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxTempFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxTemp");

        /// <summary>
        /// The absolute path of the application 3rd party dll references folder. Currently used to hold atlas file libraries.
        /// </summary>
        /// <remarks>The atlas creation libraries are extracted to this location to be used to create the map files.</remarks>
        public static readonly string RelhaxLibrariesFolderPath = Path.Combine(ApplicationStartupPath, "RelhaxLibraries");

        /// <summary>
        /// The old absolute path of the application 3rd party dll references folder.
        /// </summary>
        [Obsolete]
        public static readonly string RelhaxLibrariesFolderPathOld = Path.Combine(ApplicationStartupPath, "RelHaxLibraries");

        /// <summary>
        /// The location of the manager info zip file. Contains several xml files with database and client definitions.
        /// </summary>
        [Obsolete("Do not use this unless for file deleting, here only for legacy purposes. File is no longer created.")]
        public static readonly string ManagerInfoDatFile = Path.Combine(RelhaxTempFolderPath, "managerInfo.dat");

        /// <summary>
        /// The absolute path of the selection file used for saving last saved selection (when the setting is enabled and the user presses install).
        /// </summary>
        public static readonly string LastInstalledConfigFilepath = Path.Combine(RelhaxUserSelectionsFolderPath, LastSavedConfigFilename);

        /// <summary>
        /// The WoT 64bit folder name with the folder separator before it.
        /// </summary>
        public static readonly string WoT64bitFolderWithSlash = Path.DirectorySeparatorChar + WoT64bitFolder;

        /// <summary>
        /// The WoT 32bit folder name with the folder separator before it.
        /// </summary>
        public static readonly string WoT32bitFolderWithSlash = Path.DirectorySeparatorChar + WoT32bitFolder;

        /// <summary>
        /// The location of the WoT app data folder parsed.
        /// </summary>
        public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wargaming.net", "WorldOfTanks");

        /// <summary>
        /// The absolute path of the V2 settings file used for saving ModpackSettings.
        /// </summary>
        /// <seealso cref="Settings.ModpackSettings"/>
        public static readonly string RelhaxSettingsFilepath = Path.Combine(ApplicationStartupPath, ModpackSettings.SettingsFilename);

        /// <summary>
        /// The absolute path of the Relhax main log file.
        /// </summary>
        /// <remarks>A custom feature window (for example, the editor) will have its own log file to use, which enables multiple feature window instances open from the same exe at the same time.</remarks>
        public static readonly string RelhaxLogFilepath = Path.Combine(ApplicationStartupPath, Logging.ApplicationLogFilename);

        /// <summary>
        /// The list of installer folders in the root {WoT} directory to cleanup after an installation.
        /// </summary>
        public static readonly string[] FoldersToCleanup = new string[]
        {
#pragma warning disable CS0618 // Type or member is obsolete
            PatchFolderName,
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
            ShortcutFolderName,
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
            XmlUnpackFolderName,
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
            AtlasCreationFoldername,
#pragma warning restore CS0618 // Type or member is obsolete
            FontsToInstallFoldername,
            ReadmeFromZipfileFolderName,
            AutoUpdateZipFolderName
        };

        /// <summary>
        /// The absolute path to the md5 hash zip file download database file.
        /// </summary>
        public static readonly string MD5HashDatabaseXmlFile = Path.Combine(RelhaxDownloadsFolderPath, "MD5HashDatabase.xml");

        /// <summary>
        /// Array of all Modpack created folders in the application directory.
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
        #endregion

        #region URLs
        /// <summary>
        /// The escaped constant URL of the stable database on the server, escaped with the 'dbVersion' macro.
        /// </summary>
        public const string BigmodsDatabaseRootEscaped = "https://bigmods.relhaxmodpack.com/RelhaxModpack/resources/database/{dbVersion}/";

        /// <summary>
        /// The default download mirror located in Texas USA, escaped with the 'onlineFolder' macro.
        /// </summary>
        /// <remarks>'onlineFolder' is a 3 digit number representing the major release version of WoT e.g. 1.7.0.</remarks>
        public const string StartAddressMirrorUsaDefault = @"https://bigmods.relhaxmodpack.com/WoT/{onlineFolder}/";

        /// <summary>
        /// The download mirror located in Germany.
        /// </summary>
        /// <remarks>'onlineFolder' is a 3 digit number representing the major release version of WoT e.g. 1.7.0.</remarks>
        public const string StartAddressMirrorDe = @"https://relhax.clanverwaltung.de/filedepot/files/{onlineFolder}/";

        /// <summary>
        /// A read-only list of all download mirrors in this application.
        /// </summary>
        /// <remarks>The order of this list matters as the index is used to store the user choice.</remarks>
        public static readonly string[] DownloadMirrors = new string[]
        {
            StartAddressMirrorUsaDefault,
            StartAddressMirrorDe
        };

        /// <summary>
        /// The URL of the V2 beta database root folder, escaped with the 'branch' macro.
        /// </summary>
        /// <remarks>'branch' is a name of a github branch on the RelhaxModpackDatabase repo.</remarks>
        public const string BetaDatabaseV2FolderURLEscaped = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/{branch}/latest_database/";

        /// <summary>
        /// The API URL to return a json format document of the current branches in the repository.
        /// </summary>
        public const string BetaDatabaseBranchesURL = "https://api.github.com/repos/Willster419/RelhaxModpackDatabase/branches";

        /// <summary>
        /// The URL of the V2 manager info zip file.
        /// </summary>
        public const string ManagerInfoURLBigmods = "https://bigmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat";

        /// <summary>
        /// The URL to the location of the latest stable version of the application as a zip file.
        /// </summary>
        public const string ApplicationUpdateURL = "https://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.zip";

        /// <summary>
        /// The URL to the location of the latest beta version of the application as a zip file.
        /// </summary>
        public const string ApplicationBetaUpdateURL = "https://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpackBeta.zip";

        /// <summary>
        /// The root URL of the V2 selection files location.
        /// </summary>
        public const string SelectionsRoot = "https://raw.githubusercontent.com/Willster419/RelhaxModpackDatabase/master/selection_files/";

        /// <summary>
        /// The URL path of the latest application stable release notes.
        /// </summary>
        public const string ApplicationNotesStableUrl = "https://github.com/Willster419/RelhaxModpack/raw/master/RelhaxModpack/RelhaxModpack/bin/Debug/release_notes_stable.txt";

        /// <summary>
        /// The URL path of the latest application beta release notes.
        /// </summary>
        public const string ApplicationNotesBetaUrl = "https://github.com/Willster419/RelhaxModpack/raw/staging/RelhaxModpack/RelhaxModpack/bin/Debug/release_notes_beta.txt";

        /// <summary>
        /// The URL path of the latest V2 database release notes.
        /// </summary>
        public const string DatabaseNotesUrl = "https://github.com/Willster419/RelhaxModpackDatabase/raw/master/resources/databaseUpdate.txt";
        #endregion

        #region Application and Database properties
        /// <summary>
        /// The root of the xpath string used to get metadata information from the database.
        /// </summary>
        public const string DatabaseMetadataXpath = "/modInfoAlpha.xml/@";

        /// <summary>
        /// The xpath string to get the onlineFolder attribute from the document root.
        /// </summary>
        public const string DatabaseOnlineFolderXpath = DatabaseMetadataXpath + DatabaseManager.WoTOnlineFolderVersionXmlString;

        /// <summary>
        /// The xpath string to get the database version info attribute from the document root.
        /// </summary>
        public const string DatabaseClientVersionXpath = DatabaseMetadataXpath + DatabaseManager.WoTClientVersionXmlString;

        /// <summary>
        /// The xpath string used in the database xml for specifying the version of the document.
        /// </summary>
        /// <remarks>
        /// Document versioning allows for structural changes not just for the document (how to load it), but also for how to load it in software.
        /// It enables the specification of code for loading and saving of specific versions of the document.
        /// </remarks>
        /// <seealso cref="DatabaseSchemaVersionXpath"/>
        public const string DatabaseDocumentVersionXpath = DatabaseMetadataXpath + DatabaseManager.DocumentVersionXmlString;

        /// <summary>
        /// The xpath string used in the database xml for specifying the serialization format of components.
        /// </summary>
        public const string DatabaseSchemaVersionXpath = DatabaseMetadataXpath + DatabaseManager.SchemaVersionXmlString;

        /// <summary>
        /// The old V2 selection file format for saving the user's selection preferences.
        /// </summary>
        [Obsolete("Selection file version 2.0 is deprecated, should not be used for saving.")]
        public const string ConfigFileVersion2V0 = "2.0";

        /// <summary>
        /// The latest selection file format for saving the user's selection preferences.
        /// </summary>
        public const string ConfigFileVersion3V0 = "3.0";

        /// <summary>
        /// The name of the WoT process used for detecting if an instance of the client running.
        /// </summary>
        public const string WoTProcessName = "WorldOfTanks";

        /// <summary>
        /// The xpath to the version information used by the modpack to determine the WoT client version.
        /// </summary>
        public const string WoTVersionXmlXpath = "/version.xml/version";

        /// <summary>
        /// The current distribution version of the application.
        /// Alpha should NEVER be built for public distribution unless direct testing!
        /// </summary>
        public const ApplicationVersions ApplicationVersion = ApplicationVersions.Alpha;

        /// <summary>
        /// The amount so space characters to line up a continued log entry without the date/time.
        /// </summary>
        public const string LogSpacingLineup = "                          ";

        /// <summary>
        /// The maximum amount that the application will be allowed to scale. 300%.
        /// </summary>
        public const double MaximumDisplayScale = 3.0F;

        /// <summary>
        /// The default amount that the application will be scaled to. 100%.
        /// </summary>
        public const double MinimumDisplayScale = 1.0F;

        /// <summary>
        /// The number of characters that make up a package UID entry.
        /// </summary>
        public const int NumberUIDCharacters = 16;

        /// <summary>
        /// The array of character options that are used for generating a package UID entry.
        /// </summary>
        public const string UIDCharacters = @"abcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// The number of logical processors (threads) detected on the system. Used to make "n" installation threads for faster extraction.
        /// </summary>
        public static readonly int NumLogicalProcesors = Environment.ProcessorCount;

        /// <summary>
        /// The minimum release value of the user's installed .NET framework to use the modpack.
        /// </summary>
        /// <remarks>It varies for OS. For example:
        /// On Windows 10 May 2019 Update and Windows 10 November 2019 Update: 528040.
        /// On Windows 10 May 2020 Update: 528372.
        /// On all other Windows operating systems(including other Windows 10 operating systems): 528049.
        /// See: https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed </remarks>
        public const int MinimumDotNetFrameworkVersionRequired = 528040;
        #endregion
    }
}
