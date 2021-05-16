using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// The settings used in the editor window
    /// </summary>
    public class EditorSettings : ISettingsFile
    {
        /// <summary>
        /// The name of the settings file on disk
        /// </summary>
        public const string SettingsFilename = "EditorSettings.xml";

        /// <summary>
        /// The name of the xml file on disk
        /// </summary>
        public string Filename { get { return SettingsFilename; } }

        /// <summary>
        /// A list of properties and fields to exclude from saving/loading to and from xml
        /// </summary>
        public string[] MembersToExclude { get { return new string[] { nameof(MembersToExclude), nameof(Filename), nameof(SettingsFilename) }; } }

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
        /// Flag to determine if after an upload is completed, the file will automatically be deleted or moved to a local folder
        /// </summary>
        public bool DeleteUploadLocallyUponCompletion = false;
    }
}
