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
            this.formPageLink = new System.Windows.Forms.LinkLabel();
            this.parrentProgressBar = new System.Windows.Forms.ProgressBar();
            this.installRelhaxMod = new System.Windows.Forms.Button();
            this.uninstallRelhaxMod = new System.Windows.Forms.Button();
            this.backupModsCheckBox = new System.Windows.Forms.CheckBox();
            this.settingsGroupBox = new System.Windows.Forms.GroupBox();
            this.SettingsTable = new System.Windows.Forms.TableLayoutPanel();
            this.cancerFontCB = new System.Windows.Forms.CheckBox();
            this.saveUserDataCB = new System.Windows.Forms.CheckBox();
            this.notifyIfSameDatabaseCB = new System.Windows.Forms.CheckBox();
            this.darkUICB = new System.Windows.Forms.CheckBox();
            this.saveLastInstallCB = new System.Windows.Forms.CheckBox();
            this.SuperExtractionCB = new System.Windows.Forms.CheckBox();
            this.clearLogFilesCB = new System.Windows.Forms.CheckBox();
            this.languageSelectionGB = new System.Windows.Forms.GroupBox();
            this.LanguageComboBox = new System.Windows.Forms.ComboBox();
            this.findBugAddModLabel = new System.Windows.Forms.LinkLabel();
            this.cancelDownloadButton = new System.Windows.Forms.Button();
            this.DownloadTimer = new System.Windows.Forms.Timer(this.components);
            this.viewTypeGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.selectionDefault = new System.Windows.Forms.RadioButton();
            this.expandNodesDefault = new System.Windows.Forms.CheckBox();
            this.selectionLegacy = new System.Windows.Forms.RadioButton();
            this.selectionLegacyV2 = new System.Windows.Forms.RadioButton();
            this.expandNodesDefault2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.donateLabel = new System.Windows.Forms.LinkLabel();
            this.totalProgressBar = new System.Windows.Forms.ProgressBar();
            this.DiscordServerLink = new System.Windows.Forms.LinkLabel();
            this.viewAppUpdates = new System.Windows.Forms.Button();
            this.viewDBUpdates = new System.Windows.Forms.Button();
            this.ErrorCounterLabel = new System.Windows.Forms.Label();
            this.InfoTable = new System.Windows.Forms.TableLayoutPanel();
            this.DatabaseVersionLabel = new System.Windows.Forms.Label();
            this.ApplicationVersionLabel = new System.Windows.Forms.Label();
            this.downloadProgress = new System.Windows.Forms.RichTextBox();
            this.DiagnosticUtilitiesButton = new System.Windows.Forms.Button();
            this.ButtonTable = new System.Windows.Forms.TableLayoutPanel();
            this.ExportModeBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.fontSizeGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
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
            this.ShowAdvancedSettingsLink = new System.Windows.Forms.LinkLabel();
            this.settingsGroupBox.SuspendLayout();
            this.SettingsTable.SuspendLayout();
            this.languageSelectionGB.SuspendLayout();
            this.viewTypeGB.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.InfoTable.SuspendLayout();
            this.ButtonTable.SuspendLayout();
            this.fontSizeGB.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // childProgressBar
            // 
            this.InfoTable.SetColumnSpan(this.childProgressBar, 2);
            this.childProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.childProgressBar.Location = new System.Drawing.Point(3, 113);
            this.childProgressBar.Name = "childProgressBar";
            this.childProgressBar.Size = new System.Drawing.Size(463, 14);
            this.childProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.childProgressBar.TabIndex = 11;
            // 
            // FindWotExe
            // 
            this.FindWotExe.Filter = "WorldOfTanks.exe|WorldOfTanks.exe";
            this.FindWotExe.Title = "Find WorldOfTanks.exe";
            // 
            // formPageLink
            // 
            this.formPageLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formPageLink.Location = new System.Drawing.Point(3, 150);
            this.formPageLink.Name = "formPageLink";
            this.formPageLink.Size = new System.Drawing.Size(229, 20);
            this.formPageLink.TabIndex = 16;
            this.formPageLink.TabStop = true;
            this.formPageLink.Text = "View Modpack Form Page";
            this.formPageLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.formPageLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.formPageLink_LinkClicked);
            // 
            // parrentProgressBar
            // 
            this.InfoTable.SetColumnSpan(this.parrentProgressBar, 2);
            this.parrentProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parrentProgressBar.Location = new System.Drawing.Point(3, 93);
            this.parrentProgressBar.Name = "parrentProgressBar";
            this.parrentProgressBar.Size = new System.Drawing.Size(463, 14);
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
            this.installRelhaxMod.Click += new System.EventHandler(this.installRelhaxMod_Click);
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
            this.backupModsCheckBox.Size = new System.Drawing.Size(229, 18);
            this.backupModsCheckBox.TabIndex = 24;
            this.backupModsCheckBox.Text = "Backup current mods folder";
            this.backupModsCheckBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.backupModsCheckBox.UseVisualStyleBackColor = true;
            this.backupModsCheckBox.CheckedChanged += new System.EventHandler(this.backupModsCheckBox_CheckedChanged);
            this.backupModsCheckBox.MouseEnter += new System.EventHandler(this.backupModsCheckBox_MouseEnter);
            this.backupModsCheckBox.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Controls.Add(this.SettingsTable);
            this.settingsGroupBox.Location = new System.Drawing.Point(12, 103);
            this.settingsGroupBox.MaximumSize = new System.Drawing.Size(502, 210);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(468, 165);
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
            this.SettingsTable.Controls.Add(this.cancerFontCB, 1, 3);
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
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.SettingsTable.Size = new System.Drawing.Size(462, 146);
            this.SettingsTable.TabIndex = 43;
            // 
            // cancerFontCB
            // 
            this.cancerFontCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cancerFontCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancerFontCB.Location = new System.Drawing.Point(232, 61);
            this.cancerFontCB.Margin = new System.Windows.Forms.Padding(1);
            this.cancerFontCB.Name = "cancerFontCB";
            this.cancerFontCB.Size = new System.Drawing.Size(229, 18);
            this.cancerFontCB.TabIndex = 38;
            this.cancerFontCB.Text = "Cancer font";
            this.cancerFontCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cancerFontCB.UseVisualStyleBackColor = true;
            this.cancerFontCB.CheckedChanged += new System.EventHandler(this.cancerFontCB_CheckedChanged);
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
            this.saveUserDataCB.MouseEnter += new System.EventHandler(this.saveUserDataCB_MouseEnter);
            this.saveUserDataCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // notifyIfSameDatabaseCB
            // 
            this.notifyIfSameDatabaseCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.notifyIfSameDatabaseCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notifyIfSameDatabaseCB.Location = new System.Drawing.Point(232, 41);
            this.notifyIfSameDatabaseCB.Margin = new System.Windows.Forms.Padding(1);
            this.notifyIfSameDatabaseCB.Name = "notifyIfSameDatabaseCB";
            this.notifyIfSameDatabaseCB.Size = new System.Drawing.Size(229, 18);
            this.notifyIfSameDatabaseCB.TabIndex = 33;
            this.notifyIfSameDatabaseCB.Text = "Inform if no new database available";
            this.notifyIfSameDatabaseCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.notifyIfSameDatabaseCB.UseVisualStyleBackColor = true;
            this.notifyIfSameDatabaseCB.CheckedChanged += new System.EventHandler(this.notifyIfSameDatabaseCB_CheckedChanged);
            this.notifyIfSameDatabaseCB.MouseEnter += new System.EventHandler(this.notifyIfSameDatabaseCB_MouseEnter);
            this.notifyIfSameDatabaseCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
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
            this.darkUICB.MouseEnter += new System.EventHandler(this.darkUICB_MouseEnter);
            this.darkUICB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
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
            this.saveLastInstallCB.MouseEnter += new System.EventHandler(this.saveLastInstallCB_MouseEnter);
            this.saveLastInstallCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // SuperExtractionCB
            // 
            this.SuperExtractionCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.SuperExtractionCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SuperExtractionCB.Location = new System.Drawing.Point(1, 61);
            this.SuperExtractionCB.Margin = new System.Windows.Forms.Padding(1);
            this.SuperExtractionCB.Name = "SuperExtractionCB";
            this.SuperExtractionCB.Size = new System.Drawing.Size(229, 18);
            this.SuperExtractionCB.TabIndex = 37;
            this.SuperExtractionCB.Text = "Super extraction mode (Experimental)";
            this.SuperExtractionCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.SuperExtractionCB.UseVisualStyleBackColor = true;
            this.SuperExtractionCB.CheckedChanged += new System.EventHandler(this.SuperExtractionCB_CheckedChanged);
            this.SuperExtractionCB.MouseEnter += new System.EventHandler(this.SuperExtractionCB_MouseEnter);
            this.SuperExtractionCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
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
            this.clearLogFilesCB.MouseEnter += new System.EventHandler(this.clearLogFilesCB_MouseEnter);
            this.clearLogFilesCB.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // languageSelectionGB
            // 
            this.languageSelectionGB.Controls.Add(this.LanguageComboBox);
            this.languageSelectionGB.Location = new System.Drawing.Point(319, 419);
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
            // 
            // findBugAddModLabel
            // 
            this.findBugAddModLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.findBugAddModLabel.Location = new System.Drawing.Point(3, 130);
            this.findBugAddModLabel.Name = "findBugAddModLabel";
            this.findBugAddModLabel.Size = new System.Drawing.Size(229, 20);
            this.findBugAddModLabel.TabIndex = 27;
            this.findBugAddModLabel.TabStop = true;
            this.findBugAddModLabel.Text = "Find a bug? Want a mod added?";
            this.findBugAddModLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.findBugAddModLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.findBugAddModLabel_LinkClicked);
            // 
            // cancelDownloadButton
            // 
            this.cancelDownloadButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.cancelDownloadButton.Enabled = false;
            this.cancelDownloadButton.Location = new System.Drawing.Point(302, 133);
            this.cancelDownloadButton.Name = "cancelDownloadButton";
            this.InfoTable.SetRowSpan(this.cancelDownloadButton, 3);
            this.cancelDownloadButton.Size = new System.Drawing.Size(164, 54);
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
            // viewTypeGB
            // 
            this.viewTypeGB.Controls.Add(this.tableLayoutPanel5);
            this.viewTypeGB.Location = new System.Drawing.Point(12, 273);
            this.viewTypeGB.Margin = new System.Windows.Forms.Padding(1);
            this.viewTypeGB.Name = "viewTypeGB";
            this.viewTypeGB.Size = new System.Drawing.Size(305, 192);
            this.viewTypeGB.TabIndex = 31;
            this.viewTypeGB.TabStop = false;
            this.viewTypeGB.Text = "Selection View";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 3;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 161F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Controls.Add(this.selectionDefault, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.expandNodesDefault, 1, 3);
            this.tableLayoutPanel5.Controls.Add(this.selectionLegacy, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.selectionLegacyV2, 2, 0);
            this.tableLayoutPanel5.Controls.Add(this.expandNodesDefault2, 2, 1);
            this.tableLayoutPanel5.Controls.Add(this.checkBox1, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.checkBox2, 1, 1);
            this.tableLayoutPanel5.Controls.Add(this.checkBox3, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.checkBox4, 1, 2);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 4;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(299, 173);
            this.tableLayoutPanel5.TabIndex = 5;
            // 
            // selectionDefault
            // 
            this.selectionDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectionDefault.Location = new System.Drawing.Point(1, 1);
            this.selectionDefault.Margin = new System.Windows.Forms.Padding(1);
            this.selectionDefault.Name = "selectionDefault";
            this.selectionDefault.Size = new System.Drawing.Size(128, 41);
            this.selectionDefault.TabIndex = 0;
            this.selectionDefault.TabStop = true;
            this.selectionDefault.Text = "Default";
            this.selectionDefault.UseVisualStyleBackColor = true;
            this.selectionDefault.CheckedChanged += new System.EventHandler(this.selectionDefault_CheckedChanged);
            this.selectionDefault.MouseEnter += new System.EventHandler(this.selectionView_MouseEnter);
            this.selectionDefault.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // expandNodesDefault
            // 
            this.expandNodesDefault.AutoSize = true;
            this.expandNodesDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expandNodesDefault.Location = new System.Drawing.Point(131, 130);
            this.expandNodesDefault.Margin = new System.Windows.Forms.Padding(1);
            this.expandNodesDefault.Name = "expandNodesDefault";
            this.expandNodesDefault.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.expandNodesDefault.Size = new System.Drawing.Size(159, 42);
            this.expandNodesDefault.TabIndex = 2;
            this.expandNodesDefault.Text = "Expand all";
            this.expandNodesDefault.UseVisualStyleBackColor = true;
            this.expandNodesDefault.CheckedChanged += new System.EventHandler(this.expandNodesDefault_CheckedChanged);
            this.expandNodesDefault.MouseEnter += new System.EventHandler(this.expandNodesDefault_MouseEnter);
            this.expandNodesDefault.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // selectionLegacy
            // 
            this.selectionLegacy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectionLegacy.Location = new System.Drawing.Point(131, 1);
            this.selectionLegacy.Margin = new System.Windows.Forms.Padding(1);
            this.selectionLegacy.Name = "selectionLegacy";
            this.selectionLegacy.Size = new System.Drawing.Size(159, 41);
            this.selectionLegacy.TabIndex = 1;
            this.selectionLegacy.TabStop = true;
            this.selectionLegacy.Text = "Legacy";
            this.selectionLegacy.UseVisualStyleBackColor = true;
            this.selectionLegacy.CheckedChanged += new System.EventHandler(this.selectionLegacy_CheckedChanged);
            this.selectionLegacy.MouseEnter += new System.EventHandler(this.selectionView_MouseEnter);
            this.selectionLegacy.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // selectionLegacyV2
            // 
            this.selectionLegacyV2.AutoSize = true;
            this.selectionLegacyV2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectionLegacyV2.Location = new System.Drawing.Point(292, 1);
            this.selectionLegacyV2.Margin = new System.Windows.Forms.Padding(1);
            this.selectionLegacyV2.Name = "selectionLegacyV2";
            this.selectionLegacyV2.Size = new System.Drawing.Size(6, 41);
            this.selectionLegacyV2.TabIndex = 3;
            this.selectionLegacyV2.TabStop = true;
            this.selectionLegacyV2.Text = "Legacy V2";
            this.selectionLegacyV2.UseVisualStyleBackColor = true;
            this.selectionLegacyV2.Visible = false;
            this.selectionLegacyV2.CheckedChanged += new System.EventHandler(this.selectionLegacyV2_CheckedChanged);
            this.selectionLegacyV2.MouseEnter += new System.EventHandler(this.selectionView_MouseEnter);
            this.selectionLegacyV2.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // expandNodesDefault2
            // 
            this.expandNodesDefault2.AutoSize = true;
            this.expandNodesDefault2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expandNodesDefault2.Location = new System.Drawing.Point(292, 44);
            this.expandNodesDefault2.Margin = new System.Windows.Forms.Padding(1);
            this.expandNodesDefault2.Name = "expandNodesDefault2";
            this.expandNodesDefault2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.expandNodesDefault2.Size = new System.Drawing.Size(6, 41);
            this.expandNodesDefault2.TabIndex = 4;
            this.expandNodesDefault2.Text = "Expand all";
            this.expandNodesDefault2.UseVisualStyleBackColor = true;
            this.expandNodesDefault2.Visible = false;
            this.expandNodesDefault2.CheckedChanged += new System.EventHandler(this.expandNodesDefault2_CheckedChanged);
            this.expandNodesDefault2.MouseEnter += new System.EventHandler(this.expandNodesDefault_MouseEnter);
            this.expandNodesDefault2.MouseLeave += new System.EventHandler(this.generic_MouseLeave);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox1.Location = new System.Drawing.Point(1, 44);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(1);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.checkBox1.Size = new System.Drawing.Size(128, 41);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox2.Location = new System.Drawing.Point(131, 44);
            this.checkBox2.Margin = new System.Windows.Forms.Padding(1);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.checkBox2.Size = new System.Drawing.Size(159, 41);
            this.checkBox2.TabIndex = 6;
            this.checkBox2.Text = "checkBox2";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox3.Location = new System.Drawing.Point(1, 87);
            this.checkBox3.Margin = new System.Windows.Forms.Padding(1);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.checkBox3.Size = new System.Drawing.Size(128, 41);
            this.checkBox3.TabIndex = 7;
            this.checkBox3.Text = "checkBox3";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox4.Location = new System.Drawing.Point(131, 87);
            this.checkBox4.Margin = new System.Windows.Forms.Padding(1);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.checkBox4.Size = new System.Drawing.Size(159, 41);
            this.checkBox4.TabIndex = 8;
            this.checkBox4.Text = "checkBox4";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // donateLabel
            // 
            this.donateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.donateLabel.Location = new System.Drawing.Point(3, 170);
            this.donateLabel.Name = "donateLabel";
            this.donateLabel.Size = new System.Drawing.Size(229, 20);
            this.donateLabel.TabIndex = 32;
            this.donateLabel.TabStop = true;
            this.donateLabel.Text = "Donation for further development";
            this.donateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.donateLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.donateLabel_LinkClicked);
            // 
            // totalProgressBar
            // 
            this.InfoTable.SetColumnSpan(this.totalProgressBar, 2);
            this.totalProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.totalProgressBar.Location = new System.Drawing.Point(3, 73);
            this.totalProgressBar.Name = "totalProgressBar";
            this.totalProgressBar.Size = new System.Drawing.Size(463, 14);
            this.totalProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.totalProgressBar.TabIndex = 34;
            // 
            // DiscordServerLink
            // 
            this.DiscordServerLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiscordServerLink.Location = new System.Drawing.Point(3, 190);
            this.DiscordServerLink.Name = "DiscordServerLink";
            this.DiscordServerLink.Size = new System.Drawing.Size(229, 20);
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
            this.ErrorCounterLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorCounterLabel.Location = new System.Drawing.Point(238, 190);
            this.ErrorCounterLabel.Name = "ErrorCounterLabel";
            this.ErrorCounterLabel.Size = new System.Drawing.Size(228, 20);
            this.ErrorCounterLabel.TabIndex = 38;
            this.ErrorCounterLabel.Text = "Error counter: 0";
            this.ErrorCounterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ErrorCounterLabel.Visible = false;
            // 
            // InfoTable
            // 
            this.InfoTable.ColumnCount = 2;
            this.InfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.18182F));
            this.InfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.81818F));
            this.InfoTable.Controls.Add(this.DatabaseVersionLabel, 1, 8);
            this.InfoTable.Controls.Add(this.ApplicationVersionLabel, 0, 8);
            this.InfoTable.Controls.Add(this.downloadProgress, 0, 0);
            this.InfoTable.Controls.Add(this.ErrorCounterLabel, 1, 7);
            this.InfoTable.Controls.Add(this.totalProgressBar, 0, 1);
            this.InfoTable.Controls.Add(this.parrentProgressBar, 0, 2);
            this.InfoTable.Controls.Add(this.childProgressBar, 0, 3);
            this.InfoTable.Controls.Add(this.findBugAddModLabel, 0, 4);
            this.InfoTable.Controls.Add(this.formPageLink, 0, 5);
            this.InfoTable.Controls.Add(this.donateLabel, 0, 6);
            this.InfoTable.Controls.Add(this.DiscordServerLink, 0, 7);
            this.InfoTable.Controls.Add(this.cancelDownloadButton, 1, 4);
            this.InfoTable.Location = new System.Drawing.Point(11, 465);
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
            this.InfoTable.Size = new System.Drawing.Size(469, 233);
            this.InfoTable.TabIndex = 39;
            // 
            // DatabaseVersionLabel
            // 
            this.DatabaseVersionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DatabaseVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DatabaseVersionLabel.Location = new System.Drawing.Point(238, 210);
            this.DatabaseVersionLabel.Name = "DatabaseVersionLabel";
            this.DatabaseVersionLabel.Size = new System.Drawing.Size(228, 23);
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
            this.ApplicationVersionLabel.Size = new System.Drawing.Size(229, 23);
            this.ApplicationVersionLabel.TabIndex = 1;
            this.ApplicationVersionLabel.Text = "Application v{version}";
            this.ApplicationVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // downloadProgress
            // 
            this.InfoTable.SetColumnSpan(this.downloadProgress, 2);
            this.downloadProgress.DetectUrls = false;
            this.downloadProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.downloadProgress.Location = new System.Drawing.Point(3, 3);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.ReadOnly = true;
            this.downloadProgress.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.downloadProgress.Size = new System.Drawing.Size(463, 64);
            this.downloadProgress.TabIndex = 29;
            this.downloadProgress.Text = "";
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
            this.fontSizeGB.Controls.Add(this.tableLayoutPanel6);
            this.fontSizeGB.Location = new System.Drawing.Point(319, 272);
            this.fontSizeGB.Margin = new System.Windows.Forms.Padding(1);
            this.fontSizeGB.Name = "fontSizeGB";
            this.fontSizeGB.Size = new System.Drawing.Size(161, 145);
            this.fontSizeGB.TabIndex = 44;
            this.fontSizeGB.TabStop = false;
            this.fontSizeGB.Text = "Scaling Mode";
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.DPI100, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.DPIAUTO, 1, 5);
            this.tableLayoutPanel6.Controls.Add(this.DPI275, 1, 4);
            this.tableLayoutPanel6.Controls.Add(this.DPI125, 1, 1);
            this.tableLayoutPanel6.Controls.Add(this.DPI225, 1, 3);
            this.tableLayoutPanel6.Controls.Add(this.fontSize100, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.fontSize275, 0, 4);
            this.tableLayoutPanel6.Controls.Add(this.DPI175, 1, 2);
            this.tableLayoutPanel6.Controls.Add(this.fontSize125, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.fontSize225, 0, 3);
            this.tableLayoutPanel6.Controls.Add(this.fontSize175, 0, 2);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 6;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(155, 126);
            this.tableLayoutPanel6.TabIndex = 11;
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
            // 
            // ShowAdvancedSettingsLink
            // 
            this.ShowAdvancedSettingsLink.AutoSize = true;
            this.ShowAdvancedSettingsLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowAdvancedSettingsLink.Location = new System.Drawing.Point(234, 80);
            this.ShowAdvancedSettingsLink.Name = "ShowAdvancedSettingsLink";
            this.ShowAdvancedSettingsLink.Size = new System.Drawing.Size(225, 66);
            this.ShowAdvancedSettingsLink.TabIndex = 39;
            this.ShowAdvancedSettingsLink.TabStop = true;
            this.ShowAdvancedSettingsLink.Text = "linkLabel1";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(490, 708);
            this.Controls.Add(this.fontSizeGB);
            this.Controls.Add(this.InfoTable);
            this.Controls.Add(this.viewTypeGB);
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
            this.viewTypeGB.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.InfoTable.ResumeLayout(false);
            this.ButtonTable.ResumeLayout(false);
            this.fontSizeGB.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ProgressBar childProgressBar;
        private System.Windows.Forms.OpenFileDialog FindWotExe;
        private System.Windows.Forms.LinkLabel formPageLink;
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
        private System.Windows.Forms.GroupBox viewTypeGB;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.RichTextBox downloadProgress;
        private System.Windows.Forms.FolderBrowserDialog ExportModeBrowserDialog;
        private System.Windows.Forms.RadioButton selectionLegacyV2;
        private System.Windows.Forms.CheckBox expandNodesDefault2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox notifyIfSameDatabaseCB;
        private System.Windows.Forms.CheckBox darkUICB;
        private System.Windows.Forms.CheckBox saveLastInstallCB;
        private System.Windows.Forms.GroupBox fontSizeGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
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
        private System.Windows.Forms.CheckBox cancerFontCB;
        private System.Windows.Forms.LinkLabel ShowAdvancedSettingsLink;
    }
}

