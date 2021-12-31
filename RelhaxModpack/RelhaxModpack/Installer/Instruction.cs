using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Installer
{
    public abstract class Instruction : XmlDatabaseComponent
    {
        public abstract string[] PropertiesToSerialize();

        public abstract string RootObjectPath { get; }

        public string SchemaVersionLocal { get; set; }

        /// <summary>
        /// A single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        /// </summary>
        public string NativeProcessingFile { get; set; } = string.Empty;

        /// <summary>
        /// the actual name of the original patch before processed
        /// </summary>
        public string ActualPatchName { get; set; } = string.Empty;
    }
}
