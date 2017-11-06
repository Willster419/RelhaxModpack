using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack.AtlasesExtrator
{
    public class Program
    {
        public static void Main()
        {
            string ImageFile = @"C:\Users\Ich\Desktop\Test Atlases\battleAtlas\battleAtlas.png";
            string MapFile = @"C:\Users\Ich\Desktop\Test Atlases\battleAtlas\battleAtlas.xml";

            string workingFolder = Path.Combine(@"D:\RelHax Manager\RelHaxTemp", Path.GetFileNameWithoutExtension(ImageFile));
            Bitmap atlasImage = null;

            if (!File.Exists(ImageFile))
            {
                Utils.AppendToLog("ERROR. Atlas file not found: " + ImageFile);
                return;
            }
            if (!File.Exists(MapFile))
            {
                Utils.AppendToLog("ERROR. Map file not found: " + MapFile);
                return;
            }

            if (Directory.Exists(workingFolder))
                Directory.Delete(workingFolder, true);
            Directory.CreateDirectory(workingFolder);

            try
            {
                atlasImage = new Bitmap(ImageFile);
                List<Texture> textureList = new List<Texture>();
                try
                {
                    XDocument doc = null;
                    doc = XDocument.Load(MapFile, LoadOptions.SetLineInfo);
                    foreach (XElement texture in doc.XPathSelectElements("/root/SubTexture"))
                    {
                        try
                        {
                            Texture t = new Texture();
                            foreach (XElement item in texture.Elements())
                            {
                                try
                                {
                                    switch (item.Name.ToString().ToLower())
                                    {
                                        case "name":
                                            t.name = item.Value;
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
                                            Utils.AppendToLog(string.Format("unexpected Item found. Name: {0}  Value: {1}", item.Name.ToString(), item.Value));
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Utils.ExceptionLog("Main", "switch", ex);
                                }
                            }
                            textureList.Add(t);
                        }
                        catch (Exception ex)
                        {
                            Utils.ExceptionLog("Main", "foreach item", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("Main", "foreach root", ex);
                }
                Utils.AppendToLog("Parsed Textures: " + textureList.Count);
                int c = 0;
                foreach (Texture t in textureList)
                {
                    try
                    {
                        Bitmap CroppedImage = atlasImage.Clone(new System.Drawing.Rectangle(t.x, t.y, t.width, t.height), atlasImage.PixelFormat);
                        CroppedImage.Save(Path.Combine(workingFolder, Path.GetFileNameWithoutExtension(t.name) + ".png"), ImageFormat.Png);
                        CroppedImage.Dispose();
                        c++;
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("Main", "CroppedImage", ex);
                    }
                }
                Utils.AppendToLog(string.Format("Extracted Textures: {0}  {1}", c, c == textureList.Count ? "(all successfully done)" : "(missed some, why?)"));
            }
            finally
            {
                atlasImage.Dispose();
            }
        }

    }
}
