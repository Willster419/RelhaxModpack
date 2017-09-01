namespace RelhaxModpack
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
            this.loadingDescBox = new System.Windows.Forms.RichTextBox();
            this.backgroundPanel = new System.Windows.Forms.Panel();
            this.forgroundPanel = new System.Windows.Forms.Panel();
            this.backgroundPanel.SuspendLayout();
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
            this.label1.UseWaitCursor = true;
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
            this.loadingDescBox.UseWaitCursor = true;
            // 
            // backgroundPanel
            // 
            this.backgroundPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.backgroundPanel.Controls.Add(this.forgroundPanel);
            this.backgroundPanel.Location = new System.Drawing.Point(12, 87);
            this.backgroundPanel.Name = "backgroundPanel";
            this.backgroundPanel.Size = new System.Drawing.Size(262, 22);
            this.backgroundPanel.TabIndex = 4;
            // 
            // forgroundPanel
            // 
            this.forgroundPanel.BackColor = System.Drawing.Color.Blue;
            this.forgroundPanel.ForeColor = System.Drawing.Color.Blue;
            this.forgroundPanel.Location = new System.Drawing.Point(3, 3);
            this.forgroundPanel.Name = "forgroundPanel";
            this.forgroundPanel.Size = new System.Drawing.Size(252, 12);
            this.forgroundPanel.TabIndex = 5;
            // 
            // PleaseWait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 113);
            this.ControlBox = false;
            this.Controls.Add(this.backgroundPanel);
            this.Controls.Add(this.loadingDescBox);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PleaseWait";
            this.Text = "Loading Window";
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.PleaseWait_Load);
            this.backgroundPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox loadingDescBox;
        private System.Windows.Forms.Panel backgroundPanel;
        private System.Windows.Forms.Panel forgroundPanel;
    }
}