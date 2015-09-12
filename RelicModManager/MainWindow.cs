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
        private CustomURLs custom = new CustomURLs();
        private VersionInfo info = new VersionInfo();
        private string tempPath = Path.GetTempPath();
        private ZipFile relicZip;
        private double bytesIn;
        private double totalBytes;
        private int MBytesIn;
        private int MBytesTotal;
        private static int MBDivisor = 1048576;
        private string ingameVoiceVersion;
        private string guiVersion;
        private string managerVersion = "version 12";
        private string tanksLocation;
        private object theObject;
        private string sixthSenseVersion;
        private SelectFeatures features = new SelectFeatures();
        private List<DownloadItem> downloadQueue;
        private int downloadCount;
        private int totalDownloadCount;
        private bool isAutoDetected;
        private string versionFolder;
        private bool hasUnzipped;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void downloadMods_Click(object sender, EventArgs e)
        {
            // this is how the program will be re-written
            // - reset the UI and some download features
            // - method to auto return the tanks install location by reg search
            // if that failes, return the tanks install location by asking
            // - parse the strings
            // - delete old files, if they exist
            // - download the selected files
            // - extract them and cleanup
            this.features.ShowDialog();
            if (features.canceling)
            {
                downloadProgress.Text = "Canceled";
                return;
            }
            this.reset();
            if (this.autoFindTanks() == null || this.forceManuel.Checked)
            {
                if (this.manuallyFindTanks() == null) return;
            }
            if (this.parseStrings() == null) this.displayError("The auto-detection failed. Please use the 'force manuel' option", null);
            if (this.prepareForInstall(false) == null) this.displayError("Failed preparing for install.", null);
            this.createDownloadQueue();
            this.downloader_DownloadFileCompleted(null, null);
        }

        private void downloadRelhax_Click(object sender, EventArgs e)
        {
            // this is how the program will be re-written
            // - reset the UI and some download features
            // - parse the strings
            // - delete old files, if they exist
            // - download the selected files
            // - extract them and cleanup
            
            this.reset();
            this.features.ShowDialog();
            if (features.canceling)
            {
                downloadProgress.Text = "Canceled";
                return;
            }
            tanksLocation = this.getDownloadOnlyFolder();
            if (tanksLocation == null) return;
            parsedFolder = tanksLocation + "\\res\\audio";
            try
            {
                System.IO.Directory.Delete(tanksLocation + "\\res", true);
            }
            catch (IOException) { }
            System.IO.Directory.CreateDirectory(parsedFolder);
            this.createDownloadQueue();
            this.downloader_DownloadFileCompleted(null,null);
        }

        private void stockSounds_Click(object sender, EventArgs e)
        {
            this.reset();
            if (this.autoFindTanks() == null || this.forceManuel.Checked)
            {
                if (this.manuallyFindTanks() == null) return;
            }
            if (this.parseStrings() == null) this.displayError("The auto-detection failed. PLease use the 'force manuel' option", null);
            if (this.prepareForInstall(true) == null) this.displayError("Tell Willster he screwed up. He'll know.", null);
            downloadQueue = new List<DownloadItem>();
            downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/origional.zip"), tempPath + "\\origional.zip"));
            totalDownloadCount = downloadQueue.Count;
            downloadNumberCount.Visible = true;
            this.downloader_DownloadFileCompleted(null, null);
        }

        private void backupCustom_Click(object sender, EventArgs e)
        {
            this.reset();
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
            if (File.Exists(wotFolder + "\\xvm.fev")) File.Copy(wotFolder + "\\xvm.fev", parsedBackupFolder + "\\xvm.fev");
            if (File.Exists(wotFolder + "\\xvm.fsb")) File.Copy(wotFolder + "\\xvm.fsb", parsedBackupFolder + "\\xvm.fsb");
            File.Copy(wotFolder + "\\ingame_voice.fev", parsedBackupFolder + "\\ingame_voice.fev");
            File.Copy(wotFolder + "\\ingame_voice_def.fsb", parsedBackupFolder + "\\ingame_voice_def.fsb");
            File.Create(parsedBackupFolder + "\\yes.txt");
            downloadProgress.Text = "Complete!";
        }

        private void restoreCustom_Click(object sender, EventArgs e)
        {
            this.reset();
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
            if (File.Exists(parsedBackupFolder + "\\xvm.fev"))
            {
                File.Delete(wotFolder + "\\xvm.fev");
                File.Copy(parsedBackupFolder + "\\xvm.fev", wotFolder + "\\xvm.fev");
            }
            if (File.Exists(parsedBackupFolder + "\\xvm.fsb"))
            {
                File.Delete(wotFolder + "\\xvm.fsb");
                File.Copy(parsedBackupFolder + "\\xvm.fsb", wotFolder + "\\xvm.fsb");
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
            if (hasUnzipped) return;
            if (downloadQueue.Count > 0)
            {
                downloadCount++;
                this.downloadNumberCount.Text = "Downloading " + downloadCount + " of " + totalDownloadCount;
                this.download(downloadQueue[0].getURL(), downloadQueue[0].getZipFile());
                downloadQueue.RemoveAt(0);
                return;
            }
            if (downloadCount == totalDownloadCount)
            {
                if (File.Exists(tempPath + "\\relic.zip")) this.unzipAndCleanup(tempPath + "\\relic.zip", parsedFolder);
                if (File.Exists(tempPath + "\\relic_censored.zip")) this.unzipAndCleanup(tempPath + "\\relic_censored.zip", parsedFolder);
                if (File.Exists(tempPath + "\\gui.zip")) this.unzipAndCleanup(tempPath + "\\gui.zip", parsedFolder);
                if (File.Exists(tempPath + "\\6thSense.zip")) this.unzipAndCleanup(tempPath + "\\6thSense.zip", parsedFolder);
                if (File.Exists(tempPath + "\\origional.zip")) this.unzipAndCleanup(tempPath + "\\origional.zip", parsedFolder);
                if (File.Exists(tempPath + "\\version.zip"))
                {
                    this.unzipAndCleanup(tempPath + "\\version.zip", tempPath + "\\versionCheck");
                    this.checkForUpdates();
                }
                hasUnzipped = true;
            }
        }

        private void whatVersion_Click(object sender, EventArgs e)
        {
            // How to re-write this version
            // auto if fail manuel detect
            // parse acordingly
            // read current mod information
            // display current mod information
            // decide if checking for updates
            // so, download version info
            // determine what is out of date
            // inform the user

            this.reset();
            if (this.autoFindTanks() == null || this.forceManuel.Checked)
            {
                if (this.manuallyFindTanks() == null) return;
            }
            if (this.parseStrings() == null) this.displayError("The auto-detection failed. PLease use the 'force manuel' option", null);
            versionFolder = tanksLocation + "\\res\\audio\\relicModVersion";
            if (File.Exists(versionFolder + "\\ingame voice version.txt"))
            {
                ingameVoiceVersion = "version " + File.ReadAllText(versionFolder + "\\ingame voice version.txt");
            }
            else
            {
                ingameVoiceVersion = "not installed";
            }
            if (File.Exists(versionFolder + "\\gui version.txt"))
            {
                guiVersion = "version " + File.ReadAllText(versionFolder + "\\gui version.txt");
            }
            else
            {
                guiVersion = "not installed";
            }
            if (File.Exists(versionFolder + "\\6thSense version.txt"))
            {
                sixthSenseVersion = "version " + File.ReadAllText(versionFolder + "\\6thSense version.txt");
            }
            else
            {
                sixthSenseVersion = "not installed";
            }
            info.downloadedVersionInfo.Text = "gui sounds " + guiVersion + "\ningame voice sounds " + ingameVoiceVersion + "\n6th Sense " + sixthSenseVersion + "\ndownlaod manager " + managerVersion;
            info.ShowDialog();

            //if we're checking for updates, do so
            if (info.checkForUpdates)
            {
                downloadProgress.Text = "Checking for updates...";
                downloadQueue = new List<DownloadItem>();
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/version.zip"), tempPath + "\\version.zip"));
                totalDownloadCount = downloadQueue.Count;
                downloadNumberCount.Visible = true;
                this.downloader_DownloadFileCompleted(null, null);
                //make sure to include the \\versionCheck\\ folder
               
            }
           
        }

        private String parseStrings()
        {
            if (isAutoDetected)
            {
                tanksLocation = tanksLocation.Substring(1);
                tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 6);
                if (!File.Exists(tanksLocation)) return null;
            }
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
            parsedFolder = tanksLocation + "\\res\\audio";
            return "1";
        }

        private String prepareForInstall(bool deleteAll)
        {
            //delete the old files if they exist
            downloadProgress.Text = "Delete old files...";
            try
            {
                if (this.features.relhaxBox.Checked || this.features.relhaxCensoredBox.Checked || deleteAll) System.IO.File.Delete(parsedFolder + "\\ingame_voice_def.fev");
                if (this.features.relhaxBox.Checked || this.features.relhaxCensoredBox.Checked || deleteAll) System.IO.File.Delete(parsedFolder + "\\ingame_voice_def.fsb");
                if (this.features.guiBox.Checked || deleteAll) System.IO.File.Delete(parsedFolder + "\\gui.fev");
                if (this.features.guiBox.Checked || deleteAll) System.IO.File.Delete(parsedFolder + "\\gui.fsb");
                if (this.features.sixthSenseBox.Checked || deleteAll) System.IO.File.Delete(parsedFolder + "\\xvm.fev");
                if (this.features.sixthSenseBox.Checked || deleteAll) System.IO.File.Delete(parsedFolder + "\\xvm.fsb");
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            
            
            try
            {
                System.IO.Directory.Delete(parsedFolder + "\\relicModVersion", true);
            }
            catch (DirectoryNotFoundException) { }
            return "We're all set!";
        }

        private void displayError(String errorText, String errorHandle)
        {
            if(!errorHandle.Equals(null)) MessageBox.Show(errorText, errorHandle);
            else MessageBox.Show(errorText);
            statusLabel.Text = "Aborted";
        }

        private void download(Uri URL, string zipFile)
        {
            //delete temp if it's there
            File.Delete(zipFile);
            //download new zip file
            downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
            downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
            downloader.DownloadFileAsync(URL, zipFile, zipFile);
        }

        private void unzipAndCleanup(string zipFile, string extractFolder)
        {
            try
            {
                //exract
                relicZip = new ZipFile(zipFile);
                relicZip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
                //cleanup
                relicZip.Dispose();
                System.IO.File.Delete(zipFile);
                downloadProgress.Text = "Complete!";
            }
            catch (ZipException)
                {
                    MessageBox.Show("Error Downloading, please try again. If this is not the first\ntime you have seen this, tell Willster he messed up");
                    downloadProgress.Text = "Error";
                }
        }

        private void reset()
        {
            //reset the UI and critical componets
            downloader.Dispose();
            downloader = new WebClient();
            this.downloadProgress.Text = "Idle";
            this.downloadProgressBar.Value = downloadProgressBar.Minimum;
            downloadCount = 0;
            isAutoDetected = false;
            downloadNumberCount.Visible = false;
            hasUnzipped = false;
        }

        private void createDownloadQueue()
        {
            downloadQueue = new List<DownloadItem>();
            if (this.features.relhaxBox.Checked) downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/relic.zip"), tempPath + "\\relic.zip"));
            if (this.features.relhaxCensoredBox.Checked) downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/relic_censored.zip"), tempPath + "\\relic_censored.zip"));
            if (this.features.guiBox.Checked) downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/gui.zip"), tempPath + "\\gui.zip"));
            if (this.features.sixthSenseBox.Checked) downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/6thSense.zip"), tempPath + "\\6thSense.zip"));
            totalDownloadCount = downloadQueue.Count;
            downloadNumberCount.Visible = true;
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

        private void checkForUpdates()
        {
            bool isOutofDate = false;
            //get the version infos
            string newIngameVoiceVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\ingame voice version.txt");
            string newGuiVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\gui version.txt");
            string newManagerVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\manager version.txt");
            string new6thSenseVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\6thSense version.txt");
            //cleanup extracted folders
            Directory.Delete(tempPath + "\\versionCheck", true);
            //display what we found
            if (!ingameVoiceVersion.Equals(newIngameVoiceVersion))
            {
                if (ingameVoiceVersion.Equals("not installed")) return;
                MessageBox.Show("Your ingame voice sounds are out of date. Please update.");
                isOutofDate = true;
            }
            if (!guiVersion.Equals(newGuiVersion))
            {
                if (guiVersion.Equals("not installed")) return;
                MessageBox.Show("Your gui voice sounds are out of date. Please update.");
                isOutofDate = true;
            }
            if (!sixthSenseVersion.Equals(new6thSenseVersion))
            {
                if (sixthSenseVersion.Equals("not installed")) return;
                MessageBox.Show("Your 6th Sense sounds is out of date. Please update.");
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
                return;
            }
            if (!isOutofDate) MessageBox.Show("Your manager and sound mods are up to date");
        }

        private string autoFindTanks()
        {
            const string keyName = "HKEY_CURRENT_USER\\Software\\Classes\\.wotreplay\\shell\\open\\command";
            theObject = Registry.GetValue(keyName, "", -1);
            if (theObject == null) return null;
            isAutoDetected = true;
            return (string)theObject;
        }

        private string manuallyFindTanks()
        {
            //unable to find it in the registry, so ask for it
            if (findWotExe.ShowDialog().Equals(DialogResult.Cancel))
            {
                downloadProgress.Text = "Canceled";
                return null;
            }
            tanksLocation = findWotExe.FileName;
            return "all good";
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

    class DownloadItem
    {
        private Uri URL;
        private string zipFile;
        public DownloadItem(Uri newURL, String newZipFile)
        {
            URL = newURL;
            zipFile = newZipFile;
        }
        public Uri getURL() { return URL; }
        public string getZipFile() { return zipFile; }
    }

}
