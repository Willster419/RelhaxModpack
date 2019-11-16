using Microsoft.Win32;
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
            if(!Utils.StartProcess(new ProcessStartInfo()
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
            Logging.Debug("searching registry (Software\\Classes\\wgc\\shell\\open\\command) for wgc location");
            //search for the location of the game center from the registry
            RegistryKey wgcKey = Utils.GetRegistryKeys(new RegistrySearch() { Root = Registry.CurrentUser, Searchpath = @"Software\Classes\wgc\shell\open\command" });
            string actualLocation = string.Empty;
            if(wgcKey != null)
            {
                Logging.Debug("not null key, checking results");
                foreach(string valueInKey in wgcKey.GetValueNames())
                {
                    string wgcPath = wgcKey.GetValue(valueInKey) as string;
                    Logging.Debug("parsing result name '{0}' with value '{1}'", valueInKey, wgcPath);
                    if(!string.IsNullOrWhiteSpace(wgcPath) && wgcPath.ToLower().Contains("wgc.exe"))
                    {
                        //trim front
                        wgcPath = wgcPath.Substring(1);
                        //trim end
                        wgcPath = wgcPath.Substring(0, wgcPath.Length - 6);
                        Logging.Debug("parsed to new value of '{0}', checking if file exists");
                        if(File.Exists(wgcPath))
                        {
                            Logging.Debug("exists, use this for wgc start");
                            actualLocation = wgcPath;
                            break;
                        }
                        else
                        {
                            Logging.Debug("not exist, continue to search");
                        }
                    }
                }
            }
            if(string.IsNullOrEmpty(actualLocation) || !Utils.StartProcess(actualLocation))
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
            if (!Utils.StartProcess(string.Format("https://www.modxvm.com/{0}/", Translations.GetTranslatedString("xvmUrlLocalisation"))))
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
