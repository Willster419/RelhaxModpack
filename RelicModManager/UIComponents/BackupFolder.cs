using System;
using System.Collections.Generic;

namespace RelhaxModpack
{
    class BackupFolder
    {
        public string TopfolderName { get; set; } = "";
        public List<string> FullnameList { get; set; }
        public UInt64 FilesSize { get; set; } = 0;
        public UInt64 FilesSizeOnDisk { get; set; } = 0;
        public uint FileCount { get; set; } = 0;
        public uint FolderCount { get; set; } = 0;
    }
}
