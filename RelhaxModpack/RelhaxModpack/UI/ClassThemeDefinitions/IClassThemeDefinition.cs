using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RelhaxModpack.UI.ClassThemeDefinitions
{
    /// <summary>
    /// The UI class theme interface definition for holding all properties of theme definitions
    /// </summary>
    public interface IClassThemeDefinition
    {
        /// <summary>
        /// The Type object that corresponds to the instanced UI class
        /// </summary>
        Type ClassType { get; }

        //background
        /// <summary>
        /// Determines if Background color is allowed to change
        /// </summary>
        bool BackgroundAllowed { get; }

        /// <summary>
        /// The name of the component for WPF databinding. An empty string indicates the component is not bound.
        /// </summary>
        string BackgroundBoundName { get; }

        /// <summary>
        /// Determines if the color is applied locally as to not interfere with dependency source ordering
        /// </summary>
        /// <remarks>See https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/dependency-property-value-precedence#setcurrentvalue </remarks>
        bool BackgroundAppliedLocal { get; }

        /// <summary>
        /// The reference to the Background DependencyProperty (if exists) of this class type for setting the value locally
        /// </summary>
        DependencyProperty BackgroundDependencyProperty { get; }

        //foreground
        /// <summary>
        /// Determines if Foreground color is allowed to change
        /// </summary>
        bool ForegroundAllowed { get; }

        /// <summary>
        /// The name of the component for WPF databinding. An empty string indicates the component is not bound.
        /// </summary>
        string ForegroundBoundName { get; }

        /// <summary>
        /// Determines if the color is applied locally as to not interfere with dependency source ordering
        /// </summary>
        /// <remarks>See https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/dependency-property-value-precedence#setcurrentvalue </remarks>
        bool ForegroundAppliedLocal { get; }

        /// <summary>
        /// The reference to the Foreground DependencyProperty (if exists) of this class type for setting the value locally
        /// </summary>
        DependencyProperty ForegroundDependencyProperty { get; }

        //highlight
        /// <summary>
        /// Determines if Highlight color is allowed to change. Highlight can only be changed by WPF data-bind
        /// </summary>
        bool HighlightAllowed { get; }

        /// <summary>
        /// The name of the component for WPF databinding. An empty string indicates the component is not bound.
        /// </summary>
        string HighlightBoundName { get; }

        //select
        /// <summary>
        /// Determines if Select (mouse button down color) color is allowed to change. Select can only be changed by WPF data-bind
        /// </summary>
        bool SelectAllowed { get; }

        /// <summary>
        /// The name of the component for WPF databinding. An empty string indicates the component is not bound.
        /// </summary>
        string SelectBoundName { get; }
    }
}
