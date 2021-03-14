using Microsoft.Win32;
using RelhaxModpack.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for AddPicturesZip.xaml
    /// </summary>
    public partial class AddPicturesZip : RelhaxWindow
    {
        /// <summary>
        /// Create and initialize the AddPicturesZip window
        /// </summary>
        public AddPicturesZip(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        /// <summary>
        /// A list of files to add for any diagnostic bug report
        /// </summary>
        public List<string> FilesToAddalways;

        private void AddFilesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = string.Format("{0}|*.*", Translations.GetTranslatedString("allFiles")),
                Title = Translations.GetTranslatedString("selectFilesToInclude"),
                Multiselect = true
            };
            if((bool)dialog.ShowDialog())
                foreach (string s in dialog.FileNames)
                    //probably don't need it, but just to be safe i guess
                    if(File.Exists(s))
                        FilesToAddList.Items.Add(s);
        }

        private void RemoveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < FilesToAddList.SelectedItems.Count; i++)
            {
                if (FilesToAddalways.Contains(FilesToAddList.SelectedItems[i]))
                {
                    string filename = FilesToAddList.SelectedItems[i] as string;
                    if (string.IsNullOrEmpty(filename))
                        filename = "null";
                    MessageBox.Show(string.Format("{0}: {1}", Translations.GetTranslatedString("cantRemoveDefaultFile"), Path.GetFileName(filename)));
                }
                else
                {
                    FilesToAddList.Items.Remove(FilesToAddList.SelectedItems[i]);
                    i--;
                }
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void FilesToAddList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void FilesToAddList_Drop(object sender, DragEventArgs e)
        {
            //https://stackoverflow.com/questions/21706747/drag-and-drop-files-into-my-listbox
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (!FilesToAddList.Items.Contains(file))
                    FilesToAddList.Items.Add(file);
            }
        }
    }
}
