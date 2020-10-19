using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Database
{
    public class DownloadInstructions
    {
        public string InstructionsVersion { get; set; }

        public string ModVersion { get; set; }
        public string ClientVersion { get; set; }

        public DownloadTypes DownloadType { get; set; }
        public string UpdateURL { get; set; }
        public string DownloadFilename { get; set; }

        public string DownloadedFileLocation { get; set; }
        public string DownloadedDatabaseZipFileLocation { get; set; }
    }
}
