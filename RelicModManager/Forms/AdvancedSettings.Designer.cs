namespace RelhaxModpack.Forms
{
    partial class AdvancedSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedSettings));
            this.AdvancedSettingsHeader = new System.Windows.Forms.Label();
            this.AdvancedSettingsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.ShowInstallCompleteWindowCB = new System.Windows.Forms.CheckBox();
            this.cleanInstallCB = new System.Windows.Forms.CheckBox();
            this.ExportModeCB = new System.Windows.Forms.CheckBox();
            this.forceManuel = new System.Windows.Forms.CheckBox();
            this.createShortcutsCB = new System.Windows.Forms.CheckBox();
            this.InstantExtractionCB = new System.Windows.Forms.CheckBox();
            this.clearCacheCB = new System.Windows.Forms.CheckBox();
            this.AdvancedSettingsDescription = new System.Windows.Forms.RichTextBox();
            this.loadingImageGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.thirdGuardsLoadingImageRB = new System.Windows.Forms.RadioButton();
            this.standardImageRB = new System.Windows.Forms.RadioButton();
            this.UninstallModeGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.CleanUninstallModeRB = new System.Windows.Forms.RadioButton();
            this.SmartUninstallModeRB = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.AdvancedSettingsLayout.SuspendLayout();
            this.loadingImageGroupBox.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.UninstallModeGroupBox.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // AdvancedSettingsHeader
            // 
            this.AdvancedSettingsHeader.AutoSize = true;
            this.AdvancedSettingsHeader.BackColor = System.Drawing.SystemColors.ControlDark;
            this.AdvancedSettingsLayout.SetColumnSpan(this.AdvancedSettingsHeader, 2);
            this.AdvancedSettingsHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdvancedSettingsHeader.Location = new System.Drawing.Point(4, 1);
            this.AdvancedSettingsHeader.Name = "AdvancedSettingsHeader";
            this.AdvancedSettingsHeader.Size = new System.Drawing.Size(402, 30);
            this.AdvancedSettingsHeader.TabIndex = 0;
            this.AdvancedSettingsHeader.Text = "Hover over a setting to see its description";
            this.AdvancedSettingsHeader.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // AdvancedSettingsLayout
            // 
            this.AdvancedSettingsLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdvancedSettingsLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.AdvancedSettingsLayout.ColumnCount = 2;
            this.AdvancedSettingsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.AdvancedSettingsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.AdvancedSettingsLayout.Controls.Add(this.ShowInstallCompleteWindowCB, 0, 4);
            this.AdvancedSettingsLayout.Controls.Add(this.cleanInstallCB, 0, 2);
            this.AdvancedSettingsLayout.Controls.Add(this.ExportModeCB, 1, 1);
            this.AdvancedSettingsLayout.Controls.Add(this.forceManuel, 0, 1);
            this.AdvancedSettingsLayout.Controls.Add(this.createShortcutsCB, 1, 3);
            this.AdvancedSettingsLayout.Controls.Add(this.InstantExtractionCB, 0, 3);
            this.AdvancedSettingsLayout.Controls.Add(this.clearCacheCB, 0, 2);
            this.AdvancedSettingsLayout.Controls.Add(this.AdvancedSettingsHeader, 0, 0);
            this.AdvancedSettingsLayout.Location = new System.Drawing.Point(12, 12);
            this.AdvancedSettingsLayout.Name = "AdvancedSettingsLayout";
            this.AdvancedSettingsLayout.RowCount = 5;
            this.AdvancedSettingsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.AdvancedSettingsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.AdvancedSettingsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.AdvancedSettingsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.AdvancedSettingsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.AdvancedSettingsLayout.Size = new System.Drawing.Size(410, 168);
            this.AdvancedSettingsLayout.TabIndex = 1;
            // 
            // ShowInstallCompleteWindowCB
            // 
            this.ShowInstallCompleteWindowCB.AutoSize = true;
            this.ShowInstallCompleteWindowCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ShowInstallCompleteWindowCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ShowInstallCompleteWindowCB.Location = new System.Drawing.Point(2, 126);
            this.ShowInstallCompleteWindowCB.Margin = new System.Windows.Forms.Padding(1);
            this.ShowInstallCompleteWindowCB.Name = "ShowInstallCompleteWindowCB";
            this.ShowInstallCompleteWindowCB.Size = new System.Drawing.Size(201, 40);
            this.ShowInstallCompleteWindowCB.TabIndex = 42;
            this.ShowInstallCompleteWindowCB.Text = "Show Install complete window";
            this.ShowInstallCompleteWindowCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ShowInstallCompleteWindowCB.UseVisualStyleBackColor = true;
            this.ShowInstallCompleteWindowCB.CheckedChanged += new System.EventHandler(this.ShowInstallCompleteWindow_CheckedChanged);
            this.ShowInstallCompleteWindowCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.ShowInstallCompleteWindowCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // cleanInstallCB
            // 
            this.cleanInstallCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cleanInstallCB.Checked = true;
            this.cleanInstallCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cleanInstallCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cleanInstallCB.Location = new System.Drawing.Point(206, 64);
            this.cleanInstallCB.Margin = new System.Windows.Forms.Padding(1);
            this.cleanInstallCB.Name = "cleanInstallCB";
            this.cleanInstallCB.Size = new System.Drawing.Size(202, 28);
            this.cleanInstallCB.TabIndex = 41;
            this.cleanInstallCB.Text = "Clean Installation (Recommended)";
            this.cleanInstallCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cleanInstallCB.UseVisualStyleBackColor = true;
            this.cleanInstallCB.CheckedChanged += new System.EventHandler(this.cleanInstallCB_CheckedChanged);
            this.cleanInstallCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.cleanInstallCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // ExportModeCB
            // 
            this.ExportModeCB.AutoSize = true;
            this.ExportModeCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ExportModeCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportModeCB.Location = new System.Drawing.Point(206, 33);
            this.ExportModeCB.Margin = new System.Windows.Forms.Padding(1);
            this.ExportModeCB.Name = "ExportModeCB";
            this.ExportModeCB.Size = new System.Drawing.Size(202, 28);
            this.ExportModeCB.TabIndex = 40;
            this.ExportModeCB.Text = "Export Mode";
            this.ExportModeCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ExportModeCB.UseVisualStyleBackColor = true;
            this.ExportModeCB.CheckedChanged += new System.EventHandler(this.ExportModeCB_CheckedChanged);
            this.ExportModeCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.ExportModeCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // forceManuel
            // 
            this.forceManuel.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.forceManuel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.forceManuel.Location = new System.Drawing.Point(2, 33);
            this.forceManuel.Margin = new System.Windows.Forms.Padding(1);
            this.forceManuel.Name = "forceManuel";
            this.forceManuel.Size = new System.Drawing.Size(201, 28);
            this.forceManuel.TabIndex = 39;
            this.forceManuel.Text = "Force manual game detection";
            this.forceManuel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.forceManuel.UseVisualStyleBackColor = true;
            this.forceManuel.CheckedChanged += new System.EventHandler(this.forceManuel_CheckedChanged);
            this.forceManuel.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.forceManuel.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // createShortcutsCB
            // 
            this.createShortcutsCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.createShortcutsCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.createShortcutsCB.Location = new System.Drawing.Point(206, 95);
            this.createShortcutsCB.Margin = new System.Windows.Forms.Padding(1);
            this.createShortcutsCB.Name = "createShortcutsCB";
            this.createShortcutsCB.Size = new System.Drawing.Size(202, 28);
            this.createShortcutsCB.TabIndex = 38;
            this.createShortcutsCB.Text = "Create Shortcuts";
            this.createShortcutsCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.createShortcutsCB.UseVisualStyleBackColor = true;
            this.createShortcutsCB.CheckedChanged += new System.EventHandler(this.CreateShortcutsCB_CheckedChanged);
            this.createShortcutsCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.createShortcutsCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // InstantExtractionCB
            // 
            this.InstantExtractionCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.InstantExtractionCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InstantExtractionCB.Location = new System.Drawing.Point(2, 95);
            this.InstantExtractionCB.Margin = new System.Windows.Forms.Padding(1);
            this.InstantExtractionCB.Name = "InstantExtractionCB";
            this.InstantExtractionCB.Size = new System.Drawing.Size(201, 28);
            this.InstantExtractionCB.TabIndex = 37;
            this.InstantExtractionCB.Text = "Instant extraction mode (Experimental)";
            this.InstantExtractionCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.InstantExtractionCB.UseVisualStyleBackColor = true;
            this.InstantExtractionCB.CheckedChanged += new System.EventHandler(this.InstantExtractionCB_CheckedChanged);
            this.InstantExtractionCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.InstantExtractionCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // clearCacheCB
            // 
            this.clearCacheCB.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.clearCacheCB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clearCacheCB.Location = new System.Drawing.Point(2, 64);
            this.clearCacheCB.Margin = new System.Windows.Forms.Padding(1);
            this.clearCacheCB.Name = "clearCacheCB";
            this.clearCacheCB.Size = new System.Drawing.Size(201, 28);
            this.clearCacheCB.TabIndex = 32;
            this.clearCacheCB.Text = "Clear WoT Cache Data";
            this.clearCacheCB.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.clearCacheCB.UseVisualStyleBackColor = true;
            this.clearCacheCB.CheckedChanged += new System.EventHandler(this.clearCacheCB_CheckedChanged);
            this.clearCacheCB.MouseEnter += new System.EventHandler(this.Generic_MouseEnter);
            this.clearCacheCB.MouseLeave += new System.EventHandler(this.Generic_MouseLeave);
            // 
            // AdvancedSettingsDescription
            // 
            this.AdvancedSettingsDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdvancedSettingsDescription.Location = new System.Drawing.Point(12, 259);
            this.AdvancedSettingsDescription.Name = "AdvancedSettingsDescription";
            this.AdvancedSettingsDescription.ReadOnly = true;
            this.AdvancedSettingsDescription.Size = new System.Drawing.Size(410, 85);
            this.AdvancedSettingsDescription.TabIndex = 3;
            this.AdvancedSettingsDescription.Text = "";
            // 
            // loadingImageGroupBox
            // 
            this.loadingImageGroupBox.Controls.Add(this.tableLayoutPanel8);
            this.loadingImageGroupBox.Location = new System.Drawing.Point(220, 198);
            this.loadingImageGroupBox.Margin = new System.Windows.Forms.Padding(1);
            this.loadingImageGroupBox.Name = "loadingImageGroupBox";
            this.loadingImageGroupBox.Padding = new System.Windows.Forms.Padding(2);
            this.loadingImageGroupBox.Size = new System.Drawing.Size(202, 55);
            this.loadingImageGroupBox.TabIndex = 28;
            this.loadingImageGroupBox.TabStop = false;
            this.loadingImageGroupBox.Text = "Loading Image";
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 1;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel8.Controls.Add(this.thirdGuardsLoadingImageRB, 0, 1);
            this.tableLayoutPanel8.Controls.Add(this.standardImageRB, 0, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(2, 15);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 2;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(198, 38);
            this.tableLayoutPanel8.TabIndex = 7;
            // 
            // thirdGuardsLoadingImageRB
            // 
            this.thirdGuardsLoadingImageRB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thirdGuardsLoadingImageRB.Location = new System.Drawing.Point(0, 19);
            this.thirdGuardsLoadingImageRB.Margin = new System.Windows.Forms.Padding(0);
            this.thirdGuardsLoadingImageRB.Name = "thirdGuardsLoadingImageRB";
            this.thirdGuardsLoadingImageRB.Size = new System.Drawing.Size(198, 19);
            this.thirdGuardsLoadingImageRB.TabIndex = 1;
            this.thirdGuardsLoadingImageRB.TabStop = true;
            this.thirdGuardsLoadingImageRB.Text = "3rdguards";
            this.thirdGuardsLoadingImageRB.UseVisualStyleBackColor = true;
            this.thirdGuardsLoadingImageRB.CheckedChanged += new System.EventHandler(this.standardImageRB_CheckedChanged);
            this.thirdGuardsLoadingImageRB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.standardImageRB_MouseDown);
            // 
            // standardImageRB
            // 
            this.standardImageRB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standardImageRB.Location = new System.Drawing.Point(0, 0);
            this.standardImageRB.Margin = new System.Windows.Forms.Padding(0);
            this.standardImageRB.Name = "standardImageRB";
            this.standardImageRB.Size = new System.Drawing.Size(198, 19);
            this.standardImageRB.TabIndex = 0;
            this.standardImageRB.TabStop = true;
            this.standardImageRB.Text = "Standard";
            this.standardImageRB.UseVisualStyleBackColor = true;
            this.standardImageRB.CheckedChanged += new System.EventHandler(this.standardImageRB_CheckedChanged);
            this.standardImageRB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.standardImageRB_MouseDown);
            // 
            // UninstallModeGroupBox
            // 
            this.UninstallModeGroupBox.Controls.Add(this.tableLayoutPanel7);
            this.UninstallModeGroupBox.Location = new System.Drawing.Point(12, 200);
            this.UninstallModeGroupBox.Margin = new System.Windows.Forms.Padding(1);
            this.UninstallModeGroupBox.Name = "UninstallModeGroupBox";
            this.UninstallModeGroupBox.Padding = new System.Windows.Forms.Padding(2);
            this.UninstallModeGroupBox.Size = new System.Drawing.Size(204, 55);
            this.UninstallModeGroupBox.TabIndex = 42;
            this.UninstallModeGroupBox.TabStop = false;
            this.UninstallModeGroupBox.Text = "Uninstall Mode";
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel7.Controls.Add(this.CleanUninstallModeRB, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.SmartUninstallModeRB, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(2, 15);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(200, 38);
            this.tableLayoutPanel7.TabIndex = 6;
            // 
            // CleanUninstallModeRB
            // 
            this.CleanUninstallModeRB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CleanUninstallModeRB.Location = new System.Drawing.Point(0, 19);
            this.CleanUninstallModeRB.Margin = new System.Windows.Forms.Padding(0);
            this.CleanUninstallModeRB.Name = "CleanUninstallModeRB";
            this.CleanUninstallModeRB.Size = new System.Drawing.Size(200, 19);
            this.CleanUninstallModeRB.TabIndex = 1;
            this.CleanUninstallModeRB.TabStop = true;
            this.CleanUninstallModeRB.Text = "Quick";
            this.CleanUninstallModeRB.UseVisualStyleBackColor = true;
            this.CleanUninstallModeRB.CheckedChanged += new System.EventHandler(this.CleanUninstallModeRB_CheckedChanged);
            // 
            // SmartUninstallModeRB
            // 
            this.SmartUninstallModeRB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SmartUninstallModeRB.Location = new System.Drawing.Point(0, 0);
            this.SmartUninstallModeRB.Margin = new System.Windows.Forms.Padding(0);
            this.SmartUninstallModeRB.Name = "SmartUninstallModeRB";
            this.SmartUninstallModeRB.Size = new System.Drawing.Size(200, 19);
            this.SmartUninstallModeRB.TabIndex = 0;
            this.SmartUninstallModeRB.TabStop = true;
            this.SmartUninstallModeRB.Text = "Smart";
            this.SmartUninstallModeRB.UseVisualStyleBackColor = true;
            this.SmartUninstallModeRB.CheckedChanged += new System.EventHandler(this.SmartUninstallModeRB_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.linkLabel4, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.linkLabel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.linkLabel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.linkLabel1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 350);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(410, 68);
            this.tableLayoutPanel1.TabIndex = 43;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabel1.Location = new System.Drawing.Point(4, 1);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(197, 32);
            this.linkLabel1.TabIndex = 0;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "linkLabel1";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabel2.Location = new System.Drawing.Point(208, 1);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(198, 32);
            this.linkLabel2.TabIndex = 1;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "linkLabel2";
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabel3.Location = new System.Drawing.Point(4, 34);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(197, 33);
            this.linkLabel3.TabIndex = 2;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "linkLabel3";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabel4.Location = new System.Drawing.Point(208, 34);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(198, 33);
            this.linkLabel4.TabIndex = 3;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "linkLabel4";
            // 
            // AdvancedSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 430);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.UninstallModeGroupBox);
            this.Controls.Add(this.loadingImageGroupBox);
            this.Controls.Add(this.AdvancedSettingsDescription);
            this.Controls.Add(this.AdvancedSettingsLayout);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AdvancedSettings";
            this.Text = "AdvancedSettings";
            this.Load += new System.EventHandler(this.AdvancedSettings_Load);
            this.AdvancedSettingsLayout.ResumeLayout(false);
            this.AdvancedSettingsLayout.PerformLayout();
            this.loadingImageGroupBox.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.UninstallModeGroupBox.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label AdvancedSettingsHeader;
        private System.Windows.Forms.TableLayoutPanel AdvancedSettingsLayout;
        private System.Windows.Forms.RichTextBox AdvancedSettingsDescription;
        private System.Windows.Forms.GroupBox loadingImageGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.RadioButton thirdGuardsLoadingImageRB;
        private System.Windows.Forms.RadioButton standardImageRB;
        private System.Windows.Forms.GroupBox UninstallModeGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.RadioButton CleanUninstallModeRB;
        private System.Windows.Forms.RadioButton SmartUninstallModeRB;
        private System.Windows.Forms.CheckBox clearCacheCB;
        private System.Windows.Forms.CheckBox InstantExtractionCB;
        private System.Windows.Forms.CheckBox createShortcutsCB;
        private System.Windows.Forms.CheckBox forceManuel;
        private System.Windows.Forms.CheckBox ExportModeCB;
        private System.Windows.Forms.CheckBox cleanInstallCB;
        private System.Windows.Forms.CheckBox ShowInstallCompleteWindowCB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}