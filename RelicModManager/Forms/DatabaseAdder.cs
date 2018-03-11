using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class DatabaseAdder : Form
    {
        private EditorMode Mode;
        public Dependency SelectedGlobalDependency;
        public Dependency SelectedDependency;
        public LogicalDependency SelectedLogicalDependency;
        public SelectablePackage SelectedDatabaseObject;
        private List<SelectablePackage> PackageList;
        private bool ignoreResult = true;
        public bool sublist = false;

        public DatabaseAdder(EditorMode mode, List<Dependency> GlobalDependency, List<Dependency> Dependencies, List<LogicalDependency> LogicalDepdnedncies, List<Category> ParsedCatList, bool moveMode)
        {
            InitializeComponent();
            Mode = mode;
            sublist = false;
            ModPanel.Enabled = false;
            if (mode == EditorMode.GlobalDependnecy)
            {
                CategoryCB.DataSource = GlobalDependency;
            }
            else if(mode == EditorMode.Dependency)
            {
                CategoryCB.DataSource = Dependencies;
            }
            else if (mode == EditorMode.LogicalDependency)
            {
                CategoryCB.DataSource = LogicalDepdnedncies;
            }
            else if (mode == EditorMode.DBO)
            {
                SelectModePanel.Visible = true;
                CategoryCB.DataSource = ParsedCatList;
                if(moveMode)
                {
                    SelectModePanel.Visible = false;
                    AddUnderLabel.Text = "Move to the same level as...";
                }
            }
        }

        private void CategoryCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if(Mode == EditorMode.DBO)
            {
                Category selectedCat = (Category)cb.SelectedItem;
                ModPanel.Enabled = true;
                PackageList = new List<SelectablePackage>();
                processConfigs(selectedCat.Packages);
                PackageCB.SelectedIndexChanged -= PackageCB_SelectedIndexChanged;
                PackageCB.DataSource = PackageList;
                PackageCB.SelectedIndex = -1;
                PackageCB.SelectedIndexChanged += PackageCB_SelectedIndexChanged;
            }
            else if (Mode == EditorMode.GlobalDependnecy)
            {
                SelectedGlobalDependency = (Dependency)cb.SelectedItem;
            }
            else if (Mode == EditorMode.Dependency)
            {
                SelectedDependency = (Dependency)cb.SelectedItem;
            }
            else if (Mode == EditorMode.LogicalDependency)
            {
                SelectedLogicalDependency = (LogicalDependency)cb.SelectedItem;
            }
        }

        private void PackageCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            SelectablePackage selectedMod = (SelectablePackage)cb.SelectedItem;
            SelectedDatabaseObject = selectedMod;
        }

        private void processConfigs(List<SelectablePackage> cfgList)
        {
            foreach(SelectablePackage c in cfgList)
            {
                PackageList.Add(c);
                processConfigs(c.Packages);
            }
        }

        #region boring stuff
        private void ConfirmAddYes_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton send = (RadioButton)sender;
            if (send.Checked)
            {
                AddUnderPanel.Enabled = true;
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            ignoreResult = false;
            DialogResult = DialogResult.OK;
        }

        private void DatabaseAdder_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(ignoreResult)
            {
                SelectedDatabaseObject = null;
                SelectedDependency = null;
                SelectedGlobalDependency = null;
                SelectedLogicalDependency = null;
            }
        }

        private void SameLevelRB_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if(rb.Checked)
            {
                //adding a mod, don't show config stuff
                sublist = false;
            }
        }

        private void NewLevelRB_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.Checked)
            {
                //adding a config
                sublist = true;
            }
        }

        private void ConfirmAddNo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton send = (RadioButton)sender;
            if (send.Checked)
            {
                AddUnderPanel.Enabled = false;
            }
        }
        #endregion


    }
}
