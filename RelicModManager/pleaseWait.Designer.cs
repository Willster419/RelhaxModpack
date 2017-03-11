namespace RelicModManager
{
    partial class PleaseWait
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PleaseWait));
            this.label1 = new System.Windows.Forms.Label();
            this.loadingDescLabel = new System.Windows.Forms.Label();
            this.loadingDescBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Loading...please wait...";
            // 
            // loadingDescLabel
            // 
            this.loadingDescLabel.AutoSize = true;
            this.loadingDescLabel.Enabled = false;
            this.loadingDescLabel.Location = new System.Drawing.Point(239, 9);
            this.loadingDescLabel.Name = "loadingDescLabel";
            this.loadingDescLabel.Size = new System.Drawing.Size(35, 13);
            this.loadingDescLabel.TabIndex = 1;
            this.loadingDescLabel.Text = "label2";
            this.loadingDescLabel.Visible = false;
            // 
            // loadingDescBox
            // 
            this.loadingDescBox.Location = new System.Drawing.Point(12, 25);
            this.loadingDescBox.Name = "loadingDescBox";
            this.loadingDescBox.ReadOnly = true;
            this.loadingDescBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.loadingDescBox.Size = new System.Drawing.Size(262, 56);
            this.loadingDescBox.TabIndex = 2;
            this.loadingDescBox.Text = "";
            // 
            // PleaseWait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 91);
            this.Controls.Add(this.loadingDescBox);
            this.Controls.Add(this.loadingDescLabel);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PleaseWait";
            this.Text = "pleaseWait";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label loadingDescLabel;
        public System.Windows.Forms.RichTextBox loadingDescBox;
    }
}