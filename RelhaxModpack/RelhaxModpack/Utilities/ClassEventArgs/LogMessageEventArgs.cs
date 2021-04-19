using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.ClassEventArgs
{
    /// <summary>
    /// Event argument class when a log message is sent to a subscribed event
    /// </summary>
    public class LogMessageEventArgs : EventArgs
    {
        /// <summary>
        /// The complete message that was written to text file
        /// </summary>
        public string Message;

        /// <summary>
        /// The log level from the method call
        /// </summary>
        public LogLevel LogLevel;
    }
}
