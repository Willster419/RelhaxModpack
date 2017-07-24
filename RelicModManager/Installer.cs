using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Ionic.Zip;

namespace RelhaxModpack
{
    //Delegate to hook up them events
    public delegate void InstallChangedEventHandler(object sender, InstallerEventArgs e);

    public class Installer
    {
        /*
         * This new installer class will handle all of the installation process, effectivly black-boxing the installation, in a single seperate backgroundworker.
         * Then we can get out of using the MainWindow to install. It will handle all of the backing up, copying, extracting and patching of the modpack.
         * This way the code is easier to follow, and has one central place to take care of the entire install process.
         * This also enables us to use syncronous thinking when approaching the installation procedures of the modpack.
         * The main window will create an install instance which will take the following parameters:
         * 1. The path to World_of_Tanks
         * 2. The path to the application (Startup Path)
         * 3. The parsed list of global dependencies
         * 4. The parsed list of Dependencies to extract
         * 5. The parsed list of logical Dependnecies to extract
         * 6. The parsed list of Mods to extract
         * 7. The parsed list of Configs to extract
         * 
         * It will then do the following:
         * 1. Backup mods
         * 2. Backup user data
         * 3. Delete mods
         * 4. Extract global dependencies
         * 5. Extract dependencies
         * 6. Extract logical dependencies
         * 7. Extract mods
         * 8. Extract configs
         * 9. Restore user data
         *10. Patch files
        */
        //everything that it needs to install
        private string TanksLocation { get; set; }
        private string AppPath { get; set; }
        private List<Dependency> GlobalDependencies { get; set; }
        private List<Dependency> Dependencies { get; set; }
        private List<Dependency> LogicalDependencies { get; set; }
        private List<Mod> ModsToInstall { get; set; }
        private List<List<Config>> ConfigListsToInstall { get; set; }
        private List<Mod> ModsWithData { get; set; }
        private List<Config> ConfigsWithData { get; set; }
        private string TanksVersion { get; set; }

        //properties relevent to the handler and install
        private BackgroundWorker InstallWorker;
        private InstallerEventArgs args;
        private bool isParrentDone;
        private string xvmConfigDir = "";

        //the event that it can hook into
        public event InstallChangedEventHandler InstallProgressChanged;

        //the changed event
        protected virtual void OnInstallProgressChanged()
        {
            if (InstallProgressChanged != null)
                InstallProgressChanged(this, args);
        }
        
        //constructer
        public Installer()
        {
            InstallWorker = new BackgroundWorker();
            InstallWorker.WorkerReportsProgress = true;
            InstallWorker.DoWork += ActuallyStartInstallation;
            InstallWorker.ProgressChanged += WorkerReportProgress;
            InstallWorker.RunWorkerCompleted += WorkerReportComplete;
            args = new InstallerEventArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.Idle;
            args.ChildProcessed = 0;
            args.ChildProgressPercent = 0;
            args.ChildTotalToProcess = 0;
            args.currentFile = "";
            args.currentFileSizeProcessed = 0;
            args.ParrentProgressPercent = 0;
        }

        //Start installation on the UI thread
        public void StartInstallation()
        {
            args.InstalProgress = InstallerEventArgs.InstallProgress.PatchMods;
            //this needs to be done AFTER dependency install
            //xvmConfigDir = Utils.getXVMBootLoc(TanksLocation);
        }

        //Start the installation on the Wokrer thread
        public void ActuallyStartInstallation(object sender, DoWorkEventArgs e)
        {
            args.InstalProgress = InstallerEventArgs.InstallProgress.BackupMods;
            //do a backup if requested
            if (Settings.backupModFolder)
                BackupMods();
        }

        public void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
        {
            OnInstallProgressChanged();
        }

        public void WorkerReportComplete(object sender, AsyncCompletedEventArgs e)
        {

        }

        //reset the args
        public void ResetArgs()
        {
            args.InstalProgress = InstallerEventArgs.InstallProgress.Idle;
            args.ChildProcessed = 0;
            args.ChildProgressPercent = 0;
            args.ChildTotalToProcess = 0;
            args.currentFile = "";
            args.currentFileSizeProcessed = 0;
            args.ParrentProgressPercent = 0;
        }

