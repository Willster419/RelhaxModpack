using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RelhaxModpack.AtlasesCreator
{
    public class CreateOutputMapData
    {
        public static Dictionary<string, Rectangle> generateMapData(Dictionary<Texture, Rectangle> imagePlacement, Dictionary<Texture, Size> imageSizes)
        {
            // go through our image placements and replace the width/height found in there with
            // each image's actual width/height (since the ones in imagePlacement will have padding)
            Texture[] keys = new Texture[imagePlacement.Keys.Count];
            imagePlacement.Keys.CopyTo(keys, 0);

            foreach (var k in keys)
            {
                // get the actual size
                Size s = imageSizes[k];

                // get the placement rectangle
                Rectangle r = imagePlacement[k];

                // set the proper size
                r.Width = s.Width;
                r.Height = s.Height;

                // insert back into the dictionary
                imagePlacement[k] = r;
            }

            // copy the placement dictionary to the output
            Dictionary<string, Rectangle> outputMap = new Dictionary<string, Rectangle>();
            foreach (var pair in imagePlacement)
            {
                outputMap.Add(pair.Key.name, pair.Value);
            }
            return outputMap;
        }
    }
}
