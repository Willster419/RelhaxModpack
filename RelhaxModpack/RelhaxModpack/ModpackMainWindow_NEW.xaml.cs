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
        /// <summary>
        /// Does the stuff like __init__(self): in python
        /// </summary>
        public ModpackMainWindow_NEW()
        {
            InitializeComponent();
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
            //if (switchFromTab == ModpackMainWindow_Settings && switchToTab != ModpackMainWindow_Settings)
            //    InvokeSettingsAndnSaveToSettingsFile();
        }

        private void DarkModeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            return;
        //    //ModifyTheme(DarkModeToggleButton.IsChecked == true);
        //    var paletteHelper = new PaletteHelper();
        //    var theme = paletteHelper.GetTheme();
        //    var DarkModeEnabled = theme.GetBaseTheme() == BaseTheme.Dark;
        //
        //    theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
        }

        #endregion

        //private static void ModifyTheme(bool isDarkTheme)
        //{
        //    var paletteHelper = new PaletteHelper();
        //    var theme = paletteHelper.GetTheme();

        //    theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
        //    paletteHelper.SetTheme(theme);
        //}

        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            return;
            //    //the event should not fire if it's loading. loading boolean set in window loading event takes care of this
            //    if (loading)
            //        return;

            //    switch (ModpackMainWindow_Home_LanguageSelection_ListBox.SelectedItem as string)
            //    {
            //        case Translations.LanguageEnglish:
            //            Translations.SetLanguage(Languages.English);
            //            break;
            //        case Translations.LanguageFrench:
            //            Translations.SetLanguage(Languages.French);
            //            break;
            //        case Translations.LanguageGerman:
            //            Translations.SetLanguage(Languages.German);
            //            break;
            //        case Translations.LanguagePolish:
            //            Translations.SetLanguage(Languages.Polish);
            //            break;
            //        case Translations.LanguageRussian:
            //            Translations.SetLanguage(Languages.Russian);
            //            break;
            //        case Translations.LanguageSpanish:
            //            Translations.SetLanguage(Languages.Spanish);
            //            break;
            //    }

            //    Translations.LocalizeWindow(this, true);
            //    ApplyCustomUILocalizations();
        }

        private void ModpackMainpage_UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            MaterialDesignThemes.Wpf.DialogHost.OpenDialogCommandDataContextSourceProperty(ModpackMainWindow_Uninstall_Dialog);
            //    //parse WoT root directory
            //    Logging.WriteToLog("Started looking for WoT root directory", Logfiles.Application, LogLevel.Debug);
            //    string autoSearchResult = string.Empty;

            //    //only run the code if the user wants to auto find the WoT directory (which is default)
            //    if (!ModpackSettings.ForceManuel)
            //    {
            //        autoSearchResult = RegistryUtils.AutoFindWoTDirectoryFirst();
            //    }

            //    if (string.IsNullOrEmpty(autoSearchResult) || ModpackSettings.ForceManuel)
            //    {
            //        Logging.WriteToLog("Auto detect failed or user requests manual", Logfiles.Application, LogLevel.Debug);

            //        WoTClientSelection clientSelection = new WoTClientSelection(ModpackSettings);

            //        if ((bool)clientSelection.ShowDialog())
            //        {
            //            autoSearchResult = clientSelection.SelectedPath;
            //            autoSearchResult = autoSearchResult.Replace(ApplicationConstants.WoT32bitFolderWithSlash, string.Empty).Replace(ApplicationConstants.WoT64bitFolderWithSlash, string.Empty);
            //            Logging.Info(LogOptions.ClassName, "Selected WoT install: {0}", autoSearchResult);
            //        }
            //        else
            //        {
            //            Logging.Info("User Canceled installation");
            //            ToggleUIButtons(true);
            //            return;
            //        }
            //    }
        }
        private void Modpack_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Close();
        }
        #region Uninstall Events

        private void DialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            eventArgs.Cancel();
        }

        private void OnCleanInstallChanged(object sender, RoutedEventArgs e)
        {
            return;
        }

        #endregion
        #region Get In Touch Card Events

        private void GitHubButton_OnClick(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void TwitterButton_OnClick(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void ChatButton_OnClick(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void EmailButton_OnClick(object sender, RoutedEventArgs e)
        {
            return;
        }
        #endregion
        #region Github Card Events

        private void DonateButton_OnClick(object sender, RoutedEventArgs e)
        {
            return;
        }

        #endregion
    }
}
