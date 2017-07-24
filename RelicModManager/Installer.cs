using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

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

        //properties relevent to the handler and install
        private BackgroundWorker InstallWorker;
        private InstallerEventArgs args;

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
            
        }

        //Start the installation on the Wokrer thread
        public void ActuallyStartInstallation(object sender, DoWorkEventArgs e)
        {

        }

        public void WorkerReportProgress(object sender, ProgressChangedEventArgs e)
        {

        }

        public void WorkerReportComplete(object sender, AsyncCompletedEventArgs e)
        {

        }
    }
}