        //Step 1: Backup Mods
        public void BackupMods()
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.BackupMods;
            InstallWorker.ReportProgress(0);
            //backupResMods the mods folder
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxModBackup"))
                Directory.CreateDirectory(Application.StartupPath + "\\RelHaxModBackup");
            //create a new mods folder based on date and time
            //yyyy-MM-dd-HH-mm-ss
            DateTime now = DateTime.Now;
            string folderDateName = String.Format("{0:yyyy-MM-dd-HH-mm-ss}", now);
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxModBackup\\" + folderDateName + "\\res_mods"))
                Directory.CreateDirectory(Application.StartupPath + "\\RelHaxModBackup\\" + folderDateName + "\\res_mods");
            if (!Directory.Exists(Application.StartupPath + "\\RelHaxModBackup\\" + folderDateName + "\\mods"))
                Directory.CreateDirectory(Application.StartupPath + "\\RelHaxModBackup\\" + folderDateName + "\\mods");
            NumFilesToProcess(Application.StartupPath + "\\RelHaxModBackup\\" + folderDateName + "\\mods");
            NumFilesToProcess(Application.StartupPath + "\\RelHaxModBackup\\" + folderDateName + "\\res_mods");
            InstallWorker.ReportProgress(0);
            DirectoryCopy(TanksLocation + "\\res_mods", Application.StartupPath + "\\RelHaxModBackup\\" + folderDateName + "\\res_mods", true);
            DirectoryCopy(TanksLocation + "\\mods", Application.StartupPath + "\\RelHaxModBackup\\" + folderDateName + "\\mods", true);
        }

