using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class CRCCHECK2 : Form
    {
        public CRCCHECK2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Application.StartupPath;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            foreach (string s in files)
            {
                sb.Append(Path.GetFileName(s) + System.Environment.NewLine);
                sb2.Append(Utils.getMd5Hash(s) + System.Environment.NewLine);
            }
            richTextBox1.Text = sb.ToString();
            richTextBox2.Text = sb2.ToString();
        }

        private void CRCCHECK2_Resize(object sender, EventArgs e)
        {
            richTextBox1.Size = new Size(this.Size.Width - 32 - richTextBox2.Size.Width - 6, this.Size.Height - 40 - richTextBox1.Location.Y);
            richTextBox2.Location = new Point(this.Size.Width - 267, richTextBox2.Location.Y);
            richTextBox2.Size = new Size(richTextBox2.Size.Width, this.Size.Height - 40 - richTextBox1.Location.Y);
        }
    }
}
