using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using RelhaxModpack.InstallerComponents;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack.Windows
{   ///I exist as a branch
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AdvancedProgress : RelhaxWindow
    {
        public AdvancedProgress()
        {
            InitializeComponent();
        }

        //make a bunch of handlers for referencing the install progress options later
        public RelhaxInstallTaskReporter BackupModsReporter = null;
        public RelhaxInstallTaskReporter BackupDataClearCacheClearLogsReporter = null;
        public RelhaxInstallTaskReporter CleanModsReporter = null;
        public RelhaxInstallTaskReporter[] ExtractionModsReporters;
        public RelhaxInstallTaskReporter ExtractionUserModsReporter = null;
        public RelhaxInstallTaskReporter RestoreDataXmlUnpackReporter = null;
        public RelhaxInstallTaskReporter PatchReporter = null;
        public RelhaxInstallTaskReporter ShortcutsReporter = null;
        public RelhaxInstallTaskReporter AtlasReporter = null;
        public RelhaxInstallTaskReporter FontInstallTrimDownloadCacheCleanupReporter = null;

        private InstallerExitCodes lastExitCode = InstallerExitCodes.Success;

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
                        BackupModsReporter.SubTaskMinimum = progress.ChildTotal;
                    if (BackupModsReporter.SubTaskValue != progress.ChildCurrent)
                        BackupModsReporter.SubTaskValue = progress.ChildCurrent;
                    break;
                case InstallerExitCodes.BackupDataError:
                case InstallerExitCodes.ClearCacheError:
                case InstallerExitCodes.ClearLogsError:
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
                    if (CleanModsReporter.ReportState != TaskReportState.Active)
                        CleanModsReporter.ReportState = TaskReportState.Active;

                    CleanModsReporter.TaskText = string.Format("{0}\n{1}", Translations.GetTranslatedString("installCleanMods"), progress.Filename);

                    if (CleanModsReporter.SubTaskMinimum != 0)
                        CleanModsReporter.SubTaskMinimum = 0;
                    if (CleanModsReporter.SubTaskMaximum != progress.ChildTotal)
                        CleanModsReporter.SubTaskMinimum = progress.ChildTotal;
                    if (CleanModsReporter.SubTaskValue != progress.ChildCurrent)
                        CleanModsReporter.SubTaskValue = progress.ChildCurrent;
                    break;
                case InstallerExitCodes.ExtractionError:
                    if (ExtractionModsReporters[progress.ThreadID].ReportState != TaskReportState.Active)
                        ExtractionModsReporters[progress.ThreadID].ReportState = TaskReportState.Active;

                    ExtractionModsReporters[progress.ThreadID].TaskText = string.Format("{0}\n{1}\n{2}",
                        Path.GetFileName(progress.Filename),
                        string.Format("{0} {1} {2}", progress.EntriesProcessed, Translations.GetTranslatedString("of"), progress.EntriesTotal),
                        progress.EntryFilename);
                    break;
                case InstallerExitCodes.UserExtractionError:
                    if (ExtractionUserModsReporter.ReportState != TaskReportState.Active)
                        ExtractionUserModsReporter.ReportState = TaskReportState.Active;

                    ExtractionUserModsReporter.TaskText = string.Format("{0}\n{1}\n{2}",
                        Path.GetFileName(progress.Filename),
                        string.Format("{0} {1} {2}", progress.EntriesProcessed, Translations.GetTranslatedString("of"), progress.EntriesTotal),
                        progress.EntryFilename);
                    break;
                case InstallerExitCodes.RestoreUserdataError:
                case InstallerExitCodes.XmlUnpackError:
                    if (progress.ParrentTotal == 0)
                        break;
                    if (RestoreDataXmlUnpackReporter == null)
                    {
                        RestoreDataXmlUnpackReporter = new RelhaxInstallTaskReporter()
                        {
                            IsSubProgressActive = true,
                            TaskTitle = progress.InstallStatus == InstallerExitCodes.RestoreUserdataError ?
                                Translations.GetTranslatedString("AdvancedInstallRestoreData") : Translations.GetTranslatedString("AdvancedInstallXmlUnpack"),
                            ReportState = TaskReportState.Active,
                            TaskMaximum = 3,
                            TaskMinimum = 0
                        };
                        PostInstallPanel.Children.Add(RestoreDataXmlUnpackReporter);
                    }

                    switch (progress.InstallStatus)
                    {
                        case InstallerExitCodes.RestoreUserdataError:
                            if (RestoreDataXmlUnpackReporter.TaskValue != 1)
                                RestoreDataXmlUnpackReporter.TaskValue = 1;
                            RestoreDataXmlUnpackReporter.TaskText = string.Format("{0}\n{1}", Translations.GetTranslatedString("AdvancedInstallRestoreData"), progress.Filename);
                            break;
                        case InstallerExitCodes.XmlUnpackError:
                            if (RestoreDataXmlUnpackReporter.TaskValue != 2)
                                RestoreDataXmlUnpackReporter.TaskValue = 2;
                            RestoreDataXmlUnpackReporter.TaskText = string.Format("{0}\n{1}", Translations.GetTranslatedString("AdvancedInstallXmlUnpack"), progress.Filename);
                            break;
                    }

                    if (RestoreDataXmlUnpackReporter.SubTaskMinimum != 0)
                        RestoreDataXmlUnpackReporter.SubTaskMinimum = 0;
                    if (RestoreDataXmlUnpackReporter.SubTaskMaximum != progress.ChildTotal)
                        RestoreDataXmlUnpackReporter.SubTaskMinimum = progress.ChildTotal;
                    if (RestoreDataXmlUnpackReporter.SubTaskValue != progress.ChildCurrent)
                        RestoreDataXmlUnpackReporter.SubTaskValue = progress.ChildCurrent;

                    break;
                case InstallerExitCodes.PatchError:
                    if (progress.ParrentTotal == 0)
                        break;
                    if (PatchReporter == null)
                    {
                        PatchReporter = new RelhaxInstallTaskReporter()
                        {
                            IsSubProgressActive = false,
                            TaskTitle = Translations.GetTranslatedString("AdvancedInstallPatchFiles"),
                            ReportState = TaskReportState.Active
                        };
                        PostInstallPanel.Children.Add(PatchReporter);
                    }

                    break;
                case InstallerExitCodes.ShortcutsError:
                    if (!ModpackSettings.CreateShortcuts)
                        break;
                    if (progress.ParrentTotal == 0)
                        break;
                    if (ShortcutsReporter == null)
                    {
                        ShortcutsReporter = new RelhaxInstallTaskReporter()
                        {
                            IsSubProgressActive = false,
                            TaskTitle = Translations.GetTranslatedString("AdvancedInstallInstallFonts"),
                            ReportState = TaskReportState.Active
                        };
                        PostInstallPanel.Children.Add(ShortcutsReporter);
                    }

                    break;
                case InstallerExitCodes.ContourIconAtlasError:
                    if (AtlasReporter == null && progress.ParrentTotal > 0)
                    {
                        AtlasReporter = new RelhaxInstallTaskReporter()
                        {
                            IsSubProgressActive = false,
                            TaskTitle = Translations.GetTranslatedString("AdvancedInstallCreateAtlas"),
                            ReportState = TaskReportState.Active
                        };
                        PostInstallPanel.Children.Add(AtlasReporter);
                    }

                    break;
                case InstallerExitCodes.FontInstallError:
                case InstallerExitCodes.TrimDownloadCacheError:
                case InstallerExitCodes.CleanupError:
                    if (FontInstallTrimDownloadCacheCleanupReporter == null)
                    {
                        FontInstallTrimDownloadCacheCleanupReporter = new RelhaxInstallTaskReporter()
                        {
                            IsSubProgressActive = false,
                            ReportState = TaskReportState.Active
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

        public void ToggleComplete(RelhaxInstallTaskReporter reporter)
        {
            if (reporter == null)
                return;
            if (reporter.ReportState != TaskReportState.Complete && reporter.ReportState == TaskReportState.Active)
                reporter.ReportState = TaskReportState.Complete;
        }

        public void ToggleError(RelhaxInstallTaskReporter reporter)
        {
            if (reporter == null)
                return;
            if (reporter.ReportState != TaskReportState.Error && reporter.ReportState == TaskReportState.Active)
                reporter.ReportState = TaskReportState.Error;
        }

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
