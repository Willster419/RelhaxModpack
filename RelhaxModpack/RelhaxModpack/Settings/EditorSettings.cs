using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{
    public enum ApplyBehavior
    {
        Default,
        ApplyTriggersSave,
        SaveTriggersApply
    }

    public class EditorSettings
    {
        public string BigmodsUsername = string.Empty;

        public string BigmodsPassword = string.Empty;

        public bool SaveSelectionBeforeLeave = false;

        public bool SortDatabaseList = false;

        public ApplyBehavior ApplyBehavior = ApplyBehavior.Default;

        public bool ShowConfirmationOnPackageApply = true;

        public bool ShowConfirmationOnPackageAddRemoveMove = true;

        public string DefaultEditorSaveLocation = string.Empty;

        public uint FTPUploadDownloadWindowTimeout = 0;

        public EditorSettings() { }
    }
}
