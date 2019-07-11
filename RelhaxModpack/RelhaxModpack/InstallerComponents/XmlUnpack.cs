namespace RelhaxModpack
{
    public class XmlUnpack
    {
        //a single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        public string NativeProcessingFile { get; set; } = "";
        //the actual name of the original file before processed
        public string ActualPatchName { get; set; } = "";
        // path and name to the package file
        public string Pkg { get; set; } = "";
        // fileName of the file to extract
        public string FileName { get; set; } = "";
        // path inside the pkg file to the filename to process
        public string DirectoryInArchive { get; set; } = "";
        // path to place the finished file
        public string ExtractDirectory { get; set; } = "";
        // maybe new fileName
        public string NewFileName { get; set; } = "";
        //for the tostring thing
        public override string ToString()
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
}
