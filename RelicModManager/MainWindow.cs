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

namespace RelicModManager
{
    public partial class MainWindow : Form
    {
        //all instance variables required to be up here
        private FolderBrowserDialog selectWotFolder = new FolderBrowserDialog();
        private WebClient downloader = new WebClient();
        private string modAudioFolder;
        private string tempPath = Path.GetTempPath();
        private static int MBDivisor = 1048576;
        private string managerVersion = "version 17.4";
        private string tanksLocation;
        private SelectFeatures features = new SelectFeatures();
        private List<DownloadItem> downloadQueue;
        private string appPath = Application.StartupPath;
        private string downloadPath = Application.StartupPath + "\\RelHaxDownloads";
        private string parsedModsFolder;//0.9.x.y.z
        private string modGuiFolder;
        private string modGuiFolderBase;
        private string customUserMods;
        ZipFile zip;
        Stopwatch sw = new Stopwatch();
        private string downloadURL = "https://dl.dropboxusercontent.com/u/44191620/RelicMod/mods/";
        private List<Catagory> parsedCatagoryLists;
        private List<Mod> modsToInstall;
        private List<Config> configsToInstall;
        private List<Patch> patchList;
        private List<Dependency> dependencies;
        bool modPack;
        string tempOldDownload;
        private List<Mod> userMods;
        int numFilesToProcessInt = 0;
        int numFilesToCopyDeleteExtract = 0;
        bool RelHaxUninstall = false;
        bool userExtract = false;

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
            if (this.autoFindTanks() == null || this.forceManuel.Checked)
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
            downloadProgress.Text = "Downloaded " + MBytesIn + " MB" + " of " + MBytesTotal + " MB";
            childProgressBar.Value = e.ProgressPercentage;
            speedLabel.Text = string.Format("{0} MB/s", (e.BytesReceived / 1048576d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            if (MBytesIn == 0 && MBytesTotal == 0)
            {
                this.downloadProgress.Text = "Complete!";
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
                    //TODO:check for CRC verification before deciding you have to re-download
                    //TODO: change the download folder location to a "data" folder relative to the path of the application
                    if (File.Exists(downloadQueue[0].zipFile)) File.Delete(downloadQueue[0].zipFile);
                    //download new zip file
                    downloader = new WebClient();
                    downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                    downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                    downloader.DownloadFileAsync(downloadQueue[0].URL, downloadQueue[0].zipFile);
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
                    MessageBox.Show("Failed to download " + tempOldDownload + ". If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it crashes");
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
                    downloader.DownloadFileAsync(downloadQueue[0].URL, downloadQueue[0].zipFile);
                    tempOldDownload = Path.GetFileName(downloadQueue[0].zipFile);
                    this.appendToLog("downloading " + tempOldDownload);
                    downloadQueue.RemoveAt(0);
                    parrentProgressBar.Value++;
                    return;
                }
                if (downloadQueue.Count == 0)
                {
                    this.appendToLog("Downloading finished");
                    //tell it to extract the zip files
                    //downloadProgress.Text = "done";
                    if (cleanInstallCB.Checked)
                    {
                        this.appendToLog("CleanInstallCB checked, running backgroundDelete(" + tanksLocation + "\\res_mods)");
                        //delete everything in res_mods
                        if (Directory.Exists(tanksLocation + "\\res_mods")) this.backgroundDelete(tanksLocation + "\\res_mods");
                        return;
                    }
                    this.appendToLog("CleanInstallCB not checked, moving to extraction");
                    //this.finishInstall();
                    this.backgroundExtract(false);
                }
            }
        }

