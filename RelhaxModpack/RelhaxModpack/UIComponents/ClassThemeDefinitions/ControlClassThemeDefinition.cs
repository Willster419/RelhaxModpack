using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    public class ControlClassThemeDefinition : IClassThemeDefinition
    {
        public Type ClassType { get; } = typeof(Control);

        //background
        public bool BackgroundAllowed { get; } = true;
        public string BackgroundBoundName { get; } = string.Empty;
        public bool BackgroundAppliedLocal { get; } = false;
        public DependencyProperty BackgroundDependencyProperty { get; } = Control.BackgroundProperty;

        //foreground
        public bool ForegroundAllowed { get; } = true;
        public string ForegroundBoundName { get; } = string.Empty;
        public bool ForegroundAppliedLocal { get; } = false;
        public DependencyProperty ForegroundDependencyProperty { get; } = Control.ForegroundProperty;

        //highlight
        public bool HighlightAllowed { get; } = false;
        public string HighlightBoundName { get; } = string.Empty;

        //select
        public bool SelectAllowed { get; } = false;
        public string SelectBoundName { get; } = string.Empty;
    }
}
