using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace RelhaxModpack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon relhaxIcon;
        /// <summary>
        /// Creates the instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TheMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //load please wait thing

            //load translation hashes and set default language
            Translations.SetLanguage(Languages.English);
            Translations.LoadTranslations();
            //apply translations to this window
            Translations.LocalizeWindow(this,true);
            //add localization translation options to combobox
            Languages[] allLanguages = (Languages[])Enum.GetValues(typeof(Languages));
            foreach (Languages lang in allLanguages)
                LanguagesSelector.Items.Add(lang.ToString());
            //create the tray icons and menus

            //load application settings
            Logging.WriteToLog("Init and load settings");
            Settings.InitSettings();
            //apply settings to UI elements

            //check for updates

            //check for minimizing

            //check for conflicting settings and command line argeuemts

            //dispose of it here
        }

        private void TheMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logging.WriteToLog("Saving settings");
            if (Settings.ModpackSettings.SaveSettings())
                Logging.WriteToLog("Settings saved");
            Logging.WriteToLog("Disposing tray icon");
            if(relhaxIcon != null)
            {
                relhaxIcon.Dispose();
                relhaxIcon = null;
            }
        }

        private void CreateTray()
        {
            //create base tray icon
            relhaxIcon = new NotifyIcon()
            {
                Visible = true,
                Icon = Properties.Resources.modpack_icon,
                Text = Title
            };
            //create menu options

        }

        private void OnSelectionViewChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnMulticoreExtractionChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.MulticoreExtraction = (bool)MulticoreExtractionCB.IsChecked;
        }

        private void OnCreateShortcutsChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.CreateShortcuts = (bool)CreateShortcutsCB.IsChecked;
        }

        private void OnSaveUserDataChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.SaveUserData = (bool)SaveUserDataCB.IsChecked;
        }

        private void OnClearWoTCacheChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.ClearCache = (bool)ClearCacheCB.IsChecked;
        }

        private void OnClearLogFilesChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.DeleteLogs = (bool)ClearLogFilesCB.IsChecked;
        }

        private void OnCleanInstallChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.CleanInstallation = (bool)CleanInstallCB.IsChecked;
        }

        private void OnImmidateExtarctionChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.DownloadInstantExtraction = (bool)InstantExtractionCB.IsChecked;
        }

        private void OnShowInstallCompleteWindowChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.ShowInstallCompleteWindow = (bool)ShowInstallCompleteWindowCB.IsChecked;
        }

        private void OnBackupModsChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.BackupModFolder = (bool)BackupModsCheckBox.IsChecked;
        }

        private void OnPreviewLoadingImageChange(object sender, RoutedEventArgs e)
        {
            if ((bool)ThirdGuardsLoadingImageRB.IsChecked)
                Settings.ModpackSettings.GIF = LoadingGifs.ThirdGuards;
            else if ((bool)StandardImageRB.IsChecked)
                Settings.ModpackSettings.GIF = LoadingGifs.Standard;
        }

        private void OnForceManuelGameDetectionChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.ForceManuel = (bool)ForceManuelGameDetectionCB.IsChecked;
        }

        private void OnInformIfNoNewDatabaseChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.NotifyIfSameDatabase = (bool)NotifyIfSameDatabaseCB.IsChecked;
        }

        private void OnSaveLastInstallChanged(object sender, RoutedEventArgs e)
        {
            Settings.ModpackSettings.SaveLastConfig = (bool)SaveLastInstallCB.IsChecked;
        }

        private void OnUseBetaAppChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)UseBetaApplicationCB.IsChecked)
                Settings.ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Beta;
            else if (!(bool)UseBetaApplicationCB.IsChecked)
                Settings.ModpackSettings.ApplicationDistroVersion = ApplicationVersions.Stable;
        }

        private void OnUseBetaDatabaseChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnDefaultBordersChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnDefaultSelectColorChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnLegacyBordersChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnLegacySelectColorChenged(object sender, RoutedEventArgs e)
        {

        }

        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Translations.SetLanguage((Languages)LanguagesSelector.SelectedIndex);
            Translations.LocalizeWindow(this, true);
        }
    }
}
