using System.Collections.Generic;
using System.Linq;
using System;
using RelhaxModpack.DatabaseComponents;

namespace RelhaxModpack
{
    /// <summary>
    /// Represents a package with logical calculations. A dependency is only installed when a selectable package is checked
    /// for installation and is dependent on the dependency i.e. 6th sense sound and icon mods require the 6th sense script dependency
    /// </summary>
    public class Dependency : DatabasePackage, IComponentWithDependencies
    {
        #region XML parsing

        private static readonly List<string> DependencyElementsToXmlParseNodes = new List<string>()
        {
            nameof(Dependencies)
        };

        /// <summary>
        /// Gets a list of fields (including from base classes) that can be parsed as xml attributes
        /// </summary>
        /// <returns>The string list</returns>
        new public static List<string> FieldsToXmlParseAttributes()
        {
            //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/versioning-with-the-override-and-new-keywords
            return DatabasePackage.FieldsToXmlParseAttributes();
        }

        /// <summary>
        /// Gets a list of fields (including from base classes) that can be parsed as xml elements
        /// </summary>
        /// <returns>The string list</returns>
        new public static List<string> FieldsToXmlParseNodes()
        {
            return DatabasePackage.FieldsToXmlParseNodes().Concat(DependencyElementsToXmlParseNodes).ToList();
        }
        #endregion

        #region Database Properties

        /// <summary>
        /// List of linked mods and configs that use this dependency at install time
        /// </summary>
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();

        /// <summary>
        /// List of dependencies this dependency calls on
        /// </summary>
        public List<DatabaseLogic> Dependencies = new List<DatabaseLogic>();

        /// <summary>
        /// Property of Dependencies list to allow for interface implementation
        /// </summary>
        public List<DatabaseLogic> DependenciesProp { get { return Dependencies; } set { Dependencies = value; } }

        /// <summary>
        /// When loading from legacy database type and is was of type "logicalDependency"
        /// </summary>
        [Obsolete("This is for legacy database compatibility and will be ignored in Relhax V2")]
        public bool wasLogicalDependencyLegacy = false;
        
        /// <summary>
        /// Constructor to over-ride DatabasePackage default values
        /// </summary>
        public Dependency()
        {
            //https://stackoverflow.com/questions/326223/overriding-fields-or-properties-in-subclasses
            //the custom constructor will be called after the base one
            InstallGroup = 2;
            PatchGroup = 2;
        }
        #endregion

        #region Other Properties and Methods

        /// <summary>
        /// Create a copy of the Dependency object
        /// </summary>
        /// <param name="dependencyToCopy">The object to copy</param>
        /// <returns>A new Dependency object with the same values</returns>
        public static Dependency Copy(Dependency dependencyToCopy)
        {
            if (dependencyToCopy == null)
                return null;

            Dependency dep = (Dependency)Copy(dependencyToCopy);
#pragma warning disable CS0618 // Type or member is obsolete
            dep.wasLogicalDependencyLegacy = dependencyToCopy.wasLogicalDependencyLegacy;

            dep.DatabasePackageLogic = new List<DatabaseLogic>();
            dep.Dependencies = new List<DatabaseLogic>();

            return dep;
        }

        /// <summary>
        /// Create a copy of the Dependency object
        /// </summary>
        /// <param name="dependencyToCopy">The object to copy</param>
        /// <returns>A new Dependency object with the same values and new list elements with the same values</returns>
        public static Dependency DeepCopy(Dependency dependencyToCopy)
        {
            if (dependencyToCopy == null)
                return null;

            Dependency dep = (Dependency)DatabasePackage.DeepCopy(dependencyToCopy);
            dep.wasLogicalDependencyLegacy = dependencyToCopy.wasLogicalDependencyLegacy;
#pragma warning restore CS0618 // Type or member is obsolete
            dep.DatabasePackageLogic = new List<DatabaseLogic>();
            dep.Dependencies = new List<DatabaseLogic>();

            foreach (DatabaseLogic logic in dependencyToCopy.Dependencies)
                dep.Dependencies.Add(DatabaseLogic.Copy(logic));

            return dep;

        }
        #endregion
    }
}
