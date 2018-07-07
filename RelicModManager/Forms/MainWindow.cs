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
using System.Xml.XPath;
using System.Xml.Linq;
using RelhaxModpack.DatabaseComponents;
using RelhaxModpack.InstallerComponents;
using RelhaxModpack.Forms;
using RelhaxModpack.UIComponents;
using System.Runtime.InteropServices;

namespace RelhaxModpack
{
    public partial class MainWindow : Form
    {
        //all instance variables required to be up here
        private WebClient Downloader;
        private const int MBDivisor = 1048576;
        //sample:  C:/games/World_of_Tanks
        public string tanksLocation;
        //the location to pass into the installer
        private string tanksVersionForInstaller;
        //the folder where the user's app data is stored (C:\Users\username\AppData)
        private string appDataFolder;
        //the string representation from the xml document manager_version.xml. also passed into the installer for logging the version of the database installed at that time
        private string databaseVersionString;
        //timer to measure download speed
        private Stopwatch sw = new Stopwatch();
        // this dict will hold ALL directories and files of the backupFolder after parsing
        private List<BackupFolder> backupFolderContent;
        //The list of all mods
        private List<Category> parsedCatagoryLists;
        //queue for downloading mods
        private List<DatabasePackage> DatabasePackagesToDownload;
        //The ordered lists to install
        private List<Dependency> globalDependenciesToInstall;
        private List<Dependency> dependenciesToInstall;
        private List<LogicalDependency> logicalDependenciesToInstall;
        private List<SelectablePackage> modsConfigsToInstall;
        private List<Dependency> appendedDependenciesToInstall;
        private List<SelectablePackage> ModsWithShortcuts;
        private List<Shortcut> Shortcuts;
        private List<InstallGroup> InstallGroups;
        //list of all current dependencies
        private List<Dependency> currentDependencies;
        private List<LogicalDependency> currentLogicalDependencies;
        //list of patches
        private List<Patch> patchList;
        //list of all needed files from the current loaded modInfo file
        public static List<string> usedFilesList;
        //counter for Utils.exception calls
        public static int errorCounter = 0;
        private List<SelectablePackage> userMods;
        private string currentModDownloading;
        public Installer ins;
        private Installer unI;
        private string tanksVersion;//0.9.x.y
        private List<double> timeRemainArray;
        //the ETA variable for downlading
        private double actualTimeRemain = 0;
        private float previousTotalBytesDownloaded = 0;
        private float currentTotalBytesDownloaded = 0;
        private float differenceTotalBytesDownloaded = 0;
        private float sessionDownloadSpeed = 0;
        private int downloadCounter = -1;
        private object lockerMain = new object();
        private List<string> supportedVersions = new List<string>();
        private List<SelectablePackage> modsConfigsWithData;
        private float scale = 1.0f;
        //the previous settings for scaling
        private AutoScaleMode previousAutoScaleMode;
        private FontSize previousFontSize;
        private bool loading = false;
        private bool revertingScaling = false;

        //  interpret the created CiInfo buildTag as an "us-US" or a "de-DE" timeformat and return it as a local time- and dateformat string
        public static string CompileTime()//if getting build error, check windows date and time format settings https://puu.sh/xgCqO/e97e2e4a34.png
        {
            string date = CiInfo.BuildTag;
            if (Utils.ConvertDateToLocalCultureFormat(date, out date))
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
        public string ManagerVersion()
        {
            // string managerVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString().IndexOf('.') + 1);
            // if (Program.Version == Program.ProgramVersion.Beta)
            //    managerVersion = managerVersion + "_BETA_final";
            // return managerVersion;
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        //The constructur for the application
        public MainWindow()
        {
            Logging.Manager("MainWindow Constructed");
            InitializeComponent();
            this.SetStyle
              (                                      /// add double buffering and possibly reduce flicker https://stackoverflow.com/questions/1550293/stopping-textbox-flicker-during-update
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer, true
              );
            Logging.Manager("Style settings applied");
        }

        //parse the ModBackup folder to check current size
        private void ScanningRelHaxModBackupFolder()
        {
            using (BackgroundWorker worker = new BackgroundWorker())
            {
                worker.DoWork += ScanRelHaxModBackupFolder;
                worker.RunWorkerCompleted += OnModBackupFolderCompleted;
                worker.RunWorkerAsync();
            }
        }

        private void OnModBackupFolderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                //an exception occured!
                Logging.Manager(string.Format("Error at scanning '{0}' ({1})", Settings.RelHaxModBackupFolder, e.Error.Message));
            }
            else
            {

            }
        }

        /// <summary>
        /// this "calculation is very simple and not 100% correct. To safe time and speed up, the process is not checking the effectiv size on disk.
        /// more information about 0-size files with data see here:
        /// https://blogs.technet.microsoft.com/askcore/2009/10/16/the-four-stages-of-ntfs-file-growth/
        /// </summary>
        /// <returns></returns>
        private void ScanRelHaxModBackupFolder(object sender, DoWorkEventArgs args)
        {
            uint fileCount = 0;
            uint completeFileCount = 0;
            uint completeFileFolderCount = 0;
            uint BytesPerCluster = 1;
            UInt64 filesSize = 0;
            UInt64 filesSizeOnDisk = 0;
            UInt64 completeFolderSize = 0;
            UInt64 completeFolderSizeOnDisk = 0;

            BackupFolder bf;

            if (GetDiskFreeSpace(Settings.RelHaxModBackupFolder, out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters, out uint lpTotalNumberOfClusters))
            {
                BytesPerCluster = lpSectorsPerCluster * lpBytesPerSector;
                Logging.Manager("The cluster size was determined for drive " + Path.GetPathRoot(Settings.RelHaxModBackupFolder) + " (" + BytesPerCluster + " bytes)" );
            }
            else
            {
                Logging.Manager("failed to determine cluster size for drive " + Path.GetPathRoot(Settings.RelHaxModBackupFolder));
            }

            DirectoryInfo di = new DirectoryInfo(Settings.RelHaxModBackupFolder);
            backupFolderContent = new List<BackupFolder>();                         // this list will hold ALL directories and files after parsing
            List<DirectoryInfo> folderList = di.GetDirectories().ToList();          // parsed top folders
            foreach (var fL in folderList)
            {
                // search ModSelectionList Form. If found, user already startet the selection and the BackUpFolder cleanup will be not realy interesting
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.Name.Equals("ModSelectionList"))
                    {
                        this.backupModsCheckBox.Text = Translations.GetTranslatedString("backupModsCheckBox");
                        Logging.Manager("Scanning RelHaxModBackup folder stopped, because ModSelectionList is already started");
                        backupFolderContent = null;
                        return;
                    }
                }
                fileCount = 0;
                filesSize = 0;
                filesSizeOnDisk = 0;
                bf = new BackupFolder
                {
                    FullnameList = new List<string>(NumFilesToProcess(fL.FullName, ref fileCount, ref filesSize, ref filesSizeOnDisk, BytesPerCluster)),
                    TopfolderName = fL.FullName,
                    FileCount = fileCount,
                    FilesSize = filesSize,
                    FilesSizeOnDisk = filesSizeOnDisk
                };
                bf.FolderCount = (uint)bf.FullnameList.Count - fileCount;
                backupFolderContent.Add(bf);
                completeFileCount += fileCount;
                completeFileFolderCount += bf.FileCount + bf.FolderCount;
                completeFolderSize += filesSize;
                completeFolderSizeOnDisk += filesSizeOnDisk;
                this.backupModsCheckBox.Text = Translations.GetTranslatedString("backupModsCheckBox") + "\nBackups: " + backupFolderContent.Count + " Size: " + Utils.SizeSuffix(completeFolderSize, 2, true);
            }
            Logging.Manager(string.Format("parsed backups in BackupFolder: {0}, with a total size of {1} ({2} bytes) (files and folders: {3}).", backupFolderContent.Count, Utils.SizeSuffix(completeFolderSize, 2, true), completeFolderSize, completeFileFolderCount + backupFolderContent.Count));
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetDiskFreeSpace(string lpRootPathName,
           out uint lpSectorsPerCluster,
           out uint lpBytesPerSector,
           out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);

