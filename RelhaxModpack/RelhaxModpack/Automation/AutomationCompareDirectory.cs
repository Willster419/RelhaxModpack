using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// An AutomationCompareDirectory object is a comparison between two different directories to check if the number and name of files are equal.
    /// </summary>
    public class AutomationCompareDirectory : AutomationCompare
    {
        /// <summary>
        /// Creates an instance of the AutomationCompareDirectory class.
        /// </summary>
        /// <param name="directoryA">The path to the first directory to compare.</param>
        /// <param name="directoryB">The path to the second directory to compare.</param>
        /// <param name="compareMode">The comparison mode of the directories</param>
        /// <param name="numFilesA">The number of files in directory A.</param>
        /// <param name="numFilesB">The number of files in directory B.</param>
        /// <param name="indexOfFilenameChange">The nth+1 file in the list of files of each directory that is a different name, if applicable. A -1 means all file names are equal.</param>
        public AutomationCompareDirectory(string directoryA, string directoryB, AutomationCompareMode compareMode, int numFilesA, int numFilesB, int indexOfFilenameChange = -1) : base(directoryA, directoryB, compareMode)
        {
            this.CompareANumFiles = numFilesA;
            this.CompareBNumFiles = numFilesB;
            this.IndexOfFilenameChange = indexOfFilenameChange;
        }

        /// <summary>
        /// Gets if the number or names of files are different between the two directories.
        /// </summary>
        /// <remarks>True means that the number and names of files are equal.</remarks>
        public override bool CompareResult
        {
            get 
            {
                if (CompareBNumFiles != CompareANumFiles)
                    return false;
                else if (IndexOfFilenameChange != -1)
                    return false;
                else return true;
            } 
        }

        /// <summary>
        /// The number of files in directory A.
        /// </summary>
        public int CompareANumFiles;

        /// <summary>
        /// The number of files in directory B.
        /// </summary>
        public int CompareBNumFiles;

        /// <summary>
        /// The nth+1 file in the list of files of each directory that is a different name, if applicable. A -1 means all file names are equal.
        /// </summary>
        public int IndexOfFilenameChange = 0;

        /// <summary>
        /// Print information about this comparison entry to the log file.
        /// </summary>
        public override void PrintToLog()
        {
            Logging.Debug("A Directory {0}, Count {1}", CompareAPath, CompareANumFiles);
            Logging.Debug("B Directory {0}, Count {1}", CompareBPath, CompareBNumFiles);
        }
    }
}
