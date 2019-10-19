using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using RelhaxModpack.UIComponents.ClassThemeDefinitions;

namespace RelhaxModpack.UIComponents
{
    public class ClassColorset
    {
        public IClassThemeDefinition ClassThemeDefinition { get; set; } = null;

        public CustomBrush BackgroundBrush { get; set; } = null;

        public CustomBrush ForegroundBrush { get; set; } = null;

        public CustomBrush HighlightBrush { get; set; } = null;

        public CustomBrush SelectedBrush { get; set; } = null;

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
