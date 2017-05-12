using System.Collections;

namespace RelhaxModpack
{
    //A static class to exist throughout the entire application life, will always have translations
    public static class Translations
    {
        //Enumerator to determine which translated string to return
        public enum Languages { English = 0, German = 1 };
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
            english.Add("installRelhaxMod", "Start Mod Selection");
            german.Add("installRelhaxMod", "Auswahl der Modifikationen");

            //Componet: uninstallRelhaxMod
            //
            english.Add("uninstallRelhaxMod", "Uninstall Relhax Modpack");
            german.Add("uninstallRelhaxMod", "Relhax Modpack deinstallieren");

            //Componet: forceManuel
            //
            english.Add("forceManuel", "Force manual game detection");
            german.Add("forceManuel", "Erzwinge manuelle Spielerkennung");

            //Componet: forceManuel
            //
            english.Add("languageSelectionGB", "Language Selection");
            german.Add("languageSelectionGB", "Sprachauswahl");

            //Componet: formPageLink
            //
            english.Add("formPageLink", "View Modpack Form Page");
            german.Add("formPageLink", "Zeige Modpack Formularseite");

            //Componet: saveUserDataCB
            //
            english.Add("saveUserDataCB", "Save user data");
            german.Add("saveUserDataCB", "Benutzerdaten speichern");

            //Componet: cleanInstallCB
            //
            english.Add("cleanInstallCB", "Clean Installation (Recommended)");
            german.Add("cleanInstallCB", "Saubere Installation (Empfohlen)");

            //Componet: cancerFontCB
            //
            english.Add("cancerFontCB", "Comic Sans Font");
            german.Add("cancerFontCB", "Comic Sans Schriftart");

            //Componet: backupModsCheckBox
            //
            english.Add("backupModsCheckBox", "Backup current mods folder");
            german.Add("backupModsCheckBox", "Sicherung des aktuellen Modsordner");

            //Componet: settingsGroupBox
            //
            english.Add("settingsGroupBox", "RelHax ModPack Settings");
            german.Add("settingsGroupBox", "RelHax ModPack Einstellungen");

            //Componet: darkUICB
            //
            english.Add("darkUICB", "Dark UI");
            german.Add("darkUICB", "Dunkle Benutzeroberflaeche");

            //Componet: cleanUninstallCB
            //
            english.Add("cleanUninstallCB", "Clean uninstallation");
            german.Add("cleanUninstallCB", "Saubere Deinstallation");

            //Componet: saveLastInstallCB
            //
            english.Add("saveLastInstallCB", "Save last install\'s config");
            german.Add("saveLastInstallCB", "Speicherung der letzten Installation");

            //Componet: largerFontButton
            //
            english.Add("largerFontButton", "Larger Font");
            german.Add("largerFontButton", "Groessere Schriftart");

            //Componet: loadingImageGroupBox
            //
            english.Add("loadingImageGroupBox", "Loading Image");
            german.Add("loadingImageGroupBox", "Bild laden");

            //Componet: standardImageRB
            //
            english.Add("standardImageRB", "Standard");
            german.Add("standardImageRB", "Standard");

            //Componet: findBugAddModLabel
            //
            english.Add("findBugAddModLabel", "Find a bug? Want a mod added?");
            german.Add("findBugAddModLabel", "Fehler gefunden? Willst Du einen Mod hinzufuegen");

            //Componet: cancelDownloadButton
            //
            english.Add("cancelDownloadButton", "Cancel Download");
            german.Add("cancelDownloadButton", "Download abbrechen");

            //Section: FirstLoadHelper
            //Componet: helperText
            //
            english.Add("helperText", "Welcome to the RelHax Modpack! I have tried to make the modpack as straight-forward as possible, but questions may still arise. Hover over (or right click) a setting to have it explained. You won't see this dialog box again, unless you delete the settings xml file.");
            german.Add("helperText", "Willkommen im RelHax Modpack! Ich habe versucht, das Modpack so einfach wie möglich zu gestalten, aber Fragen können dennoch entstehen. Rechtsklick auf eine Einstellung erklaert diese dann. Du siehst diese Dialogbox nicht mehr, ausser du loescht die xml Datei.");

