using System.Collections.Generic;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Database
{
    public class UpdateInstructions
    {
        public string InstructionsVersion { get; set; }

        public UpdateTypes UpdateType { get; set; }
        public string WotmodFilenameInZip { get; set; }

        public string WotmodOldFilenameInZip { get; set; }
        public string WotmodDownloadedMD5 { get; set; }
        public string WotmodDatabaseMD5 { get; set; }

        private List<PatchUpdate> patchUpdates = new List<PatchUpdate>();
        public List<PatchUpdate> PatchUpdates
        {
            get { return patchUpdates; }
            set
            {
                patchUpdates.Clear();
                patchUpdates.AddRange(value);
            }
        }
    }
}
