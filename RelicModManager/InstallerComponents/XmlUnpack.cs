namespace RelhaxModpack
{
    class XmlUnpack
    {
        //a single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        public string nativeProcessingFile { get; set; } = "";
        //the actual name of the original file before processed
        public string actualPatchName { get; set; } = "";
        // path and name to the package file
        public string pkg { get; set; } = "";
        // fileName of the file to extract
        public string fileName { get; set; } = "";
        // path inside the pkg file to the filename to process
        public string directoryInArchive { get; set; } = "";
        // path to place the finished file
        public string extractDirectory { get; set; } = "";
        // maybe new fileName
        public string newFileName { get; set; } = "";
        //for the tostring thing
        public override string ToString()
        {
            return string.Format("nativeProcessingFile: {0}\nactualPatchName: {1}\npkg: {2}\ndirectoryInArchive: {3}\nfileName: {4}\nextractDirectory: {5}\nnewFileName: {6}",
                nativeProcessingFile.Equals("") ? "(empty)" : nativeProcessingFile,
                actualPatchName.Equals("") ? "(empty)" : actualPatchName,
                pkg.Equals("") ? "(empty)" : pkg,
                directoryInArchive.Equals("") ? "(empty)" : directoryInArchive,
                fileName.Equals("") ? "(empty)" : fileName,
                extractDirectory.Equals("") ? "(empty)" : extractDirectory,
                newFileName.Equals("") ? "(empty)" : newFileName);
        }
    }
}
