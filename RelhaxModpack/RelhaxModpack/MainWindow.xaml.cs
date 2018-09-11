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
            //create the tray icons and menus

            //load application settings
            Logging.WriteToLog("Init and load settings");
            Settings.InitSettings();
            //check for updates

            //check for minimizing

            //check for conflicting settings and command line argeuemts

            //dispose of it here

            //TODO: save all UI components to a hashtable?
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

        private void EnableBordersCB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnMulticoreExtractionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnCreateShortcutsChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnSaveUserDataChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnClearWoTCacheChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnClearLogFilesChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnCleanInstallChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnImmidateExtarctionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ShowInstallCompleteWindowCB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnShowInstallCompleteWindowChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnBackupModsChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnPreviewLoadingImageChange(object sender, RoutedEventArgs e)
        {

        }

        private void OnForceManuelGameDetectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnInformIfNoNewDatabaseChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnSaveLastInstallChanged(object sender, RoutedEventArgs e)
        {

        }

        private void OnUseBetaAppChanged(object sender, RoutedEventArgs e)
        {

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
    }
}
