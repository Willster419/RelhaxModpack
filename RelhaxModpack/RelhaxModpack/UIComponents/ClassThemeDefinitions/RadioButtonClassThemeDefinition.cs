using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    public class RadioButtonClassThemeDefinition : IClassThemeDefinition
    {
        public Type ClassType { get; } = typeof(RadioButton);

        //background
        public bool BackgroundAllowed { get; } = true;
        public string BackgroundBoundName { get; } = string.Empty;
        public DependencyProperty BackgroundDependencyProperty { get; } = RadioButton.BackgroundProperty;

        //foreground
        public bool ForegroundAllowed { get; } = true;
        public string ForegroundBoundName { get; } = string.Empty;
        public DependencyProperty ForegroundDependencyProperty { get; } = RadioButton.ForegroundProperty;

        //highlight
        public bool HighlightAllowed { get; } = true;
        public string HighlightBoundName { get; } = nameof(BoundUISettings.RadioButtonHighlightBrush);

        //select
        public bool SelectAllowed { get; } = true;
        public string SelectBoundName { get; } = nameof(BoundUISettings.RadioButtonCheckmarkBrush);
    }
}
