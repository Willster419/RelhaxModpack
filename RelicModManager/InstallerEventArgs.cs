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
            PatchMods = 12,
            InstallFonts = 13,
            ExtractUserMods = 14,
            PatchUserMods = 15,
            InstallUserFonts = 16,
            Done = 17,
            //kept after done because it's not part of the uninstall process
            Uninstall = 18,
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
        //the sixe of the current file being processed
        public float currentFileSizeProcessed;
    }
}
