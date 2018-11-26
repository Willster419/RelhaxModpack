namespace RelhaxModpack
{
    //a patch is an instruction of how to modify a text file
    //generally a mod config file
    public class Patch
    {
        //a single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        public string NativeProcessingFile { get; set; }
        //the actual name of the origional patch before processed
        public string ActualPatchName { get; set; }
        //the type of patch, xml or regex (direct text replacement)
        public string Type { get; set; }
        //if xml, the mode that the xml patcher should use
        //add xml node, remove xml node, edit xml node
        public string mode { get; set; }
        //the starting path to the file
        public string PatchPath { get; set; }
        //the path to the file, relative to patchPath
        public string file { get; set; }
        //the complete path to the file, saved at parse time
        public string CompletePath { get; set; }
        //if xml or json, the xml xpath to the node
        public string Path { get; set; }
        //if regex, the optional specific lines in the text file
        //to make the modifications
        public string[] lines { get; set; }
        //the node inner text (xml) or regex criteria to search for
        public string Search { get; set; }
        //the text to replace the found search text with
        public string Replace { get; set; }
        public Patch() { }
    }
}
