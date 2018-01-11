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
            this.StartWoTLauncherResult = new System.Windows.Forms.Label();
            this.CollectLogInfoResult = new System.Windows.Forms.Label();
            this.SelectedInstallation = new System.Windows.Forms.Label();
            this.ChangeInstall = new System.Windows.Forms.Button();
            this.LaunchWoTLauncher = new System.Windows.Forms.Button();
            this.CollectLogInfo = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTextBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.MainTextBox, 2);
            this.MainTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTextBox.Location = new System.Drawing.Point(3, 3);
            this.MainTextBox.Name = "MainTextBox";
            this.MainTextBox.ReadOnly = true;
            this.MainTextBox.Size = new System.Drawing.Size(560, 54);
            this.MainTextBox.TabIndex = 0;
            this.MainTextBox.Text = "";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.Controls.Add(this.SelectedInstallation, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.ChangeInstall, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.LaunchWoTLauncher, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.CollectLogInfo, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.StartWoTLauncherResult, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.MainTextBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.CollectLogInfoResult, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(566, 243);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // StartWoTLauncherResult
            // 
            this.StartWoTLauncherResult.AutoSize = true;
            this.StartWoTLauncherResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StartWoTLauncherResult.Location = new System.Drawing.Point(229, 120);
            this.StartWoTLauncherResult.Name = "StartWoTLauncherResult";
            this.StartWoTLauncherResult.Size = new System.Drawing.Size(334, 60);
            this.StartWoTLauncherResult.TabIndex = 2;
            this.StartWoTLauncherResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CollectLogInfoResult
            // 
            this.CollectLogInfoResult.AutoSize = true;
            this.CollectLogInfoResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CollectLogInfoResult.Location = new System.Drawing.Point(229, 180);
            this.CollectLogInfoResult.Name = "CollectLogInfoResult";
            this.CollectLogInfoResult.Size = new System.Drawing.Size(334, 63);
            this.CollectLogInfoResult.TabIndex = 3;
            this.CollectLogInfoResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SelectedInstallation
            // 
            this.SelectedInstallation.AutoSize = true;
            this.SelectedInstallation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectedInstallation.Location = new System.Drawing.Point(229, 60);
            this.SelectedInstallation.Name = "SelectedInstallation";
            this.SelectedInstallation.Size = new System.Drawing.Size(334, 60);
            this.SelectedInstallation.TabIndex = 2;
            this.SelectedInstallation.Text = "WoT Install Location";
            // 
            // ChangeInstall
            // 
            this.ChangeInstall.AutoSize = true;
            this.ChangeInstall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChangeInstall.Location = new System.Drawing.Point(3, 63);
            this.ChangeInstall.Name = "ChangeInstall";
            this.ChangeInstall.Size = new System.Drawing.Size(220, 54);
            this.ChangeInstall.TabIndex = 3;
            this.ChangeInstall.Text = "Change Installation Directory";
            this.ChangeInstall.UseVisualStyleBackColor = true;
            this.ChangeInstall.Click += new System.EventHandler(this.ChangeInstallation_Click);
            // 
            // LaunchWoTLauncher
            // 
            this.LaunchWoTLauncher.AutoSize = true;
            this.LaunchWoTLauncher.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LaunchWoTLauncher.Location = new System.Drawing.Point(3, 123);
            this.LaunchWoTLauncher.Name = "LaunchWoTLauncher";
            this.LaunchWoTLauncher.Size = new System.Drawing.Size(220, 54);
            this.LaunchWoTLauncher.TabIndex = 0;
            this.LaunchWoTLauncher.Text = "Start Wot Launcher";
            this.LaunchWoTLauncher.UseVisualStyleBackColor = true;
            this.LaunchWoTLauncher.Click += new System.EventHandler(this.LaunchWoTLauncher_Click);
            // 
            // CollectLogInfo
            // 
            this.CollectLogInfo.AutoSize = true;
            this.CollectLogInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CollectLogInfo.Location = new System.Drawing.Point(3, 183);
            this.CollectLogInfo.Name = "CollectLogInfo";
            this.CollectLogInfo.Size = new System.Drawing.Size(220, 57);
            this.CollectLogInfo.TabIndex = 1;
            this.CollectLogInfo.Text = "Collect Log info";
            this.CollectLogInfo.UseVisualStyleBackColor = true;
            this.CollectLogInfo.Click += new System.EventHandler(this.CollectLogInfo_Click);
            // 
            // Diagnostics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 267);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "Diagnostics";
            this.Text = "Diagnostics";
            this.Load += new System.EventHandler(this.Diagnostics_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox MainTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label StartWoTLauncherResult;
        private System.Windows.Forms.Label CollectLogInfoResult;
        private System.Windows.Forms.Label SelectedInstallation;
        private System.Windows.Forms.Button ChangeInstall;
        private System.Windows.Forms.Button LaunchWoTLauncher;
        private System.Windows.Forms.Button CollectLogInfo;
    }
}