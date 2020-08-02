
using RelhaxModpack.Database;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// The EditorComboBoxItem class is a wrapper class for displaying DatabaseObjects as combo box items. The ToString() allows for display of the internal PackageName property
    /// </summary>
    public class EditorComboBoxItem
    {
        /// <summary>
        /// The DatabasePackage object that is being wrapped around
        /// </summary>
        public DatabasePackage Package { get; set; }

        /// <summary>
        /// A wrapper property around the Package's PackageName object
        /// </summary>
        public string DisplayName
        {
            get
            {
                return Package == null ? "(null)" : Package.PackageName;
            }
        }

        /// <summary>
        /// Creates an instance of the EditorComboBoxItem class
        /// </summary>
        /// <param name="package">The package to wrap around</param>
        public EditorComboBoxItem(DatabasePackage package)
        {
            Package = package;
        }

        /// <summary>
        /// Allows for displaying of custom text in the Combobox
        /// </summary>
        /// <returns>The text to display in the Combobox (DisplayName property -> PackageName)</returns>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
