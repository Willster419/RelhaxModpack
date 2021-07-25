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
using RelhaxModpack.Settings;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all localization for the application User Interface
    /// </summary>
    public static partial class Translations
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
            //components of RelhaxInstallTaskReporter that should not get translated
            "TaskName",
            "TaskStatus",
            //the name of the main relhax window
            "MainWindow"
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

        /// <summary>
        /// The Currently set language of the Translations class to use for localizing windows and phrases
        /// </summary>
        public static Languages CurrentLanguageEnum { get; private set; } = Languages.English;

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
        /// Sets the language dictionary to use when returning a localized string, and set it in the modpack settings class
        /// </summary>
        /// <param name="language">The language to switch to</param>
        public static void SetLanguage(Languages language)
        {
            switch(language)
            {
                case Languages.English:
                    CurrentLanguage = English;
                    CurrentLanguageEnum = Languages.English;
                    break;
                case Languages.French:
                    CurrentLanguage = French;
                    CurrentLanguageEnum = Languages.French;
                    break;
                case Languages.German:
                    CurrentLanguage = German;
                    CurrentLanguageEnum = Languages.German;
                    break;
                case Languages.Polish:
                    CurrentLanguage = Polish;
                    CurrentLanguageEnum = Languages.Polish;
                    break;
                case Languages.Spanish:
                    CurrentLanguage = Spanish;
                    CurrentLanguageEnum = Languages.Spanish;
                    break;
                case Languages.Russian:
                    CurrentLanguage = Russian;
                    CurrentLanguageEnum = Languages.Russian;
                    break;
            }
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
        /// Get the native language name of the english enumerated name of the currently set language in the Translations class
        /// </summary>
        /// <returns>The name of the requested language in it's native language</returns>
        public static string GetLanguageNativeName()
        {
            return GetLanguageNativeName(CurrentLanguageEnum);
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
                        componentName, CurrentLanguageEnum.ToString()), Logfiles.Application, LogLevel.Error);
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
                        componentName, CurrentLanguageEnum.ToString()), Logfiles.Application, LogLevel.Error);
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
        /// <returns>True if it exists in the currently selected language, or false otherwise</returns>
        public static bool ExistsInCurrentLanguage(string componentName)
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

            return Exists(componentName, CurrentLanguageEnum);
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
        public static void LocalizeWindow(RelhaxWindow window, bool applyToolTips)
        {
            //apply window title
            string typeName = window.GetType().Name;
            if (ExistsInCurrentLanguage(typeName))
                window.Title = GetTranslatedString(typeName);

            //Get a list of all visual class controls curently presend and loaded in the window
            List<FrameworkElement> allWindowControls = UiUtils.GetAllWindowComponentsVisual(window, false);
            foreach (FrameworkElement v in allWindowControls)
            {
                TranslateComponent(v, applyToolTips);
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
                        if (ExistsInCurrentLanguage(headeredContentControl.Name + "Description"))
                            headeredContentControl.ToolTip = GetTranslatedString(headeredContentControl.Name + "Description");
                    }
                }
                //RelhaxHyperlink has text stored at the child textbox
                else if (control is UI.RelhaxHyperlink link)
                {
                    link.Text = GetTranslatedString(componentName);
                    if (applyToolTips)
                    {
                        if (ExistsInCurrentLanguage(componentName + "Description"))
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
                        if (ExistsInCurrentLanguage(contentControl.Name + "Description"))
                            contentControl.ToolTip = GetTranslatedString(contentControl.Name + "Description");
                    }
                }
                //textbox only has string text as input
                else if (control is TextBox textBox)
                {
                    textBox.Text = GetTranslatedString(textBox.Name);
                    if (applyToolTips)
                    {
                        if (ExistsInCurrentLanguage(textBox.Name + "Description"))
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
                    if (ExistsInCurrentLanguage(textBlock.Name + "Description"))
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
        public static void LoadTranslations(bool writeToLog = true)
        {
            if (writeToLog)
                Logging.Debug(LogOptions.MethodName, "Loading all translations");

            if(TranslationsLoaded)
            {
                if (writeToLog)
                    Logging.Warning(LogOptions.MethodName, "Translations already loaded, use ReloadTranslations()");
                return;
            }

            LoadTranslationsEnglish();
            LoadTranslationsFrench();
            LoadTranslationsGerman();
            LoadTranslationsPolish();
            LoadTranslationsRussian();
            LoadTranslationsSpanish();

            //apply the bool that all translations were applied
            if (writeToLog)
                Logging.Debug(LogOptions.MethodName, "All translations loaded");
            TranslationsLoaded = true;
        }
        #endregion

    }
}
