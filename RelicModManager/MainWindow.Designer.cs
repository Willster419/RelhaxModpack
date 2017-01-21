namespace RelicModManager
{
    partial class MainWindow
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
            this.installRelhax = new System.Windows.Forms.Button();
            this.uninstallRelhax = new System.Windows.Forms.Button();
            this.downloadProgress = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.downloadProgressBar = new System.Windows.Forms.ProgressBar();
            this.findWotExe = new System.Windows.Forms.OpenFileDialog();
            this.forceManuel = new System.Windows.Forms.CheckBox();
            this.downloadNumberCount = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.formPageLink = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // installRelhax
            // 
            this.installRelhax.Location = new System.Drawing.Point(12, 12);
            this.installRelhax.Name = "installRelhax";
            this.installRelhax.Size = new System.Drawing.Size(129, 34);
            this.installRelhax.TabIndex = 0;
            this.installRelhax.Text = "Install latest Relhax";
            this.installRelhax.UseVisualStyleBackColor = true;
            this.installRelhax.Click += new System.EventHandler(this.installRelhax_Click);
            // 
            // uninstallRelhax
            // 
            this.uninstallRelhax.Location = new System.Drawing.Point(12, 92);
            this.uninstallRelhax.Name = "uninstallRelhax";
            this.uninstallRelhax.Size = new System.Drawing.Size(129, 34);
            this.uninstallRelhax.TabIndex = 1;
            this.uninstallRelhax.Text = "Uninstall Relhax";
            this.uninstallRelhax.UseVisualStyleBackColor = true;
            this.uninstallRelhax.Click += new System.EventHandler(this.uninstallRelhax_Click);
            // 
            // downloadProgress
            // 
            this.downloadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.downloadProgress.AutoSize = true;
            this.downloadProgress.Location = new System.Drawing.Point(9, 182);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(24, 13);
            this.downloadProgress.TabIndex = 4;
            this.downloadProgress.Text = "Idle";
            this.downloadProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(9, 152);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(53, 13);
            this.statusLabel.TabIndex = 10;
            this.statusLabel.Text = "STATUS:";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // downloadProgressBar
            // 
            this.downloadProgressBar.Location = new System.Drawing.Point(12, 198);
            this.downloadProgressBar.Name = "downloadProgressBar";
            this.downloadProgressBar.Size = new System.Drawing.Size(265, 23);
            this.downloadProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.downloadProgressBar.TabIndex = 11;
            // 
            // findWotExe
            // 
            this.findWotExe.Filter = "WorldOfTanks.exe|WorldOfTanks.exe";
            this.findWotExe.Title = "Find WorldOfTanks.exe";
            // 
            // forceManuel
            // 
            this.forceManuel.AutoSize = true;
            this.forceManuel.Location = new System.Drawing.Point(12, 132);
            this.forceManuel.Name = "forceManuel";
            this.forceManuel.Size = new System.Drawing.Size(166, 17);
            this.forceManuel.TabIndex = 13;
            this.forceManuel.Text = "Force manual game detection";
            this.forceManuel.UseVisualStyleBackColor = true;
            // 
            // downloadNumberCount
            // 
            this.downloadNumberCount.AutoSize = true;
            this.downloadNumberCount.Location = new System.Drawing.Point(9, 165);
            this.downloadNumberCount.Name = "downloadNumberCount";
            this.downloadNumberCount.Size = new System.Drawing.Size(24, 13);
            this.downloadNumberCount.TabIndex = 15;
            this.downloadNumberCount.Text = "Idle";
            this.downloadNumberCount.Visible = false;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // formPageLink
            // 
            this.formPageLink.AutoSize = true;
            this.formPageLink.Location = new System.Drawing.Point(9, 224);
            this.formPageLink.Name = "formPageLink";
            this.formPageLink.Size = new System.Drawing.Size(84, 13);
            this.formPageLink.TabIndex = 16;
            this.formPageLink.TabStop = true;
            this.formPageLink.Text = "View Form Page";
            this.formPageLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.formPageLink_LinkClicked);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 240);
            this.Controls.Add(this.formPageLink);
            this.Controls.Add(this.downloadNumberCount);
            this.Controls.Add(this.forceManuel);
            this.Controls.Add(this.downloadProgressBar);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.downloadProgress);
            this.Controls.Add(this.uninstallRelhax);
            this.Controls.Add(this.installRelhax);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainWindow";
            this.Text = "RelHax ";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button installRelhax;
        private System.Windows.Forms.Button uninstallRelhax;
        private System.Windows.Forms.Label downloadProgress;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ProgressBar downloadProgressBar;
        private System.Windows.Forms.OpenFileDialog findWotExe;
        private System.Windows.Forms.CheckBox forceManuel;
        private System.Windows.Forms.Label downloadNumberCount;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.LinkLabel formPageLink;
    }
}

