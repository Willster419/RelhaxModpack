using System;

namespace RelhaxModpack
{
    //the class that has all the required event args for during an installation
    public class InstallerEventArgs : EventArgs
    {
        //the current "status" of the modpack installer
        public enum InstallProgress
        {
            Error = -1,
            Idle = 0,
            BackupMods = 1,
            BackupSystemData = 2,
            BackupUserData = 3,
            DeleteMods = 4,
            DeleteWoTCache = 5,
            RestoreSystemDataBefore = 6,
            RestoreUserDataBefore = 7,
            ExtractGlobalDependencies = 8,
            ExtractDependencies = 9,
            ExtractLogicalDependencies = 10,
            ExtractMods = 11,
            ExtractConfigs = 12,
            ExtractAppendedDependencies = 13,
            RestoreSystemData = 14,
            RestoreUserData = 15,
            UnpackXmlFiles = 16,
            PatchMods = 17,//no longer used
            InstallFonts = 18,//no longer used
            ExtractUserMods = 19,
            PatchUserMods = 20,
            ExtractAtlases = 21,
            CreateAtlases = 22,//no longer used
            InstallUserFonts = 23,
            CreateShortcuts = 24,
            CheckDatabase = 25,
            CleanUp = 26,
            Done = 27,
            //kept after done because it's not part of the install process
            Uninstall = 28,
            UninstallDone = 29,
            BackupDelete,
            BackupDeleteDone
            
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
        //the filecounter
        // public int Filecounter;
        //files to do
        // public int FilesToDo;
    }
}
