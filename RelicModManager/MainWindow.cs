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
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;




namespace RelicModManager
{
    public partial class MainWindow : Form
    {

        private FolderBrowserDialog selectWotFolder = new FolderBrowserDialog();
        private string wotFolder;
        private WebClient downloader = new WebClient();
        private string modAudioFolder;
        private string parsedBackupFolder;
        private CustomURLs custom = new CustomURLs();
        private VersionInfo info = new VersionInfo();
        private string tempPath = Path.GetTempPath();
        private double bytesIn;
        private double totalBytes;
        private int MBytesIn;
        private int MBytesTotal;
        private static int MBDivisor = 1048576;
        private string ingameVoiceVersion;
        private string guiVersion;
        private string managerVersion = "version 17";
        private string tanksLocation;
        private object theObject;
        private string sixthSenseVersion;
        private SelectFeatures features = new SelectFeatures();
        private List<DownloadItem> downloadQueue;
        private int downloadCount;
        private volatile int totalDownloadCount;
        private bool isAutoDetected;
        private string versionFolder;
        string appPath = Application.StartupPath;
        private string newVersionName;
        private bool isAlreadyStarted = false;
        private bool isStartupCheck = true;
        private pleaseWait wait = new pleaseWait();
        private string parsedModsFolder;//0.9.x
        private string stockTextFolder;
        private string modTextFolder;
        private string stockAudioFolder;
        private string modGuiFolder;
        private string modGuiFolderBase;
        private string modTextFolderBase;
        int numZipFiles;
        int totalZipFiles;
        bool repairMode;
        ZipFile zip;
        bool workerIdle;
        double totalProgress;
        double allProgress;
        public volatile bool closeIt = false;
        private bool isInstalling = false;

        public MainWindow()
        {
            InitializeComponent();
        }
        //install RelHax
        private void downloadMods_Click(object sender, EventArgs e)
        {
            isInstalling = true;
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
            if (this.parseStrings() == null)
            {
                this.displayError("The auto-detection failed. Please use the 'force manual' option", null);
                return;
            }
            /*if (this.prepareForInstall(false) == null)
            {
                this.displayError("Failed preparing for install.", null);
                return;
            }*/
            this.createDownloadQueue();
            this.downloader_DownloadFileCompleted(null, null);
        }

        //remove any residual files this may have left in the res folder
        private void cleanOldVersions()
        {
            Application.DoEvents();
            downloadProgress.Text = "Repairing...";
            Application.DoEvents();
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice.fev");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_def.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_CN1.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_CS.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_DE.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_EN.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_FR.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_JA.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_RH.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_RHC.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_RU.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_UK.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_cn1.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_cs.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_de.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_en.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_fr.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_ja.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_rh.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_rhc.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_ru.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_uk.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_REL.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_rel.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_U01.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\ingame_voice_U02.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\gui.fev");
            System.IO.File.Delete(stockAudioFolder + "\\gui.fsb");
            System.IO.File.Delete(stockAudioFolder + "\\xvm.fev");
            System.IO.File.Delete(stockAudioFolder + "\\xvm.fsb");
            if(Directory.Exists(stockAudioFolder + "\\relicModVersion"))Directory.Delete(stockAudioFolder + "\\relicModVersion", true);
            downloadQueue = new List<DownloadItem>();
            downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/origional.zip"), tempPath + "\\origional.zip"));
            totalDownloadCount = downloadQueue.Count;
            downloadNumberCount.Visible = true;
            this.downloader_DownloadFileCompleted(null, null);
        }

        //download RelHax
        private void downloadRelhax_Click(object sender, EventArgs e)
        {
            this.reset();
            this.features.ShowDialog();
            if (features.canceling)
            {
                downloadProgress.Text = "Canceled";
                return;
            }
            tanksLocation = this.getDownloadOnlyFolder();
            if (tanksLocation == null) return;
            parsedModsFolder = tanksLocation + "\\relHax";
            Directory.CreateDirectory(parsedModsFolder);
            Directory.Delete(parsedModsFolder, true);
            Directory.CreateDirectory(parsedModsFolder);
            if (File.Exists(tanksLocation + "\\worldOfTanks.exe"))
            {
                this.displayError("It looks like you are trying to 'install' rather \nthan 'download' the mods. please use the 'install' button", "trying to install?");
                return;
            }
            this.createDownloadQueue();
            this.downloader_DownloadFileCompleted(null,null);
        }
        
