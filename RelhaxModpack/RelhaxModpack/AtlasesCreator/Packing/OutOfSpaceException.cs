using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Atlases.Packing
{
    /// <summary>Insufficient space left in packing area to contain a given object</summary>
    /// <remarks>
    ///   An exception being sent to you from deep space. Erm, no, wait, it's an exception
    ///   that occurs when a packing algorithm runs out of space and is unable to fit
    ///   the object you tried to pack into the remaining packing area.
    /// </remarks>
    [Serializable]
    internal class OutOfSpaceException : Exception
    {
        /// <summary>Initializes the exception with an error message</summary>
        /// <param name="message">Error message describing the cause of the exception</param>
        public OutOfSpaceException(string message)
            : base(message)
        {
        }
    }
}
