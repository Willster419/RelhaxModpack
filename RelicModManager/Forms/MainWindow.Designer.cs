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
            this.FindWotExe = new System.Windows.Forms.OpenFileDialog();
            this.FormPageNALink = new System.Windows.Forms.LinkLabel();
            this.parrentProgressBar = new System.Windows.Forms.ProgressBar();
            this.installRelhaxMod = new System.Windows.Forms.Button();
            this.uninstallRelhaxMod = new System.Windows.Forms.Button();
            this.backupModsCheckBox = new System.Windows.Forms.CheckBox();
            this.settingsGroupBox = new System.Windows.Forms.GroupBox();
            this.SettingsTable = new System.Windows.Forms.TableLayoutPanel();
            this.ComicSansFontCB = new System.Windows.Forms.CheckBox();
            this.saveUserDataCB = new System.Windows.Forms.CheckBox();
            this.notifyIfSameDatabaseCB = new System.Windows.Forms.CheckBox();
            this.darkUICB = new System.Windows.Forms.CheckBox();
            this.saveLastInstallCB = new System.Windows.Forms.CheckBox();
            this.SuperExtractionCB = new System.Windows.Forms.CheckBox();
            this.clearLogFilesCB = new System.Windows.Forms.CheckBox();
            this.ShowAdvancedSettingsLink = new System.Windows.Forms.LinkLabel();
            this.languageSelectionGB = new System.Windows.Forms.GroupBox();
            this.LanguageComboBox = new System.Windows.Forms.ComboBox();
            this.findBugAddModLabel = new System.Windows.Forms.LinkLabel();
            this.cancelDownloadButton = new System.Windows.Forms.Button();
            this.DownloadTimer = new System.Windows.Forms.Timer(this.components);
            this.SelectionViewGB = new System.Windows.Forms.GroupBox();
            this.SelectionLayout = new System.Windows.Forms.TableLayoutPanel();
            this.selectionDefault = new System.Windows.Forms.RadioButton();
            this.expandNodesDefault = new System.Windows.Forms.CheckBox();
            this.selectionLegacy = new System.Windows.Forms.RadioButton();
            this.selectionLegacyV2 = new System.Windows.Forms.RadioButton();
            this.expandNodesDefault2 = new System.Windows.Forms.CheckBox();
            this.EnableBordersDefaultCB = new System.Windows.Forms.CheckBox();
            this.EnableBordersLegacyCB = new System.Windows.Forms.CheckBox();
            this.EnableColorChangeDefaultCB = new System.Windows.Forms.CheckBox();
            this.EnableColorChangeLegacyCB = new System.Windows.Forms.CheckBox();
            this.donateLabel = new System.Windows.Forms.LinkLabel();
            this.totalProgressBar = new System.Windows.Forms.ProgressBar();
            this.DiscordServerLink = new System.Windows.Forms.LinkLabel();
            this.viewAppUpdates = new System.Windows.Forms.Button();
            this.viewDBUpdates = new System.Windows.Forms.Button();
            this.ErrorCounterLabel = new System.Windows.Forms.Label();
            this.InfoTable = new System.Windows.Forms.TableLayoutPanel();
            this.ViewFacebookLink = new System.Windows.Forms.LinkLabel();
            this.ViewTwitterLink = new System.Windows.Forms.LinkLabel();
            this.SendEmailLink = new System.Windows.Forms.LinkLabel();
            this.VisitWebsiteLink = new System.Windows.Forms.LinkLabel();
            this.DatabaseVersionLabel = new System.Windows.Forms.Label();
            this.ApplicationVersionLabel = new System.Windows.Forms.Label();
            this.downloadProgress = new System.Windows.Forms.RichTextBox();
            this.FormPageEULink = new System.Windows.Forms.LinkLabel();
            this.FormPageEUGERLink = new System.Windows.Forms.LinkLabel();
            this.DiagnosticUtilitiesButton = new System.Windows.Forms.Button();
            this.ButtonTable = new System.Windows.Forms.TableLayoutPanel();
            this.ExportModeBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.fontSizeGB = new System.Windows.Forms.GroupBox();
            this.FontLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.DPI100 = new System.Windows.Forms.RadioButton();
            this.DPIAUTO = new System.Windows.Forms.RadioButton();
            this.DPI275 = new System.Windows.Forms.RadioButton();
            this.DPI125 = new System.Windows.Forms.RadioButton();
            this.DPI225 = new System.Windows.Forms.RadioButton();
            this.fontSize100 = new System.Windows.Forms.RadioButton();
            this.fontSize275 = new System.Windows.Forms.RadioButton();
            this.DPI175 = new System.Windows.Forms.RadioButton();
            this.fontSize125 = new System.Windows.Forms.RadioButton();
            this.fontSize225 = new System.Windows.Forms.RadioButton();
            this.fontSize175 = new System.Windows.Forms.RadioButton();
            this.settingsGroupBox.SuspendLayout();
            this.SettingsTable.SuspendLayout();
            this.languageSelectionGB.SuspendLayout();
            this.SelectionViewGB.SuspendLayout();
            this.SelectionLayout.SuspendLayout();
            this.InfoTable.SuspendLayout();
            this.ButtonTable.SuspendLayout();
            this.fontSizeGB.SuspendLayout();
            this.FontLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // childProgressBar
            // 
            this.InfoTable.SetColumnSpan(this.childProgressBar, 3);
            this.childProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.childProgressBar.Location = new System.Drawing.Point(3, 113);
            this.childProgressBar.Name = "childProgressBar";
            this.childProgressBar.Size = new System.Drawing.Size(461, 14);
            this.childProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.childProgressBar.TabIndex = 11;
            // 
            // FindWotExe
            // 
            this.FindWotExe.Filter = "WorldOfTanks.exe|WorldOfTanks.exe";
            this.FindWotExe.Title = "Find WorldOfTanks.exe";
            // 
            // FormPageNALink
            // 
            this.FormPageNALink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormPageNALink.Location = new System.Drawing.Point(289, 130);
            this.FormPageNALink.Name = "FormPageNALink";
            this.FormPageNALink.Size = new System.Drawing.Size(175, 20);
            this.FormPageNALink.TabIndex = 16;
            this.FormPageNALink.TabStop = true;
            this.FormPageNALink.Text = "WoT Form Page (NA, ENG)";
            this.FormPageNALink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.FormPageNALink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.FormPageNALink_LinkClicked);
            // 
            // parrentProgressBar
            // 
            this.InfoTable.SetColumnSpan(this.parrentProgressBar, 3);
            this.parrentProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parrentProgressBar.Location = new System.Drawing.Point(3, 93);
            this.parrentProgressBar.Name = "parrentProgressBar";
            this.parrentProgressBar.Size = new System.Drawing.Size(461, 14);
            this.parrentProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.parrentProgressBar.TabIndex = 17;
            // 
            // installRelhaxMod
            // 
            this.installRelhaxMod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.installRelhaxMod.Location = new System.Drawing.Point(235, 1);
            this.installRelhaxMod.Margin = new System.Windows.Forms.Padding(1);
            this.installRelhaxMod.Name = "installRelhaxMod";
            this.ButtonTable.SetRowSpan(this.installRelhaxMod, 2);
            this.installRelhaxMod.Size = new System.Drawing.Size(232, 56);
            this.installRelhaxMod.TabIndex = 19;
            this.installRelhaxMod.Text = "Install Relhax Modpack";
            this.installRelhaxMod.UseVisualStyleBackColor = true;
            this.installRelhaxMod.Click += new System.EventHandler(this.InstallRelhaxMod_Click);
            // 
            // uninstallRelhaxMod
            // 
            this.uninstallRelhaxMod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uninstallRelhaxMod.Location = new System.Drawing.Point(235, 59);
            this.uninstallRelhaxMod.Margin = new System.Windows.Forms.Padding(1);
            this.uninstallRelhaxMod.Name = "uninstallRelhaxMod";
            this.uninstallRelhaxMod.Size = new System.Drawing.Size(232, 30);
            this.uninstallRelhaxMod.TabIndex = 20;
            this.uninstallRelhaxMod.Text = "Uninstall Relhax Modpack";
            this.uninstallRelhaxMod.UseVisualStyleBackColor = true;
            this.uninstallRelhaxMod.Click += new System.EventHandler(this.UninstallRelhaxMod_Click);
            // 
            // backupModsCheckBox
            // 
            this.backupModsCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.backupModsCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.backupModsCheckBox.Location = new System.Drawing.Point(1, 41);
            this.backupModsCheckBox.Margin = new System.Windows.Forms.Padding(1);
            this.backupModsCheckBox.Name = "backupModsCheckBox";
            this.backupModsCheckBox.Size = new System.Drawing.Size(229, 33);
            this.backupModsCheckBox.TabIndex = 24;
            this.backupModsCheckBox.Text = "Backup current mods folder";
            this.backupModsCheckBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.backupModsCheckBox.UseVisualStyleBackColor = true;
            this.backupModsCheckBox.CheckedChanged += new System.EventHandler(this.backupModsCheckBox_CheckedChanged);
            this.backupModsCheckBox.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.backupModsCheckBox.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Controls.Add(this.SettingsTable);
            this.settingsGroupBox.Location = new System.Drawing.Point(12, 103);
            this.settingsGroupBox.MaximumSize = new System.Drawing.Size(502, 210);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(468, 135);
            this.settingsGroupBox.TabIndex = 25;
            this.settingsGroupBox.TabStop = false;
            this.settingsGroupBox.Text = "Modpack Settings";
            // 
            // SettingsTable
            // 
            this.SettingsTable.AutoSize = true;
            this.SettingsTable.ColumnCount = 2;
            this.SettingsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.SettingsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.SettingsTable.Controls.Add(this.ComicSansFontCB, 1, 3);
            this.SettingsTable.Controls.Add(this.saveUserDataCB, 0, 1);
            this.SettingsTable.Controls.Add(this.notifyIfSameDatabaseCB, 1, 2);
            this.SettingsTable.Controls.Add(this.darkUICB, 1, 0);
            this.SettingsTable.Controls.Add(this.saveLastInstallCB, 1, 1);
            this.SettingsTable.Controls.Add(this.SuperExtractionCB, 0, 3);
            this.SettingsTable.Controls.Add(this.clearLogFilesCB, 0, 0);
            this.SettingsTable.Controls.Add(this.backupModsCheckBox, 0, 2);
            this.SettingsTable.Controls.Add(this.ShowAdvancedSettingsLink, 1, 4);
            this.SettingsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTable.Location = new System.Drawing.Point(3, 16);
            this.SettingsTable.Name = "SettingsTable";
            this.SettingsTable.RowCount = 5;
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.Size = new System.Drawing.Size(462, 116);
            this.SettingsTable.TabIndex = 43;
            // 
            // ComicSansFontCB
            // 
            this.ComicSansFontCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ComicSansFontCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ComicSansFontCB.Location = new System.Drawing.Point(232, 76);
            this.ComicSansFontCB.Margin = new System.Windows.Forms.Padding(1);
            this.ComicSansFontCB.Name = "ComicSansFontCB";
            this.ComicSansFontCB.Size = new System.Drawing.Size(229, 18);
            this.ComicSansFontCB.TabIndex = 38;
            this.ComicSansFontCB.Text = "Comic Sans Font";
            this.ComicSansFontCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ComicSansFontCB.UseVisualStyleBackColor = true;
            this.ComicSansFontCB.CheckedChanged += new System.EventHandler(this.cancerFontCB_CheckedChanged);
            this.ComicSansFontCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.ComicSansFontCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // saveUserDataCB
            // 
            this.saveUserDataCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.saveUserDataCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveUserDataCB.Location = new System.Drawing.Point(1, 21);
            this.saveUserDataCB.Margin = new System.Windows.Forms.Padding(1);
            this.saveUserDataCB.Name = "saveUserDataCB";
            this.saveUserDataCB.Size = new System.Drawing.Size(229, 18);
            this.saveUserDataCB.TabIndex = 27;
            this.saveUserDataCB.Text = "Save User created data";
            this.saveUserDataCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.saveUserDataCB.UseVisualStyleBackColor = true;
            this.saveUserDataCB.CheckedChanged += new System.EventHandler(this.saveUserDataCB_CheckedChanged);
            this.saveUserDataCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.saveUserDataCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // notifyIfSameDatabaseCB
            // 
            this.notifyIfSameDatabaseCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.notifyIfSameDatabaseCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notifyIfSameDatabaseCB.Location = new System.Drawing.Point(232, 41);
            this.notifyIfSameDatabaseCB.Margin = new System.Windows.Forms.Padding(1);
            this.notifyIfSameDatabaseCB.Name = "notifyIfSameDatabaseCB";
            this.notifyIfSameDatabaseCB.Size = new System.Drawing.Size(229, 33);
            this.notifyIfSameDatabaseCB.TabIndex = 33;
            this.notifyIfSameDatabaseCB.Text = "Inform if no new database available";
            this.notifyIfSameDatabaseCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.notifyIfSameDatabaseCB.UseVisualStyleBackColor = true;
            this.notifyIfSameDatabaseCB.CheckedChanged += new System.EventHandler(this.notifyIfSameDatabaseCB_CheckedChanged);
            this.notifyIfSameDatabaseCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.notifyIfSameDatabaseCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // darkUICB
            // 
            this.darkUICB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.darkUICB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkUICB.Location = new System.Drawing.Point(232, 1);
            this.darkUICB.Margin = new System.Windows.Forms.Padding(1);
            this.darkUICB.Name = "darkUICB";
            this.darkUICB.Size = new System.Drawing.Size(229, 18);
            this.darkUICB.TabIndex = 30;
            this.darkUICB.Text = "Dark UI";
            this.darkUICB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.darkUICB.UseVisualStyleBackColor = true;
            this.darkUICB.CheckedChanged += new System.EventHandler(this.darkUICB_CheckedChanged);
            this.darkUICB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.darkUICB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // saveLastInstallCB
            // 
            this.saveLastInstallCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.saveLastInstallCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveLastInstallCB.Location = new System.Drawing.Point(232, 21);
            this.saveLastInstallCB.Margin = new System.Windows.Forms.Padding(1);
            this.saveLastInstallCB.Name = "saveLastInstallCB";
            this.saveLastInstallCB.Size = new System.Drawing.Size(229, 18);
            this.saveLastInstallCB.TabIndex = 26;
            this.saveLastInstallCB.Text = "Save selection of last install";
            this.saveLastInstallCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.saveLastInstallCB.UseVisualStyleBackColor = true;
            this.saveLastInstallCB.CheckedChanged += new System.EventHandler(this.saveLastInstallCB_CheckedChanged);
            this.saveLastInstallCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.saveLastInstallCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // SuperExtractionCB
            // 
            this.SuperExtractionCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.SuperExtractionCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SuperExtractionCB.Location = new System.Drawing.Point(1, 76);
            this.SuperExtractionCB.Margin = new System.Windows.Forms.Padding(1);
            this.SuperExtractionCB.Name = "SuperExtractionCB";
            this.SuperExtractionCB.Size = new System.Drawing.Size(229, 18);
            this.SuperExtractionCB.TabIndex = 37;
            this.SuperExtractionCB.Text = "Super extraction mode (Experimental)";
            this.SuperExtractionCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.SuperExtractionCB.UseVisualStyleBackColor = true;
            this.SuperExtractionCB.CheckedChanged += new System.EventHandler(this.SuperExtractionCB_CheckedChanged);
            this.SuperExtractionCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.SuperExtractionCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // clearLogFilesCB
            // 
            this.clearLogFilesCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.clearLogFilesCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clearLogFilesCB.Location = new System.Drawing.Point(1, 1);
            this.clearLogFilesCB.Margin = new System.Windows.Forms.Padding(1);
            this.clearLogFilesCB.Name = "clearLogFilesCB";
            this.clearLogFilesCB.Size = new System.Drawing.Size(229, 18);
            this.clearLogFilesCB.TabIndex = 32;
            this.clearLogFilesCB.Text = "Clear log files";
            this.clearLogFilesCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.clearLogFilesCB.UseVisualStyleBackColor = true;
            this.clearLogFilesCB.CheckedChanged += new System.EventHandler(this.clearLogFilesCB_CheckedChanged);
            this.clearLogFilesCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.clearLogFilesCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // ShowAdvancedSettingsLink
            // 
            this.ShowAdvancedSettingsLink.AutoSize = true;
            this.ShowAdvancedSettingsLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowAdvancedSettingsLink.Location = new System.Drawing.Point(234, 95);
            this.ShowAdvancedSettingsLink.Name = "ShowAdvancedSettingsLink";
            this.ShowAdvancedSettingsLink.Size = new System.Drawing.Size(225, 21);
            this.ShowAdvancedSettingsLink.TabIndex = 39;
            this.ShowAdvancedSettingsLink.TabStop = true;
            this.ShowAdvancedSettingsLink.Text = "View Advanced Settings";
            this.ShowAdvancedSettingsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowAdvancedSettingsLink_LinkClicked);
            this.ShowAdvancedSettingsLink.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.ShowAdvancedSettingsLink.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // languageSelectionGB
            // 
            this.languageSelectionGB.Controls.Add(this.LanguageComboBox);
            this.languageSelectionGB.Location = new System.Drawing.Point(12, 358);
            this.languageSelectionGB.Margin = new System.Windows.Forms.Padding(1);
            this.languageSelectionGB.Name = "languageSelectionGB";
            this.languageSelectionGB.Padding = new System.Windows.Forms.Padding(2);
            this.languageSelectionGB.Size = new System.Drawing.Size(160, 46);
            this.languageSelectionGB.TabIndex = 30;
            this.languageSelectionGB.TabStop = false;
            this.languageSelectionGB.Text = "Language";
            // 
            // LanguageComboBox
            // 
            this.LanguageComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LanguageComboBox.FormattingEnabled = true;
            this.LanguageComboBox.Items.AddRange(new object[] {
            "English",
            "Polski",
            "Deutsch",
            "Francais"});
            this.LanguageComboBox.Location = new System.Drawing.Point(2, 15);
            this.LanguageComboBox.Name = "LanguageComboBox";
            this.LanguageComboBox.Size = new System.Drawing.Size(156, 21);
            this.LanguageComboBox.TabIndex = 4;
            this.LanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.LanguageComboBox_SelectedIndexChanged);
            this.LanguageComboBox.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.LanguageComboBox.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // findBugAddModLabel
            // 
            this.findBugAddModLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.findBugAddModLabel.Location = new System.Drawing.Point(3, 130);
            this.findBugAddModLabel.Name = "findBugAddModLabel";
            this.findBugAddModLabel.Size = new System.Drawing.Size(176, 20);
            this.findBugAddModLabel.TabIndex = 27;
            this.findBugAddModLabel.TabStop = true;
            this.findBugAddModLabel.Text = "Find a bug? Want a mod added?";
            this.findBugAddModLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.findBugAddModLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.findBugAddModLabel_LinkClicked);
            // 
            // cancelDownloadButton
            // 
            this.cancelDownloadButton.Enabled = false;
            this.cancelDownloadButton.Location = new System.Drawing.Point(176, 360);
            this.cancelDownloadButton.Name = "cancelDownloadButton";
            this.cancelDownloadButton.Size = new System.Drawing.Size(141, 44);
            this.cancelDownloadButton.TabIndex = 28;
            this.cancelDownloadButton.Text = "Cancel Download";
            this.cancelDownloadButton.UseVisualStyleBackColor = true;
            this.cancelDownloadButton.Visible = false;
            this.cancelDownloadButton.Click += new System.EventHandler(this.cancelDownloadButton_Click);
            // 
            // DownloadTimer
            // 
            this.DownloadTimer.Interval = 1000;
            this.DownloadTimer.Tick += new System.EventHandler(this.DownloadTimer_Tick);
            // 
            // SelectionViewGB
            // 
            this.SelectionViewGB.Controls.Add(this.SelectionLayout);
            this.SelectionViewGB.Location = new System.Drawing.Point(12, 242);
            this.SelectionViewGB.Margin = new System.Windows.Forms.Padding(1);
            this.SelectionViewGB.Name = "SelectionViewGB";
            this.SelectionViewGB.Size = new System.Drawing.Size(305, 114);
            this.SelectionViewGB.TabIndex = 31;
            this.SelectionViewGB.TabStop = false;
            this.SelectionViewGB.Text = "Selection View";
            // 
            // SelectionLayout
            // 
            this.SelectionLayout.ColumnCount = 3;
            this.SelectionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.SelectionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 153F));
            this.SelectionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.SelectionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SelectionLayout.Controls.Add(this.selectionDefault, 0, 0);
            this.SelectionLayout.Controls.Add(this.expandNodesDefault, 1, 3);
            this.SelectionLayout.Controls.Add(this.selectionLegacy, 1, 0);
            this.SelectionLayout.Controls.Add(this.selectionLegacyV2, 2, 0);
            this.SelectionLayout.Controls.Add(this.expandNodesDefault2, 2, 1);
            this.SelectionLayout.Controls.Add(this.EnableBordersDefaultCB, 0, 1);
            this.SelectionLayout.Controls.Add(this.EnableBordersLegacyCB, 1, 1);
            this.SelectionLayout.Controls.Add(this.EnableColorChangeDefaultCB, 0, 2);
            this.SelectionLayout.Controls.Add(this.EnableColorChangeLegacyCB, 1, 2);
            this.SelectionLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectionLayout.Location = new System.Drawing.Point(3, 16);
            this.SelectionLayout.Name = "SelectionLayout";
            this.SelectionLayout.RowCount = 4;
            this.SelectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.SelectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.SelectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.SelectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.SelectionLayout.Size = new System.Drawing.Size(299, 95);
            this.SelectionLayout.TabIndex = 5;
            this.SelectionLayout.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.SelectionLayout.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // selectionDefault
            // 
            this.selectionDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectionDefault.Location = new System.Drawing.Point(1, 1);
            this.selectionDefault.Margin = new System.Windows.Forms.Padding(1);
            this.selectionDefault.Name = "selectionDefault";
            this.selectionDefault.Size = new System.Drawing.Size(136, 21);
            this.selectionDefault.TabIndex = 0;
            this.selectionDefault.TabStop = true;
            this.selectionDefault.Text = "Default";
            this.selectionDefault.UseVisualStyleBackColor = true;
            this.selectionDefault.CheckedChanged += new System.EventHandler(this.selectionDefault_CheckedChanged);
            this.selectionDefault.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.selectionDefault.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // expandNodesDefault
            // 
            this.expandNodesDefault.AutoSize = true;
            this.expandNodesDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expandNodesDefault.Location = new System.Drawing.Point(139, 70);
            this.expandNodesDefault.Margin = new System.Windows.Forms.Padding(1);
            this.expandNodesDefault.Name = "expandNodesDefault";
            this.expandNodesDefault.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.expandNodesDefault.Size = new System.Drawing.Size(151, 24);
            this.expandNodesDefault.TabIndex = 2;
            this.expandNodesDefault.Text = "Expand all";
            this.expandNodesDefault.UseVisualStyleBackColor = true;
            this.expandNodesDefault.CheckedChanged += new System.EventHandler(this.expandNodesDefault_CheckedChanged);
            this.expandNodesDefault.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.expandNodesDefault.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // selectionLegacy
            // 
            this.selectionLegacy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectionLegacy.Location = new System.Drawing.Point(139, 1);
            this.selectionLegacy.Margin = new System.Windows.Forms.Padding(1);
            this.selectionLegacy.Name = "selectionLegacy";
            this.selectionLegacy.Size = new System.Drawing.Size(151, 21);
            this.selectionLegacy.TabIndex = 1;
            this.selectionLegacy.TabStop = true;
            this.selectionLegacy.Text = "Legacy";
            this.selectionLegacy.UseVisualStyleBackColor = true;
            this.selectionLegacy.CheckedChanged += new System.EventHandler(this.selectionLegacy_CheckedChanged);
            this.selectionLegacy.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.selectionLegacy.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // selectionLegacyV2
            // 
            this.selectionLegacyV2.AutoSize = true;
            this.selectionLegacyV2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectionLegacyV2.Location = new System.Drawing.Point(292, 1);
            this.selectionLegacyV2.Margin = new System.Windows.Forms.Padding(1);
            this.selectionLegacyV2.Name = "selectionLegacyV2";
            this.selectionLegacyV2.Size = new System.Drawing.Size(6, 21);
            this.selectionLegacyV2.TabIndex = 3;
            this.selectionLegacyV2.TabStop = true;
            this.selectionLegacyV2.Text = "Legacy V2";
            this.selectionLegacyV2.UseVisualStyleBackColor = true;
            this.selectionLegacyV2.Visible = false;
            this.selectionLegacyV2.CheckedChanged += new System.EventHandler(this.selectionLegacyV2_CheckedChanged);
            this.selectionLegacyV2.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.selectionLegacyV2.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // expandNodesDefault2
            // 
            this.expandNodesDefault2.AutoSize = true;
            this.expandNodesDefault2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expandNodesDefault2.Location = new System.Drawing.Point(292, 24);
            this.expandNodesDefault2.Margin = new System.Windows.Forms.Padding(1);
            this.expandNodesDefault2.Name = "expandNodesDefault2";
            this.expandNodesDefault2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.expandNodesDefault2.Size = new System.Drawing.Size(6, 21);
            this.expandNodesDefault2.TabIndex = 4;
            this.expandNodesDefault2.Text = "Expand all";
            this.expandNodesDefault2.UseVisualStyleBackColor = true;
            this.expandNodesDefault2.Visible = false;
            this.expandNodesDefault2.CheckedChanged += new System.EventHandler(this.expandNodesDefault2_CheckedChanged);
            this.expandNodesDefault2.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.expandNodesDefault2.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // EnableBordersDefaultCB
            // 
            this.EnableBordersDefaultCB.AutoSize = true;
            this.EnableBordersDefaultCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnableBordersDefaultCB.Location = new System.Drawing.Point(1, 24);
            this.EnableBordersDefaultCB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableBordersDefaultCB.Name = "EnableBordersDefaultCB";
            this.EnableBordersDefaultCB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableBordersDefaultCB.Size = new System.Drawing.Size(136, 21);
            this.EnableBordersDefaultCB.TabIndex = 5;
            this.EnableBordersDefaultCB.Text = "Enable borders";
            this.EnableBordersDefaultCB.UseVisualStyleBackColor = true;
            this.EnableBordersDefaultCB.CheckedChanged += new System.EventHandler(this.EnableBordersDefaultCB_CheckedChanged);
            this.EnableBordersDefaultCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.EnableBordersDefaultCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // EnableBordersLegacyCB
            // 
            this.EnableBordersLegacyCB.AutoSize = true;
            this.EnableBordersLegacyCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnableBordersLegacyCB.Location = new System.Drawing.Point(139, 24);
            this.EnableBordersLegacyCB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableBordersLegacyCB.Name = "EnableBordersLegacyCB";
            this.EnableBordersLegacyCB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableBordersLegacyCB.Size = new System.Drawing.Size(151, 21);
            this.EnableBordersLegacyCB.TabIndex = 6;
            this.EnableBordersLegacyCB.Text = "Enable borders";
            this.EnableBordersLegacyCB.UseVisualStyleBackColor = true;
            this.EnableBordersLegacyCB.CheckedChanged += new System.EventHandler(this.EnableBordersLegacyCB_CheckedChanged);
            this.EnableBordersLegacyCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.EnableBordersLegacyCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // EnableColorChangeDefaultCB
            // 
            this.EnableColorChangeDefaultCB.AutoSize = true;
            this.EnableColorChangeDefaultCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnableColorChangeDefaultCB.Location = new System.Drawing.Point(1, 47);
            this.EnableColorChangeDefaultCB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableColorChangeDefaultCB.Name = "EnableColorChangeDefaultCB";
            this.EnableColorChangeDefaultCB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableColorChangeDefaultCB.Size = new System.Drawing.Size(136, 21);
            this.EnableColorChangeDefaultCB.TabIndex = 7;
            this.EnableColorChangeDefaultCB.Text = "Enable color change";
            this.EnableColorChangeDefaultCB.UseVisualStyleBackColor = true;
            this.EnableColorChangeDefaultCB.CheckedChanged += new System.EventHandler(this.EnableColorChangeDefaultCB_CheckedChanged);
            this.EnableColorChangeDefaultCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.EnableColorChangeDefaultCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // EnableColorChangeLegacyCB
            // 
            this.EnableColorChangeLegacyCB.AutoSize = true;
            this.EnableColorChangeLegacyCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnableColorChangeLegacyCB.Location = new System.Drawing.Point(139, 47);
            this.EnableColorChangeLegacyCB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableColorChangeLegacyCB.Name = "EnableColorChangeLegacyCB";
            this.EnableColorChangeLegacyCB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableColorChangeLegacyCB.Size = new System.Drawing.Size(151, 21);
            this.EnableColorChangeLegacyCB.TabIndex = 8;
            this.EnableColorChangeLegacyCB.Text = "Enable color change";
            this.EnableColorChangeLegacyCB.UseVisualStyleBackColor = true;
            this.EnableColorChangeLegacyCB.CheckedChanged += new System.EventHandler(this.EnableColorChangeLegacyCB_CheckedChanged);
            this.EnableColorChangeLegacyCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.EnableColorChangeLegacyCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // donateLabel
            // 
            this.donateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.donateLabel.Location = new System.Drawing.Point(3, 150);
            this.donateLabel.Name = "donateLabel";
            this.donateLabel.Size = new System.Drawing.Size(176, 20);
            this.donateLabel.TabIndex = 32;
            this.donateLabel.TabStop = true;
            this.donateLabel.Text = "Donation for further development";
            this.donateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.donateLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.donateLabel_LinkClicked);
            // 
            // totalProgressBar
            // 
            this.InfoTable.SetColumnSpan(this.totalProgressBar, 3);
            this.totalProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.totalProgressBar.Location = new System.Drawing.Point(3, 73);
            this.totalProgressBar.Name = "totalProgressBar";
            this.totalProgressBar.Size = new System.Drawing.Size(461, 14);
            this.totalProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.totalProgressBar.TabIndex = 34;
            // 
            // DiscordServerLink
            // 
            this.DiscordServerLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiscordServerLink.Location = new System.Drawing.Point(3, 170);
            this.DiscordServerLink.Name = "DiscordServerLink";
            this.DiscordServerLink.Size = new System.Drawing.Size(176, 20);
            this.DiscordServerLink.TabIndex = 35;
            this.DiscordServerLink.TabStop = true;
            this.DiscordServerLink.Text = "Discord Server";
            this.DiscordServerLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DiscordServerLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DiscordServerLink_LinkClicked);
            // 
            // viewAppUpdates
            // 
            this.viewAppUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewAppUpdates.Location = new System.Drawing.Point(1, 1);
            this.viewAppUpdates.Margin = new System.Windows.Forms.Padding(1);
            this.viewAppUpdates.Name = "viewAppUpdates";
            this.viewAppUpdates.Size = new System.Drawing.Size(232, 27);
            this.viewAppUpdates.TabIndex = 36;
            this.viewAppUpdates.Text = "View latest application updates";
            this.viewAppUpdates.UseVisualStyleBackColor = true;
            this.viewAppUpdates.Click += new System.EventHandler(this.viewAppUpdates_Click);
            // 
            // viewDBUpdates
            // 
            this.viewDBUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewDBUpdates.Location = new System.Drawing.Point(1, 30);
            this.viewDBUpdates.Margin = new System.Windows.Forms.Padding(1);
            this.viewDBUpdates.Name = "viewDBUpdates";
            this.viewDBUpdates.Size = new System.Drawing.Size(232, 27);
            this.viewDBUpdates.TabIndex = 37;
            this.viewDBUpdates.Text = "View latest database updates";
            this.viewDBUpdates.UseVisualStyleBackColor = true;
            this.viewDBUpdates.Click += new System.EventHandler(this.viewDBUpdates_Click);
            // 
            // ErrorCounterLabel
            // 
            this.ErrorCounterLabel.Location = new System.Drawing.Point(320, 387);
            this.ErrorCounterLabel.Name = "ErrorCounterLabel";
            this.ErrorCounterLabel.Size = new System.Drawing.Size(160, 20);
            this.ErrorCounterLabel.TabIndex = 38;
            this.ErrorCounterLabel.Text = "Error counter: 0";
            this.ErrorCounterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ErrorCounterLabel.Visible = false;
            // 
            // InfoTable
            // 
            this.InfoTable.ColumnCount = 3;
            this.InfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 63.66782F));
            this.InfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.33218F));
            this.InfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.InfoTable.Controls.Add(this.ViewFacebookLink, 1, 7);
            this.InfoTable.Controls.Add(this.ViewTwitterLink, 1, 6);
            this.InfoTable.Controls.Add(this.SendEmailLink, 1, 5);
            this.InfoTable.Controls.Add(this.VisitWebsiteLink, 1, 4);
            this.InfoTable.Controls.Add(this.DatabaseVersionLabel, 1, 8);
            this.InfoTable.Controls.Add(this.ApplicationVersionLabel, 0, 8);
            this.InfoTable.Controls.Add(this.downloadProgress, 0, 0);
            this.InfoTable.Controls.Add(this.totalProgressBar, 0, 1);
            this.InfoTable.Controls.Add(this.parrentProgressBar, 0, 2);
            this.InfoTable.Controls.Add(this.childProgressBar, 0, 3);
            this.InfoTable.Controls.Add(this.findBugAddModLabel, 0, 4);
            this.InfoTable.Controls.Add(this.FormPageNALink, 2, 4);
            this.InfoTable.Controls.Add(this.donateLabel, 0, 5);
            this.InfoTable.Controls.Add(this.DiscordServerLink, 0, 6);
            this.InfoTable.Controls.Add(this.FormPageEULink, 2, 5);
            this.InfoTable.Controls.Add(this.FormPageEUGERLink, 2, 6);
            this.InfoTable.Location = new System.Drawing.Point(13, 410);
            this.InfoTable.MaximumSize = new System.Drawing.Size(500, 233);
            this.InfoTable.MinimumSize = new System.Drawing.Size(450, 200);
            this.InfoTable.Name = "InfoTable";
            this.InfoTable.RowCount = 9;
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.InfoTable.Size = new System.Drawing.Size(467, 233);
            this.InfoTable.TabIndex = 39;
            // 
            // ViewFacebookLink
            // 
            this.ViewFacebookLink.AutoSize = true;
            this.ViewFacebookLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ViewFacebookLink.Location = new System.Drawing.Point(185, 190);
            this.ViewFacebookLink.Name = "ViewFacebookLink";
            this.ViewFacebookLink.Size = new System.Drawing.Size(98, 20);
            this.ViewFacebookLink.TabIndex = 39;
            this.ViewFacebookLink.TabStop = true;
            this.ViewFacebookLink.Text = "Facebook Page";
            this.ViewFacebookLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ViewFacebookLink_LinkClicked);
            // 
            // ViewTwitterLink
            // 
            this.ViewTwitterLink.AutoSize = true;
            this.ViewTwitterLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ViewTwitterLink.Location = new System.Drawing.Point(185, 170);
            this.ViewTwitterLink.Name = "ViewTwitterLink";
            this.ViewTwitterLink.Size = new System.Drawing.Size(98, 20);
            this.ViewTwitterLink.TabIndex = 38;
            this.ViewTwitterLink.TabStop = true;
            this.ViewTwitterLink.Text = "Twitter Page";
            this.ViewTwitterLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ViewTwitterLink_LinkClicked);
            // 
            // SendEmailLink
            // 
            this.SendEmailLink.AutoSize = true;
            this.SendEmailLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SendEmailLink.Location = new System.Drawing.Point(185, 150);
            this.SendEmailLink.Name = "SendEmailLink";
            this.SendEmailLink.Size = new System.Drawing.Size(98, 20);
            this.SendEmailLink.TabIndex = 37;
            this.SendEmailLink.TabStop = true;
            this.SendEmailLink.Text = "Send us an email";
            this.SendEmailLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SendEmailLink_LinkClicked);
            // 
            // VisitWebsiteLink
            // 
            this.VisitWebsiteLink.AutoSize = true;
            this.VisitWebsiteLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VisitWebsiteLink.Location = new System.Drawing.Point(185, 130);
            this.VisitWebsiteLink.Name = "VisitWebsiteLink";
            this.VisitWebsiteLink.Size = new System.Drawing.Size(98, 20);
            this.VisitWebsiteLink.TabIndex = 36;
            this.VisitWebsiteLink.TabStop = true;
            this.VisitWebsiteLink.Text = "Visit our website";
            this.VisitWebsiteLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.VisitWebsiteLink_LinkClicked);
            // 
            // DatabaseVersionLabel
            // 
            this.DatabaseVersionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InfoTable.SetColumnSpan(this.DatabaseVersionLabel, 2);
            this.DatabaseVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DatabaseVersionLabel.Location = new System.Drawing.Point(185, 210);
            this.DatabaseVersionLabel.Name = "DatabaseVersionLabel";
            this.DatabaseVersionLabel.Size = new System.Drawing.Size(279, 23);
            this.DatabaseVersionLabel.TabIndex = 0;
            this.DatabaseVersionLabel.Text = "Latest Database v{version}";
            this.DatabaseVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ApplicationVersionLabel
            // 
            this.ApplicationVersionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ApplicationVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ApplicationVersionLabel.Location = new System.Drawing.Point(3, 210);
            this.ApplicationVersionLabel.Name = "ApplicationVersionLabel";
            this.ApplicationVersionLabel.Size = new System.Drawing.Size(176, 23);
            this.ApplicationVersionLabel.TabIndex = 1;
            this.ApplicationVersionLabel.Text = "Application v{version}";
            this.ApplicationVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // downloadProgress
            // 
            this.InfoTable.SetColumnSpan(this.downloadProgress, 3);
            this.downloadProgress.DetectUrls = false;
            this.downloadProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.downloadProgress.Location = new System.Drawing.Point(3, 3);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.ReadOnly = true;
            this.downloadProgress.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.downloadProgress.Size = new System.Drawing.Size(461, 64);
            this.downloadProgress.TabIndex = 29;
            this.downloadProgress.Text = "";
            // 
            // FormPageEULink
            // 
            this.FormPageEULink.AutoSize = true;
            this.FormPageEULink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormPageEULink.Location = new System.Drawing.Point(289, 150);
            this.FormPageEULink.Name = "FormPageEULink";
            this.FormPageEULink.Size = new System.Drawing.Size(175, 20);
            this.FormPageEULink.TabIndex = 40;
            this.FormPageEULink.TabStop = true;
            this.FormPageEULink.Text = "WoT Form Page (EU, ENG)";
            this.FormPageEULink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.FormPageEULink_LinkClicked);
            // 
            // FormPageEUGERLink
            // 
            this.FormPageEUGERLink.AutoSize = true;
            this.FormPageEUGERLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormPageEUGERLink.Location = new System.Drawing.Point(289, 170);
            this.FormPageEUGERLink.Name = "FormPageEUGERLink";
            this.FormPageEUGERLink.Size = new System.Drawing.Size(175, 20);
            this.FormPageEUGERLink.TabIndex = 41;
            this.FormPageEUGERLink.TabStop = true;
            this.FormPageEUGERLink.Text = "WoT Form Page (EU, GER)";
            this.FormPageEUGERLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.FormPageEUGERLink_LinkClicked);
            // 
            // DiagnosticUtilitiesButton
            // 
            this.DiagnosticUtilitiesButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiagnosticUtilitiesButton.Location = new System.Drawing.Point(1, 59);
            this.DiagnosticUtilitiesButton.Margin = new System.Windows.Forms.Padding(1);
            this.DiagnosticUtilitiesButton.Name = "DiagnosticUtilitiesButton";
            this.DiagnosticUtilitiesButton.Size = new System.Drawing.Size(232, 30);
            this.DiagnosticUtilitiesButton.TabIndex = 40;
            this.DiagnosticUtilitiesButton.Text = "Diagnostic Utilities";
            this.DiagnosticUtilitiesButton.UseVisualStyleBackColor = true;
            this.DiagnosticUtilitiesButton.Click += new System.EventHandler(this.DiagnosticUtilitiesButton_Click);
            // 
            // ButtonTable
            // 
            this.ButtonTable.ColumnCount = 2;
            this.ButtonTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ButtonTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ButtonTable.Controls.Add(this.installRelhaxMod, 1, 0);
            this.ButtonTable.Controls.Add(this.viewAppUpdates, 0, 0);
            this.ButtonTable.Controls.Add(this.DiagnosticUtilitiesButton, 0, 2);
            this.ButtonTable.Controls.Add(this.viewDBUpdates, 0, 1);
            this.ButtonTable.Controls.Add(this.uninstallRelhaxMod, 1, 2);
            this.ButtonTable.Location = new System.Drawing.Point(12, 12);
            this.ButtonTable.Margin = new System.Windows.Forms.Padding(2);
            this.ButtonTable.MaximumSize = new System.Drawing.Size(550, 90);
            this.ButtonTable.Name = "ButtonTable";
            this.ButtonTable.RowCount = 3;
            this.ButtonTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.ButtonTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.ButtonTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.ButtonTable.Size = new System.Drawing.Size(468, 90);
            this.ButtonTable.TabIndex = 42;
            // 
            // fontSizeGB
            // 
            this.fontSizeGB.Controls.Add(this.FontLayoutPanel);
            this.fontSizeGB.Location = new System.Drawing.Point(319, 241);
            this.fontSizeGB.Margin = new System.Windows.Forms.Padding(1);
            this.fontSizeGB.Name = "fontSizeGB";
            this.fontSizeGB.Size = new System.Drawing.Size(161, 145);
            this.fontSizeGB.TabIndex = 44;
            this.fontSizeGB.TabStop = false;
            this.fontSizeGB.Text = "Scaling Mode";
            // 
            // FontLayoutPanel
            // 
            this.FontLayoutPanel.ColumnCount = 2;
            this.FontLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FontLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.FontLayoutPanel.Controls.Add(this.DPI100, 1, 0);
            this.FontLayoutPanel.Controls.Add(this.DPIAUTO, 1, 5);
            this.FontLayoutPanel.Controls.Add(this.DPI275, 1, 4);
            this.FontLayoutPanel.Controls.Add(this.DPI125, 1, 1);
            this.FontLayoutPanel.Controls.Add(this.DPI225, 1, 3);
            this.FontLayoutPanel.Controls.Add(this.fontSize100, 0, 0);
            this.FontLayoutPanel.Controls.Add(this.fontSize275, 0, 4);
            this.FontLayoutPanel.Controls.Add(this.DPI175, 1, 2);
            this.FontLayoutPanel.Controls.Add(this.fontSize125, 0, 1);
            this.FontLayoutPanel.Controls.Add(this.fontSize225, 0, 3);
            this.FontLayoutPanel.Controls.Add(this.fontSize175, 0, 2);
            this.FontLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FontLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.FontLayoutPanel.Name = "FontLayoutPanel";
            this.FontLayoutPanel.RowCount = 6;
            this.FontLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.FontLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.FontLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.FontLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.FontLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.FontLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.FontLayoutPanel.Size = new System.Drawing.Size(155, 126);
            this.FontLayoutPanel.TabIndex = 11;
            this.FontLayoutPanel.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.FontLayoutPanel.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // DPI100
            // 
            this.DPI100.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DPI100.Location = new System.Drawing.Point(78, 1);
            this.DPI100.Margin = new System.Windows.Forms.Padding(1);
            this.DPI100.Name = "DPI100";
            this.DPI100.Size = new System.Drawing.Size(76, 18);
            this.DPI100.TabIndex = 3;
            this.DPI100.TabStop = true;
            this.DPI100.Text = "DPI 1x";
            this.DPI100.UseVisualStyleBackColor = true;
            this.DPI100.CheckedChanged += new System.EventHandler(this.DPI100_CheckedChanged);
            this.DPI100.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // DPIAUTO
            // 
            this.DPIAUTO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DPIAUTO.Location = new System.Drawing.Point(78, 101);
            this.DPIAUTO.Margin = new System.Windows.Forms.Padding(1);
            this.DPIAUTO.Name = "DPIAUTO";
            this.DPIAUTO.Size = new System.Drawing.Size(76, 24);
            this.DPIAUTO.TabIndex = 6;
            this.DPIAUTO.TabStop = true;
            this.DPIAUTO.Text = "DPI AUTO";
            this.DPIAUTO.UseVisualStyleBackColor = true;
            this.DPIAUTO.CheckedChanged += new System.EventHandler(this.DPIAUTO_CheckedChanged);
            this.DPIAUTO.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // DPI275
            // 
            this.DPI275.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DPI275.Location = new System.Drawing.Point(78, 81);
            this.DPI275.Margin = new System.Windows.Forms.Padding(1);
            this.DPI275.Name = "DPI275";
            this.DPI275.Size = new System.Drawing.Size(76, 18);
            this.DPI275.TabIndex = 10;
            this.DPI275.TabStop = true;
            this.DPI275.Text = "DPI 2.75x";
            this.DPI275.UseVisualStyleBackColor = true;
            this.DPI275.CheckedChanged += new System.EventHandler(this.DPI275_CheckedChanged);
            this.DPI275.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // DPI125
            // 
            this.DPI125.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DPI125.Location = new System.Drawing.Point(78, 21);
            this.DPI125.Margin = new System.Windows.Forms.Padding(1);
            this.DPI125.Name = "DPI125";
            this.DPI125.Size = new System.Drawing.Size(76, 18);
            this.DPI125.TabIndex = 5;
            this.DPI125.TabStop = true;
            this.DPI125.Text = "DPI 1.25x";
            this.DPI125.UseVisualStyleBackColor = true;
            this.DPI125.CheckedChanged += new System.EventHandler(this.DPI125_CheckedChanged);
            this.DPI125.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // DPI225
            // 
            this.DPI225.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DPI225.Location = new System.Drawing.Point(78, 61);
            this.DPI225.Margin = new System.Windows.Forms.Padding(1);
            this.DPI225.Name = "DPI225";
            this.DPI225.Size = new System.Drawing.Size(76, 18);
            this.DPI225.TabIndex = 9;
            this.DPI225.TabStop = true;
            this.DPI225.Text = "DPI 2.25x";
            this.DPI225.UseVisualStyleBackColor = true;
            this.DPI225.CheckedChanged += new System.EventHandler(this.DPI225_CheckedChanged);
            this.DPI225.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // fontSize100
            // 
            this.fontSize100.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontSize100.Location = new System.Drawing.Point(1, 1);
            this.fontSize100.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize100.Name = "fontSize100";
            this.fontSize100.Size = new System.Drawing.Size(75, 18);
            this.fontSize100.TabIndex = 0;
            this.fontSize100.TabStop = true;
            this.fontSize100.Text = "Font 1x";
            this.fontSize100.UseVisualStyleBackColor = true;
            this.fontSize100.CheckedChanged += new System.EventHandler(this.fontSize100_CheckedChanged);
            this.fontSize100.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // fontSize275
            // 
            this.fontSize275.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontSize275.Location = new System.Drawing.Point(1, 81);
            this.fontSize275.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize275.Name = "fontSize275";
            this.fontSize275.Size = new System.Drawing.Size(75, 18);
            this.fontSize275.TabIndex = 8;
            this.fontSize275.TabStop = true;
            this.fontSize275.Text = "Font 2.75x";
            this.fontSize275.UseVisualStyleBackColor = true;
            this.fontSize275.CheckedChanged += new System.EventHandler(this.fontSize275_CheckedChanged);
            this.fontSize275.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // DPI175
            // 
            this.DPI175.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DPI175.Location = new System.Drawing.Point(78, 41);
            this.DPI175.Margin = new System.Windows.Forms.Padding(1);
            this.DPI175.Name = "DPI175";
            this.DPI175.Size = new System.Drawing.Size(76, 18);
            this.DPI175.TabIndex = 4;
            this.DPI175.TabStop = true;
            this.DPI175.Text = "DPI 1.75x";
            this.DPI175.UseVisualStyleBackColor = true;
            this.DPI175.CheckedChanged += new System.EventHandler(this.DPI175_CheckedChanged);
            this.DPI175.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // fontSize125
            // 
            this.fontSize125.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontSize125.Location = new System.Drawing.Point(1, 21);
            this.fontSize125.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize125.Name = "fontSize125";
            this.fontSize125.Size = new System.Drawing.Size(75, 18);
            this.fontSize125.TabIndex = 1;
            this.fontSize125.TabStop = true;
            this.fontSize125.Text = "Font 1.25x";
            this.fontSize125.UseVisualStyleBackColor = true;
            this.fontSize125.CheckedChanged += new System.EventHandler(this.fontSize125_CheckedChanged);
            this.fontSize125.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // fontSize225
            // 
            this.fontSize225.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontSize225.Location = new System.Drawing.Point(1, 61);
            this.fontSize225.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize225.Name = "fontSize225";
            this.fontSize225.Size = new System.Drawing.Size(75, 18);
            this.fontSize225.TabIndex = 7;
            this.fontSize225.TabStop = true;
            this.fontSize225.Text = "Font 2.25x";
            this.fontSize225.UseVisualStyleBackColor = true;
            this.fontSize225.CheckedChanged += new System.EventHandler(this.fontSize225_CheckedChanged);
            this.fontSize225.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // fontSize175
            // 
            this.fontSize175.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontSize175.Location = new System.Drawing.Point(1, 41);
            this.fontSize175.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize175.Name = "fontSize175";
            this.fontSize175.Size = new System.Drawing.Size(75, 18);
            this.fontSize175.TabIndex = 2;
            this.fontSize175.TabStop = true;
            this.fontSize175.Text = "Font 1.75x";
            this.fontSize175.UseVisualStyleBackColor = true;
            this.fontSize175.CheckedChanged += new System.EventHandler(this.fontSize175_CheckedChanged);
            this.fontSize175.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(490, 654);
            this.Controls.Add(this.fontSizeGB);
            this.Controls.Add(this.InfoTable);
            this.Controls.Add(this.SelectionViewGB);
            this.Controls.Add(this.ErrorCounterLabel);
            this.Controls.Add(this.languageSelectionGB);
            this.Controls.Add(this.ButtonTable);
            this.Controls.Add(this.settingsGroupBox);
            this.Controls.Add(this.cancelDownloadButton);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Relhax";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.settingsGroupBox.ResumeLayout(false);
            this.settingsGroupBox.PerformLayout();
            this.SettingsTable.ResumeLayout(false);
            this.SettingsTable.PerformLayout();
            this.languageSelectionGB.ResumeLayout(false);
            this.SelectionViewGB.ResumeLayout(false);
            this.SelectionLayout.ResumeLayout(false);
            this.SelectionLayout.PerformLayout();
            this.InfoTable.ResumeLayout(false);
            this.InfoTable.PerformLayout();
            this.ButtonTable.ResumeLayout(false);
            this.fontSizeGB.ResumeLayout(false);
            this.FontLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ProgressBar childProgressBar;
        private System.Windows.Forms.OpenFileDialog FindWotExe;
        private System.Windows.Forms.LinkLabel FormPageNALink;
        private System.Windows.Forms.ProgressBar parrentProgressBar;
        private System.Windows.Forms.Button installRelhaxMod;
        private System.Windows.Forms.Button uninstallRelhaxMod;
        private System.Windows.Forms.CheckBox backupModsCheckBox;
        private System.Windows.Forms.GroupBox settingsGroupBox;
        private System.Windows.Forms.LinkLabel findBugAddModLabel;
        private System.Windows.Forms.Button cancelDownloadButton;
        private System.Windows.Forms.CheckBox saveUserDataCB;
        private System.Windows.Forms.Timer DownloadTimer;
        private System.Windows.Forms.GroupBox languageSelectionGB;
        private System.Windows.Forms.GroupBox SelectionViewGB;
        private System.Windows.Forms.RadioButton selectionLegacy;
        private System.Windows.Forms.RadioButton selectionDefault;
        private System.Windows.Forms.LinkLabel donateLabel;
        private System.Windows.Forms.CheckBox expandNodesDefault;
        private System.Windows.Forms.ProgressBar totalProgressBar;
        private System.Windows.Forms.LinkLabel DiscordServerLink;
        private System.Windows.Forms.Button viewAppUpdates;
        private System.Windows.Forms.Button viewDBUpdates;
        private System.Windows.Forms.CheckBox clearLogFilesCB;
        public System.Windows.Forms.Label ErrorCounterLabel;
        private System.Windows.Forms.TableLayoutPanel InfoTable;
        private System.Windows.Forms.Label ApplicationVersionLabel;
        private System.Windows.Forms.ComboBox LanguageComboBox;
        private System.Windows.Forms.Button DiagnosticUtilitiesButton;
        private System.Windows.Forms.CheckBox SuperExtractionCB;
        private System.Windows.Forms.Label DatabaseVersionLabel;
        private System.Windows.Forms.TableLayoutPanel ButtonTable;
        private System.Windows.Forms.TableLayoutPanel SettingsTable;
        private System.Windows.Forms.TableLayoutPanel SelectionLayout;
        private System.Windows.Forms.RichTextBox downloadProgress;
        private System.Windows.Forms.FolderBrowserDialog ExportModeBrowserDialog;
        private System.Windows.Forms.RadioButton selectionLegacyV2;
        private System.Windows.Forms.CheckBox expandNodesDefault2;
        private System.Windows.Forms.CheckBox EnableBordersDefaultCB;
        private System.Windows.Forms.CheckBox EnableBordersLegacyCB;
        private System.Windows.Forms.CheckBox EnableColorChangeDefaultCB;
        private System.Windows.Forms.CheckBox EnableColorChangeLegacyCB;
        private System.Windows.Forms.CheckBox notifyIfSameDatabaseCB;
        private System.Windows.Forms.CheckBox darkUICB;
        private System.Windows.Forms.CheckBox saveLastInstallCB;
        private System.Windows.Forms.GroupBox fontSizeGB;
        private System.Windows.Forms.TableLayoutPanel FontLayoutPanel;
        private System.Windows.Forms.RadioButton DPI100;
        private System.Windows.Forms.RadioButton DPIAUTO;
        private System.Windows.Forms.RadioButton DPI275;
        private System.Windows.Forms.RadioButton DPI125;
        private System.Windows.Forms.RadioButton DPI225;
        private System.Windows.Forms.RadioButton fontSize100;
        private System.Windows.Forms.RadioButton fontSize275;
        private System.Windows.Forms.RadioButton DPI175;
        private System.Windows.Forms.RadioButton fontSize125;
        private System.Windows.Forms.RadioButton fontSize225;
        private System.Windows.Forms.RadioButton fontSize175;
        private System.Windows.Forms.CheckBox ComicSansFontCB;
        private System.Windows.Forms.LinkLabel ShowAdvancedSettingsLink;
        private System.Windows.Forms.LinkLabel VisitWebsiteLink;
        private System.Windows.Forms.LinkLabel SendEmailLink;
        private System.Windows.Forms.LinkLabel ViewTwitterLink;
        private System.Windows.Forms.LinkLabel ViewFacebookLink;
        private System.Windows.Forms.LinkLabel FormPageEULink;
        private System.Windows.Forms.LinkLabel FormPageEUGERLink;
    }
}

