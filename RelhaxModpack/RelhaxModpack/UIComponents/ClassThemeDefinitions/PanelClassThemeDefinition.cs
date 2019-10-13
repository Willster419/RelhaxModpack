using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    public class PanelClassThemeDefinition : IClassThemeDefinition
    {
        public Type ClassType { get; } = typeof(Panel);

        //background
        public bool BackgroundAllowed { get; } = true;
        public string BackgroundBoundName { get; } = string.Empty;
        public DependencyProperty BackgroundDependencyProperty { get; } = Panel.BackgroundProperty;

        //foreground
        public bool ForegroundAllowed { get; } = false;
        public string ForegroundBoundName { get; } = string.Empty;
        public DependencyProperty ForegroundDependencyProperty { get; } = null;

        //highlight
        public bool HighlightAllowed { get; } = false;
        public string HighlightBoundName { get; } = string.Empty;

        //select
        public bool SelectAllowed { get; } = false;
        public string SelectBoundName { get; } = string.Empty;
    }
}
