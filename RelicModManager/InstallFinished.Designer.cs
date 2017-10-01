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
            this.SuspendLayout();
            // 
            // InstallCompleteLabel
            // 
            this.InstallCompleteLabel.AutoSize = true;
            this.InstallCompleteLabel.Location = new System.Drawing.Point(9, 9);
            this.InstallCompleteLabel.Name = "InstallCompleteLabel";
            this.InstallCompleteLabel.Size = new System.Drawing.Size(178, 13);
            this.InstallCompleteLabel.TabIndex = 0;
            this.InstallCompleteLabel.Text = "Install Complete. Would you like to...";
            // 
            // StartTanksButton
            // 
            this.StartTanksButton.Location = new System.Drawing.Point(12, 25);
            this.StartTanksButton.Name = "StartTanksButton";
            this.StartTanksButton.Size = new System.Drawing.Size(267, 23);
            this.StartTanksButton.TabIndex = 1;
            this.StartTanksButton.Text = "Start World of Tanks";
            this.StartTanksButton.UseVisualStyleBackColor = true;
            this.StartTanksButton.Click += new System.EventHandler(this.StartTanksButton_Click);
            // 
            // StartWoTLauncherButton
            // 
            this.StartWoTLauncherButton.Location = new System.Drawing.Point(12, 54);
            this.StartWoTLauncherButton.Name = "StartWoTLauncherButton";
            this.StartWoTLauncherButton.Size = new System.Drawing.Size(267, 23);
            this.StartWoTLauncherButton.TabIndex = 2;
            this.StartWoTLauncherButton.Text = "Start WoT Launcher";
            this.StartWoTLauncherButton.UseVisualStyleBackColor = true;
            this.StartWoTLauncherButton.Click += new System.EventHandler(this.StartWoTLauncherButton_Click);
            // 
            // StartXVMStatButton
            // 
            this.StartXVMStatButton.Location = new System.Drawing.Point(12, 83);
            this.StartXVMStatButton.Name = "StartXVMStatButton";
            this.StartXVMStatButton.Size = new System.Drawing.Size(267, 23);
            this.StartXVMStatButton.TabIndex = 3;
            this.StartXVMStatButton.Text = "Go to xvm stat website";
            this.StartXVMStatButton.UseVisualStyleBackColor = true;
            this.StartXVMStatButton.Click += new System.EventHandler(this.StartXVMStatButton_Click);
            // 
            // InstallFinished
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 151);
            this.Controls.Add(this.StartXVMStatButton);
            this.Controls.Add(this.StartWoTLauncherButton);
            this.Controls.Add(this.StartTanksButton);
            this.Controls.Add(this.InstallCompleteLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "InstallFinished";
            this.Text = "Install Finished";
            this.Load += new System.EventHandler(this.InstallFinished_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label InstallCompleteLabel;
        private System.Windows.Forms.Button StartTanksButton;
        private System.Windows.Forms.Button StartWoTLauncherButton;
        private System.Windows.Forms.Button StartXVMStatButton;
    }
}