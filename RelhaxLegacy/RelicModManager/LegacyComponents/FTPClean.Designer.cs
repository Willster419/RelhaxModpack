namespace RelhaxModpack
{
    partial class FTPClean
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
            this.LocalFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.LocalFolderLabel = new System.Windows.Forms.Label();
            this.LocalFolderButton = new System.Windows.Forms.Button();
            this.LocalFolderTB = new System.Windows.Forms.TextBox();
            this.LocalXMLFileTB = new System.Windows.Forms.TextBox();
            this.LocalXMLFileButton = new System.Windows.Forms.Button();
            this.LocalXMLFileLabel = new System.Windows.Forms.Label();
            this.OpenXMLFileBrowser = new System.Windows.Forms.OpenFileDialog();
            this.TrashCleanup = new System.Windows.Forms.Button();
            this.DeleteFilesLabel = new System.Windows.Forms.Label();
            this.DeleteFilesRTB = new System.Windows.Forms.RichTextBox();
            this.No = new System.Windows.Forms.Button();
            this.Yes = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LocalFolderBrowser
            // 
            this.LocalFolderBrowser.Description = "Select WoT zip files folder";
            this.LocalFolderBrowser.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.LocalFolderBrowser.ShowNewFolderButton = false;
            // 
            // LocalFolderLabel
            // 
            this.LocalFolderLabel.AutoSize = true;
            this.LocalFolderLabel.Location = new System.Drawing.Point(12, 9);
            this.LocalFolderLabel.Name = "LocalFolderLabel";
            this.LocalFolderLabel.Size = new System.Drawing.Size(98, 13);
            this.LocalFolderLabel.TabIndex = 0;
            this.LocalFolderLabel.Text = "Select Local Folder";
            // 
            // LocalFolderButton
            // 
            this.LocalFolderButton.Location = new System.Drawing.Point(116, 4);
            this.LocalFolderButton.Name = "LocalFolderButton";
            this.LocalFolderButton.Size = new System.Drawing.Size(45, 23);
            this.LocalFolderButton.TabIndex = 1;
            this.LocalFolderButton.Text = "Select";
            this.LocalFolderButton.UseVisualStyleBackColor = true;
            this.LocalFolderButton.Click += new System.EventHandler(this.LocalFolderButton_Click);
            // 
            // LocalFolderTB
            // 
            this.LocalFolderTB.Location = new System.Drawing.Point(12, 33);
            this.LocalFolderTB.Name = "LocalFolderTB";
            this.LocalFolderTB.Size = new System.Drawing.Size(507, 20);
            this.LocalFolderTB.TabIndex = 2;
            // 
            // LocalXMLFileTB
            // 
            this.LocalXMLFileTB.Location = new System.Drawing.Point(12, 88);
            this.LocalXMLFileTB.Name = "LocalXMLFileTB";
            this.LocalXMLFileTB.Size = new System.Drawing.Size(507, 20);
            this.LocalXMLFileTB.TabIndex = 5;
            // 
            // LocalXMLFileButton
            // 
            this.LocalXMLFileButton.Location = new System.Drawing.Point(96, 59);
            this.LocalXMLFileButton.Name = "LocalXMLFileButton";
            this.LocalXMLFileButton.Size = new System.Drawing.Size(45, 23);
            this.LocalXMLFileButton.TabIndex = 4;
            this.LocalXMLFileButton.Text = "Select";
            this.LocalXMLFileButton.UseVisualStyleBackColor = true;
            this.LocalXMLFileButton.Click += new System.EventHandler(this.LocalXMLFileButton_Click);
            // 
            // LocalXMLFileLabel
            // 
            this.LocalXMLFileLabel.AutoSize = true;
            this.LocalXMLFileLabel.Location = new System.Drawing.Point(12, 64);
            this.LocalXMLFileLabel.Name = "LocalXMLFileLabel";
            this.LocalXMLFileLabel.Size = new System.Drawing.Size(78, 13);
            this.LocalXMLFileLabel.TabIndex = 3;
            this.LocalXMLFileLabel.Text = "Select XML file";
            // 
            // OpenXMLFileBrowser
            // 
            this.OpenXMLFileBrowser.DefaultExt = "xml";
            this.OpenXMLFileBrowser.FileName = "Trash.xml";
            this.OpenXMLFileBrowser.Filter = "*.xml|*.xml";
            this.OpenXMLFileBrowser.RestoreDirectory = true;
            this.OpenXMLFileBrowser.Title = "Load Trash XML File";
            // 
            // TrashCleanup
            // 
            this.TrashCleanup.Location = new System.Drawing.Point(12, 114);
            this.TrashCleanup.Name = "TrashCleanup";
            this.TrashCleanup.Size = new System.Drawing.Size(98, 23);
            this.TrashCleanup.TabIndex = 6;
            this.TrashCleanup.Text = "Cleanup Trash";
            this.TrashCleanup.UseVisualStyleBackColor = true;
            this.TrashCleanup.Click += new System.EventHandler(this.TrashCleanup_Click);
            // 
            // DeleteFilesLabel
            // 
            this.DeleteFilesLabel.AutoSize = true;
            this.DeleteFilesLabel.Location = new System.Drawing.Point(12, 140);
            this.DeleteFilesLabel.Name = "DeleteFilesLabel";
            this.DeleteFilesLabel.Size = new System.Drawing.Size(127, 13);
            this.DeleteFilesLabel.TabIndex = 7;
            this.DeleteFilesLabel.Text = "Delete the following files?";
            this.DeleteFilesLabel.Visible = false;
            // 
            // DeleteFilesRTB
            // 
            this.DeleteFilesRTB.Location = new System.Drawing.Point(12, 156);
            this.DeleteFilesRTB.Name = "DeleteFilesRTB";
            this.DeleteFilesRTB.ReadOnly = true;
            this.DeleteFilesRTB.Size = new System.Drawing.Size(507, 151);
            this.DeleteFilesRTB.TabIndex = 8;
            this.DeleteFilesRTB.Text = "";
            this.DeleteFilesRTB.Visible = false;
            // 
            // No
            // 
            this.No.Location = new System.Drawing.Point(12, 313);
            this.No.Name = "No";
            this.No.Size = new System.Drawing.Size(75, 23);
            this.No.TabIndex = 9;
            this.No.Text = "No";
            this.No.UseVisualStyleBackColor = true;
            this.No.Visible = false;
            this.No.Click += new System.EventHandler(this.No_Click);
            // 
            // Yes
            // 
            this.Yes.Location = new System.Drawing.Point(96, 313);
            this.Yes.Name = "Yes";
            this.Yes.Size = new System.Drawing.Size(75, 23);
            this.Yes.TabIndex = 10;
            this.Yes.Text = "Yes";
            this.Yes.UseVisualStyleBackColor = true;
            this.Yes.Visible = false;
            this.Yes.Click += new System.EventHandler(this.Yes_Click);
            // 
            // FTPClean
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 342);
            this.Controls.Add(this.Yes);
            this.Controls.Add(this.No);
            this.Controls.Add(this.DeleteFilesRTB);
            this.Controls.Add(this.DeleteFilesLabel);
            this.Controls.Add(this.TrashCleanup);
            this.Controls.Add(this.LocalXMLFileTB);
            this.Controls.Add(this.LocalXMLFileButton);
            this.Controls.Add(this.LocalXMLFileLabel);
            this.Controls.Add(this.LocalFolderTB);
            this.Controls.Add(this.LocalFolderButton);
            this.Controls.Add(this.LocalFolderLabel);
            this.Name = "FTPClean";
            this.Text = "FTPClean";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog LocalFolderBrowser;
        private System.Windows.Forms.Label LocalFolderLabel;
        private System.Windows.Forms.Button LocalFolderButton;
        private System.Windows.Forms.TextBox LocalFolderTB;
        private System.Windows.Forms.TextBox LocalXMLFileTB;
        private System.Windows.Forms.Button LocalXMLFileButton;
        private System.Windows.Forms.Label LocalXMLFileLabel;
        private System.Windows.Forms.OpenFileDialog OpenXMLFileBrowser;
        private System.Windows.Forms.Button TrashCleanup;
        private System.Windows.Forms.Label DeleteFilesLabel;
        private System.Windows.Forms.RichTextBox DeleteFilesRTB;
        private System.Windows.Forms.Button No;
        private System.Windows.Forms.Button Yes;
    }
}