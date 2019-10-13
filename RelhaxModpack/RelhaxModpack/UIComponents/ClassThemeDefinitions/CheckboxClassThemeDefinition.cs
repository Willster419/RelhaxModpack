using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    class CheckboxClassThemeDefinition : IClassThemeDefinition
    {
        public Type ClassType { get; } = typeof(CheckBox);

        //background
        public bool BackgroundAllowed { get; } = true;
        public string BackgroundBoundName { get; } = string.Empty;
        public bool BackgroundAppliedLocal { get; } = false;
        public DependencyProperty BackgroundDependencyProperty { get; } = CheckBox.BackgroundProperty;

        //foreground
        public bool ForegroundAllowed { get; } = true;
        public string ForegroundBoundName { get; } = string.Empty;
        public bool ForegroundAppliedLocal { get; } = false;
        public DependencyProperty ForegroundDependencyProperty { get; } = CheckBox.ForegroundProperty;

        //highlight
        public bool HighlightAllowed { get; } = true;
        public string HighlightBoundName { get; } = nameof(BoundUISettings.CheckboxHighlightBrush);

        //select
        public bool SelectAllowed { get; } = true;
        public string SelectBoundName { get; } = nameof(BoundUISettings.CheckboxCheckmarkBrush);
    }
}
