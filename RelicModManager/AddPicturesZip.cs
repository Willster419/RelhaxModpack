using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class AddPicturesZip : RelhaxForum
    {
        public AddPicturesZip()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedItem != null && listBox1.SelectedIndex != -1)
            {
                listBox1.Items.Remove(listBox1.SelectedItem);
                if (listBox1.Items.Count > 0)
                    listBox1.SelectedIndex = 0;
                else
                    listBox1.SelectedIndex = -1;
            }
        }
        
    }
}
