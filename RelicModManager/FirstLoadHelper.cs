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
    public partial class FirstLoadHelper : Form
    {
        int x, y;
        public FirstLoadHelper(int xx, int yy)
        {
            InitializeComponent();
            x = xx;
            y = yy;
        }

        private void FirstLoadHelper_Load(object sender, EventArgs e)
        {
            this.Location = new Point(x, y);
        }
    }
}
