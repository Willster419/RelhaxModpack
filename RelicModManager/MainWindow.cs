using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using System.Drawing;
using System.Globalization;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Text;

namespace RelhaxModpack
{
    public partial class MainWindow : Form
    {
        //all instance variables required to be up here
        private FolderBrowserDialog selectWotFolder = new FolderBrowserDialog();
        private WebClient downloader;
        private const int MBDivisor = 1048576;
        //sample:  c:/games/World_of_Tanks
        private string tanksLocation;
        //the location to pass into the installer
        private string tanksVersionForInstaller;
        //the folder where the user's app data is stored (C:\Users\username\AppData)
        private string appDataFolder;
        //the string representation from the xml document manager_version.xml. also passed into the installer for logging the version of the database installed at that time
        private string databaseVersionString;
        //queue for downloading mods
        private List<DownloadItem> downloadQueue;
        //where all the downloaded mods are placed
        public static string downloadPath = Path.Combine(Application.StartupPath, "RelHaxDownloads");
        public static string md5HashDatabaseXmlFile = Path.Combine(downloadPath, "MD5HashDatabase.xml");
        public static string onlineDatabaseXmlFile = Path.Combine(downloadPath, "onlineDatabase.xml");
        //timer to measure download speed
        Stopwatch sw = new Stopwatch();
        //The list of all mods
        private List<Category> parsedCatagoryLists;
        //The ordered lists to install
        private List<Dependency> globalDependenciesToInstall;
        private List<Dependency> dependenciesToInstall;
        private List<LogicalDependnecy> logicalDependenciesToInstall;
        private List<DatabaseObject> modsConfigsToInstall;
        private List<Dependency> appendedDependenciesToInstall;
        //list of all current dependencies
        private List<Dependency> currentDependencies;
        private List<LogicalDependnecy> currentLogicalDependencies;
        //DeveloperSelections namelist
        public static List<DeveloperSelections> developerSelections = new List<DeveloperSelections>();
        //list of patches
        private List<Patch> patchList;
        //list of all needed files from the current loaded modInfo file
        public static List<string> usedFilesList;
        //counter for Utils.exception calls
        public static int errorCounter = 0;
        string tempOldDownload;
        private List<Mod> userMods;
        private FirstLoadHelper helper;
        string helperText;
        string currentModDownloading;
        private Installer ins;
        private Installer unI;
        private string tanksVersion;//0.9.x.y
        List<double> timeRemainArray;
        //the ETA variable for downlading
        double actualTimeRemain = 0;
        float previousTotalBytesDownloaded = 0;
        float currentTotalBytesDownloaded = 0;
        float differenceTotalBytesDownloaded = 0;
        float sessionDownloadSpeed = 0;
        private LoadingGifPreview gp;
        List<string> supportedVersions = new List<string>();
        List<DatabaseObject> modsConfigsWithData;
        private float scale = 1.0f;

        //  interpret the created CiInfo buildTag as an "us-US" or a "de-DE" timeformat and return it as a local time- and dateformat string
        public static string compileTime()//if getting build error, check windows date and time format settings https://puu.sh/xgCqO/e97e2e4a34.png
        {
            string date = CiInfo.BuildTag;
            if (Utils.convertDateToLocalCultureFormat(date, out date))
                return date;
            else
                return "Error in dateTime format: " + date;
        }

