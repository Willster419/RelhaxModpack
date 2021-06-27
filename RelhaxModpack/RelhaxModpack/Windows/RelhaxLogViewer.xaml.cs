using RelhaxModpack.Settings;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities.ClassEventArgs;
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
    /// Interaction logic for RelhaxLogViewer.xaml
    /// </summary>
    public partial class RelhaxLogViewer : RelhaxWindow
    {

        public bool ViewerClosed { get; set; } = false;

        public RelhaxLogViewer(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
            Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite += OnLogMessageWrite;
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextbox.Clear();
        }

        private void LogTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogTextbox.ScrollToEnd();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite -= OnLogMessageWrite;
            ViewerClosed = true;
        }

        private void OnLogMessageWrite(object sender, LogMessageEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                LogTextbox.AppendText(e.Message + Environment.NewLine);
                UiUtils.AllowUIToUpdate();
            }));
        }

        private void ToggleWordWrapCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ToggleWordWrapCheckbox.IsChecked)
                LogTextbox.TextWrapping = TextWrapping.Wrap;
            else
                LogTextbox.TextWrapping = TextWrapping.NoWrap;
        }

        private void LogTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleWordWrapCheckbox_Click(null, null);
        }
    }
}
