using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.DatabaseComponents
{
    /// <summary>
    /// Provides an interface for Categories and packages to share commonality since they both can have dependencies
    /// </summary>
    public interface IComponentWithDependencies
    {
        /// <summary>
        /// The property wrapper of the Dependencies field
        /// </summary>
        List<DatabaseLogic> DependenciesProp { get; set; }

        /// <summary>
        /// When a databasePackage, the internal packageName. When category, the category name
        /// </summary>
        string ComponentInternalName { get; }
    }
}