            //Section: ModSelectionList
            //Componet: continueButton
            //
            english.Add("continueButton", "Install");
            german.Add("continueButton", "Installieren");

            //Componet: cancelButton
            //
            english.Add("cancelButton", "cancel");
            german.Add("cancelButton", "Abbrechen");

            //Componet: helpLabel
            //
            english.Add("helpLabel", "right-click a mod name to preview it");
            german.Add("helpLabel", "Klick mit rechten Maustaste auf einen Mod-Namen, um eine Vorschau zu sehen");

            //Componet: loadConfigButton
            //
            english.Add("loadConfigButton", "Load selection");
            german.Add("loadConfigButton", "Auswahl laden");

            //Componet: saveConfigButton
            //
            english.Add("saveConfigButton", "Save selection");
            german.Add("saveConfigButton", "Auswahl speichern");

            //Componet: label2
            //
            english.Add("label2", "\"*\" tab indicates single selection tab");
            german.Add("label2", "Ein Tab mit einem\"*\" kann nur eins der Mods ausgewählt werden.");

            //Componet: clearSelectionsButton
            //
            english.Add("clearSelectionsButton", "Clear Selections");
            german.Add("clearSelectionsButton", "Auswahl loeschen");

            //Componet: readingDatabase
            //
            english.Add("readingDatabase", "Reading Database");
            german.Add("readingDatabase", "Lese Datenbank");

            //Componet: buildingUI
            //
            english.Add("buildingUI", "Building UI");
            german.Add("buildingUI", "Erstelle UI");

            //Section: Preview
            //Componet: nextPicButton
            //
            english.Add("nextPicButton", "next");
            german.Add("nextPicButton", "naechstes");

            //Componet: previousPicButton
            //
            english.Add("previousPicButton", "previous");
            german.Add("previousPicButton", "vorheriges");

            //Componet: devLinkLabel
            //
            english.Add("devLinkLabel", "Developer Website");
            german.Add("devLinkLabel", "Entwickler Webseite");

            //Section: VersionInfo
            //Componet: updateAcceptButton
            //
            english.Add("updateAcceptButton", "yes");
            german.Add("updateAcceptButton", "ja");

            //Componet: updateDeclineButton
            //
            english.Add("updateDeclineButton", "no");
            german.Add("updateDeclineButton", "nein");

            //Componet: newVersionAvailableLabel
            //
            english.Add("newVersionAvailableLabel", "New Version Available");
            german.Add("newVersionAvailableLabel", "Neue Version verfuegbar");

            //Componet: updateQuestionLabel
            //
            english.Add("updateQuestionLabel", "Update?");
            german.Add("updateQuestionLabel", "Aktualisieren?");

            //Componet: problemsUpdatingLabel
            //
            english.Add("problemsUpdatingLabel", "If you are having problems updating, please");
            german.Add("problemsUpdatingLabel", "Wenn Sie Probleme mit der Aktualisierung haben, bitte");

            //Componet: 
            //
            english.Add("clickHereUpdateLabel", "click here.");
            german.Add("clickHereUpdateLabel", "klick hier.");

            //Section: PleaseWait
            //Componet: label1
            //
            english.Add("label1", "Loading...please wait...");
            german.Add("label1", "Laedt...bitte warten...");

            //Section: Messages of MainWindow
            //Componet: 
            //
            english.Add("Downloading", "Downloading");
            german.Add("Downloading", "Wird heruntergeladen");

            //Componet: 
            //
            english.Add("patching", "patching");
            german.Add("patching", "patching");

            //Componet: 
            //
            english.Add("done", "done");
            german.Add("done", "fertig");

            //Componet: 
            //
            english.Add("idle", "idle");
            german.Add("idle", "Leerlauf");

            //Componet: 
            //
            english.Add("status", "status:");
            german.Add("status", "Status:");

            //Componet: 
            //
            english.Add("canceled", "canceled");
            german.Add("canceled", "abgebrochen");

            //Componet: 
            //
            english.Add("appSingleInstance", "Checking for single instance");
            german.Add("appSingleInstance", "Prüfung auf Einzelinstanz");
            //Componet: 
            //
            english.Add("checkForUpdates", "Checking for updates");
            german.Add("checkForUpdates", "Auf Updates prüfen");

