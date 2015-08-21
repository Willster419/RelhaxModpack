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
            this.downloadMods = new System.Windows.Forms.Button();
            this.stockSounds = new System.Windows.Forms.Button();
            this.backupCustom = new System.Windows.Forms.Button();
            this.restoreCustom = new System.Windows.Forms.Button();
            this.downloadProgress = new System.Windows.Forms.Label();
            this.customDownloadURL = new System.Windows.Forms.CheckBox();
            this.whatVersion = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.downloadProgressBar = new System.Windows.Forms.ProgressBar();
            this.downloadOnly = new System.Windows.Forms.CheckBox();
            this.findWotExe = new System.Windows.Forms.OpenFileDialog();
            this.forceManuel = new System.Windows.Forms.CheckBox();
            this.CensoredVersion = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // downloadMods
            // 
            this.downloadMods.Location = new System.Drawing.Point(12, 12);
            this.downloadMods.Name = "downloadMods";
            this.downloadMods.Size = new System.Drawing.Size(137, 34);
            this.downloadMods.TabIndex = 0;
            this.downloadMods.Text = "download latest relic mod";
            this.downloadMods.UseVisualStyleBackColor = true;
            this.downloadMods.Click += new System.EventHandler(this.downloadMods_Click);
            // 
            // stockSounds
            // 
            this.stockSounds.Location = new System.Drawing.Point(162, 92);
            this.stockSounds.Name = "stockSounds";
            this.stockSounds.Size = new System.Drawing.Size(137, 34);
            this.stockSounds.TabIndex = 1;
            this.stockSounds.Text = "download stock sounds";
            this.stockSounds.UseVisualStyleBackColor = true;
            this.stockSounds.Click += new System.EventHandler(this.stockSounds_Click);
            // 
            // backupCustom
            // 
            this.backupCustom.Location = new System.Drawing.Point(12, 52);
            this.backupCustom.Name = "backupCustom";
            this.backupCustom.Size = new System.Drawing.Size(137, 34);
            this.backupCustom.TabIndex = 2;
            this.backupCustom.Text = "backup custom sounds";
            this.backupCustom.UseVisualStyleBackColor = true;
            this.backupCustom.Click += new System.EventHandler(this.backupCustom_Click);
            // 
            // restoreCustom
            // 
            this.restoreCustom.Location = new System.Drawing.Point(162, 52);
            this.restoreCustom.Name = "restoreCustom";
            this.restoreCustom.Size = new System.Drawing.Size(137, 34);
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
            this.downloadProgress.Location = new System.Drawing.Point(9, 183);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(24, 13);
            this.downloadProgress.TabIndex = 4;
            this.downloadProgress.Text = "Idle";
            this.downloadProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // customDownloadURL
            // 
            this.customDownloadURL.AutoSize = true;
            this.customDownloadURL.Location = new System.Drawing.Point(162, 132);
            this.customDownloadURL.Name = "customDownloadURL";
            this.customDownloadURL.Size = new System.Drawing.Size(137, 17);
            this.customDownloadURL.TabIndex = 6;
            this.customDownloadURL.Text = "Custom Download URL";
            this.customDownloadURL.UseVisualStyleBackColor = true;
            // 
            // whatVersion
            // 
            this.whatVersion.Location = new System.Drawing.Point(12, 92);
            this.whatVersion.Name = "whatVersion";
            this.whatVersion.Size = new System.Drawing.Size(137, 34);
            this.whatVersion.TabIndex = 7;
            this.whatVersion.Text = "version info/ check for updates";
            this.whatVersion.UseVisualStyleBackColor = true;
            this.whatVersion.Click += new System.EventHandler(this.whatVersion_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(9, 159);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(53, 13);
            this.statusLabel.TabIndex = 10;
            this.statusLabel.Text = "STATUS:";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // downloadProgressBar
            // 
            this.downloadProgressBar.Location = new System.Drawing.Point(12, 199);
            this.downloadProgressBar.Name = "downloadProgressBar";
            this.downloadProgressBar.Size = new System.Drawing.Size(287, 23);
            this.downloadProgressBar.TabIndex = 11;
            // 
            // downloadOnly
            // 
            this.downloadOnly.AutoSize = true;
            this.downloadOnly.Location = new System.Drawing.Point(12, 132);
            this.downloadOnly.Name = "downloadOnly";
            this.downloadOnly.Size = new System.Drawing.Size(140, 17);
            this.downloadOnly.TabIndex = 12;
            this.downloadOnly.Text = "Download only no install";
            this.downloadOnly.UseVisualStyleBackColor = true;
            this.downloadOnly.Click += new System.EventHandler(this.downloadOnly_Click);
            // 
            // findWotExe
            // 
            this.findWotExe.Filter = "WorldOfTanks.exe|WorldOfTanks.exe";
            this.findWotExe.Title = "Find WorldOfTanks.exe";
            // 
            // forceManuel
            // 
            this.forceManuel.AutoSize = true;
            this.forceManuel.Location = new System.Drawing.Point(162, 155);
            this.forceManuel.Name = "forceManuel";
            this.forceManuel.Size = new System.Drawing.Size(140, 17);
            this.forceManuel.TabIndex = 13;
            this.forceManuel.Text = "Force Manuel Detection";
            this.forceManuel.UseVisualStyleBackColor = true;
            this.forceManuel.Click += new System.EventHandler(this.forceManuel_Click);
            // 
            // CensoredVersion
            // 
            this.CensoredVersion.Location = new System.Drawing.Point(162, 12);
            this.CensoredVersion.Name = "CensoredVersion";
            this.CensoredVersion.Size = new System.Drawing.Size(137, 34);
            this.CensoredVersion.TabIndex = 14;
            this.CensoredVersion.Text = "download latest censored relic mod";
            this.CensoredVersion.UseVisualStyleBackColor = true;
            this.CensoredVersion.Click += new System.EventHandler(this.CensoredVersion_Click_1);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 229);
            this.Controls.Add(this.CensoredVersion);
            this.Controls.Add(this.forceManuel);
            this.Controls.Add(this.downloadOnly);
            this.Controls.Add(this.downloadProgressBar);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.downloadProgress);
            this.Controls.Add(this.whatVersion);
            this.Controls.Add(this.customDownloadURL);
            this.Controls.Add(this.restoreCustom);
            this.Controls.Add(this.backupCustom);
            this.Controls.Add(this.stockSounds);
            this.Controls.Add(this.downloadMods);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainWindow";
            this.Text = "RelHax ModManager V10.1";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button downloadMods;
        private System.Windows.Forms.Button stockSounds;
        private System.Windows.Forms.Button backupCustom;
        private System.Windows.Forms.Button restoreCustom;
        private System.Windows.Forms.Label downloadProgress;
        private System.Windows.Forms.CheckBox customDownloadURL;
        private System.Windows.Forms.Button whatVersion;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ProgressBar downloadProgressBar;
        private System.Windows.Forms.CheckBox downloadOnly;
        private System.Windows.Forms.OpenFileDialog findWotExe;
        private System.Windows.Forms.CheckBox forceManuel;
        private System.Windows.Forms.Button CensoredVersion;
    }
}

