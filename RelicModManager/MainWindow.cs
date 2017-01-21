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
        //all instance variables required to be up here
        private FolderBrowserDialog selectWotFolder = new FolderBrowserDialog();
        private WebClient downloader = new WebClient();
        private string modAudioFolder;
        private string tempPath = Path.GetTempPath();
        private static int MBDivisor = 1048576;
        private string managerVersion = "version 17.1";
        private string tanksLocation;
        private SelectFeatures features = new SelectFeatures();
        private List<DownloadItem> downloadQueue;
        private int downloadCount;
        private volatile int totalDownloadCount;
        private bool isAutoDetected;
        string appPath = Application.StartupPath;
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
        private bool isInstalling = false;
        
        //The constructur for the application
        public MainWindow()
        {
            InitializeComponent();
        }
        
        //install RelHax
        private void installRelhax_Click(object sender, EventArgs e)
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
        
        //uninstall RelHax
        private void uninstallRelhax_Click(object sender, EventArgs e)
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

        //handler for the mod download file progress
        void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            int MBytesIn = (int)bytesIn / MBDivisor;
            int MBytesTotal = (int)totalBytes / MBDivisor;
            downloadProgress.Text = "Downloaded " + MBytesIn + " MB" + " of " + MBytesTotal + " MB";
            downloadProgressBar.Value = e.ProgressPercentage;
            if(MBytesIn == 0 && MBytesTotal == 0)
            {
                this.downloadProgress.Text = "Complete!";
            }
        }

        //handler for the mod download file complete event
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
        }

        //method to check for updates to the application on startup
        private void checkmanagerUpdates()
        {
            WebClient updater = new WebClient();
            string versionSaveLocation = Application.ExecutablePath.Substring(0,Application.ExecutablePath.Length-4) + "_version.txt";
            if (File.Exists(versionSaveLocation)) File.Delete(versionSaveLocation);
            updater.DownloadFile("https://dl.dropboxusercontent.com/u/44191620/RelicMod/manager version.txt", versionSaveLocation);
            string version = File.ReadAllText(versionSaveLocation);
            if (!version.Equals(managerVersion))
            {
                //out of date
                DialogResult result = MessageBox.Show("Your manager is out of date. Please Download the New Version", "Manager is out of date", MessageBoxButtons.YesNo);
                if (result.Equals(DialogResult.Yes))
                {
                    //download new version
                    string newExeName = Application.StartupPath + "\\RelicModManager " + managerVersion + ".exe";
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
            string newExeName = Application.StartupPath + "\\RelicModManager " + managerVersion + ".exe";
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
            downloadProgressBar.Value = e.ProgressPercentage;
            if (MBytesIn == 0 && MBytesTotal == 0)
            {
                this.downloadProgress.Text = "Complete!";
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

        //Method for displaying an error message
        private void displayError(String errorText, String errorHandle)
        {
            if(errorHandle == null) MessageBox.Show(errorText);
            else MessageBox.Show(errorText, errorHandle);
            downloadProgress.Text = "Aborted";
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
                        catch (IOException)
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
            if (Directory.Exists(tempPath + "\\versionCheck")) Directory.Delete(tempPath + "\\versionCheck", true);
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

        private string autoFindTanks()
        {
            object theObject = new object();
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
            this.Text = this.Text + managerVersion;
            wait.Show();
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
            this.cleanup();
            this.checkmanagerUpdates();
            wait.Close();
            Application.DoEvents();
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
                this.installRelhax_Click(null, null);
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
