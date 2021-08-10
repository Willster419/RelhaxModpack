using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible return code error conditions that can occur when running a sequence
    /// </summary>
    public enum SequencerExitCode
    {
        NotRun,

        LinkPackagesToAutomationSequencesFail,

        LoadAutomationSequencesXmlToRunAsyncFail,

        ParseAutomationSequencesPreRunFail,

        LoadGlobalMacrosFail,

        LoadApplicationMacrosFail,

        LoadLocalMacrosFail,

        Errors,

        NoErrors,

        Cancel
    }
}
