using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Patching
{
    /// <summary>
    /// Represents a patch operation with a description and desired assertion condition
    /// </summary>
    public class PatchUnitTest
    {
        /// <summary>
        /// The patch operation object
        /// </summary>
        public Patch Patch;

        /// <summary>
        /// A description of what the patch should do
        /// </summary>
        public string Description;

        /// <summary>
        /// Determines if the patch should pass or fail the test
        /// </summary>
        public bool ShouldPass;
    }
}
