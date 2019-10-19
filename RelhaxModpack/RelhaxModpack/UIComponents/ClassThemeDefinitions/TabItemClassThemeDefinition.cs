using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    class TabItemClassThemeDefinition : IClassThemeDefinition
    {
        public Type ClassType { get; } = typeof(TabItem);

        //background
        public bool BackgroundAllowed { get; } = true;
        public string BackgroundBoundName { get; } = string.Empty;
        public bool BackgroundAppliedLocal { get; } = false;
        public DependencyProperty BackgroundDependencyProperty { get; } = TabItem.BackgroundProperty;

        //foreground
        public bool ForegroundAllowed { get; } = true;
        public string ForegroundBoundName { get; } = string.Empty;
        public bool ForegroundAppliedLocal { get; } = false;
        public DependencyProperty ForegroundDependencyProperty { get; } = TabItem.ForegroundProperty;

        //highlight
        public bool HighlightAllowed { get; } = true;
        public string HighlightBoundName { get; } = nameof(BoundUISettings.TabItemHighlightBrush);

        //select
        public bool SelectAllowed { get; } = true;
        public string SelectBoundName { get; } = nameof(BoundUISettings.TabItemSelectedBrush);
    }
}
