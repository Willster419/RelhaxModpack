using RelhaxModpack.Database;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// The PackageUIComponent class acts as a handler for when the enabled and checked properties are set from the Package.
    /// It can simplify the ModSelectionList code, clean it up, allow for uniform logic for all UI,
    /// and allow for easy implementation of another UI.
    /// </summary>
    public interface IPackageUIComponent
    {
        /// <summary>
        /// The package that the UI component belongs to
        /// </summary>
        SelectablePackage Package { get; set; }

        /// <summary>
        /// Method signature for when the enabled property changes
        /// </summary>
        /// <param name="Enabled">The value of the enabled property</param>
        void OnEnabledChanged(bool Enabled);

        /// <summary>
        /// Method signature for when the checked property changes
        /// </summary>
        /// <param name="Checked">The value of the checked property</param>
        void OnCheckedChanged(bool Checked);
    }
}
