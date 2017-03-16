using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace RelhaxModpack
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
            string crc = Settings.GetMd5Hash(openFileDialog1.FileName);
            crcTB.Text = crc;
        }
    }
}
