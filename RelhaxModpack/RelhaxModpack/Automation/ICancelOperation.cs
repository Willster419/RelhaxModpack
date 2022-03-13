using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// Provides a method to cancel a running automation task via the Cancel method.
    /// </summary>
    public interface ICancelOperation
    {
        /// <summary>
        /// Cancel the running automation task.
        /// </summary>
        void Cancel();
    }
}
