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
        private List<Dependency> globalDependencies = new List<Dependency>();
        private List<Category> parsedCatagoryList = new List<Category>();
        private string header = "Index\tCategory\tMod\tConfig\tLevel\tZip\tDevURL\tEnabled";

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
            parsedCatagoryList = new List<Category>();
            Utils.createModStructure(LoadDatabaseFileDialog.FileName, true, globalDependencies, parsedCatagoryList);
            if(!SpreadsheetLocation.Text.Equals(""))
                SpreadsheetLocation.Text = "(old) " + SpreadsheetLocation.Text;
        }

        private void GenretateSpreadsheetButton_Click(object sender, EventArgs e)
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
            //level++;
            foreach (Config con in configList)
            {
                //remove the old devURL value if there
                devURL = "";
                packageName = con.packageName;
                configname = con.name;
                zipfile = con.zipFile;
                enabled = con.enabled;
                devURL = "=HYPERLINK(\"" + con.devURL + "\",\"link\")";
                //header = "Index,Category,Mod,Config,Level,Zip,Enabled";
                sb.Append(packageName + "\t" + category + "\t" + modName + "\t" + configname + "\t" + newLevel + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
                if (con.configs.Count > 0)
                    processConfigsSpreadsheetGenerate(con.configs, newLevel + 1);
                //else
                    //level--;
            }
        }

        private void generateSpreadsheetUserButton_Click(object sender, EventArgs e)
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
                    //level = 1;
                    modName = m.name;
                    devURL = "=HYPERLINK(\"" + m.devURL + "\",\"link\")";
                    //header = "Category\tMod\tDevURL"
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
                devURL = "=HYPERLINK(\"" + con.devURL + "\",\"link\")";
                //header = "Category\tMod\tConfig\tDevURL";
                sb.Append(category + "\t" + configname + "\t" + devURL + "\n");
                if (con.configs.Count > 0)
                    processConfigsSpreadsheetGenerateUser(con.configs, newLevel + 1);
            }
        }
    }
}
