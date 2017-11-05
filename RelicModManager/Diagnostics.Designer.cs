namespace RelhaxModpack
{
    partial class Diagnostics
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Diagnostics));
            this.MainTextBox = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.LaunchWoTLauncher = new System.Windows.Forms.Button();
            this.CollectLogInfo = new System.Windows.Forms.Button();
            this.StartWoTLauncherResult = new System.Windows.Forms.Label();
            this.CollectLogInfoResult = new System.Windows.Forms.Label();
            this.SelectedInstallation = new System.Windows.Forms.Label();
            this.ChangeInstall = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTextBox
            // 
            this.MainTextBox.Location = new System.Drawing.Point(15, 12);
            this.MainTextBox.Name = "MainTextBox";
            this.MainTextBox.ReadOnly = true;
            this.MainTextBox.Size = new System.Drawing.Size(543, 37);
            this.MainTextBox.TabIndex = 0;
            this.MainTextBox.Text = "";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.Controls.Add(this.LaunchWoTLauncher, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.CollectLogInfo, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.StartWoTLauncherResult, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.CollectLogInfoResult, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 109);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(546, 100);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // LaunchWoTLauncher
            // 
            this.LaunchWoTLauncher.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LaunchWoTLauncher.Location = new System.Drawing.Point(3, 3);
            this.LaunchWoTLauncher.Name = "LaunchWoTLauncher";
            this.LaunchWoTLauncher.Size = new System.Drawing.Size(212, 44);
            this.LaunchWoTLauncher.TabIndex = 0;
            this.LaunchWoTLauncher.Text = "Start Wot Launcher";
            this.LaunchWoTLauncher.UseVisualStyleBackColor = true;
            this.LaunchWoTLauncher.Click += new System.EventHandler(this.LaunchWoTLauncher_Click);
            // 
            // CollectLogInfo
            // 
            this.CollectLogInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CollectLogInfo.Location = new System.Drawing.Point(3, 53);
            this.CollectLogInfo.Name = "CollectLogInfo";
            this.CollectLogInfo.Size = new System.Drawing.Size(212, 44);
            this.CollectLogInfo.TabIndex = 1;
            this.CollectLogInfo.Text = "Collect Log info";
            this.CollectLogInfo.UseVisualStyleBackColor = true;
            this.CollectLogInfo.Click += new System.EventHandler(this.CollectLogInfo_Click);
            // 
            // StartWoTLauncherResult
            // 
            this.StartWoTLauncherResult.AutoSize = true;
            this.StartWoTLauncherResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StartWoTLauncherResult.Location = new System.Drawing.Point(221, 0);
            this.StartWoTLauncherResult.Name = "StartWoTLauncherResult";
            this.StartWoTLauncherResult.Size = new System.Drawing.Size(322, 50);
            this.StartWoTLauncherResult.TabIndex = 2;
            this.StartWoTLauncherResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CollectLogInfoResult
            // 
            this.CollectLogInfoResult.AutoSize = true;
            this.CollectLogInfoResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CollectLogInfoResult.Location = new System.Drawing.Point(221, 50);
            this.CollectLogInfoResult.Name = "CollectLogInfoResult";
            this.CollectLogInfoResult.Size = new System.Drawing.Size(322, 50);
            this.CollectLogInfoResult.TabIndex = 3;
            this.CollectLogInfoResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SelectedInstallation
            // 
            this.SelectedInstallation.AutoSize = true;
            this.SelectedInstallation.Location = new System.Drawing.Point(12, 52);
            this.SelectedInstallation.Name = "SelectedInstallation";
            this.SelectedInstallation.Size = new System.Drawing.Size(105, 13);
            this.SelectedInstallation.TabIndex = 2;
            this.SelectedInstallation.Text = "WoT Install Location";
            // 
            // ChangeInstall
            // 
            this.ChangeInstall.Location = new System.Drawing.Point(15, 68);
            this.ChangeInstall.Name = "ChangeInstall";
            this.ChangeInstall.Size = new System.Drawing.Size(212, 38);
            this.ChangeInstall.TabIndex = 3;
            this.ChangeInstall.Text = "Change Installation Directory";
            this.ChangeInstall.UseVisualStyleBackColor = true;
            this.ChangeInstall.Click += new System.EventHandler(this.ChangeInstallation_Click);
            // 
            // Diagnostics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 307);
            this.Controls.Add(this.ChangeInstall);
            this.Controls.Add(this.SelectedInstallation);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.MainTextBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Diagnostics";
            this.Text = "Diagnostics";
            this.Load += new System.EventHandler(this.Diagnostics_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox MainTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button LaunchWoTLauncher;
        private System.Windows.Forms.Button CollectLogInfo;
        private System.Windows.Forms.Label StartWoTLauncherResult;
        private System.Windows.Forms.Label CollectLogInfoResult;
        private System.Windows.Forms.Label SelectedInstallation;
        private System.Windows.Forms.Button ChangeInstall;
    }
}