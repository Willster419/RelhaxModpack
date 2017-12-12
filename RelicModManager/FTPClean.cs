using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;
using System.Xml;
using System.IO;

namespace RelhaxModpack
{
    public partial class FTPClean : RelhaxForum
    {
        Control[] controls;
        StringBuilder sb;
        public FTPClean()
        {
            InitializeComponent();
            controls = new Control[] { No, Yes, DeleteFilesLabel, DeleteFilesRTB };
        }

        private void LocalFolderButton_Click(object sender, EventArgs e)
        {
            if (LocalFolderBrowser.ShowDialog() == DialogResult.Cancel)
                return;
            LocalFolderTB.Text = LocalFolderBrowser.SelectedPath;
        }

        private void LocalXMLFileButton_Click(object sender, EventArgs e)
        {
            OpenXMLFileBrowser.InitialDirectory = Application.StartupPath;
            if (OpenXMLFileBrowser.ShowDialog() == DialogResult.Cancel)
                return;
            LocalXMLFileTB.Text = OpenXMLFileBrowser.FileName;
        }

        private void TrashCleanup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LocalFolderTB.Text) || string.IsNullOrWhiteSpace(LocalXMLFileTB.Text))
                return;
            XPathDocument doc = new XPathDocument(LocalXMLFileTB.Text);
            sb = new StringBuilder();
            foreach(var file in doc.CreateNavigator().Select("//trash/filename"))
            {
                sb.Append(file.ToString() + "\n");
            }
            //if (MessageBox.Show("Delete the following files?\n" + sb.ToString(), "Delete Files?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            //    return;
            foreach(Control c in controls)
            {
                c.Visible = true;
            }
            DeleteFilesRTB.Text = sb.ToString();
        }

        private void Yes_Click(object sender, EventArgs e)
        {
            string[] files = sb.ToString().Split('\n');
            foreach (string f in files)
            {
                string fullPath = Path.Combine(LocalFolderTB.Text, f);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            MessageBox.Show("Done");
            foreach (Control c in controls)
            {
                c.Visible = false;
            }
            DeleteFilesRTB.Text = "";
        }

        private void No_Click(object sender, EventArgs e)
        {
            sb = new StringBuilder();
            foreach (Control c in controls)
            {
                c.Visible = false;
            }
            DeleteFilesRTB.Text = "";
        }
    }
}
