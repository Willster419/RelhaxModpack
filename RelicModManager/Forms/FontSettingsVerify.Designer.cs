namespace RelhaxModpack.Forms
{
    partial class FontSettingsVerify
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontSettingsVerify));
            this.SettingsChangedHeader = new System.Windows.Forms.Label();
            this.NoButton = new System.Windows.Forms.Button();
            this.YesButton = new System.Windows.Forms.Button();
            this.RevertingTimeoutText = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // SettingsChangedHeader
            // 
            this.SettingsChangedHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SettingsChangedHeader.AutoSize = true;
            this.SettingsChangedHeader.Location = new System.Drawing.Point(9, 9);
            this.SettingsChangedHeader.Name = "SettingsChangedHeader";
            this.SettingsChangedHeader.Size = new System.Drawing.Size(327, 13);
            this.SettingsChangedHeader.TabIndex = 0;
            this.SettingsChangedHeader.Text = "Your Scaling Settings have changed. Would you like to keep them?";
            this.SettingsChangedHeader.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // NoButton
            // 
            this.NoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NoButton.Location = new System.Drawing.Point(12, 56);
            this.NoButton.Name = "NoButton";
            this.NoButton.Size = new System.Drawing.Size(75, 23);
            this.NoButton.TabIndex = 1;
            this.NoButton.Text = "No";
            this.NoButton.UseVisualStyleBackColor = true;
            this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
            // 
            // YesButton
            // 
            this.YesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.YesButton.Location = new System.Drawing.Point(267, 56);
            this.YesButton.Name = "YesButton";
            this.YesButton.Size = new System.Drawing.Size(75, 23);
            this.YesButton.TabIndex = 2;
            this.YesButton.Text = "Yes";
            this.YesButton.UseVisualStyleBackColor = true;
            this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
            // 
            // RevertingTimeoutText
            // 
            this.RevertingTimeoutText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RevertingTimeoutText.AutoSize = true;
            this.RevertingTimeoutText.Location = new System.Drawing.Point(9, 30);
            this.RevertingTimeoutText.Name = "RevertingTimeoutText";
            this.RevertingTimeoutText.Size = new System.Drawing.Size(116, 13);
            this.RevertingTimeoutText.TabIndex = 3;
            this.RevertingTimeoutText.Text = "Reverting in 7 seconds";
            this.RevertingTimeoutText.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FontSettingsVerify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 91);
            this.Controls.Add(this.RevertingTimeoutText);
            this.Controls.Add(this.YesButton);
            this.Controls.Add(this.NoButton);
            this.Controls.Add(this.SettingsChangedHeader);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(370, 130);
            this.Name = "FontSettingsVerify";
            this.Text = "Scaling Changed";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FontSettingsVerify_FormClosing);
            this.Load += new System.EventHandler(this.FontSettingsVerify_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SettingsChangedHeader;
        private System.Windows.Forms.Button NoButton;
        private System.Windows.Forms.Button YesButton;
        private System.Windows.Forms.Label RevertingTimeoutText;
        private System.Windows.Forms.Timer timer1;
    }
}