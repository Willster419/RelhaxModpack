using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

using RelhaxModpack.Windows;
using RelhaxModpack.Atlases;
using RelhaxModpack.Database;
using RelhaxModpack.Common;
using RelhaxModpack.Settings;
using RelhaxModpack.Xml;
using RelhaxModpack.UI;
using RelhaxModpack.UI.Extensions;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Utilities.ClassEventArgs;

using MaterialDesignThemes.Wpf;

namespace RelhaxModpack
{
    /// <summary>
    /// Interaction logic for ModpackMainWindow_NEW.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public partial class ModpackMainWindow_NEW : Window
    {
        public Snackbar = new Snackbar();
        /// <summary>
        /// Does the stuff like __init__(self): in python
        /// </summary>
        public ModpackMainWindow_NEW()
        {
            InitializeComponent();

            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            //disconnect event handler before translation combobox is modified
            ModpackMainWindow_Home_LanguageSelection_ListBox.SelectionChanged -= OnLanguageSelectionChanged;

            //load the supported translations into combobox
            ModpackMainWindow_Home_LanguageSelection_ListBox.Items.Clear();
            ModpackMainWindow_Home_LanguageSelection_ListBox.Items.Add(Translations.LanguageEnglish);
            ModpackMainWindow_Home_LanguageSelection_ListBox.Items.Add(Translations.LanguageFrench);
            ModpackMainWindow_Home_LanguageSelection_ListBox.Items.Add(Translations.LanguageGerman);
            ModpackMainWindow_Home_LanguageSelection_ListBox.Items.Add(Translations.LanguagePolish);
            ModpackMainWindow_Home_LanguageSelection_ListBox.Items.Add(Translations.LanguageRussian);
            ModpackMainWindow_Home_LanguageSelection_ListBox.Items.Add(Translations.LanguageSpanish);

            //load translation hashes and set default language
            Translations.LoadTranslations();
            Translations.SetLanguage(Languages.English);
            ModpackMainWindow_Home_LanguageSelection_ListBox.SelectedIndex = 0;

            //reconnect event handler
            ModpackMainWindow_Home_LanguageSelection_ListBox.SelectionChanged += OnLanguageSelectionChanged;

            //DarkModeToggleButton.IsChecked = theme.GetBaseTheme() == BaseTheme.Dark;


            // Wilster: This is needed to swap themes in MD. Can it be backported to the language version we are using?
            //if (paletteHelper.GetThemeManager() is { } themeManager)
            //{
            //    themeManager.ThemeChanged += (_, e)
            //        => DarkModeToggleButton.IsChecked = e.NewTheme?.GetBaseTheme() == BaseTheme.Dark;
            //}
        }

        #region Buttons

        private void ModpackMainWindow_MainTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem switchToTab = null;
            TabItem switchFromTab = null;

            if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
                switchToTab = e.AddedItems[0] as TabItem;

            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] != null)
                switchFromTab = e.RemovedItems[0] as TabItem;

            if (switchToTab == null || switchFromTab == null)
                return;
            //When switching tab from the settings tab, save all settings
            if (switchFromTab == TabItemSettings && switchToTab != TabItemSettings)
                InvokeSettingsAndnSaveToSettingsFile();
        }

        private void DarkModeToggleButton_Click(object sender, bool isDarkTheme, RoutedEventArgs e)
        {
            //ModifyTheme(DarkModeToggleButton.IsChecked == true);
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            var DarkModeEnabled = theme.GetBaseTheme() == BaseTheme.Dark;

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
        }

        #endregion

        private static void ModifyTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }

        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //the event should not fire if it's loading. loading boolean set in window loading event takes care of this
            if (loading)
                return;

            switch (ModpackMainWindow_Home_LanguageSelection_ListBox.SelectedItem as string)
            {
                case Translations.LanguageEnglish:
                    Translations.SetLanguage(Languages.English);
                    break;
                case Translations.LanguageFrench:
                    Translations.SetLanguage(Languages.French);
                    break;
                case Translations.LanguageGerman:
                    Translations.SetLanguage(Languages.German);
                    break;
                case Translations.LanguagePolish:
                    Translations.SetLanguage(Languages.Polish);
                    break;
                case Translations.LanguageRussian:
                    Translations.SetLanguage(Languages.Russian);
                    break;
                case Translations.LanguageSpanish:
                    Translations.SetLanguage(Languages.Spanish);
                    break;
            }

            Translations.LocalizeWindow(this, true);
            ApplyCustomUILocalizations();
        }

        private async void ModpackMainpage_UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            //parse WoT root directory
            Logging.WriteToLog("Started looking for WoT root directory", Logfiles.Application, LogLevel.Debug);
            string autoSearchResult = string.Empty;

            //only run the code if the user wants to auto find the WoT directory (which is default)
            if (!ModpackSettings.ForceManuel)
            {
                autoSearchResult = RegistryUtils.AutoFindWoTDirectoryFirst();
            }

            if (string.IsNullOrEmpty(autoSearchResult) || ModpackSettings.ForceManuel)
            {
                Logging.WriteToLog("Auto detect failed or user requests manual", Logfiles.Application, LogLevel.Debug);

                WoTClientSelection clientSelection = new WoTClientSelection(ModpackSettings);

                if ((bool)clientSelection.ShowDialog())
                {
                    autoSearchResult = clientSelection.SelectedPath;
                    autoSearchResult = autoSearchResult.Replace(ApplicationConstants.WoT32bitFolderWithSlash, string.Empty).Replace(ApplicationConstants.WoT64bitFolderWithSlash, string.Empty);
                    Logging.Info(LogOptions.ClassName, "Selected WoT install: {0}", autoSearchResult);
                }
                else
                {
                    Logging.Info("User Canceled installation");
                    ToggleUIButtons(true);
                    return;
                }
            }
        }
    }
}
