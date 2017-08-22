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
        public List<Dependency> LogicalDependencies { get; set; }
        public List<Mod> ModsToInstall { get; set; }
        public List<List<Config>> ConfigListsToInstall { get; set; }
        public List<Mod> ModsWithData { get; set; }
        public List<Config> ConfigsWithData { get; set; }
        public List<Mod> UserMods { get; set; }
        private List<Patch> patchList { get; set; }
        public string TanksVersion { get; set; }
        //the folder of the current user appdata
        public string AppDataFolder { get; set; }

        //properties relevent to the handler and install
        private BackgroundWorker InstallWorker;
        private InstallerEventArgs args;
        private string xvmConfigDir = "";

        //the event that it can hook into
        public event InstallChangedEventHandler InstallProgressChanged;

        //the changed event (setups the hander)
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
            InstallWorker.ProgressChanged += WorkerReportProgress;
            InstallWorker.RunWorkerCompleted += WorkerReportComplete;
            args = new InstallerEventArgs();
            ResetArgs();
        }

        //Start installation on the UI thread
        public void StartInstallation()
        {
            InstallWorker.DoWork += ActuallyStartInstallation;
            InstallWorker.RunWorkerAsync();
        }
        //Start uinstallation on UI thread
        public void StartUninstallation()
        {
            InstallWorker.DoWork += ActuallyStartUnInstallation;
            InstallWorker.RunWorkerAsync();
        }
        //regular uninstallation method. currently does nothing
        public void ActuallyStartUnInstallation(object sender, DoWorkEventArgs e)
        {
            ResetArgs();
            args.InstalProgress = InstallerEventArgs.InstallProgress.Uninstall;
            SmartUninstall();
            Utils.appendToLog("Re-creating directories if they arn't already there");
            //put them back
            if (!Directory.Exists(TanksLocation + "\\res_mods\\" + TanksVersion)) Directory.CreateDirectory(TanksLocation + "\\res_mods\\" + TanksVersion);
            if (!Directory.Exists(TanksLocation + "\\mods\\" + TanksVersion)) Directory.CreateDirectory(TanksLocation + "\\mods\\" + TanksVersion);
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
            DeleteMods();
            //put them back when done
            if (!Directory.Exists(TanksLocation + "\\res_mods\\" + TanksVersion)) Directory.CreateDirectory(TanksLocation + "\\res_mods\\" + TanksVersion);
            if (!Directory.Exists(TanksLocation + "\\mods\\" + TanksVersion)) Directory.CreateDirectory(TanksLocation + "\\mods\\" + TanksVersion);
        }

        //Start the installation on the Wokrer thread
        public void ActuallyStartInstallation(object sender, DoWorkEventArgs e)
        {
            ResetArgs();
            //Step 1: do a backup if requested
            if (Settings.backupModFolder)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.BackupMods;
                BackupMods();
            }
            ResetArgs();
            //Step 2: do a backup of user data
            if (Settings.saveUserData)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.BackupUserData;
                BackupUserData();
            }
            ResetArgs();
            //Step 3: Delete Mods
            if (Settings.cleanInstallation)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.DeleteMods;
                DeleteMods();
            }
            ResetArgs();
            //Step 4: Delete user apadata cache
            if (Settings.clearCache)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.DeleteWoTCache;
                ClearWoTCache();
            }
            ResetArgs();
            //Step 5-9: Extracts Mods
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractGlobalDependencies;
            ExtractDatabaseObjects();
            ResetArgs();
            //Step 10: Restore User Data
            if (Settings.saveUserData)
            {
                args.InstalProgress = InstallerEventArgs.InstallProgress.RestoreUserData;
                RestoreUserData();
            }
            ResetArgs();
            //Step 11: Patch Mods
            args.InstalProgress = InstallerEventArgs.InstallProgress.PatchMods;
            if (Directory.Exists(TanksLocation + "\\_patch"))
                PatchFiles();
            ResetArgs();
            //Step 12: Extract User Mods
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractUserMods;
            if(UserMods.Count > 0)
                ExtractUserMods();
            ResetArgs();
            //Step 13: Patch Mods if User Mods extracted patch files
            args.InstalProgress = InstallerEventArgs.InstallProgress.PatchUserMods;
            if (Directory.Exists(TanksLocation + "\\_patch"))
                PatchFiles();
            ResetArgs();
            //Step 14: Install Fonts
            args.InstalProgress = InstallerEventArgs.InstallProgress.InstallUserFonts;
            if (Directory.Exists(TanksLocation + "\\_fonts"))
                InstallFonts();
        }

        public void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
        {
            OnInstallProgressChanged();
        }

        public void WorkerReportComplete(object sender, AsyncCompletedEventArgs e)
        {
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
            try
            {
                NumFilesToProcess(TanksLocation + "\\res_mods");
                NumFilesToProcess(TanksLocation + "\\mods");
                InstallWorker.ReportProgress(0);
                //don't forget to delete the readme files
                if (Directory.Exists(TanksLocation + "\\_readme"))
                    Directory.Delete(TanksLocation + "\\_readme", true);
                DirectoryDelete(TanksLocation + "\\res_mods", true);
                DirectoryDelete(TanksLocation + "\\mods", true);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("DeleteMods", ex);
            }
        }
        //Step 4: Clear WoT program cache
        public void ClearWoTCache()
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
                Utils.exceptionLog("ClearWoTCache, step 1", ex);
            }

            //2 - recursivly delete entire WorldOfTanks folder
            try
            {
                NumFilesToProcess(AppDataFolder);
                DirectoryDelete(AppDataFolder, true);
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("ClearWoTCache, step 2", ex);
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
        
        //Step 5-9: Extract All DatabaseObjects
        public void ExtractDatabaseObjects()
        {
            //just a double-check to delete all patches
            if (Directory.Exists(TanksLocation + "\\_patch")) Directory.Delete(TanksLocation + "\\_patch", true);
            if (Directory.Exists(TanksLocation + "\\_fonts")) Directory.Delete(TanksLocation + "\\_fonts", true);
            if (!Directory.Exists(TanksLocation + "\\res_mods")) Directory.CreateDirectory(TanksLocation + "\\res_mods");
            if (!Directory.Exists(TanksLocation + "\\mods")) Directory.CreateDirectory(TanksLocation + "\\mods");
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractDependencies;
            //extract RelHax Mods
            Utils.appendToLog("Starting Relhax Modpack Extraction");
            string downloadedFilesDir = Application.StartupPath + "\\RelHaxDownloads\\";
            //calculate the total number of zip files to install
            foreach (Dependency d in Dependencies)
                if (!d.dependencyZipFile.Equals(""))
                    args.ParrentTotalToProcess++;
            
            foreach (Mod m in ModsToInstall)
                if (!m.zipFile.Equals(""))
                    args.ParrentTotalToProcess++;

            foreach (List<Config> cc in ConfigListsToInstall)
                foreach (Config c in cc)
                    if (!c.zipFile.Equals(""))
                        args.ParrentTotalToProcess++;
            InstallWorker.ReportProgress(0);
            //extract dependencies
            foreach (Dependency d in Dependencies)
            {
                Utils.appendToLog("Extracting Dependency " + d.dependencyZipFile);
                if (!d.dependencyZipFile.Equals(""))
                {
                    try
                    {
                        this.Unzip(downloadedFilesDir + d.dependencyZipFile, TanksLocation);
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
            //set xvmConfigDir here because xvm is always a dependency
            xvmConfigDir = Utils.getXVMBootLoc(TanksLocation,null,false);
            //extract mods
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractMods;
            InstallWorker.ReportProgress(0);
            foreach (Mod m in ModsToInstall)
            {
                Utils.appendToLog("Extracting Mod " + m.zipFile);
                if (!m.zipFile.Equals(""))
                    this.Unzip(downloadedFilesDir + m.zipFile, TanksLocation);
                args.ParrentProcessed++;
                InstallWorker.ReportProgress(0);
            }
            //extract configs
            args.InstalProgress = InstallerEventArgs.InstallProgress.ExtractConfigs;
            InstallWorker.ReportProgress(0);
            int configLevel = 0;
            foreach (List<Config> cfgList in ConfigListsToInstall)
            {
                foreach (Config c in cfgList)
                {
                    Utils.appendToLog("Extracting Config " + c.zipFile + " of level " + configLevel);
                    if (!c.zipFile.Equals(""))
                        this.Unzip(downloadedFilesDir + c.zipFile, TanksLocation);
                    args.ParrentProcessed++;
                    InstallWorker.ReportProgress(0);
                }
                configLevel++;
            }
            Utils.appendToLog("Finished Relhax Modpack Extraction");
        }

        //Step 10: Restore User Data
        public void RestoreUserData()
        {
            args.ParrentTotalToProcess = ModsWithData.Count + ConfigsWithData.Count;
            InstallWorker.ReportProgress(0);
            string[] fileList = Directory.GetFiles(Application.StartupPath + "\\RelHaxTemp");
            foreach (Mod m in ModsWithData)
            {
                args.ChildTotalToProcess = m.userFiles.Count;
                foreach (string s in m.userFiles)
                {
                    args.currentFile = s;
                    InstallWorker.ReportProgress(0);
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
                    args.ChildProcessed++;
                    InstallWorker.ReportProgress(0);
                }
                args.ParrentProcessed++;
                InstallWorker.ReportProgress(0);
            }
            foreach (Config cfg in ConfigsWithData)
            {
                args.ChildTotalToProcess = cfg.userFiles.Count;
                foreach (string s in cfg.userFiles)
                {
                    args.currentFile = s;
                    InstallWorker.ReportProgress(0);
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
                    args.ChildProcessed++;
                    InstallWorker.ReportProgress(0);
                }
                args.ParrentProcessed++;
                InstallWorker.ReportProgress(0);
            }
        }

        //Step 11/13: Patch All files
        public void PatchFiles()
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
                    File.SetAttributes(TanksLocation + @"\_patch", FileAttributes.Normal);
                    di = new DirectoryInfo(TanksLocation + @"\_patch");
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
                if (p.nativeProcessingFile != "") { Utils.appendToLog(string.Format("nativeProcessingFile: {0}", p.nativeProcessingFile)); }
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
                        Utils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion);
                    }
                    else if (p.lines.Count() == 1 && tempp == -1)
                    {
                        //perform regex patch on entire file, as one whole string
                        Utils.appendToLog("Regex patch, all lines, whole file, " + p.file + ", " + p.search + ", " + p.replace);
                        Utils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion, -1);
                    }
                    else
                    {
                        foreach (string s in p.lines)
                        {
                            //perform regex patch on specific file lines
                            //will need to be a standard for loop BTW
                            Utils.appendToLog("Regex patch, line " + s + ", " + p.file + ", " + p.search + ", " + p.replace);
                            Utils.RegxPatch(p.file, p.search, p.replace, TanksLocation, TanksVersion, int.Parse(s));
                        }
                    }
                }
                else if (p.type.Equals("xml"))
                {
                    //perform xml patch
                    Utils.appendToLog("Xml patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                    Utils.xmlPatch(p.file, p.path, p.mode, p.search, p.replace, TanksLocation, TanksVersion);
                }
                else if (p.type.Equals("json"))
                {
                    //perform json patch
                    Utils.appendToLog("Json patch, " + p.file + ", " + p.path + ", " + p.replace);
                    Utils.jsonPatch(p.file, p.path, p.replace, p.mode, TanksLocation, TanksVersion);
                }
                else if (p.type.Equals("xvm"))
                {
                    //perform xvm style json patch
                    Utils.appendToLog("XVM patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                    Utils.xvmPatch(p.file, p.path, p.search, p.replace, p.mode, TanksLocation, TanksVersion);
                }
                else if (p.type.Equals("pmod"))
                {
                    //perform pmod/generic style json patch
                    Utils.appendToLog("PMOD/Generic patch, " + p.file + ", " + p.path + ", " + p.mode + ", " + p.search + ", " + p.replace);
                    Utils.pmodPatch(p.file, p.path, p.search, p.replace, p.mode, TanksLocation, TanksVersion);
                }
                args.ParrentProcessed++;
                InstallWorker.ReportProgress(0);
            }
            //all done, delete the patch folder
            if (Directory.Exists(TanksLocation + "\\_patch"))
                Directory.Delete(TanksLocation + "\\_patch", true);
        }

        //Step 14: Install Fonts
        public void InstallFonts()
        {
            Utils.appendToLog("Checking for fonts to install");
            if (!Directory.Exists(TanksLocation + "\\_fonts"))
            {
                Utils.appendToLog("No fonts to install");
                //no fonts to install, done display
                return;
            }
            string[] fonts = Directory.GetFiles(TanksLocation + "\\_fonts",@"*.*",System.IO.SearchOption.TopDirectoryOnly);
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
                /*var fam = System.Windows.Media.Fonts.GetFontFamilies(s);
                foreach (var temp in fam)
                {
                    fontsList.Add(temp.Source.Split('#')[1]);
                }*/
                fontsList.Add(Path.GetFileNameWithoutExtension(s));
            }
            //removes any already installed fonts
            for (int i = 0; i < fontsList.Count; i++)
            {
                //get the name of the font
                using (var fontsCollection = new InstalledFontCollection())
                {
                    //get a list of installed fonts
                    foreach (var fontFamiliy in fontsCollection.Families)
                    {
                        //check if the font name is installed
                        if (fontFamiliy.Name.Equals(fontsList[i]))
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
            Utils.appendToLog("Installing fonts");
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

                if (!File.Exists(TanksLocation + "\\_fonts\\FontReg.exe"))
                {
                    try
                    {
                        using (WebClient downloader = new WebClient())
                            downloader.DownloadFile("http://wotmods.relhaxmodpack.com/RelhaxModpack/Resources/external/FontReg.exe", TanksLocation + "\\_fonts\\FontReg.exe");
                    }
                    catch (WebException ex)
                    {
                        Utils.exceptionLog("InstallFonts", "download FontReg.exe", ex);
                        MessageBox.Show(Translations.getTranslatedString("failedToDownload_1") + " FontReg.exe");
                    }
                }
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "FontReg.exe";
                info.UseShellExecute = true;
                info.Verb = "runas"; // Provides Run as Administrator
                info.Arguments = "/copy";
                info.WorkingDirectory = TanksLocation + "\\_fonts";
                Process installFontss = new Process();
                installFontss.StartInfo = info;
                try
                {
                    installFontss.Start();
                    installFontss.WaitForExit();
                }
                catch (Exception e)
                {
                    Utils.exceptionLog("InstallFonts", "could not start font installer", e);
                    MessageBox.Show(Translations.getTranslatedString("fontsPromptError_1") + TanksLocation + Translations.getTranslatedString("fontsPromptError_2"));
                    Utils.appendToLog("Installation done, but fonts install failed");
                    return;
                }
                if (Directory.Exists(TanksLocation + "\\_fonts"))
                    Directory.Delete(TanksLocation + "\\_fonts", true);
                Utils.appendToLog("Fonts Installed Successfully");
                Utils.appendToLog("Installation done");
                return;
            }
            else
            {
                Utils.appendToLog("Installation done, but fonts install failed");
                return;
            }
        }

        //Step 12: Extract User Mods
        public void ExtractUserMods()
        {
            try
            {
                //set xvm dir location again in case it's just a user mod install
                if (xvmConfigDir == null || xvmConfigDir.Equals(""))
                    xvmConfigDir = Utils.getXVMBootLoc(TanksLocation);
                //extract user mods
                Utils.appendToLog("Starting Relhax Modpack User Mod Extraction");
                string downloadedFilesDir = Application.StartupPath + "\\RelHaxUserMods\\";
                foreach (Mod m in UserMods)
                {
                    if (m.Checked)
                    {
                        Utils.appendToLog("Exracting " + Path.GetFileName(m.zipFile));
                        this.Unzip(downloadedFilesDir + Path.GetFileName(m.zipFile), TanksLocation);
                        InstallWorker.ReportProgress(0);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.exceptionLog("ExtractUserMods", e);
            }
            Utils.appendToLog("Finished Relhax Modpack User Mod Extraction");
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
            // modify the xml filename for logging purpose
            string tmpXmlFilename = Path.GetFileNameWithoutExtension(xmlFile);
            //foreach "patch" node in the "patchs" node of the xml file
            foreach (XmlNode n in patchesList)
            {
                //create a patch instance to take the patch information
                Patch p = new Patch();

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
        }
        //do nothing at this point
        private void SmartUninstall()
        {
            
            
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
                    catch (Exception e)
                    {
                        Utils.exceptionLog("DirectoryDelete", e);
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
                                catch (Exception ex)
                                {
                                    Utils.exceptionLog("Unzip", ex);
                                }
                            }
                            if (Regex.IsMatch(zip[i].FileName, "configs/xvm/xvmConfigFolderName") && !xvmConfigDir.Equals(""))
                            {
                                zip[i].FileName = Regex.Replace(zip[i].FileName, "configs/xvm/xvmConfigFolderName", "configs/xvm/" + xvmConfigDir);
                            }
                            if (Regex.IsMatch(zip[i].FileName, "WoTAppData"))
                            {
                                //TODO: modify so that it extracts to the correct place
                                if (AppDataFolder == null || AppDataFolder.Equals("") || AppDataFolder.Equals("-1"))
                                {
                                    Utils.appendToLog("application tried to extract to WoT cache data, but WoT cache data does not exist");
                                    Utils.appendToLog("instead extracted to 'WoTAppData'");
                                }
                                else
                                {
                                    zip[i].FileName = Regex.Replace(zip[i].FileName, "WoTAppData", AppDataFolder);
                                }
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
                    Utils.deleteMd5HashDatabase(zipFile);
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
                    ModsToInstall = null;
                    ConfigListsToInstall = null;
                    ModsWithData = null;
                    UserMods = null;
                    patchList = null;
                    args = null;
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
