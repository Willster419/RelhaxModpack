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
        public LogicalDependnecy SelectedLogicalDependency;
        public DatabaseObject SelectedDatabaseObject;
        private List<Config> configList;
        private bool ignoreResult = true;
        public DatabaseAdder(EditorMode mode, List<Dependency> GlobalDependency, List<Dependency> Dependencies, List<LogicalDependnecy> LogicalDepdnedncies, List<Category> ParsedCatList)
        {
            InitializeComponent();
            Mode = mode;
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
                AddUnderCB.DataSource = ParsedCatList;
                ModPanel.Enabled = false;
                ConfigPanel.Enabled = false;
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
                ModCB.DataSource = selectedCat.mods;
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
                SelectedLogicalDependency = (LogicalDependnecy)cb.SelectedItem;
            }
        }

        private void ModCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            Mod selectedMod = (Mod)cb.SelectedItem;
            if(selectedMod.configs.Count == 0)
            {
                SelectedDatabaseObject = selectedMod;
            }
            if(cb.SelectedIndex != -1)
                ConfigPanel.Enabled = true;
            configList = new List<Config>();
            foreach(Config c in selectedMod.configs)
            {
                configList.Add(c);
                processConfigs(c.configs);
            }
            ConfigCB.DataSource = configList;
        }

        private void processConfigs(List<Config> cfgList)
        {
            foreach(Config c in cfgList)
            {
                configList.Add(c);
                processConfigs(c.configs);
            }
        }

        private void ConfigCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            SelectedDatabaseObject = (Config)cb.SelectedItem;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            ignoreResult = false;
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
    }
}
