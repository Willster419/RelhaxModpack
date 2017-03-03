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
    public partial class CRCFileSizeUpdate : Form
    {
        public CRCFileSizeUpdate()
        {
            InitializeComponent();
        }
        //returns the md5 hash of the file based on the input file string location
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

        private void loadDatabaseButton_Click(object sender, EventArgs e)
        {

        }

        private void loadZipFilesButton_Click(object sender, EventArgs e)
        {
            //check for database
            //show file dialog
            //load database
            //foreach zip file name
                //link the mod
                //get the crc value
                //get the file size
            //save config file
        }

        private void CRCFileSizeUpdate_Load(object sender, EventArgs e)
        {
            addZipsDialog.InitialDirectory = Application.StartupPath;

        }
    }
}
