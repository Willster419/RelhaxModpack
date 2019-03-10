using System;
using System.Collections.Generic;
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
using System.IO;
using System.Net;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for EditorSelectMediaUploadLocation.xaml
    /// </summary>
    public partial class EditorSelectMediaUploadLocation : RelhaxWindow
    {

        private string FTPPath = PrivateStuff.FTPMediaRoot;
        public NetworkCredential Credential;
        public string UploadFileName;
        public string UploadPath;
        private TreeViewItem root;

        public EditorSelectMediaUploadLocation()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {

        }

        private async void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "LOADING...";
            FTPTreeView.Items.Clear();
            root = new TreeViewItem()
            {
                Header = "Medias",
                Tag = FTPPath
            };
            FTPTreeView.Items.Add(root);
            await OpenFolderAsync(root);
            StatusTextBlock.Text = string.Empty;
            FilenameTextBox.Text = UploadFileName;
        }

        private void MakeFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if(FTPTreeView.SelectedItem is TreeViewItem selectedTreeViewItem)
            {
                StatusTextBlock.Text = "making folder...";
                CreateFTPFolderName createFTPFolder = new CreateFTPFolderName()
                {
                    FTPPath = selectedTreeViewItem.Tag as string,
                    Credential = Credential
                };
                if((bool)createFTPFolder.ShowDialog())
                {
                    selectedTreeViewItem.Items.Add(new TreeViewItem()
                    {
                        Header = createFTPFolder.FTPReturnFolderName,
                        Tag = createFTPFolder.FTPReturnPath
                    });
                }
                StatusTextBlock.Text = string.Empty;
            }
        }

        private void SelectFolderUploadButton_Click(object sender, RoutedEventArgs e)
        {
            List<char> invalidChars = Path.GetInvalidFileNameChars().ToList();
            invalidChars.AddRange(Path.GetInvalidPathChars().ToList());
            foreach (char c in invalidChars)
            {
                if (FilenameTextBox.Text.Contains(c))
                {
                    MessageBox.Show(string.Format("value '{0}' is invalid for name", c));
                    return;
                }
            }
            if (FTPTreeView.SelectedItem is TreeViewItem selectedTreeViewItem)
            {
                UploadPath = selectedTreeViewItem.Tag as string;
                UploadFileName = FilenameTextBox.Text;
                DialogResult = true;
                Close();
            }
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (FTPTreeView.SelectedItem is TreeViewItem selectedTreeViewItem)
            {
                StatusTextBlock.Text = "opening folder...";
                await OpenFolderAsync(selectedTreeViewItem);
                StatusTextBlock.Text = string.Empty;
            }
        }

        private async Task OpenFolderAsync(TreeViewItem itemToOpen)
        {
            itemToOpen.Items.Clear();
            string[] folders = await Utils.FTPListFilesFoldersAsync(itemToOpen.Tag as string, Credential);
            foreach(string s in folders)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                if (s.Equals(".") || s.Equals(".."))
                    continue;
                if(!Path.HasExtension(s))
                {
                    itemToOpen.Items.Add(new TreeViewItem()
                    {
                        Header = s,
                        Tag = (itemToOpen.Tag as string) + s + "/"
                    });
                }
            }
        }
    }
}
