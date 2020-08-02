namespace RelhaxModpack.Xml
{
    /// <summary>
    /// Represents a set of xml instructions for extracting and copying xml files into another location. Includes xml binary decompression
    /// </summary>
    public class XmlUnpack
    {
        /// <summary>
        /// A single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        /// </summary>
        public string NativeProcessingFile { get; set; } = string.Empty;

        /// <summary>
        /// The actual name of the original file before processed
        /// </summary>
        public string ActualPatchName { get; set; } = string.Empty;

        /// <summary>
        /// Path and name to the package file
        /// </summary>
        public string Pkg { get; set; } = string.Empty;

        /// <summary>
        /// Name of the file to extract
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Path inside the pkg file to the filename to process
        /// </summary>
        public string DirectoryInArchive { get; set; } = string.Empty;

        /// <summary>
        /// Path to place the extracted or copied file
        /// </summary>
        public string ExtractDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Filename with replaced macros for destination writing
        /// </summary>
        public string NewFileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Collect all properties of the extraction instructions to dump into the log file
        /// </summary>
        public string DumpInfoToLog
        {
            get
            {
                return string.Format("NativeProcessingFile={0}, ActualPatchName={1}, Pkg={2}, DirectoryInArchive={3}, FileName={4}, ExtractDirectory={5}, NewFileName={6}",
                NativeProcessingFile,
                ActualPatchName,
                Pkg,
                DirectoryInArchive,
                FileName,
                ExtractDirectory,
                NewFileName);
            }
        }

        /// <summary>
        /// A string representation of the object
        /// </summary>
        /// <returns>The native name of the unpack instruction file and file target</returns>
        public override string ToString()
        {
            return string.Format("NativeProcessingFile={0}, FileName={1}", NativeProcessingFile, FileName);
        }
    }
}
