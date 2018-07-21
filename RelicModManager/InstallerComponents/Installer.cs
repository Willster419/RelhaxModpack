using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Ionic.Zip;
using System.Xml;
using System.Diagnostics;
using System.Net;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.XPath;
using RelhaxModpack.InstallerComponents;
using System.Collections;

namespace RelhaxModpack
{
    //Delegate to hook up them events
    public delegate void InstallChangedEventHandler(object sender, InstallerEventArgs e);

    public class Installer : IDisposable
    {
        /*
         * This new installer class will handle all of the installation process, effectivly black-boxing the installation, in a single seperate backgroundworker.
         * Then we can get out of using the MainWindow to install. It will handle all of the backing up, copying, extracting and patching of the modpack.
         * This way the code is easier to follow, and has one central place to take care of the entire install process.
         * This also enables us to use syncronous thinking when approaching the installation procedures of the modpack.
        */
        //everything that it needs to install
        public string TanksLocation { get; set; }
        // public string AppPath { get; set; }
        public List<Dependency> GlobalDependencies { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public List<LogicalDependency> LogicalDependencies { get; set; }
        public List<Dependency> AppendedDependencies { get; set; }
        public List<SelectablePackage> ModsConfigsToInstall { get; set; }
        public List<SelectablePackage> ModsConfigsWithData { get; set; }
        public List<SelectablePackage> UserMods { get; set; }
        public List<Shortcut> Shortcuts { get; set; }
        private List<Patch> PatchList { get; set; }
        private List<XmlUnpack> XmlUnpackList { get; set; }
        private List<Atlas> AtlasesList { get; set; }
        public string TanksVersion { get; set; }
        public List<InstallGroup> InstallGroups { get; set; }
        public int TotalCategories = 0;
        //the folder of the current user appdata
        public string AppDataFolder { get; set; }
        public string DatabaseVersion { get; set; }
        //properties relevent to the handler and install
        public static BackgroundWorker InstallWorker;
        public static InstallerEventArgs args;
        private string xvmConfigDir = "";
        private int patchNum = 0;
        private int NumExtractorsCompleted = 0;
        private int NumAtlasCreatorsComplete = 0;
        //https://stackoverflow.com/questions/9280054/c-sharp-hashtable-sorted-by-keys
        private SortedDictionary<string, string> originalSortedPatchNames;
        private string InstalledFilesLogPath = "";
        private object lockerInstaller = new object();
        private Hashtable zipMacros = new Hashtable();
        private List<string> totalShortcuts;

        #region boring stuff
        //the event that it can hook into
        public event InstallChangedEventHandler InstallProgressChanged;

        //the changed event (setups the hander)
        protected virtual void OnInstallProgressChanged()
        {
            if (InstallProgressChanged != null && args.InstalProgress != InstallerEventArgs.InstallProgress.Idle)
                InstallProgressChanged(this, args);
        }
        
        //constructer
        public Installer()
        {
            InstallWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true
            };
            InstallWorker.ProgressChanged += WorkerReportProgress;
            InstallWorker.RunWorkerCompleted += WorkerReportComplete;
            args = new InstallerEventArgs();
            ResetArgs();
            //originalPatchNames = new List<string>();
            originalSortedPatchNames = new SortedDictionary<string, string>();
        }

        //Start installation on the UI thread
        public void StartInstallation()
        {
            InstallWorker.DoWork += ActuallyStartInstallation;
            InstallWorker.RunWorkerAsync();
        }

        public void StartUninstallation()
        {
            InstallWorker.DoWork += ActuallyStartUninstallation;
            InstallWorker.RunWorkerAsync();
        }
        //handler for when progress is made in extracting a zip file
        void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            //args.ChildProcessed = e.EntriesExtracted;
            if (e.CurrentEntry != null)
            {
                args.currentFile = e.CurrentEntry.FileName;
                args.currentFileSizeProcessed = e.BytesTransferred;
            }
            InstallWorker.ReportProgress(0);
        }

        public void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
        {
            OnInstallProgressChanged();
        }

