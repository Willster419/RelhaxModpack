using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    public class ButtonClassThemeDefinition : IClassThemeDefinition
    {
        public Type ClassType { get; } = typeof(Button);

        //background
        public bool BackgroundAllowed { get; } = true;
        public string BackgroundBoundName { get; } = string.Empty;
        public bool BackgroundAppliedLocal { get; } = false;
        public DependencyProperty BackgroundDependencyProperty { get; } = Button.BackgroundProperty;

        //foreground
        public bool ForegroundAllowed { get; } = true;
        public string ForegroundBoundName { get; } = string.Empty;
        public bool ForegroundAppliedLocal { get; } = false;
        public DependencyProperty ForegroundDependencyProperty { get; } = Button.ForegroundProperty;

        //highlight
        public bool HighlightAllowed { get; } = true;
        public string HighlightBoundName { get; } = nameof(BoundUISettings.ButtonHighlightBrush);

        //select
        public bool SelectAllowed { get; } = false;
        public string SelectBoundName { get; } = string.Empty;
    }
}
