namespace RelhaxModpack
{
    partial class GifPreview
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
            this.gifPreviewBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.gifPreviewBox)).BeginInit();
            this.SuspendLayout();
            // 
            // gifPreviewBox
            // 
            this.gifPreviewBox.Location = new System.Drawing.Point(12, 12);
            this.gifPreviewBox.Name = "gifPreviewBox";
            this.gifPreviewBox.Size = new System.Drawing.Size(268, 249);
            this.gifPreviewBox.TabIndex = 0;
            this.gifPreviewBox.TabStop = false;
            // 
            // GifPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.gifPreviewBox);
            this.Name = "GifPreview";
            this.Text = "GifPreview";
            ((System.ComponentModel.ISupportInitialize)(this.gifPreviewBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox gifPreviewBox;
    }
}