using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RelhaxModpack.UI.Extensions
{
    /// <summary>
    /// The FontExtensions class is a static class that enables extensions for the FontFamily class.
    /// </summary>
    public static class FontExtensions
    {
        /// <summary>
        /// Gets the name of the font without the font code and the '#' symbol.
        /// </summary>
        /// <returns>The name of the font, removing the font code after the '#' symbol.</returns>
        public static string FontName(this FontFamily fontFamily)
        {
            string[] fontNameArray = fontFamily.Source.Split('#');
            if (fontNameArray.Length > 1)
                return fontNameArray[1];
            else
                return fontNameArray[0];
        }
    }
}
