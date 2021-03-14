using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// Defines the settings used for the patch class
    /// </summary>
    /// <remarks>There is no constructor for this class. It is not needed as the act of not having one assigns a default
    /// of an empty constructor with to parameters. See https://stackoverflow.com/a/23458819/3128017 </remarks>
    public class PatchSettings : ISettingsFile
    {
        /// <summary>
        /// The name of the settings file on disk
        /// </summary>
        public const string SettingsFilename = "PatchSettings.xml";

        /// <summary>
        /// The name of the xml file on disk
        /// </summary>
        public string Filename { get { return SettingsFilename; } }

        /// <summary>
        /// A list of properties and fields to exclude from saving/loading to and from xml
        /// </summary>
        public string[] MembersToExclude { get { return new string[] { nameof(MembersToExclude), nameof(Filename), nameof(SettingsFilename) }; } }

        /// <summary>
        /// If the selected patch should be saved (hitting apply) before the selection changes
        /// </summary>
        public bool SaveSelectionBeforeLeave = false;

        /// <summary>
        /// If true, when testing a patch, the log tab will become the active tab
        /// </summary>
        public bool SwitchToLogWhenTestingPatch = true;

        /// <summary>
        /// The path to use for replacing the {app} macro in patch files
        /// </summary>
        /// <remarks>This is for the relative patch mode</remarks>
        public string AppMacro = string.Empty;

        /// <summary>
        /// The version of the game to use for replacing the {versiondir} macro in patch files
        /// </summary>
        /// <remarks>This is for the relative patch mode</remarks>
        public string VersiondirMacro = string.Empty;

        /// <summary>
        /// Controls how the save patch and apply button interact with each other. See ApplyBehavior enumeration for interaction definitions.
        /// </summary>
        public ApplyBehavior ApplyBehavior = ApplyBehavior.Default;
    }
}