            //Componet: 
            //
            english.Add("verDirStructure", "Verifying directory structure");
            german.Add("verDirStructure", "Verifizierung der Verzeichnisstruktur");

            //Componet: 
            //
            english.Add("loadingSettings", "Loading Settings");
            german.Add("loadingSettings", "Einstellungen laden");

            //Componet: 
            //
            english.Add("loadingTranslations", "Loading Translations");
            german.Add("loadingTranslations", "Laden der Uebersetzungen");

            //Componet: 
            //
            english.Add("loading", "Loading");
            german.Add("loading", "Laden");

            //Componet: 
            //
            english.Add("uninstalling", "Uninstalling");
            german.Add("uninstalling", "Deinstallieren");

            //Componet: 
            //
            english.Add("installingFonts", "Installing Fonts");
            german.Add("installingFonts", "Installieren von Schriftarten");

            //Componet: 
            //
            english.Add("loadingExtractionText", "Loading Extraction Text");
            german.Add("loadingExtractionText", "Extraktionstext laden");

            //Componet: 
            //
            english.Add("extractingRelhaxMods", "Extracting RelHax Mods");
            german.Add("extractingRelhaxMods", "Extrahieren von RelHax Mods");

            //Componet: 
            //
            english.Add("extractingUserMods", "Extracting User Mods");
            german.Add("extractingUserMods", "Extrahieren von benutzerdefinierten Mods");

            //Componet: 
            //
            english.Add("startingSmartUninstall", "Starting smart uninstall");
            german.Add("startingSmartUninstall", "Starten der intelligenten Deinstallation");

            //Componet: 
            //
            english.Add("copyingFile", "Copying file");
            german.Add("copyingFile", "Kopieren von Dateien");

            //Componet: 
            //
            english.Add("deletingFile", "Deleting file");
            german.Add("deletingFile", "Loeschen von Dateien");

            //Componet: 
            //
            english.Add("of", "of");
            german.Add("of", "von");

            //Componet: 
            //
            english.Add("forceManuelDescription", "This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");
            german.Add("forceManuelDescription", "Diese Option ist für die Erzwingung einer manuellen World of Tanks Spiel-" +
                    "Speicherort-Erkennung. Überpruefe dies, wenn Du Probleme mit der automatischen Suche des Spiels hast.");
            //Componet: 
            //
            english.Add("cleanInstallDescription", "This recommended option will empty your res_mods folder before installing" +
                    "your new mod selections. Unless you know what you are doing, it is recommended that you keep this on to avoid problems.");
            german.Add("cleanInstallDescription", "Diese empfohlene Option leert den Ordner res_mods vor der Installation" +
                    "Deiner neuen Mod-Auswahl. Bis du genau weißt, was du tust, empfohlen wir, dass du das weiter behältst, um Probleme zu vermeiden.");
            //Componet: 
            //
            english.Add("backupModsDescription", "Select this to make a backup of your current res_mods folder." +
                    "They are stored in the 'RelHaxModBackup' folder, saved in a folder inside by a time stamp.");
            german.Add("backupModsDescription", "Waehle diese Option, um eine Sicherungskopie Deines aktuellen res_mods-Ordners zu erstellen." +
                    "Sie werden im Ordner 'RelHaxModBackup' gespeichert und in einem Ordner nach innen durch einen Zeitstempel gespeichert.");
            //Componet: 
            //
            english.Add("comicSansDescription", "Enable Comic Sans font");
            german.Add("comicSansDescription", "Schriftart Comic Sans aktivieren");

            //Componet: 
            //
            english.Add("enlargeFontDescription", "Enlarge font");
            german.Add("enlargeFontDescription", "Schriftart vergroessern");

            //Componet: 
            //
            english.Add("selectGifDesc", "Select a loading gif for the mod preview window.");
            german.Add("selectGifDesc", "Waehle ein Lade-Gif fuer das Vorschaufenster des Mods.");

