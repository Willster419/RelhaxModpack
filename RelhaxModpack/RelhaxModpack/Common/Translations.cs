using RelhaxModpack.UI;
using RelhaxModpack.Windows;
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
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all localization for the application User Interface
    /// </summary>
    public static class Translations
    {
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
            //"DeveloperSettingsHeaderDescription",
            //"AutoSyncCheckFrequencyTextBox",
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
            "ApplicationUpdateNotes",
            //xml string forming in color picker
            "SampleXmlOutputTextbox",
            //{0} kb of {1} kb
            "GcDownloadStep4DownloadingSizes",
            "TaskName"
        };

        /// <summary>
        /// The entry to use when a translation is needed
        /// </summary>
        /// <remarks>When designing UI, i'll add the translation entries, but for the not english languages, i'll set
        /// this value so the application knows to return the english phrase and log the error.</remarks>
        public const string TranslationNeeded = "TODO";

        /// <summary>
        /// An array of all currently supported languages in the modpack
        /// </summary>
        /// <remarks>A supported language means that it has translation infrastructure and
        /// does not imply that all translations exist</remarks>
        public static readonly Languages[] SupportedLanguages =
        {
            Languages.English,
            Languages.French,
            Languages.German,
            Languages.Polish,
            Languages.Russian,
            Languages.Spanish
        };

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
        public const string LanguagePolish = "Polski"; // I know, adjectives in lowercase, but uppercasing for aesthetic reasons. @Nullmaruzero

        /// <summary>
        /// German language string identifier in national language
        /// </summary>
        public const string LanguageGerman = "Deutsch";

        /// <summary>
        /// French language string identifier in national language
        /// </summary>
        public const string LanguageFrench = "Français";

        /// <summary>
        /// Spanish language string identifier in national language
        /// </summary>
        public const string LanguageSpanish = "Español";

        /// <summary>
        /// Russian language string identifier in national language
        /// </summary>
        public const string LanguageRussian = "Pусский";

        private static Dictionary<string, string> English = new Dictionary<string, string>();
        private static Dictionary<string, string> German = new Dictionary<string, string>();
        private static Dictionary<string, string> Polish = new Dictionary<string, string>();
        private static Dictionary<string, string> French = new Dictionary<string, string>();
        private static Dictionary<string, string> Spanish = new Dictionary<string, string>();
        private static Dictionary<string, string> Russian = new Dictionary<string, string>();

        //default is to use English
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
                case Languages.Spanish:
                    CurrentLanguage = Spanish;
                    break;
                case Languages.Russian:
                    CurrentLanguage = Russian;
                    break;
            }
            ModpackSettings.Language = language;
        }

        /// <summary>
        /// Gets the language dictionary of the enumerated name of the language
        /// </summary>
        /// <param name="language">The english-named enumeration of the language</param>
        /// <returns>The key-value language dictionary</returns>
        public static Dictionary<string, string> GetLanguageDictionaries(Languages language)
        {
            switch (language)
            {
                case Languages.English:
                    return English;
                case Languages.French:
                    return French;
                case Languages.German:
                    return German;
                case Languages.Polish:
                    return Polish;
                case Languages.Spanish:
                    return Spanish;
                case Languages.Russian:
                    return Russian;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the native language name of the english enumerated name of the language
        /// </summary>
        /// <param name="language">The english-named enumeration of the language</param>
        /// <returns>The name of the requested language in it's native language</returns>
        public static string GetLanguageNativeName(Languages language)
        {
            switch (language)
            {
                case Languages.English:
                    return LanguageEnglish;
                case Languages.French:
                    return LanguageFrench;
                case Languages.German:
                    return LanguageGerman;
                case Languages.Polish:
                    return LanguagePolish;
                case Languages.Spanish:
                    return LanguageSpanish;
                case Languages.Russian:
                    return LanguageRussian;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Unloads the translation hashes
        /// </summary>
        public static void UnloadTranslations()
        {
            Logging.Debug(LogOptions.MethodName, "Unloading all language hashes and setting {0} to false", nameof(TranslationsLoaded));
            Dictionary<string, string>[] dics = { English, French, German, Polish, Spanish, Russian };
            foreach (Dictionary<string, string> dictionary in dics)
            {
                dictionary.Clear();
            }
            TranslationsLoaded = false;
            Logging.Debug(LogOptions.MethodName, "Unloaded all language hashes");
        }

        /// <summary>
        /// Reloads the translation hashes
        /// </summary>
        public static void ReloadTranslations()
        {
            Logging.Debug(LogOptions.MethodName, "Reloading all language hashes");
            UnloadTranslations();
            LoadTranslations();
            Logging.Debug(LogOptions.MethodName, "Reloaded all language hashes");
        }

        /// <summary>
        /// Get a localized string in the currently selected language
        /// </summary>
        /// <param name="componentName">The key value of the string phrase</param>
        /// <returns></returns>
        public static string GetTranslatedString(string componentName)
        {
            if (!TranslationsLoaded)
            {
                Logging.Error(LogOptions.MethodAndClassName, "Translations have not been loaded");
                return null;
            }

            string s;
            //check if componentName key exists in current language
            if (CurrentLanguage.ContainsKey(componentName))
            {
                s = CurrentLanguage[componentName];
                //if the value is TODO, check if we have it in english (unless it is english)
                if(s.Equals(TranslationNeeded))
                {
                    //Log warning it is todo in selected language
                    Logging.WriteToLog(string.Format("Missing translation key={0}, value=TODO, language={1}",
                        componentName, ModpackSettings.Language.ToString()),Logfiles.Application,LogLevel.Error);
                    s = English[componentName];
                    if(s.Equals(TranslationNeeded))
                    {
                        //Log error it is todo in english
                        Logging.WriteToLog(string.Format("Missing translation key={0}, value=TODO, language=English",
                            componentName), Logfiles.Application, LogLevel.Error);
                        s = componentName;
                    }
                }
            }
            else
            {
                //check if key exists in english (should not be the case 99% of the time)
                if(English.ContainsKey(componentName))
                {
                    Logging.WriteToLog(string.Format("Missing translation: key={0}, value=TODO, language={1}",
                        componentName, ModpackSettings.Language.ToString()), Logfiles.Application, LogLevel.Error);
                    s = English[componentName];
                    if (s.Equals(TranslationNeeded))
                    {
                        //Log error it is todo in english
                        Logging.WriteToLog(string.Format("Missing translation: key={0}, value=TODO, language=English",
                            componentName), Logfiles.Application, LogLevel.Error);
                    }
                }
                //Log error it does not exist
                Logging.WriteToLog(string.Format("Translation {0} does not exist in any languages",
                    componentName), Logfiles.Application, LogLevel.Error);
                s=componentName;
            }
            return s;
        }

        /// <summary>
        /// Determines if a key exists in the currently selected language, or in english if no language is selected
        /// </summary>
        /// <param name="componentName">The key of the component to look up</param>
        /// <param name="logError">Flag to log an error in the logfile if it does not exist</param>
        /// <returns>True if it exists in the currently selected language, or false otherwise</returns>
        public static bool ExistsInCurrentLanguage(string componentName, bool logError)
        {
            if (!TranslationsLoaded)
            {
                Logging.Error(LogOptions.MethodAndClassName, "Translations have not been loaded");
                return false;
            }

            if (CurrentLanguage == null)
            {
                Logging.Warning(LogOptions.MethodAndClassName, "CurrentLanguage is null, using english for default");
                return Exists(componentName, Languages.English);
            }

            if(!CurrentLanguage.ContainsKey(componentName))
            {
                if(logError)
                    Logging.Error("Missing translation: key={0}, value=MISSING_TRANSLATION, language={1}", componentName, ModpackSettings.Language.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks is a component (key value) exists in the given language (dictionary)
        /// </summary>
        /// <param name="componentName">The keyword phrase to check</param>
        /// <param name="languageToCheck">The language dictionary to check in</param>
        /// <returns>True is the entry exists, false otherwise</returns>
        public static bool Exists(string componentName, Languages languageToCheck)
        {
            if (!TranslationsLoaded)
            {
                Logging.Error(LogOptions.MethodAndClassName, "Translations have not been loaded");
                return false;
            }

            Dictionary<string, string> DictToCheck = null;
            switch (languageToCheck)
            {
                case Languages.English:
                    DictToCheck = English;
                    break;
                case Languages.French:
                    DictToCheck = French;
                    break;
                case Languages.Spanish:
                    DictToCheck = Spanish;
                    break;
                case Languages.German:
                    DictToCheck = German;
                    break;
                case Languages.Polish:
                    DictToCheck = Polish;
                    break;
                case Languages.Russian:
                    DictToCheck = Russian;
                    break;
                default:
                    DictToCheck = null;
                    break;
            }

            return DictToCheck.ContainsKey(componentName);
        }

        /// <summary>
        /// Checks that an entry exists and that the translated entry is not a TODO
        /// </summary>
        /// <param name="componentName">The language entry (key) to see if exists</param>
        /// <param name="langaugeToCheck">The language of which dictionary to check</param>
        /// <returns>True if the component (key) exists and the entry is not TODO</returns>
        public static bool ExistsAndValid(string componentName, Languages langaugeToCheck)
        {
            if(!TranslationsLoaded)
            {
                Logging.Error(LogOptions.MethodAndClassName, "Translations have not been loaded");
                return false;
            }

            if (!Exists(componentName, langaugeToCheck))
                return false;
            return !GetLanguageDictionaries(langaugeToCheck)[componentName].Equals(TranslationNeeded);
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
            //apply window title
            string typeName = window.GetType().Name;
            if (window is RelhaxWindow)
            {
                if (ExistsInCurrentLanguage(typeName, true))
                    window.Title = GetTranslatedString(typeName);
            }
            else if (window is MainWindow)
            {
                Logging.Debug("MainWindow Title localization skipped");
            }
            else
            {
                Logging.Warning("Window type {0} is not of RelhaxWindow but translation requested, skipping!", typeName);
                return;
            }

            //Get a list of all visual class controls curently presend and loaded in the window
            List<FrameworkElement> allWindowControls = UiUtils.GetAllWindowComponentsVisual(window, false);
            foreach (FrameworkElement v in allWindowControls)
            {
                TranslateComponent(v, true);
            }
        }


        private static void TranslateComponent(FrameworkElement frameworkElement, bool applyToolTips)
        {
            //check if component name is valid string
            string componentName = frameworkElement.Name;
            if (string.IsNullOrWhiteSpace(componentName))
            {
                //Logging.WriteToLog("Translation component name is blank", Logfiles.Application, LogLevel.Debug);
                return;
            }
            //first check name is none or on blacklist
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
                        if (ExistsInCurrentLanguage(headeredContentControl.Name + "Description",false))
                            headeredContentControl.ToolTip = GetTranslatedString(headeredContentControl.Name + "Description");
                    }
                }
                //RelhaxHyperlink has text stored at the child textbox
                else if (control is UI.RelhaxHyperlink link)
                {
                    link.Text = GetTranslatedString(componentName);
                    if (applyToolTips)
                    {
                        if (ExistsInCurrentLanguage(componentName + "Description",false))
                            link.ToolTip = GetTranslatedString(componentName + "Description");
                    }
                }
                //content controls have only a heder
                //NOTE: button is this type
                else if (control is ContentControl contentControl)
                {
                    //ALWAYS make sure that the header and content are of type string BEFORE over-writing! (what if it is an image?)
                    if (contentControl.Content is string)
                        contentControl.Content = GetTranslatedString(contentControl.Name);
                    if (applyToolTips)
                    {
                        if (ExistsInCurrentLanguage(contentControl.Name + "Description", false))
                            contentControl.ToolTip = GetTranslatedString(contentControl.Name + "Description");
                    }
                }
                //textbox only has string text as input
                else if (control is TextBox textBox)
                {
                    textBox.Text = GetTranslatedString(textBox.Name);
                    if (applyToolTips)
                    {
                        if (ExistsInCurrentLanguage(textBox.Name + "Description", false))
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
                    if (ExistsInCurrentLanguage(textBlock.Name + "Description", false))
                        textBlock.ToolTip = GetTranslatedString(textBlock.Name + "Description");
                }
            }
        }
        #endregion

        #region Literally loading Translations
        private static void AddTranslationToAll(string key, string message)
        {
            English.Add(key, message);
            German.Add(key, message);
            Polish.Add(key, message);
            French.Add(key, message);
            Spanish.Add(key, message);
            Russian.Add(key, message);
        }

        /// <summary>
        /// Loads all translation dictionaries. Should only be done once (at application start)
        /// </summary>
        public static void LoadTranslations()
        {
            Logging.Debug(LogOptions.MethodName, "Loading all translations");
            if(TranslationsLoaded)
            {
                Logging.Warning(LogOptions.MethodName, "Translations already loaded, use ReloadTranslations()");
                return;
            }

            //Syntax is as follows:
            //languageName.Add("componentName","TranslatedString");

            #region General expressions
            // Confirm "yes" button used in multiple places.
            // REFERRERS: VersionInfoYesButton;
            English.Add("yes", "yes");
            German.Add("yes", "ja");
            Polish.Add("yes", "Tak");
            French.Add("yes", "Oui");
            Spanish.Add("yes", "Sí");
            Russian.Add("yes", "Да");

            // Decline button used in multiple places.
            // REFERRERS: VersionInfoNoButton;
            English.Add("no", "no");
            German.Add("no", "nein");
            Polish.Add("no", "Nie");
            French.Add("no", "Non");
            Spanish.Add("no", "No");
            Russian.Add("no", "Нет");

            // Cancel button (phrase) used in multiple places.
            // REFERRERS: CancelDownloadInstallButton; CancelButtonLabel; DeveloperSelectionsCancelButton; ExportCancelButton; GcDownloadStep4DownloadingCancelButton;
            English.Add("cancel", "Cancel");
            German.Add("cancel", "Abbrechen");
            Polish.Add("cancel", "Anuluj");
            French.Add("cancel", "Annuler");
            Spanish.Add("cancel", "Cancelar");
            Russian.Add("cancel", "Отмена");

            English.Add("delete", "Delete");
            German.Add("delete", "Löschen");
            Polish.Add("delete", "Usuń");
            French.Add("delete", "Supprimer");
            Spanish.Add("delete", "Eliminar");
            Russian.Add("delete", "Удалить");

            English.Add("warning", "WARNING");
            German.Add("warning", "WARNUNG");
            Polish.Add("warning", "OSTRZEŻENIE");
            French.Add("warning", "ATTENTION");
            Spanish.Add("warning", "ATENCIÓN");
            Russian.Add("warning", "ВНИМАНИЕ");

            English.Add("critical", "CRITICAL");
            German.Add("critical", "KRITISCH");
            Polish.Add("critical", "OSTRZEŻENIE"); // Not as "critical error" but info. Dayum... @Nullmaruzero
            French.Add("critical", "CRITIQUAL");
            Spanish.Add("critical", "CRÍTICO");
            Russian.Add("critical", "КРИТИЧЕСКАЯ ОШИБКА");

            English.Add("information", "Information");
            German.Add("information", "Information");
            Polish.Add("information", "Informacja");
            French.Add("information", "information");
            Spanish.Add("information", "Información");
            Russian.Add("information", "Информация");

            English.Add("select", "Select");
            German.Add("select", "Auswählen");
            Polish.Add("select", "Wybierz");
            French.Add("select", "Sélectionner");
            Spanish.Add("select", "Seleccionar");
            Russian.Add("select", "Выбрать");

            English.Add("abort", "Abort");
            German.Add("abort", "Abbrechen");
            Polish.Add("abort", "Przerwij");
            French.Add("abort", "Abandonner");
            Spanish.Add("abort", "Abortar");
            Russian.Add("abort", "Отменить");

            // REFERRERS: extractionErrorHeader; GcDownloadStep1ValueError; 
            English.Add("error", "Error");
            German.Add("error", "Fehler");
            Polish.Add("error", "Błąd");
            French.Add("error", "Erreur");
            Spanish.Add("error", "Error");
            Russian.Add("error", "Ошибка");

            English.Add("retry", "Retry");
            German.Add("retry", "Wiederholen");
            Polish.Add("retry", "Ponów");
            French.Add("retry", "Réessayer");
            Spanish.Add("retry", "Reintentar");
            Russian.Add("retry", "Повторить");

            English.Add("ignore", "Ignore");
            German.Add("ignore", "Ignorieren");
            Polish.Add("ignore", "Ignoruj");
            French.Add("ignore", "Ignorer");
            Spanish.Add("ignore", "Ignorar");
            Russian.Add("ignore", "Игнорировать");

            English.Add("lastUpdated", "Last Updated: ");
            German.Add("lastUpdated", "Letzte Aktualisierung: ");
            Polish.Add("lastUpdated", "Ostatnio zaktualizowano: ");
            French.Add("lastUpdated", "Dernière mise à jour: ");
            Spanish.Add("lastUpdated", "Última actualización: ");
            Russian.Add("lastUpdated", "Последнее обновление: ");

            English.Add("stepsComplete", "tasks completed");
            German.Add("stepsComplete", "erledigte Aufgaben");
            Polish.Add("stepsComplete", "zadania zakończone");
            French.Add("stepsComplete", "tâches terminées");
            Spanish.Add("stepsComplete", "Operaciones completadas");
            Russian.Add("stepsComplete", "заданий выполнено");

            //Component: allFiles
            //
            English.Add("allFiles", "All files");
            German.Add("allFiles", "Alle Dateien");
            Polish.Add("allFiles", "Wszystkie pliki");
            French.Add("allFiles", "Tous les fichiers");
            Spanish.Add("allFiles", "Todos los archivos");
            Russian.Add("allFiles", "Все файлы");

            //Component: GoogleTranslateLanguageKey
            //
            English.Add("GoogleTranslateLanguageKey", "en");
            German.Add("GoogleTranslateLanguageKey", "de");
            Polish.Add("GoogleTranslateLanguageKey", "pl");
            French.Add("GoogleTranslateLanguageKey", "fr");
            Spanish.Add("GoogleTranslateLanguageKey", "es");
            Russian.Add("GoogleTranslateLanguageKey", "ru");

            //Component: at
            //example: downloading file x "at" 10MB/s
            English.Add("at", "at");
            German.Add("at", "bei");
            Polish.Add("at", "—"); // No short way for that. ALTERNATIVE: "z prędkością" (with [the] speed [of]) but it's clumsy and redundant. @Nullmaruzero
            French.Add("at", "à");
            Spanish.Add("at", "en");
            Russian.Add("at", "в");

            //Component: seconds
            // Used for download progress. Remaining time. Example: X minutes, Y seconds.
            English.Add("seconds", "seconds");
            German.Add("seconds", "sekunden");
            Polish.Add("seconds", "sekund(y)");
            French.Add("seconds", "secondes");
            Spanish.Add("seconds", "segundo(s)");
            Russian.Add("seconds", "сек.");

            //Component: minutes
            // Used for download progress (remaining time) AND AutoSyncFrequencyComboBox autoupdate check frequency comboBox (Minutes/Hours/Days).
            English.Add("minutes", "minutes");
            German.Add("minutes", "minuten");
            Polish.Add("minutes", "minut(y)");
            French.Add("minutes", "minute(s)");
            Spanish.Add("minutes", "minuto(s)");
            Russian.Add("minutes", "мин.");

            //Component: hours @ AutoSyncFrequencyComboBox
            // Used for AutoSyncCheckFrequencyTextBox autoupdate check frequency comboBox (Minutes/Hours/Days).
            English.Add("hours", "hours");
            German.Add("hours", "Stunden");
            Polish.Add("hours", "godzin(y)");
            French.Add("hours", "heures");
            Spanish.Add("hours", "hora(s)");
            Russian.Add("hours", "час.");

            //Component: days @ AutoSyncFrequencyComboBox
            // Used for AutoSyncFrequencyComboBox autoupdate check frequency comboBox (Minutes/Hours/Days).
            English.Add("days", "days");
            German.Add("days", "Tage");
            Polish.Add("days", "dni");
            French.Add("days", "jours");
            Spanish.Add("days", "día(s)");
            Russian.Add("days", "дн.");

            //Component: next
            // Used as a button in multiple places.
            // REFERRERS: PreviewNextPicButton; GcDownloadStep1NextText; GcDownloadStep2NextText; GcDownloadStep3NextText; GcDownloadStep4NextText;
            English.Add("next", "Next");
            German.Add("next", "Weiter");
            Polish.Add("next", "Dalej");
            French.Add("next", "Suivant");
            Spanish.Add("next", "Siguiente");
            Russian.Add("next", "Далее");

            //Component: ContinueButton
            // Continue button used in multiple places (at least this particular translation of it). Sometimes used as NEXT STEP, sometimes as NEXT ELEMENT.
            // REFERRERS: DiagnosticsContinueButton; ExportContinueButton; SelectLanguagesContinueButton;
            English.Add("ContinueButton", "Continue");
            German.Add("ContinueButton", "Fortsetzen");
            Polish.Add("ContinueButton", "Kontynuuj");
            French.Add("ContinueButton", "Continuer");
            Spanish.Add("ContinueButton", "Continuar");
            Russian.Add("ContinueButton", "Продолжить");

            //Component: previous
            // Previous button used in multiple places (at least this particular translation of it). Sometimes used as GO BACK, sometimes as PREVIOUS ELEMENT.
            // REFERRERS: PreviewPreviousPicButton; GcDownloadStep1PreviousText; GcDownloadStep2PreviousText; GcDownloadStep3PreviousText; GcDownloadStep4PreviousText;
            English.Add("previous", "Previous");
            German.Add("previous", "Zurück");
            Polish.Add("previous", "Wstecz");
            French.Add("previous", "Précedent");
            Spanish.Add("previous", "Anterior");
            Russian.Add("previous", "Назад");

            //Component: close
            // Close button used in multiple places.
            // REFFERERS: GcDownloadStep5CloseText;
            English.Add("close", "Close");
            German.Add("close", "Schließen");
            Polish.Add("close", "Zamknij");
            French.Add("close", "Fermer");
            Spanish.Add("close", "Cerrar");
            Russian.Add("close", "Закрыть");

            //Component: none
            // REFERRERS: SelectedInstallationNone;
            English.Add("none", "None");
            German.Add("none", "Nichts");
            Polish.Add("none", "Brak");
            French.Add("none", "Aucun");
            Spanish.Add("none", "Ninguna");
            Russian.Add("none", "Не выбрана");
            #endregion

            #region Application messages
            //Component: appFailedCreateLogfile
            //When the application first starts, it tries to open a logfile
            English.Add("appFailedCreateLogfile", "The application failed to open a logfile. Check your file permissions or move the application to a folder with write access.");
            German.Add("appFailedCreateLogfile", "Das Programm konnte eine Log-Datei nicht öffnen. Überprüfe die Berechtigungen oder verschiebe das Programm in einen Ordner mit Schreibrechten.");
            Polish.Add("appFailedCreateLogfile", "Aplikacja nie mogła otworzyć pliku dziennika. Sprawdź uprawnienia dostępu do pliku lub przenieś aplikację do folderu z dostępem do zapisu.");
            French.Add("appFailedCreateLogfile", "L'application à échouer à créer un fichier journal");
            Spanish.Add("appFailedCreateLogfile", "La aplicación no ha podido abrir un archivo de registro. Compruebe sus permisos de archivo o mueva la aplicación a una carpeta con permisos de escritura");
            Russian.Add("appFailedCreateLogfile", "Приложению не удалось открыть лог-файл. Проверьте права доступа к файлам или переместите приложение в папку, где разрешена запись.");

            //Component: failedToParse
            //
            English.Add("failedToParse", "Failed to parse the file");
            German.Add("failedToParse", "Die Datei konnte nicht verarbeitet werden");
            Polish.Add("failedToParse", "Plik nie mógł zostać przetworzony");
            French.Add("failedToParse", "Echec de l'analyse");
            Spanish.Add("failedToParse", "No se ha podido analizar el archivo");
            Russian.Add("failedToParse", "Сбой обработки файла");

            //Component: failedToGetDotNetFrameworkVersion
            //
            English.Add("failedToGetDotNetFrameworkVersion", "Failed to get the installed .NET Framework version. This could indicate a permissions problem or your antivirus software could be blocking it.");
            German.Add("failedToGetDotNetFrameworkVersion", TranslationNeeded);
            Polish.Add("failedToGetDotNetFrameworkVersion", "Brak zainstalowanego .NET Framework. Może to oznaczać problem z prawami dostępu lub konflikt z oprogramowaniem antywirusowym blokującym dostęp.");
            French.Add("failedToGetDotNetFrameworkVersion", TranslationNeeded);
            Spanish.Add("failedToGetDotNetFrameworkVersion", "No se ha podido obtener la versión de la instalación de .Net Framework. Esto puede indicar un problema de permisos, o un antivirus puede estar bloqueando la obtención.");
            Russian.Add("failedToGetDotNetFrameworkVersion", TranslationNeeded);

            //Component: invalidDotNetFrameworkVersion
            //
            English.Add("invalidDotNetFrameworkVersion", "The installed version of the .NET Framework is less then 4.8. Relhax Modpack requires version 4.8 or above to operate. Would you like to open a link" +
                "to get the latest version of the .NET Framework?");
            German.Add("invalidDotNetFrameworkVersion", TranslationNeeded);
            Polish.Add("invalidDotNetFrameworkVersion", "Zainstalowana wersja .NET Framework jest za stara. Modpack wymaga .NET Framework w wersji 4.8 lub wyższej." +
                "Czy chcesz przejść do strony pobierania najnowszej wersji .NET Framework?");
            French.Add("invalidDotNetFrameworkVersion", TranslationNeeded);
            Spanish.Add("invalidDotNetFrameworkVersion", "La versión instalada de .NET Framework es anterior a 4.8. Relhax Modpack requiere la versión 4.8 o superior para funcionar." +
                " ¿Quiere abrir un vínculo para obtener la última versión de .NET Framework?");
            Russian.Add("invalidDotNetFrameworkVersion", TranslationNeeded);
            #endregion

            #region Tray Icon
            //Component: MenuItemRestore
            //The menu item for restoring the application
            English.Add("MenuItemRestore", "Restore");
            German.Add("MenuItemRestore", "Wiederherstellen");
            Polish.Add("MenuItemRestore", "Przywróć");
            French.Add("MenuItemRestore", "Restaurer");
            Spanish.Add("MenuItemRestore", "Restaurar");
            Russian.Add("MenuItemRestore", "Восстановить");

            //Component: MenuItemCheckUpdates
            //The menu item for restoring the application
            English.Add("MenuItemCheckUpdates", "Check for Updates");
            German.Add("MenuItemCheckUpdates", "Nach Updates suchen");
            Polish.Add("MenuItemCheckUpdates", "Sprawdź aktualizacje");
            French.Add("MenuItemCheckUpdates", "Vérifier les mises à jour");
            Spanish.Add("MenuItemCheckUpdates", "Comprobar actualizaciones");
            Russian.Add("MenuItemCheckUpdates", "Проверить наличие обновлений");

            //Component: MenuItemAppClose
            //The menu item for restoring the application
            English.Add("MenuItemAppClose", English["close"]);
            German.Add("MenuItemAppClose", German["close"]);
            Polish.Add("MenuItemAppClose", Polish["close"]);
            French.Add("MenuItemAppClose", French["close"]);
            Spanish.Add("MenuItemAppClose", Spanish["close"]);
            Russian.Add("MenuItemAppClose", Russian["close"]);

            //Component: newDBApplied
            //MessageBox for when a new database version is applied
            English.Add("newDBApplied", "New database version applied");
            German.Add("newDBApplied", "Neue Datenbankversion angewendet");
            Polish.Add("newDBApplied", "Zastosowano nową bazę danych");
            French.Add("newDBApplied", "Nouvelle version de base de données appliquée");
            Spanish.Add("newDBApplied", "Aplicada nueva versión de la base de datos");
            Russian.Add("newDBApplied", "Применена новая версия базы данных");
            #endregion

            #region Main Window
            //Component: InstallModpackButton
            //The button for installing the modpack
            English.Add("InstallModpackButton", "Start mod selection");
            German.Add("InstallModpackButton", "Wähle Mods");
            Polish.Add("InstallModpackButton", "Wybierz mody do instalacji");
            French.Add("InstallModpackButton", "Sélection des mods");
            Spanish.Add("InstallModpackButton", "Comenzar selección de Mods");
            Russian.Add("InstallModpackButton", "Начать выбор модов");

            //Component: manualWoTFind
            // File picker window asking the user to select WorldOfTanks.exe manually.
            English.Add("selectWOTExecutable", "Select your WOT executable (WorldOfTanks.exe)");
            German.Add("selectWOTExecutable", "Wähle deine WoT Programmdatei (WorldOfTanks.exe)");
            Polish.Add("selectWOTExecutable", "Wybierz plik wykonywalny WOT (WorldOfTanks.exe)");
            French.Add("selectWOTExecutable", "Sélectionner votre éxécutable de WoT (WorldOfTanks.exe)");
            Spanish.Add("selectWOTExecutable", "Seleccione su ejecutable de WoT (WorldOfTanks.exe)");
            Russian.Add("selectWOTExecutable", "Выберите исполняемый файл игры (WorldOfTanks.exe)");

            //Component: InstallModpackButtonDescription
            //
            English.Add("InstallModpackButtonDescription", "Select the mods you want to install to your WoT client");
            German.Add("InstallModpackButtonDescription", "Wähle die Mods aus, die du auf deinem WoT Client installieren möchtest");
            Polish.Add("InstallModpackButtonDescription", "Wybierz mody, które chcesz zainstalować w swoim kliencie WoT.");
            French.Add("InstallModpackButtonDescription", "Sélectionnez les mods que vous souhaitez installer sur votre client WoT");
            Spanish.Add("InstallModpackButtonDescription", "Seleccione los Mods que quiere instalar a su cliente de WoT");
            Russian.Add("InstallModpackButtonDescription", "Выберите моды, которые вы хотите установить в клиент World of Tanks");

            //Component: UninstallModpackButton
            //
            English.Add("UninstallModpackButton", "Uninstall Relhax Modpack");
            German.Add("UninstallModpackButton", "Deinstalliere das Relhax Modpack");
            Polish.Add("UninstallModpackButton", "Odinstaluj Modpack Relhax");
            French.Add("UninstallModpackButton", "Désinstaller Relhax Modpack");
            Spanish.Add("UninstallModpackButton", "Desinstalar Relhax Modpack");
            Russian.Add("UninstallModpackButton", "Удалить модпак Relhax");

            //Component: UninstallModpackButtonDescription
            //
            English.Add("UninstallModpackButtonDescription", "Remove *all* mods installed to your WoT client");
            German.Add("UninstallModpackButtonDescription", "*Alle* Mods entfernen, die auf deinem WoT Client installiert sind");
            Polish.Add("UninstallModpackButtonDescription", "Usuń *wszystkie* zainstalowane mody.");
            French.Add("UninstallModpackButtonDescription", "Supprimer *tous* les mods installés sur votre client WoT");
            Spanish.Add("UninstallModpackButtonDescription", "Elimina *todos* los Mods installados en su cliente de WoT");
            Russian.Add("UninstallModpackButtonDescription", "Удаление *всех* установленных в клиент WoT модификаций");

            //Component: ViewNewsButton
            //
            English.Add("ViewNewsButton", "View update news");
            German.Add("ViewNewsButton", "Aktualisierungen");
            Polish.Add("ViewNewsButton", "Wiadomości o aktualizacjach");
            French.Add("ViewNewsButton", "Voir les mises à jour");
            Spanish.Add("ViewNewsButton", "Ver noticias de actualizaciones");
            Russian.Add("ViewNewsButton", "Последние обновления");

            //Component: ViewNewsButtonDescription
            //
            English.Add("ViewNewsButtonDescription", "View application, database, and other update news");
            German.Add("ViewNewsButtonDescription", "Anzeigen von Anwendungs-, Datenbank- und anderen Aktualisierungsnachrichten");
            Polish.Add("ViewNewsButtonDescription", "Przeczytaj wiadomości dot. aktualizacji aplikacji, bazy modów i innych.");
            French.Add("ViewNewsButtonDescription", "Afficher les actualités sur l'applications, les bases de données et autres");
            Spanish.Add("ViewNewsButtonDescription", "Ver noticias sobre actualizaciones de la aplicación, base de datos, y otros");
            Russian.Add("ViewNewsButtonDescription", "Показать новости об обновлениях приложения, БД и прочее");

            //Component: ForceManuelGameDetectionText
            //
            English.Add("ForceManuelGameDetectionText", "Force manual game detection");
            German.Add("ForceManuelGameDetectionText", "Erzwinge manuelle Spielerkennung");
            Polish.Add("ForceManuelGameDetectionText", "Wymuś ręczny wybór lokacji gry");
            French.Add("ForceManuelGameDetectionText", "Forcer détection manuel");
            Spanish.Add("ForceManuelGameDetectionText", "Forzar detección manual del cliente");
            Russian.Add("ForceManuelGameDetectionText", "Принудительно указать папку с игрой");

            //Component: ForceManuelGameDetectionCBDescription
            //
            English.Add("ForceManuelGameDetectionCBDescription", "This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");
            German.Add("ForceManuelGameDetectionCBDescription", "Diese Option ist für die manuelle selektion des World of Tanks Spiel-" +
                    "speicherortes. Nutze dies wenn Du Probleme mit der automatischen Suche des Spiels hast.");
            Polish.Add("ForceManuelGameDetectionCBDescription", "Wymusza ręczny wybór lokacji gry World of Tanks przez użytkownika.\n" +
                    "Zaznacz, jeśli występują problemy z automatycznym znalezieniem właściwej ścieżki gry.");
            French.Add("ForceManuelGameDetectionCBDescription", "Cette option consiste à forcer une détection manuel" +
                    "de World of Tanks. Sélectionnez cette option si vous rencontrez des problèmes pour localiser automatiquement le jeu.");
            Spanish.Add("ForceManuelGameDetectionCBDescription", "Esta opción es utilizada para forzar una detección manual de la" +
                    "ruta de instalación de World of Tanks. Marque esta casilla si tiene problemas encontrando el juego automáticamente.");
            Russian.Add("ForceManuelGameDetectionCBDescription", "Эта опция для принудительного указания папки с World of Tanks." +
                    "Поставьте галочку только в случае проблем с автоматическим определением расположения игры.");

            //Component: LanguageSelectionTextblock
            // A label for a ComboButton below with available languages.
            English.Add("LanguageSelectionTextblock", "Language selection");
            German.Add("LanguageSelectionTextblock", "Sprachauswahl");
            Polish.Add("LanguageSelectionTextblock", "Język:");
            French.Add("LanguageSelectionTextblock", "Choix de langue");
            Spanish.Add("LanguageSelectionTextblock", "Selección de idioma");
            Russian.Add("LanguageSelectionTextblock", "Выбрать язык");

            //Component: LanguageSelectionTextblockDescription
            // A tooltip for LanguageSelectionTextblock button label.
            English.Add("LanguageSelectionTextblockDescription", "Select your prefered language.\nIf you encounter missing translations or mistakes, feel free to inform us about them.");
            German.Add("LanguageSelectionTextblockDescription", "Wähle deine bevorzugte Sprache.\nFalls dir fehlende Übersetzungen auffallen, kannst du uns gerne darüber informieren.");
            Polish.Add("LanguageSelectionTextblockDescription", "Zmienia język aplikacji.\nJeśli napotkasz brakujące lub błędne tłumaczenia, możesz nam je zgłosić.");
            French.Add("LanguageSelectionTextblockDescription", "Sélectionner votre langage.\nSi vous rencontre des traductions manquantes ou des erreurs, n'hésitez pas à nous en informer.");
            Spanish.Add("LanguageSelectionTextblockDescription", "Seleccione su idioma de preferencia.\nSi encuentra alguna traducción faltante o error de traducción, infórmenos sobre ella.");
            Russian.Add("LanguageSelectionTextblockDescription", "Выберите желаемый язык.\nЕсли вы заметите неполноту перевода или ошибки, не стесняйтесь сообщать нам о них.");

            //Component: Forms_ENG_NAButtonDescription
            English.Add("Forms_ENG_NAButtonDescription", "Go to the English-speaking 'World of Tanks' forum page for the NA server");
            German.Add("Forms_ENG_NAButtonDescription", "Gehe zur englischsprachigen 'World of Tanks' Forum Seite für den NA Server");
            Polish.Add("Forms_ENG_NAButtonDescription", "Przejdź do anglojęzycznego forum 'World of Tanks' dla serwerów NA.");
            French.Add("Forms_ENG_NAButtonDescription", "Accéder au forum anglophone 'World of Tanks' pour le serveur NA");
            Spanish.Add("Forms_ENG_NAButtonDescription", "Acceder a la página en inglés del foro de 'World of Tanks' del servidor de NA");
            Russian.Add("Forms_ENG_NAButtonDescription", "Перейти на страницу модпака на форуме World of Tanks NA (страница на английском)");

            //Component: FormsENG_EUButtonDescription
            English.Add("Forms_ENG_EUButtonDescription", "Go to the English-speaking 'World of Tanks' forum page for the EU server");
            German.Add("Forms_ENG_EUButtonDescription", "Gehe zur englischsprachigen 'World of Tanks' Forum Seite für den EU Server");
            Polish.Add("Forms_ENG_EUButtonDescription", "Przejdź do anglojęzycznego forum 'World of Tanks' dla serwerów EU.");
            French.Add("Forms_ENG_EUButtonDescription", "Aller sur la page Anglophone du forum de World of Tanks pour le serveur EU");
            Spanish.Add("Forms_ENG_EUButtonDescription", "Acceder a la página en inglés del foro de 'World of Tanks' del servidor de EU");
            Russian.Add("Forms_ENG_EUButtonDescription", "Перейти на страницу модпака на форуме World of Tanks EU (страница на английском)");

            //Component: FormsENG_GERButtonDescription
            English.Add("Forms_GER_EUButtonDescription", "Go to the German-speaking 'World of Tanks' forum page for the EU server");
            German.Add("Forms_GER_EUButtonDescription", "Gehe zur deutschsprachigen 'World of Tanks' Forum Seite für den EU Server");
            Polish.Add("Forms_GER_EUButtonDescription", "Przejdź do niemieckojęzycznego forum 'World of Tanks' dla serwerów EU.");
            French.Add("Forms_GER_EUButtonDescription", "Aller sur la page Allemande du forum de World of Tanks pour le serveur EU");
            Spanish.Add("Forms_GER_EUButtonDescription", "Acceder a la página en alemán del foro de 'World of Tanks' del servidor de EU");
            Russian.Add("Forms_GER_EUButtonDescription", "Перейти на страницу модпака на форуме World of Tanks EU (страница на немецком)");

            //Component: SaveUserDataText
            //
            English.Add("SaveUserDataText", "Save user data");
            German.Add("SaveUserDataText", "Mod-Daten speichern");
            Polish.Add("SaveUserDataText", "Zapisz ustawienia użytkownika");
            French.Add("SaveUserDataText", "Sauvegarder les données utilisateur");
            Spanish.Add("SaveUserDataText", "Guardar datos del usuario");
            Russian.Add("SaveUserDataText", "Сохранить пользовательские данные");

            //Component:SaveUserDataCBDescription
            //
            English.Add("SaveUserDataCBDescription", "If this is selected, the installer will save user created data (like session stats from previous battles)");
            German.Add("SaveUserDataCBDescription", "Wenn dies ausgewählt ist, speichert das Installationsprogramm die vom Benutzer erstellten Daten" +
                " (wie Sitzungsstatistiken aus früheren Gefechten)");
            Polish.Add("SaveUserDataCBDescription", "Zachowuje dane użytkownika (takie jak statystyki sesji z poprzednich bitew) przy instalacji modów.");
            French.Add("SaveUserDataCBDescription", "Si cette option est sélectionnée, l'installateur vas sauvegarder les données créé par l'utilisateur" +
                " (Comme les stats de la session des batailles précédentes");
            Spanish.Add("SaveUserDataCBDescription", "Si esta opción está seleccionada, el instalador guardará datos creados por el usuario" +
                " (como estadísticas de sesión de batallas anteriores)");
            Russian.Add("SaveUserDataCBDescription", "Если выбрано, то установщик сохранит пользовательские данные (как сессионную статистику из предыдущих боев, к примеру)");


            //Component: CleanInstallText
            //
            English.Add("CleanInstallText", "Clean installation (recommended)");
            German.Add("CleanInstallText", "Saubere Installation (empfohlen)");
            Polish.Add("CleanInstallText", "Czysta instalacja (zalecane)");
            French.Add("CleanInstallText", "Installation propre (Recommandé)");
            Spanish.Add("CleanInstallText", "Instalación limpia (recomendado)");
            Russian.Add("CleanInstallText", "Чистая установка (рекомендуется)");

            //Component: CleanInstallCBDescription
            //
            English.Add("CleanInstallCBDescription", "This recommended option will uninstall your previous installation before installing the new one.");
            German.Add("CleanInstallCBDescription", "Diese empfohlene Option deinstalliert deine vorherige Installation, bevor du die neue installierst.");
            Polish.Add("CleanInstallCBDescription", "Odinstalowuje poprzednią instalację przed zainstalowaniem nowej. (ZALECANE)");
            French.Add("CleanInstallCBDescription", "Cette option recommandée va désinstaller votre installation précédente avant d'installer la nouvelle");
            Spanish.Add("CleanInstallCBDescription", "Esta opción recomendada desinstalará instalaciones anteriores antes de instalar la nueva");
            Russian.Add("CleanInstallCBDescription", "Данная рекомендуемая опция удалит ранее установленные моды перед установкой новых.");

            //Component: BackupModsText
            //
            English.Add("BackupModsText", "Backup current mods folder");
            German.Add("BackupModsText", "Sicherung des aktuellen Mod-Verzeichnis");
            Polish.Add("BackupModsText", "Stwórz kopię zapasową modów"); // Shortened. Kept splitting into two lines. @Nullmaruzero
            French.Add("BackupModsText", "Sauvegarder le dossier des mods");
            Spanish.Add("BackupModsText", "Crear una copia de seguridad de la carpeta actual de Mods");
            Russian.Add("BackupModsText", "Сделать резервную копию папки с модификациями");

            //Component: BackupModsSizeLabelUsed
            //
            English.Add("BackupModsSizeLabelUsed", "Backups: {0}  Size: {1}");
            German.Add("BackupModsSizeLabelUsed", "Sicherungen: {0}  Größe: {1}");
            Polish.Add("BackupModsSizeLabelUsed", "Kopie: {0}  Rozmiar: {1}");
            French.Add("BackupModsSizeLabelUsed", "Sauvegardes: {0} Taille: {1}");
            Spanish.Add("BackupModsSizeLabelUsed", "Copias de seguridad: {0}  Tamaño: {1}");
            Russian.Add("BackupModsSizeLabelUsed", "Бэкапов: {0} Размер: {1}");

            //Component: backupModsSizeCalculating
            //
            English.Add("backupModsSizeCalculating", "Calculating backups size...");
            German.Add("backupModsSizeCalculating", "Berechne Größe des Backups...");
            Polish.Add("backupModsSizeCalculating", "Obliczanie..."); // ASSUMED: Used as a temporary label instead of backup sizes.
            French.Add("backupModsSizeCalculating", "Calcul de la taille des backups...");
            Spanish.Add("backupModsSizeCalculating", "Calculando el tamaño de las copias de seguridad...");
            Russian.Add("backupModsSizeCalculating", "Вычисляется размер резервных копий...");

            //Component: BackupModsCBDescription
            //
            English.Add("BackupModsCBDescription", "Select this to make a backup of your current mods installation." +
                    "They are stored in the 'RelHaxModBackup' folder as a zip file, named by a time stamp.");
            German.Add("BackupModsCBDescription", "Wähle diese Option, um ein Backup deiner aktuellen Mod-Installation zu erstellen. "+
                     "Diese werden im Ordner 'RelHaxModBackup' als ZIP-Datei mit einem Zeitstempel gespeichert.");
            Polish.Add("BackupModsCBDescription", "Tworzy kopię zapasową wszystkich obecnie zainstalowanych modów i ich ustawień.\n" +
                     "Zostaną one spakowane w archiwum ZIP z sygnaturą czasową w nazwie i umieszczone w folderze „RelHaxModBackup”.");
            French.Add("BackupModsCBDescription", "Sélectionner ceci pour faire un backup de votre installation actuelle." +
                     "Les backups sont stockés dans le dossier 'RelHaxModBackup' en tant que .zip, nommé par un horodatage." );
            Spanish.Add("BackupModsCBDescription", "Seleccione esta opción para crear una copia de seguridad de los Mods actualmente instalados. " +
                    "Será almacenada en la carpeta 'RelHaxModBackup' como archivo zip, nombrado por una timestamp.");
            Russian.Add("BackupModsCBDescription", "Выберите для создания бэкапа имеющихся модов. Они будут находиться в папке 'RelHaxModBackup' в виде ZIP-архива и иметь в названии файла дату создания.");

            //Component: SaveLastInstallText
            //
            English.Add("SaveLastInstallText", "Save selection of last install");
            German.Add("SaveLastInstallText", "Speicherung der letzten Installation");
            Polish.Add("SaveLastInstallText", "Zapamiętaj ostatnią kolekcję");
            French.Add("SaveLastInstallText", "Sauvegarder la denière configuration");
            Spanish.Add("SaveLastInstallText", "Guardar selección de la instalación anterior");
            Russian.Add("SaveLastInstallText", "Запомнить выбранные моды");

            //Component: SaveLastInstallCBDescription
            //
            English.Add("SaveLastInstallCBDescription", "When enabled, the installer will automatically apply your last used selection");
            German.Add("SaveLastInstallCBDescription", "Wenn diese Option aktiviert ist, wendet das Installationsprogramm automatisch deine zuletzt verwendete Auswahl an");
            Polish.Add("SaveLastInstallCBDescription", "Zapisuje mody wybrane przy instalacji i automatycznie przywraca je na liście wyboru przy kolejnej instalacji.");
            French.Add("SaveLastInstallCBDescription", "Si activé, l'installateur appliquera automatiquement votre dernière sélection utilisée");
            Spanish.Add("SaveLastInstallCBDescription", "Si está activada, el instalador aplicará automáticamente su última selección utilizada");
            Russian.Add("SaveLastInstallCBDescription", "Если выбрано, то установщик автоматически выберет моды, указанные вами в прошлый раз");

            //Component: MinimizeToSystemTrayText
            //
            English.Add("MinimizeToSystemTrayText", "Minimize to system tray");
            German.Add("MinimizeToSystemTrayText", "Ins Benachrichtigungsfeld minimieren");
            Polish.Add("MinimizeToSystemTrayText", "Minimalizuj do zasobnika systemowego");
            French.Add("MinimizeToSystemTrayText", "Réduire dans la barre d'état système");
            Spanish.Add("MinimizeToSystemTrayText", "Minimizar a la bandeja del sistema");
            Russian.Add("MinimizeToSystemTrayText", "Свернуть в трей");

            //Component: MinimizeToSystemTrayDescription
            //
            English.Add("MinimizeToSystemTrayDescription", "When checked, the application will continue to run in the system tray when you press close");
            German.Add("MinimizeToSystemTrayDescription", "Wenn diese Option aktiviert ist, wird die Anwendung weiterhin im Benachtrichtigungsfeld ausgeführt, wenn du auf `Schließen` klickst.");
            Polish.Add("MinimizeToSystemTrayDescription", "Minimalizuje aplikację do zasobnika systemowego zamiast ją zamykać.");
            French.Add("MinimizeToSystemTrayDescription", "Si coché, l'application va continuer de s'éxécuter dans la barre d'état système");
            Spanish.Add("MinimizeToSystemTrayDescription", "Si está activada, la aplicación continuará funcionando en la bandeja del sistema al hacer clic en el botón de cerrar");
            Russian.Add("MinimizeToSystemTrayDescription", "Если выбрано, то при закрытии приложения оно продолжит работу в фоновом режиме");

            //Component: VerboseLoggingText
            //
            English.Add("VerboseLoggingText", "Verbose Logging");
            German.Add("VerboseLoggingText", "Ausführliche Protokollierung");
            Polish.Add("VerboseLoggingText", "Rozszerzone rejestrowanie zdarzeń");
            French.Add("VerboseLoggingText", "Enregistrement détaillé");
            Spanish.Add("VerboseLoggingText", "Registro detallado");
            Russian.Add("VerboseLoggingText", "Расширенное логгирование");

            //Component: VerboseLoggingCBDescription
            //
            English.Add("VerboseLoggingCBDescription", "Enable more logging messages in the log file. Useful for reporting bugs");
            German.Add("VerboseLoggingCBDescription", "Weitere Protokollmeldungen in der Protokolldatei aktivieren. Nützlich für das Melden von Fehlern.");
            Polish.Add("VerboseLoggingCBDescription", "Zapisuje pełne komunikaty zdarzeń do pliku dziennika. Przydatne przy zgłaszaniu błędów.");
            French.Add("VerboseLoggingCBDescription", "Activer plus de messages de journalisation dans le fichier journal. Utile pour signaler des bugs");
            Spanish.Add("VerboseLoggingCBDescription", "Activa más mensajes en el archivo de registro. Útil para reportar bugs");
            Russian.Add("VerboseLoggingCBDescription", "Увеличить объём собираемых данных для файла отчёта. Полезно для багрепортов");

            //Component: AllowStatsGatherText
            //
            English.Add("AllowStatsGatherText", "Allow statistics gathering of mod usage");
            German.Add("AllowStatsGatherText", "Sende Satistik zur Mod-Nutzung");
            Polish.Add("AllowStatsGatherText", "Wysyłaj anonimowe statystyki użytkowania");
            French.Add("AllowStatsGatherText", "Autoriser la collecte de statistiques sur l'utilisation du mod");
            Spanish.Add("AllowStatsGatherText", "Permitir recoleccón de estadísticas sobre el uso de Mods");
            Russian.Add("AllowStatsGatherText", "Разрешить сбор статистики об используемых модах");

            //Component: AllowStatsGatherCBDescription
            //
            English.Add("AllowStatsGatherCBDescription", "Allow the installer to upload anonymous statistic data to the server about mod selections. This allows us to prioritize our support");
            German.Add("AllowStatsGatherCBDescription", "Erlaube dem Installer, anonyme Statistikdaten über die Mod-Auswahl auf den Server hochzuladen. Dies ermöglicht es uns, unseren Support zu priorisieren");
            Polish.Add("AllowStatsGatherCBDescription", "Zezwala na zbieranie i wysyłanie anonimowych danych dot. wybieranych modów.\nDzięki temu możemy lepiej określić kierunek naszego wsparcia.");
            French.Add("AllowStatsGatherCBDescription", "Autoriser l'installateur à upload des statistiques de données anonymes au serveur sur la sélection de mods. Cela nous permet de hiérarchiser le support");
            Spanish.Add("AllowStatsGatherCBDescription", "Permite al instalador subir datos estadísticos anónimos al servidor sobre Mods seleccionados. Esto nos permite priorizar el soporte");
            Russian.Add("AllowStatsGatherCBDescription", "Позволить установщику собирать анонимные статистические данные на основе выбранных модов.\nЭто позволит нам расставить приоритеты по их поддержке");

            //Component: DisableTriggersText
            //
            English.Add("DisableTriggersText", "Disable Triggers");
            German.Add("DisableTriggersText", "Trigger deaktivieren");
            Polish.Add("DisableTriggersText", "Wyłącz wyzwalacze");
            French.Add("DisableTriggersText", "Désactiver les déclencheurs");
            Spanish.Add("DisableTriggersText", "Desactivar Desencadenantes");
            Russian.Add("DisableTriggersText", "Отключить триггеры");

            //Component: DisableTriggersCBDescription
            //
            English.Add("DisableTriggersCBDescription", "Allowing triggers can speed up the installation by running some tasks (like creating contour icons) during extraction " +
                "after all required resources for that task are ready. This is turned off automatically if User Mods are detected");
            German.Add("DisableTriggersCBDescription", "Das Zulassen von Triggern kann die Installation beschleunigen, indem einige Aufgaben ausgeführt werden (z. B. das Erstellen von Kontursymbolen), während extrahiert wird," +
                "sobald alle für diese Aufgabe erforderlichen Ressourcen verfügbar sind. Dies wird automatisch deaktiviert, wenn Benutzermodifikationen erkannt werden");
            Polish.Add("DisableTriggersCBDescription", "Przyśpiesza instalację wykonując pomniejsze zadania podczas wyodrębniania plików, " +
                 "gdy wszystkie wymagane czynności dla głównego zadania zostały już zakończone.\nWyzwalacze są automatycznie wyłączane przy instalowaniu własnych modów użytkownika.");
            French.Add("DisableTriggersCBDescription", "Autoriser les déclencheurs peut accélérer l’installation en exécutant certaines tâches (comme la création d’icônes de contour) au cours de l’extraction "+
                 "une fois que toutes les ressources requises pour cette tâche sont prêtes. Ceci est automatiquement désactivé si des mods utilisateur sont détectés");
            Spanish.Add("DisableTriggersCBDescription", "Permitir los Desencadenantes puede acelerar la instalación al ejecutar algunas tareas (como crear los iconos de contorno) durante la extracción " +
                "después de que todos los recursos para la operación estén disponibles. Se desactiva automáticamente si se detectan Mods del Usuario");
            Russian.Add("DisableTriggersCBDescription", "Включённые триггеры позволят ускорить установку, выполняя некоторые задачи (такие как создание контурных иконок) во время распаковки после того,\nкак все необходимые ресурсы готовы для этого. По умолчанию триггеры выключены при обнаружении пользовательских модов");

            //Component: CancelDownloadInstallButton
            //
            English.Add("CancelDownloadInstallButton", English["cancel"]);
            German.Add("CancelDownloadInstallButton", German["cancel"]);
            Polish.Add("CancelDownloadInstallButton", Polish["cancel"]);
            French.Add("CancelDownloadInstallButton", French["cancel"]);
            Spanish.Add("CancelDownloadInstallButton", Spanish["cancel"]);
            Russian.Add("CancelDownloadInstallButton", Russian["cancel"]);

            //Component: appDataFolderNotExistHeader
            //
            English.Add("appDataFolderNotExistHeader", "Could not detect WoT cache folder");
            German.Add("appDataFolderNotExistHeader", "Konnte den Cache-Ordner WoT nicht erkennen");
            Polish.Add("appDataFolderNotExistHeader", "Nie udało się znaleźć folderu pamięci podręcznej WoT");
            French.Add("appDataFolderNotExistHeader", "Impossible de détecter le dossier de cache WoT");
            Spanish.Add("appDataFolderNotExistHeader", "No se ha detectado la carpeta de caché de WoT");
            Russian.Add("appDataFolderNotExistHeader", "Невозможно найти папку кэша World of Tanks");


            //Component: appDataFolderNotExist
            //
            English.Add("appDataFolderNotExist", "The installer could not detect the WoT cache folder. Continue the installation without clearing WoT cache?");
            German.Add("appDataFolderNotExist", "Der Installer konnte den WoT-Cache-Ordner nicht erkennen. Installation fortsetzen ohne den WoT-Cache zu löschen?");
            Polish.Add("appDataFolderNotExist", "Instalator nie wykrył folderu pamięci podręcznej cache. Kontynuować bez opróżnienia folderu cache?");
            French.Add("appDataFolderNotExist", "L'installateur n'as pas pu détecter le dossier de cache WoT. Continuer l'installation sans nettoyer le cache?");
            Spanish.Add("appDataFolderNotExist", "El instalador no ha podido detectar la carpeta de caché de WoT. ¿Continuar la instalación sin limpiar la caché?");
            Russian.Add("appDataFolderNotExist", "Установщик не обнаружил папку кэша игры. Продолжить установку без очистки кэша?");

            //Component: viewAppUpdates
            //
            English.Add("viewAppUpdates", "View latest application updates");
            German.Add("viewAppUpdates", "Programmaktualisierungen anzeigen");
            Polish.Add("viewAppUpdates", "Wyświetl ostatnie aktualizacje aplikacji");
            French.Add("viewAppUpdates", "Afficher les dernières mises à jour de l'application");
            Spanish.Add("viewAppUpdates", "Ver las últimas actualicaciones de la aplicación");
            Russian.Add("viewAppUpdates", "Посмотреть последние обновления приложения");

            //Component: viewDBUpdates
            //
            English.Add("viewDBUpdates", "View latest database updates");
            German.Add("viewDBUpdates", "Datenbankaktualisierungen anzeigen");
            Polish.Add("viewDBUpdates", "Wyświetl ostatnie aktualizacje bazy danych");
            French.Add("viewDBUpdates", "Afficher les dernières mises à jour de la base de données");
            Spanish.Add("viewDBUpdates", "Ver las últimas actualizaciones de la base de datos");
            Russian.Add("viewDBUpdates", "Посмотреть последние обновления базы данных");

            //Component: EnableColorChangeDefaultV2Text
            //
            English.Add("EnableColorChangeDefaultV2Text", "Enable color change");
            German.Add("EnableColorChangeDefaultV2Text", "Farbwechsel");
            Polish.Add("EnableColorChangeDefaultV2Text", "Podświetlanie zmian");
            French.Add("EnableColorChangeDefaultV2Text", "Activer les changements de couleurs");
            Spanish.Add("EnableColorChangeDefaultV2Text", "Activar cambio de color");
            Russian.Add("EnableColorChangeDefaultV2Text", "Заменить цвета");

            //Component: EnableColorChangeDefaultV2CBDescription
            //
            English.Add("EnableColorChangeDefaultV2CBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");
            German.Add("EnableColorChangeDefaultV2CBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            Polish.Add("EnableColorChangeDefaultV2CBDescription", "Włącza podświatlenie wybranych i zmienionych elementów.");
            French.Add("EnableColorChangeDefaultV2CBDescription", "Activer le changement de les couleurs lors de la sélection d'un mod ou d'une config");
            Spanish.Add("EnableColorChangeDefaultV2CBDescription", "Activa el cambio de color al des/seleccionar un Mod o configuración");
            Russian.Add("EnableColorChangeDefaultV2CBDescription", "Включить замену цветов при выборе мода или конфигурации");


            //Component: EnableColorChangeLegacyText
            //
            English.Add("EnableColorChangeLegacyText", "Enable color change");
            German.Add("EnableColorChangeLegacyText", "Farbwechsel");
            Polish.Add("EnableColorChangeLegacyText", "Podświetlanie zmian");
            French.Add("EnableColorChangeLegacyText", "Activer les changements de couleurs");
            Spanish.Add("EnableColorChangeLegacyText", "Activar cambio de color");
            Russian.Add("EnableColorChangeLegacyText", "Заменить цвета");

            //Component: EnableColorChangeLegacyCBDescription
            //
            English.Add("EnableColorChangeLegacyCBDescription", "Enable the changing of colors when toggling the selection of a mod or config.");
            German.Add("EnableColorChangeLegacyCBDescription", "Die Farbe ändert sich, wenn man eine Auswahl getroffen hat");
            Polish.Add("EnableColorChangeLegacyCBDescription", "Włącza podświetlenie wybranych i zmienionych elementów.");
            French.Add("EnableColorChangeLegacyCBDescription", "Activer le changement de les couleurs lors de la sélection d'un mod ou d'une config");
            Spanish.Add("EnableColorChangeLegacyCBDescription", "Activa el cambio de color al des/seleccionar un Mod o configuración");
            Russian.Add("EnableColorChangeLegacyCBDescription", "Включить замену цветов при выборе мода или конфигурации");

            //Component: ShowOptionsCollapsedLegacyText
            //
            English.Add("ShowOptionsCollapsedLegacyText", "Show options collapsed on start");
            German.Add("ShowOptionsCollapsedLegacyText", "Optionen einklappen");
            Polish.Add("ShowOptionsCollapsedLegacyText", "Zwiń wszystkie opcje");
            French.Add("ShowOptionsCollapsedLegacyText", "Afficher les options du texte hérité réduit");
            Spanish.Add("ShowOptionsCollapsedLegacyText", "Colapsar todas las opciones al iniciar");
            Russian.Add("ShowOptionsCollapsedLegacyText", "Показывать опции свёрнутыми");

            //Component: ShowOptionsCollapsedLegacyCBDescription
            //
            English.Add("ShowOptionsCollapsedLegacyCBDescription", "When checked, all options in the selection list (except at the category level) will be collapsed.");
            German.Add("ShowOptionsCollapsedLegacyCBDescription", "Bei Auswahl wird die Optionen Liste bis auf die Hauptkategorien eingeklappt");
            Polish.Add("ShowOptionsCollapsedLegacyCBDescription", "Zwija wszystkie elementy drzewka wyboru na liście (z wyjątkiem kategorii).");
            French.Add("ShowOptionsCollapsedLegacyCBDescription", "Afficher les options du texte hérité réduit Legacy CB");
            Spanish.Add("ShowOptionsCollapsedLegacyCBDescription", "Cuando está marcada, todas las opciones de la lista de selección (excepto las de nivel categoría) serán colapsadas");
            Russian.Add("ShowOptionsCollapsedLegacyCBDescription", "Если включено, то списки модов для выбора (кроме категорий) будут свёрнуты.");


            //Component: ClearLogFilesText
            //
            English.Add("ClearLogFilesText", "Clear log files");
            German.Add("ClearLogFilesText", "Protokolldatei löschen");
            Polish.Add("ClearLogFilesText", "Wyczyść pliki dziennika");
            French.Add("ClearLogFilesText", "Effacer les fichiers logs");
            Spanish.Add("ClearLogFilesText", "Limpiar archivos de registro");
            Russian.Add("ClearLogFilesText", "Очистить логи");

            //Component: ClearLogFilesCBDescription
            //
            English.Add("ClearLogFilesCBDescription", "Clear the WoT log files, (python.log), as well as xvm log files (xvm.log) and pmod log files (pmod.log)");
            German.Add("ClearLogFilesCBDescription", "Löschen der WoT Protokolldatei, sowie XVM und PMOD Protokolldatei");
            Polish.Add("ClearLogFilesCBDescription", "Czyści pliki dziennika WoT (python.log), XVM'a (xvm.log) oraz pmod-ów (pmod.log).");
            French.Add("ClearLogFilesCBDescription", "Effacez les fichiers logs WoT (python.log), ainsi que les fichiers logs xvm (xvm.log) et les fichiers logs pmod (pmod.log)");
            Spanish.Add("ClearLogFilesCBDescription", "Limpia los archivos de registro del WoT (python.log), XVM (xvm.log), y PMOD (pmod.log)");
            Russian.Add("ClearLogFilesCBDescription", "Очистка логов World of Tanks (python.log), XVM (xvm.log) и PMOD (pmod.log).");


            //Component: CreateShortcutsText
            //
            English.Add("CreateShortcutsText", "Create desktop shortcuts");
            German.Add("CreateShortcutsText", "Erstelle Desktop Verknüpfungen");
            Polish.Add("CreateShortcutsText", "Utwórz skróty na pulpicie");
            French.Add("CreateShortcutsText", "Créer des raccourcis sur le bureau");
            Spanish.Add("CreateShortcutsText", "Crear accesos directos en el escritorio");
            Russian.Add("CreateShortcutsText", "Создать ярлыки рабочего стола");

            //Component: CreateShortcutsCBDescription
            //
            English.Add("CreateShortcutsCBDescription", "When selected, it will create shortcut icons on your desktop for mods that are exe files (like WWIIHA configuration)");
            German.Add("CreateShortcutsCBDescription", "Wenn diese Option aktiviert ist, werden bei der Installation die Verknüpfungen für \"World of Tanks\"," +
                " \"World of Tanks launcher\" und, wenn bei der Installation aktiviert, auch andere Verknüpfungen zu Konfigurationsprogrammen erstellt (z.B. WWIIHA Konfiguration)");
            Polish.Add("CreateShortcutsCBDescription", "Tworzy na pulpicie skróty do modyfikacji, które są plikami wykonywalnymi EXE (np. konfigurator moda WWIIHA).");
            French.Add("CreateShortcutsCBDescription", "Une fois sélectionné, L'installation créera des icônes de raccourci sur votre bureau pour les mods qui ont des" +
                " fichiers .exe (comme la configuration WWIIHA)");
            Spanish.Add("CreateShortcutsCBDescription", "Si está seleccionado, creará accesos directos en el escritorio para los mods que sean archivos .exe (p. ej. cofiguración de WWIIHA)");
            Russian.Add("CreateShortcutsCBDescription", "Если выбрано, то будут созданы ярлыки на рабочем столе для модов, являющимися EXE-файлами (как WWIIHA)");

            //Component: DeleteOldPackagesText
            //
            English.Add("DeleteOldPackagesText", "Delete old package files");
            German.Add("DeleteOldPackagesText", "Lösche alte Archiv-Dateien");
            Polish.Add("DeleteOldPackagesText", "Usuń stare pakiety");
            French.Add("DeleteOldPackagesText", "Supprimer les anciens packs de fichiers");
            Spanish.Add("DeleteOldPackagesText", "Eliminar paquetes de archivos antiguos");
            Russian.Add("DeleteOldPackagesText", "Удалить старые пакеты модов");

            //Component: DeleteOldPackagesCBDescription
            //
            English.Add("DeleteOldPackagesCBDescription", "Delete any zip files that are no longer used by the installer in the \"RelhaxDownloads\" folder to free up disk space");
            German.Add("DeleteOldPackagesCBDescription", "Lösche alle ZIP-Dateien im Ordner \"RelhaxDownloads\", welche vom Installationsprogramm nicht mehr verwendet werden, um Speicherplatz freizugeben.");
            Polish.Add("DeleteOldPackagesCBDescription", "Zwalnia miejsce na dysku usuwając stare i nieużywane archiwa ZIP z folderu „RelhaxDownloads”.");
            French.Add("DeleteOldPackagesCBDescription", "Supprimer tout les fichiers zip qui ne sont plus utilisés par l'installateur dans le dossier \"RelhaxDownloads\" pour libérer de la place sur le disque dur");
            Spanish.Add("DeleteOldPackagesCBDescription", "Elimina los archivos zip que ya no vayan a ser utilizados por el instalador en la carpeta \"RelHaxDownloads\" para liberar espacio en disco");
            Russian.Add("DeleteOldPackagesCBDescription", "Удалять ненужные установщику ZIP-архивы из папки \"RelhaxDownloads\" с целью освобождения места на диске");

            //Component: AutoInstallText
            //
            English.Add("AutoInstallText", "Enable auto install (NEW)");
            German.Add("AutoInstallText", "Automatische Installation (NEU)");
            Polish.Add("AutoInstallText", "Automatyczna instalacja (NOWOŚĆ)");
            French.Add("AutoInstallText", "Activer l'installation automatique (NOUVEAU)");
            Spanish.Add("AutoInstallText", "Habilitar instalación automática (NUEVO)");
            Russian.Add("AutoInstallText", "Включить автоустановку (НОВИНКА)");

            //Component: AutoInstallCBDescription
            //
            English.Add("AutoInstallCBDescription", "When a selection file and time is set below, the installer will automatically check for updates to your mods and apply them");
            German.Add("AutoInstallCBDescription", "Wenn unten eine Auswahldatei und eine Zeit eingestellt sind, sucht das Installationsprogramm automatisch nach Updates für deine Mods und wendet diese an.");
            Polish.Add("AutoInstallCBDescription", "Regularnie sprawdza dostępność aktualizacji modów i automatycznie je instaluje według pliku kolekcji i częstotliwości wybranych poniżej.");
            French.Add("AutoInstallCBDescription", "Quand un fichier et une heure sont définis ci-dessous, l'installateur va automatiquement chercher les mises à jour de vos mods et les appliquées");
            Spanish.Add("AutoInstallCBDescription", "Cuando se establece un archivo de selección y fecha abajo, el instalador buscará automáticamente actualizaciones a los Mods instalados y las aplicará");
            Russian.Add("AutoInstallCBDescription", "Установщик автоматически проверит наличие обновлений к модам в указанное время и применит их, основываясь на выбранной предустановке");

            //Component: OneClickInstallText
            //
            English.Add("OneClickInstallText", "Enable one-click install");
            German.Add("OneClickInstallText", "Ein-Klick-Installation");
            Polish.Add("OneClickInstallText", "Włącz instalację na kliknięcie");
            French.Add("OneClickInstallText", "Activer l'installation en un clique");
            Spanish.Add("OneClickInstallText", "Habilitar instalación en un clic");
            Russian.Add("OneClickInstallText", "Включить установку в один клик");

            //Component: OneClickInstallCBDescription
            //
            English.Add("OneClickInstallCBDescription", "Enable the installer to automatically load your selection file and install it");
            German.Add("OneClickInstallCBDescription", "Mit dieser Funktion wird deine Auswahldatei automatisch geladen und installiert wenn du auf den Wähle Mods Knopf drückst.");
            Polish.Add("OneClickInstallCBDescription", "Automatycznie wczytuje plik kolekcji i instaluje wybrane w nim mody.");
            French.Add("OneClickInstallCBDescription", "Autoriser l'installateur à automatiquement charger votre sélection de fichier et à l'installer");
            Spanish.Add("OneClickInstallCBDescription", "Permite al instalador cargar automáticamente el archivo de selección e instalarlo");
            Russian.Add("OneClickInstallCBDescription", "Позволить установщику автоматически запустить установку модов сразу после выбора предустановки");

            //Component: AutoOneclickShowWarningOnSelectionsFailText
            //
            English.Add("ForceEnabledCB", "Force all packages enabled [!]");
            German.Add("ForceEnabledCB", "Erzwinge, dass alle Pakete aktiviert sind [!]");
            Polish.Add("ForceEnabledCB", "Wymuś włącznie wszystkich pakietów [!]");
            French.Add("ForceEnabledCB", "Forcer tous les paquets activés [!]");
            Russian.Add("ForceEnabledCB", "Принудительно выбрать все пакеты [!]");
            //Component: AutoOneclickShowWarningOnSelectionsFailText
            //
            English.Add("AutoOneclickShowWarningOnSelectionsFailText", "Show warning if selection document has errors when loaded");
            German.Add("AutoOneclickShowWarningOnSelectionsFailText", "Warnung bei Fehler mit der Auswahldatei");
            Polish.Add("AutoOneclickShowWarningOnSelectionsFailText", "Ostrzeż w przypadku błędów"); // Shortened. Kept splitting into two lines. @Nullmaruzero
            French.Add("AutoOneclickShowWarningOnSelectionsFailText", "Le clique automatique montre un avertissement sur l'échec de la sélection du texte");
            Spanish.Add("AutoOneclickShowWarningOnSelectionsFailText", "Mostrar una advertencia si el documento de selección tiene errores al cargar");
            Russian.Add("AutoOneclickShowWarningOnSelectionsFailText", "Показывать предупреждение, если шаблон предустановки загружен с ошибками");

            //Component: AutoOneclickShowWarningOnSelectionsFailButtonDescription
            //
            English.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "When using one-click or auto install, show a warning to cancel install if any" +
                " errors occured when applying the selection file.");
            German.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "Zeige eine Warnung, wenn bei Benutzung der One-Click oder Auto-Install Funktion" +
                "ein Fehler mit der Auswahldatei auftritt");
            Polish.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "Ostrzega i umożliwia przerwanie w przypadku wystąpienia błędów z plikiem kolekcji.");
            French.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "Le clique automatique montre un avertissement sur la description du bouton d'échec de la sélection");
            Spanish.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "Cuando se utiliza instalación automática o en un clic, mostrar una advertencia para cancelar si ocurre algún error al aplicar el archivo de selección");
            Russian.Add("AutoOneclickShowWarningOnSelectionsFailButtonDescription", "При использовании функции установки в один клик или автоматической установки будет показываться предупреждение. У вас будет возможность прервать установку в случае появления ошибок.");

            //Component: ForceEnabledText
            //
            English.Add("ForceEnabledText", "Force all packages enabled [!]");
            German.Add("ForceEnabledText", "Aktiviere alle Pakete  [!]");
            Polish.Add("ForceEnabledText", "Odblokuj wszystkie pakiety [!]"); // Shortened. Kept splitting into two lines. @Nullmaruzero
            French.Add("ForceEnabledText", "Forcer tous les paquets activés [!]");
            Spanish.Add("ForceEnabledText", "Forzar habilitación de todos los paquetes [!]");
            Russian.Add("ForceEnabledText", "Принудительно выбрать все пакеты [!]");

            //Component: ForceEnabledCBDescription
            //
            English.Add("ForceEnabledCBDescription", "Causes all packages to be enabled. Can lead to severe stability issues of your installation");
            German.Add("ForceEnabledCBDescription", "Bewirkt, dass alle deaktivierten Pakete aktiviert werden. Kann zu schwerwiegenden Stabilitätsproblemen deiner Installation führen");
            Polish.Add("ForceEnabledCBDescription", "Odblokowuje wszystkie dezaktywowane pakiety.\nUWAGA: Może prowadzić do poważnych problemów ze stabilnością instalacji!");
            French.Add("ForceEnabledCBDescription", "Activer tout les paquets peut causer de lourds problèmes de stabilité de votre installation");
            Spanish.Add("ForceEnabledCBDescription", "Fuerza la habilitación de todos los paquetes. Puede causar problemas de inestabilidad severa de la instalación");
            Russian.Add("ForceEnabledCBDescription", "Отмечает все доступные к установке пакеты. Может привести к серьёзным проблемам со стабильностью");

            //Component: ForceVisibleText
            //
            English.Add("ForceVisibleText", "Force all packages visible [!]");
            German.Add("ForceVisibleText", "Alle Pakete sichtbar [!]");
            Polish.Add("ForceVisibleText", "Pokazuj wszystkie pakiety [!]"); // Shortened. Kept splitting into two lines. @Nullmaruzero
            French.Add("ForceVisibleText", "Forcer tout les paquets visible [!]");
            Spanish.Add("ForceVisibleText", "Forzar visibilidad de todos los paquetes [!]");
            Russian.Add("ForceVisibleText", "Принудительно показать все пакеты [!]");

            //Component: ForceVisibleCBDescription
            //
            English.Add("ForceVisibleCBDescription", "Causes all packages to be visible. Can lead to severe stability issues of your installation");
            German.Add("ForceVisibleCBDescription", "Bewirkt, dass alle ausgeblendeten Pakete sichtbar sind. Kann zu schwerwiegenden Stabilitätsproblemen deiner Installation führen");
            Polish.Add("ForceVisibleCBDescription", "Wyświetla na liście wyboru wszystkie ukryte pakiety i mody.\nUWAGA: Może prowadzić do poważnych problemów ze stabilnością instalacji!");
            French.Add("ForceVisibleCBDescription", "Activer tout les paquets visible peut causer de lourds problèmes de stabilité de votre installation");
            Spanish.Add("ForceVisibleCBDescription", "Fuerza a todos los paquetes a ser visibles. Puede causar problemas de inestabilidad severa de la instalación");
            Russian.Add("ForceVisibleCBDescription", "Показывает все скрытые пакеты. Может привести к серьёзным проблемам со стабильностью");

            //Component: LoadAutoSyncSelectionFileText
            // A button opening a OpenFileDialong system window for selecting the mod selection file. Used for One-Click and Auto-Install features.
            English.Add("LoadAutoSyncSelectionFileText", "Load selection file");
            German.Add("LoadAutoSyncSelectionFileText", "Auswahldatei laden");
            Polish.Add("LoadAutoSyncSelectionFileText", "Wczytaj plik kolekcji");
            French.Add("LoadAutoSyncSelectionFileText", "Charger la sélection de fichiers");
            Spanish.Add("LoadAutoSyncSelectionFileText", "Cargar archivo de selección");
            Russian.Add("LoadAutoSyncSelectionFileText", "Загрузить файл предустановки");

            //Component: LoadAutoSyncSelectionFileText
            // A tooltip for LoadAutoSyncSelectionFileText button.
            English.Add("LoadAutoSyncSelectionFileDescription", "Load your mods selection file to use the One-Click and Auto-Install features.");
            German.Add("LoadAutoSyncSelectionFileDescription", "Lade (d)eine Auswahldatei und nutze diese für die Ein-Klick und Auto-Install Funktionen. ");
            Polish.Add("LoadAutoSyncSelectionFileDescription", "Wczytuje wybrany plik kolekcji modów.\nUmożliwia używanie funkcji Instalacji na Kliknięcie oraz Automatycznej Instalacji.");
            French.Add("LoadAutoSyncSelectionFileDescription", "Chargement de votre sélection de fichiers de mods pour utiliser l'installation en un clique ainsi que l'installation automatique");
            Spanish.Add("LoadAutoSyncSelectionFileDescription", "Carga el archivo de mods seleccionados para usar las funciones de instalación automática o en un clic.");
            Russian.Add("LoadAutoSyncSelectionFileDescription", "Загрузите файл предустановки для использования автоматической установки или \"установки в один клик\"");

            //Component: AutoSyncCheckFrequencyTextBox
            // A label followed by a small text field asking for update check interval (time unit selected via a drop-list at the end of the line).
            // Frequency: every [ ... ] {Minutes / Hours / Days}
            English.Add("AutoSyncCheckFrequencyTextBox", "Frequency: every");
            German.Add("AutoSyncCheckFrequencyTextBox", "Häufigkeit: alle ");
            Polish.Add("AutoSyncCheckFrequencyTextBox", "Częstotliwość: co");
            French.Add("AutoSyncCheckFrequencyTextBox", "Fréquence: tout les");
            Spanish.Add("AutoSyncCheckFrequencyTextBox", "Frecuencia: cada");
            Russian.Add("AutoSyncCheckFrequencyTextBox", "Частота: каждые");

            //Component: DeveloperSettingsHeader
            // A header-label for multiple checkboxes with developer options below.
            English.Add("DeveloperSettingsHeader", "Developer Settings [!]");
            German.Add("DeveloperSettingsHeader", "Entwickleroptionen [!]");
            Polish.Add("DeveloperSettingsHeader", "Opcje programisty [!]");
            French.Add("DeveloperSettingsHeader", "Options de développeurs [!]");
            Spanish.Add("DeveloperSettingsHeader", "Opciones de desarrollador [!]");
            Russian.Add("DeveloperSettingsHeader", "Настройки для разработчиков [!]");

            //Component: DeveloperSettingsHeaderDescription
            // A tooltip for DeveloperSettingsHeader.
            English.Add("DeveloperSettingsHeaderDescription", "The options below may cause problems and stability issues!\nPlease, don't use them unless you know what you're doing!");
            German.Add("DeveloperSettingsHeaderDescription", "Die folgenden Optionen könnten Fehler und Instabilitäten verursachen!\nBitte wähle diese nur aus, wenn du weißt was du tust!");
            Polish.Add("DeveloperSettingsHeaderDescription", "Poniższe opcje mogą powodować problemy i niestabilność instalacji.\nUżywaj tylko jeśli wiesz co robisz!");
            French.Add("DeveloperSettingsHeaderDescription", "Les options au dessous peuvent causées des problèmes de stabilité ! \nS'il vous plaît, ne les utilisez pas si vous ne savez pas ce que vous faites !");
            Spanish.Add("DeveloperSettingsHeaderDescription", "¡Las opciones a continuación pueden causar problemas o inestabilidad!.\n¡Por favor, no las utilice a menos que sepa lo que está haciendo!");
            Russian.Add("DeveloperSettingsHeaderDescription", "Указанные ниже опции могут привести к нестабильному поведению игры и вызывать проблемы!\nПожалуйста, не используйте их, если не знаете, что делаете!");

            //Component: ApplyCustomScalingText
            //
            English.Add("ApplyCustomScalingText", "Application Scaling");
            German.Add("ApplyCustomScalingText", "Anwendungsskalierung");
            Polish.Add("ApplyCustomScalingText", "Skalowanie aplikacji");
            French.Add("ApplyCustomScalingText", "Mise à l'echelle de l'application");
            Spanish.Add("ApplyCustomScalingText", "Escalado de la aplicación");
            Russian.Add("ApplyCustomScalingText", "Мастшабирование приложения");

            //Component: EnableCustomFontCheckboxText
            //
            English.Add("EnableCustomFontCheckboxText", "Enable custom font");
            German.Add("EnableCustomFontCheckboxText", "Aktiviere eigene Schriftarten");
            Polish.Add("EnableCustomFontCheckboxText", "Użyj własnej czcionki");
            French.Add("EnableCustomFontCheckboxText", TranslationNeeded);
            Spanish.Add("EnableCustomFontCheckboxText", "Habilitar fuente personalizada");
            Russian.Add("EnableCustomFontCheckboxText", "Использовать другой шрифт");

            //Component: EnableCustomFontCheckboxTextDescription
            //
            English.Add("EnableCustomFontCheckboxTextDescription", "Enable using a custom font installed on your system inside most application windows");
            German.Add("EnableCustomFontCheckboxTextDescription", "Aktiviere eine deiner installierten Schriftarten für die Anzeige des Textes im Programm");
            Polish.Add("EnableCustomFontCheckboxTextDescription", "Pozwala wybrać czcionkę używaną w większości okien spośród listy zainstalowanych w systemie czcionek");
            French.Add("EnableCustomFontCheckboxTextDescription", TranslationNeeded);
            Spanish.Add("EnableCustomFontCheckboxTextDescription", "Habilita una fuente personalizada instalada en su sistema en la mayoría de las ventanas de la aplicación");
            Russian.Add("EnableCustomFontCheckboxTextDescription", "Вы можете выбрать любой установленный в системе шрифт,\nкоторый будет использован почти во всех окнах программы");

            //Component: LauchEditorText
            //button for launching the editor from the main application window
            English.Add("LauchEditorText", "Launch Database Editor");
            German.Add("LauchEditorText", "Starte Datenbank Editor");
            Polish.Add("LauchEditorText", "Uruchom edytor bazy danych");
            French.Add("LauchEditorText", "Lancer l'éditeur de la base de données");
            Spanish.Add("LauchEditorText", "Iniciar editor de la base de datos");
            Russian.Add("LauchEditorText", "Запустить редактор БД");

            //Component: LauchPatchDesignerText
            //button for launching the editor from the main application window
            English.Add("LauchPatchDesignerText", "Launch Patch Designer");
            German.Add("LauchPatchDesignerText", "Starte Patch Designer");
            Polish.Add("LauchPatchDesignerText", "Uruchom Patch Designer");
            French.Add("LauchPatchDesignerText", "Lancer le Patch Designer");
            Spanish.Add("LauchPatchDesignerText", "Iniciar diseñador de parche");
            Russian.Add("LauchPatchDesignerText", "Запустить конструктор патчей");

            //Component: LauchEditorDescription
            //button for launching the editor from the main application window
            English.Add("LauchEditorDescription", "Launch the Database Editor from here, instead of from command line");
            German.Add("LauchEditorDescription", "Starte den Datenbank Editor von hier, anstatt über die Befehlszeile");
            Polish.Add("LauchEditorDescription", "Uruchomi edytor bazy danych stąd, zamiast poprzez wiersz poleceń.");
            French.Add("LauchEditorDescription", "Lancer l'éditeur de la base de données d'ici, au lieu d'une ligne de commande");
            Spanish.Add("LauchEditorDescription", "Inicia el editor de la base de datos desde aquí, en lugar de desde la línea de comandos");
            Russian.Add("LauchEditorDescription", "Запуск редактора базы данных непосредственно здесь, а не в коммандной строке");

            //Component: ApplyCustomScalingTextDescription
            // A tooltip for a ApplyCustomScalingText label. Describes a slider below used for changing application scaling.
            English.Add("ApplyCustomScalingTextDescription", "Apply display scaling to the installer windows");
            German.Add("ApplyCustomScalingTextDescription", "Wende die Anzeigeskalierung auf das Installationsfenster an");
            Polish.Add("ApplyCustomScalingTextDescription", "Zastosuj skalowanie ekranu do okien instalatora.");
            French.Add("ApplyCustomScalingTextDescription", "Appliquer la mise à l'échelle de l'affichage aux fenêtres du programme d'installation");
            Spanish.Add("ApplyCustomScalingTextDescription", "Aplica esta escala de visualización a las ventanas del instalador");
            Russian.Add("ApplyCustomScalingTextDescription", "Применить масштабирование дисплея к окнам установщика");

            //Component: InstallWhileDownloadingText
            //
            English.Add("InstallWhileDownloadingText", "Extract while downloading");
            German.Add("InstallWhileDownloadingText", "Entpacke während des Downloads");
            Polish.Add("InstallWhileDownloadingText", "Wypakuj podczas pobierania");
            French.Add("InstallWhileDownloadingText", "Extraire pendant le téléchargement");
            Spanish.Add("InstallWhileDownloadingText", "Extraer durante la descarga");
            Russian.Add("InstallWhileDownloadingText", "Распаковка во время скачивания");

            //Component: InstallWhileDownloadingCBDescription
            //
            English.Add("InstallWhileDownloadingCBDescription", "When enabled, the installer will extract a zip file as soon as it is downloaded," +
                " rather than waiting for every zip file to be downloaded before extraction.");
            German.Add("InstallWhileDownloadingCBDescription", "Wenn aktiviert, der Installer wird die Zip-Dateien sofort nach dem Download entpacken" +
                " und nicht erst auf das Herunterladen aller Dateien warten bevor mit dem Entpacken begonnen wird.");
            Polish.Add("InstallWhileDownloadingCBDescription", "Wyodrębnia każdy pakiet zaraz po jego pobraniu, zamiast po pobraniu wszystkich pakietów.");
            French.Add("InstallWhileDownloadingCBDescription", "Si activé, l'installateur va extraire un fichier zip dès qu'il est télécharger, au lieu" +
                " d'attendre que chaque fichier zip soit télécharger pour l'extraction.");
            Spanish.Add("InstallWhileDownloadingCBDescription", "Cuando está habilitada, el instalador extraerá cada archivo zip tan pronto como se descargue" +
                " en lugar de esperar a que todos los archivos sean descargados para la extracción.");
            Russian.Add("InstallWhileDownloadingCBDescription", "Если включено, то установщик будет распаковывать ZIP-архив сразу после скачивания,\nвместо того," +
                " чтобы ждать окончания загрузки всех файлов перед распаковкой.");

            //Component: MulticoreExtractionCoresCountLabel
            //
            English.Add("MulticoreExtractionCoresCountLabel", "Detected Cores: {0}");
            German.Add("MulticoreExtractionCoresCountLabel", "Erkannte Kerne: {0}");
            Polish.Add("MulticoreExtractionCoresCountLabel", "Rdzenie CPU: {0}");
            French.Add("MulticoreExtractionCoresCountLabel", "Coeurs détecter: {0}");
            Spanish.Add("MulticoreExtractionCoresCountLabel", "Núcleos detectados: {0}");
            Russian.Add("MulticoreExtractionCoresCountLabel", "Обнаружено ядер: {0}");

            //Component: MulticoreExtractionCoresCountLabelDescription
            //
            English.Add("MulticoreExtractionCoresCountLabelDescription", "Number of logical CPU cores (threads) detected on your system");
            German.Add("MulticoreExtractionCoresCountLabelDescription", "Anzahl der auf deinem System erkannten logischen CPU-Kerne (Threads)");
            Polish.Add("MulticoreExtractionCoresCountLabelDescription", "Liczba rdzeni logicznych procesora (wątków) wykrytych w systemie.");
            French.Add("MulticoreExtractionCoresCountLabelDescription", "Nombre de cœurs de processeur logiques (threads) détectés sur votre système");
            Spanish.Add("MulticoreExtractionCoresCountLabelDescription", "Número de núcleos lógicos (hilos) de CPU detectados en su sistema");
            Russian.Add("MulticoreExtractionCoresCountLabelDescription", "Количество логических процессоров (потоков), обнаруженных вашей системой");

            //Component: SaveDisabledModsInSelectionText
            //
            English.Add("SaveDisabledModsInSelectionText", "Keep disabled mods when saving selection");
            German.Add("SaveDisabledModsInSelectionText", "Behalte deaktivierte Mods beim Speichern der Auswahl");
            Polish.Add("SaveDisabledModsInSelectionText", "Zachowaj dezaktywowane mody w kolekcji");
            French.Add("SaveDisabledModsInSelectionText", "Garder les mods désactivés pendant la sauvegarde de la sélection");
            Spanish.Add("SaveDisabledModsInSelectionText", "Conservar los mods deshabilitados cuando se guarde la selección");
            Russian.Add("SaveDisabledModsInSelectionText", "Запоминать отключённые моды при сохранении предустановки");

            //Component: SaveDisabledModsInSelectionDescription
            //
            English.Add("SaveDisabledModsInSelectionDescription", "When a mod is re-enabled, it will be selected from your selection file");
            German.Add("SaveDisabledModsInSelectionDescription", "Wenn ein Mod wieder aktiviert wird, wird er aus deiner Auswahldatei ausgewählt");
            Polish.Add("SaveDisabledModsInSelectionDescription", "Zaznacza poprzednio wybrane, ale zablokowane przez nas mody, po ich ponownym odblokowaniu.");
            French.Add("SaveDisabledModsInSelectionDescription", "Quand un mod est réactivé, il sera sélectionné depuis votre sélection de fichiers");
            Spanish.Add("SaveDisabledModsInSelectionDescription", "Cuando un mod sea rehabilitado, será seleccionado desde su archivo de selección");
            Russian.Add("SaveDisabledModsInSelectionDescription", "Когда мод будет включён в БД, он снова будет выбран из вашей предустановки");

            //Component: AdvancedInstallationProgressText
            //
            English.Add("AdvancedInstallationProgressText", "Show advanced installation progress window");
            German.Add("AdvancedInstallationProgressText", "Erweitertes Installationsfenster");
            Polish.Add("AdvancedInstallationProgressText", "Wyświetl szczegółowy podgląd instalacji");
            French.Add("AdvancedInstallationProgress", "Voir la fenêtre d'installation avancée");
            Spanish.Add("AdvancedInstallationProgressText", "Mostrar ventana de instalación avanzada");
            Russian.Add("AdvancedInstallationProgressText", "Показывать больше подробностей в окне прогресса установки");

            //Component: AdvancedInstallationProgressDescription
            //
            English.Add("AdvancedInstallationProgressDescription", "Shows an advanced installation window during extraction, useful when you have multicore extraction enabled");
            German.Add("AdvancedInstallationProgressDescription", "Zeigt während der Extraktion ein erweitertes Installationsfenster an, das nützlich ist, wenn die Multicore-Extraktion aktiviert ist");
            Polish.Add("AdvancedInstallationProgressDescription", "Wyświetla szczegółowe okno procesu instalacji. Przydatne przy włączonym wyodrębnianiu wielordzeniowym.");
            French.Add("AdvancedInstallationProgressDescription", "Montrer une fenêtre d'installation avancée pendant l'extraction, utile quand vous avez l'extraction multicoeurs activée");
            Spanish.Add("AdvancedInstallationProgressDescription", "Muestra una ventana de instalación avanzada durante la extracción, útil cuando la extración multinúcleo está habilitada");
            Russian.Add("AdvancedInstallationProgressDescription", "Показывает более подробное окно прогресса установки. Полезно при включённой многопоточной установке");

            //Component: ThemeSelectText
            // A label with 3 radio buttons underneach for selecting app's color theme.
            English.Add("ThemeSelectText", "Select theme:");
            German.Add("ThemeSelectText", "Wähle Thema");
            Polish.Add("ThemeSelectText", "Wybierz motyw:");
            French.Add("ThemeSelectText", "Sélectionner un thème");
            Spanish.Add("ThemeSelectText", "Seleccione tema:");
            Russian.Add("ThemeSelectText", "Выберите тему");

            //Component: ThemeDefaultText
            //
            English.Add("ThemeDefaultText", "Default");
            German.Add("ThemeDefaultText", "Standard");
            Polish.Add("ThemeDefaultText", "Domyślny");
            French.Add("ThemeDefaultText", "Standard");
            Spanish.Add("ThemeDefaultText", "Estándar");
            Russian.Add("ThemeDefaultText", "Стандартная");

            //Component: ThemeDefaultDescription
            //
            English.Add("ThemeDefaultDescription", "Default Theme");
            German.Add("ThemeDefaultDescription", "Standard Thema");
            Polish.Add("ThemeDefaultDescription", "Domyślny schemat kolorów.");
            French.Add("ThemeDefaultDescription", "Thème standard");
            Spanish.Add("ThemeDefaultDescription", "Tema por defecto");
            Russian.Add("ThemeDefaultDescription", "Стандартная тема");

            //Component: ThemeDarkText
            //
            English.Add("ThemeDarkText", "Dark");
            German.Add("ThemeDarkText", "Dunkel");
            Polish.Add("ThemeDarkText", "Ciemny");
            French.Add("ThemeDarkText", "Sombre");
            Spanish.Add("ThemeDarkText", "Oscuro");
            Russian.Add("ThemeDarkText", "Тёмная");

            //Component: ThemeDarkDescription
            //
            English.Add("ThemeDarkDescription", "Dark Theme");
            German.Add("ThemeDarkDescription", "Dunkles Thema");
            Polish.Add("ThemeDarkDescription", "Ciemny schemat kolorów.");
            French.Add("ThemeDarkDescription", "Thème sombre");
            Spanish.Add("ThemeDarkDescription", "Tema oscuro");
            Russian.Add("ThemeDarkDescription", "Тёмная тема");

            //Component: ThemeCustomText
            //
            English.Add("ThemeCustomText", "Custom");
            German.Add("ThemeCustomText", "Benutzerdefiniert");
            Polish.Add("ThemeCustomText", "Własny");
            French.Add("ThemeCustom", "Personnaliser");
            Spanish.Add("ThemeCustomText", "Personalizado");
            Russian.Add("ThemeCustomText", "Сторонняя");

            //Component: ThemeCustomDescription
            //
            English.Add("ThemeCustomDescription", "Custom Theme");
            German.Add("ThemeCustomDescription", "Benutzerdefiniertes Thema");
            Polish.Add("ThemeCustomDescription", "Własny, niestandardowy schemat kolorów.");
            French.Add("ThemeCustomDescription", "Thème personnaliser");
            Spanish.Add("ThemeCustomDescription", "Tema personalizado");
            Russian.Add("ThemeCustomDescription", "Сторонняя тема");

            //Component: DumpColorSettingsButtonText
            //
            English.Add("DumpColorSettingsButtonText", "Save current color settings");
            German.Add("DumpColorSettingsButtonText", "Speichere derzeitige Farbeinstellungen");
            Polish.Add("DumpColorSettingsButtonText", "Zapisz obecny schemat kolorów");
            French.Add("DumpColorSettingsButtonText", "Texte du bouton des options des couleurs de vidage");
            Spanish.Add("DumpColorSettingsButtonText", "Guardar configuración de colores");
            Russian.Add("DumpColorSettingsButtonText", "Сохранить текущие параметры цвета");

            //Component: DumpColorSettingsSaveSuccess
            //
            English.Add("DumpColorSettingsSaveSuccess", "Color settings saved");
            German.Add("DumpColorSettingsSaveSuccess", "Farbeinstellungen gespeichert");
            Polish.Add("DumpColorSettingsSaveSuccess", "Schemat kolorów zapisany pomyślnie");
            French.Add("DumpColorSettingsSaveSuccess", "Options des couleurs de vidage sauvergardées avec succès");
            Spanish.Add("DumpColorSettingsSaveSuccess", "Configuración de colores guardada");
            Russian.Add("DumpColorSettingsSaveSuccess", "Параметры цвета успешно сохранены");

            //Component: OpenColorPickerButtonText
            //
            English.Add("OpenColorPickerButtonText", "Open color picker");
            German.Add("OpenColorPickerButtonText", "Öffne Farbauswahl");
            Polish.Add("OpenColorPickerButtonText", "Ustawienia schematu kolorów");
            French.Add("OpenColorPickerButtonText", "Ouvrir le texte du bouton sélecteur de couleur");
            Spanish.Add("OpenColorPickerButtonText", "Abrir selector de colores");
            Russian.Add("OpenColorPickerButtonText", "Открыть палитру");

            //Component: OpenColorPickerButtonDescription
            // A tooltip for OpenColorPickerButton. The button opens the color picker for theming the app. *Captain Obvious flies away*...
            English.Add("OpenColorPickerButtonDescription", "Opens color picker, allowing you to create your own theme.");
            German.Add("OpenColorPickerButtonDescription", "Öffnet Farbwahl, und erlaubt dir eigene Themas zu erstellen");
            Polish.Add("OpenColorPickerButtonDescription", "Otwiera narzędzie do wybierania kolorów.\nUmożliwia tworzenie własnych schematów kolorów.");
            French.Add("OpenColorPickerButtonDescription", "Ouvrir la pipette de couleurs, vous autorise à créer votre propre thème.");
            Spanish.Add("OpenColorPickerButtonDescription", "Abre el selector de colores, permitiéndole crear su propio tema.");
            Russian.Add("OpenColorPickerButtonDescription", "Открывает окно палитры, из которой можно выбрать цвета для создания собственной темы.");

            //Component: DumpColorSettingsButtonDescription
            //
            English.Add("DumpColorSettingsButtonDescription", "Writes an xml document of all components that can have a custom color applied, to make a custom theme");
            German.Add("DumpColorSettingsButtonDescription", "Schreibt ein XML-Dokument aller Komponenten, auf die eine benutzerdefinierte Farbe angewendet werden kann, um ein benutzerdefiniertes Thema anzupassen");
            Polish.Add("DumpColorSettingsButtonDescription", "Tworzy plik XML ze wszystkimi elementami z opcją niestandardowego koloru, aby umożliwić stworzenie własnego motywu.");
            French.Add("DumpColorSettingsButtonDescription", "Ecrire un document xml de tous les composants qui peuvent avoir une couleur personnalisée, afin de faire un thème personnaliser");
            Spanish.Add("DumpColorSettingsButtonDescription", "Crea un documento XML con todos los componentes que pueden tener aplicado un color personalizado, para crear un tema personalizado");
            Russian.Add("DumpColorSettingsButtonDescription", "Создаёт XML-файл, в котором содержатся все параметры цветов для тех участков, где возможна замена цвета");

            //Component: MulticoreExtractionText
            //
            English.Add("MulticoreExtractionText", "Multicore extraction mode");
            German.Add("MulticoreExtractionText", "Mehrkern Extraktion");
            Polish.Add("MulticoreExtractionText", "Wyodrębnianie wielordzeniowe");
            French.Add("MulticoreExtractionText", "Mode d'extraction multicoeur");
            Spanish.Add("MulticoreExtractionText", "Modo de extración multinúcleo");
            Russian.Add("MulticoreExtractionText", "Многопроцессорный режим распаковки");

            //Component: MulticoreExtractionCBDescription
            //
            English.Add("MulticoreExtractionCBDescription", "The installer will use a parallel extraction method. It will extract multiple zip files at the same time," +
                " reducing install time. For SSD drives ONLY.");
            German.Add("MulticoreExtractionCBDescription", "Wird der Installer den parallelen Entpack-Modus verwenden. Er wird mehrere Zip-Dateien gleichzeitig entpacken" +
                " und dadurch die Installationszeit reduziert. Nur für SSD Festplatten.");
            Polish.Add("MulticoreExtractionCBDescription", "Metoda wydorębniania równoległego – skraca czas instalacji wypakowując kilka archiwów ZIP jednocześnie.\n" +
                "Opcja przeznaczona TYLKO dla dysków SSD!");
            French.Add("MulticoreExtractionCBDescription", "Le programme d'installation utilise une méthode d'extraction parallèle. Il va extraire plusieurs fichiers" +
                " zip en même temps, réduisant ainsi le temps d'installation. Pour les disques SSD SEULEMENT.");
            Spanish.Add("MulticoreExtractionCBDescription", "El instaladór utilizará un método de extracción paralela. Extraerá varios archivos zip al mismo tiempo, " +
                " reduciendo el tiempo de instalación. SÓLO para discos SSD.");
            Russian.Add("MulticoreExtractionCBDescription", "Установщик будет использовать метод параллельной распаковки. Будет извлекаться несколько\nZIP-архивов одновременно, уменьшая время установки. ТОЛЬКО ДЛЯ SSD ДИСКОВ!");

            //Component: UninstallDefaultText
            //
            English.Add("UninstallDefaultText", "Default"); // Check //verify the uninstall @ 'UninstallModpackButton_Click' before changing!
            German.Add("UninstallDefaultText", "Standard");
            Polish.Add("UninstallDefaultText", "Standardowy");
            French.Add("UninstallDefaultText", "Standard");
            Spanish.Add("UninstallDefaultText", "Estándar");
            Russian.Add("UninstallDefaultText", "Стандартный");

            //Component: UninstallQuickText
            //
            English.Add("UninstallQuickText", "Quick"); // Check //verify the uninstall @ 'UninstallModpackButton_Click' before changing!
            German.Add("UninstallQuickText", "Schnell");
            Polish.Add("UninstallQuickText", "Szybki");
            French.Add("UninstallQuickText", "Rapide");
            Spanish.Add("UninstallQuickText", "Rápida");
            Russian.Add("UninstallQuickText", "Быстрый");

            //Component: ExportModeText
            //
            English.Add("ExportModeText", "Export Mode");
            German.Add("ExportModeText", "Export-Modus");
            Polish.Add("ExportModeText", "Tryb eksportu");
            French.Add("ExportModeText", "Mode d'exportation");
            Spanish.Add("ExportModeText", "Modo de exportación");
            Russian.Add("ExportModeText", "Режим экспорта");

            //Component: ExportModeCBDescription
            //Explaining the export mode
            English.Add("ExportModeCBDescription", "Export mode will allow you to select a folder and WoT version you wish to export your mods installation to. For advanced users only." +
                "Note it will NOT: Unpack game xml files, patch files (provided from the game), or create the atlas files. Instructions can be found in the export directory.");
            German.Add("ExportModeCBDescription", "Der Export-Modus ermöglicht es dir, einen Ordner und WoT-Version zu wählen, in die du deine Mods-Installation exportieren möchtest." +
                " Nur für fortgeschrittene Benutzer. Bitte beachten: es werden KEINE Spiel-XML-Dateien entpackt und nicht modifiziert oder Atlas Dateien erstellt (jeweils aus dem Spiel" +
                " bereitgestellt). Anweisungen dazu findest du im Export-Verzeichnis.");
            Polish.Add("ExportModeCBDescription", "Pozwala na wybór folderu i docelowej wersji WoT celem eksportu instalacji. Tylko dla zaawansowanych użytkowników!\n" +
                "UWAGA: Pomija to wyodrębnianie plików XML gry, patchy i tworzenie plików atlas. Instrukcje są dostępne w folderze docelowym po wyeksportowaniu.");
            French.Add("ExportModeCBDescription", "Le mode Export vous permettra de sélectionner un dossier et la version de WoT vers lesquels vous souhaitez exporter votre installation" +
                " de mods. Pour les utilisateurs avancés, notez que l'installation ne fera pas: Déballez " +
                "les fichiers xml du jeu, corrigez les fichiers (fournis depuis le jeu) ou créez l'atlas. Les instructions peuvent être trouvées dans le répertoire d'exportation.");
            Spanish.Add("ExportModeCBDescription", "El modo de exportación le permitirá seleccionar una carpeta y versión de WoT a la que exportar la instalación de Mods. Sólo para usuarios avanzados." +
                " Tenga en cuenta que NO desempaquetará archivos XML o archivos de parche (proporcionados por el juego), ni creará los archivos de tipo atlas. Habrá instrucciones en el directorio exportado.");
            Russian.Add("ExportModeCBDescription", "Режим экспорта позволит выбрать папку для экспорта установленных модификаций в игру. Только для продвинутых пользователей.\n" +
                "Учтите, что эта опция НЕ распакует XML-файлы игры, патчить их или создавать атласы. Инструкции находятся в папке экспорта.");

            //Component: ViewCreditsButtonText
            //
            English.Add("ViewCreditsButtonText", "View Credits");
            German.Add("ViewCreditsButtonText", "Danksagung");
            Polish.Add("ViewCreditsButtonText", "Autorzy i współtwórcy Relhax Modpack"); // Authors and co-creators* of Relhax Modpack. (*also used as contributors) @Nullmaruzero
            French.Add("ViewCreditsButtonText", TranslationNeeded);
            Spanish.Add("ViewCreditsButtonText", "Ver créditos");
            Russian.Add("ViewCreditsButtonText", "Авторы модпака"); // I couldn't find anything more suitable, so "Modpack Authors" will be left here. *shrug* - DrWeb7_1

            //Component: ViewCreditsButtonDescription
            //
            English.Add("ViewCreditsButtonDescription", "See all the awesome people and projects that support the modpack!");
            German.Add("ViewCreditsButtonDescription", "Übersicht aller großartigen Leute und Projekte, die das Modpack unterstützen");
            Polish.Add("ViewCreditsButtonDescription", "Lista wszystkich wspaniałych ludzi i projektów, dzięki którym powstał ten modpack.");
            French.Add("ViewCreditsButtonDescription", TranslationNeeded);
            Spanish.Add("ViewCreditsButtonDescription", "¡Ver todas las increíbles personas y proyectos que apoyan el modpack!");
            Russian.Add("ViewCreditsButtonDescription", "Познакомьтесь с замечательными людьми и проектами, поддерживающими модпак!");

            //Component: ExportWindowDesctiption
            //
            English.Add("ExportWindowDesctiption", "Select the version of WoT you wish to export for");
            German.Add("ExportWindowDesctiption", "Wähle die Version von WoT, für die du exportieren möchtest");
            Polish.Add("ExportWindowDesctiption", "Wybierz wersję docelową klienta WoT:");
            French.Add("ExportWindowDesctiption", "Sélection de la version de WoT que vous souhaitez exporter");
            Spanish.Add("ExportWindowDescription", "Seleccione la versión de WoT para la que quiere exportar");
            Russian.Add("ExportWindowDesctiption", "Выберите версию WoT, для которой нужно произвести экспорт");

            //Component: HelperText
            // A large area in the middle of the default window and view for the modpack. Introduces the user to the modpack. You may format it using linebreaks -> \n
            English.Add("HelperText", "Welcome to the Relhax Modpack!" +
                "\nI have tried to make the modpack as straight-forward as possible, but questions may still arise. Hover over a setting to have it explained." +
                "\nThanks for using Relhax, I hope you enjoy it! - Willster419");
            German.Add("HelperText", "Willkommen beim Relhax Modpack!" +
                " \n\nIch habe versucht, das Modpack so einfach wie möglich zu gestalten. " +
                " \nZum Beispiel kannst du mit einem Klick der rechten Maustaste auf einen Eintrag eine Beschreibung oder Vorschau anzeigen." +
                " \nNatürlich können dennoch Fragen entstehen." +
                " In so einem Fall nutze bitte eine unserer unten angezeigten Kontaktmöglichkeiten." +
                " \n\nVielen Dank für die Nutzung des Relhax Modpacks\n - Willster419");
            Polish.Add("HelperText", "Witaj w Modpacku Relhax!\n\nMoim celem było stworzenie paczki modów tak prostej, jak to tylko możliwe,\n" +
                "ale jeśli nadal czujesz się nieco zagubiony, po prostu najedź kursorem na dowolną opcję i przeczytaj jej opis.\n\n" +
                "Dzięki za wybranie tego modpacka, mam nadzieję, że Ci się spodoba! — Willster419");
            French.Add("HelperText", "Bienvenue au Modpack Relhax! J'ai aissayé de faire le modpack le plus simple possible, mais des questions peuvent survenir." +
                " Survolez un paramètre pour voire une explication.");
            Spanish.Add("HelperText", "¡Bienvenido a RelHax Modpack!" +
                "\nHe intentado hacer el Modpack tan sencillo como ha sido posible, pero aún así pueden surgir dudas. Mantenga el ratón sobre una opción para obtener una explicación." +
                "\n¡Gracias por usar Relhax, espero que lo disfrute! - Willster419");
            Russian.Add("HelperText", "Вас приветствует Relhax Modpack!\n\nЯ старался сделать его максимально простым для пользователя, но вопросы всё же могут возникнуть.\n\nНаведите курсор мыши на любую настройку, и вы увидите пояснение к ней.\n\nБлагодарим вас за выбор в пользу Relhax, надеюсь, вам понравится! - Willster419");

            //Component: helperTextShort
            //
            English.Add("helperTextShort", "Welcome to the Relhax Modpack!");
            German.Add("helperTextShort", "Willkommen zum Relhax Modpack!");
            Polish.Add("helperTextShort", "Witamy w paczce Relhax!");
            French.Add("helperTextShort", "Bienvenue au Modpack Relhax!");
            Spanish.Add("helperTextShort", "¡Bienvenido a RelHax Modpack!");
            Russian.Add("helperTextShort", "Вас приветствует Relhax Modpack!");

            //Component: NotifyIfSameDatabaseText
            //
            English.Add("NotifyIfSameDatabaseText", "Inform if no new database available (stable database only)");
            German.Add("NotifyIfSameDatabaseText", "Benachrichtigung wenn es keine Aktuallisierungen für die Datenbank gibt");
            Polish.Add("NotifyIfSameDatabaseText", "Powiadom o braku nowej bazy danych*");//"Powiadom o braku nowej bazy danych"
            French.Add("NotifyIfSameDatabaseText", "Informer si aucune nouvelle base de données n'est disponible");//"Informer si aucune nouvelle base de données est disponible"
            Spanish.Add("NotifyIfSameDatabaseText", "Informar si no hay una nueva base de datos disponible (sólo estable)");//"Informar si no hay nueva base de datos"
            Russian.Add("NotifyIfSameDatabaseText", "Сообщать об актуальности БД (только для стабильной версии)");

            //Component: NotifyIfSameDatabaseCBDescriptionOLD
            //
            English.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Notify you if your last installed database version is the same. If so, it means that there is no update to any mods. This only works with the stable database.");
            German.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Erhalte eine Benachrichtigung wenn es keine Aktualisierung der Datenbank gegeben hat und diese den selben Stand wie beim letzten Start aufweist.");
            Polish.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Wyświetla powiadomienie jeśli zainstalowana jest najnowsza wersja bazy danych.\nOznacza to, że nie ma żadnych nowych aktualizacji modów.");
            French.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Avertir si votre dernière version de base de données installée est identique. Si c'est le cas," +
                " cela signifie qu'il n'y a pas de mise à jour de mods.");
            Spanish.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Mostrar una notificación si la última instalación tiene la misma versión de la base de datos." +
                " De ser así, significa que no hay ninguna actualización para ningún Mod");
            Russian.Add("NotifyIfSameDatabaseCBDescriptionOLD", "Уведомлять в случае совпадения версий баз данных. Это означает отсутствие обновлений к каким-либо модам.");

            //Component: NotifyIfSameDatabaseCBDescription
            //
            English.Add("NotifyIfSameDatabaseCBDescription", "Notify you if your last installed database version is the same. If so, it means that there is no update to any mods." +
                " This only works with the stable database.");
            German.Add("NotifyIfSameDatabaseCBDescription", "Benachrichtigt dich, wenn deine zuletzt installierte Datenbankversion dieselbe ist. Wenn ja, bedeutet dies, dass keine Modifikationen aktualisiert wurden.");
            Polish.Add("NotifyIfSameDatabaseCBDescription", "Wyświetla powiadomienie kiedy używana jest najnowsza wersja bazy danych.\nOznacza to brak dostępnych aktualizacji modów.\n" +
                "Opcja dostępna tylko dla stabilnej wersji bazy danych (nie BETA).");
            French.Add("NotifyIfSameDatabaseCBDescription", "Vous informe si votre dernière version de base de données installée est la même. Si c'est le cas, cela signifie qu'il n'y a pas de mise à jour des mods. Cela ne marche qu'avec la base de données stables.");
            Spanish.Add("NotifyIfSameDatabaseCBDescription", "Notificar si la última instalación tiene la misma versión de la base de datos que la actual. De ser así, significa que no ha habido ninguna actualización de ningún mod.\n" +
                "Esto solo funciona con la base de datos estable.");
            Russian.Add("NotifyIfSameDatabaseCBDescription", "Если ваша версия БД является актуальной, то вы увидите уведомление. Это значит, что обновлений к модам не было.");

            //Component: ShowInstallCompleteWindowText
            //
            English.Add("ShowInstallCompleteWindowText", "Show advanced install complete window");
            German.Add("ShowInstallCompleteWindowText", "Zeige erweitertes Fenster bei abgeschlossener Installation");
            Polish.Add("ShowInstallCompleteWindowText", "Szczegółowe podsumowanie instalacji");
            French.Add("ShowInstallCompleteWindowText", "Montrer la fenêtre d'installation complète terminée" );
            Spanish.Add("ShowInstallCompleteWindowText", "Ver ventana de instalación completada avanzada");
            Russian.Add("ShowInstallCompleteWindowText", "Показывать расширенное окно окончания установки");

            //Component: ShowInstallCompleteWindowCBDescription
            //
            English.Add("ShowInstallCompleteWindowCBDescription", "Show a window upon installation completion with popular operations to" +
                " perform after modpack installation, such as launching the game, going to the xvm website, etc.");
            German.Add("ShowInstallCompleteWindowCBDescription", "Zeigte am Ende der Installation ein Auswahlfenster mit nützlichen Befehlen an," +
                " wie: starte das Spiel, gehe zur XVM Webseite, usw ...");
            Polish.Add("ShowInstallCompleteWindowCBDescription", "Po zakończeniu instalacji wyświetla okno z częstymi kolejnymi krokami.");
            French.Add("ShowInstallCompleteWindowCBDescription", "Afficher une fenêtre lors de l'achèvement de l'installation avec des opérations populaires à" +
                " effectuer après l'installation de Modpack, telles que le lancement du jeu, le site Web de XVM, etc.");
            Spanish.Add("ShowInstallCompleteWindowCBDescription", "Muestra una ventana al completar la instalación con opciones comunes tras la instalación del Modpack," +
                " tales como iniciar el juego, visitar la página web del XVM, etc.");
            Russian.Add("ShowInstallCompleteWindowCBDescription", "Показывать окно по окончании установки с частыми действиями после установки" +
                " модпака (запуск игры, открыть сайт XVM, и т. п.)");

            //Component: applicationVersion
            //
            English.Add("applicationVersion", "Application Version");
            German.Add("applicationVersion", "Programmversion");
            Polish.Add("applicationVersion", "Wersja");
            French.Add("applicationVersion", "Version de l'application");
            Spanish.Add("applicationVersion", "Versión de la aplicación");
            Russian.Add("applicationVersion", "Версия ПО:");

            //Component: databaseVersion
            //
            English.Add("databaseVersion", "Latest Database");
            German.Add("databaseVersion", "Datenbank");
            Polish.Add("databaseVersion", "Baza danych");
            French.Add("databaseVersion", "Base de donnés");
            Spanish.Add("databaseVersion", "Última base de datos");
            Russian.Add("databaseVersion", "Версия БД:");

            //Component: ClearCacheText
            //
            English.Add("ClearCacheText", "Clear WoT cache data");
            German.Add("ClearCacheText", "Cache-Daten für WoT löschen");
            Polish.Add("ClearCacheText", "Wyczyść pamięć podręczną WoT");
            French.Add("ClearCacheText", "Nettoyer le dossier de Cache WoT");
            Spanish.Add("ClearCacheText", "Limpiar datos de caché de WoT");
            Russian.Add("ClearCacheText", "Очистить кэш World of Tanks");

            //Component: ClearCachCBDescription
            //
            English.Add("ClearCacheCBDescription", "Clear the WoT cache app data directory. Performs the same task as the similar option that was in OMC.");
            German.Add("ClearCacheCBDescription", "Lösche das WoT-Cache-App-Datenverzeichnis. Führt die gleiche Aufgabe wie die ähnliche Option aus, die in OMC war.");
            Polish.Add("ClearCacheCBDescription", "Usuwa pliki z folderu pamięci podręcznej cache WoT. Działa tak samo jak opcja dostępna kiedyś w modpacku OMC.");
            French.Add("ClearCacheCBDescription", "Nettoyer le dossier cache WoT. Effectue la même tâche que l'option similaire qui était dans OMC.");
            Spanish.Add("ClearCacheCBDescription", "Limpia la caché de WoT en el directorio %APPDATA%. Realiza la misma operación que la opción similar en el OMC Modpack");
            Russian.Add("ClearCacheCBDescription", "Очистить папку кэша World of Tanks. Операция аналогична соответствующей опции, присутствовавшей в OMC.");

            //Component: UninstallDefaultDescription
            // A radio button tooltip explaining the difference between this option and the option next (to the right) of it.
            English.Add("UninstallDefaultDescription", "Default Uninstall will remove all files in the game's mod directories, including shortcuts and appdata cache files.");
            German.Add("UninstallDefaultDescription", "Die Standard Deinstallation wird alle Dateien in den Mod-Verzeichnissen des Spieles löschen, inklusive der" +
                " Verknüpfungen und Dateien im 'AppData' Speicher.");
            Polish.Add("UninstallDefaultDescription", "Domyślna dezinstalacja usunie wszystkie pliki w folderze modów i powiązane pliki, także skróty" +
                " oraz pamięć podręczną aplikacji.");
            French.Add("UninstallDefaultDescription", "La méthode de désinstallation par défaut vas supprimer tout les fichiers dans le dossier du jeu, incluant les" +
                " raccourcies et le fichers de cache appdata");
            Spanish.Add("UninstallDefaultDescription", "La desinstalación estándar eliminará todos los archivos en los directorios de Mods del juego, incluyendo accesos directos y archivos de caché");
            Russian.Add("UninstallDefaultDescription", "Обычная деинсталляция удалит все моды, включая ярлыки и кэш в AppData.");

            //Component: UninstallQuickDescription
            // A radio button tooltip explaining the difference between this option and the option next (to the left) of it.
            English.Add("UninstallQuickDescription", "Quick Uninstall will only remove files in the game's mod directories. It does not remove modpack" +
                " created shortcuts or appdata cache files.");
            German.Add("UninstallQuickDescription", "Die schnelle Deinstallation wird nur Dateien in den Mod-Verzeichnissen der Spieles löschen." +
                " Es werden keine vom ModPack erstellten Verknüpfungen oder Dateien im 'AppData' Speicher gelöscht.");
            Polish.Add("UninstallQuickDescription", "Szybka dezinstalacja usuwa tylko pliki w folderze modów, pomijając skróty oraz pamięć podręczną aplikacji.");
            French.Add("UninstallQuickDescription", "La méthode de désinstallation rapide vas uniquement supprimer les fichiers dans le dossier" +
                " \"mod\" du jeu. Ceci ne supprimeras pas les raccourcis ou les fichiers de cache appdata créé par le modpack");
            Spanish.Add("UninstallQuickDescription", "La desinstalación rápida sólo eliminará archivos en los directorios de Mods del juego. No eliminará archivos" +
                " del Modpack, accesos directos o archivos de caché");
            Russian.Add("UninstallQuickDescription", "Быстрая деинсталляция удалит только моды, оставив ярлыки, созданные модпаком, и кэш в AppData.");

            //Component: DiagnosticUtilitiesButton
            //
            English.Add("DiagnosticUtilitiesButton", "Diagnostic utilities");
            German.Add("DiagnosticUtilitiesButton", "Diagnosedienst");
            Polish.Add("DiagnosticUtilitiesButton", "Diagnostyka");
            French.Add("DiagnosticUtilitiesButton", "Utilitaires de diagnostique");
            Spanish.Add("DiagnosticUtilitiesButton", "Utilidades de diagnóstico");
            Russian.Add("DiagnosticUtilitiesButton", "Диагностика");

            //Component: DiagnosticUtilitiesButtonDescription
            //
            English.Add("DiagnosticUtilitiesButtonDescription", "Report a bug, attempt a WG client repair, etc.");
            German.Add("DiagnosticUtilitiesButtonDescription", "Fehler melden, versuche eine Client Reparatur, etc.");
            Polish.Add("DiagnosticUtilitiesButtonDescription", "Zgłoś błąd, spróbuj naprawić klienta gry, itp.");
            French.Add("DiagnosticUtilitiesButtonDescription", "Signaler un bug, tenter une réparation du client WG, etc.");
            Spanish.Add("DiagnosticUtilitiesButtonDescription", "Informar de un error, intentar una reparación del cliente de WG, etc.");
            Russian.Add("DiagnosticUtilitiesButtonDescription", "Сообщить о баге, попытаться починить клиент игры, и т. д.");

            //Component: UninstallModeGroupBox
            //
            English.Add("UninstallModeGroupBox", "Uninstall Mode:");
            German.Add("UninstallModeGroupBox", "Deinstallationsmodus:");
            Polish.Add("UninstallModeGroupBox", "Tryb Dezinstalacji:");
            French.Add("UninstallModeGroupBox", "Mode de désinstallation:");
            Spanish.Add("UninstallModeGroupBox", "Modo de desinstalación:");
            Russian.Add("UninstallModeGroupBox", "Режим деинсталляции:");

            //Component: UninstallModeGroupBoxDescription
            English.Add("UninstallModeGroupBoxDescription", "Select the uninstall mode to use");
            German.Add("UninstallModeGroupBoxDescription", "Wähle den Deinstallationsmodus");
            Polish.Add("UninstallModeGroupBoxDescription", "Wybiera metodę dezinstalcji modów.");
            French.Add("UninstallModeGroupBoxDescription", "Sélectionner le mode d'installation à utiliser");
            Spanish.Add("UninstallModeGroupBoxDescription", "Seleccione el modo de desinstalación a utilizar");
            Russian.Add("UninstallModeGroupBoxDescription", "Выбрать метод удаления");

            //Component: FacebookButtonDescription
            English.Add("FacebookButtonDescription", "Go to our Facebook page");
            German.Add("FacebookButtonDescription", "Unsere Facebook Seite aufrufen");
            Polish.Add("FacebookButtonDescription", "Nasz Facebook");
            French.Add("FacebookButtonDescription", "Page Facebook");
            Spanish.Add("FacebookButtonDescription", "Ir a nuestra página de Faceboook");
            Russian.Add("FacebookButtonDescription", "Перейти на нашу страницу в Facebook");

            //Component: DiscordButtonDescription
            English.Add("DiscordButtonDescription", "Go to Discord server");
            German.Add("DiscordButtonDescription", "Zum Discord Server");
            Polish.Add("DiscordButtonDescription", "Nasz Discord");
            French.Add("DiscordButtonDescription", "Serveur Discord");
            Spanish.Add("DiscordButtonDescription", "Ir a nuestro servidor de Discord");
            Russian.Add("DiscordButtonDescription", "Перейти на наш сервер Discord");

            //Component: TwitterButtonDescription
            English.Add("TwitterButtonDescription", "Go to our Twitter page");
            German.Add("TwitterButtonDescription", "Unsere Twitter Seite aufrufen");
            Polish.Add("TwitterButtonDescription", "Nasz Twitter");
            French.Add("TwitterButtonDescription", "Page Twitter");
            Spanish.Add("TwitterButtonDescription", "Ir a nuestra página de Twitter");
            Russian.Add("TwitterButtonDescription", "Перейти на нашу страницу в Twitter");

            //Component: SendEmailButtonDescription
            English.Add("SendEmailButtonDescription", "Send us an Email (No modpack support)");
            German.Add("SendEmailButtonDescription", "Sende uns eine eMail (kein Modpack Support)");
            Polish.Add("SendEmailButtonDescription", "Nasz email (nie dotyczy wsparcia technicznego)");
            French.Add("SendEmailButtonDescription", "Nous envoyer un E-mail (Pas de support)");
            Spanish.Add("SendEmailButtonDescription", "Envíanos un e-mail (soporte del modpack no)");
            Russian.Add("SendEmailButtonDescription", "Отправить нам письмо на e-mail (Не для техподдержки)");

            //Component: HomepageButtonDescription
            English.Add("HomepageButtonDescription", "Visit our Website");
            German.Add("HomepageButtonDescription", "Zu unserer Homepage");
            Polish.Add("HomepageButtonDescription", "Odwiedz naszą strone");
            French.Add("HomepageButtonDescription", "Visiter notre site web");
            Spanish.Add("HomepageButtonDescription", "Visita nuestra página web");
            Russian.Add("HomepageButtonDescription", "Посетить наш веб-сайт");

            //Component: DonateButtonDescription
            English.Add("DonateButtonDescription", "Donation for further development");
            German.Add("DonateButtonDescription", "Spende für die Weiterentwicklung");
            Polish.Add("DonateButtonDescription", "Dotacja na dalszy rozwój");
            French.Add("DonateButtonDescription", "Donation pour aider au développement");
            Spanish.Add("DonateButtonDescription", "Donaciones para el desarrollo");
            Russian.Add("DonateButtonDescription", "Поддержать копеечкой для дальнейшей разработки");

            //Component: FindBugAddModButtonDescription
            English.Add("FindBugAddModButtonDescription", "Find a bug? Want a mod added? Report here please!");
            German.Add("FindBugAddModButtonDescription", "Fehler gefunden? Willst Du einen Mod hinzufügen? Bitte hier melden!");
            Polish.Add("FindBugAddModButtonDescription", "Znalazłeś błąd? Chcesz dodać moda? Zgłoś się tutaj!");
            French.Add("FindBugAddModButtonDescription", "Tu as trouvé un bug? Recommandation de mod?");
            Spanish.Add("FindBugAddModButtonDescription", "¿Ha encontrado un error? ¿Quiere que un mod sea añadido? Informa aquí");
            Russian.Add("FindBugAddModButtonDescription", "Нашли баг? Хотите добавить мод? Пишите сюда!");

            //Component: Mod Selection view Group Box
            //
            English.Add("SelectionViewGB", "Selection View");
            German.Add("SelectionViewGB", "Darstellungsart");
            Polish.Add("SelectionViewGB", "Widok kolekcji"); //? This fella does not seem to be used in V2. @Nullmaruzero
            French.Add("SelectionViewGB", "Affichage de sélection");
            Spanish.Add("SelectionViewGB", "Vista de selección");
            Russian.Add("SelectionViewGB", "Вид списка");

            //Component: SelectionDefaultText
            //Mod selection view default (relhax) [NOW WPF VERSION IS DEFAULT]
            English.Add("SelectionDefaultText", "Default");
            German.Add("SelectionDefaultText", "Standard");
            Polish.Add("SelectionDefaultText", "Domyślny");
            French.Add("SelectionDefaultText", "Standard");
            Spanish.Add("SelectionDefaultText", "Por defecto");
            Russian.Add("SelectionDefaultText", "Стандартный");

            //Component: SelectionLegacyText
            //Mod selection view legacy (OMC)
            AddTranslationToAll("SelectionLegacyText", "OMC Legacy");

            //Component: Mod selection Description
            //
            English.Add("SelectionLayoutDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            German.Add("SelectionLayoutDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionLayoutDescription", "Wybierz tryb widoku listy modyfikacji.\nDomyślnie: Lista wyboru RelHax\nOMC: Drzewko wyboru OMC");
            French.Add("SelectionLayoutDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");
            Spanish.Add("SelectionLayoutDescription", "Selecciona un modo de la lista de selección.\nPor defecto: modo de Relhax.\nLegacy: lista en árbol de OMC");
            Russian.Add("SelectionLayoutDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");

            //Component: Mod selection Description
            //
            English.Add("SelectionDefaultDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            German.Add("SelectionDefaultDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionDefaultDescription", "Wybierz tryb widoku listy modyfikacji.\nDomyślnie: Lista wyboru RelHax\nOMC: Drzewko wyboru OMC");
            French.Add("SelectionDefaultDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");
            Spanish.Add("SelectionDefaultDescription", "Selecciona un modo de la lista de selección\nPor defecto: modo de Relhax\nLegacy: lista en árbol de OMC");
            Russian.Add("SelectionDefaultDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");

            //Component: Mod selection Description
            //
            English.Add("SelectionLegacyDescription", "Select a mod selection list view mode\nDefault: Relhax list view mode\nLegacy: OMC tree list view mode");
            German.Add("SelectionLegacyDescription", "Anzeigearten und Auswahl der Modifikationen\nStandard: Relhax Listendarstellung\nalternativ: OMC Baumstruktur");
            Polish.Add("SelectionLegacyDescription", "Wybierz tryb widoku listy modyfikacji.\nDomyślnie: Lista wyboru RelHax\nOMC: Drzewko wyboru OMC");
            French.Add("SelectionLegacyDescription", "Sélectionnez un style de liste pour la sélection\nNormal: Liste de sélection Relhax\nLegacy: Liste de vue arbre OMC");
            Spanish.Add("SelectionLegacyDescription", "Selecciona un modo de la lista de selección\nPor defecto: modo de Relhax\nLegacy: lista en árbol de OMC");
            Russian.Add("SelectionLegacyDescription", "Выберите вид списка модов\nОбычный: как в Relhax (постранично)\nLegacy: как в OMC (деревом)");

            //Component: LanguageSelectionGBDescription
            //
            English.Add("LanguageSelectionGBDescription", "Select your preferred language.");
            German.Add("LanguageSelectionGBDescription", "wähle Deine bevorzugte Sprache");
            Polish.Add("LanguageSelectionGBDescription", "Wybierz preferowany język.");
            French.Add("LanguageSelectionGBDescription", "Sélectionnez votre langue préféré");
            Spanish.Add("LanguageSelectionGBDescription", "Seleccione su idioma preferido");
            Russian.Add("LanguageSelectionGBDescription", "Выберите желаемый язык");

            //Component: expandNodesDefault
            //
            English.Add("expandNodesDefault", "Expand all");
            German.Add("expandNodesDefault", "Alle erweitern");
            Polish.Add("expandNodesDefault", "Rozwiń wszystko"); //? Is this even used? @Nullmaruzero
            French.Add("expandNodesDefault", "Développer tout");
            Spanish.Add("expandNodesDefault", "Expandir todo");
            Russian.Add("expandNodesDefault", "Раскрыть все"); // It seems that's not used. - DrWeb7_1 replying to @Nullmaruzero

            //Component: expandNodesDefault2
            //
            English.Add("expandNodesDefault2", "Expand all");
            German.Add("expandNodesDefault2", "Alle erweitern");
            Polish.Add("expandNodesDefault2", "Rozwiń wszystko"); //? Is this even used? @Nullmaruzero
            French.Add("expandNodesDefault2", "Développer tout");
            Spanish.Add("expandNodesDefault2", "Expandir todo");
            Russian.Add("expandNodesDefault2", "Раскрыть все"); // It seems that's not used. - DrWeb7_1 replying to @Nullmaruzero

            //Component: expandNodesDefaultDescription
            //
            English.Add("expandNodesDefaultDescription", "Select this to have all options automatically expand. It applies for the Legacy Selection only.");
            German.Add("expandNodesDefaultDescription", "Erweitere alle Einträge auf allen Registerkarten automatisch. Nur bei Ansicht als Baumstruktur.");
            Polish.Add("expandNodesDefaultDescription", "Zaznacz to, aby wszystkie opcje zostały automatycznie rozwinięte. Dotyczy tylko opcji Legacy Selection."); //? Is this even used? @Nullmaruzero
            French.Add("expandNodesDefaultDescription", "Sélectionnez cette option pour que toutes les options s'élargis automatiquement. S'applique uniquement à la Sélection Legacy.");
            Spanish.Add("expandNodesDefaultDescription", "Seleccionar para tener todas las opciones expandidas automáticamente. Sólo se aplica en el modo de selección de legado");
            Russian.Add("expandNodesDefaultDescription", "Выберите этот пункт для автоматического раскрытия всех списков. Применимо только для legacy.");


            //Component: EnableBordersDefaultV2Text
            //
            English.Add("EnableBordersDefaultV2Text", "Enable borders");
            German.Add("EnableBordersDefaultV2Text", "Einrahmen");
            Polish.Add("EnableBordersDefaultV2Text", "Obramowanie elementów");
            French.Add("EnableBordersDefaultV2Text", "Activer les bordures");
            Spanish.Add("EnableBordersDefaultV2Text", "Habilitar bordes");
            Russian.Add("EnableBordersDefaultV2Text", "Включить границы");

            //Component: EnableBordersLegacyText
            //
            English.Add("EnableBordersLegacyText", "Enable borders");
            German.Add("EnableBordersLegacyText", "Einrahmen");
            Polish.Add("EnableBordersLegacyText", "Obramowanie elementów");
            French.Add("EnableBordersLegacyText", "Activer les bordures");
            Spanish.Add("EnableBordersLegacyText", "Habilitar bordes");
            Russian.Add("EnableBordersLegacyText", "Включить границы");

            //Component: EnableBordersDefaultV2CBDescription
            //
            English.Add("EnableBordersDefaultV2CBDescription", "Enable the black borders around each mod and config sublevel.");
            German.Add("EnableBordersDefaultV2CBDescription", "Jede Auswahl schwarz einrahmen");
            Polish.Add("EnableBordersDefaultV2CBDescription", "Włącza czarne obramowanie modów i opcji konfiguracji.");
            French.Add("EnableBordersDefaultV2CBDescription", "Activer les bordures noires autour de chaque mod et selections de configuration.");
            Spanish.Add("EnableBordersDefaultV2CBDescription", "Habilita los bordes negros alrededor de cada mod y subnivel de configuración");
            Russian.Add("EnableBordersDefaultV2CBDescription", "Включить показ чёрных рамок вокруг наименования каждого мода и конфигурации.");


            //Component: EnableBordersLegacyCBDescription
            //
            English.Add("EnableBordersLegacyCBDescription", "Enable the black borders around each mod and config sublevel.");
            German.Add("EnableBordersLegacyCBDescription", "Jede Auswahl schwarz einrahmen");
            Polish.Add("EnableBordersLegacyCBDescription", "Włącza czarne obramowanie modów i opcji konfiguracji.");
            French.Add("EnableBordersLegacyCBDescription", "Activer les bordures noires autour de chaque mod et selections de configuration.");
            Spanish.Add("EnableBordersLegacyCBDescription", "Habilitar los bordes negros alrededor de cada mod y subnivel de configuarción");
            Russian.Add("EnableBordersLegacyCBDescription", "Включить показ чёрных рамок вокруг наименования каждого мода и конфигурации.");


            //Component: UseBetaDatabaseText
            English.Add("UseBetaDatabaseText", "Use beta database");
            German.Add("UseBetaDatabaseText", "Beta-Datenbank");
            Polish.Add("UseBetaDatabaseText", "Używaj wersji BETA bazy danych");
            French.Add("UseBetaDatabaseText", "Utiliser la base de données beta");
            Spanish.Add("UseBetaDatabaseText", "Utilizar la base de datos en beta");
            Russian.Add("UseBetaDatabaseText", "Использовать бета-версию БД");

            //Component: UseBetaDatabaseCBDescription
            English.Add("UseBetaDatabaseCBDescription", "Use the latest beta database. Mod stability is not guaranteed");
            German.Add("UseBetaDatabaseCBDescription", "Verwende die letzte Beta-Version der ModPack-Datenbank. Die Stabilität der Mods kann nicht garantiert werden, jedoch werden hier auch Fehlerbehebungen als erstes getestet und implementiert.");
            Polish.Add("UseBetaDatabaseCBDescription", "Używa najnowszej wersji rozwojowej (beta) bazy danych. Stabilość modów nie jest gwarantowana!");
            French.Add("UseBetaDatabaseCBDescription", "Utiliser la dernière base de données beta. La stabilité des mods n'est pas garantie");
            Spanish.Add("UseBetaDatabaseCBDescription", "Utiliza la última versión en beta de la base de datos. La estabilidad de los mods no está garantizada");
            Russian.Add("UseBetaDatabaseCBDescription", "Использовать последнюю доступную бета-версию БД. Стабильность модов не гарантирована.");


            //Component: UseBetaApplicationText
            English.Add("UseBetaApplicationText", "Use beta application");
            German.Add("UseBetaApplicationText", "Beta-Version des Installers");
            Polish.Add("UseBetaApplicationText", "Używaj wersji BETA aplikacji");
            French.Add("UseBetaApplicationText", "Utiliser l'application beta");
            Spanish.Add("UseBetaApplicationText", "Utilizar la aplicación en beta");
            Russian.Add("UseBetaApplicationText", "Использовать бета-версию программы");

            //Component: UseBetaApplicationCBDescription
            English.Add("UseBetaApplicationCBDescription", "Use the latest beta application. Translations and application stability are not guaranteed");
            German.Add("UseBetaApplicationCBDescription", "Verwende die letzte Beta-Version des ModPack Managers. Fehlerfreie Übersetzungen und Programmstabilität können nicht garantiert werden.");
            Polish.Add("UseBetaApplicationCBDescription", "Używa najnowszej wersji rozwojowej (beta) aplikacji. Stabilność oraz pełne tłumaczenie nie są gwarantowane!");
            French.Add("UseBetaApplicationCBDescription", "Utiliser la dernière version beta. Les traductions et la stabilité de l'application ne sont pas garanties");
            Spanish.Add("UseBetaApplicationCBDescription", "Utiliza la última versión en beta de la aplicación. Las traducciones y estabilidad de la aplicación no están garantizadas");
            Russian.Add("UseBetaApplicationCBDescription", "Использовать последнюю доступную бета-версию программы. Корректность перевода и стабильность приложения не гарантированы.");


            //Component: SettingsTabIntroHeader
            //
            English.Add("SettingsTabIntroHeader", "Welcome!");
            German.Add("SettingsTabIntroHeader", "Willkommen");
            Polish.Add("SettingsTabIntroHeader", "Witaj!");
            French.Add("SettingsTabIntroHeader", "Bienvenue !");
            Spanish.Add("SettingsTabIntroHeader", "¡Bienvenido!");
            Russian.Add("SettingsTabIntroHeader", "Добро пожаловать!");

            //Component: SettingsTabSelectionViewHeader
            //
            English.Add("SettingsTabSelectionViewHeader", "Selection View");
            German.Add("SettingsTabSelectionViewHeader", "Auswahlansicht");
            Polish.Add("SettingsTabSelectionViewHeader", "Opcje Widoku");
            French.Add("SettingsTabSelectionViewHeader", "Vue de sélection");
            Spanish.Add("SettingsTabSelectionViewHeader", "Vista de selección");
            Russian.Add("SettingsTabSelectionViewHeader", "Выбор вида списка");

            //Component: SettingsTabInstallationSettingsHeader
            //
            English.Add("SettingsTabInstallationSettingsHeader", "Installation Settings");
            German.Add("SettingsTabInstallationSettingsHeader", "Installationseinstellungen");
            Polish.Add("SettingsTabInstallationSettingsHeader", "Opcje Instalacji");
            French.Add("SettingsTabInstallationSettingsHeader", "Options d'installation");
            Spanish.Add("SettingsTabInstallationSettingsHeader", "Opciones de Instalación");
            Russian.Add("SettingsTabInstallationSettingsHeader", "Параметры установки");

            //Component: SettingsTabApplicationSettingsHeader
            //
            English.Add("SettingsTabApplicationSettingsHeader", "Application Settings");
            German.Add("SettingsTabApplicationSettingsHeader", "Programmeinstellungen");
            Polish.Add("SettingsTabApplicationSettingsHeader", "Opcje Aplikacji");
            French.Add("SettingsTabApplicationSettingsHeader", "Options de l'application");
            Spanish.Add("SettingsTabApplicationSettingsHeader", "Opciones de la aplicación");
            Russian.Add("SettingsTabApplicationSettingsHeader", "Параметры приложения");

            //Component: SettingsTabAdvancedSettingsHeader
            //
            English.Add("SettingsTabAdvancedSettingsHeader", "Advanced");
            German.Add("SettingsTabAdvancedSettingsHeader", "Erweitert");
            Polish.Add("SettingsTabAdvancedSettingsHeader", "Zaawansowane");
            French.Add("SettingsTabAdvancedSettingsHeader", "Avancée");
            Spanish.Add("SettingsTabAdvancedSettingsHeader", "Opciones avanzadas");
            Russian.Add("SettingsTabAdvancedSettingsHeader", "Расширенные настройки");

            //Component: MainWindowSelectSelectionFileToLoad
            //
            English.Add("MainWindowSelectSelectionFileToLoad", "Select selection file to load");
            German.Add("MainWindowSelectSelectionFileToLoad", "Wähle die zu ladende Auswahldatei");
            Polish.Add("MainWindowSelectSelectionFileToLoad", "Wybierz plik kolekcji do wczytania");
            French.Add("MainWindowSelectSelectionFileToLoad", "Sélectionner la sélection de fichier à charger");
            Spanish.Add("MainWindowSelectSelectionFileToLoad", "Seleccione archivo de selección a cargar");
            Russian.Add("MainWindowSelectSelectionFileToLoad", "Выберите предустановку для загрузки");

            //Component: verifyUninstallHeader
            //
            English.Add("verifyUninstallHeader", "Confirmation");
            German.Add("verifyUninstallHeader", "Bestätigung");
            Polish.Add("verifyUninstallHeader", "Potwierdzenie");
            French.Add("verifyUninstallHeader", "Confirmation");
            Spanish.Add("verifyUninstallHeader", "Confirmación");
            Russian.Add("verifyUninstallHeader", "Подтверждение");

            //Component: verifyUninstallVersionAndLocation
            //
            English.Add("verifyUninstallVersionAndLocation", "Confirm you wish to uninstall mods from WoT installation\n\n{0}\n\nUsing uninstall method '{1}'?");
            German.Add("verifyUninstallVersionAndLocation", "Bitte bestätige das du alle Mods von Deinem WoT deinstalliert haben möchtest\n\n{0}\n\nVerwendung von Deinstallationsmethode '{1}'?");
            Polish.Add("verifyUninstallVersionAndLocation", "Czy chcesz odinstalować wszystkie mody z WoT?\n\n{0}\n\nWybrany tryb dezinstalacji: {1}");
            French.Add("verifyUninstallVersionAndLocation", "Confirmer que vous voulez désinstaller les mods du dossier WoT\n\n{0}\n\nUsing la méthode de désinstallation '{1}'?");
            Spanish.Add("verifyUninstallVersionAndLocation", "¿Confirma que desea desinstalar mods del directorio de WoT\n\n{0}\n\nutilizando el método de desinstalación '{1}'?");
            Russian.Add("verifyUninstallVersionAndLocation", "Подтвердите необходимость удалить моды для WoT в этой папке: \n\n{0}\n\nИспользуем метод '{1}'?");

            //Component: failedVerifyFolderStructure
            //When the application first starts, it tries to open a logfile
            English.Add("failedVerifyFolderStructure", "The application failed to create the required folder structure. Check your file permissions or move the application to a folder with write access.");
            German.Add("failedVerifyFolderStructure", "Das Programm konnte die erforderliche Ordnerstruktur nicht erstellen. Bitte überprüfe die Berechtigungen oder verschiebe das Programm an einen Ort mit Schreibrechten.");
            Polish.Add("failedVerifyFolderStructure", "Aplikacja nie mogła utworzyć wymaganej struktury folderów. Sprawdź swoje uprawnienia lub przenieś aplikację do folderu z prawami do zapisu.");
            French.Add("failedVerifyFolderStructure", "Echec de la vérification de la structure des dossiers");
            Spanish.Add("failedVerifyFolderStructure", "La aplicación no ha podido crear la estructura de carpetas requerida. Compruebe sus permisos de archivos o mueva la aplicación a una carpeta con permisos de escritura.");
            Russian.Add("failedVerifyFolderStructure", "Приложению не удалось создать необходимую структуру папок. Проверьте права доступа к файлам или переместите приложение в папку, где разрешена запись.");

            //Component: failedToExtractUpdateArchive
            //Upon update download, if it can't extract the exe
            English.Add("failedToExtractUpdateArchive", "The application failed to extract the update files. Check your file permissions and antivirus application.");
            German.Add("failedToExtractUpdateArchive", "Das Programm konnte die Updatedateien nicht entpacken. Bitte überprüfe die Berechtigungen und/oder deine Anti-Virus-Software.");
            Polish.Add("failedToExtractUpdateArchive", "Aplikacja nie mogła wyodrębnić plików aktualizacji. Sprawdź swoje uprawnienia oraz oprogramowanie antywirusowe.");
            French.Add("failedToExtractUpdateArchive", "Echec de l'extraction de l'archive de mise à jour");
            Spanish.Add("failedToExtractUpdateArchive", "La aplicación no ha podido extraer los archivos de actualización. Compruebe sus permisos de archivos y antivirus.");
            Russian.Add("failedToExtractUpdateArchive", "Приложению не удалось распаковать файлы обновлений. Проверьте права доступа к файлам или переместите приложение в папку, где разрешена запись.");

            //Component: downloadingUpdate
            //
            English.Add("downloadingUpdate", "Downloading application update");
            German.Add("downloadingUpdate", "Lade Programmupdate");
            Polish.Add("downloadingUpdate", "Pobieranie aktualizacji");
            French.Add("downloadingUpdate", "Téléchargement de la mise à jour");
            Spanish.Add("downloadingUpdate", "Descargando actualización de la apliación");
            Russian.Add("downloadingUpdate", "Загружается обновление приложения");

            //Component: autoOneclickSelectionFileNotExist
            //
            English.Add("autoOneclickSelectionFileNotExist", "The supplied path to the selection file does not exist");
            German.Add("autoOneclickSelectionFileNotExist", "Der angegebene Pfad zur Auswahldatei existiert nicht.");
            Polish.Add("autoOneclickSelectionFileNotExist", "Plik kolekcji pod podaną lokacją nie istnieje.");
            French.Add("autoOneclickSelectionFileNotExist", "Le fichier d'installation automatique en un clique n'existe pas");
            Spanish.Add("autoOneclickSelectionFileNotExist", "La ruta al archivo de selección no existe");
            Russian.Add("autoOneclickSelectionFileNotExist", "Указанный путь к файлу предустановки не существует.");

            //Component: AutoOneclickSelectionErrorsContinueBody
            //When loading the selection file for "one-click" or "auto-install" modes, if there are issues with loading the selection,
            //and the user selected to be informed of issues
            //(removed components, disabled components, etc.)
            English.Add("AutoOneclickSelectionErrorsContinueBody", "There were problems loading the selection file (most likely disabled/removed packages, etc.)" +
                "\nWould you like to continue anyway?");
            German.Add("AutoOneclickSelectionErrorsContinueBody", "Es gibt Probleme mit deiner Auswahldatei. Ein oder mehrere Einträge sind derzeit nicht verfügbar." +
                "\nWillst du fortfahren?");
            Polish.Add("AutoOneclickSelectionErrorsContinueBody", "Wystąpiły problemy ze wczytywaniem pliku kolekcji (prawdobodobnie dezaktywowane/usunięte pakiety itp.)" +
                "\nCzy mimo to, chcesz kontynuować?");
            French.Add("AutoOneclickSelectionErrorsContinueBody", "Des problèmes sont survenus lors du chargement de la sélection de fichiers (Un package a probablement" +
                " été désactivé/supprimé, etc...). \nVoulez-vous tout de même continuer?");
            Spanish.Add("AutoOneclickSelectionErrorsContinueBody", "Hubo problemas cargando el archivo de selección (probablemente paquetes eliminados/deshabilitados, etc.)." +
                "\n¿Quiere continuar igualmente?");
            Russian.Add("AutoOneclickSelectionErrorsContinueBody", "Возникли проблемы при загрузке файла предустановки (вероятно, были удалены или переименованы" +
                " пакеты, и т. д.). \nХотите продолжить работу?");

            //Component: AutoOneclickSelectionErrorsContinueHeader
            //see above
            English.Add("AutoOneclickSelectionErrorsContinueHeader", "Problems loading selection file");
            German.Add("AutoOneclickSelectionErrorsContinueHeader", "Probleme beim Laden der Auswahldatei");
            Polish.Add("AutoOneclickSelectionErrorsContinueHeader", "Błędy wczytywania pliku kolekcji");
            French.Add("AutoOneclickSelectionErrorsContinueHeader", "Problèmes du chargement de la sélection de fichiers");
            Spanish.Add("AutoOneclickSelectionErrorsContinueHeader", "Problemas cargando el archivo de selección");
            Russian.Add("AutoOneclickSelectionErrorsContinueHeader", "Проблемы в импорте файла предустановки");

            //Component: noAutoInstallWithBeta
            //
            English.Add("noAutoInstallWithBeta", "Auto install mode cannot be used with the beta database");
            German.Add("noAutoInstallWithBeta", "Die automatische Installation kann nicht gemeinsam mit der Beta-Datenbank genutzt werden.");
            Polish.Add("noAutoInstallWithBeta", "Automatyczna instalacja jest niedostępna dla rozwojowej bazy danych (BETA).");
            French.Add("noAutoInstallWithBeta", "Pas d'installation automatique avec la Bétâ");
            Spanish.Add("noAutoInstallWithBeta", "El modo de instalación automática no puede ser utilizado con la base de datos en beta");
            Russian.Add("noAutoInstallWithBeta", "При использовании бета-версии БД установка в автоматическом режиме невозможна.");

            //Component: autoInstallWithBetaDBConfirmBody
            //
            English.Add("autoInstallWithBetaDBConfirmBody", "Auto install will be enabled with beta database. The beta database is updated frequenty and could result in multiple" +
                " installations in one day. Are you sure you would like to do this?");
            German.Add("autoInstallWithBetaDBConfirmBody", "Die automatische Installation wird jetzt die Beta Datenbank nutzen. Diese wird häufig aktualisiert und es" +
                " kann zu mehreren Installationen am Tag kommen. Willst du das wirklich tun?");
            Polish.Add("autoInstallWithBetaDBConfirmBody", "Automatyczna Instalacja będzie dostępna dla testowej bazy danych (BETA), która jest często aktualizowana —" +
                " może to prowadzić do kilku aktualizacji w ciągu jednego dnia!\nCzy mimo to chcesz kontynuować?");
            French.Add("autoInstallWithBetaDBConfirmBody", "L'installation automatique sera activée avec la base de données bêta. Cette base de données est mise à jour fréquemment" +
                " et peut provoquer plusieurs mises à jour le même jour. Êtes-vous sûr de vouloir faire ça ?");
            Spanish.Add("autoInstallWithBetaDBConfirmBody", "La autoinstalación será habilitada con la base de datos beta. La base de datos beta es actualizada frecuentemente," +
                " y puede resultar en múltiples instalaciones en un día. ¿Está seguro de que quiere hacer esto?");
            Russian.Add("autoInstallWithBetaDBConfirmBody", "Будет включена автоматическая установка модификаций из бета-версии БД. Она обновляется чаще основной, и вы заметите," +
                " что одни и те же моды устанавливались по нескольку раз.\nВы точно хотите включить автоматическую установку?");

            //Component: autoInstallWithBetaDBConfirmHeader
            //
            English.Add("autoInstallWithBetaDBConfirmHeader", English["verifyUninstallHeader"]);
            German.Add("autoInstallWithBetaDBConfirmHeader", German["verifyUninstallHeader"]);
            Polish.Add("autoInstallWithBetaDBConfirmHeader", Polish["verifyUninstallHeader"]);
            French.Add("autoInstallWithBetaDBConfirmHeader", French["verifyUninstallHeader"]);
            Spanish.Add("autoInstallWithBetaDBConfirmHeader", Spanish["verifyUninstallHeader"]);
            Russian.Add("autoInstallWithBetaDBConfirmHeader", Russian["verifyUninstallHeader"]);


            //Component: ColorDumpSaveFileDialog
            //
            English.Add("ColorDumpSaveFileDialog", "Select where to save the colors customization file");
            German.Add("ColorDumpSaveFileDialog", "Wähle, wo die Farbdatei gespeichert werden soll");
            Polish.Add("ColorDumpSaveFileDialog", "Wybierz miejsce zapisu pliku schematu kolorów");
            French.Add("ColorDumpSaveFileDialog", "Enregistrement du fichier de sauvegarde des couleurs");
            Spanish.Add("ColorDumpSaveFileDialog", "Seleccione dónde quiere guardar el archivo de personalización de colores");
            Russian.Add("ColorDumpSaveFileDialog", "Выберите путь для сохранения файла с настройками цвета");

            //Component: loadingBranches
            //"branch" is this context is git respoitory branches
            English.Add("loadingBranches", "Loading branches");
            German.Add("loadingBranches", "Lade Branch");
            Polish.Add("loadingBranches", "Ładowanie gałęzi");
            French.Add("loadingBranches", "Chargement des branches");
            Spanish.Add("loadingBranches", "Cargando ramas");
            Russian.Add("loadingBranches", "Загружаются ветви репозитория");

            //Component: failedToParseUISettingsFile
            //"branch" is this context is git respoitory branches
            English.Add("failedToParseUISettingsFile", "Failed to apply the theme. Check the log for details. Enable \"Verbose Logging\" for additional information.");
            German.Add("failedToParseUISettingsFile", "Fehler beim Anwenden. Überprüfe log für Details. Aktiviere \"Ausführliche Protokollierung\" für erweiterte informationen.");
            Polish.Add("failedToParseUISettingsFile", "Nie udało się zastosować motywu. Szczegóły znajdziesz w pliku dziennika. Włącz \"Rozszerzone rejestrowanie zdarzeń\" dla dodatkowych informacji.");
            French.Add("failedToParseUISettingsFile", "Echec de l'analyse du fichier d'options UI");
            Spanish.Add("failedToParseUISettingsFile", "No se ha podido aplicar el tema. Compruebe el archivo de registro para más detalles. Habilite \"Registro Verboso\" para información adicional.");
            Russian.Add("failedToParseUISettingsFile", "Не удалось применить тему. Подробности в лог-файле. Включите \"Расширенное логгирование\" для получения более детальной информации.");

            //Component: UISettingsFileApplied
            //the message when the UISettings.xml file is parsed and the custom theme is loaded
            English.Add("UISettingsFileApplied", "Theme applied");
            German.Add("UISettingsFileApplied", "Thema angewendet");
            Polish.Add("UISettingsFileApplied", "Motyw zastosowany pomyślnie.");
            French.Add("UISettingsFileApplied", "Options UI du fichier appliqué");
            Spanish.Add("UISettingsFileApplied", "Tema aplicado");
            Russian.Add("UISettingsFileApplied", "Тема применена");

            //Component: failedToFindWoTExe
            //the message when the app cannot locate the WoT.exe
            English.Add("failedToFindWoTExe", "Failed to get the WoT client installation location. Please send a bug report to the developer.");
            German.Add("failedToFindWoTExe", "Konnte den Installationsort des WoT Clienten nicht finden. Bitte sende einen Fehlerbericht an den Entwickler.");
            Polish.Add("failedToFindWoTExe", "Nie udało się odnaleźć folderu instalacji klienta WoT. Prosimy zgłosić ten błąd twórcom.");
            French.Add("failedToFindWoTExe", "Echec de la recherche de WoT.exe");
            Spanish.Add("failedToFindWoTExe", "No se ha podido localizar la instalación del cliente de WoT. Por favor, envíe un informe de errores al desarrollador.");
            Russian.Add("failedToFindWoTExe", "Не удалось получить расположение клиента WoT. Пожалуйста, отправьте отчёт об ошибке разработчику.");

            //Component: failedToFindWoTVersionXml
            //the message when the UISettings.xml file is parsed and the custom theme is loaded
            English.Add("failedToFindWoTVersionXml", "Failed to get WoT client installation version information. Check if the file 'version.xml' exists in the 'World_of_Tanks' directory.");
            German.Add("failedToFindWoTVersionXml", "Fehler beim Abrufen der Versionsinformationen für die WoT-Client-Installation. Überprüfe, ob die Datei 'version.xml' im Verzeichnis 'World_of_Tanks' vorhanden ist");
            Polish.Add("failedToFindWoTVersionXml", "Nie udało się ustalić wersji klienta WoT. Sprawdź, czy plik 'version.xml' znajduje się w folderze gry (domyślnie: World_of_Tanks).");
            French.Add("failedToFindWoTVersionXml", "Echec de la recherche de la version XML de WoT");
            Spanish.Add("failedToFindWoTVersionXml", "No se ha podido obtener información de la versión instalada de WoT. Compruebe que el archivo 'version.xml' existe en el directorio 'World_of_Tanks'.");
            Russian.Add("failedToFindWoTVersionXml", "Не удалось получить информацию о версии клиента WoT. Проверьте наличие файла 'version.xml' в папке с игрой.");
            #endregion

            #region ModSelectionList
            //Component: ModSelectionList
            //
            English.Add("ModSelectionList", "Selection List");
            German.Add("ModSelectionList", "Auswahldatei");
            Polish.Add("ModSelectionList", "Lista Wyboru Modów");
            French.Add("ModSelectionList", "Liste de sélection");
            Spanish.Add("ModSelectionList", "Lista de selección");
            Russian.Add("ModSelectionList", "Файл предустановки");

            //Component: ContinueButtonLabel
            //
            English.Add("ContinueButtonLabel", "Install");
            German.Add("ContinueButtonLabel", "Installieren");
            Polish.Add("ContinueButtonLabel", "Zainstaluj");
            French.Add("ContinueButtonLabel", "Installer");
            Spanish.Add("ContinueButtonLabel", "Instalar");
            Russian.Add("ContinueButtonLabel", "Установить");

            //Component: CancelButtonLabel
            //
            English.Add("CancelButtonLabel", English["cancel"]);
            German.Add("CancelButtonLabel", German["cancel"]);
            Polish.Add("CancelButtonLabel", Polish["cancel"]);
            French.Add("CancelButtonLabel", French["cancel"]);
            Spanish.Add("CancelButtonLabel", Spanish["cancel"]);
            Russian.Add("CancelButtonLabel", Russian["cancel"]);

            //Component: HelpLabel
            //
            English.Add("HelpLabel", "Right-click a selection component to see a preview window");
            German.Add("HelpLabel", "Klicke mit der rechten Maustaste auf eine Auswahlkomponente, um ein Vorschaufenster anzuzeigen");
            Polish.Add("HelpLabel", "Wyświetl opis dowolnego elementu, klikając na niego prawym przyciskiem myszy.");
            French.Add("HelpLabel", "Clique droit sur un composant de la sélection pour voir une fenêtre de prévisualisation");
            Spanish.Add("HelpLabel", "Haga clic derecho en un componente de selección para abrir una ventana de vista previa");
            Russian.Add("HelpLabel", "Клик правой кнопкой мыши по компоненту покажет превью");

            //Component: LoadSelectionButtonLabel
            //
            English.Add("LoadSelectionButtonLabel", "Load selection");
            German.Add("LoadSelectionButtonLabel", "Auswahl laden");
            Polish.Add("LoadSelectionButtonLabel", "Wczytaj kolekcję");
            French.Add("LoadSelectionButtonLabel", "Charger une configuration");
            Spanish.Add("LoadSelectionButtonLabel", "Cargar selección");
            Russian.Add("LoadSelectionButtonLabel", "Загрузить шаблон настроек");

            //Component: SaveSelectionButtonLabel
            //
            English.Add("SaveSelectionButtonLabel", "Save selection");
            German.Add("SaveSelectionButtonLabel", "Auswahl speichern");
            Polish.Add("SaveSelectionButtonLabel", "Zapisz kolekcję");
            French.Add("SaveSelectionButtonLabel", "Sauvegarder une configuration");
            Spanish.Add("SaveSelectionButtonLabel", "Guardar selección");
            Russian.Add("SaveSelectionButtonLabel", "Сохранить шаблон настроек");

            //Component: SelectSelectionFileToSave
            //File save dialog box when the user presses 'save selection' in Mod Selection List
            English.Add("SelectSelectionFileToSave", "Save selection file");
            German.Add("SelectSelectionFileToSave", "Speichere Asuwahldatei");
            Polish.Add("SelectSelectionFileToSave", "Zapisz plik kolekcji");
            French.Add("SelectSelectionFileToSave", "Sauvegarder la sélection de fichier");
            Spanish.Add("SelectSelectionFileToSave", "Guardar archivo de selección");
            Russian.Add("SelectSelectionFileToSave", "Сохранить предустановку");

            //Component: ClearSelectionsButtonLabel
            //
            English.Add("ClearSelectionsButtonLabel", "Clear selections");
            German.Add("ClearSelectionsButtonLabel", "Auswahl löschen");
            Polish.Add("ClearSelectionsButtonLabel", "Wyczyść pola wyboru");
            French.Add("ClearSelectionsButtonLabel", "Réinitialiser la sélection");
            Spanish.Add("ClearSelectionsButtonLabel", "Limpiar selección");
            Russian.Add("ClearSelectionsButtonLabel", "Снять все галочки");

            //Component: SearchThisTabOnlyCB
            // A checkbox under the search field SearchTB in mods selection window. Toggles the search engine to search only in user's currently active tab.
            English.Add("SearchThisTabOnlyCB", "This tab only");
            German.Add("SearchThisTabOnlyCB", "Nur diese Registerkarte");
            Polish.Add("SearchThisTabOnlyCB", "Szukaj tylko na tej zakładkce");
            French.Add("SearchThisTabOnlyCB", "Cet onglet seulement");
            Spanish.Add("SearchThisTabOnlyCB", "Sólo esta pestaña");
            Russian.Add("SearchThisTabOnlyCB", "Только в этой вкладке");

            //Component: SearchTB
            // A label for the search field to the right. Used to find mods, configs etc. in the mod selection window.
            English.Add("SearchTB", "Search for a mod: ");
            German.Add("SearchTB", "Suche einen Mod: ");
            Polish.Add("SearchTB", "Szukaj modów:  ");
            French.Add("SearchTB", "Rechercher un mod: ");
            Spanish.Add("SearchTB", "Buscar un mod: ");
            Russian.Add("SearchTB", "Найти мод: ");

            //Component: SearchTBDescription
            // A tooltip for SearchTB. Describes the use of a wild character in the search query.
            English.Add("SearchTBDescription", "You can also search for multiple name parts, separated by a * (asterisk).\nFor example: config*willster419 will display" +
                " as search result: Willster419\'s Config");
            German.Add("SearchTBDescription", "Du kannst auch nach mehreren Namensteilen suchen, getrennt durch ein * (Sternchen).\nZum Beispiel: config*willster419" +
                "  wird als Suchergebnis anzeigen: Willster419\'s Config");
            Polish.Add("SearchTBDescription", "Użyj gwiazdki (*), aby wyszukać wiele członów nazwy.\nPRZYKŁAD:\n\"config * willster419\" ZWRÓCI: " +
                " \"Willster419\'s Config\"");
            French.Add("SearchTBDescription", "Vous pouvez également rechercher plusieurs parties de nom, séparées par un * (astérisque).\nPar exemple: config *" +
                " willster419 affichera comme résultat de la recherche: Config de Willster419");
            Spanish.Add("SearchTBDescription", "También puede buscar varias partes del nombre, separadas por un * (asterisco).\n Por ejemplo: config*willster419 mostrará como resultado: Willster419\'s Config");
            Russian.Add("SearchTBDescription", "Вы так же можете искать по нескольким частям названия, разделяя их * (звёздочкой).\nК примеру, config*willster419 покажет в качестве результата поиска Willster419\'s Config");

            //Component: InstallingAsWoTVersion
            //
            English.Add("InstallingAsWoTVersion", "Installing as WoT version: {0}");
            German.Add("InstallingAsWoTVersion", "Installation als WoT Version: {0}");
            Polish.Add("InstallingAsWoTVersion", "Instalacja dla WoT w wersji: {0}");
            French.Add("InstallingAsWoTVersion", "Installation en tant que version de WoT: {0}");
            Spanish.Add("InstallingAsWoTVersion", "Instalando como versión de WoT: {0}");
            Russian.Add("InstallingAsWoTVersion", "Установка в клиент WoT версии {0}");

            //Component: userMods
            //
            English.Add("userMods", "User Mods");
            German.Add("userMods", "Benutzermodifikationen");
            Polish.Add("userMods", "Własne mody"); // Changed to "own/custom mods" since it conveys a more detailed semantic meaning than "user mods". @Nullmaruzero
            French.Add("userMods", "Mods utilisateur");
            Spanish.Add("userMods", "Mods del usuario");
            Russian.Add("userMods", "Пользовательские моды");

            //Component: FirstTimeUserModsWarning
            //
            English.Add("FirstTimeUserModsWarning", "This tab is for selecting zip files you place in the \"RelhaxUserMods\" folder. They must be zip files, and should use a root directory folder of the \"World_of_Tanks\" directory"); 
            German.Add("FirstTimeUserModsWarning", "Auf dieser Registerkarte kannst du ZIP-Dateien auswählen, welche du zuvor im Ordner \"RelhaxUserMods\" hinterlegst. Es müssen Zip-Dateien sein und sollten als Stammverzeichnisordner einen Ordner des Verzeichnisses \"World_of_Tanks\" beinhalten (zB: mods, oder res_mods)");
            Polish.Add("FirstTimeUserModsWarning", "Ta zakładka umożliwia wybór modów, które możesz umieścić w folderze \"RelhaxUserMods\"" +
                " Muszą to być archiwa ZIP używające struktury folderów takiej samej jak folder gry (domyślnie: World_of_Tanks)."); //? Ain't sure about how the root directory thing. ASSUMED: As in the same folder structure. @Nullmaruzero
            French.Add("FirstTimeUserModsWarning", "Cet onglet sert à sélectionner les fichiers zip que vous placez dans le dossier \"RelhaxUserMods\". Ils doivent être des fichiers zip et doivent utiliser un dossier de répertoire racine du répertoire \"World_of_Tanks\"");
            Spanish.Add("FirstTimeUserModsWarning", "Esta pestaña es para seleccionar archivos zip en el directorio \"RelhaxUserMods\". Deben ser archivos zip, y derían usar un directorio raíz del directorio \"World_of_Tanks\"");
            Russian.Add("FirstTimeUserModsWarning", "Данная вкладка предназначена для выбора модов, расположенных в папке \"RelhaxUserMods\". Они должны быть в виде ZIP-архивов и использовать корневую папку World of Tanks.");

            //Component: downloadingDatabase
            //
            English.Add("downloadingDatabase", "Downloading database");
            German.Add("downloadingDatabase", "Datenbank herunterladen");
            Polish.Add("downloadingDatabase", "Pobieranie bazy danych");
            French.Add("downloadingDatabase", "Téléchargement de la base de données");
            Spanish.Add("downloadingDatabase", "Descargando base de datos");
            Russian.Add("downloadingDatabase", "Загружается база данных");

            //Component: readingDatabase
            //
            English.Add("readingDatabase", "Reading database");
            German.Add("readingDatabase", "Lese Datenbank");
            Polish.Add("readingDatabase", "Przetwarzanie bazy danych");
            French.Add("readingDatabase", "Chargement de la base de données");
            Spanish.Add("readingDatabase", "Leyendo base de datos");
            Russian.Add("readingDatabase", "Читается база данных");

            //Component: loadingUI
            //
            English.Add("loadingUI", "Loading UI");
            German.Add("loadingUI", "Lade Benutzerinterface");
            Polish.Add("loadingUI", "Ładowanie interfejsu");
            French.Add("loadingUI", "Chargement de l'interface");
            Spanish.Add("loadingUI", "Cargando interfaz del usuario");
            Russian.Add("loadingUI", "Загрузка интерфейса");

            //Component: verifyingDownloadCache
            //
            English.Add("verifyingDownloadCache", "Verifying file integrity of ");
            German.Add("verifyingDownloadCache", "Überprüfen der Dateiintegrität von ");
            Polish.Add("verifyingDownloadCache", "Sprawdzanie integralności plików: ");
            French.Add("verifyingDownloadCache", "Vérification de l'intégrité de");
            Spanish.Add("verifyingDownloadCache", "Verificando la integridad de los archivos de ");
            Russian.Add("verifyingDownloadCache", "Проверяется целостность файла ");

            //Component: InstallProgressTextBoxDescription
            //
            English.Add("InstallProgressTextBoxDescription", "Progress of an installation will be shown here");
            German.Add("InstallProgressTextBoxDescription", "Der Fortschritt einer Installation wird hier angezeigt");
            Polish.Add("InstallProgressTextBoxDescription", "Postęp instalacji będzie wyświetlany tutaj.");
            French.Add("InstallProgressTextBoxDescription", "Le progrès d'une installation sera afficher ici");
            Spanish.Add("InstallProgressTextBoxDescription", "El progreso de una instalación será mostrado aquí");
            Russian.Add("InstallProgressTextBoxDescription", "Прогресс текущей установки будет показан здесь");

            //Component: testModeDatabaseNotFound
            //
            English.Add("testModeDatabaseNotFound", "CRITICAL: TestMode Database not found at:\n{0}");
            German.Add("testModeDatabaseNotFound", "KRITISCH: Die Datanbank für den Testmodus wurde nicht gefunden:\n{0}");
            Polish.Add("testModeDatabaseNotFound", "BŁĄD KRYTYCZNY: Baza danych Trybu Testowego nie została znaleziona w lokacji:\n{0}");
            French.Add("testModeDatabaseNotFound", "CRITIQUE: Impossible de trouver la base de données du mode de test situé a: \n{0}");
            Spanish.Add("testModeDatabaseNotFound", "CRÍTICO: No se ha encontrado base de datos del modo de testeo en:\n{0}");
            Russian.Add("testModeDatabaseNotFound", "КРИТИЧЕСКАЯ ОШИБКА: Тестовая БД не найдена по адресу:\n{0}");


            //Component:
            //
            English.Add("duplicateMods", "CRITICAL: Duplicate package ID detected");
            German.Add("duplicateMods", "KRITISCH: Doppelte Paket-ID erkannt");
            Polish.Add("duplicateMods", "OSTRZEŻENIE: Wykryto zduplikowany identyfikator pakietu!");
            French.Add("duplicateMods", "CRITIQUE: Duplication de Package ID détectée");
            Spanish.Add("duplicateMods", "CRÍTICO: Detectada ID de paquete duplicada");
            Russian.Add("duplicateMods", "КРИТИЧЕСКАЯ ОШИБКА: Обнаружен дубликат пакета с таким же ID");


            //Component:
            //
            English.Add("databaseReadFailed", "CRITICAL: Failed to read database\n\nSee logfile for detailed info");
            German.Add("databaseReadFailed", "KRITISCH: Datenbank konnte nicht gelesen werden! \n\n In der Protokolldatei stehen weitere Informationen zu diesem Fehler");
            Polish.Add("databaseReadFailed", "BŁĄD KRYTYCZNY: Nie udało się przetworzyć bazy danych!\n\nSzczegóły znajdziesz w pliku dziennika.");
            French.Add("databaseReadFailed", "CRITIQUE: Impossible de lire la base de données");
            Spanish.Add("databaseReadFailed", "CRÍTICO: No se ha podido leer la base de datos\n\nVer archivo de registro para información detallada");
            Russian.Add("databaseReadFailed", "КРИТИЧЕСКАЯ ОШИБКА: Ошибка чтения базы данных!\nПодробности в log-файле");


            //Component:
            //
            English.Add("configSaveSuccess", "Selection Saved Successfully");
            German.Add("configSaveSuccess", "Auswahl erfolgreich gespeichert");
            Polish.Add("configSaveSuccess", "Kolekcja zapisana pomyślnie.");
            French.Add("configSaveSuccess", "Sélection sauvergardée avec succès");
            Spanish.Add("configSaveSuccess", "Selección guardada correctamente");
            Russian.Add("configSaveSuccess", "Предустановка успешно сохранена");

            //Component:
            //
            English.Add("selectConfigFile", "Find a selection file to load");
            German.Add("selectConfigFile", "Eine zu ladende Auswahldatei finden");
            Polish.Add("selectConfigFile", "Wybierz plik kolekcji do wczytania");
            French.Add("selectConfigFile", "Trouver un fichier de sélection à charger");
            Spanish.Add("selectConfigFile", "Busca un archivo de selección para cargar");
            Russian.Add("selectConfigFile", "Найти файл с предустановкой для загрузки");

            //Component:
            //
            English.Add("configLoadFailed", "The selection file could not be loaded, loading in standard mode");
            German.Add("configLoadFailed", "Die Auswahldatei konnte nicht geladen werden und wurde im Standardmodus geladen");
            Polish.Add("configLoadFailed", "Nie udało się załadować pliku kolekcji. Otwieranie w trybie standardowym.");
            French.Add("configLoadFailed", "Le fichier de sélection ne peut pas être charger, chargement en mode standard");
            Spanish.Add("configLoadFailed", "El archivo de selección no pudo ser cargado, cargando en modo estándar");
            Russian.Add("configLoadFailed", "Файл предустановки не может быть загружен, работа будет продолжена в обычном режиме");

            //Component:
            //
            English.Add("modNotFound", "The package (ID = \"{0}\") was not found in the database. It could have been renamed or removed.");
            German.Add("modNotFound", "Das Paket (ID = \"{0}\") wurde nicht in der Datenbank gefunden. Es könnte umbenannt oder entfernt worden sein");
            Polish.Add("modNotFound", "Pakiet (ID = „{0}”) nie został znaleziony w bazie danych. Mógł zostać zmieniony lub usunięty.");
            French.Add("modNotFound", "Le package (ID = \"{0\") n'a pas été trouver dans la base de données. Il peut avoir été renommer ou supprimer");
            Spanish.Add("modNotFound", "El paquete (ID = \"{0}\") no se ha encontrado en la base de datos. Puede haber sido renombrado o eliminado");
            Russian.Add("modNotFound", "Пакет (ID = \"{0}\") не найден в базе данных. Вероятно, он был переименован или удалён.");

            //Component:
            //
            English.Add("modDeactivated", "The following packages are currently deactivated in the modpack and could not to be selected to install");
            German.Add("modDeactivated", "Die folgenden Pakete sind deaktiviert und können nciht zur Installation gewählt werden");
            Polish.Add("modDeactivated", "Poniższe pakiety zostały na chwilę obecną dezaktywowane i nie mogą być zainstalowane");
            French.Add("modDeactivated", "Le package (ID = \"{0\") est actuellement désactivé dans le modpack et ne peut pas être sélectionner à l'installation");
            Spanish.Add("modDeactivated", "Los siguientes paquetes están actualmente desactivados en el modpack y no han podido ser seleccionados para instalar");
            Russian.Add("modDeactivated", "Данные пакеты на данный момент отключены в модпаке и не могут быть выбраны для установки");

            //Component:
            //
            English.Add("modsNotFoundTechnical", "The following packages could not be found and were most likely removed");
            German.Add("modsNotFoundTechnical", "Die folgenden Pakete konnten nicht gefunden werden und wurden wahrscheinlich entfernt");
            Polish.Add("modsNotFoundTechnical", "Nie udało się znaleźć poniższych pakietów, prawdopodobnie zostały usunięte");
            French.Add("modsNotFoundTechnical", "Les packages suivants sont introuvables et ont été probablement supprimer");
            Spanish.Add("modsNotFoundTechincal", "Los siguientes paquetes no han sido encontrados, y han sido probablemente eliminados");
            Russian.Add("modsNotFoundTechnical", "Не удалось найти данные пакеты. Вероятно, они были удалены.");


            //Component:
            //
            English.Add("modsBrokenStructure", "The following packages were disabled due to modifications in the package structure. You need to re-check them if you want to install them");
            German.Add("modsBrokenStructure", "Die folgenden Pakete wurden aufgrund Modifizierungen an der Datenbankstruktur deaktiviert und müssen neu ausgewählt werden.");
            Polish.Add("modsBrokenStructure", "Poniższe pakiety zostały na chwilę obecną dezaktywowane z powodu zmian w strukturze pakietu. Jeśli chcesz je zainstalować, musisz je zaznaczyć ponownie.");
            French.Add("modsBrokenStructure", "Les packages suivant ont été désactivés dû à une modification dans la structure du package. Vous devez la revérifier si vous voulez les installer");
            Spanish.Add("modsBrokenStructure", "Los siguientes paquetes han sido deshabilitados debido a modificaciones en la estructura de paquetes. Deberá volver a seleccionarlos si quiere instalarlos.");
            Russian.Add("modsBrokenStructure", "Данные пакеты были отключены в связи с изменениями в их структуре. Вам нужно проверить их самостоятельно, если хотите произвести установку.");

            //Component: packagesUpdatedShouldInstall
            //
            English.Add("packagesUpdatedShouldInstall", "The following packages were updated since you last loaded this selection file. Your selection file has been updated with the changes (a one-time backup was also made). " +
                "If this is your current installation, and you wish to keep it, it is recommended to install/update after seeing this message.");
            German.Add("packagesUpdatedShouldInstall", "Die folgenden Pakete bekamen ein Update seitdem du das letzte Mal diese Auswahldatei geladen hast. Die Datei wurde mit den Änderungen neu gespeichert (und ein einmaliges Backup wurde erstellt). " +
                "Falls dies deine aktuelle Installation ist und du diese behalten möchtest, ist es empfehlenswert eine (Neu-)Installation auszuführen wenn du diese Nachricht liest.");
            Polish.Add("packagesUpdatedShouldInstall", "Od czasu ostatniego użycia tego pliku kolekcji, poniższe pakiety zostały zaktualizowane. Twój plik kolekcji został automatycznie zaktualizowany (utworzono też jego jednorazową kopię zapasową)." +
                "Jeśli jest to Twoja bieżąca instalacja i chcesz ją zachować, zaleca się instalację/aktualizację po tej wiadomości.");
            French.Add("packagesUpdatedShouldInstall", TranslationNeeded);
            Spanish.Add("packagesUpdatedShouldInstall", "Los siguientes paquetes han sido actualizados desde la última carga de este archivo de selección. Su archivo de selección ha sido actualizado con los cambios (y se ha creado una copia de seguridad de un sólo uso). " +
                "Si ésta es su instalación actual, y quiere conservarla, se recomienda instalar/actualizar después de ver este mensaje.");
            Russian.Add("packagesUpdatedShouldInstall",  "Следующие пакеты были обновлены с момента последней загрузки данного файла предустановки. Файл предустановки был обновлён и изменён (его резервная копия так же была сделана). Если это ваша основная конфигурация и вы хотите её сохранить, рекомендуем вам установить/обновить моды после этого уведомления.");

            //Component: selectionFileIssuesTitle
            //
            English.Add("selectionFileIssuesTitle", "Selection loading messages");
            German.Add("selectionFileIssuesTitle", "Auswahl Ladenachrichten");
            Polish.Add("selectionFileIssuesTitle", "Uwaga"); // Following RU pattern.
            French.Add("selectionFileIssuesTitle", TranslationNeeded);
            Spanish.Add("selectionFileIssuesTitle", "Mensajes de carga de la selección");
            Russian.Add("selectionFileIssuesTitle", "Внимание"); // I don't think it's possible to translate that directly, so leaving it as "Warning" - DrWeb7_1

            //Component: selectionFileIssuesHeader
            //
            English.Add("selectionFileIssuesHeader", "Please read the following messages about loading your selection file");
            German.Add("selectionFileIssuesHeader", "Bitte lies folgende Nachrichten über das Laden deiner Auswahldatei");
            Polish.Add("selectionFileIssuesHeader", "Proszę zapoznać się z poniższymi komunikatami dotyczącymi Twojego pliku kolekcji");
            French.Add("selectionFileIssuesHeader", TranslationNeeded);
            Spanish.Add("selectionFileIssuesHeader", "Por favor, lea los siguientes mensajes sobre la carga de su archivo de selección");
            Russian.Add("selectionFileIssuesHeader", "Пожалуйста, прочтите следующие сообщения, связанные с вашим файлом предустановки");

            //Component: selectionFormatOldV2
            //the message box to show up when your selection file is old (v2) and will be upgraded
            English.Add("selectionFormatOldV2", "This selection file format is legacy (V2) and will be upgraded to V3. A V2 backup will be created.");
            German.Add("selectionFormatOldV2", TranslationNeeded);
            Polish.Add("selectionFormatOldV2", "Format tego pliku kolekcji jest przestarzały (V2), zostanie stworzona jego kopia zapasowa i zostanie on zaktualizowany do V3.");
            French.Add("selectionFormatOldV2", TranslationNeeded);
            Spanish.Add("selectionFormatOldV2", TranslationNeeded);
            Russian.Add("selectionFormatOldV2", "Этот файл предустановки сохранён в устаревшем формате (v2) и будет обновлён до v3. Резервная копия старого файла также будет сделана.");

            //Component:
            //
            English.Add("oldSavedConfigFile", "The saved preferences file your are using is in an outdated format and will be inaccurate in the future. Convert it to the new format? (A backup of the old format will be made)");
            German.Add("oldSavedConfigFile", "Die Konfigurationsdatei die benutzt wurde, wird in Zukunft immer ungenauer werden. Soll auf das neue Standardformat umgestellt werden? (Eine Sicherung des alten Formats erfolgt)");
            Polish.Add("oldSavedConfigFile", "Używana konfiguracja jest w przestarzałym formacie i będzie niekompatybilna w przyszłości. Czy chcesz przekonwertować ją na nowy format?");
            French.Add("oldSavedConfigFile", "Le fichier de préférences que vous avez choisi est un format obsolète et sera inexact dans le futur. Convertir au nouveau format?");
            Spanish.Add("oldSavedConfigFile", "El archivo de configuración que está utilizando tiene un formato antiguo y no será preciso en el futuro. ¿Convertir al nuevo formato? (Se guardará una copia de seguridad del formato original)");
            Russian.Add("oldSavedConfigFile", "Сохранённый файл конфигурации использует устаревший формат и может некорректно работать в будущем. Хотите преобразовать его в новый? Бэкап старого будет так же сохранён.");

            //Component:
            //
            English.Add("prefrencesSet", "Preferences Set");
            German.Add("prefrencesSet", "Bevorzugte Einstellungen");
            Polish.Add("prefrencesSet", "Ustawienia zastosowane pomyślnie.");
            French.Add("prefrencesSet", "Préférences Enregistrées");
            Spanish.Add("preferencesSet", "Preferencias guardadas");
            Russian.Add("prefrencesSet", "Настройки применены");

            //Component:
            //
            English.Add("selectionsCleared", "Selections Cleared");
            German.Add("selectionsCleared", "Auswahl gelöscht");
            Polish.Add("selectionsCleared", "Odznaczono wszystkie elementy.");
            French.Add("selectionsCleared", "Sélections effacées");
            Spanish.Add("selectionsCleared", "Selecciones limpiadas");
            Russian.Add("selectionsCleared", "Список выбранных модов очищен");

            //Component: failedLoadSelection
            //
            English.Add("failedLoadSelection", "Failed to load selection");
            German.Add("failedLoadSelection", "Konnte Auswahl nicht laden");
            Polish.Add("failedLoadSelection", "Nie udało się wczytać kolekcji");
            French.Add("failedLoadSelection", "Echec du chargement de la sélection");
            Spanish.Add("failedLoadSelection", "No se ha podido cargar la selección");
            Russian.Add("failedLoadSelection", "Сбой загрузки предустановки");

            //Component: unknownselectionFileFormat
            //
            English.Add("unknownselectionFileFormat", "Unknown selection file version");
            German.Add("unknownselectionFileFormat", "Unbekannte Version der Auswahldatei");
            Polish.Add("unknownselectionFileFormat", "Nieznany format pliku kolekcji");
            French.Add("unknownselectionFileFormat", "Version du fichier sélectionner inconnue");
            Spanish.Add("unknownselectionFileFormat", "Versión desconocida del archivo de selección");
            Russian.Add("unknownselectionFileFormat", "Неизвестная версия файла предустановки");

            //Component: ExpandAllButton
            //
            English.Add("ExpandAllButton", "Expand Current Tab");
            German.Add("ExpandAllButton", "Erweitere aktuelle Registerkarte");
            Polish.Add("ExpandAllButton", "Rozwiń bieżącą kartę");
            French.Add("ExpandAllButton", "Elargir l'onglet");
            Spanish.Add("ExpandAllButton", "Expandir pestaña actual");
            Russian.Add("ExpandAllButton", "Раскрыть текущую вкладку");

            //Component: CollapseAllButton
            //
            English.Add("CollapseAllButton", "Collapse Current Tab");
            German.Add("CollapseAllButton", "Reduziere aktuelle Registerkarte");
            Polish.Add("CollapseAllButton", "Zwiń bieżącą kartę");
            French.Add("CollapseAllButton", "Réduire l'onglet");
            Spanish.Add("CollapseAllButton", "Colapsar pestaña actual");
            Russian.Add("CollapseAllButton", "Свернуть текущую вкладку");

            //Component: InstallingTo
            //
            English.Add("InstallingTo", "Installing to: {0}");
            German.Add("InstallingTo", "Installiere nach: {0}");
            Polish.Add("InstallingTo", "Folder instalacji: {0}");
            French.Add("InstallingTo", "Installation à: {0}");
            Spanish.Add("InstallingTo", "Instalando en: {0}");
            Russian.Add("InstallingTo", "Установка в {0}");

            //Component: saveConfig
            //
            English.Add("selectWhereToSave", "Select where to save selection file");
            German.Add("selectWhereToSave", "Wähle aus, wo die Auswahldatei gespeichert werden soll");
            Polish.Add("selectWhereToSave", "Wybierz miejsce zapisu swojej kolekcji");
            French.Add("selectWhereToSave", "Sélectionner où sauvegarder la sélection de fichier");
            Spanish.Add("selectWhereToSave", "Seleccionar dónde guardar el archivo de selección");
            Russian.Add("selectWhereToSave", "Выберите путь для сохранения файла предустановки");

            //Component: updated
            //shows (updated) next to a component
            English.Add("updated", "updated");
            German.Add("updated", "aktualisiert");
            Polish.Add("updated", "zaktualizowano");
            French.Add("updated", "Mis à jours");
            Spanish.Add("updated", "actualizado");
            Russian.Add("updated", "обновлён");

            //Component: disabled
            //shows (disabled) next to a component
            English.Add("disabled", "disabled");
            German.Add("disabled", "deaktiviert");
            Polish.Add("disabled", "dezaktywowany");
            French.Add("disabled", "Désactivé");
            Spanish.Add("disabled", "deshabilitado");
            Russian.Add("disabled", "отключен");

            //Component: invisible
            //shows (invisible) next to a component
            English.Add("invisible", "invisible");
            German.Add("invisible", "unsichtbar");
            Polish.Add("invisible", "ukryty");
            French.Add("invisible", "Invisible");
            Spanish.Add("invisible", "invisible");
            Russian.Add("invisible", "невидим");

            //Component: SelectionFileIssuesDisplay
            //window title for when issues applying a user's selection
            English.Add("SelectionFileIssuesDisplay", "Errors applying selection file");
            German.Add("SelectionFileIssuesDisplay", "Fehler beim Anwenden der Auswahldatei");
            Polish.Add("SelectionFileIssuesDisplay", "Błędy z zastosowaniem pliku kolekcji");
            French.Add("SelectionFileIssuesDisplay", TranslationNeeded);
            Spanish.Add("SelectionFileIssuesDisplay", "Errores al aplicar el archivo de selección");
            Russian.Add("SelectionFileIssuesDisplay", "Ошибка применения файла предустановки");

            //Component: selectionFileIssues
            //alias of SelectionFileIssuesDisplay
            English.Add("selectionFileIssues", English["SelectionFileIssuesDisplay"]);
            German.Add("selectionFileIssues", German["SelectionFileIssuesDisplay"]);
            Polish.Add("selectionFileIssues", Polish["SelectionFileIssuesDisplay"]);
            French.Add("selectionFileIssues", French["SelectionFileIssuesDisplay"]);
            Spanish.Add("selectionFileIssues", Spanish["SelectionFileIssuesDisplay"]);
            Russian.Add("selectionFileIssues", Russian["SelectionFileIssuesDisplay"]);
            #endregion

            #region Application Update Window
            //Component: VersionInfo
            //
            English.Add("VersionInfo", "Application Update");
            German.Add("VersionInfo", "Porgrammaktualisierung");
            Polish.Add("VersionInfo", "Aktualizacja Aplikacji");
            French.Add("VersionInfo", "Mise à jour de l'application");
            Spanish.Add("VersionInfo", "Actualizacón de la aplicación");
            Russian.Add("VersionInfo", "Обновление приложения");

            //Component: VersionInfoYesButton
            // A button in VersionInfo window confirming the installation of the new available version of the application.
            English.Add("VersionInfoYesButton", English["yes"]);
            German.Add("VersionInfoYesButton", German["yes"]);
            Polish.Add("VersionInfoYesButton", Polish["yes"]);
            French.Add("VersionInfoYesButton", French["yes"]);
            Spanish.Add("VersionInfoYesButton", Spanish["yes"]);
            Russian.Add("VersionInfoYesButton", Russian["yes"]);

            //Component: VersionInfoNoButton
            // A button in VersionInfo window declining the installation of the new available version of the application.
            English.Add("VersionInfoNoButton", English["no"]);
            German.Add("VersionInfoNoButton", German["no"]);
            Polish.Add("VersionInfoNoButton", Polish["no"]);
            French.Add("VersionInfoNoButton", French["no"]);
            Spanish.Add("VersionInfoNoButton", Spanish["no"]);
            Russian.Add("VersionInfoNoButton", Russian["no"]);

            //Component: NewVersionAvailable
            //
            English.Add("NewVersionAvailable", "New version available");
            German.Add("NewVersionAvailable", "Neue version verfügbar");
            Polish.Add("NewVersionAvailable", "Nowa wersja dostępna");
            French.Add("NewVersionAvailable", "Nouvelle version disponible");
            Spanish.Add("NewVersionAvailable", "Nueva versión disponible");
            Russian.Add("NewVersionAvailable", "Доступна новая версия");

            //Component: HavingProblemsTextBlock
            // Text on the bottom of the Updater window, followed by ManualUpdateLink with hard-coded empty space (not the key) in between.
            English.Add("HavingProblemsTextBlock", "If you are having problems updating, please");
            German.Add("HavingProblemsTextBlock", "Wenn du Probleme mit der Aktualisierung hast, bitte");
            Polish.Add("HavingProblemsTextBlock", "Jeśli masz problem z aktualizacją,");
            French.Add("HavingProblemsTextBlock", "Si vous avez des problèmes avec la mise à jour, s'il vous plaît");
            Spanish.Add("HavingProblemsTextBlock", "Si tiene problemas actualizando, por favor");
            Russian.Add("HavingProblemsTextBlock", "При наличии проблем в процессе обновлений, пожалуйста,");

            //Component: ManualUpdateLink
            // Link following HavingProblemsTextBlock text. Preceeded by hard-coded empty space (not the key).
            English.Add("ManualUpdateLink", "Click Here");
            German.Add("ManualUpdateLink", "Klick Hier");
            Polish.Add("ManualUpdateLink", "kliknij tutaj");
            French.Add("ManualUpdateLink", "Cliquez Ici");
            Spanish.Add("ManualUpdateLink", "Haga clic aquí");
            Russian.Add("ManualUpdateLink", "нажмите сюда.");

            //Component: loadingApplicationUpdateNotes
            //
            English.Add("loadingApplicationUpdateNotes", "Loading application update notes...");
            German.Add("loadingApplicationUpdateNotes", "Anwendungsaktualisierungsnotizen werden geladen...");
            Polish.Add("loadingApplicationUpdateNotes", "Wczytywanie historii wersji aplikacji...");
            French.Add("loadingApplicationUpdateNotes", "Chargement des notes de mise à jour de l'application ...");
            Spanish.Add("loadingApplicationUpdateNotes", "Cargando notas de la actualización de la aplicación...");
            Russian.Add("loadingApplicationUpdateNotes", "Загружается список изменений в обновлении...");

            //Component: ViewUpdateNotesOnGoogleTranslate
            //
            English.Add("ViewUpdateNotesOnGoogleTranslate", "View this on Google Translate");
            German.Add("ViewUpdateNotesOnGoogleTranslate", "Sieh dir das auf Google Translate an");
            Polish.Add("ViewUpdateNotesOnGoogleTranslate", "Wyświetl w Tłumaczu Google");
            French.Add("ViewUpdateNotesOnGoogleTranslate", "Voir les notes de mise à jour sur Google Traduction");
            Spanish.Add("ViewUpdateNotesOnGoogleTranslate", "Ver en Traductor de Google");
            Russian.Add("ViewUpdateNotesOnGoogleTranslate", "Посмотреть через переводчик Google");

            //Component: VersionInfoAskText
            // Text in the window with Yes and No buttons asking the user if he/she wants to update the application
            English.Add("VersionInfoAskText", "Do you wish to update now?");
            German.Add("VersionInfoAskText", "Willst du jetzt updaten?");
            Polish.Add("VersionInfoAskText", "Czy chcesz zaktualizować teraz?");
            French.Add("VersionInfoAskText", "Voulez vous faire la mise à jour maintenant?");
            Spanish.Add("VersionInfoAskText", "¿Quiere actualizar ahora?");
            Russian.Add("VersionInfoAskText", "Хотите обновить прямо сейчас?");
            #endregion

            #region Installer Messages
            //Component:
            //
            English.Add("Downloading", "Downloading");
            German.Add("Downloading", "Wird heruntergeladen");
            Polish.Add("Downloading", "Pobieranie");
            French.Add("Downloading", "Téléchargement");
            Spanish.Add("Downloading", "Descargando");
            Russian.Add("Downloading", "Идёт скачивание");

            //Component:
            //
            English.Add("patching", "Patching");
            German.Add("patching", "Patching");
            Polish.Add("patching", "Patchowanie");
            French.Add("patching", "Patching");
            Spanish.Add("patching", "Parcheando");
            Russian.Add("patching", "Применение патчей");

            //Component:
            //
            English.Add("done", "Done");
            German.Add("done", "Fertig");
            Polish.Add("done", "Ukończono");
            French.Add("done", "Terminé");
            Spanish.Add("done", "Hecho");
            Russian.Add("done", "Готово");

            //Component:
            //
            English.Add("cleanUp", "Clean up resources");
            German.Add("cleanUp", "Bereinige Ressourcen");
            Polish.Add("cleanUp", "Oczyszczanie zasobów");
            French.Add("cleanUp", "Nettoyer les ressources");
            Spanish.Add("cleanUp", "Limpiando recursos");
            Russian.Add("cleanUp", "Очистка ресурсов");

            //Component:
            //
            English.Add("idle", "Idle");
            German.Add("idle", "Leerlauf");
            Polish.Add("idle", "Bezczynny");
            French.Add("idle", "En attente");
            Spanish.Add("idle", "En espera");
            Russian.Add("idle", "Ожидание");

            //Component:
            //
            English.Add("status", "Status:");
            German.Add("status", "Status:");
            Polish.Add("status", "Stan:");
            French.Add("status", "Statut");
            Spanish.Add("status", "Estado:");
            Russian.Add("status", "Состояние:");

            //Component:
            //
            English.Add("canceled", "Canceled");
            German.Add("canceled", "Abgebrochen");
            Polish.Add("canceled", "Anulowano");
            French.Add("canceled", "Annulé");
            Spanish.Add("canceled", "Cancelado");
            Russian.Add("canceled", "Отменено");

            //Component:
            //
            English.Add("appSingleInstance", "Checking for single instance");
            German.Add("appSingleInstance", "Prüfung auf Einzelinstanz");
            Polish.Add("appSingleInstance", "Sprawdzanie pojedynczej instancji");
            French.Add("appSingleInstance", "Vérification d'instance unique");
            Spanish.Add("appSingleInstance", "Comprobando instancia única");
            Russian.Add("appSingleInstance", "Проверяется наличие только одного процесса");

            //Component:
            //
            English.Add("checkForUpdates", "Checking for updates");
            German.Add("checkForUpdates", "Auf Updates prüfen");
            Polish.Add("checkForUpdates", "Szukanie aktualizacji");
            French.Add("checkForUpdates", "Vérification de mise à jours");
            Spanish.Add("checkForUpdates", "Comprobando actualizaciones");
            Russian.Add("checkForUpdates", "Проверяется наличие обновлений");

            //Component:
            //
            English.Add("verDirStructure", "Verifying directory structure");
            German.Add("verDirStructure", "Verifizierung der Verzeichnisstruktur");
            Polish.Add("verDirStructure", "Sprawdzanie struktury katalogów");
            French.Add("verDirStructure", "Vérification de la structure du directoire");
            Spanish.Add("verDirStructure", "Verificando estructura del directorio");
            Russian.Add("verDirStructure", "Проверяется структура каталогов");

            //Component:
            //
            English.Add("loadingSettings", "Loading settings");
            German.Add("loadingSettings", "Einstellungen laden");
            Polish.Add("loadingSettings", "Wczytywanie ustawnień");
            French.Add("loadingSettings", "Chargement des paramètres");
            Spanish.Add("loadingSettings", "Cargando opciones");
            Russian.Add("loadingSettings", "Загружаются настройки");

            //Component:
            //
            English.Add("loadingTranslations", "Loading translations");
            German.Add("loadingTranslations", "Laden der Übersetzungen");
            Polish.Add("loadingTranslations", "Wczytywanie tłumaczeń");
            French.Add("loadingTranslations", "Chargement des traductions");
            Spanish.Add("loadingTranslations", "Cargando traducciones");
            Russian.Add("loadingTranslations", "Загружаются переводы на другие языки");

            //Component:
            //
            English.Add("loading", "Loading");
            German.Add("loading", "Laden");
            Polish.Add("loading", "Wczytywanie");
            French.Add("loading", "Chargement de");
            Spanish.Add("loading", "Cargando");
            Russian.Add("loading", "Загрузка");

            //Component:
            //
            English.Add("of", "of");
            German.Add("of", "von");
            Polish.Add("of", "z");
            French.Add("of", "de");
            Spanish.Add("of", "de");
            Russian.Add("of", "из");

            //Component: failedToDownload1
            //
            English.Add("failedToDownload1", "Failed to download the package");
            German.Add("failedToDownload1", "Konnte Paket nicht herunterladen");
            Polish.Add("failedToDownload1", "Nie udało się pobrać pakietu");
            French.Add("failedToDownload1", "Echec du téléchargement du package");
            Spanish.Add("failedToDownload1", "No se ha podido descargar el paquete");
            Russian.Add("failedToDownload1", "Сбой загрузки пакета");

            //Component: failedToDownload2
            //
            English.Add("failedToDownload2", "Would you like to retry the download, abort the installation, or continue?");
            German.Add("failedToDownload2", "Willst du den Download wiederholen, die Installation abbrechen oder weitermachen?");
            Polish.Add("failedToDownload2", "Spróbować pobrać ponownie, przerwać instalację czy kontynuować?");
            French.Add("failedToDownload2", "Vouls-vous essayer à nouveau le téléchargement, annuler l'installation, ou continuer ?");
            Spanish.Add("failedToDownload2", "¿Quiere reintentar la descarga, abortar la instalación, o continuar?");
            Russian.Add("failedToDownload2", "Хотите попробовать ещё раз, прервать или продолжить установку?");

            //Component:
            //
            English.Add("failedToDownloadHeader", "Failed to download");
            German.Add("failedToDownloadHeader", "Fehler beim Herunterladen");
            Polish.Add("failedToDownloadHeader", "Ściąganie zakończone niepowodzeniem");
            French.Add("failedToDownloadHeader", "Échec de téléchargement");
            Spanish.Add("failedToDownloadHeader", "Fallo en la descarga");
            Russian.Add("failedToDownloadHeader", "Сбой загрузки");

            //Component: update check against online app version
            //
            English.Add("failedManager_version", "Current Beta application is outdated and must be updated against stable channel. No new Beta version online now.");
            German.Add("failedManager_version", "Die aktuelle Beta-Anwendung ist veraltet und muss für einen stabilen Kanal aktualisiert werden. Derzeit ist keine neue Beta-Version online");
            Polish.Add("failedManager_version", "Aktualna wersja rozwojowa (BETA) aplikacji jest nieaktualna i musi zostać zaktualizowana do wersji stabilnej. Brak nowej wersji beta w trybie online.");
            French.Add("failedManager_version", "L'application Beta actuelle est dépassée et doit être mise à jour. Pas de nouvelle version Beta en ligne actuellement.");
            Spanish.Add("failedManager_version", "La versión beta actual de la aplicación está anticuada y debe ser actualizada. No hay una versión Beta nueva funcionando actualmente");
            Russian.Add("failedManager_version", "Данная бета-версия не актуальна и должна быть обновлена через стабильный канал. Новых бета-версий в данный момент нет.");

            //Component:
            //
            English.Add("fontsPromptInstallHeader", "Admin to install fonts?");
            German.Add("fontsPromptInstallHeader", "Admin zum Installieren von Schriftarten?");
            Polish.Add("fontsPromptInstallHeader", "Uprawnienia Administratora");
            French.Add("fontsPromptInstallHeader", "Admin pour installer les polices?");
            Spanish.Add("fontsPromptInstallHeader", "¿Administrador para instalar fuentes?");
            Russian.Add("fontsPromptInstallHeader", "Права администратора для установки шрифтов?");

            //Component:
            //
            English.Add("fontsPromptInstallText", "Do you have admin rights to install fonts?");
            German.Add("fontsPromptInstallText", "Hast Du Admin-Rechte um Schriftarten zu installieren?");
            Polish.Add("fontsPromptInstallText", "Czy posiadasz uprawnienia administratora, aby zainstalować czcionki?");
            French.Add("fontsPromptInstallText", "Avez-vous les droits d'administrateur installer des polices?");
            Spanish.Add("fontsPromptInstallText", "¿Tiene permisos de administrador para instalar fuentes?");
            Russian.Add("fontsPromptInstallText", "У вас есть права администратора, необходимые для установки шрифтов?");

            //Component:
            //
            English.Add("fontsPromptError_1", "Unable to install fonts. Some mods may not work properly. Fonts are located in ");
            German.Add("fontsPromptError_1", "Schriftarten können nicht installiert werden. Einige Mods funktionieren möglicherweise nicht richtig. Schriften befinden sich in ");
            Polish.Add("fontsPromptError_1", "Instalacja czcionek nie powiodła się. Niektóre mody mogą nie działać prawidłowo. Czcionki znajdują się w ");
            French.Add("fontsPromptError_1", "Impossible d'installer les polices. Certain mods peut mal fonctionner. Les polices sont situé dans ");
            Spanish.Add("fontsPromptError_1", "No se han podido instalar fuentes. Algunos mods pueden no funcionar correctamente. Fuentes disponibles en ");
            Russian.Add("fontsPromptError_1", "Не удалось установить шрифты. Некоторые моды могут работать некорректно. Шрифты расположены в ");

            //Component:
            //
            English.Add("fontsPromptError_2", "\\_fonts. Either install them yourself or run this again as Administrator.");
            German.Add("fontsPromptError_2", "\\_fonts. Entweder installiere sie selbst oder führe den Relhax Manager erneut als Administrator aus.");
            Polish.Add("fontsPromptError_2", "\\_fonts. Zainstaluj je ręcznie lub uruchom aplikację jako administrator.");
            French.Add("fontsPromptError_2", "\\_fonts. Installez les polices manuellement ou redémarrez avec les droits Administrateur");
            Spanish.Add("fontsPromptError_2", "\\_fonts. Puede instalarlas usted mismo o volver a ejecutar la instalación como administrador");
            Russian.Add("fontsPromptError_2", "\\_fonts. Попробуйте установить их самостоятельно или перезапустите программу от имени администратора.");

            //Component:
            //
            English.Add("cantDownloadNewVersion", "Unable to download new version, exiting.");
            German.Add("cantDownloadNewVersion", "Die neue Version kann nicht heruntergeladen werden.");
            Polish.Add("cantDownloadNewVersion", "Nie udało się pobrać nowej wersji, zamykanie aplikacji.");
            French.Add("cantDownloadNewVersion", "Échec du téléchargement des mise à jours. Fermeture.");
            Spanish.Add("cantDownloadNewVersion", "No ha sido posible descargar la nueva versión. Cerrando el programa.");
            Russian.Add("cantDownloadNewVersion", "Невозможно загрузить новую версию, приложение будет закрыто.");

            //Component:
            //
            English.Add("failedCreateUpdateBat", "Unable to create update process.\n\nPlease manually delete the file:\n{0}\n\nrename file:\n{1}\nto:\n{2}\n\nDirectly jump to the folder?");
            German.Add("failedCreateUpdateBat", "Updateprozess kann leider nicht erstellt werden.\n\nLösche bitte diese Datei von Hand:\n{0}\n\nbenennte diese" +
                " Datei:\n{1}\nin diese um:\n{2}\n\nDirekt zum Ordner springen?");
            Polish.Add("failedCreateUpdateBat", "Nie można utworzyć procesu aktualizatora.\n\nProszę ręcznie usunąć plik:\n{0}\n\nZmienić nazwę pliku:\n{1}\nna:\n{2}\n\nCzy chcesz otworzyć lokalizację pliku?");
            French.Add("failedCreateUpdateBat", "fichier:\n{0}\n\nrenamefile:\n{1}\nto:\n{2}\n\nAfficher le dossier?");
            Spanish.Add("failedCreatureUpdateBat", "No ha sido posible crear el proceso de actualización.\n\nPor favor, elimine manualmente el archivo:\n{0}\n\nrenombre el archivo:\n{1}\na:\n{2}\n¿Ir al directorio?");
            Russian.Add("failedCreateUpdateBat", "Невозможно создать процесс программы обновления.\n\nУдалите вручную этот файл:\n{0}\n\nПереименуйте файл:\n{1}\nна:\n{2}\n\nПерейти к папке прямо сейчас?");

            //Component:
            //
            English.Add("cantStartNewApp", "Unable to start application, but it is located in \n");
            German.Add("cantStartNewApp", "Kann die Anwendung nicht starten, aber sie befindet sich in \n");
            Polish.Add("cantStartNewApp", "Niepowodzenie przy uruchamianiu aplikacji znajdującej się w \n");
            French.Add("cantStartNewApp", "Échec du lancement de l'application, mais elle est situé dans \n");
            Spanish.Add("cantStartNewApp", "No ha sido posible arrancar la aplicación, pero se encuentra en \n");
            Russian.Add("cantStartNewApp", "Не удалось запустить приложение, но оно расположено в \n");

            //Component:
            //
            English.Add("autoDetectFailed", "The auto-detection failed. Please use the 'force manual' option");
            German.Add("autoDetectFailed", "Die automatische Erkennung ist fehlgeschlagen. Bitte benutze die 'erzwinge manuelle Spielerkennung' Option");
            Polish.Add("autoDetectFailed", "Niepowodzenie automatycznego wykrywania. Użyj opcji ręcznego wybierania lokacji gry.");
            French.Add("autoDetectFailed", "Échec de la détection automatique. Utilisez l'option 'Forcer détection manuel'");
            Spanish.Add("autoDetectFailed", "La detección automática ha fallado. Por favor, use la opción de 'forzar detección manual'");
            Russian.Add("autoDetectFailed", "Не удалось автоматически обнаружить игру. Используйте опцию \"Принудительно указать папку с игрой\".");

            //Component: MainWindow_Load
            //
            English.Add("anotherInstanceRunning", "Another Instance of the Relhax Manager is already running");
            German.Add("anotherInstanceRunning", "Eine weitere Instanz des Relhax Manager  läuft bereits");
            Polish.Add("anotherInstanceRunning", "Inna instancja Relhax Manager jest już uruchomiona");
            French.Add("anotherInstanceRunning", "Une autre instance de Relhax Directeur est en cours d`éxecution");
            Spanish.Add("anotherInstanceRunning", "Hay otra instancia de Relhax Manager ejecutándose");
            Russian.Add("anotherInstanceRunning", "Запущен ещё один процесс Relhax Manager");

            //Component: closeInstanceRunningForUpdate
            //
            English.Add("closeInstanceRunningForUpdate", "Please close ALL running instances of the Relhax Manager before we can go on and update.");
            German.Add("closeInstanceRunningForUpdate", "Bitte schließe ALLE laufenden Instanzen des Relhax Managers bevor wir fortfahren und aktualisieren können.");
            Polish.Add("closeInstanceRunningForUpdate", "Proszę zamknąć WSZYSTKIE działające instancje Relhax Modpack przed dalszym procesem aktualizacji.");
            French.Add("closeInstanceRunningForUpdate", "Merci de fermé toutes les instances du modpack relhax avant que nous puissions procéder à la mise à jour");
            Spanish.Add("closeInstanceRunningForUpdate", "Por favor, cierre TODAS las instancias de Relhax Manager en ejecución antes de poder actualizar.");
            Russian.Add("closeInstanceRunningForUpdate", "Пожалуйста, закройте ВСЕ запущенные процессы Relhax Manager перед тем, как мы сможем продолжить работу и обновление.");

            //Component:
            //
            English.Add("skipUpdateWarning", "You are skipping updating. Database Compatibility is not guaranteed");
            German.Add("skipUpdateWarning", "Du überspringst die Aktualisierung. Datenbankkompatibilität ist nicht garantiert");
            Polish.Add("skipUpdateWarning", "Pomijasz aktualizację! Może wystąpić niekompatybilność bazy danych.");
            French.Add("skipUpdateWarning", "Vous ignorez la mise à jour. Compatibilité de la base de données non garanti ");
            Spanish.Add("skipUpdateWarning", "Está saltándose la actualización. La compatibilidad de la base de datos no está garantizada");
            Russian.Add("skipUpdateWarning", "Вы пропустили обновление. Совместимость базы данных не гарантирована.");

            //Component:
            //
            English.Add("patchDayMessage", "The modpack is currently down for patch day testing and mods updating. Sorry for the inconvenience." +
                " If you are a database manager, please add the command argument");
            German.Add("patchDayMessage", "Das Modpack ist zur Zeit für Patch-Tag-Tests und das Updaten von Mods nicht erreichbar. Entschuldige die Unannehmlichkeiten." +
                " Wenn Du ein Datenbankmanager bist, füge bitte das Befehlsargument hinzu");
            Polish.Add("patchDayMessage", "Paczka na chwilę obecną jest nieaktywna z powodu prac konserwacyjnych i aktualizacji modów dla nowej wersji WoT. Przepraszamy za utrudnienia." +
                " Jeśli jesteś zarządcą bazy danych, uruchom aplikację z odpowiednim argumentem wiersza poleceń"); // ASSUMED: Command line parameter (added to the shortcut).
            French.Add("patchDayMessage", "Le pack mod est actuellement indisponible aujourd'hui pour tester et mettre à jour les mods. Désolé pour le dérangement." +
                " Si vous êtes un gestionnaire de base de données, ajoutez l'argument de commande.");
            Spanish.Add("patchDayMessage", "El modpack está actualmente inactivo para testear y actualizar mods durante el día de parche. Sentimos las inconveniencias cusadas.");
            Russian.Add("patchDayMessage", "В настоящее время модпак недоступен в связи с выходом патча, как и обновлений модов. Приносим извинения за неудобства." +
                "Если вы работаете с базой данных модпака, то добавьте аргумент командной строки");

            //Component:
            //
            English.Add("configNotExist", "{0} does NOT exist, loading in regular mode");
            German.Add("configNotExist", "{0} existiert nicht, lädt im regulären Modus");
            Polish.Add("configNotExist", "{0} nie istnieje, wczytywanie trybu normalnego");
            French.Add("configNotExist", "{0} n'existe pas, chargement en mode normal");
            Spanish.Add("configNotExist", "{0} NO existe, cargando en modo normal");
            Russian.Add("configNotExist", "{0} НЕ существует, запуск в обычном режиме");

            //Component:
            //
            English.Add("autoAndFirst", "First time loading cannot be an auto install mode, loading in regular mode");
            German.Add("autoAndFirst", "Erstmaliges Laden kann kein automatischer Installationsmodus sein, lade im regulären Modus");
            Polish.Add("autoAndFirst", "Pierwsze uruchomienie nie może być automatyczną instalacją, wczytywanie trybu normalnego");
            French.Add("autoAndFirst", "Le premier lancement ne peut pas être un mode d'installation automatique, chargement en mode normal");
            Spanish.Add("autoAndFirst", "La primera vez no se puede cargar en modo autoinstalación, cargando en modo normal");
            Russian.Add("autoAndFirst", "Первый запуск не может быть произведён в режиме автоматической установкой, запуск в обычном режиме.");

            //Component:
            //
            English.Add("confirmUninstallHeader", "Confirmation");
            German.Add("confirmUninstallHeader", "Bestätigung");
            Polish.Add("confirmUninstallHeader", "Potwierdzenie");
            French.Add("confirmUninstallHeader", "Confirmation");
            Spanish.Add("confirmUninstallHeader", "Confirmación");
            Russian.Add("confirmUninstallHeader", "Подтверждение");

            //Component:
            //
            English.Add("confirmUninstallMessage", "Confirm you wish to uninstall mods from WoT installation\n\n{0}\n\nUsing uninstall method '{1}'?");
            German.Add("confirmUninstallMessage", "Bitte bestätige das du alle Mods von Deinem WoT deinstalliert haben möchtest\n\n{0}\n\nVerwendung von Deinstallationsmethode '{1}'?");
            Polish.Add("confirmUninstallMessage", "Potwierdź odinstalowanie modów z WoT\n\n{0}\n\nUżyć metody '{1}'?");
            French.Add("confirmUninstallMessage", "Confirmer que vous voulez désinstaller les mods du dossier WoT\n\n{0}\n\nUsing la méthode de désinstallation '{1}'?");
            Spanish.Add("confirmUninstallMessage", "Confirme que quiere desinstalar mods de la instalación de WoT\n\n{0}\n\n¿usando el método de desinstalación '{1}'?");
            Russian.Add("confirmUninstallMessage", "Подтвердите необходимость удалить моды для WoT в этой папке: \n\n{0}\n\nИспользуем метод '{1}'?");

            //Component:
            //
            English.Add("uninstallingText", "Uninstalling...");
            German.Add("uninstallingText", "Deinstalliere...");
            Polish.Add("uninstallingText", "Dezinstalacja w toku...");
            French.Add("uninstallingText", "Désinstallation...");
            Spanish.Add("uninstallingText", "Desinstalando...");
            Russian.Add("uninstallingText", "Удаление...");

            //Component:
            //progress message
            English.Add("uninstallingFile", "Uninstalling file");
            German.Add("uninstallingFile", "Deinstalliere datei");
            Polish.Add("uninstallingFile", "Odinstalowywanie pliku");
            French.Add("uninstallingFile", "Désinstallation du fichier");
            Spanish.Add("uninstallingFile", "Desinstalando archivo");
            Russian.Add("uninstallingFile", "Удаляется файл");

            //Component: uninstallfinished messagebox
            //
            English.Add("uninstallFinished", "Uninstallation of mods finished.");
            German.Add("uninstallFinished", "Deinstallation der mods beendet.");
            Polish.Add("uninstallFinished", "Dezinstalacja modów zakończona");
            French.Add("uninstallFinished", "Désinstallation des mods terminé");
            Spanish.Add("uninstallFinished", "Desinstalación de los mods terminada.");
            Russian.Add("uninstallfinished", "Удаление модов завершено.");

            //Component: uninstallFail
            //
            English.Add("uninstallFail", "The uninstallation failed. You could try another uninstallation mode or submit a bug report.");
            German.Add("uninstallFail", "Das Deinstallieren war nicht erfolgreich. Bitte wiederhole den Vorgang oder sende einen Fehlerbericht.");
            Polish.Add("uninstallFail", "Nie udało się odinstalować. Spróbuj innej metody dezinstalacji lub poinformuj nas o tym błędzie.");
            French.Add("uninstallFail", "La désinstallation a échouée. Vous pouvez essayer un autre mode de désinstallation ou envoyer un signalement.");
            Spanish.Add("uninstallFail", "La desinstalación ha fallado. Puede intentar otro modo de desinstalación o enviar un informe de error.");
            Russian.Add("uninstallFail", "Не удалось завершить деинсталляцию. Вы можете попробовать другой метод или отправить отчёт об ошибке.");

            //Component:
            //
            English.Add("extractionErrorMessage", "Error deleting the folder res_mods or mods. Either World of Tanks is running or your file" +
                " and folder security permissions are incorrect");
            German.Add("extractionErrorMessage", "Fehler beim Löschen des Ordners res_mods oder Mods. Entweder World of Tanks läuft oder deine Datei-" +
                " und Ordnersicherheitsberechtigungen sind falsch.");
            Polish.Add("extractionErrorMessage", "Błąd usuwania folderu res_mods lub mods. Albo World of Tanks jest obecnie uruchomione," +
                " albo uprawnienia dostępu do plików i folderów są ustawione nieprawidłowo.");
            French.Add("extractionErrorMessage", "Erreur lors de la supression du dossier res_mods ou un/plusieur mods. Sois que World of Tanks est en" +
                " cours d`éxecution ou les permissions de sécuriter sont incorrect.");
            Spanish.Add("extractionErrorMessage", "Error eliminando las carpetas 'res_mods' o 'mods'. World of Tanks está en ejecución o bien" +
                " sus permisos de seguridad de archivos y carpetas son incorrectos.");
            Russian.Add("extractionErrorMessage", "Возникла ошибка при удалении папки res_mods или mods. Возможно, запущен World of Tanks или неверно" +
                " настроены разрешения к папкам и файлам.");

            //Component: extractionErrorHeader
            //
            English.Add("extractionErrorHeader", English["error"]);
            German.Add("extractionErrorHeader", German["error"]);
            Polish.Add("extractionErrorHeader", Polish["error"]);
            French.Add("extractionErrorHeader", French["error"]);
            Spanish.Add("extractionErrorHeader", Spanish["error"]);
            Russian.Add("extractionErrorHeader", Russian["error"]);

            //Component:
            //
            English.Add("deleteErrorHeader", "close out of folders");
            German.Add("deleteErrorHeader", "Ausschliessen von Ordnern");
            Polish.Add("deleteErrorHeader", "Zamknij foldery");
            French.Add("deleteErrorHeader", "Fermez les dossiers");
            Spanish.Add("deleteErrorHeader", "Cierre las carpetas");
            Russian.Add("deleteErrorHeader", "Закройте папки");

            //Component:
            //
            English.Add("deleteErrorMessage", "Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");
            German.Add("deleteErrorMessage", "Bitte schließe alle Explorer-Fenster in Mods oder res_mods (oder Unterordner), und klicke auf OK um fortzufahren.");
            Polish.Add("deleteErrorMessage", "Proszę zamknąć folder mods lub res_mods (lub podfoldery), a następnie kliknij OK, żeby kontynuować.");
            French.Add("deleteErrorMessage", "Veuillez fermer les fenêtre res_mods ou mods (Ou tout sous-dossiers) et cliquez Ok pour continuer");
            Russian.Add("deleteErrorMessage", "Закройте окна проводника, в которых открыты mods или res_mods (или глубже), и нажмите OK, чтобы продолжить.");

            //Component:
            //
            English.Add("noUninstallLogMessage", "The log file containing the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");
            German.Add("noUninstallLogMessage", "Die Protokolldatei mit der Liste der installierten Dateien (installedRelhaxFiles.log) existiert nicht. Möchtest Du alle Mods stattdessen entfernen?");
            Polish.Add("noUninstallLogMessage", "Plik dziennika zawierający listę instalacyjną (installedRelhaxFiles.log) nie istnieje. Czy chcesz usunąć wszystkie modyfikacje?");
            French.Add("noUninstallLogMessage", "Le ficher log contenant la liste des fichiers installé (installedRelhaxFiles.log) n'existe pas. Voulez vous supprimez tout les mods ?");
            Spanish.Add("noUninstallLogMessage", "El archivo de registro que contiene la lista de archivos instalados (installedRelhaxFiles.log) no existe. ¿Quiere eliminar todos los mods en su lugar?");
            Russian.Add("noUninstallLogMessage", "Файл со списком установленных файлов (installedRelhaxFiles.log) не существует. Хотите удалить все установленные моды в таком случае?");

            //Component:
            //
            English.Add("noUninstallLogHeader", "Remove all mods");
            German.Add("noUninstallLogHeader", "Entferne alle Mods");
            Polish.Add("noUninstallLogHeader", "Usuń wszystkie mody");
            French.Add("noUninstallLogHeader", "Supprimer tous les mods");
            Spanish.Add("noUninstallLogHeader", "Eliminar todos los mods");
            Russian.Add("noUninstallLogHeader", "Удалить все моды");

            //Component: moveOutOfTanksLocation
            //
            English.Add("moveOutOfTanksLocation", "The modpack can not be run from the World_of_Tanks directory. Please move the application and try again.");
            German.Add("moveOutOfTanksLocation", "Das Modpack kann nicht aus dem World_of_Tanks Verzeichnis laufen. Bitte verschiebe die Anwendung in ein" +
                " anderes Verzeichnis und versuche es erneut.");
            Polish.Add("moveOutOfTanksLocation", "Modpack nie może być uruchamiany z katalogu World_of_Tanks. Przenieś aplikację i spróbuj ponownie.");
            French.Add("moveOutOfTanksLocation", "Le Modpack ne peut pas être éxecuté a partir du dossier de World of Tanks. Veuillez déplacer l`application" +
                " dans un autre dossier et réessayer");
            Spanish.Add("moveOutOfTanksLocation", "El modpack no puede ser ejecutado desde el directorio de World_of_Tanks. Por favor, mueva la aplicación y vuelva a intentarlo.");
            Russian.Add("moveOutOfTanksLocation", "Модпак не может быть запущен из папки с игрой. Пожалуйста, переместите его в другую папку и попробуйте ещё раз.");

            //Component: moveAppOutOfDownloads
            // Many users download the application right from the website and run it in the downloads folder. This is not reccomended
            English.Add("moveAppOutOfDownloads", "The application detected that it is launched from the 'Downloads' folder. This is not recommended because the application creates several folders and files " +
                "that may be difficult to find in a large 'Downloads' folder. You should move the application and all 'Relhax' files and folders into a new folder.");
            German.Add("moveAppOutOfDownloads", TranslationNeeded);
            Polish.Add("moveAppOutOfDownloads", "Wykryto, że aplikacja została uruchomiona z folderu 'Pobrane'." +
                "Nie jest to zalecane z racji tworzenia przez aplikację wielu plików i folderów, które mogą być trudne do znalezienia w folderze 'Pobrane' z dużą ilością plików." +
                "Zaleca się przeniesienie aplikacji i oraz jej plików do nowego/osobnego folderu.");
            French.Add("moveAppOutOfDownloads", TranslationNeeded);
            Spanish.Add("moveAppOutOfDownloads", TranslationNeeded);
            Russian.Add("moveAppOutOfDownloads", "Приложение было запущено из папки \"Загрузки\". Мы не рекомендуем использовать эту папку, поскольку приложение создаёт несколько папок и файлов, поиск которых может быть затруднительным в папке с загрузками. Вы должны переместить приложение и файлы/папки Relhax в другое расположение.");

            //Component: Current database is same as last installed database (body)
            //
            English.Add("DatabaseVersionsSameBody", "The database has not changed since your last installation. Therefore there are no updates to your current mods selection." +
                " Continue anyway?");
            German.Add("DatabaseVersionsSameBody", "Die Datenbank  wurde seit deiner letzten Installation nicht verändert. Daher gibt es keine Aktuallisierungen zu deinen aktuellen" +
                " Modifikationen. Trotzdem fortfahren?");
            Polish.Add("DatabaseVersionsSameBody", "Baza danych nie została zaktualizowana od ostatniej instalacji — nie ma żadych aktualizacji dla ostatnio zainstalowanych modów.\n" +
                "Czy nadal chcesz kontynuować?");
            French.Add("DatabaseVersionsSameBody", "La base de données n'a pas changé depuis votre dernière installation. Par conséquent, il n'y a pas de mise à jour pour votre sélection" +
                " de mods. Continuer de toute façon?");
            Spanish.Add("DatabaseVersionsSameBody", "La base de datos no ha cambiado desde su última instalación. Por tanto no hay actualizaciones para su selección de mods actual. ¿Continuar de todas formas?");
            Russian.Add("DatabaseVersionsSameBody", "База данных не менялась с момента последней установки. В то же время не было обновлений к выбранным вами модам. Продолжить в любом случае?");

            //Component: Current database is same as last installed database (header)
            //
            English.Add("DatabaseVersionsSameHeader", "Database version is the same");
            German.Add("DatabaseVersionsSameHeader", "Datenbank Version ist identisch");
            Polish.Add("DatabaseVersionsSameHeader", "Brak aktualizacji zainstalowanych modów.");
            French.Add("DatabaseVersionsSameHeader", "La version de la base de données est la même");
            Spanish.Add("DatabaseVersionsSameHeader", "La versión de la base de datos es idéntica.");
            Russian.Add("DatabaseVersionsSameHeader", "Одинаковые версии БД");

            //Component:
            //
            English.Add("databaseNotFound", "Database not found at supplied URL");
            German.Add("databaseNotFound", "Datenbank nicht an der angegebenen URL gefunden");
            Polish.Add("databaseNotFound", "Nie znaleziono bazy danych pod wskazanym URL");
            French.Add("databaseNotFound", "Base de données introuvable à L'URL fournie");
            Spanish.Add("databaseNotFound", "No se ha encontrado base de datos en la URL especificada");
            Russian.Add("databaseNotFound", "По запрошенному адресу база данных не найдена");

            //Component: Detected client version
            //
            English.Add("detectedClientVersion", "Detected Client version");
            German.Add("detectedClientVersion", "Erkannte Client Version");
            Polish.Add("detectedClientVersion", "Wykryta wersja klienta");
            French.Add("detectedClientVersion", "Version du client détecté");
            Spanish.Add("detectedClientVersion", "Versión del cliente detectada");
            Russian.Add("detectedClientVersion", "Обнаруженная версия клиента");

            //Component: Supported client versions
            //
            English.Add("supportedClientVersions", "Supported Clients");
            German.Add("supportedClientVersions", "Unterstützte Clients");
            Polish.Add("supportedClientVersions", "Wspierane wersje klienta");
            French.Add("supportedClientVersions", "Clients supportés");
            Spanish.Add("supportedClientVersions", "Versiones del cliente soportadas");
            Russian.Add("supportedClientVersions", "Поддерживаемые версии клиента");

            //Component: Supported clients notice
            //
            English.Add("supportNotGuarnteed", "This client version is not offically supported. Mods may not work.");
            German.Add("supportNotGuarnteed", "Diese Client Version wird (noch) nicht offiziell unterstützt. Die Mods könnten nicht funktionieren oder sogar Dein World of Tanks beschädigen.");
            Polish.Add("supportNotGuarnteed", "Ta wersja klienta gry nie jest oficjalnie wspierana. Mody mogą działać nieprawidłowo.\n"); // Line-break to fit with 'couldTryBeta' below. @Nullmaruzero
            French.Add("supportNotGuarnteed", "Ce client n'est pas supporté officiellement. Les mods risquent de ne pas fonctionner.");
            Spanish.Add("supportNotGuaranteed", "Esta versión del cliente no está oficialmente soportada. Algunos mods pueden no funcionar.");
            Russian.Add("supportNotGuarnteed", "Эта версия клиента официально не поддерживается. Моды могут не работать.");

            //Component: couldTryBeta
            //
            English.Add("couldTryBeta", "If a game patch was recently released, the team is working on supporting it. You could try using the beta database.");
            German.Add("couldTryBeta", "Falls gerade ein Spiel-Patch veröffentlicht wurde, arbeitet das Team an einer Unterstützung der aktuellen Version. Du kannst versuchen die Beta-Datenbank zu nutzen.");
            Polish.Add("couldTryBeta", "Jeśli niedawo została wydana nowa wersja lub aktualizacja WoT, będziemy potrzebować trochę czasu na aktualizację paczki.\n\nW międzyczasie możesz spróbować użyć wersji rozwojowej (BETA) bazy danych.");
            French.Add("couldTryBeta", "Si un patch du jeu a récemment été libéré, l'équipe travaille afin de rendre le modpack compatible. Vous pouvez essayer d'utiliser la base de données béta.");
            Spanish.Add("couldTryBeta", "Si el juego ha sido recientemente actualizado, el equipo está trabajando en proporcionarle soporte. Puede intentar usar la base de datos en beta.");
            Russian.Add("couldTryBeta", "Если недавно был выпущен патч, то команда разработчиков занята актуализацией модпака. Вы можете попробовать бета-версию БД.");

            //Component: missingMSVCPLibrariesHeader
            //
            English.Add("missingMSVCPLibrariesHeader", "Failed to load required libraries");
            German.Add("missingMSVCPLibrariesHeader", "Fehler beim Laden erforderlicher Bibliotheksdateien");
            Polish.Add("missingMSVCPLibrariesHeader", "Nie udało się wczytać wymaganych bibliotek");
            French.Add("missingMSVCPLibrariesHeader", "Echec du chargement des bibliothèques requises");
            Spanish.Add("missingMSVCPLibrariesHeader", "No se han podido cargar las librerías necesarias");
            Russian.Add("missingMSVCPLibrariesHeader", "Сбой загрузки необходимых библиотек");

            //Component: missingMSVCPLibraries
            //Microsoft Visual C++ 2013 libraries (msvcp120.dll, etc.) are required to load and process atlas images
            English.Add("missingMSVCPLibraries", "The contour icon image processing libraries failed to load. This could indicate you are missing a required Microsoft dll package.");
            German.Add("missingMSVCPLibraries", "Die Bibliotheken für die Konturbildverarbeitung konnten nicht geladen werden. Das könnte auf ein fehlendes Microsoft DLL-Paket hinweisen.");
            Polish.Add("missingMSVCPLibraries", "Nie udało się wczytać bibliotek odpowiedzialnych za przetwarzanie obrazów ikon. Prawdopodobnie wymagane biblioteki DLL nie są zainstalowane.");
            French.Add("missingMSVCPLibraries", "Impossible de charger les bibliothèques de traitement d'image des icônes de contour. Cela pourrait indiquer qu'il vous manque un  package de dll microsoft");
            Spanish.Add("missingMSVCPLibraries", "No se han podido cargar las librerías de procesamiento de imágenes de iconos de contorno. Esto puede indicar que le falta un paquete requerido .dll de Microsoft");
            Russian.Add("missingMSVCPLibraries", "Не удалось загрузить библиотеку обработчика контурных иконок. Возможно, это признак того, что у вас отсутствует одна из DLL-библиотек Microsoft.");

            //Component: openLinkToMSVCP
            //Microsoft Visual C++ 2013 libraries (msvcp120.dll, etc.) are required to load and process atlas images
            English.Add("openLinkToMSVCP", "Would you like to open your browser to the package download page?");
            German.Add("openLinkToMSVCP", "Die Seite für den Paketdownload im Browser anzeigen?");
            Polish.Add("openLinkToMSVCP", "Czy chcesz przejsć do strony pobierania wymaganego pakietu?");
            French.Add("openLinkToMSVCP", "Voulez-vous ouvrir votre navigateur à la page de téléchargement du package ?");
            Spanish.Add("openLinkToMSVCP", "¿Quiere abrir su navegador en la página de descarga del paquete?");
            Russian.Add("openLinkToMSVCP", "Хотите открыть браузер, чтобы скачать установочный пакет?");

            //Component: notifying the user the change won't take effect until application restart
            //
            English.Add("noChangeUntilRestart", "This option won't take effect until application restart");
            German.Add("noChangeUntilRestart", "Diese Option hat keine Auswirkungen bis das Programm neu gestartet wurde");
            Polish.Add("noChangeUntilRestart", "Zmiany zostaną zastosowane po ponownym uruchomieniu aplikacji.");
            French.Add("noChangeUntilRestart", "Cette option ne prendra effet qu'au redémarrage de l'application");
            Spanish.Add("noChangeUntilRestart", "Esta opción no tendrá efecto hasta reiniciar la aplicación");
            Russian.Add("noChangeUntilRestart", "Для применения настроек потребуется перезапуск программы");

            //Component: installBackupMods
            //
            English.Add("installBackupMods", "Backing up mod file");
            German.Add("installBackupMods", "Mod Dateien sichern");
            Polish.Add("installBackupMods", "Tworzenie kopii zapasowej pliku");
            French.Add("installBackupMods", "Sauvegarde du fichier mod");
            Spanish.Add("installBackupMods", "Haciendo una copia de seguridad del mod");
            Russian.Add("installBackupMods", "Создаётся бэкап мода");

            //Component: installBackupData
            //
            English.Add("installBackupData", "Backing up user data");
            German.Add("installBackupData", "Benutzerdaten sichern");
            Polish.Add("installBackupData", "Tworzenie kopii zapasowej danych użytkownika");
            French.Add("installBackupData", "Sauvegarde des données utilisateur");
            Spanish.Add("installBackupData", "Haciendo una copia de seguridad de la configuración del usuario");
            Russian.Add("installBackupData", "Создаётся бэкап пользовательских данных");

            //Component: installClearCache
            //
            English.Add("installClearCache", "Deleting WoT cache");
            German.Add("installClearCache", "WoT Zwischenspeicher löschen");
            Polish.Add("installClearCache", "Usuwanie pamięci podręcznej WoT");
            French.Add("installClearCache", "Suppression du cache WoT");
            Spanish.Add("installClearCache", "Eliminando la caché del WoT");
            Russian.Add("installClearCache", "Удаляется кэш WoT");

            //Component: installClearLogs
            //
            English.Add("installClearLogs", "Deleting log files");
            German.Add("installClearLogs", "Protokolldateien löschen");
            Polish.Add("installClearLogs", "Usuwanie plików dziennika");
            French.Add("installClearLogs", "Suppression des fichiers logs");
            Spanish.Add("installClearLogs", "Eliminando archivos de registro");
            Russian.Add("installClearLogs", "Удаляются log-файлы");

            //Component: installCleanMods
            //
            English.Add("installCleanMods", "Cleaning mods folders");
            German.Add("installCleanMods", "Bereinige Mods-Ordner");
            Polish.Add("installCleanMods", "Oczyszczanie folderu modów");
            French.Add("installCleanMods", "Nettoyage du dossier des mods");
            Spanish.Add("installCleanMods", "Limpiando carpetas de mods");
            Russian.Add("installCleanMods", "Очищаются папки модов");

            //Component: installExtractingMods
            //
            English.Add("installExtractingMods", "Extracting package");
            German.Add("installExtractingMods", "Entpacke Paket");
            Polish.Add("installExtractingMods", "Wyodrębnianie pakietu");
            French.Add("installExtractingMods", "Extraction du package");
            Spanish.Add("installExtractingMods", "Extrayendo paquete");
            Russian.Add("installExtractingMods", "Распаковывается пакет");

            //Component: installZipFileEntry
            //
            English.Add("installZipFileEntry", "File entry");
            German.Add("installZipFileEntry", "Dateneingang");
            Polish.Add("installZipFileEntry", "Element"); // Saw it used as a progress counter, it fits in this context. @Nullmaruzero
            French.Add("installZipFileEntry", "Entrée de fichier");
            Spanish.Add("installZipFileEntry", "Entrada de archivo");
            Russian.Add("installZipFileEntry", "Файл");

            //Component: installExtractingCompletedThreads
            //
            English.Add("installExtractingCompletedThreads", "Completed extraction threads");
            German.Add("installExtractingCompletedThreads", "Vollständige Enpackungsvorgänge");
            Polish.Add("installExtractingCompletedThreads", "Zakończone procesy wyodrębniania");
            French.Add("installExtractingCompletedThreads", "Fils d'extraction terminés");
            Spanish.Add("installExtractingCompletedThreads", "Completados hilos de extracción");
            Russian.Add("installExtractingCompletedThreads", "Завершено потоков установки");

            //Component: installExtractingOfGroup
            //
            English.Add("installExtractingOfGroup", "of install group");
            German.Add("installExtractingOfGroup", "der Installations Gruppe");
            Polish.Add("installExtractingOfGroup", "z grupy instalacyjnej");
            French.Add("installExtractingOfGroup", "de groupes d'installation");
            Spanish.Add("installExtractingOfGroup", "del grupo de instalación");
            Russian.Add("installExtractingOfGroup", "из установочной группы");

            //Component: extractingUserMod
            //
            English.Add("extractingUserMod", "Extracting user package");
            German.Add("extractingUserMod", "Entpacke Benutzermod");
            Polish.Add("extractingUserMod", "Wyodrębnianie paczek użytkownika");
            French.Add("extractingUserMod", "Extraction de package utilisateur");
            Spanish.Add("extractingUserMod", "Extrayendo paquete de usuario");
            Russian.Add("extractingUserMod", "Распаковывается пользовательский пакет");

            //Component: installRestoreUserdata
            //
            English.Add("installRestoreUserdata", "Restoring user data");
            German.Add("installRestoreUserdata", "Benutzerdaten wiederherstellen");
            Polish.Add("installRestoreUserdata", "Przywracanie danych użytkownika");
            French.Add("installRestoreUserdata", "Restoration des données utilisateur");
            Spanish.Add("installRestoreUserdata", "Restableciendo configuración del usuario");
            Russian.Add("installRestoreUserdata", "Восстанавливаются пользовательские данные");

            //Component: installXmlUnpack
            //
            English.Add("installXmlUnpack", "Unpacking XML file");
            German.Add("installXmlUnpack", "Entpacke XML Datei");
            Polish.Add("installXmlUnpack", "Wyodrębnianie plików XML");
            French.Add("installXmlUnpack", "Déballage de fichier XML");
            Spanish.Add("installXmlUnpack", "Desempaquetando archivo XML");
            Russian.Add("installXmlUnpack", "Распаковка XML-файла");

            //Component: installPatchFiles
            //
            English.Add("installPatchFiles", "Patching file");
            German.Add("installPatchFiles", "Datei wird geändert");
            Polish.Add("installPatchFiles", "Aktualizowanie plików");
            French.Add("installPatchFiles", "Patch du fichier");
            Spanish.Add("installPatchFiles", "Parcheando archivo");
            Russian.Add("installPatchFiles", "Применяется патч к файлу");

            //Component: installShortcuts
            //
            English.Add("installShortcuts", "Creating shortcuts");
            German.Add("installShortcuts", "Erstelle Verknüpfungen");
            Polish.Add("installShortcuts", "Tworzenie skrótów");
            French.Add("installShortcuts", "Création de raccourcis");
            Spanish.Add("installShortcuts", "Creando accesos directos");
            Russian.Add("installShortcuts", "Создаются ярлыки");

            //Component: installContourIconAtlas
            English.Add("installContourIconAtlas", "Creating Atlas file");
            German.Add("installContourIconAtlas", "Erstelle Atlas Datei");
            Polish.Add("installContourIconAtlas", "Tworzenie pliku Atlas");
            French.Add("installContourIconAtlas", "Creations des fichiers Atlas");
            Spanish.Add("installContourIconAtlas", "Creando archivo de Atlas");
            Russian.Add("installContourIconAtlas", "Создаётся файл-атлас");

            //Component: installFonts
            //
            English.Add("installFonts", "Installing fonts");
            German.Add("installFonts", "Installieren von Schriftarten");
            Polish.Add("installFonts", "Instalowanie czcionek");
            French.Add("installFonts", "Installation des polices");
            Spanish.Add("installFonts", "Instalando fuentes");
            Russian.Add("installFonts", "Устанавливаются шрифты");

            //Component: installCleanup
            //
            English.Add("installCleanup", "Cleaning Up");
            German.Add("installCleanup", "Räume auf");
            Polish.Add("installCleanup", "Czyszczenie");
            French.Add("installCleanup", "Nettoyer");
            Spanish.Add("installCleanup", "Limpiando");
            Russian.Add("installCleanup", "Очистка...");

            //Component: ExtractAtlases
            English.Add("AtlasExtraction", "Extracting Atlas file");
            German.Add("AtlasExtraction", "Entpacke Atlas Datei");
            Polish.Add("AtlasExtraction", "Wyodrębnianie pliku Atlas");
            French.Add("AtlasExtraction", "Extraction des fichiers Atlas");
            Spanish.Add("AtlasExtraction", "Extrayendo archivo de Atlas");
            Russian.Add("AtlasExtraction", "Распаковывается файл-атлас");

            //Component: copyingFile
            //
            English.Add("copyingFile", "Copying file");
            German.Add("copyingFile", "Kopieren von Dateien");
            Polish.Add("copyingFile", "Kopiowanie plików");
            French.Add("copyingFile", "Copie des fichiers");
            Spanish.Add("copyingFile", "Copiando archivo");
            Russian.Add("copyingFile", "Копирование файла");

            //Component: deletingFile
            //
            English.Add("deletingFile", "Deleting file");
            German.Add("deletingFile", "Lösche Datei");
            Polish.Add("deletingFile", "Usuwanie plików");
            French.Add("deletingFile", "Supression du fichier");
            Spanish.Add("deletingFile", "Eliminando archivo");
            Russian.Add("deletingFile", "Удаление файла");

            //Component scanningModsFolders
            //
            English.Add("scanningModsFolders", "Scanning mods folders ...");
            German.Add("scanningModsFolders", "Durchsuche Mod Verzeichnisse ...");
            Polish.Add("scanningModsFolders", "Analizowanie folderu modów");
            French.Add("scanningModsFolders", "Scan des dossiers mods...");
            Spanish.Add("scanningModsFolders", "Escaneando carpetas de mods...");
            Russian.Add("scanningModsFolders", "Сканируются папки модов...");

            //Component: file
            //
            English.Add("file", "File");
            German.Add("file", "Datei");
            Polish.Add("file", "Plik");
            French.Add("file", "Fichier");
            Spanish.Add("file", "Archivo");
            Russian.Add("file", "Файл");

            //Component: size
            //
            English.Add("size", "Size");
            German.Add("size", "Größe");
            Polish.Add("size", "Rozmiar");
            French.Add("size", "Taille");
            Spanish.Add("size", "Tamaño");
            Russian.Add("size", "Размер");

            //Component: CheckDatabase
            //
            English.Add("checkDatabase", "Checking the database for outdated or no longer needed files");
            German.Add("checkDatabase", "Durchsuche das Dateiarchive nach veralteten oder nicht mehr benötigten Dateien");
            Polish.Add("checkDatabase", "Skanowanie bazy danych w poszukiwaniu nieaktualnych i niepotrzebnych plików");
            French.Add("checkDatabase", "Vérification de la base de données pour les fichiers périmés ou non nécessaires");
            Spanish.Add("checkDatabase", "Comprobando la base de datos para archivos anticuados o no necesarios");
            Russian.Add("checkDatabase", "Идёт проверка БД на наличие неактуальных или ненужных файлов");

            //Component:
            //function checkForOldZipFiles()
            English.Add("parseDownloadFolderFailed", "Parsing the \"{0}\" folder failed.");
            German.Add("parseDownloadFolderFailed", "Durchsehen des \"{0}\" Verzeichnisses ist fehlgeschlagen.");
            Polish.Add("parseDownloadFolderFailed", "Przetwarzanie informacji o folderze \"{0}\" nie powiodło się.");
            French.Add("parseDownloadFolderFailed", "L'analyse du dossier \"{0}\" a échoué.");
            Spanish.Add("parseDownloadFolderFailed", "El análisis de la carpeta \"{0}\" ha fallado.");
            Russian.Add("parseDownloadFolderFailed", "Не удалось обработать папку \"{0}\"");

            //Component: installation finished
            //
            English.Add("installationFinished", "Installation is finished");
            German.Add("installationFinished", "Die Installation ist abgeschlossen");
            Polish.Add("installationFinished", "Instalacja zakończona");
            French.Add("installationFinished", "L'installation est terminée");
            Spanish.Add("installationFinished", "Instalación finalizada");
            Russian.Add("installationFinished", "Установка завершена");

            //Component: deletingFiles
            //
            English.Add("deletingFiles", "Deleting files");
            German.Add("deletingFiles", "Lösche Dateien");
            Polish.Add("deletingFiles", "Usuwanie plików");
            French.Add("deletingFiles", "Suppression de fichiers");
            Spanish.Add("deletingFiles", "Eliminando archivos");
            Russian.Add("deletingFiles", "Удаляются файлы");

            //Component:
            //
            English.Add("uninstalling", "Uninstalling");
            German.Add("uninstalling", "Deinstallieren");
            Polish.Add("uninstalling", "Dezinstalacja w toku");
            French.Add("uninstalling", "Désinstallation");
            Spanish.Add("uninstalling", "Desinstalando");
            Russian.Add("uninstalling", "Удаление...");

            //Component:
            //
            English.Add("zipReadingErrorHeader", "Incomplete Download");
            German.Add("zipReadingErrorHeader", "Unvollständiger Download");
            Polish.Add("zipReadingErrorHeader", "Pobieranie niekompletne");
            French.Add("zipReadingErrorHeader", "Téléchargement incomplet");
            Spanish.Add("zipReadingErrorHeader", "Descarga incompleta");
            Russian.Add("zipReadingErrorHeader", "Незаконченная загрузка");

            //Component:
            //
            English.Add("zipReadingErrorMessage1", "The zip file");
            German.Add("zipReadingErrorMessage1", "Die Zip-Datei");
            Polish.Add("zipReadingErrorMessage1", "Archiwum ZIP ");
            French.Add("zipReadingErrorMessage1", "Le fichier ZIP");
            Spanish.Add("zipReadingErrorMessage1", "El archivo zip");
            Russian.Add("zipReadingErrorMessage1", "ZIP-архив");

            //Component:
            //
            English.Add("zipReadingErrorMessage2", "could not be read, most likely due to an incomplete download. Please try again.");
            German.Add("zipReadingErrorMessage2", "Konnte nicht gelesen werden, da es höchstwahrscheinlich ein unvollständiger Download ist. Bitte versuche es später nochmal.");
            Polish.Add("zipReadingErrorMessage2", "nie mógł zostać przetworzony. Prawdopodobnie jest niekompletny. Spróbuj pobrać ponownie.");
            French.Add("zipReadingErrorMessage2", "n'as pas pus être lus, probablement un téléchargement incomplet. Veuillez réessayer.");
            Spanish.Add("zipReadingErrorMessage2", "no ha podido ser leído, probablemente por una descarga incompleta. Por favor vuelva a intentarlo.");
            Russian.Add("zipReadingErrorMessage2", "не может быть прочитан, возможно, причиной послужила незаконченная загрузка. Попробуйте ещё раз.");

            //Component:
            //
            English.Add("zipReadingErrorMessage3", "Could not be read.");
            German.Add("zipReadingErrorMessage3", "Konnte nicht gelesen werden.");
            Polish.Add("zipReadingErrorMessage3", "Nie można odczytać.");
            French.Add("zipReadingErrorMessage3", "n'as pas pus être lus");
            Spanish.Add("zipReadingErrorMessage3", "No ha podido ser leído.");
            Russian.Add("zipReadingErrorMessage3", "Сбой чтения");

            //Component:
            //
            English.Add("patchingSystemDeneidAccessMessage", "The patching system was denied access to the patch folder. Retry as Administrator. If you see this again, you need to fix your" +
                " file and folder security permissions");
            German.Add("patchingSystemDeneidAccessMessage", "Dem Patching-System wurde der Zugriff auf den Patch-Ordner verweigert. Wiederholen als Administrator. Wenn du diese Meldung wieder siehst," +
                " mußt du deine Datei- und Ordnersicherheitsberechtigungen reparieren.");
            Polish.Add("patchingSystemDeneidAccessMessage", "Aktualizator nie mógł uzyskać dostępu do folderu patcha. Spróbuj ponownie jako administrator. Jeśli problem się powtarza, zmień uprawnienia" +
                " dostępu do plików i folderów.");
            French.Add("patchingSystemDeneidAccessMessage", "Le système de patching s'est vu refuser l'accès au dossier de patch. Réessayez en tant qu'administrateur. Si vous voyez ceci" +
                " à nouveau, assurez vous que vos permissions de sécurités de dossiers et de fichiers sont suffisantes.");
            Spanish.Add("patchingSystemDeneidAccessMessage", "Se ha denegado el acceso del sistema de parcheo a la carpeta del parche. Vuelva a intentarlo como Administrador. Si vuelve a ver este mensaje," +
                " tiene que corregir los permisos de seguridad de sus archivos y carpetas.");
            Russian.Add("patchingSystemDeneidAccessMessage", "Применение патча невозможно: нет доступа к папке с патчами. Попробуйте повторить операцию от имени администратора. Если вы снова видите это окно, то исправьте ошибки в правах доступа к файлам и папкам.");

            //Component:
            //
            English.Add("patchingSystemDeneidAccessHeader", "Access Denied");
            German.Add("patchingSystemDeneidAccessHeader", "Zugriff abgelehnt");
            Polish.Add("patchingSystemDeneidAccessHeader", "Odmowa dostępu");
            French.Add("patchingSystemDeneidAccessHeader", "Accès refusé");
            Spanish.Add("patchingSystemDeniedAccessHeader", "Acceso denegado");
            Russian.Add("patchingSystemDeneidAccessHeader", "В доступе отказано");

            //Component: Failed To Delete folder
            //
            English.Add("folderDeleteFailed", "Failed to delete the folder");
            German.Add("folderDeleteFailed", "Löschen des Verzeichnis fehlgeschlagen");
            Polish.Add("folderDeleteFailed", "Próba usunięcia folderu zakończona niepowodzeniem");
            French.Add("folderDeleteFailed", "Échec de la suppression du dossier");
            Spanish.Add("folderDeleteFailed", "No se ha podido eliminar la carpeta");
            Russian.Add("folderDeleteFailed", "Не удалось удалить папку");

            //Component: Failed To Delete file
            //
            English.Add("fileDeleteFailed", "Failed to delete the file");
            German.Add("fileDeleteFailed", "Löschen der Datei fehlgeschlagen");
            Polish.Add("fileDeleteFailed", "Próba usunięcia pliku zakończona niepowodzeniem");
            French.Add("fileDeleteFailed", "Échec de la supression du fichier");
            Spanish.Add("fileDeleteFailed", "No se ha podido eliminar el archivo");
            Russian.Add("fileDeleteFailed", "Не удалось удалить файл");

            //Component: DeleteBackupFolder
            //
            English.Add("DeleteBackupFolder", "Backups");
            German.Add("DeleteBackupFolder", "Sicherungen");
            Polish.Add("DeleteBackupFolder", "Kopie zapasowe");
            French.Add("DeleteBackupFolder", "Sauvegardes");
            Spanish.Add("DeleteBackupFolder", "Copias de seguridad");
            Russian.Add("DeleteBackupFolder", "Бэкапы");

            //Component: installFailed
            //Error message to show at the end of an unsucessfull instalaltion
            //"The installation failed at the following steps: {newline} {failed_steps_list}
            English.Add("installFailed", "The installation failed at the following steps");
            German.Add("installFailed", "Die Installation misslang bei folgendem Schritt:");
            Polish.Add("installFailed", "Instalacja niepowiodła się przy następujących krokach");
            French.Add("installFailed", "L'installation a échouée a l'étape suivante");
            Spanish.Add("installFailed", "La instalación ha fallado en los siguientes puntos");
            Russian.Add("installFailed", "Установка завершилась с ошибкой на следующем(их) этапе(ах)");
            #endregion

            #region Install finished window
            //Component: InstallFinished
            //
            English.Add("InstallFinished", "Install Finished");
            German.Add("InstallFinished", "Installation abgeschlossen");
            Polish.Add("InstallFinished", "Instalacja Zakończona");
            French.Add("InstallFinished", "Installation terminée");
            Spanish.Add("InstallFinished", "Instalación completada");
            Russian.Add("InstallFinished", "Установка завершена");

            //Component: InstallationCompleteText
            //
            English.Add("InstallationCompleteText", "The Installation is complete. Would you like to...");
            German.Add("InstallationCompleteText", "Installation ist beendet. ");
            Polish.Add("InstallationCompleteText", "Instalacja zakończona. Czy chcesz teraz...");
            French.Add("InstallationCompleteText", "L'installation est terminée. Voudriez-vous...");
            Spanish.Add("InstallationCompleteText", "La instalación ha sido completada. Quiere...");
            Russian.Add("InstallationCompleteText", "Установка завершена. Хотите...");

            //Component: InstallationCompleteStartWoT
            //
            English.Add("InstallationCompleteStartWoT", "Start the game? (WorldofTanks.exe)");
            German.Add("InstallationCompleteStartWoT", "Spiel starten (WorldofTanks.exe)");
            Polish.Add("InstallationCompleteStartWoT", "Uruchomić grę? (WorldofTanks.exe)");
            French.Add("InstallationCompleteStartWoT", "Démarrez le jeu? (WorldofTanks.exe)");
            Spanish.Add("InstallationCompleteStartWoT", "¿Iniciar el juego?");
            Russian.Add("InstallationCompleteStartWoT", "Запустить игру? (WorldofTanks.exe)");

            //Component: InstallationCompleteStartGameCenter
            //
            English.Add("InstallationCompleteStartGameCenter", "Start WG Game Center?");
            German.Add("InstallationCompleteStartGameCenter", "WG Game Center");
            Polish.Add("InstallationCompleteStartGameCenter", "Uruchomić WG Game Center?");
            French.Add("InstallationCompleteStartGameCenter", "Démarrer WG Game Center?");
            Spanish.Add("InstallationCompleteStartGameCenter", "¿Iniciar WG Game Center?");
            Russian.Add("InstallationCompleteStartGameCenter", "Запустить Wargaming.net Game Center?"); // *sigh* - DrWeb7_1

            //Component: InstallationCompleteOpenXVM
            //
            English.Add("InstallationCompleteOpenXVM", "Open the xvm login website?");
            German.Add("InstallationCompleteOpenXVM", "Öffne die XVM Login Webseite");
            Polish.Add("InstallationCompleteOpenXVM", "Przejść do strony logowania moda XVM?");
            French.Add("InstallationCompleteOpenXVM", "Ouvrir votre navigateur Web vers le site de connexion aux statistiques xvm ?");
            Spanish.Add("InstallationCompleteOpenXVM", "¿Abrir su explorador en la página de inicio de sesión de las estadísticas de XVM?");
            Russian.Add("InstallationCompleteOpenXVM", "Открыть браузер на сайте XVM для активации статистики?");

            //Component: InstallationCompleteCloseThisWindow
            //
            English.Add("InstallationCompleteCloseThisWindow", "Close this window?");
            German.Add("InstallationCompleteCloseThisWindow", "Schließe dieses Fenster");
            Polish.Add("InstallationCompleteCloseThisWindow", "Zamknąć to okno?");
            French.Add("InstallationCompleteCloseThisWindow", "Fermer cette fenêtre?");
            Spanish.Add("InstallationCompleteCloseThisWindow", "¿Cerrar esta ventana?");
            Russian.Add("InstallationCompleteCloseThisWindow", "Закрыть окно?");

            //Component: InstallationCompleteCloseApp
            //
            English.Add("InstallationCompleteCloseApp", "Close the application?");
            German.Add("InstallationCompleteCloseApp", "Anwendung schließen");
            Polish.Add("InstallationCompleteCloseApp", "Zamknąć aplikację?");
            French.Add("InstallationCompleteCloseApp", "Fermer l'application");
            Spanish.Add("InstallationCompleteCloseApp", "¿Cerrar la aplicación?");
            Russian.Add("InstallationCompleteCloseApp", "Закрыть приложение?");

            //Component: xvmUrlLocalisation
            //localisation to which page you will jump
            English.Add("xvmUrlLocalisation", "en");
            German.Add("xvmUrlLocalisation", "de");
            Polish.Add("xvmUrlLocalisation", "en");
            French.Add("xvmUrlLocalisation", "fr");
            Spanish.Add("xvmUrlLocalisation", "en"); //? No Spanish on XVM website. Mistake? @Nullmaruzero
            //this seems like a bug, but it's what they have it set as
            Russian.Add("xvmUrlLocalisation", "ru");

            //Component: CouldNotStartProcess
            //
            English.Add("CouldNotStartProcess", "Could not start process");
            German.Add("CouldNotStartProcess","Konnte Prozess nicht starten");
            Polish.Add("CouldNotStartProcess", "Nie udało się uruchomić procesu");
            French.Add("CouldNotStartProcess", "Impossible de démarrer le processus");
            Spanish.Add("CouldNotStartProcess", "No se ha podido iniciar el proceso");
            Russian.Add("CouldNotStartProcess", "Не удалось запустить процесс");
            #endregion

            #region Diagnostics
            //Component: Diagnostics
            //
            English.Add("Diagnostics", "Diagnostics");
            German.Add("Diagnostics", "Diagnose");
            Polish.Add("Diagnostics", "Diagnostyka");
            French.Add("Diagnostics", "Diagnostique");
            Spanish.Add("Diagnostics", "Diagnósticos");
            Russian.Add("Diagnostics", "Диагностика");

            //Component: MainTextBox
            //
            English.Add("DiagnosticsMainTextBox", "You can use the options below to try to diagnose or solve the issues you are having.");
            German.Add("DiagnosticsMainTextBox", "Du kannst mit den folgenden Optionen Probleme mit dem Spiel diagnostizieren und ggf. beheben.");
            Polish.Add("DiagnosticsMainTextBox", "Poniższe opcje pomogą zdiagnozować i rozwiązać napotkane problemy.");
            French.Add("DiagnosticsMainTextBox", "Vous pouvez utiliser les options ci dessous pour essayer de diagnostiquer ou résoudre les soucis que vous avez");
            Spanish.Add("DiagnosticsMainTextBox", "Puede utilizar las opciones a continuación para intentar diagnosticar o resolver problemas");
            Russian.Add("DiagnosticsMainTextBox", "Вы можете попробовать способы ниже для диагностики или выявления проблем с клиентом игры.");

            //Component: LaunchWoTLauncher
            //
            English.Add("LaunchWoTLauncher", "Start the World of Tanks launcher in integrity validation mode");
            German.Add("LaunchWoTLauncher", "Starte den \"World of Tanks Launcher\" im Integritätsvalidierungsmodus");
            Polish.Add("LaunchWoTLauncher", "Uruchom WoT w trybie sprawdzania integralności.");
            French.Add("LaunchWoTLauncher", "Lancé le launcher world of tanks en mode vérification d'intégrité");
            Spanish.Add("LaunchWoTLauncher", "Inicia el lanzador de World of Tanks en el modo de validación de integridad");
            Russian.Add("LaunchWoTLauncher", "Запустить лаунчер World of Tanks в режиме проверки целостности");

            //Component: CollectLogInfo
            //
            English.Add("CollectLogInfo", "Collect log files into a zip file to report a problem");
            German.Add("CollectLogInfo", "Sammle und packe Protokolldateien in einer ZIP-Datei um ein Problem zu melden");
            Polish.Add("CollectLogInfo", "Zbierz pliki dziennika, aby zgłosić problem");
            French.Add("CollectLogInfo", "Recueillir les fichiers journaux dans un fichier zip pour signaler un problème");
            Spanish.Add("CollectLogInfo", "Recoger los archivos de registro en un archivo zip para informar de un problema");
            Russian.Add("CollectLogInfo", "Собрать log-файлы в ZIP-архив для отчёта об ошибке");

            //Component: CollectLogInfoButtonDescription
            //
            English.Add("CollectLogInfoButtonDescription", "Collects all the necessary log files into a one ZIP files.\nThis makes it easier for you to report a problem.");
            German.Add("CollectLogInfoButtonDescription", "Sammelt alle notwendigen Log-Dateien in ein .zip-Datei. \nDas macht es dir einfacher ein Problem zu melden.");
            Polish.Add("CollectLogInfoButtonDescription", "Tworzy archiwum ZIP ze wszystkimi plikami dziennika, ułatwiając zgłaszanie błędów.");
            French.Add("CollectLogInfoButtonDescription", "Collecte tous les fichiers journaux nécessaires dans un seul fichier ZIP. \nCela vous permet de signaler un problème plus facilement.");
            Spanish.Add("CollectLogInfoButtonDescription", "Recopila todos los archivos de registro necesarios en un archivo ZIP.\nEsto le facilita informar de un problema");
            Russian.Add("CollectLogInfoButtonDescription", "Собрать все необходимые лог-файлы в одном ZIP-архиве.\nЭто упростит процесс создания отчёта об ошибке.");

            //Component: DownloadWGPatchFilesText
            // A button for a new window with multiple tabs, guiding the user through the process of manual WG patch files download over HTTP for later installation by the WGC.
            English.Add("DownloadWGPatchFilesText", "Download WG Patch files for any WG client via HTTP");
            German.Add("DownloadWGPatchFilesText", "Lade WG Patchdateien für jeden WG Client über HTTP runter");
            Polish.Add("DownloadWGPatchFilesText", "Pobieranie aktualizacji dla gier WG (HTTP)");
            French.Add("DownloadWGPatchFilesText", "Télécharger les fichiers de patch WG pour n'importe quel client WG via HTTP");
            Spanish.Add("DownloadWGPatchFilesText", "Descargar los archivos de parche de WG para cualquier cliente mediante HTTP");
            Russian.Add("DownloadWGPatchFilesText", "Скачать обновления для любой игры из WGC через HTTP");

            //Component: DownloadWGPatchFilesButtonDescription
            // A tooltip for the DownloadWGPatchFiles button in diagnostics window. Used to manualy download WG patch files over HTTP for later installation by the WGC.
            English.Add("DownloadWGPatchFilesButtonDescription", "Guides you through & downloads patch files for Wargaming games (WoT, WoWs, WoWp) over HTTP so you can install them later.\n" +
                "Particularily useful for people who cannot use Wargaming Game Center's default P2P protocol.");
            German.Add("DownloadWGPatchFilesButtonDescription", "Führt dich durch die Auswahl und lädt Patch-Dateien für Wargaming-Spiele (WoT, WoWs, WoWp) über HTTP herunter, damit du sie später installieren kannst. \n" +
                 "Besonders nützlich für Leute, die das standardmäßige P2P-Protokoll von Wargaming Game Center nicht verwenden können.");
            Polish.Add("DownloadWGPatchFilesButtonDescription", "Pobiera pliki aktualizacji dla gier Wargaming za pomocą protokołu HTTP celem późniejszej instalacji w WGC.\n" +
                "Szczególnie przydatne dla osób, które nie mogą używać wbudowanego w Wargaming Game Center protokołu P2P.");
            French.Add("DownloadWGPatchFilesButtonDescription", "Vous guide et télécharge les fichiers de correctifs pour les jeux Wargaming (WoT, WoWs, WoWp) sur HTTP afin que vous puissiez les installer plus tard. \n" +
                "Particulièrement utile pour les personnes qui ne peuvent pas utiliser le protocole P2P par défaut de Wargaming Game Center.");
            Spanish.Add("DownloadWGPatchFilesButtonDescription", "Le guiará e instalará los archivos de parche para los juegos de Wargaming (WoT, WoWs, WoWp) mediante HTTP para que los pueda instalar más tarde.\n" +
                "Particularmente útil para quien no puede utilizar el protocolo estándar P2P de Wargaming Game Center (WGC)");
            Russian.Add("DownloadWGPatchFilesButtonDescription", "Данный мастер позволит загрузить обновления для любой игры от Wargaming (WoT, WoWS, WoWP) через HTTP, чтобы вы смогли установить их позднее.\nПолезно при возникновении проблем с режимом P2P (торрентом), используемым Wargaming Game Center по умолчанию.");

            //Component: SelectedInstallation
            //
            English.Add("SelectedInstallation", "Currently Selected Installation:");
            German.Add("SelectedInstallation", "Aktuell gewählte Installation:");
            Polish.Add("SelectedInstallation", "Aktywna instalacja WoT:");
            French.Add("SelectedInstallation", "Installation actuellement sélectionner:");
            Spanish.Add("SelectedInstallation", "Instalación seleccionada actualmente:");
            Russian.Add("SelectedInstallation", "Текущая папка с игрой:");

            //Component: SelectedInstallationNone
            // A text label with the (not selected in this case) path to the selected (active) installation.
            English.Add("SelectedInstallationNone", "("+English["none"].ToLower()+")");
            German.Add("SelectedInstallationNone", "("+German["none"].ToLower()+")");
            Polish.Add("SelectedInstallationNone", "("+Polish["none"].ToLower()+")"); // This Frankenstein's monster is of my creation. @Nullmaruzero
            French.Add("SelectedInstallationNone", "("+French["none"].ToLower()+")");
            Spanish.Add("SelectedInstallationNone", "("+Spanish["none"].ToLower()+")");
            Russian.Add("SelectedInstallationNone", "("+Russian["none"].ToLower()+")");

            //Component: collectionLogInfo
            //
            English.Add("collectionLogInfo", "Collecting log files...");
            German.Add("collectionLogInfo", "Sammeln der Protokolldateien...");
            Polish.Add("collectionLogInfo", "Zbieranie plików dziennika...");
            French.Add("collectionLogInfo", "Collection des fichiers log...");
            Spanish.Add("collectionLogInfo", "Reuniendo los archivos de registro...");
            Russian.Add("collectionLogInfo", "Идёт сбор log-файлов...");

            //Component: startingLauncherRepairMode
            //
            English.Add("startingLauncherRepairMode", "Starting WoTLauncher in integrity validation mode...");
            German.Add("startingLauncherRepairMode", "Starte den WoT Launcher im Integritätsvalidierungsmodus...");
            Polish.Add("startingLauncherRepairMode", "Uruchamianie launchera WOT w trybie sprawdzania integralności plików...");
            French.Add("startingLauncherRepairMode", "Lancement de WoTLauncher and mode the validation d'intégrité...");
            Spanish.Add("startingLauncherRepairMode", "Inicia WoTLauncher en el modo de validación de integridad...");
            Russian.Add("startingLauncherRepairMode", "Запускаю WoTLauncher в режиме проверки целостности..."); // May I joke about my OMLauncher? Why don't remove these lines? - DrWeb7_1

            //Component: failedStartLauncherRepairMode
            //
            English.Add("failedStartLauncherRepairMode", "Failed to start WoT Launcher in Repair mode");
            German.Add("failedStartLauncherRepairMode", "Der WoT Launcher konnte nicht im Reparaturmodus gestartet werden");
            Polish.Add("failedStartLauncherRepairMode", "Nie udało się uruchomić launchera WoT w trybie naprawy");
            French.Add("failedStartLauncherRepairMode", "Erreur lors du lancement de WoTLauncher en mode de réparation");
            Spanish.Add("failedStartLauncherRepairMode", "No se ha podido iniciar el lanzador de WoT en modo reparación");
            Russian.Add("failedStartLauncherRepairMode", "Не удалось запустить WoTLauncher в режиме проверки целостности"); // Same as "startingLauncherRepairMode". - DrWeb7_1

            //Component: failedCollectFile
            //
            English.Add("failedCollectFile", "Failed to collect the file ");
            German.Add("failedCollectFile", "Fehler beim Sammeln der Datei ");
            Polish.Add("failedCollectFile", "Nie udało się zebrać plików ");
            French.Add("failedCollectFile", "Erreur lors de la collecte du fichier");
            Spanish.Add("failedCollectFile", "No se ha podido obtener el archivo ");
            Russian.Add("failedCollectFile", "Не найден файл ");

            //Component: failedCreateZipfile
            //
            English.Add("failedCreateZipfile", "Failed to create the zip file ");
            German.Add("failedCreateZipfile", "Fehler beim Erstellen der Zip-Datei ");
            Polish.Add("failedCreateZipfile", "Nie udało się stworzyć archiwum ZIP ");
            French.Add("failedCreateZipfile", "Erreur lors de la création du fichier zip ");
            Spanish.Add("failedCreateZipfile", "No se ha podido crear el archivo zip ");
            Russian.Add("failedCreateZipfile", "Не удалось создать ZIP-архив");

            //Component: launcherRepairModeStarted
            //
            English.Add("launcherRepairModeStarted", "Repair mode successfully started");
            German.Add("launcherRepairModeStarted", "Reparaturmodus wurde erfolgreich gestartet");
            Polish.Add("launcherRepairModeStarted", "Uruchomiono tryb naprawy");
            French.Add("launcherRepairModeStarted", "Mode de réparation démarrer avec succès");
            Spanish.Add("launcherRepairModeStarted", "Modo de reparación iniciado correctamente");
            Russian.Add("launcherRepairModeStarted", "Проверка целостности успешно начата"); // Since WoTLauncher is offically unsupported, is this even used? - DrWeb7_1

            //Component: ClearDownloadCache
            //
            English.Add("ClearDownloadCache", "Clear download cache");
            German.Add("ClearDownloadCache", "Lösche Downlaod Cache");
            Polish.Add("ClearDownloadCache", "Usuń pliki tymczasowe pobierania");
            French.Add("ClearDownloadCache", "Effacer le cache de téléchargement");
            Spanish.Add("ClearDownloadCache", "Limpiar la caché de descarga");
            Russian.Add("ClearDownloadCache", "Очистить кэш загрузок");

            //Component: ClearDownloadCacheDatabase
            //
            English.Add("ClearDownloadCacheDatabase", "Delete download cache database file");
            German.Add("ClearDownloadCacheDatabase", "Lösche Datenbank-Cache");
            Polish.Add("ClearDownloadCacheDatabase", "Usuń pliki tymczasowe bazy danych");
            French.Add("ClearDownloadCacheDatabase", "Supprimer le fichier de base de données de cache de téléchargement");
            Spanish.Add("ClearDownloadCacheDatabase", "Elimina el archivo de caché de descarga");
            Russian.Add("ClearDownloadCacheDatabase", "Удалить кэш базы данных");

            //Component: ClearDownloadCacheDescription
            //
            English.Add("ClearDownloadCacheDescription", "Delete all files in the \"RelhaxDownloads\" folder");
            German.Add("ClearDownloadCacheDescription", "Lösche alle Daten aus dem \"RelhaxDownloads\" Ordner");
            Polish.Add("ClearDownloadCacheDescription", "Usuń wszystkie pliki w folderze \"RelhaxDownloads\"");
            French.Add("ClearDownloadCacheDescription", "Supprimez tous les fichiers dans le répertoire \"RelhaxDownloads\"");
            Spanish.Add("ClearDownloadCacheDescription", "Elimina todos los archivos en la carpeta \"RelhaxDownloads\"");
            Russian.Add("ClearDownloadCacheDescription", "Удаление всех файлов в папке \"RelhaxDownloads\"");

            //Component: ClearDownloadCacheDatabaseDescription
            //
            English.Add("ClearDownloadCacheDatabaseDescription", "Delete the xml database file. This will cause all zip files to be re-checked for integrity.\nAll invalid files will be re-downloaded if selected in your next installation.");
            German.Add("ClearDownloadCacheDatabaseDescription", "Lösche die XML-Datenbankdatei. Dadurch werden alle ZIP-Dateien erneut auf Integrität überprüft. \nAlle ungültigen Dateien werden erneut heruntergeladen, falls diese bei deiner nächsten Installation ausgewählt werden.");
            Polish.Add("ClearDownloadCacheDatabaseDescription", "Usuń plik XML bazy danych. Spowoduje to ponowne sprawdzenie integralności wszystkich archiwów ZIP.\nWszystkie uszkodzone pliki zostaną ponownie pobrane, jeśli zostaną wybrane w następnej instalacji");
            French.Add("ClearDownloadCacheDatabaseDescription", "Supprimez le fichier de base de données XML. L’intégrité de tous les fichiers zip sera à nouveau vérifiée. \nTous les fichiers non valides seront re-téléchargés s’ils sont sélectionnés lors de votre prochaine installation.");
            Spanish.Add("ClearDownloadCacheDatabaseDescription", "Elimina el archivo de base de datos XML. Esto causará que la integridad de todos los archivos zip vuelva a ser comprobada.\nTodos los archivos inválidos volverán a ser descargados si son seleccionados en su próxima instalación.");
            Russian.Add("ClearDownloadCacheDatabaseDescription", "Удаление XML-файла базы данных. Это приведёт к тому, что все скачанныые ZIP-архивы будут проверены на целостность.\nВсе повреждённые файлы будут загружены заново в случае, если вы ещё раз выберете моды, соотвествующие этим архивам.");

            //Component: clearingDownloadCache
            //
            English.Add("clearingDownloadCache", "Clearing download cache");
            German.Add("clearingDownloadCache", "Bereinige Download Cache");
            Polish.Add("clearingDownloadCache", "Czyszczenie pamięci podręcznej pobierania");
            French.Add("clearingDownloadCache", "Suppression du cache de téléchargement");
            Spanish.Add("clearingDownloadCache", "Limpiando caché de descarga");
            Russian.Add("clearingDownloadCache", "Очищается кэш загрузок");

            //Component: failedToClearDownloadCache
            //
            English.Add("failedToClearDownloadCache", "Failed to clear download cache");
            German.Add("failedToClearDownloadCache", "Fehler beim Bereinigen des Download Cache");
            Polish.Add("failedToClearDownloadCache", "Czyszczenie pamięci podręcznej pobierania zakończone niepowodzeniem");
            French.Add("failedToClearDownloadCache", "Echec du nettoyage du cache de téléchargement");
            Spanish.Add("failedToClearDownloadCache", "No se ha podido limpiar la caché de descarga");
            Russian.Add("failedToClearDownloadCache", "Не удалось очистить кэш загрузок");

            //Component: cleaningDownloadCacheComplete
            //
            English.Add("cleaningDownloadCacheComplete", "Download cache cleared");
            German.Add("cleaningDownloadCacheComplete", "Download Cache gelöscht");
            Polish.Add("cleaningDownloadCacheComplete", "Wyczyszczono pamieć podręczną");
            French.Add("cleaningDownloadCacheComplete", "Nettoyage du cache de téléchargement terminé");
            Spanish.Add("cleaningDownloadCacheComplete", "Caché de descarga limpiada");
            Russian.Add("cleaningDownloadCacheComplete", "Кэш загрузок успешно очищен");

            //Component: clearingDownloadCacheDatabase
            //
            English.Add("clearingDownloadCacheDatabase", "Deleting xml database cache file");
            German.Add("clearingDownloadCacheDatabase", "Lösche XML Datenbank Cache Datei");
            Polish.Add("clearingDownloadCacheDatabase", "Usuwanie pamięci podręcznej bazy danych");
            French.Add("clearingDownloadCacheDatabase", "Suppression du cache de téléchargement de la base de données");
            Spanish.Add("clearingDownloadCacheDatabase", "Eliminando archivo de caché de la base de datos XML");
            Russian.Add("clearingDownloadCacheDatabase", "Удаляется кэшированный XML-файл БД");

            //Component: failedToClearDownloadCacheDatabase
            //
            English.Add("failedToClearDownloadCacheDatabase", "Failed to delete xml database cache file");
            German.Add("failedToClearDownloadCacheDatabase", "Fehler beim löschen der XML Datenbank Cache Datei");
            Polish.Add("failedToClearDownloadCacheDatabase", "Czyszczenie pamięci podręcznej bazy danych zakończone niepowodzeniem");
            French.Add("failedToClearDownloadCacheDatabase", "Echec du nettoyage du cache de téléchargement de la base de données terminé");
            Spanish.Add("failedToClearDownloadCacheDatabase", "No se ha podido eliminar el archivo de caché de la base de datos XML");
            Russian.Add("failedToClearDownloadCacheDatabase", "Не удалось удалить кэшированный XML-файл базы данных");

            //Component: cleaningDownloadCacheDatabaseComplete
            //
            English.Add("cleaningDownloadCacheDatabaseComplete", "Xml database cache file deleted");
            German.Add("cleaningDownloadCacheDatabaseComplete", "XML Datenbank Cache Datei gelöscht");
            Polish.Add("cleaningDownloadCacheDatabaseComplete", "Wyczyszczono pamięć podręczną bazy danych");
            French.Add("cleaningDownloadCacheDatabaseComplete", "Fichier de cache de la base de données XML supprimée");
            Spanish.Add("cleaningDownloadCacheDatabaseComplete", "Archivo de caché de la base de datos XML eliminado");
            Russian.Add("cleaningDownloadCacheDatabaseComplete", "Кэшированный XML-файл базы данных успешно удалён");

            //Component: ChangeInstall
            //
            English.Add("ChangeInstall", "Change the currently selected WoT installation");
            German.Add("ChangeInstall", "Ändere die aktuell ausgewählte WoT-Installation");
            Polish.Add("ChangeInstall", "Zmień wybraną powyżej instalację WoT");
            French.Add("ChangeInstall", "Modifier l'installation WOT sélectionné");
            Spanish.Add("ChangeInstall", "Cambia la instalación de WoT seleccionada actualmente");
            Russian.Add("ChangeInstall", "Изменить выбранную папку с World of Tanks");

            //Component: ChangeInstallDescription
            //
            English.Add("ChangeInstallDescription", "This will change which log files will get added to the diagnostics zip file");
            German.Add("ChangeInstallDescription", "Dadurch wird geändert, welche Protokolldateien zur ZIP-Diagnosedatei hinzugefügt werden");
            Polish.Add("ChangeInstallDescription", "Zmieni to pliki dziennika dodawane do diagnostycznego archiwum ZIP");
            French.Add("ChangeInstallDescription", "Cela modifiera quel fichiers journaux qui seront ajoutés au fichier zip de diagnostic.");
            Spanish.Add("ChangeInstallDescription", "Esto cambiará qué archivos de registro serán añadidos en el archivo zip de diagnóstico");
            Russian.Add("ChangeInstallDescription", "Это изменит набор файлов отчёта об ошибках, добавляемых в ZIP-архив.");

            //Component: zipSavedTo
            //
            English.Add("zipSavedTo", "Zip file saved to: ");
            German.Add("zipSavedTo", "Zip-Datei gespeichert in: ");
            Polish.Add("zipSavedTo", "Archiwum ZIP zapisano w: ");
            French.Add("zipSavedTo", "Fichier zip sauvegarder à: ");
            Spanish.Add("zipSavedTo", "Archivo zip guardado en: ");
            Russian.Add("zipSavedTo", "ZIP-архив успешно сохранён в: ");

            //Component: selectFilesToInclude
            //
            English.Add("selectFilesToInclude", "Select files to include in the bug report");
            German.Add("selectFilesToInclude", "Wähle Dateien um diese dem Fehlerbericht hinzuzufügen");
            Polish.Add("selectFilesToInclude", "Wybierz pliki, które będą dołączone do zgłoszenia błędu");
            French.Add("selectFilesToInclude", "Sélectionner les fichiers à inclure dans le signalement du bug");
            Spanish.Add("selectFilesToInclude", "Seleccione los archivos a incluir en el informe de errores");
            Russian.Add("selectFilesToInclude", "Выберите файлы для включения в отчёт об ошибке");

            //Component: TestLoadImageLibraries
            // A button in Diagnostics window for test-loading Atlas processing libraries.
            English.Add("TestLoadImageLibraries", "Test loading the atlas image processing libraries");
            German.Add("TestLoadImageLibraries", "Teste das Laden der Bibliotheken zur Atlasverarbeitung");
            Polish.Add("TestLoadImageLibraries", "Test bibliotek przetwarzania plików Atlas");
            French.Add("TestLoadImageLibraries", "Test de chargement des bibliothèques de traitement d'images atlas");
            Spanish.Add("TestLoadImageLibraries", "Carga de prueba de las librerías de procesamiento de imágenes tipo atlas");
            Russian.Add("TestLoadImageLibraries", "Протестировать библиотеки обработки изображений-атласов");

            //Component: TestLoadImageLibrariesButtonDescription
            // A tooltip for TestLoadImageLibraries button. Does hmmmm... @Willster? :>
            English.Add("TestLoadImageLibrariesButtonDescription", "Tests the atlas image processing libraries"); // I will need your help here, @Willster. @Nullmaruzero
            German.Add("TestLoadImageLibrariesButtonDescription", "Testet die Atlas-Bildverarbeitungsbibliotheken");
            Polish.Add("TestLoadImageLibrariesButtonDescription", "Próbuje wczytać biblioteki przetwarzania obrazów Atlas.\n" +
                "Ewentualne błędy mogą być oznakiem braku wymaganego oprogramowania w systemie."); //=> Potential (load) errors may indicate missing dependencies in your system.
            French.Add("TestLoadImageLibrariesButtonDescription", "Tests des bibliothèques de traitement d'images atlas");
            Spanish.Add("TestLoadImageLibrariesButtonDescription", "Comprueba las librerías de procesamiento de las imágenes tipo atlas");
            Russian.Add("TestLoadImageLibrariesButtonDescription", "Тест библиотек обработчика текстурных атласов");

            //Component: loadingAtlasImageLibraries
            //
            English.Add("loadingAtlasImageLibraries", "Loading atlas image processing libraries");
            German.Add("loadingAtlasImageLibraries", "Lade Bibliotheken zur Atlasverarbeitung");
            Polish.Add("loadingAtlasImageLibraries", "Wczytywanie bibliotek przetwarzania obrazów Atlas");
            French.Add("loadingAtlasImageLibraries", "Chargement des bibliothèques de traitement d'images atlas");
            Spanish.Add("loadingAtlasImageLibraries", "Cargando librerías de procesamiento de imágenes de atlas");
            Russian.Add("loadingAtlasImageLibraries", "Загружаются библиотеки обработчика текстурных атласов");

            //Component: loadingAtlasImageLibrariesSuccess
            //
            English.Add("loadingAtlasImageLibrariesSuccess", "Successfully loaded atlas image processing libraries");
            German.Add("loadingAtlasImageLibrariesSuccess", "Erolgreiches Laden der Bibliotheken zur Atlasverarbeitung");
            Polish.Add("loadingAtlasImageLibrariesSuccess", "Wczytywanie bibliotek przetwarzania obrazów Atlas zakończone sukcesem");
            French.Add("loadingAtlasImageLibrariesSuccess", "Chargement des bibliothèques de traitement d'images atlas réussi");
            Spanish.Add("loadingAtlasImageLibrariesSuccess", "Librerías de procesamiento de imágenes de atlas cargadas correctamente");
            Russian.Add("loadingAtlasImageLibrariesSuccess", "Библиотеки обработчика успешно загружены.");

            //Component: loadingAtlasImageLibrariesFail
            //
            English.Add("loadingAtlasImageLibrariesFail", "Failed to load atlas image processing libraries");
            German.Add("loadingAtlasImageLibrariesFail", "Fehler beim Laden der Bibliotheken zur Atlasverarbeitung");
            Polish.Add("loadingAtlasImageLibrariesFail", "Wczytywanie bibliotek przetwarzania obrazów Atlas zakończone niepowodzeniem");
            French.Add("loadingAtlasImageLibrariesFail", "Echec du chargement de la bibliothèque de traitement d'images atlas");
            Spanish.Add("loadingAtlasImageLibrariesFail", "No se han podido cargar las librerías de procesamiento de imágenes de atlas");
            Russian.Add("loadingAtlasImageLibrariesFail", "Не удалось загрузить библиотеки обработчика");

            //Component: CleanupModFilesText
            // One of the buttons in the diagnostics window.
            English.Add("CleanupModFilesText", "Cleanup mod files placed in incorrect locations");
            German.Add("CleanupModFilesText", "Bereinige falsch platzierte Moddaten");
            Polish.Add("CleanupModFilesText", "Usuń mody z niewłaściwych folderów");
            French.Add("CleanupModFilesText", "Nettoyer les fichiers de mods placer à des endroits incorrects");
            Spanish.Add("CleanupModFilesText", "Limpiar archivos de mods en rutas incorrectas");
            Russian.Add("CleanupModFilesText", "Удалить некорректно установленные моды");

            //Component: CleanupModFilesButtonDescription
            // The hint/tooltip for CleanupModFilesText
            English.Add("CleanupModFilesButtonDescription", "Deletes any mods located in folders like win32 and win64 that could be causing load conflicts");
            German.Add("CleanupModFilesButtonDescription", "Löscht alle Moddaten aus den win32 und win64 Ordnern um Konflikte zu vermeiden");
            Polish.Add("CleanupModFilesButtonDescription", "Czyści wszelkie mody z folderów takich jak win32 i win64 mogące powodować problemy z wczytywaniem.");
            French.Add("CleanupModFilesButtonDescription", "Supprimer tous les mods se situant dans des dossiers comme win32 et win64 qui pourrait causer des confits de chargement");
            Spanish.Add("CleanupModFilesButtonDescription", "Elimina cualquier mod en carpetas como win32 y win64 que puedan causar conflictos al cargar");
            Russian.Add("CleanupModFilesButtonDescription", "Будут удалены все моды, установленные в папках win32 и win64 во избежание конфликтов в процессе загрузки.");

            //Component: cleanupModFilesCompleted
            // Text displaying (upon mods being cleaned up) in the dedicated status area below all the buttons in the diagnostics window (it's large enough for longer messages).
            English.Add("cleanupModFilesCompleted", "Cleanup mod files completed");
            German.Add("cleanupModFilesCompleted", "Bereinigen der Moddaten abgeschlossen");
            Polish.Add("cleanupModFilesCompleted", "Oczyszczanie modów z niewłaściwych lokalizacji zakończone pomyślnie.");
            French.Add("cleanupModFilesCompleted", "Nettoyage des fichiers de mods terminé");
            Spanish.Add("cleanupModFilesCompleted", "Limpieza de mods completada");
            Russian.Add("cleanupModFilesCompleted", "Удаление модов завершено.");

            //Component: CleanGameCacheText
            // Text block for button to allow user to clear cache data from applicationData folders
            English.Add("CleanGameCacheText", "Clear game cache files");
            German.Add("CleanGameCacheText", "Lösche Cache des Spiels");
            Polish.Add("CleanGameCacheText", "Wyczyść pamięc podręczną gry");
            French.Add("CleanGameCacheText", TranslationNeeded);
            Spanish.Add("CleanGameCacheText", "Limpar caché del juego");
            Russian.Add("CleanGameCacheText", "Очистить кэш игры");

            //Component: cleanGameCacheProgress
            //
            English.Add("cleanGameCacheProgress", "Clearing game cache files");
            German.Add("cleanGameCacheProgress", "Lösche Spielcache");
            Polish.Add("cleanGameCacheProgress", "Czyszczenie pamięci podręcznej gry");
            French.Add("cleanGameCacheProgress", TranslationNeeded);
            Spanish.Add("cleanGameCacheProgress", "Limpiando caché del juego");
            Russian.Add("cleanGameCacheProgress", "Удаляются файлы кэша игры");

            //Component: cleanGameCacheSuccess
            //
            English.Add("cleanGameCacheSuccess", "Sucessfully cleared game cache files");
            German.Add("cleanGameCacheSuccess", "Spielcache erfolgreich gelöscht");
            Polish.Add("cleanGameCacheSuccess", "Czyszczenie pamięci podręcznej gry zakończone sukcesem");
            French.Add("cleanGameCacheSuccess", TranslationNeeded);
            Spanish.Add("cleanGameCacheSuccess", "Caché del juego limpiada con éxito");
            Russian.Add("cleanGameCacheSuccess", "Кэш игры успешно очищен");

            //Component: cleanGameCacheFail
            //
            English.Add("cleanGameCacheFail", "Failed to clear game cache files");
            German.Add("cleanGameCacheFail", "Konnte Cache-Dateien nicht löschen");
            Polish.Add("cleanGameCacheFail", "Nie udało się wyczyścić pamięci podręcznej gry");
            French.Add("cleanGameCacheFail", TranslationNeeded);
            Spanish.Add("cleanGameCacheFail", "No se ha podido limpar los archivos de caché del juego");
            Russian.Add("cleanGameCacheFail", "Не удалось удалить файлы кэша игры");

            //Component: TrimRelhaxLogfileText
            // Text block for allowing the user to trim relhax.log logfile to the last 3 launches (assuming header/footer entries exist)
            English.Add("TrimRelhaxLogfileText", "Trim the Relhax log file to the last 3 launches");
            German.Add("TrimRelhaxLogfileText", TranslationNeeded);
            Polish.Add("TrimRelhaxLogfileText", "Zachowaj w pliku dziennika tylko 3 ostatnie uruchomienia");
            French.Add("TrimRelhaxLogfileText", TranslationNeeded);
            Spanish.Add("TrimRelhaxLogfileText", TranslationNeeded);
            Russian.Add("TrimRelhaxLogfileText", TranslationNeeded);

            //Component: trimRelhaxLogProgress
            //
            English.Add("trimRelhaxLogProgress", "Trimming the Relhax log file");
            German.Add("trimRelhaxLogProgress", TranslationNeeded);
            Polish.Add("trimRelhaxLogProgress", "Ograniczanie zakresu pliku dziennika");
            French.Add("trimRelhaxLogProgress", TranslationNeeded);
            Spanish.Add("trimRelhaxLogProgress", TranslationNeeded);
            Russian.Add("trimRelhaxLogProgress", TranslationNeeded);

            //Component: trimRelhaxLogSuccess
            //
            English.Add("trimRelhaxLogSuccess", "Sucessfully trimmed the Relhax log file");
            German.Add("trimRelhaxLogSuccess", TranslationNeeded);
            Polish.Add("trimRelhaxLogSuccess", "Ograniczanie zakresu pliku dziennika zakończone powodzeniem");
            French.Add("trimRelhaxLogSuccess", TranslationNeeded);
            Spanish.Add("trimRelhaxLogSuccess", TranslationNeeded);
            Russian.Add("trimRelhaxLogSuccess", TranslationNeeded);

            //Component: trimRelhaxLogFail
            //
            English.Add("trimRelhaxLogFail", "Failed to trim the Relhax log file");
            German.Add("trimRelhaxLogFail", TranslationNeeded);
            Polish.Add("trimRelhaxLogFail", "Nie udało się ograniczyć zakresu pliku dziennika");
            French.Add("trimRelhaxLogFail", TranslationNeeded);
            Spanish.Add("trimRelhaxLogFail", TranslationNeeded);
            Russian.Add("trimRelhaxLogFail", TranslationNeeded);
            #endregion

            #region Add zip files Dialog
            //Component: AddPicturesZip
            //
            English.Add("AddPicturesZip", "Add files to zip");
            German.Add("AddPicturesZip", "Füge Dateien zum ZIP-Archiv hinzu");
            Polish.Add("AddPicturesZip", "Dodaj pliki do archiwum ZIP");
            French.Add("AddPicturesZip", "Ajouter des fichiers au zip");
            Spanish.Add("AddPicturesZip", "Añadir los archivos a ZIP");
            Russian.Add("AddPicturesZip", "Добавить файлы в ZIP-архив");

            //Component: DiagnosticsAddSelectionsPicturesLabel
            //
            English.Add("DiagnosticsAddSelectionsPicturesLabel", "Add any additional files here (your selection file, picture, etc.)");
            German.Add("DiagnosticsAddSelectionsPicturesLabel", "Füge zusätzliche Dateien hinzu (deine Auswahldatei, Bilder, etc.)");
            Polish.Add("DiagnosticsAddSelectionsPicturesLabel", "Dodaj wszelkie dodatkowe pliki (plik kolekcji, obrazy, itp.)");
            French.Add("DiagnosticsAddSelectionsPicturesLabel", "Ajouter n'importe quel fichier addiotionnel ici (Votre sélection de fichier, photos, etc.)");
            Spanish.Add("DiagnosticsAddSelectionsPicturesLabel", "Añada archivos adicionales aquí (archivos de selección, imágenes, etc.)");
            Russian.Add("DiagnosticsAddSelectionsPicturesLabel", "Добавить какие-либо дополнительные файлы (файл предустановки, изображения, и т. д.)");

            //Component: DiagnosticsAddFilesButton
            //
            English.Add("DiagnosticsAddFilesButton", "Add Files");
            German.Add("DiagnosticsAddFilesButton", "Dateien hinzufügen");
            Polish.Add("DiagnosticsAddFilesButton", "Dodaj pliki");
            French.Add("DiagnosticsAddFilesButton", "Ajouter des fichiers");
            Spanish.Add("DiagnosticsAddFilesButton", "Añadir archivos");
            Russian.Add("DiagnosticsAddFilesButton", "Добавить файлы");

            //Component: DiagnosticsRemoveSelectedButton
            //
            English.Add("DiagnosticsRemoveSelectedButton", "Remove Selected");
            German.Add("DiagnosticsRemoveSelectedButton", "Entferne Ausgewähltes");
            Polish.Add("DiagnosticsRemoveSelectedButton", "Usuń zaznaczone");
            French.Add("DiagnosticsRemoveSelectedButton", "Enlever la sélection");
            Spanish.Add("DiagnosticsRemoveSelectedButton", "Eliminar seleccionados");
            Russian.Add("DiagnosticsRemoveSelectedButton", "Удалить файл");

            //Component: DiagnosticsContinueButton
            //
            English.Add("DiagnosticsContinueButton", English["ContinueButton"]);
            German.Add("DiagnosticsContinueButton", German["ContinueButton"]);
            Polish.Add("DiagnosticsContinueButton", Polish["ContinueButton"]);
            French.Add("DiagnosticsContinueButton", French["ContinueButton"]);
            Spanish.Add("DiagnosticsContinueButton", Spanish["ContinueButton"]);
            Russian.Add("DiagnosticsContinueButton", Russian["ContinueButton"]);

            //Component: cantRemoveDefaultFile
            //
            English.Add("cantRemoveDefaultFile", "Cannot remove a file to be added by default.");
            German.Add("cantRemoveDefaultFile", "Kann keine Standard Dateien entfernen");
            Polish.Add("cantRemoveDefaultFile", "Nie można usuwać pliku dodawanego domyślnie");
            French.Add("cantRemoveDefaultFile", "Impossible de supprimer le fichier de défaut");
            Spanish.Add("cantRemoveDefaultFile", "No se puede eliminar un archivo que debe ser añadido por defecto.");
            Russian.Add("cantRemoveDefaultFile", "Невозможно удалить файл, добавляемый по умолчанию.");
            #endregion

            #region Preview Window
            //Component: Preview
            //
            English.Add("Preview", "Preview");
            German.Add("Preview", "Vorschau");
            Polish.Add("Preview", "Podgląd");
            French.Add("Preview", "Aperçu");
            Spanish.Add("Preview", "Previsualización");
            Russian.Add("Preview", "Предпросмотр");

            //Component: noDescription
            //
            English.Add("noDescription", "No description provided");
            German.Add("noDescription", "Keine Beschreibung verfügbar");
            Polish.Add("noDescription", "Brak opisu");
            French.Add("noDescription", "NPas de description fournie");
            Spanish.Add("noDescription", "No hay descripción disponible");
            Russian.Add("noDescription", "Описание отсутствует");

            //Component: noUpdateInfo
            //
            English.Add("noUpdateInfo", "No update info provided");
            German.Add("noUpdateInfo", "Keine Aktualisierungsinformationen verfügbar");
            Polish.Add("noUpdateInfo", "Brak informacji o aktualizacji");
            French.Add("noUpdateInfo", "Aucune information de mise à jour fournie");
            Spanish.Add("noUpdateInfo", "No hay información de actualización disponible");
            Russian.Add("noUpdateInfo", "Отсутствует информация об обновлении");

            //Component: noTimestamp
            //
            English.Add("noTimestamp", "No timestamp provided");
            German.Add("noTimestamp", "Kein Zeitstempel verfügbar");
            Polish.Add("noTimestamp", "Brak sygnatury czasowej");
            French.Add("noTimestamp", "Pas d'horodatage fourni");
            Spanish.Add("noTimestamp", "No hay marca de tiempo disponible");
            Russian.Add("noTimestamp", "Нет метки с датой");

            //Component: PreviewNextPicButton
            // A button in mod preview window used to display the previous preview image.
            English.Add("PreviewNextPicButton", English["next"]);
            German.Add("PreviewNextPicButton", German["next"]);
            Polish.Add("PreviewNextPicButton", "Następny"); // As DrWeb7_1 pointed out, Slavic languages are sometimes a b*tch... @Nullmaruzero
            French.Add("PreviewNextPicButton", French["next"]);
            Spanish.Add("PreviewNextPicButton", Spanish["next"]);
            Russian.Add("PreviewNextPicButton", "след.");

            //Component: PreviewPreviousPicButton
            // A button in mod preview window used to display the next preview image.
            English.Add("PreviewPreviousPicButton", English["previous"]);
            German.Add("PreviewPreviousPicButton", German["previous"]);
            Polish.Add("PreviewPreviousPicButton", "Poprzedni"); // As DrWeb7_1 pointed out, Slavic languages are sometimes a b*tch... @Nullmaruzero
            French.Add("PreviewPreviousPicButton", French["previous"]);
            Spanish.Add("PreviewPreviousPicButton", Spanish["previous"]);
            Russian.Add("PreviewPreviousPicButton", "пред.");

            //Component: DevUrlHeader
            //
            English.Add("DevUrlHeader", "Developer links");
            German.Add("DevUrlHeader", "Entwickler-Link");
            Polish.Add("DevUrlHeader", "Linki twórców");
            French.Add("DevUrlHeader", "Liens développeur");
            Spanish.Add("DevUrlHeader", "Links de los desarrolladores");
            Russian.Add("DevUrlHeader", "Сайт разработчика");

            //Component: dropDownItemsInside
            //
            English.Add("dropDownItemsInside", "Items Inside");
            German.Add("dropDownItemsInside", "Gegenstände im Inneren");
            Polish.Add("dropDownItemsInside", "Zawiera elementy");
            French.Add("dropDownItemsInside", "Articles à l'intérieur");
            Spanish.Add("dropDownItemsInside", "Artículos en el interior");
            Russian.Add("dropDownItemsInside", "Элементов внутри");

            //Component: popular
            //
            English.Add("popular", "popular");
            German.Add("popular", "beliebt");
            Polish.Add("popular", "popularne");
            French.Add("popular", "Populaire");
            Spanish.Add("popular", "popular");
            Russian.Add("popular", "популярный");

            //Component: previewEncounteredError
            //
            English.Add("previewEncounteredError", "The preview window encountered an error. Failed to display preview.");
            German.Add("previewEncounteredError", "Das Vorschaufenster stellte einen Fehler fest und kann die Vorschau nicht laden");
            Polish.Add("previewEncounteredError", "Okno podglądu napotkało błąd. Nie udało się wygenerować podglądu.");
            French.Add("previewEncounteredError", "Prévisualisation des erreurs rencontrées");
            Spanish.Add("previewEncounteredError", "La ventana de previsualización ha encontrado un error. No se ha podido mostrar previsualización");
            Russian.Add("previewEncounteredError", "Возникла проблема в работе окна предпросмотра. Не удалось отобразить превью.");

            //Component: popularInDescription
            //
            English.Add("popularInDescription", "This is a popular package");
            German.Add("popularInDescription", "Dies ist ein popüläres Paket");
            Polish.Add("popularInDescription", "Popularny pakiet");
            French.Add("popularInDescription", TranslationNeeded);
            Spanish.Add("popularInDescription", "Este paquete es popular");
            Russian.Add("popularInDescription", "Это популярный пакет");

            //Component: controversialInDescription
            //
            English.Add("controversialInDescription", "This is a controversial package");
            German.Add("controversialInDescription", "Dies ist ein umstrittenes Paket");
            Polish.Add("controversialInDescription", "Kontrowersyjny pakiet");
            French.Add("controversialInDescription", TranslationNeeded);
            Spanish.Add("controversialInDescription", "Este paquete es polémico");
            Russian.Add("controversialInDescription", "Это подозрительный пакет");

            //Component: encryptedInDescription
            //
            English.Add("encryptedInDescription", "This is an encrypted package that can't be checked for viruses");
            German.Add("encryptedInDescription", "Dies ist ein  verschlüsseltes Paket, welches nicht auf Viren überprüft werden kann");
            Polish.Add("encryptedInDescription", "Pakiet zaszyfrowany, nie można go przeskanować pod kątem potencjalnych wirusów.");
            French.Add("encryptedInDescription", TranslationNeeded);
            Spanish.Add("encryptedInDescription", "Este paquete está encriptado y no se puede verificar que no contenga virus.");
            Russian.Add("encryptedInDescription", "Этот пакет зашифрован.\nПроверка на вирусы невозможна");

            //Component: fromWgmodsInDescription
            //
            English.Add("fromWgmodsInDescription", "The source of this package is the WGmods portal (wgmods.net)");
            German.Add("fromWgmodsInDescription", "Die Quelle dieses Pakets ist das WGmods Portal (wgmods.net)");
            Polish.Add("fromWgmodsInDescription", "Ten pakiet pochodzi z portalu WGMods (wgmods.net)");
            French.Add("fromWgmodsInDescription", "La source de cet archive est disponible sur le site WGMods Portal (wgmods.net)");
            Spanish.Add("fromWgmodsInDescription", "La fuente de este paquete es el portal de WGmods (wgmods.net)");
            Russian.Add("fromWgmodsInDescription", "Портал WGMods (wgmods.net) используется в качестве источника для этого пакета");
            #endregion

            #region Developer Selection Window
            //Component: DeveloperSelectionsViewer
            //
            English.Add("DeveloperSelectionsViewer", "Selections Viewer");
            German.Add("DeveloperSelectionsViewer", "Auswahl-Betrachter");
            Polish.Add("DeveloperSelectionsViewer", "Podgląd wyborów");
            French.Add("DeveloperSelectionsViewer", "Visualiseur de sélections");
            Spanish.Add("DeveloperSelectionsViewer", "Visor de selecciones");
            Russian.Add("DeveloperSelectionsViewer", "Просмотр наборов");

            //Component: DeveloperSelectionsTextHeader
            //
            English.Add("DeveloperSelectionsTextHeader", "Selection to load");
            German.Add("DeveloperSelectionsTextHeader", "Auswahl zum Laden");
            Polish.Add("DeveloperSelectionsTextHeader", "Kolekcja do wczytania");
            French.Add("DeveloperSelectionsTextHeader", "Sélection à charger");
            Spanish.Add("DeveloperSelectionsTextHeader", "Selección a cargar");
            Russian.Add("DeveloperSelectionsTextHeader", "Набор для загрузки");

            //Component: DeveloperSelectionsCancelButton
            //
            English.Add("DeveloperSelectionsCancelButton", English["cancel"]);
            German.Add("DeveloperSelectionsCancelButton", German["cancel"]);
            Polish.Add("DeveloperSelectionsCancelButton", Polish["cancel"]);
            French.Add("DeveloperSelectionsCancelButton", French["cancel"]);
            Spanish.Add("DeveloperSelectionsCancelButton", Spanish["cancel"]);
            Russian.Add("DeveloperSelectionsCancelButton", Russian["cancel"]);

            //Component: DeveloperSelectionsLocalFile
            //The text in the first radioButton in the selection viewer, for the user to select their own personal config file to load
            English.Add("DeveloperSelectionsLocalFile", "Local file");
            German.Add("DeveloperSelectionsLocalFile", "Lokale Datei");
            Polish.Add("DeveloperSelectionsLocalFile", "Plik lokalny");
            French.Add("DeveloperSelectionsLocalFile", "Fichier local");
            Spanish.Add("DeveloperSelectionsLocalFile", "Archivo local");
            Russian.Add("DeveloperSelectionsLocalFile", "Локальный файл");

            //Component: DeveloperSelectionsContinueButton
            //
            English.Add("DeveloperSelectionsContinueButton", "Select");
            German.Add("DeveloperSelectionsContinueButton", "Auswahl bestätigen");
            Polish.Add("DeveloperSelectionsContinueButton", "Wybierz");
            French.Add("DeveloperSelectionsContinueButton", "Selectionner");
            Spanish.Add("DeveloperSelectionsContinueButton", "Seleccionar");
            Russian.Add("DeveloperSelectionsContinueButton", "Выбрать");

            //Component: failedToParseSelections
            //
            English.Add("failedToParseSelections", "Failed to parse selections");
            German.Add("failedToParseSelections", "Auswahl konnte nicht analysiert werden");
            Polish.Add("failedToParseSelections", "Nie udało się przetworzyć kolekcji");
            French.Add("failedToParseSelections", "Échec de l'analyse des sélections");
            Spanish.Add("failedToParseSelections", "No se han podido analizar las selecciones");
            Russian.Add("failedToParseSelections", "Сбой обработки набора");

            //Component: lastModified
            //
            English.Add("lastModified", "Last modified");
            German.Add("lastModified", "Zuletzt geändert");
            Polish.Add("lastModified", "Ostatnia modyfikacja");
            French.Add("lastModified", "Dernière modification");
            Spanish.Add("lastModified", "Modificado por última vez");
            Russian.Add("lastModified", "Последнее изменение");
            #endregion

            #region Advanced Installer Window
            //Component: AdvancedProgress
            //
            English.Add("AdvancedProgress", "Advanced Installer Progress");
            German.Add("AdvancedProgress", "Erweiterte Instaltionsfortschrittsnanziege");
            Polish.Add("AdvancedProgress", "Szczegółowy Podgląd Instalacji");
            French.Add("AdvancedProgress", "Progression avancée du programme d'installation");
            Spanish.Add("AdvancedProgress", "Progreso detallado del instalador");
            Russian.Add("AdvancedProgress", "Прогресс расширенной установки");

            //Component: PreInstallTabHeader
            //
            English.Add("PreInstallTabHeader", "Pre-install tasks");
            German.Add("PreInstallTabHeader", "Vorbereitung");
            Polish.Add("PreInstallTabHeader", "Przygotowywanie"); // => "PREPARATION(s)", otherwise it's long and clumsy. @Nullmaruzero
            French.Add("PreInstallTabHeader", "Tâches de pré-installation");
            Spanish.Add("PreInstallTabHeader", "Operaciones pre-instalación");
            Russian.Add("PreInstallTabHeader", "Подготовка"); // "Prepairing", otherwise it's too long. @SigmaTel71 (DrWeb7_1)

            //Component: ExtractionTabHeader
            //
            English.Add("ExtractionTabHeader", "Extraction");
            German.Add("ExtractionTabHeader", "Entpackungsvorgang");
            Polish.Add("ExtractionTabHeader", "Wyodrębnianie");
            French.Add("ExtractionTabHeader", "Extraction");
            Spanish.Add("ExtractionTabHeader", "Extracción");
            Russian.Add("ExtractionTabHeader", "Распаковка");

            //Component: PostInstallTabHeader
            //
            English.Add("PostInstallTabHeader", "Post-install tasks");
            German.Add("PostInstallTabHeader", "Abschlußarbeiten");
            Polish.Add("PostInstallTabHeader", "Zadania poinstalacyjne");
            French.Add("PostInstallTabHeader", "Tâches de post-insallation");
            Spanish.Add("PostInstallTabHeader", "Operaciones post-instalación");
            Russian.Add("PostInstallTabHeader", "Завершение"); // See comment to PreInstallTabHeader. @SigmaTel71 (DrWeb7_1)

            //Component: AdvancedInstallBackupMods
            //
            English.Add("AdvancedInstallBackupMods", "Backup current installation");
            German.Add("AdvancedInstallBackupMods", "Sichere derzeitige Installation (Backup)");
            Polish.Add("AdvancedInstallBackupMods", "Tworzenie kopii zapasowej obecnej instalacji");
            French.Add("AdvancedInstallBackupMods", "Sauvegarder l'installation actuelle");
            Spanish.Add("AdvancedInstallBackupMods", "Hacer una copia de seguridad de la instalación actual");
            Russian.Add("AdvancedInstallBackupMods", "Бэкап имеющихся модов");

            //Component: AdvancedInstallBackupData
            //
            English.Add("AdvancedInstallBackupData", "Backup Mod Data");
            German.Add("AdvancedInstallBackupData", "Sicherungskopie der Mod-Daten");
            Polish.Add("AdvancedInstallBackupData", "Tworzenie kopii zapasowej ustawień modów");
            French.Add("AdvancedInstallBackupData", "Sauvegarder les données du mod");
            Spanish.Add("AdvancedInstallBackupData", "Hacer una copia de seguridad de los datos de los mods");
            Russian.Add("AdvancedInstallBackupData", "Бэкап данных модов");

            //Component: AdvancedInstallClearCache
            //
            English.Add("AdvancedInstallClearCache", "Clear WoT Cache");
            German.Add("AdvancedInstallClearCache", "Lösche den WoT Cache");
            Polish.Add("AdvancedInstallClearCache", "Czyszczenie pamięci podręcznej WoT");
            French.Add("AdvancedInstallClearCache", "Effacer le cache de WoT");
            Spanish.Add("AdvancedInstallClearCache", "Limpiar caché de WoT");
            Russian.Add("AdvancedInstallClearCache", "Очистить кэш WoT");

            //Component: AdvancedInstallClearLogs
            //
            English.Add("AdvancedInstallClearLogs", "Clear Logfiles");
            German.Add("AdvancedInstallClearLogs", "Lösche Protokolldateien");
            Polish.Add("AdvancedInstallClearLogs", "Czyszczenie plików dziennika");
            French.Add("AdvancedInstallClearLogs", "Effacer les fichiers journaux");
            Spanish.Add("AdvancedInstalledClearLogs", "Limpiar archivos de registro");
            Russian.Add("AdvancedInstallClearLogs", "Очистить логи");

            //Component: AdvancedInstallClearMods
            //
            English.Add("AdvancedInstallClearMods", "Uninstall previous installation");
            German.Add("AdvancedInstallClearMods", "Deinstalliere letzte Installation");
            Polish.Add("AdvancedInstallClearMods", "Dezinstalacja poprzedniej instalacji");
            French.Add("AdvancedInstallClearMods", "Désinstaller l'installation précédente");
            Spanish.Add("AdvancedInstallClearMods", "Desinstalar instalación anterior");
            Russian.Add("AdvancedInstallClearMods", "Удалить старые моды");

            //Component: AdvancedInstallInstallMods
            //
            English.Add("AdvancedInstallInstallMods", "Install Thread");
            German.Add("AdvancedInstallInstallMods", "Installiere Thread");
            Polish.Add("AdvancedInstallInstallMods", "Proces instalacyjny"); // Saw it used as a label with a counter (1, 2, 3...). @Nullmaruzero
            French.Add("AdvancedInstallInstallMods", "Fil d'installation");
            Spanish.Add("AdvancedInstallInstallMods", "Hilo de instalación");
            Russian.Add("AdvancedInstallInstallMods", "Поток установки");

            //Component: AdvancedInstallUserInstallMods
            //
            English.Add("AdvancedInstallUserInstallMods", "User Install");
            German.Add("AdvancedInstallUserInstallMods", "Benutzerinstallation");
            Polish.Add("AdvancedInstallUserInstallMods", "Instalowanie modów użytkownika");
            French.Add("AdvancedInstallUserInstallMods", "Installation utilisateur");
            Spanish.Add("AdvancedInstallUserInstallMods", "Instalación del usuario");
            Russian.Add("AdvancedInstallUserInstallMods", "Пользовательские моды");

            //Component: AdvancedInstallRestoreData
            //
            English.Add("AdvancedInstallRestoreData", "Restore Data");
            German.Add("AdvancedInstallRestoreData", "Daten widerherstellen");
            Polish.Add("AdvancedInstallRestoreData", "Przywracanie danych");
            French.Add("AdvancedInstallRestoreData", "Restaurer les données");
            Spanish.Add("AdvancedInstallRestoreData", "Restaurar configuración");
            Russian.Add("AdvancedInstallRestoreData", "Восстановление данных");

            //Component: AdvancedInstallXmlUnpack
            //
            English.Add("AdvancedInstallXmlUnpack", "Xml Unpacker");
            German.Add("AdvancedInstallXmlUnpack", "XML Entpacker");
            Polish.Add("AdvancedInstallXmlUnpack", "Wyodrębnianie XML");
            French.Add("AdvancedInstallXmlUnpack", "Déballeur XML");
            Spanish.Add("AdvancedInstallXmlUnpack", "Desempaquetador de XML");
            Russian.Add("AdvancedInstallXmlUnpack", "XML-распаковщик");

            //Component: AdvancedInstallPatchFiles
            //
            English.Add("AdvancedInstallPatchFiles", "Patch files");
            German.Add("AdvancedInstallPatchFiles", "Patch Dateien");
            Polish.Add("AdvancedInstallPatchFiles", "Aplikowanie poprawek (patchy)");
            French.Add("AdvancedInstallPatchFiles", "Fichiers de patch");
            Spanish.Add("AdvancedInstallPatchFiles", "Archivos de parche");
            Russian.Add("AdvancedInstallPatchFiles", "Патч файла");

            //Component: AdvancedInstallCreateShortcuts
            //
            English.Add("AdvancedInstallCreateShortcuts", "Create Shortcuts");
            German.Add("AdvancedInstallCreateShortcuts", "Erstelle Verknüpfungen");
            Polish.Add("AdvancedInstallCreateShortcuts", "Tworzenie skrótów");
            French.Add("AdvancedInstallCreateShortcuts", "Créer des raccourcis");
            Spanish.Add("AdvancedInstallCreateShortcuts", "Crear accesos directos");
            Russian.Add("AdvancedInstallCreateShortcuts", "Создать ярлыки");

            //Component: AdvancedInstallCreateAtlas
            //
            English.Add("AdvancedInstallCreateAtlas", "Create atlases");
            German.Add("AdvancedInstallCreateAtlas", "Erstelle Atlase");
            Polish.Add("AdvancedInstallCreateAtlas", "Tworzenie plików Atlas");
            French.Add("AdvancedInstallCreateAtlas", "Créer des atlas");
            Spanish.Add("AdvancedInstallCreateAtlas", "Crear atlases");
            Russian.Add("AdvancedInstallCreateAtlas", "Создать атласы");

            //Component: AdvancedInstallInstallFonts
            //
            English.Add("AdvancedInstallInstallFonts", "Install fonts");
            German.Add("AdvancedInstallInstallFonts", "Installiere Schriftarten");
            Polish.Add("AdvancedInstallInstallFonts", "Instalowanie czcionek");
            French.Add("AdvancedInstallInstallFonts", "Installer des polices");
            Spanish.Add("AdvancedInstallInstallFonts", "Instalar fuentes");
            Russian.Add("AdvancedInstallInstallFonts", "Установить шрифты");

            //Component: AdvancedInstallTrimDownloadCache
            //
            English.Add("AdvancedInstallTrimDownloadCache", "Trim download cache");
            German.Add("AdvancedInstallTrimDownloadCache", "Download Cache kürzen");
            Polish.Add("AdvancedInstallTrimDownloadCache", "Ograniczanie pamięci podręcznej pobierania");
            French.Add("AdvancedInstallTrimDownloadCache", "Réduire le cache de téléchargement");
            Spanish.Add("AdvancedInstallTrimDownloadCache", "Recortar la caché de descarga");
            Russian.Add("AdvancedInstallTrimDownloadCache", "Очистить кэш загрузок");

            //Component: AdvancedInstallCleanup
            //
            English.Add("AdvancedInstallCleanup", "Cleanup");
            German.Add("AdvancedInstallCleanup", "Aufräumen");
            Polish.Add("AdvancedInstallCleanup", "Czyszczenie");
            French.Add("AdvancedInstallCleanup", "Nettoyer");
            Spanish.Add("AdvancedInstallCleanup", "Limpieza");
            Russian.Add("AdvancedInstallCleanup", "Очистка");

            #endregion

            #region News Viewer
            //Component: NewsViewer
            //
            English.Add("NewsViewer", "News Viewer");
            German.Add("NewsViewer", "Nachrichten Viewer");
            Polish.Add("NewsViewer", "Przegląd Wiadomości");
            French.Add("NewsViewer", "Visionneuse de nouvelles");
            Spanish.Add("NewsViewer", "Novedades");
            Russian.Add("NewsViewer", "Просмотр новостей");

            //Component: application_Update_TabHeader
            //
            English.Add("application_Update_TabHeader", "Application");
            German.Add("application_Update_TabHeader", "App");
            Polish.Add("application_Update_TabHeader", "Aplikacja");
            French.Add("application_Update_TabHeader", "Nouvelles de l'application");
            Spanish.Add("application_Update_TabHeader", "Aplicación");
            Russian.Add("application_Update_TabHeader", "Изменения в приложении");

            //Component: database_Update_TabHeader
            //
            English.Add("database_Update_TabHeader", "Database");
            German.Add("database_Update_TabHeader", "Datenbank");
            Polish.Add("database_Update_TabHeader", "Baza danych");
            French.Add("database_Update_TabHeader", "Nouvelles de la base de données");
            Spanish.Add("database_Update_TabHeader", "Base de datos");
            Russian.Add("database_Update_TabHeader", "Изменения в БД");

            //Component: ViewNewsOnGoogleTranslate
            //
            English.Add("ViewNewsOnGoogleTranslate", "View this on Google Translate");
            German.Add("ViewNewsOnGoogleTranslate", "Sieh das auf Google Translate an");
            Polish.Add("ViewNewsOnGoogleTranslate", "Wyświetl w Tłumaczu Google");
            French.Add("ViewNewsOnGoogleTranslate", "Voir des nouvelles sur Google Traduction");
            Spanish.Add("ViewNewsOnGoogleTranslate", "Ver en el Traductor de Google");
            Russian.Add("ViewNewsOnGoogleTranslate", "Посмотреть через переводчик Google");
            #endregion

            #region Loading Window
            //Component: ProgressIndicator
            //
            English.Add("ProgressIndicator", "Loading");
            German.Add("ProgressIndicator", "Lade");
            Polish.Add("ProgressIndicator", "Wczytywanie");
            French.Add("ProgressIndicator", "Chargement");
            Spanish.Add("ProgressIndicator", "Cargando");
            Russian.Add("ProgressIndicator", "Загрузка");

            //Component: LoadingHeader
            //
            English.Add("LoadingHeader", "Loading, please wait");
            German.Add("LoadingHeader", "Lade, bitte warten");
            Polish.Add("LoadingHeader", "Wczytywanie, proszę czekać");
            French.Add("LoadingHeader", "Chargement, veuillez patienter");
            Spanish.Add("LoadingHeader", "Cargando, por favor espere");
            Russian.Add("LoadingHeader", "Загрузка, пожалуйста, подождите");
            #endregion

            #region First Load Acknowledgements
            //Component: FirstLoadAcknowledgments
            // ACKS window title.
            English.Add("FirstLoadAcknowledgments", "First Load Acknowledgements");
            German.Add("FirstLoadAcknowledgments", "Einverständniserklärung");
            Polish.Add("FirstLoadAcknowledgments", "Pierwsze Urochomienie — Umowa Licencyjna"); // Can't think of a gracious way to do this one. @Nullmaruzero
            French.Add("FirstLoadAcknowledgments", "Remerciement de première charge");
            Spanish.Add("FirstLoadAcknowledgments", "Primera ejecución — Acuerdo de Licencia"); // Me neither (LordFelix), so i'll follow PL procedure
            Russian.Add("FirstLoadAcknowledgments", "Первый запуск");

            //Component: AgreementLicense
            // The first acknowledgement checkbox, inlines with the LicenseLink licence agreement link at the end.
            // EXAMPLE: [x] I have read and agree to the {License Agreement}
            English.Add("AgreementLicense", "I have read and agree to the ");
            German.Add("AgreementLicense", "Von mir gelesen und bestätigt: ");
            Polish.Add("AgreementLicense", "Przeczytałem/am i zgadzam się z postanowieniami ");
            French.Add("AgreementLicense", "J'ai lu et accepté le");
            Spanish.Add("AgreementLicense", "He leído y acepto el ");
            Russian.Add("AgreementLicense", "Я прочитал и согласен с ");

            //Component: LicenseLink
            // A link inlining at the end with the AgreementLicense first checkbox text from above.
            English.Add("LicenseLink", "License Agreement");
            German.Add("LicenseLink", "Lizenzvereinbarung");
            Polish.Add("LicenseLink", "Umowy Licencyjnej");
            French.Add("LicenseLink", "Accord de licence");
            Spanish.Add("LicenseLink", "Acuerdo de licencia");
            Russian.Add("LicenseLink", "условиями лицензионного соглашения");

            //Component: AgreementSupport1
            // The second acknowledgement checkbox, inlines with link to forums, the conjunction phrase and discord server link.
            // EXAMPLE: [x] I understand that I can receive support on the dedicated {Forums} {or} {Discord}
            English.Add("AgreementSupport1", "I understand that I can receive support on the dedicated ");
            German.Add("AgreementSupport1", "Ich erhalte Support nur über das dafür bereitgestellte ");
            Polish.Add("AgreementSupport1", "Rozumiem, że mogę uzyskać wsparcie techniczne na dedykowanym ");
            French.Add("AgreementSupport1", "Je comprends que je peux recevoir de l’aide sur le ");
            Spanish.Add("AgreementSupport1", "Entiendo que puedo recibir soporte en el ");
            Russian.Add("AgreementSupport1", "Я понимаю, что могу обратиться за помощью ");

            //Component: AgreementSupportForums
            // First of the inlined elements for AgreementSupport1, a link to the forums.
            English.Add("AgreementSupportForums", "Forums");
            German.Add("AgreementSupportForums", "Forum");
            Polish.Add("AgreementSupportForums", "Forum");
            French.Add("AgreementSupportForums", "Forum");
            Spanish.Add("AgreementSupportForums", "Foro");
            Russian.Add("AgreementSupportForums", "на форум");

            //Component: AgreementSupport2
            // Second of the inlined elements for AgreementSupport1, conjunction between a forum link and discord server link.
            English.Add("AgreementSupport2", " or ");
            German.Add("AgreementSupport2", " oder ");
            Polish.Add("AgreementSupport2", " lub serwerze ");
            French.Add("AgreementSupport2", " ou le ");
            Spanish.Add("AgreementSupport2", " dedicado y en ");
            Russian.Add("AgreementSupport2", " или на сервер в ");

            //Component: AgreementSupportDiscord
            // Third and the last inlined elements for AgreementSupport1, a link to Relhax Discord server.
            English.Add("AgreementSupportDiscord", "Discord");
            Polish.Add("AgreementSupportDiscord", "Discord");
            German.Add("AgreementSupportDiscord", "Discord");
            French.Add("AgreementSupportDiscord", "Discord");
            Spanish.Add("AgreementSupportDiscord", "Discord");
            Russian.Add("AgreementSupportDiscord", "Discord");

            //Component: AgreementHoster
            // The third acknowledgement checkbox, just plain text. Make sure you communicate this one well.
            // EXAMPLE: [x] I understand Relhax is a mod hosting and installation service and Relhax does not maintain every mod found in this Modpack
            English.Add("AgreementHoster", "I understand Relhax is a mod hosting and installation service and Relhax does not maintain every mod found in this Modpack");
            German.Add("AgreementHoster", "Ich verstehe, dass Relhax ein Mod-Hosting- und Installationsservice ist und Relhax nicht alle Mods verwaltet, die in diesem Modpack enthalten sind");
            Polish.Add("AgreementHoster", "Rozumiem, że Relhax to platforma instalacji modów oraz ich hosting, a zespół Relhax nie jest odpowiedzialny za rozwój wszystkich oferowanych modów.");
            French.Add("AgreementHoster", "Je comprends que Relhax est un hébergement de mods et un service d'installation. Relhax ne gère pas tous les mods de ce Modpack");
            Spanish.Add("AgreementHoster", "Entiendo que Relhax sólo es un servicio de alojamiento e instalación de mods, y Relhax no mantiene cada mod incluido en este modpack");
            Russian.Add("AgreementHoster", "Я понимаю, что Relhax является площадкой хостинга модов и сервисом их установки и то, что Relhax не занимается разработкой каждого мода из этого модпака");

            //Component: AgreementAnonData
            // The fourth acknowledgement checkbox. Make sure you communicate this one well.
            // EXAMPLE: I understand that Relhax V2 collects anonymous usage data to improve the application, and can be disabled in the advanced settings tab
            English.Add("AgreementAnonData", "I understand that Relhax V2 collects anonymous usage data to improve the application, and can be disabled in the advanced settings tab");
            German.Add("AgreementAnonData", "Ich verstehe, dass Relhax V2 anonyme Nutzungsdaten sammelt, um die Anwendung zu verbessern, und auf der Registerkarte für erweiterte Einstellungen deaktiviert werden kann.");
            Polish.Add("AgreementAnonData", "Rozumiem, że Relhax V2 gromadzi i wysyła anonimowe dane użytkowania celem poprawy aplikacji, oraz że funkcję tę mogę wyłączyć w dowolnym momencie w zakładce 'Zaawansowane'.");
            French.Add("AgreementAnonData", "Je comprends que Relhax V2 collecte des données d'utilisation anonymes pour améliorer l'application, et peut être désactivé dans les options avancées");
            Spanish.Add("AgreementAnonData", "Entiendo que Relhax V2 recoge datos anónimos de uso para mejorar la aplicación, lo cual puede ser deshabilitado en la pestaña de opciones avanzadas");
            Russian.Add("AgreementAnonData", "Я понимаю, что Relhax V2 собирает анонимные сведения об использовании для улучшения приложения и знаю, что могу отключить сбор данных в разделе расширенных настроек");

            //Component: V2UpgradeNoticeText
            // A text block appearing (in red) under the last acknowledgement, centered in the window - only if the app detects that a conversion is required.
            English.Add("V2UpgradeNoticeText", "It looks like you are running an upgrade from V1 to V2 for the first time.\n" +
                "Pressing continue will result in an upgrade to the file structure that cannot be reverted. It is recommended to make a backup of your V1 folder before continuing");
            German.Add("V2UpgradeNoticeText", "Es sieht so aus, als würdest du zum ersten Mal ein Upgrade von V1 auf V2 ausführen.\n" +
                "Wenn du auf Fortsetzen klickst, wird ein Upgrade der Dateistruktur durchgeführt, das nicht wiederhergestellt werden kann. Es wird empfohlen, eine Sicherungskopie deines V1-Ordners zu erstellen, bevor du fortfährst");
            Polish.Add("V2UpgradeNoticeText", "Wygląda na to, że przeprowadzasz aktualizację z wersji V1 na V2 po raz pierwszy.\n" +
                "Kliknięcie 'Kontynuuj' nieodwracalnie przekonwertuje strukturę plików.\nPrzed kontynuacją zaleca się stworzenie kopii zapasowej folderu z wersją V1."); //Line-breaks, guessing it will be more readable. @Nullmaruzero
            French.Add("V2UpgradeNoticeText", "Il semble que vous exécutiez une mise à niveau de V1 à V2 pour la première fois.\n"+
                "Appuyer sur Continuer entraînera une mise à niveau de la structure de fichiers qui ne peut pas être annulée. Il est recommandé de faire une sauvegarde de votre dossier V1 avant de continuer.");
            Spanish.Add("V2UpgradeNoticeText", "Parece que está ejecutando la actualización de V1 a V2 por primera vez.\n" +
                "Pulsar continuar resultará en una actualización de la estructura de archivos que no puede ser revertida. Se recomienda crear una copia de seguridad de su carpeta V1 antes de continuar");
            Russian.Add("V2UpgradeNoticeText", "Похоже, что вы производите апгрейд с V1 на V2 в первый раз.\nПосле нажатия кнопки \"Продолжить\" будет произведено необратимое обновление структуры файлов. Рекомендуется создание бэкапа папки с V1 перед продолжением");

            //Component: upgradingStructure
            // Displays the structure upgrade log within the main app itself, if the user agreed to the conversion in the ACKS window and proceeded. Just like download and any other status.
            English.Add("upgradingStructure", "Upgrading V1 file and folder structure");
            German.Add("upgradingStructure", "Upgrad der V1 Datei- und Ordnerstruktur");
            Polish.Add("upgradingStructure", "Konwertowanie struktury plików i folderów V1");
            French.Add("upgradingStructure", "Upgrading V1 file and folder structure");
            Spanish.Add("upgradingStructure", "Actualizando estructura de archivos y carpetas de V1");
            Russian.Add("upgradingStructure", "Обновление файлов и папок первой версии");
            #endregion

            #region Export Mode
            //Component: ExportModeSelect
            //
            English.Add("ExportModeSelect", "Select WoT client for export");
            German.Add("ExportModeSelect", "Wähle WoT Client für den Export");
            Polish.Add("ExportModeSelect", "Wybierz klienta WoT dla eksportu");
            French.Add("ExportModeSelect", "Sélectionner la version du client de WoT pour exporter");
            Spanish.Add("ExportModeSelect", "Seleccione cliente de WoT para exportación");
            Russian.Add("ExportModeSelect", "Выберите клиент WoT для экспорта");

            //Component: selectLocationToExport
            //
            English.Add("selectLocationToExport", "Select the folder to export the mod installation into");
            German.Add("selectLocationToExport", "Wähle den Ordner für den Export der Mod-Installation");
            Polish.Add("selectLocationToExport", "Wybierz folder docelowy eksportu instalacji");
            French.Add("selectLocationToExport", "Sélectionner le dossier où exporter l'installation de mod");
            Spanish.Add("selectLocationToExport", "Seleccione la carpeta para exportar la instalación de mods");
            Russian.Add("selectLocationToExport", "Выберите папку для экспорта устанавливаемых модов");

            //Component: ExportSelectVersionHeader
            //
            English.Add("ExportSelectVersionHeader", "Please select the version of the WoT client you want to export for");
            German.Add("ExportSelectVersionHeader", "Bitte wähle die WoT Klientversion, für die du den Export durchführen willst");
            Polish.Add("ExportSelectVersionHeader", "Wybierz wersję docelową klienta WoT");
            French.Add("ExportSelectVersionHeader", "Merci de sélectionner la version de votre client WoT pour laquelle vous voulez exporter");
            Spanish.Add("ExportSelectVersionHeader", "Por favor, seleccione la versión del cliente de WoT para la que quiere exportar");
            Russian.Add("ExportSelectVersionHeader", "Выберите версию клиента игры, для которой будет произведён экспорт");

            //Component: ExportContinueButton
            //
            English.Add("ExportContinueButton", English["ContinueButton"]);
            German.Add("ExportContinueButton", German["ContinueButton"]);
            Polish.Add("ExportContinueButton", Polish["ContinueButton"]);
            French.Add("ExportContinueButton", French["ContinueButton"]);
            Spanish.Add("ExportContinueButton", Spanish["ContinueButton"]);
            Russian.Add("ExportContinueButton", Russian["ContinueButton"]);

            //Component: ExportCancelButton
            //
            English.Add("ExportCancelButton", English["cancel"]);
            German.Add("ExportCancelButton", German["cancel"]);
            Polish.Add("ExportCancelButton", Polish["cancel"]);
            French.Add("ExportCancelButton", French["cancel"]);
            Spanish.Add("ExportCancelButton", Spanish["cancel"]);
            Russian.Add("ExportCancelButton", Russian["cancel"]);

            //Component: ExportModeMajorVersion
            //
            English.Add("ExportModeMajorVersion", "Online folder version");
            German.Add("ExportModeMajorVersion", "Version des Online Ordners");
            Polish.Add("ExportModeMajorVersion", "Wersja online folderu");
            French.Add("ExportModeMajorVersion", "Version du dossier en ligne");
            Spanish.Add("ExportModeMajorVersion", "Versión de la carpeta online");
            Russian.Add("ExportModeMajorVersion", "Онлайн-версия папки");

            //Component: ExportModeMinorVersion
            //
            English.Add("ExportModeMinorVersion", "WoT version");
            German.Add("ExportModeMinorVersion", "WoT Version");
            Polish.Add("ExportModeMinorVersion", "Wersja WOT");
            French.Add("ExportModeMinorVersion", "Versions de WoT");
            Spanish.Add("ExportModeMinorVersion", "Versón de WoT");
            Russian.Add("ExportModeMinorVersion", "Версия WoT");
            #endregion

            #region Asking to close WoT
            //Component: AskCloseWoT
            //
            English.Add("AskCloseWoT", "WoT is Running");
            German.Add("AskCloseWoT", "WoT läuft bereits");
            Polish.Add("AskCloseWoT", "WoT jest uruchomiony!");
            French.Add("AskCloseWoT", "WoT est en marche");
            Spanish.Add("AskCloseWoT", "WoT está en ejecución");
            Russian.Add("AskCloseWoT", "Обнаружена запущенная игра!");

            //Component: WoTRunningTitle
            //
            English.Add("WoTRunningTitle", "WoT is Running");
            German.Add("WoTRunningTitle", "WoT wird gerade ausgeführt.");
            Polish.Add("WoTRunningTitle", "WoT jest już uruchomiony");
            French.Add("WoTRunningTitle", "WoT est en cours d`éxecution");
            Spanish.Add("WoTRunningTitle", "WoT está en ejecución");
            Russian.Add("WoTRunningTitle", "World of Tanks запущен");

            //Component: WoTRunningHeader
            //
            English.Add("WoTRunningHeader", "It looks like your WoT install is currently open. Please close it before we can proceed");
            German.Add("WoTRunningHeader", "Es sieht so aus als wäre WoT geöffnet. Bitte schließe das Programm um fortzufahren");
            Polish.Add("WoTRunningHeader", "Wygląda na to, że World of Tanks jest obecnie uruchomione. Przed kontynuowaniem wymagane jest zamknięcie gry.");
            French.Add("WoTRunningHeader", "Il semblerait que vous installation de WoT soit actuellement ouverte. Merci de la fermer avant que nous puissions procéder.");
            Spanish.Add("WoTRunningHeader", "Parece que su instalación de WoT está abierta. Por favor, ciérrela para poder continuar");
            Russian.Add("WoTRunningHeader", "Похоже, сейчас открыта папка с клиентом игры. Закройте её перед тем, как продолжить");

            //Component: WoTRunningCancelInstallButton
            //
            English.Add("WoTRunningCancelInstallButton", "Cancel Installation");
            German.Add("WoTRunningCancelInstallButton", "Abbruch der Installation");
            Polish.Add("WoTRunningCancelInstallButton", "Przerwij instalację");
            French.Add("WoTRunningCancelInstallButton", "Annuler l'installation");
            Spanish.Add("WoTRunningCancelInstallButton", "Cancelar instalación");
            Russian.Add("WoTRunningCancelInstallButton", "Отменить установку");

            //Component: WoTRunningRetryButton
            //
            English.Add("WoTRunningRetryButton", "Re-detect");
            German.Add("WoTRunningRetryButton", "Neuerkennung");
            Polish.Add("WoTRunningRetryButton", "Ponów");
            French.Add("WoTRunningRetryButton", "Re-détecter");
            Spanish.Add("WoTRunningRetryButton", "Volver a detectar");
            Russian.Add("WoTRunningRetryButton", "Перепроверить");

            //Component: WoTRunningForceCloseButton
            //
            English.Add("WoTRunningForceCloseButton", "Force close the game");
            German.Add("WoTRunningForceCloseButton", "Erzwinge das Schließen des Spiels");
            Polish.Add("WoTRunningForceCloseButton", "Wymuś zamknięcie gry");
            French.Add("WoTRunningForceCloseButton", "Forcer la fermeture du jeu");
            Spanish.Add("WoTRunningForceCloseButton", "Forzar el cierre del juego");
            Russian.Add("WoTRunningForceCloseButton", "Принудительно закрыть");
            #endregion

            #region Scaling Confirmation
            //Component: ScalingConfirmation
            //
            English.Add("ScalingConfirmation", "Scaling Confirmation");
            German.Add("ScalingConfirmation", "Bestätige Skalierung");
            Polish.Add("ScalingConfirmation", "Zmiana Skalowania");
            French.Add("ScalingConfirmation", "Confirmation de mise à l'échelle");
            Spanish.Add("ScalingConfirmation", "Confirmar escalado");
            Russian.Add("ScalingConfirmation", "Принять изменения");

            //Component: ScalingConfirmationHeader
            //
            English.Add("ScalingConfirmationHeader", "The scaling value has changed. Would you like to keep it?");
            German.Add("ScalingConfirmationHeader", "Der Wert für die Skalierung hat sich geändert. Willst du dies beibehalten?");
            Polish.Add("ScalingConfirmationHeader", "Ustawienia skalowania zostały zmienione. Czy chcesz je zachować?");
            French.Add("ScalingConfirmationHeader", "La valeur d'échelle à changée. Voulez-vous la gardée ?");
            Spanish.Add("ScalingConfirmationHeader", "El valor de escala ha sido cambiado. ¿Quiere conservarlo?");
            Russian.Add("ScalingConfirmationHeader", "Параметры масштабирования изменены. Хотите сохранить их?");

            //Component: ScalingConfirmationRevertTime
            //
            English.Add("ScalingConfirmationRevertTime", "Reverting in {0} Second(s)");
            German.Add("ScalingConfirmationRevertTime", "Rückgängig machen in {0} Sekunde(n)");
            Polish.Add("ScalingConfirmationRevertTime", "Automatyczne przywrócenie za {0} sekund(y)");
            French.Add("ScalingConfirmationRevertTime", "Retour dans {0} Seconde(s)");
            Spanish.Add("ScalingConfirmationRevertTime", "Revirtiendo cambios en {0} segundo(s)");
            Russian.Add("ScalingConfirmationRevertTime", "Отмена изменений через {0} сек.");

            //Component: ScalingConfirmationKeep
            //
            English.Add("ScalingConfirmationKeep", "Keep");
            German.Add("ScalingConfirmationKeep", "Behalten");
            Polish.Add("ScalingConfirmationKeep", "Zachowaj");
            French.Add("ScalingConfirmationKeep", "Garder");
            Spanish.Add("ScalingConfirmationKeep", "Mantener");
            Russian.Add("ScalingConfirmationKeep", "Сохранить");

            //Component: ScalingConfirmationDiscard
            //
            English.Add("ScalingConfirmationDiscard", "Discard");
            German.Add("ScalingConfirmationDiscard", "Verwerfen");
            Polish.Add("ScalingConfirmationDiscard", "Porzuć");
            French.Add("ScalingConfirmationDiscard", "Jeter");
            Spanish.Add("ScalingConfirmationDiscard", "Descartar");
            Russian.Add("ScalingConfirmationDiscard", "Отменить");
            #endregion

            #region Color Picker
            //Component: RelhaxColorPicker
            //
            English.Add("RelhaxColorPicker", "Color Picker");
            German.Add("RelhaxColorPicker", "Farbauswahl");
            Polish.Add("RelhaxColorPicker", "Schemat Kolorów");
            French.Add("RelhaxColorPicker", "Pipette à couleurs");
            Spanish.Add("RelhaxColorPicker", "Selector de colores");
            Russian.Add("RelhaxColorPicker", "Палитра");

            //Component: ColorType
            //
            English.Add("ColorType", "Brush type");
            German.Add("ColorType", "Pinseltyp");
            Polish.Add("ColorType", "Typ wypełnienia");
            French.Add("ColorType", "Type de pinceau");
            Spanish.Add("ColorType", "Tipo de pincel");
            Russian.Add("ColorType", "Тип кисти");

            //Component: SampleTextColor
            //
            English.Add("SampleTextColor", "Sample Text");
            German.Add("SampleTextColor", "Dies ist ein Text zum Testen");
            Polish.Add("SampleTextColor", "Przykładowy Tekst");
            French.Add("SampleTextColor", "Exemple de texte");
            Spanish.Add("SampleTextColor", "Texto de muestra");
            Russian.Add("SampleTextColor", "Пример текста");

            //Component: MainColor
            //
            English.Add("MainColor", "Main Color");
            German.Add("MainColor", "Primärfarbe");
            Polish.Add("MainColor", "Kolor Podstawowy");
            French.Add("MainColor", "Couleur Principale");
            Spanish.Add("MainColor", "Color Principal");
            Russian.Add("MainColor", "Основной цвет");

            //Component: MainColorAlpha
            //
            English.Add("MainColorAlpha", "Alpha");
            German.Add("MainColorAlpha", "Sichtbarkeit (Alpha)");
            Polish.Add("MainColorAlpha", "Widoczność (alpha)");
            French.Add("MainColorAlpha", "Alpha");
            Spanish.Add("MainColorAlpha", "Opacidad (alfa)");
            Russian.Add("MainColorAlpha", "Прозрачность (альфа-канал)");

            //Component: MainColorRed
            //
            English.Add("MainColorRed", "Red");
            German.Add("MainColorRed", "Rot");
            Polish.Add("MainColorRed", "Czerwony");
            French.Add("MainColorRed", "Rouge");
            Spanish.Add("MainColorRed", "Rojo");
            Russian.Add("MainColorRed", "Красный");

            //Component: MainColorBlue
            //
            English.Add("MainColorBlue", "Blue");
            German.Add("MainColorBlue", "Blau");
            Polish.Add("MainColorBlue", "Niebieski");
            French.Add("MainColorBlue", "Bleu");
            Spanish.Add("MainColorBlue", "Azul");
            Russian.Add("MainColorBlue", "Синий");

            //Component: MainColorGreen
            //
            English.Add("MainColorGreen", "Green");
            German.Add("MainColorGreen", "Grün");
            Polish.Add("MainColorGreen", "Zielony");
            French.Add("MainColorGreen", "Verte");
            Spanish.Add("MainColorGreen", "Verde");
            Russian.Add("MainColorGreen", "Зелёный");

            //Component: TextColor
            //
            English.Add("TextColor", "Text Color");
            German.Add("TextColor", "Textfarbe");
            Polish.Add("TextColor", "Kolor Tekstu");
            French.Add("TextColor", "Couleur du Texte");
            Spanish.Add("TextColor", "Color del Texto");
            Russian.Add("TextColor", "Цвет текста");

            //Component: TextColorAlpha
            //
            English.Add("TextColorAlpha", "Alpha");
            German.Add("TextColorAlpha", "Sichtbarkeit (Alpha)");
            Polish.Add("TextColorAlpha", "Widoczność (alpha)");
            French.Add("TextColorAlpha", "Alpha");
            Spanish.Add("TextColorAlpha", "Opacidad (alfa)");
            Russian.Add("TextColorAlpha", "Прозрачность (альфа-канал)");

            //Component: TextColorRed
            //
            English.Add("TextColorRed", "Red");
            German.Add("TextColorRed", "Rot");
            Polish.Add("TextColorRed", "Czerwony");
            French.Add("TextColorRed", "Rouge");
            Spanish.Add("TextColorRed", "Rojo");
            Russian.Add("TextColorRed", "Красный");

            //Component: TextColorBlue
            //
            English.Add("TextColorBlue", "Blue");
            German.Add("TextColorBlue", "Blau");
            Polish.Add("TextColorBlue", "Niebieski");
            French.Add("TextColorBlue", "Bleu");
            Spanish.Add("TextColorBlue", "Azul");
            Russian.Add("TextColorBlue", "Синий");

            //Component: TextColorGreen
            //
            English.Add("TextColorGreen", "Green");
            German.Add("TextColorGreen", "Grün");
            Polish.Add("TextColorGreen", "Zielony");
            French.Add("TextColorGreen", "Verte");
            Spanish.Add("TextColorGreen", "Verde");
            Russian.Add("TextColorGreen", "Зелёный");

            //Component: SecondColor
            //
            English.Add("SecondColor", "Second Color");
            German.Add("SecondColor", "Sekundärfarbe");
            Polish.Add("SecondColor", "Drugi Kolor");
            French.Add("SecondColor", "Deuxième Couleur");
            Spanish.Add("SecondColor", "Color Secundario");
            Russian.Add("SecondColor", "Второй цвет");

            //Component: SecondColorAlpha
            //
            English.Add("SecondColorAlpha", "Alpha");
            German.Add("SecondColorAlpha", "Sichtbarkeit (Alpha)");
            Polish.Add("SecondColorAlpha", "Widoczność (alpha)");
            French.Add("SecondColorAlpha", "Alpha");
            Spanish.Add("SecondColorAlpha", "Opacidad (alfa)");
            Russian.Add("SecondColorAlpha", "Прозрачность (альфа-канал)");

            //Component: SecondColorRed
            //
            English.Add("SecondColorRed", "Red");
            German.Add("SecondColorRed", "Rot");
            Polish.Add("SecondColorRed", "Czerwony");
            French.Add("SecondColorRed", "Rouge");
            Spanish.Add("SecondColorRed", "Rojo");
            Russian.Add("SecondColorRed", "Красный");

            //Component: SecondColorBlue
            //
            English.Add("SecondColorBlue", "Blue");
            German.Add("SecondColorBlue", "Blau");
            Polish.Add("SecondColorBlue", "Niebieski");
            French.Add("SecondColorBlue", "Bleu");
            Spanish.Add("SecondColorBlue", "Azul");
            Russian.Add("SecondColorBlue", "Синий");

            //Component: SecondColorGreen
            //
            English.Add("SecondColorGreen", "Green");
            German.Add("SecondColorGreen", "Grün");
            Polish.Add("SecondColorGreen", "Zielony");
            French.Add("SecondColorGreen", "Vert");
            Spanish.Add("SecondColorGreen", "Verde");
            Russian.Add("SecondColorGreen", "Зелёный");

            //Component: PointsBlock
            //
            English.Add("PointsBlock", "Point Coordinates");
            German.Add("PointsBlock", "Punkt Koordinaten");
            Polish.Add("PointsBlock", "Koordynaty Punktów");
            French.Add("PointsBlock", "Coordonnées du point");
            Spanish.Add("PointsBlock", "Coordenadas del punto");
            Russian.Add("PointsBlock", "Координаты точки");

            //Component: Point1X
            //
            English.Add("Point1X", "Point 1 X");
            German.Add("Point1X", "Punkt 1 X");
            Polish.Add("Point1X", "Punkt X1");
            French.Add("Point1X", "Point 1 X");
            Spanish.Add("Point1X", "X del Punto 1");
            Russian.Add("Point1X", "Точка 1 по X");

            //Component: Point1Y
            //
            English.Add("Point1Y", "Point 1 Y");
            German.Add("Point1Y", "Punkt 1 Y");
            Polish.Add("Point1Y", "Punkt Y1");
            French.Add("Point1Y", "Point 1 Y");
            Spanish.Add("Point1Y", "Y del Punto 1");
            Russian.Add("Point1Y", "Точка 1 по Y");

            //Component: Point2X
            //
            English.Add("Point2X", "Point 2 X");
            German.Add("Point2X", "Punkt 2 X");
            Polish.Add("Point2X", "Punkt X2");
            French.Add("Point2X", "Point 2 X");
            Spanish.Add("Point2X", "X del Punto 2");
            Russian.Add("Point2X", "Точка 2 по X");

            //Component: Point2Y
            //
            English.Add("Point2Y", "Point 2 Y");
            German.Add("Point2Y", "Punkt 2 Y");
            Polish.Add("Point2Y", "Punkt Y2");
            French.Add("Point2Y", "Point 2 Y");
            Spanish.Add("Point2Y", "Y del Punto 2");
            Russian.Add("Point2Y", "Точка 2 по Y");

            //Component: BrushesLink
            //
            English.Add("BrushesLink", "Read about brush types here");
            German.Add("BrushesLink", "Lies mehr über den Pinseltyp hier");
            Polish.Add("BrushesLink", "Więcej o typach wypełnienia");
            French.Add("BrushesLink", "En savoir plus sur les types de pinceaux");
            Spanish.Add("BrushesLink", "Lea sobre los tipos de pincel aquí");
            Russian.Add("BrushesLink", "Про тип кистей можно прочитать здесь");

            //Component: SampleXmlOutput
            //
            English.Add("SampleXmlOutput", "Sample XML output");
            German.Add("SampleXmlOutput", "XML Sample Ausgabe");
            Polish.Add("SampleXmlOutput", "Kod wynikowy XML");
            French.Add("SampleXmlOutput", "Exemple de sortie XML");
            Spanish.Add("SampleXmlOutput", "Salida de XML de muestra");
            Russian.Add("SampleXmlOutput", "Пример на XML");
            #endregion

            #region Game Center download utility
            //Component: GameCenterUpdateDownloader
            //Application window title
            English.Add("GameCenterUpdateDownloader", "Game Center Update Downloader");
            German.Add("GameCenterUpdateDownloader", "Game Center Update Downloader");
            Polish.Add("GameCenterUpdateDownloader", "Pobieranie aktualizacji gier dla Wargaming Game Center");
            French.Add("GameCenterUpdateDownloader", "Downloader de mise à jour du Game Center");
            Spanish.Add("GameCenterUpdateDownloader", "Descarga de actualizaciones para Wargaming Game Center");
            Russian.Add("GameCenterUpdateDownloader", "Мастер загрузчика обновлений из Game Center");

            //Component: GcDownloadStep1Header
            //
            English.Add("GcDownloadStep1Header", "Select Game Client");
            German.Add("GcDownloadStep1Header", "Wähle Spielclient");
            Polish.Add("GcDownloadStep1Header", "Wybór Klienta Gry");
            French.Add("GcDownloadStep1Header", "Sélectionner le client du jeu");
            Spanish.Add("GcDownloadStep1Header", "Seleccione cliente de juego");
            Russian.Add("GcDownloadStep1Header", "Выберите клиенты игры");

            //Component: GcDownloadStep1TabDescription
            //
            English.Add("GcDownloadStep1TabDescription", "Select the Wargaming client to collect data for (WoT, WoWs, WoWp)");
            German.Add("GcDownloadStep1TabDescription", "Wähle den Wargaming CLient um Daten für (WoT, WoWS, WoWP) zu sammeln");
            Polish.Add("GcDownloadStep1TabDescription", "Wybierz grę WG, dla której chcesz pobrać aktualizacje (WoT, WoWs, WoWp)");
            French.Add("GcDownloadStep1TabDescription", "Sélectionner le client Wargaming pour collecter les informations pour (WoT, Wows, WoWp)");
            Spanish.Add("GcDownloadStep1TabDescription", "Seleccione el cliente de Wargaming para el que recolectar datos (WoT, WoWs, WoWp)");
            Russian.Add("GcDownloadStep1TabDescription", "Выберите клиент игры, установленной через Game Center (WoT, WoWS, WoWP)");

            //Component: GcDownloadStep1SelectClientButton
            //
            English.Add("GcDownloadStep1SelectClientButton", "Select client");
            German.Add("GcDownloadStep1SelectClientButton", "Wähle Client");
            Polish.Add("GcDownloadStep1SelectClientButton", "Wybierz klienta gry");
            French.Add("GcDownloadStep1SelectClientButton", "Sélectionner un client");
            Spanish.Add("GcDownloadStep1SelectClientButton", "Seleccione cliente");
            Russian.Add("GcDownloadStep1SelectClientButton", "Выбрать клиент");

            //Component: GcDownloadStep1CurrentlySelectedClient
            //
            English.Add("GcDownloadStep1CurrentlySelectedClient", "Currently selected client: {0}");
            German.Add("GcDownloadStep1CurrentlySelectedClient", "Derzeit gewählter CLient: {0}");
            Polish.Add("GcDownloadStep1CurrentlySelectedClient", "Obecnie wybrany klient: {0}");
            French.Add("GcDownloadStep1CurrentlySelectedClient", "Client actuellement sélectionné: {0}");
            Spanish.Add("GcDownloadStep1CurrentlySelectedClient", "Cliente actualmente seleccionado: {0}");
            Russian.Add("GcDownloadStep1CurrentlySelectedClient", "Выбранный клиент");

            //Component: GcDownloadStep1NextText
            //
            English.Add("GcDownloadStep1NextText", English["next"]);
            German.Add("GcDownloadStep1NextText", German["next"]);
            Polish.Add("GcDownloadStep1NextText", Polish["next"]);
            French.Add("GcDownloadStep1NextText", French["next"]);
            Spanish.Add("GcDownloadStep1NextText", Spanish["next"]);
            Russian.Add("GcDownloadStep1NextText", Russian["next"]);

            //Component: GcDownloadStep1GameCenterCheckbox
            //
            English.Add("GcDownloadStep1GameCenterCheckbox", "Check for game center updates instead");
            German.Add("GcDownloadStep1GameCenterCheckbox", "Suche stattdessen nach Game Center Updates");
            Polish.Add("GcDownloadStep1GameCenterCheckbox", "Sprawdź aktualizacje tylko dla Wargaming Game Center");
            French.Add("GcDownloadStep1GameCenterCheckbox", "Chercher pour des mises à jour du game center uniquement");
            Spanish.Add("GcDownloadStep1GameCenterCheckbox", "Comprobar actualizaciones de Game Center en su lugar");
            Russian.Add("GcDownloadStep1GameCenterCheckbox", "Проверить наличие обновлений для WGC вместо игры");

            //Component: GcDownloadSelectWgClient
            //
            English.Add("GcDownloadSelectWgClient", "Select WG Client");
            German.Add("GcDownloadSelectWgClient", "Wähle WG Client");
            Polish.Add("GcDownloadSelectWgClient", "Wybierz klienta docelowej gry WG, dla której chcesz pobrać aktualizacje");
            French.Add("GcDownloadSelectWgClient", "Sélectionner le client WG");
            Spanish.Add("GcDownloadSelectWgClient", "Seleccione cliente de WG para el que buscar actualizaciones");
            Russian.Add("GcDownloadSelectWgClient", "Выбрать клиент (WGC)");

            //Component: ClientTypeValue
            //Initial value for the Component -> "None" (No current entry)
            English.Add("ClientTypeValue", "None");
            German.Add("ClientTypeValue", "Nichts");
            Polish.Add("ClientTypeValue", "Brak");
            French.Add("ClientTypeValue", "Aucun");
            Spanish.Add("ClientTypeValue", "Ninguno");
            Russian.Add("ClientTypeValue", "Нет данных");

            //Component: LangValue
            //
            English.Add("LangValue", English["ClientTypeValue"]);
            German.Add("LangValue", German["ClientTypeValue"]);
            Polish.Add("LangValue", Polish["ClientTypeValue"]);
            French.Add("LangValue", French["ClientTypeValue"]);
            Spanish.Add("LangValue", Spanish["ClientTypeValue"]);
            Russian.Add("LangValue", Russian["ClientTypeValue"]);

            //Component: MetadataVersionValue
            //
            English.Add("MetadataVersionValue", English["ClientTypeValue"]);
            German.Add("MetadataVersionValue", German["ClientTypeValue"]);
            Polish.Add("MetadataVersionValue", Polish["ClientTypeValue"]);
            French.Add("MetadataVersionValue", French["ClientTypeValue"]);
            Spanish.Add("MetadataVersionValue", Spanish["ClientTypeValue"]);
            Russian.Add("MetadataVersionValue", Russian["ClientTypeValue"]);

            //Component: MetadataProtocolVersionValue
            //
            English.Add("MetadataProtocolVersionValue", English["ClientTypeValue"]);
            German.Add("MetadataProtocolVersionValue", German["ClientTypeValue"]);
            Polish.Add("MetadataProtocolVersionValue", Polish["ClientTypeValue"]);
            French.Add("MetadataProtocolVersionValue", French["ClientTypeValue"]);
            Spanish.Add("MetadataProtocolVersionValue", Spanish["ClientTypeValue"]);
            Russian.Add("MetadataProtocolVersionValue", Russian["ClientTypeValue"]);

            //Component: ChainIDValue
            //
            English.Add("ChainIDValue", English["ClientTypeValue"]);
            German.Add("ChainIDValue", German["ClientTypeValue"]);
            Polish.Add("ChainIDValue", Polish["ClientTypeValue"]);
            French.Add("ChainIDValue", French["ClientTypeValue"]);
            Spanish.Add("ChainIDValue", Spanish["ClientTypeValue"]);
            Russian.Add("ChainIDValue", Russian["ClientTypeValue"]);

            //Component: ClientCurrentVersionValue
            //
            English.Add("ClientCurrentVersionValue", English["ClientTypeValue"]);
            German.Add("ClientCurrentVersionValue", German["ClientTypeValue"]);
            Polish.Add("ClientCurrentVersionValue", Polish["ClientTypeValue"]);
            French.Add("ClientCurrentVersionValue", French["ClientTypeValue"]);
            Spanish.Add("ClientCurrentVersionValue", Spanish["ClientTypeValue"]);
            Russian.Add("ClientCurrentVersionValue", Russian["ClientTypeValue"]);

            //Component: LocaleCurrentVersionValue
            //
            English.Add("LocaleCurrentVersionValue", English["ClientTypeValue"]);
            German.Add("LocaleCurrentVersionValue", German["ClientTypeValue"]);
            Polish.Add("LocaleCurrentVersionValue", Polish["ClientTypeValue"]);
            French.Add("LocaleCurrentVersionValue", French["ClientTypeValue"]);
            Spanish.Add("LocaleCurrentVersionValue", Spanish["ClientTypeValue"]);
            Russian.Add("LocaleCurrentVersionValue", Russian["ClientTypeValue"]);

            //Component: SdContentCurrentVersionValue
            //
            English.Add("SdContentCurrentVersionValue", English["ClientTypeValue"]);
            German.Add("SdContentCurrentVersionValue", German["ClientTypeValue"]);
            Polish.Add("SdContentCurrentVersionValue", Polish["ClientTypeValue"]);
            French.Add("SdContentCurrentVersionValue", French["ClientTypeValue"]);
            Spanish.Add("SdContentCurrentVersionValue", Spanish["ClientTypeValue"]);
            Russian.Add("SdContentCurrentVersionValue", Russian["ClientTypeValue"]);

            //Component: HdContentCurrentVersionValue
            //
            English.Add("HdContentCurrentVersionValue", English["ClientTypeValue"]);
            German.Add("HdContentCurrentVersionValue", German["ClientTypeValue"]);
            Polish.Add("HdContentCurrentVersionValue", Polish["ClientTypeValue"]);
            French.Add("HdContentCurrentVersionValue", French["ClientTypeValue"]);
            Spanish.Add("HdContentCurrentVersionValue", Spanish["ClientTypeValue"]);
            Russian.Add("HdContentCurrentVersionValue", Russian["ClientTypeValue"]);

            //Component: GameIDValue
            //
            English.Add("GameIDValue", English["ClientTypeValue"]);
            German.Add("GameIDValue", German["ClientTypeValue"]);
            Polish.Add("GameIDValue", Polish["ClientTypeValue"]);
            French.Add("GameIDValue", French["ClientTypeValue"]);
            Spanish.Add("GameIDValue", Spanish["ClientTypeValue"]);
            Russian.Add("GameIDValue", Russian["ClientTypeValue"]);

            //Component: GcMissingFiles
            //
            English.Add("GcMissingFiles", "Your Client is missing the following xml definition files");
            German.Add("GcMissingFiles", "Deinem Clienten fehlen die folgenden XML Definitionsdateien");
            Polish.Add("GcMissingFiles", "Twój klient nie posiada następujących plików definicji XML"); // I need to see this in action with the output. @Nullmaruzero
            French.Add("GcMissingFiles", "Votre client n'a pas le fichier de définitions xml suivant");
            Spanish.Add("GcMissingFiles", "Su cliente no tiene los siguientes archivos XML de definiciones");
            Russian.Add("GcMissingFiles", "У выбранного вами клиента отсутствуют следующие XML-файлы");

            //Component: GcDownloadStep2Header
            //
            English.Add("GcDownloadStep2Header", "Close Game Center");
            German.Add("GcDownloadStep2Header", "Schließe Game Center");
            Polish.Add("GcDownloadStep2Header", "Zamykanie WG Game Center");
            French.Add("GcDownloadStep2Header", "Fermer le Game Center");
            Spanish.Add("GcDownloadStep2Header", "Cerrar Game Center");
            Russian.Add("GcDownloadStep2Header", "Закрыть Game Center");

            //Component: GcDownloadStep2TabDescription
            //
            English.Add("GcDownloadStep2TabDescription", "Close the game center (application will detect closure)");
            German.Add("GcDownloadStep2TabDescription", "Schließe das Spiel (das Programm erkennt das Schließen)");
            Polish.Add("GcDownloadStep2TabDescription", "Proszę zamknąć WG Game Center (aplikacja automatycznie wykryje zamknięcie).");
            French.Add("GcDownloadStep2TabDescription", "Fermer le game center (L'application va détecter la fermeture");
            Spanish.Add("GcDownloadStep2TabDescription", "Ciera el Game Center (la aplicación detectará el cierre)");
            Russian.Add("GcDownloadStep2TabDescription", "Закройте Wargaming Game Center (приложение обнаружит, что он закрылся)");

            //Component: GcDownloadStep2GcStatus
            //Game Center is [Opened,Closed]
            English.Add("GcDownloadStep2GcStatus", "Game Center is {0}");
            German.Add("GcDownloadStep2GcStatus", "Game Center ist {0}");
            Polish.Add("GcDownloadStep2GcStatus", "Game Center jest {0}");
            French.Add("GcDownloadStep2GcStatus", "Le Game Center est {0}");
            Spanish.Add("GcDownloadStep2GcStatus", "Game Center está {0}");
            Russian.Add("GcDownloadStep2GcStatus", "На данный момент Game Center {0}");

            //Component: GcDownloadStep2GcStatusOpened
            //Game Center is [Opened,Closed]
            English.Add("GcDownloadStep2GcStatusOpened", "opened");
            German.Add("GcDownloadStep2GcStatusOpened", "geöffnet");
            Polish.Add("GcDownloadStep2GcStatusOpened", "uruchomione"); // Lowercase for the looks 'cuz it's inlined. @Nullmaruzero
            French.Add("GcDownloadStep2GcStatusOpened", "Ouvert");
            Spanish.Add("GcDownloadStep2GcStatusOpened", "abierto");
            Russian.Add("GcDownloadStep2GcStatusOpened", "запущен");

            //Component: GcDownloadStep2GcStatusClosed
            //Game Center is [Opened,Closed]
            English.Add("GcDownloadStep2GcStatusClosed", "closed");
            German.Add("GcDownloadStep2GcStatusClosed", "geschlossen");
            Polish.Add("GcDownloadStep2GcStatusClosed", "zamknięte"); // Lowercase for the looks 'cuz it's inlined. @Nullmaruzero
            French.Add("GcDownloadStep2GcStatusClosed", "Fermé");
            Spanish.Add("GcDownloadStep2GcStatusClosed", "cerrado");
            Russian.Add("GcDownloadStep2GcStatusClosed", "не запущен"); // it's not "closed", it's not a shopping center. Typical Russian. - DrWeb7_1

            //Component: GcDownloadStep2PreviousText
            //
            English.Add("GcDownloadStep2PreviousText", English["previous"]);
            German.Add("GcDownloadStep2PreviousText", German["previous"]);
            Polish.Add("GcDownloadStep2PreviousText", Polish["previous"]);
            French.Add("GcDownloadStep2PreviousText", French["previous"]);
            Spanish.Add("GcDownloadStep2PreviousText", Spanish["previous"]);
            Russian.Add("GcDownloadStep2PreviousText", Russian["previous"]);

            //Component: GcDownloadStep2NextText
            //
            English.Add("GcDownloadStep2NextText", English["next"]);
            German.Add("GcDownloadStep2NextText", German["next"]);
            Polish.Add("GcDownloadStep2NextText", Polish["next"]);
            French.Add("GcDownloadStep2NextText", French["next"]);
            Spanish.Add("GcDownloadStep2NextText", Spanish["next"]);
            Russian.Add("GcDownloadStep2NextText", Russian["next"]);

            //Component: GcDownloadStep3Header
            //
            English.Add("GcDownloadStep3Header", "Get Update Information");
            German.Add("GcDownloadStep3Header", "Hole Update Information");
            Polish.Add("GcDownloadStep3Header", "Informacje o Aktualizacji");
            French.Add("GcDownloadStep3Header", "Obtention des informations de mise à jour");
            Spanish.Add("GcDownloadStep3Header", "Obtener información de actualización");
            Russian.Add("GcDownloadStep3Header", "Получить сведения об обновлениях");

            //Component: GcDownloadStep3TabDescription
            //
            English.Add("GcDownloadStep3TabDescription", "Getting the list of patch files to download");
            German.Add("GcDownloadStep3TabDescription", "Hole die Liste der Patchdateien für den Download");
            Polish.Add("GcDownloadStep3TabDescription", "Przetwarzanie listy plików aktualizacji do pobrania");
            French.Add("GcDownloadStep3TabDescription", "Obtention de la liste des fichiers de patch à télécharger");
            Spanish.Add("GcDownloadStep3TabDescription", "Obteniendo lista de archivos de parche a descargar");
            Russian.Add("GcDownloadStep3TabDescription", "Получение сведений о списке файлов патчей, необходимых для загрузки");

            //Component: GcDownloadStep3PreviousText
            //
            English.Add("GcDownloadStep3PreviousText", English["previous"]);
            German.Add("GcDownloadStep3PreviousText", German["previous"]);
            Polish.Add("GcDownloadStep3PreviousText", Polish["previous"]);
            French.Add("GcDownloadStep3PreviousText", French["previous"]);
            Spanish.Add("GcDownloadStep3PreviousText", Spanish["previous"]);
            Russian.Add("GcDownloadStep3PreviousText", Russian["previous"]);

            //Component: GcDownloadStep3NextText
            //
            English.Add("GcDownloadStep3NextText", English["next"]);
            German.Add("GcDownloadStep3NextText", German["next"]);
            Polish.Add("GcDownloadStep3NextText", Polish["next"]);
            French.Add("GcDownloadStep3NextText", French["next"]);
            Spanish.Add("GcDownloadStep3NextText", Spanish["next"]);
            Russian.Add("GcDownloadStep3NextText", Russian["next"]);

            //Component: GcDownloadStep3NoFilesUpToDate
            //
            English.Add("GcDownloadStep3NoFilesUpToDate", "No patch files to download (up to date)");
            German.Add("GcDownloadStep3NoFilesUpToDate", "Keine Patchdateien zum Download (aktuell)");
            Polish.Add("GcDownloadStep3NoFilesUpToDate", "Brak plików do pobrania, wszystkie są aktualne.");
            French.Add("GcDownloadStep3NoFilesUpToDate", "Aucun fichiers de patch à télécharger(à jour)");
            Spanish.Add("GcDownloadStep3NoFilesUpToDate", "No se han encontrado archivos de parche para descargar");
            Russian.Add("GcDownloadStep3NoFilesUpToDate", "Вы используете актуальную версию игры.");

            //Component: GcDownloadStep4Header
            //
            English.Add("GcDownloadStep4Header", "Download Update Files");
            German.Add("GcDownloadStep4Header", "Lade Dateien für das Update runter");
            Polish.Add("GcDownloadStep4Header", "Pobieranie Aktualizacji");
            French.Add("GcDownloadStep4Header", "Téléchargement des fichiers de mise à jour");
            Spanish.Add("GcDownloadStep4Header", "Descargar archivos de actualización");
            Russian.Add("GcDownloadStep4Header", "Скачать файлы обновлений");

            //Component: GcDownloadStep4TabDescription
            //
            English.Add("GcDownloadStep4TabDescription", "Downloading the patch files...");
            German.Add("GcDownloadStep4TabDescription", "Lade Patchdateien runter...");
            Polish.Add("GcDownloadStep4TabDescription", "Pobieranie plików aktualizacji...");
            French.Add("GcDownloadStep4TabDescription", "Téléchargement des fichiers de patch");
            Spanish.Add("GcDownloadStep4TabDescription", "Descargando archivos de parche...");
            Russian.Add("GcDownloadStep4TabDescription", "Обновление клиента игры: получение обновлений");

            //Component: GcDownloadStep4DownloadingCancelButton
            //
            English.Add("GcDownloadStep4DownloadingCancelButton", English["cancel"]);
            German.Add("GcDownloadStep4DownloadingCancelButton", German["cancel"]);
            Polish.Add("GcDownloadStep4DownloadingCancelButton", Polish["cancel"]);
            French.Add("GcDownloadStep4DownloadingCancelButton", French["cancel"]);
            Spanish.Add("GcDownloadStep4DownloadingCancelButton", Spanish["cancel"]);
            Russian.Add("GcDownloadStep4DownloadingCancelButton", Russian["cancel"]);

            //Component: GcDownloadStep4DownloadingText
            //Downloading patch 1 of 2: wg_filename.wgpkg
            English.Add("GcDownloadStep4DownloadingText", "Downloading patch {0} of {1}: {2}");
            German.Add("GcDownloadStep4DownloadingText", "Lade Patch {0} von {1} runter: {2}");
            Polish.Add("GcDownloadStep4DownloadingText", "Pobieranie aktualizacji {0} z {1}: {2}");
            French.Add("GcDownloadStep4DownloadingText", "Téléchargement de patch {0} de {1}: {2}");
            Spanish.Add("GcDownloadStep4DownloadingText", "Descargando parche {0} de  {1}: {2}");
            Russian.Add("GcDownloadStep4DownloadingText", "Скачивание обновления {0} из {1}: {2}");

            //Component: GcDownloadStep4Previous
            //
            English.Add("GcDownloadStep4PreviousText", English["previous"]);
            German.Add("GcDownloadStep4PreviousText", German["previous"]);
            Polish.Add("GcDownloadStep4PreviousText", Polish["previous"]);
            French.Add("GcDownloadStep4PreviousText", French["previous"]);
            Spanish.Add("GcDownloadStep4PreviousText", Spanish["previous"]);
            Russian.Add("GcDownloadStep4PreviousText", Russian["previous"]);

            //Component: GcDownloadStep4NextText
            //
            English.Add("GcDownloadStep4NextText", English["next"]);
            German.Add("GcDownloadStep4NextText", German["next"]);
            Polish.Add("GcDownloadStep4NextText", Polish["next"]);
            French.Add("GcDownloadStep4NextText", French["next"]);
            Spanish.Add("GcDownloadStep4NextText", Spanish["next"]);
            Russian.Add("GcDownloadStep4NextText", Russian["next"]);

            //Component: GcDownloadStep4DownloadComplete
            //
            English.Add("GcDownloadStep4DownloadComplete", "Package downloads complete");
            German.Add("GcDownloadStep4DownloadComplete", "Paketdownload abgeschlossen");
            Polish.Add("GcDownloadStep4DownloadComplete", "Pobieranie plików aktualizacji zakończone sukcesem!");
            French.Add("GcDownloadStep4DownloadComplete", "Téléchargement des packages terminé !");
            Spanish.Add("GcDownloadStep4DownloadComplete", "Descarga de paquetes completada");
            Russian.Add("GcDownloadStep4DownloadComplete", "Получение обновлений завершено");

            //Component: GcDownloadStep5Header
            //
            English.Add("GcDownloadStep5Header", "Complete!");
            German.Add("GcDownloadStep5Header", "Abgeschlossen");
            Polish.Add("GcDownloadStep5Header", "Zakończono!");
            French.Add("GcDownloadStep5Header", "Compléter !");
            Spanish.Add("GcDownloadStep5Header", "¡Completado!");
            Russian.Add("GcDownloadStep5Header", "Готово!");

            //Component: GcDownloadStep5TabDescription
            //
            English.Add("GcDownloadStep5TabDescription", "The process is complete! The game center should detect the files when opened.");
            German.Add("GcDownloadStep5TabDescription", "Der Vorgang is fertig. Das Game Center sollte die Dateien beim Öffnen erkennen.");
            Polish.Add("GcDownloadStep5TabDescription", "Proces zakończony sukcesem! WG Game Center powinno wykryć pobrane pliki aktualizacji przy uruchomieniu.");
            French.Add("GcDownloadStep5TabDescription", "Le processus est complet. Le game center devrait détecter les fichiers quand ils seront ouverts.");
            Spanish.Add("GcDownloadStep5TabDescription", "¡El proceso se ha completado! WG Game Center debería detectar los archivos en su próxima ejecución");
            Russian.Add("GcDownloadStep5TabDescription", "Процесс успешно завершён. Wargaming Game Center должен обнаружить файлы после запуска.");

            //Component: GcDownloadStep5CloseText
            //
            English.Add("GcDownloadStep5CloseText", English["close"]);
            German.Add("GcDownloadStep5CloseText", German["close"]);
            Polish.Add("GcDownloadStep5CloseText", Polish["close"]);
            French.Add("GcDownloadStep5CloseText", French["close"]);
            Spanish.Add("GcDownloadStep5CloseText", Spanish["close"]);
            Russian.Add("GcDownloadStep5CloseText", Russian["close"]);

            //Component: GcDownloadStep1ValueError
            //
            English.Add("GcDownloadStep1ValueError", English["error"]);
            German.Add("GcDownloadStep1ValueError", German["error"]);
            Polish.Add("GcDownloadStep1ValueError", Polish["error"]);
            French.Add("GcDownloadStep1ValueError", French["error"]);
            Spanish.Add("GcDownloadStep1ValueError", Spanish["error"]);
            Russian.Add("GcDownloadStep1ValueError", Russian["error"]);
            #endregion

            #region Select Language Window
            //Component: FirstLoadSelectLanguage
            // Application window title.
            English.Add("FirstLoadSelectLanguage", "Language Selection");
            German.Add("FirstLoadSelectLanguage", "Sprachauswahl");
            Polish.Add("FirstLoadSelectLanguage", "Wybór Języka");
            French.Add("FirstLoadSelectLanguage", "Sélection de la langue");
            Spanish.Add("FirstLoadSelectLanguage", "Selección de Idioma");
            Russian.Add("FirstLoadSelectLanguage", "Язык интерфейса");

            //Component: SelectLanguageHeader
            // A header (label) in the window with a list of radio buttons with available languages below.
            English.Add("SelectLanguageHeader", "Please select your language");
            German.Add("SelectLanguageHeader", "Bitte wähle deine Sprache");
            Polish.Add("SelectLanguageHeader", "Wybierz język aplikacji");
            French.Add("SelectLanguageHeader", "S'il vous plaît, sélectionnez votre langue");
            Spanish.Add("SelectLanguageHeader", "Por favor, seleccione el idioma");
            Russian.Add("SelectLanguageHeader", "Пожалуйста, выберите язык интерфейса");

            //Component: SelectLanguagesContinueButton
            // Just a "Continue" button on the bottom. Proceeds to first-run acknowlegements window.
            English.Add("SelectLanguagesContinueButton", English["ContinueButton"]);
            German.Add("SelectLanguagesContinueButton", German["ContinueButton"]);
            Polish.Add("SelectLanguagesContinueButton", Polish["ContinueButton"]);
            French.Add("SelectLanguagesContinueButton", French["ContinueButton"]);
            Spanish.Add("SelectLanguagesContinueButton", Spanish["ContinueButton"]);
            Russian.Add("SelectLanguagesContinueButton", Russian["ContinueButton"]);
            #endregion

            #region Credits
            //Component: Credits
            //Application window title
            English.Add("Credits", "Relhax Modpack Credits");
            German.Add("Credits", "Relhax Modpack Danksagung");
            Polish.Add("Credits", "Lista zasług Relhax Modpack");
            French.Add("Credits", TranslationNeeded);
            Spanish.Add("Credits", "Créditos de Relhax Modpack");
            Russian.Add("Credits", "Авторы Relhax Modpack");

            //Component: creditsProjectLeader
            //
            English.Add("creditsProjectLeader", "Project Leader");
            German.Add("creditsProjectLeader", "Projektleiter");
            Polish.Add("creditsProjectLeader", "Przewodnictwo Projektu");
            French.Add("creditsProjectLeader", TranslationNeeded);
            Spanish.Add("creditsProjectLeader", "Líder de proyecto");
            Russian.Add("creditsProjectLeader", "Руководитель проекта");

            //Component: creditsDatabaseManagers
            //
            English.Add("creditsDatabaseManagers", "Database Managers");
            German.Add("creditsDatabaseManagers", "Datenbank Manager");
            Polish.Add("creditsDatabaseManagers", "Administracja Bazy Danych");
            French.Add("creditsDatabaseManagers", TranslationNeeded);
            Spanish.Add("creditsDatabaseManagers", "Administradores de la base de datos");
            Russian.Add("creditsDatabaseManagers", "Операторы базы данных");

            //Component: creditsTranslators
            //
            English.Add("creditsTranslators", "Translators");
            German.Add("creditsTranslators", "Übersetzer");
            Polish.Add("creditsTranslators", "Tłumaczenia");
            French.Add("creditsTranslators", TranslationNeeded);
            Spanish.Add("creditsTranslators", "Traductores");
            Russian.Add("creditsTranslators", "Локализация");

            //Component: creditsusingOpenSourceProjs
            //
            English.Add("creditsusingOpenSourceProjs", "Relhax Modpack uses the following Open Source projects");
            German.Add("creditsusingOpenSourceProjs", "Relhax Modpack nutzt folgende Open-Source Projekte");
            Polish.Add("creditsusingOpenSourceProjs", "Otwarte oprogramowanie wykorzystywane w modpacku");
            French.Add("creditsusingOpenSourceProjs", TranslationNeeded);
            Spanish.Add("creditsusingOpenSourceProjs", "Relhax Modpack usa los siguientes proyectos de código abierto");
            Russian.Add("creditsusingOpenSourceProjs", "В Relhax Modpack применяются следующие проекты с открытым исходным кодом");

            //Component: creditsSpecialThanks
            //
            English.Add("creditsSpecialThanks", "Special thanks");
            German.Add("creditsSpecialThanks", "Besonderer Dank");
            Polish.Add("creditsSpecialThanks", "Specjalne podziękowania dla");
            French.Add("creditsSpecialThanks", TranslationNeeded);
            Spanish.Add("creditsSpecialThanks", "Agradecimientos especiales");
            Russian.Add("creditsSpecialThanks", "Особая благодарность");

            //Component: creditsGrumpelumpf
            //
            English.Add("creditsGrumpelumpf", "Project leader of OMC, allowed us to pick up Relhax from where he left off");
            German.Add("creditsGrumpelumpf", "Projektleiter des OMC Modpack, hat Relhax erlaubt dort weiter zu machen wo er aufgehört hat");
            Polish.Add("creditsGrumpelumpf", "Lidera projektu OMC, za umożliwienie nam kontynuacji projektu po swoim odejściu");
            French.Add("creditsGrumpelumpf", TranslationNeeded);
            Spanish.Add("creditsGrumpelumpf", "Líder de proyecto de OMC Modpack, nos permitió retomar Relhax desde donde él lo dejó");
            Russian.Add("creditsGrumpelumpf", "руководитель проекта OMC Modpack, позволивший нам работать над Relhax, когда он отошёл от дел.");

            //Component: creditsRkk1945
            //
            English.Add("creditsRkk1945", "The first beta tester who worked with me for months to get the project running");
            German.Add("creditsRkk1945", "Der erste Betatester, der mir über Monate half das Projekt zu auf die Beine zu stellen");
            Polish.Add("creditsRkk1945", "Pierwszego beta-testera, który pracował ze mną nad uruchomieniem projektu przez miesiące");
            French.Add("creditsRkk1945", TranslationNeeded);
            Spanish.Add("creditsRkk1945", "El primer beta tester que trabajó conmigo durante meses para poner en marcha el proyecto");
            Russian.Add("creditsRkk1945", "первый бета-тестер, кто работал со мной месяцами, чтобы привести проект в работоспособный вид.");

            //Component: creditsRgc
            // RGC = Relic Gaming Community
            English.Add("creditsRgc", "Sponsoring the modpack and being my first beta tester group");
            German.Add("creditsRgc", "Sponsor des Modpacks und meine erste Betatestgruppe");
            Polish.Add("creditsRgc", "Za sponsorowanie modpacka oraz zostanie moją pierwszą grupą beta-testerów");
            French.Add("creditsRgc", TranslationNeeded);
            Spanish.Add("creditsRgc", "Sponsor del modpack y miembro del primer grupo de beta testers");
            Russian.Add("creditsRgc", "за продвижение модпака, а так же за то, что стали первой группой бета-тестеров.");

            //Component: creditsBetaTestersName
            //
            English.Add("creditsBetaTestersName", "Our beta testing team");
            German.Add("creditsBetaTestersName", "Unser Betatest Team");
            Polish.Add("creditsBetaTestersName", "Naszych Beta-Testerów");
            French.Add("creditsBetaTestersName", TranslationNeeded);
            Spanish.Add("creditsBetaTestersName", "Nuestro equipo de beta testers");
            Russian.Add("creditsBetaTestersName", "Нашей команде бета-тестеров");

            //Component: creditsBetaTesters
            //
            English.Add("creditsBetaTesters", "Continuing to test and report issues in the application before it goes live");
            German.Add("creditsBetaTesters", "Kontinuierlich Probleme in der Anwendung zu testen und zu melden, bevor sie live geschaltet wird");
            Polish.Add("creditsBetaTesters", "Za nieustanne testowanie i zgłaszanie błędów zanim aplikacja trafi do wszystkich użytkowników");
            French.Add("creditsBetaTesters", TranslationNeeded);
            Spanish.Add("creditsBetaTesters", "Testeo continuado e informe de bugs en la aplicación antes de su publicación");
            Russian.Add("creditsBetaTesters", "за тщательное тестирование и отправку отчётов об ошибках перед выходом в релиз.");

            //Component: creditsSilvers
            //
            English.Add("creditsSilvers", "Helping with the community outreach and social networking");
            German.Add("creditsSilvers", "Hilfe bei der Öffentlichkeitsarbeit und sozialen Netzwerken");
            Polish.Add("creditsSilvers", "Za pomoc w komunikacji ze społecznością i pozyskiwanie nowych kontaktów");
            French.Add("creditsSilvers", TranslationNeeded);
            Spanish.Add("creditsSilvers", "Ayuda con la comunicación con la comunidad y en redes sociales");
            Russian.Add("creditsSilvers", "за помощь в работе с сообществом");

            //Component: creditsXantier
            //
            English.Add("creditsXantier", "Initial IT support and setting up our server");
            German.Add("creditsXantier", "Erster IT-Support und Einrichtung unseres Servers");
            Polish.Add("creditsXantier", "Za początkowe wsparcie informatyczne oraz konfigurację naszego serwera");
            French.Add("creditsXantier", TranslationNeeded);
            Spanish.Add("creditsXantier", "Soporte inicial de IT y preparación el servidor");
            Russian.Add("creditsXantier", "за помощь в настройке сервера в первые дни");

            //Component: creditsSpritePacker
            //
            English.Add("creditsSpritePacker", "Developing the sprite sheet packer algorithm and porting to .NET");
            German.Add("creditsSpritePacker", "Entwicklung des Sprite Sheet Packer-Algorithmus und Portierung nach .NET");
            Polish.Add("creditsSpritePacker", "Za stworzenie algorytmu kompresji sprite'ów i konwersję na platformę .NET");
            French.Add("creditsSpritePacker", TranslationNeeded);
            Spanish.Add("creditsSpritePacker", "Desarrollo del algoritmo de empaquetado de hojas de sprites y portado a .NET");
            Russian.Add("creditsSpritePacker", "за разработку алгоритма упаковки спрайтов и портирование на .NET");

            //Component: creditsWargaming
            //
            English.Add("creditsWargaming", "Making an easy to automate modding system");
            German.Add("creditsWargaming", "Ein einfach zu automatisierendes Modding-System");
            Polish.Add("creditsWargaming", "Za stworzenie łatwego w automatyzacji systemu modów");
            French.Add("creditsWargaming", TranslationNeeded);
            Spanish.Add("creditsWargaming", "Crear un método sencillo para automatizar el sistema de mods");
            Russian.Add("creditsWargaming", "за создание легко автоматизируемой системы модификаций");

            //Component: creditsUsersLikeU
            // Exlamation mark at the end is hard-coded. @Nullmaruzero
            English.Add("creditsUsersLikeU", "Users like you");
            German.Add("creditsUsersLikeU", "Benutzer wie du");
            Polish.Add("creditsUsersLikeU", "Dla użytkowników takich jak Ty");
            French.Add("creditsUsersLikeU", TranslationNeeded);
            Spanish.Add("creditsUsersLikeU", "Usuarios como usted");
            Russian.Add("creditsUsersLikeU", "Таким пользователям, как вы");
            #endregion

            //apply the bool that all translations were applied
            Logging.Debug(LogOptions.MethodName, "All translations loaded");
            TranslationsLoaded = true;
        }
        #endregion

    }
}
