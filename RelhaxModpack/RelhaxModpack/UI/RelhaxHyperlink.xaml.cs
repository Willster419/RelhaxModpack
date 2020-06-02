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

        public Brush MouseOverBackground { get; set; } = Brushes.Red;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ChildTextblock.Foreground = this.Foreground;
            TheHyperlink.Foreground = this.Foreground;
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

        private void ChildTextblock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChildTextblock.Foreground = MouseOverBackground;
            TheHyperlink.Foreground = MouseOverBackground;
        }

        private void ChildTextblock_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChildTextblock.Foreground = this.Foreground;
            TheHyperlink.Foreground = this.Foreground;
        }
    }
}