            //Componet: 
            //
            english.Add("saveLastConfigInstall", "If this is selected, the installer will, upon selection window showing, load the last installed config you used.");
            german.Add("saveLastConfigInstall", "Wenn dies ausgewählt ist, lädt das Installationsprogramm die zuletzt installierte Config im Auswahlfenster, die Du verwendet hast.");

            //Componet:
            //
            english.Add("saveUserDataDesc", "If this is selected, the installer will save user created data (like session stats from previous battles)");
            german.Add("saveUserDataDesc", "Wenn dies ausgewählt ist, speichert das Installationsprogramm die vom Benutzer erstellten Daten (wie Sitzungsstatistiken aus früheren Gefechten)");

            //Componet: 
            //
            english.Add("cleanUninstallDescription", "Selected - All mods will be erased\nNot Selected - Only Modpack installed mods will be erased");
            german.Add("cleanUninstallDescription", "Ausgewählt - Alle Mods werden gelöscht\nNicht ausgewählt - Nur Mods, die vom Modpack installiert wurden, werden gelöscht");

            //Componet: 
            //
            english.Add("darkUIDesc", "Toggle the DarkUI mode. Usefull for working with the modpack at night.");
            german.Add("darkUIDesc", "Auf dunklen UI Modus umschalten. Nützlich für die Arbeit mit dem Modpack in der Nacht.");

            //Componet: 
            //
            english.Add("failedToDownload_1", "Failed to download ");
            german.Add("failedToDownload_1", "Fehler beim Herunterladen ");

            //Componet: 
            //
            english.Add("failedToDownload_2", ". If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it exits");
            german.Add("failedToDownload_2", ". Wenn du weisst, welcher Mod das ist, deaktiviere ihn und alles sollte funktionieren. Es wird bald behoben. Starte neu, wenn es besteht");

            //Componet: 
            //
            english.Add("fontsPromptInstallHeader", "Admin to install fonts?");
            german.Add("fontsPromptInstallHeader", "Admin zum Installieren von Schriftarten?");

            //Componet: 
            //
            english.Add("fontsPromptInstallText", "Do you have admin rights?");
            german.Add("fontsPromptInstallText", "Hast Du Admin-Rechte?");

            //Componet: 
            //
            english.Add("fontsPromptError_1", "Unable to install fonts. Some mods may not work properly. Fonts are located in ");
            german.Add("fontsPromptError_1", "Schriftarten können nicht installiert werden. Einige Mods funktionieren möglicherweise nicht richtig. Schriften befinden sich in ");

            //Componet: 
            //
            english.Add("fontsPromptError_2", "\\_fonts. Either install them yourself or run this again as Administrator.");
            german.Add("fontsPromptError_2", "\\_fonts. Entweder installiere sie selbst oder führe es erneut als Administrator aus.");

            //Componet: 
            //
            english.Add("cantDownloadNewVersion", "Unable to download new version, exiting.");
            german.Add("cantDownloadNewVersion", "Die neue Version kann nicht heruntergeladen werden.");

            //Componet: 
            //
            english.Add("cantStartNewApp", "Unable to start application, but it is located in \n");
            german.Add("cantStartNewApp", "Kann die Anwendung nicht starten, aber sie befindet sich in \n");

            //Componet: 
            //
            english.Add("autoDetectFailed", "The auto-detection failed. Please use the 'force manual' option");
            german.Add("autoDetectFailed", "Die automatische Erkennung ist fehlgeschlagen. Bitte benutzen Sie die 'erzwinge manuelle' Option");

            //Componet: 
            //
            english.Add("anotherInstanceRunning", "CRITICAL: Another Instance of the relic mod manager is already running");
            german.Add("anotherInstanceRunning", "KRITISCH: Eine weitere Instanz des Relic Mod Managers läuft bereits");

            //Componet: 
            //
            english.Add("skipUpdateWarning", "WARNING: You are skipping updating. Database Compatability is not guarenteed");
            german.Add("skipUpdateWarning", "WARNUNG: Sie überspringen die Aktualisierung. Datenbankkompatibilität ist nicht garantiert");

