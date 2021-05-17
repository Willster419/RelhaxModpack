using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.ClassEventArgs
{
    /// <summary>
    /// Event args returned to the editor for when an FTP upload or download is complete
    /// </summary>
    public class EditorTransferEventArgs : EventArgs
    {
        /// <summary>
        /// The package that was just uploaded
        /// </summary>
        public DatabasePackage Package;

        /// <summary>
        /// The path to the file that was uploaded or downloaded
        /// </summary>
        public string UploadedFilename;

        /// <summary>
        /// The FTP path to the field that was uploaded or downloaded
        /// </summary>
        public string UploadedFilepathOnline;

        /// <summary>
        /// The Transfer mode that was used for the upload or download
        /// </summary>
        public EditorTransferMode TransferMode;
    }
}