        //Step 2: Backup User Data
        public void BackupUserData()
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.BackupUserData;
            InstallWorker.ReportProgress(0);
            foreach (Mod m in ModsWithData)
            {
                foreach (string s in m.userFiles)
                {

                    string startLoc = TanksLocation + s;
                    string destLoc = Application.StartupPath + "\\RelHaxTemp\\" + Utils.getValidFilename(m.name + "_" + Path.GetFileName(s));
                    try
                    {
                        if (File.Exists(startLoc))
                            File.Move(startLoc, destLoc);
                    }
                    catch
                    {
                        if (Program.testMode) { MessageBox.Show(string.Format("Error: can not move file.\nstartLoc: \"{0}\"\ndestLoc: \"{1}\"", startLoc, destLoc)); };
                        Utils.appendToLog(string.Format("Error: can not move file. startLoc: \"{0}\" destLoc: \"{1}\"", startLoc, destLoc));
                    }
                }
            }
            foreach (Config cfg in ConfigsWithData)
            {
                foreach (string s in cfg.userFiles)
                {
                    string startLoc = TanksLocation + s;
                    string destLoc = Application.StartupPath + "\\RelHaxTemp\\" + Utils.getValidFilename(cfg.name + "_" + Path.GetFileName(s));
                    try
                    {
                        if (File.Exists(startLoc))
                            File.Move(startLoc, destLoc);
                    }
                    catch
                    {
                        if (Program.testMode) { MessageBox.Show(string.Format("Error: can not move file.\nstartLoc: \"{0}\"\ndestLoc: \"{1}\"", startLoc, destLoc)); };
                        Utils.appendToLog(string.Format("Error: can not move file. startLoc: \"{0}\" destLoc: \"{1}\"", startLoc, destLoc));
                    }
                }
            }
        }

        //Step 3: Ddelete all mods
        public void DeleteMods()
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.RestoreUserData;
            InstallWorker.ReportProgress(0);

        }

        //Step 4-8: Extract All DatabaseObjects
        public void ExtractDatabaseObjects()
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.RestoreUserData;
            InstallWorker.ReportProgress(0);
        }

        //Step 9: Restore User Data
        public void RestoreUserData()
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.RestoreUserData;
            InstallWorker.ReportProgress(0);
            string[] fileList = Directory.GetFiles(Application.StartupPath + "\\RelHaxTemp");
            foreach (Mod m in ModsWithData)
            {
                foreach (string s in m.userFiles)
                {
                    //find the file
                    string parsedFileName = Utils.getValidFilename(m.name + "_" + Path.GetFileName(s));
                    foreach (string ss in fileList)
                    {
                        string thePath = Path.GetFileName(ss);
                        if (thePath.Equals(parsedFileName))
                        {
                            //the file has been found in the temp directory
                            if (!Directory.Exists(TanksLocation + "\\" + Path.GetFullPath(s)))
                                Directory.CreateDirectory(TanksLocation + "\\" + Path.GetDirectoryName(s));
                            if (File.Exists(TanksLocation + "\\" + s))
                                File.Delete(TanksLocation + "\\" + s);
                            File.Move(ss, TanksLocation + "\\" + s);
                        }
                    }
                }
            }
            foreach (Config cfg in ConfigsWithData)
            {
                foreach (string s in cfg.userFiles)
                {
                    //find the file
                    string parsedFileName = Utils.getValidFilename(cfg.name + "_" + Path.GetFileName(s));
                    foreach (string ss in fileList)
                    {
                        string thePath = Path.GetFileName(ss);
                        if (thePath.Equals(parsedFileName))
                        {
                            //the file has been found in the temp directory
                            if (!Directory.Exists(TanksLocation + "\\" + Path.GetFullPath(s)))
                                Directory.CreateDirectory(TanksLocation + "\\" + Path.GetDirectoryName(s));
                            if (File.Exists(TanksLocation + "\\" + s))
                                File.Delete(TanksLocation + "\\" + s);
                            File.Move(ss, TanksLocation + "\\" + s);
                        }
                    }
                }
            }
        }

        //Step 10: Patch All files
        public void PatchFiles()
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.RestoreUserData;
            InstallWorker.ReportProgress(0);
        }

        //gets the total number of files to process to eithor delete or copy
        private void NumFilesToProcess(string folder)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(folder);
            DirectoryInfo[] dirs = dir.GetDirectories();
            // Get the files in the directory
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                //numFilesToProcessInt++;
                args.ChildTotalToProcess++;
            }
            foreach (DirectoryInfo subdir in dirs)
            {
                args.ChildTotalToProcess++;
                NumFilesToProcess(subdir.FullName);
            }
            return;
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
                        Utils.appendToLog("EXCEPTION: UnauthorizedAccessException (call stack traceback)");
                        Utils.appendToLog(e.StackTrace);
                        Utils.appendToLog("inner message: " + e.Message);
                        Utils.appendToLog("source: " + e.Source);
                        Utils.appendToLog("target: " + e.TargetSite);
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
                        catch (IOException e)
                        {
                            Utils.appendToLog("EXCEPTION: IOException (call stack traceback)");
                            Utils.appendToLog(e.StackTrace);
                            Utils.appendToLog("inner message: " + e.Message);
                            Utils.appendToLog("source: " + e.Source);
                            Utils.appendToLog("target: " + e.TargetSite);
                            DialogResult result = MessageBox.Show(Translations.getTranslatedString("deleteErrorMessage"), Translations.getTranslatedString("deleteErrorHeader"), MessageBoxButtons.RetryCancel);
                            if (result == DialogResult.Cancel)
                                Application.Exit();
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Utils.appendToLog("EXCEPTION: UnauthorizedAccessException (call stack traceback)");
                            Utils.appendToLog(e.StackTrace);
                            Utils.appendToLog("inner message: " + e.Message);
                            Utils.appendToLog("source: " + e.Source);
                            Utils.appendToLog("target: " + e.TargetSite);
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
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                InstallWorker.ReportProgress(args.ChildProcessed++);
            }
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
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
        //main unzip worker method
        private void Unzip(string zipFile, string extractFolder)
        {
            string thisVersion = TanksVersion;
            //create a filestream to append installed files log data
            using (FileStream fs = new FileStream(TanksLocation + "\\installedRelhaxFiles.log", FileMode.Append, FileAccess.Write))
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
                                catch (ArgumentException e)
                                {
                                    Utils.appendToLog("EXCEPTION: ArguementException");
                                    Utils.appendToLog(e.StackTrace);
                                    Utils.appendToLog("inner message: " + e.Message);
                                    Utils.appendToLog("source: " + e.Source);
                                    Utils.appendToLog("target: " + e.TargetSite);
                                }
                            }
                            if (Regex.IsMatch(zip[i].FileName, "configs/xvm/xvmConfigFolderName") && !xvmConfigDir.Equals(""))
                            {
                                zip[i].FileName = Regex.Replace(zip[i].FileName, "configs/xvm/xvmConfigFolderName", "configs/xvm/" + xvmConfigDir);
                            }
                            //put the entries on disk
                            fs.Write(Encoding.UTF8.GetBytes(zip[i].FileName + "\n"), 0, Encoding.UTF8.GetByteCount(zip[i].FileName + "\n"));
                        }
                        zip.ExtractProgress += Zip_ExtractProgress;
                        zip.ExtractAll(extractFolder, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                catch (ZipException e)
                {
                    //append the exception to the log
                    Utils.appendToLog("EXCEPTION: ZipException (call stack traceback)");
                    Utils.appendToLog(e.StackTrace);
                    Utils.appendToLog("inner message: " + e.Message);
                    Utils.appendToLog("source: " + e.Source);
                    Utils.appendToLog("target: " + e.TargetSite);
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
                            Utils.appendToLog("EXCEPTION: UnauthorizedAccessException (call stack traceback)");
                            Utils.appendToLog(ex.StackTrace);
                            Utils.appendToLog("inner message: " + ex.Message);
                            Utils.appendToLog("source: " + ex.Source);
                            Utils.appendToLog("target: " + ex.TargetSite);
                            Utils.appendToLog("additional info: tried to delete " + zipFile);
                        }
                    Utils.deleteMd5HashDatabase(zipFile);
                }
            }
        }
        //handler for when progress is made in extracting a zip file
        void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            args.ChildProcessed = e.EntriesExtracted;
            //childMaxProgres = e.EntriesTotal;
            isParrentDone = false;
            if (e.CurrentEntry != null)
            {
                args.currentFile = e.CurrentEntry.FileName;
                args.currentFileSizeProcessed = e.BytesTransferred;
            }
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll)
            {
                isParrentDone = true;
            }
            InstallWorker.ReportProgress(0);
        }
    }
}
