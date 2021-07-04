using RelhaxModpack.Utilities;
using RelhaxModpack.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using RelhaxModpack.Utilities.Enums;
using System.Net;
using System.IO;
using System.Xml.XPath;
using RelhaxModpack.Common;

namespace RelhaxModpack.Database
{
    #region Structs
    /// <summary>
    /// Allows the old and new versions of a SelectablePackage to be saved temporarily for comparing differences between two database structures
    /// </summary>
    public struct DatabaseBeforeAfter
    {
        /// <summary>
        /// The package reference for the database before changes
        /// </summary>
        public SelectablePackage Before;

        /// <summary>
        /// The package reference for the database after changes
        /// </summary>
        public SelectablePackage After;
    }

    /// <summary>
    /// Allows the old and new versions of a DatabasePackage to be saved temporarily for comparing differences between two database structures
    /// </summary>
    public struct DatabaseBeforeAfter2
    {
        /// <summary>
        /// The package reference for the database before changes
        /// </summary>
        public DatabasePackage Before;

        /// <summary>
        /// The package reference for the database after changes
        /// </summary>
        public DatabasePackage After;
    }

    /// <summary>
    /// A structure object to contain the WoT client version and online folder version. Allows for LINQ searching
    /// </summary>
    public struct VersionInfos
    {
        /// <summary>
        /// The WoT client version e.g. 1.5.1.3
        /// </summary>
        public string WoTClientVersion;

        /// <summary>
        /// The online folder number (major game version) that contains the game zip files
        /// </summary>
        public string WoTOnlineFolderVersion;

        /// <summary>
        /// Overrides the ToString() function to display the two properties
        /// </summary>
        /// <returns>Displays the WoTClientVersion and WoTOnlineFolderVersion</returns>
        public override string ToString()
        {
            return string.Format("WoTClientVersion={0}, WoTOnlineFolderVersion={1}", WoTClientVersion, WoTOnlineFolderVersion);
        }
    }

    /// <summary>
    /// A structure used to keep a reference of a component and a dependency that it calls
    /// </summary>
    /// <remarks>This is used to determine if any packages call any dependencies who's packageName does not exist in the database</remarks>
    public struct LogicTracking
    {
        /// <summary>
        /// The database component what has dependencies
        /// </summary>
        public IComponentWithDependencies ComponentWithDependencies;

        /// <summary>
        /// The called dependency from the component
        /// </summary>
        public DatabaseLogic DatabaseLogic;
    }
    #endregion

    /// <summary>
    /// A utility class for working with database components
    /// </summary>
    public static class DatabaseUtils
    {
        /// <summary>
        /// Returns a flat list of the given recursive lists, in the order that the parameters are stated
        /// </summary>
        /// <param name="globalDependnecies">The list of global dependences</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <returns>The flat list</returns>
        /// <remarks>In the case of Categories, the flat list has the sub-level packages added at the level of the parent</remarks>
        public static List<DatabasePackage> GetFlatList(List<DatabasePackage> globalDependnecies = null, List<Dependency> dependencies = null, List<Category> parsedCategoryList = null)
        {
            if (globalDependnecies == null && dependencies == null  && parsedCategoryList == null)
                return null;

            List<DatabasePackage> flatList = new List<DatabasePackage>();
            if (globalDependnecies != null)
                flatList.AddRange(globalDependnecies);
            if (dependencies != null)
                flatList.AddRange(dependencies);
            if (parsedCategoryList != null)
                foreach (Category cat in parsedCategoryList)
                    flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
        }

        /// <summary>
        /// Returns a flat list of the given recursive lists, in the order that the parameters are stated
        /// </summary>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <returns>The flat list</returns>
        /// <remarks>In the case of Categories, the flat list has the sub-level packages added at the level of the parent</remarks>
        public static List<SelectablePackage> GetFlatSelectablePackageList(List<Category> parsedCategoryList)
        {
            if (parsedCategoryList == null)
                return null;
            List<SelectablePackage> flatList = new List<SelectablePackage>();
            foreach (Category cat in parsedCategoryList)
                flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
        }

