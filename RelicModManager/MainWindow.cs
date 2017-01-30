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
        private string managerVersion = "version 17.3";
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
        bool modPack;
        string tempOldDownload;

        //The constructur for the application
        public MainWindow()
        {
            InitializeComponent();
        }

        //install RelHax
        private void installRelhax_Click(object sender, EventArgs e)
        {
            modPack = false;
            //reset the interface
            this.reset();
            //ask the user which features s/he wishes to install
            this.features.ShowDialog();
            if (features.canceling)
            {
                downloadProgress.Text = "Canceled";
                return;
            }
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

            if (!Directory.Exists(downloadPath)) Directory.CreateDirectory(downloadPath);

            sw.Reset();
            sw.Start();

            this.createDownloadQueue();
            this.downloader_DownloadFileCompleted(null, null);
        }

        //uninstall RelHax
        private void uninstallRelhax_Click(object sender, EventArgs e)
        {
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
                    downloadProgress.Text = "done";
                    this.extractZipFiles();
                }
            }
            else
            {
                if (e != null && e.Error != null && e.Error.Message.Equals("The remote server returned an error: (404) Not Found."))
                {
                    //404
                    MessageBox.Show("Failed to download " + tempOldDownload + ". If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it crashes");
                    Application.Exit();
                }
                //new relhax modpack code
                if (downloadQueue.Count != 0)
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
                    return;
                }
                if (downloadQueue.Count == 0)
                {
                    //tell it to extract the zip files
                    downloadProgress.Text = "done";
                    if (cleanInstallCB.Checked)
                    {
                        //delete everything in res_mods
                        if (Directory.Exists(tanksLocation + "\\res_mods")) Directory.Delete(tanksLocation + "\\res_mods", true);
                        if (!Directory.Exists(tanksLocation + "\\res_mods")) Directory.CreateDirectory(tanksLocation + "\\res_mods");
                    }
                    this.extractZipFilesModPack();
                    this.patchFiles();
                    //this.extractZipFilesCustom();
                    //this.patchFilesCustom();
                }
            }
        }

        private void extractZipFiles()
        {
            speedLabel.Text = "Extracting...";
            string[] fileNames = Directory.GetFiles(downloadPath);
            parrentProgressBar.Maximum = fileNames.Count();
            parrentProgressBar.Value = 0;
            //TODO: check with each zip file if the user is actually installing it
            foreach (string fName in fileNames)
            {
                this.unzip(fName, tanksLocation);
                parrentProgressBar.Value++;
            }
            this.patchStuff();
            this.installFonts();
        }

        private void extractZipFilesModPack()
        {
            speedLabel.Text = "Extracting...";
            parrentProgressBar.Maximum = modsToInstall.Count + configsToInstall.Count;
            parrentProgressBar.Value = 0;
            string downloadedFilesDir = Application.StartupPath + "\\RelHaxDownloads\\";
            //extract mods
            foreach (Mod m in modsToInstall)
            {
                this.unzip(downloadedFilesDir + m.modZipFile, tanksLocation);
                parrentProgressBar.Value++;
            }
            //extract configs
            foreach (Config c in configsToInstall)
            {
                this.unzip(downloadedFilesDir + c.zipConfigFile, tanksLocation);
                parrentProgressBar.Value++;
            }
            //extract user mods
            //for now assume all of them in there are to be used

        }

        private void patchFiles()
        {
            string[] patchFiles = Directory.GetFiles(tanksLocation + "\\_patch");

            patchList.Clear();//this has all the patches in memory
            for (int i = 0; i < patchFiles.Count(); i++)
            {
                //add patches to patchList
                
                this.createPatchList(patchFiles[i]);
            }
            //this would be the actual patch method
            foreach (Patch p in patchList)
            {
                if (p.type.Equals("regx"))
                {
                    if (p.lines.Count() == 0)
                    {
                        //perform regex patch on entire file
                        this.RegxPatch(p.file, p.search, p.replace);
                    }
                    else
                    {
                        foreach (string s in p.lines)
                        {
                            //perform regex patch on specific file lines
                            //will need to be a standard for loop BTW
                            this.RegxPatch(p.file, p.search, p.replace,int.Parse(s));
                        }
                    }
                }
                else if (p.type.Equals("xml"))
                {
                    //perform xml patch
                    this.xmlPatch(p.file, p.path, p.mode, p.search, p.replace);
                }
            }
            //delete patch directory for user patches later

        }

        private void installFonts()
        {

        }

        //method to check for updates to the application on startup
        private void checkmanagerUpdates()
        {
            WebClient updater = new WebClient();
            string versionSaveLocation = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 4) + "_version.txt";
            if (File.Exists(versionSaveLocation)) File.Delete(versionSaveLocation);
            updater.DownloadFile("https://dl.dropboxusercontent.com/u/44191620/RelicMod/manager version.txt", versionSaveLocation);
            string version = File.ReadAllText(versionSaveLocation);
            if (!version.Equals(managerVersion))
            {
                //out of date
                VersionInfo vi = new VersionInfo();
                vi.ShowDialog();
                DialogResult result = vi.result;
                if (result.Equals(DialogResult.Yes))
                {
                    //download new version
                    sw.Reset();
                    sw.Start();
                    string newExeName = Application.StartupPath + "\\RelicModManager " + version + ".exe";
                    updater.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updater_DownloadProgressChanged);
                    updater.DownloadFileCompleted += new AsyncCompletedEventHandler(updater_DownloadFileCompleted);
                    if (File.Exists(newExeName)) File.Delete(newExeName);
                    updater.DownloadFileAsync(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/" + "RelicModManager.exe"), newExeName);
                }
                else
                {
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
            catch (Win32Exception exeption)
            {
                string temp = exeption.Message;
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

        private String parseStrings()
        {
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
            parsedModsFolder = tanksLocation + "\\res_mods\\" + this.getFolderVersion(tanksLocation);
            modGuiFolder = parsedModsFolder + "\\gui\\soundModes";
            modAudioFolder = parsedModsFolder + "\\audioww";
            modGuiFolderBase = parsedModsFolder + "\\gui";
            customUserMods = Application.StartupPath + "\\RelHaxUserMods";
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

        //Method for displaying an error message
        private void displayError(String errorText, String errorHandle)
        {
            if (errorHandle == null) MessageBox.Show(errorText);
            else MessageBox.Show(errorText, errorHandle);
            downloadProgress.Text = "Aborted";
        }

        private void unzip(string zipFile, string extractFolder)
        {
            zip = ZipFile.Read(zipFile);
            for (int i = 0; i < zip.Entries.Count; i++)
            {
                if (Regex.IsMatch(zip[i].FileName, "versiondir"))
                {
                    zip[i].FileName = Regex.Replace(zip[i].FileName, "versiondir", this.getFolderVersion(null));
                }
            }

            zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(zip_ExtractProgress);
            childProgressBar.Maximum = zip.Entries.Count;
            childProgressBar.Value = 0;
            zip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
        }

        void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            childProgressBar.Value = e.EntriesExtracted;
            if (e.CurrentEntry != null)
            {
                downloadProgress.Text = e.CurrentEntry.FileName;
                Application.DoEvents();
            }
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll)
            {
                zip.Dispose();
                downloadProgress.Text = "Complete!";
                speedLabel.Text = "";
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

        private void createDownloadQueue()
        {
            downloadQueue = new List<DownloadItem>();
            if (this.features.relhaxBox.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHax.zip"), downloadPath + "\\RelHax.zip"));
                //downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHax.zip"), downloadPath + "\\gui.zip"));
                //downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHax.zip"), downloadPath + "\\6thSense.zip"));
            }
            if (this.features.relhaxBoxCen.Checked)
            {
                downloadQueue.Add(new DownloadItem(new Uri("https://dl.dropboxusercontent.com/u/44191620/RelicMod/RelHaxCen.zip"), downloadPath + "\\RelHax.zip"));
            }
            parrentProgressBar.Maximum = downloadQueue.Count;
            parrentProgressBar.Minimum = 0;
        }

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
            this.Text = this.Text + managerVersion;
            pleaseWait wait = new pleaseWait();
            wait.Show();
            wait.loadingDescLabel.Text = "Checking for single instance...";
            Application.DoEvents();
            try
            {
                File.WriteAllText(tempPath + "\\RelHaxOneInstance.txt", "this file is open and cannot be deleted");
                File.OpenWrite(tempPath + "\\RelHaxOneInstance.txt");
            }
            catch (IOException)
            {
                wait.Close();
                MessageBox.Show("Error: Another Instance of the relic mod manager is already running");
                this.Close();
            }
            wait.loadingDescLabel.Text = "Doing Random Cleanup...";
            Application.DoEvents();
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

        private void formPageLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://relicgaming.com/index.php?topic=697.0");
        }

        private void xmlPatch(string filePath, string xpath, string mode, string search, string replace)
        {
            //patch versiondir out of filePath
            filePath = tanksLocation + "\\res_mods" + filePath;
            filePath = Regex.Replace(filePath, "versiondir", this.getFolderVersion(null));

            //load document
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            switch (mode)
            {
                case "add":
                    //check to see if it's already there
                    XmlNodeList currentSoundBanksAdd = doc.SelectNodes(xpath);
                    foreach (XmlElement e in currentSoundBanksAdd)
                    {
                        if (e.InnerText.Equals(replace))
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
                    //add nodes to the element in hierarchy order
                    for (int i = nodes.Count-1; i > -1; i--)
                    {
                        if (i == 0)
                        {
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
                        if (e.InnerText.Equals(replace))
                            return;
                    }
                    //find and replace
                    XmlNodeList rel1Edit = doc.SelectNodes(xpath);
                    foreach (XmlElement eee in rel1Edit)
                    {
                        if (eee.InnerText.Equals(search))
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
                        if (e.InnerText.Equals(search))
                        {
                            e.RemoveAll();
                        }
                    }
                    //save it
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc.Save(filePath);
                    //check to see if it has the header info at the top to see if we need to remove it later
                    bool hasHeader = false;
                    XmlDocument doc3 = new XmlDocument();
                    doc3.Load(filePath);
                    foreach (XmlNode node in doc3)
                    {
                        if (node.NodeType == XmlNodeType.XmlDeclaration)
                        {
                            hasHeader = true;
                            //doc3.RemoveChild(node);
                        }
                    }
                    doc3.Save(filePath);
                    //remove empty elements
                    XDocument doc2 = XDocument.Load(filePath);
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc2.Save(filePath);
                    if (!hasHeader)
                    {
                        //need to remove the header
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
                    break;
            }
        }

        private void RegxPatch(string fileLocation, string search, string replace, int lineNumber = 0)
        {
            //load file from disk...
            string file = File.ReadAllText(fileLocation);
            string[] fileParsed = file.Split('\n');
            StringBuilder sb = new StringBuilder();
            //Console.WriteLine(fileParsed.Count());
            if (lineNumber == 0)
            //search entire file
            {
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (Regex.IsMatch(fileParsed[i], search))
                    {
                        //Console.WriteLine(fileParsed[i]);
                        fileParsed[i] = Regex.Replace(fileParsed[i], search, replace);
                        //Console.WriteLine(fileParsed[i]);
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
            file = sb.ToString();
            File.WriteAllText(Application.StartupPath + "\\mod_battle_assistant_patched.txt", file);
        }

        private void createPatchList(string xmlFile)
        {
            //this would be the createPatchList method
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFile);
            XmlNodeList patchesList = doc.SelectNodes("//patchs/patch");
            foreach (XmlNode n in patchesList)
            {
                Patch p = new Patch();
                foreach (XmlNode nn in n.ChildNodes)
                {
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


        private void button1_Click(object sender, EventArgs e)
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
            //the download timers started
            sw.Reset();
            sw.Start();
            //actual new code
            ModSelectionList list = new ModSelectionList();
            list.ShowDialog();
            if (list.cancel) return;
            parsedCatagoryLists = list.parsedCatagoryList;
            modsToInstall = new List<Mod>();
            configsToInstall = new List<Config>();
            patchList = new List<Patch>();
            //if mod is enabled and checked, add it to list of mods to extract/install
            //same for configs
            foreach (Catagory c in parsedCatagoryLists)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.enabled && m.modChecked)
                    {
                        modsToInstall.Add(m);
                        foreach (Config cc in m.configs)
                        {
                            if (cc.enabled && cc.configChecked)
                            {
                                configsToInstall.Add(cc);
                            }
                        }
                    }
                }
            }
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
            if (modsToInstall.Count == 0)
            {
                return;
            }
            //foreach mod and config, if the crc's don't match, download it
            downloadQueue = new List<DownloadItem>();
            string localFilesDir = Application.StartupPath + "\\RelHaxDownloads\\";
            foreach (Mod m in modsToInstall)
            {
                if (!this.CRCsMatch(localFilesDir + m.modZipFile,m.crc))
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
            //remove configs tha are value_enter patches
            for (int i = 0; i < configsToInstall.Count; i++)
            {
                if (parseable(configsToInstall[i].zipConfigFile))
                {
                    //magicly make it a patch somehow
                    //and remove it from the list
                    configsToInstall.RemoveAt(i);
                }
            }
            parrentProgressBar.Maximum = downloadQueue.Count;
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
                downloader_DownloadFileCompleted(null, null);
            }
            return;
        }

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

        private void button2_Click(object sender, EventArgs e)
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
            if (MessageBox.Show("This will delete ALL MODS. Are you Sure?", "Um...", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                downloadProgress.Text = "Uninstalling...";
                Application.DoEvents();
                Directory.Delete(tanksLocation + "\\res_mods", true);
                if (!Directory.Exists(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null))) Directory.CreateDirectory(tanksLocation + "\\res_mods\\" + this.getFolderVersion(null));
                downloadProgress.Text = "Done!";
                Application.DoEvents();
            }
        }

        private void cleanInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            if (cleanInstallCB.Checked)
            {
                MessageBox.Show("Enabling this will delete all mods in your res_mods folder");
            }
        }

        private void CIEplainLabel_Click(object sender, EventArgs e)
        {
            crcCheck crcCHecker = new crcCheck();
            crcCHecker.Show();
        }

    }

    class DownloadItem
    {
        public Uri URL { get; set; }
        public string zipFile;
        public DownloadItem(Uri newURL, String newZipFile)
        {
            URL = newURL;
            zipFile = newZipFile;
        }
    }

}
