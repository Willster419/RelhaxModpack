using System;

namespace RelhaxModpack
{
    //the class that has all the required event args for during an installation
    public class InstallerEventArgs : EventArgs
    {
        //the current "status" of the modpack installer
        public enum InstallProgress
        {
            Idle = 0,
            BackupMods = 1,
            BackupUserData = 2,
            DeleteMods = 3,
            DeleteWoTCache = 4,
            ExtractGlobalDependencies = 5,
            ExtractDependencies = 6,
            ExtractLogicalDependencies = 7,
            ExtractMods = 8,
            ExtractConfigs = 9,
            ExtractAppendedDependencies = 10,
            RestoreUserData = 11,
            UnpackXmlFiles = 12,
            PatchMods = 13,//no longer used
            InstallFonts = 14,//no longer used
            ExtractUserMods = 15,
            PatchUserMods = 16,
            ExtractAtlases = 17,
            CreateAtlases = 18,//no longer used
            InstallUserFonts = 19,
            CreateShortCuts = 20,
            CheckDatabase = 21,
            CleanUp = 22,
            Done = 23,
            //kept after done because it's not part of the install process
            Uninstall = 24,
            UninstallDone = 25
        };
        public InstallProgress InstalProgress { get; set; }
        //the total parrent processed items
        public int ParrentProcessed;
        //the total parrent items to process
        public int ParrentTotalToProcess;
        //the current number of mods/configs/userDatas/etc. processed so far
        public int ChildProcessed;
        //the total number of mods/configs/userDatas to precess
        public int ChildTotalToProcess;
        //the current file being processed
        public string currentFile;
        //the current sub file being processed
        public string currentSubFile;
        //the size of the current file being processed
        public float currentFileSizeProcessed;
    }
}