        /// <summary>
        /// Checks for any duplicate UID entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of packages with duplicate UIDs, or an empty list if no duplicates</returns>
        public static List<DatabasePackage> CheckForDuplicateUIDsPackageList(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            List<DatabasePackage> duplicatesList = new List<DatabasePackage>();
            List<DatabasePackage> flatList = GetFlatList(globalDependencies, dependencies, parsedCategoryList);
            foreach (DatabasePackage package in flatList)
            {
                List<DatabasePackage> packagesWithMatchingUID = flatList.FindAll(item => item.UID.Equals(package.UID));
                //by default it will at least match itself
                if (packagesWithMatchingUID.Count > 1)
                    duplicatesList.Add(package);
            }
            return duplicatesList;
        }

        /// <summary>
        /// Checks for any duplicate UID entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of duplicate UIDs, or an empty list if no duplicates</returns>
        public static List<string> CheckForDuplicateUIDsStringsList(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            return CheckForDuplicateUIDsPackageList(globalDependencies, dependencies, parsedCategoryList).Select(package => package.UID).ToList();
        }

        /// <summary>
        /// Checks for any duplicate PackageName entries inside the provided lists
        /// </summary>
        /// <param name="globalDependencies">The list of global dependencies</param>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of categories</param>
        /// <returns>A list of duplicate packages, or an empty list if no duplicates</returns>
        public static List<string> CheckForDuplicates(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            List<string> duplicatesList = new List<string>();
            List<DatabasePackage> flatList = GetFlatList(globalDependencies, dependencies, parsedCategoryList);
            foreach (DatabasePackage package in flatList)
            {
                List<DatabasePackage> packagesWithPackagename = flatList.Where(item => item.PackageName.Equals(package.PackageName)).ToList();
                if (packagesWithPackagename.Count > 1)
                    duplicatesList.Add(package.PackageName);
            }
            return duplicatesList;
        }

