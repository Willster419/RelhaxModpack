namespace RelhaxModpack
{
    partial class SelectionViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectionViewer));
            this.SelectConfigLabel = new System.Windows.Forms.Label();
            this.SelectButton = new System.Windows.Forms.Button();
            this.CancelCloseButton = new System.Windows.Forms.Button();
            this.SelectConfigPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // SelectConfigLabel
            // 
            this.SelectConfigLabel.AutoSize = true;
            this.SelectConfigLabel.Location = new System.Drawing.Point(12, 9);
            this.SelectConfigLabel.Name = "SelectConfigLabel";
            this.SelectConfigLabel.Size = new System.Drawing.Size(35, 13);
            this.SelectConfigLabel.TabIndex = 0;
            this.SelectConfigLabel.Text = "label1";
            // 
            // SelectButton
            // 
            this.SelectButton.Location = new System.Drawing.Point(203, 232);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(75, 23);
            this.SelectButton.TabIndex = 1;
            this.SelectButton.Text = "button1";
            this.SelectButton.UseVisualStyleBackColor = true;
            this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
            // 
            // CancelCloseButton
            // 
            this.CancelCloseButton.Location = new System.Drawing.Point(12, 232);
            this.CancelCloseButton.Name = "CancelCloseButton";
            this.CancelCloseButton.Size = new System.Drawing.Size(75, 23);
            this.CancelCloseButton.TabIndex = 2;
            this.CancelCloseButton.Text = "button2";
            this.CancelCloseButton.UseVisualStyleBackColor = true;
            this.CancelCloseButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // SelectConfigPanel
            // 
            this.SelectConfigPanel.AutoScroll = true;
            this.SelectConfigPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SelectConfigPanel.Location = new System.Drawing.Point(15, 25);
            this.SelectConfigPanel.Name = "SelectConfigPanel";
            this.SelectConfigPanel.Size = new System.Drawing.Size(263, 201);
            this.SelectConfigPanel.TabIndex = 3;
            // 
            // SelectionViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 267);
            this.Controls.Add(this.SelectConfigPanel);
            this.Controls.Add(this.CancelCloseButton);
            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.SelectConfigLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SelectionViewer";
            this.Text = "Selection Viewer";
            this.Load += new System.EventHandler(this.SelectionViewer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SelectConfigLabel;
        private System.Windows.Forms.Button SelectButton;
        private System.Windows.Forms.Button CancelCloseButton;
        private System.Windows.Forms.Panel SelectConfigPanel;
    }
}