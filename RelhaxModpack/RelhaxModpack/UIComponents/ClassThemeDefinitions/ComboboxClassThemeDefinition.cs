using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    public class ComboboxClassThemeDefinition : IClassThemeDefinition
    {
        public Type ClassType { get; } = typeof(ComboBox);

        //background
        public bool BackgroundAllowed { get; } = true;
        public string BackgroundBoundName { get; } = nameof(BoundUISettings.ComboboxOutsideColorBrush);
        public bool BackgroundAppliedLocal { get; } = false;
        public DependencyProperty BackgroundDependencyProperty { get; } = null;

        //foreground
        public bool ForegroundAllowed { get; } = true;
        public string ForegroundBoundName { get; } = nameof(BoundUISettings.ComboboxInsideColorBrush);
        public bool ForegroundAppliedLocal { get; } = false;
        public DependencyProperty ForegroundDependencyProperty { get; } = null;

        //highlight
        public bool HighlightAllowed { get; } = true;
        public string HighlightBoundName { get; } = nameof(BoundUISettings.ComboboxOutsideHighlightBrush);

        //select
        public bool SelectAllowed { get; } = false;
        public string SelectBoundName { get; } = string.Empty;
    }
}
