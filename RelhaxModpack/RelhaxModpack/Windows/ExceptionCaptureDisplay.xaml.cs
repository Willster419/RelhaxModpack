using RelhaxModpack.Settings;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for ExceptionCaptureDisplay.xaml
    /// </summary>
    public partial class ExceptionCaptureDisplay : RelhaxWindow
    {
        /// <summary>
        /// Create an instance of the ExceptionCaptureDisplay window
        /// </summary>
        public ExceptionCaptureDisplay(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Exception text to display
        /// </summary>
        public string ExceptionText
        {
            get { return ExceptionCaptureText.Text; }
            set { ExceptionCaptureText.Text = value; }
        }
    }
}
