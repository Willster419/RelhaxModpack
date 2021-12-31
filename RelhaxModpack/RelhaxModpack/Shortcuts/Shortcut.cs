using RelhaxModpack.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack.Shortcuts
{
    /// <summary>
    /// Represents instructions on how to create a shortcut
    /// </summary>
    public class Shortcut : Instruction
    {

        public const string ShortcutXmlSearchPath = "/shortcuts/shortcut";

        public override string RootObjectPath { get { return ShortcutXmlSearchPath; } }

        public override string[] PropertiesToSerialize()
        {
            return new string[]
            {
                nameof(Path),
                nameof(Name),
                nameof(Enabled)
            };
        }

        /// <summary>
        /// The target of the shortcut
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// The name for the shortcut
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Flag for in the installer to actually create the shortcut
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>The name and target of the shortcut</returns>
        public override string ToString()
        {
            return string.Format("Name={0} Target={1}", Name, Path);
        }
    }

    
}
