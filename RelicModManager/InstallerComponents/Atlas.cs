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
        // imageHandler for this atlas
        public AtlasesCreator.IImageHandler imageHandler = null;
        // fileName of the atlas map file to extract
        public string MapFile { get; set; } = "";
        // mapHandler for this atlas
        public AtlasesCreator.IMapExporter mapExporter = null;
        // path inside the pkg file to the filename to process
        public string DirectoryInArchive { get; set; } = "";
        // path to atlas file outside the archive (after extraction located here)
        // public string TempAltasPresentDirectory { get; set; } = Path.Combine(Application.StartupPath, "RelHaxTemp");
        public string TempAltasPresentDirectory { get; set; } = Settings.RelhaxTempFolder;
        // path to place the finished file
        public string AtlasSaveDirectory { get; set; } = "";
        // width of the new atlases file
        public int AtlasWidth { get; set; } = 0;   // 0 or empty = get from original atlas file
        // height of the new atlases file
        public int AtlasHeight { get; set; } = 0;   // 0 or empty = get from original atlas file
        // padding of the new atlases file
        public int Padding { get; set; } = 1;
        // positioning optimation with PowOf2
        public State PowOf2 { get; set; } = State.None;
        // positioning optimation with Square
        public State Square { get; set; } = State.None;
        // allow to accept first successfull image optimazion layout
        public bool FastImagePacker { get; set; } = true;
        // generate map file
        public State GenerateMap { get; set; } = State.None;
        // map file type
        public MapType mapType { get; set; } = MapType.None;
        // maybe new fileName
        public List<string> ImageFolderList { get; set; } = new List<string>();
        //the list of textures in each atlas
        public List<Texture> TextureList { get; set; } = new List<Texture>();
        //for the tostring thing
        public override string ToString()
        {
            return string.Format("NativeProcessingFile: {0}\nActualPatchName: {1}\nPkg: {2}\nDirectoryInArchive: {3}\nAtlasFile: {4}\nStlasSaveDirectory: {5}\nSltas width: {6}\nSltas hight: {7}\nPadding: {8}\nPowOf2: {9}\nSquare: {10}\nFastImagePacker: {11}\nGenerateMap: {12}\nMapTypeName: {13}\nImageFolderList: {14}",
                NativeProcessingFile.Equals("") ? "(empty)" : NativeProcessingFile,
                ActualPatchName.Equals("") ? "(empty)" : ActualPatchName,
                Pkg.Equals("") ? "(empty)" : Pkg,
                DirectoryInArchive.Equals("") ? "(empty)" : DirectoryInArchive,
                AtlasFile.Equals("") ? "(empty)" : AtlasFile,
                AtlasSaveDirectory.Equals("") ? "(empty)" : AtlasSaveDirectory,
                AtlasWidth == 0 ? "(empty)" : "" + AtlasWidth,
                AtlasHeight == 0 ? "(empty)" : "" + AtlasHeight,
                Padding == 0 ? "(empty)" : "" + Padding,
                PowOf2 == State.None ? "(empty)" : PowOf2 == State.True ? "True" : "False",
                Square == State.None ? "(empty)" : Square == State.True ? "True" : "False",
                FastImagePacker ? "True" : "False",
                GenerateMap == State.None ? "(empty)" : GenerateMap == State.True ? "True" : "False",
                mapType == MapType.None ? "(none selected)" : MapTypeName(mapType),
                ImageFolderList.Count == 0 ? "(empty)" : ImageFolderList.ToString());
        }

        public enum MapType
        {
            None,
            WGXmlMap,
            XmlMap,
            TxtMap,
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
                default:
                    return MapTypeName(MapType.WGXmlMap);
            }
        }

        public enum State
        {
            None = 0,
            True,
            False
        }
    }
}
