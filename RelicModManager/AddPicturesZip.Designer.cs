namespace RelhaxModpack
{
    partial class AddPicturesZip
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
            this.AddSelectionsPicturesLabel = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.Continue = new System.Windows.Forms.Button();
            this.AddPictures = new System.Windows.Forms.Button();
            this.RemoveElements = new System.Windows.Forms.Button();
            this.SelectionPictureDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // AddSelectionsPicturesLabel
            // 
            this.AddSelectionsPicturesLabel.AutoSize = true;
            this.AddSelectionsPicturesLabel.Location = new System.Drawing.Point(12, 9);
            this.AddSelectionsPicturesLabel.Name = "AddSelectionsPicturesLabel";
            this.AddSelectionsPicturesLabel.Size = new System.Drawing.Size(215, 13);
            this.AddSelectionsPicturesLabel.TabIndex = 0;
            this.AddSelectionsPicturesLabel.Text = "Add your selection file and any pictures here";
            // 
            // listBox1
            // 
            this.listBox1.AllowDrop = true;
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 25);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox1.Size = new System.Drawing.Size(558, 225);
            this.listBox1.TabIndex = 1;
            this.listBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBox1_DragDrop);
            this.listBox1.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox1_DragEnter);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(12, 262);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Add selections";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.AddSelectons_Click);
            // 
            // Continue
            // 
            this.Continue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Continue.Location = new System.Drawing.Point(495, 262);
            this.Continue.Name = "Continue";
            this.Continue.Size = new System.Drawing.Size(75, 23);
            this.Continue.TabIndex = 3;
            this.Continue.Text = "Continue";
            this.Continue.UseVisualStyleBackColor = true;
            this.Continue.Click += new System.EventHandler(this.Continue_Click);
            // 
            // AddPictures
            // 
            this.AddPictures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddPictures.Location = new System.Drawing.Point(102, 262);
            this.AddPictures.Name = "AddPictures";
            this.AddPictures.Size = new System.Drawing.Size(75, 23);
            this.AddPictures.TabIndex = 4;
            this.AddPictures.Text = "Add pictures";
            this.AddPictures.UseVisualStyleBackColor = true;
            this.AddPictures.Click += new System.EventHandler(this.AddPictures_Click);
            // 
            // RemoveElements
            // 
            this.RemoveElements.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveElements.Location = new System.Drawing.Point(183, 262);
            this.RemoveElements.Name = "RemoveElements";
            this.RemoveElements.Size = new System.Drawing.Size(107, 23);
            this.RemoveElements.TabIndex = 5;
            this.RemoveElements.Text = "Remove Selected";
            this.RemoveElements.UseVisualStyleBackColor = true;
            this.RemoveElements.Click += new System.EventHandler(this.RemoveElements_Click);
            // 
            // SelectionPictureDialog
            // 
            this.SelectionPictureDialog.FileName = "openFileDialog1";
            this.SelectionPictureDialog.Multiselect = true;
            this.SelectionPictureDialog.RestoreDirectory = true;
            // 
            // AddPicturesZip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 297);
            this.Controls.Add(this.RemoveElements);
            this.Controls.Add(this.AddPictures);
            this.Controls.Add(this.Continue);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.AddSelectionsPicturesLabel);
            this.Name = "AddPicturesZip";
            this.Text = "Add selection and pictures";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label AddSelectionsPicturesLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button Continue;
        private System.Windows.Forms.Button AddPictures;
        private System.Windows.Forms.Button RemoveElements;
        private System.Windows.Forms.OpenFileDialog SelectionPictureDialog;
        public System.Windows.Forms.ListBox listBox1;
    }
}