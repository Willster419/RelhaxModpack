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
        #endregion

        #region Other Properties and Methods
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

        public Dependency(DatabasePackage packageToCopyFrom, bool deep) : base(packageToCopyFrom, deep)
        {
            InstallGroup = 2;
            PatchGroup = 2;
            if (packageToCopyFrom is Dependency dep)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                this.wasLogicalDependencyLegacy = dep.wasLogicalDependencyLegacy;
                this.DatabasePackageLogic = new List<DatabaseLogic>();
                this.Dependencies = new List<DatabaseLogic>();
#pragma warning restore CS0618 // Type or member is obsolete

                if (deep)
                {
                    foreach (DatabaseLogic logic in dep.Dependencies)
                        this.Dependencies.Add(DatabaseLogic.Copy(logic));
                }
            }
            else if (packageToCopyFrom is SelectablePackage sp)
            {
                this.Dependencies = new List<DatabaseLogic>();
                if(deep)
                {
                    foreach (DatabaseLogic logic in sp.Dependencies)
                        this.Dependencies.Add(DatabaseLogic.Copy(logic));
                }
            }
        }
        #endregion
    }
}
