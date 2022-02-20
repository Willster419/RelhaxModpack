using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Interaction logic for RelhaxHyperlink.xaml
    /// </summary>
    public partial class RelhaxHyperlink : UserControl
    {
        /// <summary>
        /// Create an instance of RelhaxHyperlink
        /// </summary>
        public RelhaxHyperlink()
        {
            InitializeComponent();
            this.Text = string.Empty;
        }

        /// <summary>
        /// Create an instance of RelhaxHyperlink
        /// </summary>
        /// <param name="URL">The URL to open when the hyperlink is clicked</param>
        /// <param name="text">The text of the hyperlink</param>
        public RelhaxHyperlink(string URL, string text)
        {
            InitializeComponent();
            this.Text = text;
            this.URL = URL;
        }

        /// <summary>
        /// Get or set the link URL
        /// </summary>
        public string URL { get; set; } = string.Empty;

        /// <summary>
        /// Get or set the link text
        /// </summary>
        public string Text
        {
            get { return ChildTextblock.Text; }
            set { ChildTextblock.Text = value; }
        }

        private void TheHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(URL))
                return;

            try
            { System.Diagnostics.Process.Start(URL); }
            catch (Exception ex)
            { Logging.Exception(ex.ToString()); }
        }
    }
}
