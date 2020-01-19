﻿using System;
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
        //list of files who need to be seperated by 32bit and 64bit versions
        private string[] specialName32And64Versions = new string[]
        {
            Settings.PythonLog,
            Settings.XvmLog,
            Settings.PmodLog
        };

        /// <summary>
        /// Create an instance of the Diagnostics window
        /// </summary>
        public Diagnostics()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //check to make sure a selected tanks installation is selected
            ToggleInstallLocationNeededButtons();
        }

        private void ToggleInstallLocationNeededButtons()
        {
            if (string.IsNullOrWhiteSpace(Settings.WoTDirectory))
            {
                CollectLogInfoButton.IsEnabled = false;
                //DownloadWGPatchFilesText.IsEnabled = false;
                CleanupModFilesButton.IsEnabled = false;
                SelectedInstallation.Text = string.Format("{0}\n{1}",
                    Translations.GetTranslatedString("SelectedInstallation"), Translations.GetTranslatedString("SelectedInstallationNone"));
            }
            else
            {
                CollectLogInfoButton.IsEnabled = true;
                //DownloadWGPatchFilesText.IsEnabled = true;
                CleanupModFilesButton.IsEnabled = true;
                SelectedInstallation.Text = string.Format("{0}\n{1}", Translations.GetTranslatedString("SelectedInstallation"), Settings.WoTDirectory);
            }
        }

        private void ChangeInstall_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info("Diagnostics: Selecting WoT install");
            //show a standard WoT selection window from manual find WoT.exe
            OpenFileDialog manualWoTFind = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "WorldOfTanks.exe|WorldOfTanks.exe",
                Title = Translations.GetTranslatedString("selectWOTExecutable"),
                Multiselect = false,
                ValidateNames = true
            };
            if ((bool)manualWoTFind.ShowDialog())
            {
                Settings.WoTDirectory = Path.GetDirectoryName(manualWoTFind.FileName);
                Settings.WoTDirectory = Settings.WoTDirectory.Replace(Settings.WoT32bitFolderWithSlash, string.Empty).Replace(Settings.WoT64bitFolderWithSlash, string.Empty);
                Logging.Info("Diagnostics: Selected WoT install -> {0}",Settings.WoTDirectory);
            }
            else
            {
                Logging.Info("Diagnostics: User canceled selection");
            }

            //check to make sure a selected tanks installation is selected
            ToggleInstallLocationNeededButtons();
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
                //relhax files in relhax dir (relhax.log, relhaxsettings.xml, lastinstalledconfig.xml)
                Settings.RelhaxLogFilepath,
                Settings.RelhaxSettingsFilepath,
                Settings.LastInstalledConfigFilepath,
                //relhax files in wot/logs dir (need to be combined here cause it can change from installation)
                Path.Combine(Settings.WoTDirectory, Settings.LogsFolder, Logging.InstallLogFilename),
                Path.Combine(Settings.WoTDirectory, Settings.LogsFolder, Logging.UninstallLogFilename),
                //disabled for now, but in case WG decides to change it again...
                /*
                //wot files in wot/32bit folder
                Path.Combine(Settings.WoTDirectory, Settings.WoT32bitFolder, Settings.PythonLog),
                Path.Combine(Settings.WoTDirectory, Settings.WoT32bitFolder, Settings.XvmLog),
                Path.Combine(Settings.WoTDirectory, Settings.WoT32bitFolder, Settings.PmodLog),
                //wot files in wot/64bit folder
                Path.Combine(Settings.WoTDirectory, Settings.WoT64bitFolder, Settings.PythonLog),
                Path.Combine(Settings.WoTDirectory, Settings.WoT64bitFolder, Settings.XvmLog),
                Path.Combine(Settings.WoTDirectory, Settings.WoT64bitFolder, Settings.PmodLog),
                */
                //wot files in wot folder
                Path.Combine(Settings.WoTDirectory, Settings.PythonLog),
                Path.Combine(Settings.WoTDirectory, Settings.XvmLog),
                Path.Combine(Settings.WoTDirectory, Settings.PmodLog)
            };

            //use a nice diagnostic window to check if the user wants to include any other files
            AddPicturesZip apz = new AddPicturesZip() { FilesToAddalways = filesToCollect };

            //add the already above collected files to the list
            foreach (string file in filesToCollect)
                if(File.Exists(file))
                    apz.FilesToAddList.Items.Add(file);

            if(!(bool)apz.ShowDialog())
            {
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("canceled");
                return;
            }

            //add any new files the user added to the list, while not adding any of the above pre-added ones
            foreach (string s in apz.FilesToAddList.Items)
                if (!filesToCollect.Contains(s))
                    filesToCollect.Add(s);

            //check in the list to make sure that the entries are valid and paths exist
            Logging.Info("Filtering list of files to collect by if file exists");
            filesToCollect = filesToCollect.Where(fileEntry => !string.IsNullOrWhiteSpace(fileEntry) && File.Exists(fileEntry)).ToList();
            try
            {
                Logging.Info("creating diagnostic zip file and adding logs");
                using (ZipFile zip = new ZipFile())
                {
                    foreach (string s in filesToCollect)
                    {
                        int duplicate = 0;

                        //get just the filename (it will be added to the zip file as just the name)
                        string fileNameToAdd = Path.GetFileName(s);

                        //special case check for if filenames are the same in 32bit and 64bit
                        //disabled for now, but in case WG decides to change it again...
                        /*
                        if(specialName32And64Versions.Contains(fileNameToAdd))
                        {
                            string[] folderPathSep = s.Split(Path.DirectorySeparatorChar);
                            string folderName32And64 = folderPathSep[folderPathSep.Count() - 2];
                            fileNameToAdd = string.Format("{0}_{1}", folderName32And64, fileNameToAdd);
                        }
                        */

                        //run a loop to check if the file already exists in the zip with the same name, if it does then pad it until it does not
                        Logging.Info("Attempting to add filename {0} in zip entry", fileNameToAdd);
                        while (zip.ContainsEntry(fileNameToAdd))
                        {
                            fileNameToAdd = string.Format("{0}_{1}.{2}", Path.GetFileNameWithoutExtension(fileNameToAdd), duplicate++, Path.GetExtension(fileNameToAdd));
                            Logging.Info("exists, using filename {0}", fileNameToAdd);
                        }
                        Logging.Debug("new name for zip: {0}", fileNameToAdd);

                        //and the file to the zip file and grab the entry reference
                        ZipEntry entry = zip.AddFile(s);
                        //then use it to modify the name of the entry in the zip file
                        //this moves it out of all the sub-folders up to the root directory
                        entry.FileName = Path.GetFileName(fileNameToAdd);
                        Logging.Info("file {0} added, entry name in zip = {1}", fileNameToAdd, entry.FileName);
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

        private async void ClearDownloadCache_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info("Diagnostics: Deleting download cache");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("clearingDownloadCache");
            try
            {
                await Utils.DirectoryDeleteAsync(Settings.RelhaxDownloadsFolderPath, false, false, 3, 100, "*.zip");
                await Utils.DirectoryDeleteAsync(Settings.RelhaxDownloadsFolderPath, false, false, 3, 100, "*.xml");
            }
            catch (IOException ioex)
            {
                DiagnosticsStatusTextBox.Text = string.Format("{0}{1}{2}", Translations.GetTranslatedString("failedToClearDownloadCache"), Environment.NewLine, Settings.RelhaxDownloadsFolderPath);
                Logging.Exception(ioex.ToString());
            }
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleaningDownloadCacheComplete");
            Logging.Info("Diagnostics: Deleted download cache");
        }

        private async void ClearDownloadCacheDatabase_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info("Diagnostics: Deleting database cache file");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("clearingDownloadCacheDatabase");
            try
            {
                await Utils.DirectoryDeleteAsync(Settings.RelhaxDownloadsFolderPath, false, false, 3, 100, "*.xml");
            }
            catch (IOException ioex)
            {
                DiagnosticsStatusTextBox.Text = string.Format("{0}{1}{2}", Translations.GetTranslatedString("failedToClearDownloadCacheDatabase"), Environment.NewLine, Settings.RelhaxDownloadsFolderPath);
                Logging.Exception(ioex.ToString());
            }
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleaningDownloadCacheDatabaseComplete");
            Logging.Info("Diagnostics: Deleted database cache file");
        }

        private void TestLoadImageLibrariesButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info("Diagnostics: Test load image libraries");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("loadingAtlasImageLibraries");
            Utils.AllowUIToUpdate();
            if (Utils.TestLoadAtlasLibraries(true))
            {
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("loadingAtlasImageLibrariesSuccess");
                Logging.Info("Diagnostics: Test load image libraries pass");
            }
            else
            {
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("loadingAtlasImageLibrariesFail");
                if (MessageBox.Show(string.Format("{0}\n{1}", Translations.GetTranslatedString("missingMSVCPLibraries"), Translations.GetTranslatedString("openLinkToMSVCP")),
                                Translations.GetTranslatedString("missingMSVCPLibrariesHeader"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (!Utils.StartProcess(Utils.MSVCPLink))
                    {
                        Logging.Error("failed to open url to MSVCP: {0}", Utils.MSVCPLink);
                    }
                }
                Logging.Info("Diagnostics: Test load image libraries fail");
            }
        }

        private void DownloadWGPatchFiles_Click(object sender, RoutedEventArgs e)
        {
            GameCenterUpdateDownloader gameCenterUpdateDownloader = new GameCenterUpdateDownloader()
            {
                SelectedClient = string.IsNullOrWhiteSpace(Settings.WoTDirectory) ? string.Empty : Settings.WoTDirectory
            };
            gameCenterUpdateDownloader.ShowDialog();
        }

        private async void CleanupModFilesButton_Click(object sender, RoutedEventArgs e)
        {
            string[] locationsToCheck = new string[]
            {
                Path.Combine(Settings.WoTDirectory, Settings.WoT64bitFolder, Settings.ModsDir),
                Path.Combine(Settings.WoTDirectory, Settings.WoT64bitFolder, Settings.ResModsDir),
                Path.Combine(Settings.WoTDirectory, Settings.WoT32bitFolder, Settings.ModsDir),
                Path.Combine(Settings.WoTDirectory, Settings.WoT32bitFolder, Settings.ResModsDir)
            };

            List<string> filesToDelete = new List<string>();
            foreach(string folderPath in locationsToCheck)
            {
                Logging.Debug("Processing folder {0}", folderPath);
                if(!Directory.Exists(folderPath))
                {
                    Logging.Debug("Directory does not exist");
                    continue;
                }
                int count = 0;
                string[] files = Utils.DirectorySearch(folderPath, SearchOption.AllDirectories, true);
                if (files != null)
                    count = files.Count();
                Logging.Debug("Added {0} files", count);
                filesToDelete.AddRange(files);
            }

            Logging.Debug("Deleting files");
            for(int i = 0; i < filesToDelete.Count; i++)
            {
                //check to make sure it's a file
                if (!File.Exists(filesToDelete[i]))
                    continue;

                DiagnosticsStatusTextBox.Text = string.Format("{0} {1} {2} {3}",
                    Translations.GetTranslatedString("deletingFile"), (i + 1), Translations.GetTranslatedString("of"), filesToDelete.Count);

                await Task.Run(() => Utils.FileDelete(filesToDelete[i]));
            }

            //fully delete the folders now
            Logging.Debug("Complete delete of folders");
            List<string> locationsFailedToDelete = new List<string>();
            foreach (string folderPath in locationsToCheck)
            {
                if(Directory.Exists(folderPath))
                    if (!Utils.DirectoryDelete(folderPath, true))
                        locationsFailedToDelete.Add(folderPath);
            }

            if(locationsFailedToDelete.Count > 0)
            {
                Logging.Error("Failed to delete the folders: {0}", string.Join(",", locationsFailedToDelete));
                MessageBox.Show(string.Format("{0}, {1}", Translations.GetTranslatedString("folderDeleteFailed"), string.Join(",\n", locationsFailedToDelete)));
            }

            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleanupModFilesCompleted");
        }
    }
}
