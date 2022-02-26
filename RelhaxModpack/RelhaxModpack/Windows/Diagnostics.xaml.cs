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
using RelhaxModpack.Atlases;
using RelhaxModpack.Utilities;
using RelhaxModpack.UI;
using System.Text;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Settings;
using RelhaxModpack.Common;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for Diagnostics.xaml
    /// </summary>
    public partial class Diagnostics : RelhaxWindow
    {
        /// <summary>
        /// The location of the WoT installation directory parsed at installation time
        /// </summary>
        /// <remarks>The path is absolute, ending at "World_of_Tanks"</remarks>
        public string WoTDirectory { get; set; }

        /// <summary>
        /// The number of log file entries that should be kept after the trim operation
        /// </summary>
        private int RelhaxLogfileTrimLength = 3;

        /// <summary>
        /// Create an instance of the Diagnostics window
        /// </summary>
        public Diagnostics(ModpackSettings modpackSettings) : base(modpackSettings)
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
            if (string.IsNullOrWhiteSpace(WoTDirectory))
            {
                Logging.Info("WoTDirectory is empty, add default from registry search");
                WoTDirectory = RegistryUtils.AutoFindWoTDirectoryFirst();
                if (!string.IsNullOrWhiteSpace(WoTDirectory))
                {
                    WoTDirectory = Path.GetDirectoryName(WoTDirectory);
                }
                
                //if it's still empty, then mark is as not happening
                if (string.IsNullOrWhiteSpace(WoTDirectory))
                {
                    CollectLogInfoButton.IsEnabled = false;
                    CleanupModFilesButton.IsEnabled = false;
                    SelectedInstallation.Text = string.Format("{0}\n{1}",
                        Translations.GetTranslatedString("SelectedInstallation"), Translations.GetTranslatedString("SelectedInstallationNone"));
                }
            }
            CollectLogInfoButton.IsEnabled = true;
            CleanupModFilesButton.IsEnabled = true;
            SelectedInstallation.Text = string.Format("{0}\n{1}", Translations.GetTranslatedString("SelectedInstallation"), WoTDirectory);
        }

        private void ChangeInstall_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info(LogOptions.ClassName, "Selecting WoT install, showing WoTClientSelectionWindow");

            WoTClientSelection clientSelection = new WoTClientSelection(ModpackSettings);
            
            if ((bool)clientSelection.ShowDialog())
            {
                WoTDirectory = Path.GetDirectoryName(clientSelection.SelectedPath);
                WoTDirectory = WoTDirectory.Replace(ApplicationConstants.WoT32bitFolderWithSlash, string.Empty).Replace(ApplicationConstants.WoT64bitFolderWithSlash, string.Empty);
                Logging.Info(LogOptions.ClassName, "Selected WoT install: {0}", WoTDirectory);
            }
            else
            {
                Logging.Info(LogOptions.ClassName, "User canceled selection");
            }

            //check to make sure a selected tanks installation is selected
            ToggleInstallLocationNeededButtons();
        }

        private void CollectLogInfo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WoTDirectory))
            {
                Logging.Error("WoTDirectory is empty, cannon collect logs");
                return;
            }

            //setup UI
            Logging.Info(LogOptions.ClassName, "Started collection of log files");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("collectionLogInfo");

            //create the list of files to collect (should always collect these)
            List<string> filesToCollect = new List<string>()
            {
                //relhax files in relhax dir (relhax.log, relhaxsettings.xml, lastinstalledconfig.xml)
                ApplicationConstants.RelhaxLogFilepath,
                ApplicationConstants.RelhaxSettingsFilepath,
                ApplicationConstants.LastInstalledConfigFilepath,
                //relhax files in wot/logs dir (need to be combined here cause it can change from installation)
                Path.Combine(WoTDirectory, ApplicationConstants.LogsFolder, Logging.InstallLogFilename),
                Path.Combine(WoTDirectory, ApplicationConstants.LogsFolder, Logging.UninstallLogFilename),
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
                Path.Combine(WoTDirectory, ApplicationConstants.PythonLog),
                Path.Combine(WoTDirectory, ApplicationConstants.XvmLog),
                Path.Combine(WoTDirectory, ApplicationConstants.PmodLog)
            };

            //use a nice diagnostic window to check if the user wants to include any other files
            AddPicturesZip apz = new AddPicturesZip(this.ModpackSettings) { FilesToAddalways = filesToCollect };

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
            Logging.Info(LogOptions.ClassName, "Filtering list of files to collect by if file exists");
            filesToCollect = filesToCollect.Where(fileEntry => !string.IsNullOrWhiteSpace(fileEntry) && File.Exists(fileEntry)).ToList();
            try
            {
                Logging.Info(LogOptions.ClassName, "Creating diagnostic zip file and adding logs");
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
                        Logging.Info(LogOptions.ClassName, "Attempting to add filename {0} in zip entry", fileNameToAdd);
                        while (zip.ContainsEntry(fileNameToAdd))
                        {
                            fileNameToAdd = string.Format("{0}_{1}.{2}", Path.GetFileNameWithoutExtension(fileNameToAdd), duplicate++, Path.GetExtension(fileNameToAdd));
                            Logging.Info(LogOptions.ClassName, "Exists, using filename {0}", fileNameToAdd);
                        }
                        Logging.Info(LogOptions.ClassName, "New name for zip: {0}", fileNameToAdd);

                        //and the file to the zip file and grab the entry reference
                        ZipEntry entry = zip.AddFile(s);
                        //then use it to modify the name of the entry in the zip file
                        //this moves it out of all the sub-folders up to the root directory
                        entry.FileName = Path.GetFileName(fileNameToAdd);
                        Logging.Info(LogOptions.ClassName, "File {0} added, entry name in zip = {1}", fileNameToAdd, entry.FileName);
                    }
                    string zipSavePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                        string.Format("RelhaxModpackLogs_{0}.zip", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
                    zip.Save(zipSavePath);
                    Logging.Info(LogOptions.ClassName, "Zip file saved to {0}", zipSavePath);
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
            Logging.Info(LogOptions.ClassName, "Deleting download cache");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("clearingDownloadCache");
            try
            {
                await FileUtils.DirectoryDeleteAsync(ApplicationConstants.RelhaxDownloadsFolderPath, false, false, 3, 100, "*.zip");
                await FileUtils.DirectoryDeleteAsync(ApplicationConstants.RelhaxDownloadsFolderPath, false, false, 3, 100, "*.xml");
            }
            catch (IOException ioex)
            {
                DiagnosticsStatusTextBox.Text = string.Format("{0}{1}{2}", Translations.GetTranslatedString("failedToClearDownloadCache"), Environment.NewLine, ApplicationConstants.RelhaxDownloadsFolderPath);
                Logging.Exception(ioex.ToString());
            }
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleaningDownloadCacheComplete");
            Logging.Info(LogOptions.ClassName, "Deleted download cache");
        }

        private async void ClearDownloadCacheDatabase_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info(LogOptions.ClassName, "Deleting database cache file");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("clearingDownloadCacheDatabase");
            try
            {
                await FileUtils.DirectoryDeleteAsync(ApplicationConstants.RelhaxDownloadsFolderPath, false, false, 3, 100, "*.xml");
            }
            catch (IOException ioex)
            {
                DiagnosticsStatusTextBox.Text = string.Format("{0}{1}{2}", Translations.GetTranslatedString("failedToClearDownloadCacheDatabase"), Environment.NewLine, ApplicationConstants.RelhaxDownloadsFolderPath);
                Logging.Exception(ioex.ToString());
            }
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleaningDownloadCacheDatabaseComplete");
            Logging.Info(LogOptions.ClassName, "Deleted database cache file");
        }

        private void TestLoadImageLibrariesButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info(LogOptions.ClassName, "Test load image libraries");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("loadingAtlasImageLibraries");
            UiUtils.AllowUIToUpdate();
            if (AtlasUtils.TestLoadAtlasLibraries(true))
            {
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("loadingAtlasImageLibrariesSuccess");
                Logging.Info(LogOptions.ClassName, "Test load image libraries pass");
            }
            else
            {
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("loadingAtlasImageLibrariesFail");
                if (MessageBox.Show(string.Format("{0}\n{1}", Translations.GetTranslatedString("missingMSVCPLibraries"), Translations.GetTranslatedString("openLinkToMSVCP")),
                                Translations.GetTranslatedString("missingMSVCPLibrariesHeader"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (!CommonUtils.StartProcess(AtlasUtils.MSVCPLink))
                    {
                        Logging.Error(LogOptions.ClassName, "Failed to open url to MSVCP: {0}", AtlasUtils.MSVCPLink);
                    }
                }
                Logging.Info(LogOptions.ClassName, "Test load image libraries fail");
            }
        }

        private void DownloadWGPatchFiles_Click(object sender, RoutedEventArgs e)
        {
            GameCenterUpdateDownloader gameCenterUpdateDownloader = new GameCenterUpdateDownloader(this.ModpackSettings)
            {
                SelectedClient = string.IsNullOrWhiteSpace(WoTDirectory) ? string.Empty : WoTDirectory
            };
            gameCenterUpdateDownloader.ShowDialog();
        }

        private async void CleanupModFilesButton_Click(object sender, RoutedEventArgs e)
        {
            string[] locationsToCheck = new string[]
            {
                Path.Combine(WoTDirectory, ApplicationConstants.WoT64bitFolder, ApplicationConstants.ModsDir),
                Path.Combine(WoTDirectory, ApplicationConstants.WoT64bitFolder, ApplicationConstants.ResModsDir),
                Path.Combine(WoTDirectory, ApplicationConstants.WoT32bitFolder, ApplicationConstants.ModsDir),
                Path.Combine(WoTDirectory, ApplicationConstants.WoT32bitFolder, ApplicationConstants.ResModsDir)
            };

            List<string> filesToDelete = new List<string>();
            foreach(string folderPath in locationsToCheck)
            {
                Logging.Debug(LogOptions.ClassName, "Processing folder {0}", folderPath);
                if(!Directory.Exists(folderPath))
                {
                    Logging.Debug("Directory does not exist");
                    continue;
                }
                int count = 0;
                string[] files = FileUtils.FileSearch(folderPath, SearchOption.AllDirectories, true, false);
                if (files != null)
                    count = files.Count();
                Logging.Debug(LogOptions.ClassName, "Added {0} files", count);
                filesToDelete.AddRange(files);
            }

            Logging.Debug(LogOptions.ClassName, "Deleting files");
            for(int i = 0; i < filesToDelete.Count; i++)
            {
                //check to make sure it's a file
                if (!File.Exists(filesToDelete[i]))
                    continue;

                DiagnosticsStatusTextBox.Text = string.Format("{0} {1} {2} {3}",
                    Translations.GetTranslatedString("deletingFile"), (i + 1), Translations.GetTranslatedString("of"), filesToDelete.Count);

                await Task.Run(() => FileUtils.FileDelete(filesToDelete[i]));
            }

            //fully delete the folders now
            Logging.Debug(LogOptions.ClassName, "Complete delete of folders");
            List<string> locationsFailedToDelete = new List<string>();
            foreach (string folderPath in locationsToCheck)
            {
                if(Directory.Exists(folderPath))
                    if (!FileUtils.DirectoryDelete(folderPath, true))
                        locationsFailedToDelete.Add(folderPath);
            }

            if(locationsFailedToDelete.Count > 0)
            {
                Logging.Error(LogOptions.ClassName, "Failed to delete the folders: {0}", string.Join(",", locationsFailedToDelete));
                MessageBox.Show(string.Format("{0}, {1}", Translations.GetTranslatedString("folderDeleteFailed"), string.Join(",\n", locationsFailedToDelete)));
            }

            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleanupModFilesCompleted");
        }

        private async void ClearGameCacheButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info(LogOptions.ClassName, "Cleaning AppData cache");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleanGameCacheProgress");

            bool clearCache = await Task.Run(() => CommonUtils.ClearCache());

            if(clearCache)
            {
                Logging.Info(LogOptions.ClassName, "Cleaning AppData cache success");
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleanGameCacheSuccess");
            }
            else
            {
                Logging.Info(LogOptions.ClassName, "Cleaning AppData cache fail");
                DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("cleanGameCacheFail");
            }
        }

        private void TrimRelhaxLogfileButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info(LogOptions.ClassName, "Trimming relhax.log (this file)");
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("trimRelhaxLogProgress");

            //turn off the logfile to trim it now
            Logging.Debug(LogOptions.ClassName, "Shutdown of the logfile for trimming");

            string logfilePath = Logging.GetLogfile(Logfiles.Application).Filepath;

            Logging.DisposeLogging(Logfiles.Application);

            //load the logfile into a string
            string entireLogfile = File.ReadAllText(logfilePath);

            //split it into an array based on the start/stop text
            string[] entireLogfileArray = entireLogfile.Split(new string[] { Logging.ApplicationlogStartStop }, StringSplitOptions.RemoveEmptyEntries);

            //filter out any entries that are whitespace or empty, to get the true number of log start/stop entries
            string[] entireLogfileArrayTrimmed = entireLogfileArray.ToList().FindAll(entry => !string.IsNullOrWhiteSpace(entry)).ToArray();

            //calculate how far back the unfiltered int tracker should go. It shouldn't count whitespace versions
            int withWhitespaceCounter = 0;
            int withoutWhitespaceCounter = 0;
            for(int i = entireLogfileArray.Length-1; i > 0; i--)
            {
                withWhitespaceCounter++;
                if (!string.IsNullOrWhiteSpace(entireLogfileArray[i]))
                    withoutWhitespaceCounter++;
                if (withoutWhitespaceCounter >= this.RelhaxLogfileTrimLength)
                    break;
            }

            //if the number of trimmed elements is greater then 3, then we can filter it
            bool trimmed = false;
            if(entireLogfileArrayTrimmed.Length > this.RelhaxLogfileTrimLength)
            {
                //currently hard-set to get the last 3 entries
                trimmed = true;
                StringBuilder newLogfileBuilder = new StringBuilder();
                for (int i = entireLogfileArray.Length - withWhitespaceCounter; i < entireLogfileArray.Length; i++)
                {
                    newLogfileBuilder.Append(Logging.ApplicationlogStartStop);
                    newLogfileBuilder.Append(entireLogfileArray[i]);
                    if (i != entireLogfileArray.Length - 1 && string.IsNullOrWhiteSpace(entireLogfileArray[i + 1]))
                    {
                        newLogfileBuilder.Append(Logging.ApplicationlogStartStop + Environment.NewLine);
                        i++;
                    }
                }

                //write it back to disk
                File.Delete(logfilePath);
                File.WriteAllText(logfilePath, newLogfileBuilder.ToString());
            }

            //DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("trimRelhaxLogFail");

            //restore the logfile
            Logging.Init(Logfiles.Application, ModpackSettings.VerboseLogging, true);
            DiagnosticsStatusTextBox.Text = Translations.GetTranslatedString("trimRelhaxLogSuccess");
            Logging.Info(LogOptions.ClassName, "Successfully trimmed the log file ({0})", trimmed? string.Format("Trimmed to {0} entries", this.RelhaxLogfileTrimLength) : "No trim required");
        }
    }
}
