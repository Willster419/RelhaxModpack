using RelhaxModpack.DatabaseComponents;
using System.Collections.Generic;
using System;

namespace RelhaxModpack
{
    public enum Logic
    {
        AND = 1,
        OR = 0
    }
    //a dependency is a zip file like mod that is required for any of the mods to work
    //i.e. and sound mods require the sound memory to be increased
    public class Dependency : DatabasePackage
    {
        //list of linked mods and configs that use 
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();
    }
}
