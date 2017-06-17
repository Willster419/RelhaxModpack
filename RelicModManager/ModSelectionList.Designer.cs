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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModSelectionList));
            this.continueButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.helpLabel = new System.Windows.Forms.Label();
            this.loadConfigButton = new System.Windows.Forms.Button();
            this.saveConfigButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.clearSelectionsButton = new System.Windows.Forms.Button();
            this.TanksPath = new System.Windows.Forms.Label();
            this.TanksVersionLabel = new System.Windows.Forms.Label();
            this.tabPage11 = new System.Windows.Forms.TabPage();
            this.modTabGroups = new System.Windows.Forms.TabControl();
            this.expandAllButton = new System.Windows.Forms.Button();
            this.colapseAllButton = new System.Windows.Forms.Button();
            this.modTabGroups.SuspendLayout();
            this.SuspendLayout();
            // 
            // continueButton
            // 
            this.continueButton.Location = new System.Drawing.Point(907, 420);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(73, 41);
            this.continueButton.TabIndex = 5;
            this.continueButton.Text = "Continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.Click += new System.EventHandler(this.continueButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(831, 420);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(70, 41);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "label1";
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
            this.loadConfigButton.Location = new System.Drawing.Point(577, 420);
            this.loadConfigButton.Name = "loadConfigButton";
            this.loadConfigButton.Size = new System.Drawing.Size(113, 41);
            this.loadConfigButton.TabIndex = 9;
            this.loadConfigButton.Text = "Load Pref";
            this.loadConfigButton.UseVisualStyleBackColor = true;
            this.loadConfigButton.Click += new System.EventHandler(this.loadConfigButton_Click);
            // 
            // saveConfigButton
            // 
            this.saveConfigButton.Location = new System.Drawing.Point(696, 420);
            this.saveConfigButton.Name = "saveConfigButton";
            this.saveConfigButton.Size = new System.Drawing.Size(129, 41);
            this.saveConfigButton.TabIndex = 10;
            this.saveConfigButton.Text = "Save Pref";
            this.saveConfigButton.UseVisualStyleBackColor = true;
            this.saveConfigButton.Click += new System.EventHandler(this.saveConfigButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(177, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "\"*\" tab indicates single selection tab";
            // 
            // clearSelectionsButton
            // 
            this.clearSelectionsButton.Location = new System.Drawing.Point(467, 420);
            this.clearSelectionsButton.Name = "clearSelectionsButton";
            this.clearSelectionsButton.Size = new System.Drawing.Size(104, 41);
            this.clearSelectionsButton.TabIndex = 13;
            this.clearSelectionsButton.Text = "Clear Selections";
            this.clearSelectionsButton.UseVisualStyleBackColor = true;
            this.clearSelectionsButton.Click += new System.EventHandler(this.clearSelectionsButton_Click);
            // 
            // TanksPath
            // 
            this.TanksPath.AutoSize = true;
            this.TanksPath.Location = new System.Drawing.Point(13, 33);
            this.TanksPath.Name = "TanksPath";
            this.TanksPath.Size = new System.Drawing.Size(63, 13);
            this.TanksPath.TabIndex = 14;
            this.TanksPath.Text = "Installing to ";
            // 
            // TanksVersionLabel
            // 
            this.TanksVersionLabel.AutoSize = true;
            this.TanksVersionLabel.Location = new System.Drawing.Point(13, 47);
            this.TanksVersionLabel.Name = "TanksVersionLabel";
            this.TanksVersionLabel.Size = new System.Drawing.Size(34, 13);
            this.TanksVersionLabel.TabIndex = 15;
            this.TanksVersionLabel.Text = "WoT ";
            // 
            // tabPage11
            // 
            this.tabPage11.Location = new System.Drawing.Point(4, 22);
            this.tabPage11.Name = "tabPage11";
            this.tabPage11.Size = new System.Drawing.Size(960, 312);
            this.tabPage11.TabIndex = 10;
            this.tabPage11.Text = "Custom UserPackages";
            this.tabPage11.UseVisualStyleBackColor = true;
            // 
            // modTabGroups
            // 
            this.modTabGroups.Controls.Add(this.tabPage11);
            this.modTabGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modTabGroups.Location = new System.Drawing.Point(12, 76);
            this.modTabGroups.Name = "modTabGroups";
            this.modTabGroups.SelectedIndex = 0;
            this.modTabGroups.Size = new System.Drawing.Size(968, 338);
            this.modTabGroups.TabIndex = 4;
            this.modTabGroups.Selected += new System.Windows.Forms.TabControlEventHandler(this.modTabGroups_Selected);
            this.modTabGroups.Click += new System.EventHandler(this.modTabGroups_Click);
            // 
            // expandAllButton
            // 
            this.expandAllButton.Location = new System.Drawing.Point(831, 38);
            this.expandAllButton.Name = "expandAllButton";
            this.expandAllButton.Size = new System.Drawing.Size(149, 30);
            this.expandAllButton.TabIndex = 16;
            this.expandAllButton.Text = "Expand Current Tab";
            this.expandAllButton.UseVisualStyleBackColor = true;
            this.expandAllButton.Click += new System.EventHandler(this.expandAllButton_Click);
            // 
            // colapseAllButton
            // 
            this.colapseAllButton.Location = new System.Drawing.Point(831, 5);
            this.colapseAllButton.Name = "colapseAllButton";
            this.colapseAllButton.Size = new System.Drawing.Size(149, 30);
            this.colapseAllButton.TabIndex = 17;
            this.colapseAllButton.Text = "Collapse Current Tab";
            this.colapseAllButton.UseVisualStyleBackColor = true;
            this.colapseAllButton.Click += new System.EventHandler(this.ColapseAllButton_Click);
            // 
            // ModSelectionList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 473);
            this.Controls.Add(this.colapseAllButton);
            this.Controls.Add(this.expandAllButton);
            this.Controls.Add(this.TanksVersionLabel);
            this.Controls.Add(this.TanksPath);
            this.Controls.Add(this.clearSelectionsButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.saveConfigButton);
            this.Controls.Add(this.loadConfigButton);
            this.Controls.Add(this.helpLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.modTabGroups);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(550, 250);
            this.Name = "ModSelectionList";
            this.Text = "ModSelectionList";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModSelectionList_FormClosing);
            this.Load += new System.EventHandler(this.ModSelectionList_Load);
            this.SizeChanged += new System.EventHandler(this.ModSelectionList_SizeChanged);
            this.modTabGroups.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button continueButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.Button loadConfigButton;
        private System.Windows.Forms.Button saveConfigButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button clearSelectionsButton;
        private System.Windows.Forms.Label TanksPath;
        private System.Windows.Forms.Label TanksVersionLabel;
        private System.Windows.Forms.TabPage tabPage11;
        private System.Windows.Forms.TabControl modTabGroups;
        private System.Windows.Forms.Button expandAllButton;
        private System.Windows.Forms.Button colapseAllButton;
    }
}