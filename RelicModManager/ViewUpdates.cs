using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;

namespace RelhaxModpack
{
    public partial class ViewUpdates : Form
    {
        int x, y;
        // WebClient client = new WebClient();
        // string url = "";
        string zipFilename = "";
        string archivedFilename = "";
        int difference;
        private const int titleBar = 23;//set origionally for 23
        // public ViewUpdates(int xx, int yy, string urll)
        public ViewUpdates(int xx, int yy, string tZipFilename, string tArchivedFilename)
        {
            InitializeComponent();
            //parse in the new ints location of where to display the application
            x = xx;
            y = yy;
            zipFilename = tZipFilename;
            archivedFilename = tArchivedFilename;
            // url = urll;
        }

        private void ViewUpdates_Load(object sender, EventArgs e)
        {
            this.Location = new Point(x, y);
            //setting UI color
            Settings.setUIColor(this);
            //font scaling
            this.AutoScaleMode = Settings.appScalingMode;
            richTextBox1.Font = Settings.appFont;
            this.Font = Settings.appFont;
            if (Settings.appScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new SizeF(Settings.scaleSize, Settings.scaleSize));
            }
            //title bar height
            //get the size of the title bar window
            Rectangle screenRektangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRektangle.Top - this.Top;
            //largest possible is 46
            //mine (programmed for) is 23
            if (titleHeight > titleBar)
            {
                difference = titleHeight - titleBar;
            }

            string msgText = Utils.GetStringFromZip(zipFilename, archivedFilename);
            if (msgText.Equals(""))
            {
                richTextBox1.Text = "Error downloading data";
            }
            else
            {
                richTextBox1.Text = msgText;
            }

            /*
            client.DownloadStringCompleted += Client_DownloadStringCompleted;
            try
            {
                client.DownloadStringAsync(new Uri(url));
            }
            catch (WebException ex)
            {
                richTextBox1.Text = "Error downloading data";
                Utils.exceptionLog(ex);
            }
            */
            ViewUpdates_SizeChanged(null, null);
        }

        private void ViewUpdates_SizeChanged(object sender, EventArgs e)
        {
            richTextBox1.Size = new Size(this.Size.Width - 35, this.Size.Height - 65 - difference);
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        /*
        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                richTextBox1.Text = e.Result;
            }
            catch
            {
                richTextBox1.Text = e.Error.ToString();
                Utils.exceptionLog("ViewUpdates", "Client_DownloadStringCompleted", e.Error);
            }  
        }
        */
    }
}
