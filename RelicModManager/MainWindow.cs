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

namespace RelhaxModpack
{
    public partial class MainWindow : Form
    {
        //all instance variables required to be up here
        private FolderBrowserDialog selectWotFolder = new FolderBrowserDialog();
        private WebClient downloader = new WebClient();
        private string modAudioFolder;//res_mods/versiondir/audioww
        private string tempPath = Path.GetTempPath();//C:/users/userName/appdata/local/temp
        private const int MBDivisor = 1048576;
        private string managerVersion = "version 21.7.4";
        private string tanksLocation;//sample:  c:/games/World_of_Tanks
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
        private int childMaxProgres;
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
            backupUserData = 7,
            deleteResMods = 8,
            deleteMods = 9,
            extractRelhaxMods = 10,
            patchRelhaxMods = 11,
            extractUserMods = 12,
            patchUserMods = 13,
            restoreUserData = 14,
            installFonts = 15,
            uninstallResMods = 16,
            uninstallMods = 17,
            smartUninstall = 18
        };
        private InstallState state = InstallState.idle;
        private string tanksVersion;//0.9.x.y
        BackgroundWorker deleteworker;
        BackgroundWorker copyworker;
        BackgroundWorker smartDeleteworker;
        //list to maintain the refrence lines in a json patch
        List<double> timeRemainArray = new List<double>();
        //the ETA variable for downlading
        double actualTimeRemain = 0;
        float previousTotalBytesDownloaded = 0;
        float currentTotalBytesDownloaded = 0;
        float differenceTotalBytesDownloaded = 0;
        float sessionDownloadSpeed = 0;
        private loadingGifPreview gp;

