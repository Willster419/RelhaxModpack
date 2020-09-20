using RelhaxModpack;
using RelhaxModpack.Database;
using RelhaxModpack.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxInstallerUnitTester
{
    public static class InstallerUnitTestHelper
    {
        public static ModSelectionList InstallerHelperList = null;
        public static Logfile InstallerHelperLog = null;
        public static List<DatabasePackage> GlobalDependencies = null;
        public static List<Dependency> Dependencies = null;
        public static List<Category> ParsedCategoryList = null;
    }
}
