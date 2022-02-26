using RelhaxModpack.Database;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Allows for additional data for reporting when the DownloadManager reports download progress.
    /// </summary>
    public class RelhaxDownloadProgress : RelhaxProgress
    {
        /// <summary>
        /// The package being downloaded.
        /// </summary>
        public DatabasePackage DatabasePackage = null;

        /// <summary>
        /// The current state of the download operation.
        /// </summary>
        public DownloadProgressState DownloadProgressState = DownloadProgressState.None;
    }
}
