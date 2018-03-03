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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseEditor));
            this.DatabaseTreeView = new System.Windows.Forms.TreeView();
            this.ObjectName = new System.Windows.Forms.Label();
            this.ObjectNameTB = new System.Windows.Forms.TextBox();
            this.DatabasePanelTree = new System.Windows.Forms.Panel();
            this.DatabaseEditPanel = new System.Windows.Forms.Panel();
            this.ObjectLastUpdatedLabel = new System.Windows.Forms.Label();
            this.DownloadZipfileButton = new System.Windows.Forms.Button();
            this.ObjectVersionTB = new System.Windows.Forms.TextBox();
            this.ObjectVersionLabel = new System.Windows.Forms.Label();
            this.ObjectPropertiesTabControl = new System.Windows.Forms.TabControl();
            this.DescriptionTabPage = new System.Windows.Forms.TabPage();
            this.ObjectDescription = new System.Windows.Forms.Label();
            this.ObjectDescTB = new System.Windows.Forms.RichTextBox();
            this.ObjectUpdateNotesTB = new System.Windows.Forms.RichTextBox();
            this.ObjectUpdateNotes = new System.Windows.Forms.Label();
            this.DependenciesTabPage = new System.Windows.Forms.TabPage();
            this.LogicalDependencyPanel = new System.Windows.Forms.Panel();
            this.AddLogicalDependencyButton = new System.Windows.Forms.Button();
            this.CurrentLogicalDependenciesCB = new System.Windows.Forms.ComboBox();
            this.ObjectLogicalDependenciesLabel = new System.Windows.Forms.Label();
            this.RemoveLogicalDependencyButton = new System.Windows.Forms.Button();
            this.LogicalDependencyPackageNameLabel = new System.Windows.Forms.Label();
            this.LogicalDependnecyNegateFlagCB = new System.Windows.Forms.CheckBox();
            this.ObjectLogicalDependenciesList = new System.Windows.Forms.ListBox();
            this.DependencyPanel = new System.Windows.Forms.Panel();
            this.AddDependencyButton = new System.Windows.Forms.Button();
            this.CurrentDependenciesCB = new System.Windows.Forms.ComboBox();
            this.ObjectDependenciesList = new System.Windows.Forms.ListBox();
            this.ObjectDependenciesLabel = new System.Windows.Forms.Label();
            this.RemoveDependencyButton = new System.Windows.Forms.Button();
            this.DependencyPackageNameLabel = new System.Windows.Forms.Label();
            this.PictureTabPage = new System.Windows.Forms.TabPage();
            this.AddPictureTB = new System.Windows.Forms.TextBox();
            this.MovePictureTB = new System.Windows.Forms.TextBox();
            this.AddPictureButton = new System.Windows.Forms.Button();
            this.ApplyPictureEditButton = new System.Windows.Forms.Button();
            this.RemovePictureButton = new System.Windows.Forms.Button();
            this.MovePictureButton = new System.Windows.Forms.Button();
            this.ObjectPicturesLabel = new System.Windows.Forms.Label();
            this.PictureURLTB = new System.Windows.Forms.RichTextBox();
            this.ObjectPicturesList = new System.Windows.Forms.ListBox();
            this.PicturesURLLabel = new System.Windows.Forms.Label();
            this.PicturesTypeLabel = new System.Windows.Forms.Label();
            this.PicturesTypeCBox = new System.Windows.Forms.ComboBox();
            this.UserDatasTabPage = new System.Windows.Forms.TabPage();
            this.EditUserdatasButton = new System.Windows.Forms.Button();
            this.ObjectUserdatasTB = new System.Windows.Forms.TextBox();
            this.AddUserdatasButton = new System.Windows.Forms.Button();
            this.ObjectUserdatasList = new System.Windows.Forms.ListBox();
            this.ObjectUserdatasLabel = new System.Windows.Forms.Label();
            this.RemoveUserdatasButton = new System.Windows.Forms.Button();
            this.ObjectAppendExtractionCB = new System.Windows.Forms.CheckBox();
            this.ObjectVisibleCheckBox = new System.Windows.Forms.CheckBox();
            this.ObjectTypeComboBox = new System.Windows.Forms.ComboBox();
            this.ObjectType = new System.Windows.Forms.Label();
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
            this.ObjectPackageName = new System.Windows.Forms.Label();
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
            this.MoveButton = new System.Windows.Forms.Button();
            this.SearchBox = new System.Windows.Forms.ComboBox();
            this.ObjectUserdatasToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ObjectLevel = new System.Windows.Forms.Label();
            this.ObjectLevelLabel = new System.Windows.Forms.Label();
            this.DatabasePanelTree.SuspendLayout();
            this.DatabaseEditPanel.SuspendLayout();
            this.ObjectPropertiesTabControl.SuspendLayout();
            this.DescriptionTabPage.SuspendLayout();
            this.DependenciesTabPage.SuspendLayout();
            this.LogicalDependencyPanel.SuspendLayout();
            this.DependencyPanel.SuspendLayout();
            this.PictureTabPage.SuspendLayout();
            this.UserDatasTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // DatabaseTreeView
            // 
            this.DatabaseTreeView.BackColor = System.Drawing.SystemColors.Window;
            this.DatabaseTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DatabaseTreeView.ForeColor = System.Drawing.Color.Blue;
            this.DatabaseTreeView.HideSelection = false;
            this.DatabaseTreeView.Location = new System.Drawing.Point(0, 0);
            this.DatabaseTreeView.Name = "DatabaseTreeView";
            this.DatabaseTreeView.Size = new System.Drawing.Size(503, 598);
            this.DatabaseTreeView.TabIndex = 0;
            this.DatabaseTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DatabaseTreeView_AfterSelect);
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
            this.ObjectNameTB.Size = new System.Drawing.Size(447, 20);
            this.ObjectNameTB.TabIndex = 2;
            // 
            // DatabasePanelTree
            // 
            this.DatabasePanelTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DatabasePanelTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DatabasePanelTree.Controls.Add(this.DatabaseTreeView);
            this.DatabasePanelTree.Location = new System.Drawing.Point(12, 30);
            this.DatabasePanelTree.Name = "DatabasePanelTree";
            this.DatabasePanelTree.Size = new System.Drawing.Size(505, 600);
            this.DatabasePanelTree.TabIndex = 3;
            // 
            // DatabaseEditPanel
            // 
            this.DatabaseEditPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DatabaseEditPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DatabaseEditPanel.Controls.Add(this.ObjectLevelLabel);
            this.DatabaseEditPanel.Controls.Add(this.ObjectLevel);
            this.DatabaseEditPanel.Controls.Add(this.ObjectLastUpdatedLabel);
            this.DatabaseEditPanel.Controls.Add(this.DownloadZipfileButton);
            this.DatabaseEditPanel.Controls.Add(this.ObjectVersionTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectVersionLabel);
            this.DatabaseEditPanel.Controls.Add(this.ObjectPropertiesTabControl);
            this.DatabaseEditPanel.Controls.Add(this.ObjectAppendExtractionCB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectVisibleCheckBox);
            this.DatabaseEditPanel.Controls.Add(this.ObjectTypeComboBox);
            this.DatabaseEditPanel.Controls.Add(this.ObjectType);
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
            this.DatabaseEditPanel.Controls.Add(this.ObjectPackageName);
            this.DatabaseEditPanel.Controls.Add(this.ObjectNameTB);
            this.DatabaseEditPanel.Controls.Add(this.ObjectName);
            this.DatabaseEditPanel.Location = new System.Drawing.Point(523, 30);
            this.DatabaseEditPanel.Name = "DatabaseEditPanel";
            this.DatabaseEditPanel.Size = new System.Drawing.Size(547, 600);
            this.DatabaseEditPanel.TabIndex = 4;
            // 
            // ObjectLastUpdatedLabel
            // 
            this.ObjectLastUpdatedLabel.AutoSize = true;
            this.ObjectLastUpdatedLabel.Location = new System.Drawing.Point(297, 136);
            this.ObjectLastUpdatedLabel.Name = "ObjectLastUpdatedLabel";
            this.ObjectLastUpdatedLabel.Size = new System.Drawing.Size(71, 13);
            this.ObjectLastUpdatedLabel.TabIndex = 33;
            this.ObjectLastUpdatedLabel.Text = "last updated: ";
            // 
            // DownloadZipfileButton
            // 
            this.DownloadZipfileButton.Enabled = false;
            this.DownloadZipfileButton.Location = new System.Drawing.Point(202, 208);
            this.DownloadZipfileButton.Name = "DownloadZipfileButton";
            this.DownloadZipfileButton.Size = new System.Drawing.Size(83, 27);
            this.DownloadZipfileButton.TabIndex = 32;
            this.DownloadZipfileButton.Text = "Download Zip";
            this.DownloadZipfileButton.UseVisualStyleBackColor = true;
            this.DownloadZipfileButton.Click += new System.EventHandler(this.DownloadZipfileButton_Click);
            // 
            // ObjectVersionTB
            // 
            this.ObjectVersionTB.Location = new System.Drawing.Point(91, 133);
            this.ObjectVersionTB.Name = "ObjectVersionTB";
            this.ObjectVersionTB.Size = new System.Drawing.Size(194, 20);
            this.ObjectVersionTB.TabIndex = 31;
            // 
            // ObjectVersionLabel
            // 
            this.ObjectVersionLabel.AutoSize = true;
            this.ObjectVersionLabel.Location = new System.Drawing.Point(8, 136);
            this.ObjectVersionLabel.Name = "ObjectVersionLabel";
            this.ObjectVersionLabel.Size = new System.Drawing.Size(41, 13);
            this.ObjectVersionLabel.TabIndex = 30;
            this.ObjectVersionLabel.Text = "version";
            // 
            // ObjectPropertiesTabControl
            // 
            this.ObjectPropertiesTabControl.Controls.Add(this.DescriptionTabPage);
            this.ObjectPropertiesTabControl.Controls.Add(this.DependenciesTabPage);
            this.ObjectPropertiesTabControl.Controls.Add(this.PictureTabPage);
            this.ObjectPropertiesTabControl.Controls.Add(this.UserDatasTabPage);
            this.ObjectPropertiesTabControl.Location = new System.Drawing.Point(3, 241);
            this.ObjectPropertiesTabControl.Name = "ObjectPropertiesTabControl";
            this.ObjectPropertiesTabControl.SelectedIndex = 0;
            this.ObjectPropertiesTabControl.Size = new System.Drawing.Size(539, 354);
            this.ObjectPropertiesTabControl.TabIndex = 29;
            // 
            // DescriptionTabPage
            // 
            this.DescriptionTabPage.Controls.Add(this.ObjectDescription);
            this.DescriptionTabPage.Controls.Add(this.ObjectDescTB);
            this.DescriptionTabPage.Controls.Add(this.ObjectUpdateNotesTB);
            this.DescriptionTabPage.Controls.Add(this.ObjectUpdateNotes);
            this.DescriptionTabPage.Location = new System.Drawing.Point(4, 22);
            this.DescriptionTabPage.Name = "DescriptionTabPage";
            this.DescriptionTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DescriptionTabPage.Size = new System.Drawing.Size(531, 328);
            this.DescriptionTabPage.TabIndex = 1;
            this.DescriptionTabPage.Text = "Description";
            this.DescriptionTabPage.UseVisualStyleBackColor = true;
            // 
            // ObjectDescription
            // 
            this.ObjectDescription.AutoSize = true;
            this.ObjectDescription.Location = new System.Drawing.Point(3, 3);
            this.ObjectDescription.Name = "ObjectDescription";
            this.ObjectDescription.Size = new System.Drawing.Size(58, 13);
            this.ObjectDescription.TabIndex = 18;
            this.ObjectDescription.Text = "description";
            // 
            // ObjectDescTB
            // 
            this.ObjectDescTB.Location = new System.Drawing.Point(6, 19);
            this.ObjectDescTB.Name = "ObjectDescTB";
            this.ObjectDescTB.Size = new System.Drawing.Size(519, 164);
            this.ObjectDescTB.TabIndex = 16;
            this.ObjectDescTB.Text = "";
            // 
            // ObjectUpdateNotesTB
            // 
            this.ObjectUpdateNotesTB.Location = new System.Drawing.Point(6, 206);
            this.ObjectUpdateNotesTB.Name = "ObjectUpdateNotesTB";
            this.ObjectUpdateNotesTB.Size = new System.Drawing.Size(521, 89);
            this.ObjectUpdateNotesTB.TabIndex = 17;
            this.ObjectUpdateNotesTB.Text = "";
            // 
            // ObjectUpdateNotes
            // 
            this.ObjectUpdateNotes.AutoSize = true;
            this.ObjectUpdateNotes.Location = new System.Drawing.Point(5, 190);
            this.ObjectUpdateNotes.Name = "ObjectUpdateNotes";
            this.ObjectUpdateNotes.Size = new System.Drawing.Size(91, 13);
            this.ObjectUpdateNotes.TabIndex = 19;
            this.ObjectUpdateNotes.Text = "update comments";
            // 
            // DependenciesTabPage
            // 
            this.DependenciesTabPage.Controls.Add(this.LogicalDependencyPanel);
            this.DependenciesTabPage.Controls.Add(this.DependencyPanel);
            this.DependenciesTabPage.Location = new System.Drawing.Point(4, 22);
            this.DependenciesTabPage.Name = "DependenciesTabPage";
            this.DependenciesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DependenciesTabPage.Size = new System.Drawing.Size(531, 328);
            this.DependenciesTabPage.TabIndex = 2;
            this.DependenciesTabPage.Text = "Dependencies";
            this.DependenciesTabPage.UseVisualStyleBackColor = true;
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
            this.LogicalDependencyPanel.Location = new System.Drawing.Point(6, 162);
            this.LogicalDependencyPanel.Name = "LogicalDependencyPanel";
            this.LogicalDependencyPanel.Size = new System.Drawing.Size(519, 136);
            this.LogicalDependencyPanel.TabIndex = 28;
            // 
            // AddLogicalDependencyButton
            // 
            this.AddLogicalDependencyButton.Location = new System.Drawing.Point(401, 109);
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
            this.CurrentLogicalDependenciesCB.Location = new System.Drawing.Point(3, 84);
            this.CurrentLogicalDependenciesCB.Name = "CurrentLogicalDependenciesCB";
            this.CurrentLogicalDependenciesCB.Size = new System.Drawing.Size(511, 21);
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
            this.RemoveLogicalDependencyButton.Location = new System.Drawing.Point(455, 109);
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
            this.LogicalDependencyPackageNameLabel.Location = new System.Drawing.Point(3, 66);
            this.LogicalDependencyPackageNameLabel.Name = "LogicalDependencyPackageNameLabel";
            this.LogicalDependencyPackageNameLabel.Size = new System.Drawing.Size(78, 13);
            this.LogicalDependencyPackageNameLabel.TabIndex = 28;
            this.LogicalDependencyPackageNameLabel.Text = "PackageName";
            // 
            // LogicalDependnecyNegateFlagCB
            // 
            this.LogicalDependnecyNegateFlagCB.AutoSize = true;
            this.LogicalDependnecyNegateFlagCB.Location = new System.Drawing.Point(314, 113);
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
            this.ObjectLogicalDependenciesList.Location = new System.Drawing.Point(3, 20);
            this.ObjectLogicalDependenciesList.Name = "ObjectLogicalDependenciesList";
            this.ObjectLogicalDependenciesList.Size = new System.Drawing.Size(511, 43);
            this.ObjectLogicalDependenciesList.TabIndex = 26;
            this.ObjectLogicalDependenciesList.SelectedIndexChanged += new System.EventHandler(this.ObjectLogicalDependenciesList_SelectedIndexChanged);
            this.ObjectLogicalDependenciesList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ObjectLogicalDependenciesList_MouseDoubleClick);
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
            this.DependencyPanel.Location = new System.Drawing.Point(6, 6);
            this.DependencyPanel.Name = "DependencyPanel";
            this.DependencyPanel.Size = new System.Drawing.Size(519, 150);
            this.DependencyPanel.TabIndex = 28;
            // 
            // AddDependencyButton
            // 
            this.AddDependencyButton.Location = new System.Drawing.Point(401, 120);
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
            this.CurrentDependenciesCB.Location = new System.Drawing.Point(3, 93);
            this.CurrentDependenciesCB.Name = "CurrentDependenciesCB";
            this.CurrentDependenciesCB.Size = new System.Drawing.Size(511, 21);
            this.CurrentDependenciesCB.TabIndex = 38;
            // 
            // ObjectDependenciesList
            // 
            this.ObjectDependenciesList.FormattingEnabled = true;
            this.ObjectDependenciesList.Location = new System.Drawing.Point(3, 18);
            this.ObjectDependenciesList.Name = "ObjectDependenciesList";
            this.ObjectDependenciesList.Size = new System.Drawing.Size(511, 56);
            this.ObjectDependenciesList.TabIndex = 23;
            this.ObjectDependenciesList.SelectedIndexChanged += new System.EventHandler(this.ObjectDependenciesList_SelectedIndexChanged);
            this.ObjectDependenciesList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ObjectDependenciesList_MouseDoubleClick);
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
            // RemoveDependencyButton
            // 
            this.RemoveDependencyButton.Location = new System.Drawing.Point(455, 120);
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
            this.DependencyPackageNameLabel.Location = new System.Drawing.Point(3, 77);
            this.DependencyPackageNameLabel.Name = "DependencyPackageNameLabel";
            this.DependencyPackageNameLabel.Size = new System.Drawing.Size(78, 13);
            this.DependencyPackageNameLabel.TabIndex = 35;
            this.DependencyPackageNameLabel.Text = "PackageName";
            // 
            // PictureTabPage
            // 
            this.PictureTabPage.Controls.Add(this.AddPictureTB);
            this.PictureTabPage.Controls.Add(this.MovePictureTB);
            this.PictureTabPage.Controls.Add(this.AddPictureButton);
            this.PictureTabPage.Controls.Add(this.ApplyPictureEditButton);
            this.PictureTabPage.Controls.Add(this.RemovePictureButton);
            this.PictureTabPage.Controls.Add(this.MovePictureButton);
            this.PictureTabPage.Controls.Add(this.ObjectPicturesLabel);
            this.PictureTabPage.Controls.Add(this.PictureURLTB);
            this.PictureTabPage.Controls.Add(this.ObjectPicturesList);
            this.PictureTabPage.Controls.Add(this.PicturesURLLabel);
            this.PictureTabPage.Controls.Add(this.PicturesTypeLabel);
            this.PictureTabPage.Controls.Add(this.PicturesTypeCBox);
            this.PictureTabPage.Location = new System.Drawing.Point(4, 22);
            this.PictureTabPage.Name = "PictureTabPage";
            this.PictureTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.PictureTabPage.Size = new System.Drawing.Size(531, 328);
            this.PictureTabPage.TabIndex = 3;
            this.PictureTabPage.Text = "Pictures";
            this.PictureTabPage.UseVisualStyleBackColor = true;
            // 
            // AddPictureTB
            // 
            this.AddPictureTB.Location = new System.Drawing.Point(111, 266);
            this.AddPictureTB.Name = "AddPictureTB";
            this.AddPictureTB.Size = new System.Drawing.Size(31, 20);
            this.AddPictureTB.TabIndex = 56;
            // 
            // MovePictureTB
            // 
            this.MovePictureTB.Location = new System.Drawing.Point(246, 266);
            this.MovePictureTB.Name = "MovePictureTB";
            this.MovePictureTB.Size = new System.Drawing.Size(31, 20);
            this.MovePictureTB.TabIndex = 55;
            // 
            // AddPictureButton
            // 
            this.AddPictureButton.Location = new System.Drawing.Point(13, 256);
            this.AddPictureButton.Name = "AddPictureButton";
            this.AddPictureButton.Size = new System.Drawing.Size(92, 39);
            this.AddPictureButton.TabIndex = 54;
            this.AddPictureButton.Text = "add at position (0 index based)";
            this.AddPictureButton.UseVisualStyleBackColor = true;
            this.AddPictureButton.Click += new System.EventHandler(this.AddPictureButton_Click);
            // 
            // ApplyPictureEditButton
            // 
            this.ApplyPictureEditButton.Location = new System.Drawing.Point(457, 256);
            this.ApplyPictureEditButton.Name = "ApplyPictureEditButton";
            this.ApplyPictureEditButton.Size = new System.Drawing.Size(64, 23);
            this.ApplyPictureEditButton.TabIndex = 53;
            this.ApplyPictureEditButton.Text = "apply edit";
            this.ApplyPictureEditButton.UseVisualStyleBackColor = true;
            this.ApplyPictureEditButton.Click += new System.EventHandler(this.ApplyPictureEditButton_Click);
            // 
            // RemovePictureButton
            // 
            this.RemovePictureButton.Location = new System.Drawing.Point(385, 256);
            this.RemovePictureButton.Name = "RemovePictureButton";
            this.RemovePictureButton.Size = new System.Drawing.Size(64, 23);
            this.RemovePictureButton.TabIndex = 52;
            this.RemovePictureButton.Text = "remove";
            this.RemovePictureButton.UseVisualStyleBackColor = true;
            this.RemovePictureButton.Click += new System.EventHandler(this.RemovePictureButton_Click);
            // 
            // MovePictureButton
            // 
            this.MovePictureButton.Location = new System.Drawing.Point(148, 255);
            this.MovePictureButton.Name = "MovePictureButton";
            this.MovePictureButton.Size = new System.Drawing.Size(92, 41);
            this.MovePictureButton.TabIndex = 51;
            this.MovePictureButton.Text = "move to position (0 index based)";
            this.MovePictureButton.UseVisualStyleBackColor = true;
            this.MovePictureButton.Click += new System.EventHandler(this.MovePictureButton_Click);
            // 
            // ObjectPicturesLabel
            // 
            this.ObjectPicturesLabel.AutoSize = true;
            this.ObjectPicturesLabel.Location = new System.Drawing.Point(10, 7);
            this.ObjectPicturesLabel.Name = "ObjectPicturesLabel";
            this.ObjectPicturesLabel.Size = new System.Drawing.Size(107, 13);
            this.ObjectPicturesLabel.TabIndex = 45;
            this.ObjectPicturesLabel.Text = "pictures (click to edit)";
            // 
            // PictureURLTB
            // 
            this.PictureURLTB.DetectUrls = false;
            this.PictureURLTB.Location = new System.Drawing.Point(13, 156);
            this.PictureURLTB.Name = "PictureURLTB";
            this.PictureURLTB.Size = new System.Drawing.Size(508, 94);
            this.PictureURLTB.TabIndex = 50;
            this.PictureURLTB.Text = "";
            // 
            // ObjectPicturesList
            // 
            this.ObjectPicturesList.FormattingEnabled = true;
            this.ObjectPicturesList.Location = new System.Drawing.Point(12, 23);
            this.ObjectPicturesList.Name = "ObjectPicturesList";
            this.ObjectPicturesList.Size = new System.Drawing.Size(509, 82);
            this.ObjectPicturesList.TabIndex = 46;
            this.ObjectPicturesList.SelectedIndexChanged += new System.EventHandler(this.ObjectPicturesList_SelectedIndexChanged);
            this.ObjectPicturesList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ObjectPicturesList_MouseDoubleClick);
            // 
            // PicturesURLLabel
            // 
            this.PicturesURLLabel.AutoSize = true;
            this.PicturesURLLabel.Location = new System.Drawing.Point(10, 140);
            this.PicturesURLLabel.Name = "PicturesURLLabel";
            this.PicturesURLLabel.Size = new System.Drawing.Size(29, 13);
            this.PicturesURLLabel.TabIndex = 49;
            this.PicturesURLLabel.Text = "URL";
            // 
            // PicturesTypeLabel
            // 
            this.PicturesTypeLabel.AutoSize = true;
            this.PicturesTypeLabel.Location = new System.Drawing.Point(10, 114);
            this.PicturesTypeLabel.Name = "PicturesTypeLabel";
            this.PicturesTypeLabel.Size = new System.Drawing.Size(27, 13);
            this.PicturesTypeLabel.TabIndex = 47;
            this.PicturesTypeLabel.Text = "type";
            // 
            // PicturesTypeCBox
            // 
            this.PicturesTypeCBox.FormattingEnabled = true;
            this.PicturesTypeCBox.Items.AddRange(new object[] {
            "-none-",
            "1 - picture",
            "2 - webpage",
            "3 - mediaFile",
            "4 - HTML"});
            this.PicturesTypeCBox.Location = new System.Drawing.Point(48, 111);
            this.PicturesTypeCBox.Name = "PicturesTypeCBox";
            this.PicturesTypeCBox.Size = new System.Drawing.Size(151, 21);
            this.PicturesTypeCBox.TabIndex = 48;
            // 
            // UserDatasTabPage
            // 
            this.UserDatasTabPage.Controls.Add(this.EditUserdatasButton);
            this.UserDatasTabPage.Controls.Add(this.ObjectUserdatasTB);
            this.UserDatasTabPage.Controls.Add(this.AddUserdatasButton);
            this.UserDatasTabPage.Controls.Add(this.ObjectUserdatasList);
            this.UserDatasTabPage.Controls.Add(this.ObjectUserdatasLabel);
            this.UserDatasTabPage.Controls.Add(this.RemoveUserdatasButton);
            this.UserDatasTabPage.Location = new System.Drawing.Point(4, 22);
            this.UserDatasTabPage.Name = "UserDatasTabPage";
            this.UserDatasTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.UserDatasTabPage.Size = new System.Drawing.Size(531, 328);
            this.UserDatasTabPage.TabIndex = 4;
            this.UserDatasTabPage.Text = "Userdatas";
            this.UserDatasTabPage.UseVisualStyleBackColor = true;
            // 
            // EditUserdatasButton
            // 
            this.EditUserdatasButton.Location = new System.Drawing.Point(407, 119);
            this.EditUserdatasButton.Name = "EditUserdatasButton";
            this.EditUserdatasButton.Size = new System.Drawing.Size(48, 23);
            this.EditUserdatasButton.TabIndex = 45;
            this.EditUserdatasButton.Text = "edit";
            this.EditUserdatasButton.UseVisualStyleBackColor = true;
            this.EditUserdatasButton.Click += new System.EventHandler(this.EditUserdatasButton_Click);
            // 
            // ObjectUserdatasTB
            // 
            this.ObjectUserdatasTB.Location = new System.Drawing.Point(9, 93);
            this.ObjectUserdatasTB.Name = "ObjectUserdatasTB";
            this.ObjectUserdatasTB.Size = new System.Drawing.Size(511, 20);
            this.ObjectUserdatasTB.TabIndex = 44;
            this.ObjectUserdatasToolTip.SetToolTip(this.ObjectUserdatasTB, "Wildcards as * and ? are allowed at the filename");
            // 
            // AddUserdatasButton
            // 
            this.AddUserdatasButton.Location = new System.Drawing.Point(353, 119);
            this.AddUserdatasButton.Name = "AddUserdatasButton";
            this.AddUserdatasButton.Size = new System.Drawing.Size(48, 23);
            this.AddUserdatasButton.TabIndex = 43;
            this.AddUserdatasButton.Text = "add";
            this.AddUserdatasButton.UseVisualStyleBackColor = true;
            this.AddUserdatasButton.Click += new System.EventHandler(this.AddUserdatasButton_Click);
            // 
            // ObjectUserdatasList
            // 
            this.ObjectUserdatasList.FormattingEnabled = true;
            this.ObjectUserdatasList.Location = new System.Drawing.Point(9, 18);
            this.ObjectUserdatasList.Name = "ObjectUserdatasList";
            this.ObjectUserdatasList.Size = new System.Drawing.Size(511, 69);
            this.ObjectUserdatasList.TabIndex = 41;
            this.ObjectUserdatasToolTip.SetToolTip(this.ObjectUserdatasList, "Wildcards as * and ? are allowed at the filename");
            this.ObjectUserdatasList.SelectedIndexChanged += new System.EventHandler(this.ObjectUserdatasList_SelectedIndexChanged);
            // 
            // ObjectUserdatasLabel
            // 
            this.ObjectUserdatasLabel.AutoSize = true;
            this.ObjectUserdatasLabel.Location = new System.Drawing.Point(6, 3);
            this.ObjectUserdatasLabel.Name = "ObjectUserdatasLabel";
            this.ObjectUserdatasLabel.Size = new System.Drawing.Size(116, 13);
            this.ObjectUserdatasLabel.TabIndex = 40;
            this.ObjectUserdatasLabel.Text = "userdatas (click to edit)";
            // 
            // RemoveUserdatasButton
            // 
            this.RemoveUserdatasButton.Location = new System.Drawing.Point(461, 119);
            this.RemoveUserdatasButton.Name = "RemoveUserdatasButton";
            this.RemoveUserdatasButton.Size = new System.Drawing.Size(59, 23);
            this.RemoveUserdatasButton.TabIndex = 42;
            this.RemoveUserdatasButton.Text = "remove";
            this.RemoveUserdatasButton.UseVisualStyleBackColor = true;
            this.RemoveUserdatasButton.Click += new System.EventHandler(this.RemoveUserdatasButton_Click);
            // 
            // ObjectAppendExtractionCB
            // 
            this.ObjectAppendExtractionCB.AutoSize = true;
            this.ObjectAppendExtractionCB.Location = new System.Drawing.Point(243, 185);
            this.ObjectAppendExtractionCB.Name = "ObjectAppendExtractionCB";
            this.ObjectAppendExtractionCB.Size = new System.Drawing.Size(109, 17);
            this.ObjectAppendExtractionCB.TabIndex = 25;
            this.ObjectAppendExtractionCB.Text = "appendExtraction";
            this.ObjectAppendExtractionCB.UseVisualStyleBackColor = true;
            // 
            // ObjectVisibleCheckBox
            // 
            this.ObjectVisibleCheckBox.AutoSize = true;
            this.ObjectVisibleCheckBox.Location = new System.Drawing.Point(358, 185);
            this.ObjectVisibleCheckBox.Name = "ObjectVisibleCheckBox";
            this.ObjectVisibleCheckBox.Size = new System.Drawing.Size(55, 17);
            this.ObjectVisibleCheckBox.TabIndex = 23;
            this.ObjectVisibleCheckBox.Text = "visible";
            this.ObjectVisibleCheckBox.UseVisualStyleBackColor = true;
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
            this.ObjectTypeComboBox.Location = new System.Drawing.Point(91, 183);
            this.ObjectTypeComboBox.Name = "ObjectTypeComboBox";
            this.ObjectTypeComboBox.Size = new System.Drawing.Size(146, 21);
            this.ObjectTypeComboBox.TabIndex = 21;
            // 
            // ObjectType
            // 
            this.ObjectType.AutoSize = true;
            this.ObjectType.Location = new System.Drawing.Point(8, 186);
            this.ObjectType.Name = "ObjectType";
            this.ObjectType.Size = new System.Drawing.Size(27, 13);
            this.ObjectType.TabIndex = 20;
            this.ObjectType.Text = "type";
            // 
            // ObjectDevURLTB
            // 
            this.ObjectDevURLTB.Location = new System.Drawing.Point(91, 158);
            this.ObjectDevURLTB.Name = "ObjectDevURLTB";
            this.ObjectDevURLTB.Size = new System.Drawing.Size(447, 20);
            this.ObjectDevURLTB.TabIndex = 15;
            this.ObjectDevURLTB.DoubleClick += new System.EventHandler(this.callTextBoxURL_DoubleClick);
            // 
            // ObjectDevURL
            // 
            this.ObjectDevURL.AutoSize = true;
            this.ObjectDevURL.Location = new System.Drawing.Point(8, 161);
            this.ObjectDevURL.Name = "ObjectDevURL";
            this.ObjectDevURL.Size = new System.Drawing.Size(50, 13);
            this.ObjectDevURL.TabIndex = 14;
            this.ObjectDevURL.Text = "dev URL";
            // 
            // ApplyChangesButton
            // 
            this.ApplyChangesButton.Location = new System.Drawing.Point(291, 208);
            this.ApplyChangesButton.Name = "ApplyChangesButton";
            this.ApplyChangesButton.Size = new System.Drawing.Size(244, 27);
            this.ApplyChangesButton.TabIndex = 13;
            this.ApplyChangesButton.Text = "apply changes (from above and description tab)";
            this.ApplyChangesButton.UseVisualStyleBackColor = true;
            this.ApplyChangesButton.Click += new System.EventHandler(this.ApplyChangesButton_Click);
            // 
            // ObjectEnabledCheckBox
            // 
            this.ObjectEnabledCheckBox.AutoSize = true;
            this.ObjectEnabledCheckBox.Location = new System.Drawing.Point(423, 185);
            this.ObjectEnabledCheckBox.Name = "ObjectEnabledCheckBox";
            this.ObjectEnabledCheckBox.Size = new System.Drawing.Size(65, 17);
            this.ObjectEnabledCheckBox.TabIndex = 12;
            this.ObjectEnabledCheckBox.Text = "Enabled";
            this.ObjectEnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // ObjectZipFileTB
            // 
            this.ObjectZipFileTB.Location = new System.Drawing.Point(91, 107);
            this.ObjectZipFileTB.Name = "ObjectZipFileTB";
            this.ObjectZipFileTB.Size = new System.Drawing.Size(447, 20);
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
            this.ObjectEndAddressTB.Size = new System.Drawing.Size(447, 20);
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
            this.ObjectStartAddressTB.Size = new System.Drawing.Size(447, 20);
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
            this.ObjectPackageNameTB.Size = new System.Drawing.Size(447, 20);
            this.ObjectPackageNameTB.TabIndex = 4;
            // 
            // ObjectPackageName
            // 
            this.ObjectPackageName.AutoSize = true;
            this.ObjectPackageName.Location = new System.Drawing.Point(8, 32);
            this.ObjectPackageName.Name = "ObjectPackageName";
            this.ObjectPackageName.Size = new System.Drawing.Size(77, 13);
            this.ObjectPackageName.TabIndex = 3;
            this.ObjectPackageName.Text = "packageName";
            // 
            // SaveDatabaseButton
            // 
            this.SaveDatabaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveDatabaseButton.Location = new System.Drawing.Point(964, 637);
            this.SaveDatabaseButton.Name = "SaveDatabaseButton";
            this.SaveDatabaseButton.Size = new System.Drawing.Size(104, 23);
            this.SaveDatabaseButton.TabIndex = 5;
            this.SaveDatabaseButton.Text = "save database";
            this.SaveDatabaseButton.UseVisualStyleBackColor = true;
            this.SaveDatabaseButton.Click += new System.EventHandler(this.SaveDatabaseButton_Click);
            // 
            // LoadDatabaseButton
            // 
            this.LoadDatabaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadDatabaseButton.Location = new System.Drawing.Point(874, 637);
            this.LoadDatabaseButton.Name = "LoadDatabaseButton";
            this.LoadDatabaseButton.Size = new System.Drawing.Size(84, 23);
            this.LoadDatabaseButton.TabIndex = 6;
            this.LoadDatabaseButton.Text = "load database";
            this.LoadDatabaseButton.UseVisualStyleBackColor = true;
            this.LoadDatabaseButton.Click += new System.EventHandler(this.LoadDatabaseButton_Click);
            // 
            // RemoveEntryButton
            // 
            this.RemoveEntryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RemoveEntryButton.Location = new System.Drawing.Point(12, 637);
            this.RemoveEntryButton.Name = "RemoveEntryButton";
            this.RemoveEntryButton.Size = new System.Drawing.Size(75, 23);
            this.RemoveEntryButton.TabIndex = 8;
            this.RemoveEntryButton.Text = "remove";
            this.RemoveEntryButton.UseVisualStyleBackColor = true;
            this.RemoveEntryButton.Click += new System.EventHandler(this.RemoveEntryButton_Click);
            // 
            // AddEntryButton
            // 
            this.AddEntryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddEntryButton.Location = new System.Drawing.Point(174, 637);
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
            this.DBORB.Size = new System.Drawing.Size(165, 17);
            this.DBORB.TabIndex = 1;
            this.DBORB.Text = "DatabaseObject (mod/config)";
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
            // MoveButton
            // 
            this.MoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MoveButton.Location = new System.Drawing.Point(93, 637);
            this.MoveButton.Name = "MoveButton";
            this.MoveButton.Size = new System.Drawing.Size(75, 23);
            this.MoveButton.TabIndex = 28;
            this.MoveButton.Text = "move";
            this.MoveButton.UseVisualStyleBackColor = true;
            this.MoveButton.Click += new System.EventHandler(this.MoveButton_Click);
            // 
            // SearchBox
            // 
            this.SearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchBox.FormattingEnabled = true;
            this.SearchBox.Location = new System.Drawing.Point(523, 6);
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(547, 21);
            this.SearchBox.TabIndex = 29;
            this.SearchBox.SelectionChangeCommitted += new System.EventHandler(this.SearchBox_SelectionChangeCommitted);
            this.SearchBox.TextUpdate += new System.EventHandler(this.SearchBox_TextUpdate);
            // 
            // ObjectLevel
            // 
            this.ObjectLevel.AutoSize = true;
            this.ObjectLevel.Location = new System.Drawing.Point(6, 211);
            this.ObjectLevel.Name = "ObjectLevel";
            this.ObjectLevel.Size = new System.Drawing.Size(29, 13);
            this.ObjectLevel.TabIndex = 34;
            this.ObjectLevel.Text = "level";
            // 
            // ObjectLevelLabel
            // 
            this.ObjectLevelLabel.AutoSize = true;
            this.ObjectLevelLabel.Location = new System.Drawing.Point(88, 211);
            this.ObjectLevelLabel.Name = "ObjectLevelLabel";
            this.ObjectLevelLabel.Size = new System.Drawing.Size(23, 13);
            this.ObjectLevelLabel.TabIndex = 35;
            this.ObjectLevelLabel.Text = "null";
            // 
            // DatabaseEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1080, 672);
            this.Controls.Add(this.SearchBox);
            this.Controls.Add(this.MoveButton);
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1090, 705);
            this.Name = "DatabaseEditor";
            this.Text = "DatabaseEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DatabaseEditor_FormClosing);
            this.Load += new System.EventHandler(this.DatabaseEditor_Load);
            this.DatabasePanelTree.ResumeLayout(false);
            this.DatabaseEditPanel.ResumeLayout(false);
            this.DatabaseEditPanel.PerformLayout();
            this.ObjectPropertiesTabControl.ResumeLayout(false);
            this.DescriptionTabPage.ResumeLayout(false);
            this.DescriptionTabPage.PerformLayout();
            this.DependenciesTabPage.ResumeLayout(false);
            this.LogicalDependencyPanel.ResumeLayout(false);
            this.LogicalDependencyPanel.PerformLayout();
            this.DependencyPanel.ResumeLayout(false);
            this.DependencyPanel.PerformLayout();
            this.PictureTabPage.ResumeLayout(false);
            this.PictureTabPage.PerformLayout();
            this.UserDatasTabPage.ResumeLayout(false);
            this.UserDatasTabPage.PerformLayout();
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
        private System.Windows.Forms.Label ObjectPackageName;
        private System.Windows.Forms.ListBox ObjectDependenciesList;
        private System.Windows.Forms.Label ObjectDependenciesLabel;
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
        private System.Windows.Forms.CheckBox ObjectVisibleCheckBox;
        private System.Windows.Forms.ComboBox ObjectTypeComboBox;
        private System.Windows.Forms.Label ObjectType;
        private System.Windows.Forms.ListBox ObjectLogicalDependenciesList;
        private System.Windows.Forms.Label ObjectLogicalDependenciesLabel;
        private System.Windows.Forms.CheckBox ObjectAppendExtractionCB;
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
        private System.Windows.Forms.Button MoveButton;
        private System.Windows.Forms.TabControl ObjectPropertiesTabControl;
        private System.Windows.Forms.TabPage DescriptionTabPage;
        private System.Windows.Forms.TabPage DependenciesTabPage;
        private System.Windows.Forms.TabPage PictureTabPage;
        private System.Windows.Forms.TabPage UserDatasTabPage;
        private System.Windows.Forms.TextBox ObjectUserdatasTB;
        private System.Windows.Forms.Button AddUserdatasButton;
        private System.Windows.Forms.ListBox ObjectUserdatasList;
        private System.Windows.Forms.Label ObjectUserdatasLabel;
        private System.Windows.Forms.Button RemoveUserdatasButton;
        private System.Windows.Forms.Button EditUserdatasButton;
        private System.Windows.Forms.TextBox AddPictureTB;
        private System.Windows.Forms.TextBox MovePictureTB;
        private System.Windows.Forms.Button AddPictureButton;
        private System.Windows.Forms.Button ApplyPictureEditButton;
        private System.Windows.Forms.Button RemovePictureButton;
        private System.Windows.Forms.Button MovePictureButton;
        private System.Windows.Forms.Label ObjectPicturesLabel;
        private System.Windows.Forms.RichTextBox PictureURLTB;
        private System.Windows.Forms.ListBox ObjectPicturesList;
        private System.Windows.Forms.Label PicturesURLLabel;
        private System.Windows.Forms.Label PicturesTypeLabel;
        private System.Windows.Forms.ComboBox PicturesTypeCBox;
        private System.Windows.Forms.TextBox ObjectVersionTB;
        private System.Windows.Forms.Label ObjectVersionLabel;
        private System.Windows.Forms.ComboBox SearchBox;
        private System.Windows.Forms.Button DownloadZipfileButton;
        private System.Windows.Forms.Label ObjectLastUpdatedLabel;
        private System.Windows.Forms.ToolTip ObjectUserdatasToolTip;
        private System.Windows.Forms.Label ObjectLevelLabel;
        private System.Windows.Forms.Label ObjectLevel;
    }
}