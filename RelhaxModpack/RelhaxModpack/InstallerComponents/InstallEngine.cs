using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using RelhaxModpack.UIComponents;
using System.IO;
using System.Xml;
using System.Windows;
using Ionic.Zip;
using System.Text.RegularExpressions;

namespace RelhaxModpack.InstallerComponents
{
    #region Event stuff
    public enum InstallerExitCodes
    {
        Success,//starts at 0
        DownloadModsError,
        BackupModsError,
        BackupDataError,
        ClearCacheError,
        ClearLogsError,
        CleanModsError,
        ExtractionError,
        UserExtractionError,
        RestoreUserdataError,
        XmlUnpackError,
        PatchError,
        ShortcustError,
        ContourIconAtlasError,
        FontInstallError,
        TrimDownloadCacheError,
        CleanupError,
        UnknownError
    }
    public enum UninstallerExitCodes
    {
        GettingFilelistError,
        UninstallError,
        ProcessingEmptyFolders,
        PerformFinalClearup,
        Success
    }
    public class RelhaxInstallFinishedEventArgs : EventArgs
    {
        public InstallerExitCodes ExitCodes;
        public string ErrorMessage;
        public List<Category> ParsedCategoryList;
        public List<Dependency> Dependencies;
        public List<DatabasePackage> GlobalDependencies;
    }
    public class RelhaxZipFile : ZipFile
    {
        public int ThreadID;

        public RelhaxZipFile(string fileName) : base(fileName) { }
    }
    #endregion

    public class InstallEngine : IDisposable
    {
        #region Instance Variables
        //used for installation
        /// <summary>
        /// List of packages that have zip files to install and are enabled (and checked if selectable) and ordered into installGroups
        /// </summary>
        public List<DatabasePackage>[] OrderedPackagesToInstall;
        /// <summary>
        /// List of packages that have zip files to install and are enabled (and checked if selectable)
        /// </summary>
        public List<DatabasePackage> PackagesToInstall;
        /// <summary>
        /// Flat list of all selectable packages
        /// </summary>
        public List<SelectablePackage> FlatListSelectablePackages;

        //for passing back to application (DO NOT WRITE TO)
        public List<Category> ParsedCategoryList;
        public List<Dependency> Dependencies;
        public List<DatabasePackage> GlobalDependencies;

        //names of triggers
        public const string TriggerContouricons = "build_contour_icons";
        public const string TriggerInstallFonts = "install_fonts";

        public static readonly string[] CompleteTriggerList = new string[]
        {
            TriggerContouricons,
            TriggerInstallFonts
        };

        //trigger array
        public static List<Trigger> Triggers = new List<Trigger>
        {
            new Trigger(){ Fired = false, Name = TriggerContouricons, NumberProcessed = 0, Total = 0, TriggerTask = null },
            new Trigger(){ Fired = false, Name = TriggerInstallFonts, NumberProcessed = 0, Total = 0, TriggerTask = null }
        };

        //other
        private Stopwatch InstallStopWatch = new Stopwatch();
        private TimeSpan OldTime;
        private RelhaxInstallFinishedEventArgs InstallFinishedArgs = new RelhaxInstallFinishedEventArgs();
        private IProgress<RelhaxInstallerProgress> Progress = null;
        private RelhaxInstallerProgress Prog = null;
        #endregion

        #region More boring stuff
        public InstallEngine()
        {
            //init the install engine
        }
        #endregion

        #region Installer entry points

        public Task<RelhaxInstallFinishedEventArgs> RunInstallationAsync(IProgress<RelhaxInstallerProgress> progress)
        {
            //make the progress report objects
            Prog = new RelhaxInstallerProgress();
            Progress = progress;

            Task<RelhaxInstallFinishedEventArgs> task = Task.Run(() =>
            {
                RelhaxInstallFinishedEventArgs t = RunInstallation();

                //stop the logfile if it was started
                if (Logging.IsLogOpen(Logfiles.Installer))
                    Logging.DisposeLogging(Logfiles.Installer);

                return t;
            });
            return task;
        }
        
        public Task<RelhaxInstallFinishedEventArgs> RunUninstallationAsync(IProgress<RelhaxInstallerProgress> progress)
        {
            //make the progress report object
            Prog = new RelhaxInstallerProgress();
            Progress = progress;

            Task<RelhaxInstallFinishedEventArgs> task = Task.Run(() => RunUninstallation());
            return task;
        }

        private RelhaxInstallFinishedEventArgs RunUninstallation()
        {
            Logging.Info("Uninstall process starts on new thread with mode {0}", ModpackSettings.UninstallMode.ToString());

            //run the uninstall methods
            bool success = true;
            switch (ModpackSettings.UninstallMode)
            {
                case UninstallModes.Default:
                    success = UninstallModsDefault(true);
                    break;
                case UninstallModes.Quick:
                    success = UninstallModsQuick(true);
                    break;
            }

            if (success)
                InstallFinishedArgs.ExitCodes = InstallerExitCodes.Success;
            else
                InstallFinishedArgs.ExitCodes = InstallerExitCodes.UnknownError;

            InstallFinishedArgs.ParsedCategoryList = ParsedCategoryList;
            InstallFinishedArgs.Dependencies = Dependencies;
            InstallFinishedArgs.GlobalDependencies = GlobalDependencies;

            return InstallFinishedArgs;
        }
        #endregion

