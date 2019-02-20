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

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseEditor.xaml
    /// </summary>
    public partial class DatabaseEditor : RelhaxWindow
    {

        private EditorSettings EditorSettings;

        public DatabaseEditor()
        {
            InitializeComponent();
        }

        private void OnApplicationLoad(object sender, RoutedEventArgs e)
        {
            EditorSettings = new EditorSettings();
            Logging.Info("Loading editor settings");
            if(!Settings.LoadSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null,EditorSettings))
            {
                Logging.Info("Failed to load editor settings, using defaults");
            }
            else
            {
                Logging.Info("Editor settings loaded success");
            }
        }

        private void OnApplicationClose(object sender, EventArgs e)
        {
            if (!Logging.IsLogDisposed(Logfiles.Application))
            {
                Logging.WriteToLog("Saving editor settings");
                if (Settings.SaveSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null, EditorSettings))
                    Logging.WriteToLog("Editor settings saved");
            }
        }
    }
}
