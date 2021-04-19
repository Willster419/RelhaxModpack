using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Common
{
    /// <summary>
    /// An interface to allow a unique identifier for the given component. Used mostly for identifying a broken or incorrectly parsed component from a log statement
    /// </summary>
    public interface IComponentWithID
    {
        /// <summary>
        /// The internal ID of the component. Can be anything used to identify it.
        /// </summary>
        /// <remarks>When a databasePackage, the internal packageName. When category, the category name.</remarks>
        string ComponentInternalName { get; }
    }
}
