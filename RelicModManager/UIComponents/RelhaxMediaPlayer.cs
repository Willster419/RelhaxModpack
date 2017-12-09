using System.Windows.Forms;
using WMPLib;

namespace RelhaxModpack
{
    public partial class RelhaxMediaPlayer : UserControl
    {
        private WindowsMediaPlayer wplayer = new WindowsMediaPlayer();
        private Timer UITimer = new Timer();
        public string MediaURL
        {
            get
            {
                return MediaURL;
            }
            set
            {
                wplayer.URL = value;
                wplayer.controls.stop();
                FileName.Text = System.IO.Path.GetFileName(value);
            }
        }
        public string StopText
        {
            get
            {
                return Stop.Text;
            }
            set
            {
                Stop.Text = value;
            }
        }
        public string PlayPauseText
        {
            get
            {
                return PlayPause.Text;
            }
            set
            {
                PlayPause.Text = value;
            }
        }
        public RelhaxMediaPlayer()
        {
            InitializeComponent();
            Volume.Value = 10;//volume seems to always start at 10...
            UITimer.Tick += UITimer_Tick;
            UITimer.Interval = 20;
            UITimer.Stop();
        }

        private void UITimer_Tick(object sender, System.EventArgs e)
        {
            if (wplayer.currentMedia.duration != 0)
            {
                double duration = wplayer.currentMedia.duration * 1000;
                Seekbar.Maximum = (int)duration;
                double newPos = wplayer.controls.currentPosition;
                newPos = newPos * 1000;
                if (newPos >= Seekbar.Minimum && Seekbar.Maximum >= newPos)
                {
                    Seekbar.Value = (int)newPos;
                }
                if (wplayer.playState == WMPPlayState.wmppsMediaEnded || wplayer.playState == WMPPlayState.wmppsStopped)
                {
                    Stop_Click(null, null);
                }
            }
            //FileName.Text = wplayer.playState.ToString();
            if(wplayer.currentMedia.duration == 0 && wplayer.playState == WMPPlayState.wmppsTransitioning)
            {
                //file not found (or at least some error)
                Stop_Click(null, null);
                FileName.Text = "ERROR";
            }
        }

        private void Volume_Scroll(object sender, System.EventArgs e)
        {
            wplayer.settings.volume = Volume.Value * 10;
        }
        //to process a grab/drag
        private void Seekbar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            //pause
            wplayer.controls.pause();
            UITimer.Stop();
            double mouseX = e.X - 10;//becuase of the unusable first 10 pixels or so
            //subtract out the area we can't use for scrolling. it's always constant (or very close)
            double scrollWidth = Seekbar.Size.Width - 23;
            //make sure it's a positive number
            if (mouseX < 0)
                mouseX = 0;
            if (mouseX > scrollWidth)
                mouseX = scrollWidth;
            //get the percent of where the seekbar is is 0-1 form
            double seekPos = mouseX / scrollWidth;
            //set the seekbar value to closest location
            double newPos = Seekbar.Maximum * seekPos;
            Seekbar.Value = (int)newPos;
            //set the player space to the scrolled location
            double newPos2 = Seekbar.Value;
            newPos2 = newPos2 / 1000;
            wplayer.controls.currentPosition = newPos2;
        }
        //to process a single click
        private void Seekbar_MouseDown(object sender, MouseEventArgs e)
        {
            Seekbar_MouseMove(sender, e);
        }

        private void Stop_Click(object sender, System.EventArgs e)
        {
            wplayer.controls.stop();
            UITimer.Stop();
            Seekbar.Value = 0;
        }

        private void PlayPause_Click(object sender, System.EventArgs e)
        {
            if (wplayer.playState == WMPPlayState.wmppsPlaying)
            {
                //pause
                wplayer.controls.pause();
                UITimer.Stop();
            }
            else
            {
                //play
                wplayer.controls.play();
                UITimer.Start();
            }
        }

        private void RelhaxMediaPlayer_SizeChanged(object sender, System.EventArgs e)
        {
            //give the table panel a 3 pixel border
            MediaPlayerLayout.Location = new System.Drawing.Point(3, 3);
            MediaPlayerLayout.Size = new System.Drawing.Size(this.Size.Width - 6, this.Size.Height - 6);
            //determine if we need to adjust the seekbar
        }
    }
}
