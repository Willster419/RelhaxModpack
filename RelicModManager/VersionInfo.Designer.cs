namespace RelicModManager
{
    partial class VersionInfo
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
            this.downloadedVersionInfo = new System.Windows.Forms.RichTextBox();
            this.updateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // downloadedVersionInfo
            // 
            this.downloadedVersionInfo.Location = new System.Drawing.Point(12, 12);
            this.downloadedVersionInfo.Name = "downloadedVersionInfo";
            this.downloadedVersionInfo.ReadOnly = true;
            this.downloadedVersionInfo.Size = new System.Drawing.Size(179, 65);
            this.downloadedVersionInfo.TabIndex = 0;
            this.downloadedVersionInfo.Text = "";
            // 
            // updateButton
            // 
            this.updateButton.Location = new System.Drawing.Point(49, 83);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(105, 23);
            this.updateButton.TabIndex = 1;
            this.updateButton.Text = "Check for updates";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // VersionInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(203, 109);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.downloadedVersionInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "VersionInfo";
            this.Text = "VersionInfo";
            this.Load += new System.EventHandler(this.VersionInfo_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox downloadedVersionInfo;
        private System.Windows.Forms.Button updateButton;

    }
}