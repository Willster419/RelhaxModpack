namespace RelicModManager
{
    partial class CustomURLs
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
            this.URLInstructions = new System.Windows.Forms.Label();
            this.zipFileURL = new System.Windows.Forms.TextBox();
            this.doneButton = new System.Windows.Forms.Button();
            this.findDownloadLinks = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.zipFileLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // URLInstructions
            // 
            this.URLInstructions.AutoSize = true;
            this.URLInstructions.Location = new System.Drawing.Point(12, 9);
            this.URLInstructions.Name = "URLInstructions";
            this.URLInstructions.Size = new System.Drawing.Size(270, 13);
            this.URLInstructions.TabIndex = 0;
            this.URLInstructions.Text = "Please provide the google drive URL for the zip file here";
            // 
            // zipFileURL
            // 
            this.zipFileURL.Location = new System.Drawing.Point(15, 53);
            this.zipFileURL.Name = "zipFileURL";
            this.zipFileURL.Size = new System.Drawing.Size(267, 20);
            this.zipFileURL.TabIndex = 2;
            // 
            // doneButton
            // 
            this.doneButton.Location = new System.Drawing.Point(207, 79);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(75, 23);
            this.doneButton.TabIndex = 9;
            this.doneButton.Text = "done";
            this.doneButton.UseVisualStyleBackColor = true;
            this.doneButton.Click += new System.EventHandler(this.doneButton_Click);
            // 
            // findDownloadLinks
            // 
            this.findDownloadLinks.Location = new System.Drawing.Point(15, 108);
            this.findDownloadLinks.Name = "findDownloadLinks";
            this.findDownloadLinks.Size = new System.Drawing.Size(267, 23);
            this.findDownloadLinks.TabIndex = 10;
            this.findDownloadLinks.Text = "how exactly do you expect me to find those links???";
            this.findDownloadLinks.UseVisualStyleBackColor = true;
            this.findDownloadLinks.Click += new System.EventHandler(this.findDownloadLinks_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(15, 79);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // zipFileLabel
            // 
            this.zipFileLabel.AutoSize = true;
            this.zipFileLabel.Location = new System.Drawing.Point(128, 37);
            this.zipFileLabel.Name = "zipFileLabel";
            this.zipFileLabel.Size = new System.Drawing.Size(36, 13);
            this.zipFileLabel.TabIndex = 1;
            this.zipFileLabel.Text = "zip file";
            // 
            // CustomURLs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 146);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.findDownloadLinks);
            this.Controls.Add(this.doneButton);
            this.Controls.Add(this.zipFileURL);
            this.Controls.Add(this.zipFileLabel);
            this.Controls.Add(this.URLInstructions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CustomURLs";
            this.Text = "CustomURLs";
            this.Shown += new System.EventHandler(this.CustomURLs_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label URLInstructions;
        public System.Windows.Forms.TextBox zipFileURL;
        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.Button findDownloadLinks;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label zipFileLabel;
    }
}