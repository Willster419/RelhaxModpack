using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class PackageComparer : IEqualityComparer<DatabasePackage>
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
}
