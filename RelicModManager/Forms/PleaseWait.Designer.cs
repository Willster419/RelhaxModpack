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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.backgroundPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(8, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(274, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Loading...please wait...";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.UseWaitCursor = true;
            // 
            // loadingDescBox
            // 
            this.loadingDescBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingDescBox.Location = new System.Drawing.Point(8, 34);
            this.loadingDescBox.Name = "loadingDescBox";
            this.loadingDescBox.ReadOnly = true;
            this.loadingDescBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.loadingDescBox.Size = new System.Drawing.Size(274, 72);
            this.loadingDescBox.TabIndex = 2;
            this.loadingDescBox.Text = "";
            this.loadingDescBox.UseWaitCursor = true;
            // 
            // backgroundPanel
            // 
            this.backgroundPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.backgroundPanel.Controls.Add(this.forgroundPanel);
            this.backgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.backgroundPanel.Location = new System.Drawing.Point(8, 112);
            this.backgroundPanel.Name = "backgroundPanel";
            this.backgroundPanel.Size = new System.Drawing.Size(274, 20);
            this.backgroundPanel.TabIndex = 4;
            this.backgroundPanel.UseWaitCursor = true;
            // 
            // forgroundPanel
            // 
            this.forgroundPanel.BackColor = System.Drawing.Color.Blue;
            this.forgroundPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.forgroundPanel.ForeColor = System.Drawing.Color.Blue;
            this.forgroundPanel.Location = new System.Drawing.Point(0, 0);
            this.forgroundPanel.Name = "forgroundPanel";
            this.forgroundPanel.Padding = new System.Windows.Forms.Padding(2);
            this.forgroundPanel.Size = new System.Drawing.Size(272, 16);
            this.forgroundPanel.TabIndex = 5;
            this.forgroundPanel.UseWaitCursor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.backgroundPanel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.loadingDescBox, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(290, 140);
            this.tableLayoutPanel1.TabIndex = 5;
            this.tableLayoutPanel1.UseWaitCursor = true;
            // 
            // PleaseWait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 140);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 150);
            this.Name = "PleaseWait";
            this.Text = "Loading Window";
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.PleaseWait_Load);
            this.backgroundPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox loadingDescBox;
        private System.Windows.Forms.Panel backgroundPanel;
        private System.Windows.Forms.Panel forgroundPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}