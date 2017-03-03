namespace RelicModManager
{
    partial class CRCFileSizeUpdate
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
            this.loadDatabaseButton = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.loadZipFilesButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.addZipsDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // loadDatabaseButton
            // 
            this.loadDatabaseButton.Location = new System.Drawing.Point(53, 12);
            this.loadDatabaseButton.Name = "loadDatabaseButton";
            this.loadDatabaseButton.Size = new System.Drawing.Size(83, 23);
            this.loadDatabaseButton.TabIndex = 0;
            this.loadDatabaseButton.Text = "load database";
            this.loadDatabaseButton.UseVisualStyleBackColor = true;
            this.loadDatabaseButton.Click += new System.EventHandler(this.loadDatabaseButton_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 41);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBox1.Size = new System.Drawing.Size(185, 73);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "-none-";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 117);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Database Loaded";
            // 
            // loadZipFilesButton
            // 
            this.loadZipFilesButton.Location = new System.Drawing.Point(36, 133);
            this.loadZipFilesButton.Name = "loadZipFilesButton";
            this.loadZipFilesButton.Size = new System.Drawing.Size(111, 23);
            this.loadZipFilesButton.TabIndex = 3;
            this.loadZipFilesButton.Text = "add file(s) to update";
            this.loadZipFilesButton.UseVisualStyleBackColor = true;
            this.loadZipFilesButton.Click += new System.EventHandler(this.loadZipFilesButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 159);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Updating...";
            // 
            // addZipsDialog
            // 
            this.addZipsDialog.DefaultExt = "xml";
            this.addZipsDialog.FileName = "openFileDialog1";
            this.addZipsDialog.Filter = "*.xml|*.xml";
            this.addZipsDialog.Multiselect = true;
            this.addZipsDialog.Title = "select zip files to update";
            // 
            // CRCFileSizeUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(202, 193);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.loadZipFilesButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.loadDatabaseButton);
            this.Name = "CRCFileSizeUpdate";
            this.Text = "CRCFileSizeUpdate";
            this.Load += new System.EventHandler(this.CRCFileSizeUpdate_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loadDatabaseButton;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button loadZipFilesButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog addZipsDialog;
    }
}