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
        private List<SelectablePackage> configList;
        private bool ignoreResult = true;
        public bool sublist = false;
        public DatabaseAdder(EditorMode mode, List<Dependency> GlobalDependency, List<Dependency> Dependencies, List<LogicalDependency> LogicalDepdnedncies, List<Category> ParsedCatList, bool moveMode)
        {
            InitializeComponent();
            Mode = mode;
            sublist = false;
            if(mode == EditorMode.GlobalDependnecy)
            {
                AddUnderCB.DataSource = GlobalDependency;
                ModPanel.Enabled = false;
                ConfigPanel.Enabled = false;
            }
            else if(mode == EditorMode.Dependency)
            {
                AddUnderCB.DataSource = Dependencies;
                ModPanel.Enabled = false;
                ConfigPanel.Enabled = false;
            }
            else if (mode == EditorMode.LogicalDependency)
            {
                AddUnderCB.DataSource = LogicalDepdnedncies;
                ModPanel.Enabled = false;
                ConfigPanel.Enabled = false;
            }
            else if (mode == EditorMode.DBO)
            {
                SelectModePanel.Visible = true;
                AddUnderCB.DataSource = ParsedCatList;
                ModPanel.Enabled = false;
                ConfigPanel.Enabled = false;
                if(moveMode)
                {
                    SelectModePanel.Visible = false;
                    AddUnderLabel.Text = "Move to the same level as...";
                }
            }
        }

        private void ConfirmAddYes_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton send = (RadioButton)sender;
            if(send.Checked)
            {
                AddUnderPanel.Enabled = true;
            }
        }

        private void AddUnderCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if(Mode == EditorMode.DBO)
            {
                Category selectedCat = (Category)cb.SelectedItem;
                ModPanel.Enabled = true;
                ModCB.DataSource = selectedCat.Packages;
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

        private void ModCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            SelectablePackage selectedMod = (SelectablePackage)cb.SelectedItem;
            SelectedDatabaseObject = selectedMod;
            if (selectedMod.Packages.Count == 0)
            {
                ConfigPanel.Enabled = false;
            }
            else
            {
                ConfigPanel.Enabled = true;
                configList = new List<SelectablePackage>();
                foreach (SelectablePackage c in selectedMod.Packages)
                {
                    configList.Add(c);
                    processConfigs(c.Packages);
                }
                ConfigCB.SelectedIndexChanged -= ConfigCB_SelectedIndexChanged;
                ConfigCB.DataSource = configList;
                ConfigCB.SelectedIndex = -1;
                ConfigCB.SelectedIndexChanged += ConfigCB_SelectedIndexChanged;
            }
        }

        private void processConfigs(List<SelectablePackage> cfgList)
        {
            foreach(SelectablePackage c in cfgList)
            {
                configList.Add(c);
                processConfigs(c.Packages);
            }
        }

        private void ConfigCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            SelectedDatabaseObject = (SelectablePackage)cb.SelectedItem;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            ignoreResult = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
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
    }
}
