using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class DeleteBackupFolder : RelhaxForum
    {
        // this is needed to "communicate" from SubForm to MainForm
        public MainWindow MyParent { get; set; }

        public DeleteBackupFolder(List<BackupFolder> bfl)
        {
            InitializeComponent();
            CancelCloseButton.Text = Translations.GetTranslatedString("cancel");
            DeleteButton.Text = Translations.GetTranslatedString("delete");
            DeleteBackupFolder_Load(bfl);
        }

        private void DeleteBackupFolder_Load(List<BackupFolder> bfl)
        {
            foreach (BackupFolder bf in bfl)
            {
                if (!Directory.Exists(bf.TopfolderName)) continue;
                Utils.ConvertDateToLocalCultureFormat(Path.GetFileName(bf.TopfolderName), out string cultureDate);
                SelectionCheckBox cb = new SelectionCheckBox
                {
                    Directory = bf.TopfolderName,
                    NameList = bf.FullnameList,
                    Text = string.Format("{0} ({1})", cultureDate, Utils.SizeSuffix(bf.FilesSizeOnDisk, 2, true)),
                    Name = bf.TopfolderName
                };
                cb.AutoSize = true;
                cb.Location = new Point(6, (SelectBackupFolderPanel.Controls.Count * (cb.Size.Height - 3)));
                // add ToolTip to the SelectBackupFolder
                ToolTip rbToolTip = new ToolTip
                {
                    // Set up the delays for the ToolTip.
                    AutoPopDelay = 30000,
                    InitialDelay = 1000,
                    ReshowDelay = 500,
                    // Force the ToolTip text to be displayed whether or not the form is active.
                    ShowAlways = true
                };
                // Set up the ToolTip text for the Button and Checkbox.
                rbToolTip.SetToolTip(cb, string.Format("foldername: {4}\nfiles: {0}\nfolders: {1}\nsize: {2}\nsize on disk: {3}\nlast modified: {5}", bf.FileCount, bf.FolderCount, Utils.SizeSuffix(bf.FilesSize, 2, true), Utils.SizeSuffix(bf.FilesSizeOnDisk, 2, true), bf.TopfolderName, Directory.GetLastWriteTime(bf.TopfolderName).ToString()));
                SelectBackupFolderPanel.Controls.Add(cb);
            }
        }

        private void CancelCloseButton_Click(object sender, System.EventArgs e)
        {
            this.Dispose();
        }

        private void DeleteButton_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Realy delete all this backup folders?\n\nIt will clean up XX file and give you XX GB additional free HDD space.", "WARNING", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;
            List<Control> controlToRemove = new List<Control>();
            foreach (Control bf in this.SelectBackupFolderPanel.Controls)
            {
                if (bf is SelectionCheckBox scb)
                {
                    if (scb.Checked)
                    {
                        Logging.Manager("deleting backup: " + scb.Directory);
                        Utils.FileDelete(scb.NameList);
                        controlToRemove.Add(scb);
                    }
                }
            }
            foreach (Control rc in controlToRemove)
            {
                rc.Dispose();
            }
            int count = 0;
            foreach (Control bf in this.SelectBackupFolderPanel.Controls)
            {
                bf.Location = new Point(6, (count * bf.Size.Height));
                count++;
            }
        }

        private void DeleteBackupFolder_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.MyParent.ScanningRelHaxModBackupFolder();
            this.Dispose();
        }
    }
}
