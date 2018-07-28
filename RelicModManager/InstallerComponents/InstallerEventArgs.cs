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
            BackupUserData = 2,
            DeleteMods = 3,
            DeleteWoTCache = 4,
            RestoreUserDataBefore = 5,
            ExtractGlobalDependencies = 6,
            ExtractDependencies = 7,
            ExtractLogicalDependencies = 8,
            ExtractMods = 9,
            ExtractConfigs = 10,
            ExtractAppendedDependencies = 11,
            RestoreUserData = 12,
            UnpackXmlFiles = 13,
            PatchMods = 14,//no longer used
            InstallFonts = 15,//no longer used
            ExtractUserMods = 16,
            PatchUserMods = 17,
            ExtractAtlases = 18,
            CreateAtlases = 19,//no longer used
            InstallUserFonts = 20,
            CreateShortcuts = 21,
            CheckDatabase = 22,
            CleanUp = 23,
            Done = 24,
            //kept after done because it's not part of the install process
            Uninstall = 25,
            UninstallDone = 26
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
