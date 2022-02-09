using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.Structs
{
    public struct VersionInfos
    {
        /// <summary>
        /// The WoT client version e.g. 1.5.1.3
        /// </summary>
        public string WoTClientVersion;

        /// <summary>
        /// The online folder number (major game version) that contains the game zip files
        /// </summary>
        public string WoTOnlineFolderVersion;

        /// <summary>
        /// Overrides the ToString() function to display the two properties
        /// </summary>
        /// <returns>Displays the WoTClientVersion and WoTOnlineFolderVersion</returns>
        public override string ToString()
        {
            return string.Format("WoTClientVersion={0}, WoTOnlineFolderVersion={1}", WoTClientVersion, WoTOnlineFolderVersion);
        }
    }
}
