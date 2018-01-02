using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace RelhaxModpack
{
    //an enum to control the form editor mode
    public enum EditorMode
    {
        GlobalDependnecy = 0,
        Dependency = 1,
        LogicalDependency = 2,
        DBO = 3,
    };
    public partial class DatabaseEditor : Form
    {
        //basic lists
        private List<Category> ParsedCategoryList;
        private List<Dependency> GlobalDependencies;
        private List<Dependency> Dependencies;
        private List<LogicalDependency> LogicalDependencies;
        //other stuff
        private string DatabaseLocation = "";
        private Dependency SelectedGlobalDependency;
        private Dependency SelectedDependency;
        private LogicalDependency SelectedLogicalDependency;
        private SelectableDatabasePackage SelectedDatabaseObject;
        private Category SelectedCategory;
        private int currentSelectedIndex = -1;
        // string GameVersion = ""; => changed to Settings.TanksVersion => could be accessed from ANY place of code
        // string OnlineFolderVersion = ""; => changed to Settings.tanksOnlineFolderVersion => could be accessed from ANY place of code
        private StringBuilder InUseSB;
        private List<Config> ListThatContainsConfig;
        private bool UnsavedModifications = false;
        List<string> allPackageNames = new List<string>();

        private EditorMode DatabaseEditorMode;

        public DatabaseEditor()
        {
            InitializeComponent();
            Settings.LoadSettings();
        }

        private void DatabaseEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UnsavedModifications)
            {
                if (MessageBox.Show("You have unsaved changes, return to editor?", "unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Stop) == DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
        }
        //hook into the database editor loading
        private void DatabaseEditor_Load(object sender, EventArgs e)
        {
            DatabaseEditorMode = EditorMode.GlobalDependnecy;
            ResetUI();
        }
        //loads the database depending on the mode of the radiobuttons
        private void DisplayDatabase(bool resetUI = true)
        {
            SearchBox.Items.Clear();
            allPackageNames.Clear();
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (Dependency d in GlobalDependencies)
                {
                    SearchBox.Items.Add(d.PackageName);
                    allPackageNames.Add(d.PackageName);
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (Dependency d in Dependencies)
                {
                    SearchBox.Items.Add(d.PackageName);
                    allPackageNames.Add(d.PackageName);
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (LogicalDependency d in LogicalDependencies)
                {
                    SearchBox.Items.Add(d.PackageName);
                    allPackageNames.Add(d.PackageName);
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (Category cat in ParsedCategoryList)
                {
                    SearchBox.Items.Add(cat.Name);
                    allPackageNames.Add(cat.Name);
                    DatabaseTreeNode catNode = new DatabaseTreeNode(cat, 4);
                    DatabaseTreeView.Nodes.Add(catNode);
                    foreach (Mod m in cat.Mods)
                    {
                        SearchBox.Items.Add(m.PackageName);
                        allPackageNames.Add(m.PackageName);
                        DatabaseTreeNode modNode = new DatabaseTreeNode(m, (int)DatabaseEditorMode);
                        catNode.Nodes.Add(modNode);
                        if (SelectedDatabaseObject != null && SelectedDatabaseObject.PackageName.Equals(m.PackageName))
                            modNode.EnsureVisible();
                        DisplayDatabaseConfigs(modNode, m.configs);
                    }
                }

            }
            if (resetUI)
                ResetUI();
        }
        private void ResetUI()
        {
            SelectedGlobalDependency = null;
            SelectedDependency = null;
            SelectedLogicalDependency = null;
            SelectedDatabaseObject = null;
            SelectedCategory = null;

            ObjectNameTB.Enabled = false;
            ObjectNameTB.Text = "";

            ObjectPackageNameTB.Enabled = false;
            ObjectPackageNameTB.Text = "";

            ObjectStartAddressTB.Enabled = false;
            ObjectStartAddressTB.Text = "";

            ObjectEndAddressTB.Enabled = false;
            ObjectEndAddressTB.Text = "";

            ObjectZipFileTB.Enabled = false;
            ObjectZipFileTB.Text = "";

            ObjectDevURLTB.Enabled = false;
            ObjectDevURLTB.Text = "";

            ObjectVersionTB.Enabled = false;
            ObjectVersionTB.Text = "";

            ObjectTypeComboBox.Enabled = false;
            ObjectTypeComboBox.SelectedIndex = 0;

            ObjectEnabledCheckBox.Enabled = false;
            ObjectEnabledCheckBox.Checked = false;

            ObjectVisibleCheckBox.Enabled = false;
            ObjectVisibleCheckBox.Checked = false;

            ObjectAppendExtractionCB.Enabled = false;
            ObjectAppendExtractionCB.Checked = false;

            DownloadZipfileButton.Enabled = false;

            DescriptionTabPage.Enabled = false;
            DependenciesTabPage.Enabled = false;
            PictureTabPage.Enabled = false;
            UserDatasTabPage.Enabled = false;
        }
        private void DisplayDatabaseConfigs(DatabaseTreeNode parrent, List<Config> configs)
        {
            foreach (Config c in configs)
            {
                SearchBox.Items.Add(c.PackageName);
                allPackageNames.Add(c.PackageName);
                DatabaseTreeNode ConfigParrent = new DatabaseTreeNode(c, (int)DatabaseEditorMode);
                parrent.Nodes.Add(ConfigParrent);
                if (SelectedDatabaseObject != null && SelectedDatabaseObject.PackageName.Equals(c.PackageName))
                    ConfigParrent.EnsureVisible();
                DisplayDatabaseConfigs(ConfigParrent, c.configs);
            }
        }
        //show the load database dialog and load the database
        private void LoadDatabaseButton_Click(object sender, EventArgs e)
        {
            string workingDirectory = Path.Combine(string.IsNullOrEmpty(Settings.CustomModInfoPath) ? Application.StartupPath : Settings.CustomModInfoPath);
            if (Directory.Exists(workingDirectory))
            {
                OpenDatabaseDialog.InitialDirectory = workingDirectory;
            }
            if (OpenDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            DatabaseLocation = OpenDatabaseDialog.FileName;
            if (!File.Exists(DatabaseLocation))
                return;
            //for the folder version: //modInfoAlpha.xml/@version
            Settings.TanksVersion = XMLUtils.GetXMLElementAttributeFromFile(DatabaseLocation, "//modInfoAlpha.xml/@version");
            //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
            Settings.TanksOnlineFolderVersion = XMLUtils.GetXMLElementAttributeFromFile(DatabaseLocation, "//modInfoAlpha.xml/@onlineFolder");
            this.Text = String.Format("DatabaseEditor      GameVersion: {0}    OnlineFolder: {1}", Settings.TanksVersion, Settings.TanksOnlineFolderVersion);
            GlobalDependencies = new List<Dependency>();
            Dependencies = new List<Dependency>();
            LogicalDependencies = new List<LogicalDependency>();
            ParsedCategoryList = new List<Category>();
            XMLUtils.CreateModStructure(DatabaseLocation, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList);
            DatabaseEditorMode = EditorMode.GlobalDependnecy;
            this.DisplayDatabase();
        }
        //show the save database dialog and save the database
        private void SaveDatabaseButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded", "CRITICAL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (SaveDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            DatabaseLocation = SaveDatabaseDialog.FileName;
            XMLUtils.SaveDatabase(DatabaseLocation, Settings.TanksVersion, Settings.TanksOnlineFolderVersion, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList);
            UnsavedModifications = false;
        }
        //Apply all changes from the form
        private void ApplyChangesButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (MessageBox.Show("Confirm you wish to apply changes", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int duplicates = 0;
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                if (!SelectedGlobalDependency.PackageName.Equals(ObjectPackageNameTB.Text))
                    duplicates++;
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                if (!SelectedDependency.PackageName.Equals(ObjectPackageNameTB.Text))
                    duplicates++;
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                if (!SelectedLogicalDependency.PackageName.Equals(ObjectPackageNameTB.Text))
                    duplicates++;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                if (SelectedCategory == null)
                {
                    if (!SelectedDatabaseObject.PackageName.Equals(ObjectPackageNameTB.Text))
                        duplicates++;
                }
            }
            if (DuplicatePackageName(this.GlobalDependencies,this.Dependencies,this.LogicalDependencies,this.ParsedCategoryList, ObjectPackageNameTB.Text,duplicates))
            {
                MessageBox.Show(string.Format("Package name: {0}, already exists", ObjectPackageNameTB.Text));
                return;
            }
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                int index = GlobalDependencies.IndexOf(SelectedGlobalDependency);
                SelectedGlobalDependency.PackageName = ObjectPackageNameTB.Text;
                SelectedGlobalDependency.StartAddress = ObjectStartAddressTB.Text;
                SelectedGlobalDependency.EndAddress = ObjectEndAddressTB.Text;
                SelectedGlobalDependency.DevURL = ObjectDevURLTB.Text;
                if (!SelectedGlobalDependency.ZipFile.Equals(ObjectZipFileTB.Text))
                {
                    SelectedGlobalDependency.CRC = "f";
                    SelectedGlobalDependency.ZipFile = ObjectZipFileTB.Text;
                    SelectedGlobalDependency.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                    ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedGlobalDependency.Timestamp));
                }
                SelectedGlobalDependency.Enabled = ObjectEnabledCheckBox.Checked;
                SelectedGlobalDependency.AppendExtraction = ObjectAppendExtractionCB.Checked;
                GlobalDependencies[index] = SelectedGlobalDependency;
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                int index = Dependencies.IndexOf(SelectedDependency);
                SelectedDependency.PackageName = ObjectPackageNameTB.Text;
                SelectedDependency.StartAddress = ObjectStartAddressTB.Text;
                SelectedDependency.EndAddress = ObjectEndAddressTB.Text;
                SelectedDependency.DevURL = ObjectDevURLTB.Text;
                if (!SelectedDependency.ZipFile.Equals(ObjectZipFileTB.Text))
                {
                    SelectedDependency.CRC = "f";
                    SelectedDependency.ZipFile = ObjectZipFileTB.Text;
                    SelectedDependency.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                    ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedDependency.Timestamp));
                }
                SelectedDependency.Enabled = ObjectEnabledCheckBox.Checked;
                SelectedDependency.AppendExtraction = ObjectAppendExtractionCB.Checked;
                Dependencies[index] = SelectedDependency;
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                int index = LogicalDependencies.IndexOf(SelectedLogicalDependency);
                SelectedLogicalDependency.PackageName = ObjectPackageNameTB.Text;
                SelectedLogicalDependency.StartAddress = ObjectStartAddressTB.Text;
                SelectedLogicalDependency.EndAddress = ObjectEndAddressTB.Text;
                SelectedLogicalDependency.DevURL = ObjectDevURLTB.Text;
                if (!SelectedLogicalDependency.ZipFile.Equals(ObjectZipFileTB.Text))
                {
                    SelectedLogicalDependency.CRC = "f";
                    SelectedLogicalDependency.ZipFile = ObjectZipFileTB.Text;
                    SelectedLogicalDependency.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                    ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedLogicalDependency.Timestamp));
                }
                SelectedLogicalDependency.Enabled = ObjectEnabledCheckBox.Checked;
                LogicalDependencies[index] = SelectedLogicalDependency;
            }
            else if (SelectedCategory != null)
            {
                int index = ParsedCategoryList.IndexOf(SelectedCategory);
                SelectedCategory.Name = ObjectNameTB.Text;
                switch (ObjectTypeComboBox.SelectedIndex)
                {
                    case 1:
                        SelectedCategory.SelectionType = "single1";
                        break;
                    case 2:
                        SelectedCategory.SelectionType = "single1";
                        break;
                    case 3:
                        SelectedCategory.SelectionType = "single1";
                        break;
                    case 4:
                        SelectedCategory.SelectionType = "multi";
                        break;
                    default:
                        SelectedCategory.SelectionType = "multi";
                        break;
                }
                ParsedCategoryList[index] = SelectedCategory;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                if (SelectedDatabaseObject is Mod)
                {
                    Mod m = (Mod)SelectedDatabaseObject;
                    List<Mod> ModList = ListContainsMod(m);
                    int index = ModList.IndexOf(m);
                    //make changes
                    m.Name = ObjectNameTB.Text;
                    m.PackageName = ObjectPackageNameTB.Text;
                    m.StartAddress = ObjectStartAddressTB.Text;
                    m.EndAddress = ObjectEndAddressTB.Text;
                    if (!SelectedDatabaseObject.ZipFile.Equals(ObjectZipFileTB.Text))
                    {
                        m.CRC = "f";
                        m.ZipFile = ObjectZipFileTB.Text;
                        m.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(m.Timestamp));
                    }
                    m.DevURL = ObjectDevURLTB.Text;
                    m.Version = ObjectVersionTB.Text;
                    m.Enabled = ObjectEnabledCheckBox.Checked;
                    m.Visible = ObjectVisibleCheckBox.Checked;
                    m.Description = ObjectDescTB.Text;
                    m.UpdateComment = ObjectUpdateNotesTB.Text;
                    ModList[index] = m;
                }
                else if (SelectedDatabaseObject is Config)
                {
                    if (ObjectTypeComboBox.SelectedIndex == -1 || ObjectTypeComboBox.SelectedIndex == 0)
                    {
                        MessageBox.Show("Invalid Index of config type");
                        return;
                    }
                    ListThatContainsConfig = null;
                    Config cfg = (Config)SelectedDatabaseObject;
                    ListContainsConfig(cfg);
                    if (ListThatContainsConfig != null)
                    {
                        int index = ListThatContainsConfig.IndexOf(cfg);
                        //make changes
                        cfg.Name = ObjectNameTB.Text;
                        cfg.PackageName = ObjectPackageNameTB.Text;
                        cfg.StartAddress = ObjectStartAddressTB.Text;
                        cfg.EndAddress = ObjectEndAddressTB.Text;
                        if (!SelectedDatabaseObject.ZipFile.Equals(ObjectZipFileTB.Text))
                        {
                            cfg.CRC = "f";
                            cfg.ZipFile = ObjectZipFileTB.Text;
                            cfg.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                            ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(cfg.Timestamp));
                        }
                        cfg.DevURL = ObjectDevURLTB.Text;
                        cfg.Version = ObjectVersionTB.Text;
                        switch (ObjectTypeComboBox.SelectedIndex)
                        {
                            case 1:
                                cfg.Type = "single1";
                                break;
                            case 2:
                                cfg.Type = "single_dropdown1";
                                break;
                            case 3:
                                cfg.Type = "single_dropdown2";
                                break;
                            case 4:
                                cfg.Type = "multi";
                                break;
                        }
                        cfg.Enabled = ObjectEnabledCheckBox.Checked;
                        cfg.Visible = ObjectVisibleCheckBox.Checked;
                        cfg.Description = ObjectDescTB.Text;
                        cfg.UpdateComment = ObjectUpdateNotesTB.Text;
                        ListThatContainsConfig[index] = cfg;
                    }
                }
            }
            this.DisplayDatabase(false);
            UnsavedModifications = true;
        }
        private List<Mod> ListContainsMod(Mod mod)
        {
            foreach (Category cat in ParsedCategoryList)
            {
                if (cat.Mods.Contains(mod))
                    return cat.Mods;
            }
            return null;
        }
        private void ListContainsConfig(Config cfg)
        {
            foreach (Category cat in ParsedCategoryList)
            {
                foreach (Mod m in cat.Mods)
                {
                    if (m.configs.Contains(cfg) && ListThatContainsConfig == null)
                    {
                        ListThatContainsConfig = m.configs;
                        return;
                    }
                    if (m.configs.Count > 0)
                    {
                        ListContainsConfigRecursive(m.configs, cfg);
                    }
                }
            }
        }
        private void ListContainsConfigRecursive(List<Config> cfgList, Config cfg)
        {
            foreach (Config c in cfgList)
            {
                if (c.configs.Contains(cfg) && ListThatContainsConfig == null)
                {
                    ListThatContainsConfig = c.configs;
                    return;
                }
                if (c.configs.Count > 0)
                {
                    ListContainsConfigRecursive(c.configs, cfg);
                }
            }
        }
        //mode set to globalDependency
        private void GlobalDependencyRB_CheckedChanged(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            RadioButton rb1 = (RadioButton)sender;
            if (rb1.Checked)
            {
                DatabaseEditorMode = EditorMode.GlobalDependnecy;
                clearDatabaseDetailsUI();
                DisplayDatabase();
            }
        }
        //mode set to dependency
        private void DependencyRB_CheckedChanged(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            RadioButton rb1 = (RadioButton)sender;
            if (rb1.Checked)
            {
                DatabaseEditorMode = EditorMode.Dependency;
                clearDatabaseDetailsUI();
                DisplayDatabase();
            }
        }
        //mode set to logicalDependency
        private void LogicalDependencyRB_CheckedChanged(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            RadioButton rb1 = (RadioButton)sender;
            if (rb1.Checked)
            {
                DatabaseEditorMode = EditorMode.LogicalDependency;
                clearDatabaseDetailsUI();
                DisplayDatabase();
            }
        }
        //mode set to DBO
        private void DBO_CheckedChanged(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            RadioButton rb1 = (RadioButton)sender;
            if (rb1.Checked)
            {
                DatabaseEditorMode = EditorMode.DBO;
                clearDatabaseDetailsUI();
                DisplayDatabase();
            }
        }

        private void clearDatabaseDetailsUI()
        {
            ObjectNameTB.Text = "";
            ObjectPackageNameTB.Text = "";
            ObjectStartAddressTB.Text = "";
            ObjectEndAddressTB.Text = "";
            ObjectZipFileTB.Text = "";
            ObjectVersionTB.Text = "";
            ObjectLastUpdatedLabel.Text = "";
            ObjectDevURLTB.Text = "";
            ObjectTypeComboBox.SelectedIndex = -1;
            ObjectAppendExtractionCB.Checked = false;
            ObjectVisibleCheckBox.Checked = false;
            ObjectEnabledCheckBox.Checked = false;

            // Description tab
            ObjectDescTB.Text = "";
            ObjectUpdateNotesTB.Text = "";

            // Dependencies tab
            ObjectDependenciesList.DataSource = null;
            ObjectDependenciesList.Items.Clear();
            ObjectDependenciesLabel.Text = "dependencies (click to edit)";
            CurrentDependenciesCB.DataSource = null;
            CurrentDependenciesCB.Items.Clear();
            CurrentDependenciesCB.SelectedIndex = -1;
            ObjectLogicalDependenciesList.DataSource = null;
            ObjectLogicalDependenciesList.Items.Clear();
            CurrentLogicalDependenciesCB.DataSource = null;
            CurrentLogicalDependenciesCB.Items.Clear();
            CurrentLogicalDependenciesCB.SelectedIndex = -1;
            LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
            LogicalDependnecyNegateFlagCB.Checked = false;
            LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;

            // Pictures tab
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            PicturesTypeCBox.SelectedIndex = -1;
            PictureURLTB.Text = "";

            // Userdata tab
            ObjectUserdatasList.DataSource = null;
            ObjectUserdatasList.Items.Clear();
            ObjectUserdatasTB.Text = "";
        }

        private void DatabaseTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                DatabaseTreeView.SelectedNode = e.Node;
                DatabaseTreeView.Focus();
                e.Node.ForeColor = System.Drawing.Color.Blue;
                currentSelectedIndex = DatabaseTreeView.SelectedNode.Index;
                //DatabaseTreeView.SelectedNode.BackColor = Color.Blue;
                //DatabaseTreeView.SelectedNode.ForeColor = Color.Blue;
                clearDatabaseDetailsUI();
                DatabaseTreeNode node = (DatabaseTreeNode)DatabaseTreeView.SelectedNode;
                if (node.GlobalDependency != null)
                {
                    SelectedGlobalDependency = node.GlobalDependency;
                    SelectedDependency = null;
                    SelectedLogicalDependency = null;
                    SelectedDatabaseObject = null;
                    SelectedCategory = null;

                    ObjectNameTB.Enabled = false;

                    ObjectPackageNameTB.Enabled = true;
                    ObjectPackageNameTB.Text = node.GlobalDependency.PackageName;

                    ObjectStartAddressTB.Enabled = true;
                    ObjectStartAddressTB.Text = node.GlobalDependency.StartAddress;

                    ObjectEndAddressTB.Enabled = true;
                    ObjectEndAddressTB.Text = node.GlobalDependency.EndAddress;

                    ObjectZipFileTB.Enabled = true;
                    ObjectZipFileTB.Text = node.GlobalDependency.ZipFile;

                    ObjectVersionTB.Enabled = false;

                    if (node.GlobalDependency.Timestamp > 0)
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(node.GlobalDependency.Timestamp));
                    else
                        ObjectLastUpdatedLabel.Text = "last updated: (none)";

                    ObjectDevURLTB.Enabled = true;
                    ObjectDevURLTB.Text = SelectedGlobalDependency.DevURL;

                    ObjectTypeComboBox.Enabled = false;
                    ObjectTypeComboBox.SelectedIndex = 0;

                    ObjectEnabledCheckBox.Enabled = true;
                    ObjectEnabledCheckBox.Checked = node.GlobalDependency.Enabled;

                    ObjectVisibleCheckBox.Enabled = false;

                    ObjectAppendExtractionCB.Enabled = true;
                    ObjectAppendExtractionCB.Checked = node.GlobalDependency.AppendExtraction;

                    DownloadZipfileButton.Enabled = true;

                    DescriptionTabPage.Enabled = false;

                    DependenciesTabPage.Enabled = false;
                    ObjectDependenciesLabel.Text = "dependencies (click to edit)";
                    PictureTabPage.Enabled = false;
                    UserDatasTabPage.Enabled = false;
                }
                else if (node.Dependency != null)
                {
                    SelectedGlobalDependency = null;
                    SelectedDependency = node.Dependency;
                    SelectedLogicalDependency = null;
                    SelectedDatabaseObject = null;
                    SelectedCategory = null;

                    ObjectNameTB.Enabled = false;

                    ObjectPackageNameTB.Enabled = true;
                    ObjectPackageNameTB.Text = SelectedDependency.PackageName;

                    ObjectStartAddressTB.Enabled = true;
                    ObjectStartAddressTB.Text = SelectedDependency.StartAddress;

                    ObjectEndAddressTB.Enabled = true;
                    ObjectEndAddressTB.Text = SelectedDependency.EndAddress;

                    ObjectZipFileTB.Enabled = true;
                    ObjectZipFileTB.Text = SelectedDependency.ZipFile;

                    ObjectVersionTB.Enabled = false;

                    if (SelectedDependency.Timestamp > 0)
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedDependency.Timestamp));
                    else
                        ObjectLastUpdatedLabel.Text = "last updated: (none)";

                    ObjectDevURLTB.Enabled = true;
                    ObjectDevURLTB.Text = SelectedDependency.DevURL;

                    ObjectTypeComboBox.Enabled = false;
                    ObjectTypeComboBox.SelectedIndex = 0;

                    ObjectEnabledCheckBox.Enabled = true;
                    ObjectEnabledCheckBox.Checked = SelectedDependency.Enabled;

                    ObjectVisibleCheckBox.Enabled = false;

                    ObjectAppendExtractionCB.Enabled = true;
                    ObjectAppendExtractionCB.Checked = SelectedDependency.AppendExtraction;

                    DownloadZipfileButton.Enabled = true;

                    DescriptionTabPage.Enabled = false;

                    DependenciesTabPage.Enabled = true;
                    DependencyPanel.Enabled = true;
                    ObjectDependenciesLabel.Text = "Objects that use this dependency...";
                    ObjectDependenciesLabel.Enabled = true;
                    ObjectDependenciesList.Enabled = true;
                    ObjectDependenciesList.DataSource = BuildDatabaseLogic(SelectedDependency);

                    LogicalDependencyPanel.Enabled = true;
                    ObjectLogicalDependenciesList.DataSource = null;
                    ObjectLogicalDependenciesList.Items.Clear();
                    ObjectLogicalDependenciesList.DataSource = SelectedDependency.LogicalDependencies;
                    CurrentLogicalDependenciesCB.DataSource = null;
                    CurrentLogicalDependenciesCB.Items.Clear();
                    CurrentLogicalDependenciesCB.DataSource = LogicalDependencies;
                    CurrentLogicalDependenciesCB.SelectedIndex = -1;
                    LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
                    LogicalDependnecyNegateFlagCB.Checked = false;
                    LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;

                    PictureTabPage.Enabled = false;
                    UserDatasTabPage.Enabled = false;
                }
                else if (node.LogicalDependency != null)
                {
                    SelectedGlobalDependency = null;
                    SelectedDependency = null;
                    SelectedLogicalDependency = node.LogicalDependency;
                    SelectedDatabaseObject = null;
                    SelectedCategory = null;

                    ObjectNameTB.Enabled = false;

                    ObjectPackageNameTB.Enabled = true;
                    ObjectPackageNameTB.Text = SelectedLogicalDependency.PackageName;

                    ObjectStartAddressTB.Enabled = true;
                    ObjectStartAddressTB.Text = SelectedLogicalDependency.StartAddress;

                    ObjectEndAddressTB.Enabled = true;
                    ObjectEndAddressTB.Text = SelectedLogicalDependency.EndAddress;

                    ObjectZipFileTB.Enabled = true;
                    ObjectZipFileTB.Text = SelectedLogicalDependency.ZipFile;

                    ObjectVersionTB.Enabled = false;

                    if (SelectedLogicalDependency.Timestamp > 0)
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedLogicalDependency.Timestamp));
                    else
                        ObjectLastUpdatedLabel.Text = "last updated: (none)";

                    ObjectDevURLTB.Enabled = true;
                    ObjectDevURLTB.Text = SelectedLogicalDependency.DevURL;

                    ObjectTypeComboBox.Enabled = false;
                    ObjectTypeComboBox.SelectedIndex = 0;

                    ObjectEnabledCheckBox.Enabled = true;
                    ObjectEnabledCheckBox.Checked = SelectedLogicalDependency.Enabled;

                    ObjectVisibleCheckBox.Enabled = false;

                    ObjectAppendExtractionCB.Enabled = false;

                    DownloadZipfileButton.Enabled = true;

                    DescriptionTabPage.Enabled = false;

                    DependenciesTabPage.Enabled = true;
                    DependencyPanel.Enabled = true;
                    ObjectDependenciesLabel.Text = "Objects that use this logical dependency...";
                    SelectedLogicalDependency.DatabasePackageLogic.Clear();
                    BuildDatabaseLogic(SelectedLogicalDependency);
                    ObjectDependenciesList.DataSource = null;
                    ObjectDependenciesList.Items.Clear();
                    ObjectDependenciesList.DataSource = SelectedLogicalDependency.DatabasePackageLogic;
                    PictureTabPage.Enabled = false;
                    UserDatasTabPage.Enabled = false;
                }
                else if (node.DatabaseObject != null)
                {
                    SelectedGlobalDependency = null;
                    SelectedDependency = null;
                    SelectedLogicalDependency = null;
                    SelectedDatabaseObject = node.DatabaseObject;
                    SelectedCategory = null;

                    ObjectNameTB.Enabled = true;
                    ObjectNameTB.Text = SelectedDatabaseObject.Name;

                    ObjectPackageNameTB.Enabled = true;
                    ObjectPackageNameTB.Text = SelectedDatabaseObject.PackageName;

                    ObjectStartAddressTB.Enabled = true;
                    ObjectStartAddressTB.Text = SelectedDatabaseObject.StartAddress;

                    ObjectEndAddressTB.Enabled = true;
                    ObjectEndAddressTB.Text = SelectedDatabaseObject.EndAddress;

                    ObjectZipFileTB.Enabled = true;
                    ObjectZipFileTB.Text = SelectedDatabaseObject.ZipFile;

                    ObjectVersionTB.Enabled = true;
                    ObjectVersionTB.Text = SelectedDatabaseObject.Version;

                    if (SelectedDatabaseObject.Timestamp > 0)
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedDatabaseObject.Timestamp));
                    else
                        ObjectLastUpdatedLabel.Text = "last updated: (none)";

                    ObjectDevURLTB.Enabled = true;
                    ObjectDevURLTB.Text = SelectedDatabaseObject.DevURL;

                    if (SelectedDatabaseObject is Config)
                    {
                        ObjectTypeComboBox.Enabled = true;
                        Config cfg = (Config)SelectedDatabaseObject;
                        switch (cfg.Type)
                        {
                            case "single":
                                ObjectTypeComboBox.SelectedIndex = 1;
                                break;
                            case "single1":
                                ObjectTypeComboBox.SelectedIndex = 1;
                                break;
                            case "single_dropdown":
                                ObjectTypeComboBox.SelectedIndex = 2;
                                break;
                            case "single_dropdown1":
                                ObjectTypeComboBox.SelectedIndex = 2;
                                break;
                            case "single_dropdown2":
                                ObjectTypeComboBox.SelectedIndex = 3;
                                break;
                            case "multi":
                                ObjectTypeComboBox.SelectedIndex = 4;
                                break;
                        }
                    }
                    else
                    {
                        ObjectTypeComboBox.Enabled = false;
                        ObjectTypeComboBox.SelectedIndex = 0;
                    }

                    ObjectEnabledCheckBox.Enabled = true;
                    ObjectEnabledCheckBox.Checked = SelectedDatabaseObject.Enabled;

                    ObjectVisibleCheckBox.Enabled = true;
                    ObjectVisibleCheckBox.Checked = SelectedDatabaseObject.Visible;

                    ObjectAppendExtractionCB.Enabled = false;

                    DescriptionTabPage.Enabled = true;

                    ObjectDescTB.Enabled = true;
                    ObjectDescTB.Text = SelectedDatabaseObject.Description;

                    ObjectUpdateNotesTB.Enabled = true;
                    ObjectUpdateNotesTB.Text = SelectedDatabaseObject.UpdateComment;

                    DownloadZipfileButton.Enabled = true;

                    DependenciesTabPage.Enabled = true;
                    DependencyPanel.Enabled = true;
                    LogicalDependencyPanel.Enabled = true;
                    PictureTabPage.Enabled = true;
                    UserDatasTabPage.Enabled = true;

                    //logicalDependencies
                    ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.LogicalDependencies;
                    CurrentLogicalDependenciesCB.DataSource = LogicalDependencies;
                    CurrentLogicalDependenciesCB.SelectedIndex = -1;

                    //dependencies
                    ObjectDependenciesLabel.Text = "dependencies (click to edit)";
                    ObjectDependenciesList.DataSource = SelectedDatabaseObject.Dependencies;
                    CurrentDependenciesCB.DataSource = Dependencies;
                    CurrentDependenciesCB.SelectedIndex = -1;

                    //pictures
                    ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
                    if (SelectedDatabaseObject.PictureList.Count == 0)
                        PictureURLTB.Text = "";

                    //userdatas
                    ObjectUserdatasList.DataSource = SelectedDatabaseObject.UserFiles;
                }
                else if (node.Category != null)
                {
                    SelectedGlobalDependency = null;
                    SelectedDependency = null;
                    SelectedLogicalDependency = null;
                    SelectedDatabaseObject = null;
                    SelectedCategory = node.Category;

                    ObjectNameTB.Enabled = true;
                    ObjectNameTB.Text = SelectedCategory.Name;

                    ObjectPackageNameTB.Enabled = false;

                    ObjectStartAddressTB.Enabled = false;

                    ObjectEndAddressTB.Enabled = false;

                    ObjectZipFileTB.Enabled = false;

                    ObjectVersionTB.Enabled = false;

                    ObjectLastUpdatedLabel.Text = "";

                    ObjectDevURLTB.Enabled = false;

                    ObjectTypeComboBox.Enabled = true;
                    if (SelectedCategory.SelectionType.Substring(0, 5).Equals("multi"))
                        ObjectTypeComboBox.SelectedIndex = 4;
                    else if (SelectedCategory.SelectionType.Substring(0, 6).Equals("single"))
                        ObjectTypeComboBox.SelectedIndex = 1;
                    else
                        ObjectTypeComboBox.SelectedIndex = 4;

                    ObjectEnabledCheckBox.Enabled = false;

                    ObjectVisibleCheckBox.Enabled = false;

                    ObjectAppendExtractionCB.Enabled = false;

                    DownloadZipfileButton.Enabled = false;

                    ObjectDescTB.Enabled = false;

                    ObjectUpdateNotesTB.Enabled = false;

                    DependenciesTabPage.Enabled = true;
                    DependencyPanel.Enabled = true;
                    LogicalDependencyPanel.Enabled = false;

                    ObjectDependenciesLabel.Text = "dependencies (click to edit)";
                    ObjectDependenciesList.DataSource = SelectedCategory.Dependencies;
                    CurrentDependenciesCB.DataSource = Dependencies;
                    CurrentDependenciesCB.SelectedIndex = -1;

                    PictureTabPage.Enabled = false;
                    UserDatasTabPage.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("DatabaseTreeView_NodeMouseClick", ex);
                MessageBox.Show("Exception at DatabaseTreeView_NodeMouseClick()", "CRITICAL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void MoveButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (SelectedCategory != null)
            {
                MessageBox.Show("Moving categories is not supported");
                return;
            }
            if (MessageBox.Show("Confirm you wish to move the object?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            using (DatabaseAdder dba = new DatabaseAdder(DatabaseEditorMode, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList, true))
            {
                dba.ShowDialog();
                if (dba.DialogResult == DialogResult.OK)
                {
                    if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
                    {
                        if (dba.SelectedGlobalDependency == null)
                            return;
                        GlobalDependencies.Remove(SelectedGlobalDependency);
                        int index = GlobalDependencies.IndexOf(dba.SelectedGlobalDependency);
                        GlobalDependencies.Insert(index, SelectedGlobalDependency);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.Dependency)
                    {
                        if (dba.SelectedDependency == null)
                            return;
                        Dependencies.Remove(SelectedDependency);
                        int index = Dependencies.IndexOf(dba.SelectedDependency);
                        Dependencies.Insert(index, SelectedDependency);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.LogicalDependency)
                    {
                        if (dba.SelectedLogicalDependency == null)
                            return;
                        LogicalDependencies.Remove(SelectedLogicalDependency);
                        int index = LogicalDependencies.IndexOf(dba.SelectedLogicalDependency);
                        LogicalDependencies.Insert(index, SelectedLogicalDependency);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.DBO)
                    {
                        if (SelectedDatabaseObject is Mod)
                        {
                            //mod->mod
                            //mod->config
                            Mod modToMove = (Mod)SelectedDatabaseObject;
                            if (dba.SelectedDatabaseObject is Mod)
                            {
                                Mod Ref = (Mod)dba.SelectedDatabaseObject;
                                //remove mod from list
                                List<Mod> ModList = ListContainsMod(modToMove);
                                ModList.Remove(modToMove);
                                //add mod to other list
                                ModList = ListContainsMod(Ref);
                                int index = ModList.IndexOf(Ref);
                                ModList.Insert(index, modToMove);
                            }
                            else if (dba.SelectedDatabaseObject is Config)
                            {
                                Config Ref = (Config)dba.SelectedDatabaseObject;
                                //remove mod first
                                List<Mod> ModList = ListContainsMod(modToMove);
                                ModList.Remove(modToMove);
                                //convert to config
                                Config c = ModToConfig(modToMove);
                                //move to config list
                                ListContainsConfig(Ref);
                                if (ListThatContainsConfig != null)
                                {
                                    int index = ListThatContainsConfig.IndexOf(Ref);
                                    ListThatContainsConfig.Insert(index, c);
                                }
                            }
                        }
                        else if (SelectedDatabaseObject is Config)
                        {
                            //config->mod
                            //config->config
                            ListThatContainsConfig = null;
                            Config cfgToMove = (Config)SelectedDatabaseObject;
                            if (dba.SelectedDatabaseObject is Config)
                            {
                                Config Ref = (Config)dba.SelectedDatabaseObject;
                                //remove config from list
                                ListContainsConfig(cfgToMove);
                                if (ListThatContainsConfig != null)
                                {
                                    ListThatContainsConfig.Remove(cfgToMove);
                                }
                                //add config to other list
                                ListThatContainsConfig = null;
                                ListContainsConfig(Ref);
                                if (ListThatContainsConfig != null)
                                {
                                    int index = ListThatContainsConfig.IndexOf(Ref);
                                    ListThatContainsConfig.Insert(index, cfgToMove);
                                }
                            }
                            else if (dba.SelectedDatabaseObject is Mod)
                            {
                                Mod Ref = (Mod)dba.SelectedDatabaseObject;
                                //remove the config first
                                ListContainsConfig(cfgToMove);
                                if (ListThatContainsConfig != null)
                                {
                                    //make move
                                    ListThatContainsConfig.Remove(cfgToMove);
                                }
                                //convert it to a mod
                                Mod m = ConfigToMod(cfgToMove);
                                //move it to mod list
                                List<Mod> ModList = ListContainsMod(Ref);
                                int index2 = ModList.IndexOf(Ref);
                                ModList.Insert(index2, m);
                            }
                        }
                    }
                }
            }
            this.DisplayDatabase(false);
            UnsavedModifications = true;
        }

        private void AddEntryButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (SelectedCategory != null)
            {
                MessageBox.Show("Adding categories is not supported");
                return;
            }
            using (DatabaseAdder dba = new DatabaseAdder(DatabaseEditorMode, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList, false))
            {
                dba.ShowDialog();
                if (dba.DialogResult == DialogResult.OK)
                {
                    if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
                    {
                        if (dba.SelectedGlobalDependency == null)
                            return;
                        Dependency newDep = new Dependency();
                        newDep.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                        newDep.StartAddress = ObjectStartAddressTB.Text;
                        newDep.EndAddress = ObjectEndAddressTB.Text;
                        newDep.ZipFile = ObjectZipFileTB.Text;
                        newDep.Enabled = ObjectEnabledCheckBox.Checked;
                        newDep.AppendExtraction = ObjectAppendExtractionCB.Checked;
                        newDep.DevURL = ObjectDevURLTB.Text;
                        newDep.CRC = "";
                        //newDep.ExtractPath = "";
                        if (!ObjectZipFileTB.Text.Equals(""))
                            newDep.CRC = "f";
                        int index = GlobalDependencies.IndexOf(dba.SelectedGlobalDependency);
                        GlobalDependencies.Insert(index, newDep);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.Dependency)
                    {
                        if (dba.SelectedDependency == null)
                            return;
                        Dependency newDep = new Dependency();
                        newDep.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                        newDep.StartAddress = ObjectStartAddressTB.Text;
                        newDep.EndAddress = ObjectEndAddressTB.Text;
                        newDep.ZipFile = ObjectZipFileTB.Text;
                        newDep.Enabled = ObjectEnabledCheckBox.Checked;
                        newDep.AppendExtraction = ObjectAppendExtractionCB.Checked;
                        newDep.DevURL = ObjectDevURLTB.Text;
                        newDep.CRC = "";
                        //newDep.ExtractPath = "";
                        if (!ObjectZipFileTB.Text.Equals(""))
                            newDep.CRC = "f";
                        List<LogicalDependency> logicalDeps = (List<LogicalDependency>)ObjectLogicalDependenciesList.DataSource;
                        int index = Dependencies.IndexOf(dba.SelectedDependency);
                        Dependencies.Insert(index, newDep);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.LogicalDependency)
                    {
                        if (dba.SelectedLogicalDependency == null)
                            return;
                        LogicalDependency newDep = new LogicalDependency();
                        newDep.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                        newDep.StartAddress = ObjectStartAddressTB.Text;
                        newDep.EndAddress = ObjectEndAddressTB.Text;
                        newDep.ZipFile = ObjectZipFileTB.Text;
                        newDep.Enabled = ObjectEnabledCheckBox.Checked;
                        newDep.DevURL = ObjectDevURLTB.Text;
                        newDep.CRC = "";
                        //newDep.ExtractPath = "";
                        if (!ObjectZipFileTB.Text.Equals(""))
                            newDep.CRC = "f";
                        int index = LogicalDependencies.IndexOf(dba.SelectedLogicalDependency);
                        LogicalDependencies.Insert(index, newDep);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.DBO)
                    {
                        if (SelectedDatabaseObject is Mod)
                        {
                            if (dba.sublist)
                            {
                                Config cfg = new Config();
                                cfg.Name = ObjectNameTB.Text;
                                cfg.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                                cfg.StartAddress = ObjectStartAddressTB.Text;
                                cfg.EndAddress = ObjectEndAddressTB.Text;
                                cfg.ZipFile = ObjectZipFileTB.Text;
                                cfg.DevURL = ObjectDevURLTB.Text;
                                cfg.Version = ObjectVersionTB.Text;
                                switch (ObjectTypeComboBox.SelectedIndex)
                                {
                                    case 1:
                                        cfg.Type = "single1";
                                        break;
                                    case 2:
                                        cfg.Type = "single_dropdown1";
                                        break;
                                    case 3:
                                        cfg.Type = "single_dropdown2";
                                        break;
                                    case 4:
                                        cfg.Type = "multi";
                                        break;
                                }
                                cfg.Enabled = ObjectEnabledCheckBox.Checked;
                                cfg.Visible = ObjectVisibleCheckBox.Checked;
                                cfg.Description = ObjectDescTB.Text;
                                cfg.UpdateComment = ObjectUpdateNotesTB.Text;
                                cfg.CRC = "";
                                if (!ObjectZipFileTB.Text.Equals(""))
                                    cfg.CRC = "f";
                                dba.SelectedDatabaseObject.configs.Add(cfg);
                            }
                            else
                            {
                                //mod->mod
                                if(dba.SelectedDatabaseObject is Mod)
                                {
                                    Mod mm = (Mod)dba.SelectedDatabaseObject;
                                    List<Mod> ModList = ListContainsMod(mm);
                                    int index = ModList.IndexOf(mm);
                                    //make changes
                                    Mod m = new Mod();
                                    m.Name = ObjectNameTB.Text;
                                    m.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                                    m.StartAddress = ObjectStartAddressTB.Text;
                                    m.EndAddress = ObjectEndAddressTB.Text;
                                    m.ZipFile = ObjectZipFileTB.Text;
                                    m.DevURL = ObjectDevURLTB.Text;
                                    m.Version = ObjectVersionTB.Text;
                                    m.Enabled = ObjectEnabledCheckBox.Checked;
                                    m.Visible = ObjectVisibleCheckBox.Checked;
                                    m.Description = ObjectDescTB.Text;
                                    m.UpdateComment = ObjectUpdateNotesTB.Text;
                                    m.CRC = "";
                                    if (!ObjectZipFileTB.Text.Equals(""))
                                        m.CRC = "f";
                                    ModList.Insert(index, m);
                                }
                                //mod->config
                                else if (dba.SelectedDatabaseObject is Config)
                                {
                                    Config cfgg = (Config)dba.SelectedDatabaseObject;
                                    ListThatContainsConfig = null;
                                    ListContainsConfig(cfgg);
                                    if (ListThatContainsConfig != null)
                                    {
                                        int index = ListThatContainsConfig.IndexOf(cfgg);
                                        //make changes
                                        Config cfg = new Config();
                                        cfg.Name = ObjectNameTB.Text;
                                        cfg.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                                        cfg.StartAddress = ObjectStartAddressTB.Text;
                                        cfg.EndAddress = ObjectEndAddressTB.Text;
                                        cfg.ZipFile = ObjectZipFileTB.Text;
                                        cfg.DevURL = ObjectDevURLTB.Text;
                                        cfg.Version = ObjectVersionTB.Text;
                                        switch (ObjectTypeComboBox.SelectedIndex)
                                        {
                                            case 1:
                                                cfg.Type = "single1";
                                                break;
                                            case 2:
                                                cfg.Type = "single_dropdown1";
                                                break;
                                            case 3:
                                                cfg.Type = "single_dropdown2";
                                                break;
                                            case 4:
                                                cfg.Type = "multi";
                                                break;
                                        }
                                        cfg.Enabled = ObjectEnabledCheckBox.Checked;
                                        cfg.Visible = ObjectVisibleCheckBox.Checked;
                                        cfg.Description = ObjectDescTB.Text;
                                        cfg.UpdateComment = ObjectUpdateNotesTB.Text;
                                        cfg.CRC = "";
                                        if (!ObjectZipFileTB.Text.Equals(""))
                                            cfg.CRC = "f";
                                        ListThatContainsConfig.Insert(index, cfg);
                                    }
                                }
                            }
                        }
                        else if (SelectedDatabaseObject is Config)
                        {
                            if (ObjectTypeComboBox.SelectedIndex == -1 || ObjectTypeComboBox.SelectedIndex == 0)
                            {
                                MessageBox.Show("Invalid Index of config type");
                                return;
                            }
                            if (dba.sublist)
                            {
                                Config cfg = new Config();
                                cfg.Name = ObjectNameTB.Text;
                                cfg.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                                cfg.StartAddress = ObjectStartAddressTB.Text;
                                cfg.EndAddress = ObjectEndAddressTB.Text;
                                cfg.ZipFile = ObjectZipFileTB.Text;
                                cfg.DevURL = ObjectDevURLTB.Text;
                                cfg.Version = ObjectVersionTB.Text;
                                switch (ObjectTypeComboBox.SelectedIndex)
                                {
                                    case 1:
                                        cfg.Type = "single1";
                                        break;
                                    case 2:
                                        cfg.Type = "single_dropdown1";
                                        break;
                                    case 3:
                                        cfg.Type = "single_dropdown2";
                                        break;
                                    case 4:
                                        cfg.Type = "multi";
                                        break;
                                }
                                cfg.Enabled = ObjectEnabledCheckBox.Checked;
                                cfg.Visible = ObjectVisibleCheckBox.Checked;
                                cfg.Description = ObjectDescTB.Text;
                                cfg.UpdateComment = ObjectUpdateNotesTB.Text;
                                cfg.CRC = "";
                                if (!ObjectZipFileTB.Text.Equals(""))
                                    cfg.CRC = "f";
                                dba.SelectedDatabaseObject.configs.Add(cfg);
                            }
                            else
                            {
                                //config->config
                                if(dba.SelectedDatabaseObject is Config)
                                {
                                    Config cfgg = (Config)dba.SelectedDatabaseObject;
                                    ListThatContainsConfig = null;
                                    ListContainsConfig(cfgg);
                                    if (ListThatContainsConfig != null)
                                    {
                                        int index = ListThatContainsConfig.IndexOf(cfgg);
                                        //make changes
                                        Config cfg = new Config();
                                        cfg.Name = ObjectNameTB.Text;
                                        cfg.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                                        cfg.StartAddress = ObjectStartAddressTB.Text;
                                        cfg.EndAddress = ObjectEndAddressTB.Text;
                                        cfg.ZipFile = ObjectZipFileTB.Text;
                                        cfg.DevURL = ObjectDevURLTB.Text;
                                        cfg.Version = ObjectVersionTB.Text;
                                        switch (ObjectTypeComboBox.SelectedIndex)
                                        {
                                            case 1:
                                                cfg.Type = "single1";
                                                break;
                                            case 2:
                                                cfg.Type = "single_dropdown1";
                                                break;
                                            case 3:
                                                cfg.Type = "single_dropdown2";
                                                break;
                                            case 4:
                                                cfg.Type = "multi";
                                                break;
                                        }
                                        cfg.Enabled = ObjectEnabledCheckBox.Checked;
                                        cfg.Visible = ObjectVisibleCheckBox.Checked;
                                        cfg.Description = ObjectDescTB.Text;
                                        cfg.UpdateComment = ObjectUpdateNotesTB.Text;
                                        cfg.CRC = "";
                                        if (!ObjectZipFileTB.Text.Equals(""))
                                            cfg.CRC = "f";
                                        ListThatContainsConfig.Insert(index, cfg);
                                    }
                                }
                                //config->mod
                                else if (dba.SelectedDatabaseObject is Mod)
                                {
                                    Mod mm = (Mod)dba.SelectedDatabaseObject;
                                    List<Mod> ModList = ListContainsMod(mm);
                                    int index = ModList.IndexOf(mm);
                                    //make changes
                                    Mod m = new Mod();
                                    m.Name = ObjectNameTB.Text;
                                    m.PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text);
                                    m.StartAddress = ObjectStartAddressTB.Text;
                                    m.EndAddress = ObjectEndAddressTB.Text;
                                    m.ZipFile = ObjectZipFileTB.Text;
                                    m.DevURL = ObjectDevURLTB.Text;
                                    m.Version = ObjectVersionTB.Text;
                                    m.Enabled = ObjectEnabledCheckBox.Checked;
                                    m.Visible = ObjectVisibleCheckBox.Checked;
                                    m.Description = ObjectDescTB.Text;
                                    m.UpdateComment = ObjectUpdateNotesTB.Text;
                                    m.CRC = "";
                                    if (!ObjectZipFileTB.Text.Equals(""))
                                        m.CRC = "f";
                                    ModList.Insert(index, m);
                                }
                            }
                        }
                    }
                }
            }
            this.DisplayDatabase(false);
            UnsavedModifications = true;
        }

        private void RemoveEntryButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (SelectedCategory != null)
            {
                MessageBox.Show("Removing categories is not supported");
                return;
            }
            if (MessageBox.Show("Confirm you wish to remove the object?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                GlobalDependencies.Remove(SelectedGlobalDependency);
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                //check if the dependency is in use first
                if (DependencyInUse(SelectedDependency.PackageName, true))
                {
                    MessageBox.Show(string.Format("Cannot remove because it is in use:\n{0}",InUseSB.ToString()));
                }
                else
                {
                    Dependencies.Remove(SelectedDependency);
                }
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                //check if the dependency is in use first
                if (DependencyInUse(SelectedLogicalDependency.PackageName, false))
                {
                    MessageBox.Show(string.Format("Cannot remove because it is in use:\n{0}", InUseSB.ToString()));
                }
                else
                {
                    LogicalDependencies.Remove(SelectedLogicalDependency);
                }
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                if (SelectedDatabaseObject is Mod)
                {
                    Mod m = (Mod)SelectedDatabaseObject;
                    List<Mod> ModList = ListContainsMod(m);
                    int index = ModList.IndexOf(m);
                    ModList.RemoveAt(index);
                }
                else if (SelectedDatabaseObject is Config)
                {
                    ListThatContainsConfig = null;
                    Config cfg = (Config)SelectedDatabaseObject;
                    ListContainsConfig(cfg);
                    if (ListThatContainsConfig != null)
                    {
                        int index = ListThatContainsConfig.IndexOf(cfg);
                        ListThatContainsConfig.RemoveAt(index);
                    }
                }
            }
            DisplayDatabase(false);
            UnsavedModifications = true;
        }

        private bool DependencyInUse(string packageName, bool isDependency)
        {
            InUseSB = new StringBuilder();
            bool InUse = false;
            if (!isDependency)
            {
                foreach (Dependency d in Dependencies)
                {
                    if (d.PackageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append(string.Format("Dependency: {0}\n", d.PackageName));
                    }
                }
            }
            foreach (Category c in ParsedCategoryList)
            {
                foreach (Dependency d in c.Dependencies)
                {
                    if (d.PackageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append(string.Format("Category: {0}", c.Name));
                    }
                }
                foreach (Mod m in c.Mods)
                {
                    foreach (Dependency d in m.Dependencies)
                    {
                        if (d.PackageName.Equals(packageName))
                        {
                            InUse = true;
                            InUseSB.Append(string.Format("Mod: {0}\n", m.PackageName));
                        }
                    }
                    foreach (LogicalDependency d in m.LogicalDependencies)
                    {
                        if (d.PackageName.Equals(packageName))
                        {
                            InUse = true;
                            InUseSB.Append(string.Format("Mod: {0}\n", m.PackageName));
                        }
                    }
                    ProcessConfigsInUse(InUseSB, m.configs, InUse, packageName);
                }
            }
            return InUse;
        }
        private void ProcessConfigsInUse(StringBuilder sb, List<Config> configs, bool InUse, string packageName)
        {
            foreach (Config c in configs)
            {
                foreach (Dependency d in c.Dependencies)
                {
                    if (d.PackageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append(string.Format("Config: {0}\n", c.PackageName));
                    }
                }
                foreach (LogicalDependency d in c.LogicalDependencies)
                {
                    if (d.PackageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append(string.Format("Config: {0}\n", c.PackageName));
                    }
                }
                ProcessConfigsInUse(sb, c.configs, InUse, packageName);
            }
        }

        private void AddDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add dependency", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (SelectedCategory != null)
            {
                Dependency ld = new Dependency();
                Dependency ld2 = (Dependency)CurrentDependenciesCB.SelectedItem;
                ld.PackageName = ld2.PackageName;
                SelectedCategory.Dependencies.Add(ld);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedCategory.Dependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                Dependency ld = new Dependency();
                Dependency ld2 = (Dependency)CurrentDependenciesCB.SelectedItem;
                ld.PackageName = ld2.PackageName;
                SelectedDatabaseObject.Dependencies.Add(ld);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedDatabaseObject.Dependencies;
            }
            UnsavedModifications = true;
        }

        private void RemoveDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove dependency", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (SelectedCategory != null)
            {
                SelectedCategory.Dependencies.Remove((Dependency)ObjectDependenciesList.SelectedItem);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedCategory.Dependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                SelectedDatabaseObject.Dependencies.Remove((Dependency)ObjectDependenciesList.SelectedItem);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedDatabaseObject.Dependencies;
            }
            UnsavedModifications = true;
        }

        private void AddLogicalDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add logical dependency", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                LogicalDependency ld = new LogicalDependency();
                LogicalDependency ld2 = (LogicalDependency)CurrentLogicalDependenciesCB.SelectedItem;
                ld.PackageName = ld2.PackageName;
                ld.NegateFlag = LogicalDependnecyNegateFlagCB.Checked;
                SelectedDependency.LogicalDependencies.Add(ld);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.LogicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                LogicalDependency ld = new LogicalDependency();
                LogicalDependency ld2 = (LogicalDependency)CurrentLogicalDependenciesCB.SelectedItem;
                ld.PackageName = ld2.PackageName;
                ld.NegateFlag = LogicalDependnecyNegateFlagCB.Checked;
                SelectedDatabaseObject.LogicalDependencies.Add(ld);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.LogicalDependencies;
            }
            UnsavedModifications = true;
        }

        private void RemoveLogicalDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove logical dependency", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                SelectedDependency.LogicalDependencies.Remove((LogicalDependency)ObjectLogicalDependenciesList.SelectedItem);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.LogicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                SelectedDatabaseObject.LogicalDependencies.Remove((LogicalDependency)ObjectLogicalDependenciesList.SelectedItem);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.LogicalDependencies;
            }
            UnsavedModifications = true;
        }

        private void MovePictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to move picture in list", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int index = Utils.ParseInt(MovePictureTB.Text, -1);
            if (index == -1)
            {
                MessageBox.Show("Invalid index in move to position");
                return;
            }
            if (index > SelectedDatabaseObject.PictureList.Count)
            {
                MessageBox.Show("Invalid index in move to position");
                return;
            }
            Media media = (Media)ObjectPicturesList.SelectedItem;
            SelectedDatabaseObject.PictureList.Remove(media);
            //if the index is now out of range, just put it in the bottom
            if (index > SelectedDatabaseObject.PictureList.Count)
                index = SelectedDatabaseObject.PictureList.Count;
            SelectedDatabaseObject.PictureList.Insert(index, media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
            UnsavedModifications = true;
        }

        private void AddPictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add picture", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (PicturesTypeCBox.SelectedIndex == -1 || PicturesTypeCBox.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid entry in Picture Type");
                return;
            }
            int index = Utils.ParseInt(AddPictureTB.Text, -1);
            if (index == -1)
            {
                MessageBox.Show("Invalid index in add at positon");
                return;
            }
            if (index > SelectedDatabaseObject.PictureList.Count)
            {
                MessageBox.Show("Invalid index in add at positon");
                return;
            }
            Media media = new Media()
            {
                URL = PictureURLTB.Text
            };
            switch(PicturesTypeCBox.SelectedIndex)
            {
                case 1:
                    media.MediaType = MediaType.Picture;
                    break;
                case 2:
                    media.MediaType = MediaType.Webpage;
                    break;
                case 3:
                    media.MediaType = MediaType.MediaFile;
                    break;
                case 4:
                    media.MediaType = MediaType.HTML;
                    break;
            }
            SelectedDatabaseObject.PictureList.Insert(index, media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
            UnsavedModifications = true;
        }

        private void RemovePictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove picture", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            Media media = (Media)ObjectPicturesList.SelectedItem;
            SelectedDatabaseObject.PictureList.Remove(media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
            UnsavedModifications = true;
        }

        private void ApplyPictureEditButton_Click(object sender, EventArgs e)
        {
            if (ObjectPicturesList.Items.Count == 0) return;
            Media media = (Media)ObjectPicturesList.SelectedItem;
            if (PicturesTypeCBox.SelectedIndex == -1 || PicturesTypeCBox.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid entry in Picture Type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("Confirm you wish to edit picture entry", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            switch (PicturesTypeCBox.SelectedIndex)
            {
                case 1:
                    media.MediaType = MediaType.Picture;
                    break;
                case 2:
                    media.MediaType = MediaType.Webpage;
                    break;
                case 3:
                    media.MediaType = MediaType.MediaFile;
                    break;
                case 4:
                    media.MediaType = MediaType.HTML;
                    break;
            }
            media.URL = PictureURLTB.Text;
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
            UnsavedModifications = true;
        }

        private void ObjectLogicalDependenciesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            LogicalDependency ld = (LogicalDependency)lb.SelectedItem;
            foreach (LogicalDependency d in LogicalDependencies)
            {
                if (d.PackageName.Equals(ld.PackageName))
                {
                    CurrentLogicalDependenciesCB.SelectedItem = d;
                    break;
                }
            }
            LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
            LogicalDependnecyNegateFlagCB.Checked = ld.NegateFlag;
            LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;
        }

        private void LogicalDependnecyNegateFlagCB_CheckedChanged(object sender, EventArgs e)
        {
            if (MessageBox.Show("confirm change logical dependency negate flag status (press 'no' if changing negate flag for adding a new logical dependency)", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                LogicalDependency ld = (LogicalDependency)ObjectLogicalDependenciesList.SelectedItem;
                ld.NegateFlag = LogicalDependnecyNegateFlagCB.Checked;
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.LogicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                LogicalDependency ld = (LogicalDependency)ObjectLogicalDependenciesList.SelectedItem;
                ld.NegateFlag = LogicalDependnecyNegateFlagCB.Checked;
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.LogicalDependencies;
            }
            UnsavedModifications = true;
        }

        private void ObjectDependenciesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            if (lb.SelectedItem is DatabaseLogic)
                return;
            if (lb.SelectedItem is string)
                return;
            if (lb.SelectedItem is Dependency)
            {
                Dependency ld = (Dependency)lb.SelectedItem;
                foreach (Dependency d in Dependencies)
                {
                    if (d.PackageName.Equals(ld.PackageName))
                    {
                        CurrentDependenciesCB.SelectedItem = d;
                        break;
                    }
                }
            }
        }

        private void ObjectPicturesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            Media med = (Media)lb.SelectedItem;
            PicturesTypeCBox.SelectedIndex = (int)med.MediaType;
            PictureURLTB.Text = med.URL;
            MovePictureTB.Text = "" + SelectedDatabaseObject.PictureList.IndexOf(med);
            AddPictureTB.Text = "" + SelectedDatabaseObject.PictureList.IndexOf(med);
        }

        private void AddUserdatasButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add userdata entry", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            SelectedDatabaseObject.UserFiles.Add(ObjectUserdatasTB.Text);
            ObjectUserdatasList.DataSource = null;
            ObjectUserdatasList.Items.Clear();
            ObjectUserdatasList.DataSource = SelectedDatabaseObject.UserFiles;
            UnsavedModifications = true;
        }

        private void RemoveUserdatasButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove userdata entry", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int index = SelectedDatabaseObject.UserFiles.IndexOf((string)ObjectUserdatasList.SelectedItem);
            SelectedDatabaseObject.UserFiles.RemoveAt(index);
            ObjectUserdatasList.DataSource = null;
            ObjectUserdatasList.Items.Clear();
            ObjectUserdatasList.DataSource = SelectedDatabaseObject.UserFiles;
            UnsavedModifications = true;
        }

        private void ObjectUserdatasList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ObjectUserdatasList.DataSource == null)
                return;
            ObjectUserdatasTB.Text = ObjectUserdatasList.SelectedItem.ToString();
        }

        private void EditUserdatasButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to edit userdata entry", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int index = SelectedDatabaseObject.UserFiles.IndexOf((string)ObjectUserdatasList.SelectedItem);
            SelectedDatabaseObject.UserFiles[index] = ObjectUserdatasTB.Text;
            ObjectUserdatasList.DataSource = null;
            ObjectUserdatasList.Items.Clear();
            ObjectUserdatasList.DataSource = SelectedDatabaseObject.UserFiles;
            UnsavedModifications = true;
        }

        private void ObjectDependenciesList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            if (lb.SelectedItem is DatabaseLogic)
                return;
            if (lb.SelectedItem is string)
                return;
            if (MessageBox.Show("Confirm you wish to jump and loose any changes to this object you may have made", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            string savePackageName = "";
            Dependency d = (Dependency)ObjectDependenciesList.SelectedItem;
            savePackageName = d.PackageName;
            DependencyRB.Checked = true;
            foreach (DatabaseTreeNode dtn in DatabaseTreeView.Nodes)
            {
                if (dtn.Dependency.PackageName.Equals(savePackageName))
                {
                    dtn.EnsureVisible();
                    DatabaseTreeView.SelectedNode = dtn;
                    DatabaseTreeView_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(dtn, MouseButtons.Left, 0, 0, 0));
                }
            }
        }

        private void ObjectLogicalDependenciesList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to jump and loose any changes to this object you may have made", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            string savePackageName = "";
            LogicalDependency d = (LogicalDependency)ObjectLogicalDependenciesList.SelectedItem;
            savePackageName = d.PackageName;
            LogicalDependencyRB.Checked = true;
            foreach (DatabaseTreeNode dtn in DatabaseTreeView.Nodes)
            {
                if (dtn.LogicalDependency.PackageName.Equals(savePackageName))
                {
                    dtn.EnsureVisible();
                    DatabaseTreeView.SelectedNode = dtn;
                    DatabaseTreeView_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(dtn, MouseButtons.Left, 0, 0, 0));
                }
            }
        }

        private void BuildDatabaseLogic(LogicalDependency d)
        {
            foreach (Dependency depD in Dependencies)
            {
                foreach (LogicalDependency ld in depD.LogicalDependencies)
                {
                    if (ld.PackageName.Equals(d.PackageName))
                    {
                        DatabaseLogic dbl = new DatabaseLogic()
                        {
                            PackageName = depD.PackageName,
                            Enabled = false,
                            Checked = false,
                            NotFlag = ld.NegateFlag
                        };
                        d.DatabasePackageLogic.Add(dbl);
                    }
                }
            }
            foreach (Category c in ParsedCategoryList)
            {
                //will itterate through every catagory once
                foreach (Mod m in c.Mods)
                {
                    foreach (LogicalDependency ld in m.LogicalDependencies)
                    {
                        if (ld.PackageName.Equals(d.PackageName))
                        {
                            DatabaseLogic dbl = new DatabaseLogic()
                            {
                                PackageName = m.PackageName,
                                Enabled = false,
                                Checked = false,
                                NotFlag = ld.NegateFlag
                            };
                            d.DatabasePackageLogic.Add(dbl);
                        }
                    }
                    if (m.configs.Count > 0)
                        ProcessConfigsLogical(d, m.configs);
                }
            }
        }

        private void ProcessConfigsLogical(LogicalDependency d, List<Config> configList)
        {
            foreach (Config config in configList)
            {
                foreach (LogicalDependency ld in config.LogicalDependencies)
                {
                    if (ld.PackageName.Equals(d.PackageName))
                    {
                        DatabaseLogic dl = new DatabaseLogic()
                        {
                            PackageName = config.PackageName,
                            Enabled = false,
                            Checked = false,
                            NotFlag = ld.NegateFlag
                        };
                        d.DatabasePackageLogic.Add(dl);
                    }
                }
                if (config.configs.Count > 0)
                    ProcessConfigsLogical(d, config.configs);
            }
        }

        private List<string> BuildDatabaseLogic(Dependency d)
        {
            List<string> objectsThatUseDependency = new List<string>();
            foreach (Category c in ParsedCategoryList)
            {
                //will itterate through every catagory once
                foreach (Mod m in c.Mods)
                {
                    foreach (Dependency dep in m.Dependencies)
                    {
                        if (dep.PackageName.Equals(d.PackageName))
                        {
                            objectsThatUseDependency.Add(m.PackageName);
                        }
                    }
                    if (m.configs.Count > 0)
                        ProcessConfigsLogical(d, m.configs,objectsThatUseDependency);
                }
            }
            return objectsThatUseDependency;
        }
        
        private void ProcessConfigsLogical(Dependency d, List<Config> configList, List<string> objectsThatUseDependency)
        {
            foreach (Config config in configList)
            {
                foreach (Dependency dep in config.Dependencies)
                {
                    if (dep.PackageName.Equals(d.PackageName))
                    {
                        objectsThatUseDependency.Add(config.PackageName);
                    }
                }
                if (config.configs.Count > 0)
                    ProcessConfigsLogical(d, config.configs, objectsThatUseDependency);
            }
        }

        private void SearchBox_TextUpdate(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            string currentText = cb.Text;
            cb.Items.Clear();
            List<string> filteredItems = null;
            String[] filtered_parts = currentText.Split('*');
            filteredItems = new List<string>(allPackageNames);
            foreach (var f in filtered_parts)
            {
                filteredItems = filteredItems.FindAll(x => x.ToLower().Contains(f.ToLower()));
            }
            cb.Items.AddRange(filteredItems.ToArray());
            if (cb.Items.Count == 0)
            {
                cb.BackColor = System.Drawing.Color.Red;
                cb.Items.Add("none");
                cb.SelectedIndex = 0;
            }
            else
                cb.BackColor = System.Drawing.SystemColors.Window;
            cb.DroppedDown = true;
            Cursor.Current = Cursors.Default;
            cb.IntegralHeight = true;
            // set the position of the cursor
            cb.Text = currentText;
            cb.SelectionStart = currentText.Length;
            cb.SelectionLength = 0;
        }

        private void SearchBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.SelectedIndex == -1)
                return;
            string packageNameSearch = "";
            packageNameSearch = (string)cb.SelectedItem;
            foreach (DatabaseTreeNode node in DatabaseTreeView.Nodes)
            {
                if (node.GlobalDependency != null)
                {
                    if (node.GlobalDependency.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                else if (node.Dependency != null)
                {
                    if (node.Dependency.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                else if (node.LogicalDependency != null)
                {
                    if (node.LogicalDependency.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                else if (node.Category != null)
                {
                    if (node.Category.Name.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                else if (node.DatabaseObject != null)
                {
                    if (node.DatabaseObject.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                if (node.Nodes.Count > 0)
                    SearchBoxConfig(node.Nodes, packageNameSearch);
            }
        }
        private void SearchBoxConfig(TreeNodeCollection nodes, string packageNameSearch)
        {
            foreach (DatabaseTreeNode node in nodes)
            {
                if (node.DatabaseObject != null)
                {
                    if (node.DatabaseObject.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                if (node.Nodes.Count > 0)
                    SearchBoxConfig(node.Nodes, packageNameSearch);
            }
        }

        private void DownloadZipfileButton_Click(object sender, EventArgs e)
        {
            if (ObjectZipFileTB.Text.Equals(""))
            {
                MessageBox.Show("Nothing to download");
                return;
            }
            string DownloadURL = ObjectStartAddressTB.Text + ObjectZipFileTB.Text + ObjectEndAddressTB.Text;
            DatabaseDownloadEditor editor = new DatabaseDownloadEditor(Utils.ReplaceMacro(DownloadURL), Path.GetFileName(ObjectZipFileTB.Text));
            editor.Show();
        }

        private bool DuplicatePackageName(List<Dependency> GlobalDependencies, List<Dependency> Dependencies, List<LogicalDependency> LogicalDependencies, List<Category> ParsedCategoryList, string PackageName, int duplicate)
        {
            //int duplicate = 0;
            foreach (Dependency d in GlobalDependencies)
            {
                if (d.PackageName.Equals(PackageName))
                {
                    duplicate++;
                }
            }
            foreach (Dependency d in Dependencies)
            {
                if (d.PackageName.Equals(PackageName))
                {
                    duplicate++;
                }
            }
            foreach (LogicalDependency d in LogicalDependencies)
            {
                if (d.PackageName.Equals(PackageName))
                {
                    duplicate++;
                }
            }
            foreach (Category c in ParsedCategoryList)
            {
                foreach (Mod m in c.Mods)
                {
                    if (m.PackageName.Equals(PackageName))
                    {
                        duplicate++;
                    }
                    if (m.configs.Count > 0)
                    {
                        DuplicatePackageNameConfig(m.configs, PackageName, duplicate);
                    }
                }
            }
            if (duplicate > 1)
                return true;
            else 
                return false;
        }

        private void DuplicatePackageNameConfig(List<Config> configList, string PackageName, int duplicate)
        {
            foreach(Config c in configList)
            {
                if(c.PackageName.Equals(PackageName))
                {
                    duplicate++;
                }
            }
        }

        private string GetNewPackageName(string oldPackageName)
        {
            int i = 1;
            int temp = 0;
            string packageName = string.Format("{0}_NEW_{1}", oldPackageName, i);
            while (DuplicatePackageName(this.GlobalDependencies, this.Dependencies, this.LogicalDependencies, this.ParsedCategoryList, packageName, temp))
                packageName = string.Format("{0}_NEW_{1}", oldPackageName, i++);
            return packageName;
        }

        private Mod ConfigToMod(Config cfgToMove)
        {
            return new Mod()
            {
                Name = cfgToMove.Name,
                Version = cfgToMove.Version,
                ZipFile = cfgToMove.ZipFile,
                configs = cfgToMove.configs,
                StartAddress = cfgToMove.StartAddress,
                LogicalDependencies = cfgToMove.LogicalDependencies,
                Dependencies = cfgToMove.Dependencies,
                PictureList = cfgToMove.PictureList,
                EndAddress = cfgToMove.EndAddress,
                CRC = cfgToMove.CRC,
                Enabled = cfgToMove.Enabled,
                Visible = cfgToMove.Visible,
                PackageName = cfgToMove.PackageName,
                Size = cfgToMove.Size,
                UpdateComment = cfgToMove.UpdateComment,
                Description = cfgToMove.Description,
                DevURL = cfgToMove.DevURL,
                UserFiles = cfgToMove.UserFiles,
            };
        }

        private Config ModToConfig(Mod modToMove)
        {
            return new Config()
            {
                Name = modToMove.Name,
                Version = modToMove.Version,
                ZipFile = modToMove.ZipFile,
                configs = modToMove.configs,
                StartAddress = modToMove.StartAddress,
                LogicalDependencies = modToMove.LogicalDependencies,
                Dependencies = modToMove.Dependencies,
                PictureList = modToMove.PictureList,
                EndAddress = modToMove.EndAddress,
                CRC = modToMove.CRC,
                Enabled = modToMove.Enabled,
                Visible = modToMove.Visible,
                PackageName = modToMove.PackageName,
                Size = modToMove.Size,
                UpdateComment = modToMove.UpdateComment,
                Description = modToMove.Description,
                DevURL = modToMove.DevURL,
                UserFiles = modToMove.UserFiles,
                Type = "multi",
            };
        }

        private void callTextBoxURL_DoubleClick(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                if ((TextBox)sender != null && !((TextBox)sender).Text.Equals(""))
                    System.Diagnostics.Process.Start(((TextBox)sender).Text);
            }
        }

        private void ObjectPicturesList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            Media med = (Media)lb.SelectedItem;
            string[] searchList = new string[] { "http://", "https://" };
            foreach (string r in searchList)
            {
                int pos = med.URL.IndexOf(r, StringComparison.OrdinalIgnoreCase);
                if (pos >= 0)
                {
                    System.Diagnostics.Process.Start(med.URL.Substring(pos));
                }
            }
        }

        private void DatabaseTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                DatabaseTreeView_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(e.Node, MouseButtons.Left, 0, 0, 0));
            }
        }
    }
}
