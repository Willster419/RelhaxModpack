using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RelhaxModpack.UI.Extensions
{
    public static class FontExtensions
    {
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
