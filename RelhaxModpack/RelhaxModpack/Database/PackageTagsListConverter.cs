using RelhaxModpack.Utilities.Enums;
using System;
using System.ComponentModel;
using System.Globalization;

namespace RelhaxModpack.Database
{
    /// <summary>
    /// Allows conversion of a string representation of PackageTagsList.
    /// </summary>
    public class PackageTagsListConverter : TypeConverter
    {
        /// <summary>
        /// Determines if this converter can convert the supplied type.
        /// </summary>
        /// <param name="context">The container property and descriptor. Can be null if unknown.</param>
        /// <param name="sourceType">The type of source to convert from.</param>
        /// <returns>True is the type is of string or can be converted from base, false otherwise.</returns>
        /// <remarks>See https://www.cyotek.com/blog/creating-a-custom-typeconverter-part-1 </remarks>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the string representation of a PackageTagsList to a PackageTagsList.
        /// </summary>
        /// <param name="context">The container property and descriptor. Can be null if unknown.</param>
        /// <param name="culture">The culture to account for when converting, like including the location of where this program is being run.</param>
        /// <param name="value">The string value of PackageTagsList.</param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            object result = null;

            //if it's not a string, it will be null
            if (value is string stringValue)
            {
                result = new PackageTagsList();
                foreach (string val in stringValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (Enum.TryParse(val, out PackageTags packageTag))
                        (result as PackageTagsList).Add(packageTag);
                }
            }

            return result ?? base.ConvertFrom(context, culture, value);
        }
    }
}