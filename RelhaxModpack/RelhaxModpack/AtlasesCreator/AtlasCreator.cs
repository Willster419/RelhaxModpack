using RelhaxModpack.AtlasesCreator.Packing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.AtlasesCreator
{
    public enum FailCode
    {
        None = 0,
        FailedParsingArguments,
        ImageExporter,
        MapExporter,
        NoImages,
        ImageNameCollision,

        FailedToLoadImage,
        FailedToPackImage,
        FailedToCreateImage,
        FailedToSaveImage,
        FailedToSaveMap
    }

    public class AtlasCreator
    {

        public FailCode CreateAtlas(Atlas atlas)
        {
            //parse input
            Logging.Debug("Running instance of AtlasCreator in CreateAtlas() atlas={0}",atlas.AtlasFile);

            //generate the image packer object and make the image
            ImagePacker imagePacker = new ImagePacker();

            // pack the image, generating a map only if desired
            FailCode result = imagePacker.PackImage(atlas.TextureList, atlas.PowOf2, atlas.Square, atlas.FastImagePacker, atlas.AtlasWidth, atlas.AtlasHeight,
                atlas.Padding, out Bitmap outputImage, out Dictionary<string, Rectangle> outputMap);
            if (result != 0)
            {
                Logging.Error("There was an error making the image sheet.");
                //error result 7 = "failed to pack image" most likely it won't fit
                return result;
            }
            else
            {
                Logging.Info(string.Format("Success Packing '{0}' to {1} x {2} pixel", Path.GetFileName(atlas.AtlasFile), outputImage.Height, outputImage.Width));
            }

            //export it using map exporter
            if (File.Exists(atlas.AtlasFile))
                File.Delete(atlas.AtlasFile);

            //save to a dds file
            if (ImageHandler.SaveDDS(atlas.AtlasFile, outputImage,true))
                Logging.Info("successfully created Atlas image: " + atlas.AtlasFile);
            else
            {
                Logging.Error("Failed to save bitmap to dds image");
                return FailCode.FailedToSaveImage;
            }

            return FailCode.None;
        }
    }
}
