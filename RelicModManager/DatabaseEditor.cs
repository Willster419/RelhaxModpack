using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

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
        private List<LogicalDependnecy> LogicalDependencies;
        //other stuff
        private string DatabaseLocation = "";
        private Dependency SelectedGlobalDependency;
        private Dependency SelectedDependency;
        private LogicalDependnecy SelectedLogicalDependency;
        private DatabaseObject SelectedDatabaseObject;
        private Category SelectedCategory;
        private int currentSelectedIndex = -1;
        string GameVersion = "";
        private StringBuilder InUseSB;
        private List<Config> ListThatContainsConfig;

        private EditorMode DatabaseEditorMode;

        public DatabaseEditor()
        {
            InitializeComponent();
            Settings.loadSettings();
        }

        private void DatabaseEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Utils.appendToLog("|------------------------------------------------------------------------------------------------|");
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
            if(DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach(Dependency d in GlobalDependencies)
                {
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (Dependency d in Dependencies)
                {
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (LogicalDependnecy d in LogicalDependencies)
                {
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach(Category cat in ParsedCategoryList)
                {
                    DatabaseTreeNode catNode = new DatabaseTreeNode(cat, 4);
                    DatabaseTreeView.Nodes.Add(catNode);
                    foreach(Mod m in cat.mods)
                    {
                        DatabaseTreeNode modNode = new DatabaseTreeNode(m, (int)DatabaseEditorMode);
                        catNode.Nodes.Add(modNode);
                        if (SelectedDatabaseObject != null && SelectedDatabaseObject.packageName.Equals(m.packageName))
                            modNode.EnsureVisible();
                        DisplayDatabaseConfigs(modNode, m.configs);
                    }
                }
            }
            if(resetUI)
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

            ObjectTypeComboBox.Enabled = false;
            ObjectTypeComboBox.SelectedIndex = 0;

            ObjectEnabledCheckBox.Enabled = false;
            ObjectEnabledCheckBox.Checked = false;

            ObjectVisableCheckBox.Enabled = false;
            ObjectVisableCheckBox.Checked = false;

            ObjectAppendExtractionCB.Enabled = false;
            ObjectAppendExtractionCB.Checked = false;

            ObjectDescTB.Enabled = false;
            ObjectDescTB.Text = "";

            ObjectUpdateNotesTB.Enabled = false;
            ObjectUpdateNotesTB.Text = "";

            DependenciesTabPage.Enabled = false;
            PicturePanel.Enabled = false;
        }
        private void DisplayDatabaseConfigs(DatabaseTreeNode parrent, List<Config> configs)
        {
            foreach(Config c in configs)
            {
                DatabaseTreeNode ConfigParrent = new DatabaseTreeNode(c, (int)DatabaseEditorMode);
                parrent.Nodes.Add(ConfigParrent);
                if (SelectedDatabaseObject != null && SelectedDatabaseObject.packageName.Equals(c.packageName))
                    ConfigParrent.EnsureVisible();
                DisplayDatabaseConfigs(ConfigParrent, c.configs);
            }
        }
        //show the load database dialog and load the database
        private void LoadDatabaseButton_Click(object sender, EventArgs e)
        {
            string workingDirectory = Path.Combine(string.IsNullOrEmpty(Settings.customModInfoPath) ? Application.StartupPath : Settings.customModInfoPath);
            if (Directory.Exists(workingDirectory))
            {
                OpenDatabaseDialog.InitialDirectory = workingDirectory;
            }
            if (OpenDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            DatabaseLocation = OpenDatabaseDialog.FileName;
            if (!File.Exists(DatabaseLocation))
                return;
            GameVersion = Utils.readVersionFromModInfo(DatabaseLocation);
            GlobalDependencies = new List<Dependency>();
            Dependencies = new List<Dependency>();
            LogicalDependencies = new List<LogicalDependnecy>();
            ParsedCategoryList = new List<Category>();
            Utils.createModStructure(DatabaseLocation, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList);
            DatabaseEditorMode = EditorMode.GlobalDependnecy;
            this.DisplayDatabase();
        }
        //show the save database dialog and save the database
        private void SaveDatabaseButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (SaveDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            DatabaseLocation = SaveDatabaseDialog.FileName;
            Utils.SaveDatabase(DatabaseLocation, GameVersion, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList);
        }
        //Apply all changes from the form
        private void ApplyChangesButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (MessageBox.Show("Confirm you wish to apply changes", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                int index = GlobalDependencies.IndexOf(SelectedGlobalDependency);
                SelectedGlobalDependency.packageName = ObjectPackageNameTB.Text;
                SelectedGlobalDependency.startAddress = ObjectStartAddressTB.Text;
                SelectedGlobalDependency.endAddress = ObjectEndAddressTB.Text;
                SelectedGlobalDependency.devURL = ObjectDevURLTB.Text;
                SelectedGlobalDependency.dependencyZipFile = ObjectZipFileTB.Text;
                SelectedGlobalDependency.enabled = ObjectEnabledCheckBox.Checked;
                SelectedGlobalDependency.appendExtraction = ObjectAppendExtractionCB.Checked;
                GlobalDependencies[index] = SelectedGlobalDependency;
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                int index = Dependencies.IndexOf(SelectedDependency);
                SelectedDependency.packageName = ObjectPackageNameTB.Text;
                SelectedDependency.startAddress = ObjectStartAddressTB.Text;
                SelectedDependency.endAddress = ObjectEndAddressTB.Text;
                SelectedDependency.devURL = ObjectDevURLTB.Text;
                SelectedDependency.dependencyZipFile = ObjectZipFileTB.Text;
                SelectedDependency.enabled = ObjectEnabledCheckBox.Checked;
                SelectedDependency.appendExtraction = ObjectAppendExtractionCB.Checked;
                Dependencies[index] = SelectedDependency;
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                int index = LogicalDependencies.IndexOf(SelectedLogicalDependency);
                SelectedLogicalDependency.packageName = ObjectPackageNameTB.Text;
                SelectedLogicalDependency.startAddress = ObjectStartAddressTB.Text;
                SelectedLogicalDependency.endAddress = ObjectEndAddressTB.Text;
                SelectedLogicalDependency.devURL = ObjectDevURLTB.Text;
                SelectedLogicalDependency.dependencyZipFile = ObjectZipFileTB.Text;
                SelectedLogicalDependency.enabled = ObjectEnabledCheckBox.Checked;
                LogicalDependencies[index] = SelectedLogicalDependency;
            }
            else if (SelectedCategory != null)
            {
                int index = ParsedCategoryList.IndexOf(SelectedCategory);
                SelectedCategory.name = ObjectNameTB.Text;
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
                    m.name = ObjectNameTB.Text;
                    m.packageName = ObjectPackageNameTB.Text;
                    m.startAddress = ObjectStartAddressTB.Text;
                    m.endAddress = ObjectEndAddressTB.Text;
                    m.zipFile = ObjectZipFileTB.Text;
                    m.devURL = ObjectDevURLTB.Text;
                    m.enabled = ObjectEnabledCheckBox.Checked;
                    m.visible = ObjectVisableCheckBox.Checked;
                    m.description = ObjectDescTB.Text;
                    m.updateComment = ObjectUpdateNotesTB.Text;
                    ModList[index] = m;
                }
                else if (SelectedDatabaseObject is Config)
                {
                    if(ObjectTypeComboBox.SelectedIndex == -1 || ObjectTypeComboBox.SelectedIndex == 0)
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
                        cfg.name = ObjectNameTB.Text;
                        cfg.packageName = ObjectPackageNameTB.Text;
                        cfg.startAddress = ObjectStartAddressTB.Text;
                        cfg.endAddress = ObjectEndAddressTB.Text;
                        cfg.zipFile = ObjectZipFileTB.Text;
                        cfg.devURL = ObjectDevURLTB.Text;
                        switch(ObjectTypeComboBox.SelectedIndex)
                        {
                            case 1:
                                cfg.type = "single1";
                                break;
                            case 2:
                                cfg.type = "single_dropdown1";
                                break;
                            case 3:
                                cfg.type = "single_dropdown2";
                                break;
                            case 4:
                                cfg.type = "multi";
                                break;
                        }
                        cfg.enabled = ObjectEnabledCheckBox.Checked;
                        cfg.visible = ObjectVisableCheckBox.Checked;
                        cfg.description = ObjectDescTB.Text;
                        cfg.updateComment = ObjectUpdateNotesTB.Text;
                        ListThatContainsConfig[index] = cfg;
                    }
                }
            }
            this.DisplayDatabase(false);
        }
        private List<Mod> ListContainsMod(Mod mod)
        {
            foreach(Category cat in ParsedCategoryList)
            {
                if (cat.mods.Contains(mod))
                    return cat.mods;
            }
            return null;
        }
        private void ListContainsConfig(Config cfg)
        {
            foreach (Category cat in ParsedCategoryList)
            {
                foreach(Mod m in cat.mods)
                {
                    if (m.configs.Contains(cfg) && ListThatContainsConfig == null)
                    {
                        ListThatContainsConfig = m.configs;
                        return;
                    }
                    if(m.configs.Count > 0)
                    {
                        ListContainsConfigRecursive(m.configs, cfg);
                    }
                }
            }
        }
        private void ListContainsConfigRecursive(List<Config> cfgList, Config cfg)
        {
            foreach(Config c in cfgList)
            {
                if(c.configs.Contains(cfg) && ListThatContainsConfig == null)
                {
                    ListThatContainsConfig = c.configs;
                    return;
                }
                if(c.configs.Count > 0)
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
                DisplayDatabase();
            }
        }

        private void DatabaseTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            DatabaseTreeView.SelectedNode = e.Node;
            currentSelectedIndex = DatabaseTreeView.SelectedNode.Index;
            //DatabaseTreeView.SelectedNode.BackColor = Color.Blue;
            //DatabaseTreeView.SelectedNode.ForeColor = Color.Blue;
            DatabaseTreeNode node = (DatabaseTreeNode)DatabaseTreeView.SelectedNode;
            if(node.GlobalDependency != null)
            {
                SelectedGlobalDependency = node.GlobalDependency;
                SelectedDependency = null;
                SelectedLogicalDependency = null;
                SelectedDatabaseObject = null;
                SelectedCategory = null;

                ObjectNameTB.Enabled = false;
                ObjectNameTB.Text = "";

                ObjectPackageNameTB.Enabled = true;
                ObjectPackageNameTB.Text = node.GlobalDependency.packageName;

                ObjectStartAddressTB.Enabled = true;
                ObjectStartAddressTB.Text = node.GlobalDependency.startAddress;

                ObjectEndAddressTB.Enabled = true;
                ObjectEndAddressTB.Text = node.GlobalDependency.endAddress;

                ObjectZipFileTB.Enabled = true;
                ObjectZipFileTB.Text = node.GlobalDependency.dependencyZipFile;

                ObjectDevURLTB.Enabled = true;
                ObjectDevURLTB.Text = SelectedGlobalDependency.devURL;

                ObjectTypeComboBox.Enabled = false;
                ObjectTypeComboBox.SelectedIndex = 0;

                ObjectEnabledCheckBox.Enabled = true;
                ObjectEnabledCheckBox.Checked = node.GlobalDependency.enabled;

                ObjectVisableCheckBox.Enabled = false;
                ObjectVisableCheckBox.Checked = false;

                ObjectAppendExtractionCB.Enabled = true;
                ObjectAppendExtractionCB.Checked = node.GlobalDependency.appendExtraction;

                ObjectDescTB.Enabled = false;
                ObjectDescTB.Text = "";

                ObjectUpdateNotesTB.Enabled = false;
                ObjectUpdateNotesTB.Text = "";

                DependenciesTabPage.Enabled = false;
                PicturePanel.Enabled = false;
            }
            else if (node.Dependency != null)
            {
                SelectedGlobalDependency = null;
                SelectedDependency = node.Dependency;
                SelectedLogicalDependency = null;
                SelectedDatabaseObject = null;
                SelectedCategory = null;

                ObjectNameTB.Enabled = false;
                ObjectNameTB.Text = "";

                ObjectPackageNameTB.Enabled = true;
                ObjectPackageNameTB.Text = SelectedDependency.packageName;

                ObjectStartAddressTB.Enabled = true;
                ObjectStartAddressTB.Text = SelectedDependency.startAddress;

                ObjectEndAddressTB.Enabled = true;
                ObjectEndAddressTB.Text = SelectedDependency.endAddress;

                ObjectZipFileTB.Enabled = true;
                ObjectZipFileTB.Text = SelectedDependency.dependencyZipFile;

                ObjectDevURLTB.Enabled = true;
                ObjectDevURLTB.Text = SelectedDependency.devURL;

                ObjectTypeComboBox.Enabled = false;
                ObjectTypeComboBox.SelectedIndex = 0;

                ObjectEnabledCheckBox.Enabled = true;
                ObjectEnabledCheckBox.Checked = SelectedDependency.enabled;

                ObjectVisableCheckBox.Enabled = false;
                ObjectVisableCheckBox.Checked = false;

                ObjectAppendExtractionCB.Enabled = true;
                ObjectAppendExtractionCB.Checked = SelectedDependency.appendExtraction;

                ObjectDescTB.Enabled = false;
                ObjectDescTB.Text = "";

                ObjectUpdateNotesTB.Enabled = false;
                ObjectUpdateNotesTB.Text = "";

                DependenciesTabPage.Enabled = true;
                DependencyPanel.Enabled = false;

                LogicalDependencyPanel.Enabled = true;
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.logicalDependencies;
                CurrentLogicalDependenciesCB.DataSource = LogicalDependencies;
                CurrentLogicalDependenciesCB.SelectedIndex = -1;
                LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
                LogicalDependnecyNegateFlagCB.Checked = false;
                LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;

                PicturePanel.Enabled = false;
            }
            else if (node.LogicalDependency != null)
            {
                SelectedGlobalDependency = null;
                SelectedDependency = null;
                SelectedLogicalDependency = node.LogicalDependency;
                SelectedDatabaseObject = null;
                SelectedCategory = null;

                ObjectNameTB.Enabled = false;
                ObjectNameTB.Text = "";

                ObjectPackageNameTB.Enabled = true;
                ObjectPackageNameTB.Text = SelectedLogicalDependency.packageName;

                ObjectStartAddressTB.Enabled = true;
                ObjectStartAddressTB.Text = SelectedLogicalDependency.startAddress;

                ObjectEndAddressTB.Enabled = true;
                ObjectEndAddressTB.Text = SelectedLogicalDependency.endAddress;

                ObjectZipFileTB.Enabled = true;
                ObjectZipFileTB.Text = SelectedLogicalDependency.dependencyZipFile;

                ObjectDevURLTB.Enabled = true;
                ObjectDevURLTB.Text = SelectedLogicalDependency.devURL;

                ObjectTypeComboBox.Enabled = false;
                ObjectTypeComboBox.SelectedIndex = 0;

                ObjectEnabledCheckBox.Enabled = true;
                ObjectEnabledCheckBox.Checked = SelectedLogicalDependency.enabled;

                ObjectVisableCheckBox.Enabled = false;
                ObjectVisableCheckBox.Checked = false;

                ObjectAppendExtractionCB.Enabled = false;
                ObjectAppendExtractionCB.Checked = false;

                ObjectDescTB.Enabled = false;
                ObjectDescTB.Text = "";

                ObjectUpdateNotesTB.Enabled = false;
                ObjectUpdateNotesTB.Text = "";

                DependenciesTabPage.Enabled = false;
                PicturePanel.Enabled = false;
            }
            else if (node.DatabaseObject != null)
            {
                SelectedGlobalDependency = null;
                SelectedDependency = null;
                SelectedLogicalDependency = null;
                SelectedDatabaseObject = node.DatabaseObject;
                SelectedCategory = null;

                ObjectNameTB.Enabled = true;
                ObjectNameTB.Text = SelectedDatabaseObject.name;

                ObjectPackageNameTB.Enabled = true;
                ObjectPackageNameTB.Text = SelectedDatabaseObject.packageName;

                ObjectStartAddressTB.Enabled = true;
                ObjectStartAddressTB.Text = SelectedDatabaseObject.startAddress;

                ObjectEndAddressTB.Enabled = true;
                ObjectEndAddressTB.Text = SelectedDatabaseObject.endAddress;

                ObjectZipFileTB.Enabled = true;
                ObjectZipFileTB.Text = SelectedDatabaseObject.zipFile;

                ObjectDevURLTB.Enabled = true;
                ObjectDevURLTB.Text = SelectedDatabaseObject.devURL;

                if(SelectedDatabaseObject is Config)
                {
                    ObjectTypeComboBox.Enabled = true;
                    Config cfg = (Config)SelectedDatabaseObject;
                    switch(cfg.type)
                    {
                        case "single":
                            ObjectTypeComboBox.SelectedIndex = 1;
                            break;
                        case "single1":
                            ObjectTypeComboBox.SelectedIndex = 1;
                            break;
                        case "single_dropDown":
                            ObjectTypeComboBox.SelectedIndex = 2;
                            break;
                        case "single_dropDown1":
                            ObjectTypeComboBox.SelectedIndex = 2;
                            break;
                        case "single_dropDown2":
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
                ObjectEnabledCheckBox.Checked = SelectedDatabaseObject.enabled;

                ObjectVisableCheckBox.Enabled = true;
                ObjectVisableCheckBox.Checked = SelectedDatabaseObject.visible;

                ObjectAppendExtractionCB.Enabled = false;
                ObjectAppendExtractionCB.Checked = false;

                ObjectDescTB.Enabled = true;
                ObjectDescTB.Text = SelectedDatabaseObject.description;

                ObjectUpdateNotesTB.Enabled = true;
                ObjectUpdateNotesTB.Text = SelectedDatabaseObject.updateComment;

                DependenciesTabPage.Enabled = true;
                DependencyPanel.Enabled = true;
                LogicalDependencyPanel.Enabled = true;
                PicturePanel.Enabled = true;

                //logicalDependencies
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.logicalDependencies;
                CurrentLogicalDependenciesCB.DataSource = LogicalDependencies;
                CurrentLogicalDependenciesCB.SelectedIndex = -1;
                LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
                LogicalDependnecyNegateFlagCB.Checked = false;
                LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;

                //dependencies
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedDatabaseObject.dependencies;
                CurrentDependenciesCB.DataSource = Dependencies;
                CurrentDependenciesCB.SelectedIndex = -1;

                //pictures
                ObjectPicturesList.DataSource = null;
                ObjectPicturesList.Items.Clear();
                ObjectPicturesList.DataSource = SelectedDatabaseObject.pictureList;

            }
            else if (node.Category != null)
            {
                SelectedGlobalDependency = null;
                SelectedDependency = null;
                SelectedLogicalDependency = null;
                SelectedDatabaseObject = null;
                SelectedCategory = node.Category;

                ObjectNameTB.Enabled = true;
                ObjectNameTB.Text = SelectedCategory.name;

                ObjectPackageNameTB.Enabled = false;
                ObjectPackageNameTB.Text = "";

                ObjectStartAddressTB.Enabled = false;
                ObjectStartAddressTB.Text = "";

                ObjectEndAddressTB.Enabled = false;
                ObjectEndAddressTB.Text = "";

                ObjectZipFileTB.Enabled = false;
                ObjectZipFileTB.Text ="";

                ObjectDevURLTB.Enabled = false;
                ObjectDevURLTB.Text = "";

                ObjectTypeComboBox.Enabled = false;
                ObjectTypeComboBox.SelectedIndex = 0;

                ObjectEnabledCheckBox.Enabled = false;
                ObjectEnabledCheckBox.Checked = false;

                ObjectVisableCheckBox.Enabled = false;
                ObjectVisableCheckBox.Checked = false;

                ObjectAppendExtractionCB.Enabled = false;
                ObjectAppendExtractionCB.Checked = false;

                ObjectDescTB.Enabled = false;
                ObjectDescTB.Text = "";

                ObjectUpdateNotesTB.Enabled = false;
                ObjectUpdateNotesTB.Text = "";

                DependenciesTabPage.Enabled = true;
                DependencyPanel.Enabled = true;
                LogicalDependencyPanel.Enabled = false;

                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedCategory.dependencies;
                CurrentDependenciesCB.DataSource = Dependencies;
                CurrentDependenciesCB.SelectedIndex = -1;

                PicturePanel.Enabled = false;
            }
        }

        private void MoveButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if(SelectedCategory != null)
            {
                MessageBox.Show("Moving categories is not supported");
                return;
            }
            if (MessageBox.Show("Confirm you wish to move the object?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            using (DatabaseAdder dba = new DatabaseAdder(DatabaseEditorMode, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList))
            {
                dba.ShowDialog();
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
                        Mod modToMove = (Mod)SelectedDatabaseObject;
                        Mod Ref = (Mod)dba.SelectedDatabaseObject;
                        List<Mod> ModList = ListContainsMod(modToMove);
                        int index = ModList.IndexOf(Ref);
                        //make move
                        ModList.Remove(modToMove);
                        ModList.Insert(index, modToMove);
                    }
                    else if (SelectedDatabaseObject is Config)
                    {
                        ListThatContainsConfig = null;
                        Config cfgToMove = (Config)SelectedDatabaseObject;
                        Config Ref = (Config)dba.SelectedDatabaseObject;
                        ListContainsConfig(cfgToMove);
                        if (ListThatContainsConfig != null)
                        {
                            int index = ListThatContainsConfig.IndexOf(Ref);
                            //make move
                            ListThatContainsConfig.Remove(cfgToMove);
                            ListThatContainsConfig.Insert(index, cfgToMove);
                        }
                    }
                }
            }
            this.DisplayDatabase(false);
        }

        private void AddEntryButton_Click(object sender, EventArgs e)
        {
            if(ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (SelectedCategory != null)
            {
                MessageBox.Show("Adding categories is not supported");
                return;
            }
            using (DatabaseAdder dba = new DatabaseAdder(DatabaseEditorMode, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList))
            {
                dba.ShowDialog();
                if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
                {
                    if (dba.SelectedGlobalDependency == null)
                        return;
                    Dependency newDep = new Dependency();
                    newDep.packageName = ObjectPackageNameTB.Text;
                    newDep.startAddress = ObjectStartAddressTB.Text;
                    newDep.endAddress = ObjectEndAddressTB.Text;
                    newDep.dependencyZipFile = ObjectZipFileTB.Text;
                    newDep.enabled = ObjectEnabledCheckBox.Checked;
                    newDep.appendExtraction = ObjectAppendExtractionCB.Checked;
                    newDep.devURL = ObjectDevURLTB.Text;
                    int index = GlobalDependencies.IndexOf(dba.SelectedGlobalDependency);
                    GlobalDependencies.Insert(index, newDep);
                    DisplayDatabase();
                }
                else if (DatabaseEditorMode == EditorMode.Dependency)
                {
                    if (dba.SelectedDependency == null)
                        return;
                    Dependency newDep = new Dependency();
                    newDep.packageName = ObjectPackageNameTB.Text;
                    newDep.startAddress = ObjectStartAddressTB.Text;
                    newDep.endAddress = ObjectEndAddressTB.Text;
                    newDep.dependencyZipFile = ObjectZipFileTB.Text;
                    newDep.enabled = ObjectEnabledCheckBox.Checked;
                    newDep.appendExtraction = ObjectAppendExtractionCB.Checked;
                    newDep.devURL = ObjectDevURLTB.Text;
                    List<LogicalDependnecy> logicalDeps = (List<LogicalDependnecy>)ObjectLogicalDependenciesList.DataSource;
                    int index = Dependencies.IndexOf(dba.SelectedDependency);
                    Dependencies.Insert(index, newDep);
                    DisplayDatabase();
                }
                else if (DatabaseEditorMode == EditorMode.LogicalDependency)
                {
                    if (dba.SelectedLogicalDependency == null)
                        return;
                    LogicalDependnecy newDep = new LogicalDependnecy();
                    newDep.packageName = ObjectPackageNameTB.Text;
                    newDep.startAddress = ObjectStartAddressTB.Text;
                    newDep.endAddress = ObjectEndAddressTB.Text;
                    newDep.dependencyZipFile = ObjectZipFileTB.Text;
                    newDep.enabled = ObjectEnabledCheckBox.Checked;
                    newDep.devURL = ObjectDevURLTB.Text;
                    int index = LogicalDependencies.IndexOf(dba.SelectedLogicalDependency);
                    LogicalDependencies.Insert(index, newDep);
                    DisplayDatabase();
                }
                else if (DatabaseEditorMode == EditorMode.DBO)
                {
                    if (SelectedDatabaseObject is Mod)
                    {
                        Mod mm = (Mod)dba.SelectedDatabaseObject;
                        if(dba.sublist)
                        {
                            Config cfg = new Config();
                            cfg.name = ObjectNameTB.Text;
                            cfg.packageName = ObjectPackageNameTB.Text;
                            cfg.startAddress = ObjectStartAddressTB.Text;
                            cfg.endAddress = ObjectEndAddressTB.Text;
                            cfg.zipFile = ObjectZipFileTB.Text;
                            cfg.devURL = ObjectDevURLTB.Text;
                            switch (ObjectTypeComboBox.SelectedIndex)
                            {
                                case 1:
                                    cfg.type = "single1";
                                    break;
                                case 2:
                                    cfg.type = "single_dropdown1";
                                    break;
                                case 3:
                                    cfg.type = "single_dropdown2";
                                    break;
                                case 4:
                                    cfg.type = "multi";
                                    break;
                            }
                            cfg.enabled = ObjectEnabledCheckBox.Checked;
                            cfg.visible = ObjectVisableCheckBox.Checked;
                            cfg.description = ObjectDescTB.Text;
                            cfg.updateComment = ObjectUpdateNotesTB.Text;
                            mm.configs.Add(cfg);
                        }
                        else
                        {
                            List<Mod> ModList = ListContainsMod(mm);
                            int index = ModList.IndexOf(mm);
                            //make changes
                            Mod m = new Mod();
                            m.name = ObjectNameTB.Text;
                            m.packageName = ObjectPackageNameTB.Text;
                            m.startAddress = ObjectStartAddressTB.Text;
                            m.endAddress = ObjectEndAddressTB.Text;
                            m.zipFile = ObjectZipFileTB.Text;
                            m.devURL = ObjectDevURLTB.Text;
                            m.enabled = ObjectEnabledCheckBox.Checked;
                            m.visible = ObjectVisableCheckBox.Checked;
                            m.description = ObjectDescTB.Text;
                            m.updateComment = ObjectUpdateNotesTB.Text;
                            ModList.Insert(index, m);
                        }
                    }
                    else if (SelectedDatabaseObject is Config)
                    {
                        if (ObjectTypeComboBox.SelectedIndex == -1 || ObjectTypeComboBox.SelectedIndex == 0)
                        {
                            MessageBox.Show("Invalid Index of config type");
                            return;
                        }
                        if(dba.sublist)
                        {
                            Config cfg = new Config();
                            cfg.name = ObjectNameTB.Text;
                            cfg.packageName = ObjectPackageNameTB.Text;
                            cfg.startAddress = ObjectStartAddressTB.Text;
                            cfg.endAddress = ObjectEndAddressTB.Text;
                            cfg.zipFile = ObjectZipFileTB.Text;
                            cfg.devURL = ObjectDevURLTB.Text;
                            switch (ObjectTypeComboBox.SelectedIndex)
                            {
                                case 1:
                                    cfg.type = "single1";
                                    break;
                                case 2:
                                    cfg.type = "single_dropdown1";
                                    break;
                                case 3:
                                    cfg.type = "single_dropdown2";
                                    break;
                                case 4:
                                    cfg.type = "multi";
                                    break;
                            }
                            cfg.enabled = ObjectEnabledCheckBox.Checked;
                            cfg.visible = ObjectVisableCheckBox.Checked;
                            cfg.description = ObjectDescTB.Text;
                            cfg.updateComment = ObjectUpdateNotesTB.Text;
                            dba.SelectedDatabaseObject.configs.Add(cfg);
                            //SelectedDatabaseObject.configs.Add(cfg);
                        }
                        else
                        {
                            Config cfgg = (Config)dba.SelectedDatabaseObject;
                            ListThatContainsConfig = null;
                            ListContainsConfig(cfgg);
                            if (ListThatContainsConfig != null)
                            {
                                int index = ListThatContainsConfig.IndexOf(cfgg);
                                //make changes
                                Config cfg = new Config();
                                cfg.name = ObjectNameTB.Text;
                                cfg.packageName = ObjectPackageNameTB.Text;
                                cfg.startAddress = ObjectStartAddressTB.Text;
                                cfg.endAddress = ObjectEndAddressTB.Text;
                                cfg.zipFile = ObjectZipFileTB.Text;
                                cfg.devURL = ObjectDevURLTB.Text;
                                switch (ObjectTypeComboBox.SelectedIndex)
                                {
                                    case 1:
                                        cfg.type = "single1";
                                        break;
                                    case 2:
                                        cfg.type = "single_dropdown1";
                                        break;
                                    case 3:
                                        cfg.type = "single_dropdown2";
                                        break;
                                    case 4:
                                        cfg.type = "multi";
                                        break;
                                }
                                cfg.enabled = ObjectEnabledCheckBox.Checked;
                                cfg.visible = ObjectVisableCheckBox.Checked;
                                cfg.description = ObjectDescTB.Text;
                                cfg.updateComment = ObjectUpdateNotesTB.Text;
                                ListThatContainsConfig.Insert(index, cfg);
                            }
                        }
                    }
                }
            }
            this.DisplayDatabase(false);
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
            if (MessageBox.Show("Confirm you wish to remove the object?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                GlobalDependencies.Remove(SelectedGlobalDependency);
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                //check if the dependency is in use first
                if(DependencyInUse(SelectedDependency.packageName,true))
                {
                    MessageBox.Show("Cannot remove because it is in use:\n" + InUseSB.ToString());
                }
                else
                {
                    Dependencies.Remove(SelectedDependency);
                }
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                //check if the dependency is in use first
                if(DependencyInUse(SelectedLogicalDependency.packageName,false))
                {
                    MessageBox.Show("Cannot remove because it is in use:\n" + InUseSB.ToString());
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
        }

        private bool DependencyInUse(string packageName, bool isDependency)
        {
            InUseSB = new StringBuilder();
            bool InUse = false;
            if (!isDependency)
            {
                foreach (Dependency d in Dependencies)
                {
                    if (d.packageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append("Dependency: " + d.packageName + "\n");
                    }
                }
            }
            foreach(Category c in ParsedCategoryList)
            {
                foreach(Dependency d in c.dependencies)
                {
                    if (d.packageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append("Category: " + c.name);
                    }
                }
                foreach(Mod m in c.mods)
                {
                    foreach(Dependency d in m.dependencies)
                    {
                        if(d.packageName.Equals(packageName))
                        {
                            InUse = true;
                            InUseSB.Append("Mod: " + m.packageName + "\n");
                        }
                    }
                    foreach(LogicalDependnecy d in m.logicalDependencies)
                    {
                        if (d.packageName.Equals(packageName))
                        {
                            InUse = true;
                            InUseSB.Append("Mod: " + m.packageName + "\n");
                        }
                    }
                    ProcessConfigsInUse(InUseSB, m.configs, InUse, packageName);
                }
            }
            return InUse;
        }
        private void ProcessConfigsInUse(StringBuilder sb, List<Config> configs, bool InUse, string packageName)
        {
            foreach(Config c in configs)
            {
                foreach(Dependency d in c.dependencies)
                {
                    if (d.packageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append("Config: " + c.packageName + "\n");
                    }
                }
                foreach(LogicalDependnecy d in c.logicalDependencies)
                {
                    if (d.packageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append("Config: " + c.packageName + "\n");
                    }
                }
                ProcessConfigsInUse(sb, c.configs, InUse, packageName);
            }
        }

        private void AddDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add dependency", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (SelectedCategory != null)
            {
                Dependency ld = new Dependency();
                Dependency ld2 = (Dependency)CurrentDependenciesCB.SelectedItem;
                ld.packageName = ld2.packageName;
                SelectedCategory.dependencies.Add(ld);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedCategory.dependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                Dependency ld = new Dependency();
                Dependency ld2 = (Dependency)CurrentDependenciesCB.SelectedItem;
                ld.packageName = ld2.packageName;
                SelectedDatabaseObject.dependencies.Add(ld);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedDatabaseObject.dependencies;
            }
        }

        private void RemoveDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove dependency", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (SelectedCategory != null)
            {
                SelectedCategory.dependencies.Remove((Dependency)ObjectDependenciesList.SelectedItem);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedCategory.dependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                SelectedDatabaseObject.dependencies.Remove((Dependency)ObjectDependenciesList.SelectedItem);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedDatabaseObject.dependencies;
            } 
        }

        private void AddLogicalDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add logical dependency", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                LogicalDependnecy ld = new LogicalDependnecy();
                LogicalDependnecy ld2 = (LogicalDependnecy)CurrentLogicalDependenciesCB.SelectedItem;
                ld.packageName = ld2.packageName;
                ld.negateFlag = LogicalDependnecyNegateFlagCB.Checked;
                SelectedDependency.logicalDependencies.Add(ld);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.logicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                LogicalDependnecy ld = new LogicalDependnecy();
                LogicalDependnecy ld2 = (LogicalDependnecy)CurrentLogicalDependenciesCB.SelectedItem;
                ld.packageName = ld2.packageName;
                ld.negateFlag = LogicalDependnecyNegateFlagCB.Checked;
                SelectedDatabaseObject.logicalDependencies.Add(ld);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.logicalDependencies;
            }
        }

        private void RemoveLogicalDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove logical dependency", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                SelectedDependency.logicalDependencies.Remove((LogicalDependnecy)ObjectLogicalDependenciesList.SelectedItem);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.logicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                SelectedDatabaseObject.logicalDependencies.Remove((LogicalDependnecy)ObjectLogicalDependenciesList.SelectedItem);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.logicalDependencies;
            }
        }

        private void MovePictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to move picture in list", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            int index = Utils.parseInt(MovePictureTB.Text, -1);
            if (index == -1)
            {
                MessageBox.Show("Invalid index in move to position");
                return;
            }
            if (index > SelectedDatabaseObject.pictureList.Count)
            {
                MessageBox.Show("Invalid index in move to position");
                return;
            }
            Media media = (Media)ObjectPicturesList.SelectedItem;
            SelectedDatabaseObject.pictureList.Remove(media);
            //if the index is now out of range, just put it in the bottom
            if (index > SelectedDatabaseObject.pictureList.Count)
                index = SelectedDatabaseObject.pictureList.Count;
            SelectedDatabaseObject.pictureList.Insert(index, media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.pictureList;
        }

        private void AddPictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add picture", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (PicturesTypeCBox.SelectedIndex == -1 || PicturesTypeCBox.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid entry in Picture Type");
                return;
            }
            int index = Utils.parseInt(AddPictureTB.Text, -1);
            if(index == -1)
            {
                MessageBox.Show("Invalid index in add at positon");
                return;
            }
            if (index > SelectedDatabaseObject.pictureList.Count)
            {
                MessageBox.Show("Invalid index in add at positon");
                return;
            }
            Media media = new Media(SelectedDatabaseObject.name, PictureURLTB.Text);
            if (PicturesTypeCBox.SelectedIndex == 1)
            {
                media.mediaType = MediaType.picture;
            }
            else if (PicturesTypeCBox.SelectedIndex == 2)
            {
                media.mediaType = MediaType.youtube;
            }
            SelectedDatabaseObject.pictureList.Insert(index, media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.pictureList;
        }

        private void RemovePictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove picture", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            Media media = (Media)ObjectPicturesList.SelectedItem;
            SelectedDatabaseObject.pictureList.Remove(media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.pictureList;
        }

        private void ApplyPictureEditButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to edit picture entry", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            Media media = (Media)ObjectPicturesList.SelectedItem;
            if(PicturesTypeCBox.SelectedIndex == -1 || PicturesTypeCBox.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid entry in Picture Type");
                return;
            }
            if(PicturesTypeCBox.SelectedIndex == 1)
            {
                media.mediaType = MediaType.picture;
            }
            else if (PicturesTypeCBox.SelectedIndex == 2)
            {
                media.mediaType = MediaType.youtube;
            }
            media.URL = PictureURLTB.Text;
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.pictureList;
        }

        private void ObjectLogicalDependenciesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            LogicalDependnecy ld = (LogicalDependnecy)lb.SelectedItem;
            foreach(LogicalDependnecy d in LogicalDependencies)
            {
                if(d.packageName.Equals(ld.packageName))
                {
                    CurrentLogicalDependenciesCB.SelectedItem = d;
                    break;
                }
            }
            LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
            LogicalDependnecyNegateFlagCB.Checked = ld.negateFlag;
            LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;
        }

        private void LogicalDependnecyNegateFlagCB_CheckedChanged(object sender, EventArgs e)
        {
            if (MessageBox.Show("change logical dependency negate flag status (yes) or change flag status for adding new logical dependency (no)", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                LogicalDependnecy ld = (LogicalDependnecy)ObjectLogicalDependenciesList.SelectedItem;
                ld.negateFlag = LogicalDependnecyNegateFlagCB.Checked;
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.logicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                LogicalDependnecy ld = (LogicalDependnecy)ObjectLogicalDependenciesList.SelectedItem;
                ld.negateFlag = LogicalDependnecyNegateFlagCB.Checked;
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.logicalDependencies;
            }
        }

        private void ObjectDependenciesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            Dependency ld = (Dependency)lb.SelectedItem;
            foreach (Dependency d in Dependencies)
            {
                if (d.packageName.Equals(ld.packageName))
                {
                    CurrentDependenciesCB.SelectedItem = d;
                    break;
                }
            }
        }

        private void ObjectPicturesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            Media med = (Media)lb.SelectedItem;
            switch(med.mediaType)
            {
                case MediaType.picture:
                    PicturesTypeCBox.SelectedIndex = 1;
                    break;
                case MediaType.youtube:
                    PicturesTypeCBox.SelectedIndex = 2;
                    break;
            }
            PictureURLTB.Text = med.URL;
            MovePictureTB.Text = "" + SelectedDatabaseObject.pictureList.IndexOf(med);
            AddPictureTB.Text = "" + SelectedDatabaseObject.pictureList.IndexOf(med);
        }
    }
}
