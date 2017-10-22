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
        private List<LogicalDependnecy> logicalDependencies;
        private string header;

        public DatabaseListGenerater()
        {
            InitializeComponent();
        }

        private void DatabaseListGenerater_Load(object sender, EventArgs e)
        {
            //font scaling
            this.AutoScaleMode = Settings.appScalingMode;
            this.Font = Settings.appFont;
            if (Settings.appScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.scaleSize, Settings.scaleSize));
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
            logicalDependencies = new List<LogicalDependnecy>();
            parsedCatagoryList = new List<Category>();
            XMLUtils.createModStructure(LoadDatabaseFileDialog.FileName, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            if(!SpreadsheetLocation.Text.Equals(""))
                SpreadsheetLocation.Text = "(old) " + SpreadsheetLocation.Text;
        }

        private void GenretateInternalSpreadsheetButton_Click(object sender, EventArgs e)
        {
            //reset everything
            header = "packageName\tCategory\tMod\tConfig\tLevel\tZip\tDevURL\tEnabled";
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
                packageName = d.packageName;
                zipfile = d.dependencyZipFile;
                enabled = d.enabled;
                sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            //next save depenedneices
            category = "dependencies";
            foreach (Dependency d in dependencies)
            {
                packageName = d.packageName;
                zipfile = d.dependencyZipFile;
                enabled = d.enabled;
                sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            //next save logicaldepenedneices
            category = "logicalDependencies";
            foreach (LogicalDependnecy d in logicalDependencies)
            {
                packageName = d.packageName;
                zipfile = d.dependencyZipFile;
                enabled = d.enabled;
                sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            foreach (Category cat in parsedCatagoryList)
            {
                category = cat.name;
                foreach (Mod m in cat.mods)
                {
                    //remove the old devURL value if there
                    devURL = "";
                    packageName = m.packageName;
                    level = 1;
                    modName = m.name;
                    configname = "N/A";
                    zipfile = m.zipFile;
                    enabled = m.enabled;
                    if (m.devURL.Equals(""))
                        devURL = "";
                    else
                        devURL = "=HYPERLINK(\"" + m.devURL + "\",\"link\")";
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
                packageName = con.packageName;
                configname = con.name;
                zipfile = con.zipFile;
                enabled = con.enabled;
                if (con.devURL.Equals(""))
                    devURL = "";
                else
                    devURL = "=HYPERLINK(\"" + con.devURL + "\",\"link\")";
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
                category = cat.name;
                foreach (Mod m in cat.mods)
                {
                    //remove the old devURL value if there
                    devURL = "";
                    modName = m.name;
                    if (m.devURL.Equals(""))
                        devURL = "";
                    else
                        devURL = "=HYPERLINK(\"" + m.devURL + "\",\"link\")";
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
                configname = configname + con.name;
                if (con.devURL.Equals(""))
                    devURL = "";
                else
                    devURL = "=HYPERLINK(\"" + con.devURL + "\",\"link\")";
                sb.Append(category + "\t" + configname + "\t" + devURL + "\n");
                if (con.configs.Count > 0)
                    processConfigsSpreadsheetGenerateUser(con.configs, newLevel + 1);
            }
        }

        private void DatabaseListGenerater_FormClosing(object sender, FormClosingEventArgs e)
        {
            Utils.appendToLog("|------------------------------------------------------------------------------------------------|");
        }
    }
}
