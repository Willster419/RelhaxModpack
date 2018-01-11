namespace RelhaxModpack
{
    partial class InstallFinished
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallFinished));
            this.InstallCompleteLabel = new System.Windows.Forms.Label();
            this.StartTanksButton = new System.Windows.Forms.Button();
            this.StartWoTLauncherButton = new System.Windows.Forms.Button();
            this.StartXVMStatButton = new System.Windows.Forms.Button();
            this.CloseApplicationButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // InstallCompleteLabel
            // 
            this.InstallCompleteLabel.AutoSize = true;
            this.InstallCompleteLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InstallCompleteLabel.Location = new System.Drawing.Point(3, 0);
            this.InstallCompleteLabel.Name = "InstallCompleteLabel";
            this.InstallCompleteLabel.Size = new System.Drawing.Size(330, 27);
            this.InstallCompleteLabel.TabIndex = 0;
            this.InstallCompleteLabel.Text = "Install Complete. Would you like to...";
            this.InstallCompleteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StartTanksButton
            // 
            this.StartTanksButton.AutoSize = true;
            this.StartTanksButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StartTanksButton.Location = new System.Drawing.Point(3, 30);
            this.StartTanksButton.Name = "StartTanksButton";
            this.StartTanksButton.Size = new System.Drawing.Size(330, 21);
            this.StartTanksButton.TabIndex = 1;
            this.StartTanksButton.Text = "Start World of Tanks";
            this.StartTanksButton.UseVisualStyleBackColor = true;
            this.StartTanksButton.Click += new System.EventHandler(this.StartTanksButton_Click);
            // 
            // StartWoTLauncherButton
            // 
            this.StartWoTLauncherButton.AutoSize = true;
            this.StartWoTLauncherButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StartWoTLauncherButton.Location = new System.Drawing.Point(3, 57);
            this.StartWoTLauncherButton.Name = "StartWoTLauncherButton";
            this.StartWoTLauncherButton.Size = new System.Drawing.Size(330, 21);
            this.StartWoTLauncherButton.TabIndex = 2;
            this.StartWoTLauncherButton.Text = "Start WoT Launcher";
            this.StartWoTLauncherButton.UseVisualStyleBackColor = true;
            this.StartWoTLauncherButton.Click += new System.EventHandler(this.StartWoTLauncherButton_Click);
            // 
            // StartXVMStatButton
            // 
            this.StartXVMStatButton.AutoSize = true;
            this.StartXVMStatButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StartXVMStatButton.Location = new System.Drawing.Point(3, 84);
            this.StartXVMStatButton.Name = "StartXVMStatButton";
            this.StartXVMStatButton.Size = new System.Drawing.Size(330, 21);
            this.StartXVMStatButton.TabIndex = 3;
            this.StartXVMStatButton.Text = "Go to xvm stat website";
            this.StartXVMStatButton.UseVisualStyleBackColor = true;
            this.StartXVMStatButton.Click += new System.EventHandler(this.StartXVMStatButton_Click);
            // 
            // CloseApplicationButton
            // 
            this.CloseApplicationButton.AutoSize = true;
            this.CloseApplicationButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CloseApplicationButton.Location = new System.Drawing.Point(3, 111);
            this.CloseApplicationButton.Name = "CloseApplicationButton";
            this.CloseApplicationButton.Size = new System.Drawing.Size(330, 24);
            this.CloseApplicationButton.TabIndex = 4;
            this.CloseApplicationButton.Text = "Close the Application";
            this.CloseApplicationButton.UseVisualStyleBackColor = true;
            this.CloseApplicationButton.Click += new System.EventHandler(this.CloseApplicationButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.InstallCompleteLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.CloseApplicationButton, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.StartTanksButton, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.StartXVMStatButton, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.StartWoTLauncherButton, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(336, 138);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // InstallFinished
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 138);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 175);
            this.Name = "InstallFinished";
            this.Text = "Install Finished";
            this.Load += new System.EventHandler(this.InstallFinished_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label InstallCompleteLabel;
        private System.Windows.Forms.Button StartTanksButton;
        private System.Windows.Forms.Button StartWoTLauncherButton;
        private System.Windows.Forms.Button StartXVMStatButton;
        private System.Windows.Forms.Button CloseApplicationButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}