using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{
    /// <summary>
    /// Represents a trigger object used in the installer as an event starter.
    /// For example, a contour icon trigger exists to start the building of contour icons
    /// </summary>
    public class Trigger
    {

        public string Name { get; set; }

        public int Total { get; set; }

        public int NumberProcessed { get; set; }

        public bool Fired { get; set; }

        public Task TriggerTask { get; set; }
    }
}
