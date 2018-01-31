namespace RelhaxModpack
{
    partial class DatabaseUpdater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseUpdater));
            this.loadDatabaseButton = new System.Windows.Forms.Button();
            this.databaseLocationTextBox = new System.Windows.Forms.RichTextBox();
            this.updateDatabaseOnline = new System.Windows.Forms.Button();
            this.addZipsDialog = new System.Windows.Forms.OpenFileDialog();
            this.loadDatabaseDialog = new System.Windows.Forms.OpenFileDialog();
            this.RunCreateDatabasePHP = new System.Windows.Forms.Button();
            this.OnlineScriptOutput = new System.Windows.Forms.RichTextBox();
            this.updateDatabaseOffline = new System.Windows.Forms.Button();
            this.RunCreateModInfoPHP = new System.Windows.Forms.Button();
            this.RunCreateServerInfoPHP = new System.Windows.Forms.Button();
            this.InfoTB = new System.Windows.Forms.RichTextBox();
            this.RunCreateOutdatedFileList = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // loadDatabaseButton
            // 
            this.loadDatabaseButton.Location = new System.Drawing.Point(12, 12);
            this.loadDatabaseButton.Name = "loadDatabaseButton";
            this.loadDatabaseButton.Size = new System.Drawing.Size(83, 41);
            this.loadDatabaseButton.TabIndex = 0;
            this.loadDatabaseButton.Text = "load database";
            this.loadDatabaseButton.UseVisualStyleBackColor = true;
            this.loadDatabaseButton.Click += new System.EventHandler(this.loadDatabaseButton_Click);
            // 
            // databaseLocationTextBox
            // 
            this.databaseLocationTextBox.Location = new System.Drawing.Point(12, 59);
            this.databaseLocationTextBox.Name = "databaseLocationTextBox";
            this.databaseLocationTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.databaseLocationTextBox.Size = new System.Drawing.Size(316, 39);
            this.databaseLocationTextBox.TabIndex = 1;
            this.databaseLocationTextBox.Text = "-none-";
            // 
            // updateDatabaseOnline
            // 
            this.updateDatabaseOnline.Location = new System.Drawing.Point(116, 12);
            this.updateDatabaseOnline.Name = "updateDatabaseOnline";
            this.updateDatabaseOnline.Size = new System.Drawing.Size(95, 41);
            this.updateDatabaseOnline.TabIndex = 3;
            this.updateDatabaseOnline.Text = "update database (online method)";
            this.updateDatabaseOnline.UseVisualStyleBackColor = true;
            this.updateDatabaseOnline.Click += new System.EventHandler(this.updateDatabaseOnline_Click);
            // 
            // addZipsDialog
            // 
            this.addZipsDialog.DefaultExt = "xml";
            this.addZipsDialog.FileName = "file.zip";
            this.addZipsDialog.Filter = "*.zip|*.zip";
            this.addZipsDialog.Multiselect = true;
            this.addZipsDialog.RestoreDirectory = true;
            this.addZipsDialog.Title = "select zip files to update";
            // 
            // loadDatabaseDialog
            // 
            this.loadDatabaseDialog.DefaultExt = "xml";
            this.loadDatabaseDialog.FileName = "*.xml";
            this.loadDatabaseDialog.Filter = "*.xml|*.xml";
            this.loadDatabaseDialog.RestoreDirectory = true;
            this.loadDatabaseDialog.Title = "load database";
            // 
            // RunCreateDatabasePHP
            // 
            this.RunCreateDatabasePHP.Location = new System.Drawing.Point(12, 145);
            this.RunCreateDatabasePHP.Name = "RunCreateDatabasePHP";
            this.RunCreateDatabasePHP.Size = new System.Drawing.Size(155, 35);
            this.RunCreateDatabasePHP.TabIndex = 5;
            this.RunCreateDatabasePHP.Text = "Run script CreateDatabase.php";
            this.RunCreateDatabasePHP.UseVisualStyleBackColor = true;
            this.RunCreateDatabasePHP.Click += new System.EventHandler(this.RunOnlineScriptButton_Click);
            this.RunCreateDatabasePHP.MouseEnter += new System.EventHandler(this.RunCreateDatabasePHP_MouseEnter);
            this.RunCreateDatabasePHP.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // OnlineScriptOutput
            // 
            this.OnlineScriptOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OnlineScriptOutput.Location = new System.Drawing.Point(345, 12);
            this.OnlineScriptOutput.Name = "OnlineScriptOutput";
            this.OnlineScriptOutput.ReadOnly = true;
            this.OnlineScriptOutput.Size = new System.Drawing.Size(435, 424);
            this.OnlineScriptOutput.TabIndex = 6;
            this.OnlineScriptOutput.Text = "";
            // 
            // updateDatabaseOffline
            // 
            this.updateDatabaseOffline.Location = new System.Drawing.Point(233, 12);
            this.updateDatabaseOffline.Name = "updateDatabaseOffline";
            this.updateDatabaseOffline.Size = new System.Drawing.Size(95, 41);
            this.updateDatabaseOffline.TabIndex = 7;
            this.updateDatabaseOffline.Text = "update database (local method)";
            this.updateDatabaseOffline.UseVisualStyleBackColor = true;
            this.updateDatabaseOffline.Click += new System.EventHandler(this.updateDatabaseOffline_Click);
            // 
            // RunCreateModInfoPHP
            // 
            this.RunCreateModInfoPHP.Location = new System.Drawing.Point(173, 145);
            this.RunCreateModInfoPHP.Name = "RunCreateModInfoPHP";
            this.RunCreateModInfoPHP.Size = new System.Drawing.Size(155, 35);
            this.RunCreateModInfoPHP.TabIndex = 8;
            this.RunCreateModInfoPHP.Text = "Run script CreateModInfo.php";
            this.RunCreateModInfoPHP.UseVisualStyleBackColor = true;
            this.RunCreateModInfoPHP.Click += new System.EventHandler(this.RunCreateModInfoPHP_Click);
            this.RunCreateModInfoPHP.MouseEnter += new System.EventHandler(this.RunCreateModInfoPHP_MouseEnter);
            this.RunCreateModInfoPHP.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // RunCreateServerInfoPHP
            // 
            this.RunCreateServerInfoPHP.Location = new System.Drawing.Point(12, 104);
            this.RunCreateServerInfoPHP.Name = "RunCreateServerInfoPHP";
            this.RunCreateServerInfoPHP.Size = new System.Drawing.Size(155, 35);
            this.RunCreateServerInfoPHP.TabIndex = 9;
            this.RunCreateServerInfoPHP.Text = "Run script CreateManagerInfo.php";
            this.RunCreateServerInfoPHP.UseVisualStyleBackColor = true;
            this.RunCreateServerInfoPHP.Click += new System.EventHandler(this.RunCreateServerInfoPHP_Click);
            this.RunCreateServerInfoPHP.MouseEnter += new System.EventHandler(this.RunCreateServerInfoPHP_MouseEnter);
            this.RunCreateServerInfoPHP.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // InfoTB
            // 
            this.InfoTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.InfoTB.Location = new System.Drawing.Point(12, 186);
            this.InfoTB.Name = "InfoTB";
            this.InfoTB.ReadOnly = true;
            this.InfoTB.Size = new System.Drawing.Size(317, 250);
            this.InfoTB.TabIndex = 10;
            this.InfoTB.Text = "";
            // 
            // RunCreateOutdatedFileList
            // 
            this.RunCreateOutdatedFileList.Location = new System.Drawing.Point(173, 104);
            this.RunCreateOutdatedFileList.Name = "RunCreateOutdatedFileList";
            this.RunCreateOutdatedFileList.Size = new System.Drawing.Size(155, 35);
            this.RunCreateOutdatedFileList.TabIndex = 11;
            this.RunCreateOutdatedFileList.Text = "Run script CreateOutdatedFileList.php";
            this.RunCreateOutdatedFileList.UseVisualStyleBackColor = true;
            this.RunCreateOutdatedFileList.Click += new System.EventHandler(this.RunCreateOutdatedFilesList_Click);
            this.RunCreateOutdatedFileList.MouseEnter += new System.EventHandler(this.RunCreateOutdatedFileList_MouseEnter);
            this.RunCreateOutdatedFileList.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // DatabaseUpdater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 447);
            this.Controls.Add(this.RunCreateOutdatedFileList);
            this.Controls.Add(this.InfoTB);
            this.Controls.Add(this.RunCreateServerInfoPHP);
            this.Controls.Add(this.RunCreateModInfoPHP);
            this.Controls.Add(this.updateDatabaseOffline);
            this.Controls.Add(this.OnlineScriptOutput);
            this.Controls.Add(this.RunCreateDatabasePHP);
            this.Controls.Add(this.updateDatabaseOnline);
            this.Controls.Add(this.databaseLocationTextBox);
            this.Controls.Add(this.loadDatabaseButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DatabaseUpdater";
            this.Text = "CRCFileSizeUpdate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CRCFileSizeUpdate_FormClosing);
            this.Load += new System.EventHandler(this.CRCFileSizeUpdate_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button loadDatabaseButton;
        private System.Windows.Forms.RichTextBox databaseLocationTextBox;
        private System.Windows.Forms.Button updateDatabaseOnline;
        private System.Windows.Forms.OpenFileDialog addZipsDialog;
        private System.Windows.Forms.OpenFileDialog loadDatabaseDialog;
        private System.Windows.Forms.Button RunCreateDatabasePHP;
        private System.Windows.Forms.RichTextBox OnlineScriptOutput;
        private System.Windows.Forms.Button updateDatabaseOffline;
        private System.Windows.Forms.Button RunCreateModInfoPHP;
        private System.Windows.Forms.Button RunCreateServerInfoPHP;
        private System.Windows.Forms.RichTextBox InfoTB;
        private System.Windows.Forms.Button RunCreateOutdatedFileList;
    }
}