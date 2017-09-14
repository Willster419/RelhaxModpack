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
        private int currentSelectedIndex = -1;
        string GameVersion = "";

        //an enum to control the form editor mode
        private enum EditorMode
        {
            GlobalDependnecy = 0,
            Dependency = 1,
            LogicalDependency = 2,
            DBO = 3
        };
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

                }
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
            if (SaveDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            DatabaseLocation = SaveDatabaseDialog.FileName;
            if (!File.Exists(DatabaseLocation))
                return;
            Utils.SaveDatabase(DatabaseLocation, GameVersion, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList);
        }
        //Apply all changes from the form
        private void ApplyChangesButton_Click(object sender, EventArgs e)
        {

        }
        //mode set to globalDependency
        private void GlobalDependencyRB_CheckedChanged(object sender, EventArgs e)
        {
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
                ObjectNameTB.Enabled = false;
                ObjectNameTB.Text = "";
                ObjectPackageNameTB.Text = node.GlobalDependency.packageName;
                ObjectStartAddressTB.Text = node.GlobalDependency.startAddress;
                ObjectEndAddressTB.Text = node.GlobalDependency.endAddress;
                ObjectZipFileTB.Text = node.GlobalDependency.dependencyZipFile;
                ObjectDevURLTB.Enabled = false;
                ObjectDevURLTB.Text = "";
                ObjectTypeComboBox.SelectedIndex = 0;
                ObjectTypeComboBox.Enabled = false;
                ObjectEnabledCheckBox.Checked = node.GlobalDependency.enabled;
                ObjectVisableCheckBox.Enabled = false;
                ObjectDescTB.Text = "";
                ObjectDescTB.Enabled = false;
                ObjectUpdateNotesTB.Text = "";
                ObjectUpdateNotesTB.Enabled = false;
                ObjectDependenciesList.Enabled = false;
                ObjectDependenciesList.BackColor = SystemColors.Control;
                AppendExtractionCB.Enabled = true;
                AppendExtractionCB.Checked = node.GlobalDependency.appendExtraction;
                ObjectLogicalDependenciesList.Enabled = true;
                ObjectPicturesList.Enabled = false;
                ObjectPicturesList.BackColor = SystemColors.Control;
            }
            else if (node.Dependency != null)
            {

            }
            else if (node.LogicalDependency != null)
            {

            }
            else if (node.DatabaseObject != null)
            {

            }
        }
    }
}
