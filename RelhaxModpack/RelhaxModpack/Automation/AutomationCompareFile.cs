using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation.Tasks
{
    /// <summary>
    /// An AutomationCompareFile object is a comparison between two different file sources to check if both are equal in md5 hash.
    /// </summary>
    public class AutomationCompareFile : AutomationCompare
    {
        /// <summary>
        /// Creates an instance of the AutomationCompareDirectory class.
        /// </summary>
        /// <param name="directoryA">The path to the first directory that contains the file to compare (file a).</param>
        /// <param name="directoryB">The path to the second directory that contains the file to compare (file b).</param>
        /// <param name="compareMode">The comparison mode of the directories.</param>
        /// <param name="fileA">The name and extension of file a.</param>
        /// <param name="fileB">The name and extension of file b.</param>
        /// <param name="hashA">The md5 hash value of file a.</param>
        /// <param name="hashB">The md5 hash value of file b.</param>
        public AutomationCompareFile(string directoryA, string directoryB, AutomationCompareMode compareMode, string fileA, string hashA, string fileB, string hashB) : base(directoryA, directoryB, compareMode)
        {
            CompareAFile = fileA;
            CompareBFile = fileB;
            CompareAHash = hashA;
            CompareBHash = hashB;
        }

        /// <summary>
        /// Gets if the md5 hash values of the files are different between the two.
        /// </summary>
        /// <remarks>True means that the md5 hash values are equal.</remarks>
        public override bool CompareResult { get { return CompareAHash.Equals(CompareBHash); } }

        /// <summary>
        /// The name and extension of file a.
        /// </summary>
        public string CompareAFile;

        /// <summary>
        /// The name and extension of file b.
        /// </summary>
        public string CompareBFile;

        /// <summary>
        /// The md5 hash value of file a.
        /// </summary>
        public string CompareAHash;

        /// <summary>
        /// The md5 hash value of file b.
        /// </summary>
        public string CompareBHash;

        /// <summary>
        /// Gets the complete path to file a.
        /// </summary>
        public string CompareAFilepath { get { return Path.Combine(CompareAPath, CompareAFile); } }

        /// <summary>
        /// Gets the complete path to file b.
        /// </summary>
        public string CompareBFilepath { get { return Path.Combine(CompareBPath, CompareBFile); } }

        /// <summary>
        /// Print information about this comparison entry to the log file.
        /// </summary>
        public override void PrintToLog()
        {
            Logging.Debug("A File {0}, Hash {1}", CompareAFilepath, CompareAHash);
            Logging.Debug("B File {0}, Hash {1}", CompareBFilepath, CompareBHash);
        }
    }
}
