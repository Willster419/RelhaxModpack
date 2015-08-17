using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;
using System.Reflection;
using Microsoft.Win32;



namespace RelicModManager
{
    public partial class MainWindow : Form
    {

        private FolderBrowserDialog selectWotFolder = new FolderBrowserDialog();
        private string wotFolder;
        private WebClient downloader = new WebClient();
        private string parsedFolder;
        private string parsedBackupFolder;
        private DialogResult tryingToCache;
        private CustomURLs custom = new CustomURLs();
        private VersionInfo info = new VersionInfo();
        private string tempPath = Path.GetTempPath();
        private string locatedTanksFolder;
        private bool alreadyDownloaded = false;
        private ZipFile relicZip;
        private double bytesIn;
        private double totalBytes;
        private int MBytesIn;
        private int MBytesTotal;
        private static int MBDivisor = 1048576;
        private string zipFileDownloadURL;
        private string ingameVoiceVersion;
        private string guiVersion;
        private string managerVersion;
        private string tanksLocation;
        private object theObject;
        private bool checkingForUpdates;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void downloadMods_Click(object sender, EventArgs e)
        {
            checkingForUpdates = false;
            this.resetUI();
            if (this.forceManuel.Checked)
            {
                if (!this.manuallyFindTanks()) return;
            }

            //try to find the tanks location by registry
            const string keyName = "HKEY_CURRENT_USER\\Software\\Classes\\.wotreplay\\shell\\open\\command";
            theObject = Registry.GetValue(keyName, "", -1);
            if (theObject == null)
            {
                if(!this.manuallyFindTanks()) return;
            }
            //parse it from the registry
            else
            {
                tanksLocation = (string)theObject;
                tanksLocation = tanksLocation.Substring(1);
                tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 6);
                if (!File.Exists(tanksLocation))
                {
                    if (!this.manuallyFindTanks()) return;
                }
                tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
                parsedFolder = tanksLocation + "\\res\\audio";
                wotFolder = tanksLocation;
            }

            //delete the old files if they exist
            downloadProgress.Text = "Delete old files...";
            try
            {
                System.IO.File.Delete(parsedFolder + "\\gui.fev");
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Registry Detection Failed. Check the 'force manuel detection' checkbox", "Something f*cked up");
                statusLabel.Text = "Aborted";
                return;
            }
            System.IO.File.Delete(parsedFolder + "\\gui.fsb");
            System.IO.File.Delete(parsedFolder + "\\ingame_voice_def.fev");
            System.IO.File.Delete(parsedFolder + "\\ingame_voice_def.fsb");

            //handle the custom URL information
            downloadProgress.Text = "Starting Download...";
            if (customDownloadURL.Checked)
            {
                custom.ShowDialog();
                if (custom.canceling)
                {
                    downloadProgress.Text = "Canceled";
                    return;
                }
                zipFileDownloadURL = custom.zipFileURL.Text;
            }
            if (censoredVersion.Checked)
            {
                zipFileDownloadURL = "http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic_censored/relic.zip";
            }
            else
            {
                zipFileDownloadURL = "http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/relic.zip";
            }

            //download unzip cleanup
            this.download(new Uri(zipFileDownloadURL), tempPath + "\\relic.zip");
            alreadyDownloaded = true;
        }

        private void stockSounds_Click(object sender, EventArgs e)
        {
            checkingForUpdates = false;
            this.resetUI();

            const string keyName = "HKEY_CURRENT_USER\\Software\\Classes\\.wotreplay\\shell\\open\\command";
            theObject = Registry.GetValue(keyName, "", -1);
            if (theObject == null)
            {
                if (!this.manuallyFindTanks()) return;
            }

            //parse it from the registry
            else
            {
                tanksLocation = (string)theObject;
                tanksLocation = tanksLocation.Substring(1);
                tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 6);
                if (!File.Exists(tanksLocation))
                {
                    if (!this.manuallyFindTanks()) return;
                }
                tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
                parsedFolder = tanksLocation + "\\res\\audio";
                wotFolder = tanksLocation;
            }