        /// <summary>
        /// Checks if a packageName exists within a list of packages
        /// </summary>
        /// <param name="packagesToCheckWith">The list of packages to check inside</param>
        /// <param name="nameToCheck">The PackageName parameter to check</param>
        /// <returns>True if the nameToCheck exists in the list, false otherwise</returns>
        public static bool IsDuplicateName(List<DatabasePackage> packagesToCheckWith, string nameToCheck)
        {
            foreach (DatabasePackage package in packagesToCheckWith)
            {
                if (package.PackageName.Equals(nameToCheck))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sorts the packages inside each Category object
        /// </summary>
        /// <param name="parsedCategoryList">The list of categories to sort</param>
        public static void SortDatabase(List<Category> parsedCategoryList)
        {
            //the first level of packages are always sorted
            foreach (Category cat in parsedCategoryList)
            {
                SortDatabase(cat.Packages);
            }
        }

        /// <summary>
        /// Sorts a list of packages
        /// </summary>
        /// <param name="packages">The list of packages to sort</param>
        /// <param name="recursive">If the list should recursively sort</param>
        private static void SortDatabase(List<SelectablePackage> packages, bool recursive = true)
        {
            //sorts packages in alphabetical order
            packages.Sort(SelectablePackage.CompareModsName);
            if (recursive)
            {
                //if set in the database, child elements can be sorted as well
                foreach (SelectablePackage child in packages)
                {
                    if (child.SortChildPackages)
                    {
                        Logging.Debug("Sorting packages of package {0}", child.PackageName);
                        SortDatabase(child.Packages);
                    }
                }
            }
        }

        /// <summary>
        /// Links all the references (like parent, etc) for each class object making it possible to traverse the list tree in memory
        /// </summary>
        /// <param name="ParsedCategoryList">The List of categories</param>
        public static void BuildLinksRefrence(List<Category> ParsedCategoryList)
        {
            foreach (Category cat in ParsedCategoryList)
            {
                List<SelectablePackage> packagesToItterate;
                if (cat.CategoryHeader != null)
                {
                    packagesToItterate = cat.CategoryHeader.Packages;
                    cat.CategoryHeader.Parent = cat.CategoryHeader;
                    cat.CategoryHeader.TopParent = cat.CategoryHeader;
                }
                else
                {
                    packagesToItterate = cat.Packages;
                }
                foreach (SelectablePackage sp in packagesToItterate)
                {
                    BuildLinksRefrence(sp, cat, cat.CategoryHeader);
                }
            }
        }

        public static void BuildTopLevelParents(List<Category> ParsedCategoryList)
        {
            foreach (Category cat in ParsedCategoryList)
            {
                cat.CategoryHeader = new SelectablePackage()
                {
                    Name = string.Format("----------[{0}]----------", cat.Name),
                    TabIndex = cat.TabPage,
                    ParentCategory = cat,
                    Type = SelectionTypes.multi,
                    Visible = true,
                    Enabled = true,
                    Level = -1,
                    PackageName = string.Format("Category_{0}_Header", cat.Name.Replace(' ', '_')),
                    Packages = cat.Packages
                };
            }
        }

        /// <summary>
        /// Links all the references (like parent, etc) for each class object making it possible to traverse the list tree in memory
        /// </summary>
        /// <param name="sp">The package to perform linking on</param>
        /// <param name="cat">The category that the SelectablePackagesp belongs to</param>
        /// <param name="parent">The tree parent of sp</param>
        private static void BuildLinksRefrence(SelectablePackage sp, Category cat, SelectablePackage parent)
        {
            sp.Parent = parent;
            sp.TopParent = cat.CategoryHeader;
            sp.ParentCategory = cat;
            if (sp.Packages.Count > 0)
            {
                foreach (SelectablePackage sp2 in sp.Packages)
                {
                    BuildLinksRefrence(sp2, cat, sp);
                }
            }
        }

        /// <summary>
        /// Assigns the level parameter to the packages based on how recursively deep they are in the package sub lists
        /// </summary>
        /// <param name="ParsedCategoryList">The list of assign package values to</param>
        /// <param name="startingLevel">The starting level to assign the level parameter</param>
        public static void BuildLevelPerPackage(List<Category> ParsedCategoryList, int startingLevel = 0)
        {
            //root level direct form category is 0
            foreach (Category cat in ParsedCategoryList)
            {
                foreach (SelectablePackage package in cat.Packages)
                {
                    package.Level = startingLevel;
                    if (package.Packages.Count > 0)
                        //increase the level BEFORE it calls the method
                        BuildLevelPerPackage(package.Packages, startingLevel + 1);
                }
            }
        }

        /// <summary>
        /// Assigns the level parameter to the packages based on how recursively deep they are in the package sub lists
        /// </summary>
        /// <param name="packages">The list of package values to</param>
        /// <param name="level">The level to assign the level parameter</param>
        private static void BuildLevelPerPackage(List<SelectablePackage> packages, int level)
        {
            foreach (SelectablePackage package in packages)
            {
                package.Level = level;
                if (package.Packages.Count > 0)
                    //increase the level BEFORE it calls the method
                    BuildLevelPerPackage(package.Packages, level + 1);
            }
        }

        /// <summary>
        /// Links the databasePackage objects with dependencies objects to have those objects link references to the parent and the dependency object
        /// </summary>
        /// <param name="componentsWithDependencies">List of all DatabasePackage objects that have dependencies</param>
        /// <param name="dependencies">List of all Dependencies that exist in the database</param>
        public static void BuildDependencyPackageRefrences(List<Category> componentsWithDependencies, List<Dependency> dependencies)
        {
            List<IComponentWithDependencies> componentsWithDependencies_ = new List<IComponentWithDependencies>();

            //get all categories where at least one dependency exists
            componentsWithDependencies_.AddRange(componentsWithDependencies.Where(cat => cat.Dependencies.Count > 0));

            //get all packages and dependencies where at least one dependency exists
            componentsWithDependencies_.AddRange(GetFlatList(null, dependencies, componentsWithDependencies).OfType<IComponentWithDependencies>().Where(component => component.Dependencies.Count > 0).ToList());

            foreach (IComponentWithDependencies componentWithDependencies in componentsWithDependencies_)
            {
                foreach (DatabaseLogic logic in componentWithDependencies.Dependencies)
                {
                    logic.ParentPackageRefrence = componentWithDependencies;
                    logic.DependencyPackageRefrence = dependencies.Find(dependency => dependency.PackageName.Equals(logic.PackageName));
                    if (logic.DependencyPackageRefrence == null)
                    {
                        Logging.Error("DatabaseLogic component from package {0} was unable to link to dependency {1} (does the dependency not exist or bad reference?)", componentWithDependencies.ComponentInternalName, logic.PackageName);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates which packages and dependencies are dependent on other dependencies and if each dependency that is selected for install is enabled for installation
        /// </summary>
        /// <param name="dependencies">The list of dependencies</param>
        /// <param name="parsedCategoryList">The list of Categories</param>
        /// <param name="suppressSomeLogging">Flag for it some of the more verbose logging should be suppressed</param>
        /// <returns>A list of calculated dependencies to install</returns>
        public static List<Dependency> CalculateDependencies(List<Dependency> dependencies, List<Category> parsedCategoryList, bool suppressSomeLogging, bool showDependencyCalculationErrorMessages)
        {
            //flat list is packages
            List<SelectablePackage> flatListSelect = GetFlatSelectablePackageList(parsedCategoryList);

            //1- build the list of calling mods that need it
            List<Dependency> dependenciesToInstall = new List<Dependency>();

            //create list to track all database dependency references
            List<LogicTracking> refrencedDependencies = new List<LogicTracking>();

            Logging.Debug("Starting step 1 of 4 in dependency calculation: adding from categories");
            foreach (Category category in parsedCategoryList)
            {
                foreach (DatabaseLogic logic in category.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = category
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Category \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                category.Name, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                PackageName = category.Name,
                                WillBeInstalled = category.AnyPackagesChecked(),
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 1 complete");

            Logging.Debug("Starting step 2 of 4 in dependency calculation: adding from selectable packages that use each dependency");
            foreach (SelectablePackage package in flatListSelect)
            {
                //got though each logic property. if the package called is this dependency, then add it to it's list
                foreach (DatabaseLogic logic in package.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = package
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("SelectablePackage \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                package.PackageName, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                //set PackageName to the selectablepackage package name so later we know where this logic entry came from
                                PackageName = package.PackageName,
                                WillBeInstalled = package.Checked,
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 2 complete");


            Logging.Debug("Starting step 3 of 4 in dependency calculation: adding dependencies that use each dependency");
            //for each dependency go through each dependency's package logic and if it's called then add it
            foreach (Dependency processingDependency in dependencies)
            {
                foreach (DatabaseLogic logic in processingDependency.Dependencies)
                {
                    refrencedDependencies.Add(new LogicTracking
                    {
                        DatabaseLogic = logic,
                        ComponentWithDependencies = processingDependency
                    });
                    foreach (Dependency dependency in dependencies)
                    {
                        if (processingDependency.PackageName.Equals(dependency.PackageName))
                            continue;
                        if (logic.PackageName.Equals(dependency.PackageName))
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Dependency \"{0}\" logic entry added to dependency \"{1}\" of logic type \"{2}\", NotFlag value of \"{3}\"",
                                processingDependency.PackageName, dependency.PackageName, logic.Logic, logic.NotFlag);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                PackageName = processingDependency.PackageName,
                                //by default, dependences that are dependent on dependencies start as false until proven needed
                                WillBeInstalled = false,
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });

                            //log that the categories dependency reference was linked properly
                            logic.RefrenceLinked = true;
                        }
                    }
                }
            }
            Logging.Debug("Step 3 complete");

            //3a - check if any dependency references were never matched
            //like if a category references dependency the_dependency_packageName, but that package does not exist
            refrencedDependencies = refrencedDependencies.Where((refrence) => !refrence.DatabaseLogic.RefrenceLinked).ToList();
            Logging.Debug("Broken dependency references count: {0}", refrencedDependencies.Count);
            if (refrencedDependencies.Count > 0)
            {
                Logging.Error("The following packages call references to dependencies that do not exist:");
                foreach (LogicTracking logicTracking in refrencedDependencies)
                {
                    Logging.Error("Package: {0} => broken reference: {1}",
                        logicTracking.ComponentWithDependencies.ComponentInternalName, logicTracking.DatabaseLogic.PackageName);
                }
            }

            //4 - run calculations IN DEPENDENCY LIST ORDER FROM TOP DOWN
            List<Dependency> notProcessedDependnecies = new List<Dependency>(dependencies);
            Logging.Debug("Starting step 4 of 4 in dependency calculation: calculating dependencies from top down (perspective to list)");
            int calcNumber = 1;
            foreach (Dependency dependency in dependencies)
            {
                //first check if this dependency is referencing a dependency that has not yet been processed
                //if so then note it in the log
                if (!suppressSomeLogging)
                    Logging.Debug(string.Empty);
                if (!suppressSomeLogging)
                    Logging.Debug("Calculating if dependency {0} will be installed, {1} of {2}", dependency.PackageName, calcNumber++, dependencies.Count);

                foreach (DatabaseLogic login in dependency.DatabasePackageLogic)
                {
                    List<Dependency> matches = notProcessedDependnecies.Where(dep => login.PackageName.Equals(dep.PackageName)).ToList();
                    if (matches.Count > 0)
                    {
                        string errorMessage = string.Format("Dependency {0} is referenced by the dependency {1} which has not yet been processed! " +
                            "This will lead to logic errors in database calculation! Tip: this dependency ({0}) should be BELOW ({1}) in the" +
                            "list of dependencies in the editor. Order matters!", dependency.PackageName, login.PackageName);
                        Logging.Error(errorMessage);
                        //if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                        if (showDependencyCalculationErrorMessages)
                            MessageBox.Show(errorMessage);
                    }
                }

                //two types of logics - OR and AND (with NOT flags)
                //each can be calculated separately
                List<DatabaseLogic> localOR = dependency.DatabasePackageLogic.Where(logic => logic.Logic == Logic.OR).ToList();
                List<DatabaseLogic> logicalAND = dependency.DatabasePackageLogic.Where(logic => logic.Logic == Logic.AND).ToList();

                //debug logging
                if (!suppressSomeLogging)
                    Logging.Debug("Logical OR count: {0}", localOR.Count);
                if (!suppressSomeLogging)
                    Logging.Debug("Logical AND count: {0}", logicalAND.Count);

                //if there are no logical ands, then only do ors, vise versa
                bool ORsPass = localOR.Count > 0 ? false : true;
                bool ANDSPass = logicalAND.Count > 0 ? false : true;

                //if ors and ands are both true already, then something's broken
                if (ORsPass && ANDSPass)
                {
                    Logging.Warning("Logic ORs and ANDs already pass for dependency package {0} (nothing uses it?)", dependency.PackageName);
                    if (!suppressSomeLogging)
                        Logging.Debug("Skip calculation logic and remove from not processed list");

                    //remove it from list of not processed dependencies
                    notProcessedDependnecies.RemoveAt(0);
                    continue;
                }

                //calc the ORs first
                if (!suppressSomeLogging)
                    Logging.Debug("Processing OR logic");
                foreach (DatabaseLogic orLogic in localOR)
                {
                    //OR logic - if any mod/dependency is checked, then it's installed and can stop there
                    //because only one of them needs to be true
                    //same case goes for negatives - if mod is NOT checked and negateFlag
                    if (!orLogic.WillBeInstalled)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Skipping logic check of package {0} because it is not set for installation!", orLogic.PackageName);
                        continue;
                    }
                    else
                    {
                        if (!orLogic.NotFlag)
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, is checked and notFlag is false (package must be checked), sets orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                            ORsPass = true;
                            break;
                        }
                        else if (orLogic.NotFlag)
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, is NOT checked and notFlag is true (package must NOT be checked), sets orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                            ORsPass = true;
                            break;
                        }
                        else
                        {
                            if (!suppressSomeLogging)
                                Logging.Debug("Package {0}, checked={1}, notFlag={2}, does not set orLogic to pass!", orLogic.PackageName, orLogic.WillBeInstalled, orLogic.NotFlag);
                        }
                    }
                }

                //now calc the ands
                if (!suppressSomeLogging)
                    Logging.Debug("Processing AND logic");
                foreach (DatabaseLogic andLogic in logicalAND)
                {
                    if (andLogic.WillBeInstalled && !andLogic.NotFlag)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, is checked and notFlag is false (package must be checked), correct AND logic, continue", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = true;
                    }
                    else if (!andLogic.WillBeInstalled && andLogic.NotFlag)
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, is NOT checked and notFlag is true (package must NOT be checked), correct AND logic, continue", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = true;
                    }
                    else
                    {
                        if (!suppressSomeLogging)
                            Logging.Debug("Package {0}, checked={1}, notFlag={2}, incorrect AND logic, set ANDSPass=false and stop processing!", andLogic.PackageName, andLogic.WillBeInstalled, andLogic.NotFlag);
                        ANDSPass = false;
                        break;
                    }
                }

                string final = string.Format("Final result for dependency {0}: AND={1}, OR={2}", dependency.PackageName, ANDSPass, ORsPass);
                if (ANDSPass && ORsPass)
                {
                    if (suppressSomeLogging)
                        Logging.Info(LogOptions.MethodAndClassName, "Dependency {0} WILL be installed!", dependency.PackageName);
                    else
                        Logging.Debug("{0} (AND and OR) = TRUE, dependency WILL be installed!", final);
                    dependenciesToInstall.Add(dependency);
                }
                else
                {
                    if (!suppressSomeLogging)
                        Logging.Debug("{0} (AND and OR) = FALSE, dependency WILL NOT be installed!", final);
                }

                if (dependency.DatabasePackageLogic.Count > 0 && (ANDSPass && ORsPass))
                {
                    if (!suppressSomeLogging)
                        Logging.Debug("Updating future references (like logicalDependnecies) for if dependency was checked");
                    //update any dependencies that use it
                    foreach (DatabaseLogic callingLogic in dependency.Dependencies)
                    {
                        //get the dependency (if it is a dependency) that called this dependency
                        List<Dependency> found = dependencies.Where(dep => dep.PackageName.Equals(callingLogic.PackageName)).ToList();

                        if (found.Count > 0)
                        {
                            Dependency refrenced = found[0];
                            //now get the logic entry that references the original calculated dependency
                            List<DatabaseLogic> foundLogic = refrenced.DatabasePackageLogic.Where(logic => logic.PackageName.Equals(dependency.PackageName)).ToList();
                            if (foundLogic.Count > 0)
                            {
                                Logging.Debug("Logic reference entry for dependency {0} updated to {1}", refrenced.PackageName, ANDSPass && ORsPass);
                                foundLogic[0].WillBeInstalled = ANDSPass && ORsPass;
                            }
                            else
                            {
                                Logging.Error("Found logics count is 0 for updating references");
                            }
                        }
                        else
                        {
                            Logging.Error("Found count is 0 for updating references");
                        }
                    }
                }

                //remove it from list of not processed dependencies
                notProcessedDependnecies.RemoveAt(0);
            }

            Logging.Debug("Step 4 complete");
            return dependenciesToInstall;
        }

        /// <summary>
        /// Creates an array of DatabasePackage lists sorted by Installation groups i.e. list in array index 0 is packages of install group 0
        /// </summary>
        /// <param name="packagesToInstall"></param>
        /// <returns>The array of DatabasePackage lists</returns>
        public static List<DatabasePackage>[] CreateOrderedInstallList(List<DatabasePackage> packagesToInstall)
        {
            //get the max number of defined groups
            int maxGrops = packagesToInstall.Select(max => max.InstallGroupWithOffset).Max();

            //make the list to return
            //make it maxGroups +1 because group 4 exists, but making a array of 4 is 0-3
            List<DatabasePackage>[] orderedList = new List<DatabasePackage>[maxGrops + 1];

            //new up the lists
            for (int i = 0; i < orderedList.Count(); i++)
                orderedList[i] = new List<DatabasePackage>();

            foreach (DatabasePackage package in packagesToInstall)
            {
                orderedList[package.InstallGroupWithOffset].Add(package);
            }
            return orderedList;
        }

        /// <summary>
        /// Gets the maximum InstallGroup number from a list of Packages
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum InstallGroup number</returns>
        public static int GetMaxInstallGroupNumber(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.InstallGroup);
        }

        /// <summary>
        /// Gets the maximum InstallGroup number from a list of Packages factoring in the offset that a category may apply to it
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum InstallGroup number</returns>
        public static int GetMaxInstallGroupNumberWithOffset(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.InstallGroupWithOffset);
        }

        /// <summary>
        /// Gets the maximum PatchGroup number from a list of Packages
        /// </summary>
        /// <param name="listToCheck">The list of DatabasePackages</param>
        /// <returns>The maximum PatchGroup number</returns>
        public static int GetMaxPatchGroupNumber(List<DatabasePackage> listToCheck)
        {
            return listToCheck.Max(ma => ma.PatchGroup);
        }
    }
}
