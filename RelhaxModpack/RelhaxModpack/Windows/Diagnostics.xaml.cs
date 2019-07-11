using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Ionic.Zip;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for Diagnostics.xaml
    /// </summary>
    public partial class Diagnostics : RelhaxWindow
    {
        public Diagnostics()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //check to make sure a selected tanks installation is selected
            if (string.IsNullOrWhiteSpace(Settings.WoTDirectory))
                ToggleUIOptions(false);
            else
                ToggleUIOptions(true);

            //set the currently selected installation text
            SelectedInstallation.Text = string.Format("{0}\n{1}", Translations.GetTranslatedString("SelectedInstallation"), Translations.GetTranslatedString("SelectedInstallationNone"));
        }

        private void ChangeInstall_Click(object sender, RoutedEventArgs e)
        {
            //show a standard WoT selection window from manual fine WoT.exe
            OpenFileDialog manualWoTFind = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "WorldOfTanks.exe|WorldOfTanks.exe",
                Multiselect = false,
                ValidateNames = true
            };
            if ((bool)manualWoTFind.ShowDialog())
            {
                Settings.WoTDirectory = Path.GetDirectoryName(manualWoTFind.FileName);
                SelectedInstallation.Text = string.Format("{0}\n{1}", Translations.GetTranslatedString("SelectedInstallation"), Settings.WoTDirectory);
            }
            else
            {
                Logging.Info("User Canceled selection");
            }

            //check to make sure a selected tanks installation is selected
            if (string.IsNullOrWhiteSpace(Settings.WoTDirectory))
                ToggleUIOptions(false);
            else
                ToggleUIOptions(true);
        }

        private void LaunchWoTLauncher_Click(object sender, RoutedEventArgs e)
        {
            //just to make sure
            if (string.IsNullOrWhiteSpace(Settings.WoTDirectory))
                return;

            Logging.Debug("Starting WoTLauncher with argument \"-integrity_default_client\"");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("startingLauncherRepairMode");
            string filename = Path.Combine(Settings.WoTDirectory, "WoTLauncher.exe");
            string formattedArguement = "-integrity_default_client";
            Logging.Info("Complete command: {0} {1}", filename, formattedArguement);
            try
            {
                Process.Start(filename, formattedArguement);
            }
            catch (Exception ex)
            {
                Logging.Exception("LaunchWoTLauncher_Click", ex);
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("failedStartLauncherRepairMode");
                return;
            }
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("launcherRepairModeStarted");
        }

        private void CollectLogInfo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.WoTDirectory))
                return;

            //setup UI
            Logging.Info("started collection of log files");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("collectionLogInfo");

            //create the list of files to collect (should always collect these)
            List<string> filesToCollect = new List<string>()
            {
                //stuff in application startup path
                Settings.RelhaxLogFilepath,
                Settings.RelhaxSettingsFilepath,
                Settings.LastInstalledConfigFilepath,
                //stuff in the tanks location (need to be combined here cause it can change from installation)
                Path.Combine(Settings.WoTDirectory,"logs",Logging.InstallLogFilename),
                Path.Combine(Settings.WoTDirectory, "logs", Logging.UninstallLogFilename),
                Path.Combine(Settings.WoTDirectory, "python.log"),
                Path.Combine(Settings.WoTDirectory, "xvm.log"),
                Path.Combine(Settings.WoTDirectory, "pmod.log")
            };

            //use a nice diagnostic window to check if the user wants to include any other files
            AddPicturesZip apz = new AddPicturesZip() { filesToAddalways = filesToCollect };

            //add the already above collected files to the list
            foreach (string file in filesToCollect)
                if(File.Exists(file))
                    apz.FilesToAddList.Items.Add(file);

            if((bool)apz.ShowDialog())
            {
                foreach (string s in apz.FilesToAddList.Items)
                    if(!filesToCollect.Contains(s))
                        filesToCollect.Add(s);
            }
            apz = null;

            //check in the list to make sure that the entries are valid and paths exist
            Logging.Debug("Filtering list of files to collect");
            filesToCollect = filesToCollect.Where(fileEntry => !string.IsNullOrWhiteSpace(fileEntry) && File.Exists(fileEntry)).ToList();
            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    foreach (string s in filesToCollect)
                    {
                        int duplicate = 0;

                        //get just the filename (it will be added to the zip file as just the name)
                        string fileNameToAdd = Path.GetFileName(s);

                        //run a loop to check if the file already exists in the zip with the same name, if it does then pad it until it does not
                        Logging.Debug("Attempting to add filename {0} in zip entry", fileNameToAdd);
                        while (zip.ContainsEntry(fileNameToAdd))
                        {
                            fileNameToAdd = string.Format("{0}_{1}.{2}", Path.GetFileNameWithoutExtension(fileNameToAdd), duplicate++, Path.GetExtension(fileNameToAdd));
                            Logging.Debug("exists, using filename {0}", fileNameToAdd);
                        }

                        //after padding, put the full path back together
                        fileNameToAdd = Path.Combine(Path.GetDirectoryName(s), fileNameToAdd);

                        //one last check to make sure it exists
                        if(!File.Exists(fileNameToAdd))
                        {
                            Logging.Error("the file {0} was parsed to exist but after loop modification, it does not!", fileNameToAdd);
                            continue;
                        }
                        else
                        {
                            //and the file to the zip file and grab the entry refrence
                            ZipEntry entry = zip.AddFile(fileNameToAdd);
                            //then use it to modify the name of the entry in the zip file
                            entry.FileName = Path.GetFileName(fileNameToAdd);
                            Logging.Info("file {0} added, entry name in zip = {1}", fileNameToAdd, entry.FileName);
                        }
                    }
                    string zipSavePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                        string.Format("RelhaxModpackLogs_{0}.zip", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                    zip.Save(zipSavePath);
                    Logging.Info("zip file saved to {0}", zipSavePath);
                    DiagnosticsStatusTextBox.Text = string.Format("zip file saved to {0}", zipSavePath);
                }
            }
            catch(ZipException zex)
            {
                Logging.Exception(zex.ToString());
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("failedCreateZipfile");
            }
        }

        private void ToggleUIOptions(bool toggle)
        {
            LaunchWoTLauncher.IsEnabled = toggle;
            CollectLogInfo.IsEnabled = toggle;
        }

        private async void ClearDownloadCache_Click(object sender, RoutedEventArgs e)
        {
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("clearingDownloadCache");
            try
            {
                await Utils.DirectoryDeleteAsync(Settings.RelhaxDownloadsFolder, false, false, 3, 100, "*.zip");
                await Utils.DirectoryDeleteAsync(Settings.RelhaxDownloadsFolder, false, false, 3, 100, "*.xml");
            }
            catch (IOException ioex)
            {
                DiagnosticsStatusTextBox.Text = string.Format("{0}{1}{2}", Translations.GetTranslatedString("failedToClearDownloadCache"), Environment.NewLine, Settings.RelhaxDownloadsFolder);
            }
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleaningDownloadCacheComplete");
        }

        private async void ClearDownloadCacheDatabase_Click(object sender, RoutedEventArgs e)
        {
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("clearingDownloadCacheDatabase ");
            try
            {
                await Utils.DirectoryDeleteAsync(Settings.RelhaxDownloadsFolder, false, false, 3, 100, "*.xml");
            }
            catch (IOException ioex)
            {
                DiagnosticsStatusTextBox.Text = string.Format("{0}{1}{2}", Translations.GetTranslatedString("failedToClearDownloadCache"), Environment.NewLine, Settings.RelhaxDownloadsFolder);
            }
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleaningDownloadCacheComplete");
        }
    }
}
