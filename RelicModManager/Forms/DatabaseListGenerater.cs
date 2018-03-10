using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class DatabaseListGenerater : RelhaxForum
    {
        //required class variables
        private StringBuilder sb = new StringBuilder();
        private string packageDisplayName = "";
        private string category = "";
        private string packageName = "N/A";
        private string notApplicatable = "n/a";
        private string zipfile = "";
        private bool enabled = false;
        private bool visible = false;
        private string devURL = "";
        private List<Dependency> globalDependencies;
        private List<Category> parsedCatagoryList;
        private List<Dependency> dependencies;
        private List<LogicalDependency> logicalDependencies;
        private string header;

        #region Boring Stuff
        public DatabaseListGenerater()
        {
            InitializeComponent();
        }

        private void DatabaseListGenerater_Load(object sender, EventArgs e)
        {
            
            SaveSpreadsheetFileDialog.InitialDirectory = Application.StartupPath;
            LoadDatabaseFileDialog.InitialDirectory = Application.StartupPath;
        }

        private void DatabaseListGenerater_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
        }
        #endregion

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
            SaveSpreadsheetFileDialog.Filter = "database.csv|database.csv";
            //reset everything
            header = "PackageName\tCategory\tPackage\tLevel\tZip\tDevURL\tEnabled\tVisible";
            sb = new StringBuilder();
            packageDisplayName = "";
            category = "";
            packageName = "N/A";
            zipfile = "";
            devURL = "";
            enabled = false;
            visible = false;
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
                zipfile = string.IsNullOrWhiteSpace(d.ZipFile) ? notApplicatable : d.ZipFile;
                devURL = string.IsNullOrWhiteSpace(d.DevURL) ? "" : "=HYPERLINK(\"" + d.DevURL + "\",\"link\")";
                enabled = d.Enabled;
                sb.Append(packageName + "\t" + category + "\t" + packageDisplayName + "\t" + 0 + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            //next save depenedneices
            category = "dependencies";
            foreach (Dependency d in dependencies)
            {
                packageName = d.PackageName;
                zipfile = string.IsNullOrWhiteSpace(d.ZipFile) ? notApplicatable : d.ZipFile;
                devURL = string.IsNullOrWhiteSpace(d.DevURL) ? "" : "=HYPERLINK(\"" + d.DevURL + "\",\"link\")";
                enabled = d.Enabled;
                sb.Append(packageName + "\t" + category + "\t" + packageDisplayName + "\t" + 0 + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            //next save logicaldepenedneices
            category = "logicalDependencies";
            foreach (LogicalDependency d in logicalDependencies)
            {
                packageName = d.PackageName;
                zipfile = string.IsNullOrWhiteSpace(d.ZipFile) ? notApplicatable : d.ZipFile;
                devURL = string.IsNullOrWhiteSpace(d.DevURL) ? "" : "=HYPERLINK(\"" + d.DevURL + "\",\"link\")";
                enabled = d.Enabled;
                sb.Append(packageName + "\t" + category + "\t" + packageDisplayName + "\t" + 0 + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\n");
            }
            foreach (Category cat in parsedCatagoryList)
            {
                category = cat.Name;
                foreach (SelectablePackage m in cat.Packages)
                {
                    packageName = m.PackageName;
                    packageDisplayName = m.Name;
                    zipfile = string.IsNullOrWhiteSpace(m.ZipFile) ? notApplicatable : m.ZipFile;
                    enabled = m.Enabled;
                    visible = m.Visible;
                    devURL = string.IsNullOrWhiteSpace(m.DevURL) ? "" : "=HYPERLINK(\"" + m.DevURL + "\",\"link\")";
                    //header = "Index,Category,Mod,Config,Level,Zip,Enabled";
                    sb.Append(packageName + "\t" + category + "\t" + packageDisplayName + "\t" + m.Level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\t" + visible + "\n");
                    if (m.Packages.Count > 0)
                        processConfigsSpreadsheetGenerate(m.Packages);
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

        private void processConfigsSpreadsheetGenerate(List<SelectablePackage> configList)
        {
            foreach (SelectablePackage con in configList)
            {
                packageName = con.PackageName;
                packageDisplayName = con.Name;
                zipfile = string.IsNullOrWhiteSpace(con.ZipFile) ? notApplicatable : con.ZipFile;
                enabled = con.Enabled;
                visible = con.Visible;
                devURL = string.IsNullOrWhiteSpace(con.DevURL) ? "" : "=HYPERLINK(\"" + con.DevURL + "\",\"link\")";
                sb.Append(packageName + "\t" + category + "\t" + packageDisplayName + "\t" + con.Level + "\t" + zipfile + "\t" + devURL + "\t" + enabled + "\t" + visible + "\n");
                if (con.Packages.Count > 0)
                    processConfigsSpreadsheetGenerate(con.Packages);
            }
        }

        private void GenerateSpreadsheetUserButton_Click(object sender, EventArgs e)
        {
            SaveSpreadsheetFileDialog.Filter = "database_user.csv|database_user.csv";
            header = "Category\tMod\tDevURL";
            //reset everything
            sb = new StringBuilder();
            packageName = "";
            category = "";
            packageDisplayName = "";
            packageName = "";
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
                foreach (SelectablePackage m in cat.Packages)
                {
                    //remove the old devURL value if there
                    devURL = "";
                    packageDisplayName = m.Name;
                    devURL = string.IsNullOrWhiteSpace(m.DevURL) ? "" : "=HYPERLINK(\"" + m.DevURL + "\",\"link\")";
                    sb.Append(category + "\t" + packageDisplayName + "\t" + devURL + "\n");
                    if (m.Packages.Count > 0)
                        processConfigsSpreadsheetGenerateUser(m.Packages);
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

        private void processConfigsSpreadsheetGenerateUser(List<SelectablePackage> configList)
        {
            foreach (SelectablePackage con in configList)
            {
                //remove the old devURL value if there
                devURL = "";
                packageName = "";
                for (int i = 0; i <= con.Level; i++)
                {
                    packageName = packageName + "--";
                }
                packageName = packageName + con.Name;
                devURL = string.IsNullOrWhiteSpace(con.DevURL) ? "" : "=HYPERLINK(\"" + con.DevURL + "\",\"link\")";
                sb.Append(category + "\t" + packageName + "\t" + devURL + "\n");
                if (con.Packages.Count > 0)
                    processConfigsSpreadsheetGenerateUser(con.Packages);
            }
        }
    }
}
