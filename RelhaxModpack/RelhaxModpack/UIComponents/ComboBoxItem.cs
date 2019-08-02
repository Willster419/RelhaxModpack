namespace RelhaxModpack.UIComponents
{
    /// <summary>
    /// The ComboBoxItem class is a wrapper class for displaying SelectablePackages as combo box items. The ToString() allows for display of the package name
    /// </summary>
    public class ComboBoxItem : System.Windows.Controls.ComboBoxItem
    {
        /// <summary>
        /// The SelectablePackage object that is being wrapped around
        /// </summary>
        public SelectablePackage Package { get; set; }

        /// <summary>
        /// The text to display in the Combobox
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Creates an instance of the ComboBoxItem class
        /// </summary>
        /// <param name="package">The package to wrap around</param>
        /// <param name="display">The text to display in the Combobox</param>
        public ComboBoxItem(SelectablePackage package, string display)
        {
            Package = package;
            DisplayName = display;
        }

        /// <summary>
        /// Allows for displaying of custom text in the Combobox
        /// </summary>
        /// <returns>The text to display in the Combobox (DisplayName property)</returns>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
