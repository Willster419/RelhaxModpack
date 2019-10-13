using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RelhaxModpack.UIComponents.ClassThemeDefinitions
{
    public interface IClassThemeDefinition
    {
        Type ClassType { get; }

        //background
        bool BackgroundAllowed { get; }
        string BackgroundBoundName { get; }
        DependencyProperty BackgroundDependencyProperty { get; }

        //foreground
        bool ForegroundAllowed { get; }
        string ForegroundBoundName { get; }
        DependencyProperty ForegroundDependencyProperty { get; }

        //highlight
        bool HighlightAllowed { get; }
        string HighlightBoundName { get; }

        //select
        bool SelectAllowed { get; }
        string SelectBoundName { get; }
    }
}
