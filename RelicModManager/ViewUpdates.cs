using System;
using System.Drawing;
using System.Windows.Forms;

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
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