        //uninstall RelHax
        private void stockSounds_Click(object sender, EventArgs e)
        {
            this.reset();
            Application.DoEvents();
            downloadProgress.Text = "Preparing...";
            Application.DoEvents();
            if (this.autoFindTanks() == null || this.forceManuel.Checked)
            {
                if (this.manuallyFindTanks() == null) return;
            }
            if (this.parseStrings() == null)
            {
                this.displayError("The auto-detection failed. Please use the 'force manual' option", null);
                return;
            }
            //all of the stock files are in the res folder
            //only matters is if this copied to res mods
            downloadProgress.Text = "Removing Audio...";
            downloadProgressBar.Value = 20;
            Application.DoEvents();
            //delete only relHax audio files
            if (Directory.Exists(modAudioFolder))
            {
                if (File.Exists(modAudioFolder + "\\RelHaxGui.bnk")) File.Delete(modAudioFolder + "\\RelHaxGui.bnk");
                if (File.Exists(modAudioFolder + "\\RelHaxChatShotcuts.bnk")) File.Delete(modAudioFolder + "\\RelHaxChatShotcuts.bnk");
                if (File.Exists(modAudioFolder + "\\RelHaxMusicSources.bnk")) File.Delete(modAudioFolder + "\\RelHaxMusicSources.bnk");
                if (File.Exists(modAudioFolder + "\\sixthsense.bnk")) File.Delete(modAudioFolder + "\\sixthsense.bnk");
                if (Directory.Exists(modAudioFolder + "\\uk")) Directory.Delete(modAudioFolder + "\\uk", true);
                if (Directory.Exists(modAudioFolder + "\\usa")) Directory.Delete(modAudioFolder + "\\usa", true);
                if (Directory.Exists(modAudioFolder + "\\relicModVersion")) Directory.Delete(modAudioFolder + "\\relicModVersion", true);
            }
            //if audioww is empty, delete it as well
            int totalFilesAndFoldersLeft = this.anythingElseRemaining(modAudioFolder);
            if (totalFilesAndFoldersLeft == 0) Directory.Delete(modAudioFolder);
            downloadProgressBar.Value = 50;
            Application.DoEvents();

            downloadProgress.Text = "Removing gui stuff...";
            downloadProgressBar.Value = 85;
            Application.DoEvents();
            //delete gui file
            if (File.Exists(modGuiFolder + "\\main_sound_modes.xml")) File.Delete(modGuiFolder + "\\main_sound_modes.xml");
            if (this.anythingElseRemaining(modGuiFolder) == 0) Directory.Delete(modGuiFolder);
            if (this.anythingElseRemaining(modGuiFolderBase) == 0) Directory.Delete(modGuiFolderBase);
            downloadProgress.Text = "Removing scripts...";
            if (File.Exists(parsedModsFolder + "\\scripts\\client\\gui\\mods\\mod_ChatCommandsVoice.pyc")) File.Delete(parsedModsFolder + "\\scripts\\client\\gui\\mods\\mod_ChatCommandsVoice.pyc");
            if (File.Exists(parsedModsFolder + "\\scripts\\client\\gui\\mods\\mod_SoundMapper.pyc")) File.Delete(parsedModsFolder + "\\scripts\\client\\gui\\mods\\mod_SoundMapper.pyc");
            if (this.anythingElseRemaining(parsedModsFolder + "\\scripts\\client\\gui\\mods") == 0) Directory.Delete(parsedModsFolder + "\\scripts\\client\\gui\\mods");
            if (this.anythingElseRemaining(parsedModsFolder + "\\scripts\\client\\gui") == 0) Directory.Delete(parsedModsFolder + "\\scripts\\client\\gui");
            if (this.anythingElseRemaining(parsedModsFolder + "\\scripts\\client") == 0) Directory.Delete(parsedModsFolder + "\\scripts\\client");
            if (this.anythingElseRemaining(parsedModsFolder + "\\scripts") == 0) Directory.Delete(parsedModsFolder + "\\scripts");

            if (File.Exists(tanksLocation + "\\res_mods\\configs\\D2R52\\mod_SoundMapper.xml")) File.Delete(tanksLocation + "\\res_mods\\configs\\D2R52\\mod_SoundMapper.xml");
            if (this.anythingElseRemaining(tanksLocation + "\\res_mods\\configs\\D2R52") == 0) Directory.Delete(tanksLocation + "\\res_mods\\configs\\D2R52");
            if (this.anythingElseRemaining(tanksLocation + "\\res_mods\\configs") == 0) Directory.Delete(tanksLocation + "\\res_mods\\configs");
            
            downloadProgress.Text = "Unpatching xml files...";
            downloadProgressBar.Value = 95;
            if (File.Exists(parsedModsFolder + "\\engine_config.xml"))
            {
                this.decreaseSoundMemory();
                this.removeBank("RelHax_1.bnk");
                this.removeBank("RelHax_2.bnk");
                this.removeBank("RelHax_3.bnk");
                this.removeBank("RelHax_4.bnk");
                this.removeBank("RelHaxChatShotcuts.bnk");
                this.removeBank("RelHaxMusicSources.bnk");
                this.removeBank("RelHaxGui.bnk");
                this.addBank("voiceover.bnk");
                this.removeDeclaration();
            }
            downloadProgress.Text = "Complete!";
            downloadProgressBar.Value = 100;
            Application.DoEvents();
        }
        
