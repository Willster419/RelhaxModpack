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

using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace RelhaxModpack.AtlasesCreator
{
    // writes out an XML file ready to be put into a XNA Content project and get compiled as content.
    // this file can be loaded using Content.Load<Dictionary<string, Rectangle>> from inside the game.
    public class XmlMapExporter : IMapExporter
    {
        public string MapExtension
        {
            get { return "xml"; }
        }

        public void Save(string filename, Dictionary<string, Rectangle> map)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                writer.WriteLine("<XnaContent>");
                writer.WriteLine("<Asset Type=\"System.Collections.Generic.Dictionary[System.String, Microsoft.Xna.Framework.Rectangle]\">");

                foreach (var entry in map)
                {
                    Rectangle r = entry.Value;
                    writer.WriteLine(string.Format(
                        "<Item><Key>{0}</Key><Value>{1} {2} {3} {4}</Value></Item>",
                        Path.GetFileNameWithoutExtension(entry.Key),
                        r.X,
                        r.Y,
                        r.Width,
                        r.Height));
                }

                writer.WriteLine("</Asset>");
                writer.WriteLine("</XnaContent>");
            }
        }
    }
}