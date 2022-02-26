using RelhaxModpack.Settings;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Represents a window where the user is performing operations that are logged and saved to disk (for example, database editing).
    /// This class provides an implementation to handle when a user wishes to close the window while he has changes that are not yet written to disk.
    /// </summary>
    public abstract class RelhaxFeatureWindowWithChanges : RelhaxCustomFeatureWindow
    {
        /// <summary>
        /// Flag to indicate if the user has performed operations that are not yet saved to disk.
        /// </summary>
        protected bool UnsavedChanges = false;

        /// <summary>
        /// Create an instance of a parent class, using the constructor of the RelhaxFeatureWindowWithChanges.
        /// </summary>
        /// <param name="modpackSettings">The modpack settings object.</param>
        /// <param name="logfile">The logfile enumeration to control which logfile to write to from this window.</param>
        public RelhaxFeatureWindowWithChanges(ModpackSettings modpackSettings, Logfiles logfile) : base(modpackSettings, logfile)
        {

        }

        /// <summary>
        /// Check if the user has unsaved changes, and if he does, confirm the user wants to close the window (without saving the changes).
        /// </summary>
        /// <param name="e">Provides data for a cancel-able event.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (UnsavedChanges)
            {
                if (MessageBox.Show("You have unsaved changes, are you sure you want to close the window?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
    }
}
