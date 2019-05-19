using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.DatabaseComponents
{
    public interface IDatabaseLogic
    {
        List<DatabaseLogic> DependenciesProp { get; set; }
    }
}
