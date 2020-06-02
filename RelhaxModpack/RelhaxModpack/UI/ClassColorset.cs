using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using RelhaxModpack.UI.ClassThemeDefinitions;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// A ClassColorset is a definitions object of all the colors to apply to the corresponding UI object
    /// </summary>
    public class ClassColorset
    {
        /// <summary>
        /// The theme definition rule set to use for this UI object class type (like Control)
        /// </summary>
        public IClassThemeDefinition ClassThemeDefinition { get; set; } = null;

        /// <summary>
        /// The Background color property
        /// </summary>
        public CustomBrush BackgroundBrush { get; set; } = null;

        /// <summary>
        /// The Foreground color property
        /// </summary>
        public CustomBrush ForegroundBrush { get; set; } = null;

        /// <summary>
        /// The Highlight color property
        /// </summary>
        public CustomBrush HighlightBrush { get; set; } = null;

        /// <summary>
        /// The Selected (mouse down) color property
        /// </summary>
        public CustomBrush SelectedBrush { get; set; } = null;

        /// <summary>
        /// Returns a list of brushes that are bounded to WPF UI definitions
        /// </summary>
        public List<CustomPropertyBrush> BoundedBrushes
        {
            get
            {
                List <CustomPropertyBrush> list = new List<CustomPropertyBrush>();
                if (BackgroundBrush != null && BackgroundBrush.IsValid && !string.IsNullOrEmpty(ClassThemeDefinition.BackgroundBoundName))
                    list.Add(new CustomPropertyBrush() {IsValid=BackgroundBrush.IsValid, Brush=BackgroundBrush.Brush, BrushPropertyName = ClassThemeDefinition.BackgroundBoundName });
                if (ForegroundBrush != null && ForegroundBrush.IsValid && !string.IsNullOrEmpty(ClassThemeDefinition.ForegroundBoundName))
                    list.Add(new CustomPropertyBrush() { IsValid = ForegroundBrush.IsValid, Brush = ForegroundBrush.Brush, BrushPropertyName = ClassThemeDefinition.ForegroundBoundName });
                if (HighlightBrush != null && HighlightBrush.IsValid && !string.IsNullOrEmpty(ClassThemeDefinition.HighlightBoundName))
                    list.Add(new CustomPropertyBrush() { IsValid = HighlightBrush.IsValid, Brush = HighlightBrush.Brush, BrushPropertyName = ClassThemeDefinition.HighlightBoundName });
                if (SelectedBrush != null && SelectedBrush.IsValid && !string.IsNullOrEmpty(ClassThemeDefinition.SelectBoundName))
                    list.Add(new CustomPropertyBrush() { IsValid = SelectedBrush.IsValid, Brush = SelectedBrush.Brush, BrushPropertyName = ClassThemeDefinition.SelectBoundName });
                return list;
            }
        }
    }
}
