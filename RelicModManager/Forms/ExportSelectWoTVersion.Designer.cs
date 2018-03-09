namespace RelhaxModpack.Forms
{
    partial class ExportSelectWoTVersion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportSelectWoTVersion));
            this.WoTVersionsHolder = new System.Windows.Forms.Panel();
            this.ExportWindowDesctiption = new System.Windows.Forms.Label();
            this.SelectButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // WoTVersionsHolder
            // 
            this.WoTVersionsHolder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WoTVersionsHolder.AutoScroll = true;
            this.WoTVersionsHolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WoTVersionsHolder.Location = new System.Drawing.Point(12, 49);
            this.WoTVersionsHolder.MaximumSize = new System.Drawing.Size(266, 452);
            this.WoTVersionsHolder.MinimumSize = new System.Drawing.Size(266, 185);
            this.WoTVersionsHolder.Name = "WoTVersionsHolder";
            this.WoTVersionsHolder.Size = new System.Drawing.Size(266, 185);
            this.WoTVersionsHolder.TabIndex = 0;
            // 
            // ExportWindowDesctiption
            // 
            this.ExportWindowDesctiption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportWindowDesctiption.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ExportWindowDesctiption.Location = new System.Drawing.Point(12, 9);
            this.ExportWindowDesctiption.Name = "ExportWindowDesctiption";
            this.ExportWindowDesctiption.Size = new System.Drawing.Size(266, 37);
            this.ExportWindowDesctiption.TabIndex = 1;
            this.ExportWindowDesctiption.Text = "label1";
            // 
            // SelectButton
            // 
            this.SelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectButton.Location = new System.Drawing.Point(203, 240);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(75, 23);
            this.SelectButton.TabIndex = 2;
            this.SelectButton.Text = "Select";
            this.SelectButton.UseVisualStyleBackColor = true;
            this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CancelButton.Location = new System.Drawing.Point(12, 240);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ExportSelectWoTVersion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 267);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.ExportWindowDesctiption);
            this.Controls.Add(this.WoTVersionsHolder);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(300, 569);
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "ExportSelectWoTVersion";
            this.Text = "ExportSelectWoTVersion";
            this.Load += new System.EventHandler(this.ExportSelectWoTVersion_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel WoTVersionsHolder;
        private System.Windows.Forms.Label ExportWindowDesctiption;
        private System.Windows.Forms.Button SelectButton;
        private System.Windows.Forms.Button CancelButton;
    }
}