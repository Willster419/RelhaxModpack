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
using System.Drawing.Text;
using System.Threading;

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
        public List<LogicalDependnecy> LogicalDependencies { get; set; }
        public List<Dependency> AppendedDependencies { get; set; }
        public List<DatabaseObject> ModsConfigsToInstall { get; set; }
        public List<DatabaseObject> ModsConfigsWithData { get; set; }
        public List<Mod> UserMods { get; set; }
        private List<Patch> patchList { get; set; }
        public string TanksVersion { get; set; }
        //the folder of the current user appdata
        public string AppDataFolder { get; set; }
        public string DatabaseVersion { get; set; }
        //properties relevent to the handler and install
        private BackgroundWorker InstallWorker;
        private InstallerEventArgs args;
        private string xvmConfigDir = "";
        private int patchNum = 0;
        private List<string> originalPatchNames;

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
            Utils.appendToLog("Uninstallation process finished");
            MessageBox.Show(Translations.getTranslatedString("uninstallFinished"), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Start the installation on the Wokrer thread
        public void ActuallyStartInstallation(object sender, DoWorkEventArgs e)
        {
            ResetArgs();
            //Step 1: do a backup if requested
            Utils.appendToLog("Installation BackupMods");
            if (Settings.backupModFolder)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.BackupMods;
                BackupMods();
            }
            ResetArgs();
            //Step 2: do a backup of user data
            Utils.appendToLog("Installation BackupUserData");
            if (Settings.saveUserData)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.BackupUserData;
                BackupUserData();
            }
            ResetArgs();
            //Step 3: Delete Mods
            Utils.appendToLog("Installation UninstallMods");
            if (Settings.cleanInstallation)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.DeleteMods;
                // DeleteMods();
                UninstallMods();
            }
            ResetArgs();
            //Setp 3a: delete log files
            Utils.appendToLog("Installation logFiles");
            if (Settings.deleteLogs)
            {
                Utils.appendToLog("deleteLogs selected, deleting wot, xvm, and pmod logs");
                try
                {
                    if (File.Exists(Path.Combine(TanksLocation, "python.log")))
                        File.Delete(Path.Combine(TanksLocation, "python.log"));
                    if (File.Exists(Path.Combine(TanksLocation, "xvm.log")))
                        File.Delete(Path.Combine(TanksLocation, "xvm.log"));
                    if (File.Exists(Path.Combine(TanksLocation, "pmod.log")))
                        File.Delete(Path.Combine(TanksLocation, "pmod.log"));
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("ActuallyStartInstallation", "deleteLogs", ex);
                }
            }
            ResetArgs();
            //Step 4: Delete user appdata cache
            Utils.appendToLog("Installation DeleteWoTCache");
            if (Settings.clearCache)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.DeleteWoTCache;
                ClearWoTCache();
            }
            ResetArgs();
            //Step 5-9: Extracts Mods
            Utils.appendToLog("Installation ExtractGlobalDependencies");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractGlobalDependencies;
            ExtractDatabaseObjects();
            ResetArgs();
            //Step 11: Restore User Data
            Utils.appendToLog("Installation RestoreUserData");
            if (Settings.saveUserData)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.RestoreUserData;
                RestoreUserData();
            }
            ResetArgs();
            //Step 12: Patch Mods
            Utils.appendToLog("Installation PatchMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.PatchMods;
            if (Directory.Exists(Path.Combine(TanksLocation, "_patch")))
                PatchFiles();
            ResetArgs();
            //Step 13: InstallFonts

            //Step 14: Extract User Mods
            Utils.appendToLog("Installation ExtractUserMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractUserMods;
            if(UserMods.Count > 0)
                ExtractUserMods();
            ResetArgs();
            //Step 15: Patch Mods if User Mods extracted patch files
            Utils.appendToLog("Installation PatchUserMods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.PatchUserMods;
            if (Directory.Exists(Path.Combine(TanksLocation, "_patch")))
                PatchFiles();
            ResetArgs();
            //Step 16: Install Fonts
            Utils.appendToLog("Installation InstallUserFonts");
            args.InstalProgress = InstallerEventArgs.InstallProgress.InstallUserFonts;
            if (Directory.Exists(Path.Combine(TanksLocation, "_fonts")))
                InstallFonts();
            ResetArgs();
            //Step 17: CheckDatabase and delete outdated or no more needed files
            Utils.appendToLog("Installation CreateShortscuts");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CreateShortCuts;
            CreateShortCuts();
            ResetArgs();
            //Step 18: CheckDatabase and delete outdated or no more needed files
            Utils.appendToLog("Installation CheckDatabase");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CheckDatabase;
            if (!Program.testMode)
            {
                checkForOldZipFiles();
            }
        }

        public void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
        {
            OnInstallProgressChanged();
        }

        public void WorkerReportComplete(object sender, AsyncCompletedEventArgs e)
        {
            Utils.appendToLog("Installation CleanUp");
            args.InstalProgress = InstallerEventArgs.InstallProgress.CleanUp;
            OnInstallProgressChanged();
            Utils.appendToLog("Installation Done");
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
                Utils.exceptionLog("BackupMods", "ex", ex);
            }
        }

        //Step 2: Backup User Data
        public void BackupUserData()
        {
            try
            {
                foreach (DatabaseObject dbo in ModsConfigsWithData)
                {
                    try
                    {
                        foreach (string s in dbo.userFiles)
                        {
                            try
                            {
                                string correctedPath = s.TrimStart('\x005c').Replace(@"\\", @"\");
                                string folderPath = Path.Combine(TanksLocation, Path.GetDirectoryName(correctedPath));
                                if (!Directory.Exists(folderPath)) continue;
                                string[] fileList = Directory.GetFiles(folderPath, Path.GetFileName(correctedPath));   // use the GetFileName(correctedPath) as a search pattern, to only get wanted files
                                foreach (string startLoc in fileList)
                                {
                                    string destLoc = Path.Combine(Application.StartupPath, "RelHaxTemp", Utils.getValidFilename(dbo.name + "_") + Path.GetFileName(startLoc));
                                    try
                                    {
                                        if (File.Exists(@startLoc))
                                        {
                                            File.Move(startLoc, destLoc);
                                            Utils.appendToLog(string.Format("BackupUserData: {0} ({1})", Path.Combine(Path.GetDirectoryName(correctedPath), Path.GetFileName(startLoc)), Path.GetFileName(correctedPath)));
                                        }
                                    }
                                    catch
                                    {
                                        if (Program.testMode) { MessageBox.Show(string.Format("Error: can not move file.\nstartLoc: \"{0}\"\ndestLoc: \"{1}\"", startLoc, destLoc)); };
                                        Utils.appendToLog(string.Format("Error: can not move file. startLoc: \"{0}\" destLoc: \"{1}\"", startLoc, destLoc));
                                    }
                                }
                            }
                            catch (Exception exStartLoc)
                            {
                                Utils.exceptionLog("BackupUserData", "exStartLoc", exStartLoc);
                            }
                        }
                    }
                    catch (Exception exS)
                    {
                        Utils.exceptionLog("BackupUserData", "s", exS);
                    }
                }
            }
            catch (Exception exDbo)
            {
                Utils.exceptionLog("BackupUserData", "dbo", exDbo);
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
                // Utils.appendToLog(line);
                
                if (line.EndsWith("/") | line.EndsWith(@"\"))
                {
                    try
                    {
                        File.SetAttributes(line, FileAttributes.Normal);
                        Directory.Delete(line);
                    }
                    catch       // catch exception if folder is not empty
                    { }
                }
                else
                {
                    try
                    {
                        File.SetAttributes(line, FileAttributes.Normal);
                        File.Delete(line);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // 
                    }
                    catch (FileNotFoundException)
                    {
                        // 
                    }
                    catch (Exception ex)    // here is another problem, so logging it
                    {
                        Utils.exceptionLog("DeleteFilesByList", "delete file: " + line, ex);
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
                        Utils.exceptionLog("UninstallMods", string.Format("e\nChildProcessed: {0}\nChildTotalToProcess: {1}\nlines.Count(): {2}", args.ChildProcessed, args.ChildTotalToProcess, lines.Count()), ex);
                    }
                }
                InstallWorker.ReportProgress(0);
                args.ChildProcessed = 0;
                args.ChildTotalToProcess = lines.Count();
                Utils.appendToLog(string.Format("Elements to delete (from logfile): {0}", lines.Count()));
                try
                {
                    string logFile = Path.Combine(TanksLocation, "logs", "uninstallRelhaxFiles.log");
                    TextWriter tw = null;
                    if (args.InstalProgress == InstallerEventArgs.InstallProgress.Uninstall)
                    {
                        // back the last uninstall logfile
                        if (File.Exists(logFile))
                        {
                            if (File.Exists(logFile + ".bak"))
                                File.Delete(logFile + ".bak");
                            File.Move(logFile, logFile + ".bak");
                        }
                        tw = new StreamWriter(logFile);
                    }
                    try
                    {
                        DeleteFilesByList(lines, true, tw);
                    }
                    catch (Exception ex)
                    {
                        Utils.exceptionLog("UninstallMods", "DeletePass1", ex);
                    }
                    lines = NumFilesToProcess(Path.Combine(TanksLocation, "res_mods"));
                    lines.AddRange(NumFilesToProcess(Path.Combine(TanksLocation, "mods")));
                    // reverse the parsed list, to delete files and folders from the lowest to the highest folder level
                    lines.Reverse();
                    if (tw != null)
                        tw.WriteLine("/*  files and folders deleted after parsing  */");
                    InstallWorker.ReportProgress(0);
                    args.ChildProcessed = 0;
                    args.ChildTotalToProcess = lines.Count();
                    Utils.appendToLog(string.Format("Elements to delete (from parsing): {0}", lines.Count()));
                    try
                    {
                        DeleteFilesByList(lines, true, tw);
                        //don't forget to delete the readme files
                        if (Directory.Exists(Path.Combine(TanksLocation, "_readme")))
                            Directory.Delete(Path.Combine(TanksLocation, "_readme"), true);
                    }
                    catch (Exception ex)
                    {
                        Utils.exceptionLog("UninstallMods", "DeletePass2", ex);
                    }
                    if (tw != null)
                        tw.Close();
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("UninstallMods", "sw", ex);
                }
                try       // if the delete will raise an exception, it will be ignored
                {
                    if (File.Exists(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log")))
                        File.Delete(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log"));
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("UninstallMods", "Delete", ex);
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("UninstallMods", "ex", ex);
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
                Utils.exceptionLog("DeleteMods", ex);
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
                    Utils.appendToLog("ERROR: AppDataFolder not correct, value: " + AppDataFolder);
                    Utils.appendToLog("Aborting ClearWoTCache()");
                    return;
                }
                Utils.appendToLog("Started clearing of WoT cache files");

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
                    Utils.exceptionLog("ClearWoTCache", "step 1", ex);
                }

                //2 - recursivly delete entire WorldOfTanks folder
                try
                {
                    NumFilesToProcess(AppDataFolder);
                    DirectoryDelete(AppDataFolder, true);
                }
                catch (Exception ex)
                {
                    Utils.exceptionLog("ClearWoTCache", "step 2", ex);
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
                    Utils.exceptionLog("ClearWoTCache, step 3", ex);
                }
                Utils.appendToLog("Finished clearing of WoT cache files");
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("ClearWoTCache", "ex", ex);
            }
        }
        
        //Step 5-9: Extract All DatabaseObjects
        public void ExtractDatabaseObjects()
        {
            try
            {
                //just a double-check to delete all patches
                if (Directory.Exists(Path.Combine(TanksLocation, "_patch"))) Directory.Delete(Path.Combine(TanksLocation, "_patch"), true);
                if (Directory.Exists(Path.Combine(TanksLocation, "_fonts"))) Directory.Delete(Path.Combine(TanksLocation, "_fonts"), true);
                if (!Directory.Exists(Path.Combine(TanksLocation, "res_mods"))) Directory.CreateDirectory(Path.Combine(TanksLocation, "res_mods"));
                if (!Directory.Exists(Path.Combine(TanksLocation, "mods"))) Directory.CreateDirectory(Path.Combine(TanksLocation, "mods"));

                //start the entry for the database version in installedRelhaxFiles.log
                File.WriteAllText(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log"), "Database Version: " + DatabaseVersion + "\n");

                //extract RelHax Mods
                Utils.appendToLog("Starting Relhax Modpack Extraction");
                string downloadedFilesDir = Path.Combine(Application.StartupPath, "RelHaxDownloads");
                //calculate the total number of zip files to install
                foreach (Dependency d in GlobalDependencies)
                    if (!d.dependencyZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (Dependency d in Dependencies)
                    if (!d.dependencyZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (LogicalDependnecy d in LogicalDependencies)
                    if (!d.dependencyZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (DatabaseObject dbo in ModsConfigsToInstall)
                    if (!dbo.zipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                foreach (Dependency d in AppendedDependencies)
                    if (!d.dependencyZipFile.Equals(""))
                        args.ParrentTotalToProcess++;

                InstallWorker.ReportProgress(0);
                //extract global dependencies
                foreach (Dependency d in GlobalDependencies)
                {
                    Utils.appendToLog("Extracting Global Dependency " + d.dependencyZipFile);
                    if (!d.dependencyZipFile.Equals(""))
                    {
                        try
                        {
                            this.Unzip(Path.Combine(downloadedFilesDir, d.dependencyZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.exceptionLog("ExtractDatabaseObjects", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + d.dependencyZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
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
                    Utils.appendToLog("Extracting Dependency " + d.dependencyZipFile);
                    if (!d.dependencyZipFile.Equals(""))
                    {
                        try
                        {
                            this.Unzip(Path.Combine(downloadedFilesDir, d.dependencyZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.exceptionLog("ExtractDatabaseObjects", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + d.dependencyZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
                            //exit the application
                            Application.Exit();
                        }
                    }
                    InstallWorker.ReportProgress(0);
                }
                //extract logical dependencies
                args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractLogicalDependencies;
                InstallWorker.ReportProgress(0);
                foreach (LogicalDependnecy d in LogicalDependencies)
                {
                    Utils.appendToLog("Extracting Logical Dependency " + d.dependencyZipFile);
                    if (!d.dependencyZipFile.Equals(""))
                    {
                        try
                        {
                            this.Unzip(Path.Combine(downloadedFilesDir, d.dependencyZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.exceptionLog("ExtractDatabaseObjects", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + d.dependencyZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
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
                foreach (DatabaseObject dbo in ModsConfigsToInstall)
                {
                    Utils.appendToLog("Extracting Mod/Config " + dbo.zipFile);
                    if (!dbo.zipFile.Equals(""))
                    {
                        try
                        {
                            this.Unzip(Path.Combine(downloadedFilesDir, dbo.zipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.exceptionLog("ExtractDatabaseObjects", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + dbo.zipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
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
                    Utils.appendToLog("Extracting Appended Dependency " + d.dependencyZipFile);
                    if (!d.dependencyZipFile.Equals(""))
                    {
                        try
                        {
                            this.Unzip(Path.Combine(downloadedFilesDir, d.dependencyZipFile), TanksLocation);
                            args.ParrentProcessed++;
                        }
                        catch (Exception ex)
                        {
                            //append the exception to the log
                            Utils.exceptionLog("ExtractDatabaseObjects", ex);
                            //show the error message
                            MessageBox.Show(Translations.getTranslatedString("zipReadingErrorMessage1") + ", " + d.dependencyZipFile + " " + Translations.getTranslatedString("zipReadingErrorMessage3"), "");
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
                    Utils.appendToLog("WoTAppData folder detected, moving files to WoT cache folder");
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
                    Utils.processDirectory(folderToMove, false);
                    if (Directory.Exists(folderToMove))
                        Directory.Delete(folderToMove);
                }
                Utils.appendToLog("Finished Relhax Modpack Extraction");
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("ExtractDatabaseObjects", "ex", ex);
            }
        }

        //Step 10: Restore User Data
        public void RestoreUserData()
        {
            try
            {
                args.ParrentTotalToProcess = ModsConfigsWithData.Count;
                InstallWorker.ReportProgress(0);
                foreach (DatabaseObject dbo in ModsConfigsWithData)
                {
                    try
                    {
                        args.ChildTotalToProcess = dbo.userFiles.Count;
                        foreach (string s in dbo.userFiles)
                        {
                            
                            try {
                                string correctedUserFiles = s.TrimStart('\x005c').Replace(@"\\", @"\");
                                string targetDir = Path.GetDirectoryName(correctedUserFiles);
                                args.currentFile = correctedUserFiles;
                                InstallWorker.ReportProgress(0);
                                string filenamePrefix = Utils.getValidFilename(dbo.name + "_");
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
                                        Utils.appendToLog(string.Format("RestoredUserData: {0}", Path.Combine(targetDir, targetFilename)));
                                    }
                                    catch (Exception p)
                                    {
                                        Utils.exceptionLog("RestoreUserData", "p\n" + ss, p);
                                    }
                                }
                                args.ChildProcessed++;
                                InstallWorker.ReportProgress(0);
                            }
                            catch (Exception fl)
                            {
                                Utils.exceptionLog("RestoreUserData", "fl", fl);
                            }
                        }
                        args.ParrentProcessed++;
                        InstallWorker.ReportProgress(0);
                    }
                    catch (Exception uf)
                    {
                        Utils.exceptionLog("RestoreUserData", "uf", uf);
                    }

                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("RestoreUserData", "ex", ex);
            }
        }

        //Step 11/13: Patch All files
        public void PatchFiles()
        {
            try
            {
                //Give the OS time to process the folder change...
                System.Threading.Thread.Sleep(100);
                //set the folder properties to read write
                DirectoryInfo di = null;
                FileInfo[] diArr = null;
                bool kontinue = false;
                while (!kontinue)
                {
                    try
                    {
                        File.SetAttributes(Path.Combine(TanksLocation, "_patch"), FileAttributes.Normal);
                        di = new DirectoryInfo(Path.Combine(TanksLocation, "_patch"));
                        //get every patch file in the folder
                        diArr = di.GetFiles(@"*.xml", System.IO.SearchOption.TopDirectoryOnly);
                        kontinue = true;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Utils.exceptionLog("PatchFiles", e);
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
                    if (p.nativeProcessingFile != "") { Utils.appendToLog(string.Format("nativeProcessingFile: {0}, originalName: {1}", p.nativeProcessingFile, p.actualPatchName)); }
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
                            Utils.appendToLog("Regex patch, all lines, line by line, " + p.file + ", " + p.search + ", " + p.replace);
                            PatchUtils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion);
                        }
                        else if (p.lines.Count() == 1 && tempp == -1)
                        {
                            //perform regex patch on entire file, as one whole string
                            Utils.appendToLog("Regex patch, all lines, whole file, " + p.file + ", " + p.search + ", " + p.replace);
                            PatchUtils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion, -1);
                        }
                        else
                        {
                            foreach (string s in p.lines)
                            {
                                //perform regex patch on specific file lines
                                //will need to be a standard for loop BTW
                                Utils.appendToLog("Regex patch, line " + s + ", " + p.file + ", " + p.search + ", " + p.replace);
                                PatchUtils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion, int.Parse(s));
                            }
                        }
                    }
                    else if (p.type.Equals("xml"))
                    {
                        //perform xml patch
                        Utils.appendToLog("Xml patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                        PatchUtils.XMLPatch(p.file, p.path, p.mode, p.search, p.replace, TanksLocation, TanksVersion);
                    }
                    else if (p.type.Equals("json"))
                    {
                        //perform json patch
                        Utils.appendToLog("Json patch, " + p.file + ", " + p.path + ", " + p.replace);
                        PatchUtils.JSONPatch(p.file, p.path, p.replace, p.mode, TanksLocation, TanksVersion);
                    }
                    else if (p.type.Equals("xvm"))
                    {
                        //perform xvm style json patch
                        Utils.appendToLog("XVM patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                        PatchUtils.XVMPatch(p.file, p.path, p.search, p.replace, p.mode, TanksLocation, TanksVersion);
                    }
                    else if (p.type.Equals("pmod"))
                    {
                        //perform pmod/generic style json patch
                        Utils.appendToLog("PMOD/Generic patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
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
                Utils.exceptionLog("PatchFiles","ex", ex);
            }
        }

        //Step 14: Install Fonts
        public void InstallFonts()
        {
            try
            {
                Utils.appendToLog("Checking for fonts to install");
                if (!Directory.Exists(Path.Combine(TanksLocation, "_fonts")))
                {
                    Utils.appendToLog("No fonts to install");
                    //no fonts to install, done display
                    return;
                }
                string[] fonts = Directory.GetFiles(Path.Combine(TanksLocation, "_fonts"), @"*.*",System.IO.SearchOption.TopDirectoryOnly);
                if (fonts.Count() == 0)
                {
                    //done display
                    Utils.appendToLog("No fonts to install");
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
                        Utils.appendToLog("No fonts to install");
                        //done display
                        return;
                    }
                    Utils.appendToLog("Installing fonts: " + string.Join(", ", fontsList));
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
                                using (ZipFile zip = new ZipFile(Settings.managerInfoDatFile))
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
                                    Utils.exceptionLog("InstallFonts()", "download FontReg.exe", ex);
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
                            Utils.appendToLog("FontReg.exe ExitCode: " + installFontss.ExitCode);
                        }
                        catch (Exception e)
                        {
                            Utils.exceptionLog("InstallFonts", "could not start font installer", e);
                            MessageBox.Show(Translations.getTranslatedString("fontsPromptError_1") + TanksLocation + Translations.getTranslatedString("fontsPromptError_2"));
                            Utils.appendToLog("Installation done, but fonts install failed");
                            return;
                        }
                        Utils.appendToLog("Fonts Installed Successfully");
                        return;
                    }
                    else
                    {
                        Utils.appendToLog("Installation done, but fonts install failed");
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
                Utils.exceptionLog("InstallFonts()", ex);
            }
        }

        //Step 12: Extract User Mods
        public void ExtractUserMods()
        {
            try
            {
                //set xvm dir location again in case it's just a user mod install
                if (xvmConfigDir == null || xvmConfigDir.Equals(""))
                    xvmConfigDir = PatchUtils.GetXVMBootLoc(TanksLocation);
                //extract user mods
                Utils.appendToLog("Starting Relhax Modpack User Mod Extraction");
                string downloadedFilesDir = Path.Combine(Application.StartupPath, "RelHaxUserMods");
                foreach (Mod m in UserMods)
                {
                    if (m.Checked)
                    {
                        Utils.appendToLog("Exracting " + Path.GetFileName(m.zipFile));
                        this.Unzip(Path.Combine(downloadedFilesDir, Path.GetFileName(m.zipFile)), TanksLocation);
                        InstallWorker.ReportProgress(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("ExtractUserMods", "ex", ex);
            }
            Utils.appendToLog("Finished Relhax Modpack User Mod Extraction");
        }

        //Step 17: Extract User Mods
        private void CreateShortCuts()
        {
            try
            {
                string logFile = Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log");
                List<ShortCut> scToDoList = new List<ShortCut>();
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine(@"/*  Desktop shortcuts  */");
                    foreach (Dependency d in Dependencies)
                    {
                        if (d.shortCuts.Count > 0)
                        {
                            foreach (ShortCut sc in d.shortCuts)
                            {
                                if (sc.enabled)
                                {

                                    string fileTarget = Path.Combine(TanksLocation, sc.path);
                                    Utils.appendToLog(string.Format("creating desktop ShortCut: {0} ({1})", sc.path, sc.name));
                                    if (File.Exists(fileTarget))
                                    {
                                        Utils.CreateShortcut(fileTarget, sc.name, true, true, sw);
                                    }
                                }
                            }
                        }
                    }
                    foreach (LogicalDependnecy ld in LogicalDependencies)
                    {
                        if (ld.shortCuts.Count > 0)
                        {
                            foreach (ShortCut sc in ld.shortCuts)
                            {
                                if (sc.enabled)
                                {
                                    string fileTarget = Path.Combine(TanksLocation, sc.path);
                                    Utils.appendToLog(string.Format("creating desktop ShortCut: {0} ({1})", sc.path, sc.name));
                                    if (File.Exists(fileTarget))
                                    {
                                        Utils.CreateShortcut(fileTarget, sc.name, true, true, sw);
                                    }
                                }
                            }
                        }
                    }
                    foreach (Dependency ad in AppendedDependencies)
                    {
                        if (ad.shortCuts.Count > 0)
                        {
                            foreach (ShortCut sc in ad.shortCuts)
                            {
                                if (sc.enabled)
                                {
                                    string fileTarget = Path.Combine(TanksLocation, sc.path);
                                    Utils.appendToLog(string.Format("creating desktop ShortCut: {0} ({1})", sc.path, sc.name));
                                    if (File.Exists(fileTarget))
                                    {
                                        Utils.CreateShortcut(fileTarget, sc.name, true, true, sw);
                                    }
                                }
                            }
                        }
                    }
                    foreach (DatabaseObject dbo in ModsConfigsToInstall)
                    {
                        if (dbo.shortCuts.Count > 0)
                        {
                            foreach (ShortCut sc in dbo.shortCuts)
                            {
                                if (sc.enabled)
                                {
                                    string fileTarget = Path.Combine(TanksLocation, sc.path);
                                    Utils.appendToLog(string.Format("creating desktop ShortCut: {0} ({1})", sc.path, sc.name));
                                    if (File.Exists(fileTarget))
                                    {
                                        Utils.CreateShortcut(fileTarget, sc.name, true, true, sw);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("CreateShortCuts()", ex);
            }
        }

        //Step 15: Check the Database for outdated or no more needed files
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
                    Utils.exceptionLog("checkForOldZipFiles", ex);
                    MessageBox.Show(string.Format(Translations.getTranslatedString("parseDownloadFolderFailed"), "RelHaxDownloads"));
                }
                args.ParrentProcessed = 2;
                if (fi != null)
                {
                    foreach (FileInfo f in fi)
                    {
                        zipFilesList.Add(f.Name);
                    }
                    //MainWindow.usedFilesList has every single possible zipFile of the modInfo database
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
            catch (Exception ex)
            {
                Utils.exceptionLog("checkForOldZipFiles", "ex", ex);
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
                Utils.exceptionLog("createPatchList", "ex", ex);
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
                        Utils.exceptionLog("DirectoryDelete", file.FullName, e);
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
                            Utils.exceptionLog("DirectoryDelete","deleteSubDirs", ex);
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
            Utils.processDirectory(sourceDirName, false);
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
            string thisVersion = TanksVersion;
            //create a filestream to append installed files log data
            using (FileStream fs = new FileStream(Path.Combine(TanksLocation, "logs", "installedRelhaxFiles.log"), FileMode.Append, FileAccess.Write))
            {
                // create a comment with the name of the extracted and installed package, to better trace back the installation source
                string commentLine = "/*  " + Path.GetFileNameWithoutExtension(zipFile) + "  */\n";
                // write comment to logfile
                fs.Write(Encoding.UTF8.GetBytes(commentLine), 0, Encoding.UTF8.GetByteCount(commentLine));

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
                            if (Regex.IsMatch(zip[i].FileName, "versiondir"))
                            {
                                try
                                {
                                    zip[i].FileName = Regex.Replace(zip[i].FileName, "versiondir", thisVersion);
                                }
                                catch (Exception ex)
                                {
                                    Utils.exceptionLog("Unzip", ex);
                                }
                            }
                            if (Regex.IsMatch(zip[i].FileName, "configs/xvm/xvmConfigFolderName") && !xvmConfigDir.Equals(""))
                            {
                                zip[i].FileName = Regex.Replace(zip[i].FileName, "configs/xvm/xvmConfigFolderName", "configs/xvm/" + xvmConfigDir);
                            }
                            if(Regex.IsMatch(zip[i].FileName, @"_patch.*\.xml"))
                            {
                                string patchName = zip[i].FileName;
                                zip[i].FileName = Regex.Replace(zip[i].FileName, @"_patch.*\.xml", "_patch/" + patchNum.ToString("D3") + ".xml");
                                patchNum++;
                                patchName = patchName.Substring(7);
                                originalPatchNames.Add(patchName);
                            }
                            //put the entries on disk
                            fs.Write(Encoding.UTF8.GetBytes(Path.Combine(extractFolder, zip[i].FileName) + "\n"), 0, Encoding.UTF8.GetByteCount(Path.Combine(extractFolder, zip[i].FileName) + "\n"));
                        }
                        zip.ExtractProgress += Zip_ExtractProgress;
                        zip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                catch (ZipException e)
                {
                    //append the exception to the log
                    Utils.exceptionLog("Unzip", e);
                    //show the error message
                    MessageBox.Show(string.Format("{0}, {1} {2} {3}", Translations.getTranslatedString("zipReadingErrorMessage1"), Path.GetFileName(zipFile), Translations.getTranslatedString("zipReadingErrorMessage2"), Translations.getTranslatedString("zipReadingErrorHeader")));
                    //(try to)delete the file from the filesystem
                    if (File.Exists(zipFile))
                        try
                        {
                            File.Delete(zipFile);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Utils.exceptionLog("Unzip", "tried to delete " + zipFile, ex);
                        }
                    XMLUtils.DeleteMd5HashDatabase(zipFile);
                }
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
