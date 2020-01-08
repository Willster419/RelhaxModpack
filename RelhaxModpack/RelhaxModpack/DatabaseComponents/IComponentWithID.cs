using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.DatabaseComponents
{
    public interface IComponentWithID
    {
        /// <summary>
        /// When a databasePackage, the internal packageName. When category, the category name
        /// </summary>
        string ComponentInternalName { get; }
    }
}
