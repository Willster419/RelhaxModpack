namespace RelhaxModpack
{
    partial class DeleteBackupFolder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteBackupFolder));
            this.SelectBackupFolderPanel = new System.Windows.Forms.Panel();
            this.CancelCloseButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SelectBackupFolderPanel
            // 
            this.SelectBackupFolderPanel.AutoScroll = true;
            this.SelectBackupFolderPanel.Location = new System.Drawing.Point(12, 25);
            this.SelectBackupFolderPanel.Name = "SelectBackupFolderPanel";
            this.SelectBackupFolderPanel.Size = new System.Drawing.Size(301, 273);
            this.SelectBackupFolderPanel.TabIndex = 0;
            // 
            // CancelCloseButton
            // 
            this.CancelCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CancelCloseButton.Location = new System.Drawing.Point(12, 304);
            this.CancelCloseButton.Name = "CancelCloseButton";
            this.CancelCloseButton.Size = new System.Drawing.Size(75, 23);
            this.CancelCloseButton.TabIndex = 3;
            this.CancelCloseButton.Text = "Cancel";
            this.CancelCloseButton.UseVisualStyleBackColor = true;
            this.CancelCloseButton.Click += new System.EventHandler(this.CancelCloseButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteButton.Location = new System.Drawing.Point(238, 304);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteButton.TabIndex = 4;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // DeleteBackupFolder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 339);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.CancelCloseButton);
            this.Controls.Add(this.SelectBackupFolderPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(341, 378);
            this.MinimumSize = new System.Drawing.Size(341, 378);
            this.Name = "DeleteBackupFolder";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DeleteBackupFolder_FormClosing);
            this.Resize += new System.EventHandler(this.DeleteBackupFolder_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel SelectBackupFolderPanel;
        private System.Windows.Forms.Button CancelCloseButton;
        private System.Windows.Forms.Button DeleteButton;
    }
}