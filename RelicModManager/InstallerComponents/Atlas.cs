using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public class Atlas
    {
        //a single string with the filename of the processingNativeFile (needed for tracing work instructions after installation)
        public string NativeProcessingFile { get; set; } = "";
        //the actual name of the original file before processed
        public string ActualPatchName { get; set; } = "";
        // path and name to the package file
        public string Pkg { get; set; } = "";
        // fileName of the atlas file to extract
        public string AtlasFile { get; set; } = "";
        // fileName of the atlas map file to extract
        public string MapFile { get; set; } = "";
        // path inside the pkg file to the filename to process
        public string directoryInArchive { get; set; } = "";
        // path to atlas file outside the archive (after extraction located here)
        public string tempAltasPresentDirectory { get; set; } = Path.Combine(Application.StartupPath, "RelHaxTemp");
        // path to place the finished file
        public string atlasSaveDirectory { get; set; } = "";
        // width of the new atlases file
        public int atlasWidth { get; set; } = 0;   // 0 or empty = get from original atlas file
        // height of the new atlases file
        public int atlasHeight { get; set; } = 0;   // 0 or empty = get from original atlas file
        // padding of the new atlases file
        public int padding { get; set; } = 1;
        // positioning optimation with PowOf2
        public State powOf2 { get; set; } = State.Null;
        // positioning optimation with Square
        public State square { get; set; } = State.Null;
        // allow to accept first successfull image optimazion layout
        public bool fastImagePacker { get; set; } = true;
        // generate map file
        public State generateMap { get; set; } = State.Null;
        // map file type
        public MapType mapType { get; set; } = MapType.None;
        // maybe new fileName
        public List<string> imageFolderList { get; set; } = new List<string>();
        // temp workingFolder
        public string workingFolder { get; set; } = "";
        //the list of textures in each atlas
        public List<Texture> TextureList { get; set; } = new List<Texture>();
        //for the tostring thing
        public override string ToString()
        {
            return string.Format("NativeProcessingFile: {0}\nActualPatchName: {1}\nPkg: {2}\ndirectoryInArchive: {3}\nAtlasFile: {4}\natlasSaveDirectory: {5}\naltas width: {6}\naltas hight: {7}\npadding: {8}\npowOf2: {9}\nsquare: {10}\nfastImagePacker: {11}\ngenerateMap: {12}\nMapTypeName: {13}\nimageFolderList: {14}",
                NativeProcessingFile.Equals("") ? "(empty)" : NativeProcessingFile,
                ActualPatchName.Equals("") ? "(empty)" : ActualPatchName,
                Pkg.Equals("") ? "(empty)" : Pkg,
                directoryInArchive.Equals("") ? "(empty)" : directoryInArchive,
                AtlasFile.Equals("") ? "(empty)" : AtlasFile,
                atlasSaveDirectory.Equals("") ? "(empty)" : atlasSaveDirectory,
                atlasWidth == 0 ? "(empty)" : "" + atlasWidth,
                atlasHeight == 0 ? "(empty)" : "" + atlasHeight,
                padding == 0 ? "(empty)" : "" + padding,
                powOf2 == State.Null ? "(empty)" : powOf2 == State.True ? "True" : "False",
                square == State.Null ? "(empty)" : square == State.True ? "True" : "False",
                fastImagePacker ? "True" : "False",
                generateMap == State.Null ? "(empty)" : generateMap == State.True ? "True" : "False",
                mapType == MapType.None ? "(none selected)" : MapTypeName(mapType),
                imageFolderList.Count == 0 ? "(empty)" : imageFolderList.ToString());
        }

        public enum MapType
        {
            WGXmlMap,
            XmlMap,
            TxtMap,
            IMap,
            None
        }

        public static string MapTypeName(MapType mt)
        {
            switch (mt)
            {
                case MapType.WGXmlMap:
                    return "WgXml";
                case MapType.XmlMap:
                    return "Xml";
                case MapType.TxtMap:
                    return "Txt";
                case MapType.IMap:
                    return "IMap";
                default:
                    return MapTypeName(MapType.WGXmlMap);
            }
        }

        public enum State
        {
            Null = 1,
            True,
            False
        }
    }
}
