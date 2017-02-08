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
            this.descriptionBox = new System.Windows.Forms.RichTextBox();
            this.previewPicture = new System.Windows.Forms.PictureBox();
            this.nextPicButton = new System.Windows.Forms.Button();
            this.previousPicButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.pictureCountPanel = new System.Windows.Forms.Panel();
            this.fullSizeLabel = new System.Windows.Forms.LinkLabel();
            this.devLinkLabel = new System.Windows.Forms.LinkLabel();
            this.updateBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // descriptionBox
            // 
            this.descriptionBox.Location = new System.Drawing.Point(12, 356);
            this.descriptionBox.Name = "descriptionBox";
            this.descriptionBox.Size = new System.Drawing.Size(372, 96);
            this.descriptionBox.TabIndex = 0;
            this.descriptionBox.Text = "";
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
            this.closeButton.Location = new System.Drawing.Point(309, 520);
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
            // fullSizeLabel
            // 
            this.fullSizeLabel.AutoSize = true;
            this.fullSizeLabel.Location = new System.Drawing.Point(12, 525);
            this.fullSizeLabel.Name = "fullSizeLabel";
            this.fullSizeLabel.Size = new System.Drawing.Size(82, 13);
            this.fullSizeLabel.TabIndex = 6;
            this.fullSizeLabel.TabStop = true;
            this.fullSizeLabel.Text = "Click for full size";
            // 
            // devLinkLabel
            // 
            this.devLinkLabel.AutoSize = true;
            this.devLinkLabel.Location = new System.Drawing.Point(205, 525);
            this.devLinkLabel.Name = "devLinkLabel";
            this.devLinkLabel.Size = new System.Drawing.Size(98, 13);
            this.devLinkLabel.TabIndex = 7;
            this.devLinkLabel.TabStop = true;
            this.devLinkLabel.Text = "Develepor Website";
            // 
            // updateBox
            // 
            this.updateBox.Location = new System.Drawing.Point(12, 458);
            this.updateBox.Name = "updateBox";
            this.updateBox.Size = new System.Drawing.Size(372, 56);
            this.updateBox.TabIndex = 8;
            this.updateBox.Text = "";
            // 
            // Preview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 548);
            this.Controls.Add(this.updateBox);
            this.Controls.Add(this.devLinkLabel);
            this.Controls.Add(this.fullSizeLabel);
            this.Controls.Add(this.pictureCountPanel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.previousPicButton);
            this.Controls.Add(this.nextPicButton);
            this.Controls.Add(this.previewPicture);
            this.Controls.Add(this.descriptionBox);
            this.Name = "Preview";
            this.Text = "Preview";
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox descriptionBox;
        private System.Windows.Forms.PictureBox previewPicture;
        private System.Windows.Forms.Button nextPicButton;
        private System.Windows.Forms.Button previousPicButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Panel pictureCountPanel;
        private System.Windows.Forms.LinkLabel fullSizeLabel;
        private System.Windows.Forms.LinkLabel devLinkLabel;
        private System.Windows.Forms.RichTextBox updateBox;
    }
}