        private List<string> NumFilesToProcess(string folder, ref uint filesCount, ref UInt64 filesSize, ref UInt64 filesSizeOnDisk, uint BytesPerCluster = 1)
        {
            List<string> list = new List<string>();
            try
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(folder);
                // Get the files in the directory
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    list.Add(file.FullName);
                    filesCount++;
                    filesSize += (ulong)file.Length;
                    // calculate the effective size on disk of this file and add it
                    filesSizeOnDisk += (BytesPerCluster * (((ulong)file.Length + BytesPerCluster - 1) / BytesPerCluster));
                }
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    list.Add(subdir.FullName + @"\");
                    list.AddRange(NumFilesToProcess(subdir.FullName, ref filesCount, ref filesSize, ref filesSizeOnDisk, BytesPerCluster));
                }
            }
            catch (Exception ex)
            {
                Logging.Manager(string.Format("Error at scanning '{0}' ({1})", folder, ex.Message));
            }
            return list;
        }

        //handler for the mod download file progress
        void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (Settings.InstantExtraction)
                return;
            if (!DownloadTimer.Enabled)
                DownloadTimer.Enabled = true;
            string totalSpeedLabel = "";
            //get the download information into numeric classes
            float bytesIn = float.Parse(e.BytesReceived.ToString());
            float totalBytes = float.Parse(e.TotalBytesToReceive.ToString());
            float MBytesIn = (float)bytesIn / MBDivisor;
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
            double totalTimeToDownload = totalBytes / (e.BytesReceived / sw.Elapsed.TotalSeconds);
            double timeRemain = totalTimeToDownload - sw.Elapsed.TotalSeconds;
            if (timeRemain < 0)
            {
                timeRemain = 0;
            }
            if (timeRemainArray == null)
                timeRemainArray = new List<double>();
            timeRemainArray.Add(timeRemain);
            if (timeRemainArray.Count >= 10)
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
                Translations.GetTranslatedString("Downloading"), currentModDownloadingShort, Math.Round(MBytesIn, 1), Translations.GetTranslatedString("of"), Math.Round(totalBytes / MBDivisor, 1), totalSpeedLabel, actualTimeMins, actualTimeSecs);
            downloadProgress.Text = downloadStatus;
        }

        //handler for the mod download file complete event
        void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadTimer.Enabled = false;
            //check to see if the user cancled the download
            if (e != null && e.Cancelled)
            {
                //update the UI and download state
                ToggleUIButtons(true);
                downloadProgress.Text = Translations.GetTranslatedString("idle");
                parrentProgressBar.Value = 0;
                childProgressBar.Value = 0;
                return;
            }
            //i think a complete download means that error is null, if error is ever not null this will catch it and we can log it
            else if (e != null && e.Error != null)
            {
                AsyncDownloadArgs userToken = (AsyncDownloadArgs)e.UserState;
                if (Program.testMode)
                    Utils.ExceptionLog("downloader_DownloadFileCompleted", "downloaded file: " + userToken.url.ToString(), e.Error);
                else
                    Utils.ExceptionLog("downloader_DownloadFileCompleted", "downloaded file: " + Path.GetFileName(userToken.zipFile.ToString()), e.Error);
                DialogResult result = MessageBox.Show(string.Format("{0}\n{1}\n\n{2}", Translations.GetTranslatedString("failed_To_Download_1"), Path.GetFileName(userToken.zipFile.ToString()),
                    Translations.GetTranslatedString("failed_To_Download_2")),"critical",MessageBoxButtons.AbortRetryIgnore,MessageBoxIcon.Question);
                if (result == DialogResult.Abort)
                {
                    Application.Exit();
                }
                else if (result == DialogResult.Retry)
                {
                    Downloader.DownloadFileAsync(userToken.url, userToken.zipFile, e.UserState);
                    return;
                }
            }
            //at this point we're here for NOT an error and NOT a cancel
            downloadCounter++;
            if (DatabasePackagesToDownload.Count != downloadCounter)
            {
                //downloader components
                AsyncDownloadArgs args = new AsyncDownloadArgs
                {
                    //args.url = new Uri(Utils.ReplaceMacro(DatabasePackagesToDownload[downloadCounter].StartAddress) + DatabasePackagesToDownload[downloadCounter].ZipFile + DatabasePackagesToDownload[downloadCounter].EndAddress);
                    url = new Uri(DatabasePackagesToDownload[downloadCounter].StartAddress.Replace(@"{onlineFolder}", Settings.TanksOnlineFolderVersion)
                    + DatabasePackagesToDownload[downloadCounter].ZipFile
                    + DatabasePackagesToDownload[downloadCounter].EndAddress),
                    zipFile = Path.Combine(Settings.RelhaxDownloadsFolder, DatabasePackagesToDownload[downloadCounter].ZipFile)
                };
                //for the next file in the queue, delete it.
                if (File.Exists(args.zipFile)) File.Delete(args.zipFile);
                //download new zip file
                if (Downloader != null)
                    Downloader.Dispose();
                Downloader = new WebClient();
                Downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                Downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                //Downloader.Proxy = null;
                Downloader.DownloadFileAsync(args.url, args.zipFile, args);
                Logging.Manager("downloading " + Path.GetFileName(args.zipFile));
                //UI components
                if(!Settings.InstantExtraction)
                {
                    totalProgressBar.Maximum = (int)InstallerEventArgs.InstallProgress.Done;
                    totalProgressBar.Value = 1;//backup technically, but don't worry about it (for now)
                    cancelDownloadButton.Enabled = true;
                    cancelDownloadButton.Visible = true;
                    DownloadTimer.Enabled = true;
                    if (timeRemainArray == null)
                        timeRemainArray = new List<double>();
                    timeRemainArray.Clear();
                    actualTimeRemain = 0;
                    sw.Reset();
                    sw.Start();
                    currentModDownloading = Path.GetFileNameWithoutExtension(args.zipFile);
                    if ((parrentProgressBar.Value + 1) <= parrentProgressBar.Maximum)
                        parrentProgressBar.Value++;
                }
                //locking system for stopping data and access races
                if (downloadCounter!=0)
                {
                    lock (lockerMain)
                    {
                        DatabasePackagesToDownload[downloadCounter - 1].ReadyForInstall = true;
                    }
                }
            }
            if (DatabasePackagesToDownload.Count == downloadCounter || Settings.InstantExtraction)
            {
                cancelDownloadButton.Enabled = false;
                cancelDownloadButton.Visible = false;
                if (ins == null)
                {
                    ins = new Installer()
                    {
                        // AppPath = Application.StartupPath,
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
                        DatabaseVersion = this.databaseVersionString,
                        Shortcuts = this.Shortcuts,
                        InstallGroups = this.InstallGroups,
                        TotalCategories = this.parsedCatagoryLists.Count
                    };
                    ins.InstallProgressChanged += I_InstallProgressChanged;
                    ins.StartInstallation();
                }
                if(DatabasePackagesToDownload.Count == downloadCounter && Settings.InstantExtraction)
                {
                    //locking system for stopping data and access races
                    if (downloadCounter != 0)
                    {
                        lock (lockerMain)
                        {
                            DatabasePackagesToDownload[downloadCounter - 1].ReadyForInstall = true;
                        }
                    }
                }
            }
        }

        //method to check for updates to the application on startup
        public void CheckmanagerUpdates()
        {
            Logging.Manager("Starting check for application updates");
            //download the updates
            WebClient updater = new WebClient();
            //updater.Proxy = null;
            if (File.Exists(Settings.ManagerInfoDatFile))
                File.Delete(Settings.ManagerInfoDatFile);
            try
            {
                updater.DownloadFile("http://wotmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat", Settings.ManagerInfoDatFile);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("checkmanagerUpdates", @"Tried to access http://wotmods.relhaxmodpack.com/RelhaxModpack/managerInfo.dat", ex);
                MessageBox.Show(string.Format("{0} managerInfo.dat", Translations.GetTranslatedString("failed_To_Download_1")));
                Application.Exit();
            }
            if (Program.skipUpdate && !Program.testMode)
            {
                MessageBox.Show(Translations.GetTranslatedString("skipUpdateWarning"));
            }
            // string version = "";
            string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");
            if (!xmlString.Equals(""))
            {
                XDocument doc = XDocument.Parse(xmlString);

                //parse the database version
                Settings.DatabaseVersion = doc.XPathSelectElement("//version/database").Value;
                DatabaseVersionLabel.Text = Translations.GetTranslatedString("DatabaseVersionLabel") + " v" + Settings.DatabaseVersion;

                string version = "";
                bool startUpdate = false;
                bool tempManagerVersionStable = false;
                bool tempManagerVersionBeta = false;
                //parse the manager version
                string onlineManager = doc.XPathSelectElement("//version/manager_v2").Value;
                string onlineManagerBeta = doc.XPathSelectElement("//version/manager_beta_v2").Value;
                
                // check if current alpha version number is valid. Only valid is the alpha version number is higher then stable and beta version
                if (Program.Version == Program.ProgramVersion.Alpha)
                {
                    // check if online beta is not higher then online stable version
                    if (Utils.CompareVersions(onlineManager, onlineManagerBeta) != -1)
                    {
                        // check if local alpha is not higher then online stable version
                        if (Utils.CompareVersions(ManagerVersion(), onlineManager) != 1)
                        {
                            version = onlineManager + " (stable)";
                            tempManagerVersionStable = true;
                            tempManagerVersionBeta = false;
                            startUpdate = true;
                        }
                    }
                    else
                    {
                        // check if local alpha is not higher then online beta version
                        if (Utils.CompareVersions(ManagerVersion(), onlineManagerBeta) != 1)
                        {
                            version = onlineManagerBeta + " (beta)";
                            tempManagerVersionBeta = true;
                            startUpdate = true;
                        }
                    }
                }
                
                // check if current beta version is still higher then onlineManager (not beta) version. that means is "updat-to-date"
                if ((Program.Version == Program.ProgramVersion.Beta || Settings.BetaApplication) && Utils.CompareVersions(onlineManager, onlineManagerBeta) == -1)
                {
                    version = onlineManager + " (beta)";
                    // check if online beta version is higher then local beta version, then update
                    if (Utils.CompareVersions(ManagerVersion(), onlineManagerBeta) == -1)
                    {
                        startUpdate = true;         // update
                    }
                    
                    // check if the online version (not beta) is higher then the local beta version, then update to stable channel
                    if (Utils.CompareVersions(ManagerVersion(), onlineManager) == -1)
                    {
                        version = onlineManager + " (stable)";
                        tempManagerVersionStable = true;
                        startUpdate = true;
                    }
                }
                // check if online beta version is outdated and online stable version (not beta) must be used (beta and stable version with same version number = beta is outdated)
                else if ((Program.Version == Program.ProgramVersion.Beta || Settings.BetaApplication) && Utils.CompareVersions(onlineManager, onlineManagerBeta) != -1)
                {
                    // beta is outdated and current stable must be used
                    version = onlineManager + " (stable)";
                    tempManagerVersionStable = true;
                    startUpdate = true;
                }

                // if stable online Manager is higher then local, then update
                if (Program.Version == Program.ProgramVersion.Stable && Utils.CompareVersions(ManagerVersion(), onlineManager) == -1)
                {
                    version = onlineManager + " (stable)";
                    startUpdate = true;
                }

                if (Program.Version == Program.ProgramVersion.Alpha && !startUpdate)
                    Logging.Manager(string.Format("Local application is {0} alpha and no stable ({1}) or beta ({2}) version is newer", ManagerVersion(), onlineManager, onlineManagerBeta));
                else
                    Logging.Manager(string.Format("Local application is {0} ({1}), current online is {2}", ManagerVersion(), Program.Version == Program.ProgramVersion.Beta ? "beta" : Program.Version == Program.ProgramVersion.Alpha ? "alpha" : "stable", version));

                if (!Program.skipUpdate && startUpdate)
                {
                    Logging.Manager("exe is out of date. displaying user update window");
                    //out of date
                    VersionInfo vi = new VersionInfo();
                    vi.ShowDialog();
                    DialogResult result = vi.DialogResult;
                    if (result.Equals(DialogResult.Yes))
                    {
                        Logging.Manager("User accepted downloading new version");
                        //download new version
                        sw.Reset();
                        sw.Start();
                        string newExeName = Settings.UseAlternateUpdateMethod? Path.Combine(Application.StartupPath, "RelhaxModpack_update.zip"):Path.Combine(Application.StartupPath, "RelhaxModpack_update.exe");
                        updater.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Downloader_DownloadProgressChanged);
                        updater.DownloadFileCompleted += new AsyncCompletedEventHandler(Updater_DownloadFileCompleted);

                        // check if manager instance is already running and ask user to close it
                        // bool loop = true;
                        // while (loop)
                        while (true)
                        {
                            System.Threading.Thread.Sleep(500);
                            // loop = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1;
                            // if (loop)
                            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                            {
                                result = MessageBox.Show(Translations.GetTranslatedString("closeInstanceRunningForUpdate"), Translations.GetTranslatedString("critical"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Stop);
                                if (result == DialogResult.Cancel)
                                {
                                    Logging.Manager("User canceled update, because he does not want to end the parallel running Relhax instance.");
                                    Application.Exit();
                                    return;
                                }
                            }
                            else
                                break;
                        }
                        //check if the application is windowState normal
                        if (WindowState != FormWindowState.Normal)
                            WindowState = FormWindowState.Normal;

                        if (File.Exists(newExeName)) File.Delete(newExeName);
                        //using new attemp at an update method. Application now downloads a zip file of itself, rather than an exe. Maybe it will help antivirus issues
                        string modpackExeURL = null;
                        if(Settings.UseAlternateUpdateMethod)
                            modpackExeURL = (Settings.BetaApplication || tempManagerVersionBeta) && !tempManagerVersionStable ? "http://wotmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpackBeta.zip" : "http://wotmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.zip";
                        else
                            modpackExeURL = (Settings.BetaApplication || tempManagerVersionBeta) && !tempManagerVersionStable ? "http://wotmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpackBeta.exe" : "http://wotmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.exe";
                        updater.DownloadFileAsync(new Uri(modpackExeURL), newExeName);
                        Logging.Manager("New application download started, UseAlternateUpdateMethod=" + Settings.UseAlternateUpdateMethod);
                        currentModDownloading = "update ";
                    }
                    else
                    {
                        Logging.Manager("User declined downloading new version");
                        //close the application
                        Application.Exit();
                    }
                }
            }
            else
            {
                Logging.Manager("ERROR. Failed to get 'manager_version.xml'");
                MessageBox.Show(Translations.GetTranslatedString("failedManager_version"), Translations.GetTranslatedString("critical"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //close the application
                this.Close();
            }
        }

        //handler for when the update download is complete
        void Updater_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadTimer.Enabled = false;
            if (e.Error != null && e.Error.Message.Equals("The remote server returned an error: (404) Not Found."))
            {
                //404
                Logging.Manager("ERROR: unable to download new application version");
                MessageBox.Show(Translations.GetTranslatedString("cantDownloadNewVersion"));
                this.Close();
            }

            //try to extract the zipfile (if new update method)
            try
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Path.Combine(Application.StartupPath, "RelhaxModpack_update.zip")))
                {
                    zip.ExtractAll(Application.StartupPath);
                }
            }
            catch(Ionic.Zip.ZipException ezip)
            {
                //first, log it
                Utils.ExceptionLog(ezip);
                if(MessageBox.Show("Error extracting update, restart and try using normal update option?","error extraction update",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Settings.UseAlternateUpdateMethod = false;
                    //https://msdn.microsoft.com/en-us/library/system.windows.forms.application.restart(v=vs.110).aspx
                    //"If your application was originally supplied command-line options when it first executed, Restart() will launch the application again with the same options."
                    Application.Restart();
                }
                else
                {
                    Application.Exit();
                }
            }

            // part/idea of a new batch script if previous download/update failed https://stackoverflow.com/questions/4619088/windows-batch-file-file-download-from-a-url
            string newBatName = Path.Combine(Application.StartupPath, "RelicCopyUpdate.bat");
            try
            {
                //write batch file to text
                string updateScript = "";
                updateScript = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "RelicCopyUpdate.txt");
                File.WriteAllText(newBatName, updateScript);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("updater_DownloadFileCompleted", "create RelicCopyUpdate.bat failed", ex);
                string msgTxt = string.Format(Translations.GetTranslatedString("failedCreateUpdateBat"), Path.Combine(Application.StartupPath, "RelhaxModpack.exe"), "RelhaxModpack_update.exe", "RelhaxModpack.exe");
                if (DialogResult.Yes == MessageBox.Show(msgTxt, Translations.GetTranslatedString("critical"), MessageBoxButtons.YesNo, MessageBoxIcon.Stop))
                {
                    // call the windows explorer and open at the relhax folder
                    ProcessStartInfo explorer = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = Application.StartupPath
                    };
                    Process callExplorer = new Process
                    {
                        StartInfo = explorer
                    };
                    callExplorer.Start();
                }
                try
                {
                    // try to create a textfile at the temp folder
                    string howToPath = Path.Combine(Path.GetTempPath(), "howTo.txt");
                    File.WriteAllText(howToPath, msgTxt);
                    // call the notepad and open the howto.txt file
                    ProcessStartInfo notepad = new ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = howToPath
                    };
                    Process callNotepad = new Process
                    {
                        StartInfo = notepad
                    };
                    callNotepad.Start();
                }
                catch (Exception e2)
                {
                    Utils.ExceptionLog("updater_DownloadFileCompleted", "failed to create howTo.txt", e2);
                }
                Application.Exit();
            }

            try
            {
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = newBatName,
                    Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray())
                };
                Process installUpdate = new Process
                {
                    StartInfo = info
                };
                installUpdate.Start();
            }
            catch (Exception e3)
            {
                Utils.ExceptionLog("updater_DownloadFileCompleted", "could not start new application version", e3);
                MessageBox.Show(Translations.GetTranslatedString("cantStartNewApp") + newBatName);
            }
            Application.Exit();
        }

        //gets the version of tanks that this is, in the format
        //of the res_mods version folder i.e. 0.9.17.0.3
        private string GetFolderVersion()
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
        private bool IsClientVersionSupported(string detectedVersion)
        {
            supportedVersions.Clear();
            string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "supported_clients.xml");  //xml doc name can change
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
                Settings.TanksOnlineFolderVersion = element.Attribute("folder").Value;
            }
            else
            {
                // store the the last onlinefolder version to the string, if no valid detectedVersion association is found
                Settings.TanksOnlineFolderVersion = doc.Descendants("version").Last().Attribute("folder").Value;
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

        #region Tanks Install Auto/Manuel Search Code
        //checks the registry to get the location of where WoT is installed
        private string AutoFindTanks()
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
                    Logging.Manager(string.Format("valid game path found: {0}", path));
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
        public string ManuallyFindTanks()
        {
            // try to give an untrained user a littlebit support
            if (AutoFindTanks() != null)
            {
                FindWotExe.InitialDirectory = Path.GetDirectoryName(tanksLocation);
            }
            //unable to find it in the registry (or user activated manually selection), so ask for it
            if (FindWotExe.ShowDialog().Equals(DialogResult.Cancel))
            {
                downloadProgress.Text = Translations.GetTranslatedString("canceled");
                return null;
            }
            tanksLocation = FindWotExe.FileName;
            return "all good";
        }
        #endregion

        //handler for before the window is displayed
        private void MainWindow_Load(object sender, EventArgs e)
        {
            loading = true;
            Logging.Manager(string.Format("|RelHax Modpack {0} ({1})", ManagerVersion(), Program.Version == Program.ProgramVersion.Beta ? "beta" : Program.Version == Program.ProgramVersion.Alpha ? "alpha" : "stable"));
            Logging.Manager(string.Format("|Built on {0}", CompileTime()));
            Logging.Manager(string.Format("|Running on {0}", System.Environment.OSVersion.ToString()));
            //show the wait screen
            PleaseWait wait = new PleaseWait();
            if(!Program.silentStart)
                wait.Show();
            Application.DoEvents();

            //directory structure
            wait.loadingDescBox.Text = Translations.GetTranslatedString("verDirStructure");
            Application.DoEvents();
            Logging.Manager("Verifying File and Folder Structure");
            //create directory structures
            try
            {
                string[] foldersToCreate = { "RelHaxDownloads", "RelHaxUserMods", "RelHaxModBackup", "RelHaxUserConfigs", "RelHaxTemp" };
                foreach (string s in foldersToCreate)
                {
                    string folder = Path.Combine(Application.StartupPath, s);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                }
                //check if old dll files can be deleted
                string[] filesToDelete = { "DotNetZip.dll", "Ionic.Zip.dll", "Newtonsoft.Json.dll", "NAudio.dll", "nvtt32.dll", "nvtt64.dll", "FreeImage32.dll", "FreeImage64.dll", "TeximpNet.dll", "TeximpNet.xml" };
                foreach (string s in filesToDelete)
                    if (File.Exists(Path.Combine(Application.StartupPath, s)))
                        File.Delete(Path.Combine(Application.StartupPath, s));
            }
            catch (Exception ex2)
            {
                Utils.ExceptionLog(ex2);
            }

            //load settings
            wait.loadingDescBox.Text = Translations.GetTranslatedString("loadingSettings");
            Logging.Manager("Loading settings");
            Settings.LoadSettings();
            ApplyControlTranslations(Controls);
            ApplySettings();
            loading = false;

            //check for updates
            wait.loadingDescBox.Text = Translations.GetTranslatedString("checkForUpdates");
            Application.DoEvents();
            this.CheckmanagerUpdates();

            //add ability to disable the modpack for during patch day
            //this will involve having a hard coded true or false, along with a command line arguement to over-ride
            //to disable from patch day set it to false.
            //to enable for patch day (prevent users to use it), set it to true.
            //hopefully this will never have to be used
            if (false && !Program.patchDayTest)
            {
                Logging.Manager("Patch day disable detected. Remember To override use /patchday");
                MessageBox.Show(Translations.GetTranslatedString("patchDayMessage"));
                Application.Exit();
            }

            //apply translations for menu
            MenuItemAppClose.Text = Translations.GetTranslatedString(MenuItemAppClose.Name);
            MenuItemRestore.Text = Translations.GetTranslatedString(MenuItemRestore.Name);
            MenuItemCheckUpdates.Text = Translations.GetTranslatedString(MenuItemCheckUpdates.Name);

            //parsing command line arguements
            //first check for arguement conflicts
            if (Settings.BetaDatabase && Program.testMode)
            {
                if(MessageBox.Show(Translations.GetTranslatedString("conflictBetaDBTestMode"), Translations.GetTranslatedString("conflictsCommandlineHeader"), MessageBoxButtons.OKCancel,MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    Application.Exit();
                }
            }
            //next parse them
            if (Program.testMode)
            {
                Logging.Manager("Test Mode is ON, loading local modInfo.xml");
            }
            if (Program.autoInstall)
            {
                Logging.Manager("Auto Install is ON, checking for config pref xml at " + Path.Combine(Application.StartupPath, "RelHaxUserConfigs", Program.configName));
                if (!File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserConfigs", Program.configName)))
                {
                    Logging.Manager(string.Format("ERROR: {0} does NOT exist, loading in regular mode", Program.configName));
                    MessageBox.Show(string.Format(Translations.GetTranslatedString("configNotExist"), Program.configName));
                    Program.autoInstall = false;
                }
                if (!Settings.CleanInstallation)
                {
                    Logging.Manager("ERROR: clean installation is set to false. This must be set to true for auto install to work. Loading in regular mode.");
                    MessageBox.Show(Translations.GetTranslatedString("autoAndClean"));
                    Program.autoInstall = false;
                }
                if (Settings.FirstLoad)
                {
                    Logging.Manager("ERROR: First time loading cannot be an auto install mode, loading in regular mode");
                    MessageBox.Show(Translations.GetTranslatedString("autoAndFirst"));
                    Program.autoInstall = false;
                }
            }
            //check if it can still load in autoInstall config mode
            if (Program.autoInstall)
            {
                Logging.Manager("Program.autoInstall still true, loading in auto install mode");
                wait.Close();
                this.InstallRelhaxMod_Click(null, null);
                return;
            }

            //apply text labels and custom command line properties
            ApplyVersionTextLabels();

            //scan Backupfolder and show it on MainForm
            if (Program.testMode)
                ScanningRelHaxModBackupFolder();

            if (Settings.FirstLoad)
            {
                //set the textbox to show the intro help message
                Generic_MouseLeave(null, null);
            }
            if (!Program.silentStart)
                wait.Close();
            ToggleUIButtons(true);
            Application.DoEvents();
            Program.saveSettings = true;
        }

        //applies the additional version information for the database and application
        private void ApplyVersionTextLabels()
        {
            //aplication version (bottom left)
            ApplicationVersionLabel.Text = string.Format("{0} v{1} {2}", Translations.GetTranslatedString("ApplicationVersionLabel"), ManagerVersion(), Program.Version == Program.ProgramVersion.Beta ? " (beta)" : Program.Version == Program.ProgramVersion.Alpha ? " (alpha)" : "");
            //database version (bottom right)
            DatabaseVersionLabel.Text = Translations.GetTranslatedString("DatabaseVersionLabel") + " v" + Settings.DatabaseVersion;
            //The title of the main form
            Text = "Relhax";
            if (Program.testMode)
                Text = Text + " TEST MODE";
            if (Settings.BetaDatabase)
                Text = Text + " (BETA DB)";
            if (Program.Version == Program.ProgramVersion.Beta)
                Text = Text + " (BETA APP)";
            else if (Program.Version == Program.ProgramVersion.Alpha)
                Text = Text + " (ALPHA APP)";
        }

        //handler for when the install relhax modpack button is pressed
        //basicly the entire install process
        private void InstallRelhaxMod_Click(object sender, EventArgs e)
        {
            Utils.TotallyNotStatPaddingForumPageViewCount();
            ToggleUIButtons(false);
            //reset progress bars
            parrentProgressBar.Value = parrentProgressBar.Minimum;
            totalProgressBar.Value = totalProgressBar.Minimum;
            childProgressBar.Value = childProgressBar.Minimum;
            //get the user appData folder
            appDataFolder = "";
            appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wargaming.net", "WorldOfTanks");
            Logging.Manager("appDataFolder parsed as " + appDataFolder);
            if (string.IsNullOrWhiteSpace(appDataFolder) || !Directory.Exists(appDataFolder))
            {
                Logging.Manager("ERROR: appDataFolder does not exist");
                appDataFolder = "-1";
                if (Settings.ClearCache)
                {
                    //can't locate folder, continue installation anyway?
                    DialogResult clearCacheFailResult = MessageBox.Show(Translations.GetTranslatedString("appDataFolderNotExist"), Translations.GetTranslatedString("appDataFolderNotExistHeader"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (clearCacheFailResult == DialogResult.No)
                    {
                        Logging.Manager("user stopped installation");
                        ToggleUIButtons(true);
                        return;
                    }
                }
            }
            //reset the interface
            this.downloadProgress.Text = "";
            //if export mode, show a different selection method
            if(Settings.ExportMode)
            {
                if(Program.Version != Program.ProgramVersion.Stable)
                    Logging.Manager("DEBUG: export mode selected, asking where to set folder");
                if(string.IsNullOrWhiteSpace(ExportModeBrowserDialog.SelectedPath))
                    ExportModeBrowserDialog.SelectedPath = Application.StartupPath;
                if (ExportModeBrowserDialog.ShowDialog() != DialogResult.OK)
                {
                    if (Program.Version != Program.ProgramVersion.Stable)
                        Logging.Manager("DEBUG: user canceled export mode");
                    ToggleUIButtons(true);
                    return;
                }
                if (Program.Version != Program.ProgramVersion.Stable)
                    Logging.Manager("DEBUG: user continues to select version of game to export for");
                //create the list of radioButtons
                List<ExportModeRadioButton> supportedClients = new List<ExportModeRadioButton>();
                //parse the list of supportd clients
                supportedVersions.Clear();
                string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "supported_clients.xml");  //xml doc name can change
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString);
                foreach(XmlNode node in doc.SelectNodes("//versions/version"))
                {
                    supportedClients.Add(new ExportModeRadioButton()
                    {
                        Text = node.InnerText,
                        FolderVersion = node.InnerText,
                        OnlineFolderVersion = node.Attributes["folder"].InnerText,
                        AutoSize = true,
                    });
                    // fill the supportedVersions array to possible create messages
                    supportedVersions.Add(node.InnerText);
                }
                //check if the selection is valid
                using (ExportSelectWoTVersion wotv = new ExportSelectWoTVersion()
                {
                    SupportedWoTVersions = supportedClients
                })
                {
                    if(wotv.ShowDialog() != DialogResult.OK)
                    {
                        if (Program.Version != Program.ProgramVersion.Stable)
                            Logging.Manager("DEBUG: user canceled export mode");
                        ToggleUIButtons(true);
                        return;
                    }
                    tanksLocation = ExportModeBrowserDialog.SelectedPath;
                    Settings.TanksLocation = tanksLocation;
                    Logging.Manager("tanksLocation parsed as " + tanksLocation);
                    Logging.Manager("customUserMods parsed as " + Path.Combine(Application.StartupPath, "RelHaxUserMods"));
                    tanksVersion = wotv.selectedVersion.FolderVersion;
                    tanksVersionForInstaller = tanksVersion;
                    Settings.TanksVersion = tanksVersion;
                    Settings.TanksOnlineFolderVersion = wotv.selectedVersion.OnlineFolderVersion;
                    Logging.Manager("tanksVersion parsed as " + tanksVersion);
                }
            }
            else
            {
                //attempt to locate the tanks directory automatically
                //if it fails, it will prompt the user to return the world of tanks exe
                if (Settings.ForceManuel || this.AutoFindTanks() == null)
                {
                    if (this.ManuallyFindTanks() == null)
                    {
                        Logging.Manager("user stopped installation");
                        ToggleUIButtons(true);
                        return;
                    }
                }
                //parse all strings for installation
                tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
                Settings.TanksLocation = tanksLocation;
                Logging.Manager("tanksLocation parsed as " + tanksLocation);
                Logging.Manager("customUserMods parsed as " + Path.Combine(Application.StartupPath, "RelHaxUserMods"));
                if (tanksLocation.Equals(Application.StartupPath))
                {
                    //display error and abort
                    MessageBox.Show(Translations.GetTranslatedString("moveOutOfTanksLocation"));
                    ToggleUIButtons(true);
                    return;
                }
                tanksVersion = this.GetFolderVersion();
                tanksVersionForInstaller = tanksVersion;
                Settings.TanksVersion = tanksVersion;
                Logging.Manager("tanksVersion parsed as " + tanksVersion);
                //determine if the tanks client version is supported
                if (!Program.testMode && !IsClientVersionSupported(tanksVersion))
                {
                    //log and inform the user
                    Logging.Manager("WARNING: Detected client version is " + tanksVersion + ", not supported");
                    Logging.Manager("Supported versions are: " + string.Join(", ", supportedVersions));
                    // parse the string that we get from the server and delete all "Testserver" entries (Testserver entries are the version number with prefix "T")
                    string publicVersions = string.Join("\n", supportedVersions.Select(sValue => sValue.Trim()).ToArray().Where(s => !(s.Substring(0, 1) == "T")).ToArray());
                    MessageBox.Show(string.Format("{0}: {1}\n{2}\n\n{3}:\n{4}", Translations.GetTranslatedString("detectedClientVersion"), tanksVersion, Translations.GetTranslatedString("supportNotGuarnteed"), Translations.GetTranslatedString("supportedClientVersions"), publicVersions), Translations.GetTranslatedString("critical"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // select the last public modpack version
                    tanksVersion = publicVersions.Split('\n').Last();
                    // go to Client check again, because the online folder must be set correct
                    IsClientVersionSupported(tanksVersion);
                    Logging.Manager(string.Format("Version selected: {0}  OnlineFolder: {1}", tanksVersion, Settings.TanksOnlineFolderVersion));
                }
                //if the user wants to, check if the database has actually changed
                if (Settings.NotifyIfSameDatabase && SameDatabaseVersions())        // the get the string databaseVersionString filles in any case, the function must be performed first!
                {
                    if (MessageBox.Show(Translations.GetTranslatedString("DatabaseVersionsSameBody"), Translations.GetTranslatedString("DatabaseVersionsSameHeader"), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        ToggleUIButtons(true);
                        return;
                    }
                }
            }
            //reset the childProgressBar value
            childProgressBar.Maximum = 100;
            //childProgressBar.Value = 0;
            //check to make sure that the md5hashdatabase is valid before using it
            Logging.Manager("Checking md5 database file");
            if ((File.Exists(Settings.MD5HashDatabaseXmlFile)) && (!XMLUtils.IsValidXml(Settings.MD5HashDatabaseXmlFile)))
            {
                File.Delete(Settings.MD5HashDatabaseXmlFile);
            }
            //show the mod selection window
            Logging.Manager("Loading ModSelectionList");
            ModSelectionList list = new ModSelectionList()
            {
                TanksVersion = this.tanksVersion,
                TanksLocation = this.tanksLocation,
                MainWindowStartX = this.Location.X + this.Size.Width,
                MainWindowStartY = this.Location.Y
            };
            if (list.ShowDialog() != DialogResult.OK)
            {
                try
                {
                    list.Dispose();
                }
                catch
                {
                    Logging.Manager("INFO: Failed to dispose list");
                }
                list = null;
                GC.Collect();
                ToggleUIButtons(true);
                return;
            }
            //check to see if WoT is running
            bool WoTRunning = true;
            int retryCount = 0;
            while (WoTRunning)
            {
                try
                {
                    WoTRunning = false;
                    foreach (Process p in Process.GetProcesses())
                    {
                        if (p.ProcessName.ToLower().Equals("WorldOfTanks".ToLower()))
                        {
                            if (p.MainModule.FileName.ToLower().Equals(Path.Combine(tanksLocation, "worldoftanks.exe").ToLower()))
                            {
                                WoTRunning = true;
                                MessageBox.Show(Translations.GetTranslatedString("WoTRunningMessage"), Translations.GetTranslatedString("WoTRunningHeader"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                retryCount++;
                                if (retryCount > 5)
                                {
                                    if (MessageBox.Show(string.Format(Translations.GetTranslatedString("KillRunningWoTMessage"), retryCount), Translations.GetTranslatedString("KillRunningWoTHeader"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                                    {
                                        try
                                        {
                                            p.Kill();
                                            int waitingExited = 0;
                                            while (p.HasExited)
                                            {
                                                System.Threading.Thread.Sleep(5);
                                                waitingExited++;
                                                // the time to wait for finishing/killing the taask is 5 sec
                                                if (waitingExited > 1000) break;
                                            }
                                            Logging.Manager("Successfully killed " + p.ProcessName + " Id: " + p.Id);
                                        }
                                        catch
                                        {
                                            Logging.Manager("Failed to kill " + p.ProcessName + " Id: " + p.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!WoTRunning)
                        break;
                }
                catch { }
            }
            //have the application display that it is loading. it is actually doing installation calculations
            downloadProgress.Text = Translations.GetTranslatedString("loading");
            Application.DoEvents();
            Utils.BuildMacroHash();
            //run the installer calculations
            ProcessInstallCalculations(list);
        }

        #region Installer calculations
        private void ProcessInstallCalculations(ModSelectionList list)
        {

            /*
             * parses all the mods and configs into seperate lists for many types op options
             * like mods/configs to install, mods/configs with data, and others
            */
            //copies it instead
            currentDependencies = new List<Dependency>(list.Dependencies);
            currentLogicalDependencies = new List<LogicalDependency>(list.LogicalDependencies);
            parsedCatagoryLists = new List<Category>(list.ParsedCatagoryList);
            globalDependenciesToInstall = new List<Dependency>(list.GlobalDependencies);
            dependenciesToInstall = new List<Dependency>();
            logicalDependenciesToInstall = new List<LogicalDependency>();
            modsConfigsToInstall = new List<SelectablePackage>();
            appendedDependenciesToInstall = new List<Dependency>();
            modsConfigsWithData = new List<SelectablePackage>();
            patchList = new List<Patch>();
            userMods = new List<SelectablePackage>();
            ModsWithShortcuts = new List<SelectablePackage>();
            Shortcuts = new List<Shortcut>();
            DatabasePackagesToDownload = new List<DatabasePackage>();
            InstallGroups = new List<InstallGroup>();

            //code for super extraction mode. seperates the categories into installer groups.
            int installGroupCounter = 0;
            if(Settings.SuperExtraction)
            {
                foreach(Category c in parsedCatagoryLists)
                {
                    if(c.InstallGroup == installGroupCounter)
                    {
                        if (InstallGroups.Count == installGroupCounter)
                            InstallGroups.Add(new InstallGroup());
                        InstallGroups[installGroupCounter].Categories.Add(c);
                    }
                    else
                    {
                        while (installGroupCounter != c.InstallGroup)
                            installGroupCounter++;
                        if (InstallGroups.Count == installGroupCounter)
                            InstallGroups.Add(new InstallGroup());
                        InstallGroups[installGroupCounter].Categories.Add(c);
                    }
                }
            }

            try
            {
                //if mod/config is Enabled and checked, add it to list of mods to extract/install
                foreach (Category c in parsedCatagoryLists)
                {
                    //will itterate through every catagory once
                    foreach (SelectablePackage m in c.Packages)
                    {
                        //will itterate through every mod of every catagory once
                        if (m.Enabled && m.Checked)
                        {
                            //move each mod that is enalbed and checked to a new list of mods to install
                            //also check that it actually has a zip file
                            if (!m.ZipFile.Equals(""))
                            {
                                modsConfigsToInstall.Add(m);
                            }

                            //since it is checked, regardless if it has a zipfile, check if it has userdata
                            if (m.UserFiles.Count > 0)
                                modsConfigsWithData.Add(m);

                            //if it has shortcuts to create, add them to the list here
                            if (m.Shortcuts.Count > 0)
                                ModsWithShortcuts.Add(m);

                            //check for configs
                            if (m.Packages.Count > 0)
                                ProcessConfigs(m.Packages);

                            //at least one mod of this catagory is checked, add any dependenciesToInstall required
                            if (c.Dependencies.Count > 0)
                                ProcessDependencies(c.Dependencies);

                            //check dependency is Enabled and has a zip file with it
                            if (m.Dependencies.Count > 0)
                                ProcessDependencies(m.Dependencies);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("installRelhaxMod_Click", "if mod/config is Enabled and checked, add it to list of mods to extract/install", ex);
            }

            try
            {
                //build the list of mods and configs that use each logical dependency
                foreach (LogicalDependency d in currentLogicalDependencies)
                {
                    foreach (Dependency depD in currentDependencies)
                    {
                        foreach (LogicalDependency ld in depD.LogicalDependencies)
                        {
                            if (ld.PackageName.Equals(d.PackageName))
                            {
                                DatabaseLogic dbl = new DatabaseLogic()
                                {
                                    PackageName = depD.PackageName,
                                    Enabled = depD.Enabled,
                                    Checked = dependenciesToInstall.Contains(depD),
                                    NotFlag = ld.NegateFlag
                                };
                                d.DatabasePackageLogic.Add(dbl);
                            }
                        }
                    }
                    try
                    {
                        //itterate through every mod and config once for each dependency
                        //check each one's dependecy list, if PackageName's match, add it to the dependency's list of mods/configs that use it
                        foreach (Category c in parsedCatagoryLists)
                        {
                            //will itterate through every catagory once
                            foreach (SelectablePackage m in c.Packages)
                            {
                                foreach (LogicalDependency ld in m.LogicalDependencies)
                                {
                                    if (ld.PackageName.Equals(d.PackageName))
                                    {
                                        DatabaseLogic dbl = new DatabaseLogic()
                                        {
                                            PackageName = m.PackageName,
                                            Enabled = m.Enabled,
                                            Checked = m.Checked,
                                            NotFlag = ld.NegateFlag
                                        };
                                        d.DatabasePackageLogic.Add(dbl);
                                    }
                                }
                                if (m.Packages.Count > 0)
                                    ProcessConfigsLogical(d, m.Packages);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("installRelhaxMod_Click", "build the list of mods and configs that use each logical dependency", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("installRelhaxMod_Click", "add package to dependency's list of mods/configs that use it", ex);
            }

            try
            {
                //now each logical dependency has a complete list of every dependency, mod, and config that uses it, and if it is Enabled and checked
                //indicate if the logical dependency will be installed
                foreach (LogicalDependency ld in currentLogicalDependencies)
                {
                    //idea is that if all mod/config/dependency are to be installed, then install the logical dependency
                    //and factor in the negate flag
                    bool addIt;
                    int continueCount;
                    if (ld.AndOrLogic.Equals(LogicalDependency.AndOrFlag.AND))
                    {
                        // this is the logic for "AND" condition
                        // a element in the logical section "could" only be added, i at least one element could be checked. Other wise nothing to add and addIt will be "false" directly
                        addIt = ld.DatabasePackageLogic.Count > 0;
                        continueCount = 0;
                        foreach (DatabaseLogic dl in ld.DatabasePackageLogic)
                        {
                            if (!dl.Enabled)
                            {
                                //if the package that triggered it is not enabled, thengo to the next element
                                continueCount++;
                                // if all elements at ld.DatabasePackageLogic are disabled and non element is regulary checked, do not add the element
                                if (continueCount == ld.DatabasePackageLogic.Count)
                                    addIt = false;
                                continue;
                            }
                            if (dl.NotFlag)
                            {
                                //package must NOT be checked for it to be included
                                //Enabled must = true, checked must = false
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
                                //Enabled must = true, checked must = true
                                //otherwise break and don't add
                                if (dl.Enabled && !dl.Checked)
                                {
                                    addIt = false;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // this is the logic for "OR" condition
                        addIt = false;
                        continueCount = 0;
                        foreach (DatabaseLogic dl in ld.DatabasePackageLogic)
                        {
                            if (!dl.Enabled)
                            {
                                //if the package that triggered it is not enabled, then do go to the next logicalDependnecy element
                                continue;
                            }
                            if (dl.NotFlag)
                            {
                                //package must NOT be checked for it to be included
                                //Enabled must = true, checked must = false
                                //if so, add the logicalDependnecy and break, because more then installing once is wasted ;-)
                                if (dl.Enabled && !dl.Checked)
                                {
                                    addIt = true;
                                    break;
                                }
                            }
                            else
                            {
                                //package MUST be checked for it to be included
                                //Enabled must = true, checked must = true
                                //if so, add the logicalDependnecy and break, because more then installing once is wasted ;-)
                                if (dl.Enabled && dl.Checked)
                                {
                                    addIt = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (addIt && !logicalDependenciesToInstall.Contains(ld))
                        logicalDependenciesToInstall.Add(ld);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("installRelhaxMod_Click", "now each logical dependency has a complete list of every dependency ...", ex);
            }

            //create the list of shortcuts
            try
            {
                foreach (Dependency d in globalDependenciesToInstall)
                {
                    if (d.Enabled && d.Shortcuts.Count > 0)
                    {
                        foreach (Shortcut sc in d.Shortcuts)
                        {
                            if (sc.Enabled)
                            {
                                Shortcuts.Add(sc);
                            }
                        }
                    }
                }
                foreach (Dependency d in dependenciesToInstall)
                {
                    if (d.Enabled && d.Shortcuts.Count > 0)
                    {
                        foreach (Shortcut sc in d.Shortcuts)
                        {
                            if (sc.Enabled)
                            {
                                Shortcuts.Add(sc);
                            }
                        }
                    }
                }
                foreach (LogicalDependency ld in logicalDependenciesToInstall)
                {
                    if (ld.Enabled && ld.Shortcuts.Count > 0)
                    {
                        foreach (Shortcut sc in ld.Shortcuts)
                        {
                            if (sc.Enabled)
                            {
                                Shortcuts.Add(sc);
                            }
                        }
                    }
                }
                foreach (SelectablePackage dbo in modsConfigsToInstall)
                {
                    if (dbo.Enabled && dbo.Shortcuts.Count > 0)
                    {
                        foreach (Shortcut sc in dbo.Shortcuts)
                        {
                            if (sc.Enabled)
                            {
                                Shortcuts.Add(sc);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("create the list of shortcuts", ex);
            }

            //verify that all global dependencies, dependencies, and logicalDependencies are actually Enabled and have valid zip files
            //if they don't, remove them. if they do, macro the ExtractPath
            try
            {
                for (int i = 0; i < globalDependenciesToInstall.Count; i++)
                {
                    if ((!globalDependenciesToInstall[i].Enabled) || globalDependenciesToInstall[i].ZipFile.Equals(""))
                    {
                        globalDependenciesToInstall.RemoveAt(i);
                        i--;
                    }
                }
                for (int i = 0; i < dependenciesToInstall.Count; i++)
                {
                    if ((!dependenciesToInstall[i].Enabled) || dependenciesToInstall[i].ZipFile.Equals(""))
                    {
                        dependenciesToInstall.RemoveAt(i);
                        i--;
                    }
                }
                for (int i = 0; i < logicalDependenciesToInstall.Count; i++)
                {
                    if ((!logicalDependenciesToInstall[i].Enabled) || logicalDependenciesToInstall[i].ZipFile.Equals(""))
                    {
                        logicalDependenciesToInstall.RemoveAt(i);
                        i--;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("installRelhaxMod_Click", "verify that all ... are actually Enabled", ex);
            }

            //check for dependencies that actually need to be installed at the end
            try
            {
                for (int i = 0; i < globalDependenciesToInstall.Count; i++)
                {
                    if (globalDependenciesToInstall[i].AppendExtraction)
                    {
                        appendedDependenciesToInstall.Add(globalDependenciesToInstall[i]);
                        globalDependenciesToInstall.RemoveAt(i);
                        i--;
                    }
                }
                for (int i = 0; i < dependenciesToInstall.Count; i++)
                {
                    if (dependenciesToInstall[i].AppendExtraction)
                    {
                        appendedDependenciesToInstall.Add(dependenciesToInstall[i]);
                        dependenciesToInstall.RemoveAt(i);
                        i--;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("installRelhaxMod_Click", "check for dependencies that actually need to be installed at the end", ex);
            }

            try
            {
                //check for any user mods to install
                for (int i = 0; i < list.UserMods.Count; i++)
                {
                    if (list.UserMods[i].Enabled && list.UserMods[i].Checked)
                    {
                        userMods.Add(list.UserMods[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("installRelhaxMod_Click", "check for any user mods to install", ex);
            }

            //check that we will actually install something
            if (modsConfigsToInstall.Count == 0 && userMods.Count == 0)
            {
                //pull out because there are no mods to install
                downloadProgress.Text = Translations.GetTranslatedString("idle");
                ToggleUIButtons(true);
                list.Dispose();
                list = null;
                GC.Collect();
                return;
            }
            //if the user did not select any relhax modpack mods to install
            if (modsConfigsToInstall.Count == 0)
            {
                //clear any dependencies and logicalDependencies since this is a user mod only installation
                dependenciesToInstall.Clear();
                logicalDependenciesToInstall.Clear();
                appendedDependenciesToInstall.Clear();
            }
            //macro replacements

            //reset the download counter
            downloadCounter = -1;
            //create the download list
            foreach (Dependency gd in globalDependenciesToInstall)
            {
                if (gd.DownloadFlag)
                {
                    gd.ReadyForInstall = false;
                    DatabasePackagesToDownload.Add(gd);
                }
                else
                    gd.ReadyForInstall = true;
            }
            foreach (Dependency d in dependenciesToInstall)
            {
                if (d.DownloadFlag)
                {
                    d.ReadyForInstall = false;
                    DatabasePackagesToDownload.Add(d);
                }
                else
                    d.ReadyForInstall = true;
            }
            foreach (Dependency ad in appendedDependenciesToInstall)
            {
                if (ad.DownloadFlag)
                {
                    ad.ReadyForInstall = false;
                    DatabasePackagesToDownload.Add(ad);
                }
                else
                    ad.ReadyForInstall = true;
            }
            foreach (LogicalDependency ld in logicalDependenciesToInstall)
            {
                if (ld.DownloadFlag)
                {
                    ld.ReadyForInstall = false;
                    DatabasePackagesToDownload.Add(ld);
                }
                else
                    ld.ReadyForInstall = true;
            }
            foreach (SelectablePackage dbo in modsConfigsToInstall)
            {
                if (dbo.DownloadFlag)
                {
                    //CRC's don't match, need to re-download
                    dbo.ReadyForInstall = false;
                    DatabasePackagesToDownload.Add(dbo);
                }
                else
                    dbo.ReadyForInstall = true;
            }
            //upadte the parrentProgressPar
            parrentProgressBar.Maximum = DatabasePackagesToDownload.Count;
            //at this point, there may be user mods selected,
            //and there is at least one mod to extract
            Downloader_DownloadFileCompleted(null, null);
            //release no longer needed rescources and end the installation process
            list.Dispose();
            list = null;
            GC.Collect();
        }

        private void ProcessConfigs(List<SelectablePackage> configList)
        {
            foreach (SelectablePackage config in configList)
            {
                if (config.Enabled && config.Checked)
                {
                    if (!config.ZipFile.Equals(""))
                    {
                        modsConfigsToInstall.Add(config);
                    }

                    //check for userdata
                    if (config.UserFiles.Count > 0)
                        modsConfigsWithData.Add(config);

                    //check for shortcuts
                    if (config.Shortcuts.Count > 0)
                        ModsWithShortcuts.Add(config);

                    //check for configs
                    if (config.Packages.Count > 0)
                        ProcessConfigs(config.Packages);

                    //check for dependencies
                    if (config.Dependencies.Count > 0)
                        ProcessDependencies(config.Dependencies);
                }
            }
        }

        private void ProcessConfigsLogical(LogicalDependency d, List<SelectablePackage> configList)
        {
            foreach (SelectablePackage config in configList)
            {
                foreach (LogicalDependency ld in config.LogicalDependencies)
                {
                    if (ld.PackageName.Equals(d.PackageName))
                    {
                        DatabaseLogic dl = new DatabaseLogic()
                        {
                            PackageName = config.PackageName,
                            Enabled = config.Enabled,
                            Checked = config.Checked,
                            NotFlag = ld.NegateFlag
                        };
                        d.DatabasePackageLogic.Add(dl);
                    }
                }
            }
        }

        //processes a list of dependencies to add them (if needed) to the list of dependencies to install
        private void ProcessDependencies(List<Dependency> dependencies)
        {
            //every dependency is only a PackageName, and each must be added if they are not there already
            //but first need to find it
            foreach (Dependency d in dependencies)
            {
                Dependency temp = null;
                //find the actual dependency object from the list of available dependencies
                bool error = true;
                foreach (Dependency dd in currentDependencies)
                {
                    if (dd.PackageName.Equals(d.PackageName))
                    {
                        //the PackageName has been linked to the dependency
                        error = false;
                        temp = dd;
                        break;
                    }
                }
                if (error)
                {
                    Logging.Manager(string.Format("ERROR: could not match PackageName '{0}' from the list of dependencies", d.PackageName));
                    break;
                }
                //dependency has been found, if it's not in the list currently to install, add it
                if (!dependenciesToInstall.Contains(temp))
                    dependenciesToInstall.Add(temp);
            }
        }
        
        //Checks if the current database version is the same as the database version last installed into the selected World_of_Tanks directory
        private bool SameDatabaseVersions()
        {
            try
            {
                string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");  //xml doc name can change
                XDocument doc = XDocument.Parse(xmlString);

                var databaseVersion = doc.CreateNavigator().SelectSingleNode("/version/database");
                databaseVersionString = databaseVersion.InnerXml;
                Settings.DatabaseVersion = databaseVersionString;
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
                Utils.ExceptionLog("SameDatabaseVersions", ex);
                return false;
            }
        }
        #endregion

        #region progress reporting

        private string CreateExtractionMsgBoxProgressOutput(string[] s)
        {
            return string.Format("{0} {1} {2} {3}\n{4}\n{5}: {6} MB",
                Translations.GetTranslatedString("extractingPackage"),
                    s[0],
                    Translations.GetTranslatedString("of"),
                    s[1],
                    Utils.Truncate(string.Format("{0}: {1}", Translations.GetTranslatedString("file"), s[2]), downloadProgress.Font, downloadProgress.Width, 2),
                    Translations.GetTranslatedString("size"),
                    s[3].Equals("0") ? "0.01" : s[3]);
        }

        private void I_InstallProgressChanged(object sender, InstallerEventArgs e)
        {
            string message = "";
            totalProgressBar.Maximum = (int)InstallerEventArgs.InstallProgress.Done;
            switch (e.InstalProgress)
            {
                case InstallerEventArgs.InstallProgress.BackupMods:
                    childProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                        childProgressBar.Value = e.ChildProcessed;
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.BackupMods;
                    parrentProgressBar.Value = 0;
                    message = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("backupModFile"), e.ChildProcessed, Translations.GetTranslatedString("of"), e.ChildTotalToProcess);
                    break;
                case InstallerEventArgs.InstallProgress.BackupUserData:
                    childProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                        childProgressBar.Value = e.ChildProcessed;
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.BackupUserData;
                    parrentProgressBar.Value = 0;
                    message = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("backupUserdatas"), e.ChildProcessed, Translations.GetTranslatedString("of"), e.ChildTotalToProcess);
                    break;
                case InstallerEventArgs.InstallProgress.DeleteMods:
                    childProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                        childProgressBar.Value = e.ChildProcessed;
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.DeleteMods;
                    parrentProgressBar.Value = 0;
                    message = string.Format("{0} {1} {2} {3}\n{4}: {5}", Translations.GetTranslatedString("deletingFiles"), e.ChildProcessed, Translations.GetTranslatedString("of"),
                        e.ChildTotalToProcess, Translations.GetTranslatedString("file"), e.currentFile);
                    break;
                case InstallerEventArgs.InstallProgress.DeleteWoTCache:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.DeleteWoTCache;
                    childProgressBar.Value = 0;
                    parrentProgressBar.Value = 0;
                    message = Translations.GetTranslatedString("deletingWOTCache") + " ";
                    break;
                case InstallerEventArgs.InstallProgress.ExtractGlobalDependencies:
                case InstallerEventArgs.InstallProgress.ExtractDependencies:
                case InstallerEventArgs.InstallProgress.ExtractLogicalDependencies:
                case InstallerEventArgs.InstallProgress.ExtractMods:
                case InstallerEventArgs.InstallProgress.ExtractConfigs:
                case InstallerEventArgs.InstallProgress.ExtractAppendedDependencies:
                    totalProgressBar.Value = (int)e.InstalProgress;
                    parrentProgressBar.Maximum = e.ParrentTotalToProcess;
                    if ((parrentProgressBar.Minimum <= e.ParrentProcessed) && (e.ParrentProcessed <= parrentProgressBar.Maximum))
                        parrentProgressBar.Value = e.ParrentProcessed;
                    if (Settings.SuperExtraction && e.InstalProgress == InstallerEventArgs.InstallProgress.ExtractMods)
                    {
                        childProgressBar.Value = childProgressBar.Minimum;
                        message = string.Format("{0} {1} {2} {3}\n{4} {5}\n{6} MB", Translations.GetTranslatedString("parallelExtraction"), e.ParrentProcessed, Translations.GetTranslatedString("of"),
                            e.ParrentTotalToProcess, Translations.GetTranslatedString("file"), e.currentFile, Math.Round(e.currentFileSizeProcessed / MBDivisor, 2).ToString());
                    }
                    else
                    {
                        childProgressBar.Maximum = e.ChildTotalToProcess;
                        if (e.ChildProcessed > 0)
                            if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                                childProgressBar.Value = e.ChildProcessed;
                        message = CreateExtractionMsgBoxProgressOutput(new string[] { e.ParrentProcessed.ToString(), e.ParrentTotalToProcess.ToString(),
                        e.currentFile, Math.Round(e.currentFileSizeProcessed / MBDivisor, 2).ToString() });
                    }
                    break;
                case InstallerEventArgs.InstallProgress.RestoreUserData:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.RestoreUserData;
                    parrentProgressBar.Value = 0;
                    childProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                        childProgressBar.Value = e.ChildProcessed;
                    message = string.Format("{0} {1} {2} {3}", Translations.GetTranslatedString("restoringUserData"), e.ChildProcessed, Translations.GetTranslatedString("of"), e.ChildTotalToProcess);
                    break;
                case InstallerEventArgs.InstallProgress.UnpackXmlFiles:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.UnpackXmlFiles;
                    childProgressBar.Minimum = 0;
                    parrentProgressBar.Minimum = 0;
                    parrentProgressBar.Value = 0;
                    if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                        childProgressBar.Value = e.ChildProcessed;
                    message = string.Format("{0} {1} {2} {3}\n{4}", Translations.GetTranslatedString("unpackingXMLFiles"), e.ChildProcessed, Translations.GetTranslatedString("of"), e.ChildTotalToProcess, e.currentFile);
                    break;
                case InstallerEventArgs.InstallProgress.PatchMods:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.PatchMods;
                    childProgressBar.Value = 0;
                    parrentProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((parrentProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= parrentProgressBar.Maximum))
                        parrentProgressBar.Value = e.ChildProcessed;
                    message = string.Format("{0} {1}, {2} {3} {4}", Translations.GetTranslatedString("patchingFile"), e.currentFile, e.ChildProcessed, Translations.GetTranslatedString("of"), e.ChildTotalToProcess);
                    break;
                case InstallerEventArgs.InstallProgress.InstallFonts:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.InstallFonts;
                    parrentProgressBar.Value = 0;
                    childProgressBar.Value = 0;
                    message = Translations.GetTranslatedString("installingFonts") + " ";
                    break;
                case InstallerEventArgs.InstallProgress.ExtractUserMods:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.ExtractUserMods;
                    parrentProgressBar.Maximum = e.ParrentTotalToProcess;
                    if ((parrentProgressBar.Minimum <= e.ParrentProcessed) && (e.ParrentProcessed <= parrentProgressBar.Maximum))
                        parrentProgressBar.Value = e.ParrentProcessed;
                    childProgressBar.Maximum = e.ChildTotalToProcess;
                    if (e.ChildProcessed > 0)
                        childProgressBar.Value = e.ChildProcessed;
                    message = CreateExtractionMsgBoxProgressOutput(new string[] { e.ParrentProcessed.ToString(), e.ParrentTotalToProcess.ToString(), e.currentFile,
                        Math.Round(e.currentFileSizeProcessed / MBDivisor, 2).ToString() });
                    break;
                case InstallerEventArgs.InstallProgress.PatchUserMods:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.PatchMods;
                    childProgressBar.Value = 0;
                    parrentProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((parrentProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= parrentProgressBar.Maximum))
                        parrentProgressBar.Value = e.ChildProcessed;
                    message = string.Format("{0} {1}, {2} {3} {4}", Translations.GetTranslatedString("userPatchingFile"), e.currentFile, e.ChildProcessed, Translations.GetTranslatedString("of"), e.ChildTotalToProcess);
                    break;
                case InstallerEventArgs.InstallProgress.ExtractAtlases:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.ExtractAtlases;
                    parrentProgressBar.Maximum = e.ParrentTotalToProcess;
                    if ((parrentProgressBar.Minimum <= e.ParrentProcessed) && (e.ParrentProcessed <= parrentProgressBar.Maximum))
                        parrentProgressBar.Value = e.ParrentProcessed;
                    childProgressBar.Maximum = e.ChildTotalToProcess;
                    if (e.ChildProcessed > 0)
                        childProgressBar.Value = e.ChildProcessed;
                    message = string.Format("{0}: {1}\n{2}: {3}\n{4} {5} {6}", Translations.GetTranslatedString("AtlasExtraction"), e.currentFile, Translations.GetTranslatedString("AtlasTexture"),
                        e.currentSubFile, e.ChildProcessed, Translations.GetTranslatedString("of"), e.ChildTotalToProcess);
                    break;
                case InstallerEventArgs.InstallProgress.CreateAtlases:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.CreateAtlases;
                    parrentProgressBar.Minimum = 0;
                    parrentProgressBar.Maximum = e.ParrentTotalToProcess;
                    if ((parrentProgressBar.Minimum <= e.ParrentProcessed) && (e.ParrentProcessed <= parrentProgressBar.Maximum))
                        parrentProgressBar.Value = e.ParrentProcessed;
                    childProgressBar.Minimum = 0;
                    childProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                        childProgressBar.Value = e.ChildProcessed;
                    message = string.Format("{0} {1} {2} {3}\n {4} {5} {6} {7}", Translations.GetTranslatedString("AtlasCreating"), e.ParrentProcessed, Translations.GetTranslatedString("of"), e.ParrentTotalToProcess, e.ChildProcessed,
                        Translations.GetTranslatedString("of"), e.ChildTotalToProcess, Translations.GetTranslatedString("stepsComplete"));//AtlasCreating, stepsComplete
                    break;
                case InstallerEventArgs.InstallProgress.CreateShortcuts:
                    //don't bother showing anything cause it's not noticable...
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.CreateShortcuts;
                    break;
                case InstallerEventArgs.InstallProgress.InstallUserFonts:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.InstallFonts;
                    parrentProgressBar.Value = 0;
                    childProgressBar.Value = 0;
                    message = Translations.GetTranslatedString("installingUserFonts") + " ";
                    break;
                case InstallerEventArgs.InstallProgress.CheckDatabase:
                    totalProgressBar.Value = (int)InstallerEventArgs.InstallProgress.CheckDatabase;
                    parrentProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((parrentProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= parrentProgressBar.Maximum))
                        parrentProgressBar.Value = e.ChildProcessed;
                    message = string.Format("{0}: {1}", Translations.GetTranslatedString("deletingFile"), e.currentFile);
                    break;
                case InstallerEventArgs.InstallProgress.CleanUp:
                    message = Translations.GetTranslatedString("done");
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
                        MessageBox.Show(Translations.GetTranslatedString("installationFinished"), Translations.GetTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    break;
                case InstallerEventArgs.InstallProgress.Done:
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
                    DatabasePackagesToDownload = null;
                    parsedCatagoryLists = null;
                    ModsWithShortcuts = null;
                    Shortcuts = null;
                    InstallGroups = null;
                    currentDependencies = null;
                    currentLogicalDependencies = null;
                    usedFilesList = null;
                    patchList = null;
                    userMods = null;
                    modsConfigsWithData = null;
                    if (Settings.FirstLoad)
                        Settings.FirstLoad = false;
                    if (Program.autoInstall)
                        Program.autoInstall = false;
                    ToggleUIButtons(true);
                    break;
                case InstallerEventArgs.InstallProgress.Uninstall:
                    totalProgressBar.Value = 0;
                    parrentProgressBar.Value = 0;
                    childProgressBar.Maximum = e.ChildTotalToProcess;
                    if ((childProgressBar.Minimum <= e.ChildProcessed) && (e.ChildProcessed <= childProgressBar.Maximum))
                        childProgressBar.Value = e.ChildProcessed;
                    message = string.Format("{0} {1} {2} {3}\n{4}: {5}", Translations.GetTranslatedString("uninstallingFile"), e.ChildProcessed, Translations.GetTranslatedString("of"),
                        e.ChildTotalToProcess, Translations.GetTranslatedString("file"), e.currentFile);
                    break;
                case InstallerEventArgs.InstallProgress.UninstallDone:
                    message = Translations.GetTranslatedString("done");
                    totalProgressBar.Value = totalProgressBar.Maximum;
                    parrentProgressBar.Maximum = 1;
                    parrentProgressBar.Value = parrentProgressBar.Maximum;
                    childProgressBar.Maximum = 1;
                    childProgressBar.Value = childProgressBar.Maximum;
                    break;
                case InstallerEventArgs.InstallProgress.Error:
                    //a mistake has been made.
                    message = Translations.GetTranslatedString("error");
                    totalProgressBar.Value = totalProgressBar.Minimum;
                    parrentProgressBar.Value = parrentProgressBar.Minimum;
                    childProgressBar.Value = childProgressBar.Minimum;
                    break;
                default:
                    Logging.Manager("Invalid state: " + e.InstalProgress);
                    break;
            }
            if (errorCounter > 0 && Program.testMode)
            {
                this.ErrorCounterLabel.Visible = true;
                this.ErrorCounterLabel.Text = string.Format("Error counter: {0}", errorCounter);
            }
            downloadProgress.Text = message;
        }
        #endregion

        //Main method to uninstall the modpack
        private void UninstallRelhaxMod_Click(object sender, EventArgs e)
        {
            ToggleUIButtons(false);
            //reset progress bars
            parrentProgressBar.Value = parrentProgressBar.Minimum;
            totalProgressBar.Value = totalProgressBar.Minimum;
            childProgressBar.Value = childProgressBar.Minimum;
            //reset the interface
            this.downloadProgress.Text = "";
            //attempt to locate the tanks directory
            if (Settings.ForceManuel || this.AutoFindTanks() == null)
            {
                if (this.ManuallyFindTanks() == null)
                {
                    ToggleUIButtons(true);
                    return;
                }
            }
            //parse all strings
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
            Logging.Manager(string.Format("tanksLocation parsed as {0}", tanksLocation));
            Logging.Manager(string.Format("customUserMods parsed as {0}", Path.Combine(Application.StartupPath, "RelHaxUserMods")));
            tanksVersion = this.GetFolderVersion();
            if (MessageBox.Show(string.Format(Translations.GetTranslatedString("confirmUninstallMessage"), tanksLocation, Settings.UninstallMode.ToString()), Translations.GetTranslatedString("confirmUninstallHeader"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                unI = new Installer()
                {
                    // AppPath = Application.StartupPath,
                    TanksLocation = tanksLocation,
                    TanksVersion = tanksVersion
                };
                unI.InstallProgressChanged += I_InstallProgressChanged;
                Logging.Manager("Started Uninstallation process");
                //run the recursive complete uninstaller
                unI.StartUninstallation();
            }
            else
            {
                ToggleUIButtons(true);
            }
        }

        private void ApplyControlTranslations(Control.ControlCollection conts)
        {
            foreach (Control c in conts)
            {
                //only apply for common controls
                if (c is RadioButton || c is CheckBox || c is GroupBox || c is Label || c is LinkLabel || c is Button)
                    c.Text = Translations.GetTranslatedString(c.Name);
                if (c is Panel || c is GroupBox || c is TableLayoutPanel)
                    ApplyControlTranslations(c.Controls);
            }
        }
        //applies all settings from static settings class to this form
        private void ApplySettings()
        {
            //apply all checkmarks
            ComicSansFontCB.Checked = Settings.ComicSans;
            backupModsCheckBox.Checked = Settings.BackupModFolder;
            saveLastInstallCB.Checked = Settings.SaveLastConfig;
            saveUserDataCB.Checked = Settings.SaveUserData;
            darkUICB.Checked = Settings.DarkUI;
            clearLogFilesCB.Checked = Settings.DeleteLogs;
            Font = Settings.AppFont;
            notifyIfSameDatabaseCB.Checked = Settings.NotifyIfSameDatabase;
            SuperExtractionCB.Checked = Settings.SuperExtraction;
            EnableBordersDefaultCB.Checked = Settings.EnableBordersDefaultView;
            EnableBordersLegacyCB.Checked = Settings.EnableBordersLegacyView;
            EnableBordersDefaultV2CB.Checked = Settings.EnableBordersDefaultV2View;
            EnableColorChangeDefaultCB.Checked = Settings.EnableColorChangeDefaultView;
            EnableColorChangeLegacyCB.Checked = Settings.EnableColorChangeLegacyView;
            EnableColorChangeDefaultV2CB.Checked = Settings.EnableBordersDefaultV2View;
            LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedIndexChanged;
            switch (Translations.language)
            {
                //english = 0, polish = 1, german = 2, french = 3
                case (Translations.Languages.English):
                    LanguageComboBox.SelectedIndex = 0;
                    break;
                case (Translations.Languages.German):
                    LanguageComboBox.SelectedIndex = 2;
                    break;
                case (Translations.Languages.Polish):
                    LanguageComboBox.SelectedIndex = 1;
                    break;
                case (Translations.Languages.French):
                    LanguageComboBox.SelectedIndex = 3;
                    break;
            }
            LanguageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
            switch (Settings.SView)
            {
                case (SelectionView.Default):
                    //set default button, but disable checkedChanged handler to prevent stack overflow
                    SelectionDefault.Checked = true;
                    break;
                case (SelectionView.DefaultV2):
                    SelectionDefaultV2.Checked = true;
                    break;
                case (SelectionView.Legacy):
                    SelectionLegacy.Checked = true;
                    break;
            }
            switch (Settings.FontSizeforum)
            {
                case (FontSize.Font100):
                    fontSize100.Checked = true;
                    break;
                case (FontSize.Font125):
                    fontSize125.Checked = true;
                    break;
                case (FontSize.Font175):
                    fontSize175.Checked = true;
                    break;
                case (FontSize.Font225):
                    fontSize225.Checked = true;
                    break;
                case (FontSize.Font275):
                    fontSize275.Checked = true;
                    break;
                case (FontSize.DPI100):
                    DPI100.Checked = true;
                    break;
                case (FontSize.DPI125):
                    DPI125.Checked = true;
                    break;
                case (FontSize.DPI175):
                    DPI175.Checked = true;
                    break;
                case (FontSize.DPI225):
                    DPI225.Checked = true;
                    break;
                case (FontSize.DPI275):
                    DPI275.Checked = true;
                    break;
                case (FontSize.DPIAUTO):
                    DPIAUTO.Checked = true;
                    break;
            }
            ToggleScaleRBs(true);
        }

        //for when downloads are started, a timer to keep track of the download speed and ETA
        private void DownloadTimer_Tick(object sender, EventArgs e)
        {
            differenceTotalBytesDownloaded = currentTotalBytesDownloaded - previousTotalBytesDownloaded;
            float intervalInSeconds = (float)DownloadTimer.Interval / 1000;
            float sessionMBytesDownloaded = differenceTotalBytesDownloaded / MBDivisor;
            sessionDownloadSpeed = sessionMBytesDownloaded / intervalInSeconds;
            //set the previous for the last amount of bytes downloaded
            previousTotalBytesDownloaded = currentTotalBytesDownloaded;
        }
        //toggle UI buttons to be Enabled or disabled
        public void ToggleUIButtons(bool enableToggle)
        {
            installRelhaxMod.Enabled = enableToggle;
            uninstallRelhaxMod.Enabled = enableToggle;
            settingsGroupBox.Enabled = enableToggle;
            SelectionViewGB.Enabled = enableToggle;
            fontSizeGB.Enabled = enableToggle;
            languageSelectionGB.Enabled = enableToggle;
            DiagnosticUtilitiesButton.Enabled = enableToggle;
        }

        //handler for when the window is goingto be closed
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save settings
            if (Program.saveSettings) Settings.SaveSettings();
            Logging.Manager("cleaning \"RelHaxTemp\" folder");
            // Utils.DirectoryDelete(Path.Combine(Application.StartupPath, "RelHaxTemp"), true);
            Utils.DirectoryDelete(Settings.RelhaxTempFolder, true);
            Logging.Manager(string.Format("Exception counted: {0}", errorCounter));
            Logging.Manager("Application Closing");
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
            Logging.Dispose();
        }

        #region Scaling code

        public void ToggleScaleRBs(bool enableToggle)
        {
            float[] scales = new float[] { Settings.Scale100, Settings.Scale125, Settings.Scale175, Settings.Scale225, Settings.Scale275 };
            RadioButton[,] radioButtons = new RadioButton[,] { { fontSize100, DPI100 }, { fontSize125, DPI125 }, { fontSize175, DPI175 }, { fontSize225, DPI225 }, { fontSize275, DPI275 } };
            for (int i = 0; i < scales.Count(); i++)
            {
                float floatHeight = Height * scales[i];
                float floatWidth = Width * scales[i];
                radioButtons[i, 0].Enabled = CheckMainWindowSizeToMonitorSize((int)floatHeight, (int)floatWidth) && enableToggle;
                radioButtons[i, 1].Enabled = radioButtons[i, 0].Enabled;
            }
        }

        // https://stackoverflow.com/questions/254197/how-can-i-get-the-active-screen-dimensions
        private bool CheckMainWindowSizeToMonitorSize(int intHeight, int intWidth)
        {
            var hwnd = this.Handle;
            var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new NativeMethods.NativeMonitorInfo();
                NativeMethods.GetMonitorInfo(monitor, monitorInfo);
                var width = (monitorInfo.Monitor.Right - monitorInfo.Monitor.Left);
                var height = (monitorInfo.Monitor.Bottom - monitorInfo.Monitor.Top);
                return (intHeight < height && intWidth < width);
            }
            else
            {
                return false;
            }
        }

        //uses the stored options from before applying the new scaling settings to revert back to the previous settings
        private void RevertScalingSettings()
        {
            //first switch by scaling mode
            //then switch by size
            revertingScaling = true;
            switch(previousAutoScaleMode)
            {
                case AutoScaleMode.Dpi:
                    switch(previousFontSize)
                    {
                        case FontSize.DPI100:
                            DPI100.Checked = true;
                            break;
                        case FontSize.DPI125:
                            DPI125.Checked = true;
                            break;
                        case FontSize.DPI175:
                            DPI175.Checked = true;
                            break;
                        case FontSize.DPI225:
                            DPI225.Checked = true;
                            break;
                        case FontSize.DPI275:
                            DPI275.Checked = true;
                            break;
                        case FontSize.DPIAUTO:
                            DPIAUTO.Checked = true;
                            break;
                    }
                    break;
                case AutoScaleMode.Font:
                    switch(previousFontSize)
                    {
                        case FontSize.Font100:
                            fontSize100.Checked = true;
                            break;
                        case FontSize.Font125:
                            fontSize125.Checked = true;
                            break;
                        case FontSize.Font175:
                            fontSize175.Checked = true;
                            break;
                        case FontSize.Font225:
                            fontSize225.Checked = true;
                            break;
                        case FontSize.Font275:
                            fontSize275.Checked = true;
                            break;
                    }
                    break;
            }
            revertingScaling = false;
        }
        #endregion

        #region LinkClicked Events
        private void ShowAdvancedSettingsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (AdvancedSettings advset = new AdvancedSettings() { startX = Location.X + Size.Width + 3, startY = Location.Y, ApplyControlTranslationsOnLoad = true })
            {
                advset.ShowDialog();
            }
        }
        #endregion

        #region MouseEvents
        private void Generic_MouseLeave(object sender, EventArgs e)
        {
            if (DownloadTimer.Enabled)
                return;
            if (installRelhaxMod.Enabled && Settings.FirstLoad)
                downloadProgress.Text = Translations.GetTranslatedString("helperText");
            else
                downloadProgress.Text = "";
        }
        private void Generic_MouseEnter(object sender, EventArgs e)
        {
            if (DownloadTimer.Enabled)
                return;
            Control c = (Control)sender;
            downloadProgress.Text = Translations.GetTranslatedString(c.Name + "Description");
        }
        #endregion

        #region CheckChanged/SelectedIndexChanged events
        //handler for selection the new language from the combobox
        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (LanguageComboBox.SelectedIndex)
            {
                case 0://english
                    Translations.language = Translations.Languages.English;
                    break;
                case 1://polish
                    Translations.language = Translations.Languages.Polish;
                    break;
                case 2://german
                    Translations.language = Translations.Languages.German;
                    break;
                case 3://french
                    Translations.language = Translations.Languages.French;
                    break;
            }
            ApplyControlTranslations(Controls);
            ApplyVersionTextLabels();
        }
        //handler for when the "backupResMods mods" checkbox is changed
        private void BackupModsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.BackupModFolder = backupModsCheckBox.Checked;
        }

        private void SaveLastInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.SaveLastConfig = saveLastInstallCB.Checked;
        }

        private void SaveUserDataCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.SaveUserData = saveUserDataCB.Checked;
        }

        private void DarkUICB_CheckedChanged(object sender, EventArgs e)
        {
            //set the thing
            Settings.DarkUI = darkUICB.Checked;
            SuspendLayout();
            Settings.SetUIColorsWinForms(this);
            ResumeLayout(false);
        }

        private void SelectionDefault_CheckedChanged(object sender, EventArgs e)
        {
            Settings.SView = SelectionView.Default;
        }

        private void SelectionLegacy_CheckedChanged(object sender, EventArgs e)
        {
            Settings.SView = SelectionView.Legacy;
        }

        private void SelectionDefaultV2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.SView = SelectionView.DefaultV2;
        }

        private void FontSize100_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize100.Checked)
            {
                if(!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.FontSizeforum = FontSize.DPI100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.AppFont;
                }
                //change settings enum
                Settings.FontSizeforum = FontSize.Font100;
                //apply change of settings enum
                Settings.ApplyInternalProperties();
                //get new scalingMode (or no change, get it anyway)
                this.AutoScaleMode = Settings.AppScalingMode;
                //get new font
                this.Font = Settings.AppFont;
                //null means it's the revert code
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if(fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void FontSize125_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize125.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.FontSizeforum = FontSize.DPI100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.Font125;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void FontSize175_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize175.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.FontSizeforum = FontSize.DPI100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.Font175;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void FontSize225_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize225.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.FontSizeforum = FontSize.DPI100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.Font225;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void FontSize275_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSize275.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Dpi)
                {
                    Settings.FontSizeforum = FontSize.DPI100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    float temp = 1.0f / scale;
                    this.Scale(new SizeF(temp, temp));
                    scale = 1.0f;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.Font275;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void DPI100_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI100.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.FontSizeforum = FontSize.Font100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.DPI100;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                float temp = Settings.Scale100 / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.Scale100;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void DPI125_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI125.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.FontSizeforum = FontSize.Font100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.DPI125;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                float temp = Settings.Scale125 / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.Scale125;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void DPI175_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI175.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.FontSizeforum = FontSize.Font100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.DPI175;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                float temp = Settings.Scale175 / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.Scale175;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void DPI225_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI225.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.FontSizeforum = FontSize.Font100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.DPI225;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                float temp = Settings.Scale225 / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.Scale225;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void DPI275_CheckedChanged(object sender, EventArgs e)
        {
            if (DPI275.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.FontSizeforum = FontSize.Font100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.DPI275;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                float temp = Settings.Scale275 / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.Scale275;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }

        private void DPIAUTO_CheckedChanged(object sender, EventArgs e)
        {
            if (DPIAUTO.Checked)
            {
                if (!revertingScaling)
                {
                    previousAutoScaleMode = Settings.AppScalingMode;
                    previousFontSize = Settings.FontSizeforum;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
                {
                    Settings.FontSizeforum = FontSize.Font100;
                    Settings.ApplyInternalProperties();
                    this.AutoScaleMode = Settings.AppScalingMode;
                    this.Font = Settings.AppFont;
                }
                Settings.FontSizeforum = FontSize.DPIAUTO;
                Settings.ApplyInternalProperties();
                this.AutoScaleMode = Settings.AppScalingMode;
                float temp = Settings.ScaleSize / scale;
                this.Scale(new SizeF(temp, temp));
                scale = Settings.ScaleSize;
                this.Font = Settings.AppFont;
                if (!revertingScaling && !loading)
                {
                    using (FontSettingsVerify fsv = new FontSettingsVerify()
                    {
                        ApplyControlTranslationsOnLoad = true,
                        startX = Location.X,
                        startY = Location.Y
                    })
                    {
                        if (fsv.ShowDialog() == DialogResult.No)
                        {
                            RevertScalingSettings();
                        }
                    }
                    ToggleScaleRBs(true);
                }
            }
        }
        //enalbes the user to use "comic sans" font for the 1 person that would ever want to do that
        private void CancerFontCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ComicSans = ComicSansFontCB.Checked;
            Settings.ApplyInternalProperties();
            Font = Settings.AppFont;
        }
        private void ClearLogFilesCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.DeleteLogs = clearLogFilesCB.Checked;
        }

        private void NotifyIfSameDatabaseCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.NotifyIfSameDatabase = notifyIfSameDatabaseCB.Checked;
        }

        private void SuperExtractionCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.SuperExtraction = SuperExtractionCB.Checked;
        }

        private void EnableBordersDefaultCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.EnableBordersDefaultView = EnableBordersDefaultCB.Checked;
        }

        private void EnableBordersLegacyCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.EnableBordersLegacyView = EnableBordersLegacyCB.Checked;
        }

        private void EnableColorChangeDefaultCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.EnableColorChangeDefaultView = EnableColorChangeDefaultCB.Checked;
        }

        private void EnableColorChangeLegacyCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.EnableColorChangeLegacyView = EnableColorChangeLegacyCB.Checked;
        }

        private void EnableBordersDefaultV2CB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.EnableBordersDefaultV2View = EnableBordersDefaultV2CB.Checked;
        }

        private void EnableColorChangeDefaultV2CB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.EnableColorChangeDefaultV2View = EnableColorChangeDefaultV2CB.Checked;
        }
        #endregion

        #region Click events
        private void CancelDownloadButton_Click(object sender, EventArgs e)
        {
            Downloader.CancelAsync();
        }

        private void ViewAppUpdates_Click(object sender, EventArgs e)
        {
            int xloc = this.Location.X + this.Size.Width + 10;
            int yloc = this.Location.Y;
            using (ViewUpdates vu = new ViewUpdates(xloc, yloc, Settings.ManagerInfoDatFile, Settings.BetaApplication ? "releaseNotes_beta.txt" : "releaseNotes.txt"))
            {
                vu.ShowDialog();
            }
        }

        private void ViewDBUpdates_Click(object sender, EventArgs e)
        {
            int xloc = this.Location.X + this.Size.Width + 10;
            int yloc = this.Location.Y;
            using (ViewUpdates vu = new ViewUpdates(xloc, yloc, Settings.ManagerInfoDatFile, "databaseUpdate.txt"))
            {
                vu.ShowDialog();
            }
        }

        private void DiagnosticUtilitiesButton_Click(object sender, EventArgs e)
        {
            ToggleUIButtons(false);
            //attempt to locate the tanks directory
            if (Settings.ForceManuel || AutoFindTanks() == null)
            {
                if (ManuallyFindTanks() == null)
                {
                    ToggleUIButtons(true);
                    return;
                }
            }
            //parse all strings
            tanksLocation = tanksLocation.Substring(0, tanksLocation.Length - 17);
            Logging.Manager(string.Format("tanksLocation parsed as {0}", tanksLocation));
            using (Diagnostics d = new Diagnostics()
            {
                TanksLocation = tanksLocation,
                AppStartupPath = Application.StartupPath,
                ParentWindow = this
            })
            {
                d.ShowDialog();
            }
            ToggleUIButtons(true);
        }

        private void DonateLabel_Click(object sender, EventArgs e)
        {
            /*      de_DE           DE
                    pl_PL           PL
                    fr_FR           FR
                    en_US           US
                    en_GB           GB */

            // Logging.Manager("language: " + CultureInfo.CurrentUICulture.Name);
            // Logging.Manager("TwoLetterISOLanguageName: " + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);            
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=76KNV8KXKYNG2");
            // https://www.paypal.com/paypalme/grab?country.x=IN&locale.x=en_IN
        }

        private void FindBugAddModLabel_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://forums.relhaxmodpack.com/");
        }

        private void DiscordServerLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/58fdPvK");
        }
        //when the "visit form page" link is clicked. the link clicked handler
        private void FormPageNALink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.com/index.php?/topic/535868-");
        }

        private void FormPageEULink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.eu/index.php?/topic/623269-");
        }

        private void FormPageEUGERLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.worldoftanks.eu/index.php?/topic/624499-");
        }

        private void VisitWebsiteLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://relhaxmodpack.com/");
        }

        private void SendEmailLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:info@relhaxmodpack.com");
        }

        private void ViewTwitterLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://twitter.com/relhaxmodpack");
        }

        private void ViewFacebookLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.facebook.com/Relhax-Modpack-187224775238379/");
        }
        #endregion

        #region Context menu stuff
        /// <summary>
        /// Occures when the taskbar icon is right clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RMIcon_MouseClick(object sender, MouseEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButtons.Right:

                    break;
                case MouseButtons.Left:
                    //if the application is not displayed on the screen (minimized, for example), then show it.
                    if (WindowState != FormWindowState.Normal)
                        WindowState = FormWindowState.Normal;
                    break;
            }            
        }

        private void MenuItemAppClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MenuItemCheckUpdates_Click(object sender, EventArgs e)
        {
            if (ins != null)
                return;
            using (PleaseWait wait = new PleaseWait())
            {
                //save the last old database version
                string oldDatabaseVersion = Settings.DatabaseVersion;
                Hide();
                wait.Show();
                wait.loadingDescBox.Text = Translations.GetTranslatedString("checkForUpdates");
                Application.DoEvents();
                this.CheckmanagerUpdates();
                wait.Close();
                Show();
                //get the new database version and compare. if new, inform the user
                if (!Settings.DatabaseVersion.Equals(oldDatabaseVersion))
                {
                    //TODO: translate
                    MessageBox.Show(Translations.GetTranslatedString("newDBApplied"));
                }
            }
        }

        private void MenuItemRestore_Click(object sender, EventArgs e)
        {
            //if the application is not displayed on the screen (minimized, for example), then show it.
            if (WindowState != FormWindowState.Normal)
                WindowState = FormWindowState.Normal;
        }
        #endregion
    }
}
