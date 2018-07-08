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
            this.ErrorCounterLabel = new System.Windows.Forms.Label();
            this.languageSelectionGB = new System.Windows.Forms.GroupBox();
            this.LanguageComboBox = new System.Windows.Forms.ComboBox();
            this.cancelDownloadButton = new System.Windows.Forms.Button();
            this.DownloadTimer = new System.Windows.Forms.Timer(this.components);
            this.SelectionViewGB = new System.Windows.Forms.GroupBox();
            this.SelectionLayout = new System.Windows.Forms.TableLayoutPanel();
            this.SelectionDefault = new System.Windows.Forms.RadioButton();
            this.SelectionLegacy = new System.Windows.Forms.RadioButton();
            this.SelectionDefaultV2 = new System.Windows.Forms.RadioButton();
            this.EnableBordersDefaultCB = new System.Windows.Forms.CheckBox();
            this.EnableBordersLegacyCB = new System.Windows.Forms.CheckBox();
            this.EnableColorChangeDefaultCB = new System.Windows.Forms.CheckBox();
            this.EnableColorChangeLegacyCB = new System.Windows.Forms.CheckBox();
            this.EnableBordersDefaultV2CB = new System.Windows.Forms.CheckBox();
            this.EnableColorChangeDefaultV2CB = new System.Windows.Forms.CheckBox();
            this.totalProgressBar = new System.Windows.Forms.ProgressBar();
            this.viewAppUpdates = new System.Windows.Forms.Button();
            this.viewDBUpdates = new System.Windows.Forms.Button();
            this.downloadProgress = new System.Windows.Forms.RichTextBox();
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
            this.RelhaxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemRestore = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemCheckUpdates = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemAppClose = new System.Windows.Forms.ToolStripMenuItem();
            this.RMIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.DonateButton = new System.Windows.Forms.Button();
            this.Forms_ENG_NAButton = new System.Windows.Forms.Button();
            this.FormsENG_EUButton = new System.Windows.Forms.Button();
            this.FormsENG_GERButton = new System.Windows.Forms.Button();
            this.FacebookButton = new System.Windows.Forms.Button();
            this.SendEmailButton = new System.Windows.Forms.Button();
            this.DiscordButton = new System.Windows.Forms.Button();
            this.TwitterButton = new System.Windows.Forms.Button();
            this.FindBugAddModButton = new System.Windows.Forms.Button();
            this.HomepageButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ApplicationVersionLabel = new System.Windows.Forms.Label();
            this.DatabaseVersionLabel = new System.Windows.Forms.Label();
            this.backupModsSizeLabel = new System.Windows.Forms.Label();
            this.settingsGroupBox.SuspendLayout();
            this.SettingsTable.SuspendLayout();
            this.languageSelectionGB.SuspendLayout();
            this.SelectionViewGB.SuspendLayout();
            this.SelectionLayout.SuspendLayout();
            this.ButtonTable.SuspendLayout();
            this.fontSizeGB.SuspendLayout();
            this.FontLayoutPanel.SuspendLayout();
            this.RelhaxMenuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // childProgressBar
            // 
            this.childProgressBar.Location = new System.Drawing.Point(16, 526);
            this.childProgressBar.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.childProgressBar.Name = "childProgressBar";
            this.childProgressBar.Size = new System.Drawing.Size(478, 14);
            this.childProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.childProgressBar.TabIndex = 11;
            // 
            // FindWotExe
            // 
            this.FindWotExe.Filter = "WorldOfTanks.exe|WorldOfTanks.exe";
            this.FindWotExe.Title = "Find WorldOfTanks.exe";
            // 
            // parrentProgressBar
            // 
            this.parrentProgressBar.Location = new System.Drawing.Point(16, 506);
            this.parrentProgressBar.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.parrentProgressBar.Name = "parrentProgressBar";
            this.parrentProgressBar.Size = new System.Drawing.Size(478, 14);
            this.parrentProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.parrentProgressBar.TabIndex = 17;
            // 
            // installRelhaxMod
            // 
            this.installRelhaxMod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.installRelhaxMod.Location = new System.Drawing.Point(242, 1);
            this.installRelhaxMod.Margin = new System.Windows.Forms.Padding(1);
            this.installRelhaxMod.Name = "installRelhaxMod";
            this.ButtonTable.SetRowSpan(this.installRelhaxMod, 2);
            this.installRelhaxMod.Size = new System.Drawing.Size(239, 56);
            this.installRelhaxMod.TabIndex = 19;
            this.installRelhaxMod.Text = "Install Relhax Modpack";
            this.installRelhaxMod.UseVisualStyleBackColor = true;
            this.installRelhaxMod.Click += new System.EventHandler(this.InstallRelhaxMod_Click);
            // 
            // uninstallRelhaxMod
            // 
            this.uninstallRelhaxMod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uninstallRelhaxMod.Location = new System.Drawing.Point(242, 59);
            this.uninstallRelhaxMod.Margin = new System.Windows.Forms.Padding(1);
            this.uninstallRelhaxMod.Name = "uninstallRelhaxMod";
            this.uninstallRelhaxMod.Size = new System.Drawing.Size(239, 30);
            this.uninstallRelhaxMod.TabIndex = 20;
            this.uninstallRelhaxMod.Text = "Uninstall Relhax Modpack";
            this.uninstallRelhaxMod.UseVisualStyleBackColor = true;
            this.uninstallRelhaxMod.Click += new System.EventHandler(this.UninstallRelhaxMod_Click);
            // 
            // backupModsCheckBox
            // 
            this.backupModsCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.backupModsCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.backupModsCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.backupModsCheckBox.Location = new System.Drawing.Point(1, 41);
            this.backupModsCheckBox.Margin = new System.Windows.Forms.Padding(1);
            this.backupModsCheckBox.Name = "backupModsCheckBox";
            this.backupModsCheckBox.Size = new System.Drawing.Size(236, 33);
            this.backupModsCheckBox.TabIndex = 24;
            this.backupModsCheckBox.Text = "Backup current mods folder";
            this.backupModsCheckBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.backupModsCheckBox.UseVisualStyleBackColor = false;
            this.backupModsCheckBox.CheckedChanged += new System.EventHandler(this.BackupModsCheckBox_CheckedChanged);
            this.backupModsCheckBox.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.backupModsCheckBox.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsGroupBox.BackColor = System.Drawing.Color.Transparent;
            this.settingsGroupBox.Controls.Add(this.SettingsTable);
            this.settingsGroupBox.Location = new System.Drawing.Point(12, 103);
            this.settingsGroupBox.MaximumSize = new System.Drawing.Size(502, 210);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(482, 135);
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
            this.SettingsTable.Controls.Add(this.SuperExtractionCB, 0, 4);
            this.SettingsTable.Controls.Add(this.clearLogFilesCB, 0, 0);
            this.SettingsTable.Controls.Add(this.backupModsCheckBox, 0, 2);
            this.SettingsTable.Controls.Add(this.ShowAdvancedSettingsLink, 1, 4);
            this.SettingsTable.Controls.Add(this.backupModsSizeLabel, 0, 3);
            this.SettingsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTable.Location = new System.Drawing.Point(3, 16);
            this.SettingsTable.Name = "SettingsTable";
            this.SettingsTable.RowCount = 5;
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.Size = new System.Drawing.Size(476, 116);
            this.SettingsTable.TabIndex = 43;
            // 
            // ComicSansFontCB
            // 
            this.ComicSansFontCB.BackColor = System.Drawing.Color.Transparent;
            this.ComicSansFontCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ComicSansFontCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ComicSansFontCB.Location = new System.Drawing.Point(239, 76);
            this.ComicSansFontCB.Margin = new System.Windows.Forms.Padding(1);
            this.ComicSansFontCB.Name = "ComicSansFontCB";
            this.ComicSansFontCB.Size = new System.Drawing.Size(236, 18);
            this.ComicSansFontCB.TabIndex = 38;
            this.ComicSansFontCB.Text = "Comic Sans Font";
            this.ComicSansFontCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ComicSansFontCB.UseVisualStyleBackColor = false;
            this.ComicSansFontCB.CheckedChanged += new System.EventHandler(this.CancerFontCB_CheckedChanged);
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
            this.saveUserDataCB.Size = new System.Drawing.Size(236, 18);
            this.saveUserDataCB.TabIndex = 27;
            this.saveUserDataCB.Text = "Save User created data";
            this.saveUserDataCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.saveUserDataCB.UseVisualStyleBackColor = true;
            this.saveUserDataCB.CheckedChanged += new System.EventHandler(this.SaveUserDataCB_CheckedChanged);
            this.saveUserDataCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.saveUserDataCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // notifyIfSameDatabaseCB
            // 
            this.notifyIfSameDatabaseCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.notifyIfSameDatabaseCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notifyIfSameDatabaseCB.Location = new System.Drawing.Point(239, 41);
            this.notifyIfSameDatabaseCB.Margin = new System.Windows.Forms.Padding(1);
            this.notifyIfSameDatabaseCB.Name = "notifyIfSameDatabaseCB";
            this.notifyIfSameDatabaseCB.Size = new System.Drawing.Size(236, 33);
            this.notifyIfSameDatabaseCB.TabIndex = 33;
            this.notifyIfSameDatabaseCB.Text = "Inform if no new database available";
            this.notifyIfSameDatabaseCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.notifyIfSameDatabaseCB.UseVisualStyleBackColor = true;
            this.notifyIfSameDatabaseCB.CheckedChanged += new System.EventHandler(this.NotifyIfSameDatabaseCB_CheckedChanged);
            this.notifyIfSameDatabaseCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.notifyIfSameDatabaseCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // darkUICB
            // 
            this.darkUICB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.darkUICB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkUICB.Location = new System.Drawing.Point(239, 1);
            this.darkUICB.Margin = new System.Windows.Forms.Padding(1);
            this.darkUICB.Name = "darkUICB";
            this.darkUICB.Size = new System.Drawing.Size(236, 18);
            this.darkUICB.TabIndex = 30;
            this.darkUICB.Text = "Dark UI";
            this.darkUICB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.darkUICB.UseVisualStyleBackColor = true;
            this.darkUICB.CheckedChanged += new System.EventHandler(this.DarkUICB_CheckedChanged);
            this.darkUICB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.darkUICB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // saveLastInstallCB
            // 
            this.saveLastInstallCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.saveLastInstallCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveLastInstallCB.Location = new System.Drawing.Point(239, 21);
            this.saveLastInstallCB.Margin = new System.Windows.Forms.Padding(1);
            this.saveLastInstallCB.Name = "saveLastInstallCB";
            this.saveLastInstallCB.Size = new System.Drawing.Size(236, 18);
            this.saveLastInstallCB.TabIndex = 26;
            this.saveLastInstallCB.Text = "Save selection of last install";
            this.saveLastInstallCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.saveLastInstallCB.UseVisualStyleBackColor = true;
            this.saveLastInstallCB.CheckedChanged += new System.EventHandler(this.SaveLastInstallCB_CheckedChanged);
            this.saveLastInstallCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.saveLastInstallCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // SuperExtractionCB
            // 
            this.SuperExtractionCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.SuperExtractionCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SuperExtractionCB.Location = new System.Drawing.Point(1, 96);
            this.SuperExtractionCB.Margin = new System.Windows.Forms.Padding(1);
            this.SuperExtractionCB.Name = "SuperExtractionCB";
            this.SuperExtractionCB.Size = new System.Drawing.Size(236, 19);
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
            this.clearLogFilesCB.Size = new System.Drawing.Size(236, 18);
            this.clearLogFilesCB.TabIndex = 32;
            this.clearLogFilesCB.Text = "Clear log files";
            this.clearLogFilesCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.clearLogFilesCB.UseVisualStyleBackColor = true;
            this.clearLogFilesCB.CheckedChanged += new System.EventHandler(this.ClearLogFilesCB_CheckedChanged);
            this.clearLogFilesCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.clearLogFilesCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // ShowAdvancedSettingsLink
            // 
            this.ShowAdvancedSettingsLink.AutoSize = true;
            this.ShowAdvancedSettingsLink.BackColor = System.Drawing.Color.Transparent;
            this.ShowAdvancedSettingsLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowAdvancedSettingsLink.Location = new System.Drawing.Point(241, 95);
            this.ShowAdvancedSettingsLink.Name = "ShowAdvancedSettingsLink";
            this.ShowAdvancedSettingsLink.Size = new System.Drawing.Size(232, 21);
            this.ShowAdvancedSettingsLink.TabIndex = 39;
            this.ShowAdvancedSettingsLink.TabStop = true;
            this.ShowAdvancedSettingsLink.Text = "View Advanced Settings";
            this.ShowAdvancedSettingsLink.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ShowAdvancedSettingsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowAdvancedSettingsLink_LinkClicked);
            this.ShowAdvancedSettingsLink.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.ShowAdvancedSettingsLink.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // ErrorCounterLabel
            // 
            this.ErrorCounterLabel.BackColor = System.Drawing.Color.Transparent;
            this.ErrorCounterLabel.Location = new System.Drawing.Point(13, 390);
            this.ErrorCounterLabel.Name = "ErrorCounterLabel";
            this.ErrorCounterLabel.Size = new System.Drawing.Size(135, 20);
            this.ErrorCounterLabel.TabIndex = 38;
            this.ErrorCounterLabel.Text = "Error counter: 0";
            this.ErrorCounterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ErrorCounterLabel.Visible = false;
            // 
            // languageSelectionGB
            // 
            this.languageSelectionGB.BackColor = System.Drawing.Color.Transparent;
            this.languageSelectionGB.Controls.Add(this.LanguageComboBox);
            this.languageSelectionGB.Location = new System.Drawing.Point(334, 368);
            this.languageSelectionGB.Margin = new System.Windows.Forms.Padding(1);
            this.languageSelectionGB.Name = "languageSelectionGB";
            this.languageSelectionGB.Padding = new System.Windows.Forms.Padding(2);
            this.languageSelectionGB.Size = new System.Drawing.Size(160, 44);
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
            // cancelDownloadButton
            // 
            this.cancelDownloadButton.Enabled = false;
            this.cancelDownloadButton.Location = new System.Drawing.Point(173, 383);
            this.cancelDownloadButton.Name = "cancelDownloadButton";
            this.cancelDownloadButton.Size = new System.Drawing.Size(156, 21);
            this.cancelDownloadButton.TabIndex = 28;
            this.cancelDownloadButton.Text = "Cancel Download";
            this.cancelDownloadButton.UseVisualStyleBackColor = true;
            this.cancelDownloadButton.Visible = false;
            this.cancelDownloadButton.Click += new System.EventHandler(this.CancelDownloadButton_Click);
            // 
            // DownloadTimer
            // 
            this.DownloadTimer.Interval = 1000;
            this.DownloadTimer.Tick += new System.EventHandler(this.DownloadTimer_Tick);
            // 
            // SelectionViewGB
            // 
            this.SelectionViewGB.BackColor = System.Drawing.Color.Transparent;
            this.SelectionViewGB.Controls.Add(this.SelectionLayout);
            this.SelectionViewGB.Location = new System.Drawing.Point(12, 242);
            this.SelectionViewGB.Margin = new System.Windows.Forms.Padding(1);
            this.SelectionViewGB.Name = "SelectionViewGB";
            this.SelectionViewGB.Size = new System.Drawing.Size(320, 125);
            this.SelectionViewGB.TabIndex = 31;
            this.SelectionViewGB.TabStop = false;
            this.SelectionViewGB.Text = "Selection View";
            // 
            // SelectionLayout
            // 
            this.SelectionLayout.BackColor = System.Drawing.Color.Transparent;
            this.SelectionLayout.ColumnCount = 3;
            this.SelectionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.SelectionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.SelectionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.SelectionLayout.Controls.Add(this.SelectionDefault, 0, 0);
            this.SelectionLayout.Controls.Add(this.SelectionLegacy, 1, 0);
            this.SelectionLayout.Controls.Add(this.SelectionDefaultV2, 2, 0);
            this.SelectionLayout.Controls.Add(this.EnableBordersDefaultCB, 0, 1);
            this.SelectionLayout.Controls.Add(this.EnableBordersLegacyCB, 1, 1);
            this.SelectionLayout.Controls.Add(this.EnableColorChangeDefaultCB, 0, 2);
            this.SelectionLayout.Controls.Add(this.EnableColorChangeLegacyCB, 1, 2);
            this.SelectionLayout.Controls.Add(this.EnableBordersDefaultV2CB, 2, 1);
            this.SelectionLayout.Controls.Add(this.EnableColorChangeDefaultV2CB, 2, 2);
            this.SelectionLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectionLayout.Location = new System.Drawing.Point(3, 16);
            this.SelectionLayout.Name = "SelectionLayout";
            this.SelectionLayout.RowCount = 4;
            this.SelectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.SelectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.SelectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.SelectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.SelectionLayout.Size = new System.Drawing.Size(314, 106);
            this.SelectionLayout.TabIndex = 5;
            this.SelectionLayout.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.SelectionLayout.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // SelectionDefault
            // 
            this.SelectionDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectionDefault.Location = new System.Drawing.Point(1, 1);
            this.SelectionDefault.Margin = new System.Windows.Forms.Padding(1);
            this.SelectionDefault.Name = "SelectionDefault";
            this.SelectionDefault.Size = new System.Drawing.Size(102, 32);
            this.SelectionDefault.TabIndex = 0;
            this.SelectionDefault.TabStop = true;
            this.SelectionDefault.Text = "Default";
            this.SelectionDefault.UseVisualStyleBackColor = true;
            this.SelectionDefault.CheckedChanged += new System.EventHandler(this.SelectionDefault_CheckedChanged);
            this.SelectionDefault.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.SelectionDefault.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // SelectionLegacy
            // 
            this.SelectionLegacy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectionLegacy.Location = new System.Drawing.Point(105, 1);
            this.SelectionLegacy.Margin = new System.Windows.Forms.Padding(1);
            this.SelectionLegacy.Name = "SelectionLegacy";
            this.SelectionLegacy.Size = new System.Drawing.Size(102, 32);
            this.SelectionLegacy.TabIndex = 1;
            this.SelectionLegacy.TabStop = true;
            this.SelectionLegacy.Text = "Legacy";
            this.SelectionLegacy.UseVisualStyleBackColor = true;
            this.SelectionLegacy.CheckedChanged += new System.EventHandler(this.SelectionLegacy_CheckedChanged);
            this.SelectionLegacy.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.SelectionLegacy.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // SelectionDefaultV2
            // 
            this.SelectionDefaultV2.AutoSize = true;
            this.SelectionDefaultV2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectionDefaultV2.Location = new System.Drawing.Point(209, 1);
            this.SelectionDefaultV2.Margin = new System.Windows.Forms.Padding(1);
            this.SelectionDefaultV2.Name = "SelectionDefaultV2";
            this.SelectionDefaultV2.Size = new System.Drawing.Size(104, 32);
            this.SelectionDefaultV2.TabIndex = 3;
            this.SelectionDefaultV2.TabStop = true;
            this.SelectionDefaultV2.Text = "Default V2";
            this.SelectionDefaultV2.UseVisualStyleBackColor = true;
            this.SelectionDefaultV2.CheckedChanged += new System.EventHandler(this.SelectionDefaultV2_CheckedChanged);
            this.SelectionDefaultV2.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.SelectionDefaultV2.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // EnableBordersDefaultCB
            // 
            this.EnableBordersDefaultCB.AutoSize = true;
            this.EnableBordersDefaultCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnableBordersDefaultCB.Location = new System.Drawing.Point(1, 35);
            this.EnableBordersDefaultCB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableBordersDefaultCB.Name = "EnableBordersDefaultCB";
            this.EnableBordersDefaultCB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableBordersDefaultCB.Size = new System.Drawing.Size(102, 32);
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
            this.EnableBordersLegacyCB.Location = new System.Drawing.Point(105, 35);
            this.EnableBordersLegacyCB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableBordersLegacyCB.Name = "EnableBordersLegacyCB";
            this.EnableBordersLegacyCB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableBordersLegacyCB.Size = new System.Drawing.Size(102, 32);
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
            this.EnableColorChangeDefaultCB.Location = new System.Drawing.Point(1, 69);
            this.EnableColorChangeDefaultCB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableColorChangeDefaultCB.Name = "EnableColorChangeDefaultCB";
            this.EnableColorChangeDefaultCB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableColorChangeDefaultCB.Size = new System.Drawing.Size(102, 32);
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
            this.EnableColorChangeLegacyCB.Location = new System.Drawing.Point(105, 69);
            this.EnableColorChangeLegacyCB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableColorChangeLegacyCB.Name = "EnableColorChangeLegacyCB";
            this.EnableColorChangeLegacyCB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableColorChangeLegacyCB.Size = new System.Drawing.Size(102, 32);
            this.EnableColorChangeLegacyCB.TabIndex = 8;
            this.EnableColorChangeLegacyCB.Text = "Enable color change";
            this.EnableColorChangeLegacyCB.UseVisualStyleBackColor = true;
            this.EnableColorChangeLegacyCB.CheckedChanged += new System.EventHandler(this.EnableColorChangeLegacyCB_CheckedChanged);
            this.EnableColorChangeLegacyCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.EnableColorChangeLegacyCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // EnableBordersDefaultV2CB
            // 
            this.EnableBordersDefaultV2CB.AutoSize = true;
            this.EnableBordersDefaultV2CB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnableBordersDefaultV2CB.Location = new System.Drawing.Point(209, 35);
            this.EnableBordersDefaultV2CB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableBordersDefaultV2CB.Name = "EnableBordersDefaultV2CB";
            this.EnableBordersDefaultV2CB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableBordersDefaultV2CB.Size = new System.Drawing.Size(104, 32);
            this.EnableBordersDefaultV2CB.TabIndex = 9;
            this.EnableBordersDefaultV2CB.Text = "Enable borders";
            this.EnableBordersDefaultV2CB.UseVisualStyleBackColor = true;
            this.EnableBordersDefaultV2CB.CheckedChanged += new System.EventHandler(this.EnableBordersDefaultV2CB_CheckedChanged);
            this.EnableBordersDefaultV2CB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.EnableBordersDefaultV2CB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // EnableColorChangeDefaultV2CB
            // 
            this.EnableColorChangeDefaultV2CB.AutoSize = true;
            this.EnableColorChangeDefaultV2CB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnableColorChangeDefaultV2CB.Location = new System.Drawing.Point(209, 69);
            this.EnableColorChangeDefaultV2CB.Margin = new System.Windows.Forms.Padding(1);
            this.EnableColorChangeDefaultV2CB.Name = "EnableColorChangeDefaultV2CB";
            this.EnableColorChangeDefaultV2CB.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.EnableColorChangeDefaultV2CB.Size = new System.Drawing.Size(104, 32);
            this.EnableColorChangeDefaultV2CB.TabIndex = 10;
            this.EnableColorChangeDefaultV2CB.Text = "Enable color change";
            this.EnableColorChangeDefaultV2CB.UseVisualStyleBackColor = true;
            this.EnableColorChangeDefaultV2CB.CheckedChanged += new System.EventHandler(this.EnableColorChangeDefaultV2CB_CheckedChanged);
            this.EnableColorChangeDefaultV2CB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.EnableColorChangeDefaultV2CB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // totalProgressBar
            // 
            this.totalProgressBar.Location = new System.Drawing.Point(16, 486);
            this.totalProgressBar.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.totalProgressBar.Name = "totalProgressBar";
            this.totalProgressBar.Size = new System.Drawing.Size(478, 14);
            this.totalProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.totalProgressBar.TabIndex = 34;
            // 
            // viewAppUpdates
            // 
            this.viewAppUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewAppUpdates.Location = new System.Drawing.Point(1, 1);
            this.viewAppUpdates.Margin = new System.Windows.Forms.Padding(1);
            this.viewAppUpdates.Name = "viewAppUpdates";
            this.viewAppUpdates.Size = new System.Drawing.Size(239, 27);
            this.viewAppUpdates.TabIndex = 36;
            this.viewAppUpdates.Text = "View latest application updates";
            this.viewAppUpdates.UseVisualStyleBackColor = true;
            this.viewAppUpdates.Click += new System.EventHandler(this.ViewAppUpdates_Click);
            // 
            // viewDBUpdates
            // 
            this.viewDBUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewDBUpdates.Location = new System.Drawing.Point(1, 30);
            this.viewDBUpdates.Margin = new System.Windows.Forms.Padding(1);
            this.viewDBUpdates.Name = "viewDBUpdates";
            this.viewDBUpdates.Size = new System.Drawing.Size(239, 27);
            this.viewDBUpdates.TabIndex = 37;
            this.viewDBUpdates.Text = "View latest database updates";
            this.viewDBUpdates.UseVisualStyleBackColor = true;
            this.viewDBUpdates.Click += new System.EventHandler(this.ViewDBUpdates_Click);
            // 
            // downloadProgress
            // 
            this.downloadProgress.DetectUrls = false;
            this.downloadProgress.Location = new System.Drawing.Point(16, 416);
            this.downloadProgress.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.ReadOnly = true;
            this.downloadProgress.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.downloadProgress.Size = new System.Drawing.Size(478, 64);
            this.downloadProgress.TabIndex = 29;
            this.downloadProgress.Text = "";
            // 
            // DiagnosticUtilitiesButton
            // 
            this.DiagnosticUtilitiesButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiagnosticUtilitiesButton.Location = new System.Drawing.Point(1, 59);
            this.DiagnosticUtilitiesButton.Margin = new System.Windows.Forms.Padding(1);
            this.DiagnosticUtilitiesButton.Name = "DiagnosticUtilitiesButton";
            this.DiagnosticUtilitiesButton.Size = new System.Drawing.Size(239, 30);
            this.DiagnosticUtilitiesButton.TabIndex = 40;
            this.DiagnosticUtilitiesButton.Text = "Diagnostic Utilities";
            this.DiagnosticUtilitiesButton.UseVisualStyleBackColor = true;
            this.DiagnosticUtilitiesButton.Click += new System.EventHandler(this.DiagnosticUtilitiesButton_Click);
            // 
            // ButtonTable
            // 
            this.ButtonTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonTable.BackColor = System.Drawing.Color.Transparent;
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
            this.ButtonTable.Size = new System.Drawing.Size(482, 90);
            this.ButtonTable.TabIndex = 42;
            // 
            // fontSizeGB
            // 
            this.fontSizeGB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fontSizeGB.BackColor = System.Drawing.Color.Transparent;
            this.fontSizeGB.Controls.Add(this.FontLayoutPanel);
            this.fontSizeGB.Location = new System.Drawing.Point(334, 242);
            this.fontSizeGB.Margin = new System.Windows.Forms.Padding(1);
            this.fontSizeGB.Name = "fontSizeGB";
            this.fontSizeGB.Size = new System.Drawing.Size(160, 125);
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
            this.FontLayoutPanel.Size = new System.Drawing.Size(154, 106);
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
            this.DPI100.Size = new System.Drawing.Size(75, 15);
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
            this.DPIAUTO.Location = new System.Drawing.Point(78, 86);
            this.DPIAUTO.Margin = new System.Windows.Forms.Padding(1);
            this.DPIAUTO.Name = "DPIAUTO";
            this.DPIAUTO.Size = new System.Drawing.Size(75, 19);
            this.DPIAUTO.TabIndex = 6;
            this.DPIAUTO.TabStop = true;
            this.DPIAUTO.Text = "AUTO";
            this.DPIAUTO.UseVisualStyleBackColor = true;
            this.DPIAUTO.CheckedChanged += new System.EventHandler(this.DPIAUTO_CheckedChanged);
            this.DPIAUTO.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // DPI275
            // 
            this.DPI275.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DPI275.Location = new System.Drawing.Point(78, 69);
            this.DPI275.Margin = new System.Windows.Forms.Padding(1);
            this.DPI275.Name = "DPI275";
            this.DPI275.Size = new System.Drawing.Size(75, 15);
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
            this.DPI125.Location = new System.Drawing.Point(78, 18);
            this.DPI125.Margin = new System.Windows.Forms.Padding(1);
            this.DPI125.Name = "DPI125";
            this.DPI125.Size = new System.Drawing.Size(75, 15);
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
            this.DPI225.Location = new System.Drawing.Point(78, 52);
            this.DPI225.Margin = new System.Windows.Forms.Padding(1);
            this.DPI225.Name = "DPI225";
            this.DPI225.Size = new System.Drawing.Size(75, 15);
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
            this.fontSize100.Size = new System.Drawing.Size(75, 15);
            this.fontSize100.TabIndex = 0;
            this.fontSize100.TabStop = true;
            this.fontSize100.Text = "Font 1x";
            this.fontSize100.UseVisualStyleBackColor = true;
            this.fontSize100.CheckedChanged += new System.EventHandler(this.FontSize100_CheckedChanged);
            this.fontSize100.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // fontSize275
            // 
            this.fontSize275.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontSize275.Location = new System.Drawing.Point(1, 69);
            this.fontSize275.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize275.Name = "fontSize275";
            this.fontSize275.Size = new System.Drawing.Size(75, 15);
            this.fontSize275.TabIndex = 8;
            this.fontSize275.TabStop = true;
            this.fontSize275.Text = "Font 2.75x";
            this.fontSize275.UseVisualStyleBackColor = true;
            this.fontSize275.CheckedChanged += new System.EventHandler(this.FontSize275_CheckedChanged);
            this.fontSize275.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // DPI175
            // 
            this.DPI175.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DPI175.Location = new System.Drawing.Point(78, 35);
            this.DPI175.Margin = new System.Windows.Forms.Padding(1);
            this.DPI175.Name = "DPI175";
            this.DPI175.Size = new System.Drawing.Size(75, 15);
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
            this.fontSize125.Location = new System.Drawing.Point(1, 18);
            this.fontSize125.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize125.Name = "fontSize125";
            this.fontSize125.Size = new System.Drawing.Size(75, 15);
            this.fontSize125.TabIndex = 1;
            this.fontSize125.TabStop = true;
            this.fontSize125.Text = "Font 1.25x";
            this.fontSize125.UseVisualStyleBackColor = true;
            this.fontSize125.CheckedChanged += new System.EventHandler(this.FontSize125_CheckedChanged);
            this.fontSize125.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // fontSize225
            // 
            this.fontSize225.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontSize225.Location = new System.Drawing.Point(1, 52);
            this.fontSize225.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize225.Name = "fontSize225";
            this.fontSize225.Size = new System.Drawing.Size(75, 15);
            this.fontSize225.TabIndex = 7;
            this.fontSize225.TabStop = true;
            this.fontSize225.Text = "Font 2.25x";
            this.fontSize225.UseVisualStyleBackColor = true;
            this.fontSize225.CheckedChanged += new System.EventHandler(this.FontSize225_CheckedChanged);
            this.fontSize225.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // fontSize175
            // 
            this.fontSize175.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontSize175.Location = new System.Drawing.Point(1, 35);
            this.fontSize175.Margin = new System.Windows.Forms.Padding(1);
            this.fontSize175.Name = "fontSize175";
            this.fontSize175.Size = new System.Drawing.Size(75, 15);
            this.fontSize175.TabIndex = 2;
            this.fontSize175.TabStop = true;
            this.fontSize175.Text = "Font 1.75x";
            this.fontSize175.UseVisualStyleBackColor = true;
            this.fontSize175.CheckedChanged += new System.EventHandler(this.FontSize175_CheckedChanged);
            this.fontSize175.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            // 
            // RelhaxMenuStrip
            // 
            this.RelhaxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemRestore,
            this.MenuItemCheckUpdates,
            this.MenuItemAppClose});
            this.RelhaxMenuStrip.Name = "RelhaxMenuStrip";
            this.RelhaxMenuStrip.Size = new System.Drawing.Size(172, 70);
            this.RelhaxMenuStrip.Text = "Test";
            // 
            // MenuItemRestore
            // 
            this.MenuItemRestore.Name = "MenuItemRestore";
            this.MenuItemRestore.Size = new System.Drawing.Size(171, 22);
            this.MenuItemRestore.Text = "Restore";
            this.MenuItemRestore.Click += new System.EventHandler(this.MenuItemRestore_Click);
            // 
            // MenuItemCheckUpdates
            // 
            this.MenuItemCheckUpdates.Name = "MenuItemCheckUpdates";
            this.MenuItemCheckUpdates.Size = new System.Drawing.Size(171, 22);
            this.MenuItemCheckUpdates.Text = "Check for Updates";
            this.MenuItemCheckUpdates.Click += new System.EventHandler(this.MenuItemCheckUpdates_Click);
            // 
            // MenuItemAppClose
            // 
            this.MenuItemAppClose.Name = "MenuItemAppClose";
            this.MenuItemAppClose.Size = new System.Drawing.Size(171, 22);
            this.MenuItemAppClose.Text = "Close";
            this.MenuItemAppClose.Click += new System.EventHandler(this.MenuItemAppClose_Click);
            // 
            // RMIcon
            // 
            this.RMIcon.ContextMenuStrip = this.RelhaxMenuStrip;
            this.RMIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("RMIcon.Icon")));
            this.RMIcon.Text = "Relhax Modpack";
            this.RMIcon.Visible = true;
            this.RMIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RMIcon_MouseClick);
            // 
            // DonateButton
            // 
            this.DonateButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.donation;
            this.DonateButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.DonateButton.Location = new System.Drawing.Point(456, 546);
            this.DonateButton.Name = "DonateButton";
            this.DonateButton.Size = new System.Drawing.Size(38, 38);
            this.DonateButton.TabIndex = 45;
            this.DonateButton.UseVisualStyleBackColor = true;
            this.DonateButton.Click += new System.EventHandler(this.DonateLabel_Click);
            this.DonateButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.DonateButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // Forms_ENG_NAButton
            // 
            this.Forms_ENG_NAButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.flag_us;
            this.Forms_ENG_NAButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Forms_ENG_NAButton.Location = new System.Drawing.Point(101, 546);
            this.Forms_ENG_NAButton.Name = "Forms_ENG_NAButton";
            this.Forms_ENG_NAButton.Size = new System.Drawing.Size(38, 38);
            this.Forms_ENG_NAButton.TabIndex = 46;
            this.Forms_ENG_NAButton.UseVisualStyleBackColor = true;
            this.Forms_ENG_NAButton.Click += new System.EventHandler(this.FormPageNALink_Click);
            this.Forms_ENG_NAButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.Forms_ENG_NAButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // FormsENG_EUButton
            // 
            this.FormsENG_EUButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.flag_gb;
            this.FormsENG_EUButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FormsENG_EUButton.Location = new System.Drawing.Point(13, 546);
            this.FormsENG_EUButton.Name = "FormsENG_EUButton";
            this.FormsENG_EUButton.Size = new System.Drawing.Size(38, 38);
            this.FormsENG_EUButton.TabIndex = 47;
            this.FormsENG_EUButton.UseVisualStyleBackColor = true;
            this.FormsENG_EUButton.Click += new System.EventHandler(this.FormPageEULink_Click);
            this.FormsENG_EUButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.FormsENG_EUButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // FormsENG_GERButton
            // 
            this.FormsENG_GERButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.flag_de;
            this.FormsENG_GERButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FormsENG_GERButton.Location = new System.Drawing.Point(57, 546);
            this.FormsENG_GERButton.Name = "FormsENG_GERButton";
            this.FormsENG_GERButton.Size = new System.Drawing.Size(38, 38);
            this.FormsENG_GERButton.TabIndex = 48;
            this.FormsENG_GERButton.UseVisualStyleBackColor = true;
            this.FormsENG_GERButton.Click += new System.EventHandler(this.FormPageEUGERLink_Click);
            this.FormsENG_GERButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.FormsENG_GERButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // FacebookButton
            // 
            this.FacebookButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.facebook_brand;
            this.FacebookButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FacebookButton.Location = new System.Drawing.Point(171, 546);
            this.FacebookButton.Name = "FacebookButton";
            this.FacebookButton.Size = new System.Drawing.Size(38, 38);
            this.FacebookButton.TabIndex = 49;
            this.FacebookButton.UseVisualStyleBackColor = true;
            this.FacebookButton.Click += new System.EventHandler(this.ViewFacebookLink_Click);
            this.FacebookButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.FacebookButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // SendEmailButton
            // 
            this.SendEmailButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.EMail;
            this.SendEmailButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.SendEmailButton.Location = new System.Drawing.Point(412, 546);
            this.SendEmailButton.Name = "SendEmailButton";
            this.SendEmailButton.Size = new System.Drawing.Size(38, 38);
            this.SendEmailButton.TabIndex = 50;
            this.SendEmailButton.UseVisualStyleBackColor = true;
            this.SendEmailButton.Click += new System.EventHandler(this.SendEmailLink_Click);
            this.SendEmailButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.SendEmailButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // DiscordButton
            // 
            this.DiscordButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.discord_brand;
            this.DiscordButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.DiscordButton.Location = new System.Drawing.Point(259, 546);
            this.DiscordButton.Name = "DiscordButton";
            this.DiscordButton.Size = new System.Drawing.Size(38, 38);
            this.DiscordButton.TabIndex = 51;
            this.DiscordButton.UseVisualStyleBackColor = true;
            this.DiscordButton.Click += new System.EventHandler(this.DiscordServerLink_Click);
            this.DiscordButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.DiscordButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // TwitterButton
            // 
            this.TwitterButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.twitter_brand;
            this.TwitterButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.TwitterButton.Location = new System.Drawing.Point(215, 546);
            this.TwitterButton.Name = "TwitterButton";
            this.TwitterButton.Size = new System.Drawing.Size(38, 38);
            this.TwitterButton.TabIndex = 53;
            this.TwitterButton.UseVisualStyleBackColor = true;
            this.TwitterButton.Click += new System.EventHandler(this.ViewTwitterLink_Click);
            this.TwitterButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.TwitterButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // FindBugAddModButton
            // 
            this.FindBugAddModButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.report;
            this.FindBugAddModButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FindBugAddModButton.Location = new System.Drawing.Point(368, 546);
            this.FindBugAddModButton.Name = "FindBugAddModButton";
            this.FindBugAddModButton.Size = new System.Drawing.Size(38, 38);
            this.FindBugAddModButton.TabIndex = 54;
            this.FindBugAddModButton.UseVisualStyleBackColor = true;
            this.FindBugAddModButton.Click += new System.EventHandler(this.FindBugAddModLabel_Click);
            this.FindBugAddModButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.FindBugAddModButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // HomepageButton
            // 
            this.HomepageButton.BackgroundImage = global::RelhaxModpack.Properties.Resources.Home;
            this.HomepageButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.HomepageButton.Location = new System.Drawing.Point(324, 546);
            this.HomepageButton.Name = "HomepageButton";
            this.HomepageButton.Size = new System.Drawing.Size(38, 38);
            this.HomepageButton.TabIndex = 55;
            this.HomepageButton.UseVisualStyleBackColor = true;
            this.HomepageButton.Click += new System.EventHandler(this.VisitWebsiteLink_Click);
            this.HomepageButton.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.HomepageButton.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.29167F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.70833F));
            this.tableLayoutPanel1.Controls.Add(this.ApplicationVersionLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.DatabaseVersionLabel, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 590);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(481, 20);
            this.tableLayoutPanel1.TabIndex = 56;
            // 
            // ApplicationVersionLabel
            // 
            this.ApplicationVersionLabel.AutoSize = true;
            this.ApplicationVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ApplicationVersionLabel.Location = new System.Drawing.Point(4, 1);
            this.ApplicationVersionLabel.Name = "ApplicationVersionLabel";
            this.ApplicationVersionLabel.Size = new System.Drawing.Size(196, 18);
            this.ApplicationVersionLabel.TabIndex = 0;
            this.ApplicationVersionLabel.Text = "Application v{version}";
            this.ApplicationVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DatabaseVersionLabel
            // 
            this.DatabaseVersionLabel.AutoSize = true;
            this.DatabaseVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DatabaseVersionLabel.Location = new System.Drawing.Point(207, 1);
            this.DatabaseVersionLabel.Name = "DatabaseVersionLabel";
            this.DatabaseVersionLabel.Size = new System.Drawing.Size(270, 18);
            this.DatabaseVersionLabel.TabIndex = 1;
            this.DatabaseVersionLabel.Text = "Database Version {version}";
            this.DatabaseVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // backupModsSizeLabel
            // 
            this.backupModsSizeLabel.AutoSize = true;
            this.backupModsSizeLabel.Location = new System.Drawing.Point(16, 75);
            this.backupModsSizeLabel.Margin = new System.Windows.Forms.Padding(16, 0, 3, 0);
            this.backupModsSizeLabel.Name = "backupModsSizeLabel";
            this.backupModsSizeLabel.Size = new System.Drawing.Size(35, 13);
            this.backupModsSizeLabel.TabIndex = 40;
            this.backupModsSizeLabel.Text = "label1";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackgroundImage = global::RelhaxModpack.Properties.Resources.WoT_brand___light_grey_2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(504, 621);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.HomepageButton);
            this.Controls.Add(this.FindBugAddModButton);
            this.Controls.Add(this.TwitterButton);
            this.Controls.Add(this.DiscordButton);
            this.Controls.Add(this.SendEmailButton);
            this.Controls.Add(this.FacebookButton);
            this.Controls.Add(this.FormsENG_GERButton);
            this.Controls.Add(this.FormsENG_EUButton);
            this.Controls.Add(this.Forms_ENG_NAButton);
            this.Controls.Add(this.DonateButton);
            this.Controls.Add(this.fontSizeGB);
            this.Controls.Add(this.ErrorCounterLabel);
            this.Controls.Add(this.totalProgressBar);
            this.Controls.Add(this.parrentProgressBar);
            this.Controls.Add(this.downloadProgress);
            this.Controls.Add(this.childProgressBar);
            this.Controls.Add(this.cancelDownloadButton);
            this.Controls.Add(this.SelectionViewGB);
            this.Controls.Add(this.languageSelectionGB);
            this.Controls.Add(this.ButtonTable);
            this.Controls.Add(this.settingsGroupBox);
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
            this.ButtonTable.ResumeLayout(false);
            this.fontSizeGB.ResumeLayout(false);
            this.FontLayoutPanel.ResumeLayout(false);
            this.RelhaxMenuStrip.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ProgressBar childProgressBar;
        private System.Windows.Forms.OpenFileDialog FindWotExe;
        private System.Windows.Forms.ProgressBar parrentProgressBar;
        private System.Windows.Forms.Button installRelhaxMod;
        private System.Windows.Forms.Button uninstallRelhaxMod;
        private System.Windows.Forms.CheckBox backupModsCheckBox;
        private System.Windows.Forms.GroupBox settingsGroupBox;
        private System.Windows.Forms.Button cancelDownloadButton;
        private System.Windows.Forms.CheckBox saveUserDataCB;
        private System.Windows.Forms.Timer DownloadTimer;
        private System.Windows.Forms.GroupBox languageSelectionGB;
        private System.Windows.Forms.GroupBox SelectionViewGB;
        private System.Windows.Forms.RadioButton SelectionLegacy;
        private System.Windows.Forms.RadioButton SelectionDefault;
        private System.Windows.Forms.ProgressBar totalProgressBar;
        private System.Windows.Forms.Button viewAppUpdates;
        private System.Windows.Forms.Button viewDBUpdates;
        private System.Windows.Forms.CheckBox clearLogFilesCB;
        public System.Windows.Forms.Label ErrorCounterLabel;
        private System.Windows.Forms.ComboBox LanguageComboBox;
        private System.Windows.Forms.Button DiagnosticUtilitiesButton;
        private System.Windows.Forms.CheckBox SuperExtractionCB;
        private System.Windows.Forms.TableLayoutPanel ButtonTable;
        private System.Windows.Forms.TableLayoutPanel SettingsTable;
        private System.Windows.Forms.TableLayoutPanel SelectionLayout;
        private System.Windows.Forms.RichTextBox downloadProgress;
        private System.Windows.Forms.FolderBrowserDialog ExportModeBrowserDialog;
        private System.Windows.Forms.RadioButton SelectionDefaultV2;
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
        private System.Windows.Forms.CheckBox EnableBordersDefaultV2CB;
        private System.Windows.Forms.CheckBox EnableColorChangeDefaultV2CB;
        private System.Windows.Forms.ContextMenuStrip RelhaxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRestore;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCheckUpdates;
        private System.Windows.Forms.ToolStripMenuItem MenuItemAppClose;
        private System.Windows.Forms.NotifyIcon RMIcon;
        private System.Windows.Forms.Button DonateButton;
        private System.Windows.Forms.Button Forms_ENG_NAButton;
        private System.Windows.Forms.Button FormsENG_EUButton;
        private System.Windows.Forms.Button FormsENG_GERButton;
        private System.Windows.Forms.Button FacebookButton;
        private System.Windows.Forms.Button SendEmailButton;
        private System.Windows.Forms.Button DiscordButton;
        private System.Windows.Forms.Button TwitterButton;
        private System.Windows.Forms.Button FindBugAddModButton;
        private System.Windows.Forms.Button HomepageButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label ApplicationVersionLabel;
        private System.Windows.Forms.Label DatabaseVersionLabel;
        private System.Windows.Forms.Label backupModsSizeLabel;
    }
}

