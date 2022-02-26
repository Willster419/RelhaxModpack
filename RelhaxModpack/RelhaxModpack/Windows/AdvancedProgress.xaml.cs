using System.Text;
using System.IO;
using RelhaxModpack.UI;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities.Enums;
using System.Collections.Generic;
using System.Linq;
using RelhaxModpack.Utilities;

namespace RelhaxModpack.Windows
{   ///I exist as a branch
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AdvancedProgress : RelhaxWindow
    {
        /// <summary>
        /// Create and initialize the AdvancedProgress window
        /// </summary>
        public AdvancedProgress(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Flag for if the user installation reporter should have been called
        /// </summary>
        public bool ShouldUserInstallBeCalled = false;

        //make a bunch of handlers for referencing the install progress options later
        /// <summary>
        /// The UI Reporting object for the step of backing up mods
        /// </summary>
        public RelhaxInstallTaskReporter BackupModsReporter = null;

        /// <summary>
        /// The UI Reporting object for the steps of backing up data, clearing cache, and clearing logs
        /// </summary>
        public RelhaxInstallTaskReporter BackupDataClearCacheClearLogsReporter = null;

        /// <summary>
        /// The UI Reporting object for the step of cleaning mods
        /// </summary>
        public RelhaxInstallTaskReporter CleanModsReporter = null;

        /// <summary>
        /// The UI Reporting object for the step of extracting mods
        /// </summary>
        public RelhaxInstallTaskReporter[] ExtractionModsReporters;

        /// <summary>
        /// The UI Reporting object for the step of extracting user mods
        /// </summary>
        public RelhaxInstallTaskReporter ExtractionUserModsReporter = null;

        /// <summary>
        /// The UI Reporting object for the step of unpacking xml files
        /// </summary>
        public RelhaxInstallTaskReporter RestoreDataXmlUnpackReporter = null;

        /// <summary>
        /// The UI Reporting object for the step of patching files
        /// </summary>
        public RelhaxInstallTaskReporter PatchReporter = null;

        /// <summary>
        /// The UI Reporting object for the step of creating shortcuts
        /// </summary>
        public RelhaxInstallTaskReporter ShortcutsReporter = null;

        /// <summary>
        /// The UI Reporting object for the step of Creating atlas files
        /// </summary>
        public RelhaxInstallTaskReporter AtlasReporter = null;

        /// <summary>
        /// The UI Reporting object for the step of font install, trimming download cache, and cleanup
        /// </summary>
        public RelhaxInstallTaskReporter FontInstallTrimDownloadCacheCleanupReporter = null;

        private InstallerExitCodes lastExitCode = InstallerExitCodes.Success;

        private int lastInstallGroup = -1;

        private RelhaxInstallerProgress lastRelhaxInstallerProgressExtraction = null;

        /// <summary>
        /// Report download progress of a package waiting to install.
        /// </summary>
        /// <param name="progress">The progress object.</param>
        public void OnReportDownload(RelhaxDownloadProgress progress)
        {
            if (lastRelhaxInstallerProgressExtraction.WaitingOnDownloadsOfAThread == null)
                return;

            int indexWaitingOnDownload = 0;
            foreach (bool b in lastRelhaxInstallerProgressExtraction.WaitingOnDownloadsOfAThread)
            {
                if (b)
                    break;
                indexWaitingOnDownload++;
            }
            if (indexWaitingOnDownload >= lastRelhaxInstallerProgressExtraction.WaitingOnDownloadsOfAThread.Length)
                return;

            ExtractionModsReporters[indexWaitingOnDownload].TaskMinimum = 0;
            ExtractionModsReporters[indexWaitingOnDownload].TaskMaximum = progress.ChildTotal;
            ExtractionModsReporters[indexWaitingOnDownload].TaskValue = progress.ChildCurrent;

            //break it up into lines cause it's hard to read
            //"downloading package_name"
            string line1 = string.Format("{0} {1}", Translations.GetTranslatedString("Downloading"), progress.DatabasePackage.PackageName);

            //"zip_file_name"
            string line2 = progress.DatabasePackage.ZipFile;

            //https://stackoverflow.com/questions/9869346/double-string-format
            //"2MB of 8MB"
            string line3 = string.Format("{0} {1} {2}", FileUtils.SizeSuffix((ulong)progress.ChildCurrent, 1, true), Translations.GetTranslatedString("of"), FileUtils.SizeSuffix((ulong)progress.ChildTotal, 1, true));

            //also report to the download message process
            ExtractionModsReporters[indexWaitingOnDownload].TaskText = string.Format("{0}\n{1}\n{2}", line1, line2, line3);
        }

        /// <summary>
        /// Update the advanced progress UI objects
        /// </summary>
        /// <param name="progress">The progress report object</param>
        public void OnReportAdvancedProgress(RelhaxInstallerProgress progress)
        {
            //update the internal exit code only if it's reporting a new phase
            if((int)progress.InstallStatus > (int)lastExitCode)
            {
                //get the difference of how many steps it has advanced
                //for each step advanced, toggle the status of the previous one
                int difference = (int)progress.InstallStatus - (int)lastExitCode;
                for(int i = 0; i < difference; i++)
                {
                    InstallerExitCodes toToggle = (InstallerExitCodes)((int)lastExitCode + i);
                    ToggleComplete(toToggle);
                }
                lastExitCode = progress.InstallStatus;
            }

            //take care of active displayed tab
            if((int)progress.InstallStatus > (int)InstallerExitCodes.UserExtractionError)
            {
                if (!MainTabControl.SelectedItem.Equals(PostInstallTab))
                    MainTabControl.SelectedItem = PostInstallTab;
            }
            else if ((int)progress.InstallStatus > (int)InstallerExitCodes.CleanModsError)
            {
                if (!MainTabControl.SelectedItem.Equals(ExtractionTab))
                    MainTabControl.SelectedItem = ExtractionTab;
            }
            else
            {
                if (!MainTabControl.SelectedItem.Equals(PreInstallTab))
                    MainTabControl.SelectedItem = PreInstallTab;
            }

            //report progress
            switch (progress.InstallStatus)
            {
                case InstallerExitCodes.BackupModsError:
                    if(BackupModsReporter == null)
                    {
                        bool isBackupMod = ModpackSettings.BackupModFolder && !ModpackSettings.ExportMode;
                        if (isBackupMod)
                        {
                            Logging.Error("BackupModFolder is true, not export mode but its task reporter is null!");
                        }
                        break;
                    }

                    if (BackupModsReporter.ReportState != TaskReportState.Active)
                        BackupModsReporter.ReportState = TaskReportState.Active;

                    BackupModsReporter.TaskText = string.Format("{0}\n{1}", Translations.GetTranslatedString("installBackupMods"), progress.EntryFilename);

                    if (BackupModsReporter.TaskMinimum != 0)
                        BackupModsReporter.TaskMinimum = 0;
                    if (BackupModsReporter.TaskMaximum != progress.ParrentTotal)
                        BackupModsReporter.TaskMaximum = progress.ParrentTotal;
                    if (BackupModsReporter.TaskValue != progress.ParrentCurrent)
                        BackupModsReporter.TaskValue = progress.ParrentCurrent;

                    if (BackupModsReporter.SubTaskMinimum != 0)
                        BackupModsReporter.SubTaskMinimum = 0;
                    if (BackupModsReporter.SubTaskMaximum != progress.ChildTotal)
                        BackupModsReporter.SubTaskMaximum = progress.ChildTotal;
                    if (BackupModsReporter.SubTaskValue != progress.ChildCurrent)
                        BackupModsReporter.SubTaskValue = progress.ChildCurrent;
                    break;
                case InstallerExitCodes.BackupDataError:
                case InstallerExitCodes.ClearCacheError:
                case InstallerExitCodes.ClearLogsError:
                    if(BackupDataClearCacheClearLogsReporter == null)
                    {
                        if ((ModpackSettings.SaveUserData || ModpackSettings.ClearCache || ModpackSettings.DeleteLogs) && (!ModpackSettings.ExportMode))
                        {
                            Logging.Error("Backup Data/Clear Cache/Clear Logs reporter is null and export mod is false! SaveUserData={0}, ClearCache={1}, DeleteLogs={2}",
                                ModpackSettings.SaveUserData, ModpackSettings.ClearCache, ModpackSettings.DeleteLogs);
                        }
                        break;
                    }

                    if (BackupDataClearCacheClearLogsReporter.ReportState != TaskReportState.Active)
                        BackupDataClearCacheClearLogsReporter.ReportState = TaskReportState.Active;

                    if (BackupDataClearCacheClearLogsReporter.TaskMinimum != 0)
                        BackupDataClearCacheClearLogsReporter.TaskMinimum = 0;
                    if (BackupDataClearCacheClearLogsReporter.TaskMaximum != 4)
                        BackupDataClearCacheClearLogsReporter.TaskMaximum = 4;

                    switch(progress.InstallStatus)
                    {
                        case InstallerExitCodes.BackupDataError:
                            if (BackupDataClearCacheClearLogsReporter.TaskValue != 1)
                                BackupDataClearCacheClearLogsReporter.TaskValue = 1;
                            BackupDataClearCacheClearLogsReporter.TaskText = Translations.GetTranslatedString("installBackupData");
                            break;
                        case InstallerExitCodes.ClearCacheError:
                            if (BackupDataClearCacheClearLogsReporter.TaskValue != 2)
                                BackupDataClearCacheClearLogsReporter.TaskValue = 2;
                            BackupDataClearCacheClearLogsReporter.TaskText = Translations.GetTranslatedString("installClearCache");
                            break;
                        case InstallerExitCodes.ClearLogsError:
                            if (BackupDataClearCacheClearLogsReporter.TaskValue != 3)
                                BackupDataClearCacheClearLogsReporter.TaskValue = 3;
                            BackupDataClearCacheClearLogsReporter.TaskText = Translations.GetTranslatedString("installClearLogs");
                            break;
                    }
                    break;
                case InstallerExitCodes.CleanModsError:
                    if(CleanModsReporter == null)
                    {
                        if(ModpackSettings.CleanInstallation || ModpackSettings.ExportMode || ModpackSettings.AutoInstall || !string.IsNullOrEmpty(CommandLineSettings.AutoInstallFileName))
                        {
                            Logging.Error("CleanModsReporter is null when it should not be! CleanInstallation={0}, ExportMode={1}, AutoInstall={2}, AutoInstallFileName={3}",
                                ModpackSettings.CleanInstallation, ModpackSettings.ExportMode, ModpackSettings.AutoInstall, CommandLineSettings.AutoInstallFileName);
                        }
                        break;
                    }

                    if (CleanModsReporter.ReportState != TaskReportState.Active)
                        CleanModsReporter.ReportState = TaskReportState.Active;

                    CleanModsReporter.TaskText = string.Format("{0}\n{1}", Translations.GetTranslatedString("installCleanMods"), progress.Filename);

                    if (CleanModsReporter.TaskMinimum != 0)
                        CleanModsReporter.TaskMinimum = 0;
                    if (CleanModsReporter.TaskMaximum != progress.ChildTotal)
                        CleanModsReporter.TaskMaximum = progress.ChildTotal;
                    if (CleanModsReporter.TaskValue != progress.ChildCurrent)
                        CleanModsReporter.TaskValue = progress.ChildCurrent;
                    break;
                case InstallerExitCodes.ExtractionError:
                    lastRelhaxInstallerProgressExtraction = progress;
                    if(ExtractionModsReporters[progress.ThreadID] == null)
                    {
                        Logging.Error("Extraction reporter for thread {0} is null! ID={0}", progress.ThreadID);
                        break;
                    }

                    if (lastInstallGroup != (int)progress.InstallGroup)
                    {
                        lastInstallGroup = (int)progress.InstallGroup;
                        for (int i = 0; i < progress.TotalThreads; i++)
                        {
                            ExtractionModsReporters[i].ReportState = TaskReportState.Inactive;
                        }
                    }
                    else if (ExtractionModsReporters[progress.ThreadID].ReportState != TaskReportState.Active)
                    {
                        ExtractionModsReporters[progress.ThreadID].ReportState = TaskReportState.Active;
                    }

                    StringBuilder builder = new StringBuilder();

                    if (progress.TotalPackagesofAThread == null || progress.CompletedPackagesOfAThread == null)
                    {
                        builder.AppendFormat("{0} {1} {2} {3} \n", Translations.GetTranslatedString("installExtractingMods"), Translations.GetTranslatedString("of"),
                            Translations.GetTranslatedString("installExtractingOfGroup"), progress.InstallGroup);
                    }
                    else
                    {
                        uint comp = progress.CompletedPackagesOfAThread[progress.ThreadID] > 0 ? progress.CompletedPackagesOfAThread[progress.ThreadID] : 1;
                        builder.AppendFormat("{0} {1} {2} {3} {4} {5}\n", Translations.GetTranslatedString("installExtractingMods"), comp,
                            Translations.GetTranslatedString("of"), progress.TotalPackagesofAThread[progress.ThreadID], Translations.GetTranslatedString("installExtractingOfGroup"),
                            progress.InstallGroup);
                    }

                    if(progress.WaitingOnDownloadsOfAThread != null && progress.WaitingOnDownloadsOfAThread[progress.ThreadID])
                    {
                        if(ExtractionModsReporters[progress.ThreadID].ReportState == TaskReportState.Inactive)
                        {
                            ExtractionModsReporters[progress.ThreadID].ReportState = TaskReportState.Active;
                        }
                    }
                    else
                    {
                        if (progress.FilenameOfAThread != null)
                            builder.AppendFormat("{0}\n", Path.GetFileName(progress.FilenameOfAThread[progress.ThreadID]));

                        if (progress.EntriesProcessedOfAThread != null && progress.EntriesTotalOfAThread != null)
                        {
                            uint compp = progress.EntriesProcessedOfAThread[progress.ThreadID] > 0 ? progress.EntriesProcessedOfAThread[progress.ThreadID] : 1;
                            builder.AppendFormat("{0} {1} {2} {3}\n", Translations.GetTranslatedString("installZipFileEntry"), compp,
                                    Translations.GetTranslatedString("of"), progress.EntriesTotalOfAThread[progress.ThreadID]);

                            if (ExtractionModsReporters[progress.ThreadID].TaskMinimum != 0)
                                ExtractionModsReporters[progress.ThreadID].TaskMinimum = 0;
                            if (ExtractionModsReporters[progress.ThreadID].TaskMaximum != (int)progress.EntriesTotalOfAThread[progress.ThreadID])
                                ExtractionModsReporters[progress.ThreadID].TaskMaximum = (int)progress.EntriesTotalOfAThread[progress.ThreadID];
                            if (ExtractionModsReporters[progress.ThreadID].TaskValue != (int)progress.EntriesProcessedOfAThread[progress.ThreadID])
                                ExtractionModsReporters[progress.ThreadID].TaskValue = (int)progress.EntriesProcessedOfAThread[progress.ThreadID];
                        }

                        if (progress.EntryFilenameOfAThread != null)
                            builder.AppendFormat("{0}\n", progress.EntryFilenameOfAThread[progress.ThreadID]);

                    }

                    if (progress.BytesProcessedOfAThread != null && progress.BytesTotalOfAThread != null)
                    {

                        if (ExtractionModsReporters[progress.ThreadID].SubTaskMinimum != 0)
                            ExtractionModsReporters[progress.ThreadID].SubTaskMinimum = 0;
                        if (ExtractionModsReporters[progress.ThreadID].SubTaskMaximum != (int)progress.BytesTotalOfAThread[progress.ThreadID])
                            ExtractionModsReporters[progress.ThreadID].SubTaskMaximum = (int)progress.BytesTotalOfAThread[progress.ThreadID];
                        if (ExtractionModsReporters[progress.ThreadID].SubTaskValue != (int)progress.BytesProcessedOfAThread[progress.ThreadID])
                            ExtractionModsReporters[progress.ThreadID].SubTaskValue = (int)progress.BytesProcessedOfAThread[progress.ThreadID];
                    }

                    ExtractionModsReporters[progress.ThreadID].TaskText = builder.ToString();
                    break;
                case InstallerExitCodes.UserExtractionError:
                    if(ExtractionUserModsReporter == null)
                    {
                        if(ShouldUserInstallBeCalled)
                        {
                            Logging.Error("ExtractionUserModsReporter is null when user has mods to extract!");
                        }
                        break;
                    }

                    if (ExtractionUserModsReporter.ReportState != TaskReportState.Active)
                        ExtractionUserModsReporter.ReportState = TaskReportState.Active;

                    ExtractionUserModsReporter.TaskText = string.Format("{0}\n{1}\n{2}",
                        Path.GetFileName(progress.Filename),
                        string.Format("{0} {1} {2}", (progress.EntriesProcessed > 0? progress.EntriesProcessed : 1), Translations.GetTranslatedString("of"), progress.EntriesTotal),
                        progress.EntryFilename);
                    break;
                case InstallerExitCodes.RestoreUserdataError:
                case InstallerExitCodes.XmlUnpackError:
                    if (progress.ParrentTotal == 0)
                        break;
                    if (RestoreDataXmlUnpackReporter == null)
                    {
                        RestoreDataXmlUnpackReporter = new RelhaxInstallTaskReporter(nameof(RestoreDataXmlUnpackReporter))
                        {
                            IsSubProgressActive = true,
                            TaskTitle = progress.InstallStatus == InstallerExitCodes.RestoreUserdataError ?
                                Translations.GetTranslatedString("AdvancedInstallRestoreData") : Translations.GetTranslatedString("AdvancedInstallXmlUnpack"),
                            ReportState = TaskReportState.Active,
                            TaskMaximum = 3,
                            TaskMinimum = 0,
                            LoadedAfterApply = true
                        };
                        PostInstallPanel.Children.Add(RestoreDataXmlUnpackReporter);
                    }

                    switch (progress.InstallStatus)
                    {
                        case InstallerExitCodes.RestoreUserdataError:
                            if (RestoreDataXmlUnpackReporter.TaskValue != 1)
                                RestoreDataXmlUnpackReporter.TaskValue = 1;
                            RestoreDataXmlUnpackReporter.TaskText = string.Format("{0} {1} {2} {3}\n{4}", Translations.GetTranslatedString("installRestoreUserdata"),
                                ((progress.ParrentCurrent) > 0? progress.ParrentCurrent : 1).ToString(), Translations.GetTranslatedString("of"), progress.ParrentTotal.ToString(), progress.Filename);
                            break;
                        case InstallerExitCodes.XmlUnpackError:
                            if (RestoreDataXmlUnpackReporter.TaskValue != 2)
                                RestoreDataXmlUnpackReporter.TaskValue = 2;
                            RestoreDataXmlUnpackReporter.TaskText = string.Format("{0} {1} {2} {3}\n{4}", Translations.GetTranslatedString("installXmlUnpack"),
                                ((progress.ParrentCurrent) > 0? progress.ParrentCurrent : 1).ToString(), Translations.GetTranslatedString("of"), progress.ParrentTotal.ToString(), progress.Filename);
                            break;
                    }

                    if (RestoreDataXmlUnpackReporter.SubTaskMinimum != 0)
                        RestoreDataXmlUnpackReporter.SubTaskMinimum = 0;
                    if (RestoreDataXmlUnpackReporter.SubTaskMaximum != progress.ParrentTotal)
                        RestoreDataXmlUnpackReporter.SubTaskMaximum = progress.ParrentTotal;
                    if (RestoreDataXmlUnpackReporter.SubTaskValue != progress.ParrentCurrent)
                        RestoreDataXmlUnpackReporter.SubTaskValue = progress.ParrentCurrent;

                    break;
                case InstallerExitCodes.PatchError:
                    if (progress.ParrentTotal == 0)
                        break;
                    if (PatchReporter == null)
                    {
                        PatchReporter = new RelhaxInstallTaskReporter(nameof(PatchReporter))
                        {
                            IsSubProgressActive = false,
                            TaskTitle = Translations.GetTranslatedString("AdvancedInstallPatchFiles"),
                            ReportState = TaskReportState.Active,
                            LoadedAfterApply = true
                        };
                        PostInstallPanel.Children.Add(PatchReporter);
                    }

                    PatchReporter.TaskText = string.Format("{0} {1} {2} {3}\n{4}", Translations.GetTranslatedString("installPatchFiles"), ((progress.ParrentCurrent) > 0? progress.ParrentCurrent : 1).ToString(),
                            Translations.GetTranslatedString("of"), progress.ParrentTotal.ToString(), progress.Filename);

                    if (PatchReporter.TaskMinimum != 0)
                        PatchReporter.TaskMinimum = 0;
                    if (PatchReporter.TaskMaximum != progress.ParrentTotal)
                        PatchReporter.TaskMaximum = progress.ParrentTotal;
                    if (PatchReporter.TaskValue != progress.ParrentCurrent)
                        PatchReporter.TaskValue = progress.ParrentCurrent;

                    break;
                case InstallerExitCodes.ShortcutsError:
                    if (!ModpackSettings.CreateShortcuts)
                        break;
                    if (progress.ParrentTotal == 0)
                        break;
                    if (ShortcutsReporter == null)
                    {
                        ShortcutsReporter = new RelhaxInstallTaskReporter(nameof(ShortcutsReporter))
                        {
                            IsSubProgressActive = false,
                            TaskTitle = Translations.GetTranslatedString("AdvancedInstallPatchFiles"),
                            ReportState = TaskReportState.Active,
                            LoadedAfterApply = true
                        };
                        PostInstallPanel.Children.Add(ShortcutsReporter);
                    }

                    ShortcutsReporter.TaskText = string.Format("{0} {1} {2} {3}\n{4}", Translations.GetTranslatedString("AdvancedInstallCreateShortcuts"), ((progress.ParrentCurrent) > 0? progress.ParrentCurrent : 1).ToString(),
                            Translations.GetTranslatedString("of"), progress.ParrentTotal.ToString(), progress.Filename);

                    if (ShortcutsReporter.TaskMinimum != 0)
                        ShortcutsReporter.TaskMinimum = 0;
                    if (ShortcutsReporter.TaskMaximum != progress.ParrentTotal)
                        ShortcutsReporter.TaskMaximum = progress.ParrentTotal;
                    if (ShortcutsReporter.TaskValue != progress.ParrentCurrent)
                        ShortcutsReporter.TaskValue = progress.ParrentCurrent;

                    break;
                case InstallerExitCodes.ContourIconAtlasError:
                    if (AtlasReporter == null && progress.ParrentTotal > 0)
                    {
                        AtlasReporter = new RelhaxInstallTaskReporter(nameof(AtlasReporter))
                        {
                            IsSubProgressActive = true,
                            TaskTitle = Translations.GetTranslatedString("AdvancedInstallCreateAtlas"),
                            ReportState = TaskReportState.Active,
                            LoadedAfterApply = true
                        };
                        PostInstallPanel.Children.Add(AtlasReporter);
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} {1} {2} {3}\n", Translations.GetTranslatedString("installContourIconAtlas"), ((progress.ParrentCurrent) > 0? progress.ParrentCurrent : 1).ToString(),
                            Translations.GetTranslatedString("of"), progress.ParrentTotal.ToString());
                    sb.AppendFormat("{0} {1} {2} {3}\n", progress.ChildCurrent.ToString(), Translations.GetTranslatedString("of"), progress.ChildTotal.ToString(),
                            Translations.GetTranslatedString("stepsComplete"));

                    if (AtlasReporter.TaskMinimum != 0)
                        AtlasReporter.TaskMinimum = 0;
                    if (AtlasReporter.TaskMaximum != progress.ParrentTotal)
                        AtlasReporter.TaskMaximum = progress.ParrentTotal;
                    if (AtlasReporter.TaskValue != progress.ParrentCurrent)
                        AtlasReporter.TaskValue = progress.ParrentCurrent;

                    if (AtlasReporter.SubTaskMinimum != 0)
                        AtlasReporter.SubTaskMinimum = 0;
                    if (AtlasReporter.SubTaskMaximum != progress.ChildTotal)
                        AtlasReporter.SubTaskMaximum = progress.ChildTotal;
                    if (AtlasReporter.SubTaskValue != progress.ChildCurrent)
                        AtlasReporter.SubTaskValue = progress.ChildCurrent;

                    AtlasReporter.TaskText = sb.ToString();
                    break;
                case InstallerExitCodes.FontInstallError:
                case InstallerExitCodes.TrimDownloadCacheError:
                case InstallerExitCodes.CleanupError:
                    if (FontInstallTrimDownloadCacheCleanupReporter == null)
                    {
                        FontInstallTrimDownloadCacheCleanupReporter = new RelhaxInstallTaskReporter(nameof(FontInstallTrimDownloadCacheCleanupReporter))
                        {
                            IsSubProgressActive = false,
                            ReportState = TaskReportState.Active,
                            LoadedAfterApply = true
                        };
                        PostInstallPanel.Children.Add(FontInstallTrimDownloadCacheCleanupReporter);
                    }

                    if (FontInstallTrimDownloadCacheCleanupReporter.TaskMinimum != 0)
                        FontInstallTrimDownloadCacheCleanupReporter.TaskMinimum = 0;
                    if (FontInstallTrimDownloadCacheCleanupReporter.TaskMaximum != 4)
                        FontInstallTrimDownloadCacheCleanupReporter.TaskMaximum = 4;

                    switch (progress.InstallStatus)
                    {
                        case InstallerExitCodes.FontInstallError:
                            if (FontInstallTrimDownloadCacheCleanupReporter.TaskValue != 1)
                                FontInstallTrimDownloadCacheCleanupReporter.TaskValue = 1;
                            FontInstallTrimDownloadCacheCleanupReporter.TaskText = Translations.GetTranslatedString("AdvancedInstallInstallFonts");
                            break;
                        case InstallerExitCodes.TrimDownloadCacheError:
                            if (FontInstallTrimDownloadCacheCleanupReporter.TaskValue != 2)
                                FontInstallTrimDownloadCacheCleanupReporter.TaskValue = 2;
                            FontInstallTrimDownloadCacheCleanupReporter.TaskText = Translations.GetTranslatedString("AdvancedInstallTrimDownloadCache");
                            break;
                        case InstallerExitCodes.CleanupError:
                            if (FontInstallTrimDownloadCacheCleanupReporter.TaskValue != 3)
                                FontInstallTrimDownloadCacheCleanupReporter.TaskValue = 3;
                            FontInstallTrimDownloadCacheCleanupReporter.TaskText = Translations.GetTranslatedString("AdvancedInstallCleanup");
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Toggle a reporting task to the complete state
        /// </summary>
        /// <param name="reporter">The task to toggle</param>
        public void ToggleComplete(RelhaxInstallTaskReporter reporter)
        {
            if (reporter == null)
                return;
            if (reporter.ReportState != TaskReportState.Complete && reporter.ReportState == TaskReportState.Active)
                reporter.ReportState = TaskReportState.Complete;
        }

        /// <summary>
        /// Toggle a reporting task to the error state
        /// </summary>
        /// <param name="reporter">The task to toggle</param>
        public void ToggleError(RelhaxInstallTaskReporter reporter)
        {
            if (reporter == null)
                return;
            if (reporter.ReportState != TaskReportState.Error && reporter.ReportState == TaskReportState.Active)
                reporter.ReportState = TaskReportState.Error;
        }

        /// <summary>
        /// Toggle a reporting task to the complete state
        /// </summary>
        /// <param name="exitCode">The reporter of this corresponding install step</param>
        /// <remarks>If the task reporter is null, it won't throw a NullRefrenceException</remarks>
        private void ToggleComplete(InstallerExitCodes exitCode)
        {
            switch (exitCode)
            {
                case InstallerExitCodes.BackupDataError:
                case InstallerExitCodes.ClearCacheError:
                case InstallerExitCodes.ClearLogsError:
                    ToggleComplete(BackupModsReporter);
                    break;
                case InstallerExitCodes.CleanModsError:
                    ToggleComplete(BackupDataClearCacheClearLogsReporter);
                    break;
                case InstallerExitCodes.ExtractionError:
                    ToggleComplete(CleanModsReporter);
                    break;
                case InstallerExitCodes.UserExtractionError:
                    foreach (RelhaxInstallTaskReporter reporter in ExtractionModsReporters)
                        ToggleComplete(reporter);
                    break;
                case InstallerExitCodes.RestoreUserdataError:
                case InstallerExitCodes.XmlUnpackError:
                    ToggleComplete(ExtractionUserModsReporter);
                    break;
                case InstallerExitCodes.PatchError:
                    ToggleComplete(RestoreDataXmlUnpackReporter);
                    break;
                case InstallerExitCodes.ShortcutsError:
                    ToggleComplete(PatchReporter);
                    break;
                case InstallerExitCodes.TrimDownloadCacheError:
                    ToggleComplete(ShortcutsReporter);
                    ToggleComplete(AtlasReporter);
                    break;
            }
        }
    }
}
