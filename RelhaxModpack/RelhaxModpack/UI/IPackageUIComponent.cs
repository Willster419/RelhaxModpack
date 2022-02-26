using RelhaxModpack.Database;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// The PackageUIComponent class acts as an implementation handler for when the enabled property is set from the Package object.
    /// It also contains a signature for the UI component to handle when the component should be highlighted in the selection list from a user search.
    /// </summary>
    public interface IPackageUIComponent : IOnCheckedComponent
    {
        /// <summary>
        /// Method signature for when the enabled property changes
        /// </summary>
        /// <param name="Enabled">The value of the enabled property</param>
        void OnEnabledChanged(bool Enabled);

        /// <summary>
        /// Gets or sets if the UI component should be highlighted in the selection view from a user search.
        /// </summary>
        bool IsHighlightedForView { get; set; }
    }
}
