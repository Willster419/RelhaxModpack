using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace RelhaxModpack
{
    /// <summary>
    /// List of all supported Languages in the modpack
    /// </summary>
    public enum Languages
    {
        /// <summary>
        /// The English language
        /// </summary>
        English,
        /// <summary>
        /// The German language
        /// </summary>
        German,
        /// <summary>
        /// The Polish language
        /// </summary>
        Polish,
        /// <summary>
        /// The French language
        /// </summary>
        French
    };
    /// <summary>
    /// Handles all localization for the application User Interface
    /// </summary>
    public static class Translations
    {
        //TODO: when write blacklist check, check if name is blank/null/whitespace!!
        private static readonly string[] TranslationComponentBlacklist = new string[]
        {
            "ApplicationVersionLabel",
            "DatabaseVersionLabel",
            "InstallProgressTextBox",
            "LanguagesSelector",
            "PART_ContentHostDescription",
            "toggleButtonDescription",
            "AutoInstallOneClickInstallSelectionFilePath",
            "AutoSyncFrequencyTexbox",
            "DeveloperSettingsHeaderDescription",
            "AutoSyncCheckFrequencyTextBox",
            "AutoSyncCheckFrequencyTextBoxDescription",
            "DiagnosticsStatusTextBox"
        };
        private const string TranslationNeeded = "TODO";
        private static readonly string Blank = string.Empty;
        public const string LanguageEnglish = "English";
        public const string LanguagePolish = "Polski";
        public const string LanguageGerman = "Deutsch";
        public const string LanguageFrench = "Français";
        private static Dictionary<string, string> English = new Dictionary<string, string>();
        private static Dictionary<string, string> Gernam = new Dictionary<string, string>();
        private static Dictionary<string, string> Polish = new Dictionary<string, string>();
        private static Dictionary<string, string> French = new Dictionary<string, string>();
        //default is to use english
        private static Dictionary<string, string> CurrentLanguage = English;
        #region Language methods
        /// <summary>
        /// Sets the language dictionary to use when returning a localized string
        /// </summary>
        /// <param name="language">The language to switch to</param>
        public static void SetLanguage(Languages language)
        {
            switch(language)
            {
                case Languages.English:
                    CurrentLanguage = English;
                    break;
                case Languages.French:
                    CurrentLanguage = French;
                    break;
                case Languages.German:
                    CurrentLanguage = Gernam;
                    break;
                case Languages.Polish:
                    CurrentLanguage = Polish;
                    break;
            }            
            ModpackSettings.Language = language;
        }
        /// <summary>
        /// Change the language to the active language code installed on the computer, if we support
        /// </summary>
        public static void SetLanguageOnFirstLoad()
        {
            //try to get and load the native language of the user
            Logging.WriteToLog("Language: " + CultureInfo.CurrentCulture.DisplayName);
            switch (CultureInfo.InstalledUICulture.Name.Split('-')[0].ToLower())
            {
                case "de":
                    SetLanguage(Languages.German);
                    break;
                case "pl":
                    SetLanguage(Languages.Polish);
                    break;
                case "fr":
                    SetLanguage(Languages.French);
                    break;
                default:
                    SetLanguage(Languages.English);
                    break;
            }
        }
        /// <summary>
        /// Get a localized string in the currently selected language
        /// </summary>
        /// <param name="componentName">The key value of the string phrase</param>
        /// <returns></returns>
        public static string GetTranslatedString(string componentName)
        {
            string s = "";
            //check if componentName key exists in current language
            if(CurrentLanguage.ContainsKey(componentName))
            {
                s = CurrentLanguage[componentName];
                //if the value is TODO, check if we have it in english (unless it is english)
                if(s.Equals(TranslationNeeded))
                {
                    //Log warning it is todo in selected language
                    Logging.WriteToLog(string.Format("Missing translation key={0}, value=TODO, language={1}", componentName, ModpackSettings.Language.ToString()),Logfiles.Application,LogLevel.Error);
                    s = English[componentName];
                    if(s.Equals(TranslationNeeded))
                    {
                        //Log error it is todo in english
                        Logging.WriteToLog(string.Format("Missing translation key={0}, value=TODO, language=English", componentName), Logfiles.Application, LogLevel.Error);
                        s = componentName;
                    }
                }
            }
            else
            {
                //check if key exists in english (should not be the case 99% of the time)
                if(English.ContainsKey(componentName))
                {
                    Logging.WriteToLog(string.Format("Missing translation key={0}, value=TODO, language={1}", componentName, ModpackSettings.Language.ToString()), Logfiles.Application, LogLevel.Error);
                    s = English[componentName];
                    if (s.Equals(TranslationNeeded))
                    {
                        //Log error it is todo in english
                        Logging.WriteToLog(string.Format("Missing translation key={0}, value=TODO, language=English", componentName), Logfiles.Application, LogLevel.Error);
                        s = componentName;
                    }
                }
                //Log error it does not exist
                Logging.WriteToLog(string.Format("component {0} does not exist in any languages", componentName), Logfiles.Application, LogLevel.Error);
                s=componentName;
            }
            return s;
        }

        public static bool Exists(string componentName, Languages languageToCheck = Languages.English)
        {
            //English will always have the most up to date translations. that's just how it is.
            Dictionary<string, string> DictToCheck = null;
            switch (languageToCheck)
            {
                case Languages.English:
                    DictToCheck = English;
                    break;
                case Languages.French:
                    DictToCheck = French;
                    break;
                case Languages.German:
                    DictToCheck = Gernam;
                    break;
                case Languages.Polish:
                    DictToCheck = Polish;
                    break;
            }
            return DictToCheck.ContainsKey(componentName);
        }
        #endregion

        #region Literally loading Translations
        private static void AddTranslationToAll(string key, string message)
        {
            English.Add(key, message);
            Gernam.Add(key, message);
            Polish.Add(key, message);
            French.Add(key, message);
        }
        /// <summary>
        /// Loads all translation dictionaries. Should only be done once (at application start)
        /// </summary>
        public static void LoadTranslations()
        {
            //Syntax is as follows:
            //languageName.Add("componetName","TranslatedString");

            #region General expressions
            English.Add("yes", "yes");
            Gernam.Add("yes", "ja");
            Polish.Add("yes", "Tak");
            French.Add("yes", "Oui");

            English.Add("no", "no");
            Gernam.Add("no", "nein");
            Polish.Add("no", "Nie");
            French.Add("no", "Non");

            English.Add("cancel", "Cancel");
            Gernam.Add("cancel", "Abbrechen");
            Polish.Add("cancel", "Anuluj");
            French.Add("cancel", "Anuler");

            English.Add("delete", "Delete");
            Gernam.Add("delete", "Löschen");
            Polish.Add("delete", TranslationNeeded);
            French.Add("delete", TranslationNeeded);

            English.Add("warning", "WARNING");
            Gernam.Add("warning", "WARNUNG");
            Polish.Add("warning", "OSTRZEŻENIE");
            French.Add("warning", "ATTENTION");

            English.Add("critical", "CRITICAL");
            Gernam.Add("critical", "KRITISCH");
            Polish.Add("critical", "BŁĄD KRYTYCZNY");
            French.Add("critical", "CRITIQUAL");


            English.Add("information", "Information");
            Gernam.Add("information", "Information");
            Polish.Add("information", "Informacja");
            French.Add("information", "information");

            English.Add("select", "Select");
            Gernam.Add("select", "Auswählen");
            Polish.Add("select", "Wybierz");
            French.Add("select", "Sélectionner");

            English.Add("abort", "Abort");
            Gernam.Add("abort", "Abbrechen");
            Polish.Add("abort", "Anulować");
            French.Add("abort", "Annuler");

            English.Add("error", "Error");
            Gernam.Add("error", "Fehler");
            Polish.Add("error", "Błąd");
            French.Add("error", "Erreur");

            English.Add("retry", "Retry");
            Gernam.Add("retry", "Wiederholen");
            Polish.Add("retry", "Spróbować ponownie");
            French.Add("retry", "Reaissayer");

            English.Add("ignore", "Ignore");
            Gernam.Add("ignore", "Ignorieren");
            Polish.Add("ignore", "Ignorować");
            French.Add("ignore", "Ignorer");

            English.Add("lastUpdated", "Last Updated: ");
            Gernam.Add("lastUpdated", "Letzte Aktualisierung: ");
            Polish.Add("lastUpdated", "Ostatnio Zaktualizowano: ");
            French.Add("lastUpdated", "Dernière mise à jour: ");

            English.Add("stepsComplete", "tasks completed");
            Gernam.Add("stepsComplete", "erledigte Aufgaben");
            Polish.Add("stepsComplete", "zadania zakończone");
            French.Add("stepsComplete", "tâches terminées");

            English.Add("stop", "Stop");
            Gernam.Add("stop", "Stop");
            Polish.Add("stop", "Przerwać");
            French.Add("stop", "Arrêter");

            English.Add("playPause", "Play/Pause");
            Gernam.Add("playPause", "Play/Pause");
            Polish.Add("playPause", "Odtwórz/wstrzymaj");
            French.Add("playPause", "Jouer/Pauser");
            #endregion

            #region General Messages
            English.Add("conflictBetaDBTestMode", "The command line options '/BetaDatabase' and '/Test' should not be used together, the application may be unstable. Continue anyway?");
            Gernam.Add("conflictBetaDBTestMode", "Die Kommandozeilen Befehle '/BetaDatabase' und '/Test' sollten nicht zusammen verwendet werden, da sonst der ModPack Manager instabil werden könnte. Trotzdem fortfahren?");
            Polish.Add("conflictBetaDBTestMode", "Opcje Beta wiersza poleceń dotyczące Bazy Danych i Testu nie powinny być używane jednocześnie, aplikacja może nie być stabilna. Czy kontynuować mimo wszystko?");
            French.Add("conflictBetaDBTestMode", "Les options de ligne de commande '/BetaDatabase' et '/Test' ne doivent pas être utilisées ensemble, l'application peut être instable. Continuer quand même?");

            English.Add("conflictsCommandlineHeader", "Command-Line option conflicts");
            Gernam.Add("conflictsCommandlineHeader", "Konflikte in den Kommandozeilen Befehlen");
            Polish.Add("conflictsCommandlineHeader", "Konflikt opcji wiersza poleceń.");
            French.Add("conflictsCommandlineHeader", "Conflits d'options de ligne de commande");
            #endregion

            #region BackgroundForum
            //Component: MenuItemRestore
            //The menu item for restoring the application
            English.Add("MenuItemRestore", "Restore");
            Gernam.Add("MenuItemRestore", "Wiederherstellen");
            Polish.Add("MenuItemRestore", "Przywróć");
            French.Add("MenuItemRestore", "Restaurer");

            //Component: MenuItemCheckUpdates
            //The menu item for restoring the application
            English.Add("MenuItemCheckUpdates", "Check for Updates");
            Gernam.Add("MenuItemCheckUpdates", "Nach Updates suchen");
            Polish.Add("MenuItemCheckUpdates", "Sprawdź aktualizacje");
            French.Add("MenuItemCheckUpdates", "Vérifier les mises à jour");

            //Component: MenuItemAppClose
            //The menu item for restoring the application
            English.Add("MenuItemAppClose", "Close");
            Gernam.Add("MenuItemAppClose", "Schließen");
            Polish.Add("MenuItemAppClose", "Zamknij");
            French.Add("MenuItemAppClose", "Fermer");

            //Component: newDBApplied
            //MessageBox for when a new database version is applied
            English.Add("newDBApplied", "New database version applied");
            Gernam.Add("newDBApplied", "Neue Datenbankversion angewendet");
            Polish.Add("newDBApplied", "Zastosowano nową bazę danych");
            French.Add("newDBApplied", "Nouvelle version de base de données appliquée");
            #endregion

            #region Main Window
            //Component: InstallModpackButton
            //The button for installing the modpack
            English.Add("InstallModpackButton", "Start mod selection");
            Gernam.Add("InstallModpackButton", "Auswahl der Mods");
            Polish.Add("InstallModpackButton", "Przejdź Do Wyboru Modyfikacji");
            French.Add("InstallModpackButton", "Sélection des mods");

            //Component: InstallModpackButtonDescription
            //
            English.Add("InstallModpackButtonDescription", "Select the mods you want to install to your WoT client");
            Gernam.Add("InstallModpackButtonDescription", TranslationNeeded);
            Polish.Add("InstallModpackButtonDescription", TranslationNeeded);
            French.Add("InstallModpackButtonDescription", TranslationNeeded);

            //Component: UninstallModpackButton
            //
            English.Add("UninstallModpackButton", "Uninstall Relhax Modpack");
            Gernam.Add("UninstallModpackButton", "Relhax Modpack deinstallieren");
            Polish.Add("UninstallModpackButton", "Odinstaluj Paczkę Relhax");
            French.Add("UninstallModpackButton", "Désinstaller Relhax Modpack");

            //Component: UninstallModpackButtonDescription
            //
            English.Add("UninstallModpackButtonDescription", "Remove *all* mods installed to your WoT client");
            Gernam.Add("UninstallModpackButtonDescription", TranslationNeeded);
            Polish.Add("UninstallModpackButtonDescription", TranslationNeeded);
            French.Add("UninstallModpackButtonDescription", TranslationNeeded);

            //Component: ViewNewsButton
            //
            English.Add("ViewNewsButton", "View update news");
            Gernam.Add("ViewNewsButton", TranslationNeeded);
            Polish.Add("ViewNewsButton", TranslationNeeded);
            French.Add("ViewNewsButton", TranslationNeeded);

            //Component: ViewNewsButtonDescription
            //
            English.Add("ViewNewsButtonDescription", "View application, database, and other update news");
            Gernam.Add("ViewNewsButtonDescription", TranslationNeeded);
            Polish.Add("ViewNewsButtonDescription", TranslationNeeded);
            French.Add("ViewNewsButtonDescription", TranslationNeeded);

            //Component: ForceManuelGameDetectionCB
            //
            English.Add("ForceManuelGameDetectionCB", "Force manual game detection");
            Gernam.Add("ForceManuelGameDetectionCB", "Erzwinge manuelle Spielerkennung");
            Polish.Add("ForceManuelGameDetectionCB", "Wymuś ręczną weryfikację ścieżki gry");
            French.Add("ForceManuelGameDetectionCB", "Forcer détection manuel");

            //Component: ForceManuelGameDetectionCBDescription
            //
            English.Add("ForceManuelGameDetectionCBDescription", "This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");
            Gernam.Add("ForceManuelGameDetectionCBDescription", "Diese Option ist für die manuelle selektion des World of Tanks Spiel-" +
                    "speicherortes. Nutze dies wenn Du Probleme mit der automatischen Suche des Spiels hast.");
            Polish.Add("ForceManuelGameDetectionCBDescription", "Ta opcja wymusza ręczne znalezienie lokacji gry World of Tanks." +
                    "Zaznacz, jeśli masz problem z automatycznym znalezieniem ścieżki dostępu do gry.");
            French.Add("ForceManuelGameDetectionCBDescription", "Cette option consiste à forcer une détection manuel" +
                    "de World of Tanks. Sélectionnez cette option si vous rencontrez des problèmes pour localiser automatiquement le jeu.");

            //Component: LanguageSelectionGBHeader
            //
            English.Add("LanguageSelectionGBHeader", "Language selection");
            Gernam.Add("LanguageSelectionGBHeader", "Sprachauswahl");
            Polish.Add("LanguageSelectionGBHeader", "Wybór języka");
            French.Add("LanguageSelectionGBHeader", "Choix de langue");

            //Component: Forms_ENG_NAButtonDescription
            English.Add("Forms_ENG_NAButtonDescription", "Go to the English-speaking 'World of Tanks' forum page for the NA server");
            Gernam.Add("Forms_ENG_NAButtonDescription", "Gehe zur englischsprachigen 'World of Tanks' Forum Seite für den NA Server");
            Polish.Add("Forms_ENG_NAButtonDescription", TranslationNeeded);
            French.Add("Forms_ENG_NAButtonDescription", TranslationNeeded);

            //Component: FormsENG_EUButtonDescription
            English.Add("Forms_ENG_EUButtonDescription", "Go to the English-speaking 'World of Tanks' forum page for the EU server");
            Gernam.Add("Forms_ENG_EUButtonDescription", "Gehe zur englischsprachigen 'World of Tanks' Forum Seite für den EU Server");
            Polish.Add("Forms_ENG_EUButtonDescription", TranslationNeeded);
            French.Add("Forms_ENG_EUButtonDescription", TranslationNeeded);

            //Component: FormsENG_GERButtonDescription
            English.Add("Forms_GER_EUButtonDescription", "Go to the Gernam-speaking 'World of Tanks' forum page for the EU server");
            Gernam.Add("Forms_GER_EUButtonDescription", "Gehe zur deutschsprachigen 'World of Tanks' Forum Seite für den EU Server");
            Polish.Add("Forms_GER_EUButtonDescription", TranslationNeeded);
            French.Add("Forms_GER_EUButtonDescription", TranslationNeeded);

            //Component: SaveUserDataCB
            //
            English.Add("SaveUserDataCB", "Save user data");
            Gernam.Add("SaveUserDataCB", "Mod Daten speichern");
            Polish.Add("SaveUserDataCB", "Zapisz ustawienia użytkownika");
            French.Add("SaveUserDataCB", "Sauvegarder les données utilisateur");

            //Component:SaveUserDataCBDescription
            //
            English.Add("SaveUserDataCBDescription", "If this is selected, the installer will save user created data (like session stats from previous battles)");
            Gernam.Add("SaveUserDataCBDescription", "Wenn dies ausgewählt ist, speichert das Installationsprogramm die vom Benutzer erstellten Daten (wie Sitzungsstatistiken aus früheren Gefechten)");
            Polish.Add("SaveUserDataCBDescription", "Przy zaznaczeniu, instalator zachowa pliki danych użytkownika (takie jak statystyki sesji z poprzednich bitew)");
            French.Add("SaveUserDataCBDescription", "Si cette option est sélectionnée, l'installateur vas sauvegarder les données créé par l'utilisateur (Comme les stats de la session des batailles précédentes");

            //Component: CleanInstallCB
            //
            English.Add("CleanInstallCB", "Clean installation (recommended)");
            Gernam.Add("CleanInstallCB", "Saubere Installation (empfohlen)");
            Polish.Add("CleanInstallCB", "Czysta instalacja (Zalecane)");
            French.Add("CleanInstallCB", "Installation propre (Recommandé)");

            //Component: CleanInstallCBDescription
            //
            English.Add("CleanInstallCBDescription", "This recommended option will empty your res_mods folder before installing" +
                    " your new mod selections. Unless you know what you are doing, it is recommended that you keep this on to avoid problems.");
            Gernam.Add("CleanInstallCBDescription", "Diese empfohlene Option leert den Ordner res_mods vor der Installation" +
                    "Deiner neuen Mod-Auswahl. Bis du genau weißt, was du tust, empfohlen wir, dass du das weiter behältst, um Probleme zu vermeiden.");
            Polish.Add("CleanInstallCBDescription", "To rekomendowane ustawienie usunie zawartość folderu res_mods przed instalacją" +
                     "nowej konfiguracji modów. Jeśli nie wiesz co robisz zalecamy włączyć tą opcję, aby uniknąć problemów.");
            French.Add("CleanInstallCBDescription", "Cette option recommandé vas nettoyer le dossier res_mods avant d'installer" +
                    "votre nouvelle sélection de mods. À moins que vous ne sachiez ce que vous faites, il est recommandé de laisser ceci activé pour éviter des problèmes.");

            //Component: BackupModsCB
            //
            English.Add("BackupModsCB", "Backup current mods folder");
            Gernam.Add("BackupModsCB", "Sicherung des aktuellen Modsordner");
            Polish.Add("BackupModsCB", "Zrób kopię zapasową obecnego pliku z modyfikacjami");
            French.Add("BackupModsCB", "Sauvegarder le dossier de mods");

            //Component: backupModsSizeLabel
            //
            English.Add("backupModsSizeLabel", "");
            Gernam.Add("backupModsSizeLabel", "");
            Polish.Add("backupModsSizeLabel", "");
            French.Add("backupModsSizeLabel", "");

            //Component: backupModsSizeLabelUsed
            //
            English.Add("backupModsSizeLabelUsed", "Backups: {0}  Size: {1}");
            Gernam.Add("backupModsSizeLabelUsed", "Sicherungen: {0}  Größe: {1}");
            Polish.Add("backupModsSizeLabelUsed", TranslationNeeded);
            French.Add("backupModsSizeLabelUsed", TranslationNeeded);

            //Component: backupModsSizeLabelDescription
            //
            English.Add("backupModsSizeLabelDescription", "Here you see the number and size of created backup folders during the last installation with activated backup option.\nClick on it to open the Backup folder viwer and you are able to delete folders by your selection.");
            Gernam.Add("backupModsSizeLabelDescription", TranslationNeeded);
            Polish.Add("backupModsSizeLabelDescription", TranslationNeeded);
            French.Add("backupModsSizeLabelDescription", TranslationNeeded);

            //Component: BackupModsCBDescription
            //
            English.Add("BackupModsCBDescription", "Select this to make a backup of your current res_mods folder." +
                    "They are stored in the 'RelHaxModBackup' folder, saved in a folder inside by a time stamp.");
            Gernam.Add("BackupModsCBDescription", "Wähle diese Option, um eine Sicherungskopie Deines aktuellen res_mods-Ordners zu erstellen." +
                    "Sie werden im Ordner 'RelHaxModBackup' gespeichert und in einem Ordner nach innen durch einen Zeitstempel gespeichert.");
            Polish.Add("BackupModsCBDescription", "Zaznacz, aby zrobić kopię zapasową folderu res_mods." +
                     "Pliki będą przechowane w folderze RelHaxModBackup, zapisane w folderze oznaczonym datą.");
            French.Add("BackupModsCBDescription", "Choisissez ceci pour faire une sauvegarde du dossier res_mods actuel");

            //Component: SaveLastInstallCB
            //
            English.Add("SaveLastInstallCB", "Save selection of last install");
            Gernam.Add("SaveLastInstallCB", "Speicherung der letzten Installation");
            Polish.Add("SaveLastInstallCB", "Zapisz ostatnią konfigurację instalacji");
            French.Add("SaveLastInstallCB", "Sauvegarder la denière configuration");

            //Component: SaveLastInstallCBDescription
            //
            English.Add("SaveLastInstallCBDescription", "If this is selected, the installer will, upon selection window showing, load the last installed config you used.");
            Gernam.Add("SaveLastInstallCBDescription", "Wenn dies ausgewählt ist, lädt das Installationsprogramm die zuletzt installierte Konfiguration im Auswahlfenster, die Du verwendet hast.");
            Polish.Add("SaveLastInstallCBDescription", "Przy zaznaczeniu, instalator załaduje ostatnią użytą konfigurację w oknie wyboru modyfikacji.");
            French.Add("SaveLastInstallCBDescription", "Si cette option est sélectionnée, L'installateur affichera, lors de la fenêtre de sélection, la denière configuration que vous avez utilisé");

            //Component: MinimizeToSystemTray
            //
            English.Add("MinimizeToSystemTray", "Minimize to system tray");
            Gernam.Add("MinimizeToSystemTray", TranslationNeeded);
            Polish.Add("MinimizeToSystemTray", TranslationNeeded);
            French.Add("MinimizeToSystemTray", TranslationNeeded);

            //Component: MinimizeToSystemTrayDescription
            //
            English.Add("MinimizeToSystemTrayDescription", "When checked, the application will continue to run in the system tray when you press close");
            Gernam.Add("MinimizeToSystemTrayDescription", TranslationNeeded);
            Polish.Add("MinimizeToSystemTrayDescription", TranslationNeeded);
            French.Add("MinimizeToSystemTrayDescription", TranslationNeeded);

            //Component: VerboseLoggingCB
            //
            English.Add("VerboseLoggingCB", "Verbose Logging");
            Gernam.Add("VerboseLoggingCB", TranslationNeeded);
            Polish.Add("VerboseLoggingCB", TranslationNeeded);
            French.Add("VerboseLoggingCB", TranslationNeeded);

            //Component: VerboseLoggingCBDescription
            //
            English.Add("VerboseLoggingCBDescription", "Enable more logging messages in the log file. Useful for reporting bugs");
            Gernam.Add("VerboseLoggingCBDescription", TranslationNeeded);
            Polish.Add("VerboseLoggingCBDescription", TranslationNeeded);
            French.Add("VerboseLoggingCBDescription", TranslationNeeded);

            //Component: AllowStatsGatherCB
            //
            English.Add("AllowStatsGatherCB", "Allow statistics gathering of mod usage");
            Gernam.Add("AllowStatsGatherCB", TranslationNeeded);
            Polish.Add("AllowStatsGatherCB", TranslationNeeded);
            French.Add("AllowStatsGatherCB", TranslationNeeded);

            //Component: AllowStatsGatherCBDescription
            //
            English.Add("AllowStatsGatherCBDescription", "Allow the installer to upload anonymous statistic data to the server about mod selections. This allows us to prioritize our support");
            Gernam.Add("AllowStatsGatherCBDescription", TranslationNeeded);
            Polish.Add("AllowStatsGatherCBDescription", TranslationNeeded);
            French.Add("AllowStatsGatherCBDescription", TranslationNeeded);

            //Component: DisableTriggersCB
            //
            English.Add("DisableTriggersCB", "Verbose Logging");
            Gernam.Add("DisableTriggersCB", TranslationNeeded);
            Polish.Add("DisableTriggersCB", TranslationNeeded);
            French.Add("DisableTriggersCB", TranslationNeeded);

            //Component: DisableTriggersCBDescription
            //
            English.Add("DisableTriggersCBDescription", "When checked, some installation tasks will happen at the end after extraction is complete." +
                " When unchecked, it will start some tasks (like building the contout icons), after all assets required for it have been extracted." +
                " If you are using \"User Mods\", it is recommended to turn this off.");
            Gernam.Add("DisableTriggersCBDescription", TranslationNeeded);
            Polish.Add("DisableTriggersCBDescription", TranslationNeeded);
            French.Add("DisableTriggersCBDescription", TranslationNeeded);

            //Component: loadingImageGroupBox
            //
            English.Add("loadingImageGroupBox", "Loading image");
            Gernam.Add("loadingImageGroupBox", "Ladebild");
            Polish.Add("loadingImageGroupBox", "Ładowanie obrazka");
            French.Add("loadingImageGroupBox", "Image de chargement");

            //Component: StandardImageRB
            //
            English.Add("StandardImageRB", "Standard");
            Gernam.Add("StandardImageRB", "Standard");
            Polish.Add("StandardImageRB", "Podstawowe");
            French.Add("StandardImageRB", "Standard");

            //Component: 
            //
            English.Add("StandardImageRBDescription", "Select a loading gif for the mod preview window.");
            Gernam.Add("StandardImageRBDescription", "Wähle ein Lade-Gif fuer das Vorschaufenster des Mods.");
            Polish.Add("StandardImageRBDescription", "Załaduj gif w oknie podglądu.");
            French.Add("StandardImageRBDescription", "Choisissez un GIF de chargement pour l'apercu des mods");

            //Component: ThirdGuardsLoadingImageRB
            //
            AddTranslationToAll("ThirdGuardsLoadingImageRB", "3rdguards");

            //Component: ThirdGuardsLoadingImageRBDescription
            //
            English.Add("ThirdGuardsLoadingImageRBDescription", "Select a loading gif for the mod preview window.");
            Gernam.Add("ThirdGuardsLoadingImageRBDescription", "Wähle ein Lade-Gif fuer das Vorschaufenster des Mods.");
            Polish.Add("ThirdGuardsLoadingImageRBDescription", "Załaduj gif w oknie podglądu.");
            French.Add("ThirdGuardsLoadingImageRBDescription", "Choisissez un GIF de chargement pour l'apercu des mods");

            //Component: CancelDownloadButton
            //
            English.Add("CancelDownloadButton", "Cancel download");
            Gernam.Add("CancelDownloadButton", "Download abbrechen");
            Polish.Add("CancelDownloadButton", "Anuluj pobieranie");
            French.Add("CancelDownloadButton", "Anuler le téléchargement");

            //Component: appDataFolderNotExistHeader
            //
            English.Add("appDataFolderNotExistHeader", "Could not detect WoT cache folder");
            Gernam.Add("appDataFolderNotExistHeader", "Konnte den Cache-Ordner WoT nicht erkennen");
            Polish.Add("appDataFolderNotExistHeader", "Nie wykryto foldera WoT cache");
            French.Add("appDataFolderNotExistHeader", "Impossible de détecter le dossier de cache WoT");


            //Component: appDataFolderNotExist
            //
            English.Add("appDataFolderNotExist", "The installer could not detect the WoT cache folder. Continue the installation witout clearing WoT cache?");
            Gernam.Add("appDataFolderNotExist", "Der Installer konnte den WoT-Cache-Ordner nicht erkennen. Installation fortsetzen ohne den WoT-Cache zu löschen?");
            Polish.Add("appDataFolderNotExist", "Instalato nie wykrył foldera cache. Czy kontynuować bez czyszczenia folderu cache?");
            French.Add("appDataFolderNotExist", "L'installateur n'as pas pus détecter le dossier de cache WoT. Continuer l'installation sans nettoyer le cache?");

            //Component: viewAppUpdates
            //
            English.Add("viewAppUpdates", "View latest application updates");
            Gernam.Add("viewAppUpdates", "Programmaktualisierungen anzeigen");
            Polish.Add("viewAppUpdates", "Pokaż ostatnie zmiany w aplikacji");
            French.Add("viewAppUpdates", "Afficher les dernières mises à jour de l'applications");

            //Component: viewDBUpdates
            //
            English.Add("viewDBUpdates", "View latest database updates");
            Gernam.Add("viewDBUpdates", "Datenbankaktualisierungen anzeigen");
            Polish.Add("viewDBUpdates", "Pokaż ostatnie zmiany w bazie danych");
            French.Add("viewDBUpdates", "Afficher les dernières mises à jour de la base de données");

            //Component: EnableColorChangeDefaultCB
            //
            English.Add("EnableColorChangeDefaultCB", "Enable color change");
            Gernam.Add("EnableColorChangeDefaultCB", "Farbwechsel");
            Polish.Add("EnableColorChangeDefaultCB", "Włącz zmianê kolorów");
            French.Add("EnableColorChangeDefaultCB", "Activer les changements de couleurs");

            //Component: EnableColorChangeDefaultV2CB
            //
            English.Add("EnableColorChangeDefaultV2CB", "Enable color change");
            Gernam.Add("EnableColorChangeDefaultV2CB", "Farbwechsel");
            Polish.Add("EnableColorChangeDefaultV2CB", "Włącz zmianê kolorów");
            French.Add("EnableColorChangeDefaultV2CB", "Activer les changements de couleurs");

            //Component: EnableColorChangeLegacyCB
            //
            English.Add("EnableColorChangeLegacyCB", "Enable color change");
            Gernam.Add("EnableColorChangeLegacyCB", "Farbwechsel");
            Polish.Add("EnableColorChangeLegacyCB", "Włącz zmianê kolorów");
            French.Add("EnableColorChangeLegacyCB", "Activer les changements de couleurs");

            //Component: ClearLogFilesCB
            //
            English.Add("ClearLogFilesCB", "Clear log files");
            Gernam.Add("ClearLogFilesCB", "Protokolldatei löschen");
            Polish.Add("ClearLogFilesCB", "Wyczyść logi");
            French.Add("ClearLogFilesCB", "Effacer les fichiers logs");

            //Component: ClearLogFilesCBDescription
            //
            English.Add("ClearLogFilesCBDescription", "Clear the WoT log files, (python.log), as well as xvm log files (xvm.log) and pmod log files (pmod.log)");
            Gernam.Add("ClearLogFilesCBDescription", "Löschen der WoT Protokolldatei, sowie XVM und PMOD Protokolldatei");
            Polish.Add("ClearLogFilesCBDescription", "Wyczyść logi WoTa (python.log), XVM'a (xvm.log) i pmod'ów (pmod.log).");
            French.Add("ClearLogFilesCBDescription", "Effacez les fichiers logs WoT (python.log), ainsi que les fichiers logs xvm (xvm.log) et les fichiers logs pmod (pmod.log)");

            //Component: CreateShortcutsCB
            //
            English.Add("CreateShortcutsCB", "Create desktop shortcuts");
            Gernam.Add("CreateShortcutsCB", "Erstelle desktop verknüpfungen");
            Polish.Add("CreateShortcutsCB", "Stwórz skróty na pulpicie");
            French.Add("CreateShortcutsCB", "Créer des raccourcis sur le bureau");

            //Component: DeleteOldCacheFiles
            //
            English.Add("DeleteOldCacheFiles", "Delete old cache files");
            Gernam.Add("DeleteOldCacheFiles", TranslationNeeded);
            Polish.Add("DeleteOldCacheFiles", TranslationNeeded);
            French.Add("DeleteOldCacheFiles", TranslationNeeded);

            //Component: DeleteOldCacheFilesDescription
            //
            English.Add("DeleteOldCacheFilesDescription", "Delete any zip files that are no longer used by the installer in the \"RelhaxDownloads\" folder to free up disk space");
            Gernam.Add("DeleteOldCacheFilesDescription", TranslationNeeded);
            Polish.Add("DeleteOldCacheFilesDescription", TranslationNeeded);
            French.Add("DeleteOldCacheFilesDescription", TranslationNeeded);

            //Component: AutoInstallCB
            //
            English.Add("AutoInstallCB", "Enable auto install (NEW)");
            Gernam.Add("AutoInstallCB", TranslationNeeded);
            Polish.Add("AutoInstallCB", TranslationNeeded);
            French.Add("AutoInstallCB", TranslationNeeded);

            //Component: AutoInstallCBDescription
            //
            English.Add("AutoInstallCBDescription", "When a selection file and time is set below, the installer will automatically check for updates to your mods and apply them");
            Gernam.Add("AutoInstallCBDescription", TranslationNeeded);
            Polish.Add("AutoInstallCBDescription", TranslationNeeded);
            French.Add("AutoInstallCBDescription", TranslationNeeded);

            //Component: OneClickInstallCB
            //
            English.Add("OneClickInstallCB", "Enable one-click install");
            Gernam.Add("OneClickInstallCB", TranslationNeeded);
            Polish.Add("OneClickInstallCB", TranslationNeeded);
            French.Add("OneClickInstallCB", TranslationNeeded);

            //Component: OneClickInstallCBDescription
            //
            English.Add("OneClickInstallCBDescription", "Enable the installer to automatically load your selection file and install it");
            Gernam.Add("OneClickInstallCBDescription", TranslationNeeded);
            Polish.Add("OneClickInstallCBDescription", TranslationNeeded);
            French.Add("OneClickInstallCBDescription", TranslationNeeded);

            //Component: ForceEnabledCB
            //
            English.Add("ForceEnabledCB", "Force all packages enabled [!]");
            Gernam.Add("ForceEnabledCB", TranslationNeeded);
            Polish.Add("ForceEnabledCB", TranslationNeeded);
            French.Add("ForceEnabledCB", TranslationNeeded);

            //Component: ForceEnabledCBDescription
            //
            English.Add("ForceEnabledCBDescription", "Causes all packages to be enabled. Can lead to severe stability issues of your installation");
            Gernam.Add("ForceEnabledCBDescription", TranslationNeeded);
            Polish.Add("ForceEnabledCBDescription", TranslationNeeded);
            French.Add("ForceEnabledCBDescription", TranslationNeeded);

            //Component: ForceVisibleCB
            //
            English.Add("ForceVisibleCB", "Force all packages visible [!]");
            Gernam.Add("ForceVisibleCB", TranslationNeeded);
            Polish.Add("ForceVisibleCB", TranslationNeeded);
            French.Add("ForceVisibleCB", TranslationNeeded);

            //Component: ForceVisibleCBDescription
            //
            English.Add("ForceVisibleCBDescription", "Causes all packages to be visible. Can lead to severe stability issues of your installation");
            Gernam.Add("ForceVisibleCBDescription", TranslationNeeded);
            Polish.Add("ForceVisibleCBDescription", TranslationNeeded);
            French.Add("ForceVisibleCBDescription", TranslationNeeded);

            //Component: LoadAutoSyncSelectionFile
            //
            English.Add("LoadAutoSyncSelectionFile", "Load selection file");
            Gernam.Add("LoadAutoSyncSelectionFile", TranslationNeeded);
            Polish.Add("LoadAutoSyncSelectionFile", TranslationNeeded);
            French.Add("LoadAutoSyncSelectionFile", TranslationNeeded);

            //Component: DeveloperSettingsHeader
            //
            English.Add("DeveloperSettingsHeader", "Developer Settings [!]");
            Gernam.Add("DeveloperSettingsHeader", TranslationNeeded);
            Polish.Add("DeveloperSettingsHeader", TranslationNeeded);
            French.Add("DeveloperSettingsHeader", TranslationNeeded);

            //Component: CreateShortcutsCBDescription
            //
            English.Add("CreateShortcutsCBDescription", "When selected, it will create shortcut icons on your desktop for mods that are exe files (like WWIIHA configuration)");
            Gernam.Add("CreateShortcutsCBDescription", "Wenn diese Option aktiviert ist, werden bei der Installation die Verknüpfungen für \"World of Tanks\", \"World of Tanks launcher\" und, wenn bei" +
                " der Installation aktiviert, auch andere Verknüpfungen zu Konfigurationsprogrammen erstellt (z.B. WWIIHA Konfiguration)");
            Polish.Add("CreateShortcutsCBDescription", "Kiedy zaznaczone, utworzone zostaną skróty na pulpicie do modyfikacji z plikami exe (np. konfiguracja WWIIHA)");
            French.Add("CreateShortcutsCBDescription", "Une fois sélectionné, L'installation créera des icônes de raccourci sur votre bureau pour les mods qui ont des fichiers .exe (comme la configuration WWIIHA)");

            //Component: InstallWhileDownloadingCB
            //
            English.Add("InstallWhileDownloadingCB", "Extract while downloading");
            Gernam.Add("InstallWhileDownloadingCB", TranslationNeeded);
            Polish.Add("InstallWhileDownloadingCB", TranslationNeeded);
            French.Add("InstallWhileDownloadingCB", TranslationNeeded);

            //Component: InstallWhileDownloadingCBDescription
            //
            English.Add("InstallWhileDownloadingCBDescription", "When enabled, the installer will extract a zip file as soon as it is downloaded, rather than waiting for every zip file to be downloaded" +
                " before extraction. Recommended for those with Solid State Drives (SSD) only.");
            Gernam.Add("InstallWhileDownloadingCBDescription", "Wenn aktiviert, der Installer wird die Zip-Dateien sofort nach dem Download entpacken und nicht erst auf das Herunterladen aller Dateien warten" +
                " bevor mit dem Entpacken begonnen wird. Nur empfohlen für Besitzer von SSD Festplatten (Solid State Drives).");
            Polish.Add("InstallWhileDownloadingCBDescription", "Wypakowywanie pobranych plików zip w tle podczas procesu ściągania paczek. Rekomendowany jedynie dla dysków SSD.");
            French.Add("InstallWhileDownloadingCBDescription", "Quand activé , l'installateur vas extraire un fichier zip dès qu'il est télécharger , au lieu d'attendre que chaque fichier zip soit" +
                " télécharger pour l'extraction . Recommandé pour les processeurs de SSD seulement.");

            //Component: MulticoreExtractionCoresCountLabel
            //
            English.Add("MulticoreExtractionCoresCountLabel", "Detected Cores: {0}");
            Gernam.Add("MulticoreExtractionCoresCountLabel", TranslationNeeded);
            Polish.Add("MulticoreExtractionCoresCountLabel", TranslationNeeded);
            French.Add("MulticoreExtractionCoresCountLabel", TranslationNeeded);

            //Component: MulticoreExtractionCoresCountLabelDescription
            //
            English.Add("MulticoreExtractionCoresCountLabelDescription", "Number of logical CPU cores (threads) detected on your system");
            Gernam.Add("MulticoreExtractionCoresCountLabelDescription", TranslationNeeded);
            Polish.Add("MulticoreExtractionCoresCountLabelDescription", TranslationNeeded);
            French.Add("MulticoreExtractionCoresCountLabelDescription", TranslationNeeded);

            //Component: SaveDisabledModsInSelection
            //
            English.Add("SaveDisabledModsInSelection", "Keep disabled mods when saving selection");
            Gernam.Add("SaveDisabledModsInSelection", TranslationNeeded);
            Polish.Add("SaveDisabledModsInSelection", TranslationNeeded);
            French.Add("SaveDisabledModsInSelection", TranslationNeeded);

            //Component: SaveDisabledModsInSelectionDescription
            //
            English.Add("SaveDisabledModsInSelectionDescription", "When a mod is re-enabled, it will be selected from your selection file");
            Gernam.Add("SaveDisabledModsInSelectionDescription", TranslationNeeded);
            Polish.Add("SaveDisabledModsInSelectionDescription", TranslationNeeded);
            French.Add("SaveDisabledModsInSelectionDescription", TranslationNeeded);

            //Component: AdvancedInstallationProgress
            //
            English.Add("AdvancedInstallationProgress", "Show advanced installation window");
            Gernam.Add("AdvancedInstallationProgress", TranslationNeeded);
            Polish.Add("AdvancedInstallationProgress", TranslationNeeded);
            French.Add("AdvancedInstallationProgress", TranslationNeeded);

            //Component: ThemeDefault
            //
            English.Add("ThemeDefault", "Default");
            Gernam.Add("ThemeDefault", TranslationNeeded);
            Polish.Add("ThemeDefault", TranslationNeeded);
            French.Add("ThemeDefault", TranslationNeeded);

            //Component: ThemeDefaultDescription
            //
            English.Add("ThemeDefaultDescription", "Default Theme");
            Gernam.Add("ThemeDefaultDescription", TranslationNeeded);
            Polish.Add("ThemeDefaultDescription", TranslationNeeded);
            French.Add("ThemeDefaultDescription", TranslationNeeded);

            //Component: ThemeDark
            //
            English.Add("ThemeDark", "Dark");
            Gernam.Add("ThemeDark", TranslationNeeded);
            Polish.Add("ThemeDark", TranslationNeeded);
            French.Add("ThemeDark", TranslationNeeded);

            //Component: ThemeDarkDescription
            //
            English.Add("ThemeDarkDescription", "Dark Theme");
            Gernam.Add("ThemeDarkDescription", TranslationNeeded);
            Polish.Add("ThemeDarkDescription", TranslationNeeded);
            French.Add("ThemeDarkDescription", TranslationNeeded);

            //Component: ThemeCustom
            //
            English.Add("ThemeCustom", "Custom");
            Gernam.Add("ThemeCustom", TranslationNeeded);
            Polish.Add("ThemeCustom", TranslationNeeded);
            French.Add("ThemeCustom", TranslationNeeded);

            //Component: ThemeCustomDescription
            //
            English.Add("ThemeCustomDescription", "Custom Theme");
            Gernam.Add("ThemeCustomDescription", TranslationNeeded);
            Polish.Add("ThemeCustomDescription", TranslationNeeded);
            French.Add("ThemeCustomDescription", TranslationNeeded);

            //Component: DumpColorSettingsButton
            //
            English.Add("DumpColorSettingsButton", "Dump color settings to xml");
            Gernam.Add("DumpColorSettingsButton", TranslationNeeded);
            Polish.Add("DumpColorSettingsButton", TranslationNeeded);
            French.Add("DumpColorSettingsButton", TranslationNeeded);

            //Component: DumpColorSettingsButtonDescription
            //
            English.Add("DumpColorSettingsButtonDescription", "Writes an xml document of all components that can have a custom color applied, to make a custom theme");
            Gernam.Add("DumpColorSettingsButtonDescription", TranslationNeeded);
            Polish.Add("DumpColorSettingsButtonDescription", TranslationNeeded);
            French.Add("DumpColorSettingsButtonDescription", TranslationNeeded);

            //Component: AdvancedInstallationProgressDescription
            //
            English.Add("AdvancedInstallationProgressDescription", "Shows an advanced installation window during extraction, useful when you have multicore extraction enabled");
            Gernam.Add("AdvancedInstallationProgressDescription", TranslationNeeded);
            Polish.Add("AdvancedInstallationProgressDescription", TranslationNeeded);
            French.Add("AdvancedInstallationProgressDescription", TranslationNeeded);

            //Component: MulticoreExtractionCB
            //
            English.Add("MulticoreExtractionCB", "Multicore extraction mode (experimental)");
            Gernam.Add("MulticoreExtractionCB", "Mehrkern Extraktion (experimentell)");
            Polish.Add("MulticoreExtractionCB", "Wsparcie wielu rdzeni (eksperymentalne)");
            French.Add("MulticoreExtractionCB", "Mode d'extraction multicoeur (expérimental)");

            //Component: MulticoreExtractionCBDescription
            //
            English.Add("MulticoreExtractionCBDescription", "When enabled, the installer will use a parallel extraction method. It will extract multiple zip files at the same time," +
                " reducing install time. For SSD drives ONLY.");
            Gernam.Add("MulticoreExtractionCBDescription", "Wenn aktiviert, wird der Installer den parallelen Entpack-Modus verwenden. Er wird mehrere Zip-Dateien gleichzeitig entpacken" +
                " und dadurch die Installationszeit reduziert. Nur für SSD Festplatten.");
            Polish.Add("MulticoreExtractionCBDescription", "Metoda wypakowywania równoległego. Nastąpi wypakowywanie wielu plików zip jednocześnie, by skrócić czas instalacji." +
                " Jedynie dla dysków SSD.");// I always skip 'When enabled'...
            French.Add("MulticoreExtractionCBDescription", "Lorsqu'il est activé, le programme d'installation utilise une méthode d'extraction parallèle. Il va extraire plusieurs fichiers" +
                " zip en même temps, réduisant ainsi le temps d'installation. Pour les disques SSD SEULEMENT.");

            //Component: UninstallDefault
            //
            English.Add("UninstallDefault", "Default");
            Gernam.Add("UninstallDefault", "Standard");
            Polish.Add("UninstallDefault", "Standard");
            French.Add("UninstallDefault", "Défaut");

            //Component: UninstallQuick
            //
            English.Add("UninstallQuick", "Quick");
            Gernam.Add("UninstallQuick", "Schnell");
            Polish.Add("UninstallQuick", "Szybka");
            French.Add("UninstallQuick", "Rapide");

            //Component: ExportModeCB
            //
            English.Add("ExportModeCB", "Export Mode");
            Gernam.Add("ExportModeCB", "Export-Modus");
            Polish.Add("ExportModeCB", "Tryb wyboru ścieżki wypakowywania");
            French.Add("ExportModeCB", "Mode d'exportation");

            //Component: ExportWindowDesctiption
            //
            English.Add("ExportWindowDesctiption", "Select the version of WoT you wish to export for");
            Gernam.Add("ExportWindowDesctiption", "Wählen Sie die Version von WoT, für die Sie exportieren möchten");
            Polish.Add("ExportWindowDesctiption", "Wybór wersji WoT");
            French.Add("ExportWindowDesctiption", "Sélection de la version de WoT que vous souhaitez exporter");

            //Component: helperText
            //
            English.Add("helperText", "Welcome to the Relhax Modpack! I have tried to make the modpack as straight-forward as possible, but questions may still arise." +
                " Hover over a setting to have it explained. You won't see this dialog box again, unless you delete the settings xml file.");
            Gernam.Add("helperText", "Willkommen im Relhax Modpack! Ich habe versucht, das Modpack so einfach wie möglich zu gestalten, aber Fragen können dennoch entstehen." +
                " Rechtsklick auf eine Einstellung erklärt diese dann. Du siehst diese Dialogbox nicht mehr, ausser du löscht die xml Datei.");
            Polish.Add("helperText", "Witamy w paczce Relhax! Próbowałem stworzyć jak najprostszą w użytku paczkę modyfikacji, ale wciąż możesz mieć pytania." +
                " Kliknik PPM na opcji, by wyświetlić opis. Nie zobaczysz tej wiadomości ponownie, dopóki nie usuniesz pliku ustawień xml.");
            French.Add("helperText", "Bienvenue au Modpack Relhax! J'ai aissayé de faire le modpack le plus simple possible, mais des questions peuvent survenir." +
                " Survolez un paramètre pour voire une explication. Vous n'allez plus voire cette boite, sauf si vous supprimez le fichier de configuration xml ");

            //Component: NotifyIfSameDatabaseCB
            English.Add("NotifyIfSameDatabaseCB", "Inform if no new database available");
            Gernam.Add("NotifyIfSameDatabaseCB", "Hinweis wenn keine Aktuallisierungen erfolgt sind");
            Polish.Add("NotifyIfSameDatabaseCB", "Poinformuj, jeśli nie będzie dostępna nowa baza danych");
            French.Add("NotifyIfSameDatabaseCB", "Informer si aucune nouvelle base de données est disponible");

            //Component: NotifyIfSameDatabaseCBDescription
            //
            English.Add("NotifyIfSameDatabaseCBDescription", "Notify you if your last installed database version is the same. If so, it means that there is no update to any mods.");
            Gernam.Add("NotifyIfSameDatabaseCBDescription", "Dich benachrichtigen: Die letzte verwendete Datenbank ist die selbe, d.h. es gibt keine Aktualisierungen und Veränderungen.");
            Polish.Add("NotifyIfSameDatabaseCBDescription", "Powiadom, jeśli ostatnia zainstalowana wersja bazy danych jest taka sama. Jeśli tak, to nie ma potrzeby aktualizacji modyfikacji.");
            French.Add("NotifyIfSameDatabaseCBDescription", "Avertir si votre dernière version de base de données installée est identique. Si c'est le cas, cela signifie qu'il n'y a pas de mise à jour de mods.");

            //Component: ShowInstallCompleteWindowCB
            English.Add("ShowInstallCompleteWindowCB", "Show install complete window");
            Gernam.Add("ShowInstallCompleteWindowCB", "Zeigt \"Installation fertig\" Fenster");
            Polish.Add("ShowInstallCompleteWindowCB", "Pokaż okno akcji po instalacji");
            French.Add("ShowInstallCompleteWindowCB", "Afficher la fenêtre \"installation terminé\"");

            //Component: ShowInstallCompleteWindowCBDescription
            //
            English.Add("ShowInstallCompleteWindowCBDescription", "Show a window upon installation completion with popular operations to perform after modpack installation, such as launching" +
                " the game, going to the xvm website, etc.");
            Gernam.Add("ShowInstallCompleteWindowCBDescription", "Zeigte am Ende der Installation ein Auswahlfenster mit nützlichen Befehlen an, wie: starte das Spiel, gehe zur XVM Webseite, usw ...");
            Polish.Add("ShowInstallCompleteWindowCBDescription", "Po zakończeniu instalacji otwórz okno dalszych akcji.");
            French.Add("ShowInstallCompleteWindowCBDescription", "Afficher une fenêtre lors de l'achèvement de l'installation avec des opérations populaires à effectuer après l'installation de Modpack," +
                " telles que le lancement du jeu, le site Web de XVM, etc.");

            //Component: applicationVersion
            English.Add("applicationVersion", "Application Version");
            Gernam.Add("applicationVersion", TranslationNeeded);
            Polish.Add("applicationVersion", TranslationNeeded);
            French.Add("applicationVersion", TranslationNeeded);

            //Component: ErrorCounterLabel
            AddTranslationToAll("ErrorCounterLabel", "Error Counter: ");

            //Component: ClearCacheCB
            //
            English.Add("ClearCacheCB", "Clear WoT cache data");
            Gernam.Add("ClearCacheCB", "Cache-Daten für WoT löschen");
            Polish.Add("ClearCacheCB", "Usuń dane WoT cache");
            French.Add("ClearCacheCB", "Nettoyer le dossier de Cache WoT");

            //Component: ClearCachCBDescription
            //
            English.Add("ClearCacheCBDescription", "Clear the WoT cache app data directory. Performs the same task as the similar option that was in OMC.");
            Gernam.Add("ClearCacheCBDescription", "Löschen Sie das WoT-Cache-App-Datenverzeichnis. Führt die gleiche Aufgabe wie die ähnliche Option aus, die in OMC war.");
            Polish.Add("ClearCacheCBDescription", "Usuń dane aplikacji z lokacji WoT cache. Działa na podobnej zasadzie, jak kiedyś opcja z paczki OMC.");
            French.Add("ClearCacheCBDescription", "Nettoyer le dossier cache WoT. Effectue la même tâche que l'option similaire qui était dans OMC.");

            //Component: EnableColorChangeDefaultCBDescription
            //
            English.Add("EnableColorChangeDefaultCBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");
            Gernam.Add("EnableColorChangeDefaultCBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            Polish.Add("EnableColorChangeDefaultCBDescription", "Włącz zmianê kolorów podczas zmiany wyboru modyfikacji i ustawieñ.");
            French.Add("EnableColorChangeDefaultCBDescription", "Activer le changement de les couleurs lors de la selection d'un mod ou d'une config");

            //Component: EnableColorChangeDefaultV2CBDescription
            //
            English.Add("EnableColorChangeDefaultV2CBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");
            Gernam.Add("EnableColorChangeDefaultV2CBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            Polish.Add("EnableColorChangeDefaultV2CBDescription", "Włącz zmianê kolorów podczas zmiany wyboru modyfikacji i ustawieñ.");
            French.Add("EnableColorChangeDefaultV2CBDescription", "Activer le changement de les couleurs lors de la selection d'un mod ou d'une config");

            //Component: EnableColorChangeLegacyCBDescription
            //
            English.Add("EnableColorChangeLegacyCBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");
            Gernam.Add("EnableColorChangeLegacyCBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            Polish.Add("EnableColorChangeLegacyCBDescription", "Włącz zmianê kolorów podczas zmiany wyboru modyfikacji i ustawieñ.");
            French.Add("EnableColorChangeLegacyCBDescription", "Activer le changement de les couleurs lors de la selection d'un mod ou d'une config");

            //Component: DefaultUninstallModeRBDescription
            //
            English.Add("DefaultUninstallModeRBDescription", "Default Uninstall will remove all files in the game's mod directories, including shortcuts and appdata cache files.");
            Gernam.Add("DefaultUninstallModeRBDescription", "Die Standard Deinstallation wird alle Dateien in den Mod-Verzeichnissen des Spieles löschen, inklusive der Verknüpfungen und Dateien im 'AppData' Speicher.");
            Polish.Add("DefaultUninstallModeRBDescription", "Domyślna deinstalacja usunie wszystkie pliki w folderze modyfikacji i pliki z nimi związane, włączając skróty i pliki cache aplikacji.");
            French.Add("DefaultUninstallModeRBDescription", "La méthode de désinstallation par défaut vas supprimer tout les fichiers dans le dossier du jeu, incluant les raccourcies et le fichers de cache appdata");

            //Component: CleanUninstallModeRBDescription
            //
            English.Add("CleanUninstallModeRBDescription", "Quick Uninstall will only remove files in the game's mod directories. It does not remove modpack created shortcuts or appdata cache files.");
            Gernam.Add("CleanUninstallModeRBDescription", "Die schnelle Deinstallation wird nur Dateien in den Mod-Verzeichnissen der Spieles löschen. Es werden keine vom ModPack erstellten Verknüpfungen oder Dateien im 'AppData' Speicher gelöscht.");
            Polish.Add("CleanUninstallModeRBDescription", "Szybka deinstalacja usunie tylko pliki w folderze modyfikacji. Nie usunie skrótów i plików cache związanych z modpackiem.");
            French.Add("CleanUninstallModeRBDescription", "La méthode de désinstallation rapide vas uniquement supprimer les fichiers dans le dossier \"mod\" du jeu. Ceci ne supprimeras pas les raccourcis ou les fichiers de cache appdata créé par le modpack");

            //Component: ExportModeCBDescription
            //Explaiing the export mode
            English.Add("ExportModeCBDescription", "Export mode will allow you to select a folder and WoT version you wish to export your mods installation to. For advanced users only." +
                "Note it will NOT: Unpack game xml files, patch files (provided from the game), or create the atlas files. Instructions can be found in the export directory.");
            Gernam.Add("ExportModeCBDescription", "Der Export-Modus ermöglicht es Ihnen, einen Ordner und WoT-Version zu wählen, in die Sie Ihre Mods-Installation exportieren möchten." +
                " Nur für fortgeschrittene Benutzer. Bitte beachten: es werden KEINE: Spiel-XML-Dateien entpackt und nicht modifiziert oder Atlas Dateien erstellt (jeweils aus dem Spiel" +
                " bereitgestellt). Anweisungen dazu finden Sie im Export-Verzeichnis.");
            Polish.Add("ExportModeCBDescription", "Tryb wyboru ścieżki wypakowania pozwala na wybór folderu i wersji WoT, do których chcesz zainstalować modyfikacje. Tylko dla zaawansowanych użytkowników." +
                " Tryb: nie rozpakuje plików gry xml, plików patchy (zapewnianych przez grę), oraz niestworzy plików atlasu. Instrukcje można znaleźć pod ścieżką wypakowania.");
            French.Add("ExportModeCBDescription", "Le mode Export vous permettra de sélectionner un dossier et la version de WoT vers lesquels vous souhaitez exporter votre installation" +
                " de mods. Pour les utilisateurs avancés, notez que l'installation ne fera pas: Déballez " +
                "les fichiers xml du jeu, corrigez les fichiers (fournis depuis le jeu) ou créez l'atlas. Les instructions peuvent être trouvées dans le répertoire d'exportation.");

            //Component: DiagnosticUtilitiesButton
            //
            English.Add("DiagnosticUtilitiesButton", "Diagnostic utilities");
            Gernam.Add("DiagnosticUtilitiesButton", "Diagnosedienstprogramme");
            Polish.Add("DiagnosticUtilitiesButton", "Narzędzia diagnostyczne");
            French.Add("DiagnosticUtilitiesButton", "Utilitaires de diagnostique");

            //Component: DiagnosticUtilitiesButtonDescription
            //
            English.Add("DiagnosticUtilitiesButtonDescription", "Report a bug, attempt a WG client repair, etc.");
            Gernam.Add("DiagnosticUtilitiesButtonDescription", TranslationNeeded);
            Polish.Add("DiagnosticUtilitiesButtonDescription", TranslationNeeded);
            French.Add("DiagnosticUtilitiesButtonDescription", TranslationNeeded);

            //Component: databaseVersion
            //
            English.Add("databaseVersion", "Latest Database");
            Gernam.Add("databaseVersion", "Datenbank");
            Polish.Add("databaseVersion", "Baza danych");
            French.Add("databaseVersion", "Base de donnés");

            //Component: UninstallModeGroupBox
            //
            English.Add("UninstallModeGroupBox", "Uninstall Mode:");
            Gernam.Add("UninstallModeGroupBox", "Deinstallationsmodus:");
            Polish.Add("UninstallModeGroupBox", "Tryb Deinstalacji:");
            French.Add("UninstallModeGroupBox", "Mode de désinstallation:");

            //Component: UninstallModeGroupBoxDescription
            English.Add("UninstallModeGroupBoxDescription", "Select the uninstall mode to use");
            Gernam.Add("UninstallModeGroupBoxDescription", TranslationNeeded);
            Polish.Add("UninstallModeGroupBoxDescription", TranslationNeeded);
            French.Add("UninstallModeGroupBoxDescription", TranslationNeeded);

            //Component: ShowAdvancedSettingsLinkDescription
            //
            English.Add("ShowAdvancedSettingsLinkDescription", "Show the advanced settings window");
            Gernam.Add("ShowAdvancedSettingsLinkDescription", "Erweiterte Einstellungen anzeigen");
            Polish.Add("ShowAdvancedSettingsLinkDescription", "Pokaż okno ustawieñ zaawansowanych");
            French.Add("ShowAdvancedSettingsLinkDescription", "Afficher le panneau de configurations avancé");

            //Component: FacebookButtonDescription
            English.Add("FacebookButtonDescription", "Go to our Facebook page");
            Gernam.Add("FacebookButtonDescription", "Unsere Facebook Seite aufrufen");
            Polish.Add("FacebookButtonDescription", "Strona FB");
            French.Add("FacebookButtonDescription", "Page Facebook");

            //Component: DiscordButtonDescription
            English.Add("DiscordButtonDescription", "Go to Discord server");
            Gernam.Add("DiscordButtonDescription", "Zum Discord Server");
            Polish.Add("DiscordButtonDescription", "Serwer Discorda");
            French.Add("DiscordButtonDescription", "Serveur Discord");

            //Component: TwitterButtonDescription
            English.Add("TwitterButtonDescription", "Go to our Twitter page");
            Gernam.Add("TwitterButtonDescription", "Unsere Twitter Seite aufrufen");
            Polish.Add("TwitterButtonDescription", "Strona TT");
            French.Add("TwitterButtonDescription", "Page Twitter");

            //Component: SendEmailButtonDescription
            English.Add("SendEmailButtonDescription", "Send us an Email");
            Gernam.Add("SendEmailButtonDescription", "Schicke uns eine Email");
            Polish.Add("SendEmailButtonDescription", "Przeœlij nam wiadomoœæ e-mail");
            French.Add("SendEmailButtonDescription", "Nous envoyer un Email");

            //Component: HomepageButtonDescription
            English.Add("HomepageButtonDescription", "Visit our Website");
            Gernam.Add("HomepageButtonDescription", "Zu unserer Homepage");
            Polish.Add("HomepageButtonDescription", "Odwiedz nasza strone");
            French.Add("HomepageButtonDescription", "Visiter notre site web");

            //Component: DonateButtonDescription
            English.Add("DonateButtonDescription", "Donation for further development");
            Gernam.Add("DonateButtonDescription", "Spende für die Weiterentwicklung");
            Polish.Add("DonateButtonDescription", "Dotacja na dalszy rozwój");
            French.Add("DonateButtonDescription", "Donation pour aider au développement");

            //Component: FindBugAddModButtonDescription
            English.Add("FindBugAddModButtonDescription", "Find a bug? Want a mod added? Report here please!");
            Gernam.Add("FindBugAddModButtonDescription", "Fehler gefunden? Willst Du einen Mod hinzufügen? Bitte hier melden!");
            Polish.Add("FindBugAddModButtonDescription", "Znalazłeś błąd? Chcesz dodać mod?");
            French.Add("FindBugAddModButtonDescription", "Trouvé un bug? Recommandation de mod?");

            //Component: Mod Selection view Group Box
            //
            English.Add("SelectionViewGB", "Selection View");
            Gernam.Add("SelectionViewGB", "Darstellungsart");
            Polish.Add("SelectionViewGB", "Widok wyborów");
            French.Add("SelectionViewGB", "Affichage de sélection");

            //Component: Mod selection view default (relhax)
            //
            English.Add("SelectionDefault", "Default");
            Gernam.Add("SelectionDefault", "Standard");
            Polish.Add("SelectionDefault", "Domyślne");
            French.Add("SelectionDefault", "Normal");

            //Component: Mod selection view default (relhax) [WPF VERSION]
            //
            English.Add("SelectionDefaultV2", "Default V2");
            Gernam.Add("SelectionDefaultV2", "Standard V2");
            Polish.Add("SelectionDefaultV2", "Domyślne V2");
            French.Add("SelectionDefaultV2", "Normal V2");

            //Component: Mod selection view legacy (OMC)
            //
            English.Add("SelectionLegacy", "OMC Legacy");
            Gernam.Add("SelectionLegacy", "OMC Legacy");
            Polish.Add("SelectionLegacy", "OMC Legacy");
            French.Add("SelectionLegacy", "OMC Legacy");

            //Component: Mod selection Description
            //
            English.Add("SelectionLayoutDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            Gernam.Add("SelectionLayoutDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionLayoutDescription", "Wybierz tryb widoku wyborów modyfikacji\nDomyślnie: Tryb widoku listy RelHax\nSpuścizna: Tryb widoku listy OMC");
            French.Add("SelectionLayoutDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");

            //Component: Mod selection Description
            //
            English.Add("SelectionDefaultDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            Gernam.Add("SelectionDefaultDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionDefaultDescription", "Wybierz tryb widoku wyborów modyfikacji\nDomyślnie: Tryb widoku listy RelHax\nSpuścizna: Tryb widoku listy OMC");
            French.Add("SelectionDefaultDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");

            //Component: Mod selection Description
            //
            English.Add("SelectionDefaultV2Description", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            Gernam.Add("SelectionDefaultV2Description", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionDefaultV2Description", "Wybierz tryb widoku wyborów modyfikacji\nDomyślnie: Tryb widoku listy RelHax\nSpuścizna: Tryb widoku listy OMC");
            French.Add("SelectionDefaultV2Description", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");

            //Component: Mod selection Description
            //
            English.Add("SelectionLegacyDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            Gernam.Add("SelectionLegacyDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionLegacyDescription", "Wybierz tryb widoku wyborów modyfikacji\nDomyślnie: Tryb widoku listy RelHax\nSpuścizna: Tryb widoku listy OMC");
            French.Add("SelectionLegacyDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");

            //Component: selectionView_MouseEnter
            //
            English.Add("selectionView_MouseEnter", "Select a mod selection list view.");
            Gernam.Add("selectionView_MouseEnter", "wähle eine der Listenansichten.");
            Polish.Add("selectionView_MouseEnter", "Wybierz listę wyboru modyfikacji.");
            French.Add("selectionView_MouseEnter", "Sélectionnez une vue de liste de sélection de mod.");

            //Component: LanguageSelectionGBDescription
            //
            English.Add("LanguageSelectionGBDescription", "Select your preferred language.");
            Gernam.Add("LanguageSelectionGBDescription", "wähle Deine bevorzugte Sprache");
            Polish.Add("LanguageSelectionGBDescription", "Wybierz preferowany język.");
            French.Add("LanguageSelectionGBDescription", "Sélectionnez votre langue préféré");

            //Component: expandNodesDefault
            //
            English.Add("expandNodesDefault", "Expand all");
            Gernam.Add("expandNodesDefault", "Alle erweitern");
            Polish.Add("expandNodesDefault", "Rozwiń wszystkie");
            French.Add("expandNodesDefault", "Développer tout");

            //Component: expandNodesDefault2
            //
            English.Add("expandNodesDefault2", "Expand all");
            Gernam.Add("expandNodesDefault2", "Alle erweitern");
            Polish.Add("expandNodesDefault2", "Rozwiń wszystkie");
            French.Add("expandNodesDefault2", "Développer tout");

            //Component: expandNodesDefaultDescription
            //
            English.Add("expandNodesDefaultDescription", "Select this to have all options automatically expand. It applies for the Legacy Selection only.");
            Gernam.Add("expandNodesDefaultDescription", "Erweitere alle Einträge auf allen Registerkarten automatisch. Nur bei Ansicht als Baumstruktur.");
            Polish.Add("expandNodesDefaultDescription", "Zaznacz to, aby wszystkie opcje zostały automatycznie rozwinięte. Dotyczy tylko opcji Legacy Selection.");
            French.Add("expandNodesDefaultDescription", "Sélectionnez cette option pour que toutes les options s'élargis automatiquement. S'applique uniquement à la Sélection Legacy.");

            //Component: EnableBordersDefaultCB
            //
            English.Add("EnableBordersDefaultCB", "Enable borders");
            Gernam.Add("EnableBordersDefaultCB", "Einrahmen");
            Polish.Add("EnableBordersDefaultCB", "Włącz granice");
            French.Add("EnableBordersDefaultCB", "Activer les bordures");

            //Component: EnableBordersDefaultV2CB
            //
            English.Add("EnableBordersDefaultV2CB", "Enable borders");
            Gernam.Add("EnableBordersDefaultV2CB", "Einrahmen");
            Polish.Add("EnableBordersDefaultV2CB", "Włącz granice");
            French.Add("EnableBordersDefaultV2CB", "Activer les bordures");

            //Component: EnableBordersLegacyCB
            //
            English.Add("EnableBordersLegacyCB", "Enable borders");
            Gernam.Add("EnableBordersLegacyCB", "Einrahmen");
            Polish.Add("EnableBordersLegacyCB", "Włącz granice");
            French.Add("EnableBordersLegacyCB", "Activer les bordures");

            //Component: EnableBordersDefaultCBDescription
            //
            English.Add("EnableBordersDefaultCBDescription", "Enable the black borders around each mod and config sublevel.");
            Gernam.Add("EnableBordersDefaultCBDescription", "Jede Auswahl schwarz einrahmen");
            Polish.Add("EnableBordersDefaultCBDescription", "Włącz czarne obramowanie modyfikacji i opcji konfiguracji");
            French.Add("EnableBordersDefaultCBDescription", "Activer les bordures noires autour de chaque mod et selections de configuration.");

            //Component: EnableBordersDefaultCBDescription
            //
            English.Add("EnableBordersDefaultV2CBDescription", "Enable the black borders around each mod and config sublevel.");
            Gernam.Add("EnableBordersDefaultV2CBDescription", "Jede Auswahl schwarz einrahmen");
            Polish.Add("EnableBordersDefaultV2CBDescription", "Włącz czarne obramowanie modyfikacji i opcji konfiguracji");
            French.Add("EnableBordersDefaultV2CBDescription", "Activer les bordures noires autour de chaque mod et selections de configuration.");

            //Component: EnableBordersLegacyCBDescription
            //
            English.Add("EnableBordersLegacyCBDescription", "Enable the black borders around each mod and config sublevel.");
            Gernam.Add("EnableBordersLegacyCBDescription", "Jede Auswahl schwarz einrahmen");
            Polish.Add("EnableBordersLegacyCBDescription", "Włącz czarne obramowanie modyfikacji i opcji konfiguracji");
            French.Add("EnableBordersLegacyCBDescription", "Activer les bordures noires autour de chaque mod et selections de configuration.");

            //Component: UseBetaDatabaseCB
            English.Add("UseBetaDatabaseCB", "Use beta database");
            Gernam.Add("UseBetaDatabaseCB", "Die Beta-Version der Datenbank verwenden");
            Polish.Add("UseBetaDatabaseCB", "Użyj wersji beta bazy danych");
            French.Add("UseBetaDatabaseCB", "Utiliser la base de données beta");

            //Component: UseBetaApplicationCB
            English.Add("UseBetaApplicationCB", "Use beta application");
            Gernam.Add("UseBetaApplicationCB", "Die Beta-Version des ModPack Managers verwenden");
            Polish.Add("UseBetaApplicationCB", "Użyj wersji beta bazy aplikacji");
            French.Add("UseBetaApplicationCB", "Utiliser l'application beta");

            //Component: UseBetaDatabaseCBDescription
            English.Add("UseBetaDatabaseCBDescription", "Use the latest beta database. Mod stability is not guaranteed");
            Gernam.Add("UseBetaDatabaseCBDescription", "Verwende die letzte Beta-Version des ModPack Datenbank. Die Stabilität der Mods kann nicht garantiert werden, jedoch werden hier auch Fehlerbehebungen als erstes getestet und implementiert.");
            Polish.Add("UseBetaDatabaseCBDescription", "Użyj ostatniej wersji beta bazy danych. Nie gwarantujemy stabilności modyfikacji.");
            French.Add("UseBetaDatabaseCBDescription", "Utiliser la dernière base de données beta. La stabilité des mods n'est pas garantie");

            //Component: UseBetaApplicationCBDescription
            English.Add("UseBetaApplicationCBDescription", "Use the latest beta application. Translations and application stability are not guaranteed");
            Gernam.Add("UseBetaApplicationCBDescription", "Verwende die letzte Beta-Version des ModPack Managers. Fehlerfreie Übersetzungen und Programmstabilität können nicht garantiert werden.");
            Polish.Add("UseBetaApplicationCBDescription", "Użyj ostatniej wersji beta aplikacji. Nie gwarantujemy stabilności ani tłumaczenia aplikacji.");
            French.Add("UseBetaApplicationCBDescription", "Utiliser la dernière version beta. Les traductions et la stabilité de l'application ne sont pas garanties");

            //Component: SettingsTabIntroHeader
            //
            English.Add("SettingsTabIntroHeader", "Welcome!");
            Gernam.Add("SettingsTabIntroHeader", TranslationNeeded);
            Polish.Add("SettingsTabIntroHeader", TranslationNeeded);
            French.Add("SettingsTabIntroHeader", TranslationNeeded);

            //Component: SettingsTabSelectionViewHeader
            //
            English.Add("SettingsTabSelectionViewHeader", "Selection View");
            Gernam.Add("SettingsTabSelectionViewHeader", TranslationNeeded);
            Polish.Add("SettingsTabSelectionViewHeader", TranslationNeeded);
            French.Add("SettingsTabSelectionViewHeader", TranslationNeeded);

            //Component: SettingsTabInstallationSettingsHeader
            //
            English.Add("SettingsTabInstallationSettingsHeader", "Installation Settings");
            Gernam.Add("SettingsTabInstallationSettingsHeader", TranslationNeeded);
            Polish.Add("SettingsTabInstallationSettingsHeader", TranslationNeeded);
            French.Add("SettingsTabInstallationSettingsHeader", TranslationNeeded);

            //Component: SettingsTabApplicationSettingsHeader
            //
            English.Add("SettingsTabApplicationSettingsHeader", "Application Settings");
            Gernam.Add("SettingsTabApplicationSettingsHeader", TranslationNeeded);
            Polish.Add("SettingsTabApplicationSettingsHeader", TranslationNeeded);
            French.Add("SettingsTabApplicationSettingsHeader", TranslationNeeded);

            //Component: SettingsTabAdvancedSettingsHeader
            //
            English.Add("SettingsTabAdvancedSettingsHeader", "Advanced");
            Gernam.Add("SettingsTabAdvancedSettingsHeader", TranslationNeeded);
            Polish.Add("SettingsTabAdvancedSettingsHeader", TranslationNeeded);
            French.Add("SettingsTabAdvancedSettingsHeader", TranslationNeeded);
            #endregion

            #region ModSelectionList
            //Component: continueButton
            //
            English.Add("continueButton", "Install");
            Gernam.Add("continueButton", "Installieren");
            Polish.Add("continueButton", "Zainstaluj");
            French.Add("continueButton", "Installer");

            //Component: cancelButton
            //
            English.Add("cancelButton", "Cancel");
            Gernam.Add("cancelButton", "Abbrechen");
            Polish.Add("cancelButton", "Anuluj");
            French.Add("cancelButton", "Anuler");

            //Component: helpLabel
            //
            English.Add("helpLabel", "Right-click a mod name to preview it");
            Gernam.Add("helpLabel", "Klick mit rechten Maustaste auf einen Mod-Namen, um eine Vorschau zu sehen");
            Polish.Add("helpLabel", "PPM by wyświetlić opis");
            French.Add("helpLabel", "Cliquez droit sur un nom de mod pour un apercu");

            //Component: loadConfigButton
            //
            English.Add("loadConfigButton", "Load selection");
            Gernam.Add("loadConfigButton", "Auswahl laden");
            Polish.Add("loadConfigButton", "Wczytaj konfigurację z pliku");
            French.Add("loadConfigButton", "Charger une configuration");

            //Component: saveConfigButton
            //
            English.Add("saveConfigButton", "Save selection");
            Gernam.Add("saveConfigButton", "Auswahl speichern");
            Polish.Add("saveConfigButton", "Zapisz konfigurację w pliku");
            French.Add("saveConfigButton", "Sauvegarder une configuration");

            //Component: clearSelectionsButton
            //
            English.Add("clearSelectionsButton", "Clear selections");
            Gernam.Add("clearSelectionsButton", "Auswahl löschen");
            Polish.Add("clearSelectionsButton", "Wyczyść wybór");
            French.Add("clearSelectionsButton", "Réinitialiser la sélection");

            //Component: readingDatabase
            //
            English.Add("readingDatabase", "Reading database");
            Gernam.Add("readingDatabase", "Lese datenbank");
            Polish.Add("readingDatabase", "Wczytywanie baz danych");
            French.Add("readingDatabase", "Chargement de la base de données");

            //Component: buildingUI
            //
            English.Add("buildingUI", "Building UI");
            Gernam.Add("buildingUI", "Erstelle UI");
            Polish.Add("buildingUI", "Budowanie interfejsu");
            French.Add("buildingUI", "Construction de l'interface");

            //Component: checkingCache
            //
            English.Add("checkingCache", "checking download cache of ");
            Gernam.Add("checkingCache", "Überprüfen des Download-Cache von");
            Polish.Add("checkingCache", "sprawdzanie ściągniętego cache dla");
            French.Add("checkingCache", "vérification du cache de téléchargement de");

            //Section: Preview
            //Component: descriptionBox
            //
            English.Add("noDescription", "no description provided");
            Gernam.Add("noDescription", "keine Beschreibung verfügbar");
            Polish.Add("noDescription", "nie podano opisu");
            French.Add("noDescription", "nPas de description fournie");

            //Component: updateBox
            //
            English.Add("noUpdateInfo", "No update info provided");
            Gernam.Add("noUpdateInfo", "keine Aktualisierungsinformationen verfügbar");
            Polish.Add("noUpdateInfo", "brak informacji o aktualizacji");
            French.Add("noUpdateInfo", "Aucune information de mise à jour fournie");


            //Component: NextPicButton
            //
            English.Add("NextPicButton", "next");
            Gernam.Add("NextPicButton", "weiter");
            Polish.Add("NextPicButton", "Dalej");
            French.Add("NextPicButton", "Suivant");

            //Component: PreviousPicButton
            //
            English.Add("PreviousPicButton", "previous");
            Gernam.Add("PreviousPicButton", "zurück");
            Polish.Add("PreviousPicButton", "Wstecz");
            French.Add("PreviousPicButton", "Précedent");

            //Component: DevLinkLabel
            //
            English.Add("DevLinkLabel", "Developer website");
            Gernam.Add("DevLinkLabel", "Entwickler webseite");
            Polish.Add("DevLinkLabel", "Strona Dewelopera");
            French.Add("DevLinkLabel", "Site web du développeur");

            //Component: InstallProgressTextBoxDescription
            //
            English.Add("InstallProgressTextBoxDescription", "Progress of an installation will be shown here");
            Gernam.Add("InstallProgressTextBoxDescription", TranslationNeeded);
            Polish.Add("InstallProgressTextBoxDescription", TranslationNeeded);
            French.Add("InstallProgressTextBoxDescription", TranslationNeeded);
            #endregion

            #region Update Window
            //Component: updateAcceptButton
            //
            English.Add("updateAcceptButton", "yes");
            Gernam.Add("updateAcceptButton", "ja");
            Polish.Add("updateAcceptButton", "Tak");
            French.Add("updateAcceptButton", "Oui");

            //Component: updateDeclineButton
            //
            English.Add("updateDeclineButton", "no");
            Gernam.Add("updateDeclineButton", "nein");
            Polish.Add("updateDeclineButton", "Nie");
            French.Add("updateDeclineButton", "Non");

            //Component: newVersionAvailableLabel
            //
            English.Add("newVersionAvailableLabel", "New version available");
            Gernam.Add("newVersionAvailableLabel", "Neue version verfügbar");
            Polish.Add("newVersionAvailableLabel", "Dostępna nowa wersja");
            French.Add("newVersionAvailableLabel", "Nouvelle version disponible");

            //Component: updateQuestionLabel
            //
            English.Add("updateQuestionLabel", "Update?");
            Gernam.Add("updateQuestionLabel", "Aktualisieren?");
            Polish.Add("updateQuestionLabel", "Zaktualizować?");
            French.Add("updateQuestionLabel", "Mettre à jour?");

            //Component: problemsUpdatingLabel
            //
            English.Add("problemsUpdatingLabel", "If you are having problems updating, please");
            Gernam.Add("problemsUpdatingLabel", "Wenn Sie Probleme mit der Aktualisierung haben, bitte");
            Polish.Add("problemsUpdatingLabel", "Jeśli masz problemy z aktualizają proszę");
            French.Add("problemsUpdatingLabel", "Si vous avez des problèmes avec la mise à jour, s'il vous plaît");

            //Component: 
            //
            English.Add("clickHereUpdateLabel", "click here");
            Gernam.Add("clickHereUpdateLabel", "klick hier");
            Polish.Add("clickHereUpdateLabel", "kliknij tutaj");
            French.Add("clickHereUpdateLabel", "Cliquez ici");
            #endregion

            #region Please Wait Window
            //Component: label1
            //
            English.Add("label1", "Loading...please wait...");
            Gernam.Add("label1", "Lädt...bitte warten...");
            Polish.Add("label1", "Ładowanie... proszę czekać...");
            French.Add("label1", "Chargement... Patientez, s'il vous plaît...");
            #endregion

            #region MainWindow Messages and Descriptions
            //Component: 
            //
            English.Add("Downloading", "Downloading");
            Gernam.Add("Downloading", "Wird heruntergeladen");
            Polish.Add("Downloading", "Pobieranie");
            French.Add("Downloading", "Téléchargement");

            //Component: 
            //
            English.Add("patching", "Patching");
            Gernam.Add("patching", "Patching");
            Polish.Add("patching", "Patchowanie");
            French.Add("patching", "Patching");

            //Component: 
            //
            English.Add("done", "Done");
            Gernam.Add("done", "Fertig");
            Polish.Add("done", "Zrobione");
            French.Add("done", "Terminé");

            //Component: 
            //
            English.Add("cleanUp", "Clean up resources");
            Gernam.Add("cleanUp", "Bereinige Ressourcen");
            Polish.Add("cleanUp", "Oczyszczanie zasobów");
            French.Add("cleanUp", "Nettoyer les ressources");

            //Component: 
            //
            English.Add("idle", "Idle");
            Gernam.Add("idle", "Leerlauf");
            Polish.Add("idle", "Bezczynny");
            French.Add("idle", "En attente");

            //Component: 
            //
            English.Add("status", "Status:");
            Gernam.Add("status", "Status:");
            Polish.Add("status", "Stan:");
            French.Add("status", "Statut");

            //Component: 
            //
            English.Add("canceled", "Canceled");
            Gernam.Add("canceled", "Abgebrochen");
            Polish.Add("canceled", "Anulowano");
            French.Add("canceled", "Anulé");

            //Component: 
            //
            English.Add("appSingleInstance", "Checking for single instance");
            Gernam.Add("appSingleInstance", "Prüfung auf Einzelinstanz");
            Polish.Add("appSingleInstance", "Sprawdzanie ");
            French.Add("appSingleInstance", "Vérification d'instance unique");

            //Component: 
            //
            English.Add("checkForUpdates", "Checking for updates");
            Gernam.Add("checkForUpdates", "Auf Updates prüfen");
            Polish.Add("checkForUpdates", "Sprawdzanie aktualizacji");
            French.Add("checkForUpdates", "Vérification de mise à jours");

            //Component: 
            //
            English.Add("verDirStructure", "Verifying directory structure");
            Gernam.Add("verDirStructure", "Verifizierung der Verzeichnisstruktur");
            Polish.Add("verDirStructure", "Sprawdzanie struktury dostępu");
            French.Add("verDirStructure", "Vérification de la structure du directoire");

            //Component: 
            //
            English.Add("loadingSettings", "Loading settings");
            Gernam.Add("loadingSettings", "Einstellungen laden");
            Polish.Add("loadingSettings", "Ładowanie ustawnień");
            French.Add("loadingSettings", "Chargement des paramètres");

            //Component: 
            //
            English.Add("loadingTranslations", "Loading translations");
            Gernam.Add("loadingTranslations", "Laden der Übersetzungen");
            Polish.Add("loadingTranslations", "ładowanie tłumaczenia");
            French.Add("loadingTranslations", "Chargement des traductions");

            //Component: 
            //
            English.Add("loading", "Loading");
            Gernam.Add("loading", "Laden");
            Polish.Add("loading", "Ładowanie");
            French.Add("loading", "Chargement de");

            //Component: 
            //
            English.Add("uninstalling", "Uninstalling");
            Gernam.Add("uninstalling", "Deinstallieren");
            Polish.Add("uninstalling", "Deinstalacja w toku");
            French.Add("uninstalling", "Désinstallation");

            //Component: 
            //
            English.Add("installingFonts", "Installing fonts");
            Gernam.Add("installingFonts", "Installieren von Schriftarten");
            Polish.Add("installingFonts", "Instalowanie czcionek");
            French.Add("installingFonts", "Installation des polices");

            //Component: 
            //
            English.Add("loadingExtractionText", "Loading extraction text");
            Gernam.Add("loadingExtractionText", "Extraktionstext laden");
            Polish.Add("loadingExtractionText", "Ładowanie tekstu");
            French.Add("loadingExtractionText", "Chargement du texte d'extraction");

            //Component: 
            //
            English.Add("extractingRelhaxMods", "Extracting Relhax mods");
            Gernam.Add("extractingRelhaxMods", "Extrahieren von Relhax mods");
            Polish.Add("extractingRelhaxMods", "Wypakowywanie modyfikacji mods");
            French.Add("extractingRelhaxMods", "Extraction des mods Relhax");

            //Component: 
            //
            English.Add("extractingUserMods", "Extracting user mods");
            Gernam.Add("extractingUserMods", "Extrahieren von benutzerdefinierten mods");
            Polish.Add("extractingUserMods", "Wypakowywanie modyfikacji użytkownika");
            French.Add("extractingUserMods", "Extraction des mods d'utilisateur");

            //Component: 
            //
            English.Add("copyingFile", "Copying file");
            Gernam.Add("copyingFile", "Kopieren von Dateien");
            Polish.Add("copyingFile", "Kopiowanie plików");
            French.Add("copyingFile", "Copie des fichiers");

            //Component: 
            //
            English.Add("deletingFile", "Deleting file");
            Gernam.Add("deletingFile", "Lösche Datei");
            Polish.Add("deletingFile", "Usuwanie plików");
            French.Add("deletingFile", "Supression du fichier");

            //Component: 
            //
            English.Add("of", "of");
            Gernam.Add("of", "von");
            Polish.Add("of", "z");
            French.Add("of", "de");

            //Component: 
            //
            English.Add("failed_To_Download_1", "Failed to download:");
            Gernam.Add("failed_To_Download_1", "Fehler beim Herunterladen:");
            Polish.Add("failed_To_Download_1", "Ściąganie zakończone niepowodzeniem, plik:");
            French.Add("failed_To_Download_1", "Échec de téléchargement:");

            //Component: 
            //
            English.Add("failed_To_Download_2", "If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it exits");
            Gernam.Add("failed_To_Download_2", "Wenn du weisst, welcher Mod das ist, deaktiviere ihn und alles sollte funktionieren. Es wird bald behoben. Starte das Programm neu nach dem beenden.");
            Polish.Add("failed_To_Download_2", "Jeśli wiesz który to mod, odznacz go i wszystko powinno byćw porządku. Wkrótce naprawimy błąd. Zrestartuj, jeśli problem pojawia się ponownie.");
            French.Add("failed_To_Download_2", "Si vous savez quel mod est la cause, déséléectionnez celui-ci. Un corrigé vas être disponible bientôt. Redémarrez ceci a la fermeture");

            //Component: update check against online app version
            //
            English.Add("failedManager_version", "Current Beta application is outdated and must be updated against stable channel. No new Beta version online now.");
            Gernam.Add("failedManager_version", TranslationNeeded);
            Polish.Add("failedManager_version", TranslationNeeded);
            French.Add("failedManager_version", TranslationNeeded);

            //Component: initial download managerInfo.dat
            //
            English.Add("currentBetaAppOutdated", "Failed to get 'manager_version.xml'\n\nApplication will be terminated.");
            Gernam.Add("currentBetaAppOutdated", "Fehler beim lesen der 'manager_version.xml' Datei.\n\nProgramm wird abgebrochen.");
            Polish.Add("currentBetaAppOutdated", "Nie udało się uzyskać 'manager_version.xml'\n\nApplication zostanie zakończona.");
            French.Add("currentBetaAppOutdated", "Impossible d'obtenir 'manager_version.xml'\n\nL'application sera terminée.");

            //Component: 
            //
            English.Add("fontsPromptInstallHeader", "Admin to install fonts?");
            Gernam.Add("fontsPromptInstallHeader", "Admin zum Installieren von Schriftarten?");
            Polish.Add("fontsPromptInstallHeader", "Uprawnienia administratora, by zainstalować czcionki?");
            French.Add("fontsPromptInstallHeader", "Admin pour installer les polices?");

            //Component: 
            //
            English.Add("fontsPromptInstallText", "Do you have admin rights to install fonts?");
            Gernam.Add("fontsPromptInstallText", "Hast Du Admin-Rechte um Schriftarten zu installieren?");
            Polish.Add("fontsPromptInstallText", "Czy masz uprawnienia administratora zainstalować czcionki?");
            French.Add("fontsPromptInstallText", "Avez-vous les droits d'administrateur installer des polices?");

            //Component: 
            //
            English.Add("fontsPromptError_1", "Unable to install fonts. Some mods may not work properly. Fonts are located in ");
            Gernam.Add("fontsPromptError_1", "Schriftarten können nicht installiert werden. Einige Mods funktionieren möglicherweise nicht richtig. Schriften befinden sich in ");
            Polish.Add("fontsPromptError_1", "Niepowodzenie przy instalacji czcionek. Niektóre modyfikacje mogą nie działać prawidłowo. Czcionki znajdują się w ");
            French.Add("fontsPromptError_1", "Impossible d'installer les polices. Certain mods peut mal fonctionner. Les polices sont situé dans ");

            //Component: 
            //
            English.Add("fontsPromptError_2", "\\_fonts. Either install them yourself or run this again as Administrator.");
            Gernam.Add("fontsPromptError_2", "\\_fonts. Entweder installiere sie selbst oder führe der Relhax Manager erneut als Administrator aus.");
            Polish.Add("fontsPromptError_2", "\\_fonts. Albo zainstalujesz je własnoręcznie, albo uruchom jako administrator.");
            French.Add("fontsPromptError_2", "\\_fonts. Installez les polices manuellement ou redémarrez avec les droits Administrateur");

            //Component: 
            //
            English.Add("cantDownloadNewVersion", "Unable to download new version, exiting.");
            Gernam.Add("cantDownloadNewVersion", "Die neue Version kann nicht heruntergeladen werden.");
            Polish.Add("cantDownloadNewVersion", "Niepowodzenie przy pobieraniu nowej wersji.");
            French.Add("cantDownloadNewVersion", "Échec du téléchargement des mise à jours. Fermeture.");

            //Component: 
            //
            English.Add("failedCreateUpdateBat", "Unable to create update process.\n\nPlease manually delete the file:\n{0}\n\nrename file:\n{1}\nto:\n{2}\n\nDirectly jump to the folder?");
            Gernam.Add("failedCreateUpdateBat", "Updateprozess kann leider nicht erstellt werden.\n\nLösche bitte diese Datei von Hand:\n{0}\n\nbenennte diese" +
                " Datei:\n{1}\nin diese um:\n{2}\n\nDirekt zum Ordner springen?");
            Polish.Add("failedCreateUpdateBat", "Nie można zaktualizować.\n\nProszę ręcznie usunąć plik:\n{0}\n\nrzmienić nazwę pliku:\n{1}\nna:\n{2}\n\nCzy chcesz otworzyć lokalizację pliku?");
            French.Add("failedCreateUpdateBat", "fichier:\n{0}\n\nrenamefile:\n{1}\nto:\n{2}\n\nAfficher le dossier?");

            //Component: 
            //
            English.Add("cantStartNewApp", "Unable to start application, but it is located in \n");
            Gernam.Add("cantStartNewApp", "Kann die Anwendung nicht starten, aber sie befindet sich in \n");
            Polish.Add("cantStartNewApp", "Niepowodzenie przy uruchamianiu aplikacji znajdującej się w \n");
            French.Add("cantStartNewApp", "Échec du lancement de l'application, mais elle est situé dans \n");

            //Component: 
            //
            English.Add("autoDetectFailed", "The auto-detection failed. Please use the 'force manual' option");
            Gernam.Add("autoDetectFailed", "Die automatische Erkennung ist fehlgeschlagen. Bitte benutzen Sie die 'erzwinge manuelle' Option");
            Polish.Add("autoDetectFailed", "Niepowodzenie automatycznego wykrywania. Proszę wybrać opcję ręcznego znajdowania ścieżki gry.");
            French.Add("autoDetectFailed", "Échec de la détection automatique. Utilisez l'option 'Forcer détection manuel'");

            //Component: MainWindow_Load
            //
            English.Add("anotherInstanceRunning", "Another Instance of the Relhax Manager is already running");
            Gernam.Add("anotherInstanceRunning", "Eine weitere Instanz des Relhax Manager  läuft bereits");
            Polish.Add("anotherInstanceRunning", "Inna instancja Relhax Manager jest uruchomiona");
            French.Add("anotherInstanceRunning", "Une autre instance de Relhax Directeur est en cours d`éxecution");

            English.Add("closeInstanceRunningForUpdate", "Please close ALL running instances of the Relhax Manager before we can go on and update.");
            Gernam.Add("closeInstanceRunningForUpdate", "Bitte schließe ALLE laufenden Instanzen des Relhax Managers bevor wir fortfahren und aktualisieren können.");
            Polish.Add("closeInstanceRunningForUpdate", "Proszę zamknąć WSZYSTKIE działające instancje Relhax Modpack przed dalszym procesem aktualizacji.");
            French.Add("closeInstanceRunningForUpdate", "Merci de fermé toutes les instances du modpack relhax avant que nous puissions procéder à la mise à jour");

            //Component: 
            //
            English.Add("skipUpdateWarning", "WARNING: You are skipping updating. Database Compatability is not guarenteed");
            Gernam.Add("skipUpdateWarning", "WARNUNG: Sie überspringen die Aktualisierung. Datenbankkompatibilität ist nicht garantiert");
            Polish.Add("skipUpdateWarning", "OSTRZEŻENIE: Pomijasz aktualizację! Może wystąpić niezgodność wersji.");
            French.Add("skipUpdateWarning", "ATTENTION: Vous ignorez la mise à jour. Compatibilité de la base de données non garanti ");

            //Component: 
            //
            English.Add("patchDayMessage", "The modpack is curretly down for patch day testing and mods updating. Sorry for the inconvience. If you are a database manager, please add the command arguement");
            Gernam.Add("patchDayMessage", "Das Modpack ist zur Zeit für Patch-Tag-Tests und das Updaten von Mods nicht erreichbar. Entschuldige die Unannehmlichkeiten." +
                " Wenn Sie ein Datenbankmanager sind, fügen Sie bitte das Befehlsargument hinzu");
            Polish.Add("patchDayMessage", "Paczka nie działa ze względu na testy i aktualizację modyfikacji. Przepraszamy za utrudnienia. Jeśli zarządzasz bazą danych, proszę dodać odpowiednią komendę");
            French.Add("patchDayMessage", "Le pack mod est actuellement indisponible aujourd'hui pour tester et mettre à jour les mods. Désolé pour le dérangement." +
                " Si vous êtes un gestionnaire de base de données, ajoutez l'argument de commande.");

            //Component: 
            //
            English.Add("configNotExist", "ERROR: {0} does NOT exist, loading in regular mode");
            Gernam.Add("configNotExist", "FEHLER: {0} existiert nicht, lädt im regulären Modus");
            Polish.Add("configNotExist", "BŁĄD: {0} nie istnieje, ładowanie podstawowego trybu");
            French.Add("configNotExist", "ERREUR: {0} n'existe pas, chargement en mode normal");

            //Component: 
            //
            English.Add("autoAndClean", "ERROR: clean installation is set to false. You must set this to true and restart the application for auto install to work. Loading in regular mode.");
            Gernam.Add("autoAndClean", "FEHLER: Die saubere Installation ist abgewählt/deaktiviert. Du musst diese Option auswählen und die Anwendung für die automatische" +
                " Installation neu starten, damit sie funktioniert. Lädt im regulären Modus.");
            Polish.Add("autoAndClean", "BŁĄD: wyłączono czystą instalację. Musisz ją włączyć i ponownie uruchomić aplikację, by automatyczna instalacja zadziałała. Ładowanie w trybie podstawowym.");
            French.Add("autoAndClean", "ERREUR: Installation propre est désactivé. Vous devez sélectionner ceci et redémarrer l'application pour l'installation automatique. Chargement en mode normal");

            //Component: 
            //
            English.Add("autoAndFirst", "ERROR: First time loading cannot be an auto install mode, loading in regular mode");
            Gernam.Add("autoAndFirst", "FEHLER: Erstmaliges Laden kann kein automatischer Installationsmodus sein, lade im regulären Modus");
            Polish.Add("autoAndFirst", "BŁĄD: Pierwsze ładowanie nie może być automatyczną instalacją, ładowanie w trybie podstawowym");
            French.Add("autoAndFirst", "ERREUR: Le premier lancement ne peut pas être un mode d'installation automatique, chargement en mode normal");

            //Component: 
            //
            English.Add("confirmUninstallHeader", "Confirmation");
            Gernam.Add("confirmUninstallHeader", "Bestätigung");
            Polish.Add("confirmUninstallHeader", "Potwierdź");
            French.Add("confirmUninstallHeader", "Confirmation");

            //Component: 
            //
            English.Add("confirmUninstallMessage", "Confirm you wish to uninstall mods from WoT installation\n\n{0}\n\nUsing uninstall method '{1}'?");
            Gernam.Add("confirmUninstallMessage", "Bitte bestätige das du alle Mods von Deinem WoT deinstalliert haben möchtest\n\n{0}\n\nVerwendung von Deinstallationsmethode '{1}'?");
            Polish.Add("confirmUninstallMessage", "Potwierdź usunięcie modyfikacji\n\n{0}\n\nPotwierdź metodę '{1}'");
            French.Add("confirmUninstallMessage", "Confirmer que vous voulez désinstaller les mods du dossier WoT\n\n{0}\n\nUsing la méthode de désinstallation '{1}'?");

            //Component: 
            //
            English.Add("uninstallingText", "Uninstalling...");
            Gernam.Add("uninstallingText", "Deinstalliere...");
            Polish.Add("uninstallingText", "Deinstalacja w toku...");
            French.Add("uninstallingText", "Désinstallation...");

            //Component:
            //progress message
            English.Add("uninstallingFile", "Uninstalling file");
            Gernam.Add("uninstallingFile", "Deinstalliere datei");
            Polish.Add("uninstallingFile", "Odinstalowanie pliku");
            French.Add("uninstallingFile", "Désinstallation du fichier");

            //Component: uninstallfinished messagebox
            //
            English.Add("uninstallFinished", "Uninstallation of mods finished.");
            Gernam.Add("uninstallFinished", "Deinstallation der mods beendet.");
            Polish.Add("uninstallFinished", "Deinstalacja (modyfikacji) zakończona");
            French.Add("uninstallFinished", "Désinstallation des mods terminé");

            //Component: 
            //
            English.Add("specialMessage1", "If you are seeing this, it means that you have a specific computer configuration that is affected by a bug I can't replicate on my developer system." +
                " It's harmless, but if you could send your relHaxLog to me I can fix it and you can stop seeing this message");
            Gernam.Add("specialMessage1", "Wenn Du dies siehst, bedeutet dies, dass Du eine bestimmte Computer-Konfiguration hast, die von einem Fehler betroffen ist, den ich nicht auf meinem" +
                " Entwicklersystem replizieren kann. Es ist harmlos, aber wenn du dein relHaxLog an mich schicken könntest, kann ich es beheben und du wirst diese Nachricht zukuenftig nicht mehr sehen");
            Polish.Add("specialMessage1", "Jeśli to widzisz, to znaczy, że masz specificzną konfigurację komputera afektowany przez bug, który nie mogę kopiować na moim systemie. Jest nieszkodliwy," +
                " ale jeśli możesz mi przesłać relHaxLog to postaram się naprawić błąd, abyś nie widział tej wiadomości w przyszłości");
            French.Add("specialMessage1", "Si vous voyez ceci, cela signifie que vous avez une configuration d'ordinateur spécifique qui est affectée par un bug que je ne peux pas répliquer sur mon" +
                " système de développeur. Ce n'est pas grave. Si vous pouvez envoyer votre relHaxLog, je peux le réparer et vous pouvez arrêter de voir ce message.");

            //Component: 
            //
            English.Add("extractionErrorMessage", "Error deleting the folder res_mods or mods. Either World of Tanks is running or your file and folder security permissions are incorrect");
            Gernam.Add("extractionErrorMessage", "Fehler beim Löschen des Ordners res_mods oder Mods. Entweder World of Tanks läuft oder Ihre Datei- und Ordnersicherheitsberechtigungen sind falsch");
            Polish.Add("extractionErrorMessage", "Błąd usuwania folderu res_mods lub mods. Albo World of Tanks jest uruchomione, albo twój plik i folder mają nieprawidłowe zabezpieczenia dostępu");
            French.Add("extractionErrorMessage", "Erreur lors de la supression du dossier res_mods ou un/plusieur mods. Sois que World of Tanks est en cours d`éxecution ou les permissions de sécuriter sont incorrect.");

            //Component: 
            //
            English.Add("extractionErrorHeader", "Error");
            Gernam.Add("extractionErrorHeader", "Fehler");
            Polish.Add("extractionErrorHeader", "Błąd");
            French.Add("extractionErrorHeader", "Erreur");

            //Component: 
            //
            English.Add("deleteErrorHeader", "close out of folders");
            Gernam.Add("deleteErrorHeader", "Ausschliessen von Ordnern");
            Polish.Add("deleteErrorHeader", "zamknij foldery");
            French.Add("deleteErrorHeader", "Fermez les dossiers");

            //Component: 
            //
            English.Add("deleteErrorMessage", "Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");
            Gernam.Add("deleteErrorMessage", "Bitte schließen Sie alle Explorer-Fenster in Mods oder res_mods (oder Unterordner), und klicken Sie auf OK, um fortzufahren.");
            Polish.Add("deleteErrorMessage", "Proszę zamknij folder mods lub res_mods (lub podfoldery), a następnie kliknij kontynuację.");
            French.Add("deleteErrorMessage", "Veuillez fermer les fenêtre res_mods ou mods (Ou tout sous-dossiers) et cliquez Ok pour continuer");

            //Component: 
            //
            English.Add("noUninstallLogMessage", "The log file containg the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");
            Gernam.Add("noUninstallLogMessage", "Die Protokolldatei mit der Liste der installierten Dateien (installedRelhaxFiles.log) existiert nicht. Möchtest Du alle Mods stattdessen entfernen?");
            Polish.Add("noUninstallLogMessage", "Plik logów zawierający listę instalacyjną (installedRelhaxFiles.log) nie istnieje. Czy chciałbyś usunąć modyfikacje?");
            French.Add("noUninstallLogMessage", "Le ficher log contenant la liste des fichiers installé (installedRelhaxFiles.log) n'existe pas. Voulez vous supprimez tout les mods?");

            //Component: 
            //
            English.Add("noUninstallLogHeader", "Remove all mods");
            Gernam.Add("noUninstallLogHeader", "Entferne alle Mods");
            Polish.Add("noUninstallLogHeader", "Usuń wszystkie modyfikacje");
            French.Add("noUninstallLogHeader", "Supprimer tout les mods");

            //Component: appDataFolderError
            //If the appDataFolder is not parsed properly
            English.Add("appDataFolderError", "The app data folder for WoT was not found. Clearing WoT cache will be skipped. Please report this to the developer team.");
            Gernam.Add("appDataFolderError", "Der App-Datenordner für WoT wurde nicht gefunden. Das Löschen von WoT-Cache wird übersprungen. Bitte melden Sie dies dem Entwicklerteam.");
            Polish.Add("appDataFolderError", "Nie znaleziono foderu app data dla WoT. Czyszczenie folderu cache zostanie pominięte. Prosimy zgłosić problem naszym deweloperom.");
            French.Add("appDataFolderError", "Le dossier App Data pour WoT n'as pas pus être trouvé. Nettoyage du cache vas être ignoré. Veuillez avertir l`équipe de développement");

            //Component: moveOutOfTanksLocation
            //
            English.Add("moveOutOfTanksLocation", "The modpack can not be run from the World_of_Tanks directory. Please move the application and try again.");
            Gernam.Add("moveOutOfTanksLocation", "Das Modpack kann nicht aus dem World_of_Tanks Verzeichnis laufen. Bitte verschiebe die Anwendung in ein anderes Verzeichnis und versuchen Sie es erneut.");
            Polish.Add("moveOutOfTanksLocation", "Modpack nie może być uruchomiony z katalogu World_of_Tanks. Przenieś aplikację i spróbuj ponownie.");
            French.Add("moveOutOfTanksLocation", "Le Mod pack ne peut pas être éxecuté a partir du dossier de World of Tanks. Veuillez déplacer l`application dans un autre dossier et réessayer");

            //Component: Current database is same as last installed database (body)
            //
            English.Add("DatabaseVersionsSameBody", "The database has not changed since your last installation. Therefore there are no updates to your current mods selection." +
                " Continue anyway?");
            Gernam.Add("DatabaseVersionsSameBody", "Die Datenbank  wurde seit Deiner letzten Installation nicht verändert. Daher gibt es keine Aktuallisierungen zu Deinen aktuellen" +
                " Modifikationen. Trotzdem fortfahren?");
            Polish.Add("DatabaseVersionsSameBody", "Baza danych nie została zmieniona od ostatniej instalacji, nie ma żadych aktualizacji dla wybranych uprzednio modyfikacji." +
                " Czy kontynuować?");
            French.Add("DatabaseVersionsSameBody", "La base de données n'a pas changé depuis votre dernière installation. Par conséquent, il n'y a pas de mise à jour pour votre sélection" +
                "  de mods. Continuer de toute façon?");

            //Component: Current database is same as last installed database (header)
            //
            English.Add("DatabaseVersionsSameHeader", "Database version is the same");
            Gernam.Add("DatabaseVersionsSameHeader", "Datenbank Version ist identisch");
            Polish.Add("DatabaseVersionsSameHeader", "Wersja bazy danych jest taka sama");
            French.Add("DatabaseVersionsSameHeader", "La version de la base de données est la même");

            //Component: 
            //
            English.Add("databaseNotFound", "Database not found at supplied URL");
            Gernam.Add("databaseNotFound", "Datenbank nicht an der angegebenen URL gefunden");
            Polish.Add("databaseNotFound", "Nie znaleziono bazy danych pod wskazanym URL");
            French.Add("databaseNotFound", "Base de données introuvable à L'URL fournie  ");

            //Component: Detected client version
            //
            English.Add("detectedClientVersion", "Detected Client version");
            Gernam.Add("detectedClientVersion", "Erkannte Client Version");
            Polish.Add("detectedClientVersion", "Wykryta wersja klienta gry");
            French.Add("detectedClientVersion", "Version du client détecté");

            //Component: Supported client versions
            //
            English.Add("supportedClientVersions", "Supported Clients");
            Gernam.Add("supportedClientVersions", "Unterstützte Clients");
            Polish.Add("supportedClientVersions", "Wspomagane wersje klienta gry");
            French.Add("supportedClientVersions", "Clients supportés");

            //Component: Supported clients notice
            //
            English.Add("supportNotGuarnteed", "This client version is not offically supported. Mods may not work.");
            Gernam.Add("supportNotGuarnteed", "Diese Client Version wird (noch) nicht offiziell unterstützt. Die Mods könnten nicht funktionieren oder sogar Dein World of Tanks beschädigen.");
            Polish.Add("supportNotGuarnteed", "Ta wersja klienta gry nie jest oficjalnie wspomagana. Modyfikacje mogą nie działać prawidłowo.");
            French.Add("supportNotGuarnteed", "Ce client n'est pas supporté officiellement. Les mods risque de ne pas fonctionner.");

            //Component: notifying the user the change won't take effect until application restart
            English.Add("noChangeUntilRestart", "This option won't take effect until application restart");
            Gernam.Add("noChangeUntilRestart", "Diese Option hat keine Auswirkungen bis das Programm neu gestartet wurde");
            Polish.Add("noChangeUntilRestart", "Aby zastosować tą opcję należy zrestartować aplikację");
            French.Add("noChangeUntilRestart", "Cette option ne prendra effet qu'au redémarrage de l'application");
            #endregion

            #region Messages from ModSelectionList
            //Component: testModeDatabaseNotFound
            //
            English.Add("testModeDatabaseNotFound", "CRITICAL: TestMode Database not found at:\n{0}");
            Gernam.Add("testModeDatabaseNotFound", "KRITISCH: Die Datanbank für den Testmodus wurde nicht gefunden:\n{0}");
            Polish.Add("testModeDatabaseNotFound", "BŁĄD KRYTYCZNY: Baza danych Trybu Testowego nie znaleziona w lokalizacji:\n{0}");
            French.Add("testModeDatabaseNotFound", "CRITIQUE: Impossible de trouver la base de données du mode de test situé a: \n{0}");

            //Component: 
            //
            English.Add("duplicateMods", "CRITICAL: Duplicate mod name detected");
            Gernam.Add("duplicateMods", "KRITISCH: Duplizierter Modname wurde erkannt");
            Polish.Add("duplicateMods", "BŁĄD KRYTYCZNY: Wykryto zduplikowaną nazwę modyfikacji");
            French.Add("duplicateMods", "CRITIQUE: Détection de mods en double");

            //Component: 
            //
            English.Add("databaseReadFailed", "CRITICAL: Failed to read database\n\nsee Logfile for detailed info");
            Gernam.Add("databaseReadFailed", "KRITISCH: Datenbank konnte nicht gelesen werden\n\nin der Protokolldatei stehen weitere Informationen zu diesem Fehler");
            Polish.Add("databaseReadFailed", "BŁĄD KRYTYCZNY: Nie udało się wczytać bazy danych");
            French.Add("databaseReadFailed", "CRITIQUE: Impossible de lire la base de données");

            //Component: 
            //
            English.Add("configSaveSuccess", "Config Saved Successfully");
            Gernam.Add("configSaveSuccess", "Konfiguration wurde erfolgreich gespeichert");
            Polish.Add("configSaveSuccess", "Udało się zapisać konfigurację");
            French.Add("configSaveSuccess", "Succès sauvegarde de la configuration");

            //Component: 
            //
            English.Add("selectConfigFile", "Select a user preference file to load");
            Gernam.Add("selectConfigFile", "Wählen Sie die benutzerdefinierte Datei aus, die geladen werden soll");
            Polish.Add("selectConfigFile", "Wybierz plik preferencji do wczytania");
            French.Add("selectConfigFile", "Sélectionnez un fichier de préférences utilisateur à charger");

            //Component: 
            //
            English.Add("configLoadFailed", "The preference file could not be loaded, loading in standard mode");
            Gernam.Add("configLoadFailed", "Die Konfigurationsdatei konnte nicht geladen werden, lade im Standard Modus");
            Polish.Add("configLoadFailed", "Nie można wczytać pliku knfiguracji, ładowanie trybu podstawowego");
            French.Add("configLoadFailed", "Impossible de charge le fichier de préférences. Chargement en mode normal");

            //Component: 
            //
            English.Add("modNotFound", "The mod, \"{0}\" was not found in the modpack. It could have been renamed or removed.");
            Gernam.Add("modNotFound", "Der Mod, \"{0}\" wurde im Modpack nicht gefunden. Er könnte umbenannt oder entfernt worden sein.");
            Polish.Add("modNotFound", "Modyfikacja \"{0}\" nie została znaleziona w paczce. Sprawdź, czy nie została usunięta lub zmieniona nazwa.");
            French.Add("modNotFound", "Impossible de trouver le mod, \"{0}\" dans le mod pack. Il est possible qu`il sois supprimé ou changé de nom");

            //Component: 
            //
            English.Add("configNotFound", "The config \"{0}\" was not found for mod \"{1}\". It could have been renamed or removed.");
            Gernam.Add("configNotFound", "Die Config \"{0}\" wurde nicht für den Mod gefunden \"{1}\". Er könnte umbenannt oder entfernt worden sein.");
            Polish.Add("configNotFound", "Konfiguracja \"{0}\" nie została znaleziona dla modyfikacji \"{1}\". Sprawdź, czy nie została usunięta lub zmieniona nazwa.");
            French.Add("configNotFound", "Impossible de trouver la configuration \"{0}\" pour le mod \"{1}\". Il est possible qu`il sois supprimé ou changé de nom");

            //Component: 
            //
            English.Add("modDeactivated", "The mod \"{0}\" is currently deactivated in the modpack and could not to be selected to install.");
            Gernam.Add("modDeactivated", "Der Mod \"{0}\" ist derzeit im Modpack deaktiviert und steht für die Installation nicht zur Verfügung.");
            Polish.Add("modDeactivated", "Modyfikacja \"{0}\" jest obecnie zdezaktywowana w paczce i nie może zostać wybrana.");
            French.Add("modDeactivated", "Le mod est actuellement désactivé dans le mod pack et ne seras pas installé");

            //Component: 
            //
            English.Add("configDeactivated", "The config \"{0}\" of \"{1}\" is currently deactivated in the modpack and could not to be selected to install.");
            Gernam.Add("configDeactivated", "Die Konfiguration \"{0}\" von \"{1}\" ist derzeit im Modpack deaktiviert und steht für die Installation nicht zur Verfügung.");
            Polish.Add("configDeactivated", "Konfiguracja \"{0}\" z \"{1}\" jest obecnie zdezaktywowana w paczce i nie może zostać wybrana.");
            French.Add("configDeactivated", "La configuration \"{0}\" du mod \"{1}\" est actuellement désactivé dans le mod pack et ne seras pas installé");

            //Component: 
            //
            English.Add("modsNotFoundTechnical", "The following mods could not be found and were most likely removed. There are only technical names available:\n{0}");
            Gernam.Add("modsNotFoundTechnical", "Die folgenden Modifikationen können nicht gefunden werden und wurden wahrscheinlich entfernt/gelöscht. Es sind leider nur technische Namen verfügbar:\n{0}");
            Polish.Add("modsNotFoundTechnical", "Następujące modyfikacje nie zostały znalezione, najprawdopodobniej zostały usunięte:\n{0}");
            French.Add("modsNotFoundTechnical", "Les mods suivants n'ont pas pu être trouvés et ont probablement été supprimés. Il n'y a que des noms techniques disponibles:\n{0}");

            //Component: 
            //
            English.Add("modsBrokenStructure", "The following mods were disabled due to modifications in the package structure. You need to re-check them if you want to install them:\n");
            Gernam.Add("modsBrokenStructure", "Die folgenden Mods wurden aufgrund von Änderungen in der Paketstruktur deaktiviert. Sie müssen sie erneut auswählen, wenn Sie sie installieren möchten:\n");
            Polish.Add("modsBrokenStructure", "Następujące modyfikacje zostały wyłączone ze względu na zmiany w strukturze paczki. Zaznacz je ponownie, jeśli chcesz je zainstalować:\n");
            French.Add("modsBrokenStructure", "Les mods suivants ont été désactivés en raison de modifications dans la structure du paquet: vous devez les vérifier de nouveau si vous voulez les installer:\n");

            //Component: 
            //
            English.Add("oldSavedConfigFile", "The saved preferences file your are using is in an outdated format and will be inaccurate in the future. Convert it to the new format?" +
                " (A backup of the old format will be made)");
            Gernam.Add("oldSavedConfigFile", "Die Konfigurationsdatei die benutzt wurde, wird in Zukunft immer ungenauer werden. Soll auf das neue Standardformat umgestellt werden?" +
                " (Eine Sicherung des alten Formats erfolgt)");
            Polish.Add("oldSavedConfigFile", "Zapisana konfiguracja jest w przestarzałym formacie i może powodować nieścisłości. Czy chcesz przekonwertować ją na nowszy zapis?");
            French.Add("oldSavedConfigFile", "Le fichier de préférences que vous avez choisis est un format obsolète et seras inexact dans le future. Convertire au nouveau format?");

            //Component: 
            //
            English.Add("prefrencesSet", "Preferences Set");
            Gernam.Add("prefrencesSet", "Bevorzugte Einstellungen");
            Polish.Add("prefrencesSet", "Preferowane Ustawienia");
            French.Add("prefrencesSet", "Préférences Enregistrées");

            //Component: 
            //
            English.Add("selectionsCleared", "Selections Cleared");
            Gernam.Add("selectionsCleared", "Auswahl gelöscht");
            Polish.Add("selectionsCleared", "Usunięto Zaznaczenia");
            French.Add("selectionsCleared", "Sélections effacées");

            //Component: Expand current tab option
            //
            English.Add("expandAllButton", "Expand Current Tab");
            Gernam.Add("expandAllButton", "Erweitere alle Einträge der aktuellen Registerkarte");
            Polish.Add("expandAllButton", "Rozwiń bieżącą kartę");
            French.Add("expandAllButton", "Elargir l'onglet");

            //Component: Colapse current tab option
            //
            English.Add("colapseAllButton", "Collapse Current Tab");
            Gernam.Add("colapseAllButton", "Reduziere alle Einträge der aktuellen Registerkarte");
            Polish.Add("colapseAllButton", "Zwiń bieżącą kartę");
            French.Add("colapseAllButton", "Réduire l'onglet");

            //Component:
            //
            English.Add("InstallingTo", "Installing to {0}");
            Gernam.Add("InstallingTo", "Installiere nach {0}");
            Polish.Add("InstallingTo", "Instalowanie w {0}");
            French.Add("InstallingTo", "Installation à {0}");

            //Section saveConfig
            //
            English.Add("selectWhereToSave", "Select where to save user prefs");
            Gernam.Add("selectWhereToSave", "Bitte wähle wo die Konfiguation gespeichert werden soll");
            Polish.Add("selectWhereToSave", "Wybór lokalizacji zapisu preferencji użytkownika");
            French.Add("selectWhereToSave", "Sélectionnez la location pour enregistrer les préférences utilisateur");

            //Section addModTreeview
            //
            English.Add("updated", "updated");
            Gernam.Add("updated", "aktualisiert");
            Polish.Add("updated", "zaktualizowane");
            French.Add("updated", "Mis à jours");
            #endregion

            #region OldFilesToDelete
            //Window header
            English.Add("foundOldFilesHeader", "Old Files Question");
            Gernam.Add("foundOldFilesHeader", "alte Dateien löschen");
            Polish.Add("foundOldFilesHeader", "Zapytanie o starsze wersje plików");
            French.Add("foundOldFilesHeader", "Question pour les fichiers anciens (français seulement)");

            //Component: Found zip files to delete 1
            //
            English.Add("foundOldFilesDelete1", "The installer has found the following files that are old and can be deleted");
            Gernam.Add("foundOldFilesDelete1", "Wir haben folgende veralteten Dateien gefunden die gelöscht werden können");
            Polish.Add("foundOldFilesDelete1", "Instalator znalazł następujące stare pliki, które mogą zostać usunięte");
            French.Add("foundOldFilesDelete1", "L'installateur a trouvé les vieux fichiers suivants qui peuvent être supprimé");

            //Component: Found zip files to delete 2
            //
            English.Add("foundOldFilesDelete2", "Would you like to delete them?");
            Gernam.Add("foundOldFilesDelete2", "Möchtest du das sie gelöscht werden?");
            Polish.Add("foundOldFilesDelete2", "Czy chcesz je usunąć?");
            French.Add("foundOldFilesDelete2", "Voulez vous les supprimés");
            #endregion

            #region SelectionViewer
            //Component: SelectConfigLabel
            //The label in the window informing the user to select a config option to use
            English.Add("SelectConfigLabel", "Select a config to load");
            Gernam.Add("SelectConfigLabel", "Wähle eine Konfiguation zum Laden aus");
            Polish.Add("SelectConfigLabel", "Wybierz konfigurację do załadowania");
            French.Add("SelectConfigLabel", "Sélectionnez une configuration à charger");

            //Component: localFile
            //The text in the first radioButton in the selection viewer, for the user to select their own personal config file to load
            English.Add("localFile", "Local File");
            Gernam.Add("localFile", "Lokale Datei");
            Polish.Add("localFile", "Plik lokalny");
            French.Add("localFile", "Fichier local");

            //Component: radioButtonToolTip
            //
            English.Add("createdAt", "Created at: {0}");
            Gernam.Add("createdAt", "Erstellt am: {0}");
            Polish.Add("createdAt", "Utworzono w: {0}");
            French.Add("createdAt", "Créé à: {0}");
            #endregion

            #region Installer Messages
            //Component: installClearCache
            //
            English.Add("installClearCache", "Deleting WoT cache");
            Gernam.Add("installClearCache", "WoT Zwischenspeicher löschen");
            Polish.Add("installClearCache", "Usuwanie cache WoTa");
            French.Add("installClearCache", "Suppression du cache WoT");

            //Component: installClearLogs
            //
            English.Add("installClearLogs", "Deleting WoT logs");
            Gernam.Add("installClearLogs", TranslationNeeded);
            Polish.Add("installClearLogs", TranslationNeeded);
            French.Add("installClearLogs", TranslationNeeded);

            //Component: installCleanMods
            //
            English.Add("installCleanMods", "Cleaning mods folders");
            Gernam.Add("installCleanMods", TranslationNeeded);
            Polish.Add("installCleanMods", TranslationNeeded);
            French.Add("installCleanMods", TranslationNeeded);

            //Component: installExtractingMods
            //
            English.Add("installExtractingMods", "Extracting package");
            Gernam.Add("installExtractingMods", "Entpacke Paket");
            Polish.Add("installExtractingMods", "Wypakowywanie paczki");
            French.Add("installExtractingMods", "Extraction du package");

            //Component: installXmlUnpack
            //
            English.Add("installXmlUnpack", "Unpacking local XML file");
            Gernam.Add("installXmlUnpack", "Entpacke lokale XML Dateien");
            Polish.Add("installXmlUnpack", "Rozpakowywanie lokalnego pliku XML");
            French.Add("installXmlUnpack", "Décompresser le fichier XML local");

            //Component: extractingPackage
            //
            English.Add("file", "File");
            Gernam.Add("file", "Datei");
            Polish.Add("file", "Plik");
            French.Add("file", "Fichier");

            //Component: extractingPackage
            //
            English.Add("size", "Size");
            Gernam.Add("size", "Größe");
            Polish.Add("size", "Rozmiar");
            French.Add("size", "Taille");

            //Component: backupModFile
            //
            English.Add("backupModFile", "Backing up mod file");
            Gernam.Add("backupModFile", "Mod Dateien sichern");
            Polish.Add("backupModFile", "Stwórz kopię zapasową pliku modyfikacji");
            French.Add("backupModFile", "Sauvegarde du fichier mod");

            //Component: backupUserDatas
            //
            English.Add("backupUserDatas", "Backing up user data");
            Gernam.Add("backupUserDatas", "Benutzerdaten sichern");
            Polish.Add("backupUserDatas", "Stwórz kopię zapasową danych użytkownika");
            French.Add("backupUserDatas", "Sauvegarde des données utilisateur");

            //Component: backupSystemDatas
            //
            English.Add("backupSystemDatas", "Backing up mod related data");
            Gernam.Add("backupSystemDatas", TranslationNeeded);
            Polish.Add("backupSystemDatas", TranslationNeeded);
            French.Add("backupSystemDatas", TranslationNeeded);

            //Component: deletingFiles
            //
            English.Add("deletingFiles", "Deleting files");
            Gernam.Add("deletingFiles", "Lösche Dateien");
            Polish.Add("deletingFiles", "Usuwanie plików");
            French.Add("deletingFiles", "Suppression de fichiers");

            //Component DeleteMods
            //
            English.Add("scanningModsFolders", "scanning mods folders ...");
            Gernam.Add("scanningModsFolders", "Durchsuche Mod Verzeichnisse ...");
            Polish.Add("scanningModsFolders", TranslationNeeded);
            French.Add("scanningModsFolders", TranslationNeeded);

            //Component: restoringUserData
            //
            English.Add("restoringUserData", "Restoring user data");
            Gernam.Add("restoringUserData", "Benutzerdaten wiederherstellen");
            Polish.Add("restoringUserData", "Przywracanie danych użytkownika");
            French.Add("restoringUserData", "Restoration des données utilisateur");

            //Component: restoringSystemData
            //
            English.Add("restoringSystemData", "Restoring Mod-files");
            Gernam.Add("restoringSystemData", "Mod-Dateien wiederherstellen");
            Polish.Add("restoringSystemData", TranslationNeeded);
            French.Add("restoringSystemData", TranslationNeeded);

            //Component restoringData before (function moveFileEx)
            //
            English.Add("writingInstallationLogfile", "writing {0} files to installation-logfile ...");
            Gernam.Add("writingInstallationLogfile", "schreibe {0} Dateieinträge in die Installations-Logdatei ...");
            Polish.Add("writingInstallationLogfile", TranslationNeeded);
            French.Add("writingInstallationLogfile", TranslationNeeded);

            //Component: patchingFile
            //
            English.Add("patchingFile", "Patching file");
            Gernam.Add("patchingFile", "Datei wird geändert");
            Polish.Add("patchingFile", "Aktualizowanie plików");
            French.Add("patchingFile", "Patch du fichier");

            //Component: extractingUserMod
            //
            English.Add("extractingUserMod", "Extracting user mod");
            Gernam.Add("extractingUserMod", "Benutzer Modifikationen entpacken");
            Polish.Add("extractingUserMod", "Wypakowywanie modyfikacji");
            French.Add("extractingUserMod", "Extraction des mods utilisateur");

            //Component: ExtractAtlases
            English.Add("AtlasExtraction", "Extracting Atlas file");
            Gernam.Add("AtlasExtraction", "Entpacke Atlas Datei");
            Polish.Add("AtlasExtraction", "Wypakuj plik Atlas");
            French.Add("AtlasExtraction", "Extraction des fichiers Atlas");

            //Component: ExtractAtlases
            English.Add("AtlasTexture", "Texture");
            Gernam.Add("AtlasTexture", "Textur");
            Polish.Add("AtlasTexture", "Tekstury");
            French.Add("AtlasTexture", "Texture");

            //Component: CreatingAtlases
            English.Add("AtlasCreating", "Creating Atlas file");
            Gernam.Add("AtlasCreating", "Erstelle Atlas Datei");
            Polish.Add("AtlasCreating", "Tworzenie pliku Atlas");
            French.Add("AtlasCreating", "Creations des fichiers Atlas");

            //Component: installingUserFonts
            //
            English.Add("installingUserFonts", "Installing user fonts");
            Gernam.Add("installingUserFonts", "Benutzer Schriftsätze installieren");
            Polish.Add("installingUserFonts", "Instalowanie czcionek użytkownika");
            French.Add("installingUserFonts", "Installation des polices utilisateur");

            //Component: CheckDatabase
            //
            English.Add("checkDatabase", "Checking the database for outdated or no longer needed files");
            Gernam.Add("checkDatabase", "Durchsuche das Dateiarchive nach veralteten oder nicht mehr benötigten Dateien");
            Polish.Add("checkDatabase", "Trwa przeszukiwanie w bazie danych przedawnionych i niepotrzebnych plików");
            French.Add("checkDatabase", "Vérification de la base de données pour les fichiers périmés ou non nécessaires");

            //Component: 
            //function checkForOldZipFiles() 
            English.Add("parseDownloadFolderFailed", "Parsing the \"{0}\" folder failed.");
            Gernam.Add("parseDownloadFolderFailed", "Durchsehen des \"{0}\" Verzeichnisses ist fehlgeschlagen.");
            Polish.Add("parseDownloadFolderFailed", "Pobieranie informacji o folderze \"{0}\" zakończone niepowodzeniem");
            French.Add("parseDownloadFolderFailed", "L'analyse du dossier \"{0}\" a échoué.");

            //Component: installation finished
            //
            English.Add("installationFinished", "Installation is finished");
            Gernam.Add("installationFinished", "Die Installation ist abgeschlossen");
            Polish.Add("installationFinished", "Instalacja jest zakończona");
            French.Add("installationFinished", "L'installation est terminée");

            //Component:
            //
            English.Add("WoTRunningHeader", "WoT is Running");
            Gernam.Add("WoTRunningHeader", "WoT wird gerade ausgeführt.");
            Polish.Add("WoTRunningHeader", "WoT jest uruchomiony");
            French.Add("WoTRunningHeader", "WoT est en cours d`éxecution");

            //Component:
            //
            English.Add("WoTRunningMessage", "Please close World of Tanks to continue");
            Gernam.Add("WoTRunningMessage", "Um Fortzufahren, schliesse bitte World of Tanks.");
            Polish.Add("WoTRunningMessage", "Proszę zamknąć World of Tanks, aby kontynuować");
            French.Add("WoTRunningMessage", "Veuillez fermer World of Tanks pour continuer");

            //Component:
            //
            English.Add("KillRunningWoTHeader", "Problem ??");
            Gernam.Add("KillRunningWoTHeader", TranslationNeeded);
            Polish.Add("KillRunningWoTHeader", TranslationNeeded);
            French.Add("KillRunningWoTHeader", TranslationNeeded);

            //Component:
            //
            English.Add("KillRunningWoTMessage", "You where trying to go on now {0}x times, but the game is still not closed.\n\nIn some conditions, the game was closed by user, but is still running in the background (visible via the TaskManager)\n\nShould we try to kill the task for you?");
            Gernam.Add("KillRunningWoTMessage", TranslationNeeded);
            Polish.Add("KillRunningWoTMessage", TranslationNeeded);
            French.Add("KillRunningWoTMessage", TranslationNeeded);

            //Component:
            //
            English.Add("zipReadingErrorHeader", "Incomplete Download");
            Gernam.Add("zipReadingErrorHeader", "Unvollständiger Download");
            Polish.Add("zipReadingErrorHeader", "Ściąganie niekompletne");
            French.Add("zipReadingErrorHeader", "Téléchargement incomplet");

            //Component:
            //
            English.Add("zipReadingErrorMessage1", "The zip file");
            Gernam.Add("zipReadingErrorMessage1", "Die Zip-Datei");
            Polish.Add("zipReadingErrorMessage1", "Plik skomresowany formatu ZIP ");
            French.Add("zipReadingErrorMessage1", "Le fichier ZIP");

            //Component:
            //
            English.Add("zipReadingErrorMessage2", "could not be read, most likely due to an incomplete download. Please try again.");
            Gernam.Add("zipReadingErrorMessage2", "Konnte nicht gelesen werden, da es höchstwahrscheinlich ein unvollständiger Download ist. Bitte versuche es später nochmal.");
            Polish.Add("zipReadingErrorMessage2", "Nie można odczytać, prawdopodobnie niekompletność pobranych plików. Proszę spróbować ponownie");
            French.Add("zipReadingErrorMessage2", "n'as pas pus être lus, probablement un téléchargement incomplet. Veuillez réeissayer.");

            //Component:
            //
            English.Add("zipReadingErrorMessage3", "Could not be read.");
            Gernam.Add("zipReadingErrorMessage3", "Konnte nicht gelesen werden.");
            Polish.Add("zipReadingErrorMessage3", "Nie można odczytać.");
            French.Add("zipReadingErrorMessage3", "n'as pas pus être lus");

            //Component: 
            //
            English.Add("patchingSystemDeneidAccessMessage", "The patching system was denied access to the patch folder. Retry as Administrator. If you see this again, you need to fix your" +
                " file and folder security permissions");
            Gernam.Add("patchingSystemDeneidAccessMessage", "Dem Patching-System wurde der Zugriff auf den Patch-Ordner verweigert. Wiederholen als Administrator. Wenn Sie dies wieder sehen," +
                " müssen Sie Ihre Datei- und Ordnersicherheitsberechtigungen reparieren");
            Polish.Add("patchingSystemDeneidAccessMessage", "Nie uzyskano dostępu do folderu patcha. Spróbuj ponownie jako administrator. Jeśli widzisz to ponownie, to zmień ustawienia" +
                " pozwoleń dostępu do folderów.");
            French.Add("patchingSystemDeneidAccessMessage", "Le système de patching s'est vu refuser l'accès au dossier de patch. Réessayez en tant qu'administrateur. Si vous voyez ceci" +
                " à nouveau, assurez vous que vos permissions de sécurités de dossiers et de fichiers son suffisantes.");

            //Component: 
            //
            English.Add("patchingSystemDeneidAccessHeader", "Access Denied");
            Gernam.Add("patchingSystemDeneidAccessHeader", "Zugriff abgelehnt");
            Polish.Add("patchingSystemDeneidAccessHeader", "Odmowa dostępu");
            French.Add("patchingSystemDeneidAccessHeader", "Accès refusé");

            //Component: Failed To Delete folder
            //
            English.Add("folderDeleteFailed", "Failed to delete the folder");
            Gernam.Add("folderDeleteFailed", "Löschen des Verzeichnis fehlgeschlagen");
            Polish.Add("folderDeleteFailed", "Próba usunięcia folderu zakończona niepowodzeniem");
            French.Add("folderDeleteFailed", "Échec de la suppression du dossier");

            //Component: Failed To Delete file
            //
            English.Add("fileDeleteFailed", "Failed to delete the file");
            Gernam.Add("fileDeleteFailed", "Löschen der Datei fehlgeschlagen");
            Polish.Add("fileDeleteFailed", "Próba usunięcia pliku zakończona niepowodzeniem");
            French.Add("fileDeleteFailed", "Échec de la supression du fichier");

            //Component: DeleteBackupFolder
            //
            English.Add("DeleteBackupFolder", "Backups");
            Gernam.Add("DeleteBackupFolder", "Sicherungen");
            Polish.Add("DeleteBackupFolder", TranslationNeeded);
            French.Add("DeleteBackupFolder", TranslationNeeded);
            #endregion

            #region Install finished window
            //Component: InstallCompleteLabel
            //
            English.Add("InstallCompleteLabel", "The Installation is complete. Would you like to...");
            Gernam.Add("InstallCompleteLabel", "Installation ist beendet. Willst du...");
            Polish.Add("InstallCompleteLabel", "Instalacja jest zakończona. Czy chciałbyś...");
            French.Add("InstallCompleteLabel", "L'installation est terminée. Voudriez-vous...");

            //Component: StartTanksButton
            //
            English.Add("StartTanksButton", "Start the game? (WorldofTanks.exe)");
            Gernam.Add("StartTanksButton", "Das Spiel starten? (WorldofTanks.exe)");
            Polish.Add("StartTanksButton", "Uruchomić grę? (WorldofTanks.exe)");
            French.Add("StartTanksButton", "Démarrez le jeu? (WorldofTanks.exe)");

            //Component: StartWoTLauncherButton
            //
            English.Add("StartWoTLauncherButton", "Start the game launcher? (WoTLauncher.exe)");
            Gernam.Add("StartWoTLauncherButton", "Den Spiel Launcher starten? (WoTLauncher.exe)");
            Polish.Add("StartWoTLauncherButton", "Uruchomić launcher? (WoTLauncher.exe)");
            French.Add("StartWoTLauncherButton", "Démarrez le lanceur de jeux? (WoTLauncher.exe)");

            //Component: StartXVMStatButton
            //
            English.Add("StartXVMStatButton", "Open your web browser to the xvm statistics login website?");
            Gernam.Add("StartXVMStatButton", "Öffne Deinen Browser auf der XVM Statistik Login Webseite?");
            Polish.Add("StartXVMStatButton", "Otworzyć stronę statystyk XVM ?");
            French.Add("StartXVMStatButton", "Ouvrir votre navigateur Web vers le site de connexion aux statistiques xvm?");

            //Component: CloseApplicationButton
            //
            English.Add("CloseApplicationButton", "Close the application?");
            Gernam.Add("CloseApplicationButton", "Anwendung schließen");
            Polish.Add("CloseApplicationButton", "Zamknij aplikację");
            French.Add("CloseApplicationButton", "Fermer l'application");

            //Component: StartXVMStatButton_Click
            //localisation to which page you will jump
            English.Add("xvmUrlLocalisation", "en");
            Gernam.Add("xvmUrlLocalisation", "de");
            Polish.Add("xvmUrlLocalisation", "en");
            French.Add("xvmUrlLocalisation", "fr");

            //Component: loadingGifpreview
            //GifPreview.cs
            English.Add("loadingGifpreview", "Loading Picture Preview");
            Gernam.Add("loadingGifpreview", "Ladebild Vorschau");
            Polish.Add("loadingGifpreview", "Ładowanie Podglądu");
            French.Add("loadingGifpreview", "Chargement de l'image de prévisualisation");
            #endregion

            #region Diagnostics
            //Component: MainTextBox
            //
            English.Add("DiagnosticsMainTextBox", "You can use the options below to try to diagnose or solve the issues you are having.");
            Gernam.Add("DiagnosticsMainTextBox", "Du kannst mit den untenstehenden Optionen Probleme mit dem Spiel zu diagnostizieren und ggf. zu beheben.");
            Polish.Add("DiagnosticsMainTextBox", "Opcje diagnostyczne i rozwiązywanie problemów");
            French.Add("DiagnosticsMainTextBox", "Vous pouvez utiliser les options ci dessous pour essayer de diagnostiqué ou résoudre les soucis que vous avez");

            //Component: LaunchWoTLauncher
            //
            English.Add("LaunchWoTLauncher", "Start the World of Tanks launcher in integrity validation mode");
            Gernam.Add("LaunchWoTLauncher", "Starte den \"World of Tanks Launcher\" im Integritätsvalidierungsmodus");
            Polish.Add("LaunchWoTLauncher", "Uruchom launcher World of Tanks w trybie sprawdzania integralności.");
            French.Add("LaunchWoTLauncher", "Lancé le launcher world of tanks en mode vérification d'intégrité");

            //Component: CollectLogInfo
            //
            English.Add("CollectLogInfo", "Collect log files into a zip file to report a problem");
            Gernam.Add("CollectLogInfo", TranslationNeeded);
            Polish.Add("CollectLogInfo", TranslationNeeded);
            French.Add("CollectLogInfo", TranslationNeeded);

            //Component: SelectedInstallation
            //
            English.Add("SelectedInstallation", "Currently Selected Installation:");
            Gernam.Add("SelectedInstallation", "Aktuell gewählte Installation:");
            Polish.Add("SelectedInstallation", "Obecnie Wybrana Instalacja:");
            French.Add("SelectedInstallation", "Installation actuellement sélectionner:");

            //Component: SelectedInstallationNone
            //
            English.Add("SelectedInstallationNone", "(none)");
            Gernam.Add("SelectedInstallationNone", TranslationNeeded);
            Polish.Add("SelectedInstallationNone", TranslationNeeded);
            French.Add("SelectedInstallationNone", TranslationNeeded);

            //Component: collectionLogInfo
            //
            English.Add("collectionLogInfo", "Collecting log files...");
            Gernam.Add("collectionLogInfo", "Sammeln der Protokolldateien...");
            Polish.Add("collectionLogInfo", "Zbieranie logów...");
            French.Add("collectionLogInfo", "Collection des fichiers log...");

            //Component: startingLauncherRepairMode
            //
            English.Add("startingLauncherRepairMode", "Starting WoTLauncher in integrity validation mode...");
            Gernam.Add("startingLauncherRepairMode", "Starte den WoT Launcher im Integritätsvalidierungsmodus...");
            Polish.Add("startingLauncherRepairMode", "Uruchamianie WoTLaunchera w trybie sprawdzania integralności...");
            French.Add("startingLauncherRepairMode", "Lancement de WoTLauncher and mode the validation d'intégrité...");

            //Component: failedStartLauncherRepairMode
            //
            English.Add("failedStartLauncherRepairMode", "Failed to start WoT Launcher in Repair mode");
            Gernam.Add("failedStartLauncherRepairMode", "Der WoT Launcher konnte nicht im Reparaturmodus gestartet werden");
            Polish.Add("failedStartLauncherRepairMode", "Nie udało się uruchomić launchera WoT w trybie naprawy");
            French.Add("failedStartLauncherRepairMode", "Erreur lors du lancement de WoTLauncher en mode de réparation");

            //Component: failedCollectFile
            //
            English.Add("failedCollectFile", "Failed to collect the file ");
            Gernam.Add("failedCollectFile", "Fehler beim Sammeln der Datei ");
            Polish.Add("failedCollectFile", "Nie udało się zebrać plików ");
            French.Add("failedCollectFile", "Erreur lors de la collecte du fichier");

            //Component: failedCreateZipfile
            //
            English.Add("failedCreateZipfile", "Failed to create the zip file ");
            Gernam.Add("failedCreateZipfile", "Fehler beim Erstellen der Zip-Datei ");
            Polish.Add("failedCreateZipfile", "Nie udało się stworzyć pliku zip ");
            French.Add("failedCreateZipfile", "Erreur lors de la création du fichier zip ");

            //Component: launcherRepairModeStarted
            //
            English.Add("launcherRepairModeStarted", "Repair mode successfully started");
            Gernam.Add("launcherRepairModeStarted", "Reparaturmodus wurde erfolgreich gestartet");
            Polish.Add("launcherRepairModeStarted", "Uruchomiono tryb naprawy");
            French.Add("launcherRepairModeStarted", "Mode de réparation démarrer avec succès");

            //Component: ClearDownloadCache
            //
            English.Add("ClearDownloadCache", "Clear download cache");
            Gernam.Add("ClearDownloadCache", TranslationNeeded);
            Polish.Add("ClearDownloadCache", TranslationNeeded);
            French.Add("ClearDownloadCache", TranslationNeeded);

            //Component: ClearDownloadCacheDatabase
            //
            English.Add("ClearDownloadCacheDatabase", "Delete download cache database file");
            Gernam.Add("ClearDownloadCacheDatabase", TranslationNeeded);
            Polish.Add("ClearDownloadCacheDatabase", TranslationNeeded);
            French.Add("ClearDownloadCacheDatabase", TranslationNeeded);

            //Component: ClearDownloadCacheDescription
            //
            English.Add("ClearDownloadCacheDescription", "Delete all files in the \"RelhaxDownloads\" folder");
            Gernam.Add("ClearDownloadCacheDescription", TranslationNeeded);
            Polish.Add("ClearDownloadCacheDescription", TranslationNeeded);
            French.Add("ClearDownloadCacheDescription", TranslationNeeded);

            //Component: ClearDownloadCacheDatabaseDescription
            //
            English.Add("ClearDownloadCacheDatabaseDescription", "Delete the xml database file. This will cause all zip files to be re-checked for integrity.\nAll invalid files will be re-downloaded if selected in your next installation.");
            Gernam.Add("ClearDownloadCacheDatabaseDescription", TranslationNeeded);
            Polish.Add("ClearDownloadCacheDatabaseDescription", TranslationNeeded);
            French.Add("ClearDownloadCacheDatabaseDescription", TranslationNeeded);

            //Component: ChangeInstall
            //
            English.Add("ChangeInstall", "Change the currently selected WoT installation");
            Gernam.Add("ChangeInstall", TranslationNeeded);
            Polish.Add("ChangeInstall", TranslationNeeded);
            French.Add("ChangeInstall", TranslationNeeded);

            //Component: ChangeInstallDescription
            //
            English.Add("ChangeInstallDescription", "This will change which log files will get added to the diagnostics zip file");
            Gernam.Add("ChangeInstallDescription", TranslationNeeded);
            Polish.Add("ChangeInstallDescription", TranslationNeeded);
            French.Add("ChangeInstallDescription", TranslationNeeded);

            //Component: zipSavedTo
            //
            English.Add("zipSavedTo", "Zip file saved to: ");
            Gernam.Add("zipSavedTo", "Zip-Datei gespeichert in: ");
            Polish.Add("zipSavedTo", "Plik zip zapisano do: ");
            French.Add("zipSavedTo", "Fichier zip sauvegarder à: ");
            #endregion

            //Component: ModSelectionList
            //Component: seachCB ToolTip
            English.Add("searchToolTip", "You can also search for multiple name parts, separated by a * (asterisk).\nFor example: config*willster419 will display as search result: Willster419\'s Config");
            Gernam.Add("searchToolTip", "Du kannst auch nach mehreren Namensteilen suchen, getrennt durch ein * (Sternchen).\nZum Beispiel: config*willster419  wird als Suchergebnis anzeigen: Willster419\'s Config");
            Polish.Add("searchToolTip", "Można również wyszukiwać wiele części nazw oddzielonych gwiazdką (*).\nNa przykład: config * willster419 wyświetli się jako wynik wyszukiwania: Willster419's Config");
            French.Add("searchToolTip", "Vous pouvez également rechercher plusieurs parties de nom, séparées par un * (astérisque).\nPar exemple: config *" +
                " willster419 affichera comme résultat de la recherche: Config de Willster419");

            //Component: ModSelectionList
            //Component: seachTB
            English.Add("searchTB", "Search Mod Name:");
            Gernam.Add("searchTB", "Suche Mod Namen:");
            Polish.Add("searchTB", "Wyszukaj nazwę modu:");
            French.Add("searchTB", "Rechercher le nom du mod:");
            
        }

        private static void LegacyTranlationsToKeepButNotLoadJustInCaseWeEverNeedThemAgainJustInCase()
        {

            //Component: toolTip
            //
            English.Add("mainFormToolTip", "Right click for extended description");
            Gernam.Add("mainFormToolTip", "Rechtsklick für eine erweiterte Beschreibung");
            Polish.Add("mainFormToolTip", "Rozwiń opis PPM");
            French.Add("mainFormToolTip", "Clic droit pour une description étendue");

            //Component: cancerFontCB
            //
            English.Add("ComicSansFontCB", "Comic Sans font");
            Gernam.Add("ComicSansFontCB", " Schriftart Comic Sans");
            Polish.Add("ComicSansFontCB", "Czcionka Comic Sans");
            French.Add("ComicSansFontCB", "Police Comic Sans");

            //Component: settingsGroupBox
            //
            English.Add("settingsGroupBox", "Settings");
            Gernam.Add("settingsGroupBox", "Einstellungen");
            Polish.Add("settingsGroupBox", "Ustawienia");
            French.Add("settingsGroupBox", "Paramètres");

            //Component: darkUICB
            //
            English.Add("darkUICB", "Dark UI");
            Gernam.Add("darkUICB", "Dunkle Benutzeroberfläche");
            Polish.Add("darkUICB", "Ciemny Interfejs");
            French.Add("darkUICB", "Interface sombre");

            //Component: largerFontButton
            //
            English.Add("largerFontButton", "Larger font");
            Gernam.Add("largerFontButton", "Grössere Schriftart");
            Polish.Add("largerFontButton", "Większa czcionka");
            French.Add("largeFontButton", "Grande police");

            //Component: ShowAdvancedSettingsLink
            English.Add("ShowAdvancedSettingsLink", "Show advanced settings");
            Gernam.Add("ShowAdvancedSettingsLink", "Erweiterte Einstellungen");
            Polish.Add("ShowAdvancedSettingsLink", "Pokaż ustawienia zaawansowane");
            French.Add("ShowAdvancedSettingsLink", "Afficher les paramètres avancé");

            //Component: DPIAUTO
            AddTranslationToAll("DPIAUTO", "AUTO");

            //Component: DPI100
            AddTranslationToAll("DPI100", "DPI 1.0x");

            //Component: DPI125
            AddTranslationToAll("DPI125", "DPI 1.25x");

            //Component: DPI175
            AddTranslationToAll("DPI175", "DPI 1.75x");

            //Component: DPI225
            AddTranslationToAll("DPI225", "DPI 2.25x");

            //Component: DPI275
            AddTranslationToAll("DPI275", "DPI 2.75x");

            //Component: fontSize100
            AddTranslationToAll("fontSize100", "Font 1.0x");

            //Component: fontSize125
            AddTranslationToAll("fontSize125", "Font 1.25x");

            //Component: fontSize175
            AddTranslationToAll("fontSize175", "Font 1.75x");

            //Component: fontSize225
            AddTranslationToAll("fontSize225", "Font 2.25x");

            //Component: fontSize275
            AddTranslationToAll("fontSize275", "Font 2.75x");

            //Component: label2
            //
            English.Add("TabIndicatesTB", "\"*\" (asterisk) tab indicates single selection tab");
            Gernam.Add("TabIndicatesTB", "Bei einem Tab mit einem\"*\" (Sternchen), kann nur eins der primären Mods ausgewählt werden.");
            Polish.Add("TabIndicatesTB", "\"*\" wskazuje pojedynczą kartę wyboru");
            French.Add("TabIndicatesTB", "Onglet \"*\" indique un onglet a sélection unique");

            //Component: 
            //
            English.Add("ComicSansFontCBDescription", "Enable Comic Sans font");
            Gernam.Add("ComicSansFontCBDescription", "Schriftart Comic Sans aktivieren");
            Polish.Add("ComicSansFontCBDescription", "Włącz czcionkę Comic Sans");
            French.Add("ComicSansFontCBDescription", "Activé la police Comic Sans");

            //Component: 
            //
            English.Add("enlargeFontDescription", "Enlarge font");
            Gernam.Add("enlargeFontDescription", "Schriftart vergrössern");
            Polish.Add("enlargeFontDescription", "Powiększ czcionkę");
            French.Add("enlargeFontDescription", "Agrandir la police");

            //Component: 
            //
            English.Add("darkUICBDescription", "Toggle the DarkUI mode. Usefull for working with the modpack at night.");
            Gernam.Add("darkUICBDescription", "Auf dunklen UI Modus umschalten. Nützlich für die Arbeit mit dem Modpack in der Nacht.");
            Polish.Add("darkUICBDescription", "Zmień interfejs na ciemny. Przydatneprzy pracy z paczką w nocy.");
            French.Add("darkUICBDescription", "Activer l'interface sombre. Utile pour utilisé le modpack pendant la nuit");

            //Component: FontLayoutPanelDescription
            //
            English.Add("FontLayoutPanelDescription", "Select a scale mode to use.");
            Gernam.Add("FontLayoutPanelDescription", "Wähle einen Skalierungsgrad.");
            Polish.Add("FontLayoutPanelDescription", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("FontLayoutPanelDescription", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: fontSize100Description
            //
            English.Add("fontSize100Description", "Select a scale mode to use.");
            Gernam.Add("fontSize100Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("fontSize100Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("fontSize100Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: fontSize125Description
            //
            English.Add("fontSize125Description", "Select a scale mode to use.");
            Gernam.Add("fontSize125Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("fontSize125Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("fontSize125Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: fontSize175Description
            //
            English.Add("fontSize175Description", "Select a scale mode to use.");
            Gernam.Add("fontSize175Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("fontSize175Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("fontSize175Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: fontSize225Description
            //
            English.Add("fontSize225Description", "Select a scale mode to use.");
            Gernam.Add("fontSize225Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("fontSize225Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("fontSize225Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: fontSize275Description
            //
            English.Add("fontSize275Description", "Select a scale mode to use.");
            Gernam.Add("fontSize275Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("fontSize275Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("fontSize275Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: DPIAUTODescription
            //
            English.Add("DPIAUTODescription", "Select a scale mode to use.");
            Gernam.Add("DPIAUTODescription", "Wähle einen Skalierungsgrad.");
            Polish.Add("DPIAUTODescription", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("DPIAUTODescription", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: DPI100Description
            //
            English.Add("DPI100Description", "Select a scale mode to use.");
            Gernam.Add("DPI100Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("DPI100Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("DPI100Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: DPI125Description
            //
            English.Add("DPI125Description", "Select a scale mode to use.");
            Gernam.Add("DPI125Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("DPI125Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("DPI125Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: DPI175Description
            //
            English.Add("DPI175Description", "Select a scale mode to use.");
            Gernam.Add("DPI175Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("DPI175Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("DPI175Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: DPI225Description
            //
            English.Add("DPI225Description", "Select a scale mode to use.");
            Gernam.Add("DPI225Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("DPI225Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("DPI225Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: DPI275Description
            //
            English.Add("DPI275Description", "Select a scale mode to use.");
            Gernam.Add("DPI275Description", "Wähle einen Skalierungsgrad.");
            Polish.Add("DPI275Description", "Wybierz tryb skali, który ma zostać użyty.");
            French.Add("DPI275Description", "Sélectionnez un mode d'échelle à utiliser.");

            //Component: fontSizeGB
            //
            English.Add("fontSizeGB", "Scaling Mode");
            Gernam.Add("fontSizeGB", "Skalierungsmodus");
            Polish.Add("fontSizeGB", "Tryb skalowania");
            French.Add("fontSizeGB", "Mode d'échelle");

            #region Advanced Settings View
            //Component: form name
            English.Add("AdvancedSettings", "Advanced Settings");
            Gernam.Add("AdvancedSettings", "Erweiterte Einstellungen");
            Polish.Add("AdvancedSettings", "Ustawienia Zaawansowane");
            French.Add("AdvancedSettings", "Configurations avancé");

            //Component: AdvancedSettingsHeader
            English.Add("AdvancedSettingsHeader", "Hover over a setting to see its description");
            Gernam.Add("AdvancedSettingsHeader", "Mit der Maus über die Einstellungen fahren um die Beschreibungen einzublenden");
            Polish.Add("AdvancedSettingsHeader", "NajedŸ kursorem, aby zobaczyæ opis");
            French.Add("AdvancedSettingsHeader", "Survoler un paramètre pour voire sa description");

            //Component: UseAltUpdateMethodCB
            English.Add("UseAltUpdateMethodCB", "Use alternative update method");
            Gernam.Add("UseAltUpdateMethodCB", "Eine alternative Updatemethode verwenden");
            Polish.Add("UseAltUpdateMethodCB", "Użyj alternatywnej metody aktualizacji");
            French.Add("UseAltUpdateMethodCB", "Utiliser la methode alternative de mise à jour");

            //Component: UseAltUpdateMethodCBDescription
            English.Add("UseAltUpdateMethodCBDescription", "Use the alternative update method. Takes longer but should help with anti-virus software");
            Gernam.Add("UseAltUpdateMethodCBDescription", "Die alternative Updatemethode dauert länger, hilft aber bei Problemen (bspw. Fehlalarm) mit Anti-Virus-Programmen");
            Polish.Add("UseAltUpdateMethodCBDescription", "U¿yj alternatywnej metody aktualizacji. Wyd³u¿y to potrzebny czas, ale pomo¿e przy problemach z oprogramowaniem antywirusowym");
            French.Add("UseAltUpdateMethodCBDescription", "Utiliser la methode alternative de mise à jour. Plus long mais devrais aider avec les applications anti-virus");
            #endregion

            #region Scaling Verification Window
            //Component: form name
            English.Add("FontSettingsVerify", "Scaling");
            Gernam.Add("FontSettingsVerify", "Skalierung");
            Polish.Add("FontSettingsVerify", "Skalowanie");
            French.Add("FontSettingsVerify", "Mise à l'échelle");

            //Component: SettingsChangedHeader
            English.Add("SettingsChangedHeader", "Your Scaling Settings have changed. Would you like to keep them?");
            Gernam.Add("SettingsChangedHeader", "Deine Skalierungseinstellungen wurden geändert, willst du sie behalten?");
            Polish.Add("SettingsChangedHeader", "Twoje opcje skalowania siê zmieni³y. Czy chcia³byœ je zachowaæ?");
            French.Add("SettingsChangedHeader", "Vos paramètres de mise a l'échelle ont changé. Voulez-vous les garders?");

            //Component: RevertingTimeoutText
            English.Add("RevertingTimeoutText", "Reverting in {0} seconds");
            Gernam.Add("RevertingTimeoutText", "In {0} Sekunden werden sie zurückgesetzt");
            Polish.Add("RevertingTimeoutText", "Powrót do poprzednich ustawieñ za {0}s.");
            French.Add("RevertingTimeoutText", "Rétablissement dans {0} secondes");

            //Component: NoButton
            English.Add("NoButton", "No");
            Gernam.Add("NoButton", "Nein");
            Polish.Add("NoButton", "Nie");
            French.Add("NoButton", "Non");

            //Component: YesButton
            English.Add("YesButton", "Yes");
            Gernam.Add("YesButton", "Ja");
            Polish.Add("YesButton", "Tak");
            French.Add("YesButton", "Oui");
            #endregion

            //Component: cleanUninstallCB
            //
            English.Add("cleanUninstallCB", "Clean uninstallation");
            Gernam.Add("cleanUninstallCB", "Saubere deinstallation");
            Polish.Add("cleanUninstallCB", "Czysta deinstalacja");
            French.Add("cleanUninstallCB", "Désinstallation propre");
        }
        #endregion

        #region Applying Window Translations
        /// <summary>
        /// Applies localized text translations for the passed in window
        /// See the comments in the method for more information
        /// </summary>
        /// <param name="window">The window to apply translations to</param>
        /// <param name="applyToolTips">Set to true to seach and apply tooltips to the components</param>
        public static void LocalizeWindow(Window window, bool applyToolTips)
        {
            //Get a list of all visual class controls curently presend and loaded in the window
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(window, false);
            foreach(FrameworkElement v in allWindowControls)
            {
                TranslateComponent(v, true);
            }
        }

        
        private static void TranslateComponent(FrameworkElement frameworkElement, bool applyToolTips)
        {
            //TODO: pass in the object itself so now we can consider blacklist correcly and only apply when we should
            //first check name is none or on blacklist
            string componentName = frameworkElement.Name;
            if (string.IsNullOrWhiteSpace(componentName))
            {
                //log debug translation component is blank null or from blacklist
                //Logging.WriteToLog("Translation component name is blank", Logfiles.Application, LogLevel.Debug);
                return;
            }
            if (TranslationComponentBlacklist.Contains(componentName))
            {
                Logging.WriteToLog(string.Format("Skipping translation of {0}, present in blacklist and consider=true", componentName), Logfiles.Application, LogLevel.Debug);
                return;
            }
            //getting here means that the object is a framework UI element, has a name, and is not on te blacklist. it's safe to translate
            //use the "is" keyword to be able to apply translations (text is under different properties for each type of visuals)
            if (frameworkElement is Control control)
            {
                //Generic control
                //headered content controls have a header and content object
                if (control is HeaderedContentControl headeredContentControl)
                {
                    //ALWAYS make sure that the header and content are of type string BEFORE over-writing! (what if it is an image?)
                    if (headeredContentControl.Header is string)
                        headeredContentControl.Header = GetTranslatedString(headeredContentControl.Name + "Header");
                    if (headeredContentControl.Content is string)
                        headeredContentControl.Content = GetTranslatedString(headeredContentControl.Name);
                    if (applyToolTips)
                    {
                        if(Exists(headeredContentControl.Name + "Description"))
                            headeredContentControl.ToolTip = GetTranslatedString(headeredContentControl.Name + "Description");
                    }  
                }
                //content controls have only a heder
                else if (control is ContentControl contentControl)
                {
                    //ALWAYS make sure that the header and content are of type string BEFORE over-writing! (what if it is an image?)
                    if (contentControl.Content is string)
                        contentControl.Content = GetTranslatedString(contentControl.Name);
                    if (applyToolTips)
                    {
                        if (Exists(contentControl.Name + "Description"))
                            contentControl.ToolTip = GetTranslatedString(contentControl.Name + "Description");
                    }
                }
                //textbox only has string text as input
                else if (control is TextBox textBox)
                {
                    textBox.Text = GetTranslatedString(textBox.Name);
                    if (applyToolTips)
                    {
                        if (Exists(textBox.Name + "Description"))
                            textBox.ToolTip = GetTranslatedString(textBox.Name + "Description");
                    }
                }
                
            }
            else if (frameworkElement is TextBlock textBlock)
            {
                //lightweight block of text that only uses string as it's input. makes it not a control (no content of children property)
                textBlock.Text = GetTranslatedString(textBlock.Name);
                //apply tool tips?
                if (applyToolTips)
                {
                    if (Exists(textBlock.Name + "Description"))
                        textBlock.ToolTip = GetTranslatedString(textBlock.Name + "Description");
                }
            }
        }
        #endregion
    }
}
