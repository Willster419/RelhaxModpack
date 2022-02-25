using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Installer
{
    /// <summary>
    /// An Instruction is a task to be performed by the installer after packages are extracted.
    /// </summary>
    public abstract class Instruction : XmlComponent
    {
        /// <summary>
        /// Create an instance of the Instruction class.
        /// </summary>
        public Instruction() : base()
        {

        }

        /// <summary>
        /// Create an instance of the Instruction class, making a copy of a given Instruction.
        /// </summary>
        /// <param name="instructionToCopy">The Instruction whose values to copy.</param>
        public Instruction(Instruction instructionToCopy) : base(instructionToCopy)
        {

        }

        /// <summary>
        /// Creates a string array of properties in the class to serialize for loading to and from an xml document.
        /// </summary>
        /// <returns>The string array of properties to serialize between xml document and class values.</returns>
        /// <remarks>This is only used in InstructionLoader, to load documents of instructions. Now, instructions are saved directly in the packages class.</remarks>
        public abstract string[] PropertiesToSerialize();

        /// <summary>
        /// The xpath to use to get a list of xml element objects that represent each instruction to serialize.
        /// </summary>
        public abstract string RootObjectPath { get; }

        /// <summary>
        /// A single string with the filename of the processingNativeFile (needed for tracing work instructions after installation).
        /// </summary>
        public string NativeProcessingFile { get; set; } = string.Empty;

        /// <summary>
        /// the actual name of the original patch before processed.
        /// </summary>
        public string ActualPatchName { get; set; } = string.Empty;

        /// <summary>
        /// Get or set the DatabasePackage that this instruction belongs to.
        /// </summary>
        public DatabasePackage Package { get; set; }

        /// <summary>
        /// Gets a log formatted string for debugging containing key object name and values.
        /// </summary>
        /// <remarks>If debug output is enabled for the log file during an installation, then each instruction will have it's DumpInfoToLog property called.</remarks>
        public virtual string DumpInfoToLog
        {
            get
            {
                return string.Format("{0}={1}, {1}={2}", nameof(NativeProcessingFile), NativeProcessingFile, nameof(ActualPatchName), ActualPatchName);
            }
        }

        /// <summary>
        /// Compares two instructions to determine if their values are equal.
        /// </summary>
        /// <param name="instructionToCompare">The instruction to compare against.</param>
        /// <returns>True if the compared values are equal, false otherwise.</returns>
        public abstract bool InstructionsEqual(Instruction instructionToCompare);
    }
}
