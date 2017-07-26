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
            ExtractGlobalDependencies = 4,
            ExtractDependencies = 5,
            ExtractLogicalDependencies = 6,
            ExtractMods = 7,
            ExtractConfigs = 8,
            RestoreUserData = 9,
            PatchMods = 10,
            InstallFonts = 11,
            ExtractUserMods = 12,
            PatchUserMods = 13,
            InstallUserFonts = 14,
            Done = 15,
            Uninstall = 16,
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
