namespace RelhaxModpack.Forms
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
            this.SuspendLayout();
            // 
            // SelectBackupFolderPanel
            // 
            this.SelectBackupFolderPanel.Location = new System.Drawing.Point(12, 25);
            this.SelectBackupFolderPanel.Name = "SelectBackupFolderPanel";
            this.SelectBackupFolderPanel.Size = new System.Drawing.Size(266, 201);
            this.SelectBackupFolderPanel.TabIndex = 0;
            // 
            // DeleteBackupFolder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 267);
            this.Controls.Add(this.SelectBackupFolderPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "DeleteBackupFolder";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel SelectBackupFolderPanel;
    }
}