using System;
using System.ComponentModel;
using System.Collections.Generic;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// A PackageTagsList class is a child class of List of type PackageTags enumeration. Allows for string representation for database serialization.
    /// </summary>
    [TypeConverter(typeof(PackageTagsListConverter))]
    public class PackageTagsList : List<PackageTags>
    {
        /// <summary>
        /// Creates a PackageTagsList object.
        /// </summary>
        public PackageTagsList() { }

        /// <summary>
        /// Creates a PackageTagsList object with the given list of type PackageTags.
        /// </summary>
        /// <param name="packageTags">The list with elements to add from.</param>
        public PackageTagsList(List<PackageTags> packageTags)
        {
            this.AddRange(packageTags);
        }

        /// <summary>
        /// Create a string representation of the enumerations from the list.
        /// </summary>
        /// <returns>A comma separated list of the string names of the enumerations.</returns>
        public override string ToString()
        {
            return string.Join(",", this);
        }
    }
}