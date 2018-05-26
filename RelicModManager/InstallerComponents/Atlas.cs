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
        // width of the new atlases file
        public int atlasWidth { get; set; } = 0;   // -1 = get from original atlas file
        // hight of the new atlases file
        public int atlasHight { get; set; } = 0;   // -1 = get from original atlas file
        // padding of the new atlases file
        public int padding { get; set; } = 0;
        // positioning optimation with PowOf2
        public State powOf2 { get; set; } = State.Null;
        // positioning optimation with Square
        public State square { get; set; } = State.Null;
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
            return string.Format("nativeProcessingFile: {0}\nactualPatchName: {1}\npkg: {2}\ndirectoryInArchive: {3}\natlasFile: {4}\natlasSaveDirectory: {5}\naltas width: {6}\naltas hight: {7}\npadding: {8}\npowOf2: {9}\nsquare: {10}\ngenerateMap: {11}\nMapTypeName: {12}\nimageFolderList: {13}",
                nativeProcessingFile.Equals("") ? "(empty)" : nativeProcessingFile,
                actualPatchName.Equals("") ? "(empty)" : actualPatchName,
                pkg.Equals("") ? "(empty)" : pkg,
                directoryInArchive.Equals("") ? "(empty)" : directoryInArchive,
                atlasFile.Equals("") ? "(empty)" : atlasFile,
                atlasSaveDirectory.Equals("") ? "(empty)" : atlasSaveDirectory,
                atlasWidth == 0 ? "(empty)" : "" + atlasWidth,
                atlasHight == 0 ? "(empty)" : "" + atlasHight,
                padding == 0 ? "(empty)" : "" + padding,
                powOf2 == State.Null ? "(empty)" : powOf2 == State.True ? "True" : "False",
                square == State.Null ? "(empty)" : square == State.True ? "True" : "False",
                generateMap == State.Null ? "(empty)" : generateMap == State.True ? "True" : "False",
                mapType == MapType.None ? "(none selected)" : MapTypeName(mapType),
                imageFolderList.Count == 0 ? "(empty)" : imageFolderList.ToString());
        }

        public enum MapType
        {
            WGXmlMap = 1,
            XmlMap,
            TxtMap,
            IMap,
            None
        }

        public string MapTypeName(MapType mt)
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
