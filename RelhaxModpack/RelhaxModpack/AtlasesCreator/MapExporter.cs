using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack.AtlasesCreator
{
    public class MapExporter
    {

        // parses a xmlAtlasesFile to the process queue
        public static List<Texture> Load(string mapFile)
        {
            //check to make sure file exists first
            if(!File.Exists(mapFile))
            {
                Logging.Error("Texture list does not exist in {0}", mapFile);
                return null;
            }

            //make the texture list to return later
            List<Texture> TextureList = new List<Texture>();

            //load the xml file
            XDocument doc = XMLUtils.LoadXDocument(mapFile, XmlLoadType.FromFile);
            if(doc == null)
            {
                Logging.Error("Failed to load xml texture list in {0}", mapFile);
                return null;
            }

            //parse the xml values into the texture type
            foreach (XElement xmlTexture in doc.XPathSelectElements("/root/SubTexture"))
            {
                Texture texture = new Texture();
                foreach (XElement item in xmlTexture.Elements())
                {
                    switch (item.Name.ToString().ToLower())
                    {
                        case "name":
                            texture.name = item.Value.ToString().Trim();
                            break;
                        case "x":
                            texture.x = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "y":
                            texture.y = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "width":
                            texture.width = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "height":
                            texture.height = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        default:
                            Logging.Error("unexpected Item found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value);
                            break;
                    }
                }
                TextureList.Add(texture);
            }
            return TextureList;
        }

        public static void Save(string filename, Dictionary<string, Rectangle> map)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("<root createdBy=\"Relhax ModPack Manager\" date=\"{0:yyyy-MM-dd HH:mm:ss.fff}\">", System.DateTime.Now);
                foreach (var entry in map.OrderBy(key => key.Key))
                {
                    Rectangle r = entry.Value;
                    writer.WriteLine(string.Format("\t<SubTexture>"));
                    writer.WriteLine(string.Format("\t\t<name> {0} </name>", Path.GetFileNameWithoutExtension(entry.Key)));
                    writer.WriteLine(string.Format("\t\t<x> {0} </x>", r.X));
                    writer.WriteLine(string.Format("\t\t<y> {0} </y>", r.Y));
                    writer.WriteLine(string.Format("\t\t<width> {0} </width>", r.Width));
                    writer.WriteLine(string.Format("\t\t<height> {0} </height>", r.Height));
                    writer.WriteLine(string.Format("\t</SubTexture>"));
                }
                writer.WriteLine("</root>");
            }
        }

        public static Dictionary<string, Rectangle> GenerateMapData(Dictionary<Texture, Rectangle> imagePlacement, Dictionary<Texture, Size> imageSizes)
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
