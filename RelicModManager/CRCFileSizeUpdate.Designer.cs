namespace RelhaxModpack
{
    partial class CRCFileSizeUpdate
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
            this.loadDatabaseButton = new System.Windows.Forms.Button();
            this.databaseLocationTextBox = new System.Windows.Forms.RichTextBox();
            this.loadZipFilesButton = new System.Windows.Forms.Button();
            this.updatingLabel = new System.Windows.Forms.Label();
            this.loadDatabaseDialog = new System.Windows.Forms.OpenFileDialog();
            this.RunOnlineScriptButton = new System.Windows.Forms.Button();
            this.OnlineScriptOutput = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // loadDatabaseButton
            // 
            this.loadDatabaseButton.Location = new System.Drawing.Point(12, 12);
            this.loadDatabaseButton.Name = "loadDatabaseButton";
            this.loadDatabaseButton.Size = new System.Drawing.Size(83, 23);
            this.loadDatabaseButton.TabIndex = 0;
            this.loadDatabaseButton.Text = "load database";
            this.loadDatabaseButton.UseVisualStyleBackColor = true;
            this.loadDatabaseButton.Click += new System.EventHandler(this.loadDatabaseButton_Click);
            // 
            // databaseLocationTextBox
            // 
            this.databaseLocationTextBox.Location = new System.Drawing.Point(12, 41);
            this.databaseLocationTextBox.Name = "databaseLocationTextBox";
            this.databaseLocationTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.databaseLocationTextBox.Size = new System.Drawing.Size(301, 39);
            this.databaseLocationTextBox.TabIndex = 1;
            this.databaseLocationTextBox.Text = "-none-";
            // 
            // loadZipFilesButton
            // 
            this.loadZipFilesButton.Location = new System.Drawing.Point(12, 86);
            this.loadZipFilesButton.Name = "loadZipFilesButton";
            this.loadZipFilesButton.Size = new System.Drawing.Size(111, 23);
            this.loadZipFilesButton.TabIndex = 3;
            this.loadZipFilesButton.Text = "add file(s) to update";
            this.loadZipFilesButton.UseVisualStyleBackColor = true;
            this.loadZipFilesButton.Click += new System.EventHandler(this.loadZipFilesButton_Click);
            // 
            // updatingLabel
            // 
            this.updatingLabel.AutoSize = true;
            this.updatingLabel.Location = new System.Drawing.Point(12, 112);
            this.updatingLabel.Name = "updatingLabel";
            this.updatingLabel.Size = new System.Drawing.Size(24, 13);
            this.updatingLabel.TabIndex = 4;
            this.updatingLabel.Text = "Idle";
            // 
            // loadDatabaseDialog
            // 
            this.loadDatabaseDialog.DefaultExt = "xml";
            this.loadDatabaseDialog.FileName = "*.xml";
            this.loadDatabaseDialog.Filter = "*.xml|*.xml";
            this.loadDatabaseDialog.RestoreDirectory = true;
            this.loadDatabaseDialog.Title = "load database";
            // 
            // RunOnlineScriptButton
            // 
            this.RunOnlineScriptButton.Location = new System.Drawing.Point(12, 128);
            this.RunOnlineScriptButton.Name = "RunOnlineScriptButton";
            this.RunOnlineScriptButton.Size = new System.Drawing.Size(135, 23);
            this.RunOnlineScriptButton.TabIndex = 5;
            this.RunOnlineScriptButton.Text = "run online update script";
            this.RunOnlineScriptButton.UseVisualStyleBackColor = true;
            this.RunOnlineScriptButton.Click += new System.EventHandler(this.RunOnlineScriptButton_Click);
            // 
            // OnlineScriptOutput
            // 
            this.OnlineScriptOutput.Location = new System.Drawing.Point(12, 157);
            this.OnlineScriptOutput.Name = "OnlineScriptOutput";
            this.OnlineScriptOutput.ReadOnly = true;
            this.OnlineScriptOutput.Size = new System.Drawing.Size(301, 105);
            this.OnlineScriptOutput.TabIndex = 6;
            this.OnlineScriptOutput.Text = "";
            // 
            // CRCFileSizeUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 280);
            this.Controls.Add(this.OnlineScriptOutput);
            this.Controls.Add(this.RunOnlineScriptButton);
            this.Controls.Add(this.updatingLabel);
            this.Controls.Add(this.loadZipFilesButton);
            this.Controls.Add(this.databaseLocationTextBox);
            this.Controls.Add(this.loadDatabaseButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "CRCFileSizeUpdate";
            this.Text = "CRCFileSizeUpdate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CRCFileSizeUpdate_FormClosing);
            this.Load += new System.EventHandler(this.CRCFileSizeUpdate_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loadDatabaseButton;
        private System.Windows.Forms.RichTextBox databaseLocationTextBox;
        private System.Windows.Forms.Button loadZipFilesButton;
        private System.Windows.Forms.Label updatingLabel;
        // private System.Windows.Forms.OpenFileDialog addZipsDialog;
        private System.Windows.Forms.OpenFileDialog loadDatabaseDialog;
        private System.Windows.Forms.Button RunOnlineScriptButton;
        private System.Windows.Forms.RichTextBox OnlineScriptOutput;
    }
}