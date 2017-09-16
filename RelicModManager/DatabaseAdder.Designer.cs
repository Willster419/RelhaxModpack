namespace RelhaxModpack
{
    partial class DatabaseAdder
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
            this.ConfirmAddLabel = new System.Windows.Forms.Label();
            this.ConfirmAddNo = new System.Windows.Forms.RadioButton();
            this.ConfirmAddYes = new System.Windows.Forms.RadioButton();
            this.ConfirmAddPanel = new System.Windows.Forms.Panel();
            this.AddUnderPanel = new System.Windows.Forms.Panel();
            this.CategoryLabel = new System.Windows.Forms.Label();
            this.AddUnderCB = new System.Windows.Forms.ComboBox();
            this.AddUnderLabel = new System.Windows.Forms.Label();
            this.ModPanel = new System.Windows.Forms.Panel();
            this.ModLabel = new System.Windows.Forms.Label();
            this.ModCB = new System.Windows.Forms.ComboBox();
            this.ConfigPanel = new System.Windows.Forms.Panel();
            this.ConfigLabel = new System.Windows.Forms.Label();
            this.ConfigCB = new System.Windows.Forms.ComboBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.SelectModePanel = new System.Windows.Forms.Panel();
            this.SelectModeLabel = new System.Windows.Forms.Label();
            this.SameLevelRB = new System.Windows.Forms.RadioButton();
            this.NewLevelRB = new System.Windows.Forms.RadioButton();
            this.ConfirmAddPanel.SuspendLayout();
            this.AddUnderPanel.SuspendLayout();
            this.ModPanel.SuspendLayout();
            this.ConfigPanel.SuspendLayout();
            this.SelectModePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConfirmAddLabel
            // 
            this.ConfirmAddLabel.AutoSize = true;
            this.ConfirmAddLabel.Location = new System.Drawing.Point(3, 9);
            this.ConfirmAddLabel.Name = "ConfirmAddLabel";
            this.ConfirmAddLabel.Size = new System.Drawing.Size(251, 13);
            this.ConfirmAddLabel.TabIndex = 0;
            this.ConfirmAddLabel.Text = "Confirm you wish to add/move the object to the list?";
            // 
            // ConfirmAddNo
            // 
            this.ConfirmAddNo.AutoSize = true;
            this.ConfirmAddNo.Checked = true;
            this.ConfirmAddNo.Location = new System.Drawing.Point(6, 25);
            this.ConfirmAddNo.Name = "ConfirmAddNo";
            this.ConfirmAddNo.Size = new System.Drawing.Size(37, 17);
            this.ConfirmAddNo.TabIndex = 1;
            this.ConfirmAddNo.TabStop = true;
            this.ConfirmAddNo.Text = "no";
            this.ConfirmAddNo.UseVisualStyleBackColor = true;
            // 
            // ConfirmAddYes
            // 
            this.ConfirmAddYes.AutoSize = true;
            this.ConfirmAddYes.Location = new System.Drawing.Point(49, 25);
            this.ConfirmAddYes.Name = "ConfirmAddYes";
            this.ConfirmAddYes.Size = new System.Drawing.Size(41, 17);
            this.ConfirmAddYes.TabIndex = 2;
            this.ConfirmAddYes.Text = "yes";
            this.ConfirmAddYes.UseVisualStyleBackColor = true;
            this.ConfirmAddYes.CheckedChanged += new System.EventHandler(this.ConfirmAddYes_CheckedChanged);
            // 
            // ConfirmAddPanel
            // 
            this.ConfirmAddPanel.Controls.Add(this.ConfirmAddLabel);
            this.ConfirmAddPanel.Controls.Add(this.ConfirmAddYes);
            this.ConfirmAddPanel.Controls.Add(this.ConfirmAddNo);
            this.ConfirmAddPanel.Location = new System.Drawing.Point(12, 12);
            this.ConfirmAddPanel.Name = "ConfirmAddPanel";
            this.ConfirmAddPanel.Size = new System.Drawing.Size(276, 46);
            this.ConfirmAddPanel.TabIndex = 3;
            // 
            // AddUnderPanel
            // 
            this.AddUnderPanel.Controls.Add(this.CategoryLabel);
            this.AddUnderPanel.Controls.Add(this.AddUnderCB);
            this.AddUnderPanel.Controls.Add(this.AddUnderLabel);
            this.AddUnderPanel.Enabled = false;
            this.AddUnderPanel.Location = new System.Drawing.Point(12, 106);
            this.AddUnderPanel.Name = "AddUnderPanel";
            this.AddUnderPanel.Size = new System.Drawing.Size(276, 65);
            this.AddUnderPanel.TabIndex = 4;
            // 
            // CategoryLabel
            // 
            this.CategoryLabel.AutoSize = true;
            this.CategoryLabel.Location = new System.Drawing.Point(6, 17);
            this.CategoryLabel.Name = "CategoryLabel";
            this.CategoryLabel.Size = new System.Drawing.Size(49, 13);
            this.CategoryLabel.TabIndex = 2;
            this.CategoryLabel.Text = "Category";
            // 
            // AddUnderCB
            // 
            this.AddUnderCB.FormattingEnabled = true;
            this.AddUnderCB.Location = new System.Drawing.Point(3, 35);
            this.AddUnderCB.Name = "AddUnderCB";
            this.AddUnderCB.Size = new System.Drawing.Size(270, 21);
            this.AddUnderCB.TabIndex = 1;
            this.AddUnderCB.SelectedIndexChanged += new System.EventHandler(this.AddUnderCB_SelectedIndexChanged);
            // 
            // AddUnderLabel
            // 
            this.AddUnderLabel.AutoSize = true;
            this.AddUnderLabel.Location = new System.Drawing.Point(0, 0);
            this.AddUnderLabel.Name = "AddUnderLabel";
            this.AddUnderLabel.Size = new System.Drawing.Size(59, 13);
            this.AddUnderLabel.TabIndex = 0;
            this.AddUnderLabel.Text = "Add above";
            // 
            // ModPanel
            // 
            this.ModPanel.Controls.Add(this.ModLabel);
            this.ModPanel.Controls.Add(this.ModCB);
            this.ModPanel.Enabled = false;
            this.ModPanel.Location = new System.Drawing.Point(12, 177);
            this.ModPanel.Name = "ModPanel";
            this.ModPanel.Size = new System.Drawing.Size(276, 45);
            this.ModPanel.TabIndex = 5;
            // 
            // ModLabel
            // 
            this.ModLabel.AutoSize = true;
            this.ModLabel.Location = new System.Drawing.Point(3, 0);
            this.ModLabel.Name = "ModLabel";
            this.ModLabel.Size = new System.Drawing.Size(28, 13);
            this.ModLabel.TabIndex = 2;
            this.ModLabel.Text = "Mod";
            // 
            // ModCB
            // 
            this.ModCB.FormattingEnabled = true;
            this.ModCB.Location = new System.Drawing.Point(0, 18);
            this.ModCB.Name = "ModCB";
            this.ModCB.Size = new System.Drawing.Size(270, 21);
            this.ModCB.TabIndex = 1;
            this.ModCB.SelectedIndexChanged += new System.EventHandler(this.ModCB_SelectedIndexChanged);
            // 
            // ConfigPanel
            // 
            this.ConfigPanel.Controls.Add(this.ConfigLabel);
            this.ConfigPanel.Controls.Add(this.ConfigCB);
            this.ConfigPanel.Enabled = false;
            this.ConfigPanel.Location = new System.Drawing.Point(12, 228);
            this.ConfigPanel.Name = "ConfigPanel";
            this.ConfigPanel.Size = new System.Drawing.Size(276, 43);
            this.ConfigPanel.TabIndex = 5;
            // 
            // ConfigLabel
            // 
            this.ConfigLabel.AutoSize = true;
            this.ConfigLabel.Location = new System.Drawing.Point(3, 0);
            this.ConfigLabel.Name = "ConfigLabel";
            this.ConfigLabel.Size = new System.Drawing.Size(37, 13);
            this.ConfigLabel.TabIndex = 2;
            this.ConfigLabel.Text = "Config";
            // 
            // ConfigCB
            // 
            this.ConfigCB.FormattingEnabled = true;
            this.ConfigCB.Location = new System.Drawing.Point(1, 16);
            this.ConfigCB.Name = "ConfigCB";
            this.ConfigCB.Size = new System.Drawing.Size(270, 21);
            this.ConfigCB.TabIndex = 1;
            this.ConfigCB.SelectedIndexChanged += new System.EventHandler(this.ConfigCB_SelectedIndexChanged);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(213, 274);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 6;
            this.applyButton.Text = "confirm";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // SelectModePanel
            // 
            this.SelectModePanel.Controls.Add(this.NewLevelRB);
            this.SelectModePanel.Controls.Add(this.SameLevelRB);
            this.SelectModePanel.Controls.Add(this.SelectModeLabel);
            this.SelectModePanel.Location = new System.Drawing.Point(12, 60);
            this.SelectModePanel.Name = "SelectModePanel";
            this.SelectModePanel.Size = new System.Drawing.Size(276, 40);
            this.SelectModePanel.TabIndex = 7;
            this.SelectModePanel.Visible = false;
            // 
            // SelectModeLabel
            // 
            this.SelectModeLabel.AutoSize = true;
            this.SelectModeLabel.Location = new System.Drawing.Point(0, 1);
            this.SelectModeLabel.Name = "SelectModeLabel";
            this.SelectModeLabel.Size = new System.Drawing.Size(188, 13);
            this.SelectModeLabel.TabIndex = 0;
            this.SelectModeLabel.Text = "Adding at same level or new sublevel?";
            // 
            // SameLevelRB
            // 
            this.SameLevelRB.AutoSize = true;
            this.SameLevelRB.Location = new System.Drawing.Point(4, 18);
            this.SameLevelRB.Name = "SameLevelRB";
            this.SameLevelRB.Size = new System.Drawing.Size(75, 17);
            this.SameLevelRB.TabIndex = 1;
            this.SameLevelRB.TabStop = true;
            this.SameLevelRB.Text = "same level";
            this.SameLevelRB.UseVisualStyleBackColor = true;
            this.SameLevelRB.CheckedChanged += new System.EventHandler(this.SameLevelRB_CheckedChanged);
            // 
            // NewLevelRB
            // 
            this.NewLevelRB.AutoSize = true;
            this.NewLevelRB.Location = new System.Drawing.Point(85, 17);
            this.NewLevelRB.Name = "NewLevelRB";
            this.NewLevelRB.Size = new System.Drawing.Size(70, 17);
            this.NewLevelRB.TabIndex = 2;
            this.NewLevelRB.TabStop = true;
            this.NewLevelRB.Text = "new level";
            this.NewLevelRB.UseVisualStyleBackColor = true;
            this.NewLevelRB.CheckedChanged += new System.EventHandler(this.NewLevelRB_CheckedChanged);
            // 
            // DatabaseAdder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 302);
            this.Controls.Add(this.SelectModePanel);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.ConfigPanel);
            this.Controls.Add(this.ModPanel);
            this.Controls.Add(this.AddUnderPanel);
            this.Controls.Add(this.ConfirmAddPanel);
            this.Name = "DatabaseAdder";
            this.Text = "DatabaseAdder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DatabaseAdder_FormClosing);
            this.ConfirmAddPanel.ResumeLayout(false);
            this.ConfirmAddPanel.PerformLayout();
            this.AddUnderPanel.ResumeLayout(false);
            this.AddUnderPanel.PerformLayout();
            this.ModPanel.ResumeLayout(false);
            this.ModPanel.PerformLayout();
            this.ConfigPanel.ResumeLayout(false);
            this.ConfigPanel.PerformLayout();
            this.SelectModePanel.ResumeLayout(false);
            this.SelectModePanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label ConfirmAddLabel;
        private System.Windows.Forms.RadioButton ConfirmAddNo;
        private System.Windows.Forms.RadioButton ConfirmAddYes;
        private System.Windows.Forms.Panel ConfirmAddPanel;
        private System.Windows.Forms.Panel AddUnderPanel;
        private System.Windows.Forms.ComboBox AddUnderCB;
        private System.Windows.Forms.Label AddUnderLabel;
        private System.Windows.Forms.Label CategoryLabel;
        private System.Windows.Forms.Panel ModPanel;
        private System.Windows.Forms.Label ModLabel;
        private System.Windows.Forms.ComboBox ModCB;
        private System.Windows.Forms.Panel ConfigPanel;
        private System.Windows.Forms.Label ConfigLabel;
        private System.Windows.Forms.ComboBox ConfigCB;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Panel SelectModePanel;
        private System.Windows.Forms.RadioButton NewLevelRB;
        private System.Windows.Forms.RadioButton SameLevelRB;
        private System.Windows.Forms.Label SelectModeLabel;
    }
}