        /// <summary>
        /// gets now the "Release version" from RelhaxModpack-properties
        /// https://stackoverflow.com/questions/2959330/remove-characters-before-character
        /// https://www.mikrocontroller.net/topic/140764
        /// </summary>
        /// <returns></returns>
        public string managerVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().IndexOf('.') + 1);
        }

        //The constructur for the application
        public MainWindow()
        {
            Utils.appendToLog("MainWindow Constructed");
            InitializeComponent();
            this.SetStyle(                                      /// add double buffering and possibly reduce flicker https://stackoverflow.com/questions/1550293/stopping-textbox-flicker-during-update
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer, true);
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
            //set the progress bar
            childProgressBar.Value = e.ProgressPercentage;
            //set the download speed
            if (sessionDownloadSpeed < 0)
                sessionDownloadSpeed = 0;
            sessionDownloadSpeed = (float)Math.Round(sessionDownloadSpeed, 2);
            totalSpeedLabel = "" + sessionDownloadSpeed + " MB/s";
            //get the ETA for the download
            double totalTimeToDownload = MBytesTotal / (e.BytesReceived / MBDivisor / sw.Elapsed.TotalSeconds);
            double timeRemain = totalTimeToDownload - sw.Elapsed.TotalSeconds;
            if (timeRemainArray == null)
                timeRemainArray = new List<double>();
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
            string downloadStatus = string.Format("{0} {1} ({2} MB {3} {4} MB)\n{5} {6} mins {7} secs",
                Translations.getTranslatedString("Downloading"), currentModDownloadingShort, Math.Round(MBytesIn, 1), Translations.getTranslatedString("of"), Math.Round(MBytesTotal, 1), totalSpeedLabel, actualTimeMins, actualTimeSecs);
            downloadProgress.Text = downloadStatus;
        }
        //handler for the mod download file complete event
        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //check to see if the user cancled the download
            if (e != null && e.Cancelled)
            {
                //update the UI and download state
                toggleUIButtons(true);
                downloadProgress.Text = Translations.getTranslatedString("idle");
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
                return;
            }
            downloadTimer.Enabled = false;
            if (e != null && e.Error != null && e.Error.Message.Equals("The remote server returned an error: (404) Not Found."))
            {
                //404
                Utils.appendToLog(string.Format("ERROR: {0} failed to download", tempOldDownload));
                MessageBox.Show(string.Format("{0}\n{1}\n\n{2}", Translations.getTranslatedString("failedToDownload_1"), tempOldDownload, Translations.getTranslatedString("failedToDownload_2")));
                Application.Exit();
            }
            if (downloadQueue.Count != 0)
            {
                totalProgressBar.Maximum = (int)InstallerEventArgs.InstallProgress.Done;
                totalProgressBar.Value = 1;
                cancelDownloadButton.Enabled = true;
                cancelDownloadButton.Visible = true;
                //for the next file in the queue, delete it.
                if (File.Exists(downloadQueue[0].zipFile)) File.Delete(downloadQueue[0].zipFile);
                //download new zip file
                if (downloader != null)
                    downloader.Dispose();
                downloader = new WebClient();
                downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                downloader.Proxy = null;
                if (timeRemainArray == null)
                    timeRemainArray = new List<double>();
                timeRemainArray.Clear();
                actualTimeRemain = 0;
                sw.Reset();
                sw.Start();
                downloader.DownloadFileAsync(downloadQueue[0].URL, downloadQueue[0].zipFile);
                tempOldDownload = Path.GetFileName(downloadQueue[0].zipFile);
                Utils.appendToLog("downloading " + tempOldDownload);
                currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile);
                if (currentModDownloading.Length >= 200)
                {
                    currentModDownloading = Path.GetFileNameWithoutExtension(downloadQueue[0].zipFile).Substring(0, 23) + "...";
                }
                downloadQueue.RemoveAt(0);
                if ((parrentProgressBar.Value + 1) <= parrentProgressBar.Maximum)
                    parrentProgressBar.Value++;
                return;
            }
            if (downloadQueue.Count == 0)
            {
                cancelDownloadButton.Enabled = false;
                cancelDownloadButton.Visible = false;
                ins = new Installer()
                {
                    AppPath = Application.StartupPath,
                    GlobalDependencies = this.globalDependenciesToInstall,
                    Dependencies = this.dependenciesToInstall,
                    LogicalDependencies = this.logicalDependenciesToInstall,
                    AppendedDependencies = this.appendedDependenciesToInstall,
                    ModsConfigsToInstall = this.modsConfigsToInstall,
                    ModsConfigsWithData = this.modsConfigsWithData,
                    TanksLocation = this.tanksLocation,
                    TanksVersion = this.tanksVersionForInstaller,
                    UserMods = this.userMods,
                    AppDataFolder = this.appDataFolder,
                    DatabaseVersion = this.databaseVersionString
                };
                ins.InstallProgressChanged += I_InstallProgressChanged;
                ins.StartInstallation();
            }
        }

        //method to check for updates to the application on startup
        private void checkmanagerUpdates()
        {
            Utils.appendToLog("Starting check for application updates");
            //download the updates
            WebClient updater = new WebClient();
            updater.Proxy = null;
            if (File.Exists(Settings.managerInfoDatFile))
                File.Delete(Settings.managerInfoDatFile);
            try
            {
                updater.DownloadFile("http://wotmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat", Settings.managerInfoDatFile);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("checkmanagerUpdates", @"Tried to access http://wotmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat", ex);
                MessageBox.Show(string.Format("{0} managerInfo.dat", Translations.getTranslatedString("failedToDownload_1")));
                Application.Exit();
            }

            string version = "";
            string xmlString = Utils.getStringFromZip(Settings.managerInfoDatFile, "manager_version.xml");  //xml doc name can change
            if (!xmlString.Equals(""))
            {
                XDocument doc = XDocument.Parse(xmlString);
                var databaseVersion = doc.Descendants().Where(n => n.Name == "manager").FirstOrDefault();
                if (databaseVersion != null)
                    version = databaseVersion.Value;
                Utils.appendToLog(string.Format("Local application is {0}, current online is {1}", managerVersion(), version));

                if (!version.Equals(managerVersion()))
                {
                    Utils.appendToLog("exe is out of date. displaying user update window");
                    //out of date
                    VersionInfo vi = new VersionInfo();
                    vi.ShowDialog();
                    DialogResult result = vi.result;
                    if (result.Equals(DialogResult.Yes))
                    {
                        Utils.appendToLog("User accepted downloading new version");
                        //download new version
                        sw.Reset();
                        sw.Start();
                        string newExeName = Path.Combine(Application.StartupPath, "RelhaxModpack_update.exe");
                        updater.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                        updater.DownloadFileCompleted += new AsyncCompletedEventHandler(updater_DownloadFileCompleted);
                        if (File.Exists(newExeName)) File.Delete(newExeName);
                        updater.DownloadFileAsync(new Uri("http://wotmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.exe"), newExeName);
                        Utils.appendToLog("New application download started");
                        currentModDownloading = "update ";
                    }
                    else
                    {
                        Utils.appendToLog("User declined downlading new version");
                        //close the application
                        this.Close();
                    }
                }
            }
            else
            {
                Utils.appendToLog("ERROR. Failed to get 'manager_version.xml'");
                MessageBox.Show(Translations.getTranslatedString("failedManager_version"), Translations.getTranslatedString("critical"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //close the application
                this.Close();
            }

        }
        //handler for when the update download is complete
        void updater_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            downloadTimer.Enabled = false;
            if (e.Error != null && e.Error.Message.Equals("The remote server returned an error: (404) Not Found."))
            {
                //404
                Utils.appendToLog("ERROR: unable to download new application version");
                MessageBox.Show(Translations.getTranslatedString("cantDownloadNewVersion"));
                this.Close();
            }

            string newExeName = Path.Combine(Application.StartupPath, "RelicCopyUpdate.bat");
            try
            {
                File.WriteAllText(newExeName, @"@ECHO OFF
                ECHO Updating Application...
                ping 127.0.0.1 -n 3 > nul
                del  /Q RelhaxModpack.exe 2> nul
                copy /Y RelhaxModpack_update.exe RelhaxModpack.exe 2> nul
                del /Q RelicModManager_update.exe 2> nul
                del /Q RelhaxModpack_update.exe 2> nul
                del /Q RelicModManager.exe 2> nul
                ECHO Starting Application...
                start """" ""RelhaxModpack.exe"" %1 %2 %3 %4 %5 %6 %7 %8 %9 2> nul
                ".Replace("                ", ""));
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("updater_DownloadFileCompleted", "create RelicCopyUpdate.bat failed", ex);
                string msgTxt = string.Format(Translations.getTranslatedString("failedCreateUpdateBat"), Path.Combine(Application.StartupPath, "RelhaxModpack.exe"), "RelhaxModpack_update.exe", "RelhaxModpack.exe");
                if (DialogResult.Yes == MessageBox.Show(msgTxt, Translations.getTranslatedString("critical"), MessageBoxButtons.YesNo, MessageBoxIcon.Stop))
                {
                    // call the windows explorer and open at the relhax folder
                    ProcessStartInfo explorer = new ProcessStartInfo();
                    explorer.FileName = "explorer.exe";
                    explorer.Arguments = Application.StartupPath;
                    Process callExplorer = new Process();
                    callExplorer.StartInfo = explorer;
                    callExplorer.Start();
                }
                try
                {
                    // try to create a textfile at the temp folder
                    string howToPath = Path.Combine(Path.GetTempPath(), "howTo.txt");
                    File.WriteAllText(howToPath, msgTxt);
                    // call the notepad and open the howto.txt file
                    ProcessStartInfo notepad = new ProcessStartInfo();
                    notepad.FileName = "notepad.exe";
                    notepad.Arguments = howToPath;
                    Process callNotepad = new Process();
                    callNotepad.StartInfo = notepad;
                    callNotepad.Start();
                }
                catch (Exception e2)
                {
                    Utils.exceptionLog("updater_DownloadFileCompleted", "failed to create howTo.txt", e2);
                }
                Application.Exit();
            }

            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = newExeName;
                info.Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray());
                Process installUpdate = new Process();
                installUpdate.StartInfo = info;
                installUpdate.Start();
            }
            catch (Win32Exception e3)
            {
                Utils.exceptionLog("updater_DownloadFileCompleted", "could not start new application version", e3);
                MessageBox.Show(Translations.getTranslatedString("cantStartNewApp") + newExeName);
            }
            Application.Exit();
        }

        //gets the version of tanks that this is, in the format
        //of the res_mods version folder i.e. 0.9.17.0.3
        private string getFolderVersion()
        {
            if (!File.Exists(Path.Combine(tanksLocation, "version.xml")))
                return null;
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(tanksLocation, "version.xml"));
            XmlNode node = doc.SelectSingleNode("//version.xml/version");
            string[] temp = node.InnerText.Split('#');
            string version = temp[0].Trim();
            version = version.Substring(2);
            return version;
        }

        //check to see if the supplied version of tanks is on the list of supported client versions
        private bool isClientVersionSupported(string detectedVersion)
        {
            supportedVersions.Clear();
            string xmlString = Utils.getStringFromZip(Settings.managerInfoDatFile, "supported_clients.xml");  //xml doc name can change
            XDocument doc = XDocument.Parse(xmlString);
            bool result = doc.Descendants("version")
                   .Where(arg => arg.Value.Equals(detectedVersion))
                   .Any();
            if (result)
            {
                XElement element = doc.Descendants("version")
                   .Where(arg => arg.Value.Equals(detectedVersion))
                   .Single();
                // store the onlinefolder version to the string
                Settings.tanksOnlineFolderVersion = element.Attribute("folder").Value;
            }
            else
            {
                // store the the last onlinefolder version to the string, if no valid detectedVersion association is found
                Settings.tanksOnlineFolderVersion = doc.Descendants("version").Last().Attribute("folder").Value;
            }
            // fill the supportedVersions array to possible create messages
            StringReader rdr = new StringReader(xmlString);
            var docV = new XPathDocument(rdr);
            foreach (var version in docV.CreateNavigator().Select("//versions/version"))
            {
                supportedVersions.Add(version.ToString());
            }
            return result;
        }

        //checks the registry to get the location of where WoT is installed
        private string autoFindTanks()
        {
            List<string> searchPathWoT = new List<string>();
            string[] registryPathArray = new string[] { };

            // here we need the value for the searchlist
            // check replay link
            registryPathArray = new string[] { @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.wotreplay\shell\open\command", @"HKEY_CURRENT_USER\Software\Classes\.wotreplay\shell\open\command" };
            foreach (string regEntry in registryPathArray)
            {
                // get values from from registry
                object obj = Registry.GetValue(regEntry, "", -1);
                // if it is not "null", it is containing possible a string
                if (obj != null)
                {
                    try
                    {
                        // add the thing to the checklist, but remove the Quotation Marks in front of the string and the trailing -> " "%1"
                        searchPathWoT.Add(((string)obj).Substring(1).Substring(0, ((string)obj).Length - 7));
                    }
                    catch
                    { } // only exception catching
                }
            }

            // here we need the value for the searchlist
            string regPath = @"HKEY_CURRENT_USER\Software\Wargaming.net\Launcher\Apps\wot";
            RegistryKey subKeyHandle = Registry.CurrentUser.OpenSubKey(regPath.Replace(@"HKEY_CURRENT_USER\", ""));
            if (subKeyHandle != null)
            {
                // get the value names at the reg Key one by one
                foreach (string valueName in subKeyHandle.GetValueNames())
                {
                    // read the value from the regPath
                    object obj = Registry.GetValue(regPath, valueName, -1);
                    if (obj != null)
                    {
                        try
                        {
                            // we did get only a path to used WoT folders, so add the game name to the path and add it to the checklist
                            searchPathWoT.Add(Path.Combine((string)obj, "WorldOfTanks.exe"));
                        }
                        catch
                        { } // only exception catching
                    }
                }
            }

            // here we need the value name for the searchlist
            registryPathArray = new string[] { @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache", @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store" };
            foreach (string p in registryPathArray)
            {
                // set the handle to the registry key
                subKeyHandle = Registry.CurrentUser.OpenSubKey(p);
                if (subKeyHandle == null) continue;            // subKeyHandle == null not existsting
                // parse all value names of the registry key abouve
                foreach (string valueName in subKeyHandle.GetValueNames())
                {
                    try
                    {
                        // if the lower string "worldoftanks.exe" is contained => match !!
                        if (valueName.ToLower().Contains("Worldoftanks.exe".ToLower()))
                        {
                            // remove (replace it with "") the attachment ".ApplicationCompany" or ".FriendlyAppName" in the string and add the string to the searchlist
                            searchPathWoT.Add(valueName.Replace(".ApplicationCompany", "").Replace(".FriendlyAppName", ""));
                        }
                    }
                    catch
                    { } // only exception catching
                }
            }

            // this searchlist is long, maybe 30-40 entries (system depended), but the best possibility to find a currently installed WoT game.
            foreach (string path in searchPathWoT)
            {
                if (File.Exists(path))
                {
                    Utils.appendToLog(string.Format("valid game path found: {0}", path));
                    // write the path to the central value holder
                    tanksLocation = path;
                    // return the path
                    return path;
                }
            }
            // send "null" back if nothing found
            return null;
        }

        //prompts the user to specify where the "WorldOfTanks.exe" file is
        //return the file path and name of "WorldOfTanks.exe"
        private string manuallyFindTanks()
        {
            // try to give an untrained user a littlebit support
            if (autoFindTanks() != null)
            {
                findWotExe.InitialDirectory = Path.GetDirectoryName(tanksLocation);
            }
            //unable to find it in the registry (or user activated manually selection), so ask for it
            if (findWotExe.ShowDialog().Equals(DialogResult.Cancel))
            {
                downloadProgress.Text = Translations.getTranslatedString("canceled");
                return null;
            }
            tanksLocation = findWotExe.FileName;
            return "all good";
        }

        private void downloadResources(string resourcesFile, PleaseWait wait)
        {
            string localDll = Path.Combine(Application.StartupPath, resourcesFile);
            string urlPath = string.Format("http://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/external/{0}", resourcesFile);
            try
            {
                wait.loadingDescBox.Text = string.Format("{0} {1} ... ", Translations.getTranslatedString("Downloading"), resourcesFile);
                Application.DoEvents();
                using (downloader = new WebClient())
                {
                    downloader.DownloadFile(urlPath, localDll);
                    Utils.appendToLog(string.Format("successfully downloaded: {0}", resourcesFile));
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("downloadResources", urlPath, ex);
                MessageBox.Show(string.Format("{0} {1}", Translations.getTranslatedString("failedToDownload_1"), resourcesFile));
                Application.Exit();
            }
        }

        //handler for before the window is displayed
        private void MainWindow_Load(object sender, EventArgs e)
        {
            //set window header text to current version so user knows
            this.Text = this.Text + managerVersion();
            if (Program.testMode) this.Text = this.Text + " TEST MODE";
            //setup the gif preview loading window
            gp = new LoadingGifPreview(this.Location.X + this.Size.Width + 5, this.Location.Y);
            //show the wait screen
            PleaseWait wait = new PleaseWait();
            wait.Show();
            WebRequest.DefaultWebProxy = null;
            Application.DoEvents();
            Utils.appendToLog("|RelHax Modpack " + managerVersion());
            Utils.appendToLog(string.Format("|Built on {0}", compileTime()));
            Utils.appendToLog("|Running on " + System.Environment.OSVersion.ToString());
            /*
            //check for single instance
            Utils.appendToLog("Check for single instance");
            wait.loadingDescBox.Text = Translations.getTranslatedString("appSingleInstance");
            Application.DoEvents();
            int numberInstances = 0;
            foreach (Process p in Process.GetProcesses())
            {
                string s = p.MainWindowTitle;
                if(s.Contains("Relhax"))
                {
                    numberInstances++;
                }
            }
            if(numberInstances > 2)
            {
                MessageBox.Show(Translations.getTranslatedString("anotherInstanceRunning"));
                Application.Exit();
            }
            */
            //create directory structures
            wait.loadingDescBox.Text = Translations.getTranslatedString("verDirStructure");
            Application.DoEvents();
            Utils.appendToLog("Verifying Directory Structure");
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "RelHaxDownloads"))) Directory.CreateDirectory(Path.Combine(Application.StartupPath, "RelHaxDownloads"));
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "RelHaxUserMods"))) Directory.CreateDirectory(Path.Combine(Application.StartupPath, "RelHaxUserMods"));
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "RelHaxModBackup"))) Directory.CreateDirectory(Path.Combine(Application.StartupPath, "RelHaxModBackup"));
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "RelHaxUserConfigs"))) Directory.CreateDirectory(Path.Combine(Application.StartupPath, "RelHaxUserConfigs"));
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "RelHaxTemp"))) Directory.CreateDirectory(Path.Combine(Application.StartupPath, "RelHaxTemp"));
            //check if old dll files can be deleted
            try
            {
                string[] filesToDelete = { "DotNetZip.dll", "Ionic.Zip.dll", "Newtonsoft.Json.dll" };
                foreach (string s in filesToDelete)
                    if (File.Exists(Path.Combine(Application.StartupPath, s)))
                        File.Delete(Path.Combine(Application.StartupPath, s));
            }
            catch (Exception ex)
            {
                Utils.exceptionLog(ex);
            }

            //add method to disable the modpack for during patch day
            //this will involve having a hard coded true or false, along with a command line arguement to over-ride
            //to disable from patch day set it to false.
            //to enable for patch day (prevent users to use it), set it to true.
            if (false && !Program.patchDayTest)
            {
                Utils.appendToLog("Patch day disable detected. Remember To override use /patchday");
                MessageBox.Show(Translations.getTranslatedString("patchDayMessage"));
                this.Close();
            }

            //check for updates
            wait.loadingDescBox.Text = Translations.getTranslatedString("checkForUpdates");
            Application.DoEvents();
            if (Program.skipUpdate)
            {
                Utils.appendToLog("/skip-update switch detected, skipping application update");
                if (!Program.testMode) MessageBox.Show(Translations.getTranslatedString("skipUpdateWarning"));
            }
            else
            {
                this.checkmanagerUpdates();
            }

            //load settings
            wait.loadingDescBox.Text = Translations.getTranslatedString("loadingSettings");
            Utils.appendToLog("Loading settings");
            Settings.loadSettings();
            this.applySettings(true);
            if (Program.testMode)
            {
                Utils.appendToLog("Test Mode is ON, loading local modInfo.xml");
            }
            if (Program.autoInstall)
            {
                Utils.appendToLog("Auto Install is ON, checking for config pref xml at " + Path.Combine(Application.StartupPath, "RelHaxUserConfigs", Program.configName));
                if (!File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserConfigs", Program.configName)))
                {
                    Utils.appendToLog(string.Format("ERROR: {0} does NOT exist, loading in fontRegular mode", Program.configName));
                    MessageBox.Show(string.Format(Translations.getTranslatedString("configNotExist"), Program.configName));
                    Program.autoInstall = false;
                }
                if (!Settings.cleanInstallation)
                {
                    Utils.appendToLog("ERROR: clean installation is set to false. This must be set to true for auto install to work. Loading in fontRegular mode.");
                    MessageBox.Show(Translations.getTranslatedString("autoAndClean"));
                    Program.autoInstall = false;
                }
                if (Settings.firstLoad)
                {
                    Utils.appendToLog("ERROR: First time loading cannot be an auto install mode, loading in fontRegular mode");
                    MessageBox.Show(Translations.getTranslatedString("autoAndFirst"));
                    Program.autoInstall = false;
                }
            }
            //check if it can still load in autoInstall config mode
            if (Program.autoInstall)
            {
                Utils.appendToLog("Program.autoInstall still true, loading in auto install mode");
                wait.Close();
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
            toggleUIButtons(true);
            Application.DoEvents();
            Program.saveSettings = true;
        }

        //when the "visit form page" link is clicked. the link clicked handler
        private void formPageLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.com/index.php?/topic/535868-");
        }

        //handler for when the install relhax modpack button is pressed
        //basicly the entire install process
        private void installRelhaxMod_Click(object sender, EventArgs e)
        {
            Utils.TotallyNotStatPaddingForumPageViewCount();
            toggleUIButtons(false);
            downloadPath = Path.Combine(Application.StartupPath, "RelHaxDownloads");
            //get the user appData folder
            appDataFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wargaming.net", "WorldOfTanks");
            Utils.appendToLog("appDataFolder parsed as " + appDataFolder);
            if (!Directory.Exists(appDataFolder))
            {
                Utils.appendToLog("ERROR: appDataFolder does not exist");
                appDataFolder = "-1";
                if (Settings.clearCache)
                {
                    //can't locate folder, continue installation anyway?
                    DialogResult clearCacheFailResult = MessageBox.Show(Translations.getTranslatedString("appDataFolderNotExist"), Translations.getTranslatedString("appDataFolderNotExistHeader"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (clearCacheFailResult == DialogResult.No)
                    {
                        Utils.appendToLog("user stopped installation");
                        toggleUIButtons(true);
                        return;
                    }
                }
            }
            //reset the interface
            this.downloadProgress.Text = "";
            //attempt to locate the tanks directory automatically
            //if it fails, it will prompt the user to return the world of tanks exe
            if (Settings.forceManuel || this.autoFindTanks() == null)
            {
                if (this.manuallyFindTanks() == null)
                {
                    Utils.appendToLog("user stopped installation");
                    toggleUIButtons(true);
                    return;
                }
            }
            //parse all strings for installation
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
            Utils.appendToLog("tanksLocation parsed as " + tanksLocation);
            Utils.appendToLog("customUserMods parsed as " + Path.Combine(Application.StartupPath, "RelHaxUserMods"));
            // logfile moved from WoT root folder to logs subfolder after manager version 26.4.2
            if (File.Exists(Path.Combine(tanksLocation, "installedRelhaxFiles.log")))
                File.Move(Path.Combine(tanksLocation, "installedRelhaxFiles.log"), Path.Combine(tanksLocation, "logs", "installedRelhaxFiles.log"));
            if (tanksLocation.Equals(Application.StartupPath))
            {
                //display error and abort
                MessageBox.Show(Translations.getTranslatedString("moveOutOfTanksLocation"));
                toggleUIButtons(true);
                return;
            }
            tanksVersion = this.getFolderVersion();
            tanksVersionForInstaller = tanksVersion;
            Utils.appendToLog("tanksVersion parsed as " + tanksVersion);
            //determine if the tanks client version is supported
            if (!Program.testMode && !isClientVersionSupported(tanksVersion))
            {
                //log and inform the user
                Utils.appendToLog("WARNING: Detected client version is " + tanksVersion + ", not supported");
                Utils.appendToLog("Supported versions are: " + string.Join(", ", supportedVersions));
                // parse the string that we get from the server and delete all "Testserver" entries (Testserver entries are the version number with prefix "T")
                string publicVersions = string.Join("\n", supportedVersions.Select(sValue => sValue.Trim()).ToArray().Where(s => !(s.Substring(0, 1) == "T")).ToArray());
                MessageBox.Show(string.Format("{0}: {1}\n{2}\n\n{3}:\n{4}", Translations.getTranslatedString("detectedClientVersion"), tanksVersion, Translations.getTranslatedString("supportNotGuarnteed"), Translations.getTranslatedString("supportedClientVersions"), publicVersions), Translations.getTranslatedString("critical"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // select the last public modpack version
                tanksVersion = publicVersions.Split('\n').Last();
                // go to Client check again, because the online folder must be set correct
                isClientVersionSupported(tanksVersion);
                Utils.appendToLog(string.Format("Version selected: {0}  OnlineFolder: {1}", tanksVersion, Settings.tanksOnlineFolderVersion));
            }
            //if the user wants to, check if the database has actually changed
            if (Settings.NotifyIfSameDatabase && SameDatabaseVersions())
            {
                if (MessageBox.Show(Translations.getTranslatedString("DatabaseVersionsSameBody"), Translations.getTranslatedString("DatabaseVersionsSameHeader"), MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    toggleUIButtons(true);
                    return;
                }
            }
            //reset the childProgressBar value
            childProgressBar.Maximum = 100;
            //childProgressBar.Value = 0;
            //check to make sure that the md5hashdatabase is valid before using it
            string md5HashDatabaseLocation = Path.Combine(Application.StartupPath, "RelHaxDownloads", "MD5HashDatabase.xml");
            if ((File.Exists(md5HashDatabaseLocation)) && (!Utils.IsValidXml(md5HashDatabaseLocation)))
            {
                File.Delete(md5HashDatabaseLocation);
            }
            //show the mod selection window
            ModSelectionList list = new ModSelectionList(tanksVersion, tanksLocation, this.Location.X + this.Size.Width, this.Location.Y);
            list.ShowDialog();
            if (list.cancel)
            {
                try
                {
                    list.Dispose();
                }
                catch
                {
                    Utils.appendToLog("INFO: Failed to dispose list");
                }
                list = null;
                GC.Collect();
                toggleUIButtons(true);
                return;
            }
            //check to see if WoT is running
            bool WoTRunning = true;
            while (WoTRunning)
            {
                WoTRunning = false;
                foreach (Process p in Process.GetProcesses())
                {
                    if (p.MainWindowTitle.Equals("WoT Client"))
                        WoTRunning = true;
                }
                if (!WoTRunning)
                    break;
                MessageBox.Show(Translations.getTranslatedString("WoTRunningMessage"), Translations.getTranslatedString("WoTRunningHeader"));
            }
            // if the delete will raise an exception, it will be ignored
            /*
            try // moved it BEHIND INstaller.UninstallMods() function, cause this file is needed for this function
            {
                if (File.Exists(Path.Combine(tanksLocation, "installedRelhaxFiles.log")))
                    File.Delete(Path.Combine(tanksLocation, "installedRelhaxFiles.log"));
            }
            catch (Exception ex)
            {
                Utils.exceptionLog(ex);
            } */
            //have the application display that it is loading. it is actually doing installation calculations
            downloadProgress.Text = Translations.getTranslatedString("loading");
            Application.DoEvents();
            /*
             * parses all the mods and configs into seperate lists for many types op options
             * like mods/configs to install, mods/configs with data, and others
            */
            //copies it instead
            currentDependencies = new List<Dependency>(list.dependencies);
            currentLogicalDependencies = new List<LogicalDependnecy>(list.logicalDependencies);
            parsedCatagoryLists = new List<Category>(list.parsedCatagoryList);
            globalDependenciesToInstall = new List<Dependency>(list.globalDependencies);
            dependenciesToInstall = new List<Dependency>();
            logicalDependenciesToInstall = new List<LogicalDependnecy>();
            modsConfigsToInstall = new List<DatabaseObject>();
            appendedDependenciesToInstall = new List<Dependency>();
            modsConfigsWithData = new List<DatabaseObject>();
            patchList = new List<Patch>();
            userMods = new List<Mod>();

            //if mod/config is enabled and checked, add it to list of mods to extract/install
            foreach (Category c in parsedCatagoryLists)
            {
                //will itterate through every catagory once
                foreach (Mod m in c.mods)
                {
                    //will itterate through every mod of every catagory once
                    if (m.enabled && m.Checked)
                    {
                        //move each mod that is enalbed and checked to a new list of mods to install
                        //also check that it actually has a zip file
                        if (!m.zipFile.Equals(""))
                            modsConfigsToInstall.Add(m);

                        //since it is checked, regardless if it has a zipfile, check if it has userdata
                        if (m.userFiles.Count > 0)
                            modsConfigsWithData.Add(m);

                        //check for configs
                        if (m.configs.Count > 0)
                            ProcessConfigs(m.configs);

                        //at least one mod of this catagory is checked, add any dependenciesToInstall required
                        if (c.dependencies.Count > 0)
                            processDependencies(c.dependencies);

                        //check dependency is enabled and has a zip file with it
                        if (m.dependencies.Count > 0)
                            processDependencies(m.dependencies);
                    }
                }
            }

            //build the list of mods and configs that use each logical dependency
            foreach (LogicalDependnecy d in currentLogicalDependencies)
            {
                foreach (Dependency depD in currentDependencies)
                {
                    foreach (LogicalDependnecy ld in depD.logicalDependencies)
                    {
                        if (ld.packageName.Equals(d.packageName))
                        {
                            DatabaseLogic dbl = new DatabaseLogic()
                            {
                                PackageName = depD.packageName,
                                Enabled = depD.enabled,
                                Checked = dependenciesToInstall.Contains(depD),
                                NotFlag = ld.negateFlag
                            };
                            d.DatabasePackageLogic.Add(dbl);
                        }
                    }
                }
                //itterate through every mod and config once for each dependency
                //check each one's dependecy list, if packageName's match, add it to the dependency's list of mods/configs that use it
                foreach (Category c in parsedCatagoryLists)
                {
                    //will itterate through every catagory once
                    foreach (Mod m in c.mods)
                    {
                        foreach (LogicalDependnecy ld in m.logicalDependencies)
                        {
                            if (ld.packageName.Equals(d.packageName))
                            {
                                DatabaseLogic dbl = new DatabaseLogic()
                                {
                                    PackageName = m.packageName,
                                    Enabled = m.enabled,
                                    Checked = m.Checked,
                                    NotFlag = ld.negateFlag
                                };
                                d.DatabasePackageLogic.Add(dbl);
                            }
                        }
                        if (m.configs.Count > 0)
                            ProcessConfigsLogical(d, m.configs);
                    }
                }
            }

            //now each logical dependency has a complete list of every dependency, mod, and config that uses it, and if it is enabled and checked
            //indicate if the logical dependency will be installed
            foreach (LogicalDependnecy ld in currentLogicalDependencies)
            {
                //idea is that if all mod/config/dependency are to be installed, then install the logical dependency
                //and factor in the negate flag
                bool addIt = true;
                foreach (DatabaseLogic dl in ld.DatabasePackageLogic)
                {
                    if (dl.NotFlag)
                    {
                        //package must NOT be checked for it to be included
                        //enabled must = true, checked must = false
                        //otherwise break and don't add
                        if (dl.Enabled && dl.Checked)
                        {
                            addIt = false;
                            break;
                        }
                    }
                    else
                    {
                        //package MUST be checked for it to be included
                        //enabled must = true, checked must = true
                        //otherwise break and don't add
                        if (dl.Enabled && !dl.Checked)
                        {
                            addIt = false;
                            break;
                        }
                    }
                }
                if (addIt && !logicalDependenciesToInstall.Contains(ld))
                    logicalDependenciesToInstall.Add(ld);
            }

            //check for dependencies that actually need to be installed at the end
            foreach (Dependency d in dependenciesToInstall)
            {
                if (d.appendExtraction)
                {
                    appendedDependenciesToInstall.Add(d);
                    dependenciesToInstall.Remove(d);
                }
            }

            //check for any user mods to install
            for (int i = 0; i < list.userMods.Count; i++)
            {
                if (list.userMods[i].enabled && list.userMods[i].Checked)
                {
                    this.userMods.Add(list.userMods[i]);
                }
            }

            //create a new download queue. even if not downloading any
            //relhax modpack mods, still used in downloader code
            downloadQueue = new List<DownloadItem>();
            //check that we will actually install something
            if (modsConfigsToInstall.Count == 0 && userMods.Count == 0)
            {
                //pull out because there are no mods to install
                downloadProgress.Text = Translations.getTranslatedString("idle");
                toggleUIButtons(true);
                list.Dispose();
                list = null;
                GC.Collect();
                return;
            }
            //if the user did not select any relhax modpack mods to install
            if (modsConfigsToInstall.Count == 0)
            {
                //clear any dependenciesand logicalDependencies since this is a user mod only installation
                dependenciesToInstall.Clear();
                logicalDependenciesToInstall.Clear();
                appendedDependenciesToInstall.Clear();
            }
            //foreach mod and config and dependnecy, if the crc's don't match, add it to the downloadQueue
            string localFilesDir = Path.Combine(Application.StartupPath, "RelHaxDownloads");
            foreach (Dependency d in globalDependenciesToInstall)
            {
                if (d.downloadFlag)
                {
                    downloadQueue.Add(new DownloadItem(new Uri(d.startAddress + d.dependencyZipFile + d.endAddress), Path.Combine(localFilesDir, d.dependencyZipFile)));
                }
            }
            foreach (Dependency d in dependenciesToInstall)
            {
                if (d.downloadFlag)
                {
                    downloadQueue.Add(new DownloadItem(new Uri(d.startAddress + d.dependencyZipFile + d.endAddress), Path.Combine(localFilesDir, d.dependencyZipFile)));
                }
            }
            foreach (Dependency d in appendedDependenciesToInstall)
            {
                if (d.downloadFlag)
                {
                    downloadQueue.Add(new DownloadItem(new Uri(d.startAddress + d.dependencyZipFile + d.endAddress), Path.Combine(localFilesDir, d.dependencyZipFile)));
                }
            }
            foreach (LogicalDependnecy ld in logicalDependenciesToInstall)
            {
                if (ld.downloadFlag)
                {
                    downloadQueue.Add(new DownloadItem(new Uri(ld.startAddress + ld.dependencyZipFile + ld.endAddress), Path.Combine(localFilesDir, ld.dependencyZipFile)));
                }
            }
            foreach (DatabaseObject dbo in modsConfigsToInstall)
            {
                if (dbo.downloadFlag)
                {
                    //crc's don't match, need to re-download
                    downloadQueue.Add(new DownloadItem(new Uri(dbo.startAddress + dbo.zipFile + dbo.endAddress), Path.Combine(localFilesDir, dbo.zipFile)));
                }
            }

            //reset the progress bars
            parrentProgressBar.Maximum = downloadQueue.Count;
            childProgressBar.Maximum = 100;
            //at this point, there may be user mods selected,
            //and there is at least one mod to extract
            downloader_DownloadFileCompleted(null, null);
            //release no longer needed rescources and end the installation process
            list.Dispose();
            list = null;
            GC.Collect();
            return;
        }

        private void ProcessConfigs(List<Config> configList)
        {
            foreach (Config config in configList)
            {
                if (config.enabled && config.Checked)
                {
                    if (!config.zipFile.Equals(""))
                        modsConfigsToInstall.Add(config);

                    //check for userdata
                    if (config.userFiles.Count > 0)
                        modsConfigsWithData.Add(config);

                    //check for configs
                    if (config.configs.Count > 0)
                        ProcessConfigs(config.configs);

                    //check for dependencies
                    if (config.dependencies.Count > 0)
                        processDependencies(config.dependencies);
                }
            }
        }

        private void ProcessConfigsLogical(LogicalDependnecy d, List<Config> configList)
        {
            foreach (Config config in configList)
            {
                foreach (LogicalDependnecy ld in config.logicalDependencies)
                {
                    if (ld.packageName.Equals(d.packageName))
                    {
                        DatabaseLogic dl = new DatabaseLogic()
                        {
                            PackageName = config.packageName,
                            Enabled = config.enabled,
                            Checked = config.Checked,
                            NotFlag = ld.negateFlag
                        };
                        d.DatabasePackageLogic.Add(dl);
                    }
                }
            }
        }

        //processes a list of dependencies to add them (if needed) to the list of dependencies to install
        private void processDependencies(List<Dependency> dependencies)
        {
            //every dependency is only a packageName, and each must be added if they are not there already
            //but first need to find it
            foreach (Dependency d in dependencies)
            {
                Dependency temp = null;
                //find the actual dependency object from the list of available dependencies
                bool error = true;
                foreach (Dependency dd in currentDependencies)
                {
                    if (dd.packageName.Equals(d.packageName))
                    {
                        //the packageName has been linked to the dependency
                        error = false;
                        temp = dd;
                        break;
                    }
                }
                if (error)
                {
                    Utils.appendToLog(string.Format("ERROR: could not match packageName '{0}' from the list of dependencies", d.packageName));
                    break;
                }
                //dependency has been found, if it's not in the list currently to install, add it
                if (!dependenciesToInstall.Contains(temp))
                    dependenciesToInstall.Add(temp);
            }
        }

        private string createExtractionMsgBoxProgressOutput(string[] s)
        {
            return string.Format("{0} {1} {2} {3}\n{4}: {5}\n{6}: {7} MB",
                Translations.getTranslatedString("extractingPackage"),
                    s[0],
                    Translations.getTranslatedString("of"),
                    s[1],
                    Translations.getTranslatedString("file"),
                    s[2],
                    Translations.getTranslatedString("size"),
                    s[3]);
        }

        private void I_InstallProgressChanged(object sender, InstallerEventArgs e)
        {
            string message = "";
            totalProgressBar.Maximum = (int)InstallerEventArgs.InstallProgress.Done;
            if (e.InstalProgress == InstallerEventArgs.InstallProgress.BackupMods)
            {
                message = string.Format("{0} {1} {2} {3}", Translations.getTranslatedString("backupModFile"), e.ChildProcessed, Translations.getTranslatedString("of"), e.ChildTotalToProcess);
                childProgressBar.Maximum = e.ChildTotalToProcess;
                if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                    childProgressBar.Value = e.ChildProcessed;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.BackupMods;
                parrentProgressBar.Value = 0;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.BackupUserData)
            {
                message = string.Format("{0} {1} {2} {3}", Translations.getTranslatedString("backupUserdatas"), e.ChildProcessed, Translations.getTranslatedString("of"), e.ChildTotalToProcess);
                childProgressBar.Maximum = e.ChildTotalToProcess;
                if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                    childProgressBar.Value = e.ChildProcessed;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.BackupUserData;
                parrentProgressBar.Value = 0;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.DeleteMods)
            {
                message = string.Format("{0} {1} {2} {3}\n{4}: {5}", Translations.getTranslatedString("deletingFiles"), e.ChildProcessed, Translations.getTranslatedString("of"), e.ChildTotalToProcess, Translations.getTranslatedString("file"), e.currentFile);
                childProgressBar.Maximum = e.ChildTotalToProcess;
                if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                    childProgressBar.Value = e.ChildProcessed;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.DeleteMods;
                parrentProgressBar.Value = 0;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.DeleteWoTCache)
            {
                message = Translations.getTranslatedString("deletingWOTCache") + " ";
                childProgressBar.Value = 0;
                parrentProgressBar.Value = 0;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.DeleteWoTCache;
            }
            else if (
                    e.InstalProgress == InstallerEventArgs.InstallProgress.ExtractGlobalDependencies ||
                    e.InstalProgress == InstallerEventArgs.InstallProgress.ExtractDependencies ||
                    e.InstalProgress == InstallerEventArgs.InstallProgress.ExtractLogicalDependencies ||
                    e.InstalProgress == InstallerEventArgs.InstallProgress.ExtractMods ||
                    e.InstalProgress == InstallerEventArgs.InstallProgress.ExtractConfigs ||
                    e.InstalProgress == InstallerEventArgs.InstallProgress.ExtractAppendedDependencies
                    )
            {
                message = createExtractionMsgBoxProgressOutput(new string[] { e.ParrentProcessed.ToString(), e.ParrentTotalToProcess.ToString(), e.currentFile, Math.Round(e.currentFileSizeProcessed / MBDivisor, 2).ToString() });
                parrentProgressBar.Maximum = e.ParrentTotalToProcess;
                if ((parrentProgressBar.Minimum <= e.ParrentProcessed) && (e.ParrentProcessed <= parrentProgressBar.Maximum))
                    parrentProgressBar.Value = e.ParrentProcessed;
                childProgressBar.Maximum = e.ChildTotalToProcess;
                if (e.ChildProcessed > 0)
                    if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                        childProgressBar.Value = e.ChildProcessed;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.ExtractGlobalDependencies;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.RestoreUserData)
            {
                message = string.Format("{0} {1} {2} {3}", Translations.getTranslatedString("restoringUserData"), e.ChildProcessed, Translations.getTranslatedString("of"), e.ChildTotalToProcess);
                parrentProgressBar.Value = 0;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.RestoreUserData;
                childProgressBar.Maximum = e.ChildTotalToProcess;
                if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                    childProgressBar.Value = e.ChildProcessed;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.PatchMods)
            {
                message = string.Format("{0} {1}, {2} {3} {4}", Translations.getTranslatedString("patchingFile"), e.currentFile, e.ChildProcessed, Translations.getTranslatedString("of"), e.ChildTotalToProcess);
                parrentProgressBar.Maximum = e.ChildTotalToProcess;
                if ((parrentProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= parrentProgressBar.Maximum))
                    parrentProgressBar.Value = e.ChildProcessed;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.PatchMods;
                childProgressBar.Value = 0;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.InstallFonts)
            {
                message = Translations.getTranslatedString("installingFonts") + " ";
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.InstallFonts;
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.ExtractUserMods)
            {
                message = createExtractionMsgBoxProgressOutput(new string[] { e.ParrentProcessed.ToString(), e.ParrentTotalToProcess.ToString(), e.currentFile, Math.Round(e.currentFileSizeProcessed / MBDivisor, 2).ToString() });
                parrentProgressBar.Maximum = e.ParrentTotalToProcess;
                if ((parrentProgressBar.Minimum <= e.ParrentProcessed) && (e.ParrentProcessed <= parrentProgressBar.Maximum))
                    parrentProgressBar.Value = e.ParrentProcessed;
                childProgressBar.Maximum = e.ChildTotalToProcess;
                if (e.ChildProcessed > 0)
                    childProgressBar.Value = e.ChildProcessed;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.ExtractUserMods;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.PatchUserMods)
            {
                message = string.Format("{0} {1}, {2} {3} {4}", Translations.getTranslatedString("userPatchingFile"), e.currentFile, e.ChildProcessed, Translations.getTranslatedString("of"), e.ChildTotalToProcess);
                parrentProgressBar.Maximum = e.ChildTotalToProcess;
                if ((parrentProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= parrentProgressBar.Maximum))
                    parrentProgressBar.Value = e.ChildProcessed;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.PatchMods;
                childProgressBar.Value = 0;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.InstallUserFonts)
            {
                message = Translations.getTranslatedString("installingUserFonts") + " ";
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.InstallFonts;
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.CheckDatabase)
            {
                message = string.Format("{0}: {1}", Translations.getTranslatedString("deletingFile"), e.currentFile);
                parrentProgressBar.Maximum = e.ChildTotalToProcess;
                if ((parrentProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= parrentProgressBar.Maximum))
                    parrentProgressBar.Value = e.ChildProcessed;
                totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.CheckDatabase;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.CleanUp)
            {
                message = Translations.getTranslatedString("done");
                totalProgressBar.Value = totalProgressBar.Maximum;
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Maximum = 1;
                childProgressBar.Value = childProgressBar.Maximum;
                downloadProgress.Text = message;
                if (Settings.ShowInstallCompleteWindow)
                {
                    using (InstallFinished IF = new InstallFinished(tanksLocation))
                    {
                        System.Media.SystemSounds.Beep.Play();
                        IF.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show(Translations.getTranslatedString("installationFinished"), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.Done)
            {
                //dispose of a lot of stuff
                if (ins != null)
                {
                    ins.Dispose();
                    ins = null;
                }
                if (unI != null)
                {
                    unI.Dispose();
                    unI = null;
                }
                globalDependenciesToInstall = null;
                dependenciesToInstall = null;
                logicalDependenciesToInstall = null;
                appendedDependenciesToInstall = null;
                modsConfigsToInstall = null;
                downloadQueue = null;
                parsedCatagoryLists = null;
                patchList = null;
                userMods = null;
                if (helper != null)
                {
                    helper.Dispose();
                    helper = null;
                }
                modsConfigsWithData = null;
                toggleUIButtons(true);
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.Uninstall)
            {
                totalProgressBar.Value = 0;
                parrentProgressBar.Value = 0;
                message = string.Format("{0} {1} {2} {3}\n{4}: {5}", Translations.getTranslatedString("uninstallingFile"), e.ChildProcessed, Translations.getTranslatedString("of"), e.ChildTotalToProcess, Translations.getTranslatedString("file"), e.currentFile);
                childProgressBar.Maximum = e.ChildTotalToProcess;
                if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                    childProgressBar.Value = e.ChildProcessed;
            }
            else if (e.InstalProgress == InstallerEventArgs.InstallProgress.UninstallDone)
            {
                message = Translations.getTranslatedString("done");
                totalProgressBar.Value = totalProgressBar.Maximum;
                parrentProgressBar.Maximum = 1;
                parrentProgressBar.Value = parrentProgressBar.Maximum;
                childProgressBar.Maximum = 1;
                childProgressBar.Value = childProgressBar.Maximum;
            }
            else
            {
                Utils.appendToLog("Invalid state: " + e.InstalProgress);
            }
            if (errorCounter > 0 && Program.testMode)
            {
                this.ErrorCounterLabel.Visible = true;
                this.ErrorCounterLabel.Text = string.Format("Error counter: {0}", errorCounter);
            }
            downloadProgress.Text = message;
        }

        //Main method to uninstall the modpack
        private void uninstallRelhaxMod_Click(object sender, EventArgs e)
        {
            toggleUIButtons(false);
            //reset the interface
            this.downloadProgress.Text = "";
            //attempt to locate the tanks directory
            if (Settings.forceManuel || this.autoFindTanks() == null)
            {
                if (this.manuallyFindTanks() == null)
                {
                    toggleUIButtons(true);
                    return;
                }
            }
            //parse all strings
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
            Utils.appendToLog(string.Format("tanksLocation parsed as {0}", tanksLocation));
            Utils.appendToLog(string.Format("customUserMods parsed as {0}", Path.Combine(Application.StartupPath, "RelHaxUserMods")));
            tanksVersion = this.getFolderVersion();
            if (MessageBox.Show(string.Format("{0}\n\n{1}", Translations.getTranslatedString("confirmUninstallMessage"), tanksLocation), Translations.getTranslatedString("confirmUninstallHeader"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                unI = new Installer()
                {
                    AppPath = Application.StartupPath,
                    TanksLocation = tanksLocation,
                    TanksVersion = tanksVersion
                };
                unI.InstallProgressChanged += I_InstallProgressChanged;
                Utils.appendToLog("Started Uninstallation process");
                //run the recursive complete uninstaller
                unI.StartCleanUninstallation();
            }
            else
            {
                toggleUIButtons(true);
            }
        }
        //applies all settings from static settings class to this form
        private void applySettings(bool init = false)
        {
            string text = Translations.getTranslatedString("mainFormToolTip");
            Control[] toolTipSetList = new Control[] { forceManuel, cleanInstallCB, backupModsCheckBox, backupModsCheckBox, cancerFontCB, saveLastInstallCB, saveUserDataCB, darkUICB, languageSelectionGB, fontSizeGB, selectionDefault, selectionLegacy, disableBordersCB, disableColorsCB, clearCacheCB, clearLogFilesCB, viewAppUpdates, ShowInstallCompleteWindowCB, notifyIfSameDatabaseCB, standardImageRB, thirdGuardsLoadingImageRB, languageENG, languageGER, languagePL, languageFR, fontSize100, DPI100 };
            foreach (var set in toolTipSetList)
            {
                // this.toolTip.SetToolTip(forceManuel, Translations.getTranslatedString("mainFormToolTip"));
                this.toolTip.SetToolTip(set, text);
            }
            this.forceManuel.Text = Translations.getTranslatedString(forceManuel.Name);
            this.cleanInstallCB.Text = Translations.getTranslatedString(cleanInstallCB.Name);
            this.backupModsCheckBox.Text = Translations.getTranslatedString(backupModsCheckBox.Name);
            this.cancerFontCB.Text = Translations.getTranslatedString(cancerFontCB.Name);
            this.saveLastInstallCB.Text = Translations.getTranslatedString(saveLastInstallCB.Name);
            this.saveUserDataCB.Text = Translations.getTranslatedString(saveUserDataCB.Name);
            this.darkUICB.Text = Translations.getTranslatedString(darkUICB.Name);
            this.installRelhaxMod.Text = Translations.getTranslatedString(installRelhaxMod.Name);
            this.uninstallRelhaxMod.Text = Translations.getTranslatedString(uninstallRelhaxMod.Name);
            this.settingsGroupBox.Text = Translations.getTranslatedString(settingsGroupBox.Name);
            this.loadingImageGroupBox.Text = Translations.getTranslatedString(loadingImageGroupBox.Name);
            this.languageSelectionGB.Text = Translations.getTranslatedString(languageSelectionGB.Name);
            this.findBugAddModLabel.Text = Translations.getTranslatedString(findBugAddModLabel.Name);
            this.formPageLink.Text = Translations.getTranslatedString(formPageLink.Name);
            this.viewTypeGB.Text = Translations.getTranslatedString("ModSelectionListViewSelection");
            this.selectionDefault.Text = Translations.getTranslatedString(selectionDefault.Name);
            this.selectionLegacy.Text = Translations.getTranslatedString(selectionLegacy.Name);
            this.donateLabel.Text = Translations.getTranslatedString(donateLabel.Name);
            this.cancelDownloadButton.Text = Translations.getTranslatedString(cancelDownloadButton.Name);
            this.fontSizeGB.Text = Translations.getTranslatedString(fontSizeGB.Name);
            this.expandNodesDefault.Text = Translations.getTranslatedString(expandNodesDefault.Name);
            this.disableBordersCB.Text = Translations.getTranslatedString(disableBordersCB.Name);
            this.clearCacheCB.Text = Translations.getTranslatedString(clearCacheCB.Name);
            this.DiscordServerLink.Text = Translations.getTranslatedString(DiscordServerLink.Name);
            this.viewAppUpdates.Text = Translations.getTranslatedString(viewAppUpdates.Name);
            this.viewDBUpdates.Text = Translations.getTranslatedString(viewDBUpdates.Name);
            this.disableColorsCB.Text = Translations.getTranslatedString(disableColorsCB.Name);
            this.clearLogFilesCB.Text = Translations.getTranslatedString(clearLogFilesCB.Name);
            this.notifyIfSameDatabaseCB.Text = Translations.getTranslatedString(notifyIfSameDatabaseCB.Name);
            this.ShowInstallCompleteWindowCB.Text = Translations.getTranslatedString(ShowInstallCompleteWindowCB.Name);
            if (helper != null)
            {
                helper.helperText.Text = Translations.getTranslatedString("helperText");
            }
            if (init)
            {
                //apply all checkmarks
                this.forceManuel.Checked = Settings.forceManuel;
                this.cleanInstallCB.Checked = Settings.cleanInstallation;
                this.backupModsCheckBox.Checked = Settings.backupModFolder;
                this.cancerFontCB.Checked = Settings.comicSans;
                this.saveLastInstallCB.Checked = Settings.saveLastConfig;
                this.saveUserDataCB.Checked = Settings.saveUserData;
                this.darkUICB.Checked = Settings.darkUI;
                this.expandNodesDefault.Checked = Settings.expandAllLegacy;
                this.disableBordersCB.Checked = Settings.disableBorders;
                this.clearCacheCB.Checked = Settings.clearCache;
                this.disableColorsCB.Checked = Settings.disableColorChange;
                this.clearLogFilesCB.Checked = Settings.deleteLogs;
                this.Font = Settings.appFont;
                this.notifyIfSameDatabaseCB.Checked = Settings.NotifyIfSameDatabase;
                this.ShowInstallCompleteWindowCB.Checked = Settings.ShowInstallCompleteWindow;
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
                        languageENG.Checked = true;
                        break;
                    case (Translations.Languages.German):
                        //set english button
                        languageGER.Checked = true;
                        break;
                    case (Translations.Languages.Polish):
                        //set polish translation
                        languagePL.Checked = true;
                        break;
                    case (Translations.Languages.French):
                        //set french translation
                        languageFR.Checked = true;
                        break;
                }
                switch (Settings.sView)
                {
                    case (Settings.SelectionView.defaultt):
                        //set default button, but disable checkedChanged handler to prevent stack overflow
                        selectionDefault.Checked = true;
                        break;
                    case (Settings.SelectionView.legacy):
                        selectionLegacy.Checked = true;
                        break;
                }
                switch (Settings.fontSizeforum)
                {
                    case (Settings.FontSize.font100):
                        fontSize100.Checked = true;
                        break;
                    case (Settings.FontSize.font125):
                        fontSize125.Checked = true;
                        break;
                    case (Settings.FontSize.font175):
                        fontSize175.Checked = true;
                        break;
                    case (Settings.FontSize.font225):
                        fontSize225.Checked = true;
                        break;
                    case (Settings.FontSize.font275):
                        fontSize275.Checked = true;
                        break;
                    case (Settings.FontSize.DPI100):
                        DPI100.Checked = true;
                        break;
                    case (Settings.FontSize.DPI125):
                        DPI125.Checked = true;
                        break;
                    case (Settings.FontSize.DPI175):
                        DPI175.Checked = true;
                        break;
                    case (Settings.FontSize.DPI225):
                        DPI225.Checked = true;
                        break;
                    case (Settings.FontSize.DPI275):
                        DPI275.Checked = true;
                        break;
                    case (Settings.FontSize.DPIAUTO):
                        DPIAUTO.Checked = true;
                        break;
                }
            }
        }
        //check for old zip files
        private void depricated_checkForOldZipFiles()  // this process moved to Installer.cs
        {
            List<string> zipFilesList = new List<string>();
            FileInfo[] fi = null;
            try
            {
                File.SetAttributes(Path.Combine(Application.StartupPath, "RelHaxDownloads"), FileAttributes.Normal);
                DirectoryInfo di = new DirectoryInfo(Path.Combine(Application.StartupPath, "RelHaxDownloads"));
                //get every patch file in the folder
                fi = di.GetFiles(@"*.zip", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                Utils.exceptionLog("checkForOldZipFiles", e);
                MessageBox.Show(Translations.getTranslatedString("folderDeleteFailed") + " _readme");
            }
            if (fi != null)
            {
                foreach (FileInfo f in fi)
                {
                    zipFilesList.Add(f.Name);
                }
                List<string> filesToDelete = Utils.depricated_createDownloadedOldZipsList(zipFilesList, parsedCatagoryLists, globalDependenciesToInstall, currentDependencies, currentLogicalDependencies);
                string listOfFiles = "";
                foreach (string s in filesToDelete)
                    listOfFiles = listOfFiles + s + "\n";
                using (OldFilesToDelete oftd = new OldFilesToDelete())
                {
                    oftd.filesList.Text = listOfFiles;
                    if (listOfFiles.Count() == 0)
                        return;
                    oftd.ShowDialog();
                    if (oftd.result)
                    {
                        childProgressBar.Minimum = 0;
                        childProgressBar.Value = childProgressBar.Minimum;
                        childProgressBar.Maximum = filesToDelete.Count;
                        foreach (string s in filesToDelete)
                        {
                            bool retry = true;
                            bool breakOut = false;
                            while (retry)
                            {
                                //for each zip file, verify it exists, set properties to normal, delete it
                                try
                                {
                                    string file = Path.Combine(Application.StartupPath, "RelHaxDownloads", s);
                                    File.SetAttributes(file, FileAttributes.Normal);
                                    File.Delete(file);
                                    // remove file from database, too
                                    Utils.deleteMd5HashDatabase(file);
                                    childProgressBar.Value++;
                                    retry = false;
                                }
                                catch (Exception e)
                                {
                                    retry = true;
                                    Utils.exceptionLog("checkForOldZipFiles", "delete", e);
                                    DialogResult res = MessageBox.Show(string.Format("{0} {1}", Translations.getTranslatedString("fileDeleteFailed"), s), "", MessageBoxButtons.RetryCancel);
                                    if (res == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        breakOut = true;
                                        retry = false;
                                    }
                                }
                            }
                            if (breakOut)
                                break;
                        }
                    }
                }
            }
        }

        //for when downloads are started, a timer to keep track of the download speed and ETA
        private void downloadTimer_Tick(object sender, EventArgs e)
        {
            differenceTotalBytesDownloaded = currentTotalBytesDownloaded - previousTotalBytesDownloaded;
            float intervalInSeconds = (float)downloadTimer.Interval / 1000;
            float sessionMBytesDownloaded = differenceTotalBytesDownloaded / MBDivisor;
            sessionDownloadSpeed = sessionMBytesDownloaded / intervalInSeconds;
            //set the previous for the last amount of bytes downloaded
            previousTotalBytesDownloaded = currentTotalBytesDownloaded;
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
            saveUserDataCB.Enabled = enableToggle;
            saveLastInstallCB.Enabled = enableToggle;
            fontSize100.Enabled = enableToggle;
            fontSize125.Enabled = enableToggle;
            fontSize175.Enabled = enableToggle;
            fontSize225.Enabled = enableToggle;
            fontSize275.Enabled = enableToggle;
            DPI100.Enabled = enableToggle;
            DPI125.Enabled = enableToggle;
            DPI175.Enabled = enableToggle;
            DPI225.Enabled = enableToggle;
            DPI275.Enabled = enableToggle;
            DPIAUTO.Enabled = enableToggle;
            clearCacheCB.Enabled = enableToggle;
            clearLogFilesCB.Enabled = enableToggle;
            notifyIfSameDatabaseCB.Enabled = enableToggle;
            ShowInstallCompleteWindowCB.Enabled = enableToggle;
        }
        //Checks if the current database version is the same as the database version last installed into the selected World_of_Tanks directory
        private bool SameDatabaseVersions()
        {
            try
            {
                string xmlString = Utils.getStringFromZip(Settings.managerInfoDatFile, "manager_version.xml");  //xml doc name can change
                XDocument doc = XDocument.Parse(xmlString);

                var databaseVersion = doc.CreateNavigator().SelectSingleNode("/version/database");
                databaseVersionString = databaseVersion.InnerXml;
                string installedfilesLogPath = Path.Combine(tanksLocation, "logs", "installedRelhaxFiles.log");
                if (!File.Exists(installedfilesLogPath))
                    return false;
                string[] lastInstalledDatabaseVersionString = File.ReadAllText(installedfilesLogPath).Split('\n');
                //use index 0 of array, index 18 of string array
                string theDatabaseVersion = lastInstalledDatabaseVersionString[0].Substring(18).Trim();
                if (databaseVersionString.Equals(theDatabaseVersion))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("SameDatabaseVersions", "ex", ex);
                return false;
            }
        }
        //handler for when the window is goingto be closed
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save settings
            if (Program.saveSettings) Settings.saveSettings();
            Utils.appendToLog("cleaning \"RelHaxTemp\" folder");
            Utils.DirectoryDelete(Path.Combine(Application.StartupPath, "RelHaxTemp"), true);
            Utils.appendToLog(string.Format("Exception counted: {0}", errorCounter));
            Utils.appendToLog("Application Closing");
            Utils.appendToLog("|------------------------------------------------------------------------------------------------|");
        }

        #region LinkClicked Events
        private void donateLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=76KNV8KXKYNG2");
        }

        private void findBugAddModLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.google.com/spreadsheets/d/1LmPCMAx0RajW4lVYAnguHjjd8jArtWuZIGciFN76AI4/edit?usp=sharing");
        }

        private void DiscordServerLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/58fdPvK");
        }
        #endregion

        #region Loading animations handlers
        //handler for when the "standard" loading animation is clicked
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
            gp.Hide();
            gp.SetLoadingImage();
            gp.Show();
            GC.Collect();
        }
        #endregion

        #region MouseEnter/MouseLeave events
        private void generic_MouseLeave(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("helperText");
        }

        private void forceManuel_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("forceManuelDescription");
        }

        private void cleanInstallCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("cleanInstallDescription");
        }

        private void backupModsCheckBox_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("backupModsDescription");
        }

        private void cancerFontCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("comicSansDescription");
        }

        private void largerFontButton_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("enlargeFontDescription");
        }

        private void standardImageRB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("selectGifDesc");
        }

        private void saveLastInstallCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("saveLastConfigInstall");
        }

        private void font_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("font_MouseEnter");
        }

        private void selectionView_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("selectionView_MouseEnter");
        }

        private void expandNodesDefault_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("expandAllDesc");
        }

        private void language_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("language_MouseEnter");
        }

        private void disableBordersCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("disableBordersDesc");

        }
        private void clearCacheCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("clearCachCBExplanation");
        }

        private void saveUserDataCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("saveUserDataDesc");
        }

        private void cleanUninstallCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("cleanUninstallDescription");
        }

        private void disableColorsCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("disableColorsDescription");
        }

        private void clearLogFilesCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("clearLogFilesDescription");
        }

        private void darkUICB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("darkUIDesc");
        }

        private void notifyIfSameDatabaseCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("notifyIfSameDatabaseCBExplanation");
        }

        private void ShowInstallCompleteWindowCB_MouseEnter(object sender, EventArgs e)
        {
            if (helper != null)
                helper.helperText.Text = Translations.getTranslatedString("ShowInstallCompleteWindowCBExplanation");
        }
        #endregion

        #region MouseDown events
        private void font_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("font_MouseEnter");
                newHelper.ShowDialog();
            }
        }

        private void language_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("language_MouseEnter");
                newHelper.ShowDialog();
            }
        }

        private void forceManuel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("forceManuelDescription");
                newHelper.ShowDialog();
            }
        }

        private void cleanInstallCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("cleanInstallDescription");
                newHelper.ShowDialog();
            }
        }

        private void backupModsCheckBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("backupModsDescription");
                newHelper.ShowDialog();
            }
        }

        private void cancerFontCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("comicSansDescription");
                newHelper.ShowDialog();
            }
        }

        private void largerFontButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("enlargeFontDescription");
                newHelper.ShowDialog();
            }
        }

        private void saveLastInstallCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("saveLastConfigInstall");
                newHelper.ShowDialog();
            }
        }

        private void saveUserDataCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("saveUserDataDesc");
                newHelper.ShowDialog();
            }
        }

        private void expandNodesDefault_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("expandAllDesc");
                newHelper.ShowDialog();
            }
        }

        private void cleanUninstallCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("cleanUninstallDescription");
                newHelper.ShowDialog();
            }
        }

        private void darkUICB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("darkUIDesc");
                newHelper.ShowDialog();
            }
        }

        private void selectionDefault_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("selectionViewMode");
                newHelper.ShowDialog();
            }
        }

        private void selectionLegacy_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("selectionViewMode");
                newHelper.ShowDialog();
            }
        }

        private void disableBordersCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("disableBordersDesc");
                newHelper.ShowDialog();
            }
        }

        private void clearCacheCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("clearCachCBExplanation");
                newHelper.ShowDialog();
            }
        }

        private void disableColorsCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("disableColorsCBExplanation");
                newHelper.ShowDialog();
            }
        }

        private void clearLogFilesCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("clearLogFilesCBExplanation");
                newHelper.ShowDialog();
            }
        }

        private void notifyIfSameDatabaseCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("notifyIfSameDatabaseCBExplanation");
                newHelper.ShowDialog();
            }
        }

        private void ShowInstallCompleteWindowCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            using (FirstLoadHelper newHelper = new FirstLoadHelper(this.Location.X + this.Size.Width + 10, this.Location.Y))
            {
                newHelper.helperText.Text = Translations.getTranslatedString("ShowInstallCompleteWindowCBExplanation");
                newHelper.ShowDialog();
            }
        }
        #endregion

        #region CheckChanged events
        //handler for what happends when the check box "clean install" is checked or not
        private void cleanInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.cleanInstallation = cleanInstallCB.Checked;
        }
        //enalbes the user to use "comic sans" font for the 1 person that would ever want to do that
        private void cancerFontCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.comicSans = cancerFontCB.Checked;
            Settings.ApplyScalingProperties();
            this.Font = Settings.appFont;
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

        private void saveLastInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.saveLastConfig = saveLastInstallCB.Checked;
        }

        private void saveUserDataCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.saveUserData = saveUserDataCB.Checked;
        }

        private void darkUICB_CheckedChanged(object sender, EventArgs e)
        {
            //set the thing
            Settings.darkUI = darkUICB.Checked;
            Settings.setUIColor(this);
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

        private void selectionDefault_CheckedChanged(object sender, EventArgs e)
        {
            Settings.sView = Settings.SelectionView.defaultt;
            this.applySettings();
        }

        private void selectionLegacy_CheckedChanged(object sender, EventArgs e)
        {
            Settings.sView = Settings.SelectionView.legacy;
            this.applySettings();
        }

        private void languagePL_CheckedChanged(object sender, EventArgs e)
        {
            Translations.language = Translations.Languages.Polish;
            this.applySettings();
        }

        private void languageFR_CheckedChanged(object sender, EventArgs e)
        {
            Translations.language = Translations.Languages.French;
            this.applySettings();
        }

        private void expandNodesDefault_CheckedChanged(object sender, EventArgs e)
        {
            Settings.expandAllLegacy = expandNodesDefault.Checked;
        }

        private void disableBordersCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.disableBorders = disableBordersCB.Checked;
        }

        private void clearCacheCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.clearCache = clearCacheCB.Checked;
        }

        private void fontSize100_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize100.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.fontSizeforum = Settings.FontSize.DPI100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.appFont;
                }
                //change settings enum
                Settings.fontSizeforum = Settings.FontSize.font100;
                //apply change of settings enum
                Settings.ApplyScalingProperties();
                //get new scalingMode (or no change, get it anyway)
                this.AutoScaleMode = Settings.appScalingMode;
                //get new font
                this.Font = Settings.appFont;
            }
        }

        private void fontSize125_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize125.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.fontSizeforum = Settings.FontSize.DPI100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.font125;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                this.Font = Settings.appFont;
            }
        }

        private void fontSize175_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize175.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.fontSizeforum = Settings.FontSize.DPI100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.font175;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                this.Font = Settings.appFont;
            }
        }

        private void fontSize225_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize225.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.fontSizeforum = Settings.FontSize.DPI100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.font225;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                this.Font = Settings.appFont;
            }
        }

        private void fontSize275_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize275.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.fontSizeforum = Settings.FontSize.DPI100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.font275;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                this.Font = Settings.appFont;
            }
        }

        private void DPI100_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI100.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.fontSizeforum = Settings.FontSize.font100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.DPI100;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                float temp = 1.0f / scale;
                this.Scale(new SizeF(temp, temp));
                scale = 1.0f;
                this.Font = Settings.appFont;
            }
        }

        private void DPI125_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI125.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.fontSizeforum = Settings.FontSize.font100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.DPI125;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                float temp = 1.25f / scale;
                this.Scale(new SizeF(temp, temp));
                scale = 1.25f;
                this.Font = Settings.appFont;
            }
        }

        private void DPI175_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI175.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.fontSizeforum = Settings.FontSize.font100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.DPI175;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                this.Font = Settings.appFont;
                float temp = Settings.scale175 / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.scale175;
            }
        }

        private void DPI225_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI225.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.fontSizeforum = Settings.FontSize.font100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.DPI225;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                this.Font = Settings.appFont;
                float temp = Settings.scale225 / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.scale225;
            }
        }

        private void DPI275_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI275.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.fontSizeforum = Settings.FontSize.font100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.DPI275;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                this.Font = Settings.appFont;
                float temp = Settings.scale275 / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.scale275;
            }
        }

        private void DPIAUTO_CheckedChanged(object sender, EventArgs e)
        {
            if (DPIAUTO.Checked)
            {
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.fontSizeforum = Settings.FontSize.font100;
                    Settings.ApplyScalingProperties();
                    this.AutoScaleMode = Settings.appScalingMode;
                    this.Font = Settings.appFont;
                }
                Settings.fontSizeforum = Settings.FontSize.DPIAUTO;
                Settings.ApplyScalingProperties();
                this.AutoScaleMode = Settings.appScalingMode;
                this.Font = Settings.appFont;
                float temp = Settings.scaleSize / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.scaleSize;
            }
        }

        private void clearLogFilesCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.deleteLogs = clearLogFilesCB.Checked;
        }

        private void disableColorsCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.disableColorChange = disableColorsCB.Checked;
        }

        private void notifyIfSameDatabaseCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.NotifyIfSameDatabase = notifyIfSameDatabaseCB.Checked;
        }

        private void ShowInstallCompleteWindow_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ShowInstallCompleteWindow = ShowInstallCompleteWindowCB.Checked;
        }
        #endregion

        #region Click events

        private void cancelDownloadButton_Click(object sender, EventArgs e)
        {
            downloader.CancelAsync();
        }

        private void viewAppUpdates_Click(object sender, EventArgs e)
        {
            int xloc = this.Location.X + this.Size.Width + 10;
            int yloc = this.Location.Y;
            using (ViewUpdates vu = new ViewUpdates(xloc, yloc, Settings.managerInfoDatFile, "releaseNotes.txt"))
            {
                vu.ShowDialog();
            }
        }

        private void viewDBUpdates_Click(object sender, EventArgs e)
        {
            int xloc = this.Location.X + this.Size.Width + 10;
            int yloc = this.Location.Y;
            using (ViewUpdates vu = new ViewUpdates(xloc, yloc, Settings.managerInfoDatFile, "databaseUpdate.txt"))
            {
                vu.ShowDialog();
            }
        }

        #endregion
    }
    #region DownloadItem class definition
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
    #endregion
}
