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
    public partial class CustomURLs : Form
    {
        private HowToFindLinks links = new HowToFindLinks();
        public bool canceling;

        public CustomURLs()
        {
            InitializeComponent();
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            canceling = false;
            this.Hide();
        }

        private void findDownloadLinks_Click(object sender, EventArgs e)
        {
            links.ShowDialog();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void CustomURLs_Shown(object sender, EventArgs e)
        {
            canceling = true;
        }
    }
}
