using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Wraps a Brush class with a boolean value for if the brush value should be applied
    /// </summary>
    /// <remarks>The implementation reasoning for this is that some color definitions can be a null color.
    /// So, a custom property must be used outside of the Brush object to determine if it should actually be applied.</remarks>
    public class CustomBrush
    {
        /// <summary>
        /// The custom color definition
        /// </summary>
        public Brush Brush { get; set; } = null;

        /// <summary>
        /// The flag to determine if the Brush is valid
        /// </summary>
        public bool IsValid { get; set; } = false;
    }

    /// <summary>
    /// A wrapper class for CustomBrush that adds the string name of the WPF databound property name
    /// </summary>
    public class CustomPropertyBrush : CustomBrush
    {
        /// <summary>
        /// The name of the WPF databound property to change the color of
        /// </summary>
        public string BrushPropertyName { get; set; } = string.Empty;
    }
}
