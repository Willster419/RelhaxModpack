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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseAdder));
            this.ConfirmAddLabel = new System.Windows.Forms.Label();
            this.ConfirmAddNo = new System.Windows.Forms.RadioButton();
            this.ConfirmAddYes = new System.Windows.Forms.RadioButton();
            this.ConfirmAddPanel = new System.Windows.Forms.Panel();
            this.AddUnderPanel = new System.Windows.Forms.Panel();
            this.CategoryLabel = new System.Windows.Forms.Label();
            this.CategoryCB = new System.Windows.Forms.ComboBox();
            this.AddUnderLabel = new System.Windows.Forms.Label();
            this.ModPanel = new System.Windows.Forms.Panel();
            this.ModLabel = new System.Windows.Forms.Label();
            this.PackageCB = new System.Windows.Forms.ComboBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.SelectModePanel = new System.Windows.Forms.Panel();
            this.NewLevelRB = new System.Windows.Forms.RadioButton();
            this.SameLevelRB = new System.Windows.Forms.RadioButton();
            this.SelectModeLabel = new System.Windows.Forms.Label();
            this.ConfirmAddPanel.SuspendLayout();
            this.AddUnderPanel.SuspendLayout();
            this.ModPanel.SuspendLayout();
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
            this.ConfirmAddNo.Location = new System.Drawing.Point(3, 25);
            this.ConfirmAddNo.Name = "ConfirmAddNo";
            this.ConfirmAddNo.Size = new System.Drawing.Size(37, 17);
            this.ConfirmAddNo.TabIndex = 1;
            this.ConfirmAddNo.TabStop = true;
            this.ConfirmAddNo.Text = "no";
            this.ConfirmAddNo.UseVisualStyleBackColor = true;
            this.ConfirmAddNo.CheckedChanged += new System.EventHandler(this.ConfirmAddNo_CheckedChanged);
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
            this.AddUnderPanel.Controls.Add(this.CategoryCB);
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
            this.CategoryLabel.Location = new System.Drawing.Point(3, 19);
            this.CategoryLabel.Name = "CategoryLabel";
            this.CategoryLabel.Size = new System.Drawing.Size(49, 13);
            this.CategoryLabel.TabIndex = 2;
            this.CategoryLabel.Text = "Category";
            // 
            // CategoryCB
            // 
            this.CategoryCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CategoryCB.FormattingEnabled = true;
            this.CategoryCB.Location = new System.Drawing.Point(3, 35);
            this.CategoryCB.Name = "CategoryCB";
            this.CategoryCB.Size = new System.Drawing.Size(270, 21);
            this.CategoryCB.TabIndex = 1;
            this.CategoryCB.SelectedIndexChanged += new System.EventHandler(this.CategoryCB_SelectedIndexChanged);
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
            this.ModPanel.Controls.Add(this.PackageCB);
            this.ModPanel.Enabled = false;
            this.ModPanel.Location = new System.Drawing.Point(12, 177);
            this.ModPanel.Name = "ModPanel";
            this.ModPanel.Size = new System.Drawing.Size(276, 45);
            this.ModPanel.TabIndex = 5;
            // 
            // ModLabel
            // 
            this.ModLabel.AutoSize = true;
            this.ModLabel.Location = new System.Drawing.Point(1, 0);
            this.ModLabel.Name = "ModLabel";
            this.ModLabel.Size = new System.Drawing.Size(50, 13);
            this.ModLabel.TabIndex = 2;
            this.ModLabel.Text = "Package";
            // 
            // PackageCB
            // 
            this.PackageCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PackageCB.FormattingEnabled = true;
            this.PackageCB.Location = new System.Drawing.Point(3, 16);
            this.PackageCB.Name = "PackageCB";
            this.PackageCB.Size = new System.Drawing.Size(270, 21);
            this.PackageCB.TabIndex = 1;
            this.PackageCB.SelectedIndexChanged += new System.EventHandler(this.PackageCB_SelectedIndexChanged);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(213, 228);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 6;
            this.applyButton.Text = "confirm";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.ApplyButton_Click);
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
            // NewLevelRB
            // 
            this.NewLevelRB.AutoSize = true;
            this.NewLevelRB.Location = new System.Drawing.Point(84, 17);
            this.NewLevelRB.Name = "NewLevelRB";
            this.NewLevelRB.Size = new System.Drawing.Size(70, 17);
            this.NewLevelRB.TabIndex = 2;
            this.NewLevelRB.TabStop = true;
            this.NewLevelRB.Text = "new level";
            this.NewLevelRB.UseVisualStyleBackColor = true;
            this.NewLevelRB.CheckedChanged += new System.EventHandler(this.NewLevelRB_CheckedChanged);
            // 
            // SameLevelRB
            // 
            this.SameLevelRB.AutoSize = true;
            this.SameLevelRB.Location = new System.Drawing.Point(3, 17);
            this.SameLevelRB.Name = "SameLevelRB";
            this.SameLevelRB.Size = new System.Drawing.Size(75, 17);
            this.SameLevelRB.TabIndex = 1;
            this.SameLevelRB.TabStop = true;
            this.SameLevelRB.Text = "same level";
            this.SameLevelRB.UseVisualStyleBackColor = true;
            this.SameLevelRB.CheckedChanged += new System.EventHandler(this.SameLevelRB_CheckedChanged);
            // 
            // SelectModeLabel
            // 
            this.SelectModeLabel.AutoSize = true;
            this.SelectModeLabel.Location = new System.Drawing.Point(3, 1);
            this.SelectModeLabel.Name = "SelectModeLabel";
            this.SelectModeLabel.Size = new System.Drawing.Size(188, 13);
            this.SelectModeLabel.TabIndex = 0;
            this.SelectModeLabel.Text = "Adding at same level or new sublevel?";
            // 
            // DatabaseAdder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 261);
            this.Controls.Add(this.SelectModePanel);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.ModPanel);
            this.Controls.Add(this.AddUnderPanel);
            this.Controls.Add(this.ConfirmAddPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DatabaseAdder";
            this.Text = "DatabaseAdder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DatabaseAdder_FormClosing);
            this.ConfirmAddPanel.ResumeLayout(false);
            this.ConfirmAddPanel.PerformLayout();
            this.AddUnderPanel.ResumeLayout(false);
            this.AddUnderPanel.PerformLayout();
            this.ModPanel.ResumeLayout(false);
            this.ModPanel.PerformLayout();
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
        private System.Windows.Forms.ComboBox CategoryCB;
        private System.Windows.Forms.Label AddUnderLabel;
        private System.Windows.Forms.Label CategoryLabel;
        private System.Windows.Forms.Panel ModPanel;
        private System.Windows.Forms.Label ModLabel;
        private System.Windows.Forms.ComboBox PackageCB;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Panel SelectModePanel;
        private System.Windows.Forms.RadioButton NewLevelRB;
        private System.Windows.Forms.RadioButton SameLevelRB;
        private System.Windows.Forms.Label SelectModeLabel;
    }
}