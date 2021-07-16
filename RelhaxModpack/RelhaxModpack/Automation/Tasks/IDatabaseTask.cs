using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    public interface IDatabaseTask
    {
        string CustomDatabasePath { get; set; }
    }
}
