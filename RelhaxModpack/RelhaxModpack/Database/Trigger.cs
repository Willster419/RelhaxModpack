using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Represents a trigger object used in the installer as an event starter.
    /// For example, a contour icon trigger exists to start the building of contour icons.
    /// </summary>
    public class Trigger
    {
        /// <summary>
        /// Create an instance of a Trigger object.
        /// </summary>
        public Trigger() : base()
        {

        }

        /// <summary>
        /// Create an instance of a trigger object, copying from a given trigger.
        /// </summary>
        /// <param name="triggerToCopy">The trigger to copy.</param>
        public Trigger(Trigger triggerToCopy)
        {
            this.Name = triggerToCopy.Name;
        }

        /// <summary>
        /// The name of the trigger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total number of instances that this trigger exists in the selected packages to install.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// The number of processed triggers for this trigger type. Prevents the trigger from firing early.
        /// </summary>
        public int NumberProcessed { get; set; }

        /// <summary>
        /// Flag to determine if the trigger task has started.
        /// </summary>
        public bool Fired { get; set; }
    }
}
