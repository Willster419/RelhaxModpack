using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.DatabaseComponents
{
    public enum UpdateTypes
    {
        zip,
        wotmod
    }
    public class UpdateInstructions
    {
        public string InstructionsVersion { get; set; }
        public UpdateTypes UpdateType { get; set; }
        public string WotmodFilenameInZip { get; set; }
        public string WotmodMD5 { get; set; }
    }
}