        private void extractZipFiles()
        {
            speedLabel.Text = "Extracting...";
            this.appendToLog("Starting Relhax Sound Mod Extraction");
            string[] fileNames = Directory.GetFiles(downloadPath);
            parrentProgressBar.Maximum = fileNames.Count();
            parrentProgressBar.Value = 0;
            //TODO: check with each zip file if the user is actually installing it
            foreach (string fName in fileNames)
            {
                this.appendToLog("Extracting " + fName);
                this.unzip(fName, tanksLocation);
                parrentProgressBar.Value++;
            }
            this.appendToLog("Finished extracting, moving to patching");
            this.patchStuff();
        }

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
                    Application.DoEvents();
                }
            }
            this.appendToLog("Finished Extracting Relhax Modpack User Mod Extraction");
        }

        private void patchFiles()
        {
            speedLabel.Text = "Patching...";
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
                downloadProgress.Text = p.file;
                if (p.type.Equals("regx"))
                {
                    if (p.lines.Count() == 0)
                    {
                        //perform regex patch on entire file
                        this.appendToLog("Regex patch, all lines, " + p.file + ", " + p.search + ", " + p.replace);
                        this.RegxPatch(p.file, p.search, p.replace);
                    }
                    else
                    {
                        foreach (string s in p.lines)
                        {
                            //perform regex patch on specific file lines
                            //will need to be a standard for loop BTW
                            this.appendToLog("Regex patch, line " + s + ", " + p.file + ", " + p.search + ", " + p.replace);
                            this.RegxPatch(p.file, p.search, p.replace,int.Parse(s));
                        }
                    }
                }
                else if (p.type.Equals("xml"))
                {
                    //perform xml patch
                    this.appendToLog("Xml patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                    this.xmlPatch(p.file, p.path, p.mode, p.search, p.replace);
                }
            }
            //all done, delete the patch folder
            if (Directory.Exists(tanksLocation + "\\_patch"))
              Directory.Delete(tanksLocation + "\\_patch",true);
            this.appendToLog("Patching done for Relhax Mod Pack");
        }

        //installs all fonts in the fonts folder, user and custom
        private void installFonts()
        {
            speedLabel.Text = "Installing Fonts...";
            if (!Directory.Exists(tanksLocation + "\\_fonts"))
            {
                speedLabel.Text = "";
                downloadProgress.Text = "Done!";
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                return;
            }
            string[] fonts = Directory.GetFiles(tanksLocation + "\\_fonts");
            if (fonts.Count() == 0)
            {
                //done display
                speedLabel.Text = "";
                downloadProgress.Text = "Done!";
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
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
            if (fontsList.Count == 0)
            {
                //done display
                speedLabel.Text = "";
                downloadProgress.Text = "Done!";
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
                return;
            }
            this.appendToLog("Installing fonts, ask for admin rights");
            DialogResult dr = MessageBox.Show("Do you have admin rights?", "Admin to install fonts?", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                this.extractEmbeddedResource(tanksLocation + "\\_fonts", "RelicModManager", new List<string>() { "FontReg.exe" });
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName ="FontReg.exe";
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
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Value = childProgressBar.Maximum;
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
            string versionSaveLocation = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 4) + "_version.txt";
            if (File.Exists(versionSaveLocation)) File.Delete(versionSaveLocation);
            updater.DownloadFile("https://dl.dropboxusercontent.com/u/44191620/RelicMod/manager version.txt", versionSaveLocation);
            string version = File.ReadAllText(versionSaveLocation);
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
                    updater.DownloadFileAsync(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/" + "RelicModManager.exe"), newExeName);
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
            string version = File.ReadAllText(versionSaveLocation);
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

        //TODO: put this back on a seperate thread
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
                //zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);
                //childProgressBar.Maximum = zip.Entries.Count;
                //childProgressBar.Value = 0;
                zip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
            }
            
        }

        //handler for when progress is made in extracting a zip file
        void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            childProgressBar.Maximum = zip.Entries.Count;
            childProgressBar.Value = e.EntriesExtracted;
            if (e.CurrentEntry != null)
            {
                downloadProgress.Text = e.CurrentEntry.FileName;
                Application.DoEvents();
            }
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll)
            {
                zip.Dispose();
                //downloadProgress.Text = "Complete!";
                //speedLabel.Text = "";
            }
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

        //DEPRECATED: Checks for which parts of the RelHax sound mod it is to download
        private void createDownloadQueue()
        {
            downloadQueue = new List<DownloadItem>();
            //install RelHax
            if (this.features.relhaxBox.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHax.zip"), downloadPath + "\\RelHax.zip"));
                //downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHax.zip"), downloadPath + "\\gui.zip"));
                //downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHax.zip"), downloadPath + "\\6thSense.zip"));
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

        private void MainWindow_Load(object sender, EventArgs e)
        {
            //set window header text to current version so user knows
            this.Text = this.Text + managerVersion;
            //show the wait screen
            pleaseWait wait = new pleaseWait();
            wait.Show();
            wait.loadingDescLabel.Text = "Checking for single instance...";
            Application.DoEvents();
            this.appendToLog("|----------------------------------------------------------|");
            this.appendToLog("|RelHax ModManager " + managerVersion);
            this.appendToLog("|Built on BUILD_DATE, running at " + DateTime.Now);
            this.appendToLog("|Running on " + System.Environment.OSVersion.ToString());
            this.appendToLog("|----------------------------------------------------------|");
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
                MessageBox.Show("Error: Another Instance of the relic mod manager is already running");
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
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxTemp")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxTemp");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxUserConfigs")) Directory.CreateDirectory(Application.StartupPath + "\\RelHaxUserConfigs");
            wait.Close();
            Application.DoEvents();
        }

        //starts the patching process
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
            //patch versiondir out of filePath
            filePath = tanksLocation + "\\res_mods" + filePath;
            filePath = Regex.Replace(filePath, "versiondir", this.getFolderVersion(null));
            //verify the file exists...
            if (!File.Exists(filePath))
                return;
            //load document
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
                    XmlNodeList currentSoundBanksAdd = doc.SelectNodes(xpath);
                    foreach (XmlElement e in currentSoundBanksAdd)
                    {
                        if (Regex.IsMatch(e.InnerText, replace))
                            return;
                    }
                    //get to the node where to add the element
                    XmlNode reff = doc.SelectSingleNode(xpath);
                    //create node(s) to add to the element
                    string[] temp = replace.Split('/');
                    List<XmlElement> nodes = new List<XmlElement>();
                    for (int i = 0; i < temp.Count()-1; i++)
                    {
                        XmlElement ele = doc.CreateElement(temp[i]);
                        if (i == temp.Count() - 2)
                        {
                            //last node with actual data to add
                            ele.InnerText = temp[temp.Count()-1];
                        }
                        nodes.Add(ele);
                    }
                    //add nodes to the element in reverse for hierarchy order
                    for (int i = nodes.Count-1; i > -1; i--)
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
            //patch versiondir out of fileLocation
            fileLocation = tanksLocation + "\\res_mods" + fileLocation;
            fileLocation = Regex.Replace(fileLocation, "versiondir", this.getFolderVersion(null));

            //check that the file exists
            if (!File.Exists(fileLocation))
                return;
            
            //load file from disk...
            string file = File.ReadAllText(fileLocation);
            //parse each line into an index array
            string[] fileParsed = file.Split('\n');
            StringBuilder sb = new StringBuilder();
            //Console.WriteLine(fileParsed.Count());
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
            else
            {
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (Regex.IsMatch(fileParsed[i], search) && i == lineNumber - 1)
                    {
                        fileParsed[i] = Regex.Replace(fileParsed[i], search, replace);
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
            }
            //save the file back into the string and then the file
            file = sb.ToString();
            File.WriteAllText(fileLocation, file);
        }

        //parses a patch xml file into an xml patch instance in memory to be enqueued
        private void createPatchList(string xmlFile)
        {
            if(!File.Exists(xmlFile))
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
            //quick bool hack to say to the downloader to use the "modpack" code
            modPack = true;
            //reset the interface
            this.reset();
            //attempt to locate the tanks directory automatically
            //if it fails, it will prompt the user to return the world of tanks exe
            if (this.autoFindTanks() == null || this.forceManuel.Checked)
            {
                if (this.manuallyFindTanks() == null) return;
            }
            //parse all strings for installation
            if (this.parseStrings() == null)
            {
                this.displayError("The auto-detection failed. Please use the 'force manual' option", null);
                return;
            }
            //the download timers started for download speed measurement
            sw.Reset();
            sw.Start();
            //actual new code
            if (backupModsCheckBox.Checked)
            {
                //backup the mods folder
                if (Directory.Exists(Application.StartupPath + "\\RelHaxModBackup"))
                {
                    this.backgroundDelete(Application.StartupPath + "\\RelHaxModBackup");
                }
                else
                {
                    copyworker_RunWorkerCompleted(null, null);
                }
                return;
            }
            this.parseInstallationPart1();
        }

        //next part of the install process
        private void parseInstallationPart1()
        {
            //reset the childProgresBar value
            childProgressBar.Maximum = 100;
            childProgressBar.Value = 0;
            //show the mod selection window
            ModSelectionList list = new ModSelectionList();
            list.ShowDialog();
            if (list.cancel) return;
            modsToInstall = new List<Mod>();
            configsToInstall = new List<Config>();
            patchList = new List<Patch>();
            userMods = new List<Mod>();
            dependencies = new List<Dependency>();
            parsedCatagoryLists = list.parsedCatagoryList;
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
                        modsToInstall.Add(m);
                        //at least one mod of this catagory is checked, add any dependencies required
                        if (!dependenciesAdded)
                        {
                            //add dependencies
                            foreach (Dependency d in c.dependencies)
                            {
                                if (d.enabled)
                                    dependencies.Add(d);
                            }
                            dependenciesAdded = true;
                        }
                        foreach (Config cc in m.configs)
                        {
                            if (cc.enabled && cc.configChecked)
                            {
                                //same for configs
                                configsToInstall.Add(cc);
                            }
                        }
                    }
                }
            }
            //DEPRECATED: do not use!!
            /*
            //remove the mods and configs that don't have zips to download
            //this will keep those configs that have the entered value as the zip url
            for (int i = 0; i < modsToInstall.Count; i++)
            {
                if (modsToInstall[i].modZipFile == null || modsToInstall[i].modZipFile.Equals(""))
                {
                    modsToInstall.RemoveAt(i);
                }
            }
            for (int i = 0; i < configsToInstall.Count; i++)
            {
                if (configsToInstall[i].zipConfigFile == null || configsToInstall[i].zipConfigFile.Equals(""))
                {
                    configsToInstall.RemoveAt(i);
                }
            }*/
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
            if (modsToInstall.Count == 0)
            {
                //check for userMods
                if (userMods.Count > 0)
                {
                    //skip to the extraction process
                    this.downloader_DownloadFileCompleted(null, null);
                }
                //pull out because there are no mods to install
                return;
            }
            //foreach mod and config, if the crc's don't match, add it to the downloadQueue
            string localFilesDir = Application.StartupPath + "\\RelHaxDownloads\\";
            foreach (Dependency d in dependencies)
            {
                if (!this.CRCsMatch(localFilesDir + d.dependencyZipFile, d.dependencyZipCRC))
                {
                    downloadQueue.Add(new DownloadItem(new Uri(this.downloadURL + d.dependencyZipFile),localFilesDir + d.dependencyZipFile));
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
            /*
            //DEPRECATED: remove configs tha are value_enter patches
            for (int i = 0; i < configsToInstall.Count; i++)
            {
                if (parseable(configsToInstall[i].zipConfigFile))
                {
                    //magicly make it a patch somehow
                    //and remove it from the list
                    configsToInstall.RemoveAt(i);
                }
            }*/
            parrentProgressBar.Maximum = downloadQueue.Count;
            //at this point, there may be user mods selected,
            //and there is at least one mod to extract
            //check for any mods to be install tha also need to be downloaded
            if (downloadQueue.Count > 0)
            {
                if (File.Exists(downloadQueue[0].zipFile)) File.Delete(downloadQueue[0].zipFile);
                //download new zip file
                downloader = new WebClient();
                downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                downloader.DownloadFileAsync(downloadQueue[0].URL, downloadQueue[0].zipFile);
                tempOldDownload = Path.GetFileName(downloadQueue[0].zipFile);
                downloadQueue.RemoveAt(0);
                parrentProgressBar.Value++;
            }
            else
            {
                //there are no mods to download, go right to the extraction process
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
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(inputFile));
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

        //DEPRECATED: Determins if the zip file from the config/mod is actually
        //a number from a text box value_enter for patching the patch file
        private bool parseable(string configzip)
        {
            try
            {
                int i = int.Parse(configzip);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        //TODO: move uninstallation process off of the UI thread
        //Main method to uninstall the modpack
        private void uninstallRelhaxMod_Click(object sender, EventArgs e)
        {
            modPack = true;
            //reset the interface
            this.reset();
            //attempt to locate the tanks directory
            if (this.autoFindTanks() == null || this.forceManuel.Checked)
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
            if (MessageBox.Show("This will delete ALL MODS. Are you Sure?", "Um...", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                downloadProgress.Text = "Uninstalling...";
                RelHaxUninstall = true;
                this.backgroundDelete(tanksLocation + "\\res_mods");
            }
        }

        //handler for what happends when the check box "clean install" is checked or not
        private void cleanInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            if (cleanInstallCB.Checked)
            {
                MessageBox.Show("Enabling this will delete all mods in your res_mods folder");
            }
        }

        //method to bring up the crc checker to get the crc values of a mod
        private void CIEplainLabel_Click(object sender, EventArgs e)
        {
            crcCheck crcCHecker = new crcCheck();
            crcCHecker.Show();
        }

        //enalbes the user to use "comic sans" font for the 1 person that would ever want to do that
        private void cancerFontCB_CheckedChanged(object sender, EventArgs e)
        {
            if (cancerFontCB.Checked)
            {
                this.Font = new System.Drawing.Font("Comic Sans MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
            else
            {
                this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
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
            BackgroundWorker copyworker = new BackgroundWorker();
            copyworker.WorkerReportsProgress = true;
            copyworker.DoWork += new DoWorkEventHandler(copyworker_DoWork);
            copyworker.ProgressChanged += new ProgressChangedEventHandler(copyworker_ProgressChanged);
            copyworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(copyworker_RunWorkerCompleted);
            object sourceFolder = source;
            object destFolder = dest;
            object[] parameters = new object [] {sourceFolder,destFolder};
            copyworker.RunWorkerAsync(parameters);
        }
        
        //uses backgroundWorker to delete folder and everything inside
        //rather destructive if i do say so myself
        private void backgroundDelete(string folder)
        {
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            downloadProgress.Text = "Copying file " + numFilesToCopyDeleteExtract + " of " + numFilesToProcessInt;
            BackgroundWorker deleteworker = new BackgroundWorker();
            deleteworker.WorkerReportsProgress = true;
            deleteworker.DoWork += new DoWorkEventHandler(deleteworker_DoWork);
            deleteworker.ProgressChanged += new ProgressChangedEventHandler(deleteworker_ProgressChanged);
            deleteworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(deleteworker_RunWorkerCompleted);
            object folderToDelete = folder;
            object[] parameters = new object [] {folderToDelete};
            deleteworker.RunWorkerAsync(parameters);
        }
        
        //uses backgroundWorker to extract fileStream
        private void backgroundExtract(bool user)
        {
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            downloadProgress.Text = "Loading Extraction Text...";
            BackgroundWorker extractworker = new BackgroundWorker();
            extractworker.WorkerReportsProgress = true;
            extractworker.DoWork += new DoWorkEventHandler(extractworker_DoWork);
            extractworker.ProgressChanged += new ProgressChangedEventHandler(extractworker_ProgressChanged);
            extractworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(extractworker_RunWorkerCompleted);
            zip = new ZipFile();
            zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);
            object[] parameters = new object [] {};
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
            BackgroundWorker copyworker = (BackgroundWorker)sender;
            object[] parameters = e.Argument as object[];
            string sourceFolder = (string)parameters[0];
            string destFolder = (string)parameters[1];
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            this.numFilesToProcess(sourceFolder);
            this.DirectoryCopy(sourceFolder,destFolder,true,copyworker);
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
            if (backupModsCheckBox.Checked)
            {
                this.parseInstallationPart1();
            }
        }
        
        //handler for the deleteworker when it is called
        private void deleteworker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker deleteworker = (BackgroundWorker)sender;
            object[] parameters = e.Argument as object[];
            string folderToDelete = (string)parameters[0];
            numFilesToProcessInt = 0;
            numFilesToCopyDeleteExtract = 0;
            this.numFilesToProcess(folderToDelete);
            this.DirectoryDelete(folderToDelete,true,deleteworker);
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
            if (backupModsCheckBox.Checked && !RelHaxUninstall)
            {
                this.backgroundCopy(tanksLocation + "\\res_mods", Application.StartupPath + "\\RelHaxModBackup");
                return;
            }
            if (cleanInstallCB.Checked && !RelHaxUninstall)
            {
                //this.finishInstall();
                this.backgroundExtract(false);
                return;
            }
            if (RelHaxUninstall)
            {
                if (!Directory.Exists(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null));
                downloadProgress.Text = "Done!";
                RelHaxUninstall = false;
                return;
            }
        }
        
        //handler for the deleteworker when it is called
        private void extractworker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker extractworker = (BackgroundWorker)sender;
            //object[] parameters = e.Argument as object[];
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
        
        //handler for the deleteworker when progress is made
        private void extractworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            parrentProgressBar.Value++;
        }
        
        //handler for when the deleteworker is completed
        private void extractworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!userExtract)
            {
                this.patchFiles();
                this.backgroundExtract(true);
            }
            else
            {
                this.patchFiles();
                this.installFonts();
            }
        }
        
        //recursivly copies every file from one place to another
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, BackgroundWorker copyworker)
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
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, copyworker);
                }
            }
        }

        //recursivly deletes every file from one place to another
        private void DirectoryDelete(string sourceDirName, bool deleteSubDirs, BackgroundWorker deleteworker)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(sourceDirName, file.Name);
                file.Delete();
                deleteworker.ReportProgress(numFilesToCopyDeleteExtract++);
            }
            // If copying subdirectories, copy them and their contents to new location.
            if (deleteSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(sourceDirName, subdir.Name);
                    DirectoryDelete(subdir.FullName, deleteSubDirs, deleteworker);
                    subdir.Delete();
                    deleteworker.ReportProgress(numFilesToCopyDeleteExtract++);
                }
            }
        }

        private void forceManuel_CheckedChanged(object sender, EventArgs e)
        {
            if (this.forceManuel.Checked)
            {
                MessageBox.Show("Enable this if you are having problems with auto-detection");
            }
        }

        private void backupModsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.backupModsCheckBox.Checked)
            {
                MessageBox.Show("This will make a backup of your current res-mods folder that you can restore later in case of an error. Tha backup is located in the application directory, under 'RelHaxModBackup'");
            }
        }

        private void finishInstall()
        {
            //just a double-check to delete all patches
            if (Directory.Exists(tanksLocation + "\\_patch")) Directory.Delete(tanksLocation + "\\_patch", true);
            if (Directory.Exists(tanksLocation + "\\_fonts")) Directory.Delete(tanksLocation + "\\_fonts", true);
            if (!Directory.Exists(tanksLocation + "\\res_mods")) Directory.CreateDirectory(tanksLocation + "\\res_mods");
            this.extractZipFilesModPack();
            this.patchFiles();
            this.extractZipFilesUser();
            this.patchFiles();
            this.installFonts();
        }

        //extracts embeded rescource onto disk
        private void extractEmbeddedResource(string outputDir, string resourceLocation, List<string> files)
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

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.appendToLog("Application Closing");
            this.appendToLog("|----------------------------------------------------------|");
        }

        private void largerFontButton_CheckedChanged(object sender, EventArgs e)
        {
            if (largerFontButton.Checked)
            {
                this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
            else
            {
                this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
        }
    }

    //a class for the downloadQueue list, to make a queue of downloads
    class DownloadItem
    {
        public Uri URL { get; set; }
        public string zipFile {get; set;}
        //create a DownloadItem with the 2 properties set
        public DownloadItem(Uri newURL, String newZipFile)
        {
            URL = newURL;
            zipFile = newZipFile;
        }
    }

}
