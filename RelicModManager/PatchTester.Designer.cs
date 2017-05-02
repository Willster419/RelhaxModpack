namespace RelhaxModpack
{
    partial class PatchTester
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PatchTester));
            this.regexLoadFileButton = new System.Windows.Forms.Button();
            this.regexFilePathLabel = new System.Windows.Forms.Label();
            this.regexFilePathBox = new System.Windows.Forms.RichTextBox();
            this.regexLineLabel = new System.Windows.Forms.Label();
            this.regexLineBox = new System.Windows.Forms.TextBox();
            this.xmlModsGroupBox = new System.Windows.Forms.GroupBox();
            this.xmlRemoveModeButton = new System.Windows.Forms.RadioButton();
            this.xmlEditModsButton = new System.Windows.Forms.RadioButton();
            this.xmlAddModeButton = new System.Windows.Forms.RadioButton();
            this.regexSearchBox = new System.Windows.Forms.TextBox();
            this.regexSearchLabel = new System.Windows.Forms.Label();
            this.regexReplaceLabel = new System.Windows.Forms.Label();
            this.xmlReplaceLabel = new System.Windows.Forms.Label();
            this.xmlSearchBox = new System.Windows.Forms.TextBox();
            this.xmlSearchLabel = new System.Windows.Forms.Label();
            this.xmlPathBox = new System.Windows.Forms.TextBox();
            this.xmlPathLabel = new System.Windows.Forms.Label();
            this.xmlFilePathBox = new System.Windows.Forms.RichTextBox();
            this.xmlFilePathLabel = new System.Windows.Forms.Label();
            this.xmlLoadFileButton = new System.Windows.Forms.Button();
            this.regexPatchButton = new System.Windows.Forms.Button();
            this.xmlPatchButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.xmlPatcherLabel = new System.Windows.Forms.Label();
            this.regexPatcherLabel = new System.Windows.Forms.Label();
            this.regexFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.xmlFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.panel2 = new System.Windows.Forms.Panel();
            this.jsonLoadFileButton = new System.Windows.Forms.Button();
            this.jsonFilePathLabel = new System.Windows.Forms.Label();
            this.jsonFilePathBox = new System.Windows.Forms.RichTextBox();
            this.jsonPathLabel = new System.Windows.Forms.Label();
            this.jsonPathBox = new System.Windows.Forms.TextBox();
            this.jsonSearchLabel = new System.Windows.Forms.Label();
            this.jsonSearchBox = new System.Windows.Forms.TextBox();
            this.jsonReplaceLabel = new System.Windows.Forms.Label();
            this.jsonPatchButton = new System.Windows.Forms.Button();
            this.jsonPatcherLabel = new System.Windows.Forms.Label();
            this.jsonFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.regexMakePatchButton = new System.Windows.Forms.Button();
            this.xmlMakePatchButton = new System.Windows.Forms.Button();
            this.jsonMakePatchButton = new System.Windows.Forms.Button();
            this.regexReplaceBox = new System.Windows.Forms.RichTextBox();
            this.xmlReplaceBox = new System.Windows.Forms.RichTextBox();
            this.jsonReplaceBox = new System.Windows.Forms.RichTextBox();
            this.xmlModsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // regexLoadFileButton
            // 
            this.regexLoadFileButton.Location = new System.Drawing.Point(73, 138);
            this.regexLoadFileButton.Name = "regexLoadFileButton";
            this.regexLoadFileButton.Size = new System.Drawing.Size(151, 23);
            this.regexLoadFileButton.TabIndex = 1;
            this.regexLoadFileButton.Text = "load file to patch";
            this.regexLoadFileButton.UseVisualStyleBackColor = true;
            this.regexLoadFileButton.Click += new System.EventHandler(this.regexLoadFileButton_Click);
            // 
            // regexFilePathLabel
            // 
            this.regexFilePathLabel.AutoSize = true;
            this.regexFilePathLabel.Location = new System.Drawing.Point(12, 30);
            this.regexFilePathLabel.Name = "regexFilePathLabel";
            this.regexFilePathLabel.Size = new System.Drawing.Size(41, 13);
            this.regexFilePathLabel.TabIndex = 1;
            this.regexFilePathLabel.Text = "filepath";
            // 
            // regexFilePathBox
            // 
            this.regexFilePathBox.Location = new System.Drawing.Point(12, 46);
            this.regexFilePathBox.Name = "regexFilePathBox";
            this.regexFilePathBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.regexFilePathBox.Size = new System.Drawing.Size(212, 86);
            this.regexFilePathBox.TabIndex = 0;
            this.regexFilePathBox.Text = "";
            // 
            // regexLineLabel
            // 
            this.regexLineLabel.AutoSize = true;
            this.regexLineLabel.Location = new System.Drawing.Point(9, 161);
            this.regexLineLabel.Name = "regexLineLabel";
            this.regexLineLabel.Size = new System.Drawing.Size(23, 13);
            this.regexLineLabel.TabIndex = 4;
            this.regexLineLabel.Text = "line";
            // 
            // regexLineBox
            // 
            this.regexLineBox.Location = new System.Drawing.Point(12, 177);
            this.regexLineBox.Name = "regexLineBox";
            this.regexLineBox.Size = new System.Drawing.Size(212, 20);
            this.regexLineBox.TabIndex = 2;
            // 
            // xmlModsGroupBox
            // 
            this.xmlModsGroupBox.Controls.Add(this.xmlRemoveModeButton);
            this.xmlModsGroupBox.Controls.Add(this.xmlEditModsButton);
            this.xmlModsGroupBox.Controls.Add(this.xmlAddModeButton);
            this.xmlModsGroupBox.Location = new System.Drawing.Point(250, 138);
            this.xmlModsGroupBox.Name = "xmlModsGroupBox";
            this.xmlModsGroupBox.Size = new System.Drawing.Size(125, 81);
            this.xmlModsGroupBox.TabIndex = 15;
            this.xmlModsGroupBox.TabStop = false;
            this.xmlModsGroupBox.Text = "Mode";
            // 
            // xmlRemoveModeButton
            // 
            this.xmlRemoveModeButton.AutoSize = true;
            this.xmlRemoveModeButton.Location = new System.Drawing.Point(6, 58);
            this.xmlRemoveModeButton.Name = "xmlRemoveModeButton";
            this.xmlRemoveModeButton.Size = new System.Drawing.Size(60, 17);
            this.xmlRemoveModeButton.TabIndex = 2;
            this.xmlRemoveModeButton.TabStop = true;
            this.xmlRemoveModeButton.Text = "remove";
            this.xmlRemoveModeButton.UseVisualStyleBackColor = true;
            this.xmlRemoveModeButton.CheckedChanged += new System.EventHandler(this.xmlRemoveModeButton_CheckedChanged);
            // 
            // xmlEditModsButton
            // 
            this.xmlEditModsButton.AutoSize = true;
            this.xmlEditModsButton.Location = new System.Drawing.Point(6, 39);
            this.xmlEditModsButton.Name = "xmlEditModsButton";
            this.xmlEditModsButton.Size = new System.Drawing.Size(42, 17);
            this.xmlEditModsButton.TabIndex = 1;
            this.xmlEditModsButton.TabStop = true;
            this.xmlEditModsButton.Text = "edit";
            this.xmlEditModsButton.UseVisualStyleBackColor = true;
            this.xmlEditModsButton.CheckedChanged += new System.EventHandler(this.xmlEditModsButton_CheckedChanged);
            // 
            // xmlAddModeButton
            // 
            this.xmlAddModeButton.AutoSize = true;
            this.xmlAddModeButton.Location = new System.Drawing.Point(6, 19);
            this.xmlAddModeButton.Name = "xmlAddModeButton";
            this.xmlAddModeButton.Size = new System.Drawing.Size(43, 17);
            this.xmlAddModeButton.TabIndex = 0;
            this.xmlAddModeButton.TabStop = true;
            this.xmlAddModeButton.Text = "add";
            this.xmlAddModeButton.UseVisualStyleBackColor = true;
            this.xmlAddModeButton.CheckedChanged += new System.EventHandler(this.xmlAddModeButton_CheckedChanged);
            // 
            // regexSearchBox
            // 
            this.regexSearchBox.Location = new System.Drawing.Point(12, 216);
            this.regexSearchBox.Name = "regexSearchBox";
            this.regexSearchBox.Size = new System.Drawing.Size(212, 20);
            this.regexSearchBox.TabIndex = 3;
            // 
            // regexSearchLabel
            // 
            this.regexSearchLabel.AutoSize = true;
            this.regexSearchLabel.Location = new System.Drawing.Point(9, 200);
            this.regexSearchLabel.Name = "regexSearchLabel";
            this.regexSearchLabel.Size = new System.Drawing.Size(39, 13);
            this.regexSearchLabel.TabIndex = 6;
            this.regexSearchLabel.Text = "search";
            // 
            // regexReplaceLabel
            // 
            this.regexReplaceLabel.AutoSize = true;
            this.regexReplaceLabel.Location = new System.Drawing.Point(9, 242);
            this.regexReplaceLabel.Name = "regexReplaceLabel";
            this.regexReplaceLabel.Size = new System.Drawing.Size(42, 13);
            this.regexReplaceLabel.TabIndex = 8;
            this.regexReplaceLabel.Text = "replace";
            // 
            // xmlReplaceLabel
            // 
            this.xmlReplaceLabel.AutoSize = true;
            this.xmlReplaceLabel.Location = new System.Drawing.Point(247, 304);
            this.xmlReplaceLabel.Name = "xmlReplaceLabel";
            this.xmlReplaceLabel.Size = new System.Drawing.Size(42, 13);
            this.xmlReplaceLabel.TabIndex = 20;
            this.xmlReplaceLabel.Text = "replace";
            // 
            // xmlSearchBox
            // 
            this.xmlSearchBox.Location = new System.Drawing.Point(250, 278);
            this.xmlSearchBox.Name = "xmlSearchBox";
            this.xmlSearchBox.Size = new System.Drawing.Size(212, 20);
            this.xmlSearchBox.TabIndex = 10;
            // 
            // xmlSearchLabel
            // 
            this.xmlSearchLabel.AutoSize = true;
            this.xmlSearchLabel.Location = new System.Drawing.Point(247, 262);
            this.xmlSearchLabel.Name = "xmlSearchLabel";
            this.xmlSearchLabel.Size = new System.Drawing.Size(39, 13);
            this.xmlSearchLabel.TabIndex = 18;
            this.xmlSearchLabel.Text = "search";
            // 
            // xmlPathBox
            // 
            this.xmlPathBox.Location = new System.Drawing.Point(250, 239);
            this.xmlPathBox.Name = "xmlPathBox";
            this.xmlPathBox.Size = new System.Drawing.Size(212, 20);
            this.xmlPathBox.TabIndex = 9;
            // 
            // xmlPathLabel
            // 
            this.xmlPathLabel.AutoSize = true;
            this.xmlPathLabel.Location = new System.Drawing.Point(247, 223);
            this.xmlPathLabel.Name = "xmlPathLabel";
            this.xmlPathLabel.Size = new System.Drawing.Size(28, 13);
            this.xmlPathLabel.TabIndex = 16;
            this.xmlPathLabel.Text = "path";
            // 
            // xmlFilePathBox
            // 
            this.xmlFilePathBox.Location = new System.Drawing.Point(250, 46);
            this.xmlFilePathBox.Name = "xmlFilePathBox";
            this.xmlFilePathBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.xmlFilePathBox.Size = new System.Drawing.Size(212, 86);
            this.xmlFilePathBox.TabIndex = 7;
            this.xmlFilePathBox.Text = "";
            // 
            // xmlFilePathLabel
            // 
            this.xmlFilePathLabel.AutoSize = true;
            this.xmlFilePathLabel.Location = new System.Drawing.Point(250, 30);
            this.xmlFilePathLabel.Name = "xmlFilePathLabel";
            this.xmlFilePathLabel.Size = new System.Drawing.Size(41, 13);
            this.xmlFilePathLabel.TabIndex = 12;
            this.xmlFilePathLabel.Text = "filepath";
            // 
            // xmlLoadFileButton
            // 
            this.xmlLoadFileButton.Location = new System.Drawing.Point(381, 138);
            this.xmlLoadFileButton.Name = "xmlLoadFileButton";
            this.xmlLoadFileButton.Size = new System.Drawing.Size(81, 36);
            this.xmlLoadFileButton.TabIndex = 8;
            this.xmlLoadFileButton.Text = "load file to patch";
            this.xmlLoadFileButton.UseVisualStyleBackColor = true;
            this.xmlLoadFileButton.Click += new System.EventHandler(this.xmlLoadFileButton_Click);
            // 
            // regexPatchButton
            // 
            this.regexPatchButton.Location = new System.Drawing.Point(129, 364);
            this.regexPatchButton.Name = "regexPatchButton";
            this.regexPatchButton.Size = new System.Drawing.Size(95, 23);
            this.regexPatchButton.TabIndex = 5;
            this.regexPatchButton.Text = "test patch";
            this.regexPatchButton.UseVisualStyleBackColor = true;
            this.regexPatchButton.Click += new System.EventHandler(this.regexPatchButton_Click);
            // 
            // xmlPatchButton
            // 
            this.xmlPatchButton.Location = new System.Drawing.Point(367, 364);
            this.xmlPatchButton.Name = "xmlPatchButton";
            this.xmlPatchButton.Size = new System.Drawing.Size(95, 23);
            this.xmlPatchButton.TabIndex = 12;
            this.xmlPatchButton.Text = "test patch";
            this.xmlPatchButton.UseVisualStyleBackColor = true;
            this.xmlPatchButton.Click += new System.EventHandler(this.xmlPatchButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.WindowText;
            this.panel1.Location = new System.Drawing.Point(234, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(10, 430);
            this.panel1.TabIndex = 23;
            // 
            // xmlPatcherLabel
            // 
            this.xmlPatcherLabel.AutoSize = true;
            this.xmlPatcherLabel.Location = new System.Drawing.Point(328, 14);
            this.xmlPatcherLabel.Name = "xmlPatcherLabel";
            this.xmlPatcherLabel.Size = new System.Drawing.Size(69, 13);
            this.xmlPatcherLabel.TabIndex = 11;
            this.xmlPatcherLabel.Text = "XML Patcher";
            // 
            // regexPatcherLabel
            // 
            this.regexPatcherLabel.AutoSize = true;
            this.regexPatcherLabel.Location = new System.Drawing.Point(80, 14);
            this.regexPatcherLabel.Name = "regexPatcherLabel";
            this.regexPatcherLabel.Size = new System.Drawing.Size(78, 13);
            this.regexPatcherLabel.TabIndex = 0;
            this.regexPatcherLabel.Text = "Regex Patcher";
            // 
            // regexFileDialog
            // 
            this.regexFileDialog.FileName = "regexFileDialog";
            // 
            // xmlFileDialog
            // 
            this.xmlFileDialog.FileName = "xmlFileDialog";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.WindowText;
            this.panel2.Location = new System.Drawing.Point(468, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 430);
            this.panel2.TabIndex = 24;
            // 
            // jsonLoadFileButton
            // 
            this.jsonLoadFileButton.Location = new System.Drawing.Point(554, 138);
            this.jsonLoadFileButton.Name = "jsonLoadFileButton";
            this.jsonLoadFileButton.Size = new System.Drawing.Size(141, 23);
            this.jsonLoadFileButton.TabIndex = 15;
            this.jsonLoadFileButton.Text = "load file to patch";
            this.jsonLoadFileButton.UseVisualStyleBackColor = true;
            this.jsonLoadFileButton.Click += new System.EventHandler(this.jsonLoadFileButton_Click);
            // 
            // jsonFilePathLabel
            // 
            this.jsonFilePathLabel.AutoSize = true;
            this.jsonFilePathLabel.Location = new System.Drawing.Point(483, 30);
            this.jsonFilePathLabel.Name = "jsonFilePathLabel";
            this.jsonFilePathLabel.Size = new System.Drawing.Size(44, 13);
            this.jsonFilePathLabel.TabIndex = 1;
            this.jsonFilePathLabel.Text = "file path";
            // 
            // jsonFilePathBox
            // 
            this.jsonFilePathBox.Location = new System.Drawing.Point(483, 46);
            this.jsonFilePathBox.Name = "jsonFilePathBox";
            this.jsonFilePathBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.jsonFilePathBox.Size = new System.Drawing.Size(212, 86);
            this.jsonFilePathBox.TabIndex = 14;
            this.jsonFilePathBox.Text = "";
            // 
            // jsonPathLabel
            // 
            this.jsonPathLabel.AutoSize = true;
            this.jsonPathLabel.Location = new System.Drawing.Point(480, 161);
            this.jsonPathLabel.Name = "jsonPathLabel";
            this.jsonPathLabel.Size = new System.Drawing.Size(28, 13);
            this.jsonPathLabel.TabIndex = 4;
            this.jsonPathLabel.Text = "path";
            // 
            // jsonPathBox
            // 
            this.jsonPathBox.Location = new System.Drawing.Point(483, 177);
            this.jsonPathBox.Name = "jsonPathBox";
            this.jsonPathBox.Size = new System.Drawing.Size(212, 20);
            this.jsonPathBox.TabIndex = 16;
            // 
            // jsonSearchLabel
            // 
            this.jsonSearchLabel.AutoSize = true;
            this.jsonSearchLabel.Location = new System.Drawing.Point(480, 200);
            this.jsonSearchLabel.Name = "jsonSearchLabel";
            this.jsonSearchLabel.Size = new System.Drawing.Size(39, 13);
            this.jsonSearchLabel.TabIndex = 6;
            this.jsonSearchLabel.Text = "search";
            // 
            // jsonSearchBox
            // 
            this.jsonSearchBox.Enabled = false;
            this.jsonSearchBox.Location = new System.Drawing.Point(483, 216);
            this.jsonSearchBox.Name = "jsonSearchBox";
            this.jsonSearchBox.Size = new System.Drawing.Size(212, 20);
            this.jsonSearchBox.TabIndex = 17;
            // 
            // jsonReplaceLabel
            // 
            this.jsonReplaceLabel.AutoSize = true;
            this.jsonReplaceLabel.Location = new System.Drawing.Point(480, 242);
            this.jsonReplaceLabel.Name = "jsonReplaceLabel";
            this.jsonReplaceLabel.Size = new System.Drawing.Size(42, 13);
            this.jsonReplaceLabel.TabIndex = 8;
            this.jsonReplaceLabel.Text = "replace";
            // 
            // jsonPatchButton
            // 
            this.jsonPatchButton.Location = new System.Drawing.Point(600, 364);
            this.jsonPatchButton.Name = "jsonPatchButton";
            this.jsonPatchButton.Size = new System.Drawing.Size(95, 23);
            this.jsonPatchButton.TabIndex = 19;
            this.jsonPatchButton.Text = "test patch";
            this.jsonPatchButton.UseVisualStyleBackColor = true;
            this.jsonPatchButton.Click += new System.EventHandler(this.jsonPatchButton_Click);
            // 
            // jsonPatcherLabel
            // 
            this.jsonPatcherLabel.AutoSize = true;
            this.jsonPatcherLabel.Location = new System.Drawing.Point(551, 14);
            this.jsonPatcherLabel.Name = "jsonPatcherLabel";
            this.jsonPatcherLabel.Size = new System.Drawing.Size(69, 13);
            this.jsonPatcherLabel.TabIndex = 0;
            this.jsonPatcherLabel.Text = "Json Patcher";
            // 
            // jsonFileDialog
            // 
            this.jsonFileDialog.FileName = "jsonFileDialog";
            // 
            // regexMakePatchButton
            // 
            this.regexMakePatchButton.Location = new System.Drawing.Point(129, 393);
            this.regexMakePatchButton.Name = "regexMakePatchButton";
            this.regexMakePatchButton.Size = new System.Drawing.Size(95, 23);
            this.regexMakePatchButton.TabIndex = 6;
            this.regexMakePatchButton.Text = "make patch file";
            this.regexMakePatchButton.UseVisualStyleBackColor = true;
            this.regexMakePatchButton.Click += new System.EventHandler(this.regexMakePatchButton_Click);
            // 
            // xmlMakePatchButton
            // 
            this.xmlMakePatchButton.Location = new System.Drawing.Point(367, 393);
            this.xmlMakePatchButton.Name = "xmlMakePatchButton";
            this.xmlMakePatchButton.Size = new System.Drawing.Size(95, 23);
            this.xmlMakePatchButton.TabIndex = 13;
            this.xmlMakePatchButton.Text = "make patch file";
            this.xmlMakePatchButton.UseVisualStyleBackColor = true;
            this.xmlMakePatchButton.Click += new System.EventHandler(this.xmlMakePatchButton_Click);
            // 
            // jsonMakePatchButton
            // 
            this.jsonMakePatchButton.Location = new System.Drawing.Point(600, 393);
            this.jsonMakePatchButton.Name = "jsonMakePatchButton";
            this.jsonMakePatchButton.Size = new System.Drawing.Size(95, 23);
            this.jsonMakePatchButton.TabIndex = 20;
            this.jsonMakePatchButton.Text = "make patch file";
            this.jsonMakePatchButton.UseVisualStyleBackColor = true;
            this.jsonMakePatchButton.Click += new System.EventHandler(this.jsonMakePatchButton_Click);
            // 
            // regexReplaceBox
            // 
            this.regexReplaceBox.Location = new System.Drawing.Point(12, 262);
            this.regexReplaceBox.Name = "regexReplaceBox";
            this.regexReplaceBox.Size = new System.Drawing.Size(212, 96);
            this.regexReplaceBox.TabIndex = 4;
            this.regexReplaceBox.Text = "";
            this.regexReplaceBox.WordWrap = false;
            // 
            // xmlReplaceBox
            // 
            this.xmlReplaceBox.Location = new System.Drawing.Point(250, 320);
            this.xmlReplaceBox.Name = "xmlReplaceBox";
            this.xmlReplaceBox.Size = new System.Drawing.Size(212, 38);
            this.xmlReplaceBox.TabIndex = 11;
            this.xmlReplaceBox.Text = "";
            this.xmlReplaceBox.WordWrap = false;
            // 
            // jsonReplaceBox
            // 
            this.jsonReplaceBox.Location = new System.Drawing.Point(483, 258);
            this.jsonReplaceBox.Name = "jsonReplaceBox";
            this.jsonReplaceBox.Size = new System.Drawing.Size(212, 100);
            this.jsonReplaceBox.TabIndex = 18;
            this.jsonReplaceBox.Text = "";
            this.jsonReplaceBox.WordWrap = false;
            // 
            // PatchTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(707, 427);
            this.Controls.Add(this.jsonReplaceBox);
            this.Controls.Add(this.xmlReplaceBox);
            this.Controls.Add(this.regexReplaceBox);
            this.Controls.Add(this.jsonMakePatchButton);
            this.Controls.Add(this.xmlMakePatchButton);
            this.Controls.Add(this.regexMakePatchButton);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.jsonPatcherLabel);
            this.Controls.Add(this.regexPatcherLabel);
            this.Controls.Add(this.xmlPatcherLabel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.jsonPatchButton);
            this.Controls.Add(this.xmlPatchButton);
            this.Controls.Add(this.regexPatchButton);
            this.Controls.Add(this.xmlReplaceLabel);
            this.Controls.Add(this.xmlSearchBox);
            this.Controls.Add(this.xmlSearchLabel);
            this.Controls.Add(this.xmlPathBox);
            this.Controls.Add(this.xmlPathLabel);
            this.Controls.Add(this.xmlFilePathBox);
            this.Controls.Add(this.xmlFilePathLabel);
            this.Controls.Add(this.xmlLoadFileButton);
            this.Controls.Add(this.jsonReplaceLabel);
            this.Controls.Add(this.jsonSearchBox);
            this.Controls.Add(this.regexReplaceLabel);
            this.Controls.Add(this.jsonSearchLabel);
            this.Controls.Add(this.regexSearchBox);
            this.Controls.Add(this.regexSearchLabel);
            this.Controls.Add(this.jsonPathBox);
            this.Controls.Add(this.xmlModsGroupBox);
            this.Controls.Add(this.jsonPathLabel);
            this.Controls.Add(this.regexLineBox);
            this.Controls.Add(this.jsonFilePathBox);
            this.Controls.Add(this.regexLineLabel);
            this.Controls.Add(this.jsonFilePathLabel);
            this.Controls.Add(this.regexFilePathBox);
            this.Controls.Add(this.jsonLoadFileButton);
            this.Controls.Add(this.regexFilePathLabel);
            this.Controls.Add(this.regexLoadFileButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "PatchTester";
            this.Text = "PatchTester";
            this.Load += new System.EventHandler(this.PatchTester_Load);
            this.xmlModsGroupBox.ResumeLayout(false);
            this.xmlModsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button regexLoadFileButton;
        private System.Windows.Forms.Label regexFilePathLabel;
        private System.Windows.Forms.RichTextBox regexFilePathBox;
        private System.Windows.Forms.Label regexLineLabel;
        private System.Windows.Forms.TextBox regexLineBox;
        private System.Windows.Forms.GroupBox xmlModsGroupBox;
        private System.Windows.Forms.RadioButton xmlRemoveModeButton;
        private System.Windows.Forms.RadioButton xmlEditModsButton;
        private System.Windows.Forms.RadioButton xmlAddModeButton;
        private System.Windows.Forms.TextBox regexSearchBox;
        private System.Windows.Forms.Label regexSearchLabel;
        private System.Windows.Forms.Label regexReplaceLabel;
        private System.Windows.Forms.Label xmlReplaceLabel;
        private System.Windows.Forms.TextBox xmlSearchBox;
        private System.Windows.Forms.Label xmlSearchLabel;
        private System.Windows.Forms.TextBox xmlPathBox;
        private System.Windows.Forms.Label xmlPathLabel;
        private System.Windows.Forms.RichTextBox xmlFilePathBox;
        private System.Windows.Forms.Label xmlFilePathLabel;
        private System.Windows.Forms.Button xmlLoadFileButton;
        private System.Windows.Forms.Button regexPatchButton;
        private System.Windows.Forms.Button xmlPatchButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label xmlPatcherLabel;
        private System.Windows.Forms.Label regexPatcherLabel;
        private System.Windows.Forms.OpenFileDialog regexFileDialog;
        private System.Windows.Forms.OpenFileDialog xmlFileDialog;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button jsonLoadFileButton;
        private System.Windows.Forms.Label jsonFilePathLabel;
        private System.Windows.Forms.RichTextBox jsonFilePathBox;
        private System.Windows.Forms.Label jsonPathLabel;
        private System.Windows.Forms.TextBox jsonPathBox;
        private System.Windows.Forms.Label jsonSearchLabel;
        private System.Windows.Forms.TextBox jsonSearchBox;
        private System.Windows.Forms.Label jsonReplaceLabel;
        private System.Windows.Forms.Button jsonPatchButton;
        private System.Windows.Forms.Label jsonPatcherLabel;
        private System.Windows.Forms.OpenFileDialog jsonFileDialog;
        private System.Windows.Forms.Button regexMakePatchButton;
        private System.Windows.Forms.Button xmlMakePatchButton;
        private System.Windows.Forms.Button jsonMakePatchButton;
        private System.Windows.Forms.RichTextBox regexReplaceBox;
        private System.Windows.Forms.RichTextBox xmlReplaceBox;
        private System.Windows.Forms.RichTextBox jsonReplaceBox;
    }
}