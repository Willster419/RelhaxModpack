using RelhaxModpack.Settings;
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
    /// Interaction logic for ScalingConfirmation.xaml
    /// </summary>
    public partial class ScalingConfirmation : RelhaxWindow
    {

        private int TimeToRevert = 10;

        private DispatcherTimer Timer = null;

        /// <summary>
        /// Create an instance of the ScalingConfirmation window
        /// </summary>
        public ScalingConfirmation(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ScalingConfirmationRevertTime.Text = string.Format(Translations.GetTranslatedString(ScalingConfirmationRevertTime.Name), TimeToRevert);
            Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Normal, Timer_Tick, this.Dispatcher);
            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ScalingConfirmationRevertTime.Text = string.Format(Translations.GetTranslatedString(ScalingConfirmationRevertTime.Name), --TimeToRevert);
            if(TimeToRevert <= 0)
            {
                Timer.Stop();
                DialogResult = false;
                Close();
            }
        }

        private void ScalingConfirmationDiscard_Click(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            DialogResult = false;
            Close();
        }

        private void ScalingConfirmationKeep_Click(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            DialogResult = true;
            Close();
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            if (DialogResult == null)
                DialogResult = false;

            if (Timer != null)
            {
                if (Timer.IsEnabled)
                    Timer.Stop();
                Timer = null;
            }

        }
    }
}
