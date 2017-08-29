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
                if (youtubedisplay != null)
                {
                    youtubedisplay.Dispose();
                    youtubedisplay = null;
                }
                if (previewPicture != null)
                {
                    previewPicture.Dispose();
                    previewPicture = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Preview));
            this.descriptionBox = new System.Windows.Forms.RichTextBox();
            this.previewPicture = new System.Windows.Forms.PictureBox();
            this.nextPicButton = new System.Windows.Forms.Button();
            this.previousPicButton = new System.Windows.Forms.Button();
            this.pictureCountPanel = new System.Windows.Forms.Panel();
            this.devLinkLabel = new System.Windows.Forms.LinkLabel();
            this.updateBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // descriptionBox
            // 
            this.descriptionBox.Location = new System.Drawing.Point(12, 356);
            this.descriptionBox.Name = "descriptionBox";
            this.descriptionBox.ReadOnly = true;
            this.descriptionBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.descriptionBox.Size = new System.Drawing.Size(418, 75);
            this.descriptionBox.TabIndex = 0;
            this.descriptionBox.Text = "";
            this.descriptionBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.descriptionBox_LinkClicked);
            // 
            // previewPicture
            // 
            this.previewPicture.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.previewPicture.Location = new System.Drawing.Point(12, 12);
            this.previewPicture.Name = "previewPicture";
            this.previewPicture.Size = new System.Drawing.Size(418, 309);
            this.previewPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.previewPicture.TabIndex = 1;
            this.previewPicture.TabStop = false;
            this.previewPicture.Click += new System.EventHandler(this.previewPicture_Click);
            // 
            // nextPicButton
            // 
            this.nextPicButton.Location = new System.Drawing.Point(355, 327);
            this.nextPicButton.Name = "nextPicButton";
            this.nextPicButton.Size = new System.Drawing.Size(75, 23);
            this.nextPicButton.TabIndex = 2;
            this.nextPicButton.Text = "next";
            this.nextPicButton.UseVisualStyleBackColor = true;
            this.nextPicButton.Click += new System.EventHandler(this.nextPicButton_Click);
            // 
            // previousPicButton
            // 
            this.previousPicButton.Location = new System.Drawing.Point(12, 327);
            this.previousPicButton.Name = "previousPicButton";
            this.previousPicButton.Size = new System.Drawing.Size(75, 23);
            this.previousPicButton.TabIndex = 3;
            this.previousPicButton.Text = "previous";
            this.previousPicButton.UseVisualStyleBackColor = true;
            this.previousPicButton.Click += new System.EventHandler(this.previousPicButton_Click);
            // 
            // pictureCountPanel
            // 
            this.pictureCountPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureCountPanel.Location = new System.Drawing.Point(93, 327);
            this.pictureCountPanel.Name = "pictureCountPanel";
            this.pictureCountPanel.Size = new System.Drawing.Size(256, 23);
            this.pictureCountPanel.TabIndex = 5;
            // 
            // devLinkLabel
            // 
            this.devLinkLabel.AutoSize = true;
            this.devLinkLabel.Location = new System.Drawing.Point(332, 496);
            this.devLinkLabel.Name = "devLinkLabel";
            this.devLinkLabel.Size = new System.Drawing.Size(98, 13);
            this.devLinkLabel.TabIndex = 7;
            this.devLinkLabel.TabStop = true;
            this.devLinkLabel.Text = "Developer Website";
            this.devLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.devLinkLabel_LinkClicked);
            // 
            // updateBox
            // 
            this.updateBox.Location = new System.Drawing.Point(12, 437);
            this.updateBox.Name = "updateBox";
            this.updateBox.ReadOnly = true;
            this.updateBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.updateBox.Size = new System.Drawing.Size(418, 56);
            this.updateBox.TabIndex = 8;
            this.updateBox.Text = "";
            this.updateBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.updateBox_LinkClicked);
            // 
            // Preview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 667);
            this.Controls.Add(this.updateBox);
            this.Controls.Add(this.devLinkLabel);
            this.Controls.Add(this.pictureCountPanel);
            this.Controls.Add(this.previousPicButton);
            this.Controls.Add(this.nextPicButton);
            this.Controls.Add(this.previewPicture);
            this.Controls.Add(this.descriptionBox);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(450, 700);
            this.Name = "Preview";
            this.Text = "Preview";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Preview_FormClosing);
            this.Load += new System.EventHandler(this.Preview_Load);
            this.SizeChanged += new System.EventHandler(this.Preview_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox descriptionBox;
        private System.Windows.Forms.PictureBox previewPicture;
        private System.Windows.Forms.Button nextPicButton;
        private System.Windows.Forms.Button previousPicButton;
        private System.Windows.Forms.Panel pictureCountPanel;
        private System.Windows.Forms.LinkLabel devLinkLabel;
        private System.Windows.Forms.RichTextBox updateBox;
    }
}