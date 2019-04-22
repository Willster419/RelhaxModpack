using System.Collections.Generic;
using System.Linq;
using System;

namespace RelhaxModpack
{
    //a dependency is a zip file like mod that is required for any of the mods to work
    //i.e. and sound mods require the sound memory to be increased
    public class Dependency : DatabasePackage
    {
        #region XML parsing

        private static readonly List<string> DependencyElementsToXmlParseNodes = new List<string>()
        {
            nameof(Dependencies)
        };

        new public static List<string> FieldsToXmlParseAttributes()
        {
            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/versioning-with-the-override-and-new-keywords
            return DatabasePackage.FieldsToXmlParseAttributes();
        }

        new public static List<string> FieldsToXmlParseNodes()
        {
            return DatabasePackage.FieldsToXmlParseNodes().Concat(DependencyElementsToXmlParseNodes).ToList();
        }
        #endregion

        #region Database Properties

        //list of linked mods and configs that use this dependency at install time
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();

        //list of dependnecies this dependency calls on
        public List<DatabaseLogic> Dependencies = new List<DatabaseLogic>();

        //legacy compatibility feature: set this for when loading from legacy database type and is was of type "logicalDependency"
        public bool wasLogicalDependencyLegacy = false;
        
        public Dependency()
        {
            //https://stackoverflow.com/questions/326223/overriding-fields-or-properties-in-subclasses
            //the custom constructor will be called after the base one
            InstallGroup = 2;
        }
        #endregion
    }
}
