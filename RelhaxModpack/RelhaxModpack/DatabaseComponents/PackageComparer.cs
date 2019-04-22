using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    //comparing by package name
    public class PackageComparerByPackageName : IEqualityComparer<DatabasePackage>
    {

        public bool Equals(DatabasePackage x, DatabasePackage y)
        {
            if (string.IsNullOrWhiteSpace(x.PackageName) && string.IsNullOrWhiteSpace(y.PackageName))
                return true;
            return x.PackageName.Equals(y.PackageName);
        }

        public int GetHashCode(DatabasePackage package)
        {
            return package.PackageName.GetHashCode();
        }
    }

    //comparing by name
    public class PackageComparerByName : IEqualityComparer<SelectablePackage>
    {

        public bool Equals(SelectablePackage x, SelectablePackage y)
        {
            if (string.IsNullOrWhiteSpace(x.NameFormatted) && string.IsNullOrWhiteSpace(y.NameFormatted))
                return true;
            return x.NameFormatted.Equals(y.NameFormatted);
        }

        public int GetHashCode(SelectablePackage package)
        {
            return package.NameFormatted.GetHashCode();
        }
    }
}
