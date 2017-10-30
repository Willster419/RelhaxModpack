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
            this.ShowInstallCompleteWindowCB = new System.Windows.Forms.CheckBox();
            this.notifyIfSameDatabaseCB = new System.Windows.Forms.CheckBox();
            this.clearLogFilesCB = new System.Windows.Forms.CheckBox();
            this.clearCacheCB = new System.Windows.Forms.CheckBox();
            this.darkUICB = new System.Windows.Forms.CheckBox();
            this.saveUserDataCB = new System.Windows.Forms.CheckBox();
            this.saveLastInstallCB = new System.Windows.Forms.CheckBox();
            this.languageSelectionGB = new System.Windows.Forms.GroupBox();
            this.languageFR = new System.Windows.Forms.RadioButton();
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
            this.disableColorsCB = new System.Windows.Forms.CheckBox();
            this.disableBordersCB = new System.Windows.Forms.CheckBox();
            this.expandNodesDefault = new System.Windows.Forms.CheckBox();
            this.selectionLegacy = new System.Windows.Forms.RadioButton();
            this.selectionDefault = new System.Windows.Forms.RadioButton();
            this.donateLabel = new System.Windows.Forms.LinkLabel();
            this.fontSizeGB = new System.Windows.Forms.GroupBox();
            this.DPI275 = new System.Windows.Forms.RadioButton();
            this.DPI225 = new System.Windows.Forms.RadioButton();
            this.fontSize275 = new System.Windows.Forms.RadioButton();
            this.fontSize225 = new System.Windows.Forms.RadioButton();
            this.DPIAUTO = new System.Windows.Forms.RadioButton();
            this.DPI125 = new System.Windows.Forms.RadioButton();
            this.DPI175 = new System.Windows.Forms.RadioButton();
            this.DPI100 = new System.Windows.Forms.RadioButton();
            this.fontSize175 = new System.Windows.Forms.RadioButton();
            this.fontSize125 = new System.Windows.Forms.RadioButton();
            this.fontSize100 = new System.Windows.Forms.RadioButton();
            this.totalProgressBar = new System.Windows.Forms.ProgressBar();
            this.DiscordServerLink = new System.Windows.Forms.LinkLabel();
            this.viewAppUpdates = new System.Windows.Forms.Button();
            this.viewDBUpdates = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ErrorCounterLabel = new System.Windows.Forms.Label();
            this.createShortcutsCB = new System.Windows.Forms.CheckBox();
            this.settingsGroupBox.SuspendLayout();
            this.languageSelectionGB.SuspendLayout();
            this.loadingImageGroupBox.SuspendLayout();
            this.viewTypeGB.SuspendLayout();
            this.fontSizeGB.SuspendLayout();
            this.SuspendLayout();
            // 
            // childProgressBar
            // 
            this.childProgressBar.Location = new System.Drawing.Point(13, 534);
            this.childProgressBar.Name = "childProgressBar";
            this.childProgressBar.Size = new System.Drawing.Size(418, 16);
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
            this.toolTip.SetToolTip(this.forceManuel, "Right Click for extended description");
            this.forceManuel.UseVisualStyleBackColor = true;
            this.forceManuel.CheckedChanged += new System.EventHandler(this.forceManuel_CheckedChanged);
            this.forceManuel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.forceManuel_MouseDown);
            this.forceManuel.MouseEnter += new System.EventHandler(this.forceManuel_MouseEnter);
            this.forceManuel.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // formPageLink
            // 
            this.formPageLink.AutoSize = true;
            this.formPageLink.Location = new System.Drawing.Point(10, 575);
            this.formPageLink.Name = "formPageLink";
            this.formPageLink.Size = new System.Drawing.Size(132, 13);
            this.formPageLink.TabIndex = 16;
            this.formPageLink.TabStop = true;
            this.formPageLink.Text = "View Modpack Form Page";
            this.formPageLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.formPageLink_LinkClicked);
            // 
            // parrentProgressBar
            // 
            this.parrentProgressBar.Location = new System.Drawing.Point(13, 512);
            this.parrentProgressBar.Name = "parrentProgressBar";
            this.parrentProgressBar.Size = new System.Drawing.Size(418, 16);
            this.parrentProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.parrentProgressBar.TabIndex = 17;
            // 
            // installRelhaxMod
            // 
            this.installRelhaxMod.Location = new System.Drawing.Point(224, 12);
            this.installRelhaxMod.Name = "installRelhaxMod";
            this.installRelhaxMod.Size = new System.Drawing.Size(205, 30);
            this.installRelhaxMod.TabIndex = 19;
            this.installRelhaxMod.Text = "Install Relhax Modpack";
            this.installRelhaxMod.UseVisualStyleBackColor = true;
            this.installRelhaxMod.Click += new System.EventHandler(this.installRelhaxMod_Click);
            // 
            // uninstallRelhaxMod
            // 
            this.uninstallRelhaxMod.Location = new System.Drawing.Point(11, 12);
            this.uninstallRelhaxMod.Name = "uninstallRelhaxMod";
            this.uninstallRelhaxMod.Size = new System.Drawing.Size(205, 30);
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
            this.toolTip.SetToolTip(this.cleanInstallCB, "Right Click for extended description");
            this.cleanInstallCB.UseVisualStyleBackColor = true;
            this.cleanInstallCB.CheckedChanged += new System.EventHandler(this.cleanInstallCB_CheckedChanged);
            this.cleanInstallCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cleanInstallCB_MouseDown);
            this.cleanInstallCB.MouseEnter += new System.EventHandler(this.cleanInstallCB_MouseEnter);
            this.cleanInstallCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // cancerFontCB
            // 
            this.cancerFontCB.AutoSize = true;
            this.cancerFontCB.Location = new System.Drawing.Point(6, 65);
            this.cancerFontCB.Name = "cancerFontCB";
            this.cancerFontCB.Size = new System.Drawing.Size(81, 17);
            this.cancerFontCB.TabIndex = 23;
            this.cancerFontCB.Text = "Cancer font";
            this.toolTip.SetToolTip(this.cancerFontCB, "Right Click for extended description");
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
            this.toolTip.SetToolTip(this.backupModsCheckBox, "Right Click for extended description");
            this.backupModsCheckBox.UseVisualStyleBackColor = true;
            this.backupModsCheckBox.CheckedChanged += new System.EventHandler(this.backupModsCheckBox_CheckedChanged);
            this.backupModsCheckBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.backupModsCheckBox_MouseDown);
            this.backupModsCheckBox.MouseEnter += new System.EventHandler(this.backupModsCheckBox_MouseEnter);
            this.backupModsCheckBox.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Controls.Add(this.createShortcutsCB);
            this.settingsGroupBox.Controls.Add(this.ShowInstallCompleteWindowCB);
            this.settingsGroupBox.Controls.Add(this.notifyIfSameDatabaseCB);
            this.settingsGroupBox.Controls.Add(this.clearLogFilesCB);
            this.settingsGroupBox.Controls.Add(this.clearCacheCB);
            this.settingsGroupBox.Controls.Add(this.darkUICB);
            this.settingsGroupBox.Controls.Add(this.saveUserDataCB);
            this.settingsGroupBox.Controls.Add(this.saveLastInstallCB);
            this.settingsGroupBox.Controls.Add(this.forceManuel);
            this.settingsGroupBox.Controls.Add(this.cancerFontCB);
            this.settingsGroupBox.Controls.Add(this.backupModsCheckBox);
            this.settingsGroupBox.Controls.Add(this.cleanInstallCB);
            this.settingsGroupBox.Location = new System.Drawing.Point(12, 77);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(308, 221);
            this.settingsGroupBox.TabIndex = 25;
            this.settingsGroupBox.TabStop = false;
            this.settingsGroupBox.Text = "Modpack Settings (Right click for description)";
            // 
            // ShowInstallCompleteWindowCB
            // 
            this.ShowInstallCompleteWindowCB.AutoSize = true;
            this.ShowInstallCompleteWindowCB.Location = new System.Drawing.Point(6, 184);
            this.ShowInstallCompleteWindowCB.Name = "ShowInstallCompleteWindowCB";
            this.ShowInstallCompleteWindowCB.Size = new System.Drawing.Size(168, 17);
            this.ShowInstallCompleteWindowCB.TabIndex = 34;
            this.ShowInstallCompleteWindowCB.Text = "Show Install complete window";
            this.toolTip.SetToolTip(this.ShowInstallCompleteWindowCB, "Right Click for extended description");
            this.ShowInstallCompleteWindowCB.UseVisualStyleBackColor = true;
            this.ShowInstallCompleteWindowCB.CheckedChanged += new System.EventHandler(this.ShowInstallCompleteWindow_CheckedChanged);
            this.ShowInstallCompleteWindowCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ShowInstallCompleteWindowCB_MouseDown);
            this.ShowInstallCompleteWindowCB.MouseEnter += new System.EventHandler(this.ShowInstallCompleteWindowCB_MouseEnter);
            this.ShowInstallCompleteWindowCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // notifyIfSameDatabaseCB
            // 
            this.notifyIfSameDatabaseCB.AutoSize = true;
            this.notifyIfSameDatabaseCB.Location = new System.Drawing.Point(6, 167);
            this.notifyIfSameDatabaseCB.Name = "notifyIfSameDatabaseCB";
            this.notifyIfSameDatabaseCB.Size = new System.Drawing.Size(193, 17);
            this.notifyIfSameDatabaseCB.TabIndex = 33;
            this.notifyIfSameDatabaseCB.Text = "Inform if no new database available";
            this.toolTip.SetToolTip(this.notifyIfSameDatabaseCB, "Right Click for extended description");
            this.notifyIfSameDatabaseCB.UseVisualStyleBackColor = true;
            this.notifyIfSameDatabaseCB.CheckedChanged += new System.EventHandler(this.notifyIfSameDatabaseCB_CheckedChanged);
            this.notifyIfSameDatabaseCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.notifyIfSameDatabaseCB_MouseDown);
            this.notifyIfSameDatabaseCB.MouseEnter += new System.EventHandler(this.notifyIfSameDatabaseCB_MouseEnter);
            this.notifyIfSameDatabaseCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // clearLogFilesCB
            // 
            this.clearLogFilesCB.AutoSize = true;
            this.clearLogFilesCB.Location = new System.Drawing.Point(6, 150);
            this.clearLogFilesCB.Name = "clearLogFilesCB";
            this.clearLogFilesCB.Size = new System.Drawing.Size(88, 17);
            this.clearLogFilesCB.TabIndex = 32;
            this.clearLogFilesCB.Text = "Clear log files";
            this.toolTip.SetToolTip(this.clearLogFilesCB, "Right Click for extended description");
            this.clearLogFilesCB.UseVisualStyleBackColor = true;
            this.clearLogFilesCB.CheckedChanged += new System.EventHandler(this.clearLogFilesCB_CheckedChanged);
            this.clearLogFilesCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.clearLogFilesCB_MouseDown);
            this.clearLogFilesCB.MouseEnter += new System.EventHandler(this.clearLogFilesCB_MouseEnter);
            this.clearLogFilesCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // clearCacheCB
            // 
            this.clearCacheCB.AutoSize = true;
            this.clearCacheCB.Location = new System.Drawing.Point(6, 133);
            this.clearCacheCB.Name = "clearCacheCB";
            this.clearCacheCB.Size = new System.Drawing.Size(137, 17);
            this.clearCacheCB.TabIndex = 31;
            this.clearCacheCB.Text = "Clear WoT Cache Data";
            this.toolTip.SetToolTip(this.clearCacheCB, "Right Click for extended description");
            this.clearCacheCB.UseVisualStyleBackColor = true;
            this.clearCacheCB.CheckedChanged += new System.EventHandler(this.clearCacheCB_CheckedChanged);
            this.clearCacheCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.clearCacheCB_MouseDown);
            this.clearCacheCB.MouseEnter += new System.EventHandler(this.clearCacheCB_MouseEnter);
            this.clearCacheCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // darkUICB
            // 
            this.darkUICB.AutoSize = true;
            this.darkUICB.Location = new System.Drawing.Point(6, 116);
            this.darkUICB.Name = "darkUICB";
            this.darkUICB.Size = new System.Drawing.Size(63, 17);
            this.darkUICB.TabIndex = 30;
            this.darkUICB.Text = "Dark UI";
            this.toolTip.SetToolTip(this.darkUICB, "Right Click for extended description");
            this.darkUICB.UseVisualStyleBackColor = true;
            this.darkUICB.CheckedChanged += new System.EventHandler(this.darkUICB_CheckedChanged);
            this.darkUICB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.darkUICB_MouseDown);
            this.darkUICB.MouseEnter += new System.EventHandler(this.darkUICB_MouseEnter);
            this.darkUICB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // saveUserDataCB
            // 
            this.saveUserDataCB.AutoSize = true;
            this.saveUserDataCB.Location = new System.Drawing.Point(6, 99);
            this.saveUserDataCB.Name = "saveUserDataCB";
            this.saveUserDataCB.Size = new System.Drawing.Size(139, 17);
            this.saveUserDataCB.TabIndex = 27;
            this.saveUserDataCB.Text = "Save User created data";
            this.toolTip.SetToolTip(this.saveUserDataCB, "Right Click for extended description");
            this.saveUserDataCB.UseVisualStyleBackColor = true;
            this.saveUserDataCB.CheckedChanged += new System.EventHandler(this.saveUserDataCB_CheckedChanged);
            this.saveUserDataCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.saveUserDataCB_MouseDown);
            this.saveUserDataCB.MouseEnter += new System.EventHandler(this.saveUserDataCB_MouseEnter);
            this.saveUserDataCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // saveLastInstallCB
            // 
            this.saveLastInstallCB.AutoSize = true;
            this.saveLastInstallCB.Location = new System.Drawing.Point(6, 82);
            this.saveLastInstallCB.Name = "saveLastInstallCB";
            this.saveLastInstallCB.Size = new System.Drawing.Size(138, 17);
            this.saveLastInstallCB.TabIndex = 26;
            this.saveLastInstallCB.Text = "Save last install\'s config";
            this.toolTip.SetToolTip(this.saveLastInstallCB, "Right Click for extended description");
            this.saveLastInstallCB.UseVisualStyleBackColor = true;
            this.saveLastInstallCB.CheckedChanged += new System.EventHandler(this.saveLastInstallCB_CheckedChanged);
            this.saveLastInstallCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.saveLastInstallCB_MouseDown);
            this.saveLastInstallCB.MouseEnter += new System.EventHandler(this.saveLastInstallCB_MouseEnter);
            this.saveLastInstallCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // languageSelectionGB
            // 
            this.languageSelectionGB.Controls.Add(this.languageFR);
            this.languageSelectionGB.Controls.Add(this.languagePL);
            this.languageSelectionGB.Controls.Add(this.languageGER);
            this.languageSelectionGB.Controls.Add(this.languageENG);
            this.languageSelectionGB.Location = new System.Drawing.Point(326, 78);
            this.languageSelectionGB.Name = "languageSelectionGB";
            this.languageSelectionGB.Size = new System.Drawing.Size(104, 125);
            this.languageSelectionGB.TabIndex = 30;
            this.languageSelectionGB.TabStop = false;
            this.languageSelectionGB.Text = "Language";
            // 
            // languageFR
            // 
            this.languageFR.AutoSize = true;
            this.languageFR.Location = new System.Drawing.Point(6, 60);
            this.languageFR.Name = "languageFR";
            this.languageFR.Size = new System.Drawing.Size(65, 17);
            this.languageFR.TabIndex = 3;
            this.languageFR.TabStop = true;
            this.languageFR.Text = "Français";
            this.languageFR.UseVisualStyleBackColor = true;
            this.languageFR.CheckedChanged += new System.EventHandler(this.languageFR_CheckedChanged);
            // 
            // languagePL
            // 
            this.languagePL.AutoSize = true;
            this.languagePL.Location = new System.Drawing.Point(6, 30);
            this.languagePL.Name = "languagePL";
            this.languagePL.Size = new System.Drawing.Size(53, 17);
            this.languagePL.TabIndex = 2;
            this.languagePL.TabStop = true;
            this.languagePL.Text = "Polski";
            this.languagePL.UseVisualStyleBackColor = true;
            this.languagePL.CheckedChanged += new System.EventHandler(this.languagePL_CheckedChanged);
            this.languagePL.MouseDown += new System.Windows.Forms.MouseEventHandler(this.language_MouseDown);
            this.languagePL.MouseEnter += new System.EventHandler(this.language_MouseEnter);
            this.languagePL.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // languageGER
            // 
            this.languageGER.AutoSize = true;
            this.languageGER.Location = new System.Drawing.Point(6, 45);
            this.languageGER.Name = "languageGER";
            this.languageGER.Size = new System.Drawing.Size(65, 17);
            this.languageGER.TabIndex = 1;
            this.languageGER.Text = "Deutsch";
            this.languageGER.UseVisualStyleBackColor = true;
            this.languageGER.CheckedChanged += new System.EventHandler(this.languageGER_CheckedChanged);
            this.languageGER.MouseDown += new System.Windows.Forms.MouseEventHandler(this.language_MouseDown);
            this.languageGER.MouseEnter += new System.EventHandler(this.language_MouseEnter);
            this.languageGER.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // languageENG
            // 
            this.languageENG.AutoSize = true;
            this.languageENG.Location = new System.Drawing.Point(6, 15);
            this.languageENG.Name = "languageENG";
            this.languageENG.Size = new System.Drawing.Size(59, 17);
            this.languageENG.TabIndex = 0;
            this.languageENG.Text = "English";
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
            this.loadingImageGroupBox.Location = new System.Drawing.Point(326, 209);
            this.loadingImageGroupBox.Name = "loadingImageGroupBox";
            this.loadingImageGroupBox.Size = new System.Drawing.Size(103, 89);
            this.loadingImageGroupBox.TabIndex = 26;
            this.loadingImageGroupBox.TabStop = false;
            this.loadingImageGroupBox.Text = "Loading Image";
            // 
            // thirdGuardsLoadingImageRB
            // 
            this.thirdGuardsLoadingImageRB.AutoSize = true;
            this.thirdGuardsLoadingImageRB.Location = new System.Drawing.Point(6, 30);
            this.thirdGuardsLoadingImageRB.Name = "thirdGuardsLoadingImageRB";
            this.thirdGuardsLoadingImageRB.Size = new System.Drawing.Size(72, 17);
            this.thirdGuardsLoadingImageRB.TabIndex = 1;
            this.thirdGuardsLoadingImageRB.TabStop = true;
            this.thirdGuardsLoadingImageRB.Text = "3rdguards";
            this.toolTip.SetToolTip(this.thirdGuardsLoadingImageRB, "Right Click for extended description");
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
            this.toolTip.SetToolTip(this.standardImageRB, "Right Click for extended description");
            this.standardImageRB.UseVisualStyleBackColor = true;
            this.standardImageRB.CheckedChanged += new System.EventHandler(this.standardImageRB_CheckedChanged);
            this.standardImageRB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.standardImageRB_MouseDown);
            this.standardImageRB.MouseEnter += new System.EventHandler(this.standardImageRB_MouseEnter);
            this.standardImageRB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // findBugAddModLabel
            // 
            this.findBugAddModLabel.AutoSize = true;
            this.findBugAddModLabel.Location = new System.Drawing.Point(10, 556);
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
            this.cancelDownloadButton.Location = new System.Drawing.Point(328, 556);
            this.cancelDownloadButton.Name = "cancelDownloadButton";
            this.cancelDownloadButton.Size = new System.Drawing.Size(103, 60);
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
            this.downloadProgress.DetectUrls = false;
            this.downloadProgress.Location = new System.Drawing.Point(13, 424);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.ReadOnly = true;
            this.downloadProgress.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.downloadProgress.Size = new System.Drawing.Size(418, 60);
            this.downloadProgress.TabIndex = 29;
            this.downloadProgress.Text = "";
            // 
            // viewTypeGB
            // 
            this.viewTypeGB.Controls.Add(this.disableColorsCB);
            this.viewTypeGB.Controls.Add(this.disableBordersCB);
            this.viewTypeGB.Controls.Add(this.expandNodesDefault);
            this.viewTypeGB.Controls.Add(this.selectionLegacy);
            this.viewTypeGB.Controls.Add(this.selectionDefault);
            this.viewTypeGB.Location = new System.Drawing.Point(178, 304);
            this.viewTypeGB.Name = "viewTypeGB";
            this.viewTypeGB.Size = new System.Drawing.Size(252, 114);
            this.viewTypeGB.TabIndex = 31;
            this.viewTypeGB.TabStop = false;
            this.viewTypeGB.Text = "Selection View";
            // 
            // disableColorsCB
            // 
            this.disableColorsCB.Location = new System.Drawing.Point(18, 61);
            this.disableColorsCB.Name = "disableColorsCB";
            this.disableColorsCB.Size = new System.Drawing.Size(116, 47);
            this.disableColorsCB.TabIndex = 4;
            this.disableColorsCB.Text = "Disable color change";
            this.toolTip.SetToolTip(this.disableColorsCB, "Right Click for extended description");
            this.disableColorsCB.UseVisualStyleBackColor = true;
            this.disableColorsCB.CheckedChanged += new System.EventHandler(this.disableColorsCB_CheckedChanged);
            this.disableColorsCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.disableColorsCB_MouseDown);
            this.disableColorsCB.MouseEnter += new System.EventHandler(this.disableColorsCB_MouseEnter);
            this.disableColorsCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // disableBordersCB
            // 
            this.disableBordersCB.Location = new System.Drawing.Point(18, 30);
            this.disableBordersCB.MaximumSize = new System.Drawing.Size(100, 40);
            this.disableBordersCB.MinimumSize = new System.Drawing.Size(50, 20);
            this.disableBordersCB.Name = "disableBordersCB";
            this.disableBordersCB.Size = new System.Drawing.Size(100, 32);
            this.disableBordersCB.TabIndex = 3;
            this.disableBordersCB.Text = "Disable borders";
            this.toolTip.SetToolTip(this.disableBordersCB, "Right Click for extended description");
            this.disableBordersCB.UseVisualStyleBackColor = true;
            this.disableBordersCB.CheckedChanged += new System.EventHandler(this.disableBordersCB_CheckedChanged);
            this.disableBordersCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.disableBordersCB_MouseDown);
            this.disableBordersCB.MouseEnter += new System.EventHandler(this.disableBordersCB_MouseEnter);
            this.disableBordersCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // expandNodesDefault
            // 
            this.expandNodesDefault.AutoSize = true;
            this.expandNodesDefault.Location = new System.Drawing.Point(133, 35);
            this.expandNodesDefault.MaximumSize = new System.Drawing.Size(100, 40);
            this.expandNodesDefault.MinimumSize = new System.Drawing.Size(50, 20);
            this.expandNodesDefault.Name = "expandNodesDefault";
            this.expandNodesDefault.Size = new System.Drawing.Size(75, 20);
            this.expandNodesDefault.TabIndex = 2;
            this.expandNodesDefault.Text = "Expand all";
            this.toolTip.SetToolTip(this.expandNodesDefault, "Right Click for extended description");
            this.expandNodesDefault.UseVisualStyleBackColor = true;
            this.expandNodesDefault.CheckedChanged += new System.EventHandler(this.expandNodesDefault_CheckedChanged);
            this.expandNodesDefault.MouseDown += new System.Windows.Forms.MouseEventHandler(this.expandNodesDefault_MouseDown);
            this.expandNodesDefault.MouseEnter += new System.EventHandler(this.expandNodesDefault_MouseEnter);
            this.expandNodesDefault.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // selectionLegacy
            // 
            this.selectionLegacy.AutoSize = true;
            this.selectionLegacy.Location = new System.Drawing.Point(124, 13);
            this.selectionLegacy.Name = "selectionLegacy";
            this.selectionLegacy.Size = new System.Drawing.Size(60, 17);
            this.selectionLegacy.TabIndex = 1;
            this.selectionLegacy.TabStop = true;
            this.selectionLegacy.Text = "Legacy";
            this.toolTip.SetToolTip(this.selectionLegacy, "Right Click for extended description");
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
            this.toolTip.SetToolTip(this.selectionDefault, "Right Click for extended description");
            this.selectionDefault.UseVisualStyleBackColor = true;
            this.selectionDefault.CheckedChanged += new System.EventHandler(this.selectionDefault_CheckedChanged);
            this.selectionDefault.MouseDown += new System.Windows.Forms.MouseEventHandler(this.selectionDefault_MouseDown);
            this.selectionDefault.MouseEnter += new System.EventHandler(this.selectionView_MouseEnter);
            this.selectionDefault.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // donateLabel
            // 
            this.donateLabel.AutoSize = true;
            this.donateLabel.Location = new System.Drawing.Point(10, 595);
            this.donateLabel.Name = "donateLabel";
            this.donateLabel.Size = new System.Drawing.Size(162, 13);
            this.donateLabel.TabIndex = 32;
            this.donateLabel.TabStop = true;
            this.donateLabel.Text = "Donation for further development";
            this.donateLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.donateLabel_LinkClicked);
            // 
            // fontSizeGB
            // 
            this.fontSizeGB.Controls.Add(this.DPI275);
            this.fontSizeGB.Controls.Add(this.DPI225);
            this.fontSizeGB.Controls.Add(this.fontSize275);
            this.fontSizeGB.Controls.Add(this.fontSize225);
            this.fontSizeGB.Controls.Add(this.DPIAUTO);
            this.fontSizeGB.Controls.Add(this.DPI125);
            this.fontSizeGB.Controls.Add(this.DPI175);
            this.fontSizeGB.Controls.Add(this.DPI100);
            this.fontSizeGB.Controls.Add(this.fontSize175);
            this.fontSizeGB.Controls.Add(this.fontSize125);
            this.fontSizeGB.Controls.Add(this.fontSize100);
            this.fontSizeGB.Location = new System.Drawing.Point(12, 304);
            this.fontSizeGB.Name = "fontSizeGB";
            this.fontSizeGB.Size = new System.Drawing.Size(160, 114);
            this.fontSizeGB.TabIndex = 33;
            this.fontSizeGB.TabStop = false;
            this.fontSizeGB.Text = "Scaling Mode";
            // 
            // DPI275
            // 
            this.DPI275.AutoSize = true;
            this.DPI275.Location = new System.Drawing.Point(81, 77);
            this.DPI275.Name = "DPI275";
            this.DPI275.Size = new System.Drawing.Size(72, 17);
            this.DPI275.TabIndex = 10;
            this.DPI275.TabStop = true;
            this.DPI275.Text = "DPI 2.75x";
            this.DPI275.UseVisualStyleBackColor = true;
            this.DPI275.CheckedChanged += new System.EventHandler(this.DPI275_CheckedChanged);
            // 
            // DPI225
            // 
            this.DPI225.AutoSize = true;
            this.DPI225.Location = new System.Drawing.Point(81, 61);
            this.DPI225.Name = "DPI225";
            this.DPI225.Size = new System.Drawing.Size(72, 17);
            this.DPI225.TabIndex = 9;
            this.DPI225.TabStop = true;
            this.DPI225.Text = "DPI 2.25x";
            this.DPI225.UseVisualStyleBackColor = true;
            this.DPI225.CheckedChanged += new System.EventHandler(this.DPI225_CheckedChanged);
            // 
            // fontSize275
            // 
            this.fontSize275.AutoSize = true;
            this.fontSize275.Location = new System.Drawing.Point(6, 77);
            this.fontSize275.Name = "fontSize275";
            this.fontSize275.Size = new System.Drawing.Size(75, 17);
            this.fontSize275.TabIndex = 8;
            this.fontSize275.TabStop = true;
            this.fontSize275.Text = "Font 2.75x";
            this.fontSize275.UseVisualStyleBackColor = true;
            this.fontSize275.CheckedChanged += new System.EventHandler(this.fontSize275_CheckedChanged);
            // 
            // fontSize225
            // 
            this.fontSize225.AutoSize = true;
            this.fontSize225.Location = new System.Drawing.Point(6, 61);
            this.fontSize225.Name = "fontSize225";
            this.fontSize225.Size = new System.Drawing.Size(75, 17);
            this.fontSize225.TabIndex = 7;
            this.fontSize225.TabStop = true;
            this.fontSize225.Text = "Font 2.25x";
            this.fontSize225.UseVisualStyleBackColor = true;
            this.fontSize225.CheckedChanged += new System.EventHandler(this.fontSize225_CheckedChanged);
            // 
            // DPIAUTO
            // 
            this.DPIAUTO.AutoSize = true;
            this.DPIAUTO.Location = new System.Drawing.Point(81, 93);
            this.DPIAUTO.Name = "DPIAUTO";
            this.DPIAUTO.Size = new System.Drawing.Size(76, 17);
            this.DPIAUTO.TabIndex = 6;
            this.DPIAUTO.TabStop = true;
            this.DPIAUTO.Text = "DPI AUTO";
            this.DPIAUTO.UseVisualStyleBackColor = true;
            this.DPIAUTO.CheckedChanged += new System.EventHandler(this.DPIAUTO_CheckedChanged);
            // 
            // DPI125
            // 
            this.DPI125.AutoSize = true;
            this.DPI125.Location = new System.Drawing.Point(81, 29);
            this.DPI125.Name = "DPI125";
            this.DPI125.Size = new System.Drawing.Size(72, 17);
            this.DPI125.TabIndex = 5;
            this.DPI125.TabStop = true;
            this.DPI125.Text = "DPI 1.25x";
            this.DPI125.UseVisualStyleBackColor = true;
            this.DPI125.CheckedChanged += new System.EventHandler(this.DPI125_CheckedChanged);
            // 
            // DPI175
            // 
            this.DPI175.AutoSize = true;
            this.DPI175.Location = new System.Drawing.Point(81, 45);
            this.DPI175.Name = "DPI175";
            this.DPI175.Size = new System.Drawing.Size(72, 17);
            this.DPI175.TabIndex = 4;
            this.DPI175.TabStop = true;
            this.DPI175.Text = "DPI 1.75x";
            this.DPI175.UseVisualStyleBackColor = true;
            this.DPI175.CheckedChanged += new System.EventHandler(this.DPI175_CheckedChanged);
            // 
            // DPI100
            // 
            this.DPI100.AutoSize = true;
            this.DPI100.Location = new System.Drawing.Point(81, 13);
            this.DPI100.Name = "DPI100";
            this.DPI100.Size = new System.Drawing.Size(57, 17);
            this.DPI100.TabIndex = 3;
            this.DPI100.TabStop = true;
            this.DPI100.Text = "DPI 1x";
            this.toolTip.SetToolTip(this.DPI100, "Right Click for extended description");
            this.DPI100.UseVisualStyleBackColor = true;
            this.DPI100.CheckedChanged += new System.EventHandler(this.DPI100_CheckedChanged);
            this.DPI100.MouseDown += new System.Windows.Forms.MouseEventHandler(this.font_MouseDown);
            this.DPI100.MouseEnter += new System.EventHandler(this.font_MouseEnter);
            this.DPI100.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // fontSize175
            // 
            this.fontSize175.AutoSize = true;
            this.fontSize175.Location = new System.Drawing.Point(6, 45);
            this.fontSize175.Name = "fontSize175";
            this.fontSize175.Size = new System.Drawing.Size(75, 17);
            this.fontSize175.TabIndex = 2;
            this.fontSize175.TabStop = true;
            this.fontSize175.Text = "Font 1.75x";
            this.fontSize175.UseVisualStyleBackColor = true;
            this.fontSize175.CheckedChanged += new System.EventHandler(this.fontSize175_CheckedChanged);
            this.fontSize175.MouseDown += new System.Windows.Forms.MouseEventHandler(this.font_MouseDown);
            this.fontSize175.MouseEnter += new System.EventHandler(this.font_MouseEnter);
            this.fontSize175.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // fontSize125
            // 
            this.fontSize125.AutoSize = true;
            this.fontSize125.Location = new System.Drawing.Point(6, 29);
            this.fontSize125.Name = "fontSize125";
            this.fontSize125.Size = new System.Drawing.Size(75, 17);
            this.fontSize125.TabIndex = 1;
            this.fontSize125.TabStop = true;
            this.fontSize125.Text = "Font 1.25x";
            this.fontSize125.UseVisualStyleBackColor = true;
            this.fontSize125.CheckedChanged += new System.EventHandler(this.fontSize125_CheckedChanged);
            this.fontSize125.MouseDown += new System.Windows.Forms.MouseEventHandler(this.font_MouseDown);
            this.fontSize125.MouseEnter += new System.EventHandler(this.font_MouseEnter);
            this.fontSize125.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // fontSize100
            // 
            this.fontSize100.AutoSize = true;
            this.fontSize100.Location = new System.Drawing.Point(6, 13);
            this.fontSize100.Name = "fontSize100";
            this.fontSize100.Size = new System.Drawing.Size(60, 17);
            this.fontSize100.TabIndex = 0;
            this.fontSize100.TabStop = true;
            this.fontSize100.Text = "Font 1x";
            this.toolTip.SetToolTip(this.fontSize100, "Right Click for extended description");
            this.fontSize100.UseVisualStyleBackColor = true;
            this.fontSize100.CheckedChanged += new System.EventHandler(this.fontSize100_CheckedChanged);
            this.fontSize100.MouseDown += new System.Windows.Forms.MouseEventHandler(this.font_MouseDown);
            this.fontSize100.MouseEnter += new System.EventHandler(this.font_MouseEnter);
            this.fontSize100.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // totalProgressBar
            // 
            this.totalProgressBar.Location = new System.Drawing.Point(13, 490);
            this.totalProgressBar.Name = "totalProgressBar";
            this.totalProgressBar.Size = new System.Drawing.Size(418, 16);
            this.totalProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.totalProgressBar.TabIndex = 34;
            // 
            // DiscordServerLink
            // 
            this.DiscordServerLink.AutoSize = true;
            this.DiscordServerLink.Location = new System.Drawing.Point(10, 614);
            this.DiscordServerLink.Name = "DiscordServerLink";
            this.DiscordServerLink.Size = new System.Drawing.Size(77, 13);
            this.DiscordServerLink.TabIndex = 35;
            this.DiscordServerLink.TabStop = true;
            this.DiscordServerLink.Text = "Discord Server";
            this.DiscordServerLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DiscordServerLink_LinkClicked);
            // 
            // viewAppUpdates
            // 
            this.viewAppUpdates.Location = new System.Drawing.Point(12, 48);
            this.viewAppUpdates.Name = "viewAppUpdates";
            this.viewAppUpdates.Size = new System.Drawing.Size(204, 23);
            this.viewAppUpdates.TabIndex = 36;
            this.viewAppUpdates.Text = "View latest application updates";
            this.viewAppUpdates.UseVisualStyleBackColor = true;
            this.viewAppUpdates.Click += new System.EventHandler(this.viewAppUpdates_Click);
            // 
            // viewDBUpdates
            // 
            this.viewDBUpdates.Location = new System.Drawing.Point(224, 48);
            this.viewDBUpdates.Name = "viewDBUpdates";
            this.viewDBUpdates.Size = new System.Drawing.Size(205, 23);
            this.viewDBUpdates.TabIndex = 37;
            this.viewDBUpdates.Text = "View latest database updates";
            this.viewDBUpdates.UseVisualStyleBackColor = true;
            this.viewDBUpdates.Click += new System.EventHandler(this.viewDBUpdates_Click);
            // 
            // ErrorCounterLabel
            // 
            this.ErrorCounterLabel.AutoSize = true;
            this.ErrorCounterLabel.Location = new System.Drawing.Point(330, 615);
            this.ErrorCounterLabel.Name = "ErrorCounterLabel";
            this.ErrorCounterLabel.Size = new System.Drawing.Size(80, 13);
            this.ErrorCounterLabel.TabIndex = 38;
            this.ErrorCounterLabel.Text = "Error counter: 0";
            this.ErrorCounterLabel.Visible = false;
            // 
            // createShortcutsCB
            // 
            this.createShortcutsCB.AutoSize = true;
            this.createShortcutsCB.Location = new System.Drawing.Point(6, 201);
            this.createShortcutsCB.Name = "createShortcutsCB";
            this.createShortcutsCB.Size = new System.Drawing.Size(105, 17);
            this.createShortcutsCB.TabIndex = 35;
            this.createShortcutsCB.Text = "Create Shortcuts";
            this.createShortcutsCB.UseVisualStyleBackColor = true;
            this.createShortcutsCB.CheckedChanged += new System.EventHandler(this.CreateShortcutsCB_CheckedChanged);
            this.createShortcutsCB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CreateShortcutsCB_MouseDown);
            this.createShortcutsCB.MouseEnter += new System.EventHandler(this.CreateShortcutsCB_MouseEnter);
            this.createShortcutsCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(442, 632);
            this.Controls.Add(this.ErrorCounterLabel);
            this.Controls.Add(this.viewDBUpdates);
            this.Controls.Add(this.viewAppUpdates);
            this.Controls.Add(this.DiscordServerLink);
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
        private System.Windows.Forms.RadioButton fontSize175;
        private System.Windows.Forms.RadioButton fontSize125;
        private System.Windows.Forms.RadioButton fontSize100;
        private System.Windows.Forms.RadioButton DPI100;
        private System.Windows.Forms.CheckBox disableBordersCB;
        private System.Windows.Forms.RadioButton DPI125;
        private System.Windows.Forms.RadioButton DPI175;
        private System.Windows.Forms.ProgressBar totalProgressBar;
        private System.Windows.Forms.LinkLabel DiscordServerLink;
        private System.Windows.Forms.CheckBox clearCacheCB;
        private System.Windows.Forms.RadioButton languageFR;
        private System.Windows.Forms.Button viewAppUpdates;
        private System.Windows.Forms.Button viewDBUpdates;
        private System.Windows.Forms.CheckBox disableColorsCB;
        private System.Windows.Forms.CheckBox clearLogFilesCB;
        private System.Windows.Forms.RadioButton DPIAUTO;
        private System.Windows.Forms.RadioButton DPI275;
        private System.Windows.Forms.RadioButton DPI225;
        private System.Windows.Forms.RadioButton fontSize275;
        private System.Windows.Forms.RadioButton fontSize225;
        private System.Windows.Forms.CheckBox notifyIfSameDatabaseCB;
        private System.Windows.Forms.CheckBox ShowInstallCompleteWindowCB;
        private System.Windows.Forms.ToolTip toolTip;
        public System.Windows.Forms.Label ErrorCounterLabel;
        private System.Windows.Forms.CheckBox createShortcutsCB;
    }
}

