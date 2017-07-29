namespace RelhaxModpack
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.statusLabel = new System.Windows.Forms.Label();
            this.childProgressBar = new System.Windows.Forms.ProgressBar();
            this.findWotExe = new System.Windows.Forms.OpenFileDialog();
            this.forceManuel = new System.Windows.Forms.CheckBox();
            this.formPageLink = new System.Windows.Forms.LinkLabel();
            this.parrentProgressBar = new System.Windows.Forms.ProgressBar();
            this.installRelhaxMod = new System.Windows.Forms.Button();
            this.uninstallRelhaxMod = new System.Windows.Forms.Button();
            this.cleanInstallCB = new System.Windows.Forms.CheckBox();
            this.cancerFontCB = new System.Windows.Forms.CheckBox();
            this.backupModsCheckBox = new System.Windows.Forms.CheckBox();
            this.settingsGroupBox = new System.Windows.Forms.GroupBox();
            this.darkUICB = new System.Windows.Forms.CheckBox();
            this.cleanUninstallCB = new System.Windows.Forms.CheckBox();
            this.saveUserDataCB = new System.Windows.Forms.CheckBox();
            this.saveLastInstallCB = new System.Windows.Forms.CheckBox();
            this.languageSelectionGB = new System.Windows.Forms.GroupBox();
            this.languagePL = new System.Windows.Forms.RadioButton();
            this.languageGER = new System.Windows.Forms.RadioButton();
            this.languageENG = new System.Windows.Forms.RadioButton();
            this.loadingImageGroupBox = new System.Windows.Forms.GroupBox();
            this.thirdGuardsLoadingImageRB = new System.Windows.Forms.RadioButton();
            this.standardImageRB = new System.Windows.Forms.RadioButton();
            this.findBugAddModLabel = new System.Windows.Forms.LinkLabel();
            this.cancelDownloadButton = new System.Windows.Forms.Button();
            this.downloadTimer = new System.Windows.Forms.Timer(this.components);
            this.downloadProgress = new System.Windows.Forms.RichTextBox();
            this.viewTypeGB = new System.Windows.Forms.GroupBox();
            this.disableBordersCB = new System.Windows.Forms.CheckBox();
            this.expandNodesDefault = new System.Windows.Forms.CheckBox();
            this.selectionLegacy = new System.Windows.Forms.RadioButton();
            this.selectionDefault = new System.Windows.Forms.RadioButton();
            this.donateLabel = new System.Windows.Forms.LinkLabel();
            this.fontSizeGB = new System.Windows.Forms.GroupBox();
            this.DPILarge = new System.Windows.Forms.RadioButton();
            this.DPIUHD = new System.Windows.Forms.RadioButton();
            this.DPIDefault = new System.Windows.Forms.RadioButton();
            this.fontSizeHUD = new System.Windows.Forms.RadioButton();
            this.fontSizeLarge = new System.Windows.Forms.RadioButton();
            this.fontSizeDefault = new System.Windows.Forms.RadioButton();
            this.totalProgressBar = new System.Windows.Forms.ProgressBar();
            this.settingsGroupBox.SuspendLayout();
            this.languageSelectionGB.SuspendLayout();
            this.loadingImageGroupBox.SuspendLayout();
            this.viewTypeGB.SuspendLayout();
            this.fontSizeGB.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(9, 396);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(53, 13);
            this.statusLabel.TabIndex = 10;
            this.statusLabel.Text = "STATUS:";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // childProgressBar
            // 
            this.childProgressBar.Location = new System.Drawing.Point(12, 538);
            this.childProgressBar.Name = "childProgressBar";
            this.childProgressBar.Size = new System.Drawing.Size(302, 15);
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
            this.forceManuel.Location = new System.Drawing.Point(6, 14);
            this.forceManuel.Name = "forceManuel";
            this.forceManuel.Size = new System.Drawing.Size(166, 17);
            this.forceManuel.TabIndex = 13;
            this.forceManuel.Text = "Force manual game detection";
            this.forceManuel.UseVisualStyleBackColor = true;
            this.forceManuel.CheckedChanged += new System.EventHandler(this.forceManuel_CheckedChanged);
            this.forceManuel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.forceManuel_MouseDown);
            this.forceManuel.MouseEnter += new System.EventHandler(this.forceManuel_MouseEnter);
            this.forceManuel.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // formPageLink
            // 
            this.formPageLink.AutoSize = true;
            this.formPageLink.Location = new System.Drawing.Point(9, 578);
            this.formPageLink.Name = "formPageLink";
            this.formPageLink.Size = new System.Drawing.Size(132, 13);
            this.formPageLink.TabIndex = 16;
            this.formPageLink.TabStop = true;
            this.formPageLink.Text = "View Modpack Form Page";
            this.formPageLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.formPageLink_LinkClicked);
            // 
            // parrentProgressBar
            // 
            this.parrentProgressBar.Location = new System.Drawing.Point(12, 517);
            this.parrentProgressBar.Name = "parrentProgressBar";
            this.parrentProgressBar.Size = new System.Drawing.Size(302, 15);
            this.parrentProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.parrentProgressBar.TabIndex = 17;
            // 
            // installRelhaxMod
            // 
            this.installRelhaxMod.AutoSize = true;
            this.installRelhaxMod.Location = new System.Drawing.Point(12, 12);
            this.installRelhaxMod.Name = "installRelhaxMod";
            this.installRelhaxMod.Size = new System.Drawing.Size(302, 33);
            this.installRelhaxMod.TabIndex = 19;
            this.installRelhaxMod.Text = "Install Relhax ModPack";
            this.installRelhaxMod.UseVisualStyleBackColor = true;
            this.installRelhaxMod.Click += new System.EventHandler(this.installRelhaxMod_Click);
            // 
            // uninstallRelhaxMod
            // 
            this.uninstallRelhaxMod.Location = new System.Drawing.Point(12, 51);
            this.uninstallRelhaxMod.Name = "uninstallRelhaxMod";
            this.uninstallRelhaxMod.Size = new System.Drawing.Size(302, 33);
            this.uninstallRelhaxMod.TabIndex = 20;
            this.uninstallRelhaxMod.Text = "Uninstall Relhax Modpack";
            this.uninstallRelhaxMod.UseVisualStyleBackColor = true;
            this.uninstallRelhaxMod.Click += new System.EventHandler(this.uninstallRelhaxMod_Click);
            // 
            // cleanInstallCB
            // 
            this.cleanInstallCB.AutoSize = true;
            this.cleanInstallCB.Checked = true;
            this.cleanInstallCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cleanInstallCB.Location = new System.Drawing.Point(6, 31);
            this.cleanInstallCB.Name = "cleanInstallCB";
            this.cleanInstallCB.Size = new System.Drawing.Size(187, 17);
            this.cleanInstallCB.TabIndex = 21;
            this.cleanInstallCB.Text = "Clean Installation (Recommended)";
            this.cleanInstallCB.UseVisualStyleBackColor = true;
            this.cleanInstallCB.CheckedChanged += new System.EventHandler(this.cleanInstallCB_CheckedChanged);
            this.cleanInstallCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cleanInstallCB_MouseDown);
            this.cleanInstallCB.MouseEnter += new System.EventHandler(this.cleanInstallCB_MouseEnter);
            this.cleanInstallCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // cancerFontCB
            // 
            this.cancerFontCB.AutoSize = true;
            this.cancerFontCB.Location = new System.Drawing.Point(6, 64);
            this.cancerFontCB.Name = "cancerFontCB";
            this.cancerFontCB.Size = new System.Drawing.Size(81, 17);
            this.cancerFontCB.TabIndex = 23;
            this.cancerFontCB.Text = "Cancer font";
            this.cancerFontCB.UseVisualStyleBackColor = true;
            this.cancerFontCB.CheckedChanged += new System.EventHandler(this.cancerFontCB_CheckedChanged);
            this.cancerFontCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cancerFontCB_MouseDown);
            this.cancerFontCB.MouseEnter += new System.EventHandler(this.cancerFontCB_MouseEnter);
            this.cancerFontCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // backupModsCheckBox
            // 
            this.backupModsCheckBox.AutoSize = true;
            this.backupModsCheckBox.Location = new System.Drawing.Point(6, 48);
            this.backupModsCheckBox.Name = "backupModsCheckBox";
            this.backupModsCheckBox.Size = new System.Drawing.Size(156, 17);
            this.backupModsCheckBox.TabIndex = 24;
            this.backupModsCheckBox.Text = "Backup current mods folder";
            this.backupModsCheckBox.UseVisualStyleBackColor = true;
            this.backupModsCheckBox.CheckedChanged += new System.EventHandler(this.backupModsCheckBox_CheckedChanged);
            this.backupModsCheckBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.backupModsCheckBox_MouseDown);
            this.backupModsCheckBox.MouseEnter += new System.EventHandler(this.backupModsCheckBox_MouseEnter);
            this.backupModsCheckBox.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Controls.Add(this.darkUICB);
            this.settingsGroupBox.Controls.Add(this.cleanUninstallCB);
            this.settingsGroupBox.Controls.Add(this.saveUserDataCB);
            this.settingsGroupBox.Controls.Add(this.saveLastInstallCB);
            this.settingsGroupBox.Controls.Add(this.forceManuel);
            this.settingsGroupBox.Controls.Add(this.cancerFontCB);
            this.settingsGroupBox.Controls.Add(this.backupModsCheckBox);
            this.settingsGroupBox.Controls.Add(this.cleanInstallCB);
            this.settingsGroupBox.Location = new System.Drawing.Point(12, 90);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(302, 152);
            this.settingsGroupBox.TabIndex = 25;
            this.settingsGroupBox.TabStop = false;
            this.settingsGroupBox.Text = "Modpack Settings";
            // 
            // darkUICB
            // 
            this.darkUICB.AutoSize = true;
            this.darkUICB.Location = new System.Drawing.Point(6, 131);
            this.darkUICB.Name = "darkUICB";
            this.darkUICB.Size = new System.Drawing.Size(63, 17);
            this.darkUICB.TabIndex = 30;
            this.darkUICB.Text = "Dark UI";
            this.darkUICB.UseVisualStyleBackColor = true;
            this.darkUICB.CheckedChanged += new System.EventHandler(this.darkUICB_CheckedChanged);
            this.darkUICB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.darkUICB_MouseDown);
            this.darkUICB.MouseEnter += new System.EventHandler(this.darkUICB_MouseEnter);
            this.darkUICB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // cleanUninstallCB
            // 
            this.cleanUninstallCB.AutoSize = true;
            this.cleanUninstallCB.Location = new System.Drawing.Point(6, 114);
            this.cleanUninstallCB.Name = "cleanUninstallCB";
            this.cleanUninstallCB.Size = new System.Drawing.Size(117, 17);
            this.cleanUninstallCB.TabIndex = 29;
            this.cleanUninstallCB.Text = "Clean uninstallation";
            this.cleanUninstallCB.UseVisualStyleBackColor = true;
            this.cleanUninstallCB.CheckedChanged += new System.EventHandler(this.cleanUninstallCB_CheckedChanged);
            this.cleanUninstallCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cleanUninstallCB_MouseDown);
            this.cleanUninstallCB.MouseEnter += new System.EventHandler(this.cleanUninstallCB_MouseEnter);
            this.cleanUninstallCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // saveUserDataCB
            // 
            this.saveUserDataCB.AutoSize = true;
            this.saveUserDataCB.Location = new System.Drawing.Point(6, 97);
            this.saveUserDataCB.Name = "saveUserDataCB";
            this.saveUserDataCB.Size = new System.Drawing.Size(139, 17);
            this.saveUserDataCB.TabIndex = 27;
            this.saveUserDataCB.Text = "Save User created data";
            this.saveUserDataCB.UseVisualStyleBackColor = true;
            this.saveUserDataCB.CheckedChanged += new System.EventHandler(this.saveUserDataCB_CheckedChanged);
            this.saveUserDataCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.saveUserDataCB_MouseDown);
            this.saveUserDataCB.MouseEnter += new System.EventHandler(this.saveUserDataCB_MouseEnter);
            this.saveUserDataCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // saveLastInstallCB
            // 
            this.saveLastInstallCB.AutoSize = true;
            this.saveLastInstallCB.Location = new System.Drawing.Point(6, 81);
            this.saveLastInstallCB.Name = "saveLastInstallCB";
            this.saveLastInstallCB.Size = new System.Drawing.Size(138, 17);
            this.saveLastInstallCB.TabIndex = 26;
            this.saveLastInstallCB.Text = "Save last install\'s config";
            this.saveLastInstallCB.UseVisualStyleBackColor = true;
            this.saveLastInstallCB.CheckedChanged += new System.EventHandler(this.saveLastInstallCB_CheckedChanged);
            this.saveLastInstallCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.saveLastInstallCB_MouseDown);
            this.saveLastInstallCB.MouseEnter += new System.EventHandler(this.saveLastInstallCB_MouseEnter);
            this.saveLastInstallCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // languageSelectionGB
            // 
            this.languageSelectionGB.Controls.Add(this.languagePL);
            this.languageSelectionGB.Controls.Add(this.languageGER);
            this.languageSelectionGB.Controls.Add(this.languageENG);
            this.languageSelectionGB.Location = new System.Drawing.Point(154, 366);
            this.languageSelectionGB.Name = "languageSelectionGB";
            this.languageSelectionGB.Size = new System.Drawing.Size(160, 37);
            this.languageSelectionGB.TabIndex = 30;
            this.languageSelectionGB.TabStop = false;
            this.languageSelectionGB.Text = "Language";
            // 
            // languagePL
            // 
            this.languagePL.AutoSize = true;
            this.languagePL.Location = new System.Drawing.Point(61, 13);
            this.languagePL.Name = "languagePL";
            this.languagePL.Size = new System.Drawing.Size(46, 17);
            this.languagePL.TabIndex = 2;
            this.languagePL.TabStop = true;
            this.languagePL.Text = "POL";
            this.languagePL.UseVisualStyleBackColor = true;
            this.languagePL.CheckedChanged += new System.EventHandler(this.languagePL_CheckedChanged);
            this.languagePL.MouseDown += new System.Windows.Forms.MouseEventHandler(this.language_MouseDown);
            this.languagePL.MouseEnter += new System.EventHandler(this.language_MouseEnter);
            this.languagePL.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // languageGER
            // 
            this.languageGER.AutoSize = true;
            this.languageGER.Location = new System.Drawing.Point(109, 13);
            this.languageGER.Name = "languageGER";
            this.languageGER.Size = new System.Drawing.Size(48, 17);
            this.languageGER.TabIndex = 1;
            this.languageGER.Text = "GER";
            this.languageGER.UseVisualStyleBackColor = true;
            this.languageGER.CheckedChanged += new System.EventHandler(this.languageGER_CheckedChanged);
            this.languageGER.MouseDown += new System.Windows.Forms.MouseEventHandler(this.language_MouseDown);
            this.languageGER.MouseEnter += new System.EventHandler(this.language_MouseEnter);
            this.languageGER.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // languageENG
            // 
            this.languageENG.AutoSize = true;
            this.languageENG.Location = new System.Drawing.Point(6, 13);
            this.languageENG.Name = "languageENG";
            this.languageENG.Size = new System.Drawing.Size(48, 17);
            this.languageENG.TabIndex = 0;
            this.languageENG.Text = "ENG";
            this.languageENG.UseVisualStyleBackColor = true;
            this.languageENG.CheckedChanged += new System.EventHandler(this.languageENG_CheckedChanged);
            this.languageENG.MouseDown += new System.Windows.Forms.MouseEventHandler(this.language_MouseDown);
            this.languageENG.MouseEnter += new System.EventHandler(this.language_MouseEnter);
            this.languageENG.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // loadingImageGroupBox
            // 
            this.loadingImageGroupBox.Controls.Add(this.thirdGuardsLoadingImageRB);
            this.loadingImageGroupBox.Controls.Add(this.standardImageRB);
            this.loadingImageGroupBox.Location = new System.Drawing.Point(11, 344);
            this.loadingImageGroupBox.Name = "loadingImageGroupBox";
            this.loadingImageGroupBox.Size = new System.Drawing.Size(137, 49);
            this.loadingImageGroupBox.TabIndex = 26;
            this.loadingImageGroupBox.TabStop = false;
            this.loadingImageGroupBox.Text = "Loading Image";
            // 
            // thirdGuardsLoadingImageRB
            // 
            this.thirdGuardsLoadingImageRB.AutoSize = true;
            this.thirdGuardsLoadingImageRB.Location = new System.Drawing.Point(6, 29);
            this.thirdGuardsLoadingImageRB.Name = "thirdGuardsLoadingImageRB";
            this.thirdGuardsLoadingImageRB.Size = new System.Drawing.Size(72, 17);
            this.thirdGuardsLoadingImageRB.TabIndex = 1;
            this.thirdGuardsLoadingImageRB.TabStop = true;
            this.thirdGuardsLoadingImageRB.Text = "3rdguards";
            this.thirdGuardsLoadingImageRB.UseVisualStyleBackColor = true;
            this.thirdGuardsLoadingImageRB.CheckedChanged += new System.EventHandler(this.standardImageRB_CheckedChanged);
            this.thirdGuardsLoadingImageRB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.standardImageRB_MouseDown);
            this.thirdGuardsLoadingImageRB.MouseEnter += new System.EventHandler(this.standardImageRB_MouseEnter);
            this.thirdGuardsLoadingImageRB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // standardImageRB
            // 
            this.standardImageRB.AutoSize = true;
            this.standardImageRB.Location = new System.Drawing.Point(6, 13);
            this.standardImageRB.Name = "standardImageRB";
            this.standardImageRB.Size = new System.Drawing.Size(68, 17);
            this.standardImageRB.TabIndex = 0;
            this.standardImageRB.TabStop = true;
            this.standardImageRB.Text = "Standard";
            this.standardImageRB.UseVisualStyleBackColor = true;
            this.standardImageRB.CheckedChanged += new System.EventHandler(this.standardImageRB_CheckedChanged);
            this.standardImageRB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.standardImageRB_MouseDown);
            this.standardImageRB.MouseEnter += new System.EventHandler(this.standardImageRB_MouseEnter);
            this.standardImageRB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // findBugAddModLabel
            // 
            this.findBugAddModLabel.AutoSize = true;
            this.findBugAddModLabel.Location = new System.Drawing.Point(9, 559);
            this.findBugAddModLabel.Name = "findBugAddModLabel";
            this.findBugAddModLabel.Size = new System.Drawing.Size(163, 13);
            this.findBugAddModLabel.TabIndex = 27;
            this.findBugAddModLabel.TabStop = true;
            this.findBugAddModLabel.Text = "Find a bug? Want a mod added?";
            this.findBugAddModLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.findBugAddModLabel_LinkClicked);
            // 
            // cancelDownloadButton
            // 
            this.cancelDownloadButton.Enabled = false;
            this.cancelDownloadButton.Location = new System.Drawing.Point(221, 559);
            this.cancelDownloadButton.Name = "cancelDownloadButton";
            this.cancelDownloadButton.Size = new System.Drawing.Size(93, 60);
            this.cancelDownloadButton.TabIndex = 28;
            this.cancelDownloadButton.Text = "Cancel Download";
            this.cancelDownloadButton.UseVisualStyleBackColor = true;
            this.cancelDownloadButton.Visible = false;
            this.cancelDownloadButton.Click += new System.EventHandler(this.cancelDownloadButton_Click);
            // 
            // downloadTimer
            // 
            this.downloadTimer.Interval = 1000;
            this.downloadTimer.Tick += new System.EventHandler(this.downloadTimer_Tick);
            // 
            // downloadProgress
            // 
            this.downloadProgress.Location = new System.Drawing.Point(12, 412);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.ReadOnly = true;
            this.downloadProgress.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.downloadProgress.Size = new System.Drawing.Size(302, 82);
            this.downloadProgress.TabIndex = 29;
            this.downloadProgress.Text = "";
            // 
            // viewTypeGB
            // 
            this.viewTypeGB.Controls.Add(this.disableBordersCB);
            this.viewTypeGB.Controls.Add(this.expandNodesDefault);
            this.viewTypeGB.Controls.Add(this.selectionLegacy);
            this.viewTypeGB.Controls.Add(this.selectionDefault);
            this.viewTypeGB.Location = new System.Drawing.Point(11, 248);
            this.viewTypeGB.Name = "viewTypeGB";
            this.viewTypeGB.Size = new System.Drawing.Size(173, 93);
            this.viewTypeGB.TabIndex = 31;
            this.viewTypeGB.TabStop = false;
            this.viewTypeGB.Text = "Selection View";
            // 
            // disableBordersCB
            // 
            this.disableBordersCB.AutoSize = true;
            this.disableBordersCB.Location = new System.Drawing.Point(18, 30);
            this.disableBordersCB.Name = "disableBordersCB";
            this.disableBordersCB.Size = new System.Drawing.Size(99, 17);
            this.disableBordersCB.TabIndex = 3;
            this.disableBordersCB.Text = "Disable borders";
            this.disableBordersCB.UseVisualStyleBackColor = true;
            this.disableBordersCB.CheckedChanged += new System.EventHandler(this.disableBordersCB_CheckedChanged);
            this.disableBordersCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.disableBordersCB_MouseDown);
            this.disableBordersCB.MouseEnter += new System.EventHandler(this.disableBordersCB_MouseEnter);
            this.disableBordersCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // expandNodesDefault
            // 
            this.expandNodesDefault.AutoSize = true;
            this.expandNodesDefault.Location = new System.Drawing.Point(18, 68);
            this.expandNodesDefault.Name = "expandNodesDefault";
            this.expandNodesDefault.Size = new System.Drawing.Size(75, 17);
            this.expandNodesDefault.TabIndex = 2;
            this.expandNodesDefault.Text = "Expand all";
            this.expandNodesDefault.UseVisualStyleBackColor = true;
            this.expandNodesDefault.CheckedChanged += new System.EventHandler(this.expandNodesDefault_CheckedChanged);
            this.expandNodesDefault.MouseDown += new System.Windows.Forms.MouseEventHandler(this.expandNodesDefault_MouseDown);
            this.expandNodesDefault.MouseEnter += new System.EventHandler(this.expandNodesDefault_MouseEnter);
            this.expandNodesDefault.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // selectionLegacy
            // 
            this.selectionLegacy.AutoSize = true;
            this.selectionLegacy.Location = new System.Drawing.Point(6, 53);
            this.selectionLegacy.Name = "selectionLegacy";
            this.selectionLegacy.Size = new System.Drawing.Size(60, 17);
            this.selectionLegacy.TabIndex = 1;
            this.selectionLegacy.TabStop = true;
            this.selectionLegacy.Text = "Legacy";
            this.selectionLegacy.UseVisualStyleBackColor = true;
            this.selectionLegacy.CheckedChanged += new System.EventHandler(this.selectionLegacy_CheckedChanged);
            this.selectionLegacy.MouseDown += new System.Windows.Forms.MouseEventHandler(this.selectionLegacy_MouseDown);
            this.selectionLegacy.MouseEnter += new System.EventHandler(this.selectionView_MouseEnter);
            this.selectionLegacy.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // selectionDefault
            // 
            this.selectionDefault.AutoSize = true;
            this.selectionDefault.Location = new System.Drawing.Point(6, 13);
            this.selectionDefault.Name = "selectionDefault";
            this.selectionDefault.Size = new System.Drawing.Size(59, 17);
            this.selectionDefault.TabIndex = 0;
            this.selectionDefault.TabStop = true;
            this.selectionDefault.Text = "Default";
            this.selectionDefault.UseVisualStyleBackColor = true;
            this.selectionDefault.CheckedChanged += new System.EventHandler(this.selectionDefault_CheckedChanged);
            this.selectionDefault.MouseDown += new System.Windows.Forms.MouseEventHandler(this.selectionDefault_MouseDown);
            this.selectionDefault.MouseEnter += new System.EventHandler(this.selectionView_MouseEnter);
            this.selectionDefault.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // donateLabel
            // 
            this.donateLabel.AutoSize = true;
            this.donateLabel.Location = new System.Drawing.Point(9, 598);
            this.donateLabel.Name = "donateLabel";
            this.donateLabel.Size = new System.Drawing.Size(48, 13);
            this.donateLabel.TabIndex = 32;
            this.donateLabel.TabStop = true;
            this.donateLabel.Text = "Donate?";
            this.donateLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.donateLabel_LinkClicked);
            // 
            // fontSizeGB
            // 
            this.fontSizeGB.Controls.Add(this.DPILarge);
            this.fontSizeGB.Controls.Add(this.DPIUHD);
            this.fontSizeGB.Controls.Add(this.DPIDefault);
            this.fontSizeGB.Controls.Add(this.fontSizeHUD);
            this.fontSizeGB.Controls.Add(this.fontSizeLarge);
            this.fontSizeGB.Controls.Add(this.fontSizeDefault);
            this.fontSizeGB.Location = new System.Drawing.Point(193, 248);
            this.fontSizeGB.Name = "fontSizeGB";
            this.fontSizeGB.Size = new System.Drawing.Size(121, 115);
            this.fontSizeGB.TabIndex = 33;
            this.fontSizeGB.TabStop = false;
            this.fontSizeGB.Text = "Scaling Mode";
            // 
            // DPILarge
            // 
            this.DPILarge.AutoSize = true;
            this.DPILarge.Location = new System.Drawing.Point(6, 77);
            this.DPILarge.Name = "DPILarge";
            this.DPILarge.Size = new System.Drawing.Size(72, 17);
            this.DPILarge.TabIndex = 5;
            this.DPILarge.TabStop = true;
            this.DPILarge.Text = "DPI 1.25x";
            this.DPILarge.UseVisualStyleBackColor = true;
            this.DPILarge.CheckedChanged += new System.EventHandler(this.DPILarge_CheckedChanged);
            // 
            // DPIUHD
            // 
            this.DPIUHD.AutoSize = true;
            this.DPIUHD.Location = new System.Drawing.Point(6, 93);
            this.DPIUHD.Name = "DPIUHD";
            this.DPIUHD.Size = new System.Drawing.Size(72, 17);
            this.DPIUHD.TabIndex = 4;
            this.DPIUHD.TabStop = true;
            this.DPIUHD.Text = "DPI 1.75x";
            this.DPIUHD.UseVisualStyleBackColor = true;
            this.DPIUHD.CheckedChanged += new System.EventHandler(this.DPIUHD_CheckedChanged);
            // 
            // DPIDefault
            // 
            this.DPIDefault.AutoSize = true;
            this.DPIDefault.Location = new System.Drawing.Point(6, 61);
            this.DPIDefault.Name = "DPIDefault";
            this.DPIDefault.Size = new System.Drawing.Size(57, 17);
            this.DPIDefault.TabIndex = 3;
            this.DPIDefault.TabStop = true;
            this.DPIDefault.Text = "DPI 1x";
            this.DPIDefault.UseVisualStyleBackColor = true;
            this.DPIDefault.CheckedChanged += new System.EventHandler(this.DPI_CheckedChanged);
            this.DPIDefault.MouseDown += new System.Windows.Forms.MouseEventHandler(this.font_MouseDown);
            this.DPIDefault.MouseEnter += new System.EventHandler(this.font_MouseEnter);
            this.DPIDefault.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // fontSizeHUD
            // 
            this.fontSizeHUD.AutoSize = true;
            this.fontSizeHUD.Location = new System.Drawing.Point(6, 45);
            this.fontSizeHUD.Name = "fontSizeHUD";
            this.fontSizeHUD.Size = new System.Drawing.Size(75, 17);
            this.fontSizeHUD.TabIndex = 2;
            this.fontSizeHUD.TabStop = true;
            this.fontSizeHUD.Text = "Font 1.75x";
            this.fontSizeHUD.UseVisualStyleBackColor = true;
            this.fontSizeHUD.CheckedChanged += new System.EventHandler(this.fontSizeHUD_CheckedChanged);
            this.fontSizeHUD.MouseDown += new System.Windows.Forms.MouseEventHandler(this.font_MouseDown);
            this.fontSizeHUD.MouseEnter += new System.EventHandler(this.font_MouseEnter);
            this.fontSizeHUD.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // fontSizeLarge
            // 
            this.fontSizeLarge.AutoSize = true;
            this.fontSizeLarge.Location = new System.Drawing.Point(6, 29);
            this.fontSizeLarge.Name = "fontSizeLarge";
            this.fontSizeLarge.Size = new System.Drawing.Size(75, 17);
            this.fontSizeLarge.TabIndex = 1;
            this.fontSizeLarge.TabStop = true;
            this.fontSizeLarge.Text = "Font 1.25x";
            this.fontSizeLarge.UseVisualStyleBackColor = true;
            this.fontSizeLarge.CheckedChanged += new System.EventHandler(this.fontSizeLarge_CheckedChanged);
            this.fontSizeLarge.MouseDown += new System.Windows.Forms.MouseEventHandler(this.font_MouseDown);
            this.fontSizeLarge.MouseEnter += new System.EventHandler(this.font_MouseEnter);
            this.fontSizeLarge.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // fontSizeDefault
            // 
            this.fontSizeDefault.AutoSize = true;
            this.fontSizeDefault.Location = new System.Drawing.Point(6, 13);
            this.fontSizeDefault.Name = "fontSizeDefault";
            this.fontSizeDefault.Size = new System.Drawing.Size(60, 17);
            this.fontSizeDefault.TabIndex = 0;
            this.fontSizeDefault.TabStop = true;
            this.fontSizeDefault.Text = "Font 1x";
            this.fontSizeDefault.UseVisualStyleBackColor = true;
            this.fontSizeDefault.CheckedChanged += new System.EventHandler(this.fontSizeDefault_CheckedChanged);
            this.fontSizeDefault.MouseDown += new System.Windows.Forms.MouseEventHandler(this.font_MouseDown);
            this.fontSizeDefault.MouseEnter += new System.EventHandler(this.font_MouseEnter);
            this.fontSizeDefault.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // totalProgressBar
            // 
            this.totalProgressBar.Location = new System.Drawing.Point(12, 496);
            this.totalProgressBar.Name = "totalProgressBar";
            this.totalProgressBar.Size = new System.Drawing.Size(302, 15);
            this.totalProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.totalProgressBar.TabIndex = 34;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(326, 624);
            this.Controls.Add(this.totalProgressBar);
            this.Controls.Add(this.fontSizeGB);
            this.Controls.Add(this.donateLabel);
            this.Controls.Add(this.viewTypeGB);
            this.Controls.Add(this.languageSelectionGB);
            this.Controls.Add(this.downloadProgress);
            this.Controls.Add(this.cancelDownloadButton);
            this.Controls.Add(this.findBugAddModLabel);
            this.Controls.Add(this.loadingImageGroupBox);
            this.Controls.Add(this.settingsGroupBox);
            this.Controls.Add(this.uninstallRelhaxMod);
            this.Controls.Add(this.installRelhaxMod);
            this.Controls.Add(this.parrentProgressBar);
            this.Controls.Add(this.formPageLink);
            this.Controls.Add(this.childProgressBar);
            this.Controls.Add(this.statusLabel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Relhax ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.settingsGroupBox.ResumeLayout(false);
            this.settingsGroupBox.PerformLayout();
            this.languageSelectionGB.ResumeLayout(false);
            this.languageSelectionGB.PerformLayout();
            this.loadingImageGroupBox.ResumeLayout(false);
            this.loadingImageGroupBox.PerformLayout();
            this.viewTypeGB.ResumeLayout(false);
            this.viewTypeGB.PerformLayout();
            this.fontSizeGB.ResumeLayout(false);
            this.fontSizeGB.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ProgressBar childProgressBar;
        private System.Windows.Forms.OpenFileDialog findWotExe;
        private System.Windows.Forms.CheckBox forceManuel;
        private System.Windows.Forms.LinkLabel formPageLink;
        private System.Windows.Forms.ProgressBar parrentProgressBar;
        private System.Windows.Forms.Button installRelhaxMod;
        private System.Windows.Forms.Button uninstallRelhaxMod;
        private System.Windows.Forms.CheckBox cleanInstallCB;
        private System.Windows.Forms.CheckBox cancerFontCB;
        private System.Windows.Forms.CheckBox backupModsCheckBox;
        private System.Windows.Forms.GroupBox settingsGroupBox;
        private System.Windows.Forms.GroupBox loadingImageGroupBox;
        private System.Windows.Forms.RadioButton thirdGuardsLoadingImageRB;
        private System.Windows.Forms.RadioButton standardImageRB;
        private System.Windows.Forms.LinkLabel findBugAddModLabel;
        private System.Windows.Forms.CheckBox saveLastInstallCB;
        private System.Windows.Forms.Button cancelDownloadButton;
        private System.Windows.Forms.CheckBox saveUserDataCB;
        private System.Windows.Forms.Timer downloadTimer;
        private System.Windows.Forms.CheckBox cleanUninstallCB;
        private System.Windows.Forms.RichTextBox downloadProgress;
        private System.Windows.Forms.CheckBox darkUICB;
        private System.Windows.Forms.GroupBox languageSelectionGB;
        private System.Windows.Forms.RadioButton languageGER;
        private System.Windows.Forms.RadioButton languageENG;
        private System.Windows.Forms.GroupBox viewTypeGB;
        private System.Windows.Forms.RadioButton selectionLegacy;
        private System.Windows.Forms.RadioButton selectionDefault;
        private System.Windows.Forms.RadioButton languagePL;
        private System.Windows.Forms.LinkLabel donateLabel;
        private System.Windows.Forms.CheckBox expandNodesDefault;
        private System.Windows.Forms.GroupBox fontSizeGB;
        private System.Windows.Forms.RadioButton fontSizeHUD;
        private System.Windows.Forms.RadioButton fontSizeLarge;
        private System.Windows.Forms.RadioButton fontSizeDefault;
        private System.Windows.Forms.RadioButton DPIDefault;
        private System.Windows.Forms.CheckBox disableBordersCB;
        private System.Windows.Forms.RadioButton DPILarge;
        private System.Windows.Forms.RadioButton DPIUHD;
        private System.Windows.Forms.ProgressBar totalProgressBar;
    }
}

