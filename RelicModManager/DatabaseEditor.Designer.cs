namespace RelhaxModpack
{
    partial class DatabaseEditor
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
            this.DatabaseTreeView = new System.Windows.Forms.TreeView();
            this.ObjectName = new System.Windows.Forms.Label();
            this.ObjectNameTB = new System.Windows.Forms.TextBox();
            this.DatabasePanelTree = new System.Windows.Forms.Panel();
            this.DatabaseEditPanel = new System.Windows.Forms.Panel();
            this.ObjectAppendExtractionCB = new System.Windows.Forms.CheckBox();
            this.ObjectVisableCheckBox = new System.Windows.Forms.CheckBox();
            this.ObjectTypeComboBox = new System.Windows.Forms.ComboBox();
            this.ObjectType = new System.Windows.Forms.Label();
            this.ObjectUpdateNotes = new System.Windows.Forms.Label();
            this.ObjectDescription = new System.Windows.Forms.Label();
            this.ObjectUpdateNotesTB = new System.Windows.Forms.RichTextBox();
            this.ObjectDescTB = new System.Windows.Forms.RichTextBox();
            this.ObjectDevURLTB = new System.Windows.Forms.TextBox();
            this.ObjectDevURL = new System.Windows.Forms.Label();
            this.ApplyChangesButton = new System.Windows.Forms.Button();
            this.ObjectEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.ObjectZipFileTB = new System.Windows.Forms.TextBox();
            this.ObjectZipFile = new System.Windows.Forms.Label();
            this.ObjectEndAddressTB = new System.Windows.Forms.TextBox();
            this.ObjectEdnAddress = new System.Windows.Forms.Label();
            this.ObjectStartAddressTB = new System.Windows.Forms.TextBox();
            this.ObjectStartAddress = new System.Windows.Forms.Label();
            this.ObjectPackageNameTB = new System.Windows.Forms.TextBox();
            this.ObjectPacakgeName = new System.Windows.Forms.Label();
            this.ObjectDependenciesList = new System.Windows.Forms.ListBox();
            this.ObjectDependenciesLabel = new System.Windows.Forms.Label();
            this.ObjectPicturesLabel = new System.Windows.Forms.Label();
            this.SaveDatabaseButton = new System.Windows.Forms.Button();
            this.LoadDatabaseButton = new System.Windows.Forms.Button();
            this.RemoveEntryButton = new System.Windows.Forms.Button();
            this.AddEntryButton = new System.Windows.Forms.Button();
            this.DBORB = new System.Windows.Forms.RadioButton();
            this.GlobalDependencyRB = new System.Windows.Forms.RadioButton();
            this.DependencyRB = new System.Windows.Forms.RadioButton();
            this.LogicalDependencyRB = new System.Windows.Forms.RadioButton();
            this.OpenDatabaseDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveDatabaseDialog = new System.Windows.Forms.SaveFileDialog();
            this.DatabaseSubeditPanel = new System.Windows.Forms.Panel();
            this.DependencyPanel = new System.Windows.Forms.Panel();
            this.AddDependencyButton = new System.Windows.Forms.Button();
            this.CurrentDependenciesCB = new System.Windows.Forms.ComboBox();
            this.RemoveDependencyButton = new System.Windows.Forms.Button();
            this.DependencyPackageNameLabel = new System.Windows.Forms.Label();
            this.LogicalDependencyPanel = new System.Windows.Forms.Panel();
            this.AddLogicalDependencyButton = new System.Windows.Forms.Button();
            this.CurrentLogicalDependenciesCB = new System.Windows.Forms.ComboBox();
            this.ObjectLogicalDependenciesLabel = new System.Windows.Forms.Label();
            this.RemoveLogicalDependencyButton = new System.Windows.Forms.Button();
            this.LogicalDependencyPackageNameLabel = new System.Windows.Forms.Label();
            this.LogicalDependnecyNegateFlagCB = new System.Windows.Forms.CheckBox();
            this.ObjectLogicalDependenciesList = new System.Windows.Forms.ListBox();
            this.ObjectPicturesList = new System.Windows.Forms.ListBox();
            this.PicturesTypeLabel = new System.Windows.Forms.Label();
            this.PicturesTypeCBox = new System.Windows.Forms.ComboBox();
            this.PicturesURLLabel = new System.Windows.Forms.Label();
            this.PictureURLTB = new System.Windows.Forms.RichTextBox();
            this.MovePictureButton = new System.Windows.Forms.Button();
            this.PicturePanel = new System.Windows.Forms.Panel();
            this.AddPictureTB = new System.Windows.Forms.TextBox();
            this.MovePictureTB = new System.Windows.Forms.TextBox();
            this.AddPictureButton = new System.Windows.Forms.Button();
            this.ApplyPictureEditButton = new System.Windows.Forms.Button();
            this.RemovePictureButton = new System.Windows.Forms.Button();
            this.MoveButton = new System.Windows.Forms.Button();
            this.DatabasePanelTree.SuspendLayout();
            this.DatabaseEditPanel.SuspendLayout();
            this.DatabaseSubeditPanel.SuspendLayout();
            this.DependencyPanel.SuspendLayout();
            this.LogicalDependencyPanel.SuspendLayout();
            this.PicturePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // DatabaseTreeView
            // 
            this.DatabaseTreeView.ForeColor = System.Drawing.Color.Blue;
            this.DatabaseTreeView.Location = new System.Drawing.Point(3, 3);
            this.DatabaseTreeView.Name = "DatabaseTreeView";
            this.DatabaseTreeView.Size = new System.Drawing.Size(304, 431);
            this.DatabaseTreeView.TabIndex = 0;
            this.DatabaseTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DatabaseTreeView_NodeMouseClick);
            // 
            // ObjectName
            // 
            this.ObjectName.AutoSize = true;
            this.ObjectName.Location = new System.Drawing.Point(8, 6);
            this.ObjectName.Name = "ObjectName";
            this.ObjectName.Size = new System.Drawing.Size(33, 13);
            this.ObjectName.TabIndex = 1;
            this.ObjectName.Text = "name";
            // 
            // ObjectNameTB
            // 
            this.ObjectNameTB.Location = new System.Drawing.Point(91, 3);
            this.ObjectNameTB.Name = "ObjectNameTB";
            this.ObjectNameTB.Size = new System.Drawing.Size(302, 20);
            this.ObjectNameTB.TabIndex = 2;
            // 
            // DatabasePanelTree
            // 
            this.DatabasePanelTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DatabasePanelTree.Controls.Add(this.DatabaseTreeView);
            this.DatabasePanelTree.Location = new System.Drawing.Point(12, 30);
            this.DatabasePanelTree.Name = "DatabasePanelTree";
            this.DatabasePanelTree.Size = new System.Drawing.Size(310, 442);
            this.DatabasePanelTree.TabIndex = 3;
            // 
            // DatabaseEditPanel
            // 
            this.DatabaseEditPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DatabaseEditPanel.Controls.Add(this.ObjectAppendExtractionCB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectVisableCheckBox);
            this.DatabaseEditPanel.Controls.Add(this.ObjectTypeComboBox);
            this.DatabaseEditPanel.Controls.Add(this.ObjectType);
            this.DatabaseEditPanel.Controls.Add(this.ObjectUpdateNotes);
            this.DatabaseEditPanel.Controls.Add(this.ObjectDescription);
            this.DatabaseEditPanel.Controls.Add(this.ObjectUpdateNotesTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectDescTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectDevURLTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectDevURL);
            this.DatabaseEditPanel.Controls.Add(this.ApplyChangesButton);
            this.DatabaseEditPanel.Controls.Add(this.ObjectEnabledCheckBox);
            this.DatabaseEditPanel.Controls.Add(this.ObjectZipFileTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectZipFile);
            this.DatabaseEditPanel.Controls.Add(this.ObjectEndAddressTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectEdnAddress);
            this.DatabaseEditPanel.Controls.Add(this.ObjectStartAddressTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectStartAddress);
            this.DatabaseEditPanel.Controls.Add(this.ObjectPackageNameTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectPacakgeName);
            this.DatabaseEditPanel.Controls.Add(this.ObjectNameTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectName);
            this.DatabaseEditPanel.Location = new System.Drawing.Point(328, 30);
            this.DatabaseEditPanel.Name = "DatabaseEditPanel";
            this.DatabaseEditPanel.Size = new System.Drawing.Size(396, 442);
            this.DatabaseEditPanel.TabIndex = 4;
            // 
            // ObjectAppendExtractionCB
            // 
            this.ObjectAppendExtractionCB.AutoSize = true;
            this.ObjectAppendExtractionCB.Location = new System.Drawing.Point(248, 184);
            this.ObjectAppendExtractionCB.Name = "ObjectAppendExtractionCB";
            this.ObjectAppendExtractionCB.Size = new System.Drawing.Size(109, 17);
            this.ObjectAppendExtractionCB.TabIndex = 25;
            this.ObjectAppendExtractionCB.Text = "appendExtraction";
            this.ObjectAppendExtractionCB.UseVisualStyleBackColor = true;
            // 
            // ObjectVisableCheckBox
            // 
            this.ObjectVisableCheckBox.AutoSize = true;
            this.ObjectVisableCheckBox.Location = new System.Drawing.Point(173, 185);
            this.ObjectVisableCheckBox.Name = "ObjectVisableCheckBox";
            this.ObjectVisableCheckBox.Size = new System.Drawing.Size(59, 17);
            this.ObjectVisableCheckBox.TabIndex = 23;
            this.ObjectVisableCheckBox.Text = "visable";
            this.ObjectVisableCheckBox.UseVisualStyleBackColor = true;
            // 
            // ObjectTypeComboBox
            // 
            this.ObjectTypeComboBox.FormattingEnabled = true;
            this.ObjectTypeComboBox.Items.AddRange(new object[] {
            "-none-",
            "single1",
            "single_dropDown1",
            "single_dropDown2",
            "multi"});
            this.ObjectTypeComboBox.Location = new System.Drawing.Point(91, 158);
            this.ObjectTypeComboBox.Name = "ObjectTypeComboBox";
            this.ObjectTypeComboBox.Size = new System.Drawing.Size(146, 21);
            this.ObjectTypeComboBox.TabIndex = 21;
            // 
            // ObjectType
            // 
            this.ObjectType.AutoSize = true;
            this.ObjectType.Location = new System.Drawing.Point(8, 161);
            this.ObjectType.Name = "ObjectType";
            this.ObjectType.Size = new System.Drawing.Size(27, 13);
            this.ObjectType.TabIndex = 20;
            this.ObjectType.Text = "type";
            // 
            // ObjectUpdateNotes
            // 
            this.ObjectUpdateNotes.AutoSize = true;
            this.ObjectUpdateNotes.Location = new System.Drawing.Point(8, 330);
            this.ObjectUpdateNotes.Name = "ObjectUpdateNotes";
            this.ObjectUpdateNotes.Size = new System.Drawing.Size(91, 13);
            this.ObjectUpdateNotes.TabIndex = 19;
            this.ObjectUpdateNotes.Text = "update comments";
            // 
            // ObjectDescription
            // 
            this.ObjectDescription.AutoSize = true;
            this.ObjectDescription.Location = new System.Drawing.Point(6, 223);
            this.ObjectDescription.Name = "ObjectDescription";
            this.ObjectDescription.Size = new System.Drawing.Size(58, 13);
            this.ObjectDescription.TabIndex = 18;
            this.ObjectDescription.Text = "description";
            // 
            // ObjectUpdateNotesTB
            // 
            this.ObjectUpdateNotesTB.Location = new System.Drawing.Point(9, 346);
            this.ObjectUpdateNotesTB.Name = "ObjectUpdateNotesTB";
            this.ObjectUpdateNotesTB.Size = new System.Drawing.Size(384, 59);
            this.ObjectUpdateNotesTB.TabIndex = 17;
            this.ObjectUpdateNotesTB.Text = "";
            // 
            // ObjectDescTB
            // 
            this.ObjectDescTB.Location = new System.Drawing.Point(9, 239);
            this.ObjectDescTB.Name = "ObjectDescTB";
            this.ObjectDescTB.Size = new System.Drawing.Size(384, 88);
            this.ObjectDescTB.TabIndex = 16;
            this.ObjectDescTB.Text = "";
            // 
            // ObjectDevURLTB
            // 
            this.ObjectDevURLTB.Location = new System.Drawing.Point(91, 133);
            this.ObjectDevURLTB.Name = "ObjectDevURLTB";
            this.ObjectDevURLTB.Size = new System.Drawing.Size(302, 20);
            this.ObjectDevURLTB.TabIndex = 15;
            // 
            // ObjectDevURL
            // 
            this.ObjectDevURL.AutoSize = true;
            this.ObjectDevURL.Location = new System.Drawing.Point(8, 136);
            this.ObjectDevURL.Name = "ObjectDevURL";
            this.ObjectDevURL.Size = new System.Drawing.Size(50, 13);
            this.ObjectDevURL.TabIndex = 14;
            this.ObjectDevURL.Text = "dev URL";
            // 
            // ApplyChangesButton
            // 
            this.ApplyChangesButton.Location = new System.Drawing.Point(222, 411);
            this.ApplyChangesButton.Name = "ApplyChangesButton";
            this.ApplyChangesButton.Size = new System.Drawing.Size(169, 23);
            this.ApplyChangesButton.TabIndex = 13;
            this.ApplyChangesButton.Text = "apply changes (from this panel)";
            this.ApplyChangesButton.UseVisualStyleBackColor = true;
            this.ApplyChangesButton.Click += new System.EventHandler(this.ApplyChangesButton_Click);
            // 
            // ObjectEnabledCheckBox
            // 
            this.ObjectEnabledCheckBox.AutoSize = true;
            this.ObjectEnabledCheckBox.Location = new System.Drawing.Point(91, 184);
            this.ObjectEnabledCheckBox.Name = "ObjectEnabledCheckBox";
            this.ObjectEnabledCheckBox.Size = new System.Drawing.Size(64, 17);
            this.ObjectEnabledCheckBox.TabIndex = 12;
            this.ObjectEnabledCheckBox.Text = "enabled";
            this.ObjectEnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // ObjectZipFileTB
            // 
            this.ObjectZipFileTB.Location = new System.Drawing.Point(91, 107);
            this.ObjectZipFileTB.Name = "ObjectZipFileTB";
            this.ObjectZipFileTB.Size = new System.Drawing.Size(302, 20);
            this.ObjectZipFileTB.TabIndex = 10;
            // 
            // ObjectZipFile
            // 
            this.ObjectZipFile.AutoSize = true;
            this.ObjectZipFile.Location = new System.Drawing.Point(8, 110);
            this.ObjectZipFile.Name = "ObjectZipFile";
            this.ObjectZipFile.Size = new System.Drawing.Size(36, 13);
            this.ObjectZipFile.TabIndex = 9;
            this.ObjectZipFile.Text = "zip file";
            // 
            // ObjectEndAddressTB
            // 
            this.ObjectEndAddressTB.Location = new System.Drawing.Point(91, 81);
            this.ObjectEndAddressTB.Name = "ObjectEndAddressTB";
            this.ObjectEndAddressTB.Size = new System.Drawing.Size(302, 20);
            this.ObjectEndAddressTB.TabIndex = 8;
            // 
            // ObjectEdnAddress
            // 
            this.ObjectEdnAddress.AutoSize = true;
            this.ObjectEdnAddress.Location = new System.Drawing.Point(8, 84);
            this.ObjectEdnAddress.Name = "ObjectEdnAddress";
            this.ObjectEdnAddress.Size = new System.Drawing.Size(65, 13);
            this.ObjectEdnAddress.TabIndex = 7;
            this.ObjectEdnAddress.Text = "end address";
            // 
            // ObjectStartAddressTB
            // 
            this.ObjectStartAddressTB.Location = new System.Drawing.Point(91, 55);
            this.ObjectStartAddressTB.Name = "ObjectStartAddressTB";
            this.ObjectStartAddressTB.Size = new System.Drawing.Size(302, 20);
            this.ObjectStartAddressTB.TabIndex = 6;
            // 
            // ObjectStartAddress
            // 
            this.ObjectStartAddress.AutoSize = true;
            this.ObjectStartAddress.Location = new System.Drawing.Point(8, 58);
            this.ObjectStartAddress.Name = "ObjectStartAddress";
            this.ObjectStartAddress.Size = new System.Drawing.Size(67, 13);
            this.ObjectStartAddress.TabIndex = 5;
            this.ObjectStartAddress.Text = "start address";
            // 
            // ObjectPackageNameTB
            // 
            this.ObjectPackageNameTB.Location = new System.Drawing.Point(91, 29);
            this.ObjectPackageNameTB.Name = "ObjectPackageNameTB";
            this.ObjectPackageNameTB.Size = new System.Drawing.Size(302, 20);
            this.ObjectPackageNameTB.TabIndex = 4;
            // 
            // ObjectPacakgeName
            // 
            this.ObjectPacakgeName.AutoSize = true;
            this.ObjectPacakgeName.Location = new System.Drawing.Point(8, 32);
            this.ObjectPacakgeName.Name = "ObjectPacakgeName";
            this.ObjectPacakgeName.Size = new System.Drawing.Size(77, 13);
            this.ObjectPacakgeName.TabIndex = 3;
            this.ObjectPacakgeName.Text = "packageName";
            // 
            // ObjectDependenciesList
            // 
            this.ObjectDependenciesList.FormattingEnabled = true;
            this.ObjectDependenciesList.Location = new System.Drawing.Point(3, 18);
            this.ObjectDependenciesList.Name = "ObjectDependenciesList";
            this.ObjectDependenciesList.Size = new System.Drawing.Size(270, 95);
            this.ObjectDependenciesList.TabIndex = 23;
            // 
            // ObjectDependenciesLabel
            // 
            this.ObjectDependenciesLabel.AutoSize = true;
            this.ObjectDependenciesLabel.Location = new System.Drawing.Point(0, 3);
            this.ObjectDependenciesLabel.Name = "ObjectDependenciesLabel";
            this.ObjectDependenciesLabel.Size = new System.Drawing.Size(137, 13);
            this.ObjectDependenciesLabel.TabIndex = 22;
            this.ObjectDependenciesLabel.Text = "dependencies (click to edit)";
            // 
            // ObjectPicturesLabel
            // 
            this.ObjectPicturesLabel.AutoSize = true;
            this.ObjectPicturesLabel.Location = new System.Drawing.Point(3, 4);
            this.ObjectPicturesLabel.Name = "ObjectPicturesLabel";
            this.ObjectPicturesLabel.Size = new System.Drawing.Size(107, 13);
            this.ObjectPicturesLabel.TabIndex = 20;
            this.ObjectPicturesLabel.Text = "pictures (click to edit)";
            // 
            // SaveDatabaseButton
            // 
            this.SaveDatabaseButton.Location = new System.Drawing.Point(620, 478);
            this.SaveDatabaseButton.Name = "SaveDatabaseButton";
            this.SaveDatabaseButton.Size = new System.Drawing.Size(104, 23);
            this.SaveDatabaseButton.TabIndex = 5;
            this.SaveDatabaseButton.Text = "save database";
            this.SaveDatabaseButton.UseVisualStyleBackColor = true;
            this.SaveDatabaseButton.Click += new System.EventHandler(this.SaveDatabaseButton_Click);
            // 
            // LoadDatabaseButton
            // 
            this.LoadDatabaseButton.Location = new System.Drawing.Point(530, 478);
            this.LoadDatabaseButton.Name = "LoadDatabaseButton";
            this.LoadDatabaseButton.Size = new System.Drawing.Size(84, 23);
            this.LoadDatabaseButton.TabIndex = 6;
            this.LoadDatabaseButton.Text = "load database";
            this.LoadDatabaseButton.UseVisualStyleBackColor = true;
            this.LoadDatabaseButton.Click += new System.EventHandler(this.LoadDatabaseButton_Click);
            // 
            // RemoveEntryButton
            // 
            this.RemoveEntryButton.Location = new System.Drawing.Point(15, 478);
            this.RemoveEntryButton.Name = "RemoveEntryButton";
            this.RemoveEntryButton.Size = new System.Drawing.Size(75, 23);
            this.RemoveEntryButton.TabIndex = 8;
            this.RemoveEntryButton.Text = "remove";
            this.RemoveEntryButton.UseVisualStyleBackColor = true;
            this.RemoveEntryButton.Click += new System.EventHandler(this.RemoveEntryButton_Click);
            // 
            // AddEntryButton
            // 
            this.AddEntryButton.Location = new System.Drawing.Point(247, 478);
            this.AddEntryButton.Name = "AddEntryButton";
            this.AddEntryButton.Size = new System.Drawing.Size(75, 23);
            this.AddEntryButton.TabIndex = 9;
            this.AddEntryButton.Text = "add";
            this.AddEntryButton.UseVisualStyleBackColor = true;
            this.AddEntryButton.Click += new System.EventHandler(this.AddEntryButton_Click);
            // 
            // DBORB
            // 
            this.DBORB.AutoSize = true;
            this.DBORB.Location = new System.Drawing.Point(346, 7);
            this.DBORB.Name = "DBORB";
            this.DBORB.Size = new System.Drawing.Size(48, 17);
            this.DBORB.TabIndex = 1;
            this.DBORB.Text = "DBO";
            this.DBORB.UseVisualStyleBackColor = true;
            this.DBORB.CheckedChanged += new System.EventHandler(this.DBO_CheckedChanged);
            // 
            // GlobalDependencyRB
            // 
            this.GlobalDependencyRB.AutoSize = true;
            this.GlobalDependencyRB.Checked = true;
            this.GlobalDependencyRB.Location = new System.Drawing.Point(12, 7);
            this.GlobalDependencyRB.Name = "GlobalDependencyRB";
            this.GlobalDependencyRB.Size = new System.Drawing.Size(115, 17);
            this.GlobalDependencyRB.TabIndex = 10;
            this.GlobalDependencyRB.TabStop = true;
            this.GlobalDependencyRB.Text = "global dependency";
            this.GlobalDependencyRB.UseVisualStyleBackColor = true;
            this.GlobalDependencyRB.CheckedChanged += new System.EventHandler(this.GlobalDependencyRB_CheckedChanged);
            // 
            // DependencyRB
            // 
            this.DependencyRB.AutoSize = true;
            this.DependencyRB.Location = new System.Drawing.Point(133, 7);
            this.DependencyRB.Name = "DependencyRB";
            this.DependencyRB.Size = new System.Drawing.Size(84, 17);
            this.DependencyRB.TabIndex = 11;
            this.DependencyRB.TabStop = true;
            this.DependencyRB.Text = "dependency";
            this.DependencyRB.UseVisualStyleBackColor = true;
            this.DependencyRB.CheckedChanged += new System.EventHandler(this.DependencyRB_CheckedChanged);
            // 
            // LogicalDependencyRB
            // 
            this.LogicalDependencyRB.AutoSize = true;
            this.LogicalDependencyRB.Location = new System.Drawing.Point(223, 7);
            this.LogicalDependencyRB.Name = "LogicalDependencyRB";
            this.LogicalDependencyRB.Size = new System.Drawing.Size(117, 17);
            this.LogicalDependencyRB.TabIndex = 12;
            this.LogicalDependencyRB.TabStop = true;
            this.LogicalDependencyRB.Text = "logical dependency";
            this.LogicalDependencyRB.UseVisualStyleBackColor = true;
            this.LogicalDependencyRB.CheckedChanged += new System.EventHandler(this.LogicalDependencyRB_CheckedChanged);
            // 
            // OpenDatabaseDialog
            // 
            this.OpenDatabaseDialog.Title = "Open Database";
            // 
            // SaveDatabaseDialog
            // 
            this.SaveDatabaseDialog.Title = "Save Database";
            // 
            // DatabaseSubeditPanel
            // 
            this.DatabaseSubeditPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DatabaseSubeditPanel.Controls.Add(this.DependencyPanel);
            this.DatabaseSubeditPanel.Controls.Add(this.LogicalDependencyPanel);
            this.DatabaseSubeditPanel.Location = new System.Drawing.Point(730, 30);
            this.DatabaseSubeditPanel.Name = "DatabaseSubeditPanel";
            this.DatabaseSubeditPanel.Size = new System.Drawing.Size(284, 364);
            this.DatabaseSubeditPanel.TabIndex = 26;
            // 
            // DependencyPanel
            // 
            this.DependencyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependencyPanel.Controls.Add(this.AddDependencyButton);
            this.DependencyPanel.Controls.Add(this.CurrentDependenciesCB);
            this.DependencyPanel.Controls.Add(this.ObjectDependenciesList);
            this.DependencyPanel.Controls.Add(this.ObjectDependenciesLabel);
            this.DependencyPanel.Controls.Add(this.RemoveDependencyButton);
            this.DependencyPanel.Controls.Add(this.DependencyPackageNameLabel);
            this.DependencyPanel.Location = new System.Drawing.Point(3, 3);
            this.DependencyPanel.Name = "DependencyPanel";
            this.DependencyPanel.Size = new System.Drawing.Size(278, 183);
            this.DependencyPanel.TabIndex = 28;
            // 
            // AddDependencyButton
            // 
            this.AddDependencyButton.Location = new System.Drawing.Point(162, 154);
            this.AddDependencyButton.Name = "AddDependencyButton";
            this.AddDependencyButton.Size = new System.Drawing.Size(48, 23);
            this.AddDependencyButton.TabIndex = 39;
            this.AddDependencyButton.Text = "add";
            this.AddDependencyButton.UseVisualStyleBackColor = true;
            this.AddDependencyButton.Click += new System.EventHandler(this.AddDependencyButton_Click);
            // 
            // CurrentDependenciesCB
            // 
            this.CurrentDependenciesCB.FormattingEnabled = true;
            this.CurrentDependenciesCB.Location = new System.Drawing.Point(5, 132);
            this.CurrentDependenciesCB.Name = "CurrentDependenciesCB";
            this.CurrentDependenciesCB.Size = new System.Drawing.Size(270, 21);
            this.CurrentDependenciesCB.TabIndex = 38;
            // 
            // RemoveDependencyButton
            // 
            this.RemoveDependencyButton.Location = new System.Drawing.Point(216, 154);
            this.RemoveDependencyButton.Name = "RemoveDependencyButton";
            this.RemoveDependencyButton.Size = new System.Drawing.Size(59, 23);
            this.RemoveDependencyButton.TabIndex = 37;
            this.RemoveDependencyButton.Text = "remove";
            this.RemoveDependencyButton.UseVisualStyleBackColor = true;
            this.RemoveDependencyButton.Click += new System.EventHandler(this.RemoveDependencyButton_Click);
            // 
            // DependencyPackageNameLabel
            // 
            this.DependencyPackageNameLabel.AutoSize = true;
            this.DependencyPackageNameLabel.Location = new System.Drawing.Point(3, 116);
            this.DependencyPackageNameLabel.Name = "DependencyPackageNameLabel";
            this.DependencyPackageNameLabel.Size = new System.Drawing.Size(77, 13);
            this.DependencyPackageNameLabel.TabIndex = 35;
            this.DependencyPackageNameLabel.Text = "packageName";
            // 
            // LogicalDependencyPanel
            // 
            this.LogicalDependencyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogicalDependencyPanel.Controls.Add(this.AddLogicalDependencyButton);
            this.LogicalDependencyPanel.Controls.Add(this.CurrentLogicalDependenciesCB);
            this.LogicalDependencyPanel.Controls.Add(this.ObjectLogicalDependenciesLabel);
            this.LogicalDependencyPanel.Controls.Add(this.RemoveLogicalDependencyButton);
            this.LogicalDependencyPanel.Controls.Add(this.LogicalDependencyPackageNameLabel);
            this.LogicalDependencyPanel.Controls.Add(this.LogicalDependnecyNegateFlagCB);
            this.LogicalDependencyPanel.Controls.Add(this.ObjectLogicalDependenciesList);
            this.LogicalDependencyPanel.Location = new System.Drawing.Point(3, 189);
            this.LogicalDependencyPanel.Name = "LogicalDependencyPanel";
            this.LogicalDependencyPanel.Size = new System.Drawing.Size(278, 170);
            this.LogicalDependencyPanel.TabIndex = 28;
            // 
            // AddLogicalDependencyButton
            // 
            this.AddLogicalDependencyButton.Location = new System.Drawing.Point(159, 143);
            this.AddLogicalDependencyButton.Name = "AddLogicalDependencyButton";
            this.AddLogicalDependencyButton.Size = new System.Drawing.Size(48, 23);
            this.AddLogicalDependencyButton.TabIndex = 40;
            this.AddLogicalDependencyButton.Text = "add";
            this.AddLogicalDependencyButton.UseVisualStyleBackColor = true;
            this.AddLogicalDependencyButton.Click += new System.EventHandler(this.AddLogicalDependencyButton_Click);
            // 
            // CurrentLogicalDependenciesCB
            // 
            this.CurrentLogicalDependenciesCB.FormattingEnabled = true;
            this.CurrentLogicalDependenciesCB.Location = new System.Drawing.Point(6, 122);
            this.CurrentLogicalDependenciesCB.Name = "CurrentLogicalDependenciesCB";
            this.CurrentLogicalDependenciesCB.Size = new System.Drawing.Size(266, 21);
            this.CurrentLogicalDependenciesCB.TabIndex = 39;
            // 
            // ObjectLogicalDependenciesLabel
            // 
            this.ObjectLogicalDependenciesLabel.AutoSize = true;
            this.ObjectLogicalDependenciesLabel.Location = new System.Drawing.Point(3, 4);
            this.ObjectLogicalDependenciesLabel.Name = "ObjectLogicalDependenciesLabel";
            this.ObjectLogicalDependenciesLabel.Size = new System.Drawing.Size(170, 13);
            this.ObjectLogicalDependenciesLabel.TabIndex = 24;
            this.ObjectLogicalDependenciesLabel.Text = "logical dependencies (click to edit)";
            // 
            // RemoveLogicalDependencyButton
            // 
            this.RemoveLogicalDependencyButton.Location = new System.Drawing.Point(213, 143);
            this.RemoveLogicalDependencyButton.Name = "RemoveLogicalDependencyButton";
            this.RemoveLogicalDependencyButton.Size = new System.Drawing.Size(59, 23);
            this.RemoveLogicalDependencyButton.TabIndex = 38;
            this.RemoveLogicalDependencyButton.Text = "remove";
            this.RemoveLogicalDependencyButton.UseVisualStyleBackColor = true;
            this.RemoveLogicalDependencyButton.Click += new System.EventHandler(this.RemoveLogicalDependencyButton_Click);
            // 
            // LogicalDependencyPackageNameLabel
            // 
            this.LogicalDependencyPackageNameLabel.AutoSize = true;
            this.LogicalDependencyPackageNameLabel.Location = new System.Drawing.Point(3, 104);
            this.LogicalDependencyPackageNameLabel.Name = "LogicalDependencyPackageNameLabel";
            this.LogicalDependencyPackageNameLabel.Size = new System.Drawing.Size(77, 13);
            this.LogicalDependencyPackageNameLabel.TabIndex = 28;
            this.LogicalDependencyPackageNameLabel.Text = "packageName";
            // 
            // LogicalDependnecyNegateFlagCB
            // 
            this.LogicalDependnecyNegateFlagCB.AutoSize = true;
            this.LogicalDependnecyNegateFlagCB.Location = new System.Drawing.Point(6, 143);
            this.LogicalDependnecyNegateFlagCB.Name = "LogicalDependnecyNegateFlagCB";
            this.LogicalDependnecyNegateFlagCB.Size = new System.Drawing.Size(81, 17);
            this.LogicalDependnecyNegateFlagCB.TabIndex = 34;
            this.LogicalDependnecyNegateFlagCB.Text = "NegateFlag";
            this.LogicalDependnecyNegateFlagCB.UseVisualStyleBackColor = true;
            this.LogicalDependnecyNegateFlagCB.CheckedChanged += new System.EventHandler(this.LogicalDependnecyNegateFlagCB_CheckedChanged);
            // 
            // ObjectLogicalDependenciesList
            // 
            this.ObjectLogicalDependenciesList.FormattingEnabled = true;
            this.ObjectLogicalDependenciesList.Location = new System.Drawing.Point(6, 20);
            this.ObjectLogicalDependenciesList.Name = "ObjectLogicalDependenciesList";
            this.ObjectLogicalDependenciesList.Size = new System.Drawing.Size(267, 82);
            this.ObjectLogicalDependenciesList.TabIndex = 26;
            this.ObjectLogicalDependenciesList.SelectedIndexChanged += new System.EventHandler(this.ObjectLogicalDependenciesList_SelectedIndexChanged);
            // 
            // ObjectPicturesList
            // 
            this.ObjectPicturesList.FormattingEnabled = true;
            this.ObjectPicturesList.Location = new System.Drawing.Point(5, 20);
            this.ObjectPicturesList.Name = "ObjectPicturesList";
            this.ObjectPicturesList.Size = new System.Drawing.Size(269, 56);
            this.ObjectPicturesList.TabIndex = 27;
            // 
            // PicturesTypeLabel
            // 
            this.PicturesTypeLabel.AutoSize = true;
            this.PicturesTypeLabel.Location = new System.Drawing.Point(5, 79);
            this.PicturesTypeLabel.Name = "PicturesTypeLabel";
            this.PicturesTypeLabel.Size = new System.Drawing.Size(27, 13);
            this.PicturesTypeLabel.TabIndex = 30;
            this.PicturesTypeLabel.Text = "type";
            // 
            // PicturesTypeCBox
            // 
            this.PicturesTypeCBox.FormattingEnabled = true;
            this.PicturesTypeCBox.Items.AddRange(new object[] {
            "-none-",
            "1 - picture",
            "2 - video"});
            this.PicturesTypeCBox.Location = new System.Drawing.Point(46, 76);
            this.PicturesTypeCBox.Name = "PicturesTypeCBox";
            this.PicturesTypeCBox.Size = new System.Drawing.Size(151, 21);
            this.PicturesTypeCBox.TabIndex = 31;
            // 
            // PicturesURLLabel
            // 
            this.PicturesURLLabel.AutoSize = true;
            this.PicturesURLLabel.Location = new System.Drawing.Point(5, 101);
            this.PicturesURLLabel.Name = "PicturesURLLabel";
            this.PicturesURLLabel.Size = new System.Drawing.Size(29, 13);
            this.PicturesURLLabel.TabIndex = 32;
            this.PicturesURLLabel.Text = "URL";
            // 
            // PictureURLTB
            // 
            this.PictureURLTB.Location = new System.Drawing.Point(6, 117);
            this.PictureURLTB.Name = "PictureURLTB";
            this.PictureURLTB.Size = new System.Drawing.Size(268, 119);
            this.PictureURLTB.TabIndex = 33;
            this.PictureURLTB.Text = "";
            // 
            // MovePictureButton
            // 
            this.MovePictureButton.Location = new System.Drawing.Point(3, 237);
            this.MovePictureButton.Name = "MovePictureButton";
            this.MovePictureButton.Size = new System.Drawing.Size(92, 23);
            this.MovePictureButton.TabIndex = 39;
            this.MovePictureButton.Text = "move to position";
            this.MovePictureButton.UseVisualStyleBackColor = true;
            this.MovePictureButton.Click += new System.EventHandler(this.MovePictureButton_Click);
            // 
            // PicturePanel
            // 
            this.PicturePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PicturePanel.Controls.Add(this.AddPictureTB);
            this.PicturePanel.Controls.Add(this.MovePictureTB);
            this.PicturePanel.Controls.Add(this.AddPictureButton);
            this.PicturePanel.Controls.Add(this.ApplyPictureEditButton);
            this.PicturePanel.Controls.Add(this.RemovePictureButton);
            this.PicturePanel.Controls.Add(this.MovePictureButton);
            this.PicturePanel.Controls.Add(this.ObjectPicturesLabel);
            this.PicturePanel.Controls.Add(this.PictureURLTB);
            this.PicturePanel.Controls.Add(this.ObjectPicturesList);
            this.PicturePanel.Controls.Add(this.PicturesURLLabel);
            this.PicturePanel.Controls.Add(this.PicturesTypeLabel);
            this.PicturePanel.Controls.Add(this.PicturesTypeCBox);
            this.PicturePanel.Location = new System.Drawing.Point(1020, 30);
            this.PicturePanel.Name = "PicturePanel";
            this.PicturePanel.Size = new System.Drawing.Size(278, 297);
            this.PicturePanel.TabIndex = 27;
            // 
            // AddPictureTB
            // 
            this.AddPictureTB.Location = new System.Drawing.Point(102, 268);
            this.AddPictureTB.Name = "AddPictureTB";
            this.AddPictureTB.Size = new System.Drawing.Size(31, 20);
            this.AddPictureTB.TabIndex = 44;
            // 
            // MovePictureTB
            // 
            this.MovePictureTB.Location = new System.Drawing.Point(102, 239);
            this.MovePictureTB.Name = "MovePictureTB";
            this.MovePictureTB.Size = new System.Drawing.Size(31, 20);
            this.MovePictureTB.TabIndex = 43;
            // 
            // AddPictureButton
            // 
            this.AddPictureButton.Location = new System.Drawing.Point(3, 266);
            this.AddPictureButton.Name = "AddPictureButton";
            this.AddPictureButton.Size = new System.Drawing.Size(92, 23);
            this.AddPictureButton.TabIndex = 42;
            this.AddPictureButton.Text = "add at position";
            this.AddPictureButton.UseVisualStyleBackColor = true;
            this.AddPictureButton.Click += new System.EventHandler(this.AddPictureButton_Click);
            // 
            // ApplyPictureEditButton
            // 
            this.ApplyPictureEditButton.Location = new System.Drawing.Point(210, 266);
            this.ApplyPictureEditButton.Name = "ApplyPictureEditButton";
            this.ApplyPictureEditButton.Size = new System.Drawing.Size(64, 23);
            this.ApplyPictureEditButton.TabIndex = 41;
            this.ApplyPictureEditButton.Text = "apply edit";
            this.ApplyPictureEditButton.UseVisualStyleBackColor = true;
            this.ApplyPictureEditButton.Click += new System.EventHandler(this.ApplyPictureEditButton_Click);
            // 
            // RemovePictureButton
            // 
            this.RemovePictureButton.Location = new System.Drawing.Point(210, 237);
            this.RemovePictureButton.Name = "RemovePictureButton";
            this.RemovePictureButton.Size = new System.Drawing.Size(64, 23);
            this.RemovePictureButton.TabIndex = 40;
            this.RemovePictureButton.Text = "remove";
            this.RemovePictureButton.UseVisualStyleBackColor = true;
            this.RemovePictureButton.Click += new System.EventHandler(this.RemovePictureButton_Click);
            // 
            // MoveButton
            // 
            this.MoveButton.Location = new System.Drawing.Point(133, 478);
            this.MoveButton.Name = "MoveButton";
            this.MoveButton.Size = new System.Drawing.Size(75, 23);
            this.MoveButton.TabIndex = 28;
            this.MoveButton.Text = "move";
            this.MoveButton.UseVisualStyleBackColor = true;
            this.MoveButton.Click += new System.EventHandler(this.MoveButton_Click);
            // 
            // DatabaseEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1301, 512);
            this.Controls.Add(this.MoveButton);
            this.Controls.Add(this.PicturePanel);
            this.Controls.Add(this.DatabaseSubeditPanel);
            this.Controls.Add(this.LogicalDependencyRB);
            this.Controls.Add(this.DependencyRB);
            this.Controls.Add(this.GlobalDependencyRB);
            this.Controls.Add(this.DBORB);
            this.Controls.Add(this.AddEntryButton);
            this.Controls.Add(this.RemoveEntryButton);
            this.Controls.Add(this.LoadDatabaseButton);
            this.Controls.Add(this.SaveDatabaseButton);
            this.Controls.Add(this.DatabaseEditPanel);
            this.Controls.Add(this.DatabasePanelTree);
            this.Name = "DatabaseEditor";
            this.Text = "DatabaseEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DatabaseEditor_FormClosing);
            this.Load += new System.EventHandler(this.DatabaseEditor_Load);
            this.DatabasePanelTree.ResumeLayout(false);
            this.DatabaseEditPanel.ResumeLayout(false);
            this.DatabaseEditPanel.PerformLayout();
            this.DatabaseSubeditPanel.ResumeLayout(false);
            this.DependencyPanel.ResumeLayout(false);
            this.DependencyPanel.PerformLayout();
            this.LogicalDependencyPanel.ResumeLayout(false);
            this.LogicalDependencyPanel.PerformLayout();
            this.PicturePanel.ResumeLayout(false);
            this.PicturePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView DatabaseTreeView;
        private System.Windows.Forms.Label ObjectName;
        private System.Windows.Forms.TextBox ObjectNameTB;
        private System.Windows.Forms.Panel DatabasePanelTree;
        private System.Windows.Forms.Panel DatabaseEditPanel;
        private System.Windows.Forms.TextBox ObjectZipFileTB;
        private System.Windows.Forms.Label ObjectZipFile;
        private System.Windows.Forms.TextBox ObjectEndAddressTB;
        private System.Windows.Forms.Label ObjectEdnAddress;
        private System.Windows.Forms.TextBox ObjectStartAddressTB;
        private System.Windows.Forms.Label ObjectStartAddress;
        private System.Windows.Forms.TextBox ObjectPackageNameTB;
        private System.Windows.Forms.Label ObjectPacakgeName;
        private System.Windows.Forms.ListBox ObjectDependenciesList;
        private System.Windows.Forms.Label ObjectDependenciesLabel;
        private System.Windows.Forms.Label ObjectPicturesLabel;
        private System.Windows.Forms.Label ObjectUpdateNotes;
        private System.Windows.Forms.Label ObjectDescription;
        private System.Windows.Forms.RichTextBox ObjectUpdateNotesTB;
        private System.Windows.Forms.RichTextBox ObjectDescTB;
        private System.Windows.Forms.TextBox ObjectDevURLTB;
        private System.Windows.Forms.Label ObjectDevURL;
        private System.Windows.Forms.Button ApplyChangesButton;
        private System.Windows.Forms.CheckBox ObjectEnabledCheckBox;
        private System.Windows.Forms.Button SaveDatabaseButton;
        private System.Windows.Forms.Button LoadDatabaseButton;
        private System.Windows.Forms.Button RemoveEntryButton;
        private System.Windows.Forms.Button AddEntryButton;
        private System.Windows.Forms.RadioButton DBORB;
        private System.Windows.Forms.RadioButton GlobalDependencyRB;
        private System.Windows.Forms.RadioButton DependencyRB;
        private System.Windows.Forms.RadioButton LogicalDependencyRB;
        private System.Windows.Forms.OpenFileDialog OpenDatabaseDialog;
        private System.Windows.Forms.SaveFileDialog SaveDatabaseDialog;
        private System.Windows.Forms.CheckBox ObjectVisableCheckBox;
        private System.Windows.Forms.ComboBox ObjectTypeComboBox;
        private System.Windows.Forms.Label ObjectType;
        private System.Windows.Forms.Panel DatabaseSubeditPanel;
        private System.Windows.Forms.ListBox ObjectLogicalDependenciesList;
        private System.Windows.Forms.Label ObjectLogicalDependenciesLabel;
        private System.Windows.Forms.ListBox ObjectPicturesList;
        private System.Windows.Forms.CheckBox ObjectAppendExtractionCB;
        private System.Windows.Forms.Panel PicturePanel;
        private System.Windows.Forms.Button MovePictureButton;
        private System.Windows.Forms.RichTextBox PictureURLTB;
        private System.Windows.Forms.Label PicturesURLLabel;
        private System.Windows.Forms.Label PicturesTypeLabel;
        private System.Windows.Forms.ComboBox PicturesTypeCBox;
        private System.Windows.Forms.Panel DependencyPanel;
        private System.Windows.Forms.Button AddDependencyButton;
        private System.Windows.Forms.ComboBox CurrentDependenciesCB;
        private System.Windows.Forms.Button RemoveDependencyButton;
        private System.Windows.Forms.Label DependencyPackageNameLabel;
        private System.Windows.Forms.Panel LogicalDependencyPanel;
        private System.Windows.Forms.Button AddLogicalDependencyButton;
        private System.Windows.Forms.ComboBox CurrentLogicalDependenciesCB;
        private System.Windows.Forms.Button RemoveLogicalDependencyButton;
        private System.Windows.Forms.Label LogicalDependencyPackageNameLabel;
        private System.Windows.Forms.CheckBox LogicalDependnecyNegateFlagCB;
        private System.Windows.Forms.TextBox AddPictureTB;
        private System.Windows.Forms.TextBox MovePictureTB;
        private System.Windows.Forms.Button AddPictureButton;
        private System.Windows.Forms.Button ApplyPictureEditButton;
        private System.Windows.Forms.Button RemovePictureButton;
        private System.Windows.Forms.Button MoveButton;
    }
}