namespace RelhaxModpack
{
    partial class Preview
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        /// https://stackoverflow.com/questions/1052147/how-do-i-extend-a-winforms-dispose-method
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                // Dispose stuff here
                if (Browser != null)
                {
                    Browser.Dispose();
                    Browser = null;
                }
                if (PreviewPicture != null)
                {
                    PreviewPicture.Dispose();
                    PreviewPicture = null;
                }
                for(int i = 0; i < this.Controls.Count; i++)
                {
                    if(Controls[i] != null)
                    {
                        Controls[i].Dispose();
                    }
                }
                this.Controls.Clear();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Preview));
            this.DescriptionBox = new System.Windows.Forms.RichTextBox();
            this.NextPicButton = new System.Windows.Forms.Button();
            this.PreviousPicButton = new System.Windows.Forms.Button();
            this.PictureCountPanel = new System.Windows.Forms.Panel();
            this.DevLinkLabel = new System.Windows.Forms.LinkLabel();
            this.UpdateBox = new System.Windows.Forms.RichTextBox();
            this.PreviewPicture = new System.Windows.Forms.PictureBox();
            this.LegacyHotfixTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // DescriptionBox
            // 
            this.DescriptionBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DescriptionBox.Location = new System.Drawing.Point(12, 356);
            this.DescriptionBox.Name = "DescriptionBox";
            this.DescriptionBox.ReadOnly = true;
            this.DescriptionBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.DescriptionBox.Size = new System.Drawing.Size(418, 75);
            this.DescriptionBox.TabIndex = 0;
            this.DescriptionBox.Text = "";
            this.DescriptionBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.DescriptionBox_LinkClicked);
            // 
            // NextPicButton
            // 
            this.NextPicButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextPicButton.Location = new System.Drawing.Point(355, 327);
            this.NextPicButton.Name = "NextPicButton";
            this.NextPicButton.Size = new System.Drawing.Size(75, 23);
            this.NextPicButton.TabIndex = 2;
            this.NextPicButton.Text = "next";
            this.NextPicButton.UseVisualStyleBackColor = true;
            this.NextPicButton.Click += new System.EventHandler(this.NextPicButton_Click);
            // 
            // PreviousPicButton
            // 
            this.PreviousPicButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PreviousPicButton.Location = new System.Drawing.Point(12, 327);
            this.PreviousPicButton.Name = "PreviousPicButton";
            this.PreviousPicButton.Size = new System.Drawing.Size(75, 23);
            this.PreviousPicButton.TabIndex = 3;
            this.PreviousPicButton.Text = "previous";
            this.PreviousPicButton.UseVisualStyleBackColor = true;
            this.PreviousPicButton.Click += new System.EventHandler(this.PreviousPicButton_Click);
            // 
            // PictureCountPanel
            // 
            this.PictureCountPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PictureCountPanel.AutoScroll = true;
            this.PictureCountPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureCountPanel.Location = new System.Drawing.Point(93, 327);
            this.PictureCountPanel.Name = "PictureCountPanel";
            this.PictureCountPanel.Size = new System.Drawing.Size(256, 23);
            this.PictureCountPanel.TabIndex = 5;
            // 
            // DevLinkLabel
            // 
            this.DevLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DevLinkLabel.AutoSize = true;
            this.DevLinkLabel.Location = new System.Drawing.Point(332, 496);
            this.DevLinkLabel.Name = "DevLinkLabel";
            this.DevLinkLabel.Size = new System.Drawing.Size(98, 13);
            this.DevLinkLabel.TabIndex = 7;
            this.DevLinkLabel.TabStop = true;
            this.DevLinkLabel.Text = "Developer Website";
            this.DevLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DevLinkLabel_LinkClicked);
            // 
            // UpdateBox
            // 
            this.UpdateBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateBox.Location = new System.Drawing.Point(12, 437);
            this.UpdateBox.Name = "UpdateBox";
            this.UpdateBox.ReadOnly = true;
            this.UpdateBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.UpdateBox.Size = new System.Drawing.Size(418, 56);
            this.UpdateBox.TabIndex = 8;
            this.UpdateBox.Text = "";
            this.UpdateBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.UpdateBox_LinkClicked);
            // 
            // PreviewPicture
            // 
            this.PreviewPicture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewPicture.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.PreviewPicture.Location = new System.Drawing.Point(12, 12);
            this.PreviewPicture.Name = "PreviewPicture";
            this.PreviewPicture.Size = new System.Drawing.Size(418, 309);
            this.PreviewPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PreviewPicture.TabIndex = 1;
            this.PreviewPicture.TabStop = false;
            this.PreviewPicture.Click += new System.EventHandler(this.PreviewPicture_Click);
            // 
            // LegacyHotfixTimer
            // 
            this.LegacyHotfixTimer.Interval = 10;
            this.LegacyHotfixTimer.Tick += new System.EventHandler(this.LegacyHotfixTimer_Tick);
            // 
            // Preview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 517);
            this.Controls.Add(this.UpdateBox);
            this.Controls.Add(this.DevLinkLabel);
            this.Controls.Add(this.PictureCountPanel);
            this.Controls.Add(this.PreviousPicButton);
            this.Controls.Add(this.NextPicButton);
            this.Controls.Add(this.PreviewPicture);
            this.Controls.Add(this.DescriptionBox);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(450, 550);
            this.Name = "Preview";
            this.Text = "Preview";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Preview_FormClosing);
            this.Load += new System.EventHandler(this.Preview_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox DescriptionBox;
        private System.Windows.Forms.PictureBox PreviewPicture;
        private System.Windows.Forms.Button NextPicButton;
        private System.Windows.Forms.Button PreviousPicButton;
        private System.Windows.Forms.Panel PictureCountPanel;
        private System.Windows.Forms.LinkLabel DevLinkLabel;
        private System.Windows.Forms.RichTextBox UpdateBox;
        private System.Windows.Forms.Timer LegacyHotfixTimer;
    }
}