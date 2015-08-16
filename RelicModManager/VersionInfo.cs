using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelicModManager
{
    public partial class VersionInfo : Form
    {

        public bool checkForUpdates;

        public VersionInfo()
        {
            InitializeComponent();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            checkForUpdates = true;
            this.Close();
        }

        private void VersionInfo_Load(object sender, EventArgs e)
        {
            checkForUpdates = false;
        }
    }
}
