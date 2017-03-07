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
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Drawing.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RelicModManager
{
    public partial class MainWindow : Form
    {
        //all instance variables required to be up here
        private FolderBrowserDialog selectWotFolder = new FolderBrowserDialog();
        private WebClient downloader = new WebClient();
        private string modAudioFolder;//res_mods/versiondir/audioww
        private string tempPath = Path.GetTempPath();//C:/users/userName/appdata/local/temp
        private const int MBDivisor = 1048576;
        private string managerVersion = "version 20.2";
        private string tanksLocation;//sample:  c:/games/World_of_Tanks
        private SelectFeatures features = new SelectFeatures();
        //queue for downloading mods
        private List<DownloadItem> downloadQueue;
        //directory path of where the application was started form
        private string appPath = Application.StartupPath;
        //where all the downloaded mods are placed
        private string downloadPath = Application.StartupPath + "\\RelHaxDownloads";
        private string parsedModsFolder;//0.9.x.y.z
        //DEPRECATED: /res_mods/version/gui
        private string modGuiFolder;
        private string modGuiFolderBase;
        private string customUserMods;
        ZipFile zip;
        //timer to measure download speed
        Stopwatch sw = new Stopwatch();
        private string downloadURL = "http://willster419.atwebpages.com/Applications/RelHaxModPack/mods/";
        private List<Catagory> parsedCatagoryLists;
        private List<Mod> modsToInstall;
        private List<Config> configsToInstall;
        private List<Patch> patchList;
        private List<Dependency> dependencies;
        //installing the RelhaxModpack of the Relhax Sound Mod
        bool modPack;
        string tempOldDownload;
        private List<Mod> userMods;
        int numFilesToProcessInt = 0;
        int numFilesToCopyDeleteExtract = 0;
        bool userExtract = false;
        //childProgresBar.Maximum
        private int childMaxProgres;
        //childProgresBar.Value
        private int childCurrentProgres;
        private bool isParrentDone;
        //current file being processed in the zip archive
        private string currentZipEntry;
        //seperate thread for the extraction
        BackgroundWorker extractworker;
        private string versionSave;
        private FirstLoadHelper helper;
        string helperText;
        string currentModDownloading;
        private enum InstallState
        {
            error = -1,
            idle = 0,
            deleteBackupResMods = 1,
            deleteBackupMods = 2,
            backupResMods = 3,
            backupMods = 4,
            modSelection = 5,
            downloading = 6,
            deleteResMods = 7,
            deleteMods = 8,
            extractRelhaxMods = 9,
            patchRelhaxMods = 10,
            extractUserMods = 11,
            patchUserMods = 12,
            installFonts = 13,
            uninstallResMods = 14,
            uninstallMods = 15
        };
        private InstallState state = InstallState.idle;
        private string tanksVersion;//0.9.x.y
        BackgroundWorker deleteworker;
        BackgroundWorker copyworker;
        //list to maintain the refrence lines in a json patch
        List<double> timeRemainArray = new List<double>();
        //the ETA variable for downlading
        double actualTimeRemain = 0;
        
        //The constructur for the application
        public MainWindow()
        {
            InitializeComponent();
        }
        //install RelHax
        private void installRelhax_Click(object sender, EventArgs e)
        {
            this.appendToLog("Install Relhax Sound Mod started");
            modPack = false;
            downloadPath = Application.StartupPath + "\\RelHaxSoundMod";
            childProgressBar.Maximum = 100;
            //reset the interface
            this.reset();
            //ask the user which features s/he wishes to install
            this.appendToLog("Asking the user parts of mod to install");
            this.features.ShowDialog();
            if (features.canceling)
            {
                downloadProgress.Text = "Canceled";
                this.appendToLog("Install Cancled");
                return;
            }
            //attempt to locate the tanks directory
            if (this.autoFindTanks() == null || Settings.forceManuel)
            {
                this.appendToLog("Auto find tanks failed or manual tanks locating selected");
                if (this.manuallyFindTanks() == null)
                {
                    this.appendToLog("manuallyFindTanks returned null");
                    return;
                }
            }
            //parse all strings
            if (this.parseStrings() == null)
            {
                this.appendToLog("WARNING: parseStrings() returned null");
                this.displayError("The auto-detection failed. Please use the 'force manual' option", null);
                return;
            }

            if (!Directory.Exists(downloadPath)) Directory.CreateDirectory(downloadPath);

            sw.Reset();
            sw.Start();

            this.createDownloadQueue();
            this.appendToLog("Relhax Sound Mod Install Moving to download method");
            this.downloader_DownloadFileCompleted(null, null);
        }
        //uninstall RelHax
        private void uninstallRelhax_Click(object sender, EventArgs e)
        {
            this.appendToLog("Uninstall of Relhax Sound Mod started");
            this.reset();
            childProgressBar.Maximum = 100;
            downloadProgress.Text = "Preparing...";
            if (this.autoFindTanks() == null || Settings.forceManuel)
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
            childProgressBar.Value = 20;
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
            childProgressBar.Value = 50;
            Application.DoEvents();

            downloadProgress.Text = "Removing gui stuff...";
            childProgressBar.Value = 85;
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
            childProgressBar.Value = 95;
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
            childProgressBar.Value = 100;
            Application.DoEvents();
            this.appendToLog("Relhax Sound Mod uninstall complete");
        }
        //handler for the mod download file progress
        void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double msElapsed = sw.Elapsed.TotalMilliseconds;
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            int MBytesIn = (int)bytesIn / MBDivisor;
            int MBytesTotal = (int)totalBytes / MBDivisor;
            downloadProgress.Text = "Downloading " + currentModDownloading + " ("+ MBytesIn + " MB" + " of " + MBytesTotal + " MB)";
            childProgressBar.Value = e.ProgressPercentage;
            speedLabel.Text = string.Format("{0} MB/s", (e.BytesReceived / 1048576d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            double totalTimeToDownload =  MBytesTotal / (e.BytesReceived / 1048576d / sw.Elapsed.TotalSeconds);
            double timeRemain = totalTimeToDownload - sw.Elapsed.TotalSeconds;
            timeRemainArray.Add(timeRemain);
            if (timeRemainArray.Count == 10)
            {
                double timeAverageRemain = 0;
                foreach (double d in timeRemainArray)
                    timeAverageRemain += d;
                actualTimeRemain = timeAverageRemain / 10;
                timeRemainArray.Clear();
            }
            speedLabel.Text = speedLabel.Text + " ETA: " + Math.Round(actualTimeRemain,0) + " sec";
            if (MBytesIn == 0 && MBytesTotal == 0)
            {
                //this.downloadProgress.Text = "Complete!";
            }
        }
        //handler for the mod download file complete event
        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!modPack)
            {
                //old relhax sound mod code
                if (downloadQueue.Count != 0)
                {
                    if (File.Exists(downloadQueue[0].zipFile)) File.Delete(downloadQueue[0].zipFile);
                    //download new zip file
                    downloader = new WebClient();
                    downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                    downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                    downloader.DownloadFileAsync(downloadQueue[0].URL, downloadQueue[0].zipFile);
                    //currentModDownloading = downloadQueue[0].zipFile;
                    downloadQueue.RemoveAt(0);
                    parrentProgressBar.Value++;
                    return;
                }
                if (downloadQueue.Count == 0)
                {
                    //tell it to extract the zip files
                    this.appendToLog("Relhax sound mod downloading finished, moving to zip extraction");
                    this.extractZipFiles();
                }
            }
            else
            {
                //new relhax modpack code
                if (e != null && e.Error != null && e.Error.Message.Equals("The remote server returned an error: (404) Not Found."))
                {
                    //404
                    this.appendToLog("ERROR: " + tempOldDownload + " failed to download");
                    MessageBox.Show("Failed to download " + tempOldDownload + ". If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it exits");
                    Application.Exit();
                }
                if (downloadQueue.Count != 0)
                {
                    //for the next file in the queue, delete it.
                    if (File.Exists(downloadQueue[0].zipFile)) File.Delete(downloadQueue[0].zipFile);
                    //download new zip file
                    downloader = new WebClient();
                    downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                    downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                    downloader.Proxy = null;
                    timeRemainArray.Clear();
                    actualTimeRemain = 0;
                    sw.Reset();
                    sw.Start();
                    downloader.DownloadFileAsync(downloadQueue[0].URL, downloadQueue[0].zipFile);
                    tempOldDownload = Path.GetFileName(downloadQueue[0].zipFile);
                    this.appendToLog("downloading " + tempOldDownload);
                    currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile);
                    if (currentModDownloading.Length >= 30)
                    {
                        currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile).Substring(0, 23) + "...";
                    }
                    downloadQueue.RemoveAt(0);
                    parrentProgressBar.Value++;
                    return;
                }
                if (downloadQueue.Count == 0)
                {
                    if (state == InstallState.downloading)
                    {
                        //just finished downloading, needs to start extracting
                        this.appendToLog("Downloading finished");
                        if (Settings.cleanInstallation)
                        {
                            state = InstallState.deleteResMods;
                            this.appendToLog("CleanInstallCB checked, running backgroundDelete(" + tanksLocation + "\\res_mods)");
                            //delete everything in res_mods
                            if (Directory.Exists(tanksLocation + "\\res_mods")) this.backgroundDelete(tanksLocation + "\\res_mods");
                            return;
                        }
                        this.appendToLog("CleanInstallCB not checked, moving to extraction");
                        state = InstallState.extractRelhaxMods;
                        this.backgroundExtract(false);
                        return;
                    }
                    else if (state == InstallState.extractRelhaxMods)
                    {
                        if (Settings.cleanInstallation)
                        {
                            state = InstallState.deleteResMods;
                            this.appendToLog("CleanInstallCB checked, running backgroundDelete(" + tanksLocation + "\\res_mods)");
                            //delete everything in res_mods
                            if (Directory.Exists(tanksLocation + "\\res_mods")) this.backgroundDelete(tanksLocation + "\\res_mods");
                            return;
                        }
                        state = InstallState.extractRelhaxMods;
                        this.appendToLog("CleanInstallCB not checked, moving to extraction");
                        this.backgroundExtract(false);
                        return;
                    }
                    else if (state == InstallState.extractUserMods)
                    {

                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        //Extracts the zip files downloaded for the Relhax Sound Mod (not modpack)
        private void extractZipFiles()
        {
            speedLabel.Text = "Extracting...";
            this.appendToLog("Starting Relhax Sound Mod Extraction");
            string[] fileNames = Directory.GetFiles(downloadPath);
            parrentProgressBar.Maximum = fileNames.Count();
            parrentProgressBar.Value = 0;
            foreach (string fName in fileNames)
            {
                this.appendToLog("Extracting " + fName);
                this.unzip(fName, tanksLocation);
                parrentProgressBar.Value++;
            }
            this.appendToLog("Finished extracting, moving to patching");
            this.patchStuff();
        }
        //extracts the zip files downloaded fro the Relhax Modpack
        private void extractZipFilesModPack()
        {
            speedLabel.Text = "Extracting RelHax Mods...";
            this.appendToLog("Starting Relhax Modpack Extraction");
            parrentProgressBar.Maximum = modsToInstall.Count + configsToInstall.Count + dependencies.Count;
            parrentProgressBar.Value = 0;
            string downloadedFilesDir = Application.StartupPath + "\\RelHaxDownloads\\";
            //extract dependencies
            foreach (Dependency d in dependencies)
            {
                this.appendToLog("Extracting Dependency " + d.dependencyZipFile);
                if (!d.dependencyZipFile.Equals("")) this.unzip(downloadedFilesDir + d.dependencyZipFile, tanksLocation);
                parrentProgressBar.Value++;
            }
            //extract mods
            foreach (Mod m in modsToInstall)
            {
                this.appendToLog("Extracting Mod " + m.modZipFile);
                if (!m.modZipFile.Equals("")) this.unzip(downloadedFilesDir + m.modZipFile, tanksLocation);
                parrentProgressBar.Value++;
            }
            //extract configs
            foreach (Config c in configsToInstall)
            {
                this.appendToLog("Extracting Config " + c.zipConfigFile);
                if (!c.zipConfigFile.Equals("")) this.unzip(downloadedFilesDir + c.zipConfigFile, tanksLocation);
                parrentProgressBar.Value++;
            }
            this.appendToLog("Finished Relhax Modpack Extraction");
        }
        //extract all the selected user mods
        private void extractZipFilesUser()
        {
            speedLabel.Text = "Extracting User Mods...";
            this.appendToLog("Starting Relhax Modpack User Mod Extraction");
            parrentProgressBar.Maximum = userMods.Count;
            parrentProgressBar.Value = 0;
            string downloadedFilesDir = Application.StartupPath + "\\RelHaxUserMods\\";
            foreach (Mod m in userMods)
            {
                if (m.modChecked)
                {
                    this.appendToLog("Exracting " + Path.GetFileName(m.modZipFile));
                    this.unzip(downloadedFilesDir + Path.GetFileName(m.modZipFile), tanksLocation);
                    parrentProgressBar.Value++;
                }
            }
            this.appendToLog("Finished Extracting Relhax Modpack User Mod Extraction");
        }
        //main method for executing all Modpack and user patches
        private void patchFiles()
        {
            speedLabel.Text = "Patching...";
            Application.DoEvents();
            this.appendToLog("Starting to patch Relhax Mod Pack");
            //don't do anything if the file does not exist
            if (!Directory.Exists(tanksLocation + "\\_patch"))
                return;
            //get every patch file in the folder
            string[] patchFiles = Directory.GetFiles(tanksLocation + "\\_patch");
            //get any other old patches out of memory
            patchList.Clear();
            for (int i = 0; i < patchFiles.Count(); i++)
            {
                //add patches to patchList
                this.createPatchList(patchFiles[i]);
            }
            //the actual patch method
            foreach (Patch p in patchList)
            {
                downloadProgress.Text = "patching " + p.file;
                Application.DoEvents();
                if (p.type.Equals("regx"))
                {
                    string temp = null;
                    int tempp = 0;
                    if (p.lines != null)
                    {
                        temp = p.lines[0];
                        tempp = int.Parse(temp);
                    }
                    if (p.lines == null)
                    {
                        //perform regex patch on entire file, line by line
                        this.appendToLog("Regex patch, all lines, line by line, " + p.file + ", " + p.search + ", " + p.replace);
                        this.RegxPatch(p.file, p.search, p.replace);
                    }
                    else if (p.lines.Count() == 1 && tempp == -1)
                    {
                        //perform regex patch on entire file, as one whole string
                        this.appendToLog("Regex patch, all lines, whole file, " + p.file + ", " + p.search + ", " + p.replace);
                        this.RegxPatch(p.file, p.search, p.replace, -1);
                    }
                    else
                    {
                        foreach (string s in p.lines)
                        {
                            //perform regex patch on specific file lines
                            //will need to be a standard for loop BTW
                            this.appendToLog("Regex patch, line " + s + ", " + p.file + ", " + p.search + ", " + p.replace);
                            this.RegxPatch(p.file, p.search, p.replace, int.Parse(s));
                        }
                    }
                }
                else if (p.type.Equals("xml"))
                {
                    //perform xml patch
                    this.appendToLog("Xml patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                    this.xmlPatch(p.file, p.path, p.mode, p.search, p.replace);
                }
                else if (p.type.Equals("json"))
                {
                    //perform json patch
                    this.appendToLog("Json patch, " + p.file + ", " + p.path + ", " + p.replace);
                    this.jsonPatch(p.file, p.path, p.replace);
                }
            }
            //all done, delete the patch folder
            if (Directory.Exists(tanksLocation + "\\_patch"))
                Directory.Delete(tanksLocation + "\\_patch", true);
            this.appendToLog("Patching done for Relhax Mod Pack");
        }
        //installs all fonts in the fonts folder, user and custom
        private void installFonts()
        {
            speedLabel.Text = "Installing Fonts...";
            if (!Directory.Exists(tanksLocation + "\\_fonts"))
            {
                //no fonts to install, done display
                speedLabel.Text = "";
                downloadProgress.Text = "Done!";
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                state = InstallState.idle;
                return;
            }
            string[] fonts = Directory.GetFiles(tanksLocation + "\\_fonts");
            if (fonts.Count() == 0)
            {
                //done display
                speedLabel.Text = "";
                downloadProgress.Text = "Done!";
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                state = InstallState.idle;
                return;
            }
            //convert the array to a list
            List<String> fontsList = new List<string>();
            foreach (string s in fonts)
            {
                fontsList.Add(s);
            }
            //removes any already installed fonts
            for (int i = 0; i < fontsList.Count; i++)
            {
                //get just the name of the font, assumes the name of the font is the filename as well
                string fName = Path.GetFileNameWithoutExtension(fontsList[i]);
                //get a list of installed fonts
                var fontsCollection = new InstalledFontCollection();
                foreach (var fontFamiliy in fontsCollection.Families)
                {
                    //check if the font is installed
                    if (fontFamiliy.Name == fName)
                    {
                        //font in installed
                        fontsList.RemoveAt(i);
                    }
                }
            }
            //re-check the fonts to install list
            if (fontsList.Count == 0)
            {
                //done display
                speedLabel.Text = "";
                downloadProgress.Text = "Done!";
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                state = InstallState.idle;
                return;
            }
            this.appendToLog("Installing fonts");
            DialogResult dr = DialogResult.No;
            if (Program.autoInstall)
            {
                //assume rights to install
                dr = DialogResult.Yes;
            }
            else
            {
                dr = MessageBox.Show("Do you have admin rights?", "Admin to install fonts?", MessageBoxButtons.YesNo);
            }
            if (dr == DialogResult.Yes)
            {
                this.extractEmbeddedResource(tanksLocation + "\\_fonts", "RelicModManager", new List<string>() { "FontReg.exe" });
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "FontReg.exe";
                info.UseShellExecute = true;
                info.Verb = "runas"; // Provides Run as Administrator
                info.Arguments = "/copy";
                info.WorkingDirectory = tanksLocation + "\\_fonts";
                Process installFontss = new Process();
                installFontss.StartInfo = info;
                bool isAdmin = this.isAdministrator();
                try
                {
                    installFontss.Start();
                    installFontss.WaitForExit();
                }
                catch (Win32Exception)
                {
                    this.appendToLog("ERROR: could not start font installer");
                    MessageBox.Show("Unable to install fonts. Some mods may not work properly. Fonts are located in " + tanksLocation + "\\_fonts. Eithor install them yourself or run this again as Administrator");
                    return;
                }
                if (Directory.Exists(tanksLocation + "\\_fonts"))
                    Directory.Delete(tanksLocation + "\\_fonts", true);
                speedLabel.Text = "";
                downloadProgress.Text = "Done!";
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                state = InstallState.idle;
                this.appendToLog("Fonts Installed Successfully");
            }
        }
        //checks to see if the application is indeed in admin mode
        public bool isAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isPowerUser = principal.IsInRole(WindowsBuiltInRole.PowerUser);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                return (isPowerUser || isAdmin);
            }
            this.appendToLog("WARNING: user is not admin or power user");
            return false;
        }
        //method to check for updates to the application on startup
        private void checkmanagerUpdates()
        {
            this.appendToLog("Starting check for application updates");
            WebClient updater = new WebClient();
            updater.Proxy = null;
            string versionSaveLocation = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 4) + "_version.txt";
            if (File.Exists(versionSaveLocation)) File.Delete(versionSaveLocation);
            string version = updater.DownloadString("http://willster419.atwebpages.com/Applications/RelHaxModPack/manager version.txt");
            versionSave = version;
            if (!version.Equals(managerVersion))
            {
                this.appendToLog("exe is out of date. displaying user update window");
                //out of date
                VersionInfo vi = new VersionInfo();
                vi.ShowDialog();
                DialogResult result = vi.result;
                if (result.Equals(DialogResult.Yes))
                {
                    this.appendToLog("User accepted downloading new version");
                    //download new version
                    sw.Reset();
                    sw.Start();
                    string newExeName = Application.StartupPath + "\\RelicModManager " + version + ".exe";
                    updater.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updater_DownloadProgressChanged);
                    updater.DownloadFileCompleted += new AsyncCompletedEventHandler(updater_DownloadFileCompleted);
                    if (File.Exists(newExeName)) File.Delete(newExeName);
                    updater.DownloadFileAsync(new Uri("http://willster419.atwebpages.com/Applications/RelHaxModPack/RelicModManager.exe"), newExeName);
                    this.appendToLog("New application download started");
                }
                else
                {
                    this.appendToLog("User declined downlading new version");
                    //close the application
                    this.Close();
                }
            }
        }
        //handler for when the update download is complete
        void updater_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message.Equals("The remote server returned an error: (404) Not Found."))
            {
                //404
                this.appendToLog("ERROR: unable to download new application version");
                MessageBox.Show("Unable to download new version, exiting!");
                this.Close();
            }
            string versionSaveLocation = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 4) + "_version.txt";
            string version = versionSave;
            string newExeName = Application.StartupPath + "\\RelicModManager " + version + ".exe";
            try
            {
                System.Diagnostics.Process.Start(newExeName);
            }
            catch (Win32Exception)
            {
                this.appendToLog("WARNING: could not start new application version");
                MessageBox.Show("Unable to start application, but it is located in \n" + newExeName);
            }
            this.Close();
        }
        //handler for the update download progress
        void updater_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            int MBytesIn = (int)bytesIn / MBDivisor;
            int MBytesTotal = (int)totalBytes / MBDivisor;
            downloadProgress.Text = "Downloaded " + MBytesIn + " MB" + " of " + MBytesTotal + " MB";
            childProgressBar.Value = e.ProgressPercentage;
            speedLabel.Text = string.Format("{0} MB/s", (e.BytesReceived / 1048576d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            if (MBytesIn == 0 && MBytesTotal == 0)
            {
                this.downloadProgress.Text = "Complete!";
            }
        }
        //parses all instance strings to be used for (un)install processes
        private String parseStrings()
        {
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
            this.appendToLog("tanksLocation parsed as " + tanksLocation);
            parsedModsFolder = tanksLocation + "\\res_mods\\" + this.getFolderVersion(tanksLocation);
            this.appendToLog("tanks mods version parsed as " + parsedModsFolder);
            modGuiFolder = parsedModsFolder + "\\gui\\soundModes";
            modAudioFolder = parsedModsFolder + "\\audioww";
            modGuiFolderBase = parsedModsFolder + "\\gui";
            customUserMods = Application.StartupPath + "\\RelHaxUserMods";
            this.appendToLog("customUserMods parsed as " + Application.StartupPath + "\\RelHaxUserMods");
            return "1";
        }
        //gets the version of tanks that this is, in the format
        //of the res_mods version folder i.e. 0.9.17.0.3
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
        //Method for displaying an error message
        private void displayError(String errorText, String errorHandle)
        {
            if (errorHandle == null) MessageBox.Show(errorText);
            else MessageBox.Show(errorText, errorHandle);
            downloadProgress.Text = "Aborted";
        }
        //main unzip worker method
        private void unzip(string zipFile, string extractFolder)
        {
            if (!modPack)
            {
                //regualr sound mod
                string thisVersion = this.getFolderVersion(null);
                zip = ZipFile.Read(zipFile);
                //for this zip file instance, for each entry in the zip file,
                //change the "versiondir" path to this version of tanks
                for (int i = 0; i < zip.Entries.Count; i++)
                {
                    if (Regex.IsMatch(zip[i].FileName, "versiondir"))
                    {
                        zip[i].FileName = Regex.Replace(zip[i].FileName, "versiondir", thisVersion);
                    }
                }
                zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);
                childProgressBar.Maximum = zip.Entries.Count;
                childProgressBar.Value = 0;
                zip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
            }
            else
            {
                //modpack
                string thisVersion = this.getFolderVersion(null);
                //if (File.Exists(zipFile))
                zip = ZipFile.Read(zipFile);
                //for this zip file instance, for each entry in the zip file,
                //change the "versiondir" path to this version of tanks
                for (int i = 0; i < zip.Entries.Count; i++)
                {
                    if (Regex.IsMatch(zip[i].FileName, "versiondir"))
                    {
                        zip[i].FileName = Regex.Replace(zip[i].FileName, "versiondir", thisVersion);
                    }
                }
                zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);
                zip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
            }

        }
        //handler for when progress is made in extracting a zip file
        void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            childCurrentProgres = e.EntriesExtracted;
            childMaxProgres = zip.Entries.Count;
            isParrentDone = false;
            if (e.CurrentEntry != null)
            {
                currentZipEntry = e.CurrentEntry.FileName;
            }
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll)
            {
                isParrentDone = true;
                zip.Dispose();
            }
            if (modPack)
                extractworker.ReportProgress(0);
        }
        //DEPRECATED: Cleans up after legacy installers
        private void cleanup()
        {
            this.appendToLog("WARNING: using deprecated method cleanup()");
            if (File.Exists(tempPath + "\\relic.zip")) System.IO.File.Delete(tempPath + "\\relic.zip");
            if (File.Exists(tempPath + "\\relic_censored.zip")) System.IO.File.Delete(tempPath + "\\relic_censored.zip");
            if (File.Exists(tempPath + "\\gui.zip")) System.IO.File.Delete(tempPath + "\\gui.zip");
            if (File.Exists(tempPath + "\\6thSense.zip")) System.IO.File.Delete(tempPath + "\\6thSense.zip");
            if (File.Exists(tempPath + "\\origional.zip")) System.IO.File.Delete(tempPath + "\\origional.zip");
            if (File.Exists(tempPath + "\\version.zip")) System.IO.File.Delete(tempPath + "\\version.zip");
            if (Directory.Exists(tempPath + "\\versionCheck")) Directory.Delete(tempPath + "\\versionCheck", true);
        }
        //reset the UI and critical componets
        private void reset()
        {
            downloadProgress.Text = "Idle";
            childProgressBar.Value = 0;
            parrentProgressBar.Value = 0;
            statusLabel.Text = "STATUS:";
        }
        //Checks for which parts of the RelHax sound mod it is to download
        private void createDownloadQueue()
        {
            downloadQueue = new List<DownloadItem>();
            //install RelHax
            if (this.features.relhaxBox.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHax.zip"), downloadPath + "\\RelHax.zip"));
            }
            //install RelHax Censored version
            if (this.features.relhaxBoxCen.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHaxCen.zip"), downloadPath + "\\RelHax.zip"));
            }
            parrentProgressBar.Maximum = downloadQueue.Count;
            parrentProgressBar.Minimum = 0;
        }
        //checks the registry to get the location of where WoT is installed
        //idea: if the user can open replay files, this can get the WoT exe filepath
        private string autoFindTanks()
        {
            object theObject = new object();
            const string keyName = "HKEY_CURRENT_USER\\Software\\Classes\\.wotreplay\\shell\\open\\command";
            theObject = Registry.GetValue(keyName, "", -1);
            if (theObject == null) return null;
            tanksLocation = (string)theObject;
            tanksLocation = tanksLocation.Substring(1);
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 6);
            if (!File.Exists(tanksLocation)) return null;
            return (string)theObject;
        }
        //prompts the user to specify where the "WorldOfTanks.exe" file is
        //return the file path and name of "WorldOfTanks.exe"
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
        //handelr for before the window is displayed
        private void MainWindow_Load(object sender, EventArgs e)
        {
            //set window header text to current version so user knows
            this.Text = this.Text + managerVersion;
            //show the wait screen
            PleaseWait wait = new PleaseWait();
            wait.Show();
            WebRequest.DefaultWebProxy = null;
            wait.loadingDescLabel.Text = "Checking for single instance...";
            Application.DoEvents();
            this.appendToLog("|------------------------------------------------------------------------------------------------|");
            this.appendToLog("|RelHax ModManager " + managerVersion);
            this.appendToLog("|Built on 03/06/2017, running at " + DateTime.Now);
            this.appendToLog("|Running on " + System.Environment.OSVersion.ToString());
            this.appendToLog("|------------------------------------------------------------------------------------------------|");
            //enforces a single instance of the program
            try
            {
                File.WriteAllText(tempPath + "\\RelHaxOneInstance.txt", "this file is open and cannot be deleted");
                File.OpenWrite(tempPath + "\\RelHaxOneInstance.txt");
                this.appendToLog("Successfully made single instance text file");
            }
            //catching an exeption means that this is not the only instance open
            catch (IOException)
            {
                wait.Close();
                MessageBox.Show("CRITICAL: Another Instance of the relic mod manager is already running");
                this.Close();
            }
            wait.loadingDescLabel.Text = "Doing Random Cleanup...";
            Application.DoEvents();
            //cleans up after any previous relicModManager versions
            this.cleanup();
            wait.loadingDescLabel.Text = "Checking for updates...";
            Application.DoEvents();
            this.checkmanagerUpdates();
            wait.loadingDescLabel.Text = "Verifying Directory Structure...";
            Application.DoEvents();
            //create directory structures
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxDownloads")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxDownloads");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxUserMods")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxUserMods");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxModBackup")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxModBackup");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxUserConfigs")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxUserConfigs");
            //load settings
            wait.loadingDescLabel.Text = "Loading Settings...";
            this.appendToLog("Loading settings");
            Settings.loadSettings();
            this.applySettings();
            if (Program.testMode)
            {
                this.appendToLog("Test Mode is ON, loading local modInfo.xml");
            }
            if (Program.autoInstall)
            {
                this.appendToLog("Auto Install is ON, checking for config pref xml at " + Application.StartupPath + "\\RelHaxUserConfigs\\" + Program.configName);
                if (!File.Exists(Application.StartupPath + "\\RelHaxUserConfigs\\" + Program.configName))
                {
                    this.appendToLog("ERROR: " + Program.configName + " does NOT exist, loading in regualar mode");
                    MessageBox.Show("ERROR: " + Program.configName + " does NOT exist, loading in regualar mode");

                    Program.autoInstall = false;
                }
                if (!Settings.cleanInstallation)
                {
                    this.appendToLog("ERROR: clean installation is set to false. This must be set to true for auto install to work");
                    MessageBox.Show("ERROR: clean installation is set to false. You must set this to true and restart the application for auto install to work.");
                    Program.autoInstall = false;
                }
            }
            if (Program.autoInstall)
            {
                this.installRelhaxMod_Click(null, null);
            }
            if (Settings.firstLoad)
            {
                helper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
                helperText = helper.helperText.Text;
                helper.Show();
                Settings.firstLoad = false;
            }
            wait.Close();
            state = InstallState.idle;
            Application.DoEvents();
        }
        //starts the patching process for the Relhax Sound Mod
        private void patchStuff()
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
            this.downloadProgress.Text = "Complete!";
            childProgressBar.Value = childProgressBar.Maximum;
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
        //when the "visit form page" link is clicked. the link clicked handler
        private void formPageLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://relicgaming.com/index.php?topic=697.0");
        }
        //method to patch a part of an xml file
        //fileLocation is relative to res_mods folder
        private void xmlPatch(string filePath, string xpath, string mode, string search, string replace)
        {
            if (Regex.IsMatch(filePath, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                filePath = tanksLocation + filePath;
            }
            else if (Regex.IsMatch(filePath, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                filePath = tanksLocation + filePath;
            }
            else
            {
                //old style patch
                filePath = tanksLocation + "\\res_mods" + filePath;
            }
            //patch versiondir out of filePath
            filePath = Regex.Replace(filePath, "versiondir", tanksVersion);
            //verify the file exists...
            if (!File.Exists(filePath))
                return;
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //check to see if it has the header info at the top to see if we need to remove it later
            bool hadHeader = false;
            XmlDocument doc3 = new XmlDocument();
            doc3.Load(filePath);
            foreach (XmlNode node in doc3)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hadHeader = true;
                }
            }
            //determines which version of pathing will be done
            switch (mode)
            {
                case "add":
                    //check to see if it's already there
                    string[] tempp = replace.Split('/');
                    string tempPath = xpath;
                    //make the full node path
                    for (int i = 0; i < tempp.Count() - 1; i++)
                    {
                        tempPath = tempPath + "/" + tempp[i];
                    }
                    XmlNodeList currentSoundBanksAdd = doc.SelectNodes(tempPath);
                    //in each node check if the element exist with the replace innerText
                    foreach (XmlElement e in currentSoundBanksAdd)
                    {
                        string innerText = tempp[tempp.Count() - 1];
                        if (Regex.IsMatch(e.InnerText, innerText))
                            return;
                    }
                    //get to the node where to add the element
                    XmlNode reff = doc.SelectSingleNode(xpath);
                    //create node(s) to add to the element
                    string[] temp = replace.Split('/');
                    List<XmlElement> nodes = new List<XmlElement>();
                    for (int i = 0; i < temp.Count() - 1; i++)
                    {
                        XmlElement ele = doc.CreateElement(temp[i]);
                        if (i == temp.Count() - 2)
                        {
                            //last node with actual data to add
                            ele.InnerText = temp[temp.Count() - 1];
                        }
                        nodes.Add(ele);
                    }
                    //add nodes to the element in reverse for hierarchy order
                    for (int i = nodes.Count - 1; i > -1; i--)
                    {
                        if (i == 0)
                        {
                            //getting here means this is the highmost node
                            //that needto be modified
                            reff.InsertAfter(nodes[i], reff.FirstChild);
                            break;
                        }
                        XmlElement parrent = nodes[i - 1];
                        XmlElement child = nodes[i];
                        parrent.InsertAfter(child, parrent.FirstChild);
                    }
                    //save it
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc.Save(filePath);
                    break;

                case "edit":
                    //check to see if it's already there
                    XmlNodeList currentSoundBanksEdit = doc.SelectNodes(xpath);
                    foreach (XmlElement e in currentSoundBanksEdit)
                    {
                        if (Regex.IsMatch(e.InnerText, replace))
                            return;
                    }
                    //find and replace
                    XmlNodeList rel1Edit = doc.SelectNodes(xpath);
                    foreach (XmlElement eee in rel1Edit)
                    {
                        if (Regex.IsMatch(eee.InnerText, search))
                        {
                            eee.InnerText = replace;
                        }
                    }
                    //save it
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc.Save(filePath);
                    break;

                case "remove":
                    //check to see if it's there
                    XmlNodeList currentSoundBanksRemove = doc.SelectNodes(xpath);
                    foreach (XmlElement e in currentSoundBanksRemove)
                    {
                        if (Regex.IsMatch(e.InnerText, search))
                        {
                            e.RemoveAll();
                        }
                    }
                    //save it
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc.Save(filePath);
                    //remove empty elements
                    XDocument doc2 = XDocument.Load(filePath);
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc2.Save(filePath);
                    break;
            }
            //check to see if we need to remove the header
            bool hasHeader = false;
            XmlDocument doc5 = new XmlDocument();
            doc5.Load(filePath);
            foreach (XmlNode node in doc5)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hasHeader = true;
                }
            }
            //if not had header and has header, remove header
            //if had header and has header, no change
            //if not had header and not has header, no change
            //if had header and not has header, no change
            if (!hadHeader && hasHeader)
            {
                XmlDocument doc4 = new XmlDocument();
                doc4.Load(filePath);
                foreach (XmlNode node in doc4)
                {
                    if (node.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        doc4.RemoveChild(node);
                    }
                }
                doc4.Save(filePath);
            }
        }
        //method to patch a standard text or json file
        //fileLocation is relative to res_mods folder
        private void RegxPatch(string fileLocation, string search, string replace, int lineNumber = 0)
        {
            if (Regex.IsMatch(fileLocation, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                fileLocation = tanksLocation + fileLocation;
            }
            else if (Regex.IsMatch(fileLocation, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                fileLocation = tanksLocation + fileLocation;
            }
            else
            {
                //old style patch
                fileLocation = tanksLocation + "\\res_mods" + fileLocation;
            }
            //patch versiondir out of fileLocation
            fileLocation = Regex.Replace(fileLocation, "versiondir", tanksVersion);

            //check that the file exists
            if (!File.Exists(fileLocation))
                return;

            //load file from disk...
            string file = File.ReadAllText(fileLocation);
            //parse each line into an index array
            string[] fileParsed = file.Split('\n');
            StringBuilder sb = new StringBuilder();
            if (lineNumber == 0)
            //search entire file and replace each instance
            {
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (Regex.IsMatch(fileParsed[i], search))
                    {
                        fileParsed[i] = Regex.Replace(fileParsed[i], search, replace);
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
            }
            else if (lineNumber == -1)
            //search entire file and string and make one giant regex replacement
            {
                //but remove newlines first
                file = Regex.Replace(file, "\n", "newline");
                if (Regex.IsMatch(file,search))
                {
                    file = Regex.Replace(file,search,replace);
                }
                file = Regex.Replace(file, "newline", "\n");
                sb.Append(file);
            }
            else
            {
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (i == lineNumber - 1)
                    {
                        string value = fileParsed[i];
                        if (Regex.IsMatch(value, search))
                        {
                            fileParsed[i] = Regex.Replace(fileParsed[i], search, replace);
                        }
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
            }
            //save the file back into the string and then the file
            file = sb.ToString();
            File.WriteAllText(fileLocation, file);
        }
        //method to parse json files
        public void jsonPatch(string jsonFile, string jsonPath, string newValue)
        {
            //try to convert the new value to a bool or an int or double first
            bool newValueBool = false;
            int newValueInt = -69420;
            double newValueDouble = -69420.0d;
            bool useBool = false;
            bool useInt = false;
            bool useDouble = false;
            //try a bool first, only works with "true" and "false"
            try
            {
                newValueBool = bool.Parse(newValue);
                useBool = true;
                useInt = false;
                useDouble = false;
            }
            catch (FormatException)
            {

            }
            //try a double nixt. it will parse a double and int. at this point it could be eithor
            try
            {
                newValueDouble = double.Parse(newValue);
                useDouble = true;
            }
            catch (FormatException)
            {

            }
            //try an int next. if it works than turn double to false and int to true
            try
            {
                newValueInt = int.Parse(newValue);
                useInt = true;
                useDouble = false;
            }
            catch (FormatException)
            {

            }
            //check if it's the new structure
            if (Regex.IsMatch(jsonFile, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                jsonFile = tanksLocation + jsonFile;
            }
            else if (Regex.IsMatch(jsonFile, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                jsonFile = tanksLocation + jsonFile;
            }
            else
            {
                //old style patch
                jsonFile = tanksLocation + "\\res_mods" + jsonFile;
            }

            //patch versiondir out of fileLocation
            jsonFile = Regex.Replace(jsonFile, "versiondir", tanksVersion);

            //check that the file exists
            if (!File.Exists(jsonFile))
                return;

            //load file from disk...
            string file = File.ReadAllText(jsonFile);
            //save the "$" lines
            List<StringSave> ssList = new List<StringSave>();
            //patch any single line comments out of it by doing regex line by line
            StringBuilder backTogether = new StringBuilder();
            string[] removeComments = file.Split('\n');
            for (int i = 0; i < removeComments.Count(); i++)
            {
                string temp = removeComments[i];
                //replace tabs with spaces
                temp = Regex.Replace(temp, "\t", " ");
                //remove single comment lines
                if (Regex.IsMatch(temp, @"^ *//.*"))
                    temp = Regex.Replace(temp, @"^ *//.*", "");
                //remove comments after values
                if (Regex.IsMatch(temp, @" +//.*$"))
                {
                    temp = Regex.Replace(temp, @" +//.*$","");
                }
                //remove more comments after values
                if (Regex.IsMatch(temp, @",//.*$"))
                {
                    temp = Regex.Replace(temp, @",//.*$", ",");
                }
                if (Regex.IsMatch(temp, @"\${"))
                {
                    bool hadComma = false;
                    if (Regex.IsMatch(temp,","))
                    {
                        hadComma = true;
                    }
                    StringSave ss = new StringSave();
                    ss.name = temp.Split('"')[1];
                    ss.value = temp.Split('$')[1];
                    ssList.Add(ss);
                    temp = "\"" + ss.name + "\"" + ": -69420" ;
                    if (hadComma)
                        temp = temp + ",";
                }
                backTogether.Append(temp + "\n");
            }
            file = backTogether.ToString();
            //remove any newlines
            file = Regex.Replace(file, "\n", "");
            file = Regex.Replace(file, "\r", "");
            //remove any block comments
            file = Regex.Replace(file, @"/\*.*?\*/", "");
            JToken root = null;
            //it could still fail, cause it's such an awesome api
            try
            {
                root = JToken.Parse(file);
            }
            catch (JsonReaderException)
            {
                Settings.appendToLog("ERROR: Failed to patch " + jsonFile);
                MessageBox.Show("ERROR: Failed to patch " + jsonFile);
                if (Program.testMode)
                {
                    //in test mode this is worthy of an exeption
                    throw new JsonReaderException();
                }
            }
            //if it failed to parse show the message (above) and pull out
            if (root == null)
                return;
            //the actual patch method
            foreach (var value in root.SelectTokens(jsonPath).ToList())
            {
                if (value == root)
                    root = JToken.FromObject(newValue);
                else
                {
                    if (useBool)
                    {
                        value.Replace(JToken.FromObject(newValueBool));
                    }
                    else if (useInt)
                    {
                        value.Replace(JToken.FromObject(newValueInt));
                    }
                    else if (useDouble)
                    {
                        value.Replace(JToken.FromObject(newValueDouble));
                    }
                    else //string
                    {
                        value.Replace(JToken.FromObject(newValue));
                    }
                }
            }
            StringBuilder rebuilder = new StringBuilder();
            string[] putBackDollas = root.ToString().Split('\n');
            for (int i = 0; i < putBackDollas.Count(); i++)
            {
                string temp = putBackDollas[i];
                if (Regex.IsMatch(temp,"-69420"))//look for the temp value
                {
                    string name = temp.Split('"')[1];
                    for (int j = 0; j < ssList.Count; j++)
                    {
                        if (name.Equals(ssList[j].name))
                        {
                            //remake the line
                            temp = "\"" + ssList[j].name + "\"" + ": $" + ssList[j].value;
                            putBackDollas[i] = temp;
                            ssList.RemoveAt(j);
                        }
                    }
                }
                rebuilder.Append(putBackDollas[i] + "\n");
            }
            File.WriteAllText(jsonFile, rebuilder.ToString());
        }
        //parses a patch xml file into an xml patch instance in memory to be enqueued
        private void createPatchList(string xmlFile)
        {
            if (!File.Exists(xmlFile))
                return;
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFile);
            //loaded the xml file into memory, create an xml list of patchs
            XmlNodeList patchesList = doc.SelectNodes("//patchs/patch");
            //foreach "patch" node in the "patchs" node of the xml file
            foreach (XmlNode n in patchesList)
            {
                //create a patch instance to take the patch information
                Patch p = new Patch();
                //foreach node in this specific "patch" node
                foreach (XmlNode nn in n.ChildNodes)
                {
                    //each element in the xml gets put into the
                    //the correcpondng attribute for the Patch instance
                    switch (nn.Name)
                    {
                        case "type":
                            p.type = nn.InnerText;
                            break;
                        case "mode":
                            p.mode = nn.InnerText;
                            break;
                        case "file":
                            p.file = nn.InnerText;
                            break;
                        case "path":
                            p.path = nn.InnerText;
                            break;
                        case "line":
                            if (nn.InnerText.Equals(""))
                                break;
                            p.lines = nn.InnerText.Split(',');
                            break;
                        case "search":
                            p.search = nn.InnerText;
                            break;
                        case "replace":
                            p.replace = nn.InnerText;
                            break;
                    }
                }
                patchList.Add(p);
            }
        }
        //handler for when the install relhax modpack button is pressed
        //basicly the entire install process
        private void installRelhaxMod_Click(object sender, EventArgs e)
        {
            //bool to say to the downloader to use the "modpack" code
            modPack = true;
            state = InstallState.idle;
            downloadPath = Application.StartupPath + "\\RelHaxDownloads";
            //reset the interface
            this.reset();
            //attempt to locate the tanks directory automatically
            //if it fails, it will prompt the user to return the world of tanks exe
            if (this.autoFindTanks() == null || Settings.forceManuel)
            {
                if (this.manuallyFindTanks() == null) return;
            }
            //parse all strings for installation
            if (this.parseStrings() == null)
            {
                this.displayError("The auto-detection failed. Please use the 'force manual' option", null);
                state = InstallState.error;
                return;
            }
            tanksVersion = this.getFolderVersion(null);
            //the download timers started for download speed measurement
            sw.Reset();
            sw.Start();
            //actual new code
            if (Settings.backupModFolder)
            {
                //backupResMods the mods folder
                if (Directory.Exists(Application.StartupPath + "\\RelHaxModBackup"))
                {
                    state = InstallState.deleteBackupResMods;
                    //need to check for legacy positions
                    string theCurrentTanksVersion = this.getFolderVersion(null);
                    if (Directory.Exists(Application.StartupPath + "\\RelHaxModBackup\\" + theCurrentTanksVersion))
                    {
                        //move all folders info res_mods
                        Settings.appendToLog("WARNING: Legacy backup structure detected, moving files");
                        string[] dirsList = Directory.GetDirectories(Application.StartupPath + "\\RelHaxModBackup");
                        if (!Directory.Exists(Application.StartupPath + "\\RelHaxModBackup\\res_mods")) 
                            Directory.CreateDirectory(Application.StartupPath + "\\RelHaxModBackup\\res_mods");
                        foreach (string s in dirsList)
                        {
                            string dirName = new DirectoryInfo(s).Name;
                            Directory.Move(Application.StartupPath + "\\RelHaxModBackup\\" + dirName, Application.StartupPath + "\\RelHaxModBackup\\res_mods\\" + dirName);
                        }
                    }
                    if (!Directory.Exists(Application.StartupPath + "\\RelHaxModBackup\\res_mods"))
                    {
                        Directory.CreateDirectory(Application.StartupPath + "\\RelHaxModBackup\\res_mods");
                    }
                    this.backgroundDelete(Application.StartupPath + "\\RelHaxModBackup\\res_mods");
                }
                return;
            }
            this.parseInstallationPart1();
        }
        //next part of the install process
        private void parseInstallationPart1()
        {
            state = InstallState.modSelection;
            //reset the childProgresBar value
            childProgressBar.Maximum = 100;
            childProgressBar.Value = 0;
            //show the mod selection window
            ModSelectionList list = new ModSelectionList();
            list.ShowDialog();
            if (list.cancel)
            {
                state = InstallState.idle;
                return;
            }
            downloadProgress.Text = "Loading...";
            Application.DoEvents();
            modsToInstall = new List<Mod>();
            configsToInstall = new List<Config>();
            patchList = new List<Patch>();
            userMods = new List<Mod>();
            dependencies = new List<Dependency>();
            parsedCatagoryLists = list.parsedCatagoryList;
            //add the global dependencies to the dependency list
            foreach (Dependency d in list.globalDependencies)
            {
                if (d.enabled)
                    dependencies.Add(d);
            }
            //if mod is enabled and checked, add it to list of mods to extract/install
            //same for configs
            foreach (Catagory c in parsedCatagoryLists)
            {
                bool dependenciesAdded = false;
                //will itterate through every catagory once
                foreach (Mod m in c.mods)
                {
                    //will itterate through every mod of every catagory once
                    if (m.enabled && m.modChecked)
                    {
                        //move each mod that is enalbed and checked to a new
                        //list of mods to install
                        //also check that it actually has a zip file
                        if (!m.modZipFile.Equals(""))
                            modsToInstall.Add(m);
                        //at least one mod of this catagory is checked, add any dependencies required
                        if (!dependenciesAdded)
                        {
                            //add dependencies
                            foreach (Dependency d in c.dependencies)
                            {
                                //check dependency is enabled and has a zip file with it
                                if (d.enabled && !d.dependencyZipFile.Equals(""))
                                    dependencies.Add(d);
                            }
                            dependenciesAdded = true;
                        }
                        foreach (Config cc in m.configs)
                        {
                            //check to make sureit's enabled and checked and has a valid zip file with it
                            if (cc.enabled && cc.configChecked && !cc.zipConfigFile.Equals(""))
                            {
                                //same for configs
                                configsToInstall.Add(cc);
                            }
                        }
                    }
                }
            }
            //create a new download queue. even if not downloading any
            //relhax modpack mods, still used in downloader code
            downloadQueue = new List<DownloadItem>();
            //check for any user mods to install
            for (int i = 0; i < list.userMods.Count; i++)
            {
                if (list.userMods[i].enabled && list.userMods[i].modChecked)
                {
                    this.userMods.Add(list.userMods[i]);
                }
            }
            //if the user did not select any relhax modpack mods to install
            if (modsToInstall.Count == 0 && configsToInstall.Count == 0)
            {
                //check for userMods
                if (userMods.Count > 0)
                {
                    //skip to the extraction process
                    state = InstallState.extractUserMods;
                    this.downloader_DownloadFileCompleted(null, null);
                }
                //pull out because there are no mods to install
                downloadProgress.Text = "Idle";
                return;
            }
            //foreach mod and config, if the crc's don't match, add it to the downloadQueue
            string localFilesDir = Application.StartupPath + "\\RelHaxDownloads\\";
            foreach (Dependency d in dependencies)
            {
                if (!this.CRCsMatch(localFilesDir + d.dependencyZipFile, d.dependencyZipCRC))
                {
                    downloadQueue.Add(new DownloadItem(new Uri(this.downloadURL + d.dependencyZipFile), localFilesDir + d.dependencyZipFile));
                }
            }
            foreach (Mod m in modsToInstall)
            {
                if (!this.CRCsMatch(localFilesDir + m.modZipFile, m.crc))
                {
                    //crc's don't match, need to re-download
                    downloadQueue.Add(new DownloadItem(new Uri(this.downloadURL + m.modZipFile), localFilesDir + m.modZipFile));
                }
            }
            foreach (Config c in configsToInstall)
            {
                if (!this.CRCsMatch(localFilesDir + c.zipConfigFile, c.crc))
                {
                    //crc's don't match, need to re-download
                    downloadQueue.Add(new DownloadItem(new Uri(this.downloadURL + c.zipConfigFile), localFilesDir + c.zipConfigFile));
                }
            }
            parrentProgressBar.Maximum = downloadQueue.Count;
            //at this point, there may be user mods selected,
            //and there is at least one mod to extract
            //check for any mods to be install tha also need to be downloaded
            if (downloadQueue.Count > 0)
            {
                state = InstallState.downloading;
                //delete temp/0kb/corrupt file
                if (File.Exists(downloadQueue[0].zipFile)) File.Delete(downloadQueue[0].zipFile);
                //download new zip file
                downloader = new WebClient();
                downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                downloader.Proxy = null;
                downloader.DownloadFileAsync(downloadQueue[0].URL, downloadQueue[0].zipFile);
                tempOldDownload = Path.GetFileName(downloadQueue[0].zipFile);
                this.appendToLog("downloading " + tempOldDownload);
                currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile);
                if (currentModDownloading.Length >= 30)
                {
                    currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile).Substring(0, 23) + "...";
                }
                downloadQueue.RemoveAt(0);
                parrentProgressBar.Value++;
            }
            else
            {
                //there are no mods to download, go right to the extraction process
                state = InstallState.extractRelhaxMods;
                downloader_DownloadFileCompleted(null, null);
            }
            //end the installation process
            return;
        }
        //returns true if the CRC's of each file match, false otherwise
        private bool CRCsMatch(string localFile, string remoteCRC)
        {
            if (!File.Exists(localFile))
                return false;
            MD5 hash = MD5.Create();
            string crc = this.GetMd5Hash(hash, localFile);
            if (crc.Equals(remoteCRC))
                return true;
            return false;
        }
        //returns a string of the MD5 hash of an object.
        //used to determine if a download is corrupted or not,
        //or if it needs to be updated
        private string GetMd5Hash(MD5 md5Hash, string inputFile)
        {
            // Convert the input string to a byte array and compute the hash.
            var stream = File.OpenRead(inputFile);
            byte[] data = md5Hash.ComputeHash(stream);
            stream.Close();
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        //Main method to uninstall the modpack
        private void uninstallRelhaxMod_Click(object sender, EventArgs e)
        {
            modPack = true;
            //reset the interface
            this.reset();
            //attempt to locate the tanks directory
            if (this.autoFindTanks() == null || Settings.forceManuel)
            {
                if (this.manuallyFindTanks() == null) return;
            }
            //parse all strings
            if (this.parseStrings() == null)
            {
                this.displayError("The auto-detection failed. Please use the 'force manual' option", null);
                return;
            }
            //verify that the user really wants to uninstall
            if (MessageBox.Show("This will delete ALL INSTALLED MODS. Are you Sure?", "Um...", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                downloadProgress.Text = "Uninstalling...";
                state = InstallState.uninstallResMods;
                this.backgroundDelete(tanksLocation + "\\res_mods");
            }
        }
        //handler for what happends when the check box "clean install" is checked or not
        private void cleanInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.cleanInstallation = cleanInstallCB.Checked;
        }
        //method to bring up the crc checker to get the crc values of a mod
        private void CIEplainLabel_Click(object sender, EventArgs e)
        {
            CRCCheck crcCHecker = new CRCCheck();
            crcCHecker.Show();
        }
        //enalbes the user to use "comic sans" font for the 1 person that would ever want to do that
        private void cancerFontCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.comicSans = cancerFontCB.Checked;
            Settings.applyInternalSettings();
            this.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
        }
        //logs string info to the log output
        private void appendToLog(string info)
        {
            //the method should automaticly make the file if it's not there
            File.AppendAllText(Application.StartupPath + "\\RelHaxLog.txt", info + "\n");
        }
        //uses backgroundWorker to copy files
        private void backgroundCopy(string source, string dest)
        {
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            downloadProgress.Text = "Copying file " + numFilesToCopyDeleteExtract + " of " + numFilesToProcessInt;
            copyworker = new BackgroundWorker();
            copyworker.WorkerReportsProgress = true;
            copyworker.DoWork += new DoWorkEventHandler(copyworker_DoWork);
            copyworker.ProgressChanged += new ProgressChangedEventHandler(copyworker_ProgressChanged);
            copyworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(copyworker_RunWorkerCompleted);
            object sourceFolder = source;
            object destFolder = dest;
            object[] parameters = new object[] { sourceFolder, destFolder };
            copyworker.RunWorkerAsync(parameters);
        }
        //uses backgroundWorker to delete folder and everything inside
        //rather destructive if i do say so myself
        private void backgroundDelete(string folder)
        {
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            downloadProgress.Text = "Copying file " + numFilesToCopyDeleteExtract + " of " + numFilesToProcessInt;
            deleteworker = new BackgroundWorker();
            deleteworker.WorkerReportsProgress = true;
            deleteworker.DoWork += new DoWorkEventHandler(deleteworker_DoWork);
            deleteworker.ProgressChanged += new ProgressChangedEventHandler(deleteworker_ProgressChanged);
            deleteworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(deleteworker_RunWorkerCompleted);
            object folderToDelete = folder;
            object[] parameters = new object[] { folderToDelete };
            deleteworker.RunWorkerAsync(parameters);
        }
        //uses backgroundWorker to extract fileStream
        private void backgroundExtract(bool user)
        {
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            downloadProgress.Text = "Loading Extraction Text...";
            extractworker = new BackgroundWorker();
            extractworker.WorkerReportsProgress = true;
            extractworker.DoWork += new DoWorkEventHandler(extractworker_DoWork);
            extractworker.ProgressChanged += new ProgressChangedEventHandler(extractworker_ProgressChanged);
            extractworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(extractworker_RunWorkerCompleted);
            zip = new ZipFile();
            zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);

            object[] parameters = new object[] { };
            userExtract = user;
            if (!userExtract)
            {
                speedLabel.Text = "Extracting RelHax Mods...";
                parrentProgressBar.Maximum = modsToInstall.Count + configsToInstall.Count + dependencies.Count;
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
            }
            else
            {
                speedLabel.Text = "Extracting User Mods...";
                parrentProgressBar.Maximum = userMods.Count;
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
            }

            extractworker.RunWorkerAsync(parameters);
        }
        //gets the total number of files to process to eithor delete or copy
        private int numFilesToProcess(string folder)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(folder);
            DirectoryInfo[] dirs = dir.GetDirectories();
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                numFilesToProcessInt++;
            }
            foreach (DirectoryInfo subdir in dirs)
            {
                numFilesToProcessInt++;
                numFilesToProcess(subdir.FullName);
            }
            return numFilesToProcessInt;
        }
        //handler for the copyworker when it is called
        private void copyworker_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            string sourceFolder = (string)parameters[0];
            string destFolder = (string)parameters[1];
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            this.numFilesToProcess(sourceFolder);
            this.DirectoryCopy(sourceFolder, destFolder, true);
        }
        //handler for the copyworker when progress is made
        private void copyworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            downloadProgress.Text = "Copying file " + numFilesToCopyDeleteExtract + " of " + numFilesToProcessInt;
            childProgressBar.Maximum = numFilesToProcessInt;
            childProgressBar.Value = numFilesToCopyDeleteExtract;
        }
        //handler for when the copyworker is completed
        private void copyworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (state == InstallState.backupResMods)
            {
                state = InstallState.backupMods;
                if (!Directory.Exists(Application.StartupPath + "\\RelHaxModBackup\\mods"))
                {
                    Directory.CreateDirectory(Application.StartupPath + "\\RelHaxModBackup\\mods");
                }
                this.backgroundCopy(tanksLocation + "\\mods", Application.StartupPath + "\\RelHaxModBackup\\mods");
                return;
            }
            else if (state == InstallState.backupMods)
            {
                state = InstallState.modSelection;
                this.parseInstallationPart1();
                return;
            }
            else
            {
                return;
            }
        }
        //handler for the deleteworker when it is called
        private void deleteworker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            object[] parameters = e.Argument as object[];
            string folderToDelete = (string)parameters[0];
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            this.numFilesToProcess(folderToDelete);
            this.DirectoryDelete(folderToDelete, true);
        }
        //handler for the deleteworker when progress is made
        private void deleteworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            downloadProgress.Text = "Deleting file " + numFilesToCopyDeleteExtract + " of " + numFilesToProcessInt;
            childProgressBar.Maximum = numFilesToProcessInt;
            childProgressBar.Value = numFilesToCopyDeleteExtract;
        }
        //handler for when the deleteworker is completed
        private void deleteworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (state == InstallState.deleteBackupResMods)
            {
                //delete the backup mods as well
                state = InstallState.deleteBackupMods;
                if (Directory.Exists(Application.StartupPath + "\\RelHaxModBackup\\mods"))
                {
                    this.backgroundDelete(Application.StartupPath + "\\RelHaxModBackup\\mods");
                    return;
                }
                else
                {
                    //start the backupResMods copy process
                    state = InstallState.backupResMods;
                    this.backgroundCopy(tanksLocation + "\\res_mods", Application.StartupPath + "\\RelHaxModBackup\\res_mods");
                    return;
                }
            }
            else if (state == InstallState.deleteBackupMods)
            {
                //start the backupResMods copy process
                state = InstallState.backupResMods;
                this.backgroundCopy(tanksLocation + "\\res_mods", Application.StartupPath + "\\RelHaxModBackup\\res_mods");
                return;
            }
            else if (state == InstallState.deleteResMods)
            {
                state = InstallState.deleteMods;
                if (Directory.Exists(tanksLocation + "\\mods")) this.backgroundDelete(tanksLocation + "\\mods");
                return;
            }
            else if (state == InstallState.deleteMods)
            {
                //start the extraction process
                state = InstallState.extractRelhaxMods;
                this.backgroundExtract(false);
                return;
            }
            else if (state == InstallState.uninstallResMods)
            {
                //uninstall Mods folder
                if (Directory.Exists(tanksLocation + "\\mods\\" + this.getFolderVersion(null)))
                {
                    state = InstallState.uninstallMods;
                    this.backgroundDelete(tanksLocation + "\\mods");
                    return;
                }
                else
                {
                    //finish uninstallResMods process
                    state = InstallState.idle;
                    if (!Directory.Exists(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null));
                    if (!Directory.Exists(tanksLocation + "\\mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\mods\\" + this.getFolderVersion(null));
                    downloadProgress.Text = "Done!";
                    childProgressBar.Value = 0;
                    return;
                }
            }
            else if (state == InstallState.uninstallMods)
            {
                //finish uninstallResMods process
                state = InstallState.idle;
                if (!Directory.Exists(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null));
                if (!Directory.Exists(tanksLocation + "\\mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\mods\\" + this.getFolderVersion(null));
                downloadProgress.Text = "Done!";
                childProgressBar.Value = 0;
                return;
            }
            else
            {
                return;
            }
        }
        //handler for the deleteworker when it is called
        private void extractworker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker extractworker = (BackgroundWorker)sender;
            //just a double-check to delete all patches
            if (Directory.Exists(tanksLocation + "\\_patch")) Directory.Delete(tanksLocation + "\\_patch", true);
            if (Directory.Exists(tanksLocation + "\\_fonts")) Directory.Delete(tanksLocation + "\\_fonts", true);
            if (!Directory.Exists(tanksLocation + "\\res_mods")) Directory.CreateDirectory(tanksLocation + "\\res_mods");
            if (!userExtract)
            {
                //extract RelHax Mods
                this.appendToLog("Starting Relhax Modpack Extraction");
                string downloadedFilesDir = Application.StartupPath + "\\RelHaxDownloads\\";
                //extract dependencies
                foreach (Dependency d in dependencies)
                {
                    this.appendToLog("Extracting Dependency " + d.dependencyZipFile);
                    if (!d.dependencyZipFile.Equals("")) this.unzip(downloadedFilesDir + d.dependencyZipFile, tanksLocation);
                    extractworker.ReportProgress(1);
                }
                //extract mods
                foreach (Mod m in modsToInstall)
                {
                    this.appendToLog("Extracting Mod " + m.modZipFile);
                    if (!m.modZipFile.Equals("")) this.unzip(downloadedFilesDir + m.modZipFile, tanksLocation);
                    extractworker.ReportProgress(1);
                }
                //extract configs
                foreach (Config c in configsToInstall)
                {
                    this.appendToLog("Extracting Config " + c.zipConfigFile);
                    if (!c.zipConfigFile.Equals("")) this.unzip(downloadedFilesDir + c.zipConfigFile, tanksLocation);
                    extractworker.ReportProgress(1);
                }
                this.appendToLog("Finished Relhax Modpack Extraction");

            }
            else
            {
                //extract user mods
                this.appendToLog("Starting Relhax Modpack User Mod Extraction");
                string downloadedFilesDir = Application.StartupPath + "\\RelHaxUserMods\\";
                foreach (Mod m in userMods)
                {
                    if (m.modChecked)
                    {
                        this.appendToLog("Exracting " + Path.GetFileName(m.modZipFile));
                        this.unzip(downloadedFilesDir + Path.GetFileName(m.modZipFile), tanksLocation);
                        extractworker.ReportProgress(1);
                    }
                }
                this.appendToLog("Finished Extracting Relhax Modpack User Mod Extraction");
            }

        }
        //handler for the extractworker when progress is made
        private void extractworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (childProgressBar.Maximum != childMaxProgres)
            childProgressBar.Maximum = childMaxProgres;
            if (childCurrentProgres != 0)
                childProgressBar.Value = childCurrentProgres;
            if (currentZipEntry.Length >= 47)
            {
                downloadProgress.Text = currentZipEntry.Substring(0, 47) + "...";
            }
            if (isParrentDone)
            {
                if (parrentProgressBar.Value != parrentProgressBar.Maximum)
                    parrentProgressBar.Value++;
            }
        }
        //handler for when the extractworker is completed
        private void extractworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (state == InstallState.extractRelhaxMods)
            {
                state = InstallState.patchRelhaxMods;
                this.patchFiles();
                state = InstallState.extractUserMods;
                this.backgroundExtract(true);
                return;
            }
            else if (state == InstallState.extractUserMods)
            {
                state = InstallState.patchUserMods;
                this.patchFiles();
                state = InstallState.installFonts;
                this.installFonts();
                return;
            }
            else
            {
                return;
            }
        }
        //recursivly copies every file from one place to another
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
                copyworker.ReportProgress(numFilesToCopyDeleteExtract++);
            }
            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        //recursivly deletes every file from one place to another
        private void DirectoryDelete(string sourceDirName, bool deleteSubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(sourceDirName, file.Name);
                bool tryAgain = true;
                while (tryAgain)
                {
                    try
                    {
                        file.Delete();
                        tryAgain = false;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        DialogResult res = MessageBox.Show("Extraction Error", "Error", MessageBoxButtons.RetryCancel);
                        if (res == DialogResult.Retry)
                        {
                            tryAgain = true;
                        }
                        else
                        {
                            Application.Exit();
                        }
                    }
                }
                deleteworker.ReportProgress(numFilesToCopyDeleteExtract++);
            }
            // If copying subdirectories, copy them and their contents to new location.
            if (deleteSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(sourceDirName, subdir.Name);
                    DirectoryDelete(subdir.FullName, deleteSubDirs);
                    try
                    {
                        subdir.Delete();
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Please close all explorer windows in the res_mods (or deeper), and click ok to continue.");
                        subdir.Delete();
                    }
                    deleteworker.ReportProgress(numFilesToCopyDeleteExtract++);
                }
            }
        }
        //handler for when the "force manuel" checkbox is checked
        private void forceManuel_CheckedChanged(object sender, EventArgs e)
        {
            Settings.forceManuel = forceManuel.Checked;
        }
        //handler for when the "backupResMods mods" checkbox is changed
        private void backupModsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.backupModFolder = backupModsCheckBox.Checked;
        }
        //extracts embeded rescource onto disk
        public void extractEmbeddedResource(string outputDir, string resourceLocation, List<string> files)
        {
            foreach (string file in files)
            {
                using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation + @"." + file))
                {
                    using (System.IO.FileStream fileStream = new System.IO.FileStream(System.IO.Path.Combine(outputDir, file), System.IO.FileMode.Create))
                    {
                        for (int i = 0; i < stream.Length; i++)
                        {
                            fileStream.WriteByte((byte)stream.ReadByte());
                        }
                        fileStream.Close();
                    }
                }
            }
        }
        //handler for when the window is goingto be closed
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save settings
            Settings.saveSettings();
            this.appendToLog("Application Closing");
            this.appendToLog("|------------------------------------------------------------------------------------------------|");
        }
        //hander for when the "large font" button is checked
        private void largerFontButton_CheckedChanged(object sender, EventArgs e)
        {
            Settings.largeFont = largerFontButton.Checked;
            Settings.applyInternalSettings();
            this.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
        }
        //applies all settings from static settings class to this form
        private void applySettings()
        {
            //apply all checkmarks
            this.forceManuel.Checked = Settings.forceManuel;
            this.cleanInstallCB.Checked = Settings.cleanInstallation;
            this.backupModsCheckBox.Checked = Settings.backupModFolder;
            this.cancerFontCB.Checked = Settings.comicSans;
            this.largerFontButton.Checked = Settings.largeFont;
            this.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
            switch (Settings.gif)
            {
                case (Settings.LoadingGifs.standard):
                    {
                        standardImageRB.Checked = true;
                        break;
                    }
                case (Settings.LoadingGifs.thirdGuards):
                    {
                        thirdGuardsLoadingImageRB.Checked = true;
                        break;
                    }
            }
        }
        //handler for when the "standard" loading animation is cleicked
        private void standardImageRB_CheckedChanged(object sender, EventArgs e)
        {
            if (standardImageRB.Checked)
            {
                Settings.gif = Settings.LoadingGifs.standard;
            }
        }
        //handler for when the "thirdguards" loading aimation is clicked
        private void thirdGuardsLoadingImageRB_CheckedChanged(object sender, EventArgs e)
        {
            if (thirdGuardsLoadingImageRB.Checked)
            {
                Settings.gif = Settings.LoadingGifs.thirdGuards;
            }
        }

        private void forceManuel_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = "This option is for forcing a manuel World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.";
        }

        private void forceManuel_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = helperText;
        }

        private void cleanInstallCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = "This reccomended option will empty your res_mods folder before installing" +
                    "your new mod selections. Unless you know what you are doing, it is recommended that you keep this on to avoid problems.";
        }

        private void cleanInstallCB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = helperText;
        }

        private void backupModsCheckBox_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = "Select this to make a backup of your current res_mods folder." +
                    "Keep in mind that it only keeps the LATEST BACKUP, meaning if you check this and install," +
                    "it will delete what is currently in the backup location and copy what you have in your res_mods folder.";
        }

        private void backupModsCheckBox_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = helperText;
        }

        private void cancerFontCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = "Enable Comic Sans. Yes, somebody, somewhere out there actually wanted this crap.";
        }

        private void cancerFontCB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = helperText;
        }

        private void largerFontButton_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = "Enable this to enlarge all form font.";
        }

        private void largerFontButton_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = helperText;
        }

        private void standardImageRB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = "Select a loading gif for the mod preview window.";
        }

        private void standardImageRB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = helperText;
        }

        private void thirdGuardsLoadingImageRB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = "Select a loading gif for the mod preview window.";
        }

        private void thirdGuardsLoadingImageRB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = helperText;
        }

        private void findBugAddModLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.google.com/spreadsheets/d/1LmPCMAx0RajW4lVYAnguHjjd8jArtWuZIGciFN76AI4/edit?usp=sharing");
        }
    }
    //a class for the downloadQueue list, to make a queue of downloads
    class DownloadItem
    {
        public Uri URL { get; set; }
        public string zipFile { get; set; }
        //create a DownloadItem with the 2 properties set
        public DownloadItem(Uri newURL, String newZipFile)
        {
            URL = newURL;
            zipFile = newZipFile;
        }
    }
}
