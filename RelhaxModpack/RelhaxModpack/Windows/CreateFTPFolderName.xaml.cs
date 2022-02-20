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
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;
using System.Net;
using RelhaxModpack.Utilities;
using RelhaxModpack.Settings;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for CreateFTPFolderName.xaml
    /// </summary>
    public partial class CreateFTPFolderName : RelhaxWindow
    {
        /// <summary>
        /// The absolute FTP path to the current folder
        /// </summary>
        public string FTPPath;

        /// <summary>
        /// The FTP credentials to use
        /// </summary>
        public NetworkCredential Credential;

        /// <summary>
        /// The absolute FTP path to the newly created folder
        /// </summary>
        public string FTPReturnPath;

        /// <summary>
        /// The newly created folder name
        /// </summary>
        public string FTPReturnFolderName;

        /// <summary>
        /// Create an instance of the CreateFTPFolderName window
        /// </summary>
        public CreateFTPFolderName(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FolderNameTextBox.Focus();
        }

        private void FolderNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && e.IsDown)
            {
                CreateFolder(FolderNameTextBox.Text);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            CreateFolder(FolderNameTextBox.Text);
        }

        private async void CreateFolder(string folderName)
        {
            if(string.IsNullOrWhiteSpace(folderName))
            {
                MessageBox.Show("Invalid folder name");
                return;
            }
            List<char> invalidChars = Path.GetInvalidFileNameChars().ToList();
            invalidChars.AddRange(Path.GetInvalidPathChars().ToList());
            foreach(char c in invalidChars)
            {
                if(folderName.Contains(c))
                {
                    MessageBox.Show(string.Format("value '{0}' is invalid for name", c));
                    return;
                }
            }
            CreatingFolderTextBlock.Visibility = Visibility.Visible;
            try
            {
                string[] folders = await FtpUtils.FtpListFilesFoldersAsync(FTPPath, Credential);
                if (folders.Contains(folderName))
                {
                    CreatingFolderTextBlock.Text = "Folder already exists";
                    FTPReturnFolderName = string.Empty;
                    FTPReturnPath = FTPPath;
                    DialogResult = false;
                    Close();
                    return;
                }
                else
                {
                    await FtpUtils.FtpMakeFolderAsync(string.Format("{0}{1}", FTPPath, folderName), Credential);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            FTPReturnFolderName = folderName;
            FTPReturnPath = string.Format("{0}{1}/", FTPPath, folderName);
            DialogResult = true;
            Close();
        }
    }
}
