using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class DatabaseEditor : Form
    {
        public DatabaseEditor()
        {
            InitializeComponent();
        }

        private void DatabaseEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Utils.appendToLog("|------------------------------------------------------------------------------------------------|");
        }

        private void DatabaseEditor_Load(object sender, EventArgs e)
        {

        }
    }
}