        public void WorkerReportComplete(object sender, AsyncCompletedEventArgs e)
        {
            //a successfull install means that there was no error, thus error property is null
            if(e.Error == null)
            {
                Logging.Manager("Installation Done");
                args.InstalProgress = InstallerEventArgs.InstallProgress.Done;
                OnInstallProgressChanged();
            }
            else
            {
                //an error occured
                Utils.ExceptionLog(e.Error);
                Logging.Manager("ERROR: the install failed");
                args.InstalProgress = InstallerEventArgs.InstallProgress.Error;
                OnInstallProgressChanged();
            }
        }
        //reset the args
        public void ResetArgs()
        {
            args.InstalProgress = InstallerEventArgs.InstallProgress.Idle;
            args.ChildProcessed = 0;
            args.ChildTotalToProcess = 0;
            args.currentFile = "";
            args.currentSubFile = "";
            args.currentFileSizeProcessed = 0;
            args.ParrentProcessed = 0;
            args.ParrentTotalToProcess = 0;
        }

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(Program.Version == Program.ProgramVersion.Alpha)
                Logging.Manager("DEBUG: ON Bg_RunWorkerCompleted");
            lock (lockerInstaller)
            {
                NumExtractorsCompleted++;
                args.ParrentProcessed++;
                Logging.Manager("Number of threads completed: " + NumExtractorsCompleted);
                InstallWorker.ReportProgress(0);
            }
        }
        //gets the total number of files to process to eithor delete or copy
        private List<string> NumFilesToProcess(string folder)
        {
            List<string> list = new List<string>();
            try
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(folder);
                DirectoryInfo[] dirs = dir.GetDirectories();
                // Get the files in the directory
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    list.Add(file.FullName);
                    args.ChildTotalToProcess++;
                }
                foreach (DirectoryInfo subdir in dirs)
                {
                    list.Add(subdir.FullName + @"\");
                    args.ChildTotalToProcess++;
                    list.AddRange(NumFilesToProcess(subdir.FullName));
                }
            }
            catch { }
            return list;
        }
        #endregion

        public void ActuallyStartUninstallation(object sender, DoWorkEventArgs e)
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.Uninstall;
            switch(Settings.UninstallMode)
            {
                case UninstallModes.Default:
                    UninstallModsDefault();
                    break;
                case UninstallModes.Quick:
                    UninstallModsQuick();
                    break;
            }
            //put back the folders when done
            if (!Directory.Exists(Path.Combine(TanksLocation, "res_mods", TanksVersion)))
                Directory.CreateDirectory(Path.Combine(TanksLocation, "res_mods", TanksVersion));
            if (!Directory.Exists(Path.Combine(TanksLocation, "mods", TanksVersion)))
                Directory.CreateDirectory(Path.Combine(TanksLocation, "mods", TanksVersion));
            args.InstalProgress = InstallerEventArgs.InstallProgress.UninstallDone;
            InstallWorker.ReportProgress(0);
            Logging.Manager("Uninstallation process finished");
            MessageBox.Show(Translations.GetTranslatedString("uninstallFinished"), Translations.GetTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        //Start the installation on the Worker thread
        public void ActuallyStartInstallation(object sender, DoWorkEventArgs e)
        {
            Stopwatch installTimer = new Stopwatch();
            installTimer.Start();
            Logging.Manager("---Starting an installation---");
            long beforeExtraction = 0;
            long duringExtraction = 0;
            long afterExtraction = 0;
            //fill the hashtable
            zipMacros.Add("WoTAppData", AppDataFolder);
            zipMacros.Add("_AppData", AppDataFolder);
            zipMacros.Add("_RelHaxLibraries", Application.StartupPath);
            ResetArgs();
            InstalledFilesLogPath = Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log");
            //Step 1: do a backup if requested
            Logging.Manager("Installation BackupMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.BackupMods;
            if (Settings.BackupModFolder)
                BackupMods();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 2: do a backup of user data
            Logging.Manager("Installation BackupUserData");
            args.InstalProgress = InstallerEventArgs.InstallProgress.BackupUserData;
            if (Settings.SaveUserData)
                BackupUserData();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 3: Delete Mods
            Logging.Manager("Installation UninstallMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.DeleteMods;
            if (Settings.CleanInstallation)
            {
                switch (Settings.UninstallMode)
                {
                    case UninstallModes.Default:
                        UninstallModsDefault();
                        break;
                    case UninstallModes.Quick:
                        UninstallModsQuick();
                        break;
                }
            }
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Setp 4: delete log files if selected
            Logging.Manager("Installation deleteLogs");
            if (Settings.DeleteLogs)
            {
                string[] logsToDelete = new string[]
                {
                    Path.Combine(TanksLocation, "python.log"),
                    Path.Combine(TanksLocation, "xvm.log"),
                    Path.Combine(TanksLocation, "pmod.log"),
                    Path.Combine(TanksLocation, "WoTLauncher.log"),
                    Path.Combine(TanksLocation, "cef.log")
                };
                Logging.Manager("deleteLogs selected: deleting wot, xvm and pmod logs");
                try
                {
                    foreach (string file in logsToDelete)
                    {
                        try
                        {
                            if (File.Exists(file))
                                File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            ex = ex.GetBaseException();
                            Logging.Manager(string.Format("failed to delete: {0} ({1})", file, ex.Message));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("ActuallyStartInstallation", "deleteLogs", ex);
                }
            }
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 5: Delete user appdata cache if requested
            Logging.Manager("Installation DeleteWoTCache");
            args.InstalProgress = InstallerEventArgs.InstallProgress.DeleteWoTCache;
            if (Settings.ClearCache)
                ClearWoTCache();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            beforeExtraction = installTimer.ElapsedMilliseconds;
            Logging.Manager("Recorded Install time before extraction (msec): " + beforeExtraction);
            //Step 6-10: Extract packages
            Logging.Manager("Installation ExtractDatabaseObjects");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractGlobalDependencies;
            ExtractDatabaseObjects();
            ResetArgs();
            duringExtraction = installTimer.ElapsedMilliseconds - beforeExtraction;
            Logging.Manager("Recorded Install time during extraction (msec): " + duringExtraction);
            //Step 11: Restore User Data
            Logging.Manager("Installation RestoreUserData");
            args.InstalProgress = InstallerEventArgs.InstallProgress.RestoreUserData;
            if (Settings.SaveUserData)
                RestoreUserData();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 12: unpack original game xml file
            Logging.Manager("Installation UnpackXmlFiles");
            args.InstalProgress = InstallerEventArgs.InstallProgress.UnpackXmlFiles;
            if (Directory.Exists(Path.Combine(TanksLocation, "_xmlUnPack")))
            {
                UnpackXmlFiles();
            }
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 13: Extract User Mods
            Logging.Manager("Installation ExtractUserMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractUserMods;
            if (UserMods.Count > 0)
                ExtractUserMods();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 14: Patch Mods
            Logging.Manager("Installation PatchMods (previously patchUserMods)");
            args.InstalProgress = InstallerEventArgs.InstallProgress.PatchUserMods;
            if (Directory.Exists(Path.Combine(TanksLocation, "_patch")))
                PatchFiles();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 15: Parse and create Atlases
            Logging.Manager("Installation ExtractAtlases");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractAtlases;
            if (Directory.Exists(Path.Combine(TanksLocation, "_atlases")))
            {
                ExtractAtlases();
            }
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 16: Install Fonts
            Logging.Manager("Installation UserFonts");
            args.InstalProgress = InstallerEventArgs.InstallProgress.InstallUserFonts;
            if (Directory.Exists(Path.Combine(TanksLocation, "_fonts")))
                InstallFonts();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 17: create shortCuts
            Logging.Manager("Installation CreateShortscuts");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CreateShortcuts;
            if (Settings.CreateShortcuts)
                CreateShortcuts();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 18: CheckDatabase and delete outdated or no more needed files
            Logging.Manager("Installation CheckDatabase");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CheckDatabase;
            if ((!Program.testMode) && (!Settings.BetaDatabase) && (Program.Version != Program.ProgramVersion.Alpha))
                CheckForOldZipFiles();
            else
                Logging.Manager("... skipped");
            ResetArgs();
            //Step 19: Cleanup
            Logging.Manager("Intallation CleanUp");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CleanUp;
            if(!Settings.ExportMode)
            {
                List<string> folder = new List<string> { "_readme", "_patch", "_shortcuts", "_xmlUnPack", "_atlases", "_fonts" };
                foreach (string f in folder)
                {
                    try
                    {
                        if (Directory.Exists(Path.Combine(TanksLocation, f)))
                            Directory.Delete(Path.Combine(TanksLocation, f), true);
                    }
                    catch (Exception ex)
                    {
                        ex = ex.GetBaseException();
                        Logging.Manager(string.Format("error at folder delete: {0} ({1})", f, ex.Message));
                    }
                }
            }

            InstallWorker.ReportProgress(0);
            Logging.InstallerFinished();                                      // installation is finished. logfile will be flushed and filestream will be disposed
            afterExtraction = installTimer.ElapsedMilliseconds - duringExtraction - beforeExtraction;
            Logging.Manager("Recorded time after extraction (msec): " + afterExtraction);
            long totalExtraction = beforeExtraction + duringExtraction + afterExtraction;
            Logging.Manager("Total recorded install time (msec): " + totalExtraction);
        }

        
        //Step 1: Backup Mods
        public void BackupMods()
        {
            try
            {
                //backupResMods the mods folder
                if (!Directory.Exists(Settings.RelHaxModBackupFolder))
                    Directory.CreateDirectory(Settings.RelHaxModBackupFolder);
                //create a new mods folder based on date and time
                //yyyy-MM-dd-HH-mm-ss
                DateTime now = DateTime.Now;
                string folderDateName = String.Format("{0:yyyy-MM-dd-HH-mm-ss}", now);
                if (!Directory.Exists(Path.Combine(Settings.RelHaxModBackupFolder, folderDateName, "res_mods")))
                    Directory.CreateDirectory(Path.Combine(Settings.RelHaxModBackupFolder, folderDateName, "res_mods"));
                if (!Directory.Exists(Path.Combine(Settings.RelHaxModBackupFolder, folderDateName, "mods")))
                    Directory.CreateDirectory(Path.Combine(Settings.RelHaxModBackupFolder, folderDateName, "mods"));
                NumFilesToProcess(Path.Combine(Settings.RelHaxModBackupFolder, folderDateName, "mods"));
                NumFilesToProcess(Path.Combine(Settings.RelHaxModBackupFolder, folderDateName, "res_mods"));
                InstallWorker.ReportProgress(0);
                DirectoryCopy(Path.Combine(TanksLocation, "res_mods"), Path.Combine(Settings.RelHaxModBackupFolder, folderDateName, "res_mods"), true);
                DirectoryCopy(Path.Combine(TanksLocation, "mods"), Path.Combine(Settings.RelHaxModBackupFolder, folderDateName, "mods"), true);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("BackupMods", "ex", ex);
            }
        }

        //Step 2: Backup User Data
        public void BackupUserData()
        {
            try
            {
                args.ChildTotalToProcess = ModsConfigsWithData.Count();
                foreach (SelectablePackage dbo in ModsConfigsWithData)
                {
                    args.ChildProcessed++;
                    InstallWorker.ReportProgress(0);
                    try
                    {
                        int c = 0;
                        foreach (string s in dbo.UserFiles)
                        {
                            try
                            {
                                string correctedPath = s.TrimStart('\x005c').Replace(@"\\", @"\");
                                string folderPath = "";
                                if (correctedPath[0].Equals("{"))
                                {
                                    correctedPath = Utils.ReplaceMacro(correctedPath);
                                    folderPath = correctedPath;
                                }
                                else
                                {
                                    correctedPath = Utils.ReplaceMacro(correctedPath);
                                    folderPath = Path.Combine(TanksLocation, Path.GetDirectoryName(correctedPath));
                                }
                                string filenamePrefix = Utils.GetValidFilename(dbo.Name + "_" + c + "_");
                                c++;
                                if (!Directory.Exists(folderPath)) continue;
                                string[] fileList = Directory.GetFiles(folderPath, Path.GetFileName(correctedPath));   // use the GetFileName(correctedPath) as a search pattern, to only get wanted files
                                int fc = 0;
                                args.FilesToDo = fileList.Length;
                                foreach (string startLoc in fileList)
                                {
                                    string destLoc = Path.Combine(Settings.RelhaxTempFolder, filenamePrefix + Path.GetFileName(startLoc));
                                    try
                                    {
                                        if (File.Exists(@startLoc))
                                        {
                                            File.Move(startLoc, destLoc);
                                            fc++;
                                            if (fileList.Length < 5) Logging.Manager(string.Format("BackupUserData: {0} ({1})", Path.Combine(Path.GetDirectoryName(correctedPath), Path.GetFileName(startLoc)), Path.GetFileName(correctedPath)));
                                        }
                                    }
                                    catch
                                    {
                                        if (Program.testMode) { MessageBox.Show(string.Format("Error: can not move file.\nstartLoc: \"{0}\"\ndestLoc: \"{1}\"", startLoc, destLoc)); };
                                        Logging.Manager(string.Format("Error: can not move file. startLoc: \"{0}\" destLoc: \"{1}\"", startLoc, destLoc));
                                    }
                                    args.Filecounter = fc;
                                    InstallWorker.ReportProgress(0);
                                }
                                if (!(fileList.Length < 5)) Logging.Manager(string.Format("BackupUserData: {0} files ({1})", fc, correctedPath));
                            }
                            catch (Exception exStartLoc)
                            {
                                Utils.ExceptionLog("BackupUserData", "exStartLoc", exStartLoc);
                            }
                        }
                    }
                    catch (Exception exS)
                    {
                        Utils.ExceptionLog("BackupUserData", "s", exS);
                    }
                }
            }
            catch (Exception exDbo)
            {
                Utils.ExceptionLog("BackupUserData", "dbo", exDbo);
            }
        }

        //Step 3: Delete all mods (default)
        public void UninstallModsDefault()
        {
            List<string> linesFromLog = new List<string>();
            List<string> filesFromLog = new List<string>();
            List<string> foldersFromLog = new List<string>();
            List<string> shortcutsFromLog = new List<string>();
            List<string> linesFromParsing = new List<string>();
            List<string> filesFromParsing = new List<string>();
            List<string> foldersFromParsing = new List<string>();
            List<string> shortcutsFromParsing = new List<string>();
            List<string> totalFiles = new List<string>();
            List<string> totalFolders = new List<string>();
            totalShortcuts = new List<string>();//make this global later

            string installLogFile = Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log");
            if (File.Exists(installLogFile))
            {
                linesFromLog = File.ReadAllLines(installLogFile).ToList();
            }
            foreach(string s in linesFromLog)
            {
                //parse the lists so that only files are folders are saved
                if(File.Exists(s))
                {
                    if(Path.GetExtension(s).Equals(".lnk"))
                    {
                        shortcutsFromLog.Add(Path.GetFileName(s));
                    }
                    else
                    {
                        filesFromLog.Add(s);
                    }
                }
                else if (Directory.Exists(s))
                {
                    foldersFromLog.Add(s);
                }
            }
            //reverse folder list so that it deletes from most down to up the path
            foldersFromLog.Reverse();
            Logging.Manager(string.Format("Elements to delete (from logfile): {0}", filesFromLog.Count + foldersFromLog.Count + shortcutsFromLog.Count));

            //get list of files from parsing the folders
            linesFromParsing.AddRange(NumFilesToProcess(Path.Combine(TanksLocation, "res_mods")));
            linesFromParsing.AddRange(NumFilesToProcess(Path.Combine(TanksLocation, "mods")));
            foreach (string s in linesFromParsing)
            {
                //parse the lists so that only files are folders are saved
                if (File.Exists(s))
                {
                    if (Path.GetExtension(s).Equals(".lnk"))
                    {
                        shortcutsFromParsing.Add(Path.GetFileName(s));
                    }
                    else
                    {
                        filesFromParsing.Add(s);
                    }
                }
                else if (Directory.Exists(s))
                {
                    foldersFromParsing.Add(s);
                }
            }
            foldersFromParsing.Reverse();
            Logging.Manager(string.Format("Elements to delete (from parsing mods/res_mods): {0}", filesFromParsing.Count + foldersFromParsing.Count + shortcutsFromParsing.Count));

            //merge list with total files to process from parsing the mods and res_mods directories
            //assumes the paths from the log file are absolute (TODO)
            totalFiles = new List<string>(filesFromParsing);
            totalFiles.AddRange(new List<string>(filesFromLog));
            totalFiles = totalFiles.Distinct().ToList();
            totalFiles.Sort();

            totalFolders = new List<string>(foldersFromParsing);
            totalFolders.AddRange(new List<string>(foldersFromLog));
            totalFolders = totalFolders.Distinct().ToList();
            totalFolders.Sort();
            totalFolders.Reverse();

            totalShortcuts = new List<string>(shortcutsFromParsing);
            totalShortcuts.AddRange(new List<string>(shortcutsFromLog));
            totalShortcuts = totalShortcuts.Distinct().ToList();
            //we now have a list of total valid files and folders to delte
            //report the progress
            args.ChildTotalToProcess = totalFiles.Count + totalFolders.Count + totalShortcuts.Count;
            args.ChildProcessed = 0;
            InstallWorker.ReportProgress(0);

            //backup old uninstall log file
            Logging.Manager("backing up old uninstall log file", true);
            string logFile = Path.Combine(TanksLocation, "logs");
            if (!Directory.Exists(logFile))
                Directory.CreateDirectory(logFile);
            logFile = Path.Combine(logFile, "uninstallRelhaxFiles.log");
            if (File.Exists(logFile))
            {
                if (File.Exists(logFile + ".bak"))
                    File.Delete(logFile + ".bak");
                File.Move(logFile, logFile + ".bak");
            }

            //create the uninstall log
            Logging.Manager("creating uninstall log file", true);
            TextWriter tw = new StreamWriter(logFile);
            tw.WriteLine(string.Format(@"/*  Date: {0:yyyy-MM-dd HH:mm:ss}  */", DateTime.Now));
            tw.WriteLine(@"/*  files and folders deleted  */");

            //delete all files and folders from the lists (not shortcuts)
            Logging.Manager("deleting files and folders from totalFiles list", true);
            foreach (string file in totalFiles)
            {
                args.currentFile = file;
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                    tw.WriteLine(file);
                }
                catch (Exception ex)
                {
                    ex = ex.GetBaseException();
                    tw.WriteLine(string.Format(@"/* failed to delete: {0} */",file));
                    Logging.Manager(string.Format("failed to delete: {0} ({1})", file, ex.Message));
                }
                InstallWorker.ReportProgress(args.ChildProcessed++);
            }
            if(Settings.CreateShortcuts)
            {
                //don't delete them for now...
            }
            else
            {
                Logging.Manager("Settings.CreateShortcuts false, deleting totalShortcuts", true);
                foreach (string shortcut in totalShortcuts)
                {
                    int retry = 3;
                    while(retry > 0)
                    {
                        try
                        {
                            File.SetAttributes(shortcut, FileAttributes.Normal);
                            File.Delete(shortcut);
                            retry = 0;
                        }
                        catch
                        {
                            Logging.Manager(string.Format("EXCEPTION CAUGHT at deleting shortcuts, user prolly has the window open, retry={0}, trying again in 100ms...",retry--));
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    tw.WriteLine(shortcut);
                }
            }
            Logging.Manager("deleting leftover folders", true);
            foreach (string folder in totalFolders)
            {
                // fix for uninstall bug 
                if (Directory.Exists(folder) && Directory.GetFiles(folder).Count() == 0 && Directory.GetDirectories(folder).Count() == 0)
                {
                    args.currentFile = folder;
                    DirectoryDeleteNoProgress(folder, false);
                    InstallWorker.ReportProgress(args.ChildProcessed++);
                    tw.WriteLine(folder);
                }
            }
            //wipe the final directories
            Logging.Manager("wiping res_mods", true);
            if (Directory.Exists(Path.Combine(TanksLocation, "res_mods")))
            {
                //Directory.Delete(Path.Combine(TanksLocation, "res_mods"), true);
                DirectoryDeleteNoProgress(Path.Combine(TanksLocation, "res_mods"),true);
                Directory.CreateDirectory(Path.Combine(TanksLocation, "res_mods"));
            }
            tw.WriteLine("res_mods wiped");
            Logging.Manager("wiping mods", true);
            if (Directory.Exists(Path.Combine(TanksLocation, "mods")))
            {
                //Directory.Delete(Path.Combine(TanksLocation, "mods"), true);
                DirectoryDeleteNoProgress(Path.Combine(TanksLocation, "mods"),true);
                Directory.CreateDirectory(Path.Combine(TanksLocation, "mods"));
            }
            tw.WriteLine("mods wiped");
            tw.Close();
            Logging.Manager("deleting old relhaxinstalledfileslog stuffs", true);
            if (File.Exists(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak")))
                File.Delete(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak"));
            if (File.Exists(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log")))
                File.Move(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log"), Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak"));
        }

        //Step 3: Delete all mods (quick)
        public void UninstallModsQuick()
        {
            try
            {
                NumFilesToProcess(Path.Combine(TanksLocation, "res_mods"));
                NumFilesToProcess(Path.Combine(TanksLocation, "mods"));
                InstallWorker.ReportProgress(0);
                //don't forget to delete the readme files
                if (Directory.Exists(Path.Combine(TanksLocation, "_readme")))
                    Directory.Delete(Path.Combine(TanksLocation, "_readme"), true);
                DirectoryDelete(Path.Combine(TanksLocation, "res_mods"), true);
                DirectoryDelete(Path.Combine(TanksLocation, "mods"), true);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("DeleteMods", ex);
            }
        }

        //Step 4: Clear WoT program cache
        public void ClearWoTCache()
        {
            try
            {
                if (AppDataFolder == null || AppDataFolder.Equals("") || AppDataFolder.Equals("-1"))
                {
                    if (AppDataFolder == null) AppDataFolder = "(null)";
                    if (AppDataFolder.Equals("")) AppDataFolder = "(empty string)";
                    Logging.Manager("ERROR: AppDataFolder not correct, value: " + AppDataFolder);
                    Logging.Manager("Aborting ClearWoTCache()");
                    return;
                }
                Logging.Manager("Started clearing of WoT cache files");

                string[] fileFolderNames = { "preferences.xml", "preferences_ct.xml", "modsettings.dat", "xvm", "pmod" };
                // string AppPathTempFolder = Path.Combine(AppPath, "RelHaxTemp", "AppDataBackup");
                string AppPathTempFolder = Path.Combine(Settings.RelhaxTempFolder, "AppDataBackup");

                //1 - Move out prefrences.xml, prefrences_ct.xml, and xvm folder
                try
                {
                    if (!Directory.Exists(AppPathTempFolder))
                        Directory.CreateDirectory(AppPathTempFolder);
                    foreach (var f in fileFolderNames)
                    {
                        if (Directory.Exists(Path.Combine(AppDataFolder, f)))
                        {
                            DirectoryMove(Path.Combine(AppDataFolder, f), Path.Combine(AppPathTempFolder, f), true, true, false);
                        }
                        else if (File.Exists(Path.Combine(AppDataFolder, f)))
                        {
                            File.Move(Path.Combine(AppDataFolder, f), Path.Combine(AppPathTempFolder, f));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("ClearWoTCache", "step 1", ex);
                }

                //2 - recursivly delete entire WorldOfTanks folder
                try
                {
                    NumFilesToProcess(AppDataFolder);
                    DirectoryDelete(AppDataFolder, true);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("ClearWoTCache", "step 2", ex);
                }

                //3 - re-create WorldOfTanks folder and move back 3 above files and delete temp file
                try
                {
                    foreach (var f in fileFolderNames)
                    {
                        if (Directory.Exists(Path.Combine(AppPathTempFolder, f)))
                        {
                            DirectoryMove(Path.Combine(AppPathTempFolder, f), Path.Combine(AppDataFolder, f), true, true, false);
                        }
                        else if (File.Exists(Path.Combine(AppPathTempFolder, f)))
                        {
                            File.Move(Path.Combine(AppPathTempFolder, f), Path.Combine(AppDataFolder, f));
                        }
                    }
                    if (Directory.Exists(AppPathTempFolder))
                        Directory.Delete(AppPathTempFolder);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("ClearWoTCache", "step 3", ex);
                }
                Logging.Manager("Finished clearing of WoT cache files");
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ClearWoTCache", "ex", ex);
            }
        }

        //Step 5-10: Extract All DatabasePackages
        public void ExtractDatabaseObjects()
        {
            try
            {
                //just a double-check to delete all patches
                if (Directory.Exists(Path.Combine(TanksLocation, "_readme")))
                    Directory.Delete(Path.Combine(TanksLocation, "_readme"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_patch")))
                    Directory.Delete(Path.Combine(TanksLocation, "_patch"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_fonts")))
                    Directory.Delete(Path.Combine(TanksLocation, "_fonts"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_xmlUnPack")))
                    Directory.Delete(Path.Combine(TanksLocation, "_xmlUnPack"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_atlases")))
                    Directory.Delete(Path.Combine(TanksLocation, "_atlases"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_shortcuts")))
                    Directory.Delete(Path.Combine(TanksLocation, "_shortcuts"), true);
                if (!Directory.Exists(Path.Combine(TanksLocation, "res_mods")))
                    Directory.CreateDirectory(Path.Combine(TanksLocation, "res_mods"));
                if (!Directory.Exists(Path.Combine(TanksLocation, "mods")))
                    Directory.CreateDirectory(Path.Combine(TanksLocation, "mods"));
                if (!Directory.Exists(Path.Combine(TanksLocation, "logs")))
                    Directory.CreateDirectory(Path.Combine(TanksLocation, "logs"));

                //extract RelHax Mods
                Logging.Manager("Starting Relhax Modpack Extraction");
                string downloadedFilesDir = Settings.RelhaxDownloadsFolder;
                //calculate the total number of zip files to install
                foreach (Dependency d in GlobalDependencies)
                    if (!d.ZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (Dependency d in Dependencies)
                    if (!d.ZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (LogicalDependency d in LogicalDependencies)
                    if (!d.ZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (SelectablePackage dbo in ModsConfigsToInstall)
                    if (!dbo.ZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (Dependency d in AppendedDependencies)
                    if (!d.ZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                InstallWorker.ReportProgress(0);
                //extract global dependencies
                int patchCounter = 0;
                foreach (Dependency d in GlobalDependencies)
                {
                    if (!d.ZipFile.Equals(""))
                    {
                        Logging.Manager("Extracting Global Dependency " + d.ZipFile);
                        try
                        {
                            if(Settings.InstantExtraction)
                            {
                                while (!d.ReadyForInstall)
                                    System.Threading.Thread.Sleep(20);
                            }
                            Unzip(Path.Combine(downloadedFilesDir, d.ZipFile), null,-3,ref patchCounter,d.LogAtInstall);
                            patchCounter++;
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip GlobalDependencies", ex);
                            //show the error message
                            MessageBox.Show(Translations.GetTranslatedString("zipReadingErrorMessage1") + ", " + d.ZipFile + " " + Translations.GetTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //extract dependencies
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractDependencies;
                InstallWorker.ReportProgress(0);
                patchCounter = 0;
                foreach (Dependency d in Dependencies)
                {
                    if (!d.ZipFile.Equals(""))
                    {
                        Logging.Manager("Extracting Dependency " + d.ZipFile);
                        try
                        {
                            if (Settings.InstantExtraction)
                            {
                                while (!d.ReadyForInstall)
                                    System.Threading.Thread.Sleep(20);
                            }
                            Unzip(Path.Combine(downloadedFilesDir, d.ZipFile), null,-2,ref patchCounter,d.LogAtInstall);
                            patchCounter++;
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip Dependencies", ex);
                            //show the error message
                            MessageBox.Show(Translations.GetTranslatedString("zipReadingErrorMessage1") + ", " + d.ZipFile + " " + Translations.GetTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //set xvmConfigDir here because xvm is always a dependency, but don't log it
                xvmConfigDir = PatchUtils.GetXVMBootLoc(TanksLocation, null, false);
                //extract logical dependencies
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractLogicalDependencies;
                InstallWorker.ReportProgress(0);
                patchCounter = 0;
                foreach (LogicalDependency d in LogicalDependencies)
                {
                    if (!d.ZipFile.Equals(""))
                    {
                        Logging.Manager("Extracting Logical Dependency " + d.ZipFile);
                        try
                        {
                            if (Settings.InstantExtraction)
                            {
                                while (!d.ReadyForInstall)
                                    System.Threading.Thread.Sleep(20);
                            }
                            Unzip(Path.Combine(downloadedFilesDir, d.ZipFile), null,-1,ref patchCounter,d.LogAtInstall);
                            patchCounter++;
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip LogicalDependencies", ex);
                            //show the error message
                            MessageBox.Show(Translations.GetTranslatedString("zipReadingErrorMessage1") + ", " + d.ZipFile + " " + Translations.GetTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                if(Settings.SuperExtraction)
                {
                    //extract mods and configs parallel
                    args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractMods;
                    args.ParrentTotalToProcess = InstallGroups.Count;
                    args.ParrentProcessed = 0;
                    args.currentFile = "";
                    args.currentFileSizeProcessed = 0;
                    InstallWorker.ReportProgress(0);
                    int igCounter = 0;
                    foreach(InstallGroup ig in InstallGroups)
                    {
                        using (BackgroundWorker bg = new BackgroundWorker())
                        {
                            bg.DoWork += MulticoreExtract;
                            bg.RunWorkerCompleted += Bg_RunWorkerCompleted;
                            StringBuilder sb = new StringBuilder();
                            object[] args = new object[] { sb, ig.Categories };
                            bg.RunWorkerAsync(args);
                            //Logging.Manager("BackgroundWorker started for Installgroup. Number=" + igCounter++);
                            Logging.Manager(string.Format("BackgroundWorker started for Installgroup={0}, Categories={1}",igCounter++, string.Join(", ",ig.Categories)));
                        }
                    }
                    while (NumExtractorsCompleted != InstallGroups.Count)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
                else
                {
                    //extract mods and configs
                    args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractMods;
                    InstallWorker.ReportProgress(0);
                    foreach (SelectablePackage dbo in ModsConfigsToInstall)
                    {
                        if (!dbo.ZipFile.Equals(""))
                        {
                            Logging.Manager("Extracting Mod/Config " + dbo.ZipFile);
                            try
                            {
                                if (Settings.InstantExtraction)
                                {
                                    while (!dbo.ReadyForInstall)
                                        System.Threading.Thread.Sleep(20);
                                }
                                int wtf = 0;
                                Unzip(Path.Combine(downloadedFilesDir, dbo.ZipFile), null,0, ref wtf,dbo.LogAtInstall);
                                args.ParrentProcessed++;
                            }
                            catch (Exception ex)
                            {
                                //append the exception to the log
                                Utils.ExceptionLog("ExtractDatabaseObjects", "unzip dbo.ZipFile", ex);
                                //show the error message
                                MessageBox.Show(Translations.GetTranslatedString("zipReadingErrorMessage1") + ", " + dbo.ZipFile + " " + Translations.GetTranslatedString("zipReadingErrorMessage3"), "");
                                //exit the application
                                Application.Exit();
                            }
                        }
                        InstallWorker.ReportProgress(0);
                    }
                }
                sw.Stop();
                Logging.Manager("Recorded Install Time for MOD/CONFIG extraction (msec): " + sw.ElapsedMilliseconds);
                //extract dependencies
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractAppendedDependencies;
                InstallWorker.ReportProgress(0);
                patchCounter = 0;
                foreach (Dependency d in AppendedDependencies)
                {
                    if (!d.ZipFile.Equals(""))
                    {
                        Logging.Manager("Extracting Appended Dependency " + d.ZipFile);
                        try
                        {
                            if (Settings.InstantExtraction)
                            {
                                while (!d.ReadyForInstall)
                                    System.Threading.Thread.Sleep(20);
                            }
                            Unzip(Path.Combine(downloadedFilesDir, d.ZipFile), null,TotalCategories,ref patchCounter, d.LogAtInstall);
                            patchCounter++;
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip AppendedDependencies", ex);
                            //show the error message
                            MessageBox.Show(Translations.GetTranslatedString("zipReadingErrorMessage1") + ", " + d.ZipFile + " " + Translations.GetTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //don't actually run this anymore
                /*
                //finish by moving WoTAppData folder contents into application data folder
                //folder name is "WoTAppData"
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractConfigs;
                InstallWorker.ReportProgress(0);
                string folderToMove = Path.Combine(TanksLocation, "WoTAppData");
                if (Directory.Exists(folderToMove))
                {
                    Logging.Manager("WoTAppData folder detected, moving files to WoT cache folder");
                    //get each file and folder and move them
                    // Get the subdirectories for the specified directory
                    DirectoryInfo dir = new DirectoryInfo(folderToMove);
                    DirectoryInfo[] dirs = dir.GetDirectories();
                    // Get the files in the directory
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        //move the file, overwrite if required
                        string temppath = Path.Combine(AppDataFolder, file.Name);
                        args.currentFile = temppath;
                        InstallWorker.ReportProgress(0);
                        if (File.Exists(temppath))
                            File.Delete(temppath);
                        file.MoveTo(temppath);
                    }
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        //call the recursive function to move
                        //the sub dir is actaully the top dir for the function
                        string temppath = Path.Combine(TanksLocation, "WoTAppData", subdir.Name);
                        string temppath2 = Path.Combine(AppDataFolder, subdir.Name);
                        args.currentFile = temppath;
                        InstallWorker.ReportProgress(0);
                        DirectoryMove(temppath, temppath2, true, true, false);
                    }
                    //call the process folders function to delete any leftover folders
                    Utils.ProcessDirectory(folderToMove, false);
                    if (Directory.Exists(folderToMove))
                        Directory.Delete(folderToMove);
                }
                */
                Logging.Manager("Finished Relhax Modpack Extraction");
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ExtractDatabaseObjects", ex);
            }
        }

        //method for each thread if using multicore extract method
        private void MulticoreExtract(object sender, DoWorkEventArgs e)
        {
            string downloadedFilesDir = Settings.RelhaxDownloadsFolder;
            object[] args = (object[])e.Argument;
            StringBuilder sb = (StringBuilder)args[0];
            List<Category> categoriesToExtract = (List<Category>)args[1];
            foreach(Category c in categoriesToExtract)
            {
                //create the category entry so that we know which category it is
                sb.Append("/*  Category: " + c.Name + "  */\n");
                //reset the patch numbering as well for each cagetory for super mode
                //single mode: all one name
                //parallel mode: catagory_patchNum-of-category
                int superPatchNum = 0;
                foreach(SelectablePackage m in c.Packages)
                {
                    if(m.Enabled && m.Checked)
                    {
                        if(!m.ZipFile.Equals(""))
                        {
                            sb.Append("/*  " + m.ZipFile + "  */\n");
                            if (Settings.InstantExtraction)
                            {
                                while (!m.ReadyForInstall)
                                    System.Threading.Thread.Sleep(20);
                            }
                            Logging.Manager("Extraction started  of file " + m.ZipFile + ", superPatchNum=" + superPatchNum);
                            //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/ref
                            //because multi-threading and recursion wern't hard enough...
                            Unzip(Path.Combine(downloadedFilesDir, m.ZipFile), sb, m.ParentCategory.InstallGroup, ref superPatchNum, m.LogAtInstall);
                            Logging.Manager("Extraction finished of file " + m.ZipFile + ", superPatchNum=" + superPatchNum);
                        }
                        if(m.Packages.Count > 0)
                        {
                            MulticoreExtractConfigs(m.Packages,downloadedFilesDir, sender, sb, ref superPatchNum);
                        }
                    }
                }
                Logging.Installer(sb.ToString().Substring(0, sb.ToString().Length - 1));
                sb.Clear();
            }
        }

        //method like above, but is for the next (and all) levels down the tree for each package inside a package
        private void MulticoreExtractConfigs(List<SelectablePackage> configsToExtract, string downloadedFilesDir, object sender, StringBuilder sb, ref int superPatchNum)
        {
            foreach (SelectablePackage config in configsToExtract)
            {
                if (config.Enabled && config.Checked)
                {
                    if (!config.ZipFile.Equals(""))
                    {
                        sb.Append("/*  " + config.ZipFile + "  */\n");
                        if (Settings.InstantExtraction)
                        {
                            while (!config.ReadyForInstall)
                                System.Threading.Thread.Sleep(20);
                        }
                        Logging.Manager("Extraction started  of file " + config.ZipFile + ", superPatchNum=" + superPatchNum);
                        Unzip(Path.Combine(downloadedFilesDir, config.ZipFile), sb, config.ParentCategory.InstallGroup, ref superPatchNum, config.LogAtInstall);
                        Logging.Manager("Extraction finished of file " + config.ZipFile + ", superPatchNum=" + superPatchNum);
                    }
                    if(config.Packages.Count > 0)
                    {
                        MulticoreExtractConfigs(config.Packages,downloadedFilesDir, sender, sb, ref superPatchNum);
                    }
                }
            }
        }

        //Step 11: Restore User Data
        public void RestoreUserData()
        {
            try
            {
                Logging.InstallerGroup("RestoreUserData");
                args.ParrentTotalToProcess = ModsConfigsWithData.Count;
                InstallWorker.ReportProgress(0);
                foreach (SelectablePackage dbo in ModsConfigsWithData)
                {
                    try
                    {
                        args.ChildTotalToProcess = dbo.UserFiles.Count;
                        int c = 0;
                        foreach (string s in dbo.UserFiles)
                        {
                            try {
                                string correctedUserFiles = s.TrimStart('\x005c').Replace(@"\\", @"\");
                                string targetDir = "";
                                if (correctedUserFiles[0].Equals("{"))
                                {
                                    correctedUserFiles = Utils.ReplaceMacro(correctedUserFiles);
                                    targetDir = Path.GetDirectoryName(correctedUserFiles);
                                }
                                else
                                {
                                    correctedUserFiles = Utils.ReplaceMacro(correctedUserFiles);
                                    targetDir = Path.Combine(TanksLocation, Path.GetDirectoryName(correctedUserFiles));
                                }
                                try
                                {
                                    // check if target dir is existing. if not, create it
                                    if (!Directory.Exists(targetDir))
                                    {
                                        Directory.CreateDirectory(targetDir);
                                        Logging.Installer(targetDir);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logging.Manager(string.Format("failed to create folder: {0} ({1})", targetDir, ex.Message));
                                }
                                args.currentFile = correctedUserFiles;
                                InstallWorker.ReportProgress(0);
                                string filenamePrefix = Utils.GetValidFilename(dbo.Name + "_" + c + "_");
                                c++;
                                //find the files with the specified pattern
                                string[] fileList = Directory.GetFiles(Settings.RelhaxTempFolder, filenamePrefix + Path.GetFileName(correctedUserFiles));
                                //if no results, go on with the next entry
                                if (fileList.Length == 0) continue;
                                args.Filecounter = 0;
                                args.FilesToDo = fileList.Length;
                                foreach (string ss in fileList)
                                {
                                    args.Filecounter++;
                                    string targetFilename = Path.Combine(targetDir, Path.GetFileName(ss).Replace(filenamePrefix, ""));
                                    try
                                    {
                                        //the file has been found in the temp directory
                                        if (File.Exists(targetFilename))
                                            File.Delete(targetFilename);
                                        File.Move(Path.Combine(Settings.RelhaxTempFolder, Path.GetFileName(ss)), targetFilename);
                                        Logging.Installer(targetFilename);
                                        // do not log files if count is greater then 5
                                        if (fileList.Length < 5) Logging.Manager(string.Format("RestoredUserData: {0}", targetFilename));
                                    }
                                    catch (Exception p)
                                    {
                                        Utils.ExceptionLog("RestoreUserData", "p\n" + ss, p);
                                    }
                                    InstallWorker.ReportProgress(0);
                                }
                                // log proceeded files if count is greater then 5
                                if (!(fileList.Length < 5)) Logging.Manager(string.Format("RestoredUserData: {0} files ({1})", args.Filecounter, correctedUserFiles));
                                args.ChildProcessed++;
                                InstallWorker.ReportProgress(0);
                            }
                            catch (Exception fl)
                            {
                                Utils.ExceptionLog("RestoreUserData", "fl", fl);
                            }
                        }
                        args.ParrentProcessed++;
                        InstallWorker.ReportProgress(0);
                    }
                    catch (Exception uf)
                    {
                        Utils.ExceptionLog("RestoreUserData", "uf", uf);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("RestoreUserData", "ex", ex);
            }
        }

        //Step 12: Unpack Xml Files
        private void UnpackXmlFiles()
        {
            try
            {
                DirectoryInfo di = null;
                FileInfo[] diArr = null;
                try
                {
                    File.SetAttributes(Path.Combine(TanksLocation, "_xmlUnPack"), FileAttributes.Normal);
                    di = new DirectoryInfo(Path.Combine(TanksLocation, "_xmlUnPack"));
                    //get every patch file in the folder
                    diArr = di.GetFiles(@"*.xml", SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("UnpackXmlFiles", "parse _xmlUnPack folder", ex);

                }

                XmlUnpackList = new List<XmlUnpack>();
                for (int i = 0; i < diArr.Count(); i++)
                {
                    //set the attributes to normal
                    File.SetAttributes(diArr[i].FullName, FileAttributes.Normal);
                    //add jobs to xmlUnpackList
                    CreateXmlUnpackList(diArr[i].FullName);
                }
                if (XmlUnpackList.Count > 0)
                {
                    Logging.InstallerGroup("unpacked XML files");            // write comment line
                }
                args.ChildTotalToProcess = XmlUnpackList.Count;
                args.ChildProcessed = 0;
                foreach (XmlUnpack r in XmlUnpackList)
                {
                    string fn = r.NewFileName.Equals("") ? r.FileName : r.NewFileName;
                    args.currentFile = fn;
                    try
                    {
                        if (!Directory.Exists(r.ExtractDirectory)) Directory.CreateDirectory(r.ExtractDirectory);
                        if (r.Pkg.Equals(""))
                        {
                            try
                            {
                                // if value of pkg is empty, it is not contained in an archive
                                if (File.Exists(Path.Combine(r.DirectoryInArchive, r.FileName)))
                                {
                                    File.Copy(Path.Combine(r.DirectoryInArchive, r.FileName), Path.Combine(r.ExtractDirectory, fn), false);     // no overwrite of an exsisting file !!
                                }
                                else
                                {
                                    if (Settings.ExportMode)
                                    {
                                        Logging.Manager(string.Format("WARNING: file {0} not found, but most likley expected due to export mode", Path.Combine(r.DirectoryInArchive, r.FileName)));
                                    }
                                    else
                                    {
                                        Logging.Manager(string.Format("ERROR: file {0} not found!", Path.Combine(r.DirectoryInArchive, r.FileName)));
                                    }
                                }
                                    // Utils.AppendToInstallLog(Path.Combine(r.extractDirectory, fn));
                                    Logging.Installer(Path.Combine(r.ExtractDirectory, fn));            // write created file with path
                                Logging.Manager(string.Format("{0} moved", r.FileName));
                            }
                            catch (Exception ex)
                            {
                                Utils.ExceptionLog("UnpackXmlFiles", string.Format("copy: {0}", Path.Combine(r.ExtractDirectory, fn)), ex);
                            }
                        }
                        else
                        {
                            //get file from the zip archive
                            if(File.Exists(r.Pkg))
                            {
                                using (ZipFile zip = new ZipFile(r.Pkg))
                                {
                                    for (int i = 0; i < zip.Entries.Count; i++)
                                    {
                                        if (Regex.IsMatch(zip[i].FileName, Path.Combine(r.DirectoryInArchive, r.FileName).Replace(@"\", @"/")))
                                        {
                                            try
                                            {
                                                zip[i].FileName = fn;
                                                if (File.Exists(Path.Combine(r.ExtractDirectory, zip[i].FileName)))
                                                {
                                                    Logging.Manager(string.Format("File {0} already exists, so no extraction/overwrite", Path.Combine(r.ExtractDirectory, zip[i].FileName)));
                                                }
                                                else
                                                {
                                                    //when possible please use other methods than throwing exceptions
                                                    zip.ExtractSelectedEntries(zip[i].FileName, null, r.ExtractDirectory, ExtractExistingFileAction.DoNotOverwrite);  // no overwrite of an exsisting file !!
                                                    Logging.Installer(Path.Combine(r.ExtractDirectory, fn));
                                                    Logging.Manager(string.Format("{0} extracted", zip[i].FileName));
                                                    // break;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Utils.ExceptionLog("UnpackXmlFiles", string.Format("extration: {0}", Path.Combine(r.ExtractDirectory, zip[i].FileName)), ex);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Settings.ExportMode)
                                {
                                    Logging.Manager(string.Format("WARNING: package {0} not found, but most likley expected due to export mode", r.Pkg));
                                }
                                else
                                {
                                    Logging.Manager(string.Format("ERROR: package {0} not found!", r.Pkg));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog(string.Format("UnpackXmlFiles", "extract file from archive\ndirectoryInArchive: {0}\nfileName: {1}\nextractDirectory: {2}\nnewFileName: {3}", r.DirectoryInArchive, r.FileName, r.ExtractDirectory, r.NewFileName), ex);
                    }
                    try
                    {
                        XmlBinary.XmlBinaryHandler xmlUnPack = new XmlBinary.XmlBinaryHandler();
                        xmlUnPack.unPack(Path.Combine(r.ExtractDirectory, fn));
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog(string.Format("UnpackXmlFiles", "xmlUnPack\nfileName: {0}", Path.Combine(r.ExtractDirectory, fn)), ex);
                    }
                    args.ChildProcessed++;
                    InstallWorker.ReportProgress(0);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("UnpackXmlFiles", ex);
            }
        }

        //Step 13/16: Patch All files
        public void PatchFiles()
        {
            try
            {
                //Give the OS time to process the folder change...
                System.Threading.Thread.Sleep(5);
                //set the folder properties to read write
                DirectoryInfo di = null;
                FileInfo[] diArr = null;
                bool loop = false;
                while (!loop)
                {
                    try
                    {
                        File.SetAttributes(Path.Combine(TanksLocation, "_patch"), FileAttributes.Normal);
                        di = new DirectoryInfo(Path.Combine(TanksLocation, "_patch"));
                        //get every patch file in the folder
                        diArr = di.GetFiles(@"*.xml", System.IO.SearchOption.TopDirectoryOnly);
                        loop = true;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Utils.ExceptionLog("PatchFiles", e);
                        DialogResult res = MessageBox.Show(Translations.GetTranslatedString("patchingSystemDeneidAccessMessage"), Translations.GetTranslatedString("patchingSystemDeneidAccessHeader"), MessageBoxButtons.RetryCancel);
                        if (res == DialogResult.Cancel)
                        {
                            Application.Exit();
                        }
                    }
                }

                //get any other old patches out of memory
                PatchList = new List<Patch>();
                for (int i = 0; i < diArr.Count(); i++)
                {
                    // if the file is NotFiniteNumberException an xml file, skip it
                    if (!Path.GetExtension(diArr[i].FullName).ToLower().Equals(".xml".ToLower())) continue;
                    //set the attributes to normal
                    File.SetAttributes(diArr[i].FullName, FileAttributes.Normal);
                    // add patches to patchList
                    // modify the xml filename for logging purpose
                    string nativeProcessingFile = Path.GetFileNameWithoutExtension(diArr[i].Name);
                    string actualPatchName = originalSortedPatchNames[nativeProcessingFile];
                    Logging.Manager(string.Format("Adding patches from file: {0}", actualPatchName));
                    CreatePatchList(diArr[i].FullName);
                }
                args.ParrentTotalToProcess = PatchList.Count;
                args.ParrentProcessed = 0;
                //the actual patch method
                string oldNativeProcessingFile = "";
                foreach (Patch p in PatchList)
                {
                    args.currentFile = p.file;
                    InstallWorker.ReportProgress(0);
                    if (!oldNativeProcessingFile.Equals(p.nativeProcessingFile))
                    {
                        Logging.Manager(string.Format("nativeProcessingFile: {0}, originalName: {1}", p.nativeProcessingFile, p.actualPatchName));
                        oldNativeProcessingFile = p.nativeProcessingFile;
                    }
                    string patchFileOutput = p.file;
                    int maxLength = 200;
                    if (p.file.Length > maxLength)
                        patchFileOutput = p.file.Substring(0, maxLength);
                    Application.DoEvents();
                    //create the path to the file based on the patchPath
                    //in createPatchList all patch strings defined as at least nothing (""), no nulls here
                    p.completePath = "";
                    //legacy compatibility: if patchPath is null or whitespace or nothing, default to app
                    if (string.IsNullOrWhiteSpace(p.patchPath))
                        p.patchPath = "app";
                    //replace accidental "\\" with "\" (was a legacy issue)
                    p.file = p.file.Replace(@"\\", @"\");
                    switch(p.patchPath)
                    {
                        case "app":
                        case "{app}":
                            //TanksLocation (World_of_Tanks)
                            //p.completePath = Path.Combine(TanksLocation, p.file);
                            p.completePath = TanksLocation + p.file;
                            break;
                        case "appData":
                        case "{appData}":
                            //app data location
                            //p.completePath = Path.Combine(AppDataFolder, p.file);
                            p.completePath = AppDataFolder + p.file;
                            break;
                    }
                    if (p.completePath.Contains("versiondir"))
                        p.completePath = p.completePath.Replace("versiondir", TanksVersion);
                    if (p.completePath.Contains("xvmConfigFolderName"))
                    {
                        string s = PatchUtils.GetXVMBootLoc(TanksLocation);
                        if (s != null)
                            p.completePath = p.completePath.Replace("xvmConfigFolderName", s);
                    }
                    string patchParameters = string.Format("mode={0}, filePathMacro={1}, filePath={2}, search={3}, replace={4}", p.mode, p.patchPath, p.file, p.search, p.replace);
                    if (p.type.Equals("regx") || p.type.Equals("regex"))
                    {
                        Logging.Manager("regex section", true);
                        string temp = null;
                        int tempp = 0;
                        if (p.lines != null)
                        {
                            patchParameters = patchParameters + string.Format(", lines={0}", string.Join(",", p.lines));
                            temp = p.lines[0];
                            tempp = int.Parse(temp);
                        }
                        if (p.lines == null)
                        {
                            //perform regex patch on entire file, line by line
                            Logging.Manager("Regex patch, all lines, line by line, " + patchParameters);
                            PatchUtils.RegxPatch(p);
                        }
                        else if (p.lines.Count() == 1 && tempp == -1)
                        {
                            //perform regex patch on entire file, as one whole string
                            Logging.Manager("Regex patch, all lines, whole file, " + patchParameters);
                            PatchUtils.RegxPatch(p, -1);
                        }
                        else
                        {
                            foreach (string s in p.lines)
                            {
                                //perform regex patch on specific file lines
                                //will need to be a standard for loop BTW
                                Logging.Manager("Regex patch, line " + s + ", " + patchParameters);
                                PatchUtils.RegxPatch(p, int.Parse(s));
                            }
                        }
                    }
                    else if (p.type.Equals("xml"))
                    {
                        Logging.Manager("xml section", true);
                        patchParameters = patchParameters + string.Format(", path={0}", string.Join(",", p.path));
                        //perform xml patch
                        Logging.Manager("Xml patch, " + patchParameters);
                        PatchUtils.XMLPatch(p);
                    }
                    else if (p.type.Equals("json"))
                    {
                        Logging.Manager("json section", true);
                        patchParameters = patchParameters + string.Format(", path={0}", string.Join(",", p.path));
                        //perform json patch
                        Logging.Manager("Json patch, " + patchParameters);
                        PatchUtils.JSONPatch(p);
                    }
                    else if (p.type.Equals("xvm"))
                    {
                        Logging.Manager("xvm section", true);
                        patchParameters = patchParameters + string.Format(", path={0}", string.Join(",", p.path));
                        //perform xvm style json patch
                        Logging.Manager("XVM patch, " + patchParameters);
                        PatchUtils.XVMPatch(p);
                    }
                    else if (p.type.Equals("pmod"))
                    {
                        Logging.Manager("pmod section", true);
                        patchParameters = patchParameters + string.Format(", path={0}", string.Join(",", p.path));
                        //perform pmod/generic style json patch
                        Logging.Manager("PMOD/Generic patch, " + patchParameters);
                        PatchUtils.PMODPatch(p);
                    }
                    args.ParrentProcessed++;
                    InstallWorker.ReportProgress(0);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("PatchFiles", "ex", ex);
            }
        }

        //Step 17/18: Extract/Create Atlases
        private void ExtractAtlases()
        {
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            try
            {
                DirectoryInfo di = null;
                FileInfo[] diArr = null;
                try
                {
                    File.SetAttributes(Path.Combine(TanksLocation, "_atlases"), FileAttributes.Normal);
                    di = new DirectoryInfo(Path.Combine(TanksLocation, "_atlases"));
                    //get every patch file in the folder
                    diArr = di.GetFiles(@"*.xml", SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("ExtractAtlases", "parse _atlases folder", ex);
                }

                AtlasesList = new List<Atlas>();
                for (int i = 0; i < diArr.Count(); i++)
                {
                    //set the attributes to normal
                    File.SetAttributes(diArr[i].FullName, FileAttributes.Normal);
                    //add jobs to CreateAtlasesList
                    CreateAtlasesList(diArr[i].FullName);
                }
                Installer.args.ParrentTotalToProcess = AtlasesList.Count;

                // make sure we have our list of importers
                AtlasesCreator.Handlers.Load();

                foreach (Atlas a in AtlasesList)
                {
                    try
                    {
                        Installer.args.ParrentProcessed++;
                        //example of atlasSaveDirectory: F:\\TANKS\\World_of_Tanks\\res_mods\\0.9.20.1.3\\gui\\flash\\atlases
                        if (!Directory.Exists(a.AtlasSaveDirectory)) Directory.CreateDirectory(a.AtlasSaveDirectory);
                        // Utils.AppendToInstallLog(Path.Combine(a.atlasSaveDirectory));
                        Logging.Installer(Utils.ReplaceDirectorySeparatorChar(Utils.AddTrailingBackslashChar(Path.Combine(a.AtlasSaveDirectory))));                      // write used folder
                        //workingFolder example: "F:\\Tanks Stuff\\RelicModManager\\RelicModManager\\bin\\Debug\\RelHaxTemp\\battleAtlas"
                        //if (!Directory.Exists(a.workingFolder)) Directory.CreateDirectory(a.workingFolder); 

                        // get the right imageHandler for this atlas
                        string imageExtension = Path.GetExtension(a.AtlasFile).Substring(1).ToLower();
                        foreach (var handler in AtlasesCreator.Handlers.ImageHandlers)
                        {
                            if (handler.ImageExtension.ToLower() == imageExtension)
                            {
                                a.imageHandler = handler;
                                break;
                            }
                        }

                        // get the right imageHandler for this atlas
                        foreach (var exporter in AtlasesCreator.Handlers.MapExporters)
                        {
                            if (exporter.MapType.Equals(a.mapType))
                            {
                                a.mapExporter = exporter;
                                a.MapFile = Path.GetFileNameWithoutExtension(a.AtlasFile) + "." + a.mapExporter.MapExtension.ToLower();
                                break;
                            }
                        }

                        if (a.mapExporter == null && a.mapType != Atlas.MapType.None)
                        {
                            Logging.Manager("Error: no mapExporter found for " + a.AtlasFile);
                            break;
                        }

                        string[] fileList = new string[] { a.AtlasFile, a.MapFile };
                        if (!a.Pkg.Equals(""))
                        {
                            //get file from the zip archive
                            using (ZipFile zip = new ZipFile(a.Pkg))
                            {
                                int numFound = 0;
                                for (int i = 0; i < zip.Entries.Count; i++)
                                {
                                    foreach (string fl in fileList)
                                    {
                                        if (Regex.IsMatch(zip[i].FileName, Path.Combine(a.DirectoryInArchive, fl).Replace(@"\", @"/")))
                                        {
                                            try
                                            {
                                                zip[i].FileName = fl;
                                                zip.ExtractSelectedEntries(zip[i].FileName, null, a.TempAltasPresentDirectory, ExtractExistingFileAction.OverwriteSilently);  // never overwrite of an exsisting file ???
                                                numFound++;
                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                Utils.ExceptionLog("ExtractAtlases", string.Format("extraction: {0}", Path.Combine(a.TempAltasPresentDirectory, zip[i].FileName)), ex);
                                            }
                                        }
                                    }
                                    //finishing early saves not needed cpu processing
                                    if (numFound == fileList.Count())
                                        break;
                                }
                            }
                        }
                        else                  // if no pkg file, then the image is accessable directly, so simple copy it
                        {
                            foreach (string fl in fileList)
                            {
                                string source = Path.Combine(a.DirectoryInArchive, fl);
                                string target = Path.Combine(a.TempAltasPresentDirectory, fl);
                                try
                                {
                                    if (File.Exists(source))
                                        File.Copy(source, target, true);        // overwrite existing file if needed
                                }
                                catch (Exception ex)
                                {
                                    Utils.ExceptionLog("ExtractAtlases", string.Format("copy file {0} to location {1}", source, target), ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog(string.Format("ExtractAtlases", "extract file from archive\ndirectoryInArchive: {0}\natlasFile: {1}\nmapFile: {2}\natlasSaveDirectory: {3}", a.DirectoryInArchive, a.AtlasFile, a.MapFile, a.AtlasSaveDirectory), ex);
                    }
                }
                if (AtlasesList.Count > 0)
                {
                    args.InstalProgress = InstallerEventArgs.InstallProgress.CreateAtlases;
                    args.ParrentTotalToProcess = AtlasesList.Count;
                    args.ParrentProcessed = 0;
                    //4 steps per atlas (extract, optimize, build, map)
                    args.ChildTotalToProcess = AtlasesList.Count * 3;
                    args.ChildProcessed = 0;
                    InstallWorker.ReportProgress(0);

                    foreach(Atlas a in AtlasesList)
                    {
                        //just in case
                        NumAtlasCreatorsComplete = 0;
                        try
                        {
                            //create async process for creating each atlas
                            //
                            using (BackgroundWorker bg = new BackgroundWorker())
                            {
                                bg.WorkerReportsProgress = true;
                                bg.DoWork += CreateAtlasesAsync;
                                bg.RunWorkerAsync(a);
                            }
                        }
                        catch (Exception ex)
                        {
                            Utils.ExceptionLog(string.Format("ExtractAtlases", "atlasFile: {0}", Path.Combine(a.AtlasSaveDirectory, a.AtlasFile)), ex);
                        }
                    }
                    while (NumAtlasCreatorsComplete < AtlasesList.Count)
                    {
                        System.Threading.Thread.Sleep(20);
                    }
                    sw.Stop();
                    Logging.Manager("All atlas images created in (msec): " + sw.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ExtractAtlases", "root", ex);
            }
        }

        private void CreateAtlasesAsync(object sender, DoWorkEventArgs e)
        {
            Atlas a = (Atlas)e.Argument;
            ActuallyCreateAtlasesAsync(a);
        }

        //Step 18: Create Atlases
        private void ActuallyCreateAtlasesAsync(Atlas a)
        {
            Size os = ExtractAtlases_run(a);        // os = original size of the extracted Bitmap
            args.ChildProcessed++;
            InstallWorker.ReportProgress(0);
            Atlas atlasesArgs = new Atlas
            {
                AtlasHeight = a.AtlasHeight,
                AtlasWidth = a.AtlasWidth,
                AtlasFile = Path.Combine(a.AtlasSaveDirectory, a.AtlasFile),
                imageHandler = a.imageHandler,
                MapFile = Path.Combine(a.AtlasSaveDirectory, a.MapFile),
                mapExporter = a.mapExporter,
                AtlasSaveDirectory = a.AtlasSaveDirectory,
                GenerateMap = a.GenerateMap,
                mapType = a.mapType,
                PowOf2 = a.PowOf2,
                Square = a.Square,
                FastImagePacker = a.FastImagePacker,
                Padding = a.Padding
            };

            // if the arguments in width and/or height of the atlases-creator-config-xml-file are 0 (or below) or not given, work with the original file dimensions to get working width and height
            if ((atlasesArgs.AtlasHeight < 1) | (atlasesArgs.AtlasWidth < 1))
            {
                if (atlasesArgs.AtlasWidth < 1)
                    atlasesArgs.AtlasWidth = os.Width;

                if (atlasesArgs.AtlasHeight < 1)
                    atlasesArgs.AtlasHeight = os.Height;

                
                if ((os.Height * os.Width) == (atlasesArgs.AtlasWidth * atlasesArgs.AtlasHeight))
                {
                    atlasesArgs.AtlasHeight = (int)(atlasesArgs.AtlasHeight * 1.5);
                }
                else
                {
                    // this is to be shure that the image size that will be created, is at least the original size
                    while ((os.Height * os.Width) > (atlasesArgs.AtlasWidth * atlasesArgs.AtlasHeight))
                        atlasesArgs.AtlasHeight = (int)(atlasesArgs.AtlasHeight * 1.2);
                }
            }
            if ((os.Height * os.Width) > (atlasesArgs.AtlasWidth * atlasesArgs.AtlasHeight))
                Logging.Manager(string.Format("WARNING! definied {0} size is smaller then original size\noriginal h x w: {1} x {2}\ndefinied h x w: {3} x {4}", a.AtlasFile, os.Height, os.Width, atlasesArgs.AtlasHeight, atlasesArgs.AtlasWidth));
            else
                Logging.Manager(string.Format("defined max size of {0} with: {1} (h) x {2} (w)", a.AtlasFile, atlasesArgs.AtlasHeight, atlasesArgs.AtlasWidth));

            List<string> fl = new List<string>();
            fl.AddRange(a.ImageFolderList);

            //temp to get working proof of concept
            //only pass in the same bitmaps

            //CHANGE THIS TO LIST OF TEXTURES WITH MODS
            atlasesArgs.TextureList = ParseFilesForAtlasList(a.TextureList, fl.ToArray(), a.AtlasFile);

            AtlasesCreator.Program.Run(atlasesArgs);
            lock (lockerInstaller)
            {
                NumAtlasCreatorsComplete++;
                args.ParrentProcessed++;
                InstallWorker.ReportProgress(0);
            }
        }
        
        //Step 19: Install Fonts
        public void InstallFonts()
        {
            try
            {
                Logging.Manager("Checking for fonts to install");
                if (!Directory.Exists(Path.Combine(TanksLocation, "_fonts")))
                {
                    Logging.Manager("No fonts to install");
                    //no fonts to install, done display
                    return;
                }
                string[] fonts = Directory.GetFiles(Path.Combine(TanksLocation, "_fonts"), @"*.*",System.IO.SearchOption.TopDirectoryOnly);
                if (fonts.Count() == 0)
                {
                    //done display
                    Logging.Manager("No fonts to install");
                    return;
                }
                //load fonts and move names to a list
                List<String> fontsList = new List<string>();
                foreach (string s in fonts)
                {
                    //load the font into a temporoary not loaded font collection
                    fontsList.Add(Path.GetFileName(s));
                }
                try
                {


                    //removes any already installed fonts
                    for (int i = 0; i < fontsList.Count; i++)
                    {
                        //get the name of the font
                        string[] fontsCollection = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), @"*.*", SearchOption.TopDirectoryOnly);
                        {
                            //get a list of installed fonts
                            foreach (var fontFilename in fontsCollection)
                            {
                                //check if the font filename is installed
                                if (Path.GetFileName(fontFilename).ToLower().Equals(fontsList[i].ToLower()))
                                {
                                    fontsList.RemoveAt(i);
                                    i--;
                                    break;
                                }
                            }
                        }
                    }
                    //re-check the fonts to install list
                    if (fontsList.Count == 0)
                    {
                        Logging.Manager("No fonts to install");
                        //done display
                        return;
                    }
                    Logging.Manager("Installing fonts: " + string.Join(", ", fontsList));
                    DialogResult dr = DialogResult.No;
                    if (Program.autoInstall)
                    {
                        //assume rights to install
                        dr = DialogResult.Yes;
                    }
                    else
                    {
                        dr = MessageBox.Show(Translations.GetTranslatedString("fontsPromptInstallText"), Translations.GetTranslatedString("fontsPromptInstallHeader"), MessageBoxButtons.YesNo);
                    }
                    if (dr == DialogResult.Yes)
                    {

                        string fontRegPath = Path.Combine(TanksLocation, "_fonts", "FontReg.exe");
                        if (!File.Exists(fontRegPath))
                        {
                            if (!Program.testMode)                  // if not in testMode, the managerInfoDatFile was downloaded
                            {
                                //get fontreg from the zip file
                                using (ZipFile zip = new ZipFile(Settings.ManagerInfoDatFile))
                                {
                                    zip.ExtractSelectedEntries("FontReg.exe", null, Path.GetDirectoryName(fontRegPath));
                                }
                            }
                            else
                            {
                                // in testMode, the managerInfoDatFile was NOT downloaded and that have to be done now
                                try
                                {
                                    using (WebClient downloader = new WebClient())
                                    downloader.DownloadFile("http://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/external/FontReg.exe", fontRegPath);
                                }
                                catch (WebException ex)
                                {
                                    Utils.ExceptionLog("InstallFonts()", "download FontReg.exe", ex);
                                    MessageBox.Show(string.Format("{0} FontReg.exe", Translations.GetTranslatedString("failedToDownload_1")));
                                }
                            }
                        }
                        ProcessStartInfo info = new ProcessStartInfo
                        {
                            FileName = fontRegPath,
                            UseShellExecute = true,
                            Verb = "runas", // Provides Run as Administrator
                            Arguments = "/copy",
                            WorkingDirectory = Path.Combine(TanksLocation, "_fonts")
                        };
                        try
                        {
                            Process installFontss = new Process
                            {
                                StartInfo = info
                            };
                            installFontss.Start();
                            installFontss.WaitForExit();
                            Logging.Manager("FontReg.exe ExitCode: " + installFontss.ExitCode);
                        }
                        catch (Exception e)
                        {
                            Utils.ExceptionLog("InstallFonts", "could not start font installer", e);
                            MessageBox.Show(Translations.GetTranslatedString("fontsPromptError_1") + TanksLocation + Translations.GetTranslatedString("fontsPromptError_2"));
                            Logging.Manager("Installation done, but fonts install failed");
                            return;
                        }
                        Logging.Manager("Fonts Installed Successfully");
                        return;
                    }
                    else
                    {
                        Logging.Manager("Installation done, but fonts install failed");
                        return;
                    }
                }
                finally
                {
                    //System.Threading.Thread.Sleep(20);
                    
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("InstallFonts()", ex);
            }
        }

        //Step 15: Extract User Mods
        public void ExtractUserMods()
        {
            int tempPatchNum = 0;
            try
            {
                //set xvm dir location again in case it's just a user mod install
                if (xvmConfigDir == null || xvmConfigDir.Equals(""))
                    xvmConfigDir = PatchUtils.GetXVMBootLoc(TanksLocation);
                //extract user mods
                Logging.Manager("Starting Relhax Modpack User Mod Extraction");
                string downloadedFilesDir = Path.Combine(Application.StartupPath, "RelHaxUserMods");
                foreach (SelectablePackage m in UserMods)
                {
                    if (m.Enabled && m.Checked)
                    {
                        Logging.Manager("Extracting " + Path.GetFileName(m.ZipFile));
                        Unzip(Path.Combine(downloadedFilesDir, Path.GetFileName(m.ZipFile)), null,99,ref tempPatchNum,m.LogAtInstall);
                        tempPatchNum++;
                        InstallWorker.ReportProgress(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ExtractUserMods", ex);
            }
            Logging.Manager("Finished Relhax Modpack User Mod Extraction");
        }

        //Step 18: Create Shortcuts
        private void CreateShortcuts()
        {
            try
            {
                Logging.InstallerGroup("Started Processing of Desktop shortcuts");                     // write comment line
                //create shortcuts list
                //Give the OS time to process the folder change...
                System.Threading.Thread.Sleep(5);
                //set the folder properties to read write
                DirectoryInfo di = null;
                FileInfo[] diArr = null;
                try
                {
                    File.SetAttributes(Path.Combine(TanksLocation, "_shortcuts"), FileAttributes.Normal);
                    di = new DirectoryInfo(Path.Combine(TanksLocation, "_shortcuts"));
                    //get every patch file in the folder
                    diArr = di.GetFiles(@"*.xml", System.IO.SearchOption.TopDirectoryOnly);
                }
                catch (UnauthorizedAccessException e)
                {
                    Utils.ExceptionLog("CreateShortCuts(), File.SetAttributes", e);
                }

                //get any other old shortcuts out of memory
                Shortcuts = new List<Shortcut>();
                for (int i = 0; i < diArr.Count(); i++)
                {
                    //set the attributes to normall
                    File.SetAttributes(diArr[i].FullName, FileAttributes.Normal);
                    //add shortcuts to the list
                    CreateShortcutsList(diArr[i].FullName);
                }
                args.ParrentTotalToProcess = Shortcuts.Count;
                args.ParrentProcessed = 0;
                //create the new list of shortcuts by filename only
                //compare with list of shortcuts from default uninstall
                //if in default uninstall (before) but not in new list (after) then delete
                //otherwise it will be updated below
                if(Settings.UninstallMode == UninstallModes.Default)
                {
                    List<string> totalNewShortcuts = new List<string>();
                    foreach (Shortcut sc in Shortcuts)
                    {
                        totalNewShortcuts.Add(Path.GetFileNameWithoutExtension(sc.Name) + ".lnk");
                    }
                    List<string> shortcutsToDelete = totalShortcuts.Except(totalNewShortcuts).ToList();
                    if(shortcutsToDelete.Count > 0)
                    {
                        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        foreach (string s in shortcutsToDelete)
                        {
                            string completePath = Path.Combine(desktop, s + ".lnk");
                            File.SetAttributes(completePath, FileAttributes.Normal);
                            File.Delete(completePath);
                        }
                    }
                }

                foreach (Shortcut sc in Shortcuts)
                {
                    if (sc.Enabled)
                    {
                        string fileTarget = Utils.ReplaceDirectorySeparatorChar(Utils.ReplaceMacro(sc.Path));
                        if (File.Exists(fileTarget))
                        {
                            Logging.Manager(string.Format("creating desktop Shortcut: {0} ({1})", fileTarget, sc.Name));
                            Utils.CreateShortcut(fileTarget, sc.Name, true, true);
                        }
                    }
                    args.ParrentProcessed++;
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("CreateShortCuts", ex);
            }
        }

        //Step 19: Check the Database for outdated or no more needed files
        private void CheckForOldZipFiles()
        {
            try
            {
                args.ParrentTotalToProcess = 3;
                args.ParrentProcessed = 1;
                List<string> zipFilesList = new List<string>();
                FileInfo[] fi = null;
                try
                {
                    File.SetAttributes(Settings.RelhaxDownloadsFolder, FileAttributes.Normal);
                    DirectoryInfo di = new DirectoryInfo(Settings.RelhaxDownloadsFolder);
                    //get every zip file in the folder
                    fi = di.GetFiles(@"*.zip", SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("checkForOldZipFiles", "parse RelHaxDownloads folder", ex);
                    MessageBox.Show(string.Format(Translations.GetTranslatedString("parseDownloadFolderFailed"), "RelHaxDownloads"));
                }
                args.ParrentProcessed = 2;
                if (fi != null)
                {
                    foreach (FileInfo f in fi)
                    {
                        zipFilesList.Add(f.Name);
                    }
                    //MainWindow.usedFilesList has every single possible ZipFile of the modInfo database
                    //for each zipfile in it, remove it in zipFilesList if it exists
                    foreach (string s in MainWindow.usedFilesList)
                    {
                        if (zipFilesList.Contains(s))
                            zipFilesList.Remove(s);
                    }
                    List<string> filesToDelete = zipFilesList;
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
                            args.ParrentProcessed = 3;
                            args.ChildTotalToProcess = filesToDelete.Count;
                            foreach (string s in filesToDelete)
                            {
                                bool retry = true;
                                bool breakOut = false;
                                while (retry)
                                {
                                    //for each zip file, verify it exists, set properties to normal, delete it
                                    try
                                    {
                                        string file = Path.Combine(Settings.RelhaxDownloadsFolder, s);
                                        args.currentFile = s;
                                        File.SetAttributes(file, FileAttributes.Normal);
                                        File.Delete(file);
                                        // remove file from database, too
                                        XMLUtils.DeleteMd5HashDatabase(file);
                                        retry = false;
                                        args.ChildProcessed++;
                                    }
                                    catch (Exception e)
                                    {
                                        retry = true;
                                        Utils.ExceptionLog("checkForOldZipFiles", "delete", e);
                                        DialogResult res = MessageBox.Show(string.Format("{0} {1}", Translations.GetTranslatedString("fileDeleteFailed"), s), "", MessageBoxButtons.RetryCancel);
                                        if (res == DialogResult.Cancel)
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
            catch (Exception ex)
            {
                Utils.ExceptionLog("checkForOldZipFiles", "ex", ex);
            }
        }

        // parses a xmlUnpackFile to the process queue
        private void CreateXmlUnpackList(string xmlFile)
        {
            try
            {
                if (!File.Exists(xmlFile))
                    return;
                var filesToUnpack = XDocument.Load(xmlFile).Root.Elements().Select(y => y.Elements().ToDictionary(x => x.Name, x => x.Value)).ToArray();
                foreach (var r in filesToUnpack)
                {
                    if (r.ContainsKey("pkg") && r.ContainsKey("directoryInArchive") && r.ContainsKey("fileName") && r.ContainsKey("extractDirectory") && r.ContainsKey("newFileName")) 
                    {
                        XmlUnpack xup = new XmlUnpack
                        {
                            Pkg = @r["pkg"],
                            DirectoryInArchive = @r["directoryInArchive"],
                            FileName = @r["fileName"],
                            ExtractDirectory = @r["extractDirectory"],
                            NewFileName = @r["newFileName"],
                            ActualPatchName = Path.GetFileName(xmlFile)
                        };
                        if (r["directoryInArchive"].Equals("") || r["extractDirectory"].Equals("") || r["fileName"].Equals(""))
                        {
                            Logging.Manager(string.Format("ERROR. XmlUnPackFile '{0}' has an empty but needed node ('fileName', 'directoryInArchive' and 'extractDirectory' MUST be set\n----- dump of object ------\n{1}\n----- end of dump ------", xup.ActualPatchName.ToString(), xup.ToString()));
                        }
                        else
                        {
                            xup.Pkg = Utils.ReplaceMacro(xup.Pkg);
                            xup.DirectoryInArchive = Utils.ReplaceMacro(xup.DirectoryInArchive);
                            xup.FileName = Utils.ReplaceMacro(xup.FileName);
                            xup.ExtractDirectory = Utils.ReplaceMacro(xup.ExtractDirectory);
                            xup.NewFileName = Utils.ReplaceMacro(xup.NewFileName);
                            XmlUnpackList.Add(xup);
                        }
                    }
                    else
                    {
                        Utils.DumbObjectToLog(string.Format("ERROR. XmlUnPackFile '{0}' missing node. Needed: pkg, directoryInArchive, fileName, extractDirectory, newFileName", Path.GetFileName(xmlFile)), "", r);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("CreateXmlUnpackList", "File: " + xmlFile, ex);
            }
        }

        // parses a xmlAtlasesFile to the process queue
        private void CreateAtlasesList(string xmlFile)
        {
            try
            {
                if (!File.Exists(xmlFile))
                    return;
                XDocument doc = XDocument.Load(xmlFile, LoadOptions.SetLineInfo);
                foreach (XElement atlas in doc.XPathSelectElements("/atlases/atlas"))
                {
                    try
                    {
                        Atlas atlases = new Atlas()
                        {
                            ActualPatchName = Path.GetFileName(xmlFile)
                        };
                        foreach (XElement item in atlas.Elements())
                        {
                            try
                            {
                                switch (item.Name.ToString())
                                {
                                    case "pkg":
                                        atlases.Pkg = Utils.ReplaceMacro(item.Value.ToString().Trim());
                                        break;
                                    case "directoryInArchive":
                                        atlases.DirectoryInArchive = Utils.ReplaceMacro(Utils.RemoveLeadingSlash(item.Value.ToString().Trim()));
                                        break;
                                    case "atlasFile":
                                        atlases.AtlasFile = Utils.ReplaceMacro(item.Value.ToString().Trim());
                                        break;
                                    case "mapFile":
                                        atlases.MapFile = Utils.ReplaceMacro(item.Value.ToString().Trim());
                                        break;
                                    case "atlasSaveDirectory":
                                        atlases.AtlasSaveDirectory = Utils.ReplaceMacro(item.Value.ToString().Trim());
                                        break;
                                    case "atlasWidth":
                                        atlases.AtlasWidth = int.Parse("0" + item.Value.ToString().Trim());
                                        break;
                                    case "atlasHeight":
                                        atlases.AtlasHeight = int.Parse("0" + item.Value.ToString().Trim());
                                        break;
                                    case "padding":
                                        atlases.Padding = int.Parse("0" + item.Value.ToString().Trim());
                                        break;
                                    case "powOf2":
                                        atlases.PowOf2 = Utils.ParseBool(item.Value, atlases.PowOf2 == Atlas.State.True) ? Atlas.State.True : Atlas.State.False;
                                        break;
                                    case "square":
                                        atlases.Square = Utils.ParseBool(item.Value, atlases.Square == Atlas.State.True) ? Atlas.State.True : Atlas.State.False;
                                        break;
                                    case "fastImagePacker":
                                        atlases.FastImagePacker = Utils.ParseBool(item.Value, atlases.FastImagePacker);
                                        break;
                                    case "generateMap":
                                        atlases.GenerateMap = Utils.ParseBool(item.Value, atlases.GenerateMap == Atlas.State.True) ? Atlas.State.True : Atlas.State.False;
                                        break;
                                    case "mapType":
                                        foreach (Atlas.MapType mt in Enum.GetValues(typeof(Atlas.MapType)))
                                        {
                                            if (item.Value.ToLower().Trim() == Atlas.MapTypeName(mt).ToLower() && mt != Atlas.MapType.None)
                                            {
                                                atlases.mapType = mt;
                                                break;
                                            }
                                        }
                                        break;
                                    case "imageFolders":
                                        foreach (XElement image in item.Elements())
                                        {
                                            switch (image.Name.ToString())
                                            {
                                                case "imageFolder":
                                                    if (!image.Value.ToString().Trim().Equals(""))
                                                        atlases.ImageFolderList.Add(Utils.ReplaceMacro(image.Value.ToString().Trim()));
                                                    break;
                                                default:
                                                    Logging.Manager(string.Format("unexpected Node found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value));
                                                    break;
                                            }
                                        }
                                        break;
                                    default:
                                        Logging.Manager(string.Format("unexpected Item found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value));
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Utils.ExceptionLog("CreateAtlasesList", "switch", ex);
                            }
                        }
                        if (atlases.ImageFolderList.Count == 0)
                        {
                            Logging.Manager(string.Format("ERROR. Missing imageFolders in File: {0} => file will be skipped", atlases.ActualPatchName));
                            break;
                        }
                        if (atlases.DirectoryInArchive.Equals("") || atlases.AtlasFile.Equals("") || atlases.AtlasSaveDirectory.Equals(""))
                        {
                            Logging.Manager(string.Format("ERROR. {0}-Atlases file {1} is not valid and has empty (but important) nodes", Atlas.MapTypeName(atlases.mapType).ToLower(), atlases.ActualPatchName));
                            break;
                        }
                        bool duplicateFound = false;
                        foreach (Atlas check in AtlasesList)
                        {
                            if (check.Pkg.ToLower().Equals(atlases.Pkg.ToLower()) && check.DirectoryInArchive.Replace(@"\", "").Replace(@"/","").ToLower().Equals(atlases.DirectoryInArchive.Replace(@"\", "").Replace(@"/", "").ToLower()) && check.AtlasFile.ToLower().Equals(atlases.AtlasFile.ToLower()) && check.AtlasSaveDirectory.ToLower().Equals(atlases.AtlasSaveDirectory.ToLower()))
                            {
                                // if the parameters abouve are matching, then a user added maybe additional files to add in a different folder
                                check.ImageFolderList.AddRange(atlases.ImageFolderList);
                                duplicateFound = true;
                            }
                        }
                        if (!duplicateFound)
                            AtlasesList.Add(atlases);
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("CreateAtlasesList", "foreach item / File: " + xmlFile, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("CreateAtlasesList", "File: " + xmlFile, ex);
            }
        }

        //parses a patch xml file into an xml patch instance in memory to be enqueued
        private void CreatePatchList(string xmlFile)
        {
            try
            {
                if (!File.Exists(xmlFile))
                    return;
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFile);
                //loaded the xml file into memory, create an xml list of patchs
                XmlNodeList patchesList = doc.SelectNodes("//patchs/patch");
                // modify the xml filename for logging purpose
                string nativeProcessingFile = Path.GetFileNameWithoutExtension(xmlFile);
                string actualPatchName = originalSortedPatchNames[nativeProcessingFile];
                //foreach "patch" node in the "patchs" node of the xml file
                foreach (XmlNode n in patchesList)
                {
                    //create a patch instance to take the patch information
                    Patch p = new Patch()
                    {
                        //p.actualPatchName = originalPatchNames[0];
                        actualPatchName = actualPatchName,
                        nativeProcessingFile = nativeProcessingFile,
                        type = "",
                        mode = "",
                        path = "",
                        file = "",
                        search = "",
                        replace = ""
                    };
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
                            case "patchPath":
                                p.patchPath = nn.InnerText;
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
                    // filename only record once needed
                    PatchList.Add(p);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("createPatchList", "ex", ex);
            }
        }
        //create/fills the list of shortcuts to install
        private void CreateShortcutsList(string xmlFile)
        {
            try
            {
                if (!File.Exists(xmlFile))
                    return;
                Logging.Manager(string.Format("Processing shortcut xml file: {0}", Path.GetFileName(xmlFile)));
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFile);
                //loaded the xml file into memory, create an xml list of patchs
                XmlNodeList shortcutsList = doc.SelectNodes("//shortcuts/shortcut");
                //foreach "patch" node in the "patchs" node of the xml file
                foreach (XmlNode n in shortcutsList)
                {
                    //create a patch instance to take the patch information
                    Shortcut s = new Shortcut()
                    {
                        Path = "",
                        Name = "",
                        Enabled = false//just to be safe
                    };
                    //foreach node in this specific "patch" node
                    foreach (XmlNode nn in n.ChildNodes)
                    {
                        //each element in the xml gets put into the
                        //the correcpondng attribute for the Patch instance
                        switch (nn.Name)
                        {
                            case "path":
                                s.Path = nn.InnerText;
                                break;
                            case "name":
                                s.Name = nn.InnerText;
                                break;
                            case "enabled":
                                s.Enabled = Utils.ParseBool(nn.InnerText,false);
                                break;
                        }
                    }
                    // filename only record once needed
                    Shortcuts.Add(s);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("createPatchList", "ex", ex);
            }
        }

        private static List<Texture> ParseFilesForAtlasList(List<Texture> originalTextures, string[] foldersWithModTextures, string atlasName)
        {
            List<Texture> textureList = new List<Texture>(originalTextures);
            List<Texture> modTextures = new List<Texture>();
            foreach (string r in foldersWithModTextures)
            {
                if (Directory.Exists(r))
                {
                    try
                    {
                        File.SetAttributes(r, FileAttributes.Normal);
                        //get every image file in the folder (i bet, this could done be faster!)
                        FileInfo[] fi = new string[] { "*.jpg", "*.png", "*.bmp", "*.gif" }
                            .SelectMany(i => new DirectoryInfo(r).GetFiles(i, SearchOption.TopDirectoryOnly))
                            .ToArray();

                        foreach (FileInfo f in fi)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(f.Name);
                            Bitmap newImage = Bitmap.FromFile(f.FullName) as Bitmap;
                            //don't care about the x an y for the mod textures
                            modTextures.Add(new Texture()
                            {
                                name = fileName,
                                height = newImage.Height,
                                width = newImage.Width,
                                x = 0,
                                y = 0,
                                AtlasImage = newImage
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("addFilesToAtlasList", "GetFiles of folder: " + r, ex);
                    }
                }
                else
                    Logging.Manager(string.Format("Directory {0} is not existing", r));
            }
            if (modTextures.Count > 0)
                Logging.Manager("mod textures collected for " + atlasName + ": " + modTextures.Count);
            else
            {
                Logging.Manager("NO mod textures collected for " + atlasName + "!\n... skipped to create Atlas image");
                return modTextures;
            }

            
            //for every mod texture
            for(int i = 0; i < modTextures.Count; i++)
            {
                bool found = false;
                //check the entire list of original textures for the same name
                //if same, replace the bitmap image, sizes and location
                for(int j = 0; j < textureList.Count; j++)
                {
                    if(modTextures[i].name.Equals(textureList[j].name))
                    {
                        textureList[j].AtlasImage = new Bitmap(modTextures[i].AtlasImage);
                        textureList[j].x = 0;
                        textureList[j].y = 0;
                        textureList[j].height = textureList[j].AtlasImage.Height;
                        textureList[j].width = textureList[j].AtlasImage.Width;
                        found = true;
                        //break out of the inner loop to quicker continue into the outer loop
                        break;
                    }
                }
                // if the image was not found, add it too the main list
                if (!found)
                    textureList.Add(modTextures[i]);
            }

            // files to be added, after deleting needless base files (last file added is winning): 
            Logging.Manager("total files to be added for " + atlasName + ": " + textureList.Count);
            return textureList;
        }

        private static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        private Size ExtractAtlases_run(Atlas args)
        {
            Size originalAtlasSize = new Size
            {
                Height = 0,
                Width = 0
            };

            Logging.Manager("extracting Atlas: " + args.AtlasFile);
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            string ImageFile = Path.Combine(args.TempAltasPresentDirectory, args.AtlasFile);

            if (!File.Exists(ImageFile))
            {
                Logging.Manager("ERROR. Atlas file not found: " + ImageFile);
                return originalAtlasSize;
            }

            if (args.imageHandler == null)
            {
                Logging.Manager(string.Format("Failed to find image importers for specified image type: {0}", ImageFile));
                return originalAtlasSize;
            }

            string MapFile = Path.Combine(args.TempAltasPresentDirectory, args.MapFile);

            if (!File.Exists(MapFile))
            {
                Logging.Manager("ERROR. Map file not found: " + MapFile);
                return originalAtlasSize;
            }

            Bitmap atlasImage = null;

            try
            {
                // Load bitmap with the propper importer
                atlasImage = (Bitmap)args.imageHandler.Load(ImageFile);
                originalAtlasSize = atlasImage.Size;
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ExtractAtlases_run", "imageHandler: " + ImageFile, ex);
                return originalAtlasSize;
            }

            //just in case
            args.TextureList.Clear();

            args.TextureList = args.mapExporter.Load(MapFile);
            if (args.TextureList.Count == 0)
            {
                Logging.Manager("Failt to read map file: " + MapFile);
                return originalAtlasSize;
            }

            Logging.Manager("Parsed Textures for " + args.AtlasFile + ": " + args.TextureList.Count);

            Bitmap CroppedImage = null;
            try
            {
                PixelFormat pixelFormat = atlasImage.PixelFormat;
                int c = 0;
                foreach (Texture t in args.TextureList)
                {
                    try
                    {
                        CroppedImage = new Bitmap(t.width, t.height, pixelFormat);
                        // copy pixels over to avoid antialiasing or any other side effects of drawing
                        // the subimages to the output image using Graphics
                        for (int x = 0; x < t.width; x++)
                            for (int y = 0; y < t.height; y++)
                                CroppedImage.SetPixel(x, y, atlasImage.GetPixel(t.x + x, t.y + y));
                        //why save to disk when you can save to memory?
                        t.AtlasImage = new Bitmap(CroppedImage);
                        c++;
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("ExtractAtlases_run", "CroppedImage: " + t.name + "\nBitmap no: " + c.ToString(), ex);
                    }
                }
                Logging.Manager(string.Format("Extracted Textures for {0}: {1} {2}", args.AtlasFile, c, c == args.TextureList.Count ? "(all successfully done)" : "(missed some, why?)"));
            }
            finally
            {
                ImageFile = null;
                MapFile = null;
                atlasImage.Dispose();
                CroppedImage.Dispose();
                sw.Stop();
            }
            Logging.Manager("Extraction for " + args.AtlasFile + " completed in " + sw.Elapsed.TotalSeconds.ToString("N3", System.Globalization.CultureInfo.InvariantCulture) + " seconds.");
            return originalAtlasSize;
        }

        #region File IO Methods
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
                bool tryAgain = true;
                while (tryAgain)
                {
                    try
                    {
                        File.SetAttributes(file.FullName, FileAttributes.Normal);
                        file.Delete();
                        tryAgain = false;
                    }
                    catch (Exception e)
                    {
                        Utils.ExceptionLog("DirectoryDelete", file.FullName, e);
                        DialogResult res = MessageBox.Show(Translations.GetTranslatedString("extractionErrorMessage"), Translations.GetTranslatedString("extractionErrorHeader"), MessageBoxButtons.RetryCancel);
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
                InstallWorker.ReportProgress(args.ChildProcessed++);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (deleteSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    //string temppath = Path.Combine(sourceDirName, subdir.Name);
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
                        catch (Exception ex)
                        {
                            Utils.ExceptionLog("DirectoryDelete","deleteSubDirs", ex);
                            DialogResult result = MessageBox.Show(Translations.GetTranslatedString("deleteErrorMessage"), Translations.GetTranslatedString("deleteErrorHeader"), MessageBoxButtons.RetryCancel);
                            if (result == DialogResult.Cancel)
                                Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(args.ChildProcessed++);
                }
            }
        }
        
        //recursivly copies every file from one place to another
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool reportProgress = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                if(reportProgress)
                    InstallWorker.ReportProgress(args.ChildProcessed++);
            }
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
                if(reportProgress)
                    InstallWorker.ReportProgress(args.ChildProcessed++);
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

        //main method for moving every file from one place to another. solves the issue of Directory.move() does not support moving across volumes
        private void DirectoryMove(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite, bool reportProgress = true)
        {
            //call the recursive function to move
            _DirectoryMove(sourceDirName, destDirName, copySubDirs, overwrite, reportProgress);
            //call the process folders function to delete any leftover folders
            Utils.ProcessDirectory(sourceDirName, false);
            if (Directory.Exists(sourceDirName))
                Directory.Delete(sourceDirName);
        }

        //recursivly moves every file from one place to another
        private void _DirectoryMove(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite, bool reportProgress = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                if (reportProgress)
                    InstallWorker.ReportProgress(args.ChildProcessed++);
            }
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (File.Exists(temppath) && overwrite)
                    File.Delete(temppath);
                file.MoveTo(temppath);
                if (reportProgress)
                    InstallWorker.ReportProgress(args.ChildProcessed++);
            }
            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    _DirectoryMove(subdir.FullName, temppath, copySubDirs,overwrite,reportProgress);
                }
            }
        }

        //uses the build in microsoft Directory.Delete, but attemps to itterate multiple times in case the user is in the directory
        private void DirectoryDeleteNoProgress(string sourceDir, bool recursive)
        {
            int num_retries = 3;
            while(num_retries > 0)
            {
                try
                {
                    Directory.Delete(sourceDir, recursive);
                    num_retries = 0;
                }
                catch(Exception e)
                {
                    Logging.Manager(string.Format("DirectoryDeleteNoProgerss EXCEPTION CAUGHT, trying again in 100ms, num_retries={0} ({1})", num_retries--, e.Message), true);
                    if (num_retries == 0)
                        Utils.ExceptionLog(e);
                    else
                        System.Threading.Thread.Sleep(100);
                }
            }
        }
        #endregion

        //main unzip worker method
        private void Unzip(string zipFile, StringBuilder sb, int categoryGroup, ref int superPatchNum, bool fileLogging)
        {
            //write a formated comment line if in regular extraction mode
            if(sb==null)
                Logging.InstallerGroup(Path.GetFileNameWithoutExtension(zipFile));
            //create a retry counter to verify that any exception caught was not a one-off error
            for(int j = 3; j > 0; j--)
            {
                try
                {
                    using (ZipFile zip = new ZipFile(zipFile))
                    {
                        //for this zip file instance, for each entry in the zip file,
                        //change the "versiondir" path to this version of tanks
                        //also reset the args
                        args.ChildTotalToProcess = zip.Entries.Count;
                        args.ChildProcessed = 0;
                        for (int i = 0; i < zip.Entries.Count; i++)
                        {
                            //grab the entry name for modifications
                            string zipEntryName = zip[i].FileName;
                            zipEntryName = zipEntryName.Contains("versiondir") ? zipEntryName.Replace("versiondir", TanksVersion) : zipEntryName;
                            zipEntryName = zipEntryName.Contains("configs/xvm/xvmConfigFolderName") ? zipEntryName.Replace("configs/xvm/xvmConfigFolderName", "configs/xvm/" + xvmConfigDir) : zipEntryName;
                            if (Regex.IsMatch(zipEntryName, @"_patch.*\.xml"))
                            {
                                string patchName = zipEntryName;
                                string newPatchname = "";
                                if(Settings.SuperExtraction)
                                {
                                    //super extraction mode
                                    //install part that is parallel
                                    //use category number
                                    //note that if from global dependency, dependency, etc. it will be different
                                    //use a +2 offset because global dependency and dependency before it
                                    newPatchname = (categoryGroup + 3).ToString("D2") + "_" + superPatchNum++.ToString("D3");
                                    zipEntryName = Regex.Replace(zipEntryName, @"_patch.*\.xml", "_patch/" + newPatchname + ".xml");
                                }
                                else
                                {
                                    //regular
                                    //Int.ToString("D3") means to string of 3 decimal places, leading
                                    newPatchname = patchNum++.ToString("D3");
                                    zipEntryName = Regex.Replace(zipEntryName, @"_patch.*\.xml", "_patch/" + newPatchname + ".xml");
                                }
                                patchName = patchName.Substring(7);
                                //hash. key index is the new name
                                //originalPatchNames.Add(patchName);
                                lock(zip)
                                {
                                    originalSortedPatchNames.Add(newPatchname, patchName);
                                }
                            }
                            //save entry name modifications
                            zip[i].FileName = zipEntryName;
                            //put the entries on disk
                            /*
                            if (sb == null)// write the the file entry / with the first call at the installation process, the logfile will be created including headline, ....
                                Logging.Installer(zip[i].FileName);
                            else
                                sb.Append(zip[i].FileName +"\n");
                                */
                        }
                        zip.ExtractProgress += Zip_ExtractProgress;
                        //zip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
                        //NEED TO TEST
                        string completePath = "";
                        for(int i = 0; i < zip.Entries.Count; i++)
                        {
                            bool processed = false;
                            foreach(string s in zipMacros.Keys)
                            {
                                if (zip[i].FileName.Length > s.Length && zip[i].FileName.Substring(0, s.Length).Equals(s))
                                {
                                    processed = true;
                                    //change the fileName and extract
                                    zip[i].FileName = zip[i].FileName.Replace(s, "");
                                    if (!string.IsNullOrWhiteSpace(zip[i].FileName))
                                        zip[i].Extract((string)zipMacros[s], ExtractExistingFileAction.OverwriteSilently);
                                    //write to disk
                                    completePath = Path.Combine((string)zipMacros[s], zip[i].FileName.Replace(@"/",@"\"));
                                    //entry was processed, safe to move on
                                    break;
                                }
                            }
                            if (!processed)
                            {
                                //default to extract to TanksLocation
                                zip[i].Extract(TanksLocation, ExtractExistingFileAction.OverwriteSilently);
                                completePath = Path.Combine(TanksLocation, zip[i].FileName.Replace(@"/", @"\"));
                            }
                            if (sb == null)
                                Logging.Installer((fileLogging ? "": "# ") + completePath);
                            else
                                sb.Append((fileLogging ? "": "# ") + completePath + "\n");
                            args.ChildProcessed++;
                        }
                        //we made it, set j to 1 to break out of the exception catch loop
                        j = 1;
                    }
                }
                catch (Exception e)
                {
                    if(j <= 1)
                    {
                        //append the exception to the log
                        Utils.ExceptionLog("Unzip", "ZipFile: " + zipFile, e);
                        //show the error message
                        MessageBox.Show(string.Format("{0}, {1} {2} {3}", Translations.GetTranslatedString("zipReadingErrorMessage1"), Path.GetFileName(zipFile), Translations.GetTranslatedString("zipReadingErrorMessage2"), Translations.GetTranslatedString("zipReadingErrorHeader")));
                        //(try to)delete the file from the filesystem
                        if (File.Exists(zipFile))
                        {
                            try
                            {
                                File.Delete(zipFile);
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Utils.ExceptionLog("Unzip", "tried to delete " + zipFile, ex);
                            }
                        }
                        XMLUtils.DeleteMd5HashDatabase(zipFile);
                    }
                    else
                    {
                        Logging.Manager("WARNING: " + e.GetType().Name + " caught, retrying number " + j + ". File=" + Path.GetFileName(zipFile)+ "\nmessage=" + e.Message);
                    }
                }
            }
            
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if(InstallWorker != null)
                        InstallWorker.Dispose();
                    InstallWorker = null;
                    GlobalDependencies = null;
                    Dependencies = null;
                    LogicalDependencies = null;
                    ModsConfigsToInstall = null;
                    AppendedDependencies = null;
                    ModsConfigsWithData = null;
                    AppendedDependencies = null;
                    Shortcuts = null;
                    XmlUnpackList = null;
                    AtlasesList = null;
                    UserMods = null;
                    PatchList = null;
                    args = null;
                    InstallGroups = null;
                    originalSortedPatchNames = null;
                    lockerInstaller = null;
                    originalSortedPatchNames = null;
                    InstallProgressChanged = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                // NOTE: There are no unmanaged rescources in this project that *need* to be freed AFAIK

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Installer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
