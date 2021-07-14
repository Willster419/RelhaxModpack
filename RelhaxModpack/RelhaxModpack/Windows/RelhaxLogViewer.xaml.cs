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
using System.Windows.Threading;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for RelhaxLogViewer.xaml
    /// </summary>
    public partial class RelhaxLogViewer : RelhaxWindow
    {
        public bool ViewerClosed { get; set; } = false;

        public bool HighPriorityLogViewer
        {
            get
            {
                return highPriorityLogViewer;
            }
            set
            {
                Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite -= OnLogMessageWriteLowPriority;
                Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite -= OnLogMessageWriteHighPriority;
                if (value)
                {
                    Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite += OnLogMessageWriteHighPriority;
                }
                else
                {
                    Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite += OnLogMessageWriteLowPriority;
                }
                highPriorityLogViewer = value;
            }
        }

        private bool highPriorityLogViewer;

        public RelhaxLogViewer(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
            highPriorityLogViewer = true;
            Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite += OnLogMessageWriteHighPriority;
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextbox.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite -= OnLogMessageWriteLowPriority;
            ViewerClosed = true;
        }

        private void OnLogMessageWriteLowPriority(object sender, LogMessageEventArgs e)
        {
            Dispatcher.InvokeAsync((Action)(() => { UpdateLogDisplay(e.Message, false); }), DispatcherPriority.Send);
        }

        private void OnLogMessageWriteHighPriority(object sender, LogMessageEventArgs e)
        {
            Dispatcher.Invoke((Action)(() => { UpdateLogDisplay(e.Message, true); }), DispatcherPriority.Normal);
        }

        private void UpdateLogDisplay(string text, bool allowUIUpdate)
        {
            LogTextbox.AppendText(text + Environment.NewLine);
            LogTextbox.ScrollToEnd();

            if (allowUIUpdate)
                UiUtils.AllowUIToUpdate();
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
