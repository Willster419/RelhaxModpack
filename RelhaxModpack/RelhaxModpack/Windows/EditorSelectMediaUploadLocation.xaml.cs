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

        private string FTPPath = PrivateStuff.BigmodsFTPMediaRoot;
        public NetworkCredential Credential;
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
            root.MouseDoubleClick += Item_MouseDoubleClick;
            FTPTreeView.Items.Add(root);
            await OpenFolderAsync(root);
            StatusTextBlock.Text = string.Empty;
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
            if (FTPTreeView.SelectedItem is TreeViewItem selectedTreeViewItem)
            {
                UploadPath = selectedTreeViewItem.Tag as string;
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
                if (!selectedTreeViewItem.IsExpanded)
                    selectedTreeViewItem.IsExpanded = true;
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
                    TreeViewItem item = new TreeViewItem()
                    {
                        Header = s,
                        Tag = (itemToOpen.Tag as string) + s + "/"
                    };
                    item.MouseDoubleClick += Item_MouseDoubleClick;
                    itemToOpen.Items.Add(item);
                }
            }
        }

        private async void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FTPTreeView.SelectedItem is TreeViewItem selectedTreeViewItem && selectedTreeViewItem.Equals(sender))
            {
                StatusTextBlock.Text = "opening folder...";
                await OpenFolderAsync(selectedTreeViewItem);
                StatusTextBlock.Text = string.Empty;
                if (!selectedTreeViewItem.IsExpanded)
                    selectedTreeViewItem.IsExpanded = true;
            }
        }
    }
}
