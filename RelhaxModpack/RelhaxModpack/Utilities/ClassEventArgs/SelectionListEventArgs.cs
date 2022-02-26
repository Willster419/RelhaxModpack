using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.ClassEventArgs
{
    /// <summary>
    /// Event arguments for when the selection list is closed
    /// </summary>
    /// <remarks>See https://stackoverflow.com/questions/623451/how-can-i-make-my-own-event-in-c </remarks>
    public class SelectionListEventArgs : EventArgs
    {
        /// <summary>
        /// If the installation should be continued or if the user canceled
        /// </summary>
        public bool ContinueInstallation = false;

        /// <summary>
        /// The database manager object that was used for loading the selection list.
        /// </summary>
        public DatabaseManager DatabaseManager;

        /// <summary>
        /// The list of use mods
        /// </summary>
        public List<SelectablePackage> UserMods;

        /// <summary>
        /// Flag to determine if the current installation is started from auto install mode
        /// </summary>
        public bool IsAutoInstall = false;

        /// <summary>
        /// Flag to determine if the current installation loaded with selection file
        /// format V3+ is out of date with what the database has
        /// </summary>
        public bool IsSelectionOutOfDate = false;

        /// <summary>
        /// The online FTP folder name that is used for downloading packages for this version of the database.
        /// </summary>
        public string WoTModpackOnlineFolderFromDB = string.Empty;
    }
}
