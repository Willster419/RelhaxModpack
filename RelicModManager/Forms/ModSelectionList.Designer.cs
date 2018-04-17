using System.Windows.Forms;

namespace RelhaxModpack
{
    partial class ModSelectionList
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                // Dispose stuff here
                if (modTabGroups != null)
                {
                    foreach (System.Windows.Forms.TabPage t in this.modTabGroups.TabPages)
                    {
                        foreach(System.Windows.Forms.Control host in t.Controls)
                        {
                            if(host is System.Windows.Forms.Integration.ElementHost)
                            {
                                System.Windows.Forms.Integration.ElementHost host2 = (System.Windows.Forms.Integration.ElementHost)host;
                                host2.Child = null;
                                host2.Dispose();
                                host2 = null;
                            }
                        }
                        if(t.Controls.Count > 0)
                            t.Controls.Clear();
                        if(t != null)
                            t.Dispose();
                    }
                    if(modTabGroups.TabPages.Count > 0)
                        modTabGroups.TabPages.Clear();
                    if(modTabGroups.Controls.Count > 0)
                        modTabGroups.Controls.Clear();
                    modTabGroups.Dispose();
                    modTabGroups = null;
                }
                for (int i = 0; i < this.Controls.Count; i++)
                {
                    if (this.Controls[i] != null)
                    {
                        this.Controls[i].Dispose();
                    }
                }
                if(this.Controls.Count > 0)
                    this.Controls.Clear();
                ParsedCatagoryList = null;
                UserMods = null;
                GlobalDependencies = null;
                if (p != null)
                {
                    p.Dispose();
                    p = null;
                }
                if (pw != null)
                {
                    pw.Dispose();
                    pw = null;
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModSelectionList));
            this.continueButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.WindowSizeTB = new System.Windows.Forms.Label();
            this.helpLabel = new System.Windows.Forms.Label();
            this.loadConfigButton = new System.Windows.Forms.Button();
            this.saveConfigButton = new System.Windows.Forms.Button();
            this.clearSelectionsButton = new System.Windows.Forms.Button();
            this.TanksPath = new System.Windows.Forms.Label();
            this.TanksVersionLabel = new System.Windows.Forms.Label();
            this.modTabGroups = new System.Windows.Forms.TabControl();
            this.expandAllButton = new System.Windows.Forms.Button();
            this.colapseAllButton = new System.Windows.Forms.Button();
            this.searchCB = new System.Windows.Forms.ComboBox();
            this.DescriptionToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SearchToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.searchTB = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // continueButton
            // 
            this.continueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.continueButton.Location = new System.Drawing.Point(698, 394);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(80, 41);
            this.continueButton.TabIndex = 5;
            this.continueButton.Text = "Continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.Click += new System.EventHandler(this.ContinueButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(610, 394);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(82, 41);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // WindowSizeTB
            // 
            this.WindowSizeTB.AutoSize = true;
            this.WindowSizeTB.Location = new System.Drawing.Point(13, 47);
            this.WindowSizeTB.Name = "WindowSizeTB";
            this.WindowSizeTB.Size = new System.Drawing.Size(80, 13);
            this.WindowSizeTB.TabIndex = 7;
            this.WindowSizeTB.Text = "WindowSizeTB";
            // 
            // helpLabel
            // 
            this.helpLabel.AutoSize = true;
            this.helpLabel.Location = new System.Drawing.Point(13, 5);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(173, 13);
            this.helpLabel.TabIndex = 8;
            this.helpLabel.Text = "right-click a mod name to preview it";
            // 
            // loadConfigButton
            // 
            this.loadConfigButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.loadConfigButton.Location = new System.Drawing.Point(356, 394);
            this.loadConfigButton.Name = "loadConfigButton";
            this.loadConfigButton.Size = new System.Drawing.Size(113, 41);
            this.loadConfigButton.TabIndex = 9;
            this.loadConfigButton.Text = "Load Pref";
            this.loadConfigButton.UseVisualStyleBackColor = true;
            this.loadConfigButton.Click += new System.EventHandler(this.LoadConfigButton_Click);
            // 
            // saveConfigButton
            // 
            this.saveConfigButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveConfigButton.Location = new System.Drawing.Point(475, 394);
            this.saveConfigButton.Name = "saveConfigButton";
            this.saveConfigButton.Size = new System.Drawing.Size(129, 41);
            this.saveConfigButton.TabIndex = 10;
            this.saveConfigButton.Text = "Save Pref";
            this.saveConfigButton.UseVisualStyleBackColor = true;
            this.saveConfigButton.Click += new System.EventHandler(this.SaveConfigButton_Click);
            // 
            // clearSelectionsButton
            // 
            this.clearSelectionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.clearSelectionsButton.Location = new System.Drawing.Point(246, 394);
            this.clearSelectionsButton.Name = "clearSelectionsButton";
            this.clearSelectionsButton.Size = new System.Drawing.Size(104, 41);
            this.clearSelectionsButton.TabIndex = 13;
            this.clearSelectionsButton.Text = "Clear Selections";
            this.clearSelectionsButton.UseVisualStyleBackColor = true;
            this.clearSelectionsButton.Click += new System.EventHandler(this.ClearSelectionsButton_Click);
            // 
            // TanksPath
            // 
            this.TanksPath.AutoSize = true;
            this.TanksPath.Location = new System.Drawing.Point(13, 20);
            this.TanksPath.Name = "TanksPath";
            this.TanksPath.Size = new System.Drawing.Size(63, 13);
            this.TanksPath.TabIndex = 14;
            this.TanksPath.Text = "Installing to ";
            // 
            // TanksVersionLabel
            // 
            this.TanksVersionLabel.AutoSize = true;
            this.TanksVersionLabel.Location = new System.Drawing.Point(13, 33);
            this.TanksVersionLabel.Name = "TanksVersionLabel";
            this.TanksVersionLabel.Size = new System.Drawing.Size(34, 13);
            this.TanksVersionLabel.TabIndex = 15;
            this.TanksVersionLabel.Text = "WoT ";
            // 
            // modTabGroups
            // 
            this.modTabGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modTabGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modTabGroups.Location = new System.Drawing.Point(12, 76);
            this.modTabGroups.Multiline = true;
            this.modTabGroups.Name = "modTabGroups";
            this.modTabGroups.SelectedIndex = 0;
            this.modTabGroups.Size = new System.Drawing.Size(766, 312);
            this.modTabGroups.TabIndex = 4;
            this.modTabGroups.Selected += new System.Windows.Forms.TabControlEventHandler(this.modTabGroups_Selected);
            this.modTabGroups.Click += new System.EventHandler(this.modTabGroups_Click);
            // 
            // expandAllButton
            // 
            this.expandAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.expandAllButton.Location = new System.Drawing.Point(629, 12);
            this.expandAllButton.Name = "expandAllButton";
            this.expandAllButton.Size = new System.Drawing.Size(149, 34);
            this.expandAllButton.TabIndex = 16;
            this.expandAllButton.Text = "Expand Current Tab";
            this.expandAllButton.UseVisualStyleBackColor = true;
            this.expandAllButton.Click += new System.EventHandler(this.expandAllButton_Click);
            // 
            // colapseAllButton
            // 
            this.colapseAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.colapseAllButton.Location = new System.Drawing.Point(474, 12);
            this.colapseAllButton.Name = "colapseAllButton";
            this.colapseAllButton.Size = new System.Drawing.Size(149, 34);
            this.colapseAllButton.TabIndex = 17;
            this.colapseAllButton.Text = "Collapse Current Tab";
            this.colapseAllButton.UseVisualStyleBackColor = true;
            this.colapseAllButton.Click += new System.EventHandler(this.ColapseAllButton_Click);
            // 
            // searchCB
            // 
            this.searchCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchCB.Location = new System.Drawing.Point(474, 52);
            this.searchCB.Name = "searchCB";
            this.searchCB.Size = new System.Drawing.Size(304, 21);
            this.searchCB.TabIndex = 18;
            this.SearchToolTip.SetToolTip(this.searchCB, "custom ToolTip Text");
            this.searchCB.SelectionChangeCommitted += new System.EventHandler(this.searchCB_SelectionChangeCommitted);
            this.searchCB.TextUpdate += new System.EventHandler(this.searchComboBox_TextUpdate);
            this.searchCB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchCB_KeyDown);
            // 
            // searchTB
            // 
            this.searchTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTB.Location = new System.Drawing.Point(319, 55);
            this.searchTB.Name = "searchTB";
            this.searchTB.Size = new System.Drawing.Size(150, 13);
            this.searchTB.TabIndex = 19;
            this.searchTB.Text = "Search Mod Name:";
            this.searchTB.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.SearchToolTip.SetToolTip(this.searchTB, "custom ToolTip Text");
            // 
            // ModSelectionList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 447);
            this.Controls.Add(this.searchTB);
            this.Controls.Add(this.searchCB);
            this.Controls.Add(this.colapseAllButton);
            this.Controls.Add(this.expandAllButton);
            this.Controls.Add(this.TanksVersionLabel);
            this.Controls.Add(this.TanksPath);
            this.Controls.Add(this.clearSelectionsButton);
            this.Controls.Add(this.saveConfigButton);
            this.Controls.Add(this.loadConfigButton);
            this.Controls.Add(this.helpLabel);
            this.Controls.Add(this.WindowSizeTB);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.modTabGroups);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 480);
            this.Name = "ModSelectionList";
            this.Text = "Relhax Mod Selection";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModSelectionList_FormClosing);
            this.Load += new System.EventHandler(this.ModSelectionList_Load);
            this.SizeChanged += new System.EventHandler(this.ModSelectionList_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button continueButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label WindowSizeTB;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.Button loadConfigButton;
        private System.Windows.Forms.Button saveConfigButton;
        private System.Windows.Forms.Button clearSelectionsButton;
        private System.Windows.Forms.Label TanksPath;
        private System.Windows.Forms.Label TanksVersionLabel;
        private System.Windows.Forms.TabControl modTabGroups;
        private System.Windows.Forms.Button expandAllButton;
        private System.Windows.Forms.Button colapseAllButton;
        private System.Windows.Forms.ComboBox searchCB;
        private ToolTip DescriptionToolTip;
        private ToolTip SearchToolTip;
        private Label searchTB;
    }
}