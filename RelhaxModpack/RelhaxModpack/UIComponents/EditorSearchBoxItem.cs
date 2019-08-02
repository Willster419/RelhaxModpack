namespace RelhaxModpack.UIComponents
{
    /// <summary>
    /// The EditorSearchBoxItem class is a wrapper class for displaying DatabaseObjects as combo box items. The ToString() allows for display of the internal PackageName property
    /// </summary>
    public class EditorSearchBoxItem : System.Windows.Controls.ComboBoxItem
    {
        /// <summary>
        /// The DatabasePackage object that is being wrapped around
        /// </summary>
        public DatabasePackage Package { get; set; }

        /// <summary>
        /// The text to display in the Combobox
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Creates an instance of the EditorSearchBoxItem class
        /// </summary>
        /// <param name="package">The package to wrap around</param>
        /// <param name="display">The text to display in the Combobox</param>
        public EditorSearchBoxItem(DatabasePackage package, string display)
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
