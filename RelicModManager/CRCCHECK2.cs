using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace RelicModManager
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
                MD5 hash = MD5.Create();
                sb.Append(Path.GetFileName(s) + System.Environment.NewLine);
                sb2.Append(GetMd5Hash(hash, s) + System.Environment.NewLine);
            }
            richTextBox1.Text = sb.ToString();
            richTextBox2.Text = sb2.ToString();
        }
        //returns a string of the MD5 hash of an object.
        //used to determine if a download is corrupted or not,
        //or if it needs to be updated
        private string GetMd5Hash(MD5 md5Hash, string inputFile)
        {
            // Convert the input string to a byte array and compute the hash.
            var stream = File.OpenRead(inputFile);
            byte[] data = md5Hash.ComputeHash(stream);
            stream.Close();
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        private void CRCCHECK2_Resize(object sender, EventArgs e)
        {
            //richTextBox1.Size = new Size(this.Size.Width - 32, this.Size.Height - 40 - richTextBox1.Location.Y);
        }
    }
}
