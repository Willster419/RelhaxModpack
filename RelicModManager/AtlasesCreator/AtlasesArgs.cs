using System.Collections.Generic;
using System.Drawing;

namespace RelhaxModpack.AtlasesCreator
{
    public class AtlasesArgs
    {
        public int MaxWidth { get; set; } = Settings.Default.MaxWidth;
        public int MaxHeight { get; set; } = Settings.Default.MaxHeight;
        public int Padding { get; set; } = Settings.Default.Padding;
        public bool PowOf2 { get; set; } = Settings.Default.PowOf2;
        public bool Square { get; set; } = Settings.Default.Square;
        public bool GenerateMap { get; set; } = Settings.Default.GenerateMap;
        // the atlas xml file
        public string MapFile = "";
        // the atlas image file
        public string ImageFile = "";
        // the images list
        public List<Texture> Images { get; set; } = null;
    }
}
