using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Provides an interface for Categories and packages to share commonality since they both can have dependencies.
    /// </summary>
    public interface IComponentWithDependencies : IDatabaseComponent
    {
        /// <summary>
        /// The property wrapper of the Dependencies field.
        /// </summary>
        List<DatabaseLogic> Dependencies { get; set; }
    }
}
