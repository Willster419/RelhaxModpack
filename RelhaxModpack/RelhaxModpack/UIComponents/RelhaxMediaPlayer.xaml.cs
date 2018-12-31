using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;


namespace RelhaxModpack.UIComponents
{
    /// <summary>
    /// Interaction logic for RelhaxMediaPlayer.xaml
    /// </summary>
    public partial class RelhaxMediaPlayer : UserControl
    {

        private Timer UITimer = new Timer();
        IWavePlayer waveOutDevice = new WaveOut();
        MediaFoundationReader audioFileReader2;
        public string MediaURL { get; set; }

        public RelhaxMediaPlayer()
        {
            InitializeComponent();
        }

        public RelhaxMediaPlayer(string mediaURL)
        {
            InitializeComponent();
            MediaURL = mediaURL;
        }

        private async void OnComponentLoad(object sender, RoutedEventArgs e)
        {
            //tell the user it's loading the file
            FileName.Text = Translations.GetTranslatedString("loading");
            //use an async load
            await Task.Factory.StartNew(() =>
            {
                audioFileReader2 = new MediaFoundationReader(MediaURL);
                waveOutDevice.Init(audioFileReader2);
                waveOutDevice.Stop();
            });
            //now that it's loaded, setup the UI
            //https://stackoverflow.com/questions/10371741/naudio-seeking-and-navigation-to-play-from-the-specified-position
            Seekbar.Maximum = (int)audioFileReader2.TotalTime.TotalMilliseconds;
            FileName.Text = Path.GetFileName(MediaURL);
            //start off the volume at 50%
            Volume.Minimum = 0;
            Volume.Maximum = 100;
            Volume.Value = 50;
            VolumeNumber.Text = Volume.Value.ToString();
            waveOutDevice.Volume = (float)(Volume.Value / Volume.Maximum);// 50 / 100 = 0.5f
            waveOutDevice.PlaybackStopped += OnWaveDevicePlaybackStopped;
            //setup the UI timer to make the display updated based on position of audio
            UITimer.Interval = 100;
            UITimer.Elapsed += OnUITimerElapse;
            UITimer.Stop();
            UITimer.AutoReset = true;
        }

        private void OnUITimerElapse(object sender, ElapsedEventArgs e)
        {
            if (waveOutDevice.PlaybackState != PlaybackState.Playing)
                StopButton_Click(null, null);
            if (Seekbar.Minimum <= audioFileReader2.CurrentTime.TotalMilliseconds && audioFileReader2.CurrentTime.TotalMilliseconds <= Seekbar.Maximum)
                Seekbar.Value = (int)audioFileReader2.CurrentTime.TotalMilliseconds;
        }

        private void OnWaveDevicePlaybackStopped(object sender, StoppedEventArgs e)
        {
            //treat is as a stop (stop the audio)
            StopButton_Click(null, null);
        }

        private void OnVolumeScroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            waveOutDevice.Volume = (float)(Volume.Value / Volume.Maximum);
            VolumeNumber.Text = Volume.Value.ToString();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            waveOutDevice.Stop();
            UITimer.Stop();
            Seekbar.Value = 0;
            audioFileReader2.Position = 0;
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            switch (waveOutDevice.PlaybackState)
            {
                case PlaybackState.Stopped:
                case PlaybackState.Paused:
                    waveOutDevice.Play();
                    UITimer.Start();
                    break;
                case PlaybackState.Playing:
                    waveOutDevice.Pause();
                    UITimer.Stop();
                    break;
            }
        }

        private void OnSeekbarMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                return;
            //pause
            waveOutDevice.Pause();
            UITimer.Stop();
            //becuase of the unusable first 10 pixels or so
            //subtract out the area we can't use for scrolling. it's always constant (or very close)
            //TODO:test
            double mouseX = e.GetPosition(Seekbar).X;
            //get the total scroll bar usable length
            double scrollWidth = Seekbar.Width - 20;
            //make sure it's a positive number (border at beginning of scroll bar)
            if (mouseX < 0)
                mouseX = 0;
            //border at end of scroll bar
            if (mouseX > scrollWidth)
                mouseX = scrollWidth;
            //get the percent of where the seekbar is, 0-1 form
            double seekPos = mouseX / scrollWidth;
            //set the seekbar UI value to the scrolled location
            double newPos = Seekbar.Maximum * seekPos;
            Seekbar.Value = (int)newPos;
            audioFileReader2.CurrentTime = new TimeSpan(0, 0, 0, 0, (int)Seekbar.Value);
        }
    }
}
