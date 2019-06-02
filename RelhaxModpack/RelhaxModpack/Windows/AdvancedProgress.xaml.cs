using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RelhaxModpack.InstallerComponents;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack.Windows
{   ///I exist as a branch
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AdvancedProgress : RelhaxWindow
    {
        public AdvancedProgress()
        {
            InitializeComponent();
        }

        //make a bunch of handlers for referencing the install progress options later
        public RelhaxInstallTaskReporter BackupModsReporter = null;
        public RelhaxInstallTaskReporter BackupDataClearCacheClearLogsReporter = null;
        public RelhaxInstallTaskReporter CleanModsReporter = null;
        public RelhaxInstallTaskReporter[] ExtractionModsReporters;
        public RelhaxInstallTaskReporter ExtractionUserModsReporter = null;
        public RelhaxInstallTaskReporter RestoreDataXmlUnpackReporter = null;
        public RelhaxInstallTaskReporter PatchReporter = null;
        public RelhaxInstallTaskReporter ShortcutsFontsReporter = null;
        public RelhaxInstallTaskReporter AtlasReporter = null;

        public void OnReportAdvancedProgress(RelhaxInstallerProgress progress)
        {
            //sample of what you could do...
            switch(progress.InstallStatus)
            {

            }
        }
    }
}
