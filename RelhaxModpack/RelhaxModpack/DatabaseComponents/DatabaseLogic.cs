using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    /// <summary>
    /// An enumerated representation of "and" and "or" logic
    /// </summary>
    public enum Logic
    {
        /// <summary>
        /// A logical AND
        /// </summary>
        AND = 1,

        /// <summary>
        /// A logical OR
        /// </summary>
        OR = 0
    }

    /// <summary>
    /// Used for database dependency calculation. Determines what dependent packages use the package that this object is attached to
    /// </summary>
    public class DatabaseLogic
    {
        /// <summary>
        /// The name of the package that this object is attached to
        /// </summary>
        public string PackageName = "";

        //public bool Enabled { get; set; } = false;

        /// <summary>
        /// Flag to determine if this package will be installed
        /// </summary>
        public bool willBeInstalled { get; set; } = false;

        /// <summary>
        /// Flag for negating the "AND" and "OR" logic (Creates "NAND" and "NOR") of how to install the attach object
        /// </summary>
        public bool NotFlag = false;

        /// <summary>
        /// The logic type to use for this package definition
        /// </summary>
        public Logic Logic = Logic.OR;

        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>The name of the package this object attaches to</returns>
        public override string ToString()
        {
            return PackageName;
        }
    }
}
