using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace RelicModManager
{
    public partial class CRCCheck : Form
    {
        public CRCCheck()
        {
            InitializeComponent();
        }
        //handler to get the crc of the file
        private void button1_Click(object sender, EventArgs e)
        {
            //unable to find it in the registry, so ask for it
            if (openFileDialog1.ShowDialog().Equals(DialogResult.Cancel))
            {
                return;
            }
            MD5 hash = MD5.Create();
            string crc = this.GetMd5Hash(hash, openFileDialog1.FileName);
            textBox1.Text = crc;
        }
        //returns the md5 hash of the file based on the input file string location
        private string GetMd5Hash(MD5 md5Hash, string inputFile)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(inputFile));
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
    }
}
