#region MIT License

/*
 * Copyright (c) 2009 Nick Gravelyn (nick@gravelyn.com), Markus Ewald (cygon@nuclex.org)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software 
 * is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 * 
 */

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack.AtlasesCreator
{
    // writes out an XML file ready to be put into a Wargaming Atlases folder.
    public class WgXmlMapExporter : IMapExporter
    {
        public string MapExtension
        {
            get { return "xml"; }
        }

        public Atlas.MapTypes MapType
        {
            get { return Atlas.MapTypes.WGXmlMap; }
        }

        // parses a xmlAtlasesFile to the process queue
        public List<Texture> Load(string MapFile)
        {
            List<Texture> TextureList = new List<Texture>();
            try
            {
                //just in case
                XDocument doc = null;
                Texture t = null;
                doc = XDocument.Load(MapFile, LoadOptions.SetLineInfo);
                foreach (XElement texture in doc.XPathSelectElements("/root/SubTexture"))
                {
                    try
                    {
                        t = new Texture();
                        foreach (XElement item in texture.Elements())
                        {
                            try
                            {
                                switch (item.Name.ToString().ToLower())
                                {
                                    case "name":
                                        t.name = item.Value.ToString().Trim();
                                        break;
                                    case "x":
                                        t.x = int.Parse("0" + item.Value.ToString().Trim());
                                        break;
                                    case "y":
                                        t.y = int.Parse("0" + item.Value.ToString().Trim());
                                        break;
                                    case "width":
                                        t.width = int.Parse("0" + item.Value.ToString().Trim());
                                        break;
                                    case "height":
                                        t.height = int.Parse("0" + item.Value.ToString().Trim());
                                        break;
                                    default:
                                        Logging.Manager(string.Format("unexpected Item found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value));
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Utils.ExceptionLog("WgXmlMapExporter", "switch", ex);
                            }
                        }
                        TextureList.Add(t);
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("WgXmlMapExporter", "foreach item", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("WgXmlMapExporter", "foreach texture", ex);
            }
            return TextureList;
        }

        public void Save(string filename, Dictionary<string, Rectangle> map)
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
    }
}