        #region Main Install method
        private RelhaxInstallFinishedEventArgs RunInstallation()
        {
            //rookie mistake checks
            if(OrderedPackagesToInstall == null || OrderedPackagesToInstall.Count() == 0)
                throw new BadMemeException("WOW you really suck at programming");

            Logging.WriteToLog("Installation starts now from RunInstallation() in Install Engine");
            //do more stuff here I'm sure like init log files
            //also check enabled just to be safe
            List<SelectablePackage> selectedPackages = FlatListSelectablePackages.Where(package => package.Checked && package.Enabled).ToList();

            //do any list processing here
            if(!ModpackSettings.DisableTriggers)
            {
                //process any packages that have triggers
                //reset the internal list first
                foreach (Trigger trig in Triggers)
                {
                    if (trig.Total != 0)
                        trig.Total = 0;
                    if (trig.NumberProcessed != 0)
                        trig.NumberProcessed = 0;
                    trig.Fired = false;
                    trig.TriggerTask = null;
                }
                foreach (DatabasePackage package in PackagesToInstall)
                {
                    if (package.Triggers.Count > 0)
                    {
                        foreach (string triggerFromPackage in package.Triggers)
                        {
                            //in theory, each database package trigger is unique in each package AND in installer
                            Trigger match = Triggers.Find(search => search.Name.ToLower().Equals(triggerFromPackage.ToLower()));
                            if (match == null)
                            {
                                Logging.Debug("trigger match is null (no match!) {0}", triggerFromPackage);
                                continue;
                            }
                            match.Total++;
                        }
                    }
                }
            }

            //process packages with data (cache) for cache backup and restore
            List<SelectablePackage> packagesWithData = selectedPackages.Where(package => package.UserFiles.Count > 0).ToList();

            //and reset the stopwatch
            InstallStopWatch.Restart();

            //step 1 on install: backup user mods
            OldTime = InstallStopWatch.Elapsed;
            //unknown error is last step in ints
            Prog.TotalTotal = (int)InstallerExitCodes.UnknownError;
            Prog.TotalCurrent = 1;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.BackupModsError;
            Prog.InstallStatus = InstallerExitCodes.BackupModsError;
            Progress.Report(Prog);

            Logging.WriteToLog("Backup of mods, current install time = 0 msec");
            if (ModpackSettings.BackupModFolder)
            {
                if (! BackupMods())
                {
                    return InstallFinishedArgs;
                }
                Logging.WriteToLog(string.Format("Backup of mods complete, took {0} msec",(int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds)));
            }
            else
                Logging.WriteToLog("...skipped");

            //step 2: backup data
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent++;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.BackupDataError;
            Prog.InstallStatus = InstallerExitCodes.BackupDataError;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Backup of user data, current install time = {0} msec",
                (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.SaveUserData)
            {
                if(!BackupData(packagesWithData))
                {
                    return InstallFinishedArgs;
                }
                Logging.Info("Back of user data complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped");

            //step 3: clear cache
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent++;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.ClearCacheError;
            Prog.InstallStatus = InstallerExitCodes.ClearCacheError;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Cleaning of cache folders, current install time = {0} msec",
                (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.ClearCache)
            {
                if(!ClearCache())
                {
                    return InstallFinishedArgs;
                }
                Logging.Info("Cleaning of cache complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped");

            //step 4: clear logs
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent++;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.ClearLogsError;
            Prog.InstallStatus = InstallerExitCodes.ClearLogsError;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Cleaning of logs, current install time = {0} msec",
                (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.DeleteLogs)
            {
                if(!ClearLogs())
                {
                    return InstallFinishedArgs;
                }
                Logging.Info("Cleaning of Logs complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped");

            //step 5: clean mods folders
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent++;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.CleanModsError;
            Prog.InstallStatus = InstallerExitCodes.CleanModsError;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Cleaning of mods folders, current install time = {0} msec",
                (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.CleanInstallation)
            {
                if (!ClearModsFolders())
                {
                    return InstallFinishedArgs;
                }
                Logging.Info("Cleaning of mods folders complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped");

            //backup the last installed log file
            string backupInstallLogfile = Path.Combine(Settings.WoTDirectory, "logs", Logging.InstallLogFilenameBackup);
            string installLogfile = Path.Combine(Settings.WoTDirectory, "logs", Logging.InstallLogFilename);
            if (File.Exists(backupInstallLogfile))
                Utils.FileDelete(backupInstallLogfile);
            if (File.Exists(installLogfile))
                File.Move(installLogfile, backupInstallLogfile);

            //start the logfile for the installer
            Logging.Init(Logfiles.Installer, installLogfile);

            //step 6: extract mods
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent++;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.ExtractionError;
            Prog.InstallStatus = InstallerExitCodes.ExtractionError;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Extracting mods, current install time = {0} msec",
                (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            if(!ExtractFilesAsyncSetup())
            {
                return InstallFinishedArgs;
            }
            Logging.Info("Extraction complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));

            //step 7: restore data
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent++;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.RestoreUserdataError;
            Prog.InstallStatus = InstallerExitCodes.RestoreUserdataError;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Restore of user data, current install time = {0} msec", (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.SaveUserData)
            {
                if (packagesWithData.Count > 0)
                {
                    StringBuilder restoreDataBuilder = new StringBuilder();
                    restoreDataBuilder.AppendLine("/*   Restored data   */");
                    if (!RestoreData(packagesWithData, restoreDataBuilder))
                    {
                        return InstallFinishedArgs;
                    }
                    Logging.Installer(restoreDataBuilder.ToString());
                }
                Logging.Info("Restore of data complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped");

            //step 8: unpack xml files
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent++;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.XmlUnpackError;
            Prog.InstallStatus = InstallerExitCodes.XmlUnpackError;
            Prog.ChildTotal = 1;
            Prog.ParrentTotal = 1;
            Prog.ParrentCurrent = Prog.ChildCurrent = 0;
            Prog.Filename = string.Empty;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Unpack of xml files, current install time = {0} msec", (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            List<XmlUnpack> xmlUnpacks = MakeXmlUnpackList();
            if (xmlUnpacks.Count > 0)
            {
                //progress
                Prog.ParrentTotal = xmlUnpacks.Count;

                StringBuilder unpackBuilder = new StringBuilder();
                unpackBuilder.AppendLine("/*   Unpack xml files   */");
                foreach (XmlUnpack xmlUnpack in xmlUnpacks)
                {
                    Prog.ParrentCurrent++;
                    Prog.Filename = xmlUnpack.FileName;
                    Progress.Report(Prog);

                    XMLUtils.UnpackXmlFile(xmlUnpack, unpackBuilder);
                }
                Logging.Installer(unpackBuilder.ToString());
                Logging.Info("Unpack of xml files complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped (no XmlUnpack entries parsed");

            //step 8: patch files (async option)
            //make the task array here. so far can be a maximum of 3 items
            Task patchTask = null;
            Task createShortcutsTask = null;
            Task createAtlasesTask = null;
            Task createFontsTask = null;
            Task[] concurrentTasksAfterMainExtractoin = new Task[]
            {
                patchTask,
                createShortcutsTask,
                createAtlasesTask,
                createFontsTask
            };

            OldTime = InstallStopWatch.Elapsed;

            Logging.WriteToLog(string.Format("Patching of files, current install time = {0} msec",
                (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            List<Patch> pathces = MakePatchList();
            if (pathces.Count > 0)
            {
                //no need to installer log patches, since it's operating on files that already exist
                patchTask = Task.Factory.StartNew(() =>
                {
                    Prog.ChildTotal = 1;
                    Prog.ParrentTotal = pathces.Count;
                    Prog.ParrentCurrent = Prog.ChildCurrent = 0;
                    Prog.Filename = string.Empty;
                    Progress.Report(Prog);

                    foreach (Patch patch in pathces)
                    {
                        Prog.Filename = patch.File;
                        Prog.ParrentCurrent++;
                        Progress.Report(Prog);

                        PatchUtils.RunPatch(patch);
                    }
                    Logging.Info("Patching of files complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
                    Prog.TotalCurrent = (int)InstallerExitCodes.PatchError;
                    InstallFinishedArgs.ExitCodes = InstallerExitCodes.PatchError;
                    Prog.InstallStatus = InstallerExitCodes.PatchError;
                    Progress.Report(Prog);
                });
            }
            else
                Logging.WriteToLog("...skipped (no patch entries parsed)");

            //step 9: create shortcuts (async option)

            Logging.WriteToLog(string.Format("Creating of shortcuts, current install time = {0} msec",
                (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            List<Shortcut> shortcuts = MakeShortcutList();
            if (shortcuts.Count > 0)
            {
                StringBuilder shortcutBuilder = new StringBuilder();
                shortcutBuilder.AppendLine("/*   Shortcuts   */");
                createShortcutsTask = Task.Factory.StartNew(() =>
                {
                    if (TaskNullOrDone(patchTask))
                    {
                        Prog.ChildTotal = 1;
                        Prog.ParrentTotal = shortcuts.Count;
                        Prog.ParrentCurrent = 0;
                        Prog.ChildCurrent = 0;
                        Prog.Filename = string.Empty;
                        LockProgress();
                    }
                    int numDone = 0;
                    foreach (Shortcut shortcut in shortcuts)
                    {
                        numDone++;
                        if (TaskNullOrDone(patchTask))
                        {
                            Prog.ChildTotal = 1;
                            Prog.ParrentTotal = shortcuts.Count;
                            Prog.ParrentCurrent = numDone;
                            Prog.ChildCurrent = 0;
                            Prog.Filename = shortcut.Path;
                            LockProgress();
                        }

                        if (shortcut.Enabled)
                        {
                            Utils.CreateShortcut(shortcut, shortcutBuilder);
                        }
                    }
                    Logging.Installer(shortcutBuilder.ToString());
                    Logging.Info("Creating of shortcuts complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
                    if(TaskNullOrDone(patchTask))
                    {
                        Prog.TotalCurrent = (int)InstallerExitCodes.ShortcustError;
                        InstallFinishedArgs.ExitCodes = InstallerExitCodes.ShortcustError;
                        Prog.InstallStatus = InstallerExitCodes.ShortcustError;
                        LockProgress();
                    }
                });
            }
            else
                Logging.WriteToLog("...skipped (no shortcut entries parsed)");

            //step 10: create atlases (async option)
            if (ModpackSettings.DisableTriggers)
            {
                Logging.WriteToLog(string.Format("Creating of atlases, current install time = {0} msec", (int)InstallStopWatch.Elapsed.TotalMilliseconds));
                List<Atlas> atlases = MakeAtlasList();
                if (atlases.Count > 0)
                {
                    //NOTE: stringbuilder logging is done inside BuildContourIcons()
                    createAtlasesTask = Task.Factory.StartNew(() =>
                    {
                        if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask))
                        {
                            Prog.ChildTotal = 1;
                            Prog.ParrentTotal = 1;
                            Prog.ParrentCurrent = Prog.ChildCurrent = 0;
                            Prog.Filename = string.Empty;
                            LockProgress();
                        }
                        
                        BuildContourIcons(patchTask, createShortcutsTask);
                        Logging.Info("Creating of atlases complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
                        if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask))
                        {
                            Prog.TotalCurrent = (int)InstallerExitCodes.ContourIconAtlasError;
                            InstallFinishedArgs.ExitCodes = InstallerExitCodes.ContourIconAtlasError;
                            Prog.InstallStatus = InstallerExitCodes.ContourIconAtlasError;
                            LockProgress();
                        }
                    });
                }
                else
                    Logging.WriteToLog("...skipped (no atlas entries parsed)");
            }

            //step 11: install fonts (async operation)
            if (ModpackSettings.DisableTriggers)
            {
                Logging.WriteToLog(string.Format("Installing of fonts, current install time = {0} msec",
                    (int)InstallStopWatch.Elapsed.TotalMilliseconds));
                string[] fontsToInstall = Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, Settings.FontsToInstallFoldername), SearchOption.TopDirectoryOnly, false, @"*", 50, 3, true);
                if (fontsToInstall == null || fontsToInstall.Count() == 0)
                    Logging.WriteToLog("...skipped (no font files to install)");
                else
                {
                    createFontsTask = Task.Factory.StartNew(() =>
                    {
                        if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask) && TaskNullOrDone(createAtlasesTask))
                        {
                            Prog.ChildTotal = 1;
                            Prog.ParrentTotal = 1;
                            Prog.ParrentCurrent = Prog.ChildCurrent = 0;
                            Prog.Filename = string.Empty;
                            LockProgress();
                        }
                        InstallFonts(fontsToInstall, patchTask, createShortcutsTask, createAtlasesTask);
                        Logging.Info("Installing of fonts complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
                        if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask) && TaskNullOrDone(createAtlasesTask))
                        {
                            Prog.TotalCurrent = (int)InstallerExitCodes.FontInstallError;
                            InstallFinishedArgs.ExitCodes = InstallerExitCodes.FontInstallError;
                            Prog.InstallStatus = InstallerExitCodes.FontInstallError;
                            LockProgress();
                        }
                    });
                }
            }
            
            //barrier goes here to make sure cleanup is the last thing to do
            Task.WaitAll(concurrentTasksAfterMainExtractoin.Where(task => task != null).ToArray());
            Logging.Info("All async operations after extraction complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));

            //step 12: trim download cache folder
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent = (int)InstallerExitCodes.TrimDownloadCacheError;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.TrimDownloadCacheError;
            Prog.InstallStatus = InstallerExitCodes.TrimDownloadCacheError;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Trim download cache, current install time = {0} msec", (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            if(ModpackSettings.DeleteCacheFiles)
            {
                if (!TrimDownloadCache())
                {
                    return InstallFinishedArgs;
                }
                Logging.Info("Trim download cache complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped (ModpackSettings.DeleteCacheFiles = false)");


            //step 13: cleanup
            OldTime = InstallStopWatch.Elapsed;
            Prog.TotalCurrent++;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.CleanupError;
            Prog.InstallStatus = InstallerExitCodes.CleanupError;
            Progress.Report(Prog);

            Logging.WriteToLog(string.Format("Cleanup, current install time = {0} msec", (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            if(!ModpackSettings.ExportMode)
            {
                if (!Cleanup())
                    return InstallFinishedArgs;
                Logging.Info("Cleanup complete, took {0} msec", (int)(InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped (ModpackSettings.ExportMode = true)");


            if (!ModpackSettings.DisableTriggers)
            {
                //check if any triggers are still running
                foreach (Trigger trigger in Triggers)
                {
                    if (trigger.TriggerTask != null)
                    {
                        Logging.Debug("Start waiting for task {0} to complete at time {1}", trigger.Name, (int)InstallStopWatch.Elapsed.TotalMilliseconds);
                        trigger.TriggerTask.Wait();
                        Logging.Debug("Task {0} finished or was already done at time {1}", trigger.Name, (int)InstallStopWatch.Elapsed.TotalMilliseconds);
                    }
                    else
                        Logging.Debug("Trigger task {0} is null, skipping", trigger.Name);
                }
            }

            //report to log install is finished
            OldTime = InstallStopWatch.Elapsed;
            InstallFinishedArgs.ExitCodes = InstallerExitCodes.Success;
            Prog.InstallStatus = InstallerExitCodes.Success;
            Logging.Info("Install finished, total install time = {0} msec", (int)InstallStopWatch.Elapsed.TotalMilliseconds);
            InstallStopWatch.Stop();
            return InstallFinishedArgs;
        }
        #endregion

        #region Main Uninstall methods
        private bool UninstallModsQuick(bool logIt)
        {
            Prog.UninstallStatus = UninstallerExitCodes.GettingFilelistError;
            Progress.Report(Prog);

            //get a list of all files and folders in mods and res_mods
            List<string> ListOfAllItems = new List<string>();
            if (Directory.Exists(Path.Combine(Settings.WoTDirectory, "res_mods")))
                ListOfAllItems.AddRange(Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, "res_mods"), SearchOption.AllDirectories,true).ToList());
            if (Directory.Exists(Path.Combine(Settings.WoTDirectory, "mods")))
                ListOfAllItems.AddRange(Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, "mods"), SearchOption.AllDirectories,true).ToList());

            //combine with a list of any installer engine created folders
            foreach (string folder in Settings.FoldersToCleanup)
            {
                string folderPath = Path.Combine(Settings.WoTDirectory, folder);
                if (Directory.Exists(folderPath))
                    ListOfAllItems.AddRange(Utils.DirectorySearch(folderPath, SearchOption.AllDirectories,true));
            }

            //start init progress reporting. for uninstall, only use child current, total and filename
            Prog.ChildTotal = ListOfAllItems.Count;
            Prog.ChildCurrent = 0;
            Progress.Report(Prog);

            if(logIt)
            {
                string backupUninstallLogfile = Path.Combine(Settings.WoTDirectory, "logs", Logging.UninstallLogFilenameBackup);
                string uninstallLogfile = Path.Combine(Settings.WoTDirectory, "logs", Logging.UninstallLogFilename);
                if (File.Exists(backupUninstallLogfile))
                    Utils.FileDelete(backupUninstallLogfile);
                if (File.Exists(uninstallLogfile))
                    File.Move(uninstallLogfile, backupUninstallLogfile);

                //create the uninstall logfile and write header info
                if (!Logging.Init(Logfiles.Uninstaller, uninstallLogfile))
                {
                    Logging.Error("Failed to init the uninstall logfile");
                    return false;
                }
                else
                {
                    Logging.WriteToLog(string.Format(@"/*  Date: {0:yyyy-MM-dd HH:mm:ss}  */", DateTime.Now), Logfiles.Uninstaller, LogLevel.Info);
                    Logging.WriteToLog(string.Format("/* Uninstall Method: {0} */", ModpackSettings.UninstallMode.ToString()), Logfiles.Uninstaller, LogLevel.Info);
                    Logging.WriteToLog(@"/*  files and folders deleted  */", Logfiles.Uninstaller, LogLevel.Info);
                }
            }

            //backup old uninstall logfile
            bool success = true;
            Prog.UninstallStatus = UninstallerExitCodes.UninstallError;
            foreach(string file in ListOfAllItems)
            {
                Prog.ChildCurrent++;
                Prog.Filename = file;
                Progress.Report(Prog);
                if (!Utils.FileDelete(file))
                    success = false;
                else
                {
                    if(logIt)
                        Logging.WriteToLog(file, Logfiles.Uninstaller, LogLevel.Info);
                }
            }

            //final wipe of the folders
            Prog.UninstallStatus = UninstallerExitCodes.PerformFinalClearup;
            Progress.Report(Prog);
            if (Directory.Exists(Path.Combine(Settings.WoTDirectory, "res_mods")))
                if (!Utils.DirectoryDelete(Path.Combine(Settings.WoTDirectory, "res_mods"), true))
                    success = false;
            if (Directory.Exists(Path.Combine(Settings.WoTDirectory, "mods")))
                if (!Utils.DirectoryDelete(Path.Combine(Settings.WoTDirectory, "mods"), true))
                    success = false;

            //re-create the folders at the end
            Directory.CreateDirectory(Path.Combine(Settings.WoTDirectory, "res_mods", Settings.WoTClientVersion));
            Directory.CreateDirectory(Path.Combine(Settings.WoTDirectory, "mods", Settings.WoTClientVersion));

            //if we are logging, we need to dispose of the uninstall log
            if (logIt)
            {
                Logging.DisposeLogging(Logfiles.Uninstaller);
            }

            if (success)
                Prog.UninstallStatus = UninstallerExitCodes.Success;
            else
                Prog.UninstallStatus = UninstallerExitCodes.UninstallError;

            InstallFinishedArgs.ParsedCategoryList = ParsedCategoryList;
            InstallFinishedArgs.Dependencies = Dependencies;
            InstallFinishedArgs.GlobalDependencies = GlobalDependencies;

            Prog.ChildCurrent = Prog.ChildTotal;
            Progress.Report(Prog);
            return success;
        }

        private bool UninstallModsDefault(bool logIt)
        {
            //setup progress
            Prog.UninstallStatus = UninstallerExitCodes.GettingFilelistError;
            Progress.Report(Prog);

            //get a list of all files and folders in the install log (assuming checking for logfile already done and exists)
            List<string> ListOfAllItems = File.ReadAllLines(Path.Combine(Settings.WoTDirectory, "logs", Logging.InstallLogFilename)).ToList();

            //combine with a list of all files and folders in mods and res_mods
            if(Directory.Exists(Path.Combine(Settings.WoTDirectory, "res_mods")))
                ListOfAllItems.AddRange(Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, "res_mods"), SearchOption.AllDirectories,true).ToList());
            if (Directory.Exists(Path.Combine(Settings.WoTDirectory, "mods")))
                ListOfAllItems.AddRange(Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, "mods"), SearchOption.AllDirectories,true).ToList());

            //combine with a list of any installer engine created folders
            foreach(string folder in Settings.FoldersToCleanup)
            {
                string folderPath = Path.Combine(Settings.WoTDirectory, folder);
                if (Directory.Exists(folderPath))
                    ListOfAllItems.AddRange(Utils.DirectorySearch(folderPath, SearchOption.AllDirectories,true));
            }

            //merge then sort and then reverse
            ListOfAllItems = ListOfAllItems.Distinct().ToList();
            ListOfAllItems.Sort();
            ListOfAllItems.Reverse();

            //split off into files and folders and shortcuts
            List<string> ListOfAllDirectories = ListOfAllItems.Where(item => Directory.Exists(item)).ToList();
            List<string> ListOfAllFiles = ListOfAllItems.Where(item => File.Exists(item)).ToList();
            List<string> ListOfAllShortcuts = ListOfAllFiles.Where(file => Path.GetExtension(file).Equals(".lnk")).ToList();
            ListOfAllFiles = ListOfAllFiles.Except(ListOfAllShortcuts).ToList();

            //start init progress reporting. for uninstall, only use child current, total and filename
            Prog.ChildTotal = ListOfAllItems.Count;
            Prog.ChildCurrent = 0;
            Progress.Report(Prog);

            if(logIt)
            {
                //backup old uninstall logfile
                string backupUninstallLogfile = Path.Combine(Settings.WoTDirectory, "logs", Logging.UninstallLogFilenameBackup);
                string uninstallLogfile = Path.Combine(Settings.WoTDirectory, "logs", Logging.UninstallLogFilename);
                if (File.Exists(backupUninstallLogfile))
                    Utils.FileDelete(backupUninstallLogfile);
                if (File.Exists(uninstallLogfile))
                    File.Move(uninstallLogfile, backupUninstallLogfile);

                //create the uninstall logfile and write header info
                if (!Logging.Init(Logfiles.Uninstaller, uninstallLogfile))
                {
                    Logging.Error("Failed to init the uninstall logfile");
                }
                else
                {
                    Logging.WriteToLog(string.Format(@"/*  Date: {0:yyyy-MM-dd HH:mm:ss}  */", DateTime.Now), Logfiles.Uninstaller, LogLevel.Info);
                    Logging.WriteToLog(string.Format("/* Uninstall Method: {0} */", ModpackSettings.UninstallMode.ToString()), Logfiles.Uninstaller, LogLevel.Info);
                    Logging.WriteToLog(@"/*  files and folders deleted  */", Logfiles.Uninstaller, LogLevel.Info);
                }
            }

            //delete all files (not shortcuts)
            bool success = true;
            Prog.UninstallStatus = UninstallerExitCodes.UninstallError;
            foreach (string file in ListOfAllFiles)
            {
                Prog.ChildCurrent++;
                Prog.Filename = file;
                Progress.Report(Prog);
                if (File.Exists(file))
                {
                    if (!Utils.FileDelete(file))
                        success = false;
                    else
                    {
                        if(logIt)
                            Logging.WriteToLog(file, Logfiles.Uninstaller, LogLevel.Info);
                    }
                }
            }

            //deal with shortcuts
            //if settings.createShortcuts, then don't delete them (at least here, for now)
            //otherwise delete them
            if (!ModpackSettings.CreateShortcuts)
            {
                Logging.Debug("Deleting shortcuts");
                foreach (string file in ListOfAllShortcuts)
                {
                    if (File.Exists(file))
                    {
                        if (!Utils.FileDelete(file))
                            success = false;
                        else
                        {
                            if(logIt)
                                Logging.WriteToLog(file, Logfiles.Uninstaller, LogLevel.Info);
                        }
                    }
                }
            }

            //final wipe of the folders
            Prog.UninstallStatus = UninstallerExitCodes.PerformFinalClearup;
            Progress.Report(Prog);
            if (Directory.Exists(Path.Combine(Settings.WoTDirectory, "res_mods")))
                if (!Utils.DirectoryDelete(Path.Combine(Settings.WoTDirectory, "res_mods"), true))
                    success = false;
            if (Directory.Exists(Path.Combine(Settings.WoTDirectory, "mods")))
                if (!Utils.DirectoryDelete(Path.Combine(Settings.WoTDirectory, "mods"), true))
                    success = false;

            //delete all empty folders
            Prog.UninstallStatus = UninstallerExitCodes.ProcessingEmptyFolders;
            Progress.Report(Prog);
            foreach (string folder in ListOfAllDirectories)
            {
                if (Directory.Exists(folder))
                {
                    Utils.ProcessEmptyDirectories(folder, false);
                    if(logIt)
                        Logging.WriteToLog(folder, Logfiles.Uninstaller, LogLevel.Info);
                }
            }

            //re-create the folders at the end
            Directory.CreateDirectory(Path.Combine(Settings.WoTDirectory, "res_mods", Settings.WoTClientVersion));
            Directory.CreateDirectory(Path.Combine(Settings.WoTDirectory, "mods", Settings.WoTClientVersion));

            //if we are logging, we need to dispose of the uninstall log
            if(logIt)
            {
                Logging.DisposeLogging(Logfiles.Uninstaller);
            }

            if (success)
                Prog.UninstallStatus = UninstallerExitCodes.Success;
            else
                Prog.UninstallStatus = UninstallerExitCodes.UninstallError;

            Prog.ChildCurrent = Prog.ChildTotal;
            Progress.Report(Prog);
            return success;
        }
        #endregion

        #region Installer methods
        private bool BackupMods()
        {
            //setup prog reporting
            Prog.ParrentTotal = Prog.ChildTotal = 1;
            Prog.ParrentCurrent = Prog.ChildCurrent = 0;
            Prog.Filename = string.Empty;
            Progress.Report(Prog);

            //check first if the modpack backup directory exists first
            if (!Directory.Exists(Settings.RelhaxModBackupFolder))
                Directory.CreateDirectory(Settings.RelhaxModBackupFolder);

            //create the directory for this version to backup to
            string zipFileName = string.Format("{0:yyyy-MM-dd-HH-mm-ss}_{1}.zip", DateTime.Now,Settings.WoTClientVersion);
            string zipFileFullPath = Path.Combine(Settings.RelhaxModBackupFolder, zipFileName);
            Logging.Debug("started backupMods(), making zipfile {0}", zipFileFullPath);

            //make a zip file of the mods and res_mods and appdata
            using (ZipFile backupZip = new ZipFile(zipFileFullPath))
            {
                //set the compression level to be normal
                backupZip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;

                //report progress of gettings mods to save into the zip file
                Prog.ParrentCurrent = 0;
                Prog.ParrentTotal = 1;
                Prog.ChildCurrent = 0;
                Prog.ChildTotal = 1;
                Prog.ParrentCurrentProgress = Translations.GetTranslatedString("scanningModsFolders");
                Progress.Report(Prog);

                //get the list of mods to add to the zip
                List<string> filesToAdd = Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, "mods"), SearchOption.AllDirectories, false, "*", 5, 3, false).ToList();
                filesToAdd.AddRange(Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, "res_mods"), SearchOption.AllDirectories, false, "*", 5, 3, false).ToList());

                //add them to the zip. also get the string to be the path in the zip file, meaning that the root path in the zip file starts at "World_of_Tanks"
                foreach(string file in filesToAdd)
                {
                    backupZip.AddFile(file, Path.GetDirectoryName(file.Substring(Settings.WoTDirectory.Length + 1)));
                }

                //clear the list and repeat the process for the appDataFolder
                filesToAdd.Clear();
                filesToAdd.AddRange(Utils.DirectorySearch(Settings.AppDataFolder, SearchOption.AllDirectories, false, "*", 5, 3, false).ToList());
                foreach (string file in filesToAdd)
                {
                    backupZip.AddFile(file, Path.GetDirectoryName(Path.Combine("appData", file.Substring(Settings.AppDataFolder.Length + 1))));
                }

                //save the file. all the time to wait is in this method, so add the event handler here
                backupZip.SaveProgress += BackupZip_SaveProgress;
                Prog.ParrentCurrentProgress = string.Empty;
                backupZip.Save();
            }
            Logging.Debug("finished backupMods()");
            return true;
        }

        private void BackupZip_SaveProgress(object sender, SaveProgressEventArgs e)
        {
            //we only want entry bytes read for report (let's *try* to be efficient here)
            switch (e.EventType)
            {
                case ZipProgressEventType.Saving_EntryBytesRead:
                    Prog.ChildCurrent = (int)e.BytesTransferred;
                    Prog.ChildTotal = (int)e.TotalBytesToTransfer;
                    Progress.Report(Prog);
                    break;
                case ZipProgressEventType.Saving_AfterWriteEntry:
                case ZipProgressEventType.Saving_BeforeWriteEntry:
                    Prog.ParrentCurrent = e.EntriesSaved;
                    Prog.ParrentTotal = e.EntriesTotal;
                    Prog.EntryFilename = e.CurrentEntry.FileName;
                    Progress.Report(Prog);
                    break;
            } 
        }

        private bool BackupData(List<SelectablePackage> packagesWithData)
        {
            //setup progress reporting
            //total is backup data operation (not touched)
            //parent is number of packages with data
            //child is the file being backed up in this package
            //using file parameter
            Prog.ParrentTotal = packagesWithData.Count;

            //setup prog reporting
            Prog.ChildTotal = 1;
            Prog.ParrentCurrent = Prog.ChildCurrent = 0;
            Prog.Filename = string.Empty;
            Progress.Report(Prog);

            foreach (SelectablePackage package in packagesWithData)
            {
                Logging.Info("Backup data of package {0} starting", package.PackageName);
                Prog.ParrentCurrent++;
                Prog.ParrentCurrentProgress = package.NameFormatted;
                Progress.Report(Prog);
                foreach(UserFile files in package.UserFiles)
                {
                    //use the search parameter to get the actual files to move
                    //remove the mistake I made over a year ago of the double slashes
                    string searchPattern = files.Pattern.Replace(@"\\", @"\");

                    //legacy compatibility: the path will either start with a macro or with the raw path
                    //examples:
                    //new: {appData}\Roaming\Wargaming.net\WorldOfTanks\xvm\users\*
                    //old: \\mods\\configs\\promod\\artylog\\modCache.json
                    //new: {app}\autoequip.json
                    //old: \res_mods\mods\shared_resources\xvm\res\clanicons\CT\clan\*.png
                    //old: mods\configs\battle_assistant\mod_battle_assistant.txt
                    //they need to be treated differently
                    string root_directory = "";
                    string actual_search = "";

                    //build the entire directory path, still including the search
                    if (searchPattern[0].Equals(@"{"))
                    {
                        //it does not have the macro, so add it. (assume {app} macro)
                        Logging.Debug("pattern starts with \"{\", continue");
                    }
                    else if (searchPattern[0].Equals(@"\"))
                    {
                        Logging.Debug("pattern starts with \"\\\", adding macro and continue");
                        searchPattern = @"{app}" + searchPattern;
                    }
                    else
                    {
                        Logging.Debug("pattern starts with folder name, adding macro and folder slash and continue");
                        searchPattern = @"{app}\" + searchPattern;
                    }
                    Logging.Debug("path with macro: {0}", searchPattern);

                    //replace the macro to make the complete path
                    searchPattern = Utils.MacroReplace(searchPattern, ReplacementTypes.FilePath);
                    Logging.Debug("complete path: {0}", searchPattern);

                    //get the list of files to replace
                    string[] filesToSave = Utils.DirectorySearch(Path.GetDirectoryName(searchPattern), SearchOption.AllDirectories, false, Path.GetFileName(searchPattern), 5, 3, false);

                    //check if we have files to move
                    if(filesToSave.Count() == 0)
                    {
                        Logging.Info("no files found to backup");
                        continue;
                    }

                    //update progress
                    Prog.ChildTotal = filesToSave.Count();

                    //make the temp directory to place the files based on this package
                    string tempFolderPath = Path.Combine(Settings.RelhaxTempFolder, package.PackageName);
                    Directory.CreateDirectory(tempFolderPath);

                    //move each file
                    foreach(string file in filesToSave)
                    {
                        Prog.ChildCurrent++;
                        Prog.Filename = file;
                        Progress.Report(Prog);
                        File.Move(file, Path.Combine(tempFolderPath, Path.GetFileName(file)));
                    }
                }
                Logging.Info("backup data of {0} finished", package.PackageName);
            }
            return true;
        }

        private bool ClearCache()
        {
            //setup prog reporting
            Prog.ParrentTotal = Prog.ChildTotal = 1;
            Prog.ParrentCurrent = Prog.ChildCurrent = 0;
            Prog.Filename = string.Empty;
            Progress.Report(Prog);

            //make sure that the app data folder exists
            //if it does not, then it does not need to run this
            if (!Directory.Exists(Settings.AppDataFolder))
            {
                Logging.WriteToLog("Appdata folder does not exist, creating");
                Directory.CreateDirectory(Settings.AppDataFolder);
                return true;
            }
            Logging.WriteToLog("Appdata folder exists, backing up user settings and clearing cache");

            //make the temp folder if it does not already exist
            string AppPathTempFolder = Path.Combine(Settings.RelhaxTempFolder, "AppDataBackup");
            //delete if possibly from previous install
            if (Directory.Exists(AppPathTempFolder))
                Utils.DirectoryDelete(AppPathTempFolder, true);
            //and make the folder at the end
            Directory.CreateDirectory(AppPathTempFolder);

            //backup files and folders that should be kept that aren't cache
            string[] fileNames = { "preferences.xml", "preferences_ct.xml", "modsettings.dat" };
            string[] folderNames = { "xvm", "pmod" };

            //check if the directories are files or folders
            //if files they can move directly
            //if folders they have to be re-created on the destination and files moved manually
            Logging.WriteToLog("Starting clearing cache step 1 of 3: backing up old files", Logfiles.Application, LogLevel.Debug);
            foreach(string file in fileNames)
            {
                Logging.WriteToLog("Processing cache file/folder to move: " + file, Logfiles.Application, LogLevel.Debug);
                if(File.Exists(Path.Combine(Settings.AppDataFolder, file)))
                {
                    try { File.Move(Path.Combine(Settings.AppDataFolder, file), Path.Combine(AppPathTempFolder, file)); }
                    catch (Exception ex)
                    {
                        Logging.Exception(ex.ToString());
                        return false;
                    }
                } 
                else
                {
                    Logging.Info("File does not exist in step clearCache: {0}", file);
                }
            }

            foreach(string folder in folderNames)
            {
                if (Directory.Exists(Path.Combine(Settings.AppDataFolder, folder)))
                {
                    Utils.DirectoryMove(Path.Combine(Settings.AppDataFolder, folder), Path.Combine(AppPathTempFolder, folder), true);
                }
                else
                {
                    Logging.Info("Folder does not exist in step clearCache: {0}", folder);
                }
            }

            //now delete the temp folder
            Logging.WriteToLog("Starting clearing cache step 2 of 3: actually clearing cache", Logfiles.Application, LogLevel.Debug);
            Utils.DirectoryDelete(Settings.AppDataFolder, true);

            //then put the above files back
            Logging.WriteToLog("Starting clearing cache step 3 of 3: restoring old files", Logfiles.Application, LogLevel.Debug);
            Directory.CreateDirectory(Settings.AppDataFolder);
            foreach (string file in fileNames)
            {
                Logging.WriteToLog("Processing cache file/folder to move: " + file, Logfiles.Application, LogLevel.Debug);
                if (File.Exists(Path.Combine(AppPathTempFolder, file)))
                {
                    try { File.Move(Path.Combine(AppPathTempFolder, file), Path.Combine(Settings.AppDataFolder, file)); }
                    catch (Exception ex)
                    {
                        Logging.Exception(ex.ToString());
                        return false;
                    }
                }
                else
                {
                    Logging.Info("File does not exist in step clearCache: {0}", file);
                }
            }

            foreach (string folder in folderNames)
            {
                if (Directory.Exists(Path.Combine(AppPathTempFolder, folder)))
                {
                    Utils.DirectoryMove(Path.Combine(AppPathTempFolder, folder), Path.Combine(Settings.AppDataFolder, folder), true);
                }
                else
                {
                    Logging.Info("Folder does not exist in step clearCache: {0}", folder);
                }
            }
            return true;
        }

        private bool ClearLogs()
        {
            //setup prog reporting
            Prog.ParrentTotal = Prog.ChildTotal = 1;
            Prog.ParrentCurrent = Prog.ChildCurrent = 0;
            Prog.Filename = string.Empty;
            Progress.Report(Prog);
            string[] logsToDelete = new string[]
            {
                Path.Combine(Settings.WoTDirectory, "python.log"),
                Path.Combine(Settings.WoTDirectory, "xvm.log"),
                Path.Combine(Settings.WoTDirectory, "pmod.log"),
                Path.Combine(Settings.WoTDirectory, "WoTLauncher.log"),
                Path.Combine(Settings.WoTDirectory, "cef.log")
            };
            Prog.ParrentTotal = logsToDelete.Count();
            Progress.Report(Prog);
            foreach(string s in logsToDelete)
            {
                Logging.WriteToLog("Processing log file (if exists) " + s, Logfiles.Application, LogLevel.Info);
                Prog.ParrentCurrent++;
                Prog.Filename = s;
                Progress.Report(Prog);
                if (File.Exists(s))
                    if (!Utils.FileDelete(s))
                        return false;
            }
            return true;
        }

        private bool ClearModsFolders()
        {
            switch (ModpackSettings.UninstallMode)
            {
                case UninstallModes.Default:
                    Logging.WriteToLog("Running uninstall modes method Default");
                    return UninstallModsDefault(false);
                case UninstallModes.Quick:
                    Logging.WriteToLog("Running uninstall modes method Quick (Advanced)");
                    return UninstallModsQuick(false);
                default:
                    Logging.WriteToLog("Unknown uninstall mode: " + ModpackSettings.UninstallMode.ToString());
                    return false;
            }
        }

        private bool ExtractFilesAsyncSetup()
        {
            //progress reporting
            Prog.ChildCurrent = Prog.ParrentCurrent = 0;
            Prog.ChildTotal = Prog.ParrentTotal = 1;
            Prog.Filename = string.Empty;
            Progress.Report(Prog);

            //this is the only really new one. for each install group, spawn a bunch of threads to start the install process
            //get the number of threads we will use for each of the install steps
            int numThreads = ModpackSettings.MulticoreExtraction ? Settings.NumLogicalProcesors : 1;

            Logging.WriteToLog(string.Format("Number of threads to use for install is {0}, (MulticoreExtraction={1}, LogicalProcesosrs={2}",
                numThreads, ModpackSettings.MulticoreExtraction, Settings.NumLogicalProcesors));

            //setup progress reporting for parent
            Prog.ParrentTotal = OrderedPackagesToInstall.Count();

            for (int i = 0; i < OrderedPackagesToInstall.Count(); i++)
            {
                Logging.WriteToLog("Install Group " + i + " starts now");
                Prog.ParrentCurrent++;
                Progress.Report(Prog);

                //get the list of packages to install
                //this list represents all the packages in this install group that can be installed at the same time
                //i.e. there are NO conflicting zip file paths in ALL of the files (all the entries in all zip files are mutually exclusive)
                List<DatabasePackage> packages = new List<DatabasePackage>(OrderedPackagesToInstall[i]);

                //set it for the progress report
                Prog.ChildTotal = packages.Count;

                //first sort the packages by the size parameter
                //https://stackoverflow.com/questions/3309188/how-to-sort-a-listt-by-a-property-in-the-object
                //TODO: the size of all objects MUST BE KNOWN??
                packages = packages.OrderByDescending(pack => pack.Size).ToList();
                //for not just go with the packages as they are, they should already be in alphabetical order

                //make a list of packages again, but size is based on number of logical processors and/or multi-core install mods
                //if a user has 8 cores, then make a lists of packages to install
                List<DatabasePackage>[] packageThreads = new List<DatabasePackage>[numThreads];

                //new up the lists before we can assign to them
                for (int j = 0; j < packageThreads.Count(); j++)
                {
                    packageThreads[j] = new List<DatabasePackage>();
                }

                //assign each package one at a time into a package thread
                for (int j = 0; j < packages.Count; j++)
                {
                    int threadSelector = j % numThreads;
                    packageThreads[threadSelector].Add(packages[j]);
                    Logging.WriteToLog(string.Format("j index = {0} package {1} has been assigned to packageThread {2}", j, packages[j].PackageName,
                        threadSelector), Logfiles.Application, LogLevel.Debug);
                }

                //now the fun starts. these all can run at once. yeah.
                //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming
                Task[] tasks = new Task[numThreads];
                if (tasks.Count() != packageThreads.Count())
                    throw new BadMemeException("ohhhhhhhhh, NOW you f*cked UP!");

                bool valueLocked = false;

                //start the threads
                for (int k = 0; k < tasks.Count(); k++)
                {
                    Logging.WriteToLog(string.Format("thread {0} starting task, packages to extract={1}", k, packageThreads[k].Count));
                    //use the task factory to create tasks(threads) for each logical cores
                    tasks[k] = Task.Run(() =>
                    {
                        int temp = k;
                        valueLocked = true;
                        ExtractFiles(packageThreads[temp], temp);
                    });
                    Logging.Debug("thread {0} waiting to be started", k);
                    while (!valueLocked) ;
                    valueLocked = false;
                    Logging.Debug("thread {0} running, starting next task", k);
                }

                //and log it all
                Logging.WriteToLog(string.Format("all threads started on group {0}, master thread now waiting on Task.WaitAll(tasks)",i),
                    Logfiles.Application,LogLevel.Debug);
                Task.WaitAll(tasks);
                Logging.WriteToLog("Install Group " + i + " finishes now");
            }
            return true;
        }

        private bool RestoreData(List<SelectablePackage> packagesWithData, StringBuilder restoreDataBuilder)
        {
            //progress reporting
            Prog.ChildTotal = packagesWithData.Count;
            Prog.ChildCurrent = Prog.ParrentCurrent = 0;
            Prog.ParrentTotal = 1;
            Prog.Filename = string.Empty;
            Progress.Report(Prog);

            foreach (SelectablePackage package in packagesWithData)
            {
                //actually report
                Prog.ChildCurrent++;
                Progress.Report(Prog);
                Logging.WriteToLog(string.Format("Restore data of package {0} starting", package.PackageName));

                //check if the package name exists first
                string tempBackupFolder = Path.Combine(Settings.RelhaxTempFolder, package.PackageName);
                if(!Directory.Exists(tempBackupFolder))
                {
                    Logging.WriteToLog(string.Format("folder {0} does not exist, skipping", package.PackageName), Logfiles.Application, LogLevel.Error);
                }
                foreach (UserFile files in package.UserFiles)
                {
                    foreach(string savedFile in files.Files_saved)
                    {
                        string filePath = Path.Combine(Settings.RelhaxTempFolder, package.PackageName, Path.GetFileName(savedFile));
                        if(File.Exists(filePath))
                        {
                            Logging.WriteToLog(string.Format("Restoring file {0} of {1}", Path.GetFileName(savedFile), package.PackageName));
                            if (!Directory.Exists(Path.GetDirectoryName(savedFile)))
                                Directory.CreateDirectory(Path.GetDirectoryName(savedFile));
                            if (File.Exists(savedFile))
                                File.Delete(savedFile);
                            File.Move(filePath, savedFile);
                            restoreDataBuilder.AppendLine(savedFile);
                        }
                    }
                }
            }
            return true;
        }

        private void InstallFonts(string[] fontsToInstall, Task patchTask, Task createShortcutsTask, Task createAtlasesTask)
        {
            Logging.Debug("checking system installed fonts to remove duplicates");
            if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask) && TaskNullOrDone(createAtlasesTask))
            {
                Prog.ChildTotal = 1;
                Prog.ParrentTotal = 6;
                Prog.ParrentCurrent = 0;
                Prog.ChildCurrent = 0;
                Prog.Filename = string.Empty;
                LockProgress();
            }

            string[] fontscurrentlyInstalled = Utils.DirectorySearch(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), SearchOption.TopDirectoryOnly,false);
            string[] fontsNamesCurrentlyInstalled = fontscurrentlyInstalled.Select(s => Path.GetFileName(s).ToLower()).ToArray();
            if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask) && TaskNullOrDone(createAtlasesTask))
            {
                Prog.ChildTotal = 1;
                Prog.ParrentTotal = 6;
                Prog.ParrentCurrent = 1;
                Prog.ChildCurrent = 0;
                Prog.Filename = string.Empty;
                LockProgress();
            }

            //remove any fonts whos filename match what is already installed
            for (int i = 0; i < fontsToInstall.Count(); i++)
            {
                string fontToInstallNameLower = Path.GetFileName(fontsToInstall[i]).ToLower();
                if(fontsNamesCurrentlyInstalled.Contains(fontToInstallNameLower))
                {
                    //empty the entry
                    fontsToInstall[i] = string.Empty;
                }
            }

            //get the new array of fonts to install that don't already exist
            string[] realFontsToInstall = fontsToInstall.Where(font => !string.IsNullOrWhiteSpace(font)).ToArray();

            Logging.Debug("fontsToInstallReal count: {0}", realFontsToInstall.Count());
            if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask) && TaskNullOrDone(createAtlasesTask))
            {
                Prog.ChildTotal = 1;
                Prog.ParrentTotal = 6;
                Prog.ParrentCurrent = 2;
                Prog.ChildCurrent = 0;
                Prog.Filename = string.Empty;
                LockProgress();
            }

            if (realFontsToInstall.Count() > 0)
            {
                //extract he exe to install fonts
                Logging.Debug("extracting fontReg for font install");
                string fontRegPath = Path.Combine(Settings.WoTDirectory, Settings.FontsToInstallFoldername, "FontReg.exe");
                if(!File.Exists(fontRegPath))
                {
                    //get fontreg from the zip file
                    using (ZipFile zip = new ZipFile(Settings.ManagerInfoDatFile))
                    {
                        zip.ExtractSelectedEntries("FontReg.exe", null, Path.GetDirectoryName(fontRegPath));
                    }
                    if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask) && TaskNullOrDone(createAtlasesTask))
                    {
                        Prog.ChildTotal = 1;
                        Prog.ParrentTotal = 6;
                        Prog.ParrentCurrent = 3;
                        Prog.ChildCurrent = 0;
                        Prog.Filename = string.Empty;
                        LockProgress();
                    }
                }
                Logging.Info("Attemping to install fonts: {0}", string.Join(",", realFontsToInstall));
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = fontRegPath,
                    UseShellExecute = true,
                    Verb = "runas", // Provides Run as Administrator
                    Arguments = "/copy",
                    WorkingDirectory = Path.Combine(Settings.WoTDirectory, Settings.FontsToInstallFoldername)
                };
                try
                {
                    Process installFontss = new Process
                    {
                        StartInfo = info
                    };
                    installFontss.Start();
                    installFontss.WaitForExit();
                    Logging.Info("FontReg.exe ExitCode: " + installFontss.ExitCode);
                    if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask) && TaskNullOrDone(createAtlasesTask))
                    {
                        Prog.ChildTotal = 1;
                        Prog.ParrentTotal = 6;
                        Prog.ParrentCurrent = 6;
                        Prog.ChildCurrent = 0;
                        Prog.Filename = string.Empty;
                        LockProgress();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("could not start font installer:{0}{1}", Environment.NewLine, ex.ToString());
                    MessageBox.Show(Translations.GetTranslatedString("fontsPromptError_1") + Settings.WoTDirectory + Translations.GetTranslatedString("fontsPromptError_2"));
                    Logging.Info("Installation done, but fonts install failed");
                    return;
                }
            }
        }

        private bool TrimDownloadCache()
        {
            //progress reporting
            Prog.ChildCurrent = Prog.ParrentCurrent = 0;
            Prog.ChildTotal = 1;
            Prog.ParrentTotal = 2;
            Prog.Filename = string.Empty;
            Progress.Report(Prog);

            //get a list of all packages in the database with zip files
            List<DatabasePackage> allFlatList = Utils.GetFlatList(GlobalDependencies, Dependencies, null, ParsedCategoryList).Where(package => !string.IsNullOrWhiteSpace(package.ZipFile)).ToList();

            //convert it to a list of strings
            List<string> zipFilesInDatabase = allFlatList.Select(package => package.ZipFile).ToList();

            //get a list of all files in the download cache folder
            List<string> zipFilesInCache = Utils.DirectorySearch(Settings.RelhaxDownloadsFolder, SearchOption.TopDirectoryOnly, false, "*.zip").ToList();
            if(zipFilesInCache == null)
            {
                Logging.Error("failed to get list of zip files in download cache, skipping this step");
                return false;
            }
            Prog.ParrentCurrent = 1;
            Progress.Report(Prog);

            //update the list to only have the filename, not the complete path
            zipFilesInCache = zipFilesInCache.Select(str => Path.GetFileName(str)).ToList();

            //get a list of zip files in the cache that aren't in the database, these are old and can be deleted
            List<string> oldZipFilesNotInDatabase = zipFilesInCache.Except(zipFilesInDatabase).ToList();

            //report progress
            Prog.ChildTotal = oldZipFilesNotInDatabase.Count;

            //if there are any in the above list, it means they are old and can be deleted
            if (oldZipFilesNotInDatabase.Count > 0)
            {
                foreach (string zipfile in oldZipFilesNotInDatabase)
                {
                    Prog.ChildCurrent++;
                    Prog.Filename = zipfile;
                    Progress.Report(Prog);

                    Utils.FileDelete(Path.Combine(Settings.RelhaxDownloadsFolder, zipfile));
                }
            }
            return true;
        }

        private bool Cleanup()
        {
            //progress reporting
            Prog.ChildCurrent = Prog.ParrentCurrent = 0;
            Prog.ChildTotal = 1;
            Prog.ParrentTotal = Settings.FoldersToCleanup.Count();
            Prog.Filename = string.Empty;
            Progress.Report(Prog);

            foreach (string folder in Settings.FoldersToCleanup)
            {
                Logging.Debug("cleaning folder {0}, if exists", folder);
                Prog.ParrentCurrent++;
                Prog.Filename = folder;
                Progress.Report(Prog);

                string folderPath = Path.Combine(Settings.WoTDirectory, folder);
                if (Directory.Exists(folderPath))
                    Utils.DirectoryDelete(folderPath, true);
            }
            return true;
        }
        #endregion

        #region Util Methods
        private void ExtractFiles(List<DatabasePackage> packagesToExtract, int threadNum)
        {
            bool notAllPackagesExtracted = true;
            //in case the user selected to "download and install at the same time", there may be cases where
            //some items in this list (earlier, for sake of areugment) are not downloaded yet, but others below are.
            //if this is the case, then we need to skip over those items for now while they download in the background
            while (notAllPackagesExtracted)
            {
                int numExtracted = 0;
                foreach (DatabasePackage package in packagesToExtract)
                {
                    if (ModpackSettings.InstallWhileDownloading && package.DownloadFlag)
                    {
                        continue;
                    }
                    else
                    {
                        Logging.WriteToLog("Thread ID=" + threadNum + ", starting extraction of " + package.ZipFile);
                        numExtracted++;
                        if (string.IsNullOrWhiteSpace(package.ZipFile))
                            continue;
                        StringBuilder zipLogger = new StringBuilder();
                        zipLogger.AppendLine(string.Format("/*   {0}   */",package.ZipFile));
                        Unzip(package, threadNum, zipLogger);
                        Logging.Installer(zipLogger.ToString());
                        //after zip file extraction, process triggers (if enabled)
                        if(!ModpackSettings.DisableTriggers)
                        {
                            if (package.Triggers.Count > 0)
                                ProcessTriggers(package.Triggers);
                        }
                    }
                }
                if (numExtracted == packagesToExtract.Count)
                    notAllPackagesExtracted = false;
                else
                    System.Threading.Thread.Sleep(200);
            }
            //report progress on the thread. in case multiple threads, make sure you lock it
            lock(Prog)
            {
                Prog.ChildCurrent++;
                Progress.Report(Prog);
            }
        }

        private void Unzip(DatabasePackage package, int threadNum, StringBuilder zipLogger)
        {
            //for each zip file, put it in a try catch to see if we can catch any issues in case of a one-off IO error
            string zipFilePath = Path.Combine(Settings.RelhaxDownloadsFolder, package.ZipFile);
            for(int i = 3; i > 0; i--)//3 strikes and you're out
            {
                try
                {
                    using (RelhaxZipFile zip = new RelhaxZipFile(zipFilePath))
                    {
                        zip.ThreadID = threadNum;
                        //update args and logging here...
                        //first for loop takes care of any path replacing in the zipfile
                        for(int j = 0; j < zip.Entries.Count; j++)
                        {
                            //check for versiondir
                            string zipEntryName = zip[j].FileName;
                            if (zipEntryName.Contains("versiondir"))
                                zipEntryName = zipEntryName.Replace("versiondir", Settings.WoTClientVersion);
                            //check for xvmConfigFolderName
                            if (zipEntryName.Contains("configs/xvm/xvmConfigFolderName"))
                                zipEntryName = zipEntryName.Replace("configs/xvm/xvmConfigFolderName", "configs/xvm/TODO");
                            if(Regex.IsMatch(zipEntryName, @"_patch.*\.xml"))
                            {
                                //build the patch name manually
                                StringBuilder sb = new StringBuilder();
                                //first get the "_patch/" prefix (that does not change)
                                sb.Append(zipEntryName.Substring(0,7));
                                //pad the patchGroup name
                                sb.Append(package.PatchGroup.ToString("D3"));
                                //name else doesn't need to change, to set the rest of the name and use it
                                sb.Append(zipEntryName.Substring(7));
                                //and save it to the string
                                zipEntryName = sb.ToString();
                            }
                            //save zip entry file modifications back to zipfile
                            zip[j].FileName = zipEntryName;
                        }
                        //attach the event handler to report progress
                        zip.ExtractProgress += OnZipfileExtractProgress;
                        //second loop extracts each file and checks for extraction macros
                        for (int j = 0; j < zip.Entries.Count; j++)
                        {
                            string zipFilename = zip[j].FileName;
                            //create logging entry
                            string loggingCompletePath = string.Empty;
                            string extractPath = string.Empty;

                            //check each entry if it's going to a custom extraction location by form of macro
                            //default is nothing, goes to root (World_of_Tanks) directory
                            //if macro is found, extract to a different folder
                            //first check for "_AppData" macro in root of zip file
                            //NOTE: "WoTAppData" is not supported
                            //check if the root entry contains it
                            //_AppData = root application data directory
                            if (zipFilename.Length >= "_AppData".Length && zipFilename.Substring(0,"_AppData".Length).Equals("_AppData"))
                            {
                                zipFilename = zipFilename.Replace("_AppData", string.Empty);
                                if(!string.IsNullOrWhiteSpace(zipFilename))
                                {
                                    zip[j].FileName = zipFilename;
                                    zip[j].Extract(Settings.AppDataFolder, ExtractExistingFileAction.OverwriteSilently);
                                    extractPath = Settings.AppDataFolder;
                                }
                            }
                            //_RelhaxRoot = app startup directory
                            else if (zipFilename.Length >= "_RelhaxRoot".Length && zipFilename.Substring(0, "_RelhaxRoot".Length).Equals("_RelhaxRoot"))
                            {
                                zipFilename = zipFilename.Replace("_RelhaxRoot", string.Empty);
                                if (!string.IsNullOrWhiteSpace(zipFilename))
                                {
                                    zip[j].FileName = zipFilename;
                                    zip[j].Extract(Settings.ApplicationStartupPath, ExtractExistingFileAction.OverwriteSilently);
                                    extractPath = Settings.ApplicationStartupPath;
                                }
                            }
                            //default is World_of_Tanks directory
                            else
                            {
                                zip[j].Extract(Settings.WoTDirectory, ExtractExistingFileAction.OverwriteSilently);
                                extractPath = Settings.WoTDirectory;
                            }
                            loggingCompletePath = Path.Combine(extractPath, zipFilename.Replace(@"/", @"\"));
                            zipLogger.AppendLine(package.LogAtInstall ? loggingCompletePath : "#" + loggingCompletePath);
                        }
                    }
                    //set i to 0 so that it breaks out of the loop
                    i = 0;
                }
                catch (Exception e)
                {
                    if(i <= 1)
                    {
                        //log as error, 3 tries and all failures
                        Logging.Exception("Failed to extract zipfile {0}, exception message:{1}{2}", package.ZipFile, Environment.NewLine, e.ToString());
                        MessageBox.Show(string.Format("{0}, {1} {2} {3}",
                            Translations.GetTranslatedString("zipReadingErrorMessage1"), package.ZipFile, Translations.GetTranslatedString("zipReadingErrorMessage2"),
                            Translations.GetTranslatedString("zipReadingErrorHeader")));
                        //delete the file (if it exists)
                        if(File.Exists(zipFilePath))
                        {
                            try
                            {
                                File.Delete(zipFilePath);
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Logging.Exception(ex.ToString());
                            }
                        }
                    }
                    else
                    {
                        //log warning, try again
                        Logging.Warning("Exception of type {0} caught, retryNum = {1} (to 1), on thread = {2}, zipfile = {3}", e.GetType().Name, i, threadNum, package.ZipFile);
                    }
                }
            }
        }

        private void OnZipfileExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            //this thing fires for all sorts of events, but we only care about 3. and we want to keep locking to a minimum, as well as progress reporting
            //(no needless locking or progress reporting)
            switch (e.EventType)
            {
                case ZipProgressEventType.Extracting_EntryBytesWritten:
                    lock (Progress)
                    {
                        Prog.ChildCurrent = (int)e.BytesTransferred;
                        Prog.ChildTotal = (int)e.TotalBytesToTransfer;
                        Prog.Filename = e.ArchiveName;
                        Prog.ThreadID = (uint)(sender as RelhaxZipFile).ThreadID;
                        Progress.Report(Prog);
                    }
                    break;
                case ZipProgressEventType.Extracting_BeforeExtractEntry:
                case ZipProgressEventType.Extracting_AfterExtractEntry:
                    lock (Progress)
                    {
                        Prog.ParrentCurrent = e.EntriesExtracted;
                        Prog.ParrentTotal = e.EntriesTotal;
                        Prog.EntryFilename = e.CurrentEntry.FileName;
                        Prog.Filename = e.ArchiveName;
                        Prog.ThreadID = (uint)(sender as RelhaxZipFile).ThreadID;
                        Progress.Report(Prog);
                    }
                    break;
            }
        }

        private void BuildContourIcons(Task patchTask, Task createShortcutsTask)
        {
            Logging.WriteToLog(string.Format("Creating of atlases, current install time = {0} msec",
                (int)InstallStopWatch.Elapsed.TotalMilliseconds));
            List<Atlas> atlases = MakeAtlasList();
            if (atlases.Count > 0)
            {
                StringBuilder atlasBuilder = new StringBuilder();
                atlasBuilder.AppendLine("/*   Atlases   */");
                int done = 0;
                foreach (Atlas atlas in atlases)
                {
                    done++;
                    //replace macros
                    atlas.Pkg = Utils.MacroReplace(atlas.Pkg, ReplacementTypes.FilePath);
                    atlas.AtlasSaveDirectory = Utils.MacroReplace(atlas.AtlasSaveDirectory, ReplacementTypes.FilePath);
                    for(int i = 0; i < atlas.ImageFolderList.Count; i++)
                    {
                        atlas.ImageFolderList[i] = Utils.MacroReplace(atlas.ImageFolderList[i], ReplacementTypes.FilePath);
                    }
                    if (TaskNullOrDone(patchTask) && TaskNullOrDone(createShortcutsTask))
                    {
                        Prog.ChildTotal = 1;
                        Prog.ParrentTotal = atlases.Count;
                        Prog.ParrentCurrent = done;
                        Prog.ChildCurrent = 0;
                        Prog.Filename = atlas.AtlasFile;
                        LockProgress();
                    }
                    Utils.CreateAtlas(atlas);
                    atlasBuilder.AppendLine(atlas.MapFile);
                }
                Logging.Installer(atlasBuilder.ToString());
            }
            else
                Logging.Warning("building contour icons triggered, but none exist! (is this the intent)");
        }

        private void ProcessTriggers(List<string> packageTriggers)
        {
            //at least 1 trigger exists
            foreach (string triggerFromPackage in packageTriggers)
            {
                //in theory, each database package trigger is unique in each package AND in installer
                Trigger match = Triggers.Find(search => search.Name.ToLower().Equals(triggerFromPackage.ToLower()));
                if (match == null)
                {
                    Logging.Debug("trigger match is null (no match!) {0}", triggerFromPackage);
                    continue;
                }
                //this could be in multiple threads, so needs to be done in a lock statement (read modify write operation)
                lock (Triggers)
                {
                    match.NumberProcessed++;
                    if (match.NumberProcessed >= match.Total)
                    {
                        string message = string.Format("matched trigger {0} has numberProcessed {1}, total is {2}", match.Name, match.NumberProcessed, match.Total);
                        if (match.NumberProcessed > match.Total)
                            Logging.Error(message);
                        else //it's equal
                            Logging.Debug(message);
                        if (match.Fired)
                        {
                            Logging.Error("trigger {0} has already fired, skipping", match.Name);
                        }
                        else
                        {
                            Logging.Debug("trigger {0} is starting", match.Name);
                            match.Fired = true;
                            //hard_coded list of triggers that can be fired from list at top of class
                            switch (match.Name)
                            {
                                case TriggerContouricons:
                                    match.TriggerTask = Task.Run(() => BuildContourIcons(null,null));
                                    break;
                                case TriggerInstallFonts:
                                    match.TriggerTask = Task.Run(() =>
                                    {
                                        string[] fontsToInstall = Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, Settings.FontsToInstallFoldername), SearchOption.TopDirectoryOnly, false, @"*", 50, 3, true);
                                        if (fontsToInstall == null || fontsToInstall.Count() == 0)
                                            Logging.WriteToLog("...skipped (no font files to install)");
                                        else
                                            InstallFonts(fontsToInstall,null,null,null);
                                    });
                                    break;
                                default:
                                    Logging.Error("Invalid trigger name for switch block: {0}", match.Name);
                                    break;
                            }
                        }
                    }
                }
            }
        }


        #region Patch list parsing
        private List<Patch> MakePatchList()
        {
            //get a list of all files in the dedicated patch directory
            //foreach one add it to the patch list
            List<Patch> patches = new List<Patch>();

            //if the patches folder does not exist, then there are no patches to load or run
            if (!Directory.Exists(Path.Combine(Settings.WoTDirectory, Settings.PatchFolderName)))
            {
                Logging.Debug("\"{0}\" folder does not exist, skipping", Settings.PatchFolderName);
                Logging.Debug("Number of patches: {0}", patches.Count());
                return patches;
            }

            //get the list of all patches in the directory
            string[] patch_files = Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, Settings.PatchFolderName), SearchOption.TopDirectoryOnly, false, @"*.xml", 50, 3, true);
            if (patch_files == null)
                Logging.WriteToLog("Failed to parse patches from patch directory (see above lines for more info", Logfiles.Application, LogLevel.Error);
            else
            {
                Logging.Debug("Number of patch files: {0}",patch_files.Count());
                //if there wern't any, don't bother doing anything
                if(patch_files.Count() > 0)
                {
                    string completePath = string.Empty;
                    foreach (string filename in patch_files)
                    {
                        completePath = Path.Combine(Settings.WoTDirectory, Settings.PatchFolderName, filename);
                        //just double check...
                        if(!File.Exists(completePath))
                        {
                            Logging.WriteToLog("patch file does not exist?? " + completePath, Logfiles.Application, LogLevel.Warning);
                            continue;
                        }
                        Utils.ApplyNormalFileProperties(completePath);
                        //ok NOW actually add the file to the patch list
                        AddPatchesFromFile(patches, completePath);
                    }
                }
            }
            return patches;
        }

        private void AddPatchesFromFile(List<Patch> patches, string filename)
        {
            //make an xml document to get all patches
            XmlDocument doc = XMLUtils.LoadXmlDocument(filename, XmlLoadType.FromFile);
            if (doc == null)
                return;
            //make new patch object for each entry
            //remember to add lots of logging
            XmlNodeList XMLpatches = XMLUtils.GetXMLNodesFromXPath(doc, "//patchs/patch");
            if(XMLpatches == null || XMLpatches.Count == 0)
            {
                Logging.Error("File {0} contains no patch entries", filename);
                return;
            }
            Logging.Info("Adding {0} patches from patchFile {1}", Logfiles.Application, XMLpatches.Count, filename);
            foreach(XmlNode patchNode in XMLpatches)
            {
                Patch p = new Patch();
                //we have the patchNode "patch" object, now we need to get it's children to actually get the properties of said patch
                foreach(XmlNode property in patchNode.ChildNodes)
                {
                    //each element in the xml gets put into the
                    //the corresponding attribute for the Patch instance
                    switch (property.Name)
                    {
                        case "type":
                            p.Type = property.InnerText;
                            break;
                        case "mode":
                            p.Mode = property.InnerText;
                            break;
                        case "patchPath":
                            p.PatchPath = property.InnerText;
                            break;
                        case "file":
                            p.File = property.InnerText;
                            break;
                        case "path":
                            p.Path = property.InnerText;
                            break;
                        case "line":
                            if(!string.IsNullOrWhiteSpace(property.InnerText))
                                p.Lines = property.InnerText.Split(',');
                            break;
                        case "search":
                            p.Search = property.InnerText;
                            break;
                        case "replace":
                            p.Replace = property.InnerText;
                            break;
                    }
                }
                patches.Add(p);
            }
        }
        #endregion

        #region Shortcut parsing

        private List<Shortcut> MakeShortcutList()
        {
            List<Shortcut> shortcuts = new List<Shortcut>();

            //if the patches folder does not exist, then there are no patches to load or run
            if (!Directory.Exists(Path.Combine(Settings.WoTDirectory, Settings.ShortcutFolderName)))
            {
                Logging.Debug("\"{0}\" folder does not exist, skipping", Settings.ShortcutFolderName);
                Logging.WriteToLog(string.Format("Number of shortcuts: {0}", shortcuts.Count()), Logfiles.Application, LogLevel.Debug);
                return shortcuts;
            }

            //get a list of all files in the dedicated shortcuts directory
            //foreach one add it to the patch list
            string[] shortcut_files = Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, Settings.ShortcutFolderName), SearchOption.TopDirectoryOnly, false, @"*.xml", 50, 3, true);
            if (shortcut_files == null)
                Logging.WriteToLog("Failed to parse shortcuts from directory", Logfiles.Application, LogLevel.Error);
            else if (shortcut_files.Count() == 0)
            {
                Logging.Info("No shortcut files to operate on");
            }
            else
            {
                Logging.WriteToLog(string.Format("Number of shortcut xml instruction files: {0}", shortcut_files.Count()), Logfiles.Application, LogLevel.Debug);
                //if there weren't any, don't bother doing anything
                string completePath = string.Empty;
                foreach (string filename in shortcut_files)
                {
                    completePath = Path.Combine(Settings.WoTDirectory, Settings.ShortcutFolderName, filename);
                    //apply "normal" file properties just in case the user's wot install directory is special
                    Utils.ApplyNormalFileProperties(completePath);
                    //ok NOW actually add the file to the patch list
                    Logging.Info("Adding shortcuts from shortcutFile {1}", Logfiles.Application, filename);
                    AddShortcutsFromFile(shortcuts, filename);
                }
            }
            return shortcuts;
        }

        private void AddShortcutsFromFile(List<Shortcut> shortcuts, string filename)
        {
            //make an xml document to get all shortcuts
            XmlDocument doc = XMLUtils.LoadXmlDocument(filename, XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Error("Failed to parse xml shortcut file, skipping");
                return;
            } 
            //make new patch object for each entry
            //remember to add lots of logging
            XmlNodeList XMLshortcuts = XMLUtils.GetXMLNodesFromXPath(doc, "//shortcuts/shortcut");
            if (XMLshortcuts == null || XMLshortcuts.Count == 0)
            {
                Logging.Warning("File {0} contains no shortcut entries", filename);
                return;
            }
            Logging.Info("Adding {0} shortcuts from shortcutFile {1}", Logfiles.Application, XMLshortcuts.Count, filename);
            foreach (XmlNode patchNode in XMLshortcuts)
            {
                Shortcut sc = new Shortcut();
                //we have the patchNode "patch" object, now we need to get it's children to actually get the properties of said patch
                foreach (XmlNode property in patchNode.ChildNodes)
                {
                    //each element in the xml gets put into the
                    //the corresponding attribute for the Patch instance
                    switch (property.Name)
                    {
                        case "path":
                            sc.Path = property.InnerText;
                            break;
                        case "name":
                            sc.Name = property.InnerText;
                            break;
                        case "enabled":
                            sc.Enabled = Utils.ParseBool(property.InnerText, false);
                            break;
                    }
                }
                shortcuts.Add(sc);
            }
        }
        #endregion

        #region xml unpack
        //XML Unpack
        private List<XmlUnpack> MakeXmlUnpackList()
        {
            List<XmlUnpack> XmlUnpacks = new List<XmlUnpack>();

            //if the patches folder does not exist, then there are no patches to load or run
            if (!Directory.Exists(Path.Combine(Settings.WoTDirectory, Settings.XmlUnpackFolderName)))
            {
                Logging.Debug("\"{0}\" folder does not exist, skipping", Settings.XmlUnpackFolderName);
                Logging.WriteToLog(string.Format("Number of XmlUnpack files: {0}", XmlUnpacks.Count()), Logfiles.Application, LogLevel.Debug);
                return XmlUnpacks;
            }

            //get a list of all files in the dedicated patch directory
            //foreach one add it to the patch list
            string[] unpack_files = Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, Settings.XmlUnpackFolderName), SearchOption.TopDirectoryOnly, false, @"*.xml", 50, 3, true);
            if (unpack_files == null)
                Logging.WriteToLog("Failed to parse xml unpacks from unpack directory", Logfiles.Application, LogLevel.Error);
            else
            {
                Logging.WriteToLog(string.Format("Number of XmlUnpack files: {0}", unpack_files.Count()), Logfiles.Application, LogLevel.Debug);
                //if there wern't any, don't bother doing anything
                if (unpack_files.Count() > 0)
                {
                    string completePath = string.Empty;
                    foreach (string filename in unpack_files)
                    {
                        completePath = Path.Combine(Settings.WoTDirectory, Settings.ShortcutFolderName, filename);

                        //ok NOW actually add the file to the patch list
                        Logging.Info("Adding xml unpack entries from file {1}", Logfiles.Application, filename);
                        AddXmlUnpackFromFile(XmlUnpacks, filename);
                    }
                }
            }

            //perform macro replacement on all xml unpack entries
            foreach(XmlUnpack xmlUnpack in XmlUnpacks)
            {
                xmlUnpack.DirectoryInArchive = Utils.MacroReplace(xmlUnpack.DirectoryInArchive, ReplacementTypes.FilePath);
                xmlUnpack.FileName = Utils.MacroReplace(xmlUnpack.FileName, ReplacementTypes.FilePath);
                xmlUnpack.ExtractDirectory = Utils.MacroReplace(xmlUnpack.ExtractDirectory, ReplacementTypes.FilePath);
                xmlUnpack.NewFileName = Utils.MacroReplace(xmlUnpack.NewFileName, ReplacementTypes.FilePath);
                xmlUnpack.Pkg = Utils.MacroReplace(xmlUnpack.Pkg, ReplacementTypes.FilePath);
            }

            return XmlUnpacks;
        }

        //actual XML unpack parsing TODO
        private void AddXmlUnpackFromFile(List<XmlUnpack> XmlUnpacks, string filename)
        {
            //make an xml document to get all Xml Unpacks
            XmlDocument doc = XMLUtils.LoadXmlDocument(filename, XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Error("failed to parse xml file");
                return;
            }
            //make new patch object for each entry
            //remember to add lots of logging
            XmlNodeList XMLUnpacks = XMLUtils.GetXMLNodesFromXPath(doc, "//files/file");
            if (XMLUnpacks == null || XMLUnpacks.Count == 0)
            {
                Logging.Error("File {0} contains no XmlUnapck entries", filename);
                return;
            }
            Logging.Info("Adding {0} xml unpack entries from file {1}", Logfiles.Application, XMLUnpacks.Count, filename);
            foreach (XmlNode patchNode in XMLUnpacks)
            {
                XmlUnpack xmlup = new XmlUnpack();
                //we have the patchNode "patch" object, now we need to get it's children to actually get the properties of said patch
                foreach (XmlNode property in patchNode.ChildNodes)
                {
                    //each element in the xml gets put into the
                    //the corresponding attribute for the Patch instance
                    switch (property.Name)
                    {
                        case "pkg":
                            xmlup.Pkg = property.InnerText;
                            break;
                        case "directoryInArchive":
                            xmlup.DirectoryInArchive = property.InnerText;
                            break;
                        case "fileName":
                            xmlup.FileName = property.InnerText;
                            break;
                        case "extractDirectory":
                            xmlup.ExtractDirectory = property.InnerText;
                            break;
                        case "newFileName":
                            xmlup.NewFileName = property.InnerText;
                            break;
                    }
                }
                XmlUnpacks.Add(xmlup);
            }
        }
        #endregion

        #region atlas parsing
        //Atlas parsing
        private List<Atlas> MakeAtlasList()
        {
            List<Atlas> atlases = new List<Atlas>();

            //if the patches folder does not exist, then there are no patches to load or run
            if (!Directory.Exists(Path.Combine(Settings.WoTDirectory, Settings.AtlasCreationFoldername)))
            {
                Logging.Debug("\"{0}\" folder does not exist, skipping", Settings.AtlasCreationFoldername);
                Logging.WriteToLog(string.Format("Number of atlases: {0}", atlases.Count()), Logfiles.Application, LogLevel.Debug);
                return atlases;
            }

            //get a list of all files in the dedicated patch directory
            //foreach one add it to the patch list
            string[] atlas_files = Utils.DirectorySearch(Path.Combine(Settings.WoTDirectory, Settings.AtlasCreationFoldername), SearchOption.TopDirectoryOnly, false, @"*.xml", 50, 3, true);
            if (atlas_files == null)
                Logging.WriteToLog("Failed to parse atlases from atlas directory", Logfiles.Application, LogLevel.Error);
            else
            {
                Logging.WriteToLog(string.Format("Number of atlas files: {0}", atlas_files.Count()), Logfiles.Application, LogLevel.Debug);
                //if there wern't any, don't bother doing anything
                if (atlas_files.Count() > 0)
                {
                    string completePath = string.Empty;
                    foreach (string filename in atlas_files)
                    {
                        completePath = Path.Combine(Settings.WoTDirectory, Settings.ShortcutFolderName, filename);
                        //apply "normal" file properties just in case the user's wot install directory is special
                        Utils.ApplyNormalFileProperties(completePath);
                        //ok NOW actually add the file to the patch list
                        Logging.Info("Adding atlas entries from file {1}", Logfiles.Application, filename);
                        AddAtlasFromFile(atlases, filename);
                    }
                }
            }
            return atlases;
        }

        //actual Atlas parsing TODO
        private void AddAtlasFromFile(List<Atlas> atlases, string filename)
        {
            //make an xml document to get all Xml Unpacks
            XmlDocument doc = XMLUtils.LoadXmlDocument(filename, XmlLoadType.FromFile);
            if (doc == null)
                return;
            //make new patch object for each entry
            //remember to add lots of logging
            XmlNodeList XMLAtlases = XMLUtils.GetXMLNodesFromXPath(doc, "//atlases/atlas");
            if (XMLAtlases == null || XMLAtlases.Count == 0)
            {
                Logging.Error("File {0} contains no atlas entries", filename);
                return;
            }
            Logging.Info("Adding {0} atlas entries from file {1}", Logfiles.Application, XMLAtlases.Count, filename);
            foreach (XmlNode atlasNode in XMLAtlases)
            {
                Atlas sc = new Atlas();
                //we have the patchNode "patch" object, now we need to get it's children to actually get the properties of said patch
                foreach (XmlNode property in atlasNode.ChildNodes)
                {
                    //each element in the xml gets put into the
                    //the corresponding attribute for the Patch instance
                    switch (property.Name)
                    {
                        case "pkg":
                            sc.Pkg = property.InnerText;
                            break;
                        case "directoryInArchive":
                            sc.DirectoryInArchive = property.InnerText;
                            break;
                        case "atlasFile":
                            sc.AtlasFile = property.InnerText;
                            break;
                        case "mapFile":
                            sc.MapFile = property.InnerText;
                            break;
                        case "generateMap":
                            sc.GenerateMap = Utils.ParseEnum(property.InnerText,Atlas.State.True);
                            break;
                        case "powOf2":
                            sc.PowOf2 = Utils.ParseEnum(property.InnerText,Atlas.State.False);
                            break;
                        case "square":
                            sc.Square = Utils.ParseEnum(property.InnerText,Atlas.State.False);
                            break;
                        case "fastImagePacker":
                            sc.FastImagePacker = Utils.ParseBool(property.InnerText,false);
                            break;
                        case "padding":
                            sc.Padding = Utils.ParseInt(property.InnerText,1);
                            break;
                        case "atlasWidth":
                            sc.AtlasWidth = Utils.ParseInt(property.InnerText, 2400);
                            break;
                        case "atlasHeight":
                            sc.AtlasHeight = Utils.ParseInt(property.InnerText,8192);
                            break;
                        case "atlasSaveDirectory":
                            sc.AtlasSaveDirectory = property.InnerText;
                            break;
                        case "imageFolders":
                            //sc.im = property.InnerText;
                            foreach(XmlNode imageFolder in property.ChildNodes)
                            {
                                sc.ImageFolderList.Add(imageFolder.InnerText);
                            }
                            break;
                    }
                }
                atlases.Add(sc);
            }
        }
        #endregion

        #region other

        private bool TaskNullOrDone(Task task)
        {
            return task == null || task.IsCompleted;
        }

        private void LockProgress()
        {
            lock(Progress)
            {
                Progress.Report(Prog);
            }
        }
        #endregion

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~InstallEngine() {
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
