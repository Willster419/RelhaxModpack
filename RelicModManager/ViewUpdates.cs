using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;

namespace RelhaxModpack
{
    public partial class ViewUpdates : RelhaxForum
    {
        int x, y;
        string zipFilename = "";
        string archivedFilename = "";
        public ViewUpdates(int xx, int yy, string tZipFilename, string tArchivedFilename)
        {
            InitializeComponent();
            //parse in the new ints location of where to display the application
            x = xx;
            y = yy;
            zipFilename = tZipFilename;
            archivedFilename = tArchivedFilename;
        }

        private void ViewUpdates_Load(object sender, EventArgs e)
        {
            this.Location = new Point(x, y);
            richTextBox1.Font = Settings.AppFont;
            string msgText = Utils.GetStringFromZip(zipFilename, archivedFilename);
            if (msgText.Equals(""))
            {
                richTextBox1.Text = "Error downloading data";
            }
            else
            {
                richTextBox1.Text = msgText;
            }
            ViewUpdates_SizeChanged(null, null);
        }

        private void ViewUpdates_SizeChanged(object sender, EventArgs e)
        {
            richTextBox1.Size = new Size(this.Size.Width - 35, this.Size.Height - 65 - TitleBarDifference);
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
