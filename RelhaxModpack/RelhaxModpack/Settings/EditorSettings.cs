using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{
    /// <summary>
    /// The settings used in the editor window
    /// </summary>
    public class EditorSettings
    {
        /// <summary>
        /// The user's FTP account username to the bigmods FTP server
        /// </summary>
        public string BigmodsUsername = string.Empty;

        /// <summary>
        /// The user's FTP account password to the bigmods FTP server
        /// </summary>
        public string BigmodsPassword = string.Empty;

        /// <summary>
        /// Before you click on a new selection to display, it will apply any changes made. Can be used with ApplyBehavior.
        /// </summary>
        public bool SaveSelectionBeforeLeave = false;

        /// <summary>
        /// The behavior the editor should use for the save and apply buttons
        /// </summary>
        public ApplyBehavior ApplyBehavior = ApplyBehavior.Default;

        /// <summary>
        /// Show a confirmation window when clicking apply
        /// </summary>
        public bool ShowConfirmationOnPackageApply = true;

        /// <summary>
        /// Show a confirmation window when clicking to add or move a package
        /// </summary>
        public bool ShowConfirmationOnPackageAddRemoveMove = true;

        /// <summary>
        /// The location to save the database file
        /// </summary>
        public string DefaultEditorSaveLocation = string.Empty;

        /// <summary>
        /// The timeout, in seconds, until the FTP upload or download window will close. Set to 0 to disable timeout.
        /// </summary>
        public uint FTPUploadDownloadWindowTimeout = 0;

        /// <summary>
        /// Flag to determine if the delete zip button (actually open file by default) will actually delete the zip, or move it to the specified folder
        /// </summary>
        public bool UploadZipDeleteIsActuallyMove = false;

        /// <summary>
        /// The folder path to move the uploaded file to. If the file already exists, it will be overridden
        /// </summary>
        public string UploadZipMoveFolder = string.Empty;

        /// <summary>
        /// Create an instance of the EditorSettings class. Settings should be set via property initialization style.
        /// </summary>
        public EditorSettings() { }
    }
}
