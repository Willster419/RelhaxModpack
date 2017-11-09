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
        public string AppPath { get; set; }
        public List<Dependency> GlobalDependencies { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public List<LogicalDependency> LogicalDependencies { get; set; }
        public List<Dependency> AppendedDependencies { get; set; }
        public List<SelectableDatabasePackage> ModsConfigsToInstall { get; set; }
        public List<SelectableDatabasePackage> ModsConfigsWithData { get; set; }
        public List<Mod> UserMods { get; set; }
        public List<Shortcut> Shortcuts { get; set; }
        private List<Patch> patchList { get; set; }
        private List<XmlUnpack> xmlUnpackList { get; set; }
        private List<Atlases> atlasesList { get; set; }
        public string TanksVersion { get; set; }
        //the folder of the current user appdata
        public string AppDataFolder { get; set; }
        public string DatabaseVersion { get; set; }
        //properties relevent to the handler and install
        public static BackgroundWorker InstallWorker;
        public static InstallerEventArgs args;
        private string xvmConfigDir = "";
        private int patchNum = 0;
        private List<string> originalPatchNames;
        private FileStream fs;
        private string InstalledFilesLogPath = "";
        private object lockerInstaller = new object();

        private static readonly Stopwatch stopWatch = new Stopwatch();

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
            InstallWorker = new BackgroundWorker();
            InstallWorker.WorkerReportsProgress = true;
            InstallWorker.ProgressChanged += WorkerReportProgress;
            InstallWorker.RunWorkerCompleted += WorkerReportComplete;
            args = new InstallerEventArgs();
            ResetArgs();
            originalPatchNames = new List<string>();            
        }

        //Start installation on the UI thread
        public void StartInstallation()
        {
            InstallWorker.DoWork += ActuallyStartInstallation;
            InstallWorker.RunWorkerAsync();
        }

        public void StartCleanUninstallation()
        {
            InstallWorker.DoWork += ActuallyStartCleanUnInstallation;
            InstallWorker.RunWorkerAsync();
        }

        public void ActuallyStartCleanUnInstallation(object sender, DoWorkEventArgs e)
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.Uninstall;
            UninstallMods();
            //put them back when done
            if (!Directory.Exists(Path.Combine(TanksLocation, "res_mods", TanksVersion))) Directory.CreateDirectory(Path.Combine(TanksLocation, "res_mods", TanksVersion));
            if (!Directory.Exists(Path.Combine(TanksLocation, "mods", TanksVersion))) Directory.CreateDirectory(Path.Combine(TanksLocation, "mods", TanksVersion));
            args.InstalProgress = InstallerEventArgs.InstallProgress.UninstallDone;
            InstallWorker.ReportProgress(0);
            Utils.AppendToLog("Uninstallation process finished");
            MessageBox.Show(Translations.getTranslatedString("uninstallFinished"), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Start the installation on the Wokrer thread
        public void ActuallyStartInstallation(object sender, DoWorkEventArgs e)
        {
            ResetArgs();
            InstalledFilesLogPath = Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log");
            //Step 1: do a backup if requested
            Utils.AppendToLog("Installation BackupMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.BackupMods;
            if (Settings.BackupModFolder)
                BackupMods();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 2: do a backup of user data
            Utils.AppendToLog("Installation BackupUserData");
            args.InstalProgress = InstallerEventArgs.InstallProgress.BackupUserData;
            if (Settings.SaveUserData)
                BackupUserData();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 3: Delete Mods
            Utils.AppendToLog("Installation UninstallMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.DeleteMods;
            if (Settings.CleanInstallation)
                UninstallMods();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Setp 3a: delete log files
            Utils.AppendToLog("Installation deleteLogs");
            if (Settings.DeleteLogs)
            {
                Utils.AppendToLog("deleteLogs selected: deleting wot, xvm, and pmod logs");
                try
                {
                    if (File.Exists(Path.Combine(TanksLocation, "python.log")))
                        File.Delete(Path.Combine(TanksLocation, "python.log"));
                    if (File.Exists(Path.Combine(TanksLocation, "xvm.log")))
                        File.Delete(Path.Combine(TanksLocation, "xvm.log"));
                    if (File.Exists(Path.Combine(TanksLocation, "pmod.log")))
                        File.Delete(Path.Combine(TanksLocation, "pmod.log"));
                    if (File.Exists(Path.Combine(TanksLocation, "WoTLauncher.log")))
                        File.Delete(Path.Combine(TanksLocation, "WoTLauncher.log"));
                    if (File.Exists(Path.Combine(TanksLocation, "cef.log")))
                        File.Delete(Path.Combine(TanksLocation, "cef.log"));
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("ActuallyStartInstallation", "deleteLogs", ex);
                }
            }
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 4: Delete user appdata cache
            Utils.AppendToLog("Installation DeleteWoTCache");
            args.InstalProgress = InstallerEventArgs.InstallProgress.DeleteWoTCache;
            if (Settings.ClearCache)
                ClearWoTCache();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 5-10: Extracts Mods
            Utils.AppendToLog("Installation ExtractDatabaseObjects");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractGlobalDependencies;
            ExtractDatabaseObjects();
            ResetArgs();
            //Step 11: Restore User Data
            Utils.AppendToLog("Installation RestoreUserData");
            args.InstalProgress = InstallerEventArgs.InstallProgress.RestoreUserData;
            if (Settings.SaveUserData)
                RestoreUserData();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 12: unpack original game xml file
            Utils.AppendToLog("Installation UnpackXmlFiles");
            args.InstalProgress = InstallerEventArgs.InstallProgress.UnpackXmlFiles;
            if (Directory.Exists(Path.Combine(TanksLocation, "_xmlUnPack")))
            {
                UnpackXmlFiles();
                Directory.Delete(Path.Combine(TanksLocation, "_xmlUnPack"), true);
            }
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 13: Patch Mods
            Utils.AppendToLog("Installation PatchMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.PatchMods;
            if (Directory.Exists(Path.Combine(TanksLocation, "_patch")))
                PatchFiles();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 14: InstallFonts
            // => only Step 17 for both now
            //Step 15: Extract User Mods
            Utils.AppendToLog("Installation ExtractUserMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractUserMods;
            if (UserMods.Count > 0)
                ExtractUserMods();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 16: Patch Mods if User Mods extracted patch files
            Utils.AppendToLog("Installation PatchUserMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.PatchUserMods;
            if (Directory.Exists(Path.Combine(TanksLocation, "_patch")))
                PatchFiles();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 17: Extract Atlases
            Utils.AppendToLog("Installation ExtractAtlases");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractAtlases;
            if (Directory.Exists(Path.Combine(TanksLocation, "_atlases")))
            {
                ExtractAtlases();
                Directory.Delete(Path.Combine(TanksLocation, "_atlases"), true);
            }
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 18: Create Atlases
            Utils.AppendToLog("Installation CreateAtlases");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CreateAtlases;
            if (atlasesList != null && atlasesList.Count > 0)
                CreateAtlases();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 19: Install Fonts
            Utils.AppendToLog("Installation UserFonts");
            args.InstalProgress = InstallerEventArgs.InstallProgress.InstallUserFonts;
            if (Directory.Exists(Path.Combine(TanksLocation, "_fonts")))
                InstallFonts();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 20: create shortCuts
            Utils.AppendToLog("Installation CreateShortscuts");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CreateShortCuts;
            if (Settings.CreateShortcuts)
                CreateShortCuts();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 21: CheckDatabase and delete outdated or no more needed files
            Utils.AppendToLog("Installation CheckDatabase");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CheckDatabase;
            if (!Program.testMode)
                checkForOldZipFiles();
            else
                Utils.AppendToLog("... skipped");
            ResetArgs();
            //Step 22: Cleanup
            Utils.AppendToLog("Intallation CleanUp");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CleanUp;
            try
            {
                if (Directory.Exists(Path.Combine(TanksLocation, "_readme")))
                    Directory.Delete(Path.Combine(TanksLocation, "_readme"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_patch")))
                    Directory.Delete(Path.Combine(TanksLocation, "_patch"), true);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ActuallyStartInstallation", "Cleanup _folders", ex);
            }
            InstallWorker.ReportProgress(0);
        }

        public void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
        {
            OnInstallProgressChanged();
        }

        public void WorkerReportComplete(object sender, AsyncCompletedEventArgs e)
        {
            Utils.AppendToLog("Installation Done");
            args.InstalProgress = InstallerEventArgs.InstallProgress.Done;
            OnInstallProgressChanged();
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

        //Step 1: Backup Mods
        public void BackupMods()
        {
            try
            {
                //backupResMods the mods folder
                if (!Directory.Exists(Path.Combine(Application.StartupPath, "RelHaxModBackup")))
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "RelHaxModBackup"));
                //create a new mods folder based on date and time
                //yyyy-MM-dd-HH-mm-ss
                DateTime now = DateTime.Now;
                string folderDateName = String.Format("{0:yyyy-MM-dd-HH-mm-ss}", now);
                if (!Directory.Exists(Path.Combine(Application.StartupPath, "RelHaxModBackup", folderDateName, "res_mods")))
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "RelHaxModBackup", folderDateName, "res_mods"));
                if (!Directory.Exists(Path.Combine(Application.StartupPath, "RelHaxModBackup", folderDateName, "mods")))
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "RelHaxModBackup", folderDateName, "mods"));
                NumFilesToProcess(Path.Combine(Application.StartupPath, "RelHaxModBackup", folderDateName, "mods"));
                NumFilesToProcess(Path.Combine(Application.StartupPath, "RelHaxModBackup", folderDateName, "res_mods"));
                InstallWorker.ReportProgress(0);
                DirectoryCopy(Path.Combine(TanksLocation, "res_mods"), Path.Combine(Application.StartupPath, "RelHaxModBackup", folderDateName, "res_mods"), true);
                DirectoryCopy(Path.Combine(TanksLocation, "mods"), Path.Combine(Application.StartupPath, "RelHaxModBackup", folderDateName, "mods"), true);
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
                foreach (SelectableDatabasePackage dbo in ModsConfigsWithData)
                {
                    try
                    {
                        foreach (string s in dbo.UserFiles)
                        {
                            try
                            {
                                string correctedPath = s.TrimStart('\x005c').Replace(@"\\", @"\");
                                string folderPath = Path.Combine(TanksLocation, Path.GetDirectoryName(correctedPath));
                                if (!Directory.Exists(folderPath)) continue;
                                string[] fileList = Directory.GetFiles(folderPath, Path.GetFileName(correctedPath));   // use the GetFileName(correctedPath) as a search pattern, to only get wanted files
                                foreach (string startLoc in fileList)
                                {
                                    string destLoc = Path.Combine(Application.StartupPath, "RelHaxTemp", Utils.GetValidFilename(dbo.Name + "_") + Path.GetFileName(startLoc));
                                    try
                                    {
                                        if (File.Exists(@startLoc))
                                        {
                                            File.Move(startLoc, destLoc);
                                            Utils.AppendToLog(string.Format("BackupUserData: {0} ({1})", Path.Combine(Path.GetDirectoryName(correctedPath), Path.GetFileName(startLoc)), Path.GetFileName(correctedPath)));
                                        }
                                    }
                                    catch
                                    {
                                        if (Program.testMode) { MessageBox.Show(string.Format("Error: can not move file.\nstartLoc: \"{0}\"\ndestLoc: \"{1}\"", startLoc, destLoc)); };
                                        Utils.AppendToLog(string.Format("Error: can not move file. startLoc: \"{0}\" destLoc: \"{1}\"", startLoc, destLoc));
                                    }
                                }
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

        private void DeleteFilesByList(List<string> list, bool reportProgress = false, TextWriter tw = null)
        {
            foreach (string line in list)
            {
                if (reportProgress)
                {
                    args.currentFile = line;
                    InstallWorker.ReportProgress(args.ChildProcessed++);
                }
                
                if (line.EndsWith("/") | line.EndsWith(@"\"))
                {
                    try
                    {
                        if(Directory.Exists(line))
                        {
                            File.SetAttributes(line, FileAttributes.Normal);
                            Directory.Delete(line);
                        }
                    }
                    catch       // catch exception if folder is not empty
                    { }
                }
                else
                {
                    try
                    {
                        //always try to solve the problem without throwing exceptions
                        //https://stackoverflow.com/questions/1395205/better-way-to-check-if-a-path-is-a-file-or-a-directory
                        if ((File.Exists(line)) || (Directory.Exists(line)))
                        {
                            File.SetAttributes(line, FileAttributes.Normal);
                            FileAttributes attr = File.GetAttributes(line);
                            if (attr.HasFlag(FileAttributes.Directory))
                            {
                                Directory.Delete(line);
                            }
                            else
                            {
                                File.Delete(line);
                            }
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // 
                    }
                    catch (FileNotFoundException)
                    {
                        // 
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //
                    }
                    catch (Exception ex)    // here is another problem, so logging it
                    {
                        Utils.ExceptionLog("DeleteFilesByList", "delete file: " + line, ex);
                    }
                }
                if (tw != null)
                    tw.WriteLine(line);
            }
        }

        public void UninstallMods()
        {
            try
            {
                List<string> lines = new List<string>();
                string installLogFile = Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log");
                if (File.Exists(installLogFile))
                {
                    lines = File.ReadAllLines(installLogFile).ToList();
                }
                lines.Reverse();
                while (args.ChildProcessed < lines.Count())
                {
                    try
                    {
                        if (!lines[args.ChildProcessed].Substring(0, 2).Equals("/*"))
                        {
                            if (lines[args.ChildProcessed].Length > 17)
                            {
                                if (lines[args.ChildProcessed].Substring(0, 17).Equals("Database Version:"))
                                {
                                    lines.RemoveAt(args.ChildProcessed);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            lines.RemoveAt(args.ChildProcessed);
                            continue;
                        }
                        args.currentFile = lines[args.ChildProcessed].Replace("/",@"\");
                        args.ChildProcessed++;
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("UninstallMods", string.Format("e\nChildProcessed: {0}\nChildTotalToProcess: {1}\nlines.Count(): {2}", args.ChildProcessed, args.ChildTotalToProcess, lines.Count()), ex);
                    }
                }
                InstallWorker.ReportProgress(0);
                args.ChildProcessed = 0;
                args.ChildTotalToProcess = lines.Count();
                Utils.AppendToLog(string.Format("Elements to delete (from logfile): {0}", lines.Count()));
                try
                {
                    string logFile = Path.Combine(TanksLocation, "logs", "uninstallRelhaxFiles.log");
                    // backup the last uninstall logfile
                    if (File.Exists(logFile))
                    {
                        if (File.Exists(logFile + ".bak"))
                            File.Delete(logFile + ".bak");
                        File.Move(logFile, logFile + ".bak");
                    }
                    TextWriter tw = new StreamWriter(logFile);
                    if (tw != null)
                    {
                        tw.WriteLine(string.Format(@"/*  Date: {0:yyyy-MM-dd HH:mm:ss}  */", DateTime.Now));
                        tw.WriteLine(@"/*  files and folders deleted from logfile  */");
                    }
                    try
                    {
                        DeleteFilesByList(lines, true, tw);
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("UninstallMods", "delete from logfile", ex);
                    }
                    lines = NumFilesToProcess(Path.Combine(TanksLocation, "res_mods"));
                    lines.AddRange(NumFilesToProcess(Path.Combine(TanksLocation, "mods")));
                    // reverse the parsed list, to delete files and folders from the lowest to the highest folder level
                    lines.Reverse();
                    InstallWorker.ReportProgress(0);
                    Utils.AppendToLog(string.Format("Elements to delete (from parsing): {0}", lines.Count()));
                    if (lines.Count()> 0)
                    {
                        args.ChildProcessed = 0;
                        args.ChildTotalToProcess = lines.Count();
                        try
                        {
                            if (tw != null)
                                tw.WriteLine("/*  files and folders deleted after parsing  */");
                            DeleteFilesByList(lines, true, tw);
                            //don't forget to delete the readme files
                            if (Directory.Exists(Path.Combine(TanksLocation, "_readme")))
                               Directory.Delete(Path.Combine(TanksLocation, "_readme"), true);
                        }
                        catch (Exception ex)
                        {
                            Utils.ExceptionLog("UninstallMods", "delete from parsing", ex);
                        }
                    }
                    if (tw != null)
                        tw.Close();
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("UninstallMods", "sw", ex);
                }
                try       // if the delete will raise an exception, it will be ignored
                {
                    if (File.Exists(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak")))
                        File.Delete(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak"));
                    if (File.Exists(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log")))
                        File.Move(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log"), Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak"));
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("UninstallMods", "Delete installedRelhaxFiles.log.bak", ex);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("UninstallMods", "ex", ex);
            }
        }
        
        //Step 3: Delete all mods
        public void DeleteMods()
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
                    Utils.AppendToLog("ERROR: AppDataFolder not correct, value: " + AppDataFolder);
                    Utils.AppendToLog("Aborting ClearWoTCache()");
                    return;
                }
                Utils.AppendToLog("Started clearing of WoT cache files");

                string[] fileFolderNames = { "preferences.xml", "preferences_ct.xml", "modsettings.dat", "xvm", "pmod" };
                string AppPathTempFolder = Path.Combine(AppPath, "RelHaxTemp", "AppDataBackup");

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
                    Utils.ExceptionLog("ClearWoTCache, step 3", ex);
                }
                Utils.AppendToLog("Finished clearing of WoT cache files");
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ClearWoTCache", "ex", ex);
            }
        }
        
        //Step 5-10: Extract All DatabaseObjects
        public void ExtractDatabaseObjects()
        {
            try
            {
                //just a double-check to delete all patches
                if (Directory.Exists(Path.Combine(TanksLocation, "_patch"))) Directory.Delete(Path.Combine(TanksLocation, "_patch"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_fonts"))) Directory.Delete(Path.Combine(TanksLocation, "_fonts"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_xmlUnPack"))) Directory.Delete(Path.Combine(TanksLocation, "_xmlUnPack"), true);
                if (!Directory.Exists(Path.Combine(TanksLocation, "res_mods"))) Directory.CreateDirectory(Path.Combine(TanksLocation, "res_mods"));
                if (!Directory.Exists(Path.Combine(TanksLocation, "mods"))) Directory.CreateDirectory(Path.Combine(TanksLocation, "mods"));
                if (!Directory.Exists(Path.Combine(TanksLocation, "logs"))) Directory.CreateDirectory(Path.Combine(TanksLocation, "logs"));

                //start the entry for the database version in installedRelhaxFiles.log
                try
                {
                    //if a current log and backup log exist
                    if (File.Exists(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak")) && File.Exists(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log")))
                    {
                        //current becomes backup, backup is deleted
                        File.Delete(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak"));
                        File.Move(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log"), Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak"));
                    }
                    //only log exists
                    else if (File.Exists(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log")))
                    {
                        //move it to be a backup
                        File.Move(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log"), Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log.bak"));
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("ExtractDatabaseObjects", "move installedRelhaxFiles.log", ex);
                }
                //start the entry for the files log
                fs = new FileStream(InstalledFilesLogPath, FileMode.Append, FileAccess.Write);
                string databaseHeader = string.Format("Database Version: {0}", Settings.DatabaseVersion);
                string dateTimeHeader = string.Format("/*  Date: {0:yyyy-MM-dd HH:mm:ss}  */", DateTime.Now);
                fs.Write(Encoding.UTF8.GetBytes(databaseHeader), 0, Encoding.UTF8.GetByteCount(databaseHeader));
                fs.Write(Encoding.UTF8.GetBytes(dateTimeHeader), 0, Encoding.UTF8.GetByteCount(dateTimeHeader));

                //extract RelHax Mods
                Utils.AppendToLog("Starting Relhax Modpack Extraction");
                string downloadedFilesDir = Path.Combine(Application.StartupPath, "RelHaxDownloads");
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

                foreach (SelectableDatabasePackage dbo in ModsConfigsToInstall)
                    if (!dbo.ZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (Dependency d in AppendedDependencies)
                    if (!d.ZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                InstallWorker.ReportProgress(0);
                //extract global dependencies
                foreach (Dependency d in GlobalDependencies)
                {
                    if (!d.ZipFile.Equals(""))
                    {
                        Utils.AppendToLog("Extracting Global Dependency " + d.ZipFile);
                        try
                        {
                            if(Settings.InstantExtraction)
                            {
                                lock (lockerInstaller)
                                {
                                    while (!d.ReadyForInstall)
                                        System.Threading.Thread.Sleep(50);
                                }
                            }
                            Unzip(Path.Combine(downloadedFilesDir, d.ZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip GlobalDependencies", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + d.ZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //extract dependencies
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractDependencies;
                InstallWorker.ReportProgress(0);
                foreach (Dependency d in Dependencies)
                {
                    if (!d.ZipFile.Equals(""))
                    {
                        Utils.AppendToLog("Extracting Dependency " + d.ZipFile);
                        try
                        {
                            if (Settings.InstantExtraction)
                            {
                                lock (lockerInstaller)
                                {
                                    while (!d.ReadyForInstall)
                                        System.Threading.Thread.Sleep(50);
                                }
                            }
                            Unzip(Path.Combine(downloadedFilesDir, d.ZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip Dependencies", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + d.ZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //extract logical dependencies
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractLogicalDependencies;
                InstallWorker.ReportProgress(0);
                foreach (LogicalDependency d in LogicalDependencies)
                {
                    if (!d.ZipFile.Equals(""))
                    {
                        Utils.AppendToLog("Extracting Logical Dependency " + d.ZipFile);
                        try
                        {
                            if (Settings.InstantExtraction)
                            {
                                lock (lockerInstaller)
                                {
                                    while (!d.ReadyForInstall)
                                        System.Threading.Thread.Sleep(50);
                                }
                            }
                            Unzip(Path.Combine(downloadedFilesDir, d.ZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip LogicalDependencies", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + d.ZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //set xvmConfigDir here because xvm is always a dependency, but don't log it
                xvmConfigDir = PatchUtils.GetXVMBootLoc(TanksLocation, null, false);
                //extract mods and configs
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractMods;
                InstallWorker.ReportProgress(0);
                foreach (SelectableDatabasePackage dbo in ModsConfigsToInstall)
                {
                    if (!dbo.ZipFile.Equals(""))
                    {
                        Utils.AppendToLog("Extracting Mod/Config " + dbo.ZipFile);
                        try
                        {
                            if (Settings.InstantExtraction)
                            {
                                lock (lockerInstaller)
                                {
                                    while (!dbo.ReadyForInstall)
                                        System.Threading.Thread.Sleep(50);
                                }
                            }
                            Unzip(Path.Combine(downloadedFilesDir, dbo.ZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip dbo.ZipFile", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + dbo.ZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //extract dependencies
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractAppendedDependencies;
                InstallWorker.ReportProgress(0);
                foreach (Dependency d in AppendedDependencies)
                {
                    if (!d.ZipFile.Equals(""))
                    {
                        Utils.AppendToLog("Extracting Appended Dependency " + d.ZipFile);
                        try
                        {
                            if (Settings.InstantExtraction)
                            {
                                lock (lockerInstaller)
                                {
                                    while (!d.ReadyForInstall)
                                        System.Threading.Thread.Sleep(50);
                                }
                            }
                            Unzip(Path.Combine(downloadedFilesDir, d.ZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.ExceptionLog("ExtractDatabaseObjects", "unzip AppendedDependencies", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + d.ZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //finish by moving WoTAppData folder contents into application data folder
                //folder name is "WoTAppData"
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractConfigs;
                InstallWorker.ReportProgress(0);
                string folderToMove = Path.Combine(TanksLocation, "WoTAppData");
                if (Directory.Exists(folderToMove))
                {
                    Utils.AppendToLog("WoTAppData folder detected, moving files to WoT cache folder");
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
                Utils.AppendToLog("Finished Relhax Modpack Extraction");
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ExtractDatabaseObjects", ex);
            }
        }

        //Step 11: Restore User Data
        public void RestoreUserData()
        {
            try
            {
                args.ParrentTotalToProcess = ModsConfigsWithData.Count;
                InstallWorker.ReportProgress(0);
                foreach (SelectableDatabasePackage dbo in ModsConfigsWithData)
                {
                    try
                    {
                        args.ChildTotalToProcess = dbo.UserFiles.Count;
                        foreach (string s in dbo.UserFiles)
                        {
                            try {
                                string correctedUserFiles = s.TrimStart('\x005c').Replace(@"\\", @"\");
                                string targetDir = Path.GetDirectoryName(correctedUserFiles);
                                args.currentFile = correctedUserFiles;
                                InstallWorker.ReportProgress(0);
                                string filenamePrefix = Utils.GetValidFilename(dbo.Name + "_");
                                //find the files with the specified pattern
                                string[] fileList = Directory.GetFiles(Path.Combine(Application.StartupPath, "RelHaxTemp"), filenamePrefix + Path.GetFileName(correctedUserFiles));
                                //if no results, go on with the next entry
                                if (fileList.Length == 0) continue;
                                foreach (string ss in fileList)
                                {
                                    string targetFilename = Path.GetFileName(ss).Replace(filenamePrefix, "");
                                    try
                                    {
                                        //the file has been found in the temp directory
                                        if (!Directory.Exists(Path.Combine(TanksLocation, targetDir)))
                                            Directory.CreateDirectory(Path.Combine(TanksLocation, targetDir));
                                        if (File.Exists(Path.Combine(TanksLocation, targetDir, targetFilename)))
                                            File.Delete(Path.Combine(TanksLocation, targetDir, targetFilename));
                                        File.Move(Path.Combine(Application.StartupPath, "RelHaxTemp", Path.GetFileName(ss)), Path.Combine(TanksLocation, targetDir, targetFilename));
                                        Utils.AppendToLog(string.Format("RestoredUserData: {0}", Path.Combine(targetDir, targetFilename)));
                                    }
                                    catch (Exception p)
                                    {
                                        Utils.ExceptionLog("RestoreUserData", "p\n" + ss, p);
                                    }
                                }
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
                    diArr = di.GetFiles(@"*.xml", System.IO.SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("UnpackXmlFiles", "parse _xmlUnPack folder", ex);

                }

                xmlUnpackList = new List<XmlUnpack>();
                for (int i = 0; i < diArr.Count(); i++)
                {
                    //set the attributes to normal
                    File.SetAttributes(diArr[i].FullName, FileAttributes.Normal);
                    //add jobs to xmlUnpackList
                    CreateXmlUnpackList(diArr[i].FullName);
                }
                if (xmlUnpackList.Count > 0) Utils.AppendToInstallLog(@"/*  unpacked XML files  */");
                foreach (XmlUnpack r in xmlUnpackList)
                {
                    string fn = r.newFileName.Equals("") ? r.fileName : r.newFileName;
                    try
                    {
                        if (!Directory.Exists(r.extractDirectory)) Directory.CreateDirectory(r.extractDirectory);
                        if (r.pkg.Equals(""))
                        {
                            try
                            {
                                // if value of pkg is empty, it is not contained in an archive
                                File.Copy(Path.Combine(r.directoryInArchive, r.fileName), Path.Combine(r.extractDirectory, fn), false);     // no overwrite of an exsisting file !!
                                Utils.AppendToInstallLog(Path.Combine(r.extractDirectory, fn));
                            }
                            catch (Exception ex)
                            {
                                Utils.ExceptionLog("Unzip", string.Format("move: {0}", Path.Combine(r.extractDirectory, fn)), ex);
                            }
                        }
                        else
                        {
                            //get file from the zip archive
                            using (ZipFile zip = new ZipFile(r.pkg))
                            {
                                for (int i = 0; i < zip.Entries.Count; i++)
                                {
                                    if (Regex.IsMatch(zip[i].FileName, Path.Combine(r.directoryInArchive, r.fileName).Replace(@"\", @"/")))
                                    {
                                        try
                                        {
                                            zip[i].FileName = fn;
                                            zip.ExtractSelectedEntries(zip[i].FileName, null, r.extractDirectory, ExtractExistingFileAction.Throw);  // no overwrite of an exsisting file !!
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            Utils.ExceptionLog("Unzip", string.Format("extration: {0}", Path.Combine(r.extractDirectory, zip[i].FileName)), ex);
                                        }
                                    }
                                }
                                zip.Dispose(); 
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog(string.Format("UnpackXmlFiles", "extract file from archive\ndirectoryInArchive: {0}\nfileName: {1}\nextractDirectory: {2}\nnewFileName: {3}", r.directoryInArchive, r.fileName, r.extractDirectory, r.newFileName), ex);
                    }
                    try
                    {
                        XmlBinary.XmlBinaryHandler xmlUnPack = new XmlBinary.XmlBinaryHandler();
                        xmlUnPack.unPack(Path.Combine(r.extractDirectory, fn));
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog(string.Format("UnpackXmlFiles", "xmlUnPack\nfileName: {0}", Path.Combine(r.extractDirectory, fn)), ex);
                    }
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
                System.Threading.Thread.Sleep(100);
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
                        DialogResult res = MessageBox.Show(Translations.getTranslatedString("patchingSystemDeneidAccessMessage"), Translations.getTranslatedString("patchingSystemDeneidAccessHeader"), MessageBoxButtons.RetryCancel);
                        if (res == DialogResult.Cancel)
                        {
                            Application.Exit();
                        }
                    }
                }

                //get any other old patches out of memory
                patchList = new List<Patch>();
                for (int i = 0; i < diArr.Count(); i++)
                {
                    //set the attributes to normall
                    File.SetAttributes(diArr[i].FullName, FileAttributes.Normal);
                    //add patches to patchList
                    this.createPatchList(diArr[i].FullName);
                }
                args.ParrentTotalToProcess = patchList.Count;
                args.ParrentProcessed = 0;
                //the actual patch method
                foreach (Patch p in patchList)
                {
                    args.currentFile = p.file;
                    InstallWorker.ReportProgress(0);
                    //if nativeProcessingFile is not empty, it is the first entry of a new nativ xml processing file. Add a comment at the loglist, to be able to traceback the native Processing File
                    if (p.nativeProcessingFile != "") { Utils.AppendToLog(string.Format("nativeProcessingFile: {0}, originalName: {1}", p.nativeProcessingFile, p.actualPatchName)); }
                    string patchFileOutput = p.file;
                    int maxLength = 200;
                    if (p.file.Length > maxLength)
                        patchFileOutput = p.file.Substring(0, maxLength);
                    Application.DoEvents();
                    if (p.type.Equals("regx") || p.type.Equals("regex"))
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
                            Utils.AppendToLog("Regex patch, all lines, line by line, " + p.file + ", " + p.search + ", " + p.replace);
                            PatchUtils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion);
                        }
                        else if (p.lines.Count() == 1 && tempp == -1)
                        {
                            //perform regex patch on entire file, as one whole string
                            Utils.AppendToLog("Regex patch, all lines, whole file, " + p.file + ", " + p.search + ", " + p.replace);
                            PatchUtils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion, -1);
                        }
                        else
                        {
                            foreach (string s in p.lines)
                            {
                                //perform regex patch on specific file lines
                                //will need to be a standard for loop BTW
                                Utils.AppendToLog("Regex patch, line " + s + ", " + p.file + ", " + p.search + ", " + p.replace);
                                PatchUtils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion, int.Parse(s));
                            }
                        }
                    }
                    else if (p.type.Equals("xml"))
                    {
                        //perform xml patch
                        Utils.AppendToLog("Xml patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                        PatchUtils.XMLPatch(p.file, p.path, p.mode, p.search, p.replace, TanksLocation, TanksVersion);
                    }
                    else if (p.type.Equals("json"))
                    {
                        //perform json patch
                        Utils.AppendToLog("Json patch, " + p.file + ", " + p.path + ", " + p.replace);
                        PatchUtils.JSONPatch(p.file, p.path, p.search, p.replace, p.mode, TanksLocation, TanksVersion);
                    }
                    else if (p.type.Equals("xvm"))
                    {
                        //perform xvm style json patch
                        Utils.AppendToLog("XVM patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                        PatchUtils.XVMPatch(p.file, p.path, p.search, p.replace, p.mode, TanksLocation, TanksVersion);
                    }
                    else if (p.type.Equals("pmod"))
                    {
                        //perform pmod/generic style json patch
                        Utils.AppendToLog("PMOD/Generic patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                        PatchUtils.PMODPatch(p.file, p.path, p.search, p.replace, p.mode, TanksLocation, TanksVersion);
                    }
                    args.ParrentProcessed++;
                    InstallWorker.ReportProgress(0);
                }
                //all done, delete the patch folder
                if (Directory.Exists(Path.Combine(TanksLocation, "_patch")))
                    Directory.Delete(Path.Combine(TanksLocation, "_patch"), true);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("PatchFiles","ex", ex);
            }
        }

        //Step 17: Extract Atlases
        private void ExtractAtlases()
        {
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

                atlasesList = new List<Atlases>();
                for (int i = 0; i < diArr.Count(); i++)
                {
                    //set the attributes to normal
                    File.SetAttributes(diArr[i].FullName, FileAttributes.Normal);
                    //add jobs to CreateAtlasesList
                    CreateAtlasesList(diArr[i].FullName);
                }
                Installer.args.ParrentTotalToProcess = atlasesList.Count;
                foreach (Atlases a in atlasesList)
                {
                    try
                    {
                        Installer.args.ParrentProcessed++;
                        if (!Directory.Exists(a.atlasSaveDirectory)) Directory.CreateDirectory(a.atlasSaveDirectory);
                        Utils.AppendToInstallLog(Path.Combine(a.atlasSaveDirectory));
                        if (!Directory.Exists(a.workingFolder)) Directory.CreateDirectory(a.workingFolder); 

                        if (!a.pkg.Equals(""))
                        {
                            //get file from the zip archive
                            using (ZipFile zip = new ZipFile(a.pkg))
                            {
                                for (int i = 0; i < zip.Entries.Count; i++)
                                {
                                    string[] fileList = new string[] { a.atlasFile, a.mapFile };
                                    foreach (string fl in fileList)
                                    {
                                        if (Regex.IsMatch(zip[i].FileName, Path.Combine(a.directoryInArchive, fl).Replace(@"\", @"/")))
                                        {
                                            try
                                            {
                                                zip[i].FileName = fl;
                                                zip.ExtractSelectedEntries(zip[i].FileName, null, Path.Combine(Application.StartupPath, "RelHaxTemp"), ExtractExistingFileAction.Throw);  // no overwrite of an exsisting file !!
                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                Utils.ExceptionLog("ExtractAtlases", string.Format("extration: {0}", Path.Combine(Application.StartupPath, "RelHaxTemp", zip[i].FileName)), ex);
                                            }
                                        }
                                    }
                                }
                                zip.Dispose();
                            }
                            a.directoryInArchive = Path.Combine(Application.StartupPath, "RelHaxTemp");
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog(string.Format("ExtractAtlases", "extract file from archive\ndirectoryInArchive: {0}\natlasFile: {1}\nmapFile: {2}\natlasSaveDirectory: {3}", a.directoryInArchive, a.atlasFile, a.mapFile, a.atlasSaveDirectory), ex);
                    }
                    if (atlasesList.Count > 0)
                    {
                        try
                        {
                            ExtractAtlases_run(a);
                        }
                        catch (Exception ex)
                        {
                            Utils.ExceptionLog(string.Format("ExtractAtlases", "atlasFile: {0}", Path.Combine(a.atlasSaveDirectory, a.atlasFile)), ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ExtractAtlases", "root", ex);
            }
        }

        //Step 18: Create Atlases
        private void CreateAtlases()
        {
            foreach (Atlases a in atlasesList)
            {
                AtlasesCreator.AtlasesArgs atlasesArgs = new AtlasesCreator.AtlasesArgs();
                atlasesArgs.MaxHeight = 2048;
                atlasesArgs.ImageFile = Path.Combine(a.atlasSaveDirectory, a.atlasFile);
                atlasesArgs.MapFile = Path.Combine(a.atlasSaveDirectory, a.mapFile);
                atlasesArgs.PowOf2 = true;
                atlasesArgs.Square = false;
                atlasesArgs.GenerateMap = true;
                List<string> fl = new List<string>();
                fl.Add(a.workingFolder);
                fl.AddRange(a.imageFolderList);
                atlasesArgs.Images = addFilesToAtlasList(fl.ToArray());
                AtlasesCreator.Program.Run(atlasesArgs);
            }
        }
        
        //Step 19: Install Fonts
        public void InstallFonts()
        {
            try
            {
                Utils.AppendToLog("Checking for fonts to install");
                if (!Directory.Exists(Path.Combine(TanksLocation, "_fonts")))
                {
                    Utils.AppendToLog("No fonts to install");
                    //no fonts to install, done display
                    return;
                }
                string[] fonts = Directory.GetFiles(Path.Combine(TanksLocation, "_fonts"), @"*.*",System.IO.SearchOption.TopDirectoryOnly);
                if (fonts.Count() == 0)
                {
                    //done display
                    Utils.AppendToLog("No fonts to install");
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
                        Utils.AppendToLog("No fonts to install");
                        //done display
                        return;
                    }
                    Utils.AppendToLog("Installing fonts: " + string.Join(", ", fontsList));
                    DialogResult dr = DialogResult.No;
                    if (Program.autoInstall)
                    {
                        //assume rights to install
                        dr = DialogResult.Yes;
                    }
                    else
                    {
                        dr = MessageBox.Show(Translations.getTranslatedString("fontsPromptInstallText"), Translations.getTranslatedString("fontsPromptInstallHeader"), MessageBoxButtons.YesNo);
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
                                    MessageBox.Show(string.Format("{0} FontReg.exe", Translations.getTranslatedString("failedToDownload_1")));
                                }
                            }
                        }
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.FileName = fontRegPath;
                        info.UseShellExecute = true;
                        info.Verb = "runas"; // Provides Run as Administrator
                        info.Arguments = "/copy";
                        info.WorkingDirectory = Path.Combine(TanksLocation, "_fonts");
                        Process installFontss = new Process();
                        installFontss.StartInfo = info;
                        try
                        {
                            installFontss.Start();
                            installFontss.WaitForExit();
                            Utils.AppendToLog("FontReg.exe ExitCode: " + installFontss.ExitCode);
                        }
                        catch (Exception e)
                        {
                            Utils.ExceptionLog("InstallFonts", "could not start font installer", e);
                            MessageBox.Show(Translations.getTranslatedString("fontsPromptError_1") + TanksLocation + Translations.getTranslatedString("fontsPromptError_2"));
                            Utils.AppendToLog("Installation done, but fonts install failed");
                            return;
                        }
                        Utils.AppendToLog("Fonts Installed Successfully");
                        return;
                    }
                    else
                    {
                        Utils.AppendToLog("Installation done, but fonts install failed");
                        return;
                    }
                }
                finally
                {
                    System.Threading.Thread.Sleep(200);
                    if (Directory.Exists(Path.Combine(TanksLocation, "_fonts")))
                        Directory.Delete(Path.Combine(TanksLocation, "_fonts"), true);
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
            try
            {
                //set xvm dir location again in case it's just a user mod install
                if (xvmConfigDir == null || xvmConfigDir.Equals(""))
                    xvmConfigDir = PatchUtils.GetXVMBootLoc(TanksLocation);
                //extract user mods
                Utils.AppendToLog("Starting Relhax Modpack User Mod Extraction");
                string downloadedFilesDir = Path.Combine(Application.StartupPath, "RelHaxUserMods");
                foreach (Mod m in UserMods)
                {
                    if (m.Enabled && m.Checked)
                    {
                        Utils.AppendToLog("Exracting " + Path.GetFileName(m.ZipFile));
                        this.Unzip(Path.Combine(downloadedFilesDir, Path.GetFileName(m.ZipFile)), TanksLocation);
                        InstallWorker.ReportProgress(0);
                    }
                }
                //done with the filestream
                if (fs != null)
                {
                    fs.Dispose();
                    fs = null;
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ExtractUserMods", ex);
            }
            Utils.AppendToLog("Finished Relhax Modpack User Mod Extraction");
        }

        //Step 18: Create Shortcuts
        private void CreateShortCuts()
        {
            try
            {
                Utils.AppendToInstallLog(@"/*  Desktop shortcuts  */");
                foreach (Shortcut sc in Shortcuts)
                {
                    if (sc.Enabled)
                    {
                        string fileTarget = Path.Combine(TanksLocation, sc.Path);
                        Utils.AppendToLog(string.Format("creating desktop ShortCut: {0} ({1})", sc.Path, sc.Name));
                        if (File.Exists(fileTarget))
                        {
                            Utils.CreateShortcut(fileTarget, sc.Name, true, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("CreateShortCuts", ex);
            }
        }

        //Step 19: Check the Database for outdated or no more needed files
        private void checkForOldZipFiles()
        {
            try
            {
                args.ParrentTotalToProcess = 3;
                args.ParrentProcessed = 1;
                List<string> zipFilesList = new List<string>();
                FileInfo[] fi = null;
                try
                {
                    File.SetAttributes(Path.Combine(Application.StartupPath, "RelHaxDownloads"), FileAttributes.Normal);
                    DirectoryInfo di = new DirectoryInfo(Path.Combine(Application.StartupPath, "RelHaxDownloads"));
                    //get every zip file in the folder
                    fi = di.GetFiles(@"*.zip", SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("checkForOldZipFiles", "parse RelHaxDownloads folder", ex);
                    MessageBox.Show(string.Format(Translations.getTranslatedString("parseDownloadFolderFailed"), "RelHaxDownloads"));
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
                                        string file = Path.Combine(Application.StartupPath, "RelHaxDownloads", s);
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
                        XmlUnpack xup = new XmlUnpack();
                        xup.pkg = @r["pkg"];
                        xup.directoryInArchive = @r["directoryInArchive"];
                        xup.fileName = @r["fileName"];
                        xup.extractDirectory = @r["extractDirectory"];
                        xup.newFileName = @r["newFileName"];
                        xup.actualPatchName = Path.GetFileName(xmlFile);
                        if (r["directoryInArchive"].Equals("") || r["extractDirectory"].Equals("") || r["fileName"].Equals(""))
                        {
                            Utils.AppendToLog(string.Format("ERROR. XmlUnPackFile '{0}' has an empty but needed node ('fileName', 'directoryInArchive' and 'extractDirectory' MUST be set\n----- dump of object ------\n{1}\n----- end of dump ------", xup.actualPatchName.ToString(), xup.ToString()));
                        }
                        else
                        {
                            xup.pkg = Utils.ReplaceMacro(@xup.pkg);
                            xup.directoryInArchive = Utils.ReplaceMacro(@xup.directoryInArchive);
                            xup.fileName = Utils.ReplaceMacro(@xup.fileName);
                            xup.extractDirectory = Utils.ReplaceMacro(@xup.extractDirectory);
                            xup.newFileName = Utils.ReplaceMacro(@xup.newFileName);
                            xmlUnpackList.Add(xup);
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
                        Atlases atlases = new Atlases();
                        atlases.actualPatchName = Path.GetFileName(xmlFile);
                        foreach (XElement item in atlas.Elements())
                        {
                            try
                            {
                                switch (item.Name.ToString())
                                {
                                    case "pkg":
                                        atlases.pkg = Utils.ReplaceMacro(item.Value.ToString().Trim());
                                        break;
                                    case "directoryInArchive":
                                        atlases.directoryInArchive = Utils.ReplaceMacro(Utils.RemoveLeadingSlash(item.Value.ToString().Trim()));
                                        break;
                                    case "atlasFile":
                                        atlases.atlasFile = Utils.ReplaceMacro(item.Value.ToString().Trim());
                                        break;
                                    case "mapFile":
                                        atlases.mapFile = Utils.ReplaceMacro(item.Value.ToString().Trim());
                                        break;
                                    case "atlasSaveDirectory":
                                        atlases.atlasSaveDirectory = Utils.ReplaceMacro(item.Value.ToString().Trim());
                                        break;
                                    case "imageFolders":
                                        foreach (XElement image in item.Elements())
                                        {
                                            switch (image.Name.ToString())
                                            {
                                                case "imageFolder":
                                                    if (!image.Value.ToString().Trim().Equals(""))
                                                        atlases.imageFolderList.Add(Utils.ReplaceMacro(image.Value.ToString().Trim()));
                                                    break;
                                                default:
                                                    Utils.AppendToLog(string.Format("unexpected Node found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value));
                                                    break;
                                            }
                                        }
                                        break;
                                    default:
                                        Utils.AppendToLog(string.Format("unexpected Item found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value));
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Utils.ExceptionLog("CreateAtlasesList", "switch", ex);
                            }
                        }
                        if (atlases.imageFolderList.Count == 0)
                        {
                            Utils.AppendToLog(string.Format("ERROR. Missing imageFolders in File: {0} => file will be skipped", atlases.actualPatchName));
                            break;
                        }
                        if (atlases.directoryInArchive.Equals("") || atlases.atlasFile.Equals("") || atlases.atlasSaveDirectory.Equals(""))
                        {
                            Utils.AppendToLog(string.Format("ERROR. xmlAtlases file {0} is not valid and has empty (but important) nodes", atlases.actualPatchName));
                            break;
                        }
                        atlases.workingFolder = Path.Combine(Application.StartupPath, "RelHaxTemp", Path.GetFileNameWithoutExtension(atlases.atlasFile));
                        if (atlases.mapFile.Equals("")) atlases.mapFile = Path.GetFileNameWithoutExtension(atlases.atlasFile) + ".xml";
                        atlasesList.Add(atlases);
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
        private void createPatchList(string xmlFile)
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
                string tmpXmlFilename = Path.GetFileNameWithoutExtension(xmlFile);
                //foreach "patch" node in the "patchs" node of the xml file
                foreach (XmlNode n in patchesList)
                {
                    //create a patch instance to take the patch information
                    Patch p = new Patch();
                    p.actualPatchName = originalPatchNames[0];
                    p.nativeProcessingFile = tmpXmlFilename;
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
                    // filename only record once needed
                    tmpXmlFilename = "";
                    patchList.Add(p);
                }
                originalPatchNames.RemoveAt(0);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("createPatchList", "ex", ex);
            }
        }

        private static List<string> addFilesToAtlasList(string[] folders)
        {
            // System.Collections.Hashtable fileList = new System.Collections.Hashtable();
            List<string> collectiveList = new List<string>();
            List<string> shortNameList = new List<string>();
            foreach (string r in folders)
            {
                if (Directory.Exists(r))
                {
                    try
                    {
                        File.SetAttributes(r, FileAttributes.Normal);
                        //get every png file in the folder
                        FileInfo[] fi = new DirectoryInfo(r).GetFiles(@"*.png", SearchOption.TopDirectoryOnly);
                        foreach (FileInfo f in fi)
                        {
                            collectiveList.Add(Path.Combine(r, f.Name));
                            shortNameList.Add(Path.GetFileName(Path.GetFileNameWithoutExtension(f.Name)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("addFilesToAtlasList", "GetFiles of folder: " + r, ex);
                    }
                }
                else
                    Utils.AppendToLog(string.Format("Directory {0} is not existing", r));
            }
            Utils.AppendToLog("files collected: " + shortNameList.Count);
            int i = collectiveList.Count - 1;
            while (i > 0)
            {
                while (shortNameList.IndexOf(shortNameList[i]) != i)
                {
                    int r = shortNameList.IndexOf(shortNameList[i]);
                    shortNameList.RemoveAt(r);
                    collectiveList.RemoveAt(r);
                    i--;
                }
                i--;
            }
            // files to be added, after deleting needless base files (last file added is winning): 
            Utils.AppendToLog("files to be added: " + collectiveList.Count);
            return collectiveList;
        }

        private void ExtractAtlases_run(Atlases args)
        {
            Utils.AppendToLog("extracting Atlas: " + args.atlasFile);

            stopWatch.Reset();
            stopWatch.Start();

            string ImageFile = Path.Combine(args.directoryInArchive, args.atlasFile);
            string workingFolder = Path.Combine(Application.StartupPath, "RelHaxTemp", Path.GetFileNameWithoutExtension(ImageFile));

            if (!File.Exists(ImageFile))
            {
                Utils.AppendToLog("ERROR. Atlas file not found: " + ImageFile);
                return;
            }

            string MapFile = Path.Combine(args.directoryInArchive, args.mapFile);
            Installer.args.currentFile = Path.GetFileNameWithoutExtension(args.atlasFile);

            if (!File.Exists(MapFile))
            {
                Utils.AppendToLog("ERROR. Map file not found: " + MapFile);
                return;
            }

            if (Directory.Exists(workingFolder))
                Directory.Delete(workingFolder, true);
            Directory.CreateDirectory(workingFolder);

            Bitmap atlasImage = new Bitmap(ImageFile);
            Bitmap CroppedImage = null;
            List<Texture> textureList = new List<Texture>();
            try
            {
                try
                {
                    XDocument doc = null;
                    Texture t = null;
                    doc = XDocument.Load(MapFile, LoadOptions.SetLineInfo);
                    foreach (XElement texture in doc.XPathSelectElements("/root/SubTexture"))
                    {
                        try
                        {
                            t = new Texture();
                            foreach (XElement item in texture.Elements())
                            {
                                try
                                {
                                    switch (item.Name.ToString().ToLower())
                                    {
                                        case "name":
                                            t.name = item.Value.ToString().Trim();
                                            break;
                                        case "x":
                                            t.x = int.Parse("0" + item.Value.ToString().Trim());
                                            break;
                                        case "y":
                                            t.y = int.Parse("0" + item.Value.ToString().Trim());
                                            break;
                                        case "width":
                                            t.width = int.Parse("0" + item.Value.ToString().Trim());
                                            break;
                                        case "height":
                                            t.height = int.Parse("0" + item.Value.ToString().Trim());
                                            break;
                                        default:
                                            Utils.AppendToLog(string.Format("unexpected Item found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value));
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Utils.ExceptionLog("ExtractAtlases_run", "switch", ex);
                                }
                            }
                            textureList.Add(t);
                        }
                        catch (Exception ex)
                        {
                            Utils.ExceptionLog("ExtractAtlases_run", "foreach item", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("ExtractAtlases_run", "foreach root", ex);
                }
                Utils.AppendToLog("Parsed Textures: " + textureList.Count);

                Installer.args.ChildTotalToProcess = textureList.Count;
                PixelFormat pixelFormat = atlasImage.PixelFormat;
                int c = 0;
                foreach (Texture t in textureList)
                {
                    try
                    {
                        CroppedImage = new Bitmap(t.width, t.height, pixelFormat);
                        // copy pixels over to avoid antialiasing or any other side effects of drawing
                        // the subimages to the output image using Graphics
                        for (int x = 0; x < t.width; x++)
                            for (int y = 0; y < t.height; y++)
                                CroppedImage.SetPixel(x, y, atlasImage.GetPixel(t.x + x, t.y + y));
                        CroppedImage.Save(Path.Combine(workingFolder, t.name + ".png"), ImageFormat.Png);
                        c++;
                        Installer.args.ChildProcessed = c;
                        Installer.args.currentSubFile = t.name;
                        Installer.InstallWorker.ReportProgress(0);
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("ExtractAtlases_run", "CroppedImage: " + Path.Combine(workingFolder, t.name + ".png"), ex);
                    }
                }
                Utils.AppendToLog(string.Format("Extracted Textures: {0} {1}", c, c == textureList.Count ? "(all successfully done)" : "(missed some, why?)"));
            }
            finally
            {
                ImageFile = null;
                MapFile = null;
                textureList = null;
                atlasImage.Dispose();
                CroppedImage.Dispose();
                stopWatch.Stop();
            }
            Utils.AppendToLog("Extraction completed in " + stopWatch.Elapsed.TotalSeconds + " seconds.");
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
                    catch (Exception e)
                    {
                        Utils.ExceptionLog("DirectoryDelete", file.FullName, e);
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
                InstallWorker.ReportProgress(args.ChildProcessed++);
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
                        catch (Exception ex)
                        {
                            Utils.ExceptionLog("DirectoryDelete","deleteSubDirs", ex);
                            DialogResult result = MessageBox.Show(Translations.getTranslatedString("deleteErrorMessage"), Translations.getTranslatedString("deleteErrorHeader"), MessageBoxButtons.RetryCancel);
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

        //main unzip worker method
        private void Unzip(string zipFile, string extractFolder)
        {
            string zipFileHeader = string.Format(@"/*  {0}  */", Path.GetFileNameWithoutExtension(zipFile));
            fs.Write(Encoding.UTF8.GetBytes(zipFileHeader), 0, Encoding.UTF8.GetByteCount(zipFileHeader));
            try
            {
                using (ZipFile zip = new ZipFile(zipFile))
                {
                    //hacks to get it to lag less possibly
                    //zip.BufferSize = 65536*16; //1MB buffer
                    //zip.CodecBufferSize = 65536*16; //1MB buffer
                    //zip.ParallelDeflateThreshold = -1; //single threaded
                    //for this zip file instance, for each entry in the zip file,
                    //change the "versiondir" path to this version of tanks
                    args.ChildTotalToProcess = zip.Entries.Count;
                    for (int i = 0; i < zip.Entries.Count; i++)
                    {
                        //grab the entry name for modifications
                        string zipEntryName = zip[i].FileName;
                        zipEntryName = zipEntryName.Contains("versiondir") ? zipEntryName.Replace("versiondir", TanksVersion) : zipEntryName;
                        zipEntryName = zipEntryName.Contains("configs/xvm/xvmConfigFolderName") ? zipEntryName.Replace("configs/xvm/xvmConfigFolderName", "configs/xvm/" + xvmConfigDir) : zipEntryName;
                        if(Regex.IsMatch(zipEntryName, @"_patch.*\.xml"))
                        {
                            string patchName = zipEntryName;
                            zipEntryName = Regex.Replace(zipEntryName, @"_patch.*\.xml", "_patch/" + patchNum.ToString("D3") + ".xml");
                            patchNum++;
                            patchName = patchName.Substring(7);
                            originalPatchNames.Add(patchName);
                        }
                        //finish entry name modifications
                        zip[i].FileName = zipEntryName;
                        //put the entries on disk
                        fs.Write(Encoding.UTF8.GetBytes(Path.Combine(extractFolder, zip[i].FileName) + "\n"), 0, Encoding.UTF8.GetByteCount(Path.Combine(extractFolder, zip[i].FileName) + "\n"));
                    }
                    zip.ExtractProgress += Zip_ExtractProgress;
                    zip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch (Exception e)
            {
                //append the exception to the log
                Utils.ExceptionLog("Unzip", "ZipFile: " + zipFile, e);
                //show the error message
                MessageBox.Show(string.Format("{0}, {1} {2} {3}", Translations.getTranslatedString("zipReadingErrorMessage1"), Path.GetFileName(zipFile), Translations.getTranslatedString("zipReadingErrorMessage2"), Translations.getTranslatedString("zipReadingErrorHeader")));
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
        }
        
        //handler for when progress is made in extracting a zip file
        void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            args.ChildProcessed = e.EntriesExtracted;
            if (e.CurrentEntry != null)
            {
                args.currentFile = e.CurrentEntry.FileName;
                args.currentFileSizeProcessed = e.BytesTransferred;
            }
            InstallWorker.ReportProgress(0);
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
                    GlobalDependencies = null;
                    Dependencies = null;
                    LogicalDependencies = null;
                    ModsConfigsToInstall = null;
                    AppendedDependencies = null;
                    ModsConfigsWithData = null;
                    AppendedDependencies = null;
                    Shortcuts = null;
                    
                    UserMods = null;
                    patchList = null;
                    args = null;
                    originalPatchNames = null;
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