        //The constructur for the application
        public MainWindow()
        {
            Settings.appendToLog("MainWindow Constructed");
            InitializeComponent();
        }
        //handler for the mod download file progress
        void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!downloadTimer.Enabled)
                downloadTimer.Enabled = true;
            string totalSpeedLabel = "";
            //get the download information into numeric classes
            float bytesIn = float.Parse(e.BytesReceived.ToString());
            float totalBytes = float.Parse(e.TotalBytesToReceive.ToString());
            float MBytesIn = (float)bytesIn / MBDivisor;
            float MBytesTotal = (float)totalBytes / MBDivisor;
            currentTotalBytesDownloaded = bytesIn;
            //create the download progress string
            string currentModDownloadingShort = currentModDownloading;
            if (currentModDownloading.Length > 200)
                currentModDownloadingShort = currentModDownloading.Substring(0, 15) + "...";
            downloadProgress.Text = Translations.getTranslatedString("Downloading")  + " " + currentModDownloadingShort + " (" + Math.Round(MBytesIn, 1) + " MB" + " of " + Math.Round(MBytesTotal, 1) + " MB)";
            //set the progress bar
            childProgressBar.Value = e.ProgressPercentage;
            //set the download speed
            sessionDownloadSpeed = (float)Math.Round(sessionDownloadSpeed, 2);
            totalSpeedLabel = "" + sessionDownloadSpeed + " MB/s";
            //get the ETA for the download
            double totalTimeToDownload = MBytesTotal / (e.BytesReceived / 1048576d / sw.Elapsed.TotalSeconds);
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
            //round to a whole number
            actualTimeRemain = Math.Round(actualTimeRemain, 0);
            //prevent the eta from becomming less than 0
            if (actualTimeRemain < 0)
                actualTimeRemain = 0;
            //convert the total seconds to mins and seconds
            int actualTimeMins = (int)actualTimeRemain / 60;
            int actualTimeSecs = (int)actualTimeRemain % 60;
            totalSpeedLabel = totalSpeedLabel + " " + actualTimeMins + " mins " + actualTimeSecs + " secs ";
            speedLabel.Text = totalSpeedLabel;
        }
        //handler for the mod download file complete event
        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //check to see if the user cancled the download
            if (e != null && e.Cancelled)
            {
                //update the UI and download state
                state = InstallState.idle;
                toggleUIButtons(true);
                speedLabel.Text = "";
                downloadProgress.Text = Translations.getTranslatedString("idle");
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
                return;
            }
            downloadTimer.Enabled = false;


            //new relhax modpack code
            if (e != null && e.Error != null && e.Error.Message.Equals("The remote server returned an error: (404) Not Found."))
            {
                //404
                Settings.appendToLog("ERROR: " + tempOldDownload + " failed to download");
                MessageBox.Show(Translations.getTranslatedString("failedToDownload_1") + tempOldDownload + Translations.getTranslatedString("failedToDownload_2"));
                Application.Exit();
            }
            if (downloadQueue.Count != 0)
            {
                cancelDownloadButton.Enabled = true;
                cancelDownloadButton.Visible = true;
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
                Settings.appendToLog("downloading " + tempOldDownload);
                currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile);
                if (currentModDownloading.Length >= 200)
                {
                    currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile).Substring(0, 23) + "...";
                }
                downloadQueue.RemoveAt(0);
                parrentProgressBar.Value++;
                return;
            }
            if (downloadQueue.Count == 0)
            {
                cancelDownloadButton.Enabled = false;
                cancelDownloadButton.Visible = false;
                //check if backing up user cache files
                if (Settings.saveUserData)
                {
                    Settings.appendToLog("saveUserData checked, saving user cache data");
                    int tempState = (int)state;
                    state = InstallState.backupUserData;
                    //backup user data on UI thread. it won't be long
                    this.backupUserData();
                    state = (InstallState)tempState;
                }
                if (state == InstallState.downloading)
                {
                    Settings.appendToLog("Downloading finished");
                    //just finished downloading, needs to start extracting
                    if (Settings.cleanInstallation)
                    {
                        state = InstallState.deleteResMods;
                        Settings.appendToLog("CleanInstallCB checked, running backgroundDelete(" + tanksLocation + "\\res_mods)");
                        //delete everything in res_mods
                        if (!Directory.Exists(tanksLocation + "\\res_mods"))
                            Directory.CreateDirectory(tanksLocation + "\\res_mods");
                        if (!Directory.Exists(tanksLocation + "\\mods"))
                            Directory.CreateDirectory(tanksLocation + "\\mods");
                        this.backgroundDelete(tanksLocation + "\\res_mods");
                        return;
                    }
                    Settings.appendToLog("CleanInstallCB not checked, moving to extraction");
                    state = InstallState.extractRelhaxMods;
                    this.backgroundExtract(false);
                    return;
                }
                else if (state == InstallState.extractRelhaxMods)
                {
                    if (Settings.cleanInstallation)
                    {
                        state = InstallState.deleteResMods;
                        Settings.appendToLog("CleanInstallCB checked, running backgroundDelete(" + tanksLocation + "\\res_mods)");
                        //delete everything in res_mods
                        if (Directory.Exists(tanksLocation + "\\res_mods")) this.backgroundDelete(tanksLocation + "\\res_mods");
                        return;
                    }
                    state = InstallState.extractRelhaxMods;
                    Settings.appendToLog("CleanInstallCB not checked, moving to extraction");
                    this.backgroundExtract(false);
                    return;
                }
                else if (state == InstallState.extractUserMods)
                {
                    this.backgroundExtract(true);
                    return;
                }
                else
                {
                    return;
                }
            }

        }
        //main method for executing all Modpack and user patches
        private void patchFiles()
        {
            speedLabel.Text = Translations.getTranslatedString("patching") + "...";
            Application.DoEvents();
            //Settings.appendToLog("Starting to patch Relhax Mod Pack");
            //don't do anything if the file does not exist
            if (!Directory.Exists(tanksLocation + @"\_patch"))
                return;
            //Give the OS time to process the folder change...
            System.Threading.Thread.Sleep(100);
            //set the folder properties to read write
            File.SetAttributes(tanksLocation + @"\_patch", FileAttributes.Normal);
            string[] patchFiles = null;
            bool kontinue = false;
            while (!kontinue)
            {
                try
                {
                    //get every patch file in the folder
                    patchFiles = Directory.GetFiles(tanksLocation + @"\_patch", @"*.xml");
                    kontinue = true;
                }
                catch (UnauthorizedAccessException e)
                {
                    Settings.appendToLog("EXCEPTION: UnauthorizedAccessException (call stack traceback)");
                    Settings.appendToLog(e.StackTrace);
                    Settings.appendToLog("inner message: " + e.Message);
                    Settings.appendToLog("source: " + e.Source);
                    Settings.appendToLog("target: " + e.TargetSite);
                    DialogResult res = MessageBox.Show(Translations.getTranslatedString("patchingSystemDeneidAccessMessage"), Translations.getTranslatedString("patchingSystemDeneidAccessHeader"), MessageBoxButtons.RetryCancel);
                    if (res == DialogResult.Cancel)
                    {
                        Application.Exit();
                    }
                }
            }
            //get any other old patches out of memory
            patchList.Clear();
            for (int i = 0; i < patchFiles.Count(); i++)
            {
                //set the attributes to normall
                File.SetAttributes(patchFiles[i],FileAttributes.Normal);
                //add patches to patchList
                this.createPatchList(patchFiles[i]);
            }
            //the actual patch method
            foreach (Patch p in patchList)
            {
                string patchFileOutput = p.file;
                int maxLength = 200;
                if (p.file.Length > maxLength)
                    patchFileOutput = p.file.Substring(0, maxLength);
                downloadProgress.Text = Translations.getTranslatedString("patching") + patchFileOutput + "...";
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
                        Settings.appendToLog("Regex patch, all lines, line by line, " + p.file + ", " + p.search + ", " + p.replace);
                        this.RegxPatch(p.file, p.search, p.replace);
                    }
                    else if (p.lines.Count() == 1 && tempp == -1)
                    {
                        //perform regex patch on entire file, as one whole string
                        Settings.appendToLog("Regex patch, all lines, whole file, " + p.file + ", " + p.search + ", " + p.replace);
                        this.RegxPatch(p.file, p.search, p.replace, -1);
                    }
                    else
                    {
                        foreach (string s in p.lines)
                        {
                            //perform regex patch on specific file lines
                            //will need to be a standard for loop BTW
                            Settings.appendToLog("Regex patch, line " + s + ", " + p.file + ", " + p.search + ", " + p.replace);
                            this.RegxPatch(p.file, p.search, p.replace, int.Parse(s));
                        }
                    }
                }
                else if (p.type.Equals("xml"))
                {
                    //perform xml patch
                    Settings.appendToLog("Xml patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                    this.xmlPatch(p.file, p.path, p.mode, p.search, p.replace);
                }
                else if (p.type.Equals("json"))
                {
                    //perform json patch
                    Settings.appendToLog("Json patch, " + p.file + ", " + p.path + ", " + p.replace);
                    this.jsonPatch(p.file, p.path, p.replace, p.mode);
                }
            }
            //all done, delete the patch folder
            if (Directory.Exists(tanksLocation + "\\_patch"))
                Directory.Delete(tanksLocation + "\\_patch", true);
            Settings.appendToLog("Patching done for Relhax Mod Pack");
        }
        //installs all fonts in the fonts folder, user and custom
        private void installFonts()
        {
            Settings.appendToLog("Checking for fonts to install");
            speedLabel.Text = Translations.getTranslatedString("installingFonts") + "...";
            if (!Directory.Exists(tanksLocation + "\\_fonts"))
            {
                //no fonts to install, done display
                speedLabel.Text = "";
                downloadProgress.Text = Translations.getTranslatedString("done");
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                state = InstallState.idle;
                toggleUIButtons(true);
                Settings.appendToLog("Installation done");
                return;
            }
            string[] fonts = Directory.GetFiles(tanksLocation + "\\_fonts");
            if (fonts.Count() == 0)
            {
                //done display
                speedLabel.Text = "";
                downloadProgress.Text = Translations.getTranslatedString("done");
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                state = InstallState.idle;
                toggleUIButtons(true);
                Settings.appendToLog("Installation done");
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
                downloadProgress.Text = Translations.getTranslatedString("done");
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                state = InstallState.idle;
                toggleUIButtons(true);
                Settings.appendToLog("Installation done");
                return;
            }
            Settings.appendToLog("Installing fonts");
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
                Settings.extractEmbeddedResource(tanksLocation + "\\_fonts", "RelhaxModpack", new List<string>() { "FontReg.exe" });
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
                    Settings.appendToLog("ERROR: could not start font installer");
                    MessageBox.Show(Translations.getTranslatedString("fontsPromptError_1") + tanksLocation + Translations.getTranslatedString("fontsPromptError_1"));
                    return;
                }
                if (Directory.Exists(tanksLocation + "\\_fonts"))
                    Directory.Delete(tanksLocation + "\\_fonts", true);
                speedLabel.Text = "";
                downloadProgress.Text = Translations.getTranslatedString("done");
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                state = InstallState.idle;
                toggleUIButtons(true);
                Settings.appendToLog("Fonts Installed Successfully");
                Settings.appendToLog("Installation done");
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
            Settings.appendToLog("WARNING: user is not admin or power user");
            return false;
        }
        //method to check for updates to the application on startup
        private void checkmanagerUpdates()
        {
            Settings.appendToLog("Starting check for application updates");
            WebClient updater = new WebClient();
            updater.Proxy = null;
            string versionSaveLocation = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 4) + "_version.txt";
            if (File.Exists(versionSaveLocation)) File.Delete(versionSaveLocation);
            string version = updater.DownloadString("http://willster419.atwebpages.com/Applications/RelHaxModPack/manager version.txt");
            versionSave = version;
            if (!version.Equals(managerVersion))
            {
                Settings.appendToLog("exe is out of date. displaying user update window");
                //out of date
                VersionInfo vi = new VersionInfo();
                vi.ShowDialog();
                DialogResult result = vi.result;
                if (result.Equals(DialogResult.Yes))
                {
                    Settings.appendToLog("User accepted downloading new version");
                    //download new version
                    sw.Reset();
                    sw.Start();
                    string newExeName = Application.StartupPath + "\\RelhaxModpack_update" + ".exe";
                    updater.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                    updater.DownloadFileCompleted += new AsyncCompletedEventHandler(updater_DownloadFileCompleted);
                    if (File.Exists(newExeName)) File.Delete(newExeName);
                    updater.DownloadFileAsync(new Uri("http://willster419.atwebpages.com/Applications/RelHaxModPack/RelhaxModpack.exe"), newExeName);
                    Settings.appendToLog("New application download started");
                    currentModDownloading = "update ";
                }
                else
                {
                    Settings.appendToLog("User declined downlading new version");
                    //close the application
                    this.Close();
                }
            }
        }
        //handler for when the update download is complete
        void updater_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            downloadTimer.Enabled = false;
            if (e.Error != null && e.Error.Message.Equals("The remote server returned an error: (404) Not Found."))
            {
                //404
                Settings.appendToLog("ERROR: unable to download new application version");
                MessageBox.Show(Translations.getTranslatedString("cantDownloadNewVersion"));
                this.Close();
            }
            string versionSaveLocation = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 4) + "_version.txt";
            string version = versionSave;
            Settings.extractEmbeddedResource(Application.StartupPath, "RelhaxModpack", new List<string>() { "RelicCopyUpdate.bat" });
            string newExeName = Application.StartupPath + "\\RelicCopyUpdate.bat";
            try
            {
                //System.Diagnostics.Process.Start(newExeName + " /updateExeReplace");
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = newExeName;
                Process installUpdate = new Process();
                installUpdate.StartInfo = info;
                installUpdate.Start();
            }
            catch (Win32Exception)
            {
                Settings.appendToLog("WARNING: could not start new application version");
                MessageBox.Show(Translations.getTranslatedString("cantStartNewApp") + newExeName);
            }
            Application.Exit();
        }
        //parses all instance strings to be used for (un)install processes
        private String parseStrings()
        {
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
            Settings.appendToLog("tanksLocation parsed as " + tanksLocation);
            parsedModsFolder = tanksLocation + "\\res_mods\\" + this.getFolderVersion(tanksLocation);
            Settings.appendToLog("tanks mods version parsed as " + parsedModsFolder);
            modGuiFolder = parsedModsFolder + "\\gui\\soundModes";
            modAudioFolder = parsedModsFolder + "\\audioww";
            modGuiFolderBase = parsedModsFolder + "\\gui";
            customUserMods = Application.StartupPath + "\\RelHaxUserMods";
            Settings.appendToLog("customUserMods parsed as " + Application.StartupPath + "\\RelHaxUserMods");
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
        //main unzip worker method
        private void unzip(string zipFile, string extractFolder)
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
                string thisVersion = this.getFolderVersion(null);
                foreach (ZipEntry ze in zip.Entries)
                {
                    //regex again
                    string s = ze.FileName;
                    if (Regex.IsMatch(s, "versiondir"))
                    {
                        s = Regex.Replace(s, "versiondir", thisVersion);
                    }
                    //put the entries on disk
                    File.AppendAllText(tanksLocation + "\\installedRelhaxFiles.log", s + "\n");
                }
                zip.Dispose();
            }
            if (modPack)
                extractworker.ReportProgress(0);
        }
        //reset the UI and critical componets
        private void reset()
        {
            downloadProgress.Text = Translations.getTranslatedString("idle");
            childProgressBar.Value = 0;
            parrentProgressBar.Value = 0;
            statusLabel.Text = Translations.getTranslatedString("status");
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
                downloadProgress.Text = Translations.getTranslatedString("canceled");
                return null;
            }
            tanksLocation = findWotExe.FileName;
            return "all good";
        }
        //handelr for before the window is displayed
        private void MainWindow_Load(object sender, EventArgs e)
        {
            //set window header text to current version so user knows
            this.Text = this.Text + managerVersion.Substring(8);
            if (Program.testMode) this.Text = this.Text + " TEST MODE";
            //show the wait screen
            PleaseWait wait = new PleaseWait();
            wait.Show();
            WebRequest.DefaultWebProxy = null;
            wait.loadingDescBox.Text = "Verifying single instance...";
            Application.DoEvents();
            //Settings.appendToLog("|------------------------------------------------------------------------------------------------|");
            Settings.appendToLog("|RelHax Modpack " + managerVersion);
            Settings.appendToLog("|Built on 04/22/2017, running at " + DateTime.Now);
            Settings.appendToLog("|Running on " + System.Environment.OSVersion.ToString());
            //Settings.appendToLog("|------------------------------------------------------------------------------------------------|");
            //enforces a single instance of the program
            try
            {
                File.WriteAllText(tempPath + "\\RelHaxOneInstance.txt", "this file is open and cannot be deleted");
                File.OpenWrite(tempPath + "\\RelHaxOneInstance.txt");
                Settings.appendToLog("Successfully made single instance text file");
            }
            //catching an EXCEPTION means that this is not the only instance open
            catch (IOException)
            {
                wait.Close();
                Settings.appendToLog("CRITICAL: Another Instance of the relic mod manager is already running");
                MessageBox.Show("CRITICAL: Another Instance of the relic mod manager is already running");
                this.Close();
            }
            //check for updates
            wait.loadingDescBox.Text = Translations.getTranslatedString("checkForUpdates");
            Application.DoEvents();
            if (Program.skipUpdate)
            {
                Settings.appendToLog("/skip-update switch detected, skipping application update");
                if (!Program.testMode) MessageBox.Show(Translations.getTranslatedString("skipUpdateWarning"));
            }
            else
            {
                this.checkmanagerUpdates();
            }
            //add method to disable the modpack for during patch day
            //this will involve having a hard coded true or false, along with a command line arguement to over-ride
            //to disable from patch day set it to false.
            //to enable for patch day (prevent users to use it), set it to true.
            if (false && !Program.patchDayTest)
            {
                Settings.appendToLog("Patch day disable detected. Remember To override use /patchday");
                MessageBox.Show(Translations.getTranslatedString("patchDayMessage"));
                this.Close();
            }
            wait.loadingDescBox.Text = Translations.getTranslatedString("verDirStructure");
            Application.DoEvents();
            //create directory structures
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxDownloads")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxDownloads");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxUserMods")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxUserMods");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxModBackup")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxModBackup");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxUserConfigs")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxUserConfigs");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxTemp")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxTemp");
            
            //load settings
            wait.loadingDescBox.Text = Translations.getTranslatedString("loadingSettings");
            Settings.appendToLog("Loading settings");
            Settings.loadSettings();
            this.applySettings();
            if (Program.testMode)
            {
                Settings.appendToLog("Test Mode is ON, loading local modInfo.xml");
            }
            if (Program.autoInstall)
            {
                Settings.appendToLog("Auto Install is ON, checking for config pref xml at " + Application.StartupPath + "\\RelHaxUserConfigs\\" + Program.configName);
                if (!File.Exists(Application.StartupPath + "\\RelHaxUserConfigs\\" + Program.configName))
                {
                    Settings.appendToLog(Translations.getTranslatedString("extractionErrorHeader") + ": " + Program.configName + " does NOT exist, loading in regular mode");
                    MessageBox.Show("ERROR: " + Program.configName + " does NOT exist, loading in regular mode");
                    Program.autoInstall = false;
                }
                if (!Settings.cleanInstallation)
                {
                    Settings.appendToLog("ERROR: clean installation is set to false. This must be set to true for auto install to work. Loading in regular mode.");
                    MessageBox.Show(Translations.getTranslatedString("autoAndClean"));
                    Program.autoInstall = false;
                }
                if (Settings.firstLoad)
                {
                    Settings.appendToLog("ERROR: First time loading cannot be an auto install mode, loading in regular mode");
                    MessageBox.Show(Translations.getTranslatedString("autoAndFirst"));
                    Program.autoInstall = false;
                }
            }
            //check if it can still load in autoInstall config mode
            if (Program.autoInstall)
            {
                Settings.appendToLog("Program.autoInstall still true, loading in auto install mode");
                wait.Close();
                state = InstallState.idle;
                this.installRelhaxMod_Click(null, null);
                return;
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
            toggleUIButtons(true);
            Application.DoEvents();
            Program.saveSettings = true;
        }
        //when the "visit form page" link is clicked. the link clicked handler
        private void formPageLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.com/index.php?/topic/535868-09171-the-relhax-modpack/");
        }
        //method to patch a part of an xml file
        //fileLocation is relative to res_mods folder
        public void xmlPatch(string filePath, string xpath, string mode, string search, string replace, bool testMods = false)
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
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                filePath = tanksLocation + "\\res_mods" + filePath;
            }

            if (testMods)
            {

            }
            else
            {
                //patch versiondir out of filePath
                filePath = Regex.Replace(filePath, "versiondir", tanksVersion);
            }
            
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
        public void RegxPatch(string fileLocation, string search, string replace, int lineNumber = 0, bool testMods = false)
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
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                fileLocation = tanksLocation + "\\res_mods" + fileLocation;
            }

            if (testMods)
            {

            }
            else
            {
                //patch versiondir out of fileLocation
                fileLocation = Regex.Replace(fileLocation, "versiondir", tanksVersion);
            }

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
                try
                {
                    if (Regex.IsMatch(file, search))
                    {
                        file = Regex.Replace(file, search, replace);
                    }
                    file = Regex.Replace(file, "newline", "\n");
                    sb.Append(file);
                }
                catch (ArgumentException)
                {
                    if (testMods) MessageBox.Show("invalid regex command");
                }
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
        public void jsonPatch(string jsonFile, string jsonPath, string newValue, string mode, bool testMods = false)
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
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                jsonFile = tanksLocation + "\\res_mods" + jsonFile;
            }

            //patch versiondir out of fileLocation
            if (testMods)
            {

            }
            else
            {
                jsonFile = Regex.Replace(jsonFile, "versiondir", tanksVersion);
            }

            //check that the file exists
            if (!File.Exists(jsonFile))
                return;

            //load file from disk...
            string file = File.ReadAllText(jsonFile);
            //save the "$" lines
            List<StringSave> ssList = new List<StringSave>();
            StringBuilder backTogether = new StringBuilder();
            string[] removeComments = file.Split('\n');
            for (int i = 0; i < removeComments.Count(); i++)
            {
                string temp = removeComments[i];
                if (Regex.IsMatch(temp, @"\${"))
                {
                    bool hadComma = false;
                    if (Regex.IsMatch(temp, ","))
                    {
                        hadComma = true;
                    }
                    StringSave ss = new StringSave();
                    ss.name = temp.Split('"')[1];
                    ss.value = temp.Split('$')[1];
                    ssList.Add(ss);
                    temp = "\"" + ss.name + "\"" + ": -69420";
                    if (hadComma)
                        temp = temp + ",";
                }
                backTogether.Append(temp + "\n");
            }
            file = backTogether.ToString();
            JsonLoadSettings settings = new JsonLoadSettings();
            settings.CommentHandling = CommentHandling.Ignore;
            JObject root = null;
            //load json for editing
            try
            {
                root = JObject.Parse(file,settings);
            }
            catch (JsonReaderException)
            {
                Settings.appendToLog("ERROR: Failed to patch " + jsonFile);
                //MessageBox.Show("ERROR: Failed to patch " + jsonFile);
                if (Program.testMode)
                {
                    //in test mode this is worthy of an EXCEPTION
                    throw new JsonReaderException();
                }
            }
            //if it failed to parse show the message (above) and pull out
            if (root == null)
                return;
            if (mode == null || mode.Equals("") || mode.Equals("edit") || mode.Equals("arrayEdit"))
            {
                //the actual patch method
                JValue newObject = (JValue)root.SelectToken(jsonPath);
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Settings.appendToLog("ERROR: path " + jsonPath + " not found for " + Path.GetFileName(jsonFile));
                }
                else if (useBool)
                {
                    newObject.Value = newValueBool;
                }
                else if (useInt)
                {
                    newObject.Value = newValueInt;
                }
                else if (useDouble)
                {
                    newObject.Value = newValueDouble;
                }
                else //string
                {
                    newObject.Value = newValue;
                }
            }
            else if (mode.Equals("remove"))
            {
                //TODO
            }
            else if (mode.Equals("arrayRemove"))
            {
                //TODO
            }
            else if (mode.Equals("add"))
            {
                //TODO
            }
            else if (mode.Equals("arrayAdd"))
            {
                //TODO
            }
            else
            {
                Settings.appendToLog("ERROR: Unknown json patch mode, " + mode);
            }
            StringBuilder rebuilder = new StringBuilder();
            string[] putBackDollas = root.ToString().Split('\n');
            for (int i = 0; i < putBackDollas.Count(); i++)
            {
                string temp = putBackDollas[i];
                if (Regex.IsMatch(temp, "-69420"))//look for the temp value
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
            toggleUIButtons(false);
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
                MessageBox.Show(Translations.getTranslatedString("autoDetectFailed"));
                state = InstallState.error;
                toggleUIButtons(true);
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
            //reset the childProgressBar value
            childProgressBar.Maximum = 100;
            childProgressBar.Value = 0;
            //show the mod selection window
            ModSelectionList list = new ModSelectionList(tanksVersion);
            list.ShowDialog();
            if (list.cancel)
            {
                state = InstallState.idle;
                toggleUIButtons(true);
                return;
            }
            if (File.Exists(tanksLocation + "\\installedRelhaxFiles.log"))
                File.Delete(tanksLocation + "\\installedRelhaxFiles.log");
            downloadProgress.Text = Translations.getTranslatedString("loading");
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
                        //add dependencies
                        foreach (Dependency d in c.dependencies)
                        {
                            //check dependency is enabled and has a zip file with it
                            if (d.enabled && !d.dependencyZipFile.Equals(""))
                                this.addUniqueDependency(d);
                        }
                        foreach (Config cc in m.configs)
                        {
                            //check to make sureit's enabled and checked and has a valid zip file with it
                            if (cc.enabled && cc.configChecked && !cc.zipConfigFile.Equals(""))
                            {
                                //same for configs
                                configsToInstall.Add(cc);
                            }
                            //check to see if any catagory dependencies need to be added
                            if (cc.enabled && cc.configChecked)
                            {
                                foreach (Dependency d in cc.catDependencies)
                                {
                                    //check dependency is enabled and has a zip file with it
                                    if (d.enabled && !d.dependencyZipFile.Equals(""))
                                        this.addUniqueDependency(d);
                                }
                            }
                        }
                        foreach (Dependency d in m.modDependencies)
                        {
                            //check dependency is enabled and has a zip file with it
                            if (d.enabled && !d.dependencyZipFile.Equals(""))
                                this.addUniqueDependency(d);
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
                downloadProgress.Text = Translations.getTranslatedString("idle");
                toggleUIButtons(true);
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
                cancelDownloadButton.Enabled = true;
                cancelDownloadButton.Visible = true;
                //delete temp/0kb/corrupt file
                if (File.Exists(downloadQueue[0].zipFile)) File.Delete(downloadQueue[0].zipFile);
                //download new zip file
                downloader = new WebClient();
                downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                downloader.Proxy = null;
                downloader.DownloadFileAsync(downloadQueue[0].URL, downloadQueue[0].zipFile);
                tempOldDownload = Path.GetFileName(downloadQueue[0].zipFile);
                Settings.appendToLog("downloading " + tempOldDownload);
                currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile);
                if (currentModDownloading.Length >= 200)
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
            string crc = Settings.GetMd5Hash(localFile);
            if (crc.Equals(remoteCRC))
                return true;
            return false;
        }
        //Main method to uninstall the modpack
        private void uninstallRelhaxMod_Click(object sender, EventArgs e)
        {
            modPack = true;
            toggleUIButtons(false);
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
                MessageBox.Show(Translations.getTranslatedString("autoDetectFailed"));
                return;
            }
            if (MessageBox.Show(Translations.getTranslatedString("confirmUninstallMessage"), Translations.getTranslatedString("confirmUninstallHeader"), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (Settings.cleanUninstall)
                {
                    //run the recursive complete uninstaller
                    downloadProgress.Text = Translations.getTranslatedString("uninstalling") + "...";
                    state = InstallState.uninstallResMods;
                    this.backgroundDelete(tanksLocation + "\\res_mods");
                }
                else
                {
                    //run the smart uninstaller
                    this.newUninstallMethod();
                }
            }
            else
            {
                toggleUIButtons(true);
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
        //uses backgroundWorker to copy files
        private void backgroundCopy(string source, string dest)
        {
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            downloadProgress.Text = Translations.getTranslatedString("copyingFile") + numFilesToCopyDeleteExtract + Translations.getTranslatedString("of") + numFilesToProcessInt;
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
            downloadProgress.Text = Translations.getTranslatedString("deletingFile") + numFilesToCopyDeleteExtract + Translations.getTranslatedString("of") + numFilesToProcessInt;
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
            downloadProgress.Text = Translations.getTranslatedString("loadingExtractionText") + "...";
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
                speedLabel.Text = Translations.getTranslatedString("extractingRelhaxMods") + "...";
                parrentProgressBar.Maximum = modsToInstall.Count + configsToInstall.Count + dependencies.Count;
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
            }
            else
            {
                speedLabel.Text = Translations.getTranslatedString("extractingUserMods") + "...";
                parrentProgressBar.Maximum = userMods.Count;
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
            }

            extractworker.RunWorkerAsync(parameters);
        }
        //uses a backgroundWorker to smart uninstall
        private void backgroundSmartUninstall()
        {
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            parrentProgressBar.Value = parrentProgressBar.Maximum;
            downloadProgress.Text = Translations.getTranslatedString("startingSmartUninstall");
            Settings.appendToLog("Starting smart uninstall");
            smartDeleteworker = new BackgroundWorker();
            smartDeleteworker.WorkerReportsProgress = true;
            smartDeleteworker.DoWork += new DoWorkEventHandler(smartDeleteworker_DoWork);
            smartDeleteworker.ProgressChanged += new ProgressChangedEventHandler(smartDeleteworker_ProgressChanged);
            smartDeleteworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(smartDeleteworker_RunWorkerCompleted);
            smartDeleteworker.RunWorkerAsync();
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
            downloadProgress.Text = Translations.getTranslatedString("copyingFile") + numFilesToCopyDeleteExtract + Translations.getTranslatedString("of") + numFilesToProcessInt;
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
            downloadProgress.Text = Translations.getTranslatedString("deletingFile") + numFilesToCopyDeleteExtract + Translations.getTranslatedString("of") + numFilesToProcessInt;
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
                    toggleUIButtons(true);
                    if (!Directory.Exists(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null));
                    if (!Directory.Exists(tanksLocation + "\\mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\mods\\" + this.getFolderVersion(null));
                    downloadProgress.Text = Translations.getTranslatedString("done");
                    childProgressBar.Value = 0;
                    return;
                }
            }
            else if (state == InstallState.uninstallMods)
            {
                //finish uninstallResMods process
                state = InstallState.idle;
                toggleUIButtons(true);
                if (File.Exists(tanksLocation + "\\installedRelhaxFiles.log"))
                    File.Delete(tanksLocation + "\\installedRelhaxFiles.log");
                if (!Directory.Exists(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null));
                if (!Directory.Exists(tanksLocation + "\\mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\mods\\" + this.getFolderVersion(null));
                downloadProgress.Text = Translations.getTranslatedString("done");
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
                Settings.appendToLog("Starting Relhax Modpack Extraction");
                string downloadedFilesDir = Application.StartupPath + "\\RelHaxDownloads\\";
                //extract dependencies
                foreach (Dependency d in dependencies)
                {
                    Settings.appendToLog("Extracting Dependency " + d.dependencyZipFile);
                    if (!d.dependencyZipFile.Equals("")) this.unzip(downloadedFilesDir + d.dependencyZipFile, tanksLocation);
                    extractworker.ReportProgress(1);
                }
                //extract mods
                foreach (Mod m in modsToInstall)
                {
                    Settings.appendToLog("Extracting Mod " + m.modZipFile);
                    if (!m.modZipFile.Equals("")) this.unzip(downloadedFilesDir + m.modZipFile, tanksLocation);
                    extractworker.ReportProgress(1);
                }
                //extract configs
                foreach (Config c in configsToInstall)
                {
                    Settings.appendToLog("Extracting Config " + c.zipConfigFile);
                    if (!c.zipConfigFile.Equals("")) this.unzip(downloadedFilesDir + c.zipConfigFile, tanksLocation);
                    extractworker.ReportProgress(1);
                }
                Settings.appendToLog("Finished Relhax Modpack Extraction");
            }
            else
            {
                //extract user mods
                Settings.appendToLog("Starting Relhax Modpack User Mod Extraction");
                string downloadedFilesDir = Application.StartupPath + "\\RelHaxUserMods\\";
                foreach (Mod m in userMods)
                {
                    if (m.modChecked)
                    {
                        Settings.appendToLog("Exracting " + Path.GetFileName(m.modZipFile));
                        this.unzip(downloadedFilesDir + Path.GetFileName(m.modZipFile), tanksLocation);
                        extractworker.ReportProgress(1);
                    }
                }
                Settings.appendToLog("Finished Extracting Relhax Modpack User Mod Extraction");
            }

        }
        //handler for the extractworker when progress is made
        private void extractworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (childProgressBar.Maximum != childMaxProgres)
                    childProgressBar.Maximum = childMaxProgres;
                if (childCurrentProgres != 0)
                    childProgressBar.Value = childCurrentProgres;
                if (true)
                {
                    downloadProgress.Text = currentZipEntry + "...";
                }
                if (isParrentDone)
                {
                    if (parrentProgressBar.Value != parrentProgressBar.Maximum)
                        parrentProgressBar.Value++;
                }
            }
            catch (NullReferenceException ne)
            {
                Settings.appendToLog("EXCEPTION: traceback from instance: " + ne.InnerException);
                Settings.appendToLog("Message: " + ne.Message);
                Settings.appendToLog("From source: " + ne.Source);
                Settings.appendToLog("Callstack: " + ne.StackTrace);
                MessageBox.Show(Translations.getTranslatedString("specialMessage1"));
            }
        }
        //handler for when the extractworker is completed
        private void extractworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (state == InstallState.extractRelhaxMods)
            {
                state = InstallState.patchRelhaxMods;
                Settings.appendToLog("Starting to patch Relhax Modpack Mods");
                this.patchFiles();
                state = InstallState.extractUserMods;
                this.backgroundExtract(true);
                return;
            }
            else if (state == InstallState.extractUserMods)
            {
                state = InstallState.patchUserMods;
                Settings.appendToLog("Starting to patch Relhax User Mods");
                this.patchFiles();
                state = InstallState.restoreUserData;
                this.restoreUserData();
                state = InstallState.installFonts;
                this.installFonts();
                return;
            }
            else
            {
                return;
            }
        }
        //the main worker method for the smart uninstaller
        private void smartDeleteworker_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] createdFiles = File.ReadAllLines(tanksLocation + "\\installedRelhaxFiles.log");
            //sort into directories and files
            List<string> files = new List<string>();
            List<string> resModsFolders = new List<string>();
            List<string> modsFolders = new List<string>();
            foreach (string s in createdFiles)
            {
                if (Regex.IsMatch(s, @"\.[A-Za-z0-9_\-]*$"))
                {
                    //it's a files
                    files.Add(s);
                }
                else
                {
                    //it's a folder
                    if (Regex.IsMatch(s, "res_mods"))
                    {
                        //it's a res_mods folder
                        resModsFolders.Add(s);
                    }
                    else
                    {
                        //it's a mods folder
                        modsFolders.Add(s);
                    }
                }
            }
            numFilesToProcessInt = files.Count;
            //delete all the files
            foreach (string s in files)
            {
                string filePath = tanksLocation + "\\" + s;
                if (File.Exists(filePath))
                {
                    //Settings.appendToLog("Deleting file " + filePath);
                    File.Delete(filePath);
                    smartDeleteworker.ReportProgress(numFilesToCopyDeleteExtract++);
                }
            }
            //delete all the folders if nothing else is in them
            Settings.appendToLog("Finished deleting, processing mods folder");
            this.processDirectory(tanksLocation + "\\mods");
            Settings.appendToLog("processing res_mods folder");
            this.processDirectory(tanksLocation + "\\res_mods");
            Settings.appendToLog("creating directories if they arn't already there");
            if (!Directory.Exists(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null));
            if (!Directory.Exists(tanksLocation + "\\mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\mods\\" + this.getFolderVersion(null));
        }
        //the method to update the UI on the uninstall process
        private void smartDeleteworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            downloadProgress.Text = Translations.getTranslatedString("deletingFile") + numFilesToCopyDeleteExtract + Translations.getTranslatedString("of") + numFilesToProcessInt;
            childProgressBar.Maximum = numFilesToProcessInt;
            if (numFilesToCopyDeleteExtract < numFilesToProcessInt)
                childProgressBar.Value = numFilesToCopyDeleteExtract;
        }
        //the method to run when the smart uninstall is compete
        private void smartDeleteworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            downloadProgress.Text = Translations.getTranslatedString("done");
            childProgressBar.Value = 0;
            parrentProgressBar.Value = 0;
            Settings.appendToLog("Uninstall complete");
            state = InstallState.idle;
            toggleUIButtons(true);
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
                        File.SetAttributes(file.FullName, FileAttributes.Normal);
                        file.Delete();
                        tryAgain = false;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Settings.appendToLog("EXCEPTION: UnauthorizedAccessException (call stack traceback)");
                        Settings.appendToLog(e.StackTrace);
                        Settings.appendToLog("inner message: " + e.Message);
                        Settings.appendToLog("source: " + e.Source);
                        Settings.appendToLog("target: " + e.TargetSite);
                        DialogResult res = MessageBox.Show(Translations.getTranslatedString("extractionErrorMessage"), Translations.getTranslatedString("extractionErrorHeader"), MessageBoxButtons.RetryCancel);
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
                    bool tryAgain = true;
                    while (tryAgain)
                    {
                        try
                        {
                            File.SetAttributes(subdir.FullName, FileAttributes.Normal);
                            subdir.Delete();
                            tryAgain = false;
                        }
                        catch (IOException e)
                        {
                            Settings.appendToLog("EXCEPTION: IOException (call stack traceback)");
                            Settings.appendToLog(e.StackTrace);
                            Settings.appendToLog("inner message: " + e.Message);
                            Settings.appendToLog("source: " + e.Source);
                            Settings.appendToLog("target: " + e.TargetSite);
                            DialogResult result = MessageBox.Show(Translations.getTranslatedString("deleteErrorMessage"), Translations.getTranslatedString("deleteErrorHeader"), MessageBoxButtons.RetryCancel);
                            if (result == DialogResult.Cancel)
                                Application.Exit();
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Settings.appendToLog("EXCEPTION: UnauthorizedAccessException (call stack traceback)");
                            Settings.appendToLog(e.StackTrace);
                            Settings.appendToLog("inner message: " + e.Message);
                            Settings.appendToLog("source: " + e.Source);
                            Settings.appendToLog("target: " + e.TargetSite);
                            DialogResult result = MessageBox.Show(Translations.getTranslatedString("deleteErrorMessage"), Translations.getTranslatedString("deleteErrorHeader"), MessageBoxButtons.RetryCancel);
                            if (result == DialogResult.Cancel)
                                Application.Exit();
                        }
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
        //handler for when the window is goingto be closed
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save settings
            if (Program.saveSettings) Settings.saveSettings();
            Settings.appendToLog("Application Closing");
            Settings.appendToLog("|------------------------------------------------------------------------------------------------|");
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
            this.forceManuel.Text = Translations.getTranslatedString(forceManuel.Name);
            this.cleanInstallCB.Checked = Settings.cleanInstallation;
            this.cleanInstallCB.Text = Translations.getTranslatedString(cleanInstallCB.Name);
            this.backupModsCheckBox.Checked = Settings.backupModFolder;
            this.backupModsCheckBox.Text = Translations.getTranslatedString(backupModsCheckBox.Name);
            this.cancerFontCB.Checked = Settings.comicSans;
            this.cancerFontCB.Text = Translations.getTranslatedString(cancerFontCB.Name);
            this.largerFontButton.Checked = Settings.largeFont;
            this.largerFontButton.Text = Translations.getTranslatedString(largerFontButton.Name);
            this.saveLastInstallCB.Checked = Settings.saveLastConfig;
            this.saveLastInstallCB.Text = Translations.getTranslatedString(saveLastInstallCB.Name);
            this.saveUserDataCB.Checked = Settings.saveUserData;
            this.saveUserDataCB.Text = Translations.getTranslatedString(saveUserDataCB.Name);
            this.cleanUninstallCB.Checked = Settings.cleanUninstall;
            this.cleanUninstallCB.Text = Translations.getTranslatedString(cleanUninstallCB.Name);
            this.darkUICB.Checked = Settings.darkUI;
            this.darkUICB.Text = Translations.getTranslatedString(darkUICB.Name);
            this.installRelhaxMod.Text = Translations.getTranslatedString(installRelhaxMod.Name);
            this.uninstallRelhaxMod.Text = Translations.getTranslatedString(uninstallRelhaxMod.Name);
            this.settingsGroupBox.Text = Translations.getTranslatedString(settingsGroupBox.Name);
            this.loadingImageGroupBox.Text = Translations.getTranslatedString(loadingImageGroupBox.Name);
            this.languageSelectionGB.Text = Translations.getTranslatedString(languageSelectionGB.Name);
            this.statusLabel.Text = Translations.getTranslatedString("status");
            this.findBugAddModLabel.Text = Translations.getTranslatedString(findBugAddModLabel.Name);
            this.formPageLink.Text = Translations.getTranslatedString(formPageLink.Name);
            if (helper != null)
            {
                helper.helperText.Text = Translations.getTranslatedString("helperText");
            }
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
            switch (Translations.language)
            {
                case (Translations.Languages.English):
                    //set english button
                    languageENG.CheckedChanged -= languageENG_CheckedChanged;
                    languageENG.Checked = true;
                    languageENG.CheckedChanged += languageENG_CheckedChanged;
                    break;
                case (Translations.Languages.German):
                    //set english button
                    languageGER.CheckedChanged -= languageGER_CheckedChanged;
                    languageGER.Checked = true;
                    languageGER.CheckedChanged += languageGER_CheckedChanged;
                    break;
            }
        }
        //handler for when the "standard" loading animation is cleicked
        private void standardImageRB_CheckedChanged(object sender, EventArgs e)
        {
            if (standardImageRB.Checked)
            {
                Settings.gif = Settings.LoadingGifs.standard;
            }
            else if (thirdGuardsLoadingImageRB.Checked)
            {
                Settings.gif = Settings.LoadingGifs.thirdGuards;
            }
        }

        private void forceManuel_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("forceManuelDescription");
        }

        private void forceManuel_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void cleanInstallCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("cleanInstallDescription");
        }

        private void cleanInstallCB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void backupModsCheckBox_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("backupModsDescription");
        }

        private void backupModsCheckBox_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void cancerFontCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("comicSansDescription");
        }

        private void cancerFontCB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void largerFontButton_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("enlargeFontDescription");
        }

        private void largerFontButton_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void standardImageRB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("selectGifDesc");
        }

        private void standardImageRB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void findBugAddModLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.google.com/spreadsheets/d/1LmPCMAx0RajW4lVYAnguHjjd8jArtWuZIGciFN76AI4/edit?usp=sharing");
        }

        private void saveLastInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.saveLastConfig = saveLastInstallCB.Checked;
        }

        private void saveLastInstallCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("saveLastConfigInstall");
        }

        private void saveLastInstallCB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void cancelDownloadButton_Click(object sender, EventArgs e)
        {
            downloader.CancelAsync();
        }

        private void forceManuel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("forceManuelDescription");
            newHelper.ShowDialog();
        }

        private void cleanInstallCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("cleanInstallDescription");
            newHelper.ShowDialog();
        }

        private void backupModsCheckBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("backupModsDescription");
            newHelper.ShowDialog();
        }

        private void cancerFontCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("comicSansDescription");
            newHelper.ShowDialog();
        }

        private void largerFontButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("enlargeFontDescription");
            newHelper.ShowDialog();
        }

        private void saveLastInstallCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("saveLastConfigInstall");
            newHelper.ShowDialog();
        }

        private void backupUserData()
        {
            List<Mod> modsWithData = new List<Mod>();
            foreach (Mod m in modsToInstall)
            {
                if (m.userFiles.Count > 0)
                    modsWithData.Add(m);
            }
            foreach (Mod m in modsWithData)
            {
                foreach (string s in m.userFiles)
                {
                    string startLoc = tanksLocation + s;
                    string destLoc = Application.StartupPath + "\\RelHaxTemp\\" + m.name + "_" + Path.GetFileName(s);
                    if (File.Exists(startLoc))
                        File.Move(startLoc, destLoc);
                }
            }
        }

        private void restoreUserData()
        {
            List<Mod> modsWithData = new List<Mod>();
            foreach (Mod m in modsToInstall)
            {
                if (m.userFiles.Count > 0)
                    modsWithData.Add(m);
            }
            string[] fileList = Directory.GetFiles(Application.StartupPath + "\\RelHaxTemp");
            foreach (Mod m in modsWithData)
            {
                foreach (string s in m.userFiles)
                {
                    //find the file
                    string parsedFileName = m.name + "_" + Path.GetFileName(s);
                    foreach (string ss in fileList)
                    {
                        string thePath = Path.GetFileName(ss);
                        if (thePath.Equals(parsedFileName))
                        {
                            //the file has been found in the temp directory
                            if (!Directory.Exists(tanksLocation + "\\" + Path.GetFullPath(s)))
                                Directory.CreateDirectory(tanksLocation + "\\" + Path.GetDirectoryName(s));
                            if (File.Exists(tanksLocation + "\\" + s))
                                File.Delete(tanksLocation + "\\" + s);
                            File.Move(ss, tanksLocation + "\\" + s);
                        }
                    }
                }
            }
        }

        private void saveUserDataCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.saveUserData = saveUserDataCB.Checked;
        }

        private void saveUserDataCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("saveUserDataDesc");
            newHelper.ShowDialog();
        }

        private void saveUserDataCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("saveUserDataDesc");
        }

        private void saveUserDataCB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }
        //new unistall method
        private void newUninstallMethod()
        {
            Settings.appendToLog("Started Uninstallation process");
            if (!File.Exists(tanksLocation + "\\installedRelhaxFiles.log"))
            {
                Settings.appendToLog("ERROR: installedRelhaxFiles.log does not exist, prompt user to delete everything instead");
                DialogResult result = MessageBox.Show(Translations.getTranslatedString("noUninstallLogMessage"), Translations.getTranslatedString("noUninstallLogHeader"), MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    state = InstallState.uninstallResMods;
                    Settings.appendToLog("User said yes to delete");
                    this.backgroundDelete(tanksLocation + "\\res_mods");
                    return;
                }
                Settings.appendToLog("User said no, aborting");
                return;
            }
            state = InstallState.smartUninstall;
            this.backgroundSmartUninstall();
            return;
        }
        //deletes all empty directories from a given start location
        private void processDirectory(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                processDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Settings.appendToLog("Deleting empty directory " + directory);
                    Directory.Delete(directory, false);
                }
            }
        }

        private void downloadTimer_Tick(object sender, EventArgs e)
        {
            differenceTotalBytesDownloaded = currentTotalBytesDownloaded - previousTotalBytesDownloaded;
            float intervalInSeconds = (float)downloadTimer.Interval / 1000;
            float sessionMBytesDownloaded = differenceTotalBytesDownloaded / MBDivisor;
            sessionDownloadSpeed = sessionMBytesDownloaded / intervalInSeconds;

            //set the previous for the last amount of bytes downloaded
            previousTotalBytesDownloaded = currentTotalBytesDownloaded;
        }

        private void cleanUninstallCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.cleanUninstall = cleanUninstallCB.Checked;
        }

        private void cleanUninstallCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("cleanUninstallDescription");
        }

        private void cleanUninstallCB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void cleanUninstallCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("cleanUninstallDescription");
            newHelper.ShowDialog();
        }

        private void darkUICB_CheckedChanged(object sender, EventArgs e)
        {
            //set the thing
            Settings.darkUI = darkUICB.Checked;
            Settings.setUIColor(this);
        }
        
        private void darkUICB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("darkUIDesc");
        }

        private void darkUICB_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void darkUICB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y);
            newHelper.helperText.Text = Translations.getTranslatedString("darkUIDesc");
            newHelper.ShowDialog();
        }

        private void standardImageRB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;
            RadioButton rb = (RadioButton)sender;
            Settings.LoadingGifs backup = Settings.gif;
            if (rb.Name.Equals("standardImageRB"))
            {
                Settings.gif = Settings.LoadingGifs.standard;
            }
            else if (rb.Name.Equals("thirdGuardsLoadingImageRB"))
            {
                Settings.gif = Settings.LoadingGifs.thirdGuards;
            }
            else
                return;
            //create the preview
            if (gp != null)
            {
                gp.Close();
                gp = null;
            }
            gp = new loadingGifPreview(this.Location.X + this.Size.Width + 5, this.Location.Y);
            gp.Show();
            Settings.gif = backup;
        }
        //toggle UI buttons to be enalbed or disabled
        private void toggleUIButtons(bool enableToggle)
        {
            forceManuel.Enabled = enableToggle;
            installRelhaxMod.Enabled = enableToggle;
            uninstallRelhaxMod.Enabled = enableToggle;
            cleanInstallCB.Enabled = enableToggle;
            cancerFontCB.Enabled = enableToggle;
            backupModsCheckBox.Enabled = enableToggle;
            darkUICB.Enabled = enableToggle;
            cleanUninstallCB.Enabled = enableToggle;
            saveUserDataCB.Enabled = enableToggle;
            saveLastInstallCB.Enabled = enableToggle;
            largerFontButton.Enabled = enableToggle;
        }
        
        private void languageENG_CheckedChanged(object sender, EventArgs e)
        {
            Translations.language = Translations.Languages.English;
            this.applySettings();
        }
        
        private void languageGER_CheckedChanged(object sender, EventArgs e)
        {
            Translations.language = Translations.Languages.German;
            this.applySettings();
        }
        //adds a dependency to the dependency list only if it is not already added
        private void addUniqueDependency(Dependency toAdd)
        {
            foreach (Dependency existing in dependencies)
            {
                //check if the mod zip name is the same
                if (existing.dependencyZipFile.Equals(toAdd.dependencyZipFile))
                    return;
            }
            //getting here means that the dependency to add is unique
            dependencies.Add(toAdd);
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
