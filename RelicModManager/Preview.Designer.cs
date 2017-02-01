namespace RelicModManager
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.previewPicture = new System.Windows.Forms.PictureBox();
            this.nextPicButton = new System.Windows.Forms.Button();
            this.previousPicButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.pictureCountPanel = new System.Windows.Forms.Panel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 356);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(372, 96);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // previewPicture
            // 
            this.previewPicture.Location = new System.Drawing.Point(12, 12);
            this.previewPicture.Name = "previewPicture";
            this.previewPicture.Size = new System.Drawing.Size(372, 309);
            this.previewPicture.TabIndex = 1;
            this.previewPicture.TabStop = false;
            // 
            // nextPicButton
            // 
            this.nextPicButton.Location = new System.Drawing.Point(309, 327);
            this.nextPicButton.Name = "nextPicButton";
            this.nextPicButton.Size = new System.Drawing.Size(75, 23);
            this.nextPicButton.TabIndex = 2;
            this.nextPicButton.Text = "next";
            this.nextPicButton.UseVisualStyleBackColor = true;
            // 
            // previousPicButton
            // 
            this.previousPicButton.Location = new System.Drawing.Point(12, 327);
            this.previousPicButton.Name = "previousPicButton";
            this.previousPicButton.Size = new System.Drawing.Size(75, 23);
            this.previousPicButton.TabIndex = 3;
            this.previousPicButton.Text = "previous";
            this.previousPicButton.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(309, 458);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // pictureCountPanel
            // 
            this.pictureCountPanel.Location = new System.Drawing.Point(93, 327);
            this.pictureCountPanel.Name = "pictureCountPanel";
            this.pictureCountPanel.Size = new System.Drawing.Size(210, 23);
            this.pictureCountPanel.TabIndex = 5;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 463);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(82, 13);
            this.linkLabel1.TabIndex = 6;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Click for full size";
            // 
            // Preview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 488);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.pictureCountPanel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.previousPicButton);
            this.Controls.Add(this.nextPicButton);
            this.Controls.Add(this.previewPicture);
            this.Controls.Add(this.richTextBox1);
            this.Name = "Preview";
            this.Text = "Preview";
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.PictureBox previewPicture;
        private System.Windows.Forms.Button nextPicButton;
        private System.Windows.Forms.Button previousPicButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Panel pictureCountPanel;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}