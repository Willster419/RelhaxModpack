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
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Settings;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for ProgressIndicator.xaml
    /// </summary>
    public partial class ProgressIndicator : RelhaxWindow
    {
        /// <summary>
        /// Gets or sets the minimum value of progress to display
        /// </summary>
        public double ProgressMinimum
        {
            get { return LoadingProgressBar.Minimum; }
            set { LoadingProgressBar.Minimum = value; }
        }

        /// <summary>
        /// Gets or sets the maximum value of progress to display
        /// </summary>
        public double ProgressMaximum
        {
            get { return LoadingProgressBar.Maximum; }
            set { LoadingProgressBar.Maximum = value; }
        }

        /// <summary>
        /// Gets or sets the progress message to display
        /// </summary>
        public string Message
        {
            get { return LoadingText.Text; }
            set { LoadingText.Text = value; }
        }

        /// <summary>
        /// Gets or sets the progress value to display
        /// </summary>
        public double ProgressValue
        {
            get { return LoadingProgressBar.Value; }
            set { LoadingProgressBar.Value = value; }
        }

        /// <summary>
        /// Creates an instance of the ProgressIndicator class
        /// </summary>
        public ProgressIndicator(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            if (ProgressMinimum != 0)
                Logging.WriteToLog("progress minimum value is not 0! (is this the intent?)", Logfiles.Application, LogLevel.Warning);
            if (ProgressMaximum <= 0)
            {
                Logging.WriteToLog("progress maximum is less than or equal to 0! (is this the intent?) setting to default of 100",
                    Logfiles.Application, LogLevel.Warning);
            }

            //only load the header text if the translations are loaded
            if(Translations.TranslationsLoaded)
            {
                LoadingHeader.Text = Translations.GetTranslatedString(LoadingHeader.Name);
            }
            else
            {
                LoadingHeader.Text = "Loading, please wait";
            }
        }

        /// <summary>
        /// Update the progress bar value
        /// </summary>
        /// <param name="percent">The progress report from minimum to maximum</param>
        public void UpdateProgress(double percent)
        {
            if(percent > LoadingProgressBar.Maximum)
            {
                Logging.WriteToLog(string.Format("percent value of {0} is greater than maximum set value of {1}, setting to maximum instead"
                    , percent, LoadingProgressBar.Maximum), Logfiles.Application, LogLevel.Warning);
                LoadingProgressBar.Value = LoadingProgressBar.Maximum;
            }
            else if (percent < 0)
            {
                Logging.WriteToLog(string.Format("percent value of {0} is less than {1}, setting to maximum instead", percent, LoadingProgressBar.Minimum),
                    Logfiles.Application, LogLevel.Warning);
                LoadingProgressBar.Value = LoadingProgressBar.Minimum;
            }
            else
            {
                LoadingProgressBar.Value = percent;
            }
        }

        /// <summary>
        /// Update the progress bar and message values
        /// </summary>
        /// <param name="percent">The progress report from minimum to maximum</param>
        /// <param name="updatedmessage">The message to display</param>
        public void UpdateProgress(double percent, string updatedmessage)
        {
            UpdateProgress(percent);
            Message = updatedmessage;
        }
    }
}
