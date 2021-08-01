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
    public class RelhaxFeatureWindowWithChanges : RelhaxCustomFeatureWindow
    {
        protected bool UnsavedChanges = false;

        public RelhaxFeatureWindowWithChanges(ModpackSettings modpackSettings, Logfiles logfile) : base(modpackSettings, logfile)
        {

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (UnsavedChanges)
            {
                if (MessageBox.Show("You have unsaved changes, return to editor?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
    }
}
