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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OldFilesToDelete));
            this.deleteFilesHeader = new System.Windows.Forms.Label();
            this.filesList = new System.Windows.Forms.RichTextBox();
            this.deleteFilesQuestion = new System.Windows.Forms.Label();
            this.yesDeleteButton = new System.Windows.Forms.Button();
            this.noDeleteButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // deleteFilesHeader
            // 
            this.deleteFilesHeader.AutoSize = true;
            this.deleteFilesHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deleteFilesHeader.Location = new System.Drawing.Point(3, 8);
            this.deleteFilesHeader.Margin = new System.Windows.Forms.Padding(3);
            this.deleteFilesHeader.Name = "deleteFilesHeader";
            this.deleteFilesHeader.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.deleteFilesHeader.Size = new System.Drawing.Size(484, 14);
            this.deleteFilesHeader.TabIndex = 0;
            this.deleteFilesHeader.Text = "label1";
            // 
            // filesList
            // 
            this.filesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filesList.Location = new System.Drawing.Point(6, 28);
            this.filesList.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.filesList.Name = "filesList";
            this.filesList.ReadOnly = true;
            this.filesList.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.filesList.Size = new System.Drawing.Size(478, 201);
            this.filesList.TabIndex = 1;
            this.filesList.Text = "";
            // 
            // deleteFilesQuestion
            // 
            this.deleteFilesQuestion.AutoSize = true;
            this.deleteFilesQuestion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deleteFilesQuestion.Location = new System.Drawing.Point(164, 0);
            this.deleteFilesQuestion.Name = "deleteFilesQuestion";
            this.deleteFilesQuestion.Size = new System.Drawing.Size(155, 29);
            this.deleteFilesQuestion.TabIndex = 2;
            this.deleteFilesQuestion.Text = "label2";
            this.deleteFilesQuestion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // yesDeleteButton
            // 
            this.yesDeleteButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yesDeleteButton.Location = new System.Drawing.Point(325, 3);
            this.yesDeleteButton.Name = "yesDeleteButton";
            this.yesDeleteButton.Size = new System.Drawing.Size(156, 23);
            this.yesDeleteButton.TabIndex = 3;
            this.yesDeleteButton.Text = "button1";
            this.yesDeleteButton.UseVisualStyleBackColor = true;
            this.yesDeleteButton.Click += new System.EventHandler(this.yesDeleteButton_Click);
            // 
            // noDeleteButton
            // 
            this.noDeleteButton.AutoSize = true;
            this.noDeleteButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.noDeleteButton.Location = new System.Drawing.Point(3, 3);
            this.noDeleteButton.Name = "noDeleteButton";
            this.noDeleteButton.Size = new System.Drawing.Size(155, 23);
            this.noDeleteButton.TabIndex = 4;
            this.noDeleteButton.Text = "button2";
            this.noDeleteButton.UseVisualStyleBackColor = true;
            this.noDeleteButton.Click += new System.EventHandler(this.noDeleteButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.deleteFilesHeader, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.filesList, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(490, 267);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanel2.Controls.Add(this.yesDeleteButton, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.noDeleteButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.deleteFilesQuestion, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 235);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(484, 29);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // OldFilesToDelete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 267);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "OldFilesToDelete";
            this.Text = "Old Files Question";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OldFilesToDelete_FormClosing);
            this.Load += new System.EventHandler(this.OldFilesToDelete_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label deleteFilesHeader;
        private System.Windows.Forms.Label deleteFilesQuestion;
        private System.Windows.Forms.Button yesDeleteButton;
        private System.Windows.Forms.Button noDeleteButton;
        public System.Windows.Forms.RichTextBox filesList;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}