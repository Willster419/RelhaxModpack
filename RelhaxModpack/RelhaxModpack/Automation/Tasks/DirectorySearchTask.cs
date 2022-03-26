using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// A DirectorySearchTask provides an implementation to search for files in a path, specifying the file name and recursion.
    /// </summary>
    public abstract class DirectorySearchTask : DirectoryTask, IXmlSerializable
    {
        /// <summary>
        /// The default search for files. Will return all files.
        /// </summary>
        public const string SEARCH_ALL = "*";

        /// <summary>
        /// The search pattern to use for the file search.
        /// </summary>
        public string SearchPattern { get; set; } = SEARCH_ALL;

        /// <summary>
        /// Determines if the serach should recurse into child directories.
        /// </summary>
        public string Recursive { get; set; }

        /// <summary>
        /// Parsed result of the argument Recursive.
        /// </summary>
        /// <seealso cref="Recursive"/>
        protected bool recursive;

        /// <summary>
        /// The list of complete file paths that match the search criteria.
        /// </summary>
        protected string[] searchResults;

        /// <summary>
        /// Flag to indicate if the search operation completed correctly.
        /// </summary>
        protected bool ableToParseRecursive = false;

        #region Xml Serialization
        /// <summary>
        /// Defines a list of properties in the class to be serialized into xml attributes.
        /// </summary>
        /// <returns>A list of string property names.</returns>
        /// <remarks>Xml attributes will always be written, xml elements are optional.</remarks>
        public override string[] PropertiesForSerializationAttributes()
        {
            return base.PropertiesForSerializationAttributes().Concat(new string[] { nameof(SearchPattern), nameof(Recursive)}).ToArray();
        }
        #endregion

        #region Task Execution
        /// <summary>
        /// Process any macros that exist in the task's arguments.
        /// </summary>
        public override void ProcessMacros()
        {
            base.ProcessMacros();
            SearchPattern = ProcessMacro(nameof(SearchPattern), SearchPattern);
            Recursive = ProcessMacro(nameof(Recursive), Recursive);

            if (bool.TryParse(Recursive, out bool result))
            {
                ableToParseRecursive = true;
                recursive = result;
            }
        }

        /// <summary>
        /// Validates that all task arguments are correct and the task is initialized correctly to execute.
        /// </summary>
        public override void ValidateCommands()
        {
            base.ValidateCommands();
            if (ValidateCommandStringNullEmptyTrue(nameof(SearchPattern), SearchPattern))
                return;
            if (ValidateCommandStringNullEmptyTrue(nameof(Recursive), Recursive))
                return;

            if (ValidateCommandFalse(ableToParseRecursive, string.Format("Unable to parse The argument Recursive from given string {0}", Recursive)))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RunTask()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            RunSearch();
        }

        /// <summary>
        /// Runs the file search.
        /// </summary>
        protected virtual void RunSearch()
        {
            searchResults = FileUtils.FileSearch(DirectoryPath, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, false, false, SearchPattern);
        }

        /// <summary>
        /// Validate that the task executed without error and any expected output resources were processed correctly.
        /// </summary>
        public override void ProcessTaskResults()
        {
            if (ProcessTaskResultTrue(searchResults == null, "The searchResult array returned null"))
                return;

            if (ProcessTaskResultTrue(searchResults.Count() == 0, "The searchResult array returned 0 results"))
                return;
        }
        #endregion
    }
}
