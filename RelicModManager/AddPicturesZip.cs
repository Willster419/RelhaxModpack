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

        private string[] AcceptableImageExtensions = new string[] { "*.bmp;", "*.jpg;", "*.jpeg;", "*.png;", "*.gif;" };
        private string[] AcceptableSelectionExtensions = new string[] { "*.xml;" };

        public string AppStartupPath { get; set; }

        public AddPicturesZip()
        {
            InitializeComponent();
        }

        private void AddSelectons_Click(object sender, EventArgs e)
        {
            SelectionPictureDialog.DefaultExt = ".xml";
            SelectionPictureDialog.Filter = @" *.xml|*.xml";
            SelectionPictureDialog.InitialDirectory = Path.Combine(AppStartupPath, "RelHaxUserConfigs");
            SelectionPictureDialog.Title = "Select selection file(s) to include";
            if (SelectionPictureDialog.ShowDialog() == DialogResult.Cancel)
                return;
            foreach (string s in SelectionPictureDialog.FileNames)
                listBox1.Items.Add(s);
        }

        private void AddPictures_Click(object sender, EventArgs e)
        {
            SelectionPictureDialog.DefaultExt = "";
            string allPictureTypes = @" Picture files|";
            foreach(string s in AcceptableImageExtensions)
            {
                allPictureTypes = allPictureTypes + s.ToLower();
                allPictureTypes = allPictureTypes + s.ToUpper();
            }
            SelectionPictureDialog.Filter = allPictureTypes;
            SelectionPictureDialog.InitialDirectory = Path.Combine(AppStartupPath, "RelHaxUserConfigs");
            SelectionPictureDialog.Title = "Select Picture(s) to include";
            if (SelectionPictureDialog.ShowDialog() == DialogResult.Cancel)
                return;
            foreach (string s in SelectionPictureDialog.FileNames)
                listBox1.Items.Add(s);
        }

        private void RemoveElements_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < listBox1.SelectedItems.Count; i++)
            {
                listBox1.Items.Remove(listBox1.SelectedItems[i]);
                i--;
            }
        }

        private void Continue_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        //https://stackoverflow.com/questions/21706747/drag-and-drop-files-into-my-listbox
        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file);
                if(IsAcceptableExtantion(extension) && !(listBox1.Items.Contains(file)))
                    listBox1.Items.Add(file);
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private bool IsAcceptableExtantion(string extension)
        {
            extension = extension.ToUpper().ToLower();
            foreach (string s in AcceptableSelectionExtensions)
            {
                string ext = s.Substring(1, s.Length - 2);
                if (ext.ToUpper().ToLower().Equals(extension))
                    return true;
            }
            foreach (string s in AcceptableImageExtensions)
            {
                //"*.bmp;"
                string ext = s.Substring(1, s.Length - 2);
                if (ext.ToUpper().ToLower().Equals(extension))
                    return true;
            }
            return false;
        }
    }
}
