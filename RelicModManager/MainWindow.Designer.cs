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
            this.installMods = new System.Windows.Forms.Button();
            this.stockSounds = new System.Windows.Forms.Button();
            this.backupCustom = new System.Windows.Forms.Button();
            this.restoreCustom = new System.Windows.Forms.Button();
            this.downloadProgress = new System.Windows.Forms.Label();
            this.whatVersion = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.downloadProgressBar = new System.Windows.Forms.ProgressBar();
            this.findWotExe = new System.Windows.Forms.OpenFileDialog();
            this.forceManuel = new System.Windows.Forms.CheckBox();
            this.downloadRelhax = new System.Windows.Forms.Button();
            this.downloadNumberCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // installMods
            // 
            this.installMods.Location = new System.Drawing.Point(12, 12);
            this.installMods.Name = "installMods";
            this.installMods.Size = new System.Drawing.Size(129, 34);
            this.installMods.TabIndex = 0;
            this.installMods.Text = "install latest relhax";
            this.installMods.UseVisualStyleBackColor = true;
            this.installMods.Click += new System.EventHandler(this.downloadMods_Click);
            // 
            // stockSounds
            // 
            this.stockSounds.Location = new System.Drawing.Point(12, 92);
            this.stockSounds.Name = "stockSounds";
            this.stockSounds.Size = new System.Drawing.Size(129, 34);
            this.stockSounds.TabIndex = 1;
            this.stockSounds.Text = "uninstall relhax";
            this.stockSounds.UseVisualStyleBackColor = true;
            this.stockSounds.Click += new System.EventHandler(this.stockSounds_Click);
            // 
            // backupCustom
            // 
            this.backupCustom.Location = new System.Drawing.Point(147, 12);
            this.backupCustom.Name = "backupCustom";
            this.backupCustom.Size = new System.Drawing.Size(129, 34);
            this.backupCustom.TabIndex = 2;
            this.backupCustom.Text = "backup custom sounds";
            this.backupCustom.UseVisualStyleBackColor = true;
            this.backupCustom.Click += new System.EventHandler(this.backupCustom_Click);
            // 
            // restoreCustom
            // 
            this.restoreCustom.Location = new System.Drawing.Point(147, 52);
            this.restoreCustom.Name = "restoreCustom";
            this.restoreCustom.Size = new System.Drawing.Size(129, 34);
            this.restoreCustom.TabIndex = 3;
            this.restoreCustom.Text = "restore custom sounds";
            this.restoreCustom.UseVisualStyleBackColor = true;
            this.restoreCustom.Click += new System.EventHandler(this.restoreCustom_Click);
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
            // whatVersion
            // 
            this.whatVersion.Location = new System.Drawing.Point(147, 92);
            this.whatVersion.Name = "whatVersion";
            this.whatVersion.Size = new System.Drawing.Size(129, 34);
            this.whatVersion.TabIndex = 7;
            this.whatVersion.Text = "version info";
            this.whatVersion.UseVisualStyleBackColor = true;
            this.whatVersion.Click += new System.EventHandler(this.whatVersion_Click);
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
            this.forceManuel.Size = new System.Drawing.Size(171, 17);
            this.forceManuel.TabIndex = 13;
            this.forceManuel.Text = "Force Manuel Game Detection";
            this.forceManuel.UseVisualStyleBackColor = true;
            // 
            // downloadRelhax
            // 
            this.downloadRelhax.Location = new System.Drawing.Point(12, 52);
            this.downloadRelhax.Name = "downloadRelhax";
            this.downloadRelhax.Size = new System.Drawing.Size(129, 34);
            this.downloadRelhax.TabIndex = 14;
            this.downloadRelhax.Text = "download latest relhax (no install)";
            this.downloadRelhax.UseVisualStyleBackColor = true;
            this.downloadRelhax.Click += new System.EventHandler(this.downloadRelhax_Click);
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
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 231);
            this.Controls.Add(this.downloadNumberCount);
            this.Controls.Add(this.downloadRelhax);
            this.Controls.Add(this.forceManuel);
            this.Controls.Add(this.downloadProgressBar);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.downloadProgress);
            this.Controls.Add(this.whatVersion);
            this.Controls.Add(this.restoreCustom);
            this.Controls.Add(this.backupCustom);
            this.Controls.Add(this.stockSounds);
            this.Controls.Add(this.installMods);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainWindow";
            this.Text = "RelHax V13.2";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button installMods;
        private System.Windows.Forms.Button stockSounds;
        private System.Windows.Forms.Button backupCustom;
        private System.Windows.Forms.Button restoreCustom;
        private System.Windows.Forms.Label downloadProgress;
        private System.Windows.Forms.Button whatVersion;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ProgressBar downloadProgressBar;
        private System.Windows.Forms.OpenFileDialog findWotExe;
        private System.Windows.Forms.CheckBox forceManuel;
        private System.Windows.Forms.Button downloadRelhax;
        private System.Windows.Forms.Label downloadNumberCount;
    }
}

