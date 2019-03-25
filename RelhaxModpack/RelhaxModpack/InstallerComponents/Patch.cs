namespace RelhaxModpack
{
    //TODO: move patch classes into interface stuff
    //a patch is an instruction of how to modify a text file
    //generally a mod config file
    public class Patch
    {
        //a single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        public string NativeProcessingFile = string.Empty;
        //the actual name of the origional patch before processed
        public string ActualPatchName = string.Empty;
        //the type of patch, xml or regex (direct text replacement)
        public string Type = string.Empty;
        //if xml, the mode that the xml patcher should use
        //add xml node, remove xml node, edit xml node
        public string Mode = string.Empty;
        //the starting path to the file
        public string PatchPath = string.Empty;
        //the path to the file, relative to patchPath
        public string File = string.Empty;
        //the complete path to the file, saved at parse time
        public string CompletePath = string.Empty;
        //if xml or json, the xml xpath to the node
        public string Path = string.Empty;
        //if regex, the optional specific lines in the text file
        //to make the modifications
        public string[] Lines;
        //the node inner text (xml) or regex criteria to search for
        public string Search = string.Empty;
        //the text to replace the found search text with
        public string Replace = string.Empty;
        //for json, if it should use the new method of seperating the path for getting the xvm refrences
        public bool FollowPath = false;
        public string DumpPatchInfoForLog
        {
            get
            {
                switch (Type.ToLower())
                {
                    case "regex":
                    case "regx":
                        return string.Format("regex patch, {0}={1}, {2}={3}, {4}={5}, {6}={7}, {8}={9}", nameof(PatchPath), PatchPath,
                            nameof(File), File, nameof(Lines), Lines == null ? "null" : string.Join(",",Lines), nameof(Search), Search,
                            nameof(Replace), Replace);
                    case "xml":
                        return string.Format("xml patch, {0}={1}, {2}={3}, {4}={5}, {6}={7}, {8}={9}", nameof(PatchPath), PatchPath,
                            nameof(File), File, nameof(Path), Path, nameof(Search), Search, nameof(Replace), Replace);
                    case "json":
                        return string.Format("json patch, {0}={1}, {2}={3}, {4}={5}, {6}={7}, {8}={9}", nameof(PatchPath), PatchPath,
                            nameof(File), File, nameof(Path), Path, nameof(Search), Search,  nameof(Replace), Replace);
                    default:
                        return string.Format("ERROR: unknown type: {0}",Type);
                }
            }
        }
    }
}
