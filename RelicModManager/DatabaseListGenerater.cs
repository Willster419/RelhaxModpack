using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RelhaxModpack
{
    public partial class DatabaseListGenerater : Form
    {
        //required class variables
        private StringBuilder sb = new StringBuilder();
        private string packageName = "";
        private string category = "";
        private int level = 0;
        private string modName = "";
        private string configname = "N/A";
        private string zipfile = "";
        private bool enabled = false;
        private string devURL = "";
        private List<Dependency> globalDependencies;
        private List<Category> parsedCatagoryList;
        private List<Dependency> dependencies;
        private List<LogicalDependency> logicalDependencies;
        private string header;

        public DatabaseListGenerater()
        {
            InitializeComponent();
        }

        private void DatabaseListGenerater_Load(object sender, EventArgs e)
        {
            //font scaling
            this.AutoScaleMode = Settings.AppScalingMode;
            this.Font = Settings.AppFont;
            if (Settings.AppScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            SaveSpreadsheetFileDialog.InitialDirectory = Application.StartupPath;
            LoadDatabaseFileDialog.InitialDirectory = Application.StartupPath;
        }

        private void LoadDatabaseButton_Click(object sender, EventArgs e)
        {
            if (LoadDatabaseFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            DatabaseLocation.Text = LoadDatabaseFileDialog.FileName;
            //except actually load it here
            //load database
            globalDependencies = new List<Dependency>();
            dependencies = new List<Dependency>();
            logicalDependencies = new List<LogicalDependency>();
            parsedCatagoryList = new List<Category>();
            XMLUtils.CreateModStructure(LoadDatabaseFileDialog.FileName, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            if(!SpreadsheetLocation.Text.Equals(""))
                SpreadsheetLocation.Text = "(old) " + SpreadsheetLocation.Text;
        }

        private void GenretateInternalSpreadsheetButton_Click(object sender, EventArgs e)
        {
            //reset everything
            header = "PackageName\tCategory\tMod\tConfig\tLevel\tZip\tDevURL\tEnabled";
            sb = new StringBuilder();
            packageName = "";
            category = "";
            level = 0;
            modName = "";
            configname = "N/A";
            zipfile = "";
            devURL = "";
            enabled = false;
            //ask where to save the file
            if (SaveSpreadsheetFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            //save it
            sb.Append(header + "\n");
            //first save globaldependencies
            category = "globalDependencies";
            foreach(Dependency d in globalDependencies)
            {
                packageName = d.PackageName;
                zipfile = d.ZipFile;
                enabled = d.Enabled;
                sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            //next save depenedneices
            category = "dependencies";
            foreach (Dependency d in dependencies)
            {
                packageName = d.PackageName;
                zipfile = d.ZipFile;
                enabled = d.Enabled;
                sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            //next save logicaldepenedneices
            category = "logicalDependencies";
            foreach (LogicalDependency d in logicalDependencies)
            {
                packageName = d.PackageName;
                zipfile = d.ZipFile;
                enabled = d.Enabled;
                sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            foreach (Category cat in parsedCatagoryList)
            {
                category = cat.Name;
                foreach (Mod m in cat.Mods)
                {
                    //remove the old devURL value if there
                    devURL = "";
                    packageName = m.PackageName;
                    level = 1;
                    modName = m.Name;
                    configname = "N/A";
                    zipfile = m.ZipFile;
                    enabled = m.Enabled;
                    if (m.DevURL.Equals(""))
                        devURL = "";
                    else
                        devURL = "=HYPERLINK(\"" + m.DevURL + "\",\"link\")";
                    //header = "Index,Category,Mod,Config,Level,Zip,Enabled";
                    sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
                    if (m.configs.Count > 0)
                        processConfigsSpreadsheetGenerate(m.configs, level + 1);
                }
            }
            try
            {
                File.WriteAllText(SaveSpreadsheetFileDialog.FileName, sb.ToString());
                SpreadsheetLocation.Text = "Saved in " + SaveSpreadsheetFileDialog.FileName;
            }
            catch (IOException)
            {
                SpreadsheetLocation.Text = "Failed to save in " + SaveSpreadsheetFileDialog.FileName + " (IOException, probably file open in another window)";
            }
        }
        private void processConfigsSpreadsheetGenerate(List<Config> configList, int newLevel)
        {
            foreach (Config con in configList)
            {
                //remove the old devURL value if there
                devURL = "";
                packageName = con.PackageName;
                configname = con.Name;
                zipfile = con.ZipFile;
                enabled = con.Enabled;
                if (con.DevURL.Equals(""))
                    devURL = "";
                else
                    devURL = "=HYPERLINK(\"" + con.DevURL + "\",\"link\")";
                sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + newLevel + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
                if (con.configs.Count > 0)
                    processConfigsSpreadsheetGenerate(con.configs, newLevel + 1);
            }
        }

        private void GenerateSpreadsheetUserButton_Click(object sender, EventArgs e)
        {
            header = "Category\tMod\tDevURL";
            //reset everything
            sb = new StringBuilder();
            packageName = "";
            category = "";
            level = 0;
            modName = "";
            configname = "";
            zipfile = "";
            devURL = "";
            enabled = false;
            //ask where to save the file
            if (SaveSpreadsheetFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            //save it
            sb.Append(header + "\n");
            foreach (Category cat in parsedCatagoryList)
            {
                category = cat.Name;
                foreach (Mod m in cat.Mods)
                {
                    //remove the old devURL value if there
                    devURL = "";
                    modName = m.Name;
                    if (m.DevURL.Equals(""))
                        devURL = "";
                    else
                        devURL = "=HYPERLINK(\"" + m.DevURL + "\",\"link\")";
                    sb.Append(category + "\t" + modName + "\t" + devURL + "\n");
                    if (m.configs.Count > 0)
                        processConfigsSpreadsheetGenerateUser(m.configs, level + 1);
                }
            }
            try
            {
                File.WriteAllText(SaveSpreadsheetFileDialog.FileName, sb.ToString());
                SpreadsheetLocation.Text = "Saved in " + SaveSpreadsheetFileDialog.FileName;
            }
            catch (IOException)
            {
                SpreadsheetLocation.Text = "Failed to save in " + SaveSpreadsheetFileDialog.FileName + " (IOException, probably file open in another window)";
            }
        }

        private void processConfigsSpreadsheetGenerateUser(List<Config> configList, int newLevel)
        {
            foreach (Config con in configList)
            {
                //remove the old devURL value if there
                devURL = "";
                configname = "";
                for (int i = 0; i < newLevel; i++)
                {
                    configname = configname + "--";
                }
                configname = configname + con.Name;
                if (con.DevURL.Equals(""))
                    devURL = "";
                else
                    devURL = "=HYPERLINK(\"" + con.DevURL + "\",\"link\")";
                sb.Append(category + "\t" + configname + "\t" + devURL + "\n");
                if (con.configs.Count > 0)
                    processConfigsSpreadsheetGenerateUser(con.configs, newLevel + 1);
            }
        }

        private void DatabaseListGenerater_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
        }
    }
}