            //Componet: 
            //
            english.Add("patchDayMessage", "The modpack is curretly down for patch day testing and mods updating. Sorry for the inconvience. If you are a database manager, please add the command arguement");
            german.Add("patchDayMessage", "Das Modpack ist zur Zeit für Patch-Tag-Tests und das Updaten von Mods nicht erreichbar. Entschuldige die Unannehmlichkeiten. Wenn Sie ein Datenbankmanager sind, fügen Sie bitte das Befehlsargument hinzu");

            //Componet: 
            //
            english.Add("configNotExist", " does NOT exist, loading in regular mode");
            german.Add("configNotExist", " existiert nicht, laedt im regulaeren Modus");

            //Componet: 
            //
            english.Add("autoAndClean", "ERROR: clean installation is set to false. You must set this to true and restart the application for auto install to work. Loading in regular mode.");
            german.Add("autoAndClean", "FEHLER: Die saubere Installation ist auf false eingestellt. Du musst dies auf true setzen und die Anwendung für die automatische Installation neu starten, damit sie funktioniert. Laedt im regulaeren Modus.");

            //Componet: 
            //
            english.Add("autoAndFirst", "ERROR: First time loading cannot be an auto install mode, loading in regular mode");
            german.Add("autoAndFirst", "FEHLER: Erstmaliges Laden kann kein automatischer Installationsmodus sein, lade im regulaeren Modus");

            //Componet: 
            //
            english.Add("confirmUninstallHeader", "Confirmation");
            german.Add("confirmUninstallHeader", "Bestätigung");

            //Componet: 
            //
            english.Add("confirmUninstallMessage", "Confirm you wish to uninstall?");
            german.Add("confirmUninstallMessage", "Bestätige, wenn du installieren möchtest?");

            //Componet: 
            //
            english.Add("uninstallingText", "Uninstalling...");
            german.Add("uninstallingText", "Deinstalliere...");

            //Componet: 
            //
            english.Add("specialMessage1", "If you are seeing this, it means that you have a specific computer configuration that is affected by a bug I can't replicate on my developer system. It's harmless, but if you could send your relHaxLog to me I can fix it and you can stop seeing this message");
            german.Add("specialMessage1", "Wenn Du dies siehst, bedeutet dies, dass Du eine bestimmte Computer-Konfiguration hast, die von einem Fehler betroffen ist, den ich nicht auf meinem Entwicklersystem replizieren kann. Es ist harmlos, aber wenn du dein relHaxLog an mich schicken könntest, kann ich es beheben und du wirst diese Nachricht zukuenftig nicht mehr sehen");

            //Componet: 
            //
            english.Add("extractionErrorMessage", "Error deleting the folder res_mods or mods. Either World of Tanks is running or your file and folder security permissions are incorrect");
            german.Add("extractionErrorMessage", "Fehler beim Löschen des Ordners res_mods oder Mods. Entweder World of Tanks läuft oder Ihre Datei- und Ordnersicherheitsberechtigungen sind falsch");

            //Componet: 
            //
            english.Add("extractionErrorHeader", "Error");
            german.Add("extractionErrorHeader", "Fehler");

            //Componet: 
            //
            english.Add("deleteErrorHeader", "close out of folders");
            german.Add("deleteErrorHeader", "Ausschliessen von Ordnern");

            //Componet: 
            //
            english.Add("deleteErrorMessage", "Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");
            german.Add("deleteErrorMessage", "Bitte schließen Sie alle Explorer-Fenster in Mods oder res_mods (oder Unterordner), und klicken Sie auf OK, um fortzufahren.");

            //Componet: 
            //
            english.Add("noUninstallLogMessage", "The log file containg the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");
            german.Add("noUninstallLogMessage", "Die Protokolldatei mit der Liste der installierten Dateien (installedRelhaxFiles.log) existiert nicht. Möchtest Du alle Mods stattdessen entfernen?");

            //Componet: 
            //
            english.Add("noUninstallLogHeader", "Remove all mods");
            german.Add("noUninstallLogHeader", "Entferne alle Mods");

            //Section: Messages from ModSelectionList
            //Componet: 
            //
            english.Add("duplicateMods", "CRITICAL: Duplicate mod name detected");
            german.Add("duplicateMods", "KRITISCH: Duplizierter Modname wurde erkannt");

