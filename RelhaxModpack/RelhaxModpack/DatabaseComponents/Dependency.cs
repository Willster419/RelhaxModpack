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
        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/versioning-with-the-override-and-new-keywords

        public Dependency()
        {
            //https://stackoverflow.com/questions/326223/overriding-fields-or-properties-in-subclasses
            InstallGroup = 2;
        }
    }
}
