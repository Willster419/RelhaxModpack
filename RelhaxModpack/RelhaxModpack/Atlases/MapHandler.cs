using RelhaxModpack.Xml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Atlases
{
    /// <summary>
    /// A class for handling WG xml atlas map files.
    /// </summary>
    public class MapHandler
    {
        /// <summary>
        /// Loads a WG atlas map file into a texture list.
        /// </summary>
        /// <param name="mapFile">The map file to load.</param>
        /// <returns>The texture list if success, null otherwise.</returns>
        public List<Texture> LoadMapFile(string mapFile)
        {
            //check to make sure file exists first
            if (!File.Exists(mapFile))
            {
                Logging.Error("Texture list does not exist in {0}", mapFile);
                return null;
            }

            //load the xml file
            XDocument doc = XmlUtils.LoadXDocument(mapFile, XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Error("Failed to load xml texture list in {0}", mapFile);
                return null;
            }

            //make the texture list to return later
            List<Texture> TextureList = new List<Texture>();

            //parse the xml values into the texture type
            foreach (XElement xmlTexture in doc.XPathSelectElements("/root/SubTexture"))
            {
                Texture texture = new Texture();
                foreach (XElement item in xmlTexture.Elements())
                {
                    switch (item.Name.ToString().ToLower())
                    {
                        case "name":
                            texture.Name = item.Value.ToString().Trim();
                            break;
                        case "x":
                            texture.X = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "y":
                            texture.Y = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "width":
                            texture.Width = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        case "height":
                            texture.Height = int.Parse("0" + item.Value.ToString().Trim());
                            break;
                        default:
                            Logging.Error("Unexpected Item found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value);
                            break;
                    }
                }
                TextureList.Add(texture);
            }
            return TextureList;
        }

        /// <summary>
        /// Saves a map dictionary to the WG map xml file.
        /// </summary>
        /// <param name="filename">The location to save the file.</param>
        /// <param name="map">The Dictionary of each image name (key) and location (value) on the atlas file.</param>
        public void SaveMapfile(string filename, Dictionary<string, Rectangle> map)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("<root createdBy=\"Relhax Modpack\" date=\"{0:yyyy-MM-dd HH:mm:ss.fff}\">", System.DateTime.Now);
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
    }
}
