using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// Represents a class that is used within windows as a serialized to/from xml settings definition holder
    /// </summary>
    public interface ISettingsFile
    {
        /// <summary>
        /// The name of the file on disk
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// A list of properties and fields to exclude from saving/loading to and from xml
        /// </summary>
        string[] MembersToExclude { get; }
    }
}
