using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{
    /// <summary>
    /// Defines settings used in the database automation runner window
    /// </summary>
    public class AutomationRunnerSettings : ISettingsFile
    {
        /// <summary>
        /// The name of the xml file on disk
        /// </summary>
        public string Filename { get; } = Settings.AutomationRunnerSettingsFilename;

        public string SelectedBranch { get; set; } = "master";
    }
}
