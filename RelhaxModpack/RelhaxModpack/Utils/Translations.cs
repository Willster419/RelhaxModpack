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
        French,

        /// <summary>
        /// The Russian language
        /// </summary>
        Russian
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
            "DiagnosticsStatusTextBox",
            "seachCB",
            "PART_EditableTextBox",
            "ApplyCustomScalingLabel",
            "DatabaseUpdateText",
            "ApplicationUpdateText",
            "LoadingText",
            "PreviewDescriptionBox",
            "PreviewUpdatesBox",
            //RelhaxHyperlink components
            "ParentTextBlock",
            "ChildTextblock",
            //loading header for loading window
            "LoadingHeader",
            //application update notes textbox
            "ApplicationUpdateNotes"
        };
        private const string TranslationNeeded = "TODO";
        private static readonly string Blank = string.Empty;

        /// <summary>
        /// Get if the translation dictionaries have been loaded yet
        /// </summary>
        public static bool TranslationsLoaded { get; private set; } = false;

        /// <summary>
        /// English language string identifier in national language
        /// </summary>
        public const string LanguageEnglish = "English";
        /// <summary>
        /// Polish language string identifier in national language
        /// </summary>
        public const string LanguagePolish = "Polski";
        /// <summary>
        /// German language string identifier in national language
        /// </summary>
        public const string LanguageGerman = "Deutsch";
        /// <summary>
        /// French language string identifier in national language
        /// </summary>
        public const string LanguageFrench = "Français";
        /// <summary>
        /// French language string identifier in national language
        /// </summary>
        public const string LanguageRussian = "Pусский";

        private static Dictionary<string, string> English = new Dictionary<string, string>();
        private static Dictionary<string, string> German = new Dictionary<string, string>();
        private static Dictionary<string, string> Polish = new Dictionary<string, string>();
        private static Dictionary<string, string> French = new Dictionary<string, string>();
        private static Dictionary<string, string> Russian = new Dictionary<string, string>();

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
                    CurrentLanguage = German;
                    break;
                case Languages.Polish:
                    CurrentLanguage = Polish;
                    break;
                case Languages.Russian:
                    CurrentLanguage = Russian;
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
            Logging.Info("Detected OS Language: " + CultureInfo.CurrentCulture.DisplayName);
            //list of culture keys
            //http://www.localeplanet.com/dotnet/
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
                case "ru":
                    SetLanguage(Languages.Russian);
                    break;
                default:
                    SetLanguage(Languages.English);
                    break;
            }
            Logging.Info("Language has been set: {0}", ModpackSettings.Language.ToString());
        }
        /// <summary>
        /// Get a localized string in the currently selected language
        /// </summary>
        /// <param name="componentName">The key value of the string phrase</param>
        /// <returns></returns>
        public static string GetTranslatedString(string componentName)
        {
            string s;
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
                    }
                }
                //Log error it does not exist
                Logging.WriteToLog(string.Format("component {0} does not exist in any languages", componentName), Logfiles.Application, LogLevel.Error);
                s=componentName;
            }
            return s;
        }

        /// <summary>
        /// Checks is a component (key value) exists in the given language (dictionary)
        /// </summary>
        /// <param name="componentName">The keyword phrase to check</param>
        /// <param name="languageToCheck">The language dictionary to check in</param>
        /// <returns></returns>
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
                    DictToCheck = German;
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
            German.Add(key, message);
            Polish.Add(key, message);
            French.Add(key, message);
            Russian.Add(key, message);
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
            German.Add("yes", "ja");
            Polish.Add("yes", "Tak");
            French.Add("yes", "Oui");
            Russian.Add("yes", "Да");

            English.Add("no", "no");
            German.Add("no", "nein");
            Polish.Add("no", "Nie");
            French.Add("no", "Non");
            Russian.Add("no", "Нет");

            English.Add("cancel", "Cancel");
            German.Add("cancel", "Abbrechen");
            Polish.Add("cancel", "Anuluj");
            French.Add("cancel", "Anuler");
            Russian.Add("cancel", "Отмена");

            English.Add("delete", "Delete");
            German.Add("delete", "Löschen");
            Polish.Add("delete", "Usuń");
            French.Add("delete", "Supprimer");
            Russian.Add("delete", "Удалить");

            English.Add("warning", "WARNING");
            German.Add("warning", "WARNUNG");
            Polish.Add("warning", "OSTRZEŻENIE");
            French.Add("warning", "ATTENTION");
            Russian.Add("warning", "ВНИМАНИЕ");

            English.Add("critical", "CRITICAL");
            German.Add("critical", "KRITISCH");
            Polish.Add("critical", "BŁĄD KRYTYCZNY");
            French.Add("critical", "CRITIQUAL");
            Russian.Add("critical", "КРИТИЧЕСКАЯ ОШИБКА");

            English.Add("information", "Information");
            German.Add("information", "Information");
            Polish.Add("information", "Informacja");
            French.Add("information", "information");
            Russian.Add("information", "Информация");

            English.Add("select", "Select");
            German.Add("select", "Auswählen");
            Polish.Add("select", "Wybierz");
            French.Add("select", "Sélectionner");
            Russian.Add("select", "Выбрать");

            English.Add("abort", "Abort");
            German.Add("abort", "Abbrechen");
            Polish.Add("abort", "Przerwij");
            French.Add("abort", "Annuler");
            Russian.Add("abort", "Отменить");

            English.Add("error", "Error");
            German.Add("error", "Fehler");
            Polish.Add("error", "Błąd");
            French.Add("error", "Erreur");
            Russian.Add("error", "Ошибка");

            English.Add("retry", "Retry");
            German.Add("retry", "Wiederholen");
            Polish.Add("retry", "Spróbuj ponownie");
            French.Add("retry", "Reaissayer");
            Russian.Add("retry", "Повторить");

            English.Add("ignore", "Ignore");
            German.Add("ignore", "Ignorieren");
            Polish.Add("ignore", "Ignoruj");
            French.Add("ignore", "Ignorer");
            Russian.Add("ignore", "Игнорировать");

            English.Add("lastUpdated", "Last Updated: ");
            German.Add("lastUpdated", "Letzte Aktualisierung: ");
            Polish.Add("lastUpdated", "Ostatnio zaktualizowano: ");
            French.Add("lastUpdated", "Dernière mise à jour: ");
            Russian.Add("lastUpdated", "Последнее обновление: ");

            English.Add("stepsComplete", "tasks completed");
            German.Add("stepsComplete", "erledigte Aufgaben");
            Polish.Add("stepsComplete", "zadania zakończone");
            French.Add("stepsComplete", "tâches terminées");
            Russian.Add("stepsComplete", "заданий выполнено");

            //Component: allFiles
            //
            English.Add("allFiles", "All files");
            German.Add("allFiles", TranslationNeeded);
            Polish.Add("allFiles", TranslationNeeded);
            French.Add("allFiles", TranslationNeeded);
            Russian.Add("allFiles", TranslationNeeded);
            #endregion

            #region Application messages
            //Component: appFailedCreateLogfile
            //When the application first starts, it tries to open a logfile
            English.Add("appFailedCreateLogfile", "The application failed to open a logfile. Check your file permissions or move the application to a folder with write access.");
            German.Add("appFailedCreateLogfile", TranslationNeeded);
            Polish.Add("appFailedCreateLogfile", TranslationNeeded);
            French.Add("appFailedCreateLogfile", TranslationNeeded);
            Russian.Add("appFailedCreateLogfile", TranslationNeeded);

            //Component: failedToParse
            //
            English.Add("failedToParse", "Failed to parse the file");
            German.Add("failedToParse", TranslationNeeded);
            Polish.Add("failedToParse", TranslationNeeded);
            French.Add("failedToParse", TranslationNeeded);
            Russian.Add("failedToParse", TranslationNeeded);
            #endregion

            #region Tray Icon
            //Component: MenuItemRestore
            //The menu item for restoring the application
            English.Add("MenuItemRestore", "Restore");
            German.Add("MenuItemRestore", "Wiederherstellen");
            Polish.Add("MenuItemRestore", "Przywróć");
            French.Add("MenuItemRestore", "Restaurer");
            Russian.Add("MenuItemRestore", "Восстановить");

            //Component: MenuItemCheckUpdates
            //The menu item for restoring the application
            English.Add("MenuItemCheckUpdates", "Check for Updates");
            German.Add("MenuItemCheckUpdates", "Nach Updates suchen");
            Polish.Add("MenuItemCheckUpdates", "Sprawdź aktualizacje");
            French.Add("MenuItemCheckUpdates", "Vérifier les mises à jour");
            Russian.Add("MenuItemCheckUpdates", "Проверить наличие обновлений");

            //Component: MenuItemAppClose
            //The menu item for restoring the application
            English.Add("MenuItemAppClose", "Close");
            German.Add("MenuItemAppClose", "Schließen");
            Polish.Add("MenuItemAppClose", "Zamknij");
            French.Add("MenuItemAppClose", "Fermer");
            Russian.Add("MenuItemAppClose", "Закрыть");

            //Component: newDBApplied
            //MessageBox for when a new database version is applied
            English.Add("newDBApplied", "New database version applied");
            German.Add("newDBApplied", "Neue Datenbankversion angewendet");
            Polish.Add("newDBApplied", "Zastosowano nową bazę danych");
            French.Add("newDBApplied", "Nouvelle version de base de données appliquée");
            Russian.Add("newDBApplied", "Применена новая версия базы данных");
            #endregion

            #region Main Window
            //Component: InstallModpackButton
            //The button for installing the modpack
            English.Add("InstallModpackButton", "Start mod selection");
            German.Add("InstallModpackButton", "Auswahl der Mods");
            Polish.Add("InstallModpackButton", "Przejdź Do Wyboru Modyfikacji");
            French.Add("InstallModpackButton", "Sélection des mods");
            Russian.Add("InstallModpackButton", "Начать выбор модов");

            //Component: InstallModpackButtonDescription
            //
            English.Add("InstallModpackButtonDescription", "Select the mods you want to install to your WoT client");
            German.Add("InstallModpackButtonDescription", "Wählen Sie die Mods aus, die Sie auf ihrem WoT CLient installieren möchten");
            Polish.Add("InstallModpackButtonDescription", "Zaznacz modyfikacje, które chcesz zainstalować w swoim kliencie WoT");
            French.Add("InstallModpackButtonDescription", "Sélectionnez les mods que vous souhaitez installer sur votre client WoT");
            Russian.Add("InstallModpackButtonDescription", "Выберите моды, которые вы хотите установить в клиент World of Tanks");

            //Component: UninstallModpackButton
            //
            English.Add("UninstallModpackButton", "Uninstall Relhax Modpack");
            German.Add("UninstallModpackButton", "Relhax Modpack deinstallieren");
            Polish.Add("UninstallModpackButton", "Odinstaluj Paczkę Relhax");
            French.Add("UninstallModpackButton", "Désinstaller Relhax Modpack");
            Russian.Add("UninstallModpackButton", "Удалить модпак Relhax");

            //Component: UninstallModpackButtonDescription
            //
            English.Add("UninstallModpackButtonDescription", "Remove *all* mods installed to your WoT client");
            German.Add("UninstallModpackButtonDescription", "*Alle* Mods entfernen, die auf deinem WoT-Client installiert sind");
            Polish.Add("UninstallModpackButtonDescription", "Usuń wszystkie zainstalowane modyfikacje do klienta WoT");
            French.Add("UninstallModpackButtonDescription", "Supprimer *tous* les mods installés sur votre client WoT");
            Russian.Add("UninstallModpackButtonDescription", "Удаление *всех* установленных в клиент WoT модификаций");

            //Component: ViewNewsButton
            //
            English.Add("ViewNewsButton", "View update news");
            German.Add("ViewNewsButton", "Aktualisierungsnachrichten anzeigen");
            Polish.Add("ViewNewsButton", "Zobacz wiadomości o aktualizacjach");
            French.Add("ViewNewsButton", "Voir les mises à jour");
            Russian.Add("ViewNewsButton", "Новости обновлений");

            //Component: ViewNewsButtonDescription
            //
            English.Add("ViewNewsButtonDescription", "View application, database, and other update news");
            German.Add("ViewNewsButtonDescription", "Anzeigen von Anwendungs-, Datenbank- und anderen Aktualisierungsnachrichten");
            Polish.Add("ViewNewsButtonDescription", "Zobacz aplikację, bazę danych i inne wiadomości o aktualizacjach");
            French.Add("ViewNewsButtonDescription", "Afficher les actualités sur l'applications, les bases de données et autres");
            Russian.Add("ViewNewsButtonDescription", "Показать новости об обновлениях приложения, БД и прочее");

            //Component: ForceManuelGameDetectionText
            //
            English.Add("ForceManuelGameDetectionText", "Force manual game detection");
            German.Add("ForceManuelGameDetectionText", "Erzwinge manuelle Spielerkennung");
            Polish.Add("ForceManuelGameDetectionText", "Wymuś ręczną weryfikację ścieżki gry");
            French.Add("ForceManuelGameDetectionText", "Forcer détection manuel");
            Russian.Add("ForceManuelGameDetectionText", "Принудительно указать папку с игрой");

            //Component: ForceManuelGameDetectionCBDescription
            //
            English.Add("ForceManuelGameDetectionCBDescription", "This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");
            German.Add("ForceManuelGameDetectionCBDescription", "Diese Option ist für die manuelle selektion des World of Tanks Spiel-" +
                    "speicherortes. Nutze dies wenn Du Probleme mit der automatischen Suche des Spiels hast.");
            Polish.Add("ForceManuelGameDetectionCBDescription", "Ta opcja wymusza ręczne znalezienie lokacji gry World of Tanks." +
                    "Zaznacz, jeśli masz problem z automatycznym znalezieniem ścieżki dostępu do gry.");
            French.Add("ForceManuelGameDetectionCBDescription", "Cette option consiste à forcer une détection manuel" +
                    "de World of Tanks. Sélectionnez cette option si vous rencontrez des problèmes pour localiser automatiquement le jeu.");
            Russian.Add("ForceManuelGameDetectionCBDescription", "Эта опция для принудительного указания папки с World of Tanks." +
                    "Поставьте галочку только в случае проблем с автоматическим определением расположения игры.");

            //Component: LanguageSelectionGBHeader
            //
            English.Add("LanguageSelectionGBHeader", "Language selection");
            German.Add("LanguageSelectionGBHeader", "Sprachauswahl");
            Polish.Add("LanguageSelectionGBHeader", "Wybór języka");
            French.Add("LanguageSelectionGBHeader", "Choix de langue");
            Russian.Add("LanguageSelectionGBHeader", "Выбрать язык");

            //Component: Forms_ENG_NAButtonDescription
            English.Add("Forms_ENG_NAButtonDescription", "Go to the English-speaking 'World of Tanks' forum page for the NA server");
            German.Add("Forms_ENG_NAButtonDescription", "Gehe zur englischsprachigen 'World of Tanks' Forum Seite für den NA Server");
            Polish.Add("Forms_ENG_NAButtonDescription", "Idź do anglojęzycznego forum 'World of Tanks' dla serwerów NA");
            French.Add("Forms_ENG_NAButtonDescription", "Accéder au forum anglophone 'World of Tanks' pour le serveur NA");
            Russian.Add("Forms_ENG_NAButtonDescription", "Перейти на страницу модпака на World of Tanks NA (страница на английском)");

            //Component: FormsENG_EUButtonDescription
            English.Add("Forms_ENG_EUButtonDescription", "Go to the English-speaking 'World of Tanks' forum page for the EU server");
            German.Add("Forms_ENG_EUButtonDescription", "Gehe zur englischsprachigen 'World of Tanks' Forum Seite für den EU Server");
            Polish.Add("Forms_ENG_EUButtonDescription", "Idź do anglojęzycznego forum 'World of Tanks' dla serwerów EU");
            French.Add("Forms_ENG_EUButtonDescription", "Accéder au forum anglophone 'World of Tanks' pour le serveur EU");
            Russian.Add("Forms_ENG_EUButtonDescription", "Перейти на страницу модпака на World of Tanks EU (страница на английском)");

            //Component: FormsENG_GERButtonDescription
            English.Add("Forms_GER_EUButtonDescription", "Go to the Gernam-speaking 'World of Tanks' forum page for the EU server");
            German.Add("Forms_GER_EUButtonDescription", "Gehe zur deutschsprachigen 'World of Tanks' Forum Seite für den EU Server");
            Polish.Add("Forms_GER_EUButtonDescription", "Idź do niemieckojęzycznego forum 'World of Tanks' dla serwerów NA");
            French.Add("Forms_GER_EUButtonDescription", "Allez sur la page du forum allemand 'World of Tanks' pour le serveur EU");
            Russian.Add("Forms_ENG_GERButtonDescription", "Перейти на страницу модпака на World of Tanks EU (страница на немецком)");

            //Component: SaveUserDataText
            //
            English.Add("SaveUserDataText", "Save user data");
            German.Add("SaveUserDataText", "Mod Daten speichern");
            Polish.Add("SaveUserDataText", "Zapisz ustawienia użytkownika");
            French.Add("SaveUserDataText", "Sauvegarder les données utilisateur");
            Russian.Add("SaveUserDataText", "Сохранить пользовательские данные");

            //Component:SaveUserDataCBDescription
            //
            English.Add("SaveUserDataCBDescription", "If this is selected, the installer will save user created data (like session stats from previous battles)");
            German.Add("SaveUserDataCBDescription", "Wenn dies ausgewählt ist, speichert das Installationsprogramm die vom Benutzer erstellten Daten" +
                " (wie Sitzungsstatistiken aus früheren Gefechten)");
            Polish.Add("SaveUserDataCBDescription", "Przy zaznaczeniu, instalator zachowa pliki danych użytkownika (takie jak statystyki sesji z poprzednich bitew)");
            French.Add("SaveUserDataCBDescription", "Si cette option est sélectionnée, l'installateur vas sauvegarder les données créé par l'utilisateur" +
                " (Comme les stats de la session des batailles précédentes");
            Russian.Add("SaveUserDataCBDescription", "Если выбрано, то установщик сохранит пользовательские данные (как сессионную статистику из предыдущих боев, к примеру)");


            //Component: CleanInstallText
            //
            English.Add("CleanInstallText", "Clean installation (recommended)");
            German.Add("CleanInstallText", "Saubere Installation (empfohlen)");
            Polish.Add("CleanInstallText", "Czysta instalacja (Zalecane)");
            French.Add("CleanInstallText", "Installation propre (Recommandé)");
            Russian.Add("CleanInstallText", "Чистая установка (рекомендуется)");

            //Component: CleanInstallCBDescription
            //
            English.Add("CleanInstallCBDescription", "This recommended option will uninstall your previous installation before installing the new one.");
            German.Add("CleanInstallCBDescription", "Diese empfohlene Option deinstalliert Ihre vorherige Installation, bevor Sie die neue installieren.");
            Polish.Add("CleanInstallCBDescription", "Ta rekomendowana opcja odinstaluje Twoje poprzednie instalacje przed zainstalowaniem nowych.");
            French.Add("CleanInstallCBDescription", "Cette option recommandée désinstallera votre installation précédente avant d'installer la nouvelle.");
            Russian.Add("CleanInstallCBDescription", TranslationNeeded);

            //Component: BackupModsText
            //
            English.Add("BackupModsText", "Backup current mods folder");
            German.Add("BackupModsText", "Sicherung des aktuellen Modsordner");
            Polish.Add("BackupModsText", "Zrób kopię zapasową obecnego pliku z modyfikacjami");
            French.Add("BackupModsText", "Sauvegarder le dossier de mods");
            Russian.Add("BackupModsText", "Сделать резервную копию папки с модификациями");

            //Component: BackupModsSizeLabelUsed
            //
            English.Add("BackupModsSizeLabelUsed", "Backups: {0}  Size: {1}");
            German.Add("BackupModsSizeLabelUsed", "Sicherungen: {0}  Größe: {1}");
            Polish.Add("BackupModsSizeLabelUsed", "Kopie zapasowe: {0}  Rozmiar: {1}");
            French.Add("BackupModsSizeLabelUsed", "Sauvegardes: {0} Taille: {1}");
            Russian.Add("BackupModsSizeLabelUsed", "Бэкапов: {0} Размер: {1}");

            //Component: BackupModsCBDescription
            //
            English.Add("BackupModsCBDescription", "Select this to make a backup of your current mods installation." +
                    "They are stored in the 'RelHaxModBackup' folder as a zip file, named by a time stamp.");
            German.Add("BackupModsCBDescription", "Wählen Sie diese Option, um ein Backup Ihrer aktuellen Mod-Installation zu erstellen. "+ 
                     "Sie werden im Ordner 'RelHaxModBackup' als ZIP-Datei mit einem Zeitstempel gespeichert.");
            Polish.Add("BackupModsCBDescription", "Wybierz tę opcję, aby utworzyć kopię zapasową bieżącej konfiguracji modyfikacji." +
                     "Będą one przechowywane w folderze „RelHaxModBackup” jako plik zip, nazwany znacznikiem czasowym. ");
            French.Add("BackupModsCBDescription", "Sélectionnez cette option pour effectuer une sauvegarde de votre installation de mods actuelle. "+
                     "Ils sont stockés dans le dossier 'RelHaxModBackup' en tant que fichier zip, nommé par un horodatage.");
            Russian.Add("BackupModsCBDescription", "Выберите для создания бэкапа имеющихся модов. Они будут находиться в папке 'RelHaxModBackup' в виде ZIP-архива и иметь в названии файла дату создания.");

            //Component: SaveLastInstallText
            //
            English.Add("SaveLastInstallText", "Save selection of last install");
            German.Add("SaveLastInstallText", "Speicherung der letzten Installation");
            Polish.Add("SaveLastInstallText", "Zapisz ostatnią konfigurację instalacji");
            French.Add("SaveLastInstallText", "Sauvegarder la denière configuration");
            Russian.Add("SaveLastInstallText", "Запомнить выбранные моды");

            //Component: SaveLastInstallCBDescription
            //
            English.Add("SaveLastInstallCBDescription", "When enabled, the installer will automatically apply your last used selection");
            German.Add("SaveLastInstallCBDescription", "Wenn diese Option aktiviert ist, wendet das Installationsprogramm automatisch Ihre zuletzt verwendete Auswahl an");
            Polish.Add("SaveLastInstallCBDescription", "Jeśli zaznaczone, instalator automatycznie zastosuje ostatnią użytą konfigurację");
            French.Add("SaveLastInstallCBDescription", "Lorsqu'il est activé, l'installateur appliquera automatiquement votre dernière sélection utilisée");
            Russian.Add("SaveLastInstallCBDescription", "Если выбрано, установщик автоматически применит шаблон из выбранных вами модов");

            //Component: MinimizeToSystemTrayText
            //
            English.Add("MinimizeToSystemTrayText", "Minimize to system tray");
            German.Add("MinimizeToSystemTrayText", "Ins Benachrichtigungsfeld minimieren");
            Polish.Add("MinimizeToSystemTrayText", "Zminimalizuj do ikony zasobnika");
            French.Add("MinimizeToSystemTrayText", "Réduire dans la barre d'état système");
            Russian.Add("MinimizeToSystemTrayText", "Свернуть в трей");

            //Component: MinimizeToSystemTrayDescription
            //
            English.Add("MinimizeToSystemTrayDescription", "When checked, the application will continue to run in the system tray when you press close");
            German.Add("MinimizeToSystemTrayDescription", "Wenn diese Option aktiviert ist, wird die Anwendung weiterhin im Benachtrichtigungsfeld ausgeführt, wenn Sie auf `Schließen` klicken");
            Polish.Add("MinimizeToSystemTrayDescription", "Jeśli zaznaczone, aplikacja będzie działać w tle po kliknięciu przycisku Zamknij");
            French.Add("MinimizeToSystemTrayDescription", "Lorsque cette case est cochée, l'application continue de s'exécuter dans la barre d'état système lorsque vous appuyez sur fermer");
            Russian.Add("MinimizeToSystemTrayDescription", "Если выбрано, то приложение продолжит работу в системе, когда вы закроете окно");

            //Component: VerboseLoggingText
            //
            English.Add("VerboseLoggingText", "Verbose Logging");
            German.Add("VerboseLoggingText", "Ausführliche Protokollierung");
            Polish.Add("VerboseLoggingText", "Pełne logowanie");
            French.Add("VerboseLoggingText", "Journalisation détaillé");
            Russian.Add("VerboseLoggingText", "Расширенное логгирование");

            //Component: VerboseLoggingCBDescription
            //
            English.Add("VerboseLoggingCBDescription", "Enable more logging messages in the log file. Useful for reporting bugs");
            German.Add("VerboseLoggingCBDescription", "Weitere Protokollmeldungen in der Protokolldatei aktivieren. Nützlich für das Melden von Fehlern.");
            Polish.Add("VerboseLoggingCBDescription", "Dołącz komunikaty logowania do pliku logów. Użyteczne podczas raportowanioa błędów");
            French.Add("VerboseLoggingCBDescription", "Activer plus de messages de journalisation dans le fichier journal. Utile pour signaler des bugs");
            Russian.Add("VerboseLoggingCBDescription", "Увеличить объём собираемых данных для файла отчёта. Полезно для багрепортов");

            //Component: AllowStatsGatherText
            //
            English.Add("AllowStatsGatherText", "Allow statistics gathering of mod usage");
            German.Add("AllowStatsGatherText", "Erlaube das Sammeln von Statistiken über die Verwendung von Mods");
            Polish.Add("AllowStatsGatherText", "Pozwól na gromadzenie statystyk dot. użycia modyfikacji");
            French.Add("AllowStatsGatherText", "Autoriser la collecte de statistiques sur l'utilisation des mods");
            Russian.Add("AllowStatsGatherText", "Разрешить сбор статистики о используемых модах");

            //Component: AllowStatsGatherCBDescription
            //
            English.Add("AllowStatsGatherCBDescription", "Allow the installer to upload anonymous statistic data to the server about mod selections. This allows us to prioritize our support");
            German.Add("AllowStatsGatherCBDescription", "Erlauben Sie dem Installer, anonyme Statistikdaten über die Mod-Auswahl auf den Server hochzuladen. Dies ermöglicht es uns, unseren Support zu priorisieren");
            Polish.Add("AllowStatsGatherCBDescription", "Pozwól instalatorowi przesłać anonimowe dane statystyczne o wyborze modyfikacji. Dzięki temu możemy lepiej określić kierunek naszego wsparcia");
            French.Add("AllowStatsGatherCBDescription", "Autorisez le programme d'installation à téléverser des données statistiques anonymes à nos serveur concernant les sélections de mods. Cela nous permet de prioritiser notre soutien");
            Russian.Add("AllowStatsGatherCBDescription", "Позволить установщику собирать анонимные статистические данные на основе выбранных модов. Это позволит нам расставить приоритеты по поддержке");

            //Component: DisableTriggersText
            //
            English.Add("DisableTriggersText", "Disable Triggers");
            German.Add("DisableTriggersText", "Trigger deaktivieren");
            Polish.Add("DisableTriggersText", "Wyłącz Wyzwalacze");
            French.Add("DisableTriggersText", "Désactiver les déclencheurs");
            Russian.Add("DisableTriggersText", "Отключить триггеры");

            //Component: DisableTriggersCBDescription
            //
            English.Add("DisableTriggersCBDescription", "Allowing triggers can speed up the installation by running some tasks (like creating contour icons) during extraction " + 
                "after all required resources for that task are ready. This is turned off automatically if User Mods are detected");
            German.Add("DisableTriggersCBDescription", "Das Zulassen von Triggern kann die Installation beschleunigen, indem einige Aufgaben ausgeführt werden (z. B. das Erstellen von Kontursymbolen), während extrahiert wird," +
                "sobald alle für diese Aufgabe erforderlichen Ressourcen verfügbar sind. Dies wird automatisch deaktiviert, wenn Benutzermodifikationen erkannt werden");
            Polish.Add("DisableTriggersCBDescription", "Włączenie Wyzwalaczy może przyspieszyć instalację, uruchamiając niektóre zadania (takie jak tworzenie kontur ikon) podczas wyodrębniania plików" +
                 "po tym, jak wszystkie wymagane czynności dla tego zadania zostaną skończone. Jest to wyłączane automatycznie, jeśli wykryte zostaną modyfikacje użytkownika");
            French.Add("DisableTriggersCBDescription", "Autoriser les déclencheurs peut accélérer l’installation en exécutant certaines tâches (comme la création d’icônes de contour) au cours de l’extraction "+
                 "une fois que toutes les ressources requises pour cette tâche sont prêtes. Ceci est automatiquement désactivé si des mods utilisateur sont détectés");
            Russian.Add("DisableTriggersCBDescription", "Включённые триггеры позволят ускорить установку, выполняя некоторые задачи (такие как создание контурных иконок) во время распаковки после того, как все необходимые ресурсы готовы для этого. По умолчанию триггеры выключены при обнаружении пользовательских модов");

            //Component: CancelDownloadInstallButton
            //
            English.Add("CancelDownloadInstallButton", "Cancel");
            German.Add("CancelDownloadInstallButton", "Abbrechen");
            Polish.Add("CancelDownloadInstallButton", "Anuluj");
            French.Add("CancelDownloadInstallButton", "Anuler");
            Russian.Add("CancelDownloadInstallButton", "Отмена");

            //Component: appDataFolderNotExistHeader
            //
            English.Add("appDataFolderNotExistHeader", "Could not detect WoT cache folder");
            German.Add("appDataFolderNotExistHeader", "Konnte den Cache-Ordner WoT nicht erkennen");
            Polish.Add("appDataFolderNotExistHeader", "Nie wykryto foldera WoT cache");
            French.Add("appDataFolderNotExistHeader", "Impossible de détecter le dossier de cache WoT");
            Russian.Add("appDataFolderNotExistHeader", "Невозможно найти папку кэша World of Tanks");


            //Component: appDataFolderNotExist
            //
            English.Add("appDataFolderNotExist", "The installer could not detect the WoT cache folder. Continue the installation without clearing WoT cache?");
            German.Add("appDataFolderNotExist", "Der Installer konnte den WoT-Cache-Ordner nicht erkennen. Installation fortsetzen ohne den WoT-Cache zu löschen?");
            Polish.Add("appDataFolderNotExist", "Instalato nie wykrył foldera cache. Czy kontynuować bez czyszczenia folderu cache?");
            French.Add("appDataFolderNotExist", "L'installateur n'as pas pus détecter le dossier de cache WoT. Continuer l'installation sans nettoyer le cache?");
            Russian.Add("appDataFolderNotExist", "Установщик не обнаружил папку кэша игры. Продолжить установку без очистки кэша?");

            //Component: viewAppUpdates
            //
            English.Add("viewAppUpdates", "View latest application updates");
            German.Add("viewAppUpdates", "Programmaktualisierungen anzeigen");
            Polish.Add("viewAppUpdates", "Pokaż ostatnie zmiany w aplikacji");
            French.Add("viewAppUpdates", "Afficher les dernières mises à jour de l'applications");
            Russian.Add("viewAppUpdates", "Посмотреть последние обновления приложения");

            //Component: viewDBUpdates
            //
            English.Add("viewDBUpdates", "View latest database updates");
            German.Add("viewDBUpdates", "Datenbankaktualisierungen anzeigen");
            Polish.Add("viewDBUpdates", "Pokaż ostatnie zmiany w bazie danych");
            French.Add("viewDBUpdates", "Afficher les dernières mises à jour de la base de données");
            Russian.Add("viewDBUpdates", "Посмотреть последние обновления базы данных");

            //Component: EnableColorChangeDefaultV2Text
            //
            English.Add("EnableColorChangeDefaultV2Text", "Enable color change");
            German.Add("EnableColorChangeDefaultV2Text", "Farbwechsel");
            Polish.Add("EnableColorChangeDefaultV2Text", "Włącz zmianę kolorów");
            French.Add("EnableColorChangeDefaultV2Text", "Activer les changements de couleurs");
            Russian.Add("EnableColorChangeDefaultV2Text", "Заменить цвета");

            //Component: EnableColorChangeDefaultV2CBDescription
            //
            English.Add("EnableColorChangeDefaultV2CBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");
            German.Add("EnableColorChangeDefaultV2CBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            Polish.Add("EnableColorChangeDefaultV2CBDescription", "Włącz zmianę kolorów podczas zmiany wyboru modyfikacji i ustawieñ.");
            French.Add("EnableColorChangeDefaultV2CBDescription", "Activer le changement de les couleurs lors de la selection d'un mod ou d'une config");
            Russian.Add("EnableColorChangeDefaultV2CBDescription", "Включить замену цветов при выборе мода или конфигурации");


            //Component: EnableColorChangeLegacyText
            //
            English.Add("EnableColorChangeLegacyText", "Enable color change");
            German.Add("EnableColorChangeLegacyText", "Farbwechsel");
            Polish.Add("EnableColorChangeLegacyText", "Włącz zmianę kolorów");
            French.Add("EnableColorChangeLegacyText", "Activer les changements de couleurs");
            Russian.Add("EnableColorChangeLegacyText", "Заменить цвета");

            //Component: EnableColorChangeLegacyCBDescription
            //
            English.Add("EnableColorChangeLegacyCBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");
            German.Add("EnableColorChangeLegacyCBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            Polish.Add("EnableColorChangeLegacyCBDescription", "Włącz zmianę kolorów podczas zmiany wyboru modyfikacji i ustawieñ.");
            French.Add("EnableColorChangeLegacyCBDescription", "Activer le changement de les couleurs lors de la selection d'un mod ou d'une config");
            Russian.Add("EnableColorChangeLegacyCBDescription", "Включить замену цветов при выборе мода или конфигурации");


            //Component: ClearLogFilesText
            //
            English.Add("ClearLogFilesText", "Clear log files");
            German.Add("ClearLogFilesText", "Protokolldatei löschen");
            Polish.Add("ClearLogFilesText", "Wyczyść logi");
            French.Add("ClearLogFilesText", "Effacer les fichiers logs");
            Russian.Add("ClearLogFilesText", "Очистить логи");

            //Component: ClearLogFilesCBDescription
            //
            English.Add("ClearLogFilesCBDescription", "Clear the WoT log files, (python.log), as well as xvm log files (xvm.log) and pmod log files (pmod.log)");
            German.Add("ClearLogFilesCBDescription", "Löschen der WoT Protokolldatei, sowie XVM und PMOD Protokolldatei");
            Polish.Add("ClearLogFilesCBDescription", "Wyczyść logi WoTa (python.log), XVM'a (xvm.log) i pmod'ów (pmod.log).");
            French.Add("ClearLogFilesCBDescription", "Effacez les fichiers logs WoT (python.log), ainsi que les fichiers logs xvm (xvm.log) et les fichiers logs pmod (pmod.log)");
            Russian.Add("ClearLogFilesCBDescription", "Очистка логов World of Tanks (python.log), XVM (xvm.log) и PMOD (pmod.log).");


            //Component: CreateShortcutsText
            //
            English.Add("CreateShortcutsText", "Create desktop shortcuts");
            German.Add("CreateShortcutsText", "Erstelle desktop verknüpfungen");
            Polish.Add("CreateShortcutsText", "Stwórz skróty na pulpicie");
            French.Add("CreateShortcutsText", "Créer des raccourcis sur le bureau");
            Russian.Add("CreateShortcutsText", "Создать ярлыки рабочего стола");

            //Component: CreateShortcutsCBDescription
            //
            English.Add("CreateShortcutsCBDescription", "When selected, it will create shortcut icons on your desktop for mods that are exe files (like WWIIHA configuration)");
            German.Add("CreateShortcutsCBDescription", "Wenn diese Option aktiviert ist, werden bei der Installation die Verknüpfungen für \"World of Tanks\"," +
                " \"World of Tanks launcher\" und, wenn bei der Installation aktiviert, auch andere Verknüpfungen zu Konfigurationsprogrammen erstellt (z.B. WWIIHA Konfiguration)");
            Polish.Add("CreateShortcutsCBDescription", "Kiedy zaznaczone, utworzone zostaną skróty na pulpicie do modyfikacji z plikami exe (np. konfiguracja WWIIHA)");
            French.Add("CreateShortcutsCBDescription", "Une fois sélectionné, L'installation créera des icônes de raccourci sur votre bureau pour les mods qui ont des" +
                " fichiers .exe (comme la configuration WWIIHA)");
            Russian.Add("CreateShortcutsCBDescription", "Если выбрано, то будут созданы ярлыки на рабочем столе для модов, являющимися EXE-файлами (как WWIIHA)");

            //Component: DeleteOldPackagesText
            //
            English.Add("DeleteOldPackagesText", "Delete old package files");
            German.Add("DeleteOldPackagesText", "Lösche alte Archiv-Dateien");
            Polish.Add("DeleteOldPackagesText", "Usuń stare pliki pakietowe");
            French.Add("DeleteOldPackagesText", "Supprimer les anciens fichiers de package");
            Russian.Add("DeleteOldPackagesText", "Удалить старые файлы пакетов");

            //Component: DeleteOldPackagesCBDescription
            //
            English.Add("DeleteOldPackagesCBDescription", "Delete any zip files that are no longer used by the installer in the \"RelhaxDownloads\" folder to free up disk space");
            German.Add("DeleteOldPackagesCBDescription", "Löschen Sie alle ZIP-Dateien im Ordner \"RelhaxDownloads\", welche vom Installationsprogramm nicht mehr verwendet werden, um Speicherplatz freizugeben.");
            Polish.Add("DeleteOldPackagesCBDescription", "Usuń nieużywane pliki zip w folderze „RelhaxDownloads”, aby zwolnić miejsce na dysku");
            French.Add("DeleteOldPackagesCBDescription", "Supprimez tous les fichiers zip qui ne sont plus utilisés par le programme d’installation dans le dossier \"RelhaxDownloads\" pour libérer de l’espace disque.");
            Russian.Add("DeleteOldPackagesCBDescription", "Удалять ZIP-архивы из папки \"RelhaxDownloads\", которые потеряли актуальность для установщика, с целью освобождения места на диске");

            //Component: AutoInstallText
            //
            English.Add("AutoInstallText", "Enable auto install (NEW)");
            German.Add("AutoInstallText", "Automatische Installation aktivieren (NEU)");
            Polish.Add("AutoInstallText", "Użyj automatycznej instalacji (NOWOŚĆ)");
            French.Add("AutoInstallText", "Activer l'installation automatique (NOUVEAU)");
            Russian.Add("AutoInstallText", "Включить автоустановку (НОВИНКА)");

            //Component: AutoInstallCBDescription
            //
            English.Add("AutoInstallCBDescription", "When a selection file and time is set below, the installer will automatically check for updates to your mods and apply them");
            German.Add("AutoInstallCBDescription", "Wenn unten eine Auswahldatei und eine Zeit eingestellt sind, sucht das Installationsprogramm automatisch nach Updates für Ihre Mods und wendet diese an");
            Polish.Add("AutoInstallCBDescription", "Jeśli zaznaczone, instalator automatycznie sprawdzi dostępność nowych modyfikacji i zastosuje je.");
            French.Add("AutoInstallCBDescription", "Lorsqu'un fichier et une heure de sélection sont définis ci-dessous, le programme d'installation vérifiera automatiquement les mises à jour de vos mods et les appliquera.");
            Russian.Add("AutoInstallCBDescription", "Установщик автоматически проверит наличие обновлений к модам в указанное время и применит их, основываясь на выбранной предустановке");

            //Component: OneClickInstallText
            //
            English.Add("OneClickInstallText", "Enable one-click install");
            German.Add("OneClickInstallText", "Aktivieren Sie die Ein-Klick-Installation");
            Polish.Add("OneClickInstallText", "Włącz instalację na kliknięcie");
            French.Add("OneClickInstallText", "Activer l'installation en un clic");
            Russian.Add("OneClickInstallText", "Включить установку в один клик");

            //Component: OneClickInstallCBDescription
            //
            English.Add("OneClickInstallCBDescription", "Enable the installer to automatically load your selection file and install it");
            German.Add("OneClickInstallCBDescription", "Aktivieren Sie diese Funktion, um Ihre Auswahldatei automatisch zu laden und zu installieren");
            Polish.Add("OneClickInstallCBDescription", "Automatycznie załaduj plik konfiguracji i zainstaluj go");
            French.Add("OneClickInstallCBDescription", "Activer le programme d'installation pour charger automatiquement votre fichier de sélection et l'installer");
            Russian.Add("OneClickInstallCBDescription", "Позволить установщику автоматически запустить установку модов сразу после выбора предустановки");

            //Component: ForceEnabledText
            //
            English.Add("ForceEnabledText", "Force all packages enabled [!]");
            German.Add("ForceEnabledText", "Erzwinge, dass alle Pakete aktiviert sind [!]");
            Polish.Add("ForceEnabledText", "Wymuś włącznie wszystkich pakietów [!]");
            French.Add("ForceEnabledText", "Forcer tous les paquets activés [!]");
            Russian.Add("ForceEnabledText", "Принудительно выбрать все пакеты [!]");

            //Component: ForceEnabledCBDescription
            //
            English.Add("ForceEnabledCBDescription", "Causes all packages to be enabled. Can lead to severe stability issues of your installation");
            German.Add("ForceEnabledCBDescription", "Bewirkt, dass alle Pakete aktiviert werden. Kann zu schwerwiegenden Stabilitätsproblemen Ihrer Installation führen");
            Polish.Add("ForceEnabledCBDescription", "Powoduje włączenie wszystkich pakietów. Może prowadzić do poważnych problemów ze stabilnością instalacji");
            French.Add("ForceEnabledCBDescription", "Fait en sorte que tous les paquets soient activés. Peut entraîner de graves problèmes de stabilité de votre installation");
            Russian.Add("ForceEnabledCBDescription", "Отмечает все доступные к установке пакеты. Может привести к серьёзным проблемам со стабильностью");

            //Component: ForceVisibleText
            //
            English.Add("ForceVisibleText", "Force all packages visible [!]");
            German.Add("ForceVisibleText", "Erzwinge, dass alle Pakete sichtbar sind [!]");
            Polish.Add("ForceVisibleText", "Wymuś widoczność wszystkich pakietów [!]");
            French.Add("ForceVisibleText", "Force tous les paquets visibles [!]");
            Russian.Add("ForceVisibleText", "Принудительно показать все пакеты [!]");

            //Component: ForceVisibleCBDescription
            //
            English.Add("ForceVisibleCBDescription", "Causes all packages to be visible. Can lead to severe stability issues of your installation");
            German.Add("ForceVisibleCBDescription", "Bewirkt, dass alle Pakete sichtbar sind. Kann zu schwerwiegenden Stabilitätsproblemen Ihrer Installation führen");
            Polish.Add("ForceVisibleCBDescription", "Powoduje, że wszystkie pakiety są widoczne. Może prowadzić do poważnych problemów ze stabilnością instalacji");
            French.Add("ForceVisibleCBDescription", "Fait en sorte que tous les paquets soient visibles. Peut entraîner de graves problèmes de stabilité de votre installation");
            Russian.Add("ForceVisibleCBDescription", "Показывает все скрытые пакеты. Может привести к серьёзным проблемам со стабильностью");

            //Component: LoadAutoSyncSelectionFileText
            //
            English.Add("LoadAutoSyncSelectionFileText", "Load selection file");
            German.Add("LoadAutoSyncSelectionFileText", "Auswahldatei laden");
            Polish.Add("LoadAutoSyncSelectionFileText", "Załaduj plik wyboru");
            French.Add("LoadAutoSyncSelectionFileText", "Charger le fichier de sélection");
            Russian.Add("LoadAutoSyncSelectionFileText", "Загрузить файл предустановки");

            //Component: DeveloperSettingsHeader
            //
            English.Add("DeveloperSettingsHeader", "Developer Settings [!]");
            German.Add("DeveloperSettingsHeader", "Entwickleroptionen [!]");
            Polish.Add("DeveloperSettingsHeader", "Ustawienia Twórców [!]");
            French.Add("DeveloperSettingsHeader", "Paramètres de développeur [!]");
            Russian.Add("DeveloperSettingsHeader", "Настройки для разработчиков [!]");

            //Component: ApplyCustomScalingText
            //
            English.Add("ApplyCustomScalingText", "Application Scaling");
            German.Add("ApplyCustomScalingText", "Anwendungsskalierung");
            Polish.Add("ApplyCustomScalingText", "Skalowanie Aplikacji");
            French.Add("ApplyCustomScalingText", "Mise à l'échelle de l'application");
            Russian.Add("ApplyCustomScalingText", "Мастшабирование приложения");

            //Component: LauchEditorText
            //button for launching the editor from the main application window
            English.Add("LauchEditorText", "Launch Database Editor");
            German.Add("LauchEditorText", "Starte Datenbank Editor");
            Polish.Add("LauchEditorText", "Uruchom edytor bazy danych");
            French.Add("LauchEditorText", "Lancer l'éditeur de base de données");
            Russian.Add("LauchEditorText", "Запустить редактор БД");

            //Component: LauchEditorDescription
            //button for launching the editor from the main application window
            English.Add("LauchEditorDescription", "Launch the Database Editor from here, instead of from command line");
            German.Add("LauchEditorDescription", "Dtarte den Datenbank Editor von hier, anstatt über die Befehlszeile");
            Polish.Add("LauchEditorDescription", "Uruchom editor baz danych stąd, zamiast z linii komend");
            French.Add("LauchEditorDescription", "Lancez l'éditeur de base de données à partir d'ici, au lieu de la ligne de commande");
            Russian.Add("LauchEditorDescription", "Запуск редактора базы данных непосредственно здесь, а не в коммандной строке");

            //Component: ApplyCustomScalingCBDescription
            //
            English.Add("ApplyCustomScalingCBDescription", "Apply display scaling to the installer windows");
            German.Add("ApplyCustomScalingCBDescription", "Wenden Sie die Anzeigeskalierung auf das Installationsfenster an");
            Polish.Add("ApplyCustomScalingCBDescription", "Zastosuj skalowanie do okien instalatora");
            French.Add("ApplyCustomScalingCBDescription", "Appliquer la mise à l'échelle de l'affichage aux fenêtres du programme d'installation");
            Russian.Add("ApplyCustomScalingCBDescription", "Применить масштабирование дисплея к окнам установщика");

            //Component: InstallWhileDownloadingText
            //
            English.Add("InstallWhileDownloadingText", "Extract while downloading");
            German.Add("InstallWhileDownloadingText", "Entpacke während des Downloads");
            Polish.Add("InstallWhileDownloadingText", "Wypakuj podczas ściągania");
            French.Add("InstallWhileDownloadingText", "Extraire lors du téléchargement");
            Russian.Add("InstallWhileDownloadingText", "Распаковка во время скачивания");

            //Component: InstallWhileDownloadingCBDescription
            //
            English.Add("InstallWhileDownloadingCBDescription", "When enabled, the installer will extract a zip file as soon as it is downloaded," +
                " rather than waiting for every zip file to be downloaded before extraction.");
            German.Add("InstallWhileDownloadingCBDescription", "Wenn aktiviert, der Installer wird die Zip-Dateien sofort nach dem Download entpacken" +
                " und nicht erst auf das Herunterladen aller Dateien warten bevor mit dem Entpacken begonnen wird.");
            Polish.Add("InstallWhileDownloadingCBDescription", "Wypakowywanie pobranych plików zip w tle podczas procesu ściągania paczek.");
            French.Add("InstallWhileDownloadingCBDescription", "Quand activé, l'installateur vas extraire un fichier zip dès qu'il est télécharger, au lieu" +
                " d'attendre que chaque fichier zip soit télécharger pour l'extraction.");
            Russian.Add("InstallWhileDownloadingCBDescription", "Если включено, то установщик будет распаковывать ZIP-архив сразу после скачивания, вместо того," +
                " чтобы ждать окончания загрузки всех файлов перед распаковкой.");

            //Component: MulticoreExtractionCoresCountLabel
            //
            English.Add("MulticoreExtractionCoresCountLabel", "Detected Cores: {0}");
            German.Add("MulticoreExtractionCoresCountLabel", "Erkannte Kerne: {0}");
            Polish.Add("MulticoreExtractionCoresCountLabel", "Wykryte Rdzenie: {0}");
            French.Add("MulticoreExtractionCoresCountLabel", "Cœurs détectés: {0}");
            Russian.Add("MulticoreExtractionCoresCountLabel", "Обнаружено ядер: {0}");

            //Component: MulticoreExtractionCoresCountLabelDescription
            //
            English.Add("MulticoreExtractionCoresCountLabelDescription", "Number of logical CPU cores (threads) detected on your system");
            German.Add("MulticoreExtractionCoresCountLabelDescription", "Anzahl der auf Ihrem System erkannten logischen CPU-Kerne (Threads)");
            Polish.Add("MulticoreExtractionCoresCountLabelDescription", "Liczba rdzeni logicznych procesora (wątków) wykrytych w systemie");
            French.Add("MulticoreExtractionCoresCountLabelDescription", "Nombre de cœurs de processeur logiques (threads) détectés sur votre système");
            Russian.Add("MulticoreExtractionCoresCountLabelDescription", "Количество логических процессоров (потоков), обнаруженных вашей системой");

            //Component: SaveDisabledModsInSelectionText
            //
            English.Add("SaveDisabledModsInSelectionText", "Keep disabled mods when saving selection");
            German.Add("SaveDisabledModsInSelectionText", "Behalten Sie deaktivierte Mods bei, wenn Sie die Auswahl speichern");
            Polish.Add("SaveDisabledModsInSelectionText", "Zachowaj wyłączone modyfikacje podczas zapisywania wyboru");
            French.Add("SaveDisabledModsInSelectionText", "Conserver les mods désactivés lors de l'enregistrement de la sélection");
            Russian.Add("SaveDisabledModsInSelectionText", "Запоминать отключённые моды при сохранении предустановки");

            //Component: SaveDisabledModsInSelectionDescription
            //
            English.Add("SaveDisabledModsInSelectionDescription", "When a mod is re-enabled, it will be selected from your selection file");
            German.Add("SaveDisabledModsInSelectionDescription", "Wenn ein Mod wieder aktiviert wird, wird er aus deiner Auswahldatei ausgewählt");
            Polish.Add("SaveDisabledModsInSelectionDescription", "Po ponownym włączeniu modyfikacja zostanie wybrana z pliku wyboru");
            French.Add("SaveDisabledModsInSelectionDescription", "Lorsqu'un mod est réactivé, il sera sélectionné dans votre fichier de sélection.");
            Russian.Add("SaveDisabledModsInSelectionDescription", "Когда мод будет включён в БД, он снова будет выбран из вашей предустановки");

            //Component: AdvancedInstallationProgressText
            //
            English.Add("AdvancedInstallationProgressText", "Show advanced installation progress window");
            German.Add("AdvancedInstallationProgressText", TranslationNeeded);
            Polish.Add("AdvancedInstallationProgressText", TranslationNeeded);
            French.Add("AdvancedInstallationProgressText", TranslationNeeded);
            Russian.Add("AdvancedInstallationProgressText", TranslationNeeded);

            //Component: AdvancedInstallationProgressDescription
            //
            English.Add("AdvancedInstallationProgressDescription", "Shows an advanced installation window during extraction, useful when you have multicore extraction enabled");
            German.Add("AdvancedInstallationProgressDescription", "Zeigt während der Extraktion ein erweitertes Installationsfenster an, das nützlich ist, wenn die Multicore-Extraktion aktiviert ist");
            Polish.Add("AdvancedInstallationProgressDescription", "Pokazuje zaawansowane okno instalacji podczas wyodrębniania, przydatne przy włączonej ekstrakcji wielordzeniowej");
            French.Add("AdvancedInstallationProgressDescription", "Affiche une fenêtre d'installation avancée pendant l'extraction, utile lorsque l'extraction multicœur est activée");
            Russian.Add("AdvancedInstallationProgressDescription", "Показывает более подробное окно прогресса установки. Полезно при включённой многопоточной установке");

            //Component: ThemeDefaultText
            //
            English.Add("ThemeDefaultText", "Default");
            German.Add("ThemeDefaultText", "Standard");
            Polish.Add("ThemeDefaultText", "Standardowy");
            French.Add("ThemeDefaultText", "Standard");
            Russian.Add("ThemeDefaultText", "Стандартная");

            //Component: ThemeDefaultDescriptionText
            //
            English.Add("ThemeDefaultDescriptionText", "Default Theme");
            German.Add("ThemeDefaultDescriptionText", "Standard Theme");
            Polish.Add("ThemeDefaultDescriptionText", "Domyślny Temat");
            French.Add("ThemeDefaultDescriptionText", "Thème par défaut");
            Russian.Add("ThemeDefaultDescriptionText", "Стандартная тема");

            //Component: ThemeDarkText
            //
            English.Add("ThemeDarkText", "Dark");
            German.Add("ThemeDarkText", "Dunkel");
            Polish.Add("ThemeDarkText", "Ciemny");
            French.Add("ThemeDarkText", "Sombre");
            Russian.Add("ThemeDarkText", "Тёмная");

            //Component: ThemeDarkDescription
            //
            English.Add("ThemeDarkDescription", "Dark Theme");
            German.Add("ThemeDarkDescription", "Dunkles Thema");
            Polish.Add("ThemeDarkDescription", "Ciemny Temat");
            French.Add("ThemeDarkDescription", "Thème sombre");
            Russian.Add("ThemeDarkDescription", "Тёмная тема");

            //Component: ThemeCustomText
            //
            English.Add("ThemeCustomText", "Custom");
            German.Add("ThemeCustomText", "Benutzerdefiniert");
            Polish.Add("ThemeCustomText", "Własny");
            French.Add("ThemeCustomText", "Personnalisé");
            Russian.Add("ThemeCustomText", "Сторонняя");

            //Component: ThemeCustomDescription
            //
            English.Add("ThemeCustomDescription", "Custom Theme");
            German.Add("ThemeCustomDescription", "Benutzerdefiniertes Thema");
            Polish.Add("ThemeCustomDescription", "Własny Temat");
            French.Add("ThemeCustomDescription", "Thème personnalisé");
            Russian.Add("ThemeCustomDescription", "Сторонняя тема");

            //Component: DumpColorSettingsButtonText
            //
            English.Add("DumpColorSettingsButtonText", "Dump color settings to xml");
            German.Add("DumpColorSettingsButtonText", "Speichere die Farbeinstellungen in xml");
            Polish.Add("DumpColorSettingsButtonText", "Zrzuć ustawienia kolorów do pliku xml");
            French.Add("DumpColorSettingsButtonText", "Dump des paramètres de couleur au format XML");
            Russian.Add("DumpColorSettingsButtonText", "Сохранить настройки цвета в XML");

            //Component: DumpColorSettingsButtonDescription
            //
            English.Add("DumpColorSettingsButtonDescription", "Writes an xml document of all components that can have a custom color applied, to make a custom theme");
            German.Add("DumpColorSettingsButtonDescription", "Schreibt ein XML-Dokument aller Komponenten, auf die eine benutzerdefinierte Farbe angewendet werden kann, um ein benutzerdefiniertes Thema anzupassen");
            Polish.Add("DumpColorSettingsButtonDescription", "Zapisuje dokument xml wszystkich komponentów, które mogą mieć niestandardowy kolor, aby utworzyć własny motyw");
            French.Add("DumpColorSettingsButtonDescription", "Écrit un document XML contenant tous les composants auxquels une couleur personnalisée peut être appliquée pour créer un thème personnalisé.");
            Russian.Add("DumpColorSettingsButtonDescription", "Создаёт XML-файл, в котором содержатся все параметры цветов для тех участков, где возможна замена цвета");

            //Component: MulticoreExtractionText
            //
            English.Add("MulticoreExtractionText", "Multicore extraction mode");
            German.Add("MulticoreExtractionText", "Mehrkern Extraktion");
            Polish.Add("MulticoreExtractionText", "Wsparcie wielu rdzeni");
            French.Add("MulticoreExtractionText", "Mode d'extraction multicoeur");
            Russian.Add("MulticoreExtractionText", "Многопроцессорный режим распаковки");

            //Component: MulticoreExtractionCBDescription
            //
            English.Add("MulticoreExtractionCBDescription", "The installer will use a parallel extraction method. It will extract multiple zip files at the same time," +
                " reducing install time. For SSD drives ONLY.");
            German.Add("MulticoreExtractionCBDescription", "Wird der Installer den parallelen Entpack-Modus verwenden. Er wird mehrere Zip-Dateien gleichzeitig entpacken" +
                " und dadurch die Installationszeit reduziert. Nur für SSD Festplatten.");
            Polish.Add("MulticoreExtractionCBDescription", "Metoda wypakowywania równoległego. Nastąpi wypakowywanie wielu plików zip jednocześnie, by skrócić czas instalacji." +
                " Jedynie dla dysków SSD.");// I always skip 'When enabled'...
            French.Add("MulticoreExtractionCBDescription", "Le programme d'installation utilise une méthode d'extraction parallèle. Il va extraire plusieurs fichiers" +
                " zip en même temps, réduisant ainsi le temps d'installation. Pour les disques SSD SEULEMENT.");
            Russian.Add("MulticoreExtractionCBDescription", "Tо установщик будет использовать метод параллельной распаковки. Будет извлекаться несколько ZIP-архивов одновременно," +
                " снижая время установки. ТОЛЬКО ДЛЯ SSD ДИСКОВ!");

            //Component: UninstallDefaultText
            //
            English.Add("UninstallDefaultText", "Default");
            German.Add("UninstallDefaultText", "Standard");
            Polish.Add("UninstallDefaultText", "Standardowa");
            French.Add("UninstallDefaultText", "Défaut");
            Russian.Add("UninstallDefaultText", "Стандартный");

            //Component: UninstallQuickText
            //
            English.Add("UninstallQuickText", "Quick");
            German.Add("UninstallQuickText", "Schnell");
            Polish.Add("UninstallQuickText", "Szybka");
            French.Add("UninstallQuickText", "Rapide");
            Russian.Add("UninstallQuickText", "Быстрый");

            //Component: ExportModeText
            //
            English.Add("ExportModeText", "Export Mode");
            German.Add("ExportModeText", "Export-Modus");
            Polish.Add("ExportModeText", "Tryb wyboru ścieżki wypakowywania");
            French.Add("ExportModeText", "Mode d'exportation");
            Russian.Add("ExportModeText", "Режим экспорта");

            //Component: ExportModeCBDescription
            //Explaining the export mode
            English.Add("ExportModeCBDescription", "Export mode will allow you to select a folder and WoT version you wish to export your mods installation to. For advanced users only." +
                "Note it will NOT: Unpack game xml files, patch files (provided from the game), or create the atlas files. Instructions can be found in the export directory.");
            German.Add("ExportModeCBDescription", "Der Export-Modus ermöglicht es Ihnen, einen Ordner und WoT-Version zu wählen, in die Sie Ihre Mods-Installation exportieren möchten." +
                " Nur für fortgeschrittene Benutzer. Bitte beachten: es werden KEINE Spiel-XML-Dateien entpackt und nicht modifiziert oder Atlas Dateien erstellt (jeweils aus dem Spiel" +
                " bereitgestellt). Anweisungen dazu finden Sie im Export-Verzeichnis.");
            Polish.Add("ExportModeCBDescription", "Tryb wyboru ścieżki wypakowania pozwala na wybór folderu i wersji WoT, do których chcesz zainstalować modyfikacje. Tylko dla zaawansowanych użytkowników." +
                " Tryb: nie rozpakuje plików gry xml, plików patchy (zapewnianych przez grę), oraz niestworzy plików atlasu. Instrukcje można znaleźć pod ścieżką wypakowania.");
            French.Add("ExportModeCBDescription", "Le mode Export vous permettra de sélectionner un dossier et la version de WoT vers lesquels vous souhaitez exporter votre installation" +
                " de mods. Pour les utilisateurs avancés, notez que l'installation ne fera pas: Déballez " +
                "les fichiers xml du jeu, corrigez les fichiers (fournis depuis le jeu) ou créez l'atlas. Les instructions peuvent être trouvées dans le répertoire d'exportation.");
            Russian.Add("ExportModeCBDescription", "Режим экспорта позволит выбрать папку для экспорта установленных модификаций в игру. Только для продвинутых пользователей." +
                "Учтите, что эта опция НЕ распакует XML-файлы игры, патчить их или создавать атласы. Инструкции находятся в папке экспорта.");

            //Component: ExportWindowDesctiption
            //
            English.Add("ExportWindowDesctiption", "Select the version of WoT you wish to export for");
            German.Add("ExportWindowDesctiption", "Wählen Sie die Version von WoT, für die Sie exportieren möchten");
            Polish.Add("ExportWindowDesctiption", "Wybór wersji WoT");
            French.Add("ExportWindowDesctiption", "Sélection de la version de WoT que vous souhaitez exporter");
            Russian.Add("ExportWindowDesctiption", "Выберите версию WoT, для которой нужно произвести экспорт");

            //Component: HelperText
            //
            English.Add("HelperText", "Welcome to the Relhax Modpack! I have tried to make the modpack as straight-forward as possible, but questions may still arise." +
                " Hover over a setting to have it explained. You won't see this dialog box again, unless you delete the settings xml file.");
            German.Add("HelperText", "Willkommen im Relhax Modpack! Ich habe versucht, das Modpack so einfach wie möglich zu gestalten, aber Fragen können dennoch entstehen." +
                " Rechtsklick auf eine Einstellung erklärt diese dann. Du siehst diese Dialogbox nicht mehr, ausser du löscht die xml Datei.");
            Polish.Add("HelperText", "Witamy w paczce Relhax! Próbowałem stworzyć jak najprostszą w użytku paczkę modyfikacji, ale wciąż możesz mieć pytania." +
                " Kliknik PPM na opcji, by wyświetlić opis. Nie zobaczysz tej wiadomości ponownie, dopóki nie usuniesz pliku ustawień xml.");
            French.Add("HelperText", "Bienvenue au Modpack Relhax! J'ai aissayé de faire le modpack le plus simple possible, mais des questions peuvent survenir." +
                " Survolez un paramètre pour voire une explication. Vous n'allez plus voire cette boite, sauf si vous supprimez le fichier de configuration xml ");
            Russian.Add("HelperText", "Добро пожаловать в модпак Relhax! Я пытался сделать модпак максимально простым для использования, но вопросы всё же могут возникнуть." +
                " Наведите курсор мыши на любую настройку, и вы увидите пояснение к ней. Данное диалоговое окно больше не появится, если не удалите XML-файл с настройками.");

            //Component: helperTextShort
            //
            English.Add("helperTextShort", "Welcome to the Relhax Modpack!");
            German.Add("helperTextShort", "Willkommen im Relhax Modpack!");
            Polish.Add("helperTextShort", "Witamy w paczce Relhax!");
            French.Add("helperTextShort", "Bienvenue au Modpack Relhax!");
            Russian.Add("helperTextShort", "Добро пожаловать в модпак Relhax!");

            //Component: NotifyIfSameDatabaseText
            //
            English.Add("NotifyIfSameDatabaseText", "Inform if no new database available");
            German.Add("NotifyIfSameDatabaseText", "Hinweis wenn keine Aktuallisierungen erfolgt sind");
            Polish.Add("NotifyIfSameDatabaseText", "Poinformuj, jeśli nie będzie dostępna nowa baza danych");
            French.Add("NotifyIfSameDatabaseText", "Informer si aucune nouvelle base de données est disponible");
            Russian.Add("NotifyIfSameDatabaseText", "Уведомлять при отстутсвии новых баз данных");

            //Component: NotifyIfSameDatabaseCBDescription
            //
            English.Add("NotifyIfSameDatabaseCBDescription", "Notify you if your last installed database version is the same. If so, it means that there is no update to any mods.");
            German.Add("NotifyIfSameDatabaseCBDescription", "Dich benachrichtigen: Die letzte verwendete Datenbank ist die selbe, d.h. es gibt keine Aktualisierungen und Veränderungen.");
            Polish.Add("NotifyIfSameDatabaseCBDescription", "Powiadom, jeśli ostatnia zainstalowana wersja bazy danych jest taka sama. Jeśli tak, to nie ma potrzeby aktualizacji modyfikacji.");
            French.Add("NotifyIfSameDatabaseCBDescription", "Avertir si votre dernière version de base de données installée est identique. Si c'est le cas," +
                " cela signifie qu'il n'y a pas de mise à jour de mods.");
            Russian.Add("NotifyIfSameDatabaseCBDescription", "Уведомлять в случае совпадения версий баз данных. Это означает отсутствие обновлений к каким-либо модам.");

            //Component: ShowInstallCompleteWindowText
            //
            English.Add("ShowInstallCompleteWindowText", "Show advanced install complete window");
            German.Add("ShowInstallCompleteWindowText", "Zeige erweitertes Fenster bei abgeschlossener Installation");
            Polish.Add("ShowInstallCompleteWindowText", "Pokaż zaawansowane okno po skończonej instalacji");
            French.Add("ShowInstallCompleteWindowText", "Afficher la fenêtre d'installation terminée");
            Russian.Add("ShowInstallCompleteWindowText", "Показывать расширенное окно окончания установки");

            //Component: ShowInstallCompleteWindowCBDescription
            //
            English.Add("ShowInstallCompleteWindowCBDescription", "Show a window upon installation completion with popular operations to" +
                " perform after modpack installation, such as launching the game, going to the xvm website, etc.");
            German.Add("ShowInstallCompleteWindowCBDescription", "Zeigte am Ende der Installation ein Auswahlfenster mit nützlichen Befehlen an," +
                " wie: starte das Spiel, gehe zur XVM Webseite, usw ...");
            Polish.Add("ShowInstallCompleteWindowCBDescription", "Po zakończeniu instalacji otwórz okno dalszych akcji.");
            French.Add("ShowInstallCompleteWindowCBDescription", "Afficher une fenêtre lors de l'achèvement de l'installation avec des opérations populaires à" +
                " effectuer après l'installation de Modpack, telles que le lancement du jeu, le site Web de XVM, etc.");
            Russian.Add("ShowInstallCompleteWindowCBDescription", "Показывать окно по окончании установки с частыми действиями после установки" +
                " модпака (запуск игры, открыть сайт XVM, и т. п.)");

            //Component: applicationVersion
            //
            English.Add("applicationVersion", "Application Version");
            German.Add("applicationVersion", "Programmversion");
            Polish.Add("applicationVersion", "Wersja Aplikacji");
            French.Add("applicationVersion", "Version de l'application");
            Russian.Add("applicationVersion", "Версия ПО");

            //Component: databaseVersion
            //
            English.Add("databaseVersion", "Latest Database");
            German.Add("databaseVersion", "Datenbank");
            Polish.Add("databaseVersion", "Baza danych");
            French.Add("databaseVersion", "Base de donnés");
            Russian.Add("databaseVersion", "Версия БД");

            //Component: ClearCacheText
            //
            English.Add("ClearCacheText", "Clear WoT cache data");
            German.Add("ClearCacheText", "Cache-Daten für WoT löschen");
            Polish.Add("ClearCacheText", "Usuń dane WoT cache");
            French.Add("ClearCacheText", "Nettoyer le dossier de Cache WoT");
            Russian.Add("ClearCacheText", "Очистить кэш World of Tanks");

            //Component: ClearCachCBDescription
            //
            English.Add("ClearCacheCBDescription", "Clear the WoT cache app data directory. Performs the same task as the similar option that was in OMC.");
            German.Add("ClearCacheCBDescription", "Löschen Sie das WoT-Cache-App-Datenverzeichnis. Führt die gleiche Aufgabe wie die ähnliche Option aus, die in OMC war.");
            Polish.Add("ClearCacheCBDescription", "Usuń dane aplikacji z lokacji WoT cache. Działa na podobnej zasadzie, jak kiedyś opcja z paczki OMC.");
            French.Add("ClearCacheCBDescription", "Nettoyer le dossier cache WoT. Effectue la même tâche que l'option similaire qui était dans OMC.");
            Russian.Add("ClearCacheCBDescription", "Очистить папку кэша World of Tanks. Операция аналогична соответствующей опции, присутствовавшей в OMC.");

            //Component: DefaultUninstallModeRBDescription
            //
            English.Add("DefaultUninstallModeRBDescription", "Default Uninstall will remove all files in the game's mod directories, including shortcuts and appdata cache files.");
            German.Add("DefaultUninstallModeRBDescription", "Die Standard Deinstallation wird alle Dateien in den Mod-Verzeichnissen des Spieles löschen, inklusive der" +
                " Verknüpfungen und Dateien im 'AppData' Speicher.");
            Polish.Add("DefaultUninstallModeRBDescription", "Domyślna deinstalacja usunie wszystkie pliki w folderze modyfikacji i pliki z nimi związane, włączając skróty" +
                " i pliki cache aplikacji.");
            French.Add("DefaultUninstallModeRBDescription", "La méthode de désinstallation par défaut vas supprimer tout les fichiers dans le dossier du jeu, incluant les" +
                " raccourcies et le fichers de cache appdata");
            Russian.Add("DefaultUninstallModeRBDescription", "Обычная деинсталляция удалит все моды, включая ярлыки и кэш в AppData.");

            //Component: CleanUninstallModeRBDescription
            //
            English.Add("CleanUninstallModeRBDescription", "Quick Uninstall will only remove files in the game's mod directories. It does not remove modpack" +
                " created shortcuts or appdata cache files.");
            German.Add("CleanUninstallModeRBDescription", "Die schnelle Deinstallation wird nur Dateien in den Mod-Verzeichnissen der Spieles löschen." +
                " Es werden keine vom ModPack erstellten Verknüpfungen oder Dateien im 'AppData' Speicher gelöscht.");
            Polish.Add("CleanUninstallModeRBDescription", "Szybka deinstalacja usunie tylko pliki w folderze modyfikacji. Nie usunie skrótów i plików cache związanych z modpackiem.");
            French.Add("CleanUninstallModeRBDescription", "La méthode de désinstallation rapide vas uniquement supprimer les fichiers dans le dossier" +
                " \"mod\" du jeu. Ceci ne supprimeras pas les raccourcis ou les fichiers de cache appdata créé par le modpack");
            Russian.Add("CleanUninstallModeRBDescription", "Быстрая деинсталляция удалит только моды, оставив ярлыки, созданные модпаком, и кэш в AppData.");

            //Component: DiagnosticUtilitiesButton
            //
            English.Add("DiagnosticUtilitiesButton", "Diagnostic utilities");
            German.Add("DiagnosticUtilitiesButton", "Diagnosedienstprogramme");
            Polish.Add("DiagnosticUtilitiesButton", "Narzędzia diagnostyczne");
            French.Add("DiagnosticUtilitiesButton", "Utilitaires de diagnostique");
            Russian.Add("DiagnosticUtilitiesButton", "Диагностика");

            //Component: DiagnosticUtilitiesButtonDescription
            //
            English.Add("DiagnosticUtilitiesButtonDescription", "Report a bug, attempt a WG client repair, etc.");
            German.Add("DiagnosticUtilitiesButtonDescription", "Fehler melden, versuche eine Client Reparatur, etc.");
            Polish.Add("DiagnosticUtilitiesButtonDescription", "Zgłoś błąd, spróbuj naprawić klienta WG, itp.");
            French.Add("DiagnosticUtilitiesButtonDescription", "Signaler un bug, tenter de réparer un client du groupe de travail, etc.");
            Russian.Add("DiagnosticUtilitiesButtonDescription", "Сообщить о баге, попытаться починить клиент, и т. д.");

            //Component: UninstallModeGroupBox
            //
            English.Add("UninstallModeGroupBox", "Uninstall Mode:");
            German.Add("UninstallModeGroupBox", "Deinstallationsmodus:");
            Polish.Add("UninstallModeGroupBox", "Tryb Deinstalacji:");
            French.Add("UninstallModeGroupBox", "Mode de désinstallation:");
            Russian.Add("UninstallModeGroupBox", "Режим деинсталляции: ");

            //Component: UninstallModeGroupBoxDescription
            English.Add("UninstallModeGroupBoxDescription", "Select the uninstall mode to use");
            German.Add("UninstallModeGroupBoxDescription", "Wähle den Deinstallationsmodus");
            Polish.Add("UninstallModeGroupBoxDescription", "Zaznacz tryb deinstalcji");
            French.Add("UninstallModeGroupBoxDescription", "Sélectionnez le mode de désinstallation à utiliser");
            Russian.Add("UninstallModeGroupBoxDescription", "Выбрать метод удаления");

            //Component: FacebookButtonDescription
            English.Add("FacebookButtonDescription", "Go to our Facebook page");
            German.Add("FacebookButtonDescription", "Unsere Facebook Seite aufrufen");
            Polish.Add("FacebookButtonDescription", "Strona FB");
            French.Add("FacebookButtonDescription", "Page Facebook");
            Russian.Add("FacebookButtonDescription", "Перейти на нашу страницу в Facebook");

            //Component: DiscordButtonDescription
            English.Add("DiscordButtonDescription", "Go to Discord server");
            German.Add("DiscordButtonDescription", "Zum Discord Server");
            Polish.Add("DiscordButtonDescription", "Serwer Discord");
            French.Add("DiscordButtonDescription", "Serveur Discord");
            Russian.Add("DiscordButtonDescription", "Перейти на наш сервер Discord");

            //Component: TwitterButtonDescription
            English.Add("TwitterButtonDescription", "Go to our Twitter page");
            German.Add("TwitterButtonDescription", "Unsere Twitter Seite aufrufen");
            Polish.Add("TwitterButtonDescription", "Strona Twitter");
            French.Add("TwitterButtonDescription", "Page Twitter");
            Russian.Add("TwitterButtonDescription", "Перейти на нашу страницу в Twitter");

            //Component: SendEmailButtonDescription
            English.Add("SendEmailButtonDescription", "Send us an Email (No modpack support)");
            German.Add("SendEmailButtonDescription", "Sende uns eine eMail (kein Modpack Support)");
            Polish.Add("SendEmailButtonDescription", "Wyślij maila do nas (Bez wsparcia)");
            French.Add("SendEmailButtonDescription", "Envoyez-nous un email (Pas de support modpack)");
            Russian.Add("SendEmailButtonDescription", "Отправить нам письмо на e-mail (Не для техподдержки)");

            //Component: HomepageButtonDescription
            English.Add("HomepageButtonDescription", "Visit our Website");
            German.Add("HomepageButtonDescription", "Zu unserer Homepage");
            Polish.Add("HomepageButtonDescription", "Odwiedz nasza strone");
            French.Add("HomepageButtonDescription", "Visiter notre site web");
            Russian.Add("HomepageButtonDescription", "Посетить наш веб-сайт");

            //Component: DonateButtonDescription
            English.Add("DonateButtonDescription", "Donation for further development");
            German.Add("DonateButtonDescription", "Spende für die Weiterentwicklung");
            Polish.Add("DonateButtonDescription", "Dotacja na dalszy rozwój");
            French.Add("DonateButtonDescription", "Donation pour aider au développement");
            Russian.Add("DonateButtonDescription", "Поддержать копеечкой для дальнейшей разработки");

            //Component: FindBugAddModButtonDescription
            English.Add("FindBugAddModButtonDescription", "Find a bug? Want a mod added? Report here please!");
            German.Add("FindBugAddModButtonDescription", "Fehler gefunden? Willst Du einen Mod hinzufügen? Bitte hier melden!");
            Polish.Add("FindBugAddModButtonDescription", "Znalazłeś błąd? Chcesz dodać mod?");
            French.Add("FindBugAddModButtonDescription", "Trouvé un bug? Recommandation de mod?");
            Russian.Add("FindBugAddModButtonDescription", "Нашли баг? Хотите добавить мод? Пишите сюда!");

            //Component: Mod Selection view Group Box
            //
            English.Add("SelectionViewGB", "Selection View");
            German.Add("SelectionViewGB", "Darstellungsart");
            Polish.Add("SelectionViewGB", "Widok wyborów");
            French.Add("SelectionViewGB", "Affichage de sélection");
            Russian.Add("SelectionViewGB", "Вид списка");

            //Component: SelectionDefaultText
            //Mod selection view default (relhax) [NOW WPF VERSION IS DEFAULT]
            English.Add("SelectionDefaultText", "Default");
            German.Add("SelectionDefaultText", "Standard");
            Polish.Add("SelectionDefaultText", "Domyślne");
            French.Add("SelectionDefaultText", "Normal");
            Russian.Add("SelectionDefaultText", "Стандартный");

            //Component: SelectionLegacyText
            //Mod selection view legacy (OMC)
            AddTranslationToAll("SelectionLegacyText", "OMC Legacy");

            //Componet: Mod selection Description
            //
            English.Add("SelectionLayoutDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            German.Add("SelectionLayoutDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionLayoutDescription", "Wybierz tryb widoku wyborów modyfikacji\nDomyślnie: Tryb widoku listy RelHax\nSpuścizna: Tryb widoku listy OMC");
            French.Add("SelectionLayoutDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");
            Russian.Add("SelectionLayoutDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");

            //Componet: Mod selection Description
            //
            English.Add("SelectionDefaultDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            German.Add("SelectionDefaultDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionDefaultDescription", "Wybierz tryb widoku wyborów modyfikacji\nDomyślnie: Tryb widoku listy RelHax\nSpuścizna: Tryb widoku listy OMC");
            French.Add("SelectionDefaultDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");
            Russian.Add("SelectionDefaultDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");

            //Componet: Mod selection Description
            //
            English.Add("SelectionLegacyDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            German.Add("SelectionLegacyDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionLegacyDescription", "Wybierz tryb widoku wyborów modyfikacji\nDomyślnie: Tryb widoku listy RelHax\nSpuścizna: Tryb widoku listy OMC");
            French.Add("SelectionLegacyDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");
            Russian.Add("SelectionLegacyDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");

            //Component: LanguageSelectionGBDescription
            //
            English.Add("LanguageSelectionGBDescription", "Select your preferred language.");
            German.Add("LanguageSelectionGBDescription", "wähle Deine bevorzugte Sprache");
            Polish.Add("LanguageSelectionGBDescription", "Wybierz preferowany język.");
            French.Add("LanguageSelectionGBDescription", "Sélectionnez votre langue préféré");
            Russian.Add("LanguageSelectionGBDescription", "Выберите желаемый язык");

            //Component: expandNodesDefault
            //
            English.Add("expandNodesDefault", "Expand all");
            German.Add("expandNodesDefault", "Alle erweitern");
            Polish.Add("expandNodesDefault", "Rozwiń wszystkie");
            French.Add("expandNodesDefault", "Développer tout");
            Russian.Add("expandNodesDefault", "Раскрыть все");

            //Component: expandNodesDefault2
            //
            English.Add("expandNodesDefault2", "Expand all");
            German.Add("expandNodesDefault2", "Alle erweitern");
            Polish.Add("expandNodesDefault2", "Rozwiń wszystkie");
            French.Add("expandNodesDefault2", "Développer tout");
            Russian.Add("expandNodesDefault2", "Раскрыть все");

            //Component: expandNodesDefaultDescription
            //
            English.Add("expandNodesDefaultDescription", "Select this to have all options automatically expand. It applies for the Legacy Selection only.");
            German.Add("expandNodesDefaultDescription", "Erweitere alle Einträge auf allen Registerkarten automatisch. Nur bei Ansicht als Baumstruktur.");
            Polish.Add("expandNodesDefaultDescription", "Zaznacz to, aby wszystkie opcje zostały automatycznie rozwinięte. Dotyczy tylko opcji Legacy Selection.");
            French.Add("expandNodesDefaultDescription", "Sélectionnez cette option pour que toutes les options s'élargis automatiquement. S'applique uniquement à la Sélection Legacy.");
            Russian.Add("expandNodesDefaultDescription", "Выберите этот пункт для автоматического раскрытия всех списков. Применимо только для legacy.");


            //Component: EnableBordersDefaultV2Text
            //
            English.Add("EnableBordersDefaultV2Text", "Enable borders");
            German.Add("EnableBordersDefaultV2Text", "Einrahmen");
            Polish.Add("EnableBordersDefaultV2Text", "Włącz granice");
            French.Add("EnableBordersDefaultV2Text", "Activer les bordures");
            Russian.Add("EnableBordersDefaultV2Text", "Включить границы");

            //Component: EnableBordersLegacyText
            //
            English.Add("EnableBordersLegacyText", "Enable borders");
            German.Add("EnableBordersLegacyText", "Einrahmen");
            Polish.Add("EnableBordersLegacyText", "Włącz granice");
            French.Add("EnableBordersLegacyText", "Activer les bordures");
            Russian.Add("EnableBordersLegacyText", "Включить границы");

            //Component: EnableBordersDefaultV2CBDescription
            //
            English.Add("EnableBordersDefaultV2CBDescription", "Enable the black borders around each mod and config sublevel.");
            German.Add("EnableBordersDefaultV2CBDescription", "Jede Auswahl schwarz einrahmen");
            Polish.Add("EnableBordersDefaultV2CBDescription", "Włącz czarne obramowanie modyfikacji i opcji konfiguracji");
            French.Add("EnableBordersDefaultV2CBDescription", "Activer les bordures noires autour de chaque mod et selections de configuration.");
            Russian.Add("EnableBordersDefaultV2CBDescription", "Включить показ чёрных рамок вокруг наименования каждого мода и конфигурации.");


            //Component: EnableBordersLegacyCBDescription
            //
            English.Add("EnableBordersLegacyCBDescription", "Enable the black borders around each mod and config sublevel.");
            German.Add("EnableBordersLegacyCBDescription", "Jede Auswahl schwarz einrahmen");
            Polish.Add("EnableBordersLegacyCBDescription", "Włącz czarne obramowanie modyfikacji i opcji konfiguracji");
            French.Add("EnableBordersLegacyCBDescription", "Activer les bordures noires autour de chaque mod et selections de configuration.");
            Russian.Add("EnableBordersLegacyCBDescription", "Включить показ чёрных рамок вокруг наименования каждого мода и конфигурации.");


            //Component: UseBetaDatabaseText
            English.Add("UseBetaDatabaseText", "Use beta database");
            German.Add("UseBetaDatabaseText", "Die Beta-Version der Datenbank verwenden");
            Polish.Add("UseBetaDatabaseText", "Użyj wersji beta bazy danych");
            French.Add("UseBetaDatabaseText", "Utiliser la base de données beta");
            Russian.Add("UseBetaDatabaseText", "Использовать бета-версию БД");

            //Component: UseBetaDatabaseCBDescription
            English.Add("UseBetaDatabaseCBDescription", "Use the latest beta database. Mod stability is not guaranteed");
            German.Add("UseBetaDatabaseCBDescription", "Verwende die letzte Beta-Version des ModPack Datenbank. Die Stabilität der Mods kann nicht garantiert werden, jedoch werden hier auch Fehlerbehebungen als erstes getestet und implementiert.");
            Polish.Add("UseBetaDatabaseCBDescription", "Użyj ostatniej wersji beta bazy danych. Nie gwarantujemy stabilności modyfikacji.");
            French.Add("UseBetaDatabaseCBDescription", "Utiliser la dernière base de données beta. La stabilité des mods n'est pas garantie");
            Russian.Add("UseBetaDatabaseCBDescription", "Использовать последнюю доступную бета-версию БД. Стабильность модов не гарантирована.");


            //Component: UseBetaApplicationText
            English.Add("UseBetaApplicationText", "Use beta application");
            German.Add("UseBetaApplicationText", "Die Beta-Version des ModPack Managers verwenden");
            Polish.Add("UseBetaApplicationText", "Użyj wersji beta bazy aplikacji");
            French.Add("UseBetaApplicationText", "Utiliser l'application beta");
            Russian.Add("UseBetaApplicationText", "Использовать бета-версию программы");

            //Component: UseBetaApplicationCBDescription
            English.Add("UseBetaApplicationCBDescription", "Use the latest beta application. Translations and application stability are not guaranteed");
            German.Add("UseBetaApplicationCBDescription", "Verwende die letzte Beta-Version des ModPack Managers. Fehlerfreie Übersetzungen und Programmstabilität können nicht garantiert werden.");
            Polish.Add("UseBetaApplicationCBDescription", "Użyj ostatniej wersji beta aplikacji. Nie gwarantujemy stabilności ani tłumaczenia aplikacji.");
            French.Add("UseBetaApplicationCBDescription", "Utiliser la dernière version beta. Les traductions et la stabilité de l'application ne sont pas garanties");
            Russian.Add("UseBetaApplicationCBDescription", "Использовать последнюю доступную бета-версию программы. Корректность перевода и стабильность приложения не гарантированы.");


            //Component: SettingsTabIntroHeader
            //
            English.Add("SettingsTabIntroHeader", "Welcome!");
            German.Add("SettingsTabIntroHeader", "Willkommen");
            Polish.Add("SettingsTabIntroHeader", "Witamy!");
            French.Add("SettingsTabIntroHeader", "Bienvenue!");
            Russian.Add("SettingsTabIntroHeader", "Добро пожаловать!");

            //Component: SettingsTabSelectionViewHeader
            //
            English.Add("SettingsTabSelectionViewHeader", "Selection View");
            German.Add("SettingsTabSelectionViewHeader", "Auswahlansicht");
            Polish.Add("SettingsTabSelectionViewHeader", "Wybór Widoku");
            French.Add("SettingsTabSelectionViewHeader", "Vue de sélection");
            Russian.Add("SettingsTabSelectionViewHeader", "Выбор вида списка");

            //Component: SettingsTabInstallationSettingsHeader
            //
            English.Add("SettingsTabInstallationSettingsHeader", "Installation Settings");
            German.Add("SettingsTabInstallationSettingsHeader", "Installationseinstellungen");
            Polish.Add("SettingsTabInstallationSettingsHeader", "Ustawienia Instalacji");
            French.Add("SettingsTabInstallationSettingsHeader", "Paramètres d'installation");
            Russian.Add("SettingsTabInstallationSettingsHeader", "Параметры установки");

            //Component: SettingsTabApplicationSettingsHeader
            //
            English.Add("SettingsTabApplicationSettingsHeader", "Application Settings");
            German.Add("SettingsTabApplicationSettingsHeader", "Programmeinstellungen");
            Polish.Add("SettingsTabApplicationSettingsHeader", "Ustawienia Aplikacji");
            French.Add("SettingsTabApplicationSettingsHeader", "Paramètres de l'application");
            Russian.Add("SettingsTabApplicationSettingsHeader", "Параметры приложения");

            //Component: SettingsTabAdvancedSettingsHeader
            //
            English.Add("SettingsTabAdvancedSettingsHeader", "Advanced");
            German.Add("SettingsTabAdvancedSettingsHeader", "Erweitert");
            Polish.Add("SettingsTabAdvancedSettingsHeader", "Zaawansowane");
            French.Add("SettingsTabAdvancedSettingsHeader", "Avancé");
            Russian.Add("SettingsTabAdvancedSettingsHeader", "Расширенные настройки");

            //Component: MainWindowSelectSelectionFileToLoad
            //
            English.Add("MainWindowSelectSelectionFileToLoad", "Select selection file to load");
            German.Add("MainWindowSelectSelectionFileToLoad", "Wählen Sie die zu ladende Auswahldatei");
            Polish.Add("MainWindowSelectSelectionFileToLoad", "Zaznacz plik wyboru do załadowania");
            French.Add("MainWindowSelectSelectionFileToLoad", "Sélectionner le fichier de sélection à charger");
            Russian.Add("MainWindowSelectSelectionFileToLoad", "Выберите предустановку для загрузки");

            //Componet: verifyUninstallHeader
            //
            English.Add("verifyUninstallHeader", "Confirmation");
            German.Add("verifyUninstallHeader", "Bestätigung");
            Polish.Add("verifyUninstallHeader", "Potwierdź");
            French.Add("verifyUninstallHeader", "Confirmation");
            Russian.Add("verifyUninstallHeader", "Подтверждение");

            //Componet: verifyUninstallVersionAndLocation
            //
            English.Add("verifyUninstallVersionAndLocation", "Confirm you wish to uninstall mods from WoT installation\n\n{0}\n\nUsing uninstall method '{1}'?");
            German.Add("verifyUninstallVersionAndLocation", "Bitte bestätige das du alle Mods von Deinem WoT deinstalliert haben möchtest\n\n{0}\n\nVerwendung von Deinstallationsmethode '{1}'?");
            Polish.Add("verifyUninstallVersionAndLocation", "Potwierdź usunięcie modyfikacji\n\n{0}\n\nPotwierdź metodę '{1}'");
            French.Add("verifyUninstallVersionAndLocation", "Confirmer que vous voulez désinstaller les mods du dossier WoT\n\n{0}\n\nUsing la méthode de désinstallation '{1}'?");
            Russian.Add("verifyUninstallVersionAndLocation", "Подтвердите необходимость удалить моды для WoT в этой папке: \n\n{0}\n\nИспользуем метод '{1}'?");

            //Component: failedVerifyFolderStructure
            //When the application first starts, it tries to open a logfile
            English.Add("failedVerifyFolderStructure", "The application failed to create the required folder structure. Check your file permissions or move the application to a folder with write access.");
            German.Add("failedVerifyFolderStructure", TranslationNeeded);
            Polish.Add("failedVerifyFolderStructure", TranslationNeeded);
            French.Add("failedVerifyFolderStructure", TranslationNeeded);
            Russian.Add("failedVerifyFolderStructure", TranslationNeeded);

            //Component: failedToExtractUpdateArchive
            //Upon update download, if it can't extract the exe
            English.Add("failedToExtractUpdateArchive", "The application failed to extract the update files. Check your file permissions and antivirus application.");
            German.Add("failedToExtractUpdateArchive", TranslationNeeded);
            Polish.Add("failedToExtractUpdateArchive", TranslationNeeded);
            French.Add("failedToExtractUpdateArchive", TranslationNeeded);
            Russian.Add("failedToExtractUpdateArchive", TranslationNeeded);

            //Component: downloadingUpdate
            //
            English.Add("downloadingUpdate", "Downloading application update");
            German.Add("downloadingUpdate", TranslationNeeded);
            Polish.Add("downloadingUpdate", TranslationNeeded);
            French.Add("downloadingUpdate", TranslationNeeded);
            Russian.Add("downloadingUpdate", TranslationNeeded);

            //Component: autoOneclickSelectionFileNotExist
            //
            English.Add("autoOneclickSelectionFileNotExist", "The supplied path to the selection file does not exist");
            German.Add("autoOneclickSelectionFileNotExist", TranslationNeeded);
            Polish.Add("autoOneclickSelectionFileNotExist", TranslationNeeded);
            French.Add("autoOneclickSelectionFileNotExist", TranslationNeeded);
            Russian.Add("autoOneclickSelectionFileNotExist", TranslationNeeded);

            //Component: noAutoInstallWithBeta
            //
            English.Add("noAutoInstallWithBeta", "Auto install mode cannot be used with the beta database");
            German.Add("noAutoInstallWithBeta", TranslationNeeded);
            Polish.Add("noAutoInstallWithBeta", TranslationNeeded);
            French.Add("noAutoInstallWithBeta", TranslationNeeded);
            Russian.Add("noAutoInstallWithBeta", TranslationNeeded);

            //Component: ColorDumpSaveFileDialog
            //
            English.Add("ColorDumpSaveFileDialog", "Select where to save the colors customization file");
            German.Add("ColorDumpSaveFileDialog", TranslationNeeded);
            Polish.Add("ColorDumpSaveFileDialog", TranslationNeeded);
            French.Add("ColorDumpSaveFileDialog", TranslationNeeded);
            Russian.Add("ColorDumpSaveFileDialog", TranslationNeeded);

            //Component: loadingBranches
            //"branch" is this context is git respoitory branches
            English.Add("loadingBranches", "Loading branches");
            German.Add("loadingBranches", TranslationNeeded);
            Polish.Add("loadingBranches", TranslationNeeded);
            French.Add("loadingBranches", TranslationNeeded);
            Russian.Add("loadingBranches", TranslationNeeded);
            #endregion

            #region ModSelectionList
            //Component: ContinueButtonLabel
            //
            English.Add("ContinueButtonLabel", "Install");
            German.Add("ContinueButtonLabel", "Installieren");
            Polish.Add("ContinueButtonLabel", "Zainstaluj");
            French.Add("ContinueButtonLabel", "Installer");
            Russian.Add("ContinueButtonLabel", "Установить");

            //Component: CancelButtonLabel
            //
            English.Add("CancelButtonLabel", "Cancel");
            German.Add("CancelButtonLabel", "Abbrechen");
            Polish.Add("CancelButtonLabel", "Anuluj");
            French.Add("CancelButtonLabel", "Anuler");
            Russian.Add("CancelButtonLabel", "Отмена");

            //Component: HelpLabel
            //
            English.Add("HelpLabel", "Right-click a selection component to see a preview window");
            German.Add("HelpLabel", "Klicken Sie mit der rechten Maustaste auf eine Auswahlkomponente, um ein Vorschaufenster anzuzeigen");
            Polish.Add("HelpLabel", "PPM, aby wyświetlić opis");
            French.Add("HelpLabel", "Cliquez avec le bouton droit sur un composant de sélection pour afficher une fenêtre d'aperçu");
            Russian.Add("HelpLabel", "Клик правой кнопкой мыши по компоненту покажет превью");

            //Component: LoadSelectionButtonLabel
            //
            English.Add("LoadSelectionButtonLabel", "Load selection");
            German.Add("LoadSelectionButtonLabel", "Auswahl laden");
            Polish.Add("LoadSelectionButtonLabel", "Wczytaj konfigurację z pliku");
            French.Add("LoadSelectionButtonLabel", "Charger une configuration");
            Russian.Add("LoadSelectionButtonLabel", "Загрузить шаблон настроек");

            //Component: SaveSelectionButtonLabel
            //
            English.Add("SaveSelectionButtonLabel", "Save selection");
            German.Add("SaveSelectionButtonLabel", "Auswahl speichern");
            Polish.Add("SaveSelectionButtonLabel", "Zapisz konfigurację w pliku");
            French.Add("SaveSelectionButtonLabel", "Sauvegarder une configuration");
            Russian.Add("SaveSelectionButtonLabel", "Сохранить шаблон настроек");

            //Component: ClearSelectionsButtonLabel
            //
            English.Add("ClearSelectionsButtonLabel", "Clear selections");
            German.Add("ClearSelectionsButtonLabel", "Auswahl löschen");
            Polish.Add("ClearSelectionsButtonLabel", "Wyczyść wybór");
            French.Add("ClearSelectionsButtonLabel", "Réinitialiser la sélection");
            Russian.Add("ClearSelectionsButtonLabel", "Снять все галочки");

            //Component: SearchThisTabOnlyCB
            //
            English.Add("SearchThisTabOnlyCB", "This tab only");
            German.Add("SearchThisTabOnlyCB", "Nur diese Registerkarte");
            Polish.Add("SearchThisTabOnlyCB", "Tylko ta zakładka");
            French.Add("SearchThisTabOnlyCB", "Cet onglet seulement");
            Russian.Add("SearchThisTabOnlyCB", "Только в этой вкладке");

            //Component: SearchTB
            //
            English.Add("SearchTB", "Search for a mod...");
            German.Add("SearchTB", "Suche einen Mod ...");
            Polish.Add("SearchTB", "Szukaj modyfikacji… ");
            French.Add("SearchTB", "Rechercher un mod ...");
            Russian.Add("SearchTB", "Найти мод...");

            //Component: SeachTBDescription
            //
            English.Add("SeachTBDescription", "You can also search for multiple name parts, separated by a * (asterisk).\nFor example: config*willster419 will display" +
                " as search result: Willster419\'s Config");
            German.Add("SeachTBDescription", "Du kannst auch nach mehreren Namensteilen suchen, getrennt durch ein * (Sternchen).\nZum Beispiel: config*willster419" +
                "  wird als Suchergebnis anzeigen: Willster419\'s Config");
            Polish.Add("SeachTBDescription", "Można również wyszukiwać wiele części nazw oddzielonych gwiazdką (*).\nNa przykład: config * willster419 wyświetli się" +
                " jako wynik wyszukiwania: Willster419's Config");
            French.Add("SeachTBDescription", "Vous pouvez également rechercher plusieurs parties de nom, séparées par un * (astérisque).\nPar exemple: config *" +
                " willster419 affichera comme résultat de la recherche: Config de Willster419");
            Russian.Add("SeachTBDescription", "Вы так же можете искать по нескольким частям названия, разделяя их * (звёздочкой).\nК примеру, config*willster419 покажет в качестве результата поиска Willster419\'s Config");

            //Component: InstallingAsWoTVersion
            //
            English.Add("InstallingAsWoTVersion", "Installing as WoT version: {0}");
            German.Add("InstallingAsWoTVersion", "Installation als WoT Version: {0}");
            Polish.Add("InstallingAsWoTVersion", "Instalacja do wersji WoT: {0}");
            French.Add("InstallingAsWoTVersion", "Installation en tant que version WoT: {0}");
            Russian.Add("InstallingAsWoTVersion", "Установка в клиент WoT версии {0}");

            //Component: userMods
            //
            English.Add("userMods", "User Mods");
            German.Add("userMods", "Benutzermodifikationen");
            Polish.Add("userMods", "Modyfikacje Użytkownika");
            French.Add("userMods", "Mods utilisateur");
            Russian.Add("userMods", "Пользовательские моды");

            //Component: FirstTimeUserModsWarning
            //
            English.Add("FirstTimeUserModsWarning", "This tab is for selecting zip files you place in the \"RelhaxUserMods\" folder. They must be zip files, and should use a root directory folder of the \"World_of_Tanks\" directory");
            German.Add("FirstTimeUserModsWarning", "Auf dieser Registerkarte können Sie ZIP-Dateien auswählen, die Sie im Ordner \"RelhaxUserMods\" ablegen. Sie müssen Zip-Dateien sein und sollten einen Stammverzeichnisordner des Verzeichnisses \"World_of_Tanks\" verwenden");
            Polish.Add("FirstTimeUserModsWarning", TranslationNeeded);
            French.Add("FirstTimeUserModsWarning", "Cet onglet sert à sélectionner les fichiers zip que vous placez dans le dossier \"RelhaxUserMods \". Il doit s'agir de fichiers zip et utiliser un dossier dans le dossier racine du répertoire \"World_of_Tanks \"");
            Russian.Add("FirstTimeUserModsWarning", "Данная вкладка предназначена для выбора модов, расположенных в папке \"RelhaxUserMods\". Они должны быть в виде ZIP-архивов и использовать корневую папку World of Tanks.");

            //Component: downloadingDatabase
            //
            English.Add("downloadingDatabase", "Downloading database");
            German.Add("downloadingDatabase", "Datenbank herunterladen");
            Polish.Add("downloadingDatabase", "Ściąganie baz danych");
            French.Add("downloadingDatabase", "Téléchargement de la base de données");
            Russian.Add("downloadingDatabase", "Загружается база данных");

            //Component: readingDatabase
            //
            English.Add("readingDatabase", "Reading database");
            German.Add("readingDatabase", "Lese datenbank");
            Polish.Add("readingDatabase", "Wczytywanie baz danych");
            French.Add("readingDatabase", "Chargement de la base de données");
            Russian.Add("readingDatabase", "Читается база данных");

            //Component: loadingUI
            //
            English.Add("loadingUI", "Loading UI");
            German.Add("loadingUI", "Lade Benutzerinterface");
            Polish.Add("loadingUI", "Ładowanie interfejsu");
            French.Add("loadingUI", "Chargement de l'interface utilisateur");
            Russian.Add("loadingUI", "Загрузка интерфейса");

            //Component: verifyingDownloadCache
            //
            English.Add("verifyingDownloadCache", "Verifying file integrity of ");
            German.Add("verifyingDownloadCache", "Überprüfen der Dateiintegrität von ");
            Polish.Add("verifyingDownloadCache", "Sprawdzanie integralności plików: ");
            French.Add("verifyingDownloadCache", "Vérification de l'intégrité du fichier ");
            Russian.Add("verifyingDownloadCache", "Проверяется целостность файла ");

            //Component: InstallProgressTextBoxDescription
            //
            English.Add("InstallProgressTextBoxDescription", "Progress of an installation will be shown here");
            German.Add("InstallProgressTextBoxDescription", "Der Fortschritt einer Installation wird hier angezeigt");
            Polish.Add("InstallProgressTextBoxDescription", "Postęp instalacji będzie wyświetlony tutaj");
            French.Add("InstallProgressTextBoxDescription", "Le progrès d'une installation sera affichée ici");
            Russian.Add("InstallProgressTextBoxDescription", "Прогресс текущей установки будет показан здесь");

            //Component: testModeDatabaseNotFound
            //
            English.Add("testModeDatabaseNotFound", "CRITICAL: TestMode Database not found at:\n{0}");
            German.Add("testModeDatabaseNotFound", "KRITISCH: Die Datanbank für den Testmodus wurde nicht gefunden:\n{0}");
            Polish.Add("testModeDatabaseNotFound", "BŁĄD KRYTYCZNY: Baza danych Trybu Testowego nie znaleziona w lokalizacji:\n{0}");
            French.Add("testModeDatabaseNotFound", "CRITIQUE: Impossible de trouver la base de données du mode de test situé a: \n{0}");
            Russian.Add("testModeDatabaseNotFound", "КРИТИЧЕСКАЯ ОШИБКА: Тестовая БД не найдена по адресу:\n{0}");


            //Component: 
            //
            English.Add("duplicateMods", "CRITICAL: Duplicate package ID detected");
            German.Add("duplicateMods", "KRITISCH: Doppelte Paket-ID erkannt");
            Polish.Add("duplicateMods", "BŁĄD KRYTYCZNY: Wykryto zduplikowany identyfikator pakietu");
            French.Add("duplicateMods", "CRITIQUE: ID de package en double détecté");
            Russian.Add("duplicateMods", "КРИТИЧЕСКАЯ ОШИБКА: Обнаружен дубликат пакета с таким же ID");


            //Component: 
            //
            English.Add("databaseReadFailed", "CRITICAL: Failed to read database\n\nsee Logfile for detailed info");
            German.Add("databaseReadFailed", "KRITISCH: Datenbank konnte nicht gelesen werden\n\nin der Protokolldatei stehen weitere Informationen zu diesem Fehler");
            Polish.Add("databaseReadFailed", "BŁĄD KRYTYCZNY: Nie udało się wczytać bazy danych");
            French.Add("databaseReadFailed", "CRITIQUE: Impossible de lire la base de données");
            Russian.Add("databaseReadFailed", "КРИТИЧЕСКАЯ ОШИБКА: Ошибка чтения базы данных!\nПодробности в log-файле");


            //Component: 
            //
            English.Add("configSaveSuccess", "Selection Saved Successfully");
            German.Add("configSaveSuccess", "Auswahl erfolgreich gespeichert");
            Polish.Add("configSaveSuccess", "Konfiguracja  zapisany pomyślnie");
            French.Add("configSaveSuccess", "Sélection enregistrée avec succès");
            Russian.Add("configSaveSuccess", "Предустановка успешно сохранена");

            //Component: 
            //
            English.Add("selectConfigFile", "Find a selection file to load");
            German.Add("selectConfigFile", "Eine zu ladende Auswahldatei finden");
            Polish.Add("selectConfigFile", "Znajdź plik konfiguracji do załadowania");
            French.Add("selectConfigFile", "Trouver un fichier de sélection à charger");
            Russian.Add("selectConfigFile", "Найти файл с предустановкой для загрузки");

            //Component: 
            //
            English.Add("configLoadFailed", "The selection file could not be loaded, loading in standard mode");
            German.Add("configLoadFailed", "Die Auswahldatei konnte nicht geladen werden und wurde im Standardmodus geladen");
            Polish.Add("configLoadFailed", "Nie można załadować pliku konfiguracji, otwieranie w trybie standardowym");
            French.Add("configLoadFailed", "Le fichier de sélection n'a pas pu être chargé, chargement en mode standard");
            Russian.Add("configLoadFailed", "Файл предустановки не может быть загружен, работа будет продолжена в обычном режиме");

            //Component: 
            //
            English.Add("modNotFound", "The package (ID = \"{0}\") was not found in the database. It could have been renamed or removed.");
            German.Add("modNotFound", "Das Paket (ID = \"{0}\") wurde nicht in der Datenbank gefunden. Es könnte umbenannt oder entfernt worden sein");
            Polish.Add("modNotFound", "Pakiet (ID = „{0}”) nie został znaleziony w bazie danych. Mógł zostać zmieniony lub usunięty.");
            French.Add("modNotFound", "Le paquet (ID = \"{0}\") n'a pas été trouvé dans la base de données. Il aurait pu être renommé ou supprimé.");
            Russian.Add("modNotFound", "Пакет (ID = \"{0}\") не был найден в базе данных. Он мог быть переименован или удалён.");

            //Component: 
            //
            English.Add("modDeactivated", "The package (ID = \"{0}\") is currently deactivated in the modpack and could not to be selected to install.");
            German.Add("modDeactivated", "Das Paket (ID = \"{0}\") ist vorrübergehend deaktiviert und kann nicht für die Installation ausgewählt werden");
            Polish.Add("modDeactivated", "Pakiet (ID = „{0}”) jest obecnie nieaktywny w modpacku i nie można go wybrać do instalacji.");
            French.Add("modDeactivated", "Le paquet (ID = \"{0}\") est actuellement désactivé dans le modpack et n'a pas pu être sélectionné pour l'installation.");
            Russian.Add("modDeactivated", "Пакет (ID= \"{0}\") в настоящее время отключён и недоступен для выбора и установки в модпаке.");

            //Component: 
            //
            English.Add("modsNotFoundTechnical", "The following mods could not be found and were most likely removed. There are only technical names available:\n{0}");
            German.Add("modsNotFoundTechnical", "Die folgenden Modifikationen können nicht gefunden werden und wurden wahrscheinlich entfernt/gelöscht. Es sind leider nur technische Namen verfügbar:\n{0}");
            Polish.Add("modsNotFoundTechnical", "Następujące modyfikacje nie zostały znalezione, najprawdopodobniej zostały usunięte:\n{0}");
            French.Add("modsNotFoundTechnical", "Les mods suivants n'ont pas pu être trouvés et ont probablement été supprimés. Il n'y a que des noms techniques disponibles:\n{0}");
            Russian.Add("modsNotFoundTechnical", "Данные моды не найдены, вероятно они были удалены. Есть только их техническое название:\n{0}");


            //Component: 
            //
            English.Add("modsBrokenStructure", "The following mods were disabled due to modifications in the package structure. You need to re-check them if you want to install them:\n");
            German.Add("modsBrokenStructure", "Die folgenden Mods wurden aufgrund von Änderungen in der Paketstruktur deaktiviert. Sie müssen sie erneut auswählen, wenn Sie sie installieren möchten:\n");
            Polish.Add("modsBrokenStructure", "Następujące modyfikacje zostały wyłączone ze względu na zmiany w strukturze paczki. Zaznacz je ponownie, jeśli chcesz je zainstalować:\n");
            French.Add("modsBrokenStructure", "Les mods suivants ont été désactivés en raison de modifications dans la structure du paquet: vous devez les vérifier de nouveau si vous voulez les installer:\n");
            Russian.Add("modsBrokenStructure", "Данные моды были отключены в связи с изменением структуры пакета. Вы можете перепроверить их в случае, если хотите установить их:\n");


            //Component: 
            //
            English.Add("oldSavedConfigFile", "The saved preferences file your are using is in an outdated format and will be inaccurate in the future. Convert it to the new format?" +
                " (A backup of the old format will be made)");
            German.Add("oldSavedConfigFile", "Die Konfigurationsdatei die benutzt wurde, wird in Zukunft immer ungenauer werden. Soll auf das neue Standardformat umgestellt werden?" +
                " (Eine Sicherung des alten Formats erfolgt)");
            Polish.Add("oldSavedConfigFile", "Zapisana konfiguracja jest w przestarzałym formacie i może powodować nieścisłości. Czy chcesz przekonwertować ją na nowszy zapis?");
            French.Add("oldSavedConfigFile", "Le fichier de préférences que vous avez choisis est un format obsolète et seras inexact dans le future. Convertire au nouveau format?");
            Russian.Add("oldSavedConfigFile", "Сохранённый файл конфигурации использует устаревший формат и может некорректно работать в будущем. Хотите преобразовать его в новый? Бэкап старого будет так же сохранён.");


            //Component: 
            //
            English.Add("prefrencesSet", "Preferences Set");
            German.Add("prefrencesSet", "Bevorzugte Einstellungen");
            Polish.Add("prefrencesSet", "Preferowane Ustawienia");
            French.Add("prefrencesSet", "Préférences Enregistrées");
            Russian.Add("prefrencesSet", "Настройки применены");

            //Component: 
            //
            English.Add("selectionsCleared", "Selections Cleared");
            German.Add("selectionsCleared", "Auswahl gelöscht");
            Polish.Add("selectionsCleared", "Usunięto Zaznaczenia");
            French.Add("selectionsCleared", "Sélections effacées");
            Russian.Add("selectionsCleared", "Список выбранных модов очищен");

            //Component: failedLoadSelection
            //
            English.Add("failedLoadSelection", "Failed to load selection");
            German.Add("failedLoadSelection", TranslationNeeded);
            Polish.Add("failedLoadSelection", TranslationNeeded);
            French.Add("failedLoadSelection", TranslationNeeded);
            Russian.Add("failedLoadSelection", TranslationNeeded);

            //Component: unknownselectionFileFormat
            //
            English.Add("unknownselectionFileFormat", "Unknown selection file version");
            German.Add("unknownselectionFileFormat", TranslationNeeded);
            Polish.Add("unknownselectionFileFormat", TranslationNeeded);
            French.Add("unknownselectionFileFormat", TranslationNeeded);
            Russian.Add("unknownselectionFileFormat", TranslationNeeded);

            //Component: ExpandAllButton
            //
            English.Add("ExpandAllButton", "Expand Current Tab");
            German.Add("ExpandAllButton", "Erweitere alle Einträge der aktuellen Registerkarte");
            Polish.Add("ExpandAllButton", "Rozwiń bieżącą kartę");
            French.Add("ExpandAllButton", "Elargir l'onglet");
            Russian.Add("ExpandAllButton", "Раскрыть текущую вкладку");

            //Component: CollapseAllButton
            //
            English.Add("CollapseAllButton", "Collapse Current Tab");
            German.Add("CollapseAllButton", "Reduziere alle Einträge der aktuellen Registerkarte");
            Polish.Add("CollapseAllButton", "Zwiń bieżącą kartę");
            French.Add("CollapseAllButton", "Réduire l'onglet");
            Russian.Add("CollapseAllButton", "Свернуть текущую вкладку");

            //Component: InstallingTo
            //
            English.Add("InstallingTo", "Installing to: {0}");
            German.Add("InstallingTo", "Installiere nach: {0}");
            Polish.Add("InstallingTo", "Instalowanie w: {0}");
            French.Add("InstallingTo", "Installation à: {0}");
            Russian.Add("InstallingTo", "Установка в {0}");

            //Component: saveConfig
            //
            English.Add("selectWhereToSave", "Select where to save selection file");
            German.Add("selectWhereToSave", "Wählen Sie aus, wo die Auswahldatei gespeichert werden soll");
            Polish.Add("selectWhereToSave", "Wybierz, gdzie zapisać plik konfiguracji");
            French.Add("selectWhereToSave", "Sélectionner où sauvegarder le fichier de sélection");

            //Component: updated
            //shows (updated) next to a component
            English.Add("updated", "updated");
            German.Add("updated", "aktualisiert");
            Polish.Add("updated", "zaktualizowane");
            French.Add("updated", "Mis à jours");
            Russian.Add("updated", "обновлено");

            //Component: disabled
            //shows (disabled) next to a component
            English.Add("disabled", "disabled");
            German.Add("disabled", TranslationNeeded);
            Polish.Add("disabled", TranslationNeeded);
            French.Add("disabled", TranslationNeeded);
            Russian.Add("disabled", TranslationNeeded);

            //Component: invisible
            //shows (invisible) next to a component
            English.Add("invisible", "invisible");
            German.Add("invisible", TranslationNeeded);
            Polish.Add("invisible", TranslationNeeded);
            French.Add("invisible", TranslationNeeded);
            Russian.Add("invisible", TranslationNeeded);
            #endregion

            #region Application Update Window
            //Componet: VersionInfoYesButton
            //
            English.Add("VersionInfoYesButton", "yes");
            German.Add("VersionInfoYesButton", "ja");
            Polish.Add("VersionInfoYesButton", "Tak");
            French.Add("VersionInfoYesButton", "Oui");
            Russian.Add("VersionInfoYesButton", "Да");

            //Componet: VersionInfoNoButton
            //
            English.Add("VersionInfoNoButton", "no");
            German.Add("VersionInfoNoButton", "nein");
            Polish.Add("VersionInfoNoButton", "Nie");
            French.Add("VersionInfoNoButton", "Non");
            Russian.Add("VersionInfoNoButton", "Нет");

            //Componet: NewVersionAvailable
            //
            English.Add("NewVersionAvailable", "New version available");
            German.Add("NewVersionAvailable", "Neue version verfügbar");
            Polish.Add("NewVersionAvailable", "Dostępna nowa wersja");
            French.Add("NewVersionAvailable", "Nouvelle version disponible");
            Russian.Add("NewVersionAvailable", "Доступна новая версия");

            //Componet: HavingProblemsTextBlock
            //
            English.Add("HavingProblemsTextBlock", "If you are having problems updating, please");
            German.Add("HavingProblemsTextBlock", "Wenn Sie Probleme mit der Aktualisierung haben, bitte");
            Polish.Add("HavingProblemsTextBlock", "Jeśli masz problemy z aktualizają proszę");
            French.Add("HavingProblemsTextBlock", "Si vous avez des problèmes avec la mise à jour, s'il vous plaît");
            Russian.Add("HavingProblemsTextBlock", "При наличии проблем в процессе обновлений, пожалуйста,");

            //Componet: ManualUpdateLink
            //
            English.Add("ManualUpdateLink", "Click Here");
            German.Add("ManualUpdateLink", "Klick Hier");
            Polish.Add("ManualUpdateLink", "Kliknij Tutaj");
            French.Add("ManualUpdateLink", "Cliquez Ici");
            Russian.Add("ManualUpdateLink", "Hажмите Cюда.");

            //Component: loadingApplicationUpdateNotes
            //
            English.Add("loadingApplicationUpdateNotes", "Loading application update notes...");
            German.Add("loadingApplicationUpdateNotes", "Anwendungsaktualisierungsnotizen werden geladen...");
            Polish.Add("loadingApplicationUpdateNotes", "Ładowanie historii zmian aplikacji...");
            French.Add("loadingApplicationUpdateNotes", "Chargement des notes de mise à jour de l'application ...");
            Russian.Add("loadingApplicationUpdateNotes", "Загружается список изменений в обновлении...");
            #endregion

            #region Installer Messages
            //Component: 
            //
            English.Add("Downloading", "Downloading");
            German.Add("Downloading", "Wird heruntergeladen");
            Polish.Add("Downloading", "Pobieranie");
            French.Add("Downloading", "Téléchargement");
            Russian.Add("Downloading", "Идёт скачивание");

            //Component: seconds
            //
            English.Add("seconds", "seconds");
            German.Add("seconds", "Sekunden");
            Polish.Add("seconds", "sekund");
            French.Add("seconds", "secondes");
            Russian.Add("seconds", "сек.");

            //Component: 
            //
            English.Add("patching", "Patching");
            German.Add("patching", "Patching");
            Polish.Add("patching", "Patchowanie");
            French.Add("patching", "Patching");
            Russian.Add("patching", "Идёт процесс патчинга");

            //Component: 
            //
            English.Add("done", "Done");
            German.Add("done", "Fertig");
            Polish.Add("done", "Ukończono");
            French.Add("done", "Terminé");
            Russian.Add("done", "Готово");

            //Component: 
            //
            English.Add("cleanUp", "Clean up resources");
            German.Add("cleanUp", "Bereinige Ressourcen");
            Polish.Add("cleanUp", "Oczyszczanie zasobów");
            French.Add("cleanUp", "Nettoyer les ressources");
            Russian.Add("cleanUp", "Очистка ресурсов");

            //Component: 
            //
            English.Add("idle", "Idle");
            German.Add("idle", "Leerlauf");
            Polish.Add("idle", "Bezczynny");
            French.Add("idle", "En attente");
            Russian.Add("idle", "Ожидание");

            //Component: 
            //
            English.Add("status", "Status:");
            German.Add("status", "Status:");
            Polish.Add("status", "Stan:");
            French.Add("status", "Statut");
            Russian.Add("status", "Состояние:");

            //Component: 
            //
            English.Add("canceled", "Canceled");
            German.Add("canceled", "Abgebrochen");
            Polish.Add("canceled", "Anulowano");
            French.Add("canceled", "Anulé");
            Russian.Add("canceled", "Отменено");

            //Component: 
            //
            English.Add("appSingleInstance", "Checking for single instance");
            German.Add("appSingleInstance", "Prüfung auf Einzelinstanz");
            Polish.Add("appSingleInstance", "Sprawdzanie ");
            French.Add("appSingleInstance", "Vérification d'instance unique");
            Russian.Add("appSingleInstance", "Проверяется наличие только одного процесса");

            //Component: 
            //
            English.Add("checkForUpdates", "Checking for updates");
            German.Add("checkForUpdates", "Auf Updates prüfen");
            Polish.Add("checkForUpdates", "Sprawdzanie aktualizacji");
            French.Add("checkForUpdates", "Vérification de mise à jours");
            Russian.Add("checkForUpdates", "Проверяется наличие обновлений");

            //Component: 
            //
            English.Add("verDirStructure", "Verifying directory structure");
            German.Add("verDirStructure", "Verifizierung der Verzeichnisstruktur");
            Polish.Add("verDirStructure", "Sprawdzanie struktury dostępu");
            French.Add("verDirStructure", "Vérification de la structure du directoire");
            Russian.Add("verDirStructure", "Проверяется структура каталогов");

            //Component: 
            //
            English.Add("loadingSettings", "Loading settings");
            German.Add("loadingSettings", "Einstellungen laden");
            Polish.Add("loadingSettings", "Ładowanie ustawnień");
            French.Add("loadingSettings", "Chargement des paramètres");
            Russian.Add("loadingSettings", "Загружаются настройки");

            //Component: 
            //
            English.Add("loadingTranslations", "Loading translations");
            German.Add("loadingTranslations", "Laden der Übersetzungen");
            Polish.Add("loadingTranslations", "ładowanie tłumaczenia");
            French.Add("loadingTranslations", "Chargement des traductions");
            Russian.Add("loadingTranslations", "Загружаются переводы на другие языки");

            //Component: 
            //
            English.Add("loading", "Loading");
            German.Add("loading", "Laden");
            Polish.Add("loading", "Ładowanie");
            French.Add("loading", "Chargement de");
            Russian.Add("loading", "Загрузка");

            //Component: 
            //
            English.Add("of", "of");
            German.Add("of", "von");
            Polish.Add("of", "z");
            French.Add("of", "de");
            Russian.Add("of", "из");

            //Component: failedToDownload1
            //
            English.Add("failedToDownload1", "Failed to download the package");
            German.Add("failedToDownload1", TranslationNeeded);
            Polish.Add("failedToDownload1", TranslationNeeded);
            French.Add("failedToDownload1", TranslationNeeded);
            Russian.Add("failedToDownload1", TranslationNeeded);

            //Component: failedToDownload2
            //
            English.Add("failedToDownload2", "Would you like to retry the download, abort the installation, or continue?");
            German.Add("failedToDownload2", TranslationNeeded);
            Polish.Add("failedToDownload2", TranslationNeeded);
            French.Add("failedToDownload2", TranslationNeeded);
            Russian.Add("failedToDownload2", TranslationNeeded);

            //Component: 
            //
            English.Add("failedToDownloadHeader", "Failed to download");
            German.Add("failedToDownloadHeader", "Fehler beim Herunterladen");
            Polish.Add("failedToDownloadHeader", "Ściąganie zakończone niepowodzeniem, plik");
            French.Add("failedToDownloadHeader", "Échec de téléchargement");
            Russian.Add("failedToDownloadHeader", "Сбой загрузки");

            //Component: update check against online app version
            //
            English.Add("failedManager_version", "Current Beta application is outdated and must be updated against stable channel. No new Beta version online now.");
            German.Add("failedManager_version", "Die aktuelle Beta-Anwendung ist veraltet und muss für einen stabilen Kanal aktualisiert werden. Derzeit ist keine neue Beta-Version online");
            Polish.Add("failedManager_version", "Aktualna wersja Beta aplikacji jest przestarzała i musi zostać zaktualizowana do wersji stabilnej. Brak nowej wersji beta w trybie online.");
            French.Add("failedManager_version", "L’application bêta actuelle est obsolète et doit être mise à jour avec le canal stable. Aucune nouvelle version bêta en ligne pour le moment.");
            Russian.Add("failedManager_version", "Данная бета-версия не актуальна и должна быть обновлена через стабильный канал. Новых бета-версий в данный момент нет.");

            //Component: 
            //
            English.Add("fontsPromptInstallHeader", "Admin to install fonts?");
            German.Add("fontsPromptInstallHeader", "Admin zum Installieren von Schriftarten?");
            Polish.Add("fontsPromptInstallHeader", "Uprawnienia administratora, by zainstalować czcionki?");
            French.Add("fontsPromptInstallHeader", "Admin pour installer les polices?");
            Russian.Add("fontsPromptInstallHeader", "Права администратора для установки шрифтов?");

            //Component: 
            //
            English.Add("fontsPromptInstallText", "Do you have admin rights to install fonts?");
            German.Add("fontsPromptInstallText", "Hast Du Admin-Rechte um Schriftarten zu installieren?");
            Polish.Add("fontsPromptInstallText", "Czy masz uprawnienia administratora zainstalować czcionki?");
            French.Add("fontsPromptInstallText", "Avez-vous les droits d'administrateur installer des polices?");
            Russian.Add("fontsPromptInstallText", "У вас есть права администратора, необходимые для установки шрифтов?");

            //Component: 
            //
            English.Add("fontsPromptError_1", "Unable to install fonts. Some mods may not work properly. Fonts are located in ");
            German.Add("fontsPromptError_1", "Schriftarten können nicht installiert werden. Einige Mods funktionieren möglicherweise nicht richtig. Schriften befinden sich in ");
            Polish.Add("fontsPromptError_1", "Niepowodzenie przy instalacji czcionek. Niektóre modyfikacje mogą nie działać prawidłowo. Czcionki znajdują się w ");
            French.Add("fontsPromptError_1", "Impossible d'installer les polices. Certain mods peut mal fonctionner. Les polices sont situé dans ");
            Russian.Add("fontsPromptError_1", "Не удалось установить шрифты. Некоторые моды могут работать некорректно. Шрифты расположены в ");

            //Component: 
            //
            English.Add("fontsPromptError_2", "\\_fonts. Either install them yourself or run this again as Administrator.");
            German.Add("fontsPromptError_2", "\\_fonts. Entweder installiere sie selbst oder führe der Relhax Manager erneut als Administrator aus.");
            Polish.Add("fontsPromptError_2", "\\_fonts. Albo zainstalujesz je własnoręcznie, albo uruchom jako administrator.");
            French.Add("fontsPromptError_2", "\\_fonts. Installez les polices manuellement ou redémarrez avec les droits Administrateur");
            Russian.Add("fontsPromptError_2", "\\_fonts. Попробуйте установить их самостоятельно или перезапустите программу от имени администратора.");

            //Component: 
            //
            English.Add("cantDownloadNewVersion", "Unable to download new version, exiting.");
            German.Add("cantDownloadNewVersion", "Die neue Version kann nicht heruntergeladen werden.");
            Polish.Add("cantDownloadNewVersion", "Niepowodzenie przy pobieraniu nowej wersji.");
            French.Add("cantDownloadNewVersion", "Échec du téléchargement des mise à jours. Fermeture.");
            Russian.Add("cantDownloadNewVersion", "Невозможно загрузить новую версию, приложение будет закрыто.");

            //Component: 
            //
            English.Add("failedCreateUpdateBat", "Unable to create update process.\n\nPlease manually delete the file:\n{0}\n\nrename file:\n{1}\nto:\n{2}\n\nDirectly jump to the folder?");
            German.Add("failedCreateUpdateBat", "Updateprozess kann leider nicht erstellt werden.\n\nLösche bitte diese Datei von Hand:\n{0}\n\nbenennte diese" +
                " Datei:\n{1}\nin diese um:\n{2}\n\nDirekt zum Ordner springen?");
            Polish.Add("failedCreateUpdateBat", "Nie można zaktualizować.\n\nProszę ręcznie usunąć plik:\n{0}\n\nrzmienić nazwę pliku:\n{1}\nna:\n{2}\n\nCzy chcesz otworzyć lokalizację pliku?");
            French.Add("failedCreateUpdateBat", "fichier:\n{0}\n\nrenamefile:\n{1}\nto:\n{2}\n\nAfficher le dossier?");
            Russian.Add("failedCreateUpdateBat", "Невозможно создать процесс программы обновления.\n\nУдалите вручную этот файл:\n{0}\n\nПереименуйте файл:\n{1}\nна:\n{2}\n\nПерейти к папке прямо сейчас?");

            //Component: 
            //
            English.Add("cantStartNewApp", "Unable to start application, but it is located in \n");
            German.Add("cantStartNewApp", "Kann die Anwendung nicht starten, aber sie befindet sich in \n");
            Polish.Add("cantStartNewApp", "Niepowodzenie przy uruchamianiu aplikacji znajdującej się w \n");
            French.Add("cantStartNewApp", "Échec du lancement de l'application, mais elle est situé dans \n");
            Russian.Add("cantStartNewApp", "Не удалось запустить приложение, но оно расположено в \n");

            //Component: 
            //
            English.Add("autoDetectFailed", "The auto-detection failed. Please use the 'force manual' option");
            German.Add("autoDetectFailed", "Die automatische Erkennung ist fehlgeschlagen. Bitte benutzen Sie die 'erzwinge manuelle' Option");
            Polish.Add("autoDetectFailed", "Niepowodzenie automatycznego wykrywania. Proszę wybrać opcję ręcznego znajdowania ścieżki gry.");
            French.Add("autoDetectFailed", "Échec de la détection automatique. Utilisez l'option 'Forcer détection manuel'");
            Russian.Add("autoDetectFailed", "Сбой автоматического обнаружения игры. Используйте опцию принудительного указания.");

            //Component: MainWindow_Load
            //
            English.Add("anotherInstanceRunning", "Another Instance of the Relhax Manager is already running");
            German.Add("anotherInstanceRunning", "Eine weitere Instanz des Relhax Manager  läuft bereits");
            Polish.Add("anotherInstanceRunning", "Inna instancja Relhax Manager jest uruchomiona");
            French.Add("anotherInstanceRunning", "Une autre instance de Relhax Directeur est en cours d`éxecution");
            Russian.Add("anotherInstanceRunning", "Запущен ещё один процесс Relhax Manager");

            //Component: closeInstanceRunningForUpdate
            //
            English.Add("closeInstanceRunningForUpdate", "Please close ALL running instances of the Relhax Manager before we can go on and update.");
            German.Add("closeInstanceRunningForUpdate", "Bitte schließe ALLE laufenden Instanzen des Relhax Managers bevor wir fortfahren und aktualisieren können.");
            Polish.Add("closeInstanceRunningForUpdate", "Proszę zamknąć WSZYSTKIE działające instancje Relhax Modpack przed dalszym procesem aktualizacji.");
            French.Add("closeInstanceRunningForUpdate", "Merci de fermé toutes les instances du modpack relhax avant que nous puissions procéder à la mise à jour");
            Russian.Add("closeInstanceRunningForUpdate", "Пожалуйста, закройте ВСЕ запущенные процессы Relhax Manager перед тем, как мы сможем продолжить работу и обновление.");

            //Component: 
            //
            English.Add("skipUpdateWarning", "You are skipping updating. Database Compatibility is not guaranteed");
            German.Add("skipUpdateWarning", "Sie überspringen die Aktualisierung. Datenbankkompatibilität ist nicht garantiert");
            Polish.Add("skipUpdateWarning", "Pomijasz aktualizację! Może wystąpić niezgodność wersji.");
            French.Add("skipUpdateWarning", "Vous ignorez la mise à jour. Compatibilité de la base de données non garanti ");
            Russian.Add("skipUpdateWarning", "Вы пропустили обновление. Совместимость базы данных не гарантирована.");

            //Component: 
            //
            English.Add("patchDayMessage", "The modpack is currently down for patch day testing and mods updating. Sorry for the inconvenience." +
                " If you are a database manager, please add the command argument");
            German.Add("patchDayMessage", "Das Modpack ist zur Zeit für Patch-Tag-Tests und das Updaten von Mods nicht erreichbar. Entschuldige die Unannehmlichkeiten." +
                " Wenn Sie ein Datenbankmanager sind, fügen Sie bitte das Befehlsargument hinzu");
            Polish.Add("patchDayMessage", "Paczka nie działa ze względu na testy i aktualizację modyfikacji. Przepraszamy za utrudnienia. Jeśli zarządzasz bazą danych," +
                " proszę dodać odpowiednią komendę");
            French.Add("patchDayMessage", "Le pack mod est actuellement indisponible aujourd'hui pour tester et mettre à jour les mods. Désolé pour le dérangement." +
                " Si vous êtes un gestionnaire de base de données, ajoutez l'argument de commande.");
            Russian.Add("patchDayMessage", "В настоящее время модпак отключён в связи с выходом патча, как и обновлений модов. Приносим извинения за неудобства." +
                "Если вы ответственны непосредственно за базу данных, то добавьте аргумент командной строки");

            //Component: 
            //
            English.Add("configNotExist", "{0} does NOT exist, loading in regular mode");
            German.Add("configNotExist", "{0} existiert nicht, lädt im regulären Modus");
            Polish.Add("configNotExist", "{0} nie istnieje, ładowanie podstawowego trybu");
            French.Add("configNotExist", "{0} n'existe pas, chargement en mode normal");
            Russian.Add("configNotExist", "{0} НЕ существует, запуск в обычном режиме");

            //Component: 
            //
            English.Add("autoAndFirst", "First time loading cannot be an auto install mode, loading in regular mode");
            German.Add("autoAndFirst", "Erstmaliges Laden kann kein automatischer Installationsmodus sein, lade im regulären Modus");
            Polish.Add("autoAndFirst", "Pierwsze ładowanie nie może być automatyczną instalacją, ładowanie w trybie podstawowym");
            French.Add("autoAndFirst", "Le premier lancement ne peut pas être un mode d'installation automatique, chargement en mode normal");
            Russian.Add("autoAndFirst", "Первый запуск не может быть произведён в режиме автоматической установкой, запуск в обычном режиме.");

            //Component: 
            //
            English.Add("confirmUninstallHeader", "Confirmation");
            German.Add("confirmUninstallHeader", "Bestätigung");
            Polish.Add("confirmUninstallHeader", "Potwierdź");
            French.Add("confirmUninstallHeader", "Confirmation");
            Russian.Add("confirmUninstallHeader", "Подтверждение");

            //Component: 
            //
            English.Add("confirmUninstallMessage", "Confirm you wish to uninstall mods from WoT installation\n\n{0}\n\nUsing uninstall method '{1}'?");
            German.Add("confirmUninstallMessage", "Bitte bestätige das du alle Mods von Deinem WoT deinstalliert haben möchtest\n\n{0}\n\nVerwendung von Deinstallationsmethode '{1}'?");
            Polish.Add("confirmUninstallMessage", "Potwierdź usunięcie modyfikacji\n\n{0}\n\nPotwierdź metodę '{1}'");
            French.Add("confirmUninstallMessage", "Confirmer que vous voulez désinstaller les mods du dossier WoT\n\n{0}\n\nUsing la méthode de désinstallation '{1}'?");
            Russian.Add("confirmUninstallMessage", "Подтвердите необходимость удалить моды для WoT в этой папке: \n\n{0}\n\nИспользуем метод '{1}'?");

            //Component: 
            //
            English.Add("uninstallingText", "Uninstalling...");
            German.Add("uninstallingText", "Deinstalliere...");
            Polish.Add("uninstallingText", "Deinstalacja w toku...");
            French.Add("uninstallingText", "Désinstallation...");
            Russian.Add("uninstallingText", "Удаление...");

            //Component:
            //progress message
            English.Add("uninstallingFile", "Uninstalling file");
            German.Add("uninstallingFile", "Deinstalliere datei");
            Polish.Add("uninstallingFile", "Odinstalowanie pliku");
            French.Add("uninstallingFile", "Désinstallation du fichier");
            Russian.Add("uninstallingFile", "Удаляется файл");

            //Component: uninstallfinished messagebox
            //
            English.Add("uninstallFinished", "Uninstallation of mods finished.");
            German.Add("uninstallFinished", "Deinstallation der mods beendet.");
            Polish.Add("uninstallFinished", "Deinstalacja (modyfikacji) zakończona");
            French.Add("uninstallFinished", "Désinstallation des mods terminé");
            Russian.Add("uninstallfinished", "Удаление модов завершено.");

            //Component: uninstallFail
            //
            English.Add("uninstallFail", "The uninstallation failed. You could try another uninstallation mode or submit a bug report.");
            German.Add("uninstallFail", TranslationNeeded);
            Polish.Add("uninstallFail", TranslationNeeded);
            French.Add("uninstallFail", TranslationNeeded);
            Russian.Add("uninstallFail", TranslationNeeded);

            //Component: 
            //
            English.Add("extractionErrorMessage", "Error deleting the folder res_mods or mods. Either World of Tanks is running or your file" +
                " and folder security permissions are incorrect");
            German.Add("extractionErrorMessage", "Fehler beim Löschen des Ordners res_mods oder Mods. Entweder World of Tanks läuft oder Ihre Datei-" +
                " und Ordnersicherheitsberechtigungen sind falsch");
            Polish.Add("extractionErrorMessage", "Błąd usuwania folderu res_mods lub mods. Albo World of Tanks jest uruchomione, albo twój plik i folder" +
                " mają nieprawidłowe zabezpieczenia dostępu");
            French.Add("extractionErrorMessage", "Erreur lors de la supression du dossier res_mods ou un/plusieur mods. Sois que World of Tanks est en" +
                " cours d`éxecution ou les permissions de sécuriter sont incorrect.");
            Russian.Add("extractionErrorMessage", "Возникла ошибка при удалении папки res_mods или mods. Возможно, запущен World of Tanks или неверно" +
                " настроены разрешения к папкам и файлам.");

            //Component: 
            //
            English.Add("extractionErrorHeader", "Error");
            German.Add("extractionErrorHeader", "Fehler");
            Polish.Add("extractionErrorHeader", "Błąd");
            French.Add("extractionErrorHeader", "Erreur");
            Russian.Add("extractionErrorHeader", "Ошибка");

            //Component: 
            //
            English.Add("deleteErrorHeader", "close out of folders");
            German.Add("deleteErrorHeader", "Ausschliessen von Ordnern");
            Polish.Add("deleteErrorHeader", "zamknij foldery");
            French.Add("deleteErrorHeader", "Fermez les dossiers");
            Russian.Add("deleteErrorHeader", "Закройте папки");

            //Component: 
            //
            English.Add("deleteErrorMessage", "Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");
            German.Add("deleteErrorMessage", "Bitte schließen Sie alle Explorer-Fenster in Mods oder res_mods (oder Unterordner), und klicken Sie auf OK, um fortzufahren.");
            Polish.Add("deleteErrorMessage", "Proszę zamknij folder mods lub res_mods (lub podfoldery), a następnie kliknij kontynuację.");
            French.Add("deleteErrorMessage", "Veuillez fermer les fenêtre res_mods ou mods (Ou tout sous-dossiers) et cliquez Ok pour continuer");
            Russian.Add("deleteErrorMessage", "Закройте окна проводника, в которых открыты mods или res_mods (или глубже), и нажмите OK, чтобы продолжить.");

            //Component: 
            //
            English.Add("noUninstallLogMessage", "The log file containing the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");
            German.Add("noUninstallLogMessage", "Die Protokolldatei mit der Liste der installierten Dateien (installedRelhaxFiles.log) existiert nicht. Möchtest Du alle Mods stattdessen entfernen?");
            Polish.Add("noUninstallLogMessage", "Plik logów zawierający listę instalacyjną (installedRelhaxFiles.log) nie istnieje. Czy chciałbyś usunąć modyfikacje?");
            French.Add("noUninstallLogMessage", "Le ficher log contenant la liste des fichiers installé (installedRelhaxFiles.log) n'existe pas. Voulez vous supprimez tout les mods?");
            Russian.Add("noUninstallLogMessage", "Файл со списком установленных файлов (installedRelhaxFiles.log) не существует. Хотите удалить все установленные моды в таком случае?");

            //Component: 
            //
            English.Add("noUninstallLogHeader", "Remove all mods");
            German.Add("noUninstallLogHeader", "Entferne alle Mods");
            Polish.Add("noUninstallLogHeader", "Usuń wszystkie modyfikacje");
            French.Add("noUninstallLogHeader", "Supprimer tout les mods");
            Russian.Add("noUninstallLogHeader", "Удалить все моды");

            //Component: moveOutOfTanksLocation
            //
            English.Add("moveOutOfTanksLocation", "The modpack can not be run from the World_of_Tanks directory. Please move the application and try again.");
            German.Add("moveOutOfTanksLocation", "Das Modpack kann nicht aus dem World_of_Tanks Verzeichnis laufen. Bitte verschiebe die Anwendung in ein" +
                " anderes Verzeichnis und versuchen Sie es erneut.");
            Polish.Add("moveOutOfTanksLocation", "Modpack nie może być uruchomiony z katalogu World_of_Tanks. Przenieś aplikację i spróbuj ponownie.");
            French.Add("moveOutOfTanksLocation", "Le Mod pack ne peut pas être éxecuté a partir du dossier de World of Tanks. Veuillez déplacer l`application" +
                " dans un autre dossier et réessayer");
            Russian.Add("moveOutOfTanksLocation", "Модпак не может быть запущен из папки с игрой. Пожалуйста, переместите его в другую папку и попробуйте ещё раз.");

            //Component: Current database is same as last installed database (body)
            //
            English.Add("DatabaseVersionsSameBody", "The database has not changed since your last installation. Therefore there are no updates to your current mods selection." +
                " Continue anyway?");
            German.Add("DatabaseVersionsSameBody", "Die Datenbank  wurde seit Deiner letzten Installation nicht verändert. Daher gibt es keine Aktuallisierungen zu Deinen aktuellen" +
                " Modifikationen. Trotzdem fortfahren?");
            Polish.Add("DatabaseVersionsSameBody", "Baza danych nie została zmieniona od ostatniej instalacji, nie ma żadych aktualizacji dla wybranych uprzednio modyfikacji." +
                " Czy kontynuować?");
            French.Add("DatabaseVersionsSameBody", "La base de données n'a pas changé depuis votre dernière installation. Par conséquent, il n'y a pas de mise à jour pour votre sélection" +
                "  de mods. Continuer de toute façon?");
            Russian.Add("DatabaseVersionsSameBody", "База данных не менялась с момента последней установки. В то же время не обновлений к выбранным вами модам. Продолжить в любом случае?");

            //Component: Current database is same as last installed database (header)
            //
            English.Add("DatabaseVersionsSameHeader", "Database version is the same");
            German.Add("DatabaseVersionsSameHeader", "Datenbank Version ist identisch");
            Polish.Add("DatabaseVersionsSameHeader", "Wersja bazy danych jest taka sama");
            French.Add("DatabaseVersionsSameHeader", "La version de la base de données est la même");
            Russian.Add("DatabaseVersionsSameHeader", "Одинаковые версии БД");

            //Component: 
            //
            English.Add("databaseNotFound", "Database not found at supplied URL");
            German.Add("databaseNotFound", "Datenbank nicht an der angegebenen URL gefunden");
            Polish.Add("databaseNotFound", "Nie znaleziono bazy danych pod wskazanym URL");
            French.Add("databaseNotFound", "Base de données introuvable à L'URL fournie");
            Russian.Add("databaseNotFound", "По запрошенному адресу база данных не найдена");

            //Component: Detected client version
            //
            English.Add("detectedClientVersion", "Detected Client version");
            German.Add("detectedClientVersion", "Erkannte Client Version");
            Polish.Add("detectedClientVersion", "Wykryta wersja klienta gry");
            French.Add("detectedClientVersion", "Version du client détecté");
            Russian.Add("detectedClientVersion", "Обнаруженная версия клиента");

            //Component: Supported client versions
            //
            English.Add("supportedClientVersions", "Supported Clients");
            German.Add("supportedClientVersions", "Unterstützte Clients");
            Polish.Add("supportedClientVersions", "Wspomagane wersje klienta gry");
            French.Add("supportedClientVersions", "Clients supportés");
            Russian.Add("supportedClientVersions", "Поддерживаемые версии клиента");

            //Component: Supported clients notice
            //
            English.Add("supportNotGuarnteed", "This client version is not offically supported. Mods may not work.");
            German.Add("supportNotGuarnteed", "Diese Client Version wird (noch) nicht offiziell unterstützt. Die Mods könnten nicht funktionieren oder sogar Dein World of Tanks beschädigen.");
            Polish.Add("supportNotGuarnteed", "Ta wersja klienta gry nie jest oficjalnie wspomagana. Modyfikacje mogą nie działać prawidłowo.");
            French.Add("supportNotGuarnteed", "Ce client n'est pas supporté officiellement. Les mods risque de ne pas fonctionner.");
            Russian.Add("supportNotGuarnteed", "Эта версия клиента официально не поддерживается. Моды могут не работать.");

            //Component: notifying the user the change won't take effect until application restart
            //
            English.Add("noChangeUntilRestart", "This option won't take effect until application restart");
            German.Add("noChangeUntilRestart", "Diese Option hat keine Auswirkungen bis das Programm neu gestartet wurde");
            Polish.Add("noChangeUntilRestart", "Aby zastosować tą opcję należy zrestartować aplikację");
            French.Add("noChangeUntilRestart", "Cette option ne prendra effet qu'au redémarrage de l'application");
            Russian.Add("noChangeUntilRestart", "Для применения настроек потребуется перезапуск программы");

            //Component: installBackupMods
            //
            English.Add("installBackupMods", "Backing up mod file");
            German.Add("installBackupMods", "Mod Dateien sichern");
            Polish.Add("installBackupMods", "Stwórz kopię zapasową pliku modyfikacji");
            French.Add("installBackupMods", "Sauvegarde du fichier mod");
            Russian.Add("installBackupMods", "Создаётся бэкап мода");

            //Component: installBackupData
            //
            English.Add("installBackupData", "Backing up user data");
            German.Add("installBackupData", "Benutzerdaten sichern");
            Polish.Add("installBackupData", "Stwórz kopię zapasową danych użytkownika");
            French.Add("installBackupData", "Sauvegarde des données utilisateur");
            Russian.Add("installBackupData", "Создаётся бэкап пользовательских данных");

            //Component: installClearCache
            //
            English.Add("installClearCache", "Deleting WoT cache");
            German.Add("installClearCache", "WoT Zwischenspeicher löschen");
            Polish.Add("installClearCache", "Usuwanie cache WoTa");
            French.Add("installClearCache", "Suppression du cache WoT");
            Russian.Add("installClearCache", "Удаляется кэш WoT");

            //Component: installClearLogs
            //
            English.Add("installClearLogs", "Deleting log files");
            German.Add("installClearLogs", "Protokolldateien löschen");
            Polish.Add("installClearLogs", "Usuwanie logów");
            French.Add("installClearLogs", "Suppression des fichiers journaux");
            Russian.Add("installClearLogs", "Удаляются log-файлы");

            //Component: installCleanMods
            //
            English.Add("installCleanMods", "Cleaning mods folders");
            German.Add("installCleanMods", "Bereinige Mods-Ordner");
            Polish.Add("installCleanMods", "Oczyszczanie folderu modyfikacji");
            French.Add("installCleanMods", "Nettoyage des dossiers de mods");
            Russian.Add("installCleanMods", "Очищаются папки модов");

            //Component: installExtractingMods
            //
            English.Add("installExtractingMods", "Extracting package");
            German.Add("installExtractingMods", "Entpacke Paket");
            Polish.Add("installExtractingMods", "Wypakowywanie paczki");
            French.Add("installExtractingMods", "Extraction du package");
            Russian.Add("installExtractingMods", "Распаковывается пакет");

            //Component: extractingUserMod
            //
            English.Add("extractingUserMod", "Extracting user package");
            German.Add("extractingUserMod", "Entpacke Benutzermod");
            Polish.Add("extractingUserMod", "Wypakowywanie paczek użytkownika");
            French.Add("extractingUserMod", "Extraction du paquet utilisateur");
            Russian.Add("extractingUserMod", "Распаковывается пользовательский пакет");

            //Component: installRestoreUserdata
            //
            English.Add("installRestoreUserdata", "Restoring user data");
            German.Add("installRestoreUserdata", "Benutzerdaten wiederherstellen");
            Polish.Add("installRestoreUserdata", "Przywracanie danych użytkownika");
            French.Add("installRestoreUserdata", "Restoration des données utilisateur");
            Russian.Add("installRestoreUserdata", "Восстанавливаются пользовательские данные");

            //Component: installXmlUnpack
            //
            English.Add("installXmlUnpack", "Unpacking XML file");
            German.Add("installXmlUnpack", "Entpacke XML Datei");
            Polish.Add("installXmlUnpack", "Rozpakowywanie plików XML");
            French.Add("installXmlUnpack", "Décompression du fichier XML");
            Russian.Add("installXmlUnpack", "Распаковка XML-файла");

            //Component: installPatchFiles
            //
            English.Add("installPatchFiles", "Patching file");
            German.Add("installPatchFiles", "Datei wird geändert");
            Polish.Add("installPatchFiles", "Aktualizowanie plików");
            French.Add("installPatchFiles", "Patch du fichier");
            Russian.Add("installPatchFiles", "Идёт патч файла");

            //Component: installShortcuts
            //
            English.Add("installShortcuts", "Creating shortcuts");
            German.Add("installShortcuts", "Erstelle Verknüpfungen");
            Polish.Add("installShortcuts", "Tworzenie skrótów");
            French.Add("installShortcuts", "Création de raccourcis");
            Russian.Add("installShortcuts", "Создаются ярлыки");

            //Component: installContourIconAtlas
            English.Add("installContourIconAtlas", "Creating Atlas file");
            German.Add("installContourIconAtlas", "Erstelle Atlas Datei");
            Polish.Add("installContourIconAtlas", "Tworzenie pliku Atlas");
            French.Add("installContourIconAtlas", "Creations des fichiers Atlas");
            Russian.Add("installContourIconAtlas", "Создаётся файл-атлас");

            //Component: installFonts
            //
            English.Add("installFonts", "Installing fonts");
            German.Add("installFonts", "Installieren von Schriftarten");
            Polish.Add("installFonts", "Instalowanie czcionek");
            French.Add("installFonts", "Installation des polices");
            Russian.Add("installFonts", "Устанавливаются шрифты");

            //Component: installCleanup
            //
            English.Add("installCleanup", "Cleaning Up");
            German.Add("installCleanup", "Räume auf");
            Polish.Add("installCleanup", "Czyszczenie");
            French.Add("installCleanup", "Nettoyage");
            Russian.Add("installCleanup", "Очистка...");

            //Component: ExtractAtlases
            English.Add("AtlasExtraction", "Extracting Atlas file");
            German.Add("AtlasExtraction", "Entpacke Atlas Datei");
            Polish.Add("AtlasExtraction", "Wypakuj plik Atlas");
            French.Add("AtlasExtraction", "Extraction des fichiers Atlas");
            Russian.Add("AtlasExtraction", "Распаковывается файл-атлас");

            //Component: 
            //
            English.Add("copyingFile", "Copying file");
            German.Add("copyingFile", "Kopieren von Dateien");
            Polish.Add("copyingFile", "Kopiowanie plików");
            French.Add("copyingFile", "Copie des fichiers");
            Russian.Add("copyingFile", "Копирование файла");

            //Component: 
            //
            English.Add("deletingFile", "Deleting file");
            German.Add("deletingFile", "Lösche Datei");
            Polish.Add("deletingFile", "Usuwanie plików");
            French.Add("deletingFile", "Supression du fichier");
            Russian.Add("deletingFile", "Удаление файла");

            //Component DeleteMods
            //
            English.Add("scanningModsFolders", "Scanning mods folders ...");
            German.Add("scanningModsFolders", "Durchsuche Mod Verzeichnisse ...");
            Polish.Add("scanningModsFolders", "Skanowanie folderu modyfikacji");
            French.Add("scanningModsFolders", "Analyse des dossiers de mods ...");
            Russian.Add("scanningModsFolders", "Сканируются папки модов...");

            //Component: file
            //
            English.Add("file", "File");
            German.Add("file", "Datei");
            Polish.Add("file", "Plik");
            French.Add("file", "Fichier");
            Russian.Add("file", "Файл");

            //Component: size
            //
            English.Add("size", "Size");
            German.Add("size", "Größe");
            Polish.Add("size", "Rozmiar");
            French.Add("size", "Taille");
            Russian.Add("size", "Размер");

            //Component: CheckDatabase
            //
            English.Add("checkDatabase", "Checking the database for outdated or no longer needed files");
            German.Add("checkDatabase", "Durchsuche das Dateiarchive nach veralteten oder nicht mehr benötigten Dateien");
            Polish.Add("checkDatabase", "Trwa przeszukiwanie w bazie danych przedawnionych i niepotrzebnych plików");
            French.Add("checkDatabase", "Vérification de la base de données pour les fichiers périmés ou non nécessaires");
            Russian.Add("checkDatabase", "Идёт проверка БД на наличие неактуальных или ненужных файлов");

            //Component: 
            //function checkForOldZipFiles() 
            English.Add("parseDownloadFolderFailed", "Parsing the \"{0}\" folder failed.");
            German.Add("parseDownloadFolderFailed", "Durchsehen des \"{0}\" Verzeichnisses ist fehlgeschlagen.");
            Polish.Add("parseDownloadFolderFailed", "Pobieranie informacji o folderze \"{0}\" zakończone niepowodzeniem");
            French.Add("parseDownloadFolderFailed", "L'analyse du dossier \"{0}\" a échoué.");
            Russian.Add("parseDownloadFolderFailed", "Не удалось обработать папку \"{0}\"");

            //Component: installation finished
            //
            English.Add("installationFinished", "Installation is finished");
            German.Add("installationFinished", "Die Installation ist abgeschlossen");
            Polish.Add("installationFinished", "Instalacja jest zakończona");
            French.Add("installationFinished", "L'installation est terminée");
            Russian.Add("installationFinished", "Установка завершена");

            //Component: deletingFiles
            //
            English.Add("deletingFiles", "Deleting files");
            German.Add("deletingFiles", "Lösche Dateien");
            Polish.Add("deletingFiles", "Usuwanie plików");
            French.Add("deletingFiles", "Suppression de fichiers");
            Russian.Add("deletingFiles", "Удаляются файлы");

            //Component: 
            //
            English.Add("uninstalling", "Uninstalling");
            German.Add("uninstalling", "Deinstallieren");
            Polish.Add("uninstalling", "Deinstalacja w toku");
            French.Add("uninstalling", "Désinstallation");
            Russian.Add("uninstalling", "Удаляется...");

            //Component:
            //
            English.Add("zipReadingErrorHeader", "Incomplete Download");
            German.Add("zipReadingErrorHeader", "Unvollständiger Download");
            Polish.Add("zipReadingErrorHeader", "Ściąganie niekompletne");
            French.Add("zipReadingErrorHeader", "Téléchargement incomplet");
            Russian.Add("zipReadingErrorHeader", "Незаконченная загрузка");

            //Component:
            //
            English.Add("zipReadingErrorMessage1", "The zip file");
            German.Add("zipReadingErrorMessage1", "Die Zip-Datei");
            Polish.Add("zipReadingErrorMessage1", "Plik skomresowany formatu ZIP ");
            French.Add("zipReadingErrorMessage1", "Le fichier ZIP");
            Russian.Add("zipReadingErrorMessage1", "ZIP-архив");

            //Component:
            //
            English.Add("zipReadingErrorMessage2", "could not be read, most likely due to an incomplete download. Please try again.");
            German.Add("zipReadingErrorMessage2", "Konnte nicht gelesen werden, da es höchstwahrscheinlich ein unvollständiger Download ist. Bitte versuche es später nochmal.");
            Polish.Add("zipReadingErrorMessage2", "Nie można odczytać, prawdopodobnie niekompletność pobranych plików. Proszę spróbować ponownie");
            French.Add("zipReadingErrorMessage2", "n'as pas pus être lus, probablement un téléchargement incomplet. Veuillez réeissayer.");
            Russian.Add("zipReadingErrorMessage2", "не может быть прочитан, возможно, причиной послужила незаконченная загрузка. Попробуйте ещё раз.");

            //Component:
            //
            English.Add("zipReadingErrorMessage3", "Could not be read.");
            German.Add("zipReadingErrorMessage3", "Konnte nicht gelesen werden.");
            Polish.Add("zipReadingErrorMessage3", "Nie można odczytać.");
            French.Add("zipReadingErrorMessage3", "n'as pas pus être lus");
            Russian.Add("zipReadingErrorMessage3", "Сбой чтения");

            //Component: 
            //
            English.Add("patchingSystemDeneidAccessMessage", "The patching system was denied access to the patch folder. Retry as Administrator. If you see this again, you need to fix your" +
                " file and folder security permissions");
            German.Add("patchingSystemDeneidAccessMessage", "Dem Patching-System wurde der Zugriff auf den Patch-Ordner verweigert. Wiederholen als Administrator. Wenn Sie dies wieder sehen," +
                " müssen Sie Ihre Datei- und Ordnersicherheitsberechtigungen reparieren");
            Polish.Add("patchingSystemDeneidAccessMessage", "Nie uzyskano dostępu do folderu patcha. Spróbuj ponownie jako administrator. Jeśli widzisz to ponownie, to zmień ustawienia" +
                " pozwoleń dostępu do folderów.");
            French.Add("patchingSystemDeneidAccessMessage", "Le système de patching s'est vu refuser l'accès au dossier de patch. Réessayez en tant qu'administrateur. Si vous voyez ceci" +
                " à nouveau, assurez vous que vos permissions de sécurités de dossiers et de fichiers son suffisantes.");
            Russian.Add("patchingSystemDeneidAccessMessage", "Системе патчинга был отказан доступ к папке с патчами. Попробуйте повторить операцию от имени администратора. " +
                "Если вы снова видите это окно, то исправьте ошибки в правах доступа к файлам и папкам.");

            //Component: 
            //
            English.Add("patchingSystemDeneidAccessHeader", "Access Denied");
            German.Add("patchingSystemDeneidAccessHeader", "Zugriff abgelehnt");
            Polish.Add("patchingSystemDeneidAccessHeader", "Odmowa dostępu");
            French.Add("patchingSystemDeneidAccessHeader", "Accès refusé");
            Russian.Add("patchingSystemDeneidAccessHeader", "В доступе отказано");

            //Component: Failed To Delete folder
            //
            English.Add("folderDeleteFailed", "Failed to delete the folder");
            German.Add("folderDeleteFailed", "Löschen des Verzeichnis fehlgeschlagen");
            Polish.Add("folderDeleteFailed", "Próba usunięcia folderu zakończona niepowodzeniem");
            French.Add("folderDeleteFailed", "Échec de la suppression du dossier");
            Russian.Add("folderDeleteFailed", "Не удалось удалить папку");

            //Component: Failed To Delete file
            //
            English.Add("fileDeleteFailed", "Failed to delete the file");
            German.Add("fileDeleteFailed", "Löschen der Datei fehlgeschlagen");
            Polish.Add("fileDeleteFailed", "Próba usunięcia pliku zakończona niepowodzeniem");
            French.Add("fileDeleteFailed", "Échec de la supression du fichier");
            Russian.Add("fileDeleteFailed", "Не удалось удалить файл");

            //Component: DeleteBackupFolder
            //
            English.Add("DeleteBackupFolder", "Backups");
            German.Add("DeleteBackupFolder", "Sicherungen");
            Polish.Add("DeleteBackupFolder", "Kopie zapasowe");
            French.Add("DeleteBackupFolder", "Sauvegardes");
            Russian.Add("DeleteBackupFolder", "Бэкапы");

            //Component: installFailed
            //
            English.Add("installFailed", "The installation failed at step");
            German.Add("installFailed", TranslationNeeded);
            Polish.Add("installFailed", TranslationNeeded);
            French.Add("installFailed", TranslationNeeded);
            Russian.Add("installFailed", TranslationNeeded);
            #endregion

            #region Install finished window
            //Component: InstallationCompleteText
            //
            English.Add("InstallationCompleteText", "The Installation is complete. Would you like to...");
            German.Add("InstallationCompleteText", "Installation ist beendet. Willst du...");
            Polish.Add("InstallationCompleteText", "Instalacja jest zakończona. Czy chciałbyś...");
            French.Add("InstallationCompleteText", "L'installation est terminée. Voudriez-vous...");
            Russian.Add("InstallationCompleteText", "Установка завершена. Хотите...");

            //Component: InstallationCompleteStartWoT
            //
            English.Add("InstallationCompleteStartWoT", "Start the game? (WorldofTanks.exe)");
            German.Add("InstallationCompleteStartWoT", "Das Spiel starten? (WorldofTanks.exe)");
            Polish.Add("InstallationCompleteStartWoT", "Uruchomić grę? (WorldofTanks.exe)");
            French.Add("InstallationCompleteStartWoT", "Démarrez le jeu? (WorldofTanks.exe)");
            Russian.Add("InstallationCompleteStartWoT", "Запустить игру? (WorldofTanks.exe)");

            //Component: InstallationCompleteStartGameCenter
            //
            English.Add("InstallationCompleteStartGameCenter", "Start WG Game Center (WoTLauncher.exe)?");
            German.Add("InstallationCompleteStartGameCenter", "Starte WG Game Center (WoTLauncher.exe)?");
            Polish.Add("InstallationCompleteStartGameCenter", "Czy uruchomić WG Game Center (WoTLauncher.exe)?");
            French.Add("InstallationCompleteStartGameCenter", "Démarrer WG Game Center (WoTLauncher.exe)?");
            Russian.Add("InstallationCompleteStartGameCenter", "Запустить WG Game Center (или WoTLauncher.exe)?");

            //Component: InstallationCompleteOpenXVM
            //
            English.Add("InstallationCompleteOpenXVM", "Open your web browser to the xvm statistics login website?");
            German.Add("InstallationCompleteOpenXVM", "Öffne Deinen Browser auf der XVM Statistik Login Webseite?");
            Polish.Add("InstallationCompleteOpenXVM", "Otworzyć stronę statystyk XVM ?");
            French.Add("InstallationCompleteOpenXVM", "Ouvrir votre navigateur Web vers le site de connexion aux statistiques xvm?");
            Russian.Add("InstallationCompleteOpenXVM", "Открыть браузер на сайте XVM для активации статистики?");

            //Component: InstallationCompleteCloseThisWindow
            //
            English.Add("InstallationCompleteCloseThisWindow", "Close this window?");
            German.Add("InstallationCompleteCloseThisWindow", "Schließe dieses Fenster?");
            Polish.Add("InstallationCompleteCloseThisWindow", TranslationNeeded);
            French.Add("InstallationCompleteCloseThisWindow", "Fermer cette fenêtre?");
            Russian.Add("InstallationCompleteCloseThisWindow", "Закрыть окно?");

            //Component: InstallationCompleteCloseApp
            //
            English.Add("InstallationCompleteCloseApp", "Close the application?");
            German.Add("InstallationCompleteCloseApp", "Anwendung schließen");
            Polish.Add("InstallationCompleteCloseApp", "Zamknij aplikację");
            French.Add("InstallationCompleteCloseApp", "Fermer l'application");
            Russian.Add("InstallationCompleteCloseApp", "Закрыть приложение?");

            //Component: StartXVMStatButton_Click
            //localisation to which page you will jump
            English.Add("xvmUrlLocalisation", "en");
            German.Add("xvmUrlLocalisation", "de");
            Polish.Add("xvmUrlLocalisation", "en");
            French.Add("xvmUrlLocalisation", "fr");
            Russian.Add("xvmUrlLocalisation", "ru");

            //Component: CouldNotStartProcess
            //
            English.Add("CouldNotStartProcess", "Could not start process");
            German.Add("CouldNotStartProcess","Konnte Prozess nicht starten");
            Polish.Add("CouldNotStartProcess", "Nie można rozpocząć procesu");
            French.Add("CouldNotStartProcess", "Impossible de démarrer le processus");
            Russian.Add("CouldNotStartProcess", "Не удалось запустить процесс");
            #endregion

            #region Diagnostics
            //Component: MainTextBox
            //
            English.Add("DiagnosticsMainTextBox", "You can use the options below to try to diagnose or solve the issues you are having.");
            German.Add("DiagnosticsMainTextBox", "Du kannst mit den untenstehenden Optionen Probleme mit dem Spiel zu diagnostizieren und ggf. zu beheben.");
            Polish.Add("DiagnosticsMainTextBox", "Opcje diagnostyczne i rozwiązywanie problemów");
            French.Add("DiagnosticsMainTextBox", "Vous pouvez utiliser les options ci dessous pour essayer de diagnostiqué ou résoudre les soucis que vous avez");
            Russian.Add("DiagnosticsMainTextBox", "Вы можете попробовать способы ниже для диагностики или выявления проблем с клиентом игры.");

            //Component: LaunchWoTLauncher
            //
            English.Add("LaunchWoTLauncher", "Start the World of Tanks launcher in integrity validation mode");
            German.Add("LaunchWoTLauncher", "Starte den \"World of Tanks Launcher\" im Integritätsvalidierungsmodus");
            Polish.Add("LaunchWoTLauncher", "Uruchom launcher World of Tanks w trybie sprawdzania integralności.");
            French.Add("LaunchWoTLauncher", "Lancé le launcher world of tanks en mode vérification d'intégrité");
            Russian.Add("LaunchWoTLauncher", "Запустить лаунчер World of Tanks в режиме проверки целостности");

            //Component: CollectLogInfo
            //
            English.Add("CollectLogInfo", "Collect log files into a zip file to report a problem");
            German.Add("CollectLogInfo", "Sammle und packe Protokolldateien in einer ZIP-Datei um ein Problem zu melden");
            Polish.Add("CollectLogInfo", "Zbierz pliki logów do pliku zip, aby zgłosić problem");
            French.Add("CollectLogInfo", "Recueillir les fichiers journaux dans un fichier zip pour signaler un problème");
            Russian.Add("CollectLogInfo", "Собрать log-файлы в ZIP-архив для отчёта об ошибке");

            //Component: SelectedInstallation
            //
            English.Add("SelectedInstallation", "Currently Selected Installation:");
            German.Add("SelectedInstallation", "Aktuell gewählte Installation:");
            Polish.Add("SelectedInstallation", "Obecnie Wybrana Instalacja:");
            French.Add("SelectedInstallation", "Installation actuellement sélectionner:");
            Russian.Add("SelectedInstallation", "Текущая папка с игрой:");

            //Component: SelectedInstallationNone
            //
            English.Add("SelectedInstallationNone", "(none)");
            German.Add("SelectedInstallationNone", "(nichts)");
            Polish.Add("SelectedInstallationNone", "(nic)");
            French.Add("SelectedInstallationNone", "(aucun)");
            Russian.Add("SelectedInstallationNone", "(не выбрана)");

            //Component: collectionLogInfo
            //
            English.Add("collectionLogInfo", "Collecting log files...");
            German.Add("collectionLogInfo", "Sammeln der Protokolldateien...");
            Polish.Add("collectionLogInfo", "Zbieranie logów...");
            French.Add("collectionLogInfo", "Collection des fichiers log...");
            Russian.Add("collectionLogInfo", "Идёт сбор log-файлов...");

            //Component: startingLauncherRepairMode
            //
            English.Add("startingLauncherRepairMode", "Starting WoTLauncher in integrity validation mode...");
            German.Add("startingLauncherRepairMode", "Starte den WoT Launcher im Integritätsvalidierungsmodus...");
            Polish.Add("startingLauncherRepairMode", "Uruchamianie WoTLaunchera w trybie sprawdzania integralności...");
            French.Add("startingLauncherRepairMode", "Lancement de WoTLauncher and mode the validation d'intégrité...");
            Russian.Add("startingLauncherRepairMode", "Запускаю WoTLauncher в режиме проверки целостности...");

            //Component: failedStartLauncherRepairMode
            //
            English.Add("failedStartLauncherRepairMode", "Failed to start WoT Launcher in Repair mode");
            German.Add("failedStartLauncherRepairMode", "Der WoT Launcher konnte nicht im Reparaturmodus gestartet werden");
            Polish.Add("failedStartLauncherRepairMode", "Nie udało się uruchomić launchera WoT w trybie naprawy");
            French.Add("failedStartLauncherRepairMode", "Erreur lors du lancement de WoTLauncher en mode de réparation");
            Russian.Add("failedStartLauncherRepairMode", "Не удалось запустить WoTLauncher в режиме проверки целостности");

            //Component: failedCollectFile
            //
            English.Add("failedCollectFile", "Failed to collect the file ");
            German.Add("failedCollectFile", "Fehler beim Sammeln der Datei ");
            Polish.Add("failedCollectFile", "Nie udało się zebrać plików ");
            French.Add("failedCollectFile", "Erreur lors de la collecte du fichier");
            Russian.Add("failedCollectFile", "Не найден файл ");

            //Component: failedCreateZipfile
            //
            English.Add("failedCreateZipfile", "Failed to create the zip file ");
            German.Add("failedCreateZipfile", "Fehler beim Erstellen der Zip-Datei ");
            Polish.Add("failedCreateZipfile", "Nie udało się stworzyć pliku zip ");
            French.Add("failedCreateZipfile", "Erreur lors de la création du fichier zip ");
            Russian.Add("failedCreateZipfile", "Не удалось создать ZIP-архив");

            //Component: launcherRepairModeStarted
            //
            English.Add("launcherRepairModeStarted", "Repair mode successfully started");
            German.Add("launcherRepairModeStarted", "Reparaturmodus wurde erfolgreich gestartet");
            Polish.Add("launcherRepairModeStarted", "Uruchomiono tryb naprawy");
            French.Add("launcherRepairModeStarted", "Mode de réparation démarrer avec succès");
            Russian.Add("launcherRepairModeStarted", "Проверка целостности успешно начата");

            //Component: ClearDownloadCache
            //
            English.Add("ClearDownloadCache", "Clear download cache");
            German.Add("ClearDownloadCache", "Lösche Downlaod Cache");
            Polish.Add("ClearDownloadCache", "Wyczyść ściągnięte pliki cache");
            French.Add("ClearDownloadCache", "Effacer le cache de téléchargement");
            Russian.Add("ClearDownloadCache", "Очистить кэш загрузок");

            //Component: ClearDownloadCacheDatabase
            //
            English.Add("ClearDownloadCacheDatabase", "Delete download cache database file");
            German.Add("ClearDownloadCacheDatabase", "Lösche Datenbank-Cache");
            Polish.Add("ClearDownloadCacheDatabase", "Usuń ściągnięte pliki cache baz danych");
            French.Add("ClearDownloadCacheDatabase", "Supprimer le fichier de base de données de cache de téléchargement");
            Russian.Add("ClearDownloadCacheDatabase", "Удалить кэш базы данных");

            //Component: ClearDownloadCacheDescription
            //
            English.Add("ClearDownloadCacheDescription", "Delete all files in the \"RelhaxDownloads\" folder");
            German.Add("ClearDownloadCacheDescription", "Lösche alle Daten aus dem \"RelhaxDownloads\" Ordner");
            Polish.Add("ClearDownloadCacheDescription", "Usuń wszystkie pliki w folderze \"RelhaxDownloads\"");
            French.Add("ClearDownloadCacheDescription", "Supprimez tous les fichiers dans le répertoire \"RelhaxDownloads\"");
            Russian.Add("ClearDownloadCacheDescription", "Удаление всех файлов в папке \"RelhaxDownloads\"");

            //Component: ClearDownloadCacheDatabaseDescription
            //
            English.Add("ClearDownloadCacheDatabaseDescription", "Delete the xml database file. This will cause all zip files to be re-checked for integrity.\nAll invalid files will be re-downloaded if selected in your next installation.");
            German.Add("ClearDownloadCacheDatabaseDescription", "Löschen Sie die XML-Datenbankdatei. Dadurch werden alle ZIP-Dateien erneut auf Integrität überprüft. \nAlle ungültigen Dateien werden erneut heruntergeladen, wenn sie bei Ihrer nächsten Installation ausgewählt werden");
            Polish.Add("ClearDownloadCacheDatabaseDescription", "Usuń plik bazy danych xml. Spowoduje to ponowne sprawdzenie integralności wszystkich plików zip.\nWszystkie nieprawidłowe pliki zostaną ponownie pobrane, jeśli zostaną wybrane w następnej instalacji");
            French.Add("ClearDownloadCacheDatabaseDescription", "Supprimez le fichier de base de données XML. L’intégrité de tous les fichiers zip sera à nouveau vérifiée. \nTous les fichiers non valides seront re-téléchargés s’ils sont sélectionnés lors de votre prochaine installation.");
            Russian.Add("ClearDownloadCacheDatabaseDescription", "Удаление XML-файла базы данных. Это приведёт к тому, что все скачанныые ZIP-архивы будут проверены на целостность.\nВсе повреждённые файлы будут загружены заново в случае, если вы ещё раз выберете моды, соотвествующие этим архивам.");

            //Component: ChangeInstall
            //
            English.Add("ChangeInstall", "Change the currently selected WoT installation");
            German.Add("ChangeInstall", "Ändern Sie die aktuell ausgewählte WoT-Installation");
            Polish.Add("ChangeInstall", "Zmień obecnie wybraną instalację WoT");
            French.Add("ChangeInstall", "Modifier l'installation WOT sélectionné");
            Russian.Add("ChangeInstall", "Изменить выбранную папку с World of Tanks");

            //Component: ChangeInstallDescription
            //
            English.Add("ChangeInstallDescription", "This will change which log files will get added to the diagnostics zip file");
            German.Add("ChangeInstallDescription", "Dadurch wird geändert, welche Protokolldateien zur ZIP-Diagnosedatei hinzugefügt werden");
            Polish.Add("ChangeInstallDescription", "Spowoduje to zmianę logów, które zostaną dodane do diagnostycznego pliku zip");
            French.Add("ChangeInstallDescription", "Cela modifiera quel fichiers journaux qui seront ajoutés au fichier zip de diagnostic.");
            Russian.Add("ChangeInstallDescription", "Это изменит набор файлов отчёта об ошибках, добавляемых в ZIP-архив.");

            //Component: zipSavedTo
            //
            English.Add("zipSavedTo", "Zip file saved to: ");
            German.Add("zipSavedTo", "Zip-Datei gespeichert in: ");
            Polish.Add("zipSavedTo", "Plik zip zapisano do: ");
            French.Add("zipSavedTo", "Fichier zip sauvegarder à: ");
            Russian.Add("zipSavedTo", "ZIP-архив успешно сохранён в: ");

            //Component: selectFilesToInclude
            //
            English.Add("selectFilesToInclude", "Select files to include in the bug report");
            German.Add("selectFilesToInclude", TranslationNeeded);
            Polish.Add("selectFilesToInclude", TranslationNeeded);
            French.Add("selectFilesToInclude", TranslationNeeded);
            Russian.Add("selectFilesToInclude", TranslationNeeded);
            #endregion

            #region Preview Window
            //Component: noDescription
            //
            English.Add("noDescription", "No description provided");
            German.Add("noDescription", "Keine Beschreibung verfügbar");
            Polish.Add("noDescription", "Nie podano opisu");
            French.Add("noDescription", "NPas de description fournie");
            Russian.Add("noDescription", "Описание отсутствует");

            //Component: noUpdateInfo
            //
            English.Add("noUpdateInfo", "No update info provided");
            German.Add("noUpdateInfo", "Keine Aktualisierungsinformationen verfügbar");
            Polish.Add("noUpdateInfo", "Brak informacji o aktualizacji");
            French.Add("noUpdateInfo", "Aucune information de mise à jour fournie");
            Russian.Add("noUpdateInfo", "Отсутствует информация об обновлении");

            //Component: noTimestamp
            //
            English.Add("noTimestamp", "No timestamp provided");
            German.Add("noTimestamp", "Kein Zeitstempel verfügbar");
            Polish.Add("noTimestamp", "Brak znacznika czasowego");
            French.Add("noTimestamp", "Pas d'horodatage fourni");
            Russian.Add("noTimestamp", "Нет метки с датой");

            //Component: PreviewNextPicButton
            //
            English.Add("PreviewNextPicButton", "Next");
            German.Add("PreviewNextPicButton", "Weiter");
            Polish.Add("PreviewNextPicButton", "Dalej");
            French.Add("PreviewNextPicButton", "Suivant");
            Russian.Add("PreviewNextPicButton", "след.");

            //Component: PreviewPreviousPicButton
            //
            English.Add("PreviewPreviousPicButton", "Previous");
            German.Add("PreviewPreviousPicButton", "Zurück");
            Polish.Add("PreviewPreviousPicButton", "Wstecz");
            French.Add("PreviewPreviousPicButton", "Précedent");
            Russian.Add("PreviewPreviousPicButton", "пред.");

            //Component: DevUrlHeader
            //
            English.Add("DevUrlHeader", "Developer links");
            German.Add("DevUrlHeader", "Entwickler-Link");
            Polish.Add("DevUrlHeader", "Linki deweloperów");
            French.Add("DevUrlHeader", "Liens développeur");
            Russian.Add("DevUrlHeader", "Сайт разработчика");

            //Component: dropDownItemsInside
            //
            English.Add("dropDownItemsInside", "Items Inside");
            German.Add("dropDownItemsInside", "Gegenstände im Inneren");
            Polish.Add("dropDownItemsInside", TranslationNeeded);
            French.Add("dropDownItemsInside", "Articles à l'intérieur");
            Russian.Add("dropDownItemsInside", "Элементов внутри");

            //Component: popular
            //
            English.Add("popular", "popular");
            German.Add("popular", TranslationNeeded);
            Polish.Add("popular", TranslationNeeded);
            French.Add("popular", TranslationNeeded);
            Russian.Add("popular", TranslationNeeded);

            //Component: previewEncounteredError
            //
            English.Add("previewEncounteredError", "The preview window encountered an error. Failed to display preview.");
            German.Add("previewEncounteredError", TranslationNeeded);
            Polish.Add("previewEncounteredError", TranslationNeeded);
            French.Add("previewEncounteredError", TranslationNeeded);
            Russian.Add("previewEncounteredError", TranslationNeeded);
            #endregion

            #region Developer Selection Window
            //Component: DeveloperSelectionsViewer
            //
            English.Add("DeveloperSelectionsViewer", "Selections Viewer");
            German.Add("DeveloperSelectionsViewer", "Auswahl-Betrachter");
            Polish.Add("DeveloperSelectionsViewer", "Wybór Widoku");
            French.Add("DeveloperSelectionsViewer", "Visualiseur de sélections");
            Russian.Add("DeveloperSelectionsViewer", "Просмотр наборов");

            //Component: DeveloperSelectionsTextHeader
            //
            English.Add("DeveloperSelectionsTextHeader", "Selection to load");
            German.Add("DeveloperSelectionsTextHeader", "Auswahl zum Laden");
            Polish.Add("DeveloperSelectionsTextHeader", "Wybór do załadowania");
            French.Add("DeveloperSelectionsTextHeader", "Sélection à charger");
            Russian.Add("DeveloperSelectionsTextHeader", "Набор для загрузки");

            //Component: DeveloperSelectionsCancelButton
            //
            English.Add("DeveloperSelectionsCancelButton", "Cancel");
            German.Add("DeveloperSelectionsCancelButton", "Abbrechen");
            Polish.Add("DeveloperSelectionsCancelButton", "Anuluj");
            French.Add("DeveloperSelectionsCancelButton", "Annuler");
            Russian.Add("DeveloperSelectionsCancelButton", "Отмена");

            //Component: DeveloperSelectionsLocalFile
            //The text in the first radioButton in the selection viewer, for the user to select their own personal config file to load
            English.Add("DeveloperSelectionsLocalFile", "Local file");
            German.Add("DeveloperSelectionsLocalFile", "Lokale Datei");
            Polish.Add("DeveloperSelectionsLocalFile", "Plik lokalny");
            French.Add("DeveloperSelectionsLocalFile", "Fichier local");
            Russian.Add("DeveloperSelectionsLocalFile", "Локальный файл");

            //Component: DeveloperSelectionsContinueButton
            //
            English.Add("DeveloperSelectionsContinueButton", "Select");
            German.Add("DeveloperSelectionsContinueButton", "Auswahl bestätigen");
            Polish.Add("DeveloperSelectionsContinueButton", "Wybierz");
            French.Add("DeveloperSelectionsContinueButton", "Selectionner");
            Russian.Add("DeveloperSelectionsContinueButton", "Выбрать");

            //Component: failedToParseSelections
            //
            English.Add("failedToParseSelections", "Failed to parse selections");
            German.Add("failedToParseSelections", "Auswahl konnte nicht analysiert werden");
            Polish.Add("failedToParseSelections", "Nie udało się przeanalizować konfiguracji");
            French.Add("failedToParseSelections", "Échec de l'analyse des sélections");
            Russian.Add("failedToParseSelections", "Сбой обработки набора");

            //Component: lastModified
            //
            English.Add("lastModified", "Last modified");
            German.Add("lastModified", "Zuletzt geändert");
            Polish.Add("lastModified", "Ostatnio zmodyfikowano");
            French.Add("lastModified", "Dernière modification");
            Russian.Add("lastModified", "Последнее изменение");
            #endregion

            #region Advanced Installer Window
            //Component: AdvancedInstallBackupMods
            //
            English.Add("AdvancedInstallBackupMods", "Backup current installation");
            German.Add("AdvancedInstallBackupMods", "Sichere derzeitige Installation (Backup)");
            Polish.Add("AdvancedInstallBackupMods", "Kopia zapasowa obecnej instalacji");
            French.Add("AdvancedInstallBackupMods", "Sauvegarder l'installation actuelle");
            Russian.Add("AdvancedInstallBackupMods", "Бэкап имеющихся модов");

            //Component: AdvancedInstallBackupData
            //
            English.Add("AdvancedInstallBackupData", "Backup Mod Data");
            German.Add("AdvancedInstallBackupData", "Sichern Sie die Mod-Daten");
            Polish.Add("AdvancedInstallBackupData", "Dane kopii zapasowej modyfikacji");
            French.Add("AdvancedInstallBackupData", "Sauvegarder les données du mod");
            Russian.Add("AdvancedInstallBackupData", "Бэкап данных модов");

            //Component: AdvancedInstallClearCache
            //
            English.Add("AdvancedInstallClearCache", "Clear WoT Cache");
            German.Add("AdvancedInstallClearCache", "Lösche den WoT Cache");
            Polish.Add("AdvancedInstallClearCache", "Wyczyść cache WoT");
            French.Add("AdvancedInstallClearCache", "Effacer le cache de WoT");
            Russian.Add("AdvancedInstallClearCache", "Очистить кэш WoT");

            //Component: AdvancedInstallClearLogs
            //
            English.Add("AdvancedInstallClearLogs", "Clear Logfiles");
            German.Add("AdvancedInstallClearLogs", "Lösche Protokolldateien");
            Polish.Add("AdvancedInstallClearLogs", "Wyczyść pliki logów");
            French.Add("AdvancedInstallClearLogs", "Effacer les fichiers journaux");
            Russian.Add("AdvancedInstallClearLogs", "Очистить логи");

            //Component: AdvancedInstallClearMods
            //
            English.Add("AdvancedInstallClearMods", "Uninstall previous installation");
            German.Add("AdvancedInstallClearMods", "Deinstalliere letzte Installation");
            Polish.Add("AdvancedInstallClearMods", "Odinstaluj poprzednią instalację");
            French.Add("AdvancedInstallClearMods", "Désinstaller l'installation précédente");
            Russian.Add("AdvancedInstallClearMods", "Удалить старые моды");

            //Component: AdvancedInstallInstallMods
            //
            English.Add("AdvancedInstallInstallMods", "Install Thread");
            German.Add("AdvancedInstallInstallMods", "Installiere Thread");
            Polish.Add("AdvancedInstallInstallMods", "Zainstaluj wątek");
            French.Add("AdvancedInstallInstallMods", "Fil d'installation");
            Russian.Add("AdvancedInstallInstallMods", "Поток установки");

            //Component: AdvancedInstallUserInstallMods
            //
            English.Add("AdvancedInstallUserInstallMods", "User Install");
            German.Add("AdvancedInstallUserInstallMods", "Benutzerinstallation");
            Polish.Add("AdvancedInstallUserInstallMods", "Instalacja użytkownika");
            French.Add("AdvancedInstallUserInstallMods", "Installation utilisateur");
            Russian.Add("AdvancedInstallUserInstallMods", "Пользовательские моды");

            //Component: AdvancedInstallRestoreData
            //
            English.Add("AdvancedInstallRestoreData", "Restore Data");
            German.Add("AdvancedInstallRestoreData", "Daten widerherstellen");
            Polish.Add("AdvancedInstallRestoreData", "Przywróć dane");
            French.Add("AdvancedInstallRestoreData", "Restaurer les données");
            Russian.Add("AdvancedInstallRestoreData", "Восстановление данных");

            //Component: AdvancedInstallXmlUnpack
            //
            English.Add("AdvancedInstallXmlUnpack", "Xml Unpacker");
            German.Add("AdvancedInstallXmlUnpack", "XML Entpacker");
            Polish.Add("AdvancedInstallXmlUnpack", "XML Unpacker");
            French.Add("AdvancedInstallXmlUnpack", "Déballeur XML");
            Russian.Add("AdvancedInstallXmlUnpack", "XML-распаковщик");

            //Component: AdvancedInstallPatchFiles
            //
            English.Add("AdvancedInstallPatchFiles", "Patch files");
            German.Add("AdvancedInstallPatchFiles", "Patch Dateien");
            Polish.Add("AdvancedInstallPatchFiles", "Pliki poprawek (patch)");
            French.Add("AdvancedInstallPatchFiles", "Fichiers de patch");
            Russian.Add("AdvancedInstallPatchFiles", "Патч файла");

            //Component: AdvancedInstallCreateShortcuts
            //
            English.Add("AdvancedInstallCreateShortcuts", "Create Shortcuts");
            German.Add("AdvancedInstallCreateShortcuts", "Erstelle Verknüpfungen");
            Polish.Add("AdvancedInstallCreateShortcuts", "Utwórz skróty");
            French.Add("AdvancedInstallCreateShortcuts", "Créer des raccourcis");
            Russian.Add("AdvancedInstallCreateShortcuts", "Создать ярлыки");

            //Component: AdvancedInstallCreateAtlas
            //
            English.Add("AdvancedInstallCreateAtlas", "Create atlases");
            German.Add("AdvancedInstallCreateAtlas", "Erstelle Atlase");
            Polish.Add("AdvancedInstallCreateAtlas", "Utwórz atlas");
            French.Add("AdvancedInstallCreateAtlas", "Créer des atlas");
            Russian.Add("AdvancedInstallCreateAtlas", "Создать атласы");

            //Component: AdvancedInstallInstallFonts
            //
            English.Add("AdvancedInstallInstallFonts", "Install fonts");
            German.Add("AdvancedInstallInstallFonts", "Installiere Schriftarten");
            Polish.Add("AdvancedInstallInstallFonts", "Zainstaluj czcionki");
            French.Add("AdvancedInstallInstallFonts", "Installer des polices");
            Russian.Add("AdvancedInstallInstallFonts", "Установить шрифты");

            //Component: AdvancedInstallTrimDownloadCache
            //
            English.Add("AdvancedInstallTrimDownloadCache", "Trim download cache");
            German.Add("AdvancedInstallTrimDownloadCache", "Download-Cache kürzen");
            Polish.Add("AdvancedInstallTrimDownloadCache", "Ogranicz ściąganie plików cache");
            French.Add("AdvancedInstallTrimDownloadCache", "Réduire le cache de téléchargement");
            Russian.Add("AdvancedInstallTrimDownloadCache", "Очистить кэш загрузок");

            //Component: AdvancedInstallCleanup
            //
            English.Add("AdvancedInstallCleanup", "Cleanup");
            German.Add("AdvancedInstallCleanup", "Aufräumen");
            Polish.Add("AdvancedInstallCleanup", "Sprzątanie");
            French.Add("AdvancedInstallCleanup", "Nettoyer");
            Russian.Add("AdvancedInstallCleanup", "Очистка");

            #endregion

            #region News Viewer
            //Component: application_Update_TabHeader
            //
            English.Add("application_Update_TabHeader", "Application News");
            German.Add("application_Update_TabHeader", "App Neuigkeiten");
            Polish.Add("application_Update_TabHeader", TranslationNeeded);
            French.Add("application_Update_TabHeader", "Nouvelles d'application");
            Russian.Add("application_Update_TabHeader", "Новости приложения");

            //Component: database_Update_TabHeader
            //
            English.Add("database_Update_TabHeader", "Database News");
            German.Add("database_Update_TabHeader", "Datenbank Neuigkeiten");
            Polish.Add("database_Update_TabHeader", TranslationNeeded);
            French.Add("database_Update_TabHeader", "Nouvelles de la base de données");
            Russian.Add("database_Update_TabHeader", "Новости базы данных");
            #endregion

            #region Loading Window
            //Component: LoadingHeader
            //
            English.Add("LoadingHeader", "Loading, please wait");
            German.Add("LoadingHeader", "Lade, bitte warten");
            Polish.Add("LoadingHeader", TranslationNeeded);
            French.Add("LoadingHeader", "Chargement, veuillez patienter");
            Russian.Add("LoadingHeader", TranslationNeeded);
            #endregion

            #region First Load acks (yes i wrote that to avoid spelling the whole thing cause I may not know how even enough for auto correct to fix it)
            //Component: AgreementLicense
            //
            English.Add("AgreementLicense", "You have read and agree to the ");
            German.Add("AgreementLicense", "Sie haben Folgendes gelesen und zugestimmt: ");
            Polish.Add("AgreementLicense", TranslationNeeded);
            French.Add("AgreementLicense", "Vous avez lu et accepté le ");
            Russian.Add("AgreementLicense", "Вы прочли и согласны с ");

            //Component: LicenseLink
            //
            English.Add("LicenseLink", "License Agreement");
            German.Add("LicenseLink", "Lizenzvereinbarung");
            Polish.Add("LicenseLink", TranslationNeeded);
            French.Add("LicenseLink", "Contrat de licence");
            Russian.Add("LicenseLink", "условиями лицензионного соглашения");

            //Component: AgreementSupport1
            //
            English.Add("AgreementSupport1", "If you need support you can either visit our ");
            German.Add("AgreementSupport1", "Falls sie Unterstützung benötigen, besuchen Sie entweder unser ");
            Polish.Add("AgreementSupport1", TranslationNeeded);
            French.Add("AgreementSupport1", "Si vous avez besoin d'aide, vous pouvez soit visiter notre ");
            Russian.Add("AgreementSupport1", "Если понадобится помощь, вы смодете посетить наш ");

            //Component: AgreementSupportForums
            //
            English.Add("AgreementSupportForums", "Forums");
            German.Add("AgreementSupportForums", "Forum");
            Polish.Add("AgreementSupportForums", TranslationNeeded);
            French.Add("AgreementSupportForums", "forum");
            Russian.Add("AgreementSupportForums", "форум");

            //Component: AgreementSupport2
            //
            English.Add("AgreementSupport2", " or our ");
            German.Add("AgreementSupport2", " oder unseren ");
            Polish.Add("AgreementSupport2", TranslationNeeded);
            French.Add("AgreementSupport2", "ou notre");
            Russian.Add("AgreementSupport2", " или наш ");

            //Component: AgreementSupportDiscord
            //
            English.Add("AgreementSupportDiscord", "Discord");
            German.Add("AgreementSupportDiscord", "D15C0RD");
            Polish.Add("AgreementSupportDiscord", TranslationNeeded);
            French.Add("AgreementSupportDiscord", "Discord");
            Russian.Add("AgreementSupportDiscord", "Discord");

            //Component: AgreementHoster
            //
            English.Add("AgreementHoster", "I understand Relhax is a mod hosting and installation service and Relhax does not maintain every mod found in this Modpack");
            German.Add("AgreementHoster", "Ich verstehe, dass Relhax ein Mod-Hosting- und Installationsservice ist und Relhax nicht alle Mods verwaltet, die in diesem Modpack enthalten sind");
            Polish.Add("AgreementHoster", TranslationNeeded);
            French.Add("AgreementHoster", "Je comprends que Relhax est un service d'installation et d'hébergement de mods et Relhax ne gère pas tous les mods trouvés dans ce Modpack");
            Russian.Add("AgreementHoster", "Я понимаю, что Relhax является площадкой хостинга модов и сервисом их установки и то, что Relhax не занимается разработкой каждого мода из этого модпака");

            //Component: AgreementAnonData
            //
            English.Add("AgreementAnonData", "I understand that Relhax V2 collects anonymous usage data to improve the application, and can be disabled in the advanced settings tab");
            German.Add("AgreementAnonData", "Ich verstehe, dass Relhax V2 anonyme Nutzungsdaten sammelt, um die Anwendung zu verbessern, und auf der Registerkarte  für erweiterte Einstellungen deaktiviert werden kann");
            Polish.Add("AgreementAnonData", TranslationNeeded);
            French.Add("AgreementAnonData", "Je comprends que Relhax V2 collecte des données d'utilisation anonymes pour améliorer l'application et peut être désactivé dans l'onglet Paramètres avancés");
            Russian.Add("AgreementAnonData", "Я понимаю, что Relhax V2 собирает анонимные сведения об использовании для улучшения приложения и могу отключить сбор данных в разделе расширенных настроек");

            //Component: ContinueButton
            //
            English.Add("ContinueButton", "Continue");
            German.Add("ContinueButton", "Fortsetzen");
            Polish.Add("ContinueButton", TranslationNeeded);
            French.Add("ContinueButton", "Continuer");
            Russian.Add("ContinueButton", "Продолжить");

            //Component: V2UpgradeNoticeText
            //
            English.Add("V2UpgradeNoticeText", "It looks like you are running an upgrade from V1 to V2 for the first time." +
                " Pressing continue will result in an upgrade to file structure that cannot be reverted. It is recommended to make a backup of your V1 folder before continuing");
            German.Add("V2UpgradeNoticeText", "Es sieht so aus, als würden Sie zum ersten Mal ein Upgrade von V1 auf V2 ausführen. " + 
                " Wenn Sie auf Fortsetzen klicken, wird ein Upgrade der Dateistruktur durchgeführt, das nicht wiederhergestellt werden kann. Es wird empfohlen, eine Sicherungskopie Ihres V1-Ordners zu erstellen, bevor Sie fortfahren");
            Polish.Add("V2UpgradeNoticeText", TranslationNeeded);
            French.Add("V2UpgradeNoticeText", "Il semble que vous exécutiez une mise à niveau de V1 à V2 pour la première fois." +
                " Appuyer sur Continuer entraînera une mise à niveau de la structure de fichiers qui ne peut pas être annulée. Il est recommandé de faire une sauvegarde de votre dossier V1 avant de continuer");
            Russian.Add("V2UpgradeNoticeText", "Похоже, что вы производите апгрейд с V1 на V2 в первый раз. Нажатие кнопки продолжения произведёт обновление структуры файлов, которое невозможно откатить. Рекомендуется создание бэкапа папки с V1 перед продолжением");

            //Component: upgradingStructure
            //
            English.Add("upgradingStructure", "Upgrading V1 file and folder structure");
            German.Add("upgradingStructure", TranslationNeeded);
            Polish.Add("upgradingStructure", TranslationNeeded);
            French.Add("upgradingStructure", TranslationNeeded);
            Russian.Add("upgradingStructure", TranslationNeeded);
            #endregion

            #region Export Mode
            //Component: selectLocationToExport
            //
            English.Add("selectLocationToExport", "Select the folder to export the mod installation into");
            German.Add("selectLocationToExport", TranslationNeeded);
            Polish.Add("selectLocationToExport", TranslationNeeded);
            French.Add("selectLocationToExport", TranslationNeeded);
            Russian.Add("selectLocationToExport", TranslationNeeded);

            //Component: ExportSelectVersionHeader
            //
            English.Add("ExportSelectVersionHeader", "Please select the version of the WoT client you want to export for");
            German.Add("ExportSelectVersionHeader", TranslationNeeded);
            Polish.Add("ExportSelectVersionHeader", TranslationNeeded);
            French.Add("ExportSelectVersionHeader", TranslationNeeded);
            Russian.Add("ExportSelectVersionHeader", TranslationNeeded);

            //Component: ExportContinueButton
            //
            English.Add("ExportContinueButton", "Continue");
            German.Add("ExportContinueButton", TranslationNeeded);
            Polish.Add("ExportContinueButton", TranslationNeeded);
            French.Add("ExportContinueButton", TranslationNeeded);
            Russian.Add("ExportContinueButton", TranslationNeeded);

            //Component: ExportCancelButton
            //
            English.Add("ExportCancelButton", "Cancel");
            German.Add("ExportCancelButton", TranslationNeeded);
            Polish.Add("ExportCancelButton", TranslationNeeded);
            French.Add("ExportCancelButton", TranslationNeeded);
            Russian.Add("ExportCancelButton", TranslationNeeded);

            //Component: ExportModeMajorVersion
            //
            English.Add("ExportModeMajorVersion", "Online folder version");
            German.Add("ExportModeMajorVersion", TranslationNeeded);
            Polish.Add("ExportModeMajorVersion", TranslationNeeded);
            French.Add("ExportModeMajorVersion", TranslationNeeded);
            Russian.Add("ExportModeMajorVersion", TranslationNeeded);

            //Component: ExportModeMinorVersion
            //
            English.Add("ExportModeMinorVersion", "WoT version");
            German.Add("ExportModeMinorVersion", TranslationNeeded);
            Polish.Add("ExportModeMinorVersion", TranslationNeeded);
            French.Add("ExportModeMinorVersion", TranslationNeeded);
            Russian.Add("ExportModeMinorVersion", TranslationNeeded);
            #endregion

            #region Asking to close WoT
            //Component: WoTRunningTitle
            //
            English.Add("WoTRunningTitle", "WoT is Running");
            German.Add("WoTRunningTitle", "WoT wird gerade ausgeführt.");
            Polish.Add("WoTRunningTitle", "WoT jest uruchomiony");
            French.Add("WoTRunningTitle", "WoT est en cours d`éxecution");
            Russian.Add("WoTRunningTitle", "World of Tanks запущен");

            //Component: WoTRunningHeader
            //
            English.Add("WoTRunningHeader", "It looks like your WoT install is currently open. Please close it before we can proceed");
            German.Add("WoTRunningHeader", TranslationNeeded);
            Polish.Add("WoTRunningHeader", TranslationNeeded);
            French.Add("WoTRunningHeader", TranslationNeeded);
            Russian.Add("WoTRunningHeader", TranslationNeeded);

            //Component: WoTRunningCancelInstallButton
            //
            English.Add("WoTRunningCancelInstallButton", "Cancel Installation");
            German.Add("WoTRunningCancelInstallButton", TranslationNeeded);
            Polish.Add("WoTRunningCancelInstallButton", TranslationNeeded);
            French.Add("WoTRunningCancelInstallButton", TranslationNeeded);
            Russian.Add("WoTRunningCancelInstallButton", TranslationNeeded);

            //Component: WoTRunningRetryButton
            //
            English.Add("WoTRunningRetryButton", "Re-detect");
            German.Add("WoTRunningRetryButton", TranslationNeeded);
            Polish.Add("WoTRunningRetryButton", TranslationNeeded);
            French.Add("WoTRunningRetryButton", TranslationNeeded);
            Russian.Add("WoTRunningRetryButton", TranslationNeeded);

            //Component: WoTRunningForceCloseButton
            //
            English.Add("WoTRunningForceCloseButton", "Force close the game");
            German.Add("WoTRunningForceCloseButton", TranslationNeeded);
            Polish.Add("WoTRunningForceCloseButton", TranslationNeeded);
            French.Add("WoTRunningForceCloseButton", TranslationNeeded);
            Russian.Add("WoTRunningForceCloseButton", TranslationNeeded);
            #endregion

            #region Sacling Confirmation
            //Component: ScalingConfirmationHeader
            //
            English.Add("ScalingConfirmationHeader", "The scaling value has changed. Would you like to keep it?");
            German.Add("ScalingConfirmationHeader", TranslationNeeded);
            Polish.Add("ScalingConfirmationHeader", TranslationNeeded);
            French.Add("ScalingConfirmationHeader", TranslationNeeded);
            Russian.Add("ScalingConfirmationHeader", TranslationNeeded);

            //Component: ScalingConfirmationRevertTime
            //
            English.Add("ScalingConfirmationRevertTime", "Reverting in {0} Second(s)");
            German.Add("ScalingConfirmationRevertTime", TranslationNeeded);
            Polish.Add("ScalingConfirmationRevertTime", TranslationNeeded);
            French.Add("ScalingConfirmationRevertTime", TranslationNeeded);
            Russian.Add("ScalingConfirmationRevertTime", TranslationNeeded);

            //Component: ScalingConfirmationKeep
            //
            English.Add("ScalingConfirmationKeep", "Keep");
            German.Add("ScalingConfirmationKeep", TranslationNeeded);
            Polish.Add("ScalingConfirmationKeep", TranslationNeeded);
            French.Add("ScalingConfirmationKeep", TranslationNeeded);
            Russian.Add("ScalingConfirmationKeep", TranslationNeeded);

            //Component: ScalingConfirmationDiscard
            //
            English.Add("ScalingConfirmationDiscard", "Discard");
            German.Add("ScalingConfirmationDiscard", TranslationNeeded);
            Polish.Add("ScalingConfirmationDiscard", TranslationNeeded);
            French.Add("ScalingConfirmationDiscard", TranslationNeeded);
            Russian.Add("ScalingConfirmationDiscard", TranslationNeeded);
            #endregion

            //apply the bool
            TranslationsLoaded = true;
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
                //RelhaxHyperlink has text stored at the child textbox
                else if (control is UIComponents.RelhaxHyperlink link)
                {
                    link.Text = GetTranslatedString(componentName);
                    if (applyToolTips)
                    {
                        if (Exists(componentName + "Description"))
                            link.ToolTip = GetTranslatedString(componentName + "Description");
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
