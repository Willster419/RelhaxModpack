using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Enables comparison of Packages by their PackageName property.
    /// </summary>
    public class PackageComparerByPackageName : IEqualityComparer<DatabasePackage>
    {
        /// <summary>
        /// Determines if PackageName of package x is before or after PackageName of Package y.
        /// </summary>
        /// <param name="x">The first package.</param>
        /// <param name="y">The second package.</param>
        /// <returns>1,0,-1 from string PackageName comparison.</returns>
        public bool Equals(DatabasePackage x, DatabasePackage y)
        {
            if (string.IsNullOrWhiteSpace(x.PackageName) && string.IsNullOrWhiteSpace(y.PackageName))
                return true;
            return x.PackageName.Equals(y.PackageName);
        }

        /// <summary>
        /// Returns the hash code of the PackageName of the Package.
        /// </summary>
        /// <param name="package">The package to get the hash code.</param>
        /// <returns>The hash code of the PackageName of the Package.</returns>
        public int GetHashCode(DatabasePackage package)
        {
            return package.PackageName.GetHashCode();
        }
    }

    /// <summary>
    /// Enables comparison of Packages by their Name property
    /// </summary>
    public class PackageComparerByDisplayName : IEqualityComparer<SelectablePackage>
    {
        /// <summary>
        /// Determines if Name of package x is before or after Name of Package y.
        /// </summary>
        /// <param name="x">The first package.</param>
        /// <param name="y">The second package.</param>
        /// <returns>1,0,-1 from string Name comparison.</returns>
        public bool Equals(SelectablePackage x, SelectablePackage y)
        {
            if (string.IsNullOrWhiteSpace(x.NameFormatted) && string.IsNullOrWhiteSpace(y.NameFormatted))
                return true;
            return x.NameFormatted.Equals(y.NameFormatted);
        }

        /// <summary>
        /// Returns the hash code of the Name of the Package.
        /// </summary>
        /// <param name="package">The package to get the hash code.</param>
        /// <returns>The hash code of the Name of the Package.</returns>
        public int GetHashCode(SelectablePackage package)
        {
            return package.NameFormatted.GetHashCode();
        }
    }

    /// <summary>
    /// Enables comparison of Packages by their Unique ID (UID) property.
    /// </summary>
    public class PackageComparerByUID : IEqualityComparer<DatabasePackage>
    {
        /// <summary>
        /// Determines if PackageName of package x is before or after PackageName of Package y.
        /// </summary>
        /// <param name="x">The first package.</param>
        /// <param name="y">The second package.</param>
        /// <returns>1,0,-1 from string PackageName comparison.</returns>
        public bool Equals(DatabasePackage x, DatabasePackage y)
        {
            if (string.IsNullOrWhiteSpace(x.UID) && string.IsNullOrWhiteSpace(y.UID))
                return true;
            return x.UID.Equals(y.UID);
        }

        /// <summary>
        /// Returns the hash code of the PackageName of the Package.
        /// </summary>
        /// <param name="package">The package to get the hash code.</param>
        /// <returns>The hash code of the PackageName of the Package.</returns>
        public int GetHashCode(DatabasePackage package)
        {
            return package.UID.GetHashCode();
        }
    }
}
