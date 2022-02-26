using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible return code error conditions that can occur when running a sequence.
    /// </summary>
    public enum SequencerExitCode
    {
        /// <summary>
        /// The sequence was not run. It could mean that a previous sequence in the job list was canceled, or there was an error before the sequencer could start.
        /// </summary>
        NotRun,

        /// <summary>
        /// There was an error with linking packages to automation sequences.
        /// </summary>
        LinkPackagesToAutomationSequencesFail,

        /// <summary>
        /// There was an error with loading automation sequences as defined from the automation root document.
        /// </summary>
        LoadAutomationSequencesXmlToRunAsyncFail,

        /// <summary>
        /// There was an error with parsing an automation sequence. Check xml syntax or missing task attributes.
        /// </summary>
        ParseAutomationSequencesPreRunFail,

        /// <summary>
        /// There was an error loading or parsing the global macros xml document.
        /// </summary>
        LoadGlobalMacrosFail,

        /// <summary>
        /// There was an error loading application defined macros.
        /// </summary>
        LoadApplicationMacrosFail,

        /// <summary>
        /// There was an error loading locally defined macros in the sequence or custom defined macros from the sequencer.
        /// </summary>
        LoadLocalMacrosFail,

        /// <summary>
        /// There was an error resetting application macros.
        /// </summary>
        ResetApplicationMacrosFail,

        /// <summary>
        /// There were errors running a task in a sequence.
        /// </summary>
        Errors,

        /// <summary>
        /// No errors occurred, the sequence(s) completed successfully.
        /// </summary>
        NoErrors,

        /// <summary>
        /// The sequence run was canceled.
        /// </summary>
        Cancel
    }
}
