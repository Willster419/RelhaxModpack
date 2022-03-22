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
    public abstract class DirectorySearchTask : DirectoryTask, IXmlSerializable
    {
        public const string SEARCH_ALL = "*";

        public string SearchPattern { get; set; } = SEARCH_ALL;

        public string Recursive { get; set; }

        protected bool recursive;

        protected string[] searchResults;

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

            if (ValidateCommandFalse(ableToParseRecursive, string.Format("Unable to parse the arg Recursive from given string {0}", Recursive)))
                return;
        }

        /// <summary>
        /// Runs the main feature of the task.
        /// </summary>
        public override async Task RunTask()
        {
            RunSearch();
        }

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
