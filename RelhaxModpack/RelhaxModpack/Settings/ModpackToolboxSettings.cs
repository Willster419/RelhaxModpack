using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// The settings file for the ModpackToolbox window class
    /// </summary>
    public class ModpackToolboxSettings : ISettingsFile
    {
        /// <summary>
        /// The name of the settings file on disk
        /// </summary>
        public const string SettingsFilename = "ModpackToolboxSettings.xml";

        /// <summary>
        /// The name of the xml file on disk
        /// </summary>
        public string Filename { get { return SettingsFilename; } }

        /// <summary>
        /// A list of properties and fields to exclude from saving/loading to and from xml
        /// </summary>
        public string[] MembersToExclude { get { return new string[] { nameof(MembersToExclude), nameof(Filename), nameof(SettingsFilename) }; } }

        /// <summary>
        /// Toggle if using the custom path
        /// </summary>
        public bool UseCustomDbPath { get; set; } = false;

        /// <summary>
        /// If using a custom path to perform work, toggle the folder path of the RelhaxModpackDatabaseRepo
        /// </summary>
        public string CustomDbPath { get; set; } = "C:\\Users\\Willster419\\Tanks Stuff\\RelhaxModpackDatabase";
    }
}
