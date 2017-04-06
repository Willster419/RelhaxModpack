using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace RelhaxModpack
{
    //A static class to exist throughout the entire application life, will always have translations
    public static class Translations
    {
        //Enumerator to determine which translated string to return
        public enum Languages {English = 0, German = 1};
        public static Languages language = Languages.English;//set it to this default
        public static Hashtable english = new Hashtable();
        public static Hashtable german = new Hashtable();
        //load hashes on application startup
        //Translations.loadHashes();
        
        public static string getTranslatedString(string componetName)
        {
            if (language == Languages.English)
            {
                if (english.Contains(componetName))
                {
                    return (string)english[componetName];
                }
            }
            else if (language == Languages.German)
            {
                if (german.Contains(componetName))
                {
                    return (string)german[componetName];
                }
            }
            Settings.appendToLog("ERROR: no value in language hash for key: " + componetName + ": Language: " + language);
            return componetName;
        }
        //method to load each translated string based on which language is selected
        public static void loadHashes()
        {
            //Syntax is as follows:
            //languageName.Add("componetName","TranslatedString");
            //Section: MainWindow
            //Componet: installRelhaxMod
            //The button for installing the modpack
            english.Add("installRelhaxMod","Install Relhax Modpack");
            german.Add("installRelhaxMod","Install Relhax Modpack");
            
            //Componet: uninstallRelhaxMod
            //
            english.Add("uninstallRelhaxMod","Uninstall Relhax Modpack");
            german.Add("uninstallRelhaxMod","Uninstall Relhax Modpack");
            
            //Componet: forceManuel
            //
            english.Add("forceManuel","Force manual game detection");
            german.Add("forceManuel","Force manual game detection");

            //Componet: forceManuel
            //
            english.Add("languageSelectionGB", "Language Selection");
            german.Add("languageSelectionGB", "Language Selections");
            
            //Componet: formPageLink
            //
            english.Add("formPageLink","View Modpack Form Page");
            german.Add("formPageLink","View Modpack Form Page");

            //Componet: saveUserDataCB
            //
            english.Add("saveUserDataCB", "Save user data");
            german.Add("saveUserDataCB", "Save user data");

            //Componet: cleanInstallCB
            //
            english.Add("cleanInstallCB","Clean Installation (Recommended)");
            german.Add("cleanInstallCB","Clean Installation (Recommended)");
            
            //Componet: cancerFontCB
            //
            english.Add("cancerFontCB","Comic Sans Font");
            german.Add("cancerFontCB","Comic Sans Font");
            
            //Componet: backupModsCheckBox
            //
            english.Add("backupModsCheckBox","Backup current mods folder");
            german.Add("backupModsCheckBox","Backup current mods folder");
            
            //Componet: settingsGroupBox
            //
            english.Add("settingsGroupBox","RelHax ModPack Settings");
            german.Add("settingsGroupBox","RelHax ModPack Settings");
            
            //Componet: darkUICB
            //
            english.Add("darkUICB","Dark UI");
            german.Add("darkUICB","Dark UI");
            
            //Componet: cleanUninstallCB
            //
            english.Add("cleanUninstallCB","Clean uninstallation");
            german.Add("cleanUninstallCB","Clean uninstallation");
            
            //Componet: saveLastInstallCB
            //
            english.Add("saveLastInstallCB","Save last install\'s config");
            german.Add("saveLastInstallCB","Save last install\'s config");
            
            //Componet: largerFontButton
            //
            english.Add("largerFontButton","Larger Font");
            german.Add("largerFontButton","Larger Font");
            
            //Componet: loadingImageGroupBox
            //
            english.Add("loadingImageGroupBox","Preview Loading Image");
            german.Add("loadingImageGroupBox","Preview Loading Image");
            
            //Componet: standardImageRB
            //
            english.Add("standardImageRB","Standard");
            german.Add("standardImageRB","Standard");
            
            //Componet: findBugAddModLabel
            //
            english.Add("findBugAddModLabel","Find a bug? Want a mod added?");
            german.Add("findBugAddModLabel","Find a bug? Want a mod added?");
            
            //Componet: cancelDownloadButton
            //
            english.Add("cancelDownloadButton","Cancel Download");
            german.Add("cancelDownloadButton","Cancel Download");
            
            //Section: FirstLoadHelper
            //Componet: helperText
            //
            english.Add("helperText","Welcome to the RelHax Modpack! I have tried to make the modpack as straight-forward as possible, but questions may still arise. Hover over (or right click) a setting to have it explained. You won't see this dialog box again, unless you delete the settings xml file.");
            german.Add("helperText","Welcome to the RelHax Modpack! I have tried to make the modpack as straight-forward as possible, but questions may still arise. Hover over (or right click) a setting to have it explained. You won't see this dialog box again, unless you delete the settings xml file.");
            
            //Section: ModSelectionList
            //Componet: continueButton
            //
            english.Add("continueButton","continue");
            german.Add("continueButton","continue");
            
            //Componet: cancelButton
            //
            english.Add("cancelButton","cancel");
            german.Add("cancelButton","cancel");
            
            //Componet: helpLabel
            //
            english.Add("helpLabel","right-click a mod name to preview it");
            german.Add("helpLabel","right-click a mod name to preview it");
            
            //Componet: loadConfigButton
            //
            english.Add("loadConfigButton","load prefrence");
            german.Add("loadConfigButton","load prefrence");
            
            //Componet: saveConfigButton
            //
            english.Add("saveConfigButton","save prefrence");
            german.Add("saveConfigButton","save prefrence");
            
            //Componet: label2
            //
            english.Add("label2","\"*\" tab indicates single selection tab");
            german.Add("label2","\"*\" tab indicates single selection tab");
            
            //Componet: clearSelectionsButton
            //
            english.Add("clearSelectionsButton","clear selections");
            german.Add("clearSelectionsButton","clear selections");
            
            //Componet: readingDatabase
            //
            english.Add("readingDatabase","Reading Database");
            german.Add("readingDatabase","Reading Database");
            
            //Componet: buildingUI
            //
            english.Add("buildingUI","Building UI");
            german.Add("buildingUI","Building UI");
            
            //Section: Preview
            //Componet: nextPicButton
            //
            english.Add("nextPicButton","next");
            german.Add("nextPicButton","next");
            
            //Componet: previousPicButton
            //
            english.Add("previousPicButton","previous");
            german.Add("previousPicButton","previous");
            
            //Componet: devLinkLabel
            //
            english.Add("devLinkLabel","Developer Website");
            german.Add("devLinkLabel","Developer Website");
            
            //Section: VersionInfo
            //Componet: updateAcceptButton
            //
            english.Add("updateAcceptButton","yes");
            german.Add("updateAcceptButton","yes");
            
            //Componet: updateDeclineButton
            //
            english.Add("updateDeclineButton","no");
            german.Add("updateDeclineButton","no");
            
            //Componet: newVersionAvailableLabel
            //
            english.Add("newVersionAvailableLabel","New Version Available");
            german.Add("newVersionAvailableLabel","New Version Available");
            
            //Componet: updateQuestionLabel
            //
            english.Add("updateQuestionLabel","Update?");
            german.Add("updateQuestionLabel","Update?");
            
            //Componet: problemsUpdatingLabel
            //
            english.Add("problemsUpdatingLabel","If you are having problems updating, please");
            german.Add("problemsUpdatingLabel","If you are having problems updating, please");
            
            //Componet: 
            //
            english.Add("clickHereUpdateLabel","click here.");
            german.Add("clickHereUpdateLabel","click here.");
            
            //Section: PleaseWait
            //Componet: label1
            //
            english.Add("label1","Loading...please wait...");
            german.Add("label1","Loading...please wait...");
            
            //Section: Messages of MainWindow
            //Componet: 
            //
            english.Add("Downloading","Downloading");
            german.Add("Downloading","Downloading");
            
            //Componet: 
            //
            english.Add("patching","patching");
            german.Add("patching","patching");
            
            //Componet: 
            //
            english.Add("done","done");
            german.Add("done","done");
            
            //Componet: 
            //
            english.Add("idle","idle");
            german.Add("idle","idle");
            
            //Componet: 
            //
            english.Add("status","status:");
            german.Add("status","status:");
            
            //Componet: 
            //
            english.Add("canceled","canceled");
            german.Add("canceled","canceled");
            
            //Componet: 
            //
            english.Add("appSingleInstance","Checking for single instance");
            german.Add("appSingleInstance","Checking for single instance");
            //Componet: 
            //
            english.Add("checkForUpdates","Checking for updates");
            german.Add("checkForUpdates","Checking for updates");
            
            //Componet: 
            //
            english.Add("verDirStructure","Verifying directory structure");
            german.Add("verDirStructure","Verifying directory structure");
            
            //Componet: 
            //
            english.Add("loadingSettings","Loading Settings");
            german.Add("loadingSettings","Loading Settings");
            
            //Componet: 
            //
            english.Add("loadingTranslations","Loading Translations");
            german.Add("loadingTranslations","Loading Translations");
            
            //Componet: 
            //
            english.Add("loading","Loading");
            german.Add("loading","Loading");
            
            //Componet: 
            //
            english.Add("uninstalling","Uninstalling");
            german.Add("uninstalling","Uninstalling");
            
            //Componet: 
            //
            english.Add("installingFonts","Installing Fonts");
            german.Add("installingFonts","Installing Fonts");
            
            //Componet: 
            //
            english.Add("loadingExtractionText","Loading Extraction Text");
            german.Add("loadingExtractionText","Loading Extraction Text");
            
            //Componet: 
            //
            english.Add("extractingRelhaxMods","Extracting RelHax Mods");
            german.Add("extractingRelhaxMods","Extracting RelHax Mods");
            
            //Componet: 
            //
            english.Add("extractingUserMods","Extracting User Mods");
            german.Add("extractingUserMods","Extracting User Mods");
            
            //Componet: 
            //
            english.Add("startingSmartUninstall","Starting smart uninstall");
            german.Add("startingSmartUninstall","Starting smart uninstall");
            
            //Componet: 
            //
            english.Add("copyingFile","Copying file");
            german.Add("copyingFile","Copying file");
            
            //Componet: 
            //
            english.Add("deletingFile","Deleting file");
            german.Add("deletingFile","Deleting file");
            
            //Componet: 
            //
            english.Add("of","of");
            german.Add("of","of");
            
            //Componet: 
            //
            english.Add("forceManuelDescription","This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");
            german.Add("forceManuelDescription","This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");
            //Componet: 
            //
            english.Add("cleanInstallDescription","This recommended option will empty your res_mods folder before installing" +
                    "your new mod selections. Unless you know what you are doing, it is recommended that you keep this on to avoid problems.");
            german.Add("cleanInstallDescription","This recommended option will empty your res_mods folder before installing" +
                    "your new mod selections. Unless you know what you are doing, it is recommended that you keep this on to avoid problems.");
            //Componet: 
            //
            english.Add("backupModsDescription","Select this to make a backup of your current res_mods folder." +
                    "Keep in mind that it only keeps the LATEST BACKUP, meaning if you check this and install," +
                    "it will delete what is currently in the backup location and copy what you have in your res_mods folder.");
            german.Add("backupModsDescription","Select this to make a backup of your current res_mods folder." +
                    "Keep in mind that it only keeps the LATEST BACKUP, meaning if you check this and install," +
                    "it will delete what is currently in the backup location and copy what you have in your res_mods folder.");
            //Componet: 
            //
            english.Add("comicSansDescription","Enable Comic Sans font");
            german.Add("comicSansDescription","Enable Comic Sans font");
            
            //Componet: 
            //
            english.Add("enlargeFontDescription","Enlarge font");
            german.Add("enlargeFontDescription","Enlarge font");
            
            //Componet: 
            //
            english.Add("selectGifDesc","Select a loading gif for the mod preview window.");
            german.Add("selectGifDesc","Select a loading gif for the mod preview window.");
            
            //Componet: 
            //
            english.Add("saveLastConfigInstall","If this is selected, the installer will, upon selection window showing, load the last installed config you used.");
            german.Add("saveLastConfigInstall","If this is selected, the installer will, upon selection window showing, load the last installed config you used.");
            
            //Componet:
            //
            english.Add("saveUserDataDesc","If this is selected, the installer will save user created data (like session stats from previous battles)");
            german.Add("saveUserDataDesc","If this is selected, the installer will save user created data (like session stats from previous battles)");
            
            //Componet: 
            //
            english.Add("cleanUninstallDescription","Selected - All mods will be erased\nNot Selected - Only Modpack installed mods will be erased");
            german.Add("cleanUninstallDescription","Selected - All mods will be erased\nNot Selected - Only Modpack installed mods will be erased");
            
            //Componet: 
            //
            english.Add("darkUIDesc","Toggle the DarkUI mode. Usefull for working with the modpack at night.");
            german.Add("darkUIDesc","Toggle the DarkUI mode. Usefull for working with the modpack at night.");
            
            //Componet: 
            //
            english.Add("failedToDownload_1","Failed to download ");
            german.Add("failedToDownload_1","Failed to download ");
            
            //Componet: 
            //
            english.Add("failedToDownload_2",". If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it exits");
            german.Add("failedToDownload_2",". If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it exits");
            
            //Componet: 
            //
            english.Add("fontsPromptInstallHeader","Admin to install fonts?");
            german.Add("fontsPromptInstallHeader","Admin to install fonts?");
            
            //Componet: 
            //
            english.Add("fontsPromptInstallText","Do you have admin rights?");
            german.Add("fontsPromptInstallText","Do you have admin rights?");
            
            //Componet: 
            //
            english.Add("fontsPromptError_1","Unable to install fonts. Some mods may not work properly. Fonts are located in ");
            german.Add("fontsPromptError_1","Unable to install fonts. Some mods may not work properly. Fonts are located in ");
            
            //Componet: 
            //
            english.Add("fontsPromptError_2","\\_fonts. Eithor install them yourself or run this again as Administrator.");
            german.Add("fontsPromptError_2","\\_fonts. Eithor install them yourself or run this again as Administrator.");
            
            //Componet: 
            //
            english.Add("cantDownloadNewVersion","Unable to download new version, exiting.");
            german.Add("cantDownloadNewVersion","Unable to download new version, exiting.");
            
            //Componet: 
            //
            english.Add("cantStartNewApp","Unable to start application, but it is located in \n");
            german.Add("cantStartNewApp","Unable to start application, but it is located in \n");
            
            //Componet: 
            //
            english.Add("autoDetectFailed","The auto-detection failed. Please use the 'force manual' option");
            german.Add("autoDetectFailed","The auto-detection failed. Please use the 'force manual' option");
            
            //Componet: 
            //
            english.Add("anotherInstanceRunning","CRITICAL: Another Instance of the relic mod manager is already running");
            german.Add("anotherInstanceRunning","CRITICAL: Another Instance of the relic mod manager is already running");
            
            //Componet: 
            //
            english.Add("skipUpdateWarning","WARNING: You are skipping updating. Database Compatability is not guarenteed");
            german.Add("skipUpdateWarning","WARNING: You are skipping updating. Database Compatability is not guarenteed");
            
            //Componet: 
            //
            english.Add("patchDayMessage","The modpack is curretly down for patch day testing and mods updating. Sorry for the inconvience. If you are a database manager, please add the command arguement");
            german.Add("patchDayMessage","The modpack is curretly down for patch day testing and mods updating. Sorry for the inconvience. If you are a database manager, please add the command arguement");
            
            //Componet: 
            //
            english.Add("configNotExist"," does NOT exist, loading in regular mode");
            german.Add("configNotExist"," does NOT exist, loading in regular mode");
            
            //Componet: 
            //
            english.Add("autoAndClean","ERROR: clean installation is set to false. You must set this to true and restart the application for auto install to work. Loading in regular mode.");
            german.Add("autoAndClean","ERROR: clean installation is set to false. You must set this to true and restart the application for auto install to work. Loading in regular mode.");
            
            //Componet: 
            //
            english.Add("autoAndFirst","ERROR: First time loading cannot be an auto install mode, loading in regular mode");
            german.Add("autoAndFirst","ERROR: First time loading cannot be an auto install mode, loading in regular mode");
            
            //Componet: 
            //
            english.Add("confirmUninstallHeader","Confirmation");
            german.Add("confirmUninstallHeader","Confirmation");
            
            //Componet: 
            //
            english.Add("confirmUninstallMessage","Confirm you wish to uninstall?");
            german.Add("confirmUninstallMessage","Confirm you wish to uninstall?");
            
            //Componet: 
            //
            english.Add("uninstallingText","Uninstalling...");
            german.Add("uninstallingText","Uninstalling...");
            
            //Componet: 
            //
            english.Add("specialMessage1","If you are seeing this, it means that you have a specific computer configuration that is affected by a bug I can't replicate on my developer system. It's harmless, but if you could send your relHaxLog to me I can fix it and you can stop seeing this message");
            german.Add("specialMessage1","If you are seeing this, it means that you have a specific computer configuration that is affected by a bug I can't replicate on my developer system. It's harmless, but if you could send your relHaxLog to me I can fix it and you can stop seeing this message");
            
            //Componet: 
            //
            english.Add("extractionErrorMessage","Extraction Error. Is World of Tanks running?");
            german.Add("extractionErrorMessage","Extraction Error. Is World of Tanks running?");
            
            //Componet: 
            //
            english.Add("extractionErrorHeader","Error");
            german.Add("extractionErrorHeader","Error");
            
            //Componet: 
            //
            english.Add("deleteErrorHeader","close out of folders");
            german.Add("deleteErrorHeader","close out of folders");
            
            //Componet: 
            //
            english.Add("deleteErrorMessage","Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");
            german.Add("deleteErrorMessage","Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");
            
            //Componet: 
            //
            english.Add("noUninstallLogMessage","The log file containg the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");
            german.Add("noUninstallLogMessage","The log file containg the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");
            
            //Componet: 
            //
            english.Add("noUninstallLogHeader","Remove all mods");
            german.Add("noUninstallLogHeader","Remove all mods");
            
            //Section: Messages from ModSelectionList
            //Componet: 
            //
            english.Add("duplicateMods","CRITICAL: Duplicate mod name detected");
            german.Add("duplicateMods","CRITICAL: Duplicate mod name detected");
            
            //Componet: 
            //
            english.Add("databaseReadFailed","CRITICAL: Failed to read database");
            german.Add("databaseReadFailed","CRITICAL: Failed to read database");
            
            //Componet: 
            //
            english.Add("configSaveSucess","Config Saved Sucessfully");
            german.Add("configSaveSucess","Config Saved Sucessfully");
            
            //Componet: 
            //
            english.Add("selectConfigFile","Select a user prefrence file to load");
            german.Add("selectConfigFile","Select a user prefrence file to load");
            
            //Componet: 
            //
            english.Add("configLoadFailed","The config file could not be loaded, loading in standard mode");
            german.Add("configLoadFailed","The config file could not be loaded, loading in standard mode");
            
            //Componet: 
            //
            english.Add("modNotFound_1","The mod, \"");
            german.Add("modNotFound_1","The mod, \"");
            
            //Componet: 
            //
            english.Add("modNotFound_2","\" was not found in the modpack. It could have been renamed or removed.");
            german.Add("modNotFound_2","\" was not found in the modpack. It could have been renamed or removed.");
            
            //Componet: 
            //
            english.Add("configNotFound_1","The config \"");
            german.Add("configNotFound_1","The config \"");
            
            //Componet: 
            //
            english.Add("configNotFound_2","\" was not found for mod \"");
            german.Add("configNotFound_2","\" was not found for mod \"");
            
            //Componet: 
            //
            english.Add("configNotFound_3","\". It could have been renamed or removed.");
            german.Add("configNotFound_3","\". It could have been renamed or removed.");
            
            //Componet: 
            //
            english.Add("prefrencesSet","prefrences Set");
            german.Add("prefrencesSet","prefrences Set");
            
            //Componet: 
            //
            english.Add("selectionsCleared","selections cleared");
            german.Add("selectionsCleared","selections cleared");
        }
    }
}
