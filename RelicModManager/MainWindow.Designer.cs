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
            this.installRelhaxSound = new System.Windows.Forms.Button();
            this.uninstallRelhaxSound = new System.Windows.Forms.Button();
            this.downloadProgress = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.childProgressBar = new System.Windows.Forms.ProgressBar();
            this.findWotExe = new System.Windows.Forms.OpenFileDialog();
            this.forceManuel = new System.Windows.Forms.CheckBox();
            this.formPageLink = new System.Windows.Forms.LinkLabel();
            this.parrentProgressBar = new System.Windows.Forms.ProgressBar();
            this.speedLabel = new System.Windows.Forms.Label();
            this.installRelhaxMod = new System.Windows.Forms.Button();
            this.uninstallRelhaxMod = new System.Windows.Forms.Button();
            this.cleanInstallCB = new System.Windows.Forms.CheckBox();
            this.cancerFontCB = new System.Windows.Forms.CheckBox();
            this.backupModsCheckBox = new System.Windows.Forms.CheckBox();
            this.settingsGroupBox = new System.Windows.Forms.GroupBox();
            this.settingsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // installRelhaxSound
            // 
            this.installRelhaxSound.Location = new System.Drawing.Point(12, 12);
            this.installRelhaxSound.Name = "installRelhaxSound";
            this.installRelhaxSound.Size = new System.Drawing.Size(129, 34);
            this.installRelhaxSound.TabIndex = 0;
            this.installRelhaxSound.Text = "Install Relhax Soundmod";
            this.installRelhaxSound.UseVisualStyleBackColor = true;
            this.installRelhaxSound.Click += new System.EventHandler(this.installRelhax_Click);
            // 
            // uninstallRelhaxSound
            // 
            this.uninstallRelhaxSound.Location = new System.Drawing.Point(12, 92);
            this.uninstallRelhaxSound.Name = "uninstallRelhaxSound";
            this.uninstallRelhaxSound.Size = new System.Drawing.Size(129, 34);
            this.uninstallRelhaxSound.TabIndex = 1;
            this.uninstallRelhaxSound.Text = "Uninstall Relhax Soundmod";
            this.uninstallRelhaxSound.UseVisualStyleBackColor = true;
            this.uninstallRelhaxSound.Click += new System.EventHandler(this.uninstallRelhax_Click);
            // 
            // downloadProgress
            // 
            this.downloadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.downloadProgress.AutoSize = true;
            this.downloadProgress.Location = new System.Drawing.Point(9, 241);
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
            this.statusLabel.Location = new System.Drawing.Point(9, 228);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(53, 13);
            this.statusLabel.TabIndex = 10;
            this.statusLabel.Text = "STATUS:";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // childProgressBar
            // 
            this.childProgressBar.Location = new System.Drawing.Point(12, 286);
            this.childProgressBar.Name = "childProgressBar";
            this.childProgressBar.Size = new System.Drawing.Size(265, 23);
            this.childProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.childProgressBar.TabIndex = 11;
            // 
            // findWotExe
            // 
            this.findWotExe.Filter = "WorldOfTanks.exe|WorldOfTanks.exe";
            this.findWotExe.Title = "Find WorldOfTanks.exe";
            // 
            // forceManuel
            // 
            this.forceManuel.AutoSize = true;
            this.forceManuel.Location = new System.Drawing.Point(6, 19);
            this.forceManuel.Name = "forceManuel";
            this.forceManuel.Size = new System.Drawing.Size(166, 17);
            this.forceManuel.TabIndex = 13;
            this.forceManuel.Text = "Force manual game detection";
            this.forceManuel.UseVisualStyleBackColor = true;
            this.forceManuel.CheckedChanged += new System.EventHandler(this.forceManuel_CheckedChanged);
            // 
            // formPageLink
            // 
            this.formPageLink.AutoSize = true;
            this.formPageLink.Location = new System.Drawing.Point(9, 325);
            this.formPageLink.Name = "formPageLink";
            this.formPageLink.Size = new System.Drawing.Size(84, 13);
            this.formPageLink.TabIndex = 16;
            this.formPageLink.TabStop = true;
            this.formPageLink.Text = "View Form Page";
            this.formPageLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.formPageLink_LinkClicked);
            // 
            // parrentProgressBar
            // 
            this.parrentProgressBar.Location = new System.Drawing.Point(12, 257);
            this.parrentProgressBar.Name = "parrentProgressBar";
            this.parrentProgressBar.Size = new System.Drawing.Size(265, 23);
            this.parrentProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.parrentProgressBar.TabIndex = 17;
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(12, 312);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(24, 13);
            this.speedLabel.TabIndex = 18;
            this.speedLabel.Text = "Idle";
            // 
            // installRelhaxMod
            // 
            this.installRelhaxMod.Location = new System.Drawing.Point(147, 12);
            this.installRelhaxMod.Name = "installRelhaxMod";
            this.installRelhaxMod.Size = new System.Drawing.Size(130, 34);
            this.installRelhaxMod.TabIndex = 19;
            this.installRelhaxMod.Text = "Install Relhax ModPack";
            this.installRelhaxMod.UseVisualStyleBackColor = true;
            this.installRelhaxMod.Click += new System.EventHandler(this.button1_Click);
            // 
            // uninstallRelhaxMod
            // 
            this.uninstallRelhaxMod.Location = new System.Drawing.Point(147, 92);
            this.uninstallRelhaxMod.Name = "uninstallRelhaxMod";
            this.uninstallRelhaxMod.Size = new System.Drawing.Size(130, 34);
            this.uninstallRelhaxMod.TabIndex = 20;
            this.uninstallRelhaxMod.Text = "Uninstall Relhax Modpack";
            this.uninstallRelhaxMod.UseVisualStyleBackColor = true;
            this.uninstallRelhaxMod.Click += new System.EventHandler(this.button2_Click);
            // 
            // cleanInstallCB
            // 
            this.cleanInstallCB.AutoSize = true;
            this.cleanInstallCB.Checked = true;
            this.cleanInstallCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cleanInstallCB.Location = new System.Drawing.Point(6, 35);
            this.cleanInstallCB.Name = "cleanInstallCB";
            this.cleanInstallCB.Size = new System.Drawing.Size(187, 17);
            this.cleanInstallCB.TabIndex = 21;
            this.cleanInstallCB.Text = "Clean Installation (Recommended)";
            this.cleanInstallCB.UseVisualStyleBackColor = true;
            this.cleanInstallCB.CheckedChanged += new System.EventHandler(this.cleanInstallCB_CheckedChanged);
            // 
            // cancerFontCB
            // 
            this.cancerFontCB.AutoSize = true;
            this.cancerFontCB.Location = new System.Drawing.Point(6, 65);
            this.cancerFontCB.Name = "cancerFontCB";
            this.cancerFontCB.Size = new System.Drawing.Size(115, 17);
            this.cancerFontCB.TabIndex = 23;
            this.cancerFontCB.Text = "enable cancer font";
            this.cancerFontCB.UseVisualStyleBackColor = true;
            this.cancerFontCB.CheckedChanged += new System.EventHandler(this.cancerFontCB_CheckedChanged);
            // 
            // backupModsCheckBox
            // 
            this.backupModsCheckBox.AutoSize = true;
            this.backupModsCheckBox.Location = new System.Drawing.Point(6, 50);
            this.backupModsCheckBox.Name = "backupModsCheckBox";
            this.backupModsCheckBox.Size = new System.Drawing.Size(156, 17);
            this.backupModsCheckBox.TabIndex = 24;
            this.backupModsCheckBox.Text = "Backup current mods folder";
            this.backupModsCheckBox.UseVisualStyleBackColor = true;
            this.backupModsCheckBox.CheckedChanged += new System.EventHandler(this.backupModsCheckBox_CheckedChanged);
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Controls.Add(this.forceManuel);
            this.settingsGroupBox.Controls.Add(this.cancerFontCB);
            this.settingsGroupBox.Controls.Add(this.backupModsCheckBox);
            this.settingsGroupBox.Controls.Add(this.cleanInstallCB);
            this.settingsGroupBox.Location = new System.Drawing.Point(12, 132);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(265, 93);
            this.settingsGroupBox.TabIndex = 25;
            this.settingsGroupBox.TabStop = false;
            this.settingsGroupBox.Text = "RelHax ModPack Settings";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 341);
            this.Controls.Add(this.settingsGroupBox);
            this.Controls.Add(this.uninstallRelhaxMod);
            this.Controls.Add(this.installRelhaxMod);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.parrentProgressBar);
            this.Controls.Add(this.formPageLink);
            this.Controls.Add(this.childProgressBar);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.downloadProgress);
            this.Controls.Add(this.uninstallRelhaxSound);
            this.Controls.Add(this.installRelhaxSound);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "RelHax ";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.settingsGroupBox.ResumeLayout(false);
            this.settingsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button installRelhaxSound;
        private System.Windows.Forms.Button uninstallRelhaxSound;
        private System.Windows.Forms.Label downloadProgress;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ProgressBar childProgressBar;
        private System.Windows.Forms.OpenFileDialog findWotExe;
        private System.Windows.Forms.CheckBox forceManuel;
        private System.Windows.Forms.LinkLabel formPageLink;
        private System.Windows.Forms.ProgressBar parrentProgressBar;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.Button installRelhaxMod;
        private System.Windows.Forms.Button uninstallRelhaxMod;
        private System.Windows.Forms.CheckBox cleanInstallCB;
        private System.Windows.Forms.CheckBox cancerFontCB;
        private System.Windows.Forms.CheckBox backupModsCheckBox;
        private System.Windows.Forms.GroupBox settingsGroupBox;
    }
}

