using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack.InstallerComponents
{
    #region Event stuff
    public enum InstallerExitCodes
    {
        Success,
        DownloadModsError,
        ClearCacheError,
        ClearLogsError,
        BackupModsError,
        CleanModsError,
        ExtractionError,
        PatchError,
        ShortcustError,
        ContourIconAtlasError,
        CleanupError,
        UnknownError
    }
    public class RelhaxInstallFinishedEventArgs : EventArgs
    {
        public InstallerExitCodes ExitCodes;
        public string ErrorMessage;
    }
    public delegate void InstallFinishedDelegate(object sender, RelhaxInstallFinishedEventArgs e);
    public delegate void InstallProgressDelegate(object sender, RelhaxProgress e);
    #endregion

    public class InstallEngine : IDisposable
    {
        #region Instance Variables
        public List<DatabasePackage>[] orderedPackagesToInstall;
        public event InstallFinishedDelegate OnInstallFinish;
        public event InstallProgressDelegate OnInstallProgress;
        public RelhaxInstallFinishedEventArgs InstallFinishedEventArgs = new RelhaxInstallFinishedEventArgs();
        public Stopwatch InstallStopWatch = new Stopwatch();
        private TimeSpan OldTime;
        private RelhaxProgress Progress = new RelhaxProgress();
        #endregion

        #region More boring stuff
        public InstallEngine()
        {
            //init the install engine
        }
        private void ReportProgress()
        {
            if(OnInstallProgress != null)
            {
                OnInstallProgress(this, Progress);
            }
        }
        private void ReportFinish()
        {
            if(OnInstallFinish != null)
            {
                OnInstallFinish(this, InstallFinishedEventArgs);
            }
        }
        #endregion

        public async void RunInstallationAsync()
        {
            if(orderedPackagesToInstall == null || orderedPackagesToInstall.Count() == 0)
                throw new BadMemeException("WOW you really suck at programming");
            if (OnInstallFinish == null || OnInstallProgress == null)
                throw new BadMemeException("HOW DAFAQ DID YOU FAQ THIS UP???");
            Logging.WriteToLog("Installation starts now from RunInstallation() in Install Engine");
            //do more stuff here im sure like init log files
            
            //and reset the stopwatch
            InstallStopWatch.Reset();

            //step 1 on install: backup user mods
            OldTime = InstallStopWatch.Elapsed;
            Progress.TotalCurrent++;
            InstallFinishedEventArgs.ExitCodes++;
            Logging.WriteToLog("Backup of mods, current install time = 0 msec");
            if (ModpackSettings.BackupModFolder)
            {
                if (!await BackupModsAsync())
                {
                    ReportProgress();
                    ReportFinish();
                    return;
                }
                Logging.WriteToLog(string.Format("Backup of mods complete, took {0}msec",
                    InstallStopWatch.Elapsed.TotalMilliseconds - OldTime.TotalMilliseconds));
            }
            else
                Logging.WriteToLog("...skipped");

            //step 2: clear cache
            OldTime = InstallStopWatch.Elapsed;
            Progress.TotalCurrent++;
            InstallFinishedEventArgs.ExitCodes++;
            Logging.WriteToLog(string.Format("Cleaning ofcache folders, current install time = {0} msec",
                InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.ClearCache)
            {

            }
            else
                Logging.WriteToLog("...skipped");

            //step 3: clear logs
            OldTime = InstallStopWatch.Elapsed;
            Progress.TotalCurrent++;
            InstallFinishedEventArgs.ExitCodes++;
            Logging.WriteToLog(string.Format("Cleaning of logs, current install time = {0} msec",
                InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.DeleteLogs)
            {

            }
            else
                Logging.WriteToLog("...skipped");

            //step 4: clean mods folders
            OldTime = InstallStopWatch.Elapsed;
            Progress.TotalCurrent++;
            InstallFinishedEventArgs.ExitCodes++;
            Logging.WriteToLog(string.Format("Cleaning of mods foldres, current install time = {0} msec",
                InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.CleanInstallation)
            {

            }
            else
                Logging.WriteToLog("...skipped");

            //step 5: extract mods
            OldTime = InstallStopWatch.Elapsed;
            Progress.TotalCurrent++;
            InstallFinishedEventArgs.ExitCodes++;
            Logging.WriteToLog(string.Format("Cleaning of mods foldres, current install time = {0} msec",
                InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.CleanInstallation)
            {

            }
            else
                Logging.WriteToLog("...skipped");

            //step 6: patch files (async option)
            OldTime = InstallStopWatch.Elapsed;
            Progress.TotalCurrent++;
            InstallFinishedEventArgs.ExitCodes++;
            Logging.WriteToLog(string.Format("Cleaning of mods foldres, current install time = {0} msec",
                InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.CleanInstallation)
            {

            }
            else
                Logging.WriteToLog("...skipped");

            //step 7: create create shortcuts (async option)
            OldTime = InstallStopWatch.Elapsed;
            Progress.TotalCurrent++;
            InstallFinishedEventArgs.ExitCodes++;
            Logging.WriteToLog(string.Format("Cleaning of mods foldres, current install time = {0} msec",
                InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.CleanInstallation)
            {

            }
            else
                Logging.WriteToLog("...skipped");

            //step 8: create atlasas (async option)
            OldTime = InstallStopWatch.Elapsed;
            Progress.TotalCurrent++;
            InstallFinishedEventArgs.ExitCodes++;
            Logging.WriteToLog(string.Format("Cleaning of mods foldres, current install time = {0} msec",
                InstallStopWatch.Elapsed.TotalMilliseconds));
            if (ModpackSettings.CleanInstallation)
            {

            }
            else
                Logging.WriteToLog("...skipped");

            //barrier goes here to make sure cleanup is the last thing to do

            //step 9: cleanup (whatever that implies lol)

            //and i guess we're done
        }

        private async Task<bool> BackupModsAsync()
        {

            return true;
        }

        private async Task<bool> ClearCacheAsync()
        {

            return true;
        }

        private async Task<bool> ClearLogsAsync()
        {

            return true;
        }

        private async Task<bool> ClearModsFolders()
        {

            return true;
        }

        private async Task<bool> ExtractFilesAsync()
        {

            return true;
        }

        private async Task<bool> PatchFilesAsync()
        {

            return true;
        }

        private async Task<bool> CreateShortcutsAsync()
        {

            return true;
        }

        private async Task<bool> CreateAtlasesAsync()
        {

            return true;
        }

        private bool Cleanup()
        {

            return true;
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
