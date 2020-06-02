using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack.Atlases
{
    /// <summary>
    /// A class that serves as a description of an atlas file with processing instructions
    /// </summary>
    public class Atlas
    {
        /// <summary>
        /// Path and name to the package file
        /// </summary>
        public string Pkg { get; set; } = string.Empty;

        /// <summary>
        /// FileName of the atlas image file to extract
        /// </summary>
        public string AtlasFile { get; set; } = string.Empty;

        /// <summary>
        /// FileName of the atlas map file to extract
        /// </summary>
        public string MapFile { get; set; } = string.Empty;

        /// <summary>
        /// Path inside the pkg file to the filename to process
        /// </summary>
        public string DirectoryInArchive { get; set; } = string.Empty;

        /// <summary>
        /// Path to place the generated atlas file
        /// </summary>
        public string AtlasSaveDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Width of the new atlases file. 0 = get from original atlas file
        /// </summary>
        public int AtlasWidth { get; set; } = 0;

        /// <summary>
        /// Height of the new atlases file. 0 = get from original atlas file
        /// </summary>
        public int AtlasHeight { get; set; } = 0;

        /// <summary>
        /// Padding of the new atlases file (amount of pixels as a border between each image)
        /// </summary>
        public int Padding { get; set; } = 1;

        /// <summary>
        /// Creating an atlas file only with log base 2 numbers (16, 32, 64, etc.)
        /// </summary>
        public bool PowOf2 { get; set; } = false;

        /// <summary>
        /// Creating an atlas file only in a square (same width and height of atlas)
        /// </summary>
        public bool Square { get; set; } = false;

        /// <summary>
        /// allow to accept first successful image optimization layout
        /// </summary>
        public bool FastImagePacker { get; set; } = true;

        /// <summary>
        /// List of folders that could contain images to replace original images
        /// </summary>
        public List<string> ImageFolderList { get; set; } = new List<string>();

        /// <summary>
        /// The list of textures in each atlas
        /// </summary>
        public List<Texture> TextureList { get; set; } = new List<Texture>();
        
        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        /// <returns>The atlas file name</returns>
        public override string ToString()
        {
            return string.Format("AtlasFile: {0}", string.IsNullOrEmpty(AtlasFile) ? "(empty)" : AtlasFile);
        }
    }
}
