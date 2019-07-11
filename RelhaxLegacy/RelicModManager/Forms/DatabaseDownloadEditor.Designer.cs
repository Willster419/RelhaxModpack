namespace RelhaxModpack
{
    partial class DatabaseDownloadEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseDownloadEditor));
            this.DownloadingLabel = new System.Windows.Forms.Label();
            this.OpenFileButton = new System.Windows.Forms.Button();
            this.OpenFolderButton = new System.Windows.Forms.Button();
            this.DownloadProgressBar = new System.Windows.Forms.ProgressBar();
            this.SaveDownloadDialog = new System.Windows.Forms.SaveFileDialog();
            this.CancelButtonn = new System.Windows.Forms.Button();
            this.SizeToDownload = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DownloadingLabel
            // 
            this.DownloadingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DownloadingLabel.Location = new System.Drawing.Point(12, 9);
            this.DownloadingLabel.Name = "DownloadingLabel";
            this.DownloadingLabel.Size = new System.Drawing.Size(316, 61);
            this.DownloadingLabel.TabIndex = 0;
            this.DownloadingLabel.Text = "Downloading...";
            // 
            // OpenFileButton
            // 
            this.OpenFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenFileButton.Enabled = false;
            this.OpenFileButton.Location = new System.Drawing.Point(253, 123);
            this.OpenFileButton.Name = "OpenFileButton";
            this.OpenFileButton.Size = new System.Drawing.Size(75, 23);
            this.OpenFileButton.TabIndex = 1;
            this.OpenFileButton.Text = "Open File";
            this.OpenFileButton.UseVisualStyleBackColor = true;
            this.OpenFileButton.Click += new System.EventHandler(this.OpenFileButton_Click);
            // 
            // OpenFolderButton
            // 
            this.OpenFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenFolderButton.Enabled = false;
            this.OpenFolderButton.Location = new System.Drawing.Point(172, 123);
            this.OpenFolderButton.Name = "OpenFolderButton";
            this.OpenFolderButton.Size = new System.Drawing.Size(75, 23);
            this.OpenFolderButton.TabIndex = 2;
            this.OpenFolderButton.Text = "Open Folder";
            this.OpenFolderButton.UseVisualStyleBackColor = true;
            this.OpenFolderButton.Click += new System.EventHandler(this.OpenFolderButton_Click);
            // 
            // DownloadProgressBar
            // 
            this.DownloadProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DownloadProgressBar.Location = new System.Drawing.Point(12, 73);
            this.DownloadProgressBar.Name = "DownloadProgressBar";
            this.DownloadProgressBar.Size = new System.Drawing.Size(316, 23);
            this.DownloadProgressBar.TabIndex = 3;
            // 
            // SaveDownloadDialog
            // 
            this.SaveDownloadDialog.RestoreDirectory = true;
            this.SaveDownloadDialog.SupportMultiDottedExtensions = true;
            this.SaveDownloadDialog.Title = "Save File";
            // 
            // CancelButtonn
            // 
            this.CancelButtonn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButtonn.Location = new System.Drawing.Point(91, 123);
            this.CancelButtonn.Name = "CancelButtonn";
            this.CancelButtonn.Size = new System.Drawing.Size(75, 23);
            this.CancelButtonn.TabIndex = 4;
            this.CancelButtonn.Text = "Cancel";
            this.CancelButtonn.UseVisualStyleBackColor = true;
            this.CancelButtonn.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // SizeToDownload
            // 
            this.SizeToDownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SizeToDownload.AutoSize = true;
            this.SizeToDownload.Location = new System.Drawing.Point(12, 99);
            this.SizeToDownload.Name = "SizeToDownload";
            this.SizeToDownload.Size = new System.Drawing.Size(64, 13);
            this.SizeToDownload.TabIndex = 5;
            this.SizeToDownload.Text = "0 kb of 0 kb";
            // 
            // DatabaseDownloadEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 150);
            this.Controls.Add(this.SizeToDownload);
            this.Controls.Add(this.CancelButtonn);
            this.Controls.Add(this.DownloadProgressBar);
            this.Controls.Add(this.OpenFolderButton);
            this.Controls.Add(this.OpenFileButton);
            this.Controls.Add(this.DownloadingLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DatabaseDownloadEditor";
            this.Text = "EditorDownloader";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DatabaseDownloadEditor_FormClosed);
            this.Load += new System.EventHandler(this.DatabaseDownloadEditor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DownloadingLabel;
        private System.Windows.Forms.Button OpenFileButton;
        private System.Windows.Forms.Button OpenFolderButton;
        private System.Windows.Forms.ProgressBar DownloadProgressBar;
        private System.Windows.Forms.SaveFileDialog SaveDownloadDialog;
        private System.Windows.Forms.Button CancelButtonn;
        private System.Windows.Forms.Label SizeToDownload;
    }
}