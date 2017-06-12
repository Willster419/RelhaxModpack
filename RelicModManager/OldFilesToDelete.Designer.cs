namespace RelhaxModpack
{
    partial class OldFilesToDelete
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
            this.deleteFilesHeader = new System.Windows.Forms.Label();
            this.filesList = new System.Windows.Forms.RichTextBox();
            this.deleteFilesQuestion = new System.Windows.Forms.Label();
            this.yesDeleteButton = new System.Windows.Forms.Button();
            this.noDeleteButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // deleteFilesHeader
            // 
            this.deleteFilesHeader.AutoSize = true;
            this.deleteFilesHeader.Location = new System.Drawing.Point(75, 9);
            this.deleteFilesHeader.Name = "deleteFilesHeader";
            this.deleteFilesHeader.Size = new System.Drawing.Size(35, 13);
            this.deleteFilesHeader.TabIndex = 0;
            this.deleteFilesHeader.Text = "label1";
            // 
            // filesList
            // 
            this.filesList.Location = new System.Drawing.Point(12, 35);
            this.filesList.Name = "filesList";
            this.filesList.ReadOnly = true;
            this.filesList.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.filesList.Size = new System.Drawing.Size(514, 266);
            this.filesList.TabIndex = 1;
            this.filesList.Text = "";
            // 
            // deleteFilesQuestion
            // 
            this.deleteFilesQuestion.AutoSize = true;
            this.deleteFilesQuestion.Location = new System.Drawing.Point(177, 312);
            this.deleteFilesQuestion.Name = "deleteFilesQuestion";
            this.deleteFilesQuestion.Size = new System.Drawing.Size(35, 13);
            this.deleteFilesQuestion.TabIndex = 2;
            this.deleteFilesQuestion.Text = "label2";
            // 
            // yesDeleteButton
            // 
            this.yesDeleteButton.Location = new System.Drawing.Point(420, 307);
            this.yesDeleteButton.Name = "yesDeleteButton";
            this.yesDeleteButton.Size = new System.Drawing.Size(106, 23);
            this.yesDeleteButton.TabIndex = 3;
            this.yesDeleteButton.Text = "button1";
            this.yesDeleteButton.UseVisualStyleBackColor = true;
            this.yesDeleteButton.Click += new System.EventHandler(this.yesDeleteButton_Click);
            // 
            // noDeleteButton
            // 
            this.noDeleteButton.Location = new System.Drawing.Point(12, 307);
            this.noDeleteButton.Name = "noDeleteButton";
            this.noDeleteButton.Size = new System.Drawing.Size(98, 23);
            this.noDeleteButton.TabIndex = 4;
            this.noDeleteButton.Text = "button2";
            this.noDeleteButton.UseVisualStyleBackColor = true;
            this.noDeleteButton.Click += new System.EventHandler(this.noDeleteButton_Click);
            // 
            // OldFilesToDelete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 342);
            this.Controls.Add(this.noDeleteButton);
            this.Controls.Add(this.yesDeleteButton);
            this.Controls.Add(this.deleteFilesQuestion);
            this.Controls.Add(this.filesList);
            this.Controls.Add(this.deleteFilesHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "OldFilesToDelete";
            this.Text = "OldFilesToDelete";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OldFilesToDelete_FormClosing);
            this.Load += new System.EventHandler(this.OldFilesToDelete_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label deleteFilesHeader;
        private System.Windows.Forms.Label deleteFilesQuestion;
        private System.Windows.Forms.Button yesDeleteButton;
        private System.Windows.Forms.Button noDeleteButton;
        public System.Windows.Forms.RichTextBox filesList;
    }
}