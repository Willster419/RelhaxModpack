namespace RelhaxModpack
{
    partial class LoadingGifPreview
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadingGifPreview));
            this.gifPreviewBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.gifPreviewBox)).BeginInit();
            this.SuspendLayout();
            // 
            // gifPreviewBox
            // 
            this.gifPreviewBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gifPreviewBox.Image = global::RelhaxModpack.Properties.Resources.loading_3rdguards;
            this.gifPreviewBox.Location = new System.Drawing.Point(12, 12);
            this.gifPreviewBox.Name = "gifPreviewBox";
            this.gifPreviewBox.Size = new System.Drawing.Size(268, 249);
            this.gifPreviewBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.gifPreviewBox.TabIndex = 0;
            this.gifPreviewBox.TabStop = false;
            // 
            // LoadingGifPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.gifPreviewBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "LoadingGifPreview";
            this.Text = "loadingGifPreview";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LoadingGifPreview_FormClosing);
            this.Load += new System.EventHandler(this.GifPreview_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gifPreviewBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox gifPreviewBox;
    }
}