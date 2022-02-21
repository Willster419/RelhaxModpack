using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Windows.Threading;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Interaction logic for RelhaxMediaPlayer.xaml
    /// </summary>
    public partial class RelhaxMediaPlayer : UserControl, IDisposable
    {
        //public
        /// <summary>
        /// The direct link to the audio file to preview
        /// </summary>
        private string MediaURL = string.Empty;

        /// <summary>
        /// The raw audio data to parse
        /// </summary>
        private byte[] AudioData = null;

        //private
        private DispatcherTimer UITimer_ = null;
        private IWavePlayer waveOutDevice = new WaveOut();
        private MemoryStream audioStream = null;
        private WaveStream audioFileReader2 = null;

        /// <summary>
        /// Creates an instance of the RelhaxMediaPlayer user control
        /// </summary>
        /// <param name="_audioData">The audio data to use in the preview</param>
        /// <param name="_mediaURL">The URL to the audio source. Used for audio type parsing.</param>
        public RelhaxMediaPlayer( string _mediaURL, byte[] _audioData)
        {
            MediaURL = _mediaURL;
            AudioData = _audioData;
            InitializeComponent();
        }

        private async void OnComponentLoad(object sender, RoutedEventArgs e)
        {
            if (AudioData == null)
                throw new BadMemeException("lol you forgot to set the audio data");
            if (string.IsNullOrWhiteSpace(MediaURL))
                throw new BadMemeException("lol you forgot to pass in the Media URL");

            //tell the user it's loading the file
            FileName.Text = Translations.GetTranslatedString("loading");

            //use an async load
            bool taskComplete = false;
            await Task.Run(() =>
            {
                try
                {
                    audioStream = new MemoryStream(AudioData);
                    switch (Path.GetExtension(MediaURL).ToLower())
                    {
                        case ".mp3":
                            audioFileReader2 = new Mp3FileReader(audioStream);
                            break;
                        case ".wav":
                            audioFileReader2 = new WaveFileReader(audioStream);
                            break;
                        default:
                            throw new NotSupportedException("wave and mp3 only k thanks");
                    }
                    waveOutDevice.Init(audioFileReader2);
                    waveOutDevice.Stop();
                    taskComplete = true;
                }
                catch (Exception ex)
                {
                    Logging.Exception("Failed to load audio preview: {0}", MediaURL);
                    Logging.Exception(ex.ToString());
                }
            });
            if (!taskComplete)
            {
                FileName.Text = "ERROR";
                StopButton.IsEnabled = false;
                PlayPause.IsEnabled = false;
                Seekbar.IsEnabled = false;
                Volume.IsEnabled = false;
                return;
            }

            //now that it's loaded, setup the UI
            //https://stackoverflow.com/questions/10371741/naudio-seeking-and-navigation-to-play-from-the-specified-position
            Seekbar.Maximum = (int)audioFileReader2.TotalTime.TotalMilliseconds;
            FileName.Text = Path.GetFileName(MediaURL);

            //start off the volume at 50%
            Volume.Minimum = 0;
            Volume.Maximum = 100;
            Volume.Value = 50;
            VolumeNumber.Text = Volume.Value.ToString("F1");
            waveOutDevice.Volume = (float)(Volume.Value / Volume.Maximum);// 50 / 100 = 0.5f
            waveOutDevice.PlaybackStopped += OnWaveDevicePlaybackStopped;

            //setup the UI timer to make the display updated based on position of audio
            UITimer_ = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, OnUITimerElapse, this.Dispatcher) { IsEnabled = false };
        }

        private void OnUITimerElapse(object sender, EventArgs e)
        {
            if (waveOutDevice == null)
            {
                UITimer_.Stop();
                return;
            }
            if (waveOutDevice.PlaybackState != PlaybackState.Playing)
                StopButton_Click(null, null);

            if (Seekbar.Minimum <= audioFileReader2.CurrentTime.TotalMilliseconds && audioFileReader2.CurrentTime.TotalMilliseconds <= Seekbar.Maximum)
                Seekbar.Value = (int)audioFileReader2.CurrentTime.TotalMilliseconds;
        }

        /// <summary>
        /// Stops playback from an outside source, like if changing previews
        /// </summary>
        public void StopPlaybackIfPlaying()
        {
            if (waveOutDevice.PlaybackState == PlaybackState.Playing)
                StopButton_Click(null, null);
        }

        private void OnWaveDevicePlaybackStopped(object sender, StoppedEventArgs e)
        {
            //treat is as a stop (stop the audio)
            StopButton_Click(null, null);
        }

        private void OnVolumeScroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            waveOutDevice.Volume = (float)(Volume.Value / Volume.Maximum);
            VolumeNumber.Text = Volume.Value.ToString("F1");
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            waveOutDevice.Stop();
            UITimer_.Stop();
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
                    UITimer_.Start();
                    break;
                case PlaybackState.Playing:
                    waveOutDevice.Pause();
                    UITimer_.Stop();
                    break;
            }
        }

        private void OnSeekbarMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                return;

            //because of the unusable first 10 pixels or so
            //subtract out the area we can't use for scrolling. it's always constant (or very close)
            ProcessMouseMove(e.GetPosition(Seekbar).X);
        }

        private void Seekbar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                return;

            //because of the unusable first 10 pixels or so
            //subtract out the area we can't use for scrolling. it's always constant (or very close)
            ProcessMouseMove(e.GetPosition(Seekbar).X);
        }

        //processes mouse scrolling
        private void ProcessMouseMove(double mouseX)
        {
            //pause
            waveOutDevice.Pause();
            UITimer_.Stop();

            //get the total scroll bar usable length
            double scrollWidth = Seekbar.ActualWidth - 20;

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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose of the RelhaxMediaPlayer references
        /// </summary>
        /// <param name="disposing">True to dispose of managed objects</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    waveOutDevice.Dispose();
                    waveOutDevice = null;
                    audioStream.Dispose();
                    audioStream = null;
                    audioFileReader2.Dispose();
                    audioFileReader2 = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RelhaxMediaPlayer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Dispose of the RelhaxMediaPlayer references
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
