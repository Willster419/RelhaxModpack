using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class Atlas
    {
        //a single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        public string nativeProcessingFile { get; set; } = "";
        //the actual name of the original file before processed
        public string actualPatchName { get; set; } = "";
        // path and name to the package file
        public string pkg { get; set; } = "";
        // fileName of the atlas file to extract
        public string atlasFile { get; set; } = "";
        // fileName of the atlas map file to extract
        public string mapFile { get; set; } = "";
        // path inside the pkg file to the filename to process
        public string directoryInArchive { get; set; } = "";
        // path to atlas file outside the archive (after extraction located here)
        public string tempAltasPresentDirectory { get; set; } = "";
        // path to place the finished file
        public string atlasSaveDirectory { get; set; } = "";
        // maybe new fileName
        public List<string> imageFolderList { get; set; } = new List<string>();
        // temp workingFolder
        public string workingFolder { get; set; } = "";
        //the list of textures in each atlas
        public List<Texture> TextureList { get; set; } = new List<Texture>();
        //for the tostring thing
        public override string ToString()
        {
            return string.Format("nativeProcessingFile: {0}\nactualPatchName: {1}\npkg: {2}\ndirectoryInArchive: {3}\natlasFile: {4}\natlasSaveDirectory: {5}\nimageFolderList: {6}",
                nativeProcessingFile.Equals("") ? "(empty)" : nativeProcessingFile,
                actualPatchName.Equals("") ? "(empty)" : actualPatchName,
                pkg.Equals("") ? "(empty)" : pkg,
                directoryInArchive.Equals("") ? "(empty)" : directoryInArchive,
                atlasFile.Equals("") ? "(empty)" : atlasFile,
                atlasSaveDirectory.Equals("") ? "(empty)" : atlasSaveDirectory,
                imageFolderList.Count == 0 ? "(empty)" : imageFolderList.ToString());
        }
    }
}
