using System.Windows.Media;
namespace RelhaxModpack.UIComponents
{
    /*
     * The PackageUIComponent class acts as a handler for when the enabled and checked properties are set from the Package
     * It can simplify the ModSelectionList code, clean it up, allow for uniform logic for all UI,
     * and allow for easy implimentation of another UI
     */
    public interface IPackageUIComponent
    {
        //the package that the UI component belongs to
        SelectablePackage Package { get; set; }
        void OnEnabledChanged(bool Enabled);
        void OnCheckedChanged(bool Checked);
        Brush TextColor { get; set; }
        Brush PanelColor { get; set; }
    }
}