        private void backupCustom_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Feature Currently Disabled");
            return;
            /*this.reset();
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
            downloadProgress.Text = "Complete!";*/
        }
        
        private void restoreCustom_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Feature Currently Disabled");
            return;
            /*
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
            downloadProgress.Text = "Complete!";*/
        }

        void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            bytesIn = double.Parse(e.BytesReceived.ToString());
            totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            MBytesIn = (int)bytesIn / MBDivisor;
            MBytesTotal = (int)totalBytes / MBDivisor;
            downloadProgress.Text = "Downloaded " + MBytesIn + " MB" + " of " + MBytesTotal + " MB";
            downloadProgressBar.Value = e.ProgressPercentage;
            if(MBytesIn == 0 && MBytesTotal == 0)
            {
                this.downloadProgress.Text = "Complete!";
            }
        }

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //get rid of this, just let it look again and again
            //it won't do anything cause after unzipping it will
            //delete the file
            if (downloadQueue.Count > 0)
            {
                downloadCount++;
                this.downloadNumberCount.Text = "Downloading " + downloadCount + " of " + totalDownloadCount;
                this.download(downloadQueue[0].getURL(), downloadQueue[0].getZipFile());
                downloadQueue.RemoveAt(0);
                return;
            }
            else
            {
                //just let it keep running
                //since the files won't exist
                //use the overloaded method of cleanup(filepath,filename);
                //if (zipThread.ThreadState != ThreadState.Running)zipThread.Start();
                //this.lookForFilesToUnzip();
                if (workerIdle)
                {
                    workerIdle = false;
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }

        private void lookForFilesToUnzip()
        {
            //just let it keep running
            //since the files won't exist
            //use the overloaded method of cleanup(filepath,filename);

            //System.Threading.Thread.Sleep(100);
            if (File.Exists(tempPath + "\\RelHax.zip"))
            {
                numZipFiles++;
                this.unzip(tempPath + "\\RelHax.zip", tanksLocation);
                System.Threading.Thread.Sleep(10);
                this.cleanup(tempPath + "\\RelHax.zip");
            }
            if (File.Exists(tempPath + "\\gui.zip"))
            {
                numZipFiles++;
                this.unzip(tempPath + "\\gui.zip", tanksLocation);
                System.Threading.Thread.Sleep(10);
                this.cleanup(tempPath + "\\gui.zip");
            }
            if (File.Exists(tempPath + "\\6thSense.zip"))
            {
                numZipFiles++;
                this.unzip(tempPath + "\\6thSense.zip", tanksLocation);
                System.Threading.Thread.Sleep(10);
                this.cleanup(tempPath + "\\6thSense.zip");
            }
            if (File.Exists(tempPath + "\\origional.zip"))
            {
                numZipFiles = 1;
                totalZipFiles = 1;
                this.unzip(tempPath + "\\origional.zip", tanksLocation + "\\res");
                System.Threading.Thread.Sleep(10);
                this.cleanup(tempPath + "\\origional.zip");
                return;
            }
            if (File.Exists(tempPath + "\\version.zip"))
            {
                this.unzip(tempPath + "\\version.zip", tempPath + "\\versionCheck");
                //sleep
                this.cleanup(tempPath + "\\version.zip");
                if (isStartupCheck)
                {
                    this.checkForUpdatesOnStartup();
                    
                    isStartupCheck = false;
                    return;
                }
                else this.checkForUpdates();
            }
            if (File.Exists(appPath + "\\" + newVersionName))
            {
                string exePath = appPath + "\\" + newVersionName;
                if (exePath.Equals(Application.ExecutablePath)) return;
                if (!isAlreadyStarted)
                {
                    //try
                    //{
                        //System.Diagnostics.Process.Start(appPath + "\\" + newVersionName);
                        isAlreadyStarted = true;
                        //MessageBox.Show("New mod manager downloaded, you may now close the old version");
                        //closeIt = true;
                        //this.Close();
                    //}
                    /*catch (Win32Exception)
                    {
                        isAlreadyStarted = false;
                    }*/
                }
            }
            else
            {
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
            if (this.parseStrings() == null) this.displayError("The auto-detection failed. Please use the 'force manual' option", null);
            versionFolder = modAudioFolder + "\\relicModVersion";
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
            info.downloadedVersionInfo.Text = "gui sounds " + guiVersion + "\ningame voice sounds " + ingameVoiceVersion + "\n6th Sense " + sixthSenseVersion + "\ndownload manager " + managerVersion;
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
            }
        }

        private void checkmanagerUpdatesStartup()
        {
            this.reset();
            downloadQueue = new List<DownloadItem>();
            downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/version.zip"), tempPath + "\\version.zip"));
            totalDownloadCount = downloadQueue.Count;
            downloadNumberCount.Visible = true;
            this.downloader_DownloadFileCompleted(null, null);
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
            parsedModsFolder =  tanksLocation + "\\res_mods\\" + this.getFolderVersion(tanksLocation);
            stockAudioFolder = tanksLocation + "\\res\\audioww";
            stockTextFolder = tanksLocation + "\\res\\text\\lc_messages";
            modGuiFolder = parsedModsFolder + "\\gui\\soundModes";
            modTextFolder = parsedModsFolder + "\\text\\lc_messages";
            modAudioFolder = parsedModsFolder + "\\audioww";
            modGuiFolderBase = parsedModsFolder + "\\gui";
            modTextFolderBase = parsedModsFolder + "\\text";
            return "1";
        }

        private string getFolderVersion(string gamePath)
        {
            if (!File.Exists(tanksLocation + "\\version.xml"))
                return null;
            XmlDocument doc = new XmlDocument();
            doc.Load(tanksLocation + "\\version.xml");
            XmlNode node = doc.SelectSingleNode("//version.xml/version");
            string[] temp = node.InnerText.Split('#');
            string version = temp[0].Trim();
            version = version.Substring(2);
            return version;
        }
        //DO NOT USE THIS METHOD
        private String prepareForInstall(bool deleteAll)
        {
            //re-written for using the res-mods folder
            
            //delete the old files if they exist
            downloadProgress.Text = "Delete old files...";
            
            if(!Directory.Exists(modAudioFolder)) Directory.CreateDirectory(modAudioFolder);
            try
            {
                System.IO.Directory.Delete(modAudioFolder + "\\relicModVersion", true);
            }
            catch (DirectoryNotFoundException) { }

            //if the res_mods audio relic mod version folder does not exist, create it and not modify the res audio folder
            if (!File.Exists(modAudioFolder + "\\voiceover.bnk") || !File.Exists(modAudioFolder + "\\xvm.bnk"))
            {
                this.copyToResMods(true);
                File.WriteAllText(modAudioFolder + "\\RelHaxCopied.txt", "This folder was created by RelHax");
            }
            if (!File.Exists(modTextFolder + "\\achievements.mo"))
            {
                this.copyToResMods(false);
                File.WriteAllText(modTextFolder + "\\RelHaxCopied.txt", "This folder was created by RelHax");
            }

            return "We're all set!";
        }

        private void copyToResMods(bool audioOrText)
        {
            //true = audio
            int numProcessedFiles;
            string[] files;
            if (audioOrText)
            {
                numProcessedFiles = 0;
                files = System.IO.Directory.GetFiles(stockAudioFolder);
                foreach (string s in files)
                {
                    string theFileName = Path.GetFileName(s);
                    File.Copy(s, modAudioFolder + "\\" + theFileName);
                    numProcessedFiles++;
                    Application.DoEvents();
                    this.updateOnCopy(numProcessedFiles, files.Length, audioOrText);
                    Application.DoEvents();
                }
            }
            else
            {
                numProcessedFiles = 0;
                files = System.IO.Directory.GetFiles(stockTextFolder);
                Directory.CreateDirectory(modTextFolder);
                foreach (string s in files)
                {
                    string theFileName = Path.GetFileName(s);
                    File.Copy(s, modTextFolder + "\\" + theFileName);
                    numProcessedFiles++;
                    Application.DoEvents();
                    this.updateOnCopy(numProcessedFiles, files.Length, audioOrText);
                    Application.DoEvents();
                }
            }
            
        }

        private void updateOnCopy(int fileNumber, int totalFiles, bool audioOrText)
        {
            if (audioOrText)
            {
                downloadProgress.Text = "Copying audio files to res_mods, " + fileNumber + " of " + totalFiles;
            }
            else
            {
                downloadProgress.Text = "Copying text files to res_mods, " + fileNumber + " of " + totalFiles;
            }
            double realFileNumber = fileNumber;
            double realTotalFiles = totalFiles;
            double percent = realFileNumber / realTotalFiles;
            percent = percent * 100;
            int intPercent = (int)percent;
            downloadProgressBar.Value = intPercent;
        }

        private void displayError(String errorText, String errorHandle)
        {
            if(errorHandle == null) MessageBox.Show(errorText);
            else MessageBox.Show(errorText, errorHandle);
            downloadProgress.Text = "Aborted";
            //statusLabel.Text = "aborted";
        }

        private void download(Uri URL, string zipFile)
        {
            //delete temp if it's there
            if (File.Exists(zipFile)) File.Delete(zipFile);
            //download new zip file
            downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
            downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
            downloader.DownloadFileAsync(URL, zipFile, zipFile);
        }

        private void unzip(string zipFile, string extractFolder)
        {
            try
            {
                using (zip = ZipFile.Read(zipFile))
                {
                    double step = (100 / zip.Count);
                    double percentComplete = 0;
                    int realPercentComplete = 0;
                    foreach (ZipEntry file in zip)
                    {
                        try
                        {
                            System.Threading.Thread.Sleep(10);
                            file.Extract(extractFolder, ExtractExistingFileAction.OverwriteSilently);
                        }
                        catch (IOException e)
                        {
                            
                            //file.Extract(extractFolder, ExtractExistingFileAction.OverwriteSilently);
                            //e.Message;
                            //File.Delete()
                        }
                            percentComplete = percentComplete + step;
                            realPercentComplete = (int)percentComplete;
                            backgroundWorker1.ReportProgress(realPercentComplete);
                    }
                    zip.Dispose();
                }
            }
            catch (ZipException)
                {
                    MessageBox.Show("Error Downloading, please try again. If this is not the first\ntime you have seen this, tell Willster he messed up");
                }
        }

        private void cleanup()
        {
            if (File.Exists(tempPath + "\\relic.zip")) System.IO.File.Delete(tempPath + "\\relic.zip");
            if (File.Exists(tempPath + "\\relic_censored.zip")) System.IO.File.Delete(tempPath + "\\relic_censored.zip");
            if (File.Exists(tempPath + "\\gui.zip")) System.IO.File.Delete(tempPath + "\\gui.zip");
            if (File.Exists(tempPath + "\\6thSense.zip")) System.IO.File.Delete(tempPath + "\\6thSense.zip");
            if (File.Exists(tempPath + "\\origional.zip")) System.IO.File.Delete(tempPath + "\\origional.zip");
            if (File.Exists(tempPath + "\\version.zip")) System.IO.File.Delete(tempPath + "\\version.zip");
        }

        private void cleanup(string theZipFile)
        {
            if (File.Exists(theZipFile)) File.Delete(theZipFile);
            
        }

        private void reset()
        {
            //reset the UI and critical componets
            downloader.Dispose();
            downloader = new WebClient();
            this.downloadProgress.Text = "Idle";
            this.downloadProgressBar.Value = 0;
            downloadCount = 0;
            isAutoDetected = false;
            downloadNumberCount.Visible = false;
            repairMode = false;
            zip = new ZipFile();
            numZipFiles = 0;
            totalZipFiles = 0;
            workerIdle = true;
            totalProgress = 0;
            allProgress = 0;
            statusLabel.Text = "STATUS:";
        }

        private void createDownloadQueue()
        {
            downloadQueue = new List<DownloadItem>();
            if (this.features.relhaxBox.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHax.zip"), tempPath + "\\RelHax.zip"));
                totalZipFiles++;
            }
            if (this.features.relhaxBoxCen.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHaxCen.zip"), tempPath + "\\RelHax.zip"));
                totalZipFiles++;
            }
            /*if (this.features.guiBox.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/gui.zip"), tempPath + "\\gui.zip"));
                totalZipFiles++;
            }
            if (this.features.sixthSenseBox.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/6thSense.zip"), tempPath + "\\6thSense.zip"));
                totalZipFiles++;
            }*/
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
            selectWotFolder.Description = "Select the 'audio' folder where your modded audio files are held. (Either under res or res_mods)";
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
            newVersionName = "RelicModManager v" + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\manager version.txt") + ".exe";
            string new6thSenseVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\6thSense version.txt");
            //cleanup extracted folders
            Directory.Delete(tempPath + "\\versionCheck", true);
            if (File.Exists(tempPath + "\\version.zip")) System.IO.File.Delete(tempPath + "\\version.zip");
            //display what we found
            if (!ingameVoiceVersion.Equals(newIngameVoiceVersion))
            {
                if (ingameVoiceVersion.Equals("not installed")) { }
                else
                {
                    MessageBox.Show("Your ingame voice sounds are out of date. Please update.");
                    isOutofDate = true;
                }
            }
            if (!guiVersion.Equals(newGuiVersion))
            {
                if (guiVersion.Equals("not installed")) { }
                else
                {
                    MessageBox.Show("Your gui voice sounds are out of date. Please update.");
                    isOutofDate = true;
                }
            }
            if (!sixthSenseVersion.Equals(new6thSenseVersion))
            {
                if (sixthSenseVersion.Equals("not installed")) { }
                else
                {
                    MessageBox.Show("Your 6th Sense sounds is out of date. Please update.");
                    isOutofDate = true;
                }
            }
            if (!managerVersion.Equals(newManagerVersion))
            {
                //have it download the new version
                //when downloaded, move it to this application's directory
                //maintain file name structure
                //open the new one
                //close this one
                //try to delete the old one (nah)
                DialogResult result = MessageBox.Show("Your manager is out of date. Please Download the New Version", "Manager is out of date", MessageBoxButtons.YesNo);
                isOutofDate = true;
                if (result.Equals(DialogResult.Yes))
                {
                    //downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/" + newVersionName), appPath + "\\" + newVersionName));
                    //this.downloader_DownloadFileCompleted(null, null);
                    //downloadProgress.Text = "Downloading new version...";
                    //downloader.DownloadFile("https://dl.dropboxusercontent.com/u/44191620/RelicMod/" + newVersionName, appPath + "\\" + newVersionName);
                    System.Diagnostics.Process.Start("http://relicgaming.com/index.php?topic=697.0");
                    closeIt = true;
                    //isAlreadyStarted = true;
                }
                else { closeIt = true; }
                return;
            }
            if (!isOutofDate) MessageBox.Show("Your manager and/or sound mods are up to date");
        }

        private void checkForUpdatesOnStartup()
        {
            //get the version infos
            string newManagerVersion = "version " + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\manager version.txt");
            newVersionName = "RelicModManager v" + File.ReadAllText(tempPath + "\\versionCheck\\relicModVersion" + "\\manager version.txt") + ".exe";
            //cleanup extracted folders
            Directory.Delete(tempPath + "\\versionCheck", true);
            if (File.Exists(tempPath + "\\version.zip")) System.IO.File.Delete(tempPath + "\\version.zip");
            if (!managerVersion.Equals(newManagerVersion))
            {
                DialogResult result = MessageBox.Show("Your manager is out of date. Please Download the New Version", "Manager is out of date", MessageBoxButtons.YesNo);
                if (result.Equals(DialogResult.Yes))
                {
                    //downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/" + newVersionName), appPath + "\\" + newVersionName));
                    //this.downloader_DownloadFileCompleted(null, null);
                    //downloadProgress.Text = "Downloading new version...";
                    //downloader.DownloadFile("https://dl.dropboxusercontent.com/u/44191620/RelicMod/" + newVersionName, appPath + "\\" + newVersionName);
                    System.Diagnostics.Process.Start("http://relicgaming.com/index.php?topic=697.0");
                    closeIt = true;
                    //isAlreadyStarted = true;
                }
                else { closeIt = true; }
                return;
            }
        }

        private string autoFindTanks()
        {
            const string keyName = "HKEY_CURRENT_USER\\Software\\Classes\\.wotreplay\\shell\\open\\command";
            theObject = Registry.GetValue(keyName, "", -1);
            if (theObject == null) return null;
            isAutoDetected = true;
            tanksLocation = (string)theObject;
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
            isAutoDetected = false;
            return "all good";
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Application.DoEvents();
            wait.Show();
            wait.Close();
            Application.DoEvents();
            //ns = new NotSupported();
            //ns.ShowDialog();
            //this.Close();
            try
            {
                File.WriteAllText(tempPath + "\\RelHaxOneInstance.txt", "this file is open and cannot be deleted");
                File.OpenWrite(tempPath + "\\RelHaxOneInstance.txt");
            }
            catch (IOException)
            {
                wait.Close();
                this.Close();
            }
            try
            {
                if (File.Exists(tempPath + "\\relhax.zip")) File.Delete(tempPath + "\\relhax.zip");
                if (File.Exists(tempPath + "\\gui.zip")) File.Delete(tempPath + "\\gui.zip");
                if (File.Exists(tempPath + "\\6thSense.zip")) File.Delete(tempPath + "\\6thSense.zip");
                if (File.Exists(tempPath + "\\origional.zip")) File.Delete(tempPath + "\\origional.zip");
                if (File.Exists(tempPath + "\\version.zip")) File.Delete(tempPath + "\\version.zip");
                if (Directory.Exists(tempPath + "\\versionCheck")) Directory.Delete(tempPath + "\\versionCheck", true);
            }
            catch (IOException)
            {
                wait.Close();
                MessageBox.Show("Error: Another Instance of the relic mod manager is already running");
                this.Close();
            }
            Application.DoEvents();
            this.checkmanagerUpdatesStartup();
            Application.DoEvents();
            wait.Close();
            Application.DoEvents();
            this.Text = this.Text + managerVersion;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            this.lookForFilesToUnzip();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            downloadProgress.Text = e.ProgressPercentage + "% Complete extracting " + numZipFiles + " of " + totalZipFiles;
            //real progress
            /*allProgress++;
            if (totalZipFiles !=0) totalProgress = allProgress / totalZipFiles;
            int val = (int)totalProgress;*/
            //realistic progress
            if(numZipFiles == 1) downloadProgressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (repairMode)
            {
                this.reset();
                this.downloadMods_Click(null, null);
            }
            else
            {
                this.cleanup();
                this.downloadNumberCount.Visible = false;
                //patch the engine config file
                if (isInstalling)
                {
                    this.downloadProgress.Text = "patching xml file...";
                    if (!File.Exists(parsedModsFolder + "\\engine_config.xml"))
                    {
                        downloader.DownloadFile("https://dl.dropboxusercontent.com/u/44191620/RelicMod/stock_engine_config.xml", parsedModsFolder + "\\engine_config.xml");
                    }
                    this.increaseSoundMemory();
                    this.addBank("RelHax_1.bnk");
                    this.addBank("RelHax_2.bnk");
                    this.addBank("RelHax_3.bnk");
                    this.addBank("RelHax_4.bnk");
                    this.addBank("RelHaxChatShotcuts.bnk");
                    this.addBank("RelHaxMusicSources.bnk");
                    this.addBank("RelHaxGui.bnk");
                    this.removeDeclaration();
                    isInstalling = false;
                }
                this.downloadProgress.Text = "Complete!";
                downloadProgressBar.Value = 100;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (closeIt) this.Close();
        }

        //patches the engine config xml to increase the sound memory available
        private void increaseSoundMemory()
        {
            XmlDocument doc = new XmlDocument();
            int temp = 0;
            doc.Load(parsedModsFolder + "\\engine_config.xml");
            //patch defaultPool
            XmlNode defaultPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/defaultPool");
            temp = int.Parse(defaultPool.InnerText);
            if (temp < 32)
                defaultPool.InnerText = "32";
            //patch defaultPool
            XmlNode lowEnginePool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/lowEnginePool");
            temp = int.Parse(lowEnginePool.InnerText);
            if (temp < 24)
            lowEnginePool.InnerText = "24";
            //patch defaultPool
            XmlNode preparedPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/preparedPool");
            temp = int.Parse(preparedPool.InnerText);
            if (temp < 256)
            preparedPool.InnerText = "256";
            //patch defaultPool
            XmlNode streamingPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/streamingPool");
            temp = int.Parse(streamingPool.InnerText);
            if (temp < 8)
            streamingPool.InnerText = "8";
            //patch defaultPool
            XmlNode IOPoolSize = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/IOPoolSize");
            temp = int.Parse(IOPoolSize.InnerText);
            if (temp < 12)
            IOPoolSize.InnerText = "12";
            if (File.Exists("engine_config_test.xml")) File.Delete("engine_config_test.xml");
            doc.Save(parsedModsFolder + "\\engine_config.xml");
        }

        //patches the engine config xml to increase the sound memory available
        private void decreaseSoundMemory()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(parsedModsFolder + "\\engine_config.xml");
            //patch defaultPool
            XmlNode defaultPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/defaultPool");
                defaultPool.InnerText = "12";
            //patch defaultPool
            XmlNode lowEnginePool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/lowEnginePool");
                lowEnginePool.InnerText = "10";
            //patch defaultPool
            XmlNode preparedPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/preparedPool");
                preparedPool.InnerText = "106";
            //patch defaultPool
            XmlNode streamingPool = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/streamingPool");
                streamingPool.InnerText = "2";
            //patch defaultPool
            XmlNode IOPoolSize = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/memoryManager/IOPoolSize");
                IOPoolSize.InnerText = "4";
            if (File.Exists("engine_config_test.xml")) File.Delete("engine_config_test.xml");
            doc.Save(parsedModsFolder + "\\engine_config.xml");
        }

        //adds a sound bank to the engine config xml file
        private void addBank(string bankName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(parsedModsFolder + "\\engine_config.xml");

            //check to see if the list is empty
            XmlNode rel11 = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks/project/name");
            if (rel11 == null)
            //no soundbanks
            {
                XmlNode reff = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks");
                //create project node
                XmlElement project = doc.CreateElement("project");

                //create new soundbank node
                XmlElement rel2 = doc.CreateElement("name");
                rel2.InnerText = bankName;

                //insert soundbank into project
                project.InsertAfter(rel2, project.FirstChild);

                //insert project into voice_soundbanks
                reff.InsertAfter(project, reff.FirstChild);

                if (File.Exists(parsedModsFolder + "\\engine_config.xml")) File.Delete(parsedModsFolder + "\\engine_config.xml");
                doc.Save(parsedModsFolder + "\\engine_config.xml");
                return;
            }

            //check to see if it's already there
            XmlNodeList currentSoundBanks = doc.SelectNodes("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks/project/name");
            foreach (XmlElement e in currentSoundBanks)
            {
                if (e.InnerText.Equals(bankName))
                    return;
            }

            //find and replace voiceover.bnk first
            XmlNodeList rel1 = doc.SelectNodes("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks/project/name");
            foreach (XmlElement e in rel1)
            {
                if (e.InnerText.Equals("voiceover.bnk"))
                {
                    e.InnerText = bankName;
                    if (File.Exists(parsedModsFolder + "\\engine_config.xml")) File.Delete(parsedModsFolder + "\\engine_config.xml");
                    doc.Save(parsedModsFolder + "\\engine_config.xml");
                    return;
                }
            }
            
            {
                //create refrence node
                XmlNode reff = doc.SelectSingleNode("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks");

                //create project node
                XmlElement project = doc.CreateElement("project");

                //create new soundbank node
                XmlElement rel2 = doc.CreateElement("name");
                rel2.InnerText = bankName;

                //insert soundbank into project
                project.InsertAfter(rel2, project.FirstChild);

                //insert project into voice_soundbanks
                reff.InsertAfter(project, reff.FirstChild);
            }
            if (File.Exists("engine_config_test.xml")) File.Delete("engine_config_test.xml");
            doc.Save(parsedModsFolder + "\\engine_config.xml");
        }

        //removes a sound bank to the engine config xml file
        private void removeBank(string bankName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(parsedModsFolder + "\\engine_config.xml");
            //check to see if it's already there
            XmlNodeList nl = doc.SelectNodes("//engine_config.xml/soundMgr/WWISE_adv_profile/voice_soundbanks/project/name");
            foreach (XmlElement e in nl)
            {
                if (e.InnerText.Equals(bankName))
                    e.RemoveAll();
            }
            //save
            if (File.Exists(parsedModsFolder + "\\engine_config.xml")) File.Delete(parsedModsFolder + "\\engine_config.xml");
            doc.Save(parsedModsFolder + "\\engine_config.xml");

            //remove empty elements
            XDocument doc2 = XDocument.Load(parsedModsFolder + "\\engine_config.xml");
            doc2.Descendants().Elements("project").Where(e => string.IsNullOrEmpty(e.Value)).Remove();
            if (File.Exists(parsedModsFolder + "\\engine_config.xml")) File.Delete(parsedModsFolder + "\\engine_config.xml");
            doc2.Save(parsedModsFolder + "\\engine_config.xml");
        }

        //removes the declaration statement at the start of the doc
        private void removeDeclaration()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(parsedModsFolder + "\\engine_config.xml");
            foreach (XmlNode node in doc)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    doc.RemoveChild(node);
                }
            }
            doc.Save(parsedModsFolder + "\\engine_config.xml");
        }

        //checks for any files and folders left in the directory
        private int anythingElseRemaining(string folderName)
        {
            int total = 0;
            if (!Directory.Exists(folderName))
                return -1;
            total = total + Directory.GetFiles(folderName).Count();
            total = total + Directory.GetDirectories(folderName).Count();
            return total;
        }
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
