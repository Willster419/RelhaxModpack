using RelhaxModpack.DatabaseComponents;
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
    public class DatabaseLogic : IXmlSerializable
    {
        #region Xml serialization
        public string[] PropertiesForSerializationAttributes()
        {
            return new string[] { nameof(PackageName), nameof(NotFlag), nameof(Logic) };
        }

        public string[] PropertiesForSerializationElements()
        {
            return new string[] { };
        }
        #endregion

        /// <summary>
        /// The name of the package that this object is attached to
        /// </summary>
        public string PackageName { get; set; } = "";

        //public bool Enabled { get; set; } = false;

        /// <summary>
        /// Flag to determine if this package will be installed
        /// </summary>
        public bool WillBeInstalled { get; set; } = false;

        /// <summary>
        /// Flag for negating the "AND" and "OR" logic (Creates "NAND" and "NOR") of how to install the attach object
        /// </summary>
        public bool NotFlag { get; set; } = false;

        /// <summary>
        /// The logic type to use for this package definition
        /// </summary>
        public Logic Logic { get; set; } = Logic.OR;

        /// <summary>
        /// A flag for dependency calculation for when the application has linked the dependnecy refrence
        /// </summary>
        /// <remarks>During dependnecy calcuation, the application will 'link' the refrenced dependencies in a package
        /// to the refrenced dependency. This allows for the application to process dependency calcuation logic in a dynamic
        /// AND and OR system. Having the flag can help to determine if a refrence does not exist</remarks>
        public bool RefrenceLinked { get; set; } = false;

        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>The name of the package this object attaches to</returns>
        public override string ToString()
        {
            return PackageName;
        }

        /// <summary>
        /// Create a copy of the given DatabaseLogic object
        /// </summary>
        /// <param name="databaseLogicToCopy">The object to copy</param>
        /// <returns>A new DatabaseLogic object with the same values</returns>
        public static DatabaseLogic Copy(DatabaseLogic databaseLogicToCopy)
        {
            return new DatabaseLogic()
            {
                Logic = databaseLogicToCopy.Logic,
                NotFlag = databaseLogicToCopy.NotFlag,
                PackageName = databaseLogicToCopy.PackageName,
                WillBeInstalled = databaseLogicToCopy.WillBeInstalled
            };
        }
    }
}
