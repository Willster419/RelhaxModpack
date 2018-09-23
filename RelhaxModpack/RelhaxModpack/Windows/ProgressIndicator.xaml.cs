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
    /// Interaction logic for ProgressIndicator.xaml
    /// </summary>
    public partial class ProgressIndicator : RelhaxWindow
    {
        public double ProgressMinimum
        {
            get { return LoadingProgressBar.Minimum; }
            set { LoadingProgressBar.Minimum = value; }
        }

        public double ProgressMaximum
        {
            get { return LoadingProgressBar.Maximum; }
            set { LoadingProgressBar.Maximum = value; }
        }
        
        public string Message
        {
            get { return LoadingText.Text; }
            set { LoadingText.Text = value; }
        }

        public ProgressIndicator()
        {
            InitializeComponent();
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            //TODO: happends after propeties set?
            if (ProgressMinimum != 0)
                Logging.WriteToLog("progress minimum value is not 0! (is this the intent?)", Logfiles.Application, LogLevel.Warning);
            if (ProgressMaximum <= 0)
            {
                Logging.WriteToLog("progress maximum is less than or equal to 0! (is this the intent?) setting to default of 100",
                    Logfiles.Application, LogLevel.Warning);
            }
        }

        public void UpdateProgress(double percent)
        {
            if(percent > LoadingProgressBar.Maximum)
            {
                Logging.WriteToLog(string.Format("percent value of {0} is greator than maximu set value of {1}, setting to maxumum instead"
                    , percent, LoadingProgressBar.Maximum), Logfiles.Application, LogLevel.Warning);
                LoadingProgressBar.Value = LoadingProgressBar.Maximum;
            }
            else if (percent < 0)
            {
                Logging.WriteToLog(string.Format("percent value of {0} is less than 0, setting to maxumum instead", percent, LoadingProgressBar.Minimum),
                    Logfiles.Application, LogLevel.Warning);
                LoadingProgressBar.Value = LoadingProgressBar.Minimum;
            }
            else
            {
                LoadingProgressBar.Value = percent;
            }
        }

        public void UpdateProgress(double percent, string updatedmessage)
        {
            UpdateProgress(percent);
            Message = updatedmessage;
        }
    }
}
