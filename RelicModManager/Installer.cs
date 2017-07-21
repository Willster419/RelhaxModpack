using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace RelhaxModpack
{
    public static class Installer
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
        private static string TanksLocation { get; set; }
        private static string AppPath { get; set; }
        private static List<Dependency> GlobalDependencies { get; set; }
        private static List<Dependency> Dependencies { get; set; }
        private static List<Dependency> LogicalDependencies { get; set; }
        private static List<Mod> ModsToInstall { get; set; }
        private static List<List<Config>> ConfigListsToInstall { get; set; }
        private static List<Mod> ModsWithData { get; set; }
        private static List<Config> ConfigsWithData { get; set; }

        //properties relevent to the handler and install
        public static BackgroundWorker InstallWorker = new BackgroundWorker();
        public enum InstallerState
        {
            Error = -1,
            Idle = 0

        }
        public static InstallerState State = InstallerState.Idle;
        public static int ParentCurrentProgress;
        public static int ParentMaxProgress;
        public static int ChildCurrentProgress;
        public static int ChildMaxProgress;

        public static void StartInstallation()
        {
            //reset everything (todo: actually figure out what to reset)
        }

        public static void ResetInstaller()
        {
            //reset everything (todo: actually figure out what to reset)

        }
    }
}