            //delete the old files if they exist
            downloadProgress.Text = "Delete old files...";
            System.IO.File.Delete(parsedFolder + "\\gui.fev");
            System.IO.File.Delete(parsedFolder + "\\gui.fsb");
            System.IO.File.Delete(parsedFolder + "\\ingame_voice_def.fev");
            System.IO.File.Delete(parsedFolder + "\\ingame_voice_def.fsb");

            //handle the custom URL information
            downloadProgress.Text = "Starting Download...";
            if (customDownloadURL.Checked)
            {
                custom.ShowDialog();
                if (custom.canceling)
                {
                    downloadProgress.Text = "Canceled";
                    return;
                }
                zipFileDownloadURL = custom.zipFileURL.Text;
            }
            else
            {
                zipFileDownloadURL = "http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/origional/origional.zip";
            }

            //download unzip cleanup
            this.download(new Uri(zipFileDownloadURL), tempPath + "\\relic.zip");
            alreadyDownloaded = true;
        }

        private void backupCustom_Click(object sender, EventArgs e)
        {
            this.resetUI();
            wotFolder = this.getBackupFolder();
            if (wotFolder == null)
            {
                downloadProgress.Text = "Canceled";
                return;
            }
            downloadProgress.Text = "Backing up...";
            parsedBackupFolder = wotFolder + "\\backup";
            if (!Directory.Exists(parsedBackupFolder)) Directory.CreateDirectory(parsedBackupFolder);
            if (File.Exists(parsedBackupFolder + "\\yes.txt")) return;
            if (File.Exists(wotFolder + "\\gui.fev")) File.Copy(wotFolder + "\\gui.fev", parsedBackupFolder + "\\gui.fev");
            if (File.Exists(wotFolder + "\\gui.fsb")) File.Copy(wotFolder + "\\gui.fsb", parsedBackupFolder + "\\gui.fsb");
            File.Copy(wotFolder + "\\ingame_voice.fev", parsedBackupFolder + "\\ingame_voice.fev");
            File.Copy(wotFolder + "\\ingame_voice_def.fsb", parsedBackupFolder + "\\ingame_voice_def.fsb");
            File.Create(parsedBackupFolder + "\\yes.txt");
            downloadProgress.Text = "Complete!";
        }

        private void restoreCustom_Click(object sender, EventArgs e)
        {
            this.resetUI();
            wotFolder = this.getBackupFolder();
            if (wotFolder == null)
            {
                downloadProgress.Text = "Canceled";
                return;
            }
            downloadProgress.Text = "Restoring...";
            parsedBackupFolder = wotFolder + "\\backup";
            if (!Directory.Exists(parsedBackupFolder)) return;
            if (File.Exists(parsedBackupFolder + "\\gui.fev"))
            {
                File.Delete(wotFolder + "\\gui.fev");
                File.Copy(parsedBackupFolder + "\\gui.fev", wotFolder + "\\gui.fev");
            }
            if (File.Exists(parsedBackupFolder + "\\gui.fsb"))
            {
                File.Delete(wotFolder + "\\gui.fsb");
                File.Copy(parsedBackupFolder + "\\gui.fsb", wotFolder + "\\gui.fsb");
            }
            File.Delete(wotFolder + "\\ingame_voice.fev");
            File.Delete(wotFolder + "\\ingame_voice_def.fsb");
            File.Copy(parsedBackupFolder + "\\ingame_voice.fev", wotFolder + "\\ingame_voice.fev");
            File.Copy(parsedBackupFolder + "\\ingame_voice_def.fsb", wotFolder + "\\ingame_voice_def.fsb");
            downloadProgress.Text = "Complete";
        }

        void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            bytesIn = double.Parse(e.BytesReceived.ToString());
            totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            MBytesIn = (int)bytesIn / MBDivisor;
            MBytesTotal = (int)totalBytes / MBDivisor;
            downloadProgress.Text = "Downloaded " + MBytesIn + " MB" + " of " + MBytesTotal + " MB";
            downloadProgressBar.Value = e.ProgressPercentage;
        }

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (checkingForUpdates)
            {
                this.unzipAndCleanup(tempPath + "\\relic.zip", tempPath + "\\versionCheck");
            }
            else
            {
                this.unzipAndCleanup(tempPath + "\\relic.zip", parsedFolder);
            }
            
        }

        private void download(Uri URL, string zipFile)
        {
            try
            {
                //delete temp if it's there
                File.Delete(zipFile);
                //download new zip file
                downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                downloader.DownloadFileAsync(URL, zipFile, zipFile);
            }
            catch (WebException)
            {
                if (checkingForUpdates)
                {
                    MessageBox.Show("Unable to check for updates\n try using it with the custom links checked");
                    downloadProgress.Text = "Failed";
                    return;
                }
                else
                {
                    MessageBox.Show("Error: Eithor you are offline or my NAS is offline\nTry downloading using the custom URL option via google drive");
                    downloadProgress.Text = "Failed";
                    return;
                }
            }
        }

        private void unzipAndCleanup(string zipFile, string extractFolder)
        {
            //exract
            relicZip = new ZipFile(zipFile);
            relicZip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
            //cleanup
            relicZip.Dispose();
            System.IO.File.Delete(zipFile);
            downloadProgress.Text = "Complete!";
            if (checkingForUpdates) this.checkForUpdates();
        }

        private void resetUI()
        {
            this.downloadProgress.Text = "Idle";
            this.downloadProgressBar.Value = downloadProgressBar.Minimum;
        }

        private string getDownloadOnlyFolder()
        {
            selectWotFolder.Description = "Select folder to download to";
            if (selectWotFolder.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return null;
            return selectWotFolder.SelectedPath;
        }

        private string getBackupFolder()
        {
            selectWotFolder.Description = "Select the 'audio' folder where your moded audio files are held. (Eithor under res or res_mods)";
            if (selectWotFolder.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return null;
            return selectWotFolder.SelectedPath;
        }

        private void whatVersion_Click(object sender, EventArgs e)
        {
            checkingForUpdates = true;
            //if this is the first thing the user did when opening the application
            if (!alreadyDownloaded)
            {

                //try to find the tanks location by registry
                const string keyName = "HKEY_CURRENT_USER\\Software\\Classes\\.wotreplay\\shell\\open\\command";
                theObject = Registry.GetValue(keyName, "", -1);
                if (theObject == null)
                {
                    if (!this.manuallyFindTanks()) return;
                }

                //parse it from the registry
                else
                {
                    tanksLocation = (string)theObject;
                    tanksLocation = tanksLocation.Substring(1);
                    tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 6);
                    if (!File.Exists(tanksLocation))
                    {
                        if (!this.manuallyFindTanks()) return;
                    }
                    tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
                    parsedFolder = tanksLocation + "\\res\\audio";
                    wotFolder = tanksLocation;
                }

                //get the version infos if it's there
                locatedTanksFolder = wotFolder + "\\res\\audio\\relicModVersion";
                if (File.Exists(locatedTanksFolder + "\\ingame voice version.txt"))
                {
                    ingameVoiceVersion = "version " + File.ReadAllText(locatedTanksFolder + "\\ingame voice version.txt");
                }
                else
                {
                    ingameVoiceVersion = "not installed";
                }
                if (File.Exists(locatedTanksFolder + "\\gui version.txt"))
                {
                    guiVersion = "version " + File.ReadAllText(locatedTanksFolder + "\\gui version.txt");
                }
                else
                {
                    guiVersion = "not installed";
                }
                managerVersion = "version 9.2";

                //display the version info
                info.downloadedVersionInfo.Text = "gui sounds " + guiVersion + "\ningame voice sounds " + ingameVoiceVersion + "\ndownlaod manager " + managerVersion;
                info.ShowDialog();

                //if we're checking for updates, do so
                if (info.checkForUpdates)
                {
                    downloadProgress.Text = "Checking for updates...";
                    //download and extract to temp folder
                    if (customDownloadURL.Checked)
                    {
                        custom.ShowDialog();
                        if (custom.canceling)
                        {
                            downloadProgress.Text = "Canceled";
                            return;
                        }
                        zipFileDownloadURL = custom.zipFileURL.Text;
                    }
                    else
                    {
                        zipFileDownloadURL = "http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/version.zip";
                    }
                    this.download(new Uri(zipFileDownloadURL), tempPath + "\\relic.zip");
                }
                return;
            }

            //else just display what is in the version text files
            //no need to re-get the world of tanks folder
            else
            {
                locatedTanksFolder = wotFolder + "\\res\\audio\\relicModVersion";
                if (File.Exists(locatedTanksFolder + "\\ingame voice version.txt"))
                {
                    ingameVoiceVersion = "version " + File.ReadAllText(locatedTanksFolder + "\\ingame voice version.txt");
                }
                else
                {
                    ingameVoiceVersion = "not installed";
                }
                if (File.Exists(locatedTanksFolder + "\\gui version.txt"))
                {
                    guiVersion = "version " + File.ReadAllText(locatedTanksFolder + "\\gui version.txt");
                }
                else
                {
                    guiVersion = "not installed";
                }
                managerVersion = "version 9.2";

                //display the version info
                info.downloadedVersionInfo.Text = "gui sounds " + guiVersion + "\ningame voice sounds " + ingameVoiceVersion + "\ndownlaod manager " + managerVersion;
                info.ShowDialog();

                //if we're checking for updates, do so
                if (info.checkForUpdates)
                {
                    downloadProgress.Text = "Checking for updates...";
                    //download and extract to temp folder
                    if (customDownloadURL.Checked)
                    {
                        custom.ShowDialog();
                        if (custom.canceling)
                        {
                            downloadProgress.Text = "Canceled";
                            return;
                        }
                        zipFileDownloadURL = custom.zipFileURL.Text;
                    }
                    else
                    {
                        zipFileDownloadURL = "http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/version.zip";
                    }
                    this.download(new Uri(zipFileDownloadURL), tempPath + "\\relic.zip");
                }
                return;
            }
        }

        private void censoredVersion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("When i make the censored version this will work");
            censoredVersion.Checked = false;
            if (this.customDownloadURL.Checked)
            {
                MessageBox.Show("Irrelevant since custom download link is checked");
                censoredVersion.Checked = false;
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            downloader.Credentials = new NetworkCredential("tudbury209", "tudbury209");
        }

        private void checkForUpdates()
        {
            bool isOutofDate = false;
            //get the version infos
            string newIngameVoiceVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\ingame voice version.txt");
            string newGuiVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\gui version.txt");
            string newManagerVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\manager version.txt");
            //cleanup extracted folders
            Directory.Delete(tempPath + "\\versionCheck", true);
            //display what we found
            if (!guiVersion.Equals(newGuiVersion) || !ingameVoiceVersion.Equals(newIngameVoiceVersion))
            {
                MessageBox.Show("Your voice sounds are out of date. Please update.");
                isOutofDate = true;
            }
            if (!managerVersion.Equals(newManagerVersion))
            {
                DialogResult result = MessageBox.Show("Your manager is out of date. Go to the forms to download the update?", "Manager is out of date", MessageBoxButtons.YesNo);
                isOutofDate = true;
                if (result.Equals(DialogResult.Yes))
                {
                    System.Diagnostics.Process.Start("http://relicgaming.com/index.php?topic=165");
                }
                checkingForUpdates = false;
                return;
            }
            checkingForUpdates = false;
            if (!isOutofDate) MessageBox.Show("Your manager and sound mods are up to date");
        }

        private bool manuallyFindTanks()
        {
             //unable to find it in the registry, so ask for it
            //the user is caching...so far
            if (downloadOnly.Checked)
            {
                wotFolder = this.getDownloadOnlyFolder();
                if (wotFolder == null)
                {
                    downloadProgress.Text = "Canceled";
                    return false;
                }

                //save the tanks install!!!
                if (File.Exists(wotFolder + "\\WorldOfTanks.exe"))
                {
                    tryingToCache = MessageBox.Show("World of Tanks install detected. Uncheck 'Download only' and try aagain.", "Your Tanks install was saved!");
                    downloadProgress.Text = "Aborted";
                    return false;
                }
                if (Directory.Exists(wotFolder + "\\res")) Directory.Delete((wotFolder + "\\res"), true);
                Directory.CreateDirectory(wotFolder + "\\res\\audio");
                return true;
            }

            //the user is installing
            else
            {
                if (findWotExe.ShowDialog().Equals(DialogResult.Cancel))
                {
                    downloadProgress.Text = "Canceled";
                    return false;
                }
                wotFolder = findWotExe.FileName;
                wotFolder = wotFolder.Substring(0, wotFolder.Length - 17);
            }

            parsedFolder = wotFolder + "\\res\\audio";
            return true;
            }

        //old method of unzipping
        /*public static void UnZip(string zipFile, string folderPath)
        {
            if (!File.Exists(zipFile))
                throw new FileNotFoundException();

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            Shell32.Shell objShell = new Shell32.Shell();
            Shell32.Folder destinationFolder = objShell.NameSpace(folderPath);
            Shell32.Folder sourceFile = objShell.NameSpace(zipFile);

            foreach (var file in sourceFile.Items())
            {
                destinationFolder.CopyHere(file, 4 | 16);
            }
        }*/

        //old method of downloading
        /*if (installing) System.IO.File.Delete(parsedFolder + "\\gui.fev");
        downloadProgress.Text = "Downloading 1 of 4...";
        try
        {
            downloader.DownloadFile("http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/gui.fev", parsedFolder + "\\gui.fev");
            //WebExeption
        }
        catch (WebException)
        {
            MessageBox.Show("Error: Eithor you are offline or my NAS is offline\nTry downloading using the custom URL option via google drive");
            downloadProgress.Text = "Failed";
            return;
        }

        if (installing) System.IO.File.Delete(parsedFolder + "\\gui.fsb");
        downloadProgress.Text = "Downloading 2 of 4...";
        downloader.DownloadFile("http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/gui.fsb", parsedFolder + "\\gui.fsb");

        if (installing) System.IO.File.Delete(parsedFolder + "\\ingame_voice_def.fev");
        downloadProgress.Text = "Downloading 3 of 4...";
        downloader.DownloadFile("http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/ingame_voice.fev", parsedFolder + "\\ingame_voice.fev");

        if (installing) System.IO.File.Delete(parsedFolder + "\\ingame_voice_def.fsb");
        downloadProgress.Text = "Downloading 4 of 4...";
        downloader.DownloadFile("http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/ingame_voice_def.fsb", parsedFolder + "\\ingame_voice_def.fsb");

        downloadProgress.Text = "Complete!";*/

        //old method of getting version
        /*if (File.Exists(tempPath + "\\Version.txt")) File.Delete(tempPath + "\\Version.txt");
            downloader.DownloadFile("http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/version.txt", tempPath + "\\Version.txt");
            info.downloadedVersionInfo.Text = File.ReadAllText(tempPath + "\\Version.txt");*/

        //new old method of getting version
        /*downloadProgress.Text = "Checking for updates...";
        Application.DoEvents();
        //download and extract to temp folder
        //this.download(new Uri("http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/relic.zip"), tempPath + "\\relic.zip");
        downloader.DownloadFile("http://96.61.83.3/OtherStuff/Other%20Stuff/World%20of%20Pdanks%20stuffs/relic%20mod/relic/relic.zip", tempPath + "\\relic.zip");
        //download stardard way
        this.unzipAndCleanup(tempPath + "\\relic.zip", tempPath + "\\versionCheck");*/
    }
}
