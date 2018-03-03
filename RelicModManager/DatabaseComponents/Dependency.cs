using RelhaxModpack.DatabaseComponents;
using System.Collections.Generic;
using System;

namespace RelhaxModpack
{
    //a dependency is a zip file like mod that is required for any of the mods to work
    //i.e. and sound mods require the sound memory to be increased
    public class Dependency : DatabasePackage
    {
        public List<LogicalDependency> LogicalDependencies = new List<LogicalDependency>();
        public Dependency() {}
        public override string ToString()
        {
            return PackageName;
        }
    }
}
