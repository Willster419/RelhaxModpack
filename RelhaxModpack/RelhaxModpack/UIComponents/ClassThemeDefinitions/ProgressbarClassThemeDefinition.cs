using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    public class ProgressbarClassThemeDefinition : IClassThemeDefinition
    {
        public Type ClassType { get; } = typeof(ProgressBar);

        //background
        public bool BackgroundAllowed { get; } = true;
        public string BackgroundBoundName { get; } = string.Empty;

        public bool BackgroundAppliedLocal { get; } = true;
        public DependencyProperty BackgroundDependencyProperty { get; } = ProgressBar.BackgroundProperty;

        //foreground
        public bool ForegroundAllowed { get; } = true;
        public string ForegroundBoundName { get; } = string.Empty;
        public bool ForegroundAppliedLocal { get; } = true;
        public DependencyProperty ForegroundDependencyProperty { get; } = ProgressBar.ForegroundProperty;

        //highlight
        public bool HighlightAllowed { get; } = false;
        public string HighlightBoundName { get; } = string.Empty;

        //select
        public bool SelectAllowed { get; } = false;
        public string SelectBoundName { get; } = string.Empty;
    }
}
