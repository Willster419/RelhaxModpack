using RelhaxModpack.DatabaseComponents;
using System.Collections.Generic;
using System;

namespace RelhaxModpack
{
    //a dependency is a zip file like mod that is required for any of the mods to work
    //i.e. and sound mods require the sound memory to be increased
    public class Dependency : DatabasePackage
    {
        //list of linked mods and configs that use this dependency at install time
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();
        //list of dependnecies this dependency calls on
        public List<DatabaseLogic> Dependencies = new List<DatabaseLogic>();
    }
}
