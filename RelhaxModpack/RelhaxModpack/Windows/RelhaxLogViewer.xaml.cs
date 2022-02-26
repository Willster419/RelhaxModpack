using RelhaxModpack.Settings;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities.ClassEventArgs;
using RelhaxModpack.Utilities.Enums;
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
        /// <summary>
        /// If true, don't show debug level log messages in the window.
        /// </summary>
        public bool SuppressDebugMessages { get; set; } = true;

        /// <summary>
        /// Create an instance of the RelhaxLogViewer class.
        /// </summary>
        /// <param name="modpackSettings">The modpack settings object.</param>
        public RelhaxLogViewer(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
            StartLogListener_();
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            ClearLogWindow();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopLogListener_();
        }

        private void OnLogMessageWrite(object sender, LogMessageEventArgs e)
        {
            if (SuppressDebugMessages && e.LogLevel == LogLevel.Debug)
                return;

            Dispatcher.InvokeAsync((Action)(() => { UpdateLogDisplay(e.Message); }), DispatcherPriority.Send);
        }

        private void UpdateLogDisplay(string text)
        {
            LogTextbox.AppendText(text + Environment.NewLine);
            LogTextbox.ScrollToEnd();
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

        /// <summary>
        /// Use the dispatcher that created this window to clear the log text display.
        /// </summary>
        public void ClearLogWindow()
        {
            Dispatcher.Invoke((Action)(() => { LogTextbox.Clear(); }));
        }

        /// <summary>
        /// Use the dispatcher that created this window to start the log listener.
        /// </summary>
        /// <remarks>The windows's dispatcher is subscribed to the OnLogMessageWrite event, which when invoked asynchronously writes the log message to the log window.</remarks>
        /// <seealso cref="OnLogMessageWrite(object, LogMessageEventArgs)"/>
        public void StartLogListener()
        {
            Dispatcher.Invoke(() => StartLogListener_());
        }

        private void StartLogListener_()
        {
            Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite += OnLogMessageWrite;
        }

        /// <summary>
        /// Use the dispatcher that created this window to stop the log listener.
        /// </summary>
        /// <remarks>The window's dispatcher is un-subscribed from the OnLogMessageWriteEvent.</remarks>
        /// <seealso cref="OnLogMessageWrite(object, LogMessageEventArgs)"/>
        public void StopLogListener()
        {
            Dispatcher.Invoke(() => StopLogListener_());
        }

        private void StopLogListener_()
        {
            Logging.GetLogfile(Utilities.Enums.Logfiles.Application).OnLogfileWrite -= OnLogMessageWrite;
        }
    }
}
