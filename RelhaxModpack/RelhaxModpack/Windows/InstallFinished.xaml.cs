using Microsoft.Win32;
using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for InstallFinished.xaml
    /// </summary>
    public partial class InstallFinished : RelhaxWindow
    {
        /// <summary>
        /// Create an instance of the InstallFinished window
        /// </summary>
        public InstallFinished()
        {
            InitializeComponent();
        }

        private void InstallationCompleteStartWoTButton_Click(object sender, RoutedEventArgs e)
        {
            if(!CommonUtils.StartProcess(new ProcessStartInfo()
            {
                WorkingDirectory = Settings.WoTDirectory,
                FileName = Path.Combine(Settings.WoTDirectory, "WorldOfTanks.exe")
            }))
            {
                MessageBox.Show(Translations.GetTranslatedString("CouldNotStartProcess"));
            }
            DialogResult = true;
            Close();
        }

        private void InstallationCompleteStartGameCenterButton_Click(object sender, RoutedEventArgs e)
        {
            string actualLocation = RegistryUtils.AutoFindWgcDirectory();
            if(string.IsNullOrEmpty(actualLocation) || !CommonUtils.StartProcess(actualLocation))
            {
                Logging.Error("could not start wgc process using command line '{0}'",actualLocation);
                MessageBox.Show(Translations.GetTranslatedString("CouldNotStartProcess"));
            }
            else
            {
                Logging.Debug("wgc successfully started!");
            }
            DialogResult = true;
            Close();
        }

        private void InstallationCompleteOpenXVMButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CommonUtils.StartProcess(string.Format("https://www.modxvm.com/{0}/", Translations.GetTranslatedString("xvmUrlLocalisation"))))
            {
                MessageBox.Show(Translations.GetTranslatedString("CouldNotStartProcess"));
            }
            DialogResult = true;
            Close();
        }

        private void InstallationCompleteCloseAppButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            Application.Current.Shutdown();
        }

        private void InstallationCompleteCloseThisWindowButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
