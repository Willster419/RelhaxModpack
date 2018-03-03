using RelhaxModpack.DatabaseComponents;
using System.Collections.Generic;

namespace RelhaxModpack
{
    public class LogicalDependency : DatabasePackage
    {
        //list of linked mods and configs that use 
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();
        //acts as a NOT flag
        public bool NegateFlag;
        public LogicalDependency() {}
        //for the tostring thing
        public override string ToString()
        {
            return NegateFlag? "(Not) " + PackageName : "" + PackageName;
        }
    }
}
