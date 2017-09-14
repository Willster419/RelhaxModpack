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
        DBO = 3
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

        private EditorMode DatabaseEditorMode;

        public DatabaseEditor()
        {
            InitializeComponent();
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
        private void DisplayDatabase()
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
                        DisplayDatabaseConfigs(modNode, m.configs);
                    }
                }
            }
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

            DatabaseSubeditPanel.Enabled = false;
            PicturePanel.Enabled = false;
        }
        private void DisplayDatabaseConfigs(DatabaseTreeNode parrent, List<Config> configs)
        {
            foreach(Config c in configs)
            {
                DatabaseTreeNode ConfigParrent = new DatabaseTreeNode(c, (int)DatabaseEditorMode);
                parrent.Nodes.Add(ConfigParrent);
                DisplayDatabaseConfigs(ConfigParrent, c.configs);
            }
        }
        //show the load database dialog and load the database
        private void LoadDatabaseButton_Click(object sender, EventArgs e)
        {
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
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                int index = GlobalDependencies.IndexOf(SelectedGlobalDependency);
                SelectedGlobalDependency.packageName = ObjectPackageNameTB.Text;
                SelectedGlobalDependency.startAddress = ObjectStartAddressTB.Text;
                SelectedGlobalDependency.endAddress = ObjectEndAddressTB.Text;
                SelectedGlobalDependency.dependencyZipFile = ObjectZipFileTB.Text;
                SelectedGlobalDependency.enabled = ObjectEnabledCheckBox.Checked;
                SelectedGlobalDependency.appendExtraction = ObjectAppendExtractionCB.Checked;
                GlobalDependencies[index] = SelectedGlobalDependency;
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
               
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

                ObjectDevURLTB.Enabled = false;
                ObjectDevURLTB.Text = "";

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

                DatabaseSubeditPanel.Enabled = false;
                PicturePanel.Enabled = false;
            }
            else if (node.Dependency != null)
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

                ObjectDevURLTB.Enabled = false;
                ObjectDevURLTB.Text = "";

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

                DatabaseSubeditPanel.Enabled = false;
                PicturePanel.Enabled = false;
            }
            else if (node.LogicalDependency != null)
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

                ObjectDevURLTB.Enabled = false;
                ObjectDevURLTB.Text = "";

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

                DatabaseSubeditPanel.Enabled = false;
                PicturePanel.Enabled = false;
            }
            else if (node.DatabaseObject != null)
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

                ObjectDevURLTB.Enabled = false;
                ObjectDevURLTB.Text = "";

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

                DatabaseSubeditPanel.Enabled = false;
                PicturePanel.Enabled = false;
            }
            else if (node.Category != null)
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

                ObjectDevURLTB.Enabled = false;
                ObjectDevURLTB.Text = "";

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

                DatabaseSubeditPanel.Enabled = false;
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
        }

        private void AddEntryButton_Click(object sender, EventArgs e)
        {
            if(ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
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
                    int index = GlobalDependencies.IndexOf(dba.SelectedGlobalDependency);
                    GlobalDependencies.Insert(index, newDep);
                    DisplayDatabase();
                }
                else if (DatabaseEditorMode == EditorMode.Dependency)
                {

                }
                else if (DatabaseEditorMode == EditorMode.LogicalDependency)
                {

                }
                else if (DatabaseEditorMode == EditorMode.DBO)
                {

                }
                
            }
        }

        private void RemoveEntryButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
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
                Dependencies.Remove(SelectedDependency);
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                LogicalDependencies.Remove(SelectedLogicalDependency);
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                foreach(Category cat in ParsedCategoryList)
                {
                    foreach(Mod m in cat.mods)
                    {

                    }
                }
            }
            DisplayDatabase();
        }

        private void AddDependencyButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
        }

        private void RemoveDependencyButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
        }

        private void AddLogicalDependencyButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
        }

        private void RemoveLogicalDependencyButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
        }

        private void MovePictureButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
        }

        private void AddPictureButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
        }

        private void RemovePictureButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
        }

        private void ApplyPictureEditButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
        }
    }
}