            //Componet: 
            //
            english.Add("databaseReadFailed", "CRITICAL: Failed to read database");
            german.Add("databaseReadFailed", "KRITISCH: Datenbank konnte nicht gelesen werden");

            //Componet: 
            //
            english.Add("configSaveSucess", "Config Saved Sucessfully");
            german.Add("configSaveSucess", "Config wurde erfolgreich gespeichert");

            //Componet: 
            //
            english.Add("selectConfigFile", "Select a user preference file to load");
            german.Add("selectConfigFile", "Wählen Sie die benutzerdefinierte Datei aus, die geladen werden soll");

            //Componet: 
            //
            english.Add("configLoadFailed", "The config file could not be loaded, loading in standard mode");
            german.Add("configLoadFailed", "Die Konfigurationsdatei konnte nicht geladen werden, lade im Standard Modus");

            //Componet: 
            //
            english.Add("modNotFound_1", "The mod, \"");
            german.Add("modNotFound_1", "Der Mod, \"");

            //Componet: 
            //
            english.Add("modNotFound_2", "\" was not found in the modpack. It could have been renamed or removed.");
            german.Add("modNotFound_2", "\" wurde im Modpack nicht gefunden. Er könnte umbenannt oder entfernt worden sein.");

            //Componet: 
            //
            english.Add("configNotFound_1", "The config \"");
            german.Add("configNotFound_1", "Die Config \"");

            //Componet: 
            //
            english.Add("configNotFound_2", "\" was not found for mod \"");
            german.Add("configNotFound_2", "\" wurde nicht für den Mod gefunden \"");

            //Componet: 
            //
            english.Add("configNotFound_3", "\". It could have been renamed or removed.");
            german.Add("configNotFound_3", "\". Er könnte umbenannt oder entfernt worden sein.");

            //Componet: 
            //
            english.Add("prefrencesSet", "preferences Set");
            german.Add("prefrencesSet", "bevorzugte Einstellungen");

            //Componet: 
            //
            english.Add("selectionsCleared", "selections cleared");
            german.Add("selectionsCleared", "Auswahlen geloescht");

            //Componet: 
            //
            english.Add("patchingSystemDeneidAccessMessage", "The patching system was denied access to the patch folder. Retry as Administrator. If you see this again, you need to fix your file and folder security permissions");
            german.Add("patchingSystemDeneidAccessMessage", "Dem Patching-System wurde der Zugriff auf den Patch-Ordner verweigert. Wiederholen als Administrator. Wenn Sie dies wieder sehen, müssen Sie Ihre Datei- und Ordnersicherheitsberechtigungen reparieren");

            //Componet: 
            //
            english.Add("patchingSystemDeneidAccessHeader", "Access Deneid");
            german.Add("patchingSystemDeneidAccessHeader", "Zugriff abgelehnt");

            //Componet: 
            //
            english.Add("databaseNotFound", "Database not found at supplied URL");
            german.Add("databaseNotFound", "Datenbank nicht bei der angegebenen URL gefunden");

            //Componet:
            //
            english.Add("WoTRunningHeader", "WoT is Running");
            german.Add("WoTRunningHeader", "WoT wird gerade ausgeführt.");

            //Componet:
            //
            english.Add("WoTRunningMessage", "Please close World of Tanks to continue");
            german.Add("WoTRunningMessage", "Um Fortzufahren, schliesse bitte World of Tanks.");

            //Componet:
            //
            english.Add("InstallingTo", "Installing to");
            german.Add("InstallingTo", "Installiere nach");

            //Componet:
            //
            english.Add("zipReadingErrorHeader", "Incomplete Download");
            german.Add("zipReadingErrorHeader", "Installiere nach");

            //Componet:
            //
            english.Add("zipReadingErrorMessage1", "The zip file");
            german.Add("zipReadingErrorMessage1", "Installiere nach");

            //Componet:
            //
            english.Add("zipReadingErrorMessage2", "failed to be read, most likely to an incomplete download. It will be skipped. Please try downloading again.");
            german.Add("zipReadingErrorMessage2", "Installiere nach");
        }
    }
}
