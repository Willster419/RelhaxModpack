using System;

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
        public string FollowPathEditorCompletePath = string.Empty;
        //the version of the patch. default to 1
        public int Version = 1;
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
        //if from editor, enable verbose logging for the duration of that patch
        public bool FromEditor = false;
        public string DumpPatchInfoForLog
        {
            get
            {
                return string.Format("{0} patch, NativeProcessingFile={1}, ActualFile={2}," +
                    "{3}{4}PatchPath={5}, FileToPatch={6}," +
                    "{7}{8}Lines={9}, Path={0}, Search={10}, Replace={11}",
                    Type.ToLower(),
                    NativeProcessingFile,
                    ActualPatchName,
                    Environment.NewLine,
                    Settings.LogSpacingLinup,
                    PatchPath,
                    File,
                    Environment.NewLine,
                    Settings.LogSpacingLinup,
                    Lines == null ? "null" : string.Join(",", Lines),
                    string.IsNullOrEmpty(Path) ? "null" :Path,
                    Search,
                    Replace);
            }
        }
        public override string ToString()
        {
            if (FromEditor)
            {
                return string.Format("type={0} ,mode={1}, lines/path={2}", Type, Mode, Lines == null ? Path: string.Join(",", Lines));
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
