using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.DatabaseComponents
{
    public enum DownloadTypes
    {
        StaticLink,
        WgMods,
        WebScrape
    }
    public class DownloadInstructions
    {
        public string InstructionsVersion { get; set; }
        public string ModVersion { get; set; }
        public string ClientVersion { get; set; }
        public DownloadTypes DownloadType { get; set; }
        public string UpdateURL { get; set; }
        public string DownloadFilename { get; set; }
    }
}
