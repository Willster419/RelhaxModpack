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
    }
}
